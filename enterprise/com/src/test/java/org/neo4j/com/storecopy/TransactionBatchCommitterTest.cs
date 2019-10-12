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
	using Test = org.junit.Test;
	using InOrder = org.mockito.InOrder;

	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using KernelTransactionHandle = Org.Neo4j.Kernel.api.KernelTransactionHandle;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;
	using KernelTransactions = Org.Neo4j.Kernel.Impl.Api.KernelTransactions;
	using TestKernelTransactionHandle = Org.Neo4j.Kernel.Impl.Api.TestKernelTransactionHandle;
	using TransactionCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionToApply = Org.Neo4j.Kernel.Impl.Api.TransactionToApply;
	using PhysicalTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using AssertableLogProvider = Org.Neo4j.Logging.AssertableLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.TransactionApplicationMode.EXTERNAL;

	public class TransactionBatchCommitterTest
	{
		 private readonly KernelTransactions _kernelTransactions = mock( typeof( KernelTransactions ) );
		 private readonly TransactionCommitProcess _commitProcess = mock( typeof( TransactionCommitProcess ) );
		 private readonly AssertableLogProvider _logProvider = new AssertableLogProvider();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCommitSmallBatch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCommitSmallBatch()
		 {
			  // given
			  long safeZone = 10;
			  TransactionBatchCommitter committer = NewBatchCommitter( safeZone );

			  TransactionChain chain = CreateTxChain( 3, 1, 1 );

			  // when
			  committer.Apply( chain.First, chain.Last );

			  // then
			  verify( _commitProcess ).commit( eq( chain.First ), any(), eq(EXTERNAL) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCommitLargeBatch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCommitLargeBatch()
		 {
			  // given
			  long safeZone = 10;
			  TransactionBatchCommitter committer = NewBatchCommitter( safeZone );

			  TransactionChain chain = CreateTxChain( 100, 1, 10 );

			  // when
			  committer.Apply( chain.First, chain.Last );

			  // then
			  verify( _commitProcess ).commit( eq( chain.First ), any(), eq(EXTERNAL) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBlockTransactionsForSmallBatch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBlockTransactionsForSmallBatch()
		 {
			  // given
			  long safeZone = 10;
			  TransactionBatchCommitter committer = NewBatchCommitter( safeZone );

			  TransactionChain chain = CreateTxChain( 3, 1, 1 );

			  // when
			  committer.Apply( chain.First, chain.Last );

			  // then
			  verify( _kernelTransactions, never() ).blockNewTransactions();
			  verify( _kernelTransactions, never() ).unblockNewTransactions();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBlockTransactionsForLargeBatch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBlockTransactionsForLargeBatch()
		 {
			  // given
			  long safeZone = 10;
			  TransactionBatchCommitter committer = NewBatchCommitter( safeZone );

			  TransactionChain chain = CreateTxChain( 100, 1, 10 );

			  // when
			  committer.Apply( chain.First, chain.Last );

			  // then
			  InOrder inOrder = inOrder( _kernelTransactions );
			  inOrder.verify( _kernelTransactions ).blockNewTransactions();
			  inOrder.verify( _kernelTransactions ).unblockNewTransactions();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTerminateOutdatedTransactions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTerminateOutdatedTransactions()
		 {
			  // given
			  long safeZone = 10;
			  int txCount = 3;
			  long firstCommitTimestamp = 10;
			  long commitTimestampInterval = 2;
			  TransactionBatchCommitter committer = NewBatchCommitter( safeZone );

			  TransactionChain chain = CreateTxChain( txCount, firstCommitTimestamp, commitTimestampInterval );
			  long timestampOutsideSafeZone = chain.Last.transactionRepresentation().LatestCommittedTxWhenStarted - safeZone - 1;
			  KernelTransaction txToTerminate = NewKernelTransaction( timestampOutsideSafeZone );
			  KernelTransaction tx = NewKernelTransaction( firstCommitTimestamp - 1 );

			  when( _kernelTransactions.activeTransactions() ).thenReturn(Iterators.asSet(NewHandle(txToTerminate), NewHandle(tx)));

			  // when
			  committer.Apply( chain.First, chain.Last );

			  // then
			  verify( txToTerminate ).markForTermination( Org.Neo4j.Kernel.Api.Exceptions.Status_Transaction.Outdated );
			  verify( tx, never() ).markForTermination(any());
			  _logProvider.formattedMessageMatcher().assertContains("Marking transaction for termination");
			  _logProvider.formattedMessageMatcher().assertContains("lastCommittedTxId:" + (BASE_TX_ID + txCount - 1));
		 }

		 private KernelTransactionHandle NewHandle( KernelTransaction tx )
		 {
			  return new TestKernelTransactionHandle( tx );
		 }

		 private KernelTransaction NewKernelTransaction( long lastTransactionTimestampWhenStarted )
		 {
			  KernelTransaction txToTerminate = mock( typeof( KernelTransaction ) );
			  when( txToTerminate.LastTransactionTimestampWhenStarted() ).thenReturn(lastTransactionTimestampWhenStarted);
			  return txToTerminate;
		 }

		 private TransactionBatchCommitter NewBatchCommitter( long safeZone )
		 {
			  return new TransactionBatchCommitter( _kernelTransactions, safeZone, _commitProcess, _logProvider.getLog( typeof( TransactionBatchCommitter ) ) );
		 }

		 private TransactionChain CreateTxChain( int txCount, long firstCommitTimestamp, long commitTimestampInterval )
		 {
			  TransactionToApply first = null;
			  TransactionToApply last = null;
			  long commitTimestamp = firstCommitTimestamp;
			  for ( long i = BASE_TX_ID; i < BASE_TX_ID + txCount; i++ )
			  {
					TransactionToApply tx = tx( i, commitTimestamp );
					if ( first == null )
					{
						 first = tx;
						 last = tx;
					}
					else
					{
						 last.Next( tx );
						 last = tx;
					}
					commitTimestamp += commitTimestampInterval;
			  }
			  return new TransactionChain( this, first, last );
		 }

		 private TransactionToApply Tx( long id, long commitTimestamp )
		 {
			  PhysicalTransactionRepresentation representation = new PhysicalTransactionRepresentation( emptyList() );
			  representation.SetHeader( new sbyte[0], 0, 0, commitTimestamp - 10, id - 1, commitTimestamp, 0 );
			  return new TransactionToApply( representation, id );
		 }

		 private class TransactionChain
		 {
			 private readonly TransactionBatchCommitterTest _outerInstance;

			  internal readonly TransactionToApply First;
			  internal readonly TransactionToApply Last;

			  internal TransactionChain( TransactionBatchCommitterTest outerInstance, TransactionToApply first, TransactionToApply last )
			  {
				  this._outerInstance = outerInstance;
					this.First = first;
					this.Last = last;
			  }
		 }
	}

}