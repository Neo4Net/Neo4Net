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
namespace Neo4Net.Kernel.Impl.Api
{

	using LockGroup = Neo4Net.Kernel.impl.locking.LockGroup;
	using CommandsToApply = Neo4Net.Storageengine.Api.CommandsToApply;

	/// <summary>
	/// Responsible for dealing with batches of transactions. See also <seealso cref="TransactionApplier"/>
	/// 
	/// Typical usage looks like:
	/// <pre>
	/// try ( BatchTransactionApplier batchApplier = getBatchApplier() )
	/// {
	///     TransactionToApply tx = batch;
	///     while ( tx != null )
	///     {
	///         try ( LockGroup locks = new LockGroup() )
	///         {
	///             ensureValidatedIndexUpdates( tx );
	///             try ( TransactionApplier txApplier = batchApplier.startTx( tx, locks ) )
	///             {
	///                 tx.transactionRepresentation().accept( txApplier );
	///             }
	///         }
	///         catch ( Throwable cause )
	///         {
	///             databaseHealth.panic( cause );
	///             throw cause;
	///         }
	///         tx = tx.next();
	///     }
	/// }
	/// </pre>
	/// </summary>
	public interface BatchTransactionApplier : AutoCloseable
	{
		 /// <summary>
		 /// Get the suitable <seealso cref="TransactionApplier"/> for a given transaction, and the store which this {@link
		 /// BatchTransactionApplier} is associated with. See also <seealso cref="startTx(CommandsToApply, LockGroup)"/> if
		 /// your operations need to share a <seealso cref="LockGroup"/>.
		 /// 
		 /// Typically you'd want to use this in a try-with-resources block to automatically close the {@link
		 /// TransactionApplier} when finished with the transaction, f.ex. as:
		 /// <pre>
		 /// try ( TransactionApplier txApplier = batchTxApplier.startTx( txToApply )
		 /// {
		 ///     // Apply the transaction
		 ///     txToApply.transactionRepresentation().accept( txApplier );
		 ///     // Or apply other commands
		 ///     // txApplier.visit( command );
		 /// }
		 /// </pre>
		 /// </summary>
		 /// <param name="transaction"> The transaction which this applier is going to apply. Once we don't have to validate index
		 /// updates anymore, we can change this to simply be the transactionId </param>
		 /// <returns> a <seealso cref="TransactionApplier"/> which can apply this transaction and other commands to the store. </returns>
		 /// <exception cref="IOException"> on error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: TransactionApplier startTx(org.neo4j.storageengine.api.CommandsToApply transaction) throws java.io.IOException;
		 TransactionApplier StartTx( CommandsToApply transaction );

		 /// <summary>
		 /// Get the suitable <seealso cref="TransactionApplier"/> for a given transaction, and the store which this {@link
		 /// BatchTransactionApplier} is associated with. See also <seealso cref="startTx(CommandsToApply)"/> if your transaction
		 /// does not require any locks.
		 /// 
		 /// Typically you'd want to use this in a try-with-resources block to automatically close the {@link
		 /// TransactionApplier} when finished with the transaction, f.ex. as:
		 /// <pre>
		 /// try ( TransactionApplier txApplier = batchTxApplier.startTx( txToApply )
		 /// {
		 ///     // Apply the transaction
		 ///     txToApply.transactionRepresentation().accept( txApplier );
		 ///     // Or apply other commands
		 ///     // txApplier.visit( command );
		 /// }
		 /// </pre>
		 /// </summary>
		 /// <param name="transaction"> The transaction which this applier is going to apply. Once we don't have to validate index
		 /// updates anymore, we can change this to simply be the transactionId </param>
		 /// <param name="lockGroup"> A lockGroup which can hold the locks that the transaction requires. </param>
		 /// <returns> a <seealso cref="TransactionApplier"/> which can apply this transaction and other commands to the store. </returns>
		 /// <exception cref="IOException"> on error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: TransactionApplier startTx(org.neo4j.storageengine.api.CommandsToApply transaction, org.neo4j.kernel.impl.locking.LockGroup lockGroup) throws java.io.IOException;
		 TransactionApplier StartTx( CommandsToApply transaction, LockGroup lockGroup );

		 /// <summary>
		 /// This method is suitable for any work that needs to be done after a batch of transactions. Typically called
		 /// implicitly at the end of a try-with-resources block.
		 /// </summary>
		 /// <exception cref="Exception"> on error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void close() throws Exception;
		 void Close();
	}

	 public abstract class BatchTransactionApplier_Adapter : BatchTransactionApplier
	 {
		 public abstract TransactionApplier StartTx( CommandsToApply transaction );
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public TransactionApplier startTx(org.neo4j.storageengine.api.CommandsToApply transaction, org.neo4j.kernel.impl.locking.LockGroup lockGroup) throws java.io.IOException
		  public override TransactionApplier StartTx( CommandsToApply transaction, LockGroup lockGroup )
		  {
				return StartTx( transaction );
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
		  public override void Close()
		  { // Nothing to close
		  }
	 }

}