﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Graphalgo.impl.shortestpath
{

	using Direction = Org.Neo4j.Graphdb.Direction;
	using Node = Org.Neo4j.Graphdb.Node;
	using PropertyContainer = Org.Neo4j.Graphdb.PropertyContainer;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;

	/// <summary>
	/// An object implementing this encapsulates an algorithm able to solve the
	/// single source single sink shortest path problem. I.e. it can find the
	/// shortest path(s) between two given nodes in a network.
	/// @author Patrik Larsson </summary>
	/// @param <CostType>
	///            The datatype the edge weights are represented by. </param>
	public interface SingleSourceSingleSinkShortestPath<CostType>
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
		 /// This sets the end node. The found paths will end in this node. </summary>
		 /// <param name="node">
		 ///            The end node. </param>
		 Node EndNode { set; }

		 /// <summary>
		 /// A call to this will run the algorithm to find a single shortest path, if
		 /// not already done, and return it as an alternating list of
		 /// Node/Relationship. </summary>
		 /// <returns> The path as an alternating list of Node/Relationship. </returns>
		 IList<PropertyContainer> Path { get; }

		 /// <summary>
		 /// A call to this will run the algorithm to find a single shortest path, if
		 /// not already done, and return it as a list of nodes. </summary>
		 /// <returns> The path as a list of nodes. </returns>
		 IList<Node> PathAsNodes { get; }

		 /// <summary>
		 /// A call to this will run the algorithm to find a single shortest path, if
		 /// not already done, and return it as a list of Relationships. </summary>
		 /// <returns> The path as a list of Relationships. </returns>
		 IList<Relationship> PathAsRelationships { get; }

		 /// <summary>
		 /// A call to this will run the algorithm to find all shortest paths, if not
		 /// already done, and return them as alternating lists of Node/Relationship. </summary>
		 /// <returns> A list of the paths as alternating lists of Node/Relationship. </returns>
		 IList<IList<PropertyContainer>> Paths { get; }

		 /// <summary>
		 /// A call to this will run the algorithm to find all shortest paths, if not
		 /// already done, and return them as lists of nodes. </summary>
		 /// <returns> A list of the paths as lists of nodes. </returns>
		 IList<IList<Node>> PathsAsNodes { get; }

		 /// <summary>
		 /// A call to this will run the algorithm to find all shortest paths, if not
		 /// already done, and return them as lists of relationships. </summary>
		 /// <returns> A list of the paths as lists of relationships. </returns>
		 IList<IList<Relationship>> PathsAsRelationships { get; }

		 /// <summary>
		 /// A call to this will run the algorithm to find the cost for the shortest
		 /// paths between the start node and the end node, if not calculated before.
		 /// This will usually find a single shortest path. </summary>
		 /// <returns> The total weight of the shortest path(s). </returns>
		 CostType Cost { get; }

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