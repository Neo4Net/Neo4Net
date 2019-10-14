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
namespace Neo4Net.Kernel.impl.transaction.log
{

	using TransactionMetadata = Neo4Net.Kernel.impl.transaction.log.TransactionMetadataCache.TransactionMetadata;
	using CheckPoint = Neo4Net.Kernel.impl.transaction.log.entry.CheckPoint;

	/// <summary>
	/// Accessor of transactions and meta data information about transactions.
	/// </summary>
	public interface LogicalTransactionStore
	{
		 /// <summary>
		 /// Acquires a <seealso cref="TransactionCursor cursor"/> which will provide <seealso cref="CommittedTransactionRepresentation"/>
		 /// instances for committed transactions, starting from the specified {@code transactionIdToStartFrom}.
		 /// Transactions will be returned from the cursor in transaction-id-sequential order.
		 /// </summary>
		 /// <param name="transactionIdToStartFrom"> id of the first transaction that the cursor will return. </param>
		 /// <returns> an <seealso cref="TransactionCursor"/> capable of returning <seealso cref="CommittedTransactionRepresentation"/> instances
		 /// for committed transactions, starting from the specified {@code transactionIdToStartFrom}. </returns>
		 /// <exception cref="NoSuchTransactionException"> if the requested transaction hasn't been committed,
		 /// or if the transaction has been committed, but information about it is no longer available for some reason. </exception>
		 /// <exception cref="IOException"> if there was an I/O related error looking for the start transaction. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: TransactionCursor getTransactions(long transactionIdToStartFrom) throws java.io.IOException;
		 TransactionCursor GetTransactions( long transactionIdToStartFrom );

		 /// <summary>
		 /// Acquires a <seealso cref="TransactionCursor cursor"/> which will provide <seealso cref="CommittedTransactionRepresentation"/>
		 /// instances for committed transactions, starting from the specified <seealso cref="LogPosition"/>.
		 /// This is useful for placing a cursor at a position referred to by a <seealso cref="CheckPoint"/>.
		 /// Transactions will be returned from the cursor in transaction-id-sequential order.
		 /// </summary>
		 /// <param name="position"> <seealso cref="LogPosition"/> of the first transaction that the cursor will return. </param>
		 /// <returns> an <seealso cref="TransactionCursor"/> capable of returning <seealso cref="CommittedTransactionRepresentation"/> instances
		 /// for committed transactions, starting from the specified {@code position}. </returns>
		 /// <exception cref="NoSuchTransactionException"> if the requested transaction hasn't been committed,
		 /// or if the transaction has been committed, but information about it is no longer available for some reason. </exception>
		 /// <exception cref="IOException"> if there was an I/O related error looking for the start transaction. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: TransactionCursor getTransactions(LogPosition position) throws java.io.IOException;
		 TransactionCursor GetTransactions( LogPosition position );

		 /// <summary>
		 /// Acquires a <seealso cref="TransactionCursor cursor"/> which will provide <seealso cref="CommittedTransactionRepresentation"/>
		 /// instances for committed transactions, starting from the end of the whole transaction stream
		 /// back to (and including) the transaction at <seealso cref="LogPosition"/>.
		 /// Transactions will be returned in reverse order from the end of the transaction stream and backwards in
		 /// descending order of transaction id.
		 /// </summary>
		 /// <param name="backToPosition"> <seealso cref="LogPosition"/> of the lowest (last to be returned) transaction. </param>
		 /// <returns> an <seealso cref="TransactionCursor"/> capable of returning <seealso cref="CommittedTransactionRepresentation"/> instances
		 /// for committed transactions in the given range in reverse order. </returns>
		 /// <exception cref="IOException"> if there was an I/O related error looking for the start transaction. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: TransactionCursor getTransactionsInReverseOrder(LogPosition backToPosition) throws java.io.IOException;
		 TransactionCursor GetTransactionsInReverseOrder( LogPosition backToPosition );

		 /// <summary>
		 /// Looks up meta data about a committed transaction.
		 /// </summary>
		 /// <param name="transactionId"> id of the transaction to look up meta data for. </param>
		 /// <returns> <seealso cref="TransactionMetadata"/> containing meta data about the specified transaction. </returns>
		 /// <exception cref="NoSuchTransactionException"> if the requested transaction hasn't been committed,
		 /// or if the transaction has been committed, but information about it is no longer available for some reason. </exception>
		 /// <exception cref="IOException"> if there was an I/O related error during reading the meta data. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: TransactionMetadataCache.TransactionMetadata getMetadataFor(long transactionId) throws java.io.IOException;
		 TransactionMetadataCache.TransactionMetadata GetMetadataFor( long transactionId );

		 /// <summary>
		 /// Checks if a transaction with a given transaction id exists on disk. This is to ensure that a transaction's
		 /// metadata is not found because it is cached, even if the tx has itself been pruned. </summary>
		 /// <param name="transactionId"> The id of the transaction to check. </param>
		 /// <returns> true if there is currently a transaction log file containing this transaction, false otherwise. </returns>
		 /// <exception cref="IOException"> If there was an I/O error during the lookup. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean existsOnDisk(long transactionId) throws java.io.IOException;
		 bool ExistsOnDisk( long transactionId );
	}

}