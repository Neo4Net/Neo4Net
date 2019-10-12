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
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using Role = Org.Neo4j.causalclustering.core.consensus.roles.Role;
	using Org.Neo4j.causalclustering.discovery;
	using ClusterRule = Org.Neo4j.Test.causalclustering.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class ConsensusGroupSettingsIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.causalclustering.ClusterRule clusterRule = new org.neo4j.test.causalclustering.ClusterRule().withNumberOfCoreMembers(5).withNumberOfReadReplicas(0).withInstanceCoreParam(org.neo4j.causalclustering.core.CausalClusteringSettings.minimum_core_cluster_size_at_formation, value -> "5").withInstanceCoreParam(org.neo4j.causalclustering.core.CausalClusteringSettings.minimum_core_cluster_size_at_runtime,value -> "3").withInstanceCoreParam(org.neo4j.causalclustering.core.CausalClusteringSettings.leader_election_timeout, value -> "1s").withTimeout(1000, SECONDS);
		 public readonly ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(5).withNumberOfReadReplicas(0).withInstanceCoreParam(CausalClusteringSettings.minimum_core_cluster_size_at_formation, value => "5").withInstanceCoreParam(CausalClusteringSettings.minimum_core_cluster_size_at_runtime,value => "3").withInstanceCoreParam(CausalClusteringSettings.leader_election_timeout, value => "1s").withTimeout(1000, SECONDS);

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.causalclustering.discovery.Cluster<?> cluster;
		 private Cluster<object> _cluster;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _cluster = ClusterRule.startCluster();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowTheConsensusGroupToDropBelowMinimumConsensusGroupSize() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowTheConsensusGroupToDropBelowMinimumConsensusGroupSize()
		 {
			  // given
			  int numberOfCoreSeversToRemove = 3;

			  _cluster.awaitLeader();

			  // when
			  for ( int i = 0; i < numberOfCoreSeversToRemove; i++ )
			  {
					_cluster.removeCoreMember( _cluster.getMemberWithRole( Role.LEADER ) );
					_cluster.awaitLeader( 30, SECONDS );
			  }

			  // then

			  assertEquals( 3, _cluster.coreMembers().GetEnumerator().next().raft().replicationMembers().size() );
		 }
	}

}