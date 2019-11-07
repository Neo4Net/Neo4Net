using System;
using System.IO;
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
namespace Neo4Net.Io.pagecache.impl
{
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using DisabledOnOs = org.junit.jupiter.api.condition.DisabledOnOs;
	using OS = org.junit.jupiter.api.condition.OS;
	using ParameterizedTest = org.junit.jupiter.@params.ParameterizedTest;
	using ValueSource = org.junit.jupiter.@params.provider.ValueSource;


	using RandomAdversary = Neo4Net.Adversaries.RandomAdversary;
	using AdversarialFileSystemAbstraction = Neo4Net.Adversaries.fs.AdversarialFileSystemAbstraction;
	using Configuration = Neo4Net.GraphDb.config.Configuration;
	using DelegatingFileSystemAbstraction = Neo4Net.GraphDb.mockfs.DelegatingFileSystemAbstraction;
	using DelegatingStoreChannel = Neo4Net.GraphDb.mockfs.DelegatingStoreChannel;
	using EphemeralFileSystemAbstraction = Neo4Net.GraphDb.mockfs.EphemeralFileSystemAbstraction;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using StoreFileChannel = Neo4Net.Io.fs.StoreFileChannel;
	using UnsafeUtil = Neo4Net.@unsafe.Impl.Internal.Dragons.UnsafeUtil;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assume.assumeThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.matchers.ByteArrayMatcher.byteArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.proc.ProcessUtil.getClassPath;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.proc.ProcessUtil.getJavaExecutable;

	public class SingleFilePageSwapperTest : PageSwapperTest
	{
		 private EphemeralFileSystemAbstraction _ephemeralFileSystem;
		 private DefaultFileSystemAbstraction _fileSystem;
		 private File _file;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SetUp()
		 {
			  _file = ( new File( "file" ) ).CanonicalFile;
			  _ephemeralFileSystem = new EphemeralFileSystemAbstraction();
			  _fileSystem = new DefaultFileSystemAbstraction();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach void tearDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TearDown()
		 {
			  IOUtils.closeAll( _ephemeralFileSystem, _fileSystem );
		 }

		 protected internal override PageSwapperFactory SwapperFactory()
		 {
			  SingleFilePageSwapperFactory factory = new SingleFilePageSwapperFactory();
			  factory.Open( Fs, Configuration.EMPTY );
			  return factory;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void mkdirs(java.io.File dir) throws java.io.IOException
		 protected internal override void Mkdirs( File dir )
		 {
			  Fs.mkdirs( dir );
		 }

		 protected internal virtual File File
		 {
			 get
			 {
				  return _file;
			 }
		 }

		 protected internal virtual FileSystemAbstraction Fs
		 {
			 get
			 {
				  return EphemeralFileSystem;
			 }
		 }

		 private FileSystemAbstraction EphemeralFileSystem
		 {
			 get
			 {
				  return _ephemeralFileSystem;
			 }
		 }

		 internal virtual FileSystemAbstraction RealFileSystem
		 {
			 get
			 {
				  return _fileSystem;
			 }
		 }

		 private void PutBytes( long page, sbyte[] data, int srcOffset, int tgtOffset, int length )
		 {
			  for ( int i = 0; i < length; i++ )
			  {
					UnsafeUtil.putByte( page + srcOffset + i, data[tgtOffset + i] );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @ValueSource(ints = {0, 1}) void swappingInMustFillPageWithData(int noChannelStriping) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SwappingInMustFillPageWithData( int noChannelStriping )
		 {
			  sbyte[] bytes = new sbyte[] { 1, 2, 3, 4 };
			  StoreChannel channel = Fs.create( File );
			  channel.WriteAll( Wrap( bytes ) );
			  channel.close();

			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapper( factory, File, 4, null, false, Bool( noChannelStriping ) );
			  long target = CreatePage( 4 );
			  swapper.Read( 0, target, SizeOfAsInt( target ) );

			  assertThat( Array( target ), byteArray( bytes ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @ValueSource(ints = {0, 1}) void mustZeroFillPageBeyondEndOfFile(int noChannelStriping) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustZeroFillPageBeyondEndOfFile( int noChannelStriping )
		 {
			  sbyte[] bytes = new sbyte[] { 1, 2, 3, 4, 5, 6 };
			  StoreChannel channel = Fs.create( File );
			  channel.WriteAll( Wrap( bytes ) );
			  channel.close();

			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapper( factory, File, 4, null, false, Bool( noChannelStriping ) );
			  long target = CreatePage( 4 );
			  swapper.Read( 1, target, SizeOfAsInt( target ) );

			  assertThat( Array( target ), byteArray( new sbyte[]{ 5, 6, 0, 0 } ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @ValueSource(ints = {0, 1}) void swappingOutMustWritePageToFile(int noChannelStriping) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SwappingOutMustWritePageToFile( int noChannelStriping )
		 {
			  Fs.create( File ).close();

			  sbyte[] expected = new sbyte[] { 1, 2, 3, 4 };
			  long page = CreatePage( expected );

			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapper( factory, File, 4, null, false, Bool( noChannelStriping ) );
			  swapper.Write( 0, page );

			  Stream stream = Fs.openAsInputStream( File );
			  sbyte[] actual = new sbyte[expected.Length];

			  assertThat( stream.Read( actual, 0, actual.Length ), @is( actual.Length ) );
			  assertThat( actual, byteArray( expected ) );
		 }

		 private long CreatePage( sbyte[] expected )
		 {
			  long page = CreatePage( expected.Length );
			  PutBytes( page, expected, 0, 0, expected.Length );
			  return page;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @ValueSource(ints = {0, 1}) void swappingOutMustNotOverwriteDataBeyondPage(int noChannelStriping) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SwappingOutMustNotOverwriteDataBeyondPage( int noChannelStriping )
		 {
			  sbyte[] initialData = new sbyte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
			  sbyte[] finalData = new sbyte[] { 1, 2, 3, 4, 8, 7, 6, 5, 9, 10 };
			  StoreChannel channel = Fs.create( File );
			  channel.WriteAll( Wrap( initialData ) );
			  channel.close();

			  sbyte[] change = new sbyte[] { 8, 7, 6, 5 };
			  long page = CreatePage( change );

			  PageSwapperFactory factory = CreateSwapperFactory();
			  PageSwapper swapper = CreateSwapper( factory, File, 4, null, false, Bool( noChannelStriping ) );
			  swapper.Write( 1, page );

			  Stream stream = Fs.openAsInputStream( File );
			  sbyte[] actual = new sbyte[( int ) Fs.getFileSize( File )];

			  assertThat( stream.Read( actual, 0, actual.Length ), @is( actual.Length ) );
			  assertThat( actual, byteArray( finalData ) );
		 }

		 /// <summary>
		 /// The OverlappingFileLockException is thrown when tryLock is called on the same file *in the same JVM*.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @ValueSource(ints = {0, 1}) @DisabledOnOs(org.junit.jupiter.api.condition.OS.WINDOWS) void creatingSwapperForFileMustTakeLockOnFile(int noChannelStriping) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CreatingSwapperForFileMustTakeLockOnFile( int noChannelStriping )
		 {
			  PageSwapperFactory factory = CreateSwapperFactory();
			  factory.Open( _fileSystem, Configuration.EMPTY );
			  File file = TestDir.file( "file" );
			  _fileSystem.create( file ).close();

			  PageSwapper pageSwapper = CreateSwapper( factory, file, 4, NoCallback, false, Bool( noChannelStriping ) );

			  try
			  {
					StoreChannel channel = _fileSystem.open( file, OpenMode.READ_WRITE );
					assertThrows( typeof( OverlappingFileLockException ), channel.tryLock );
			  }
			  finally
			  {
					pageSwapper.Close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @ValueSource(ints = {0, 1}) @DisabledOnOs(org.junit.jupiter.api.condition.OS.WINDOWS) void creatingSwapperForInternallyLockedFileMustThrow(int noChannelStriping) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CreatingSwapperForInternallyLockedFileMustThrow( int noChannelStriping )
		 {
			  PageSwapperFactory factory = CreateSwapperFactory();
			  factory.Open( _fileSystem, Configuration.EMPTY );
			  File file = TestDir.file( "file" );

			  StoreFileChannel channel = _fileSystem.create( file );

			  using ( FileLock fileLock = channel.TryLock() )
			  {
					assertThat( fileLock, @is( not( nullValue() ) ) );
					assertThrows( typeof( FileLockException ), () => CreateSwapper(factory, file, 4, NoCallback, true, Bool(noChannelStriping)) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @ValueSource(ints = {0, 1}) @DisabledOnOs(org.junit.jupiter.api.condition.OS.WINDOWS) void creatingSwapperForExternallyLockedFileMustThrow(int noChannelStriping) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CreatingSwapperForExternallyLockedFileMustThrow( int noChannelStriping )
		 {
			  PageSwapperFactory factory = CreateSwapperFactory();
			  factory.Open( _fileSystem, Configuration.EMPTY );
			  File file = TestDir.file( "file" );

			  _fileSystem.create( file ).close();

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getCanonicalName method:
			  ProcessBuilder pb = new ProcessBuilder( JavaExecutable.ToString(), "-cp", ClassPath, typeof(LockThisFileProgram).FullName, file.AbsolutePath );
			  File wd = ( new File( "target/test-classes" ) ).AbsoluteFile;
			  pb.directory( wd );
			  Process process = pb.start();
			  StreamReader stdout = new StreamReader( process.InputStream );
			  Stream stderr = process.ErrorStream;
			  try
			  {
					assumeThat( stdout.ReadLine(), @is(LockThisFileProgram.LOCKED_OUTPUT) );
			  }
			  catch ( Exception e )
			  {
					int b = stderr.Read();
					while ( b != -1 )
					{
						 System.err.write( b );
						 b = stderr.Read();
					}
					System.err.flush();
					int exitCode = process.waitFor();
					Console.WriteLine( "exitCode = " + exitCode );
					throw e;
			  }

			  try
			  {
					assertThrows( typeof( FileLockException ), () => CreateSwapper(factory, file, 4, NoCallback, true, Bool(noChannelStriping)) );
			  }
			  finally
			  {
					process.OutputStream.write( 0 );
					process.OutputStream.flush();
					process.waitFor();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @ValueSource(ints = {0, 1}) @DisabledOnOs(org.junit.jupiter.api.condition.OS.WINDOWS) void mustUnlockFileWhenThePageSwapperIsClosed(int noChannelStriping) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustUnlockFileWhenThePageSwapperIsClosed( int noChannelStriping )
		 {
			  PageSwapperFactory factory = CreateSwapperFactory();
			  factory.Open( _fileSystem, Configuration.EMPTY );
			  File file = TestDir.file( "file" );
			  _fileSystem.create( file ).close();

			  CreateSwapper( factory, file, 4, NoCallback, false, false ).close();

			  using ( StoreFileChannel channel = _fileSystem.open( file, OpenMode.READ_WRITE ), FileLock fileLock = channel.TryLock() )
			  {
					assertThat( fileLock, @is( not( nullValue() ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @ValueSource(ints = {0, 1}) @DisabledOnOs(org.junit.jupiter.api.condition.OS.WINDOWS) void fileMustRemainLockedEvenIfChannelIsClosedByStrayInterrupt(int noChannelStriping) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void FileMustRemainLockedEvenIfChannelIsClosedByStrayInterrupt( int noChannelStriping )
		 {
			  PageSwapperFactory factory = CreateSwapperFactory();
			  factory.Open( _fileSystem, Configuration.EMPTY );
			  File file = TestDir.file( "file" );
			  _fileSystem.create( file ).close();

			  PageSwapper pageSwapper = CreateSwapper( factory, file, 4, NoCallback, false, Bool( noChannelStriping ) );

			  try
			  {
					StoreChannel channel = _fileSystem.open( file, OpenMode.READ_WRITE );

					Thread.CurrentThread.Interrupt();
					pageSwapper.Force();

					assertThrows( typeof( OverlappingFileLockException ), channel.tryLock );
			  }
			  finally
			  {
					pageSwapper.Close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @ValueSource(ints = {0, 1}) @DisabledOnOs(org.junit.jupiter.api.condition.OS.WINDOWS) void mustCloseFilesIfTakingFileLockThrows(int noChannelStriping) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustCloseFilesIfTakingFileLockThrows( int noChannelStriping )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger openFilesCounter = new java.util.concurrent.atomic.AtomicInteger();
			  AtomicInteger openFilesCounter = new AtomicInteger();
			  PageSwapperFactory factory = CreateSwapperFactory();
			  factory.open(new DelegatingFileSystemAbstractionAnonymousInnerClass(this, _fileSystem, openFilesCounter)
			 , Configuration.EMPTY);
			  File file = TestDir.file( "file" );
			  try
			  {
					  using ( StoreChannel ch = _fileSystem.create( file ), FileLock ignore = ch.TryLock() )
					  {
						CreateSwapper( factory, file, 4, NoCallback, false, Bool( noChannelStriping ) ).close();
						fail( "Creating a page swapper for a locked channel should have thrown" );
					  }
			  }
			  catch ( FileLockException )
			  {
					// As expected.
			  }
			  assertThat( openFilesCounter.get(), @is(0) );
		 }

		 private class DelegatingFileSystemAbstractionAnonymousInnerClass : DelegatingFileSystemAbstraction
		 {
			 private readonly SingleFilePageSwapperTest _outerInstance;

			 private AtomicInteger _openFilesCounter;

			 public DelegatingFileSystemAbstractionAnonymousInnerClass( SingleFilePageSwapperTest outerInstance, DefaultFileSystemAbstraction fileSystem, AtomicInteger openFilesCounter ) : base( fileSystem )
			 {
				 this.outerInstance = outerInstance;
				 this._openFilesCounter = openFilesCounter;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.io.fs.StoreChannel open(java.io.File fileName, Neo4Net.io.fs.OpenMode openMode) throws java.io.IOException
			 public override StoreChannel open( File fileName, OpenMode openMode )
			 {
				  _openFilesCounter.AndIncrement;
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
//ORIGINAL LINE: public void close() throws java.io.IOException
				 public override void close()
				 {
					  _outerInstance.openFilesCounter.AndDecrement;
					  base.close();
				 }
			 }
		 }

		 private sbyte[] Array( long page )
		 {
			  int size = SizeOfAsInt( page );
			  sbyte[] array = new sbyte[size];
			  for ( int i = 0; i < size; i++ )
			  {
					array[i] = UnsafeUtil.getByte( page + i );
			  }
			  return array;
		 }

		 private ByteBuffer Wrap( sbyte[] bytes )
		 {
			  ByteBuffer buffer = ByteBuffer.allocate( bytes.Length );
			  foreach ( sbyte b in bytes )
			  {
					buffer.put( b );
			  }
			  buffer.clear();
			  return buffer;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @ValueSource(ints = {0, 1}) void mustHandleMischiefInPositionedRead(int noChannelStriping) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustHandleMischiefInPositionedRead( int noChannelStriping )
		 {
			  int bytesTotal = 512;
			  sbyte[] data = new sbyte[bytesTotal];
			  ThreadLocalRandom.current().NextBytes(data);

			  PageSwapperFactory factory = CreateSwapperFactory();
			  factory.Open( Fs, Configuration.EMPTY );
			  File file = File;
			  PageSwapper swapper = CreateSwapper( factory, file, bytesTotal, NoCallback, true, Bool( noChannelStriping ) );
			  try
			  {
					long page = CreatePage( data );
					swapper.Write( 0, page );
			  }
			  finally
			  {
					swapper.Close();
			  }

			  RandomAdversary adversary = new RandomAdversary( 0.5, 0.0, 0.0 );
			  factory.Open( new AdversarialFileSystemAbstraction( adversary, Fs ), Configuration.EMPTY );
			  swapper = CreateSwapper( factory, file, bytesTotal, NoCallback, false, Bool( noChannelStriping ) );

			  long page = CreatePage( bytesTotal );

			  try
			  {
					for ( int i = 0; i < 10_000; i++ )
					{
						 Clear( page );
						 assertThat( swapper.Read( 0, page, SizeOfAsInt( page ) ), @is( ( long ) bytesTotal ) );
						 assertThat( Array( page ), @is( data ) );
					}
			  }
			  finally
			  {
					swapper.Close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @ValueSource(ints = {0, 1}) void mustHandleMischiefInPositionedWrite(int noChannelStriping) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustHandleMischiefInPositionedWrite( int noChannelStriping )
		 {
			  int bytesTotal = 512;
			  sbyte[] data = new sbyte[bytesTotal];
			  ThreadLocalRandom.current().NextBytes(data);
			  long zeroPage = CreatePage( bytesTotal );
			  Clear( zeroPage );

			  File file = File;
			  PageSwapperFactory factory = CreateSwapperFactory();
			  RandomAdversary adversary = new RandomAdversary( 0.5, 0.0, 0.0 );
			  factory.Open( new AdversarialFileSystemAbstraction( adversary, Fs ), Configuration.EMPTY );
			  PageSwapper swapper = CreateSwapper( factory, file, bytesTotal, NoCallback, true, Bool( noChannelStriping ) );

			  long page = CreatePage( bytesTotal );

			  try
			  {
					for ( int i = 0; i < 10_000; i++ )
					{
						 adversary.ProbabilityFactor = 0;
						 swapper.Write( 0, zeroPage );
						 PutBytes( page, data, 0, 0, data.Length );
						 adversary.ProbabilityFactor = 1;
						 assertThat( swapper.Write( 0, page ), @is( ( long ) bytesTotal ) );
						 Clear( page );
						 adversary.ProbabilityFactor = 0;
						 swapper.Read( 0, page, SizeOfAsInt( page ) );
						 assertThat( Array( page ), @is( data ) );
					}
			  }
			  finally
			  {
					swapper.Close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @ValueSource(ints = {0, 1}) void mustHandleMischiefInPositionedVectoredRead(int noChannelStriping) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustHandleMischiefInPositionedVectoredRead( int noChannelStriping )
		 {
			  int bytesTotal = 512;
			  int bytesPerPage = 32;
			  int pageCount = bytesTotal / bytesPerPage;
			  sbyte[] data = new sbyte[bytesTotal];
			  ThreadLocalRandom.current().NextBytes(data);

			  PageSwapperFactory factory = CreateSwapperFactory();
			  factory.Open( Fs, Configuration.EMPTY );
			  File file = File;
			  PageSwapper swapper = CreateSwapper( factory, file, bytesTotal, NoCallback, true, Bool( noChannelStriping ) );
			  try
			  {
					long page = CreatePage( data );
					swapper.Write( 0, page );
			  }
			  finally
			  {
					swapper.Close();
			  }

			  RandomAdversary adversary = new RandomAdversary( 0.5, 0.0, 0.0 );
			  factory.Open( new AdversarialFileSystemAbstraction( adversary, Fs ), Configuration.EMPTY );
			  swapper = CreateSwapper( factory, file, bytesPerPage, NoCallback, false, Bool( noChannelStriping ) );

			  long[] pages = new long[pageCount];
			  for ( int i = 0; i < pageCount; i++ )
			  {
					pages[i] = CreatePage( bytesPerPage );
			  }

			  sbyte[] temp = new sbyte[bytesPerPage];
			  try
			  {
					for ( int i = 0; i < 10_000; i++ )
					{
						 foreach ( long page in pages )
						 {
							  Clear( page );
						 }
						 assertThat( swapper.Read( 0, pages, bytesPerPage, 0, pages.Length ), @is( ( long ) bytesTotal ) );
						 for ( int j = 0; j < pageCount; j++ )
						 {
							  Array.Copy( data, j * bytesPerPage, temp, 0, bytesPerPage );
							  assertThat( Array( pages[j] ), @is( temp ) );
						 }
					}
			  }
			  finally
			  {
					swapper.Close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @ValueSource(ints = {0, 1}) void mustHandleMischiefInPositionedVectoredWrite(int noChannelStriping) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustHandleMischiefInPositionedVectoredWrite( int noChannelStriping )
		 {
			  int bytesTotal = 512;
			  int bytesPerPage = 32;
			  int pageCount = bytesTotal / bytesPerPage;
			  sbyte[] data = new sbyte[bytesTotal];
			  ThreadLocalRandom.current().NextBytes(data);
			  long zeroPage = CreatePage( bytesPerPage );
			  Clear( zeroPage );

			  File file = File;
			  PageSwapperFactory factory = CreateSwapperFactory();
			  RandomAdversary adversary = new RandomAdversary( 0.5, 0.0, 0.0 );
			  factory.Open( new AdversarialFileSystemAbstraction( adversary, Fs ), Configuration.EMPTY );
			  PageSwapper swapper = CreateSwapper( factory, file, bytesPerPage, NoCallback, true, Bool( noChannelStriping ) );

			  long[] writePages = new long[pageCount];
			  long[] readPages = new long[pageCount];
			  long[] zeroPages = new long[pageCount];
			  for ( int i = 0; i < pageCount; i++ )
			  {
					writePages[i] = CreatePage( bytesPerPage );
					PutBytes( writePages[i], data, 0, i * bytesPerPage, bytesPerPage );
					readPages[i] = CreatePage( bytesPerPage );
					zeroPages[i] = zeroPage;
			  }

			  try
			  {
					for ( int i = 0; i < 10_000; i++ )
					{
						 adversary.ProbabilityFactor = 0;
						 swapper.Write( 0, zeroPages, 0, pageCount );
						 adversary.ProbabilityFactor = 1;
						 swapper.Write( 0, writePages, 0, pageCount );
						 foreach ( long readPage in readPages )
						 {
							  Clear( readPage );
						 }
						 adversary.ProbabilityFactor = 0;
						 assertThat( swapper.Read( 0, readPages, bytesPerPage, 0, pageCount ), @is( ( long ) bytesTotal ) );
						 for ( int j = 0; j < pageCount; j++ )
						 {
							  assertThat( Array( readPages[j] ), @is( Array( writePages[j] ) ) );
						 }
					}
			  }
			  finally
			  {
					swapper.Close();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustDisableStripingIfToldTo() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustDisableStripingIfToldTo()
		 {
			  // given
			  int bytesPerPage = 32;
			  PageSwapperFactory factory = CreateSwapperFactory();
			  FileSystemAbstraction fs = mock( typeof( FileSystemAbstraction ) );
			  StoreChannel channel = mock( typeof( StoreChannel ) );
			  when( channel.TryLock() ).thenReturn(mock(typeof(FileLock)));
			  when( fs.Create( any( typeof( File ) ) ) ).thenReturn( channel );
			  when( fs.Open( any( typeof( File ) ), any() ) ).thenReturn(channel);

			  // when
			  factory.Open( fs, Configuration.EMPTY );
			  PageSwapper swapper = CreateSwapper( factory, _file, bytesPerPage, NoCallback, true, true );
			  try
			  {
					// then
					verify( fs, times( 1 ) ).open( eq( _file ), any( typeof( OpenMode ) ) );
			  }
			  finally
			  {
					swapper.Close();
			  }
		 }

		 /*
		  * Funny how @{@link ParameterizedTest} doesn't have support for booleans so this test is using int instead, acting as boolean.
		  * Good ol' C-style.
		  */
		 private bool Bool( int noChannelStriping )
		 {
			  return noChannelStriping == 1;
		 }
	}

}