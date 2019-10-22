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
namespace Neo4Net.Graphalgo.shortestpath
{
	using Neo4Net.Graphalgo.impl.shortestpath;
	using Neo4Net.Graphalgo.impl.shortestpath;
	using IntegerAdder = Neo4Net.Graphalgo.impl.util.IntegerAdder;
	using Direction = Neo4Net.GraphDb.Direction;
	using Node = Neo4Net.GraphDb.Node;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;

	public class SingleSourceShortestPathDijkstraTest : SingleSourceShortestPathTest
	{
		 protected internal override SingleSourceShortestPath<int> GetSingleSourceAlgorithm( Node startNode )
		 {
			  return new SingleSourceShortestPathDijkstra<int>( 0, startNode, ( relationship, direction ) => 1, new IntegerAdder(), int?.compareTo, Direction.BOTH, MyRelTypes.R1 );
		 }

		 protected internal override SingleSourceShortestPath<int> GetSingleSourceAlgorithm( Node startNode, Direction direction, params RelationshipType[] relTypes )
		 {
			  return new SingleSourceShortestPathDijkstra<int>( 0, startNode, ( relationship, direction1 ) => 1, new IntegerAdder(), int?.compareTo, direction, relTypes );
		 }
	}

}