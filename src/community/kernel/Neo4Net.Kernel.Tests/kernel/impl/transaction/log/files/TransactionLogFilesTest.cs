using System;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.transaction.log.files
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;
	using Neo4Net.Test.rule.fs;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class TransactionLogFilesTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.FileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public readonly FileSystemRule FileSystemRule = new DefaultFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
		 private readonly string _filename = "filename";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetTheFileNameForAGivenVersion() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGetTheFileNameForAGivenVersion()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LogFiles files = createLogFiles();
			  LogFiles files = CreateLogFiles();
			  const int version = 12;

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File versionFileName = files.getLogFileForVersion(version);
			  File versionFileName = Files.getLogFileForVersion( version );

			  // then
			  DatabaseLayout databaseLayout = TestDirectory.databaseLayout();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File expected = databaseLayout.file(getVersionedLogFileName(version));
			  File expected = databaseLayout.File( GetVersionedLogFileName( version ) );
			  assertEquals( expected, versionFileName );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldVisitEachLofFile() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldVisitEachLofFile()
		 {
			  // given
			  LogFiles files = CreateLogFiles();
			  DatabaseLayout databaseLayout = TestDirectory.databaseLayout();

			  FileSystemRule.create( databaseLayout.File( GetVersionedLogFileName( "1" ) ) ).close();
			  FileSystemRule.create( databaseLayout.File( GetVersionedLogFileName( "some", "2" ) ) ).close();
			  FileSystemRule.create( databaseLayout.File( GetVersionedLogFileName( "3" ) ) ).close();
			  FileSystemRule.create( databaseLayout.File( _filename ) ).close();

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<java.io.File> seenFiles = new java.util.ArrayList<>();
			  IList<File> seenFiles = new List<File>();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<long> seenVersions = new java.util.ArrayList<>();
			  IList<long> seenVersions = new List<long>();

			  Files.accept((file, logVersion) =>
			  {
				seenFiles.Add( file );
				seenVersions.Add( logVersion );
			  });

			  // then
			  assertThat( seenFiles, containsInAnyOrder( databaseLayout.File( GetVersionedLogFileName( _filename, "1" ) ), databaseLayout.File( GetVersionedLogFileName( _filename, "3" ) ) ) );
			  assertThat( seenVersions, containsInAnyOrder( 1L, 3L ) );
			  Files.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRetrieveTheHighestLogVersion() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToRetrieveTheHighestLogVersion()
		 {
			  // given
			  LogFiles files = CreateLogFiles();

			  DatabaseLayout databaseLayout = TestDirectory.databaseLayout();
			  FileSystemRule.create( databaseLayout.File( GetVersionedLogFileName( "1" ) ) ).close();
			  FileSystemRule.create( databaseLayout.File( GetVersionedLogFileName( "some", "4" ) ) ).close();
			  FileSystemRule.create( databaseLayout.File( GetVersionedLogFileName( "3" ) ) ).close();
			  FileSystemRule.create( databaseLayout.File( _filename ) ).close();

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long highestLogVersion = files.getHighestLogVersion();
			  long highestLogVersion = Files.HighestLogVersion;

			  // then
			  assertEquals( 3, highestLogVersion );
			  Files.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnANegativeValueIfThereAreNoLogFiles() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnANegativeValueIfThereAreNoLogFiles()
		 {
			  // given
			  LogFiles files = CreateLogFiles();
			  DatabaseLayout databaseLayout = TestDirectory.databaseLayout();

			  FileSystemRule.create( databaseLayout.File( GetVersionedLogFileName( "some", "4" ) ) ).close();
			  FileSystemRule.create( databaseLayout.File( _filename ) ).close();

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long highestLogVersion = files.getHighestLogVersion();
			  long highestLogVersion = Files.HighestLogVersion;

			  // then
			  assertEquals( -1, highestLogVersion );
			  Files.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindTheVersionBasedOnTheFilename() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindTheVersionBasedOnTheFilename()
		 {
			  // given
			  LogFiles logFiles = CreateLogFiles();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File file = new java.io.File("v....2");
			  File file = new File( "v....2" );

			  // when
			  long logVersion = logFiles.GetLogVersion( file );

			  // then
			  assertEquals( 2, logVersion );
			  logFiles.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowIfThereIsNoVersionInTheFileName() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowIfThereIsNoVersionInTheFileName()
		 {
			  LogFiles logFiles = CreateLogFiles();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File file = new java.io.File("wrong");
			  File file = new File( "wrong" );

			  // when
			  try
			  {
					logFiles.GetLogVersion( file );
					fail( "should have thrown" );
			  }
			  catch ( Exception ex )
			  {
					assertEquals( "Invalid log file '" + file.Name + "'", ex.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = NumberFormatException.class) public void shouldThrowIfVersionIsNotANumber() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowIfVersionIsNotANumber()
		 {
			  // given
			  LogFiles logFiles = CreateLogFiles();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File file = new java.io.File(getVersionedLogFileName("aa", "A"));
			  File file = new File( GetVersionedLogFileName( "aa", "A" ) );

			  // when
			  logFiles.GetLogVersion( file );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void isLogFile() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void isLogFile()
		 {
			  LogFiles logFiles = CreateLogFiles();
			  assertFalse( logFiles.IsLogFile( new File( "aaa.tx.log" ) ) );
			  assertTrue( logFiles.IsLogFile( new File( "filename.0" ) ) );
			  assertTrue( logFiles.IsLogFile( new File( "filename.17" ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private LogFiles createLogFiles() throws java.io.IOException
		 private LogFiles CreateLogFiles()
		 {
			  return LogFilesBuilder.Builder( TestDirectory.databaseLayout(), FileSystemRule ).withLogFileName(_filename).withTransactionIdStore(new SimpleTransactionIdStore()).withLogVersionRepository(new SimpleLogVersionRepository()).build();
		 }

		 private string GetVersionedLogFileName( int version )
		 {
			  return GetVersionedLogFileName( _filename, version.ToString() );
		 }

		 private string GetVersionedLogFileName( string version )
		 {
			  return GetVersionedLogFileName( _filename, version );
		 }

		 private string GetVersionedLogFileName( string filename, string version )
		 {
			  return filename + "." + version;
		 }
	}

}