using System.Collections.Generic;

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
namespace Org.Neo4j.@unsafe.Batchinsert
{

	using Label = Org.Neo4j.Graphdb.Label;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using ConstraintCreator = Org.Neo4j.Graphdb.schema.ConstraintCreator;
	using IndexCreator = Org.Neo4j.Graphdb.schema.IndexCreator;

	/// <summary>
	/// The batch inserter drops support for transactions and concurrency in favor
	/// of insertion speed. When done using the batch inserter <seealso cref="shutdown()"/>
	/// must be invoked and complete successfully for the Neo4j store to be in
	/// consistent state.
	/// <para>
	/// Only one thread at a time may work against the batch inserter, multiple
	/// threads performing concurrent access have to employ synchronization.
	/// </para>
	/// <para>
	/// Transactions are not supported so if the JVM/machine crashes or you fail to
	/// invoke <seealso cref="shutdown()"/> before JVM exits the Neo4j store can be considered
	/// being in non consistent state and the insertion has to be re-done from
	/// scratch.
	/// </para>
	/// </summary>
	public interface BatchInserter
	{
		 /// <summary>
		 /// Creates a node assigning next available id to id and also adds any
		 /// properties supplied.
		 /// </summary>
		 /// <param name="properties"> a map containing properties or <code>null</code> if no
		 /// properties should be added. </param>
		 /// <param name="labels"> a list of labels to initially create the node with.
		 /// </param>
		 /// <returns> The id of the created node. </returns>
		 long CreateNode( IDictionary<string, object> properties, params Label[] labels );

		 /// <summary>
		 /// Creates a node with supplied id and properties. If a node with the given
		 /// id exist a runtime exception will be thrown.
		 /// </summary>
		 /// <param name="id"> the id of the node to create. </param>
		 /// <param name="properties"> map containing properties or <code>null</code> if no
		 /// properties should be added. </param>
		 /// <param name="labels"> a list of labels to initially create the node with. </param>
		 void CreateNode( long id, IDictionary<string, object> properties, params Label[] labels );

		 /// <summary>
		 /// Checks if a node with the given id exists.
		 /// </summary>
		 /// <param name="nodeId"> the id of the node. </param>
		 /// <returns> <code>true</code> if the node exists. </returns>
		 bool NodeExists( long nodeId );

		 /// <summary>
		 /// Sets the properties of a node. This method will remove any properties
		 /// already existing and replace it with properties in the supplied map.
		 /// <para>
		 /// For best performance try supply all the nodes properties upon creation
		 /// of the node. This method will delete any existing properties so using it
		 /// together with <seealso cref="getNodeProperties(long)"/> will have bad performance.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="node"> the id of the node. </param>
		 /// <param name="properties"> map containing the properties or <code>null</code> to
		 /// clear all properties. </param>
		 void SetNodeProperties( long node, IDictionary<string, object> properties );

		 /// <summary>
		 /// Returns true iff the node with id {@code node} has a property with name
		 /// {@code propertyName}.
		 /// </summary>
		 /// <param name="node"> The node id of the node to check. </param>
		 /// <param name="propertyName"> The property name to check for </param>
		 /// <returns> True if the node has the named property - false otherwise. </returns>
		 bool NodeHasProperty( long node, string propertyName );

		 /// <summary>
		 /// Replaces any existing labels for the given node with the supplied list of labels.
		 /// </summary>
		 /// <param name="node"> the node to set labels for. </param>
		 /// <param name="labels"> the labels to set for the node. </param>
		 void SetNodeLabels( long node, params Label[] labels );

		 /// <param name="node"> the node to get labels for. </param>
		 /// <returns> all labels for the given node. </returns>
		 IEnumerable<Label> GetNodeLabels( long node );

		 /// <param name="node"> the node to check. </param>
		 /// <param name="label"> the label to check. </param>
		 /// <returns> {@code true} if a node has a given label, otherwise {@code false}. </returns>
		 bool NodeHasLabel( long node, Label label );

		 /// <summary>
		 /// Returns true iff the relationship with id {@code relationship} has a
		 /// property with name {@code propertyName}.
		 /// </summary>
		 /// <param name="relationship"> The relationship id of the relationship to check. </param>
		 /// <param name="propertyName"> The property name to check for </param>
		 /// <returns> True if the relationship has the named property - false
		 ///         otherwise. </returns>
		 bool RelationshipHasProperty( long relationship, string propertyName );

		 /// <summary>
		 /// Sets the property with name {@code propertyName} of node with id
		 /// {@code node} to the value {@code propertyValue}. If the property exists
		 /// it is updated, otherwise created.
		 /// </summary>
		 /// <param name="node"> The node id of the node whose property is to be set </param>
		 /// <param name="propertyName"> The name of the property to set </param>
		 /// <param name="propertyValue"> The value of the property to set </param>
		 void SetNodeProperty( long node, string propertyName, object propertyValue );

		 /// <summary>
		 /// Sets the property with name {@code propertyName} of relationship with id
		 /// {@code relationship} to the value {@code propertyValue}. If the property
		 /// exists it is updated, otherwise created.
		 /// </summary>
		 /// <param name="relationship"> The node id of the relationship whose property is to
		 ///            be set </param>
		 /// <param name="propertyName"> The name of the property to set </param>
		 /// <param name="propertyValue"> The value of the property to set </param>
		 void SetRelationshipProperty( long relationship, string propertyName, object propertyValue );
		 /// <summary>
		 /// Returns a map containing all the properties of this node.
		 /// </summary>
		 /// <param name="nodeId"> the id of the node.
		 /// </param>
		 /// <returns> map containing this node's properties. </returns>
		 IDictionary<string, object> GetNodeProperties( long nodeId );

		 /// <summary>
		 /// Returns an iterable over all the relationship ids connected to node with
		 /// supplied id.
		 /// </summary>
		 /// <param name="nodeId"> the id of the node. </param>
		 /// <returns> iterable over the relationship ids connected to the node. </returns>
		 IEnumerable<long> GetRelationshipIds( long nodeId );

		 /// <summary>
		 /// Returns an iterable of <seealso cref="BatchRelationship relationships"/> connected
		 /// to the node with supplied id.
		 /// </summary>
		 /// <param name="nodeId"> the id of the node. </param>
		 /// <returns> iterable over the relationships connected to the node. </returns>
		 IEnumerable<BatchRelationship> GetRelationships( long nodeId );

		 /// <summary>
		 /// Creates a relationship between two nodes of a specific type.
		 /// </summary>
		 /// <param name="node1"> the start node. </param>
		 /// <param name="node2"> the end node. </param>
		 /// <param name="type"> relationship type. </param>
		 /// <param name="properties"> map containing properties or <code>null</code> if no
		 /// properties should be added. </param>
		 /// <returns> the id of the created relationship. </returns>
		 long CreateRelationship( long node1, long node2, RelationshipType type, IDictionary<string, object> properties );

		 /// <summary>
		 /// Gets a relationship by id.
		 /// </summary>
		 /// <param name="relId"> the relationship id. </param>
		 /// <returns> a simple relationship wrapper for the relationship. </returns>
		 BatchRelationship GetRelationshipById( long relId );

		 /// <summary>
		 /// Sets the properties of a relationship. This method will remove any
		 /// properties already existing and replace it with properties in the
		 /// supplied map.
		 /// <para>
		 /// For best performance try supply all the relationship properties upon
		 /// creation of the relationship. This method will delete any existing
		 /// properties so using it together with
		 /// <seealso cref="getRelationshipProperties(long)"/> will have bad performance.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="rel"> the id of the relationship. </param>
		 /// <param name="properties"> map containing the properties or <code>null</code> to
		 /// clear all properties. </param>
		 void SetRelationshipProperties( long rel, IDictionary<string, object> properties );

		 /// <summary>
		 /// Returns a map containing all the properties of the relationships.
		 /// </summary>
		 /// <param name="relId"> the id of the relationship. </param>
		 /// <returns> map containing the relationship's properties. </returns>
		 IDictionary<string, object> GetRelationshipProperties( long relId );

		 /// <summary>
		 /// Removes the property named {@code property} from the node with id
		 /// {@code id}, if present.
		 /// </summary>
		 /// <param name="node"> The id of the node from which to remove the property </param>
		 /// <param name="property"> The name of the property </param>
		 void RemoveNodeProperty( long node, string property );

		 /// <summary>
		 /// Removes the property named {@code property} from the relationship with id
		 /// {@code id}, if present.
		 /// </summary>
		 /// <param name="relationship"> The id of the relationship from which to remove the
		 ///            property </param>
		 /// <param name="property"> The name of the property </param>
		 void RemoveRelationshipProperty( long relationship, string property );

		 /// <summary>
		 /// Returns an <seealso cref="IndexCreator"/> where details about the index to create can be
		 /// specified. When all details have been entered <seealso cref="IndexCreator.create() create"/>
		 /// must be called for it to actually be created.
		 /// 
		 /// Creating an index enables indexing for nodes with the specified label. The index will
		 /// have the details supplied to the <seealso cref="IndexCreator returned index creator"/>.
		 /// 
		 /// Indexes created with the method are deferred until the batch inserter is shut down, at
		 /// which point a background job will populate all indexes, i.e. the index
		 /// is not available during the batch insertion itself. It is therefore advisable to
		 /// create deferred indexes just before shutting down the batch inserter.
		 /// </summary>
		 /// <param name="label"> <seealso cref="Label label"/> on nodes to be indexed
		 /// </param>
		 /// <returns> an <seealso cref="IndexCreator"/> capable of providing details for, as well as creating
		 /// an index for the given <seealso cref="Label label"/>. </returns>
		 IndexCreator CreateDeferredSchemaIndex( Label label );

		 /// <summary>
		 /// Returns a <seealso cref="ConstraintCreator"/> where details about the constraint can be
		 /// specified. When all details have been entered <seealso cref="ConstraintCreator.create()"/>
		 /// must be called for it to actually be created.
		 /// 
		 /// Note that the batch inserter is not enforcing any constraint on the inserted data
		 /// (also the one created with the batch inserter itself).  Although the batch inserter
		 /// will verify all constraints on <seealso cref="shutdown()"/> and fail in case of any constraint
		 /// violation.
		 /// </summary>
		 /// <param name="label"> the label this constraint is for. </param>
		 /// <returns> a <seealso cref="ConstraintCreator"/> capable of providing details for, as well as creating
		 /// a constraint for the given <seealso cref="Label label"/>. </returns>
		 ConstraintCreator CreateDeferredConstraint( Label label );

		 /// <summary>
		 /// Shuts down this batch inserter syncing all changes that are still only
		 /// in memory to disk. Failing to invoke this method may leave the Neo4j
		 /// store in a inconsistent state.
		 /// 
		 /// Note that this method will trigger population of all indexes, both
		 /// those created in the batch insertion session, as well as those that existed
		 /// previously. This may take a long time, depending on data size.
		 /// 
		 /// <para>
		 /// After this method has been invoked any other method call to this batch
		 /// inserter is illegal.
		 /// </para>
		 /// </summary>
		 void Shutdown();

		 /// <summary>
		 /// Returns the path to default neo4j database.
		 /// </summary>
		 /// <returns> the path to default Neo4j database. </returns>
		 string StoreDir { get; }

	}

}