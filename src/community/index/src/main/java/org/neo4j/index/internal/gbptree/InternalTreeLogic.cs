using System.Collections.Generic;
using System.Diagnostics;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Index.Internal.gbptree
{

	using Overflow = Neo4Net.Index.Internal.gbptree.TreeNode.Overflow;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.KeySearch.isHit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.KeySearch.positionOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.PointerChecking.assertNoSuccessor;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.StructurePropagation.KeyReplaceStrategy.BUBBLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.StructurePropagation.KeyReplaceStrategy.REPLACE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.StructurePropagation.UPDATE_LEFT_CHILD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.StructurePropagation.UPDATE_MID_CHILD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.StructurePropagation.UPDATE_RIGHT_CHILD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.TreeNode.Overflow.NO_NEED_DEFRAG;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.TreeNode.Overflow.YES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.TreeNode.Type.INTERNAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.TreeNode.Type.LEAF;

	/// <summary>
	/// Implementation of GB+ tree insert/remove algorithms.
	/// <para>
	/// Changes involved in splitting a leaf (L = leaf page to split, R` = L's current right sibling):
	/// <ol>
	/// <li>Acquire new page id R</li>
	/// <li>Copy "right-hand" keys/values to R and set key count</li>
	/// <li>Set L's right sibling to R</li>
	/// <li>Set key count of L to new "left-hand" key count</li>
	/// <li>Write new key/values in L</li>
	/// </ol>
	/// </para>
	/// <para>
	/// Reader concurrent with writer may have to compensate its reading to cope with following scenario
	/// (key/value abstracted into E for simplicity, right bracket ends by keyCount):
	/// SCENARIO1 (new key ends up in right leaf)
	/// <pre>
	/// - L[E1,E2,E4,E5]
	///           ^
	///   Reader have read E1-E2 and is about to read E4
	/// 
	/// - Split happens where E3 is inserted and the leaf needs to be split, which modifies the tree into:
	///   L[E1,E2] -> R[E3,E4,E5]
	/// 
	///   During this split, reader could see this state:
	///   L[E1,E2,E4,E5] -> R[E3,E4,E5]
	///           ^  ^           x  x
	///   Reader will need to ignore lower keys already seen, assuming unique keys
	/// </pre>
	/// SCENARIO2 (new key ends up in left leaf)
	/// <pre>
	/// - L[E1,E2,E4,E5,E6]
	///           ^
	///   Reader have read E1-E2 and is about to read E4
	/// 
	/// - Split happens where E3 is inserted and the leaf needs to be split, which modifies the tree into:
	///   L[E1,E2,E3] -> R[E4,E5,E6]
	/// 
	///   There's no bad intermediate state
	/// </pre>
	/// 
	/// </para>
	/// </summary>
	/// @param <KEY> type of internal/leaf keys </param>
	/// @param <VALUE> type of leaf values </param>
	internal class InternalTreeLogic<KEY, VALUE>
	{
		 internal const double DEFAULT_SPLIT_RATIO = 0.5;

		 private readonly IdProvider _idProvider;
		 private readonly TreeNode<KEY, VALUE> _bTreeNode;
		 private readonly Layout<KEY, VALUE> _layout;
		 private readonly KEY _newKeyPlaceHolder;
		 private readonly KEY _readKey;
		 private readonly VALUE _readValue;
		 private readonly GBPTree.Monitor _monitor;

		 /// <summary>
		 /// Current path down the tree
		 /// - level:-1 is uninitialized (so that a call to <seealso cref="initialize(PageCursor)"/> is required)
		 /// - level: 0 is at root
		 /// - level: 1 is at first level below root
		 /// ... a.s.o
		 /// <para>
		 /// Calling <seealso cref="insert(PageCursor, StructurePropagation, object, object, ValueMerger, long, long)"/>
		 /// or <seealso cref="remove(PageCursor, StructurePropagation, object, object, long, long)"/> leaves the cursor
		 /// at the last updated page (tree node id) and remembers the path down the tree to where it is.
		 /// Further inserts/removals will move the cursor from its current position to where the next change will
		 /// take place using as few page pins as possible.
		 /// </para>
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private Level<KEY>[] levels = new Level[0];
		 private Level<KEY>[] _levels = new Level[0]; // grows on demand
		 private int _currentLevel = -1;
		 private double _ratioToKeepInLeftOnSplit;

		 /// <summary>
		 /// Keeps information about one level in a path down the tree where the <seealso cref="PageCursor"/> is currently at.
		 /// </summary>
		 /// @param <KEY> type of keys in the tree. </param>
		 private class Level<KEY>
		 {
			  // For comparing keys
			  internal readonly IComparer<KEY> Layout;
			  // Id of the tree node id this level of the path
			  internal long TreeNodeId;

			  // Child position which was selected from parent to get to this level
			  internal int ChildPos;
			  // Lower bound of key range this level covers
			  internal readonly KEY Lower;
			  // Whether or not the lower bound is fixed or open-ended (far left in the tree)
			  internal bool LowerIsOpenEnded;
			  // Upper bound of key range this level covers
			  internal readonly KEY Upper;
			  // Whether or not the upper bound is fixed or open-ended (far right in the tree)
			  internal bool UpperIsOpenEnded;

			  internal Level<T1>( Layout<T1> layout )
			  {
					this.Layout = layout;
					this.Lower = layout.NewKey();
					this.Upper = layout.NewKey();
			  }

			  /// <summary>
			  /// Returns whether or not the key range of this level of the path covers the given {@code key}.
			  /// </summary>
			  /// <param name="key"> KEY to check. </param>
			  /// <returns> {@code true} if key is within the key range if this level, otherwise {@code false}. </returns>
			  internal virtual bool Covers( KEY key )
			  {
					bool insideLower = LowerIsOpenEnded || Layout.Compare( key, Lower ) >= 0;
					bool insideHigher = UpperIsOpenEnded || Layout.Compare( key, Upper ) < 0;
					return insideLower && insideHigher;
			  }
		 }

		 internal InternalTreeLogic( IdProvider idProvider, TreeNode<KEY, VALUE> bTreeNode, Layout<KEY, VALUE> layout, GBPTree.Monitor monitor )
		 {
			  this._idProvider = idProvider;
			  this._bTreeNode = bTreeNode;
			  this._layout = layout;
			  this._newKeyPlaceHolder = layout.NewKey();
			  this._readKey = layout.NewKey();
			  this._readValue = layout.NewValue();
			  this._monitor = monitor;

			  // an arbitrary depth slightly bigger than an unimaginably big tree
			  EnsureStackCapacity( 10 );
		 }

		 private void EnsureStackCapacity( int depth )
		 {
			  if ( depth > _levels.Length )
			  {
					int oldStackLength = _levels.Length;
					_levels = Arrays.copyOf( _levels, depth );
					for ( int i = oldStackLength; i < depth; i++ )
					{
						 _levels[i] = new Level<KEY>( _layout );
					}
			  }
		 }

		 protected internal virtual void Initialize( PageCursor cursorAtRoot )
		 {
			  Initialize( cursorAtRoot, DEFAULT_SPLIT_RATIO );
		 }

		 /// <summary>
		 /// Prepare for starting over with new updates. </summary>
		 /// <param name="cursorAtRoot"> <seealso cref="PageCursor"/> pointing at root of tree. </param>
		 /// <param name="ratioToKeepInLeftOnSplit"> Decide how much to keep in left node on split, 0=keep nothing, 0.5=split 50-50, 1=keep everything. </param>
		 protected internal virtual void Initialize( PageCursor cursorAtRoot, double ratioToKeepInLeftOnSplit )
		 {
			  _currentLevel = 0;
			  Level<KEY> level = _levels[_currentLevel];
			  level.TreeNodeId = cursorAtRoot.CurrentPageId;
			  level.LowerIsOpenEnded = true;
			  level.UpperIsOpenEnded = true;
			  this._ratioToKeepInLeftOnSplit = ratioToKeepInLeftOnSplit;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean popLevel(org.Neo4Net.io.pagecache.PageCursor cursor) throws java.io.IOException
		 private bool PopLevel( PageCursor cursor )
		 {
			  _currentLevel--;
			  if ( _currentLevel >= 0 )
			  {
					Level<KEY> level = _levels[_currentLevel];
					TreeNode.GoTo( cursor, "parent", level.TreeNodeId );
					return true;
			  }
			  return false;
		 }

		 /// <summary>
		 /// Moves the cursor to the correct leaf for {@code key}, taking the current path into consideration
		 /// and moving the cursor as few hops as possible to get from the current position to the target position,
		 /// e.g given tree:
		 /// 
		 /// <pre>
		 ///              [A]
		 ///       ------/ | \------
		 ///      /        |        \
		 ///    [B]       [C]       [D]
		 ///   / | \     / | \     / | \
		 /// [E][F][G] [H][I][J] [K][L][M]
		 /// </pre>
		 /// 
		 /// Examples:
		 /// <para>
		 /// 
		 /// inserting a key into J (path A,C,J) after previously have inserted a key into F (path A,B,F):
		 /// </para>
		 /// <para>
		 /// <ol>
		 /// <li>Seeing that F doesn't cover new key</li>
		 /// <li>Popping stack, seeing that B doesn't cover new key (only by asking existing information in path)</li>
		 /// <li>Popping stack, seeing that A covers new key (only by asking existing information in path)</li>
		 /// <li>Binary search A to select C to go down to</li>
		 /// <li>Binary search C to select J to go down to</li>
		 /// </ol>
		 /// </para>
		 /// <para>
		 /// inserting a key into G (path A,B,G) after previously have inserted a key into F (path A,B,F):
		 /// </para>
		 /// <para>
		 /// <ol>
		 /// <li>Seeing that F doesn't cover new key</li>
		 /// <li>Popping stack, seeing that B covers new key (only by asking existing information in path)</li>
		 /// <li>Binary search B to select G to go down to</li>
		 /// </ol>
		 /// 
		 /// The closer keys are together from one change to the next, the fewer page pins and searches needs
		 /// to be performed to get there.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to move to the correct location. </param>
		 /// <param name="key"> KEY to make change for. </param>
		 /// <param name="stableGeneration"> stable generation. </param>
		 /// <param name="unstableGeneration"> unstable generation. </param>
		 /// <exception cref="IOException"> on <seealso cref="PageCursor"/> error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void moveToCorrectLeaf(org.Neo4Net.io.pagecache.PageCursor cursor, KEY key, long stableGeneration, long unstableGeneration) throws java.io.IOException
		 private void MoveToCorrectLeaf( PageCursor cursor, KEY key, long stableGeneration, long unstableGeneration )
		 {
			  int previousLevel = _currentLevel;
			  while ( !_levels[_currentLevel].covers( key ) )
			  {
					_currentLevel--;
			  }
			  if ( _currentLevel != previousLevel )
			  {
					TreeNode.GoTo( cursor, "parent", _levels[_currentLevel].treeNodeId );
			  }

			  while ( TreeNode.IsInternal( cursor ) )
			  {
					// We still need to go down further, but we're on the right path
					int keyCount = TreeNode.KeyCount( cursor );
					int searchResult = Search( cursor, INTERNAL, key, _readKey, keyCount );
					int childPos = positionOf( searchResult );
					if ( isHit( searchResult ) )
					{
						 childPos++;
					}

					Level<KEY> parentLevel = _levels[_currentLevel];
					_currentLevel++;
					EnsureStackCapacity( _currentLevel + 1 );
					Level<KEY> level = _levels[_currentLevel];

					// Restrict the key range as the cursor moves down to the next level
					level.ChildPos = childPos;
					level.LowerIsOpenEnded = childPos == 0 && !TreeNode.IsNode( TreeNode.LeftSibling( cursor, stableGeneration, unstableGeneration ) );
					if ( !level.LowerIsOpenEnded )
					{
						 if ( childPos == 0 )
						 {
							  _layout.copyKey( parentLevel.Lower, level.Lower );
							  level.LowerIsOpenEnded = parentLevel.LowerIsOpenEnded;
						 }
						 else
						 {
							  _bTreeNode.keyAt( cursor, level.Lower, childPos - 1, INTERNAL );
						 }
					}
					level.UpperIsOpenEnded = childPos >= keyCount && !TreeNode.IsNode( TreeNode.RightSibling( cursor, stableGeneration, unstableGeneration ) );
					if ( !level.UpperIsOpenEnded )
					{
						 if ( childPos == keyCount )
						 {
							  _layout.copyKey( parentLevel.Upper, level.Upper );
							  level.UpperIsOpenEnded = parentLevel.UpperIsOpenEnded;
						 }
						 else
						 {
							  _bTreeNode.keyAt( cursor, level.Upper, childPos, INTERNAL );
						 }
					}

					long childId = _bTreeNode.childAt( cursor, childPos, stableGeneration, unstableGeneration );
					PointerChecking.CheckPointer( childId, false );

					TreeNode.GoTo( cursor, "child", childId );
					level.TreeNodeId = cursor.CurrentPageId;

					Debug.Assert( assertNoSuccessor( cursor, stableGeneration, unstableGeneration ) );
			  }

			  Debug.Assert( TreeNode.IsLeaf( cursor ), "Ended up on a tree node which isn't a leaf after moving cursor towards " + key + ", cursor is at " + cursor.CurrentPageId );
		 }

		 /// <summary>
		 /// Insert {@code key} and associate it with {@code value} if {@code key} does not already exist in
		 /// tree.
		 /// <para>
		 /// If {@code key} already exists in tree, {@code valueMerger} will be used to decide how to merge existing value
		 /// with {@code value}.
		 /// </para>
		 /// <para>
		 /// Insert may cause structural changes in the tree in form of splits and or new generation of nodes being created.
		 /// Note that a split in a leaf can propagate all the way up to root node.
		 /// </para>
		 /// <para>
		 /// Structural changes in tree that need to propagate to the level above will be reported through the provided
		 /// <seealso cref="StructurePropagation"/> by overwriting state. This is safe because structure changes happens one level
		 /// at the time.
		 /// <seealso cref="StructurePropagation"/> is provided from outside to minimize garbage.
		 /// </para>
		 /// <para>
		 /// When this method returns, {@code structurePropagation} will be populated with information about split or new
		 /// generation version of root. This needs to be handled by caller.
		 /// </para>
		 /// <para>
		 /// Leaves cursor at the page which was last updated. No guarantees on offset.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> pinned to root of tree (if first insert/remove since
		 /// <seealso cref="initialize(PageCursor)"/>) or at where last insert/remove left it. </param>
		 /// <param name="structurePropagation"> <seealso cref="StructurePropagation"/> used to report structure changes between tree levels. </param>
		 /// <param name="key"> key to be inserted </param>
		 /// <param name="value"> value to be associated with key </param>
		 /// <param name="valueMerger"> <seealso cref="ValueMerger"/> for deciding what to do with existing keys </param>
		 /// <param name="stableGeneration"> stable generation, i.e. generations <= this generation are considered stable. </param>
		 /// <param name="unstableGeneration"> unstable generation, i.e. generation which is under development right now. </param>
		 /// <exception cref="IOException"> on cursor failure </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void insert(org.Neo4Net.io.pagecache.PageCursor cursor, StructurePropagation<KEY> structurePropagation, KEY key, VALUE value, ValueMerger<KEY,VALUE> valueMerger, long stableGeneration, long unstableGeneration) throws java.io.IOException
		 internal virtual void Insert( PageCursor cursor, StructurePropagation<KEY> structurePropagation, KEY key, VALUE value, ValueMerger<KEY, VALUE> valueMerger, long stableGeneration, long unstableGeneration )
		 {
			  Debug.Assert( CursorIsAtExpectedLocation( cursor ) );
			  _bTreeNode.validateKeyValueSize( key, value );
			  MoveToCorrectLeaf( cursor, key, stableGeneration, unstableGeneration );

			  InsertInLeaf( cursor, structurePropagation, key, value, valueMerger, stableGeneration, unstableGeneration );

			  HandleStructureChanges( cursor, structurePropagation, stableGeneration, unstableGeneration );
		 }

		 private int Search( PageCursor cursor, TreeNode.Type type, KEY key, KEY readKey, int keyCount )
		 {
			  int searchResult = KeySearch.Search( cursor, _bTreeNode, type, key, readKey, keyCount );
			  KeySearch.AssertSuccess( searchResult );
			  return searchResult;
		 }

		 /// <summary>
		 /// Asserts that cursor is where it's expected to be at, compared to current level.
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> to check. </param>
		 /// <returns> {@code true} so that it can be called in an {@code assert} statement. </returns>
		 private bool CursorIsAtExpectedLocation( PageCursor cursor )
		 {
			  Debug.Assert( _currentLevel >= 0, "Uninitialized tree logic, currentLevel:" + _currentLevel );
			  long currentPageId = cursor.CurrentPageId;
			  long expectedPageId = _levels[_currentLevel].treeNodeId;
			  Debug.Assert( currentPageId == expectedPageId, "Expected cursor to be at page:" + expectedPageId + " at level:" + _currentLevel + ", but was at page:" + currentPageId );
			  return true;
		 }

		 /// <summary>
		 /// Leaves cursor at same page as when called. No guarantees on offset.
		 /// <para>
		 /// Insertion in internal is always triggered by a split in child.
		 /// The result of a split is a primary key that is sent upwards in the b+tree and the newly created right child.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> pinned to page containing internal node, current node </param>
		 /// <param name="structurePropagation"> <seealso cref="StructurePropagation"/> used to report structure changes between tree levels. </param>
		 /// <param name="keyCount"> the key count of current node </param>
		 /// <param name="primKey"> the primary key to be inserted </param>
		 /// <param name="rightChild"> the right child of primKey </param>
		 /// <exception cref="IOException"> on cursor failure </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void insertInInternal(org.Neo4Net.io.pagecache.PageCursor cursor, StructurePropagation<KEY> structurePropagation, int keyCount, KEY primKey, long rightChild, long stableGeneration, long unstableGeneration) throws java.io.IOException
		 private void InsertInInternal( PageCursor cursor, StructurePropagation<KEY> structurePropagation, int keyCount, KEY primKey, long rightChild, long stableGeneration, long unstableGeneration )
		 {
			  CreateSuccessorIfNeeded( cursor, structurePropagation, UPDATE_MID_CHILD, stableGeneration, unstableGeneration );

			  DoInsertInInternal( cursor, structurePropagation, keyCount, primKey, rightChild, stableGeneration, unstableGeneration );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void doInsertInInternal(org.Neo4Net.io.pagecache.PageCursor cursor, StructurePropagation<KEY> structurePropagation, int keyCount, KEY primKey, long rightChild, long stableGeneration, long unstableGeneration) throws java.io.IOException
		 private void DoInsertInInternal( PageCursor cursor, StructurePropagation<KEY> structurePropagation, int keyCount, KEY primKey, long rightChild, long stableGeneration, long unstableGeneration )
		 {
			  Overflow overflow = _bTreeNode.internalOverflow( cursor, keyCount, primKey );
			  if ( overflow == YES )
			  {
					// Overflow
					// We will overwrite rightKey in structurePropagation, so copy it over to a place holder
					_layout.copyKey( primKey, _newKeyPlaceHolder );
					SplitInternal( cursor, structurePropagation, _newKeyPlaceHolder, rightChild, keyCount, stableGeneration, unstableGeneration );
					return;
			  }

			  if ( overflow == NO_NEED_DEFRAG )
			  {
					_bTreeNode.defragmentInternal( cursor );
			  }

			  // No overflow
			  int pos = positionOf( Search( cursor, INTERNAL, primKey, _readKey, keyCount ) );
			  _bTreeNode.insertKeyAndRightChildAt( cursor, primKey, rightChild, pos, keyCount, stableGeneration, unstableGeneration );
			  // Increase key count
			  TreeNode.SetKeyCount( cursor, keyCount + 1 );
		 }

		 /// <summary>
		 /// Leaves cursor at same page as when called. No guarantees on offset.
		 /// <para>
		 /// Split in internal node caused by an insertion of rightKey and newRightChild
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> pinned to page containing internal node, full node. </param>
		 /// <param name="structurePropagation"> <seealso cref="StructurePropagation"/> used to report structure changes between tree levels. </param>
		 /// <param name="newKey"> new key to be inserted together with newRightChild, causing the split </param>
		 /// <param name="newRightChild"> new child to be inserted to the right of newKey </param>
		 /// <param name="keyCount"> key count for fullNode </param>
		 /// <exception cref="IOException"> on cursor failure </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void splitInternal(org.Neo4Net.io.pagecache.PageCursor cursor, StructurePropagation<KEY> structurePropagation, KEY newKey, long newRightChild, int keyCount, long stableGeneration, long unstableGeneration) throws java.io.IOException
		 private void SplitInternal( PageCursor cursor, StructurePropagation<KEY> structurePropagation, KEY newKey, long newRightChild, int keyCount, long stableGeneration, long unstableGeneration )
		 {
			  long current = cursor.CurrentPageId;
			  long oldRight = TreeNode.RightSibling( cursor, stableGeneration, unstableGeneration );
			  PointerChecking.CheckPointer( oldRight, true );
			  long newRight = _idProvider.acquireNewId( stableGeneration, unstableGeneration );

			  // Find position to insert new key
			  int pos = positionOf( Search( cursor, INTERNAL, newKey, _readKey, keyCount ) );

			  // Update structurePropagation
			  structurePropagation.HasRightKeyInsert = true;
			  structurePropagation.MidChild = current;
			  structurePropagation.RightChild = newRight;

			  using ( PageCursor rightCursor = cursor.OpenLinkedCursor( newRight ) )
			  {
					// Initialize new right
					TreeNode.GoTo( rightCursor, "new right sibling in split", newRight );
					_bTreeNode.initializeInternal( rightCursor, stableGeneration, unstableGeneration );
					TreeNode.SetRightSibling( rightCursor, oldRight, stableGeneration, unstableGeneration );
					TreeNode.SetLeftSibling( rightCursor, current, stableGeneration, unstableGeneration );

					// Do split
					_bTreeNode.doSplitInternal( cursor, keyCount, rightCursor, pos, newKey, newRightChild, stableGeneration, unstableGeneration, structurePropagation.RightKey, _ratioToKeepInLeftOnSplit );
			  }

			  // Update old right with new left sibling (newRight)
			  if ( TreeNode.IsNode( oldRight ) )
			  {
					using ( PageCursor oldRightCursor = cursor.OpenLinkedCursor( oldRight ) )
					{
						 TreeNode.GoTo( oldRightCursor, "old right sibling", oldRight );
						 TreeNode.SetLeftSibling( oldRightCursor, newRight, stableGeneration, unstableGeneration );
					}
			  }

			  // Update left node with new right sibling
			  TreeNode.SetRightSibling( cursor, newRight, stableGeneration, unstableGeneration );
		 }

		 /// <summary>
		 /// Leaves cursor at same page as when called. No guarantees on offset.
		 /// <para>
		 /// Split in leaf node caused by an insertion of key and value
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> pinned to page containing leaf node targeted for insertion. </param>
		 /// <param name="structurePropagation"> <seealso cref="StructurePropagation"/> used to report structure changes between tree levels. </param>
		 /// <param name="key"> key to be inserted </param>
		 /// <param name="value"> value to be associated with key </param>
		 /// <param name="valueMerger"> <seealso cref="ValueMerger"/> for deciding what to do with existing keys </param>
		 /// <exception cref="IOException"> on cursor failure </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void insertInLeaf(org.Neo4Net.io.pagecache.PageCursor cursor, StructurePropagation<KEY> structurePropagation, KEY key, VALUE value, ValueMerger<KEY,VALUE> valueMerger, long stableGeneration, long unstableGeneration) throws java.io.IOException
		 private void InsertInLeaf( PageCursor cursor, StructurePropagation<KEY> structurePropagation, KEY key, VALUE value, ValueMerger<KEY, VALUE> valueMerger, long stableGeneration, long unstableGeneration )
		 {
			  int keyCount = TreeNode.KeyCount( cursor );
			  int search = search( cursor, LEAF, key, _readKey, keyCount );
			  int pos = positionOf( search );
			  if ( isHit( search ) )
			  {
					OverwriteValue( cursor, structurePropagation, key, value, valueMerger, pos, keyCount, stableGeneration, unstableGeneration );
					return;
			  }

			  CreateSuccessorIfNeeded( cursor, structurePropagation, UPDATE_MID_CHILD, stableGeneration, unstableGeneration );

			  DoInsertInLeaf( cursor, structurePropagation, key, value, pos, keyCount, stableGeneration, unstableGeneration );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void overwriteValue(org.Neo4Net.io.pagecache.PageCursor cursor, StructurePropagation<KEY> structurePropagation, KEY key, VALUE value, ValueMerger<KEY,VALUE> valueMerger, int pos, int keyCount, long stableGeneration, long unstableGeneration) throws java.io.IOException
		 private void OverwriteValue( PageCursor cursor, StructurePropagation<KEY> structurePropagation, KEY key, VALUE value, ValueMerger<KEY, VALUE> valueMerger, int pos, int keyCount, long stableGeneration, long unstableGeneration )
		 {
			  // this key already exists, what shall we do? ask the valueMerger
			  _bTreeNode.valueAt( cursor, _readValue, pos );
			  VALUE mergedValue = valueMerger.Merge( _readKey, key, _readValue, value );
			  if ( mergedValue != default( VALUE ) )
			  {
					CreateSuccessorIfNeeded( cursor, structurePropagation, UPDATE_MID_CHILD, stableGeneration, unstableGeneration );
					// simple, just write the merged value right in there
					bool couldOverwrite = _bTreeNode.setValueAt( cursor, mergedValue, pos );
					//noinspection StatementWithEmptyBody
					if ( !couldOverwrite )
					{
						 // Value could not be overwritten in a simple way because they differ in size.
						 // Delete old value
						 _bTreeNode.removeKeyValueAt( cursor, pos, keyCount );
						 TreeNode.SetKeyCount( cursor, keyCount - 1 );
						 bool didSplit = DoInsertInLeaf( cursor, structurePropagation, key, mergedValue, pos, keyCount - 1, stableGeneration, unstableGeneration );
						 if ( !didSplit && _bTreeNode.leafUnderflow( cursor, keyCount ) )
						 {
							  UnderflowInLeaf( cursor, structurePropagation, keyCount, stableGeneration, unstableGeneration );
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean doInsertInLeaf(org.Neo4Net.io.pagecache.PageCursor cursor, StructurePropagation<KEY> structurePropagation, KEY key, VALUE value, int pos, int keyCount, long stableGeneration, long unstableGeneration) throws java.io.IOException
		 private bool DoInsertInLeaf( PageCursor cursor, StructurePropagation<KEY> structurePropagation, KEY key, VALUE value, int pos, int keyCount, long stableGeneration, long unstableGeneration )
		 {
			  Overflow overflow = _bTreeNode.leafOverflow( cursor, keyCount, key, value );
			  if ( overflow == YES )
			  {
					// Overflow, split leaf
					SplitLeaf( cursor, structurePropagation, key, value, keyCount, stableGeneration, unstableGeneration );
					return true;
			  }

			  if ( overflow == NO_NEED_DEFRAG )
			  {
					_bTreeNode.defragmentLeaf( cursor );
			  }

			  // No overflow, insert key and value
			  _bTreeNode.insertKeyValueAt( cursor, key, value, pos, keyCount );
			  TreeNode.SetKeyCount( cursor, keyCount + 1 );
			  return false;
		 }

		 /// <summary>
		 /// Leaves cursor at same page as when called. No guarantees on offset.
		 /// Cursor is expected to be pointing to full leaf.
		 /// </summary>
		 /// <param name="cursor"> cursor pointing into full (left) leaf that should be split in two. </param>
		 /// <param name="structurePropagation"> <seealso cref="StructurePropagation"/> used to report structure changes between tree levels. </param>
		 /// <param name="newKey"> key to be inserted </param>
		 /// <param name="newValue"> value to be inserted (in association with key) </param>
		 /// <param name="keyCount"> number of keys in this leaf (it was already read anyway) </param>
		 /// <exception cref="IOException"> on cursor failure </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void splitLeaf(org.Neo4Net.io.pagecache.PageCursor cursor, StructurePropagation<KEY> structurePropagation, KEY newKey, VALUE newValue, int keyCount, long stableGeneration, long unstableGeneration) throws java.io.IOException
		 private void SplitLeaf( PageCursor cursor, StructurePropagation<KEY> structurePropagation, KEY newKey, VALUE newValue, int keyCount, long stableGeneration, long unstableGeneration )
		 {
			  // To avoid moving cursor between pages we do all operations on left node first.

			  // UPDATE SIBLINGS
			  //
			  // Before split
			  // newRight is leaf node to be inserted between left and oldRight
			  // [left] -> [oldRight]
			  //
			  //     [newRight]
			  //
			  // After split
			  // [left] -> [newRight] -> [oldRight]
			  //

			  long current = cursor.CurrentPageId;
			  long oldRight = TreeNode.RightSibling( cursor, stableGeneration, unstableGeneration );
			  PointerChecking.CheckPointer( oldRight, true );
			  long newRight = _idProvider.acquireNewId( stableGeneration, unstableGeneration );

			  // BALANCE KEYS AND VALUES
			  // Two different scenarios
			  // Before split
			  // [key1]<=[key2]<=[key3]<=[key4]<=[key5]   (<= greater than or equal to)
			  //                           ^
			  //                           |
			  //                      pos  |
			  // [newKey] -----------------
			  //
			  // After split
			  // Left
			  // [key1]<=[key2]<=[key3]
			  //
			  // Right
			  // [newKey][key4][key5]
			  //
			  // Before split
			  // [key1]<=[key2]<=[key3]<=[key4]<=[key5]   (<= greater than or equal to)
			  //   ^
			  //   | pos
			  //   |
			  // [newKey]
			  //
			  // After split
			  // Left
			  // [newKey]<=[key1]<=[key2]
			  //
			  // Right
			  // [key3][key4][key5]
			  //

			  // CONCURRENCY
			  // To have readers see correct state at all times, the order of updates must be:
			  // 1. Acquire new page id R
			  // 2. Copy "right-hand" keys/values to R and set key count
			  // 3. Set L's right sibling to R
			  // 4. Set key count of L to new "left-hand" key count
			  // 5. Write new key/values into L

			  // Position where newKey / newValue is to be inserted
			  int pos = positionOf( Search( cursor, LEAF, newKey, _readKey, keyCount ) );

			  structurePropagation.HasRightKeyInsert = true;
			  structurePropagation.MidChild = current;
			  structurePropagation.RightChild = newRight;

			  using ( PageCursor rightCursor = cursor.OpenLinkedCursor( newRight ) )
			  {
					// Initialize new right
					TreeNode.GoTo( rightCursor, "new right sibling in split", newRight );
					_bTreeNode.initializeLeaf( rightCursor, stableGeneration, unstableGeneration );
					TreeNode.SetRightSibling( rightCursor, oldRight, stableGeneration, unstableGeneration );
					TreeNode.SetLeftSibling( rightCursor, current, stableGeneration, unstableGeneration );

					// Do split
					_bTreeNode.doSplitLeaf( cursor, keyCount, rightCursor, pos, newKey, newValue, structurePropagation.RightKey, _ratioToKeepInLeftOnSplit );
			  }

			  // Update old right with new left sibling (newRight)
			  if ( TreeNode.IsNode( oldRight ) )
			  {
					using ( PageCursor oldRightCursor = cursor.OpenLinkedCursor( oldRight ) )
					{
						 TreeNode.GoTo( oldRightCursor, "old right sibling", oldRight );
						 TreeNode.SetLeftSibling( oldRightCursor, newRight, stableGeneration, unstableGeneration );
					}
			  }

			  // Update left child
			  TreeNode.SetRightSibling( cursor, newRight, stableGeneration, unstableGeneration );
		 }

		 /// <summary>
		 /// Remove given {@code key} and associated value from tree if it exists. The removed value will be stored in
		 /// provided {@code into} which will be returned for convenience.
		 /// <para>
		 /// If the given {@code key} does not exist in tree, return {@code null}.
		 /// </para>
		 /// <para>
		 /// Structural changes in tree that need to propagate to the level above will be reported through the provided
		 /// <seealso cref="StructurePropagation"/> by overwriting state. This is safe because structure changes happens one level
		 /// at the time.
		 /// <seealso cref="StructurePropagation"/> is provided from outside to minimize garbage.
		 /// </para>
		 /// <para>
		 /// Leaves cursor at the page which was last updated. No guarantees on offset.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> pinned to root of tree (if first insert/remove since
		 /// <seealso cref="initialize(PageCursor)"/>) or at where last insert/remove left it. </param>
		 /// <param name="structurePropagation"> <seealso cref="StructurePropagation"/> used to report structure changes between tree levels. </param>
		 /// <param name="key"> key to be removed </param>
		 /// <param name="into"> {@code VALUE} instance to write removed value to </param>
		 /// <param name="stableGeneration"> stable generation, i.e. generations <= this generation are considered stable. </param>
		 /// <param name="unstableGeneration"> unstable generation, i.e. generation which is under development right now. </param>
		 /// <returns> Provided {@code into}, populated with removed value for convenience if {@code key} was removed.
		 /// Otherwise {@code null}. </returns>
		 /// <exception cref="IOException"> on cursor failure </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: VALUE remove(org.Neo4Net.io.pagecache.PageCursor cursor, StructurePropagation<KEY> structurePropagation, KEY key, VALUE into, long stableGeneration, long unstableGeneration) throws java.io.IOException
		 internal virtual VALUE Remove( PageCursor cursor, StructurePropagation<KEY> structurePropagation, KEY key, VALUE into, long stableGeneration, long unstableGeneration )
		 {
			  Debug.Assert( CursorIsAtExpectedLocation( cursor ) );
			  MoveToCorrectLeaf( cursor, key, stableGeneration, unstableGeneration );

			  if ( !RemoveFromLeaf( cursor, structurePropagation, key, into, stableGeneration, unstableGeneration ) )
			  {
					return default( VALUE );
			  }

			  HandleStructureChanges( cursor, structurePropagation, stableGeneration, unstableGeneration );

			  if ( _currentLevel <= 0 )
			  {
					TryShrinkTree( cursor, structurePropagation, stableGeneration, unstableGeneration );
			  }

			  return into;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void handleStructureChanges(org.Neo4Net.io.pagecache.PageCursor cursor, StructurePropagation<KEY> structurePropagation, long stableGeneration, long unstableGeneration) throws java.io.IOException
		 private void HandleStructureChanges( PageCursor cursor, StructurePropagation<KEY> structurePropagation, long stableGeneration, long unstableGeneration )
		 {
			  while ( structurePropagation.HasLeftChildUpdate || structurePropagation.HasMidChildUpdate || structurePropagation.HasRightChildUpdate || structurePropagation.HasLeftKeyReplace || structurePropagation.HasRightKeyReplace || structurePropagation.HasRightKeyInsert )
			  {
					int pos = _levels[_currentLevel].childPos;
					if ( !PopLevel( cursor ) )
					{
						 // Root split, let that be handled outside
						 break;
					}

					if ( structurePropagation.HasLeftChildUpdate )
					{
						 structurePropagation.HasLeftChildUpdate = false;
						 if ( pos == 0 )
						 {
							  UpdateRightmostChildInLeftSibling( cursor, structurePropagation.LeftChild, stableGeneration, unstableGeneration );
						 }
						 else
						 {
							  _bTreeNode.setChildAt( cursor, structurePropagation.LeftChild, pos - 1, stableGeneration, unstableGeneration );
						 }
					}

					if ( structurePropagation.HasMidChildUpdate )
					{
						 UpdateMidChild( cursor, structurePropagation, pos, stableGeneration, unstableGeneration );
					}

					if ( structurePropagation.HasRightChildUpdate )
					{
						 structurePropagation.HasRightChildUpdate = false;
						 int keyCount = TreeNode.KeyCount( cursor );
						 if ( pos == keyCount )
						 {
							  UpdateLeftmostChildInRightSibling( cursor, structurePropagation.RightChild, stableGeneration, unstableGeneration );
						 }
						 else
						 {
							  _bTreeNode.setChildAt( cursor, structurePropagation.RightChild, pos + 1, stableGeneration, unstableGeneration );
						 }
					}

					// Insert before replace because replace can lead to split and another insert in next level.
					// Replace can only come from rebalance on lower levels and because we do no rebalance among
					// internal nodes we will only ever have one replace on our way up.
					if ( structurePropagation.HasRightKeyInsert )
					{
						 structurePropagation.HasRightKeyInsert = false;
						 InsertInInternal( cursor, structurePropagation, TreeNode.KeyCount( cursor ), structurePropagation.RightKey, structurePropagation.RightChild, stableGeneration, unstableGeneration );
					}

					if ( structurePropagation.HasLeftKeyReplace && _levels[_currentLevel].covers( structurePropagation.LeftKey ) )
					{
						 structurePropagation.HasLeftKeyReplace = false;
						 switch ( structurePropagation.KeyReplaceStrategy )
						 {
						 case Neo4Net.Index.Internal.gbptree.StructurePropagation.KeyReplaceStrategy.Replace:
							  OverwriteKeyInternal( cursor, structurePropagation, structurePropagation.LeftKey, pos - 1, stableGeneration, unstableGeneration );
							  break;
						 case Neo4Net.Index.Internal.gbptree.StructurePropagation.KeyReplaceStrategy.Bubble:
							  ReplaceKeyByBubbleRightmostFromSubtree( cursor, structurePropagation, pos - 1, stableGeneration, unstableGeneration );
							  break;
						 default:
							  throw new System.ArgumentException( "Unknown KeyReplaceStrategy " + structurePropagation.KeyReplaceStrategy );
						 }
					}

					if ( structurePropagation.HasRightKeyReplace && _levels[_currentLevel].covers( structurePropagation.RightKey ) )
					{
						 structurePropagation.HasRightKeyReplace = false;
						 switch ( structurePropagation.KeyReplaceStrategy )
						 {
						 case Neo4Net.Index.Internal.gbptree.StructurePropagation.KeyReplaceStrategy.Replace:
							  OverwriteKeyInternal( cursor, structurePropagation, structurePropagation.RightKey, pos, stableGeneration, unstableGeneration );
							  break;
						 case Neo4Net.Index.Internal.gbptree.StructurePropagation.KeyReplaceStrategy.Bubble:
							  ReplaceKeyByBubbleRightmostFromSubtree( cursor, structurePropagation, pos, stableGeneration, unstableGeneration );
							  break;
						 default:
							  throw new System.ArgumentException( "Unknown KeyReplaceStrategy " + structurePropagation.KeyReplaceStrategy );
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void overwriteKeyInternal(org.Neo4Net.io.pagecache.PageCursor cursor, StructurePropagation<KEY> structurePropagation, KEY newKey, int pos, long stableGeneration, long unstableGeneration) throws java.io.IOException
		 private void OverwriteKeyInternal( PageCursor cursor, StructurePropagation<KEY> structurePropagation, KEY newKey, int pos, long stableGeneration, long unstableGeneration )
		 {
			  CreateSuccessorIfNeeded( cursor, structurePropagation, UPDATE_MID_CHILD, stableGeneration, unstableGeneration );
			  int keyCount = TreeNode.KeyCount( cursor );
			  bool couldOverwrite = _bTreeNode.setKeyAtInternal( cursor, newKey, pos );
			  if ( !couldOverwrite )
			  {
					// Remove key and right child
					long rightChild = _bTreeNode.childAt( cursor, pos + 1, stableGeneration, unstableGeneration );
					_bTreeNode.removeKeyAndRightChildAt( cursor, pos, keyCount );
					TreeNode.SetKeyCount( cursor, keyCount - 1 );

					DoInsertInInternal( cursor, structurePropagation, keyCount - 1, newKey, rightChild, stableGeneration, unstableGeneration );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void tryShrinkTree(org.Neo4Net.io.pagecache.PageCursor cursor, StructurePropagation<KEY> structurePropagation, long stableGeneration, long unstableGeneration) throws java.io.IOException
		 private void TryShrinkTree( PageCursor cursor, StructurePropagation<KEY> structurePropagation, long stableGeneration, long unstableGeneration )
		 {
			  // New root will be propagated out. If rootKeyCount is 0 we can shrink the tree.
			  int rootKeyCount = TreeNode.KeyCount( cursor );

			  while ( rootKeyCount == 0 && TreeNode.IsInternal( cursor ) )
			  {
					long oldRoot = cursor.CurrentPageId;
					long onlyChildOfRoot = _bTreeNode.childAt( cursor, 0, stableGeneration, unstableGeneration );
					PointerChecking.CheckPointer( onlyChildOfRoot, false );

					structurePropagation.HasMidChildUpdate = true;
					structurePropagation.MidChild = onlyChildOfRoot;

					_idProvider.releaseId( stableGeneration, unstableGeneration, oldRoot );
					TreeNode.GoTo( cursor, "child", onlyChildOfRoot );

					rootKeyCount = TreeNode.KeyCount( cursor );
					_monitor.treeShrink();
			  }
		 }

		 private void UpdateMidChild( PageCursor cursor, StructurePropagation<KEY> structurePropagation, int childPos, long stableGeneration, long unstableGeneration )
		 {
			  structurePropagation.HasMidChildUpdate = false;
			  _bTreeNode.setChildAt( cursor, structurePropagation.MidChild, childPos, stableGeneration, unstableGeneration );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void replaceKeyByBubbleRightmostFromSubtree(org.Neo4Net.io.pagecache.PageCursor cursor, StructurePropagation<KEY> structurePropagation, int subtreePosition, long stableGeneration, long unstableGeneration) throws java.io.IOException
		 private void ReplaceKeyByBubbleRightmostFromSubtree( PageCursor cursor, StructurePropagation<KEY> structurePropagation, int subtreePosition, long stableGeneration, long unstableGeneration )
		 {
			  long currentPageId = cursor.CurrentPageId;
			  long subtree = _bTreeNode.childAt( cursor, subtreePosition, stableGeneration, unstableGeneration );
			  PointerChecking.CheckPointer( subtree, false );

			  TreeNode.GoTo( cursor, "child", subtree );
			  bool foundKeyBelow = BubbleRightmostKeyRecursive( cursor, structurePropagation, currentPageId, stableGeneration, unstableGeneration );

			  // Propagate structurePropagation from below
			  if ( structurePropagation.HasMidChildUpdate )
			  {
					UpdateMidChild( cursor, structurePropagation, subtreePosition, stableGeneration, unstableGeneration );
			  }

			  if ( foundKeyBelow )
			  {
					// A key has been bubble up to us.
					// It's in structurePropagation.bubbleKey and should be inserted in subtreePosition.
					OverwriteKeyInternal( cursor, structurePropagation, structurePropagation.BubbleKey, subtreePosition, stableGeneration, unstableGeneration );
			  }
			  else
			  {
					// No key could be found in subtree, it's completely empty and can be removed.
					// We shift keys and children in this internal node to the left (potentially creating new version of this
					// node).
					CreateSuccessorIfNeeded( cursor, structurePropagation, UPDATE_MID_CHILD, stableGeneration, unstableGeneration );
					int keyCount = TreeNode.KeyCount( cursor );
					SimplyRemoveFromInternal( cursor, keyCount, subtreePosition, true );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean bubbleRightmostKeyRecursive(org.Neo4Net.io.pagecache.PageCursor cursor, StructurePropagation<KEY> structurePropagation, long previousNode, long stableGeneration, long unstableGeneration) throws java.io.IOException
		 private bool BubbleRightmostKeyRecursive( PageCursor cursor, StructurePropagation<KEY> structurePropagation, long previousNode, long stableGeneration, long unstableGeneration )
		 {
			  try
			  {
					if ( TreeNode.IsLeaf( cursor ) )
					{
						 // Base case
						 return false;
					}
					// Recursive case
					long currentPageId = cursor.CurrentPageId;
					int keyCount = TreeNode.KeyCount( cursor );
					long rightmostSubtree = _bTreeNode.childAt( cursor, keyCount, stableGeneration, unstableGeneration );
					PointerChecking.CheckPointer( rightmostSubtree, false );

					TreeNode.GoTo( cursor, "child", rightmostSubtree );

					bool foundKeyBelow = BubbleRightmostKeyRecursive( cursor, structurePropagation, currentPageId, stableGeneration, unstableGeneration );

					// Propagate structurePropagation from below
					if ( structurePropagation.HasMidChildUpdate )
					{
						 UpdateMidChild( cursor, structurePropagation, keyCount, stableGeneration, unstableGeneration );
					}

					if ( foundKeyBelow )
					{
						 return true;
					}

					if ( keyCount == 0 )
					{
						 // This subtree does not contain anything any more
						 // Repoint sibling and add to freelist and return false
						 ConnectLeftAndRightSibling( cursor, stableGeneration, unstableGeneration );
						 _idProvider.releaseId( stableGeneration, unstableGeneration, currentPageId );
						 return false;
					}

					// Create new version of node, save rightmost key in structurePropagation, remove rightmost key and child
					CreateSuccessorIfNeeded( cursor, structurePropagation, UPDATE_MID_CHILD, stableGeneration, unstableGeneration );
					_bTreeNode.keyAt( cursor, structurePropagation.BubbleKey, keyCount - 1, INTERNAL );
					SimplyRemoveFromInternal( cursor, keyCount, keyCount - 1, false );

					return true;
			  }
			  finally
			  {
					TreeNode.GoTo( cursor, "back to previous node", previousNode );
			  }
		 }

		 private int SimplyRemoveFromInternal( PageCursor cursor, int keyCount, int keyPos, bool leftChild )
		 {
			  // Remove key and child
			  if ( leftChild )
			  {
					_bTreeNode.removeKeyAndLeftChildAt( cursor, keyPos, keyCount );
			  }
			  else
			  {
					_bTreeNode.removeKeyAndRightChildAt( cursor, keyPos, keyCount );
			  }

			  // Decrease key count
			  int newKeyCount = keyCount - 1;
			  TreeNode.SetKeyCount( cursor, newKeyCount );
			  return newKeyCount;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void updateRightmostChildInLeftSibling(org.Neo4Net.io.pagecache.PageCursor cursor, long childPointer, long stableGeneration, long unstableGeneration) throws java.io.IOException
		 private void UpdateRightmostChildInLeftSibling( PageCursor cursor, long childPointer, long stableGeneration, long unstableGeneration )
		 {
			  long leftSibling = TreeNode.LeftSibling( cursor, stableGeneration, unstableGeneration );
			  // Left sibling is not allowed to be NO_NODE here because that means there is a child node with no parent
			  PointerChecking.CheckPointer( leftSibling, false );

			  using ( PageCursor leftSiblingCursor = cursor.OpenLinkedCursor( leftSibling ) )
			  {
					TreeNode.GoTo( leftSiblingCursor, "left sibling", leftSibling );
					int keyCount = TreeNode.KeyCount( leftSiblingCursor );
					_bTreeNode.setChildAt( leftSiblingCursor, childPointer, keyCount, stableGeneration, unstableGeneration );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void updateLeftmostChildInRightSibling(org.Neo4Net.io.pagecache.PageCursor cursor, long childPointer, long stableGeneration, long unstableGeneration) throws java.io.IOException
		 private void UpdateLeftmostChildInRightSibling( PageCursor cursor, long childPointer, long stableGeneration, long unstableGeneration )
		 {
			  long rightSibling = TreeNode.RightSibling( cursor, stableGeneration, unstableGeneration );
			  // Left sibling is not allowed to be NO_NODE here because that means there is a child node with no parent
			  PointerChecking.CheckPointer( rightSibling, false );

			  using ( PageCursor rightSiblingCursor = cursor.OpenLinkedCursor( rightSibling ) )
			  {
					TreeNode.GoTo( rightSiblingCursor, "right sibling", rightSibling );
					_bTreeNode.setChildAt( rightSiblingCursor, childPointer, 0, stableGeneration, unstableGeneration );
			  }
		 }

		 /// <summary>
		 /// Remove given {@code key} and associated value from tree if it exists. The removed value will be stored in
		 /// provided {@code into} which will be returned for convenience.
		 /// <para>
		 /// If the given {@code key} does not exist in tree, return {@code null}.
		 /// </para>
		 /// <para>
		 /// Leaves cursor at same page as when called. No guarantees on offset.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> pinned to page where remove is to be done. </param>
		 /// <param name="structurePropagation"> <seealso cref="StructurePropagation"/> used to report structure changes between tree levels. </param>
		 /// <param name="key"> key to be removed </param>
		 /// <param name="into"> {@code VALUE} instance to write removed value to </param>
		 /// <param name="stableGeneration"> stable generation, i.e. generations <= this generation are considered stable. </param>
		 /// <param name="unstableGeneration"> unstable generation, i.e. generation which is under development right now. </param>
		 /// <returns> {@code true} if key was removed, otherwise {@code false}. </returns>
		 /// <exception cref="IOException"> on cursor failure </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean removeFromLeaf(org.Neo4Net.io.pagecache.PageCursor cursor, StructurePropagation<KEY> structurePropagation, KEY key, VALUE into, long stableGeneration, long unstableGeneration) throws java.io.IOException
		 private bool RemoveFromLeaf( PageCursor cursor, StructurePropagation<KEY> structurePropagation, KEY key, VALUE into, long stableGeneration, long unstableGeneration )
		 {
			  int keyCount = TreeNode.KeyCount( cursor );

			  int search = search( cursor, LEAF, key, _readKey, keyCount );
			  int pos = positionOf( search );
			  bool hit = isHit( search );
			  if ( !hit )
			  {
					return false;
			  }

			  CreateSuccessorIfNeeded( cursor, structurePropagation, UPDATE_MID_CHILD, stableGeneration, unstableGeneration );
			  keyCount = SimplyRemoveFromLeaf( cursor, into, keyCount, pos );

			  if ( _bTreeNode.leafUnderflow( cursor, keyCount ) )
			  {
					// Underflow
					UnderflowInLeaf( cursor, structurePropagation, keyCount, stableGeneration, unstableGeneration );
			  }

			  return true;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void underflowInLeaf(org.Neo4Net.io.pagecache.PageCursor cursor, StructurePropagation<KEY> structurePropagation, int keyCount, long stableGeneration, long unstableGeneration) throws java.io.IOException
		 private void UnderflowInLeaf( PageCursor cursor, StructurePropagation<KEY> structurePropagation, int keyCount, long stableGeneration, long unstableGeneration )
		 {
			  long leftSibling = TreeNode.LeftSibling( cursor, stableGeneration, unstableGeneration );
			  PointerChecking.CheckPointer( leftSibling, true );
			  long rightSibling = TreeNode.RightSibling( cursor, stableGeneration, unstableGeneration );
			  PointerChecking.CheckPointer( rightSibling, true );

			  if ( TreeNode.IsNode( leftSibling ) )
			  {
					// Go to left sibling and read stuff
					using ( PageCursor leftSiblingCursor = cursor.OpenLinkedCursor( GenerationSafePointerPair.Pointer( leftSibling ) ) )
					{
						 leftSiblingCursor.Next();
						 int leftSiblingKeyCount = TreeNode.KeyCount( leftSiblingCursor );

						 int keysToRebalance = _bTreeNode.canRebalanceLeaves( leftSiblingCursor, leftSiblingKeyCount, cursor, keyCount );
						 if ( keysToRebalance > 0 )
						 {
							  CreateSuccessorIfNeeded( leftSiblingCursor, structurePropagation, UPDATE_LEFT_CHILD, stableGeneration, unstableGeneration );
							  RebalanceLeaf( leftSiblingCursor, leftSiblingKeyCount, cursor, keyCount, keysToRebalance, structurePropagation );
						 }
						 else if ( keysToRebalance == -1 )
						 {
							  // No need to create new unstable version of left sibling.
							  // Parent pointer will be updated later.
							  MergeFromLeftSiblingLeaf( cursor, leftSiblingCursor, structurePropagation, keyCount, leftSiblingKeyCount, stableGeneration, unstableGeneration );
						 }
					}
			  }
			  else if ( TreeNode.IsNode( rightSibling ) )
			  {
					using ( PageCursor rightSiblingCursor = cursor.OpenLinkedCursor( GenerationSafePointerPair.Pointer( rightSibling ) ) )
					{
						 rightSiblingCursor.Next();
						 int rightSiblingKeyCount = TreeNode.KeyCount( rightSiblingCursor );

						 if ( _bTreeNode.canMergeLeaves( cursor, keyCount, rightSiblingCursor, rightSiblingKeyCount ) )
						 {
							  CreateSuccessorIfNeeded( rightSiblingCursor, structurePropagation, UPDATE_RIGHT_CHILD, stableGeneration, unstableGeneration );
							  MergeToRightSiblingLeaf( cursor, rightSiblingCursor, structurePropagation, keyCount, rightSiblingKeyCount, stableGeneration, unstableGeneration );
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void connectLeftAndRightSibling(org.Neo4Net.io.pagecache.PageCursor cursor, long stableGeneration, long unstableGeneration) throws java.io.IOException
		 private void ConnectLeftAndRightSibling( PageCursor cursor, long stableGeneration, long unstableGeneration )
		 {
			  long currentId = cursor.CurrentPageId;
			  long leftSibling = TreeNode.LeftSibling( cursor, stableGeneration, unstableGeneration );
			  PointerChecking.CheckPointer( leftSibling, true );
			  long rightSibling = TreeNode.RightSibling( cursor, stableGeneration, unstableGeneration );
			  PointerChecking.CheckPointer( rightSibling, true );
			  if ( TreeNode.IsNode( leftSibling ) )
			  {
					TreeNode.GoTo( cursor, "left sibling", leftSibling );
					TreeNode.SetRightSibling( cursor, rightSibling, stableGeneration, unstableGeneration );
			  }
			  if ( TreeNode.IsNode( rightSibling ) )
			  {
					TreeNode.GoTo( cursor, "right sibling", rightSibling );
					TreeNode.SetLeftSibling( cursor, leftSibling, stableGeneration, unstableGeneration );
			  }

			  TreeNode.GoTo( cursor, "back to origin after repointing siblings", currentId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void mergeToRightSiblingLeaf(org.Neo4Net.io.pagecache.PageCursor cursor, org.Neo4Net.io.pagecache.PageCursor rightSiblingCursor, StructurePropagation<KEY> structurePropagation, int keyCount, int rightSiblingKeyCount, long stableGeneration, long unstableGeneration) throws java.io.IOException
		 private void MergeToRightSiblingLeaf( PageCursor cursor, PageCursor rightSiblingCursor, StructurePropagation<KEY> structurePropagation, int keyCount, int rightSiblingKeyCount, long stableGeneration, long unstableGeneration )
		 {
			  // Read the right-most key from the right sibling to use when comparing whether or not
			  // a common parent covers the keys in right sibling too
			  _bTreeNode.keyAt( rightSiblingCursor, structurePropagation.RightKey, rightSiblingKeyCount - 1, LEAF );
			  Merge( cursor, keyCount, rightSiblingCursor, rightSiblingKeyCount, stableGeneration, unstableGeneration );

			  // Propagate change
			  // mid child has been merged into right child
			  // right key was separator key
			  structurePropagation.HasMidChildUpdate = true;
			  structurePropagation.MidChild = rightSiblingCursor.CurrentPageId;
			  structurePropagation.HasRightKeyReplace = true;
			  structurePropagation.KeyReplaceStrategy = BUBBLE;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void mergeFromLeftSiblingLeaf(org.Neo4Net.io.pagecache.PageCursor cursor, org.Neo4Net.io.pagecache.PageCursor leftSiblingCursor, StructurePropagation<KEY> structurePropagation, int keyCount, int leftSiblingKeyCount, long stableGeneration, long unstableGeneration) throws java.io.IOException
		 private void MergeFromLeftSiblingLeaf( PageCursor cursor, PageCursor leftSiblingCursor, StructurePropagation<KEY> structurePropagation, int keyCount, int leftSiblingKeyCount, long stableGeneration, long unstableGeneration )
		 {
			  // Read the left-most key from the left sibling to use when comparing whether or not
			  // a common parent covers the keys in left sibling too
			  _bTreeNode.keyAt( leftSiblingCursor, structurePropagation.LeftKey, 0, LEAF );
			  Merge( leftSiblingCursor, leftSiblingKeyCount, cursor, keyCount, stableGeneration, unstableGeneration );

			  // Propagate change
			  // left child has been merged into mid child
			  // left key was separator key
			  structurePropagation.HasLeftChildUpdate = true;
			  structurePropagation.LeftChild = cursor.CurrentPageId;
			  structurePropagation.HasLeftKeyReplace = true;
			  structurePropagation.KeyReplaceStrategy = BUBBLE;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void merge(org.Neo4Net.io.pagecache.PageCursor leftSiblingCursor, int leftSiblingKeyCount, org.Neo4Net.io.pagecache.PageCursor rightSiblingCursor, int rightSiblingKeyCount, long stableGeneration, long unstableGeneration) throws java.io.IOException
		 private void Merge( PageCursor leftSiblingCursor, int leftSiblingKeyCount, PageCursor rightSiblingCursor, int rightSiblingKeyCount, long stableGeneration, long unstableGeneration )
		 {
			  _bTreeNode.copyKeyValuesFromLeftToRight( leftSiblingCursor, leftSiblingKeyCount, rightSiblingCursor, rightSiblingKeyCount );

			  // Update successor of left sibling to be right sibling
			  TreeNode.SetSuccessor( leftSiblingCursor, rightSiblingCursor.CurrentPageId, stableGeneration, unstableGeneration );

			  // Add left sibling to free list
			  ConnectLeftAndRightSibling( leftSiblingCursor, stableGeneration, unstableGeneration );
			  _idProvider.releaseId( stableGeneration, unstableGeneration, leftSiblingCursor.CurrentPageId );
		 }

		 private void RebalanceLeaf( PageCursor leftCursor, int leftKeyCount, PageCursor rightCursor, int rightKeyCount, int numberOfKeysToMove, StructurePropagation<KEY> structurePropagation )
		 {
			  _bTreeNode.moveKeyValuesFromLeftToRight( leftCursor, leftKeyCount, rightCursor, rightKeyCount, leftKeyCount - numberOfKeysToMove );

			  // Propagate change
			  structurePropagation.HasLeftKeyReplace = true;
			  structurePropagation.KeyReplaceStrategy = REPLACE;
			  _bTreeNode.keyAt( rightCursor, structurePropagation.LeftKey, 0, LEAF );
		 }

		 /// <summary>
		 /// Remove key and value on given position and decrement key count. Deleted value is stored in {@code into}.
		 /// Key count after remove is returned.
		 /// </summary>
		 /// <param name="cursor"> Cursor pinned to node in which to remove from, </param>
		 /// <param name="into"> VALUE in which to store removed value </param>
		 /// <param name="keyCount"> Key count of node before remove </param>
		 /// <param name="pos"> Position to remove from </param>
		 /// <returns> keyCount after remove </returns>
		 private int SimplyRemoveFromLeaf( PageCursor cursor, VALUE into, int keyCount, int pos )
		 {
			  // Save value to remove
			  _bTreeNode.valueAt( cursor, into, pos );
			  // Remove key/value
			  _bTreeNode.removeKeyValueAt( cursor, pos, keyCount );

			  // Decrease key count
			  int newKeyCount = keyCount - 1;
			  TreeNode.SetKeyCount( cursor, newKeyCount );
			  return newKeyCount;
		 }

		 /// <summary>
		 /// Create a new node and copy content from current node (where {@code cursor} sits) if current node is not already
		 /// of {@code unstableGeneration}.
		 /// <para>
		 /// Neighboring nodes' sibling pointers will be updated to point to new node.
		 /// </para>
		 /// <para>
		 /// Current node will be updated with successor pointer to new node.
		 /// </para>
		 /// <para>
		 /// {@code structurePropagation} will be updated with information about this new node so that it can report to
		 /// level above.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="cursor"> <seealso cref="PageCursor"/> pinned to page containing node to potentially create a new version of </param>
		 /// <param name="structurePropagation"> <seealso cref="StructurePropagation"/> used to report structure changes between tree levels. </param>
		 /// <param name="structureUpdate"> <seealso cref="StructurePropagation.StructureUpdate"/> define how to update structurePropagation
		 /// if new unstable version is created </param>
		 /// <param name="stableGeneration"> stable generation, i.e. generations <= this generation are considered stable. </param>
		 /// <param name="unstableGeneration"> unstable generation, i.e. generation which is under development right now. </param>
		 /// <exception cref="IOException"> on cursor failure </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createSuccessorIfNeeded(org.Neo4Net.io.pagecache.PageCursor cursor, StructurePropagation<KEY> structurePropagation, StructurePropagation.StructureUpdate structureUpdate, long stableGeneration, long unstableGeneration) throws java.io.IOException
		 private void CreateSuccessorIfNeeded( PageCursor cursor, StructurePropagation<KEY> structurePropagation, StructurePropagation.StructureUpdate structureUpdate, long stableGeneration, long unstableGeneration )
		 {
			  long oldId = cursor.CurrentPageId;
			  long nodeGeneration = TreeNode.Generation( cursor );
			  if ( nodeGeneration == unstableGeneration )
			  {
					// Don't copy
					return;
			  }

			  // Do copy
			  long successorId = _idProvider.acquireNewId( stableGeneration, unstableGeneration );
			  using ( PageCursor successorCursor = cursor.OpenLinkedCursor( successorId ) )
			  {
					TreeNode.GoTo( successorCursor, "successor", successorId );
					cursor.CopyTo( 0, successorCursor, 0, cursor.CurrentPageSize );
					TreeNode.SetGeneration( successorCursor, unstableGeneration );
					TreeNode.SetSuccessor( successorCursor, TreeNode.NO_NODE_FLAG, stableGeneration, unstableGeneration );
			  }

			  // Insert successor pointer in old stable version
			  //   (stableNode)
			  //        |
			  //     [successor]
			  //        |
			  //        v
			  // (newUnstableNode)
			  TreeNode.SetSuccessor( cursor, successorId, stableGeneration, unstableGeneration );

			  // Redirect sibling pointers
			  //               ---------[leftSibling]---------(stableNode)----------[rightSibling]---------
			  //              |                                     |                                      |
			  //              |                                  [successor]                                    |
			  //              |                                     |                                      |
			  //              v                                     v                                      v
			  // (leftSiblingOfStableNode) -[rightSibling]-> (newUnstableNode) <-[leftSibling]- (rightSiblingOfStableNode)
			  long leftSibling = TreeNode.LeftSibling( cursor, stableGeneration, unstableGeneration );
			  PointerChecking.CheckPointer( leftSibling, true );
			  long rightSibling = TreeNode.RightSibling( cursor, stableGeneration, unstableGeneration );
			  PointerChecking.CheckPointer( rightSibling, true );
			  if ( TreeNode.IsNode( leftSibling ) )
			  {
					TreeNode.GoTo( cursor, "left sibling in split", leftSibling );
					TreeNode.SetRightSibling( cursor, successorId, stableGeneration, unstableGeneration );
			  }
			  if ( TreeNode.IsNode( rightSibling ) )
			  {
					TreeNode.GoTo( cursor, "right sibling in split", rightSibling );
					TreeNode.SetLeftSibling( cursor, successorId, stableGeneration, unstableGeneration );
			  }

			  // Leave cursor at new tree node
			  TreeNode.GoTo( cursor, "successor", successorId );

			  // Propagate structure change
			  structureUpdate.Update( structurePropagation, successorId );

			  _idProvider.releaseId( stableGeneration, unstableGeneration, oldId );
		 }
	}

}