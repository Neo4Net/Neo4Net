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
namespace Org.Neo4j.backup.impl
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Parameter = org.junit.runners.Parameterized.Parameter;
	using Parameters = org.junit.runners.Parameterized.Parameters;


	using ClusterHelper = Org.Neo4j.causalclustering.ClusterHelper;
	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using CoreGraphDatabase = Org.Neo4j.causalclustering.core.CoreGraphDatabase;
	using Role = Org.Neo4j.causalclustering.core.consensus.roles.Role;
	using Org.Neo4j.causalclustering.discovery;
	using CoreClusterMember = Org.Neo4j.causalclustering.discovery.CoreClusterMember;
	using DiscoveryServiceFactory = Org.Neo4j.causalclustering.discovery.DiscoveryServiceFactory;
	using EnterpriseCluster = Org.Neo4j.causalclustering.discovery.EnterpriseCluster;
	using IpFamily = Org.Neo4j.causalclustering.discovery.IpFamily;
	using SharedDiscoveryServiceFactory = Org.Neo4j.causalclustering.discovery.SharedDiscoveryServiceFactory;
	using CausalClusteringTestHelpers = Org.Neo4j.causalclustering.helpers.CausalClusteringTestHelpers;
	using ConsistencyCheckService = Org.Neo4j.Consistency.ConsistencyCheckService;
	using ConsistencyFlags = Org.Neo4j.Consistency.checking.full.ConsistencyFlags;
	using Node = Org.Neo4j.Graphdb.Node;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using ProgressMonitorFactory = Org.Neo4j.Helpers.progress.ProgressMonitorFactory;
	using ByteUnit = Org.Neo4j.Io.ByteUnit;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using OnlineBackupSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using RecoveryRequiredChecker = Org.Neo4j.Kernel.impl.recovery.RecoveryRequiredChecker;
	using HighLimit = Org.Neo4j.Kernel.impl.store.format.highlimit.HighLimit;
	using Standard = Org.Neo4j.Kernel.impl.store.format.standard.Standard;
	using LogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.LogFiles;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using PortAuthority = Org.Neo4j.Ports.Allocation.PortAuthority;
	using DbRepresentation = Org.Neo4j.Test.DbRepresentation;
	using ClusterRule = Org.Neo4j.Test.causalclustering.ClusterRule;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using SuppressOutput = Org.Neo4j.Test.rule.SuppressOutput;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;
	using TestHelpers = Org.Neo4j.Util.TestHelpers;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.backup.impl.OnlineBackupContextFactory.ARG_NAME_FALLBACK_FULL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.enterprise.configuration.OnlineBackupSettings.online_backup_server;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class OnlineBackupCommandCcIT
	public class OnlineBackupCommandCcIT
	{
		private bool InstanceFieldsInitialized = false;

		public OnlineBackupCommandCcIT()
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
			RuleChain = RuleChain.outerRule( _suppressOutput ).around( _fileSystemRule ).around( _testDirectory ).around( _pageCacheRule ).around( _clusterRule );
		}

		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();
		 private TestDirectory _testDirectory;
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();
		 private ClusterRule _clusterRule = new ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(3).withSharedCoreParam(CausalClusteringSettings.cluster_topology_refresh, "5s");

		 private readonly SuppressOutput _suppressOutput = SuppressOutput.suppressAll();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(suppressOutput).around(fileSystemRule).around(testDirectory).around(pageCacheRule).around(clusterRule);
		 public RuleChain RuleChain;

		 private const string DATABASE_NAME = "defaultport";
		 private File _backupDatabaseDir;
		 private File _backupStoreDir;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameter public String recordFormat;
		 public string RecordFormat;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters(name = "{0}") public static java.util.List<String> recordFormats()
		 public static IList<string> RecordFormats()
		 {
			  return Arrays.asList( Standard.LATEST_NAME, HighLimit.NAME );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void initialiseBackupDirectory()
		 public virtual void InitialiseBackupDirectory()
		 {
			  _backupStoreDir = _testDirectory.directory( "backupStore" );
			  _backupDatabaseDir = new File( _backupStoreDir, DATABASE_NAME );
			  _backupDatabaseDir.mkdirs();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void backupCanBePerformedOverCcWithCustomPort() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BackupCanBePerformedOverCcWithCustomPort()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = startCluster(recordFormat);
			  Cluster<object> cluster = StartCluster( RecordFormat );
			  string customAddress = CausalClusteringTestHelpers.transactionAddress( ClusterLeader( cluster ).database() );

			  assertEquals( 0, RunBackupOtherJvm( customAddress, DATABASE_NAME ) );
			  assertEquals( DbRepresentation.of( ClusterDatabase( cluster ) ), GetBackupDbRepresentation( DATABASE_NAME, _backupStoreDir ) );

			  CreateSomeData( cluster );
			  assertEquals( 0, RunBackupOtherJvm( customAddress, DATABASE_NAME ) );
			  assertEquals( DbRepresentation.of( ClusterDatabase( cluster ) ), GetBackupDbRepresentation( DATABASE_NAME, _backupStoreDir ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dataIsInAUsableStateAfterBackup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DataIsInAUsableStateAfterBackup()
		 {
			  // given database exists
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = startCluster(recordFormat);
			  Cluster<object> cluster = StartCluster( RecordFormat );

			  // and the database has indexes
			  ClusterHelper.createIndexes( cluster.GetMemberWithAnyRole( Role.LEADER ).database() );

			  // and the database is being populated
			  AtomicBoolean populateDatabaseFlag = new AtomicBoolean( true );
			  Thread thread = new Thread( () => repeatedlyPopulateDatabase(cluster, populateDatabaseFlag) );
			  thread.Start(); // populate db with number properties etc.
			  try
			  {
					// then backup is successful
					string address = cluster.AwaitLeader().config().get(online_backup_server).ToString();
					assertEquals( 0, RunBackupOtherJvm( address, DATABASE_NAME ) );
			  }
			  finally
			  {
					populateDatabaseFlag.set( false );
					thread.Join();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void backupCanBeOptionallySwitchedOnWithTheBackupConfig() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BackupCanBeOptionallySwitchedOnWithTheBackupConfig()
		 {
			  // given a cluster with backup switched on
			  int[] backupPorts = new int[]{ PortAuthority.allocatePort(), PortAuthority.allocatePort(), PortAuthority.allocatePort() };
			  string value = "localhost:%d";
			  _clusterRule = _clusterRule.withSharedCoreParam( OnlineBackupSettings.online_backup_enabled, "true" ).withInstanceCoreParam( online_backup_server, i => format( value, backupPorts[i] ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = startCluster(recordFormat);
			  Cluster<object> cluster = StartCluster( RecordFormat );
			  string customAddress = "localhost:" + backupPorts[0];

			  // when a full backup is performed
			  assertEquals( 0, RunBackupOtherJvm( customAddress, DATABASE_NAME ) );
			  assertEquals( DbRepresentation.of( ClusterDatabase( cluster ) ), GetBackupDbRepresentation( DATABASE_NAME, _backupStoreDir ) );

			  // and an incremental backup is performed
			  CreateSomeData( cluster );
			  assertEquals( 0, RunBackupOtherJvm( customAddress, DATABASE_NAME ) );
			  assertEquals( 0, RunBackupToolFromOtherJvmToGetExitCode( "--from=" + customAddress, "--cc-report-dir=" + _backupStoreDir, "--backup-dir=" + _backupStoreDir, "--name=defaultport", Arg( ARG_NAME_FALLBACK_FULL, false ) ) );

			  // then the data matches
			  assertEquals( DbRepresentation.of( ClusterDatabase( cluster ) ), GetBackupDbRepresentation( DATABASE_NAME, _backupStoreDir ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void secondaryTransactionProtocolIsSwitchedOffCorrespondingBackupSetting() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SecondaryTransactionProtocolIsSwitchedOffCorrespondingBackupSetting()
		 {
			  // given a cluster with backup switched off
			  int[] backupPorts = new int[]{ PortAuthority.allocatePort(), PortAuthority.allocatePort(), PortAuthority.allocatePort() };
			  string value = "localhost:%d";
			  _clusterRule = _clusterRule.withSharedCoreParam( OnlineBackupSettings.online_backup_enabled, "false" ).withInstanceCoreParam( online_backup_server, i => format( value, backupPorts[i] ) );
			  StartCluster( RecordFormat );
			  string customAddress = "localhost:" + backupPorts[0];

			  // then a full backup is impossible from the backup port
			  assertEquals( 1, RunBackupOtherJvm( customAddress, DATABASE_NAME ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void backupDoesntDisplayExceptionWhenSuccessful() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BackupDoesntDisplayExceptionWhenSuccessful()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = startCluster(recordFormat);
			  Cluster<object> cluster = StartCluster( RecordFormat );
			  string customAddress = CausalClusteringTestHelpers.transactionAddress( ClusterLeader( cluster ).database() );

			  // when
			  assertEquals( 0, RunBackupOtherJvm( customAddress, DATABASE_NAME ) );

			  // then
			  assertFalse( _suppressOutput.ErrorVoice.ToString().ToLower().contains("exception") );
			  assertFalse( _suppressOutput.OutputVoice.ToString().ToLower().contains("exception") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void reportsProgress() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReportsProgress()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = startCluster(recordFormat);
			  Cluster<object> cluster = StartCluster( RecordFormat );
			  ClusterHelper.createIndexes( cluster.GetMemberWithAnyRole( Role.LEADER ).database() );
			  string customAddress = CausalClusteringTestHelpers.backupAddress( ClusterLeader( cluster ).database() );

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String backupName = "reportsProgress_" + recordFormat;
			  string backupName = "reportsProgress_" + RecordFormat;
			  assertEquals( 0, RunBackupOtherJvm( customAddress, backupName ) );

			  // then
			  string output = _suppressOutput.OutputVoice.ToString();
			  string location = Paths.get( _backupStoreDir.ToString(), backupName ).ToString();

			  assertTrue( output.Contains( "Start receiving store files" ) );
			  assertTrue( output.Contains( "Finish receiving store files" ) );
			  string tested = Paths.get( location, "neostore.nodestore.db.labels" ).ToString();
			  assertTrue( tested, output.Contains( format( "Start receiving store file %s", tested ) ) );
			  assertTrue( tested, output.Contains( format( "Finish receiving store file %s", tested ) ) );
			  assertTrue( output.Contains( "Start receiving transactions from " ) );
			  assertTrue( output.Contains( "Finish receiving transactions at " ) );
			  assertTrue( output.Contains( "Start receiving index snapshots" ) );
			  assertTrue( output.Contains( "Finished receiving index snapshots" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fullBackupIsRecoveredAndConsistent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FullBackupIsRecoveredAndConsistent()
		 {
			  // given database exists with data
			  Cluster cluster = StartCluster( RecordFormat );
			  CreateSomeData( cluster );
			  string address = cluster.awaitLeader().Config().get(online_backup_server).ToString();

			  string name = System.Guid.randomUUID().ToString();
			  File backupLocation = new File( _backupStoreDir, name );
			  DatabaseLayout backupLayout = DatabaseLayout.of( backupLocation );

			  // when
			  assertEquals( 0, RunBackupToolFromOtherJvmToGetExitCode( "--from", address, "--cc-report-dir=" + _backupStoreDir, "--backup-dir=" + _backupStoreDir, "--name=" + name ) );

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
			  // given database exists with data
			  Cluster cluster = StartCluster( RecordFormat );
			  CreateSomeData( cluster );
			  string address = cluster.awaitLeader().Config().get(online_backup_server).ToString();

			  string name = System.Guid.randomUUID().ToString();
			  File backupLocation = new File( _backupStoreDir, name );
			  DatabaseLayout backupLayout = DatabaseLayout.of( backupLocation );

			  // when
			  assertEquals( 0, RunBackupToolFromOtherJvmToGetExitCode( "--from", address, "--cc-report-dir=" + _backupStoreDir, "--backup-dir=" + _backupStoreDir, "--name=" + name ) );

			  // and
			  CreateSomeData( cluster );
			  assertEquals( 0, RunBackupToolFromOtherJvmToGetExitCode( "--from", address, "--cc-report-dir=" + _backupStoreDir, "--backup-dir=" + _backupStoreDir, "--name=" + name, Arg( ARG_NAME_FALLBACK_FULL, false ) ) );

			  // then
			  assertFalse( "Store should not require recovery", ( new RecoveryRequiredChecker( _fileSystemRule, _pageCacheRule.getPageCache( _fileSystemRule ), Config.defaults(), new Monitors() ) ).isRecoveryRequiredAt(backupLayout) );
			  ConsistencyFlags consistencyFlags = new ConsistencyFlags( true, true, true, true, true );
			  assertTrue("Consistency check failed", new ConsistencyCheckService()
						 .runFullConsistencyCheck( backupLayout, Config.defaults(), ProgressMonitorFactory.NONE, NullLogProvider.Instance, false, consistencyFlags ).Successful);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void onlyTheLatestTransactionIsKeptAfterIncrementalBackup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void OnlyTheLatestTransactionIsKeptAfterIncrementalBackup()
		 {
			  // given database exists with data
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = startCluster(recordFormat);
			  Cluster<object> cluster = StartCluster( RecordFormat );
			  CreateSomeData( cluster );

			  // and backup client is told to rotate conveniently
			  Config config = Config.builder().withSetting(GraphDatabaseSettings.logical_log_rotation_threshold, "1m").build();
			  File configOverrideFile = _testDirectory.file( "neo4j-backup.conf" );
			  OnlineBackupCommandBuilder.WriteConfigToFile( config, configOverrideFile );

			  // and we have a full backup
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String backupName = "backupName" + recordFormat;
			  string backupName = "backupName" + RecordFormat;
			  string address = CausalClusteringTestHelpers.backupAddress( ClusterLeader( cluster ).database() );
			  assertEquals( 0, RunBackupToolFromOtherJvmToGetExitCode( "--from", address, "--cc-report-dir=" + _backupStoreDir, "--backup-dir=" + _backupStoreDir, "--additional-config=" + configOverrideFile, "--name=" + backupName ) );

			  // and the database contains a few more transactions
			  Transactions1M( cluster );
			  Transactions1M( cluster ); // rotation, second tx log file

			  // when we perform an incremental backup
			  assertEquals( 0, RunBackupToolFromOtherJvmToGetExitCode( "--from", address, "--cc-report-dir=" + _backupStoreDir, "--backup-dir=" + _backupStoreDir, "--additional-config=" + configOverrideFile, "--name=" + backupName, Arg( ARG_NAME_FALLBACK_FULL, false ) ) );

			  // then there has been a rotation
			  LogFiles logFiles = BackupTransactionLogFilesHelper.ReadLogFiles( DatabaseLayout.of( new File( _backupStoreDir, backupName ) ) );
			  long highestTxIdInLogFiles = logFiles.HighestLogVersion;
			  assertEquals( 2, highestTxIdInLogFiles );

			  // and the original log has not been removed since the transactions are applied at start
			  long lowestTxIdInLogFiles = logFiles.LowestLogVersion;
			  assertEquals( 0, lowestTxIdInLogFiles );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void backupRenamesWork() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BackupRenamesWork()
		 {
			  // given a prexisting backup from a different store
			  string backupName = "preexistingBackup_" + RecordFormat;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = startCluster(recordFormat);
			  Cluster<object> cluster = StartCluster( RecordFormat );
			  string firstBackupAddress = CausalClusteringTestHelpers.transactionAddress( ClusterLeader( cluster ).database() );

			  assertEquals( 0, RunBackupOtherJvm( firstBackupAddress, backupName ) );
			  DbRepresentation firstDatabaseRepresentation = DbRepresentation.of( ClusterLeader( cluster ).database() );

			  // and a different database
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster2 = startCluster2(recordFormat);
			  Cluster<object> cluster2 = StartCluster2( RecordFormat );
			  DbRepresentation secondDatabaseRepresentation = DbRepresentation.of( ClusterLeader( cluster2 ).database() );
			  assertNotEquals( firstDatabaseRepresentation, secondDatabaseRepresentation );
			  string secondBackupAddress = CausalClusteringTestHelpers.transactionAddress( ClusterLeader( cluster2 ).database() );

			  // when backup is performed
			  assertEquals( 0, RunBackupOtherJvm( secondBackupAddress, backupName ) );
			  cluster2.Shutdown();

			  // then the new backup has the correct name
			  assertEquals( secondDatabaseRepresentation, GetBackupDbRepresentation( backupName, _backupStoreDir ) );

			  // and the old backup is in a renamed location
			  assertEquals( firstDatabaseRepresentation, GetBackupDbRepresentation( backupName + ".err.0", _backupStoreDir ) );

			  // and the data isn't equal (sanity check)
			  assertNotEquals( firstDatabaseRepresentation, secondDatabaseRepresentation );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int runBackupOtherJvm(String customAddress, String databaseName) throws Exception
		 private int RunBackupOtherJvm( string customAddress, string databaseName )
		 {
			  return RunBackupToolFromOtherJvmToGetExitCode( "--from", customAddress, "--cc-report-dir=" + _backupStoreDir, "--backup-dir=" + _backupStoreDir, "--name=" + databaseName );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void ipv6Enabled() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Ipv6Enabled()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = startIpv6Cluster();
			  Cluster<object> cluster = StartIpv6Cluster();
			  try
			  {
					assertNotNull( DbRepresentation.of( ClusterDatabase( cluster ) ) );
					int port = ClusterLeader( cluster ).config().get(CausalClusteringSettings.transaction_listen_address).Port;
					string customAddress = string.Format( "[{0}]:{1:D}", IpFamily.IPV6.localhostAddress(), port );
					string backupName = "backup_" + RecordFormat;

					// when full backup
					assertEquals( 0, RunBackupToolFromOtherJvmToGetExitCode( "--from", customAddress, "--protocol=catchup", "--cc-report-dir=" + _backupStoreDir, "--backup-dir=" + _backupStoreDir, "--name=" + backupName ) );

					// and
					CreateSomeData( cluster );

					// and incremental backup
					assertEquals( 0, RunBackupToolFromOtherJvmToGetExitCode( "--from", customAddress, "--protocol=catchup", "--cc-report-dir=" + _backupStoreDir, "--backup-dir=" + _backupStoreDir, "--name=" + backupName, Arg( ARG_NAME_FALLBACK_FULL, false ) ) );

					// then
					assertEquals( DbRepresentation.of( ClusterDatabase( cluster ) ), GetBackupDbRepresentation( backupName, _backupStoreDir ) );
			  }
			  finally
			  {
					cluster.Shutdown();
			  }
		 }

		 internal static string Arg( string key, object value )
		 {
			  return "--" + key + "=" + value;
		 }

		 internal static PrintStream WrapWithNormalOutput( PrintStream normalOutput, PrintStream nullAbleOutput )
		 {
			  if ( nullAbleOutput == null )
			  {
					return normalOutput;
			  }
			  return DuplexPrintStream( normalOutput, nullAbleOutput );
		 }

		 private static PrintStream DuplexPrintStream( PrintStream first, PrintStream second )
		 {
			  return new PrintStreamAnonymousInnerClass( first, second );
		 }

		 private class PrintStreamAnonymousInnerClass : PrintStream
		 {
			 private PrintStream _second;

			 public PrintStreamAnonymousInnerClass( PrintStream first, PrintStream second ) : base( first )
			 {
				 this._second = second;
			 }


			 public override void write( int i )
			 {
				  base.write( i );
				  _second.write( i );
			 }

			 public override void write( sbyte[] bytes, int i, int i1 )
			 {
				  base.write( bytes, i, i1 );
				  _second.write( bytes, i, i1 );
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(byte[] bytes) throws java.io.IOException
			 public override void write( sbyte[] bytes )
			 {
				  base.write( bytes );
				  _second.write( bytes );
			 }

			 public override void flush()
			 {
				  base.flush();
				  _second.flush();
			 }

			 public override void close()
			 {
				  base.close();
				  _second.close();
			 }
		 }

		 private static void RepeatedlyPopulateDatabase<T1>( Cluster<T1> cluster, AtomicBoolean continueFlagReference )
		 {
			  while ( continueFlagReference.get() )
			  {
					CreateSomeData( cluster );
			  }
		 }

		 public static CoreGraphDatabase ClusterDatabase<T1>( Cluster<T1> cluster )
		 {
			  return ClusterLeader( cluster ).database();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.causalclustering.discovery.Cluster<?> startCluster(String recordFormat) throws Exception
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 private Cluster<object> StartCluster( string recordFormat )
		 {
			  ClusterRule clusterRule = this._clusterRule.withSharedCoreParam( GraphDatabaseSettings.record_format, recordFormat ).withSharedReadReplicaParam( GraphDatabaseSettings.record_format, recordFormat );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = clusterRule.StartCluster();
			  CreateSomeData( cluster );
			  return cluster;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.causalclustering.discovery.Cluster<?> startIpv6Cluster() throws java.util.concurrent.ExecutionException, InterruptedException
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 private Cluster<object> StartIpv6Cluster()
		 {
			  DiscoveryServiceFactory discoveryServiceFactory = new SharedDiscoveryServiceFactory();
			  File parentDir = _testDirectory.directory( "ipv6_cluster" );
			  IDictionary<string, string> coreParams = new Dictionary<string, string>();
			  coreParams[GraphDatabaseSettings.record_format.name()] = RecordFormat;
			  IDictionary<string, System.Func<int, string>> instanceCoreParams = new Dictionary<string, System.Func<int, string>>();

			  IDictionary<string, string> readReplicaParams = new Dictionary<string, string>();
			  readReplicaParams[GraphDatabaseSettings.record_format.name()] = RecordFormat;
			  IDictionary<string, System.Func<int, string>> instanceReadReplicaParams = new Dictionary<string, System.Func<int, string>>();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = new org.neo4j.causalclustering.discovery.EnterpriseCluster(parentDir, 3, 3, discoveryServiceFactory, coreParams, instanceCoreParams, readReplicaParams, instanceReadReplicaParams, recordFormat, org.neo4j.causalclustering.discovery.IpFamily.IPV6, false);
			  Cluster<object> cluster = new EnterpriseCluster( parentDir, 3, 3, discoveryServiceFactory, coreParams, instanceCoreParams, readReplicaParams, instanceReadReplicaParams, RecordFormat, IpFamily.IPV6, false );
			  cluster.Start();
			  CreateSomeData( cluster );
			  return cluster;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.causalclustering.discovery.Cluster<?> startCluster2(String recordFormat) throws java.util.concurrent.ExecutionException, InterruptedException
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 private Cluster<object> StartCluster2( string recordFormat )
		 {
			  IDictionary<string, string> sharedParams = new Dictionary<string, string>();
			  sharedParams[GraphDatabaseSettings.record_format.name()] = recordFormat;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = new org.neo4j.causalclustering.discovery.EnterpriseCluster(testDirectory.directory("cluster-b_" + recordFormat), 3, 0, new org.neo4j.causalclustering.discovery.SharedDiscoveryServiceFactory(), sharedParams, emptyMap(), sharedParams, emptyMap(), recordFormat, org.neo4j.causalclustering.discovery.IpFamily.IPV4, false);
			  Cluster<object> cluster = new EnterpriseCluster( _testDirectory.directory( "cluster-b_" + recordFormat ), 3, 0, new SharedDiscoveryServiceFactory(), sharedParams, emptyMap(), sharedParams, emptyMap(), recordFormat, IpFamily.IPV4, false );
			  cluster.Start();
			  CreateSomeData( cluster );
			  return cluster;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void transactions1M(org.neo4j.causalclustering.discovery.Cluster<?> cluster) throws Exception
		 private static void Transactions1M<T1>( Cluster<T1> cluster )
		 {
			  int numberOfTransactions = 500;
			  long sizeOfTransaction = ( ByteUnit.mebiBytes( 1 ) / numberOfTransactions ) + 1;
			  for ( int txId = 0; txId < numberOfTransactions; txId++ )
			  {
					cluster.CoreTx((coreGraphDatabase, transaction) =>
					{
					 Node node = coreGraphDatabase.createNode();
					 string longString = LongStream.range( 0, sizeOfTransaction ).map( l => l % 10 ).mapToObj( long?.toString ).collect( joining( "" ) );
					 node.setProperty( "name", longString );
					 coreGraphDatabase.createNode().createRelationshipTo(node, RelationshipType.withName("KNOWS"));
					 transaction.success();
					});
			  }
		 }

		 public static void CreateSomeData<T1>( Cluster<T1> cluster )
		 {
			  try
			  {
					cluster.CoreTx( ClusterHelper.createSomeData );
			  }
			  catch ( Exception e )
			  {
					throw new Exception( e );
			  }
		 }

		 private static CoreClusterMember ClusterLeader<T1>( Cluster<T1> cluster )
		 {
			  return cluster.GetMemberWithRole( Role.LEADER );
		 }

		 public static DbRepresentation GetBackupDbRepresentation( string name, File storeDir )
		 {
			  Config config = Config.defaults();
			  config.Augment( OnlineBackupSettings.online_backup_enabled, Settings.FALSE );
			  return DbRepresentation.of( DatabaseLayout.of( storeDir, name ).databaseDirectory(), config );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int runBackupToolFromOtherJvmToGetExitCode(String... args) throws Exception
		 private int RunBackupToolFromOtherJvmToGetExitCode( params string[] args )
		 {
			  return TestHelpers.runBackupToolFromOtherJvmToGetExitCode( _testDirectory.absolutePath(), args );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private int runBackupToolFromSameJvm(String... args) throws Exception
		 private int RunBackupToolFromSameJvm( params string[] args )
		 {
			  return RunBackupToolFromSameJvmToGetExitCode( _testDirectory.absolutePath(), _testDirectory.absolutePath().Name, args );
		 }

		 /// <summary>
		 /// This unused method is used for debugging, so don't remove
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static int runBackupToolFromSameJvmToGetExitCode(java.io.File backupDir, String backupName, String... args) throws Exception
		 private static int RunBackupToolFromSameJvmToGetExitCode( File backupDir, string backupName, params string[] args )
		 {
			  return ( new OnlineBackupCommandBuilder() ).WithRawArgs(args).backup(backupDir, backupName) ? 0 : 1;
		 }
	}

}