using System.Collections.Generic;

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
namespace Neo4Net.Kernel.api
{

	using Kernel = Neo4Net.Internal.Kernel.Api.Kernel;
	using Transaction = Neo4Net.Internal.Kernel.Api.Transaction;
	using AuthSubject = Neo4Net.Internal.Kernel.Api.security.AuthSubject;
	using LoginContext = Neo4Net.Internal.Kernel.Api.security.LoginContext;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using ExecutingQuery = Neo4Net.Kernel.api.query.ExecutingQuery;
	using TransactionExecutionStatistic = Neo4Net.Kernel.Impl.Api.TransactionExecutionStatistic;
	using ActiveLock = Neo4Net.Kernel.impl.locking.ActiveLock;

	/// <summary>
	/// View of a <seealso cref="KernelTransaction"/> that provides a limited set of actions against the transaction.
	/// </summary>
	public interface KernelTransactionHandle
	{
		 /// <summary>
		 /// The id of the last transaction that was committed to the store when the underlying transaction started.
		 /// </summary>
		 /// <returns> the committed transaction id. </returns>
		 long LastTransactionIdWhenStarted();

		 /// <summary>
		 /// The timestamp of the last transaction that was committed to the store when the underlying transaction started.
		 /// </summary>
		 /// <returns> the timestamp value obtained with <seealso cref="System.currentTimeMillis()"/>. </returns>
		 long LastTransactionTimestampWhenStarted();

		 /// <summary>
		 /// The start time of the underlying transaction. I.e. basically <seealso cref="System.currentTimeMillis()"/> when user
		 /// called <seealso cref="Kernel.beginTransaction(Transaction.Type, LoginContext)"/>.
		 /// </summary>
		 /// <returns> the transaction start time. </returns>
		 long StartTime();

		 /// <summary>
		 /// The start time of the underlying transaction. I.e. basically <seealso cref="System.nanoTime()"/> ()} when user
		 /// called <seealso cref="org.neo4j.internal.kernel.api.Session.beginTransaction(KernelTransaction.Type)"/>.
		 /// 
		 /// This can be used to measure elapsed time in a safe way that is not affected by system time changes.
		 /// </summary>
		 /// <returns> nanoTime at the start of the transaction. </returns>
		 long StartTimeNanos();

		 /// <summary>
		 /// Underlying transaction specific timeout. In case if timeout is 0 - transaction does not have a timeout. </summary>
		 /// <returns> transaction timeout in milliseconds, <b>0 in case if transaction does not have a timeout<b/> </returns>
		 long TimeoutMillis();

		 /// <summary>
		 /// Check if the underlying transaction is open.
		 /// </summary>
		 /// <returns> {@code true} if the underlying transaction (<seealso cref="KernelTransaction.close()"/> was not called),
		 /// {@code false} otherwise. </returns>
		 bool Open { get; }

		 /// <summary>
		 /// Mark the underlying transaction for termination.
		 /// </summary>
		 /// <param name="reason"> the reason for termination. </param>
		 /// <returns> {@code true} if the underlying transaction was marked for termination, {@code false} otherwise
		 /// (when this handle represents an old transaction that has been closed). </returns>
		 bool MarkForTermination( Status reason );

		 /// <summary>
		 /// Security context of underlying transaction that transaction has when handle was created.
		 /// </summary>
		 /// <returns> underlying transaction security context </returns>
		 AuthSubject Subject();

		 /// <summary>
		 /// Metadata of underlying transaction that transaction has when handle was created. </summary>
		 /// <returns> underlying transaction metadata </returns>
		 IDictionary<string, object> MetaData { get; }

		 /// <summary>
		 /// Transaction termination reason that transaction had when handle was created.
		 /// </summary>
		 /// <returns> transaction termination reason. </returns>
		 Optional<Status> TerminationReason();

		 /// <summary>
		 /// Check if this handle points to the same underlying transaction as the given one.
		 /// </summary>
		 /// <param name="tx"> the expected transaction. </param>
		 /// <returns> {@code true} if this handle represents {@code tx}, {@code false} otherwise. </returns>
		 bool IsUnderlyingTransaction( KernelTransaction tx );

		 /// <summary>
		 /// User transaction id of underlying transaction. User transaction id is a not negative long number.
		 /// Should be unique across transactions. </summary>
		 /// <returns> user transaction id </returns>
		 long UserTransactionId { get; }

		 /// <summary>
		 /// User transaction name of the underlying transaction.
		 /// User transaction name consists of the name prefix and user transaction id.
		 /// Should be unique across transactions. </summary>
		 /// <returns> user transaction name </returns>
		 string UserTransactionName { get; }

		 /// <returns> a list of all queries currently executing that use the underlying transaction </returns>
		 Stream<ExecutingQuery> ExecutingQueries();

		 /// <returns> the lock requests granted for this transaction. </returns>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.stream.Stream<? extends org.neo4j.kernel.impl.locking.ActiveLock> activeLocks();
		 Stream<ActiveLock> ActiveLocks();

		 /// <summary>
		 /// Provide underlying transaction execution statistics. For example: elapsed time, allocated bytes etc </summary>
		 /// <returns> transaction statistics projection </returns>
		 TransactionExecutionStatistic TransactionStatistic();

		 /// <returns> whether or not this transaction is a schema transaction. Type of transaction is decided
		 /// on first write operation, be it data or schema operation. </returns>
		 bool SchemaTransaction { get; }
	}

}