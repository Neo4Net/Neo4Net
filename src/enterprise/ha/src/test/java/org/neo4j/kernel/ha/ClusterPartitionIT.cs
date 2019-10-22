using System;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.ha
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using ClusterSettings = Neo4Net.cluster.ClusterSettings;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using TransactionFailureException = Neo4Net.GraphDb.TransactionFailureException;
	using TransientDatabaseFailureException = Neo4Net.GraphDb.TransientDatabaseFailureException;
	using TestRunConditions = Neo4Net.ha.TestRunConditions;
	using HighAvailabilityMemberChangeEvent = Neo4Net.Kernel.ha.cluster.HighAvailabilityMemberChangeEvent;
	using HighAvailabilityMemberListener = Neo4Net.Kernel.ha.cluster.HighAvailabilityMemberListener;
	using HighAvailabilityMemberStateMachine = Neo4Net.Kernel.ha.cluster.HighAvailabilityMemberStateMachine;
	using ClusterManager = Neo4Net.Kernel.impl.ha.ClusterManager;
	using NetworkFlag = Neo4Net.Kernel.impl.ha.ClusterManager.NetworkFlag;
	using LoggerRule = Neo4Net.Test.rule.LoggerRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assume.assumeTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.ha.cluster.HighAvailabilityMemberState.PENDING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.ha.ClusterManager.allSeesAllAsAvailable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.ha.ClusterManager.instanceEvicted;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.ha.ClusterManager.masterAvailable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.ha.ClusterManager.masterSeesSlavesAsAvailable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.ha.ClusterManager.memberSeesOtherMemberAsFailed;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.rule.DatabaseRule.tx;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.rule.RetryACoupleOfTimesHandler.TRANSIENT_ERRORS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.rule.RetryACoupleOfTimesHandler.retryACoupleOfTimesOn;

	public class ClusterPartitionIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.LoggerRule logger = new org.Neo4Net.test.rule.LoggerRule();
		 public LoggerRule Logger = new LoggerRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.TestDirectory dir = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public TestDirectory Dir = TestDirectory.testDirectory();

		 private readonly string _testPropKey = "testPropKey";
		 private readonly string _testPropValue = "testPropValue";
		 private long _testNodeId;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void isolatedMasterShouldRemoveSelfFromClusterAndBecomeReadOnly() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IsolatedMasterShouldRemoveSelfFromClusterAndBecomeReadOnly()
		 {
			  int clusterSize = 3;

			  ClusterManager manager = ( new ClusterManager.Builder() ).withRootDirectory(Dir.cleanDirectory("testcluster")).withCluster(ClusterManager.clusterOfSize(clusterSize)).build();

			  try
			  {
					manager.Start();
					ClusterManager.ManagedCluster cluster = manager.Cluster;

					cluster.Await( allSeesAllAsAvailable() );
					cluster.Await( masterAvailable() );

					HighlyAvailableGraphDatabase oldMaster = cluster.Master;

					System.Threading.CountdownEvent masterTransitionLatch = new System.Threading.CountdownEvent( 1 );
					SetupForWaitOnSwitchToDetached( oldMaster, masterTransitionLatch );

					AddSomeData( oldMaster );

					ClusterManager.RepairKit fail = cluster.fail( oldMaster, Enum.GetValues( typeof( ClusterManager.NetworkFlag ) ) );
					cluster.Await( instanceEvicted( oldMaster ), 20 );

					masterTransitionLatch.await();

					EnsureInstanceIsReadOnlyInPendingState( oldMaster );

					fail.Repair();

					cluster.Await( allSeesAllAsAvailable() );

					EnsureInstanceIsWritable( oldMaster );
			  }
			  finally
			  {
					manager.SafeShutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void isolatedSlaveShouldRemoveSelfFromClusterAndBecomeReadOnly() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IsolatedSlaveShouldRemoveSelfFromClusterAndBecomeReadOnly()
		 {
			  int clusterSize = 3;

			  ClusterManager manager = ( new ClusterManager.Builder() ).withRootDirectory(Dir.cleanDirectory("testcluster")).withCluster(ClusterManager.clusterOfSize(clusterSize)).build();

			  try
			  {
					manager.Start();
					ClusterManager.ManagedCluster cluster = manager.Cluster;

					cluster.Await( allSeesAllAsAvailable() );
					cluster.Await( masterAvailable() );

					HighlyAvailableGraphDatabase slave = cluster.AnySlave;

					System.Threading.CountdownEvent slaveTransitionLatch = new System.Threading.CountdownEvent( 1 );
					SetupForWaitOnSwitchToDetached( slave, slaveTransitionLatch );

					AddSomeData( slave );

					ClusterManager.RepairKit fail = cluster.fail( slave, Enum.GetValues( typeof( ClusterManager.NetworkFlag ) ) );
					cluster.Await( instanceEvicted( slave ), 20 );

					slaveTransitionLatch.await();

					EnsureInstanceIsReadOnlyInPendingState( slave );

					fail.Repair();

					cluster.Await( allSeesAllAsAvailable() );

					EnsureInstanceIsWritable( slave );
			  }
			  finally
			  {
					manager.SafeShutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void losingQuorumIncrementallyShouldMakeAllInstancesPendingAndReadOnly() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LosingQuorumIncrementallyShouldMakeAllInstancesPendingAndReadOnly()
		 {
			  int clusterSize = 5; // we need 5 to differentiate between all other instances gone and just quorum being gone
			  assumeTrue( TestRunConditions.shouldRunAtClusterSize( clusterSize ) );

			  ClusterManager manager = ( new ClusterManager.Builder() ).withRootDirectory(Dir.cleanDirectory("testcluster")).withCluster(ClusterManager.clusterOfSize(clusterSize)).withSharedConfig(Config()).build();

			  try
			  {
					manager.Start();
					ClusterManager.ManagedCluster cluster = manager.Cluster;

					cluster.Await( allSeesAllAsAvailable() );
					cluster.Await( masterAvailable() );

					HighlyAvailableGraphDatabase master = cluster.Master;
					AddSomeData( master );

					/*
					 * we need 3 failures. We'll end up with the old master and a slave connected. They should both be in
					 * PENDING state, allowing reads but not writes. Repairing just one of the removed instances should
					 * result in a master being elected and all instances being read and writable.
					 * The instances we remove do not need additional verification for their state. Their behaviour is already
					 * known by other tests.
					 */
					HighlyAvailableGraphDatabase failed1;
					ClusterManager.RepairKit rk1;
					HighlyAvailableGraphDatabase failed2;
					HighlyAvailableGraphDatabase failed3;
					HighlyAvailableGraphDatabase remainingSlave;

					failed1 = cluster.AnySlave;
					failed2 = cluster.GetAnySlave( failed1 );
					failed3 = cluster.GetAnySlave( failed1, failed2 );
					remainingSlave = cluster.GetAnySlave( failed1, failed2, failed3 );

					System.Threading.CountdownEvent masterTransitionLatch = new System.Threading.CountdownEvent( 1 );
					System.Threading.CountdownEvent slaveTransitionLatch = new System.Threading.CountdownEvent( 1 );

					SetupForWaitOnSwitchToDetached( master, masterTransitionLatch );
					SetupForWaitOnSwitchToDetached( remainingSlave, slaveTransitionLatch );

					rk1 = KillIncrementally( cluster, failed1, failed2, failed3 );

					cluster.Await( memberSeesOtherMemberAsFailed( remainingSlave, failed1 ) );
					cluster.Await( memberSeesOtherMemberAsFailed( remainingSlave, failed2 ) );
					cluster.Await( memberSeesOtherMemberAsFailed( remainingSlave, failed3 ) );

					cluster.Await( memberSeesOtherMemberAsFailed( master, failed1 ) );
					cluster.Await( memberSeesOtherMemberAsFailed( master, failed2 ) );
					cluster.Await( memberSeesOtherMemberAsFailed( master, failed3 ) );

					masterTransitionLatch.await();
					slaveTransitionLatch.await();

					EnsureInstanceIsReadOnlyInPendingState( master );
					EnsureInstanceIsReadOnlyInPendingState( remainingSlave );

					rk1.Repair();

					cluster.Await( masterAvailable( failed2, failed3 ) );
					cluster.Await( masterSeesSlavesAsAvailable( 2 ) );

					EnsureInstanceIsWritable( master );
					EnsureInstanceIsWritable( remainingSlave );
					EnsureInstanceIsWritable( failed1 );

			  }
			  finally
			  {
					manager.Shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void losingQuorumAbruptlyShouldMakeAllInstancesPendingAndReadOnly() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LosingQuorumAbruptlyShouldMakeAllInstancesPendingAndReadOnly()
		 {
			  int clusterSize = 5; // we need 5 to differentiate between all other instances gone and just quorum being gone
			  assumeTrue( TestRunConditions.shouldRunAtClusterSize( clusterSize ) );

			  ClusterManager manager = ( new ClusterManager.Builder() ).withRootDirectory(Dir.cleanDirectory("testcluster")).withCluster(ClusterManager.clusterOfSize(clusterSize)).withSharedConfig(Config()).build();

			  try
			  {
					manager.Start();
					ClusterManager.ManagedCluster cluster = manager.Cluster;

					cluster.Await( allSeesAllAsAvailable() );
					cluster.Await( masterAvailable() );

					HighlyAvailableGraphDatabase master = cluster.Master;
					AddSomeData( master );

					/*
					 * we need 3 failures. We'll end up with the old master and a slave connected. They should both be in
					 * PENDING state, allowing reads but not writes. Repairing just one of the removed instances should
					 * result in a master being elected and all instances being read and writable.
					 * The instances we remove do not need additional verification for their state. Their behaviour is already
					 * known by other tests.
					 */
					HighlyAvailableGraphDatabase failed1;
					ClusterManager.RepairKit rk1;
					HighlyAvailableGraphDatabase failed2;
					HighlyAvailableGraphDatabase failed3;
					HighlyAvailableGraphDatabase remainingSlave;

					failed1 = cluster.AnySlave;
					failed2 = cluster.GetAnySlave( failed1 );
					failed3 = cluster.GetAnySlave( failed1, failed2 );
					remainingSlave = cluster.GetAnySlave( failed1, failed2, failed3 );

					System.Threading.CountdownEvent masterTransitionLatch = new System.Threading.CountdownEvent( 1 );
					System.Threading.CountdownEvent slaveTransitionLatch = new System.Threading.CountdownEvent( 1 );

					SetupForWaitOnSwitchToDetached( master, masterTransitionLatch );
					SetupForWaitOnSwitchToDetached( remainingSlave, slaveTransitionLatch );

					rk1 = KillAbruptly( cluster, failed1, failed2, failed3 );

					cluster.Await( memberSeesOtherMemberAsFailed( remainingSlave, failed1 ) );
					cluster.Await( memberSeesOtherMemberAsFailed( remainingSlave, failed2 ) );
					cluster.Await( memberSeesOtherMemberAsFailed( remainingSlave, failed3 ) );

					cluster.Await( memberSeesOtherMemberAsFailed( master, failed1 ) );
					cluster.Await( memberSeesOtherMemberAsFailed( master, failed2 ) );
					cluster.Await( memberSeesOtherMemberAsFailed( master, failed3 ) );

					masterTransitionLatch.await();
					slaveTransitionLatch.await();

					EnsureInstanceIsReadOnlyInPendingState( master );
					EnsureInstanceIsReadOnlyInPendingState( remainingSlave );

					rk1.Repair();

					cluster.Await( masterAvailable( failed2, failed3 ) );
					cluster.Await( masterSeesSlavesAsAvailable( 2 ) );

					EnsureInstanceIsWritable( master );
					EnsureInstanceIsWritable( remainingSlave );
					EnsureInstanceIsWritable( failed1 );
			  }
			  finally
			  {
					manager.Shutdown();
			  }
		 }

		 private IDictionary<string, string> Config()
		 {
			  return stringMap( HaSettings.TxPushFactor.name(), "4", ClusterSettings.heartbeat_interval.name(), "250ms" );
		 }

		 private ClusterManager.RepairKit KillAbruptly( ClusterManager.ManagedCluster cluster, HighlyAvailableGraphDatabase failed1, HighlyAvailableGraphDatabase failed2, HighlyAvailableGraphDatabase failed3 )
		 {
			  ClusterManager.RepairKit firstFailure = cluster.Fail( failed1 );
			  cluster.Fail( failed2 );
			  cluster.Fail( failed3 );

			  cluster.Await( instanceEvicted( failed1 ) );
			  cluster.Await( instanceEvicted( failed2 ) );
			  cluster.Await( instanceEvicted( failed3 ) );

			  return firstFailure;
		 }

		 private ClusterManager.RepairKit KillIncrementally( ClusterManager.ManagedCluster cluster, HighlyAvailableGraphDatabase failed1, HighlyAvailableGraphDatabase failed2, HighlyAvailableGraphDatabase failed3 )
		 {
			  ClusterManager.RepairKit firstFailure = cluster.Fail( failed1 );
			  cluster.Await( instanceEvicted( failed1 ) );
			  cluster.Fail( failed2 );
			  cluster.Await( instanceEvicted( failed2 ) );
			  cluster.Fail( failed3 );
			  cluster.Await( instanceEvicted( failed3 ) );

			  return firstFailure;
		 }

		 private void AddSomeData( HighlyAvailableGraphDatabase instance )
		 {
			  using ( Transaction tx = instance.BeginTx() )
			  {
					Node testNode = instance.CreateNode();
					_testNodeId = testNode.Id;
					testNode.SetProperty( _testPropKey, _testPropValue );
					tx.Success();
			  }
		 }

		 /*
		  * This method must be called on an instance that has had addSomeData() called on it.
		  */
		 private void EnsureInstanceIsReadOnlyInPendingState( HighlyAvailableGraphDatabase instance )
		 {
			  assertEquals( PENDING, instance.InstanceState );

			  tx( instance, retryACoupleOfTimesOn( TRANSIENT_ERRORS ), db => assertEquals( _testPropValue, instance.GetNodeById( _testNodeId ).getProperty( _testPropKey ) ) );

			  try
			  {
					  using ( Transaction ignored = instance.BeginTx() )
					  {
						instance.GetNodeById( _testNodeId ).delete();
						fail( "Should not be able to do write transactions when detached" );
					  }
			  }
			  catch ( Exception expected ) when ( expected is TransientDatabaseFailureException || expected is TransactionFailureException )
			  {
					// expected
			  }
		 }

		 private void EnsureInstanceIsWritable( HighlyAvailableGraphDatabase instance )
		 {
			  tx( instance, retryACoupleOfTimesOn( TRANSIENT_ERRORS ), db => Db.createNode().setProperty(_testPropKey, _testPropValue) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void setupForWaitOnSwitchToDetached(HighlyAvailableGraphDatabase db, final java.util.concurrent.CountDownLatch latch)
		 private void SetupForWaitOnSwitchToDetached( HighlyAvailableGraphDatabase db, System.Threading.CountdownEvent latch )
		 {
			  Db.DependencyResolver.resolveDependency( typeof( HighAvailabilityMemberStateMachine ) ).addHighAvailabilityMemberListener( new HighAvailabilityMemberListener_AdapterAnonymousInnerClass( this, latch ) );
		 }

		 private class HighAvailabilityMemberListener_AdapterAnonymousInnerClass : Neo4Net.Kernel.ha.cluster.HighAvailabilityMemberListener_Adapter
		 {
			 private readonly ClusterPartitionIT _outerInstance;

			 private System.Threading.CountdownEvent _latch;

			 public HighAvailabilityMemberListener_AdapterAnonymousInnerClass( ClusterPartitionIT outerInstance, System.Threading.CountdownEvent latch )
			 {
				 this.outerInstance = outerInstance;
				 this._latch = latch;
			 }

			 public override void instanceDetached( HighAvailabilityMemberChangeEvent @event )
			 {
				  _latch.Signal();
			 }
		 }
	}

}