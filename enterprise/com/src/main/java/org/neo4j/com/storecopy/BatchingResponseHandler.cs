using System;

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

	using Org.Neo4j.com;
	using Handler = Org.Neo4j.com.Response.Handler;
	using Org.Neo4j.Helpers.Collection;
	using VersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using TransactionQueue = Org.Neo4j.Kernel.Impl.Api.TransactionQueue;
	using TransactionToApply = Org.Neo4j.Kernel.Impl.Api.TransactionToApply;
	using CommittedTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using Commitment = Org.Neo4j.Kernel.impl.transaction.log.Commitment;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using Log = Org.Neo4j.Logging.Log;

	/// <summary>
	/// <seealso cref="Handler Response handler"/> which commits received transactions (for transaction stream responses)
	/// in batches. Can fulfill transaction obligations.
	/// </summary>
	internal class BatchingResponseHandler : Response.Handler, Visitor<CommittedTransactionRepresentation, Exception>
	{
		 private readonly TransactionQueue _queue;
		 private readonly ResponseUnpacker_TxHandler _txHandler;
		 private readonly VersionContextSupplier _versionContextSupplier;
		 private readonly TransactionObligationFulfiller _obligationFulfiller;
		 private readonly Log _log;

		 internal BatchingResponseHandler( int maxBatchSize, TransactionQueue.Applier applier, TransactionObligationFulfiller obligationFulfiller, ResponseUnpacker_TxHandler txHandler, VersionContextSupplier versionContextSupplier, Log log )
		 {
			  this._obligationFulfiller = obligationFulfiller;
			  this._txHandler = txHandler;
			  this._versionContextSupplier = versionContextSupplier;
			  this._queue = new TransactionQueue( maxBatchSize, applier );
			  this._log = log;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void obligation(long txId) throws java.io.IOException
		 public override void Obligation( long txId )
		 {
			  if ( txId == Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID )
			  { // Means "empty" response
					return;
			  }

			  try
			  {
					_obligationFulfiller.fulfill( txId );
			  }
			  catch ( System.InvalidOperationException e )
			  {
					throw ( new ComException( "Failed to pull updates", e ) ).traceComException( _log, "BatchingResponseHandler.obligation" );
			  }
			  catch ( InterruptedException e )
			  {
					throw new IOException( e );
			  }
		 }

		 public override Visitor<CommittedTransactionRepresentation, Exception> Transactions()
		 {
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visit(org.neo4j.kernel.impl.transaction.CommittedTransactionRepresentation transaction) throws Exception
		 public override bool Visit( CommittedTransactionRepresentation transaction )
		 {
			  _queue.queue( new TransactionToApplyAnonymousInnerClass( this, transaction.TransactionRepresentation, _versionContextSupplier.VersionContext ) );
			  return false;
		 }

		 private class TransactionToApplyAnonymousInnerClass : TransactionToApply
		 {
			 private readonly BatchingResponseHandler _outerInstance;

			 public TransactionToApplyAnonymousInnerClass( BatchingResponseHandler outerInstance, Org.Neo4j.Kernel.impl.transaction.TransactionRepresentation getTransactionRepresentation, Org.Neo4j.Io.pagecache.tracing.cursor.context.VersionContext getVersionContext ) : base( getTransactionRepresentation, transaction.CommitEntry.TxId, getVersionContext )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void commitment( Commitment commitment, long transactionId )
			 {
				  // TODO Perhaps odd to override this method here just to be able to call txHandler?
				  base.commitment( commitment, transactionId );
				  _outerInstance.txHandler.accept( transactionId );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void applyQueuedTransactions() throws Exception
		 internal virtual void ApplyQueuedTransactions()
		 {
			  _queue.empty();
		 }
	}

}