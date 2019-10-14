using System;

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
	using Test = org.junit.Test;

	using Node = Neo4Net.Graphdb.Node;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Neo4Net.Graphdb;
	using Neo4Net.Graphdb;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asResourceIterator;

	public class TestNeo4j : AbstractNeo4jTestCase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testBasicNodeRelationships()
		 public virtual void TestBasicNodeRelationships()
		 {
			  Node firstNode;
			  Node secondNode;
			  Relationship rel;
			  // Create nodes and a relationship between them
			  firstNode = GraphDb.createNode();
			  assertNotNull( "Failure creating first node", firstNode );
			  secondNode = GraphDb.createNode();
			  assertNotNull( "Failure creating second node", secondNode );
			  rel = firstNode.CreateRelationshipTo( secondNode, MyRelTypes.TEST );
			  assertNotNull( "Relationship is null", rel );
			  RelationshipType relType = rel.Type;
			  assertNotNull( "Relationship's type is is null", relType );

			  // Verify that the node reports that it has a relationship of
			  // the type we created above
			  using ( ResourceIterator<Relationship> iterator = asResourceIterator( firstNode.GetRelationships( relType ).GetEnumerator() ) )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( iterator.hasNext() );
			  }
			  using ( ResourceIterator<Relationship> iterator = asResourceIterator( secondNode.GetRelationships( relType ).GetEnumerator() ) )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( iterator.hasNext() );
			  }

			  ResourceIterable<Relationship> allRels;

			  // Verify that both nodes return the relationship we created above
			  allRels = ( ResourceIterable<Relationship> ) firstNode.Relationships;
			  assertTrue( this.ObjectExistsInIterable( rel, allRels ) );
			  allRels = ( ResourceIterable<Relationship> ) firstNode.GetRelationships( relType );
			  assertTrue( this.ObjectExistsInIterable( rel, allRels ) );

			  allRels = ( ResourceIterable<Relationship> ) secondNode.Relationships;
			  assertTrue( this.ObjectExistsInIterable( rel, allRels ) );
			  allRels = ( ResourceIterable<Relationship> ) secondNode.GetRelationships( relType );
			  assertTrue( this.ObjectExistsInIterable( rel, allRels ) );

			  // Verify that the relationship reports that it is associated with
			  // firstNode and secondNode
			  Node[] relNodes = rel.Nodes;
			  assertEquals( "A relationship should always be connected to exactly " + "two nodes", relNodes.Length, 2 );
			  assertTrue( "Relationship says that it isn't connected to firstNode", this.ObjectExistsInArray( firstNode, relNodes ) );
			  assertTrue( "Relationship says that it isn't connected to secondNode", this.ObjectExistsInArray( secondNode, relNodes ) );
			  assertEquals( "The other node should be secondNode but it isn't", rel.GetOtherNode( firstNode ), secondNode );
			  assertEquals( "The other node should be firstNode but it isn't", rel.GetOtherNode( secondNode ), firstNode );
			  rel.Delete();
			  secondNode.Delete();
			  firstNode.Delete();
		 }

		 private bool ObjectExistsInIterable( Relationship rel, ResourceIterable<Relationship> allRels )
		 {
			  using ( ResourceIterator<Relationship> resourceIterator = allRels.GetEnumerator() )
			  {
					while ( resourceIterator.MoveNext() )
					{
						 Relationship iteratedRel = resourceIterator.Current;
						 {
							  if ( rel.Equals( iteratedRel ) )
							  {
									return true;
							  }
						 }
					}
					return false;
			  }
		 }

		 private bool ObjectExistsInArray( object obj, object[] objArray )
		 {
			  foreach ( object o in objArray )
			  {
					if ( o.Equals( obj ) )
					{
						 return true;
					}
			  }
			  return false;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRandomPropertyName()
		 public virtual void TestRandomPropertyName()
		 {
			  Node node1 = GraphDb.createNode();
			  string key = "random_"
					+ ( new Random( DateTimeHelper.CurrentUnixTimeMillis() ) ).nextLong();
			  node1.SetProperty( key, "value" );
			  assertEquals( "value", node1.GetProperty( key ) );
			  node1.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodeChangePropertyArray()
		 public virtual void TestNodeChangePropertyArray()
		 {
			  Transaction.close();

			  Node node;
			  using ( Transaction tx = GraphDb.beginTx() )
			  {
					node = GraphDb.createNode();
					tx.Success();
			  }

			  using ( Transaction tx = GraphDb.beginTx() )
			  {
					node.SetProperty( "test", new string[] { "value1" } );
					tx.Success();
			  }

			  using ( Transaction ignored = GraphDb.beginTx() )
			  {
					node.SetProperty( "test", new string[] { "value1", "value2" } );
					// no success, we wanna test rollback on this operation
			  }

			  using ( Transaction tx = GraphDb.beginTx() )
			  {
					string[] value = ( string[] ) node.GetProperty( "test" );
					assertEquals( 1, value.Length );
					assertEquals( "value1", value[0] );
					tx.Success();
			  }

			  Transaction = GraphDb.beginTx();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGetAllNodes()
		 public virtual void TestGetAllNodes()
		 {
			  long highId = GetIdGenerator( IdType.NODE ).HighestPossibleIdInUse;
			  if ( highId >= 0 && highId < 10000 )
			  {
					long count = Iterables.count( GraphDb.AllNodes );
					bool found = false;
					Node newNode = GraphDb.createNode();
					NewTransaction();
					long oldCount = count;
					count = 0;
					foreach ( Node node in GraphDb.AllNodes )
					{
						 count++;
						 if ( node.Equals( newNode ) )
						 {
							  found = true;
						 }
					}
					assertTrue( found );
					assertEquals( count, oldCount + 1 );

					// Tests a bug in the "all nodes" iterator
					ResourceIterator<Node> allNodesIterator = GraphDb.AllNodes.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertNotNull( allNodesIterator.next() );
					allNodesIterator.Close();

					newNode.Delete();
					NewTransaction();
					found = false;
					count = 0;
					foreach ( Node node in GraphDb.AllNodes )
					{
						 count++;
						 if ( node.Equals( newNode ) )
						 {
							  found = true;
						 }
					}
					assertTrue( !found );
					assertEquals( count, oldCount );
			  }
			  // else we skip test, takes too long
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMultipleShutdown()
		 public virtual void TestMultipleShutdown()
		 {
			  Commit();
			  GraphDb.shutdown();
			  GraphDb.shutdown();
		 }
	}

}