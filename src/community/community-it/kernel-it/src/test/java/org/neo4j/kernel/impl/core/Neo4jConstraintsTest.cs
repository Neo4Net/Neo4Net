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

	using Node = Neo4Net.GraphDb.Node;
	using NotFoundException = Neo4Net.GraphDb.NotFoundException;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Transaction = Neo4Net.GraphDb.Transaction;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class Neo4NetConstraintsTest : AbstractNeo4NetTestCase
	{
		 private readonly string _key = "testproperty";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDeleteReferenceNodeOrLastNodeIsOk()
		 public virtual void TestDeleteReferenceNodeOrLastNodeIsOk()
		 {
			  Transaction tx = Transaction;
			  for ( int i = 0; i < 10; i++ )
			  {
					GraphDb.createNode();
			  }
			  // long numNodesPre = getNodeManager().getNumberOfIdsInUse( Node.class
			  // );
			  // empty the DB instance
			  foreach ( Node node in GraphDb.AllNodes )
			  {
					foreach ( Relationship rel in node.Relationships )
					{
						 rel.Delete();
					}
					node.Delete();
			  }
			  tx.Success();
			  tx.Close();
			  tx = GraphDb.beginTx();
			  assertFalse( GraphDb.AllNodes.GetEnumerator().hasNext() );
			  // TODO: this should be valid, fails right now!
			  // assertEquals( 0, numNodesPost );
			  tx.Success();
			  tx.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDeleteNodeWithRel1()
		 public virtual void TestDeleteNodeWithRel1()
		 {
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  node1.CreateRelationshipTo( node2, MyRelTypes.TEST );
			  node1.Delete();
			  try
			  {
					Transaction tx = Transaction;
					tx.Success();
					tx.Close();
					fail( "Should not validate" );
			  }
			  catch ( Exception )
			  {
					// good
			  }
			  Transaction = GraphDb.beginTx();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDeleteNodeWithRel2()
		 public virtual void TestDeleteNodeWithRel2()
		 {
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  node1.CreateRelationshipTo( node2, MyRelTypes.TEST );
			  node2.Delete();
			  node1.Delete();
			  try
			  {
					Transaction tx = Transaction;
					tx.Success();
					tx.Close();
					fail( "Should not validate" );
			  }
			  catch ( Exception )
			  {
					// good
			  }
			  Transaction = GraphDb.beginTx();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDeleteNodeWithRel3()
		 public virtual void TestDeleteNodeWithRel3()
		 {
			  // make sure we can delete in wrong order
			  Node node0 = GraphDb.createNode();
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  Relationship rel0 = node0.CreateRelationshipTo( node1, MyRelTypes.TEST );
			  Relationship rel1 = node0.CreateRelationshipTo( node2, MyRelTypes.TEST );
			  node1.Delete();
			  rel0.Delete();
			  Transaction tx = Transaction;
			  tx.Success();
			  tx.Close();
			  Transaction = GraphDb.beginTx();
			  node2.Delete();
			  rel1.Delete();
			  node0.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCreateRelOnDeletedNode()
		 public virtual void TestCreateRelOnDeletedNode()
		 {
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  Transaction tx = Transaction;
			  tx.Success();
			  tx.Close();
			  tx = GraphDb.beginTx();
			  node1.Delete();
			  try
			  {
					node1.CreateRelationshipTo( node2, MyRelTypes.TEST );
					fail( "Create of rel on deleted node should fail fast" );
			  }
			  catch ( Exception )
			  { // ok
			  }
			  try
			  {
					tx.Failure();
					tx.Close();
					// fail( "Transaction should be marked rollback" );
			  }
			  catch ( Exception )
			  { // good
			  }
			  Transaction = GraphDb.beginTx();
			  node2.Delete();
			  node1.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAddPropertyDeletedNode()
		 public virtual void TestAddPropertyDeletedNode()
		 {
			  Node node = GraphDb.createNode();
			  node.Delete();
			  try
			  {
					node.SetProperty( _key, 1 );
					fail( "Add property on deleted node should not validate" );
			  }
			  catch ( Exception )
			  {
					// good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRemovePropertyDeletedNode()
		 public virtual void TestRemovePropertyDeletedNode()
		 {
			  Node node = GraphDb.createNode();
			  node.SetProperty( _key, 1 );
			  node.Delete();
			  try
			  {
					node.RemoveProperty( _key );
					Transaction tx = Transaction;
					tx.Success();
					tx.Close();
					fail( "Change property on deleted node should not validate" );
			  }
			  catch ( Exception )
			  {
					// ok
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testChangePropertyDeletedNode()
		 public virtual void TestChangePropertyDeletedNode()
		 {
			  Node node = GraphDb.createNode();
			  node.SetProperty( _key, 1 );
			  node.Delete();
			  try
			  {
					node.SetProperty( _key, 2 );
					Transaction tx = Transaction;
					tx.Success();
					tx.Close();
					fail( "Change property on deleted node should not validate" );
			  }
			  catch ( Exception )
			  {
					// ok
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAddPropertyDeletedRelationship()
		 public virtual void TestAddPropertyDeletedRelationship()
		 {
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  Relationship rel = node1.CreateRelationshipTo( node2, MyRelTypes.TEST );
			  rel.Delete();
			  try
			  {
					rel.SetProperty( _key, 1 );
					Transaction tx = Transaction;
					tx.Success();
					tx.Close();
					fail( "Add property on deleted rel should not validate" );
			  }
			  catch ( Exception )
			  { // good
			  }
			  node1.Delete();
			  node2.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRemovePropertyDeletedRelationship()
		 public virtual void TestRemovePropertyDeletedRelationship()
		 {
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  Relationship rel = node1.CreateRelationshipTo( node2, MyRelTypes.TEST );
			  rel.SetProperty( _key, 1 );
			  rel.Delete();
			  try
			  {
					rel.RemoveProperty( _key );
					Transaction tx = Transaction;
					tx.Success();
					tx.Close();
					fail( "Remove property on deleted rel should not validate" );
			  }
			  catch ( Exception )
			  {
					// ok
			  }
			  node1.Delete();
			  node2.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testChangePropertyDeletedRelationship()
		 public virtual void TestChangePropertyDeletedRelationship()
		 {
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  Relationship rel = node1.CreateRelationshipTo( node2, MyRelTypes.TEST );
			  rel.SetProperty( _key, 1 );
			  rel.Delete();
			  try
			  {
					rel.SetProperty( _key, 2 );
					Transaction tx = Transaction;
					tx.Success();
					tx.Close();
					fail( "Change property on deleted rel should not validate" );
			  }
			  catch ( Exception )
			  {
					// ok
			  }
			  node1.Delete();
			  node2.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMultipleDeleteNode()
		 public virtual void TestMultipleDeleteNode()
		 {
			  Node node1 = GraphDb.createNode();
			  node1.Delete();
			  try
			  {
					node1.Delete();
					Transaction tx = Transaction;
					tx.Success();
					tx.Close();
					fail( "Should not validate" );
			  }
			  catch ( Exception )
			  {
					// ok
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMultipleDeleteRelationship()
		 public virtual void TestMultipleDeleteRelationship()
		 {
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  Relationship rel = node1.CreateRelationshipTo( node2, MyRelTypes.TEST );
			  rel.Delete();
			  node1.Delete();
			  node2.Delete();
			  try
			  {
					rel.Delete();
					Transaction tx = Transaction;
					tx.Success();
					tx.Close();
					fail( "Should not validate" );
			  }
			  catch ( Exception )
			  {
					// ok
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testIllegalPropertyType()
		 public virtual void TestIllegalPropertyType()
		 {
			  Node node1 = GraphDb.createNode();
			  try
			  {
					node1.SetProperty( _key, new object() );
					fail( "Shouldn't validate" );
			  }
			  catch ( Exception )
			  { // good
			  }
			  {
					Transaction tx = Transaction;
					tx.Failure();
					tx.Close();
			  }
			  Transaction = GraphDb.beginTx();
			  try
			  {
					GraphDb.getNodeById( node1.Id );
					fail( "Node should not exist, previous tx didn't rollback" );
			  }
			  catch ( NotFoundException )
			  {
					// good
			  }
			  node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  Relationship rel = node1.CreateRelationshipTo( node2, MyRelTypes.TEST );
			  try
			  {
					rel.SetProperty( _key, new object() );
					fail( "Shouldn't validate" );
			  }
			  catch ( Exception )
			  { // good
			  }
			  try
			  {
					Transaction tx = Transaction;
					tx.Success();
					tx.Close();
					fail( "Shouldn't validate" );
			  }
			  catch ( Exception )
			  { // good
			  }
			  Transaction = GraphDb.beginTx();
			  try
			  {
					GraphDb.getNodeById( node1.Id );
					fail( "Node should not exist, previous tx didn't rollback" );
			  }
			  catch ( NotFoundException )
			  {
					// good
			  }
			  try
			  {
					GraphDb.getNodeById( node2.Id );
					fail( "Node should not exist, previous tx didn't rollback" );
			  }
			  catch ( NotFoundException )
			  {
					// good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodeRelDeleteSemantics()
		 public virtual void TestNodeRelDeleteSemantics()
		 {
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  Relationship rel1 = node1.CreateRelationshipTo( node2, MyRelTypes.TEST );
			  Relationship rel2 = node1.CreateRelationshipTo( node2, MyRelTypes.TEST );
			  node1.SetProperty( "key1", "value1" );
			  rel1.SetProperty( "key1", "value1" );

			  NewTransaction();
			  node1.Delete();
			  try
			  {
					node1.GetProperty( "key1" );
					fail( "Should throw exception" );
			  }
			  catch ( NotFoundException )
			  { // good
			  }
			  try
			  {
					node1.SetProperty( "key1", "value2" );
					fail( "Should throw exception" );
			  }
			  catch ( NotFoundException )
			  { // good
			  }
			  try
			  {
					node1.RemoveProperty( "key1" );
					fail( "Should throw exception" );
			  }
			  catch ( NotFoundException )
			  { // good
			  }
			  node2.Delete();
			  try
			  {
					node2.Delete();
					fail( "Should throw exception" );
			  }
			  catch ( NotFoundException )
			  { // good
			  }
			  try
			  {
					node1.GetProperty( "key1" );
					fail( "Should throw exception" );
			  }
			  catch ( NotFoundException )
			  { // good
			  }
			  try
			  {
					node1.SetProperty( "key1", "value2" );
					fail( "Should throw exception" );
			  }
			  catch ( NotFoundException )
			  { // good
			  }
			  try
			  {
					node1.RemoveProperty( "key1" );
					fail( "Should throw exception" );
			  }
			  catch ( NotFoundException )
			  { // good
			  }
			  assertEquals( "value1", rel1.GetProperty( "key1" ) );
			  rel1.Delete();
			  try
			  {
					rel1.Delete();
					fail( "Should throw exception" );
			  }
			  catch ( NotFoundException )
			  { // good
			  }
			  try
			  {
					rel1.GetProperty( "key1" );
					fail( "Should throw exception" );
			  }
			  catch ( NotFoundException )
			  { // good
			  }
			  try
			  {
					rel1.SetProperty( "key1", "value2" );
					fail( "Should throw exception" );
			  }
			  catch ( NotFoundException )
			  { // good
			  }
			  try
			  {
					rel1.RemoveProperty( "key1" );
					fail( "Should throw exception" );
			  }
			  catch ( NotFoundException )
			  { // good
			  }
			  try
			  {
					rel1.GetProperty( "key1" );
					fail( "Should throw exception" );
			  }
			  catch ( NotFoundException )
			  { // good
			  }
			  try
			  {
					rel1.SetProperty( "key1", "value2" );
					fail( "Should throw exception" );
			  }
			  catch ( NotFoundException )
			  { // good
			  }
			  try
			  {
					rel1.RemoveProperty( "key1" );
					fail( "Should throw exception" );
			  }
			  catch ( NotFoundException )
			  { // good
			  }
			  try
			  {
					node2.CreateRelationshipTo( node1, MyRelTypes.TEST );
					fail( "Should throw exception" );
			  }
			  catch ( NotFoundException )
			  { // good
			  }
			  try
			  {
					node2.CreateRelationshipTo( node1, MyRelTypes.TEST );
					fail( "Should throw exception" );
			  }
			  catch ( NotFoundException )
			  { // good
			  }

			  assertEquals( node1, rel1.StartNode );
			  assertEquals( node2, rel2.EndNode );
			  Node[] nodes = rel1.Nodes;
			  assertEquals( node1, nodes[0] );
			  assertEquals( node2, nodes[1] );
			  assertEquals( node2, rel1.GetOtherNode( node1 ) );
			  rel2.Delete();
			  // will be marked for rollback so commit will throw exception
			  Rollback();
		 }
	}

}