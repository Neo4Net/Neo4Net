using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Io.pagecache
{
	using RepeatedTest = org.junit.jupiter.api.RepeatedTest;
	using Test = org.junit.jupiter.api.Test;


	using Neo4Net.Functions;
	using Configuration = Neo4Net.GraphDb.config.Configuration;
	using DelegatingFileSystemAbstraction = Neo4Net.GraphDb.mockfs.DelegatingFileSystemAbstraction;
	using DelegatingStoreChannel = Neo4Net.GraphDb.mockfs.DelegatingStoreChannel;
	using EphemeralFileSystemAbstraction = Neo4Net.GraphDb.mockfs.EphemeralFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using FileIsNotMappedException = Neo4Net.Io.pagecache.impl.FileIsNotMappedException;
	using SingleFilePageSwapperFactory = Neo4Net.Io.pagecache.impl.SingleFilePageSwapperFactory;
	using Record = Neo4Net.Io.pagecache.randomharness.Record;
	using StandardRecordFormat = Neo4Net.Io.pagecache.randomharness.StandardRecordFormat;
	using Neo4Net.Io.pagecache.tracing;
	using DefaultPageCacheTracer = Neo4Net.Io.pagecache.tracing.DefaultPageCacheTracer;
	using PageCacheTracer = Neo4Net.Io.pagecache.tracing.PageCacheTracer;
	using PinEvent = Neo4Net.Io.pagecache.tracing.PinEvent;
	using DefaultPageCursorTracer = Neo4Net.Io.pagecache.tracing.cursor.DefaultPageCursorTracer;
	using DefaultPageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.DefaultPageCursorTracerSupplier;
	using PageCursorTracer = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracer;
	using PageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using BinaryLatch = Neo4Net.Utils.Concurrent.BinaryLatch;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.toHexString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.ArrayUtils.addAll;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.both;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.sameInstance;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNotSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTimeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assumptions.assumeTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.pagecache.PagedFile_Fields.PF_EAGER_FLUSH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.pagecache.PagedFile_Fields.PF_NO_FAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.pagecache.PagedFile_Fields.PF_NO_GROW;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.pagecache.PagedFile_Fields.PF_SHARED_WRITE_LOCK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.ThreadTestUtils.fork;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.matchers.ByteArrayMatcher.byteArray;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("OptionalGetWithoutIsPresent") public abstract class PageCacheTest<T extends PageCache> extends PageCacheTestSupport<T>
	public abstract class PageCacheTest<T> : PageCacheTestSupport<T> where T : PageCache
	{
		 // Sub-classes can override this. The reason this isn't a constructor parameter is that it would require this test class
		 // to have two constructors, one zero-parameter for junit and one internally with the intention to have one sub-class have its own
		 // zero-parameter constructor calling super constructor with a specific set of option options... and junit doesn't allow multiple
		 // constructors on a test class. Making this class abstract and have one sub-class with no specific open options and another for
		 // specific open options seemed a bit excessive, that's all.
		 protected internal OpenOption[] OpenOptions = new OpenOption[0];

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected PagedFile map(PageCache pageCache, java.io.File file, int filePageSize, java.nio.file.OpenOption... options) throws java.io.IOException
		 protected internal virtual PagedFile Map( PageCache pageCache, File file, int filePageSize, params OpenOption[] options )
		 {
			  return pageCache.Map( file, filePageSize, addAll( OpenOptions, options ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected PagedFile map(java.io.File file, int filePageSize, java.nio.file.OpenOption... options) throws java.io.IOException
		 protected internal virtual PagedFile Map( File file, int filePageSize, params OpenOption[] options )
		 {
			  return map( pageCache, file, filePageSize, options );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustReportConfiguredMaxPages()
		 internal virtual void MustReportConfiguredMaxPages()
		 {
			  configureStandardPageCache();
			  assertThat( pageCache.maxCachedPages(), @is((long) maxPages) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustReportConfiguredCachePageSize()
		 internal virtual void MustReportConfiguredCachePageSize()
		 {
			  configureStandardPageCache();
			  assertThat( pageCache.pageSize(), @is(pageCachePageSize) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustHaveAtLeastTwoPages()
		 internal virtual void MustHaveAtLeastTwoPages()
		 {
			  assertThrows( typeof( System.ArgumentException ), () => getPageCache(fs, 1, PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustAcceptTwoPagesAsMinimumConfiguration()
		 internal virtual void MustAcceptTwoPagesAsMinimumConfiguration()
		 {
			  getPageCache( fs, 2, PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void gettingNameFromMappedFileMustMatchMappedFileName() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void GettingNameFromMappedFileMustMatchMappedFileName()
		 {
			  configureStandardPageCache();
			  File file = file( "a" );
			  using ( PagedFile pf = map( file, filePageSize ) )
			  {
					assertThat( pf.File(), equalTo(file) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustClosePageSwapperFactoryOnPageCacheClose() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustClosePageSwapperFactoryOnPageCacheClose()
		 {
			  AtomicBoolean closed = new AtomicBoolean();
			  PageSwapperFactory swapperFactory = new SingleFilePageSwapperFactoryAnonymousInnerClass( this, closed );
			  PageCache cache = createPageCache( swapperFactory, maxPages, PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, EmptyVersionContextSupplier.EMPTY );
			  Exception exception = null;
			  try
			  {
					assertFalse( closed.get() );
			  }
			  catch ( Exception e )
			  {
					exception = e;
			  }
			  finally
			  {
					try
					{
						 cache.Close();
						 assertTrue( closed.get() );
					}
					catch ( Exception e )
					{
						 if ( exception == null )
						 {
							  exception = e;
						 }
						 else
						 {
							  exception.addSuppressed( e );
						 }
					}
					if ( exception != null )
					{
						 throw exception;
					}
			  }
		 }

		 private class SingleFilePageSwapperFactoryAnonymousInnerClass : SingleFilePageSwapperFactory
		 {
			 private readonly PageCacheTest<T> _outerInstance;

			 private AtomicBoolean _closed;

			 public SingleFilePageSwapperFactoryAnonymousInnerClass( PageCacheTest<T> outerInstance, AtomicBoolean closed )
			 {
				 this.outerInstance = outerInstance;
				 this._closed = closed;
			 }

			 public override void close()
			 {
				  _closed.set( true );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void closingOfPageCacheMustBeConsideredSuccessfulEvenIfPageSwapperFactoryCloseThrows()
		 internal virtual void ClosingOfPageCacheMustBeConsideredSuccessfulEvenIfPageSwapperFactoryCloseThrows()
		 {
			  AtomicInteger closed = new AtomicInteger();
			  PageSwapperFactory swapperFactory = new SingleFilePageSwapperFactoryAnonymousInnerClass2( this, closed );
			  PageCache cache = createPageCache( swapperFactory, maxPages, PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, EmptyVersionContextSupplier.EMPTY );
			  Exception exception = assertThrows( typeof( Exception ), cache.close );
			  assertThat( exception.Message, @is( "boo" ) );

			  // We must still consider this a success, and not call PageSwapperFactory.close() again
			  cache.Close();
		 }

		 private class SingleFilePageSwapperFactoryAnonymousInnerClass2 : SingleFilePageSwapperFactory
		 {
			 private readonly PageCacheTest<T> _outerInstance;

			 private AtomicInteger _closed;

			 public SingleFilePageSwapperFactoryAnonymousInnerClass2( PageCacheTest<T> outerInstance, AtomicInteger closed )
			 {
				 this.outerInstance = outerInstance;
				 this._closed = closed;
			 }

			 public override void close()
			 {
				  _closed.AndIncrement;
				  throw new Exception( "boo" );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustReadExistingData()
		 internal virtual void MustReadExistingData()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				generateFileWithRecords( file( "a" ), recordCount, recordSize );

				int recordId = 0;
				using ( PagedFile pagedFile = map( file( "a" ), filePageSize ), PageCursor cursor = pagedFile.Io( 0L, PF_SHARED_READ_LOCK ) )
				{
					 while ( cursor.Next() )
					 {
						  verifyRecordsMatchExpected( cursor );
						  recordId += recordsPerFilePage;
					 }
				}

				assertThat( recordId, @is( recordCount ) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustScanInTheMiddleOfTheFile()
		 internal virtual void MustScanInTheMiddleOfTheFile()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				long startPage = 10;
				long endPage = ( recordCount / recordsPerFilePage ) - 10;
				generateFileWithRecords( file( "a" ), recordCount, recordSize );

				int recordId = ( int )( startPage * recordsPerFilePage );
				using ( PagedFile pagedFile = map( file( "a" ), filePageSize ), PageCursor cursor = pagedFile.Io( startPage, PF_SHARED_READ_LOCK ) )
				{
					 while ( cursor.Next() && cursor.CurrentPageId < endPage )
					 {
						  verifyRecordsMatchExpected( cursor );
						  recordId += recordsPerFilePage;
					 }
				}

				assertThat( recordId, @is( recordCount - ( 10 * recordsPerFilePage ) ) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void writesFlushedFromPageFileMustBeExternallyObservable()
		 internal virtual void WritesFlushedFromPageFileMustBeExternallyObservable()
		 {
			  assertTimeout(ofMillis(SEMI_LONG_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();
				PagedFile pagedFile = map( file( "a" ), filePageSize );

				long startPageId = 0;
				long endPageId = recordCount / recordsPerFilePage;
				using ( PageCursor cursor = pagedFile.Io( startPageId, PF_SHARED_WRITE_LOCK ) )
				{
					 while ( cursor.CurrentPageId < endPageId && cursor.Next() )
					 {
						  writeRecords( cursor );
					 }
				}

				pagedFile.FlushAndForce();

				verifyRecordsInFile( file( "a" ), recordCount );
				pagedFile.Close();
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pageCacheFlushAndForceMustThrowOnNullIOPSLimiter()
		 internal virtual void PageCacheFlushAndForceMustThrowOnNullIOPSLimiter()
		 {
			  configureStandardPageCache();
			  assertThrows( typeof( System.ArgumentException ), () => pageCache.flushAndForce(null) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pagedFileFlushAndForceMustThrowOnNullIOPSLimiter() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void PagedFileFlushAndForceMustThrowOnNullIOPSLimiter()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ) )
			  {
					assertThrows( typeof( System.ArgumentException ), () => pf.flushAndForce(null) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pageCacheFlushAndForceMustQueryTheGivenIOPSLimiter() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void PageCacheFlushAndForceMustQueryTheGivenIOPSLimiter()
		 {
			  int pagesToDirty = 10_000;
			  PageCache cache = getPageCache( fs, NextPowerOf2( 2 * pagesToDirty ), PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null );
			  PagedFile pfA = cache.Map( existingFile( "a" ), filePageSize );
			  PagedFile pfB = cache.Map( existingFile( "b" ), filePageSize );

			  DirtyManyPages( pfA, pagesToDirty );
			  DirtyManyPages( pfB, pagesToDirty );

			  AtomicInteger callbackCounter = new AtomicInteger();
			  AtomicInteger ioCounter = new AtomicInteger();
			  cache.FlushAndForce((previousStamp, recentlyCompletedIOs, swapper) =>
			  {
				ioCounter.addAndGet( recentlyCompletedIOs );
				return callbackCounter.AndIncrement;
			  });
			  pfA.Close();
			  pfB.Close();

			  assertThat( callbackCounter.get(), greaterThan(0) );
			  assertThat( ioCounter.get(), greaterThanOrEqualTo(pagesToDirty * 2 - 30) ); // -30 because of the eviction thread
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pagedFileFlushAndForceMustQueryTheGivenIOPSLimiter() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void PagedFileFlushAndForceMustQueryTheGivenIOPSLimiter()
		 {
			  int pagesToDirty = 10_000;
			  PageCache cache = getPageCache( fs, NextPowerOf2( pagesToDirty ), PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null );
			  PagedFile pf = cache.Map( file( "a" ), filePageSize );

			  // Dirty a bunch of data
			  DirtyManyPages( pf, pagesToDirty );

			  AtomicInteger callbackCounter = new AtomicInteger();
			  AtomicInteger ioCounter = new AtomicInteger();
			  pf.FlushAndForce((previousStamp, recentlyCompletedIOs, swapper) =>
			  {
				ioCounter.addAndGet( recentlyCompletedIOs );
				return callbackCounter.AndIncrement;
			  });
			  pf.Close();

			  assertThat( callbackCounter.get(), greaterThan(0) );
			  assertThat( ioCounter.get(), greaterThanOrEqualTo(pagesToDirty - 30) ); // -30 because of the eviction thread
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void dirtyManyPages(PagedFile pf, int pagesToDirty) throws java.io.IOException
		 private void DirtyManyPages( PagedFile pf, int pagesToDirty )
		 {
			  using ( PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					for ( int i = 0; i < pagesToDirty; i++ )
					{
						 assertTrue( cursor.Next() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void writesFlushedFromPageFileMustBeObservableEvenWhenRacingWithEviction()
		 internal virtual void WritesFlushedFromPageFileMustBeObservableEvenWhenRacingWithEviction()
		 {
			  assertTimeout(ofMillis(LONG_TIMEOUT_MILLIS), () =>
			  {
				getPageCache( fs, 20, PageCacheTracer.NULL, PageCursorTracerSupplier.NULL );

				long startPageId = 0;
				long endPageId = 21;
				int iterations = 500;
				int shortsPerPage = pageCachePageSize / 2;

				using ( PagedFile pagedFile = map( file( "a" ), pageCachePageSize ) )
				{
					 for ( int i = 1; i <= iterations; i++ )
					 {
						  using ( PageCursor cursor = pagedFile.Io( startPageId, PF_SHARED_WRITE_LOCK ) )
						  {
								while ( cursor.CurrentPageId < endPageId && cursor.Next() )
								{
									 for ( int j = 0; j < shortsPerPage; j++ )
									 {
										  cursor.PutShort( ( short ) i );
									 }
								}
						  }

						  // There are 20 pages in the cache and we've overwritten 20 pages.
						  // This means eviction has probably fallen behind and is likely
						  // running concurrently right now.
						  // Therefor, a flush right now would have a high chance of racing
						  // with eviction.
						  pagedFile.FlushAndForce();

						  // Race or not, a flush should still put all changes in storage,
						  // so we should be able to verify the contents of the file.
						  using ( DataInputStream stream = new DataInputStream( fs.openAsInputStream( file( "a" ) ) ) )
						  {
								for ( int j = 0; j < shortsPerPage; j++ )
								{
									 int value = stream.readShort();
									 assertThat( "short pos = " + j + ", iteration = " + i, value, @is( i ) );
								}
						  }
					 }
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void flushAndForceMustNotLockPageCacheForWholeDuration()
		 internal virtual void FlushAndForceMustNotLockPageCacheForWholeDuration()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				maxPages = 5000;
				configureStandardPageCache();
				PageCache pageCache = this.pageCache;
				this.pageCache = null; // `null` out to prevent `tearDown` from getting stuck if test fails.
				File a = existingFile( "a" );
				File b = existingFile( "b" );
				using ( PagedFile pfA = map( pageCache, a, filePageSize ) )
				{
					 // Dirty a bunch of pages.
					 using ( PageCursor cursor = pfA.Io( 0, PF_SHARED_WRITE_LOCK ) )
					 {
						  for ( int i = 0; i < maxPages; i++ )
						  {
								assertTrue( cursor.Next() );
						  }
					 }

					 BinaryLatch limiterStartLatch = new BinaryLatch();
					 BinaryLatch limiterBlockLatch = new BinaryLatch();
					 Future<object> flusher = executor.submit(() =>
					 {
						  pageCache.FlushAndForce((stamp, ios, flushable) =>
						  {
								limiterStartLatch.release();
								limiterBlockLatch.await();
								return 0;
						  });
						  return null;
					 });

					 limiterStartLatch.await(); // Flusher is now stuck inside flushAndForce.

					 // We should be able to map and close paged files.
					 map( pageCache, b, filePageSize ).Close();
					 // We should be able to get and list existing mappings.
					 pageCache.ListExistingMappings();
					 pageCache.GetExistingMapping( a ).ifPresent(pf =>
					 {
						  try
						  {
								pf.close();
						  }
						  catch ( IOException e )
						  {
								throw new UncheckedIOException( e );
						  }
					 });

					 limiterBlockLatch.release();
					 flusher.get();
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void flushAndForceMustTolerateAsynchronousFileUnmapping() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void FlushAndForceMustTolerateAsynchronousFileUnmapping()
		 {
			  configureStandardPageCache();
			  PageCache pageCache = this.pageCache;
			  this.pageCache = null; // `null` out to prevent `tearDown` from getting stuck if test fails.
			  File a = existingFile( "a" );
			  File b = existingFile( "b" );
			  File c = existingFile( "c" );

			  BinaryLatch limiterStartLatch = new BinaryLatch();
			  BinaryLatch limiterBlockLatch = new BinaryLatch();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> flusher;
			  Future<object> flusher;

			  using ( PagedFile pfA = map( pageCache, a, filePageSize ), PagedFile pfB = map( pageCache, b, filePageSize ), PagedFile pfC = map( pageCache, c, filePageSize ) )
			  {
					// Dirty a bunch of pages.
					using ( PageCursor cursor = pfA.Io( 0, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursor.Next() );
					}
					using ( PageCursor cursor = pfB.Io( 0, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursor.Next() );
					}
					using ( PageCursor cursor = pfC.Io( 0, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursor.Next() );
					}
					flusher = executor.submit(() =>
					{
					 pageCache.FlushAndForce((stamp, ios, flushable) =>
					 {
						  limiterStartLatch.Release();
						  limiterBlockLatch.Await();
						  return 0;
					 });
					 return null;
					});

					limiterStartLatch.Await(); // Flusher is now stuck inside flushAndForce.
			  } // We should be able to unmap all the files.
			  // And then when the flusher resumes again, it should not throw any exceptions from the asynchronously
			  // closed files.
			  limiterBlockLatch.Release();
			  flusher.get(); // This must not throw.
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void writesFlushedFromPageCacheMustBeExternallyObservable()
		 internal virtual void WritesFlushedFromPageCacheMustBeExternallyObservable()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				long startPageId = 0;
				long endPageId = recordCount / recordsPerFilePage;
				File file = file( "a" );
				using ( PagedFile pagedFile = map( file, filePageSize ), PageCursor cursor = pagedFile.Io( startPageId, PF_SHARED_WRITE_LOCK ) )
				{
					 while ( cursor.CurrentPageId < endPageId && cursor.Next() )
					 {
						  writeRecords( cursor );
					 }
				} // closing the PagedFile implies flushing because it was the last reference

				verifyRecordsInFile( file, recordCount );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void writesToPagesMustNotBleedIntoAdjacentPages()
		 internal virtual void WritesToPagesMustNotBleedIntoAdjacentPages()
		 {
			  assertTimeout(ofMillis(SEMI_LONG_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				// Write the pageId+1 to every byte in the file
				using ( PagedFile pagedFile = map( file( "a" ), filePageSize ), PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
				{
					 for ( int i = 1; i <= 100; i++ )
					 {
						  assertTrue( cursor.Next() );
						  for ( int j = 0; j < filePageSize; j++ )
						  {
								cursor.PutByte( ( sbyte ) i );
						  }
					 }
				}

				// Then check that none of those writes ended up in adjacent pages
				Stream inputStream = fs.openAsInputStream( file( "a" ) );
				for ( int i = 1; i <= 100; i++ )
				{
					 for ( int j = 0; j < filePageSize; j++ )
					 {
						  assertThat( inputStream.read(), @is(i) );
					 }
				}
				inputStream.close();
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void channelMustBeForcedAfterPagedFileFlushAndForce() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ChannelMustBeForcedAfterPagedFileFlushAndForce()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger writeCounter = new java.util.concurrent.atomic.AtomicInteger();
			  AtomicInteger writeCounter = new AtomicInteger();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger forceCounter = new java.util.concurrent.atomic.AtomicInteger();
			  AtomicInteger forceCounter = new AtomicInteger();
			  FileSystemAbstraction fs = WriteAndForceCountingFs( writeCounter, forceCounter );

			  getPageCache( fs, maxPages, PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null );

			  using ( PagedFile pagedFile = map( file( "a" ), filePageSize ) )
			  {
					using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 cursor.PutInt( 1 );
						 assertTrue( cursor.Next() );
						 cursor.PutInt( 1 );
					}

					pagedFile.FlushAndForce();

					assertThat( writeCounter.get(), greaterThanOrEqualTo(2) ); // we might race with background flushing
					assertThat( forceCounter.get(), @is(1) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void channelsMustBeForcedAfterPageCacheFlushAndForce() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ChannelsMustBeForcedAfterPageCacheFlushAndForce()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger writeCounter = new java.util.concurrent.atomic.AtomicInteger();
			  AtomicInteger writeCounter = new AtomicInteger();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger forceCounter = new java.util.concurrent.atomic.AtomicInteger();
			  AtomicInteger forceCounter = new AtomicInteger();
			  FileSystemAbstraction fs = WriteAndForceCountingFs( writeCounter, forceCounter );

			  getPageCache( fs, maxPages, PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null );

			  using ( PagedFile pagedFileA = map( existingFile( "a" ), filePageSize ), PagedFile pagedFileB = map( existingFile( "b" ), filePageSize ) )
			  {
					using ( PageCursor cursor = pagedFileA.Io( 0, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 cursor.PutInt( 1 );
						 assertTrue( cursor.Next() );
						 cursor.PutInt( 1 );
					}
					using ( PageCursor cursor = pagedFileB.Io( 0, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 cursor.PutInt( 1 );
					}

					pageCache.flushAndForce();

					assertThat( writeCounter.get(), greaterThanOrEqualTo(3) ); // we might race with background flushing
					assertThat( forceCounter.get(), @is(2) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.Neo4Net.graphdb.mockfs.DelegatingFileSystemAbstraction writeAndForceCountingFs(final java.util.concurrent.atomic.AtomicInteger writeCounter, final java.util.concurrent.atomic.AtomicInteger forceCounter)
		 private DelegatingFileSystemAbstraction WriteAndForceCountingFs( AtomicInteger writeCounter, AtomicInteger forceCounter )
		 {
			  return new DelegatingFileSystemAbstractionAnonymousInnerClass( this, fs, writeCounter, forceCounter );
		 }

		 private class DelegatingFileSystemAbstractionAnonymousInnerClass : DelegatingFileSystemAbstraction
		 {
			 private readonly PageCacheTest<T> _outerInstance;

			 private AtomicInteger _writeCounter;
			 private AtomicInteger _forceCounter;

			 public DelegatingFileSystemAbstractionAnonymousInnerClass( PageCacheTest<T> outerInstance, UnknownType fs, AtomicInteger writeCounter, AtomicInteger forceCounter ) : base( fs )
			 {
				 this.outerInstance = outerInstance;
				 this._writeCounter = writeCounter;
				 this._forceCounter = forceCounter;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.io.fs.StoreChannel open(java.io.File fileName, org.Neo4Net.io.fs.OpenMode openMode) throws java.io.IOException
			 public override StoreChannel open( File fileName, OpenMode openMode )
			 {
				  return new DelegatingStoreChannelAnonymousInnerClass( this, base.open( fileName, openMode ) );
			 }

			 private class DelegatingStoreChannelAnonymousInnerClass : DelegatingStoreChannel
			 {
				 private readonly DelegatingFileSystemAbstractionAnonymousInnerClass _outerInstance;

				 public DelegatingStoreChannelAnonymousInnerClass( DelegatingFileSystemAbstractionAnonymousInnerClass outerInstance, UnknownType open ) : base( open )
				 {
					 this.outerInstance = outerInstance;
				 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeAll(ByteBuffer src, long position) throws java.io.IOException
				 public override void writeAll( ByteBuffer src, long position )
				 {
					  _outerInstance.writeCounter.AndIncrement;
					  base.writeAll( src, position );
				 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void force(boolean metaData) throws java.io.IOException
				 public override void force( bool metaData )
				 {
					  _outerInstance.forceCounter.AndIncrement;
					  base.force( metaData );
				 }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void firstNextCallMustReturnFalseWhenTheFileIsEmptyAndNoGrowIsSpecified()
		 internal virtual void FirstNextCallMustReturnFalseWhenTheFileIsEmptyAndNoGrowIsSpecified()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				using ( PagedFile pagedFile = map( file( "a" ), filePageSize ), PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
				{
					 assertFalse( cursor.Next() );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nextMustReturnTrueThenFalseWhenThereIsOnlyOnePageInTheFileAndNoGrowIsSpecified()
		 internal virtual void NextMustReturnTrueThenFalseWhenThereIsOnlyOnePageInTheFileAndNoGrowIsSpecified()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				int numberOfRecordsToGenerate = recordsPerFilePage; // one page worth
				generateFileWithRecords( file( "a" ), numberOfRecordsToGenerate, recordSize );

				using ( PagedFile pagedFile = map( file( "a" ), filePageSize ), PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
				{
					 assertTrue( cursor.Next() );
					 verifyRecordsMatchExpected( cursor );
					 assertFalse( cursor.Next() );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void closingWithoutCallingNextMustLeavePageUnpinnedAndUntouched()
		 internal virtual void ClosingWithoutCallingNextMustLeavePageUnpinnedAndUntouched()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				int numberOfRecordsToGenerate = recordsPerFilePage; // one page worth
				generateFileWithRecords( file( "a" ), numberOfRecordsToGenerate, recordSize );

				using ( PagedFile pagedFile = map( file( "a" ), filePageSize ) )
				{
					 //noinspection EmptyTryBlock
					 using ( PageCursor ignore = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
					 {
						  // No call to next, so the page should never get pinned in the first place, nor
						  // should the page corruption take place.
					 }

					 using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
					 {
						  // We didn't call next before, so the page and its records should still be fine
						  cursor.Next();
						  verifyRecordsMatchExpected( cursor );
					 }
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nextWithNegativeInitialPageIdMustReturnFalse() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void NextWithNegativeInitialPageIdMustReturnFalse()
		 {
			  configureStandardPageCache();
			  File file = file( "a" );
			  generateFileWithRecords( file, recordCount, recordSize );
			  using ( PagedFile pf = map( file, filePageSize ) )
			  {
					using ( PageCursor cursor = pf.Io( -1, PF_SHARED_WRITE_LOCK ) )
					{
						 assertFalse( cursor.Next() );
					}
					using ( PageCursor cursor = pf.Io( -1, PF_SHARED_READ_LOCK ) )
					{
						 assertFalse( cursor.Next() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nextWithNegativePageIdMustReturnFalse() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void NextWithNegativePageIdMustReturnFalse()
		 {
			  File file = file( "a" );
			  generateFileWithRecords( file, recordCount, recordSize );
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file, filePageSize ) )
			  {
					long pageId = 12;
					using ( PageCursor cursor = pf.Io( pageId, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 assertFalse( cursor.Next( -1 ) );
						 assertThat( cursor.CurrentPageId, @is( PageCursor.UNBOUND_PAGE_ID ) );
					}
					using ( PageCursor cursor = pf.Io( pageId, PF_SHARED_READ_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 assertFalse( cursor.Next( -1 ) );
						 assertThat( cursor.CurrentPageId, @is( PageCursor.UNBOUND_PAGE_ID ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void rewindMustStartScanningOverFromTheBeginning()
		 internal virtual void RewindMustStartScanningOverFromTheBeginning()
		 {
			  assertTimeout(ofMillis(SEMI_LONG_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				int numberOfRewindsToTest = 10;
				generateFileWithRecords( file( "a" ), recordCount, recordSize );
				int actualPageCounter = 0;
				int filePageCount = recordCount / recordsPerFilePage;
				int expectedPageCounterResult = numberOfRewindsToTest * filePageCount;

				using ( PagedFile pagedFile = map( file( "a" ), filePageSize ), PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
				{
					 for ( int i = 0; i < numberOfRewindsToTest; i++ )
					 {
						  while ( cursor.Next() )
						  {
								verifyRecordsMatchExpected( cursor );
								actualPageCounter++;
						  }
						  cursor.Rewind();
					 }
				}

				assertThat( actualPageCounter, @is( expectedPageCounterResult ) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustCloseFileChannelWhenTheLastHandleIsUnmapped()
		 internal virtual void MustCloseFileChannelWhenTheLastHandleIsUnmapped()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				assumeTrue( fs.GetType() == typeof(EphemeralFileSystemAbstraction), "This depends on EphemeralFSA specific features" );

				configureStandardPageCache();
				PagedFile a = map( file( "a" ), filePageSize );
				PagedFile b = map( file( "a" ), filePageSize );
				a.Close();
				b.Close();
				( ( EphemeralFileSystemAbstraction ) fs ).assertNoOpenFiles();
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void dirtyPagesMustBeFlushedWhenTheCacheIsClosed()
		 internal virtual void DirtyPagesMustBeFlushedWhenTheCacheIsClosed()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				long startPageId = 0;
				long endPageId = recordCount / recordsPerFilePage;
				File file = file( "a" );
				try
				{
					using ( PagedFile pagedFile = map( file, filePageSize ), PageCursor cursor = pagedFile.Io( startPageId, PF_SHARED_WRITE_LOCK ) )
					{
						 while ( cursor.CurrentPageId < endPageId && cursor.Next() )
						 {
							  writeRecords( cursor );
						 }
					}
				}
				finally
				{
					 //noinspection ThrowFromFinallyBlock
					 pageCache.close();
				}

				verifyRecordsInFile( file, recordCount );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void dirtyPagesMustBeFlushedWhenThePagedFileIsClosed()
		 internal virtual void DirtyPagesMustBeFlushedWhenThePagedFileIsClosed()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				long startPageId = 0;
				long endPageId = recordCount / recordsPerFilePage;
				File file = file( "a" );
				using ( PagedFile pagedFile = map( file, filePageSize ), PageCursor cursor = pagedFile.Io( startPageId, PF_SHARED_WRITE_LOCK ) )
				{
					 while ( cursor.CurrentPageId < endPageId && cursor.Next() )
					 {
						  writeRecords( cursor );
					 }
				}

				verifyRecordsInFile( file, recordCount );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RepeatedTest(100) void flushingDuringPagedFileCloseMustRetryUntilItSucceeds()
		 internal virtual void FlushingDuringPagedFileCloseMustRetryUntilItSucceeds()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				FileSystemAbstraction fs = new DelegatingFileSystemAbstractionAnonymousInnerClass( this, this.fs );
				getPageCache( fs, maxPages, PageCacheTracer.NULL, PageCursorTracerSupplier.NULL );
				PrintStream oldSystemErr = System.err;

				try
				{
					using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 writeRecords( cursor );
   
						 // Silence any stack traces the failed flushes might print.
						 System.Err = new PrintStream( new MemoryStream() );
					}
				}
				finally
				{
					 System.Err = oldSystemErr;
				}

				verifyRecordsInFile( file( "a" ), recordsPerFilePage );
			  });
		 }

		 private class DelegatingFileSystemAbstractionAnonymousInnerClass : DelegatingFileSystemAbstraction
		 {
			 private readonly PageCacheTest<T> _outerInstance;

			 public DelegatingFileSystemAbstractionAnonymousInnerClass( PageCacheTest<T> outerInstance, UnknownType fs ) : base( fs )
			 {
				 this._outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.io.fs.StoreChannel open(java.io.File fileName, org.Neo4Net.io.fs.OpenMode openMode) throws java.io.IOException
			 public override StoreChannel open( File fileName, OpenMode openMode )
			 {
				  return new DelegatingStoreChannelAnonymousInnerClass2( this, base.open( fileName, openMode ) );
			 }

			 private class DelegatingStoreChannelAnonymousInnerClass2 : DelegatingStoreChannel
			 {
				 private readonly DelegatingFileSystemAbstractionAnonymousInnerClass _outerInstance;

				 public DelegatingStoreChannelAnonymousInnerClass2( DelegatingFileSystemAbstractionAnonymousInnerClass outerInstance, UnknownType open ) : base( open )
				 {
					 this.outerInstance = outerInstance;
				 }

				 private int writeCount;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeAll(ByteBuffer src, long position) throws java.io.IOException
				 public override void writeAll( ByteBuffer src, long position )
				 {
					  if ( writeCount++ < 10 )
					  {
							throw new IOException( "This is a benign exception that we expect to be thrown " + "during a flush of a PagedFile." );
					  }
					  base.writeAll( src, position );
				 }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mappingFilesInClosedCacheMustThrow()
		 internal virtual void MappingFilesInClosedCacheMustThrow()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();
				pageCache.close();
				assertThrows( typeof( System.InvalidOperationException ), () => map(file("a"), filePageSize) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void flushingClosedCacheMustThrow()
		 internal virtual void FlushingClosedCacheMustThrow()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();
				pageCache.close();
				assertThrows( typeof( System.InvalidOperationException ), () => pageCache.flushAndForce() );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mappingFileWithPageSizeGreaterThanCachePageSizeMustThrow()
		 internal virtual void MappingFileWithPageSizeGreaterThanCachePageSizeMustThrow()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();
				assertThrows( typeof( System.ArgumentException ), () => map(file("a"), pageCachePageSize + 1) ); // this must throw;
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mappingFileWithPageSizeSmallerThanLongSizeBytesMustThrow()
		 internal virtual void MappingFileWithPageSizeSmallerThanLongSizeBytesMustThrow()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				// Because otherwise we cannot ensure that our branch-free bounds checking always lands within a page boundary.
				configureStandardPageCache();
				assertThrows( typeof( System.ArgumentException ), () => map(file("a"), Long.BYTES - 1) ); // this must throw;
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mappingFileWithPageSizeSmallerThanLongSizeBytesMustThrowEvenWithAnyPageSizeOpenOptionAndNoExistingMapping()
		 internal virtual void MappingFileWithPageSizeSmallerThanLongSizeBytesMustThrowEvenWithAnyPageSizeOpenOptionAndNoExistingMapping()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				// Because otherwise we cannot ensure that our branch-free bounds checking always lands within a page boundary.
				configureStandardPageCache();
				assertThrows( typeof( System.ArgumentException ), () => map(file("a"), Long.BYTES - 1, PageCacheOpenOptions.AnyPageSize) ); // this must throw;
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mappingFileWithPageZeroPageSizeMustThrowEvenWithExistingMapping()
		 internal virtual void MappingFileWithPageZeroPageSizeMustThrowEvenWithExistingMapping()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();
				File file = file( "a" );
				//noinspection unused
				using ( PagedFile oldMapping = map( file, filePageSize ) )
				{
					 assertThrows( typeof( System.ArgumentException ), () => map(file, Long.BYTES - 1) ); // this must throw
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mappingFileWithPageZeroPageSizeAndAnyPageSizeOpenOptionMustNotThrowGivenExistingMapping()
		 internal virtual void MappingFileWithPageZeroPageSizeAndAnyPageSizeOpenOptionMustNotThrowGivenExistingMapping()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();
				File file = file( "a" );
				//noinspection unused,EmptyTryBlock
				using ( PagedFile oldMapping = map( file, filePageSize ), PagedFile newMapping = map( file, 0, PageCacheOpenOptions.AnyPageSize ) )
				{
					 // All good
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mappingFileWithPageSizeEqualToCachePageSizeMustNotThrow()
		 internal virtual void MappingFileWithPageSizeEqualToCachePageSizeMustNotThrow()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();
				PagedFile pagedFile = map( file( "a" ), pageCachePageSize ); // this must NOT throw
				pagedFile.Close();
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void notSpecifyingAnyPfFlagsMustThrow()
		 internal virtual void NotSpecifyingAnyPfFlagsMustThrow()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();
				using ( PagedFile pagedFile = map( file( "a" ), filePageSize ) )
				{
					 assertThrows( typeof( System.ArgumentException ), () => pagedFile.Io(0, 0) ); // this must throw
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void notSpecifyingAnyPfLockFlagsMustThrow()
		 internal virtual void NotSpecifyingAnyPfLockFlagsMustThrow()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();
				using ( PagedFile pagedFile = map( file( "a" ), filePageSize ) )
				{
					 assertThrows( typeof( System.ArgumentException ), () => pagedFile.Io(0, PF_NO_FAULT) ); // this must throw
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void specifyingBothReadAndWriteLocksMustThrow()
		 internal virtual void SpecifyingBothReadAndWriteLocksMustThrow()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();
				using ( PagedFile pagedFile = map( file( "a" ), filePageSize ) )
				{
					 assertThrows( typeof( System.ArgumentException ), () => pagedFile.Io(0, PF_SHARED_WRITE_LOCK | PF_SHARED_READ_LOCK) ); // this must throw
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustNotPinPagesAfterNextReturnsFalse()
		 internal virtual void MustNotPinPagesAfterNextReturnsFalse()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				System.Threading.CountdownEvent startLatch = new System.Threading.CountdownEvent( 1 );
				System.Threading.CountdownEvent unpinLatch = new System.Threading.CountdownEvent( 1 );
				AtomicReference<Exception> exceptionRef = new AtomicReference<Exception>();
				configureStandardPageCache();
				generateFileWithRecords( file( "a" ), recordsPerFilePage, recordSize );
				PagedFile pagedFile = map( file( "a" ), filePageSize );

				ThreadStart runnable = () =>
				{
					 try
					 {
						 using ( PageCursor cursorA = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
						 {
							  assertTrue( cursorA.Next() );
							  assertFalse( cursorA.Next() );
							  startLatch.countDown();
							  unpinLatch.await();
						 }
					 }
					 catch ( Exception e )
					 {
						  exceptionRef.set( e );
					 }
				};
				executor.submit( runnable );

				startLatch.await();
				try
				{
					using ( PageCursor cursorB = pagedFile.Io( 1, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursorB.Next() );
						 unpinLatch.countDown();
					}
				}
				finally
				{
					 //noinspection ThrowFromFinallyBlock
					 pagedFile.Close();
				}
				Exception e = exceptionRef.get();
				if ( e != null )
				{
					 throw new Exception( "Child thread got exception", e );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nextMustResetTheCursorOffset()
		 internal virtual void NextMustResetTheCursorOffset()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();
				PagedFile pagedFile = map( file( "a" ), filePageSize );

				using ( PageCursor cursor = pagedFile.Io( 0L, PF_SHARED_WRITE_LOCK ) )
				{
					 assertTrue( cursor.Next() );
					 cursor.Offset = 0;
					 cursor.PutByte( ( sbyte ) 1 );
					 cursor.PutByte( ( sbyte ) 2 );
					 cursor.PutByte( ( sbyte ) 3 );
					 cursor.PutByte( ( sbyte ) 4 );

					 assertTrue( cursor.Next() );
					 cursor.Offset = 0;
					 cursor.PutByte( ( sbyte ) 5 );
					 cursor.PutByte( ( sbyte ) 6 );
					 cursor.PutByte( ( sbyte ) 7 );
					 cursor.PutByte( ( sbyte ) 8 );
				}

				using ( PageCursor cursor = pagedFile.Io( 0L, PF_SHARED_WRITE_LOCK ) )
				{
					 sbyte[] bytes = new sbyte[4];
					 assertTrue( cursor.Next() );
					 cursor.GetBytes( bytes );
					 assertThat( bytes, byteArray( new sbyte[]{ 1, 2, 3, 4 } ) );

					 assertTrue( cursor.Next() );
					 cursor.GetBytes( bytes );
					 assertThat( bytes, byteArray( new sbyte[]{ 5, 6, 7, 8 } ) );
				}
				pagedFile.Close();
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nextMustAdvanceCurrentPageId()
		 internal virtual void NextMustAdvanceCurrentPageId()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				using ( PagedFile pagedFile = map( file( "a" ), filePageSize ), PageCursor cursor = pagedFile.Io( 0L, PF_SHARED_WRITE_LOCK ) )
				{
					 assertTrue( cursor.Next() );
					 assertThat( cursor.CurrentPageId, @is( 0L ) );
					 assertTrue( cursor.Next() );
					 assertThat( cursor.CurrentPageId, @is( 1L ) );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nextToSpecificPageIdMustAdvanceFromThatPointOn()
		 internal virtual void NextToSpecificPageIdMustAdvanceFromThatPointOn()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				using ( PagedFile pagedFile = map( file( "a" ), filePageSize ), PageCursor cursor = pagedFile.Io( 1L, PF_SHARED_WRITE_LOCK ) )
				{
					 assertTrue( cursor.Next() );
					 assertThat( cursor.CurrentPageId, @is( 1L ) );
					 assertTrue( cursor.Next( 4L ) );
					 assertThat( cursor.CurrentPageId, @is( 4L ) );
					 assertTrue( cursor.Next() );
					 assertThat( cursor.CurrentPageId, @is( 5L ) );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void currentPageIdIsUnboundBeforeFirstNextAndAfterRewind()
		 internal virtual void CurrentPageIdIsUnboundBeforeFirstNextAndAfterRewind()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				using ( PagedFile pagedFile = map( file( "a" ), filePageSize ), PageCursor cursor = pagedFile.Io( 0L, PF_SHARED_WRITE_LOCK ) )
				{
					 assertThat( cursor.CurrentPageId, @is( PageCursor.UNBOUND_PAGE_ID ) );
					 assertTrue( cursor.Next() );
					 assertThat( cursor.CurrentPageId, @is( 0L ) );
					 cursor.Rewind();
					 assertThat( cursor.CurrentPageId, @is( PageCursor.UNBOUND_PAGE_ID ) );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pageCursorMustKnowCurrentFilePageSize()
		 internal virtual void PageCursorMustKnowCurrentFilePageSize()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				using ( PagedFile pagedFile = map( file( "a" ), filePageSize ), PageCursor cursor = pagedFile.Io( 0L, PF_SHARED_WRITE_LOCK ) )
				{
					 assertThat( cursor.CurrentPageSize, @is( PageCursor.UNBOUND_PAGE_SIZE ) );
					 assertTrue( cursor.Next() );
					 assertThat( cursor.CurrentPageSize, @is( filePageSize ) );
					 cursor.Rewind();
					 assertThat( cursor.CurrentPageSize, @is( PageCursor.UNBOUND_PAGE_SIZE ) );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pageCursorMustKnowCurrentFile()
		 internal virtual void PageCursorMustKnowCurrentFile()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				using ( PagedFile pagedFile = map( file( "a" ), filePageSize ), PageCursor cursor = pagedFile.Io( 0L, PF_SHARED_WRITE_LOCK ) )
				{
					 assertThat( cursor.CurrentFile, nullValue() );
					 assertTrue( cursor.Next() );
					 assertThat( cursor.CurrentFile, @is( file( "a" ) ) );
					 cursor.Rewind();
					 assertThat( cursor.CurrentFile, nullValue() );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void readingFromUnboundReadCursorMustThrow()
		 internal virtual void ReadingFromUnboundReadCursorMustThrow()
		 {
			  assertTimeout( ofMillis( SHORT_TIMEOUT_MILLIS ), () => verifyOnReadCursor(this.checkUnboundReadCursorAccess) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void readingFromUnboundWriteCursorMustThrow()
		 internal virtual void ReadingFromUnboundWriteCursorMustThrow()
		 {
			  assertTimeout( ofMillis( SHORT_TIMEOUT_MILLIS ), () => verifyOnReadCursor(this.checkUnboundWriteCursorAccess) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void readingFromPreviouslyBoundCursorMustThrow()
		 internal virtual void ReadingFromPreviouslyBoundCursorMustThrow()
		 {
			  assertTimeout( ofMillis( SHORT_TIMEOUT_MILLIS ), () => verifyOnReadCursor(this.checkPreviouslyBoundWriteCursorAccess) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void writingToUnboundCursorMustThrow()
		 internal virtual void WritingToUnboundCursorMustThrow()
		 {
			  assertTimeout( ofMillis( SHORT_TIMEOUT_MILLIS ), () => verifyOnWriteCursor(this.checkUnboundWriteCursorAccess) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void writingToPreviouslyBoundCursorMustThrow()
		 internal virtual void WritingToPreviouslyBoundCursorMustThrow()
		 {
			  assertTimeout( ofMillis( SHORT_TIMEOUT_MILLIS ), () => verifyOnWriteCursor(this.checkPreviouslyBoundWriteCursorAccess) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void readFromReadCursorAfterNextReturnsFalseMustThrow()
		 internal virtual void ReadFromReadCursorAfterNextReturnsFalseMustThrow()
		 {
			  assertTimeout( ofMillis( SHORT_TIMEOUT_MILLIS ), () => verifyOnReadCursor(this.checkReadCursorAfterFailedNext) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void readFromPreviouslyBoundReadCursorAfterNextReturnsFalseMustThrow()
		 internal virtual void ReadFromPreviouslyBoundReadCursorAfterNextReturnsFalseMustThrow()
		 {
			  assertTimeout( ofMillis( SHORT_TIMEOUT_MILLIS ), () => verifyOnReadCursor(this.checkPreviouslyBoundReadCursorAfterFailedNext) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void readFromWriteCursorAfterNextReturnsFalseMustThrow()
		 internal virtual void ReadFromWriteCursorAfterNextReturnsFalseMustThrow()
		 {
			  assertTimeout( ofMillis( SHORT_TIMEOUT_MILLIS ), () => verifyOnReadCursor(this.checkWriteCursorAfterFailedNext) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void readFromPreviouslyBoundWriteCursorAfterNextReturnsFalseMustThrow()
		 internal virtual void ReadFromPreviouslyBoundWriteCursorAfterNextReturnsFalseMustThrow()
		 {
			  assertTimeout( ofMillis( SHORT_TIMEOUT_MILLIS ), () => verifyOnReadCursor(this.checkPreviouslyBoundWriteCursorAfterFailedNext) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void writeAfterNextReturnsFalseMustThrow()
		 internal virtual void WriteAfterNextReturnsFalseMustThrow()
		 {
			  assertTimeout( ofMillis( SHORT_TIMEOUT_MILLIS ), () => verifyOnWriteCursor(this.checkWriteCursorAfterFailedNext) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void writeToPreviouslyBoundCursorAfterNextReturnsFalseMustThrow()
		 internal virtual void WriteToPreviouslyBoundCursorAfterNextReturnsFalseMustThrow()
		 {
			  assertTimeout( ofMillis( SHORT_TIMEOUT_MILLIS ), () => verifyOnWriteCursor(this.checkPreviouslyBoundWriteCursorAfterFailedNext) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyOnReadCursor(org.Neo4Net.function.ThrowingConsumer<PageCursorAction,java.io.IOException> testTemplate) throws java.io.IOException
		 private void VerifyOnReadCursor( ThrowingConsumer<PageCursorAction, IOException> testTemplate )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  testTemplate.Accept( PageCursor::getByte );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  testTemplate.Accept( PageCursor::getInt );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  testTemplate.Accept( PageCursor::getLong );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  testTemplate.Accept( PageCursor::getShort );
			  testTemplate.Accept( cursor => cursor.getByte( 0 ) );
			  testTemplate.Accept( cursor => cursor.getInt( 0 ) );
			  testTemplate.Accept( cursor => cursor.getLong( 0 ) );
			  testTemplate.Accept( cursor => cursor.getShort( 0 ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyOnWriteCursor(org.Neo4Net.function.ThrowingConsumer<PageCursorAction,java.io.IOException> testTemplate) throws java.io.IOException
		 private void VerifyOnWriteCursor( ThrowingConsumer<PageCursorAction, IOException> testTemplate )
		 {
			  testTemplate.Accept( cursor => cursor.putByte( ( sbyte ) 1 ) );
			  testTemplate.Accept( cursor => cursor.putInt( 1 ) );
			  testTemplate.Accept( cursor => cursor.putLong( 1 ) );
			  testTemplate.Accept( cursor => cursor.putShort( ( short ) 1 ) );
			  testTemplate.Accept( cursor => cursor.putByte( 0, ( sbyte ) 1 ) );
			  testTemplate.Accept( cursor => cursor.putInt( 0, 1 ) );
			  testTemplate.Accept( cursor => cursor.putLong( 0, 1 ) );
			  testTemplate.Accept( cursor => cursor.putShort( 0, ( short ) 1 ) );
			  testTemplate.Accept( PageCursor.zapPage );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void checkUnboundReadCursorAccess(PageCursorAction action) throws java.io.IOException
		 private void CheckUnboundReadCursorAccess( PageCursorAction action )
		 {
			  configureStandardPageCache();

			  using ( PagedFile pagedFile = map( file( "a" ), filePageSize ), PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
			  {
					action.Apply( cursor );
					assertTrue( cursor.CheckAndClearBoundsFlag() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void checkUnboundWriteCursorAccess(PageCursorAction action) throws java.io.IOException
		 private void CheckUnboundWriteCursorAccess( PageCursorAction action )
		 {
			  configureStandardPageCache();

			  using ( PagedFile pagedFile = map( file( "a" ), filePageSize ), PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					action.Apply( cursor );
					assertTrue( cursor.CheckAndClearBoundsFlag() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void checkPreviouslyBoundWriteCursorAccess(PageCursorAction action) throws java.io.IOException
		 private void CheckPreviouslyBoundWriteCursorAccess( PageCursorAction action )
		 {
			  configureStandardPageCache();

			  using ( PagedFile pagedFile = map( file( "a" ), filePageSize ) )
			  {
					PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK );
					assertTrue( cursor.Next() );
					action.Apply( cursor );
					assertFalse( cursor.CheckAndClearBoundsFlag() );
					cursor.Close();
					action.Apply( cursor );
					assertTrue( cursor.CheckAndClearBoundsFlag() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void checkReadCursorAfterFailedNext(PageCursorAction action) throws java.io.IOException
		 private void CheckReadCursorAfterFailedNext( PageCursorAction action )
		 {
			  configureStandardPageCache();

			  using ( PagedFile pagedFile = map( file( "a" ), filePageSize ), PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
			  {
					assertFalse( cursor.Next() );
					action.Apply( cursor );
					assertTrue( cursor.CheckAndClearBoundsFlag() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void checkPreviouslyBoundReadCursorAfterFailedNext(PageCursorAction action) throws java.io.IOException
		 private void CheckPreviouslyBoundReadCursorAfterFailedNext( PageCursorAction action )
		 {
			  configureStandardPageCache();

			  using ( PagedFile pagedFile = map( file( "a" ), filePageSize ), PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					assertTrue( cursor.Next() );
			  }

			  using ( PagedFile pagedFile = map( file( "a" ), filePageSize ), PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
			  {
					assertTrue( cursor.Next() );
					assertFalse( cursor.Next() );
					action.Apply( cursor );
					assertTrue( cursor.CheckAndClearBoundsFlag() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void checkWriteCursorAfterFailedNext(PageCursorAction action) throws java.io.IOException
		 private void CheckWriteCursorAfterFailedNext( PageCursorAction action )
		 {
			  configureStandardPageCache();

			  using ( PagedFile pagedFile = map( file( "a" ), filePageSize ), PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
			  {
					assertFalse( cursor.Next() );
					action.Apply( cursor );
					assertTrue( cursor.CheckAndClearBoundsFlag() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void checkPreviouslyBoundWriteCursorAfterFailedNext(PageCursorAction action) throws java.io.IOException
		 private void CheckPreviouslyBoundWriteCursorAfterFailedNext( PageCursorAction action )
		 {
			  configureStandardPageCache();

			  using ( PagedFile pagedFile = map( file( "a" ), filePageSize ), PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					assertTrue( cursor.Next() );
			  }

			  using ( PagedFile pagedFile = map( file( "a" ), filePageSize ), PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
			  {
					assertTrue( cursor.Next() );
					assertFalse( cursor.Next() );
					action.Apply( cursor );
					assertTrue( cursor.CheckAndClearBoundsFlag() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void tryMappedPagedFileShouldReportMappedFilePresent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TryMappedPagedFileShouldReportMappedFilePresent()
		 {
			  configureStandardPageCache();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File file = file("a");
			  File file = file( "a" );
			  using ( PagedFile pf = map( file, filePageSize ) )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Optional<PagedFile> optional = pageCache.getExistingMapping(file);
					Optional<PagedFile> optional = pageCache.getExistingMapping( file );
					assertTrue( optional.Present );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final PagedFile actual = optional.get();
					PagedFile actual = optional.get();
					assertThat( actual, sameInstance( pf ) );
					actual.Close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void tryMappedPagedFileShouldReportNonMappedFileNotPresent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TryMappedPagedFileShouldReportNonMappedFileNotPresent()
		 {
			  configureStandardPageCache();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Optional<PagedFile> dontExist = pageCache.getExistingMapping(new java.io.File("dont_exist"));
			  Optional<PagedFile> dontExist = pageCache.getExistingMapping( new File( "dont_exist" ) );
			  assertFalse( dontExist.Present );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustListExistingMappings() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustListExistingMappings()
		 {
			  configureStandardPageCache();
			  File f1 = existingFile( "1" );
			  File f2 = existingFile( "2" );
			  File f3 = existingFile( "3" ); // Not mapped at the time of calling listExistingMappings.
			  existingFile( "4" ); // Never mapped.
			  using ( PagedFile pf1 = map( f1, filePageSize ), PagedFile pf2 = map( f2, filePageSize ) )
			  {
					map( f3, filePageSize ).close();
					IList<PagedFile> existingMappings = pageCache.listExistingMappings();
					assertThat( existingMappings.Count, @is( 2 ) );
					assertThat( existingMappings, containsInAnyOrder( pf1, pf2 ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void listExistingMappingsMustNotIncrementPagedFileReferenceCount() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ListExistingMappingsMustNotIncrementPagedFileReferenceCount()
		 {
			  configureStandardPageCache();
			  File file = file( "a" );
			  PagedFile existingMapping;
			  using ( PagedFile pf = map( file, filePageSize ) )
			  {
					existingMapping = pageCache.listExistingMappings().get(0);
					using ( PageCursor cursor = existingMapping.Io( 0, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursor.Next() );
					}
			  }
			  // Now the mapping should be closed, which is signalled as an illegal state.
			  assertThrows( typeof( FileIsNotMappedException ), () => existingMapping.Io(0, PF_SHARED_WRITE_LOCK).next() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void listExistingMappingsMustThrowOnClosedPageCache() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ListExistingMappingsMustThrowOnClosedPageCache()
		 {
			  configureStandardPageCache();
			  T pc = pageCache;
			  pageCache = null;
			  pc.Close();
			  assertThrows( typeof( System.InvalidOperationException ), pc.listExistingMappings );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void lastPageMustBeAccessibleWithNoGrowSpecified()
		 internal virtual void LastPageMustBeAccessibleWithNoGrowSpecified()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				generateFileWithRecords( file( "a" ), recordsPerFilePage * 2, recordSize );

				using ( PagedFile pagedFile = map( file( "a" ), filePageSize ) )
				{
					 using ( PageCursor cursor = pagedFile.Io( 0L, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
					 {
						  assertTrue( cursor.Next() );
						  assertTrue( cursor.Next() );
						  assertFalse( cursor.Next() );
					 }

					 using ( PageCursor cursor = pagedFile.Io( 0L, PF_SHARED_READ_LOCK ) )
					 {
						  assertTrue( cursor.Next() );
						  assertTrue( cursor.Next() );
						  assertFalse( cursor.Next() );
					 }

					 using ( PageCursor cursor = pagedFile.Io( 1L, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
					 {
						  assertTrue( cursor.Next() );
						  assertFalse( cursor.Next() );
					 }

					 using ( PageCursor cursor = pagedFile.Io( 1L, PF_SHARED_READ_LOCK ) )
					 {
						  assertTrue( cursor.Next() );
						  assertFalse( cursor.Next() );
					 }

					 using ( PageCursor cursor = pagedFile.Io( 2L, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
					 {
						  assertFalse( cursor.Next() );
					 }

					 using ( PageCursor cursor = pagedFile.Io( 2L, PF_SHARED_READ_LOCK ) )
					 {
						  assertFalse( cursor.Next() );
					 }

					 using ( PageCursor cursor = pagedFile.Io( 3L, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
					 {
						  assertFalse( cursor.Next() );
					 }

					 using ( PageCursor cursor = pagedFile.Io( 3L, PF_SHARED_READ_LOCK ) )
					 {
						  assertFalse( cursor.Next() );
					 }
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void lastPageMustBeAccessibleWithNoGrowSpecifiedEvenIfLessThanFilePageSize()
		 internal virtual void LastPageMustBeAccessibleWithNoGrowSpecifiedEvenIfLessThanFilePageSize()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				generateFileWithRecords( file( "a" ), ( recordsPerFilePage * 2 ) - 1, recordSize );
				using ( PagedFile pagedFile = map( file( "a" ), filePageSize ) )
				{
					 using ( PageCursor cursor = pagedFile.Io( 0L, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
					 {
						  assertTrue( cursor.Next() );
						  assertTrue( cursor.Next() );
						  assertFalse( cursor.Next() );
					 }

					 using ( PageCursor cursor = pagedFile.Io( 0L, PF_SHARED_READ_LOCK ) )
					 {
						  assertTrue( cursor.Next() );
						  assertTrue( cursor.Next() );
						  assertFalse( cursor.Next() );
					 }

					 using ( PageCursor cursor = pagedFile.Io( 1L, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
					 {
						  assertTrue( cursor.Next() );
						  assertFalse( cursor.Next() );
					 }

					 using ( PageCursor cursor = pagedFile.Io( 1L, PF_SHARED_READ_LOCK ) )
					 {
						  assertTrue( cursor.Next() );
						  assertFalse( cursor.Next() );
					 }

					 using ( PageCursor cursor = pagedFile.Io( 2L, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
					 {
						  assertFalse( cursor.Next() );
					 }

					 using ( PageCursor cursor = pagedFile.Io( 2L, PF_SHARED_READ_LOCK ) )
					 {
						  assertFalse( cursor.Next() );
					 }

					 using ( PageCursor cursor = pagedFile.Io( 3L, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
					 {
						  assertFalse( cursor.Next() );
					 }

					 using ( PageCursor cursor = pagedFile.Io( 3L, PF_SHARED_READ_LOCK ) )
					 {
						  assertFalse( cursor.Next() );
					 }
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void firstPageMustBeAccessibleWithNoGrowSpecifiedIfItIsTheOnlyPage()
		 internal virtual void FirstPageMustBeAccessibleWithNoGrowSpecifiedIfItIsTheOnlyPage()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				generateFileWithRecords( file( "a" ), recordsPerFilePage, recordSize );

				using ( PagedFile pagedFile = map( file( "a" ), filePageSize ) )
				{
					 using ( PageCursor cursor = pagedFile.Io( 0L, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
					 {
						  assertTrue( cursor.Next() );
						  assertFalse( cursor.Next() );
					 }

					 using ( PageCursor cursor = pagedFile.Io( 0L, PF_SHARED_READ_LOCK ) )
					 {
						  assertTrue( cursor.Next() );
						  assertFalse( cursor.Next() );
					 }

					 using ( PageCursor cursor = pagedFile.Io( 1L, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
					 {
						  assertFalse( cursor.Next() );
					 }

					 using ( PageCursor cursor = pagedFile.Io( 1L, PF_SHARED_READ_LOCK ) )
					 {
						  assertFalse( cursor.Next() );
					 }
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void firstPageMustBeAccessibleEvenIfTheFileIsNonEmptyButSmallerThanFilePageSize()
		 internal virtual void FirstPageMustBeAccessibleEvenIfTheFileIsNonEmptyButSmallerThanFilePageSize()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				generateFileWithRecords( file( "a" ), 1, recordSize );

				configureStandardPageCache();

				using ( PagedFile pagedFile = map( file( "a" ), filePageSize ) )
				{
					 using ( PageCursor cursor = pagedFile.Io( 0L, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
					 {
						  assertTrue( cursor.Next() );
						  assertFalse( cursor.Next() );
					 }

					 using ( PageCursor cursor = pagedFile.Io( 0L, PF_SHARED_READ_LOCK ) )
					 {
						  assertTrue( cursor.Next() );
						  assertFalse( cursor.Next() );
					 }

					 using ( PageCursor cursor = pagedFile.Io( 1L, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
					 {
						  assertFalse( cursor.Next() );
					 }

					 using ( PageCursor cursor = pagedFile.Io( 1L, PF_SHARED_READ_LOCK ) )
					 {
						  assertFalse( cursor.Next() );
					 }
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void firstPageMustNotBeAccessibleIfFileIsEmptyAndNoGrowSpecified()
		 internal virtual void FirstPageMustNotBeAccessibleIfFileIsEmptyAndNoGrowSpecified()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				using ( PagedFile pagedFile = map( file( "a" ), filePageSize ) )
				{
					 using ( PageCursor cursor = pagedFile.Io( 0L, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
					 {
						  assertFalse( cursor.Next() );
						  assertFalse( cursor.Next() );
					 }

					 using ( PageCursor cursor = pagedFile.Io( 0L, PF_SHARED_READ_LOCK ) )
					 {
						  assertFalse( cursor.Next() );
						  assertFalse( cursor.Next() );
					 }

					 using ( PageCursor cursor = pagedFile.Io( 1L, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
					 {
						  assertFalse( cursor.Next() );
					 }

					 using ( PageCursor cursor = pagedFile.Io( 1L, PF_SHARED_READ_LOCK ) )
					 {
						  assertFalse( cursor.Next() );
					 }
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void newlyWrittenPagesMustBeAccessibleWithNoGrow()
		 internal virtual void NewlyWrittenPagesMustBeAccessibleWithNoGrow()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				int initialPages = 1;
				int pagesToAdd = 3;
				generateFileWithRecords( file( "a" ), recordsPerFilePage * initialPages, recordSize );

				PagedFile pagedFile = map( file( "a" ), filePageSize );

				using ( PageCursor cursor = pagedFile.Io( 1L, PF_SHARED_WRITE_LOCK ) )
				{
					 for ( int i = 0; i < pagesToAdd; i++ )
					 {
						  assertTrue( cursor.Next() );
						  writeRecords( cursor );
					 }
				}

				int pagesChecked = 0;
				using ( PageCursor cursor = pagedFile.Io( 0L, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
				{
					 while ( cursor.Next() )
					 {
						  verifyRecordsMatchExpected( cursor );
						  pagesChecked++;
					 }
				}
				assertThat( pagesChecked, @is( initialPages + pagesToAdd ) );

				pagesChecked = 0;
				using ( PageCursor cursor = pagedFile.Io( 0L, PF_SHARED_READ_LOCK ) )
				{
					 while ( cursor.Next() )
					 {
						  verifyRecordsMatchExpected( cursor );
						  pagesChecked++;
					 }
				}
				assertThat( pagesChecked, @is( initialPages + pagesToAdd ) );
				pagedFile.Close();
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void readLockImpliesNoGrow()
		 internal virtual void ReadLockImpliesNoGrow()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				int initialPages = 3;
				generateFileWithRecords( file( "a" ), recordsPerFilePage * initialPages, recordSize );

				int pagesChecked = 0;
				using ( PagedFile pagedFile = map( file( "a" ), filePageSize ), PageCursor cursor = pagedFile.Io( 0L, PF_SHARED_READ_LOCK ) )
				{
					 while ( cursor.Next() )
					 {
						  pagesChecked++;
					 }
				}
				assertThat( pagesChecked, @is( initialPages ) );
			  });
		 }

		 // This test has an internal timeout in that it tries to verify 1000 reads within SHORT_TIMEOUT_MILLIS,
		 // although this is a soft limit in that it may abort if number of verifications isn't reached.
		 // This is so because on some machines this test takes a very long time to run. Verifying in the end
		 // that at least there were some correct reads is good enough.
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void retryMustResetCursorOffset() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void RetryMustResetCursorOffset()
		 {
			  // The general idea here, is that we have a page with a particular value in its 0th position.
			  // We also have a thread that constantly writes to the middle of the page, so it modifies
			  // the page, but does not change the value in the 0th position. This thread will in principle
			  // mean that it is possible for a reader to get an inconsistent view and must retry.
			  // We then check that every retry iteration will read the special value in the 0th position.
			  // We repeat the experiment a couple of times to make sure we didn't succeed by chance.

			  configureStandardPageCache();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final PagedFile pagedFile = map(file("a"), filePageSize);
			  PagedFile pagedFile = map( file( "a" ), filePageSize );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicReference<Exception> caughtWriterException = new java.util.concurrent.atomic.AtomicReference<>();
			  AtomicReference<Exception> caughtWriterException = new AtomicReference<Exception>();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch startLatch = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent startLatch = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte expectedByte = (byte) 13;
			  sbyte expectedByte = ( sbyte ) 13;

			  using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					if ( cursor.Next() )
					{
						 cursor.PutByte( expectedByte );
					}
			  }

			  AtomicBoolean end = new AtomicBoolean( false );
			  ThreadStart writer = () =>
			  {
				while ( !end.get() )
				{
					 try
					 {
						 using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
						 {
							  if ( cursor.Next() )
							  {
									cursor.Offset = recordSize;
									cursor.PutByte( ( sbyte ) 14 );
							  }
							  startLatch.Signal();
						 }
					 }
					 catch ( IOException e )
					 {
						  caughtWriterException.set( e );
						  throw new Exception( e );
					 }
				}
			  };
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> writerFuture = executor.submit(writer);
			  Future<object> writerFuture = executor.submit( writer );

			  startLatch.await();

			  long timeout = currentTimeMillis() + SHORT_TIMEOUT_MILLIS;
			  int i = 0;
			  for ( ; i < 1000 && currentTimeMillis() < timeout; i++ )
			  {
					using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 do
						 {
							  assertThat( cursor.Byte, @is( expectedByte ) );
						 } while ( cursor.ShouldRetry() && currentTimeMillis() < timeout );
					}
			  }

			  end.set( true );
			  writerFuture.get();
			  assertTrue( i > 1 );
			  pagedFile.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nextWithPageIdMustAllowTraversingInReverse()
		 internal virtual void NextWithPageIdMustAllowTraversingInReverse()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();
				generateFileWithRecords( file( "a" ), recordCount, recordSize );
				long lastFilePageId = ( recordCount / recordsPerFilePage ) - 1;

				using ( PagedFile pagedFile = map( file( "a" ), filePageSize ), PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
				{
					 for ( long currentPageId = lastFilePageId; currentPageId >= 0; currentPageId-- )
					 {
						  assertTrue( cursor.Next( currentPageId ), "next( currentPageId = " + currentPageId + " )" );
						  assertThat( cursor.CurrentPageId, @is( currentPageId ) );
						  verifyRecordsMatchExpected( cursor );
					 }
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nextWithPageIdMustReturnFalseIfPageIdIsBeyondFilePageRangeAndNoGrowSpecified()
		 internal virtual void NextWithPageIdMustReturnFalseIfPageIdIsBeyondFilePageRangeAndNoGrowSpecified()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				generateFileWithRecords( file( "a" ), recordsPerFilePage * 2, recordSize );

				using ( PagedFile pagedFile = map( file( "a" ), filePageSize ) )
				{
					 using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
					 {
						  assertFalse( cursor.Next( 2 ) );
						  assertTrue( cursor.Next( 1 ) );
					 }

					 using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
					 {
						  assertFalse( cursor.Next( 2 ) );
						  assertTrue( cursor.Next( 1 ) );
					 }
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pagesAddedWithNextWithPageIdMustBeAccessibleWithNoGrowSpecified()
		 internal virtual void PagesAddedWithNextWithPageIdMustBeAccessibleWithNoGrowSpecified()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();
				PagedFile pagedFile = map( file( "a" ), filePageSize );

				using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
				{
					 assertTrue( cursor.Next( 2 ) );
					 writeRecords( cursor );
					 assertTrue( cursor.Next( 0 ) );
					 writeRecords( cursor );
					 assertTrue( cursor.Next( 1 ) );
					 writeRecords( cursor );
				}

				using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
				{
					 while ( cursor.Next() )
					 {
						  verifyRecordsMatchExpected( cursor );
					 }
				}

				using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
				{
					 while ( cursor.Next() )
					 {
						  verifyRecordsMatchExpected( cursor );
					 }
				}
				pagedFile.Close();
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void writesOfDifferentUnitsMustHaveCorrectEndianness()
		 internal virtual void WritesOfDifferentUnitsMustHaveCorrectEndianness()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();
				using ( PagedFile pagedFile = map( file( "a" ), 23 ) )
				{
					 using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
					 {
						  assertTrue( cursor.Next() );
						  sbyte[] data = new sbyte[] { 42, 43, 44, 45, 46 };

						  cursor.PutLong( 41 ); //  0+8 = 8
						  cursor.PutInt( 41 ); //  8+4 = 12
						  cursor.PutShort( ( short ) 41 ); // 12+2 = 14
						  cursor.PutByte( ( sbyte ) 41 ); // 14+1 = 15
						  cursor.PutBytes( data ); // 15+5 = 20
						  cursor.PutBytes( 3, ( sbyte ) 47 ); // 20+3 = 23
					 }
					 using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
					 {
						  assertTrue( cursor.Next() );

						  long a = cursor.Long; //  8
						  int b = cursor.Int; // 12
						  short c = cursor.Short; // 14
						  sbyte[] data = new sbyte[]{ cursor.Byte, cursor.Byte, cursor.Byte, cursor.Byte, cursor.Byte, cursor.Byte };
						  sbyte d = cursor.Byte; // 21
						  sbyte e = cursor.Byte; // 22
						  sbyte f = cursor.Byte; // 23
						  cursor.Offset = 0;
						  cursor.PutLong( 1 + a );
						  cursor.PutInt( 1 + b );
						  cursor.PutShort( ( short )( 1 + c ) );
						  foreach ( sbyte g in data )
						  {
								g++;
								cursor.PutByte( g );
						  }
						  cursor.PutByte( ( sbyte )( 1 + d ) );
						  cursor.PutByte( ( sbyte )( 1 + e ) );
						  cursor.PutByte( ( sbyte )( 1 + f ) );
					 }
				}

				ByteBuffer buf = ByteBuffer.allocate( 23 );
				using ( StoreChannel channel = fs.open( file( "a" ), OpenMode.READ ) )
				{
					 channel.readAll( buf );
				}
				buf.flip();

				assertThat( buf.Long, @is( 42L ) );
				assertThat( buf.Int, @is( 42 ) );
				assertThat( buf.Short, @is( ( short ) 42 ) );
				assertThat( buf.get(), @is((sbyte) 42) );
				assertThat( buf.get(), @is((sbyte) 43) );
				assertThat( buf.get(), @is((sbyte) 44) );
				assertThat( buf.get(), @is((sbyte) 45) );
				assertThat( buf.get(), @is((sbyte) 46) );
				assertThat( buf.get(), @is((sbyte) 47) );
				assertThat( buf.get(), @is((sbyte) 48) );
				assertThat( buf.get(), @is((sbyte) 48) );
				assertThat( buf.get(), @is((sbyte) 48) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mappingFileSecondTimeWithLesserPageSizeMustThrow()
		 internal virtual void MappingFileSecondTimeWithLesserPageSizeMustThrow()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();
				using ( PagedFile ignore = map( file( "a" ), filePageSize ) )
				{
					 assertThrows( typeof( System.ArgumentException ), () => map(file("a"), filePageSize - 1) );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mappingFileSecondTimeWithGreaterPageSizeMustThrow()
		 internal virtual void MappingFileSecondTimeWithGreaterPageSizeMustThrow()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();
				using ( PagedFile ignore = map( file( "a" ), filePageSize ) )
				{
					 assertThrows( typeof( System.ArgumentException ), () => map(file("a"), filePageSize + 1) );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void allowOpeningMultipleReadAndWriteCursorsPerThread()
		 internal virtual void AllowOpeningMultipleReadAndWriteCursorsPerThread()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				File fileA = existingFile( "a" );
				File fileB = existingFile( "b" );

				generateFileWithRecords( fileA, 1, 16 );
				generateFileWithRecords( fileB, 1, 16 );

				using ( PagedFile pfA = map( fileA, filePageSize ), PagedFile pfB = map( fileB, filePageSize ), PageCursor a = pfA.Io( 0, PF_SHARED_READ_LOCK ), PageCursor b = pfA.Io( 0, PF_SHARED_READ_LOCK ), PageCursor c = pfA.Io( 0, PF_SHARED_WRITE_LOCK ), PageCursor d = pfA.Io( 0, PF_SHARED_WRITE_LOCK ), PageCursor e = pfB.Io( 0, PF_SHARED_READ_LOCK ), PageCursor f = pfB.Io( 0, PF_SHARED_READ_LOCK ), PageCursor g = pfB.Io( 0, PF_SHARED_WRITE_LOCK ), PageCursor h = pfB.Io( 0, PF_SHARED_WRITE_LOCK ) )
				{
					 assertTrue( a.Next() );
					 assertTrue( b.Next() );
					 assertTrue( c.Next() );
					 assertTrue( d.Next() );
					 assertTrue( e.Next() );
					 assertTrue( f.Next() );
					 assertTrue( g.Next() );
					 assertTrue( h.Next() );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustNotLiveLockIfWeRunOutOfEvictablePages()
		 internal virtual void MustNotLiveLockIfWeRunOutOfEvictablePages()
		 {
			  assertTimeout(ofMillis(SEMI_LONG_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				IList<PageCursor> cursors = new LinkedList<PageCursor>();
				using ( PagedFile pf = map( existingFile( "a" ), filePageSize ) )
				{
					 try
					 {
						  assertThrows(typeof(IOException), () =>
						  {
								//noinspection InfiniteLoopStatement
								for ( long i = 0; ; i++ )
								{
									 PageCursor cursor = pf.Io( i, PF_SHARED_WRITE_LOCK );
									 cursors.add( cursor );
									 assertTrue( cursor.Next() );
								}
						  });
					 }
					 finally
					 {
						  foreach ( PageCursor cursor in cursors )
						  {
								cursor.Close();
						  }
					 }
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void writeLocksMustNotBeExclusive()
		 internal virtual void WriteLocksMustNotBeExclusive()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				using ( PagedFile pf = map( existingFile( "a" ), filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
				{
					 assertTrue( cursor.Next() );

					 executor.submit(() =>
					 {
						  using ( PageCursor innerCursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
						  {
								assertTrue( innerCursor.Next() );
						  }
						  return null;
					 }).get();
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void writeLockMustInvalidateInnerReadLock()
		 internal virtual void WriteLockMustInvalidateInnerReadLock()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				using ( PagedFile pf = map( existingFile( "a" ), filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
				{
					 assertTrue( cursor.Next() );

					 executor.submit(() =>
					 {
						  using ( PageCursor innerCursor = pf.Io( 0, PF_SHARED_READ_LOCK ) )
						  {
								assertTrue( innerCursor.Next() );
								assertTrue( innerCursor.ShouldRetry() );
						  }
						  return null;
					 }).get();
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void writeLockMustInvalidateExistingReadLock()
		 internal virtual void WriteLockMustInvalidateExistingReadLock()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				BinaryLatch startLatch = new BinaryLatch();
				BinaryLatch continueLatch = new BinaryLatch();

				using ( PagedFile pf = map( existingFile( "a" ), filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
				{
					 assertTrue( cursor.Next() ); // Ensure that page 0 exists so the read cursor can get it
					 assertTrue( cursor.Next() ); // Then unlock it

					 Future<object> read = executor.submit(() =>
					 {
						  using ( PageCursor innerCursor = pf.Io( 0, PF_SHARED_READ_LOCK ) )
						  {
								assertTrue( innerCursor.Next() );
								assertFalse( innerCursor.ShouldRetry() );
								startLatch.release();
								continueLatch.await();
								assertTrue( innerCursor.ShouldRetry() );
						  }
						  return null;
					 });

					 startLatch.await();
					 assertTrue( cursor.Next( 0 ) ); // Re-take the write lock on page 0.
					 continueLatch.release();
					 read.get();
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void writeUnlockMustInvalidateReadLocks()
		 internal virtual void WriteUnlockMustInvalidateReadLocks()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				BinaryLatch startLatch = new BinaryLatch();
				BinaryLatch continueLatch = new BinaryLatch();

				using ( PagedFile pf = map( existingFile( "a" ), filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
				{
					 assertTrue( cursor.Next() ); // Lock page 0

					 Future<object> read = executor.submit(() =>
					 {
						  using ( PageCursor innerCursor = pf.Io( 0, PF_SHARED_READ_LOCK ) )
						  {
								assertTrue( innerCursor.Next() );
								assertTrue( innerCursor.ShouldRetry() );
								startLatch.release();
								continueLatch.await();
								assertTrue( innerCursor.ShouldRetry() );
						  }
						  return null;
					 });

					 startLatch.await();
					 assertTrue( cursor.Next() ); // Unlock page 0
					 continueLatch.release();
					 read.get();
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustNotFlushCleanPagesWhenEvicting()
		 internal virtual void MustNotFlushCleanPagesWhenEvicting()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				AtomicBoolean observedWrite = new AtomicBoolean();
				FileSystemAbstraction fs = new DelegatingFileSystemAbstractionAnonymousInnerClass2( this, this.fs, observedWrite );
				getPageCache( fs, maxPages, PageCacheTracer.NULL, PageCursorTracerSupplier.NULL );
				generateFileWithRecords( file( "a" ), recordCount, recordSize );

				using ( PagedFile pagedFile = map( file( "a" ), filePageSize ), PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
				{
					 while ( cursor.Next() )
					 {
						  verifyRecordsMatchExpected( cursor );
					 }
				}
				assertFalse( observedWrite.get() );
			  });
		 }

		 private class DelegatingFileSystemAbstractionAnonymousInnerClass2 : DelegatingFileSystemAbstraction
		 {
			 private readonly PageCacheTest<T> _outerInstance;

			 private AtomicBoolean _observedWrite;

			 public DelegatingFileSystemAbstractionAnonymousInnerClass2( PageCacheTest<T> outerInstance, UnknownType fs, AtomicBoolean observedWrite ) : base( fs )
			 {
				 this.outerInstance = outerInstance;
				 this._observedWrite = observedWrite;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.io.fs.StoreChannel open(java.io.File fileName, org.Neo4Net.io.fs.OpenMode openMode) throws java.io.IOException
			 public override StoreChannel open( File fileName, OpenMode openMode )
			 {
				  StoreChannel channel = base.open( fileName, openMode );
				  return new DelegatingStoreChannelAnonymousInnerClass3( this, channel );
			 }

			 private class DelegatingStoreChannelAnonymousInnerClass3 : DelegatingStoreChannel
			 {
				 private readonly DelegatingFileSystemAbstractionAnonymousInnerClass2 _outerInstance;

				 public DelegatingStoreChannelAnonymousInnerClass3( DelegatingFileSystemAbstractionAnonymousInnerClass2 outerInstance, StoreChannel channel ) : base( channel )
				 {
					 this.outerInstance = outerInstance;
				 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long write(ByteBuffer[] srcs, int offset, int length) throws java.io.IOException
				 public override long write( ByteBuffer[] srcs, int offset, int length )
				 {
					  _outerInstance.observedWrite.set( true );
					  throw new IOException( "not allowed" );
				 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeAll(ByteBuffer src, long position) throws java.io.IOException
				 public override void writeAll( ByteBuffer src, long position )
				 {
					  _outerInstance.observedWrite.set( true );
					  throw new IOException( "not allowed" );
				 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeAll(ByteBuffer src) throws java.io.IOException
				 public override void writeAll( ByteBuffer src )
				 {
					  _outerInstance.observedWrite.set( true );
					  throw new IOException( "not allowed" );
				 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public int write(ByteBuffer src) throws java.io.IOException
				 public override int write( ByteBuffer src )
				 {
					  _outerInstance.observedWrite.set( true );
					  throw new IOException( "not allowed" );
				 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long write(ByteBuffer[] srcs) throws java.io.IOException
				 public override long write( ByteBuffer[] srcs )
				 {
					  _outerInstance.observedWrite.set( true );
					  throw new IOException( "not allowed" );
				 }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void evictionMustFlushPagesToTheRightFiles()
		 internal virtual void EvictionMustFlushPagesToTheRightFiles()
		 {
			  assertTimeout(ofMillis(SEMI_LONG_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				generateFileWithRecords( file( "a" ), recordCount, recordSize );

				int filePageSize2 = filePageSize - 3; // diff. page size just to be difficult
				long maxPageIdCursor1 = recordCount / recordsPerFilePage;
				File file2 = file( "b" );
				long file2sizeBytes = ( maxPageIdCursor1 + 17 ) * filePageSize2;
				using ( Stream outputStream = fs.openAsOutputStream( file2, false ) )
				{
					 for ( int i = 0; i < file2sizeBytes; i++ )
					 {
						  // We will ues the page cache to change these 'a's into 'b's.
						  outputStream.write( 'a' );
					 }
					 outputStream.flush();
				}

				using ( PagedFile pagedFile1 = Map( file( "a" ), filePageSize ), PagedFile pagedFile2 = Map( file2, filePageSize2 ) )
				{
					 long pageId1 = 0;
					 long pageId2 = 0;
					 bool moreWorkToDo;
					 do
					 {
						  bool cursorReady1;
						  bool cursorReady2;

						  using ( PageCursor cursor = pagedFile1.Io( pageId1, PF_SHARED_WRITE_LOCK ) )
						  {
								cursorReady1 = cursor.Next() && cursor.CurrentPageId < maxPageIdCursor1;
								if ( cursorReady1 )
								{
									 writeRecords( cursor );
									 pageId1++;
								}
						  }

						  using ( PageCursor cursor = pagedFile2.Io( pageId2, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
						  {
								cursorReady2 = cursor.Next();
								if ( cursorReady2 )
								{
									 for ( int i = 0; i < filePageSize2; i++ )
									 {
										  cursor.PutByte( ( sbyte ) 'b' );
									 }
									 assertFalse( cursor.ShouldRetry() );
								}
								pageId2++;
						  }

						  moreWorkToDo = cursorReady1 || cursorReady2;
					 } while ( moreWorkToDo );
				}

				// Verify the file contents
				assertThat( fs.getFileSize( file2 ), @is( file2sizeBytes ) );
				using ( Stream inputStream = fs.openAsInputStream( file2 ) )
				{
					 for ( int i = 0; i < file2sizeBytes; i++ )
					 {
						  int b = inputStream.read();
						  assertThat( b, @is( ( int ) 'b' ) );
					 }
					 assertThat( inputStream.read(), @is(-1) );
				}

				using ( StoreChannel channel = fs.open( file( "a" ), OpenMode.READ ) )
				{
					 ByteBuffer bufB = ByteBuffer.allocate( recordSize );
					 for ( int i = 0; i < recordCount; i++ )
					 {
						  bufA.clear();
						  channel.readAll( bufA );
						  bufA.flip();
						  bufB.clear();
						  generateRecordForId( i, bufB );
						  assertThat( bufB.array(), byteArray(bufA.array()) );
					 }
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void tracerMustBeNotifiedAboutPinUnpinFaultAndEvictEventsWhenReading()
		 internal virtual void TracerMustBeNotifiedAboutPinUnpinFaultAndEvictEventsWhenReading()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				DefaultPageCacheTracer tracer = new DefaultPageCacheTracer();
				DefaultPageCursorTracerSupplier cursorTracerSupplier = GetCursorTracerSupplier( tracer );
				getPageCache( fs, maxPages, tracer, cursorTracerSupplier );

				generateFileWithRecords( file( "a" ), recordCount, recordSize );

				long countedPages = 0;
				long countedFaults = 0;
				using ( PagedFile pagedFile = map( file( "a" ), filePageSize ), PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
				{
					 while ( cursor.Next() )
					 {
						  countedPages++;
						  countedFaults++;
					 }

					 // Using next( pageId ) to the already-pinned page id does not count,
					 // so we only increment once for this section
					 countedPages++;
					 for ( int i = 0; i < 20; i++ )
					 {
						  assertTrue( cursor.Next( 1 ) );
					 }

					 // But if we use next( pageId ) to a page that is different from the one already pinned,
					 // then it counts
					 for ( int i = 0; i < 20; i++ )
					 {
						  assertTrue( cursor.Next( i ) );
						  countedPages++;
					 }
				}

				pageCache.reportEvents();

				assertThat( "wrong count of pins", tracer.pins(), @is(countedPages) );
				assertThat( "wrong count of unpins", tracer.unpins(), @is(countedPages) );

				// We might be unlucky and fault in the second next call, on the page
				// we brought up in the first next call. That's why we assert that we
				// have observed *at least* the countedPages number of faults.
				long faults = tracer.faults();
				long bytesRead = tracer.bytesRead();
				assertThat( "wrong count of faults", faults, greaterThanOrEqualTo( countedFaults ) );
				assertThat( "wrong number of bytes read", bytesRead, greaterThanOrEqualTo( countedFaults * filePageSize ) );
				// Every page we move forward can put the freelist behind so the cache
				// wants to evict more pages. Plus, every page fault we do could also
				// block and get a page directly transferred to it, and these kinds of
				// evictions can count in addition to the evictions we do when the
				// cache is behind on keeping the freelist full.
				assertThat( "wrong count of evictions", tracer.evictions(), both(greaterThanOrEqualTo(countedFaults - maxPages)).and(lessThanOrEqualTo(countedPages + faults)) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void tracerMustBeNotifiedAboutPinUnpinFaultFlushAndEvictionEventsWhenWriting()
		 internal virtual void TracerMustBeNotifiedAboutPinUnpinFaultFlushAndEvictionEventsWhenWriting()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				long pagesToGenerate = 142;
				DefaultPageCacheTracer tracer = new DefaultPageCacheTracer();
				DefaultPageCursorTracerSupplier tracerSupplier = GetCursorTracerSupplier( tracer );
				getPageCache( fs, maxPages, tracer, tracerSupplier );

				using ( PagedFile pagedFile = map( file( "a" ), filePageSize ), PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
				{
					 for ( long i = 0; i < pagesToGenerate; i++ )
					 {
						  assertTrue( cursor.Next() );
						  assertThat( cursor.CurrentPageId, @is( i ) );
						  assertTrue( cursor.Next( i ) ); // This does not count as a pin
						  assertThat( cursor.CurrentPageId, @is( i ) );

						  writeRecords( cursor );
					 }

					 // This counts as a single pin
					 assertTrue( cursor.Next( 0 ) );
					 assertTrue( cursor.Next( 0 ) );
				}
				pageCache.reportEvents();

				assertThat( "wrong count of pins", tracer.pins(), @is(pagesToGenerate + 1) );
				assertThat( "wrong count of unpins", tracer.unpins(), @is(pagesToGenerate + 1) );

				// We might be unlucky and fault in the second next call, on the page
				// we brought up in the first next call. That's why we assert that we
				// have observed *at least* the countedPages number of faults.
				long faults = tracer.faults();
				assertThat( "wrong count of faults", faults, greaterThanOrEqualTo( pagesToGenerate ) );
				// Every page we move forward can put the freelist behind so the cache
				// wants to evict more pages. Plus, every page fault we do could also
				// block and get a page directly transferred to it, and these kinds of
				// evictions can count in addition to the evictions we do when the
				// cache is behind on keeping the freelist full.
				assertThat( "wrong count of evictions", tracer.evictions(), both(greaterThanOrEqualTo(pagesToGenerate - maxPages)).and(lessThanOrEqualTo(pagesToGenerate + faults)) );

				// We use greaterThanOrEqualTo because we visit each page twice, and
				// that leaves a small window wherein we can race with eviction, have
				// the evictor flush the page, and then fault it back and mark it as
				// dirty again.
				// We also subtract 'maxPages' from the expected flush count, because
				// vectored IO may coalesce all the flushes we do as part of unmapping
				// the file, into a single flush.
				long flushes = tracer.flushes();
				long bytesWritten = tracer.bytesWritten();
				assertThat( "wrong count of flushes", flushes, greaterThanOrEqualTo( pagesToGenerate - maxPages ) );
				assertThat( "wrong count of bytes written", bytesWritten, greaterThanOrEqualTo( pagesToGenerate * filePageSize ) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void tracerMustBeNotifiedOfReadAndWritePins() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TracerMustBeNotifiedOfReadAndWritePins()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger writeCount = new java.util.concurrent.atomic.AtomicInteger();
			  AtomicInteger writeCount = new AtomicInteger();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger readCount = new java.util.concurrent.atomic.AtomicInteger();
			  AtomicInteger readCount = new AtomicInteger();

			  DefaultPageCacheTracer tracer = new DefaultPageCacheTracer();
			  DefaultPageCursorTracer pageCursorTracer = new DefaultPageCursorTracerAnonymousInnerClass( this, writeCount, readCount );
			  ConfigurablePageCursorTracerSupplier<DefaultPageCursorTracer> cursorTracerSupplier = new ConfigurablePageCursorTracerSupplier<DefaultPageCursorTracer>( pageCursorTracer );
			  getPageCache( fs, maxPages, tracer, cursorTracerSupplier );

			  generateFileWithRecords( file( "a" ), recordCount, recordSize );

			  int pinsForRead = 13;
			  int pinsForWrite = 42;

			  using ( PagedFile pagedFile = map( file( "a" ), filePageSize ) )
			  {
					using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
					{
						 for ( int i = 0; i < pinsForRead; i++ )
						 {
							  assertTrue( cursor.Next() );
						 }
					}

					DirtyManyPages( pagedFile, pinsForWrite );
			  }

			  pageCache.reportEvents(); // Reset thread-local event counters.
			  assertThat( "wrong read pin count", readCount.get(), @is(pinsForRead) );
			  assertThat( "wrong write pin count", writeCount.get(), @is(pinsForWrite) );
		 }

		 private class DefaultPageCursorTracerAnonymousInnerClass : DefaultPageCursorTracer
		 {
			 private readonly PageCacheTest<T> _outerInstance;

			 private AtomicInteger _writeCount;
			 private AtomicInteger _readCount;

			 public DefaultPageCursorTracerAnonymousInnerClass( PageCacheTest<T> outerInstance, AtomicInteger writeCount, AtomicInteger readCount )
			 {
				 this.outerInstance = outerInstance;
				 this._writeCount = writeCount;
				 this._readCount = readCount;
			 }

			 public override PinEvent beginPin( bool writeLock, long filePageId, PageSwapper swapper )
			 {
				  ( writeLock ? _writeCount : _readCount ).AndIncrement;
				  return base.beginPin( writeLock, filePageId, swapper );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void lastPageIdOfEmptyFileIsLessThanZero() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void LastPageIdOfEmptyFileIsLessThanZero()
		 {
			  configureStandardPageCache();

			  using ( PagedFile pagedFile = map( file( "a" ), filePageSize ) )
			  {
					assertThat( pagedFile.LastPageId, lessThan( 0L ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void lastPageIdOfFileWithOneByteIsZero() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void LastPageIdOfFileWithOneByteIsZero()
		 {
			  StoreChannel channel = fs.create( file( "a" ) );
			  channel.write( ByteBuffer.wrap( new sbyte[]{ 1 } ) );
			  channel.close();

			  configureStandardPageCache();

			  using ( PagedFile pagedFile = map( file( "a" ), filePageSize ) )
			  {
					assertThat( pagedFile.LastPageId, @is( 0L ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void lastPageIdOfFileWithExactlyTwoPagesWorthOfDataIsOne() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void LastPageIdOfFileWithExactlyTwoPagesWorthOfDataIsOne()
		 {
			  configureStandardPageCache();

			  int twoPagesWorthOfRecords = recordsPerFilePage * 2;
			  generateFileWithRecords( file( "a" ), twoPagesWorthOfRecords, recordSize );

			  using ( PagedFile pagedFile = map( file( "a" ), filePageSize ) )
			  {
					assertThat( pagedFile.LastPageId, @is( 1L ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void lastPageIdOfFileWithExactlyTwoPagesAndOneByteWorthOfDataIsTwo() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void LastPageIdOfFileWithExactlyTwoPagesAndOneByteWorthOfDataIsTwo()
		 {
			  configureStandardPageCache();

			  int twoPagesWorthOfRecords = recordsPerFilePage * 2;
			  generateFileWithRecords( file( "a" ), twoPagesWorthOfRecords, recordSize );
			  Stream outputStream = fs.openAsOutputStream( file( "a" ), true );
			  outputStream.WriteByte( 'a' );
			  outputStream.Close();

			  using ( PagedFile pagedFile = map( file( "a" ), filePageSize ) )
			  {
					assertThat( pagedFile.LastPageId, @is( 2L ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void lastPageIdMustNotIncreaseWhenReadingToEndWithReadLock() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void LastPageIdMustNotIncreaseWhenReadingToEndWithReadLock()
		 {
			  configureStandardPageCache();
			  generateFileWithRecords( file( "a" ), recordCount, recordSize );
			  PagedFile pagedFile = map( file( "a" ), filePageSize );

			  long initialLastPageId = pagedFile.LastPageId;
			  using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
			  {
					//noinspection StatementWithEmptyBody
					while ( cursor.Next() )
					{
						 // scan through the lot
					}
			  }
			  long resultingLastPageId = pagedFile.LastPageId;
			  pagedFile.Close();
			  assertThat( resultingLastPageId, @is( initialLastPageId ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void lastPageIdMustNotIncreaseWhenReadingToEndWithNoGrowAndWriteLock() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void LastPageIdMustNotIncreaseWhenReadingToEndWithNoGrowAndWriteLock()
		 {
			  configureStandardPageCache();
			  generateFileWithRecords( file( "a" ), recordCount, recordSize );
			  PagedFile pagedFile = map( file( "a" ), filePageSize );

			  long initialLastPageId = pagedFile.LastPageId;
			  using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
			  {
					//noinspection StatementWithEmptyBody
					while ( cursor.Next() )
					{
						 // scan through the lot
					}
			  }
			  long resultingLastPageId = pagedFile.LastPageId;

			  try
			  {
					assertThat( resultingLastPageId, @is( initialLastPageId ) );
			  }
			  finally
			  {
					//noinspection ThrowFromFinallyBlock
					pagedFile.Close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void lastPageIdMustIncreaseWhenScanningPastEndWithWriteLock() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void LastPageIdMustIncreaseWhenScanningPastEndWithWriteLock()
		 {
			  configureStandardPageCache();
			  generateFileWithRecords( file( "a" ), recordsPerFilePage * 10, recordSize );
			  PagedFile pagedFile = map( file( "a" ), filePageSize );

			  assertThat( pagedFile.LastPageId, @is( 9L ) );
			  DirtyManyPages( pagedFile, 15 );
			  try
			  {
					assertThat( pagedFile.LastPageId, @is( 14L ) );
			  }
			  finally
			  {
					//noinspection ThrowFromFinallyBlock
					pagedFile.Close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void lastPageIdMustIncreaseWhenJumpingPastEndWithWriteLock() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void LastPageIdMustIncreaseWhenJumpingPastEndWithWriteLock()
		 {
			  configureStandardPageCache();
			  generateFileWithRecords( file( "a" ), recordsPerFilePage * 10, recordSize );
			  PagedFile pagedFile = map( file( "a" ), filePageSize );

			  assertThat( pagedFile.LastPageId, @is( 9L ) );
			  using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					assertTrue( cursor.Next( 15 ) );
			  }
			  try
			  {
					assertThat( pagedFile.LastPageId, @is( 15L ) );
			  }
			  finally
			  {
					//noinspection ThrowFromFinallyBlock
					pagedFile.Close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void lastPageIdFromUnmappedFileMustThrow() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void LastPageIdFromUnmappedFileMustThrow()
		 {
			  configureStandardPageCache();

			  PagedFile file;
			  using ( PagedFile pf = map( file( "a" ), filePageSize, StandardOpenOption.CREATE ) )
			  {
					file = pf;
			  }

			  assertThrows( typeof( FileIsNotMappedException ), file.getLastPageId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void cursorOffsetMustBeUpdatedReadAndWrite() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CursorOffsetMustBeUpdatedReadAndWrite()
		 {
			  configureStandardPageCache();

			  using ( PagedFile pagedFile = map( file( "a" ), filePageSize ) )
			  {
					using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 VerifyWriteOffsets( cursor );

						 cursor.Offset = 0;
						 VerifyReadOffsets( cursor );
					}

					using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 VerifyReadOffsets( cursor );
					}
			  }
		 }

		 private void VerifyWriteOffsets( PageCursor cursor )
		 {
			  cursor.Offset = filePageSize / 2;
			  cursor.ZapPage();
			  assertThat( cursor.Offset, @is( filePageSize / 2 ) );
			  cursor.Offset = 0;
			  cursor.PutLong( 1 );
			  assertThat( cursor.Offset, @is( 8 ) );
			  cursor.PutInt( 1 );
			  assertThat( cursor.Offset, @is( 12 ) );
			  cursor.PutShort( ( short ) 1 );
			  assertThat( cursor.Offset, @is( 14 ) );
			  cursor.PutByte( ( sbyte ) 1 );
			  assertThat( cursor.Offset, @is( 15 ) );
			  cursor.PutBytes( new sbyte[]{ 1, 2, 3 } );
			  assertThat( cursor.Offset, @is( 18 ) );
			  cursor.PutBytes( new sbyte[]{ 1, 2, 3 }, 1, 1 );
			  assertThat( cursor.Offset, @is( 19 ) );
			  cursor.PutBytes( 5, ( sbyte ) 1 );
			  assertThat( cursor.Offset, @is( 24 ) );
		 }

		 private void VerifyReadOffsets( PageCursor cursor )
		 {
			  assertThat( cursor.Offset, @is( 0 ) );
			  cursor.Long;
			  assertThat( cursor.Offset, @is( 8 ) );
			  cursor.Int;
			  assertThat( cursor.Offset, @is( 12 ) );
			  cursor.Short;
			  assertThat( cursor.Offset, @is( 14 ) );
			  cursor.Byte;
			  assertThat( cursor.Offset, @is( 15 ) );
			  cursor.GetBytes( new sbyte[3] );
			  assertThat( cursor.Offset, @is( 18 ) );
			  cursor.GetBytes( new sbyte[3], 1, 1 );
			  assertThat( cursor.Offset, @is( 19 ) );
			  cursor.GetBytes( new sbyte[5] );
			  assertThat( cursor.Offset, @is( 24 ) );

			  sbyte[] expectedBytes = new sbyte[]{ 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 1, 1, 1, 2, 3, 2, 1, 1, 1, 1, 1 };
			  sbyte[] actualBytes = new sbyte[24];
			  cursor.Offset = 0;
			  cursor.GetBytes( actualBytes );
			  assertThat( actualBytes, byteArray( expectedBytes ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void closeOnPageCacheMustThrowIfFilesAreStillMapped()
		 internal virtual void CloseOnPageCacheMustThrowIfFilesAreStillMapped()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				using ( PagedFile ignore = map( file( "a" ), filePageSize ) )
				{
					 assertThrows( typeof( System.InvalidOperationException ), () => pageCache.close() );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pagedFileIoMustThrowIfFileIsUnmapped()
		 internal virtual void PagedFileIoMustThrowIfFileIsUnmapped()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				PagedFile pagedFile = map( file( "a" ), filePageSize );
				CloseThisPagedFile( pagedFile );

				using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
				{
					 FileIsNotMappedException exception = assertThrows( typeof( FileIsNotMappedException ), cursor.next );
					 StringWriter @out = new StringWriter();
					 exception.printStackTrace( new PrintWriter( @out ) );
					 assertThat( @out.ToString(), containsString("closeThisPagedFile") );
				}
			  });
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void closeThisPagedFile(PagedFile pagedFile) throws java.io.IOException
		 private void CloseThisPagedFile( PagedFile pagedFile )
		 {
			  pagedFile.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void writeLockedPageCursorNextMustThrowIfFileIsUnmapped()
		 internal virtual void WriteLockedPageCursorNextMustThrowIfFileIsUnmapped()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				PagedFile pagedFile = map( file( "a" ), filePageSize );
				PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK );
				CloseThisPagedFile( pagedFile );

				FileIsNotMappedException exception = assertThrows( typeof( FileIsNotMappedException ), cursor.next );
				StringWriter @out = new StringWriter();
				exception.printStackTrace( new PrintWriter( @out ) );
				assertThat( @out.ToString(), containsString("closeThisPagedFile") );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void writeLockedPageCursorNextWithIdMustThrowIfFileIsUnmapped()
		 internal virtual void WriteLockedPageCursorNextWithIdMustThrowIfFileIsUnmapped()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				PagedFile pagedFile = map( file( "a" ), filePageSize );
				PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK );
				pagedFile.Close();

				assertThrows( typeof( FileIsNotMappedException ), () => cursor.Next(1) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void readLockedPageCursorNextMustThrowIfFileIsUnmapped()
		 internal virtual void ReadLockedPageCursorNextMustThrowIfFileIsUnmapped()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				generateFileWithRecords( file( "a" ), 1, recordSize );

				PagedFile pagedFile = map( file( "a" ), filePageSize );
				PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK );
				pagedFile.Close();

				assertThrows( typeof( FileIsNotMappedException ), cursor.next );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void readLockedPageCursorNextWithIdMustThrowIfFileIsUnmapped()
		 internal virtual void ReadLockedPageCursorNextWithIdMustThrowIfFileIsUnmapped()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				generateFileWithRecords( file( "a" ), recordsPerFilePage * 2, recordSize );

				PagedFile pagedFile = map( file( "a" ), filePageSize );
				PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK );
				pagedFile.Close();

				assertThrows( typeof( FileIsNotMappedException ), () => cursor.Next(1) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void writeLockedPageMustBlockFileUnmapping()
		 internal virtual void WriteLockedPageMustBlockFileUnmapping()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				PagedFile pagedFile = map( file( "a" ), filePageSize );
				PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK );
				assertTrue( cursor.Next() );

				Thread unmapper = fork( closePageFile( pagedFile ) );
				unmapper.Join( 100 );

				cursor.Close();
				unmapper.Join();
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void optimisticReadLockedPageMustNotBlockFileUnmapping()
		 internal virtual void OptimisticReadLockedPageMustNotBlockFileUnmapping()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				generateFileWithRecords( file( "a" ), 1, recordSize );

				configureStandardPageCache();

				PagedFile pagedFile = map( file( "a" ), filePageSize );
				PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK );
				assertTrue( cursor.Next() ); // Got a read lock

				fork( closePageFile( pagedFile ) ).join();

				cursor.Close();
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void advancingPessimisticReadLockingCursorAfterUnmappingMustThrow()
		 internal virtual void AdvancingPessimisticReadLockingCursorAfterUnmappingMustThrow()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				generateFileWithRecords( file( "a" ), recordsPerFilePage * 2, recordSize );

				PagedFile pagedFile = map( file( "a" ), filePageSize );
				PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK );
				assertTrue( cursor.Next() ); // Got a pessimistic read lock

				fork( closePageFile( pagedFile ) ).join();

				assertThrows( typeof( FileIsNotMappedException ), cursor.next );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void advancingOptimisticReadLockingCursorAfterUnmappingMustThrow()
		 internal virtual void AdvancingOptimisticReadLockingCursorAfterUnmappingMustThrow()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				generateFileWithRecords( file( "a" ), recordsPerFilePage * 2, recordSize );

				PagedFile pagedFile = map( file( "a" ), filePageSize );
				PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK );
				assertTrue( cursor.Next() ); // fault
				assertTrue( cursor.Next() ); // fault + unpin page 0
				assertTrue( cursor.Next( 0 ) ); // potentially optimistic read lock page 0

				fork( closePageFile( pagedFile ) ).join();

				assertThrows( typeof( FileIsNotMappedException ), cursor.next );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void readingAndRetryingOnPageWithOptimisticReadLockingAfterUnmappingMustNotThrow()
		 internal virtual void ReadingAndRetryingOnPageWithOptimisticReadLockingAfterUnmappingMustNotThrow()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				generateFileWithRecords( file( "a" ), recordsPerFilePage * 2, recordSize );

				PagedFile pagedFile = map( file( "a" ), filePageSize );
				PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK );
				assertTrue( cursor.Next() ); // fault
				assertTrue( cursor.Next() ); // fault + unpin page 0
				assertTrue( cursor.Next( 0 ) ); // potentially optimistic read lock page 0

				fork( closePageFile( pagedFile ) ).join();
				pageCache.close();
				pageCache = null;

				cursor.Byte;
				cursor.ShouldRetry();
				assertThrows( typeof( FileIsNotMappedException ), cursor.next );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRetryFromUnboundReadCursorMustNotThrow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRetryFromUnboundReadCursorMustNotThrow()
		 {
			  File file = file( "a" );
			  generateFileWithRecords( file, recordsPerFilePage, recordSize );
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file, filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_READ_LOCK ) )
			  {
					assertFalse( cursor.ShouldRetry() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRetryFromUnboundWriteCursorMustNotThrow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRetryFromUnboundWriteCursorMustNotThrow()
		 {
			  configureStandardPageCache();
			  File file = file( "a" );
			  generateFileWithRecords( file, recordsPerFilePage, recordSize );
			  using ( PagedFile pf = map( file, filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					assertFalse( cursor.ShouldRetry() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRetryFromUnboundLinkedReadCursorMustNotThrow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRetryFromUnboundLinkedReadCursorMustNotThrow()
		 {
			  configureStandardPageCache();
			  File file = file( "a" );
			  generateFileWithRecords( file, recordsPerFilePage * 2, recordSize );
			  using ( PagedFile pf = map( file, filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_READ_LOCK ) )
			  {
					assertTrue( cursor.Next() );
					//noinspection unused
					using ( PageCursor linked = cursor.OpenLinkedCursor( 1 ) )
					{
						 assertFalse( cursor.ShouldRetry() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRetryFromUnboundLinkedWriteCursorMustNotThrow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRetryFromUnboundLinkedWriteCursorMustNotThrow()
		 {
			  configureStandardPageCache();
			  File file = file( "a" );
			  generateFileWithRecords( file, recordsPerFilePage * 2, recordSize );
			  using ( PagedFile pf = map( file, filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					assertTrue( cursor.Next() );
					//noinspection unused
					using ( PageCursor linked = cursor.OpenLinkedCursor( 1 ) )
					{
						 assertFalse( cursor.ShouldRetry() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRetryOnWriteParentOfClosedLinkedCursorMustNotThrow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRetryOnWriteParentOfClosedLinkedCursorMustNotThrow()
		 {
			  configureStandardPageCache();
			  File file = file( "a" );
			  generateFileWithRecords( file, recordsPerFilePage * 2, recordSize );
			  using ( PagedFile pf = map( file, filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					assertTrue( cursor.Next() );
					using ( PageCursor linked = cursor.OpenLinkedCursor( 1 ) )
					{
						 assertTrue( linked.Next() );
					}
					cursor.ShouldRetry();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRetryOnReadParentOfClosedLinkedCursorMustNotThrow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRetryOnReadParentOfClosedLinkedCursorMustNotThrow()
		 {
			  configureStandardPageCache();
			  File file = file( "a" );
			  generateFileWithRecords( file, recordsPerFilePage * 2, recordSize );
			  using ( PagedFile pf = map( file, filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_READ_LOCK ) )
			  {
					assertTrue( cursor.Next() );
					using ( PageCursor linked = cursor.OpenLinkedCursor( 1 ) )
					{
						 assertTrue( linked.Next() );
					}
					cursor.ShouldRetry();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRetryOnReadParentOnDirtyPageOfClosedLinkedCursorMustNotThrow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRetryOnReadParentOnDirtyPageOfClosedLinkedCursorMustNotThrow()
		 {
			  configureStandardPageCache();
			  File file = file( "a" );
			  generateFileWithRecords( file, recordsPerFilePage * 2, recordSize );
			  using ( PagedFile pf = map( file, filePageSize ), PageCursor reader = pf.Io( 0, PF_SHARED_READ_LOCK ) )
			  {
					assertTrue( reader.Next() );
					using ( PageCursor writer = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( writer.Next() );
					}
					using ( PageCursor linked = reader.OpenLinkedCursor( 1 ) )
					{
						 assertTrue( linked.Next() );
					}
					assertTrue( reader.ShouldRetry() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pageCursorCloseShouldNotReturnAlreadyClosedLinkedCursorToPool() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void PageCursorCloseShouldNotReturnAlreadyClosedLinkedCursorToPool()
		 {
			  getPageCache( fs, maxPages, PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null );
			  File file = file( "a" );
			  generateFileWithRecords( file, recordsPerFilePage * 2, recordSize );
			  using ( PagedFile pf = map( file, filePageSize ) )
			  {
					PageCursor a = pf.Io( 0, PF_SHARED_WRITE_LOCK );
					PageCursor b = a.OpenLinkedCursor( 0 );
					b.Close();
					PageCursor c = a.OpenLinkedCursor( 0 ); // Will close b again, creating a loop in the CursorPool
					PageCursor d = pf.Io( 0, PF_SHARED_WRITE_LOCK ); // Same object as c because of loop in pool
					assertNotSame( c, d );
					c.Close();
					d.Close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pageCursorCloseShouldNotReturnSameObjectToCursorPoolTwice() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void PageCursorCloseShouldNotReturnSameObjectToCursorPoolTwice()
		 {
			  getPageCache( fs, maxPages, PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null );
			  File file = file( "a" );
			  generateFileWithRecords( file, recordsPerFilePage * 2, recordSize );
			  using ( PagedFile pf = map( file, filePageSize ) )
			  {
					PageCursor a = pf.Io( 0, PF_SHARED_WRITE_LOCK );
					a.Close();
					a.Close(); // Return same object to CursorPool again, creating a Loop
					PageCursor b = pf.Io( 0, PF_SHARED_WRITE_LOCK );
					PageCursor c = pf.Io( 0, PF_SHARED_WRITE_LOCK );
					assertNotSame( b, c );
					b.Close();
					c.Close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pageCursorCloseWithClosedLinkedCursorShouldNotReturnSameObjectToCursorPoolTwice() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void PageCursorCloseWithClosedLinkedCursorShouldNotReturnSameObjectToCursorPoolTwice()
		 {
			  File file = file( "a" );
			  generateFileWithRecords( file, recordsPerFilePage * 2, recordSize );
			  getPageCache( fs, maxPages, PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null );
			  using ( PagedFile pf = map( file, filePageSize ) )
			  {
					PageCursor a = pf.Io( 0, PF_SHARED_WRITE_LOCK );
					a.OpenLinkedCursor( 0 );
					a.OpenLinkedCursor( 0 ).close();
					a.Close();

					PageCursor x = pf.Io( 0, PF_SHARED_WRITE_LOCK );
					PageCursor y = pf.Io( 0, PF_SHARED_WRITE_LOCK );
					PageCursor z = pf.Io( 0, PF_SHARED_WRITE_LOCK );

					assertNotSame( x, y );
					assertNotSame( x, z );
					assertNotSame( y, z );
					x.Close();
					y.Close();
					z.Close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pageCursorCloseMustNotClosePreviouslyLinkedCursorThatGotReused() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void PageCursorCloseMustNotClosePreviouslyLinkedCursorThatGotReused()
		 {
			  File file = file( "a" );
			  generateFileWithRecords( file, recordsPerFilePage * 2, recordSize );
			  getPageCache( fs, maxPages, PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null );
			  using ( PagedFile pf = map( file, filePageSize ) )
			  {
					PageCursor a = pf.Io( 0, PF_SHARED_WRITE_LOCK );
					a.OpenLinkedCursor( 0 ).close();
					PageCursor x = pf.Io( 0, PF_SHARED_WRITE_LOCK );
					a.Close();
					assertTrue( x.Next( 1 ) );
					x.Close();
			  }
		 }

		 private interface PageCursorAction
		 {
			  void Apply( PageCursor cursor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getByteBeyondPageEndMustThrow()
		 internal virtual void getByteBeyondPageEndMustThrow()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertTimeout( ofMillis( SHORT_TIMEOUT_MILLIS ), () => verifyPageBounds(PageCursor::getByte) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putByteBeyondPageEndMustThrow()
		 internal virtual void PutByteBeyondPageEndMustThrow()
		 {
			  assertTimeout( ofMillis( SHORT_TIMEOUT_MILLIS ), () => verifyPageBounds(cursor => cursor.putByte((sbyte) 42)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getShortBeyondPageEndMustThrow()
		 internal virtual void getShortBeyondPageEndMustThrow()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertTimeout( ofMillis( SHORT_TIMEOUT_MILLIS ), () => verifyPageBounds(PageCursor::getShort) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putShortBeyondPageEndMustThrow()
		 internal virtual void PutShortBeyondPageEndMustThrow()
		 {
			  assertTimeout( ofMillis( SHORT_TIMEOUT_MILLIS ), () => verifyPageBounds(cursor => cursor.putShort((short) 42)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getIntBeyondPageEndMustThrow()
		 internal virtual void getIntBeyondPageEndMustThrow()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertTimeout( ofMillis( SHORT_TIMEOUT_MILLIS ), () => verifyPageBounds(PageCursor::getInt) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putIntBeyondPageEndMustThrow()
		 internal virtual void PutIntBeyondPageEndMustThrow()
		 {
			  assertTimeout( ofMillis( SHORT_TIMEOUT_MILLIS ), () => verifyPageBounds(cursor => cursor.putInt(42)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putLongBeyondPageEndMustThrow()
		 internal virtual void PutLongBeyondPageEndMustThrow()
		 {
			  assertTimeout( ofMillis( SHORT_TIMEOUT_MILLIS ), () => verifyPageBounds(cursor => cursor.putLong(42)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getLongBeyondPageEndMustThrow()
		 internal virtual void getLongBeyondPageEndMustThrow()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  assertTimeout( ofMillis( SHORT_TIMEOUT_MILLIS ), () => verifyPageBounds(PageCursor::getLong) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putBytesBeyondPageEndMustThrow()
		 internal virtual void PutBytesBeyondPageEndMustThrow()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				sbyte[] bytes = new sbyte[]{ 1, 2, 3 };
				VerifyPageBounds( cursor => cursor.putBytes( bytes ) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putBytesRepeatedByteBeyondPageEndMustThrow()
		 internal virtual void PutBytesRepeatedByteBeyondPageEndMustThrow()
		 {
			  assertTimeout( ofMillis( SHORT_TIMEOUT_MILLIS ), () => verifyPageBounds(cursor => cursor.putBytes(3, (sbyte) 1)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getBytesBeyondPageEndMustThrow()
		 internal virtual void getBytesBeyondPageEndMustThrow()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				sbyte[] bytes = new sbyte[3];
				VerifyPageBounds( cursor => cursor.getBytes( bytes ) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putBytesWithOffsetAndLengthBeyondPageEndMustThrow()
		 internal virtual void PutBytesWithOffsetAndLengthBeyondPageEndMustThrow()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				sbyte[] bytes = new sbyte[]{ 1, 2, 3 };
				VerifyPageBounds( cursor => cursor.putBytes( bytes, 1, 1 ) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getBytesWithOffsetAndLengthBeyondPageEndMustThrow()
		 internal virtual void getBytesWithOffsetAndLengthBeyondPageEndMustThrow()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				sbyte[] bytes = new sbyte[3];
				VerifyPageBounds( cursor => cursor.getBytes( bytes, 1, 1 ) );
			  });
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyPageBounds(PageCursorAction action) throws java.io.IOException
		 private void VerifyPageBounds( PageCursorAction action )
		 {
			  configureStandardPageCache();

			  generateFileWithRecords( file( "a" ), 1, recordSize );

			  using ( PagedFile pagedFile = map( file( "a" ), filePageSize ), PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					cursor.Next();
					assertThrows(typeof(System.IndexOutOfRangeException), () =>
					{
					 for ( int i = 0; i < 100000; i++ )
					 {
						  action.Apply( cursor );
						  if ( cursor.CheckAndClearBoundsFlag() )
						  {
								throw new System.IndexOutOfRangeException();
						  }
					 }
					});
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRetryMustClearBoundsFlagWhenReturningTrue()
		 internal virtual void ShouldRetryMustClearBoundsFlagWhenReturningTrue()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor reader = pf.Io( 0, PF_SHARED_READ_LOCK ) )
				{
					 PageCursor writer = pf.Io( 0, PF_SHARED_WRITE_LOCK );
					 assertTrue( writer.Next() );

					 assertTrue( reader.Next() );
					 reader.GetByte( -1 ); // out-of-bounds flag now raised
					 writer.Close(); // reader overlapped with writer, so must retry
					 assertTrue( reader.ShouldRetry() );

					 // shouldRetry returned 'true', so it must clear the out-of-bounds flag
					 assertFalse( reader.CheckAndClearBoundsFlag() );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRetryMustNotClearBoundsFlagWhenReturningFalse()
		 internal virtual void ShouldRetryMustNotClearBoundsFlagWhenReturningFalse()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor reader = pf.Io( 0, PF_SHARED_READ_LOCK ) )
				{
					 PageCursor writer = pf.Io( 0, PF_SHARED_WRITE_LOCK );
					 assertTrue( writer.Next() );
					 writer.Close(); // writer closed before reader comes to this page, so no need for retry

					 assertTrue( reader.Next() );
					 reader.GetByte( -1 ); // out-of-bounds flag now raised
					 assertFalse( reader.ShouldRetry() );

					 // shouldRetry returned 'true', so it must clear the out-of-bounds flag
					 assertTrue( reader.CheckAndClearBoundsFlag() );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nextThatReturnsTrueMustNotClearBoundsFlagOnReadCursor()
		 internal virtual void NextThatReturnsTrueMustNotClearBoundsFlagOnReadCursor()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor reader = pf.Io( 0, PF_SHARED_READ_LOCK ) )
				{
					 PageCursor writer = pf.Io( 0, PF_SHARED_WRITE_LOCK );
					 assertTrue( writer.Next() );

					 assertTrue( reader.Next() );
					 reader.GetByte( -1 ); // out-of-bounds flag now raised
					 writer.Next(); // make sure there's a next page for the reader to move to
					 writer.Close(); // reader overlapped with writer, so must retry
					 assertTrue( reader.Next() );

					 assertTrue( reader.CheckAndClearBoundsFlag() );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nextThatReturnsTrueMustNotClearBoundsFlagOnWriteCursor()
		 internal virtual void NextThatReturnsTrueMustNotClearBoundsFlagOnWriteCursor()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor writer = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
				{
					 assertTrue( writer.Next() );
					 writer.GetByte( -1 ); // out-of-bounds flag now raised
					 assertTrue( writer.Next() );

					 assertTrue( writer.CheckAndClearBoundsFlag() );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nextThatReturnsFalseMustNotClearBoundsFlagOnReadCursor()
		 internal virtual void NextThatReturnsFalseMustNotClearBoundsFlagOnReadCursor()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor reader = pf.Io( 0, PF_SHARED_READ_LOCK ) )
				{
					 PageCursor writer = pf.Io( 0, PF_SHARED_WRITE_LOCK );
					 assertTrue( writer.Next() );

					 assertTrue( reader.Next() );
					 reader.GetByte( -1 ); // out-of-bounds flag now raised
					 // don't call next of the writer, so there won't be a page for the reader to move onto
					 writer.Close(); // reader overlapped with writer, so must retry
					 assertFalse( reader.Next() );

					 assertTrue( reader.CheckAndClearBoundsFlag() );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nextThatReturnsFalseMustNotClearBoundsFlagOnWriteCursor()
		 internal virtual void NextThatReturnsFalseMustNotClearBoundsFlagOnWriteCursor()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();
				File file = file( "a" );
				generateFileWithRecords( file, recordsPerFilePage, recordSize );

				using ( PagedFile pf = map( file, filePageSize ), PageCursor writer = pf.Io( 0, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
				{
					 assertTrue( writer.Next() );
					 writer.GetByte( -1 ); // out-of-bounds flag now raised
					 assertFalse( writer.Next() );

					 assertTrue( writer.CheckAndClearBoundsFlag() );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nextWithPageIdThatReturnsTrueMustNotClearBoundsFlagOnReadCursor()
		 internal virtual void NextWithPageIdThatReturnsTrueMustNotClearBoundsFlagOnReadCursor()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor reader = pf.Io( 0, PF_SHARED_READ_LOCK ) )
				{
					 PageCursor writer = pf.Io( 0, PF_SHARED_WRITE_LOCK );
					 assertTrue( writer.Next() );

					 assertTrue( reader.Next() );
					 reader.GetByte( -1 ); // out-of-bounds flag now raised
					 writer.Next( 3 ); // make sure there's a next page for the reader to move to
					 writer.Close(); // reader overlapped with writer, so must retry
					 assertTrue( reader.Next( 3 ) );

					 assertTrue( reader.CheckAndClearBoundsFlag() );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nextWithPageIdMustNotClearBoundsFlagOnWriteCursor()
		 internal virtual void NextWithPageIdMustNotClearBoundsFlagOnWriteCursor()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor writer = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
				{
					 assertTrue( writer.Next() );
					 writer.GetByte( -1 ); // out-of-bounds flag now raised
					 assertTrue( writer.Next( 3 ) );

					 assertTrue( writer.CheckAndClearBoundsFlag() );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void settingOutOfBoundsCursorOffsetMustRaiseBoundsFlag()
		 internal virtual void SettingOutOfBoundsCursorOffsetMustRaiseBoundsFlag()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				generateFileWithRecords( file( "a" ), 1, recordSize );
				using ( PagedFile pagedFile = map( file( "a" ), filePageSize ), PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
				{
					 cursor.Offset = -1;
					 assertTrue( cursor.CheckAndClearBoundsFlag() );
					 assertFalse( cursor.CheckAndClearBoundsFlag() );

					 cursor.Offset = filePageSize + 1;
					 assertTrue( cursor.CheckAndClearBoundsFlag() );
					 assertFalse( cursor.CheckAndClearBoundsFlag() );

					 cursor.Offset = pageCachePageSize + 1;
					 assertTrue( cursor.CheckAndClearBoundsFlag() );
					 assertFalse( cursor.CheckAndClearBoundsFlag() );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void manuallyRaisedBoundsFlagMustBeObservable() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ManuallyRaisedBoundsFlagMustBeObservable()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pagedFile = map( file( "a" ), filePageSize ), PageCursor writer = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ), PageCursor reader = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
			  {
					assertTrue( writer.Next() );
					writer.RaiseOutOfBounds();
					assertTrue( writer.CheckAndClearBoundsFlag() );

					assertTrue( reader.Next() );
					reader.RaiseOutOfBounds();
					assertTrue( reader.CheckAndClearBoundsFlag() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pageFaultForWriteMustThrowIfOutOfStorageSpace()
		 internal virtual void PageFaultForWriteMustThrowIfOutOfStorageSpace()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				AtomicInteger writeCounter = new AtomicInteger();
				AtomicBoolean restrictWrites = new AtomicBoolean( true );
				FileSystemAbstraction fs = new DelegatingFileSystemAbstractionAnonymousInnerClass3( this, this.fs, writeCounter, restrictWrites );

				fs.create( file( "a" ) ).close();

				getPageCache( fs, maxPages, PageCacheTracer.NULL, PageCursorTracerSupplier.NULL );
				PagedFile pagedFile = map( file( "a" ), filePageSize );

				try
				{
					using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
					{
						 assertThrows(typeof(IOException), () =>
						 {
						  //noinspection StatementWithEmptyBody
						  while ( cursor.Next() )
						  {
								// Profound and interesting I/O.
						  }
						 });
					}
				}
				finally
				{
					 restrictWrites.set( false );
					 pagedFile.Close();
					 pageCache.close();
					 fs.close();
				}
			  });
		 }

		 private class DelegatingFileSystemAbstractionAnonymousInnerClass3 : DelegatingFileSystemAbstraction
		 {
			 private readonly PageCacheTest<T> _outerInstance;

			 private AtomicInteger _writeCounter;
			 private AtomicBoolean _restrictWrites;

			 public DelegatingFileSystemAbstractionAnonymousInnerClass3( PageCacheTest<T> outerInstance, UnknownType fs, AtomicInteger writeCounter, AtomicBoolean restrictWrites ) : base( fs )
			 {
				 this.outerInstance = outerInstance;
				 this._writeCounter = writeCounter;
				 this._restrictWrites = restrictWrites;
				 channels = new CopyOnWriteArrayList<>();
			 }

			 private IList<StoreChannel> channels;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.io.fs.StoreChannel open(java.io.File fileName, org.Neo4Net.io.fs.OpenMode openMode) throws java.io.IOException
			 public override StoreChannel open( File fileName, OpenMode openMode )
			 {
				  StoreChannel channel = new DelegatingStoreChannelAnonymousInnerClass4( this, base.open( fileName, openMode ) );
				  channels.add( channel );
				  return channel;
			 }

			 private class DelegatingStoreChannelAnonymousInnerClass4 : DelegatingStoreChannel
			 {
				 private readonly DelegatingFileSystemAbstractionAnonymousInnerClass3 _outerInstance;

				 public DelegatingStoreChannelAnonymousInnerClass4( DelegatingFileSystemAbstractionAnonymousInnerClass3 outerInstance, UnknownType open ) : base( open )
				 {
					 this.outerInstance = outerInstance;
				 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeAll(ByteBuffer src, long position) throws java.io.IOException
				 public override void writeAll( ByteBuffer src, long position )
				 {
					  if ( _outerInstance.restrictWrites.get() && _outerInstance.writeCounter.incrementAndGet() > 10 )
					  {
							throw new IOException( "No space left on device" );
					  }
					  base.writeAll( src, position );
				 }
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
			 public override void close()
			 {
				  IOUtils.closeAll( channels );
				  base.close();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pageFaultForReadMustThrowIfOutOfStorageSpace()
		 internal virtual void PageFaultForReadMustThrowIfOutOfStorageSpace()
		 {
			  try
			  {
					assertTimeout(ofMillis(SEMI_LONG_TIMEOUT_MILLIS), () =>
					{
					 AtomicInteger writeCounter = new AtomicInteger();
					 AtomicBoolean restrictWrites = new AtomicBoolean( true );
					 FileSystemAbstraction fs = new DelegatingFileSystemAbstractionAnonymousInnerClass4( this, this.fs, writeCounter, restrictWrites );

					 getPageCache( fs, maxPages, PageCacheTracer.NULL, PageCursorTracerSupplier.NULL );
					 generateFileWithRecords( file( "a" ), recordCount, recordSize );
					 PagedFile pagedFile = map( file( "a" ), filePageSize );

					 // Create 1 dirty page
					 using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
					 {
						  assertTrue( cursor.Next() );
					 }

					 // Read pages until the dirty page gets flushed
					 try
					 {
						 using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
						 {
							  //noinspection InfiniteLoopStatement
							  for ( ; ; )
							  {
									//noinspection StatementWithEmptyBody
									while ( cursor.Next() )
									{
										 // Profound and interesting I/O.
									}
									// Use rewind if we get to the end, because it is non-
									// deterministic which pages get evicted and when.
									cursor.Rewind();
							  }
						 }
					 }
					 finally
					 {
						  restrictWrites.set( false );
						  pagedFile.Close();
						  pageCache.close();
						  fs.close();
					 }
					});
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( IOException ) ) );
			  }
		 }

		 private class DelegatingFileSystemAbstractionAnonymousInnerClass4 : DelegatingFileSystemAbstraction
		 {
			 private readonly PageCacheTest<T> _outerInstance;

			 private AtomicInteger _writeCounter;
			 private AtomicBoolean _restrictWrites;

			 public DelegatingFileSystemAbstractionAnonymousInnerClass4( PageCacheTest<T> outerInstance, UnknownType fs, AtomicInteger writeCounter, AtomicBoolean restrictWrites ) : base( fs )
			 {
				 this.outerInstance = outerInstance;
				 this._writeCounter = writeCounter;
				 this._restrictWrites = restrictWrites;
				 channels = new CopyOnWriteArrayList<>();
			 }

			 private readonly IList<StoreChannel> channels;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.io.fs.StoreChannel open(java.io.File fileName, org.Neo4Net.io.fs.OpenMode openMode) throws java.io.IOException
			 public override StoreChannel open( File fileName, OpenMode openMode )
			 {
				  StoreChannel channel = new DelegatingStoreChannelAnonymousInnerClass5( this, base.open( fileName, openMode ) );
				  channels.add( channel );
				  return channel;
			 }

			 private class DelegatingStoreChannelAnonymousInnerClass5 : DelegatingStoreChannel
			 {
				 private readonly DelegatingFileSystemAbstractionAnonymousInnerClass4 _outerInstance;

				 public DelegatingStoreChannelAnonymousInnerClass5( DelegatingFileSystemAbstractionAnonymousInnerClass4 outerInstance, UnknownType open ) : base( open )
				 {
					 this.outerInstance = outerInstance;
				 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeAll(ByteBuffer src, long position) throws java.io.IOException
				 public override void writeAll( ByteBuffer src, long position )
				 {
					  if ( _outerInstance.restrictWrites.get() && _outerInstance.writeCounter.incrementAndGet() >= 1 )
					  {
							throw new IOException( "No space left on device" );
					  }
					  base.writeAll( src, position );
				 }
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
			 public override void close()
			 {
				  IOUtils.closeAll( channels );
				  base.close();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustRecoverViaFileCloseFromFullDriveWhenMoreStorageBecomesAvailable()
		 internal virtual void MustRecoverViaFileCloseFromFullDriveWhenMoreStorageBecomesAvailable()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				AtomicBoolean hasSpace = new AtomicBoolean();
				FileSystemAbstraction fs = new DelegatingFileSystemAbstractionAnonymousInnerClass5( this, this.fs, hasSpace );

				fs.create( file( "a" ) ).close();

				getPageCache( fs, maxPages, PageCacheTracer.NULL, PageCursorTracerSupplier.NULL );
				PagedFile pagedFile = map( file( "a" ), filePageSize );

				try
				{
					using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
					{
						 //noinspection InfiniteLoopStatement
						 for ( ; ; ) // Keep writing until we get an exception! (when the cache starts evicting stuff)
						 {
							  assertTrue( cursor.Next() );
							  writeRecords( cursor );
						 }
					}
				}
				catch ( IOException )
				{
					 // We're out of space! Salty tears...
				}

				// Fix the situation:
				hasSpace.set( true );

				// Closing the last reference of a paged file implies a flush, and it mustn't throw:
				pagedFile.Close();

				using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_READ_LOCK ) )
				{
					 assertTrue( cursor.Next() ); // this should not throw
				}
			  });
		 }

		 private class DelegatingFileSystemAbstractionAnonymousInnerClass5 : DelegatingFileSystemAbstraction
		 {
			 private readonly PageCacheTest<T> _outerInstance;

			 private AtomicBoolean _hasSpace;

			 public DelegatingFileSystemAbstractionAnonymousInnerClass5( PageCacheTest<T> outerInstance, UnknownType fs, AtomicBoolean hasSpace ) : base( fs )
			 {
				 this.outerInstance = outerInstance;
				 this._hasSpace = hasSpace;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.io.fs.StoreChannel open(java.io.File fileName, org.Neo4Net.io.fs.OpenMode openMode) throws java.io.IOException
			 public override StoreChannel open( File fileName, OpenMode openMode )
			 {
				  return new DelegatingStoreChannelAnonymousInnerClass6( this, base.open( fileName, openMode ) );
			 }

			 private class DelegatingStoreChannelAnonymousInnerClass6 : DelegatingStoreChannel
			 {
				 private readonly DelegatingFileSystemAbstractionAnonymousInnerClass5 _outerInstance;

				 public DelegatingStoreChannelAnonymousInnerClass6( DelegatingFileSystemAbstractionAnonymousInnerClass5 outerInstance, UnknownType open ) : base( open )
				 {
					 this.outerInstance = outerInstance;
				 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeAll(ByteBuffer src, long position) throws java.io.IOException
				 public override void writeAll( ByteBuffer src, long position )
				 {
					  if ( !_outerInstance.hasSpace.get() )
					  {
							throw new IOException( "No space left on device" );
					  }
					  base.writeAll( src, position );
				 }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustRecoverViaFileFlushFromFullDriveWhenMoreStorageBecomesAvailable() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustRecoverViaFileFlushFromFullDriveWhenMoreStorageBecomesAvailable()
		 {

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean hasSpace = new java.util.concurrent.atomic.AtomicBoolean();
			  AtomicBoolean hasSpace = new AtomicBoolean();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean hasThrown = new java.util.concurrent.atomic.AtomicBoolean();
			  AtomicBoolean hasThrown = new AtomicBoolean();
			  FileSystemAbstraction fs = new DelegatingFileSystemAbstractionAnonymousInnerClass2( this, this.fs, hasSpace, hasThrown );

			  fs.Create( file( "a" ) ).close();

			  getPageCache( fs, maxPages, PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null );
			  PagedFile pagedFile = map( file( "a" ), filePageSize );

			  try
			  {
					  using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
					  {
						//noinspection InfiniteLoopStatement
						while ( !hasThrown.get() ) // Keep writing until we get an exception! (when the cache starts evicting stuff)
						{
							 assertTrue( cursor.Next() );
							 writeRecords( cursor );
						}
					  }
			  }
			  catch ( IOException )
			  {
					// We're out of space! Salty tears...
			  }

			  // Fix the situation:
			  hasSpace.set( true );

			  // Flushing the paged file implies the eviction exception gets cleared, and mustn't itself throw:
			  pagedFile.FlushAndForce();

			  using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
			  {
					assertTrue( cursor.Next() ); // this should not throw
			  }
			  pagedFile.Close();
		 }

		 private class DelegatingFileSystemAbstractionAnonymousInnerClass2 : DelegatingFileSystemAbstraction
		 {
			 private readonly PageCacheTest<T> _outerInstance;

			 private AtomicBoolean _hasSpace;
			 private AtomicBoolean _hasThrown;

			 public DelegatingFileSystemAbstractionAnonymousInnerClass2( PageCacheTest<T> outerInstance, UnknownType fs, AtomicBoolean hasSpace, AtomicBoolean hasThrown ) : base( fs )
			 {
				 this._outerInstance = outerInstance;
				 this._hasSpace = hasSpace;
				 this._hasThrown = hasThrown;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.io.fs.StoreChannel open(java.io.File fileName, org.Neo4Net.io.fs.OpenMode openMode) throws java.io.IOException
			 public override StoreChannel open( File fileName, OpenMode openMode )
			 {
				  return new DelegatingStoreChannelAnonymousInnerClass7( this, base.open( fileName, openMode ) );
			 }

			 private class DelegatingStoreChannelAnonymousInnerClass7 : DelegatingStoreChannel
			 {
				 private readonly DelegatingFileSystemAbstractionAnonymousInnerClass2 _outerInstance;

				 public DelegatingStoreChannelAnonymousInnerClass7( DelegatingFileSystemAbstractionAnonymousInnerClass2 outerInstance, UnknownType open ) : base( open )
				 {
					 this.outerInstance = outerInstance;
				 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeAll(ByteBuffer src, long position) throws java.io.IOException
				 public override void writeAll( ByteBuffer src, long position )
				 {
					  if ( !_outerInstance.hasSpace.get() )
					  {
							_outerInstance.hasThrown.set( true );
							throw new IOException( "No space left on device" );
					  }
					  base.writeAll( src, position );
				 }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void dataFromDifferentFilesMustNotBleedIntoEachOther()
		 internal virtual void DataFromDifferentFilesMustNotBleedIntoEachOther()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				// The idea with this test is, that the pages for fileA are larger than
				// the pages for fileB, so we can put A-data beyond the end of the B
				// file pages.
				// Furthermore, our writes to the B-pages do not overwrite the entire page.
				// In those cases, the bytes not written to must be zeros.

				configureStandardPageCache();
				File fileB = existingFile( "b" );
				int filePageSizeA = pageCachePageSize - 2;
				int filePageSizeB = pageCachePageSize - 6;
				int pagesToWriteA = 100;
				int pagesToWriteB = 3;

				PagedFile pagedFileA = map( existingFile( "a" ), filePageSizeA );

				using ( PageCursor cursor = pagedFileA.Io( 0, PF_SHARED_WRITE_LOCK ) )
				{
					 for ( int i = 0; i < pagesToWriteA; i++ )
					 {
						  assertTrue( cursor.Next() );
						  for ( int j = 0; j < filePageSizeA; j++ )
						  {
								cursor.PutByte( ( sbyte ) 42 );
						  }
					 }
				}

				PagedFile pagedFileB = Map( fileB, filePageSizeB );

				using ( PageCursor cursor = pagedFileB.Io( 0, PF_SHARED_WRITE_LOCK ) )
				{
					 for ( int i = 0; i < pagesToWriteB; i++ )
					 {
						  assertTrue( cursor.Next() );
						  cursor.PutByte( ( sbyte ) 63 );
					 }
				}

				pagedFileA.Close();
				pagedFileB.Close();

				Stream inputStream = fs.openAsInputStream( fileB );
				assertThat( "first page first byte", inputStream.read(), @is(63) );
				for ( int i = 0; i < filePageSizeB - 1; i++ )
				{
					 assertThat( "page 0 byte pos " + i, inputStream.read(), @is(0) );
				}
				assertThat( "second page first byte", inputStream.read(), @is(63) );
				for ( int i = 0; i < filePageSizeB - 1; i++ )
				{
					 assertThat( "page 1 byte pos " + i, inputStream.read(), @is(0) );
				}
				assertThat( "third page first byte", inputStream.read(), @is(63) );
				for ( int i = 0; i < filePageSizeB - 1; i++ )
				{
					 assertThat( "page 2 byte pos " + i, inputStream.read(), @is(0) );
				}
				assertThat( "expect EOF", inputStream.read(), @is(-1) );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void freshlyCreatedPagesMustContainAllZeros()
		 internal virtual void FreshlyCreatedPagesMustContainAllZeros()
		 {
			  assertTimeout(ofMillis(SEMI_LONG_TIMEOUT_MILLIS), () =>
			  {
				ThreadLocalRandom rng = ThreadLocalRandom.current();

				configureStandardPageCache();

				using ( PagedFile pagedFile = map( existingFile( "a" ), filePageSize ), PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
				{
					 for ( int i = 0; i < 100; i++ )
					 {
						  assertTrue( cursor.Next() );
						  for ( int j = 0; j < filePageSize; j++ )
						  {
								cursor.PutByte( ( sbyte ) rng.Next() );
						  }
					 }
				}
				pageCache.close();
				pageCache = null;

				configureStandardPageCache();

				using ( PagedFile pagedFile = map( existingFile( "b" ), filePageSize ), PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
				{
					 for ( int i = 0; i < 100; i++ )
					 {
						  assertTrue( cursor.Next() );
						  for ( int j = 0; j < filePageSize; j++ )
						  {
								assertThat( cursor.Byte, @is( ( sbyte ) 0 ) );
						  }
					 }
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void optimisticReadLockMustFaultOnRetryIfPageHasBeenEvicted()
		 internal virtual void OptimisticReadLockMustFaultOnRetryIfPageHasBeenEvicted()
		 {
			  assertTimeout(ofMillis(SEMI_LONG_TIMEOUT_MILLIS), () =>
			  {
				const sbyte a = ( sbyte )'a';
				const sbyte b = ( sbyte )'b';
				File fileA = existingFile( "a" );
				File fileB = existingFile( "b" );

				configureStandardPageCache();

				PagedFile pagedFileA = map( fileA, filePageSize );
				PagedFile pagedFileB = map( fileB, filePageSize );

				// Fill fileA with some predicable data
				using ( PageCursor cursor = pagedFileA.Io( 0, PF_SHARED_WRITE_LOCK ) )
				{
					 for ( int i = 0; i < maxPages; i++ )
					 {
						  assertTrue( cursor.Next() );
						  for ( int j = 0; j < filePageSize; j++ )
						  {
								cursor.PutByte( a );
						  }
					 }
				}

				ThreadStart fillPagedFileB = () =>
				{
					 try
					 {
						 using ( PageCursor cursor = pagedFileB.Io( 0, PF_SHARED_WRITE_LOCK ) )
						 {
							  for ( int i = 0; i < maxPages * 30; i++ )
							  {
									assertTrue( cursor.Next() );
									for ( int j = 0; j < filePageSize; j++ )
									{
										 cursor.PutByte( b );
									}
							  }
						 }
					 }
					 catch ( IOException e )
					 {
						  Console.WriteLine( e.ToString() );
						  Console.Write( e.StackTrace );
					 }
				};

				using ( PageCursor cursor = pagedFileA.Io( 0, PF_SHARED_READ_LOCK ) )
				{
					 // First, make sure page 0 is in the cache:
					 assertTrue( cursor.Next( 0 ) );
					 // If we took a page fault, we'd have a pessimistic lock on page 0.
					 // Move to the next page to release that lock:
					 assertTrue( cursor.Next() );
					 // Now go back to page 0. It's still in the cache, so we should get
					 // an optimistic lock, if that's available:
					 assertTrue( cursor.Next( 0 ) );

					 // Verify the page is all 'a's:
					 for ( int i = 0; i < filePageSize; i++ )
					 {
						  assertThat( cursor.Byte, @is( a ) );
					 }

					 // Now fill file B with 'b's... this will cause our current page to be evicted
					 fork( fillPagedFileB ).join();
					 // So if we had an optimistic lock, we should be asked to retry:
					 if ( cursor.ShouldRetry() )
					 {
						  // When we do reads after the shouldRetry() call, we should fault our page back
						  // and get consistent reads (assuming we don't race any further with eviction)
						  int expected = a * filePageSize;
						  int actual;
						  do
						  {
								actual = 0;
								for ( int i = 0; i < filePageSize; i++ )
								{
									 actual += cursor.Byte;
								}
						  } while ( cursor.ShouldRetry() );
						  assertThat( actual, @is( expected ) );
					 }
				}

				pagedFileA.Close();
				pagedFileB.Close();
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pagesMustReturnToFreelistIfSwapInThrows()
		 internal virtual void PagesMustReturnToFreelistIfSwapInThrows()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();

				generateFileWithRecords( file( "a" ), recordCount, recordSize );
				PagedFile pagedFile = map( file( "a" ), filePageSize );

				int iterations = maxPages * 2;
				AccessPagesWhileInterrupted( pagedFile, PF_SHARED_READ_LOCK, iterations );
				AccessPagesWhileInterrupted( pagedFile, PF_SHARED_WRITE_LOCK, iterations );

				// Verify that after all those troubles, page faulting starts working again
				// as soon as our thread is no longer interrupted and the PageSwapper no
				// longer throws.
				Thread.interrupted(); // make sure to clear our interruption status

				using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
				{
					 assertTrue( cursor.Next() );
					 verifyRecordsMatchExpected( cursor );
				}
				pagedFile.Close();
			  });
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void accessPagesWhileInterrupted(PagedFile pagedFile, int pf_flags, int iterations) throws java.io.IOException
		 private void AccessPagesWhileInterrupted( PagedFile pagedFile, int pfFlags, int iterations )
		 {
			  using ( PageCursor cursor = pagedFile.Io( 0, pfFlags ) )
			  {
					for ( int i = 0; i < iterations; i++ )
					{
						 Thread.CurrentThread.Interrupt();
						 try
						 {
							  cursor.Next( 0 );
						 }
						 catch ( IOException )
						 {
							  // We don't care about the exception per se.
							  // We just want lots of failed page faults.
						 }
					}
			  }
		 }

		 // NOTE: This test is CPU architecture dependent, but it should fail on no
		 // architecture that we support.
		 // This test has no timeout because one may want to run it on a CPU
		 // emulator, where it's not unthinkable for it to take minutes.
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustSupportUnalignedWordAccesses() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustSupportUnalignedWordAccesses()
		 {
			  getPageCache( fs, 5, PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null );
			  int pageSize = pageCache.pageSize();

			  ThreadLocalRandom rng = ThreadLocalRandom.current();

			  using ( PagedFile pagedFile = map( file( "a" ), pageSize ), PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					assertTrue( cursor.Next() );

					long x = rng.nextLong();
					int limit = pageSize - Long.BYTES;
					for ( int i = 0; i < limit; i++ )
					{
						 x += i;
						 cursor.Offset = i;
						 cursor.PutLong( x );
						 cursor.Offset = i;
						 long y = cursor.Long;

						 assertFalse( cursor.CheckAndClearBoundsFlag(), "Should not have had a page out-of-bounds access!" );
						 if ( x != y )
						 {
							  string reason = "Failed to read back the value that was written at " + "offset " + toHexString( i );
							  assertThat( reason, toHexString( y ), @is( toHexString( x ) ) );
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RepeatedTest(50) void mustEvictPagesFromUnmappedFiles()
		 internal virtual void MustEvictPagesFromUnmappedFiles()
		 {
			  assertTimeout(ofMillis(SEMI_LONG_TIMEOUT_MILLIS), () =>
			  {
				// GIVEN mapping then unmapping
				configureStandardPageCache();
				using ( PagedFile pagedFile = map( file( "a" ), filePageSize ), PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
				{
					 assertTrue( cursor.Next() );
				}

				// WHEN using all pages, so that eviction of some pages will happen
				using ( PagedFile pagedFile = map( file( "a" ), filePageSize ), PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
				{
					 for ( int i = 0; i < maxPages + 5; i++ )
					 {
						  // THEN eviction happening here should not result in any exception
						  assertTrue( cursor.Next() );
					 }
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustReadZerosFromBeyondEndOfFile()
		 internal virtual void MustReadZerosFromBeyondEndOfFile()
		 {
			  assertTimeout(ofMillis(SEMI_LONG_TIMEOUT_MILLIS), () =>
			  {
				StandardRecordFormat recordFormat = new StandardRecordFormat();
				File[] files = new File[] { file( "1" ), file( "2" ), file( "3" ), file( "4" ), file( "5" ), file( "6" ), file( "7" ), file( "8" ), file( "9" ), file( "0" ), file( "A" ), file( "B" ) };
				for ( int fileId = 0; fileId < Files.Length; fileId++ )
				{
					 File file = files[fileId];
					 StoreChannel channel = fs.open( file, OpenMode.READ_WRITE );
					 for ( int recordId = 0; recordId < fileId + 1; recordId++ )
					 {
						  Record record = recordFormat.createRecord( file, recordId );
						  recordFormat.writeRecord( record, channel );
					 }
					 channel.close();
				}

				getPageCache( fs, 2, PageCacheTracer.NULL, PageCursorTracerSupplier.NULL );
				int pageSize = pageCache.pageSize();

				int fileId = Files.Length;
				while ( fileId-- > 0 )
				{
					 File file = files[fileId];
					 using ( PagedFile pf = Map( file, pageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_READ_LOCK ) )
					 {
						  int pageCount = 0;
						  while ( cursor.Next() )
						  {
								pageCount++;
								recordFormat.assertRecordsWrittenCorrectly( cursor );
						  }
						  assertThat( "pages in file " + file, pageCount, greaterThan( 0 ) );
					 }
				}
			  });
		 }

		 private int NextPowerOf2( int i )
		 {
			  return 1 << ( 32 - Integer.numberOfLeadingZeros( i ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private PageSwapperFactory factoryCountingSyncDevice(final java.util.concurrent.atomic.AtomicInteger syncDeviceCounter, final java.util.Queue<int> expectedCountsInForce)
		 private PageSwapperFactory FactoryCountingSyncDevice( AtomicInteger syncDeviceCounter, LinkedList<int> expectedCountsInForce )
		 {
			  SingleFilePageSwapperFactory factory = new SingleFilePageSwapperFactoryAnonymousInnerClass3( this, syncDeviceCounter, expectedCountsInForce );
			  factory.Open( fs, Configuration.EMPTY );
			  return factory;
		 }

		 private class SingleFilePageSwapperFactoryAnonymousInnerClass3 : SingleFilePageSwapperFactory
		 {
			 private readonly PageCacheTest<T> _outerInstance;

			 private AtomicInteger _syncDeviceCounter;
			 private LinkedList<int> _expectedCountsInForce;

			 public SingleFilePageSwapperFactoryAnonymousInnerClass3( PageCacheTest<T> outerInstance, AtomicInteger syncDeviceCounter, LinkedList<int> expectedCountsInForce )
			 {
				 this.outerInstance = outerInstance;
				 this._syncDeviceCounter = syncDeviceCounter;
				 this._expectedCountsInForce = expectedCountsInForce;
			 }

			 public override void syncDevice()
			 {
				  base.syncDevice();
				  _syncDeviceCounter.AndIncrement;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PageSwapper createPageSwapper(java.io.File file, int filePageSize, PageEvictionCallback onEviction, boolean createIfNotExist, boolean noChannelStriping) throws java.io.IOException
			 public override PageSwapper createPageSwapper( File file, int filePageSize, PageEvictionCallback onEviction, bool createIfNotExist, bool noChannelStriping )
			 {
				  PageSwapper @delegate = base.createPageSwapper( file, filePageSize, onEviction, createIfNotExist, noChannelStriping );
				  return new DelegatingPageSwapperAnonymousInnerClass( this, @delegate );
			 }

			 private class DelegatingPageSwapperAnonymousInnerClass : DelegatingPageSwapper
			 {
				 private readonly SingleFilePageSwapperFactoryAnonymousInnerClass3 _outerInstance;

				 public DelegatingPageSwapperAnonymousInnerClass( SingleFilePageSwapperFactoryAnonymousInnerClass3 outerInstance, Neo4Net.Io.pagecache.PageSwapper @delegate ) : base( @delegate )
				 {
					 this.outerInstance = outerInstance;
				 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void force() throws java.io.IOException
				 public override void force()
				 {
					  base.force();
					  assertThat( _outerInstance.syncDeviceCounter.get(), @is(_outerInstance.expectedCountsInForce.RemoveFirst()) );
				 }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs private static <E> java.util.Queue<E> queue(E... items)
		 private static LinkedList<E> Queue<E>( params E[] items )
		 {
			  LinkedList<E> queue = new ConcurrentLinkedQueue<E>();
			  foreach ( E item in items )
			  {
					queue.AddLast( item );
			  }
			  return queue;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustSyncDeviceWhenFlushAndForcingPagedFile()
		 internal virtual void MustSyncDeviceWhenFlushAndForcingPagedFile()
		 {
			  assertTimeout(ofMillis(SEMI_LONG_TIMEOUT_MILLIS), () =>
			  {
				AtomicInteger syncDeviceCounter = new AtomicInteger();
				AtomicInteger expectedCountInForce = new AtomicInteger();
				LinkedList<int> expectedCountsInForce = Queue( 0, 1, 2 ); // closing+forcing the files one by one, we get 2 more `syncDevice`
				PageSwapperFactory factory = FactoryCountingSyncDevice( syncDeviceCounter, expectedCountsInForce );
				using ( PageCache cache = createPageCache( factory, maxPages, PageCacheTracer.NULL, PageCursorTracerSupplier.NULL, EmptyVersionContextSupplier.EMPTY ), PagedFile p1 = cache.Map( existingFile( "a" ), filePageSize ), PagedFile p2 = cache.Map( existingFile( "b" ), filePageSize ) )
				{
					 using ( PageCursor cursor = p1.Io( 0, PF_SHARED_WRITE_LOCK ) )
					 {
						  assertTrue( cursor.Next() );
					 }
					 using ( PageCursor cursor = p2.Io( 0, PF_SHARED_WRITE_LOCK ) )
					 {
						  assertTrue( cursor.Next() );
					 }

					 p1.FlushAndForce();
					 expectedCountInForce.set( 1 );
					 assertThat( syncDeviceCounter.get(), @is(1) );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustSyncDeviceWhenFlushAndForcingPageCache()
		 internal virtual void MustSyncDeviceWhenFlushAndForcingPageCache()
		 {
			  assertTimeout(ofMillis(SEMI_LONG_TIMEOUT_MILLIS), () =>
			  {
				AtomicInteger syncDeviceCounter = new AtomicInteger();
				AtomicInteger expectedCountInForce = new AtomicInteger();
				LinkedList<int> expectedCountsInForce = Queue( 0, 0, 1, 2 ); // after test, files are closed+forced one by one
				PageSwapperFactory factory = FactoryCountingSyncDevice( syncDeviceCounter, expectedCountsInForce );
				using ( PageCache cache = createPageCache( factory, maxPages, PageCacheTracer.NULL, PageCursorTracerSupplier.NULL, EmptyVersionContextSupplier.EMPTY ), PagedFile p1 = cache.Map( existingFile( "a" ), filePageSize ), PagedFile p2 = cache.Map( existingFile( "b" ), filePageSize ) )
				{
					 using ( PageCursor cursor = p1.Io( 0, PF_SHARED_WRITE_LOCK ) )
					 {
						  assertTrue( cursor.Next() );
					 }
					 using ( PageCursor cursor = p2.Io( 0, PF_SHARED_WRITE_LOCK ) )
					 {
						  assertTrue( cursor.Next() );
					 }

					 cache.FlushAndForce();
					 expectedCountInForce.set( 1 );
					 assertThat( syncDeviceCounter.get(), @is(1) );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustThrowWhenMappingNonExistingFile()
		 internal virtual void MustThrowWhenMappingNonExistingFile()
		 {
			  assertThrows(typeof(NoSuchFileException), () =>
			  {
				configureStandardPageCache();
				map( file( "does not exist" ), filePageSize );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustCreateNonExistingFileWithCreateOption()
		 internal virtual void MustCreateNonExistingFileWithCreateOption()
		 {
			  assertTimeout(ofMillis(SEMI_LONG_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();
				using ( PagedFile pf = map( file( "does not exist" ), filePageSize, StandardOpenOption.CREATE ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
				{
					 assertTrue( cursor.Next() );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustIgnoreCreateOptionIfFileAlreadyExists()
		 internal virtual void MustIgnoreCreateOptionIfFileAlreadyExists()
		 {
			  assertTimeout(ofMillis(SEMI_LONG_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();
				using ( PagedFile pf = map( file( "a" ), filePageSize, StandardOpenOption.CREATE ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
				{
					 assertTrue( cursor.Next() );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustIgnoreCertainOpenOptions()
		 internal virtual void MustIgnoreCertainOpenOptions()
		 {
			  assertTimeout(ofMillis(SEMI_LONG_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();
				using ( PagedFile pf = map( file( "a" ), filePageSize, StandardOpenOption.READ, StandardOpenOption.WRITE, StandardOpenOption.APPEND, StandardOpenOption.SPARSE ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
				{
					 assertTrue( cursor.Next() );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustThrowOnUnsupportedOpenOptions()
		 internal virtual void MustThrowOnUnsupportedOpenOptions()
		 {
			  assertTimeout(ofMillis(SEMI_LONG_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();
				VerifyMappingWithOpenOptionThrows( StandardOpenOption.CREATE_NEW );
				VerifyMappingWithOpenOptionThrows( StandardOpenOption.SYNC );
				VerifyMappingWithOpenOptionThrows( StandardOpenOption.DSYNC );
				VerifyMappingWithOpenOptionThrows( new OpenOptionAnonymousInnerClass( this ) );
			  });
		 }

		 private class OpenOptionAnonymousInnerClass : OpenOption
		 {
			 private readonly PageCacheTest<T> _outerInstance;

			 public OpenOptionAnonymousInnerClass( PageCacheTest<T> outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override string ToString()
			 {
				  return "NonStandardOpenOption";
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyMappingWithOpenOptionThrows(java.nio.file.OpenOption option) throws java.io.IOException
		 private void VerifyMappingWithOpenOptionThrows( OpenOption option )
		 {
			  try
			  {
					map( file( "a" ), filePageSize, option ).Close();
					fail( "Expected map() to throw when given the OpenOption " + option );
			  }
			  catch ( Exception e ) when ( e is System.ArgumentException || e is System.NotSupportedException )
			  {
					// good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mappingFileWithTruncateOptionMustTruncateFile()
		 internal virtual void MappingFileWithTruncateOptionMustTruncateFile()
		 {
			  assertTimeout(ofMillis(SEMI_LONG_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();
				using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor cursor = pf.Io( 10, PF_SHARED_WRITE_LOCK ) )
				{
					 assertThat( pf.LastPageId, lessThan( 0L ) );
					 assertTrue( cursor.Next() );
					 cursor.PutInt( unchecked( ( int )0xcafebabe ) );
				}
				using ( PagedFile pf = map( file( "a" ), filePageSize, StandardOpenOption.TRUNCATE_EXISTING ), PageCursor cursor = pf.Io( 0, PF_SHARED_READ_LOCK ) )
				{
					 assertThat( pf.LastPageId, lessThan( 0L ) );
					 assertFalse( cursor.Next() );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") @Test void mappingAlreadyMappedFileWithTruncateOptionMustThrow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MappingAlreadyMappedFileWithTruncateOptionMustThrow()
		 {
			  configureStandardPageCache();
			  using ( PagedFile first = map( file( "a" ), filePageSize ) )
			  {
					assertThrows(typeof(System.NotSupportedException), () =>
					{
					 using ( PagedFile second = map( file( "a" ), filePageSize, StandardOpenOption.TRUNCATE_EXISTING ) )
					 {
						  // empty
					 }
					});
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustThrowIfFileIsClosedMoreThanItIsMapped() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustThrowIfFileIsClosedMoreThanItIsMapped()
		 {
			  configureStandardPageCache();
			  PagedFile pf = map( file( "a" ), filePageSize );
			  pf.Close();
			  assertThrows( typeof( System.InvalidOperationException ), pf.close );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void fileMappedWithDeleteOnCloseMustNotExistAfterUnmap() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void FileMappedWithDeleteOnCloseMustNotExistAfterUnmap()
		 {
			  configureStandardPageCache();
			  map( file( "a" ), filePageSize, DELETE_ON_CLOSE ).Close();
			  assertThrows( typeof( NoSuchFileException ), () => map(file("a"), filePageSize) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void fileMappedWithDeleteOnCloseMustNotExistAfterLastUnmap() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void FileMappedWithDeleteOnCloseMustNotExistAfterLastUnmap()
		 {
			  configureStandardPageCache();
			  File file = file( "a" );
			  using ( PagedFile ignore = map( file, filePageSize ) )
			  {
					map( file, filePageSize, DELETE_ON_CLOSE ).Close();
			  }
			  assertThrows( typeof( NoSuchFileException ), () => map(file, filePageSize) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void fileMappedWithDeleteOnCloseShouldNotFlushDirtyPagesOnClose() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void FileMappedWithDeleteOnCloseShouldNotFlushDirtyPagesOnClose()
		 {
			  AtomicInteger flushCounter = new AtomicInteger();
			  PageSwapperFactory swapperFactory = FlushCountingPageSwapperFactory( flushCounter );
			  swapperFactory.Open( fs, Configuration.EMPTY );
			  File file = file( "a" );
			  using ( PageCache cache = createPageCache( swapperFactory, maxPages, PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, EmptyVersionContextSupplier.EMPTY ), PagedFile pf = cache.Map( file, filePageSize, DELETE_ON_CLOSE ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					writeRecords( cursor );
					assertTrue( cursor.Next() );
			  }
			  assertThat( flushCounter.get(), lessThan(recordCount / recordsPerFilePage) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFlushAllDirtyPagesWhenClosingPagedFileThatIsNotMappedWithDeleteOnClose() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFlushAllDirtyPagesWhenClosingPagedFileThatIsNotMappedWithDeleteOnClose()
		 {
			  AtomicInteger flushCounter = new AtomicInteger();
			  PageSwapperFactory swapperFactory = FlushCountingPageSwapperFactory( flushCounter );
			  swapperFactory.Open( fs, Configuration.EMPTY );
			  File file = file( "a" );
			  using ( PageCache cache = createPageCache( swapperFactory, maxPages, PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, EmptyVersionContextSupplier.EMPTY ), PagedFile pf = cache.Map( file, filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					writeRecords( cursor );
					assertTrue( cursor.Next() );
			  }
			  assertThat( flushCounter.get(), @is(1) );
		 }

		 private SingleFilePageSwapperFactory FlushCountingPageSwapperFactory( AtomicInteger flushCounter )
		 {
			  return new SingleFilePageSwapperFactoryAnonymousInnerClass4( this, flushCounter );
		 }

		 private class SingleFilePageSwapperFactoryAnonymousInnerClass4 : SingleFilePageSwapperFactory
		 {
			 private readonly PageCacheTest<T> _outerInstance;

			 private AtomicInteger _flushCounter;

			 public SingleFilePageSwapperFactoryAnonymousInnerClass4( PageCacheTest<T> outerInstance, AtomicInteger flushCounter )
			 {
				 this.outerInstance = outerInstance;
				 this._flushCounter = flushCounter;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PageSwapper createPageSwapper(java.io.File file, int filePageSize, PageEvictionCallback onEviction, boolean createIfNotExist, boolean noChannelStriping) throws java.io.IOException
			 public override PageSwapper createPageSwapper( File file, int filePageSize, PageEvictionCallback onEviction, bool createIfNotExist, bool noChannelStriping )
			 {
				  PageSwapper swapper = base.createPageSwapper( file, filePageSize, onEviction, createIfNotExist, noChannelStriping );
				  return new DelegatingPageSwapperAnonymousInnerClass2( this, swapper );
			 }

			 private class DelegatingPageSwapperAnonymousInnerClass2 : DelegatingPageSwapper
			 {
				 private readonly SingleFilePageSwapperFactoryAnonymousInnerClass4 _outerInstance;

				 public DelegatingPageSwapperAnonymousInnerClass2( SingleFilePageSwapperFactoryAnonymousInnerClass4 outerInstance, Neo4Net.Io.pagecache.PageSwapper swapper ) : base( swapper )
				 {
					 this.outerInstance = outerInstance;
				 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long write(long filePageId, long bufferAddress) throws java.io.IOException
				 public override long write( long filePageId, long bufferAddress )
				 {
					  _outerInstance.flushCounter.AndIncrement;
					  return base.write( filePageId, bufferAddress );
				 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long write(long startFilePageId, long[] bufferAddresses, int arrayOffset, int length) throws java.io.IOException
				 public override long write( long startFilePageId, long[] bufferAddresses, int arrayOffset, int length )
				 {
					  _outerInstance.flushCounter.getAndAdd( length );
					  return base.write( startFilePageId, bufferAddresses, arrayOffset, length );
				 }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void fileMappedWithDeleteOnCloseMustNotLeakDirtyPages()
		 internal virtual void FileMappedWithDeleteOnCloseMustNotLeakDirtyPages()
		 {
			  assertTimeout(ofMillis(SEMI_LONG_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();
				File file = file( "a" );
				int iterations = 50;
				for ( int i = 0; i < iterations; i++ )
				{
					 ensureExists( file );
					 using ( PagedFile pf = map( file, filePageSize, DELETE_ON_CLOSE ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
					 {
						  writeRecords( cursor );
						  assertTrue( cursor.Next() );
					 }
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustNotThrowWhenMappingFileWithDifferentFilePageSizeAndAnyPageSizeIsSpecified() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustNotThrowWhenMappingFileWithDifferentFilePageSizeAndAnyPageSizeIsSpecified()
		 {
			  configureStandardPageCache();
			  using ( PagedFile ignore = map( file( "a" ), filePageSize ) )
			  {
					map( file( "a" ), filePageSize + 1, PageCacheOpenOptions.AnyPageSize ).Close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustCopyIntoSameSizedWritePageCursor() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustCopyIntoSameSizedWritePageCursor()
		 {
			  configureStandardPageCache();
			  int bytes = 200;

			  // Put some data into the file
			  using ( PagedFile pf = map( file( "a" ), 32 ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					for ( int i = 0; i < bytes; i++ )
					{
						 if ( ( i & 31 ) == 0 )
						 {
							  assertTrue( cursor.Next() );
						 }
						 cursor.PutByte( ( sbyte ) i );
					}
			  }

			  // Then copy all the pages into another file, with a larger file page size
			  int pageSize = 16;
			  using ( PagedFile pfA = map( file( "a" ), pageSize ), PagedFile pfB = map( existingFile( "b" ), pageSize ), PageCursor cursorA = pfA.Io( 0, PF_SHARED_READ_LOCK ), PageCursor cursorB = pfB.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					while ( cursorA.Next() )
					{
						 assertTrue( cursorB.Next() );
						 int bytesCopied;
						 do
						 {
							  bytesCopied = cursorA.CopyTo( 0, cursorB, 0, cursorA.CurrentPageSize );
						 } while ( cursorA.ShouldRetry() );
						 assertThat( bytesCopied, @is( pageSize ) );
					}
			  }

			  // Finally, verify the contents of file 'b'
			  using ( PagedFile pf = map( file( "b" ), 32 ), PageCursor cursor = pf.Io( 0, PF_SHARED_READ_LOCK ) )
			  {
					for ( int i = 0; i < bytes; i++ )
					{
						 if ( ( i & 31 ) == 0 )
						 {
							  assertTrue( cursor.Next() );
						 }
						 int offset = cursor.Offset;
						 sbyte b;
						 do
						 {
							  cursor.Offset = offset;
							  b = cursor.Byte;
						 } while ( cursor.ShouldRetry() );
						 assertThat( b, @is( ( sbyte ) i ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustCopyIntoLargerPageCursor() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustCopyIntoLargerPageCursor()
		 {
			  configureStandardPageCache();
			  int smallPageSize = 16;
			  int largePageSize = 17;
			  using ( PagedFile pfA = map( file( "a" ), smallPageSize ), PagedFile pfB = map( existingFile( "b" ), largePageSize ), PageCursor cursorA = pfA.Io( 0, PF_SHARED_WRITE_LOCK ), PageCursor cursorB = pfB.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					assertTrue( cursorA.Next() );
					for ( int i = 0; i < smallPageSize; i++ )
					{
						 cursorA.PutByte( ( sbyte )( i + 1 ) );
					}
					assertTrue( cursorB.Next() );
					assertThat( cursorA.CopyTo( 0, cursorB, 0, smallPageSize ), @is( smallPageSize ) );
					for ( int i = 0; i < smallPageSize; i++ )
					{
						 assertThat( cursorB.Byte, @is( ( sbyte )( i + 1 ) ) );
					}
					assertThat( cursorB.Byte, @is( ( sbyte ) 0 ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustCopyIntoSmallerPageCursor() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustCopyIntoSmallerPageCursor()
		 {
			  configureStandardPageCache();
			  int smallPageSize = 16;
			  int largePageSize = 17;
			  using ( PagedFile pfA = map( file( "a" ), largePageSize ), PagedFile pfB = map( existingFile( "b" ), smallPageSize ), PageCursor cursorA = pfA.Io( 0, PF_SHARED_WRITE_LOCK ), PageCursor cursorB = pfB.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					assertTrue( cursorA.Next() );
					for ( int i = 0; i < largePageSize; i++ )
					{
						 cursorA.PutByte( ( sbyte )( i + 1 ) );
					}
					assertTrue( cursorB.Next() );
					assertThat( cursorA.CopyTo( 0, cursorB, 0, largePageSize ), @is( smallPageSize ) );
					for ( int i = 0; i < smallPageSize; i++ )
					{
						 assertThat( cursorB.Byte, @is( ( sbyte )( i + 1 ) ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustThrowOnCopyIntoReadPageCursor() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustThrowOnCopyIntoReadPageCursor()
		 {
			  configureStandardPageCache();
			  int pageSize = 17;
			  using ( PagedFile pfA = map( file( "a" ), pageSize ), PagedFile pfB = map( existingFile( "b" ), pageSize ) )
			  {
					// Create data
					using ( PageCursor cursorA = pfA.Io( 0, PF_SHARED_WRITE_LOCK ), PageCursor cursorB = pfB.Io( 0, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursorA.Next() );
						 assertTrue( cursorB.Next() );
					}

					// Try copying
					using ( PageCursor cursorA = pfA.Io( 0, PF_SHARED_WRITE_LOCK ), PageCursor cursorB = pfB.Io( 0, PF_SHARED_READ_LOCK ) )
					{
						 assertTrue( cursorA.Next() );
						 assertTrue( cursorB.Next() );
						 assertThrows( typeof( System.ArgumentException ), () => cursorA.CopyTo(0, cursorB, 0, pageSize) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void copyToPageCursorMustCheckBounds() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CopyToPageCursorMustCheckBounds()
		 {
			  configureStandardPageCache();
			  int pageSize = 16;
			  using ( PagedFile pf = map( file( "a" ), pageSize ), PageCursor cursorA = pf.Io( 0, PF_SHARED_READ_LOCK ), PageCursor cursorB = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					assertTrue( cursorB.Next() );
					assertTrue( cursorB.Next() );
					assertTrue( cursorA.Next() );

					// source buffer underflow
					cursorA.CopyTo( -1, cursorB, 0, 1 );
					assertTrue( cursorA.CheckAndClearBoundsFlag() );
					assertFalse( cursorB.CheckAndClearBoundsFlag() );

					// target buffer underflow
					cursorA.CopyTo( 0, cursorB, -1, 1 );
					assertTrue( cursorA.CheckAndClearBoundsFlag() );
					assertFalse( cursorB.CheckAndClearBoundsFlag() );

					// source buffer offset overflow
					cursorA.CopyTo( pageSize, cursorB, 0, 1 );
					assertTrue( cursorA.CheckAndClearBoundsFlag() );
					assertFalse( cursorB.CheckAndClearBoundsFlag() );

					// target buffer offset overflow
					cursorA.CopyTo( 0, cursorB, pageSize, 1 );
					assertTrue( cursorA.CheckAndClearBoundsFlag() );
					assertFalse( cursorB.CheckAndClearBoundsFlag() );

					// source buffer length overflow
					assertThat( cursorA.CopyTo( 1, cursorB, 0, pageSize ), @is( pageSize - 1 ) );
					assertFalse( cursorA.CheckAndClearBoundsFlag() );
					assertFalse( cursorB.CheckAndClearBoundsFlag() );

					// target buffer length overflow
					assertThat( cursorA.CopyTo( 0, cursorB, 1, pageSize ), @is( pageSize - 1 ) );
					assertFalse( cursorA.CheckAndClearBoundsFlag() );
					assertFalse( cursorB.CheckAndClearBoundsFlag() );

					// zero length
					assertThat( cursorA.CopyTo( 0, cursorB, 1, 0 ), @is( 0 ) );
					assertFalse( cursorA.CheckAndClearBoundsFlag() );
					assertFalse( cursorB.CheckAndClearBoundsFlag() );

					// negative length
					cursorA.CopyTo( 1, cursorB, 1, -1 );
					assertTrue( cursorA.CheckAndClearBoundsFlag() );
					assertFalse( cursorB.CheckAndClearBoundsFlag() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void copyToHeapByteBufferFromReadPageCursorMustCheckBounds() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CopyToHeapByteBufferFromReadPageCursorMustCheckBounds()
		 {
			  configureStandardPageCache();
			  ByteBuffer buffer = ByteBuffer.allocate( filePageSize );
			  File file = file( "a" );
			  generateFileWithRecords( file, recordsPerFilePage, recordSize );
			  using ( PagedFile pf = map( file, filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_READ_LOCK ) )
			  {
					assertTrue( cursor.Next() );
					VerifyCopyToBufferBounds( cursor, buffer );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void copyToDirectByteBufferFromReadPageCursorMustCheckBounds() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CopyToDirectByteBufferFromReadPageCursorMustCheckBounds()
		 {
			  configureStandardPageCache();
			  ByteBuffer buffer = ByteBuffer.allocateDirect( filePageSize );
			  File file = file( "a" );
			  generateFileWithRecords( file, recordsPerFilePage, recordSize );
			  using ( PagedFile pf = map( file, filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_READ_LOCK ) )
			  {
					assertTrue( cursor.Next() );
					VerifyCopyToBufferBounds( cursor, buffer );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void copyToHeapByteBufferFromWritePageCursorMustCheckBounds() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CopyToHeapByteBufferFromWritePageCursorMustCheckBounds()
		 {
			  configureStandardPageCache();
			  ByteBuffer buffer = ByteBuffer.allocate( filePageSize );
			  File file = file( "a" );
			  generateFileWithRecords( file, recordsPerFilePage, recordSize );
			  using ( PagedFile pf = map( file, filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					assertTrue( cursor.Next() );
					VerifyCopyToBufferBounds( cursor, buffer );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void copyToDirectByteBufferFromWritePageCursorMustCheckBounds() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CopyToDirectByteBufferFromWritePageCursorMustCheckBounds()
		 {
			  configureStandardPageCache();
			  ByteBuffer buffer = ByteBuffer.allocateDirect( filePageSize );
			  File file = file( "a" );
			  generateFileWithRecords( file, recordsPerFilePage, recordSize );
			  using ( PagedFile pf = map( file, filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					assertTrue( cursor.Next() );
					VerifyCopyToBufferBounds( cursor, buffer );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyCopyToBufferBounds(PageCursor cursor, ByteBuffer buffer) throws java.io.IOException
		 private void VerifyCopyToBufferBounds( PageCursor cursor, ByteBuffer buffer )
		 {
			  // Assuming no mistakes, the data must be copied as is.
			  int copied;
			  do
			  {
					buffer.clear();
					copied = cursor.CopyTo( 0, buffer );
			  } while ( cursor.ShouldRetry() );
			  assertThat( copied, @is( filePageSize ) );
			  buffer.clear();
			  verifyRecordsMatchExpected( 0, 0, buffer );

			  // Source buffer underflow.
			  buffer.clear();
			  cursor.CopyTo( -1, buffer );
			  assertTrue( cursor.CheckAndClearBoundsFlag() );

			  // Target buffer overflow^W truncation.
			  buffer.clear();
			  copied = cursor.CopyTo( 1, buffer );
			  assertFalse( cursor.CheckAndClearBoundsFlag() );
			  assertThat( copied, @is( filePageSize - 1 ) );
			  assertThat( buffer.position(), @is(filePageSize - 1) );
			  assertThat( buffer.remaining(), @is(1) );
			  buffer.clear();

			  // Smaller buffer at offset zero.
			  ZapBuffer( buffer );
			  do
			  {
					buffer.clear();
					buffer.limit( filePageSize - recordSize );
					copied = cursor.CopyTo( 0, buffer );
			  } while ( cursor.ShouldRetry() );
			  assertThat( copied, @is( filePageSize - recordSize ) );
			  assertThat( buffer.position(), @is(filePageSize - recordSize) );
			  assertThat( buffer.remaining(), @is(0) );
			  buffer.clear();
			  buffer.limit( filePageSize - recordSize );
			  verifyRecordsMatchExpected( 0, 0, buffer );

			  // Smaller buffer at non-zero offset.
			  ZapBuffer( buffer );
			  do
			  {
					buffer.clear();
					buffer.limit( filePageSize - recordSize );
					copied = cursor.CopyTo( recordSize, buffer );
			  } while ( cursor.ShouldRetry() );
			  assertThat( copied, @is( filePageSize - recordSize ) );
			  assertThat( buffer.position(), @is(filePageSize - recordSize) );
			  assertThat( buffer.remaining(), @is(0) );
			  buffer.clear();
			  buffer.limit( filePageSize - recordSize );
			  verifyRecordsMatchExpected( 0, recordSize, buffer );
		 }

		 private void ZapBuffer( ByteBuffer buffer )
		 {
			  sbyte zero = ( sbyte ) 0;
			  if ( buffer.hasArray() )
			  {
					Arrays.fill( buffer.array(), zero );
			  }
			  else
			  {
					buffer.clear();
					while ( buffer.hasRemaining() )
					{
						 buffer.put( zero );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void copyToReadOnlyHeapByteBufferMustThrow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CopyToReadOnlyHeapByteBufferMustThrow()
		 {
			  configureStandardPageCache();
			  ByteBuffer buf = ByteBuffer.allocate( filePageSize ).asReadOnlyBuffer();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					assertTrue( cursor.Next() );

					assertThrows( typeof( ReadOnlyBufferException ), () => cursor.CopyTo(0, buf) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void copyToReadOnlyDirectByteBufferMustThrow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CopyToReadOnlyDirectByteBufferMustThrow()
		 {
			  configureStandardPageCache();
			  ByteBuffer buf = ByteBuffer.allocateDirect( filePageSize ).asReadOnlyBuffer();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					assertTrue( cursor.Next() );

					assertThrows( typeof( ReadOnlyBufferException ), () => cursor.CopyTo(0, buf) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shiftBytesMustNotRaiseOutOfBoundsOnLengthWithinPageBoundary() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShiftBytesMustNotRaiseOutOfBoundsOnLengthWithinPageBoundary()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					assertTrue( cursor.Next() );
					cursor.ShiftBytes( 0, filePageSize, 0 );
					assertFalse( cursor.CheckAndClearBoundsFlag() );
					cursor.ShiftBytes( 0, filePageSize - 1, 1 );
					assertFalse( cursor.CheckAndClearBoundsFlag() );
					cursor.ShiftBytes( 1, filePageSize - 1, -1 );
					assertFalse( cursor.CheckAndClearBoundsFlag() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shiftBytesMustRaiseOutOfBoundsOnLengthLargerThanPageSize() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShiftBytesMustRaiseOutOfBoundsOnLengthLargerThanPageSize()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					assertTrue( cursor.Next() );
					cursor.ShiftBytes( 0, filePageSize + 1, 0 );
					assertTrue( cursor.CheckAndClearBoundsFlag() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shiftBytesMustRaiseOutOfBoundsOnNegativeLength() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShiftBytesMustRaiseOutOfBoundsOnNegativeLength()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					assertTrue( cursor.Next() );
					cursor.ShiftBytes( 1, -1, 0 );
					assertTrue( cursor.CheckAndClearBoundsFlag() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shiftBytesMustRaiseOutOfBoundsOnNegativeSource() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShiftBytesMustRaiseOutOfBoundsOnNegativeSource()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					assertTrue( cursor.Next() );
					cursor.ShiftBytes( -1, 10, 0 );
					assertTrue( cursor.CheckAndClearBoundsFlag() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shiftBytesMustRaiseOutOfBoundsOnOverSizedSource() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShiftBytesMustRaiseOutOfBoundsOnOverSizedSource()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					assertTrue( cursor.Next() );
					cursor.ShiftBytes( filePageSize, 1, 0 );
					assertTrue( cursor.CheckAndClearBoundsFlag() );
					cursor.ShiftBytes( filePageSize + 1, 0, 0 );
					assertTrue( cursor.CheckAndClearBoundsFlag() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shiftBytesMustRaiseOutOfBoundsOnBufferUnderflow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShiftBytesMustRaiseOutOfBoundsOnBufferUnderflow()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					assertTrue( cursor.Next() );
					cursor.ShiftBytes( 0, 1, -1 );
					assertTrue( cursor.CheckAndClearBoundsFlag() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shiftBytesMustRaiseOutOfBoundsOnBufferOverflow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShiftBytesMustRaiseOutOfBoundsOnBufferOverflow()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					assertTrue( cursor.Next() );
					cursor.ShiftBytes( filePageSize - 1, 1, 1 );
					assertTrue( cursor.CheckAndClearBoundsFlag() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shiftBytesMustThrowOnReadCursor() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShiftBytesMustThrowOnReadCursor()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor writer = pf.Io( 0, PF_SHARED_WRITE_LOCK ), PageCursor reader = pf.Io( 0, PF_SHARED_READ_LOCK ) )
			  {
					assertTrue( writer.Next() );
					assertTrue( reader.Next() );

					assertThrows( typeof( System.InvalidOperationException ), () => reader.shiftBytes(0, 0, 0) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shiftBytesMustShiftBytesToTheRightOverlapping() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShiftBytesMustShiftBytesToTheRightOverlapping()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					assertTrue( cursor.Next() );

					sbyte[] bytes = new sbyte[30];
					for ( int i = 0; i < bytes.Length; i++ )
					{
						 bytes[i] = ( sbyte )( i + 1 );
					}

					int srcOffset = 10;
					cursor.Offset = srcOffset;
					cursor.PutBytes( bytes );

					int shift = 5;
					AssertZeroes( cursor, 0, srcOffset );
					AssertZeroes( cursor, srcOffset + bytes.Length, filePageSize - srcOffset - bytes.Length );

					cursor.ShiftBytes( srcOffset, bytes.Length, shift );

					AssertZeroes( cursor, 0, srcOffset );
					AssertZeroes( cursor, srcOffset + bytes.Length + shift, filePageSize - srcOffset - bytes.Length - shift );

					cursor.Offset = srcOffset;
					for ( int i = 0; i < shift; i++ )
					{
						 assertThat( cursor.Byte, @is( ( sbyte )( i + 1 ) ) );
					}
					for ( int i = 0; i < bytes.Length; i++ )
					{
						 assertThat( cursor.Byte, @is( ( sbyte )( i + 1 ) ) );
					}

					assertFalse( cursor.CheckAndClearBoundsFlag() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shiftBytesMustShiftBytesToTheRightNotOverlapping() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShiftBytesMustShiftBytesToTheRightNotOverlapping()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					assertTrue( cursor.Next() );

					sbyte[] bytes = new sbyte[30];
					for ( int i = 0; i < bytes.Length; i++ )
					{
						 bytes[i] = ( sbyte )( i + 1 );
					}

					int srcOffset = 10;
					cursor.Offset = srcOffset;
					cursor.PutBytes( bytes );

					int gap = 5;
					int shift = bytes.Length + gap;
					AssertZeroes( cursor, 0, srcOffset );
					AssertZeroes( cursor, srcOffset + bytes.Length, filePageSize - srcOffset - bytes.Length );

					cursor.ShiftBytes( srcOffset, bytes.Length, shift );

					AssertZeroes( cursor, 0, srcOffset );
					AssertZeroes( cursor, srcOffset + bytes.Length + shift, filePageSize - srcOffset - bytes.Length - shift );

					cursor.Offset = srcOffset;
					for ( int i = 0; i < bytes.Length; i++ )
					{
						 assertThat( cursor.Byte, @is( ( sbyte )( i + 1 ) ) );
					}
					AssertZeroes( cursor, srcOffset + bytes.Length + shift, gap );
					cursor.Offset = srcOffset + shift;
					for ( int i = 0; i < bytes.Length; i++ )
					{
						 assertThat( cursor.Byte, @is( ( sbyte )( i + 1 ) ) );
					}

					assertFalse( cursor.CheckAndClearBoundsFlag() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shiftBytesMustShiftBytesToTheLeftOverlapping() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShiftBytesMustShiftBytesToTheLeftOverlapping()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					assertTrue( cursor.Next() );

					sbyte[] bytes = new sbyte[30];
					for ( int i = 0; i < bytes.Length; i++ )
					{
						 bytes[i] = ( sbyte )( i + 1 );
					}

					int srcOffset = 10;
					cursor.Offset = srcOffset;
					cursor.PutBytes( bytes );

					int shift = -5;
					AssertZeroes( cursor, 0, srcOffset );
					AssertZeroes( cursor, srcOffset + bytes.Length, filePageSize - srcOffset - bytes.Length );

					cursor.ShiftBytes( srcOffset, bytes.Length, shift );

					AssertZeroes( cursor, 0, srcOffset + shift );
					AssertZeroes( cursor, srcOffset + bytes.Length, filePageSize - srcOffset - bytes.Length );

					cursor.Offset = srcOffset + shift;
					for ( int i = 0; i < bytes.Length; i++ )
					{
						 assertThat( cursor.Byte, @is( ( sbyte )( i + 1 ) ) );
					}
					for ( int i = shift; i < 0; i++ )
					{
						 assertThat( cursor.Byte, @is( ( sbyte )( bytes.Length + i + 1 ) ) );
					}

					assertFalse( cursor.CheckAndClearBoundsFlag() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shiftBytesMustShiftBytesToTheLeftNotOverlapping() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShiftBytesMustShiftBytesToTheLeftNotOverlapping()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					assertTrue( cursor.Next() );

					sbyte[] bytes = new sbyte[30];
					for ( int i = 0; i < bytes.Length; i++ )
					{
						 bytes[i] = ( sbyte )( i + 1 );
					}

					int srcOffset = filePageSize - bytes.Length - 10;
					cursor.Offset = srcOffset;
					cursor.PutBytes( bytes );

					int gap = 5;
					int shift = -bytes.Length - gap;
					AssertZeroes( cursor, 0, srcOffset );
					AssertZeroes( cursor, srcOffset + bytes.Length, filePageSize - srcOffset - bytes.Length );

					cursor.ShiftBytes( srcOffset, bytes.Length, shift );

					AssertZeroes( cursor, 0, srcOffset + shift );
					AssertZeroes( cursor, srcOffset + bytes.Length, filePageSize - srcOffset - bytes.Length );

					cursor.Offset = srcOffset + shift;
					for ( int i = 0; i < bytes.Length; i++ )
					{
						 assertThat( cursor.Byte, @is( ( sbyte )( i + 1 ) ) );
					}
					AssertZeroes( cursor, srcOffset + bytes.Length + shift, gap );
					cursor.Offset = srcOffset;
					for ( int i = 0; i < bytes.Length; i++ )
					{
						 assertThat( cursor.Byte, @is( ( sbyte )( i + 1 ) ) );
					}

					assertFalse( cursor.CheckAndClearBoundsFlag() );
			  }
		 }

		 private void AssertZeroes( PageCursor cursor, int offset, int length )
		 {
			  for ( int i = 0; i < length; i++ )
			  {
					assertThat( cursor.GetByte( offset + i ), @is( ( sbyte ) 0 ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void readCursorsCanOpenLinkedCursor()
		 internal virtual void ReadCursorsCanOpenLinkedCursor()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();
				generateFileWithRecords( file( "a" ), recordsPerFilePage * 2, recordSize );
				using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor parent = pf.Io( 0, PF_SHARED_READ_LOCK ) )
				{
					 PageCursor linked = parent.OpenLinkedCursor( 1 );
					 assertTrue( parent.Next() );
					 assertTrue( linked.Next() );
					 verifyRecordsMatchExpected( parent );
					 verifyRecordsMatchExpected( linked );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void writeCursorsCanOpenLinkedCursor()
		 internal virtual void WriteCursorsCanOpenLinkedCursor()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();
				File file = file( "a" );
				using ( PagedFile pf = map( file, filePageSize ), PageCursor parent = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
				{
					 PageCursor linked = parent.OpenLinkedCursor( 1 );
					 assertTrue( parent.Next() );
					 assertTrue( linked.Next() );
					 writeRecords( parent );
					 writeRecords( linked );
				}
				verifyRecordsInFile( file, recordsPerFilePage * 2 );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void closingParentCursorMustCloseLinkedCursor()
		 internal virtual void ClosingParentCursorMustCloseLinkedCursor()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();
				using ( PagedFile pf = map( file( "a" ), filePageSize ) )
				{
					 PageCursor writerParent = pf.Io( 0, PF_SHARED_WRITE_LOCK );
					 PageCursor readerParent = pf.Io( 0, PF_SHARED_READ_LOCK );
					 assertTrue( writerParent.Next() );
					 assertTrue( readerParent.Next() );
					 PageCursor writerLinked = writerParent.OpenLinkedCursor( 1 );
					 PageCursor readerLinked = readerParent.OpenLinkedCursor( 1 );
					 assertTrue( writerLinked.Next() );
					 assertTrue( readerLinked.Next() );
					 writerParent.Close();
					 readerParent.Close();
					 writerLinked.GetByte( 0 );
					 assertTrue( writerLinked.CheckAndClearBoundsFlag() );
					 readerLinked.GetByte( 0 );
					 assertTrue( readerLinked.CheckAndClearBoundsFlag() );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void writeCursorWithNoGrowCanOpenLinkedCursorWithNoGrow()
		 internal virtual void WriteCursorWithNoGrowCanOpenLinkedCursorWithNoGrow()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();
				generateFileWithRecords( file( "a" ), recordsPerFilePage * 2, recordSize );
				using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor parent = pf.Io( 0, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
				{
					 PageCursor linked = parent.OpenLinkedCursor( 1 );
					 assertTrue( parent.Next() );
					 assertTrue( linked.Next() );
					 verifyRecordsMatchExpected( parent );
					 verifyRecordsMatchExpected( linked );
					 assertFalse( linked.Next() );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void openingLinkedCursorMustCloseExistingLinkedCursor()
		 internal virtual void OpeningLinkedCursorMustCloseExistingLinkedCursor()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();
				File file = file( "a" );

				// write case
				using ( PagedFile pf = map( file, filePageSize ), PageCursor parent = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
				{
					 PageCursor linked = parent.OpenLinkedCursor( 1 );
					 assertTrue( parent.Next() );
					 assertTrue( linked.Next() );
					 writeRecords( parent );
					 writeRecords( linked );
					 parent.OpenLinkedCursor( 2 );

					 // should cause out of bounds condition because it should be closed by our opening of another linked cursor
					 linked.PutByte( 0, ( sbyte ) 1 );
					 assertTrue( linked.CheckAndClearBoundsFlag() );
				}

				// read case
				using ( PagedFile pf = map( file, filePageSize ), PageCursor parent = pf.Io( 0, PF_SHARED_READ_LOCK ) )
				{
					 PageCursor linked = parent.OpenLinkedCursor( 1 );
					 assertTrue( parent.Next() );
					 assertTrue( linked.Next() );
					 parent.OpenLinkedCursor( 2 );

					 // should cause out of bounds condition because it should be closed by our opening of another linked cursor
					 linked.GetByte( 0 );
					 assertTrue( linked.CheckAndClearBoundsFlag() );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRetryOnParentCursorMustReturnTrueIfLinkedCursorNeedsRetry()
		 internal virtual void ShouldRetryOnParentCursorMustReturnTrueIfLinkedCursorNeedsRetry()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();
				generateFileWithRecords( file( "a" ), recordsPerFilePage * 2, recordSize );
				using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor parentReader = pf.Io( 0, PF_SHARED_READ_LOCK ), PageCursor writer = pf.Io( 1, PF_SHARED_WRITE_LOCK ) )
				{
					 PageCursor linkedReader = parentReader.OpenLinkedCursor( 1 );
					 assertTrue( parentReader.Next() );
					 assertTrue( linkedReader.Next() );
					 assertTrue( writer.Next() );
					 assertTrue( writer.Next() ); // writer now moved on to page 2

					 // parentReader shouldRetry should be true because the linked cursor needs retry
					 assertTrue( parentReader.ShouldRetry() );
					 // then, the next read should be consistent
					 assertFalse( parentReader.ShouldRetry() );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void checkAndClearBoundsFlagMustCheckAndClearLinkedCursor()
		 internal virtual void CheckAndClearBoundsFlagMustCheckAndClearLinkedCursor()
		 {
			  assertTimeout(ofMillis(SHORT_TIMEOUT_MILLIS), () =>
			  {
				configureStandardPageCache();
				using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor parent = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
				{
					 assertTrue( parent.Next() );
					 PageCursor linked = parent.OpenLinkedCursor( 1 );
					 linked.RaiseOutOfBounds();
					 assertTrue( parent.CheckAndClearBoundsFlag() );
					 assertFalse( linked.CheckAndClearBoundsFlag() );
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRetryMustClearBoundsFlagIfLinkedCursorNeedsRetry() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRetryMustClearBoundsFlagIfLinkedCursorNeedsRetry()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor writer = pf.Io( 0, PF_SHARED_WRITE_LOCK ), PageCursor reader = pf.Io( 0, PF_SHARED_READ_LOCK ) )
			  {
					assertTrue( writer.Next() ); // now at page id 0
					assertTrue( writer.Next() ); // now at page id 1, 0 is unlocked
					assertTrue( writer.Next() ); // now at page id 2, 1 is unlocked
					assertTrue( reader.Next() ); // reader now at page id 0
					using ( PageCursor linkedReader = reader.OpenLinkedCursor( 1 ) )
					{
						 assertTrue( linkedReader.Next() ); // linked reader now at page id 1
						 assertTrue( writer.Next( 1 ) ); // invalidate linked readers lock
						 assertTrue( writer.Next() ); // move writer out of the way
						 reader.RaiseOutOfBounds(); // raise bounds flag on parent reader
						 assertTrue( reader.ShouldRetry() ); // we must retry because linked reader was invalidated
						 assertFalse( reader.CheckAndClearBoundsFlag() ); // must return false because we are doing a retry
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void checkAndClearCursorExceptionMustNotThrowIfNoExceptionIsSet() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CheckAndClearCursorExceptionMustNotThrowIfNoExceptionIsSet()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ) )
			  {
					using ( PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 cursor.CheckAndClearCursorException();
					}
					using ( PageCursor cursor = pf.Io( 0, PF_SHARED_READ_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 cursor.CheckAndClearCursorException();
						 //noinspection StatementWithEmptyBody
						 do
						 {
							  // nothing
						 } while ( cursor.ShouldRetry() );
						 cursor.CheckAndClearCursorException();
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void checkAndClearCursorExceptionMustThrowIfExceptionIsSet() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CheckAndClearCursorExceptionMustThrowIfExceptionIsSet()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ) )
			  {
					string msg = "Boo" + ThreadLocalRandom.current().Next();
					try
					{
							using ( PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
							{
							 assertTrue( cursor.Next() );
							 cursor.CursorException = msg;
							 cursor.CheckAndClearCursorException();
							 fail( "checkAndClearError on write cursor should have thrown" );
							}
					}
					catch ( CursorException e )
					{
						 assertThat( e.Message, @is( msg ) );
					}

					msg = "Boo" + ThreadLocalRandom.current().Next();
					try
					{
							using ( PageCursor cursor = pf.Io( 0, PF_SHARED_READ_LOCK ) )
							{
							 assertTrue( cursor.Next() );
							 cursor.CursorException = msg;
							 cursor.CheckAndClearCursorException();
							 fail( "checkAndClearError on read cursor should have thrown" );
							}
					}
					catch ( CursorException e )
					{
						 assertThat( e.Message, @is( msg ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void checkAndClearCursorExceptionMustClearExceptionIfSet() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CheckAndClearCursorExceptionMustClearExceptionIfSet()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ) )
			  {
					using ( PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 cursor.CursorException = "boo";
						 try
						 {
							  cursor.CheckAndClearCursorException();
							  fail( "checkAndClearError on write cursor should have thrown" );
						 }
						 catch ( CursorException )
						 {
						 }
						 cursor.CheckAndClearCursorException();
					}

					using ( PageCursor cursor = pf.Io( 0, PF_SHARED_READ_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 cursor.CursorException = "boo";
						 try
						 {
							  cursor.CheckAndClearCursorException();
							  fail( "checkAndClearError on read cursor should have thrown" );
						 }
						 catch ( CursorException )
						 {
						 }
						 cursor.CheckAndClearCursorException();
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nextMustClearCursorExceptionIfSet() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void NextMustClearCursorExceptionIfSet()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ) )
			  {
					using ( PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 cursor.CursorException = "boo";
						 assertTrue( cursor.Next() );
						 cursor.CheckAndClearCursorException();
					}

					using ( PageCursor cursor = pf.Io( 0, PF_SHARED_READ_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 cursor.CursorException = "boo";
						 assertTrue( cursor.Next() );
						 cursor.CheckAndClearCursorException();
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nextWithIdMustClearCursorExceptionIfSet() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void NextWithIdMustClearCursorExceptionIfSet()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ) )
			  {
					using ( PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursor.Next( 1 ) );
						 cursor.CursorException = "boo";
						 assertTrue( cursor.Next( 2 ) );
						 cursor.CheckAndClearCursorException();
					}

					using ( PageCursor cursor = pf.Io( 0, PF_SHARED_READ_LOCK ) )
					{
						 assertTrue( cursor.Next( 1 ) );
						 cursor.CursorException = "boo";
						 assertTrue( cursor.Next( 2 ) );
						 cursor.CheckAndClearCursorException();
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRetryMustClearCursorExceptionIfItReturnsTrue() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRetryMustClearCursorExceptionIfItReturnsTrue()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor writer = pf.Io( 0, PF_SHARED_WRITE_LOCK ), PageCursor reader = pf.Io( 0, PF_SHARED_READ_LOCK ) )
			  {
					assertTrue( writer.Next() ); // now at page id 0
					assertTrue( writer.Next() ); // now at page id 1, 0 is unlocked
					assertTrue( reader.Next() ); // now at page id 0
					assertTrue( writer.Next( 0 ) ); // invalidate the readers lock on page 0
					assertTrue( writer.Next() ); // move writer out of the way
					reader.CursorException = "boo";
					assertTrue( reader.ShouldRetry() ); // this should clear the cursor error
					reader.CheckAndClearCursorException();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRetryMustNotClearCursorExceptionIfItReturnsFalse() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRetryMustNotClearCursorExceptionIfItReturnsFalse()
		 {
			  configureStandardPageCache();
			  File file = file( "a" );
			  generateFileWithRecords( file, recordCount, recordSize );
			  using ( PagedFile pf = map( file, filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_READ_LOCK ) )
			  {
					assertTrue( cursor.Next() );
					do
					{
						 cursor.CursorException = "boo";
					} while ( cursor.ShouldRetry() );
					// The last shouldRetry has obviously returned 'false'
					assertThrows( typeof( CursorException ), cursor.checkAndClearCursorException );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRetryMustClearCursorExceptionIfLinkedShouldRetryReturnsTrue() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRetryMustClearCursorExceptionIfLinkedShouldRetryReturnsTrue()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor writer = pf.Io( 0, PF_SHARED_WRITE_LOCK ), PageCursor reader = pf.Io( 0, PF_SHARED_READ_LOCK ) )
			  {
					assertTrue( writer.Next() ); // now at page id 0
					assertTrue( writer.Next() ); // now at page id 1, 0 is unlocked
					assertTrue( writer.Next() ); // now at page id 2, 1 is unlocked
					assertTrue( reader.Next() ); // reader now at page id 0
					using ( PageCursor linkedReader = reader.OpenLinkedCursor( 1 ) )
					{
						 assertTrue( linkedReader.Next() ); // linked reader now at page id 1
						 assertTrue( writer.Next( 1 ) ); // invalidate linked readers lock
						 assertTrue( writer.Next() ); // move writer out of the way
						 reader.CursorException = "boo"; // raise cursor error on parent reader
						 assertTrue( reader.ShouldRetry() ); // we must retry because linked reader was invalidated
						 reader.CheckAndClearCursorException(); // must not throw because shouldRetry returned true
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRetryMustClearLinkedCursorExceptionIfItReturnsTrue() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRetryMustClearLinkedCursorExceptionIfItReturnsTrue()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor writer = pf.Io( 0, PF_SHARED_WRITE_LOCK ), PageCursor reader = pf.Io( 0, PF_SHARED_READ_LOCK ) )
			  {
					assertTrue( writer.Next() ); // now at page id 0
					assertTrue( writer.Next() ); // now at page id 1, 0 is unlocked
					assertTrue( writer.Next() ); // now at page id 2, 1 is unlocked
					assertTrue( reader.Next() ); // reader now at page id 0
					using ( PageCursor linkedReader = reader.OpenLinkedCursor( 1 ) )
					{
						 assertTrue( linkedReader.Next() ); // linked reader now at page id 1
						 linkedReader.CursorException = "boo";
						 assertTrue( writer.Next( 0 ) ); // invalidate the read lock held by the parent reader
						 assertTrue( reader.ShouldRetry() ); // this should clear the linked cursor error
						 linkedReader.CheckAndClearCursorException();
						 reader.CheckAndClearCursorException();
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRetryMustClearLinkedCursorExceptionIfLinkedShouldRetryReturnsTrue() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRetryMustClearLinkedCursorExceptionIfLinkedShouldRetryReturnsTrue()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor writer = pf.Io( 0, PF_SHARED_WRITE_LOCK ), PageCursor reader = pf.Io( 0, PF_SHARED_READ_LOCK ) )
			  {
					assertTrue( writer.Next() ); // now at page id 0
					assertTrue( writer.Next() ); // now at page id 1, 0 is unlocked
					assertTrue( writer.Next() ); // now at page id 2, 1 is unlocked
					assertTrue( reader.Next() ); // reader now at page id 0
					using ( PageCursor linkedReader = reader.OpenLinkedCursor( 1 ) )
					{
						 assertTrue( linkedReader.Next() ); // linked reader now at page id 1
						 linkedReader.CursorException = "boo";
						 assertTrue( writer.Next( 1 ) ); // invalidate the read lock held by the linked reader
						 assertTrue( reader.ShouldRetry() ); // this should clear the linked cursor error
						 linkedReader.CheckAndClearCursorException();
						 reader.CheckAndClearCursorException();
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRetryMustNotClearCursorExceptionIfBothItAndLinkedShouldRetryReturnsFalse() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRetryMustNotClearCursorExceptionIfBothItAndLinkedShouldRetryReturnsFalse()
		 {
			  configureStandardPageCache();
			  File file = file( "a" );
			  generateFileWithRecords( file, recordCount, recordSize );
			  using ( PagedFile pf = map( file, filePageSize ), PageCursor reader = pf.Io( 0, PF_SHARED_READ_LOCK ), PageCursor linkedReader = reader.OpenLinkedCursor( 1 ) )
			  {
					assertTrue( reader.Next() );
					assertTrue( linkedReader.Next() );
					do
					{
						 reader.CursorException = "boo";
					} while ( reader.ShouldRetry() );
					assertThrows( typeof( CursorException ), reader.checkAndClearCursorException );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRetryMustNotClearLinkedCursorExceptionIfBothItAndLinkedShouldRetryReturnsFalse() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRetryMustNotClearLinkedCursorExceptionIfBothItAndLinkedShouldRetryReturnsFalse()
		 {
			  configureStandardPageCache();
			  File file = file( "a" );
			  generateFileWithRecords( file, recordCount, recordSize );
			  using ( PagedFile pf = map( file, filePageSize ), PageCursor reader = pf.Io( 0, PF_SHARED_READ_LOCK ), PageCursor linkedReader = reader.OpenLinkedCursor( 1 ) )
			  {
					assertTrue( reader.Next() );
					assertTrue( linkedReader.Next() );
					do
					{
						 linkedReader.CursorException = "boo";
					} while ( reader.ShouldRetry() );
					assertThrows( typeof( CursorException ), reader.checkAndClearCursorException );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void checkAndClearCursorExceptionMustThrowIfLinkedCursorHasErrorSet() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CheckAndClearCursorExceptionMustThrowIfLinkedCursorHasErrorSet()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor writer = pf.Io( 0, PF_SHARED_WRITE_LOCK ), PageCursor reader = pf.Io( 0, PF_SHARED_READ_LOCK ) )
			  {
					string msg = "Boo" + ThreadLocalRandom.current().Next();
					assertTrue( writer.Next() );
					using ( PageCursor linkedWriter = writer.OpenLinkedCursor( 1 ) )
					{
						 assertTrue( linkedWriter.Next() );
						 linkedWriter.CursorException = msg;
						 CursorException exception = assertThrows( typeof( CursorException ), writer.checkAndClearCursorException );
						 assertThat( exception.Message, @is( msg ) );
					}

					msg = "Boo" + ThreadLocalRandom.current().Next();
					assertTrue( reader.Next() );
					using ( PageCursor linkedReader = reader.OpenLinkedCursor( 1 ) )
					{
						 assertTrue( linkedReader.Next() );
						 linkedReader.CursorException = msg;
						 CursorException exception = assertThrows( typeof( CursorException ), reader.checkAndClearCursorException );
						 assertThat( exception.Message, @is( msg ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void checkAndClearCursorMustNotThrowIfErrorHasBeenSetButTheCursorHasBeenClosed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CheckAndClearCursorMustNotThrowIfErrorHasBeenSetButTheCursorHasBeenClosed()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ) )
			  {
					PageCursor writer = pf.Io( 0, PF_SHARED_WRITE_LOCK );
					assertTrue( writer.Next() );
					writer.CursorException = "boo";
					writer.Close();
					writer.CheckAndClearCursorException();

					PageCursor reader = pf.Io( 0, PF_SHARED_READ_LOCK );
					assertTrue( reader.Next() );
					reader.CursorException = "boo";
					reader.Close();
					reader.CheckAndClearCursorException();

					writer = pf.Io( 0, PF_SHARED_WRITE_LOCK );
					PageCursor linkedWriter = writer.OpenLinkedCursor( 1 );
					assertTrue( linkedWriter.Next() );
					linkedWriter.CursorException = "boo";
					writer.Close();
					linkedWriter.CheckAndClearCursorException();

					reader = pf.Io( 0, PF_SHARED_READ_LOCK );
					PageCursor linkedReader = reader.OpenLinkedCursor( 1 );
					assertTrue( linkedReader.Next() );
					linkedReader.CursorException = "boo";
					reader.Close();
					linkedReader.CheckAndClearCursorException();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void openingLinkedCursorOnClosedCursorMustThrow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void OpeningLinkedCursorOnClosedCursorMustThrow()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ) )
			  {
					PageCursor writer = pf.Io( 0, PF_SHARED_WRITE_LOCK );
					assertTrue( writer.Next() );
					writer.Close();
					assertThrows( typeof( System.InvalidOperationException ), () => writer.OpenLinkedCursor(1) );

					PageCursor reader = pf.Io( 0, PF_SHARED_READ_LOCK );
					assertTrue( reader.Next() );
					reader.Close();
					assertThrows( typeof( System.InvalidOperationException ), () => reader.OpenLinkedCursor(1) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void settingNullCursorExceptionMustThrow() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SettingNullCursorExceptionMustThrow()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor writer = pf.Io( 0, PF_SHARED_WRITE_LOCK ), PageCursor reader = pf.Io( 0, PF_SHARED_READ_LOCK ) )
			  {
					assertTrue( writer.Next() );
					assertThrows( typeof( Exception ), () => writer.setCursorException(null) );

					assertTrue( reader.Next() );
					assertThrows( typeof( Exception ), () => reader.setCursorException(null) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void clearCursorExceptionMustUnsetErrorCondition() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ClearCursorExceptionMustUnsetErrorCondition()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor writer = pf.Io( 0, PF_SHARED_WRITE_LOCK ), PageCursor reader = pf.Io( 0, PF_SHARED_READ_LOCK ) )
			  {
					assertTrue( writer.Next() );
					writer.CursorException = "boo";
					writer.ClearCursorException();
					writer.CheckAndClearCursorException();

					assertTrue( reader.Next() );
					reader.CursorException = "boo";
					reader.ClearCursorException();
					reader.CheckAndClearCursorException();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void clearCursorExceptionMustUnsetErrorConditionOnLinkedCursor() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ClearCursorExceptionMustUnsetErrorConditionOnLinkedCursor()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor writer = pf.Io( 0, PF_SHARED_WRITE_LOCK ), PageCursor reader = pf.Io( 0, PF_SHARED_READ_LOCK ) )
			  {
					assertTrue( writer.Next() );
					PageCursor linkedWriter = writer.OpenLinkedCursor( 1 );
					assertTrue( linkedWriter.Next() );
					linkedWriter.CursorException = "boo";
					writer.ClearCursorException();
					writer.CheckAndClearCursorException();

					assertTrue( reader.Next() );
					PageCursor linkedReader = reader.OpenLinkedCursor( 1 );
					assertTrue( linkedReader.Next() );
					linkedReader.CursorException = "boo";
					reader.ClearCursorException();
					reader.CheckAndClearCursorException();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void sizeOfEmptyFileMustBeZero() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SizeOfEmptyFileMustBeZero()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ) )
			  {
					assertThat( pf.FileSize(), @is(0L) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void fileSizeMustIncreaseInPageIncrements() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void FileSizeMustIncreaseInPageIncrements()
		 {
			  configureStandardPageCache();
			  long increment = filePageSize;
			  using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					assertTrue( cursor.Next() );
					assertThat( pf.FileSize(), @is(increment) );
					assertTrue( cursor.Next() );
					assertThat( pf.FileSize(), @is(2 * increment) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldZeroAllBytesOnClear() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldZeroAllBytesOnClear()
		 {
			  // GIVEN
			  configureStandardPageCache();
			  using ( PagedFile pagedFile = map( file( "a" ), filePageSize ) )
			  {
					long pageId = 0;
					using ( PageCursor cursor = pagedFile.Io( pageId, PF_SHARED_WRITE_LOCK ) )
					{
						 ThreadLocalRandom rng = ThreadLocalRandom.current();
						 sbyte[] bytes = new sbyte[filePageSize];
						 rng.NextBytes( bytes );

						 assertTrue( cursor.Next() );
						 cursor.PutBytes( bytes );
					}
					// WHEN
					sbyte[] allZeros = new sbyte[filePageSize];
					using ( PageCursor cursor = pagedFile.Io( pageId, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 cursor.ZapPage();

						 sbyte[] read = new sbyte[filePageSize];
						 cursor.GetBytes( read );
						 assertFalse( cursor.CheckAndClearBoundsFlag() );
						 assertArrayEquals( allZeros, read );
					}
					// THEN
					using ( PageCursor cursor = pagedFile.Io( pageId, PF_SHARED_READ_LOCK ) )
					{
						 assertTrue( cursor.Next() );

						 sbyte[] read = new sbyte[filePageSize];
						 do
						 {
							  cursor.GetBytes( read );
						 } while ( cursor.ShouldRetry() );
						 assertFalse( cursor.CheckAndClearBoundsFlag() );
						 assertArrayEquals( allZeros, read );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void isWriteLockingMustBeTrueForCursorOpenedWithSharedWriteLock() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void isWriteLockingMustBeTrueForCursorOpenedWithSharedWriteLock()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					assertTrue( cursor.WriteLocked );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void isWriteLockingMustBeFalseForCursorOpenedWithSharedReadLock() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void isWriteLockingMustBeFalseForCursorOpenedWithSharedReadLock()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_READ_LOCK ) )
			  {
					assertFalse( cursor.WriteLocked );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void eagerFlushMustWriteToFileOnUnpin() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void EagerFlushMustWriteToFileOnUnpin()
		 {
			  configureStandardPageCache();
			  File file = file( "a" );
			  using ( PagedFile pf = map( file, filePageSize ), PageCursor cursor = pf.Io( 0, PF_SHARED_WRITE_LOCK | PF_EAGER_FLUSH ) )
			  {
					assertTrue( cursor.Next() );
					writeRecords( cursor );
					assertTrue( cursor.Next() ); // this will unpin and flush page 0
					verifyRecordsInFile( file, recordsPerFilePage );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void noFaultNextReadOnInMemoryPages() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void NoFaultNextReadOnInMemoryPages()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor faulter = pf.Io( 0, PF_SHARED_WRITE_LOCK ), PageCursor nofault = pf.Io( 0, PF_SHARED_READ_LOCK | PF_NO_FAULT ) )
			  {
					VerifyNoFaultAccessToInMemoryPages( faulter, nofault );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void noFaultNextWriteOnInMemoryPages() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void NoFaultNextWriteOnInMemoryPages()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor faulter = pf.Io( 0, PF_SHARED_WRITE_LOCK ), PageCursor nofault = pf.Io( 0, PF_SHARED_WRITE_LOCK | PF_NO_FAULT ) )
			  {
					VerifyNoFaultAccessToInMemoryPages( faulter, nofault );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void noFaultNextLinkedReadOnInMemoryPages() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void NoFaultNextLinkedReadOnInMemoryPages()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor faulter = pf.Io( 0, PF_SHARED_WRITE_LOCK ), PageCursor nofault = pf.Io( 0, PF_SHARED_READ_LOCK | PF_NO_FAULT ), PageCursor linkedNoFault = nofault.OpenLinkedCursor( 0 ) )
			  {
					VerifyNoFaultAccessToInMemoryPages( faulter, linkedNoFault );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void noFaultNextLinkedWriteOnInMemoryPages() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void NoFaultNextLinkedWriteOnInMemoryPages()
		 {
			  configureStandardPageCache();
			  using ( PagedFile pf = map( file( "a" ), filePageSize ), PageCursor faulter = pf.Io( 0, PF_SHARED_WRITE_LOCK ), PageCursor nofault = pf.Io( 0, PF_SHARED_WRITE_LOCK | PF_NO_FAULT ), PageCursor linkedNoFault = nofault.OpenLinkedCursor( 0 ) )
			  {
					VerifyNoFaultAccessToInMemoryPages( faulter, linkedNoFault );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyNoFaultAccessToInMemoryPages(PageCursor faulter, PageCursor nofault) throws java.io.IOException
		 private void VerifyNoFaultAccessToInMemoryPages( PageCursor faulter, PageCursor nofault )
		 {
			  assertTrue( faulter.Next() ); // Page 0 now exists.
			  assertTrue( nofault.Next() ); // NO_FAULT next on page that is in memory.
			  VerifyNoFaultCursorIsInMemory( nofault, 0L ); // Page id must be bound for page that is in memory.

			  assertTrue( faulter.Next() ); // Page 1 now exists.
			  assertTrue( nofault.Next( 1 ) ); // NO_FAULT next with page id on page that is in memory.
			  VerifyNoFaultCursorIsInMemory( nofault, 1L ); // Still bound.
		 }

		 private void VerifyNoFaultCursorIsInMemory( PageCursor nofault, long expectedPageId )
		 {
			  assertThat( nofault.CurrentPageId, @is( expectedPageId ) );
			  nofault.Byte;
			  assertFalse( nofault.CheckAndClearBoundsFlag() ); // Access must not be out of bounds.
			  nofault.GetByte( 0 );
			  assertFalse( nofault.CheckAndClearBoundsFlag() ); // Access must not be out of bounds.
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void noFaultReadOfPagesNotInMemory() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void NoFaultReadOfPagesNotInMemory()
		 {
			  DefaultPageCacheTracer cacheTracer = new DefaultPageCacheTracer();
			  DefaultPageCursorTracerSupplier cursorTracerSupplier = GetCursorTracerSupplier( cacheTracer );
			  getPageCache( fs, maxPages, cacheTracer, cursorTracerSupplier );

			  File file = file( "a" );
			  generateFileWithRecords( file, recordsPerFilePage * 2, recordSize );
			  using ( PagedFile pf = map( file, filePageSize ), PageCursor nofault = pf.Io( 0, PF_SHARED_READ_LOCK | PF_NO_FAULT ) )
			  {
					VerifyNoFaultAccessToPagesNotInMemory( cacheTracer, cursorTracerSupplier, nofault );
			  }
		 }

		 private DefaultPageCursorTracerSupplier GetCursorTracerSupplier( DefaultPageCacheTracer cacheTracer )
		 {
			  DefaultPageCursorTracerSupplier cursorTracerSupplier = DefaultPageCursorTracerSupplier.INSTANCE;
			  // This cursor tracer is thread-local, so we'll initialise it on behalf of this thread.
			  PageCursorTracer cursorTracer = cursorTracerSupplier.Get();
			  // Clear any stray counts that might have been carried over form other tests,
			  // by reporting into a throw-away cache tracer.
			  cursorTracer.Init( new DefaultPageCacheTracer() );
			  cursorTracer.ReportEvents();
			  // Initialize it for real.
			  cursorTracer.Init( cacheTracer );
			  return cursorTracerSupplier;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void noFaultWriteOnPagesNotInMemory() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void NoFaultWriteOnPagesNotInMemory()
		 {
			  DefaultPageCacheTracer cacheTracer = new DefaultPageCacheTracer();
			  DefaultPageCursorTracerSupplier cursorTracerSupplier = GetCursorTracerSupplier( cacheTracer );
			  getPageCache( fs, maxPages, cacheTracer, cursorTracerSupplier );

			  File file = file( "a" );
			  generateFileWithRecords( file, recordsPerFilePage * 2, recordSize );
			  using ( PagedFile pf = map( file, filePageSize ), PageCursor nofault = pf.Io( 0, PF_SHARED_WRITE_LOCK | PF_NO_FAULT ) )
			  {
					VerifyNoFaultAccessToPagesNotInMemory( cacheTracer, cursorTracerSupplier, nofault );
					VerifyNoFaultWriteIsOutOfBounds( nofault );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void noFaultLinkedReadOfPagesNotInMemory() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void NoFaultLinkedReadOfPagesNotInMemory()
		 {
			  DefaultPageCacheTracer cacheTracer = new DefaultPageCacheTracer();
			  DefaultPageCursorTracerSupplier cursorTracerSupplier = GetCursorTracerSupplier( cacheTracer );
			  getPageCache( fs, maxPages, cacheTracer, cursorTracerSupplier );

			  File file = file( "a" );
			  generateFileWithRecords( file, recordsPerFilePage * 2, recordSize );
			  using ( PagedFile pf = map( file, filePageSize ), PageCursor nofault = pf.Io( 0, PF_SHARED_READ_LOCK | PF_NO_FAULT ), PageCursor linkedNoFault = nofault.OpenLinkedCursor( 0 ) )
			  {
					VerifyNoFaultAccessToPagesNotInMemory( cacheTracer, cursorTracerSupplier, linkedNoFault );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void noFaultLinkedWriteOnPagesNotInMemory() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void NoFaultLinkedWriteOnPagesNotInMemory()
		 {
			  DefaultPageCacheTracer cacheTracer = new DefaultPageCacheTracer();
			  DefaultPageCursorTracerSupplier cursorTracerSupplier = GetCursorTracerSupplier( cacheTracer );
			  getPageCache( fs, maxPages, cacheTracer, cursorTracerSupplier );

			  File file = file( "a" );
			  generateFileWithRecords( file, recordsPerFilePage * 2, recordSize );
			  using ( PagedFile pf = map( file, filePageSize ), PageCursor nofault = pf.Io( 0, PF_SHARED_WRITE_LOCK | PF_NO_FAULT ), PageCursor linkedNoFault = nofault.OpenLinkedCursor( 0 ) )
			  {
					VerifyNoFaultAccessToPagesNotInMemory( cacheTracer, cursorTracerSupplier, linkedNoFault );
					VerifyNoFaultWriteIsOutOfBounds( linkedNoFault );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyNoFaultAccessToPagesNotInMemory(org.Neo4Net.io.pagecache.tracing.DefaultPageCacheTracer cacheTracer, org.Neo4Net.io.pagecache.tracing.cursor.DefaultPageCursorTracerSupplier cursorTracerSupplier, PageCursor nofault) throws java.io.IOException
		 private void VerifyNoFaultAccessToPagesNotInMemory( DefaultPageCacheTracer cacheTracer, DefaultPageCursorTracerSupplier cursorTracerSupplier, PageCursor nofault )
		 {
			  assertTrue( nofault.Next() ); // File contains a page id 0.
			  VerifyNoFaultReadIsNotInMemory( nofault ); // But page is not in memory.
			  assertTrue( nofault.Next() ); // File contains a page id 1.
			  VerifyNoFaultReadIsNotInMemory( nofault ); // Also not in memory.
			  assertFalse( nofault.Next() ); // But there is no page id 2.
			  assertTrue( nofault.Next( 0 ) ); // File has page id 0, even when using next with id.
			  VerifyNoFaultReadIsNotInMemory( nofault ); // Still not in memory.
			  assertFalse( nofault.Next( 2 ) ); // Still no page id two, even when using next with id.

			  // Also check that no faults happened.
			  cursorTracerSupplier.Get().reportEvents();
			  assertThat( cacheTracer.Faults(), @is(0L) );
		 }

		 private void VerifyNoFaultReadIsNotInMemory( PageCursor nofault )
		 {
			  assertThat( nofault.CurrentPageId, @is( PageCursor.UNBOUND_PAGE_ID ) );
			  nofault.Byte;
			  assertTrue( nofault.CheckAndClearBoundsFlag() ); // Access must be out of bounds.
			  nofault.GetByte( 0 );
			  assertTrue( nofault.CheckAndClearBoundsFlag() ); // Access must be out of bounds.
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyNoFaultWriteIsOutOfBounds(PageCursor nofault) throws java.io.IOException
		 private void VerifyNoFaultWriteIsOutOfBounds( PageCursor nofault )
		 {
			  assertTrue( nofault.Next( 0 ) );
			  assertThat( nofault.CurrentPageId, @is( PageCursor.UNBOUND_PAGE_ID ) );
			  nofault.PutByte( ( sbyte ) 1 );
			  assertTrue( nofault.CheckAndClearBoundsFlag() ); // Access must be out of bounds.
			  nofault.PutByte( 0, ( sbyte ) 1 );
			  assertTrue( nofault.CheckAndClearBoundsFlag() ); // Access must be out of bounds.
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void noFaultNextReadMustStrideForwardMonotonically() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void NoFaultNextReadMustStrideForwardMonotonically()
		 {
			  configureStandardPageCache();
			  File file = file( "a" );
			  generateFileWithRecords( file, recordsPerFilePage * 6, recordSize );
			  using ( PagedFile pf = map( file, filePageSize ), PageCursor faulter = pf.Io( 0, PF_SHARED_READ_LOCK ), PageCursor nofault = pf.Io( 0, PF_SHARED_READ_LOCK | PF_NO_FAULT ) )
			  {
					assertTrue( faulter.Next( 1 ) );
					assertTrue( faulter.Next( 3 ) );
					assertTrue( faulter.Next( 5 ) );
					assertTrue( nofault.Next() ); // Page id 0.
					VerifyNoFaultReadIsNotInMemory( nofault );
					assertTrue( nofault.Next() ); // Page id 1.
					VerifyNoFaultCursorIsInMemory( nofault, 1 );
					assertTrue( nofault.Next() ); // Page id 2.
					VerifyNoFaultReadIsNotInMemory( nofault );
					assertTrue( nofault.Next() ); // Page id 3.
					VerifyNoFaultCursorIsInMemory( nofault, 3 );
					assertTrue( nofault.Next() ); // Page id 4.
					VerifyNoFaultReadIsNotInMemory( nofault );
					assertTrue( nofault.Next() ); // Page id 5.
					VerifyNoFaultCursorIsInMemory( nofault, 5 );
					assertFalse( nofault.Next() ); // There's no page id 6..
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void noFaultReadCursorMustCopeWithPageEviction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void NoFaultReadCursorMustCopeWithPageEviction()
		 {
			  configureStandardPageCache();
			  File file = file( "a" );
			  using ( PagedFile pf = map( file, filePageSize ), PageCursor faulter = pf.Io( 0, PF_SHARED_WRITE_LOCK ), PageCursor nofault = pf.Io( 0, PF_SHARED_READ_LOCK | PF_NO_FAULT ) )
			  {
					assertTrue( faulter.Next() ); // Page id 0 now exists.
					assertTrue( faulter.Next() ); // And page id 1 now exists.
					assertTrue( nofault.Next() ); // No_FAULT cursor parked on page id 0.
					VerifyNoFaultCursorIsInMemory( nofault, 0 );
					PageCursor[] writerArray = new PageCursor[maxPages - 1]; // The `- 1` to leave our `faulter` cursor.
					for ( int i = 0; i < writerArray.Length; i++ )
					{
						 writerArray[i] = pf.Io( 2 + i, PF_SHARED_WRITE_LOCK );
						 assertTrue( writerArray[i].Next() );
					}
					// The page the nofault cursor is bound to should now be evicted.
					foreach ( PageCursor writer in writerArray )
					{
						 writer.Close();
					}
					// If the page is evicted, then our read must have been inconsistent.
					assertTrue( nofault.ShouldRetry() );
					// However, we are no longer in memory, because the page we had earlier got evicted.
					VerifyNoFaultReadIsNotInMemory( nofault );
			  }
		 }
	}

}