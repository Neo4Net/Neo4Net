using System;

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
namespace Org.Neo4j.Kernel.ha
{
	using Before = org.junit.Before;
	using Ignore = org.junit.Ignore;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using ClusterSettings = Org.Neo4j.cluster.ClusterSettings;
	using InstanceId = Org.Neo4j.cluster.InstanceId;
	using ClusterClient = Org.Neo4j.cluster.client.ClusterClient;
	using ClusterClientModule = Org.Neo4j.cluster.client.ClusterClientModule;
	using ClusterMemberEvents = Org.Neo4j.cluster.member.ClusterMemberEvents;
	using ClusterMemberListener = Org.Neo4j.cluster.member.ClusterMemberListener;
	using ClusterConfiguration = Org.Neo4j.cluster.protocol.cluster.ClusterConfiguration;
	using ClusterListener = Org.Neo4j.cluster.protocol.cluster.ClusterListener;
	using NotElectableElectionCredentialsProvider = Org.Neo4j.cluster.protocol.election.NotElectableElectionCredentialsProvider;
	using HeartbeatListener = Org.Neo4j.cluster.protocol.heartbeat.HeartbeatListener;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using MapUtil = Org.Neo4j.Helpers.Collection.MapUtil;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using HighAvailabilityMemberState = Org.Neo4j.Kernel.ha.cluster.HighAvailabilityMemberState;
	using ClusterManager = Org.Neo4j.Kernel.impl.ha.ClusterManager;
	using RepairKit = Org.Neo4j.Kernel.impl.ha.ClusterManager.RepairKit;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using FormattedLogProvider = Org.Neo4j.Logging.FormattedLogProvider;
	using SimpleLogService = Org.Neo4j.Logging.@internal.SimpleLogService;
	using PortAuthority = Org.Neo4j.Ports.Allocation.PortAuthority;
	using ClusterRule = Org.Neo4j.Test.ha.ClusterRule;
	using CleanupRule = Org.Neo4j.Test.rule.CleanupRule;
	using RepeatRule = Org.Neo4j.Test.rule.RepeatRule;
	using SuppressOutput = Org.Neo4j.Test.rule.SuppressOutput;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cluster.protocol.cluster.ClusterConfiguration.COORDINATOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.allSeesAllAsAvailable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.masterAvailable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.masterSeesSlavesAsAvailable;

	public class ClusterTopologyChangesIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.ha.ClusterRule clusterRule = new org.neo4j.test.ha.ClusterRule();
		 public readonly ClusterRule ClusterRule = new ClusterRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.CleanupRule cleanup = new org.neo4j.test.rule.CleanupRule();
		 public readonly CleanupRule Cleanup = new CleanupRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.RepeatRule repeat = new org.neo4j.test.rule.RepeatRule();
		 public readonly RepeatRule Repeat = new RepeatRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.SuppressOutput suppressOutput = org.neo4j.test.rule.SuppressOutput.suppressAll();
		 public readonly SuppressOutput SuppressOutput = SuppressOutput.suppressAll();

		 private ClusterManager.ManagedCluster _cluster;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _cluster = ClusterRule.withSharedSetting( HaSettings.ReadTimeout, "1s" ).withSharedSetting( HaSettings.StateSwitchTimeout, "2s" ).withSharedSetting( HaSettings.ComChunkSize, "1024" ).startCluster();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void masterRejoinsAfterFailureAndReelection() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MasterRejoinsAfterFailureAndReelection()
		 {
			  // Given
			  HighlyAvailableGraphDatabase initialMaster = _cluster.Master;

			  // When
			  _cluster.info( "Fail master" );
			  ClusterManager.RepairKit kit = _cluster.fail( initialMaster );

			  _cluster.info( "Wait for 2 to become master and 3 slave" );
			  _cluster.await( masterAvailable( initialMaster ) );
			  _cluster.await( masterSeesSlavesAsAvailable( 1 ) );

			  _cluster.info( "Repair 1" );
			  kit.Repair();

			  // Then
			  _cluster.info( "Wait for cluster recovery" );
			  _cluster.await( masterAvailable() );
			  _cluster.await( allSeesAllAsAvailable() );
			  assertEquals( 3, _cluster.size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Ignore public void slaveShouldServeTxsAfterMasterLostQuorumWentToPendingAndThenQuorumWasRestored() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SlaveShouldServeTxsAfterMasterLostQuorumWentToPendingAndThenQuorumWasRestored()
		 {
			  // GIVEN: cluster with 3 members
			  HighlyAvailableGraphDatabase master = _cluster.Master;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final HighlyAvailableGraphDatabase slave1 = cluster.getAnySlave();
			  HighlyAvailableGraphDatabase slave1 = _cluster.AnySlave;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final HighlyAvailableGraphDatabase slave2 = cluster.getAnySlave(slave1);
			  HighlyAvailableGraphDatabase slave2 = _cluster.getAnySlave( slave1 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch slave1Left = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent slave1Left = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch slave2Left = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent slave2Left = new System.Threading.CountdownEvent( 1 );

			  ClusterClientOf( master ).addHeartbeatListener( new HeartbeatListener_AdapterAnonymousInnerClass( this, slave1, slave2, slave1Left, slave2Left ) );

			  // fail slave1 and await master to spot the failure
			  ClusterManager.RepairKit slave1RepairKit = _cluster.fail( slave1 );
			  assertTrue( slave1Left.await( 60, SECONDS ) );

			  // fail slave2 and await master to spot the failure
			  ClusterManager.RepairKit slave2RepairKit = _cluster.fail( slave2 );
			  assertTrue( slave2Left.await( 60, SECONDS ) );

			  // master loses quorum and goes to PENDING, cluster is unavailable
			  _cluster.await( masterAvailable().negate() );
			  assertEquals( HighAvailabilityMemberState.PENDING, master.InstanceState );

			  // WHEN: both slaves are repaired, majority restored, quorum can be achieved
			  slave1RepairKit.Repair();
			  slave2RepairKit.Repair();

			  // whole cluster looks fine, but slaves have stale value of the epoch if they rejoin the cluster in SLAVE state
			  _cluster.await( masterAvailable() );
			  _cluster.await( masterSeesSlavesAsAvailable( 2 ) );
			  HighlyAvailableGraphDatabase newMaster = _cluster.Master;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final HighlyAvailableGraphDatabase newSlave1 = cluster.getAnySlave();
			  HighlyAvailableGraphDatabase newSlave1 = _cluster.AnySlave;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final HighlyAvailableGraphDatabase newSlave2 = cluster.getAnySlave(newSlave1);
			  HighlyAvailableGraphDatabase newSlave2 = _cluster.getAnySlave( newSlave1 );

			  // now adding another failing listener and wait for the failure due to stale epoch
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch slave1Unavailable = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent slave1Unavailable = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch slave2Unavailable = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent slave2Unavailable = new System.Threading.CountdownEvent( 1 );
			  ClusterMemberEvents clusterEvents = newMaster.DependencyResolver.resolveDependency( typeof( ClusterMemberEvents ) );
			  clusterEvents.AddClusterMemberListener( new ClusterMemberListener_AdapterAnonymousInnerClass( this, newSlave1, newSlave2, slave1Unavailable, slave2Unavailable ) );

			  // attempt to perform transactions on both slaves throws, election is triggered
			  AttemptTransactions( newSlave1, newSlave2 );
			  // set a timeout in case the instance does not have stale epoch
			  assertTrue( slave1Unavailable.await( 60, TimeUnit.SECONDS ) );
			  assertTrue( slave2Unavailable.await( 60, TimeUnit.SECONDS ) );

			  // THEN: done with election, cluster feels good and able to serve transactions
			  _cluster.info( "Waiting for cluster to stabilize" );
			  _cluster.await( allSeesAllAsAvailable() );

			  _cluster.info( "Assert ok" );
			  assertNotNull( CreateNodeOn( newMaster ) );
			  assertNotNull( CreateNodeOn( newSlave1 ) );
			  assertNotNull( CreateNodeOn( newSlave2 ) );
		 }

		 private class HeartbeatListener_AdapterAnonymousInnerClass : Org.Neo4j.cluster.protocol.heartbeat.HeartbeatListener_Adapter
		 {
			 private readonly ClusterTopologyChangesIT _outerInstance;

			 private Org.Neo4j.Kernel.ha.HighlyAvailableGraphDatabase _slave1;
			 private Org.Neo4j.Kernel.ha.HighlyAvailableGraphDatabase _slave2;
			 private System.Threading.CountdownEvent _slave1Left;
			 private System.Threading.CountdownEvent _slave2Left;

			 public HeartbeatListener_AdapterAnonymousInnerClass( ClusterTopologyChangesIT outerInstance, Org.Neo4j.Kernel.ha.HighlyAvailableGraphDatabase slave1, Org.Neo4j.Kernel.ha.HighlyAvailableGraphDatabase slave2, System.Threading.CountdownEvent slave1Left, System.Threading.CountdownEvent slave2Left )
			 {
				 this.outerInstance = outerInstance;
				 this._slave1 = slave1;
				 this._slave2 = slave2;
				 this._slave1Left = slave1Left;
				 this._slave2Left = slave2Left;
			 }

			 public override void failed( InstanceId server )
			 {
				  if ( InstanceIdOf( _slave1 ).Equals( server ) )
				  {
						_slave1Left.Signal();
				  }
				  else if ( InstanceIdOf( _slave2 ).Equals( server ) )
				  {
						_slave2Left.Signal();
				  }
			 }
		 }

		 private class ClusterMemberListener_AdapterAnonymousInnerClass : Org.Neo4j.cluster.member.ClusterMemberListener_Adapter
		 {
			 private readonly ClusterTopologyChangesIT _outerInstance;

			 private Org.Neo4j.Kernel.ha.HighlyAvailableGraphDatabase _newSlave1;
			 private Org.Neo4j.Kernel.ha.HighlyAvailableGraphDatabase _newSlave2;
			 private System.Threading.CountdownEvent _slave1Unavailable;
			 private System.Threading.CountdownEvent _slave2Unavailable;

			 public ClusterMemberListener_AdapterAnonymousInnerClass( ClusterTopologyChangesIT outerInstance, Org.Neo4j.Kernel.ha.HighlyAvailableGraphDatabase newSlave1, Org.Neo4j.Kernel.ha.HighlyAvailableGraphDatabase newSlave2, System.Threading.CountdownEvent slave1Unavailable, System.Threading.CountdownEvent slave2Unavailable )
			 {
				 this.outerInstance = outerInstance;
				 this._newSlave1 = newSlave1;
				 this._newSlave2 = newSlave2;
				 this._slave1Unavailable = slave1Unavailable;
				 this._slave2Unavailable = slave2Unavailable;
			 }

			 public override void memberIsUnavailable( string role, InstanceId unavailableId )
			 {
				  if ( InstanceIdOf( _newSlave1 ).Equals( unavailableId ) )
				  {
						_slave1Unavailable.Signal();
				  }
				  else if ( InstanceIdOf( _newSlave2 ).Equals( unavailableId ) )
				  {
						_slave2Unavailable.Signal();
				  }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failedInstanceShouldReceiveCorrectCoordinatorIdUponRejoiningCluster() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailedInstanceShouldReceiveCorrectCoordinatorIdUponRejoiningCluster()
		 {
			  // Given
			  HighlyAvailableGraphDatabase initialMaster = _cluster.Master;

			  // When
			  _cluster.shutdown( initialMaster );
			  _cluster.await( masterAvailable( initialMaster ) );
			  _cluster.await( masterSeesSlavesAsAvailable( 1 ) );

			  // create node on new master to ensure that it has the greatest tx id
			  CreateNodeOn( _cluster.Master );
			  _cluster.sync();

			  LifeSupport life = new LifeSupport();
			  ClusterClientModule clusterClient = NewClusterClient( life, new InstanceId( 1 ) );
			  Cleanup.add( life );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicReference<org.neo4j.cluster.InstanceId> coordinatorIdWhenReJoined = new java.util.concurrent.atomic.AtomicReference<>();
			  AtomicReference<InstanceId> coordinatorIdWhenReJoined = new AtomicReference<InstanceId>();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch latch = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( 1 );
			  clusterClient.ClusterClient.addClusterListener( new ClusterListener_AdapterAnonymousInnerClass( this, coordinatorIdWhenReJoined, latch ) );

			  life.Start();

			  // Then
			  assertTrue( latch.await( 20, SECONDS ) );
			  assertEquals( new InstanceId( 2 ), coordinatorIdWhenReJoined.get() );
		 }

		 private class ClusterListener_AdapterAnonymousInnerClass : Org.Neo4j.cluster.protocol.cluster.ClusterListener_Adapter
		 {
			 private readonly ClusterTopologyChangesIT _outerInstance;

			 private AtomicReference<InstanceId> _coordinatorIdWhenReJoined;
			 private System.Threading.CountdownEvent _latch;

			 public ClusterListener_AdapterAnonymousInnerClass( ClusterTopologyChangesIT outerInstance, AtomicReference<InstanceId> coordinatorIdWhenReJoined, System.Threading.CountdownEvent latch )
			 {
				 this.outerInstance = outerInstance;
				 this._coordinatorIdWhenReJoined = coordinatorIdWhenReJoined;
				 this._latch = latch;
			 }

			 public override void enteredCluster( ClusterConfiguration clusterConfiguration )
			 {
				  _coordinatorIdWhenReJoined.set( clusterConfiguration.GetElected( COORDINATOR ) );
				  _latch.Signal();
			 }
		 }

		 private static ClusterClient ClusterClientOf( HighlyAvailableGraphDatabase db )
		 {
			  return Db.DependencyResolver.resolveDependency( typeof( ClusterClient ) );
		 }

		 private static InstanceId InstanceIdOf( HighlyAvailableGraphDatabase db )
		 {
			  return ClusterClientOf( db ).ServerId;
		 }

		 private static Node CreateNodeOn( HighlyAvailableGraphDatabase db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();
					node.SetProperty( "key", DateTimeHelper.CurrentUnixTimeMillis().ToString() );
					tx.Success();
					return node;
			  }
		 }

		 private ClusterClientModule NewClusterClient( LifeSupport life, InstanceId id )
		 {
			  Config config = Config.defaults( MapUtil.stringMap( ClusterSettings.initial_hosts.name(), _cluster.InitialHostsConfigString, ClusterSettings.server_id.name(), id.ToIntegerIndex().ToString(), ClusterSettings.cluster_server.name(), "0.0.0.0:" + PortAuthority.allocatePort() ) );

			  FormattedLogProvider logProvider = FormattedLogProvider.toOutputStream( System.out );
			  SimpleLogService logService = new SimpleLogService( logProvider, logProvider );

			  return new ClusterClientModule( life, new Dependencies(), new Monitors(), config, logService, new NotElectableElectionCredentialsProvider() );
		 }

		 private static void AttemptTransactions( params HighlyAvailableGraphDatabase[] dbs )
		 {
			  foreach ( HighlyAvailableGraphDatabase db in dbs )
			  {
					try
					{
						 CreateNodeOn( db );
					}
					catch ( Exception )
					{
					}
			  }
		 }
	}

}