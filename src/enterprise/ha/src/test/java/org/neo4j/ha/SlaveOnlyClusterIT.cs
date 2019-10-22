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
namespace Neo4Net.ha
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using InstanceId = Neo4Net.cluster.InstanceId;
	using ClusterClient = Neo4Net.cluster.client.ClusterClient;
	using HeartbeatListener = Neo4Net.cluster.protocol.heartbeat.HeartbeatListener;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using HaSettings = Neo4Net.Kernel.ha.HaSettings;
	using HighlyAvailableGraphDatabase = Neo4Net.Kernel.ha.HighlyAvailableGraphDatabase;
	using ClusterManager = Neo4Net.Kernel.impl.ha.ClusterManager;
	using ManagedCluster = Neo4Net.Kernel.impl.ha.ClusterManager.ManagedCluster;
	using RepairKit = Neo4Net.Kernel.impl.ha.ClusterManager.RepairKit;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.ha.ClusterManager.allSeesAllAsAvailable;

	public class SlaveOnlyClusterIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.ha.ClusterRule clusterRule = new org.Neo4Net.test.ha.ClusterRule().withInstanceSetting(org.Neo4Net.kernel.ha.HaSettings.slave_only, value -> value == 1 || value == 2 ? org.Neo4Net.kernel.configuration.Settings.TRUE : org.Neo4Net.kernel.configuration.Settings.FALSE);
		 public ClusterRule ClusterRule = new ClusterRule().withInstanceSetting(HaSettings.slave_only, value => value == 1 || value == 2 ? Settings.TRUE : Settings.FALSE);

		 private const string PROPERTY = "foo";
		 private const string VALUE = "bar";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMasterElectionAfterMasterRecoversInSlaveOnlyCluster() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestMasterElectionAfterMasterRecoversInSlaveOnlyCluster()
		 {
			  ClusterManager.ManagedCluster cluster = ClusterRule.startCluster();
			  assertThat( cluster.GetServerId( cluster.Master ), equalTo( new InstanceId( 3 ) ) );
			  HighlyAvailableGraphDatabase master = cluster.Master;
			  System.Threading.CountdownEvent masterFailedLatch = CreateMasterFailLatch( cluster );
			  ClusterManager.RepairKit repairKit = cluster.Fail( master );
			  try
			  {
					assertTrue( masterFailedLatch.await( 60, TimeUnit.SECONDS ) );
			  }
			  finally
			  {
					repairKit.Repair();
			  }

			  cluster.Await( allSeesAllAsAvailable() );
			  long nodeId = CreateNodeWithPropertyOn( cluster.AnySlave, PROPERTY, VALUE );

			  using ( Transaction ignore = master.BeginTx() )
			  {
					assertThat( master.GetNodeById( nodeId ).getProperty( PROPERTY ), equalTo( VALUE ) );
			  }
		 }

		 private long CreateNodeWithPropertyOn( HighlyAvailableGraphDatabase db, string property, string value )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();
					node.SetProperty( property, value );

					tx.Success();

					return node.Id;
			  }
		 }

		 private System.Threading.CountdownEvent CreateMasterFailLatch( ClusterManager.ManagedCluster cluster )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch failedLatch = new java.util.concurrent.CountDownLatch(2);
			  System.Threading.CountdownEvent failedLatch = new System.Threading.CountdownEvent( 2 );
			  foreach ( HighlyAvailableGraphDatabase db in cluster.AllMembers )
			  {
					if ( !Db.Master )
					{
						 Db.DependencyResolver.resolveDependency( typeof( ClusterClient ) ).addHeartbeatListener( new HeartbeatListenerAnonymousInnerClass( this, failedLatch ) );
					}
			  }
			  return failedLatch;
		 }

		 private class HeartbeatListenerAnonymousInnerClass : HeartbeatListener
		 {
			 private readonly SlaveOnlyClusterIT _outerInstance;

			 private System.Threading.CountdownEvent _failedLatch;

			 public HeartbeatListenerAnonymousInnerClass( SlaveOnlyClusterIT outerInstance, System.Threading.CountdownEvent failedLatch )
			 {
				 this.outerInstance = outerInstance;
				 this._failedLatch = failedLatch;
			 }

			 public void failed( InstanceId server )
			 {
				  _failedLatch.Signal();
			 }

			 public void alive( InstanceId server )
			 {
			 }
		 }
	}

}