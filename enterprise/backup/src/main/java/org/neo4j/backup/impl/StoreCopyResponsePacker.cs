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
namespace Org.Neo4j.backup.impl
{

	using RequestContext = Org.Neo4j.com.RequestContext;
	using ResourceReleaser = Org.Neo4j.com.ResourceReleaser;
	using Org.Neo4j.com;
	using TransactionStream = Org.Neo4j.com.TransactionStream;
	using Org.Neo4j.com;
	using ResponsePacker = Org.Neo4j.com.storecopy.ResponsePacker;
	using StoreCopyServer = Org.Neo4j.com.storecopy.StoreCopyServer;
	using Monitor = Org.Neo4j.com.storecopy.StoreCopyServer.Monitor;
	using Org.Neo4j.Helpers.Collection;
	using CommittedTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using LogFileInformation = Org.Neo4j.Kernel.impl.transaction.log.LogFileInformation;
	using LogicalTransactionStore = Org.Neo4j.Kernel.impl.transaction.log.LogicalTransactionStore;
	using NoSuchTransactionException = Org.Neo4j.Kernel.impl.transaction.log.NoSuchTransactionException;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using StoreId = Org.Neo4j.Storageengine.Api.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;

	/// <summary>
	/// In full backup we're more tolerant about missing transactions. Just as long as we fulfill this criterion:
	/// We must be able to find and stream transactions that has happened since the start of the full backup.
	/// Full backup will stream at most N transactions back, even if nothing happened during the backup.
	/// Streaming these transaction aren't as important, since they are mostly a nice-to-have.
	/// </summary>
	internal class StoreCopyResponsePacker : ResponsePacker
	{
		 private readonly long _mandatoryStartTransactionId;
		 private readonly LogFileInformation _logFileInformation;
		 private readonly TransactionIdStore _transactionIdStore;
		 private readonly StoreCopyServer.Monitor _monitor;

		 internal StoreCopyResponsePacker( LogicalTransactionStore transactionStore, TransactionIdStore transactionIdStore, LogFileInformation logFileInformation, System.Func<StoreId> storeId, long mandatoryStartTransactionId, StoreCopyServer.Monitor monitor ) : base( transactionStore, transactionIdStore, storeId )
		 {
			  this._transactionIdStore = transactionIdStore;
			  this._mandatoryStartTransactionId = mandatoryStartTransactionId;
			  this._logFileInformation = logFileInformation;
			  this._monitor = monitor;
		 }

		 public override Response<T> PackTransactionStreamResponse<T>( RequestContext context, T response )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String packerIdentifier = Thread.currentThread().getName();
			  string packerIdentifier = Thread.CurrentThread.Name;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long toStartFrom = mandatoryStartTransactionId;
			  long toStartFrom = _mandatoryStartTransactionId;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long toEndAt = transactionIdStore.getLastCommittedTransactionId();
			  long toEndAt = _transactionIdStore.LastCommittedTransactionId;
			  TransactionStream transactions = visitor =>
			  {
				// Check so that it's even worth thinking about extracting any transactions at all
				if ( toStartFrom > BASE_TX_ID && toStartFrom <= toEndAt )
				{
					 _monitor.startStreamingTransactions( toStartFrom, packerIdentifier );
					 ExtractTransactions( toStartFrom, FilterVisitor( visitor, toEndAt ) );
					 _monitor.finishStreamingTransactions( toEndAt, packerIdentifier );
				}
			  };
			  return new TransactionStreamResponse<T>( response, StoreId.get(), transactions, Org.Neo4j.com.ResourceReleaser_Fields.NoOp );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void extractTransactions(long startingAtTransactionId, org.neo4j.helpers.collection.Visitor<org.neo4j.kernel.impl.transaction.CommittedTransactionRepresentation,Exception> accumulator) throws Exception
		 protected internal override void ExtractTransactions( long startingAtTransactionId, Visitor<CommittedTransactionRepresentation, Exception> accumulator )
		 {
			  try
			  {
					startingAtTransactionId = Math.Min( _mandatoryStartTransactionId, startingAtTransactionId );
					base.ExtractTransactions( startingAtTransactionId, accumulator );
			  }
			  catch ( NoSuchTransactionException e )
			  {
					// We no longer have transactions that far back. Which transaction is the farthest back?
					if ( startingAtTransactionId < _mandatoryStartTransactionId )
					{
						 // We don't necessarily need to ask that far back. Ask which is the oldest transaction in the log(s)
						 // that we can possibly serve
						 long oldestExistingTransactionId = _logFileInformation.FirstExistingEntryId;
						 if ( oldestExistingTransactionId == -1 )
						 {
							  // Seriously, there are no logs that we can serve?
							  if ( _mandatoryStartTransactionId >= _transactionIdStore.LastCommittedTransactionId )
							  {
									// Although there are no mandatory transactions to stream, so we're good here.
									return;
							  }

							  // We are required to serve one or more transactions, but there are none, tell that
							  throw e;
						 }

						 if ( oldestExistingTransactionId <= _mandatoryStartTransactionId )
						 {
							  base.ExtractTransactions( oldestExistingTransactionId, accumulator );
						 }

						 // We can't serve the mandatory transactions, tell that
						 throw e;
					}
			  }
		 }
	}

}