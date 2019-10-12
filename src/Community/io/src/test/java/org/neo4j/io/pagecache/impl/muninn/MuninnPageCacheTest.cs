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
namespace Neo4Net.Io.pagecache.impl.muninn
{
	using MutableBoolean = org.apache.commons.lang3.mutable.MutableBoolean;
	using Test = org.junit.jupiter.api.Test;


	using DelegatingFileSystemAbstraction = Neo4Net.Graphdb.mockfs.DelegatingFileSystemAbstraction;
	using DelegatingStoreChannel = Neo4Net.Graphdb.mockfs.DelegatingStoreChannel;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using Neo4Net.Io.pagecache;
	using Neo4Net.Io.pagecache.tracing;
	using DefaultPageCacheTracer = Neo4Net.Io.pagecache.tracing.DefaultPageCacheTracer;
	using DelegatingPageCacheTracer = Neo4Net.Io.pagecache.tracing.DelegatingPageCacheTracer;
	using EvictionRunEvent = Neo4Net.Io.pagecache.tracing.EvictionRunEvent;
	using MajorFlushEvent = Neo4Net.Io.pagecache.tracing.MajorFlushEvent;
	using PageCacheTracer = Neo4Net.Io.pagecache.tracing.PageCacheTracer;
	using DefaultPageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.DefaultPageCursorTracerSupplier;
	using PageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using VersionContext = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContext;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using RecordingPageCacheTracer = Neo4Net.Io.pagecache.tracing.recording.RecordingPageCacheTracer;
	using RecordingPageCursorTracer = Neo4Net.Io.pagecache.tracing.recording.RecordingPageCursorTracer;
	using Fault = Neo4Net.Io.pagecache.tracing.recording.RecordingPageCursorTracer.Fault;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTimeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.PagedFile_Fields.PF_NO_GROW;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.PagedFile_Fields.PF_SHARED_READ_LOCK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.PagedFile_Fields.PF_SHARED_WRITE_LOCK;
	using static Neo4Net.Io.pagecache.tracing.recording.RecordingPageCacheTracer.Evict;

	public class MuninnPageCacheTest : PageCacheTest<MuninnPageCache>
	{
		 private readonly long _x = unchecked( ( long )0xCAFEBABEDEADBEEFL );
		 private readonly long _y = unchecked( ( long )0xDECAFC0FFEEDECAFL );
		 private MuninnPageCacheFixture _fixture;

		 protected internal override Fixture<MuninnPageCache> CreateFixture()
		 {
			  return _fixture = new MuninnPageCacheFixture();
		 }

		 private PageCacheTracer BlockCacheFlush( PageCacheTracer @delegate )
		 {
			  _fixture.backgroundFlushLatch = new System.Threading.CountdownEvent( 1 );
			  return new DelegatingPageCacheTracerAnonymousInnerClass( this, @delegate );
		 }

		 private class DelegatingPageCacheTracerAnonymousInnerClass : DelegatingPageCacheTracer
		 {
			 private readonly MuninnPageCacheTest _outerInstance;

			 public DelegatingPageCacheTracerAnonymousInnerClass( MuninnPageCacheTest outerInstance, PageCacheTracer @delegate ) : base( @delegate )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override MajorFlushEvent beginCacheFlush()
			 {
				  try
				  {
						_outerInstance.fixture.backgroundFlushLatch.await();
				  }
				  catch ( InterruptedException )
				  {
						Thread.CurrentThread.Interrupt();
				  }
				  return base.beginCacheFlush();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void ableToEvictAllPageInAPageCache() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void AbleToEvictAllPageInAPageCache()
		 {
			  WriteInitialDataTo( file( "a" ) );
			  RecordingPageCacheTracer tracer = new RecordingPageCacheTracer();
			  RecordingPageCursorTracer cursorTracer = new RecordingPageCursorTracer();
			  ConfigurablePageCursorTracerSupplier<RecordingPageCursorTracer> cursorTracerSupplier = new ConfigurablePageCursorTracerSupplier<RecordingPageCursorTracer>( cursorTracer );
			  using ( MuninnPageCache pageCache = createPageCache( fs, 2, BlockCacheFlush( tracer ), cursorTracerSupplier ), PagedFile pagedFile = map( pageCache, file( "a" ), 8 ) )
			  {
					using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
					{
						 assertTrue( cursor.Next() );
					}
					using ( PageCursor cursor = pagedFile.Io( 1, PF_SHARED_READ_LOCK ) )
					{
						 assertTrue( cursor.Next() );
					}
					EvictAllPages( pageCache );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustEvictCleanPageWithoutFlushing() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustEvictCleanPageWithoutFlushing()
		 {
			  WriteInitialDataTo( file( "a" ) );
			  RecordingPageCacheTracer tracer = new RecordingPageCacheTracer();
			  RecordingPageCursorTracer cursorTracer = new RecordingPageCursorTracer();
			  ConfigurablePageCursorTracerSupplier<RecordingPageCursorTracer> cursorTracerSupplier = new ConfigurablePageCursorTracerSupplier<RecordingPageCursorTracer>( cursorTracer );

			  using ( MuninnPageCache pageCache = createPageCache( fs, 2, BlockCacheFlush( tracer ), cursorTracerSupplier ), PagedFile pagedFile = map( pageCache, file( "a" ), 8 ) )
			  {
					using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
					{
						 assertTrue( cursor.Next() );
					}
					cursorTracer.ReportEvents();
					assertNotNull( cursorTracer.Observe( typeof( RecordingPageCursorTracer.Fault ) ) );
					assertEquals( 1, cursorTracer.Faults() );
					assertEquals( 1, tracer.Faults() );

					long clockArm = pageCache.EvictPages( 1, 1, tracer.BeginPageEvictions( 1 ) );
					assertThat( clockArm, @is( 1L ) );
					assertNotNull( tracer.Observe( typeof( Evict ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFlushDirtyPagesOnEvictingFirstPage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFlushDirtyPagesOnEvictingFirstPage()
		 {
			  WriteInitialDataTo( file( "a" ) );
			  RecordingPageCacheTracer tracer = new RecordingPageCacheTracer();
			  RecordingPageCursorTracer cursorTracer = new RecordingPageCursorTracer();
			  ConfigurablePageCursorTracerSupplier<RecordingPageCursorTracer> cursorTracerSupplier = new ConfigurablePageCursorTracerSupplier<RecordingPageCursorTracer>( cursorTracer );

			  using ( MuninnPageCache pageCache = createPageCache( fs, 2, BlockCacheFlush( tracer ), cursorTracerSupplier ), PagedFile pagedFile = map( pageCache, file( "a" ), 8 ) )
			  {

					using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 cursor.PutLong( 0L );
					}
					cursorTracer.ReportEvents();
					assertNotNull( cursorTracer.Observe( typeof( RecordingPageCursorTracer.Fault ) ) );
					assertEquals( 1, cursorTracer.Faults() );
					assertEquals( 1, tracer.Faults() );

					long clockArm = pageCache.EvictPages( 1, 0, tracer.BeginPageEvictions( 1 ) );
					assertThat( clockArm, @is( 1L ) );
					assertNotNull( tracer.Observe( typeof( Evict ) ) );

					ByteBuffer buf = ReadIntoBuffer( "a" );
					assertThat( buf.Long, @is( 0L ) );
					assertThat( buf.Long, @is( _y ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFlushDirtyPagesOnEvictingLastPage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFlushDirtyPagesOnEvictingLastPage()
		 {
			  WriteInitialDataTo( file( "a" ) );
			  RecordingPageCacheTracer tracer = new RecordingPageCacheTracer();
			  RecordingPageCursorTracer cursorTracer = new RecordingPageCursorTracer();
			  ConfigurablePageCursorTracerSupplier<RecordingPageCursorTracer> cursorTracerSupplier = new ConfigurablePageCursorTracerSupplier<RecordingPageCursorTracer>( cursorTracer );

			  using ( MuninnPageCache pageCache = createPageCache( fs, 2, BlockCacheFlush( tracer ), cursorTracerSupplier ), PagedFile pagedFile = map( pageCache, file( "a" ), 8 ) )
			  {
					using ( PageCursor cursor = pagedFile.Io( 1, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 cursor.PutLong( 0L );
					}
					cursorTracer.ReportEvents();
					assertNotNull( cursorTracer.Observe( typeof( RecordingPageCursorTracer.Fault ) ) );
					assertEquals( 1, cursorTracer.Faults() );
					assertEquals( 1, tracer.Faults() );

					long clockArm = pageCache.EvictPages( 1, 0, tracer.BeginPageEvictions( 1 ) );
					assertThat( clockArm, @is( 1L ) );
					assertNotNull( tracer.Observe( typeof( Evict ) ) );

					ByteBuffer buf = ReadIntoBuffer( "a" );
					assertThat( buf.Long, @is( _x ) );
					assertThat( buf.Long, @is( 0L ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustFlushDirtyPagesOnEvictingAllPages() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustFlushDirtyPagesOnEvictingAllPages()
		 {
			  WriteInitialDataTo( file( "a" ) );
			  RecordingPageCacheTracer tracer = new RecordingPageCacheTracer();
			  RecordingPageCursorTracer cursorTracer = new RecordingPageCursorTracer( typeof( RecordingPageCursorTracer.Fault ) );
			  ConfigurablePageCursorTracerSupplier<RecordingPageCursorTracer> cursorTracerSupplier = new ConfigurablePageCursorTracerSupplier<RecordingPageCursorTracer>( cursorTracer );

			  using ( MuninnPageCache pageCache = createPageCache( fs, 4, BlockCacheFlush( tracer ), cursorTracerSupplier ), PagedFile pagedFile = map( pageCache, file( "a" ), 8 ) )
			  {
					using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK | PF_NO_GROW ) )
					{
						 assertTrue( cursor.Next() );
						 cursor.PutLong( 0L );
						 assertTrue( cursor.Next() );
						 cursor.PutLong( 0L );
						 assertFalse( cursor.Next() );
					}
					cursorTracer.ReportEvents();
					assertNotNull( cursorTracer.Observe( typeof( RecordingPageCursorTracer.Fault ) ) );
					assertNotNull( cursorTracer.Observe( typeof( RecordingPageCursorTracer.Fault ) ) );
					assertEquals( 2, cursorTracer.Faults() );
					assertEquals( 2, tracer.Faults() );

					long clockArm = pageCache.EvictPages( 2, 0, tracer.BeginPageEvictions( 2 ) );
					assertThat( clockArm, @is( 2L ) );
					assertNotNull( tracer.Observe( typeof( Evict ) ) );
					assertNotNull( tracer.Observe( typeof( Evict ) ) );

					ByteBuffer buf = ReadIntoBuffer( "a" );
					assertThat( buf.Long, @is( 0L ) );
					assertThat( buf.Long, @is( 0L ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void trackPageModificationTransactionId() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TrackPageModificationTransactionId()
		 {
			  TestVersionContext cursorContext = new TestVersionContext( () => 0 );
			  VersionContextSupplier versionContextSupplier = new ConfiguredVersionContextSupplier( cursorContext );
			  using ( MuninnPageCache pageCache = createPageCache( fs, 2, PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, versionContextSupplier ), PagedFile pagedFile = map( pageCache, file( "a" ), 8 ) )
			  {
					cursorContext.InitWrite( 7 );
					using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 cursor.PutLong( 1 );
					}

					using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 MuninnPageCursor pageCursor = ( MuninnPageCursor ) cursor;
						 assertEquals( 7, pageCursor.PagedFile.getLastModifiedTxId( pageCursor.PinnedPageRef ) );
						 assertEquals( 1, cursor.Long );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pageModificationTrackingNoticeWriteFromAnotherThread() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void PageModificationTrackingNoticeWriteFromAnotherThread()
		 {
			  TestVersionContext cursorContext = new TestVersionContext( () => 0 );
			  VersionContextSupplier versionContextSupplier = new ConfiguredVersionContextSupplier( cursorContext );
			  using ( MuninnPageCache pageCache = createPageCache( fs, 2, PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, versionContextSupplier ), PagedFile pagedFile = map( pageCache, file( "a" ), 8 ) )
			  {
					cursorContext.InitWrite( 7 );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> future = executor.submit(() ->
					Future<object> future = executor.submit(() =>
					{
					 try
					 {
						 using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
						 {
							  assertTrue( cursor.Next() );
							  cursor.PutLong( 1 );
						 }
					 }
					 catch ( IOException e )
					 {
						  throw new Exception( e );
					 }
					});
					future.get();

					using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 MuninnPageCursor pageCursor = ( MuninnPageCursor ) cursor;
						 assertEquals( 7, pageCursor.PagedFile.getLastModifiedTxId( pageCursor.PinnedPageRef ) );
						 assertEquals( 1, cursor.Long );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pageModificationTracksHighestModifierTransactionId() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void PageModificationTracksHighestModifierTransactionId()
		 {
			  TestVersionContext cursorContext = new TestVersionContext( () => 0 );
			  VersionContextSupplier versionContextSupplier = new ConfiguredVersionContextSupplier( cursorContext );
			  using ( MuninnPageCache pageCache = createPageCache( fs, 2, PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, versionContextSupplier ), PagedFile pagedFile = map( pageCache, file( "a" ), 8 ) )
			  {
					cursorContext.InitWrite( 1 );
					using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 cursor.PutLong( 1 );
					}
					cursorContext.InitWrite( 12 );
					using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 cursor.PutLong( 2 );
					}
					cursorContext.InitWrite( 7 );
					using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 cursor.PutLong( 3 );
					}

					using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 MuninnPageCursor pageCursor = ( MuninnPageCursor ) cursor;
						 assertEquals( 12, pageCursor.PagedFile.getLastModifiedTxId( pageCursor.PinnedPageRef ) );
						 assertEquals( 3, cursor.Long );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void markCursorContextDirtyWhenRepositionCursorOnItsCurrentPage() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MarkCursorContextDirtyWhenRepositionCursorOnItsCurrentPage()
		 {
			  TestVersionContext cursorContext = new TestVersionContext( () => 3 );
			  VersionContextSupplier versionContextSupplier = new ConfiguredVersionContextSupplier( cursorContext );
			  using ( MuninnPageCache pageCache = createPageCache( fs, 2, PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, versionContextSupplier ), PagedFile pagedFile = map( pageCache, file( "a" ), 8 ) )
			  {
					cursorContext.InitRead();
					using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursor.Next( 0 ) );
						 assertFalse( cursorContext.Dirty );

						 MuninnPageCursor pageCursor = ( MuninnPageCursor ) cursor;
						 pageCursor.PagedFile.setLastModifiedTxId( ( ( MuninnPageCursor ) cursor ).PinnedPageRef, 17 );

						 assertTrue( cursor.Next( 0 ) );
						 assertTrue( cursorContext.Dirty );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void markCursorContextAsDirtyWhenReadingDataFromMoreRecentTransactions() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MarkCursorContextAsDirtyWhenReadingDataFromMoreRecentTransactions()
		 {
			  TestVersionContext cursorContext = new TestVersionContext( () => 3 );
			  VersionContextSupplier versionContextSupplier = new ConfiguredVersionContextSupplier( cursorContext );
			  using ( MuninnPageCache pageCache = createPageCache( fs, 2, PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, versionContextSupplier ), PagedFile pagedFile = map( pageCache, file( "a" ), 8 ) )
			  {
					cursorContext.InitWrite( 7 );
					using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 cursor.PutLong( 3 );
					}

					cursorContext.InitRead();
					assertFalse( cursorContext.Dirty );
					using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 assertEquals( 3, cursor.Long );
						 assertTrue( cursorContext.Dirty );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void doNotMarkCursorContextAsDirtyWhenReadingDataFromOlderTransactions() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void DoNotMarkCursorContextAsDirtyWhenReadingDataFromOlderTransactions()
		 {
			  TestVersionContext cursorContext = new TestVersionContext( () => 23 );
			  VersionContextSupplier versionContextSupplier = new ConfiguredVersionContextSupplier( cursorContext );
			  using ( MuninnPageCache pageCache = createPageCache( fs, 2, PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, versionContextSupplier ), PagedFile pagedFile = map( pageCache, file( "a" ), 8 ) )
			  {
					cursorContext.InitWrite( 17 );
					using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 cursor.PutLong( 3 );
					}

					cursorContext.InitRead();
					assertFalse( cursorContext.Dirty );
					using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 assertEquals( 3, cursor.Long );
						 assertFalse( cursorContext.Dirty );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void markContextAsDirtyWhenAnyEvictedPageHaveModificationTransactionHigherThenReader() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MarkContextAsDirtyWhenAnyEvictedPageHaveModificationTransactionHigherThenReader()
		 {
			  TestVersionContext cursorContext = new TestVersionContext( () => 5 );
			  VersionContextSupplier versionContextSupplier = new ConfiguredVersionContextSupplier( cursorContext );
			  using ( MuninnPageCache pageCache = createPageCache( fs, 2, PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, versionContextSupplier ), PagedFile pagedFile = map( pageCache, file( "a" ), 8 ) )
			  {
					cursorContext.InitWrite( 3 );
					using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 cursor.PutLong( 3 );
					}

					cursorContext.InitWrite( 13 );
					using ( PageCursor cursor = pagedFile.Io( 1, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 cursor.PutLong( 4 );
					}

					EvictAllPages( pageCache );

					cursorContext.InitRead();
					using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 assertEquals( 3, cursor.Long );
						 assertTrue( cursorContext.Dirty );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void doNotMarkContextAsDirtyWhenAnyEvictedPageHaveModificationTransactionLowerThenReader() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void DoNotMarkContextAsDirtyWhenAnyEvictedPageHaveModificationTransactionLowerThenReader()
		 {
			  TestVersionContext cursorContext = new TestVersionContext( () => 15 );
			  VersionContextSupplier versionContextSupplier = new ConfiguredVersionContextSupplier( cursorContext );
			  using ( MuninnPageCache pageCache = createPageCache( fs, 2, PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, versionContextSupplier ), PagedFile pagedFile = map( pageCache, file( "a" ), 8 ) )
			  {
					cursorContext.InitWrite( 3 );
					using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 cursor.PutLong( 3 );
					}

					cursorContext.InitWrite( 13 );
					using ( PageCursor cursor = pagedFile.Io( 1, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 cursor.PutLong( 4 );
					}

					EvictAllPages( pageCache );

					cursorContext.InitRead();
					using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_READ_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 assertEquals( 3, cursor.Long );
						 assertFalse( cursorContext.Dirty );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void closingTheCursorMustUnlockModifiedPage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ClosingTheCursorMustUnlockModifiedPage()
		 {
			  WriteInitialDataTo( file( "a" ) );

			  using ( MuninnPageCache pageCache = createPageCache( fs, 2, PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null ), PagedFile pagedFile = map( pageCache, file( "a" ), 8 ) )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> task = executor.submit(() ->
					Future<object> task = executor.submit(() =>
					{
					 try
					 {
						 using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
						 {
							  assertTrue( cursor.Next() );
							  cursor.PutLong( 41 );
						 }
					 }
					 catch ( IOException e )
					 {
						  throw new Exception( e );
					 }
					});
					task.get();

					using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
					{
						 assertTrue( cursor.Next() );
						 long value = cursor.Long;
						 cursor.Offset = 0;
						 cursor.PutLong( value + 1 );
					}

					long clockArm = pageCache.EvictPages( 1, 0, EvictionRunEvent.NULL );
					assertThat( clockArm, @is( 1L ) );

					ByteBuffer buf = ReadIntoBuffer( "a" );
					assertThat( buf.Long, @is( 42L ) );
					assertThat( buf.Long, @is( _y ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustUnblockPageFaultersWhenEvictionGetsException()
		 internal virtual void MustUnblockPageFaultersWhenEvictionGetsException()
		 {
			  assertTimeout(ofMillis(SEMI_LONG_TIMEOUT_MILLIS), () =>
			  {
				WriteInitialDataTo( file( "a" ) );

				MutableBoolean throwException = new MutableBoolean( true );
				FileSystemAbstraction fs = new DelegatingFileSystemAbstractionAnonymousInnerClass( this, this.fs, throwException );

				using ( MuninnPageCache pageCache = createPageCache( fs, 2, PageCacheTracer.NULL, PageCursorTracerSupplier.NULL ), PagedFile pagedFile = map( pageCache, file( "a" ), 8 ) )
				{
					 // The basic idea is that this loop, which will encounter a lot of page faults, must not block forever even
					 // though the eviction thread is unable to flush any dirty pages because the file system throws
					 // exceptions on all writes.
					 try
					 {
						 using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
						 {
							  for ( int i = 0; i < 1000; i++ )
							  {
									assertTrue( cursor.Next() );
							  }
							  fail( "Expected an exception at this point" );
						 }
					 }
					 catch ( IOException )
					 {
						  // Good.
					 }

					 throwException.setFalse();
				}
			  });
		 }

		 private class DelegatingFileSystemAbstractionAnonymousInnerClass : DelegatingFileSystemAbstraction
		 {
			 private readonly MuninnPageCacheTest _outerInstance;

			 private MutableBoolean _throwException;

			 public DelegatingFileSystemAbstractionAnonymousInnerClass( MuninnPageCacheTest outerInstance, UnknownType fs, MutableBoolean throwException ) : base( fs )
			 {
				 this.outerInstance = outerInstance;
				 this._throwException = throwException;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.fs.StoreChannel open(java.io.File fileName, org.neo4j.io.fs.OpenMode openMode) throws java.io.IOException
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
					  if ( _outerInstance.throwException.booleanValue() )
					  {
							throw new IOException( "uh-oh..." );
					  }
					  else
					  {
							base.writeAll( src, position );
					  }
				 }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void pageCacheFlushAndForceMustClearBackgroundEvictionException()
		 internal virtual void PageCacheFlushAndForceMustClearBackgroundEvictionException()
		 {
			  assertTimeout(ofMillis(SEMI_LONG_TIMEOUT_MILLIS), () =>
			  {
				MutableBoolean throwException = new MutableBoolean( true );
				FileSystemAbstraction fs = new DelegatingFileSystemAbstractionAnonymousInnerClass2( this, this.fs, throwException );

				using ( MuninnPageCache pageCache = createPageCache( fs, 2, PageCacheTracer.NULL, PageCursorTracerSupplier.NULL ), PagedFile pagedFile = map( pageCache, file( "a" ), 8 ) )
				{
					 using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
					 {
						  assertTrue( cursor.Next() ); // Page 0 is now dirty, but flushing it will throw an exception.
					 }

					 // This will run into that exception, in background eviction:
					 pageCache.EvictPages( 1, 0, EvictionRunEvent.NULL );

					 // We now have a background eviction exception. A successful flushAndForce should clear it, though.
					 throwException.setFalse();
					 pageCache.FlushAndForce();

					 // And with a cleared exception, we should be able to work with the page cache without worry.
					 using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
					 {
						  for ( int i = 0; i < maxPages * 20; i++ )
						  {
								assertTrue( cursor.Next() );
						  }
					 }
				}
			  });
		 }

		 private class DelegatingFileSystemAbstractionAnonymousInnerClass2 : DelegatingFileSystemAbstraction
		 {
			 private readonly MuninnPageCacheTest _outerInstance;

			 private MutableBoolean _throwException;

			 public DelegatingFileSystemAbstractionAnonymousInnerClass2( MuninnPageCacheTest outerInstance, UnknownType fs, MutableBoolean throwException ) : base( fs )
			 {
				 this.outerInstance = outerInstance;
				 this._throwException = throwException;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.fs.StoreChannel open(java.io.File fileName, org.neo4j.io.fs.OpenMode openMode) throws java.io.IOException
			 public override StoreChannel open( File fileName, OpenMode openMode )
			 {
				  return new DelegatingStoreChannelAnonymousInnerClass2( this, base.open( fileName, openMode ) );
			 }

			 private class DelegatingStoreChannelAnonymousInnerClass2 : DelegatingStoreChannel
			 {
				 private readonly DelegatingFileSystemAbstractionAnonymousInnerClass2 _outerInstance;

				 public DelegatingStoreChannelAnonymousInnerClass2( DelegatingFileSystemAbstractionAnonymousInnerClass2 outerInstance, UnknownType open ) : base( open )
				 {
					 this.outerInstance = outerInstance;
				 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeAll(ByteBuffer src, long position) throws java.io.IOException
				 public override void writeAll( ByteBuffer src, long position )
				 {
					  if ( _outerInstance.throwException.booleanValue() )
					  {
							throw new IOException( "uh-oh..." );
					  }
					  else
					  {
							base.writeAll( src, position );
					  }
				 }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustThrowIfMappingFileWouldOverflowReferenceCount()
		 internal virtual void MustThrowIfMappingFileWouldOverflowReferenceCount()
		 {
			  assertTimeout(ofMillis(SEMI_LONG_TIMEOUT_MILLIS), () =>
			  {
				File file = file( "a" );
				WriteInitialDataTo( file );
				using ( MuninnPageCache pageCache = createPageCache( fs, 30, PageCacheTracer.NULL, DefaultPageCursorTracerSupplier.NULL ) )
				{
					 PagedFile pf = null;
					 int i = 0;

					 try
					 {
						  for ( ; i < int.MaxValue; i++ )
						  {
								pf = map( pageCache, file, filePageSize );
						  }
						  fail( "Failure was expected" );
					 }
					 catch ( System.InvalidOperationException )
					 {
						  // expected
					 }
					 finally
					 {
						  for ( int j = 0; j < i; j++ )
						  {
								try
								{
									 pf.Close();
								}
								catch ( Exception e )
								{
									 //noinspection ThrowFromFinallyBlock
									 throw new AssertionError( "Did not expect pf.close() to throw", e );
								}
						  }
					 }
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void unlimitedShouldFlushInParallel()
		 internal virtual void UnlimitedShouldFlushInParallel()
		 {
			  assertTimeout(ofMillis(SEMI_LONG_TIMEOUT_MILLIS), () =>
			  {
				IList<File> mappedFiles = new List<File>();
				mappedFiles.add( existingFile( "a" ) );
				mappedFiles.add( existingFile( "b" ) );
				getPageCache( fs, maxPages, new FlushRendezvousTracer( mappedFiles.size() ), PageCursorTracerSupplier.NULL );

				IList<PagedFile> mappedPagedFiles = new List<PagedFile>();
				foreach ( File mappedFile in mappedFiles )
				{
					 PagedFile pagedFile = map( pageCache, mappedFile, filePageSize );
					 mappedPagedFiles.add( pagedFile );
					 using ( PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
					 {
						  assertTrue( cursor.Next() );
						  cursor.PutInt( 1 );
					 }
				}

				pageCache.flushAndForce( IOLimiter_Fields.Unlimited );

				IOUtils.CloseAll( mappedPagedFiles );
			  });
		 }

		 private class FlushRendezvousTracer : DefaultPageCacheTracer
		 {
			  internal readonly System.Threading.CountdownEvent Latch;

			  internal FlushRendezvousTracer( int fileCountToWaitFor )
			  {
					Latch = new System.Threading.CountdownEvent( fileCountToWaitFor );
			  }

			  public override MajorFlushEvent BeginFileFlush( PageSwapper swapper )
			  {
					Latch.Signal();
					try
					{
						 Latch.await();
					}
					catch ( InterruptedException e )
					{
						 Console.WriteLine( e.ToString() );
						 Console.Write( e.StackTrace );
					}
					return MajorFlushEvent.NULL;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void evictAllPages(MuninnPageCache pageCache) throws java.io.IOException
		 private void EvictAllPages( MuninnPageCache pageCache )
		 {
			  PageList pages = pageCache.Pages;
			  for ( int pageId = 0; pageId < pages.PageCount; pageId++ )
			  {
					long pageReference = pages.Deref( pageId );
					while ( pages.IsLoaded( pageReference ) )
					{
						 pages.TryEvict( pageReference, EvictionRunEvent.NULL );
					}
			  }
			  for ( int pageId = 0; pageId < pages.PageCount; pageId++ )
			  {
					long pageReference = pages.Deref( pageId );
					pageCache.AddFreePageToFreelist( pageReference );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeInitialDataTo(java.io.File file) throws java.io.IOException
		 private void WriteInitialDataTo( File file )
		 {
			  using ( StoreChannel channel = fs.create( file ) )
			  {
					ByteBuffer buf = ByteBuffer.allocate( 16 );
					buf.putLong( _x );
					buf.putLong( _y );
					buf.flip();
					channel.WriteAll( buf );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private ByteBuffer readIntoBuffer(String fileName) throws java.io.IOException
		 private ByteBuffer ReadIntoBuffer( string fileName )
		 {
			  ByteBuffer buffer = ByteBuffer.allocate( 16 );
			  using ( StoreChannel channel = fs.open( file( fileName ), OpenMode.READ ) )
			  {
					channel.ReadAll( buffer );
			  }
			  buffer.flip();
			  return buffer;
		 }

		 private class ConfiguredVersionContextSupplier : VersionContextSupplier
		 {

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly VersionContext VersionContextConflict;

			  internal ConfiguredVersionContextSupplier( VersionContext versionContext )
			  {
					this.VersionContextConflict = versionContext;
			  }

			  public override void Init( System.Func<long> lastClosedTransactionIdSupplier )
			  {
			  }

			  public virtual VersionContext VersionContext
			  {
				  get
				  {
						return VersionContextConflict;
				  }
			  }
		 }

		 private class TestVersionContext : VersionContext
		 {

			  internal readonly System.Func<int> ClosedTxIdSupplier;
			  internal long CommittingTxId;
			  internal long LastClosedTxId;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool DirtyConflict;

			  internal TestVersionContext( System.Func<int> closedTxIdSupplier )
			  {
					this.ClosedTxIdSupplier = closedTxIdSupplier;
			  }

			  public override void InitRead()
			  {
					this.LastClosedTxId = ClosedTxIdSupplier.AsInt;
			  }

			  public override void InitWrite( long committingTxId )
			  {
					this.CommittingTxId = committingTxId;
			  }

			  public override long CommittingTransactionId()
			  {
					return CommittingTxId;
			  }

			  public override long LastClosedTransactionId()
			  {
					return LastClosedTxId;
			  }

			  public override void MarkAsDirty()
			  {
					DirtyConflict = true;
			  }

			  public virtual bool Dirty
			  {
				  get
				  {
						return DirtyConflict;
				  }
			  }
		 }
	}

}