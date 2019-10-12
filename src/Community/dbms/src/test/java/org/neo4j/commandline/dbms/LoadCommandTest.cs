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
namespace Neo4Net.Commandline.dbms
{
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using DisabledOnOs = org.junit.jupiter.api.condition.DisabledOnOs;
	using OS = org.junit.jupiter.api.condition.OS;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using CommandFailed = Neo4Net.Commandline.Admin.CommandFailed;
	using CommandLocator = Neo4Net.Commandline.Admin.CommandLocator;
	using IncorrectUsage = Neo4Net.Commandline.Admin.IncorrectUsage;
	using Usage = Neo4Net.Commandline.Admin.Usage;
	using IncorrectFormat = Neo4Net.Dbms.archive.IncorrectFormat;
	using Loader = Neo4Net.Dbms.archive.Loader;
	using Neo4Net.Graphdb.config;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using ArrayUtil = Neo4Net.Helpers.ArrayUtil;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using StoreLayout = Neo4Net.Io.layout.StoreLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using StoreLocker = Neo4Net.Kernel.@internal.locker.StoreLocker;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.data_directory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.logical_logs_location;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class LoadCommandTest
	internal class LoadCommandTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;
		 private Path _homeDir;
		 private Path _configDir;
		 private Path _archive;
		 private Loader _loader;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp()
		 internal virtual void SetUp()
		 {
			  _homeDir = _testDirectory.directory( "home-dir" ).toPath();
			  _configDir = _testDirectory.directory( "config-dir" ).toPath();
			  _archive = _testDirectory.directory( "some-archive.dump" ).toPath();
			  _loader = mock( typeof( Loader ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldLoadTheDatabaseFromTheArchive() throws org.neo4j.commandline.admin.CommandFailed, org.neo4j.commandline.admin.IncorrectUsage, java.io.IOException, org.neo4j.dbms.archive.IncorrectFormat
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldLoadTheDatabaseFromTheArchive()
		 {
			  Execute( "foo.db" );
			  verify( _loader ).load( _archive, _homeDir.resolve( "data/databases/foo.db" ), _homeDir.resolve( "data/databases/foo.db" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCalculateTheDatabaseDirectoryFromConfig() throws java.io.IOException, org.neo4j.commandline.admin.CommandFailed, org.neo4j.commandline.admin.IncorrectUsage, org.neo4j.dbms.archive.IncorrectFormat
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCalculateTheDatabaseDirectoryFromConfig()
		 {
			  Path dataDir = _testDirectory.directory( "some-other-path" ).toPath();
			  Path databaseDir = dataDir.resolve( "databases/foo.db" );
			  Files.createDirectories( databaseDir );
			  Files.write( _configDir.resolve( Config.DEFAULT_CONFIG_FILE_NAME ), singletonList( FormatProperty( data_directory, dataDir ) ) );

			  Execute( "foo.db" );
			  verify( _loader ).load( any(), eq(databaseDir), eq(databaseDir) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCalculateTheTxLogDirectoryFromConfig() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCalculateTheTxLogDirectoryFromConfig()
		 {
			  Path dataDir = _testDirectory.directory( "some-other-path" ).toPath();
			  Path txLogsDir = _testDirectory.directory( "txLogsPath" ).toPath();
			  Path databaseDir = dataDir.resolve( "databases/foo.db" );
			  Files.write( _configDir.resolve( Config.DEFAULT_CONFIG_FILE_NAME ), asList( FormatProperty( data_directory, dataDir ), FormatProperty( logical_logs_location, txLogsDir ) ) );

			  Execute( "foo.db" );
			  verify( _loader ).load( any(), eq(databaseDir), eq(txLogsDir) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @DisabledOnOs(org.junit.jupiter.api.condition.OS.WINDOWS) void shouldHandleSymlinkToDatabaseDir() throws java.io.IOException, org.neo4j.commandline.admin.CommandFailed, org.neo4j.commandline.admin.IncorrectUsage, org.neo4j.dbms.archive.IncorrectFormat
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldHandleSymlinkToDatabaseDir()
		 {
			  Path symDir = _testDirectory.directory( "path-to-links" ).toPath();
			  Path realDatabaseDir = symDir.resolve( "foo.db" );

			  Path dataDir = _testDirectory.directory( "some-other-path" ).toPath();
			  Path databaseDir = dataDir.resolve( "databases/foo.db" );

			  Files.createDirectories( realDatabaseDir );
			  Files.createDirectories( dataDir.resolve( "databases" ) );

			  Files.createSymbolicLink( databaseDir, realDatabaseDir );

			  Files.write( _configDir.resolve( Config.DEFAULT_CONFIG_FILE_NAME ), singletonList( FormatProperty( data_directory, dataDir ) ) );

			  Execute( "foo.db" );
			  verify( _loader ).load( any(), eq(realDatabaseDir), eq(realDatabaseDir) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldMakeFromCanonical() throws java.io.IOException, org.neo4j.commandline.admin.CommandFailed, org.neo4j.commandline.admin.IncorrectUsage, org.neo4j.dbms.archive.IncorrectFormat
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldMakeFromCanonical()
		 {
			  Path dataDir = _testDirectory.directory( "some-other-path" ).toPath();
			  Path databaseDir = dataDir.resolve( "databases/foo.db" );
			  Files.createDirectories( databaseDir );
			  Files.write( _configDir.resolve( Config.DEFAULT_CONFIG_FILE_NAME ), singletonList( FormatProperty( data_directory, dataDir ) ) );

			  ( new LoadCommand( _homeDir, _configDir, _loader ) ).execute( ArrayUtil.concat( new string[]{ "--database=foo.db", "--from=foo.dump" } ) );

			  verify( _loader ).load( eq( Paths.get( ( new File( "foo.dump" ) ).CanonicalPath ) ), any(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDeleteTheOldDatabaseIfForceArgumentIsProvided() throws org.neo4j.commandline.admin.CommandFailed, org.neo4j.commandline.admin.IncorrectUsage, java.io.IOException, org.neo4j.dbms.archive.IncorrectFormat
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldDeleteTheOldDatabaseIfForceArgumentIsProvided()
		 {
			  Path databaseDirectory = _homeDir.resolve( "data/databases/foo.db" );
			  Files.createDirectories( databaseDirectory );

			  doAnswer(ignored =>
			  {
				assertThat( Files.exists( databaseDirectory ), equalTo( false ) );
				return null;
			  }).when( _loader ).load( any(), any(), any() );

			  Execute( "foo.db", "--force" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotDeleteTheOldDatabaseIfForceArgumentIsNotProvided() throws org.neo4j.commandline.admin.CommandFailed, org.neo4j.commandline.admin.IncorrectUsage, java.io.IOException, org.neo4j.dbms.archive.IncorrectFormat
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldNotDeleteTheOldDatabaseIfForceArgumentIsNotProvided()
		 {
			  Path databaseDirectory = _homeDir.resolve( "data/databases/foo.db" );
			  Files.createDirectories( databaseDirectory );

			  doAnswer(ignored =>
			  {
				assertThat( Files.exists( databaseDirectory ), equalTo( true ) );
				return null;
			  }).when( _loader ).load( any(), any(), any() );

			  Execute( "foo.db" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRespectTheStoreLock() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRespectTheStoreLock()
		 {
			  Path databaseDirectory = _homeDir.resolve( "data/databases/foo.db" );
			  Files.createDirectories( databaseDirectory );
			  StoreLayout storeLayout = DatabaseLayout.of( databaseDirectory.toFile() ).StoreLayout;

			  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction(), StoreLocker locker = new StoreLocker(fileSystem, storeLayout) )
			  {
					locker.CheckLock();
					CommandFailed commandFailed = assertThrows( typeof( CommandFailed ), () => execute("foo.db", "--force") );
					assertEquals( "the database is in use -- stop Neo4j and try again", commandFailed.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDefaultToGraphDb() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldDefaultToGraphDb()
		 {
			  Path databaseDir = _homeDir.resolve( "data/databases/" + GraphDatabaseSettings.DEFAULT_DATABASE_NAME );
			  Files.createDirectories( databaseDir );

			  ( new LoadCommand( _homeDir, _configDir, _loader ) ).execute( new string[]{ "--from=something" } );
			  verify( _loader ).load( any(), eq(databaseDir), eq(databaseDir) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldObjectIfTheArchiveArgumentIsMissing()
		 internal virtual void ShouldObjectIfTheArchiveArgumentIsMissing()
		 {
			  System.ArgumentException exception = assertThrows( typeof( System.ArgumentException ), () => (new LoadCommand(_homeDir, _configDir, _loader)).execute(new string[]{ "--database=something" }) );
			  assertEquals( "Missing argument 'from'", exception.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGiveAClearMessageIfTheArchiveDoesntExist() throws java.io.IOException, org.neo4j.dbms.archive.IncorrectFormat
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldGiveAClearMessageIfTheArchiveDoesntExist()
		 {
			  doThrow( new NoSuchFileException( _archive.ToString() ) ).when(_loader).load(any(), any(), any());
			  CommandFailed commandFailed = assertThrows( typeof( CommandFailed ), () => execute(null) );
			  assertEquals( "archive does not exist: " + _archive, commandFailed.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGiveAClearMessageIfTheDatabaseAlreadyExists() throws java.io.IOException, org.neo4j.dbms.archive.IncorrectFormat, org.neo4j.commandline.admin.IncorrectUsage
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldGiveAClearMessageIfTheDatabaseAlreadyExists()
		 {
			  doThrow( typeof( FileAlreadyExistsException ) ).when( _loader ).load( any(), any(), any() );
			  CommandFailed commandFailed = assertThrows( typeof( CommandFailed ), () => execute("foo.db") );
			  assertEquals( "database already exists: foo.db", commandFailed.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGiveAClearMessageIfTheDatabasesDirectoryIsNotWritable() throws java.io.IOException, org.neo4j.dbms.archive.IncorrectFormat
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldGiveAClearMessageIfTheDatabasesDirectoryIsNotWritable()
		 {
			  doThrow( typeof( AccessDeniedException ) ).when( _loader ).load( any(), any(), any() );
			  CommandFailed commandFailed = assertThrows( typeof( CommandFailed ), () => execute(null) );
			  assertEquals( "you do not have permission to load a database -- is Neo4j running as a different user?", commandFailed.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldWrapIOExceptionsCarefullyBecauseCriticalInformationIsOftenEncodedInTheirNameButMissingFromTheirMessage() throws java.io.IOException, org.neo4j.dbms.archive.IncorrectFormat
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldWrapIOExceptionsCarefullyBecauseCriticalInformationIsOftenEncodedInTheirNameButMissingFromTheirMessage()
		 {
			  doThrow( new FileSystemException( "the-message" ) ).when( _loader ).load( any(), any(), any() );
			  CommandFailed commandFailed = assertThrows( typeof( CommandFailed ), () => execute(null) );
			  assertEquals( "unable to load database: FileSystemException: the-message", commandFailed.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowIfTheArchiveFormatIsInvalid() throws java.io.IOException, org.neo4j.dbms.archive.IncorrectFormat
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldThrowIfTheArchiveFormatIsInvalid()
		 {
			  doThrow( typeof( IncorrectFormat ) ).when( _loader ).load( any(), any(), any() );
			  CommandFailed commandFailed = assertThrows( typeof( CommandFailed ), () => execute(null) );
			  assertThat( commandFailed.Message, containsString( _archive.ToString() ) );
			  assertThat( commandFailed.Message, containsString( "valid Neo4j archive" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldPrintNiceHelp() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldPrintNiceHelp()
		 {
			  using ( MemoryStream baos = new MemoryStream() )
			  {
					PrintStream ps = new PrintStream( baos );

					Usage usage = new Usage( "neo4j-admin", mock( typeof( CommandLocator ) ) );
					usage.PrintUsageForCommand( new LoadCommandProvider(), ps.println );

					assertEquals( string.Format( "usage: neo4j-admin load --from=<archive-path> [--database=<name>]%n" + "                        [--force[=<true|false>]]%n" + "%n" + "environment variables:%n" + "    NEO4J_CONF    Path to directory which contains neo4j.conf.%n" + "    NEO4J_DEBUG   Set to anything to enable debug output.%n" + "    NEO4J_HOME    Neo4j home directory.%n" + "    HEAP_SIZE     Set JVM maximum heap size during command execution.%n" + "                  Takes a number and a unit, for example 512m.%n" + "%n" + "Load a database from an archive. <archive-path> must be an archive created with%n" + "the dump command. <database> is the name of the database to create. Existing%n" + "databases can be replaced by specifying --force. It is not possible to replace a%n" + "database that is mounted in a running Neo4j server.%n" + "%n" + "options:%n" + "  --from=<archive-path>   Path to archive created with the dump command.%n" + "  --database=<name>       Name of database. [default:" + GraphDatabaseSettings.DEFAULT_DATABASE_NAME + "]%n" + "  --force=<true|false>    If an existing database should be replaced.%n" + "                          [default:false]%n" ), baos.ToString() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void execute(String database, String... otherArgs) throws org.neo4j.commandline.admin.IncorrectUsage, org.neo4j.commandline.admin.CommandFailed
		 private void Execute( string database, params string[] otherArgs )
		 {
			  ( new LoadCommand( _homeDir, _configDir, _loader ) ).execute( ArrayUtil.concat( new string[]{ "--database=" + database, "--from=" + _archive }, otherArgs ) );
		 }

		 private static string FormatProperty( Setting setting, Path path )
		 {
			  return format( "%s=%s", setting.name(), path.ToString().Replace('\\', '/') );
		 }
	}

}