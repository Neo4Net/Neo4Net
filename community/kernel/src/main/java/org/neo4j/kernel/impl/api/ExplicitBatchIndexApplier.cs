﻿using System.Threading;

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
namespace Org.Neo4j.Kernel.Impl.Api
{

	using IndexCommand = Org.Neo4j.Kernel.impl.index.IndexCommand;
	using IndexConfigStore = Org.Neo4j.Kernel.impl.index.IndexConfigStore;
	using IdOrderingQueue = Org.Neo4j.Kernel.impl.util.IdOrderingQueue;
	using CommandsToApply = Org.Neo4j.Storageengine.Api.CommandsToApply;
	using TransactionApplicationMode = Org.Neo4j.Storageengine.Api.TransactionApplicationMode;

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
//ORIGINAL LINE: public TransactionApplier startTx(org.neo4j.storageengine.api.CommandsToApply transaction) throws java.io.IOException
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