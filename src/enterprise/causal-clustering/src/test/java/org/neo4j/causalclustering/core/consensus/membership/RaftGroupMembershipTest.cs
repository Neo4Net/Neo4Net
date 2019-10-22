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
namespace Neo4Net.causalclustering.core.consensus.membership
{
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Mock = org.mockito.Mock;
	using MockitoJUnitRunner = org.mockito.junit.MockitoJUnitRunner;

	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Neo4Net.causalclustering.messaging;
	using Message = Neo4Net.causalclustering.messaging.Message;
	using Neo4Net.causalclustering.messaging;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.consensus.RaftMachine.Timeouts.ELECTION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.consensus.RaftMachine.Timeouts.HEARTBEAT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.consensus.roles.Role.FOLLOWER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.consensus.roles.Role.LEADER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.identity.RaftTestMember.member;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(MockitoJUnitRunner.class) public class RaftGroupMembershipTest
	public class RaftGroupMembershipTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock private org.Neo4Net.causalclustering.messaging.Outbound<org.Neo4Net.causalclustering.identity.MemberId, org.Neo4Net.causalclustering.messaging.Message> outbound;
		 private Outbound<MemberId, Message> _outbound;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock private org.Neo4Net.causalclustering.messaging.Inbound inbound;
		 private Inbound _inbound;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFormGroupWithoutAnyBootstrapping()
		 public virtual void ShouldNotFormGroupWithoutAnyBootstrapping()
		 {
			  // given
			  DirectNetworking net = new DirectNetworking();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId[] ids = {member(0), member(1), member(2)};
			  MemberId[] ids = new MemberId[] { member( 0 ), member( 1 ), member( 2 ) };

			  RaftTestFixture fixture = new RaftTestFixture( net, 3, ids );

			  fixture.Members().TargetMembershipSet = (new RaftTestGroup(ids)).Members;
			  fixture.Members().invokeTimeout(ELECTION);

			  // when
			  net.ProcessMessages();

			  // then
			  assertThat( fixture.Members(), HasCurrentMembers(new RaftTestGroup(new int[0])) );
			  assertEquals( fixture.MessageLog(), 0, fixture.Members().withRole(LEADER).size() );
			  assertEquals( fixture.MessageLog(), 3, fixture.Members().withRole(FOLLOWER).size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddSingleInstanceToExistingRaftGroup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAddSingleInstanceToExistingRaftGroup()
		 {
			  // given
			  DirectNetworking net = new DirectNetworking();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId leader = member(0);
			  MemberId leader = member( 0 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId stable1 = member(1);
			  MemberId stable1 = member( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId stable2 = member(2);
			  MemberId stable2 = member( 2 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId toBeAdded = member(3);
			  MemberId toBeAdded = member( 3 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId[] initialMembers = {leader, stable1, stable2};
			  MemberId[] initialMembers = new MemberId[] { leader, stable1, stable2 };
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId[] finalMembers = {leader, stable1, stable2, toBeAdded};
			  MemberId[] finalMembers = new MemberId[] { leader, stable1, stable2, toBeAdded };

			  RaftTestFixture fixture = new RaftTestFixture( net, 3, finalMembers );
			  fixture.Bootstrap( initialMembers );

			  fixture.Members().withId(leader).timerService().invoke(ELECTION);
			  net.ProcessMessages();

			  // when
			  fixture.Members().withId(leader).raftInstance().TargetMembershipSet = (new RaftTestGroup(finalMembers)).Members;
			  net.ProcessMessages();

			  fixture.Members().withId(leader).timerService().invoke(HEARTBEAT);
			  net.ProcessMessages();

			  // then
			  assertThat( fixture.Members().withIds(finalMembers), HasCurrentMembers(new RaftTestGroup(finalMembers)) );
			  assertEquals( fixture.MessageLog(),1, fixture.Members().withRole(LEADER).size() );
			  assertEquals( fixture.MessageLog(),3, fixture.Members().withRole(FOLLOWER).size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddMultipleInstancesToExistingRaftGroup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAddMultipleInstancesToExistingRaftGroup()
		 {
			  // given
			  DirectNetworking net = new DirectNetworking();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId leader = member(0);
			  MemberId leader = member( 0 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId stable1 = member(1);
			  MemberId stable1 = member( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId stable2 = member(2);
			  MemberId stable2 = member( 2 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId toBeAdded1 = member(3);
			  MemberId toBeAdded1 = member( 3 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId toBeAdded2 = member(4);
			  MemberId toBeAdded2 = member( 4 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId toBeAdded3 = member(5);
			  MemberId toBeAdded3 = member( 5 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId[] initialMembers = {leader, stable1, stable2};
			  MemberId[] initialMembers = new MemberId[] { leader, stable1, stable2 };
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId[] finalMembers = {leader, stable1, stable2, toBeAdded1, toBeAdded2, toBeAdded3};
			  MemberId[] finalMembers = new MemberId[] { leader, stable1, stable2, toBeAdded1, toBeAdded2, toBeAdded3 };

			  RaftTestFixture fixture = new RaftTestFixture( net, 3, finalMembers );
			  fixture.Bootstrap( initialMembers );

			  fixture.Members().withId(leader).timerService().invoke(ELECTION);
			  net.ProcessMessages();

			  // when
			  fixture.Members().TargetMembershipSet = (new RaftTestGroup(finalMembers)).Members;
			  net.ProcessMessages();

			  // We need a heartbeat for every member we add. It is necessary to have the new members report their state
			  // so their membership change can be processed. We can probably do better here.
			  fixture.Members().withId(leader).timerService().invoke(HEARTBEAT);
			  net.ProcessMessages();
			  fixture.Members().withId(leader).timerService().invoke(HEARTBEAT);
			  net.ProcessMessages();
			  fixture.Members().withId(leader).timerService().invoke(HEARTBEAT);
			  net.ProcessMessages();

			  // then
			  assertThat( fixture.MessageLog(), fixture.Members().withIds(finalMembers), HasCurrentMembers(new RaftTestGroup(finalMembers)) );
			  assertEquals( fixture.MessageLog(), 1, fixture.Members().withRole(LEADER).size() );
			  assertEquals( fixture.MessageLog(), 5, fixture.Members().withRole(FOLLOWER).size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveSingleInstanceFromExistingRaftGroup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveSingleInstanceFromExistingRaftGroup()
		 {
			  DirectNetworking net = new DirectNetworking();

			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId leader = member(0);
			  MemberId leader = member( 0 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId stable = member(1);
			  MemberId stable = member( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId toBeRemoved = member(2);
			  MemberId toBeRemoved = member( 2 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId[] initialMembers = {leader, stable, toBeRemoved};
			  MemberId[] initialMembers = new MemberId[] { leader, stable, toBeRemoved };
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId[] finalMembers = {leader, stable};
			  MemberId[] finalMembers = new MemberId[] { leader, stable };

			  RaftTestFixture fixture = new RaftTestFixture( net, 2, initialMembers );
			  fixture.Bootstrap( initialMembers );

			  fixture.Members().withId(leader).timerService().invoke(ELECTION);

			  // when
			  fixture.Members().TargetMembershipSet = (new RaftTestGroup(finalMembers)).Members;
			  net.ProcessMessages();

			  // then
			  assertThat( fixture.MessageLog(), fixture.Members().withIds(finalMembers), HasCurrentMembers(new RaftTestGroup(finalMembers)) );
			  assertEquals( fixture.MessageLog(), 1, fixture.Members().withIds(finalMembers).withRole(LEADER).size() );
			  assertEquals( fixture.MessageLog(), 1, fixture.Members().withIds(finalMembers).withRole(FOLLOWER).size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveMultipleInstancesFromExistingRaftGroup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveMultipleInstancesFromExistingRaftGroup()
		 {
			  DirectNetworking net = new DirectNetworking();

			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId leader = member(0);
			  MemberId leader = member( 0 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId stable = member(1);
			  MemberId stable = member( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId toBeRemoved1 = member(2);
			  MemberId toBeRemoved1 = member( 2 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId toBeRemoved2 = member(3);
			  MemberId toBeRemoved2 = member( 3 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId toBeRemoved3 = member(4);
			  MemberId toBeRemoved3 = member( 4 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId[] initialMembers = {leader, stable, toBeRemoved1, toBeRemoved2, toBeRemoved3};
			  MemberId[] initialMembers = new MemberId[] { leader, stable, toBeRemoved1, toBeRemoved2, toBeRemoved3 };
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId[] finalMembers = {leader, stable};
			  MemberId[] finalMembers = new MemberId[] { leader, stable };

			  RaftTestFixture fixture = new RaftTestFixture( net, 2, initialMembers );
			  fixture.Bootstrap( initialMembers );

			  fixture.Members().withId(leader).timerService().invoke(ELECTION);
			  net.ProcessMessages();

			  // when
			  fixture.Members().withId(leader).raftInstance().TargetMembershipSet = (new RaftTestGroup(finalMembers)).Members;
			  net.ProcessMessages();

			  // then
			  assertThat( fixture.Members().withIds(finalMembers), HasCurrentMembers(new RaftTestGroup(finalMembers)) );
			  assertEquals( fixture.MessageLog(), 1, fixture.Members().withIds(finalMembers).withRole(LEADER).size() );
			  assertEquals( fixture.MessageLog(), 1, fixture.Members().withIds(finalMembers).withRole(FOLLOWER).size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleMixedChangeToExistingRaftGroup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleMixedChangeToExistingRaftGroup()
		 {
			  DirectNetworking net = new DirectNetworking();

			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId leader = member(0);
			  MemberId leader = member( 0 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId stable = member(1);
			  MemberId stable = member( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId toBeRemoved1 = member(2);
			  MemberId toBeRemoved1 = member( 2 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId toBeRemoved2 = member(3);
			  MemberId toBeRemoved2 = member( 3 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId toBeAdded1 = member(4);
			  MemberId toBeAdded1 = member( 4 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId toBeAdded2 = member(5);
			  MemberId toBeAdded2 = member( 5 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId[] everyone = {leader, stable, toBeRemoved1, toBeRemoved2, toBeAdded1, toBeAdded2};
			  MemberId[] everyone = new MemberId[] { leader, stable, toBeRemoved1, toBeRemoved2, toBeAdded1, toBeAdded2 };

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId[] initialMembers = {leader, stable, toBeRemoved1, toBeRemoved2};
			  MemberId[] initialMembers = new MemberId[] { leader, stable, toBeRemoved1, toBeRemoved2 };
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId[] finalMembers = {leader, stable, toBeAdded1, toBeAdded2};
			  MemberId[] finalMembers = new MemberId[] { leader, stable, toBeAdded1, toBeAdded2 };

			  RaftTestFixture fixture = new RaftTestFixture( net, 3, everyone );
			  fixture.Bootstrap( initialMembers );

			  fixture.Members().withId(leader).timerService().invoke(ELECTION);
			  net.ProcessMessages();

			  // when
			  fixture.Members().withId(leader).raftInstance().TargetMembershipSet = (new RaftTestGroup(finalMembers)).Members;
			  net.ProcessMessages();

			  fixture.Members().withId(leader).timerService().invoke(HEARTBEAT);
			  net.ProcessMessages();
			  fixture.Members().withId(leader).timerService().invoke(HEARTBEAT);
			  net.ProcessMessages();
			  fixture.Members().withId(leader).timerService().invoke(HEARTBEAT);
			  net.ProcessMessages();

			  // then
			  assertThat( fixture.Members().withIds(finalMembers), HasCurrentMembers(new RaftTestGroup(finalMembers)) );
			  assertEquals( fixture.MessageLog(), 1, fixture.Members().withIds(finalMembers).withRole(LEADER).size() );
			  assertEquals( fixture.MessageLog(), 3, fixture.Members().withIds(finalMembers).withRole(FOLLOWER).size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveLeaderFromExistingRaftGroupAndActivelyTransferLeadership() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveLeaderFromExistingRaftGroupAndActivelyTransferLeadership()
		 {
			  DirectNetworking net = new DirectNetworking();

			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId leader = member(0);
			  MemberId leader = member( 0 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId stable1 = member(1);
			  MemberId stable1 = member( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId stable2 = member(2);
			  MemberId stable2 = member( 2 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId[] initialMembers = {leader, stable1, stable2};
			  MemberId[] initialMembers = new MemberId[] { leader, stable1, stable2 };
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId[] finalMembers = {stable1, stable2};
			  MemberId[] finalMembers = new MemberId[] { stable1, stable2 };

			  RaftTestFixture fixture = new RaftTestFixture( net, 2, initialMembers );
			  fixture.Bootstrap( initialMembers );
			  fixture.Members().withId(leader).timerService().invoke(ELECTION);
			  net.ProcessMessages();

			  // when
			  fixture.Members().withId(leader).raftInstance().TargetMembershipSet = (new RaftTestGroup(finalMembers)).Members;
			  net.ProcessMessages();

			  fixture.Members().withId(stable1).timerService().invoke(ELECTION);
			  net.ProcessMessages();

			  // then
			  assertThat( fixture.MessageLog(), fixture.Members().withIds(finalMembers), HasCurrentMembers(new RaftTestGroup(finalMembers)) );
			  assertTrue( fixture.MessageLog(), fixture.Members().withId(stable1).raftInstance().Leader || fixture.Members().withId(stable2).raftInstance().Leader );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveLeaderAndAddItBackIn() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveLeaderAndAddItBackIn()
		 {
			  DirectNetworking net = new DirectNetworking();

			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId leader1 = member(0);
			  MemberId leader1 = member( 0 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId leader2 = member(1);
			  MemberId leader2 = member( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId stable1 = member(2);
			  MemberId stable1 = member( 2 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId stable2 = member(3);
			  MemberId stable2 = member( 3 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId[] allMembers = {leader1, leader2, stable1, stable2};
			  MemberId[] allMembers = new MemberId[] { leader1, leader2, stable1, stable2 };
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId[] fewerMembers = {leader2, stable1, stable2};
			  MemberId[] fewerMembers = new MemberId[] { leader2, stable1, stable2 };

			  RaftTestFixture fixture = new RaftTestFixture( net, 3, allMembers );
			  fixture.Bootstrap( allMembers );

			  // when
			  fixture.Members().withId(leader1).timerService().invoke(ELECTION);
			  net.ProcessMessages();

			  fixture.Members().withId(leader1).raftInstance().setTargetMembershipSet(new RaftTestGroup(fewerMembers)
						 .Members);
			  net.ProcessMessages();

			  fixture.Members().withId(leader2).timerService().invoke(ELECTION);
			  net.ProcessMessages();

			  fixture.Members().withId(leader2).raftInstance().setTargetMembershipSet(new RaftTestGroup(allMembers)
						 .Members);
			  net.ProcessMessages();

			  fixture.Members().withId(leader2).timerService().invoke(HEARTBEAT);
			  net.ProcessMessages();

			  // then
			  assertTrue( fixture.MessageLog(), fixture.Members().withId(leader2).raftInstance().Leader );
			  assertThat( fixture.MessageLog(), fixture.Members().withIds(allMembers), HasCurrentMembers(new RaftTestGroup(allMembers)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveFollowerAndAddItBackIn() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveFollowerAndAddItBackIn()
		 {
			  DirectNetworking net = new DirectNetworking();

			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId leader = member(0);
			  MemberId leader = member( 0 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId unstable = member(1);
			  MemberId unstable = member( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId stable1 = member(2);
			  MemberId stable1 = member( 2 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId stable2 = member(3);
			  MemberId stable2 = member( 3 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId[] allMembers = {leader, unstable, stable1, stable2};
			  MemberId[] allMembers = new MemberId[] { leader, unstable, stable1, stable2 };
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId[] fewerMembers = {leader, stable1, stable2};
			  MemberId[] fewerMembers = new MemberId[] { leader, stable1, stable2 };

			  RaftTestFixture fixture = new RaftTestFixture( net, 3, allMembers );
			  fixture.Bootstrap( allMembers );

			  // when
			  fixture.Members().withId(leader).timerService().invoke(ELECTION);
			  net.ProcessMessages();

			  fixture.Members().withId(leader).raftInstance().TargetMembershipSet = (new RaftTestGroup(fewerMembers)).Members;
			  net.ProcessMessages();

			  assertTrue( fixture.Members().withId(leader).raftInstance().Leader );
			  assertThat( fixture.Members().withIds(fewerMembers), HasCurrentMembers(new RaftTestGroup(fewerMembers)) );

			  fixture.Members().withId(leader).raftInstance().TargetMembershipSet = (new RaftTestGroup(allMembers)).Members;
			  net.ProcessMessages();

			  fixture.Members().withId(leader).timerService().invoke(HEARTBEAT);
			  net.ProcessMessages();

			  // then
			  assertTrue( fixture.MessageLog(), fixture.Members().withId(leader).raftInstance().Leader );
			  assertThat( fixture.MessageLog(), fixture.Members().withIds(allMembers), HasCurrentMembers(new RaftTestGroup(allMembers)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldElectNewLeaderWhenOldOneAbruptlyLeaves() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldElectNewLeaderWhenOldOneAbruptlyLeaves()
		 {
			  DirectNetworking net = new DirectNetworking();

			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId leader1 = member(0);
			  MemberId leader1 = member( 0 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId leader2 = member(1);
			  MemberId leader2 = member( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId stable = member(2);
			  MemberId stable = member( 2 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId[] initialMembers = {leader1, leader2, stable};
			  MemberId[] initialMembers = new MemberId[] { leader1, leader2, stable };

			  RaftTestFixture fixture = new RaftTestFixture( net, 2, initialMembers );
			  fixture.Bootstrap( initialMembers );

			  fixture.Members().withId(leader1).timerService().invoke(ELECTION);
			  net.ProcessMessages();

			  // when
			  net.Disconnect( leader1 );
			  fixture.Members().withId(leader2).timerService().invoke(ELECTION);
			  net.ProcessMessages();

			  // then
			  assertTrue( fixture.MessageLog(), fixture.Members().withId(leader2).raftInstance().Leader );
			  assertFalse( fixture.MessageLog(), fixture.Members().withId(stable).raftInstance().Leader );
			  assertEquals( fixture.MessageLog(), 1, fixture.Members().withIds(leader2, stable).withRole(LEADER).size() );
			  assertEquals( fixture.MessageLog(), 1, fixture.Members().withIds(leader2, stable).withRole(FOLLOWER).size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: private org.hamcrest.Matcher<? super org.Neo4Net.causalclustering.core.consensus.RaftTestFixture.Members> hasCurrentMembers(final RaftTestGroup raftGroup)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 private Matcher<object> HasCurrentMembers( RaftTestGroup raftGroup )
		 {
			  return new TypeSafeMatcherAnonymousInnerClass( this, raftGroup );
		 }

		 private class TypeSafeMatcherAnonymousInnerClass : TypeSafeMatcher<RaftTestFixture.Members>
		 {
			 private readonly RaftGroupMembershipTest _outerInstance;

			 private Neo4Net.causalclustering.core.consensus.membership.RaftTestGroup _raftGroup;

			 public TypeSafeMatcherAnonymousInnerClass( RaftGroupMembershipTest outerInstance, Neo4Net.causalclustering.core.consensus.membership.RaftTestGroup raftGroup )
			 {
				 this.outerInstance = outerInstance;
				 this._raftGroup = raftGroup;
			 }

			 protected internal override bool matchesSafely( RaftTestFixture.Members members )
			 {
				  foreach ( RaftTestFixture.MemberFixture finalMember in members )
				  {
						if ( !_raftGroup.Equals( new RaftTestGroup( finalMember.RaftInstance().replicationMembers() ) ) )
						{
							 return false;
						}
				  }
				  return true;
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "Raft group: " ).appendValue( _raftGroup );
			 }
		 }
	}

}