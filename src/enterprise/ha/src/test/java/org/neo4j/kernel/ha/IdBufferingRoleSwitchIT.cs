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
namespace Neo4Net.Kernel.ha
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using ClusterSettings = Neo4Net.cluster.ClusterSettings;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using ManagedCluster = Neo4Net.Kernel.impl.ha.ClusterManager.ManagedCluster;
	using IdController = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.id.IdController;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Barrier = Neo4Net.Test.Barrier;
	using Neo4Net.Test.OtherThreadExecutor;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;
	using Neo4Net.Test.rule.concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.ha.ClusterManager.masterAvailable;

	public class IdBufferingRoleSwitchIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.ha.ClusterRule clusterRule = new Neo4Net.test.ha.ClusterRule().withSharedSetting(HaSettings.pull_interval, "0").withSharedSetting(HaSettings.tx_push_factor, "0").withSharedSetting(Neo4Net.cluster.ClusterSettings.join_timeout, "60s").withConsistencyCheckAfterwards();
		 public ClusterRule ClusterRule = new ClusterRule().withSharedSetting(HaSettings.PullInterval, "0").withSharedSetting(HaSettings.TxPushFactor, "0").withSharedSetting(ClusterSettings.join_timeout, "60s").withConsistencyCheckAfterwards();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.rule.concurrent.OtherThreadRule<Void> t2 = new Neo4Net.test.rule.concurrent.OtherThreadRule<>();
		 public OtherThreadRule<Void> T2 = new OtherThreadRule<Void>();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSeeFreedIdsCrossRoleSwitch() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSeeFreedIdsCrossRoleSwitch()
		 {
			  // GIVEN
			  ManagedCluster cluster = ClusterRule.startCluster();
			  HighlyAvailableGraphDatabase firstMaster = cluster.Master;

			  // WHEN
			  // a node with a property
			  Node node = CreateNodeWithProperties( firstMaster, 1 );
			  // sync cluster
			  cluster.Sync();
			  // a transaction on master which deletes the property
			  DeleteNode( node, firstMaster );
			  TriggerIdMaintenance( firstMaster );
			  CreateNodeWithProperties( firstMaster, 1 ); // <-- this one reuses the same property id 0
			  // a transaction T on slave which will be kept open using a barrier
			  GraphDatabaseAPI slave = cluster.AnySlave;
			  Neo4Net.Test.Barrier_Control barrier = new Neo4Net.Test.Barrier_Control();
			  Future<Void> t = T2.execute( BarrierControlledReadTransaction( slave, barrier ) );
			  // pull updates on slave
			  barrier.Await();
			  slave.DependencyResolver.resolveDependency( typeof( UpdatePuller ) ).pullUpdates();
			  // a role switch
			  cluster.Shutdown( firstMaster );
			  cluster.Await( masterAvailable( firstMaster ) );
			  // close T
			  barrier.Release();
			  t.get();
			  TriggerIdMaintenance( slave );

			  // THEN the deleted property record should now not be in freelist on new master
			  CreateNodeWithProperties( slave, 10 ); // <-- this transaction should introduce inconsistencies
			  cluster.Stop(); // <-- CC will be run here since that's configured above ^^^
		 }

		 private void TriggerIdMaintenance( GraphDatabaseAPI db )
		 {
			  Db.DependencyResolver.resolveDependency( typeof( IdController ) ).maintenance();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Neo4Net.test.OtherThreadExecutor.WorkerCommand<Void,Void> barrierControlledReadTransaction(final Neo4Net.graphdb.GraphDatabaseService slave, final Neo4Net.test.Barrier_Control barrier)
		 private WorkerCommand<Void, Void> BarrierControlledReadTransaction( IGraphDatabaseService slave, Neo4Net.Test.Barrier_Control barrier )
		 {
			  return state =>
			  {
				try
				{
					using ( Transaction tx = slave.BeginTx() )
					{
						 barrier.Reached();
						 tx.success();
					}
				}
				catch ( Exception )
				{
					 // This is OK, we expect this transaction to fail after role switch
				}
				finally
				{
					 barrier.Release();
				}
				return null;
			  };
		 }

		 private void DeleteNode( Node node, IGraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					node.Delete();
					tx.Success();
			  }
		 }

		 private Node CreateNodeWithProperties( IGraphDatabaseService db, int numberOfProperties )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();
					for ( int i = 0; i < numberOfProperties; i++ )
					{
						 node.SetProperty( "key" + i, "value" + i );
					}
					tx.Success();
					return node;
			  }
		 }
	}

}