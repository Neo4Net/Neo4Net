using System.Diagnostics;

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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache
{

	using Direction = Neo4Net.Graphdb.Direction;
	using BigIdTracker = Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.@string.BigIdTracker;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Numbers.safeCastIntToUnsignedShort;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Numbers.unsignedShortToInt;

	/// <summary>
	/// Caches of parts of node store and relationship group store. A crucial part of batch import where
	/// any random access must be covered by this cache. All I/O, both read and write must be sequential.
	/// 
	/// <pre>
	/// Main array (index into array is nodeId):
	/// [ID,DEGREE]
	/// 
	/// ID means:
	/// - DEGREE >= THRESHOLD: pointer into RelationshipGroupCache array
	///   RelationshipGroupCache array:
	///   [NEXT,OUT_ID,OUT_DEGREE,IN_ID,IN_DEGREE,LOOP_ID,LOOP_DEGREE]
	/// - DEGREE < THRESHOLD: last seen relationship id for this node
	/// </pre>
	/// 
	/// This class is designed to be thread safe if callers are coordinated such that different threads owns different
	/// parts of the main cache array, with the constraint that a thread which accesses item N must continue doing
	/// so in order to make further changes to N, if another thread accesses N the semantics will no longer hold.
	/// 
	/// Since multiple threads are making changes external memory synchronization is also required in between
	/// a phase of making changes using <seealso cref="getAndPutRelationship(long, int, Direction, long, bool)"/> and e.g
	/// <seealso cref="visitChangedNodes(NodeChangeVisitor, int)"/>.
	/// </summary>
	public class NodeRelationshipCache : MemoryStatsVisitor_Visitable, AutoCloseable
	{
		 private const int CHUNK_SIZE = 1_000_000;
		 private const long EMPTY = -1;
		 private static readonly long _maxRelationshipId = ( 1L << 48 ) - 2;
		 // if count goes beyond this max count then count is redirected to bigCounts and index into that array
		 // is stored as value in count offset
		 internal static readonly int MaxSmallCount = ( 1 << 29 ) - 2;
		 // this max count is pessimistic in that it's what community format can hold, still pretty big.
		 // we can make this as big as our storage needs them later on
		 internal static readonly long MaxCount = ( 1L << 35 ) - 1;

		 // Sizes and offsets of values in each sparse node ByteArray item
		 private const int ID_SIZE = 6;
		 private const int COUNT_SIZE = 4;
		 private static readonly int _idAndCountSize = ID_SIZE + COUNT_SIZE;
		 private const int SPARSE_ID_OFFSET = 0;
		 private const int SPARSE_COUNT_OFFSET = ID_SIZE;

		 // Masking for tracking changes per node
		 private const int DENSE_NODE_CHANGED_MASK = unchecked( ( int )0x80000000 );
		 private const int SPARSE_NODE_CHANGED_MASK = 0x40000000;
		 private const int BIG_COUNT_MASK = 0x20000000;
		 private const int COUNT_FLAGS_MASKS = DENSE_NODE_CHANGED_MASK | SPARSE_NODE_CHANGED_MASK | BIG_COUNT_MASK;
		 private static readonly int _countMask = ~COUNT_FLAGS_MASKS;

		 private const int TYPE_SIZE = 2;
		 public static readonly int GroupEntrySize = TYPE_SIZE + ID_SIZE + _idAndCountSize * Direction.values().length;

		 private ByteArray _array;
		 private sbyte[] _chunkChangedArray;
		 private readonly int _denseNodeThreshold;
		 private readonly RelGroupCache _relGroupCache;
		 private long _highNodeId;
		 // This cache participates in scans backwards and forwards, marking entities as changed in the process.
		 // When going forward (forward==true) changes are marked with a set bit, a cleared bit when going backwards.
		 // This way there won't have to be a clearing of the change bits in between the scans.
		 private volatile bool _forward = true;
		 private readonly int _chunkSize;
		 private readonly NumberArrayFactory _arrayFactory;
		 private readonly LongArray _bigCounts;
		 private readonly AtomicInteger _bigCountsCursor = new AtomicInteger();
		 private long _numberOfDenseNodes;

		 public NodeRelationshipCache( NumberArrayFactory arrayFactory, int denseNodeThreshold ) : this( arrayFactory, denseNodeThreshold, CHUNK_SIZE, 0 )
		 {
		 }

		 internal NodeRelationshipCache( NumberArrayFactory arrayFactory, int denseNodeThreshold, int chunkSize, long @base )
		 {
			  this._arrayFactory = arrayFactory;
			  this._chunkSize = chunkSize;
			  this._denseNodeThreshold = denseNodeThreshold;
			  this._bigCounts = arrayFactory.NewDynamicLongArray( 1_000, 0 );
			  this._relGroupCache = new RelGroupCache( this, arrayFactory, chunkSize, @base );
		 }

		 private static sbyte[] MinusOneBytes( int length )
		 {
			  sbyte[] bytes = new sbyte[length];
			  Arrays.fill( bytes, ( sbyte ) - 1 );
			  return bytes;
		 }

		 /// <summary>
		 /// Increment relationship count for {@code nodeId}. </summary>
		 /// <param name="nodeId"> node to increment relationship count for. </param>
		 /// <returns> count after the increment. </returns>
		 public virtual long IncrementCount( long nodeId )
		 {
			  return IncrementCount( _array, nodeId, SPARSE_COUNT_OFFSET );
		 }

		 /// <summary>
		 /// Should only be used by tests
		 /// </summary>
		 public virtual void SetCount( long nodeId, long count, int typeId, Direction direction )
		 {
			  if ( IsDense( nodeId ) )
			  {
					long relGroupId = All48Bits( _array, nodeId, SPARSE_ID_OFFSET );
					_relGroupCache.setCount( relGroupId, typeId, direction, count );
			  }
			  else
			  {
					SetCount( _array, nodeId, SPARSE_COUNT_OFFSET, count );
			  }
		 }

		 /// <summary>
		 /// This method sets count (node degree, really). It's somewhat generic in that it accepts
		 /// array and offset to set the count into. This is due to there being multiple places where
		 /// we store counts. Simplest one is for sparse nodes, which live in the main
		 /// NodeRelationshipCache.array at the dedicated offset. Other counts live in RelGroupCache.array
		 /// which contain three counts, one for each direction. That's covered by array and offset,
		 /// the count field works the same in all those scenarios. It's an integer which happens to have
		 /// some other flags at msb, so it's the 29 lsb bits which represents the count. 2^29 is merely
		 /// 1/2bn and so the count field has its 30th bit marking whether or not it's a "big count",
		 /// if it is then the 29 count bits instead point to an array index/slot into bigCounts array
		 /// which has much bigger space per count. This is of course quite rare, but nice to support.
		 /// 
		 /// <pre>
		 /// "small" count, i.e. < 2^29
		 /// [  0c,cccc][cccc,cccc][cccc,cccc][cccc,cccc]
		 ///    │└──────────────────┬──────────────────┘
		 ///    │       bits containing actual count
		 ///  0 marking that this is a small count
		 /// 
		 /// "big" count, i.e. >= 2^29
		 /// [  1i,iiii][iiii,iiii][iiii,iiii][iiii,iiii]
		 ///    │└──────────────────┬──────────────────┘
		 ///    │    bits containing array index into bigCounts array which contains the actual count
		 ///  1 marking that this is a big count
		 /// </pre>
		 /// 
		 /// so the bigCounts array is shared between all different types of counts, because big counts are so rare
		 /// </summary>
		 /// <param name="array"> <seealso cref="ByteArray"/> to set count in </param>
		 /// <param name="index"> node id, i.e. array index </param>
		 /// <param name="offset"> offset on that array index (a ByteArray feature) </param>
		 /// <param name="count"> count to set at this position </param>
		 private void SetCount( ByteArray array, long index, int offset, long count )
		 {
			  AssertValidCount( index, count );

			  if ( count > MaxSmallCount )
			  {
					int rawCount = array.GetInt( index, offset );
					int slot;
					if ( rawCount == -1 || !IsBigCount( rawCount ) )
					{
						 // Allocate a slot in the bigCounts array
						 slot = _bigCountsCursor.AndIncrement;
						 array.SetInt( index, offset, BIG_COUNT_MASK | slot );
					}
					else
					{
						 slot = CountValue( rawCount );
					}
					_bigCounts.set( slot, count );
			  }
			  else
			  { // We can simply set it
					array.SetInt( index, offset, toIntExact( count ) );
			  }
		 }

		 private static void AssertValidCount( long nodeId, long count )
		 {
			  if ( count > MaxCount )
			  {
					// Meaning there are bits outside of this mask, meaning this value is too big
					throw new System.InvalidOperationException( "Tried to increment count of node id " + nodeId + " to " + count + ", which is too big in one single import" );
			  }
		 }

		 private static bool IsBigCount( int storedCount )
		 {
			  return ( storedCount & BIG_COUNT_MASK ) != 0;
		 }

		 /// <summary>
		 /// Called by the one calling <seealso cref="incrementCount(long)"/> after all nodes have been added.
		 /// Done like this since currently it's just overhead trying to maintain a high id in the face
		 /// of current updates, whereas it's much simpler to do this from the code incrementing the counts.
		 /// </summary>
		 /// <param name="nodeCount"> high node id in the store, e.g. the highest node id + 1 </param>
		 public virtual long NodeCount
		 {
			 set
			 {
				  if ( value - 1 > BigIdTracker.MAX_ID )
				  {
						throw new System.ArgumentException( format( "Invalid number of nodes %d. Max is %d", value, BigIdTracker.MAX_ID ) );
				  }
   
				  this._highNodeId = value;
				  this._array = _arrayFactory.newByteArray( _highNodeId, MinusOneBytes( _idAndCountSize ) );
				  this._chunkChangedArray = new sbyte[ChunkOf( value ) + 1];
			 }
		 }

		 /// <seealso cref= #setCount(ByteArray, long, int, long) setCount for description on how bigCounts work </seealso>
		 private long GetCount( ByteArray array, long index, int offset )
		 {
			  int rawCount = array.GetInt( index, offset );
			  int count = CountValue( rawCount );
			  if ( count == _countMask )
			  {
					// All bits 1, i.e. default initialized field
					return 0;
			  }

			  if ( IsBigCount( rawCount ) )
			  {
					// 'count' means index into bigCounts in this context
					return _bigCounts.get( count );
			  }

			  return count;
		 }

		 private static int CountValue( int rawCount )
		 {
			  return rawCount & _countMask;
		 }

		 private long IncrementCount( ByteArray array, long index, int offset )
		 {
			  array = array.at( index );
			  long count = GetCount( array, index, offset ) + 1;
			  SetCount( array, index, offset, count );
			  return count;
		 }

		 /// <param name="nodeId"> node to check whether dense or not. </param>
		 /// <returns> whether or not the given {@code nodeId} is dense. A node is sparse if it has less relationships,
		 /// e.g. has had less calls to <seealso cref="incrementCount(long)"/>, then the given dense node threshold. </returns>
		 public virtual bool IsDense( long nodeId )
		 {
			  return IsDense( _array, nodeId );
		 }

		 private bool IsDense( ByteArray array, long nodeId )
		 {
			  if ( _denseNodeThreshold == EMPTY )
			  { // We haven't initialized the rel group cache yet
					return false;
			  }

			  return GetCount( array, nodeId, SPARSE_COUNT_OFFSET ) >= _denseNodeThreshold;
		 }

		 /// <summary>
		 /// Puts a relationship id to be the head of a relationship chain. If the node is sparse then
		 /// the head is set directly in the cache, else if dense which head to update will depend on
		 /// the {@code direction}.
		 /// </summary>
		 /// <param name="nodeId"> node to update relationship head for. </param>
		 /// <param name="typeId"> relationship type id. </param>
		 /// <param name="direction"> <seealso cref="Direction"/> this node represents for this relationship. </param>
		 /// <param name="firstRelId"> the relationship id which is now the head of this chain. </param>
		 /// <param name="incrementCount"> as side-effect also increment count for this chain. </param>
		 /// <returns> the previous head of the updated relationship chain. </returns>
		 public virtual long GetAndPutRelationship( long nodeId, int typeId, Direction direction, long firstRelId, bool incrementCount )
		 {
			  if ( firstRelId > _maxRelationshipId )
			  {
					throw new System.ArgumentException( "Illegal relationship id, max is " + _maxRelationshipId );
			  }

			  /*
			   * OK so the story about counting goes: there's an initial pass for counting number of relationships
			   * per node, globally, not per type/direction. After that the relationship group cache is initialized
			   * and the relationship stage is executed where next pointers are constructed. That forward pass should
			   * not increment the global count, but it should increment the type/direction counts.
			   */

			  ByteArray array = this._array.at( nodeId );
			  long existingId = All48Bits( array, nodeId, SPARSE_ID_OFFSET );
			  bool dense = IsDense( array, nodeId );
			  bool wasChanged = MarkAsChanged( array, nodeId, ChangeMask( dense ) );
			  MarkChunkAsChanged( nodeId, dense );
			  if ( dense )
			  {
					if ( existingId == EMPTY )
					{
						 existingId = _relGroupCache.allocate( typeId );
						 SetRelationshipId( array, nodeId, existingId );
					}
					return _relGroupCache.getAndPutRelationship( existingId, typeId, direction, firstRelId, incrementCount );
			  }

			  // Don't increment count for sparse node since that has already been done in a previous pass
			  SetRelationshipId( array, nodeId, firstRelId );
			  return wasChanged ? EMPTY : existingId;
		 }

		 private void MarkChunkAsChanged( long nodeId, bool dense )
		 {
			  sbyte mask = ChunkChangeMask( dense );
			  if ( !ChunkHasChange( nodeId, mask ) )
			  {
					int chunk = ChunkOf( nodeId );
					if ( ( _chunkChangedArray[chunk] & mask ) == 0 )
					{
						 // Multiple threads may update this chunk array, synchronized performance-wise is fine on change since
						 // it'll only happen at most a couple of times for each chunk (1M).
						 lock ( _chunkChangedArray )
						 {
							  _chunkChangedArray[chunk] |= mask;
						 }
					}
			  }
		 }

		 internal virtual long CalculateNumberOfDenseNodes()
		 {
			  long count = 0;
			  for ( long i = 0; i < _highNodeId; i++ )
			  {
					if ( IsDense( i ) )
					{
						 count++;
					}
			  }
			  return count;
		 }

		 private int ChunkOf( long nodeId )
		 {
			  return toIntExact( nodeId / _chunkSize );
		 }

		 private static sbyte ChunkChangeMask( bool dense )
		 {
			  return ( sbyte )( 1 << ( dense ? 1 : 0 ) );
		 }

		 private bool MarkAsChanged( ByteArray array, long nodeId, int mask )
		 {
			  int bits = array.GetInt( nodeId, SPARSE_COUNT_OFFSET );
			  bool changeBitIsSet = ( bits & mask ) != 0;
			  bool changeBitWasFlipped = changeBitIsSet != _forward;
			  if ( changeBitWasFlipped )
			  {
					bits ^= mask; // flip the mask bit
					array.SetInt( nodeId, SPARSE_COUNT_OFFSET, bits );
			  }
			  return changeBitWasFlipped;
		 }

		 private bool NodeIsChanged( ByteArray array, long nodeId, long mask )
		 {
			  int bits = array.GetInt( nodeId, SPARSE_COUNT_OFFSET );

			  // The values in the cache are initialized with -1, i.e. all bits set, i.e. also the
			  // change bits set. For nodes that gets at least one call to incrementCount these will be
			  // set properly to reflect the count, e.g. 1, 2, 3, a.s.o. Nodes that won't get any call
			  // to incrementCount will not see any changes to them either, so for this matter we check
			  // if the count field is -1 as a whole and if so we can tell we've just run into such a node
			  // and we can safely say it hasn't been changed.
			  if ( bits == unchecked( ( int )0xFFFFFFFF ) )
			  {
					return false;
			  }
			  bool changeBitIsSet = ( bits & mask ) != 0;
			  return changeBitIsSet == _forward;
		 }

		 private static void SetRelationshipId( ByteArray array, long nodeId, long firstRelId )
		 {
			  array.Set6ByteLong( nodeId, SPARSE_ID_OFFSET, firstRelId );
		 }

		 private static long GetRelationshipId( ByteArray array, long nodeId )
		 {
			  return array.Get6ByteLong( nodeId, SPARSE_ID_OFFSET );
		 }

		 private static long All48Bits( ByteArray array, long index, int offset )
		 {
			  return All48Bits( array.Get6ByteLong( index, offset ) );
		 }

		 private static long All48Bits( long raw )
		 {
			  return raw == -1L ? raw : raw & 0xFFFFFFFFFFFFL;
		 }

		 /// <summary>
		 /// Used when setting node nextRel fields. Gets the first relationship for this node,
		 /// or the relationship group id. As a side effect this method also creates a relationship group
		 /// if this node is dense, and returns that relationship group record id.
		 /// </summary>
		 /// <param name="nodeId"> id to get first relationship for. </param>
		 /// <param name="visitor"> <seealso cref="GroupVisitor"/> which will be notified with data about group to be created.
		 /// This visitor is expected to create the group. </param>
		 /// <returns> the first relationship if node is sparse, or the result of <seealso cref="GroupVisitor"/> if dense. </returns>
		 public virtual long GetFirstRel( long nodeId, GroupVisitor visitor )
		 {
			  Debug.Assert( _forward, "This should only be done at forward scan" );

			  ByteArray array = this._array.at( nodeId );
			  long id = GetRelationshipId( array, nodeId );
			  if ( id != EMPTY && IsDense( array, nodeId ) )
			  { // Indirection into rel group cache
					return _relGroupCache.visitGroup( nodeId, id, visitor );
			  }

			  return id;
		 }

		 /// <summary>
		 /// First a note about tracking which nodes have been updated with new relationships by calls to
		 /// <seealso cref="getAndPutRelationship(long, int, Direction, long, bool)"/>:
		 /// 
		 /// We use two high bits of the count field in the "main" array to mark whether or not a change
		 /// have been made to a node. One bit for a sparse node and one for a dense. Sparse and dense nodes
		 /// now have different import cycles. When importing the relationships, all relationships are imported,
		 /// one type at a time, but only dense nodes and relationship chains for dense nodes are updated
		 /// for every type. After all types have been imported the sparse chains and nodes are updated in one pass.
		 /// 
		 /// Tells this cache which direction it's about to observe changes for. If {@code true} then changes
		 /// marked as the change-bit set and an unset change-bit means a change is the first one for that node.
		 /// {@code false} is the opposite. This is so that there won't need to be any clearing of the cache
		 /// in between forward and backward linking, since the cache can be rather large.
		 /// </summary>
		 /// <param name="forward"> {@code true} if going forward and having change marked as a set bit, otherwise
		 /// change is marked with an unset bit. </param>
		 /// <param name="denseNodes"> whether or not this is about dense nodes. If so then some additional cache
		 /// preparation work needs to be done. </param>
		 public virtual void SetForwardScan( bool forward, bool denseNodes )
		 {
			  if ( this._forward == forward )
			  {
					return;
			  }

			  // There's some additional preparations to do for dense nodes between each pass,
			  // this is because that piece of memory is reused.
			  if ( denseNodes )
			  {
					if ( forward )
					{
						 // Clear relationship group cache and references to it
						 VisitChangedNodes( ( nodeId, _array ) => setRelationshipId( _array, nodeId, EMPTY ), NodeType.NODE_TYPE_DENSE );
						 ClearChangedChunks( true );
						 _relGroupCache.clear();
					}
					else
					{
						 // Keep the relationship group cache entries, but clear all relationship chain heads
						 _relGroupCache.clearRelationshipIds();
					}
			  }
			  this._forward = forward;
		 }

		 /// <summary>
		 /// Returns the count (degree) of the requested relationship chain. If node is sparse then the single count
		 /// for this node is returned, otherwise if the node is dense the count for the chain for the specific
		 /// direction is returned.
		 /// 
		 /// For dense nodes the count will be reset after returned here. This is so that the same memory area
		 /// can be used for the next type import.
		 /// </summary>
		 /// <param name="nodeId"> node to get count for. </param>
		 /// <param name="typeId"> relationship type id to get count for. </param>
		 /// <param name="direction"> <seealso cref="Direction"/> to get count for. </param>
		 /// <returns> count (degree) of the requested relationship chain. </returns>
		 public virtual long GetCount( long nodeId, int typeId, Direction direction )
		 {
			  ByteArray array = this._array.at( nodeId );
			  bool dense = IsDense( array, nodeId );
			  if ( dense )
			  { // Indirection into rel group cache
					long id = GetRelationshipId( array, nodeId );
					return id == EMPTY ? 0 : _relGroupCache.getAndResetCount( id, typeId, direction );
			  }

			  return GetCount( array, nodeId, SPARSE_COUNT_OFFSET );
		 }

		 public interface GroupVisitor
		 {
			  /// <summary>
			  /// Visits with data required to create a relationship group.
			  /// Type can be decided on the outside since there'll be only one type per node.
			  /// </summary>
			  /// <param name="nodeId"> node id. </param>
			  /// <param name="typeId"> relationship type id. </param>
			  /// <param name="out"> first outgoing relationship id. </param>
			  /// <param name="in"> first incoming relationship id. </param>
			  /// <param name="loop"> first loop relationship id. </param>
			  /// <returns> the created relationship group id. </returns>
			  long Visit( long nodeId, int typeId, long @out, long @in, long loop );
		 }

		 public static readonly GroupVisitor NoGroupVisitor = ( nodeId, typeId, @out, @in, loop ) => -1;

		 private class RelGroupCache : AutoCloseable, MemoryStatsVisitor_Visitable
		 {
			 private readonly NodeRelationshipCache _outerInstance;

			  internal const int TYPE_OFFSET = 0;
			  internal const int NEXT_OFFSET = TYPE_SIZE;
			  internal static readonly int BaseIdsOffset = NEXT_OFFSET + ID_SIZE;

			  // Used for testing high id values. Should always be zero in production
			  internal readonly sbyte[] DefaultValue = MinusOneBytes( GroupEntrySize );
			  internal readonly long ChunkSize;
			  internal readonly long Base;
			  internal readonly ByteArray Array;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly AtomicLong NextFreeIdConflict;

			  internal RelGroupCache( NodeRelationshipCache outerInstance, NumberArrayFactory arrayFactory, long chunkSize, long @base )
			  {
				  this._outerInstance = outerInstance;
					this.ChunkSize = chunkSize;
					this.Base = @base;
					Debug.Assert( chunkSize > 0 );
					this.Array = arrayFactory.NewDynamicByteArray( chunkSize, DefaultValue );
					this.NextFreeIdConflict = new AtomicLong( @base );
			  }

			  internal virtual void ClearIndex( ByteArray array, long relGroupId )
			  {
					array.Set( relGroupId, DefaultValue );
			  }

			  internal virtual long GetAndResetCount( long relGroupIndex, int typeId, Direction direction )
			  {
					long index = Rebase( relGroupIndex );
					while ( index != EMPTY )
					{
						 ByteArray array = this.Array.at( index );
						 if ( GetTypeId( array, index ) == typeId )
						 {
							  int offset = CountOffset( direction );
							  long count = _outerInstance.getCount( array, index, offset );
							  _outerInstance.setCount( array, index, offset, 0 );
							  return count;
						 }
						 index = GetNext( array, index );
					}
					return 0;
			  }

			  internal virtual void SetCount( long relGroupIndex, int typeId, Direction direction, long count )
			  {
					long index = Rebase( relGroupIndex );
					while ( index != EMPTY )
					{
						 ByteArray array = this.Array.at( index );
						 if ( GetTypeId( array, index ) == typeId )
						 {
							  _outerInstance.setCount( array, index, CountOffset( direction ), count );
							  break;
						 }
						 index = GetNext( array, index );
					}
			  }

			  internal virtual long GetNext( ByteArray array, long index )
			  {
					return All48Bits( array, index, NEXT_OFFSET );
			  }

			  internal virtual int GetTypeId( ByteArray array, long index )
			  {
					return unsignedShortToInt( array.GetShort( index, TYPE_OFFSET ) );
			  }

			  /// <summary>
			  /// Compensate for test value of index (to avoid allocating all your RAM)
			  /// </summary>
			  internal virtual long Rebase( long index )
			  {
					return index - Base;
			  }

			  internal virtual long NextFreeId()
			  {
					return NextFreeIdConflict.AndIncrement;
			  }

			  internal virtual long VisitGroup( long nodeId, long relGroupIndex, GroupVisitor visitor )
			  {
					long currentIndex = Rebase( relGroupIndex );
					long first = EMPTY;
					while ( currentIndex != EMPTY )
					{
						 ByteArray array = this.Array.at( currentIndex );
						 long @out = All48Bits( array, currentIndex, IdOffset( Direction.OUTGOING ) );
						 int typeId = GetTypeId( array, currentIndex );
						 long @in = All48Bits( array, currentIndex, IdOffset( Direction.INCOMING ) );
						 long loop = All48Bits( array, currentIndex, IdOffset( Direction.BOTH ) );
						 long next = GetNext( array, currentIndex );
						 long nextId = @out == EMPTY && @in == EMPTY && loop == EMPTY ? EMPTY : visitor.Visit( nodeId, typeId, @out, @in, loop );
						 if ( first == EMPTY )
						 { // This is the one we return
							  first = nextId;
						 }
						 currentIndex = next;
					}
					return first;
			  }

			  internal virtual int IdOffset( Direction direction )
			  {
					return BaseIdsOffset + ( direction.ordinal() * _idAndCountSize );
			  }

			  internal virtual int CountOffset( Direction direction )
			  {
					return IdOffset( direction ) + ID_SIZE;
			  }

			  internal virtual long Allocate( int typeId )
			  {
					long index = NextFreeId();
					long rebasedIndex = Rebase( index );
					ByteArray array = this.Array.at( rebasedIndex );
					ClearIndex( array, rebasedIndex );
					short shortTypeId = safeCastIntToUnsignedShort( typeId );
					array.SetShort( rebasedIndex, TYPE_OFFSET, shortTypeId );
					return index;
			  }

			  internal virtual long GetAndPutRelationship( long relGroupIndex, int typeId, Direction direction, long relId, bool incrementCount )
			  {
					long index = Rebase( relGroupIndex );
					index = FindOrAllocateIndex( index, typeId );
					ByteArray array = this.Array.at( index );
					int directionOffset = IdOffset( direction );
					long previousId = All48Bits( array, index, directionOffset );
					array.Set6ByteLong( index, directionOffset, relId );
					if ( incrementCount )
					{
						 incrementCount( array, index, CountOffset( direction ) );
					}
					return previousId;
			  }

			  internal virtual void ClearRelationshipIds( ByteArray array, long index )
			  {
					array.Set6ByteLong( index, IdOffset( Direction.OUTGOING ), EMPTY );
					array.Set6ByteLong( index, IdOffset( Direction.INCOMING ), EMPTY );
					array.Set6ByteLong( index, IdOffset( Direction.BOTH ), EMPTY );
			  }

			  internal virtual long FindOrAllocateIndex( long index, int typeId )
			  {
					long lastIndex = index;
					ByteArray array = this.Array.at( index );
					while ( index != EMPTY )
					{
						 lastIndex = index;
						 array = this.Array.at( index );
						 int candidateTypeId = GetTypeId( array, index );
						 if ( candidateTypeId == typeId )
						 {
							  return index;
						 }
						 index = GetNext( array, index );
					}

					// No such found, create at the end
					long newIndex = Allocate( typeId );
					array.Set6ByteLong( lastIndex, NEXT_OFFSET, newIndex );
					return newIndex;
			  }

			  public override void Close()
			  {
					if ( Array != null )
					{
						 Array.close();
					}
			  }

			  public override void AcceptMemoryStatsVisitor( MemoryStatsVisitor visitor )
			  {
					NullSafeMemoryStatsVisitor( Array, visitor );
			  }

			  public virtual void Clear()
			  {
					NextFreeIdConflict.set( Base );
			  }

			  public virtual void ClearRelationshipIds()
			  {
					long highId = Rebase( NextFreeIdConflict.get() );
					for ( long i = 0; i < highId; )
					{
						 ByteArray chunk = Array.at( i );
						 for ( int j = 0; j < ChunkSize && i < highId; j++, i++ )
						 {
							  ClearRelationshipIds( chunk, i );
						 }
					}
			  }
		 }

		 public override string ToString()
		 {
			  return _array.ToString();
		 }

		 public override void Close()
		 {
			  if ( _array != null )
			  {
					_array.close();
			  }
			  if ( _relGroupCache != null )
			  {
					_relGroupCache.close();
			  }
		 }

		 public override void AcceptMemoryStatsVisitor( MemoryStatsVisitor visitor )
		 {
			  NullSafeMemoryStatsVisitor( _array, visitor );
			  _relGroupCache.acceptMemoryStatsVisitor( visitor );
		 }

		 internal static void NullSafeMemoryStatsVisitor( MemoryStatsVisitor_Visitable visitable, MemoryStatsVisitor visitor )
		 {
			  if ( visitable != null )
			  {
					visitable.AcceptMemoryStatsVisitor( visitor );
			  }
		 }

		 private static int ChangeMask( bool dense )
		 {
			  return dense ? DENSE_NODE_CHANGED_MASK : SPARSE_NODE_CHANGED_MASK;
		 }

		 public delegate void NodeChangeVisitor( long nodeId, ByteArray array );

		 /// <summary>
		 /// Efficiently visits changed nodes, e.g. nodes that have had any relationship chain updated by
		 /// <seealso cref="getAndPutRelationship(long, int, Direction, long, bool)"/>.
		 /// </summary>
		 /// <param name="visitor"> <seealso cref="NodeChangeVisitor"/> which will be notified about all changes. </param>
		 /// <param name="nodeTypes"> which types to visit (dense/sparse). </param>
		 public virtual void VisitChangedNodes( NodeChangeVisitor visitor, int nodeTypes )
		 {
			  long denseMask = ChangeMask( true );
			  long sparseMask = ChangeMask( false );
			  sbyte denseChunkMask = ChunkChangeMask( true );
			  sbyte sparseChunkMask = ChunkChangeMask( false );
			  for ( long nodeId = 0; nodeId < _highNodeId; )
			  {
					bool chunkHasChanged = ( NodeType.IsDense( nodeTypes ) && ChunkHasChange( nodeId, denseChunkMask ) ) || ( NodeType.IsSparse( nodeTypes ) && ChunkHasChange( nodeId, sparseChunkMask ) );
					if ( !chunkHasChanged )
					{
						 nodeId += _chunkSize;
						 continue;
					}

					ByteArray subArray = _array.at( nodeId );
					long subArrayLength = subArray.length();
					for ( int i = 0; i < subArrayLength && nodeId < _highNodeId; i++, nodeId++ )
					{
						 bool nodeHasChanged = ( NodeType.IsDense( nodeTypes ) && NodeIsChanged( subArray, nodeId, denseMask ) ) || ( NodeType.IsSparse( nodeTypes ) && NodeIsChanged( subArray, nodeId, sparseMask ) );

						 if ( nodeHasChanged && NodeType.MatchesDense( nodeTypes, IsDense( _array, nodeId ) ) )
						 {
							  visitor( nodeId, subArray );
						 }
					}
			  }
		 }

		 /// <summary>
		 /// Clears the high-level change marks.
		 /// </summary>
		 /// <param name="denseNodes"> {@code true} for clearing marked dense nodes, {@code false} for clearing marked sparse nodes. </param>
		 private void ClearChangedChunks( bool denseNodes )
		 {
			  // Executed by a single thread, so no synchronized required
			  sbyte chunkMask = ChunkChangeMask( denseNodes );
			  for ( int i = 0; i < _chunkChangedArray.Length; i++ )
			  {
					_chunkChangedArray[i] &= ( sbyte )( ~chunkMask );
			  }
		 }

		 private bool ChunkHasChange( long nodeId, sbyte chunkMask )
		 {
			  int chunkId = ChunkOf( nodeId );
			  return ( _chunkChangedArray[chunkId] & chunkMask ) != 0;
		 }

		 public virtual long CalculateMaxMemoryUsage( long numberOfRelationships )
		 {
			  return CalculateMaxMemoryUsage( _numberOfDenseNodes, numberOfRelationships );
		 }

		 public static long CalculateMaxMemoryUsage( long numberOfDenseNodes, long numberOfRelationships )
		 {
			  long maxDenseNodesForThisType = min( numberOfDenseNodes, numberOfRelationships * 2 );
			  return maxDenseNodesForThisType * NodeRelationshipCache.GroupEntrySize;
		 }

		 public virtual void CountingCompleted()
		 {
			  _numberOfDenseNodes = CalculateNumberOfDenseNodes();
		 }

		 public virtual long NumberOfDenseNodes
		 {
			 get
			 {
				  return _numberOfDenseNodes;
			 }
		 }

		 public virtual MemoryStatsVisitor_Visitable MemoryEstimation( long numberOfNodes )
		 {
			  return new MemoryStatsVisitor_VisitableAnonymousInnerClass( this, numberOfNodes );
		 }

		 private class MemoryStatsVisitor_VisitableAnonymousInnerClass : MemoryStatsVisitor_Visitable
		 {
			 private readonly NodeRelationshipCache _outerInstance;

			 private long _numberOfNodes;

			 public MemoryStatsVisitor_VisitableAnonymousInnerClass( NodeRelationshipCache outerInstance, long numberOfNodes )
			 {
				 this.outerInstance = outerInstance;
				 this._numberOfNodes = numberOfNodes;
			 }

			 public void acceptMemoryStatsVisitor( MemoryStatsVisitor visitor )
			 {
				  visitor.OffHeapUsage( _idAndCountSize * _numberOfNodes );
			 }
		 }
	}

}