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
namespace Org.Neo4j.causalclustering.core.consensus.roles
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using MockitoJUnitRunner = org.mockito.junit.MockitoJUnitRunner;

	using InMemoryRaftLog = Org.Neo4j.causalclustering.core.consensus.log.InMemoryRaftLog;
	using RaftLog = Org.Neo4j.causalclustering.core.consensus.log.RaftLog;
	using RaftLogEntry = Org.Neo4j.causalclustering.core.consensus.log.RaftLogEntry;
	using ReadableRaftLog = Org.Neo4j.causalclustering.core.consensus.log.ReadableRaftLog;
	using AppendLogEntry = Org.Neo4j.causalclustering.core.consensus.outcome.AppendLogEntry;
	using BatchAppendLogEntries = Org.Neo4j.causalclustering.core.consensus.outcome.BatchAppendLogEntries;
	using Outcome = Org.Neo4j.causalclustering.core.consensus.outcome.Outcome;
	using ShipCommand = Org.Neo4j.causalclustering.core.consensus.outcome.ShipCommand;
	using FollowerState = Org.Neo4j.causalclustering.core.consensus.roles.follower.FollowerState;
	using Org.Neo4j.causalclustering.core.consensus.roles.follower;
	using RaftState = Org.Neo4j.causalclustering.core.consensus.state.RaftState;
	using ReadableRaftState = Org.Neo4j.causalclustering.core.consensus.state.ReadableRaftState;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.MessageUtils.messageFor;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.ReplicatedInteger.valueOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.TestMessageBuilders.appendEntriesResponse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.roles.Role.FOLLOWER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.roles.Role.LEADER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.state.RaftStateBuilder.raftState;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.identity.RaftTestMember.member;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.count;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(MockitoJUnitRunner.class) public class LeaderTest
	public class LeaderTest
	{
		 private MemberId _myself = member( 0 );

		 /* A few members that we use at will in tests. */
		 private MemberId _member1 = member( 1 );
		 private MemberId _member2 = member( 2 );

		 private LogProvider _logProvider = NullLogProvider.Instance;

		 private static readonly ReplicatedString _content = ReplicatedString.valueOf( "some-content-to-raft" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void leaderShouldNotRespondToSuccessResponseFromFollowerThatWillSoonUpToDateViaInFlightMessages() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LeaderShouldNotRespondToSuccessResponseFromFollowerThatWillSoonUpToDateViaInFlightMessages()
		 {
			  // given
			  /*
			   * A leader who
			   * - has an append index of 100
			   * - knows about instance 2
			   * - assumes that instance 2 is at an index less than 100 -say 84 but it has already been sent up to 100
			   */
			  Leader leader = new Leader();
			  MemberId instance2 = member( 2 );
			  FollowerState instance2State = CreateArtificialFollowerState( 84 );

			  ReadableRaftState state = mock( typeof( ReadableRaftState ) );

			  FollowerStates<MemberId> followerState = new FollowerStates<MemberId>();
			  followerState = new FollowerStates<MemberId>( followerState, instance2, instance2State );

			  ReadableRaftLog logMock = mock( typeof( ReadableRaftLog ) );
			  when( logMock.AppendIndex() ).thenReturn(100L);

			  when( state.CommitIndex() ).thenReturn(-1L);
			  when( state.EntryLog() ).thenReturn(logMock);
			  when( state.FollowerStates() ).thenReturn(followerState);
			  when( state.Term() ).thenReturn(4L); // both leader and follower are in the same term

			  // when
			  // that leader is asked to handle a response from that follower that says that the follower is up to date
			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Response response = appendEntriesResponse().success().matchIndex(90).term(4).from(instance2).build();

			  Outcome outcome = leader.Handle( response, state, mock( typeof( Log ) ) );

			  // then
			  // The leader should not be trying to send any messages to that instance
			  assertTrue( outcome.OutgoingMessages.Count == 0 );
			  // And the follower state should be updated
			  FollowerStates<MemberId> leadersViewOfFollowerStates = outcome.FollowerStates;
			  assertEquals( 90, leadersViewOfFollowerStates.Get( instance2 ).MatchIndex );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void leaderShouldNotRespondToSuccessResponseThatIndicatesUpToDateFollower() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LeaderShouldNotRespondToSuccessResponseThatIndicatesUpToDateFollower()
		 {
			  // given
			  /*
			   * A leader who
			   * - has an append index of 100
			   * - knows about instance 2
			   * - assumes that instance 2 is at an index less than 100 -say 84
			   */
			  Leader leader = new Leader();
			  MemberId instance2 = member( 2 );
			  FollowerState instance2State = CreateArtificialFollowerState( 84 );

			  ReadableRaftState state = mock( typeof( ReadableRaftState ) );

			  FollowerStates<MemberId> followerState = new FollowerStates<MemberId>();
			  followerState = new FollowerStates<MemberId>( followerState, instance2, instance2State );

			  ReadableRaftLog logMock = mock( typeof( ReadableRaftLog ) );
			  when( logMock.AppendIndex() ).thenReturn(100L);

			  when( state.CommitIndex() ).thenReturn(-1L);
			  when( state.EntryLog() ).thenReturn(logMock);
			  when( state.FollowerStates() ).thenReturn(followerState);
			  when( state.Term() ).thenReturn(4L); // both leader and follower are in the same term

			  // when
			  // that leader is asked to handle a response from that follower that says that the follower is up to date
			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Response response = appendEntriesResponse().success().matchIndex(100).term(4).from(instance2).build();

			  Outcome outcome = leader.Handle( response, state, mock( typeof( Log ) ) );

			  // then
			  // The leader should not be trying to send any messages to that instance
			  assertTrue( outcome.OutgoingMessages.Count == 0 );
			  // And the follower state should be updated
			  FollowerStates<MemberId> updatedFollowerStates = outcome.FollowerStates;
			  assertEquals( 100, updatedFollowerStates.Get( instance2 ).MatchIndex );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void leaderShouldRespondToSuccessResponseThatIndicatesLaggingFollowerWithJustWhatItsMissing() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LeaderShouldRespondToSuccessResponseThatIndicatesLaggingFollowerWithJustWhatItsMissing()
		 {
			  // given
			  /*
			   * A leader who
			   * - has an append index of 100
			   * - knows about instance 2
			   * - assumes that instance 2 is at an index less than 100 -say 50
			   */
			  Leader leader = new Leader();
			  MemberId instance2 = member( 2 );
			  FollowerState instance2State = CreateArtificialFollowerState( 50 );

			  ReadableRaftState state = mock( typeof( ReadableRaftState ) );

			  FollowerStates<MemberId> followerState = new FollowerStates<MemberId>();
			  followerState = new FollowerStates<MemberId>( followerState, instance2, instance2State );

			  ReadableRaftLog logMock = mock( typeof( ReadableRaftLog ) );
			  when( logMock.AppendIndex() ).thenReturn(100L);
			  // with commit requests in this test

			  when( state.CommitIndex() ).thenReturn(-1L);
			  when( state.EntryLog() ).thenReturn(logMock);
			  when( state.FollowerStates() ).thenReturn(followerState);
			  when( state.Term() ).thenReturn(231L); // both leader and follower are in the same term

			  // when that leader is asked to handle a response from that follower that says that the follower is still
			  // missing things
			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Response response = appendEntriesResponse().success().matchIndex(89).term(231).from(instance2).build();

			  Outcome outcome = leader.Handle( response, state, mock( typeof( Log ) ) );

			  // then
			  int matchCount = 0;
			  foreach ( ShipCommand shipCommand in outcome.ShipCommands )
			  {
					if ( shipCommand is ShipCommand.Match )
					{
						 matchCount++;
					}
			  }

			  assertThat( matchCount, greaterThan( 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void leaderShouldIgnoreSuccessResponseThatIndicatesLaggingWhileLocalStateIndicatesFollowerIsCaughtUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LeaderShouldIgnoreSuccessResponseThatIndicatesLaggingWhileLocalStateIndicatesFollowerIsCaughtUp()
		 {
			  // given
			  /*
			   * A leader who
			   * - has an append index of 100
			   * - knows about instance 2
			   * - assumes that instance 2 is fully caught up
			   */
			  Leader leader = new Leader();
			  MemberId instance2 = member( 2 );
			  int j = 100;
			  FollowerState instance2State = CreateArtificialFollowerState( j );

			  ReadableRaftState state = mock( typeof( ReadableRaftState ) );

			  FollowerStates<MemberId> followerState = new FollowerStates<MemberId>();
			  followerState = new FollowerStates<MemberId>( followerState, instance2, instance2State );

			  ReadableRaftLog logMock = mock( typeof( ReadableRaftLog ) );
			  when( logMock.AppendIndex() ).thenReturn(100L);
			  //  with commit requests in this test

			  when( state.CommitIndex() ).thenReturn(-1L);
			  when( state.EntryLog() ).thenReturn(logMock);
			  when( state.FollowerStates() ).thenReturn(followerState);
			  when( state.Term() ).thenReturn(4L); // both leader and follower are in the same term

			  // when that leader is asked to handle a response from that follower that says that the follower is still
			  // missing things
			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Response response = appendEntriesResponse().success().matchIndex(80).term(4).from(instance2).build();

			  Outcome outcome = leader.Handle( response, state, mock( typeof( Log ) ) );

			  // then the leader should not send anything, since this is a delayed, out of order response to a previous append
			  // request
			  assertTrue( outcome.OutgoingMessages.Count == 0 );
			  // The follower state should not be touched
			  FollowerStates<MemberId> updatedFollowerStates = outcome.FollowerStates;
			  assertEquals( 100, updatedFollowerStates.Get( instance2 ).MatchIndex );
		 }

		 private static FollowerState CreateArtificialFollowerState( long matchIndex )
		 {
			  return ( new FollowerState() ).onSuccessResponse(matchIndex);
		 }

		 // TODO: rethink this test, it does too much
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void leaderShouldSpawnMismatchCommandOnFailure() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LeaderShouldSpawnMismatchCommandOnFailure()
		 {
			  // given
			  /*
			   * A leader who
			   * - has an append index of 100
			   * - knows about instance 2
			   * - assumes that instance 2 is fully caught up
			   */
			  Leader leader = new Leader();
			  MemberId instance2 = member( 2 );
			  FollowerState instance2State = CreateArtificialFollowerState( 100 );

			  ReadableRaftState state = mock( typeof( ReadableRaftState ) );

			  FollowerStates<MemberId> followerState = new FollowerStates<MemberId>();
			  followerState = new FollowerStates<MemberId>( followerState, instance2, instance2State );

			  RaftLog log = new InMemoryRaftLog();
			  for ( int i = 0; i <= 100; i++ )
			  {
					log.Append( new RaftLogEntry( 0, valueOf( i ) ) );
			  }

			  when( state.CommitIndex() ).thenReturn(-1L);
			  when( state.EntryLog() ).thenReturn(log);
			  when( state.FollowerStates() ).thenReturn(followerState);
			  when( state.Term() ).thenReturn(4L); // both leader and follower are in the same term

			  // when
			  // that leader is asked to handle a response from that follower that says that the follower is still missing
			  // things
			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Response response = appendEntriesResponse().failure().appendIndex(0).matchIndex(-1).term(4).from(instance2).build();

			  Outcome outcome = leader.Handle( response, state, mock( typeof( Log ) ) );

			  // then
			  int mismatchCount = 0;
			  foreach ( ShipCommand shipCommand in outcome.ShipCommands )
			  {
					if ( shipCommand is ShipCommand.Mismatch )
					{
						 mismatchCount++;
					}
			  }

			  assertThat( mismatchCount, greaterThan( 0 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendCompactionInfoIfFailureWithNoEarlierEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendCompactionInfoIfFailureWithNoEarlierEntries()
		 {
			  // given
			  Leader leader = new Leader();
			  long term = 1;
			  long leaderPrevIndex = 3;
			  long followerIndex = leaderPrevIndex - 1;

			  InMemoryRaftLog raftLog = new InMemoryRaftLog();
			  raftLog.Skip( leaderPrevIndex, term );

			  RaftState state = raftState().term(term).entryLog(raftLog).build();

			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Response incomingResponse = appendEntriesResponse().failure().term(term).appendIndex(followerIndex).from(_member1).build();

			  // when
			  Outcome outcome = leader.Handle( incomingResponse, state, Log() );

			  // then
			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_RaftMessage outgoingMessage = messageFor( outcome, _member1 );
			  assertThat( outgoingMessage, instanceOf( typeof( Org.Neo4j.causalclustering.core.consensus.RaftMessages_LogCompactionInfo ) ) );

			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_LogCompactionInfo typedOutgoingMessage = ( Org.Neo4j.causalclustering.core.consensus.RaftMessages_LogCompactionInfo ) outgoingMessage;
			  assertThat( typedOutgoingMessage.PrevIndex(), equalTo(leaderPrevIndex) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreAppendResponsesFromOldTerms() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreAppendResponsesFromOldTerms()
		 {
			  // given
			  Leader leader = new Leader();
			  long leaderTerm = 5;
			  long followerTerm = 3;

			  RaftState state = raftState().term(leaderTerm).build();

						 Org.Neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Response incomingResponse = appendEntriesResponse().failure().term(followerTerm).from(_member1).build();

			  // when
			  Outcome outcome = leader.Handle( incomingResponse, state, Log() );

			  // then
			  assertThat( outcome.Term, equalTo( leaderTerm ) );
			  assertThat( outcome.Role, equalTo( LEADER ) );

			  assertThat( outcome.OutgoingMessages, empty() );
			  assertThat( outcome.ShipCommands, empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void leaderShouldRejectAppendEntriesResponseWithNewerTermAndBecomeAFollower() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LeaderShouldRejectAppendEntriesResponseWithNewerTermAndBecomeAFollower()
		 {
			  // given
			  RaftState state = raftState().myself(_myself).build();

			  Leader leader = new Leader();

			  // when
			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Response message = appendEntriesResponse().from(_member1).term(state.Term() + 1).build();
			  Outcome outcome = leader.Handle( message, state, Log() );

			  // then
			  assertEquals( 0, count( outcome.OutgoingMessages ) );
			  assertEquals( FOLLOWER, outcome.Role );
			  assertEquals( 0, count( outcome.LogCommands ) );
			  assertEquals( state.Term() + 1, outcome.Term );
		 }

		 // TODO: test that shows we don't commit for previous terms

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void leaderShouldSendHeartbeatsToAllClusterMembersOnReceiptOfHeartbeatTick() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LeaderShouldSendHeartbeatsToAllClusterMembersOnReceiptOfHeartbeatTick()
		 {
			  // given
			  RaftState state = raftState().votingMembers(_myself, _member1, _member2).replicationMembers(_myself, _member1, _member2).build();

			  Leader leader = new Leader();
			  leader.Handle( new Org.Neo4j.causalclustering.core.consensus.RaftMessages_HeartbeatResponse( _member1 ), state, Log() ); // make sure it has quorum.

			  // when
			  Outcome outcome = leader.Handle( new RaftMessages_Timeout_Heartbeat( _myself ), state, Log() );

			  // then
			  assertTrue( messageFor( outcome, _member1 ) is Org.Neo4j.causalclustering.core.consensus.RaftMessages_Heartbeat );
			  assertTrue( messageFor( outcome, _member2 ) is Org.Neo4j.causalclustering.core.consensus.RaftMessages_Heartbeat );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void leaderShouldStepDownWhenLackingHeartbeatResponses() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LeaderShouldStepDownWhenLackingHeartbeatResponses()
		 {
			  // given
			  RaftState state = raftState().votingMembers(asSet(_myself, _member1, _member2)).leader(_myself).build();

			  Leader leader = new Leader();
			  leader.Handle( new Org.Neo4j.causalclustering.core.consensus.RaftMessages_Timeout_Election( _myself ), state, Log() );

			  // when
			  Outcome outcome = leader.Handle( new Org.Neo4j.causalclustering.core.consensus.RaftMessages_Timeout_Election( _myself ), state, Log() );

			  // then
			  assertThat( outcome.Role, not( LEADER ) );
			  assertNull( outcome.Leader );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void leaderShouldNotStepDownWhenReceivedQuorumOfHeartbeatResponses() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LeaderShouldNotStepDownWhenReceivedQuorumOfHeartbeatResponses()
		 {
			  // given
			  RaftState state = raftState().votingMembers(asSet(_myself, _member1, _member2)).build();

			  Leader leader = new Leader();

			  // when
			  Outcome outcome = leader.Handle( new Org.Neo4j.causalclustering.core.consensus.RaftMessages_HeartbeatResponse( _member1 ), state, Log() );
			  state.Update( outcome );

			  // we now have quorum and should not step down
			  outcome = leader.Handle( new Org.Neo4j.causalclustering.core.consensus.RaftMessages_Timeout_Election( _myself ), state, Log() );

			  // then
			  assertThat( outcome.Role, @is( LEADER ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void oldHeartbeatResponseShouldNotPreventStepdown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void OldHeartbeatResponseShouldNotPreventStepdown()
		 {
			  // given
			  RaftState state = raftState().votingMembers(asSet(_myself, _member1, _member2)).build();

			  Leader leader = new Leader();

			  Outcome outcome = leader.Handle( new Org.Neo4j.causalclustering.core.consensus.RaftMessages_HeartbeatResponse( _member1 ), state, Log() );
			  state.Update( outcome );

			  outcome = leader.Handle( new Org.Neo4j.causalclustering.core.consensus.RaftMessages_Timeout_Election( _myself ), state, Log() );
			  state.Update( outcome );

			  assertThat( outcome.Role, @is( LEADER ) );

			  // when
			  outcome = leader.Handle( new Org.Neo4j.causalclustering.core.consensus.RaftMessages_Timeout_Election( _myself ), state, Log() );

			  // then
			  assertThat( outcome.Role, @is( FOLLOWER ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void leaderShouldDecideToAppendToItsLogAndSendAppendEntriesMessageOnReceiptOfClientProposal() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LeaderShouldDecideToAppendToItsLogAndSendAppendEntriesMessageOnReceiptOfClientProposal()
		 {
			  // given
			  RaftState state = raftState().votingMembers(asSet(_myself, _member1, _member2)).build();

			  Leader leader = new Leader();

			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_NewEntry_Request newEntryRequest = new Org.Neo4j.causalclustering.core.consensus.RaftMessages_NewEntry_Request( member( 9 ), _content );

			  // when
			  Outcome outcome = leader.Handle( newEntryRequest, state, Log() );
			  //state.update( outcome );

			  // then
			  AppendLogEntry logCommand = ( AppendLogEntry ) single( outcome.LogCommands );
			  assertEquals( 0, logCommand.Index );
			  assertEquals( 0, logCommand.Entry.term() );

			  ShipCommand.NewEntries shipCommand = ( ShipCommand.NewEntries ) single( outcome.ShipCommands );

			  assertEquals( shipCommand, new ShipCommand.NewEntries( -1, -1, new RaftLogEntry[]{ new RaftLogEntry( 0, _content ) } ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void leaderShouldHandleBatch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LeaderShouldHandleBatch()
		 {
			  // given
			  RaftState state = raftState().votingMembers(asSet(_myself, _member1, _member2)).build();

			  Leader leader = new Leader();

			  const int batchSize = 3;
			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_NewEntry_BatchRequest batchRequest = new Org.Neo4j.causalclustering.core.consensus.RaftMessages_NewEntry_BatchRequest( new IList<Org.Neo4j.causalclustering.core.replication.ReplicatedContent> { valueOf( 0 ), valueOf( 1 ), valueOf( 2 ) } );

			  // when
			  Outcome outcome = leader.Handle( batchRequest, state, Log() );

			  // then
			  BatchAppendLogEntries logCommand = ( BatchAppendLogEntries ) single( outcome.LogCommands );

			  assertEquals( 0, logCommand.BaseIndex );
			  for ( int i = 0; i < batchSize; i++ )
			  {
					assertEquals( 0, logCommand.Entries[i].term() );
					assertEquals( i, ( ( ReplicatedInteger ) logCommand.Entries[i].content() ).get() );
			  }

			  ShipCommand.NewEntries shipCommand = ( ShipCommand.NewEntries ) single( outcome.ShipCommands );

			  assertEquals(shipCommand, new ShipCommand.NewEntries(-1, -1, new RaftLogEntry[]
			  {
				  new RaftLogEntry( 0, valueOf( 0 ) ),
				  new RaftLogEntry( 0, valueOf( 1 ) ),
				  new RaftLogEntry( 0, valueOf( 2 ) )
			  }));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void leaderShouldCommitOnMajorityResponse() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LeaderShouldCommitOnMajorityResponse()
		 {
			  // given
			  InMemoryRaftLog raftLog = new InMemoryRaftLog();
			  raftLog.Append( new RaftLogEntry( 0, new ReplicatedString( "lalalala" ) ) );

			  RaftState state = raftState().votingMembers(_member1, _member2).term(0).lastLogIndexBeforeWeBecameLeader(-1).leader(_myself).leaderCommit(-1).entryLog(raftLog).messagesSentToFollower(_member1, raftLog.AppendIndex() + 1).messagesSentToFollower(_member2, raftLog.AppendIndex() + 1).build();

			  Leader leader = new Leader();

			  // when a single instance responds (plus self == 2 out of 3 instances)
			  Outcome outcome = leader.Handle( new Org.Neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Response( _member1, 0, true, 0, 0 ), state, Log() );

			  // then
			  assertEquals( 0L, outcome.CommitIndex );
			  assertEquals( 0L, outcome.LeaderCommit );
		 }

		 // TODO move this someplace else, since log no longer holds the commit
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void leaderShouldCommitAllPreviouslyAppendedEntriesWhenCommittingLaterEntryInSameTerm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LeaderShouldCommitAllPreviouslyAppendedEntriesWhenCommittingLaterEntryInSameTerm()
		 {
			  // given
			  InMemoryRaftLog raftLog = new InMemoryRaftLog();
			  raftLog.Append( new RaftLogEntry( 0, new ReplicatedString( "first!" ) ) );
			  raftLog.Append( new RaftLogEntry( 0, new ReplicatedString( "second" ) ) );
			  raftLog.Append( new RaftLogEntry( 0, new ReplicatedString( "third" ) ) );

			  RaftState state = raftState().votingMembers(_myself, _member1, _member2).term(0).entryLog(raftLog).messagesSentToFollower(_member1, raftLog.AppendIndex() + 1).messagesSentToFollower(_member2, raftLog.AppendIndex() + 1).build();

			  Leader leader = new Leader();

			  // when
			  Outcome outcome = leader.Handle( new Org.Neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Response( _member1, 0, true, 2, 2 ), state, Log() );

			  state.Update( outcome );

			  // then
			  assertEquals( 2, state.CommitIndex() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendNegativeResponseForVoteRequestFromTermNotGreaterThanLeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendNegativeResponseForVoteRequestFromTermNotGreaterThanLeader()
		 {
			  // given
			  long leaderTerm = 5;
			  long leaderCommitIndex = 10;
			  long rivalTerm = leaderTerm - 1;

			  Leader leader = new Leader();
			  RaftState state = raftState().term(leaderTerm).commitIndex(leaderCommitIndex).build();

			  // when
			  Outcome outcome = leader.Handle( new Org.Neo4j.causalclustering.core.consensus.RaftMessages_Vote_Request( _member1, rivalTerm, _member1, leaderCommitIndex, leaderTerm ), state, Log() );

			  // then
			  assertThat( outcome.Role, equalTo( LEADER ) );
			  assertThat( outcome.Term, equalTo( leaderTerm ) );

			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_RaftMessage response = messageFor( outcome, _member1 );
			  assertThat( response, instanceOf( typeof( Org.Neo4j.causalclustering.core.consensus.RaftMessages_Vote_Response ) ) );
			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_Vote_Response typedResponse = ( Org.Neo4j.causalclustering.core.consensus.RaftMessages_Vote_Response ) response;
			  assertThat( typedResponse.VoteGranted(), equalTo(false) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStepDownIfReceiveVoteRequestFromGreaterTermThanLeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStepDownIfReceiveVoteRequestFromGreaterTermThanLeader()
		 {
			  // given
			  long leaderTerm = 1;
			  long leaderCommitIndex = 10;
			  long rivalTerm = leaderTerm + 1;

			  Leader leader = new Leader();
			  RaftState state = raftState().term(leaderTerm).commitIndex(leaderCommitIndex).build();

			  // when
			  Outcome outcome = leader.Handle( new Org.Neo4j.causalclustering.core.consensus.RaftMessages_Vote_Request( _member1, rivalTerm, _member1, leaderCommitIndex, leaderTerm ), state, Log() );

			  // then
			  assertThat( outcome.Role, equalTo( FOLLOWER ) );
			  assertThat( outcome.Leader, nullValue() );
			  assertThat( outcome.Term, equalTo( rivalTerm ) );

			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_RaftMessage response = messageFor( outcome, _member1 );
			  assertThat( response, instanceOf( typeof( Org.Neo4j.causalclustering.core.consensus.RaftMessages_Vote_Response ) ) );
			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_Vote_Response typedResponse = ( Org.Neo4j.causalclustering.core.consensus.RaftMessages_Vote_Response ) response;
			  assertThat( typedResponse.VoteGranted(), equalTo(true) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreHeartbeatFromOlderTerm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreHeartbeatFromOlderTerm()
		 {
			  // given
			  long leaderTerm = 5;
			  long leaderCommitIndex = 10;
			  long rivalTerm = leaderTerm - 1;

			  Leader leader = new Leader();
			  RaftState state = raftState().term(leaderTerm).commitIndex(leaderCommitIndex).build();

			  // when
			  Outcome outcome = leader.Handle( new Org.Neo4j.causalclustering.core.consensus.RaftMessages_Heartbeat( _member1, rivalTerm, leaderCommitIndex, leaderTerm ), state, Log() );

			  // then
			  assertThat( outcome.Role, equalTo( LEADER ) );
			  assertThat( outcome.Term, equalTo( leaderTerm ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStepDownIfHeartbeatReceivedWithGreaterOrEqualTerm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStepDownIfHeartbeatReceivedWithGreaterOrEqualTerm()
		 {
			  // given
			  long leaderTerm = 1;
			  long leaderCommitIndex = 10;
			  long rivalTerm = leaderTerm + 1;

			  Leader leader = new Leader();
			  RaftState state = raftState().term(leaderTerm).commitIndex(leaderCommitIndex).build();

			  // when
			  Outcome outcome = leader.Handle( new Org.Neo4j.causalclustering.core.consensus.RaftMessages_Heartbeat( _member1, rivalTerm, leaderCommitIndex, leaderTerm ), state, Log() );

			  // then
			  assertThat( outcome.Role, equalTo( FOLLOWER ) );
			  assertThat( outcome.Leader, equalTo( _member1 ) );
			  assertThat( outcome.Term, equalTo( rivalTerm ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondNegativelyToAppendEntriesRequestFromEarlierTerm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondNegativelyToAppendEntriesRequestFromEarlierTerm()
		 {
			  // given
			  long leaderTerm = 5;
			  long leaderCommitIndex = 10;
			  long rivalTerm = leaderTerm - 1;
			  long logIndex = 20;
			  RaftLogEntry[] entries = new RaftLogEntry[] { new RaftLogEntry( rivalTerm, ReplicatedInteger.valueOf( 99 ) ) };

			  Leader leader = new Leader();
			  RaftState state = raftState().term(leaderTerm).commitIndex(leaderCommitIndex).build();

			  // when
			  Outcome outcome = leader.Handle( new Org.Neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Request( _member1, rivalTerm, logIndex, leaderTerm, entries, leaderCommitIndex ), state, Log() );

			  // then
			  assertThat( outcome.Role, equalTo( LEADER ) );
			  assertThat( outcome.Term, equalTo( leaderTerm ) );

			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_RaftMessage response = messageFor( outcome, _member1 );
			  assertThat( response, instanceOf( typeof( Org.Neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Response ) ) );
			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Response typedResponse = ( Org.Neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Response ) response;
			  assertThat( typedResponse.Term(), equalTo(leaderTerm) );
			  assertThat( typedResponse.Success(), equalTo(false) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStepDownIfAppendEntriesRequestFromLaterTerm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStepDownIfAppendEntriesRequestFromLaterTerm()
		 {
			  // given
			  long leaderTerm = 1;
			  long leaderCommitIndex = 10;
			  long rivalTerm = leaderTerm + 1;
			  long logIndex = 20;
			  RaftLogEntry[] entries = new RaftLogEntry[] { new RaftLogEntry( rivalTerm, ReplicatedInteger.valueOf( 99 ) ) };

			  Leader leader = new Leader();
			  RaftState state = raftState().term(leaderTerm).commitIndex(leaderCommitIndex).build();

			  // when
			  Outcome outcome = leader.Handle( new Org.Neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Request( _member1, rivalTerm, logIndex, leaderTerm, entries, leaderCommitIndex ), state, Log() );

			  // then
			  assertThat( outcome.Role, equalTo( FOLLOWER ) );
			  assertThat( outcome.Leader, equalTo( _member1 ) );
			  assertThat( outcome.Term, equalTo( rivalTerm ) );

			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_RaftMessage response = messageFor( outcome, _member1 );
			  assertThat( response, instanceOf( typeof( Org.Neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Response ) ) );
			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Response typedResponse = ( Org.Neo4j.causalclustering.core.consensus.RaftMessages_AppendEntries_Response ) response;
			  assertThat( typedResponse.Term(), equalTo(rivalTerm) );
			  // Not checking success or failure of append
		 }

		 private Log Log()
		 {
			  return _logProvider.getLog( this.GetType() );
		 }
	}

}