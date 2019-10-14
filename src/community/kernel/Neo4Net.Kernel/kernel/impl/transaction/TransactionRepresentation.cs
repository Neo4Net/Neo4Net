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
namespace Neo4Net.Kernel.impl.transaction
{
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using TransactionAppender = Neo4Net.Kernel.impl.transaction.log.TransactionAppender;
	using CommandStream = Neo4Net.Storageengine.Api.CommandStream;

	/// <summary>
	/// Representation of a transaction that can be written to a <seealso cref="TransactionAppender"/> and read back later.
	/// </summary>
	public interface TransactionRepresentation : CommandStream
	{
		 /// <returns> an additional header of this transaction. Just arbitrary bytes that means nothing
		 /// to this transaction representation. </returns>
		 sbyte[] AdditionalHeader();

		 /// <returns> database instance id of current master in a potential database cluster at the time of committing
		 /// this transaction {@code -1} means no cluster. </returns>
		 int MasterId { get; }

		 /// <returns> database instance id of the author of this transaction. </returns>
		 int AuthorId { get; }

		 /// <returns> time when transaction was started, i.e. when the user started it, not when it was committed.
		 /// Reported in milliseconds. </returns>
		 long TimeStarted { get; }

		 /// <returns> last committed transaction id at the time when this transaction was started. </returns>
		 long LatestCommittedTxWhenStarted { get; }

		 /// <returns> time when transaction was committed. Reported in milliseconds. </returns>
		 long TimeCommitted { get; }

		 /// <returns> the identifier for the lock session associated with this transaction, or {@value Locks.Client#NO_LOCK_SESSION_ID} if none.
		 /// This is only used for slave commits. </returns>
		 int LockSessionId { get; }
	}

}