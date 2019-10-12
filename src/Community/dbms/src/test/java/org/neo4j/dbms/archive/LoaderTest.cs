using System;

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
namespace Neo4Net.Dbms.archive
{
	using GzipCompressorOutputStream = org.apache.commons.compress.compressors.gzip.GzipCompressorOutputStream;
	using Test = org.junit.jupiter.api.Test;
	using DisabledOnOs = org.junit.jupiter.api.condition.DisabledOnOs;
	using OS = org.junit.jupiter.api.condition.OS;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.dbms.archive.TestUtils.withPermissions;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class LoaderTest
	internal class LoaderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGiveAClearErrorMessageIfTheArchiveDoesntExist()
		 internal virtual void ShouldGiveAClearErrorMessageIfTheArchiveDoesntExist()
		 {
			  Path archive = _testDirectory.file( "the-archive.dump" ).toPath();
			  Path destination = _testDirectory.file( "the-destination" ).toPath();
			  NoSuchFileException exception = assertThrows( typeof( NoSuchFileException ), () => (new Loader()).load(archive, destination, destination) );
			  assertEquals( archive.ToString(), exception.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGiveAClearErrorMessageIfTheArchiveIsNotInGzipFormat() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldGiveAClearErrorMessageIfTheArchiveIsNotInGzipFormat()
		 {
			  Path archive = _testDirectory.file( "the-archive.dump" ).toPath();
			  Files.write( archive, singletonList( "some incorrectly formatted data" ) );
			  Path destination = _testDirectory.file( "the-destination" ).toPath();
			  IncorrectFormat incorrectFormat = assertThrows( typeof( IncorrectFormat ), () => (new Loader()).load(archive, destination, destination) );
			  assertEquals( archive.ToString(), incorrectFormat.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGiveAClearErrorMessageIfTheArchiveIsNotInTarFormat() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldGiveAClearErrorMessageIfTheArchiveIsNotInTarFormat()
		 {
			  Path archive = _testDirectory.file( "the-archive.dump" ).toPath();
			  using ( GzipCompressorOutputStream compressor = new GzipCompressorOutputStream( Files.newOutputStream( archive ) ) )
			  {
					sbyte[] bytes = new sbyte[1000];
					( new Random() ).NextBytes(bytes);
					compressor.write( bytes );
			  }

			  Path destination = _testDirectory.file( "the-destination" ).toPath();

			  IncorrectFormat incorrectFormat = assertThrows( typeof( IncorrectFormat ), () => (new Loader()).load(archive, destination, destination) );
			  assertEquals( archive.ToString(), incorrectFormat.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGiveAClearErrorIfTheDestinationAlreadyExists()
		 internal virtual void ShouldGiveAClearErrorIfTheDestinationAlreadyExists()
		 {
			  Path archive = _testDirectory.file( "the-archive.dump" ).toPath();
			  Path destination = _testDirectory.directory( "the-destination" ).toPath();
			  FileAlreadyExistsException exception = assertThrows( typeof( FileAlreadyExistsException ), () => (new Loader()).load(archive, destination, destination) );
			  assertEquals( destination.ToString(), exception.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGiveAClearErrorIfTheDestinationTxLogAlreadyExists()
		 internal virtual void ShouldGiveAClearErrorIfTheDestinationTxLogAlreadyExists()
		 {
			  Path archive = _testDirectory.file( "the-archive.dump" ).toPath();
			  Path destination = _testDirectory.file( "the-destination" ).toPath();
			  Path txLogsDestination = _testDirectory.directory( "txLogsDestination" ).toPath();

			  FileAlreadyExistsException exception = assertThrows( typeof( FileAlreadyExistsException ), () => (new Loader()).load(archive, destination, txLogsDestination) );
			  assertEquals( txLogsDestination.ToString(), exception.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGiveAClearErrorMessageIfTheDestinationsParentDirectoryDoesntExist() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldGiveAClearErrorMessageIfTheDestinationsParentDirectoryDoesntExist()
		 {
			  Path archive = _testDirectory.file( "the-archive.dump" ).toPath();
			  Path destination = Paths.get( _testDirectory.absolutePath().AbsolutePath, "subdir", "the-destination" );
			  NoSuchFileException noSuchFileException = assertThrows( typeof( NoSuchFileException ), () => (new Loader()).load(archive, destination, destination) );
			  assertEquals( destination.Parent.ToString(), noSuchFileException.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGiveAClearErrorMessageIfTheTxLogsParentDirectoryDoesntExist()
		 internal virtual void ShouldGiveAClearErrorMessageIfTheTxLogsParentDirectoryDoesntExist()
		 {
			  Path archive = _testDirectory.file( "the-archive.dump" ).toPath();
			  Path destination = _testDirectory.file( "destination" ).toPath();
			  Path txLogsDestination = Paths.get( _testDirectory.absolutePath().AbsolutePath, "subdir", "txLogs" );
			  NoSuchFileException noSuchFileException = assertThrows( typeof( NoSuchFileException ), () => (new Loader()).load(archive, destination, txLogsDestination) );
			  assertEquals( txLogsDestination.Parent.ToString(), noSuchFileException.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGiveAClearErrorMessageIfTheDestinationsParentDirectoryIsAFile() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldGiveAClearErrorMessageIfTheDestinationsParentDirectoryIsAFile()
		 {
			  Path archive = _testDirectory.file( "the-archive.dump" ).toPath();
			  Path destination = Paths.get( _testDirectory.absolutePath().AbsolutePath, "subdir", "the-destination" );
			  Files.write( destination.Parent, new sbyte[0] );
			  FileSystemException exception = assertThrows( typeof( FileSystemException ), () => (new Loader()).load(archive, destination, destination) );
			  assertEquals( destination.Parent.ToString() + ": Not a directory", exception.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @DisabledOnOs(org.junit.jupiter.api.condition.OS.WINDOWS) void shouldGiveAClearErrorMessageIfTheDestinationsParentDirectoryIsNotWritable() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldGiveAClearErrorMessageIfTheDestinationsParentDirectoryIsNotWritable()
		 {
			  Path archive = _testDirectory.file( "the-archive.dump" ).toPath();
			  Path destination = _testDirectory.directory( "subdir/the-destination" ).toPath();
			  Files.createDirectories( destination.Parent );
			  using ( System.IDisposable ignored = withPermissions( destination.Parent, emptySet() ) )
			  {
					AccessDeniedException exception = assertThrows( typeof( AccessDeniedException ), () => (new Loader()).load(archive, destination, destination) );
					assertEquals( destination.Parent.ToString(), exception.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @DisabledOnOs(org.junit.jupiter.api.condition.OS.WINDOWS) void shouldGiveAClearErrorMessageIfTheTxLogsParentDirectoryIsNotWritable() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldGiveAClearErrorMessageIfTheTxLogsParentDirectoryIsNotWritable()
		 {
			  Path archive = _testDirectory.file( "the-archive.dump" ).toPath();
			  Path destination = _testDirectory.file( "destination" ).toPath();
			  Path txLogsDirectory = _testDirectory.directory( "subdir/txLogs" ).toPath();
			  Files.createDirectories( txLogsDirectory.Parent );
			  using ( System.IDisposable ignored = withPermissions( txLogsDirectory.Parent, emptySet() ) )
			  {
					AccessDeniedException exception = assertThrows( typeof( AccessDeniedException ), () => (new Loader()).load(archive, destination, txLogsDirectory) );
					assertEquals( txLogsDirectory.Parent.ToString(), exception.Message );
			  }
		 }
	}

}