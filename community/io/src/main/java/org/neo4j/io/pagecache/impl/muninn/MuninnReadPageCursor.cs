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

	using PageCursorTracer = Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracer;
	using VersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.VersionContextSupplier;

	internal sealed class MuninnReadPageCursor : MuninnPageCursor
	{
		 private long _lockStamp;

		 internal MuninnReadPageCursor( long victimPage, PageCursorTracer pageCursorTracer, VersionContextSupplier versionContextSupplier ) : base( victimPage, pageCursorTracer, versionContextSupplier )
		 {
		 }

		 protected internal override void UnpinCurrentPage()
		 {
			  if ( PinnedPageRef != 0 )
			  {
					PinEvent.done();
			  }
			  _lockStamp = 0; // make sure not to accidentally keep a lock state around
			  ClearPageCursorState();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next() throws java.io.IOException
		 public override bool Next()
		 {
			  UnpinCurrentPage();
			  long lastPageId = AssertPagedFileStillMappedAndGetIdOfLastPage();
			  if ( NextPageId > lastPageId | NextPageId < 0 )
			  {
					return false;
			  }
			  CurrentPageIdConflict = NextPageId;
			  NextPageId++;
			  Pin( CurrentPageIdConflict, false );
			  VerifyContext();
			  return true;
		 }

		 protected internal override bool TryLockPage( long pageRef )
		 {
			  _lockStamp = PagedFile.tryOptimisticReadLock( pageRef );
			  return true;
		 }

		 protected internal override void UnlockPage( long pageRef )
		 {
		 }

		 protected internal override void PinCursorToPage( long pageRef, long filePageId, PageSwapper swapper )
		 {
			  Reset( pageRef );
			  PagedFile.incrementUsage( pageRef );
		 }

		 protected internal override void ConvertPageFaultLock( long pageRef )
		 {
			  _lockStamp = PagedFile.unlockExclusive( pageRef );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean shouldRetry() throws java.io.IOException
		 public override bool ShouldRetry()
		 {
			  MuninnReadPageCursor cursor = this;
			  do
			  {
					long pageRef = cursor.PinnedPageRef;
					if ( pageRef != 0 && !PagedFile.validateReadLock( pageRef, cursor._lockStamp ) )
					{
						 StartRetryLinkedChain();
						 return true;
					}
					cursor = ( MuninnReadPageCursor ) cursor.LinkedCursor;
			  } while ( cursor != null );
			  return false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void startRetryLinkedChain() throws java.io.IOException
		 private void StartRetryLinkedChain()
		 {
			  MuninnReadPageCursor cursor = this;
			  do
			  {
					if ( cursor.PinnedPageRef != 0 )
					{
						 cursor.StartRetry();
					}
					cursor = ( MuninnReadPageCursor ) cursor.LinkedCursor;
			  } while ( cursor != null );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void startRetry() throws java.io.IOException
		 private void StartRetry()
		 {
			  Offset = 0;
			  CheckAndClearBoundsFlag();
			  ClearCursorException();
			  _lockStamp = PagedFile.tryOptimisticReadLock( PinnedPageRef );
			  // The page might have been evicted while we held the optimistic
			  // read lock, so we need to check with page.pin that this is still
			  // the page we're actually interested in:
			  if ( !PagedFile.isBoundTo( PinnedPageRef, PagedFile.swapperId, CurrentPageIdConflict ) )
			  {
					// This is no longer the page we're interested in, so we have
					// to redo the pinning.
					// This might in turn lead to a new optimistic lock on a
					// different page if someone else has taken the page fault for
					// us. If nobody has done that, we'll take the page fault
					// ourselves, and in that case we'll end up with first an exclusive
					// lock during the faulting, and then an optimistic read lock once the
					// fault itself is over.
					// First, forget about this page in case pin() throws and the cursor
					// is closed; we don't want unpinCurrentPage() to try unlocking
					// this page.
					ClearPageReference();
					// Then try pin again.
					Pin( CurrentPageIdConflict, false );
			  }
		 }

		 public override void PutByte( sbyte value )
		 {
			  throw new System.InvalidOperationException( "Cannot write to read-locked page" );
		 }

		 public override void PutLong( long value )
		 {
			  throw new System.InvalidOperationException( "Cannot write to read-locked page" );
		 }

		 public override void PutInt( int value )
		 {
			  throw new System.InvalidOperationException( "Cannot write to read-locked page" );
		 }

		 public override void PutBytes( sbyte[] data, int arrayOffset, int length )
		 {
			  throw new System.InvalidOperationException( "Cannot write to read-locked page" );
		 }

		 public override void PutShort( short value )
		 {
			  throw new System.InvalidOperationException( "Cannot write to read-locked page" );
		 }

		 public override void ShiftBytes( int sourceStart, int length, int shift )
		 {
			  throw new System.InvalidOperationException( "Cannot write to read-locked page" );
		 }

		 public override void ZapPage()
		 {
			  throw new System.InvalidOperationException( "Cannot write to read-locked page" );
		 }
	}

}