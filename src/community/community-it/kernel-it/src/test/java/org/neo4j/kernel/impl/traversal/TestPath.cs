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
namespace Neo4Net.Kernel.impl.traversal
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using Direction = Neo4Net.GraphDb.Direction;
	using Node = Neo4Net.GraphDb.Node;
	using Path = Neo4Net.GraphDb.Path;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Neo4Net.GraphDb;
	using Neo4Net.GraphDb;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using BidirectionalTraversalDescription = Neo4Net.GraphDb.Traversal.BidirectionalTraversalDescription;
	using TraversalDescription = Neo4Net.GraphDb.Traversal.TraversalDescription;
	using Traverser = Neo4Net.GraphDb.Traversal.Traverser;
	using Uniqueness = Neo4Net.GraphDb.Traversal.Uniqueness;
	using Iterators = Neo4Net.Collections.Helpers.Iterators;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.traversal.Evaluators.atDepth;

	public class TestPath : TraversalTestBase
	{
		 private static Node _a;
		 private static Node _b;
		 private static Node _c;
		 private static Node _d;
		 private static Node _e;
		 private Transaction _tx;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  CreateGraph( "A TO B", "B TO C", "C TO D", "D TO E" );

			  _tx = BeginTx();

			  _a = GetNodeWithName( "A" );
			  _b = GetNodeWithName( "B" );
			  _c = GetNodeWithName( "C" );
			  _d = GetNodeWithName( "D" );
			  _e = GetNodeWithName( "E" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _tx.close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testPathIterator()
		 public virtual void TestPathIterator()
		 {
			  Traverser traverse = GraphDb.traversalDescription().evaluator(atDepth(4)).traverse(Node("A"));
			  using ( IResourceIterator<Path> resourceIterator = traverse.GetEnumerator() )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					Path path = resourceIterator.next();
					AssertPathIsCorrect( path );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void reverseNodes()
		 public virtual void ReverseNodes()
		 {
			  Traverser traverse = GraphDb.traversalDescription().evaluator(atDepth(0)).traverse(_a);
			  Path path = GetFirstPath( traverse );
			  AssertContains( path.ReverseNodes(), _a );

			  Traverser traverse2 = GraphDb.traversalDescription().evaluator(atDepth(4)).traverse(_a);
			  Path path2 = GetFirstPath( traverse2 );
			  AssertContainsInOrder( path2.ReverseNodes(), _e, _d, _c, _b, _a );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void reverseRelationships()
		 public virtual void ReverseRelationships()
		 {
			  Traverser traverser = GraphDb.traversalDescription().evaluator(atDepth(0)).traverse(_a);
			  Path path = GetFirstPath( traverser );
			  assertFalse( path.ReverseRelationships().GetEnumerator().hasNext() );

			  Traverser traverser2 = GraphDb.traversalDescription().evaluator(atDepth(4)).traverse(_a);
			  Path path2 = GetFirstPath( traverser2 );
			  Node[] expectedNodes = new Node[]{ _e, _d, _c, _b, _a };
			  int index = 0;
			  foreach ( Relationship rel in path2.ReverseRelationships() )
			  {
					assertEquals( "For index " + index, expectedNodes[index++], rel.EndNode );
			  }
			  assertEquals( 4, index );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBidirectionalPath()
		 public virtual void TestBidirectionalPath()
		 {
			  TraversalDescription side = GraphDb.traversalDescription().uniqueness(Uniqueness.NODE_PATH);
			  BidirectionalTraversalDescription bidirectional = GraphDb.bidirectionalTraversalDescription().mirroredSides(side);
			  Path bidirectionalPath = GetFirstPath( bidirectional.Traverse( _a, _e ) );
			  AssertPathIsCorrect( bidirectionalPath );

			  Path path = GetFirstPath( bidirectional.Traverse( _a, _e ) );
			  Node node = path.StartNode();
			  assertEquals( _a, node );

			  // White box testing below: relationships(), nodes(), reverseRelationships(), reverseNodes()
			  // does cache the start node if not already cached, so just make sure they to it properly.
			  bidirectionalPath = GetFirstPath( bidirectional.Traverse( _a, _e ) );
			  bidirectionalPath.Relationships();
			  assertEquals( _a, bidirectionalPath.StartNode() );

			  bidirectionalPath = GetFirstPath( bidirectional.Traverse( _a,_e ) );
			  bidirectionalPath.Nodes();
			  assertEquals( _a, bidirectionalPath.StartNode() );

			  bidirectionalPath = GetFirstPath( bidirectional.Traverse( _a, _e ) );
			  bidirectionalPath.ReverseRelationships();
			  assertEquals( _a, bidirectionalPath.StartNode() );

			  bidirectionalPath = GetFirstPath( bidirectional.Traverse( _a, _e ) );
			  bidirectionalPath.ReverseNodes();
			  assertEquals( _a, bidirectionalPath.StartNode() );

			  bidirectionalPath = GetFirstPath( bidirectional.Traverse( _a, _e ) );
			  bidirectionalPath.GetEnumerator();
			  assertEquals( _a, bidirectionalPath.StartNode() );
		 }

		 private Path GetFirstPath( Traverser traverse )
		 {
			  using ( IResourceIterator<Path> iterator = traverse.GetEnumerator() )
			  {
					return Iterators.first( iterator );
			  }
		 }

		 private void AssertPathIsCorrect( Path path )
		 {
			  Node a = Node( "A" );
			  Relationship to1 = GetFistRelationship( a );
			  Node b = to1.EndNode;
			  Relationship to2 = GetFistRelationship( b );
			  Node c = to2.EndNode;
			  Relationship to3 = GetFistRelationship( c );
			  Node d = to3.EndNode;
			  Relationship to4 = GetFistRelationship( d );
			  Node e = to4.EndNode;
			  assertEquals( ( int? ) 4, ( int? ) path.Length() );
			  assertEquals( a, path.StartNode() );
			  assertEquals( e, path.EndNode() );
			  assertEquals( to4, path.LastRelationship() );

			  AssertContainsInOrder( path, a, to1, b, to2, c, to3, d, to4, e );
			  AssertContainsInOrder( path.Nodes(), a, b, c, d, e );
			  AssertContainsInOrder( path.Relationships(), to1, to2, to3, to4 );
			  AssertContainsInOrder( path.ReverseNodes(), e, d, c, b, a );
			  AssertContainsInOrder( path.ReverseRelationships(), to4, to3, to2, to1 );
		 }

		 private Relationship GetFistRelationship( Node node )
		 {
			 IResourceIterable<Relationship> relationships = (IResourceIterable<Relationship> ) node.GetRelationships( Direction.OUTGOING );
			  using ( IResourceIterator<Relationship> iterator = relationships.GetEnumerator() )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					return iterator.next();
			  }
		 }
	}

}