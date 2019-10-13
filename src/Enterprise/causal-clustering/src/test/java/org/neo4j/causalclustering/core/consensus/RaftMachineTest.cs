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
namespace Neo4Net.causalclustering.core.consensus
{
	using Test = org.junit.Test;

	using InMemoryRaftLog = Neo4Net.causalclustering.core.consensus.log.InMemoryRaftLog;
	using RaftLog = Neo4Net.causalclustering.core.consensus.log.RaftLog;
	using RaftLogCursor = Neo4Net.causalclustering.core.consensus.log.RaftLogCursor;
	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using ConsecutiveInFlightCache = Neo4Net.causalclustering.core.consensus.log.cache.ConsecutiveInFlightCache;
	using InFlightCache = Neo4Net.causalclustering.core.consensus.log.cache.InFlightCache;
	using InFlightCacheMonitor = Neo4Net.causalclustering.core.consensus.log.cache.InFlightCacheMonitor;
	using MemberIdSet = Neo4Net.causalclustering.core.consensus.membership.MemberIdSet;
	using MembershipEntry = Neo4Net.causalclustering.core.consensus.membership.MembershipEntry;
	using OnDemandTimerService = Neo4Net.causalclustering.core.consensus.schedule.OnDemandTimerService;
	using RaftCoreState = Neo4Net.causalclustering.core.state.snapshot.RaftCoreState;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using RaftTestMemberSetBuilder = Neo4Net.causalclustering.identity.RaftTestMemberSetBuilder;
	using Neo4Net.causalclustering.messaging;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Clocks = Neo4Net.Time.Clocks;
	using FakeClock = Neo4Net.Time.FakeClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.RaftMachine.Timeouts.ELECTION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.TestMessageBuilders.appendEntriesRequest;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.TestMessageBuilders.voteRequest;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.TestMessageBuilders.voteResponse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.log.RaftLogHelper.readLogEntry;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.roles.Role.FOLLOWER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.identity.RaftTestMember.member;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.last;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;

	public class RaftMachineTest
	{
		 private readonly int _electionTimeout = 500;
		 private MemberId _myself = member( 0 );

		 /* A few members that we use at will in tests. */
		 private MemberId _member1 = member( 1 );
		 private MemberId _member2 = member( 2 );
		 private MemberId _member3 = member( 3 );
		 private MemberId _member4 = member( 4 );

		 private ReplicatedInteger _data1 = ReplicatedInteger.ValueOf( 1 );
		 private ReplicatedInteger _data2 = ReplicatedInteger.ValueOf( 2 );

		 private RaftLog _raftLog = new InMemoryRaftLog();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAlwaysStartAsFollower()
		 public virtual void ShouldAlwaysStartAsFollower()
		 {
			  // when
			  RaftMachine raft = ( new RaftMachineBuilder( _myself, 3, RaftTestMemberSetBuilder.INSTANCE ) ).build();

			  // then
			  assertEquals( FOLLOWER, raft.CurrentRole() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRequestVotesOnElectionTimeout() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRequestVotesOnElectionTimeout()
		 {
			  // Given
			  FakeClock fakeClock = Clocks.fakeClock();
			  OnDemandTimerService timerService = new OnDemandTimerService( fakeClock );
			  OutboundMessageCollector messages = new OutboundMessageCollector();

			  RaftMachine raft = ( new RaftMachineBuilder( _myself, 3, RaftTestMemberSetBuilder.INSTANCE ) ).timerService( timerService ).electionTimeout( _electionTimeout ).clock( fakeClock ).outbound( messages ).build();

			  raft.InstallCoreState( new RaftCoreState( new MembershipEntry( 0, asSet( _myself, _member1, _member2 ) ) ) );
			  raft.PostRecoveryActions();

			  // When
			  timerService.Invoke( ELECTION );

			  // Then
			  assertThat( messages.SentTo( _myself ).Count, equalTo( 0 ) );

			  assertThat( messages.SentTo( _member1 ).Count, equalTo( 1 ) );
			  assertThat( messages.SentTo( _member1 )[0], instanceOf( typeof( RaftMessages_Vote_Request ) ) );

			  assertThat( messages.SentTo( _member2 ).Count, equalTo( 1 ) );
			  assertThat( messages.SentTo( _member2 )[0], instanceOf( typeof( RaftMessages_Vote_Request ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBecomeLeaderInMajorityOf3() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBecomeLeaderInMajorityOf3()
		 {
			  // Given
			  FakeClock fakeClock = Clocks.fakeClock();
			  OnDemandTimerService timerService = new OnDemandTimerService( fakeClock );
			  RaftMachine raft = ( new RaftMachineBuilder( _myself, 3, RaftTestMemberSetBuilder.INSTANCE ) ).timerService( timerService ).clock( fakeClock ).build();

			  raft.InstallCoreState( new RaftCoreState( new MembershipEntry( 0, asSet( _myself, _member1, _member2 ) ) ) );
			  raft.PostRecoveryActions();

			  timerService.Invoke( ELECTION );
			  assertThat( raft.Leader, @is( false ) );

			  // When
			  raft.Handle( voteResponse().from(_member1).term(1).grant().build() );

			  // Then
			  assertThat( raft.Leader, @is( true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBecomeLeaderInMajorityOf5() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBecomeLeaderInMajorityOf5()
		 {
			  // Given
			  FakeClock fakeClock = Clocks.fakeClock();
			  OnDemandTimerService timerService = new OnDemandTimerService( fakeClock );
			  RaftMachine raft = ( new RaftMachineBuilder( _myself, 3, RaftTestMemberSetBuilder.INSTANCE ) ).timerService( timerService ).clock( fakeClock ).build();

			  raft.InstallCoreState( new RaftCoreState( new MembershipEntry( 0, asSet( _myself, _member1, _member2, _member3, _member4 ) ) ) );
			  raft.PostRecoveryActions();

			  timerService.Invoke( ELECTION );

			  raft.Handle( voteResponse().from(_member1).term(1).grant().build() );
			  assertThat( raft.Leader, @is( false ) );

			  // When
			  raft.Handle( voteResponse().from(_member2).term(1).grant().build() );

			  // Then
			  assertThat( raft.Leader, @is( true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBecomeLeaderOnMultipleVotesFromSameMember() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBecomeLeaderOnMultipleVotesFromSameMember()
		 {
			  // Given
			  FakeClock fakeClock = Clocks.fakeClock();
			  OnDemandTimerService timerService = new OnDemandTimerService( fakeClock );
			  RaftMachine raft = ( new RaftMachineBuilder( _myself, 3, RaftTestMemberSetBuilder.INSTANCE ) ).timerService( timerService ).clock( fakeClock ).build();

			  raft.InstallCoreState( new RaftCoreState( new MembershipEntry( 0, asSet( _myself, _member1, _member2, _member3, _member4 ) ) ) );
			  raft.PostRecoveryActions();

			  timerService.Invoke( ELECTION );

			  // When
			  raft.Handle( voteResponse().from(_member1).term(1).grant().build() );
			  raft.Handle( voteResponse().from(_member1).term(1).grant().build() );

			  // Then
			  assertThat( raft.Leader, @is( false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBecomeLeaderWhenVotingOnItself() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBecomeLeaderWhenVotingOnItself()
		 {
			  // Given
			  FakeClock fakeClock = Clocks.fakeClock();
			  OnDemandTimerService timerService = new OnDemandTimerService( fakeClock );
			  RaftMachine raft = ( new RaftMachineBuilder( _myself, 3, RaftTestMemberSetBuilder.INSTANCE ) ).timerService( timerService ).clock( fakeClock ).build();

			  raft.InstallCoreState( new RaftCoreState( new MembershipEntry( 0, asSet( _myself, _member1, _member2 ) ) ) );
			  raft.PostRecoveryActions();

			  timerService.Invoke( ELECTION );

			  // When
			  raft.Handle( voteResponse().from(_myself).term(1).grant().build() );

			  // Then
			  assertThat( raft.Leader, @is( false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBecomeLeaderWhenMembersVoteNo() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBecomeLeaderWhenMembersVoteNo()
		 {
			  // Given
			  FakeClock fakeClock = Clocks.fakeClock();
			  OnDemandTimerService timerService = new OnDemandTimerService( fakeClock );
			  RaftMachine raft = ( new RaftMachineBuilder( _myself, 3, RaftTestMemberSetBuilder.INSTANCE ) ).timerService( timerService ).clock( fakeClock ).build();

			  raft.InstallCoreState( new RaftCoreState( new MembershipEntry( 0, asSet( _myself, _member1, _member2 ) ) ) );
			  raft.PostRecoveryActions();

			  timerService.Invoke( ELECTION );

			  // When
			  raft.Handle( voteResponse().from(_member1).term(1).deny().build() );
			  raft.Handle( voteResponse().from(_member2).term(1).deny().build() );

			  // Then
			  assertThat( raft.Leader, @is( false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBecomeLeaderByVotesFromOldTerm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBecomeLeaderByVotesFromOldTerm()
		 {
			  // Given
			  FakeClock fakeClock = Clocks.fakeClock();
			  OnDemandTimerService timerService = new OnDemandTimerService( fakeClock );
			  RaftMachine raft = ( new RaftMachineBuilder( _myself, 3, RaftTestMemberSetBuilder.INSTANCE ) ).timerService( timerService ).clock( fakeClock ).build();

			  raft.InstallCoreState( new RaftCoreState( new MembershipEntry( 0, asSet( _myself, _member1, _member2 ) ) ) );
			  raft.PostRecoveryActions();

			  timerService.Invoke( ELECTION );
			  // When
			  raft.Handle( voteResponse().from(_member1).term(0).grant().build() );
			  raft.Handle( voteResponse().from(_member2).term(0).grant().build() );

			  // Then
			  assertThat( raft.Leader, @is( false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldVoteFalseForCandidateInOldTerm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldVoteFalseForCandidateInOldTerm()
		 {
			  // Given
			  FakeClock fakeClock = Clocks.fakeClock();
			  OnDemandTimerService timerService = new OnDemandTimerService( fakeClock );
			  OutboundMessageCollector messages = new OutboundMessageCollector();

			  RaftMachine raft = ( new RaftMachineBuilder( _myself, 3, RaftTestMemberSetBuilder.INSTANCE ) ).timerService( timerService ).clock( fakeClock ).outbound( messages ).build();

			  raft.InstallCoreState( new RaftCoreState( new MembershipEntry( 0, asSet( _myself, _member1, _member2 ) ) ) );
			  raft.PostRecoveryActions();

			  // When
			  raft.Handle( voteRequest().from(_member1).term(-1).candidate(_member1).lastLogIndex(0).lastLogTerm(-1).build() );

			  // Then
			  assertThat( messages.SentTo( _member1 ).Count, equalTo( 1 ) );
			  assertThat( messages.SentTo( _member1 ), hasItem( voteResponse().from(_myself).term(0).deny().build() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBecomeLeaderByVotesFromFutureTerm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBecomeLeaderByVotesFromFutureTerm()
		 {
			  // Given
			  FakeClock fakeClock = Clocks.fakeClock();
			  OnDemandTimerService timerService = new OnDemandTimerService( fakeClock );
			  RaftMachine raft = ( new RaftMachineBuilder( _myself, 3, RaftTestMemberSetBuilder.INSTANCE ) ).timerService( timerService ).clock( fakeClock ).build();

			  raft.InstallCoreState( new RaftCoreState( new MembershipEntry( 0, asSet( _myself, _member1, _member2 ) ) ) );
			  raft.PostRecoveryActions();

			  timerService.Invoke( ELECTION );

			  // When
			  raft.Handle( voteResponse().from(_member1).term(2).grant().build() );
			  raft.Handle( voteResponse().from(_member2).term(2).grant().build() );

			  assertThat( raft.Leader, @is( false ) );
			  assertEquals( raft.Term(), 2L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAppendNewLeaderBarrierAfterBecomingLeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAppendNewLeaderBarrierAfterBecomingLeader()
		 {
			  // Given
			  FakeClock fakeClock = Clocks.fakeClock();
			  OnDemandTimerService timerService = new OnDemandTimerService( fakeClock );
			  OutboundMessageCollector messages = new OutboundMessageCollector();

			  InMemoryRaftLog raftLog = new InMemoryRaftLog();
			  RaftMachine raft = ( new RaftMachineBuilder( _myself, 3, RaftTestMemberSetBuilder.INSTANCE ) ).timerService( timerService ).clock( fakeClock ).outbound( messages ).raftLog( raftLog ).build();

			  raft.InstallCoreState( new RaftCoreState( new MembershipEntry( 0, asSet( _myself, _member1, _member2 ) ) ) );
			  raft.PostRecoveryActions();

			  // When
			  timerService.Invoke( ELECTION );
			  raft.Handle( voteResponse().from(_member1).term(1).grant().build() );

			  // Then
			  assertEquals( new NewLeaderBarrier(), readLogEntry(raftLog, raftLog.AppendIndex()).content() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void leaderShouldSendHeartBeatsOnHeartbeatTimeout() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LeaderShouldSendHeartBeatsOnHeartbeatTimeout()
		 {
			  // Given
			  FakeClock fakeClock = Clocks.fakeClock();
			  OnDemandTimerService timerService = new OnDemandTimerService( fakeClock );
			  OutboundMessageCollector messages = new OutboundMessageCollector();

			  RaftMachine raft = ( new RaftMachineBuilder( _myself, 3, RaftTestMemberSetBuilder.INSTANCE ) ).timerService( timerService ).outbound( messages ).clock( fakeClock ).build();

			  raft.InstallCoreState( new RaftCoreState( new MembershipEntry( 0, asSet( _myself, _member1, _member2 ) ) ) );
			  raft.PostRecoveryActions();

			  timerService.Invoke( ELECTION );
			  raft.Handle( voteResponse().from(_member1).term(1).grant().build() );

			  // When
			  timerService.Invoke( RaftMachine.Timeouts.Heartbeat );

			  // Then
			  assertTrue( last( messages.SentTo( _member1 ) ) is RaftMessages_Heartbeat );
			  assertTrue( last( messages.SentTo( _member2 ) ) is RaftMessages_Heartbeat );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowExceptionIfReceivesClientRequestWithNoLeaderElected() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowExceptionIfReceivesClientRequestWithNoLeaderElected()
		 {
			  // Given
			  FakeClock fakeClock = Clocks.fakeClock();
			  OnDemandTimerService timerService = new OnDemandTimerService( fakeClock );

			  RaftMachine raft = ( new RaftMachineBuilder( _myself, 3, RaftTestMemberSetBuilder.INSTANCE ) ).timerService( timerService ).clock( fakeClock ).build();

			  raft.InstallCoreState( new RaftCoreState( new MembershipEntry( 0, asSet( _myself, _member1, _member2 ) ) ) );
			  raft.PostRecoveryActions();

			  try
			  {
					// When
					// There is no leader
					raft.Leader;
					fail( "Should have thrown exception" );
			  }
			  // Then
			  catch ( NoLeaderFoundException )
			  {
					// expected
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPersistAtSpecifiedLogIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPersistAtSpecifiedLogIndex()
		 {
			  // given
			  FakeClock fakeClock = Clocks.fakeClock();
			  OnDemandTimerService timerService = new OnDemandTimerService( fakeClock );
			  RaftMachine raft = ( new RaftMachineBuilder( _myself, 3, RaftTestMemberSetBuilder.INSTANCE ) ).timerService( timerService ).clock( fakeClock ).raftLog( _raftLog ).build();

			  _raftLog.append( new RaftLogEntry( 0, new MemberIdSet( asSet( _myself, _member1, _member2 ) ) ) );

			  // when
			  raft.Handle( appendEntriesRequest().from(_member1).prevLogIndex(0).prevLogTerm(0).leaderTerm(0).logEntry(new RaftLogEntry(0, _data1)).build() );
			  // then
			  assertEquals( 1, _raftLog.appendIndex() );
			  assertEquals( _data1, readLogEntry( _raftLog, 1 ).content() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void newMembersShouldBeIncludedInHeartbeatMessages() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NewMembersShouldBeIncludedInHeartbeatMessages()
		 {
			  // Given
			  DirectNetworking network = new DirectNetworking();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.identity.MemberId newMember = member(99);
			  MemberId newMember = member( 99 );
			  DirectNetworking.Inbound<RaftMessages_RaftMessage> newMemberInbound = new Neo4Net.causalclustering.core.consensus.DirectNetworking.Inbound<RaftMessages_RaftMessage>( network, newMember );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final OutboundMessageCollector messages = new OutboundMessageCollector();
			  OutboundMessageCollector messages = new OutboundMessageCollector();
			  newMemberInbound.RegisterHandler( ( Neo4Net.causalclustering.messaging.Inbound_MessageHandler<RaftMessages_RaftMessage> ) message => messages.send( newMember, message ) );

			  FakeClock fakeClock = Clocks.fakeClock();
			  OnDemandTimerService timerService = new OnDemandTimerService( fakeClock );
			  RaftMachine raft = ( new RaftMachineBuilder( _myself, 3, RaftTestMemberSetBuilder.INSTANCE ) ).timerService( timerService ).outbound( messages ).clock( fakeClock ).build();

			  raft.InstallCoreState( new RaftCoreState( new MembershipEntry( 0, asSet( _myself, _member1, _member2 ) ) ) );
			  raft.PostRecoveryActions();

			  // We make ourselves the leader
			  timerService.Invoke( ELECTION );
			  raft.Handle( voteResponse().from(_member1).term(1).grant().build() );

			  // When
			  raft.TargetMembershipSet = asSet( _myself, _member1, _member2, newMember );
			  network.ProcessMessages();

			  timerService.Invoke( RaftMachine.Timeouts.Heartbeat );
			  network.ProcessMessages();

			  // Then
			  assertEquals( typeof( RaftMessages_AppendEntries_Request ), messages.SentTo( newMember )[0].GetType() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMonitorLeaderNotFound() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMonitorLeaderNotFound()
		 {
			  // Given
			  FakeClock fakeClock = Clocks.fakeClock();
			  OnDemandTimerService timerService = new OnDemandTimerService( fakeClock );

			  RaftMachine raft = ( new RaftMachineBuilder( _myself, 3, RaftTestMemberSetBuilder.INSTANCE ) ).timerService( timerService ).build();

			  raft.InstallCoreState( new RaftCoreState( new MembershipEntry( 0, asSet( _myself, _member1, _member2 ) ) ) );

			  try
			  {
					// When
					// There is no leader
					raft.Leader;
					fail( "Should have thrown exception" );
			  }
			  // Then
			  catch ( NoLeaderFoundException )
			  {
					// expected
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCacheInFlightEntriesUntilAfterRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotCacheInFlightEntriesUntilAfterRecovery()
		 {
			  // given
			  FakeClock fakeClock = Clocks.fakeClock();
			  InFlightCache inFlightCache = new ConsecutiveInFlightCache( 10, 10000, InFlightCacheMonitor.VOID, false );
			  OnDemandTimerService timerService = new OnDemandTimerService( fakeClock );
			  RaftMachine raft = ( new RaftMachineBuilder( _myself, 3, RaftTestMemberSetBuilder.INSTANCE ) ).timerService( timerService ).clock( fakeClock ).raftLog( _raftLog ).inFlightCache( inFlightCache ).build();

			  _raftLog.append( new RaftLogEntry( 0, new MemberIdSet( asSet( _myself, _member1, _member2 ) ) ) );

			  // when
			  raft.Handle( appendEntriesRequest().from(_member1).prevLogIndex(0).prevLogTerm(0).leaderTerm(0).logEntry(new RaftLogEntry(0, _data1)).build() );

			  // then
			  assertEquals( _data1, readLogEntry( _raftLog, 1 ).content() );
			  assertNull( inFlightCache.Get( 1L ) );

			  // when
			  raft.PostRecoveryActions();
			  raft.Handle( appendEntriesRequest().from(_member1).prevLogIndex(1).prevLogTerm(0).leaderTerm(0).logEntry(new RaftLogEntry(0, _data2)).build() );

			  // then
			  assertEquals( _data2, readLogEntry( _raftLog, 2 ).content() );
			  assertEquals( _data2, inFlightCache.Get( 2L ).content() );
		 }

		 private class ExplodingRaftLog : RaftLog
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool StartExplodingConflict;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long append(org.neo4j.causalclustering.core.consensus.log.RaftLogEntry... entries) throws java.io.IOException
			  public override long Append( params RaftLogEntry[] entries )
			  {
					if ( StartExplodingConflict )
					{
						 throw new IOException( "Boom! append" );
					}
					else
					{
						 return 0;
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void truncate(long fromIndex) throws java.io.IOException
			  public override void Truncate( long fromIndex )
			  {
					throw new IOException( "Boom! truncate" );
			  }

			  public override long Prune( long safeIndex )
			  {
					return -1;
			  }

			  public override long AppendIndex()
			  {
					return -1;
			  }

			  public override long PrevIndex()
			  {
					return -1;
			  }

			  public override long ReadEntryTerm( long logIndex )
			  {
					return -1;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.causalclustering.core.consensus.log.RaftLogCursor getEntryCursor(long fromIndex) throws java.io.IOException
			  public override RaftLogCursor GetEntryCursor( long fromIndex )
			  {
					if ( StartExplodingConflict )
					{
						 throw new IOException( "Boom! entry cursor" );
					}
					else
					{
						 return RaftLogCursor.empty();
					}
			  }

			  public override long Skip( long index, long term )
			  {
					return -1;
			  }

			  public virtual void StartExploding()
			  {
					StartExplodingConflict = true;
			  }
		 }
	}

}