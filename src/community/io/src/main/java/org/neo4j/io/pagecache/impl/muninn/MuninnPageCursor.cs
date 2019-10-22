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
namespace Neo4Net.Io.pagecache.impl.muninn
{

	using PageFaultEvent = Neo4Net.Io.pagecache.tracing.PageFaultEvent;
	using PinEvent = Neo4Net.Io.pagecache.tracing.PinEvent;
	using PageCursorTracer = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracer;
	using VersionContext = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContext;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using UnsafeUtil = Neo4Net.@unsafe.Impl.Internal.Dragons.UnsafeUtil;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.pagecache.PagedFile_Fields.PF_EAGER_FLUSH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.pagecache.PagedFile_Fields.PF_NO_FAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.pagecache.PagedFile_Fields.PF_SHARED_WRITE_LOCK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.pagecache.impl.muninn.MuninnPagedFile.UNMAPPED_TTE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.util.FeatureToggles.flag;

	internal abstract class MuninnPageCursor : PageCursor
	{
		 private static readonly bool _usePreciseCursorErrorStackTraces = flag( typeof( MuninnPageCursor ), "usePreciseCursorErrorStackTraces", false );

		 private static readonly bool _boundsCheck = flag( typeof( MuninnPageCursor ), "boundsCheck", true );

		 // Size of the respective primitive types in bytes.
		 private static readonly int _sizeOfByte = Byte.BYTES;
		 private static readonly int _sizeOfShort = Short.BYTES;
		 private static readonly int _sizeOfInt = Integer.BYTES;
		 private static readonly int _sizeOfLong = Long.BYTES;

		 private readonly long _victimPage;
		 private readonly PageCursorTracer _tracer;
		 protected internal MuninnPagedFile PagedFile;
		 protected internal PageSwapper Swapper;
		 protected internal int SwapperId;
		 protected internal long PinnedPageRef;
		 protected internal PinEvent PinEvent;
		 protected internal long PageId;
		 protected internal int PfFlags;
		 protected internal bool EagerFlush;
		 protected internal bool NoFault;
		 protected internal bool NoGrow;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal long CurrentPageIdConflict;
		 protected internal long NextPageId;
		 protected internal MuninnPageCursor LinkedCursor;
		 private long _pointer;
		 private int _pageSize;
		 private int _filePageSize;
		 protected internal readonly VersionContextSupplier VersionContextSupplier;
		 private int _offset;
		 private int _mark;
		 private bool _outOfBounds;
		 private bool _isLinkedCursor;
		 // This is a String with the exception message if usePreciseCursorErrorStackTraces is false, otherwise it is a
		 // CursorExceptionWithPreciseStackTrace with the message and stack trace pointing more or less directly at the
		 // offending code.
		 private object _cursorException;

		 internal MuninnPageCursor( long victimPage, PageCursorTracer tracer, VersionContextSupplier versionContextSupplier )
		 {
			  this._victimPage = victimPage;
			  this._pointer = victimPage;
			  this._tracer = tracer;
			  this.VersionContextSupplier = versionContextSupplier;
		 }

		 internal void Initialize( MuninnPagedFile pagedFile, long pageId, int pfFlags )
		 {
			  this.Swapper = pagedFile.Swapper;
			  this.SwapperId = pagedFile.SwapperId;
			  this._filePageSize = pagedFile.FilePageSize;
			  this.PagedFile = pagedFile;
			  this.PageId = pageId;
			  this.PfFlags = pfFlags;
			  this.EagerFlush = IsFlagRaised( pfFlags, PF_EAGER_FLUSH );
			  this.NoFault = IsFlagRaised( pfFlags, PF_NO_FAULT );
			  this.NoGrow = NoFault | IsFlagRaised( pfFlags, Neo4Net.Io.pagecache.PagedFile_Fields.PfNoGrow );
		 }

		 private bool IsFlagRaised( int flagSet, int flag )
		 {
			  return ( flagSet & flag ) == flag;
		 }

		 public override void Rewind()
		 {
			  NextPageId = PageId;
			  CurrentPageIdConflict = UNBOUND_PAGE_ID;
		 }

		 public void Reset( long pageRef )
		 {
			  this.PinnedPageRef = pageRef;
			  this._offset = 0;
			  this._pointer = PagedFile.getAddress( pageRef );
			  this._pageSize = _filePageSize;
			  PinEvent.CachePageId = PagedFile.toId( pageRef );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final boolean next(long pageId) throws java.io.IOException
		 public override bool Next( long pageId )
		 {
			  if ( CurrentPageIdConflict == pageId )
			  {
					VerifyContext();
					return true;
			  }
			  NextPageId = pageId;
			  return Next();
		 }

		 internal virtual void VerifyContext()
		 {
			  VersionContext versionContext = VersionContextSupplier.VersionContext;
			  long lastClosedTransactionId = versionContext.LastClosedTransactionId();
			  if ( lastClosedTransactionId == long.MaxValue )
			  {
					return;
			  }
			  if ( IsPotentiallyReadingDirtyData( lastClosedTransactionId ) )
			  {
					versionContext.MarkAsDirty();
			  }
		 }

		 /// <summary>
		 /// We reading potentially dirty data in case if our page last modification version is higher then
		 /// requested lastClosedTransactionId; or for this page file we already evict some page with version that is higher
		 /// then requested lastClosedTransactionId. In this case we can't be sure that data of current page satisfying
		 /// visibility requirements and we pessimistically will assume that we reading dirty data. </summary>
		 /// <param name="lastClosedTransactionId"> last closed transaction id </param>
		 /// <returns> true in case if we reading potentially dirty data for requested lastClosedTransactionId. </returns>
		 private bool IsPotentiallyReadingDirtyData( long lastClosedTransactionId )
		 {
			  return PagedFile.getLastModifiedTxId( PinnedPageRef ) > lastClosedTransactionId || PagedFile.HighestEvictedTransactionId > lastClosedTransactionId;
		 }

		 public override void Close()
		 {
			  if ( PagedFile == null )
			  {
					return; // already closed
			  }
			  CloseLinks( this );
		 }

		 private void CloseLinks( MuninnPageCursor cursor )
		 {
			  while ( cursor != null && cursor.PagedFile != null )
			  {
					cursor.UnpinCurrentPage();
					// We null out the pagedFile field to allow it and its (potentially big) translation table to be garbage
					// collected when the file is unmapped, since the cursors can stick around in thread local caches, etc.
					cursor.PagedFile = null;
					cursor = cursor.LinkedCursor;
			  }
		 }

		 private void CloseLinkedCursorIfAny()
		 {
			  if ( LinkedCursor != null )
			  {
					CloseLinks( LinkedCursor );
			  }
		 }

		 public override PageCursor OpenLinkedCursor( long pageId )
		 {
			  CloseLinkedCursorIfAny();
			  MuninnPagedFile pf = PagedFile;
			  if ( pf == null )
			  {
					// This cursor has been closed
					throw new System.InvalidOperationException( "Cannot open linked cursor on closed page cursor" );
			  }
			  if ( LinkedCursor != null )
			  {
					LinkedCursor.initialise( pf, pageId, PfFlags );
					LinkedCursor.rewind();
			  }
			  else
			  {
					LinkedCursor = ( MuninnPageCursor ) pf.Io( pageId, PfFlags );
					LinkedCursor.isLinkedCursor = true;
			  }
			  return LinkedCursor;
		 }

		 /// <summary>
		 /// Must be called by <seealso cref="unpinCurrentPage()"/>.
		 /// </summary>
		 internal virtual void ClearPageCursorState()
		 {
			  // We don't need to clear the pointer field, because setting the page size to 0 will make all future accesses
			  // go out of bounds, which in turn imply that they will always end up accessing the victim page anyway.
			  ClearPageReference();
			  CurrentPageIdConflict = UNBOUND_PAGE_ID;
			  _cursorException = null;
		 }

		 internal virtual void ClearPageReference()
		 {
			  // Make all future bounds checks fail, and send future accesses to the victim page.
			  _pageSize = 0;
			  // Decouple us from the memory page, so we avoid messing with the page meta-data.
			  PinnedPageRef = 0;
		 }

		 public override sealed long CurrentPageId
		 {
			 get
			 {
				  return CurrentPageIdConflict;
			 }
		 }

		 public override sealed int CurrentPageSize
		 {
			 get
			 {
				  return CurrentPageIdConflict == UNBOUND_PAGE_ID ? UNBOUND_PAGE_SIZE : PagedFile.pageSize();
			 }
		 }

		 public override sealed File CurrentFile
		 {
			 get
			 {
				  return CurrentPageIdConflict == UNBOUND_PAGE_ID ? null : PagedFile.file();
			 }
		 }

		 /// <summary>
		 /// Pin the desired file page to this cursor, page faulting it into memory if it isn't there already. </summary>
		 /// <param name="filePageId"> The file page id we want to pin this cursor to. </param>
		 /// <param name="writeLock"> 'true' if we will be taking a write lock on the page as part of the pin. </param>
		 /// <exception cref="IOException"> if anything goes wrong with the pin, most likely during a page fault. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void pin(long filePageId, boolean writeLock) throws java.io.IOException
		 protected internal virtual void Pin( long filePageId, bool writeLock )
		 {
			  PinEvent = _tracer.beginPin( writeLock, filePageId, Swapper );
			  int chunkId = MuninnPagedFile.ComputeChunkId( filePageId );
			  // The chunkOffset is the addressing offset into the chunk array object for the relevant array slot. Using
			  // this, we can access the array slot with Unsafe.
			  long chunkOffset = MuninnPagedFile.ComputeChunkOffset( filePageId );
			  int[][] tt = PagedFile.translationTable;
			  if ( tt.Length <= chunkId )
			  {
					tt = ExpandTranslationTableCapacity( chunkId );
			  }
			  int[] chunk = tt[chunkId];

			  // Now, if the reference in the chunk slot is a latch, we wait on it and look up again (in a loop, since the
			  // page might get evicted right after the page fault completes). If we find a page, we lock it and check its
			  // binding (since it might get evicted and faulted into something else in the time between our look up and
			  // our locking of the page). If the reference is null or it referred to a page that had wrong bindings, we CAS
			  // in a latch. If that CAS succeeds, we page fault, set the slot to the faulted in page and open the latch.
			  // If the CAS failed, we retry the look up and start over from the top.
			  for ( ;; )
			  {
					int mappedPageId = UnsafeUtil.getIntVolatile( chunk, chunkOffset );
					if ( mappedPageId != UNMAPPED_TTE )
					{
						 // We got *a* page, but we might be racing with eviction. To cope with that, we have to take some
						 // kind of lock on the page, and check that it is indeed bound to what we expect. If not, then it has
						 // been evicted, and possibly even page faulted into something else. In this case, we discard the
						 // item and try again, as the eviction thread would have set the chunk array slot to null.
						 long pageRef = PagedFile.deref( mappedPageId );
						 bool locked = TryLockPage( pageRef );
						 if ( locked & PagedFile.isBoundTo( pageRef, SwapperId, filePageId ) )
						 {
							  PinCursorToPage( pageRef, filePageId, Swapper );
							  PinEvent.hit();
							  return;
						 }
						 if ( locked )
						 {
							  UnlockPage( pageRef );
						 }
					}
					else
					{
						 if ( UncommonPin( filePageId, chunkOffset, chunk ) )
						 {
							  return;
						 }
					}
			  }
		 }

		 private int[][] ExpandTranslationTableCapacity( int chunkId )
		 {
			  return PagedFile.expandCapacity( chunkId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean uncommonPin(long filePageId, long chunkOffset, int[] chunk) throws java.io.IOException
		 private bool UncommonPin( long filePageId, long chunkOffset, int[] chunk )
		 {
			  if ( NoFault )
			  {
					// The only page state that needs to be cleared is the currentPageId, since it was set prior to pin.
					CurrentPageIdConflict = UNBOUND_PAGE_ID;
					return true;
			  }
			  // Looks like there's no mapping, so we'd like to do a page fault.
			  LatchMap.Latch latch = PagedFile.pageFaultLatches.takeOrAwaitLatch( filePageId );
			  if ( latch != null )
			  {
					// We managed to inject our latch, so we now own the right to perform the page fault. We also
					// have a duty to eventually release and remove the latch, no matter what happens now.
					// However, we first have to double-check that a page fault did not complete in-between our initial
					// check in the translation table, and us getting a latch.
					if ( UnsafeUtil.getIntVolatile( chunk, chunkOffset ) == UNMAPPED_TTE )
					{
						 // Sweet, we didn't race with any other fault on this translation table entry.
						 long pageRef = PageFault( filePageId, Swapper, chunkOffset, chunk, latch );
						 PinCursorToPage( pageRef, filePageId, Swapper );
						 return true;
					}
					// Oops, looks like we raced with another page fault on this file page.
					// Let's release our latch and retry the pin.
					latch.Release();
			  }
			  // We found a latch, so someone else is already doing a page fault for this page.
			  // The `takeOrAwaitLatch` already waited for this latch to be released on our behalf,
			  // so now we just have to do another iteration of the loop to see what's in the translation table now.
			  return false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long pageFault(long filePageId, org.Neo4Net.io.pagecache.PageSwapper swapper, long chunkOffset, int[] chunk, LatchMap.Latch latch) throws java.io.IOException
		 private long PageFault( long filePageId, PageSwapper swapper, long chunkOffset, int[] chunk, LatchMap.Latch latch )
		 {
			  // We are page faulting. This is a critical time, because we currently have the given latch in the chunk array
			  // slot that we are faulting into. We MUST make sure to release that latch, and remove it from the chunk, no
			  // matter what happens. Otherwise other threads will get stuck waiting forever for our page fault to finish.
			  // If we manage to get a free page to fault into, then we will also be taking a write lock on that page, to
			  // protect it against concurrent eviction as we assigning a binding to the page. If anything goes wrong, then
			  // we must make sure to release that write lock as well.
			  PageFaultEvent faultEvent = PinEvent.beginPageFault();
			  long pageRef;
			  try
			  {
					// The grabFreePage method might throw.
					pageRef = PagedFile.grabFreeAndExclusivelyLockedPage( faultEvent );

					// We got a free page, and we know that we have race-free access to it. Well, it's not entirely race
					// free, because other paged files might have it in their translation tables (or rather, their reads of
					// their translation tables might race with eviction) and try to pin it.
					// However, they will all fail because when they try to pin, because the page will be exclusively locked
					// and possibly bound to our page.
			  }
			  catch ( Exception throwable )
			  {
					// Make sure to unstuck the page fault latch.
					AbortPageFault( throwable, chunk, chunkOffset, latch, faultEvent );
					throw throwable;
			  }
			  try
			  {
					// Check if we're racing with unmapping. We have the page lock
					// here, so the unmapping would have already happened. We do this
					// check before page.fault(), because that would otherwise reopen
					// the file channel.
					AssertPagedFileStillMappedAndGetIdOfLastPage();
					PagedFile.initBuffer( pageRef );
					PagedFile.fault( pageRef, swapper, PagedFile.swapperId, filePageId, faultEvent );
			  }
			  catch ( Exception throwable )
			  {
					// Make sure to unlock the page, so the eviction thread can pick up our trash.
					PagedFile.unlockExclusive( pageRef );
					// Make sure to unstuck the page fault latch.
					AbortPageFault( throwable, chunk, chunkOffset, latch, faultEvent );
					throw throwable;
			  }
			  // Put the page in the translation table before we undo the exclusive lock, as we could otherwise race with
			  // eviction, and the onEvict callback expects to find a MuninnPage object in the table.
			  UnsafeUtil.putIntVolatile( chunk, chunkOffset, PagedFile.toId( pageRef ) );
			  // Once we page has been published to the translation table, we can convert our exclusive lock to whatever we
			  // need for the page cursor.
			  ConvertPageFaultLock( pageRef );
			  latch.Release();
			  faultEvent.Done();
			  return pageRef;
		 }

		 private void AbortPageFault( Exception throwable, int[] chunk, long chunkOffset, LatchMap.Latch latch, PageFaultEvent faultEvent )
		 {
			  UnsafeUtil.putIntVolatile( chunk, chunkOffset, UNMAPPED_TTE );
			  latch.Release();
			  faultEvent.Done( throwable );
			  PinEvent.done();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long assertPagedFileStillMappedAndGetIdOfLastPage() throws org.Neo4Net.io.pagecache.impl.FileIsNotMappedException
		 internal virtual long AssertPagedFileStillMappedAndGetIdOfLastPage()
		 {
			  return PagedFile.LastPageId;
		 }

		 protected internal abstract void UnpinCurrentPage();

		 protected internal abstract void ConvertPageFaultLock( long pageRef );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract void pinCursorToPage(long pageRef, long filePageId, org.Neo4Net.io.pagecache.PageSwapper swapper) throws org.Neo4Net.io.pagecache.impl.FileIsNotMappedException;
		 protected internal abstract void PinCursorToPage( long pageRef, long filePageId, PageSwapper swapper );

		 protected internal abstract bool TryLockPage( long pageRef );

		 protected internal abstract void UnlockPage( long pageRef );

		 // --- IO methods:

		 /// <summary>
		 /// Compute a pointer that guarantees (assuming {@code size} is less than or equal to <seealso cref="pageSize"/>) that the
		 /// page access will be within the bounds of the page.
		 /// This might mean that the pointer won't point to where one might naively expect, but will instead be
		 /// truncated to point within the page. In this case, an overflow has happened and the <seealso cref="outOfBounds"/>
		 /// flag will be raised.
		 /// </summary>
		 private long GetBoundedPointer( int offset, int size )
		 {
			  long p = _pointer;
			  long can = p + offset;
			  if ( _boundsCheck )
			  {
					if ( can + size > p + _pageSize || can < p )
					{
						 _outOfBounds = true;
						 // Return the victim page when we are out of bounds, since at this point we can't tell if the pointer
						 // will be used for reading or writing.
						 return _victimPage;
					}
			  }
			  return can;
		 }

		 /// <summary>
		 /// Compute a pointer that guarantees (assuming {@code size} is less than or equal to <seealso cref="pageSize"/>) that the
		 /// page access will be within the bounds of the page.
		 /// This works just like <seealso cref="getBoundedPointer(int, int)"/>, except in terms of the current <seealso cref="offset"/>.
		 /// This version is faster when applicable, because it can ignore the <em>page underflow</em> case.
		 /// </summary>
		 private long NextBoundedPointer( int size )
		 {
			  int offset = this._offset;
			  long can = _pointer + offset;
			  if ( _boundsCheck )
			  {
					if ( offset + size > _pageSize )
					{
						 _outOfBounds = true;
						 // Return the victim page when we are out of bounds, since at this point we can't tell if the pointer
						 // will be used for reading or writing.
						 return _victimPage;
					}
			  }
			  return can;
		 }

		 public override sealed sbyte Byte
		 {
			 get
			 {
				  long p = NextBoundedPointer( _sizeOfByte );
				  sbyte b = UnsafeUtil.getByte( p );
				  _offset++;
				  return b;
			 }
		 }

		 public override sbyte getByte( int offset )
		 {
			  long p = GetBoundedPointer( offset, _sizeOfByte );
			  return UnsafeUtil.getByte( p );
		 }

		 public override void PutByte( sbyte value )
		 {
			  long p = NextBoundedPointer( _sizeOfByte );
			  UnsafeUtil.putByte( p, value );
			  _offset++;
		 }

		 public override void PutByte( int offset, sbyte value )
		 {
			  long p = GetBoundedPointer( offset, _sizeOfByte );
			  UnsafeUtil.putByte( p, value );
		 }

		 public override long Long
		 {
			 get
			 {
				  long p = NextBoundedPointer( _sizeOfLong );
				  long value = GetLongAt( p );
				  _offset += _sizeOfLong;
				  return value;
			 }
		 }

		 public override long getLong( int offset )
		 {
			  long p = GetBoundedPointer( offset, _sizeOfLong );
			  return GetLongAt( p );
		 }

		 private long GetLongAt( long p )
		 {
			  long value;
			  if ( UnsafeUtil.allowUnalignedMemoryAccess )
			  {
					value = UnsafeUtil.getLong( p );
					if ( !UnsafeUtil.storeByteOrderIsNative )
					{
						 value = Long.reverseBytes( value );
					}
			  }
			  else
			  {
					value = GetLongBigEndian( p );
			  }
			  return value;
		 }

		 private long GetLongBigEndian( long p )
		 {
			  long a = UnsafeUtil.getByte( p ) & 0xFF;
			  long b = UnsafeUtil.getByte( p + 1 ) & 0xFF;
			  long c = UnsafeUtil.getByte( p + 2 ) & 0xFF;
			  long d = UnsafeUtil.getByte( p + 3 ) & 0xFF;
			  long e = UnsafeUtil.getByte( p + 4 ) & 0xFF;
			  long f = UnsafeUtil.getByte( p + 5 ) & 0xFF;
			  long g = UnsafeUtil.getByte( p + 6 ) & 0xFF;
			  long h = UnsafeUtil.getByte( p + 7 ) & 0xFF;
			  return ( a << 56 ) | ( b << 48 ) | ( c << 40 ) | ( d << 32 ) | ( e << 24 ) | ( f << 16 ) | ( g << 8 ) | h;
		 }

		 public override void PutLong( long value )
		 {
			  long p = NextBoundedPointer( _sizeOfLong );
			  PutLongAt( p, value );
			  _offset += _sizeOfLong;
		 }

		 public override void PutLong( int offset, long value )
		 {
			  long p = GetBoundedPointer( offset, _sizeOfLong );
			  PutLongAt( p, value );
		 }

		 private void PutLongAt( long p, long value )
		 {
			  if ( UnsafeUtil.allowUnalignedMemoryAccess )
			  {
					UnsafeUtil.putLong( p, UnsafeUtil.storeByteOrderIsNative ? value : Long.reverseBytes( value ) );
			  }
			  else
			  {
					PutLongBigEndian( value, p );
			  }
		 }

		 private void PutLongBigEndian( long value, long p )
		 {
			  UnsafeUtil.putByte( p, ( sbyte )( value >> 56 ) );
			  UnsafeUtil.putByte( p + 1, ( sbyte )( value >> 48 ) );
			  UnsafeUtil.putByte( p + 2, ( sbyte )( value >> 40 ) );
			  UnsafeUtil.putByte( p + 3, ( sbyte )( value >> 32 ) );
			  UnsafeUtil.putByte( p + 4, ( sbyte )( value >> 24 ) );
			  UnsafeUtil.putByte( p + 5, ( sbyte )( value >> 16 ) );
			  UnsafeUtil.putByte( p + 6, ( sbyte )( value >> 8 ) );
			  UnsafeUtil.putByte( p + 7, ( sbyte ) value );
		 }

		 public override int Int
		 {
			 get
			 {
				  long p = NextBoundedPointer( _sizeOfInt );
				  int i = GetIntAt( p );
				  _offset += _sizeOfInt;
				  return i;
			 }
		 }

		 public override int getInt( int offset )
		 {
			  long p = GetBoundedPointer( offset, _sizeOfInt );
			  return GetIntAt( p );
		 }

		 private int GetIntAt( long p )
		 {
			  if ( UnsafeUtil.allowUnalignedMemoryAccess )
			  {
					int x = UnsafeUtil.getInt( p );
					return UnsafeUtil.storeByteOrderIsNative ? x : Integer.reverseBytes( x );
			  }
			  return GetIntBigEndian( p );
		 }

		 private int GetIntBigEndian( long p )
		 {
			  int a = UnsafeUtil.getByte( p ) & 0xFF;
			  int b = UnsafeUtil.getByte( p + 1 ) & 0xFF;
			  int c = UnsafeUtil.getByte( p + 2 ) & 0xFF;
			  int d = UnsafeUtil.getByte( p + 3 ) & 0xFF;
			  return ( a << 24 ) | ( b << 16 ) | ( c << 8 ) | d;
		 }

		 public override void PutInt( int value )
		 {
			  long p = NextBoundedPointer( _sizeOfInt );
			  PutIntAt( p, value );
			  _offset += _sizeOfInt;
		 }

		 public override void PutInt( int offset, int value )
		 {
			  long p = GetBoundedPointer( offset, _sizeOfInt );
			  PutIntAt( p, value );
		 }

		 private void PutIntAt( long p, int value )
		 {
			  if ( UnsafeUtil.allowUnalignedMemoryAccess )
			  {
					UnsafeUtil.putInt( p, UnsafeUtil.storeByteOrderIsNative ? value : Integer.reverseBytes( value ) );
			  }
			  else
			  {
					PutIntBigEndian( value, p );
			  }
		 }

		 private void PutIntBigEndian( int value, long p )
		 {
			  UnsafeUtil.putByte( p, ( sbyte )( value >> 24 ) );
			  UnsafeUtil.putByte( p + 1, ( sbyte )( value >> 16 ) );
			  UnsafeUtil.putByte( p + 2, ( sbyte )( value >> 8 ) );
			  UnsafeUtil.putByte( p + 3, ( sbyte ) value );
		 }

		 public override void GetBytes( sbyte[] data )
		 {
			  GetBytes( data, 0, data.Length );
		 }

		 public override void GetBytes( sbyte[] data, int arrayOffset, int length )
		 {
			  long p = GetBoundedPointer( _offset, length );
			  if ( !_outOfBounds )
			  {
					for ( int i = 0; i < length; i++ )
					{
						 data[arrayOffset + i] = UnsafeUtil.getByte( p + i );
					}
			  }
			  _offset += length;
		 }

		 public override void PutBytes( sbyte[] data )
		 {
			  PutBytes( data, 0, data.Length );
		 }

		 public override void PutBytes( sbyte[] data, int arrayOffset, int length )
		 {
			  long p = GetBoundedPointer( _offset, length );
			  if ( !_outOfBounds )
			  {
					for ( int i = 0; i < length; i++ )
					{
						 sbyte b = data[arrayOffset + i];
						 UnsafeUtil.putByte( p + i, b );
					}
			  }
			  _offset += length;
		 }

		 public override void PutBytes( int bytes, sbyte value )
		 {
			  long p = GetBoundedPointer( _offset, bytes );
			  if ( !_outOfBounds )
			  {
					UnsafeUtil.setMemory( p, bytes, value );
			  }
			  _offset += bytes;
		 }

		 public override sealed short Short
		 {
			 get
			 {
				  long p = NextBoundedPointer( _sizeOfShort );
				  short s = GetShortAt( p );
				  _offset += _sizeOfShort;
				  return s;
			 }
		 }

		 public override short getShort( int offset )
		 {
			  long p = GetBoundedPointer( offset, _sizeOfShort );
			  return GetShortAt( p );
		 }

		 private short GetShortAt( long p )
		 {
			  if ( UnsafeUtil.allowUnalignedMemoryAccess )
			  {
					short x = UnsafeUtil.getShort( p );
					return UnsafeUtil.storeByteOrderIsNative ? x : Short.reverseBytes( x );
			  }
			  return GetShortBigEndian( p );
		 }

		 private short GetShortBigEndian( long p )
		 {
			  short a = ( short )( UnsafeUtil.getByte( p ) & 0xFF );
			  short b = ( short )( UnsafeUtil.getByte( p + 1 ) & 0xFF );
			  return ( short )( ( a << 8 ) | b );
		 }

		 public override void PutShort( short value )
		 {
			  long p = NextBoundedPointer( _sizeOfShort );
			  PutShortAt( p, value );
			  _offset += _sizeOfShort;
		 }

		 public override void PutShort( int offset, short value )
		 {
			  long p = GetBoundedPointer( offset, _sizeOfShort );
			  PutShortAt( p, value );
		 }

		 private void PutShortAt( long p, short value )
		 {
			  if ( UnsafeUtil.allowUnalignedMemoryAccess )
			  {
					UnsafeUtil.putShort( p, UnsafeUtil.storeByteOrderIsNative ? value : Short.reverseBytes( value ) );
			  }
			  else
			  {
					PutShortBigEndian( value, p );
			  }
		 }

		 private void PutShortBigEndian( short value, long p )
		 {
			  UnsafeUtil.putByte( p, ( sbyte )( value >> 8 ) );
			  UnsafeUtil.putByte( p + 1, ( sbyte ) value );
		 }

		 public override int CopyTo( int sourceOffset, PageCursor targetCursor, int targetOffset, int lengthInBytes )
		 {
			  int sourcePageSize = CurrentPageSize;
			  int targetPageSize = targetCursor.CurrentPageSize;
			  if ( targetCursor.GetType() != typeof(MuninnWritePageCursor) )
			  {
					throw new System.ArgumentException( "Target cursor must be writable" );
			  }
			  if ( sourceOffset >= 0 & targetOffset >= 0 & sourceOffset < sourcePageSize & targetOffset < targetPageSize & lengthInBytes >= 0 )
			  {
					MuninnPageCursor cursor = ( MuninnPageCursor ) targetCursor;
					int remainingSource = sourcePageSize - sourceOffset;
					int remainingTarget = targetPageSize - targetOffset;
					int bytes = Math.Min( lengthInBytes, Math.Min( remainingSource, remainingTarget ) );
					UnsafeUtil.copyMemory( _pointer + sourceOffset, cursor._pointer + targetOffset, bytes );
					return bytes;
			  }
			  _outOfBounds = true;
			  return 0;
		 }

		 public override int CopyTo( int sourceOffset, ByteBuffer buf )
		 {
			  if ( buf.GetType() == UnsafeUtil.directByteBufferClass && buf.Direct && !buf.ReadOnly )
			  {
					// We expect that the mutable direct byte buffer is implemented with a class that is distinct from the
					// non-mutable (read-only) and non-direct (on-heap) byte buffers. By comparing class object instances,
					// we also implicitly assume that the classes are loaded by the same class loader, which should be
					// trivially true in almost all practical cases.
					// If our expectations are not met, then the additional isDirect and !isReadOnly checks will send all
					// calls to the byte-wise-copy fallback.
					return CopyToDirectByteBuffer( sourceOffset, buf );
			  }
			  return CopyToByteBufferByteWise( sourceOffset, buf );
		 }

		 private int CopyToDirectByteBuffer( int sourceOffset, ByteBuffer buf )
		 {
			  int pos = buf.position();
			  int bytesToCopy = Math.Min( buf.limit() - pos, _pageSize - sourceOffset );
			  long source = _pointer + sourceOffset;
			  if ( sourceOffset < CurrentPageSize & sourceOffset >= 0 )
			  {
					long target = UnsafeUtil.getDirectByteBufferAddress( buf );
					UnsafeUtil.copyMemory( source, target + pos, bytesToCopy );
					buf.position( pos + bytesToCopy );
			  }
			  else
			  {
					_outOfBounds = true;
			  }
			  return bytesToCopy;
		 }

		 private int CopyToByteBufferByteWise( int sourceOffset, ByteBuffer buf )
		 {
			  int bytesToCopy = Math.Min( buf.limit() - buf.position(), _pageSize - sourceOffset );
			  for ( int i = 0; i < bytesToCopy; i++ )
			  {
					sbyte b = GetByte( sourceOffset + i );
					buf.put( b );
			  }
			  return bytesToCopy;
		 }

		 public override void ShiftBytes( int sourceStart, int length, int shift )
		 {
			  int sourceEnd = sourceStart + length;
			  int targetStart = sourceStart + shift;
			  int targetEnd = sourceStart + length + shift;
			  if ( sourceStart < 0 | sourceEnd > _filePageSize | targetStart < 0 | targetEnd > _filePageSize | length < 0 )
			  {
					_outOfBounds = true;
					return;
			  }

			  if ( shift < 0 )
			  {
					UnsafeShiftLeft( sourceStart, sourceEnd, length, shift );
			  }
			  else
			  {
					UnsafeShiftRight( sourceEnd, sourceStart, length, shift );
			  }
		 }

		 private void UnsafeShiftLeft( int fromPos, int toPos, int length, int shift )
		 {
			  int longSteps = length >> 3;
			  if ( UnsafeUtil.allowUnalignedMemoryAccess && longSteps > 0 )
			  {
					for ( int i = 0; i < longSteps; i++ )
					{
						 long x = UnsafeUtil.getLong( _pointer + fromPos );
						 UnsafeUtil.putLong( _pointer + fromPos + shift, x );
						 fromPos += Long.BYTES;
					}
			  }

			  while ( fromPos < toPos )
			  {
					sbyte b = UnsafeUtil.getByte( _pointer + fromPos );
					UnsafeUtil.putByte( _pointer + fromPos + shift, b );
					fromPos++;
			  }
		 }

		 private void UnsafeShiftRight( int fromPos, int toPos, int length, int shift )
		 {
			  int longSteps = length >> 3;
			  if ( UnsafeUtil.allowUnalignedMemoryAccess && longSteps > 0 )
			  {
					for ( int i = 0; i < longSteps; i++ )
					{
						 fromPos -= Long.BYTES;
						 long x = UnsafeUtil.getLong( _pointer + fromPos );
						 UnsafeUtil.putLong( _pointer + fromPos + shift, x );
					}
			  }

			  while ( fromPos > toPos )
			  {
					fromPos--;
					sbyte b = UnsafeUtil.getByte( _pointer + fromPos );
					UnsafeUtil.putByte( _pointer + fromPos + shift, b );
			  }
		 }

		 public override int Offset
		 {
			 set
			 {
				  this._offset = value;
				  if ( value < 0 | value > _filePageSize )
				  {
						this._offset = 0;
						_outOfBounds = true;
				  }
			 }
			 get
			 {
				  return _offset;
			 }
		 }


		 public override void Mark()
		 {
			  this._mark = _offset;
		 }

		 public override void SetOffsetToMark()
		 {
			  this._offset = _mark;
		 }

		 public override bool CheckAndClearBoundsFlag()
		 {
			  MuninnPageCursor cursor = this;
			  bool result = false;
			  do
			  {
					result |= cursor._outOfBounds;
					cursor._outOfBounds = false;
					cursor = cursor.LinkedCursor;
			  } while ( cursor != null );
			  return result;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void checkAndClearCursorException() throws org.Neo4Net.io.pagecache.CursorException
		 public override void CheckAndClearCursorException()
		 {
			  MuninnPageCursor cursor = this;
			  do
			  {
					object error = cursor._cursorException;
					if ( error != null )
					{
						 ClearCursorError( cursor );
						 if ( _usePreciseCursorErrorStackTraces )
						 {
							  throw ( CursorExceptionWithPreciseStackTrace ) error;
						 }
						 else
						 {
							  throw new CursorException( ( string ) error );
						 }
					}
					cursor = cursor.LinkedCursor;
			  } while ( cursor != null );
		 }

		 public override void ClearCursorException()
		 {
			  ClearCursorError( this );
		 }

		 private void ClearCursorError( MuninnPageCursor cursor )
		 {
			  while ( cursor != null )
			  {
					cursor._cursorException = null;
					cursor = cursor.LinkedCursor;
			  }
		 }

		 public override void RaiseOutOfBounds()
		 {
			  _outOfBounds = true;
		 }

		 public override string CursorException
		 {
			 set
			 {
				  Objects.requireNonNull( value );
				  if ( _usePreciseCursorErrorStackTraces )
				  {
						this._cursorException = new CursorExceptionWithPreciseStackTrace( value );
				  }
				  else
				  {
						this._cursorException = value;
				  }
			 }
		 }

		 public override void ZapPage()
		 {
			  if ( _pageSize == 0 )
			  {
					// if this page has been closed then pageSize == 0 and we must adhere to making writes
					// trigger outOfBounds when closed
					_outOfBounds = true;
			  }
			  else
			  {
					UnsafeUtil.setMemory( _pointer, _pageSize, ( sbyte ) 0 );
			  }
		 }

		 public override bool WriteLocked
		 {
			 get
			 {
				  return IsFlagRaised( PfFlags, PF_SHARED_WRITE_LOCK );
			 }
		 }
	}

}