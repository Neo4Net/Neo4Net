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
namespace Org.Neo4j.Graphalgo.shortestpath
{
	using Neo4jAlgoTestCase = Common.Neo4jAlgoTestCase;
	using Test = org.junit.Test;


	using Org.Neo4j.Graphalgo.impl.shortestpath;
	using DoubleAdder = Org.Neo4j.Graphalgo.impl.util.DoubleAdder;
	using Direction = Org.Neo4j.Graphdb.Direction;
	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;

	public class DijkstraIteratorTest : Neo4jAlgoTestCase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRun()
		 public virtual void TestRun()
		 {
			  ( new TestDijkstra( this ) ).RunTest();
		 }

		 protected internal class TestDijkstra : Dijkstra<double>
		 {
			 private readonly DijkstraIteratorTest _outerInstance;

			  public TestDijkstra( DijkstraIteratorTest outerInstance ) : base( 0.0, null, null, CommonEvaluators.doubleCostEvaluator( "cost" ), new DoubleAdder(), double?.compareTo, Direction.BOTH, MyRelTypes.R1 )
			  {
				  this._outerInstance = outerInstance;
			  }

			  protected internal class TestIterator : Dijkstra<double>.DijkstraIterator
			  {
				  private readonly DijkstraIteratorTest.TestDijkstra _outerInstance;

					public TestIterator( DijkstraIteratorTest.TestDijkstra outerInstance, Node startNode, Dictionary<Node, IList<Relationship>> predecessors, Dictionary<Node, double> mySeen, Dictionary<Node, double> otherSeen, Dictionary<Node, double> myDistances, Dictionary<Node, double> otherDistances, bool backwards ) : base( startNode, predecessors, mySeen, otherSeen, myDistances, otherDistances, backwards )
					{
						this._outerInstance = outerInstance;
					}
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void runTest()
			  public virtual void RunTest()
			  {
					Graph.makeEdge( "start", "a", "cost", ( double ) 1 );
					Graph.makeEdge( "a", "x", "cost", ( double ) 9 );
					Graph.makeEdge( "a", "b", "cost", ( float ) 1 );
					Graph.makeEdge( "b", "x", "cost", ( double ) 7 );
					Graph.makeEdge( "b", "c", "cost", ( long ) 1 );
					Graph.makeEdge( "c", "x", "cost", 5 );
					Graph.makeEdge( "c", "d", "cost", ( sbyte ) 1 );
					Graph.makeEdge( "d", "x", "cost", ( short ) 3 );
					Graph.makeEdge( "d", "e", "cost", ( double ) 1 );
					Graph.makeEdge( "e", "x", "cost", ( double ) 1 );
					Dictionary<Node, double> seen1 = new Dictionary<Node, double>();
					Dictionary<Node, double> seen2 = new Dictionary<Node, double>();
					Dictionary<Node, double> dists1 = new Dictionary<Node, double>();
					Dictionary<Node, double> dists2 = new Dictionary<Node, double>();
					DijkstraIterator iter1 = new TestIterator( this, Graph.getNode( "start" ), Predecessors1, seen1, seen2, dists1, dists2, false );
					// while ( iter1.hasNext() && !limitReached() && !iter1.isDone() )
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( iter1.Next(), Graph.getNode("start") );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( iter1.Next(), Graph.getNode("a") );
					assertEquals( 10.0, seen1[Graph.getNode( "x" )], 0.0 );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( iter1.Next(), Graph.getNode("b") );
					assertEquals( 9.0, seen1[Graph.getNode( "x" )], 0.0 );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( iter1.Next(), Graph.getNode("c") );
					assertEquals( 8.0, seen1[Graph.getNode( "x" )], 0.0 );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( iter1.Next(), Graph.getNode("d") );
					assertEquals( 7.0, seen1[Graph.getNode( "x" )], 0.0 );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( iter1.Next(), Graph.getNode("e") );
					assertEquals( 6.0, seen1[Graph.getNode( "x" )], 0.0 );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( iter1.Next(), Graph.getNode("x") );
					assertEquals( 6.0, seen1[Graph.getNode( "x" )], 0.0 );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( iter1.HasNext() );
					int count = 0;
					// This code below is correct for the alternative priority queue
					// while ( iter1.hasNext() )
					// {
					// iter1.next();
					// ++count;
					// }
					// assertTrue( count == 4 );
					// assertTrue( seen1.get( graph.getNode( "x" ) ) == 6.0 );
					// Now test node limit
					seen1 = new Dictionary<Node, double>();
					seen2 = new Dictionary<Node, double>();
					dists1 = new Dictionary<Node, double>();
					dists2 = new Dictionary<Node, double>();
					iter1 = new TestIterator( this, Graph.getNode( "start" ), Predecessors1, seen1, seen2, dists1, dists2, false );
					this.NumberOfNodesTraversed = 0;
					this.LimitMaxNodesToTraverse( 3 );
					count = 0;
					while ( iter1.MoveNext() )
					{
						 iter1.Current;
						 ++count;
					}
					assertEquals( 3, count );
			  }
		 }
	}

}