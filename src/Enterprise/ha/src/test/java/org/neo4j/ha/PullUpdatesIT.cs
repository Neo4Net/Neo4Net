using System;
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
namespace Neo4Net.ha
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using ClusterSettings = Neo4Net.cluster.ClusterSettings;
	using InstanceId = Neo4Net.cluster.InstanceId;
	using ClusterClient = Neo4Net.cluster.client.ClusterClient;
	using ClusterListener = Neo4Net.cluster.protocol.cluster.ClusterListener;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using NotFoundException = Neo4Net.Graphdb.NotFoundException;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using TransientTransactionFailureException = Neo4Net.Graphdb.TransientTransactionFailureException;
	using TestHighlyAvailableGraphDatabaseFactory = Neo4Net.Graphdb.factory.TestHighlyAvailableGraphDatabaseFactory;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using HaSettings = Neo4Net.Kernel.ha.HaSettings;
	using HighlyAvailableGraphDatabase = Neo4Net.Kernel.ha.HighlyAvailableGraphDatabase;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using ClusterManager = Neo4Net.Kernel.impl.ha.ClusterManager;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using LogService = Neo4Net.Logging.@internal.LogService;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.allSeesAllAsAvailable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.masterAvailable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.masterSeesSlavesAsAvailable;

	public class PullUpdatesIT
	{
		 private const int PULL_INTERVAL = 100;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.ha.ClusterRule clusterRule = new org.neo4j.test.ha.ClusterRule();
		 public readonly ClusterRule ClusterRule = new ClusterRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureUpdatePullerGetsGoingAfterMasterSwitch() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MakeSureUpdatePullerGetsGoingAfterMasterSwitch()
		 {
			  ClusterManager.ManagedCluster cluster = ClusterRule.withSharedSetting( HaSettings.pull_interval, PULL_INTERVAL + "ms" ).startCluster();

			  cluster.Info( "### Creating initial dataset" );
			  long commonNodeId = CreateNodeOnMaster( cluster );

			  HighlyAvailableGraphDatabase master = cluster.Master;
			  SetProperty( master, commonNodeId, 1 );
			  cluster.Info( "### Initial dataset created" );
			  AwaitPropagation( 1, commonNodeId, cluster );

			  cluster.Info( "### Shutting down master" );
			  ClusterManager.RepairKit masterShutdownRK = cluster.Shutdown( master );

			  cluster.Info( "### Awaiting new master" );
			  cluster.Await( masterAvailable( master ) );
			  cluster.Await( masterSeesSlavesAsAvailable( 1 ) );

			  cluster.Info( "### Doing a write to master" );
			  SetProperty( cluster.Master, commonNodeId, 2 );
			  AwaitPropagation( 2, commonNodeId, cluster, master );

			  cluster.Info( "### Repairing cluster" );
			  masterShutdownRK.Repair();
			  cluster.Await( masterAvailable() );
			  cluster.Await( masterSeesSlavesAsAvailable( 2 ) );
			  cluster.Await( allSeesAllAsAvailable() );

			  cluster.Info( "### Awaiting change propagation" );
			  AwaitPropagation( 2, commonNodeId, cluster );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void terminatedTransactionDoesNotForceUpdatePulling()
		 public virtual void TerminatedTransactionDoesNotForceUpdatePulling()
		 {
			  int testTxsOnMaster = 42;
			  ClusterManager.ManagedCluster cluster = ClusterRule.withSharedSetting( HaSettings.pull_interval, "0s" ).withSharedSetting( HaSettings.tx_push_factor, "0" ).startCluster();

			  HighlyAvailableGraphDatabase master = cluster.Master;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.ha.HighlyAvailableGraphDatabase slave = cluster.getAnySlave();
			  HighlyAvailableGraphDatabase slave = cluster.AnySlave;

			  CreateNodeOn( master );
			  cluster.Sync();

			  long lastClosedTxIdOnMaster = LastClosedTxIdOn( master );
			  long lastClosedTxIdOnSlave = LastClosedTxIdOn( slave );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch slaveTxStarted = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent slaveTxStarted = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch slaveShouldCommit = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent slaveShouldCommit = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicReference<org.neo4j.graphdb.Transaction> slaveTx = new java.util.concurrent.atomic.AtomicReference<>();
			  AtomicReference<Transaction> slaveTx = new AtomicReference<Transaction>();
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> slaveCommit = java.util.concurrent.Executors.newSingleThreadExecutor().submit(() ->
			  Future<object> slaveCommit = Executors.newSingleThreadExecutor().submit(() =>
			  {
				using ( Transaction tx = slave.BeginTx() )
				{
					 slaveTx.set( tx );
					 slaveTxStarted.Signal();
					 Await( slaveShouldCommit );
					 tx.success();
				}
			  });

			  Await( slaveTxStarted );
			  CreateNodesOn( master, testTxsOnMaster );

			  assertNotNull( slaveTx.get() );
			  slaveTx.get().terminate();
			  slaveShouldCommit.Signal();

			  try
			  {
					slaveCommit.get();
					fail( "Exception expected" );
			  }
			  catch ( Exception e )
			  {
					assertThat( e, instanceOf( typeof( ExecutionException ) ) );
					assertThat( e.InnerException, instanceOf( typeof( TransientTransactionFailureException ) ) );
			  }

			  assertEquals( lastClosedTxIdOnMaster + testTxsOnMaster, LastClosedTxIdOn( master ) );
			  assertEquals( lastClosedTxIdOnSlave, LastClosedTxIdOn( slave ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPullUpdatesOnStartupNoMatterWhat() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPullUpdatesOnStartupNoMatterWhat()
		 {
			  HighlyAvailableGraphDatabase slave = null;
			  HighlyAvailableGraphDatabase master = null;
			  try
			  {
					File testRootDir = ClusterRule.cleanDirectory( "shouldPullUpdatesOnStartupNoMatterWhat" );
					File masterDir = ClusterRule.TestDirectory.databaseDir( "master" );
					int masterClusterPort = PortAuthority.allocatePort();
					master = ( HighlyAvailableGraphDatabase ) ( new TestHighlyAvailableGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(masterDir).setConfig(ClusterSettings.server_id, "1").setConfig(ClusterSettings.cluster_server, "127.0.0.1:" + masterClusterPort).setConfig(ClusterSettings.initial_hosts, "localhost:" + masterClusterPort).setConfig(HaSettings.ha_server, "127.0.0.1:" + PortAuthority.allocatePort()).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newGraphDatabase();

					// Copy the store, then shutdown, so update pulling later makes sense
					File slaveDir = ClusterRule.TestDirectory.databaseDir( "slave" );
					slave = ( HighlyAvailableGraphDatabase ) ( new TestHighlyAvailableGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(slaveDir).setConfig(ClusterSettings.server_id, "2").setConfig(ClusterSettings.cluster_server, "127.0.0.1:" + PortAuthority.allocatePort()).setConfig(ClusterSettings.initial_hosts, "localhost:" + masterClusterPort).setConfig(HaSettings.ha_server, "127.0.0.1:" + PortAuthority.allocatePort()).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newGraphDatabase();

					// Required to block until the slave has left for sure
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch slaveLeftLatch = new java.util.concurrent.CountDownLatch(1);
					System.Threading.CountdownEvent slaveLeftLatch = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.cluster.client.ClusterClient masterClusterClient = master.getDependencyResolver().resolveDependency(org.neo4j.cluster.client.ClusterClient.class);
					ClusterClient masterClusterClient = master.DependencyResolver.resolveDependency( typeof( ClusterClient ) );
					masterClusterClient.AddClusterListener( new ClusterListener_AdapterAnonymousInnerClass( this, slaveLeftLatch, masterClusterClient ) );

					master.DependencyResolver.resolveDependency( typeof( LogService ) ).getInternalLog( this.GetType() ).info("SHUTTING DOWN SLAVE");
					slave.Shutdown();
					slave = null;

					// Make sure that the slave has left, because shutdown() may return before the master knows
					assertTrue( "Timeout waiting for slave to leave", slaveLeftLatch.await( 60, TimeUnit.SECONDS ) );

					long nodeId;
					using ( Transaction tx = master.BeginTx() )
					{
						 Node node = master.CreateNode();
						 node.SetProperty( "from", "master" );
						 nodeId = node.Id;
						 tx.Success();
					}

					// Store is already in place, should pull updates
					slave = ( HighlyAvailableGraphDatabase ) ( new TestHighlyAvailableGraphDatabaseFactory() ).newEmbeddedDatabaseBuilder(slaveDir).setConfig(ClusterSettings.server_id, "2").setConfig(ClusterSettings.cluster_server, "127.0.0.1:" + PortAuthority.allocatePort()).setConfig(ClusterSettings.initial_hosts, "localhost:" + masterClusterPort).setConfig(HaSettings.ha_server, "127.0.0.1:" + PortAuthority.allocatePort()).setConfig(HaSettings.pull_interval, "0").setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newGraphDatabase();

					slave.BeginTx().close(); // Make sure switch to slave completes and so does the update pulling on startup

					using ( Transaction tx = slave.BeginTx() )
					{
						 assertEquals( "master", slave.GetNodeById( nodeId ).getProperty( "from" ) );
						 tx.Success();
					}
			  }
			  finally
			  {
					if ( slave != null )
					{
						 slave.Shutdown();
					}
					if ( master != null )
					{
						 master.Shutdown();
					}
			  }
		 }

		 private class ClusterListener_AdapterAnonymousInnerClass : Neo4Net.cluster.protocol.cluster.ClusterListener_Adapter
		 {
			 private readonly PullUpdatesIT _outerInstance;

			 private System.Threading.CountdownEvent _slaveLeftLatch;
			 private ClusterClient _masterClusterClient;

			 public ClusterListener_AdapterAnonymousInnerClass( PullUpdatesIT outerInstance, System.Threading.CountdownEvent slaveLeftLatch, ClusterClient masterClusterClient )
			 {
				 this.outerInstance = outerInstance;
				 this._slaveLeftLatch = slaveLeftLatch;
				 this._masterClusterClient = masterClusterClient;
			 }

			 public override void leftCluster( InstanceId instanceId, URI member )
			 {
				  _slaveLeftLatch.Signal();
				  _masterClusterClient.removeClusterListener( this );
			 }
		 }

		 private long CreateNodeOnMaster( ClusterManager.ManagedCluster cluster )
		 {
			  return CreateNodeOn( cluster.Master );
		 }

		 private static void CreateNodesOn( GraphDatabaseService db, int count )
		 {
			  for ( int i = 0; i < count; i++ )
			  {
					CreateNodeOn( db );
			  }
		 }

		 private static long CreateNodeOn( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					long id = Db.createNode().Id;
					tx.Success();
					return id;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void powerNap() throws InterruptedException
		 private void PowerNap()
		 {
			  Thread.Sleep( 50 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void awaitPropagation(int expectedPropertyValue, long nodeId, org.neo4j.kernel.impl.ha.ClusterManager.ManagedCluster cluster, org.neo4j.kernel.ha.HighlyAvailableGraphDatabase... excepts) throws Exception
		 private void AwaitPropagation( int expectedPropertyValue, long nodeId, ClusterManager.ManagedCluster cluster, params HighlyAvailableGraphDatabase[] excepts )
		 {
			  long endTime = currentTimeMillis() + PULL_INTERVAL * 20;
			  bool ok = false;
			  while ( !ok && currentTimeMillis() < endTime )
			  {
					ok = true;
					foreach ( HighlyAvailableGraphDatabase db in cluster.AllMembers )
					{
						 foreach ( HighlyAvailableGraphDatabase except in excepts )
						 {
							  if ( db == except )
							  {
									goto loopContinue;
							  }
						 }
						 try
						 {
								 using ( Transaction tx = Db.beginTx() )
								 {
								  Number value = ( Number ) Db.getNodeById( nodeId ).getProperty( "i", null );
								  if ( value == null || value.intValue() != expectedPropertyValue )
								  {
										ok = false;
								  }
								 }
						 }
						 catch ( NotFoundException )
						 {
							  ok = false;
						 }
						loopContinue:;
					}
					loopBreak:
					if ( !ok )
					{
						 PowerNap();
					}
			  }
			  assertTrue( "Change wasn't propagated by pulling updates", ok );
		 }

		 private void SetProperty( HighlyAvailableGraphDatabase db, long nodeId, int i )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.getNodeById( nodeId ).setProperty( "i", i );
					tx.Success();
			  }
		 }

		 private void Await( System.Threading.CountdownEvent latch )
		 {
			  try
			  {
					assertTrue( latch.await( 1, TimeUnit.MINUTES ) );
			  }
			  catch ( InterruptedException e )
			  {
					throw new AssertionError( e );
			  }
		 }

		 private long LastClosedTxIdOn( GraphDatabaseAPI db )
		 {
			  TransactionIdStore txIdStore = Db.DependencyResolver.resolveDependency( typeof( TransactionIdStore ) );
			  return txIdStore.LastClosedTransactionId;
		 }
	}

}