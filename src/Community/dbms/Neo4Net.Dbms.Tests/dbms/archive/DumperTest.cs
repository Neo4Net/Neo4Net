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
namespace Neo4Net.Dbms.archive
{
	using Test = org.junit.jupiter.api.Test;
	using DisabledOnOs = org.junit.jupiter.api.condition.DisabledOnOs;
	using OS = org.junit.jupiter.api.condition.OS;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using Predicates = Neo4Net.Functions.Predicates;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.dbms.archive.CompressionFormat.GZIP;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class DumperTest
	internal class DumperTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.Neo4Net.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGiveAClearErrorIfTheArchiveAlreadyExists() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldGiveAClearErrorIfTheArchiveAlreadyExists()
		 {
			  Path directory = _testDirectory.directory( "a-directory" ).toPath();
			  Path archive = _testDirectory.file( "the-archive.dump" ).toPath();
			  Files.write( archive, new sbyte[0] );
			  FileAlreadyExistsException exception = assertThrows( typeof( FileAlreadyExistsException ), () => (new Dumper()).dump(directory, directory, archive, GZIP, Predicates.alwaysFalse()) );
			  assertEquals( archive.ToString(), exception.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGiveAClearErrorMessageIfTheDirectoryDoesntExist()
		 internal virtual void ShouldGiveAClearErrorMessageIfTheDirectoryDoesntExist()
		 {
			  Path directory = _testDirectory.file( "a-directory" ).toPath();
			  Path archive = _testDirectory.file( "the-archive.dump" ).toPath();
			  NoSuchFileException exception = assertThrows( typeof( NoSuchFileException ), () => (new Dumper()).dump(directory, directory, archive, GZIP, Predicates.alwaysFalse()) );
			  assertEquals( directory.ToString(), exception.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGiveAClearErrorMessageIfTheArchivesParentDirectoryDoesntExist()
		 internal virtual void ShouldGiveAClearErrorMessageIfTheArchivesParentDirectoryDoesntExist()
		 {
			  Path directory = _testDirectory.directory( "a-directory" ).toPath();
			  Path archive = _testDirectory.file( "subdir/the-archive.dump" ).toPath();
			  NoSuchFileException exception = assertThrows( typeof( NoSuchFileException ), () => (new Dumper()).dump(directory, directory, archive, GZIP, Predicates.alwaysFalse()) );
			  assertEquals( archive.Parent.ToString(), exception.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGiveAClearErrorMessageIfTheArchivesParentDirectoryIsAFile() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldGiveAClearErrorMessageIfTheArchivesParentDirectoryIsAFile()
		 {
			  Path directory = _testDirectory.directory( "a-directory" ).toPath();
			  Path archive = _testDirectory.file( "subdir/the-archive.dump" ).toPath();
			  Files.write( archive.Parent, new sbyte[0] );
			  FileSystemException exception = assertThrows( typeof( FileSystemException ), () => (new Dumper()).dump(directory, directory, archive, GZIP, Predicates.alwaysFalse()) );
			  assertEquals( archive.Parent.ToString() + ": Not a directory", exception.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @DisabledOnOs(org.junit.jupiter.api.condition.OS.WINDOWS) void shouldGiveAClearErrorMessageIfTheArchivesParentDirectoryIsNotWritable() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldGiveAClearErrorMessageIfTheArchivesParentDirectoryIsNotWritable()
		 {
			  Path directory = _testDirectory.directory( "a-directory" ).toPath();
			  Path archive = _testDirectory.file( "subdir/the-archive.dump" ).toPath();
			  Files.createDirectories( archive.Parent );
			  using ( System.IDisposable ignored = TestUtils.WithPermissions( archive.Parent, emptySet() ) )
			  {
					AccessDeniedException exception = assertThrows( typeof( AccessDeniedException ), () => (new Dumper()).dump(directory, directory, archive, GZIP, Predicates.alwaysFalse()) );
					assertEquals( archive.Parent.ToString(), exception.Message );
			  }
		 }
	}

}