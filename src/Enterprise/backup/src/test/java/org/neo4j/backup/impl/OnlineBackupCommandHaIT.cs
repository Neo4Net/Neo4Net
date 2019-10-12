using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.backup.impl
{
	using SystemUtils = org.apache.commons.lang3.SystemUtils;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Parameter = org.junit.runners.Parameterized.Parameter;
	using Parameters = org.junit.runners.Parameterized.Parameters;


	using ConsistencyCheckService = Neo4Net.Consistency.ConsistencyCheckService;
	using ConsistencyFlags = Neo4Net.Consistency.checking.full.ConsistencyFlags;
	using DatabaseShutdownException = Neo4Net.Graphdb.DatabaseShutdownException;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseBuilder = Neo4Net.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseFactory = Neo4Net.Graphdb.factory.GraphDatabaseFactory;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using ProgressMonitorFactory = Neo4Net.Helpers.progress.ProgressMonitorFactory;
	using ByteUnit = Neo4Net.Io.ByteUnit;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using StoreLayout = Neo4Net.Io.layout.StoreLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using RecoveryRequiredChecker = Neo4Net.Kernel.impl.recovery.RecoveryRequiredChecker;
	using HighLimit = Neo4Net.Kernel.impl.store.format.highlimit.HighLimit;
	using Standard = Neo4Net.Kernel.impl.store.format.standard.Standard;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using DbRepresentation = Neo4Net.Test.DbRepresentation;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assume.assumeFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.backup.impl.OnlineBackupCommandCcIT.arg;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.backup.impl.OnlineBackupCommandCcIT.wrapWithNormalOutput;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.backup.impl.OnlineBackupContextFactory.ARG_NAME_FALLBACK_FULL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.TestHelpers.runBackupToolFromOtherJvmToGetExitCode;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class OnlineBackupCommandHaIT
	public class OnlineBackupCommandHaIT
	{
		private bool InstanceFieldsInitialized = false;

		public OnlineBackupCommandHaIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_testDirectory = TestDirectory.testDirectory( _fileSystemRule );
			RuleChain = RuleChain.outerRule( _suppressOutput ).around( _fileSystemRule ).around( _testDirectory ).around( _pageCacheRule ).around( _db );
		}

		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();
		 private readonly SuppressOutput _suppressOutput = SuppressOutput.suppressAll();
		 private TestDirectory _testDirectory;
		 private readonly EmbeddedDatabaseRule _db = new EmbeddedDatabaseRule().startLazily();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(suppressOutput).around(fileSystemRule).around(testDirectory).around(pageCacheRule).around(db);
		 public RuleChain RuleChain;

		 private File _backupDir;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameter public String recordFormat;
		 public string RecordFormat;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters(name = "{0}") public static java.util.List<String> recordFormats()
		 public static IList<string> RecordFormats()
		 {
			  return Arrays.asList( Standard.LATEST_NAME, HighLimit.NAME );
		 }

		 private IList<ThreadStart> _oneOffShutdownTasks;
		 private static readonly Label _label = Label.label( "any_label" );
		 private const string PROP_NAME = "name";
		 private const string PROP_RANDOM = "random";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void resetTasks()
		 public virtual void ResetTasks()
		 {
			  _backupDir = _testDirectory.directory( "backups" );
			  _oneOffShutdownTasks = new List<ThreadStart>();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void shutdownTasks()
		 public virtual void ShutdownTasks()
		 {
			  _oneOffShutdownTasks.ForEach( ThreadStart.run );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureBackupCanBePerformedWithCustomPort() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MakeSureBackupCanBePerformedWithCustomPort()
		 {
			  assumeFalse( SystemUtils.IS_OS_WINDOWS );
			  string backupName = "customport" + RecordFormat; // due to ClassRule not cleaning between tests

			  int backupPort = PortAuthority.allocatePort();
			  StartDb( backupPort );
			  assertEquals( "should not be able to do backup when noone is listening", 1, RunBackupTool( _testDirectory.absolutePath(), "--from", "127.0.0.1:" + PortAuthority.allocatePort(), "--cc-report-dir=" + _backupDir, "--backup-dir=" + _backupDir, "--name=" + backupName ) );
			  assertEquals( 0, RunBackupTool( _testDirectory.absolutePath(), "--from", "127.0.0.1:" + backupPort, "--cc-report-dir=" + _backupDir, "--backup-dir=" + _backupDir, "--name=" + backupName ) );
			  assertEquals( DbRepresentation.of( _db ), GetBackupDbRepresentation( backupName ) );
			  CreateSomeData( _db );
			  assertEquals( 0, RunBackupTool( _testDirectory.absolutePath(), "--from", "127.0.0.1:" + backupPort, "--cc-report-dir=" + _backupDir, "--backup-dir=" + _backupDir, "--name=" + backupName ) );
			  assertEquals( DbRepresentation.of( _db ), GetBackupDbRepresentation( backupName ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fullBackupIsRecoveredAndConsistent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FullBackupIsRecoveredAndConsistent()
		 {
			  // given database exists
			  int backupPort = PortAuthority.allocatePort();
			  StartDb( backupPort );
			  string ip = ":" + backupPort;

			  string name = System.Guid.randomUUID().ToString();
			  File backupLocation = new File( _backupDir, name );
			  DatabaseLayout backupLayout = DatabaseLayout.of( backupLocation );

			  // when
			  assertEquals( 0, RunBackupTool( _testDirectory.absolutePath(), "--from", ip, "--cc-report-dir=" + _backupDir, "--backup-dir=" + _backupDir, "--name=" + name ) );

			  // then
			  assertFalse( "Store should not require recovery", ( new RecoveryRequiredChecker( _fileSystemRule, _pageCacheRule.getPageCache( _fileSystemRule ), Config.defaults(), new Monitors() ) ).isRecoveryRequiredAt(backupLayout) );
			  ConsistencyFlags consistencyFlags = new ConsistencyFlags( true, true, true, true, true );
			  assertTrue("Consistency check failed", new ConsistencyCheckService()
						 .runFullConsistencyCheck( backupLayout, Config.defaults(), ProgressMonitorFactory.NONE, NullLogProvider.Instance, false, consistencyFlags ).Successful);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void incrementalBackupIsRecoveredAndConsistent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IncrementalBackupIsRecoveredAndConsistent()
		 {
			  // given database exists
			  int backupPort = PortAuthority.allocatePort();
			  StartDb( backupPort );
			  string ip = ":" + backupPort;

			  string name = System.Guid.randomUUID().ToString();
			  File backupLocation = new File( _backupDir, name );
			  DatabaseLayout backupLayout = DatabaseLayout.of( backupLocation );

			  // when
			  assertEquals( 0, RunBackupTool( _testDirectory.absolutePath(), "--from", ip, "--cc-report-dir=" + _backupDir, "--backup-dir=" + _backupDir, "--name=" + name ) );

			  // and
			  CreateSomeData( _db );
			  assertEquals( 0, RunBackupTool( _testDirectory.absolutePath(), "--from", ip, "--cc-report-dir=" + _backupDir, "--backup-dir=" + _backupDir, "--name=" + name, arg(ARG_NAME_FALLBACK_FULL, false) ) );

			  // then
			  assertFalse( "Store should not require recovery", ( new RecoveryRequiredChecker( _fileSystemRule, _pageCacheRule.getPageCache( _fileSystemRule ), Config.defaults(), new Monitors() ) ).isRecoveryRequiredAt(backupLayout) );
			  ConsistencyFlags consistencyFlags = new ConsistencyFlags( true, true, true, true, true );
			  assertTrue("Consistency check failed", new ConsistencyCheckService()
						 .runFullConsistencyCheck( backupLayout, Config.defaults(), ProgressMonitorFactory.NONE, NullLogProvider.Instance, false, consistencyFlags ).Successful);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dataIsInAUsableStateAfterBackup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DataIsInAUsableStateAfterBackup()
		 {
			  // given database exists
			  int backupPort = PortAuthority.allocatePort();
			  StartDb( backupPort );

			  // and the database has indexes
			  CreateIndexes( _db );

			  // and the database is being populated
			  AtomicBoolean continueFlagReference = new AtomicBoolean( true );
			  ( new Thread( () => repeatedlyPopulateDatabase(_db, continueFlagReference) ) ).Start(); // populate db with number properties etc.
			  _oneOffShutdownTasks.Add( () => continueFlagReference.set(false) ); // kill thread

			  // then backup is successful
			  string ip = ":" + backupPort;
			  string backupName = "usableState" + RecordFormat;
			  assertEquals( 0, RunBackupTool( _testDirectory.absolutePath(), "--from", ip, "--cc-report-dir=" + _backupDir, "--backup-dir=" + _backupDir, "--name=" + backupName ) );
			  _db.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void backupDatabaseTransactionLogsStoredWithDatabase() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BackupDatabaseTransactionLogsStoredWithDatabase()
		 {
			  int backupPort = PortAuthority.allocatePort();
			  StartDb( backupPort );
			  string ip = ":" + backupPort;
			  string name = "backupWithTxLogs" + RecordFormat;
			  assertEquals( 0, RunBackupTool( _testDirectory.absolutePath(), "--from", ip, "--cc-report-dir=" + _backupDir, "--backup-dir=" + _backupDir, "--name=" + name ) );
			  _db.shutdown();

			  using ( DefaultFileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction() )
			  {
					LogFiles logFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( new File( _backupDir, name ), fileSystem ).build();
					assertTrue( logFiles.VersionExists( 0 ) );
					assertThat( logFiles.GetLogFileForVersion( 0 ).length(), greaterThan(50L) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void backupFailsWithCatchupProtoOverride() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BackupFailsWithCatchupProtoOverride()
		 {
			  string backupName = "portOverride" + RecordFormat; // due to ClassRule not cleaning between tests

			  int backupPort = PortAuthority.allocatePort();
			  StartDb( backupPort );

			  assertEquals( 1, RunBackupTool( _testDirectory.absolutePath(), "--from", "127.0.0.1:" + backupPort, "--cc-report-dir=" + _backupDir, "--backup-dir=" + _backupDir, "--protocol=catchup", "--name=" + backupName ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void backupDoesNotDisplayExceptionWhenSuccessful() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BackupDoesNotDisplayExceptionWhenSuccessful()
		 {
			  // given
			  string backupName = "noErrorTest_" + RecordFormat;
			  int backupPort = PortAuthority.allocatePort();
			  StartDb( backupPort );

			  // and
			  MemoryStream byteArrayOutputStream = new MemoryStream();
			  PrintStream outputStream = wrapWithNormalOutput( System.out, new PrintStream( byteArrayOutputStream ) );

			  // and
			  MemoryStream byteArrayErrorStream = new MemoryStream();
			  PrintStream errorStream = wrapWithNormalOutput( System.err, new PrintStream( byteArrayErrorStream ) );

			  // when
			  assertEquals( 0, RunBackupTool( _testDirectory.absolutePath(), outputStream, errorStream, "--from", "127.0.0.1:" + backupPort, "--cc-report-dir=" + _backupDir, "--backup-dir=" + _backupDir, "--name=" + backupName ) );

			  // then
			  assertFalse( byteArrayErrorStream.ToString().ToLower().contains("exception") );
			  assertFalse( byteArrayOutputStream.ToString().ToLower().contains("exception") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void reportsProgress() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReportsProgress()
		 {
			  // given
			  string backupName = "reportsProgress_" + RecordFormat;
			  int backupPort = PortAuthority.allocatePort();
			  StartDb( backupPort );

			  // when
			  assertEquals( 0, RunBackupTool( _backupDir, System.out, System.err, "--from", "127.0.0.1:" + backupPort, "--protocol=common", "--cc-report-dir=" + _backupDir, "--backup-dir=" + _backupDir, "--name=" + backupName ) );

			  // then
			  string output = _suppressOutput.OutputVoice.ToString();
			  string legacyImplementationDetail = "temp-copy";
			  string location = Paths.get( _backupDir.ToString(), backupName, legacyImplementationDetail ).ToString();
			  assertTrue( output.Contains( "Start receiving store files" ) );
			  assertTrue( output.Contains( "Finish receiving store files" ) );
			  string tested = Paths.get( location, "neostore.nodestore.db.labels" ).ToString();
			  assertTrue( tested, output.Contains( format( "Start receiving store file %s", tested ) ) );
			  assertTrue( tested, output.Contains( format( "Finish receiving store file %s", tested ) ) );
			  assertFalse( output.Contains( "Start receiving transactions from " ) );
			  assertFalse( output.Contains( "Finish receiving transactions at " ) );
			  assertTrue( output.Contains( "Start recovering store" ) );
			  assertTrue( output.Contains( "Finish recovering store" ) );
			  assertFalse( output.Contains( "Start receiving index snapshots" ) );
			  assertFalse( output.Contains( "Start receiving index snapshot id 1" ) );
			  assertFalse( output.Contains( "Finished receiving index snapshot id 1" ) );
			  assertFalse( output.Contains( "Finished receiving index snapshots" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void backupRenamesWork()
		 public virtual void BackupRenamesWork()
		 {
			  // given a prexisting backup from a different store
			  string backupName = "preexistingBackup_" + RecordFormat;
			  int firstBackupPort = PortAuthority.allocatePort();
			  StartDb( firstBackupPort );
			  CreateSpecificNodePair( _db, "first" );

			  assertEquals( 0, RunSameJvm( _backupDir, backupName, "--from", "127.0.0.1:" + firstBackupPort, "--cc-report-dir=" + _backupDir, "--protocol=common", "--backup-dir=" + _backupDir, "--name=" + backupName ) );
			  DbRepresentation firstDatabaseRepresentation = DbRepresentation.of( _db );

			  // and a different database
			  int secondBackupPort = PortAuthority.allocatePort();
			  GraphDatabaseService db2 = CreateDb2( secondBackupPort );
			  CreateSpecificNodePair( db2, "second" );
			  DbRepresentation secondDatabaseRepresentation = DbRepresentation.of( db2 );

			  // when backup is performed
			  assertEquals( 0, RunSameJvm( _backupDir, backupName, "--from", "127.0.0.1:" + secondBackupPort, "--cc-report-dir=" + _backupDir, "--backup-dir=" + _backupDir, "--protocol=common", "--name=" + backupName ) );

			  // then the new backup has the correct name
			  assertEquals( secondDatabaseRepresentation, GetBackupDbRepresentation( backupName ) );

			  // and the old backup is in a renamed location
			  assertEquals( firstDatabaseRepresentation, GetBackupDbRepresentation( backupName + ".err.0" ) );

			  // and the data isn't equal (sanity check)
			  assertNotEquals( firstDatabaseRepresentation, secondDatabaseRepresentation );
			  db2.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void onlyTheLatestTransactionIsKeptAfterIncrementalBackup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void OnlyTheLatestTransactionIsKeptAfterIncrementalBackup()
		 {
			  // given database exists with data
			  int port = PortAuthority.allocatePort();
			  StartDb( port );
			  CreateSomeData( _db );

			  // and backup client is told to rotate conveniently
			  Config config = Config.builder().withSetting(GraphDatabaseSettings.logical_log_rotation_threshold, "1m").build();
			  File configOverrideFile = _testDirectory.file( "neo4j-backup.conf" );
			  OnlineBackupCommandBuilder.WriteConfigToFile( config, configOverrideFile );

			  // and we have a full backup
			  string backupName = "backupName" + RecordFormat;
			  string address = "localhost:" + port;
			  assertEquals( 0, runBackupToolFromOtherJvmToGetExitCode( _backupDir, "--from", address, "--cc-report-dir=" + _backupDir, "--backup-dir=" + _backupDir, "--protocol=common", "--additional-config=" + configOverrideFile, "--name=" + backupName ) );

			  // and the database contains a few more transactions
			  Transactions1M( _db );
			  Transactions1M( _db ); // rotation, second tx log file

			  // when we perform an incremental backup
			  assertEquals( 0, runBackupToolFromOtherJvmToGetExitCode( _backupDir, "--from", address, "--cc-report-dir=" + _backupDir, "--backup-dir=" + _backupDir, "--protocol=common", "--additional-config=" + configOverrideFile, "--name=" + backupName ) );

			  // then there has been a rotation
			  BackupTransactionLogFilesHelper backupTransactionLogFilesHelper = new BackupTransactionLogFilesHelper();
			  LogFiles logFiles = BackupTransactionLogFilesHelper.ReadLogFiles( StoreLayout.of( _backupDir ).databaseLayout( backupName ) );
			  long highestTxIdInLogFiles = logFiles.HighestLogVersion;
			  assertEquals( 2, highestTxIdInLogFiles );

			  // and the original log has not been removed since the transactions are applied at start
			  long lowestTxIdInLogFiles = logFiles.LowestLogVersion;
			  assertEquals( 0, lowestTxIdInLogFiles );
		 }

		 private static void Transactions1M( GraphDatabaseService db )
		 {
			  int numberOfTransactions = 500;
			  long sizeOfTransaction = ( ByteUnit.mebiBytes( 1 ) / numberOfTransactions ) + 1;
			  for ( int txId = 0; txId < numberOfTransactions; txId++ )
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 Node node = Db.createNode();
						 string longString = LongStream.range( 0, sizeOfTransaction ).map( l => l % 10 ).mapToObj( long?.toString ).collect( joining( "" ) );
						 node.SetProperty( "name", longString );
						 Db.createNode().createRelationshipTo(node, RelationshipType.withName("KNOWS"));
						 tx.Success();
					}
			  }
		 }

		 private static void CreateSomeData( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();
					node.SetProperty( "name", "Neo" );
					Db.createNode().createRelationshipTo(node, RelationshipType.withName("KNOWS"));
					tx.Success();
			  }
		 }

		 private static void CreateSpecificNodePair( GraphDatabaseService db, string name )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node left = Db.createNode();
					left.SetProperty( "name", name + "Left" );
					Node right = Db.createNode();
					right.SetProperty( "name", name + "Right" );
					right.CreateRelationshipTo( left, RelationshipType.withName( "KNOWS" ) );
					tx.Success();
			  }
		 }

		 private static void RepeatedlyPopulateDatabase( GraphDatabaseService db, AtomicBoolean continueFlagReference )
		 {
			  while ( continueFlagReference.get() )
			  {
					try
					{
						 CreateSomeData( db );
					}
					catch ( DatabaseShutdownException )
					{
						 break;
					}
			  }
		 }

		 private static void CreateIndexes( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().indexFor(_label).on(PROP_NAME).on(PROP_RANDOM).create();
					tx.Success();
			  }
		 }

		 private GraphDatabaseService CreateDb2( int? backupPort )
		 {
			  File storeDir = _testDirectory.databaseDir( "graph-db-2" );
			  GraphDatabaseFactory factory = new GraphDatabaseFactory();
			  GraphDatabaseBuilder builder = factory.NewEmbeddedDatabaseBuilder( storeDir );
			  builder.SetConfig( OnlineBackupSettings.online_backup_server, "0.0.0.0:" + backupPort );
			  return builder.NewGraphDatabase();
		 }

		 private void StartDb( int? backupPort )
		 {
			  StartDb( _db, backupPort );
		 }

		 private void StartDb( EmbeddedDatabaseRule db, int? backupPort )
		 {
			  Db.withSetting( GraphDatabaseSettings.record_format, RecordFormat );
			  Db.withSetting( OnlineBackupSettings.online_backup_enabled, Settings.TRUE );
			  Db.withSetting( OnlineBackupSettings.online_backup_server, "127.0.0.1" + ":" + backupPort );
			  Db.ensureStarted();
			  CreateSomeData( db );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static int runBackupTool(java.io.File neo4jHome, java.io.PrintStream outputStream, java.io.PrintStream errorStream, String... args) throws Exception
		 private static int RunBackupTool( File neo4jHome, PrintStream outputStream, PrintStream errorStream, params string[] args )
		 {
			  return runBackupToolFromOtherJvmToGetExitCode( neo4jHome, outputStream, errorStream, false, args );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static int runBackupTool(java.io.File neo4jHome, String... args) throws Exception
		 private static int RunBackupTool( File neo4jHome, params string[] args )
		 {
			  return runBackupToolFromOtherJvmToGetExitCode( neo4jHome, args );
		 }

		 private static int RunSameJvm( File home, string name, params string[] args )
		 {
			  try
			  {
					( new OnlineBackupCommandBuilder() ).WithRawArgs(args).backup(home, name);
					return 0;
			  }
			  catch ( Exception e )
			  {
					Console.WriteLine( e.ToString() );
					Console.Write( e.StackTrace );
					return 1;
			  }
		 }

		 private DbRepresentation GetBackupDbRepresentation( string name )
		 {
			  Config config = Config.defaults( OnlineBackupSettings.online_backup_enabled, Settings.FALSE );
			  return DbRepresentation.of( DatabaseLayout.of( _backupDir, name ).databaseDirectory(), config );
		 }
	}

}