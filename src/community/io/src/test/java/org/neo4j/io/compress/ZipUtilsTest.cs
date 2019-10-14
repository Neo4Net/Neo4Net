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
namespace Neo4Net.Io.compress
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using DefaultFileSystemExtension = Neo4Net.Test.extension.DefaultFileSystemExtension;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({DefaultFileSystemExtension.class, TestDirectoryExtension.class}) class ZipUtilsTest
	internal class ZipUtilsTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject TestDirectory testDirectory;
		 internal TestDirectory TestDirectory;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject DefaultFileSystemAbstraction fileSystem;
		 internal DefaultFileSystemAbstraction FileSystem;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void doNotCreateZipArchiveForNonExistentSource() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void DoNotCreateZipArchiveForNonExistentSource()
		 {
			  File archiveFile = TestDirectory.file( "archive.zip" );
			  ZipUtils.Zip( FileSystem, TestDirectory.file( "doesNotExist" ), archiveFile );
			  assertFalse( FileSystem.fileExists( archiveFile ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void doNotCreateZipArchiveForEmptyDirectory() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void DoNotCreateZipArchiveForEmptyDirectory()
		 {
			  File archiveFile = TestDirectory.file( "archive.zip" );
			  File emptyDirectory = TestDirectory.directory( "emptyDirectory" );
			  ZipUtils.Zip( FileSystem, emptyDirectory, archiveFile );
			  assertFalse( FileSystem.fileExists( archiveFile ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void archiveDirectory() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ArchiveDirectory()
		 {
			  File archiveFile = TestDirectory.file( "directoryArchive.zip" );
			  File directory = TestDirectory.directory( "directory" );
			  FileSystem.create( new File( directory, "a" ) ).close();
			  FileSystem.create( new File( directory, "b" ) ).close();
			  ZipUtils.Zip( FileSystem, directory, archiveFile );

			  assertTrue( FileSystem.fileExists( archiveFile ) );
			  assertEquals( 2, CountArchiveEntries( archiveFile ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void archiveDirectoryWithSubdirectories() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ArchiveDirectoryWithSubdirectories()
		 {
			  File archiveFile = TestDirectory.file( "directoryWithSubdirectoriesArchive.zip" );
			  File directoryArchive = TestDirectory.directory( "directoryWithSubdirs" );
			  File subdir1 = new File( directoryArchive, "subdir1" );
			  File subdir2 = new File( directoryArchive, "subdir" );
			  FileSystem.mkdir( subdir1 );
			  FileSystem.mkdir( subdir2 );
			  FileSystem.create( new File( directoryArchive, "a" ) ).close();
			  FileSystem.create( new File( directoryArchive, "b" ) ).close();
			  FileSystem.create( new File( subdir1, "c" ) ).close();
			  FileSystem.create( new File( subdir2, "d" ) ).close();

			  ZipUtils.Zip( FileSystem, directoryArchive, archiveFile );

			  assertTrue( FileSystem.fileExists( archiveFile ) );
			  assertEquals( 6, CountArchiveEntries( archiveFile ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void archiveFile() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ArchiveFile()
		 {
			  File archiveFile = TestDirectory.file( "fileArchive.zip" );
			  File aFile = TestDirectory.file( "a" );
			  FileSystem.create( aFile ).close();
			  ZipUtils.Zip( FileSystem, aFile, archiveFile );

			  assertTrue( FileSystem.fileExists( archiveFile ) );
			  assertEquals( 1, CountArchiveEntries( archiveFile ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void supportSpacesInDestinationPath() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SupportSpacesInDestinationPath()
		 {
			  File archiveFile = TestDirectory.file( "file archive.zip" );
			  File aFile = TestDirectory.file( "a" );
			  FileSystem.create( aFile ).close();
			  ZipUtils.Zip( FileSystem, aFile, archiveFile );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int countArchiveEntries(java.io.File archiveFile) throws java.io.IOException
		 private int CountArchiveEntries( File archiveFile )
		 {
			  using ( ZipInputStream zipInputStream = new ZipInputStream( new BufferedInputStream( new FileStream( archiveFile, FileMode.Open, FileAccess.Read ) ) ) )
			  {
					int entries = 0;
					while ( zipInputStream.NextEntry != null )
					{
						 entries++;
					}
					return entries;
			  }
		 }
	}

}