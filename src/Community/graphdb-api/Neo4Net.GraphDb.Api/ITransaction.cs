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
using System;

namespace Neo4Net.GraphDb
{
	/// <summary>
	/// A programmatically handled transaction.
	/// <para>
	/// <em>All database operations that access the graph, indexes, or the schema must be performed in a transaction.</em>
	/// </para>
	/// <para>
	/// If you attempt to access the graph outside of a transaction, those operations will throw
	/// <seealso cref="NotInTransactionException"/>.
	/// </para>
	/// <para>
	/// Transactions are bound to the thread in which they were created.
	/// Here's the idiomatic use of programmatic transactions in Neo4Net:
	/// 
	/// <pre>
	/// <code>
	/// try ( Transaction tx = graphDb.beginTx() )
	/// {
	///     // operations on the graph
	///     // ...
	/// 
	///     tx.success();
	/// }
	/// </code>
	/// </pre>
	/// 
	/// </para>
	/// <para>
	/// Let's walk through this example line by line. First we retrieve a Transaction
	/// object by invoking the <seealso cref="GraphDatabaseService.beginTx()"/> factory method.
	/// This creates a new transaction which has internal state to keep
	/// track of whether the current transaction is successful. Then we wrap all
	/// operations that modify the graph in a try-finally block with the transaction
	/// as resource. At the end of the block, we invoke the <seealso cref="success() tx.success()"/>
	/// method to indicate that the transaction is successful. As we exit the block,
	/// the transaction will automatically be closed where <seealso cref="close() tx.close()"/>
	/// will be called and commit the transaction if the internal state indicates success
	/// or else mark it for rollback.
	/// </para>
	/// <para>
	/// If an exception is raised in the try-block, <seealso cref="success()"/> will never be
	/// invoked and the internal state of the transaction object will cause
	/// <seealso cref="close()"/> to roll back the transaction. This is very important:
	/// unless <seealso cref="success()"/> is invoked, the transaction will fail upon
	/// <seealso cref="close()"/>. A transaction can be explicitly marked for rollback by
	/// invoking the <seealso cref="failure()"/> method.
	/// </para>
	/// <para>
	/// Read operations inside of a transaction will also read uncommitted data from
	/// the same transaction.
	/// </para>
	/// <para>
	/// </para>
	/// <para>
	/// All <seealso cref="ResourceIterable ResourceIterables"/> that where returned from operations executed inside a transaction
	/// will be automatically closed when the transaction is committed or rolled back.
	/// Note however, that the <seealso cref="ResourceIterator"/> should be <seealso cref="ResourceIterator.close() closed"/> as soon as
	/// possible if you don't intend to exhaust the iterator.
	/// </para>
	/// </summary>
	public interface ITransaction : IDisposable
	{
		 /// <summary>
		 /// Marks this transaction as terminated, which means that it will be, much like in the case of failure,
		 /// unconditionally rolled back when <seealso cref="close()"/> is called. Once this method has been invoked, it doesn't matter
		 /// if <seealso cref="success()"/> is invoked afterwards -- the transaction will still be rolled back.
		 /// 
		 /// Additionally, terminating a transaction causes all subsequent operations carried out within that
		 /// transaction to throw a <seealso cref="TransactionTerminatedException"/> in the owning thread.
		 /// 
		 /// Note that, unlike the other transaction operations, this method can be called from threads other than
		 /// the owning thread of the transaction. When this method is called from a different thread,
		 /// it signals the owning thread to terminate the transaction and returns immediately.
		 /// 
		 /// Calling this method on an already closed transaction has no effect.
		 /// </summary>
		 void Terminate();

		 /// <summary>
		 /// Marks this transaction as failed, which means that it will
		 /// unconditionally be rolled back when <seealso cref="close()"/> is called. Once
		 /// this method has been invoked, it doesn't matter if
		 /// <seealso cref="success()"/> is invoked afterwards -- the transaction will still be
		 /// rolled back.
		 /// </summary>
		 void Failure();

		 /// <summary>
		 /// Marks this transaction as successful, which means that it will be
		 /// committed upon invocation of <seealso cref="close()"/> unless <seealso cref="failure()"/>
		 /// has or will be invoked before then.
		 /// </summary>
		 void Success();

		 /// <summary>
		 /// Commits or marks this transaction for rollback, depending on whether
		 /// <seealso cref="success()"/> or <seealso cref="failure()"/> has been previously invoked.
		 /// 
		 /// All <seealso cref="ResourceIterable ResourceIterables"/> that where returned from operations executed inside this
		 /// transaction will be automatically closed by this method.
		 /// 
		 /// This method comes from <seealso cref="IDisposable"/> so that a <seealso cref="ITransaction"/> can participate
		 /// in try-with-resource statements. It will not throw any declared exception.
		 /// 
		 /// Invoking this method (which is unnecessary when in try-with-resource statement).
		 /// </summary>
		 void Close();

		 /// <summary>
		 /// Acquires a write lock for {@code IEntity} for this transaction.
		 /// The lock (returned from this method) can be released manually, but
		 /// if not it's released automatically when the transaction finishes. </summary>
		 /// <param name="entity"> the IEntity to acquire a lock for. If another transaction
		 /// currently holds a write lock to that IEntity this call will wait until
		 /// it's released.
		 /// </param>
		 /// <returns> a <seealso cref="ILock"/> which optionally can be used to release this
		 /// lock earlier than when the transaction finishes. If not released
		 /// (with <seealso cref="ILock.release()"/> it's going to be released with the
		 /// transaction finishes. </returns>
		 ILock AcquireWriteLock( IPropertyContainer IEntity );

		 /// <summary>
		 /// Acquires a read lock for {@code IEntity} for this transaction.
		 /// The lock (returned from this method) can be released manually, but
		 /// if not it's released automatically when the transaction finishes. </summary>
		 /// <param name="entity"> the IEntity to acquire a lock for. If another transaction
		 /// currently hold a write lock to that IEntity this call will wait until
		 /// it's released.
		 /// </param>
		 /// <returns> a <seealso cref="ILock"/> which optionally can be used to release this
		 /// lock earlier than when the transaction finishes. If not released
		 /// (with <seealso cref="ILock.release()"/> it's going to be released with the
		 /// transaction finishes. </returns>
		 ILock AcquireReadLock( IPropertyContainer IEntity );
	}

}