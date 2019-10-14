using System;
using System.Collections.Generic;
using System.Diagnostics;

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
	using MutableBoolean = org.apache.commons.lang3.mutable.MutableBoolean;
	using MutableLong = org.apache.commons.lang3.mutable.MutableLong;
	using Pair = org.apache.commons.lang3.tuple.Pair;
	using CoreMatchers = org.hamcrest.CoreMatchers;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using Neo4Net.Cursors;
	using Neo4Net.Functions;
	using EphemeralFileSystemAbstraction = Neo4Net.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using Monitor = Neo4Net.Index.Internal.gbptree.GBPTree.Monitor;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using DelegatingPageCache = Neo4Net.Io.pagecache.DelegatingPageCache;
	using DelegatingPagedFile = Neo4Net.Io.pagecache.DelegatingPagedFile;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using PageSwapper = Neo4Net.Io.pagecache.PageSwapper;
	using PagedFile = Neo4Net.Io.pagecache.PagedFile;
	using FileIsNotMappedException = Neo4Net.Io.pagecache.impl.FileIsNotMappedException;
	using PinEvent = Neo4Net.Io.pagecache.tracing.PinEvent;
	using DefaultPageCursorTracer = Neo4Net.Io.pagecache.tracing.cursor.DefaultPageCursorTracer;
	using PageCursorTracer = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracer;
	using PageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using Barrier = Neo4Net.Test.Barrier;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.MAX_VALUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.AllOf.allOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.rules.RuleChain.outerRule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.GBPTree.NO_HEADER_READER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.SimpleLongLayout.longLayout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.Internal.gbptree.ThrowingRunnable.throwing;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.IOLimiter_Fields.UNLIMITED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.PagedFile_Fields.PF_SHARED_WRITE_LOCK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.PageCacheRule.config;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("EmptyTryBlock") public class GBPTreeTest
	public class GBPTreeTest
	{
		private bool InstanceFieldsInitialized = false;

		public GBPTreeTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_directory = TestDirectory.testDirectory( this.GetType(), _fs.get() );
			Rules = outerRule( _fs ).around( _directory ).around( _pageCacheRule ).around( _random );
		}

		 private const int DEFAULT_PAGE_SIZE = 256;

		 private static readonly Layout<MutableLong, MutableLong> _layout = longLayout().build();

		 private readonly DefaultFileSystemRule _fs = new DefaultFileSystemRule();
		 private TestDirectory _directory;
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule( config().withAccessChecks(true) );
		 private readonly RandomRule _random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain rules = outerRule(fs).around(directory).around(pageCacheRule).around(random);
		 public RuleChain Rules;

		 private File _indexFile;
		 private ExecutorService _executor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _executor = Executors.newFixedThreadPool( Runtime.Runtime.availableProcessors() );
			  _indexFile = _directory.file( "index" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void teardown()
		 public virtual void Teardown()
		 {
			  _executor.shutdown();
		 }

		 /* Meta and state page tests */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadWrittenMetaData() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadWrittenMetaData()
		 {
			  // GIVEN
			  using ( GBPTree<MutableLong, MutableLong> ignored = Index().build() )
			  { // open/close is enough
			  }

			  // WHEN
			  using ( GBPTree<MutableLong, MutableLong> ignored = Index().build() )
			  { // open/close is enough
			  }

			  // THEN being able to open validates that the same meta data was read
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToOpenOnDifferentMetaData() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToOpenOnDifferentMetaData()
		 {
			  // GIVEN
			  using ( GBPTree<MutableLong, MutableLong> ignored = Index( 1024 ).build() )
			  { // Open/close is enough
			  }

			  // WHEN
			  SimpleLongLayout otherLayout = longLayout().withCustomerNameAsMetaData("Something else").build();
			  try
			  {
					  using ( GBPTree<MutableLong, MutableLong> ignored = Index().with(otherLayout).build() )
					  {
						fail( "Should not load" );
					  }
			  }
			  catch ( MetadataMismatchException )
			  {
					// THEN good
			  }

			  // THEN being able to open validates that the same meta data was read
			  // the test also closes the index afterwards
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToOpenOnDifferentLayout() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToOpenOnDifferentLayout()
		 {
			  // GIVEN
			  using ( GBPTree<MutableLong, MutableLong> ignored = Index().build() )
			  { // Open/close is enough
			  }

			  // WHEN
			  SimpleLongLayout otherLayout = longLayout().withIdentifier(123456).build();
			  try
			  {
					  using ( GBPTree<MutableLong, MutableLong> ignored = Index().with(otherLayout).build() )
					  {
						fail( "Should not load" );
					  }
			  }
			  catch ( MetadataMismatchException )
			  {
					// THEN good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToOpenOnDifferentMajorVersion() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToOpenOnDifferentMajorVersion()
		 {
			  // GIVEN
			  using ( GBPTree<MutableLong, MutableLong> ignored = Index( 1024 ).build() )
			  { // Open/close is enough
			  }

			  // WHEN
			  SimpleLongLayout otherLayout = longLayout().withMajorVersion(123).build();
			  try
			  {
					  using ( GBPTree<MutableLong, MutableLong> ignored = Index().with(otherLayout).build() )
					  {
						fail( "Should not load" );
					  }
			  }
			  catch ( MetadataMismatchException )
			  {
					// THEN good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToOpenOnDifferentMinorVersion() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToOpenOnDifferentMinorVersion()
		 {
			  // GIVEN
			  using ( GBPTree<MutableLong, MutableLong> ignored = Index().build() )
			  { // Open/close is enough
			  }

			  // WHEN
			  SimpleLongLayout otherLayout = longLayout().withMinorVersion(123).build();
			  try
			  {
					  using ( GBPTree<MutableLong, MutableLong> ignored = Index().with(otherLayout).build() )
					  {
						fail( "Should not load" );
					  }
			  }
			  catch ( MetadataMismatchException )
			  {
					// THEN good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnOpenWithDifferentPageSize() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailOnOpenWithDifferentPageSize()
		 {
			  // GIVEN
			  int pageSize = 1024;
			  using ( GBPTree<MutableLong, MutableLong> ignored = Index( pageSize ).build() )
			  { // Open/close is enough
			  }

			  // WHEN
			  try
			  {
					  using ( GBPTree<MutableLong, MutableLong> ignored = Index( pageSize / 2 ).build() )
					  {
						fail( "Should not load" );
					  }
			  }
			  catch ( MetadataMismatchException e )
			  {
					// THEN good
					assertThat( e.Message, containsString( "page size" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnStartingWithPageSizeLargerThanThatOfPageCache() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailOnStartingWithPageSizeLargerThanThatOfPageCache()
		 {
			  // WHEN
			  int pageCachePageSize = 512;
			  try
			  {
					  using ( GBPTree<MutableLong, MutableLong> ignored = Index( pageCachePageSize ).withIndexPageSize( 2 * pageCachePageSize ).build() )
					  {
						fail( "Shouldn't have been created" );
					  }
			  }
			  catch ( MetadataMismatchException e )
			  {
					// THEN good
					assertThat( e.Message, containsString( "page size" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMapIndexFileWithProvidedPageSizeIfLessThanOrEqualToCachePageSize() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMapIndexFileWithProvidedPageSizeIfLessThanOrEqualToCachePageSize()
		 {
			  // WHEN
			  int pageCachePageSize = 1024;
			  using ( GBPTree<MutableLong, MutableLong> ignored = Index( pageCachePageSize ).withIndexPageSize( pageCachePageSize / 2 ).build() )
			  {
					// Good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenTryingToRemapWithPageSizeLargerThanCachePageSize() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenTryingToRemapWithPageSizeLargerThanCachePageSize()
		 {
			  // WHEN
			  int pageCachePageSize = 1024;
			  using ( GBPTree<MutableLong, MutableLong> ignored = Index( pageCachePageSize ).build() )
			  {
					// Good
			  }

			  try
			  {
					  using ( GBPTree<MutableLong, MutableLong> ignored = Index( pageCachePageSize / 2 ).withIndexPageSize( pageCachePageSize ).build() )
					  {
						fail( "Expected to fail" );
					  }
			  }
			  catch ( MetadataMismatchException e )
			  {
					// THEN Good
					assertThat( e.Message, containsString( "page size" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemapFileIfMappedWithPageSizeLargerThanCreationSize() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemapFileIfMappedWithPageSizeLargerThanCreationSize()
		 {
			  // WHEN
			  int pageSize = 1024;
			  IList<long> expectedData = new List<long>();
			  for ( long i = 0; i < 100; i++ )
			  {
					expectedData.Add( i );
			  }
			  using ( GBPTree<MutableLong, MutableLong> index = index( pageSize ).withIndexPageSize( pageSize / 2 ).Build() )
			  {
					// Insert some data
					using ( Writer<MutableLong, MutableLong> writer = index.Writer() )
					{
						 MutableLong key = new MutableLong();
						 MutableLong value = new MutableLong();

						 foreach ( long? insert in expectedData )
						 {
							  key.Value = insert;
							  value.Value = insert;
							  writer.Put( key, value );
						 }
					}
					index.Checkpoint( UNLIMITED );
			  }

			  // THEN
			  using ( GBPTree<MutableLong, MutableLong> index = index( pageSize ).build() )
			  {
					MutableLong fromInclusive = new MutableLong( 0L );
					MutableLong toExclusive = new MutableLong( 200L );
					using ( RawCursor<Hit<MutableLong, MutableLong>, IOException> seek = index.Seek( fromInclusive, toExclusive ) )
					{
						 int i = 0;
						 while ( seek.Next() )
						 {
							  Hit<MutableLong, MutableLong> hit = seek.get();
							  assertEquals( hit.Key().Value, expectedData[i] );
							  assertEquals( hit.Value().Value, expectedData[i] );
							  i++;
						 }
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWhenTryingToOpenWithDifferentFormatIdentifier() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWhenTryingToOpenWithDifferentFormatIdentifier()
		 {
			  // GIVEN
			  PageCache pageCache = CreatePageCache( DEFAULT_PAGE_SIZE );
			  GBPTreeBuilder<MutableLong, MutableLong> builder = Index( pageCache );
			  using ( GBPTree<MutableLong, MutableLong> ignored = builder.Build() )
			  { // Open/close is enough
			  }

			  try
			  {
					// WHEN
					builder.With( longLayout().withFixedSize(false).build() ).build();
					fail( "Should have failed" );
			  }
			  catch ( MetadataMismatchException )
			  {
					// THEN good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnNoResultsOnEmptyIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnNoResultsOnEmptyIndex()
		 {
			  // GIVEN
			  using ( GBPTree<MutableLong, MutableLong> index = index().build() )
			  {

					// WHEN
					RawCursor<Hit<MutableLong, MutableLong>, IOException> result = index.Seek( new MutableLong( 0 ), new MutableLong( 10 ) );

					// THEN
					assertFalse( result.Next() );
			  }
		 }

		 /* Lifecycle tests */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToAcquireModifierTwice() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBeAbleToAcquireModifierTwice()
		 {
			  // GIVEN
			  using ( GBPTree<MutableLong, MutableLong> index = index().build() )
			  {
					Writer<MutableLong, MutableLong> writer = index.Writer();

					// WHEN
					try
					{
						 index.Writer();
						 fail( "Should have failed" );
					}
					catch ( System.InvalidOperationException )
					{
						 // THEN good
					}

					// Should be able to close old writer
					writer.Dispose();
					// And open and closing a new one
					index.Writer().close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowClosingWriterMultipleTimes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowClosingWriterMultipleTimes()
		 {
			  // GIVEN
			  using ( GBPTree<MutableLong, MutableLong> index = index().build() )
			  {
					Writer<MutableLong, MutableLong> writer = index.Writer();
					writer.Put( new MutableLong( 0 ), new MutableLong( 1 ) );
					writer.Dispose();

					try
					{
						 // WHEN
						 writer.Dispose();
						 fail( "Should have failed" );
					}
					catch ( System.InvalidOperationException e )
					{
						 // THEN
						 assertThat( e.Message, containsString( "already closed" ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failureDuringInitializeWriterShouldNotFailNextInitialize() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailureDuringInitializeWriterShouldNotFailNextInitialize()
		 {
			  // GIVEN
			  IOException no = new IOException( "No" );
			  AtomicBoolean throwOnNextIO = new AtomicBoolean();
			  PageCache controlledPageCache = PageCacheThatThrowExceptionWhenToldTo( no, throwOnNextIO );
			  using ( GBPTree<MutableLong, MutableLong> index = index( controlledPageCache ).build() )
			  {
					// WHEN
					assertTrue( throwOnNextIO.compareAndSet( false, true ) );
					try
					{
							using ( Writer<MutableLong, MutableLong> ignored = index.Writer() )
							{
							 fail( "Expected to throw" );
							}
					}
					catch ( IOException e )
					{
						 assertSame( no, e );
					}

					// THEN
					using ( Writer<MutableLong, MutableLong> writer = index.Writer() )
					{
						 writer.Put( new MutableLong( 1 ), new MutableLong( 1 ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowClosingTreeMultipleTimes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowClosingTreeMultipleTimes()
		 {
			  // GIVEN
			  GBPTree<MutableLong, MutableLong> index = index().build();

			  // WHEN
			  index.Dispose();

			  // THEN
			  index.Dispose(); // should be OK
		 }

		 /* Header test */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPutHeaderDataInCheckPoint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPutHeaderDataInCheckPoint()
		 {
			  System.Action<GBPTree<MutableLong, MutableLong>, sbyte[]> beforeClose = ( index, expected ) =>
			  {
				ThrowingRunnable throwingRunnable = () => index.checkpoint(UNLIMITED, cursor => cursor.putBytes(expected));
				ThrowingRunnable.throwing( throwingRunnable ).run();
			  };
			  VerifyHeaderDataAfterClose( beforeClose );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCarryOverHeaderDataInCheckPoint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCarryOverHeaderDataInCheckPoint()
		 {
			  System.Action<GBPTree<MutableLong, MutableLong>, sbyte[]> beforeClose = ( index, expected ) =>
			  {
				ThrowingRunnable throwingRunnable = () =>
				{
					 index.checkpoint( UNLIMITED, cursor => cursor.putBytes( expected ) );
					 Insert( index, 0, 1 );

					 // WHEN
					 // Should carry over header data
					 index.checkpoint( UNLIMITED );
				};
				ThrowingRunnable.throwing( throwingRunnable ).run();
			  };
			  VerifyHeaderDataAfterClose( beforeClose );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCarryOverHeaderDataOnDirtyClose() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCarryOverHeaderDataOnDirtyClose()
		 {
			  System.Action<GBPTree<MutableLong, MutableLong>, sbyte[]> beforeClose = ( index, expected ) =>
			  {
				ThrowingRunnable throwingRunnable = () =>
				{
					 index.checkpoint( UNLIMITED, cursor => cursor.putBytes( expected ) );
					 Insert( index, 0, 1 );

					 // No checkpoint
				};
				ThrowingRunnable.throwing( throwingRunnable ).run();
			  };
			  VerifyHeaderDataAfterClose( beforeClose );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReplaceHeaderDataInNextCheckPoint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReplaceHeaderDataInNextCheckPoint()
		 {
			  System.Action<GBPTree<MutableLong, MutableLong>, sbyte[]> beforeClose = ( index, expected ) =>
			  {
				ThrowingRunnable throwingRunnable = () =>
				{
					 index.checkpoint( UNLIMITED, cursor => cursor.putBytes( expected ) );
					 ThreadLocalRandom.current().NextBytes(expected);
					 index.checkpoint( UNLIMITED, cursor => cursor.putBytes( expected ) );
				};
				ThrowingRunnable.throwing( throwingRunnable ).run();
			  };

			  VerifyHeaderDataAfterClose( beforeClose );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustWriteHeaderOnInitialization() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustWriteHeaderOnInitialization()
		 {
			  // GIVEN
			  sbyte[] headerBytes = new sbyte[12];
			  ThreadLocalRandom.current().NextBytes(headerBytes);
			  System.Action<PageCursor> headerWriter = pc => pc.putBytes( headerBytes );

			  // WHEN
			  PageCache pageCache = CreatePageCache( DEFAULT_PAGE_SIZE );
			  using ( GBPTree<MutableLong, MutableLong> ignore = Index( pageCache ).with( headerWriter ).build() )
			  {
			  }

			  // THEN
			  VerifyHeader( pageCache, headerBytes );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustNotOverwriteHeaderOnExistingTree() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustNotOverwriteHeaderOnExistingTree()
		 {
			  // GIVEN
			  sbyte[] expectedBytes = new sbyte[12];
			  ThreadLocalRandom.current().NextBytes(expectedBytes);
			  System.Action<PageCursor> headerWriter = pc => pc.putBytes( expectedBytes );
			  PageCache pageCache = CreatePageCache( DEFAULT_PAGE_SIZE );
			  using ( GBPTree<MutableLong, MutableLong> ignore = Index( pageCache ).with( headerWriter ).build() )
			  {
			  }

			  // WHEN
			  sbyte[] fraudulentBytes = new sbyte[12];
			  do
			  {
					ThreadLocalRandom.current().NextBytes(fraudulentBytes);
			  } while ( Arrays.Equals( expectedBytes, fraudulentBytes ) );

			  using ( GBPTree<MutableLong, MutableLong> ignore = Index( pageCache ).with( headerWriter ).build() )
			  {
			  }

			  // THEN
			  VerifyHeader( pageCache, expectedBytes );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyHeaderDataAfterClose(System.Action<GBPTree<org.apache.commons.lang3.mutable.MutableLong,org.apache.commons.lang3.mutable.MutableLong>,byte[]> beforeClose) throws java.io.IOException
		 private void VerifyHeaderDataAfterClose( System.Action<GBPTree<MutableLong, MutableLong>, sbyte[]> beforeClose )
		 {
			  sbyte[] expectedHeader = new sbyte[12];
			  ThreadLocalRandom.current().NextBytes(expectedHeader);
			  PageCache pageCache = CreatePageCache( DEFAULT_PAGE_SIZE );

			  // GIVEN
			  using ( GBPTree<MutableLong, MutableLong> index = index( pageCache ).build() )
			  {
					beforeClose( index, expectedHeader );
			  }

			  VerifyHeader( pageCache, expectedHeader );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 10_000) public void writeHeaderInDirtyTreeMustNotDeadlock() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void WriteHeaderInDirtyTreeMustNotDeadlock()
		 {
			  PageCache pageCache = CreatePageCache( 256 );
			  MakeDirty( pageCache );

			  System.Action<PageCursor> headerWriter = pc => pc.putBytes( "failed".GetBytes() );
			  using ( GBPTree<MutableLong, MutableLong> index = index( pageCache ).with( RecoveryCleanupWorkCollector.Ignore() ).Build() )
			  {
					index.Checkpoint( UNLIMITED, headerWriter );
			  }

			  VerifyHeader( pageCache, "failed".GetBytes() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyHeader(org.neo4j.io.pagecache.PageCache pageCache, byte[] expectedHeader) throws java.io.IOException
		 private void VerifyHeader( PageCache pageCache, sbyte[] expectedHeader )
		 {
			  // WHEN
			  sbyte[] readHeader = new sbyte[expectedHeader.Length];
			  AtomicInteger length = new AtomicInteger();
			  Header.Reader headerReader = headerData =>
			  {
				length.set( headerData.limit() );
				headerData.get( readHeader );
			  };

			  // Read as part of construction
			  using ( GBPTree<MutableLong, MutableLong> ignored = Index( pageCache ).with( headerReader ).build() )
			  { // open/close is enough to read header
			  }

			  // THEN
			  assertEquals( expectedHeader.Length, length.get() );
			  assertArrayEquals( expectedHeader, readHeader );

			  // WHEN
			  // Read separate
			  GBPTree.ReadHeader( pageCache, _indexFile, headerReader );

			  assertEquals( expectedHeader.Length, length.get() );
			  assertArrayEquals( expectedHeader, readHeader );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readHeaderMustThrowIfFileDoesNotExist() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadHeaderMustThrowIfFileDoesNotExist()
		 {
			  // given
			  File doesNotExist = new File( "Does not exist" );
			  try
			  {
					GBPTree.ReadHeader( CreatePageCache( DEFAULT_PAGE_SIZE ), doesNotExist, NO_HEADER_READER );
					fail( "Should have failed" );
			  }
			  catch ( NoSuchFileException )
			  {
					// good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void openWithReadHeaderMustThrowMetadataMismatchExceptionIfFileIsEmpty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void OpenWithReadHeaderMustThrowMetadataMismatchExceptionIfFileIsEmpty()
		 {
			  OpenMustThrowMetadataMismatchExceptionIfFileIsEmpty( pageCache => GBPTree.readHeader( pageCache, _indexFile, NO_HEADER_READER ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void openWithConstructorMustThrowMetadataMismatchExceptionIfFileIsEmpty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void OpenWithConstructorMustThrowMetadataMismatchExceptionIfFileIsEmpty()
		 {
			  OpenMustThrowMetadataMismatchExceptionIfFileIsEmpty( pageCache => index( pageCache ).Build() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void openMustThrowMetadataMismatchExceptionIfFileIsEmpty(org.neo4j.function.ThrowingConsumer<org.neo4j.io.pagecache.PageCache,java.io.IOException> opener) throws Exception
		 private void OpenMustThrowMetadataMismatchExceptionIfFileIsEmpty( ThrowingConsumer<PageCache, IOException> opener )
		 {
			  // given an existing empty file
			  PageCache pageCache = CreatePageCache( DEFAULT_PAGE_SIZE );
			  pageCache.Map( _indexFile, pageCache.PageSize(), StandardOpenOption.CREATE ).close();

			  // when
			  try
			  {
					opener.Accept( pageCache );
					fail( "Should've thrown IOException" );
			  }
			  catch ( MetadataMismatchException )
			  {
					// then good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readHeaderMustThrowMetadataMismatchExceptionIfSomeMetaPageIsMissing() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadHeaderMustThrowMetadataMismatchExceptionIfSomeMetaPageIsMissing()
		 {
			  OpenMustThrowMetadataMismatchExceptionIfSomeMetaPageIsMissing( pageCache => GBPTree.readHeader( pageCache, _indexFile, NO_HEADER_READER ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void constructorMustThrowMetadataMismatchExceptionIfSomeMetaPageIsMissing() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ConstructorMustThrowMetadataMismatchExceptionIfSomeMetaPageIsMissing()
		 {
			  OpenMustThrowMetadataMismatchExceptionIfSomeMetaPageIsMissing( pageCache => index( pageCache ).Build() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void openMustThrowMetadataMismatchExceptionIfSomeMetaPageIsMissing(org.neo4j.function.ThrowingConsumer<org.neo4j.io.pagecache.PageCache,java.io.IOException> opener) throws Exception
		 private void OpenMustThrowMetadataMismatchExceptionIfSomeMetaPageIsMissing( ThrowingConsumer<PageCache, IOException> opener )
		 {
			  // given an existing index with only the first page in it
			  PageCache pageCache = CreatePageCache( DEFAULT_PAGE_SIZE );
			  using ( GBPTree<MutableLong, MutableLong> ignored = Index( pageCache ).build() )
			  { // Just for creating it
			  }
			  _fs.truncate( _indexFile, DEFAULT_PAGE_SIZE );

			  // when
			  try
			  {
					opener.Accept( pageCache );
					fail( "Should've thrown IOException" );
			  }
			  catch ( MetadataMismatchException )
			  {
					// then good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readHeaderMustThrowIOExceptionIfStatePagesAreAllZeros() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadHeaderMustThrowIOExceptionIfStatePagesAreAllZeros()
		 {
			  OpenMustThrowMetadataMismatchExceptionIfStatePagesAreAllZeros( pageCache => GBPTree.readHeader( pageCache, _indexFile, NO_HEADER_READER ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void constructorMustThrowMetadataMismatchExceptionIfStatePagesAreAllZeros() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ConstructorMustThrowMetadataMismatchExceptionIfStatePagesAreAllZeros()
		 {
			  OpenMustThrowMetadataMismatchExceptionIfStatePagesAreAllZeros( pageCache => index( pageCache ).Build() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void openMustThrowMetadataMismatchExceptionIfStatePagesAreAllZeros(org.neo4j.function.ThrowingConsumer<org.neo4j.io.pagecache.PageCache,java.io.IOException> opener) throws Exception
		 private void OpenMustThrowMetadataMismatchExceptionIfStatePagesAreAllZeros( ThrowingConsumer<PageCache, IOException> opener )
		 {
			  // given an existing index with all-zero state pages
			  PageCache pageCache = CreatePageCache( DEFAULT_PAGE_SIZE );
			  using ( GBPTree<MutableLong, MutableLong> ignored = Index( pageCache ).build() )
			  { // Just for creating it
			  }
			  _fs.truncate( _indexFile, DEFAULT_PAGE_SIZE );
			  using ( Stream @out = _fs.openAsOutputStream( _indexFile, true ) )
			  {
					sbyte[] allZeroPage = new sbyte[DEFAULT_PAGE_SIZE];
					@out.Write( allZeroPage, 0, allZeroPage.Length ); // page A
					@out.Write( allZeroPage, 0, allZeroPage.Length ); // page B
			  }

			  // when
			  try
			  {
					opener.Accept( pageCache );
					fail( "Should've thrown IOException" );
			  }
			  catch ( MetadataMismatchException )
			  {
					// then good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readHeaderMustWorkWithOpenIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadHeaderMustWorkWithOpenIndex()
		 {
			  // GIVEN
			  sbyte[] headerBytes = new sbyte[12];
			  ThreadLocalRandom.current().NextBytes(headerBytes);
			  System.Action<PageCursor> headerWriter = pc => pc.putBytes( headerBytes );
			  PageCache pageCache = CreatePageCache( DEFAULT_PAGE_SIZE );

			  // WHEN
			  using ( GBPTree<MutableLong, MutableLong> ignore = Index( pageCache ).with( headerWriter ).build() )
			  {
					sbyte[] readHeader = new sbyte[headerBytes.Length];
					AtomicInteger length = new AtomicInteger();
					Header.Reader headerReader = headerData =>
					{
					 length.set( headerData.limit() );
					 headerData.get( readHeader );
					};
					GBPTree.ReadHeader( pageCache, _indexFile, headerReader );

					// THEN
					assertEquals( headerBytes.Length, length.get() );
					assertArrayEquals( headerBytes, readHeader );
			  }
		 }

		 /* Mutex tests */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 5_000L) public void checkPointShouldLockOutWriter() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CheckPointShouldLockOutWriter()
		 {
			  // GIVEN
			  CheckpointControlledMonitor monitor = new CheckpointControlledMonitor();
			  using ( GBPTree<MutableLong, MutableLong> index = index().with(monitor).Build() )
			  {
					long key = 10;
					using ( Writer<MutableLong, MutableLong> writer = index.Writer() )
					{
						 writer.Put( new MutableLong( key ), new MutableLong( key ) );
					}

					// WHEN
					monitor.Enabled = true;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> checkpoint = executor.submit(throwing(() -> index.checkpoint(UNLIMITED)));
					Future<object> checkpoint = _executor.submit( throwing( () => index.checkpoint(UNLIMITED) ) );
					monitor.Barrier.awaitUninterruptibly();
					// now we're in the smack middle of a checkpoint
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> writerClose = executor.submit(throwing(() -> index.writer().close()));
					Future<object> writerClose = _executor.submit( throwing( () => index.Writer().close() ) );

					// THEN
					ShouldWait( writerClose );
					monitor.Barrier.release();

					writerClose.get();
					checkpoint.get();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 5_000L) public void checkPointShouldWaitForWriter() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CheckPointShouldWaitForWriter()
		 {
			  // GIVEN
			  using ( GBPTree<MutableLong, MutableLong> index = index().build() )
			  {
					// WHEN
					Neo4Net.Test.Barrier_Control barrier = new Neo4Net.Test.Barrier_Control();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> write = executor.submit(throwing(() ->
					Future<object> write = _executor.submit(throwing(() =>
					{
					 using ( Writer<MutableLong, MutableLong> writer = index.Writer() )
					 {
						  writer.Put( new MutableLong( 1 ), new MutableLong( 1 ) );
						  barrier.Reached();
					 }
					}));
					barrier.AwaitUninterruptibly();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> checkpoint = executor.submit(throwing(() -> index.checkpoint(UNLIMITED)));
					Future<object> checkpoint = _executor.submit( throwing( () => index.checkpoint(UNLIMITED) ) );
					ShouldWait( checkpoint );

					// THEN
					barrier.Release();
					checkpoint.get();
					write.get();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 50_000L) public void closeShouldLockOutWriter() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CloseShouldLockOutWriter()
		 {
			  // GIVEN
			  AtomicBoolean enabled = new AtomicBoolean();
			  Neo4Net.Test.Barrier_Control barrier = new Neo4Net.Test.Barrier_Control();
			  PageCache pageCacheWithBarrier = PageCacheWithBarrierInClose( enabled, barrier );
			  GBPTree<MutableLong, MutableLong> index = index( pageCacheWithBarrier ).build();
			  long key = 10;
			  using ( Writer<MutableLong, MutableLong> writer = index.Writer() )
			  {
					writer.Put( new MutableLong( key ), new MutableLong( key ) );
			  }

			  // WHEN
			  enabled.set( true );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> close = executor.submit(throwing(index::close));
			  Future<object> close = _executor.submit( throwing( index.close ) );
			  barrier.AwaitUninterruptibly();
			  // now we're in the smack middle of a close/checkpoint
			  AtomicReference<Exception> writerError = new AtomicReference<Exception>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> write = executor.submit(() ->
			  Future<object> write = _executor.submit(() =>
			  {
				try
				{
					 index.Writer().close();
				}
				catch ( Exception e )
				{
					 writerError.set( e );
				}
			  });

			  ShouldWait( write );
			  barrier.Release();

			  // THEN
			  write.get();
			  close.get();
			  assertTrue( "Writer should not be able to acquired after close", writerError.get() is FileIsNotMappedException );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.neo4j.io.pagecache.PageCache pageCacheWithBarrierInClose(final java.util.concurrent.atomic.AtomicBoolean enabled, final org.neo4j.test.Barrier_Control barrier)
		 private PageCache PageCacheWithBarrierInClose( AtomicBoolean enabled, Neo4Net.Test.Barrier_Control barrier )
		 {
			  return new DelegatingPageCacheAnonymousInnerClass( this, CreatePageCache( 1024 ), enabled, barrier );
		 }

		 private class DelegatingPageCacheAnonymousInnerClass : DelegatingPageCache
		 {
			 private readonly GBPTreeTest _outerInstance;

			 private AtomicBoolean _enabled;
			 private Neo4Net.Test.Barrier_Control _barrier;

			 public DelegatingPageCacheAnonymousInnerClass( GBPTreeTest outerInstance, PageCache createPageCache, AtomicBoolean enabled, Neo4Net.Test.Barrier_Control barrier ) : base( createPageCache )
			 {
				 this.outerInstance = outerInstance;
				 this._enabled = enabled;
				 this._barrier = barrier;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.pagecache.PagedFile map(java.io.File file, int pageSize, java.nio.file.OpenOption... openOptions) throws java.io.IOException
			 public override PagedFile map( File file, int pageSize, params OpenOption[] openOptions )
			 {
				  return new DelegatingPagedFileAnonymousInnerClass( this, base.map( file, pageSize, openOptions ) );
			 }

			 private class DelegatingPagedFileAnonymousInnerClass : DelegatingPagedFile
			 {
				 private readonly DelegatingPageCacheAnonymousInnerClass _outerInstance;

				 public DelegatingPagedFileAnonymousInnerClass( DelegatingPageCacheAnonymousInnerClass outerInstance, UnknownType map ) : base( map )
				 {
					 this.outerInstance = outerInstance;
				 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
				 public override void close()
				 {
					  if ( _outerInstance.enabled.get() )
					  {
							_outerInstance.barrier.reached();
					  }
					  base.close();
				 }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 5_000L) public void writerShouldLockOutClose() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void WriterShouldLockOutClose()
		 {
			  // GIVEN
			  GBPTree<MutableLong, MutableLong> index = index().build();

			  // WHEN
			  Neo4Net.Test.Barrier_Control barrier = new Neo4Net.Test.Barrier_Control();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> write = executor.submit(throwing(() ->
			  Future<object> write = _executor.submit(throwing(() =>
			  {
				using ( Writer<MutableLong, MutableLong> writer = index.Writer() )
				{
					 writer.Put( new MutableLong( 1 ), new MutableLong( 1 ) );
					 barrier.Reached();
				}
			  }));
			  barrier.AwaitUninterruptibly();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> close = executor.submit(throwing(index::close));
			  Future<object> close = _executor.submit( throwing( index.close ) );
			  ShouldWait( close );

			  // THEN
			  barrier.Release();
			  close.get();
			  write.get();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dirtyIndexIsNotCleanOnNextStartWithoutRecovery() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DirtyIndexIsNotCleanOnNextStartWithoutRecovery()
		 {
			  MakeDirty();

			  using ( GBPTree<MutableLong, MutableLong> index = index().with(RecoveryCleanupWorkCollector.Ignore()).Build() )
			  {
					assertTrue( index.WasDirtyOnStartup() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void correctlyShutdownIndexIsClean() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CorrectlyShutdownIndexIsClean()
		 {
			  using ( GBPTree<MutableLong, MutableLong> index = index().build() )
			  {
					using ( Writer<MutableLong, MutableLong> writer = index.Writer() )
					{
						 writer.Put( new MutableLong( 1L ), new MutableLong( 2L ) );
					}
					index.Checkpoint( UNLIMITED );
			  }
			  using ( GBPTree<MutableLong, MutableLong> index = index().build() )
			  {
					assertFalse( index.WasDirtyOnStartup() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 5_000L) public void cleanJobShouldLockOutCheckpoint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CleanJobShouldLockOutCheckpoint()
		 {
			  // GIVEN
			  MakeDirty();

			  RecoveryCleanupWorkCollector cleanupWork = new ControlledRecoveryCleanupWorkCollector();
			  CleanJobControlledMonitor monitor = new CleanJobControlledMonitor();
			  using ( GBPTree<MutableLong, MutableLong> index = index().with(monitor).With(cleanupWork).build() )
			  {
					// WHEN cleanup not finished
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> cleanup = executor.submit(throwing(cleanupWork::start));
					Future<object> cleanup = _executor.submit( throwing( cleanupWork.start ) );
					monitor.Barrier.awaitUninterruptibly();

					// THEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> checkpoint = executor.submit(throwing(() -> index.checkpoint(UNLIMITED)));
					Future<object> checkpoint = _executor.submit( throwing( () => index.checkpoint(UNLIMITED) ) );
					ShouldWait( checkpoint );

					monitor.Barrier.release();
					cleanup.get();
					checkpoint.get();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 5_000L) public void cleanJobShouldLockOutCheckpointOnNoUpdate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CleanJobShouldLockOutCheckpointOnNoUpdate()
		 {
			  // GIVEN
			  MakeDirty();

			  RecoveryCleanupWorkCollector cleanupWork = new ControlledRecoveryCleanupWorkCollector();
			  CleanJobControlledMonitor monitor = new CleanJobControlledMonitor();
			  using ( GBPTree<MutableLong, MutableLong> index = index().with(monitor).With(cleanupWork).build() )
			  {
					// WHEN cleanup not finished
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> cleanup = executor.submit(throwing(cleanupWork::start));
					Future<object> cleanup = _executor.submit( throwing( cleanupWork.start ) );
					monitor.Barrier.awaitUninterruptibly();

					// THEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> checkpoint = executor.submit(throwing(() -> index.checkpoint(UNLIMITED)));
					Future<object> checkpoint = _executor.submit( throwing( () => index.checkpoint(UNLIMITED) ) );
					ShouldWait( checkpoint );

					monitor.Barrier.release();
					cleanup.get();
					checkpoint.get();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 5_000L) public void cleanJobShouldNotLockOutClose() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CleanJobShouldNotLockOutClose()
		 {
			  // GIVEN
			  MakeDirty();

			  RecoveryCleanupWorkCollector cleanupWork = new ControlledRecoveryCleanupWorkCollector();
			  CleanJobControlledMonitor monitor = new CleanJobControlledMonitor();
			  GBPTree<MutableLong, MutableLong> index = index().with(monitor).With(cleanupWork).build();

			  // WHEN
			  // Cleanup not finished
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> cleanup = executor.submit(throwing(cleanupWork::start));
			  Future<object> cleanup = _executor.submit( throwing( cleanupWork.start ) );
			  monitor.Barrier.awaitUninterruptibly();

			  // THEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> close = executor.submit(throwing(index::close));
			  Future<object> close = _executor.submit( throwing( index.close ) );
			  close.get();

			  monitor.Barrier.release();
			  cleanup.get();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 5_000L) public void cleanJobShouldLockOutWriter() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CleanJobShouldLockOutWriter()
		 {
			  // GIVEN
			  MakeDirty();

			  RecoveryCleanupWorkCollector cleanupWork = new ControlledRecoveryCleanupWorkCollector();
			  CleanJobControlledMonitor monitor = new CleanJobControlledMonitor();
			  using ( GBPTree<MutableLong, MutableLong> index = index().with(monitor).With(cleanupWork).build() )
			  {
					// WHEN
					// Cleanup not finished
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> cleanup = executor.submit(throwing(cleanupWork::start));
					Future<object> cleanup = _executor.submit( throwing( cleanupWork.start ) );
					monitor.Barrier.awaitUninterruptibly();

					// THEN
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> writer = executor.submit(throwing(() -> index.writer().close()));
					Future<object> writer = _executor.submit( throwing( () => index.Writer().close() ) );
					ShouldWait( writer );

					monitor.Barrier.release();
					cleanup.get();
					writer.get();
			  }
		 }

		 /* Cleaner test */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void cleanerShouldDieSilentlyOnClose() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CleanerShouldDieSilentlyOnClose()
		 {
			  // GIVEN
			  MakeDirty();

			  AtomicBoolean blockOnNextIO = new AtomicBoolean();
			  Neo4Net.Test.Barrier_Control control = new Neo4Net.Test.Barrier_Control();
			  PageCache pageCache = PageCacheThatBlockWhenToldTo( control, blockOnNextIO );
			  ControlledRecoveryCleanupWorkCollector collector = new ControlledRecoveryCleanupWorkCollector();
			  collector.Init();

			  Future<IList<CleanupJob>> cleanJob;
			  using ( GBPTree<MutableLong, MutableLong> ignored = Index( pageCache ).with( collector ).build() )
			  {
					blockOnNextIO.set( true );
					cleanJob = _executor.submit( StartAndReturnStartedJobs( collector ) );

					// WHEN
					// ... cleaner is still alive
					control.Await();

					// ... close
			  }

			  // THEN
			  control.Release();
			  AssertFailedDueToUnmappedFile( cleanJob );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void treeMustBeDirtyAfterCleanerDiedOnClose() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TreeMustBeDirtyAfterCleanerDiedOnClose()
		 {
			  // GIVEN
			  MakeDirty();

			  AtomicBoolean blockOnNextIO = new AtomicBoolean();
			  Neo4Net.Test.Barrier_Control control = new Neo4Net.Test.Barrier_Control();
			  PageCache pageCache = PageCacheThatBlockWhenToldTo( control, blockOnNextIO );
			  ControlledRecoveryCleanupWorkCollector collector = new ControlledRecoveryCleanupWorkCollector();
			  collector.Init();

			  Future<IList<CleanupJob>> cleanJob;
			  using ( GBPTree<MutableLong, MutableLong> ignored = Index( pageCache ).with( collector ).build() )
			  {
					blockOnNextIO.set( true );
					cleanJob = _executor.submit( StartAndReturnStartedJobs( collector ) );

					// WHEN
					// ... cleaner is still alive
					control.Await();

					// ... close
			  }

			  // THEN
			  control.Release();
			  AssertFailedDueToUnmappedFile( cleanJob );

			  MonitorDirty monitor = new MonitorDirty();
			  using ( GBPTree<MutableLong, MutableLong> ignored = Index().with(monitor).build() )
			  {
					assertFalse( monitor.CleanOnStart() );
			  }
		 }

		 private Callable<IList<CleanupJob>> StartAndReturnStartedJobs( ControlledRecoveryCleanupWorkCollector collector )
		 {
			  return () =>
			  {
				try
				{
					 collector.Start();
				}
				catch ( Exception throwable )
				{
					 throw new Exception( throwable );
				}
				return collector.AllStartedJobs();
			  };
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertFailedDueToUnmappedFile(java.util.concurrent.Future<java.util.List<CleanupJob>> cleanJob) throws InterruptedException, java.util.concurrent.ExecutionException
		 private void AssertFailedDueToUnmappedFile( Future<IList<CleanupJob>> cleanJob )
		 {
			  foreach ( CleanupJob job in cleanJob.get() )
			  {
					assertTrue( job.HasFailed() );
					assertThat( job.Cause.Message, allOf( containsString( "File" ), containsString( "unmapped" ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void writerMustRecognizeFailedCleaning() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void WriterMustRecognizeFailedCleaning()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  MustRecognizeFailedCleaning( GBPTree::writer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void checkpointMustRecognizeFailedCleaning() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CheckpointMustRecognizeFailedCleaning()
		 {
			  MustRecognizeFailedCleaning( index => index.checkpoint( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void mustRecognizeFailedCleaning(org.neo4j.function.ThrowingConsumer<GBPTree<org.apache.commons.lang3.mutable.MutableLong,org.apache.commons.lang3.mutable.MutableLong>,java.io.IOException> operation) throws Exception
		 private void MustRecognizeFailedCleaning( ThrowingConsumer<GBPTree<MutableLong, MutableLong>, IOException> operation )
		 {
			  // given
			  MakeDirty();
			  Exception cleanupException = new Exception( "Fail cleaning job" );
			  CleanJobControlledMonitor cleanupMonitor = new CleanJobControlledMonitorAnonymousInnerClass( this, cleanupException );
			  ControlledRecoveryCleanupWorkCollector collector = new ControlledRecoveryCleanupWorkCollector();

			  // when
			  using ( GBPTree<MutableLong, MutableLong> index = index().with(cleanupMonitor).With(collector).build() )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> cleanup = executor.submit(throwing(collector::start));
					Future<object> cleanup = _executor.submit( throwing( collector.start ) );
					ShouldWait( cleanup );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> checkpoint = executor.submit(throwing(() -> operation.accept(index)));
					Future<object> checkpoint = _executor.submit( throwing( () => operation.accept(index) ) );
					ShouldWait( checkpoint );

					cleanupMonitor.Barrier.release();
					cleanup.get();

					// then
					try
					{
						 checkpoint.get();
						 fail( "Expected checkpoint to fail because of failed cleaning job" );
					}
					catch ( ExecutionException e )
					{
						 assertThat( e.Message, allOf( containsString( "cleaning" ), containsString( "failed" ) ) );
					}
			  }
		 }

		 private class CleanJobControlledMonitorAnonymousInnerClass : CleanJobControlledMonitor
		 {
			 private readonly GBPTreeTest _outerInstance;

			 private Exception _cleanupException;

			 public CleanJobControlledMonitorAnonymousInnerClass( GBPTreeTest outerInstance, Exception cleanupException )
			 {
				 this.outerInstance = outerInstance;
				 this._cleanupException = cleanupException;
			 }

			 public override void cleanupFinished( long numberOfPagesVisited, long numberOfCleanedCrashPointers, long durationMillis )
			 {
				  base.cleanupFinished( numberOfPagesVisited, numberOfCleanedCrashPointers, durationMillis );
				  throw _cleanupException;
			 }
		 }

		 /* Checkpoint tests */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCheckpointAfterInitialCreation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCheckpointAfterInitialCreation()
		 {
			  // GIVEN
			  CheckpointCounter checkpointCounter = new CheckpointCounter();

			  // WHEN
			  GBPTree<MutableLong, MutableLong> index = index().with(checkpointCounter).Build();

			  // THEN
			  assertEquals( 1, checkpointCounter.Count() );
			  index.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCheckpointOnClose() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotCheckpointOnClose()
		 {
			  // GIVEN
			  CheckpointCounter checkpointCounter = new CheckpointCounter();

			  // WHEN
			  using ( GBPTree<MutableLong, MutableLong> index = index().with(checkpointCounter).Build() )
			  {
					checkpointCounter.Reset();
					using ( Writer<MutableLong, MutableLong> writer = index.Writer() )
					{
						 writer.Put( new MutableLong( 0 ), new MutableLong( 1 ) );
					}
					index.Checkpoint( UNLIMITED );
					assertEquals( 1, checkpointCounter.Count() );
			  }

			  // THEN
			  assertEquals( 1, checkpointCounter.Count() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCheckpointEvenIfNoChanges() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCheckpointEvenIfNoChanges()
		 {
			  // GIVEN
			  CheckpointCounter checkpointCounter = new CheckpointCounter();

			  // WHEN
			  using ( GBPTree<MutableLong, MutableLong> index = index().with(checkpointCounter).Build() )
			  {
					checkpointCounter.Reset();
					index.Checkpoint( UNLIMITED );

					// THEN
					assertEquals( 1, checkpointCounter.Count() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustNotSeeUpdatesThatWasNotCheckpointed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustNotSeeUpdatesThatWasNotCheckpointed()
		 {
			  // GIVEN
			  using ( GBPTree<MutableLong, MutableLong> index = index().build() )
			  {
					Insert( index, 0, 1 );

					// WHEN
					// No checkpoint before close
			  }

			  // THEN
			  using ( GBPTree<MutableLong, MutableLong> index = index().build() )
			  {
					MutableLong from = new MutableLong( long.MinValue );
					MutableLong to = new MutableLong( MAX_VALUE );
					using ( RawCursor<Hit<MutableLong, MutableLong>, IOException> seek = index.Seek( from, to ) )
					{
						 assertFalse( seek.Next() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustSeeUpdatesThatWasCheckpointed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustSeeUpdatesThatWasCheckpointed()
		 {
			  // GIVEN
			  int key = 1;
			  int value = 2;
			  using ( GBPTree<MutableLong, MutableLong> index = index().build() )
			  {
					Insert( index, key, value );

					// WHEN
					index.Checkpoint( UNLIMITED );
			  }

			  // THEN
			  using ( GBPTree<MutableLong, MutableLong> index = index().build() )
			  {
					MutableLong from = new MutableLong( long.MinValue );
					MutableLong to = new MutableLong( MAX_VALUE );
					using ( RawCursor<Hit<MutableLong, MutableLong>, IOException> seek = index.Seek( from, to ) )
					{
						 assertTrue( seek.Next() );
						 assertEquals( key, seek.get().key().longValue() );
						 assertEquals( value, seek.get().value().longValue() );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustBumpUnstableGenerationOnOpen() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustBumpUnstableGenerationOnOpen()
		 {
			  // GIVEN
			  using ( GBPTree<MutableLong, MutableLong> index = index().build() )
			  {
					Insert( index, 0, 1 );

					// no checkpoint
			  }

			  // WHEN
			  SimpleCleanupMonitor monitor = new SimpleCleanupMonitor();
			  using ( GBPTree<MutableLong, MutableLong> ignore = index().With(monitor).build() )
			  {
			  }

			  // THEN
			  assertTrue( "Expected monitor to get recovery complete message", monitor.CleanupFinishedConflict );
			  assertEquals( "Expected index to have exactly 1 crash pointer from root to successor of root", 1, monitor.NumberOfCleanedCrashPointers );
			  assertEquals( "Expected index to have exactly 2 tree node pages, root and successor of root", 2, monitor.NumberOfPagesVisited ); // Root and successor of root
		 }

		 /* Dirty state tests */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexMustBeCleanOnFirstInitialization() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IndexMustBeCleanOnFirstInitialization()
		 {
			  // GIVEN
			  assertFalse( _fs.get().fileExists(_indexFile) );
			  MonitorDirty monitorDirty = new MonitorDirty();

			  // WHEN
			  using ( GBPTree<MutableLong, MutableLong> ignored = Index().with(monitorDirty).build() )
			  {
			  }

			  // THEN
			  assertTrue( "Expected to be clean on start", monitorDirty.CleanOnStart() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexMustBeCleanWhenClosedWithoutAnyChanges() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IndexMustBeCleanWhenClosedWithoutAnyChanges()
		 {
			  // GIVEN
			  using ( GBPTree<MutableLong, MutableLong> ignored = Index().build() )
			  {
			  }

			  // WHEN
			  MonitorDirty monitorDirty = new MonitorDirty();
			  using ( GBPTree<MutableLong, MutableLong> ignored = Index().with(monitorDirty).build() )
			  {
			  }

			  // THEN
			  assertTrue( "Expected to be clean on start after close with no changes", monitorDirty.CleanOnStart() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexMustBeCleanWhenClosedAfterCheckpoint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IndexMustBeCleanWhenClosedAfterCheckpoint()
		 {
			  // GIVEN
			  using ( GBPTree<MutableLong, MutableLong> index = index().build() )
			  {
					Insert( index, 0, 1 );

					index.Checkpoint( UNLIMITED );
			  }

			  // WHEN
			  MonitorDirty monitorDirty = new MonitorDirty();
			  using ( GBPTree<MutableLong, MutableLong> ignored = index().With(monitorDirty).build() )
			  {
			  }

			  // THEN
			  assertTrue( "Expected to be clean on start after close with checkpoint", monitorDirty.CleanOnStart() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexMustBeDirtyWhenClosedWithChangesSinceLastCheckpoint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IndexMustBeDirtyWhenClosedWithChangesSinceLastCheckpoint()
		 {
			  // GIVEN
			  using ( GBPTree<MutableLong, MutableLong> index = index().build() )
			  {
					Insert( index, 0, 1 );

					// no checkpoint
			  }

			  // WHEN
			  MonitorDirty monitorDirty = new MonitorDirty();
			  using ( GBPTree<MutableLong, MutableLong> ignored = index().With(monitorDirty).build() )
			  {
			  }

			  // THEN
			  assertFalse( "Expected to be dirty on start after close without checkpoint", monitorDirty.CleanOnStart() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void indexMustBeDirtyWhenCrashedWithChangesSinceLastCheckpoint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IndexMustBeDirtyWhenCrashedWithChangesSinceLastCheckpoint()
		 {
			  // GIVEN
			  using ( EphemeralFileSystemAbstraction ephemeralFs = new EphemeralFileSystemAbstraction() )
			  {
					ephemeralFs.Mkdirs( _indexFile.ParentFile );
					PageCache pageCache = _pageCacheRule.getPageCache( ephemeralFs );
					EphemeralFileSystemAbstraction snapshot;
					using ( GBPTree<MutableLong, MutableLong> index = index( pageCache ).build() )
					{
						 Insert( index, 0, 1 );

						 // WHEN
						 // crash
						 snapshot = ephemeralFs.Snapshot();
					}
					pageCache.Close();

					// THEN
					MonitorDirty monitorDirty = new MonitorDirty();
					pageCache = _pageCacheRule.getPageCache( snapshot );
					using ( GBPTree<MutableLong, MutableLong> ignored = index( pageCache ).With( monitorDirty ).build() )
					{
					}
					assertFalse( "Expected to be dirty on start after crash", monitorDirty.CleanOnStart() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void cleanCrashPointersMustTriggerOnDirtyStart() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CleanCrashPointersMustTriggerOnDirtyStart()
		 {
			  // GIVEN
			  using ( GBPTree<MutableLong, MutableLong> index = index().build() )
			  {
					Insert( index, 0, 1 );

					// No checkpoint
			  }

			  // WHEN
			  MonitorCleanup monitor = new MonitorCleanup();
			  using ( GBPTree<MutableLong, MutableLong> ignored = index().With(monitor).build() )
			  {
					// THEN
					assertTrue( "Expected cleanup to be called when starting on dirty tree", monitor.CleanupCalled() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void cleanCrashPointersMustNotTriggerOnCleanStart() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CleanCrashPointersMustNotTriggerOnCleanStart()
		 {
			  // GIVEN
			  using ( GBPTree<MutableLong, MutableLong> index = index().build() )
			  {
					Insert( index, 0, 1 );

					index.Checkpoint( UNLIMITED );
			  }

			  // WHEN
			  MonitorCleanup monitor = new MonitorCleanup();
			  using ( GBPTree<MutableLong, MutableLong> ignored = index().With(monitor).build() )
			  {
					// THEN
					assertFalse( "Expected cleanup not to be called when starting on clean tree", monitor.CleanupCalled() );
			  }
		 }

		 /* TreeState has outdated root */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowIfTreeStatePointToRootWithValidSuccessor() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowIfTreeStatePointToRootWithValidSuccessor()
		 {
			  // GIVEN
			  using ( PageCache specificPageCache = CreatePageCache( DEFAULT_PAGE_SIZE ) )
			  {
					using ( GBPTree<MutableLong, MutableLong> ignore = Index( specificPageCache ).build() )
					{
					}

					// a tree state pointing to root with valid successor
					using ( PagedFile pagedFile = specificPageCache.Map( _indexFile, specificPageCache.PageSize() ), PageCursor cursor = pagedFile.Io(0, PF_SHARED_WRITE_LOCK) )
					{
						 Pair<TreeState, TreeState> treeStates = TreeStatePair.ReadStatePages( cursor, IdSpace.STATE_PAGE_A, IdSpace.STATE_PAGE_B );
						 TreeState newestState = TreeStatePair.SelectNewestValidState( treeStates );
						 long rootId = newestState.RootId();
						 long stableGeneration = newestState.StableGeneration();
						 long unstableGeneration = newestState.UnstableGeneration();

						 TreeNode.GoTo( cursor, "root", rootId );
						 TreeNode.SetSuccessor( cursor, 42, stableGeneration + 1, unstableGeneration + 1 );
					}

					// WHEN
					try
					{
							using ( GBPTree<MutableLong, MutableLong> index = index( specificPageCache ).build() )
							{
							 using ( Writer<MutableLong, MutableLong> ignored = index.Writer() )
							 {
								  fail( "Expected to throw because root pointed to by tree state should have a valid successor." );
							 }
							}
					}
					catch ( TreeInconsistencyException e )
					{
						 assertThat( e.Message, containsString( PointerChecking.WriterTraverseOldStateMessage ) );
					}
			  }
		 }

		 /* IO failure on close */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustRetryCloseIfFailure() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustRetryCloseIfFailure()
		 {
			  // GIVEN
			  AtomicBoolean throwOnNext = new AtomicBoolean();
			  IOException exception = new IOException( "My failure" );
			  PageCache pageCache = PageCacheThatThrowExceptionWhenToldTo( exception, throwOnNext );
			  using ( GBPTree<MutableLong, MutableLong> ignored = Index( pageCache ).build() )
			  {
					// WHEN
					throwOnNext.set( true );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowIllegalStateExceptionOnCallingNextAfterClose() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowIllegalStateExceptionOnCallingNextAfterClose()
		 {
			  // given
			  using ( GBPTree<MutableLong, MutableLong> tree = Index().build() )
			  {
					using ( Writer<MutableLong, MutableLong> writer = tree.Writer() )
					{
						 MutableLong value = new MutableLong();
						 for ( int i = 0; i < 10; i++ )
						 {
							  value.Value = i;
							  writer.Put( value, value );
						 }
					}

					RawCursor<Hit<MutableLong, MutableLong>, IOException> seek = tree.Seek( new MutableLong( 0 ), new MutableLong( MAX_VALUE ) );
					assertTrue( seek.Next() );
					assertTrue( seek.Next() );
					seek.Close();

					for ( int i = 0; i < 2; i++ )
					{
						 try
						 {
							  // when
							  seek.Next();
							  fail( "Should have failed" );
						 }
						 catch ( System.InvalidOperationException )
						 {
							  // then good
						 }
					}
			  }
		 }

		 /* Inconsistency tests */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 60_000L) public void mustThrowIfStuckInInfiniteRootCatchup() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustThrowIfStuckInInfiniteRootCatchup()
		 {
			  // Create a tree with root and two children.
			  // Corrupt one of the children and make it look like a freelist node.
			  // This will cause seekCursor to start from root in an attempt, believing it went wrong because of concurrent updates.
			  // When seekCursor comes back to the same corrupt child again and again it should eventually escape from that loop
			  // with an exception.

			  IList<long> trace = new List<long>();
			  MutableBoolean onOffSwitch = new MutableBoolean( true );
			  PageCursorTracer pageCursorTracer = TrackingPageCursorTracer( trace, onOffSwitch );
			  PageCache pageCache = PageCacheWithTrace( pageCursorTracer );

			  // Build a tree with root and two children.
			  using ( GBPTree<MutableLong, MutableLong> tree = Index( pageCache ).build() )
			  {
					// Insert data until we have a split in root
					TreeWithRootSplit( trace, tree );
					long corruptChild = trace[1];

					// We are not interested in further trace tracking
					onOffSwitch.setFalse();

					// Corrupt the child
					CorruptTheChild( pageCache, corruptChild );

					// when seek end up in this corrupt child we should eventually fail with a tree inconsistency exception
					try
					{
							using ( RawCursor<Hit<MutableLong, MutableLong>, IOException> seek = tree.Seek( new MutableLong( 0 ), new MutableLong( 0 ) ) )
							{
							 seek.Next();
							 fail( "Expected to throw" );
							}
					}
					catch ( TreeInconsistencyException e )
					{
						 // then good
						 assertThat( e.Message, CoreMatchers.containsString( "Index traversal aborted due to being stuck in infinite loop. This is most likely caused by an inconsistency in the index. " + "Loop occurred when restarting search from root from page " + corruptChild + "." ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 5_000L) public void mustThrowIfStuckInInfiniteRootCatchupMultipleConcurrentSeekers() throws java.io.IOException, InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustThrowIfStuckInInfiniteRootCatchupMultipleConcurrentSeekers()
		 {
			  IList<long> trace = new List<long>();
			  MutableBoolean onOffSwitch = new MutableBoolean( true );
			  PageCursorTracer pageCursorTracer = TrackingPageCursorTracer( trace, onOffSwitch );
			  PageCache pageCache = PageCacheWithTrace( pageCursorTracer );

			  // Build a tree with root and two children.
			  using ( GBPTree<MutableLong, MutableLong> tree = Index( pageCache ).build() )
			  {
					// Insert data until we have a split in root
					TreeWithRootSplit( trace, tree );
					long leftChild = trace[1];
					long rightChild = trace[2];

					// Stop trace tracking because we will soon start pinning pages from different threads
					onOffSwitch.setFalse();

					// Corrupt the child
					CorruptTheChild( pageCache, leftChild );
					CorruptTheChild( pageCache, rightChild );

					// When seek end up in this corrupt child we should eventually fail with a tree inconsistency exception
					// even if we have multiple seeker that traverse different part of the tree and both get stuck in start from root loop.
					ExecutorService executor = Executors.newFixedThreadPool( 2 );
					System.Threading.CountdownEvent go = new System.Threading.CountdownEvent( 2 );
					Future<object> execute1 = executor.submit(() =>
					{
					 go.Signal();
					 go.await();
					 using ( RawCursor<Hit<MutableLong, MutableLong>, IOException> seek = tree.Seek( new MutableLong( 0 ), new MutableLong( 0 ) ) )
					 {
						  seek.next();
					 }
					 return null;
					});

					Future<object> execute2 = executor.submit(() =>
					{
					 go.Signal();
					 go.await();
					 using ( RawCursor<Hit<MutableLong, MutableLong>, IOException> seek = tree.Seek( new MutableLong( MAX_VALUE ), new MutableLong( MAX_VALUE ) ) )
					 {
						  seek.next();
					 }
					 return null;
					});

					AssertFutureFailsWithTreeInconsistencyException( execute1 );
					AssertFutureFailsWithTreeInconsistencyException( execute2 );
			  }
		 }

		 /* ReadOnly */

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustNotMakeAnyChangesInReadOnlyMode() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustNotMakeAnyChangesInReadOnlyMode()
		 {
			  // given
			  PageCache pageCache = CreatePageCache( DEFAULT_PAGE_SIZE );
			  using ( GBPTree<MutableLong, MutableLong> tree = Index( pageCache ).build() )
			  {
					for ( int i = 0; i < 10; i++ )
					{
						 for ( int j = 0; j < 100; j++ )
						 {
							  Insert( tree, _random.nextLong(), _random.nextLong() );
						 }
						 tree.Checkpoint( UNLIMITED );
					}
			  }
			  sbyte[] before = FileContent( _indexFile );

			  using ( GBPTree<MutableLong, MutableLong> tree = Index( pageCache ).withReadOnly( true ).build() )
			  {
					try
					{
						 tree.Writer();
						 fail( "Should have failed" );
					}
					catch ( System.NotSupportedException e )
					{
						 assertThat( e.Message, containsString( "GBPTree was opened in read only mode and can not finish operation: " ) );
					}

					MutableBoolean ioLimitChecked = new MutableBoolean();
					tree.Checkpoint((previousStamp, recentlyCompletedIOs, flushable) =>
					{
					ioLimitChecked.setTrue();
					return 0;
					});
					assertFalse( "Expected checkpoint to be a no-op in read only mode.", ioLimitChecked.Value );
			  }
			  sbyte[] after = FileContent( _indexFile );
			  assertArrayEquals( "Expected file content to be identical before and after opening GBPTree in read only mode.", before, after );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustFailGracefullyIfFileNotExistInReadOnlyMode() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustFailGracefullyIfFileNotExistInReadOnlyMode()
		 {
			  // given
			  PageCache pageCache = CreatePageCache( DEFAULT_PAGE_SIZE );
			  try
			  {
					  using ( GBPTree<MutableLong, MutableLong> ignore = Index( pageCache ).withReadOnly( true ).build() )
					  {
						fail( "Expected constructor to fail when trying to initialize with no index file in readOnly mode." );
					  }
			  }
			  catch ( TreeFileNotFoundException e )
			  {
					assertThat( e.Message, containsString( "Can not create new tree file in read only mode" ) );
					assertThat( e.Message, containsString( _indexFile.AbsolutePath ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private byte[] fileContent(java.io.File indexFile) throws java.io.IOException
		 private sbyte[] FileContent( File indexFile )
		 {
			  using ( StoreChannel storeChannel = _fs.open( indexFile, OpenMode.READ ) )
			  {
					int fileSize = ( int ) storeChannel.size();
					ByteBuffer expectedContent = ByteBuffer.allocate( fileSize );
					storeChannel.ReadAll( expectedContent );
					expectedContent.flip();
					sbyte[] bytes = new sbyte[fileSize];
					expectedContent.get( bytes );
					return bytes;
			  }
		 }

		 private DefaultPageCursorTracer TrackingPageCursorTracer( IList<long> trace, MutableBoolean onOffSwitch )
		 {
			  return new DefaultPageCursorTracerAnonymousInnerClass( this, trace, onOffSwitch );
		 }

		 private class DefaultPageCursorTracerAnonymousInnerClass : DefaultPageCursorTracer
		 {
			 private readonly GBPTreeTest _outerInstance;

			 private IList<long> _trace;
			 private MutableBoolean _onOffSwitch;

			 public DefaultPageCursorTracerAnonymousInnerClass( GBPTreeTest outerInstance, IList<long> trace, MutableBoolean onOffSwitch )
			 {
				 this.outerInstance = outerInstance;
				 this._trace = trace;
				 this._onOffSwitch = onOffSwitch;
			 }

			 public override PinEvent beginPin( bool writeLock, long filePageId, PageSwapper swapper )
			 {
				  if ( _onOffSwitch.True )
				  {
						_trace.Add( filePageId );
				  }
				  return base.beginPin( writeLock, filePageId, swapper );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertFutureFailsWithTreeInconsistencyException(java.util.concurrent.Future<Object> execute1) throws InterruptedException
		 private void AssertFutureFailsWithTreeInconsistencyException( Future<object> execute1 )
		 {
			  try
			  {
					execute1.get();
					fail( "Expected to fail" );
			  }
			  catch ( ExecutionException e )
			  {
					Exception cause = e.InnerException;
					if ( !( cause is TreeInconsistencyException ) )
					{
						 fail( "Expected cause to be " + typeof( TreeInconsistencyException ) + " but was " + Exceptions.stringify( cause ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void corruptTheChild(org.neo4j.io.pagecache.PageCache pageCache, long corruptChild) throws java.io.IOException
		 private void CorruptTheChild( PageCache pageCache, long corruptChild )
		 {
			  using ( PagedFile pagedFile = pageCache.Map( _indexFile, DEFAULT_PAGE_SIZE ), PageCursor cursor = pagedFile.Io( 0, PF_SHARED_WRITE_LOCK ) )
			  {
					assertTrue( cursor.Next( corruptChild ) );
					assertTrue( TreeNode.IsLeaf( cursor ) );

					// Make child look like freelist node
					cursor.PutByte( TreeNode.BYTE_POS_NODE_TYPE, TreeNode.NODE_TYPE_FREE_LIST_NODE );
			  }
		 }

		 /// <summary>
		 /// When split is done, trace contain:
		 /// trace.get( 0 ) - root
		 /// trace.get( 1 ) - leftChild
		 /// trace.get( 2 ) - rightChild
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void treeWithRootSplit(java.util.List<long> trace, GBPTree<org.apache.commons.lang3.mutable.MutableLong,org.apache.commons.lang3.mutable.MutableLong> tree) throws java.io.IOException
		 private void TreeWithRootSplit( IList<long> trace, GBPTree<MutableLong, MutableLong> tree )
		 {
			  long count = 0;
			  do
			  {
					using ( Writer<MutableLong, MutableLong> writer = tree.Writer() )
					{
						 writer.Put( new MutableLong( count ), new MutableLong( count ) );
						 count++;
					}
					trace.Clear();
					using ( RawCursor<Hit<MutableLong, MutableLong>, IOException> seek = tree.Seek( new MutableLong( 0 ), new MutableLong( 0 ) ) )
					{
						 seek.Next();
					}
			  } while ( trace.Count <= 1 );

			  trace.Clear();
			  using ( RawCursor<Hit<MutableLong, MutableLong>, IOException> seek = tree.Seek( new MutableLong( 0 ), new MutableLong( MAX_VALUE ) ) )
			  {
					//noinspection StatementWithEmptyBody
					while ( seek.Next() )
					{
					}
			  }
		 }

		 private PageCache PageCacheWithTrace( PageCursorTracer pageCursorTracer )
		 {
			  // A page cache tracer that we can use to see when tree has seen enough updates and to figure out on which page the child sits.Trace( trace );
			  PageCursorTracerSupplier pageCursorTracerSupplier = () => pageCursorTracer;
			  return CreatePageCache( DEFAULT_PAGE_SIZE, pageCursorTracerSupplier );
		 }

		 private class ControlledRecoveryCleanupWorkCollector : RecoveryCleanupWorkCollector
		 {
			  internal LinkedList<CleanupJob> Jobs = new LinkedList<CleanupJob>();
			  internal IList<CleanupJob> StartedJobs = new LinkedList<CleanupJob>();

			  public override void Start()
			  {
					ExecuteWithExecutor(_outerInstance.executor =>
					{
					 CleanupJob job;
					 while ( ( job = Jobs.RemoveFirst() ) != null )
					 {
						  try
						  {
								job.Run( _outerInstance.executor );
								StartedJobs.Add( job );
						  }
						  finally
						  {
								job.Close();
						  }
					 }
					});
			  }

			  public override void Add( CleanupJob job )
			  {
					Jobs.AddLast( job );
			  }

			  internal virtual IList<CleanupJob> AllStartedJobs()
			  {
					return StartedJobs;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.neo4j.io.pagecache.PageCache pageCacheThatThrowExceptionWhenToldTo(final java.io.IOException e, final java.util.concurrent.atomic.AtomicBoolean throwOnNextIO)
		 private PageCache PageCacheThatThrowExceptionWhenToldTo( IOException e, AtomicBoolean throwOnNextIO )
		 {
			  return new DelegatingPageCacheAnonymousInnerClass( this, CreatePageCache( DEFAULT_PAGE_SIZE ), e, throwOnNextIO );
		 }

		 private class DelegatingPageCacheAnonymousInnerClass : DelegatingPageCache
		 {
			 private readonly GBPTreeTest _outerInstance;

			 private IOException _e;
			 private AtomicBoolean _throwOnNextIO;

			 public DelegatingPageCacheAnonymousInnerClass( GBPTreeTest outerInstance, PageCache createPageCache, IOException e, AtomicBoolean throwOnNextIO ) : base( createPageCache )
			 {
				 this._outerInstance = outerInstance;
				 this._e = e;
				 this._throwOnNextIO = throwOnNextIO;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.pagecache.PagedFile map(java.io.File file, int pageSize, java.nio.file.OpenOption... openOptions) throws java.io.IOException
			 public override PagedFile map( File file, int pageSize, params OpenOption[] openOptions )
			 {
				  return new DelegatingPagedFileAnonymousInnerClass( this, base.map( file, pageSize, openOptions ) );
			 }

			 private class DelegatingPagedFileAnonymousInnerClass : DelegatingPagedFile
			 {
				 private readonly DelegatingPageCacheAnonymousInnerClass _outerInstance;

				 public DelegatingPagedFileAnonymousInnerClass( DelegatingPageCacheAnonymousInnerClass outerInstance, UnknownType map ) : base( map )
				 {
					 this.outerInstance = outerInstance;
				 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.pagecache.PageCursor io(long pageId, int pf_flags) throws java.io.IOException
				 public override PageCursor io( long pageId, int pfFlags )
				 {
					  maybeThrow();
					  return base.io( pageId, pfFlags );
				 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flushAndForce(org.neo4j.io.pagecache.IOLimiter limiter) throws java.io.IOException
				 public override void flushAndForce( IOLimiter limiter )
				 {
					  maybeThrow();
					  base.flushAndForce( limiter );
				 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void maybeThrow() throws java.io.IOException
				 private void maybeThrow()
				 {
					  if ( _outerInstance.throwOnNextIO.get() )
					  {
							_outerInstance.throwOnNextIO.set( false );
							Debug.Assert( _outerInstance.e != null );
							throw _outerInstance.e;
					  }
				 }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.neo4j.io.pagecache.PageCache pageCacheThatBlockWhenToldTo(final org.neo4j.test.Barrier barrier, final java.util.concurrent.atomic.AtomicBoolean blockOnNextIO)
		 private PageCache PageCacheThatBlockWhenToldTo( Barrier barrier, AtomicBoolean blockOnNextIO )
		 {
			  return new DelegatingPageCacheAnonymousInnerClass2( this, CreatePageCache( DEFAULT_PAGE_SIZE ), barrier, blockOnNextIO );
		 }

		 private class DelegatingPageCacheAnonymousInnerClass2 : DelegatingPageCache
		 {
			 private readonly GBPTreeTest _outerInstance;

			 private Barrier _barrier;
			 private AtomicBoolean _blockOnNextIO;

			 public DelegatingPageCacheAnonymousInnerClass2( GBPTreeTest outerInstance, PageCache createPageCache, Barrier barrier, AtomicBoolean blockOnNextIO ) : base( createPageCache )
			 {
				 this.outerInstance = outerInstance;
				 this._barrier = barrier;
				 this._blockOnNextIO = blockOnNextIO;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.pagecache.PagedFile map(java.io.File file, int pageSize, java.nio.file.OpenOption... openOptions) throws java.io.IOException
			 public override PagedFile map( File file, int pageSize, params OpenOption[] openOptions )
			 {
				  return new DelegatingPagedFileAnonymousInnerClass2( this, base.map( file, pageSize, openOptions ) );
			 }

			 private class DelegatingPagedFileAnonymousInnerClass2 : DelegatingPagedFile
			 {
				 private readonly DelegatingPageCacheAnonymousInnerClass2 _outerInstance;

				 public DelegatingPagedFileAnonymousInnerClass2( DelegatingPageCacheAnonymousInnerClass2 outerInstance, UnknownType map ) : base( map )
				 {
					 this.outerInstance = outerInstance;
				 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.pagecache.PageCursor io(long pageId, int pf_flags) throws java.io.IOException
				 public override PageCursor io( long pageId, int pfFlags )
				 {
					  maybeBlock();
					  return base.io( pageId, pfFlags );
				 }

				 private void maybeBlock()
				 {
					  if ( _outerInstance.blockOnNextIO.get() )
					  {
							_outerInstance.barrier.reached();
					  }
				 }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void makeDirty() throws java.io.IOException
		 private void MakeDirty()
		 {
			  MakeDirty( CreatePageCache( DEFAULT_PAGE_SIZE ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void makeDirty(org.neo4j.io.pagecache.PageCache pageCache) throws java.io.IOException
		 private void MakeDirty( PageCache pageCache )
		 {
			  using ( GBPTree<MutableLong, MutableLong> index = index( pageCache ).build() )
			  {
					// Make dirty
					index.Writer().close();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void insert(GBPTree<org.apache.commons.lang3.mutable.MutableLong,org.apache.commons.lang3.mutable.MutableLong> index, long key, long value) throws java.io.IOException
		 private void Insert( GBPTree<MutableLong, MutableLong> index, long key, long value )
		 {
			  using ( Writer<MutableLong, MutableLong> writer = index.Writer() )
			  {
					writer.Put( new MutableLong( key ), new MutableLong( value ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shouldWait(java.util.concurrent.Future<?> future) throws InterruptedException, java.util.concurrent.ExecutionException
		 private void ShouldWait<T1>( Future<T1> future )
		 {
			  try
			  {
					future.get( 200, TimeUnit.MILLISECONDS );
					fail( "Expected timeout" );
			  }
			  catch ( TimeoutException )
			  {
					// good
			  }
		 }

		 private PageCache CreatePageCache( int pageSize )
		 {
			  return _pageCacheRule.getPageCache( _fs.get(), config().withPageSize(pageSize) );
		 }

		 private PageCache CreatePageCache( int pageSize, PageCursorTracerSupplier pageCursorTracerSupplier )
		 {
			  return _pageCacheRule.getPageCache( _fs.get(), config().withPageSize(pageSize).withCursorTracerSupplier(pageCursorTracerSupplier) );
		 }

		 private class CleanJobControlledMonitor : Neo4Net.Index.Internal.gbptree.GBPTree.Monitor_Adaptor
		 {
			  internal readonly Neo4Net.Test.Barrier_Control Barrier = new Neo4Net.Test.Barrier_Control();

			  public override void CleanupFinished( long numberOfPagesVisited, long numberOfCleanedCrashPointers, long durationMillis )
			  {
					Barrier.reached();
			  }
		 }

		 // The most common tree builds in this test
		 private GBPTreeBuilder<MutableLong, MutableLong> Index()
		 {
			  return Index( DEFAULT_PAGE_SIZE );
		 }

		 private GBPTreeBuilder<MutableLong, MutableLong> Index( int pageSize )
		 {
			  return Index( CreatePageCache( pageSize ) );
		 }

		 private GBPTreeBuilder<MutableLong, MutableLong> Index( PageCache pageCache )
		 {
			  return new GBPTreeBuilder<MutableLong, MutableLong>( pageCache, _indexFile, _layout );
		 }

		 private class CheckpointControlledMonitor : Neo4Net.Index.Internal.gbptree.GBPTree.Monitor_Adaptor
		 {
			  internal readonly Neo4Net.Test.Barrier_Control Barrier = new Neo4Net.Test.Barrier_Control();
			  internal volatile bool Enabled;

			  public override void CheckpointCompleted()
			  {
					if ( Enabled )
					{
						 Barrier.reached();
					}
			  }
		 }

		 private class CheckpointCounter : Neo4Net.Index.Internal.gbptree.GBPTree.Monitor_Adaptor
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal int CountConflict;

			  public override void CheckpointCompleted()
			  {
					CountConflict++;
			  }

			  public virtual void Reset()
			  {
					CountConflict = 0;
			  }

			  public virtual int Count()
			  {
					return CountConflict;
			  }
		 }

		 private class MonitorDirty : Neo4Net.Index.Internal.gbptree.GBPTree.Monitor_Adaptor
		 {
			  internal bool Called;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool CleanOnStartConflict;

			  public override void StartupState( bool clean )
			  {
					if ( Called )
					{
						 throw new System.InvalidOperationException( "State has already been set. Can't set it again." );
					}
					Called = true;
					CleanOnStartConflict = clean;
			  }

			  internal virtual bool CleanOnStart()
			  {
					if ( !Called )
					{
						 throw new System.InvalidOperationException( "State has not been set" );
					}
					return CleanOnStartConflict;
			  }
		 }

		 private class MonitorCleanup : Neo4Net.Index.Internal.gbptree.GBPTree.Monitor_Adaptor
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool CleanupCalledConflict;

			  public override void CleanupFinished( long numberOfPagesVisited, long numberOfCleanedCrashPointers, long durationMillis )
			  {
					CleanupCalledConflict = true;
			  }

			  internal virtual bool CleanupCalled()
			  {
					return CleanupCalledConflict;
			  }
		 }
	}

}