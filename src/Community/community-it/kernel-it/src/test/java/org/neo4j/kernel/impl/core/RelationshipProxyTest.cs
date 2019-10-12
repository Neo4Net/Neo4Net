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
	using RandomStringUtils = org.apache.commons.lang3.RandomStringUtils;
	using RandomUtils = org.apache.commons.lang3.RandomUtils;
	using Test = org.junit.Test;

	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using NamedToken = Neo4Net.@internal.Kernel.Api.NamedToken;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.RelationshipType.withName;

	public class RelationshipProxyTest : PropertyContainerProxyTest
	{
		 protected internal override long CreatePropertyContainer()
		 {
			  return Db.createNode().createRelationshipTo(Db.createNode(), withName("FOO")).Id;
		 }

		 protected internal override PropertyContainer LookupPropertyContainer( long id )
		 {
			  return Db.getRelationshipById( id );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToReferToIdsBeyondMaxInt()
		 public virtual void ShouldBeAbleToReferToIdsBeyondMaxInt()
		 {
			  // GIVEN
			  EmbeddedProxySPI actions = mock( typeof( EmbeddedProxySPI ) );
			  when( actions.NewNodeProxy( anyLong() ) ).then(invocation => NodeWithId(invocation.getArgument(0)));
			  when( actions.GetRelationshipTypeById( anyInt() ) ).then(invocation => new NamedToken("whatever", invocation.getArgument(0)));

			  long[] ids = new long[]{ 1437589437, 2047587483, 2147496246L, 2147342921, 3276473721L, 4762746373L, 57587348738L, 59892898932L };
			  int[] types = new int[]{ 0, 10, 101, 3024, 20123, 45008 };

			  // WHEN/THEN
			  for ( int i = 0; i < ids.Length - 2; i++ )
			  {
					long id = ids[i];
					long nodeId1 = ids[i + 1];
					long nodeId2 = ids[i + 2];
					int type = types[i];
					VerifyIds( actions, id, nodeId1, type, nodeId2 );
					VerifyIds( actions, id, nodeId2, type, nodeId1 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPrintCypherEsqueRelationshipToString()
		 public virtual void ShouldPrintCypherEsqueRelationshipToString()
		 {
			  // GIVEN
			  Node start;
			  Node end;
			  RelationshipType type = RelationshipType.withName( "NICE" );
			  Relationship relationship;
			  using ( Transaction tx = Db.beginTx() )
			  {
					// GIVEN
					start = Db.createNode();
					end = Db.createNode();
					relationship = start.CreateRelationshipTo( end, type );
					tx.Success();

					// WHEN
					string toString = relationship.ToString();

					// THEN
					assertEquals( "(" + start.Id + ")-[" + type + "," + relationship.Id + "]->(" + end.Id + ")", toString );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createDropRelationshipLongStringProperty()
		 public virtual void CreateDropRelationshipLongStringProperty()
		 {
			  Label markerLabel = Label.label( "marker" );
			  string testPropertyKey = "testProperty";
			  string propertyValue = RandomStringUtils.randomAscii( 255 );

			  using ( Transaction tx = Db.beginTx() )
			  {
					Node start = Db.createNode( markerLabel );
					Node end = Db.createNode( markerLabel );
					Relationship relationship = start.CreateRelationshipTo( end, withName( "type" ) );
					relationship.SetProperty( testPropertyKey, propertyValue );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					Relationship relationship = Db.getRelationshipById( 0 );
					assertEquals( propertyValue, relationship.GetProperty( testPropertyKey ) );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					Relationship relationship = Db.getRelationshipById( 0 );
					relationship.RemoveProperty( testPropertyKey );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					Relationship relationship = Db.getRelationshipById( 0 );
					assertFalse( relationship.HasProperty( testPropertyKey ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createDropRelationshipLongArrayProperty()
		 public virtual void CreateDropRelationshipLongArrayProperty()
		 {
			  Label markerLabel = Label.label( "marker" );
			  string testPropertyKey = "testProperty";
			  sbyte[] propertyValue = RandomUtils.NextBytes( 1024 );

			  using ( Transaction tx = Db.beginTx() )
			  {
					Node start = Db.createNode( markerLabel );
					Node end = Db.createNode( markerLabel );
					Relationship relationship = start.CreateRelationshipTo( end, withName( "type" ) );
					relationship.SetProperty( testPropertyKey, propertyValue );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					Relationship relationship = Db.getRelationshipById( 0 );
					assertArrayEquals( propertyValue, ( sbyte[] ) relationship.GetProperty( testPropertyKey ) );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					Relationship relationship = Db.getRelationshipById( 0 );
					relationship.RemoveProperty( testPropertyKey );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					Relationship relationship = Db.getRelationshipById( 0 );
					assertFalse( relationship.HasProperty( testPropertyKey ) );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToForceTypeChangeOfProperty()
		 public virtual void ShouldBeAbleToForceTypeChangeOfProperty()
		 {
			  // Given
			  Relationship relationship;
			  using ( Transaction tx = Db.beginTx() )
			  {
					relationship = Db.createNode().createRelationshipTo(Db.createNode(), withName("R"));
					relationship.SetProperty( "prop", 1337 );
					tx.Success();
			  }

			  // When
			  using ( Transaction tx = Db.beginTx() )
			  {
					relationship.SetProperty( "prop", 1337.0 );
					tx.Success();
			  }

			  // Then
			  using ( Transaction ignore = Db.beginTx() )
			  {
					assertThat( relationship.GetProperty( "prop" ), instanceOf( typeof( Double ) ) );
			  }
		 }

		 private void VerifyIds( EmbeddedProxySPI actions, long relationshipId, long nodeId1, int typeId, long nodeId2 )
		 {
			  RelationshipProxy proxy = new RelationshipProxy( actions, relationshipId, nodeId1, typeId, nodeId2 );
			  assertEquals( relationshipId, proxy.Id );
			  // our mock above is known to return RelationshipTypeToken
			  assertEquals( nodeId1, proxy.StartNode.Id );
			  assertEquals( nodeId1, proxy.StartNodeId );
			  assertEquals( nodeId2, proxy.EndNode.Id );
			  assertEquals( nodeId2, proxy.EndNodeId );
			  assertEquals( nodeId2, proxy.GetOtherNode( NodeWithId( nodeId1 ) ).Id );
			  assertEquals( nodeId2, proxy.GetOtherNodeId( nodeId1 ) );
			  assertEquals( nodeId1, proxy.GetOtherNode( NodeWithId( nodeId2 ) ).Id );
			  assertEquals( nodeId1, proxy.GetOtherNodeId( nodeId2 ) );
		 }

		 private Node NodeWithId( long id )
		 {
			  NodeProxy node = mock( typeof( NodeProxy ) );
			  when( node.Id ).thenReturn( id );
			  return node;
		 }
	}

}