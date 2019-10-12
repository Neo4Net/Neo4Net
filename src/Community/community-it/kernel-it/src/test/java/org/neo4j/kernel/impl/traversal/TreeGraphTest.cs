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
namespace Neo4Net.Kernel.impl.traversal
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using Node = Neo4Net.Graphdb.Node;
	using Path = Neo4Net.Graphdb.Path;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Traverser = Neo4Net.Graphdb.traversal.Traverser;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.traversal.BranchOrderingPolicies.POSTORDER_BREADTH_FIRST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.traversal.BranchOrderingPolicies.POSTORDER_DEPTH_FIRST;

	public class TreeGraphTest : TraversalTestBase
	{
		 /*
		  *                     (1)
		  *               ------ | ------
		  *             /        |        \
		  *           (2)       (3)       (4)
		  *          / | \     / | \     / | \
		  *        (5)(6)(7) (8)(9)(A) (B)(C)(D)
		  */
		 private static readonly string[] _theWorldAsWeKnowsIt = new string[] { "1 TO 2", "1 TO 3", "1 TO 4", "2 TO 5", "2 TO 6", "2 TO 7", "3 TO 8", "3 TO 9", "3 TO A", "4 TO B", "4 TO C", "4 TO D" };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setupGraph()
		 public virtual void SetupGraph()
		 {
			  CreateGraph( _theWorldAsWeKnowsIt );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nodesIteratorReturnAllNodes()
		 public virtual void NodesIteratorReturnAllNodes()
		 {
			  using ( Transaction transaction = BeginTx() )
			  {
					Traverser traverser = GraphDb.traversalDescription().traverse(Node("1"));
					int count = 0;
					foreach ( Node node in traverser.Nodes() )
					{
						 assertNotNull( "returned nodes should not be null. node #" + count, node );
						 count++;
					}
					assertEquals( 13, count );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void relationshipsIteratorReturnAllNodes()
		 public virtual void RelationshipsIteratorReturnAllNodes()
		 {
			  using ( Transaction transaction = BeginTx() )
			  {
					Traverser traverser = GraphDb.traversalDescription().traverse(Node("1"));
					int count = 0;
					foreach ( Relationship relationship in traverser.Relationships() )
					{
						 assertNotNull( "returned relationships should not be. relationship #" + count, relationship );
						 count++;
					}
					assertEquals( 12, count );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pathsIteratorReturnAllNodes()
		 public virtual void PathsIteratorReturnAllNodes()
		 {

			  using ( Transaction transaction = BeginTx() )
			  {
					Traverser traverser = GraphDb.traversalDescription().traverse(Node("1"));
					int count = 0;
					foreach ( Path path in traverser )
					{
						 assertNotNull( "returned paths should not be null. path #" + count, path );
						 count++;
					}
					assertEquals( 13, count );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBreadthFirst()
		 public virtual void TestBreadthFirst()
		 {
			  Traverser traverser = GraphDb.traversalDescription().breadthFirst().traverse(Node("1"));
			  Stack<ISet<string>> levels = new Stack<ISet<string>>();
			  levels.Push( new HashSet<>( asList( "5", "6", "7", "8", "9", "A", "B", "C", "D" ) ) );
			  levels.Push( new HashSet<>( asList( "2", "3", "4" ) ) );
			  levels.Push( new HashSet<>( asList( "1" ) ) );

			  using ( Transaction tx = BeginTx() )
			  {
					AssertLevels( traverser, levels );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDepthFirstTraversalReturnsNodesOnCorrectDepths()
		 public virtual void TestDepthFirstTraversalReturnsNodesOnCorrectDepths()
		 {

			  using ( Transaction transaction = BeginTx() )
			  {
					Traverser traverser = GraphDb.traversalDescription().depthFirst().traverse(Node("1"));
					int i = 0;
					foreach ( Path pos in traverser )
					{
						 assertEquals( ExpectedDepth( i++ ), pos.Length() );
					}
					assertEquals( 13, i );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testPostorderDepthFirstReturnsDeeperNodesFirst()
		 public virtual void TestPostorderDepthFirstReturnsDeeperNodesFirst()
		 {
			  Traverser traverser = GraphDb.traversalDescription().order(POSTORDER_DEPTH_FIRST).traverse(Node("1"));
			  int i = 0;
			  IList<string> encounteredNodes = new List<string>();
			  using ( Transaction tx = BeginTx() )
			  {
					foreach ( Path pos in traverser )
					{
						 encounteredNodes.Add( ( string ) pos.EndNode().getProperty("name") );
						 assertEquals( ExpectedDepth( 12 - i++ ), pos.Length() );
					}
					tx.Success();
			  }
			  assertEquals( 13, i );

			  assertTrue( encounteredNodes.IndexOf( "5" ) < encounteredNodes.IndexOf( "2" ) );
			  assertTrue( encounteredNodes.IndexOf( "6" ) < encounteredNodes.IndexOf( "2" ) );
			  assertTrue( encounteredNodes.IndexOf( "7" ) < encounteredNodes.IndexOf( "2" ) );
			  assertTrue( encounteredNodes.IndexOf( "8" ) < encounteredNodes.IndexOf( "3" ) );
			  assertTrue( encounteredNodes.IndexOf( "9" ) < encounteredNodes.IndexOf( "3" ) );
			  assertTrue( encounteredNodes.IndexOf( "A" ) < encounteredNodes.IndexOf( "3" ) );
			  assertTrue( encounteredNodes.IndexOf( "B" ) < encounteredNodes.IndexOf( "4" ) );
			  assertTrue( encounteredNodes.IndexOf( "C" ) < encounteredNodes.IndexOf( "4" ) );
			  assertTrue( encounteredNodes.IndexOf( "D" ) < encounteredNodes.IndexOf( "4" ) );
			  assertTrue( encounteredNodes.IndexOf( "2" ) < encounteredNodes.IndexOf( "1" ) );
			  assertTrue( encounteredNodes.IndexOf( "3" ) < encounteredNodes.IndexOf( "1" ) );
			  assertTrue( encounteredNodes.IndexOf( "4" ) < encounteredNodes.IndexOf( "1" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testPostorderBreadthFirstReturnsDeeperNodesFirst()
		 public virtual void TestPostorderBreadthFirstReturnsDeeperNodesFirst()
		 {
			  Traverser traverser = GraphDb.traversalDescription().order(POSTORDER_BREADTH_FIRST).traverse(Node("1"));
			  Stack<ISet<string>> levels = new Stack<ISet<string>>();
			  levels.Push( new HashSet<>( asList( "1" ) ) );
			  levels.Push( new HashSet<>( asList( "2", "3", "4" ) ) );
			  levels.Push( new HashSet<>( asList( "5", "6", "7", "8", "9", "A", "B", "C", "D" ) ) );
			  using ( Transaction tx = BeginTx() )
			  {
					AssertLevels( traverser, levels );
					tx.Success();
			  }
		 }

		 private int ExpectedDepth( int i )
		 {
			  assertTrue( i < 13 );
			  if ( i == 0 )
			  {
					return 0;
			  }
			  else if ( ( i - 1 ) % 4 == 0 )
			  {
					return 1;
			  }
			  else
			  {
					return 2;
			  }
		 }
	}

}