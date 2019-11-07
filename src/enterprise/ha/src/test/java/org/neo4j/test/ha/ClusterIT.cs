using System;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Test.ha
{
	using CoreMatchers = org.hamcrest.CoreMatchers;
	using Matchers = org.hamcrest.Matchers;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using TestName = org.junit.rules.TestName;


	using ClusterSettings = Neo4Net.cluster.ClusterSettings;
	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using TransactionTerminatedException = Neo4Net.GraphDb.TransactionTerminatedException;
	using TransientTransactionFailureException = Neo4Net.GraphDb.TransientTransactionFailureException;
	using TestHighlyAvailableGraphDatabaseFactory = Neo4Net.GraphDb.factory.TestHighlyAvailableGraphDatabaseFactory;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using HaSettings = Neo4Net.Kernel.ha.HaSettings;
	using HighlyAvailableGraphDatabase = Neo4Net.Kernel.ha.HighlyAvailableGraphDatabase;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using ClusterManager = Neo4Net.Kernel.impl.ha.ClusterManager;
	using MetaDataStore = Neo4Net.Kernel.impl.store.MetaDataStore;
	using TransactionId = Neo4Net.Kernel.impl.store.TransactionId;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using LoggerRule = Neo4Net.Test.rule.LoggerRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.Exceptions.rootCause;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.pagecache.impl.muninn.StandalonePageCacheFactory.createPageCache;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.ha.ClusterManager.allSeesAllAsAvailable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.ha.ClusterManager.clusterOfSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.ha.ClusterManager.masterAvailable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.ha.ClusterManager.masterSeesSlavesAsAvailable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.scheduler.JobSchedulerFactory.createInitializedScheduler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.MetaDataStore.Position.LAST_TRANSACTION_COMMIT_TIMESTAMP;

	public class ClusterIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.LoggerRule logging = new Neo4Net.test.rule.LoggerRule(java.util.logging.Level.ALL);
		 public LoggerRule Logging = new LoggerRule( Level.ALL );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.TestDirectory testDirectory = Neo4Net.test.rule.TestDirectory.testDirectory();
		 public TestDirectory TestDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.TestName testName = new org.junit.rules.TestName();
		 public TestName TestName = new TestName();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCluster() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestCluster()
		 {
			  ClusterManager clusterManager = ( new ClusterManager.Builder( TestDirectory.directory( TestName.MethodName ) ) ).withSharedConfig( stringMap( HaSettings.tx_push_factor.name(), "2", OnlineBackupSettings.online_backup_enabled.name(), false.ToString() ) ).build();
			  CreateClusterWithNode( clusterManager );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testClusterWithHostnames() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestClusterWithHostnames()
		 {
			  ClusterManager clusterManager = ( new ClusterManager.Builder( TestDirectory.directory( TestName.MethodName ) ) ).withCluster( clusterOfSize( "localhost", 3 ) ).withSharedConfig( stringMap( HaSettings.tx_push_factor.name(), "2", OnlineBackupSettings.online_backup_enabled.name(), false.ToString() ) ).build();
			  CreateClusterWithNode( clusterManager );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testClusterWithWildcardIP() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestClusterWithWildcardIP()
		 {
			  ClusterManager clusterManager = ( new ClusterManager.Builder( TestDirectory.directory( TestName.MethodName ) ) ).withCluster( clusterOfSize( "0.0.0.0", 3 ) ).withSharedConfig( stringMap( HaSettings.tx_push_factor.name(), "2", OnlineBackupSettings.online_backup_enabled.name(), false.ToString() ) ).build();
			  CreateClusterWithNode( clusterManager );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testInstancesWithConflictingClusterPorts()
		 public virtual void TestInstancesWithConflictingClusterPorts()
		 {
			  HighlyAvailableGraphDatabase first = null;

			  int clusterPort = PortAuthority.allocatePort();

			  try
			  {
					File masterStoreDir = TestDirectory.directory( TestName.MethodName + "Master" );
					first = ( HighlyAvailableGraphDatabase ) ( new TestHighlyAvailableGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(masterStoreDir).setConfig(ClusterSettings.initial_hosts, "127.0.0.1:" + clusterPort).setConfig(ClusterSettings.cluster_server, "127.0.0.1:" + clusterPort).setConfig(ClusterSettings.server_id, "1").setConfig(HaSettings.ha_server, "127.0.0.1:" + PortAuthority.allocatePort()).setConfig(OnlineBackupSettings.online_backup_enabled, false.ToString()).newGraphDatabase();

					try
					{
						 File slaveStoreDir = TestDirectory.directory( TestName.MethodName + "Slave" );
						 HighlyAvailableGraphDatabase failed = ( HighlyAvailableGraphDatabase ) ( new TestHighlyAvailableGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(slaveStoreDir).setConfig(ClusterSettings.initial_hosts, "127.0.0.1:" + clusterPort).setConfig(ClusterSettings.cluster_server, "127.0.0.1:" + clusterPort).setConfig(ClusterSettings.server_id, "2").setConfig(HaSettings.ha_server, "127.0.0.1:" + PortAuthority.allocatePort()).setConfig(OnlineBackupSettings.online_backup_enabled, false.ToString()).newGraphDatabase();
						 failed.Shutdown();
						 fail( "Should not start when ports conflict" );
					}
					catch ( Exception )
					{
						 // good
					}
			  }
			  finally
			  {
					if ( first != null )
					{
						 first.Shutdown();
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void given4instanceClusterWhenMasterGoesDownThenElectNewMaster() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Given4instanceClusterWhenMasterGoesDownThenElectNewMaster()
		 {
			  ClusterManager clusterManager = ( new ClusterManager.Builder( TestDirectory.directory( TestName.MethodName ) ) ).withCluster( ClusterManager.clusterOfSize( 4 ) ).build();
			  try
			  {
					clusterManager.Start();
					ClusterManager.ManagedCluster cluster = clusterManager.Cluster;
					cluster.Await( allSeesAllAsAvailable() );

					Logging.Logger.info( "STOPPING MASTER" );
					cluster.Shutdown( cluster.Master );
					Logging.Logger.info( "STOPPED MASTER" );

					cluster.Await( ClusterManager.masterAvailable() );

					GraphDatabaseService master = cluster.Master;
					Logging.Logger.info( "CREATE NODE" );
					using ( Transaction tx = master.BeginTx() )
					{
						 master.CreateNode();
						 Logging.Logger.info( "CREATED NODE" );
						 tx.Success();
					}

					Logging.Logger.info( "STOPPING CLUSTER" );
			  }
			  finally
			  {
					clusterManager.SafeShutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenEmptyHostListWhenClusterStartupThenFormClusterWithSingleInstance()
		 public virtual void GivenEmptyHostListWhenClusterStartupThenFormClusterWithSingleInstance()
		 {
			  HighlyAvailableGraphDatabase db = ( HighlyAvailableGraphDatabase ) ( new TestHighlyAvailableGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(TestDirectory.directory(TestName.MethodName)).setConfig(ClusterSettings.server_id, "1").setConfig(ClusterSettings.cluster_server, "127.0.0.1:" + PortAuthority.allocatePort()).setConfig(ClusterSettings.initial_hosts, "").setConfig(HaSettings.ha_server, "127.0.0.1:" + PortAuthority.allocatePort()).setConfig(OnlineBackupSettings.online_backup_enabled, false.ToString()).newGraphDatabase();

			  try
			  {
					assertTrue( "Single instance cluster was not formed in time", Db.isAvailable( 10_000 ) );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void givenClusterWhenMasterGoesDownAndTxIsRunningThenDontWaitToSwitch() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void GivenClusterWhenMasterGoesDownAndTxIsRunningThenDontWaitToSwitch()
		 {
			  ClusterManager clusterManager = ( new ClusterManager.Builder( TestDirectory.directory( TestName.MethodName ) ) ).withCluster( ClusterManager.clusterOfSize( 3 ) ).build();
			  try
			  {
					clusterManager.Start();
					ClusterManager.ManagedCluster cluster = clusterManager.Cluster;
					cluster.Await( allSeesAllAsAvailable() );

					HighlyAvailableGraphDatabase slave = cluster.AnySlave;

					Transaction tx = slave.BeginTx();
					// Do a little write operation so that all "write" aspects of this tx is initializes properly
					slave.CreateNode();

					// Shut down master while we're keeping this transaction open
					cluster.Shutdown( cluster.Master );

					cluster.Await( masterAvailable() );
					cluster.Await( masterSeesSlavesAsAvailable( 1 ) );
					// Ending up here means that we didn't wait for this transaction to complete

					tx.Success();

					try
					{
						 tx.Close();
						 fail( "Exception expected" );
					}
					catch ( Exception e )
					{
						 assertThat( e, instanceOf( typeof( TransientTransactionFailureException ) ) );
						 Exception rootCause = rootCause( e );
						 assertThat( rootCause, instanceOf( typeof( TransactionTerminatedException ) ) );
						 assertThat( ( ( TransactionTerminatedException )rootCause ).status(), Matchers.equalTo(Neo4Net.Kernel.Api.Exceptions.Status_General.DatabaseUnavailable) );
					}
			  }
			  finally
			  {
					clusterManager.Stop();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void lastTxCommitTimestampShouldGetInitializedOnSlaveIfNotPresent() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LastTxCommitTimestampShouldGetInitializedOnSlaveIfNotPresent()
		 {
			  ClusterManager clusterManager = ( new ClusterManager.Builder( TestDirectory.directory( TestName.MethodName ) ) ).withCluster( ClusterManager.clusterOfSize( 3 ) ).build();

			  try
			  {
					clusterManager.Start();
					ClusterManager.ManagedCluster cluster = clusterManager.Cluster;
					cluster.Await( allSeesAllAsAvailable() );

					RunSomeTransactions( cluster.Master );
					cluster.Sync();

					HighlyAvailableGraphDatabase slave = cluster.AnySlave;
					DatabaseLayout databaseLayout = slave.DatabaseLayout();
					ClusterManager.RepairKit slaveRepairKit = cluster.Shutdown( slave );

					ClearLastTransactionCommitTimestampField( databaseLayout );

					HighlyAvailableGraphDatabase repairedSlave = slaveRepairKit.Repair();
					cluster.Await( allSeesAllAsAvailable() );

					assertEquals( LastCommittedTxTimestamp( cluster.Master ), LastCommittedTxTimestamp( repairedSlave ) );

			  }
			  finally
			  {
					clusterManager.Stop();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void lastTxCommitTimestampShouldBeUnknownAfterStartIfNoFiledOrLogsPresent() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LastTxCommitTimestampShouldBeUnknownAfterStartIfNoFiledOrLogsPresent()
		 {
			  ClusterManager clusterManager = ( new ClusterManager.Builder( TestDirectory.directory( TestName.MethodName ) ) ).withCluster( ClusterManager.clusterOfSize( 3 ) ).build();

			  try
			  {
					clusterManager.Start();
					ClusterManager.ManagedCluster cluster = clusterManager.Cluster;
					cluster.Await( allSeesAllAsAvailable() );

					RunSomeTransactions( cluster.Master );
					cluster.Sync();

					HighlyAvailableGraphDatabase slave = cluster.AnySlave;
					DatabaseLayout databaseLayout = slave.DatabaseLayout();
					ClusterManager.RepairKit slaveRepairKit = cluster.Shutdown( slave );

					ClearLastTransactionCommitTimestampField( databaseLayout );
					DeleteLogs( databaseLayout );

					HighlyAvailableGraphDatabase repairedSlave = slaveRepairKit.Repair();
					cluster.Await( allSeesAllAsAvailable() );

					assertEquals( Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.UNKNOWN_TX_COMMIT_TIMESTAMP, LastCommittedTxTimestamp( repairedSlave ) );
			  }
			  finally
			  {
					clusterManager.Stop();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void createClusterWithNode(Neo4Net.kernel.impl.ha.ClusterManager clusterManager) throws Throwable
		 private static void CreateClusterWithNode( ClusterManager clusterManager )
		 {
			  try
			  {
					clusterManager.Start();

					clusterManager.Cluster.await( allSeesAllAsAvailable() );

					long nodeId;
					HighlyAvailableGraphDatabase master = clusterManager.Cluster.Master;
					using ( Transaction tx = master.BeginTx() )
					{
						 Node node = master.CreateNode();
						 nodeId = node.Id;
						 node.SetProperty( "foo", "bar" );
						 tx.Success();
					}

					HighlyAvailableGraphDatabase slave = clusterManager.Cluster.AnySlave;
					using ( Transaction ignored = slave.BeginTx() )
					{
						 Node node = slave.GetNodeById( nodeId );
						 assertThat( node.GetProperty( "foo" ).ToString(), CoreMatchers.equalTo("bar") );
					}
			  }
			  finally
			  {
					clusterManager.SafeShutdown();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void deleteLogs(Neo4Net.io.layout.DatabaseLayout databaseLayout) throws java.io.IOException
		 private static void DeleteLogs( DatabaseLayout databaseLayout )
		 {
			  using ( DefaultFileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction() )
			  {
					LogFiles logFiles = LogFilesBuilder.logFilesBasedOnlyBuilder( databaseLayout.DatabaseDirectory(), fileSystem ).build();
					foreach ( File file in logFiles.LogFilesConflict() )
					{
						 fileSystem.DeleteFile( file );
					}
			  }
		 }

		 private static void RunSomeTransactions( HighlyAvailableGraphDatabase db )
		 {
			  for ( int i = 0; i < 10; i++ )
			  {
					using ( Transaction tx = Db.beginTx() )
					{
						 for ( int j = 0; j < 10; j++ )
						 {
							  Db.createNode();
						 }
						 tx.Success();
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void clearLastTransactionCommitTimestampField(Neo4Net.io.layout.DatabaseLayout databaseLayout) throws Exception
		 private static void ClearLastTransactionCommitTimestampField( DatabaseLayout databaseLayout )
		 {
			  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction(), IJobScheduler jobScheduler = createInitializedScheduler(), PageCache pageCache = createPageCache(fileSystem, jobScheduler) )
			  {
					File neoStore = databaseLayout.MetadataStore();
					MetaDataStore.setRecord( pageCache, neoStore, LAST_TRANSACTION_COMMIT_TIMESTAMP, MetaDataStore.BASE_TX_COMMIT_TIMESTAMP );
			  }
		 }

		 private static long LastCommittedTxTimestamp( HighlyAvailableGraphDatabase db )
		 {
			  DependencyResolver resolver = Db.DependencyResolver;
			  MetaDataStore metaDataStore = resolver.ResolveDependency( typeof( MetaDataStore ) );
			  TransactionId txInfo = metaDataStore.LastCommittedTransaction;
			  return txInfo.CommitTimestamp();
		 }
	}

}