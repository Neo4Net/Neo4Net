using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.Io.fs
{
	using Matchers = org.hamcrest.Matchers;
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using Predicates = Org.Neo4j.Function.Predicates;
	using FileWatcher = Org.Neo4j.Io.fs.watcher.FileWatcher;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.fs.FileHandle_Fields.HANDLE_DELETE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.fs.FileHandle.handleRename;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.matchers.ByteArrayMatcher.byteArray;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) public abstract class FileSystemAbstractionTest
	public abstract class FileSystemAbstractionTest
	{
		private bool InstanceFieldsInitialized = false;

		public FileSystemAbstractionTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_recordsPerFilePage = _pageCachePageSize / _recordSize;
			_recordCount = 25 * _maxPages * _recordsPerFilePage;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject TestDirectory testDirectory;
		 internal TestDirectory TestDirectory;

		 private int _recordSize = 9;
		 private int _maxPages = 20;
		 private int _pageCachePageSize = 32;
		 private int _recordsPerFilePage;
		 private int _recordCount;
		 protected internal FileSystemAbstraction Fsa;
		 protected internal File Path;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void before()
		 internal virtual void Before()
		 {
			  Fsa = BuildFileSystemAbstraction();
			  Path = new File( TestDirectory.directory(), System.Guid.randomUUID().ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach void tearDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void TearDown()
		 {
			  Fsa.Dispose();
		 }

		 protected internal abstract FileSystemAbstraction BuildFileSystemAbstraction();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreatePath() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCreatePath()
		 {
			  Fsa.mkdirs( Path );

			  assertTrue( Fsa.fileExists( Path ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateDeepPath() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCreateDeepPath()
		 {
			  Path = new File( Path, System.Guid.randomUUID() + "/" + System.Guid.randomUUID() );

			  Fsa.mkdirs( Path );

			  assertTrue( Fsa.fileExists( Path ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreatePathThatAlreadyExists() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCreatePathThatAlreadyExists()
		 {
			  Fsa.mkdirs( Path );
			  assertTrue( Fsa.fileExists( Path ) );

			  Fsa.mkdirs( Path );

			  assertTrue( Fsa.fileExists( Path ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreatePathThatPointsToFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCreatePathThatPointsToFile()
		 {
			  Fsa.mkdirs( Path );
			  assertTrue( Fsa.fileExists( Path ) );
			  Path = new File( Path, "some_file" );
			  using ( StoreChannel channel = Fsa.create( Path ) )
			  {
					assertThat( channel, @is( not( nullValue() ) ) );

					Fsa.mkdirs( Path );

					assertTrue( Fsa.fileExists( Path ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void moveToDirectoryMustMoveFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MoveToDirectoryMustMoveFile()
		 {
			  File source = new File( Path, "source" );
			  File target = new File( Path, "target" );
			  File file = new File( source, "file" );
			  File fileAfterMove = new File( target, "file" );
			  Fsa.mkdirs( source );
			  Fsa.mkdirs( target );
			  Fsa.create( file ).close();
			  assertTrue( Fsa.fileExists( file ) );
			  assertFalse( Fsa.fileExists( fileAfterMove ) );
			  Fsa.moveToDirectory( file, target );
			  assertFalse( Fsa.fileExists( file ) );
			  assertTrue( Fsa.fileExists( fileAfterMove ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void copyToDirectoryCopiesFile() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CopyToDirectoryCopiesFile()
		 {
			  File source = new File( Path, "source" );
			  File target = new File( Path, "target" );
			  File file = new File( source, "file" );
			  File fileAfterCopy = new File( target, "file" );
			  Fsa.mkdirs( source );
			  Fsa.mkdirs( target );
			  Fsa.create( file ).close();
			  assertTrue( Fsa.fileExists( file ) );
			  assertFalse( Fsa.fileExists( fileAfterCopy ) );
			  Fsa.copyToDirectory( file, target );
			  assertTrue( Fsa.fileExists( file ) );
			  assertTrue( Fsa.fileExists( fileAfterCopy ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void copyToDirectoryReplaceExistingFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CopyToDirectoryReplaceExistingFile()
		 {
			  File source = new File( Path, "source" );
			  File target = new File( Path, "target" );
			  File file = new File( source, "file" );
			  File targetFile = new File( target, "file" );
			  Fsa.mkdirs( source );
			  Fsa.mkdirs( target );
			  Fsa.create( file ).close();

			  WriteIntegerIntoFile( targetFile );

			  Fsa.copyToDirectory( file, target );
			  assertTrue( Fsa.fileExists( file ) );
			  assertTrue( Fsa.fileExists( targetFile ) );
			  assertEquals( 0L, Fsa.getFileSize( targetFile ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void deleteRecursivelyMustDeleteAllFilesInDirectory() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void DeleteRecursivelyMustDeleteAllFilesInDirectory()
		 {
			  Fsa.mkdirs( Path );
			  File a = new File( Path, "a" );
			  Fsa.create( a ).close();
			  File b = new File( Path, "b" );
			  Fsa.create( b ).close();
			  File c = new File( Path, "c" );
			  Fsa.create( c ).close();
			  File d = new File( Path, "d" );
			  Fsa.create( d ).close();

			  Fsa.deleteRecursively( Path );

			  assertFalse( Fsa.fileExists( a ) );
			  assertFalse( Fsa.fileExists( b ) );
			  assertFalse( Fsa.fileExists( c ) );
			  assertFalse( Fsa.fileExists( d ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void deleteRecursivelyMustDeleteGivenDirectory() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void DeleteRecursivelyMustDeleteGivenDirectory()
		 {
			  Fsa.mkdirs( Path );
			  Fsa.deleteRecursively( Path );
			  assertFalse( Fsa.fileExists( Path ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void deleteRecursivelyMustDeleteGivenFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void DeleteRecursivelyMustDeleteGivenFile()
		 {
			  Fsa.mkdirs( Path );
			  File file = new File( Path, "file" );
			  Fsa.create( file ).close();
			  Fsa.deleteRecursively( file );
			  assertFalse( Fsa.fileExists( file ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void fileWatcherCreation() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void FileWatcherCreation()
		 {
			  using ( FileWatcher fileWatcher = Fsa.fileWatcher() )
			  {
					assertNotNull( fileWatcher.Watch( TestDirectory.directory( "testDirectory" ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void readAndWriteMustTakeBufferPositionIntoAccount() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ReadAndWriteMustTakeBufferPositionIntoAccount()
		 {
			  sbyte[] bytes = new sbyte[]{ 1, 2, 3, 4, 5 };
			  ByteBuffer buf = ByteBuffer.wrap( bytes );
			  buf.position( 1 );

			  Fsa.mkdirs( Path );
			  File file = new File( Path, "file" );
			  using ( StoreChannel channel = Fsa.open( file, OpenMode.ReadWrite ) )
			  {
					assertThat( channel.write( buf ), @is( 4 ) );
			  }
			  using ( Stream stream = Fsa.openAsInputStream( file ) )
			  {
					assertThat( stream.Read(), @is(2) );
					assertThat( stream.Read(), @is(3) );
					assertThat( stream.Read(), @is(4) );
					assertThat( stream.Read(), @is(5) );
					assertThat( stream.Read(), @is(-1) );
			  }
			  Arrays.fill( bytes, ( sbyte ) 0 );
			  buf.position( 1 );
			  using ( StoreChannel channel = Fsa.open( file, OpenMode.ReadWrite ) )
			  {
					assertThat( channel.read( buf ), @is( 4 ) );
					buf.clear();
					assertThat( buf.get(), @is((sbyte) 0) );
					assertThat( buf.get(), @is((sbyte) 2) );
					assertThat( buf.get(), @is((sbyte) 3) );
					assertThat( buf.get(), @is((sbyte) 4) );
					assertThat( buf.get(), @is((sbyte) 5) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamFilesRecursiveMustBeEmptyForEmptyBaseDirectory() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StreamFilesRecursiveMustBeEmptyForEmptyBaseDirectory()
		 {
			  File dir = ExistingDirectory( "dir" );
			  assertThat( Fsa.streamFilesRecursive( dir ).count(), Matchers.@is(0L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamFilesRecursiveMustListAllFilesInBaseDirectory() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StreamFilesRecursiveMustListAllFilesInBaseDirectory()
		 {
			  File a = ExistingFile( "a" );
			  File b = ExistingFile( "b" );
			  File c = ExistingFile( "c" );
			  Stream<FileHandle> stream = Fsa.streamFilesRecursive( a.ParentFile );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IList<File> filepaths = stream.map( FileHandle::getFile ).collect( toList() );
			  assertThat( filepaths, containsInAnyOrder( a.CanonicalFile, b.CanonicalFile, c.CanonicalFile ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamFilesRecursiveMustListAllFilesInSubDirectories() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StreamFilesRecursiveMustListAllFilesInSubDirectories()
		 {
			  File sub1 = ExistingDirectory( "sub1" );
			  File sub2 = ExistingDirectory( "sub2" );
			  File a = ExistingFile( "a" );
			  File b = new File( sub1, "b" );
			  File c = new File( sub2, "c" );
			  EnsureExists( b );
			  EnsureExists( c );

			  Stream<FileHandle> stream = Fsa.streamFilesRecursive( a.ParentFile );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IList<File> filepaths = stream.map( FileHandle::getFile ).collect( toList() );
			  assertThat( filepaths, containsInAnyOrder( a.CanonicalFile, b.CanonicalFile, c.CanonicalFile ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamFilesRecursiveMustNotListSubDirectories() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StreamFilesRecursiveMustNotListSubDirectories()
		 {
			  File sub1 = ExistingDirectory( "sub1" );
			  File sub2 = ExistingDirectory( "sub2" );
			  File sub2sub1 = new File( sub2, "sub1" );
			  EnsureDirectoryExists( sub2sub1 );
			  ExistingDirectory( "sub3" ); // must not be observed in the stream
			  File a = ExistingFile( "a" );
			  File b = new File( sub1, "b" );
			  File c = new File( sub2, "c" );
			  EnsureExists( b );
			  EnsureExists( c );

			  Stream<FileHandle> stream = Fsa.streamFilesRecursive( a.ParentFile );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IList<File> filepaths = stream.map( FileHandle::getFile ).collect( toList() );
			  assertThat( filepaths, containsInAnyOrder( a.CanonicalFile, b.CanonicalFile, c.CanonicalFile ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamFilesRecursiveFilePathsMustBeCanonical() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StreamFilesRecursiveFilePathsMustBeCanonical()
		 {
			  File sub = ExistingDirectory( "sub" );
			  File a = new File( new File( new File( sub, ".." ), "sub" ), "a" );
			  EnsureExists( a );

			  Stream<FileHandle> stream = Fsa.streamFilesRecursive( sub.ParentFile );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IList<File> filepaths = stream.map( FileHandle::getFile ).collect( toList() );
			  assertThat( filepaths, containsInAnyOrder( a.CanonicalFile ) ); // file in our sub directory

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamFilesRecursiveMustBeAbleToGivePathRelativeToBase() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StreamFilesRecursiveMustBeAbleToGivePathRelativeToBase()
		 {
			  File sub = ExistingDirectory( "sub" );
			  File a = ExistingFile( "a" );
			  File b = new File( sub, "b" );
			  EnsureExists( b );
			  File @base = a.ParentFile;
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  ISet<File> set = Fsa.streamFilesRecursive( @base ).map( FileHandle::getRelativeFile ).collect( toSet() );
			  assertThat( "Files relative to base directory " + @base, set, containsInAnyOrder( new File( "a" ), new File( "sub" + File.separator + "b" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamFilesRecursiveMustListSingleFileGivenAsBase() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StreamFilesRecursiveMustListSingleFileGivenAsBase()
		 {
			  ExistingDirectory( "sub" ); // must not be observed
			  ExistingFile( "sub/x" ); // must not be observed
			  File a = ExistingFile( "a" );

			  Stream<FileHandle> stream = Fsa.streamFilesRecursive( a );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IList<File> filepaths = stream.map( FileHandle::getFile ).collect( toList() );
			  assertThat( filepaths, containsInAnyOrder( a ) ); // note that we don't go into 'sub'
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamFilesRecursiveListedSingleFileMustHaveCanonicalPath() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StreamFilesRecursiveListedSingleFileMustHaveCanonicalPath()
		 {
			  File sub = ExistingDirectory( "sub" );
			  ExistingFile( "sub/x" ); // we query specifically for 'a', so this must not be listed
			  File a = ExistingFile( "a" );
			  File queryForA = new File( new File( sub, ".." ), "a" );

			  Stream<FileHandle> stream = Fsa.streamFilesRecursive( queryForA );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IList<File> filepaths = stream.map( FileHandle::getFile ).collect( toList() );
			  assertThat( filepaths, containsInAnyOrder( a.CanonicalFile ) ); // note that we don't go into 'sub'
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamFilesRecursiveMustReturnEmptyStreamForNonExistingBasePath() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StreamFilesRecursiveMustReturnEmptyStreamForNonExistingBasePath()
		 {
			  File nonExisting = new File( "nonExisting" );
			  assertFalse( Fsa.streamFilesRecursive( nonExisting ).anyMatch( Predicates.alwaysTrue() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamFilesRecursiveMustRenameFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StreamFilesRecursiveMustRenameFiles()
		 {
			  File a = ExistingFile( "a" );
			  File b = NonExistingFile( "b" ); // does not yet exist
			  File @base = a.ParentFile;
			  Fsa.streamFilesRecursive( @base ).forEach( handleRename( b ) );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IList<File> filepaths = Fsa.streamFilesRecursive( @base ).map( FileHandle::getFile ).collect( toList() );
			  assertThat( filepaths, containsInAnyOrder( b.CanonicalFile ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamFilesRecursiveMustDeleteFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StreamFilesRecursiveMustDeleteFiles()
		 {
			  File a = ExistingFile( "a" );
			  File b = ExistingFile( "b" );
			  File c = ExistingFile( "c" );

			  File @base = a.ParentFile;
			  Fsa.streamFilesRecursive( @base ).forEach( HANDLE_DELETE );

			  assertFalse( Fsa.fileExists( a ) );
			  assertFalse( Fsa.fileExists( b ) );
			  assertFalse( Fsa.fileExists( c ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamFilesRecursiveMustThrowWhenDeletingNonExistingFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StreamFilesRecursiveMustThrowWhenDeletingNonExistingFile()
		 {
			  File a = ExistingFile( "a" );
			  FileHandle handle = Fsa.streamFilesRecursive( a ).findAny().get();
			  Fsa.deleteFile( a );
			  assertThrows( typeof( NoSuchFileException ), handle.delete );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamFilesRecursiveMustThrowWhenTargetFileOfRenameAlreadyExists() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StreamFilesRecursiveMustThrowWhenTargetFileOfRenameAlreadyExists()
		 {
			  File a = ExistingFile( "a" );
			  File b = ExistingFile( "b" );
			  FileHandle handle = Fsa.streamFilesRecursive( a ).findAny().get();
			  assertThrows( typeof( FileAlreadyExistsException ), () => handle.rename(b) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamFilesRecursiveMustNotThrowWhenTargetFileOfRenameAlreadyExistsAndUsingReplaceExisting() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StreamFilesRecursiveMustNotThrowWhenTargetFileOfRenameAlreadyExistsAndUsingReplaceExisting()
		 {
			  File a = ExistingFile( "a" );
			  File b = ExistingFile( "b" );
			  FileHandle handle = Fsa.streamFilesRecursive( a ).findAny().get();
			  handle.Rename( b, StandardCopyOption.REPLACE_EXISTING );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamFilesRecursiveMustDeleteSubDirectoriesEmptiedByFileRename() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StreamFilesRecursiveMustDeleteSubDirectoriesEmptiedByFileRename()
		 {
			  File sub = ExistingDirectory( "sub" );
			  File x = new File( sub, "x" );
			  EnsureExists( x );
			  File target = NonExistingFile( "target" );

			  Fsa.streamFilesRecursive( sub ).forEach( handleRename( target ) );

			  assertFalse( Fsa.isDirectory( sub ) );
			  assertFalse( Fsa.fileExists( sub ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamFilesRecursiveMustDeleteMultipleLayersOfSubDirectoriesIfTheyBecomeEmptyByRename() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StreamFilesRecursiveMustDeleteMultipleLayersOfSubDirectoriesIfTheyBecomeEmptyByRename()
		 {
			  File sub = ExistingDirectory( "sub" );
			  File subsub = new File( sub, "subsub" );
			  EnsureDirectoryExists( subsub );
			  File x = new File( subsub, "x" );
			  EnsureExists( x );
			  File target = NonExistingFile( "target" );

			  Fsa.streamFilesRecursive( sub ).forEach( handleRename( target ) );

			  assertFalse( Fsa.isDirectory( subsub ) );
			  assertFalse( Fsa.fileExists( subsub ) );
			  assertFalse( Fsa.isDirectory( sub ) );
			  assertFalse( Fsa.fileExists( sub ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamFilesRecursiveMustNotDeleteDirectoriesAboveBaseDirectoryIfTheyBecomeEmptyByRename() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StreamFilesRecursiveMustNotDeleteDirectoriesAboveBaseDirectoryIfTheyBecomeEmptyByRename()
		 {
			  File sub = ExistingDirectory( "sub" );
			  File subsub = new File( sub, "subsub" );
			  File subsubsub = new File( subsub, "subsubsub" );
			  EnsureDirectoryExists( subsub );
			  EnsureDirectoryExists( subsubsub );
			  File x = new File( subsubsub, "x" );
			  EnsureExists( x );
			  File target = NonExistingFile( "target" );

			  Fsa.streamFilesRecursive( subsub ).forEach( handleRename( target ) );

			  assertFalse( Fsa.fileExists( subsubsub ) );
			  assertFalse( Fsa.isDirectory( subsubsub ) );
			  assertFalse( Fsa.fileExists( subsub ) );
			  assertFalse( Fsa.isDirectory( subsub ) );
			  assertTrue( Fsa.fileExists( sub ) );
			  assertTrue( Fsa.isDirectory( sub ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamFilesRecursiveMustDeleteSubDirectoriesEmptiedByFileDelete() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StreamFilesRecursiveMustDeleteSubDirectoriesEmptiedByFileDelete()
		 {
			  File sub = ExistingDirectory( "sub" );
			  File x = new File( sub, "x" );
			  EnsureExists( x );

			  Fsa.streamFilesRecursive( sub ).forEach( HANDLE_DELETE );

			  assertFalse( Fsa.isDirectory( sub ) );
			  assertFalse( Fsa.fileExists( sub ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamFilesRecursiveMustDeleteMultipleLayersOfSubDirectoriesIfTheyBecomeEmptyByDelete() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StreamFilesRecursiveMustDeleteMultipleLayersOfSubDirectoriesIfTheyBecomeEmptyByDelete()
		 {
			  File sub = ExistingDirectory( "sub" );
			  File subsub = new File( sub, "subsub" );
			  EnsureDirectoryExists( subsub );
			  File x = new File( subsub, "x" );
			  EnsureExists( x );

			  Fsa.streamFilesRecursive( sub ).forEach( HANDLE_DELETE );

			  assertFalse( Fsa.isDirectory( subsub ) );
			  assertFalse( Fsa.fileExists( subsub ) );
			  assertFalse( Fsa.isDirectory( sub ) );
			  assertFalse( Fsa.fileExists( sub ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamFilesRecursiveMustNotDeleteDirectoriesAboveBaseDirectoryIfTheyBecomeEmptyByDelete() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StreamFilesRecursiveMustNotDeleteDirectoriesAboveBaseDirectoryIfTheyBecomeEmptyByDelete()
		 {
			  File sub = ExistingDirectory( "sub" );
			  File subsub = new File( sub, "subsub" );
			  File subsubsub = new File( subsub, "subsubsub" );
			  EnsureDirectoryExists( subsub );
			  EnsureDirectoryExists( subsubsub );
			  File x = new File( subsubsub, "x" );
			  EnsureExists( x );

			  Fsa.streamFilesRecursive( subsub ).forEach( HANDLE_DELETE );

			  assertFalse( Fsa.fileExists( subsubsub ) );
			  assertFalse( Fsa.isDirectory( subsubsub ) );
			  assertFalse( Fsa.fileExists( subsub ) );
			  assertFalse( Fsa.isDirectory( subsub ) );
			  assertTrue( Fsa.fileExists( sub ) );
			  assertTrue( Fsa.isDirectory( sub ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamFilesRecursiveMustCreateMissingPathDirectoriesImpliedByFileRename() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StreamFilesRecursiveMustCreateMissingPathDirectoriesImpliedByFileRename()
		 {
			  File a = ExistingFile( "a" );
			  File sub = new File( Path, "sub" ); // does not exists
			  File target = new File( sub, "b" );

			  FileHandle handle = Fsa.streamFilesRecursive( a ).findAny().get();
			  handle.Rename( target );

			  assertTrue( Fsa.isDirectory( sub ) );
			  assertTrue( Fsa.fileExists( target ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamFilesRecursiveMustNotSeeFilesLaterCreatedBaseDirectory() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StreamFilesRecursiveMustNotSeeFilesLaterCreatedBaseDirectory()
		 {
			  File a = ExistingFile( "a" );
			  Stream<FileHandle> stream = Fsa.streamFilesRecursive( a.ParentFile );
			  File b = ExistingFile( "b" );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  ISet<File> files = stream.map( FileHandle::getFile ).collect( toSet() );
			  assertThat( files, contains( a ) );
			  assertThat( files, not( contains( b ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamFilesRecursiveMustNotSeeFilesRenamedIntoBaseDirectory() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StreamFilesRecursiveMustNotSeeFilesRenamedIntoBaseDirectory()
		 {
			  File a = ExistingFile( "a" );
			  File sub = ExistingDirectory( "sub" );
			  File x = new File( sub, "x" );
			  EnsureExists( x );
			  File target = NonExistingFile( "target" );
			  ISet<File> observedFiles = new HashSet<File>();
			  Fsa.streamFilesRecursive( a.ParentFile ).forEach(fh =>
			  {
				File file = fh.File;
				observedFiles.Add( file );
				if ( file.Equals( x ) )
				{
					 handleRename( target ).accept( fh );
				}
			  });
			  assertThat( observedFiles, containsInAnyOrder( a, x ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamFilesRecursiveMustNotSeeFilesRenamedIntoSubDirectory() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StreamFilesRecursiveMustNotSeeFilesRenamedIntoSubDirectory()
		 {
			  File a = ExistingFile( "a" );
			  File sub = ExistingDirectory( "sub" );
			  File target = new File( sub, "target" );
			  ISet<File> observedFiles = new HashSet<File>();
			  Fsa.streamFilesRecursive( a.ParentFile ).forEach(fh =>
			  {
				File file = fh.File;
				observedFiles.Add( file );
				if ( file.Equals( a ) )
				{
					 handleRename( target ).accept( fh );
				}
			  });
			  assertThat( observedFiles, containsInAnyOrder( a ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamFilesRecursiveRenameMustCanonicaliseSourceFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StreamFilesRecursiveRenameMustCanonicaliseSourceFile()
		 {
			  // File 'a' should canonicalise from 'a/poke/..' to 'a', which is a file that exists.
			  // Thus, this should not throw a NoSuchFileException.
			  File a = new File( new File( ExistingFile( "a" ), "poke" ), ".." );
			  File b = NonExistingFile( "b" );

			  FileHandle handle = Fsa.streamFilesRecursive( a ).findAny().get();
			  handle.Rename( b ); // must not throw
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamFilesRecursiveRenameMustCanonicaliseTargetFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StreamFilesRecursiveRenameMustCanonicaliseTargetFile()
		 {
			  // File 'b' should canonicalise from 'b/poke/..' to 'b', which is a file that doesn't exists.
			  // Thus, this should not throw a NoSuchFileException for the 'poke' directory.
			  File a = ExistingFile( "a" );
			  File b = new File( new File( new File( Path, "b" ), "poke" ), ".." );
			  FileHandle handle = Fsa.streamFilesRecursive( a ).findAny().get();
			  handle.Rename( b );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamFilesRecursiveRenameTargetFileMustBeRenamed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StreamFilesRecursiveRenameTargetFileMustBeRenamed()
		 {
			  File a = ExistingFile( "a" );
			  File b = NonExistingFile( "b" );
			  FileHandle handle = Fsa.streamFilesRecursive( a ).findAny().get();
			  handle.Rename( b );
			  assertTrue( Fsa.fileExists( b ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamFilesRecursiveSourceFileMustNotBeMappableAfterRename() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StreamFilesRecursiveSourceFileMustNotBeMappableAfterRename()
		 {
			  File a = ExistingFile( "a" );
			  File b = NonExistingFile( "b" );
			  FileHandle handle = Fsa.streamFilesRecursive( a ).findAny().get();
			  handle.Rename( b );
			  assertFalse( Fsa.fileExists( a ) );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamFilesRecursiveRenameMustNotChangeSourceFileContents() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StreamFilesRecursiveRenameMustNotChangeSourceFileContents()
		 {
			  File a = ExistingFile( "a" );
			  File b = NonExistingFile( "b" );
			  GenerateFileWithRecords( a, _recordCount );
			  FileHandle handle = Fsa.streamFilesRecursive( a ).findAny().get();
			  handle.Rename( b );
			  VerifyRecordsInFile( b, _recordCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void streamFilesRecursiveRenameMustNotChangeSourceFileContentsWithReplaceExisting() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StreamFilesRecursiveRenameMustNotChangeSourceFileContentsWithReplaceExisting()
		 {
			  File a = ExistingFile( "a" );
			  File b = ExistingFile( "b" );
			  GenerateFileWithRecords( a, _recordCount );
			  GenerateFileWithRecords( b, _recordCount + _recordsPerFilePage );

			  // Fill 'b' with random data
			  using ( StoreChannel channel = Fsa.open( b, OpenMode.ReadWrite ) )
			  {
					ThreadLocalRandom rng = ThreadLocalRandom.current();
					int fileSize = ( int ) channel.size();
					ByteBuffer buffer = ByteBuffer.allocate( fileSize );
					for ( int i = 0; i < fileSize; i++ )

					{
						 buffer.put( i, ( sbyte ) rng.Next() );
					}
					buffer.rewind();
					channel.WriteAll( buffer );
			  }

			  // Do the rename
			  FileHandle handle = Fsa.streamFilesRecursive( a ).findAny().get();
			  handle.Rename( b, REPLACE_EXISTING );

			  // Then verify that the old random data we put in 'b' has been replaced with the contents of 'a'
			  VerifyRecordsInFile( b, _recordCount );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void lastModifiedOfNonExistingFileIsZero()
		 internal virtual void LastModifiedOfNonExistingFileIsZero()
		 {
			  assertThat( Fsa.lastModifiedTime( NonExistingFile( "blabla" ) ), @is( 0L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandlePathThatLooksVeryDifferentWhenCanonicalized() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldHandlePathThatLooksVeryDifferentWhenCanonicalized()
		 {
			  File dir = ExistingDirectory( "/././home/.././././home/././.././././././././././././././././././home/././" );
			  File a = ExistingFile( "/home/a" );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IList<File> filepaths = Fsa.streamFilesRecursive( dir ).map( FileHandle::getRelativeFile ).collect( toList() );
			  assertThat( filepaths, containsInAnyOrder( new File( a.Name ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void generateFileWithRecords(java.io.File file, int recordCount) throws java.io.IOException
		 private void GenerateFileWithRecords( File file, int recordCount )
		 {
			  using ( StoreChannel channel = Fsa.open( file, OpenMode.ReadWrite ) )
			  {
					ByteBuffer buf = ByteBuffer.allocate( _recordSize );
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
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyRecordsInFile(java.io.File file, int recordCount) throws java.io.IOException
		 private void VerifyRecordsInFile( File file, int recordCount )
		 {
			  using ( StoreChannel channel = Fsa.open( file, OpenMode.Read ) )
			  {
					ByteBuffer buf = ByteBuffer.allocate( _recordSize );
					ByteBuffer observation = ByteBuffer.allocate( _recordSize );
					for ( int i = 0; i < recordCount; i++ )
					{
						 GenerateRecordForId( i, buf );
						 observation.position( 0 );
						 channel.read( observation );
						 AssertRecord( i, observation, buf );
					}
			  }
		 }

		 private void AssertRecord( long pageId, ByteBuffer actualPageContents, ByteBuffer expectedPageContents )
		 {
			  sbyte[] actualBytes = actualPageContents.array();
			  sbyte[] expectedBytes = expectedPageContents.array();
			  int estimatedPageId = EstimateId( actualBytes );
			  assertThat( "Page id: " + pageId + " " + "(based on record data, it should have been " + estimatedPageId + ", a difference of " + Math.Abs( pageId - estimatedPageId ) + ")", actualBytes, byteArray( expectedBytes ) );
		 }

		 private int EstimateId( sbyte[] record )
		 {
			  return ByteBuffer.wrap( record ).Int - 1;
		 }

		 private static void GenerateRecordForId( long id, ByteBuffer buf )
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
//ORIGINAL LINE: private java.io.File existingFile(String fileName) throws java.io.IOException
		 private File ExistingFile( string fileName )
		 {
			  File file = new File( Path, fileName );
			  Fsa.mkdirs( Path );
			  Fsa.create( file ).close();
			  return file;
		 }

		 private File NonExistingFile( string fileName )
		 {
			  return new File( Path, fileName );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File existingDirectory(String dir) throws java.io.IOException
		 private File ExistingDirectory( string dir )
		 {
			  File directory = new File( Path, dir );
			  Fsa.mkdirs( directory );
			  return directory;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void ensureExists(java.io.File file) throws java.io.IOException
		 private void EnsureExists( File file )
		 {
			  Fsa.mkdirs( file.ParentFile );
			  Fsa.create( file ).close();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void ensureDirectoryExists(java.io.File directory) throws java.io.IOException
		 private void EnsureDirectoryExists( File directory )
		 {
			  Fsa.mkdirs( directory );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeIntegerIntoFile(java.io.File targetFile) throws java.io.IOException
		 private void WriteIntegerIntoFile( File targetFile )
		 {
			  StoreChannel storeChannel = Fsa.create( targetFile );
			  ByteBuffer byteBuffer = ByteBuffer.allocate( ( sizeof( int ) * 8 ) ).putInt( 7 );
			  byteBuffer.flip();
			  storeChannel.WriteAll( byteBuffer );
			  storeChannel.close();
		 }
	}

}