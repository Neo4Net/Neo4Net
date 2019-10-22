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
namespace Neo4Net.Graphalgo.path
{
	using Neo4NetAlgoTestCase = Common.Neo4NetAlgoTestCase;
	using Test = org.junit.Test;


	using Neo4Net.Graphalgo;
	using ExactDepthPathFinder = Neo4Net.Graphalgo.impl.path.ExactDepthPathFinder;
	using Direction = Neo4Net.GraphDb.Direction;
	using Node = Neo4Net.GraphDb.Node;
	using Path = Neo4Net.GraphDb.Path;
	using Neo4Net.GraphDb;
	using PathExpanders = Neo4Net.GraphDb.PathExpanders;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.collection.IsIn.isIn;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class TestExactDepthPathFinder : Neo4NetAlgoTestCase
	{
		 public virtual void CreateGraph()
		 {
			  Graph.makeEdgeChain( "SOURCE,SUPER,c,d" );
			  Graph.makeEdgeChain( "SUPER,e,f" );
			  Graph.makeEdgeChain( "SUPER,5,6" );
			  Graph.makeEdgeChain( "SUPER,7,8" );
			  Graph.makeEdgeChain( "SUPER,r,SPIDER" );
			  Graph.makeEdgeChain( "SUPER,g,h,i,j,SPIDER" );
			  Graph.makeEdgeChain( "SUPER,k,l,m,SPIDER" );
			  Graph.makeEdgeChain( "SUPER,s,t,u,SPIDER" );
			  Graph.makeEdgeChain( "SUPER,v,w,x,y,SPIDER" );
			  Graph.makeEdgeChain( "SPIDER,n,o" );
			  Graph.makeEdgeChain( "SPIDER,p,q" );
			  Graph.makeEdgeChain( "SPIDER,1,2" );
			  Graph.makeEdgeChain( "SPIDER,3,4" );
			  Graph.makeEdgeChain( "SPIDER,TARGET" );
			  Graph.makeEdgeChain( "SOURCE,a,b,TARGET" );
			  Graph.makeEdgeChain( "SOURCE,z,9,0,TARGET" );
		 }

		 private PathFinder<Path> NewFinder()
		 {
			  return new ExactDepthPathFinder( PathExpanders.allTypesAndDirections(), 4, 4, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSingle()
		 public virtual void TestSingle()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Set<String> possiblePaths = new java.util.HashSet<>();
			  ISet<string> possiblePaths = new HashSet<string>();
			  possiblePaths.Add( "SOURCE,z,9,0,TARGET" );
			  possiblePaths.Add( "SOURCE,SUPER,r,SPIDER,TARGET" );
			  CreateGraph();
			  PathFinder<Path> finder = NewFinder();
			  Path path = finder.FindSinglePath( Graph.getNode( "SOURCE" ), Graph.getNode( "TARGET" ) );
			  assertNotNull( path );
			  assertThat( GetPathDef( path ), isIn( possiblePaths ) );
			  assertTrue( possiblePaths.Contains( GetPathDef( path ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAll()
		 public virtual void TestAll()
		 {
			  CreateGraph();
			  AssertPaths( NewFinder().findAllPaths(Graph.getNode("SOURCE"), Graph.getNode("TARGET")), "SOURCE,z,9,0,TARGET", "SOURCE,SUPER,r,SPIDER,TARGET" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleDirectionalGraph()
		 public virtual void ShouldHandleDirectionalGraph()
		 {
			  // ALL DIRECTED from (a) towards (g)
			  //     (b) ----------------- (c)      length 3
			  //   /                          \
			  // (a) - (h) - (i) - (j) - (k) - (g)  length 5
			  //   \                          /
			  //     (d) - (e) ------------ (f)     length 4
			  Graph.makeEdgeChain( "a,b,c,g" );
			  Graph.makeEdgeChain( "a,d,e,f,g" );
			  Graph.makeEdgeChain( "a,h,i,j,k,g" );
			  Node a = Graph.getNode( "a" );
			  Node g = Graph.getNode( "g" );
			  AssertPaths( ( new ExactDepthPathFinder( PathExpanders.forDirection( Direction.OUTGOING ), 3, int.MaxValue, false ) ).findAllPaths( a, g ), "a,b,c,g" );
			  AssertPaths( ( new ExactDepthPathFinder( PathExpanders.forDirection( Direction.OUTGOING ), 4, int.MaxValue, false ) ).findAllPaths( a, g ), "a,d,e,f,g" );
			  AssertPaths( ( new ExactDepthPathFinder( PathExpanders.forDirection( Direction.OUTGOING ), 5, int.MaxValue, false ) ).findAllPaths( a, g ), "a,h,i,j,k,g" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleNondirectedGraph()
		 public virtual void ShouldHandleNondirectedGraph()
		 {
			  //     (b) ----------------- (c)      length 3
			  //   /                          \
			  // (a) - (h) - (i) - (j) - (k) - (g)  length 5
			  //   \                          /
			  //     (d) - (e) ------------ (f)     length 4
			  Graph.makeEdgeChain( "a,b,c,g" );
			  Graph.makeEdgeChain( "a,d,e,f,g" );
			  Graph.makeEdgeChain( "a,h,i,j,k,g" );
			  Node a = Graph.getNode( "a" );
			  Node g = Graph.getNode( "g" );
			  AssertPaths( ( new ExactDepthPathFinder( PathExpanders.allTypesAndDirections(), 3, int.MaxValue, false ) ).findAllPaths(a, g), "a,b,c,g" );
			  AssertPaths( ( new ExactDepthPathFinder( PathExpanders.allTypesAndDirections(), 4, int.MaxValue, false ) ).findAllPaths(a, g), "a,d,e,f,g" );
			  AssertPaths( ( new ExactDepthPathFinder( PathExpanders.allTypesAndDirections(), 5, int.MaxValue, false ) ).findAllPaths(a, g), "a,h,i,j,k,g" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleSimpleChainEvenDepth()
		 public virtual void ShouldHandleSimpleChainEvenDepth()
		 {
			  // (a) - (b) - (c)
			  Graph.makeEdgeChain( "a,b,c" );
			  Node a = Graph.getNode( "a" );
			  Node c = Graph.getNode( "c" );
			  AssertPaths( ( new ExactDepthPathFinder( PathExpanders.allTypesAndDirections(), 2, int.MaxValue, false ) ).findAllPaths(a, c), "a,b,c" );
			  AssertPaths( ( new ExactDepthPathFinder( PathExpanders.allTypesAndDirections(), 2, int.MaxValue, false ) ).findAllPaths(a, c), "a,b,c" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleSimpleChainOddDepth()
		 public virtual void ShouldHandleSimpleChainOddDepth()
		 {
			  // (a) - (b) - (c) - (d)
			  Graph.makeEdgeChain( "a,b,c,d" );
			  Node a = Graph.getNode( "a" );
			  Node d = Graph.getNode( "d" );
			  AssertPaths( ( new ExactDepthPathFinder( PathExpanders.allTypesAndDirections(), 3, int.MaxValue, false ) ).findAllPaths(a, d), "a,b,c,d" );
			  AssertPaths( ( new ExactDepthPathFinder( PathExpanders.allTypesAndDirections(), 3, int.MaxValue, false ) ).findAllPaths(a, d), "a,b,c,d" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleNeighbouringNodes()
		 public virtual void ShouldHandleNeighbouringNodes()
		 {
			  // (a) - (b)
			  Graph.makeEdgeChain( "a,b" );
			  Node a = Graph.getNode( "a" );
			  Node b = Graph.getNode( "b" );
			  ExactDepthPathFinder pathFinder = new ExactDepthPathFinder( PathExpanders.allTypesAndDirections(), 1, int.MaxValue, false );
			  IEnumerable<Path> allPaths = pathFinder.FindAllPaths( a, b );
			  AssertPaths( ( new ExactDepthPathFinder( PathExpanders.allTypesAndDirections(), 1, int.MaxValue, false ) ).findAllPaths(a, b), "a,b" );
			  AssertPaths( ( new ExactDepthPathFinder( PathExpanders.allTypesAndDirections(), 1, int.MaxValue, false ) ).findAllPaths(a, b), "a,b" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleNeighbouringNodesWhenNotAlone()
		 public virtual void ShouldHandleNeighbouringNodesWhenNotAlone()
		 {
			  // (a) - (b)
			  //  |
			  // (c)
			  Graph.makeEdge( "a", "b" );
			  Graph.makeEdge( "a", "c" );
			  Node a = Graph.getNode( "a" );
			  Node b = Graph.getNode( "b" );
			  ExactDepthPathFinder pathFinder = new ExactDepthPathFinder( PathExpanders.allTypesAndDirections(), 1, int.MaxValue, false );
			  IEnumerable<Path> allPaths = pathFinder.FindAllPaths( a, b );
			  AssertPaths( ( new ExactDepthPathFinder( PathExpanders.allTypesAndDirections(), 1, int.MaxValue, false ) ).findAllPaths(a, b), "a,b" );
			  AssertPaths( ( new ExactDepthPathFinder( PathExpanders.allTypesAndDirections(), 1, int.MaxValue, false ) ).findAllPaths(a, b), "a,b" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleNeighbouringNodesMultiplePaths()
		 public virtual void ShouldHandleNeighbouringNodesMultiplePaths()
		 {
			  // (a) = (b)
			  //  |
			  // (c)
			  Graph.makeEdgeChain( "a,b" );
			  Graph.makeEdgeChain( "a,b" );
			  Graph.makeEdgeChain( "a,c" );
			  Node a = Graph.getNode( "a" );
			  Node b = Graph.getNode( "b" );
			  ExactDepthPathFinder pathFinder = new ExactDepthPathFinder( PathExpanders.allTypesAndDirections(), 1, int.MaxValue, false );
			  IEnumerable<Path> allPaths = pathFinder.FindAllPaths( a, b );
			  AssertPaths( ( new ExactDepthPathFinder( PathExpanders.allTypesAndDirections(), 1, int.MaxValue, false ) ).findAllPaths(a, b), "a,b", "a,b" );
			  AssertPaths( ( new ExactDepthPathFinder( PathExpanders.allTypesAndDirections(), 1, int.MaxValue, false ) ).findAllPaths(a, b), "a,b", "a,b" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testExactDepthFinder()
		 public virtual void TestExactDepthFinder()
		 {
			  // Layout (a to k):
			  //
			  //     (a)--(c)--(g)--(k)
			  //    /                /
			  //  (b)-----(d)------(j)
			  //   |        \      /
			  //  (e)--(f)--(h)--(i)
			  //
			  Graph.makeEdgeChain( "a,c,g,k" );
			  Graph.makeEdgeChain( "a,b,d,j,k" );
			  Graph.makeEdgeChain( "b,e,f,h,i,j" );
			  Graph.makeEdgeChain( "d,h" );
			  PathExpander<object> expander = PathExpanders.forTypeAndDirection( MyRelTypes.R1, Direction.OUTGOING );
			  Node a = Graph.getNode( "a" );
			  Node k = Graph.getNode( "k" );
			  AssertPaths( GraphAlgoFactory.pathsWithLength( expander, 3 ).findAllPaths( a, k ), "a,c,g,k" );
			  AssertPaths( GraphAlgoFactory.pathsWithLength( expander, 4 ).findAllPaths( a, k ), "a,b,d,j,k" );
			  AssertPaths( GraphAlgoFactory.pathsWithLength( expander, 5 ).findAllPaths( a, k ) );
			  AssertPaths( GraphAlgoFactory.pathsWithLength( expander, 6 ).findAllPaths( a, k ), "a,b,d,h,i,j,k" );
			  AssertPaths( GraphAlgoFactory.pathsWithLength( expander, 7 ).findAllPaths( a, k ), "a,b,e,f,h,i,j,k" );
			  AssertPaths( GraphAlgoFactory.pathsWithLength( expander, 8 ).findAllPaths( a, k ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testExactDepthPathsReturnsNoLoops()
		 public virtual void TestExactDepthPathsReturnsNoLoops()
		 {
			  // Layout:
			  //
			  // (a)-->(b)==>(c)-->(e)
			  //        ^    /
			  //         \  v
			  //         (d)
			  //
			  Graph.makeEdgeChain( "a,b,c,d,b,c,e" );
			  Node a = Graph.getNode( "a" );
			  Node e = Graph.getNode( "e" );
			  AssertPaths( GraphAlgoFactory.pathsWithLength( PathExpanders.forType( MyRelTypes.R1 ), 3 ).findAllPaths( a, e ), "a,b,c,e", "a,b,c,e" );
			  AssertPaths( GraphAlgoFactory.pathsWithLength( PathExpanders.forType( MyRelTypes.R1 ), 4 ).findAllPaths( a, e ), "a,b,d,c,e" );
			  AssertPaths( GraphAlgoFactory.pathsWithLength( PathExpanders.forType( MyRelTypes.R1 ), 6 ).findAllPaths( a, e ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testExactDepthPathsLoopsAllowed()
		 public virtual void TestExactDepthPathsLoopsAllowed()
		 {
			  // Layout:
			  //
			  // (a)-->(b)==>(c)-->(e)
			  //        ^    /
			  //         \  v
			  //         (d)
			  //
			  Graph.makeEdgeChain( "a,b,c,d,b,c,e" );
			  Node a = Graph.getNode( "a" );
			  Node e = Graph.getNode( "e" );
			  AssertPaths( ( new ExactDepthPathFinder( PathExpanders.forDirection( Direction.OUTGOING ), 6, int.MaxValue, true ) ).findAllPaths( a, e ), "a,b,c,d,b,c,e" );
		 }
	}

}