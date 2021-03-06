﻿/*
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
namespace Org.Neo4j.ha
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using InstanceId = Org.Neo4j.cluster.InstanceId;
	using ClusterClient = Org.Neo4j.cluster.client.ClusterClient;
	using HeartbeatListener = Org.Neo4j.cluster.protocol.heartbeat.HeartbeatListener;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using HaSettings = Org.Neo4j.Kernel.ha.HaSettings;
	using HighlyAvailableGraphDatabase = Org.Neo4j.Kernel.ha.HighlyAvailableGraphDatabase;
	using ClusterManager = Org.Neo4j.Kernel.impl.ha.ClusterManager;
	using ManagedCluster = Org.Neo4j.Kernel.impl.ha.ClusterManager.ManagedCluster;
	using RepairKit = Org.Neo4j.Kernel.impl.ha.ClusterManager.RepairKit;
	using ClusterRule = Org.Neo4j.Test.ha.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.ha.ClusterManager.allSeesAllAsAvailable;

	public class SlaveOnlyClusterIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.ha.ClusterRule clusterRule = new org.neo4j.test.ha.ClusterRule().withInstanceSetting(org.neo4j.kernel.ha.HaSettings.slave_only, value -> value == 1 || value == 2 ? org.neo4j.kernel.configuration.Settings.TRUE : org.neo4j.kernel.configuration.Settings.FALSE);
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