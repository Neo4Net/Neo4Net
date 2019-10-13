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
namespace Neo4Net.Graphdb
{
	/// <summary>
	/// A node in the graph with properties and relationships to other entities.
	/// Along with <seealso cref="Relationship relationships"/>, nodes are the core building
	/// blocks of the Neo4j data representation model. Nodes are created by invoking
	/// the <seealso cref="GraphDatabaseService.createNode"/> method.
	/// <para>
	/// Node has three major groups of operations: operations that deal with
	/// relationships, operations that deal with properties (see
	/// <seealso cref="PropertyContainer"/>) and operations that deal with <seealso cref="Label labels"/>.
	/// </para>
	/// <para>
	/// The relationship operations provide a number of overloaded accessors (such as
	/// <code>getRelationships(...)</code> with "filters" for type, direction, etc),
	/// as well as the factory method {@link #createRelationshipTo
	/// createRelationshipTo(...)} that connects two nodes with a relationship. It
	/// also includes the convenience method {@link #getSingleRelationship
	/// getSingleRelationship(...)} for accessing the commonly occurring
	/// one-to-zero-or-one association.
	/// </para>
	/// <para>
	/// The property operations give access to the key-value property pairs. Property
	/// keys are always strings. Valid property value types are all the Java
	/// primitives (<code>int</code>, <code>byte</code>, <code>float</code>, etc),
	/// <code>java.lang.String</code>s and arrays of primitives and Strings.
	/// </para>
	/// <para>
	/// <b>Please note</b> that Neo4j does NOT accept arbitrary objects as property
	/// values. <seealso cref="setProperty(string, object) setProperty()"/> takes a
	/// <code>java.lang.Object</code> only to avoid an explosion of overloaded
	/// <code>setProperty()</code> methods. For further documentation see
	/// <seealso cref="PropertyContainer"/>.
	/// </para>
	/// <para>
	/// A node's id is unique, but note the following: Neo4j reuses its internal ids
	/// when nodes and relationships are deleted, which means it's bad practice to
	/// refer to them this way. Instead, use application generated ids.
	/// </para>
	/// </summary>
	public interface Node : Entity
	{
		 /// <summary>
		 /// Returns the unique id of this node. Ids are garbage collected over time
		 /// so they are only guaranteed to be unique during a specific time span: if
		 /// the node is deleted, it's likely that a new node at some point will get
		 /// the old id. <b>Note</b>: This makes node ids brittle as public APIs.
		 /// </summary>
		 /// <returns> the id of this node </returns>
		 long Id { get; }

		 /// <summary>
		 /// Deletes this node if it has no relationships attached to it. If
		 /// <code>delete()</code> is invoked on a node with relationships, an
		 /// unchecked exception will be raised when the transaction is committing.
		 /// Invoking any methods on this node after <code>delete()</code> has
		 /// returned is invalid and will lead to <seealso cref="NotFoundException"/> being thrown.
		 /// </summary>
		 void Delete();

		 /// <summary>
		 /// Returns all the relationships attached to this node. If no relationships
		 /// are attached to this node, an empty iterable will be returned.
		 /// </summary>
		 /// <returns> all relationships attached to this node </returns>
		 IEnumerable<Relationship> Relationships { get; }

		 /// <summary>
		 /// Returns <code>true</code> if there are any relationships attached to this
		 /// node, <code>false</code> otherwise.
		 /// </summary>
		 /// <returns> <code>true</code> if there are any relationships attached to this
		 ///         node, <code>false</code> otherwise </returns>
		 bool HasRelationship();

		 /// <summary>
		 /// Returns all the relationships of any of the types in <code>types</code>
		 /// that are attached to this node, regardless of direction. If no
		 /// relationships of the given types are attached to this node, an empty
		 /// iterable will be returned.
		 /// </summary>
		 /// <param name="types"> the given relationship type(s) </param>
		 /// <returns> all relationships of the given type(s) that are attached to this
		 ///         node </returns>
		 IEnumerable<Relationship> GetRelationships( params RelationshipType[] types );

		 /// <summary>
		 /// Returns all the relationships of any of the types in <code>types</code>
		 /// that are attached to this node and have the given <code>direction</code>.
		 /// If no relationships of the given types are attached to this node, an empty
		 /// iterable will be returned.
		 /// </summary>
		 /// <param name="types"> the given relationship type(s) </param>
		 /// <param name="direction"> the direction of the relationships to return. </param>
		 /// <returns> all relationships of the given type(s) that are attached to this
		 ///         node </returns>
		 IEnumerable<Relationship> GetRelationships( Direction direction, params RelationshipType[] types );

		 /// <summary>
		 /// Returns <code>true</code> if there are any relationships of any of the
		 /// types in <code>types</code> attached to this node (regardless of
		 /// direction), <code>false</code> otherwise.
		 /// </summary>
		 /// <param name="types"> the given relationship type(s) </param>
		 /// <returns> <code>true</code> if there are any relationships of any of the
		 ///         types in <code>types</code> attached to this node,
		 ///         <code>false</code> otherwise </returns>
		 bool HasRelationship( params RelationshipType[] types );

		 /// <summary>
		 /// Returns <code>true</code> if there are any relationships of any of the
		 /// types in <code>types</code> attached to this node (for the given
		 /// <code>direction</code>), <code>false</code> otherwise.
		 /// </summary>
		 /// <param name="types"> the given relationship type(s) </param>
		 /// <param name="direction"> the direction to check relationships for </param>
		 /// <returns> <code>true</code> if there are any relationships of any of the
		 ///         types in <code>types</code> attached to this node,
		 ///         <code>false</code> otherwise </returns>
		 bool HasRelationship( Direction direction, params RelationshipType[] types );

		 /// <summary>
		 /// Returns all <seealso cref="Direction.OUTGOING OUTGOING"/> or
		 /// <seealso cref="Direction.INCOMING INCOMING"/> relationships from this node. If
		 /// there are no relationships with the given direction attached to this
		 /// node, an empty iterable will be returned. If <seealso cref="Direction.BOTH BOTH"/>
		 /// is passed in as a direction, relationships of both directions are
		 /// returned (effectively turning this into <code>getRelationships()</code>).
		 /// </summary>
		 /// <param name="dir"> the given direction, where <code>Direction.OUTGOING</code>
		 ///            means all relationships that have this node as
		 ///            <seealso cref="Relationship.getStartNode() start node"/> and <code>
		 /// Direction.INCOMING</code>
		 ///            means all relationships that have this node as
		 ///            <seealso cref="Relationship.getEndNode() end node"/> </param>
		 /// <returns> all relationships with the given direction that are attached to
		 ///         this node </returns>
		 IEnumerable<Relationship> GetRelationships( Direction dir );

		 /// <summary>
		 /// Returns <code>true</code> if there are any relationships in the given
		 /// direction attached to this node, <code>false</code> otherwise. If
		 /// <seealso cref="Direction.BOTH BOTH"/> is passed in as a direction, relationships of
		 /// both directions are matched (effectively turning this into
		 /// <code>hasRelationships()</code>).
		 /// </summary>
		 /// <param name="dir"> the given direction, where <code>Direction.OUTGOING</code>
		 ///            means all relationships that have this node as
		 ///            <seealso cref="Relationship.getStartNode() start node"/> and <code>
		 /// Direction.INCOMING</code>
		 ///            means all relationships that have this node as
		 ///            <seealso cref="Relationship.getEndNode() end node"/> </param>
		 /// <returns> <code>true</code> if there are any relationships in the given
		 ///         direction attached to this node, <code>false</code> otherwise </returns>
		 bool HasRelationship( Direction dir );

		 /// <summary>
		 /// Returns all relationships with the given type and direction that are
		 /// attached to this node. If there are no matching relationships, an empty
		 /// iterable will be returned.
		 /// </summary>
		 /// <param name="type"> the given type </param>
		 /// <param name="dir"> the given direction, where <code>Direction.OUTGOING</code>
		 ///            means all relationships that have this node as
		 ///            <seealso cref="Relationship.getStartNode() start node"/> and <code>
		 /// Direction.INCOMING</code>
		 ///            means all relationships that have this node as
		 ///            <seealso cref="Relationship.getEndNode() end node"/> </param>
		 /// <returns> all relationships attached to this node that match the given type
		 ///         and direction </returns>
		 IEnumerable<Relationship> GetRelationships( RelationshipType type, Direction dir );

		 /// <summary>
		 /// Returns <code>true</code> if there are any relationships of the given
		 /// relationship type and direction attached to this node, <code>false</code>
		 /// otherwise.
		 /// </summary>
		 /// <param name="type"> the given type </param>
		 /// <param name="dir"> the given direction, where <code>Direction.OUTGOING</code>
		 ///            means all relationships that have this node as
		 ///            <seealso cref="Relationship.getStartNode() start node"/> and <code>
		 /// Direction.INCOMING</code>
		 ///            means all relationships that have this node as
		 ///            <seealso cref="Relationship.getEndNode() end node"/> </param>
		 /// <returns> <code>true</code> if there are any relationships of the given
		 ///         relationship type and direction attached to this node,
		 ///         <code>false</code> otherwise </returns>
		 bool HasRelationship( RelationshipType type, Direction dir );

		 /// <summary>
		 /// Returns the only relationship of a given type and direction that is
		 /// attached to this node, or <code>null</code>. This is a convenience method
		 /// that is used in the commonly occurring situation where a node has exactly
		 /// zero or one relationships of a given type and direction to another node.
		 /// Typically this invariant is maintained by the rest of the code: if at any
		 /// time more than one such relationships exist, it is a fatal error that
		 /// should generate an unchecked exception. This method reflects that
		 /// semantics and returns either:
		 /// <ol>
		 /// <li><code>null</code> if there are zero relationships of the given type
		 /// and direction,</li>
		 /// <li>the relationship if there's exactly one, or
		 /// <li>throws an unchecked exception in all other cases.</li>
		 /// </ol>
		 /// <para>
		 /// This method should be used only in situations with an invariant as
		 /// described above. In those situations, a "state-checking" method (e.g.
		 /// <code>hasSingleRelationship(...)</code>) is not required, because this
		 /// method behaves correctly "out of the box."
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="type"> the type of the wanted relationship </param>
		 /// <param name="dir"> the direction of the wanted relationship (where
		 ///            <code>Direction.OUTGOING</code> means a relationship that has
		 ///            this node as <seealso cref="Relationship.getStartNode() start node"/>
		 ///            and <code>
		 /// Direction.INCOMING</code> means a relationship that has
		 ///            this node as <seealso cref="Relationship.getEndNode() end node"/>) or
		 ///            <seealso cref="Direction.BOTH"/> if direction is irrelevant </param>
		 /// <returns> the single relationship matching the given type and direction if
		 ///         exactly one such relationship exists, or <code>null</code> if
		 ///         exactly zero such relationships exists </returns>
		 /// <exception cref="RuntimeException"> if more than one relationship matches the given
		 ///             type and direction </exception>
		 Relationship GetSingleRelationship( RelationshipType type, Direction dir );

		 /// <summary>
		 /// Creates a relationship between this node and another node. The
		 /// relationship is of type <code>type</code>. It starts at this node and
		 /// ends at <code>otherNode</code>.
		 /// <para>
		 /// A relationship is equally well traversed in both directions so there's no
		 /// need to create another relationship in the opposite direction (in regards
		 /// to traversal or performance).
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="otherNode"> the end node of the new relationship </param>
		 /// <param name="type"> the type of the new relationship </param>
		 /// <returns> the newly created relationship </returns>
		 Relationship CreateRelationshipTo( Node otherNode, RelationshipType type );

		 /// <summary>
		 /// Returns relationship types which this node has one more relationships
		 /// for. If this node doesn't have any relationships an empty <seealso cref="System.Collections.IEnumerable"/>
		 /// will be returned. </summary>
		 /// <returns> relationship types which this node has one more relationships for. </returns>
		 IEnumerable<RelationshipType> RelationshipTypes { get; }

		 /// <summary>
		 /// Returns the number of relationships connected to this node regardless of
		 /// direction or type. This operation is always O(1). </summary>
		 /// <returns> the number of relationships connected to this node. </returns>
		 int Degree { get; }

		 /// <summary>
		 /// Returns the number of relationships of a given {@code type} connected to this node.
		 /// </summary>
		 /// <param name="type"> the type of relationships to get the degree for </param>
		 /// <returns> the number of relationships of a given {@code type} connected to this node. </returns>
		 int GetDegree( RelationshipType type );

		 /// <summary>
		 /// Returns the number of relationships of a given {@code direction} connected to this node.
		 /// </summary>
		 /// <param name="direction"> the direction of the relationships </param>
		 /// <returns> the number of relationships of a given {@code direction} for this node. </returns>
		 int GetDegree( Direction direction );

		 /// <summary>
		 /// Returns the number of relationships of a given {@code type} and {@code direction}
		 /// connected to this node.
		 /// </summary>
		 /// <param name="type"> the type of relationships to get the degree for </param>
		 /// <param name="direction"> the direction of the relationships </param>
		 /// <returns> the number of relationships of a given {@code type} and {@code direction}
		 /// for this node. </returns>
		 int GetDegree( RelationshipType type, Direction direction );

		 /// <summary>
		 /// Adds a <seealso cref="Label"/> to this node. If this node doesn't already have
		 /// this label it will be added. If it already has the label, nothing will happen.
		 /// </summary>
		 /// <param name="label"> the label to add to this node. </param>
		 void AddLabel( Label label );

		 /// <summary>
		 /// Removes a <seealso cref="Label"/> from this node. If this node doesn't have this label,
		 /// nothing will happen.
		 /// </summary>
		 /// <param name="label"> the label to remove from this node. </param>
		 void RemoveLabel( Label label );

		 /// <summary>
		 /// Checks whether or not this node has the given label.
		 /// </summary>
		 /// <param name="label"> the label to check for. </param>
		 /// <returns> {@code true} if this node has the given label, otherwise {@code false}. </returns>
		 bool HasLabel( Label label );

		 /// <summary>
		 /// Lists all labels attached to this node. If this node has no
		 /// labels an empty <seealso cref="System.Collections.IEnumerable"/> will be returned.
		 /// </summary>
		 /// <returns> all labels attached to this node. </returns>
		 IEnumerable<Label> Labels { get; }
	}

}