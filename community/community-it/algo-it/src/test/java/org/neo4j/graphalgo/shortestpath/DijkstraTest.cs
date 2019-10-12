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
	using Neo4jAlgoTestCase = Common.Neo4jAlgoTestCase;
	using SimpleGraphBuilder = Common.SimpleGraphBuilder;
	using Test = org.junit.Test;

	using Org.Neo4j.Graphalgo.impl.shortestpath;
	using Direction = Org.Neo4j.Graphdb.Direction;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;

	public class DijkstraTest : Neo4jAlgoTestCase
	{
		 protected internal virtual Dijkstra<double> GetDijkstra( SimpleGraphBuilder graph, double? startCost, string startNode, string endNode )
		 {
			  return new Dijkstra<double>( startCost, graph.GetNode( startNode ), graph.GetNode( endNode ), CommonEvaluators.doubleCostEvaluator( "cost" ), new Org.Neo4j.Graphalgo.impl.util.DoubleAdder(), double?.compareTo, Direction.BOTH, MyRelTypes.R1 );
		 }

		 /// <summary>
		 /// Test case for just a single node (path length zero)
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDijkstraMinimal()
		 public virtual void TestDijkstraMinimal()
		 {
			  Graph.makeNode( "lonely" );
			  Dijkstra<double> dijkstra = GetDijkstra( Graph, 0.0, "lonely", "lonely" );
			  assertEquals( 0.0, dijkstra.Cost, 0.0 );
			  assertEquals( 1, dijkstra.PathAsNodes.Count );
			  dijkstra = GetDijkstra( Graph, 3.0, "lonely", "lonely" );
			  assertEquals( 6.0, dijkstra.Cost, 0.0 );
			  assertEquals( 1, dijkstra.PathAsNodes.Count );
			  assertEquals( 1, dijkstra.PathsAsNodes.Count );
		 }

		 /// <summary>
		 /// Test case for a path of length zero, with some surrounding nodes
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDijkstraMinimal2()
		 public virtual void TestDijkstraMinimal2()
		 {
			  Graph.makeEdge( "a", "b", "cost", ( double ) 1 );
			  Graph.makeEdge( "a", "c", "cost", ( float ) 1 );
			  Graph.makeEdge( "a", "d", "cost", ( long ) 1 );
			  Graph.makeEdge( "a", "e", "cost", 1 );
			  Graph.makeEdge( "b", "c", "cost", ( sbyte ) 1 );
			  Graph.makeEdge( "c", "d", "cost", ( short ) 1 );
			  Graph.makeEdge( "d", "e", "cost", ( double ) 1 );
			  Graph.makeEdge( "e", "f", "cost", ( double ) 1 );
			  Dijkstra<double> dijkstra = GetDijkstra( Graph, 0.0, "a", "a" );
			  assertEquals( 0.0, dijkstra.Cost, 0.0 );
			  assertEquals( 1, dijkstra.PathAsNodes.Count );
			  dijkstra = GetDijkstra( Graph, 3.0, "a", "a" );
			  assertEquals( 6.0, dijkstra.Cost, 0.0 );
			  assertEquals( 1, dijkstra.PathAsNodes.Count );
			  assertEquals( 0, dijkstra.PathAsRelationships.Count );
			  assertEquals( 1, dijkstra.Path.Count );
			  assertEquals( 1, dijkstra.PathsAsNodes.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDijkstraChain()
		 public virtual void TestDijkstraChain()
		 {
			  Graph.makeEdge( "a", "b", "cost", ( double ) 1 );
			  Graph.makeEdge( "b", "c", "cost", ( float ) 2 );
			  Graph.makeEdge( "c", "d", "cost", ( sbyte ) 3 );
			  Dijkstra<double> dijkstra = GetDijkstra( Graph, 0.0, "a", "d" );
			  assertEquals( 6.0, dijkstra.Cost, 0.0 );
			  assertNotNull( dijkstra.PathAsNodes );
			  assertEquals( 4, dijkstra.PathAsNodes.Count );
			  assertEquals( 1, dijkstra.PathsAsNodes.Count );
			  dijkstra = GetDijkstra( Graph, 0.0, "d", "a" );
			  assertEquals( 6.0, dijkstra.Cost, 0.0 );
			  assertEquals( 4, dijkstra.PathAsNodes.Count );
			  dijkstra = GetDijkstra( Graph, 0.0, "d", "b" );
			  assertEquals( 5.0, dijkstra.Cost, 0.0 );
			  assertEquals( 3, dijkstra.PathAsNodes.Count );
			  assertEquals( 2, dijkstra.PathAsRelationships.Count );
			  assertEquals( 5, dijkstra.Path.Count );
		 }

		 /// <summary>
		 /// /--2--A--7--B--2--\ S E \----7---C---7----/
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDijkstraTraverserMeeting()
		 public virtual void TestDijkstraTraverserMeeting()
		 {
			  Graph.makeEdge( "s", "c", "cost", ( double ) 7 );
			  Graph.makeEdge( "c", "e", "cost", ( float ) 7 );
			  Graph.makeEdge( "s", "a", "cost", ( long ) 2 );
			  Graph.makeEdge( "a", "b", "cost", 7 );
			  Graph.makeEdge( "b", "e", "cost", ( sbyte ) 2 );
			  Dijkstra<double> dijkstra = GetDijkstra( Graph, 0.0, "s", "e" );
			  assertEquals( 11.0, dijkstra.Cost, 0.0 );
			  assertNotNull( dijkstra.PathAsNodes );
			  assertEquals( 4, dijkstra.PathAsNodes.Count );
			  assertEquals( 1, dijkstra.PathsAsNodes.Count );
		 }
	}

}