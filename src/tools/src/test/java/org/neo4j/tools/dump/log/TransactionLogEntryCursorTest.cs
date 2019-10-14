using System.Collections.Generic;

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
namespace Neo4Net.tools.dump.log
{
	using Test = org.junit.Test;


	using Neo4Net.Kernel.impl.transaction.log;
	using LogEntry = Neo4Net.Kernel.impl.transaction.log.entry.LogEntry;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.arrayWithSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogEntryByteCodes.CHECK_POINT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogEntryByteCodes.COMMAND;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogEntryByteCodes.TX_COMMIT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogEntryByteCodes.TX_START;

	public class TransactionLogEntryCursorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeliverIntactTransactions() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDeliverIntactTransactions()
		 {
			  // GIVEN
			  // tx 1
			  IList<LogEntry> tx1 = MakeTransaction( TX_START, COMMAND, TX_COMMIT );

			  // tx 2
			  IList<LogEntry> tx2 = MakeTransaction( TX_START, COMMAND, COMMAND, TX_COMMIT );

			  // All transactions

			  // The cursor
			  TransactionLogEntryCursor transactionCursor = GetTransactionLogEntryCursor( tx1, tx2 );

			  // THEN
			  // tx1
			  assertTrue( transactionCursor.Next() );
			  AssertTx( tx1, transactionCursor.Get() );

			  // tx2
			  assertTrue( transactionCursor.Next() );
			  AssertTx( tx2, transactionCursor.Get() );

			  // No more transactions
			  assertFalse( transactionCursor.Next() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deliverTransactionsWithoutEnd() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DeliverTransactionsWithoutEnd()
		 {
			  // GIVEN
			  // tx 1
			  IList<LogEntry> tx1 = MakeTransaction( TX_START, COMMAND, COMMAND, COMMAND, TX_COMMIT );

			  // tx 2
			  IList<LogEntry> tx2 = MakeTransaction( TX_START, COMMAND, COMMAND );

			  TransactionLogEntryCursor transactionCursor = GetTransactionLogEntryCursor( tx1, tx2 );

			  // THEN
			  assertTrue( transactionCursor.Next() );
			  AssertTx( tx1, transactionCursor.Get() );

			  assertTrue( transactionCursor.Next() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readNonTransactionalEntries() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReadNonTransactionalEntries()
		 {
			  IList<LogEntry> recordSet1 = MakeTransaction( CHECK_POINT, CHECK_POINT, CHECK_POINT );
			  IList<LogEntry> recordSet2 = MakeTransaction( CHECK_POINT );
			  TransactionLogEntryCursor transactionCursor = GetTransactionLogEntryCursor( recordSet1, recordSet2 );

			  for ( int i = 0; i < 4; i++ )
			  {
					assertTrue( transactionCursor.Next() );
					assertThat( transactionCursor.Get(), arrayWithSize(1) );
					assertThat( transactionCursor.Get()[0].Type, equalTo(CHECK_POINT) );
			  }
		 }

		 private TransactionLogEntryCursor GetTransactionLogEntryCursor( params IList<LogEntry>[] txEntries )
		 {
			  return new TransactionLogEntryCursor( new ArrayIOCursor( TransactionsAsArray( txEntries ) ) );
		 }

		 private LogEntry[] TransactionsAsArray( params IList<LogEntry>[] transactions )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return Stream.of( transactions ).flatMap( System.Collections.ICollection.stream ).toArray( LogEntry[]::new );
		 }

		 private void AssertTx( IList<LogEntry> expected, LogEntry[] actual )
		 {
			  assertArrayEquals( expected.ToArray(), actual );
		 }

		 private IList<LogEntry> MakeTransaction( params sbyte[] types )
		 {
			  IList<LogEntry> transaction = new List<LogEntry>( types.Length );
			  foreach ( sbyte? type in types )
			  {
					transaction.Add( MockedLogEntry( type.Value ) );
			  }
			  return transaction;
		 }

		 private static LogEntry MockedLogEntry( sbyte type )
		 {
			  LogEntry logEntry = mock( typeof( LogEntry ) );
			  when( logEntry.Type ).thenReturn( type );
			  return logEntry;
		 }
	}


}