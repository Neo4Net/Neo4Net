using System.Collections.Generic;

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
namespace Neo4Net.cluster.protocol.cluster
{
	using Test = org.junit.Test;



	public class InstanceIdTest : ClusterMockTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nodeTriesToJoinAnotherNodeWithSameServerId() throws java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NodeTriesToJoinAnotherNodeWithSameServerId()
		 {
			  TestCluster( new int[] { 1, 1 }, new VerifyInstanceConfiguration[] { new VerifyInstanceConfiguration( Collections.emptyList(), Collections.emptyMap(), Collections.emptySet() ), new VerifyInstanceConfiguration(Collections.emptyList(), Collections.emptyMap(), Collections.emptySet()) }, DefaultNetwork(), (new ClusterTestScriptDSL(this)).Rounds(600).join(100, 1, 1, 2).join(100, 2, 1, 2).message(500, "*** All nodes tried to start, should be in failed mode") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nodeTriesToJoinRunningClusterWithExistingServerId() throws java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NodeTriesToJoinRunningClusterWithExistingServerId()
		 {
			  IList<URI> correctMembers = new List<URI>();
			  correctMembers.Add( URI.create( "server1" ) );
			  correctMembers.Add( URI.create( "server2" ) );
			  correctMembers.Add( URI.create( "server3" ) );

			  IDictionary<string, InstanceId> roles = new Dictionary<string, InstanceId>();
			  roles["coordinator"] = new InstanceId( 1 );

			  TestCluster( new int[] { 1, 2, 3, 3 }, new VerifyInstanceConfiguration[]{ new VerifyInstanceConfiguration( correctMembers, roles, Collections.emptySet() ), new VerifyInstanceConfiguration(correctMembers, roles, Collections.emptySet()), new VerifyInstanceConfiguration(correctMembers, roles, Collections.emptySet()), new VerifyInstanceConfiguration(Collections.emptyList(), Collections.emptyMap(), Collections.emptySet()) }, DefaultNetwork(), (new ClusterTestScriptDSL(this)).Rounds(600).join(100, 1, 1).join(100, 2, 1).join(100, 3, 1).join(5000, 4, 1).message(0, "*** Conflicting node tried to join") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void substituteFailedNode() throws java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SubstituteFailedNode()
		 {
			  IList<URI> correctMembers = new List<URI>();
			  correctMembers.Add( URI.create( "server1" ) );
			  correctMembers.Add( URI.create( "server2" ) );
			  correctMembers.Add( URI.create( "server4" ) );

			  IList<URI> wrongMembers = new List<URI>();
			  wrongMembers.Add( URI.create( "server1" ) );
			  wrongMembers.Add( URI.create( "server2" ) );
			  wrongMembers.Add( URI.create( "server3" ) );

			  IDictionary<string, InstanceId> roles = new Dictionary<string, InstanceId>();
			  roles["coordinator"] = new InstanceId( 1 );

			  ISet<InstanceId> clusterMemberFailed = new HashSet<InstanceId>();
			  ISet<InstanceId> isolatedMemberFailed = new HashSet<InstanceId>();
			  isolatedMemberFailed.Add( new InstanceId( 1 ) ); // will never receive heartbeats again from 1,2 so they are failed
			  isolatedMemberFailed.Add( new InstanceId( 2 ) );

			  TestCluster( new int[]{ 1, 2, 3, 3 }, new VerifyInstanceConfiguration[]{ new VerifyInstanceConfiguration( correctMembers, roles, clusterMemberFailed ), new VerifyInstanceConfiguration( correctMembers, roles, clusterMemberFailed ), new VerifyInstanceConfiguration( wrongMembers, roles, isolatedMemberFailed ), new VerifyInstanceConfiguration( correctMembers, roles, clusterMemberFailed ) }, DefaultNetwork(), (new ClusterTestScriptDSL(this)).Rounds(8000).join(100, 1, 1).join(100, 2, 1).join(100, 3, 1).down(3000, 3).join(1000, 4, 1, 2, 3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void substituteFailedNodeAndFailedComesOnlineAgain() throws java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SubstituteFailedNodeAndFailedComesOnlineAgain()
		 {
			  IList<URI> correctMembers = new List<URI>();
			  correctMembers.Add( URI.create( "server1" ) );
			  correctMembers.Add( URI.create( "server2" ) );
			  correctMembers.Add( URI.create( "server4" ) );

			  IList<URI> badMembers = new List<URI>();
			  badMembers.Add( URI.create( "server1" ) );
			  badMembers.Add( URI.create( "server2" ) );
			  badMembers.Add( URI.create( "server3" ) );

			  IDictionary<string, InstanceId> roles = new Dictionary<string, InstanceId>();
			  roles["coordinator"] = new InstanceId( 1 );

			  ISet<InstanceId> clusterMemberFailed = new HashSet<InstanceId>(); // no failures
			  ISet<InstanceId> isolatedMemberFailed = new HashSet<InstanceId>();
			  isolatedMemberFailed.Add( new InstanceId( 1 ) ); // will never receive heartbeats again from 1,2 so they are failed
			  isolatedMemberFailed.Add( new InstanceId( 2 ) );

			  TestCluster( new int[]{ 1, 2, 3, 3 }, new VerifyInstanceConfiguration[]{ new VerifyInstanceConfiguration( correctMembers, roles, clusterMemberFailed ), new VerifyInstanceConfiguration( correctMembers, roles, clusterMemberFailed ), new VerifyInstanceConfiguration( badMembers, roles, isolatedMemberFailed ), new VerifyInstanceConfiguration( correctMembers, roles, clusterMemberFailed ) }, DefaultNetwork(), (new ClusterTestScriptDSL(this)).Rounds(800).join(100, 1, 1).join(100, 2, 1).join(100, 3, 1).down(3000, 3).join(1000, 4, 1, 2, 3).up(1000, 3) );
		 }
	}

}