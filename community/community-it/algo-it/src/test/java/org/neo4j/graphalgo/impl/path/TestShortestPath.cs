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
namespace Org.Neo4j.Graphalgo.impl.path
{
	using Neo4jAlgoTestCase = Common.Neo4jAlgoTestCase;
	using MutableInt = org.apache.commons.lang3.mutable.MutableInt;
	using Test = org.junit.Test;


	using Org.Neo4j.Graphalgo;
	using Direction = Org.Neo4j.Graphdb.Direction;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Path = Org.Neo4j.Graphdb.Path;
	using Org.Neo4j.Graphdb;
	using PathExpanderBuilder = Org.Neo4j.Graphdb.PathExpanderBuilder;
	using PathExpanders = Org.Neo4j.Graphdb.PathExpanders;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Org.Neo4j.Graphdb;
	using StandardExpander = Org.Neo4j.Graphdb.impl.StandardExpander;
	using Org.Neo4j.Graphdb.traversal;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static common.Neo4jAlgoTestCase.MyRelTypes.R1;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphalgo.GraphAlgoFactory.shortestPath;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Direction.BOTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Direction.INCOMING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Direction.OUTGOING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.PathExpanders.allTypesAndDirections;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.count;

	public class TestShortestPath : Neo4jAlgoTestCase
	{
		 // Attempt at recreating this issue without cypher
		 // https://github.com/neo4j/neo4j/issues/4160
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAbortAsSoonAsPossible()
		 public virtual void ShouldAbortAsSoonAsPossible()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Label A = org.neo4j.graphdb.Label.label("A");
			  Label a = Label.label( "A" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Label B = org.neo4j.graphdb.Label.label("B");
			  Label b = Label.label( "B" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Label C = org.neo4j.graphdb.Label.label("C");
			  Label c = Label.label( "C" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Label D = org.neo4j.graphdb.Label.label("D");
			  Label d = Label.label( "D" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Label E = org.neo4j.graphdb.Label.label("E");
			  Label e = Label.label( "E" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Label F = org.neo4j.graphdb.Label.label("F");
			  Label f = Label.label( "F" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.RelationshipType relType = org.neo4j.graphdb.RelationshipType.withName("TO");
			  RelationshipType relType = RelationshipType.withName( "TO" );
			  RecursiveSnowFlake( null, 0, 4, 5, new Label[]{ a, b, c, d, e }, relType );
			  Node a = GetNodeByLabel( a );
			  using ( ResourceIterator<Node> allE = GraphDb.findNodes( e ) )
			  {
					while ( allE.MoveNext() )
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node e = allE.Current;
						 Node e = allE.Current;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node f = graphDb.createNode(F);
						 Node f = GraphDb.createNode( f );
						 f.CreateRelationshipTo( e, relType );
					}
			  }
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final CountingPathExpander countingPathExpander = new CountingPathExpander(org.neo4j.graphdb.PathExpanders.forTypeAndDirection(relType, org.neo4j.graphdb.Direction.OUTGOING));
			  CountingPathExpander countingPathExpander = new CountingPathExpander( this, PathExpanders.forTypeAndDirection( relType, Direction.OUTGOING ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ShortestPath shortestPath = new ShortestPath(Integer.MAX_VALUE, countingPathExpander, Integer.MAX_VALUE);
			  ShortestPath shortestPath = new ShortestPath( int.MaxValue, countingPathExpander, int.MaxValue );
			  using ( ResourceIterator<Node> allF = GraphDb.findNodes( f ) )
			  {
					while ( allF.MoveNext() )
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node f = allF.Current;
						 Node f = allF.Current;
						 shortestPath.FindAllPaths( a, f );
					}
			  }
			  assertEquals( "There are 625 different end nodes. The algorithm should start one traversal for each such node. " + "That is 625*2 visited nodes if traversal is interrupted correctly.", 1250, countingPathExpander.NodesVisited.intValue() );
		 }

		 private Node GetNodeByLabel( Label label )
		 {
			  using ( ResourceIterator<Node> iterator = GraphDb.findNodes( label ) )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					return iterator.next();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void recursiveSnowFlake(org.neo4j.graphdb.Node parent, int level, final int desiredLevel, final int branchingFactor, final org.neo4j.graphdb.Label[] labels, final org.neo4j.graphdb.RelationshipType relType)
		 private void RecursiveSnowFlake( Node parent, int level, int desiredLevel, int branchingFactor, Label[] labels, RelationshipType relType )
		 {
			  if ( level != 0 )
			  {
					for ( int n = 0; n < branchingFactor; n++ )
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node node = graphDb.createNode(labels[level]);
						 Node node = GraphDb.createNode( labels[level] );
						 if ( parent != null )
						 {
							  parent.CreateRelationshipTo( node, relType );
						 }
						 if ( level < desiredLevel )
						 {
							  RecursiveSnowFlake( node, level + 1, desiredLevel, branchingFactor, labels, relType );
						 }
					}
			  }
			  else
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node node = graphDb.createNode(labels[level]);
					Node node = GraphDb.createNode( labels[level] );
					RecursiveSnowFlake( node, level + 1, desiredLevel, branchingFactor, labels, relType );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSimplestGraph()
		 public virtual void TestSimplestGraph()
		 {
			  // Layout:
			  //    __
			  //   /  \
			  // (s)  (t)
			  //   \__/
			  Graph.makeEdge( "s", "t" );
			  Graph.makeEdge( "s", "t" );
			  TestShortestPathFinder(finder =>
			  {
				IEnumerable<Path> paths = finder.findAllPaths( Graph.getNode( "s" ), Graph.getNode( "t" ) );
				AssertPaths( paths, "s,t", "s,t" );
				AssertPaths( asList( finder.findSinglePath( Graph.getNode( "s" ), Graph.getNode( "t" ) ) ), "s,t" );
			  }, PathExpanders.forTypeAndDirection( R1, BOTH ), 1);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAnotherSimpleGraph()
		 public virtual void TestAnotherSimpleGraph()
		 {
			  // Layout:
			  //   (m)
			  //   /  \
			  // (s)  (o)---(t)
			  //   \  /       \
			  //   (n)---(p)---(q)
			  Graph.makeEdge( "s", "m" );
			  Graph.makeEdge( "m", "o" );
			  Graph.makeEdge( "s", "n" );
			  Graph.makeEdge( "n", "p" );
			  Graph.makeEdge( "p", "q" );
			  Graph.makeEdge( "q", "t" );
			  Graph.makeEdge( "n", "o" );
			  Graph.makeEdge( "o", "t" );
			  TestShortestPathFinder(finder =>
			  {
				IEnumerable<Path> paths = finder.findAllPaths( Graph.getNode( "s" ), Graph.getNode( "t" ) );
				AssertPaths( paths, "s,m,o,t", "s,n,o,t" );
			  }, PathExpanders.forTypeAndDirection( R1, BOTH ), 6);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCrossedCircle()
		 public virtual void TestCrossedCircle()
		 {
			  // Layout:
			  //    (s)
			  //   /   \
			  // (3)   (1)
			  //  | \ / |
			  //  | / \ |
			  // (4)   (2)
			  //   \   /
			  //    (t)
			  Graph.makeEdge( "s", "1" );
			  Graph.makeEdge( "s", "3" );
			  Graph.makeEdge( "1", "2" );
			  Graph.makeEdge( "1", "4" );
			  Graph.makeEdge( "3", "2" );
			  Graph.makeEdge( "3", "4" );
			  Graph.makeEdge( "2", "t" );
			  Graph.makeEdge( "4", "t" );
			  TestShortestPathFinder( finder => assertPaths( finder.findAllPaths( Graph.getNode( "s" ), Graph.getNode( "t" ) ), "s,1,2,t", "s,1,4,t", "s,3,2,t", "s,3,4,t" ), PathExpanders.forTypeAndDirection( R1, BOTH ), 3 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDirectedFinder()
		 public virtual void TestDirectedFinder()
		 {
			  // Layout:
			  //
			  // (a)->(b)->(c)->(d)->(e)->(f)-------\
			  //    \                                v
			  //     >(g)->(h)->(i)->(j)->(k)->(l)->(m)
			  //
			  Graph.makeEdgeChain( "a,b,c,d,e,f,m" );
			  Graph.makeEdgeChain( "a,g,h,i,j,k,l,m" );
			  TestShortestPathFinder( finder => assertPaths( finder.findAllPaths( Graph.getNode( "a" ), Graph.getNode( "j" ) ), "a,g,h,i,j" ), PathExpanders.forTypeAndDirection( R1, OUTGOING ), 4 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureShortestPathsReturnsNoLoops()
		 public virtual void MakeSureShortestPathsReturnsNoLoops()
		 {
			  // Layout:
			  //
			  // (a)-->(b)==>(c)-->(e)
			  //        ^    /
			  //         \  v
			  //         (d)
			  //
			  Graph.makeEdgeChain( "a,b,c,d,b,c,e" );
			  TestShortestPathFinder(finder =>
			  {
				Node a = Graph.getNode( "a" );
				Node e = Graph.getNode( "e" );
				AssertPaths( finder.findAllPaths( a, e ), "a,b,c,e", "a,b,c,e" );
			  }, PathExpanders.forTypeAndDirection( R1, BOTH ), 6);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void withFilters()
		 public virtual void WithFilters()
		 {
			  // Layout:
			  //
			  // (a)-->(b)-->(c)-->(d)
			  //   \               ^
			  //    -->(g)-->(h)--/
			  //
			  Graph.makeEdgeChain( "a,b,c,d" );
			  Graph.makeEdgeChain( "a,g,h,d" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node a = graph.getNode("a");
			  Node a = Graph.getNode( "a" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node d = graph.getNode("d");
			  Node d = Graph.getNode( "d" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node b = graph.getNode("b");
			  Node b = Graph.getNode( "b" );
			  b.SetProperty( "skip", true );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final System.Predicate<org.neo4j.graphdb.Node> filter = item ->
			  System.Predicate<Node> filter = item =>
			  {
				bool skip = ( bool? ) item.getProperty( "skip", false ).Value;
				return !skip;
			  };
			  TestShortestPathFinder( finder => assertPaths( finder.findAllPaths( a, d ), "a,g,h,d" ), ( ( StandardExpander ) PathExpanders.allTypesAndDirections() ).addNodeFilter(filter), 10 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void filtersTouchesAllIntermediateNodes()
		 public virtual void FiltersTouchesAllIntermediateNodes()
		 {
			  // Layout:
			  //
			  // (a)-->(b)-->(c)-->(d)
			  //
			  Graph.makeEdgeChain( "a,b,c,d" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node a = graph.getNode("a");
			  Node a = Graph.getNode( "a" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node d = graph.getNode("d");
			  Node d = Graph.getNode( "d" );
			  ICollection<Node> touchedByFilter = new HashSet<Node>();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final System.Predicate<org.neo4j.graphdb.Node> filter = item ->
			  System.Predicate<Node> filter = item =>
			  {
				touchedByFilter.Add( item );
				return true;
			  };
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.PathExpander expander = org.neo4j.graphdb.PathExpanderBuilder.empty().add(R1, OUTGOING).addNodeFilter(filter).build();
			  PathExpander expander = PathExpanderBuilder.empty().add(R1, OUTGOING).addNodeFilter(filter).build();
			  //final PathExpander expander = ((StandardExpander) PathExpanders.forTypeAndDirection(R1, OUTGOING)).addNodeFilter( filter );
			  Path path = Iterables.single( GraphAlgoFactory.shortestPath( expander, 10 ).findAllPaths( a, d ) );
			  assertEquals( 3, path.Length() );

			  IList<Node> nodes = Iterables.asList( path.Nodes() );
			  IList<Node> intermediateNodes = nodes.subList( 1, nodes.Count - 1 );
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'containsAll' method:
			  assertTrue( "touchedByFilter: " + touchedByFilter, touchedByFilter.containsAll( intermediateNodes ) );
			  assertTrue( "startNode was not filtered", !touchedByFilter.Contains( a ) );
			  assertTrue( "endNode was not filtered", !touchedByFilter.Contains( d ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testFinderShouldNotFindAnythingBeyondLimit()
		 public virtual void TestFinderShouldNotFindAnythingBeyondLimit()
		 {
			  // Layout:
			  //
			  // (a)-->(b)-->(c)-->(d)-->(e)
			  //
			  Graph.makeEdgeChain( "a,b,c,d,e" );
			  TestShortestPathFinder( finder => assertPaths( finder.findAllPaths( Graph.getNode( "a" ), Graph.getNode( "b" ) ) ), PathExpanders.allTypesAndDirections(), 0 );
			  TestShortestPathFinder(finder =>
			  {
				AssertPaths( finder.findAllPaths( Graph.getNode( "a" ), Graph.getNode( "c" ) ) );
				AssertPaths( finder.findAllPaths( Graph.getNode( "a" ), Graph.getNode( "d" ) ) );
			  }, PathExpanders.allTypesAndDirections(), 1);
			  TestShortestPathFinder(finder =>
			  {
				AssertPaths( finder.findAllPaths( Graph.getNode( "a" ), Graph.getNode( "d" ) ) );
				AssertPaths( finder.findAllPaths( Graph.getNode( "a" ), Graph.getNode( "e" ) ) );
			  }, PathExpanders.allTypesAndDirections(), 2);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureDescentStopsWhenPathIsFound()
		 public virtual void MakeSureDescentStopsWhenPathIsFound()
		 {
			  /*
			   * (a)==>(b)==>(c)==>(d)==>(e)
			   *   \
			   *    v
			   *    (f)-->(g)-->(h)-->(i)
			   */
			  Graph.makeEdgeChain( "a,b,c,d,e" );
			  Graph.makeEdgeChain( "a,b,c,d,e" );
			  Graph.makeEdgeChain( "a,f,g,h,i" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node a = graph.getNode("a");
			  Node a = Graph.getNode( "a" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node b = graph.getNode("b");
			  Node b = Graph.getNode( "b" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node c = graph.getNode("c");
			  Node c = Graph.getNode( "c" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Set<org.neo4j.graphdb.Node> allowedNodes = new java.util.HashSet<>(java.util.Arrays.asList(a, b, c));
			  ISet<Node> allowedNodes = new HashSet<Node>( Arrays.asList( a, b, c ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphalgo.PathFinder<org.neo4j.graphdb.Path> finder = new ShortestPath(100, org.neo4j.graphdb.PathExpanders.forDirection(OUTGOING))
			  PathFinder<Path> finder = new ShortestPathAnonymousInnerClass( this, PathExpanders.forDirection( OUTGOING ), allowedNodes );
			  IEnumerator<Path> paths = finder.FindAllPaths( a, c ).GetEnumerator();
			  for ( int i = 0; i < 4; i++ )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					Path aToBToC = paths.next();
					AssertPath( aToBToC, a, b, c );
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( "should only have contained four paths", paths.hasNext() );
		 }

		 private class ShortestPathAnonymousInnerClass : ShortestPath
		 {
			 private readonly TestShortestPath _outerInstance;

			 private ISet<Node> _allowedNodes;

			 public ShortestPathAnonymousInnerClass( TestShortestPath outerInstance, PathExpander<STATE> forDirection, ISet<Node> allowedNodes ) : base( 100, forDirection )
			 {
				 this.outerInstance = outerInstance;
				 this._allowedNodes = allowedNodes;
			 }

			 protected internal override Node filterNextLevelNodes( Node nextNode )
			 {
				  if ( !_allowedNodes.Contains( nextNode ) )
				  {
						return null;
				  }
				  return nextNode;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureRelationshipNotConnectedIssueNotThere()
		 public virtual void MakeSureRelationshipNotConnectedIssueNotThere()
		 {
			  /*
			   *                                  (g)
			   *                                  / ^
			   *                                 v   \
			   * (a)<--(b)<--(c)<--(d)<--(e)<--(f)   (i)
			   *                                 ^   /
			   *                                  \ v
			   *                                  (h)
			   */
			  Graph.makeEdgeChain( "i,g,f,e,d,c,b,a" );
			  Graph.makeEdgeChain( "i,h,f" );
			  TestShortestPathFinder(finder =>
			  {
				Node start = Graph.getNode( "a" );
				Node end = Graph.getNode( "i" );
				AssertPaths( finder.findAllPaths( start, end ), "a,b,c,d,e,f,g,i", "a,b,c,d,e,f,h,i" );
			  }, PathExpanders.forTypeAndDirection( R1, INCOMING ), 10);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureShortestPathCanBeFetchedEvenIfANodeHasLoops()
		 public virtual void MakeSureShortestPathCanBeFetchedEvenIfANodeHasLoops()
		 {
			  // Layout:
			  //
			  // = means loop :)
			  //
			  //   (m)
			  //   /  \
			  // (s)  (o)=
			  //   \  /
			  //   (n)=
			  //    |
			  //   (p)
			  Graph.makeEdgeChain( "m,s,n,p" );
			  Graph.makeEdgeChain( "m,o,n" );
			  Graph.makeEdge( "o", "o" );
			  Graph.makeEdge( "n", "n" );
			  TestShortestPathFinder( finder => assertPaths( finder.findAllPaths( Graph.getNode( "m" ), Graph.getNode( "p" ) ), "m,s,n,p", "m,o,n,p" ), PathExpanders.forTypeAndDirection( R1, BOTH ), 3 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureAMaxResultCountIsObeyed()
		 public virtual void MakeSureAMaxResultCountIsObeyed()
		 {
			  // Layout:
			  //
			  //   (a)--(b)--(c)--(d)--(e)
			  //    |                 / | \
			  //   (f)--(g)---------(h) |  \
			  //    |                   |   |
			  //   (i)-----------------(j)  |
			  //    |                       |
			  //   (k)----------------------
			  //
			  Graph.makeEdgeChain( "a,b,c,d,e" );
			  Graph.makeEdgeChain( "a,f,g,h,e" );
			  Graph.makeEdgeChain( "f,i,j,e" );
			  Graph.makeEdgeChain( "i,k,e" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node a = graph.getNode("a");
			  Node a = Graph.getNode( "a" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node e = graph.getNode("e");
			  Node e = Graph.getNode( "e" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.PathExpander expander = org.neo4j.graphdb.PathExpanders.forTypeAndDirection(R1, OUTGOING);
			  PathExpander expander = PathExpanders.forTypeAndDirection( R1, OUTGOING );
			  TestShortestPathFinder( finder => assertEquals( 4, Iterables.count( finder.findAllPaths( a, e ) ) ), expander, 10, 10 );
			  for ( int i = 4; i >= 1; i-- )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int count = i;
					int count = i;
					TestShortestPathFinder( finder => assertEquals( count, Iterables.count( finder.findAllPaths( a, e ) ) ), expander, 10, count );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unfortunateRelationshipOrderingInTriangle()
		 public virtual void UnfortunateRelationshipOrderingInTriangle()
		 {
			  /*
			   *            (b)
			   *           ^   \
			   *          /     v
			   *        (a)---->(c)
			   *
			   * Relationships are created in such a way that they are iterated in the worst order,
			   * i.e. (S) a-->b, (E) c<--b, (S) a-->c
			   */
			  Graph.makeEdgeChain( "a,b,c" );
			  Graph.makeEdgeChain( "a,c" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node a = graph.getNode("a");
			  Node a = Graph.getNode( "a" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node c = graph.getNode("c");
			  Node c = Graph.getNode( "c" );
			  TestShortestPathFinder( finder => assertPathDef( finder.findSinglePath( a, c ), "a", "c" ), PathExpanders.forTypeAndDirection( R1, OUTGOING ), 2 );
			  TestShortestPathFinder( finder => assertPathDef( finder.findSinglePath( c, a ), "c", "a" ), PathExpanders.forTypeAndDirection( R1, INCOMING ), 2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindShortestPathWhenOneSideFindsLongerPathFirst()
		 public virtual void ShouldFindShortestPathWhenOneSideFindsLongerPathFirst()
		 {
			  /*
			  The order in which nodes are created matters when reproducing the original problem
			   */
			  Graph.makeEdge( "start", "c" );
			  Graph.makeEdge( "start", "a" );
			  Graph.makeEdge( "b", "end" );
			  Graph.makeEdge( "d", "end" );
			  Graph.makeEdge( "c", "e" );
			  Graph.makeEdge( "f", "end" );
			  Graph.makeEdge( "c", "b" );
			  Graph.makeEdge( "e", "end" );
			  Graph.makeEdge( "a", "end" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node start = graph.getNode("start");
			  Node start = Graph.getNode( "start" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Node end = graph.getNode("end");
			  Node end = Graph.getNode( "end" );
			  assertThat( ( new ShortestPath( 2, allTypesAndDirections(), 42 ) ).FindSinglePath(start, end).length(), @is(2) );
			  assertThat( ( new ShortestPath( 3, allTypesAndDirections(), 42 ) ).FindSinglePath(start, end).length(), @is(2) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMakeSureResultLimitIsRespectedForMultiPathHits()
		 public virtual void ShouldMakeSureResultLimitIsRespectedForMultiPathHits()
		 {
			  /*       _____
			   *      /     \
			   *    (a)-----(b)
			   *      \_____/
			   */
			  for ( int i = 0; i < 3; i++ )
			  {
					Graph.makeEdge( "a", "b" );
			  }

			  Node a = Graph.getNode( "a" );
			  Node b = Graph.getNode( "b" );
			  TestShortestPathFinder( finder => assertEquals( 1, count( finder.findAllPaths( a, b ) ) ), allTypesAndDirections(), 2, 1 );
		 }

		 private void TestShortestPathFinder( PathFinderTester tester, PathExpander expander, int maxDepth )
		 {
			  TestShortestPathFinder( tester, expander, maxDepth, null );
		 }

		 private void TestShortestPathFinder( PathFinderTester tester, PathExpander expander, int maxDepth, int? maxResultCount )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LengthCheckingExpanderWrapper lengthChecker = new LengthCheckingExpanderWrapper(expander);
			  LengthCheckingExpanderWrapper lengthChecker = new LengthCheckingExpanderWrapper( expander );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<org.neo4j.graphalgo.PathFinder<org.neo4j.graphdb.Path>> finders = new java.util.ArrayList<>();
			  IList<PathFinder<Path>> finders = new List<PathFinder<Path>>();
			  finders.Add( maxResultCount != null ? shortestPath( lengthChecker, maxDepth, maxResultCount ) : shortestPath( lengthChecker, maxDepth ) );
			  finders.add(maxResultCount != null ? new TraversalShortestPath(lengthChecker, maxDepth, maxResultCount.Value)
															  : new TraversalShortestPath( lengthChecker, maxDepth ));
			  foreach ( PathFinder<Path> finder in finders )
			  {
					tester.Test( finder );
			  }
		 }

		 private interface PathFinderTester
		 {
			  void Test( PathFinder<Path> finder );
		 }

		 private class LengthCheckingExpanderWrapper : PathExpander<object>
		 {
			  internal readonly PathExpander Expander;

			  internal LengthCheckingExpanderWrapper( PathExpander expander )
			  {
					this.Expander = expander;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") public Iterable<org.neo4j.graphdb.Relationship> expand(org.neo4j.graphdb.Path path, org.neo4j.graphdb.traversal.BranchState<Object> state)
			  public override IEnumerable<Relationship> Expand( Path path, BranchState<object> state )
			  {
					if ( path.StartNode().Equals(path.EndNode()) )
					{
						 assertEquals( "Path length must be zero", 0, path.Length() );
					}
					else
					{
						 assertTrue( "Path length must be positive", path.Length() > 0 );
					}
					return Expander.expand( path, state );
			  }

			  public override PathExpander<object> Reverse()
			  {
					return new LengthCheckingExpanderWrapper( Expander.reverse() );
			  }
		 }

		 // Used to count how many nodes are visited
		 private class CountingPathExpander : PathExpander
		 {
			 private readonly TestShortestPath _outerInstance;

			  internal MutableInt NodesVisited;
			  internal readonly PathExpander Delegate;

			  internal CountingPathExpander( TestShortestPath outerInstance, PathExpander @delegate )
			  {
				  this._outerInstance = outerInstance;
					NodesVisited = new MutableInt( 0 );
					this.Delegate = @delegate;
			  }

			  internal CountingPathExpander( TestShortestPath outerInstance, PathExpander @delegate, MutableInt nodesVisited ) : this( outerInstance, @delegate )
			  {
				  this._outerInstance = outerInstance;
					this.NodesVisited = nodesVisited;
			  }

			  public override System.Collections.IEnumerable Expand( Path path, BranchState state )
			  {
					NodesVisited.increment();
					return Delegate.expand( path, state );
			  }

			  public override PathExpander Reverse()
			  {
					return new CountingPathExpander( _outerInstance, Delegate.reverse(), NodesVisited );
			  }
		 }
	}

}