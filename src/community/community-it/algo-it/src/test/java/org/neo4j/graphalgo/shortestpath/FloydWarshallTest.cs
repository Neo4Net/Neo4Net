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
	using Test = org.junit.Test;

	using Neo4Net.Graphalgo.impl.shortestpath;
	using Direction = Neo4Net.GraphDb.Direction;
	using Node = Neo4Net.GraphDb.Node;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class FloydWarshallTest : Neo4NetAlgoTestCase
	{
		 /// <summary>
		 /// Test case for paths of length 0 and 1, and an impossible path
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMinimal()
		 public virtual void TestMinimal()
		 {
			  Graph.makeEdge( "a", "b", "cost", ( double ) 1 );
			  Graph.makeEdge( "a", "c", "cost", ( float ) 1 );
			  Graph.makeEdge( "a", "d", "cost", ( long ) 1 );
			  Graph.makeEdge( "a", "e", "cost", 1 );
			  Graph.makeEdge( "b", "c", "cost", ( double ) 1 );
			  Graph.makeEdge( "c", "d", "cost", ( sbyte ) 1 );
			  Graph.makeEdge( "d", "e", "cost", ( short ) 1 );
			  Graph.makeEdge( "e", "b", "cost", ( sbyte ) 1 );
			  FloydWarshall<double> floydWarshall = new FloydWarshall<double>( 0.0, double.MaxValue, Direction.OUTGOING, CommonEvaluators.doubleCostEvaluator( "cost" ), new Neo4Net.Graphalgo.impl.util.DoubleAdder(), double?.compareTo, Graph.AllNodes, Graph.AllEdges );
			  assertEquals( 0.0, floydWarshall.GetCost( Graph.getNode( "a" ), Graph.getNode( "a" ) ), 0.0 );
			  assertEquals( 1.0, floydWarshall.GetCost( Graph.getNode( "a" ), Graph.getNode( "b" ) ), 0.0 );
			  assertEquals( floydWarshall.GetCost( Graph.getNode( "b" ), Graph.getNode( "a" ) ), double.MaxValue, 0.0 );
		 }

		 /// <summary>
		 /// Test case for extracting paths
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testPath()
		 public virtual void TestPath()
		 {
			  Graph.makeEdge( "a", "b", "cost", ( double ) 1 );
			  Graph.makeEdge( "b", "c", "cost", ( float ) 1 );
			  Graph.makeEdge( "c", "d", "cost", 1 );
			  Graph.makeEdge( "d", "e", "cost", ( long ) 1 );
			  Graph.makeEdge( "e", "f", "cost", ( sbyte ) 1 );
			  FloydWarshall<double> floydWarshall = new FloydWarshall<double>( 0.0, double.MaxValue, Direction.OUTGOING, CommonEvaluators.doubleCostEvaluator( "cost" ), new Neo4Net.Graphalgo.impl.util.DoubleAdder(), double?.compareTo, Graph.AllNodes, Graph.AllEdges );
			  IList<Node> path = floydWarshall.GetPath( Graph.getNode( "a" ), Graph.getNode( "f" ) );
			  assertEquals( 6, path.Count );
			  assertEquals( path[0], Graph.getNode( "a" ) );
			  assertEquals( path[1], Graph.getNode( "b" ) );
			  assertEquals( path[2], Graph.getNode( "c" ) );
			  assertEquals( path[3], Graph.getNode( "d" ) );
			  assertEquals( path[4], Graph.getNode( "e" ) );
			  assertEquals( path[5], Graph.getNode( "f" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDirection()
		 public virtual void TestDirection()
		 {
			  Graph.makeEdge( "a", "b" );
			  Graph.makeEdge( "b", "c" );
			  Graph.makeEdge( "c", "d" );
			  Graph.makeEdge( "d", "a" );
			  Graph.makeEdge( "s", "a" );
			  Graph.makeEdge( "b", "s" );
			  Graph.makeEdge( "e", "c" );
			  Graph.makeEdge( "d", "e" );
			  (new FloydWarshall<>(0.0, double.MaxValue, Direction.OUTGOING, (relationship, direction) =>
			  {
			  assertEquals( Direction.OUTGOING, direction );
			  return 1.0;
			  }, new Neo4Net.Graphalgo.impl.util.DoubleAdder(), double?.compareTo, Graph.AllNodes, Graph.AllEdges)).calculate();
			  (new FloydWarshall<>(0.0, double.MaxValue, Direction.INCOMING, (relationship, direction) =>
			  {
			  assertEquals( Direction.INCOMING, direction );
			  return 1.0;
			  }, new Neo4Net.Graphalgo.impl.util.DoubleAdder(), double?.compareTo, Graph.AllNodes, Graph.AllEdges)).calculate();
		 }
	}

}