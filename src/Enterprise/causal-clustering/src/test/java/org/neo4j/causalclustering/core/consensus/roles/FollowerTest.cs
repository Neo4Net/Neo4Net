using System.Collections.Generic;

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


	using InMemoryRaftLog = Neo4Net.causalclustering.core.consensus.log.InMemoryRaftLog;
	using RaftLog = Neo4Net.causalclustering.core.consensus.log.RaftLog;
	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using RaftTestGroup = Neo4Net.causalclustering.core.consensus.membership.RaftTestGroup;
	using Outcome = Neo4Net.causalclustering.core.consensus.outcome.Outcome;
	using RaftState = Neo4Net.causalclustering.core.consensus.state.RaftState;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Log = Neo4Net.Logging.Log;
	using NullLog = Neo4Net.Logging.NullLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.MessageUtils.messageFor;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
	using static Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.TestMessageBuilders.appendEntriesRequest;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.roles.Role.CANDIDATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.roles.Role.FOLLOWER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.state.RaftStateBuilder.raftState;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.identity.RaftTestMember.member;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;

	public class FollowerTest
	{
		 private MemberId _myself = member( 0 );

		 /* A few members that we use at will in tests. */
		 private MemberId _member1 = member( 1 );
		 private MemberId _member2 = member( 2 );
		 private MemberId _member3 = member( 3 );
		 private MemberId _member4 = member( 4 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInstigateAnElectionAfterTimeout() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInstigateAnElectionAfterTimeout()
		 {
			  // given
			  RaftState state = raftState().myself(_myself).votingMembers(asSet(_myself, _member1, _member2)).build();

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new RaftMessages_Timeout_Election(_myself), state, Log());

			  // then
			  assertEquals( Neo4Net.causalclustering.core.consensus.RaftMessages_Type.VoteRequest, messageFor( outcome, _member1 ).type() );
			  assertEquals( Neo4Net.causalclustering.core.consensus.RaftMessages_Type.VoteRequest, messageFor( outcome, _member2 ).type() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBecomeCandidateOnReceivingElectionTimeoutMessage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBecomeCandidateOnReceivingElectionTimeoutMessage()
		 {
			  // given
			  RaftState state = raftState().myself(_myself).votingMembers(asSet(_myself, _member1, _member2)).build();

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new RaftMessages_Timeout_Election(_myself), state, Log());

			  // then
			  assertEquals( CANDIDATE, outcome.Role );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotInstigateElectionOnElectionTimeoutIfRefusingToBeLeaderAndPreVoteNotSupported() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotInstigateElectionOnElectionTimeoutIfRefusingToBeLeaderAndPreVoteNotSupported()
		 {
			  // given
			  RaftState state = raftState().setRefusesToBeLeader(true).supportsPreVoting(false).build();

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new RaftMessages_Timeout_Election(_myself), state, Log());

			  // then
			  assertThat( outcome.OutgoingMessages, empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreAnElectionTimeoutIfRefusingToBeLeaderAndPreVoteNotSupported() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreAnElectionTimeoutIfRefusingToBeLeaderAndPreVoteNotSupported()
		 {
			  // given
			  RaftState state = raftState().setRefusesToBeLeader(true).supportsPreVoting(false).build();

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new RaftMessages_Timeout_Election(_myself), state, Log());

			  // then
			  assertEquals( new Outcome( Role.Follower, state ), outcome );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetPreElectionOnTimeoutIfSupportedAndIAmVoterAndIRefuseToLead() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetPreElectionOnTimeoutIfSupportedAndIAmVoterAndIRefuseToLead()
		 {
			  // given
			  RaftState state = raftState().myself(_myself).votingMembers(_myself, _member1, _member2).setRefusesToBeLeader(true).supportsPreVoting(true).build();

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new RaftMessages_Timeout_Election(_myself), state, Log());

			  // then
			  assertTrue( outcome.PreElection );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSetPreElectionOnTimeoutIfSupportedAndIAmNotVoterAndIRefuseToLead() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSetPreElectionOnTimeoutIfSupportedAndIAmNotVoterAndIRefuseToLead()
		 {
			  // given
			  RaftState state = raftState().myself(_myself).votingMembers(_member1, _member2, _member3).setRefusesToBeLeader(true).supportsPreVoting(true).build();

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new RaftMessages_Timeout_Election(_myself), state, Log());

			  // then
			  assertFalse( outcome.PreElection );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSolicitPreVotesOnTimeoutEvenIfSupportedIfRefuseToLead() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSolicitPreVotesOnTimeoutEvenIfSupportedIfRefuseToLead()
		 {
			  // given
			  RaftState state = raftState().setRefusesToBeLeader(true).supportsPreVoting(true).build();

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new RaftMessages_Timeout_Election(_myself), state, Log());

			  // then
			  assertThat( outcome.OutgoingMessages, empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void followerReceivingHeartbeatIndicatingClusterIsAheadShouldElicitAppendResponse() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FollowerReceivingHeartbeatIndicatingClusterIsAheadShouldElicitAppendResponse()
		 {
			  // given
			  int term = 1;
			  int followerAppendIndex = 9;
			  RaftLog entryLog = new InMemoryRaftLog();
			  entryLog.Append( new RaftLogEntry( 0, new RaftTestGroup( 0 ) ) );
			  RaftState state = raftState().myself(_myself).term(term).build();

			  Follower follower = new Follower();
			  AppendSomeEntriesToLog( state, follower, followerAppendIndex - 1, term, 1 );

			  AppendEntries.Request heartbeat = appendEntriesRequest().from(_member1).leaderTerm(term).prevLogIndex(followerAppendIndex + 2).prevLogTerm(term).build(); // no entries, this is a heartbeat

			  Outcome outcome = follower.Handle( heartbeat, state, Log() );

			  assertEquals( 1, outcome.OutgoingMessages.Count );
			  RaftMessages_RaftMessage outgoing = outcome.OutgoingMessages.GetEnumerator().next().message();
			  assertEquals( Neo4Net.causalclustering.core.consensus.RaftMessages_Type.AppendEntriesResponse, outgoing.Type() );
			  Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Response response = ( AppendEntries.Response ) outgoing;
			  assertFalse( response.Success() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTruncateIfTermDoesNotMatch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTruncateIfTermDoesNotMatch()
		 {
			  // given
			  RaftLog entryLog = new InMemoryRaftLog();
			  entryLog.Append( new RaftLogEntry( 0, new RaftTestGroup( 0 ) ) );
			  int term = 1;
			  RaftState state = raftState().myself(_myself).entryLog(entryLog).term(term).build();

			  Follower follower = new Follower();

			  state.Update( follower.Handle( new AppendEntries.Request( _member1, 1, 0, 0, new RaftLogEntry[]{ new RaftLogEntry( 2, ContentGenerator.Content() ) }, 0 ), state, Log() ) );

			  RaftLogEntry[] entries = new RaftLogEntry[] { new RaftLogEntry( 1, new ReplicatedString( "commit this!" ) ) };

			  Outcome outcome = follower.Handle( new AppendEntries.Request( _member1, 1, 0, 0, entries, 0 ), state, Log() );
			  state.Update( outcome );

			  // then
			  assertEquals( 1, state.EntryLog().appendIndex() );
			  assertEquals( 1, state.EntryLog().readEntryTerm(1) );
		 }

		 // TODO move this to outcome tests
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void followerLearningAboutHigherCommitCausesValuesTobeAppliedToItsLog() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FollowerLearningAboutHigherCommitCausesValuesTobeAppliedToItsLog()
		 {
			  // given
			  RaftLog entryLog = new InMemoryRaftLog();
			  entryLog.Append( new RaftLogEntry( 0, new RaftTestGroup( 0 ) ) );
			  RaftState state = raftState().myself(_myself).entryLog(entryLog).build();

			  Follower follower = new Follower();

			  AppendSomeEntriesToLog( state, follower, 3, 0, 1 );

			  // when receiving AppEntries with high leader commit (4)
			  Outcome outcome = follower.Handle( new AppendEntries.Request( _myself, 0, 3, 0, new RaftLogEntry[] { new RaftLogEntry( 0, ContentGenerator.Content() ) }, 4 ), state, Log() );

			  state.Update( outcome );

			  // then
			  assertEquals( 4, state.CommitIndex() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateCommitIndexIfNecessary() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpdateCommitIndexIfNecessary()
		 {
			  //  If leaderCommit > commitIndex, set commitIndex = min( leaderCommit, index of last new entry )

			  // given
			  RaftLog entryLog = new InMemoryRaftLog();
			  entryLog.Append( new RaftLogEntry( 0, new RaftTestGroup( 0 ) ) );
			  RaftState state = raftState().myself(_myself).entryLog(entryLog).build();

			  Follower follower = new Follower();

			  int localAppendIndex = 3;
			  int localCommitIndex = localAppendIndex - 1;
			  int term = 0;
			  AppendSomeEntriesToLog( state, follower, localAppendIndex, term, 1 ); // append index is 0 based

			  // the next when-then simply verifies that the test is setup properly, with commit and append index as expected
			  // when
			  Outcome raftTestMemberOutcome = new Outcome( FOLLOWER, state );
			  raftTestMemberOutcome.CommitIndex = localCommitIndex;
			  state.Update( raftTestMemberOutcome );

			  // then
			  assertEquals( localAppendIndex, state.EntryLog().appendIndex() );
			  assertEquals( localCommitIndex, state.CommitIndex() );

			  // when
			  // an append req comes in with leader commit index > localAppendIndex but localCommitIndex < localAppendIndex
			  Outcome outcome = follower.Handle( appendEntriesRequest().leaderTerm(term).prevLogIndex(3).prevLogTerm(term).leaderCommit(localCommitIndex + 4).build(), state, Log() );

			  state.Update( outcome );

			  // then
			  // The local commit index must be brought as far along as possible
			  assertEquals( 3, state.CommitIndex() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotRenewElectionTimeoutOnReceiptOfHeartbeatInLowerTerm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotRenewElectionTimeoutOnReceiptOfHeartbeatInLowerTerm()
		 {
			  // given
			  RaftState state = raftState().myself(_myself).term(2).build();

			  Follower follower = new Follower();

			  Outcome outcome = follower.Handle( new Neo4Net.causalclustering.core.consensus.RaftMessages_Heartbeat( _myself, 1, 1, 1 ), state, Log() );

			  // then
			  assertFalse( outcome.ElectionTimeoutRenewed() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcknowledgeHeartbeats() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcknowledgeHeartbeats()
		 {
			  // given
			  RaftState state = raftState().myself(_myself).term(2).build();

			  Follower follower = new Follower();

			  Outcome outcome = follower.Handle( new Neo4Net.causalclustering.core.consensus.RaftMessages_Heartbeat( state.Leader(), 2, 2, 2 ), state, Log() );

			  // then
			  ICollection<Neo4Net.causalclustering.core.consensus.RaftMessages_Directed> outgoingMessages = outcome.OutgoingMessages;
			  assertTrue( outgoingMessages.Contains( new Neo4Net.causalclustering.core.consensus.RaftMessages_Directed( state.Leader(), new Neo4Net.causalclustering.core.consensus.RaftMessages_HeartbeatResponse(_myself) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondPositivelyToPreVoteRequestsIfWouldVoteForCandidate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondPositivelyToPreVoteRequestsIfWouldVoteForCandidate()
		 {
			  // given
			  RaftState raftState = PreElectionActive();

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Request(_member1, 0, _member1, 0, 0), raftState, Log());

			  // then
			  RaftMessages_RaftMessage raftMessage = messageFor( outcome, _member1 );
			  assertThat( raftMessage.Type(), equalTo(Neo4Net.causalclustering.core.consensus.RaftMessages_Type.PreVoteResponse) );
			  assertThat( ( ( Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response )raftMessage ).VoteGranted(), equalTo(true) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondPositivelyToPreVoteRequestsEvenIfAlreadyVotedInRealElection() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondPositivelyToPreVoteRequestsEvenIfAlreadyVotedInRealElection()
		 {
			  // given
			  RaftState raftState = PreElectionActive();
			  raftState.Update( ( new Follower() ).Handle(new Neo4Net.causalclustering.core.consensus.RaftMessages_Vote_Request(_member1, 0, _member1, 0, 0), raftState, Log()) );

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Request(_member2, 0, _member2, 0, 0), raftState, Log());

			  // then
			  RaftMessages_RaftMessage raftMessage = messageFor( outcome, _member2 );
			  assertThat( raftMessage.Type(), equalTo(Neo4Net.causalclustering.core.consensus.RaftMessages_Type.PreVoteResponse) );
			  assertThat( ( ( Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response )raftMessage ).VoteGranted(), equalTo(true) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondNegativelyToPreVoteRequestsIfNotInPreVoteMyself() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondNegativelyToPreVoteRequestsIfNotInPreVoteMyself()
		 {
			  // given
			  RaftState raftState = raftState().myself(_myself).supportsPreVoting(true).votingMembers(asSet(_myself, _member1, _member2)).setPreElection(false).build();

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Request(_member1, 0, _member1, 0, 0), raftState, Log());

			  // then
			  RaftMessages_RaftMessage raftMessage = messageFor( outcome, _member1 );
			  assertThat( raftMessage.Type(), equalTo(Neo4Net.causalclustering.core.consensus.RaftMessages_Type.PreVoteResponse) );
			  assertThat( ( ( Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response )raftMessage ).VoteGranted(), equalTo(false) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondNegativelyToPreVoteRequestsIfWouldNotVoteForCandidate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondNegativelyToPreVoteRequestsIfWouldNotVoteForCandidate()
		 {
			  // given
			  RaftState raftState = raftState().myself(_myself).term(1).setPreElection(true).supportsPreVoting(true).votingMembers(asSet(_myself, _member1, _member2)).build();

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Request(_member1, 0, _member1, 0, 0), raftState, Log());

			  // then
			  RaftMessages_RaftMessage raftMessage = messageFor( outcome, _member1 );
			  assertThat( raftMessage.Type(), equalTo(Neo4Net.causalclustering.core.consensus.RaftMessages_Type.PreVoteResponse) );
			  assertThat( ( ( Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response )raftMessage ).VoteGranted(), equalTo(false) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondPositivelyToPreVoteRequestsToMultipleMembersIfWouldVoteForAny() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondPositivelyToPreVoteRequestsToMultipleMembersIfWouldVoteForAny()
		 {
			  // given
			  RaftState raftState = PreElectionActive();

			  // when
			  Outcome outcome1 = ( new Follower() ).Handle(new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Request(_member1, 0, _member1, 0, 0), raftState, Log());
			  raftState.Update( outcome1 );
			  Outcome outcome2 = ( new Follower() ).Handle(new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Request(_member2, 0, _member2, 0, 0), raftState, Log());
			  raftState.Update( outcome2 );

			  // then
			  RaftMessages_RaftMessage raftMessage = messageFor( outcome2, _member2 );

			  assertThat( raftMessage.Type(), equalTo(Neo4Net.causalclustering.core.consensus.RaftMessages_Type.PreVoteResponse) );
			  assertThat( ( ( Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response )raftMessage ).VoteGranted(), equalTo(true) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseTermFromPreVoteRequestIfHigherThanOwn() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUseTermFromPreVoteRequestIfHigherThanOwn()
		 {
			  // given
			  RaftState raftState = PreElectionActive();
			  long newTerm = raftState.Term() + 1;

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Request(_member1, newTerm, _member1, 0, 0), raftState, Log());

			  // then
			  RaftMessages_RaftMessage raftMessage = messageFor( outcome, _member1 );

			  assertThat( raftMessage.Type(), equalTo(Neo4Net.causalclustering.core.consensus.RaftMessages_Type.PreVoteResponse) );
			  assertThat( ( ( Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response )raftMessage ).Term(), equalTo(newTerm) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateOutcomeWithTermFromPreVoteRequestOfLaterTermIfInPreVoteState() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpdateOutcomeWithTermFromPreVoteRequestOfLaterTermIfInPreVoteState()
		 {
			  // given
			  RaftState raftState = PreElectionActive();
			  long newTerm = raftState.Term() + 1;

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Request(_member1, newTerm, _member1, 0, 0), raftState, Log());

			  // then
			  assertEquals( newTerm, outcome.Term );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateOutcomeWithTermFromPreVoteRequestOfLaterTermIfNotInPreVoteState() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpdateOutcomeWithTermFromPreVoteRequestOfLaterTermIfNotInPreVoteState()
		 {
			  // given
			  RaftState raftState = raftState().myself(_myself).supportsPreVoting(true).setPreElection(false).build();
			  long newTerm = raftState.Term() + 1;

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Request(_member1, newTerm, _member1, 0, 0), raftState, Log());

			  // then

			  assertEquals( newTerm, outcome.Term );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInstigatePreElectionIfSupportedAndNotActiveAndReceiveTimeout() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInstigatePreElectionIfSupportedAndNotActiveAndReceiveTimeout()
		 {
			  // given
			  RaftState raftState = raftState().myself(_myself).votingMembers(asSet(_myself, _member1, _member2)).supportsPreVoting(true).setPreElection(false).build();

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new RaftMessages_Timeout_Election(_myself), raftState, Log());

			  // then
			  assertEquals( Neo4Net.causalclustering.core.consensus.RaftMessages_Type.PreVoteRequest, messageFor( outcome, _member1 ).type() );
			  assertEquals( Neo4Net.causalclustering.core.consensus.RaftMessages_Type.PreVoteRequest, messageFor( outcome, _member2 ).type() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetPreElectionActiveWhenReceiveTimeout() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetPreElectionActiveWhenReceiveTimeout()
		 {
			  // given
			  RaftState raftState = raftState().myself(_myself).votingMembers(asSet(_myself, _member1, _member2)).supportsPreVoting(true).setPreElection(false).build();

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new RaftMessages_Timeout_Election(_myself), raftState, Log());

			  // then
			  assertTrue( outcome.PreElection );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInstigatePreElectionIfSupportedAndActiveAndReceiveTimeout() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInstigatePreElectionIfSupportedAndActiveAndReceiveTimeout()
		 {
			  // given
			  RaftState raftState = PreElectionActive();

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new RaftMessages_Timeout_Election(_myself), raftState, Log());

			  // then
			  assertEquals( Neo4Net.causalclustering.core.consensus.RaftMessages_Type.PreVoteRequest, messageFor( outcome, _member1 ).type() );
			  assertEquals( Neo4Net.causalclustering.core.consensus.RaftMessages_Type.PreVoteRequest, messageFor( outcome, _member2 ).type() );
			  assertEquals( Neo4Net.causalclustering.core.consensus.RaftMessages_Type.PreVoteRequest, messageFor( outcome, _member3 ).type() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKeepPreElectionActiveWhenReceiveTimeout() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldKeepPreElectionActiveWhenReceiveTimeout()
		 {
			  // given
			  RaftState raftState = PreElectionActive();

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new RaftMessages_Timeout_Election(_myself), raftState, Log());

			  // then
			  assertTrue( outcome.PreElection );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAbortPreElectionIfReceivePreVoteResponseFromNewerTerm() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAbortPreElectionIfReceivePreVoteResponseFromNewerTerm()
		 {
			  // given
			  RaftState raftState = PreElectionActive();
			  long newTerm = raftState.Term() + 1;

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response(_member1, newTerm, false), raftState, Log());

			  // then
			  assertEquals( newTerm, outcome.Term );
			  assertFalse( outcome.PreElection );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreVotesFromEarlierTerms() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreVotesFromEarlierTerms()
		 {
			  // given
			  RaftState raftState = PreElectionActive();
			  long oldTerm = raftState.Term() - 1;

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response(_member1, oldTerm, true), raftState, Log());

			  // then
			  assertEquals( new Outcome( Role.Follower, raftState ), outcome );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreVotesDeclining() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreVotesDeclining()
		 {
			  // given
			  RaftState raftState = PreElectionActive();

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response(_member1, raftState.Term(), false), raftState, Log());

			  // then
			  assertEquals( new Outcome( Role.Follower, raftState ), outcome );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddVoteFromADifferentMember() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAddVoteFromADifferentMember()
		 {
			  // given
			  RaftState raftState = PreElectionActive();

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response(_member1, raftState.Term(), true), raftState, Log());

			  // then
			  assertThat( outcome.PreVotesForMe, contains( _member1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAddVoteFromMyself() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAddVoteFromMyself()
		 {
			  // given
			  RaftState raftState = PreElectionActive();

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response(_myself, raftState.Term(), true), raftState, Log());

			  // then
			  assertThat( outcome.PreVotesForMe, not( contains( _member1 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotStartElectionIfHaveNotReachedQuorum() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotStartElectionIfHaveNotReachedQuorum()
		 {
			  // given
			  RaftState raftState = PreElectionActive();

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response(_member1, raftState.Term(), true), raftState, Log());

			  // then
			  assertEquals( Role.Follower, outcome.Role );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTransitionToCandidateIfHaveReachedQuorum() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTransitionToCandidateIfHaveReachedQuorum()
		 {
			  // given
			  RaftState raftState = PreElectionActive();

			  // when
			  Outcome outcome1 = ( new Follower() ).Handle(new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response(_member1, raftState.Term(), true), raftState, Log());
			  raftState.Update( outcome1 );
			  Outcome outcome2 = ( new Follower() ).Handle(new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response(_member2, raftState.Term(), true), raftState, Log());

			  // then
			  assertEquals( Role.Candidate, outcome2.Role );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInstigateElectionIfHaveReachedQuorum() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInstigateElectionIfHaveReachedQuorum()
		 {
			  // given
			  RaftState raftState = PreElectionActive();

			  // when
			  Outcome outcome1 = ( new Follower() ).Handle(new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response(_member1, raftState.Term(), true), raftState, Log());
			  raftState.Update( outcome1 );
			  Outcome outcome2 = ( new Follower() ).Handle(new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response(_member2, raftState.Term(), true), raftState, Log());

			  // then
			  assertEquals( Neo4Net.causalclustering.core.consensus.RaftMessages_Type.VoteRequest, messageFor( outcome2, _member1 ).type() );
			  assertEquals( Neo4Net.causalclustering.core.consensus.RaftMessages_Type.VoteRequest, messageFor( outcome2, _member2 ).type() );
			  assertEquals( Neo4Net.causalclustering.core.consensus.RaftMessages_Type.VoteRequest, messageFor( outcome2, _member3 ).type() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnorePreVoteResponsesIfPreVoteInactive() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnorePreVoteResponsesIfPreVoteInactive()
		 {
			  // given
			  RaftState raftState = raftState().myself(_myself).supportsPreVoting(true).setPreElection(false).votingMembers(asSet(_myself, _member1, _member2)).build();

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response(_member1, raftState.Term(), true), raftState, Log());

			  assertEquals( new Outcome( Role.Follower, raftState ), outcome );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnorePreVoteRequestsIfPreVoteUnsupported() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnorePreVoteRequestsIfPreVoteUnsupported()
		 {
			  // given
			  RaftState raftState = raftState().myself(_myself).supportsPreVoting(false).setPreElection(false).votingMembers(asSet(_myself, _member1, _member2)).build();

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Request(_member1, raftState.Term(), _member1, 0, 0), raftState, Log());

			  assertEquals( new Outcome( Role.Follower, raftState ), outcome );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnorePreVoteResponsesIfPreVoteUnsupported() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnorePreVoteResponsesIfPreVoteUnsupported()
		 {
			  // given
			  RaftState raftState = raftState().myself(_myself).supportsPreVoting(false).setPreElection(false).votingMembers(asSet(_myself, _member1, _member2)).build();

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response(_member1, raftState.Term(), true), raftState, Log());

			  assertEquals( new Outcome( Role.Follower, raftState ), outcome );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnorePreVoteResponseWhenPreElectionFalseRefuseToBeLeader() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnorePreVoteResponseWhenPreElectionFalseRefuseToBeLeader()
		 {
			  // given
			  RaftState raftState = raftState().myself(_myself).setPreElection(false).supportsPreVoting(true).votingMembers(asSet(_myself, _member1, _member2)).setRefusesToBeLeader(true).build();

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response(_member1, raftState.Term(), true), raftState, Log());

			  // then
			  assertEquals( new Outcome( Role.Follower, raftState ), outcome );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnorePreVoteResponseWhenPreElectionTrueAndRefuseLeader() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnorePreVoteResponseWhenPreElectionTrueAndRefuseLeader()
		 {
			  // given
			  RaftState raftState = raftState().myself(_myself).setPreElection(true).supportsPreVoting(true).votingMembers(asSet(_myself, _member1, _member2)).setRefusesToBeLeader(true).build();

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response(_member1, raftState.Term(), true), raftState, Log());

			  // then
			  assertEquals( new Outcome( Role.Follower, raftState ), outcome );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotInstigateElectionOnPreVoteResponseWhenPreElectionTrueAndRefuseLeader() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotInstigateElectionOnPreVoteResponseWhenPreElectionTrueAndRefuseLeader()
		 {
			  // given
			  RaftState raftState = raftState().myself(_myself).setPreElection(true).supportsPreVoting(true).votingMembers(asSet(_myself, _member1, _member2)).setRefusesToBeLeader(true).build();

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response(_member1, raftState.Term(), true), raftState, Log());

			  // then
			  assertThat( outcome.OutgoingMessages, empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeclinePreVoteRequestsIfPreElectionNotActiveAndRefusesToLead() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDeclinePreVoteRequestsIfPreElectionNotActiveAndRefusesToLead()
		 {
			  // given
			  RaftState raftState = raftState().myself(_myself).setPreElection(false).supportsPreVoting(true).votingMembers(asSet(_myself, _member1, _member2)).setRefusesToBeLeader(true).build();

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Request(_member1, raftState.Term(), _member1, 0, 0), raftState, Log());

			  // then
			  assertFalse( ( ( Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response ) messageFor( outcome, _member1 ) ).voteGranted() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApprovePreVoteRequestIfPreElectionActiveAndRefusesToLead() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApprovePreVoteRequestIfPreElectionActiveAndRefusesToLead()
		 {
			  // given
			  RaftState raftState = raftState().myself(_myself).setPreElection(true).supportsPreVoting(true).votingMembers(asSet(_myself, _member1, _member2)).setRefusesToBeLeader(true).build();

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Request(_member1, raftState.Term(), _member1, 0, 0), raftState, Log());

			  // then
			  assertTrue( ( ( Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response ) messageFor( outcome, _member1 ) ).voteGranted() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetPreElectionOnElectionTimeout() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetPreElectionOnElectionTimeout()
		 {
			  // given
			  RaftState state = PreElectionSupported();

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new RaftMessages_Timeout_Election(_myself), state, Log());
			  state.Update( outcome );

			  // then
			  assertThat( outcome.Role, equalTo( FOLLOWER ) );
			  assertThat( outcome.PreElection, equalTo( true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendPreVoteRequestsOnElectionTimeout() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendPreVoteRequestsOnElectionTimeout()
		 {
			  // given
			  RaftState state = PreElectionSupported();

			  // when
			  Outcome outcome = ( new Follower() ).Handle(new RaftMessages_Timeout_Election(_myself), state, Log());
			  state.Update( outcome );

			  // then
			  assertThat( messageFor( outcome, _member1 ).type(), equalTo(Neo4Net.causalclustering.core.consensus.RaftMessages_Type.PreVoteRequest) );
			  assertThat( messageFor( outcome, _member2 ).type(), equalTo(Neo4Net.causalclustering.core.consensus.RaftMessages_Type.PreVoteRequest) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProceedToRealElectionIfReceiveQuorumOfPositivePreVoteResponses() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProceedToRealElectionIfReceiveQuorumOfPositivePreVoteResponses()
		 {
			  // given
			  RaftState state = PreElectionSupported();

			  Follower underTest = new Follower();

			  Outcome outcome1 = underTest.Handle( new RaftMessages_Timeout_Election( _myself ), state, Log() );
			  state.Update( outcome1 );

			  // when
			  Outcome outcome2 = underTest.Handle( new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response( _member1, 0L, true ), state, Log() );

			  // then
			  assertThat( outcome2.Role, equalTo( CANDIDATE ) );
			  assertThat( outcome2.PreElection, equalTo( false ) );
			  assertThat( outcome2.PreVotesForMe, contains( _member1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnorePreVotePositiveResponsesFromOlderTerm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnorePreVotePositiveResponsesFromOlderTerm()
		 {
			  // given
			  RaftState state = raftState().myself(_myself).term(1).supportsPreVoting(true).votingMembers(asSet(_myself, _member1, _member2)).build();

			  Follower underTest = new Follower();

			  Outcome outcome1 = underTest.Handle( new RaftMessages_Timeout_Election( _myself ), state, Log() );
			  state.Update( outcome1 );

			  // when
			  Outcome outcome2 = underTest.Handle( new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response( _member1, 0L, true ), state, Log() );

			  // then
			  assertThat( outcome2.Role, equalTo( FOLLOWER ) );
			  assertThat( outcome2.PreElection, equalTo( true ) );
			  assertThat( outcome2.PreVotesForMe, empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnorePositivePreVoteResponsesIfNotInPreVotingStage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnorePositivePreVoteResponsesIfNotInPreVotingStage()
		 {
			  // given
			  RaftState state = PreElectionSupported();

			  Follower underTest = new Follower();

			  // when
			  Outcome outcome = underTest.Handle( new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response( _member1, 0L, true ), state, Log() );

			  // then
			  assertThat( outcome.Role, equalTo( FOLLOWER ) );
			  assertThat( outcome.PreElection, equalTo( false ) );
			  assertThat( outcome.PreVotesForMe, empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotMoveToRealElectionWithoutPreVoteQuorum() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotMoveToRealElectionWithoutPreVoteQuorum()
		 {
			  // given
			  RaftState state = raftState().myself(_myself).supportsPreVoting(true).votingMembers(asSet(_myself, _member1, _member2, _member3, _member4)).build();

			  Follower underTest = new Follower();
			  Outcome outcome1 = underTest.Handle( new RaftMessages_Timeout_Election( _myself ), state, Log() );
			  state.Update( outcome1 );

			  // when
			  Outcome outcome2 = underTest.Handle( new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response( _member1, 0L, true ), state, Log() );

			  // then
			  assertThat( outcome2.Role, equalTo( FOLLOWER ) );
			  assertThat( outcome2.PreElection, equalTo( true ) );
			  assertThat( outcome2.PreVotesForMe, contains( _member1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMoveToRealElectionWithPreVoteQuorumOf5() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMoveToRealElectionWithPreVoteQuorumOf5()
		 {
			  // given
			  RaftState state = raftState().myself(_myself).supportsPreVoting(true).votingMembers(asSet(_myself, _member1, _member2, _member3, _member4)).build();

			  Follower underTest = new Follower();
			  Outcome outcome1 = underTest.Handle( new RaftMessages_Timeout_Election( _myself ), state, Log() );
			  state.Update( outcome1 );

			  // when
			  Outcome outcome2 = underTest.Handle( new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response( _member1, 0L, true ), state, Log() );
			  state.Update( outcome2 );
			  Outcome outcome3 = underTest.Handle( new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response( _member2, 0L, true ), state, Log() );

			  // then
			  assertThat( outcome3.Role, equalTo( CANDIDATE ) );
			  assertThat( outcome3.PreElection, equalTo( false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCountPreVotesVotesFromSameMemberTwice() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotCountPreVotesVotesFromSameMemberTwice()
		 {
			  // given
			  RaftState state = raftState().myself(_myself).supportsPreVoting(true).votingMembers(asSet(_myself, _member1, _member2, _member3, _member4)).build();

			  Follower underTest = new Follower();
			  Outcome outcome1 = underTest.Handle( new RaftMessages_Timeout_Election( _myself ), state, Log() );
			  state.Update( outcome1 );

			  // when
			  Outcome outcome2 = underTest.Handle( new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response( _member1, 0L, true ), state, Log() );
			  state.Update( outcome2 );
			  Outcome outcome3 = underTest.Handle( new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response( _member1, 0L, true ), state, Log() );

			  // then
			  assertThat( outcome3.Role, equalTo( FOLLOWER ) );
			  assertThat( outcome3.PreElection, equalTo( true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldResetPreVotesWhenMovingBackToFollower() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldResetPreVotesWhenMovingBackToFollower()
		 {
			  // given
			  RaftState state = PreElectionSupported();

			  Outcome outcome1 = ( new Follower() ).Handle(new RaftMessages_Timeout_Election(_myself), state, Log());
			  state.Update( outcome1 );
			  Outcome outcome2 = ( new Follower() ).Handle(new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response(_member1, 0L, true), state, Log());
			  assertThat( CANDIDATE, equalTo( outcome2.Role ) );
			  assertThat( outcome2.PreVotesForMe, contains( _member1 ) );

			  // when
			  Outcome outcome3 = ( new Candidate() ).Handle(new RaftMessages_Timeout_Election(_myself), state, Log());

			  // then
			  assertThat( outcome3.PreVotesForMe, empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendRealVoteRequestsIfReceivePositivePreVoteResponses() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendRealVoteRequestsIfReceivePositivePreVoteResponses()
		 {
			  // given
			  RaftState state = PreElectionSupported();

			  Follower underTest = new Follower();

			  Outcome outcome1 = underTest.Handle( new RaftMessages_Timeout_Election( _myself ), state, Log() );
			  state.Update( outcome1 );

			  // when
			  Outcome outcome2 = underTest.Handle( new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response( _member1, 0L, true ), state, Log() );

			  // then
			  assertThat( messageFor( outcome2, _member1 ).type(), equalTo(Neo4Net.causalclustering.core.consensus.RaftMessages_Type.VoteRequest) );
			  assertThat( messageFor( outcome2, _member2 ).type(), equalTo(Neo4Net.causalclustering.core.consensus.RaftMessages_Type.VoteRequest) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotProceedToRealElectionIfReceiveNegativePreVoteResponses() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotProceedToRealElectionIfReceiveNegativePreVoteResponses()
		 {
			  // given
			  RaftState state = PreElectionSupported();

			  Follower underTest = new Follower();

			  Outcome outcome1 = underTest.Handle( new RaftMessages_Timeout_Election( _myself ), state, Log() );
			  state.Update( outcome1 );

			  // when
			  Outcome outcome2 = underTest.Handle( new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response( _member1, 0L, false ), state, Log() );
			  state.Update( outcome2 );
			  Outcome outcome3 = underTest.Handle( new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response( _member2, 0L, false ), state, Log() );

			  // then
			  assertThat( outcome3.Role, equalTo( FOLLOWER ) );
			  assertThat( outcome3.PreElection, equalTo( true ) );
			  assertThat( outcome3.PreVotesForMe, empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSendRealVoteRequestsIfReceiveNegativePreVoteResponses() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSendRealVoteRequestsIfReceiveNegativePreVoteResponses()
		 {
			  // given
			  RaftState state = PreElectionSupported();

			  Follower underTest = new Follower();

			  Outcome outcome1 = underTest.Handle( new RaftMessages_Timeout_Election( _myself ), state, Log() );
			  state.Update( outcome1 );

			  // when
			  Outcome outcome2 = underTest.Handle( new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response( _member1, 0L, false ), state, Log() );
			  state.Update( outcome2 );
			  Outcome outcome3 = underTest.Handle( new Neo4Net.causalclustering.core.consensus.RaftMessages_PreVote_Response( _member2, 0L, false ), state, Log() );

			  // then
			  assertThat( outcome2.OutgoingMessages, empty() );
			  assertThat( outcome3.OutgoingMessages, empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldResetPreVoteIfReceiveHeartbeatFromLeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldResetPreVoteIfReceiveHeartbeatFromLeader()
		 {
			  // given
			  RaftState state = PreElectionSupported();

			  Follower underTest = new Follower();

			  Outcome outcome1 = underTest.Handle( new RaftMessages_Timeout_Election( _myself ), state, Log() );
			  state.Update( outcome1 );

			  // when
			  Outcome outcome2 = underTest.Handle( new Neo4Net.causalclustering.core.consensus.RaftMessages_Heartbeat( _member1, 0L, 0L, 0L ), state, Log() );

			  // then
			  assertThat( outcome2.Role, equalTo( FOLLOWER ) );
			  assertThat( outcome2.PreElection, equalTo( false ) );
			  assertThat( outcome2.PreVotesForMe, empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldResetPreVoteIfReceiveAppendEntriesRequestFromLeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldResetPreVoteIfReceiveAppendEntriesRequestFromLeader()
		 {
			  // given
			  RaftState state = PreElectionSupported();

			  Follower underTest = new Follower();

			  Outcome outcome1 = underTest.Handle( new RaftMessages_Timeout_Election( _myself ), state, Log() );
			  state.Update( outcome1 );

			  // when
			  Outcome outcome2 = underTest.Handle( appendEntriesRequest().leaderTerm(state.Term()).prevLogTerm(state.Term()).prevLogIndex(0).build(), state, Log() );

			  // then
			  assertThat( outcome2.Role, equalTo( FOLLOWER ) );
			  assertThat( outcome2.PreElection, equalTo( false ) );
			  assertThat( outcome2.PreVotesForMe, empty() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.causalclustering.core.consensus.state.RaftState preElectionActive() throws java.io.IOException
		 private RaftState PreElectionActive()
		 {
			  return raftState().myself(_myself).supportsPreVoting(true).setPreElection(true).votingMembers(asSet(_myself, _member1, _member2, _member3)).build();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.causalclustering.core.consensus.state.RaftState preElectionSupported() throws java.io.IOException
		 private RaftState PreElectionSupported()
		 {
			  return raftState().myself(_myself).supportsPreVoting(true).votingMembers(asSet(_myself, _member1, _member2)).build();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void appendSomeEntriesToLog(org.neo4j.causalclustering.core.consensus.state.RaftState raft, Follower follower, int numberOfEntriesToAppend, int term, int firstIndex) throws java.io.IOException
		 private void AppendSomeEntriesToLog( RaftState raft, Follower follower, int numberOfEntriesToAppend, int term, int firstIndex )
		 {
			  for ( int i = 0; i < numberOfEntriesToAppend; i++ )
			  {
					int prevLogIndex = ( firstIndex + i ) - 1;
					raft.Update( follower.Handle( new AppendEntries.Request( _myself, term, prevLogIndex, term, new RaftLogEntry[]{ new RaftLogEntry( term, ContentGenerator.Content() ) }, -1 ), raft, Log() ) );
			  }
		 }

		 private class ContentGenerator
		 {
			  internal static int Count;

			  public static ReplicatedString Content()
			  {
					return new ReplicatedString( string.Format( "content#{0:D}", Count++ ) );
			  }
		 }

		 private Log Log()
		 {
			  return NullLog.Instance;
		 }
	}

}