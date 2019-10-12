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
namespace Org.Neo4j.Graphalgo.shortestpath
{
	using Org.Neo4j.Graphalgo.impl.shortestpath;
	using SingleSourceShortestPathBFS = Org.Neo4j.Graphalgo.impl.shortestpath.SingleSourceShortestPathBFS;
	using Direction = Org.Neo4j.Graphdb.Direction;
	using Node = Org.Neo4j.Graphdb.Node;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;

	public class SingleSourceShortestPathBFSTest : SingleSourceShortestPathTest
	{
		 protected internal override SingleSourceShortestPath<int> GetSingleSourceAlgorithm( Node startNode )
		 {
			  SingleSourceShortestPathBFS sourceBFS = new SingleSourceShortestPathBFS( startNode, Direction.BOTH, MyRelTypes.R1 );
			  return sourceBFS;
		 }

		 protected internal override SingleSourceShortestPath<int> GetSingleSourceAlgorithm( Node startNode, Direction direction, params RelationshipType[] relTypes )
		 {
			  return new SingleSourceShortestPathBFS( startNode, direction, relTypes );
		 }
	}

}