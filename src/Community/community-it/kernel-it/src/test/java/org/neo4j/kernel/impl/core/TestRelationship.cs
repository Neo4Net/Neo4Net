using System;
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
namespace Neo4Net.Kernel.impl.core
{
	using Test = org.junit.Test;


	using Direction = Neo4Net.Graphdb.Direction;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using NotFoundException = Neo4Net.Graphdb.NotFoundException;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Neo4Net.Graphdb;
	using Neo4Net.Graphdb;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Iterables = Neo4Net.Helpers.Collection.Iterables;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.RelationshipType.withName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.MyRelTypes.TEST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.MyRelTypes.TEST2;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.MyRelTypes.TEST_TRAVERSAL;

	public class TestRelationship : AbstractNeo4jTestCase
	{
		 private readonly string _key1 = "key1";
		 private readonly string _key2 = "key2";
		 private readonly string _key3 = "key3";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSimple()
		 public virtual void TestSimple()
		 {
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  Relationship rel1 = node1.CreateRelationshipTo( node2, TEST );
			  node1.CreateRelationshipTo( node2, TEST );
			  rel1.Delete();
			  NewTransaction();
			  AssertHasNext( ( ResourceIterable<Relationship> ) node1.Relationships );
			  AssertHasNext( ( ResourceIterable<Relationship> ) node2.Relationships );
			  AssertHasNext( ( ResourceIterable<Relationship> ) node1.GetRelationships( TEST ) );
			  AssertHasNext( ( ResourceIterable<Relationship> ) node2.GetRelationships( TEST ) );
			  AssertHasNext( ( ResourceIterable<Relationship> ) node1.GetRelationships( TEST, Direction.OUTGOING ) );
			  AssertHasNext( ( ResourceIterable<Relationship> ) node2.GetRelationships( TEST, Direction.INCOMING ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSimple2()
		 public virtual void TestSimple2()
		 {
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  for ( int i = 0; i < 3; i++ )
			  {
					node1.CreateRelationshipTo( node2, TEST );
					node1.CreateRelationshipTo( node2, TEST_TRAVERSAL );
					node1.CreateRelationshipTo( node2, TEST2 );
			  }
			  AllGetRelationshipMethods( node1, Direction.OUTGOING );
			  AllGetRelationshipMethods( node2, Direction.INCOMING );
			  NewTransaction();
			  AllGetRelationshipMethods( node1, Direction.OUTGOING );
			  AllGetRelationshipMethods( node2, Direction.INCOMING );
			  DeleteFirst( ( ResourceIterable<Relationship> ) node1.GetRelationships( TEST, Direction.OUTGOING ) );
			  DeleteFirst( ( ResourceIterable<Relationship> ) node1.GetRelationships( TEST_TRAVERSAL, Direction.OUTGOING ) );
			  DeleteFirst( ( ResourceIterable<Relationship> ) node1.GetRelationships( TEST2, Direction.OUTGOING ) );
			  node1.CreateRelationshipTo( node2, TEST );
			  node1.CreateRelationshipTo( node2, TEST_TRAVERSAL );
			  node1.CreateRelationshipTo( node2, TEST2 );
			  AllGetRelationshipMethods( node1, Direction.OUTGOING );
			  AllGetRelationshipMethods( node2, Direction.INCOMING );
			  NewTransaction();
			  AllGetRelationshipMethods( node1, Direction.OUTGOING );
			  AllGetRelationshipMethods( node2, Direction.INCOMING );
			  foreach ( Relationship rel in node1.Relationships )
			  {
					rel.Delete();
			  }
			  node1.Delete();
			  node2.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSimple3()
		 public virtual void TestSimple3()
		 {
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  for ( int i = 0; i < 1; i++ )
			  {
					node1.CreateRelationshipTo( node2, TEST );
					node1.CreateRelationshipTo( node2, TEST_TRAVERSAL );
					node1.CreateRelationshipTo( node2, TEST2 );
			  }
			  AllGetRelationshipMethods2( node1, Direction.OUTGOING );
			  AllGetRelationshipMethods2( node2, Direction.INCOMING );
			  NewTransaction();
			  AllGetRelationshipMethods2( node1, Direction.OUTGOING );
			  AllGetRelationshipMethods2( node2, Direction.INCOMING );
			  DeleteFirst( ( ResourceIterable<Relationship> ) node1.GetRelationships( TEST, Direction.OUTGOING ) );
			  DeleteFirst( ( ResourceIterable<Relationship> ) node1.GetRelationships( TEST_TRAVERSAL, Direction.OUTGOING ) );
			  DeleteFirst( ( ResourceIterable<Relationship> ) node1.GetRelationships( TEST2, Direction.OUTGOING ) );
			  node1.CreateRelationshipTo( node2, TEST );
			  node1.CreateRelationshipTo( node2, TEST_TRAVERSAL );
			  node1.CreateRelationshipTo( node2, TEST2 );
			  AllGetRelationshipMethods2( node1, Direction.OUTGOING );
			  AllGetRelationshipMethods2( node2, Direction.INCOMING );
			  NewTransaction();
			  AllGetRelationshipMethods2( node1, Direction.OUTGOING );
			  AllGetRelationshipMethods2( node2, Direction.INCOMING );
			  foreach ( Relationship rel in node1.Relationships )
			  {
					rel.Delete();
			  }
			  node1.Delete();
			  node2.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSimple4()
		 public virtual void TestSimple4()
		 {
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  for ( int i = 0; i < 2; i++ )
			  {
					node1.CreateRelationshipTo( node2, TEST );
					node1.CreateRelationshipTo( node2, TEST_TRAVERSAL );
					node1.CreateRelationshipTo( node2, TEST2 );
			  }
			  AllGetRelationshipMethods3( node1, Direction.OUTGOING );
			  AllGetRelationshipMethods3( node2, Direction.INCOMING );
			  NewTransaction();
			  AllGetRelationshipMethods3( node1, Direction.OUTGOING );
			  AllGetRelationshipMethods3( node2, Direction.INCOMING );
			  DeleteFirst( ( ResourceIterable<Relationship> ) node1.GetRelationships( TEST, Direction.OUTGOING ) );
			  int count = 0;
			  foreach ( Relationship rel in node1.GetRelationships( TEST_TRAVERSAL, Direction.OUTGOING ) )
			  {
					if ( count == 1 )
					{
						 rel.Delete();
					}
					count++;
			  }
			  DeleteFirst( ( ResourceIterable<Relationship> ) node1.GetRelationships( TEST2, Direction.OUTGOING ) );
			  node1.CreateRelationshipTo( node2, TEST );
			  node1.CreateRelationshipTo( node2, TEST_TRAVERSAL );
			  node1.CreateRelationshipTo( node2, TEST2 );
			  AllGetRelationshipMethods3( node1, Direction.OUTGOING );
			  AllGetRelationshipMethods3( node2, Direction.INCOMING );
			  NewTransaction();
			  AllGetRelationshipMethods3( node1, Direction.OUTGOING );
			  AllGetRelationshipMethods3( node2, Direction.INCOMING );
			  foreach ( Relationship rel in node1.Relationships )
			  {
					rel.Delete();
			  }
			  node1.Delete();
			  node2.Delete();
		 }

		 private void AllGetRelationshipMethods( Node node, Direction dir )
		 {
			  CountRelationships( 9, node.Relationships );
			  CountRelationships( 9, node.GetRelationships( dir ) );
			  CountRelationships( 9, node.GetRelationships( TEST, TEST2, TEST_TRAVERSAL ) );
			  CountRelationships( 6, node.GetRelationships( TEST, TEST2 ) );
			  CountRelationships( 6, node.GetRelationships( TEST, TEST_TRAVERSAL ) );
			  CountRelationships( 6, node.GetRelationships( TEST2, TEST_TRAVERSAL ) );
			  CountRelationships( 3, node.GetRelationships( TEST ) );
			  CountRelationships( 3, node.GetRelationships( TEST2 ) );
			  CountRelationships( 3, node.GetRelationships( TEST_TRAVERSAL ) );
			  CountRelationships( 3, node.GetRelationships( TEST, dir ) );
			  CountRelationships( 3, node.GetRelationships( TEST2, dir ) );
			  CountRelationships( 3, node.GetRelationships( TEST_TRAVERSAL, dir ) );
		 }

		 private void AllGetRelationshipMethods2( Node node, Direction dir )
		 {
			  CountRelationships( 3, node.Relationships );
			  CountRelationships( 3, node.GetRelationships( dir ) );
			  CountRelationships( 3, node.GetRelationships( TEST, TEST2, TEST_TRAVERSAL ) );
			  CountRelationships( 2, node.GetRelationships( TEST, TEST2 ) );
			  CountRelationships( 2, node.GetRelationships( TEST, TEST_TRAVERSAL ) );
			  CountRelationships( 2, node.GetRelationships( TEST2, TEST_TRAVERSAL ) );
			  CountRelationships( 1, node.GetRelationships( TEST ) );
			  CountRelationships( 1, node.GetRelationships( TEST2 ) );
			  CountRelationships( 1, node.GetRelationships( TEST_TRAVERSAL ) );
			  CountRelationships( 1, node.GetRelationships( TEST, dir ) );
			  CountRelationships( 1, node.GetRelationships( TEST2, dir ) );
			  CountRelationships( 1, node.GetRelationships( TEST_TRAVERSAL, dir ) );
		 }

		 private void AllGetRelationshipMethods3( Node node, Direction dir )
		 {
			  CountRelationships( 6, node.Relationships );
			  CountRelationships( 6, node.GetRelationships( dir ) );
			  CountRelationships( 6, node.GetRelationships( TEST, TEST2, TEST_TRAVERSAL ) );
			  CountRelationships( 4, node.GetRelationships( TEST, TEST2 ) );
			  CountRelationships( 4, node.GetRelationships( TEST, TEST_TRAVERSAL ) );
			  CountRelationships( 4, node.GetRelationships( TEST2, TEST_TRAVERSAL ) );
			  CountRelationships( 2, node.GetRelationships( TEST ) );
			  CountRelationships( 2, node.GetRelationships( TEST2 ) );
			  CountRelationships( 2, node.GetRelationships( TEST_TRAVERSAL ) );
			  CountRelationships( 2, node.GetRelationships( TEST, dir ) );
			  CountRelationships( 2, node.GetRelationships( TEST2, dir ) );
			  CountRelationships( 2, node.GetRelationships( TEST_TRAVERSAL, dir ) );
		 }

		 private void CountRelationships( int expectedCount, IEnumerable<Relationship> rels )
		 {
			  int count = 0;
			  foreach ( Relationship ignored in rels )
			  {
					count++;
			  }
			  assertEquals( expectedCount, count );
		 }

		 private void DeleteFirst( ResourceIterable<Relationship> iterable )
		 {
			  using ( ResourceIterator<Relationship> iterator = iterable.GetEnumerator() )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					iterator.next().delete();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRelationshipCreateAndDelete()
		 public virtual void TestRelationshipCreateAndDelete()
		 {
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  Relationship relationship = node1.CreateRelationshipTo( node2, TEST );
			  Relationship[] relArray1 = GetRelationshipArray( node1.Relationships );
			  Relationship[] relArray2 = GetRelationshipArray( node2.Relationships );
			  assertEquals( 1, relArray1.Length );
			  assertEquals( relationship, relArray1[0] );
			  assertEquals( 1, relArray2.Length );
			  assertEquals( relationship, relArray2[0] );
			  relArray1 = GetRelationshipArray( node1.GetRelationships( TEST ) );
			  assertEquals( 1, relArray1.Length );
			  assertEquals( relationship, relArray1[0] );
			  relArray2 = GetRelationshipArray( node2.GetRelationships( TEST ) );
			  assertEquals( 1, relArray2.Length );
			  assertEquals( relationship, relArray2[0] );
			  relArray1 = GetRelationshipArray( node1.GetRelationships( TEST, Direction.OUTGOING ) );
			  assertEquals( 1, relArray1.Length );
			  relArray2 = GetRelationshipArray( node2.GetRelationships( TEST, Direction.INCOMING ) );
			  assertEquals( 1, relArray2.Length );
			  relArray1 = GetRelationshipArray( node1.GetRelationships( TEST, Direction.INCOMING ) );
			  assertEquals( 0, relArray1.Length );
			  relArray2 = GetRelationshipArray( node2.GetRelationships( TEST, Direction.OUTGOING ) );
			  assertEquals( 0, relArray2.Length );
			  relationship.Delete();
			  node2.Delete();
			  node1.Delete();
		 }

		 private Relationship[] GetRelationshipArray( IEnumerable<Relationship> relsIterable )
		 {
			  List<Relationship> relList = new List<Relationship>();
			  foreach ( Relationship rel in relsIterable )
			  {
					relList.Add( rel );
			  }
			  return relList.ToArray();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDeleteWithRelationship()
		 public virtual void TestDeleteWithRelationship()
		 {
			  // do some evil stuff
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  node1.CreateRelationshipTo( node2, TEST );
			  node1.Delete();
			  node2.Delete();
			  try
			  {
					Transaction.success();
					Transaction.close();
					fail( "deleting node with relationship should not commit." );
			  }
			  catch ( Exception )
			  {
					// good
			  }
			  Transaction = GraphDb.beginTx();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDeletedRelationship()
		 public virtual void TestDeletedRelationship()
		 {
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  Relationship relationship = node1.CreateRelationshipTo( node2, TEST );
			  relationship.Delete();
			  try
			  {
					relationship.SetProperty( "key1", 1 );
					fail( "Adding property to deleted rel should throw exception." );
			  }
			  catch ( Exception )
			  { // good
			  }
			  node1.Delete();
			  node2.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRelationshipAddPropertyWithNullKey()
		 public virtual void TestRelationshipAddPropertyWithNullKey()
		 {
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  Relationship rel1 = node1.CreateRelationshipTo( node2, TEST );

			  try
			  {
					rel1.SetProperty( null, "bar" );
					fail( "Null key should result in exception." );
			  }
			  catch ( System.ArgumentException )
			  { // OK
			  }

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRelationshipAddPropertyWithNullValue()
		 public virtual void TestRelationshipAddPropertyWithNullValue()
		 {
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  Relationship rel1 = node1.CreateRelationshipTo( node2, TEST );

			  try
			  {
					rel1.SetProperty( "foo", null );
					fail( "Null value should result in exception." );
			  }
			  catch ( System.ArgumentException )
			  { // OK
			  }

			  Transaction.failure();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRelationshipAddProperty()
		 public virtual void TestRelationshipAddProperty()
		 {
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  Relationship rel1 = node1.CreateRelationshipTo( node2, TEST );
			  Relationship rel2 = node2.CreateRelationshipTo( node1, TEST );

			  int? int1 = 1;
			  int? int2 = 2;
			  string string1 = "1";
			  string string2 = "2";

			  // add property
			  rel1.SetProperty( _key1, int1 );
			  rel2.SetProperty( _key1, string1 );
			  rel1.SetProperty( _key2, string2 );
			  rel2.SetProperty( _key2, int2 );
			  assertTrue( rel1.HasProperty( _key1 ) );
			  assertTrue( rel2.HasProperty( _key1 ) );
			  assertTrue( rel1.HasProperty( _key2 ) );
			  assertTrue( rel2.HasProperty( _key2 ) );
			  assertTrue( !rel1.HasProperty( _key3 ) );
			  assertTrue( !rel2.HasProperty( _key3 ) );
			  assertEquals( int1, rel1.GetProperty( _key1 ) );
			  assertEquals( string1, rel2.GetProperty( _key1 ) );
			  assertEquals( string2, rel1.GetProperty( _key2 ) );
			  assertEquals( int2, rel2.GetProperty( _key2 ) );

			  Transaction.failure();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRelationshipRemoveProperty()
		 public virtual void TestRelationshipRemoveProperty()
		 {
			  int? int1 = 1;
			  int? int2 = 2;
			  string string1 = "1";
			  string string2 = "2";

			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  Relationship rel1 = node1.CreateRelationshipTo( node2, TEST );
			  Relationship rel2 = node2.CreateRelationshipTo( node1, TEST );
			  // verify that we can rely on PL to remove non existing properties
			  try
			  {
					if ( rel1.RemoveProperty( _key1 ) != null )
					{
						 fail( "Remove of non existing property should return null" );
					}
			  }
			  catch ( NotFoundException )
			  { // OK
			  }
			  try
			  {
					rel1.RemoveProperty( null );
					fail( "Remove null property should throw exception." );
			  }
			  catch ( System.ArgumentException )
			  { // OK
			  }

			  rel1.SetProperty( _key1, int1 );
			  rel2.SetProperty( _key1, string1 );
			  rel1.SetProperty( _key2, string2 );
			  rel2.SetProperty( _key2, int2 );
			  try
			  {
					rel1.RemoveProperty( null );
					fail( "Null argument should result in exception." );
			  }
			  catch ( System.ArgumentException )
			  { // OK
			  }

			  // test remove property
			  assertEquals( int1, rel1.RemoveProperty( _key1 ) );
			  assertEquals( string1, rel2.RemoveProperty( _key1 ) );
			  // test remove of non existing property
			  try
			  {
					if ( rel2.RemoveProperty( _key1 ) != null )
					{
						 fail( "Remove of non existing property should return null" );
					}
			  }
			  catch ( NotFoundException )
			  {
					// have to set rollback only here
					Transaction.failure();
			  }
			  rel1.Delete();
			  rel2.Delete();
			  node1.Delete();
			  node2.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRelationshipChangeProperty()
		 public virtual void TestRelationshipChangeProperty()
		 {
			  int? int1 = 1;
			  int? int2 = 2;
			  string string1 = "1";
			  string string2 = "2";

			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  Relationship rel1 = node1.CreateRelationshipTo( node2, TEST );
			  Relationship rel2 = node2.CreateRelationshipTo( node1, TEST );
			  rel1.SetProperty( _key1, int1 );
			  rel2.SetProperty( _key1, string1 );
			  rel1.SetProperty( _key2, string2 );
			  rel2.SetProperty( _key2, int2 );

			  try
			  {
					rel1.SetProperty( null, null );
					fail( "Null argument should result in exception." );
			  }
			  catch ( System.ArgumentException )
			  { // OK
			  }
			  catch ( NotFoundException )
			  {
					fail( "wrong exception" );
			  }

			  // test type change of existing property
			  // cannot test this for now because of exceptions in PL
			  rel2.SetProperty( _key1, int1 );

			  rel1.Delete();
			  rel2.Delete();
			  node2.Delete();
			  node1.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRelationshipChangeProperty2()
		 public virtual void TestRelationshipChangeProperty2()
		 {
			  int? int1 = 1;
			  int? int2 = 2;
			  string string1 = "1";
			  string string2 = "2";

			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  Relationship rel1 = node1.CreateRelationshipTo( node2, TEST );
			  rel1.SetProperty( _key1, int1 );
			  rel1.SetProperty( _key1, int2 );
			  assertEquals( int2, rel1.GetProperty( _key1 ) );
			  rel1.RemoveProperty( _key1 );
			  rel1.SetProperty( _key1, string1 );
			  rel1.SetProperty( _key1, string2 );
			  assertEquals( string2, rel1.GetProperty( _key1 ) );
			  rel1.RemoveProperty( _key1 );
			  rel1.SetProperty( _key1, true );
			  rel1.SetProperty( _key1, false );
			  assertEquals( false, rel1.GetProperty( _key1 ) );
			  rel1.RemoveProperty( _key1 );

			  rel1.Delete();
			  node2.Delete();
			  node1.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRelGetProperties()
		 public virtual void TestRelGetProperties()
		 {
			  int? int1 = 1;
			  int? int2 = 2;
			  string @string = "3";

			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  Relationship rel1 = node1.CreateRelationshipTo( node2, TEST );
			  try
			  {
					rel1.GetProperty( _key1 );
					fail( "get non existing property didn't throw exception" );
			  }
			  catch ( NotFoundException )
			  { // OK
			  }
			  try
			  {
					rel1.GetProperty( null );
					fail( "get of null key didn't throw exception" );
			  }
			  catch ( System.ArgumentException )
			  { // OK
			  }
			  assertTrue( !rel1.HasProperty( _key1 ) );
			  assertTrue( !rel1.HasProperty( null ) );
			  rel1.SetProperty( _key1, int1 );
			  rel1.SetProperty( _key2, int2 );
			  rel1.SetProperty( _key3, @string );
			  assertTrue( rel1.HasProperty( _key1 ) );
			  assertTrue( rel1.HasProperty( _key2 ) );
			  assertTrue( rel1.HasProperty( _key3 ) );

			  IDictionary<string, object> properties = rel1.AllProperties;
			  assertEquals( properties[_key1], int1 );
			  assertEquals( properties[_key2], int2 );
			  assertEquals( properties[_key3], @string );
			  properties = rel1.GetProperties( _key1, _key2 );
			  assertEquals( properties[_key1], int1 );
			  assertEquals( properties[_key2], int2 );
			  assertFalse( properties.ContainsKey( _key3 ) );

			  properties = node1.Properties;
			  assertTrue( properties.Count == 0 );

			  try
			  {
					node1.GetProperties( ( string[] ) null );
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
					rel1.RemoveProperty( _key3 );
			  }
			  catch ( NotFoundException )
			  {
					fail( "Remove of property failed." );
			  }
			  assertTrue( !rel1.HasProperty( _key3 ) );
			  assertTrue( !rel1.HasProperty( null ) );
			  rel1.Delete();
			  node2.Delete();
			  node1.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testDirectedRelationship()
		 public virtual void TestDirectedRelationship()
		 {
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  Relationship rel2 = node1.CreateRelationshipTo( node2, TEST );
			  Relationship rel3 = node2.CreateRelationshipTo( node1, TEST );
			  Node[] nodes = rel2.Nodes;
			  assertEquals( 2, nodes.Length );
			  assertTrue( nodes[0].Equals( node1 ) && nodes[1].Equals( node2 ) );
			  nodes = rel3.Nodes;
			  assertEquals( 2, nodes.Length );
			  assertTrue( nodes[0].Equals( node2 ) && nodes[1].Equals( node1 ) );
			  assertEquals( node1, rel2.StartNode );
			  assertEquals( node2, rel2.EndNode );
			  assertEquals( node2, rel3.StartNode );
			  assertEquals( node1, rel3.EndNode );

			  Relationship[] relArray = GetRelationshipArray( node1.GetRelationships( TEST, Direction.OUTGOING ) );
			  assertEquals( 1, relArray.Length );
			  assertEquals( rel2, relArray[0] );
			  relArray = GetRelationshipArray( node1.GetRelationships( TEST, Direction.INCOMING ) );
			  assertEquals( 1, relArray.Length );
			  assertEquals( rel3, relArray[0] );

			  relArray = GetRelationshipArray( node2.GetRelationships( TEST, Direction.OUTGOING ) );
			  assertEquals( 1, relArray.Length );
			  assertEquals( rel3, relArray[0] );
			  relArray = GetRelationshipArray( node2.GetRelationships( TEST, Direction.INCOMING ) );
			  assertEquals( 1, relArray.Length );
			  assertEquals( rel2, relArray[0] );

			  rel2.Delete();
			  rel3.Delete();
			  node1.Delete();
			  node2.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRollbackDeleteRelationship()
		 public virtual void TestRollbackDeleteRelationship()
		 {
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  Relationship rel1 = node1.CreateRelationshipTo( node2, TEST );
			  NewTransaction();
			  node1.Delete();
			  rel1.Delete();
			  Transaction.failure();
			  Transaction.close();
			  Transaction = GraphDb.beginTx();
			  node1.Delete();
			  node2.Delete();
			  rel1.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCreateRelationshipWithCommits()
		 public virtual void TestCreateRelationshipWithCommits() // throws NotFoundException
		 {
			  Node n1 = GraphDb.createNode();
			  NewTransaction();
			  n1 = GraphDb.getNodeById( n1.Id );
			  Node n2 = GraphDb.createNode();
			  n1.CreateRelationshipTo( n2, TEST );
			  NewTransaction();
			  Relationship[] relArray = GetRelationshipArray( n1.Relationships );
			  assertEquals( 1, relArray.Length );
			  relArray = GetRelationshipArray( n1.Relationships );
			  relArray[0].Delete();
			  n1.Delete();
			  n2.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAddPropertyThenDelete()
		 public virtual void TestAddPropertyThenDelete()
		 {
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  Relationship rel = node1.CreateRelationshipTo( node2, TEST );
			  rel.SetProperty( "test", "test" );
			  NewTransaction();
			  rel.SetProperty( "test2", "test2" );
			  rel.Delete();
			  node1.Delete();
			  node2.Delete();
			  NewTransaction();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRelationshipIsType()
		 public virtual void TestRelationshipIsType()
		 {
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  Relationship rel = node1.CreateRelationshipTo( node2, TEST );
			  assertTrue( rel.IsType( TEST ) );
			  assertTrue( rel.IsType( TEST.name ) );
			  assertFalse( rel.IsType( TEST_TRAVERSAL ) );
			  rel.Delete();
			  node1.Delete();
			  node2.Delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testChangeProperty()
		 public virtual void TestChangeProperty()
		 {
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  Relationship rel = node1.CreateRelationshipTo( node2, TEST );
			  rel.SetProperty( "test", "test1" );
			  NewTransaction();
			  rel.SetProperty( "test", "test2" );
			  rel.RemoveProperty( "test" );
			  rel.SetProperty( "test", "test3" );
			  assertEquals( "test3", rel.GetProperty( "test" ) );
			  rel.RemoveProperty( "test" );
			  rel.SetProperty( "test", "test4" );
			  NewTransaction();
			  assertEquals( "test4", rel.GetProperty( "test" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testChangeProperty2()
		 public virtual void TestChangeProperty2()
		 {
			  // Create relationship with "test"="test1"
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  Relationship rel = node1.CreateRelationshipTo( node2, TEST );
			  rel.SetProperty( "test", "test1" );
			  NewTransaction(); // commit

			  // Remove "test" and set "test"="test3" instead
			  rel.RemoveProperty( "test" );
			  rel.SetProperty( "test", "test3" );
			  assertEquals( "test3", rel.GetProperty( "test" ) );
			  NewTransaction(); // commit

			  // Remove "test" and set "test"="test4" instead
			  assertEquals( "test3", rel.GetProperty( "test" ) );
			  rel.RemoveProperty( "test" );
			  rel.SetProperty( "test", "test4" );
			  NewTransaction(); // commit

			  // Should still be "test4"
			  assertEquals( "test4", rel.GetProperty( "test" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void makeSureLazyLoadingRelationshipsWorksEvenIfOtherIteratorAlsoLoadsInTheSameIteration()
		 public virtual void MakeSureLazyLoadingRelationshipsWorksEvenIfOtherIteratorAlsoLoadsInTheSameIteration()
		 {
			  int numEdges = 100;

			  /* create 256 nodes */
			  GraphDatabaseService graphDB = GraphDb;
			  Node[] nodes = new Node[256];
			  for ( int numNodes = 0; numNodes < nodes.Length; numNodes += 1 )
			  {
					nodes[numNodes] = graphDB.CreateNode();
			  }
			  NewTransaction();

			  /* create random outgoing relationships from node 5 */
			  Node hub = nodes[4];
			  int nextID = 7;

			  RelationshipType outtie = withName( "outtie" );
			  RelationshipType innie = withName( "innie" );
			  for ( int k = 0; k < numEdges; k++ )
			  {
					Node neighbor = nodes[nextID];
					nextID += 7;
					nextID &= 255;
					if ( nextID == 0 )
					{
						 nextID = 1;
					}
					hub.CreateRelationshipTo( neighbor, outtie );
			  }
			  NewTransaction();

			  /* create random incoming relationships to node 5 */
			  for ( int k = 0; k < numEdges; k += 1 )
			  {
					Node neighbor = nodes[nextID];
					nextID += 7;
					nextID &= 255;
					if ( nextID == 0 )
					{
						 nextID = 1;
					}
					neighbor.CreateRelationshipTo( hub, innie );
			  }
			  Commit();

			  NewTransaction();
			  hub = graphDB.GetNodeById( hub.Id );

			  int count = 0;
			  foreach ( Relationship ignore in hub.Relationships )
			  {
					count += ( int )Iterables.count( hub.Relationships );
			  }
			  assertEquals( 40000, count );

			  count = 0;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: for (@SuppressWarnings("unused") org.neo4j.graphdb.Relationship r1 : hub.getRelationships())
			  foreach ( Relationship r1 in hub.Relationships )
			  {
					count += ( int )Iterables.count( hub.Relationships );
			  }
			  assertEquals( 40000, count );
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createRelationshipAfterClearedCache()
		 public virtual void CreateRelationshipAfterClearedCache()
		 {
			  // Assumes relationship grab size 100
			  Node node1 = GraphDb.createNode();
			  Node node2 = GraphDb.createNode();
			  int expectedCount = 0;
			  for ( int i = 0; i < 150; i++ )
			  {
					node1.CreateRelationshipTo( node2, TEST );
					expectedCount++;
			  }
			  NewTransaction();
			  for ( int i = 0; i < 50; i++ )
			  {
					node1.CreateRelationshipTo( node2, TEST );
					expectedCount++;
			  }
			  assertEquals( expectedCount, Iterables.count( node1.Relationships ) );
			  NewTransaction();
			  assertEquals( expectedCount, Iterables.count( node1.Relationships ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void getAllRelationships()
		 public virtual void getAllRelationships()
		 {
			  ISet<Relationship> existingRelationships = Iterables.addToCollection( GraphDb.AllRelationships, new HashSet<Relationship>() );

			  ISet<Relationship> createdRelationships = new HashSet<Relationship>();
			  Node node = GraphDb.createNode();
			  for ( int i = 0; i < 100; i++ )
			  {
					createdRelationships.Add( node.CreateRelationshipTo( GraphDb.createNode(), TEST ) );
			  }
			  NewTransaction();

			  ISet<Relationship> allRelationships = new HashSet<Relationship>();
			  allRelationships.addAll( existingRelationships );
			  allRelationships.addAll( createdRelationships );

			  int count = 0;
			  foreach ( Relationship rel in GraphDb.AllRelationships )
			  {
					assertTrue( "Unexpected rel " + rel + ", expected one of " + allRelationships, allRelationships.Contains( rel ) );
					count++;
			  }
			  assertEquals( allRelationships.Count, count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createAndClearCacheBeforeCommit()
		 public virtual void CreateAndClearCacheBeforeCommit()
		 {
			  Node node = GraphDb.createNode();
			  node.CreateRelationshipTo( GraphDb.createNode(), TEST );
			  assertEquals( 1, Iterables.count( node.Relationships ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void setPropertyAndClearCacheBeforeCommit()
		 public virtual void SetPropertyAndClearCacheBeforeCommit()
		 {
			  Node node = GraphDb.createNode();
			  node.SetProperty( "name", "Test" );
			  assertEquals( "Test", node.GetProperty( "name" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotGetTheSameRelationshipMoreThanOnceWhenAskingForTheSameTypeMultipleTimes()
		 public virtual void ShouldNotGetTheSameRelationshipMoreThanOnceWhenAskingForTheSameTypeMultipleTimes()
		 {
			  // given
			  Node node = GraphDb.createNode();
			  node.CreateRelationshipTo( GraphDb.createNode(), withName("FOO") );

			  // when
			  long relationships = Iterables.count( node.GetRelationships( withName( "FOO" ), withName( "FOO" ) ) );

			  // then
			  assertEquals( 1, relationships );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLoadAllRelationships()
		 public virtual void ShouldLoadAllRelationships()
		 {
			  // GIVEN
			  GraphDatabaseService db = GraphDbAPI;
			  Node node;
			  using ( Transaction tx = Db.beginTx() )
			  {
					node = Db.createNode();
					for ( int i = 0; i < 112; i++ )
					{
						 node.CreateRelationshipTo( Db.createNode(), TEST );
						 Db.createNode().createRelationshipTo(node, TEST);
					}
					tx.Success();
			  }
			  // WHEN
			  long one;
			  long two;
			  using ( Transaction tx = Db.beginTx() )
			  {
					one = Iterables.count( node.GetRelationships( TEST, Direction.OUTGOING ) );
					two = Iterables.count( node.GetRelationships( TEST, Direction.OUTGOING ) );
					tx.Success();
			  }

			  // THEN
			  assertEquals( two, one );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.neo4j.graphdb.NotFoundException.class) public void deletionOfSameRelationshipTwiceInOneTransactionShouldNotRollbackIt()
		 public virtual void DeletionOfSameRelationshipTwiceInOneTransactionShouldNotRollbackIt()
		 {
			  // Given
			  GraphDatabaseService db = GraphDb;

			  // transaction is opened by test
			  Node node1 = Db.createNode();
			  Node node2 = Db.createNode();
			  Relationship relationship = node1.CreateRelationshipTo( node2, TEST );
			  Commit();

			  // When
			  Exception exceptionThrownBySecondDelete = null;

			  using ( Transaction tx = Db.beginTx() )
			  {
					relationship.Delete();
					try
					{
						 relationship.Delete();
					}
					catch ( System.InvalidOperationException e )
					{
						 exceptionThrownBySecondDelete = e;
					}
					tx.Success();
			  }

			  // Then
			  assertNotNull( exceptionThrownBySecondDelete );

			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.getRelationshipById( relationship.Id ); // should throw NotFoundException
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = org.neo4j.graphdb.NotFoundException.class) public void deletionOfAlreadyDeletedRelationshipShouldThrow()
		 public virtual void DeletionOfAlreadyDeletedRelationshipShouldThrow()
		 {
			  // Given
			  GraphDatabaseService db = GraphDb;

			  // transaction is opened by test
			  Node node1 = Db.createNode();
			  Node node2 = Db.createNode();
			  Relationship relationship = node1.CreateRelationshipTo( node2, TEST );
			  Commit();

			  using ( Transaction tx = Db.beginTx() )
			  {
					relationship.Delete();
					tx.Success();
			  }

			  // When
			  using ( Transaction tx = Db.beginTx() )
			  {
					relationship.Delete();
					tx.Success();
			  }
		 }

		 private void AssertHasNext( ResourceIterable<Relationship> relationships )
		 {
			  using ( ResourceIterator<Relationship> iterator = relationships.GetEnumerator() )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( iterator.hasNext() );
			  }
		 }
	}

}