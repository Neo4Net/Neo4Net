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
namespace Neo4Net.Kernel.recovery
{
	using Test = org.junit.Test;
	using Answers = org.mockito.Answers;
	using InvocationOnMock = org.mockito.invocation.InvocationOnMock;
	using Answer = org.mockito.stubbing.Answer;

	using CommittedTransactionRepresentation = Neo4Net.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using LogPosition = Neo4Net.Kernel.impl.transaction.log.LogPosition;
	using TransactionCursor = Neo4Net.Kernel.impl.transaction.log.TransactionCursor;
	using LogEntryCommit = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using ProgressReporter = Neo4Net.Kernel.impl.util.monitoring.ProgressReporter;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class RecoveryProgressIndicatorTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void reportProgressOnRecovery() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReportProgressOnRecovery()
		 {
			  RecoveryService recoveryService = mock( typeof( RecoveryService ), Answers.RETURNS_MOCKS );
			  CorruptedLogsTruncator logsTruncator = mock( typeof( CorruptedLogsTruncator ) );
			  RecoveryMonitor recoveryMonitor = mock( typeof( RecoveryMonitor ) );
			  TransactionCursor reverseTransactionCursor = mock( typeof( TransactionCursor ) );
			  TransactionCursor transactionCursor = mock( typeof( TransactionCursor ) );
			  CommittedTransactionRepresentation transactionRepresentation = mock( typeof( CommittedTransactionRepresentation ) );

			  int transactionsToRecover = 5;
			  int expectedMax = transactionsToRecover * 2;
			  int lastCommittedTransactionId = 14;
			  LogPosition recoveryStartPosition = LogPosition.start( 0 );
			  int firstTxIdAfterLastCheckPoint = 10;
			  RecoveryStartInformation startInformation = new RecoveryStartInformation( recoveryStartPosition, firstTxIdAfterLastCheckPoint );

			  when( reverseTransactionCursor.next() ).thenAnswer(new NextTransactionAnswer(transactionsToRecover));
			  when( transactionCursor.next() ).thenAnswer(new NextTransactionAnswer(transactionsToRecover));
			  when( reverseTransactionCursor.get() ).thenReturn(transactionRepresentation);
			  when( transactionCursor.get() ).thenReturn(transactionRepresentation);
			  when( transactionRepresentation.CommitEntry ).thenReturn( new LogEntryCommit( lastCommittedTransactionId, 1L ) );

			  when( recoveryService.RecoveryStartInformation ).thenReturn( startInformation );
			  when( recoveryService.GetTransactionsInReverseOrder( recoveryStartPosition ) ).thenReturn( reverseTransactionCursor );
			  when( recoveryService.GetTransactions( recoveryStartPosition ) ).thenReturn( transactionCursor );

			  AssertableProgressReporter progressReporter = new AssertableProgressReporter( expectedMax );
			  Recovery recovery = new Recovery( recoveryService, logsTruncator, new LifecycleAdapter(), recoveryMonitor, progressReporter, true );
			  Recovery.init();

			  progressReporter.Verify();
		 }

		 private class AssertableProgressReporter : ProgressReporter
		 {
			  internal readonly int ExpectedMax;
			  internal int RecoveredTransactions;
			  internal long Max;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool CompletedConflict;

			  internal AssertableProgressReporter( int expectedMax )
			  {
					this.ExpectedMax = expectedMax;
			  }

			  public override void Start( long max )
			  {
					this.Max = max;
			  }

			  public override void Progress( long add )
			  {
					RecoveredTransactions += ( int )add;
			  }

			  public override void Completed()
			  {
					CompletedConflict = true;
			  }

			  public virtual void Verify()
			  {
					assertTrue( "Progress reporting was not completed.", CompletedConflict );
					assertEquals( "Number of max recovered transactions is different.", ExpectedMax, Max );
					assertEquals( "Number of recovered transactions is different.", ExpectedMax, RecoveredTransactions );
			  }
		 }

		 private class NextTransactionAnswer : Answer<bool>
		 {
			  internal readonly int ExpectedTransactionsToRecover;
			  internal int Invocations;

			  internal NextTransactionAnswer( int expectedTransactionsToRecover )
			  {
					this.ExpectedTransactionsToRecover = expectedTransactionsToRecover;
			  }

			  public override bool? Answer( InvocationOnMock invocationOnMock )
			  {
					Invocations++;
					return Invocations <= ExpectedTransactionsToRecover;
			  }
		 }
	}

}