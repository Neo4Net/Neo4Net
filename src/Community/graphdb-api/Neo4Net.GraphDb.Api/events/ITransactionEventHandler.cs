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

namespace Neo4Net.GraphDb.Events
{
   /// <summary>
   /// An event handler interface for Neo4Net Transaction events. Once it has been
   /// registered at a <seealso cref="GraphDatabaseService"/> instance it will receive events
   /// about what has happened in each transaction which is about to be committed
   /// and has any data that is accessible via <seealso cref="ITransactionData"/>.
   /// Handlers won't get notified about transactions which hasn't performed any
   /// write operation or won't be committed (either if
   /// <seealso cref="ITransaction.success()"/> hasn't been called or the transaction has been
   /// marked as failed, <seealso cref="ITransaction.failure()"/>.
   /// <para>
   /// Right before a transaction is about to be committed the
   /// <seealso cref="beforeCommit(ITransactionData)"/> method is called with the entire diff
   /// of modifications made in the transaction. At this point the transaction is
   /// still running so changes can still be made. However there's no guarantee that
   /// other handlers will see such changes since the order in which handlers are
   /// executed is undefined. This method can also throw an exception and will, in
   /// such a case, prevent the transaction from being committed.
   /// </para>
   /// <para>
   /// If <seealso cref="beforeCommit(ITransactionData)"/> is successfully executed the
   /// transaction will be committed and the
   /// <seealso cref="afterCommit(ITransactionData, object)"/> method will be called with the
   /// same transaction data as well as the object returned from
   /// <seealso cref="beforeCommit(ITransactionData)"/>. This assumes that all other handlers
   /// (if more were registered) also executed
   /// <seealso cref="beforeCommit(ITransactionData)"/> successfully.
   /// </para>
   /// <para>
   /// If <seealso cref="beforeCommit(ITransactionData)"/> isn't executed successfully, but
   /// instead throws an exception the transaction won't be committed and a
   /// <seealso cref="TransactionFailureException"/> will (eventually) be thrown from
   /// <seealso cref="ITransaction.close()"/>. All handlers which at this point have had its
   /// <seealso cref="beforeCommit(ITransactionData)"/> method executed successfully will
   /// receive a call to <seealso cref="afterRollback(ITransactionData, object)"/>.
   ///
   /// @author Tobias Ivarsson
   /// @author Mattias Persson
   ///
   /// </para>
   /// </summary>
   /// @param <T> The type of a state object that the transaction handler can use to
   ///            pass information from the <seealso cref="beforeCommit(ITransactionData)"/>
   ///            event dispatch method to the
   ///            <seealso cref="afterCommit(ITransactionData, object)"/> or
   ///            <seealso cref="afterRollback(ITransactionData, object)"/> method, depending
   ///            on whether the transaction succeeded or failed. </param>
   public interface ITransactionEventHandler<T>
   {
      /// <summary>
      /// Invoked when a transaction that has changes accessible via <seealso cref="ITransactionData"/>
      /// is about to be committed.
      ///
      /// If this method throws an exception the transaction will be rolled back
      /// and a <seealso cref="TransactionFailureException"/> will be thrown from
      /// <seealso cref="ITransaction.close()"/>.
      ///
      /// The transaction is still open when this method is invoked, making it
      /// possible to perform mutating operations in this method. This is however
      /// highly discouraged since changes made in this method are not guaranteed to be
      /// visible by this or other <seealso cref="TransactionEventHandler"/>s.
      /// </summary>
      /// <param name="data"> the changes that will be committed in this transaction. </param>
      /// <returns> a state object (or <code>null</code>) that will be passed on to
      ///         <seealso cref="afterCommit(ITransactionData, object)"/> or
      ///         <seealso cref="afterRollback(ITransactionData, object)"/> of this object. </returns>
      /// <exception cref="Exception"> to indicate that the transaction should be rolled back. </exception>

      T BeforeCommit(ITransactionData data);

      /// <summary>
      /// Invoked after the transaction has been committed successfully.
      /// Any <seealso cref="ITransactionData"/> being passed in to this method is guaranteed
      /// to first have been called with <seealso cref="beforeCommit(ITransactionData)"/>.
      /// At the point of calling this method the transaction have been closed
      /// and so accessing data outside that of what the <seealso cref="ITransactionData"/>
      /// can provide will require a new transaction to be opened.
      /// </summary>
      /// <param name="data"> the changes that were committed in this transaction. </param>
      /// <param name="state"> the object returned by
      ///            <seealso cref="beforeCommit(ITransactionData)"/>. </param>
      void AfterCommit(ITransactionData data, T state);

      /// <summary>
      /// Invoked after the transaction has been rolled back if committing the
      /// transaction failed for some reason.
      /// Any <seealso cref="ITransactionData"/> being passed in to this method is guaranteed
      /// to first have been called with <seealso cref="beforeCommit(ITransactionData)"/>.
      /// At the point of calling this method the transaction have been closed
      /// and so accessing data outside that of what the <seealso cref="ITransactionData"/>
      /// can provide will require a new transaction to be opened.
      /// </summary>
      /// <param name="data"> the changes that were attempted to be committed in this transaction. </param>
      /// <param name="state"> the object returned by <seealso cref="beforeCommit(ITransactionData)"/>.
      /// If this handler failed when executing <seealso cref="beforeCommit(ITransactionData)"/> this
      /// {@code state} will be {@code null}. </param>
      // TODO: should this method take a parameter describing WHY the tx failed?
      void AfterRollback(ITransactionData data, T state);

      /// <summary>
      /// Adapter for a <seealso cref="TransactionEventHandler"/>
      /// </summary>
      /// @param <T> the type of object communicated from a successful
      /// <seealso cref="beforeCommit(ITransactionData)"/> to <seealso cref="afterCommit(ITransactionData, object)"/>. </param>
   }

   public class TransactionEventHandler_Adapter<T> : ITransactionEventHandler<T>
   {
      public T BeforeCommit(ITransactionData data)
      {
         return default(T);
      }

      public void AfterCommit(ITransactionData data, T state)
      {
      }

      public void AfterRollback(ITransactionData data, T state)
      {
      }
   }
}