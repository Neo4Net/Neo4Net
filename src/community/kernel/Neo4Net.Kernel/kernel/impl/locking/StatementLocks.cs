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
namespace Neo4Net.Kernel.impl.locking
{

	using KernelStatement = Neo4Net.Kernel.Impl.Api.KernelStatement;
	using LockTracer = Neo4Net.Storageengine.Api.@lock.LockTracer;

	/// <summary>
	/// Component used by <seealso cref="KernelStatement"/> to acquire <seealso cref="pessimistic() pessimistic"/> and
	/// <seealso cref="optimistic() optimistic"/> locks.
	/// </summary>
	public interface StatementLocks : AutoCloseable
	{
		 /// <summary>
		 /// Get <seealso cref="Locks.Client"/> responsible for pessimistic locks. Such locks will be grabbed right away.
		 /// </summary>
		 /// <returns> the locks client to serve pessimistic locks. </returns>
		 Locks_Client Pessimistic();

		 /// <summary>
		 /// Get <seealso cref="Locks.Client"/> responsible for optimistic locks. Such locks could potentially be grabbed later at
		 /// commit time.
		 /// </summary>
		 /// <returns> the locks client to serve optimistic locks. </returns>
		 Locks_Client Optimistic();

		 /// <summary>
		 /// Prepare the underlying <seealso cref="Locks.Client client"/>(s) for commit. This will grab all locks that have
		 /// previously been taken <seealso cref="optimistic() optimistically"/>, and tell the underlying lock client to enter the
		 /// <em>prepare</em> state. </summary>
		 /// <param name="lockTracer"> lock tracer </param>
		 void PrepareForCommit( LockTracer lockTracer );

		 /// <summary>
		 /// Stop the underlying <seealso cref="Locks.Client client"/>(s).
		 /// </summary>
		 void Stop();

		 /// <summary>
		 /// Close the underlying <seealso cref="Locks.Client client"/>(s).
		 /// </summary>
		 void Close();

		 /// <summary>
		 /// List the locks held by this transaction.
		 /// 
		 /// This method is invoked by concurrent threads in order to inspect the lock state in this transaction.
		 /// </summary>
		 /// <returns> the locks held by this transaction. </returns>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.stream.Stream<? extends ActiveLock> activeLocks();
		 Stream<ActiveLock> ActiveLocks();

		 /// <summary>
		 /// Get the current number of active locks.
		 /// 
		 /// Note that the value returned by this method might differ from the number of locks returned by
		 /// <seealso cref="activeLocks()"/>, since they would introspect the lock state at different points in time.
		 /// </summary>
		 /// <returns> the number of active locks in this transaction. </returns>
		 long ActiveLockCount();
	}

}