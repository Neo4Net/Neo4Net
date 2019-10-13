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
	using Description = org.hamcrest.Description;
	using TypeSafeMatcher = org.hamcrest.TypeSafeMatcher;
	using Test = org.junit.Test;

	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using ReadableRaftLog = Neo4Net.causalclustering.core.consensus.log.ReadableRaftLog;
	using Outcome = Neo4Net.causalclustering.core.consensus.outcome.Outcome;
	using RaftLogCommand = Neo4Net.causalclustering.core.consensus.outcome.RaftLogCommand;
	using TruncateLogCommand = Neo4Net.causalclustering.core.consensus.outcome.TruncateLogCommand;
	using ReadableRaftState = Neo4Net.causalclustering.core.consensus.state.ReadableRaftState;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using NullLog = Neo4Net.Logging.NullLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.hamcrest.MockitoHamcrest.argThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.identity.RaftTestMember.member;

	public class AppendingTest
	{
		 private MemberId _aMember = member( 0 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPerformTruncation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPerformTruncation()
		 {
			  // when
			  // we have a log appended up to appendIndex, and committed somewhere before that
			  long appendIndex = 5;
			  long localTermForAllEntries = 1L;

			  Outcome outcome = mock( typeof( Outcome ) );
			  ReadableRaftLog logMock = mock( typeof( ReadableRaftLog ) );
			  when( logMock.ReadEntryTerm( anyLong() ) ).thenReturn(localTermForAllEntries); // for simplicity, all entries are at term 1
			  when( logMock.AppendIndex() ).thenReturn(appendIndex);

			  ReadableRaftState state = mock( typeof( ReadableRaftState ) );
			  when( state.EntryLog() ).thenReturn(logMock);
			  when( state.CommitIndex() ).thenReturn(appendIndex - 3);

			  // when
			  // the leader asks to append after the commit index an entry that mismatches on term
			  Appending.HandleAppendEntriesRequest( state, outcome, new Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request( _aMember, localTermForAllEntries, appendIndex - 2, localTermForAllEntries, new RaftLogEntry[]{ new RaftLogEntry( localTermForAllEntries + 1, ReplicatedInteger.valueOf( 2 ) ) }, appendIndex + 3 ), NullLog.Instance );

			  // then
			  // we must produce a TruncateLogCommand at the earliest mismatching index
			  verify( outcome, times( 1 ) ).addLogCommand( argThat( new LogCommandMatcher( appendIndex - 1 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowTruncationAtCommit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowTruncationAtCommit()
		 {
			  // given
			  long commitIndex = 5;
			  long localTermForAllEntries = 1L;

			  Outcome outcome = mock( typeof( Outcome ) );
			  ReadableRaftLog logMock = mock( typeof( ReadableRaftLog ) );
			  when( logMock.ReadEntryTerm( anyLong() ) ).thenReturn(localTermForAllEntries); // for simplicity, all entries are at term 1
			  when( logMock.AppendIndex() ).thenReturn(commitIndex);

			  ReadableRaftState state = mock( typeof( ReadableRaftState ) );
			  when( state.EntryLog() ).thenReturn(logMock);
			  when( state.CommitIndex() ).thenReturn(commitIndex);

			  // when - then
			  try
			  {
					Appending.HandleAppendEntriesRequest( state, outcome, new Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request( _aMember, localTermForAllEntries, commitIndex - 1, localTermForAllEntries, new RaftLogEntry[]{ new RaftLogEntry( localTermForAllEntries + 1, ReplicatedInteger.valueOf( 2 ) ) }, commitIndex + 3 ), NullLog.Instance );
					fail( "Appending should not allow truncation at or before the commit index" );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// ok
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowTruncationBeforeCommit() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowTruncationBeforeCommit()
		 {
			  // given
			  long commitIndex = 5;
			  long localTermForAllEntries = 1L;

			  Outcome outcome = mock( typeof( Outcome ) );
			  ReadableRaftLog logMock = mock( typeof( ReadableRaftLog ) );
			  when( logMock.ReadEntryTerm( anyLong() ) ).thenReturn(localTermForAllEntries); // for simplicity, all entries are at term 1
			  when( logMock.AppendIndex() ).thenReturn(commitIndex);

			  ReadableRaftState state = mock( typeof( ReadableRaftState ) );
			  when( state.EntryLog() ).thenReturn(logMock);
			  when( state.CommitIndex() ).thenReturn(commitIndex);

			  // when - then
			  try
			  {
					Appending.HandleAppendEntriesRequest( state, outcome, new Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request( _aMember, localTermForAllEntries, commitIndex - 2, localTermForAllEntries, new RaftLogEntry[]{ new RaftLogEntry( localTermForAllEntries + 1, ReplicatedInteger.valueOf( 2 ) ) }, commitIndex + 3 ), NullLog.Instance );
					fail( "Appending should not allow truncation at or before the commit index" );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// fine
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAttemptToTruncateAtIndexBeforeTheLogPrevIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAttemptToTruncateAtIndexBeforeTheLogPrevIndex()
		 {
			  // given
			  // a log with prevIndex and prevTerm set
			  ReadableRaftLog logMock = mock( typeof( ReadableRaftLog ) );
			  long prevIndex = 5;
			  long prevTerm = 5;
			  when( logMock.PrevIndex() ).thenReturn(prevIndex);
			  when( logMock.ReadEntryTerm( prevIndex ) ).thenReturn( prevTerm );
			  // and which also properly returns -1 as the term for an unknown entry, in this case prevIndex - 2
			  when( logMock.ReadEntryTerm( prevIndex - 2 ) ).thenReturn( -1L );

			  // also, a state with a given commitIndex, obviously ahead of prevIndex
			  long commitIndex = 10;
			  ReadableRaftState state = mock( typeof( ReadableRaftState ) );
			  when( state.EntryLog() ).thenReturn(logMock);
			  when( state.CommitIndex() ).thenReturn(commitIndex);
			  // which is also the append index
			  when( logMock.AppendIndex() ).thenReturn(commitIndex);

			  // when
			  // an appendEntriesRequest arrives for appending entries before the prevIndex (for whatever reason)
			  Outcome outcome = mock( typeof( Outcome ) );
			  Appending.HandleAppendEntriesRequest( state, outcome, new Neo4Net.causalclustering.core.consensus.RaftMessages_AppendEntries_Request( _aMember, prevTerm, prevIndex - 2, prevTerm, new RaftLogEntry[]{ new RaftLogEntry( prevTerm, ReplicatedInteger.valueOf( 2 ) ) }, commitIndex + 3 ), NullLog.Instance );

			  // then
			  // there should be no truncate commands. Actually, the whole thing should be a no op
			  verify( outcome, never() ).addLogCommand(any());
		 }

		 private class LogCommandMatcher : TypeSafeMatcher<RaftLogCommand>
		 {
			  internal readonly long TruncateIndex;

			  internal LogCommandMatcher( long truncateIndex )
			  {
					this.TruncateIndex = truncateIndex;
			  }

			  protected internal override bool MatchesSafely( RaftLogCommand item )
			  {
					return item is TruncateLogCommand && ( ( TruncateLogCommand ) item ).fromIndex == TruncateIndex;
			  }

			  public override void DescribeTo( Description description )
			  {
					description.appendText( ( new TruncateLogCommand( TruncateIndex ) ).ToString() );
			  }
		 }
	}

}