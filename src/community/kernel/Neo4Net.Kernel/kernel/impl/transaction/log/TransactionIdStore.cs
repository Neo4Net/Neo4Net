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

	using TransactionId = Neo4Net.Kernel.impl.store.TransactionId;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.transaction.log.entry.LogHeader.LOG_HEADER_SIZE;

	/// <summary>
	/// Keeps a latest transaction id. There's one counter for {@code committed transaction id} and one for
	/// {@code closed transaction id}. The committed transaction id is for writing into a log before making
	/// the changes to be made. After that the application of those transactions might be asynchronous and
	/// completion of those are marked using <seealso cref="transactionClosed(long, long, long)"/>.
	/// <para>
	/// A transaction ID passes through a <seealso cref="TransactionIdStore"/> like this:
	/// <ol>
	/// <li><seealso cref="nextCommittingTransactionId()"/> is called and an id is returned to a committer.
	/// At this point that id isn't visible from any getter.</li>
	/// <li><seealso cref="transactionCommitted(long, long, long)"/> is called with this id after the fact that the transaction
	/// has been committed, i.e. written forcefully to a log. After this call the id may be visible from
	/// <seealso cref="getLastCommittedTransactionId()"/> if all ids before it have also been committed.</li>
	/// <li><seealso cref="transactionClosed(long, long, long)"/> is called with this id again, this time after all changes the
	/// transaction imposes have been applied to the store.
	/// </ol>
	/// </para>
	/// </summary>
	public interface TransactionIdStore
	{
		 /// <summary>
		 /// Tx id counting starting from this value (this value means no transaction ever committed).
		 /// 
		 /// Note that a read only transaction will get txId = 0, see <seealso cref="org.Neo4Net.Kernel.Api.Internal.Transaction"/>.
		 /// </summary>

		 /// <summary>
		 /// Timestamp value used initially for an empty database.
		 /// </summary>

		 /// <summary>
		 /// CONSTANT FOR UNKNOWN TX CHECKSUM
		 /// </summary>

		 /// <summary>
		 /// Timestamp value used when record in the metadata store is not present and there are no transactions in logs.
		 /// </summary>

		 /// <returns> the next transaction id for a committing transaction. The transaction id is incremented
		 /// with each call. Ids returned from this method will not be visible from <seealso cref="getLastCommittedTransactionId()"/>
		 /// until handed to <seealso cref="transactionCommitted(long, long, long)"/>. </returns>
		 long NextCommittingTransactionId();

		 /// <returns> the transaction id of last committing transaction. </returns>
		 long CommittingTransactionId();

		 /// <summary>
		 /// Signals that a transaction with the given transaction id has been committed (i.e. appended to a log).
		 /// Calls to this method may come in out-of-transaction-id order. The highest transaction id
		 /// seen given to this method will be visible in <seealso cref="getLastCommittedTransactionId()"/>. </summary>
		 /// <param name="transactionId"> the applied transaction id. </param>
		 /// <param name="checksum"> checksum of the transaction. </param>
		 /// <param name="commitTimestamp"> the timestamp of the transaction commit. </param>
		 void TransactionCommitted( long transactionId, long checksum, long commitTimestamp );

		 /// <returns> highest seen <seealso cref="transactionCommitted(long, long, long) committed transaction id"/>. </returns>
		 long LastCommittedTransactionId { get; }

		 /// <summary>
		 /// Returns transaction information about the highest committed transaction, i.e.
		 /// transaction id as well as checksum.
		 /// </summary>
		 /// <returns> <seealso cref="TransactionId"/> describing the last (i.e. highest) committed transaction. </returns>
		 TransactionId LastCommittedTransaction { get; }

		 /// <summary>
		 /// Returns transaction information about transaction where the last upgrade was performed, i.e.
		 /// transaction id as well as checksum.
		 /// </summary>
		 /// <returns> <seealso cref="TransactionId"/> describing the most recent upgrade transaction. </returns>
		 TransactionId UpgradeTransaction { get; }

		 /// <returns> highest seen gap-free <seealso cref="transactionClosed(long, long, long) closed transaction id"/>. </returns>
		 long LastClosedTransactionId { get; }

		 /// <summary>
		 /// Awaits gap-free <seealso cref="transactionClosed(long, long, long) closed transaction id"/>.
		 /// </summary>
		 /// <param name="txId"> the awaited transaction id. </param>
		 /// <param name="timeoutMillis"> the time to wait for it. </param>
		 /// <exception cref="InterruptedException"> interrupted. </exception>
		 /// <exception cref="TimeoutException"> timed out. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void awaitClosedTransactionId(long txId, long timeoutMillis) throws InterruptedException, java.util.concurrent.TimeoutException;
		 void AwaitClosedTransactionId( long txId, long timeoutMillis );

		 /// <summary>
		 /// Returns transaction information about the last committed transaction, i.e.
		 /// transaction id as well as the log position following the commit entry in the transaction log.
		 /// </summary>
		 /// <returns> transaction information about the last closed (highest gap-free) transaction.
		 /// <pre>
		 /// [0]: transaction id
		 /// [1]: log version
		 /// [2]: byte offset into that log version
		 /// </pre> </returns>
		 long[] LastClosedTransaction { get; }

		 /// <summary>
		 /// Used by recovery, where last committed/closed transaction ids are set.
		 /// Perhaps this shouldn't be exposed like this? </summary>
		 ///  <param name="transactionId"> transaction id that will be the last closed/committed id. </param>
		 /// <param name="checksum"> checksum of the transaction. </param>
		 /// <param name="commitTimestamp"> the timestamp of the transaction commit. </param>
		 /// <param name="byteOffset"> offset in the log file where the committed entry has been written. </param>
		 /// <param name="logVersion"> version of log the committed entry has been written into. </param>
		 void SetLastCommittedAndClosedTransactionId( long transactionId, long checksum, long commitTimestamp, long byteOffset, long logVersion );

		 /// <summary>
		 /// Signals that a transaction with the given transaction id has been fully applied. Calls to this method
		 /// may come in out-of-transaction-id order. </summary>
		 /// <param name="transactionId"> the applied transaction id. </param>
		 /// <param name="logVersion"> version of log the committed entry has been written into. </param>
		 /// <param name="byteOffset"> offset in the log file where start writing the next log entry. </param>
		 void TransactionClosed( long transactionId, long logVersion, long byteOffset );

		 /// <summary>
		 /// Forces the transaction id counters to persistent storage.
		 /// </summary>
		 void Flush();
	}

	public static class TransactionIdStore_Fields
	{
		 public const long BASE_TX_ID = 1;
		 public const long BASE_TX_CHECKSUM = 0;
		 public const long BASE_TX_COMMIT_TIMESTAMP = 0;
		 public const long UNKNOWN_TX_CHECKSUM = 1;
		 public const long UNKNOWN_TX_COMMIT_TIMESTAMP = 1;
		 public const long BASE_TX_LOG_VERSION = 0;
		 public static readonly long BaseTxLogByteOffset = LOG_HEADER_SIZE;
	}

}