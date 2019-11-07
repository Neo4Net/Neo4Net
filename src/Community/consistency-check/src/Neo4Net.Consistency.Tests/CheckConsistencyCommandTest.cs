﻿/*
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
namespace Neo4Net.Consistency
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using CommandFailed = Neo4Net.CommandLine.Admin.CommandFailed;
	using CommandLocator = Neo4Net.CommandLine.Admin.CommandLocator;
	using IncorrectUsage = Neo4Net.CommandLine.Admin.IncorrectUsage;
	using Usage = Neo4Net.CommandLine.Admin.Usage;
	using ConsistencyCheckIncompleteException = Neo4Net.Consistency.checking.full.ConsistencyCheckIncompleteException;
	using ConsistencyFlags = Neo4Net.Consistency.checking.full.ConsistencyFlags;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using ProgressMonitorFactory = Neo4Net.Helpers.progress.ProgressMonitorFactory;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class CheckConsistencyCommandTest
	internal class CheckConsistencyCommandTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private Neo4Net.test.rule.TestDirectory testDir;
		 private TestDirectory _testDir;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void runsConsistencyChecker() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void RunsConsistencyChecker()
		 {
			  ConsistencyCheckService consistencyCheckService = mock( typeof( ConsistencyCheckService ) );

			  Path homeDir = _testDir.directory( "home" ).toPath();
			  File databasesFolder = GetDatabasesFolder( homeDir );
			  CheckConsistencyCommand checkConsistencyCommand = new CheckConsistencyCommand( homeDir, _testDir.directory( "conf" ).toPath(), consistencyCheckService );

			  DatabaseLayout databaseLayout = DatabaseLayout.of( databasesFolder, "mydb" );

			  when( consistencyCheckService.runFullConsistencyCheck( eq( databaseLayout ), any( typeof( Config ) ), any( typeof( ProgressMonitorFactory ) ), any( typeof( LogProvider ) ), any( typeof( FileSystemAbstraction ) ), eq( false ), any(), any(typeof(ConsistencyFlags)) ) ).thenReturn(ConsistencyCheckService.Result.success(null));

			  checkConsistencyCommand.Execute( new string[]{ "--database=mydb" } );

			  verify( consistencyCheckService ).runFullConsistencyCheck( eq( databaseLayout ), any( typeof( Config ) ), any( typeof( ProgressMonitorFactory ) ), any( typeof( LogProvider ) ), any( typeof( FileSystemAbstraction ) ), eq( false ), any(), any(typeof(ConsistencyFlags)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void enablesVerbosity() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void EnablesVerbosity()
		 {
			  ConsistencyCheckService consistencyCheckService = mock( typeof( ConsistencyCheckService ) );

			  Path homeDir = _testDir.directory( "home" ).toPath();
			  File databasesFolder = GetDatabasesFolder( homeDir );
			  CheckConsistencyCommand checkConsistencyCommand = new CheckConsistencyCommand( homeDir, _testDir.directory( "conf" ).toPath(), consistencyCheckService );

			  DatabaseLayout databaseLayout = DatabaseLayout.of( databasesFolder, "mydb" );

			  when( consistencyCheckService.runFullConsistencyCheck( eq( databaseLayout ), any( typeof( Config ) ), any( typeof( ProgressMonitorFactory ) ), any( typeof( LogProvider ) ), any( typeof( FileSystemAbstraction ) ), eq( true ), any(), any(typeof(ConsistencyFlags)) ) ).thenReturn(ConsistencyCheckService.Result.success(null));

			  checkConsistencyCommand.Execute( new string[]{ "--database=mydb", "--verbose" } );

			  verify( consistencyCheckService ).runFullConsistencyCheck( eq( databaseLayout ), any( typeof( Config ) ), any( typeof( ProgressMonitorFactory ) ), any( typeof( LogProvider ) ), any( typeof( FileSystemAbstraction ) ), eq( true ), any(), any(typeof(ConsistencyFlags)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void failsWhenInconsistenciesAreFound() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void FailsWhenInconsistenciesAreFound()
		 {
			  ConsistencyCheckService consistencyCheckService = mock( typeof( ConsistencyCheckService ) );

			  Path homeDir = _testDir.directory( "home" ).toPath();
			  File databasesFolder = GetDatabasesFolder( homeDir );
			  CheckConsistencyCommand checkConsistencyCommand = new CheckConsistencyCommand( homeDir, _testDir.directory( "conf" ).toPath(), consistencyCheckService );
			  DatabaseLayout databaseLayout = DatabaseLayout.of( databasesFolder, "mydb" );

			  when( consistencyCheckService.runFullConsistencyCheck( eq( databaseLayout ), any( typeof( Config ) ), any( typeof( ProgressMonitorFactory ) ), any( typeof( LogProvider ) ), any( typeof( FileSystemAbstraction ) ), eq( true ), any(), any(typeof(ConsistencyFlags)) ) ).thenReturn(ConsistencyCheckService.Result.failure(new File("/the/report/path")));

			  CommandFailed commandFailed = assertThrows( typeof( CommandFailed ), () => checkConsistencyCommand.execute(new string[]{ "--database=mydb", "--verbose" }) );
			  assertThat( commandFailed.Message, containsString( ( new File( "/the/report/path" ) ).ToString() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldWriteReportFileToCurrentDirectoryByDefault() throws java.io.IOException, Neo4Net.consistency.checking.full.ConsistencyCheckIncompleteException, Neo4Net.commandline.admin.CommandFailed, Neo4Net.commandline.admin.IncorrectUsage
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldWriteReportFileToCurrentDirectoryByDefault()

		 {
			  ConsistencyCheckService consistencyCheckService = mock( typeof( ConsistencyCheckService ) );

			  Path homeDir = _testDir.directory( "home" ).toPath();
			  CheckConsistencyCommand checkConsistencyCommand = new CheckConsistencyCommand( homeDir, _testDir.directory( "conf" ).toPath(), consistencyCheckService );

			  when( consistencyCheckService.runFullConsistencyCheck( any(), any(), any(), any(), any(), anyBoolean(), any(), any(typeof(ConsistencyFlags)) ) ).thenReturn(ConsistencyCheckService.Result.success(null));

			  checkConsistencyCommand.Execute( new string[]{ "--database=mydb" } );

			  verify( consistencyCheckService ).runFullConsistencyCheck( any(), any(), any(), any(), any(), anyBoolean(), eq((new File(".")).CanonicalFile), any(typeof(ConsistencyFlags)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldWriteReportFileToSpecifiedDirectory() throws java.io.IOException, Neo4Net.consistency.checking.full.ConsistencyCheckIncompleteException, Neo4Net.commandline.admin.CommandFailed, Neo4Net.commandline.admin.IncorrectUsage
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldWriteReportFileToSpecifiedDirectory()

		 {
			  ConsistencyCheckService consistencyCheckService = mock( typeof( ConsistencyCheckService ) );

			  Path homeDir = _testDir.directory( "home" ).toPath();
			  CheckConsistencyCommand checkConsistencyCommand = new CheckConsistencyCommand( homeDir, _testDir.directory( "conf" ).toPath(), consistencyCheckService );

			  when( consistencyCheckService.runFullConsistencyCheck( any(), any(), any(), any(), any(), anyBoolean(), any(), any(typeof(ConsistencyFlags)) ) ).thenReturn(ConsistencyCheckService.Result.success(null));

			  checkConsistencyCommand.Execute( new string[]{ "--database=mydb", "--report-dir=some-dir-or-other" } );

			  verify( consistencyCheckService ).runFullConsistencyCheck( any(), any(), any(), any(), any(), anyBoolean(), eq((new File("some-dir-or-other")).CanonicalFile), any(typeof(ConsistencyFlags)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCanonicalizeReportDirectory() throws java.io.IOException, Neo4Net.consistency.checking.full.ConsistencyCheckIncompleteException, Neo4Net.commandline.admin.CommandFailed, Neo4Net.commandline.admin.IncorrectUsage
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCanonicalizeReportDirectory()
		 {
			  ConsistencyCheckService consistencyCheckService = mock( typeof( ConsistencyCheckService ) );

			  Path homeDir = _testDir.directory( "home" ).toPath();
			  CheckConsistencyCommand checkConsistencyCommand = new CheckConsistencyCommand( homeDir, _testDir.directory( "conf" ).toPath(), consistencyCheckService );

			  when( consistencyCheckService.runFullConsistencyCheck( any(), any(), any(), any(), any(), anyBoolean(), any(), any(typeof(ConsistencyFlags)) ) ).thenReturn(ConsistencyCheckService.Result.success(null));

			  checkConsistencyCommand.Execute( new string[]{ "--database=mydb", "--report-dir=" + Paths.get( "..", "bar" ) } );

			  verify( consistencyCheckService ).runFullConsistencyCheck( any(), any(), any(), any(), any(), anyBoolean(), eq((new File("../bar")).CanonicalFile), any(typeof(ConsistencyFlags)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void passesOnCheckParameters() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void PassesOnCheckParameters()
		 {
			  ConsistencyCheckService consistencyCheckService = mock( typeof( ConsistencyCheckService ) );

			  Path homeDir = _testDir.directory( "home" ).toPath();
			  CheckConsistencyCommand checkConsistencyCommand = new CheckConsistencyCommand( homeDir, _testDir.directory( "conf" ).toPath(), consistencyCheckService );

			  when( consistencyCheckService.runFullConsistencyCheck( any(), any(), any(), any(), any(), anyBoolean(), any(), any(typeof(ConsistencyFlags)) ) ).thenReturn(ConsistencyCheckService.Result.success(null));

			  checkConsistencyCommand.Execute( new string[]{ "--database=mydb", "--check-graph=false", "--check-indexes=false", "--check-index-structure=false", "--check-label-scan-store=false", "--check-property-owners=true" } );

			  verify( consistencyCheckService ).runFullConsistencyCheck( any(), any(), any(), any(), any(), anyBoolean(), any(), eq(new ConsistencyFlags(false, false, false, false, true)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void databaseAndBackupAreMutuallyExclusive() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void DatabaseAndBackupAreMutuallyExclusive()
		 {
			  ConsistencyCheckService consistencyCheckService = mock( typeof( ConsistencyCheckService ) );

			  Path homeDir = _testDir.directory( "home" ).toPath();
			  CheckConsistencyCommand checkConsistencyCommand = new CheckConsistencyCommand( homeDir, _testDir.directory( "conf" ).toPath(), consistencyCheckService );

			  when( consistencyCheckService.runFullConsistencyCheck( any(), any(), any(), any(), any(), anyBoolean(), any(typeof(ConsistencyFlags)) ) ).thenReturn(ConsistencyCheckService.Result.success(null));

			  IncorrectUsage incorrectUsage = assertThrows( typeof( IncorrectUsage ), () => checkConsistencyCommand.execute(new string[]{ "--database=foo", "--backup=bar" }) );
			  assertEquals( "Only one of '--database' and '--backup' can be specified.", incorrectUsage.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void backupNeedsToBePath()
		 internal virtual void BackupNeedsToBePath()
		 {
			  ConsistencyCheckService consistencyCheckService = mock( typeof( ConsistencyCheckService ) );

			  Path homeDir = _testDir.directory( "home" ).toPath();
			  CheckConsistencyCommand checkConsistencyCommand = new CheckConsistencyCommand( homeDir, _testDir.directory( "conf" ).toPath(), consistencyCheckService );

			  File backupPath = new File( homeDir.toFile(), "dir/does/not/exist" );

			  CommandFailed commandFailed = assertThrows( typeof( CommandFailed ), () => checkConsistencyCommand.execute(new string[]{ "--backup=" + backupPath }) );
			  assertEquals( "Specified backup should be a directory: " + backupPath, commandFailed.Message );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void canRunOnBackup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CanRunOnBackup()
		 {
			  ConsistencyCheckService consistencyCheckService = mock( typeof( ConsistencyCheckService ) );

			  DatabaseLayout backupLayout = _testDir.databaseLayout( "backup" );
			  Path homeDir = _testDir.directory( "home" ).toPath();
			  CheckConsistencyCommand checkConsistencyCommand = new CheckConsistencyCommand( homeDir, _testDir.directory( "conf" ).toPath(), consistencyCheckService );

			  when( consistencyCheckService.runFullConsistencyCheck( eq( backupLayout ), any( typeof( Config ) ), any( typeof( ProgressMonitorFactory ) ), any( typeof( LogProvider ) ), any( typeof( FileSystemAbstraction ) ), eq( false ), any(), any(typeof(ConsistencyFlags)) ) ).thenReturn(ConsistencyCheckService.Result.success(null));

			  checkConsistencyCommand.Execute( new string[]{ "--backup=" + backupLayout.DatabaseDirectory() } );

			  verify( consistencyCheckService ).runFullConsistencyCheck( eq( backupLayout ), any( typeof( Config ) ), any( typeof( ProgressMonitorFactory ) ), any( typeof( LogProvider ) ), any( typeof( FileSystemAbstraction ) ), eq( false ), any(), any(typeof(ConsistencyFlags)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldPrintNiceHelp() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldPrintNiceHelp()
		 {
			  using ( MemoryStream baos = new MemoryStream() )
			  {
					PrintStream ps = new PrintStream( baos );

					Usage usage = new Usage( "Neo4Net-admin", mock( typeof( CommandLocator ) ) );
					usage.PrintUsageForCommand( new CheckConsistencyCommandProvider(), ps.println );

					assertEquals( string.Format( "usage: Neo4Net-admin check-consistency [--database=<name>]%n" + "                                     [--backup=</path/to/backup>]%n" + "                                     [--verbose[=<true|false>]]%n" + "                                     [--report-dir=<directory>]%n" + "                                     [--additional-config=<config-file-path>]%n" + "                                     [--check-graph[=<true|false>]]%n" + "                                     [--check-indexes[=<true|false>]]%n" + "                                     [--check-index-structure[=<true|false>]]%n" + "                                     [--check-label-scan-store[=<true|false>]]%n" + "                                     [--check-property-owners[=<true|false>]]%n" + "%n" + "environment variables:%n" + "    Neo4Net_CONF    Path to directory which contains Neo4Net.conf.%n" + "    Neo4Net_DEBUG   Set to anything to enable debug output.%n" + "    Neo4Net_HOME    Neo4Net home directory.%n" + "    HEAP_SIZE     Set JVM maximum heap size during command execution.%n" + "                  Takes a number and a unit, for example 512m.%n" + "%n" + "This command allows for checking the consistency of a database or a backup%n" + "thereof. It cannot be used with a database which is currently in use.%n" + "%n" + "All checks except 'check-graph' can be quite expensive so it may be useful to%n" + "turn them off for very large databases. Increasing the heap size can also be a%n" + "good idea. See 'Neo4Net-admin help' for details.%n" + "%n" + "options:%n" + "  --database=<name>                        Name of database. [default:" + GraphDatabaseSettings.DEFAULT_DATABASE_NAME + "]%n" + "  --backup=</path/to/backup>               Path to backup to check consistency%n" + "                                           of. Cannot be used together with%n" + "                                           --database. [default:]%n" + "  --verbose=<true|false>                   Enable verbose output.%n" + "                                           [default:false]%n" + "  --report-dir=<directory>                 Directory to write report file in.%n" + "                                           [default:.]%n" + "  --additional-config=<config-file-path>   Configuration file to supply%n" + "                                           additional configuration in. This%n" + "                                           argument is DEPRECATED. [default:]%n" + "  --check-graph=<true|false>               Perform checks between nodes,%n" + "                                           relationships, properties, types and%n" + "                                           tokens. [default:true]%n" + "  --check-indexes=<true|false>             Perform checks on indexes.%n" + "                                           [default:true]%n" + "  --check-index-structure=<true|false>     Perform structure checks on indexes.%n" + "                                           [default:false]%n" + "  --check-label-scan-store=<true|false>    Perform checks on the label scan%n" + "                                           store. [default:true]%n" + "  --check-property-owners=<true|false>     Perform additional checks on property%n" + "                                           ownership. This check is *very*%n" + "                                           expensive in time and memory.%n" + "                                           [default:false]%n" ), baos.ToString() );
			  }
		 }

		 private static File GetDatabasesFolder( Path homeDir )
		 {
			  return Config.defaults( GraphDatabaseSettings.Neo4Net_home, homeDir.toAbsolutePath().ToString() ).get(GraphDatabaseSettings.databases_root_path);
		 }
	}

}