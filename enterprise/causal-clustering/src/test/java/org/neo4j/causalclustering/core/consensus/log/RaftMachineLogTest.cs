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
namespace Org.Neo4j.causalclustering.core.consensus.log
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Mock = org.mockito.Mock;
	using MockitoJUnitRunner = org.mockito.junit.MockitoJUnitRunner;

	using RaftTestGroup = Org.Neo4j.causalclustering.core.consensus.membership.RaftTestGroup;
	using ReplicatedContent = Org.Neo4j.causalclustering.core.replication.ReplicatedContent;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using RaftTestMemberSetBuilder = Org.Neo4j.causalclustering.identity.RaftTestMemberSetBuilder;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.ReplicatedInteger.valueOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.TestMessageBuilders.appendEntriesRequest;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.log.RaftLogHelper.readLogEntry;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.identity.RaftTestMember.member;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(MockitoJUnitRunner.class) public class RaftMachineLogTest
	public class RaftMachineLogTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock RaftMachineBuilder.CommitListener commitListener;
		 internal RaftMachineBuilder.CommitListener CommitListener;

		 private MemberId _myself = member( 0 );
		 private ReplicatedContent _content = valueOf( 1 );
		 private RaftLog _testEntryLog;

		 private RaftMachine _raft;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Before()
		 {
			  // given
			  _testEntryLog = new InMemoryRaftLog();
			  _testEntryLog.append( new RaftLogEntry( 0, new RaftTestGroup( _myself ) ) );

			  _raft = ( new RaftMachineBuilder( _myself, 3, RaftTestMemberSetBuilder.INSTANCE ) ).raftLog( _testEntryLog ).commitListener( CommitListener ).build();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPersistAtSpecifiedLogIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPersistAtSpecifiedLogIndex()
		 {
			  // when
			  _raft.handle( appendEntriesRequest().leaderTerm(0).prevLogIndex(0).prevLogTerm(0).logEntry(new RaftLogEntry(0, _content)).build() );

			  // then
			  assertEquals( 1, _testEntryLog.appendIndex() );
			  assertEquals( _content, readLogEntry( _testEntryLog, 1 ).content() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyPersistSameLogEntryOnce() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOnlyPersistSameLogEntryOnce()
		 {
			  // when
			  _raft.handle( appendEntriesRequest().leaderTerm(0).prevLogIndex(0).prevLogTerm(0).logEntry(new RaftLogEntry(0, _content)).build() );
			  _raft.handle( appendEntriesRequest().leaderTerm(0).prevLogIndex(0).prevLogTerm(0).logEntry(new RaftLogEntry(0, _content)).build() );

			  // then
			  assertEquals( 1, _testEntryLog.appendIndex() );
			  assertEquals( _content, readLogEntry( _testEntryLog, 1 ).content() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveLaterEntryFromLogConflictingWithNewEntry() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveLaterEntryFromLogConflictingWithNewEntry()
		 {
			  _testEntryLog.append( new RaftLogEntry( 1, valueOf( 1 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 1, valueOf( 4 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 1, valueOf( 7 ) ) ); // conflicting entry

			  // when
			  ReplicatedInteger newData = valueOf( 11 );
			  _raft.handle( appendEntriesRequest().leaderTerm(2).prevLogIndex(2).prevLogTerm(1).logEntry(new RaftLogEntry(2, newData)).build() );

			  // then
			  assertEquals( 3, _testEntryLog.appendIndex() );
			  assertEquals( newData, readLogEntry( _testEntryLog, 3 ).content() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotTouchTheLogIfWeDoMatchEverywhere() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotTouchTheLogIfWeDoMatchEverywhere()
		 {
			  _testEntryLog.append( new RaftLogEntry( 1, valueOf( 99 ) ) ); // 0
			  _testEntryLog.append( new RaftLogEntry( 1, valueOf( 99 ) ) ); // 1
			  _testEntryLog.append( new RaftLogEntry( 1, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 2, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 2, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 2, valueOf( 99 ) ) ); // 5
			  _testEntryLog.append( new RaftLogEntry( 3, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 3, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 3, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 3, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 3, valueOf( 99 ) ) ); // 10

			  // when instance A as leader
			  ReplicatedInteger newData = valueOf( 99 );

			  // Matches everything in the given range
			  _raft.handle( appendEntriesRequest().leaderTerm(8).prevLogIndex(5).prevLogTerm(2).logEntry(new RaftLogEntry(2, newData)).logEntry(new RaftLogEntry(3, newData)).logEntry(new RaftLogEntry(3, newData)).logEntry(new RaftLogEntry(3, newData)).build() );

			  // then
			  assertEquals( 11, _testEntryLog.appendIndex() );
			  assertEquals( 3, _testEntryLog.readEntryTerm( 11 ) );
		 }

		 /* Figure 3.6 */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotTouchTheLogIfWeDoNotMatchAnywhere() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotTouchTheLogIfWeDoNotMatchAnywhere()
		 {
			  _testEntryLog.append( new RaftLogEntry( 1, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 1, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 1, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 2, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 2, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 2, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 3, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 3, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 3, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 3, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 3, valueOf( 99 ) ) );

			  // when instance A as leader
			  ReplicatedInteger newData = valueOf( 99 );

			  // Will not match as the entry at index 5 has term  2
			  _raft.handle( appendEntriesRequest().leaderTerm(8).prevLogIndex(6).prevLogTerm(5).logEntry(new RaftLogEntry(5, newData)).logEntry(new RaftLogEntry(5, newData)).logEntry(new RaftLogEntry(6, newData)).logEntry(new RaftLogEntry(6, newData)).logEntry(new RaftLogEntry(6, newData)).build() );

			  // then
			  assertEquals( 11, _testEntryLog.appendIndex() );
			  assertEquals( 3, _testEntryLog.readEntryTerm( 11 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTruncateOnFirstMismatchAndThenAppendOtherEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTruncateOnFirstMismatchAndThenAppendOtherEntries()
		 {
			  _testEntryLog.append( new RaftLogEntry( 1, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 1, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 1, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 2, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 2, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 2, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 3, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 3, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 3, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 3, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 3, valueOf( 99 ) ) );

			  // when instance A as leader
			  ReplicatedInteger newData = valueOf( 99 );

			  _raft.handle( appendEntriesRequest().leaderTerm(8).prevLogIndex(0).prevLogTerm(0).logEntry(new RaftLogEntry(1, newData)).logEntry(new RaftLogEntry(1, newData)).logEntry(new RaftLogEntry(1, newData)).logEntry(new RaftLogEntry(4, newData)).logEntry(new RaftLogEntry(4, newData)).logEntry(new RaftLogEntry(5, newData)).logEntry(new RaftLogEntry(5, newData)).logEntry(new RaftLogEntry(6, newData)).logEntry(new RaftLogEntry(6, newData)).logEntry(new RaftLogEntry(6, newData)).build() );

			  // then
			  assertEquals( 10, _testEntryLog.appendIndex() );
			  assertEquals( 1, _testEntryLog.readEntryTerm( 1 ) );
			  assertEquals( 1, _testEntryLog.readEntryTerm( 2 ) );
			  assertEquals( 1, _testEntryLog.readEntryTerm( 3 ) );
			  assertEquals( 4, _testEntryLog.readEntryTerm( 4 ) );
			  assertEquals( 4, _testEntryLog.readEntryTerm( 5 ) );
			  assertEquals( 5, _testEntryLog.readEntryTerm( 6 ) );
			  assertEquals( 5, _testEntryLog.readEntryTerm( 7 ) );
			  assertEquals( 6, _testEntryLog.readEntryTerm( 8 ) );
			  assertEquals( 6, _testEntryLog.readEntryTerm( 9 ) );
			  assertEquals( 6, _testEntryLog.readEntryTerm( 10 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotTruncateLogIfHistoryDoesNotMatch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotTruncateLogIfHistoryDoesNotMatch()
		 {
			  _testEntryLog.append( new RaftLogEntry( 1, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 1, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 1, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 2, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 2, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 2, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 3, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 3, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 3, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 3, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 3, valueOf( 99 ) ) );

			  // when instance A as leader
			  ReplicatedInteger newData = valueOf( 99 );
			  _raft.handle( appendEntriesRequest().leaderTerm(8).prevLogIndex(4).prevLogTerm(4).logEntry(new RaftLogEntry(4, newData)).logEntry(new RaftLogEntry(5, newData)).logEntry(new RaftLogEntry(5, newData)).logEntry(new RaftLogEntry(6, newData)).logEntry(new RaftLogEntry(6, newData)).logEntry(new RaftLogEntry(6, newData)).build() );

			  // then
			  assertEquals( 11, _testEntryLog.appendIndex() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTruncateLogIfFirstEntryMatchesAndSecondEntryMismatchesOnTerm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTruncateLogIfFirstEntryMatchesAndSecondEntryMismatchesOnTerm()
		 {
			  _testEntryLog.append( new RaftLogEntry( 1, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 1, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 1, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 2, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 2, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 2, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 3, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 3, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 3, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 3, valueOf( 99 ) ) );
			  _testEntryLog.append( new RaftLogEntry( 3, valueOf( 99 ) ) );

			  // when instance A as leader
			  ReplicatedInteger newData = valueOf( 99 );
			  _raft.handle( appendEntriesRequest().leaderTerm(8).prevLogIndex(2).prevLogTerm(1).logEntry(new RaftLogEntry(1, newData)).logEntry(new RaftLogEntry(4, newData)).logEntry(new RaftLogEntry(4, newData)).logEntry(new RaftLogEntry(5, newData)).logEntry(new RaftLogEntry(5, newData)).logEntry(new RaftLogEntry(6, newData)).logEntry(new RaftLogEntry(6, newData)).logEntry(new RaftLogEntry(6, newData)).build() );

			  // then
			  assertEquals( 10, _testEntryLog.appendIndex() );

			  // stay the same
			  assertEquals( 1, _testEntryLog.readEntryTerm( 1 ) );
			  assertEquals( 1, _testEntryLog.readEntryTerm( 2 ) );
			  assertEquals( 1, _testEntryLog.readEntryTerm( 3 ) );

			  // replaced
			  assertEquals( 4, _testEntryLog.readEntryTerm( 4 ) );
			  assertEquals( 4, _testEntryLog.readEntryTerm( 5 ) );
			  assertEquals( 5, _testEntryLog.readEntryTerm( 6 ) );
			  assertEquals( 5, _testEntryLog.readEntryTerm( 7 ) );
			  assertEquals( 6, _testEntryLog.readEntryTerm( 8 ) );
			  assertEquals( 6, _testEntryLog.readEntryTerm( 9 ) );
			  assertEquals( 6, _testEntryLog.readEntryTerm( 10 ) );
		 }
	}

}