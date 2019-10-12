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
namespace Neo4Net.causalclustering.catchup.tx
{
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using Test = org.junit.Test;


	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using Neo4Net.Cursors;
	using CommittedTransactionRepresentation = Neo4Net.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using LogEntryCommit = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using NullLog = Neo4Net.Logging.NullLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.AdditionalAnswers.returnsElementsOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.CatchupResult.E_TRANSACTION_PRUNED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.CatchupResult.SUCCESS_END_OF_STREAM;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({"unchecked", "UnnecessaryLocalVariable"}) public class ChunkedTransactionStreamTest
	public class ChunkedTransactionStreamTest
	{
		 private readonly StoreId _storeId = StoreId.DEFAULT;
		 private readonly ByteBufAllocator _allocator = mock( typeof( ByteBufAllocator ) );
		 private readonly CatchupServerProtocol _protocol = mock( typeof( CatchupServerProtocol ) );
		 private readonly IOCursor<CommittedTransactionRepresentation> _cursor = mock( typeof( IOCursor ) );
		 private readonly int _baseTxId = ( int ) BASE_TX_ID;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSucceedExactNumberOfTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSucceedExactNumberOfTransactions()
		 {
			  int firstTxId = _baseTxId;
			  int lastTxId = 10;
			  int txIdPromise = 10;
			  TestTransactionStream( firstTxId, lastTxId, txIdPromise, SUCCESS_END_OF_STREAM );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSucceedWithNoTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSucceedWithNoTransactions()
		 {
			  int firstTxId = _baseTxId;
			  int lastTxId = _baseTxId;
			  int txIdPromise = _baseTxId;
			  TestTransactionStream( firstTxId, lastTxId, txIdPromise, SUCCESS_END_OF_STREAM );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSucceedExcessiveNumberOfTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSucceedExcessiveNumberOfTransactions()
		 {
			  int firstTxId = _baseTxId;
			  int lastTxId = 10;
			  int txIdPromise = 9;
			  TestTransactionStream( firstTxId, lastTxId, txIdPromise, SUCCESS_END_OF_STREAM );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIncompleteStreamOfTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailIncompleteStreamOfTransactions()
		 {
			  int firstTxId = _baseTxId;
			  int lastTxId = 10;
			  int txIdPromise = 11;
			  TestTransactionStream( firstTxId, lastTxId, txIdPromise, E_TRANSACTION_PRUNED );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSucceedLargeNumberOfTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSucceedLargeNumberOfTransactions()
		 {
			  int firstTxId = _baseTxId;
			  int lastTxId = 1000;
			  int txIdPromise = 900;
			  TestTransactionStream( firstTxId, lastTxId, txIdPromise, SUCCESS_END_OF_STREAM );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("SameParameterValue") private void testTransactionStream(int firstTxId, int lastTxId, int txIdPromise, org.neo4j.causalclustering.catchup.CatchupResult expectedResult) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private void TestTransactionStream( int firstTxId, int lastTxId, int txIdPromise, CatchupResult expectedResult )
		 {
			  ChunkedTransactionStream txStream = new ChunkedTransactionStream( NullLog.Instance, _storeId, firstTxId, txIdPromise, _cursor, _protocol );

			  IList<bool> more = new List<bool>();
			  IList<CommittedTransactionRepresentation> txs = new List<CommittedTransactionRepresentation>();

			  for ( int txId = firstTxId; txId <= lastTxId; txId++ )
			  {
					more.Add( true );
					txs.Add( Tx( txId ) );
			  }
			  txs.Add( null );
			  more.Add( false );

			  when( _cursor.next() ).thenAnswer(returnsElementsOf(more));
			  when( _cursor.get() ).thenAnswer(returnsElementsOf(txs));

			  // when/then
			  assertFalse( txStream.EndOfInput );

			  for ( int txId = firstTxId; txId <= lastTxId; txId++ )
			  {
					assertEquals( ResponseMessageType.TX, txStream.ReadChunk( _allocator ) );
					assertEquals( new TxPullResponse( _storeId, txs[txId - firstTxId] ), txStream.ReadChunk( _allocator ) );
			  }

			  assertEquals( ResponseMessageType.TX_STREAM_FINISHED, txStream.ReadChunk( _allocator ) );
			  assertEquals( new TxStreamFinishedResponse( expectedResult, lastTxId ), txStream.ReadChunk( _allocator ) );

			  assertTrue( txStream.EndOfInput );

			  // when
			  txStream.Close();

			  // then
			  verify( _cursor ).close();
		 }

		 private CommittedTransactionRepresentation Tx( int txId )
		 {
			  CommittedTransactionRepresentation tx = mock( typeof( CommittedTransactionRepresentation ) );
			  when( tx.CommitEntry ).thenReturn( new LogEntryCommit( txId, 0 ) );
			  return tx;
		 }
	}

}