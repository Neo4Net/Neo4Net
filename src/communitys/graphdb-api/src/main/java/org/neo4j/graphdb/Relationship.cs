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
	/// A relationship between two nodes in the graph. A relationship has a start
	/// node, an end node and a <seealso cref="RelationshipType type"/>. You can attach
	/// properties to relationships with the API specified in
	/// <seealso cref="PropertyContainer"/>.
	/// <para>
	/// Relationships are created by invoking the
	/// {@link Node#createRelationshipTo(Node, RelationshipType)
	/// Node.createRelationshipTo()} method on a node as follows:
	/// </para>
	/// <para>
	/// <code>
	/// Relationship rel = node.<seealso cref="Node.createRelationshipTo(Node, RelationshipType) createRelationshipTo"/>( otherNode, MyRels.REL_TYPE );
	/// </code>
	/// </para>
	/// <para>
	/// The fact that the relationship API gives meaning to {@link #getStartNode()
	/// start} and <seealso cref="getEndNode() end"/> nodes implicitly means that all
	/// relationships have a direction. In the example above, <code>rel</code> would
	/// be directed <i>from</i> <code>node</code> <i>to</i> <code>otherNode</code>. A
	/// relationship's start node and end node and their relation to
	/// <seealso cref="Direction.OUTGOING"/> and <seealso cref="Direction.INCOMING"/> are defined so that
	/// the assertions in the following code are <code>true</code>:
	/// 
	/// <pre>
	/// <code>
	/// <seealso cref="Node"/> a = graphDb.<seealso cref="GraphDatabaseService.createNode() createNode"/>();
	/// <seealso cref="Node"/> b = graphDb.<seealso cref="GraphDatabaseService.createNode() createNode"/>();
	/// <seealso cref="Relationship"/> rel = a.{@link Node#createRelationshipTo(Node, RelationshipType)
	/// createRelationshipTo}( b, <seealso cref="RelationshipType MyRels.REL_TYPE"/> );
	/// // Now we have: (a) --- REL_TYPE ---&gt; (b)
	/// 
	/// assert rel.<seealso cref="Relationship.getStartNode() getStartNode"/>().equals( a );
	/// assert rel.<seealso cref="Relationship.getEndNode() getEndNode"/>().equals( b );
	/// assert rel.<seealso cref="Relationship.getNodes() getNodes"/>()[0].equals( a ) &amp;&amp;
	///        rel.<seealso cref="Relationship.getNodes() getNodes"/>()[1].equals( b );
	/// </code>
	/// </pre>
	/// 
	/// Even though all relationships have a direction they are equally well
	/// traversed in both directions so there's no need to create duplicate
	/// relationships in the opposite direction (with regard to traversal or
	/// performance).
	/// </para>
	/// <para>
	/// Furthermore, Neo4j guarantees that a relationship is never "hanging freely,"
	/// i.e. <seealso cref="getStartNode()"/>, <seealso cref="getEndNode()"/>,
	/// <seealso cref="getOtherNode(Node)"/> and <seealso cref="getNodes()"/> are guaranteed to always
	/// return valid, non-null nodes.
	/// </para>
	/// <para>
	/// A relationship's id is unique, but note the following: Neo4j reuses its internal ids
	/// when nodes and relationships are deleted, which means it's bad practice to
	/// refer to them this way. Instead, use application generated ids.
	/// </para>
	/// </summary>
	public interface Relationship : Entity
	{
		 /// <summary>
		 /// Returns the unique id of this relationship. Ids are garbage collected
		 /// over time so they are only guaranteed to be unique during a specific time
		 /// span: if the relationship is deleted, it's likely that a new relationship
		 /// at some point will get the old id. <b>Note</b>: This makes relationship
		 /// ids brittle as public APIs.
		 /// </summary>
		 /// <returns> The id of this relationship </returns>
		 long Id { get; }

		 /// <summary>
		 /// Deletes this relationship. Invoking any methods on this relationship
		 /// after <code>delete()</code> has returned is invalid and will lead to
		 /// <seealso cref="NotFoundException"/> being thrown.
		 /// </summary>
		  void Delete();

		 // Node accessors
		 /// <summary>
		 /// Returns the start node of this relationship. For a definition of how
		 /// start node relates to <seealso cref="Direction directions"/> as arguments to the
		 /// <seealso cref="Node.getRelationships() relationship accessors"/> in Node, see the
		 /// class documentation of Relationship.
		 /// </summary>
		 /// <returns> the start node of this relationship </returns>
		 Node StartNode { get; }

		 /// <summary>
		 /// Returns the end node of this relationship. For a definition of how end
		 /// node relates to <seealso cref="Direction directions"/> as arguments to the
		 /// <seealso cref="Node.getRelationships() relationship accessors"/> in Node, see the
		 /// class documentation of Relationship.
		 /// </summary>
		 /// <returns> the end node of this relationship </returns>
		 Node EndNode { get; }

		 /// <summary>
		 /// A convenience operation that, given a node that is attached to this
		 /// relationship, returns the other node. For example if <code>node</code> is
		 /// a start node, the end node will be returned, and vice versa. This is a
		 /// very convenient operation when you're manually traversing the graph
		 /// by invoking one of the <seealso cref="Node.getRelationships() getRelationships()"/>
		 /// operations on a node. For example, to get the node "at the other end" of
		 /// a relationship, use the following:
		 /// <para>
		 /// <code>
		 /// Node endNode = node.getSingleRelationship( MyRels.REL_TYPE ).getOtherNode( node );
		 /// </code>
		 /// </para>
		 /// <para>
		 /// This operation will throw a runtime exception if <code>node</code> is
		 /// neither this relationship's start node nor its end node.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="node"> the start or end node of this relationship </param>
		 /// <returns> the other node </returns>
		 /// <exception cref="RuntimeException"> if the given node is neither the start nor end
		 ///             node of this relationship </exception>
		 Node GetOtherNode( Node node );

		 /// <summary>
		 /// Returns the two nodes that are attached to this relationship. The first
		 /// element in the array will be the start node, the second element the end
		 /// node.
		 /// </summary>
		 /// <returns> the two nodes that are attached to this relationship </returns>
		 Node[] Nodes { get; }

		 /// <summary>
		 /// Returns the type of this relationship. A relationship's type is an
		 /// immutable attribute that is specified at Relationship
		 /// <seealso cref="Node.createRelationshipTo creation"/>. Remember that relationship
		 /// types are semantically equivalent if their
		 /// <seealso cref="RelationshipType.name() names"/> are {@link Object#equals(Object)
		 /// equal}. This is NOT the same as checking for identity equality using the
		 /// == operator. If you want to know whether this relationship is of a
		 /// certain type, use the <seealso cref="isType(RelationshipType) isType()"/>
		 /// operation.
		 /// </summary>
		 /// <returns> the type of this relationship </returns>
		 RelationshipType Type { get; }

		 /// <summary>
		 /// Indicates whether this relationship is of the type <code>type</code>.
		 /// This is a convenience method that checks for equality using the contract
		 /// specified by RelationshipType, i.e. by checking for equal
		 /// <seealso cref="RelationshipType.name() names"/>.
		 /// </summary>
		 /// <param name="type"> the type to check </param>
		 /// <returns> <code>true</code> if this relationship is of the type
		 ///         <code>type</code>, <code>false</code> otherwise or if
		 ///         <code>null</code> </returns>
		 bool IsType( RelationshipType type );

		 /// <summary>
		 /// Returns the id of the start node of this relationship. For a definition of how
		 /// start node relates to <seealso cref="Direction directions"/> as arguments to the
		 /// <seealso cref="Node.getRelationships() relationship accessors"/> in Node, see the
		 /// class documentation of Relationship.
		 /// <para>
		 /// Note that this id can get reused in the future, if this relationship and the given node are deleted.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> the id of the start node of this relationship. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default long getStartNodeId()
	//	 {
	//		  return getStartNode().getId();
	//	 }

		 /// <summary>
		 /// Returns the id of the end node of this relationship. For a definition of how end
		 /// node relates to <seealso cref="Direction directions"/> as arguments to the
		 /// <seealso cref="Node.getRelationships() relationship accessors"/> in Node, see the
		 /// class documentation of Relationship.
		 /// <para>
		 /// Note that this id can get reused in the future, if this relationship and the given node are deleted.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> the id of the end node of this relationship. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default long getEndNodeId()
	//	 {
	//		  return getEndNode().getId();
	//	 }

		 /// <summary>
		 /// A convenience operation that, given an id of a node that is attached to this
		 /// relationship, returns the id of the other node. For example if <code>id</code> is
		 /// the start node id, the end node id will be returned, and vice versa.
		 /// <para>
		 /// This operation will throw a runtime exception if <code>id</code> is
		 /// not the id of either of this relationship's nodes.
		 /// </para>
		 /// <para>
		 /// Note that this id can get reused in the future, if this relationship and the given node are deleted.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="id"> the id of the start or end node of this relationship </param>
		 /// <returns> the id of the other node </returns>
		 /// <exception cref="RuntimeException"> if the given node id is not the id of either the start or end
		 ///             node of this relationship. </exception>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default long getOtherNodeId(long id)
	//	 {
	//		  long start = getStartNodeId();
	//		  long end = getEndNodeId();
	//		  if (id == start)
	//		  {
	//				return end;
	//		  }
	//		  else if (id == end)
	//		  {
	//				return start;
	//		  }
	//		  throw new NotFoundException("Node[" + id + "] not connected to this relationship[" + getId() + "]");
	//	 }
	}

}