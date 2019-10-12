using System;
using System.Threading;

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
namespace Org.Neo4j.causalclustering.catchup.tx
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using ArgumentCaptor = org.mockito.ArgumentCaptor;

	using CommandIndexTracker = Org.Neo4j.causalclustering.core.state.machines.id.CommandIndexTracker;
	using LogIndexTxHeaderEncoding = Org.Neo4j.causalclustering.core.state.machines.tx.LogIndexTxHeaderEncoding;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using PageCursorTracerSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using TransactionCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionToApply = Org.Neo4j.Kernel.Impl.Api.TransactionToApply;
	using CommittedTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using TransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.TransactionRepresentation;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using LogEntryCommit = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.state.machines.tx.LogIndexTxHeaderEncoding.encodeLogIndexAsTxHeader;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.tracing.CommitEvent.NULL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.TransactionApplicationMode.EXTERNAL;

	public class BatchingTxApplierTest
	{
		private bool InstanceFieldsInitialized = false;

		public BatchingTxApplierTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_txApplier = new BatchingTxApplier( _maxBatchSize, () => _idStore, () => _commitProcess, new Monitors(), Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, EmptyVersionContextSupplier.EMPTY, _commandIndexTracker, NullLogProvider.Instance );
		}

		 private readonly TransactionIdStore _idStore = mock( typeof( TransactionIdStore ) );
		 private readonly TransactionCommitProcess _commitProcess = mock( typeof( TransactionCommitProcess ) );
		 private readonly CommandIndexTracker _commandIndexTracker = new CommandIndexTracker();

		 private readonly long _startTxId = 31L;
		 private readonly int _maxBatchSize = 16;

		 private BatchingTxApplier _txApplier;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  when( _idStore.LastCommittedTransactionId ).thenReturn( _startTxId );
			  _txApplier.start();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  _txApplier.stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveCorrectDefaults()
		 public virtual void ShouldHaveCorrectDefaults()
		 {
			  assertEquals( _startTxId, _txApplier.lastQueuedTxId() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyBatch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyBatch()
		 {
			  // given
			  _txApplier.queue( CreateTxWithId( _startTxId + 1 ) );
			  _txApplier.queue( CreateTxWithId( _startTxId + 2 ) );
			  _txApplier.queue( CreateTxWithId( _startTxId + 3 ) );

			  // when
			  _txApplier.applyBatch();

			  // then
			  assertEquals( _startTxId + 3, _txApplier.lastQueuedTxId() );
			  AssertTransactionsCommitted( _startTxId + 1, 3 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreOutOfOrderTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreOutOfOrderTransactions()
		 {
			  // given
			  _txApplier.queue( CreateTxWithId( _startTxId + 4 ) ); // ignored
			  _txApplier.queue( CreateTxWithId( _startTxId + 1 ) );
			  _txApplier.queue( CreateTxWithId( _startTxId + 3 ) ); // ignored
			  _txApplier.queue( CreateTxWithId( _startTxId + 2 ) );
			  _txApplier.queue( CreateTxWithId( _startTxId + 3 ) );
			  _txApplier.queue( CreateTxWithId( _startTxId + 5 ) ); // ignored
			  _txApplier.queue( CreateTxWithId( _startTxId + 5 ) ); // ignored
			  _txApplier.queue( CreateTxWithId( _startTxId + 4 ) );
			  _txApplier.queue( CreateTxWithId( _startTxId + 4 ) ); // ignored
			  _txApplier.queue( CreateTxWithId( _startTxId + 4 ) ); // ignored
			  _txApplier.queue( CreateTxWithId( _startTxId + 6 ) ); // ignored

			  // when
			  _txApplier.applyBatch();

			  // then
			  AssertTransactionsCommitted( _startTxId + 1, 4 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToQueueMaxBatchSize() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToQueueMaxBatchSize()
		 {
			  // given
			  long endTxId = _startTxId + _maxBatchSize;
			  for ( long txId = _startTxId + 1; txId <= endTxId; txId++ )
			  {
					_txApplier.queue( CreateTxWithId( txId ) );
			  }

			  // when
			  _txApplier.applyBatch();

			  // then
			  AssertTransactionsCommitted( _startTxId + 1, _maxBatchSize );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 3_000) public void shouldGiveUpQueueingOnStop() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGiveUpQueueingOnStop()
		 {
			  // given
			  for ( int i = 1; i <= _maxBatchSize; i++ ) // fell the queue
			  {
					_txApplier.queue( CreateTxWithId( _startTxId + i ) );
			  }

			  // when
			  System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( 1 );
			  Thread thread = new Thread(() =>
			  {
			  latch.Signal();
			  try
			  {
				  _txApplier.queue( CreateTxWithId( _startTxId + _maxBatchSize + 1 ) );
			  }
			  catch ( Exception e )
			  {
				  throw new Exception( e );
			  }
			  });

			  thread.Start();

			  latch.await();
			  _txApplier.stop();

			  // then we don't get stuck
			  thread.Join();
		 }

		 private CommittedTransactionRepresentation CreateTxWithId( long txId )
		 {
			  CommittedTransactionRepresentation tx = mock( typeof( CommittedTransactionRepresentation ) );
			  LogEntryCommit commitEntry = mock( typeof( LogEntryCommit ) );
			  when( commitEntry.TxId ).thenReturn( txId );
			  TransactionRepresentation txRep = mock( typeof( TransactionRepresentation ) );
			  sbyte[] encodedRaftLogIndex = encodeLogIndexAsTxHeader( txId - 5 ); // just some arbitrary offset
			  when( txRep.AdditionalHeader() ).thenReturn(encodedRaftLogIndex);
			  when( tx.TransactionRepresentation ).thenReturn( txRep );
			  when( tx.CommitEntry ).thenReturn( commitEntry );
			  return tx;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertTransactionsCommitted(long startTxId, long expectedCount) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 private void AssertTransactionsCommitted( long startTxId, long expectedCount )
		 {
			  ArgumentCaptor<TransactionToApply> batchCaptor = ArgumentCaptor.forClass( typeof( TransactionToApply ) );
			  verify( _commitProcess ).commit( batchCaptor.capture(), eq(NULL), eq(EXTERNAL) );

			  TransactionToApply batch = Iterables.single( batchCaptor.AllValues );
			  long expectedTxId = startTxId;
			  long count = 0;
			  while ( batch != null )
			  {
					assertEquals( expectedTxId, batch.TransactionId() );
					expectedTxId++;
					batch = batch.Next();
					count++;
			  }
			  assertEquals( expectedCount, count );
		 }
	}

}