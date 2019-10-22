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
	using Node = Neo4Net.GraphDb.Node;

	/// <summary>
	/// Abstraction of the priority queue used by Dijkstra in order to make (testing
	/// of) alternative implementations easier. </summary>
	/// @param <CostType>
	///            The datatype the path weights are represented by. </param>
	public interface DijkstraPriorityQueue<CostType>
	{
		 /// <summary>
		 /// Used to insert a new value into the queue. </summary>
		 /// <param name="node"> </param>
		 /// <param name="value"> </param>
		 void InsertValue( Node node, CostType value );

		 /// <summary>
		 /// Used to update a value in the queue (or insert it). </summary>
		 /// <param name="node"> </param>
		 /// <param name="newValue"> </param>
		 void DecreaseValue( Node node, CostType newValue );

		 /// <summary>
		 /// Retrieve and remove the node with the most optimal value.
		 /// </summary>
		 Node ExtractMin();

		 /// <summary>
		 /// Retrieve without removing the node with the most optimal value.
		 /// </summary>
		 Node Peek();

		 /// <returns> True if the queue is empty. </returns>
		 bool Empty { get; }
	}

}