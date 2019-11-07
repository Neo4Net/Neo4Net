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
namespace Neo4Net.causalclustering.core.consensus.roles
{
	using Matchers = org.hamcrest.Matchers;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using InMemoryRaftLog = Neo4Net.causalclustering.core.consensus.log.InMemoryRaftLog;
	using RaftLog = Neo4Net.causalclustering.core.consensus.log.RaftLog;
	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using BatchAppendLogEntries = Neo4Net.causalclustering.core.consensus.outcome.BatchAppendLogEntries;
	using Outcome = Neo4Net.causalclustering.core.consensus.outcome.Outcome;
	using TruncateLogCommand = Neo4Net.causalclustering.core.consensus.outcome.TruncateLogCommand;
	using RaftState = Neo4Net.causalclustering.core.consensus.state.RaftState;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Log = Neo4Net.Logging.Log;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.consensus.MessageUtils.messageFor;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.consensus.TestMessageBuilders.appendEntriesRequest;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.consensus.roles.AppendEntriesRequestTest.ContentGenerator.content;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.consensus.state.RaftStateBuilder.raftState;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.identity.RaftTestMember.member;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class AppendEntriesRequestTest
	public class AppendEntriesRequestTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0} with leader {1} terms ahead.") public static java.util.Collection<Object[]> data()
		 public static ICollection<object[]> Data()
		 {
			  return Arrays.asList(new object[][]
			  {
				  new object[] { Role.Follower, 0 },
				  new object[] { Role.Follower, 1 },
				  new object[] { Role.Leader, 1 },
				  new object[] { Role.Candidate, 1 }
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(value = 0) public Role role;
		 public Role Role;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(value = 1) public int leaderTermDifference;
		 public int LeaderTermDifference;

		 private MemberId _myself = member( 0 );
		 private MemberId _leader = member( 1 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptInitialEntryAfterBootstrap() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcceptInitialEntryAfterBootstrap()
		 {
			  RaftLog raftLog = BootstrappedLog();
			  RaftState state = raftState().entryLog(raftLog).myself(_myself).build();

			  long leaderTerm = state.Term() + LeaderTermDifference;
			  RaftLogEntry logEntry = new RaftLogEntry( leaderTerm, content() );

			  // when
			  Outcome outcome = Role.handler.handle( appendEntriesRequest().from(_leader).leaderTerm(leaderTerm).prevLogIndex(0).prevLogTerm(0).logEntry(logEntry).build(), state, Log() );

			  // then
			  assertTrue( ( ( RaftMessages_AppendEntries_Response ) messageFor( outcome, _leader ) ).success() );
			  assertThat( outcome.LogCommands, hasItem( new BatchAppendLogEntries( 1, 0, new RaftLogEntry[]{ logEntry } ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptInitialEntriesAfterBootstrap() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcceptInitialEntriesAfterBootstrap()
		 {
			  RaftLog raftLog = BootstrappedLog();
			  RaftState state = raftState().entryLog(raftLog).myself(_myself).build();

			  long leaderTerm = state.Term() + LeaderTermDifference;
			  RaftLogEntry logEntry1 = new RaftLogEntry( leaderTerm, content() );
			  RaftLogEntry logEntry2 = new RaftLogEntry( leaderTerm, content() );

			  // when
			  Outcome outcome = Role.handler.handle( appendEntriesRequest().from(_leader).leaderTerm(leaderTerm).prevLogIndex(0).prevLogTerm(0).logEntry(logEntry1).logEntry(logEntry2).build(), state, Log() );

			  // then
			  assertTrue( ( ( RaftMessages_AppendEntries_Response ) messageFor( outcome, _leader ) ).success() );
			  assertThat( outcome.LogCommands, hasItem( new BatchAppendLogEntries( 1, 0, new RaftLogEntry[]{ logEntry1, logEntry2 } ) ) );
		 }

		 private RaftLog BootstrappedLog()
		 {
			  InMemoryRaftLog raftLog = new InMemoryRaftLog();
			  raftLog.Append( new RaftLogEntry( 0, content() ) );
			  return raftLog;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRejectDiscontinuousEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRejectDiscontinuousEntries()
		 {
			  // given
			  RaftState state = raftState().myself(_myself).build();

			  long leaderTerm = state.Term() + LeaderTermDifference;

			  // when
			  Outcome outcome = Role.handler.handle( appendEntriesRequest().from(_leader).leaderTerm(leaderTerm).prevLogIndex(state.EntryLog().appendIndex() + 1).prevLogTerm(leaderTerm).logEntry(new RaftLogEntry(leaderTerm, content())).build(), state, Log() );

			  // then
			  RaftMessages_AppendEntries_Response response = ( RaftMessages_AppendEntries_Response ) messageFor( outcome, _leader );
			  assertEquals( state.EntryLog().appendIndex(), response.AppendIndex() );
			  assertFalse( response.Success() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptContinuousEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcceptContinuousEntries()
		 {
			  InMemoryRaftLog raftLog = new InMemoryRaftLog();
			  RaftState state = raftState().myself(_myself).entryLog(raftLog).build();

			  long leaderTerm = state.Term() + LeaderTermDifference;
			  raftLog.Append( new RaftLogEntry( leaderTerm, content() ) );

			  // when
			  Outcome outcome = Role.handler.handle( appendEntriesRequest().from(_leader).leaderTerm(leaderTerm).prevLogIndex(raftLog.AppendIndex()).prevLogTerm(leaderTerm).logEntry(new RaftLogEntry(leaderTerm, content())).build(), state, Log() );

			  // then
			  assertTrue( ( ( RaftMessages_AppendEntries_Response ) messageFor( outcome, _leader ) ).success() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTruncateOnReceiptOfConflictingEntry() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTruncateOnReceiptOfConflictingEntry()
		 {
			  // given
			  InMemoryRaftLog raftLog = new InMemoryRaftLog();
			  RaftState state = raftState().myself(_myself).term(5).entryLog(raftLog).build();

			  long leaderTerm = state.Term() + LeaderTermDifference;
			  raftLog.Append( new RaftLogEntry( state.Term() - 1, content() ) );
			  raftLog.Append( new RaftLogEntry( state.Term() - 1, content() ) );

			  // when
			  long previousIndex = raftLog.AppendIndex() - 1;
			  Outcome outcome = Role.handler.handle( appendEntriesRequest().from(_leader).leaderTerm(leaderTerm).prevLogIndex(previousIndex).prevLogTerm(raftLog.ReadEntryTerm(previousIndex)).logEntry(new RaftLogEntry(leaderTerm, content())).build(), state, Log() );

			  // then
			  assertTrue( ( ( RaftMessages_AppendEntries_Response ) messageFor( outcome, _leader ) ).success() );
			  assertThat( outcome.LogCommands, hasItem( new TruncateLogCommand( 1 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCommitEntry() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCommitEntry()
		 {
			  // given
			  InMemoryRaftLog raftLog = new InMemoryRaftLog();
			  RaftState state = raftState().entryLog(raftLog).myself(_myself).build();

			  long leaderTerm = state.Term() + LeaderTermDifference;
			  raftLog.Append( new RaftLogEntry( leaderTerm, content() ) );

			  // when
			  Outcome outcome = Role.handler.handle( appendEntriesRequest().from(_leader).leaderTerm(leaderTerm).prevLogIndex(raftLog.AppendIndex()).prevLogTerm(leaderTerm).leaderCommit(0).build(), state, Log() );

			  // then
			  assertTrue( ( ( RaftMessages_AppendEntries_Response ) messageFor( outcome, _leader ) ).success() );
			  assertThat( outcome.CommitIndex, Matchers.equalTo( 0L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAppendNewEntryAndCommitPreviouslyAppendedEntry() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAppendNewEntryAndCommitPreviouslyAppendedEntry()
		 {
			  // given
			  InMemoryRaftLog raftLog = new InMemoryRaftLog();
			  RaftState state = raftState().entryLog(raftLog).myself(_myself).build();

			  long leaderTerm = state.Term() + LeaderTermDifference;
			  RaftLogEntry previouslyAppendedEntry = new RaftLogEntry( leaderTerm, content() );
			  raftLog.Append( previouslyAppendedEntry );
			  RaftLogEntry newLogEntry = new RaftLogEntry( leaderTerm, content() );

			  // when
			  Outcome outcome = Role.handler.handle( appendEntriesRequest().from(_leader).leaderTerm(leaderTerm).prevLogIndex(raftLog.AppendIndex()).prevLogTerm(leaderTerm).logEntry(newLogEntry).leaderCommit(0).build(), state, Log() );

			  // then
			  assertTrue( ( ( RaftMessages_AppendEntries_Response ) messageFor( outcome, _leader ) ).success() );
			  assertThat( outcome.CommitIndex, Matchers.equalTo( 0L ) );
			  assertThat( outcome.LogCommands, hasItem( new BatchAppendLogEntries( 1, 0, new RaftLogEntry[]{ newLogEntry } ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotCommitAheadOfMatchingHistory() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotCommitAheadOfMatchingHistory()
		 {
			  // given
			  InMemoryRaftLog raftLog = new InMemoryRaftLog();
			  RaftState state = raftState().entryLog(raftLog).myself(_myself).build();

			  long leaderTerm = state.Term() + LeaderTermDifference;
			  RaftLogEntry previouslyAppendedEntry = new RaftLogEntry( leaderTerm, content() );
			  raftLog.Append( previouslyAppendedEntry );

			  // when
			  Outcome outcome = Role.handler.handle( appendEntriesRequest().from(_leader).leaderTerm(leaderTerm).prevLogIndex(raftLog.AppendIndex() + 1).prevLogTerm(leaderTerm).leaderCommit(0).build(), state, Log() );

			  // then
			  assertFalse( ( ( RaftMessages_AppendEntries_Response ) messageFor( outcome, _leader ) ).success() );
			  assertThat( outcome.LogCommands, empty() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.causalclustering.core.consensus.state.RaftState newState() throws java.io.IOException
		 public virtual RaftState NewState()
		 {
			  return raftState().myself(_myself).build();
		 }

		 private Log Log()
		 {
			  return NullLogProvider.Instance.getLog( this.GetType() );
		 }

		 internal class ContentGenerator
		 {
			  internal static int Count;

			  public static ReplicatedString Content()
			  {
					return new ReplicatedString( string.Format( "content#{0:D}", Count++ ) );
			  }
		 }
	}

}