﻿using System.Collections.Generic;

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
namespace Org.Neo4j.com.storecopy
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using InOrder = org.mockito.InOrder;


	using Org.Neo4j.com;
	using Org.Neo4j.com;
	using Org.Neo4j.com;
	using Dependencies = Org.Neo4j.com.storecopy.TransactionCommittingResponseUnpacker.Dependencies;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using KernelTransactions = Org.Neo4j.Kernel.Impl.Api.KernelTransactions;
	using TransactionCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionToApply = Org.Neo4j.Kernel.Impl.Api.TransactionToApply;
	using CommittedTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using PhysicalTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using LogEntryCommit = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryCommit;
	using LogEntryStart = Org.Neo4j.Kernel.impl.transaction.log.entry.LogEntryStart;
	using CommitEvent = Org.Neo4j.Kernel.impl.transaction.tracing.CommitEvent;
	using LifeRule = Org.Neo4j.Kernel.Lifecycle.LifeRule;
	using NullLogService = Org.Neo4j.Logging.@internal.NullLogService;
	using StoreId = Org.Neo4j.Storageengine.Api.StoreId;
	using TransactionApplicationMode = Org.Neo4j.Storageengine.Api.TransactionApplicationMode;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.storecopy.ResponseUnpacker_TxHandler_Fields.NO_OP_TX_HANDLER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.LogPosition.UNSPECIFIED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;

	public class TransactionCommittingResponseUnpackerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.kernel.lifecycle.LifeRule life = new org.neo4j.kernel.lifecycle.LifeRule(true);
		 public readonly LifeRule Life = new LifeRule( true );

		 /*
		   * Tests that we unfreeze active transactions after commit and after apply of batch if batch length (in time)
		   * is larger than safeZone time.
		   */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUnfreezeKernelTransactionsAfterApplyIfBatchIsLarge() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUnfreezeKernelTransactionsAfterApplyIfBatchIsLarge()
		 {
			  // GIVEN
			  int maxBatchSize = 10;
			  long idReuseSafeZoneTime = 100;
			  Dependencies dependencies = mock( typeof( Dependencies ) );
			  TransactionObligationFulfiller fulfiller = mock( typeof( TransactionObligationFulfiller ) );
			  when( dependencies.ObligationFulfiller() ).thenReturn(fulfiller);
			  when( dependencies.LogService() ).thenReturn(NullLogService.Instance);
			  when( dependencies.VersionContextSupplier() ).thenReturn(EmptyVersionContextSupplier.EMPTY);
			  KernelTransactions kernelTransactions = mock( typeof( KernelTransactions ) );
			  when( dependencies.KernelTransactions() ).thenReturn(kernelTransactions);
			  TransactionCommitProcess commitProcess = mock( typeof( TransactionCommitProcess ) );
			  when( dependencies.CommitProcess() ).thenReturn(commitProcess);
			  TransactionCommittingResponseUnpacker unpacker = Life.add( new TransactionCommittingResponseUnpacker( dependencies, maxBatchSize, idReuseSafeZoneTime ) );

			  // WHEN
			  int txCount = maxBatchSize;
			  int doesNotMatter = 1;
			  unpacker.UnpackResponse( new DummyTransactionResponse( doesNotMatter, txCount, idReuseSafeZoneTime + 1 ), NO_OP_TX_HANDLER );

			  // THEN
			  InOrder inOrder = inOrder( commitProcess, kernelTransactions );
			  inOrder.verify( commitProcess, times( 1 ) ).commit( any(), any(), any() );
			  inOrder.verify( kernelTransactions, times( 1 ) ).unblockNewTransactions();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAwaitTransactionObligationsToBeFulfilled() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAwaitTransactionObligationsToBeFulfilled()
		 {
			  // GIVEN
			  Dependencies dependencies = mock( typeof( Dependencies ) );
			  TransactionObligationFulfiller fulfiller = mock( typeof( TransactionObligationFulfiller ) );
			  when( dependencies.ObligationFulfiller() ).thenReturn(fulfiller);
			  when( dependencies.LogService() ).thenReturn(NullLogService.Instance);
			  TransactionCommittingResponseUnpacker unpacker = Life.add( new TransactionCommittingResponseUnpacker( dependencies, 10, 0 ) );

			  // WHEN
			  unpacker.UnpackResponse( new DummyObligationResponse( 4 ), NO_OP_TX_HANDLER );

			  // THEN
			  verify( fulfiller, times( 1 ) ).fulfill( 4L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCommitTransactionsInBatches() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCommitTransactionsInBatches()
		 {
			  // GIVEN
			  Dependencies dependencies = mock( typeof( Dependencies ) );
			  TransactionCountingTransactionCommitProcess commitProcess = new TransactionCountingTransactionCommitProcess( this );
			  when( dependencies.CommitProcess() ).thenReturn(commitProcess);
			  when( dependencies.LogService() ).thenReturn(NullLogService.Instance);
			  when( dependencies.VersionContextSupplier() ).thenReturn(EmptyVersionContextSupplier.EMPTY);
			  KernelTransactions kernelTransactions = mock( typeof( KernelTransactions ) );
			  when( dependencies.KernelTransactions() ).thenReturn(kernelTransactions);
			  TransactionCommittingResponseUnpacker unpacker = Life.add( new TransactionCommittingResponseUnpacker( dependencies, 5, 0 ) );

			  // WHEN
			  unpacker.UnpackResponse( new DummyTransactionResponse( BASE_TX_ID + 1, 7 ), NO_OP_TX_HANDLER );

			  // THEN
			  commitProcess.AssertBatchSize( 5 );
			  commitProcess.AssertBatchSize( 2 );
			  commitProcess.AssertNoMoreBatches();
		 }

		 private class DummyObligationResponse : TransactionObligationResponse<object>
		 {
			  internal DummyObligationResponse( long obligationTxId ) : base( new object(), StoreId.DEFAULT, obligationTxId, org.neo4j.com.ResourceReleaser_Fields.NoOp )
			  {
			  }
		 }

		 private class DummyTransactionResponse : TransactionStreamResponse<object>
		 {
			  internal const long UNDEFINED_BATCH_LENGTH = -1;

			  internal readonly long StartingAtTxId;
			  internal readonly int TxCount;
			  internal readonly long BatchLength;

			  internal DummyTransactionResponse( long startingAtTxId, int txCount ) : this( startingAtTxId, txCount, UNDEFINED_BATCH_LENGTH )
			  {
			  }

			  internal DummyTransactionResponse( long startingAtTxId, int txCount, long batchLength ) : base( new object(), StoreId.DEFAULT, mock(typeof(TransactionStream)), org.neo4j.com.ResourceReleaser_Fields.NoOp )
			  {
					this.StartingAtTxId = startingAtTxId;
					this.TxCount = txCount;
					this.BatchLength = batchLength;
			  }

			  internal virtual CommittedTransactionRepresentation Tx( long id, long commitTimestamp )
			  {
					PhysicalTransactionRepresentation representation = new PhysicalTransactionRepresentation( emptyList() );
					representation.SetHeader( new sbyte[0], 0, 0, commitTimestamp - 10, id - 1, commitTimestamp, 0 );

					return new CommittedTransactionRepresentation( new LogEntryStart( 0, 0, 0, 0, new sbyte[0], UNSPECIFIED ), representation, new LogEntryCommit( id, commitTimestamp ) );
			  }

			  internal virtual long Timestamp( int txNbr, int txCount, long batchLength )
			  {
					if ( txCount == 1 )
					{
						 return 0;
					}
					return txNbr * batchLength / ( txCount - 1 );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void accept(org.neo4j.com.Response.Handler handler) throws Exception
			  public override void Accept( Response.Handler handler )
			  {
					for ( int i = 0; i < TxCount; i++ )
					{
						 handler.Transactions().visit(Tx(StartingAtTxId + i, Timestamp(i, TxCount, BatchLength)));
					}
			  }
		 }

		 public class TransactionCountingTransactionCommitProcess : TransactionCommitProcess
		 {
			 private readonly TransactionCommittingResponseUnpackerTest _outerInstance;

			 public TransactionCountingTransactionCommitProcess( TransactionCommittingResponseUnpackerTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal readonly LinkedList<int> BatchSizes = new LinkedList<int>();

			  public override long Commit( TransactionToApply batch, CommitEvent commitEvent, TransactionApplicationMode mode )
			  {
					int batchSize = Count( batch );
					BatchSizes.AddLast( batchSize );
					return 42;
			  }

			  protected internal virtual void AssertBatchSize( int expected )
			  {
					int batchSize = BatchSizes.RemoveFirst();
					assertEquals( expected, batchSize );
			  }

			  protected internal virtual void AssertNoMoreBatches()
			  {
					assertTrue( BatchSizes.Count == 0 );
			  }

			  internal virtual int Count( TransactionToApply batch )
			  {
					int count = 0;
					while ( batch != null )
					{
						 count++;
						 batch = batch.Next();
					}
					return count;
			  }
		 }
	}

}