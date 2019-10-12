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
namespace Org.Neo4j.Graphalgo.path
{
	using Neo4jAlgoTestCase = Common.Neo4jAlgoTestCase;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Org.Neo4j.Graphalgo;
	using Org.Neo4j.Graphalgo;
	using Dijkstra = Org.Neo4j.Graphalgo.impl.path.Dijkstra;
	using DijkstraBidirectional = Org.Neo4j.Graphalgo.impl.path.DijkstraBidirectional;
	using Direction = Org.Neo4j.Graphdb.Direction;
	using Node = Org.Neo4j.Graphdb.Node;
	using Path = Org.Neo4j.Graphdb.Path;
	using Org.Neo4j.Graphdb;
	using PathExpanders = Org.Neo4j.Graphdb.PathExpanders;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Org.Neo4j.Graphdb.traversal;
	using NoneStrictMath = Org.Neo4j.Kernel.impl.util.NoneStrictMath;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class DijkstraTest extends common.Neo4jAlgoTestCase
	public class DijkstraTest : Neo4jAlgoTestCase
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pathToSelfReturnsZero()
		 public virtual void PathToSelfReturnsZero()
		 {
			  // GIVEN
			  Node start = Graph.makeNode( "A" );

			  // WHEN
			  PathFinder<WeightedPath> finder = factory.dijkstra( PathExpanders.allTypesAndDirections() );
			  WeightedPath path = finder.FindSinglePath( start, start );
			  // THEN
			  assertNotNull( path );
			  assertEquals( start, path.StartNode() );
			  assertEquals( start, path.EndNode() );
			  assertEquals( 0, path.Length() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allPathsToSelfReturnsZero()
		 public virtual void AllPathsToSelfReturnsZero()
		 {
			  // GIVEN
			  Node start = Graph.makeNode( "A" );

			  // WHEN
			  PathFinder<WeightedPath> finder = factory.dijkstra( PathExpanders.allTypesAndDirections() );
			  IEnumerable<WeightedPath> paths = finder.FindAllPaths( start, start );

			  // THEN
			  foreach ( WeightedPath path in paths )
			  {
					assertNotNull( path );
					assertEquals( start, path.StartNode() );
					assertEquals( start, path.EndNode() );
					assertEquals( 0, path.Length() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canFindOrphanGraph()
		 public virtual void CanFindOrphanGraph()
		 {
			  /*
			   *
			   * (A)=1   (relationship to self)
			   *
			   * Should not find (A)-(A). Should find (A)
			   */

			  Node nodeA = Graph.makeNode( "A" );
			  Graph.makeEdge( "A", "A", "length", 1d );

			  PathFinder finder = factory.dijkstra( PathExpanders.allTypesAndDirections() );
			  AssertPaths( finder.findAllPaths( nodeA, nodeA ), "A" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canFindNeighbour()
		 public virtual void CanFindNeighbour()
		 {
			  /*
			   * (A) - 1 -(B)
			   */
			  Node nodeA = Graph.makeNode( "A" );
			  Node nodeB = Graph.makeNode( "B" );
			  Graph.makeEdge( "A", "B", "length", 1 );

			  PathFinder finder = factory.dijkstra( PathExpanders.allTypesAndDirections() );
			  AssertPaths( finder.findAllPaths( nodeA, nodeB ), "A,B" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canFindNeighbourMultipleCorrectPaths()
		 public virtual void CanFindNeighbourMultipleCorrectPaths()
		 {
			  /*
			   *     - 1.0 -
			   *   /        \
			   * (A) - 1 - (B)
			   */
			  Node nodeA = Graph.makeNode( "A" );
			  Node nodeB = Graph.makeNode( "B" );
			  Graph.makeEdge( "A", "B", "length", 1.0 );
			  Graph.makeEdge( "A", "B", "length", 1 );

			  PathFinder finder = factory.dijkstra( PathExpanders.allTypesAndDirections() );
			  AssertPaths( finder.findAllPaths( nodeA, nodeB ), "A,B","A,B" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canFindNeighbourMultipleIncorrectPaths()
		 public virtual void CanFindNeighbourMultipleIncorrectPaths()
		 {
			  /*
			   *     - 2.0 -
			   *   /        \
			   * (A) - 1 - (B)
			   */
			  Node nodeA = Graph.makeNode( "A" );
			  Node nodeB = Graph.makeNode( "B" );
			  Graph.makeEdge( "A", "B", "length", 2.0 );
			  Graph.makeEdge( "A", "B", "length", 1 );

			  PathFinder finder = factory.dijkstra( PathExpanders.allTypesAndDirections() );
			  IEnumerator<WeightedPath> paths = finder.findAllPaths( nodeA, nodeB ).GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( "Expect at least one path", paths.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  WeightedPath path = paths.next();
			  assertPath( path, nodeA, nodeB );
			  assertEquals( "Expect weight 1", 1, path.Weight(), 0.0 );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( "Expected at most one path", paths.hasNext() );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canKeepSearchingUntilFoundTrueShortest()
		 public virtual void CanKeepSearchingUntilFoundTrueShortest()
		 {
			  /*
			   *
			   *  1 - (B) - 1 - (C) - 1 - (D) - 1 - (E) - 1
			   *  |                                       |
			   * (A) --- 1 --- (G) -- 2 -- (H) --- 1 --- (F)
			   *
			   */

			  Node a = Graph.makeNode( "A" );
			  Node b = Graph.makeNode( "B" );
			  Node c = Graph.makeNode( "C" );
			  Node d = Graph.makeNode( "D" );
			  Node e = Graph.makeNode( "E" );
			  Node f = Graph.makeNode( "F" );
			  Node g = Graph.makeNode( "G" );
			  Node h = Graph.makeNode( "H" );

			  Graph.makeEdgeChain( "A,B,C,D,E,F", "length", 1 );
			  Graph.makeEdge( "A", "G", "length", 1 );
			  Graph.makeEdge( "G", "H", "length", 2 );
			  Graph.makeEdge( "H", "F", "length", 1 );

			  PathFinder finder = factory.dijkstra( PathExpanders.allTypesAndDirections() );
			  IEnumerator<WeightedPath> paths = finder.findAllPaths( a, f ).GetEnumerator();

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( "Expect at least one path", paths.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  WeightedPath path = paths.next();
			  assertPath( path, a,g,h,f );
			  assertEquals( "Expect weight 1", 4, path.Weight(), 0.0 );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( "Expected at most one path", paths.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canGetPathsInTriangleGraph()
		 public virtual void CanGetPathsInTriangleGraph()
		 {
			  /* NODE (NAME/INDEX)
			   *
			   * (A/0) ------- 2 -----> (B/1)
			   *   \                     /
			   *    - 10 -> (C/2) <- 3 -
			   */
			  Node nodeA = Graph.makeNode( "A" );
			  Node nodeB = Graph.makeNode( "B" );
			  Node nodeC = Graph.makeNode( "C" );
			  Graph.makeEdge( "A", "B", "length", 2d );
			  Graph.makeEdge( "B", "C", "length", 3L );
			  Graph.makeEdge( "A", "C", "length", ( sbyte )10 );

			  PathFinder finder = factory.dijkstra( PathExpanders.allTypesAndDirections() );
			  IEnumerator<WeightedPath> paths = finder.findAllPaths( nodeA, nodeC ).GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( "expected at least one path", paths.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertPath( paths.next(), nodeA, nodeB, nodeC );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( "expected at most one path", paths.hasNext() );

			  AssertPath( finder.findSinglePath( nodeA, nodeC ), nodeA, nodeB, nodeC );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canContinueGettingPathsByDiminishingCost()
		 public virtual void CanContinueGettingPathsByDiminishingCost()
		 {
			  /*
			   * NODE (NAME/INDEX)
			   *
			   * (A)-*2->(B)-*3->(C)-*1->(D)
			   *  |        \             ^ ^
			   *  |          ----*5-----/  |
			   *   \                       |
			   *     ---------*6-----------
			   */

			  Node nodeA = Graph.makeNode( "A" );
								Graph.makeNode( "B" );
								Graph.makeNode( "C" );
			  Node nodeD = Graph.makeNode( "D" );

			  // Path "1"
			  Graph.makeEdge( "A", "B", "length", 2d );
			  Graph.makeEdge( "B", "C", "length", 3L );
			  Graph.makeEdge( "C", "D", "length", ( sbyte )1 ); // = 6

			  // Path "2"
			  Graph.makeEdge( "B", "D", "length", ( short )5 ); // = 7

			  // Path "3"
			  Graph.makeEdge( "A", "D", "length", ( float )6 ); // = 6

			  PathFinder finder = factory.dijkstra( PathExpanders.allTypesAndDirections() );
			  AssertPaths( finder.findAllPaths( nodeA, nodeD ), "A,B,C,D", "A,D" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canGetMultiplePathsInTriangleGraph()
		 public virtual void CanGetMultiplePathsInTriangleGraph()
		 {
			  /* NODE (NAME/INDEX)
			   * ==> (two relationships)
			   *
			   * (A/0) ====== 1 =====> (B/1)
			   *   \                    /
			   *    - 5 -> (C/2) <- 2 -
			   */
			  Node nodeA = Graph.makeNode( "A" );
			  Node nodeB = Graph.makeNode( "B" );
			  Node nodeC = Graph.makeNode( "C" );
			  ISet<Relationship> expectedFirsts = new HashSet<Relationship>();
			  expectedFirsts.Add( Graph.makeEdge( "A", "B", "length", 1d ) );
			  expectedFirsts.Add( Graph.makeEdge( "A", "B", "length", 1 ) );
			  Relationship expectedSecond = Graph.makeEdge( "B", "C", "length", 2L );
			  Graph.makeEdge( "A", "C", "length", 5d );

			  PathFinder finder = factory.dijkstra( PathExpanders.allTypesAndDirections() );
			  IEnumerator<WeightedPath> paths = finder.findAllPaths( nodeA, nodeC ).GetEnumerator();
			  for ( int i = 0; i < 2; i++ )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( "expected more paths", paths.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					Path path = paths.next();
					AssertPath( path, nodeA, nodeB, nodeC );

					IEnumerator<Relationship> relationships = path.Relationships().GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( "found shorter path than expected", relationships.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( "path contained unexpected relationship", expectedFirsts.remove( relationships.next() ) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( "found shorter path than expected", relationships.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( expectedSecond, relationships.next() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( "found longer path than expected", relationships.hasNext() );
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( "expected at most two paths", paths.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canGetMultiplePathsInASmallRoadNetwork()
		 public virtual void CanGetMultiplePathsInASmallRoadNetwork()
		 {
			  /*    NODE (NAME/INDEX)
			   *
			   *     --------------------- 25 -----------------------
			   *    /                                                 \
			   *  (A/0) - 2 - (B/1) - 2.5 - (D/3) - 3 - (E/4) - 5 - (F/5)
			   *    |                        /           /           /
			   *   2.5  ---------- 7.3 -----            /           /
			   *    |  /                               /           /
			   *  (C/2) ------------------ 5 ---------            /
			   *    \                                            /
			   *      ------------------ 12 --------------------
			   *
			   */
			  Node nodeA = Graph.makeNode( "A" );
			  Node nodeB = Graph.makeNode( "B" );
			  Node nodeC = Graph.makeNode( "C" );
			  Node nodeD = Graph.makeNode( "D" );
			  Node nodeE = Graph.makeNode( "E" );
			  Node nodeF = Graph.makeNode( "F" );
			  Graph.makeEdge( "A", "B", "length", 2d );
			  Graph.makeEdge( "A", "C", "length", 2.5f );
			  Graph.makeEdge( "C", "D", "length", 7.3d );
			  Graph.makeEdge( "B", "D", "length", 2.5f );
			  Graph.makeEdge( "D", "E", "length", 3L );
			  Graph.makeEdge( "C", "E", "length", 5 );
			  Graph.makeEdge( "E", "F", "length", ( sbyte )5 );
			  Graph.makeEdge( "C", "F", "length", ( short )12 );
			  Graph.makeEdge( "A", "F", "length", ( long )25 );

			  PathFinder finder = factory.dijkstra( PathExpanders.allTypesAndDirections() );
			  // Try the search in both directions.
			  foreach (Node[] nodes in new Node[][]
			  {
				  new Node[] { nodeA, nodeF },
				  new Node[] { nodeF, nodeA }
			  })
			  {
					int found = 0;
					IEnumerator<WeightedPath> paths = finder.findAllPaths( nodes[0], nodes[1] ).GetEnumerator();
					for ( int i = 0; i < 2; i++ )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 assertTrue( "expected more paths", paths.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 Path path = paths.next();
						 if ( path.Length() != found && path.Length() == 3 )
						 {
							  AssertContains( path.Nodes(), nodeA, nodeC, nodeE, nodeF );
						 }
						 else if ( path.Length() != found && path.Length() == 4 )
						 {
							  AssertContains( path.Nodes(), nodeA, nodeB, nodeD, nodeE, nodeF );
						 }
						 else
						 {
							  fail( "unexpected path length: " + path.Length() );
						 }
						 found = path.Length();
					}
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( "expected at most two paths", paths.hasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyFindTheShortestPaths()
		 public virtual void ShouldOnlyFindTheShortestPaths()
		 {
			  /*
			   *
			   *      ----- (e) - 1 - (f) ---
			   *    /                         \
			   *   /    ------- (a) --------   \
			   *  1   /            \         \  2
			   *  |  2              0         0 |
			   *  | /                \         \|
			   * (s) - 1 - (c) - 1 - (d) - 1 - (t)
			   *   \                 /
			   *    -- 1 - (b) - 1 -
			   *
			   */

			  Node s = Graph.makeNode( "s" );
			  Node t = Graph.makeNode( "t" );
			  Node a = Graph.makeNode( "a" );
			  Node b = Graph.makeNode( "b" );
			  Node c = Graph.makeNode( "c" );

			  Graph.makeEdgeChain( "s,e,f", "length", 1.0 );
			  Graph.makeEdge( "f", "t", "length", 2 );
			  Graph.makeEdge( "s","a", "length", 2 );
			  Graph.makeEdge( "a","t", "length", 0 );
			  Graph.makeEdge( "s", "c", "length", 1 );
			  Graph.makeEdge( "c","d", "length", 1 );
			  Graph.makeEdge( "s","b", "length", 1 );
			  Graph.makeEdge( "b","d", "length", 1 );
			  Graph.makeEdge( "d","a", "length", 0 );
			  Graph.makeEdge( "d","t", "length", 1 );

			  PathFinder finder = factory.dijkstra( PathExpanders.allTypesAndDirections() );
			  IEnumerator<WeightedPath> paths = finder.findAllPaths( s, t ).GetEnumerator();

			  for ( int i = 1; i <= 3; i++ )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( "Expected at least " + i + " path(s)", paths.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( "Expected 3 paths of cost 2", NoneStrictMath.Equals( paths.next().weight(), 2 ) );
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( "Expected exactly 3 paths", paths.hasNext() );
		 }

		 private Relationship CreateGraph( bool includeOnes )
		 {
			  /* Layout:
			   *                       (y)
			   *                        ^
			   *                        [2]  _____[1]___
			   *                          \ v           |
			   * (start)--[1]->(a)--[9]-->(x)<-        (e)--[2]->(f)
			   *                |         ^ ^^  \       ^
			   *               [1]  ---[7][5][3] -[3]  [1]
			   *                v  /       | /      \  /
			   *               (b)--[1]-->(c)--[1]->(d)
			   */

			  IDictionary<string, object> propertiesForOnes = includeOnes ? map( "cost", ( double ) 1 ) : map();

			  Graph.makeEdge( "start", "a", "cost", ( double ) 1 );
			  Graph.makeEdge( "a", "x", "cost", ( short ) 9 );
			  Graph.makeEdge( "a", "b", propertiesForOnes );
			  Graph.makeEdge( "b", "x", "cost", ( double ) 7 );
			  Graph.makeEdge( "b", "c", propertiesForOnes );
			  Graph.makeEdge( "c", "x", "cost", 5 );
			  Relationship shortCTOXRelationship = Graph.makeEdge( "c", "x", "cost", ( float ) 3 );
			  Graph.makeEdge( "c", "d", propertiesForOnes );
			  Graph.makeEdge( "d", "x", "cost", ( double ) 3 );
			  Graph.makeEdge( "d", "e", propertiesForOnes );
			  Graph.makeEdge( "e", "x", propertiesForOnes );
			  Graph.makeEdge( "e", "f", "cost", ( sbyte ) 2 );
			  Graph.makeEdge( "x", "y", "cost", ( double ) 2 );
			  return shortCTOXRelationship;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSmallGraph()
		 public virtual void TestSmallGraph()
		 {
			  Relationship shortCTOXRelationship = CreateGraph( true );

			  PathFinder<WeightedPath> finder = factory.dijkstra( PathExpanders.forTypeAndDirection( MyRelTypes.R1, Direction.OUTGOING ), CommonEvaluators.doubleCostEvaluator( "cost" ) );

			  // Assert that there are two matching paths
			  Node startNode = Graph.getNode( "start" );
			  Node endNode = Graph.getNode( "x" );
			  AssertPaths( finder.FindAllPaths( startNode, endNode ), "start,a,b,c,x", "start,a,b,c,d,e,x" );

			  // Assert that for the shorter one it picked the correct relationship
			  // of the two from (c) --> (x)
			  foreach ( WeightedPath path in finder.FindAllPaths( startNode, endNode ) )
			  {
					if ( GetPathDef( path ).Equals( "start,a,b,c,x" ) )
					{
						 AssertContainsRelationship( path, shortCTOXRelationship );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSmallGraphWithDefaults()
		 public virtual void TestSmallGraphWithDefaults()
		 {
			  Relationship shortCTOXRelationship = CreateGraph( true );

			  PathFinder<WeightedPath> finder = factory.dijkstra( PathExpanders.forTypeAndDirection( MyRelTypes.R1, Direction.OUTGOING ), CommonEvaluators.doubleCostEvaluator( "cost", 1.0d ) );

			  // Assert that there are two matching paths
			  Node startNode = Graph.getNode( "start" );
			  Node endNode = Graph.getNode( "x" );
			  AssertPaths( finder.FindAllPaths( startNode, endNode ), "start,a,b,c,x", "start,a,b,c,d,e,x" );

			  // Assert that for the shorter one it picked the correct relationship
			  // of the two from (c) --> (x)
			  foreach ( WeightedPath path in finder.FindAllPaths( startNode, endNode ) )
			  {
					if ( GetPathDef( path ).Equals( "start,a,b,c,x" ) )
					{
						 AssertContainsRelationship( path, shortCTOXRelationship );
					}
			  }
		 }

		 private void AssertContainsRelationship( WeightedPath path, Relationship relationship )
		 {
			  foreach ( Relationship rel in path.Relationships() )
			  {
					if ( rel.Equals( relationship ) )
					{
						 return;
					}
			  }
			  fail( path + " should've contained " + relationship );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters public static java.util.Collection<Object[]> data()
		 public static ICollection<object[]> Data()
		 {
			  return Arrays.asList(new object[][] { { new DijkstraFactoryAnonymousInnerClass()},{ // DijkstraBidirectionalnew DijkstraFactory(){@@Override public PathFinder dijkstra(PathExpander expander){return new DijkstraBidirectional(expander, CommonEvaluators.DoubleCostEvaluator("length"));
													}

													private class DijkstraFactoryAnonymousInnerClass : DijkstraFactory
													{
														public PathFinder dijkstra( PathExpander expander )
														{
															 return new Dijkstra( expander, CommonEvaluators.doubleCostEvaluator( "length" ) );
														}

														public PathFinder dijkstra( PathExpander expander, CostEvaluator costEvaluator )
														{
															 return new Dijkstra( expander, costEvaluator );
														}
													}

													public override PathFinder dijkstra( PathExpander expander, CostEvaluator costEvaluator )
													{
														 return new DijkstraBidirectional( expander, costEvaluator );
													}
		 }
	}
								  ,
								  {
											  // Dijkstra (mono directional) with state.
											  new DijkstraFactoryAnonymousInnerClass( this )
								  }
}
						);
		 }

		 private interface DijkstraFactory
		 {
			  PathFinder<WeightedPath> dijkstra( PathExpander expander );
			  PathFinder<WeightedPath> dijkstra( PathExpander expander, CostEvaluator costEvaluator );
		 }

		 private final DijkstraFactory _factory;

		 public DijkstraTest( DijkstraFactory _factory )
		 {
			  this._factory = _factory;
		 }
	}

}