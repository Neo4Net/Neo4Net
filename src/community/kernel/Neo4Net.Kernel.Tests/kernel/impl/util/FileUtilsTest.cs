using System.Diagnostics;
using System.IO;

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
namespace Neo4Net.Kernel.impl.util
{
	using SystemUtils = org.apache.commons.lang3.SystemUtils;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RuleChain = org.junit.rules.RuleChain;


	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assume.assumeTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.fs.FileUtils.pathToFileAfterMove;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.fs.FileUtils.size;

	public class FileUtilsTest
	{
		private bool InstanceFieldsInitialized = false;

		public FileUtilsTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Chain = RuleChain.outerRule( TestDirectory ).around( Expected );
		}

		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
		 public readonly ExpectedException Expected = ExpectedException.none();
		 public readonly FileSystemAbstraction Fs = new DefaultFileSystemAbstraction();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain chain = org.junit.rules.RuleChain.outerRule(testDirectory).around(expected);
		 public RuleChain Chain;

		 private File _path;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void doBefore()
		 public virtual void DoBefore()
		 {
			  _path = TestDirectory.directory( "path" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void moveFileToDirectory() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MoveFileToDirectory()
		 {
			  File file = TouchFile( "source" );
			  File targetDir = Directory( "dir" );

			  File newLocationOfFile = FileUtils.moveFileToDirectory( file, targetDir );
			  assertTrue( newLocationOfFile.exists() );
			  assertFalse( file.exists() );
			  assertEquals( newLocationOfFile, targetDir.listFiles()[0] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void moveFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MoveFile()
		 {
			  File file = TouchFile( "source" );
			  File targetDir = Directory( "dir" );

			  File newLocationOfFile = new File( targetDir, "new-name" );
			  FileUtils.moveFile( file, newLocationOfFile );
			  assertTrue( newLocationOfFile.exists() );
			  assertFalse( file.exists() );
			  assertEquals( newLocationOfFile, targetDir.listFiles()[0] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testEmptyDirectory() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestEmptyDirectory()
		 {
			  File emptyDir = Directory( "emptyDir" );

			  File nonEmptyDir = Directory( "nonEmptyDir" );
			  File directoryContent = new File( nonEmptyDir, "somefile" );
			  Debug.Assert( directoryContent.createNewFile() );

			  assertTrue( FileUtils.isEmptyDirectory( emptyDir ) );
			  assertFalse( FileUtils.isEmptyDirectory( nonEmptyDir ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pathToFileAfterMoveMustThrowIfFileNotSubPathToFromShorter()
		 public virtual void PathToFileAfterMoveMustThrowIfFileNotSubPathToFromShorter()
		 {
			  File file = new File( "/a" );
			  File from = new File( "/a/b" );
			  File to = new File( "/a/c" );

			  Expected.expect( typeof( System.ArgumentException ) );
			  pathToFileAfterMove( from, to, file );
		 }

		 // INVALID
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pathToFileAfterMoveMustThrowIfFileNotSubPathToFromSameLength()
		 public virtual void PathToFileAfterMoveMustThrowIfFileNotSubPathToFromSameLength()
		 {
			  File file = new File( "/a/f" );
			  File from = new File( "/a/b" );
			  File to = new File( "/a/c" );

			  Expected.expect( typeof( System.ArgumentException ) );
			  pathToFileAfterMove( from, to, file );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pathToFileAfterMoveMustThrowIfFileNotSubPathToFromLonger()
		 public virtual void PathToFileAfterMoveMustThrowIfFileNotSubPathToFromLonger()
		 {
			  File file = new File( "/a/c/f" );
			  File from = new File( "/a/b" );
			  File to = new File( "/a/c" );

			  Expected.expect( typeof( System.ArgumentException ) );
			  pathToFileAfterMove( from, to, file );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pathToFileAfterMoveMustThrowIfFromDirIsCompletePathToFile()
		 public virtual void PathToFileAfterMoveMustThrowIfFromDirIsCompletePathToFile()
		 {
			  File file = new File( "/a/b/f" );
			  File from = new File( "/a/b/f" );
			  File to = new File( "/a/c" );

			  Expected.expect( typeof( System.ArgumentException ) );
			  pathToFileAfterMove( from, to, file );
		 }

		 // SIBLING
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pathToFileAfterMoveMustWorkIfMovingToSibling()
		 public virtual void PathToFileAfterMoveMustWorkIfMovingToSibling()
		 {
			  File file = new File( "/a/b/f" );
			  File from = new File( "/a/b" );
			  File to = new File( "/a/c" );

			  assertThat( pathToFileAfterMove( from, to, file ).Path, @is( Path( "/a/c/f" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pathToFileAfterMoveMustWorkIfMovingToSiblingAndFileHasSubDir()
		 public virtual void PathToFileAfterMoveMustWorkIfMovingToSiblingAndFileHasSubDir()
		 {
			  File file = new File( "/a/b/d/f" );
			  File from = new File( "/a/b" );
			  File to = new File( "/a/c" );

			  assertThat( pathToFileAfterMove( from, to, file ).Path, @is( Path( "/a/c/d/f" ) ) );
		 }

		 // DEEPER
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pathToFileAfterMoveMustWorkIfMovingToSubDir()
		 public virtual void PathToFileAfterMoveMustWorkIfMovingToSubDir()
		 {
			  File file = new File( "/a/b/f" );
			  File from = new File( "/a/b" );
			  File to = new File( "/a/b/c" );

			  assertThat( pathToFileAfterMove( from, to, file ).Path, @is( Path( "/a/b/c/f" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pathToFileAfterMoveMustWorkIfMovingToSubDirAndFileHasSubDir()
		 public virtual void PathToFileAfterMoveMustWorkIfMovingToSubDirAndFileHasSubDir()
		 {
			  File file = new File( "/a/b/d/f" );
			  File from = new File( "/a/b" );
			  File to = new File( "/a/b/c" );

			  assertThat( pathToFileAfterMove( from, to, file ).Path, @is( Path( "/a/b/c/d/f" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pathToFileAfterMoveMustWorkIfMovingOutOfDir()
		 public virtual void PathToFileAfterMoveMustWorkIfMovingOutOfDir()
		 {
			  File file = new File( "/a/b/f" );
			  File from = new File( "/a/b" );
			  File to = new File( "/c" );

			  assertThat( pathToFileAfterMove( from, to, file ).Path, @is( Path( "/c/f" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pathToFileAfterMoveMustWorkIfMovingOutOfDirAndFileHasSubDir()
		 public virtual void PathToFileAfterMoveMustWorkIfMovingOutOfDirAndFileHasSubDir()
		 {
			  File file = new File( "/a/b/d/f" );
			  File from = new File( "/a/b" );
			  File to = new File( "/c" );

			  assertThat( pathToFileAfterMove( from, to, file ).Path, @is( Path( "/c/d/f" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pathToFileAfterMoveMustWorkIfNotMovingAtAll()
		 public virtual void PathToFileAfterMoveMustWorkIfNotMovingAtAll()
		 {
			  File file = new File( "/a/b/f" );
			  File from = new File( "/a/b" );
			  File to = new File( "/a/b" );

			  assertThat( pathToFileAfterMove( from, to, file ).Path, @is( Path( "/a/b/f" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pathToFileAfterMoveMustWorkIfNotMovingAtAllAndFileHasSubDir()
		 public virtual void PathToFileAfterMoveMustWorkIfNotMovingAtAllAndFileHasSubDir()
		 {
			  File file = new File( "/a/b/d/f" );
			  File from = new File( "/a/b" );
			  File to = new File( "/a/b" );

			  assertThat( pathToFileAfterMove( from, to, file ).Path, @is( Path( "/a/b/d/f" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allMacsHaveHighIO()
		 public virtual void AllMacsHaveHighIO()
		 {
			  assumeTrue( SystemUtils.IS_OS_MAC );
			  assertTrue( FileUtils.highIODevice( Paths.get( "." ), false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void windowsNeverHaveHighIO()
		 public virtual void WindowsNeverHaveHighIO()
		 {
			  // Future work: Maybe we should do like on Mac and assume true on Windows as well?
			  assumeTrue( SystemUtils.IS_OS_WINDOWS );
			  assertFalse( FileUtils.highIODevice( Paths.get( "." ), false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void onLinuxDevShmHasHighIO()
		 public virtual void OnLinuxDevShmHasHighIO()
		 {
			  assumeTrue( SystemUtils.IS_OS_LINUX );
			  assertTrue( FileUtils.highIODevice( Paths.get( "/dev/shm" ), false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sizeOfFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SizeOfFile()
		 {
			  File file = TouchFile( "a" );

			  using ( StreamWriter fileWriter = new StreamWriter( file ) )
			  {
					fileWriter.append( 'a' );
			  }

			  assertThat( size( Fs, file ), @is( 1L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sizeOfDirector() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SizeOfDirector()
		 {
			  File dir = Directory( "dir" );
			  File file1 = new File( dir, "file1" );
			  File file2 = new File( dir, "file2" );

			  using ( StreamWriter fileWriter = new StreamWriter( file1 ) )
			  {
					fileWriter.append( 'a' ).append( 'b' );
			  }
			  using ( StreamWriter fileWriter = new StreamWriter( file2 ) )
			  {
					fileWriter.append( 'a' );
			  }

			  assertThat( size( Fs, dir ), @is( 3L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustCountDirectoryContents() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustCountDirectoryContents()
		 {
			  File dir = Directory( "dir" );
			  File file = new File( dir, "file" );
			  File subdir = new File( dir, "subdir" );
			  file.createNewFile();
			  subdir.mkdirs();

			  assertThat( FileUtils.countFilesInDirectoryPath( dir.toPath() ), @is(2L) );
		 }

		 private File Directory( string name )
		 {
			  File dir = new File( _path, name );
			  dir.mkdirs();
			  return dir;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File touchFile(String name) throws java.io.IOException
		 private File TouchFile( string name )
		 {
			  File file = new File( _path, name );
			  file.createNewFile();
			  return file;
		 }

		 private string Path( string path )
		 {
			  return ( new File( path ) ).Path;
		 }
	}

}