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
	/// Represents a changed property.
	/// 
	/// Instances of this interface represent the property as it is after the
	/// transaction when returned from
	/// <seealso cref="TransactionData.assignedNodeProperties()"/> or
	/// <seealso cref="TransactionData.assignedRelationshipProperties()"/>. Instances of this
	/// interface represent the property as it was before the transaction as well as
	/// how it will be after the transaction has been committed.
	/// </summary>
	/// @param <T> The type of the entity the property belongs to, either
	///            <seealso cref="Node"/> or <seealso cref="Relationship"/>. </param>
	public interface PropertyEntry<T> where T : Neo4Net.Graphdb.PropertyContainer
	{
		 /// <summary>
		 /// Get the entity that this property was modified on. The entity is either a
		 /// <seealso cref="Node"/> or a <seealso cref="Relationship"/>, depending on the generic type of
		 /// this instance.
		 /// </summary>
		 /// <returns> the <seealso cref="Node"/> or <seealso cref="Relationship"/> that the property was
		 ///         modified on. </returns>
		 T Entity();

		 /// <summary>
		 /// Get the key of the modified property.
		 /// </summary>
		 /// <returns> the key of the modified property. </returns>
		 string Key();

		 /// <summary>
		 /// Get the value of the modified property as it was before the transaction
		 /// (which modified it) started. If this <seealso cref="PropertyEntry"/> was returned
		 /// from <seealso cref="TransactionData.assignedNodeProperties()"/> or
		 /// <seealso cref="TransactionData.assignedRelationshipProperties()"/>, the value
		 /// returned from this method is the value that was set for {@code key} on
		 /// {@code entity} before the transaction started, or {@code null} if such a
		 /// property wasn't set.
		 /// 
		 /// If this <seealso cref="PropertyEntry"/> was returned from
		 /// <seealso cref="TransactionData.removedNodeProperties()"/> or
		 /// <seealso cref="TransactionData.removedRelationshipProperties()"/> the value
		 /// returned from this method is the value that was stored at this property
		 /// before the transaction started.
		 /// </summary>
		 /// <returns> The value of the property as it was before the transaction
		 /// started. </returns>
		 object PreviouslyCommitedValue();

		 /// <summary>
		 /// Get the value of the modified property. If this <seealso cref="PropertyEntry"/>
		 /// was returned from <seealso cref="TransactionData.assignedNodeProperties()"/> or
		 /// <seealso cref="TransactionData.assignedRelationshipProperties()"/>, the value
		 /// returned from this method is the value that will be assigned to the
		 /// property after the transaction is committed. If this
		 /// <seealso cref="PropertyEntry"/> was returned from
		 /// <seealso cref="TransactionData.removedNodeProperties()"/> or
		 /// <seealso cref="TransactionData.removedRelationshipProperties()"/> an
		 /// <seealso cref="System.InvalidOperationException"/> will be thrown.
		 /// </summary>
		 /// <returns> The value of the modified property. </returns>
		 /// <exception cref="IllegalStateException"> if this method is called where this
		 /// instance represents a removed property. </exception>
		 object Value();
	}

}