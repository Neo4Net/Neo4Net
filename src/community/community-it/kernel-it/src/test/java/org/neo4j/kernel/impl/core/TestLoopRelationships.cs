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
namespace Neo4Net.Kernel.impl.core
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using ConstraintViolationException = Neo4Net.GraphDb.ConstraintViolationException;
	using Direction = Neo4Net.GraphDb.Direction;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using NotFoundException = Neo4Net.GraphDb.NotFoundException;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Neo4Net.Helpers.Collections;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.MyRelTypes.TEST;

	public class TestLoopRelationships : AbstractNeo4NetTestCase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canCreateRelationshipBetweenTwoNodesWithLoopsThenDeleteOneOfTheNodesAndItsRelationships()
		 public virtual void CanCreateRelationshipBetweenTwoNodesWithLoopsThenDeleteOneOfTheNodesAndItsRelationships()
		 {
			  Node source = GraphDb.createNode();
			  Node target = GraphDb.createNode();
			  source.CreateRelationshipTo( source, TEST );
			  target.CreateRelationshipTo( target, TEST );
			  source.CreateRelationshipTo( target, TEST );

			  NewTransaction();

			  foreach ( Relationship rel in target.Relationships )
			  {
					rel.Delete();
			  }
			  target.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canDeleteNodeAfterDeletingItsRelationshipsIfThoseRelationshipsIncludeLoops()
		 public virtual void CanDeleteNodeAfterDeletingItsRelationshipsIfThoseRelationshipsIncludeLoops()
		 {
			  Node node = GraphDb.createNode();

			  TxCreateLoop( node );
			  TxCreateRel( node );
			  TxCreateLoop( node );

			  foreach ( Relationship rel in node.Relationships )
			  {
					rel.Delete();
			  }
			  node.Delete();

			  Commit();
		 }

		 private void TxCreateRel( Node node )
		 {
			  node.CreateRelationshipTo( GraphDb.createNode(), TEST );
			  NewTransaction();
		 }

		 private void TxCreateLoop( Node node )
		 {
			  node.CreateRelationshipTo( node, TEST );
			  NewTransaction();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canAddLoopRelationship()
		 public virtual void CanAddLoopRelationship()
		 {
			  Node node = GraphDb.createNode();
			  node.CreateRelationshipTo( node, TEST );

			  NewTransaction();

			  foreach ( Direction dir in Direction.values() )
			  {
					int count = 0;
					foreach ( Relationship rel in node.GetRelationships( dir ) )
					{
						 count++;
						 assertEquals( "start node", node, rel.StartNode );
						 assertEquals( "end node", node, rel.EndNode );
						 assertEquals( "other node", node, rel.GetOtherNode( node ) );
					}
					assertEquals( dir.name() + " relationship count", 1, count );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canAddManyLoopRelationships()
		 public virtual void CanAddManyLoopRelationships()
		 {
			  TestAddManyLoopRelationships( 2 );
			  TestAddManyLoopRelationships( 3 );
			  TestAddManyLoopRelationships( 5 );
		 }

		 private void TestAddManyLoopRelationships( int count )
		 {
			  foreach ( bool[] loop in Permutations( count ) )
			  {
					Node root = GraphDb.createNode();
					Relationship[] relationships = new Relationship[count];
					for ( int i = 0; i < count; i++ )
					{
						 if ( loop[i] )
						 {
							  relationships[i] = root.CreateRelationshipTo( root, TEST );
						 }
						 else
						 {
							  relationships[i] = root.CreateRelationshipTo( GraphDb.createNode(), TEST );
						 }
					}
					NewTransaction();
					verifyRelationships( Arrays.ToString( loop ), root, loop, relationships );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canAddLoopRelationshipAndOtherRelationships()
		 public virtual void CanAddLoopRelationshipAndOtherRelationships()
		 {
			  TestAddLoopRelationshipAndOtherRelationships( 2 );
			  TestAddLoopRelationshipAndOtherRelationships( 3 );
			  TestAddLoopRelationshipAndOtherRelationships( 5 );
		 }

		 private void TestAddLoopRelationshipAndOtherRelationships( int size )
		 {
			  for ( int i = 0; i < size; i++ )
			  {
					Node root = GraphDb.createNode();
					Relationship[] relationships = CreateRelationships( size, i, root );
					verifyRelationships( string.Format( "loop on {0} of {1}", i, size ), root, i, relationships );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canAddAndRemoveLoopRelationshipAndOtherRelationships()
		 public virtual void CanAddAndRemoveLoopRelationshipAndOtherRelationships()
		 {
			  TestAddAndRemoveLoopRelationshipAndOtherRelationships( 2 );
			  TestAddAndRemoveLoopRelationshipAndOtherRelationships( 3 );
			  TestAddAndRemoveLoopRelationshipAndOtherRelationships( 5 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getSingleRelationshipOnNodeWithOneLoopOnly()
		 public virtual void getSingleRelationshipOnNodeWithOneLoopOnly()
		 {
			  Node node = GraphDb.createNode();
			  Relationship singleRelationship = node.CreateRelationshipTo( node, TEST );
			  assertEquals( singleRelationship, node.GetSingleRelationship( TEST, Direction.OUTGOING ) );
			  assertEquals( singleRelationship, node.GetSingleRelationship( TEST, Direction.INCOMING ) );
			  assertEquals( singleRelationship, node.GetSingleRelationship( TEST, Direction.BOTH ) );
			  Commit();

			  NewTransaction();
			  assertEquals( singleRelationship, node.GetSingleRelationship( TEST, Direction.OUTGOING ) );
			  assertEquals( singleRelationship, node.GetSingleRelationship( TEST, Direction.INCOMING ) );
			  assertEquals( singleRelationship, node.GetSingleRelationship( TEST, Direction.BOTH ) );
			  Finish();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void cannotDeleteNodeWithLoopStillAttached()
		 public virtual void CannotDeleteNodeWithLoopStillAttached()
		 {
			  // Given
			  IGraphDatabaseService db = GraphDb;
			  Node node;
			  using ( Transaction tx = Db.beginTx() )
			  {
					node = Db.createNode();
					node.CreateRelationshipTo( node, RelationshipType.withName( "MAYOR_OF" ) );
					tx.Success();
			  }

			  // And given a transaction deleting just the node
			  Transaction tx = NewTransaction();
			  node.Delete();
			  tx.Success();

			  // Expect
			  Exception.expect( typeof( ConstraintViolationException ) );
			  Exception.expectMessage( "Cannot delete node<" + node.Id + ">, because it still has relationships. " + "To delete this node, you must first delete its relationships." );

			  // When I commit
			  tx.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getOtherNodeFunctionsCorrectly()
		 public virtual void getOtherNodeFunctionsCorrectly()
		 {
			  Node node = GraphDb.createNode();
			  Relationship relationship = node.CreateRelationshipTo( node, TEST );

			  // This loop messes up the readability of the test case, but avoids duplicated
			  // assertion code. Same assertions withing the transaction as after it has committed.
			  for ( int i = 0; i < 2; i++ )
			  {
					assertEquals( node, relationship.GetOtherNode( node ) );
					assertEquals( asList( node, node ), asList( relationship.Nodes ) );
					try
					{
						 relationship.GetOtherNode( GraphDb.createNode() );
						 fail( "Should throw exception if another node is passed into loop.getOtherNode" );
					}
					catch ( NotFoundException )
					{ // Good
					}
					NewTransaction();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getNewlyCreatedLoopRelationshipFromCache()
		 public virtual void getNewlyCreatedLoopRelationshipFromCache()
		 {
			  Node node = GraphDb.createNode();
			  node.CreateRelationshipTo( GraphDb.createNode(), TEST );
			  NewTransaction();
			  Relationship relationship = node.CreateRelationshipTo( node, TEST );
			  NewTransaction();
			  assertEquals( relationship, node.GetSingleRelationship( TEST, Direction.INCOMING ) );
		 }

		 private void TestAddAndRemoveLoopRelationshipAndOtherRelationships( int size )
		 {
			  foreach ( bool[] delete in Permutations( size ) )
			  {
					for ( int i = 0; i < size; i++ )
					{
						 Node root = GraphDb.createNode();
						 Relationship[] relationships = CreateRelationships( size, i, root );
						 for ( int j = 0; j < size; j++ )
						 {
							  if ( delete[j] )
							  {
									relationships[j].Delete();
									relationships[j] = null;
							  }
							  NewTransaction();
						 }
						 verifyRelationships( string.Format( "loop on {0} of {1}, delete {2}", i, size, Arrays.ToString( delete ) ), root, i, relationships );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static Iterable<boolean[]> permutations(final int size)
		 private static IEnumerable<bool[]> Permutations( int size )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int max = 1 << size;
			  int max = 1 << size;
			  return () => new PrefetchingIteratorAnonymousInnerClass(size, max);
		 }

		 private class PrefetchingIteratorAnonymousInnerClass : PrefetchingIterator<bool[]>
		 {
			 private int _size;
			 private int _max;

			 public PrefetchingIteratorAnonymousInnerClass( int size, int max )
			 {
				 this._size = size;
				 this._max = max;
			 }

			 internal int pos;

			 protected internal override bool[] fetchNextOrNull()
			 {
				  if ( pos < _max )
				  {
						int cur = pos++;
						bool[] result = new bool[_size];
						for ( int i = 0; i < _size; i++ )
						{
							 result[i] = ( cur & 1 ) == 1;
							 cur >>= 1;
						}
						return result;
				  }
				  return null;
			 }
		 }

		 private Relationship[] CreateRelationships( int count, int loop, Node root )
		 {
			  Node[] nodes = new Node[count];
			  for ( int i = 0; i < count; i++ )
			  {
					if ( loop == i )
					{
						 nodes[i] = root;
					}
					else
					{
						 nodes[i] = GraphDb.createNode();
					}
			  }

			  NewTransaction();

			  Relationship[] relationships = new Relationship[count];
			  for ( int i = 0; i < count; i++ )
			  {
					relationships[i] = root.CreateRelationshipTo( nodes[i], TEST );
					NewTransaction();
			  }
			  return relationships;
		 }

		 private void VerifyRelationships( string message, Node root, int loop, params Relationship[] relationships )
		 {
			  bool[] loops = new bool[relationships.Length];
			  for ( int i = 0; i < relationships.Length; i++ )
			  {
					loops[i] = i == loop;
			  }
			  verifyRelationships( message, root, loops, relationships );
		 }

		 private void VerifyRelationships( string message, Node root, bool[] loop, params Relationship[] relationships )
		 {
			  foreach ( Direction dir in Direction.values() )
			  {
					ISet<Relationship> expected = new HashSet<Relationship>();
					for ( int i = 0; i < relationships.Length; i++ )
					{
						 if ( relationships[i] != null && ( dir != Direction.INCOMING || loop[i] ) )
						 {
							  expected.Add( relationships[i] );
						 }
					}

					foreach ( Relationship rel in root.GetRelationships( dir ) )
					{
						 assertTrue( message + ": unexpected relationship: " + rel, expected.remove( rel ) );
					}
					assertTrue( message + ": expected relationships not seen " + expected, expected.Count == 0 );
			  }
		 }
	}

}