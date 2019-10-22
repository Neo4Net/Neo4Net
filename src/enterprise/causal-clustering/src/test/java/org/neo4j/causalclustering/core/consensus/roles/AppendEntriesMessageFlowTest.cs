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
namespace Neo4Net.causalclustering.core.consensus.roles
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using InOrder = org.mockito.InOrder;
	using Mock = org.mockito.Mock;
	using MockitoJUnitRunner = org.mockito.junit.MockitoJUnitRunner;

	using InMemoryRaftLog = Neo4Net.causalclustering.core.consensus.log.InMemoryRaftLog;
	using RaftLog = Neo4Net.causalclustering.core.consensus.log.RaftLog;
	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using RaftTestGroup = Neo4Net.causalclustering.core.consensus.membership.RaftTestGroup;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using RaftTestMemberSetBuilder = Neo4Net.causalclustering.identity.RaftTestMemberSetBuilder;
	using Neo4Net.causalclustering.messaging;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.same;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.consensus.TestMessageBuilders.appendEntriesRequest;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.consensus.TestMessageBuilders.appendEntriesResponse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.identity.RaftTestMember.member;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(MockitoJUnitRunner.class) public class AppendEntriesMessageFlowTest
	public class AppendEntriesMessageFlowTest
	{
		 private MemberId _myself = member( 0 );
		 private MemberId _otherMember = member( 1 );

		 private ReplicatedInteger _data = ReplicatedInteger.valueOf( 1 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock private org.Neo4Net.causalclustering.messaging.Outbound<org.Neo4Net.causalclustering.identity.MemberId, org.Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> outbound;
		 private Outbound<MemberId, Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> _outbound;

		 internal virtual ReplicatedInteger Data( int value )
		 {
			  return ReplicatedInteger.valueOf( value );
		 }

		 private RaftMachine _raft;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  // given
			  RaftLog raftLog = new InMemoryRaftLog();
			  raftLog.Append( new RaftLogEntry( 0, new RaftTestGroup( 0 ) ) );

			  _raft = ( new RaftMachineBuilder( _myself, 3, RaftTestMemberSetBuilder.INSTANCE ) ).raftLog( raftLog ).outbound( _outbound ).build();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnFalseOnAppendRequestFromOlderTerm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnFalseOnAppendRequestFromOlderTerm()
		 {
			  // when
			  _raft.handle( appendEntriesRequest().from(_otherMember).leaderTerm(-1).prevLogIndex(0).prevLogTerm(0).leaderCommit(0).build() );

			  // then
			  verify( _outbound ).send( same( _otherMember ), eq( appendEntriesResponse().from(_myself).term(0).appendIndex(0).matchIndex(-1).failure().build() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnTrueOnAppendRequestWithFirstLogEntry() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnTrueOnAppendRequestWithFirstLogEntry()
		 {
			  // when
			  _raft.handle( appendEntriesRequest().from(_otherMember).leaderTerm(1).prevLogIndex(0).prevLogTerm(0).logEntry(new RaftLogEntry(1, _data)).leaderCommit(-1).build() );

			  // then
			  verify( _outbound ).send( same( _otherMember ), eq( appendEntriesResponse().appendIndex(1).matchIndex(1).from(_myself).term(1).success().build() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnFalseOnAppendRequestWhenPrevLogEntryNotMatched() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnFalseOnAppendRequestWhenPrevLogEntryNotMatched()
		 {
			  // when
			  _raft.handle( appendEntriesRequest().from(_otherMember).leaderTerm(0).prevLogIndex(0).prevLogTerm(1).logEntry(new RaftLogEntry(0, _data)).build() );

			  // then
			  verify( _outbound ).send( same( _otherMember ), eq( appendEntriesResponse().matchIndex(-1).appendIndex(0).from(_myself).term(0).failure().build() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptSequenceOfAppendEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcceptSequenceOfAppendEntries()
		 {
			  // when
			  _raft.handle( appendEntriesRequest().from(_otherMember).leaderTerm(0).prevLogIndex(0).prevLogTerm(0).logEntry(new RaftLogEntry(0, Data(1))).leaderCommit(-1).build() );
			  _raft.handle( appendEntriesRequest().from(_otherMember).leaderTerm(0).prevLogIndex(1).prevLogTerm(0).logEntry(new RaftLogEntry(0, Data(2))).leaderCommit(-1).build() );
			  _raft.handle( appendEntriesRequest().from(_otherMember).leaderTerm(0).prevLogIndex(2).prevLogTerm(0).logEntry(new RaftLogEntry(0, Data(3))).leaderCommit(0).build() );

			  _raft.handle( appendEntriesRequest().from(_otherMember).leaderTerm(1).prevLogIndex(3).prevLogTerm(0).logEntry(new RaftLogEntry(1, Data(4))).leaderCommit(1).build() );
			  _raft.handle( appendEntriesRequest().from(_otherMember).leaderTerm(1).prevLogIndex(4).prevLogTerm(1).logEntry(new RaftLogEntry(1, Data(5))).leaderCommit(2).build() );
			  _raft.handle( appendEntriesRequest().from(_otherMember).leaderTerm(1).prevLogIndex(5).prevLogTerm(1).logEntry(new RaftLogEntry(1, Data(6))).leaderCommit(4).build() );

			  // then
			  InOrder invocationOrder = inOrder( _outbound );
			  invocationOrder.verify( _outbound, times( 1 ) ).send( same( _otherMember ), eq( appendEntriesResponse().from(_myself).term(0).appendIndex(1).matchIndex(1).success().build() ) );
			  invocationOrder.verify( _outbound, times( 1 ) ).send( same( _otherMember ), eq( appendEntriesResponse().from(_myself).term(0).appendIndex(2).matchIndex(2).success().build() ) );
			  invocationOrder.verify( _outbound, times( 1 ) ).send( same( _otherMember ), eq( appendEntriesResponse().from(_myself).term(0).appendIndex(3).matchIndex(3).success().build() ) );

			  invocationOrder.verify( _outbound, times( 1 ) ).send( same( _otherMember ), eq( appendEntriesResponse().from(_myself).term(1).appendIndex(4).matchIndex(4).success().build() ) );
			  invocationOrder.verify( _outbound, times( 1 ) ).send( same( _otherMember ), eq( appendEntriesResponse().from(_myself).term(1).appendIndex(5).matchIndex(5).success().build() ) );
			  invocationOrder.verify( _outbound, times( 1 ) ).send( same( _otherMember ), eq( appendEntriesResponse().from(_myself).term(1).appendIndex(6).matchIndex(6).success().build() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnFalseIfLogHistoryDoesNotMatch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnFalseIfLogHistoryDoesNotMatch()
		 {
			  _raft.handle( appendEntriesRequest().from(_otherMember).leaderTerm(0).prevLogIndex(0).prevLogTerm(0).logEntry(new RaftLogEntry(0, Data(1))).build() );
			  _raft.handle( appendEntriesRequest().from(_otherMember).leaderTerm(0).prevLogIndex(1).prevLogTerm(0).logEntry(new RaftLogEntry(0, Data(2))).build() );
			  _raft.handle( appendEntriesRequest().from(_otherMember).leaderTerm(0).prevLogIndex(2).prevLogTerm(0).logEntry(new RaftLogEntry(0, Data(3))).build() ); // will conflict

			  // when
			  _raft.handle( appendEntriesRequest().from(_otherMember).leaderTerm(2).prevLogIndex(3).prevLogTerm(1).logEntry(new RaftLogEntry(2, Data(4))).build() ); // should reply false because of prevLogTerm

			  // then
			  InOrder invocationOrder = inOrder( _outbound );
			  invocationOrder.verify( _outbound, times( 1 ) ).send( same( _otherMember ), eq( appendEntriesResponse().from(_myself).term(0).matchIndex(1).appendIndex(1).success().build() ) );
			  invocationOrder.verify( _outbound, times( 1 ) ).send( same( _otherMember ), eq( appendEntriesResponse().from(_myself).term(0).matchIndex(2).appendIndex(2).success().build() ) );
			  invocationOrder.verify( _outbound, times( 1 ) ).send( same( _otherMember ), eq( appendEntriesResponse().from(_myself).term(0).matchIndex(3).appendIndex(3).success().build() ) );
			  invocationOrder.verify( _outbound, times( 1 ) ).send( same( _otherMember ), eq( appendEntriesResponse().from(_myself).term(2).matchIndex(-1).appendIndex(3).failure().build() ) );
		 }
	}

}