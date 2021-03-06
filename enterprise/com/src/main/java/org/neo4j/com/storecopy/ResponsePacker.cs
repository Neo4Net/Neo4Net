﻿using System;

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
	using Org.Neo4j.com;
	using Org.Neo4j.com;
	using Org.Neo4j.Cursor;
	using Org.Neo4j.Helpers.Collection;
	using CommittedTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using LogicalTransactionStore = Org.Neo4j.Kernel.impl.transaction.log.LogicalTransactionStore;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using StoreId = Org.Neo4j.Storageengine.Api.StoreId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;

	public class ResponsePacker
	{
		 protected internal readonly LogicalTransactionStore TransactionStore;
		 protected internal readonly System.Func<StoreId> StoreId; // for lazy storeId getter
		 private readonly TransactionIdStore _transactionIdStore;

		 public ResponsePacker( LogicalTransactionStore transactionStore, TransactionIdStore transactionIdStore, System.Func<StoreId> storeId )
		 {
			  this.TransactionStore = transactionStore;
			  this._transactionIdStore = transactionIdStore;
			  this.StoreId = storeId;
		 }

		 public virtual Response<T> PackTransactionStreamResponse<T>( RequestContext context, T response )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long toStartFrom = context.lastAppliedTransaction() + 1;
			  long toStartFrom = context.LastAppliedTransaction() + 1;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long toEndAt = transactionIdStore.getLastCommittedTransactionId();
			  long toEndAt = _transactionIdStore.LastCommittedTransactionId;
			  TransactionStream transactions = visitor =>
			  {
				// Check so that it's even worth thinking about extracting any transactions at all
				if ( toStartFrom > BASE_TX_ID && toStartFrom <= toEndAt )
				{
					 ExtractTransactions( toStartFrom, FilterVisitor( visitor, toEndAt ) );
				}
			  };
			  return new TransactionStreamResponse<T>( response, StoreId.get(), transactions, Org.Neo4j.com.ResourceReleaser_Fields.NoOp );
		 }

		 public virtual Response<T> PackTransactionObligationResponse<T>( RequestContext context, T response )
		 {
			  return PackTransactionObligationResponse( context, response, _transactionIdStore.LastCommittedTransactionId );
		 }

		 public virtual Response<T> PackTransactionObligationResponse<T>( RequestContext context, T response, long obligationTxId )
		 {
			  return new TransactionObligationResponse<T>( response, StoreId.get(), obligationTxId, Org.Neo4j.com.ResourceReleaser_Fields.NoOp );
		 }

		 public virtual Response<T> PackEmptyResponse<T>( T response )
		 {
			  return new TransactionObligationResponse<T>( response, StoreId.get(), Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID, Org.Neo4j.com.ResourceReleaser_Fields.NoOp );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected org.neo4j.helpers.collection.Visitor<org.neo4j.kernel.impl.transaction.CommittedTransactionRepresentation,Exception> filterVisitor(final org.neo4j.helpers.collection.Visitor<org.neo4j.kernel.impl.transaction.CommittedTransactionRepresentation,Exception> delegate, final long txToEndAt)
		 protected internal virtual Visitor<CommittedTransactionRepresentation, Exception> FilterVisitor( Visitor<CommittedTransactionRepresentation, Exception> @delegate, long txToEndAt )
		 {
			  return element => element.CommitEntry.TxId <= txToEndAt && @delegate.Visit( element );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void extractTransactions(long startingAtTransactionId, org.neo4j.helpers.collection.Visitor<org.neo4j.kernel.impl.transaction.CommittedTransactionRepresentation,Exception> visitor) throws Exception
		 protected internal virtual void ExtractTransactions( long startingAtTransactionId, Visitor<CommittedTransactionRepresentation, Exception> visitor )
		 {
			  using ( IOCursor<CommittedTransactionRepresentation> cursor = TransactionStore.getTransactions( startingAtTransactionId ) )
			  {
					while ( cursor.next() && !visitor.Visit(cursor.get()) )
					{
					}
			  }
		 }
	}

}