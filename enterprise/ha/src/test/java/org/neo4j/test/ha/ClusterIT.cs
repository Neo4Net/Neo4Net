﻿using System;

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
namespace Org.Neo4j.Test.ha
{
	using CoreMatchers = org.hamcrest.CoreMatchers;
	using Matchers = org.hamcrest.Matchers;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using TestName = org.junit.rules.TestName;


	using ClusterSettings = Org.Neo4j.cluster.ClusterSettings;
	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using TransactionTerminatedException = Org.Neo4j.Graphdb.TransactionTerminatedException;
	using TransientTransactionFailureException = Org.Neo4j.Graphdb.TransientTransactionFailureException;
	using TestHighlyAvailableGraphDatabaseFactory = Org.Neo4j.Graphdb.factory.TestHighlyAvailableGraphDatabaseFactory;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using HaSettings = Org.Neo4j.Kernel.ha.HaSettings;
	using HighlyAvailableGraphDatabase = Org.Neo4j.Kernel.ha.HighlyAvailableGraphDatabase;
	using OnlineBackupSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using ClusterManager = Org.Neo4j.Kernel.impl.ha.ClusterManager;
	using MetaDataStore = Org.Neo4j.Kernel.impl.store.MetaDataStore;
	using TransactionId = Org.Neo4j.Kernel.impl.store.TransactionId;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using LogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Org.Neo4j.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using PortAuthority = Org.Neo4j.Ports.Allocation.PortAuthority;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using LoggerRule = Org.Neo4j.Test.rule.LoggerRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

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
//	import static org.neo4j.helpers.Exceptions.rootCause;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.impl.muninn.StandalonePageCacheFactory.createPageCache;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.allSeesAllAsAvailable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.clusterOfSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.masterAvailable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.masterSeesSlavesAsAvailable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createInitialisedScheduler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.MetaDataStore.Position.LAST_TRANSACTION_COMMIT_TIMESTAMP;

	public class ClusterIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.LoggerRule logging = new org.neo4j.test.rule.LoggerRule(java.util.logging.Level.ALL);
		 public LoggerRule Logging = new LoggerRule( Level.ALL );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
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
						 assertThat( ( ( TransactionTerminatedException )rootCause ).status(), Matchers.equalTo(Org.Neo4j.Kernel.Api.Exceptions.Status_General.DatabaseUnavailable) );
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

					assertEquals( Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.UNKNOWN_TX_COMMIT_TIMESTAMP, LastCommittedTxTimestamp( repairedSlave ) );
			  }
			  finally
			  {
					clusterManager.Stop();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void createClusterWithNode(org.neo4j.kernel.impl.ha.ClusterManager clusterManager) throws Throwable
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
//ORIGINAL LINE: private static void deleteLogs(org.neo4j.io.layout.DatabaseLayout databaseLayout) throws java.io.IOException
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
//ORIGINAL LINE: private static void clearLastTransactionCommitTimestampField(org.neo4j.io.layout.DatabaseLayout databaseLayout) throws Exception
		 private static void ClearLastTransactionCommitTimestampField( DatabaseLayout databaseLayout )
		 {
			  using ( FileSystemAbstraction fileSystem = new DefaultFileSystemAbstraction(), JobScheduler jobScheduler = createInitialisedScheduler(), PageCache pageCache = createPageCache(fileSystem, jobScheduler) )
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