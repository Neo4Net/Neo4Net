using System.Collections;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Index.@internal.gbptree
{
	using MutableList = org.eclipse.collections.api.list.MutableList;
	using MutableLongList = org.eclipse.collections.api.list.primitive.MutableLongList;
	using Lists = org.eclipse.collections.impl.factory.Lists;
	using LongLists = org.eclipse.collections.impl.factory.primitive.LongLists;


	using CursorException = Org.Neo4j.Io.pagecache.CursorException;
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.GenerationSafePointerPair.pointer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.PageCursorUtil.checkOutOfBounds;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.TreeNode.Type.INTERNAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.TreeNode.Type.LEAF;

	/// <summary>
	/// <ul>
	/// Checks:
	/// <li>order of keys in isolated nodes
	/// <li>keys fit inside range given by parent node
	/// <li>sibling pointers match
	/// <li>GSPP
	/// </ul>
	/// </summary>
	internal class GBPTreeConsistencyChecker<KEY>
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final TreeNode<KEY,?> node;
		 private readonly TreeNode<KEY, ?> _node;
		 private readonly IComparer<KEY> _comparator;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final Layout<KEY,?> layout;
		 private readonly Layout<KEY, ?> _layout;
		 private readonly IList<RightmostInChain> _rightmostPerLevel = new List<RightmostInChain>();
		 private readonly IdProvider _idProvider;
		 private readonly long _lastId;
		 private readonly long _stableGeneration;
		 private readonly long _unstableGeneration;
		 private readonly GenerationKeeper _generationTarget = new GenerationKeeper();

		 internal GBPTreeConsistencyChecker<T1, T2>( TreeNode<T1> node, Layout<T2> layout, IdProvider idProvider, long stableGeneration, long unstableGeneration )
		 {
			  this._node = node;
			  this._comparator = node.KeyComparator();
			  this._layout = layout;
			  this._idProvider = idProvider;
			  this._lastId = idProvider.LastId();
			  this._stableGeneration = stableGeneration;
			  this._unstableGeneration = unstableGeneration;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void check(java.io.File file, org.neo4j.io.pagecache.PageCursor cursor, Root root, GBPTreeConsistencyCheckVisitor<KEY> visitor) throws java.io.IOException
		 public virtual void Check( File file, PageCursor cursor, Root root, GBPTreeConsistencyCheckVisitor<KEY> visitor )
		 {
			  long highId = _lastId + 1;
			  BitArray seenIds = new BitArray( Math.toIntExact( highId ) );

			  // Log ids in freelist together with ids occupied by freelist pages.
			  IdProvider_IdProviderVisitor freelistSeenIdsVisitor = new FreelistSeenIdsVisitor<>( file, seenIds, _lastId, visitor );
			  _idProvider.visitFreelist( freelistSeenIdsVisitor );

			  // Check structure of GBPTree
			  long rootGeneration = root.GoTo( cursor );
			  KeyRange<KEY> openRange = new KeyRange<KEY>( -1, -1, _comparator, null, null, _layout, null );
			  CheckSubtree( file, cursor, openRange, -1, rootGeneration, GBPTreePointerType.noPointer(), 0, visitor, seenIds );

			  // Assert that rightmost node on each level has empty right sibling.
			  _rightmostPerLevel.ForEach( rightmost => rightmost.assertLast( visitor ) );

			  // Assert that all pages in file are either present as an active tree node or in freelist.
			  AssertAllIdsOccupied( file, highId, seenIds, visitor );
			  root.GoTo( cursor );
		 }

		 private static void AssertAllIdsOccupied<KEY>( File file, long highId, BitArray seenIds, GBPTreeConsistencyCheckVisitor<KEY> visitor )
		 {
			  long expectedNumberOfPages = highId - IdSpace.MIN_TREE_NODE_ID;
			  if ( seenIds.cardinality() != expectedNumberOfPages )
			  {
					int index = ( int ) IdSpace.MIN_TREE_NODE_ID;
					while ( index >= 0 && index < highId )
					{
						 index = seenIds.nextClearBit( index );
						 if ( index != -1 && index < highId )
						 {
							  visitor.UnusedPage( index, file );
						 }
						 index++;
					}
			  }
		 }

		 private static void AddToSeenList<KEY>( File file, BitArray target, long id, long lastId, GBPTreeConsistencyCheckVisitor<KEY> visitor )
		 {
			  int index = toIntExact( id );
			  if ( target.Get( index ) )
			  {
					visitor.PageIdSeenMultipleTimes( id, file );
			  }
			  if ( id > lastId )
			  {
					visitor.PageIdExceedLastId( lastId, id, file );
			  }
			  target.Set( index, true );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void checkSubtree(java.io.File file, org.neo4j.io.pagecache.PageCursor cursor, KeyRange<KEY> range, long parentNode, long pointerGeneration, GBPTreePointerType parentPointerType, int level, GBPTreeConsistencyCheckVisitor<KEY> visitor, java.util.BitSet seenIds) throws java.io.IOException
		 private void CheckSubtree( File file, PageCursor cursor, KeyRange<KEY> range, long parentNode, long pointerGeneration, GBPTreePointerType parentPointerType, int level, GBPTreeConsistencyCheckVisitor<KEY> visitor, BitArray seenIds )
		 {
			  long pageId = cursor.CurrentPageId;
			  AddToSeenList( file, seenIds, pageId, _lastId, visitor );
			  if ( range.HasPageIdInStack( pageId ) )
			  {
					visitor.ChildNodeFoundAmongParentNodes( range, level, pageId, file );
					return;
			  }
			  sbyte nodeType;
			  sbyte treeNodeType;
			  int keyCount;
			  long successor;

			  long leftSiblingPointer;
			  long rightSiblingPointer;
			  long leftSiblingPointerGeneration;
			  long rightSiblingPointerGeneration;
			  long currentNodeGeneration;

			  do
			  {
					// for assertSiblings
					leftSiblingPointer = TreeNode.LeftSibling( cursor, _stableGeneration, _unstableGeneration, _generationTarget );
					leftSiblingPointerGeneration = _generationTarget.generation;
					rightSiblingPointer = TreeNode.RightSibling( cursor, _stableGeneration, _unstableGeneration, _generationTarget );
					rightSiblingPointerGeneration = _generationTarget.generation;
					leftSiblingPointer = pointer( leftSiblingPointer );
					rightSiblingPointer = pointer( rightSiblingPointer );
					currentNodeGeneration = TreeNode.Generation( cursor );

					successor = TreeNode.Successor( cursor, _stableGeneration, _unstableGeneration, _generationTarget );

					keyCount = TreeNode.KeyCount( cursor );
					nodeType = TreeNode.NodeType( cursor );
					treeNodeType = TreeNode.TreeNodeType( cursor );
			  } while ( cursor.ShouldRetry() );
			  CheckAfterShouldRetry( cursor );

			  if ( nodeType != TreeNode.NODE_TYPE_TREE_NODE )
			  {
					visitor.NotATreeNode( pageId, file );
					return;
			  }

			  bool isLeaf = treeNodeType == TreeNode.LEAF_FLAG;
			  bool isInternal = treeNodeType == TreeNode.INTERNAL_FLAG;
			  if ( !isInternal && !isLeaf )
			  {
					visitor.UnknownTreeNodeType( pageId, treeNodeType, file );
					return;
			  }

			  // check header pointers
			  AssertNoCrashOrBrokenPointerInGSPP( file, cursor, _stableGeneration, _unstableGeneration, GBPTreePointerType.leftSibling(), TreeNode.BytePosLeftsibling, visitor );
			  AssertNoCrashOrBrokenPointerInGSPP( file, cursor, _stableGeneration, _unstableGeneration, GBPTreePointerType.rightSibling(), TreeNode.BytePosRightsibling, visitor );
			  AssertNoCrashOrBrokenPointerInGSPP( file, cursor, _stableGeneration, _unstableGeneration, GBPTreePointerType.successor(), TreeNode.BytePosSuccessor, visitor );

			  bool reasonableKeyCount = _node.reasonableKeyCount( keyCount );
			  if ( !reasonableKeyCount )
			  {
					visitor.UnreasonableKeyCount( pageId, keyCount, file );
			  }
			  else
			  {
					AssertKeyOrder( file, cursor, range, keyCount, isLeaf ? LEAF : INTERNAL, visitor );
			  }

			  string nodeMetaReport;
			  bool consistentNodeMeta;
			  do
			  {
					nodeMetaReport = _node.checkMetaConsistency( cursor, keyCount, isLeaf ? LEAF : INTERNAL, visitor );
					consistentNodeMeta = nodeMetaReport.Length == 0;
			  } while ( cursor.ShouldRetry() );
			  CheckAfterShouldRetry( cursor );
			  if ( !consistentNodeMeta )
			  {
					visitor.NodeMetaInconsistency( pageId, nodeMetaReport, file );
			  }

			  AssertPointerGenerationMatchesGeneration( file, parentPointerType, parentNode, pageId, pointerGeneration, currentNodeGeneration, visitor );
			  AssertSiblings( file, cursor, currentNodeGeneration, leftSiblingPointer, leftSiblingPointerGeneration, rightSiblingPointer, rightSiblingPointerGeneration, level, visitor );
			  CheckSuccessorPointerGeneration( file, cursor, successor, visitor );

			  if ( isInternal && reasonableKeyCount && consistentNodeMeta )
			  {
					AssertSubtrees( file, cursor, range, keyCount, level, visitor, seenIds );
			  }
		 }

		 private static void AssertPointerGenerationMatchesGeneration<KEY>( File file, GBPTreePointerType pointerType, long sourceNode, long pointer, long pointerGeneration, long targetNodeGeneration, GBPTreeConsistencyCheckVisitor<KEY> visitor )
		 {
			  if ( targetNodeGeneration > pointerGeneration )
			  {
					visitor.PointerHasLowerGenerationThanNode( pointerType, sourceNode, pointerGeneration, pointer, targetNodeGeneration, file );
			  }
		 }

		 private void CheckSuccessorPointerGeneration( File file, PageCursor cursor, long successor, GBPTreeConsistencyCheckVisitor<KEY> visitor )
		 {
			  if ( TreeNode.IsNode( successor ) )
			  {
					visitor.PointerToOldVersionOfTreeNode( cursor.CurrentPageId, pointer( successor ), file );
			  }
		 }

		 // Assumption: We traverse the tree from left to right on every level
		 private void AssertSiblings( File file, PageCursor cursor, long currentNodeGeneration, long leftSiblingPointer, long leftSiblingPointerGeneration, long rightSiblingPointer, long rightSiblingPointerGeneration, int level, GBPTreeConsistencyCheckVisitor<KEY> visitor )
		 {
			  // If this is the first time on this level, we will add a new entry
			  for ( int i = _rightmostPerLevel.Count; i <= level; i++ )
			  {
					_rightmostPerLevel.Insert( i, new RightmostInChain( file ) );
			  }
			  RightmostInChain rightmost = _rightmostPerLevel[level];

			  rightmost.AssertNext( cursor, currentNodeGeneration, leftSiblingPointer, leftSiblingPointerGeneration, rightSiblingPointer, rightSiblingPointerGeneration, visitor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertSubtrees(java.io.File file, org.neo4j.io.pagecache.PageCursor cursor, KeyRange<KEY> range, int keyCount, int level, GBPTreeConsistencyCheckVisitor<KEY> visitor, java.util.BitSet seenIds) throws java.io.IOException
		 private void AssertSubtrees( File file, PageCursor cursor, KeyRange<KEY> range, int keyCount, int level, GBPTreeConsistencyCheckVisitor<KEY> visitor, BitArray seenIds )
		 {
			  long pageId = cursor.CurrentPageId;
			  KEY prev = default( KEY );
			  KeyRange<KEY> childRange;
			  KEY readKey = _layout.newKey();

			  // Check children, all except the last one
			  int pos = 0;
			  while ( pos < keyCount )
			  {
					long child;
					long childGeneration;
					AssertNoCrashOrBrokenPointerInGSPP( file, cursor, _stableGeneration, _unstableGeneration, GBPTreePointerType.child( pos ), _node.childOffset( pos ), visitor );
					do
					{
						 child = ChildAt( cursor, pos, _generationTarget );
						 childGeneration = _generationTarget.generation;
						 _node.keyAt( cursor, readKey, pos, INTERNAL );
					} while ( cursor.ShouldRetry() );
					CheckAfterShouldRetry( cursor );

					childRange = range.NewSubRange( level, pageId ).restrictRight( readKey );
					if ( pos > 0 )
					{
						 childRange = childRange.RestrictLeft( prev );
					}

					TreeNode.GoTo( cursor, "child at pos " + pos, child );
					CheckSubtree( file, cursor, childRange, pageId, childGeneration, GBPTreePointerType.child( pos ), level + 1, visitor, seenIds );

					TreeNode.GoTo( cursor, "parent", pageId );

					if ( pos == 0 )
					{
						 prev = _layout.newKey();
					}
					_layout.copyKey( readKey, prev );
					pos++;
			  }

			  // Check last child
			  long child;
			  long childGeneration;
			  AssertNoCrashOrBrokenPointerInGSPP( file, cursor, _stableGeneration, _unstableGeneration, GBPTreePointerType.child( pos ), _node.childOffset( pos ), visitor );
			  do
			  {
					child = ChildAt( cursor, pos, _generationTarget );
					childGeneration = _generationTarget.generation;
			  } while ( cursor.ShouldRetry() );
			  CheckAfterShouldRetry( cursor );

			  TreeNode.GoTo( cursor, "child at pos " + pos, child );
			  childRange = range.NewSubRange( level, pageId ).restrictLeft( prev );
			  CheckSubtree( file, cursor, childRange, pageId, childGeneration, GBPTreePointerType.child( pos ), level + 1, visitor, seenIds );
			  TreeNode.GoTo( cursor, "parent", pageId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void checkAfterShouldRetry(org.neo4j.io.pagecache.PageCursor cursor) throws org.neo4j.io.pagecache.CursorException
		 private static void CheckAfterShouldRetry( PageCursor cursor )
		 {
			  checkOutOfBounds( cursor );
			  cursor.CheckAndClearCursorException();
		 }

		 private long ChildAt( PageCursor cursor, int pos, GBPTreeGenerationTarget childGeneration )
		 {
			  return _node.childAt( cursor, pos, _stableGeneration, _unstableGeneration, childGeneration );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertKeyOrder(java.io.File file, org.neo4j.io.pagecache.PageCursor cursor, KeyRange<KEY> range, int keyCount, TreeNode.Type type, GBPTreeConsistencyCheckVisitor<KEY> visitor) throws java.io.IOException
		 private void AssertKeyOrder( File file, PageCursor cursor, KeyRange<KEY> range, int keyCount, TreeNode.Type type, GBPTreeConsistencyCheckVisitor<KEY> visitor )
		 {
			  DelayedVisitor<KEY> delayedVisitor = new DelayedVisitor<KEY>( file );
			  do
			  {
					delayedVisitor.Clear();
					KEY prev = _layout.newKey();
					KEY readKey = _layout.newKey();
					bool first = true;
					for ( int pos = 0; pos < keyCount; pos++ )
					{
						 _node.keyAt( cursor, readKey, pos, type );
						 if ( !range.InRange( readKey ) )
						 {
							  KEY keyCopy = _layout.newKey();
							  _layout.copyKey( readKey, keyCopy );
							  delayedVisitor.KeysLocatedInWrongNode( range, keyCopy, pos, keyCount, cursor.CurrentPageId, file );
						 }
						 if ( !first )
						 {
							  if ( _comparator.Compare( prev, readKey ) >= 0 )
							  {
									delayedVisitor.KeysOutOfOrderInNode( cursor.CurrentPageId, file );
							  }
						 }
						 else
						 {
							  first = false;
						 }
						 _layout.copyKey( readKey, prev );
					}
			  } while ( cursor.ShouldRetry() );
			  CheckAfterShouldRetry( cursor );
			  delayedVisitor.Report( visitor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static <KEY> void assertNoCrashOrBrokenPointerInGSPP(java.io.File file, org.neo4j.io.pagecache.PageCursor cursor, long stableGeneration, long unstableGeneration, GBPTreePointerType pointerType, int offset, GBPTreeConsistencyCheckVisitor<KEY> visitor) throws java.io.IOException
		 internal static void AssertNoCrashOrBrokenPointerInGSPP<KEY>( File file, PageCursor cursor, long stableGeneration, long unstableGeneration, GBPTreePointerType pointerType, int offset, GBPTreeConsistencyCheckVisitor<KEY> visitor )
		 {
			  long currentNodeId = cursor.CurrentPageId;

			  long generationA;
			  long readPointerA;
			  long pointerA;
			  short checksumA;
			  bool correctChecksumA;
			  sbyte stateA;

			  long generationB;
			  long readPointerB;
			  long pointerB;
			  short checksumB;
			  bool correctChecksumB;
			  sbyte stateB;
			  do
			  {
					cursor.Offset = offset;
					// A
					generationA = GenerationSafePointer.ReadGeneration( cursor );
					readPointerA = GenerationSafePointer.ReadPointer( cursor );
					pointerA = GenerationSafePointerPair.Pointer( readPointerA );
					checksumA = GenerationSafePointer.ReadChecksum( cursor );
					correctChecksumA = GenerationSafePointer.ChecksumOf( generationA, readPointerA ) == checksumA;
					stateA = GenerationSafePointerPair.PointerState( stableGeneration, unstableGeneration, generationA, readPointerA, correctChecksumA );

					// B
					generationB = GenerationSafePointer.ReadGeneration( cursor );
					readPointerB = GenerationSafePointer.ReadPointer( cursor );
					pointerB = GenerationSafePointerPair.Pointer( readPointerA );
					checksumB = GenerationSafePointer.ReadChecksum( cursor );
					correctChecksumB = GenerationSafePointer.ChecksumOf( generationB, readPointerB ) == checksumB;
					stateB = GenerationSafePointerPair.PointerState( stableGeneration, unstableGeneration, generationB, readPointerB, correctChecksumB );
			  } while ( cursor.ShouldRetry() );

			  if ( stateA == GenerationSafePointerPair.CRASH || stateB == GenerationSafePointerPair.CRASH )
			  {
					visitor.CrashedPointer( currentNodeId, pointerType, generationA, readPointerA, pointerA, stateA, generationB, readPointerB, pointerB, stateB, file );
			  }
			  if ( stateA == GenerationSafePointerPair.BROKEN || stateB == GenerationSafePointerPair.BROKEN )
			  {
					visitor.BrokenPointer( currentNodeId, pointerType, generationA, readPointerA, pointerA, stateA, generationB, readPointerB, pointerB, stateB, file );
			  }
		 }

		 private class DelayedVisitor<KEY> : GBPTreeConsistencyCheckVisitor_Adaptor<KEY>
		 {
			  internal readonly File File;
			  internal MutableLongList KeysOutOfOrder = LongLists.mutable.empty();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal MutableList<KeyInWrongNode<KEY>> KeysLocatedInWrongNodeConflict = Lists.mutable.empty();

			  internal DelayedVisitor( File file )
			  {
					this.File = file;
			  }

			  public override void KeysOutOfOrderInNode( long pageId, File file )
			  {
					KeysOutOfOrder.add( pageId );
			  }

			  public override void KeysLocatedInWrongNode( KeyRange<KEY> range, KEY key, int pos, int keyCount, long pageId, File file )
			  {
					KeysLocatedInWrongNodeConflict.add( new KeyInWrongNode<>( pageId, range, key, pos, keyCount ) );
			  }

			  internal virtual void Clear()
			  {
					KeysOutOfOrder.clear();
					KeysLocatedInWrongNodeConflict.clear();
			  }

			  internal virtual void Report( GBPTreeConsistencyCheckVisitor<KEY> visitor )
			  {
					if ( KeysOutOfOrder.notEmpty() )
					{
						 KeysOutOfOrder.forEach( pageId => visitor.keysOutOfOrderInNode( pageId, File ) );
					}
					if ( KeysLocatedInWrongNodeConflict.notEmpty() )
					{
						 KeysLocatedInWrongNodeConflict.forEach( keyInWrongNode => visitor.keysLocatedInWrongNode( keyInWrongNode.range, keyInWrongNode.key, keyInWrongNode.pos, keyInWrongNode.keyCount, keyInWrongNode.pageId, File ) );
					}
			  }

			  private class KeyInWrongNode<KEY>
			  {
					internal readonly long PageId;
					internal readonly KeyRange<KEY> Range;
					internal readonly KEY Key;
					internal readonly int Pos;
					internal readonly int KeyCount;

					internal KeyInWrongNode( long pageId, KeyRange<KEY> range, KEY key, int pos, int keyCount )
					{
						 this.PageId = pageId;
						 this.Range = range;
						 this.Key = key;
						 this.Pos = pos;
						 this.KeyCount = keyCount;
					}
			  }
		 }

		 private class FreelistSeenIdsVisitor<KEY> : IdProvider_IdProviderVisitor
		 {
			  internal readonly File File;
			  internal readonly BitArray SeenIds;
			  internal readonly long LastId;
			  internal readonly GBPTreeConsistencyCheckVisitor<KEY> Visitor;

			  internal FreelistSeenIdsVisitor( File file, BitArray seenIds, long lastId, GBPTreeConsistencyCheckVisitor<KEY> visitor )
			  {
					this.File = file;
					this.SeenIds = seenIds;
					this.LastId = lastId;
					this.Visitor = visitor;
			  }

			  public override void BeginFreelistPage( long pageId )
			  {
					AddToSeenList( File, SeenIds, pageId, LastId, Visitor );
			  }

			  public override void EndFreelistPage( long pageId )
			  {
			  }

			  public override void FreelistEntry( long pageId, long generation, int pos )
			  {
					AddToSeenList( File, SeenIds, pageId, LastId, Visitor );
			  }
		 }
	}

}