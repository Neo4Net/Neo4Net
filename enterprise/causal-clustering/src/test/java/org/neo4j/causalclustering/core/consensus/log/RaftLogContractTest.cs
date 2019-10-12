using System.Text;

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
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.ReplicatedInteger.valueOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.log.RaftLogHelper.hasNoContent;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.log.RaftLogHelper.readLogEntry;

	public abstract class RaftLogContractTest
	{
		 public abstract RaftLog CreateRaftLog();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportCorrectDefaultValuesOnEmptyLog() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportCorrectDefaultValuesOnEmptyLog()
		 {
			  // given
			  ReadableRaftLog log = CreateRaftLog();

			  // then
			  assertThat( log.AppendIndex(), @is(-1L) );
			  assertThat( log.PrevIndex(), @is(-1L) );
			  assertThat( log.ReadEntryTerm( 0 ), @is( -1L ) );
			  assertThat( log.ReadEntryTerm( -1 ), @is( -1L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldResetHighTermOnTruncate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldResetHighTermOnTruncate()
		 {
			  // given
			  RaftLog log = CreateRaftLog();
			  log.Append( new RaftLogEntry( 45, valueOf( 99 ) ), new RaftLogEntry( 46, valueOf( 99 ) ), new RaftLogEntry( 47, valueOf( 99 ) ) );

			  // truncate the last 2
			  log.Truncate( 1 );

			  // then
			  log.Append( new RaftLogEntry( 46, valueOf( 9999 ) ) );

			  assertThat( log.ReadEntryTerm( 1 ), @is( 46L ) );
			  assertThat( log.AppendIndex(), @is(1L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAppendDataAndNotCommitImmediately() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAppendDataAndNotCommitImmediately()
		 {
			  RaftLog log = CreateRaftLog();

			  RaftLogEntry logEntry = new RaftLogEntry( 1, valueOf( 1 ) );
			  log.Append( logEntry );

			  assertThat( log.AppendIndex(), @is(0L) );
			  assertThat( readLogEntry( log, 0 ), equalTo( logEntry ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTruncatePreviouslyAppendedEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTruncatePreviouslyAppendedEntries()
		 {
			  RaftLog log = CreateRaftLog();

			  RaftLogEntry logEntryA = new RaftLogEntry( 1, valueOf( 1 ) );
			  RaftLogEntry logEntryB = new RaftLogEntry( 1, valueOf( 2 ) );

			  log.Append( logEntryA, logEntryB );

			  assertThat( log.AppendIndex(), @is(1L) );

			  log.Truncate( 1 );

			  assertThat( log.AppendIndex(), @is(0L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReplacePreviouslyAppendedEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReplacePreviouslyAppendedEntries()
		 {
			  RaftLog log = CreateRaftLog();

			  RaftLogEntry logEntryA = new RaftLogEntry( 1, valueOf( 1 ) );
			  RaftLogEntry logEntryB = new RaftLogEntry( 1, valueOf( 2 ) );
			  RaftLogEntry logEntryC = new RaftLogEntry( 1, valueOf( 3 ) );
			  RaftLogEntry logEntryD = new RaftLogEntry( 1, valueOf( 4 ) );
			  RaftLogEntry logEntryE = new RaftLogEntry( 1, valueOf( 5 ) );

			  log.Append( logEntryA, logEntryB, logEntryC );

			  log.Truncate( 1 );

			  log.Append( logEntryD, logEntryE );

			  assertThat( log.AppendIndex(), @is(2L) );
			  assertThat( readLogEntry( log, 0 ), equalTo( logEntryA ) );
			  assertThat( readLogEntry( log, 1 ), equalTo( logEntryD ) );
			  assertThat( readLogEntry( log, 2 ), equalTo( logEntryE ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveNoEffectWhenTruncatingNonExistingEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHaveNoEffectWhenTruncatingNonExistingEntries()
		 {
			  // Given
			  RaftLog log = CreateRaftLog();

			  RaftLogEntry logEntryA = new RaftLogEntry( 1, valueOf( 1 ) );
			  RaftLogEntry logEntryB = new RaftLogEntry( 1, valueOf( 2 ) );

			  log.Append( logEntryA, logEntryB );

			  try
			  {
					// When
					log.Truncate( 5 );
					fail( "Truncate at index after append index should never be attempted" );
			  }
			  catch ( System.ArgumentException )
			  {
					// Then
					// an exception should be thrown
			  }

			  // Then, assert that the state is unchanged
			  assertThat( log.AppendIndex(), @is(1L) );
			  assertThat( readLogEntry( log, 0 ), equalTo( logEntryA ) );
			  assertThat( readLogEntry( log, 1 ), equalTo( logEntryB ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogDifferentContentTypes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogDifferentContentTypes()
		 {
			  RaftLog log = CreateRaftLog();

			  RaftLogEntry logEntryA = new RaftLogEntry( 1, valueOf( 1 ) );
			  RaftLogEntry logEntryB = new RaftLogEntry( 1, ReplicatedString.valueOf( "hejzxcjkzhxcjkxz" ) );

			  log.Append( logEntryA, logEntryB );

			  assertThat( log.AppendIndex(), @is(1L) );

			  assertThat( readLogEntry( log, 0 ), equalTo( logEntryA ) );
			  assertThat( readLogEntry( log, 1 ), equalTo( logEntryB ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRejectNonMonotonicTermsForEntries() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRejectNonMonotonicTermsForEntries()
		 {
			  // given
			  RaftLog log = CreateRaftLog();
			  log.Append( new RaftLogEntry( 0, valueOf( 1 ) ), new RaftLogEntry( 1, valueOf( 2 ) ) );

			  try
			  {
					// when the term has a lower value
					log.Append( new RaftLogEntry( 0, valueOf( 3 ) ) );
					// then an exception should be thrown
					fail( "Should have failed because of non-monotonic terms" );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// expected
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAppendAndThenTruncateSubsequentEntry() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAppendAndThenTruncateSubsequentEntry()
		 {
			  // given
			  RaftLog log = CreateRaftLog();
			  log.Append( new RaftLogEntry( 0, valueOf( 0 ) ) );
			  long toBeSpared = log.Append( new RaftLogEntry( 0, valueOf( 1 ) ) );
			  long toTruncate = log.Append( new RaftLogEntry( 1, valueOf( 2 ) ) );

			  // when
			  log.Truncate( toTruncate );

			  // then
			  assertThat( log.AppendIndex(), @is(toBeSpared) );
			  assertThat( log.ReadEntryTerm( toBeSpared ), @is( 0L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAppendAfterTruncating() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAppendAfterTruncating()
		 {
			  // given
			  RaftLog log = CreateRaftLog();
			  log.Append( new RaftLogEntry( 0, valueOf( 0 ) ) );
			  long toCommit = log.Append( new RaftLogEntry( 0, valueOf( 1 ) ) );
			  long toTruncate = log.Append( new RaftLogEntry( 1, valueOf( 2 ) ) );

			  // when
			  log.Truncate( toTruncate );
			  long lastAppended = log.Append( new RaftLogEntry( 2, valueOf( 3 ) ) );

			  // then
			  assertThat( log.AppendIndex(), @is(lastAppended) );
			  assertThat( log.ReadEntryTerm( toCommit ), @is( 0L ) );
			  assertThat( log.ReadEntryTerm( lastAppended ), @is( 2L ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEventuallyPrune() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldEventuallyPrune()
		 {
			  // given
			  RaftLog log = CreateRaftLog();
			  int term = 0;

			  long safeIndex = -1;
			  long prunedIndex = -1;

			  // this loop should eventually be able to prune something
			  while ( prunedIndex == -1 )
			  {
					for ( int i = 0; i < 100; i++ )
					{
						 log.Append( new RaftLogEntry( term, valueOf( 10 * term ) ) );
						 term++;
					}
					safeIndex = log.AppendIndex() - 50;
					// when
					prunedIndex = log.Prune( safeIndex );
			  }

			  // then
			  assertThat( prunedIndex, lessThanOrEqualTo( safeIndex ) );
			  assertEquals( prunedIndex, log.PrevIndex() );
			  assertEquals( prunedIndex, log.ReadEntryTerm( prunedIndex ) );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long[] expectedVal = {prunedIndex + 1};
			  long[] expectedVal = new long[] { prunedIndex + 1 };
			  log.GetEntryCursor( prunedIndex + 1 ).forAll( entry => assertThat( entry.content(), @is(valueOf(10 * (int)expectedVal[0]++)) ) );

			  assertThat( log, hasNoContent( prunedIndex ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSkipAheadInEmptyLog() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSkipAheadInEmptyLog()
		 {
			  // given
			  RaftLog log = CreateRaftLog();

			  // when
			  long skipIndex = 10;
			  long skipTerm = 2;
			  log.Skip( skipIndex, skipTerm );

			  // then
			  assertEquals( skipIndex, log.AppendIndex() );
			  assertEquals( skipTerm, log.ReadEntryTerm( skipIndex ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSkipAheadInLogWithContent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSkipAheadInLogWithContent()
		 {
			  // given
			  RaftLog log = CreateRaftLog();

			  long term = 0;
			  int entryCount = 5;
			  for ( int i = 0; i < entryCount; i++ )
			  {
					log.Append( new RaftLogEntry( term, valueOf( i ) ) );
			  }

			  // when
			  long skipIndex = entryCount + 5;
			  long skipTerm = term + 2;
			  log.Skip( skipIndex, skipTerm );

			  // then
			  assertEquals( skipIndex, log.AppendIndex() );
			  assertEquals( skipTerm, log.ReadEntryTerm( skipIndex ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSkipInLogWithLaterContent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotSkipInLogWithLaterContent()
		 {
			  // given
			  RaftLog log = CreateRaftLog();

			  long term = 0;
			  int entryCount = 5;
			  for ( int i = 0; i < entryCount; i++ )
			  {
					log.Append( new RaftLogEntry( term, valueOf( i ) ) );
			  }
			  long lastIndex = log.AppendIndex();

			  // when
			  long skipIndex = entryCount - 2;
			  log.Skip( skipIndex, term );

			  // then
			  assertEquals( lastIndex, log.AppendIndex() );
			  assertEquals( term, log.ReadEntryTerm( skipIndex ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToAppendAfterSkipping() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToAppendAfterSkipping()
		 {
			  // given
			  RaftLog log = CreateRaftLog();

			  // when
			  long skipIndex = 5;
			  long term = 0;
			  log.Skip( skipIndex, term );

			  int newContentValue = 100;
			  long newEntryIndex = skipIndex + 1;
			  long appendedIndex = log.Append( new RaftLogEntry( term, valueOf( newContentValue ) ) );

			  // then
			  assertEquals( newEntryIndex, log.AppendIndex() );
			  assertEquals( newEntryIndex, appendedIndex );

			  try
			  {
					readLogEntry( log, skipIndex );
					fail( "Should have thrown exception" );
			  }
			  catch ( IOException )
			  {
					// expected
			  }
			  assertThat( readLogEntry( log, newEntryIndex ).content(), @is(valueOf(newContentValue)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pruneShouldNotChangePrevIndexAfterSkipping() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PruneShouldNotChangePrevIndexAfterSkipping()
		 {
			  /// <summary>
			  /// Given the situation where a skip happens followed by a prune, you may have the prune operation incorrectly
			  /// set the prevIndex to be the value of the last segment in the log, disregarding the skip command.
			  /// This test ensures that in this scenario, we will respect the current prevIndex value if it has been set to
			  /// something in the future (i.e. skip) rather than modify it to be the value of the last segment.
			  /// 
			  /// Initial Scenario:    [0][1][2][3][4][5][6][7][8][9]              prevIndex = 0
			  /// Skip to 20 :         [0][1][2][3][4][5][6][7][8][9]...[20]               prevIndex = 20
			  /// Prune segment 8:                                [9]...[20]               prevIndex = 20 //not 9
			  /// </summary>

			  // given
			  RaftLog log = CreateRaftLog();

			  long term = 0;
			  for ( int i = 0; i < 2000; i++ )
			  {
					log.Append( new RaftLogEntry( term, valueOf( i ) ) );
			  }

			  long skipIndex = 3000;
			  log.Skip( skipIndex, term );
			  assertEquals( skipIndex, log.PrevIndex() );

			  // when
			  log.Prune( skipIndex );

			  // then
			  assertEquals( skipIndex, log.PrevIndex() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProperlyReportExistenceOfIndexesAfterSkipping() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProperlyReportExistenceOfIndexesAfterSkipping()
		 {
			  // given
			  RaftLog log = CreateRaftLog();
			  long term = 0;
			  long existingEntryIndex = log.Append( new RaftLogEntry( term, valueOf( 100 ) ) );

			  long skipIndex = 15;

			  // when
			  log.Skip( skipIndex, term );

			  // then
			  assertEquals( skipIndex, log.AppendIndex() );

			  // all indexes starting from the next of the last appended to the skipped index (and forward) should not be present
			  for ( long i = existingEntryIndex + 1; i < skipIndex + 2; i++ )
			  {
					try
					{
						 readLogEntry( log, i );
						 fail( "Should have thrown exception at index " + i );
					}
					catch ( IOException )
					{
						 // expected
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowExceptionWhenReadingAnEntryWhichHasBeenPruned() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowExceptionWhenReadingAnEntryWhichHasBeenPruned()
		 {
			  RaftLog log = CreateRaftLog();
			  log.Append( new RaftLogEntry( 0, String( 1024 ) ) );
			  log.Append( new RaftLogEntry( 1, String( 1024 ) ) );
			  log.Append( new RaftLogEntry( 2, String( 1024 ) ) );
			  log.Append( new RaftLogEntry( 3, String( 1024 ) ) );
			  log.Append( new RaftLogEntry( 4, String( 1024 ) ) );

			  long pruneIndex = log.Prune( 4 );
			  assertThat( pruneIndex, greaterThanOrEqualTo( 2L ) );

			  long term = log.ReadEntryTerm( 1 );

			  RaftLogCursor cursor = log.GetEntryCursor( 1 );
			  if ( cursor.Next() )
			  {
					fail(); //the cursor should return false since this has been pruned.
			  }

			  assertEquals( -1L, term );
		 }

		 private ReplicatedString String( int numberOfCharacters )
		 {
			  StringBuilder builder = new StringBuilder();
			  for ( int i = 0; i < numberOfCharacters; i++ )
			  {
					builder.Append( i.ToString() );
			  }
			  return ReplicatedString.valueOf( builder.ToString() );
		 }

		 // TODO: Test what happens when the log has rotated, *not* pruned and then skipping happens which causes
		 // TODO: archived logs to be forgotten about. Does it still return the entries or at least fail gracefully?
		 // TODO: In the case of PhysicalRaftLog, are the ranges kept properly up to date to notify of non existing files?
	}

}