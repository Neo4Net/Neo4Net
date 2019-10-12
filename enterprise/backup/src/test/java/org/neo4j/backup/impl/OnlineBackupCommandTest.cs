using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.backup.impl
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using CommandFailed = Org.Neo4j.Commandline.admin.CommandFailed;
	using CommandLocator = Org.Neo4j.Commandline.admin.CommandLocator;
	using IncorrectUsage = Org.Neo4j.Commandline.admin.IncorrectUsage;
	using OutsideWorld = Org.Neo4j.Commandline.admin.OutsideWorld;
	using ParameterisedOutsideWorld = Org.Neo4j.Commandline.admin.ParameterisedOutsideWorld;
	using Usage = Org.Neo4j.Commandline.admin.Usage;
	using ConsistencyFlags = Org.Neo4j.Consistency.checking.full.ConsistencyFlags;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using OptionalHostnamePort = Org.Neo4j.Kernel.impl.util.OptionalHostnamePort;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.StringContains.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.text.StringContainsInOrder.stringContainsInOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class OnlineBackupCommandTest
	{
		private bool InstanceFieldsInitialized = false;

		public OnlineBackupCommandTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			TestDirectory = TestDirectory.testDirectory( _fileSystemAbstraction );
			_stdout = new PrintStream( _baosOut );
			_stderr = new PrintStream( _baosErr );
			_outsideWorld = new ParameterisedOutsideWorld( System.console(), _stdout, _stderr, System.in, _fileSystemAbstraction );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expected = org.junit.rules.ExpectedException.none();
		 public ExpectedException Expected = ExpectedException.none();

		 private FileSystemAbstraction _fileSystemAbstraction = new DefaultFileSystemAbstraction();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory(fileSystemAbstraction);
		 public TestDirectory TestDirectory;

		 private BackupStrategyCoordinatorFactory _backupStrategyCoordinatorFactory = mock( typeof( BackupStrategyCoordinatorFactory ) );
		 private BackupStrategyCoordinator _backupStrategyCoordinator = mock( typeof( BackupStrategyCoordinator ) );

		 private MemoryStream _baosOut = new MemoryStream();
		 private MemoryStream _baosErr = new MemoryStream();
		 private PrintStream _stdout;
		 private PrintStream _stderr;
		 private OutsideWorld _outsideWorld;

		 // Parameters and helpers
		 private readonly Config _config = Config.defaults();
		 private OnlineBackupRequiredArguments _requiredArguments;
		 private readonly ConsistencyFlags _consistencyFlags = new ConsistencyFlags( true, true, true, true, true );

		 private Path _backupDirectory;
		 private Path _reportDirectory;
		 private BackupSupportingClassesFactory _backupSupportingClassesFactory = mock( typeof( BackupSupportingClassesFactory ) );

		 private readonly OptionalHostnamePort _address = new OptionalHostnamePort( "hostname", 12, 34 );
		 private readonly string _backupName = "backup name";
		 private readonly bool _fallbackToFull = true;
		 private readonly bool _doConsistencyCheck = true;
		 private readonly long _timeout = 1000;

		 private OnlineBackupCommand _subject;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _backupDirectory = TestDirectory.directory( "backupDirectory" ).toPath();
			  _reportDirectory = TestDirectory.directory( "reportDirectory/" ).toPath();
			  BackupSupportingClasses backupSupportingClasses = new BackupSupportingClasses( mock( typeof( BackupDelegator ) ), mock( typeof( BackupProtocolService ) ), mock( typeof( PageCache ) ), Collections.emptyList() );
			  when( _backupSupportingClassesFactory.createSupportingClasses( any() ) ).thenReturn(backupSupportingClasses);

			  _requiredArguments = new OnlineBackupRequiredArguments( _address, _backupDirectory, _backupName, SelectedBackupProtocol.Any, _fallbackToFull, _doConsistencyCheck, _timeout, _reportDirectory );
			  OnlineBackupContext onlineBackupContext = new OnlineBackupContext( _requiredArguments, _config, _consistencyFlags );

			  when( _backupStrategyCoordinatorFactory.backupStrategyCoordinator( any(), any(), any(), any() ) ).thenReturn(_backupStrategyCoordinator);

			  _subject = NewOnlineBackupCommand( _outsideWorld, onlineBackupContext, _backupSupportingClassesFactory, _backupStrategyCoordinatorFactory );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nonExistingBackupDirectoryRaisesException() throws org.neo4j.commandline.admin.CommandFailed, org.neo4j.commandline.admin.IncorrectUsage, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NonExistingBackupDirectoryRaisesException()
		 {
			  // given backup directory is not a directory
			  _fileSystemAbstraction.deleteRecursively( _backupDirectory.toFile() );
			  _fileSystemAbstraction.create( _backupDirectory.toFile() ).close();

			  // then
			  Expected.expect( typeof( CommandFailed ) );
			  Expected.expectMessage( stringContainsInOrder( asList( "Directory '", "backupDirectory' does not exist." ) ) );

			  // when
			  Execute();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nonExistingReportDirectoryRaisesException() throws org.neo4j.commandline.admin.CommandFailed, org.neo4j.commandline.admin.IncorrectUsage, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NonExistingReportDirectoryRaisesException()
		 {
			  // given report directory is not a directory
			  _fileSystemAbstraction.deleteRecursively( _reportDirectory.toFile() );
			  _fileSystemAbstraction.create( _reportDirectory.toFile() ).close();

			  // then
			  Expected.expect( typeof( CommandFailed ) );
			  Expected.expectMessage( stringContainsInOrder( asList( "Directory '", "reportDirectory' does not exist." ) ) );

			  // when
			  Execute();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPrintNiceHelp() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPrintNiceHelp()
		 {
			  Usage usage = new Usage( "neo4j-admin", mock( typeof( CommandLocator ) ) );
			  usage.PrintUsageForCommand( new OnlineBackupCommandProvider(), _stdout.println );

			  assertEquals( format( "usage: neo4j-admin backup --backup-dir=<backup-path> --name=<graph.db-backup>%n" + "                          [--from=<address>] [--protocol=<any|catchup|common>]%n" + "                          [--fallback-to-full[=<true|false>]]%n" + "                          [--timeout=<timeout>] [--pagecache=<8m>]%n" + "                          [--check-consistency[=<true|false>]]%n" + "                          [--cc-report-dir=<directory>]%n" + "                          [--additional-config=<config-file-path>]%n" + "                          [--cc-graph[=<true|false>]]%n" + "                          [--cc-indexes[=<true|false>]]%n" + "                          [--cc-label-scan-store[=<true|false>]]%n" + "                          [--cc-property-owners[=<true|false>]]%n" + "%n" + "environment variables:%n" + "    NEO4J_CONF    Path to directory which contains neo4j.conf.%n" + "    NEO4J_DEBUG   Set to anything to enable debug output.%n" + "    NEO4J_HOME    Neo4j home directory.%n" + "    HEAP_SIZE     Set JVM maximum heap size during command execution.%n" + "                  Takes a number and a unit, for example 512m.%n" + "%n" + "Perform an online backup from a running Neo4j enterprise server. Neo4j's backup%n" + "service must have been configured on the server beforehand.%n" + "%n" + "All consistency checks except 'cc-graph' can be quite expensive so it may be%n" + "useful to turn them off for very large databases. Increasing the heap size can%n" + "also be a good idea. See 'neo4j-admin help' for details.%n" + "%n" + "For more information see:%n" + "https://neo4j.com/docs/operations-manual/current/backup/%n" + "%n" + "options:%n" + "  --backup-dir=<backup-path>               Directory to place backup in.%n" + "  --name=<graph.db-backup>                 Name of backup. If a backup with this%n" + "                                           name already exists an incremental%n" + "                                           backup will be attempted.%n" + "  --from=<address>                         Host and port of Neo4j.%n" + "                                           [default:localhost:6362]%n" + "  --protocol=<any|catchup|common>          Preferred backup protocol%n" + "                                           [default:any]%n" + "  --fallback-to-full=<true|false>          If an incremental backup fails backup%n" + "                                           will move the old backup to%n" + "                                           <name>.err.<N> and fallback to a full%n" + "                                           backup instead. [default:true]%n" + "  --timeout=<timeout>                      Timeout in the form <time>[ms|s|m|h],%n" + "                                           where the default unit is seconds.%n" + "                                           [default:20m]%n" + "  --pagecache=<8m>                         The size of the page cache to use for%n" + "                                           the backup process. [default:8m]%n" + "  --check-consistency=<true|false>         If a consistency check should be%n" + "                                           made. [default:true]%n" + "  --cc-report-dir=<directory>              Directory where consistency report%n" + "                                           will be written. [default:.]%n" + "  --additional-config=<config-file-path>   Configuration file to supply%n" + "                                           additional configuration in. This%n" + "                                           argument is DEPRECATED. [default:]%n" + "  --cc-graph=<true|false>                  Perform consistency checks between%n" + "                                           nodes, relationships, properties,%n" + "                                           types and tokens. [default:true]%n" + "  --cc-indexes=<true|false>                Perform consistency checks on%n" + "                                           indexes. [default:true]%n" + "  --cc-label-scan-store=<true|false>       Perform consistency checks on the%n" + "                                           label scan store. [default:true]%n" + "  --cc-property-owners=<true|false>        Perform additional consistency checks%n" + "                                           on property ownership. This check is%n" + "                                           *very* expensive in time and memory.%n" + "                                           [default:false]%n" ), _baosOut.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void protocolOverrideWarnsUser() throws org.neo4j.commandline.admin.CommandFailed, org.neo4j.commandline.admin.IncorrectUsage
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ProtocolOverrideWarnsUser()
		 {
			  // with
			  IList<object[]> cases = new IList<object[]>
			  {
				  new object[]{ SelectedBackupProtocol.Catchup, string.Format( "The selected protocol `catchup` means that it is only compatible with causal clustering instances%n" ) },
				  new object[]{ SelectedBackupProtocol.Common, string.Format( "The selected protocol `common` means that it is only compatible with HA and single instances%n" ) }
			  };
			  foreach ( object[] thisCase in cases )
			  {
					// given
					_requiredArguments = new OnlineBackupRequiredArguments( _address, _backupDirectory, _backupName, ( SelectedBackupProtocol ) thisCase[0], _fallbackToFull, _doConsistencyCheck, _timeout, _reportDirectory );
					OnlineBackupContext onlineBackupContext = new OnlineBackupContext( _requiredArguments, _config, _consistencyFlags );
					_subject = NewOnlineBackupCommand( _outsideWorld, onlineBackupContext, _backupSupportingClassesFactory, _backupStrategyCoordinatorFactory );

					// when
					Execute();

					// then
					assertThat( _baosOut.ToString(), containsString((string) thisCase[1]) );
					_baosOut.reset();
			  }
		 }

		 private static OnlineBackupCommand NewOnlineBackupCommand( OutsideWorld outsideWorld, OnlineBackupContext onlineBackupContext, BackupSupportingClassesFactory backupSupportingClassesFactory, BackupStrategyCoordinatorFactory backupStrategyCoordinatorFactory )
		 {
			  OnlineBackupContextFactory contextBuilder = mock( typeof( OnlineBackupContextFactory ) );
			  try
			  {
					when( contextBuilder.CreateContext( any() ) ).thenReturn(onlineBackupContext);
			  }
			  catch ( Exception e ) when ( e is IncorrectUsage || e is CommandFailed )
			  {
					throw new Exception( "Shouldn't happen", e );
			  }

			  return new OnlineBackupCommand( outsideWorld, contextBuilder, backupSupportingClassesFactory, backupStrategyCoordinatorFactory );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void execute() throws org.neo4j.commandline.admin.IncorrectUsage, org.neo4j.commandline.admin.CommandFailed
		 private void Execute()
		 {
			  string[] implementationDoesNotUseArguments = new string[0];
			  _subject.execute( implementationDoesNotUseArguments );
		 }
	}

}