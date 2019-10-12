/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Kernel.impl.transaction.log.reverse
{
	using Test = org.junit.Test;


	using Neo4Net.Function;
	using ArrayUtil = Neo4Net.Helpers.ArrayUtil;
	using LogEntryCommit = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryCommit;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.copyOfRange;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.GivenTransactionCursor.exhaust;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.GivenTransactionCursor.given;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.LogPosition.start;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogHeader.LOG_HEADER_SIZE;

	public class ReversedMultiFileTransactionCursorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadSingleVersionReversed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadSingleVersionReversed()
		 {
			  // GIVEN
			  TransactionCursor cursor = new ReversedMultiFileTransactionCursor( Log( 5 ), 0, start( 0 ) );

			  // WHEN
			  CommittedTransactionRepresentation[] reversed = exhaust( cursor );

			  // THEN
			  AssertTransactionRange( reversed, 5, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadMultipleVersionsReversed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadMultipleVersionsReversed()
		 {
			  // GIVEN
			  TransactionCursor cursor = new ReversedMultiFileTransactionCursor( Log( 5, 3, 8 ), 2, start( 0 ) );

			  // WHEN
			  CommittedTransactionRepresentation[] reversed = exhaust( cursor );

			  // THEN
			  AssertTransactionRange( reversed, 5 + 3 + 8, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespectStartLogPosition() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespectStartLogPosition()
		 {
			  // GIVEN
			  TransactionCursor cursor = new ReversedMultiFileTransactionCursor( Log( 5, 6, 8 ), 2, new LogPosition( 1, LOG_HEADER_SIZE + 3 ) );

			  // WHEN
			  CommittedTransactionRepresentation[] reversed = exhaust( cursor );

			  // THEN
			  AssertTransactionRange( reversed, 5 + 6 + 8, 5 + 3 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleEmptyLogsMidStream() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleEmptyLogsMidStream()
		 {
			  // GIVEN
			  TransactionCursor cursor = new ReversedMultiFileTransactionCursor( Log( 5, 0, 2, 0, 3 ), 4, start( 0 ) );

			  // WHEN
			  CommittedTransactionRepresentation[] reversed = exhaust( cursor );

			  // THEN
			  AssertTransactionRange( reversed, 5 + 2 + 3, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleEmptySingleLogVersion() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleEmptySingleLogVersion()
		 {
			  // GIVEN
			  TransactionCursor cursor = new ReversedMultiFileTransactionCursor( Log( 0 ), 0, start( 0 ) );

			  // WHEN
			  CommittedTransactionRepresentation[] reversed = exhaust( cursor );

			  // THEN
			  AssertTransactionRange( reversed, 0, 0 );
		 }

		 private void AssertTransactionRange( CommittedTransactionRepresentation[] reversed, long highTxId, long lowTxId )
		 {
			  long expectedTxId = highTxId;
			  foreach ( CommittedTransactionRepresentation transaction in reversed )
			  {
					expectedTxId--;
					assertEquals( expectedTxId, transaction.CommitEntry.TxId );
			  }
			  assertEquals( lowTxId, expectedTxId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.function.ThrowingFunction<org.neo4j.kernel.impl.transaction.log.LogPosition,org.neo4j.kernel.impl.transaction.log.TransactionCursor,java.io.IOException> log(int... transactionCounts) throws java.io.IOException
		 private ThrowingFunction<LogPosition, TransactionCursor, IOException> Log( params int[] transactionCounts )
		 {
			  long baseOffset = LogPosition.start( 0 ).ByteOffset;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") org.neo4j.function.ThrowingFunction<org.neo4j.kernel.impl.transaction.log.LogPosition,org.neo4j.kernel.impl.transaction.log.TransactionCursor,java.io.IOException> result = mock(org.neo4j.function.ThrowingFunction.class);
			  ThrowingFunction<LogPosition, TransactionCursor, IOException> result = mock( typeof( ThrowingFunction ) );
			  AtomicLong txId = new AtomicLong( 0 );
			  CommittedTransactionRepresentation[][] logs = new CommittedTransactionRepresentation[transactionCounts.Length][];
			  for ( int logVersion = 0; logVersion < transactionCounts.Length; logVersion++ )
			  {
					logs[logVersion] = Transactions( transactionCounts[logVersion], txId );
			  }

			  when( result.Apply( any( typeof( LogPosition ) ) ) ).thenAnswer(invocation =>
			  {
				LogPosition position = invocation.getArgument( 0 );
				if ( position == null )
				{
					 // A mockito issue when calling the "when" methods, I believe
					 return null;
				}

				// For simplicity the offset means, in this test, the array offset
				CommittedTransactionRepresentation[] transactions = logs[toIntExact( position.LogVersion )];
				CommittedTransactionRepresentation[] subset = copyOfRange( transactions, toIntExact( position.ByteOffset - baseOffset ), transactions.Length );
				ArrayUtil.reverse( subset );
				return given( subset );
			  });
			  return result;
		 }

		 private CommittedTransactionRepresentation[] Transactions( int count, AtomicLong txId )
		 {
			  CommittedTransactionRepresentation[] result = new CommittedTransactionRepresentation[count];
			  for ( int i = 0; i < count; i++ )
			  {
					CommittedTransactionRepresentation transaction = result[i] = mock( typeof( CommittedTransactionRepresentation ) );
					LogEntryCommit commitEntry = mock( typeof( LogEntryCommit ) );
					when( commitEntry.TxId ).thenReturn( txId.AndIncrement );
					when( transaction.CommitEntry ).thenReturn( commitEntry );
			  }
			  return result;
		 }
	}

}