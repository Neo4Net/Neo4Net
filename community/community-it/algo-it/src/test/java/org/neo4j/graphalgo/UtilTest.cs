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
namespace Org.Neo4j.Graphalgo
{
	using Neo4jAlgoTestCase = Common.Neo4jAlgoTestCase;
	using Test = org.junit.Test;


	using Util = Org.Neo4j.Graphalgo.impl.shortestpath.Util;
	using PathCounter = Org.Neo4j.Graphalgo.impl.shortestpath.Util.PathCounter;
	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class UtilTest : Neo4jAlgoTestCase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testPathCounter()
		 public virtual void TestPathCounter()
		 {
			  // Nodes
			  Node a = GraphDb.createNode();
			  Node b = GraphDb.createNode();
			  Node c = GraphDb.createNode();
			  Node d = GraphDb.createNode();
			  Node e = GraphDb.createNode();
			  Node f = GraphDb.createNode();
			  // Predecessor lists
			  IList<Relationship> ap = new LinkedList<Relationship>();
			  IList<Relationship> bp = new LinkedList<Relationship>();
			  IList<Relationship> cp = new LinkedList<Relationship>();
			  IList<Relationship> dp = new LinkedList<Relationship>();
			  IList<Relationship> ep = new LinkedList<Relationship>();
			  IList<Relationship> fp = new LinkedList<Relationship>();
			  // Predecessor map
			  IDictionary<Node, IList<Relationship>> predecessors = new Dictionary<Node, IList<Relationship>>();
			  predecessors[a] = ap;
			  predecessors[b] = bp;
			  predecessors[c] = cp;
			  predecessors[d] = dp;
			  predecessors[e] = ep;
			  predecessors[f] = fp;
			  // Add relations
			  fp.Add( f.CreateRelationshipTo( c, MyRelTypes.R1 ) );
			  fp.Add( f.CreateRelationshipTo( e, MyRelTypes.R1 ) );
			  ep.Add( e.CreateRelationshipTo( b, MyRelTypes.R1 ) );
			  ep.Add( e.CreateRelationshipTo( d, MyRelTypes.R1 ) );
			  dp.Add( d.CreateRelationshipTo( a, MyRelTypes.R1 ) );
			  cp.Add( c.CreateRelationshipTo( b, MyRelTypes.R1 ) );
			  bp.Add( b.CreateRelationshipTo( a, MyRelTypes.R1 ) );
			  // Count
			  Util.PathCounter counter = new Util.PathCounter( predecessors );
			  assertEquals( 1, counter.GetNumberOfPathsToNode( a ) );
			  assertEquals( 1, counter.GetNumberOfPathsToNode( b ) );
			  assertEquals( 1, counter.GetNumberOfPathsToNode( c ) );
			  assertEquals( 1, counter.GetNumberOfPathsToNode( d ) );
			  assertEquals( 2, counter.GetNumberOfPathsToNode( e ) );
			  assertEquals( 3, counter.GetNumberOfPathsToNode( f ) );
			  // Reverse
			  counter = new Util.PathCounter( Util.reversedPredecessors( predecessors ) );
			  assertEquals( 3, counter.GetNumberOfPathsToNode( a ) );
			  assertEquals( 2, counter.GetNumberOfPathsToNode( b ) );
			  assertEquals( 1, counter.GetNumberOfPathsToNode( c ) );
			  assertEquals( 1, counter.GetNumberOfPathsToNode( d ) );
			  assertEquals( 1, counter.GetNumberOfPathsToNode( e ) );
			  assertEquals( 1, counter.GetNumberOfPathsToNode( f ) );
		 }
	}

}