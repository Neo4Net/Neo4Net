using System.Threading;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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

	using IndexCommand = Neo4Net.Kernel.impl.index.IndexCommand;
	using IndexConfigStore = Neo4Net.Kernel.impl.index.IndexConfigStore;
	using IdOrderingQueue = Neo4Net.Kernel.impl.util.IdOrderingQueue;
	using CommandsToApply = Neo4Net.Kernel.Api.StorageEngine.CommandsToApply;
	using TransactionApplicationMode = Neo4Net.Kernel.Api.StorageEngine.TransactionApplicationMode;

	/// <summary>
	/// This class reuses the same <seealso cref="ExplicitIndexTransactionApplier"/> for all transactions in the batch for performance
	/// reasons. The <seealso cref="ExplicitIndexTransactionApplier"/> contains appliers specific for each <seealso cref="IndexCommand"/> which
	/// are closed here on the batch level in <seealso cref="close()"/>, before the last transaction locks are released.
	/// </summary>
	public class ExplicitBatchIndexApplier : BatchTransactionApplier_Adapter
	{
		 private readonly IdOrderingQueue _transactionOrdering;
		 private readonly TransactionApplicationMode _mode;
		 private readonly IndexConfigStore _indexConfigStore;
		 private readonly ExplicitIndexApplierLookup _applierLookup;

		 // There are some expensive lookups made in the TransactionApplier, so cache it
		 private ExplicitIndexTransactionApplier _txApplier;
		 private long _lastTransactionId = -1;

		 public ExplicitBatchIndexApplier( IndexConfigStore indexConfigStore, ExplicitIndexApplierLookup applierLookup, IdOrderingQueue transactionOrdering, TransactionApplicationMode mode )
		 {
			  this._indexConfigStore = indexConfigStore;
			  this._applierLookup = applierLookup;
			  this._transactionOrdering = transactionOrdering;
			  this._mode = mode;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public TransactionApplier startTx(Neo4Net.Kernel.Api.StorageEngine.CommandsToApply transaction) throws java.io.IOException
		 public override TransactionApplier StartTx( CommandsToApply transaction )
		 {
			  long activeTransactionId = transaction.TransactionId();
			  try
			  {
					// Cache transactionApplier because it has some expensive lookups
					if ( _txApplier == null )
					{
						 _txApplier = new ExplicitIndexTransactionApplier( _applierLookup, _indexConfigStore, _mode, _transactionOrdering );
					}

					if ( transaction.RequiresApplicationOrdering() )
					{
						 // Index operations must preserve order so wait for previous tx to finish
						 _transactionOrdering.waitFor( activeTransactionId );
						 // And set current tx so we can notify the next transaction when we are finished
						 if ( transaction.Next() != null )
						 {
							  // Let each transaction notify the next
							  _txApplier.TransactionId = activeTransactionId;
						 }
						 else
						 {
							  // except the last transaction, which notifies that it is done after appliers have been closed
							  _lastTransactionId = activeTransactionId;
						 }
					}

					return _txApplier;
			  }
			  catch ( InterruptedException e )
			  {
					Thread.CurrentThread.Interrupt();
					throw new IOException( "Interrupted while waiting for applying tx:" + activeTransactionId + " explicit index updates", e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
		 public override void Close()
		 {
			  if ( _txApplier == null )
			  {
					// Never started a transaction, so nothing to do
					return;
			  }

			  foreach ( TransactionApplier applier in _txApplier.applierByProvider.Values )
			  {
					applier.close();
			  }

			  // Allow other batches to run
			  if ( _lastTransactionId != -1 )
			  {
					_transactionOrdering.removeChecked( _lastTransactionId );
					_lastTransactionId = -1;
			  }
		 }
	}

}