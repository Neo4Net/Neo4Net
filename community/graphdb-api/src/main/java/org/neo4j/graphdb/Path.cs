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
namespace Org.Neo4j.Graphdb
{

	/// <summary>
	/// Represents a path in the graph. A path starts with a node followed by
	/// pairs of <seealso cref="Relationship"/> and <seealso cref="Node"/> objects. The shortest path
	/// is of length 0. Such a path contains only one node and no relationships.
	/// 
	/// During a traversal <seealso cref="Path"/> instances are emitted where the current
	/// position of the traverser is represented by each such path. The current
	/// node in such a traversal is reached via <seealso cref="Path.endNode()"/>.
	/// </summary>
	public interface Path : IEnumerable<PropertyContainer>
	{
		 /// <summary>
		 /// Returns the start node of this path. It's also the first node returned
		 /// from the <seealso cref="nodes()"/> iterable. </summary>
		 /// <returns> the start node. </returns>
		 Node StartNode();

		 /// <summary>
		 /// Returns the end node of this path. It's also the last node returned
		 /// from <seealso cref="nodes()"/> iterable. If the <seealso cref="length()"/> of this path
		 /// is 0 the end node returned by this method is the same as the start node.
		 /// 
		 /// If a path is emitted from a traverser the end node is the current node
		 /// where the traverser is at the moment.
		 /// </summary>
		 /// <returns> the end node. </returns>
		 Node EndNode();

		 /// <summary>
		 /// Returns the last <seealso cref="Relationship"/> in this path.
		 /// </summary>
		 /// <returns> the last <seealso cref="Relationship"/> in this path, or <code>null</code>
		 ///         if this path contains no <seealso cref="Relationship"/>s. </returns>
		 Relationship LastRelationship();

		 /// <summary>
		 /// Returns all the relationships in between the nodes which this path
		 /// consists of. For a path with <seealso cref="length()"/> 0 this will be an
		 /// empty <seealso cref="System.Collections.IEnumerable"/>. </summary>
		 /// <returns> the relationships in this path. </returns>
		 IEnumerable<Relationship> Relationships();

		 /// <summary>
		 /// Returns all the relationships in between the nodes which this path
		 /// consists of in reverse order, i.e. starting from the <seealso cref="lastRelationship()"/>
		 /// going backwards towards the first relationship in the path.
		 /// For a path with <seealso cref="length()"/> 0 this will be an empty <seealso cref="System.Collections.IEnumerable"/>. </summary>
		 /// <returns> the relationships in this path in reverse order. </returns>
		 IEnumerable<Relationship> ReverseRelationships();

		 /// <summary>
		 /// Returns all the nodes in this path starting from the start node going
		 /// forward towards the end node. The first node is the same as
		 /// <seealso cref="startNode()"/> and the last node is the same as <seealso cref="endNode()"/>.
		 /// In between those nodes there can be an arbitrary number of nodes. The
		 /// shortest path possible is just one node, where also the the start node is
		 /// the same as the end node.
		 /// </summary>
		 /// <returns> the nodes in this path. </returns>
		 IEnumerable<Node> Nodes();

		 /// <summary>
		 /// Returns all the nodes in this path in reversed order, i.e. starting from the
		 /// end node going backwards instead of from the start node going forwards.
		 /// The first node is the same as <seealso cref="endNode()"/> and the last node is the
		 /// same as <seealso cref="startNode()"/>. In between those nodes there can be an arbitrary
		 /// number of nodes. The shortest path possible is just one node, where also the
		 /// the start node is the same as the end node.
		 /// </summary>
		 /// <returns> the nodes in this path starting from the end node going backwards
		 /// towards the start node. </returns>
		 IEnumerable<Node> ReverseNodes();

		 /// <summary>
		 /// Returns the length of this path. That is the number of relationships
		 /// (which is the same as the number of nodes minus one). The shortest path
		 /// possible is of length 0.
		 /// </summary>
		 /// <returns> the length (i.e. the number of relationships) in the path. </returns>
		 int Length();

		 /// <summary>
		 /// Returns a natural string representation of this path.
		 /// 
		 /// The string representation shows the nodes with relationships
		 /// (and their directions) in between them.
		 /// </summary>
		 /// <returns> A string representation of the path. </returns>
		 String ();

		 /// <summary>
		 /// Iterates through both the <seealso cref="Node"/>s and <seealso cref="Relationship"/>s of this
		 /// path in order. Interleaving <seealso cref="Node"/>s with <seealso cref="Relationship"/>s,
		 /// starting and ending with a <seealso cref="Node"/> (the <seealso cref="startNode()"/> and
		 /// <seealso cref="endNode()"/> respectively).
		 /// </summary>
		 /// <seealso cref= Iterable#iterator() </seealso>
		 IEnumerator<PropertyContainer> Iterator();
	}

}