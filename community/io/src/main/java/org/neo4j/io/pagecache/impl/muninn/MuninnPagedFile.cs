using System;

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

	using FlushEvent = Org.Neo4j.Io.pagecache.tracing.FlushEvent;
	using FlushEventOpportunity = Org.Neo4j.Io.pagecache.tracing.FlushEventOpportunity;
	using MajorFlushEvent = Org.Neo4j.Io.pagecache.tracing.MajorFlushEvent;
	using PageCacheTracer = Org.Neo4j.Io.pagecache.tracing.PageCacheTracer;
	using PageFaultEvent = Org.Neo4j.Io.pagecache.tracing.PageFaultEvent;
	using PageCursorTracerSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using VersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using UnsafeUtil = Org.Neo4j.@unsafe.Impl.@internal.Dragons.UnsafeUtil;

	internal sealed class MuninnPagedFile : PageList, PagedFile, Flushable
	{
		 internal const int UNMAPPED_TTE = -1;
		 private static readonly int _translationTableChunkSizePower = Integer.getInteger( "org.neo4j.io.pagecache.impl.muninn.MuninnPagedFile.translationTableChunkSizePower", 12 );
		 private static readonly int _translationTableChunkSize = 1 << _translationTableChunkSizePower;
		 private static readonly long _translationTableChunkSizeMask = _translationTableChunkSize - 1;
		 private static readonly int _translationTableChunkArrayBase = UnsafeUtil.arrayBaseOffset( typeof( int[] ) );
		 private static readonly int _translationTableChunkArrayScale = UnsafeUtil.arrayIndexScale( typeof( int[] ) );

		 private static readonly long _headerStateOffset = UnsafeUtil.getFieldOffset( typeof( MuninnPagedFile ), "headerState" );
		 private const int HEADER_STATE_REF_COUNT_SHIFT = 48;
		 private const int HEADER_STATE_REF_COUNT_MAX = 0x7FFF;
		 private const long HEADER_STATE_REF_COUNT_MASK = 0x7FFF_0000_0000_0000L;
		 private const long HEADER_STATE_LAST_PAGE_ID_MASK = unchecked( ( long )0x8000_FFFF_FFFF_FFFFL );
		 private static readonly int _pfLockMask = Org.Neo4j.Io.pagecache.PagedFile_Fields.PfSharedWriteLock | Org.Neo4j.Io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK;

		 internal readonly MuninnPageCache PageCache;
		 internal readonly int FilePageSize;
		 private readonly PageCacheTracer _pageCacheTracer;
		 internal readonly LatchMap PageFaultLatches;

		 // This is the table where we translate file-page-ids to cache-page-ids. Only one thread can perform a resize at
		 // a time, and we ensure this mutual exclusion using the monitor lock on this MuninnPagedFile object.
		 internal volatile int[][] TranslationTable;

		 internal readonly PageSwapper Swapper;
		 internal readonly int SwapperId;
		 private readonly CursorFactory _cursorFactory;

		 // Guarded by the monitor lock on MuninnPageCache (map and unmap)
		 private bool _deleteOnClose;

		 // Used to trace the causes of any exceptions from getLastPageId.
		 private volatile Exception _closeStackTrace;

		 // max modifier transaction id among evicted pages for this file
		 private static readonly long _evictedTransactionIdOffset = UnsafeUtil.getFieldOffset( typeof( MuninnPagedFile ), "highestEvictedTransactionId" );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private volatile long highestEvictedTransactionId;
		 private volatile long _highestEvictedTransactionId;

		 /// <summary>
		 /// The header state includes both the reference count of the PagedFile – 15 bits – and the ID of the last page in
		 /// the file – 48 bits, plus an empty file marker bit. Because our pages are usually 2^13 bytes, this means that we
		 /// only lose 3 bits to the reference count, in terms of keeping large files byte addressable.
		 /// 
		 /// The layout looks like this:
		 /// 
		 /// ┏━ Empty file marker bit. When 1, the file is empty.
		 /// ┃    ┏━ Reference count, 15 bits.
		 /// ┃    ┃                ┏━ 48 bits for the last page id.
		 /// ┃┏━━━┻━━━━━━━━━━┓ ┏━━━┻━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓
		 /// MRRRRRRR RRRRRRRR IIIIIIII IIIIIIII IIIIIIII IIIIIIII IIIIIIII IIIIIIII
		 /// 1        2        3        4        5        6        7        8        byte
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private volatile long headerState;
		 private volatile long _headerState;

		 /// <summary>
		 /// Create muninn page file </summary>
		 /// <param name="file"> original file </param>
		 /// <param name="pageCache"> page cache </param>
		 /// <param name="filePageSize"> file page size </param>
		 /// <param name="swapperFactory"> page cache swapper factory </param>
		 /// <param name="pageCacheTracer"> global page cache tracer </param>
		 /// <param name="pageCursorTracerSupplier"> supplier of thread local (transaction local) page cursor tracer that will provide
		 /// thread local page cache statistics </param>
		 /// <param name="versionContextSupplier"> supplier of thread local (transaction local) version context that will provide
		 /// access to thread local version context </param>
		 /// <param name="createIfNotExists"> should create file if it does not exists </param>
		 /// <param name="truncateExisting"> should truncate file if it exists </param>
		 /// <param name="noChannelStriping"> when true, overrides channel striping behaviour,
		 /// setting it to a single channel per mapped file. </param>
		 /// <exception cref="IOException"> If the <seealso cref="PageSwapper"/> could not be created. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: MuninnPagedFile(java.io.File file, MuninnPageCache pageCache, int filePageSize, org.neo4j.io.pagecache.PageSwapperFactory swapperFactory, org.neo4j.io.pagecache.tracing.PageCacheTracer pageCacheTracer, org.neo4j.io.pagecache.tracing.cursor.PageCursorTracerSupplier pageCursorTracerSupplier, org.neo4j.io.pagecache.tracing.cursor.context.VersionContextSupplier versionContextSupplier, boolean createIfNotExists, boolean truncateExisting, boolean noChannelStriping) throws java.io.IOException
		 internal MuninnPagedFile( File file, MuninnPageCache pageCache, int filePageSize, PageSwapperFactory swapperFactory, PageCacheTracer pageCacheTracer, PageCursorTracerSupplier pageCursorTracerSupplier, VersionContextSupplier versionContextSupplier, bool createIfNotExists, bool truncateExisting, bool noChannelStriping ) : base( pageCache.Pages )
		 {
			  this.PageCache = pageCache;
			  this.FilePageSize = filePageSize;
			  this._cursorFactory = new CursorFactory( this, pageCursorTracerSupplier, pageCacheTracer, versionContextSupplier );
			  this._pageCacheTracer = pageCacheTracer;
			  this.PageFaultLatches = new LatchMap();

			  // The translation table is an array of arrays of integers that are either UNMAPPED_TTE, or the id of a page in
			  // the page list. The table only grows the outer array, and all the inner "chunks" all stay the same size. This
			  // means that pages can be addressed with simple bit-wise operations on the filePageId. Eviction sets slots
			  // to UNMAPPED_TTE with volatile writes. Page faults guard their target entries via the LatchMap, and overwrites
			  // the UNMAPPED_TTE value with the new page id, with a volatile write, and then finally releases their latch
			  // from the LatchMap. The LatchMap will ensure that only a single thread will fault a page at a time. However,
			  // after a latch has been taken, the thread must double-check the entry to make sure that it did not race with
			  // another thread to fault in the page – this is called double-check locking. Look-ups use volatile reads of the
			  // slots. If a look-up finds UNMAPPED_TTE, it will attempt to page fault. If the LatchMap returns null, then
			  // someone else might already be faulting in that page. The LatchMap will wait for the existing latch to be
			  // released, before returning null. Thus the thread can retry the lookup immediately. If a look-up finds that it
			  // is out of bounds of the translation table, it resizes the table by first taking the resize lock, then
			  // verifying that the given filePageId is still out of bounds, then creates a new and larger outer array, then
			  // copies over the existing inner arrays, fills the remaining outer array slots with more inner arrays, in turn
			  // filled with UNMAPPED_TTE values, and then finally assigns the new outer array to the translationTable field
			  // and releases the resize lock.
			  PageEvictionCallback onEviction = this.evictPage;
			  Swapper = swapperFactory.CreatePageSwapper( file, filePageSize, onEviction, createIfNotExists, noChannelStriping );
			  if ( truncateExisting )
			  {
					Swapper.truncate();
			  }
			  long lastPageId = Swapper.LastPageId;

			  int initialChunks = 1 + ComputeChunkId( lastPageId );
			  int[][] tt = new int[initialChunks][];
			  for ( int i = 0; i < initialChunks; i++ )
			  {
					tt[i] = NewChunk();
			  }
			  TranslationTable = tt;

			  InitialiseLastPageId( lastPageId );
			  this.SwapperId = Swappers.allocate( Swapper );
		 }

		 public override string ToString()
		 {
			  return this.GetType().Name + "[" + Swapper.file() + "]";
		 }

		 public override PageCursor Io( long pageId, int pfFlags )
		 {
			  int lockFlags = pfFlags & _pfLockMask;
			  MuninnPageCursor cursor;
			  if ( lockFlags == Org.Neo4j.Io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK )
			  {
					cursor = _cursorFactory.takeReadCursor( pageId, pfFlags );
			  }
			  else if ( lockFlags == Org.Neo4j.Io.pagecache.PagedFile_Fields.PfSharedWriteLock )
			  {
					cursor = _cursorFactory.takeWriteCursor( pageId, pfFlags );
			  }
			  else
			  {
					throw WrongLocksArgument( lockFlags );
			  }

			  cursor.Rewind();
			  return cursor;
		 }

		 private System.ArgumentException WrongLocksArgument( int lockFlags )
		 {
			  if ( lockFlags == 0 )
			  {
					return new System.ArgumentException( "Must specify either PF_SHARED_WRITE_LOCK or PF_SHARED_READ_LOCK" );
			  }
			  else
			  {
					return new System.ArgumentException( "Cannot specify both PF_SHARED_WRITE_LOCK and PF_SHARED_READ_LOCK" );
			  }
		 }

		 public override int PageSize()
		 {
			  return FilePageSize;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long fileSize() throws org.neo4j.io.pagecache.impl.FileIsNotMappedException
		 public override long FileSize()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long lastPageId = getLastPageId();
			  long lastPageId = LastPageId;
			  if ( lastPageId < 0 )
			  {
					return 0L;
			  }
			  return ( lastPageId + 1 ) * PageSize();
		 }

		 public override File File()
		 {
			  return Swapper.file();
		 }

		 public override void Close()
		 {
			  PageCache.unmap( this );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void closeSwapper() throws java.io.IOException
		 internal void CloseSwapper()
		 {
			  // We don't set closeStackTrace in close(), because the reference count may keep the file open.
			  // But if we get here, to close the swapper, then we are definitely unmapping!
			  _closeStackTrace = new Exception( "tracing paged file closing" );

			  if ( !_deleteOnClose )
			  {
					Swapper.close();
			  }
			  else
			  {
					Swapper.closeAndDelete();
			  }
			  if ( Swappers.free( SwapperId ) )
			  {
					// We need to do a vacuum of the cache, fully evicting all pages that have freed swapper ids.
					// We cannot reuse those swapper ids until there are no more pages using them.
					PageCache.vacuum( Swappers );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flushAndForce() throws java.io.IOException
		 public override void FlushAndForce()
		 {
			  FlushAndForce( Org.Neo4j.Io.pagecache.IOLimiter_Fields.Unlimited );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flushAndForce(org.neo4j.io.pagecache.IOLimiter limiter) throws java.io.IOException
		 public override void FlushAndForce( IOLimiter limiter )
		 {
			  if ( limiter == null )
			  {
					throw new System.ArgumentException( "IOPSLimiter cannot be null" );
			  }
			  using ( MajorFlushEvent flushEvent = _pageCacheTracer.beginFileFlush( Swapper ) )
			  {
					FlushAndForceInternal( flushEvent.FlushEventOpportunity(), false, limiter );
					SyncDevice();
			  }
			  PageCache.clearEvictorException();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void flushAndForceForClose() throws java.io.IOException
		 internal void FlushAndForceForClose()
		 {
			  if ( _deleteOnClose )
			  {
					// No need to spend time flushing data to a file we're going to delete anyway.
					// However, we still have to mark the dirtied pages as clean since evicting would otherwise try to flush
					// these pages, and would fail because the file is closed, and we cannot allow that to happen.
					MarkAllDirtyPagesAsClean();
					return;
			  }
			  using ( MajorFlushEvent flushEvent = _pageCacheTracer.beginFileFlush( Swapper ) )
			  {
					FlushAndForceInternal( flushEvent.FlushEventOpportunity(), true, Org.Neo4j.Io.pagecache.IOLimiter_Fields.Unlimited );
					SyncDevice();
			  }
			  PageCache.clearEvictorException();
		 }

		 private void MarkAllDirtyPagesAsClean()
		 {
			  long filePageId = -1; // Start at -1 because we increment at the *start* of the chunk-loop iteration.
			  int[][] tt = this.TranslationTable;
			  foreach ( int[] chunk in tt )
			  {
					for ( int i = 0; i < chunk.Length; i++ )
					{
						 filePageId++;
						 long offset = ComputeChunkOffset( filePageId );

						 // We might race with eviction, but we also mustn't miss a dirty page, so we loop until we succeed
						 // in getting a lock on all available pages.
						 for ( ;; )
						 {
							  int pageId = UnsafeUtil.getIntVolatile( chunk, offset );
							  if ( pageId != UNMAPPED_TTE )
							  {
									long pageRef = Deref( pageId );
									long stamp = TryOptimisticReadLock( pageRef );
									if ( ( !IsModified( pageRef ) ) && ValidateReadLock( pageRef, stamp ) )
									{
										 // We got a valid read, and the page isn't dirty, so we skip it.
										 goto chunkLoopContinue;
									}

									if ( !TryExclusiveLock( pageRef ) )
									{
										 continue;
									}
									if ( IsBoundTo( pageRef, SwapperId, filePageId ) && IsModified( pageRef ) )
									{
										 // The page is still bound to the expected file and file page id after we locked it,
										 // so we didn't race with eviction and faulting, and the page is dirty.
										 ExplicitlyMarkPageUnmodifiedUnderExclusiveLock( pageRef );
										 UnlockExclusive( pageRef );
										 goto chunkLoopContinue;
									}
							  }
							  // There was no page at this entry in the table. Continue to the next entry.
							  goto chunkLoopContinue;
						 }
						chunkLoopContinue:;
					}
					chunkLoopBreak:;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void flushAndForceInternal(org.neo4j.io.pagecache.tracing.FlushEventOpportunity flushes, boolean forClosing, org.neo4j.io.pagecache.IOLimiter limiter) throws java.io.IOException
		 internal void FlushAndForceInternal( FlushEventOpportunity flushes, bool forClosing, IOLimiter limiter )
		 {
			  try
			  {
					DoFlushAndForceInternal( flushes, forClosing, limiter );
			  }
			  catch ( ClosedChannelException e )
			  {
					e.addSuppressed( _closeStackTrace );
					throw e;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void doFlushAndForceInternal(org.neo4j.io.pagecache.tracing.FlushEventOpportunity flushes, boolean forClosing, org.neo4j.io.pagecache.IOLimiter limiter) throws java.io.IOException
		 private void DoFlushAndForceInternal( FlushEventOpportunity flushes, bool forClosing, IOLimiter limiter )
		 {
			  // TODO it'd be awesome if, on Linux, we'd call sync_file_range(2) instead of fsync
			  long[] pages = new long[_translationTableChunkSize];
			  long[] flushStamps = forClosing ? null : new long[_translationTableChunkSize];
			  long[] bufferAddresses = new long[_translationTableChunkSize];
			  long filePageId = -1; // Start at -1 because we increment at the *start* of the chunk-loop iteration.
			  long limiterStamp = Org.Neo4j.Io.pagecache.IOLimiter_Fields.INITIAL_STAMP;
			  int[][] tt = this.TranslationTable;
			  foreach ( int[] chunk in tt )
			  {
					// TODO Look into if we can tolerate flushing a few clean pages if it means we can use larger vectors.
					// TODO The clean pages in question must still be loaded, though. Otherwise we'll end up writing
					// TODO garbage to the file.
					int pagesGrabbed = 0;
					for ( int i = 0; i < chunk.Length; i++ )
					{
						 filePageId++;
						 long offset = ComputeChunkOffset( filePageId );

						 // We might race with eviction, but we also mustn't miss a dirty page, so we loop until we succeed
						 // in getting a lock on all available pages.
						 for ( ;; )
						 {
							  int pageId = UnsafeUtil.getIntVolatile( chunk, offset );
							  if ( pageId != UNMAPPED_TTE )
							  {
									long pageRef = Deref( pageId );
									long stamp = TryOptimisticReadLock( pageRef );
									if ( ( !IsModified( pageRef ) ) && ValidateReadLock( pageRef, stamp ) )
									{
										 break;
									}

									long flushStamp = 0;
									if ( !( forClosing ? TryExclusiveLock( pageRef ) : ( ( flushStamp = TryFlushLock( pageRef ) ) != 0 ) ) )
									{
										 continue;
									}
									if ( IsBoundTo( pageRef, SwapperId, filePageId ) && IsModified( pageRef ) )
									{
										 // The page is still bound to the expected file and file page id after we locked it,
										 // so we didn't race with eviction and faulting, and the page is dirty.
										 // So we add it to our IO vector.
										 pages[pagesGrabbed] = pageRef;
										 if ( !forClosing )
										 {
											  flushStamps[pagesGrabbed] = flushStamp;
										 }
										 bufferAddresses[pagesGrabbed] = GetAddress( pageRef );
										 pagesGrabbed++;
										 goto chunkLoopContinue;
									}
									else if ( forClosing )
									{
										 UnlockExclusive( pageRef );
									}
									else
									{
										 UnlockFlush( pageRef, flushStamp, false );
									}
							  }
							  break;
						 }
						 if ( pagesGrabbed > 0 )
						 {
							  VectoredFlush( pages, bufferAddresses, flushStamps, pagesGrabbed, flushes, forClosing );
							  limiterStamp = limiter.MaybeLimitIO( limiterStamp, pagesGrabbed, this );
							  pagesGrabbed = 0;
						 }
						chunkLoopContinue:;
					}
					chunkLoopBreak:
					if ( pagesGrabbed > 0 )
					{
						 VectoredFlush( pages, bufferAddresses, flushStamps, pagesGrabbed, flushes, forClosing );
						 limiterStamp = limiter.MaybeLimitIO( limiterStamp, pagesGrabbed, this );
					}
			  }

			  Swapper.force();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void vectoredFlush(long[] pages, long[] bufferAddresses, long[] flushStamps, int pagesGrabbed, org.neo4j.io.pagecache.tracing.FlushEventOpportunity flushOpportunity, boolean forClosing) throws java.io.IOException
		 private void VectoredFlush( long[] pages, long[] bufferAddresses, long[] flushStamps, int pagesGrabbed, FlushEventOpportunity flushOpportunity, bool forClosing )
		 {
			  FlushEvent flush = null;
			  bool successful = false;
			  try
			  {
					// Write the pages vector
					long firstPageRef = pages[0];
					long startFilePageId = GetFilePageId( firstPageRef );
					flush = flushOpportunity.BeginFlush( startFilePageId, ToId( firstPageRef ), Swapper );
					long bytesWritten = Swapper.write( startFilePageId, bufferAddresses, 0, pagesGrabbed );

					// Update the flush event
					flush.AddBytesWritten( bytesWritten );
					flush.AddPagesFlushed( pagesGrabbed );
					flush.Done();
					successful = true;

					// There are now 0 'grabbed' pages
			  }
			  catch ( IOException ioe )
			  {
					if ( flush != null )
					{
						 flush.Done( ioe );
					}
					throw ioe;
			  }
			  finally
			  {
					// Always unlock all the pages in the vector
					if ( forClosing )
					{
						 for ( int i = 0; i < pagesGrabbed; i++ )
						 {
							  long pageRef = pages[i];
							  if ( successful )
							  {
									ExplicitlyMarkPageUnmodifiedUnderExclusiveLock( pageRef );
							  }
							  UnlockExclusive( pageRef );
						 }
					}
					else
					{
						 for ( int i = 0; i < pagesGrabbed; i++ )
						 {
							  UnlockFlush( pages[i], flushStamps[i], successful );
						 }
					}
			  }
		 }

		 internal bool FlushLockedPage( long pageRef, long filePageId )
		 {
			  bool success = false;
			  using ( MajorFlushEvent flushEvent = _pageCacheTracer.beginFileFlush( Swapper ) )
			  {
					FlushEvent flush = flushEvent.FlushEventOpportunity().beginFlush(filePageId, ToId(pageRef), Swapper);
					long address = GetAddress( pageRef );
					try
					{
						 long bytesWritten = Swapper.write( filePageId, address );
						 flush.AddBytesWritten( bytesWritten );
						 flush.AddPagesFlushed( 1 );
						 flush.Done();
						 success = true;
					}
					catch ( IOException e )
					{
						 flush.Done( e );
					}
			  }
			  return success;
		 }

		 private void SyncDevice()
		 {
			  PageCache.syncDevice();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flush() throws java.io.IOException
		 public override void Flush()
		 {
			  Swapper.force();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long getLastPageId() throws org.neo4j.io.pagecache.impl.FileIsNotMappedException
		 public long LastPageId
		 {
			 get
			 {
				  long state = HeaderState;
				  if ( RefCountOf( state ) == 0 )
				  {
						throw FileIsNotMappedException();
				  }
				  return state & HEADER_STATE_LAST_PAGE_ID_MASK;
			 }
		 }

		 private FileIsNotMappedException FileIsNotMappedException()
		 {
			  FileIsNotMappedException exception = new FileIsNotMappedException( File() );
			  Exception closedBy = _closeStackTrace;
			  if ( closedBy != null )
			  {
					exception.addSuppressed( closedBy );
			  }
			  return exception;
		 }

		 private long HeaderState
		 {
			 get
			 {
				  return UnsafeUtil.getLongVolatile( this, _headerStateOffset );
			 }
		 }

		 private long RefCountOf( long state )
		 {
			  return ( long )( ( ulong )( state & HEADER_STATE_REF_COUNT_MASK ) >> HEADER_STATE_REF_COUNT_SHIFT );
		 }

		 private void InitialiseLastPageId( long lastPageIdFromFile )
		 {
			  if ( lastPageIdFromFile < 0 )
			  {
					// MIN_VALUE only has the sign bit raised, and the rest of the bits are zeros.
					UnsafeUtil.putLongVolatile( this, _headerStateOffset, long.MinValue );
			  }
			  else
			  {
					UnsafeUtil.putLongVolatile( this, _headerStateOffset, lastPageIdFromFile );
			  }
		 }

		 /// <summary>
		 /// Make sure that the lastPageId is at least the given pageId
		 /// </summary>
		 internal void IncreaseLastPageIdTo( long newLastPageId )
		 {
			  long current;
			  long update;
			  long lastPageId;
			  do
			  {
					current = HeaderState;
					update = newLastPageId + ( current & HEADER_STATE_REF_COUNT_MASK );
					lastPageId = current & HEADER_STATE_LAST_PAGE_ID_MASK;
			  } while ( lastPageId < newLastPageId && !UnsafeUtil.compareAndSwapLong( this, _headerStateOffset, current, update ) );
		 }

		 /// <summary>
		 /// Atomically increment the reference count for this mapped file.
		 /// </summary>
		 internal void IncrementRefCount()
		 {
			  long current;
			  long update;
			  do
			  {
					current = HeaderState;
					long count = RefCountOf( current ) + 1;
					if ( count > HEADER_STATE_REF_COUNT_MAX )
					{
						 throw new System.InvalidOperationException( "Cannot map file because reference counter would overflow. " + "Maximum reference count is " + HEADER_STATE_REF_COUNT_MAX + ". " + "File is " + Swapper.file().AbsolutePath );
					}
					update = ( current & HEADER_STATE_LAST_PAGE_ID_MASK ) + ( count << HEADER_STATE_REF_COUNT_SHIFT );
			  } while ( !UnsafeUtil.compareAndSwapLong( this, _headerStateOffset, current, update ) );
		 }

		 /// <summary>
		 /// Atomically decrement the reference count. Returns true if this was the
		 /// last reference.
		 /// </summary>
		 internal bool DecrementRefCount()
		 {
			  long current;
			  long update;
			  long count;
			  do
			  {
					current = HeaderState;
					count = RefCountOf( current ) - 1;
					if ( count < 0 )
					{
						 throw new System.InvalidOperationException( "File has already been closed and unmapped. " + "It cannot be closed any further." );
					}
					update = ( current & HEADER_STATE_LAST_PAGE_ID_MASK ) + ( count << HEADER_STATE_REF_COUNT_SHIFT );
			  } while ( !UnsafeUtil.compareAndSwapLong( this, _headerStateOffset, current, update ) );
			  return count == 0;
		 }

		 /// <summary>
		 /// Get the current ref-count. Useful for checking if this PagedFile should
		 /// be considered unmapped.
		 /// </summary>
		 internal int RefCount
		 {
			 get
			 {
				  return ( int ) RefCountOf( HeaderState );
			 }
		 }

		 internal void MarkDeleteOnClose( bool deleteOnClose )
		 {
			  this._deleteOnClose |= deleteOnClose;
		 }

		 /// <summary>
		 /// Grab a free page for the purpose of page faulting. Possibly blocking if
		 /// none are immediately available. </summary>
		 /// <param name="faultEvent"> The trace event for the current page fault. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long grabFreeAndExclusivelyLockedPage(org.neo4j.io.pagecache.tracing.PageFaultEvent faultEvent) throws java.io.IOException
		 internal long GrabFreeAndExclusivelyLockedPage( PageFaultEvent faultEvent )
		 {
			  return PageCache.grabFreeAndExclusivelyLockedPage( faultEvent );
		 }

		 /// <summary>
		 /// Remove the mapping of the given filePageId from the translation table, and return the evicted page object. </summary>
		 /// <param name="filePageId"> The id of the file page to evict. </param>
		 private void EvictPage( long filePageId )
		 {
			  int chunkId = ComputeChunkId( filePageId );
			  long chunkOffset = ComputeChunkOffset( filePageId );
			  int[] chunk = TranslationTable[chunkId];

			  int mappedPageId = UnsafeUtil.getIntVolatile( chunk, chunkOffset );
			  long pageRef = Deref( mappedPageId );
			  HighestEvictedTransactionId = GetAndResetLastModifiedTransactionId( pageRef );
			  UnsafeUtil.putIntVolatile( chunk, chunkOffset, UNMAPPED_TTE );
		 }

		 private long HighestEvictedTransactionId
		 {
			 set
			 {
				  UnsafeUtil.compareAndSetMaxLong( this, _evictedTransactionIdOffset, value );
			 }
			 get
			 {
				  return UnsafeUtil.getLongVolatile( this, _evictedTransactionIdOffset );
			 }
		 }


		 /// <summary>
		 /// Expand the translation table such that it can include at least the given chunkId. </summary>
		 /// <param name="maxChunkId"> The new translation table must be big enough to include at least this chunkId. </param>
		 /// <returns> A reference to the expanded transaction table. </returns>
		 internal int[][] ExpandCapacity( int maxChunkId )
		 {
			 lock ( this )
			 {
				  int[][] tt = TranslationTable;
				  if ( tt.Length <= maxChunkId )
				  {
						int newLength = ComputeNewRootTableLength( maxChunkId );
						int[][] ntt = new int[newLength][];
						Array.Copy( tt, 0, ntt, 0, tt.Length );
						for ( int i = tt.Length; i < ntt.Length; i++ )
						{
							 ntt[i] = NewChunk();
						}
						tt = ntt;
						TranslationTable = tt;
				  }
				  return tt;
			 }
		 }

		 private static int[] NewChunk()
		 {
			  int[] chunk = new int[_translationTableChunkSize];
			  Arrays.fill( chunk, UNMAPPED_TTE );
			  return chunk;
		 }

		 private int ComputeNewRootTableLength( int maxChunkId )
		 {
			  // Grow by approx. 10% but always by at least one full chunk.
			  return 1 + ( int )( maxChunkId * 1.1 );
		 }

		 internal static int ComputeChunkId( long filePageId )
		 {
			  return ( int )( ( long )( ( ulong )filePageId >> _translationTableChunkSizePower ) );
		 }

		 internal static long ComputeChunkOffset( long filePageId )
		 {
			  int index = ( int )( filePageId & _translationTableChunkSizeMask );
			  return UnsafeUtil.arrayOffset( index, _translationTableChunkArrayBase, _translationTableChunkArrayScale );
		 }
	}

}