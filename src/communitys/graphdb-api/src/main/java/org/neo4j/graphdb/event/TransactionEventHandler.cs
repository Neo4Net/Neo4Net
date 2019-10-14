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
namespace Neo4Net.Graphdb.@event
{

	/// <summary>
	/// An event handler interface for Neo4j Transaction events. Once it has been
	/// registered at a <seealso cref="GraphDatabaseService"/> instance it will receive events
	/// about what has happened in each transaction which is about to be committed
	/// and has any data that is accessible via <seealso cref="TransactionData"/>.
	/// Handlers won't get notified about transactions which hasn't performed any
	/// write operation or won't be committed (either if
	/// <seealso cref="Transaction.success()"/> hasn't been called or the transaction has been
	/// marked as failed, <seealso cref="Transaction.failure()"/>.
	/// <para>
	/// Right before a transaction is about to be committed the
	/// <seealso cref="beforeCommit(TransactionData)"/> method is called with the entire diff
	/// of modifications made in the transaction. At this point the transaction is
	/// still running so changes can still be made. However there's no guarantee that
	/// other handlers will see such changes since the order in which handlers are
	/// executed is undefined. This method can also throw an exception and will, in
	/// such a case, prevent the transaction from being committed.
	/// </para>
	/// <para>
	/// If <seealso cref="beforeCommit(TransactionData)"/> is successfully executed the
	/// transaction will be committed and the
	/// <seealso cref="afterCommit(TransactionData, object)"/> method will be called with the
	/// same transaction data as well as the object returned from
	/// <seealso cref="beforeCommit(TransactionData)"/>. This assumes that all other handlers
	/// (if more were registered) also executed
	/// <seealso cref="beforeCommit(TransactionData)"/> successfully.
	/// </para>
	/// <para>
	/// If <seealso cref="beforeCommit(TransactionData)"/> isn't executed successfully, but
	/// instead throws an exception the transaction won't be committed and a
	/// <seealso cref="TransactionFailureException"/> will (eventually) be thrown from
	/// <seealso cref="Transaction.close()"/>. All handlers which at this point have had its
	/// <seealso cref="beforeCommit(TransactionData)"/> method executed successfully will
	/// receive a call to <seealso cref="afterRollback(TransactionData, object)"/>.
	/// 
	/// @author Tobias Ivarsson
	/// @author Mattias Persson
	/// 
	/// </para>
	/// </summary>
	/// @param <T> The type of a state object that the transaction handler can use to
	///            pass information from the <seealso cref="beforeCommit(TransactionData)"/>
	///            event dispatch method to the
	///            <seealso cref="afterCommit(TransactionData, object)"/> or
	///            <seealso cref="afterRollback(TransactionData, object)"/> method, depending
	///            on whether the transaction succeeded or failed. </param>
	public interface TransactionEventHandler<T>
	{
		 /// <summary>
		 /// Invoked when a transaction that has changes accessible via <seealso cref="TransactionData"/>
		 /// is about to be committed.
		 /// 
		 /// If this method throws an exception the transaction will be rolled back
		 /// and a <seealso cref="TransactionFailureException"/> will be thrown from
		 /// <seealso cref="Transaction.close()"/>.
		 /// 
		 /// The transaction is still open when this method is invoked, making it
		 /// possible to perform mutating operations in this method. This is however
		 /// highly discouraged since changes made in this method are not guaranteed to be
		 /// visible by this or other <seealso cref="TransactionEventHandler"/>s.
		 /// </summary>
		 /// <param name="data"> the changes that will be committed in this transaction. </param>
		 /// <returns> a state object (or <code>null</code>) that will be passed on to
		 ///         <seealso cref="afterCommit(TransactionData, object)"/> or
		 ///         <seealso cref="afterRollback(TransactionData, object)"/> of this object. </returns>
		 /// <exception cref="Exception"> to indicate that the transaction should be rolled back. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: T beforeCommit(TransactionData data) throws Exception;
		 T BeforeCommit( TransactionData data );

		 /// <summary>
		 /// Invoked after the transaction has been committed successfully.
		 /// Any <seealso cref="TransactionData"/> being passed in to this method is guaranteed
		 /// to first have been called with <seealso cref="beforeCommit(TransactionData)"/>.
		 /// At the point of calling this method the transaction have been closed
		 /// and so accessing data outside that of what the <seealso cref="TransactionData"/>
		 /// can provide will require a new transaction to be opened.
		 /// </summary>
		 /// <param name="data"> the changes that were committed in this transaction. </param>
		 /// <param name="state"> the object returned by
		 ///            <seealso cref="beforeCommit(TransactionData)"/>. </param>
		 void AfterCommit( TransactionData data, T state );

		 /// <summary>
		 /// Invoked after the transaction has been rolled back if committing the
		 /// transaction failed for some reason.
		 /// Any <seealso cref="TransactionData"/> being passed in to this method is guaranteed
		 /// to first have been called with <seealso cref="beforeCommit(TransactionData)"/>.
		 /// At the point of calling this method the transaction have been closed
		 /// and so accessing data outside that of what the <seealso cref="TransactionData"/>
		 /// can provide will require a new transaction to be opened.
		 /// </summary>
		 /// <param name="data"> the changes that were attempted to be committed in this transaction. </param>
		 /// <param name="state"> the object returned by <seealso cref="beforeCommit(TransactionData)"/>.
		 /// If this handler failed when executing <seealso cref="beforeCommit(TransactionData)"/> this
		 /// {@code state} will be {@code null}. </param>
		 // TODO: should this method take a parameter describing WHY the tx failed?
		 void AfterRollback( TransactionData data, T state );

		 /// <summary>
		 /// Adapter for a <seealso cref="TransactionEventHandler"/>
		 /// </summary>
		 /// @param <T> the type of object communicated from a successful
		 /// <seealso cref="beforeCommit(TransactionData)"/> to <seealso cref="afterCommit(TransactionData, object)"/>. </param>
	}

	 public class TransactionEventHandler_Adapter<T> : TransactionEventHandler<T>
	 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public T beforeCommit(TransactionData data) throws Exception
		  public override T BeforeCommit( TransactionData data )
		  {
				return default( T );
		  }

		  public override void AfterCommit( TransactionData data, T state )
		  {
		  }

		  public override void AfterRollback( TransactionData data, T state )
		  {
		  }
	 }

}