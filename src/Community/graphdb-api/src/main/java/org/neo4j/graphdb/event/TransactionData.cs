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
namespace Neo4Net.Graphdb.@event
{


	/// <summary>
	/// Represents the data that has changed during the course of one transaction.
	/// It consists of what has happened in the transaction compared to how
	/// it was before the transaction started. This implies f.ex. that a node which
	/// is created, modified and then deleted in the same transaction won't be seen
	/// in the transaction data at all.
	/// 
	/// @author Tobias Ivarsson
	/// @author Mattias Persson
	/// </summary>
	public interface TransactionData
	{

		 /// <summary>
		 /// Get the nodes that were created during the transaction.
		 /// </summary>
		 /// <returns> all nodes that were created during the transaction. </returns>
		 IEnumerable<Node> CreatedNodes();

		 /// <summary>
		 /// Get the nodes that were deleted during the transaction.
		 /// </summary>
		 /// <returns> all nodes that were deleted during the transaction. </returns>
		 IEnumerable<Node> DeletedNodes();

		 /// <summary>
		 /// Returns whether or not {@code node} is deleted in this transaction. </summary>
		 /// <param name="node"> the <seealso cref="Node"/> to check whether or not it is deleted
		 /// in this transaction. </param>
		 /// <returns> whether or not {@code node} is deleted in this transaction. </returns>
		 bool IsDeleted( Node node );

		 /// <summary>
		 /// Get the properties that had a value assigned or overwritten on a node
		 /// during the transaction. All the properties of nodes created
		 /// during the transaction will be returned by this method as well. Only the
		 /// values that are present at the end of the transaction will be returned,
		 /// whereas all previously assigned values of properties that have been
		 /// assigned multiple times during the transaction will not be returned.
		 /// </summary>
		 /// <returns> all properties that have been assigned on nodes. </returns>
		 IEnumerable<PropertyEntry<Node>> AssignedNodeProperties();

		 /// <summary>
		 /// Get the properties that had a value removed from a node during the
		 /// transaction. Values are considered to be removed if the value is
		 /// overwritten by calling <seealso cref="Node.setProperty(string, object)"/> with a
		 /// property that has a previous value, or if the property is explicitly
		 /// removed by calling <seealso cref="Node.removeProperty(string)"/>. Only the values
		 /// that were present before the transaction are returned by this method, all
		 /// previous values of properties that have been assigned multiple times
		 /// during the transaction will not be returned. This is also true for
		 /// properties that had no value before the transaction, was assigned during
		 /// the transaction, and then removed during the same transaction. Deleting
		 /// a node will cause all its currently assigned properties to be added to
		 /// this list as well.
		 /// </summary>
		 /// <returns> all properties that have been removed from nodes. </returns>
		 IEnumerable<PropertyEntry<Node>> RemovedNodeProperties();

		 /// <summary>
		 /// Get all new labels that have been assigned during the transaction. This
		 /// will return one entry for each label added to each node. All labels assigned
		 /// to nodes that were created in the transaction will also be included.
		 /// </summary>
		 /// <returns> all labels assigned on nodes. </returns>
		 IEnumerable<LabelEntry> AssignedLabels();

		 /// <summary>
		 /// Get all labels that have been removed from nodes during the transaction.
		 /// </summary>
		 /// <returns> all labels removed from nodes. </returns>
		 IEnumerable<LabelEntry> RemovedLabels();

		 /// <summary>
		 /// Get the relationships that were created during the transaction.
		 /// </summary>
		 /// <returns> all relationships that were created during the transaction. </returns>
		 IEnumerable<Relationship> CreatedRelationships();

		 /// <summary>
		 /// Get the relationships that were deleted during the transaction.
		 /// </summary>
		 /// <returns> all relationships that were deleted during the transaction. </returns>
		 IEnumerable<Relationship> DeletedRelationships();

		 /// <summary>
		 /// Returns whether or not {@code relationship} is deleted in this
		 /// transaction.
		 /// </summary>
		 /// <param name="relationship"> the <seealso cref="Relationship"/> to check whether or not it
		 ///            is deleted in this transaction. </param>
		 /// <returns> whether or not {@code relationship} is deleted in this
		 ///         transaction. </returns>
		 bool IsDeleted( Relationship relationship );

		 /// <summary>
		 /// Get the properties that had a value assigned on a relationship during the
		 /// transaction. If the property had a value on that relationship before the
		 /// transaction started the previous value will be returned by
		 /// <seealso cref="removedRelationshipProperties()"/>. All the properties of
		 /// relationships created during the transaction will be returned by this
		 /// method as well. Only the values that are present at the end of the
		 /// transaction will be returned by this method, all previously assigned
		 /// values of properties that have been assigned multiple times during the
		 /// transaction will not be returned.
		 /// </summary>
		 /// <returns> all properties that have been assigned on relationships. </returns>
		 IEnumerable<PropertyEntry<Relationship>> AssignedRelationshipProperties();

		 /// <summary>
		 /// Get the properties that had a value removed from a relationship during
		 /// the transaction. Values are considered to be removed if the value is
		 /// overwritten by calling <seealso cref="Relationship.setProperty(string, object)"/>
		 /// with a property that has a previous value, or if the property is
		 /// explicitly removed by calling <seealso cref="Relationship.removeProperty(string)"/>
		 /// . Only the values that were present before the transaction are returned
		 /// by this method, all previous values of properties that have been assigned
		 /// multiple times during the transaction will not be returned. This is also
		 /// true for properties that had no value before the transaction, was
		 /// assigned during the transaction, and then removed during the same
		 /// transaction. Deleting a relationship will cause all its currently
		 /// assigned properties to be added to this list as well.
		 /// </summary>
		 /// <returns> all properties that have been removed from relationships. </returns>
		 IEnumerable<PropertyEntry<Relationship>> RemovedRelationshipProperties();

		 /// <summary>
		 /// Get the username under which authorization state this transaction is running.
		 /// </summary>
		 /// <returns> the username of the user who initiated the transaction. </returns>
		 string Username();

		 /// <summary>
		 /// Applications that start transactions may attach additional application specific meta-data to each transaction.
		 /// </summary>
		 /// <returns> The application specific meta-data map associated with this transaction. </returns>
		 IDictionary<string, object> MetaData();

		 /// <summary>
		 /// Return transaction id that assigned during transaction commit process. </summary>
		 /// <returns> transaction id. </returns>
		 /// <exception cref="IllegalStateException"> if transaction id is not assigned yet </exception>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default long getTransactionId()
	//	 {
	//		  throw new IllegalStateException("Transaction id is not available.");
	//	 }

		 /// <summary>
		 /// Return transaction commit time (in millis) that assigned during transaction commit process. </summary>
		 /// <returns> transaction commit time </returns>
		 /// <exception cref="IllegalStateException"> if commit time is not assigned yet </exception>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default long getCommitTime()
	//	 {
	//		  throw new IllegalStateException("Transaction commit time it not available.");
	//	 }
	}

}