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
namespace Neo4Net.Kernel.Impl.Api
{
	using Test = org.junit.Test;


	using TransactionFailureException = Neo4Net.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using TransactionRepresentation = Neo4Net.Kernel.impl.transaction.TransactionRepresentation;
	using FakeCommitment = Neo4Net.Kernel.impl.transaction.log.FakeCommitment;
	using PhysicalTransactionRepresentation = Neo4Net.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using TestableTransactionAppender = Neo4Net.Kernel.impl.transaction.log.TestableTransactionAppender;
	using TransactionAppender = Neo4Net.Kernel.impl.transaction.log.TransactionAppender;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using CommitEvent = Neo4Net.Kernel.impl.transaction.tracing.CommitEvent;
	using LogAppendEvent = Neo4Net.Kernel.impl.transaction.tracing.LogAppendEvent;
	using StorageEngine = Neo4Net.Storageengine.Api.StorageEngine;
	using TransactionApplicationMode = Neo4Net.Storageengine.Api.TransactionApplicationMode;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Exceptions.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.TransactionApplicationMode.INTERNAL;

	public class TransactionRepresentationCommitProcessTest
	{
		 private readonly CommitEvent _commitEvent = CommitEvent.NULL;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailWithProperMessageOnAppendException() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailWithProperMessageOnAppendException()
		 {
			  // GIVEN
			  TransactionAppender appender = mock( typeof( TransactionAppender ) );
			  IOException rootCause = new IOException( "Mock exception" );
			  doThrow( new IOException( rootCause ) ).when( appender ).append( any( typeof( TransactionToApply ) ), any( typeof( LogAppendEvent ) ) );
			  StorageEngine storageEngine = mock( typeof( StorageEngine ) );
			  TransactionCommitProcess commitProcess = new TransactionRepresentationCommitProcess( appender, storageEngine );

			  // WHEN
			  try
			  {
					commitProcess.Commit( MockedTransaction(), _commitEvent, INTERNAL );
					fail( "Should have failed, something is wrong with the mocking in this test" );
			  }
			  catch ( TransactionFailureException e )
			  {
					assertThat( e.Message, containsString( "Could not append transaction representation to log" ) );
					assertTrue( contains( e, rootCause.Message, rootCause.GetType() ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseTransactionRegardlessOfWhetherOrNotItAppliedCorrectly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseTransactionRegardlessOfWhetherOrNotItAppliedCorrectly()
		 {
			  // GIVEN
			  TransactionIdStore transactionIdStore = mock( typeof( TransactionIdStore ) );
			  TransactionAppender appender = new TestableTransactionAppender( transactionIdStore );
			  long txId = 11;
			  when( transactionIdStore.NextCommittingTransactionId() ).thenReturn(txId);
			  IOException rootCause = new IOException( "Mock exception" );
			  StorageEngine storageEngine = mock( typeof( StorageEngine ) );
			  doThrow( new IOException( rootCause ) ).when( storageEngine ).apply( any( typeof( TransactionToApply ) ), any( typeof( TransactionApplicationMode ) ) );
			  TransactionCommitProcess commitProcess = new TransactionRepresentationCommitProcess( appender, storageEngine );
			  TransactionToApply transaction = MockedTransaction();

			  // WHEN
			  try
			  {
					commitProcess.Commit( transaction, _commitEvent, INTERNAL );
			  }
			  catch ( TransactionFailureException e )
			  {
					assertThat( e.Message, containsString( "Could not apply the transaction to the store" ) );
					assertTrue( contains( e, rootCause.Message, rootCause.GetType() ) );
			  }

			  // THEN
			  // we can't verify transactionCommitted since that's part of the TransactionAppender, which we have mocked
			  verify( transactionIdStore, times( 1 ) ).transactionClosed( eq( txId ), anyLong(), anyLong() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSuccessfullyCommitTransactionWithNoCommands() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSuccessfullyCommitTransactionWithNoCommands()
		 {
			  // GIVEN
			  long txId = 11;
			  long commitTimestamp = DateTimeHelper.CurrentUnixTimeMillis();
			  TransactionIdStore transactionIdStore = mock( typeof( TransactionIdStore ) );
			  TransactionAppender appender = new TestableTransactionAppender( transactionIdStore );
			  when( transactionIdStore.NextCommittingTransactionId() ).thenReturn(txId);

			  StorageEngine storageEngine = mock( typeof( StorageEngine ) );

			  TransactionCommitProcess commitProcess = new TransactionRepresentationCommitProcess( appender, storageEngine );
			  PhysicalTransactionRepresentation noCommandTx = new PhysicalTransactionRepresentation( Collections.emptyList() );
			  noCommandTx.SetHeader( new sbyte[0], -1, -1, -1, -1, -1, -1 );

			  // WHEN

			  commitProcess.Commit( new TransactionToApply( noCommandTx ), _commitEvent, INTERNAL );

			  verify( transactionIdStore ).transactionCommitted( txId, FakeCommitment.CHECKSUM, FakeCommitment.TIMESTAMP );
		 }

		 private TransactionToApply MockedTransaction()
		 {
			  TransactionRepresentation transaction = mock( typeof( TransactionRepresentation ) );
			  when( transaction.AdditionalHeader() ).thenReturn(new sbyte[0]);
			  return new TransactionToApply( transaction );
		 }
	}

}