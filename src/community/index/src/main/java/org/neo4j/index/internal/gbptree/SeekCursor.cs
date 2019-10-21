using System;

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

	using Neo4Net.Cursors;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.PageCursorUtil.checkOutOfBounds;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.TreeNode.Type.INTERNAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.TreeNode.Type.LEAF;

	/// <summary>
	/// <seealso cref="RawCursor"/> over tree leaves, making keys/values accessible to user. Given a starting leaf
	/// and key range this cursor traverses each leaf and its right siblings as long as visited keys are within
	/// key range. Each visited key within the key range can be accessible using <seealso cref="get()"/>.
	/// The key/value instances provided by <seealso cref="Hit"/> instance are mutable and overwritten with new values
	/// for every call to <seealso cref="next()"/> so user cannot keep references to key/value instances, expecting them
	/// to keep their values intact.
	/// <para>
	/// Concurrent writes can happen in the visited nodes and tree structure may change. This implementation
	/// guards for that by re-reading if change happens underneath, but will not provide a consistent view of
	/// the data as it were when the seek starts, i.e. doesn't support MVCC-style.
	/// </para>
	/// <para>
	/// Seek can be performed forwards or backwards, returning hits in ascending or descending order respectively
	/// (as defined by <seealso cref="Layout.compare(object, object)"/>). Direction is decided on relation between
	/// <seealso cref="fromInclusive"/> and <seealso cref="toExclusive"/>. Backwards seek is expected to be slower than forwards seek
	/// because extra care needs to be taken to make sure no keys are skipped when keys are move to the right in the tree.
	/// See detailed documentation about difficult cases below.
	/// <pre>
	/// If fromInclusive <= toExclusive, then seek forwards, otherwise seek backwards
	/// </pre>
	/// </para>
	/// <para>
	/// Implementation note: there are assumptions that keys are unique in the tree.
	/// </para>
	/// <para>
	/// <strong>Note on backwards seek</strong>
	/// </para>
	/// <para>
	/// To allow for lock free concurrency control the GBPTree agree to only move keys between nodes in 'forward' direction,
	/// from left to right. This means when we do normal forward seek we never risk having keys moved 'passed' us and
	/// therefore we can always be sure that we never miss any keys when traversing the leaf nodes or end up in the middle
	/// of the range when traversing down the internal nodes. This assumption, of course, does not hold when seeking
	/// backwards.
	/// There are two complicated cases where we risk missing keys.
	/// Case 1 - Split in current node
	/// Case 2 - Split in next node while seeker is moving to that node
	/// </para>
	/// <para>
	/// <strong>Case 1 - Split in current node</strong>
	/// </para>
	/// <para>
	/// <em>Forward seek</em>
	/// </para>
	/// <para>
	/// Here, the seeker is seeking forward and has read K0, K1, K2 and is about to read K3.
	/// Then K4 is suddenly inserted and a split happens.
	/// Seeker will now wake up and find that he is now outside the range of (previously returned key, end of range).
	/// But he knows that he can continue to read forward until he hits the previously returned key, which is K2 and
	/// then continue to return the next key, K3.
	/// <pre>
	///     Seeker->
	///        v
	/// [K0 K1 K2 K3]
	/// 
	///     Seeker->
	///        v
	/// [K0 K1 __ __]<->[K2 K3 K4 __]
	/// </pre>
	/// <em>Backward seek</em>
	/// </para>
	/// <para>
	/// Here, the seeking is seeking backwards and has only read K3 and is about to read K2.
	/// Again, K4 is inserted, causing the same split as above.
	/// Seeker now wakes up and find that the next key he can return is K1. What he do not see is that K2 has been
	/// moved to the previous sibling and so, because he can not find the previously returned key, K3, in the current
	/// node and because the next to return, K1, is located to the far right in the current node he need to jump back to
	/// the previous sibling to find where to start again.
	/// <pre>
	///      <-Seeker
	///           v
	/// [K0 K1 K2 K3]
	/// 
	///      <-Seeker (WRONG)
	///           v
	/// [K0 K1 __ __]<->[K2 K3 K4 __]
	/// 
	///                <-Seeker (RIGHT)
	///                     v
	/// [K0 K1 __ __]<->[K2 K3 K4 __]
	/// </pre>
	/// </para>
	/// <para>
	/// <strong>Case 2 - Split in next node while seeker is moving to that node</strong>
	/// </para>
	/// <para>
	/// <em>Forward seek</em>
	/// </para>
	/// <para>
	/// Seeker has read K0, K1 on node 1  and has just moved to right sibling, node 2.
	/// Now, K6 is inserted and a split  happens in node 2, right sibling of node 1.
	/// Seeker wakes up and continues to read on node 2. Everything is fine.
	/// <pre>
	///                   Seeker->
	///                      v
	/// 1:[K0 K1 __ __]<->2:[K2 K3 K4 K5]
	/// 
	///                   Seeker->
	///                      v
	/// 1:[K0 K1 __ __]<->2:[K2 K3 __ __]<->3:[K4 K5 K6 __]
	/// </pre>
	/// <em>Backward seek</em>
	/// </para>
	/// <para>
	/// Seeker has read K4, K5 and has just moved to left sibling, node 1.
	/// Insert K6, split in node 2. Note that keys are move to node 3, passed our seeker.
	/// Seeker wakes up and see K1 as next key but misses K3 and K2.
	/// <pre>
	///       <--Seeker
	///             v
	/// 1:[K0 K1 K2 K3]<->2:[K4 K5 __ __]
	/// 
	///        <-Seeker
	///             v
	/// 1:[K0 K1 __ __]<->3:[K2 K3 K4 __]<->2:[K5 K6 __ __]
	/// </pre>
	/// To guard for this, seeker 'scout' next sibling before moving there and read first key that he expect to see, K3
	/// in this case. By using a linked cursor to 'scout' we create a consistent read over the node gap. If there us
	/// suddenly another key when he goes there he knows that he could have missed some keys and he needs to go back until
	/// he find the place where he left off, K4.
	/// </para>
	/// </summary>
	internal class SeekCursor<KEY, VALUE> : IRawCursor<Hit<KEY, VALUE>, IOException>, Hit<KEY, VALUE>
	{
		 internal const int DEFAULT_MAX_READ_AHEAD = 20;

		 /// <summary>
		 /// Cursor for reading from tree nodes and also will be moved around when following pointers.
		 /// </summary>
		 private readonly PageCursor _cursor;

		 /// <summary>
		 /// Key instances to use for reading keys from current node.
		 /// </summary>
		 private readonly KEY[] _mutableKeys;

		 /// <summary>
		 /// Value instances to use for reading values from current node.
		 /// </summary>
		 private readonly VALUE[] _mutableValues;

		 /// <summary>
		 /// Index into <seealso cref="mutableKeys"/>/<seealso cref="mutableValues"/>, i.e. which key/value to consider as result next.
		 /// </summary>
		 private int _cachedIndex;

		 /// <summary>
		 /// Number of keys/values read into <seealso cref="mutableKeys"/> and <seealso cref="mutableValues"/> from the most recently read batch.
		 /// </summary>
		 private int _cachedLength;

		 /// <summary>
		 /// Initially set to {@code false} after a <seealso cref="readAndValidateNextKeyValueBatch()"/> and will become {@code true}
		 /// as soon as coming across a key which is a result key. From that point on and until the next batch read,
		 /// having this a {@code true} will allow fewer comparisons to figure out whether or not a key is a result key.
		 /// </summary>
		 private bool _resultOnTrack;

		 /// <summary>
		 /// Provided when constructing the <seealso cref="SeekCursor"/>, marks the start (inclusive) of the key range to seek.
		 /// Comparison with <seealso cref="toExclusive"/> decide if seeking forwards or backwards.
		 /// </summary>
		 private readonly KEY _fromInclusive;

		 /// <summary>
		 /// Provided when constructing the <seealso cref="SeekCursor"/>, marks the end (exclusive) of the key range to seek.
		 /// Comparison with <seealso cref="fromInclusive"/> decide if seeking forwards or backwards.
		 /// </summary>
		 private readonly KEY _toExclusive;

		 /// <summary>
		 /// True if seeker is performing an exact match lookup, <seealso cref="toExclusive"/> will then be treated as inclusive.
		 /// </summary>
		 private readonly bool _exactMatch;

		 /// <summary>
		 /// <seealso cref="Layout"/> instance used to perform some functions around keys, like copying and comparing.
		 /// </summary>
		 private readonly Layout<KEY, VALUE> _layout;

		 /// <summary>
		 /// Logic for reading data from tree nodes.
		 /// </summary>
		 private readonly TreeNode<KEY, VALUE> _bTreeNode;

		 /// <summary>
		 /// Contains the highest returned key, i.e. from the last call to <seealso cref="next()"/> returning {@code true}.
		 /// </summary>
		 private readonly KEY _prevKey;

		 /// <summary>
		 /// Retrieves latest generation, only used when noticing that reading given a stale generation.
		 /// </summary>
		 private readonly System.Func<long> _generationSupplier;

		 /// <summary>
		 /// Retrieves latest root id and generation, moving the <seealso cref="PageCursor"/> to the root id and returning
		 /// the root generation. This is used when a query is re-traversing from the root, due to e.g. ending up
		 /// on a reused tree node and not knowing how to proceed from there.
		 /// </summary>
		 private readonly RootCatchup _rootCatchup;

		 /// <summary>
		 /// Whether or not some result has been found, i.e. if {@code true} if there have been no call to
		 /// <seealso cref="next()"/> returning {@code true}, otherwise {@code false}. If {@code false} then value in
		 /// <seealso cref="prevKey"/> can be used and trusted.
		 /// </summary>
		 private bool _first = true;

		 /// <summary>
		 /// Current stable generation from this seek cursor's POV. Can be refreshed using <seealso cref="generationSupplier"/>.
		 /// </summary>
		 private long _stableGeneration;

		 /// <summary>
		 /// Current stable generation from this seek cursor's POV. Can be refreshed using <seealso cref="generationSupplier"/>.
		 /// </summary>
		 private long _unstableGeneration;

		 // *** Data structures for the current b-tree node ***

		 /// <summary>
		 /// Position in current node, this is used when scanning the values of a leaf, each call to <seealso cref="next()"/>
		 /// incrementing this position and reading the next key/value.
		 /// </summary>
		 private int _pos;

		 /// <summary>
		 /// Number of keys in the current leaf, this value is cached and only re-read every time there's
		 /// a <seealso cref="PageCursor.shouldRetry() retry due to concurrent write"/>.
		 /// </summary>
		 private int _keyCount;

		 /// <summary>
		 /// Set if the position of the last returned key need to be found again.
		 /// </summary>
		 private bool _concurrentWriteHappened;

		 /// <summary>
		 /// <seealso cref="TreeNode.generation(PageCursor) generation"/> of the current leaf node, read every call to <seealso cref="next()"/>.
		 /// </summary>
		 private long _currentNodeGeneration;

		 /// <summary>
		 /// Generation of the pointer which was last followed, either a
		 /// <seealso cref="TreeNode.rightSibling(PageCursor, long, long) sibling"/> during scan or otherwise following
		 /// <seealso cref="TreeNode.successor(PageCursor, long, long) successor"/> or
		 /// <seealso cref="TreeNode.childAt(PageCursor, int, long, long) child"/>.
		 /// </summary>
		 private long _lastFollowedPointerGeneration;

		 /// <summary>
		 /// Cached <seealso cref="TreeNode.generation(PageCursor) generation"/> of the current leaf node, read every time a pointer
		 /// is followed to a new node. Used to ensure that a node hasn't been reused between two calls to <seealso cref="next()"/>.
		 /// </summary>
		 private long _expectedCurrentNodeGeneration;

		 /// <summary>
		 /// Decide if seeker is configured to seek forwards or backwards.
		 /// <para>
		 /// {@code true} if {@code layout.compare(fromInclusive, toExclusive) <= 0}, otherwise false.
		 /// </para>
		 /// </summary>
		 private readonly bool _seekForward;

		 /// <summary>
		 /// Add to <seealso cref="pos"/> to move this {@code SeekCursor} forward in the seek direction.
		 /// </summary>
		 private readonly int _stride;

		 /// <summary>
		 /// Set within should retry loop.
		 /// <para>
		 /// Is node a <seealso cref="TreeNode.NODE_TYPE_TREE_NODE"/> or something else?
		 /// </para>
		 /// </summary>

		 private sbyte _nodeType;
		 /// <summary>
		 /// Set within should retry loop.
		 /// <para>
		 /// Pointer to successor of node.
		 /// </para>
		 /// </summary>
		 private long _successor;

		 /// <summary>
		 /// Set within should retry loop.
		 /// <para>
		 /// Generation of successor pointer
		 /// </para>
		 /// </summary>
		 private long _successorGeneration;

		 /// <summary>
		 /// Set within should retry loop.
		 /// <para>
		 /// Is node internal or leaf?
		 /// </para>
		 /// </summary>
		 private bool _isInternal;

		 /// <summary>
		 /// Set within should retry loop.
		 /// <para>
		 /// Used to store next child pointer to follow while traversing down the tree
		 /// or next sibling pointer to follow if traversing along the leaves.
		 /// </para>
		 /// </summary>
		 private long _pointerId;

		 /// <summary>
		 /// Set within should retry loop.
		 /// <para>
		 /// Generation of <seealso cref="pointerId"/>.
		 /// </para>
		 /// </summary>
		 private long _pointerGeneration;

		 /// <summary>
		 /// Result from <seealso cref="KeySearch.search(PageCursor, TreeNode, TreeNode.Type, object, object, int)"/>.
		 /// </summary>
		 private int _searchResult;

		 // ┌── Special variables for backwards seek ──┐
		 // v                                          v

		 /// <summary>
		 /// Set within should retry loop.
		 /// <para>
		 /// Pointer to sibling opposite to seek direction. Only used when seeking backwards.
		 /// </para>
		 /// </summary>
		 private long _prevSiblingId;

		 /// <summary>
		 /// Set within should retry loop.
		 /// <para>
		 /// Generation of <seealso cref="prevSiblingId"/>.
		 /// </para>
		 /// </summary>
		 private long _prevSiblingGeneration;

		 /// <summary>
		 /// Set by linked cursor scouting next sibling to go to when seeking backwards.
		 /// If first key when reading from next sibling node is not equal to this we
		 /// may have missed some keys that was moved passed us and we need to start
		 /// over from previous node.
		 /// </summary>
		 private readonly KEY _expectedFirstAfterGoToNext;

		 /// <summary>
		 /// Key on pos 0 if traversing forward, pos {@code keyCount - 1} if traversing backwards.
		 /// To be compared with <seealso cref="expectedFirstAfterGoToNext"/>.
		 /// </summary>
		 private readonly KEY _firstKeyInNode;

		 /// <summary>
		 /// {@code true} to indicate that first key in node needs to be verified to ensure no keys
		 /// was moved passed us while we where changing nodes.
		 /// </summary>
		 private bool _verifyExpectedFirstAfterGoToNext;

		 /// <summary>
		 /// Whether or not this seeker have been closed.
		 /// </summary>
		 private bool _closed;

		 /// <summary>
		 /// Decorator for caught exceptions, adding information about which tree the exception relates to.
		 /// </summary>
		 private readonly System.Action<Exception> _exceptionDecorator;

		 /// <summary>
		 /// Normally <seealso cref="readHeader()"/> is called when <seealso cref="concurrentWriteHappened"/> is {@code true}. However this flag
		 /// guards for cases where the header must be read and <seealso cref="concurrentWriteHappened"/> is {@code false},
		 /// such as when moving over to the next sibling and continuing reading.
		 /// </summary>
		 private bool _forceReadHeader;

		 /// <summary>
		 /// Place where read generations will be kept when reading child/sibling/successor pointers.
		 /// </summary>
		 private readonly GenerationKeeper _generationKeeper = new GenerationKeeper();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") SeekCursor(org.neo4j.io.pagecache.PageCursor cursor, TreeNode<KEY,VALUE> bTreeNode, KEY fromInclusive, KEY toExclusive, Layout<KEY,VALUE> layout, long stableGeneration, long unstableGeneration, System.Func<long> generationSupplier, RootCatchup rootCatchup, long lastFollowedPointerGeneration, System.Action<Throwable> exceptionDecorator, int maxReadAhead) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal SeekCursor( PageCursor cursor, TreeNode<KEY, VALUE> bTreeNode, KEY fromInclusive, KEY toExclusive, Layout<KEY, VALUE> layout, long stableGeneration, long unstableGeneration, System.Func<long> generationSupplier, RootCatchup rootCatchup, long lastFollowedPointerGeneration, System.Action<Exception> exceptionDecorator, int maxReadAhead )
		 {
			  this._cursor = cursor;
			  this._fromInclusive = fromInclusive;
			  this._toExclusive = toExclusive;
			  this._layout = layout;
			  this._exceptionDecorator = exceptionDecorator;
			  this._exactMatch = layout.Compare( fromInclusive, toExclusive ) == 0;
			  this._stableGeneration = stableGeneration;
			  this._unstableGeneration = unstableGeneration;
			  this._generationSupplier = generationSupplier;
			  this._bTreeNode = bTreeNode;
			  this._rootCatchup = rootCatchup;
			  this._lastFollowedPointerGeneration = lastFollowedPointerGeneration;
			  int batchSize = _exactMatch ? 1 : maxReadAhead;
			  this._mutableKeys = ( KEY[] ) new object[batchSize];
			  this._mutableValues = ( VALUE[] ) new object[batchSize];
			  this._mutableKeys[0] = layout.NewKey();
			  this._mutableValues[0] = layout.NewValue();
			  this._prevKey = layout.NewKey();
			  this._seekForward = layout.Compare( fromInclusive, toExclusive ) <= 0;
			  this._stride = _seekForward ? 1 : -1;
			  this._expectedFirstAfterGoToNext = layout.NewKey();
			  this._firstKeyInNode = layout.NewKey();

			  try
			  {
					TraverseDownToFirstLeaf();
			  }
			  catch ( Exception e )
			  {
					exceptionDecorator( e );
					throw e;
			  }
		 }

		 /// <summary>
		 /// Traverses from the root down to the leaf containing the next key that we're looking for, or the first
		 /// one provided in the constructor if this no result have yet been returned.
		 /// <para>
		 /// This method is called when constructing the cursor, but also if this traversal itself or leaf scan
		 /// later on ends up on an unexpected tree node (typically due to concurrent changes,
		 /// checkpoint and tree node reuse).
		 /// </para>
		 /// <para>
		 /// Before calling this method the caller is expected to place the <seealso cref="PageCursor"/> at the root, by using
		 /// <seealso cref="rootCatchup"/>. After this method returns the <seealso cref="PageCursor"/> is placed on the leaf containing
		 /// the next result and <seealso cref="pos"/> is also initialized correctly.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <exception cref="IOException"> on <seealso cref="PageCursor"/> error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void traverseDownToFirstLeaf() throws java.io.IOException
		 private void TraverseDownToFirstLeaf()
		 {
			  do
			  {
					// Read
					do
					{
						 // Where we are
						 if ( !ReadHeader() )
						 {
							  continue;
						 }

						 _searchResult = SearchKey( _fromInclusive, _isInternal ? INTERNAL : LEAF );
						 if ( !KeySearch.IsSuccess( _searchResult ) )
						 {
							  continue;
						 }
						 _pos = PositionOf( _searchResult );

						 if ( _isInternal )
						 {
							  _pointerId = _bTreeNode.childAt( _cursor, _pos, _stableGeneration, _unstableGeneration, _generationKeeper );
							  _pointerGeneration = _generationKeeper.generation;
						 }
					} while ( _cursor.shouldRetry() );
					checkOutOfBounds( _cursor );
					_cursor.checkAndClearCursorException();

					// Act
					if ( !EndedUpOnExpectedNode() )
					{
						 PrepareToStartFromRoot();
						 _isInternal = true;
						 continue;
					}
					else if ( !SaneRead() )
					{
						 throw new TreeInconsistencyException( "Read inconsistent tree node %d%n" + "  nodeType:%d%n  currentNodeGeneration:%d%n  successor:%d%n  successorGeneration:%d%n" + "  isInternal:%b%n  keyCount:%d%n  searchResult:%d%n  pos:%d%n" + "  childId:%d%n  childIdGeneration:%d", _cursor.CurrentPageId, _nodeType, _currentNodeGeneration, _successor, _successorGeneration, _isInternal, _keyCount, _searchResult, _pos, _pointerId, _pointerGeneration );
					}

					if ( GoToSuccessor() )
					{
						 continue;
					}

					if ( _isInternal )
					{
						 GoTo( _pointerId, _pointerGeneration, "child", false );
					}
			  } while ( _isInternal );

			  // We've now come to the first relevant leaf, initialize the state for the coming leaf scan
			  _pos -= _stride;
			  if ( !_seekForward )
			  {
					// The tree traversal is best effort when seeking backwards
					// need to trigger search for key in next
					_concurrentWriteHappened = true;
			  }
			  _cachedLength = 0;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next() throws java.io.IOException
		 public override bool Next()
		 {
			  try
			  {
					while ( true )
					{
						 _pos += _stride;

						 // There are two main tracks in this loop:
						 // - (SLOW) no keys/values have been read and will therefore need to be read from the cursor.
						 //   Reading from the cursor means there are a lot of things around the actual keys and values
						 //   that need to be check to validate the read. This is expensive to do since there's so much
						 //   to validate. This is why keys/values are read in batches of N entries. The validations
						 //   are made only once per batch instead of once per key/value.
						 // - (FAST) there are keys/values read and validated and ready to simply be returned to the user.

						 if ( _cachedIndex + 1 < _cachedLength && !_closed && !( _concurrentWriteHappened = _cursor.shouldRetry() ) )
						 { // FAST, key/value is readily available
							  _cachedIndex++;
							  if ( 0 <= _pos && _pos < _keyCount )
							  {
									if ( _resultOnTrack || ResultKey )
									{
										 _resultOnTrack = true;
										 return true; // which marks this read a hit that user can see
									}
									continue;
							  }
						 }
						 else
						 { // SLOW, next batch of keys/values needs to be read
							  if ( _resultOnTrack )
							  {
									_layout.copyKey( _mutableKeys[_cachedIndex], _prevKey );
							  }
							  if ( !ReadAndValidateNextKeyValueBatch() )
							  {
									// Concurrent changes
									_cachedLength = 0;
									continue;
							  }

							  // Below, the cached key/value at slot [0] will be used
							  if ( !_seekForward && _pos >= _keyCount )
							  {
									GoTo( _prevSiblingId, _prevSiblingGeneration, "prev sibling", true );
									// Continue in the read loop above so that we can continue reading from previous sibling
									// or on next position
									continue;
							  }

							  if ( ( _seekForward && _pos >= _keyCount ) || ( !_seekForward && _pos <= 0 && !InsidePrevKey( _cachedIndex ) ) )
							  {
									if ( GoToNextSibling() )
									{
										 continue; // in the read loop above so that we can continue reading from next sibling
									}
							  }
							  else if ( 0 <= _pos && _pos < _keyCount && InsideEndRange( _exactMatch, 0 ) )
							  {
									if ( ResultKey )
									{
										 _resultOnTrack = true;
										 return true; // which marks this read a hit that user can see
									}
									continue;
							  }
						 }

						 // We've come too far and so this means the end of the result set
						 return false;
					}
			  }
			  catch ( Exception e )
			  {
					_exceptionDecorator.accept( e );
					throw e;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean readAndValidateNextKeyValueBatch() throws java.io.IOException
		 private bool ReadAndValidateNextKeyValueBatch()
		 {
			  do
			  {
					_cachedIndex = 0;
					_cachedLength = 0;
					_resultOnTrack = false;

					// Where we are
					if ( _concurrentWriteHappened || _forceReadHeader || !_seekForward )
					{
						 if ( !ReadHeader() || _isInternal )
						 {
							  continue;
						 }
					}

					if ( _verifyExpectedFirstAfterGoToNext )
					{
						 _pos = _seekForward ? 0 : _keyCount - 1;
						 _bTreeNode.keyAt( _cursor, _firstKeyInNode, _pos, LEAF );
					}

					if ( _concurrentWriteHappened )
					{
						 // Keys could have been moved so we need to make sure we are not missing any keys by
						 // moving position back until we find previously returned key
						 _searchResult = SearchKey( _first ? _fromInclusive : _prevKey, LEAF );
						 if ( !KeySearch.IsSuccess( _searchResult ) )
						 {
							  continue;
						 }
						 _pos = PositionOf( _searchResult );

						 if ( !_seekForward && _pos >= _keyCount )
						 {
							  // We may need to go to previous sibling to find correct place to start seeking from
							  _prevSiblingId = ReadPrevSibling();
							  _prevSiblingGeneration = _generationKeeper.generation;
						 }
					}

					// Next result
					if ( ( _seekForward && _pos >= _keyCount ) || ( !_seekForward && _pos <= 0 ) )
					{
						 // Read right sibling
						 _pointerId = ReadNextSibling();
						 _pointerGeneration = _generationKeeper.generation;
					}
					for ( int readPos = _pos; _cachedLength < _mutableKeys.Length && 0 <= readPos && readPos < _keyCount; readPos += _stride )
					{
						 // Read the next value in this leaf
						 if ( _mutableKeys[_cachedLength] == default( KEY ) )
						 {
							  // Lazy instantiation of key/value
							  _mutableKeys[_cachedLength] = _layout.newKey();
							  _mutableValues[_cachedLength] = _layout.newValue();
						 }
						 _bTreeNode.keyValueAt( _cursor, _mutableKeys[_cachedLength], _mutableValues[_cachedLength], readPos );

						 if ( InsideEndRange( _exactMatch, _cachedLength ) )
						 {
							  // This seems to be a result that should be part of our result set
							  if ( _cachedLength > 0 || InsidePrevKey( _cachedLength ) )
							  {
									_cachedLength++;
							  }
						 }
						 else
						 {
							  // OK so we read too far, abort this ahead-reading
							  break;
						 }
					}
			  } while ( _concurrentWriteHappened = _cursor.shouldRetry() );
			  CheckOutOfBoundsAndClosed();
			  _cursor.checkAndClearCursorException();

			  // Act
			  if ( !EndedUpOnExpectedNode() || _isInternal )
			  {
					// This node has been reused for something else than a tree node. Restart seek from root.
					PrepareToStartFromRoot();
					TraverseDownToFirstLeaf();
					return false;
			  }
			  else if ( !SaneRead() )
			  {
					throw new TreeInconsistencyException( "Read inconsistent tree node %d%n" + "  nodeType:%d%n  currentNodeGeneration:%d%n  successor:%d%n  successorGeneration:%d%n" + "  keyCount:%d%n  searchResult:%d%n  pos:%d%n" + "  rightSibling:%d%n  rightSiblingGeneration:%d", _cursor.CurrentPageId, _nodeType, _currentNodeGeneration, _successor, _successorGeneration, _keyCount, _searchResult, _pos, _pointerId, _pointerGeneration );
			  }

			  if ( !VerifyFirstKeyInNodeIsExpectedAfterGoTo() )
			  {
					return false;
			  }

			  if ( GoToSuccessor() )
			  {
					return false;
			  }

			  return true;
		 }

		 /// <summary>
		 /// Check out of bounds for cursor. If out of bounds, check if seeker has been closed and throw exception accordingly
		 /// </summary>
		 private void CheckOutOfBoundsAndClosed()
		 {
			  try
			  {
					checkOutOfBounds( _cursor );
			  }
			  catch ( TreeInconsistencyException e )
			  {
					// Only check the closed status here when we get an out of bounds to avoid making
					// this check for every call to next.
					if ( _closed )
					{
						 throw new System.InvalidOperationException( "Tried to use seeker after it was closed" );
					}
					throw e;
			  }
		 }

		 /// <returns> whether or not the read key (<seealso cref="mutableKeys"/>) is "before" the end of the key range
		 /// (<seealso cref="toExclusive"/>) of this seek. </returns>
		 private bool InsideEndRange( bool exactMatch, int cachedIndex )
		 {
			  if ( exactMatch )
			  {
					return _seekForward ? _layout.Compare( _mutableKeys[cachedIndex], _toExclusive ) <= 0 : _layout.Compare( _mutableKeys[cachedIndex], _toExclusive ) >= 0;
			  }
			  else
			  {
					return _seekForward ? _layout.Compare( _mutableKeys[cachedIndex], _toExclusive ) < 0 : _layout.Compare( _mutableKeys[cachedIndex], _toExclusive ) > 0;
			  }
		 }

		 /// <returns> whether or not the read key (<seealso cref="mutableKeys"/>) is "after" the start of the key range
		 /// (<seealso cref="fromInclusive"/>) of this seek. </returns>
		 private bool InsideStartRange( int cachedIndex )
		 {
			  return _seekForward ? _layout.Compare( _mutableKeys[cachedIndex], _fromInclusive ) >= 0 : _layout.Compare( _mutableKeys[cachedIndex], _fromInclusive ) <= 0;
		 }

		 /// <returns> whether or not the read key (<seealso cref="mutableKeys"/>) is "after" the last returned key of this seek
		 /// (<seealso cref="prevKey"/>), or if no result has been returned the start of the key range (<seealso cref="fromInclusive"/>). </returns>
		 private bool InsidePrevKey( int cachedIndex )
		 {
			  if ( _first )
			  {
					return InsideStartRange( cachedIndex );
			  }
			  return _seekForward ? _layout.Compare( _mutableKeys[cachedIndex], _prevKey ) > 0 : _layout.Compare( _mutableKeys[cachedIndex], _prevKey ) < 0;
		 }

		 /// <summary>
		 /// Tries to move the <seealso cref="PageCursor"/> to the tree node specified inside {@code pointerId},
		 /// also setting the pointer generation expectation on the next read on that new tree node.
		 /// <para>
		 /// As with all pointers, the generation is checked for sanity and if generation looks to be in the future,
		 /// there's a generation catch-up made and the read will have to be re-attempted.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="pointerId"> read result containing pointer id to go to. </param>
		 /// <param name="pointerGeneration"> generation of {@code pointerId}. </param>
		 /// <param name="type"> type of pointer, e.g. "child" or "sibling" or so. </param>
		 /// <returns> {@code true} if context was updated or <seealso cref="PageCursor"/> was moved, both cases meaning that
		 /// caller should retry its most recent read, otherwise {@code false} meaning that nothing happened. </returns>
		 /// <exception cref="IOException"> on <seealso cref="PageCursor"/> error. </exception>
		 /// <exception cref="TreeInconsistencyException"> if {@code allowNoNode} is {@code true} and {@code pointerId}
		 /// contains a "null" tree node id. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean goTo(long pointerId, long pointerGeneration, String type, boolean allowNoNode) throws java.io.IOException
		 private bool GoTo( long pointerId, long pointerGeneration, string type, bool allowNoNode )
		 {
			  if ( PointerCheckingWithGenerationCatchup( pointerId, allowNoNode ) )
			  {
					_concurrentWriteHappened = true;
					return true;
			  }
			  else if ( !allowNoNode || TreeNode.IsNode( pointerId ) )
			  {
					TreeNode.GoTo( _cursor, type, pointerId );
					_lastFollowedPointerGeneration = pointerGeneration;
					_concurrentWriteHappened = true;
					return true;
			  }
			  return false;
		 }

		 /// <summary>
		 /// Calls <seealso cref="goTo(long, long, string, bool)"/> with successor fields.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean goToSuccessor() throws java.io.IOException
		 private bool GoToSuccessor()
		 {
			  return GoTo( _successor, _successorGeneration, "successor", true );
		 }

		 /// <returns> {@code false} if there was a set expectancy on first key in tree node which weren't met,
		 /// otherwise {@code true}. Caller should </returns>
		 private bool VerifyFirstKeyInNodeIsExpectedAfterGoTo()
		 {
			  bool result = true;
			  if ( _verifyExpectedFirstAfterGoToNext && _layout.Compare( _firstKeyInNode, _expectedFirstAfterGoToNext ) != 0 )
			  {
					_concurrentWriteHappened = true;
					result = false;
			  }
			  _verifyExpectedFirstAfterGoToNext = false;
			  return result;
		 }

		 /// <returns> the read previous sibling, depending on the direction this seek is going. </returns>
		 private long ReadPrevSibling()
		 {
			  return _seekForward ? TreeNode.LeftSibling( _cursor, _stableGeneration, _unstableGeneration, _generationKeeper ) : TreeNode.RightSibling( _cursor, _stableGeneration, _unstableGeneration, _generationKeeper );
		 }

		 /// <returns> the read next sibling, depending on the direction this seek is going. </returns>
		 private long ReadNextSibling()
		 {
			  return _seekForward ? TreeNode.RightSibling( _cursor, _stableGeneration, _unstableGeneration, _generationKeeper ) : TreeNode.LeftSibling( _cursor, _stableGeneration, _unstableGeneration, _generationKeeper );
		 }

		 /// <summary>
		 /// Does a binary search for the given {@code key} in the current tree node and returns its position.
		 /// </summary>
		 /// <returns> position of the {@code key} in the current tree node, or position of the closest key. </returns>
		 private int SearchKey( KEY key, TreeNode.Type type )
		 {
			  return KeySearch.Search( _cursor, _bTreeNode, type, key, _mutableKeys[0], _keyCount );
		 }

		 private int PositionOf( int searchResult )
		 {
			  int pos = KeySearch.PositionOf( searchResult );

			  // Assuming unique keys
			  if ( _isInternal && KeySearch.IsHit( searchResult ) )
			  {
					pos++;
			  }
			  return pos;
		 }

		 /// <returns> {@code true} if header was read and looks sane, otherwise {@code false} meaning that node doesn't look
		 /// like a tree node or we can expect a shouldRetry to take place. </returns>
		 private bool ReadHeader()
		 {
			  _nodeType = TreeNode.NodeType( _cursor );
			  if ( _nodeType != TreeNode.NODE_TYPE_TREE_NODE )
			  {
					// If this node doesn't even look like a tree node then anything we read from it
					// will be just random data when looking at it as if it were a tree node.
					return false;
			  }

			  _currentNodeGeneration = TreeNode.Generation( _cursor );

			  _successor = TreeNode.Successor( _cursor, _stableGeneration, _unstableGeneration, _generationKeeper );
			  _successorGeneration = _generationKeeper.generation;
			  _isInternal = TreeNode.IsInternal( _cursor );
			  // Find the left-most key within from-range
			  _keyCount = TreeNode.KeyCount( _cursor );

			  _forceReadHeader = false;
			  return KeyCountIsSane( _keyCount );
		 }

		 private bool EndedUpOnExpectedNode()
		 {
			  return _nodeType == TreeNode.NODE_TYPE_TREE_NODE && VerifyNodeGenerationInvariants();
		 }

		 /// <returns> the key/value found from the most recent call to <seealso cref="next()"/> returning {@code true}. </returns>
		 /// <exception cref="IllegalStateException"> if no <seealso cref="next()"/> call which returned {@code true} has been made yet. </exception>
		 public override Hit<KEY, VALUE> Get()
		 {
			  if ( _first )
			  {
					throw new System.InvalidOperationException( "There has been no successful call to next() yet" );
			  }

			  return this;
		 }

		 /// <summary>
		 /// Moves <seealso cref="PageCursor"/> to next sibling (read before this call into <seealso cref="pointerId"/>).
		 /// Also, on backwards seek, calls <seealso cref="scoutNextSibling()"/> to be able to verify consistent read on
		 /// new sibling even on concurrent writes.
		 /// <para>
		 /// As with all pointers, the generation is checked for sanity and if generation looks to be in the future,
		 /// there's a generation catch-up made and the read will have to be re-attempted.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> {@code true} if we should read more after this call, otherwise {@code false} to mark the end. </returns>
		 /// <exception cref="IOException"> on <seealso cref="PageCursor"/> error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean goToNextSibling() throws java.io.IOException
		 private bool GoToNextSibling()
		 {
			  if ( PointerCheckingWithGenerationCatchup( _pointerId, true ) )
			  {
					// Reading sibling pointer resulted in a bad read, but generation had changed
					// (a checkpoint has occurred since we started this cursor) so the generation fields in this
					// cursor are now updated with the latest, so let's try that read again.
					_concurrentWriteHappened = true;
					return true;
			  }
			  else if ( TreeNode.IsNode( _pointerId ) )
			  {
					if ( _seekForward )
					{
						 // TODO: Check if rightSibling is within expected range before calling next.
						 // TODO: Possibly by getting highest expected from IdProvider
						 TreeNode.GoTo( _cursor, "sibling", _pointerId );
						 _lastFollowedPointerGeneration = _pointerGeneration;
						 if ( _first )
						 {
							  // Have not yet found first hit among leaves.
							  // First hit can be several leaves to the right.
							  // Continue to use binary search in right leaf
							  _concurrentWriteHappened = true;
						 }
						 else
						 {
							  // It is likely that first key in right sibling is a next hit.
							  // Continue using scan
							  _forceReadHeader = true;
							  _pos = -1;
						 }
						 return true;
					}
					else
					{
						 // Need to scout next sibling because we are seeking backwards
						 if ( ScoutNextSibling() )
						 {
							  TreeNode.GoTo( _cursor, "sibling", _pointerId );
							  _verifyExpectedFirstAfterGoToNext = true;
							  _lastFollowedPointerGeneration = _pointerGeneration;
						 }
						 else
						 {
							  _concurrentWriteHappened = true;
						 }
						 return true;
					}
			  }

			  // The current node is exhausted and it had no sibling to read more from.
			  return false;
		 }

		 /// <summary>
		 /// Reads first key on next sibling, without moving the main <seealso cref="PageCursor"/> to that sibling.
		 /// This to be able to guard for, and retry read if, concurrent writes moving keys in the "wrong" direction.
		 /// The first key read here will be matched after actually moving the main <seealso cref="PageCursor"/> to
		 /// the next sibling.
		 /// <para>
		 /// May only be called if <seealso cref="pointerId"/> points to next sibling.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> {@code true} if first key in next sibling was read successfully, otherwise {@code false},
		 /// which means that caller should retry most recent read. </returns>
		 /// <exception cref="IOException"> on <seealso cref="PageCursor"/> error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean scoutNextSibling() throws java.io.IOException
		 private bool ScoutNextSibling()
		 {
			  // Read header but to local variables and not global once
			  sbyte nodeType;
			  int keyCount = -1;
			  using ( PageCursor scout = this._cursor.openLinkedCursor( GenerationSafePointerPair.Pointer( _pointerId ) ) )
			  {
					scout.Next();
					nodeType = TreeNode.NodeType( scout );
					if ( nodeType == TreeNode.NODE_TYPE_TREE_NODE )
					{
						 keyCount = TreeNode.KeyCount( scout );
						 if ( KeyCountIsSane( keyCount ) )
						 {
							  int firstPos = _seekForward ? 0 : keyCount - 1;
							  _bTreeNode.keyAt( scout, _expectedFirstAfterGoToNext, firstPos, LEAF );
						 }
					}

					if ( this._cursor.shouldRetry() )
					{
						 // We scouted next sibling but either next sibling or current node has been changed
						 // since we left shouldRetry loop, this means keys could have been moved passed us
						 // and we need to start over.
						 // Because we also need to restart read on current node there is no use to loop
						 // on shouldRetry here.
						 return false;
					}
					checkOutOfBounds( this._cursor );
			  }
			  return !( nodeType != TreeNode.NODE_TYPE_TREE_NODE || !KeyCountIsSane( keyCount ) );
		 }

		 /// <returns> whether or not the read <seealso cref="mutableKeys"/> is one that should be included in the result.
		 /// If this method returns {@code true} then <seealso cref="next()"/> will return {@code true}.
		 /// Returns {@code false} if this happened to be a bad read in the middle of a split or merge or so. </returns>
		 private bool ResultKey
		 {
			 get
			 {
				  if ( !InsideStartRange( _cachedIndex ) )
				  {
						// Key is outside start range, possibly because page reuse
						_concurrentWriteHappened = true;
						return false;
				  }
				  else if ( !_first && !InsidePrevKey( _cachedIndex ) )
				  {
						// We've come across a bad read in the middle of a split
						// This is outlined in InternalTreeLogic, skip this value (it's fine)
						return false;
				  }
   
				  // A hit, it's within the range we search for
				  if ( _first )
				  {
						// Setting first to false include an additional check for coming potential
						// hits so that we cannot go backwards in our result. Going backwards can
						// happen when reading through concurrent splits or similar and is a benign
						// temporary observed state.
						_first = false;
				  }
				  return true;
			 }
		 }

		 /// <summary>
		 /// <seealso cref="TreeNode.keyCount(PageCursor) keyCount"/> is the only value read inside a do-shouldRetry loop
		 /// which is used as data fed into another read. Because of that extra assertions are made around
		 /// keyCount, both inside do-shouldRetry (requesting one more round in the loop) and outside
		 /// (calling this method, which may throw exception).
		 /// </summary>
		 /// <param name="keyCount"> key count of a tree node. </param>
		 /// <returns> {@code true} if key count is sane, i.e. positive and within max expected key count on a tree node. </returns>
		 private bool KeyCountIsSane( int keyCount )
		 {
			  // if keyCount is out of bounds of what a tree node can hold, it must be that we're
			  // reading from an evicted page that just happened to look like a tree node.
			  return _bTreeNode.reasonableKeyCount( keyCount );
		 }

		 private bool SaneRead()
		 {
			  return KeyCountIsSane( _keyCount ) && KeySearch.IsSuccess( _searchResult );
		 }

		 /// <summary>
		 /// Perform a generation catchup, updates current root and update range to start from
		 /// previously returned key. Should be followed by a call to <seealso cref="traverseDownToFirstLeaf()"/>
		 /// or if already in that method just loop again.
		 /// <para>
		 /// Caller should retry most recent read after calling this method.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <exception cref="IOException"> on <seealso cref="PageCursor"/>. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void prepareToStartFromRoot() throws java.io.IOException
		 private void PrepareToStartFromRoot()
		 {
			  GenerationCatchup();
			  Root root = _rootCatchup( _cursor.CurrentPageId );
			  _lastFollowedPointerGeneration = root.GoTo( _cursor );
			  if ( !_first )
			  {
					_layout.copyKey( _prevKey, _fromInclusive );
			  }
		 }

		 /// <summary>
		 /// Verifies that the generation of the tree node arrived at matches the generation of the pointer
		 /// pointing to the tree node. Generation of the node cannot be higher than the generation of the pointer -
		 /// if it is then it means that the tree node has been removed (or made obsolete) and reused since we read the
		 /// pointer pointing to it and that the seek is now in an invalid location and needs to be restarted from the root.
		 /// </summary>
		 /// <returns> {@code true} if generation matches, otherwise {@code false} if seek needs to be restarted from root. </returns>
		 private bool VerifyNodeGenerationInvariants()
		 {
			  if ( _lastFollowedPointerGeneration != 0 )
			  {
					if ( _currentNodeGeneration > _lastFollowedPointerGeneration )
					{
						 // We've just followed a pointer to a new node, we have arrived there and made
						 // the first read on it. It looks like the node we arrived at have a higher generation
						 // than the pointer generation, this means that this node node have been reused between
						 // following the pointer and reading the node after getting there.
						 return false;
					}
					_lastFollowedPointerGeneration = 0;
					_expectedCurrentNodeGeneration = _currentNodeGeneration;
			  }
			  else if ( _currentNodeGeneration != _expectedCurrentNodeGeneration )
			  {
					// We've read more than once from this node and between reads the node generation has changed.
					// This means the node has been reused.
					return false;
			  }
			  return true;
		 }

		 /// <summary>
		 /// Checks the provided pointer read and if not successful performs a generation catch-up with
		 /// <seealso cref="generationSupplier"/> to allow reading that same pointer again given the updated generation context.
		 /// </summary>
		 /// <param name="pointer"> read result to check for success. </param>
		 /// <param name="allowNoNode"> whether or not pointer is allowed to be "null". </param>
		 /// <returns> {@code true} if there was a generation catch-up called and generation was actually updated,
		 /// this means that caller should retry its most recent read. </returns>
		 private bool PointerCheckingWithGenerationCatchup( long pointer, bool allowNoNode )
		 {
			  if ( !GenerationSafePointerPair.IsSuccess( pointer ) )
			  {
					// An unexpected sibling read, this could have been caused by a concurrent checkpoint
					// where generation has been incremented. Re-read generation and, if changed since this
					// seek started then update generation locally
					if ( GenerationCatchup() )
					{
						 return true;
					}
					PointerChecking.CheckPointer( pointer, allowNoNode );
			  }
			  return false;
		 }

		 /// <summary>
		 /// Updates generation using the <seealso cref="generationSupplier"/>. If there has been a generation change
		 /// since construction of this seeker or since last calling this method the generation context in this
		 /// seeker is updated.
		 /// </summary>
		 /// <returns> {@code true} if generation was updated, which means that caller should retry its most recent read. </returns>
		 private bool GenerationCatchup()
		 {
			  long newGeneration = _generationSupplier.AsLong;
			  long newStableGeneration = Generation.StableGeneration( newGeneration );
			  long newUnstableGeneration = Generation.UnstableGeneration( newGeneration );
			  if ( newStableGeneration != _stableGeneration || newUnstableGeneration != _unstableGeneration )
			  {
					_stableGeneration = newStableGeneration;
					_unstableGeneration = newUnstableGeneration;
					return true;
			  }
			  return false;
		 }

		 public override KEY Key()
		 {
			  return _mutableKeys[_cachedIndex];
		 }

		 public override VALUE Value()
		 {
			  return _mutableValues[_cachedIndex];
		 }

		 public override void Close()
		 {
			  _cursor.close();
			  _closed = true;
		 }
	}

}