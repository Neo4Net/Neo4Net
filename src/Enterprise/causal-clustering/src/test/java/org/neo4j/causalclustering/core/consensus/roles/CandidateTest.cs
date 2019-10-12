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
namespace Neo4Net.causalclustering.core.consensus.roles
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using MockitoJUnitRunner = org.mockito.junit.MockitoJUnitRunner;

	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using AppendLogEntry = Neo4Net.causalclustering.core.consensus.outcome.AppendLogEntry;
	using Outcome = Neo4Net.causalclustering.core.consensus.outcome.Outcome;
	using RaftState = Neo4Net.causalclustering.core.consensus.state.RaftState;
	using RaftStateBuilder = Neo4Net.causalclustering.core.consensus.state.RaftStateBuilder;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.hasItems;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.TestMessageBuilders.preVoteRequest;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.TestMessageBuilders.preVoteResponse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.TestMessageBuilders.voteRequest;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.TestMessageBuilders.voteResponse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.roles.Role.CANDIDATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.roles.Role.FOLLOWER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.roles.Role.LEADER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.state.RaftStateBuilder.raftState;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.identity.RaftTestMember.member;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(MockitoJUnitRunner.class) public class CandidateTest
	public class CandidateTest
	{
		 private MemberId _myself = member( 0 );
		 private MemberId _member1 = member( 1 );
		 private MemberId _member2 = member( 2 );

		 private LogProvider _logProvider = NullLogProvider.Instance;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeElectedLeaderOnReceivingGrantedVoteResponseWithCurrentTerm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeElectedLeaderOnReceivingGrantedVoteResponseWithCurrentTerm()
		 {
			  // given
			  RaftState state = RaftStateBuilder.raftState().term(1).myself(_myself).votingMembers(_member1, _member2).replicationMembers(_member1, _member2).build();

			  // when
			  Outcome outcome = CANDIDATE.handler.handle( voteResponse().term(state.Term()).from(_member1).grant().build(), state, Log() );

			  // then
			  assertEquals( LEADER, outcome.Role );
			  assertTrue( outcome.ElectionTimeoutRenewed() );
			  assertThat( outcome.LogCommands, hasItem( new AppendLogEntry( 0, new RaftLogEntry( state.Term(), new NewLeaderBarrier() ) ) ) );
			  assertThat( outcome.OutgoingMessages, hasItems( new Neo4Net.causalclustering.core.consensus.RaftMessages_Directed( _member1, new Neo4Net.causalclustering.core.consensus.RaftMessages_Heartbeat( _myself, state.Term(), -1, -1 ) ), new Neo4Net.causalclustering.core.consensus.RaftMessages_Directed(_member2, new Neo4Net.causalclustering.core.consensus.RaftMessages_Heartbeat(_myself, state.Term(), -1, -1)) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStayAsCandidateOnReceivingDeniedVoteResponseWithCurrentTerm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStayAsCandidateOnReceivingDeniedVoteResponseWithCurrentTerm()
		 {
			  // given
			  RaftState state = NewState();

			  // when
			  Outcome outcome = CANDIDATE.handler.handle( voteResponse().term(state.Term()).from(_member1).deny().build(), state, Log() );

			  // then
			  assertEquals( CANDIDATE, outcome.Role );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateTermOnReceivingVoteResponseWithLaterTerm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpdateTermOnReceivingVoteResponseWithLaterTerm()
		 {
			  // given
			  RaftState state = NewState();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long voterTerm = state.term() + 1;
			  long voterTerm = state.Term() + 1;

			  // when
			  Outcome outcome = CANDIDATE.handler.handle( voteResponse().term(voterTerm).from(_member1).grant().build(), state, Log() );

			  // then
			  assertEquals( FOLLOWER, outcome.Role );
			  assertEquals( voterTerm, outcome.Term );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRejectVoteResponseWithOldTerm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRejectVoteResponseWithOldTerm()
		 {
			  // given
			  RaftState state = NewState();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long voterTerm = state.term() - 1;
			  long voterTerm = state.Term() - 1;

			  // when
			  Outcome outcome = CANDIDATE.handler.handle( voteResponse().term(voterTerm).from(_member1).grant().build(), state, Log() );

			  // then
			  assertEquals( CANDIDATE, outcome.Role );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeclineVoteRequestsIfFromSameTerm() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDeclineVoteRequestsIfFromSameTerm()
		 {
			  // given
			  RaftState raftState = NewState();

			  // when
			  Outcome outcome = CANDIDATE.handler.handle( voteRequest().candidate(_member1).from(_member1).term(raftState.Term()).build(), raftState, Log() );

			  // then
			  assertThat( outcome.OutgoingMessages, hasItem( new Neo4Net.causalclustering.core.consensus.RaftMessages_Directed( _member1, voteResponse().term(raftState.Term()).from(_myself).deny().build() ) ) );
			  assertEquals( Role.Candidate, outcome.Role );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBecomeFollowerIfReceiveVoteRequestFromLaterTerm() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBecomeFollowerIfReceiveVoteRequestFromLaterTerm()
		 {
			  // given
			  RaftState raftState = NewState();

			  // when
			  long newTerm = raftState.Term() + 1;
			  Outcome outcome = CANDIDATE.handler.handle( voteRequest().candidate(_member1).from(_member1).term(newTerm).build(), raftState, Log() );

			  // then
			  assertEquals( newTerm,outcome.Term );
			  assertEquals( Role.Follower, outcome.Role );
			  assertThat( outcome.VotesForMe, empty() );

			  assertThat( outcome.OutgoingMessages, hasItem( new Neo4Net.causalclustering.core.consensus.RaftMessages_Directed( _member1, voteResponse().term(newTerm).from(_myself).grant().build() ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeclinePreVoteFromSameTerm() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDeclinePreVoteFromSameTerm()
		 {
			  // given
			  RaftState raftState = raftState().myself(_myself).supportsPreVoting(true).build();

			  // when
			  Outcome outcome = CANDIDATE.handler.handle( preVoteRequest().candidate(_member1).from(_member1).term(raftState.Term()).build(), raftState, Log() );

			  // then
			  assertThat( outcome.OutgoingMessages, hasItem( new Neo4Net.causalclustering.core.consensus.RaftMessages_Directed( _member1, preVoteResponse().term(raftState.Term()).from(_myself).deny().build() ) ) );
			  assertEquals( Role.Candidate, outcome.Role );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBecomeFollowerIfReceivePreVoteRequestFromLaterTerm() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBecomeFollowerIfReceivePreVoteRequestFromLaterTerm()
		 {
			  // given
			  RaftState raftState = raftState().myself(_myself).supportsPreVoting(true).build();
			  long newTerm = raftState.Term() + 1;

			  // when
			  Outcome outcome = CANDIDATE.handler.handle( preVoteRequest().candidate(_member1).from(_member1).term(newTerm).build(), raftState, Log() );

			  // then
			  assertEquals( newTerm,outcome.Term );
			  assertEquals( Role.Follower, outcome.Role );
			  assertThat( outcome.VotesForMe, empty() );

			  assertThat( outcome.OutgoingMessages, hasItem( new Neo4Net.causalclustering.core.consensus.RaftMessages_Directed( _member1, preVoteResponse().term(newTerm).from(_myself).deny().build() ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.causalclustering.core.consensus.state.RaftState newState() throws java.io.IOException
		 public virtual RaftState NewState()
		 {
			  return raftState().myself(_myself).build();
		 }

		 private Log Log()
		 {
			  return _logProvider.getLog( this.GetType() );
		 }

	}

}