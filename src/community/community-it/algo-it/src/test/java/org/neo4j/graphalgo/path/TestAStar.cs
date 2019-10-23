using System;
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
namespace Neo4Net.GraphAlgo.path
{
	using Neo4NetAlgoTestCase = Common.Neo4NetAlgoTestCase;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Parameters = org.junit.runners.Parameterized.Parameters;


	using Neo4Net.GraphAlgo;
	using Neo4Net.GraphAlgo;
	using TraversalAStar = Neo4Net.GraphAlgo.Path.TraversalAStar;
	using Node = Neo4Net.GraphDb.Node;
	using Path = Neo4Net.GraphDb.Path;
	using Neo4Net.GraphDb;
	using PathExpanders = Neo4Net.GraphDb.PathExpanders;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Neo4Net.GraphDb;
	using Neo4Net.GraphDb.Traversal;
	using Neo4Net.GraphDb.Traversal;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using MapUtil = Neo4Net.Helpers.Collections.MapUtil;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphalgo.CommonEvaluators.doubleCostEvaluator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphalgo.GraphAlgoFactory.aStar;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Direction.OUTGOING;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class TestAStar extends common.Neo4NetAlgoTestCase
	public class TestAStar : Neo4NetAlgoTestCase
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pathToSelfReturnsZero()
		 public virtual void PathToSelfReturnsZero()
		 {
			  // GIVEN
			  Node start = Graph.makeNode( "start", "x", 0d, "y", 0d );

			  // WHEN
			  WeightedPath path = _finder.findSinglePath( start, start );
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
			  Node start = Graph.makeNode( "start", "x", 0d, "y", 0d );

			  // WHEN
			  ResourceIterable<WeightedPath> paths = Iterables.asResourceIterable( _finder.findAllPaths( start, start ) );

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
//ORIGINAL LINE: @Test public void wikipediaExample()
		 public virtual void WikipediaExample()
		 {
			  /* GIVEN
			   *
			   * (start)---2--->(d)
			   *    \             \
			   *    1.5            .\
			   *     v               3
			   *    (a)-\             v
			   *         -2-\         (e)
			   *             ->(b)     \
			   *               /        \
			   *           /--           2
			   *      /-3-                v
			   *     v        --4------->(end)
			   *    (c)------/
			   */
			  Node start = Graph.makeNode( "start", "x", 0d, "y", 0d );
			  Graph.makeNode( "a", "x", 0.3d, "y", 1d );
			  Graph.makeNode( "b", "x", 2d, "y", 2d );
			  Graph.makeNode( "c", "x", 0d, "y", 3d );
			  Graph.makeNode( "d", "x", 2d, "y", 0d );
			  Graph.makeNode( "e", "x", 3d, "y", 1.5d );
			  Node end = Graph.makeNode( "end", "x", 3.3d, "y", 2.8d );
			  Graph.makeEdge( "start", "a", "length", 1.5d );
			  Graph.makeEdge( "a", "b", "length", 2f );
			  Graph.makeEdge( "b", "c", "length", 3 );
			  Graph.makeEdge( "c", "end", "length", 4L );
			  Graph.makeEdge( "start", "d", "length", ( short )2 );
			  Graph.makeEdge( "d", "e", "length", ( sbyte )3 );
			  Graph.makeEdge( "e", "end", "length", 2 );

			  // WHEN
			  WeightedPath path = _finder.findSinglePath( start, end );
			  // THEN
			  AssertPathDef( path, "start", "d", "e", "end" );
		 }

		 /// <summary>
		 /// <pre>
		 ///   01234567
		 ///  +-------->x  A - C: 10
		 /// 0|A      C    A - B:  2 (x2)
		 /// 1|  B         B - C:  3
		 ///  V
		 ///  y
		 /// </pre>
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSimplest()
		 public virtual void TestSimplest()
		 {
			  Node nodeA = Graph.makeNode( "A", "x", 0d, "y", 0d );
			  Node nodeB = Graph.makeNode( "B", "x", 2d, "y", 1d );
			  Node nodeC = Graph.makeNode( "C", "x", 7d, "y", 0d );
			  Relationship relAB = Graph.makeEdge( "A", "B", "length", 2d );
			  Relationship relAB2 = Graph.makeEdge( "A", "B", "length", 2 );
			  Relationship relBC = Graph.makeEdge( "B", "C", "length", 3f );
			  Relationship relAC = Graph.makeEdge( "A", "C", "length", ( short )10 );

			  int counter = 0;
			  IEnumerable<WeightedPath> allPaths = _finder.findAllPaths( nodeA, nodeC );
			  foreach ( WeightedPath path in allPaths )
			  {
					assertEquals( ( double? )5d, ( double? )path.Weight() );
					assertPath( path, nodeA, nodeB, nodeC );
					counter++;
			  }
			  assertEquals( 1, counter );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({ "rawtypes", "unchecked" }) @Test public void canUseBranchState()
		 public virtual void CanUseBranchState()
		 {
			  // This test doesn't use the predefined finder, which only means an unnecessary instantiation
			  // if such an object. And this test will be run twice (once for each finder type in data()).
			  /// <summary>
			  /// <pre>
			  ///   012345    A - B:  2
			  ///  +------>y  A - B:  2
			  /// 0|A         B - C:  3
			  /// 1|          A - C:  10
			  /// 2| B
			  /// 3|
			  /// 4|
			  /// 5|
			  /// 6|
			  /// 7|C
			  ///  V
			  ///  x
			  /// 
			  /// </pre>
			  /// </summary>
			  Node nodeA = Graph.makeNode( "A", "x", 0d, "y", 0d );
			  Node nodeB = Graph.makeNode( "B", "x", 2d, "y", 1d );
			  Node nodeC = Graph.makeNode( "C", "x", 7d, "y", 0d );
			  Graph.makeEdge( "A", "B", "length", 2d );
			  Graph.makeEdge( "A", "B", "length", 2d );
			  Graph.makeEdge( "B", "C", "length", 3d );
			  Graph.makeEdge( "A", "C", "length", 10d );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<org.Neo4Net.graphdb.Node, double> seenBranchStates = new java.util.HashMap<>();
			  IDictionary<Node, double> seenBranchStates = new Dictionary<Node, double>();
			  PathExpander<double> expander = new PathExpanderAnonymousInnerClass( this, seenBranchStates );

			  double initialStateValue = 0D;
			  PathFinder<WeightedPath> traversalFinder = new TraversalAStar( expander, new Neo4Net.GraphDb.Traversal.InitialBranchState_State( initialStateValue, initialStateValue ), doubleCostEvaluator( "length" ), EstimateEvaluator );
			  WeightedPath path = traversalFinder.FindSinglePath( nodeA, nodeC );
			  assertEquals( ( double? ) 5.0D, ( double? ) path.Weight() );
			  AssertPathDef( path, "A", "B", "C" );
			  assertEquals( MapUtil.genericMap<Node, double>( nodeA, 0D, nodeB, 2D ), seenBranchStates );
		 }

		 private class PathExpanderAnonymousInnerClass : PathExpander<double>
		 {
			 private readonly TestAStar _outerInstance;

			 private IDictionary<Node, double> _seenBranchStates;

			 public PathExpanderAnonymousInnerClass( TestAStar outerInstance, IDictionary<Node, double> seenBranchStates )
			 {
				 this.outerInstance = outerInstance;
				 this._seenBranchStates = seenBranchStates;
			 }

			 public IEnumerable<Relationship> expand( Path path, BranchState<double> state )
			 {
				  double newState = state.State;
				  if ( path.Length() > 0 )
				  {
						newState += ( double? ) path.LastRelationship().getProperty("length").Value;
						state.State = newState;
				  }
				  _seenBranchStates[path.EndNode()] = newState;

				  return path.EndNode().getRelationships(OUTGOING);
			 }

			 public PathExpander<double> reverse()
			 {
				  throw new System.NotSupportedException();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void betterTentativePath()
		 public virtual void BetterTentativePath()
		 {
			  // GIVEN
			  EstimateEvaluator<double> estimator = ( node, goal ) => ( double? ) node.getProperty( "estimate" );
			  PathFinder<WeightedPath> finder = aStar( PathExpanders.allTypesAndDirections(), doubleCostEvaluator("weight", 0d), estimator );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.Node node1 = graph.makeNode("1", "estimate", 0.003d);
			  Node node1 = Graph.makeNode( "1", "estimate", 0.003d );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.Node node2 = graph.makeNode("2", "estimate", 0.002d);
			  Node node2 = Graph.makeNode( "2", "estimate", 0.002d );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.Node node3 = graph.makeNode("3", "estimate", 0.001d);
			  Node node3 = Graph.makeNode( "3", "estimate", 0.001d );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.Node node4 = graph.makeNode("4", "estimate", 0d);
			  Node node4 = Graph.makeNode( "4", "estimate", 0d );
			  Graph.makeEdge( "1", "3", "weight", 0.253d );
			  Graph.makeEdge( "1", "2", "weight", 0.018d );
			  Graph.makeEdge( "2", "4", "weight", 0.210d );
			  Graph.makeEdge( "2", "3", "weight", 0.180d );
			  Graph.makeEdge( "2", "3", "weight", 0.024d );
			  Graph.makeEdge( "3", "4", "weight", 0.135d );
			  Graph.makeEdge( "3", "4", "weight", 0.013d );

			  // WHEN
			  WeightedPath best14 = finder.FindSinglePath( node1, node4 );
			  // THEN
			  assertPath( best14, node1, node2, node3, node4 );
		 }

		 internal static EstimateEvaluator<double> EstimateEvaluator = ( node, goal ) =>
		 {
		  double dx = ( double? ) node.getProperty( "x" ) - ( double? ) goal.getProperty( "x" );
		  double dy = ( double? ) node.getProperty( "y" ) - ( double? ) goal.getProperty( "y" );
		  return Math.Sqrt( Math.Pow( dx, 2 ) + Math.Pow( dy, 2 ) );
		 };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters public static java.util.Collection<Object[]> data()
		 public static ICollection<object[]> Data()
		 {
			  return Arrays.asList(new object[][]
			  {
				  new object[] { GraphAlgoFactory.aStar( PathExpanders.allTypesAndDirections(), doubleCostEvaluator("length"), EstimateEvaluator ) },
				  new object[] { new TraversalAStar( PathExpanders.allTypesAndDirections(), doubleCostEvaluator("length"), EstimateEvaluator ) }
			  });
		 }

		 private readonly PathFinder<WeightedPath> _finder;

		 public TestAStar( PathFinder<WeightedPath> finder )
		 {
			  this._finder = finder;
		 }
	}

}