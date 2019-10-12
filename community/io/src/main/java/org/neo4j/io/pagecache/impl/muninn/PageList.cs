using System.Text;

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
namespace Org.Neo4j.Io.pagecache.impl.muninn
{

	using MemoryAllocator = Org.Neo4j.Io.mem.MemoryAllocator;
	using EvictionEvent = Org.Neo4j.Io.pagecache.tracing.EvictionEvent;
	using EvictionEventOpportunity = Org.Neo4j.Io.pagecache.tracing.EvictionEventOpportunity;
	using FlushEvent = Org.Neo4j.Io.pagecache.tracing.FlushEvent;
	using PageFaultEvent = Org.Neo4j.Io.pagecache.tracing.PageFaultEvent;
	using UnsafeUtil = Org.Neo4j.@unsafe.Impl.@internal.Dragons.UnsafeUtil;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.FeatureToggles.flag;

	/// <summary>
	/// The PageList maintains the off-heap meta-data for the individual memory pages.
	/// <para>
	/// The meta-data for each page is the following:
	/// 
	/// <table>
	/// <tr><th>Bytes</th><th>Use</th></tr>
	/// <tr><td>8</td><td>Sequence lock word.</td></tr>
	/// <tr><td>8</td><td>Pointer to the memory page.</td></tr>
	/// <tr><td>8</td><td>Last modified transaction id.</td></tr>
	/// <tr><td>8</td><td>Page binding. The first 40 bits (5 bytes) are the file page id.
	/// The following (low order) 21 bits (2 bytes and 5 bits) are the swapper id.
	/// The last (lowest order) 3 bits are the page usage counter.</td></tr>
	/// </table>
	/// </para>
	/// </summary>
	internal class PageList
	{
		 private static readonly bool _forceSlowMemoryClear = flag( typeof( PageList ), "forceSlowMemoryClear", false );

		 internal const int META_DATA_BYTES_PER_PAGE = 32;
		 internal static readonly long MaxPages = int.MaxValue;

		 private const int UNBOUND_LAST_MODIFIED_TX_ID = -1;
		 private const long MAX_USAGE_COUNT = 4;
		 private const int SHIFT_FILE_PAGE_ID = 24;
		 private const int SHIFT_SWAPPER_ID = 3;
		 private static readonly int _shiftPartialFilePageId = SHIFT_FILE_PAGE_ID - SHIFT_SWAPPER_ID;
		 private static readonly long _maskUsageCount = ( 1L << SHIFT_SWAPPER_ID ) - 1L;
		 private static readonly long _maskNotFilePageId = ( 1L << SHIFT_FILE_PAGE_ID ) - 1L;
		 private static readonly long _maskShiftedSwapperId = ( long )( ( ulong )_maskNotFilePageId >> SHIFT_SWAPPER_ID );
		 private static readonly long _maskNotSwapperId = ~( _maskShiftedSwapperId << SHIFT_SWAPPER_ID );
		 private static readonly long _unboundPageBinding = PageCursor.UNBOUND_PAGE_ID << SHIFT_FILE_PAGE_ID;

		 // 40 bits for file page id
		 private static readonly long _maxFilePageId = ( 1L << ( sizeof( long ) * 8 ) - SHIFT_FILE_PAGE_ID ) - 1L;

		 private const int OFFSET_LOCK_WORD = 0; // 8 bytes.
		 private const int OFFSET_ADDRESS = 8; // 8 bytes.
		 private const int OFFSET_LAST_TX_ID = 16; // 8 bytes.
		 // The high 5 bytes of the page binding are the file page id.
		 // The 21 following lower bits are the swapper id.
		 // And the last 3 low bits are the usage counter.
		 private const int OFFSET_PAGE_BINDING = 24; // 8 bytes.

		 private readonly int _pageCount;
		 private readonly int _cachePageSize;
		 private readonly MemoryAllocator _memoryAllocator;
		 private readonly SwapperSet _swappers;
		 private readonly long _victimPageAddress;
		 private readonly long _baseAddress;
		 private readonly long _bufferAlignment;

		 internal PageList( int pageCount, int cachePageSize, MemoryAllocator memoryAllocator, SwapperSet swappers, long victimPageAddress, long bufferAlignment )
		 {
			  this._pageCount = pageCount;
			  this._cachePageSize = cachePageSize;
			  this._memoryAllocator = memoryAllocator;
			  this._swappers = swappers;
			  this._victimPageAddress = victimPageAddress;
			  long bytes = ( ( long ) pageCount ) * META_DATA_BYTES_PER_PAGE;
			  this._baseAddress = memoryAllocator.AllocateAligned( bytes, Long.BYTES );
			  this._bufferAlignment = bufferAlignment;
			  ClearMemory( _baseAddress, pageCount );
		 }

		 /// <summary>
		 /// This copy-constructor is useful for classes that want to extend the {@code PageList} class to inline its fields.
		 /// All data and state will be shared between this and the given {@code PageList}. This means that changing the page
		 /// list state through one has the same effect as changing it through the other – they are both effectively the same
		 /// object.
		 /// </summary>
		 /// <param name="pageList"> The {@code PageList} instance whose state to copy. </param>
		 internal PageList( PageList pageList )
		 {
			  this._pageCount = pageList._pageCount;
			  this._cachePageSize = pageList._cachePageSize;
			  this._memoryAllocator = pageList._memoryAllocator;
			  this._swappers = pageList._swappers;
			  this._victimPageAddress = pageList._victimPageAddress;
			  this._baseAddress = pageList._baseAddress;
			  this._bufferAlignment = pageList._bufferAlignment;
		 }

		 private void ClearMemory( long baseAddress, long pageCount )
		 {
			  long memcpyChunkSize = UnsafeUtil.pageSize();
			  long metaDataEntriesPerChunk = memcpyChunkSize / META_DATA_BYTES_PER_PAGE;
			  if ( pageCount < metaDataEntriesPerChunk || _forceSlowMemoryClear )
			  {
					ClearMemorySimple( baseAddress, pageCount );
			  }
			  else
			  {
					ClearMemoryFast( baseAddress, pageCount, memcpyChunkSize, metaDataEntriesPerChunk );
			  }
			  UnsafeUtil.fullFence(); // Guarantee the visibility of the cleared memory.
		 }

		 private void ClearMemorySimple( long baseAddress, long pageCount )
		 {
			  long address = baseAddress - Long.BYTES;
			  long initialLockWord = OffHeapPageLock.InitialLockWordWithExclusiveLock();
			  for ( long i = 0; i < pageCount; i++ )
			  {
					UnsafeUtil.putLong( address += Long.BYTES, initialLockWord ); // lock word
					UnsafeUtil.putLong( address += Long.BYTES, 0 ); // pointer
					UnsafeUtil.putLong( address += Long.BYTES, 0 ); // last tx id
					UnsafeUtil.putLong( address += Long.BYTES, _unboundPageBinding );
			  }
		 }

		 private void ClearMemoryFast( long baseAddress, long pageCount, long memcpyChunkSize, long metaDataEntriesPerChunk )
		 {
			  // Initialise one chunk worth of data.
			  ClearMemorySimple( baseAddress, metaDataEntriesPerChunk );
			  // Since all entries contain the same data, we can now copy this chunk over and over.
			  long chunkCopies = pageCount / metaDataEntriesPerChunk - 1;
			  long address = baseAddress + memcpyChunkSize;
			  for ( int i = 0; i < chunkCopies; i++ )
			  {
					UnsafeUtil.copyMemory( baseAddress, address, memcpyChunkSize );
					address += memcpyChunkSize;
			  }
			  // Finally fill in the tail.
			  long tailCount = pageCount % metaDataEntriesPerChunk;
			  ClearMemorySimple( address, tailCount );
		 }

		 /// <returns> The capacity of the page list. </returns>
		 internal virtual int PageCount
		 {
			 get
			 {
				  return _pageCount;
			 }
		 }

		 internal virtual SwapperSet Swappers
		 {
			 get
			 {
				  return _swappers;
			 }
		 }

		 /// <summary>
		 /// Turn a {@code pageId} into a {@code pageRef} that can be used for accessing and manipulating the given page
		 /// using the other methods in this class.
		 /// </summary>
		 /// <param name="pageId"> The {@code pageId} to turn into a {@code pageRef}. </param>
		 /// <returns> A {@code pageRef} which is an opaque, internal and direct pointer to the meta-data of the given memory
		 /// page. </returns>
		 internal virtual long Deref( int pageId )
		 {
			  //noinspection UnnecessaryLocalVariable
			  long id = pageId; // convert to long to avoid int multiplication
			  return _baseAddress + ( id * META_DATA_BYTES_PER_PAGE );
		 }

		 internal virtual int ToId( long pageRef )
		 {
			  // >> 5 is equivalent to dividing by 32, META_DATA_BYTES_PER_PAGE.
			  return ( int )( ( pageRef - _baseAddress ) >> 5 );
		 }

		 private long OffLastModifiedTransactionId( long pageRef )
		 {
			  return pageRef + OFFSET_LAST_TX_ID;
		 }

		 private long OffLock( long pageRef )
		 {
			  return pageRef + OFFSET_LOCK_WORD;
		 }

		 private long OffAddress( long pageRef )
		 {
			  return pageRef + OFFSET_ADDRESS;
		 }

		 private long OffPageBinding( long pageRef )
		 {
			  return pageRef + OFFSET_PAGE_BINDING;
		 }

		 internal virtual long TryOptimisticReadLock( long pageRef )
		 {
			  return OffHeapPageLock.TryOptimisticReadLock( OffLock( pageRef ) );
		 }

		 internal virtual bool ValidateReadLock( long pageRef, long stamp )
		 {
			  return OffHeapPageLock.ValidateReadLock( OffLock( pageRef ), stamp );
		 }

		 internal virtual bool IsModified( long pageRef )
		 {
			  return OffHeapPageLock.IsModified( OffLock( pageRef ) );
		 }

		 internal virtual bool IsExclusivelyLocked( long pageRef )
		 {
			  return OffHeapPageLock.IsExclusivelyLocked( OffLock( pageRef ) );
		 }

		 internal virtual bool TryWriteLock( long pageRef )
		 {
			  return OffHeapPageLock.TryWriteLock( OffLock( pageRef ) );
		 }

		 internal virtual void UnlockWrite( long pageRef )
		 {
			  OffHeapPageLock.UnlockWrite( OffLock( pageRef ) );
		 }

		 internal virtual long UnlockWriteAndTryTakeFlushLock( long pageRef )
		 {
			  return OffHeapPageLock.UnlockWriteAndTryTakeFlushLock( OffLock( pageRef ) );
		 }

		 internal virtual bool TryExclusiveLock( long pageRef )
		 {
			  return OffHeapPageLock.TryExclusiveLock( OffLock( pageRef ) );
		 }

		 internal virtual long UnlockExclusive( long pageRef )
		 {
			  return OffHeapPageLock.UnlockExclusive( OffLock( pageRef ) );
		 }

		 internal virtual void UnlockExclusiveAndTakeWriteLock( long pageRef )
		 {
			  OffHeapPageLock.UnlockExclusiveAndTakeWriteLock( OffLock( pageRef ) );
		 }

		 internal virtual long TryFlushLock( long pageRef )
		 {
			  return OffHeapPageLock.TryFlushLock( OffLock( pageRef ) );
		 }

		 internal virtual void UnlockFlush( long pageRef, long stamp, bool success )
		 {
			  OffHeapPageLock.UnlockFlush( OffLock( pageRef ), stamp, success );
		 }

		 internal virtual void ExplicitlyMarkPageUnmodifiedUnderExclusiveLock( long pageRef )
		 {
			  OffHeapPageLock.ExplicitlyMarkPageUnmodifiedUnderExclusiveLock( OffLock( pageRef ) );
		 }

		 internal virtual int CachePageSize
		 {
			 get
			 {
				  return _cachePageSize;
			 }
		 }

		 internal virtual long GetAddress( long pageRef )
		 {
			  return UnsafeUtil.getLong( OffAddress( pageRef ) );
		 }

		 internal virtual void InitBuffer( long pageRef )
		 {
			  if ( GetAddress( pageRef ) == 0L )
			  {
					long addr = _memoryAllocator.allocateAligned( CachePageSize, _bufferAlignment );
					UnsafeUtil.putLong( OffAddress( pageRef ), addr );
			  }
		 }

		 private sbyte GetUsageCounter( long pageRef )
		 {
			  return ( sbyte )( UnsafeUtil.getLongVolatile( OffPageBinding( pageRef ) ) & _maskUsageCount );
		 }

		 /// <summary>
		 /// Increment the usage stamp to at most 4.
		 /// 
		 /// </summary>
		 internal virtual void IncrementUsage( long pageRef )
		 {
			  // This is intentionally left benignly racy for performance.
			  long address = OffPageBinding( pageRef );
			  long value = UnsafeUtil.getLongVolatile( address );
			  long usage = value & _maskUsageCount;
			  if ( usage < MAX_USAGE_COUNT ) // avoid cache sloshing by not doing a write if counter is already maxed out
			  {
					long update = value + 1;
					// Use compareAndSwapLong to only actually store the updated count if nothing else changed
					// in this word-line. The word-line is shared with the file page id, and the swapper id.
					// Those fields are updated under guard of the exclusive lock, but we *might* race with
					// that here, and in that case we would never want a usage counter update to clobber a page
					// binding update.
					UnsafeUtil.compareAndSwapLong( null, address, value, update );
			  }
		 }

		 /// <summary>
		 /// Decrement the usage stamp. Returns true if it reaches 0.
		 /// 
		 /// </summary>
		 internal virtual bool DecrementUsage( long pageRef )
		 {
			  // This is intentionally left benignly racy for performance.
			  long address = OffPageBinding( pageRef );
			  long value = UnsafeUtil.getLongVolatile( address );
			  long usage = value & _maskUsageCount;
			  if ( usage > 0 )
			  {
					long update = value - 1;
					// See `incrementUsage` about why we use `compareAndSwapLong`.
					UnsafeUtil.compareAndSwapLong( null, address, value, update );
			  }
			  return usage <= 1;
		 }

		 internal virtual long GetFilePageId( long pageRef )
		 {
			  long filePageId = ( long )( ( ulong )UnsafeUtil.getLong( OffPageBinding( pageRef ) ) >> SHIFT_FILE_PAGE_ID );
			  return filePageId == _maxFilePageId ? PageCursor.UNBOUND_PAGE_ID : filePageId;
		 }

		 private void SetFilePageId( long pageRef, long filePageId )
		 {
			  if ( filePageId > _maxFilePageId )
			  {
					throw new System.ArgumentException( format( "File page id: %s is bigger then max supported value %s.", filePageId, _maxFilePageId ) );
			  }
			  long address = OffPageBinding( pageRef );
			  long v = UnsafeUtil.getLong( address );
			  filePageId = ( filePageId << SHIFT_FILE_PAGE_ID ) + ( v & _maskNotFilePageId );
			  UnsafeUtil.putLong( address, filePageId );
		 }

		 internal virtual long GetLastModifiedTxId( long pageRef )
		 {
			  return UnsafeUtil.getLongVolatile( OffLastModifiedTransactionId( pageRef ) );
		 }

		 /// <returns> return last modifier transaction id and resets it to <seealso cref="UNBOUND_LAST_MODIFIED_TX_ID"/> </returns>
		 internal virtual long GetAndResetLastModifiedTransactionId( long pageRef )
		 {
			  return UnsafeUtil.getAndSetLong( null, OffLastModifiedTransactionId( pageRef ), UNBOUND_LAST_MODIFIED_TX_ID );
		 }

		 internal virtual void SetLastModifiedTxId( long pageRef, long modifierTxId )
		 {
			  UnsafeUtil.compareAndSetMaxLong( null, OffLastModifiedTransactionId( pageRef ), modifierTxId );
		 }

		 internal virtual int GetSwapperId( long pageRef )
		 {
			  long v = ( long )( ( ulong )UnsafeUtil.getLong( OffPageBinding( pageRef ) ) >> SHIFT_SWAPPER_ID );
			  return ( int )( v & _maskShiftedSwapperId ); // 21 bits.
		 }

		 private void SetSwapperId( long pageRef, int swapperId )
		 {
			  swapperId = swapperId << SHIFT_SWAPPER_ID;
			  long address = OffPageBinding( pageRef );
			  long v = UnsafeUtil.getLong( address ) & _maskNotSwapperId;
			  UnsafeUtil.putLong( address, v + swapperId );
		 }

		 internal virtual bool IsLoaded( long pageRef )
		 {
			  return GetFilePageId( pageRef ) != PageCursor.UNBOUND_PAGE_ID;
		 }

		 internal virtual bool IsBoundTo( long pageRef, int swapperId, long filePageId )
		 {
			  long address = OffPageBinding( pageRef );
			  long expectedBinding = ( filePageId << _shiftPartialFilePageId ) + swapperId;
			  long actualBinding = ( long )( ( ulong )UnsafeUtil.getLong( address ) >> SHIFT_SWAPPER_ID );
			  return expectedBinding == actualBinding;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void fault(long pageRef, org.neo4j.io.pagecache.PageSwapper swapper, int swapperId, long filePageId, org.neo4j.io.pagecache.tracing.PageFaultEvent event) throws java.io.IOException
		 internal virtual void Fault( long pageRef, PageSwapper swapper, int swapperId, long filePageId, PageFaultEvent @event )
		 {
			  if ( swapper == null )
			  {
					throw SwapperCannotBeNull();
			  }
			  int currentSwapper = GetSwapperId( pageRef );
			  long currentFilePageId = GetFilePageId( pageRef );
			  if ( filePageId == PageCursor.UNBOUND_PAGE_ID || !IsExclusivelyLocked( pageRef ) || currentSwapper != 0 || currentFilePageId != PageCursor.UNBOUND_PAGE_ID )
			  {
					throw CannotFaultException( pageRef, swapper, swapperId, filePageId, currentSwapper, currentFilePageId );
			  }
			  // Note: It is important that we assign the filePageId before we swap
			  // the page in. If the swapping fails, the page will be considered
			  // loaded for the purpose of eviction, and will eventually return to
			  // the freelist. However, because we don't assign the swapper until the
			  // swapping-in has succeeded, the page will not be considered bound to
			  // the file page, so any subsequent thread that finds the page in their
			  // translation table will re-do the page fault.
			  SetFilePageId( pageRef, filePageId ); // Page now considered isLoaded()
			  long bytesRead = swapper.Read( filePageId, GetAddress( pageRef ), _cachePageSize );
			  @event.AddBytesRead( bytesRead );
			  @event.CachePageId = ToId( pageRef );
			  SetSwapperId( pageRef, swapperId ); // Page now considered isBoundTo( swapper, filePageId )
		 }

		 private static System.ArgumentException SwapperCannotBeNull()
		 {
			  return new System.ArgumentException( "swapper cannot be null" );
		 }

		 private static System.InvalidOperationException CannotFaultException( long pageRef, PageSwapper swapper, int swapperId, long filePageId, int currentSwapper, long currentFilePageId )
		 {
			  string msg = format( "Cannot fault page {filePageId = %s, swapper = %s (swapper id = %s)} into " + "cache page %s. Already bound to {filePageId = " + "%s, swapper id = %s}.", filePageId, swapper, swapperId, pageRef, currentFilePageId, currentSwapper );
			  return new System.InvalidOperationException( msg );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean tryEvict(long pageRef, org.neo4j.io.pagecache.tracing.EvictionEventOpportunity evictionOpportunity) throws java.io.IOException
		 internal virtual bool TryEvict( long pageRef, EvictionEventOpportunity evictionOpportunity )
		 {
			  if ( TryExclusiveLock( pageRef ) )
			  {
					if ( IsLoaded( pageRef ) )
					{
						 using ( EvictionEvent evictionEvent = evictionOpportunity.BeginEviction() )
						 {
							  Evict( pageRef, evictionEvent );
							  return true;
						 }
					}
					UnlockExclusive( pageRef );
			  }
			  return false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void evict(long pageRef, org.neo4j.io.pagecache.tracing.EvictionEvent evictionEvent) throws java.io.IOException
		 private void Evict( long pageRef, EvictionEvent evictionEvent )
		 {
			  long filePageId = GetFilePageId( pageRef );
			  evictionEvent.FilePageId = filePageId;
			  evictionEvent.CachePageId = pageRef;
			  int swapperId = GetSwapperId( pageRef );
			  if ( swapperId != 0 )
			  {
					// If the swapper id is non-zero, then the page was not only loaded, but also bound, and possibly modified.
					SwapperSet.SwapperMapping swapperMapping = _swappers.getAllocation( swapperId );
					if ( swapperMapping != null )
					{
						 // The allocation can be null if the file has been unmapped, but there are still pages
						 // lingering in the cache that were bound to file page in that file.
						 PageSwapper swapper = swapperMapping.Swapper;
						 evictionEvent.Swapper = swapper;

						 if ( IsModified( pageRef ) )
						 {
							  FlushModifiedPage( pageRef, evictionEvent, filePageId, swapper );
						 }
						 swapper.Evicted( filePageId );
					}
			  }
			  ClearBinding( pageRef );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void flushModifiedPage(long pageRef, org.neo4j.io.pagecache.tracing.EvictionEvent evictionEvent, long filePageId, org.neo4j.io.pagecache.PageSwapper swapper) throws java.io.IOException
		 private void FlushModifiedPage( long pageRef, EvictionEvent evictionEvent, long filePageId, PageSwapper swapper )
		 {
			  FlushEvent flushEvent = evictionEvent.FlushEventOpportunity().beginFlush(filePageId, pageRef, swapper);
			  try
			  {
					long address = GetAddress( pageRef );
					long bytesWritten = swapper.Write( filePageId, address );
					ExplicitlyMarkPageUnmodifiedUnderExclusiveLock( pageRef );
					flushEvent.AddBytesWritten( bytesWritten );
					flushEvent.AddPagesFlushed( 1 );
					flushEvent.Done();
			  }
			  catch ( IOException e )
			  {
					UnlockExclusive( pageRef );
					flushEvent.Done( e );
					evictionEvent.ThrewException( e );
					throw e;
			  }
		 }

		 private void ClearBinding( long pageRef )
		 {
			  UnsafeUtil.putLong( OffPageBinding( pageRef ), _unboundPageBinding );
		 }

		 internal virtual void ToString( long pageRef, StringBuilder sb )
		 {
			  sb.Append( "Page[ id = " ).Append( ToId( pageRef ) );
			  sb.Append( ", address = " ).Append( GetAddress( pageRef ) );
			  sb.Append( ", filePageId = " ).Append( GetFilePageId( pageRef ) );
			  sb.Append( ", swapperId = " ).Append( GetSwapperId( pageRef ) );
			  sb.Append( ", usageCounter = " ).Append( GetUsageCounter( pageRef ) );
			  sb.Append( " ] " ).Append( OffHeapPageLock.ToString( OffLock( pageRef ) ) );
		 }
	}

}