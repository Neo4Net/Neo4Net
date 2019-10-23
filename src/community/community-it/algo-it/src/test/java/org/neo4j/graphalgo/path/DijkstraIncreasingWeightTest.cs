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


	using Neo4Net.GraphAlgo;
	using Dijkstra = Neo4Net.GraphAlgo.Path.Dijkstra;
	using PathInterestFactory = Neo4Net.GraphAlgo.Utils.PathInterestFactory;
	using Node = Neo4Net.GraphDb.Node;
	using Path = Neo4Net.GraphDb.Path;
	using Neo4Net.GraphDb;
	using PathExpanders = Neo4Net.GraphDb.PathExpanders;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Neo4Net.GraphDb;
	using Neo4Net.GraphDb;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Neo4Net.GraphDb.Traversal;
	using Neo4Net.GraphDb.Traversal;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using NoneStrictMath = Neo4Net.Kernel.impl.util.NoneStrictMath;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;


	public class DijkstraIncreasingWeightTest : Neo4NetAlgoTestCase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canFetchLongerPaths()
		 public virtual void CanFetchLongerPaths()
		 {
			  /*
			   *    1-(b)-1
			   *   /       \
			   * (s) - 1 - (a) - 1 - (c) - 10 - (d) - 10 - (t)
			   *
			   */
			  Node s = Graph.makeNode( "s" );
			  Node a = Graph.makeNode( "a" );
			  Node b = Graph.makeNode( "b" );
			  Node c = Graph.makeNode( "c" );
			  Node d = Graph.makeNode( "d" );
			  Node t = Graph.makeNode( "t" );

			  Graph.makeEdge( "s", "a", "length", 1 );
			  Graph.makeEdge( "a", "c", "length", 1 );
			  Graph.makeEdge( "s", "b", "length", 1 );
			  Graph.makeEdge( "b", "a", "length", 1 );
			  Graph.makeEdge( "c", "d", "length", 10 );
			  Graph.makeEdge( "d", "t", "length", 10 );

			  PathExpander expander = PathExpanders.allTypesAndDirections();
			  Dijkstra algo = new Dijkstra( expander, CommonEvaluators.doubleCostEvaluator( "length" ), PathInterestFactory.all( NoneStrictMath.EPSILON ) );

			  IEnumerator<WeightedPath> paths = algo.FindAllPaths( s, t ).GetEnumerator();

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( "Expected at least one path.", paths.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertPath( paths.next(), s, a, c, d, t );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( "Expected two paths", paths.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertPath( paths.next(), s, b, a, c, d, t );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnPathsInIncreasingOrderOfCost()
		 public virtual void ShouldReturnPathsInIncreasingOrderOfCost()
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

			  PathExpander expander = PathExpanders.allTypesAndDirections();
			  Dijkstra algo = new Dijkstra( expander, CommonEvaluators.doubleCostEvaluator( "length" ), PathInterestFactory.all( NoneStrictMath.EPSILON ) );

			  IEnumerator<WeightedPath> paths = algo.FindAllPaths( s, t ).GetEnumerator();

			  for ( int i = 1; i <= 3; i++ )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( "Expected at least " + i + " path(s)", paths.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( "Expected 3 paths of cost 2", NoneStrictMath.Equals( paths.next().weight(), 2 ) );
			  }
			  for ( int i = 1; i <= 3; i++ )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( "Expected at least " + i + " path(s)", paths.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( "Expected 3 paths of cost 3", NoneStrictMath.Equals( paths.next().weight(), 3 ) );
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( "Expected at least 7 paths", paths.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( "Expected 1 path of cost 4", NoneStrictMath.Equals( paths.next().weight(), 4 ) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( "Expected exactly 7 paths", paths.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 5000) public void testForLoops()
		 public virtual void TestForLoops()
		 {
			  /*
			   *
			   *            (b)
			   *           /  \         0
			   *          0    0       / \            - 0 - (c1) - 0 -
			   *           \  /        \/           /                 \
			   * (s) - 1 - (a1) - 1 - (a2) - 1 - (a3)                (a4) - 1 - (t)
			   *                                    \                 /
			   *                                     - 0 - (c2) - 0 -
			   *
			   */

			  using ( Transaction tx = GraphDb.beginTx() )
			  {
					Node s = Graph.makeNode( "s" );
					Node t = Graph.makeNode( "t" );

					// Blob loop
					Graph.makeEdge( "s", "a1", "length", 1 );
					Graph.makeEdge( "a1", "b", "length", 0 );
					Graph.makeEdge( "b", "a1", "length", 0 );

					// Self loop
					Graph.makeEdge( "a1", "a2", "length", 1 );
					Graph.makeEdge( "a2", "a2", "length", 0 );

					// Diamond loop
					Graph.makeEdge( "a2", "a3", "length", 1 );
					Graph.makeEdge( "a3", "c1", "length", 0 );
					Graph.makeEdge( "a3", "c2", "length", 0 );
					Graph.makeEdge( "c1", "a4", "length", 0 );
					Graph.makeEdge( "c1", "a4", "length", 0 );
					Graph.makeEdge( "a4", "t", "length", 1 );

					PathExpander expander = PathExpanders.allTypesAndDirections();
					Dijkstra algo = new Dijkstra( expander, CommonEvaluators.doubleCostEvaluator( "length" ), PathInterestFactory.all( NoneStrictMath.EPSILON ) );

					IEnumerator<WeightedPath> paths = algo.FindAllPaths( s, t ).GetEnumerator();

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( "Expected at least one path", paths.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( "Expected first path of length 6", 6, paths.next().length() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( "Expected at least two paths", paths.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( "Expected second path of length 6", 6, paths.next().length() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( "Expected exactly two paths", paths.hasNext() );

					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testKShortestPaths()
		 public virtual void TestKShortestPaths()
		 {
			  /*
			   *      ----- (e) - 3 - (f) ---
			   *    /                         \
			   *   /    ------- (a) --------   \
			   *  3   /            \         \  3
			   *  |  2              0         0 |
			   *  | /                \         \|
			   * (s) - 1 - (c) - 1 - (d) - 1 - (t)
			   *   \                 /
			   *    -- 1 - (b) - 1 -
			   *
			   */
			  Node s = Graph.makeNode( "s" );
			  Node t = Graph.makeNode( "t" );

			  Graph.makeEdge( "s", "a", "length", 2 );
			  Graph.makeEdge( "s", "b", "length", 1 );
			  Graph.makeEdge( "s", "c", "length", 1 );
			  Graph.makeEdge( "s", "e", "length", 3 );
			  Graph.makeEdge( "a", "t", "length", 0 );
			  Graph.makeEdge( "b", "d", "length", 1 );
			  Graph.makeEdge( "c", "d", "length", 1 );
			  Graph.makeEdge( "d", "a", "length", 0 );
			  Graph.makeEdge( "d", "t", "length", 1 );
			  Graph.makeEdge( "e", "f", "length", 3 );
			  Graph.makeEdge( "f", "t", "length", 3 );

			  PathExpander expander = PathExpanders.allTypesAndDirections();
			  PathFinder<WeightedPath> algo = new Dijkstra( expander, CommonEvaluators.doubleCostEvaluator( "length" ), PathInterestFactory.numberOfShortest( NoneStrictMath.EPSILON, 6 ) );

			  IEnumerator<WeightedPath> paths = algo.FindAllPaths( s, t ).GetEnumerator();

			  int count = 0;
			  while ( paths.MoveNext() )
			  {
					count++;
					WeightedPath path = paths.Current;
					double expectedWeight;
					if ( count <= 3 )
					{
						 expectedWeight = 2.0;
					}
					else
					{
						 expectedWeight = 3.0;
					}
					assertTrue( "Expected path number " + count + " to have weight of " + expectedWeight, NoneStrictMath.Equals( path.Weight(), expectedWeight ) );
			  }
			  assertEquals( "Expected exactly 6 returned paths", 6, count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void withState()
		 public virtual void WithState()
		 {
			  /* Graph
			   *
			   * (a)-[1]->(b)-[2]->(c)-[5]->(d)
			   */

			  Graph.makeEdgeChain( "a,b,c,d" );
			  SetWeight( "a", "b", 1 );
			  SetWeight( "b", "c", 2 );
			  SetWeight( "c", "d", 5 );

			  InitialBranchState<int> state = new Neo4Net.GraphDb.Traversal.InitialBranchState_State<int>( 0, 0 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<org.Neo4Net.graphdb.Node, int> encounteredState = new java.util.HashMap<>();
			  IDictionary<Node, int> encounteredState = new Dictionary<Node, int>();
			  PathExpander<int> expander = new PathExpanderAnonymousInnerClass( this, state, encounteredState );

			  PathFinder<WeightedPath> finder = new Dijkstra( expander, state, CommonEvaluators.doubleCostEvaluator( "weight" ) );
			  AssertPaths( finder.FindAllPaths( Graph.getNode( "a" ), Graph.getNode( "d" ) ), "a,b,c,d" );
			  assertEquals( 1, encounteredState[Graph.getNode( "b" )] );
			  assertEquals( 3, encounteredState[Graph.getNode( "c" )] );
			  assertEquals( 8, encounteredState[Graph.getNode( "d" )] );
		 }

		 private class PathExpanderAnonymousInnerClass : PathExpander<int>
		 {
			 private readonly DijkstraIncreasingWeightTest _outerInstance;

			 private InitialBranchState<int> _state;
			 private IDictionary<Node, int> _encounteredState;

			 public PathExpanderAnonymousInnerClass( DijkstraIncreasingWeightTest outerInstance, InitialBranchState<int> state, IDictionary<Node, int> encounteredState )
			 {
				 this.outerInstance = outerInstance;
				 this._state = state;
				 this._encounteredState = encounteredState;
			 }

			 public IEnumerable<Relationship> expand( Path path, BranchState<int> state )
			 {
				  if ( path.Length() > 0 )
				  {
						int newState = state.State + ( ( Number )path.LastRelationship().getProperty("weight") ).intValue();
						state.State = newState;
						_encounteredState[path.EndNode()] = newState;
				  }
				  return path.EndNode().Relationships;
			 }

			 public PathExpander<int> reverse()
			 {
				  return this;
			 }
		 }

		 private void SetWeight( string start, string end, double weight )
		 {
			  Node startNode = Graph.getNode( start );
			  Node endNode = Graph.getNode( end );
			  ResourceIterable<Relationship> relationships = Iterables.asResourceIterable( startNode.Relationships );
			  using ( ResourceIterator<Relationship> resourceIterator = relationships.GetEnumerator() )
			  {
					while ( resourceIterator.MoveNext() )
					{
						 Relationship rel = resourceIterator.Current;
						 if ( rel.GetOtherNode( startNode ).Equals( endNode ) )
						 {
							  rel.SetProperty( "weight", weight );
							  return;
						 }
					}
			  }
			  throw new Exception( "No relationship between nodes " + start + " and " + end );
		 }
	}

}