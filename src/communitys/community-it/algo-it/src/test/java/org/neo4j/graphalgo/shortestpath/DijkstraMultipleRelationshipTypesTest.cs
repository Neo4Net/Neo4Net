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
	using Neo4jAlgoTestCase = Common.Neo4jAlgoTestCase;
	using Test = org.junit.Test;

	using Neo4Net.Graphalgo.impl.shortestpath;
	using DoubleAdder = Neo4Net.Graphalgo.impl.util.DoubleAdder;
	using Direction = Neo4Net.Graphdb.Direction;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class DijkstraMultipleRelationshipTypesTest : Neo4jAlgoTestCase
	{
		 protected internal virtual Dijkstra<double> GetDijkstra( string startNode, string endNode, params RelationshipType[] relTypes )
		 {
			  return new Dijkstra<double>( 0.0, Graph.getNode( startNode ), Graph.getNode( endNode ), ( relationship, direction ) => 1.0, new DoubleAdder(), double?.compareTo, Direction.BOTH, relTypes );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRun()
		 public virtual void TestRun()
		 {
			  Graph.CurrentRelType = MyRelTypes.R1;
			  Graph.makeEdgeChain( "a,b,c,d,e" );
			  Graph.CurrentRelType = MyRelTypes.R2;
			  Graph.makeEdges( "a,c" ); // first shortcut
			  Graph.CurrentRelType = MyRelTypes.R3;
			  Graph.makeEdges( "c,e" ); // second shortcut
			  Dijkstra<double> dijkstra;
			  dijkstra = GetDijkstra( "a", "e", MyRelTypes.R1 );
			  assertEquals( 4.0, dijkstra.Cost, 0.0 );
			  dijkstra = GetDijkstra( "a", "e", MyRelTypes.R1, MyRelTypes.R2 );
			  assertEquals( 3.0, dijkstra.Cost, 0.0 );
			  dijkstra = GetDijkstra( "a", "e", MyRelTypes.R1, MyRelTypes.R3 );
			  assertEquals( 3.0, dijkstra.Cost, 0.0 );
			  dijkstra = GetDijkstra( "a", "e", MyRelTypes.R1, MyRelTypes.R2, MyRelTypes.R3 );
			  assertEquals( 2.0, dijkstra.Cost, 0.0 );
		 }
	}

}