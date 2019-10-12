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
namespace Org.Neo4j.causalclustering.scenarios
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;

	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using Org.Neo4j.causalclustering.discovery;
	using Node = Org.Neo4j.Graphdb.Node;
	using ClusterRule = Org.Neo4j.Test.causalclustering.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;

	public class WillNotBecomeLeaderIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.causalclustering.ClusterRule clusterRule = new org.neo4j.test.causalclustering.ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0).withDiscoveryServiceType(EnterpriseDiscoveryServiceType.HAZELCAST).withSharedCoreParam(org.neo4j.causalclustering.core.CausalClusteringSettings.multi_dc_license, "true");
		 public readonly ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0).withDiscoveryServiceType(EnterpriseDiscoveryServiceType.Hazelcast).withSharedCoreParam(CausalClusteringSettings.multi_dc_license, "true");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exceptionMatcher = org.junit.rules.ExpectedException.none();
		 public ExpectedException ExceptionMatcher = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void clusterShouldNotElectNewLeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ClusterShouldNotElectNewLeader()
		 {
			  // given
			  int leaderId = 0;

			  ClusterRule.withInstanceCoreParam(CausalClusteringSettings.refuse_to_be_leader, x =>
			  {
				if ( x == leaderId )
				{
					 return "false";
				}
				else
				{
					 return "true";
				}
			  });

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.createCluster();
			  Cluster<object> cluster = ClusterRule.createCluster();
			  cluster.Start();
			  assertEquals( leaderId, cluster.AwaitLeader().serverId() );

			  cluster.CoreTx((db, tx) =>
			  {
				Node node = Db.createNode( label( "boo" ) );
				node.setProperty( "foobar", "baz_bat" );
				tx.success();
			  });

			  // When
			  cluster.RemoveCoreMemberWithServerId( leaderId );

			  // Then
			  try
			  {
					cluster.AwaitLeader( 10, SECONDS );

					fail( "Should not have elected a leader" );
			  }
			  catch ( TimeoutException )
			  {
					// Successful
			  }
		 }
	}

}