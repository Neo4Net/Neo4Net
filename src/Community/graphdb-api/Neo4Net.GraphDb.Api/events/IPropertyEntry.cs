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
   /// Represents a changed property.
   ///
   /// Instances of this interface represent the property as it is after the
   /// transaction when returned from
   /// <seealso cref="ITransactionData.assignedNodeProperties()"/> or
   /// <seealso cref="ITransactionData.assignedRelationshipProperties()"/>. Instances of this
   /// interface represent the property as it was before the transaction as well as
   /// how it will be after the transaction has been committed.
   /// </summary>
   /// @param <T> The type of the IEntity the property belongs to, either
   ///            <seealso cref="INode"/> or <seealso cref="IRelationship"/>. </param>
   public interface IPropertyEntry<T> where T : Neo4Net.GraphDb.IPropertyContainer
   {
      /// <summary>
      /// Get the IEntity that this property was modified on. The IEntity is either a
      /// <seealso cref="INode"/> or a <seealso cref="IRelationship"/>, depending on the generic type of
      /// this instance.
      /// </summary>
      /// <returns> the <seealso cref="INode"/> or <seealso cref="IRelationship"/> that the property was
      ///         modified on. </returns>
      T IEntity();

      /// <summary>
      /// Get the key of the modified property.
      /// </summary>
      /// <returns> the key of the modified property. </returns>
      string Key();

      /// <summary>
      /// Get the value of the modified property as it was before the transaction
      /// (which modified it) started. If this <seealso cref="PropertyEntry"/> was returned
      /// from <seealso cref="ITransactionData.assignedNodeProperties()"/> or
      /// <seealso cref="ITransactionData.assignedRelationshipProperties()"/>, the value
      /// returned from this method is the value that was set for {@code key} on
      /// {@code IEntity} before the transaction started, or {@code null} if such a
      /// property wasn't set.
      ///
      /// If this <seealso cref="PropertyEntry"/> was returned from
      /// <seealso cref="ITransactionData.removedNodeProperties()"/> or
      /// <seealso cref="ITransactionData.removedRelationshipProperties()"/> the value
      /// returned from this method is the value that was stored at this property
      /// before the transaction started.
      /// </summary>
      /// <returns> The value of the property as it was before the transaction
      /// started. </returns>
      object PreviouslyCommitedValue();

      /// <summary>
      /// Get the value of the modified property. If this <seealso cref="PropertyEntry"/>
      /// was returned from <seealso cref="ITransactionData.assignedNodeProperties()"/> or
      /// <seealso cref="ITransactionData.assignedRelationshipProperties()"/>, the value
      /// returned from this method is the value that will be assigned to the
      /// property after the transaction is committed. If this
      /// <seealso cref="PropertyEntry"/> was returned from
      /// <seealso cref="ITransactionData.removedNodeProperties()"/> or
      /// <seealso cref="ITransactionData.removedRelationshipProperties()"/> an
      /// <seealso cref="System.InvalidOperationException"/> will be thrown.
      /// </summary>
      /// <returns> The value of the modified property. </returns>
      /// <exception cref="IllegalStateException"> if this method is called where this
      /// instance represents a removed property. </exception>
      object Value();
   }
}