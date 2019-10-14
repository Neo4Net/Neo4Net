using System;
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
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using ConstraintViolationException = Neo4Net.Graphdb.ConstraintViolationException;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using NotFoundException = Neo4Net.Graphdb.NotFoundException;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using GraphTransactionRule = Neo4Net.Test.rule.GraphTransactionRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class NodeTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public static DatabaseRule Db = new ImpermanentDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.GraphTransactionRule tx = new org.neo4j.test.rule.GraphTransactionRule(db);
		 public GraphTransactionRule Tx = new GraphTransactionRule( Db );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveHelpfulExceptionWhenDeletingNodeWithRels()
		 public virtual void ShouldGiveHelpfulExceptionWhenDeletingNodeWithRels()
		 {
			  // Given
			  Node node;

			  node = Db.createNode();
			  Node node2 = Db.createNode();
			  node.CreateRelationshipTo( node2, RelationshipType.withName( "MAYOR_OF" ) );
			  Tx.success();

			  // And given a transaction deleting just the node
			  Tx.begin();
			  node.Delete();

			  // Expect
			  Exception.expect( typeof( ConstraintViolationException ) );
			  Exception.expectMessage( "Cannot delete node<" + node.Id + ">, because it still has relationships. " + "To delete this node, you must first delete its relationships." );

			  // When I commit
			  Tx.success();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodeCreateAndDelete()
		 public virtual void TestNodeCreateAndDelete()
		 {
			  Node node = GraphDb.createNode();
			  long nodeId = node.Id;
			  GraphDb.getNodeById( nodeId );
			  node.Delete();

			  Tx.success();
			  Tx.begin();
			  try
			  {
					GraphDb.getNodeById( nodeId );
					fail( "Node[" + nodeId + "] should be deleted." );
			  }
			  catch ( NotFoundException )
			  {
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDeletedNode()
		 public virtual void TestDeletedNode()
		 {
			  // do some evil stuff
			  Node node = GraphDb.createNode();
			  node.Delete();
			  try
			  {
					node.SetProperty( "key1", 1 );
					fail( "Adding stuff to deleted node should throw exception" );
			  }
			  catch ( Exception )
			  { // good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodeAddPropertyWithNullKey()
		 public virtual void TestNodeAddPropertyWithNullKey()
		 {
			  Node node1 = GraphDb.createNode();
			  try
			  {
					node1.SetProperty( null, "bar" );
					fail( "Null key should result in exception." );
			  }
			  catch ( System.ArgumentException )
			  {
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodeAddPropertyWithNullValue()
		 public virtual void TestNodeAddPropertyWithNullValue()
		 {
			  Node node1 = GraphDb.createNode();
			  try
			  {
					node1.SetProperty( "foo", null );
					fail( "Null value should result in exception." );
			  }
			  catch ( System.ArgumentException )
			  {
			  }
			  Tx.failure();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodeAddProperty()
		 public virtual void TestNodeAddProperty()
		 {
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();

			  string key1 = "key1";
			  string key2 = "key2";
			  string key3 = "key3";
			  int? int1 = 1;
			  int? int2 = 2;
			  string string1 = "1";
			  string string2 = "2";

			  // add property
			  node1.SetProperty( key1, int1 );
			  node2.SetProperty( key1, string1 );
			  node1.SetProperty( key2, string2 );
			  node2.SetProperty( key2, int2 );
			  assertTrue( node1.HasProperty( key1 ) );
			  assertTrue( node2.HasProperty( key1 ) );
			  assertTrue( node1.HasProperty( key2 ) );
			  assertTrue( node2.HasProperty( key2 ) );
			  assertTrue( !node1.HasProperty( key3 ) );
			  assertTrue( !node2.HasProperty( key3 ) );
			  assertEquals( int1, node1.GetProperty( key1 ) );
			  assertEquals( string1, node2.GetProperty( key1 ) );
			  assertEquals( string2, node1.GetProperty( key2 ) );
			  assertEquals( int2, node2.GetProperty( key2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodeRemoveProperty()
		 public virtual void TestNodeRemoveProperty()
		 {
			  string key1 = "key1";
			  string key2 = "key2";
			  int? int1 = 1;
			  int? int2 = 2;
			  string string1 = "1";
			  string string2 = "2";

			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();

			  try
			  {
					if ( node1.RemoveProperty( key1 ) != null )
					{
						 fail( "Remove of non existing property should return null" );
					}
			  }
			  catch ( NotFoundException )
			  {
			  }
			  try
			  {
					node1.RemoveProperty( null );
					fail( "Remove null property should throw exception." );
			  }
			  catch ( System.ArgumentException )
			  {
			  }

			  node1.SetProperty( key1, int1 );
			  node2.SetProperty( key1, string1 );
			  node1.SetProperty( key2, string2 );
			  node2.SetProperty( key2, int2 );
			  try
			  {
					node1.RemoveProperty( null );
					fail( "Null argument should result in exception." );
			  }
			  catch ( System.ArgumentException )
			  {
			  }

			  // test remove property
			  assertEquals( int1, node1.RemoveProperty( key1 ) );
			  assertEquals( string1, node2.RemoveProperty( key1 ) );
			  // test remove of non existing property
			  try
			  {
					if ( node2.RemoveProperty( key1 ) != null )
					{
						 fail( "Remove of non existing property return null." );
					}
			  }
			  catch ( NotFoundException )
			  {
					// must mark as rollback only
			  }
			  //       getTransaction().failure();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodeChangeProperty()
		 public virtual void TestNodeChangeProperty()
		 {
			  string key1 = "key1";
			  string key2 = "key2";
			  string key3 = "key3";
			  int? int1 = 1;
			  int? int2 = 2;
			  string string1 = "1";
			  string string2 = "2";
			  bool? bool1 = true;
			  bool? bool2 = false;

			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  node1.SetProperty( key1, int1 );
			  node2.SetProperty( key1, string1 );
			  node1.SetProperty( key2, string2 );
			  node2.SetProperty( key2, int2 );

			  try
			  {
					node1.SetProperty( null, null );
					fail( "Null argument should result in exception." );
			  }
			  catch ( System.ArgumentException )
			  {
			  }
			  catch ( NotFoundException )
			  {
					fail( "wrong exception" );
			  }

			  // test change property
			  node1.SetProperty( key1, int2 );
			  node2.SetProperty( key1, string2 );
			  assertEquals( string2, node2.GetProperty( key1 ) );
			  node1.SetProperty( key3, bool1 );
			  node1.SetProperty( key3, bool2 );
			  assertEquals( string2, node2.GetProperty( key1 ) );
			  node1.Delete();
			  node2.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodeChangeProperty2()
		 public virtual void TestNodeChangeProperty2()
		 {
			  string key1 = "key1";
			  int? int1 = 1;
			  int? int2 = 2;
			  string string1 = "1";
			  string string2 = "2";
			  bool? bool1 = true;
			  bool? bool2 = false;
			  Node node1 = GraphDb.createNode();
			  node1.SetProperty( key1, int1 );
			  node1.SetProperty( key1, int2 );
			  assertEquals( int2, node1.GetProperty( key1 ) );
			  node1.RemoveProperty( key1 );
			  node1.SetProperty( key1, string1 );
			  node1.SetProperty( key1, string2 );
			  assertEquals( string2, node1.GetProperty( key1 ) );
			  node1.RemoveProperty( key1 );
			  node1.SetProperty( key1, bool1 );
			  node1.SetProperty( key1, bool2 );
			  assertEquals( bool2, node1.GetProperty( key1 ) );
			  node1.RemoveProperty( key1 );
			  node1.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNodeGetProperties()
		 public virtual void TestNodeGetProperties()
		 {
			  string key1 = "key1";
			  string key2 = "key2";
			  string key3 = "key3";
			  int? int1 = 1;
			  int? int2 = 2;
			  string @string = "3";

			  Node node1 = GraphDb.createNode();
			  try
			  {
					node1.GetProperty( key1 );
					fail( "get non existing property didn't throw exception" );
			  }
			  catch ( NotFoundException )
			  {
			  }
			  try
			  {
					node1.GetProperty( null );
					fail( "get of null key didn't throw exception" );
			  }
			  catch ( System.ArgumentException )
			  {
			  }
			  assertTrue( !node1.HasProperty( key1 ) );
			  assertTrue( !node1.HasProperty( null ) );
			  node1.SetProperty( key1, int1 );
			  node1.SetProperty( key2, int2 );
			  node1.SetProperty( key3, @string );
			  IEnumerator<string> keys = node1.PropertyKeys.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  keys.next();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  keys.next();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  keys.next();
			  IDictionary<string, object> properties = node1.AllProperties;
			  assertEquals( properties[key1], int1 );
			  assertEquals( properties[key2], int2 );
			  assertEquals( properties[key3], @string );
			  properties = node1.GetProperties( key1, key2 );
			  assertEquals( properties[key1], int1 );
			  assertEquals( properties[key2], int2 );
			  assertFalse( properties.ContainsKey( key3 ) );

			  properties = node1.Properties;
			  assertTrue( properties.Count == 0 );

			  try
			  {
					string[] names = null;
					node1.GetProperties( names );
					fail();
			  }
			  catch ( System.NullReferenceException )
			  {
					// Ok
			  }

			  try
			  {
					string[] names = new string[]{ null };
					node1.GetProperties( names );
					fail();
			  }
			  catch ( System.NullReferenceException )
			  {
					// Ok
			  }

			  try
			  {
					node1.RemoveProperty( key3 );
			  }
			  catch ( NotFoundException )
			  {
					fail( "Remove of property failed." );
			  }
			  assertTrue( !node1.HasProperty( key3 ) );
			  assertTrue( !node1.HasProperty( null ) );
			  node1.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAddPropertyThenDelete()
		 public virtual void TestAddPropertyThenDelete()
		 {
			  Node node = GraphDb.createNode();
			  node.SetProperty( "test", "test" );

			  Tx.success();
			  Tx.begin();
			  node.SetProperty( "test2", "test2" );
			  node.Delete();

			  Tx.success();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testChangeProperty()
		 public virtual void TestChangeProperty()
		 {
			  Node node = GraphDb.createNode();
			  node.SetProperty( "test", "test1" );
			  Tx.success();
			  Tx.begin();
			  node.SetProperty( "test", "test2" );
			  node.RemoveProperty( "test" );
			  node.SetProperty( "test", "test3" );
			  assertEquals( "test3", node.GetProperty( "test" ) );
			  node.RemoveProperty( "test" );
			  node.SetProperty( "test", "test4" );
			  Tx.success();
			  Tx.begin();
			  assertEquals( "test4", node.GetProperty( "test" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testChangeProperty2()
		 public virtual void TestChangeProperty2()
		 {
			  Node node = GraphDb.createNode();
			  node.SetProperty( "test", "test1" );
			  Tx.success();
			  Tx.begin();
			  node.RemoveProperty( "test" );
			  node.SetProperty( "test", "test3" );
			  assertEquals( "test3", node.GetProperty( "test" ) );
			  Tx.success();
			  Tx.begin();
			  assertEquals( "test3", node.GetProperty( "test" ) );
			  node.RemoveProperty( "test" );
			  node.SetProperty( "test", "test4" );
			  Tx.success();
			  Tx.begin();
			  assertEquals( "test4", node.GetProperty( "test" ) );
		 }

		 private GraphDatabaseService GraphDb
		 {
			 get
			 {
				  return Db.GraphDatabaseAPI;
			 }
		 }
	}

}