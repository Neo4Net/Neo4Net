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
namespace Neo4Net.cluster.protocol.cluster
{
	using Ignore = org.junit.Ignore;
	using Test = org.junit.Test;

	/// <summary>
	/// TODO
	/// </summary>
	public class ClusterMembershipTest : ClusterMockTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void threeNodesJoinAndThenLeave() throws java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ThreeNodesJoinAndThenLeave()
		 {
			  TestCluster( 3, DefaultNetwork(), (new ClusterTestScriptDSL(this)).Rounds(70).join(100, 1).join(100, 2).join(100, 3).message(100, "*** Cluster formed, now leave").leave(0, 3).leave(100, 2).leave(100, 1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void threeNodesJoinAndThenLeaveInOriginalOrder() throws java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ThreeNodesJoinAndThenLeaveInOriginalOrder()
		 {
			  TestCluster( 3, DefaultNetwork(), (new ClusterTestScriptDSL(this)).Rounds(100).join(100, 1).join(100, 2).join(100, 3).message(100, "*** Cluster formed, now leave").verifyConfigurations("starting leave", 0).sleep(100).leave(0, 1).verifyConfigurations("after 1 left", 200).leave(0, 2).leave(200, 3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void noobTest() throws java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NoobTest()
		 {
			  TestCluster( 1, DefaultNetwork(), (new ClusterTestScriptDSL(this)).Rounds(3).sleep(10).join(0, 1).message(100, "*** Cluster formed, now leave").leave(0, 1).verifyConfigurations("after 1 left", 0) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sevenNodesJoinAndThenLeave() throws java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SevenNodesJoinAndThenLeave()
		 {
			  TestCluster( 7, DefaultNetwork(), (new ClusterTestScriptDSL(this)).Rounds(500).join(100, 1).join(100, 2).join(100, 3).join(100, 4).join(100, 5).join(100, 6).join(100, 7).leave(100, 7).leave(500, 6).leave(500, 5).leave(500, 4).leave(500, 3).leave(500, 2).leave(500, 1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void oneNodeJoinThenTwoJoinRoughlyAtSameTime() throws java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void OneNodeJoinThenTwoJoinRoughlyAtSameTime()
		 {
			  TestCluster( 3, DefaultNetwork(), (new ClusterTestScriptDSL(this)).Rounds(500).join(100, 1).join(100, 2).join(10, 3).message(2000, "*** All are in ").leave(0, 3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void oneNodeJoinThenThreeJoinRoughlyAtSameTime2() throws java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void OneNodeJoinThenThreeJoinRoughlyAtSameTime2()
		 {
			  TestCluster( 4, DefaultNetwork(), (new ClusterTestScriptDSL(this)).Rounds(800).join(100, 1).join(100, 2).join(10, 3).join(10, 4).message(2000, "*** All are in ").broadcast(10, 2, "Hello world") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void twoNodesJoinThenOneLeavesAsThirdJoins() throws java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TwoNodesJoinThenOneLeavesAsThirdJoins()
		 {
			  TestCluster( 3, DefaultNetwork(), (new ClusterTestScriptDSL(this)).Rounds(820).join(0, 1).join(10, 2).message(80, "*** 1 and 2 are in cluster").leave(10, 2).join(20, 3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Ignore("instance 1 is in start, 2 in discovery. Correct but we don't have a way to verify it yet") public void oneNodeCreatesClusterAndThenAnotherJoinsAsFirstLeaves() throws java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void OneNodeCreatesClusterAndThenAnotherJoinsAsFirstLeaves()
		 {
			  TestCluster( 2, DefaultNetwork(), (new ClusterTestScriptDSL(this)).Rounds(1000).join(0, 1).join(10, 2, 1, 2).leave(20, 1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void threeNodesJoinAndThenFirstLeavesAsFourthJoins() throws java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ThreeNodesJoinAndThenFirstLeavesAsFourthJoins()
		 {
			  TestCluster( 4, DefaultNetwork(), (new ClusterTestScriptDSL(this)).Rounds(200).join(100, 1).join(100, 2).join(100, 3).message(100, "*** Cluster formed, now leave").leave(0, 1).join(10, 4) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void threeNodesJoinAndThenFirstLeavesAsFourthJoins2() throws java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ThreeNodesJoinAndThenFirstLeavesAsFourthJoins2()
		 {
			  TestCluster( 5, DefaultNetwork(), (new ClusterTestScriptDSL(this)).Rounds(200).join(100, 1).join(100, 2).join(100, 3).join(100, 4).message(100, "*** Cluster formed, now leave").leave(0, 1).join(30, 5).leave(0, 2) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore("Ignore until fix available") @Test public void threeNodesJoinAtSameTime() throws java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ThreeNodesJoinAtSameTime()
		 {
			  TestCluster( 3, DefaultNetwork(), (new ClusterTestScriptDSL(this)).Rounds(400).join(0, 1, 1, 2, 3).join(0, 2, 1, 2, 3).join(0, 3, 1, 2, 3).message(390, "*** Cluster formed") );
		 }
	}

}