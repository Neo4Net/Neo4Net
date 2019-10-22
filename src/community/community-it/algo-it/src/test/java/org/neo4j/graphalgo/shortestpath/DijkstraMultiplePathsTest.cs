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
namespace Neo4Net.Graphalgo.shortestpath
{
	using Neo4NetAlgoTestCase = Common.Neo4NetAlgoTestCase;
	using SimpleGraphBuilder = Common.SimpleGraphBuilder;
	using Test = org.junit.Test;

	using Neo4Net.Graphalgo.impl.shortestpath;
	using DoubleAdder = Neo4Net.Graphalgo.impl.util.DoubleAdder;
	using Direction = Neo4Net.GraphDb.Direction;
	using Node = Neo4Net.GraphDb.Node;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Relationship = Neo4Net.GraphDb.Relationship;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class DijkstraMultiplePathsTest : Neo4NetAlgoTestCase
	{
		 protected internal virtual Dijkstra<double> GetDijkstra( SimpleGraphBuilder graph, double? startCost, string startNode, string endNode )
		 {
			  return new Dijkstra<double>( startCost, graph.GetNode( startNode ), graph.GetNode( endNode ), CommonEvaluators.doubleCostEvaluator( "cost" ), new DoubleAdder(), double?.compareTo, Direction.BOTH, MyRelTypes.R1 );
		 }

		 /// <summary>
		 /// A triangle with 0 cost should generate two paths between every pair of
		 /// nodes.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testTriangle()
		 public virtual void TestTriangle()
		 {
			  Graph.makeEdge( "a", "b", "cost", ( double ) 0 );
			  Graph.makeEdge( "b", "c", "cost", ( double ) 0 );
			  Graph.makeEdge( "c", "a", "cost", ( double ) 0 );
			  Dijkstra<double> dijkstra;
			  string[] nodes = new string[] { "a", "b", "c" };
			  foreach ( string node1 in nodes )
			  {
					foreach ( string node2 in nodes )
					{
						 dijkstra = GetDijkstra( Graph, 0.0, node1, node2 );
						 int nrPaths = dijkstra.PathsAsNodes.Count;
						 if ( !node1.Equals( node2 ) )
						 {
							  assertEquals( "Number of paths (" + node1 + "->" + node2 + "): " + nrPaths, 2, nrPaths );
						 }
						 assertEquals( 0.0, dijkstra.Cost, 0.0 );
					}
			  }
		 }

		 /// <summary>
		 /// From each direction 2 ways are possible so 4 ways should be the total.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void test1()
		 public virtual void Test1()
		 {
			  Graph.makeEdge( "a", "b", "cost", ( double ) 1 );
			  Graph.makeEdge( "b", "d", "cost", ( float ) 1 );
			  Graph.makeEdge( "a", "c", "cost", 1 );
			  Graph.makeEdge( "c", "d", "cost", ( long ) 1 );
			  Graph.makeEdge( "d", "e", "cost", ( short ) 1 );
			  Graph.makeEdge( "e", "f", "cost", ( sbyte ) 1 );
			  Graph.makeEdge( "f", "h", "cost", ( float ) 1 );
			  Graph.makeEdge( "e", "g", "cost", ( double ) 1 );
			  Graph.makeEdge( "g", "h", "cost", ( double ) 1 );
			  Dijkstra<double> dijkstra = GetDijkstra( Graph, 0.0, "a", "h" );
			  assertEquals( 4, dijkstra.Paths.Count );
			  assertEquals( 4, dijkstra.PathsAsNodes.Count );
			  assertEquals( 4, dijkstra.PathsAsRelationships.Count );
			  assertEquals( 5.0, dijkstra.Cost, 0.0 );
		 }

		 /// <summary>
		 /// Two different ways. This is supposed to test when the traversers meet in
		 /// several places.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void test2()
		 public virtual void Test2()
		 {
			  Graph.makeEdge( "a", "b", "cost", ( double ) 1 );
			  Graph.makeEdge( "a", "f", "cost", ( float ) 1 );
			  Graph.makeEdge( "b", "c", "cost", ( long ) 1 );
			  Graph.makeEdge( "f", "g", "cost", 1 );
			  Graph.makeEdge( "c", "d", "cost", ( short ) 1 );
			  Graph.makeEdge( "g", "h", "cost", ( sbyte ) 1 );
			  Graph.makeEdge( "d", "e", "cost", ( float ) 1 );
			  Graph.makeEdge( "h", "e", "cost", ( double ) 1 );
			  Dijkstra<double> dijkstra = GetDijkstra( Graph, 0.0, "a", "e" );
			  assertEquals( 2, dijkstra.Paths.Count );
			  assertEquals( 2, dijkstra.PathsAsNodes.Count );
			  assertEquals( 2, dijkstra.PathsAsRelationships.Count );
			  assertEquals( 4.0, dijkstra.Cost, 0.0 );
		 }

		 /// <summary>
		 /// One side finding several paths to one node previously visited by the
		 /// other side. The other side is kept busy with a chain of cost zero.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void test3()
		 public virtual void Test3()
		 {
			  // "zero" side
			  Graph.makeEdge( "a", "b", "cost", ( double ) 0 );
			  Graph.makeEdge( "b", "c", "cost", ( float ) 0 );
			  Graph.makeEdge( "c", "d", "cost", ( long ) 0 );
			  Graph.makeEdge( "d", "e", "cost", 0 );
			  Graph.makeEdge( "e", "f", "cost", ( sbyte ) 0 );
			  Graph.makeEdge( "f", "g", "cost", ( float ) 0 );
			  Graph.makeEdge( "g", "h", "cost", ( short ) 0 );
			  Graph.makeEdge( "h", "i", "cost", ( double ) 0 );
			  Graph.makeEdge( "i", "j", "cost", ( double ) 0 );
			  Graph.makeEdge( "j", "k", "cost", ( double ) 0 );
			  // "discovering" side
			  Graph.makeEdge( "z", "y", "cost", ( double ) 0 );
			  Graph.makeEdge( "y", "x", "cost", ( double ) 0 );
			  Graph.makeEdge( "x", "w", "cost", ( double ) 0 );
			  Graph.makeEdge( "w", "b", "cost", ( double ) 1 );
			  Graph.makeEdge( "x", "b", "cost", ( float ) 2 );
			  Graph.makeEdge( "y", "b", "cost", ( long ) 1 );
			  Graph.makeEdge( "z", "b", "cost", 1 );
			  Graph.makeEdge( "zz", "z", "cost", ( double ) 0 );
			  Dijkstra<double> dijkstra = GetDijkstra( Graph, 0.0, "a", "zz" );
			  assertEquals( 3, dijkstra.PathsAsNodes.Count );
			  assertEquals( 1.0, dijkstra.Cost, 0.0 );
		 }

		 /// <summary>
		 /// another variant of the test above, but the discovering is a bit mixed.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void test4()
		 public virtual void Test4()
		 {
			  // "zero" side
			  Graph.makeEdge( "a", "b", "cost", ( double ) 0 );
			  Graph.makeEdge( "b", "c", "cost", ( double ) 0 );
			  Graph.makeEdge( "c", "d", "cost", ( double ) 0 );
			  Graph.makeEdge( "d", "e", "cost", ( double ) 0 );
			  Graph.makeEdge( "e", "f", "cost", ( double ) 0 );
			  Graph.makeEdge( "f", "g", "cost", ( double ) 0 );
			  Graph.makeEdge( "g", "h", "cost", ( double ) 0 );
			  Graph.makeEdge( "h", "i", "cost", ( double ) 0 );
			  Graph.makeEdge( "i", "j", "cost", ( double ) 0 );
			  Graph.makeEdge( "j", "k", "cost", ( double ) 0 );
			  // "discovering" side
			  Graph.makeEdge( "z", "y", "cost", ( double ) 0 );
			  Graph.makeEdge( "y", "x", "cost", ( double ) 0 );
			  Graph.makeEdge( "x", "w", "cost", ( double ) 0 );
			  Graph.makeEdge( "w", "b", "cost", ( double ) 1 );
			  Graph.makeEdge( "x", "b", "cost", ( float ) 2 );
			  Graph.makeEdge( "y", "b", "cost", ( long ) 1 );
			  Graph.makeEdge( "z", "b", "cost", 1 );
			  Dijkstra<double> dijkstra = GetDijkstra( Graph, 0.0, "a", "z" );
			  assertEquals( 3, dijkstra.PathsAsNodes.Count );
			  assertEquals( 1.0, dijkstra.Cost, 0.0 );
		 }

		 /// <summary>
		 /// "Diamond" shape, with some weights to resemble the test case above.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void test5()
		 public virtual void Test5()
		 {
			  Graph.makeEdge( "a", "b", "cost", ( double ) 0 );
			  Graph.makeEdge( "z", "y", "cost", ( float ) 0 );
			  Graph.makeEdge( "y", "b", "cost", ( long ) 1 );
			  Graph.makeEdge( "z", "b", "cost", 1 );
			  Graph.makeEdge( "y", "a", "cost", ( sbyte ) 1 );
			  Dijkstra<double> dijkstra = GetDijkstra( Graph, 0.0, "a", "z" );
			  IList<IList<Node>> paths = dijkstra.PathsAsNodes;
			  assertEquals( 3, paths.Count );
			  assertEquals( 1.0, dijkstra.Cost, 0.0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void test6()
		 public virtual void Test6()
		 {
			  Graph.makeEdgeChain( "a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,z", "cost", ( double ) 1 );
			  Graph.makeEdge( "a", "b2", "cost", ( double ) 4 );
			  Graph.makeEdge( "b2", "c", "cost", -2 );
			  Dijkstra<double> dijkstra = new Dijkstra<double>( 0.0, Graph.getNode( "a" ), Graph.getNode( "z" ), CommonEvaluators.doubleCostEvaluator( "cost" ), new DoubleAdder(), double?.compareTo, Direction.OUTGOING, MyRelTypes.R1 );
			  IList<IList<Node>> paths = dijkstra.PathsAsNodes;
			  assertEquals( 2, paths.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void test7()
		 public virtual void Test7()
		 {
			  Relationship edgeAB = Graph.makeEdge( "a", "b" );
			  Relationship edgeBC = Graph.makeEdge( "b", "c" );
			  Relationship edgeCD = Graph.makeEdge( "c", "d" );
			  Relationship edgeDE = Graph.makeEdge( "d", "e" );
			  Relationship edgeAB2 = Graph.makeEdge( "a", "b2" );
			  Relationship edgeB2C = Graph.makeEdge( "b2", "c" );
			  Relationship edgeCD2 = Graph.makeEdge( "c", "d2" );
			  Relationship edgeD2E = Graph.makeEdge( "d2", "e" );
			  Dijkstra<double> dijkstra = new Dijkstra<double>( 0.0, Graph.getNode( "a" ), Graph.getNode( "e" ), ( relationship, direction ) => 1.0, new DoubleAdder(), double?.compareTo, Direction.OUTGOING, MyRelTypes.R1 );
			  // path discovery flags
			  bool pathBD = false;
			  bool pathB2D = false;
			  bool pathBD2 = false;
			  bool pathB2D2 = false;
			  IList<IList<PropertyContainer>> paths = dijkstra.Paths;
			  assertEquals( 4, paths.Count );
			  foreach ( IList<PropertyContainer> path in paths )
			  {
					assertEquals( 9, path.Count );
					assertEquals( path[0], Graph.getNode( "a" ) );
					assertEquals( path[4], Graph.getNode( "c" ) );
					assertEquals( path[8], Graph.getNode( "e" ) );
					// first choice
					if ( path[2].Equals( Graph.getNode( "b" ) ) )
					{
						 assertEquals( path[1], edgeAB );
						 assertEquals( path[3], edgeBC );
					}
					else
					{
						 assertEquals( path[1], edgeAB2 );
						 assertEquals( path[2], Graph.getNode( "b2" ) );
						 assertEquals( path[3], edgeB2C );
					}
					// second choice
					if ( path[6].Equals( Graph.getNode( "d" ) ) )
					{
						 assertEquals( path[5], edgeCD );
						 assertEquals( path[7], edgeDE );
					}
					else
					{
						 assertEquals( path[5], edgeCD2 );
						 assertEquals( path[6], Graph.getNode( "d2" ) );
						 assertEquals( path[7], edgeD2E );
					}
					// combinations
					if ( path[2].Equals( Graph.getNode( "b" ) ) )
					{
						 if ( path[6].Equals( Graph.getNode( "d" ) ) )
						 {
							  pathBD = true;
						 }
						 else if ( path[6].Equals( Graph.getNode( "d2" ) ) )
						 {
							  pathBD2 = true;
						 }
					}
					else
					{
						 if ( path[6].Equals( Graph.getNode( "d" ) ) )
						 {
							  pathB2D = true;
						 }
						 else if ( path[6].Equals( Graph.getNode( "d2" ) ) )
						 {
							  pathB2D2 = true;
						 }
					}
			  }
			  assertTrue( pathBD );
			  assertTrue( pathB2D );
			  assertTrue( pathBD2 );
			  assertTrue( pathB2D2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void test8()
		 public virtual void Test8()
		 {
			  Relationship edgeAB = Graph.makeEdge( "a", "b" );
			  Relationship edgeBC = Graph.makeEdge( "b", "c" );
			  Relationship edgeCD = Graph.makeEdge( "c", "d" );
			  Relationship edgeDE = Graph.makeEdge( "d", "e" );
			  Relationship edgeAB2 = Graph.makeEdge( "a", "b2" );
			  Relationship edgeB2C = Graph.makeEdge( "b2", "c" );
			  Relationship edgeCD2 = Graph.makeEdge( "c", "d2" );
			  Relationship edgeD2E = Graph.makeEdge( "d2", "e" );
			  Dijkstra<double> dijkstra = new Dijkstra<double>( 0.0, Graph.getNode( "a" ), Graph.getNode( "e" ), ( relationship, direction ) => 1.0, new DoubleAdder(), double?.compareTo, Direction.OUTGOING, MyRelTypes.R1 );
			  // path discovery flags
			  bool pathBD = false;
			  bool pathB2D = false;
			  bool pathBD2 = false;
			  bool pathB2D2 = false;
			  IList<IList<Relationship>> paths = dijkstra.PathsAsRelationships;
			  assertEquals( 4, paths.Count );
			  foreach ( IList<Relationship> path in paths )
			  {
					assertEquals( 4, path.Count );
					// first choice
					if ( path[0].Equals( edgeAB ) )
					{
						 assertEquals( path[1], edgeBC );
					}
					else
					{
						 assertEquals( path[0], edgeAB2 );
						 assertEquals( path[1], edgeB2C );
					}
					// second choice
					if ( path[2].Equals( edgeCD ) )
					{
						 assertEquals( path[3], edgeDE );
					}
					else
					{
						 assertEquals( path[2], edgeCD2 );
						 assertEquals( path[3], edgeD2E );
					}
					// combinations
					if ( path[0].Equals( edgeAB ) )
					{
						 if ( path[2].Equals( edgeCD ) )
						 {
							  pathBD = true;
						 }
						 else if ( path[2].Equals( edgeCD2 ) )
						 {
							  pathBD2 = true;
						 }
					}
					else
					{
						 if ( path[2].Equals( edgeCD ) )
						 {
							  pathB2D = true;
						 }
						 else if ( path[2].Equals( edgeCD2 ) )
						 {
							  pathB2D2 = true;
						 }
					}
			  }
			  assertTrue( pathBD );
			  assertTrue( pathB2D );
			  assertTrue( pathBD2 );
			  assertTrue( pathB2D2 );
		 }

		 /// <summary>
		 /// Should generate three paths. The three paths must have the prefix: a, b, and c. The three paths must have the sufix: f and g.
		 /// All the edges have cost 0.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void test9()
		 public virtual void Test9()
		 {
			  Graph.makeEdge( "a", "b", "cost", ( double ) 0 );
			  Graph.makeEdge( "b", "c", "cost", ( double ) 0 );
			  Graph.makeEdge( "c", "d", "cost", ( double ) 0 );
			  Graph.makeEdge( "d", "e", "cost", ( double ) 0 );
			  Graph.makeEdge( "e", "f", "cost", ( double ) 0 );
			  Graph.makeEdge( "f", "g", "cost", ( double ) 0 );

			  Graph.makeEdge( "d", "j", "cost", ( double ) 0 );
			  Graph.makeEdge( "j", "k", "cost", ( double ) 0 );
			  Graph.makeEdge( "k", "f", "cost", ( double ) 0 );

			  Graph.makeEdge( "c", "h", "cost", ( double ) 0 );
			  Graph.makeEdge( "h", "i", "cost", ( double ) 0 );
			  Graph.makeEdge( "i", "e", "cost", ( double ) 0 );

			  Dijkstra<double> dijkstra = new Dijkstra<double>( 0.0, Graph.getNode( "a" ), Graph.getNode( "g" ), ( relationship, direction ) => .0, new DoubleAdder(), double?.compareTo, Direction.OUTGOING, MyRelTypes.R1 );

			  IList<IList<Node>> paths = dijkstra.PathsAsNodes;

			  assertEquals( paths.Count, 3 );
			  string[] commonPrefix = new string[] { "a", "b", "c" };
			  string[] commonSuffix = new string[] { "f", "g" };
			  foreach ( IList<Node> path in paths )
			  {
					/// <summary>
					/// Check if the prefixes are all correct.
					/// </summary>
					for ( int j = 0; j < commonPrefix.Length; j++ )
					{
						 assertEquals( path[j], Graph.getNode( commonPrefix[j] ) );
					}

					int pathSize = path.Count;

					/// <summary>
					/// Check if the suffixes are all correct.
					/// </summary>
					for ( int j = 0; j < commonSuffix.Length; j++ )
					{
						 assertEquals( path[pathSize - j - 1], Graph.getNode( commonSuffix[commonSuffix.Length - j - 1] ) );
					}
			  }
		 }
	}

}