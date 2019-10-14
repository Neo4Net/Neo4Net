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
namespace Neo4Net.causalclustering.core.consensus.roles
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Mock = org.mockito.Mock;
	using MockitoJUnitRunner = org.mockito.junit.MockitoJUnitRunner;

	using MembershipEntry = Neo4Net.causalclustering.core.consensus.membership.MembershipEntry;
	using OnDemandTimerService = Neo4Net.causalclustering.core.consensus.schedule.OnDemandTimerService;
	using TimerService = Neo4Net.causalclustering.core.consensus.schedule.TimerService;
	using RaftCoreState = Neo4Net.causalclustering.core.state.snapshot.RaftCoreState;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using RaftTestMemberSetBuilder = Neo4Net.causalclustering.identity.RaftTestMemberSetBuilder;
	using Neo4Net.causalclustering.messaging;
	using Neo4Net.causalclustering.messaging;
	using Clocks = Neo4Net.Time.Clocks;
	using FakeClock = Neo4Net.Time.FakeClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.isA;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.atLeast;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.TestMessageBuilders.voteRequest;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.TestMessageBuilders.voteResponse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.roles.Role.CANDIDATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.roles.Role.LEADER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.identity.RaftTestMember.member;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;


//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(MockitoJUnitRunner.class) public class ElectionTest
	public class ElectionTest
	{
		 private MemberId _myself = member( 0 );

		 /* A few members that we use at will in tests. */
		 private MemberId _member1 = member( 1 );
		 private MemberId _member2 = member( 2 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock private org.neo4j.causalclustering.messaging.Inbound inbound;
		 private Inbound _inbound;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock private org.neo4j.causalclustering.messaging.Outbound<org.neo4j.causalclustering.identity.MemberId, org.neo4j.causalclustering.core.consensus.RaftMessages_RaftMessage> outbound;
		 private Outbound<MemberId, Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> _outbound;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void candidateShouldWinElectionAndBecomeLeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CandidateShouldWinElectionAndBecomeLeader()
		 {
			  // given
			  FakeClock fakeClock = Clocks.fakeClock();
			  TimerService timeouts = new OnDemandTimerService( fakeClock );
			  RaftMachine raft = ( new RaftMachineBuilder( _myself, 3, RaftTestMemberSetBuilder.INSTANCE ) ).outbound( _outbound ).timerService( timeouts ).clock( fakeClock ).build();

			  raft.InstallCoreState( new RaftCoreState( new MembershipEntry( 0, asSet( _myself, _member1, _member2 ) ) ) );
			  raft.PostRecoveryActions();

			  timeouts.Invoke( RaftMachine.Timeouts.ELECTION );

			  // when
			  raft.Handle( voteResponse().from(_member1).term(1).grant().build() );
			  raft.Handle( voteResponse().from(_member2).term(1).grant().build() );

			  // then
			  assertEquals( 1, raft.Term() );
			  assertEquals( LEADER, raft.CurrentRole() );

			  /*
			   * We require atLeast here because RaftMachine has its own scheduled service, which can spuriously wake up and
			   * send empty entries. These are fine and have no bearing on the correctness of this test, but can cause it
			   * fail if we expect exactly 2 of these messages
			   */
			  verify( _outbound, atLeast( 1 ) ).send( eq( _member1 ), isA( typeof( Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request ) ) );
			  verify( _outbound, atLeast( 1 ) ).send( eq( _member2 ), isA( typeof( Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void candidateShouldLoseElectionAndRemainCandidate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CandidateShouldLoseElectionAndRemainCandidate()
		 {
			  // Note the etcd implementation seems to diverge from the paper here, since the paper suggests that it should
			  // remain as a candidate

			  // given
			  FakeClock fakeClock = Clocks.fakeClock();
			  TimerService timeouts = new OnDemandTimerService( fakeClock );
			  RaftMachine raft = ( new RaftMachineBuilder( _myself, 3, RaftTestMemberSetBuilder.INSTANCE ) ).outbound( _outbound ).timerService( timeouts ).clock( fakeClock ).build();

			  raft.InstallCoreState( new RaftCoreState( new MembershipEntry( 0, asSet( _myself, _member1, _member2 ) ) ) );
			  raft.PostRecoveryActions();

			  timeouts.Invoke( RaftMachine.Timeouts.ELECTION );

			  // when
			  raft.Handle( voteResponse().from(_member1).term(1).deny().build() );
			  raft.Handle( voteResponse().from(_member2).term(1).deny().build() );

			  // then
			  assertEquals( 1, raft.Term() );
			  assertEquals( CANDIDATE, raft.CurrentRole() );

			  verify( _outbound, never() ).send(eq(_member1), isA(typeof(Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request)));
			  verify( _outbound, never() ).send(eq(_member2), isA(typeof(Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request)));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void candidateShouldVoteForTheSameCandidateInTheSameTerm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CandidateShouldVoteForTheSameCandidateInTheSameTerm()
		 {
			  // given
			  FakeClock fakeClock = Clocks.fakeClock();
			  TimerService timeouts = new OnDemandTimerService( fakeClock );
			  RaftMachine raft = ( new RaftMachineBuilder( _myself, 3, RaftTestMemberSetBuilder.INSTANCE ) ).outbound( _outbound ).timerService( timeouts ).clock( fakeClock ).build();

			  raft.InstallCoreState( new RaftCoreState( new MembershipEntry( 0, asSet( _myself, _member1, _member2 ) ) ) );

			  // when
			  raft.Handle( voteRequest().from(_member1).candidate(_member1).term(1).build() );
			  raft.Handle( voteRequest().from(_member1).candidate(_member1).term(1).build() );

			  // then
			  verify( _outbound, times( 2 ) ).send( _member1, voteResponse().term(1).grant().build() );
		 }
	}

}