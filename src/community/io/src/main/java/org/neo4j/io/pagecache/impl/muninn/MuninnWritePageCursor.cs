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

	using PageCursorTracer = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracer;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;

	internal sealed class MuninnWritePageCursor : MuninnPageCursor
	{
		 internal MuninnWritePageCursor( long victimPage, PageCursorTracer pageCursorTracer, VersionContextSupplier versionContextSupplier ) : base( victimPage, pageCursorTracer, versionContextSupplier )
		 {
		 }

		 protected internal override void UnpinCurrentPage()
		 {
			  if ( PinnedPageRef != 0 )
			  {
					PinEvent.done();
					// Mark the page as dirty *after* our write access, to make sure it's dirty even if it was concurrently
					// flushed. Unlocking the write-locked page will mark it as dirty for us.
					if ( EagerFlush )
					{
						 EagerlyFlushAndUnlockPage();
					}
					else
					{
						 PagedFile.unlockWrite( PinnedPageRef );
					}
			  }
			  ClearPageCursorState();
		 }

		 private void EagerlyFlushAndUnlockPage()
		 {
			  long flushStamp = PagedFile.unlockWriteAndTryTakeFlushLock( PinnedPageRef );
			  if ( flushStamp != 0 )
			  {
					bool success = false;
					try
					{
						 success = PagedFile.flushLockedPage( PinnedPageRef, CurrentPageIdConflict );
					}
					finally
					{
						 PagedFile.unlockFlush( PinnedPageRef, flushStamp, success );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next() throws java.io.IOException
		 public override bool Next()
		 {
			  UnpinCurrentPage();
			  long lastPageId = AssertPagedFileStillMappedAndGetIdOfLastPage();
			  if ( NextPageId < 0 )
			  {
					return false;
			  }
			  if ( NextPageId > lastPageId )
			  {
					if ( NoGrow )
					{
						 return false;
					}
					else
					{
						 PagedFile.increaseLastPageIdTo( NextPageId );
					}
			  }
			  CurrentPageIdConflict = NextPageId;
			  NextPageId++;
			  Pin( CurrentPageIdConflict, true );
			  return true;
		 }

		 protected internal override bool TryLockPage( long pageRef )
		 {
			  return PagedFile.tryWriteLock( pageRef );
		 }

		 protected internal override void UnlockPage( long pageRef )
		 {
			  PagedFile.unlockWrite( pageRef );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void pinCursorToPage(long pageRef, long filePageId, Neo4Net.io.pagecache.PageSwapper swapper) throws Neo4Net.io.pagecache.impl.FileIsNotMappedException
		 protected internal override void PinCursorToPage( long pageRef, long filePageId, PageSwapper swapper )
		 {
			  Reset( pageRef );
			  // Check if we've been racing with unmapping. We want to do this before
			  // we make any changes to the contents of the page, because once all
			  // files have been unmapped, the page cache can be closed. And when
			  // that happens, dirty contents in memory will no longer have a chance
			  // to get flushed. It is okay for this method to throw, because we are
			  // after the reset() call, which means that if we throw, the cursor will
			  // be closed and the page lock will be released.
			  AssertPagedFileStillMappedAndGetIdOfLastPage();
			  PagedFile.incrementUsage( pageRef );
			  PagedFile.setLastModifiedTxId( pageRef, VersionContextSupplier.VersionContext.committingTransactionId() );
		 }

		 protected internal override void ConvertPageFaultLock( long pageRef )
		 {
			  PagedFile.unlockExclusiveAndTakeWriteLock( pageRef );
		 }

		 public override bool ShouldRetry()
		 {
			  // We take exclusive locks, so there's never a need to retry.
			  return false;
		 }
	}

}