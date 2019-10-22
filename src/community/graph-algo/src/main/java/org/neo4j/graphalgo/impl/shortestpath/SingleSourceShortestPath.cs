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
namespace Neo4Net.Graphalgo.impl.shortestpath
{

	using Direction = Neo4Net.GraphDb.Direction;
	using Node = Neo4Net.GraphDb.Node;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;

	/// <summary>
	/// An object implementing this encapsulates an algorithm able to solve the
	/// single source shortest path problem. I.e. it can find the shortest path(s)
	/// from a given start node to all other nodes in a network.
	/// @author Patrik Larsson </summary>
	/// @param <CostType>
	///            The datatype the edge weights are represented by. </param>
	public interface SingleSourceShortestPath<CostType>
	{
		 /// <summary>
		 /// This resets the calculation if we for some reason would like to redo it.
		 /// </summary>
		 void Reset();

		 /// <summary>
		 /// This sets the start node. The found paths will start in this node. </summary>
		 /// <param name="node">
		 ///            The start node. </param>
		 Node StartNode { set; }

		 /// <summary>
		 /// A call to this will run the algorithm to find a single shortest path, if
		 /// not already done, and return it as an alternating list of
		 /// Node/Relationship. </summary>
		 /// <returns> The path as an alternating list of Node/Relationship. </returns>
		 IList<PropertyContainer> GetPath( Node targetNode );

		 /// <summary>
		 /// A call to this will run the algorithm, if not already done, and return
		 /// the found path to the target node if found as a list of nodes. </summary>
		 /// <returns> The path as a list of nodes. </returns>
		 IList<Node> GetPathAsNodes( Node targetNode );

		 /// <summary>
		 /// A call to this will run the algorithm to find a single shortest path, if
		 /// not already done, and return it as a list of Relationships. </summary>
		 /// <returns> The path as a list of Relationships. </returns>
		 IList<Relationship> GetPathAsRelationships( Node targetNode );

		 /// <summary>
		 /// A call to this will run the algorithm to find all shortest paths, if not
		 /// already done, and return them as alternating lists of Node/Relationship. </summary>
		 /// <returns> A list of the paths as alternating lists of Node/Relationship. </returns>
		 IList<IList<PropertyContainer>> GetPaths( Node targetNode );

		 /// <summary>
		 /// A call to this will run the algorithm to find all shortest paths, if not
		 /// already done, and return them as lists of nodes. </summary>
		 /// <returns> A list of the paths as lists of nodes. </returns>
		 IList<IList<Node>> GetPathsAsNodes( Node targetNode );

		 /// <summary>
		 /// A call to this will run the algorithm to find all shortest paths, if not
		 /// already done, and return them as lists of relationships. </summary>
		 /// <returns> A list of the paths as lists of relationships. </returns>
		 IList<IList<Relationship>> GetPathsAsRelationships( Node targetNode );

		 /// <summary>
		 /// A call to this will run the algorithm, if not already done, and return
		 /// the cost for the shortest paths between the start node and the target
		 /// node. </summary>
		 /// <returns> The total weight of the shortest path(s). </returns>
		 CostType GetCost( Node targetNode );

		 /// <param name="node"> </param>
		 /// <returns> The nodes previous to the argument node in all found shortest
		 ///         paths or null if there are no such nodes. </returns>
		 IList<Node> GetPredecessorNodes( Node node );

		 /// <summary>
		 /// This can be used to retrieve the entire data structure representing the
		 /// predecessors for every node.
		 /// @return
		 /// </summary>
		 IDictionary<Node, IList<Relationship>> Predecessors { get; }

		 /// <summary>
		 /// This can be used to retrieve the Direction in which relationships should
		 /// be in the shortest path(s). </summary>
		 /// <returns> The direction. </returns>
		 Direction Direction { get; }

		 /// <summary>
		 /// This can be used to retrieve the types of relationships that are
		 /// traversed. </summary>
		 /// <returns> The relationship type(s). </returns>
		 RelationshipType[] RelationshipTypes { get; }
	}

}