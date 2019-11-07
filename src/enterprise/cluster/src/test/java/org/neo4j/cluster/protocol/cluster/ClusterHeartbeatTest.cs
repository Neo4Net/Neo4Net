using System.Collections.Generic;

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
namespace Neo4Net.cluster.protocol.cluster
{
	using Test = org.junit.Test;


	public class ClusterHeartbeatTest : ClusterMockTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void threeNodesJoinAndNoFailures() throws java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ThreeNodesJoinAndNoFailures()
		 {
			  TestCluster( 3, DefaultNetwork(), (new ClusterTestScriptDSL(this)).Rounds(200).join(100, 1).join(100, 2).join(100, 3).verifyConfigurations("after setup", 3000).leave(0, 1).leave(200, 2).leave(200, 3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void threeNodesJoinAndThenSlaveDies() throws java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ThreeNodesJoinAndThenSlaveDies()
		 {
			  TestCluster( 3, DefaultNetwork(), (new ClusterTestScriptDSL(this)).Rounds(1000).join(100, 1).join(100, 2).join(100, 3).verifyConfigurations("after setup", 3000).message(100, "*** All nodes up and ok").down(100, 3).message(1000, "*** Should have seen failure by now").up(0, 3).message(2000, "*** Should have recovered by now").verifyConfigurations("after recovery", 0).leave(200, 1).leave(200, 2).leave(200, 3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void threeNodesJoinAndThenCoordinatorDies() throws java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ThreeNodesJoinAndThenCoordinatorDies()
		 {
			  TestCluster( 3, DefaultNetwork(), (new ClusterTestScriptDSL(this)).Rounds(1000).join(100, 1, 1).join(100, 2, 1).join(100, 3, 1).message(3000, "*** All nodes up and ok").down(500, 1).message(1000, "*** Should have seen failure by now").up(0, 1).message(2000, "*** Should have recovered by now").verifyConfigurations("after recovery", 0).down(0, 2).message(1400, "*** Should have seen failure by now").up(0, 2).message(800, "*** All nodes leave").verifyConfigurations("before leave", 0).leave(0, 1).leave(300, 2).leave(300, 3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void threeNodesJoinAndThenCoordinatorDiesForReal() throws java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ThreeNodesJoinAndThenCoordinatorDiesForReal()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<String, Neo4Net.cluster.InstanceId> roles = new java.util.HashMap<>();
			  IDictionary<string, InstanceId> roles = new Dictionary<string, InstanceId>();

			  TestCluster( 3, DefaultNetwork(), (new ClusterTestScriptDSL(this)).Rounds(1000).join(100, 1, 1).join(100, 2, 1).join(100, 3, 1).message(3000, "*** All nodes up and ok").getRoles(roles).down(800, 1).message(2000, "*** Should have seen failure by now").verifyCoordinatorRoleSwitched(roles).leave(0, 1).leave(300, 2).leave(300, 3) );
		 }
	}

}