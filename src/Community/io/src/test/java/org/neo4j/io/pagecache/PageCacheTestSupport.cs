using System;
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
	using AfterAll = org.junit.jupiter.api.AfterAll;
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using BeforeAll = org.junit.jupiter.api.BeforeAll;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;


	using Configuration = Neo4Net.Graphdb.config.Configuration;
	using EphemeralFileSystemAbstraction = Neo4Net.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using SingleFilePageSwapperFactory = Neo4Net.Io.pagecache.impl.SingleFilePageSwapperFactory;
	using PageCacheTracer = Neo4Net.Io.pagecache.tracing.PageCacheTracer;
	using PageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Neo4Net.Scheduler.ThreadPoolJobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.matchers.ByteArrayMatcher.byteArray;

	public abstract class PageCacheTestSupport<T> where T : PageCache
	{
		 protected internal const long SHORT_TIMEOUT_MILLIS = 10_000;
		 protected internal const long SEMI_LONG_TIMEOUT_MILLIS = 120_000;
		 protected internal const long LONG_TIMEOUT_MILLIS = 360_000;
		 protected internal static ExecutorService Executor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeAll public static void startExecutor()
		 public static void StartExecutor()
		 {
			  Executor = Executors.newCachedThreadPool();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterAll public static void stopExecutor()
		 public static void StopExecutor()
		 {
			  Executor.shutdown();
		 }

		 protected internal int RecordSize = 9;
		 protected internal int MaxPages = 20;

		 protected internal int PageCachePageSize;
		 protected internal int RecordsPerFilePage;
		 protected internal int RecordCount;
		 protected internal int FilePageSize;
		 protected internal ByteBuffer BufA;
		 protected internal FileSystemAbstraction Fs;
		 protected internal JobScheduler JobScheduler;
		 protected internal T PageCache;

		 private Fixture<T> _fixture;

		 protected internal abstract Fixture<T> CreateFixture();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach public void setUp() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  _fixture = CreateFixture();
			  Thread.interrupted(); // Clear stray interrupts
			  Fs = CreateFileSystemAbstraction();
			  JobScheduler = new ThreadPoolJobScheduler();
			  EnsureExists( File( "a" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach public void tearDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  Thread.interrupted(); // Clear stray interrupts

			  if ( PageCache != null )
			  {
					TearDownPageCache( PageCache );
			  }
			  JobScheduler.close();
			  Fs.Dispose();
		 }

		 protected internal T CreatePageCache( PageSwapperFactory swapperFactory, int maxPages, PageCacheTracer tracer, PageCursorTracerSupplier cursorTracerSupplier, VersionContextSupplier versionContextSupplier )
		 {
			  T pageCache = _fixture.createPageCache( swapperFactory, maxPages, tracer, cursorTracerSupplier, versionContextSupplier, JobScheduler );
			  PageCachePageSize = pageCache.PageSize();
			  RecordsPerFilePage = PageCachePageSize / RecordSize;
			  RecordCount = 5 * maxPages * RecordsPerFilePage;
			  FilePageSize = RecordsPerFilePage * RecordSize;
			  BufA = ByteBuffer.allocate( RecordSize );
			  return pageCache;
		 }

		 protected internal virtual T CreatePageCache( FileSystemAbstraction fs, int maxPages, PageCacheTracer tracer, PageCursorTracerSupplier cursorTracerSupplier, VersionContextSupplier versionContextSupplier )
		 {
			  PageSwapperFactory swapperFactory = new SingleFilePageSwapperFactory();
			  swapperFactory.Open( fs, Configuration.EMPTY );
			  return CreatePageCache( swapperFactory, maxPages, tracer, cursorTracerSupplier, versionContextSupplier );
		 }

		 protected internal virtual T CreatePageCache( FileSystemAbstraction fs, int maxPages, PageCacheTracer tracer, PageCursorTracerSupplier cursorTracerSupplier )
		 {
			  return CreatePageCache( fs, maxPages, tracer, cursorTracerSupplier, EmptyVersionContextSupplier.EMPTY );
		 }

		 protected internal T GetPageCache( FileSystemAbstraction fs, int maxPages, PageCacheTracer tracer, PageCursorTracerSupplier cursorTracerSupplier )
		 {
			  if ( PageCache != null )
			  {
					TearDownPageCache( PageCache );
			  }
			  PageCache = CreatePageCache( fs, maxPages, tracer, cursorTracerSupplier, EmptyVersionContextSupplier.EMPTY );
			  return PageCache;
		 }

		 protected internal virtual void ConfigureStandardPageCache()
		 {
			  GetPageCache( Fs, MaxPages, PageCacheTracer.NULL, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null );
		 }

		 protected internal void TearDownPageCache( T pageCache )
		 {
			  _fixture.tearDownPageCache( pageCache );
		 }

		 protected internal FileSystemAbstraction CreateFileSystemAbstraction()
		 {
			  return _fixture.FileSystemAbstraction;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected final java.io.File file(String pathname) throws java.io.IOException
		 protected internal File File( string pathname )
		 {
			  return _fixture.file( pathname );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void ensureExists(java.io.File file) throws java.io.IOException
		 protected internal virtual void EnsureExists( File file )
		 {
			  Fs.mkdirs( file.ParentFile );
			  Fs.create( file ).close();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected java.io.File existingFile(String name) throws java.io.IOException
		 protected internal virtual File ExistingFile( string name )
		 {
			  File file = file( name );
			  EnsureExists( file );
			  return file;
		 }

		 /// <summary>
		 /// Verifies the records on the current page of the cursor.
		 /// <para>
		 /// This does the do-while-retry loop internally.
		 /// </para>
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void verifyRecordsMatchExpected(PageCursor cursor) throws java.io.IOException
		 protected internal virtual void VerifyRecordsMatchExpected( PageCursor cursor )
		 {
			  ByteBuffer expectedPageContents = ByteBuffer.allocate( FilePageSize );
			  ByteBuffer actualPageContents = ByteBuffer.allocate( FilePageSize );
			  sbyte[] record = new sbyte[RecordSize];
			  long pageId = cursor.CurrentPageId;
			  for ( int i = 0; i < RecordsPerFilePage; i++ )
			  {
					long recordId = ( pageId * RecordsPerFilePage ) + i;
					expectedPageContents.position( RecordSize * i );
					ByteBuffer slice = expectedPageContents.slice();
					slice.limit( RecordSize );
					GenerateRecordForId( recordId, slice );
					do
					{
						 cursor.Offset = RecordSize * i;
						 cursor.GetBytes( record );
					} while ( cursor.ShouldRetry() );
					actualPageContents.position( RecordSize * i );
					actualPageContents.put( record );
			  }
			  AssertRecords( pageId, actualPageContents, expectedPageContents );
		 }

		 /// <summary>
		 /// Verifies the records in the current buffer assuming the given page id.
		 /// <para>
		 /// This does the do-while-retry loop internally.
		 /// </para>
		 /// </summary>
		 protected internal virtual void VerifyRecordsMatchExpected( long pageId, int offset, ByteBuffer actualPageContents )
		 {
			  ByteBuffer expectedPageContents = ByteBuffer.allocate( FilePageSize );
			  for ( int i = 0; i < RecordsPerFilePage; i++ )
			  {
					long recordId = ( pageId * RecordsPerFilePage ) + i;
					expectedPageContents.position( RecordSize * i );
					ByteBuffer slice = expectedPageContents.slice();
					slice.limit( RecordSize );
					GenerateRecordForId( recordId, slice );
			  }
			  int len = actualPageContents.limit() - actualPageContents.position();
			  sbyte[] actual = new sbyte[len];
			  sbyte[] expected = new sbyte[len];
			  actualPageContents.get( actual );
			  expectedPageContents.position( offset );
			  expectedPageContents.get( expected );
			  AssertRecords( pageId, actual, expected );
		 }

		 protected internal virtual void AssertRecords( long pageId, ByteBuffer actualPageContents, ByteBuffer expectedPageContents )
		 {
			  sbyte[] actualBytes = actualPageContents.array();
			  sbyte[] expectedBytes = expectedPageContents.array();
			  AssertRecords( pageId, actualBytes, expectedBytes );
		 }

		 protected internal virtual void AssertRecords( long pageId, sbyte[] actualBytes, sbyte[] expectedBytes )
		 {
			  int estimatedPageId = EstimateId( actualBytes );
			  assertThat( "Page id: " + pageId + " " + "(based on record data, it should have been " + estimatedPageId + ", a difference of " + Math.Abs( pageId - estimatedPageId ) + ")", actualBytes, byteArray( expectedBytes ) );
		 }

		 protected internal virtual int EstimateId( sbyte[] record )
		 {
			  return ByteBuffer.wrap( record ).Int - 1;
		 }

		 /// <summary>
		 /// Fill the page bound by the cursor with records that can be verified with
		 /// <seealso cref="verifyRecordsMatchExpected(PageCursor)"/> or <seealso cref="verifyRecordsInFile(java.io.File, int)"/>.
		 /// </summary>
		 protected internal virtual void WriteRecords( PageCursor cursor )
		 {
			  cursor.Offset = 0;
			  for ( int i = 0; i < RecordsPerFilePage; i++ )
			  {
					long recordId = ( cursor.CurrentPageId * RecordsPerFilePage ) + i;
					GenerateRecordForId( recordId, BufA );
					cursor.PutBytes( BufA.array() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void generateFileWithRecords(java.io.File file, int recordCount, int recordSize) throws java.io.IOException
		 protected internal virtual void GenerateFileWithRecords( File file, int recordCount, int recordSize )
		 {
			  using ( StoreChannel channel = Fs.open( file, OpenMode.READ_WRITE ) )
			  {
					GenerateFileWithRecords( channel, recordCount, recordSize );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void generateFileWithRecords(java.nio.channels.WritableByteChannel channel, int recordCount, int recordSize) throws java.io.IOException
		 protected internal virtual void GenerateFileWithRecords( WritableByteChannel channel, int recordCount, int recordSize )
		 {
			  ByteBuffer buf = ByteBuffer.allocate( recordSize );
			  for ( int i = 0; i < recordCount; i++ )
			  {
					GenerateRecordForId( i, buf );
					int rem = buf.remaining();
					do
					{
						 rem -= channel.write( buf );
					} while ( rem > 0 );
			  }
		 }

		 protected internal static void GenerateRecordForId( long id, ByteBuffer buf )
		 {
			  buf.position( 0 );
			  int x = ( int )( id + 1 );
			  buf.putInt( x );
			  while ( buf.position() < buf.limit() )
			  {
					x++;
					buf.put( unchecked( ( sbyte )( x & 0xFF ) ) );
			  }
			  buf.position( 0 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void verifyRecordsInFile(java.io.File file, int recordCount) throws java.io.IOException
		 protected internal virtual void VerifyRecordsInFile( File file, int recordCount )
		 {
			  using ( StoreChannel channel = Fs.open( file, OpenMode.READ ) )
			  {
					VerifyRecordsInFile( channel, recordCount );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void verifyRecordsInFile(java.nio.channels.ReadableByteChannel channel, int recordCount) throws java.io.IOException
		 protected internal virtual void VerifyRecordsInFile( ReadableByteChannel channel, int recordCount )
		 {
			  ByteBuffer buf = ByteBuffer.allocate( RecordSize );
			  ByteBuffer observation = ByteBuffer.allocate( RecordSize );
			  for ( int i = 0; i < recordCount; i++ )
			  {
					GenerateRecordForId( i, buf );
					observation.position( 0 );
					channel.read( observation );
					AssertRecords( i, observation, buf );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected Runnable closePageFile(final PagedFile file)
		 protected internal virtual ThreadStart ClosePageFile( PagedFile file )
		 {
			  return () =>
			  {
				try
				{
					 file.Close();
				}
				catch ( IOException e )
				{
					 throw new AssertionError( e );
				}
			  };
		 }

		 public abstract class Fixture<T> where T : PageCache
		 {
			  public abstract T CreatePageCache( PageSwapperFactory swapperFactory, int maxPages, PageCacheTracer tracer, PageCursorTracerSupplier cursorTracerSupplier, VersionContextSupplier contextSupplier, JobScheduler jobScheduler );

			  public abstract void TearDownPageCache( T pageCache );

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  internal System.Func<FileSystemAbstraction> FileSystemAbstractionSupplier = EphemeralFileSystemAbstraction::new;
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  internal System.Func<string, File> FileConstructor = File::new;

			  public FileSystemAbstraction FileSystemAbstraction
			  {
				  get
				  {
						return FileSystemAbstractionSupplier.get();
				  }
			  }

			  public Fixture<T> WithFileSystemAbstraction( System.Func<FileSystemAbstraction> fileSystemAbstractionSupplier )
			  {
					this.FileSystemAbstractionSupplier = fileSystemAbstractionSupplier;
					return this;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final java.io.File file(String pathname) throws java.io.IOException
			  public File File( string pathname )
			  {
					return FileConstructor.apply( pathname ).CanonicalFile;
			  }

			  public Fixture<T> WithFileConstructor( System.Func<string, File> fileConstructor )
			  {
					this.FileConstructor = fileConstructor;
					return this;
			  }
		 }
	}

}