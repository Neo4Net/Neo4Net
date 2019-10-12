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
	using Direction = Org.Neo4j.Graphdb.Direction;
	using Node = Org.Neo4j.Graphdb.Node;
	using PropertyContainer = Org.Neo4j.Graphdb.PropertyContainer;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public abstract class SingleSourceShortestPathTest : Neo4jAlgoTestCase
	{
		 protected internal abstract SingleSourceShortestPath<int> GetSingleSourceAlgorithm( Node startNode );

		 protected internal abstract SingleSourceShortestPath<int> GetSingleSourceAlgorithm( Node startNode, Direction direction, params RelationshipType[] relTypes );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRun()
		 public virtual void TestRun()
		 {
			  // make the graph
			  Graph.makeEdgeChain( "a,b1,c1,d1,e1,f1,g1" );
			  Graph.makeEdgeChain( "a,b2,c2,d2,e2,f2,g2" );
			  Graph.makeEdgeChain( "a,b3,c3,d3,e3,f3,g3" );
			  Graph.makeEdgeChain( "b1,b2,b3,b1" );
			  Graph.makeEdgeChain( "d1,d2,d3,d1" );
			  Graph.makeEdgeChain( "f1,f2,f3,f1" );
			  // make the computation
			  SingleSourceShortestPath<int> singleSource = GetSingleSourceAlgorithm( Graph.getNode( "a" ) );
			  // check a few distances
			  assertEquals( 0, ( int ) singleSource.GetCost( Graph.getNode( "a" ) ) );
			  assertEquals( 1, ( int ) singleSource.GetCost( Graph.getNode( "b2" ) ) );
			  assertEquals( 2, ( int ) singleSource.GetCost( Graph.getNode( "c3" ) ) );
			  assertEquals( 3, ( int ) singleSource.GetCost( Graph.getNode( "d1" ) ) );
			  assertEquals( 4, ( int ) singleSource.GetCost( Graph.getNode( "e2" ) ) );
			  assertEquals( 5, ( int ) singleSource.GetCost( Graph.getNode( "f3" ) ) );
			  assertEquals( 6, ( int ) singleSource.GetCost( Graph.getNode( "g1" ) ) );
			  // check one path
			  IList<Node> path = singleSource.GetPathAsNodes( Graph.getNode( "g2" ) );
			  assertEquals( 7, path.Count );
			  assertEquals( path[0], Graph.getNode( "a" ) );
			  assertEquals( path[1], Graph.getNode( "b2" ) );
			  assertEquals( path[2], Graph.getNode( "c2" ) );
			  assertEquals( path[3], Graph.getNode( "d2" ) );
			  assertEquals( path[4], Graph.getNode( "e2" ) );
			  assertEquals( path[5], Graph.getNode( "f2" ) );
			  assertEquals( path[6], Graph.getNode( "g2" ) );
			  // check it as relationships
			  IList<Relationship> rpath = singleSource.GetPathAsRelationships( Graph.getNode( "g2" ) );
			  assertEquals( 6, rpath.Count );
			  assertEquals( rpath[0], Graph.getRelationship( "a", "b2" ) );
			  assertEquals( rpath[1], Graph.getRelationship( "b2", "c2" ) );
			  assertEquals( rpath[2], Graph.getRelationship( "c2", "d2" ) );
			  assertEquals( rpath[3], Graph.getRelationship( "d2", "e2" ) );
			  assertEquals( rpath[4], Graph.getRelationship( "e2", "f2" ) );
			  assertEquals( rpath[5], Graph.getRelationship( "f2", "g2" ) );
			  // check it as both
			  IList<PropertyContainer> cpath = singleSource.GetPath( Graph.getNode( "g2" ) );
			  assertEquals( 13, cpath.Count );
			  assertEquals( cpath[0], Graph.getNode( "a" ) );
			  assertEquals( cpath[2], Graph.getNode( "b2" ) );
			  assertEquals( cpath[4], Graph.getNode( "c2" ) );
			  assertEquals( cpath[6], Graph.getNode( "d2" ) );
			  assertEquals( cpath[8], Graph.getNode( "e2" ) );
			  assertEquals( cpath[10], Graph.getNode( "f2" ) );
			  assertEquals( cpath[12], Graph.getNode( "g2" ) );
			  assertEquals( cpath[1], Graph.getRelationship( "a", "b2" ) );
			  assertEquals( cpath[3], Graph.getRelationship( "b2", "c2" ) );
			  assertEquals( cpath[5], Graph.getRelationship( "c2", "d2" ) );
			  assertEquals( cpath[7], Graph.getRelationship( "d2", "e2" ) );
			  assertEquals( cpath[9], Graph.getRelationship( "e2", "f2" ) );
			  assertEquals( cpath[11], Graph.getRelationship( "f2", "g2" ) );
			  Graph.clear();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMultipleRelTypes()
		 public virtual void TestMultipleRelTypes()
		 {
			  Graph.CurrentRelType = MyRelTypes.R1;
			  Graph.makeEdgeChain( "a,b,c,d,e" );
			  Graph.CurrentRelType = MyRelTypes.R2;
			  Graph.makeEdges( "a,c" ); // first shortcut
			  Graph.CurrentRelType = MyRelTypes.R3;
			  Graph.makeEdges( "c,e" ); // second shortcut
			  SingleSourceShortestPath<int> singleSource;
			  // one path
			  singleSource = GetSingleSourceAlgorithm( Graph.getNode( "a" ), Direction.BOTH, MyRelTypes.R1 );
			  assertEquals( 4, ( int ) singleSource.GetCost( Graph.getNode( "e" ) ) );
			  // one shortcut
			  singleSource = GetSingleSourceAlgorithm( Graph.getNode( "a" ), Direction.BOTH, MyRelTypes.R1, MyRelTypes.R2 );
			  assertEquals( 3, ( int ) singleSource.GetCost( Graph.getNode( "e" ) ) );
			  // other shortcut
			  singleSource = GetSingleSourceAlgorithm( Graph.getNode( "a" ), Direction.BOTH, MyRelTypes.R1, MyRelTypes.R3 );
			  assertEquals( 3, ( int ) singleSource.GetCost( Graph.getNode( "e" ) ) );
			  // both shortcuts
			  singleSource = GetSingleSourceAlgorithm( Graph.getNode( "a" ), Direction.BOTH, MyRelTypes.R1, MyRelTypes.R2, MyRelTypes.R3 );
			  assertEquals( 2, ( int ) singleSource.GetCost( Graph.getNode( "e" ) ) );
		 }
	}

}