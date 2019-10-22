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
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using HighAvailabilityMemberState = Neo4Net.Kernel.ha.cluster.HighAvailabilityMemberState;
	using ClusterManager = Neo4Net.Kernel.impl.ha.ClusterManager;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.ha.ClusterManager.allSeesAllAsAvailable;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.ha.ClusterManager.clusterOfSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.ha.ClusterManager.memberSeesOtherMemberAsFailed;

	public class TwoInstanceClusterIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.ha.ClusterRule clusterRule = new org.Neo4Net.test.ha.ClusterRule();
		 public readonly ClusterRule ClusterRule = new ClusterRule();

		 private ClusterManager.ManagedCluster _cluster;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _cluster = ClusterRule.withSharedSetting( HaSettings.ReadTimeout, "1s" ).withSharedSetting( HaSettings.StateSwitchTimeout, "2s" ).withSharedSetting( HaSettings.ComChunkSize, "1024" ).withCluster( clusterOfSize( 2 ) ).startCluster();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void masterShouldRemainAvailableIfTheSlaveDiesAndRecovers() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MasterShouldRemainAvailableIfTheSlaveDiesAndRecovers()
		 {
			  HighlyAvailableGraphDatabase master = _cluster.Master;
			  HighlyAvailableGraphDatabase theSlave = _cluster.AnySlave;

			  string propertyName = "prop";
			  string propertyValue1 = "value1";
			  string propertyValue2 = "value2";
			  long masterNodeId;
			  long slaveNodeId;

			  ClusterManager.RepairKit repairKit = _cluster.fail( theSlave );
			  _cluster.await( memberSeesOtherMemberAsFailed( master, theSlave ) );

			  using ( Transaction tx = master.BeginTx() )
			  {
					Node node = master.CreateNode();
					node.SetProperty( propertyName, propertyValue1 );
					masterNodeId = node.Id;
					tx.Success();
			  }

			  repairKit.Repair();

			  _cluster.await( allSeesAllAsAvailable() );

			  using ( Transaction tx = theSlave.BeginTx() )
			  {
					Node node = theSlave.CreateNode();
					node.SetProperty( propertyName, propertyValue2 );
					assertEquals( propertyValue1, theSlave.GetNodeById( masterNodeId ).getProperty( propertyName ) );
					slaveNodeId = node.Id;
					tx.Success();
			  }

			  using ( Transaction tx = master.BeginTx() )
			  {
					assertEquals( propertyValue2, master.GetNodeById( slaveNodeId ).getProperty( propertyName ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void slaveShouldMoveToPendingAndThenRecoverIfMasterDiesAndThenRecovers() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SlaveShouldMoveToPendingAndThenRecoverIfMasterDiesAndThenRecovers()
		 {
			  HighlyAvailableGraphDatabase master = _cluster.Master;
			  HighlyAvailableGraphDatabase theSlave = _cluster.AnySlave;

			  string propertyName = "prop";
			  string propertyValue = "value1";
			  long slaveNodeId;

			  ClusterManager.RepairKit repairKit = _cluster.fail( master );
			  _cluster.await( memberSeesOtherMemberAsFailed( theSlave, master ) );

			  assertEquals( HighAvailabilityMemberState.PENDING, theSlave.InstanceState );

			  repairKit.Repair();

			  _cluster.await( allSeesAllAsAvailable() );

			  using ( Transaction tx = theSlave.BeginTx() )
			  {
					Node node = theSlave.CreateNode();
					slaveNodeId = node.Id;
					node.SetProperty( propertyName, propertyValue );
					tx.Success();
			  }

			  using ( Transaction tx = master.BeginTx() )
			  {
					assertEquals( propertyValue, master.GetNodeById( slaveNodeId ).getProperty( propertyName ) );
					tx.Success();
			  }
		 }
	}

}