using System;
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
namespace Neo4Net.Kernel.ha.transaction
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using ClusterManager = Neo4Net.Kernel.impl.ha.ClusterManager;
	using ManagedCluster = Neo4Net.Kernel.impl.ha.ClusterManager.ManagedCluster;
	using NetworkFlag = Neo4Net.Kernel.impl.ha.ClusterManager.NetworkFlag;
	using RepairKit = Neo4Net.Kernel.impl.ha.ClusterManager.RepairKit;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;
	using Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.@string;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.TimeUtil.parseTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher.MASTER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher.UNKNOWN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.MyRelTypes.TEST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.memberThinksItIsRole;

	/// <summary>
	/// Non-deterministically tries to reproduce a problem where transactions may, at the time of master switches,
	/// sometimes overwrite each others data. More specifically not respect each others locks, among other things.
	/// There is no chance this test will yield a false failure, although sometimes it will be successful
	/// meaning it didn't manage to reproduce the problem. At the time of writing 2.2.6 and 2.3.0 is out.
	/// 
	/// The master switching in this test focuses on keeping the same master, but fiddle with the cluster so that
	/// the master loses quorum and goes to pending, to quickly thereafter go to master role again.
	/// Transactions happen before, during and after that (re)election. Each transaction does the following:
	/// 
	/// <ol>
	/// <li>Locks the node (all transactions use the same node)
	/// <li>Reads an int property from that node
	/// <li>Increments and sets that int property value back
	/// <li>Commits
	/// </ol>
	/// 
	/// Some transactions may not commit, that's fine. For all those that commit the int property (counter)
	/// on that node must be incremented by one. In the end, after all threads have.completed, the number of successes
	/// is compared with the counter node property. They should be the same. If not, then it means that one or more
	/// transactions made changes off of stale values and still managed to commit.
	/// 
	/// This test is a stress test and duration of execution can be controlled via system property
	/// -D<seealso cref="org.neo4j.kernel.ha.transaction.TransactionThroughMasterSwitchStressIT"/>.duration
	/// </summary>
	public class TransactionThroughMasterSwitchStressIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.ha.ClusterRule clusterRule = new org.neo4j.test.ha.ClusterRule().withInstanceSetting(org.neo4j.kernel.ha.HaSettings.slave_only, value -> value == 1 || value == 2 ? org.neo4j.kernel.configuration.Settings.TRUE : org.neo4j.kernel.configuration.Settings.FALSE);
		 public readonly ClusterRule ClusterRule = new ClusterRule().withInstanceSetting(HaSettings.slave_only, value => value == 1 || value == 2 ? Settings.TRUE : Settings.FALSE);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotHaveTransactionsRunningThroughRoleSwitchProduceInconsistencies() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotHaveTransactionsRunningThroughRoleSwitchProduceInconsistencies()
		 {
			  // Duration of this test. If the timeout is hit in the middle of a round, the round will be completed
			  // and exit after that.
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  long duration = parseTimeMillis.apply( System.getProperty( this.GetType().FullName + ".duration", "30s" ) );
			  long endTime = currentTimeMillis() + duration;
			  while ( currentTimeMillis() < endTime )
			  {
					OneRound();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void oneRound() throws Throwable
		 private void OneRound()
		 {
			  // GIVEN a cluster and a node
			  const string key = "key";
			  ClusterManager.ManagedCluster cluster = ClusterRule.startCluster();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.GraphDatabaseService master = cluster.getMaster();
			  GraphDatabaseService master = cluster.Master;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long nodeId = createNode(master);
			  long nodeId = CreateNode( master );
			  cluster.Sync();

			  // and a bunch of workers contending on that node, each changing it
			  Workers<ThreadStart> transactors = new Workers<ThreadStart>( "Transactors" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger successes = new java.util.concurrent.atomic.AtomicInteger();
			  AtomicInteger successes = new AtomicInteger();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean end = new java.util.concurrent.atomic.AtomicBoolean();
			  AtomicBoolean end = new AtomicBoolean();
			  for ( int i = 0; i < 10; i++ )
			  {
					transactors.Start(() =>
					{
					 Random random = ThreadLocalRandom.current();
					 while ( !end.get() )
					 {
						  bool committed = true;
						  try
						  {
							  using ( Transaction tx = master.BeginTx() )
							  {
									Node node = master.GetNodeById( nodeId );
   
									// Acquiring lock, read int property value, increment, set incremented int property
									// should not break under any circumstances.
									tx.acquireWriteLock( node );
									node.setProperty( key, ( int? ) node.getProperty( key, 0 ) + 1 );
									// Throw in relationship for good measure
									node.createRelationshipTo( master.CreateNode(), TEST );
   
									Thread.Sleep( random.Next( 1_000 ) );
									tx.success();
							  }
						  }
						  catch ( Exception )
						  {
								// It's OK
								committed = false;
						  }
						  if ( committed )
						  {
								successes.incrementAndGet();
						  }
					 }
					});
			  }

			  // WHEN entering a period of induced cluster instabilities
			  ReelectTheSameMasterMakingItGoToPendingAndBack( cluster );

			  // ... letting transactions run a bit after the role switch as well.
			  long targetSuccesses = successes.get() + 20;
			  while ( successes.get() < targetSuccesses )
			  {
					Thread.Sleep( 100 );
			  }
			  end.set( true );
			  transactors.AwaitAndThrowOnError();

			  // THEN verify that the count is equal to the number of successful transactions
			  assertEquals( successes.get(), GetNodePropertyValue(master, nodeId, key) );
		 }

		 private object GetNodePropertyValue( GraphDatabaseService db, long nodeId, string key )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					object value = Db.getNodeById( nodeId ).getProperty( key );
					tx.Success();
					return value;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void reelectTheSameMasterMakingItGoToPendingAndBack(org.neo4j.kernel.impl.ha.ClusterManager.ManagedCluster cluster) throws Throwable
		 private void ReelectTheSameMasterMakingItGoToPendingAndBack( ClusterManager.ManagedCluster cluster )
		 {
			  HighlyAvailableGraphDatabase master = cluster.Master;

			  // Fail master and wait for master to go to pending, since it detects it's partitioned away
			  ClusterManager.RepairKit masterRepair = cluster.Fail( master, false, ClusterManager.NetworkFlag.IN, ClusterManager.NetworkFlag.OUT );
			  cluster.Await( memberThinksItIsRole( master, UNKNOWN ) );

			  // Then Immediately repair
			  masterRepair.Repair();

			  // Wait for this instance to go to master again, since the other instances are slave only
			  cluster.Await( memberThinksItIsRole( master, MASTER ) );
			  cluster.Await( ClusterManager.masterAvailable() );
			  assertEquals( master, cluster.Master );
		 }

		 private long CreateNode( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();
					tx.Success();
					return node.Id;
			  }
		 }
	}

}