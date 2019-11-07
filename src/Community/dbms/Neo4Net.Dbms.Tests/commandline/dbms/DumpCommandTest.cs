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
namespace Neo4Net.Dbms.CommandLine
{
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using DisabledOnOs = org.junit.jupiter.api.condition.DisabledOnOs;
	using OS = org.junit.jupiter.api.condition.OS;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using CommandFailed = Neo4Net.CommandLine.Admin.CommandFailed;
	using CommandLocator = Neo4Net.CommandLine.Admin.CommandLocator;
	using IncorrectUsage = Neo4Net.CommandLine.Admin.IncorrectUsage;
	using Usage = Neo4Net.CommandLine.Admin.Usage;
	using Dumper = Neo4Net.Dbms.archive.Dumper;
	using Neo4Net.GraphDb.config;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using StoreLayout = Neo4Net.Io.layout.StoreLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using TransactionLogFiles = Neo4Net.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using StoreLocker = Neo4Net.Kernel.Internal.locker.StoreLocker;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.startsWith;
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
//	import static Neo4Net.dbms.archive.CompressionFormat.ZSTD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.dbms.archive.TestUtils.withPermissions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.GraphDatabaseSettings.data_directory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.GraphDatabaseSettings.logical_logs_location;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class DumpCommandTest
	internal class DumpCommandTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private Neo4Net.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;

		 private Path _homeDir;
		 private Path _configDir;
		 private Path _archive;
		 private Dumper _dumper;
		 private Path _databaseDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SetUp()
		 {
			  _homeDir = _testDirectory.directory( "home-dir" ).toPath();
			  _configDir = _testDirectory.directory( "config-dir" ).toPath();
			  _archive = _testDirectory.file( "some-archive.dump" ).toPath();
			  _dumper = mock( typeof( Dumper ) );
			  PutStoreInDirectory( _homeDir.resolve( "data/databases/foo.db" ) );
			  _databaseDirectory = _homeDir.resolve( "data/databases/foo.db" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDumpTheDatabaseToTheArchive() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldDumpTheDatabaseToTheArchive()
		 {
			  Execute( "foo.db" );
			  verify( _dumper ).dump( eq( _homeDir.resolve( "data/databases/foo.db" ) ), eq( _homeDir.resolve( "data/databases/foo.db" ) ), eq( _archive ), eq( ZSTD ), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCalculateTheDatabaseDirectoryFromConfig() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCalculateTheDatabaseDirectoryFromConfig()
		 {
			  Path dataDir = _testDirectory.directory( "some-other-path" ).toPath();
			  Path databaseDir = dataDir.resolve( "databases/foo.db" );
			  PutStoreInDirectory( databaseDir );
			  Files.write( _configDir.resolve( Config.DEFAULT_CONFIG_FILE_NAME ), singletonList( FormatProperty( data_directory, dataDir ) ) );

			  Execute( "foo.db" );
			  verify( _dumper ).dump( eq( databaseDir ), eq( databaseDir ), any(), any(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCalculateTheTxLogDirectoryFromConfig() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCalculateTheTxLogDirectoryFromConfig()
		 {
			  Path dataDir = _testDirectory.directory( "some-other-path" ).toPath();
			  Path txLogsDir = _testDirectory.directory( "txLogsPath" ).toPath();
			  Path databaseDir = dataDir.resolve( "databases/foo.db" );
			  PutStoreInDirectory( databaseDir );
			  Files.write( _configDir.resolve( Config.DEFAULT_CONFIG_FILE_NAME ), asList( FormatProperty( data_directory, dataDir ), FormatProperty( logical_logs_location, txLogsDir ) ) );

			  Execute( "foo.db" );
			  verify( _dumper ).dump( eq( databaseDir ), eq( txLogsDir ), any(), any(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @DisabledOnOs(org.junit.jupiter.api.condition.OS.WINDOWS) void shouldHandleDatabaseSymlink() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldHandleDatabaseSymlink()
		 {
			  Path symDir = _testDirectory.directory( "path-to-links" ).toPath();
			  Path realDatabaseDir = symDir.resolve( "foo.db" );

			  Path dataDir = _testDirectory.directory( "some-other-path" ).toPath();
			  Path databaseDir = dataDir.resolve( "databases/foo.db" );

			  PutStoreInDirectory( realDatabaseDir );
			  Files.createDirectories( dataDir.resolve( "databases" ) );

			  Files.createSymbolicLink( databaseDir, realDatabaseDir );
			  Files.write( _configDir.resolve( Config.DEFAULT_CONFIG_FILE_NAME ), singletonList( format( "%s=%s", data_directory.name(), dataDir.ToString().Replace('\\', '/') ) ) );

			  Execute( "foo.db" );
			  verify( _dumper ).dump( eq( realDatabaseDir ), eq( realDatabaseDir ), any(), any(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCalculateTheArchiveNameIfPassedAnExistingDirectory() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCalculateTheArchiveNameIfPassedAnExistingDirectory()
		 {
			  File to = _testDirectory.directory( "some-dir" );
			  ( new DumpCommand( _homeDir, _configDir, _dumper ) ).execute( new string[]{ "--database=" + "foo.db", "--to=" + to } );
			  verify( _dumper ).dump( any( typeof( Path ) ), any( typeof( Path ) ), eq( to.toPath().resolve("foo.db.dump") ), any(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldConvertToCanonicalPath() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldConvertToCanonicalPath()
		 {
			  ( new DumpCommand( _homeDir, _configDir, _dumper ) ).execute( new string[]{ "--database=" + "foo.db", "--to=foo.dump" } );
			  verify( _dumper ).dump( any( typeof( Path ) ), any( typeof( Path ) ), eq( Paths.get( ( new File( "foo.dump" ) ).CanonicalPath ) ), any(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotCalculateTheArchiveNameIfPassedAnExistingFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldNotCalculateTheArchiveNameIfPassedAnExistingFile()
		 {
			  Files.createFile( _archive );
			  Execute( "foo.db" );
			  verify( _dumper ).dump( any(), any(), eq(_archive), any(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRespectTheStoreLock() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRespectTheStoreLock()
		 {
			  Path databaseDirectory = _homeDir.resolve( "data/databases/foo.db" );
			  StoreLayout storeLayout = DatabaseLayout.of( databaseDirectory.toFile() ).StoreLayout;
			  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction(), StoreLocker storeLocker = new StoreLocker(fileSystem, storeLayout) )
			  {
					storeLocker.CheckLock();

					CommandFailed commandFailed = assertThrows( typeof( CommandFailed ), () => execute("foo.db") );
					assertEquals( "the database is in use -- stop Neo4Net and try again", commandFailed.Message );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void databaseThatRequireRecoveryIsNotDumpable() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void DatabaseThatRequireRecoveryIsNotDumpable()
		 {
			  File logFile = new File( _databaseDirectory.toFile(), TransactionLogFiles.DEFAULT_NAME + ".0" );
			  using ( StreamWriter fileWriter = new StreamWriter( logFile ) )
			  {
					fileWriter.Write( "brb" );
			  }
			  CommandFailed commandFailed = assertThrows( typeof( CommandFailed ), () => execute("foo.db") );
			  assertThat( commandFailed.Message, startsWith( "Active logical log detected, this might be a source of inconsistencies." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReleaseTheStoreLockAfterDumping() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReleaseTheStoreLockAfterDumping()
		 {
			  Execute( "foo.db" );
			  AssertCanLockStore( _databaseDirectory );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReleaseTheStoreLockEvenIfThereIsAnError() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReleaseTheStoreLockEvenIfThereIsAnError()
		 {
			  doThrow( typeof( IOException ) ).when( _dumper ).dump( any(), any(), any(), any(), any() );
			  assertThrows( typeof( CommandFailed ), () => execute("foo.db") );
			  AssertCanLockStore( _databaseDirectory );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotAccidentallyCreateTheDatabaseDirectoryAsASideEffectOfStoreLocking() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldNotAccidentallyCreateTheDatabaseDirectoryAsASideEffectOfStoreLocking()
		 {
			  Path databaseDirectory = _homeDir.resolve( "data/databases/accident.db" );

			  doAnswer(ignored =>
			  {
				assertThat( Files.exists( databaseDirectory ), equalTo( false ) );
				return null;
			  }).when( _dumper ).dump( any(), any(), any(), any(), any() );

			  Execute( "foo.db" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @DisabledOnOs(org.junit.jupiter.api.condition.OS.WINDOWS) void shouldReportAHelpfulErrorIfWeDontHaveWritePermissionsForLock() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReportAHelpfulErrorIfWeDontHaveWritePermissionsForLock()
		 {
			  StoreLayout storeLayout = DatabaseLayout.of( _databaseDirectory.toFile() ).StoreLayout;
			  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction(), StoreLocker storeLocker = new StoreLocker(fileSystem, storeLayout) )
			  {
					storeLocker.CheckLock();

					using ( System.IDisposable ignored = withPermissions( storeLayout.StoreLockFile().toPath(), emptySet() ) )
					{
						 CommandFailed commandFailed = assertThrows( typeof( CommandFailed ), () => execute("foo.db") );
						 assertEquals( commandFailed.Message, "you do not have permission to dump the database -- is Neo4Net running as a different user?" );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldExcludeTheStoreLockFromTheArchiveToAvoidProblemsWithReadingLockedFilesOnWindows() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldExcludeTheStoreLockFromTheArchiveToAvoidProblemsWithReadingLockedFilesOnWindows()
		 {
			  File lockFile = StoreLayout.of( new File( "." ) ).storeLockFile();
			  doAnswer(invocation =>
			  {
				Predicate<Path> exclude = invocation.getArgument( 4 );
				assertThat( exclude.test( Paths.get( lockFile.Name ) ), @is( true ) );
				assertThat( exclude.test( Paths.get( "some-other-file" ) ), @is( false ) );
				return null;
			  }).when( _dumper ).dump( any(), any(), any(), any(), any() );

			  Execute( "foo.db" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDefaultToGraphDB() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldDefaultToGraphDB()
		 {
			  Path dataDir = _testDirectory.directory( "some-other-path" ).toPath();
			  Path databaseDir = dataDir.resolve( "databases/" + GraphDatabaseSettings.DEFAULT_DATABASE_NAME );
			  PutStoreInDirectory( databaseDir );
			  Files.write( _configDir.resolve( Config.DEFAULT_CONFIG_FILE_NAME ), singletonList( FormatProperty( data_directory, dataDir ) ) );

			  ( new DumpCommand( _homeDir, _configDir, _dumper ) ).execute( new string[]{ "--to=" + _archive } );
			  verify( _dumper ).dump( eq( databaseDir ), eq( databaseDir ), any(), any(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldObjectIfTheArchiveArgumentIsMissing()
		 internal virtual void ShouldObjectIfTheArchiveArgumentIsMissing()
		 {

			  System.ArgumentException exception = assertThrows( typeof( System.ArgumentException ), () => (new DumpCommand(_homeDir, _configDir, null)).execute(new string[]{ "--database=something" }) );
			  assertEquals( "Missing argument 'to'", exception.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGiveAClearErrorIfTheArchiveAlreadyExists() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldGiveAClearErrorIfTheArchiveAlreadyExists()
		 {
			  doThrow( new FileAlreadyExistsException( "the-archive-path" ) ).when( _dumper ).dump( any(), any(), any(), any(), any() );
			  CommandFailed commandFailed = assertThrows( typeof( CommandFailed ), () => execute("foo.db") );
			  assertEquals( "archive already exists: the-archive-path", commandFailed.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGiveAClearMessageIfTheDatabaseDoesntExist()
		 internal virtual void ShouldGiveAClearMessageIfTheDatabaseDoesntExist()
		 {
			  CommandFailed commandFailed = assertThrows( typeof( CommandFailed ), () => execute("bobo.db") );
			  assertEquals( "database does not exist: bobo.db", commandFailed.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGiveAClearMessageIfTheArchivesParentDoesntExist() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldGiveAClearMessageIfTheArchivesParentDoesntExist()
		 {
			  doThrow( new NoSuchFileException( _archive.Parent.ToString() ) ).when(_dumper).dump(any(), any(), any(), any(), any());
			  CommandFailed commandFailed = assertThrows( typeof( CommandFailed ), () => execute("foo.db") );
			  assertEquals( "unable to dump database: NoSuchFileException: " + _archive.Parent, commandFailed.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldWrapIOExceptionsCarefullyBecauseCriticalInformationIsOftenEncodedInTheirNameButMissingFromTheirMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldWrapIOExceptionsCarefullyBecauseCriticalInformationIsOftenEncodedInTheirNameButMissingFromTheirMessage()
		 {
			  doThrow( new IOException( "the-message" ) ).when( _dumper ).dump( any(), any(), any(), any(), any() );
			  CommandFailed commandFailed = assertThrows( typeof( CommandFailed ), () => execute("foo.db") );
			  assertEquals( "unable to dump database: IOException: the-message", commandFailed.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldPrintNiceHelp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldPrintNiceHelp()
		 {
			  using ( MemoryStream baos = new MemoryStream() )
			  {
					PrintStream ps = new PrintStream( baos );

					Usage usage = new Usage( "Neo4Net-admin", mock( typeof( CommandLocator ) ) );
					usage.PrintUsageForCommand( new DumpCommandProvider(), ps.println );

					assertEquals( string.Format( "usage: Neo4Net-admin dump [--database=<name>] --to=<destination-path>%n" + "%n" + "environment variables:%n" + "    Neo4Net_CONF    Path to directory which contains Neo4Net.conf.%n" + "    Neo4Net_DEBUG   Set to anything to enable debug output.%n" + "    Neo4Net_HOME    Neo4Net home directory.%n" + "    HEAP_SIZE     Set JVM maximum heap size during command execution.%n" + "                  Takes a number and a unit, for example 512m.%n" + "%n" + "Dump a database into a single-file archive. The archive can be used by the load%n" + "command. <destination-path> can be a file or directory (in which case a file%n" + "called <database>.dump will be created). It is not possible to dump a database%n" + "that is mounted in a running Neo4Net server.%n" + "%n" + "options:%n" + "  --database=<name>         Name of database. [default:" + GraphDatabaseSettings.DEFAULT_DATABASE_NAME + "]%n" + "  --to=<destination-path>   Destination (file or folder) of database dump.%n" ), baos.ToString() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void execute(final String database) throws Neo4Net.commandline.admin.IncorrectUsage, Neo4Net.commandline.admin.CommandFailed
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private void Execute( string database )
		 {
			  ( new DumpCommand( _homeDir, _configDir, _dumper ) ).execute( new string[]{ "--database=" + database, "--to=" + _archive } );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void assertCanLockStore(java.nio.file.Path databaseDirectory) throws java.io.IOException
		 private static void AssertCanLockStore( Path databaseDirectory )
		 {
			  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction(), StoreLocker storeLocker = new StoreLocker(fileSystem, DatabaseLayout.of(databaseDirectory.toFile()).StoreLayout) )
			  {
					storeLocker.CheckLock();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void putStoreInDirectory(java.nio.file.Path databaseDirectory) throws java.io.IOException
		 private static void PutStoreInDirectory( Path databaseDirectory )
		 {
			  Files.createDirectories( databaseDirectory );
			  Path storeFile = DatabaseLayout.of( databaseDirectory.toFile() ).metadataStore().toPath();
			  Files.createFile( storeFile );
		 }

		 private static string FormatProperty( Setting setting, Path path )
		 {
			  return format( "%s=%s", setting.name(), path.ToString().Replace('\\', '/') );
		 }
	}

}