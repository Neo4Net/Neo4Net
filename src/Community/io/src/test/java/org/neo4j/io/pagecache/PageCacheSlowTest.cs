using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Io.pagecache
{
	using RepeatedTest = org.junit.jupiter.api.RepeatedTest;
	using Test = org.junit.jupiter.api.Test;


	using RandomAdversary = Neo4Net.Adversaries.RandomAdversary;
	using AdversarialFileSystemAbstraction = Neo4Net.Adversaries.fs.AdversarialFileSystemAbstraction;
	using EphemeralFileSystemAbstraction = Neo4Net.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using FileIsNotMappedException = Neo4Net.Io.pagecache.impl.FileIsNotMappedException;
	using PageCacheTracer = Neo4Net.Io.pagecache.tracing.PageCacheTracer;
	using PageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using LinearHistoryTracerFactory = Neo4Net.Io.pagecache.tracing.linear.LinearHistoryTracerFactory;
	using LinearTracers = Neo4Net.Io.pagecache.tracing.linear.LinearTracers;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTimeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assumptions.assumeTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.PagedFile_Fields.PF_SHARED_WRITE_LOCK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.matchers.ByteArrayMatcher.byteArray;

	public abstract class PageCacheSlowTest<T> : PageCacheTestSupport<T> where T : PageCache
	{
		 private class UpdateResult
		 {
			  internal readonly int ThreadId;
			  internal readonly long RealThreadId;
			  internal readonly int[] PageCounts;

			  internal UpdateResult( int threadId, int[] pageCounts )
			  {
					this.ThreadId = threadId;
					this.RealThreadId = Thread.CurrentThread.Id;
					this.PageCounts = pageCounts;
			  }
		 }

		 private abstract class UpdateWorker : Callable<UpdateResult>
		 {
			  internal readonly int ThreadId;
			  internal readonly int FilePages;
			  internal readonly AtomicBoolean ShouldStop;
			  internal readonly PagedFile PagedFile;
			  internal readonly int[] PageCounts;
			  internal readonly int Offset;

			  internal UpdateWorker( int threadId, int filePages, AtomicBoolean shouldStop, PagedFile pagedFile )
			  {
					this.ThreadId = threadId;
					this.FilePages = filePages;
					this.ShouldStop = shouldStop;
					this.PagedFile = pagedFile;
					PageCounts = new int[filePages];
					Offset = threadId * 4;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public UpdateResult call() throws Exception
			  public override UpdateResult Call()
			  {
					ThreadLocalRandom rng = ThreadLocalRandom.current();

					while ( !ShouldStop.get() )
					{
						 bool updateCounter = rng.nextBoolean();
						 int pfFlags = updateCounter ? PF_SHARED_WRITE_LOCK : PF_SHARED_READ_LOCK;
						 PerformReadOrUpdate( rng, updateCounter, pfFlags );
					}

					return new UpdateResult( ThreadId, PageCounts );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract void performReadOrUpdate(java.util.concurrent.ThreadLocalRandom rng, boolean updateCounter, int pf_flags) throws java.io.IOException;
			  protected internal abstract void PerformReadOrUpdate( ThreadLocalRandom rng, bool updateCounter, int pfFlags );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RepeatedTest(50) void mustNotLoseUpdates()
		 internal virtual void MustNotLoseUpdates()
		 {
			  assertTimeout(ofMillis(SEMI_LONG_TIMEOUT_MILLIS), () =>
			  {
				// Another test that tries to squeeze out data race bugs. The idea is
				// the following:
				// We have a number of threads that are going to perform one of two
				// operations on randomly chosen pages. The first operation is this:
				// They are going to pin a random page, and then scan through it to
				// find a record that is their own. A record has a thread-id and a
				// counter, both 32-bit integers. If the record is not found, it will
				// be added after all the other existing records on that page, if any.
				// The last 32-bit word on a page is a sum of all the counters, and it
				// will be updated. Then it will verify that the sum matches the
				// counters.
				// The second operation is read-only, where only the verification is
				// performed.
				// The kicker is this: the threads will also keep track of which of
				// their counters on what pages are at what value, by maintaining
				// mirror counters in memory. The threads will continuously check if
				// these stay in sync with the data on the page cache. If they go out
				// of sync, then we have a data race bug where we either pin the wrong
				// pages or somehow lose updates to the pages.
				// This is somewhat similar to what the PageCacheStressTest does.

				AtomicBoolean shouldStop = new AtomicBoolean();
				const int cachePages = 20;
				int filePages = cachePages * 2;
				const int threadCount = 4;
				int pageSize = threadCount * 4;

				// For debugging via the linear tracers:
//        LinearTracers linearTracers = LinearHistoryTracerFactory.pageCacheTracer();
//        getPageCache( fs, cachePages, pageSize, linearTracers.getPageCacheTracer(),
//                linearTracers.getCursorTracerSupplier() );
				getPageCache( fs, cachePages, PageCacheTracer.NULL, PageCursorTracerSupplier.NULL );
				PagedFile pagedFile = pageCache.map( file( "a" ), pageSize );

				EnsureAllPagesExists( filePages, pagedFile );

				IList<Future<UpdateResult>> futures = new List<Future<UpdateResult>>();
				for ( int i = 0; i < threadCount; i++ )
				{
					 UpdateWorker worker = new UpdateWorkerAnonymousInnerClass( this, i, filePages, shouldStop, pagedFile );
					 futures.add( executor.submit( worker ) );
				}

				Thread.Sleep( 10 );
				shouldStop.set( true );

				try
				{
					 VerifyUpdateResults( filePages, pagedFile, futures );
				}
				catch ( Exception e )
				{
					 // For debugging via linear tracers:
//            synchronized ( System.err )
//            {
//                System.err.flush();
//                linearTracers.printHistory( System.err );
//                System.err.flush();
//            }
//            try ( PrintStream out = new PrintStream( "trace.log" ) )
//            {
//                linearTracers.printHistory( out );
//                out.flush();
//            }
					 throw e;
				}
				pagedFile.Close();
			  });
		 }

		 private class UpdateWorkerAnonymousInnerClass : UpdateWorker
		 {
			 private readonly PageCacheSlowTest<T> _outerInstance;

			 private new AtomicBoolean _shouldStop;
			 private new int _filePages;
			 private new Neo4Net.Io.pagecache.PagedFile _pagedFile;

			 public UpdateWorkerAnonymousInnerClass( PageCacheSlowTest<T> outerInstance, int i, int filePages, AtomicBoolean shouldStop, Neo4Net.Io.pagecache.PagedFile pagedFile ) : base( i, filePages, shouldStop, pagedFile )
			 {
				 this.outerInstance = outerInstance;
				 this._shouldStop = shouldStop;
				 this._filePages = filePages;
				 this._pagedFile = pagedFile;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void performReadOrUpdate(java.util.concurrent.ThreadLocalRandom rng, boolean updateCounter, int pf_flags) throws java.io.IOException
			 protected internal override void performReadOrUpdate( ThreadLocalRandom rng, bool updateCounter, int pfFlags )
			 {
				  int pageId = rng.Next( 0, _filePages );
				  using ( PageCursor cursor = _pagedFile.io( pageId, pfFlags ) )
				  {
						int counter;
						try
						{
							 assertTrue( cursor.Next() );
							 do
							 {
								  cursor.Offset = offset;
								  counter = cursor.Int;
							 } while ( cursor.ShouldRetry() );
							 string lockName = updateCounter ? "PF_SHARED_WRITE_LOCK" : "PF_SHARED_READ_LOCK";
							 string reason = string.Format( "inconsistent page read from filePageId:{0}, with {1}, threadId:{2}", pageId, lockName, Thread.CurrentThread.Id );
							 assertThat( reason, counter, @is( pageCounts[pageId] ) );
						}
						catch ( Exception throwable )
						{
							 _shouldStop.set( true );
							 throw throwable;
						}
						if ( updateCounter )
						{
							 counter++;
							 pageCounts[pageId]++;
							 cursor.Offset = offset;
							 cursor.PutInt( counter );
						}
						if ( cursor.CheckAndClearBoundsFlag() )
						{
							 _shouldStop.set( true );
							 throw new System.IndexOutOfRangeException( "offset = " + offset + ", filPageId:" + pageId + ", threadId: " + threadId + ", updateCounter = " + updateCounter );
						}
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void ensureAllPagesExists(int filePages, PagedFile pagedFile) throws java.io.IOException
		 private void EnsureAllPagesExists( int filePages, PagedFile pagedFile )
		 {
			  using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					for ( int i = 0; i < filePages; i++ )
					{
						 assertTrue( cursor.Next(), "failed to initialise file page " + i );
					}
			  }
			  pageCache.flushAndForce();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyUpdateResults(int filePages, PagedFile pagedFile, java.util.List<java.util.concurrent.Future<UpdateResult>> futures) throws InterruptedException, java.util.concurrent.ExecutionException, java.io.IOException
		 private void VerifyUpdateResults( int filePages, PagedFile pagedFile, IList<Future<UpdateResult>> futures )
		 {
			  UpdateResult[] results = new UpdateResult[futures.Count];
			  for ( int i = 0; i < results.Length; i++ )
			  {
					results[i] = futures[i].get();
			  }
			  foreach ( UpdateResult result in results )
			  {
					using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
					{
						 for ( int i = 0; i < filePages; i++ )
						 {
							  assertTrue( cursor.Next() );

							  int threadId = result.ThreadId;
							  int expectedCount = result.PageCounts[i];
							  int actualCount;
							  do
							  {
									cursor.Offset = threadId * 4;
									actualCount = cursor.Int;
							  } while ( cursor.ShouldRetry() );

							  assertThat( "wrong count for threadId:" + threadId + ", aka. real threadId:" + result.RealThreadId + ", filePageId:" + i, actualCount, @is( expectedCount ) );
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RepeatedTest(100) void mustNotLoseUpdatesWhenOpeningMultiplePageCursorsPerThread()
		 internal virtual void MustNotLoseUpdatesWhenOpeningMultiplePageCursorsPerThread()
		 {
			  assertTimeout(ofMillis(SEMI_LONG_TIMEOUT_MILLIS), () =>
			  {
				// Similar to the test above, except the threads will have multiple page cursors opened at a time.

				AtomicBoolean shouldStop = new AtomicBoolean();
				const int cachePages = 40;
				int filePages = cachePages * 2;
				const int threadCount = 8;
				int pageSize = threadCount * 4;

				// It's very important that even if all threads grab their maximum number of pages at the same time, there will
				// still be free pages left in the cache. If we don't keep this invariant, then there's a chance that our test
				// will run into live-locks, where a page fault will try to find a page to cooperatively evict, but all pages
				// in cache are already taken.
				int maxCursorsPerThread = cachePages / ( 1 + threadCount );
				assertThat( maxCursorsPerThread * threadCount, lessThan( cachePages ) );

				getPageCache( fs, cachePages, PageCacheTracer.NULL, PageCursorTracerSupplier.NULL );
				PagedFile pagedFile = pageCache.map( file( "a" ), pageSize );

				EnsureAllPagesExists( filePages, pagedFile );

				IList<Future<UpdateResult>> futures = new List<Future<UpdateResult>>();
				for ( int i = 0; i < threadCount; i++ )
				{
					 UpdateWorker worker = new UpdateWorkerAnonymousInnerClass2( this, i, filePages, shouldStop, pagedFile, maxCursorsPerThread );
					 futures.add( executor.submit( worker ) );
				}

				Thread.Sleep( 40 );
				shouldStop.set( true );

				VerifyUpdateResults( filePages, pagedFile, futures );
				pagedFile.Close();
			  });
		 }

		 private class UpdateWorkerAnonymousInnerClass2 : UpdateWorker
		 {
			 private readonly PageCacheSlowTest<T> _outerInstance;

			 private new AtomicBoolean _shouldStop;
			 private new int _filePages;
			 private int _maxCursorsPerThread;
			 private new Neo4Net.Io.pagecache.PagedFile _pagedFile;

			 public UpdateWorkerAnonymousInnerClass2( PageCacheSlowTest<T> outerInstance, int i, int filePages, AtomicBoolean shouldStop, Neo4Net.Io.pagecache.PagedFile pagedFile, int maxCursorsPerThread ) : base( i, filePages, shouldStop, pagedFile )
			 {
				 this.outerInstance = outerInstance;
				 this._shouldStop = shouldStop;
				 this._filePages = filePages;
				 this._maxCursorsPerThread = maxCursorsPerThread;
				 this._pagedFile = pagedFile;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void performReadOrUpdate(java.util.concurrent.ThreadLocalRandom rng, boolean updateCounter, int pf_flags) throws java.io.IOException
			 protected internal override void performReadOrUpdate( ThreadLocalRandom rng, bool updateCounter, int pfFlags )
			 {
				  try
				  {
						int pageCount = rng.Next( 1, _maxCursorsPerThread );
						int[] pageIds = new int[pageCount];
						for ( int j = 0; j < pageCount; j++ )
						{
							 pageIds[j] = rng.Next( 0, _filePages );
						}
						PageCursor[] cursors = new PageCursor[pageCount];
						for ( int j = 0; j < pageCount; j++ )
						{
							 cursors[j] = _pagedFile.io( pageIds[j], pfFlags );
							 assertTrue( cursors[j].Next() );
						}
						for ( int j = 0; j < pageCount; j++ )
						{
							 int pageId = pageIds[j];
							 PageCursor cursor = cursors[j];
							 int counter;
							 do
							 {
								  cursor.Offset = offset;
								  counter = cursor.Int;
							 } while ( cursor.ShouldRetry() );
							 string lockName = updateCounter ? "PF_SHARED_WRITE_LOCK" : "PF_SHARED_READ_LOCK";
							 string reason = string.Format( "inconsistent page read from filePageId = {0}, with {1}, workerId = {2} [t:{3}]", pageId, lockName, threadId, Thread.CurrentThread.Id );
							 assertThat( reason, counter, @is( pageCounts[pageId] ) );
							 if ( updateCounter )
							 {
								  counter++;
								  pageCounts[pageId]++;
								  cursor.Offset = offset;
								  cursor.PutInt( counter );
							 }
						}
						foreach ( PageCursor cursor in cursors )
						{
							 cursor.Close();
						}
				  }
				  catch ( Exception throwable )
				  {
						_shouldStop.set( true );
						throw throwable;
				  }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RepeatedTest(50) void writeLockingCursorMustThrowWhenLockingPageRacesWithUnmapping()
		 internal virtual void WriteLockingCursorMustThrowWhenLockingPageRacesWithUnmapping()
		 {
			  assertTimeout(ofMillis(SEMI_LONG_TIMEOUT_MILLIS), () =>
			  {
				// Even if we block in pin, waiting to grab a lock on a page that is
				// already locked, and the PagedFile is concurrently closed, then we
				// want to have an exception thrown, such that we race and get a
				// page that is writable after the PagedFile has been closed.
				// This is important because closing a PagedFile implies flushing, thus
				// ensuring that all changes make it to storage.
				// Conversely, we don't have to go to the same lengths for read locked
				// pages, because those are never changed. Not by us, anyway.

				File file = file( "a" );
				generateFileWithRecords( file, recordsPerFilePage * 2, recordSize );

				getPageCache( fs, maxPages, PageCacheTracer.NULL, PageCursorTracerSupplier.NULL );

				PagedFile pf = pageCache.map( file, filePageSize );
				System.Threading.CountdownEvent hasLockLatch = new System.Threading.CountdownEvent( 1 );
				System.Threading.CountdownEvent unlockLatch = new System.Threading.CountdownEvent( 1 );
				System.Threading.CountdownEvent secondThreadGotLockLatch = new System.Threading.CountdownEvent( 1 );
				AtomicBoolean doneWriteSignal = new AtomicBoolean();
				AtomicBoolean doneCloseSignal = new AtomicBoolean();

				executor.submit(() =>
				{
					 using ( PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
					 {
						  cursor.Next();
						  hasLockLatch.countDown();
						  unlockLatch.await();
					 }
					 return null;
				});

				hasLockLatch.await(); // A write lock is now held on page 0.

				Future<object> takeLockFuture = executor.submit(() =>
				{
					 using ( PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
					 {
						  cursor.Next();
						  doneWriteSignal.set( true );
						  secondThreadGotLockLatch.await();
					 }
					 return null;
				});

				Future<object> closeFuture = executor.submit(() =>
				{
					 pf.Close();
					 doneCloseSignal.set( true );
					 return null;
				});

				try
				{
					 Thread.yield();
					 closeFuture.get( 50, TimeUnit.MILLISECONDS );
					 fail( "Expected a TimeoutException here" );
				}
				catch ( TimeoutException )
				{
					 // As expected, the close cannot not complete while an write
					 // lock is held
				}

				// Now, both the close action and a grab for an write page lock is
				// waiting for our first thread.
				// When we release that lock, we should see that either close completes
				// and our second thread, the one blocked on the write lock, gets an
				// exception, or we should see that the second thread gets the lock,
				// and then close has to wait for that thread as well.

				unlockLatch.countDown(); // The race is on.

				bool anyDone;
				do
				{
					 Thread.yield();
					 anyDone = doneWriteSignal.get() | doneCloseSignal.get();
				} while ( !anyDone );

				if ( doneCloseSignal.get() )
				{
					 closeFuture.get( 1000, TimeUnit.MILLISECONDS );
					 // The closeFuture got it first, so the takeLockFuture should throw.
					 try
					 {
						  secondThreadGotLockLatch.countDown(); // only to prevent incorrect programs from deadlocking
						  takeLockFuture.get();
						  fail( "Expected takeLockFuture.get() to throw an ExecutionException" );
					 }
					 catch ( ExecutionException e )
					 {
						  Exception cause = e.InnerException;
						  assertThat( cause, instanceOf( typeof( FileIsNotMappedException ) ) );
						  assertThat( cause.Message, startsWith( "File has been unmapped" ) );
					 }
				}
				else
				{
					 assertTrue( doneWriteSignal.get() );
					 // The takeLockFuture got it first, so the closeFuture should
					 // complete when we release the latch.
					 secondThreadGotLockLatch.countDown();
					 closeFuture.get( 20000, TimeUnit.MILLISECONDS );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RepeatedTest(20) void pageCacheMustRemainInternallyConsistentWhenGettingRandomFailures()
		 internal virtual void PageCacheMustRemainInternallyConsistentWhenGettingRandomFailures()
		 {
			  assertTimeout(ofMillis(LONG_TIMEOUT_MILLIS), () =>
			  {
				// NOTE: This test is inherently non-deterministic. This means that every failure must be
				// thoroughly investigated, since they have a good chance of being a real issue.
				// This is effectively a targeted robustness test.

				RandomAdversary adversary = new RandomAdversary( 0.5, 0.2, 0.2 );
				adversary.ProbabilityFactor = 0.0;
				FileSystemAbstraction fs = new AdversarialFileSystemAbstraction( adversary, this.fs );
				ThreadLocalRandom rng = ThreadLocalRandom.current();

				// Because our test failures are non-deterministic, we use this tracer to capture a full history of the
				// events leading up to any given failure.
				LinearTracers linearTracers = LinearHistoryTracerFactory.pageCacheTracer();
				getPageCache( fs, maxPages, linearTracers.PageCacheTracer, linearTracers.CursorTracerSupplier );

				PagedFile pfA = pageCache.map( existingFile( "a" ), filePageSize );
				PagedFile pfB = pageCache.map( existingFile( "b" ), filePageSize / 2 + 1 );
				adversary.ProbabilityFactor = 1.0;

				for ( int i = 0; i < 1000; i++ )
				{
					 PagedFile pagedFile = rng.nextBoolean() ? pfA : pfB;
					 long maxPageId = pagedFile.LastPageId;
					 bool performingRead = rng.nextBoolean() && maxPageId != -1;
					 long startingPage = maxPageId < 0 ? 0 : rng.nextLong( maxPageId + 1 );
					 int pfFlags = performingRead ? PF_SHARED_READ_LOCK : PF_SHARED_WRITE_LOCK;
					 int pageSize = pagedFile.PageSize();

					 try
					 {
						 using ( PageCursor cursor = pagedFile.Io( startingPage, pfFlags ) )
						 {
							  if ( performingRead )
							  {
									PerformConsistentAdversarialRead( cursor, maxPageId, startingPage, pageSize );
							  }
							  else
							  {
									PerformConsistentAdversarialWrite( cursor, rng, pageSize );
							  }
						 }
					 }
					 catch ( AssertionError error )
					 {
						  // Capture any exception that might have hit the eviction thread.
						  adversary.ProbabilityFactor = 0.0;
						  try
						  {
							  using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
							  {
									for ( int j = 0; j < 100; j++ )
									{
										 cursor.Next( rng.nextLong( maxPageId + 1 ) );
									}
							  }
						  }
						  catch ( Exception throwable )
						  {
								error.addSuppressed( throwable );
						  }

						  throw error;
					 }
					 catch ( Exception )
					 {
						  // Don't worry about it... it's fine!
//                throwable.printStackTrace(); // only enable this when debugging test failures.
					 }
				}

				// Unmapping will cause pages to be flushed.
				// We don't want that to fail, since it will upset the test tear-down.
				adversary.ProbabilityFactor = 0.0;
				try
				{
					 // Flushing all pages, if successful, should clear any internal
					 // exception.
					 pageCache.flushAndForce();

					 // Do some post-chaos verification of what has been written.
					 VerifyAdversarialPagedContent( pfA );
					 VerifyAdversarialPagedContent( pfB );

					 pfA.Close();
					 pfB.Close();
				}
				catch ( Exception e )
				{
					 linearTracers.printHistory( System.err );
					 throw e;
				}
			  });
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void performConsistentAdversarialRead(PageCursor cursor, long maxPageId, long startingPage, int pageSize) throws java.io.IOException
		 private void PerformConsistentAdversarialRead( PageCursor cursor, long maxPageId, long startingPage, int pageSize )
		 {
			  long pagesToLookAt = Math.Min( maxPageId, startingPage + 3 ) - startingPage + 1;
			  for ( int j = 0; j < pagesToLookAt; j++ )
			  {
					assertTrue( cursor.Next() );
					ReadAndVerifyAdversarialPage( cursor, pageSize );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void readAndVerifyAdversarialPage(PageCursor cursor, int pageSize) throws java.io.IOException
		 private void ReadAndVerifyAdversarialPage( PageCursor cursor, int pageSize )
		 {
			  sbyte[] actualPage = new sbyte[pageSize];
			  sbyte[] expectedPage = new sbyte[pageSize];
			  do
			  {
					cursor.GetBytes( actualPage );
			  } while ( cursor.ShouldRetry() );
			  Arrays.fill( expectedPage, actualPage[0] );
			  string msg = string.Format( "filePageId = {0}, pageSize = {1}", cursor.CurrentPageId, pageSize );
			  assertThat( msg, actualPage, byteArray( expectedPage ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void performConsistentAdversarialWrite(PageCursor cursor, java.util.concurrent.ThreadLocalRandom rng, int pageSize) throws java.io.IOException
		 private void PerformConsistentAdversarialWrite( PageCursor cursor, ThreadLocalRandom rng, int pageSize )
		 {
			  for ( int j = 0; j < 3; j++ )
			  {
					assertTrue( cursor.Next() );
					// Avoid generating zeros, so we can tell them apart from the
					// absence of a write:
					sbyte b = ( sbyte ) rng.Next( 1, 127 );
					for ( int k = 0; k < pageSize; k++ )
					{
						 cursor.PutByte( b );
					}
					assertFalse( cursor.ShouldRetry() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyAdversarialPagedContent(PagedFile pagedFile) throws java.io.IOException
		 private void VerifyAdversarialPagedContent( PagedFile pagedFile )
		 {
			  using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
			  {
					while ( cursor.Next() )
					{
						 ReadAndVerifyAdversarialPage( cursor, pagedFile.PageSize() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustNotRunOutOfSwapperAllocationSpace() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustNotRunOutOfSwapperAllocationSpace()
		 {
			  assumeTrue( fs is EphemeralFileSystemAbstraction, "This test is file system agnostic, and too slow on a real file system" );
			  configureStandardPageCache();

			  File file = file( "a" );
			  int iterations = short.MaxValue * 3;
			  for ( int i = 0; i < iterations; i++ )
			  {
					PagedFile pagedFile = pageCache.map( file, filePageSize );
					using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursor.Next() );
					}
					pagedFile.Close();
			  }
		 }
	}

}