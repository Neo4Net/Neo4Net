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
namespace Org.Neo4j.causalclustering.scenarios
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using Role = Org.Neo4j.causalclustering.core.consensus.roles.Role;
	using Org.Neo4j.causalclustering.discovery;
	using CoreClusterMember = Org.Neo4j.causalclustering.discovery.CoreClusterMember;
	using ClusterRule = Org.Neo4j.Test.causalclustering.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class PreElectionIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.causalclustering.ClusterRule clusterRule = new org.neo4j.test.causalclustering.ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0).withSharedCoreParam(org.neo4j.causalclustering.core.CausalClusteringSettings.leader_election_timeout, "2s").withSharedCoreParam(org.neo4j.causalclustering.core.CausalClusteringSettings.enable_pre_voting, "true");
		 public ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0).withSharedCoreParam(CausalClusteringSettings.leader_election_timeout, "2s").withSharedCoreParam(CausalClusteringSettings.enable_pre_voting, "true");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldActuallyStartAClusterWithPreVoting() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldActuallyStartAClusterWithPreVoting()
		 {
			  ClusterRule.startCluster();
			  // pass
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldActuallyStartAClusterWithPreVotingAndARefuseToBeLeader() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldActuallyStartAClusterWithPreVotingAndARefuseToBeLeader()
		 {
			  ClusterRule.withInstanceCoreParam( CausalClusteringSettings.refuse_to_be_leader, this.firstServerRefusesToBeLeader ).withSharedCoreParam( CausalClusteringSettings.multi_dc_license, "true" );
			  ClusterRule.startCluster();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotStartAnElectionIfAMinorityOfServersHaveTimedOutOnHeartbeats() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotStartAnElectionIfAMinorityOfServersHaveTimedOutOnHeartbeats()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();
			  CoreClusterMember follower = cluster.AwaitCoreMemberWithRole( Role.FOLLOWER, 1, TimeUnit.MINUTES );

			  // when
			  follower.Raft().triggerElection(Clock.systemUTC());

			  // then
			  try
			  {
					cluster.AwaitCoreMemberWithRole( Role.CANDIDATE, 1, TimeUnit.MINUTES );
					fail( "Should not have started an election if less than a quorum have timed out" );
			  }
			  catch ( TimeoutException )
			  {
					// pass
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStartElectionIfLeaderRemoved() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStartElectionIfLeaderRemoved()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();
			  CoreClusterMember oldLeader = cluster.AwaitLeader();

			  // when
			  cluster.RemoveCoreMember( oldLeader );

			  // then
			  CoreClusterMember newLeader = cluster.AwaitLeader();

			  assertThat( newLeader.ServerId(), not(equalTo(oldLeader.ServerId())) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldElectANewLeaderIfAServerRefusesToBeLeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldElectANewLeaderIfAServerRefusesToBeLeader()
		 {
			  // given
			  ClusterRule.withInstanceCoreParam( CausalClusteringSettings.refuse_to_be_leader, this.firstServerRefusesToBeLeader ).withSharedCoreParam( CausalClusteringSettings.multi_dc_license, "true" );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.discovery.Cluster<?> cluster = clusterRule.startCluster();
			  Cluster<object> cluster = ClusterRule.startCluster();
			  CoreClusterMember oldLeader = cluster.AwaitLeader();

			  // when
			  cluster.RemoveCoreMember( oldLeader );

			  // then
			  CoreClusterMember newLeader = cluster.AwaitLeader();

			  assertThat( newLeader.ServerId(), not(equalTo(oldLeader.ServerId())) );
		 }

		 private string FirstServerRefusesToBeLeader( int id )
		 {
			  return id == 0 ? "true" : "false";
		 }
	}

}