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
namespace Neo4Net.Kernel.impl.transaction.log
{

	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using LogAppendEvent = Neo4Net.Kernel.impl.transaction.tracing.LogAppendEvent;
	using LogCheckPointEvent = Neo4Net.Kernel.impl.transaction.tracing.LogCheckPointEvent;
	using DatabaseHealth = Neo4Net.Kernel.@internal.DatabaseHealth;

	/// <summary>
	/// Writes batches of transactions, each containing groups of commands to a log that is guaranteed to be recoverable,
	/// i.e. consistently readable, in the event of failure.
	/// </summary>
	public interface TransactionAppender
	{
		 /// <summary>
		 /// Appends a batch of transactions to a log, effectively committing the transactions.
		 /// After this method have returned the returned transaction id should be visible in
		 /// <seealso cref="TransactionIdStore.getLastCommittedTransactionId()"/>.
		 /// <para>
		 /// Any failure happening inside this method will cause a <seealso cref="DatabaseHealth.panic(System.Exception) kernel panic"/>.
		 /// Callers must make sure that successfully appended
		 /// transactions exiting this method are <seealso cref="Commitment.publishAsClosed()"/>}.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="batch"> transactions to append to the log. These transaction instances provide both input arguments
		 /// as well as a place to provide output data, namely <seealso cref="TransactionToApply.commitment()"/> and
		 /// <seealso cref="TransactionToApply.transactionId()"/>. </param>
		 /// <param name="logAppendEvent"> A trace event for the given log append operation. </param>
		 /// <returns> last committed transaction in this batch. The appended (i.e. committed) transactions
		 /// will have had their <seealso cref="TransactionToApply.commitment()"/> available and caller is expected to
		 /// <seealso cref="Commitment.publishAsClosed() mark them as applied"/> after they have been applied to storage.
		 /// Note that <seealso cref="Commitment commitments"/> must be <seealso cref="Commitment.publishAsCommitted() marked as committed"/>
		 /// by this method. </returns>
		 /// <exception cref="IOException"> if there was a problem appending the transaction. See method javadoc body for
		 /// how to handle exceptions in general thrown from this method. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long append(org.neo4j.kernel.impl.api.TransactionToApply batch, org.neo4j.kernel.impl.transaction.tracing.LogAppendEvent logAppendEvent) throws java.io.IOException;
		 long Append( TransactionToApply batch, LogAppendEvent logAppendEvent );

		 /// <summary>
		 /// Appends a check point to a log which marks a starting point for recovery in the event of failure.
		 /// After this method have returned the check point mark must have been flushed to disk.
		 /// </summary>
		 /// <param name="logPosition"> the log position contained in the written check point </param>
		 /// <param name="logCheckPointEvent"> a trace event for the given check point operation. </param>
		 /// <exception cref="IOException"> if there was a problem appending the transaction. See method javadoc body for
		 /// how to handle exceptions in general thrown from this method. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void checkPoint(LogPosition logPosition, org.neo4j.kernel.impl.transaction.tracing.LogCheckPointEvent logCheckPointEvent) throws java.io.IOException;
		 void CheckPoint( LogPosition logPosition, LogCheckPointEvent logCheckPointEvent );
	}

}