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

	using Org.Neo4j.Graphalgo;
	using Org.Neo4j.Graphalgo.impl.shortestpath;
	using Direction = Org.Neo4j.Graphdb.Direction;
	using Relationship = Org.Neo4j.Graphdb.Relationship;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	/// <summary>
	/// This set of tests is mainly made to test the "backwards" argument to the
	/// CostEvaluator sent to a Dijkstra.
	/// @author Patrik Larsson </summary>
	/// <seealso cref= CostEvaluator </seealso>
	public class DijkstraDirectionTest : Neo4jAlgoTestCase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDijkstraDirection1()
		 public virtual void TestDijkstraDirection1()
		 {
			  Graph.makeEdge( "s", "e" );
			  Dijkstra<double> dijkstra = new Dijkstra<double>((double) 0, Graph.getNode("s"), Graph.getNode("e"), (relationship, direction) =>
			  {
			  assertEquals( Direction.OUTGOING, direction );
			  return 1.0;
			  }, new Org.Neo4j.Graphalgo.impl.util.DoubleAdder(), double?.compareTo, Direction.OUTGOING, MyRelTypes.R1);
			  dijkstra.Cost;
			  dijkstra = new Dijkstra<double>((double) 0, Graph.getNode("s"), Graph.getNode("e"), (relationship, direction) =>
			  {
			  assertEquals( Direction.INCOMING, direction );
			  return 1.0;
			  }, new Org.Neo4j.Graphalgo.impl.util.DoubleAdder(), double?.compareTo, Direction.INCOMING, MyRelTypes.R1);
			  dijkstra.Cost;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDijkstraDirection2()
		 public virtual void TestDijkstraDirection2()
		 {
			  Graph.makeEdge( "a", "b" );
			  Graph.makeEdge( "b", "c" );
			  Graph.makeEdge( "c", "d" );
			  Graph.makeEdge( "d", "a" );
			  Graph.makeEdge( "s", "a" );
			  Graph.makeEdge( "b", "s" );
			  Graph.makeEdge( "e", "c" );
			  Graph.makeEdge( "d", "e" );
			  Dijkstra<double> dijkstra = new Dijkstra<double>((double) 0, Graph.getNode("s"), Graph.getNode("e"), (relationship, direction) =>
			  {
			  assertEquals( Direction.OUTGOING, direction );
			  return 1.0;
			  }, new Org.Neo4j.Graphalgo.impl.util.DoubleAdder(), double?.compareTo, Direction.OUTGOING, MyRelTypes.R1);
			  dijkstra.Cost;
			  dijkstra = new Dijkstra<double>((double) 0, Graph.getNode("s"), Graph.getNode("e"), (relationship, direction) =>
			  {
			  assertEquals( Direction.INCOMING, direction );
			  return 1.0;
			  }, new Org.Neo4j.Graphalgo.impl.util.DoubleAdder(), double?.compareTo, Direction.INCOMING, MyRelTypes.R1);
			  dijkstra.Cost;
		 }

		 // This saves the first direction observed
		 internal class directionSavingCostEvaluator : CostEvaluator<double>
		 {
			 private readonly DijkstraDirectionTest _outerInstance;

			  internal Dictionary<Relationship, Direction> Dirs;

			  internal directionSavingCostEvaluator( DijkstraDirectionTest outerInstance, Dictionary<Relationship, Direction> dirs ) : base()
			  {
				  this._outerInstance = outerInstance;
					this.Dirs = dirs;
			  }

			  public override double? GetCost( Relationship relationship, Direction direction )
			  {
					if ( !Dirs.ContainsKey( relationship ) )
					{
						 Dirs[relationship] = direction;
					}
					return 1.0;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDijkstraDirection3()
		 public virtual void TestDijkstraDirection3()
		 {
			  Relationship r1 = Graph.makeEdge( "start", "b" );
			  Relationship r2 = Graph.makeEdge( "c", "b" );
			  Relationship r3 = Graph.makeEdge( "c", "d" );
			  Relationship r4 = Graph.makeEdge( "e", "d" );
			  Relationship r5 = Graph.makeEdge( "e", "f" );
			  Relationship r6 = Graph.makeEdge( "g", "f" );
			  Relationship r7 = Graph.makeEdge( "g", "end" );
			  Dictionary<Relationship, Direction> dirs = new Dictionary<Relationship, Direction>();
			  Dijkstra<double> dijkstra = new Dijkstra<double>( ( double ) 0, Graph.getNode( "start" ), Graph.getNode( "end" ), new directionSavingCostEvaluator( this, dirs ), new Org.Neo4j.Graphalgo.impl.util.DoubleAdder(), double?.compareTo, Direction.BOTH, MyRelTypes.R1 );
			  dijkstra.Cost;
			  assertEquals( Direction.OUTGOING, dirs[r1] );
			  assertEquals( Direction.INCOMING, dirs[r2] );
			  assertEquals( Direction.OUTGOING, dirs[r3] );
			  assertEquals( Direction.INCOMING, dirs[r4] );
			  assertEquals( Direction.OUTGOING, dirs[r5] );
			  assertEquals( Direction.INCOMING, dirs[r6] );
			  assertEquals( Direction.OUTGOING, dirs[r7] );
		 }
	}

}