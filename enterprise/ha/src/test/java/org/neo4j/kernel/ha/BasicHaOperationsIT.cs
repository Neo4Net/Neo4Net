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
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using ClusterManager = Org.Neo4j.Kernel.impl.ha.ClusterManager;
	using ManagedCluster = Org.Neo4j.Kernel.impl.ha.ClusterManager.ManagedCluster;
	using RepairKit = Org.Neo4j.Kernel.impl.ha.ClusterManager.RepairKit;
	using ClusterRule = Org.Neo4j.Test.ha.ClusterRule;
	using LoggerRule = Org.Neo4j.Test.rule.LoggerRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class BasicHaOperationsIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static org.neo4j.test.rule.LoggerRule logger = new org.neo4j.test.rule.LoggerRule(java.util.logging.Level.OFF);
		 public static LoggerRule Logger = new LoggerRule( Level.OFF );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.ha.ClusterRule clusterRule = new org.neo4j.test.ha.ClusterRule().withSharedSetting(HaSettings.tx_push_factor, "2");
		 public ClusterRule ClusterRule = new ClusterRule().withSharedSetting(HaSettings.TxPushFactor, "2");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBasicFailover() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestBasicFailover()
		 {
			  // given
			  ClusterManager.ManagedCluster cluster = ClusterRule.startCluster();
			  HighlyAvailableGraphDatabase master = cluster.Master;
			  HighlyAvailableGraphDatabase slave1 = cluster.AnySlave;
			  HighlyAvailableGraphDatabase slave2 = cluster.GetAnySlave( slave1 );

			  // When
			  long start = System.nanoTime();
			  ClusterManager.RepairKit repair = cluster.Shutdown( master );
			  try
			  {
					Logger.Logger.warning( "Shut down master" );
					cluster.Await( ClusterManager.masterAvailable() );
					long end = System.nanoTime();
					Logger.Logger.warning( "Failover took:" + ( end - start ) / 1000000 + "ms" );
					// Then
					bool slave1Master = slave1.Master;
					bool slave2Master = slave2.Master;
					if ( slave1Master )
					{
						 assertFalse( slave2Master );
					}
					else
					{
						 assertTrue( slave2Master );
					}
			  }
			  finally
			  {
					repair.Repair();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBasicPropagationFromSlaveToMaster()
		 public virtual void TestBasicPropagationFromSlaveToMaster()
		 {
			  // given
			  ClusterManager.ManagedCluster cluster = ClusterRule.startCluster();
			  HighlyAvailableGraphDatabase master = cluster.Master;
			  HighlyAvailableGraphDatabase slave = cluster.AnySlave;
			  long nodeId;

			  // a node with a property
			  using ( Transaction tx = master.BeginTx() )
			  {
					Node node = master.CreateNode();
					nodeId = node.Id;
					node.SetProperty( "foo", "bar" );
					tx.Success();
			  }

			  cluster.Sync();

			  // when
			  // the slave does a change
			  using ( Transaction tx = slave.BeginTx() )
			  {
					slave.GetNodeById( nodeId ).setProperty( "foo", "bar2" );
					tx.Success();
			  }

			  // then
			  // the master must pick up the change
			  using ( Transaction tx = master.BeginTx() )
			  {
					assertEquals( "bar2", master.GetNodeById( nodeId ).getProperty( "foo" ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBasicPropagationFromMasterToSlave()
		 public virtual void TestBasicPropagationFromMasterToSlave()
		 {
			  // given
			  ClusterManager.ManagedCluster cluster = ClusterRule.startCluster();
			  long nodeId = 4;
			  HighlyAvailableGraphDatabase master = cluster.Master;
			  using ( Transaction tx = master.BeginTx() )
			  {
					Node node = master.CreateNode();
					node.SetProperty( "Hello", "World" );
					nodeId = node.Id;

					tx.Success();
			  }

			  cluster.Sync();

			  // No need to wait, the push factor is 2
			  HighlyAvailableGraphDatabase slave1 = cluster.AnySlave;
			  CheckNodeOnSlave( nodeId, slave1 );

			  HighlyAvailableGraphDatabase slave2 = cluster.GetAnySlave( slave1 );
			  CheckNodeOnSlave( nodeId, slave2 );
		 }

		 private void CheckNodeOnSlave( long nodeId, HighlyAvailableGraphDatabase slave2 )
		 {
			  using ( Transaction tx = slave2.BeginTx() )
			  {
					string value = slave2.GetNodeById( nodeId ).getProperty( "Hello" ).ToString();
					Logger.Logger.info( "Hello=" + value );
					assertEquals( "World", value );
					tx.Success();
			  }
		 }
	}

}