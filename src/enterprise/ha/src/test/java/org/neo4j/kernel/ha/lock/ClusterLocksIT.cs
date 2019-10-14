using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.Kernel.ha.@lock
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RuleChain = org.junit.rules.RuleChain;


	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Neo4Net.Graphdb;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using TransactionTerminatedException = Neo4Net.Graphdb.TransactionTerminatedException;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using EntityNotFoundException = Neo4Net.Internal.Kernel.Api.exceptions.EntityNotFoundException;
	using ClusterManager = Neo4Net.Kernel.impl.ha.ClusterManager;
	using EntityType = Neo4Net.Storageengine.Api.EntityType;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.HighAvailabilityMemberState.PENDING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.allSeesAllAsAvailable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.instanceEvicted;

	public class ClusterLocksIT
	{
		private bool InstanceFieldsInitialized = false;

		public ClusterLocksIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Rules = RuleChain.outerRule( ExpectedException ).around( ClusterRule );
		}

		 private const long TIMEOUT_MILLIS = 120_000;

		 public readonly ExpectedException ExpectedException = ExpectedException.none();
		 public readonly ClusterRule ClusterRule = new ClusterRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain rules = org.junit.rules.RuleChain.outerRule(expectedException).around(clusterRule);
		 public RuleChain Rules;

		 private ClusterManager.ManagedCluster _cluster;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _cluster = ClusterRule.withSharedSetting( HaSettings.tx_push_factor, "2" ).withInstanceSetting( GraphDatabaseSettings.lock_manager, i => "community" ).startCluster();
		 }

		 private readonly Label _testLabel = Label.label( "testLabel" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = TIMEOUT_MILLIS) public void lockCleanupOnModeSwitch() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LockCleanupOnModeSwitch()
		 {
			  HighlyAvailableGraphDatabase master = _cluster.Master;
			  CreateNodeOnMaster( _testLabel, master );

			  HighlyAvailableGraphDatabase slave = _cluster.AnySlave;
			  ClusterManager.RepairKit repairKit = TakeExclusiveLockAndKillSlave( _testLabel, slave );

			  // repair of slave and new mode switch cycle on all members
			  repairKit.Repair();
			  _cluster.await( allSeesAllAsAvailable() );

			  HighlyAvailableGraphDatabase clusterMaster = _cluster.Master;
			  // now it should be possible to take exclusive lock on the same node
			  TakeExclusiveLockOnSameNodeAfterSwitch( _testLabel, master, clusterMaster );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void oneOrTheOtherShouldDeadlock() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void OneOrTheOtherShouldDeadlock()
		 {
			  AtomicInteger deadlockCount = new AtomicInteger();
			  HighlyAvailableGraphDatabase master = _cluster.Master;
			  Node masterA = CreateNodeOnMaster( _testLabel, master );
			  Node masterB = CreateNodeOnMaster( _testLabel, master );

			  HighlyAvailableGraphDatabase slave = _cluster.AnySlave;

			  using ( Transaction transaction = slave.BeginTx() )
			  {
					Node slaveA = slave.GetNodeById( masterA.Id );
					Node slaveB = slave.GetNodeById( masterB.Id );
					System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( 1 );

					transaction.AcquireWriteLock( slaveB );

					Thread masterTx = new Thread(() =>
					{
					 try
					 {
						 using ( Transaction tx = master.BeginTx() )
						 {
							  tx.acquireWriteLock( masterA );
							  latch.Signal();
							  tx.acquireWriteLock( masterB );
						 }
					 }
					 catch ( DeadlockDetectedException )
					 {
						  deadlockCount.incrementAndGet();
					 }
					});
					masterTx.Start();
					latch.await();

					try
					{
						 transaction.AcquireWriteLock( slaveA );
					}
					catch ( DeadlockDetectedException )
					{
						 deadlockCount.incrementAndGet();
					}
					masterTx.Join();
			  }

			  assertEquals( 1, deadlockCount.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void aPendingMemberShouldBeAbleToServeReads() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void APendingMemberShouldBeAbleToServeReads()
		 {
			  // given
			  CreateNodeOnMaster( _testLabel, _cluster.Master );
			  _cluster.sync();

			  HighlyAvailableGraphDatabase slave = _cluster.AnySlave;
			  _cluster.fail( slave, Enum.GetValues( typeof( ClusterManager.NetworkFlag ) ) );
			  _cluster.await( instanceEvicted( slave ) );

			  assertEquals( PENDING, slave.InstanceState );

			  // when
			  for ( int i = 0; i < 10; i++ )
			  {
					try
					{
							using ( Transaction tx = slave.BeginTx() )
							{
							 Node single = Iterables.single( slave.AllNodes );
							 Label label = Iterables.single( single.Labels );
							 assertEquals( _testLabel, label );
							 tx.Success();
							 break;
							}
					}
					catch ( TransactionTerminatedException )
					{
						 // Race between going to pending and reading, try again in a little while
						 Thread.Sleep( 1_000 );
					}
			  }

			  // then no exceptions thrown
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void takeExclusiveLockOnSameNodeAfterSwitch(org.neo4j.graphdb.Label testLabel, org.neo4j.kernel.ha.HighlyAvailableGraphDatabase master, org.neo4j.kernel.ha.HighlyAvailableGraphDatabase db) throws org.neo4j.internal.kernel.api.exceptions.EntityNotFoundException
		 private void TakeExclusiveLockOnSameNodeAfterSwitch( Label testLabel, HighlyAvailableGraphDatabase master, HighlyAvailableGraphDatabase db )
		 {
			  using ( Transaction transaction = Db.beginTx() )
			  {
					Node node = GetNode( master, testLabel );
					transaction.AcquireWriteLock( node );
					node.SetProperty( "key", "value" );
					transaction.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.impl.ha.ClusterManager.RepairKit takeExclusiveLockAndKillSlave(org.neo4j.graphdb.Label testLabel, org.neo4j.kernel.ha.HighlyAvailableGraphDatabase db) throws org.neo4j.internal.kernel.api.exceptions.EntityNotFoundException
		 private ClusterManager.RepairKit TakeExclusiveLockAndKillSlave( Label testLabel, HighlyAvailableGraphDatabase db )
		 {
			  TakeExclusiveLock( testLabel, db );
			  return _cluster.shutdown( db );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.graphdb.Transaction takeExclusiveLock(org.neo4j.graphdb.Label testLabel, org.neo4j.kernel.ha.HighlyAvailableGraphDatabase db) throws org.neo4j.internal.kernel.api.exceptions.EntityNotFoundException
		 private Transaction TakeExclusiveLock( Label testLabel, HighlyAvailableGraphDatabase db )
		 {
			  Transaction transaction = Db.beginTx();
			  Node node = GetNode( db, testLabel );
			  transaction.AcquireWriteLock( node );
			  return transaction;
		 }

		 private Node CreateNodeOnMaster( Label testLabel, HighlyAvailableGraphDatabase master )
		 {
			  Node node;
			  using ( Transaction transaction = master.BeginTx() )
			  {
					node = master.CreateNode( testLabel );
					transaction.Success();
			  }
			  return node;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.graphdb.Node getNode(org.neo4j.kernel.ha.HighlyAvailableGraphDatabase db, org.neo4j.graphdb.Label testLabel) throws org.neo4j.internal.kernel.api.exceptions.EntityNotFoundException
		 private Node GetNode( HighlyAvailableGraphDatabase db, Label testLabel )
		 {
			  using ( ResourceIterator<Node> nodes = Db.findNodes( testLabel ) )
			  {
					return nodes.First().orElseThrow(() => new EntityNotFoundException(EntityType.NODE, 0L));
			  }
		 }
	}

}