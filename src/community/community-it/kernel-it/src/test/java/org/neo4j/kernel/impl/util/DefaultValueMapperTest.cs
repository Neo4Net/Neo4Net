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
namespace Neo4Net.Kernel.impl.util
{

	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Path = Neo4Net.GraphDb.Path;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Iterables = Neo4Net.Collections.Helpers.Iterables;
	using Iterators = Neo4Net.Collections.Helpers.Iterators;
	using EmbeddedProxySPI = Neo4Net.Kernel.impl.core.EmbeddedProxySPI;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using Values = Neo4Net.Values.Storable.Values;
	using NodeValue = Neo4Net.Values.@virtual.NodeValue;
	using RelationshipValue = Neo4Net.Values.@virtual.RelationshipValue;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.collection.IsCollectionWithSize.hasSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.stringValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.@virtual.VirtualValues.EMPTY_MAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.@virtual.VirtualValues.nodeValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.@virtual.VirtualValues.path;

	public class DefaultValueMapperTest
	{
		 internal IGraphDatabaseService Db;
		 private DefaultValueMapper _mapper;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  Db = ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
			  _mapper = new DefaultValueMapper( ( EmbeddedProxySPI ) Db );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleSingleNodePath()
		 public virtual void ShouldHandleSingleNodePath()
		 {
			  // Given
			  Node node;
			  using ( Transaction tx = Db.beginTx() )
			  {
					node = Db.createNode();
					tx.Success();
			  }

			  // When
			  Path mapped = _mapper.mapPath( path( AsNodeValues( node ), AsRelationshipsValues() ) );

			  // Then
			  using ( Transaction ignore = Db.beginTx() )
			  {
					assertThat( mapped.Length(), equalTo(0) );
					assertThat( mapped.StartNode(), equalTo(node) );
					assertThat( mapped.EndNode(), equalTo(node) );
					assertThat( Iterables.asList( mapped.Relationships() ), hasSize(0) );
					assertThat( Iterables.asList( mapped.ReverseRelationships() ), hasSize(0) );
					assertThat( Iterables.asList( mapped.Nodes() ), equalTo(singletonList(node)) );
					assertThat( Iterables.asList( mapped.ReverseNodes() ), equalTo(singletonList(node)) );
					assertThat( mapped.LastRelationship(), nullValue() );
					assertThat( Iterators.asList( mapped.GetEnumerator() ), equalTo(singletonList(node)) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleSingleRelationshipPath()
		 public virtual void ShouldHandleSingleRelationshipPath()
		 {
			  // Given
			  Node start, end;
			  Relationship relationship;
			  using ( Transaction tx = Db.beginTx() )
			  {
					start = Db.createNode();
					end = Db.createNode();
					relationship = start.CreateRelationshipTo( end, RelationshipType.withName( "R" ) );
					tx.Success();
			  }

			  // When
			  Path mapped = _mapper.mapPath( path( AsNodeValues( start, end ), AsRelationshipsValues( relationship ) ) );

			  // Then
			  using ( Transaction ignore = Db.beginTx() )
			  {
					assertThat( mapped.Length(), equalTo(1) );
					assertThat( mapped.StartNode(), equalTo(start) );
					assertThat( mapped.EndNode(), equalTo(end) );
					assertThat( Iterables.asList( mapped.Relationships() ), equalTo(singletonList(relationship)) );
					assertThat( Iterables.asList( mapped.ReverseRelationships() ), equalTo(singletonList(relationship)) );
					assertThat( Iterables.asList( mapped.Nodes() ), equalTo(Arrays.asList(start, end)) );
					assertThat( Iterables.asList( mapped.ReverseNodes() ), equalTo(Arrays.asList(end, start)) );
					assertThat( mapped.LastRelationship(), equalTo(relationship) );
					assertThat( Iterators.asList( mapped.GetEnumerator() ), equalTo(Arrays.asList(start, relationship, end)) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleLongPath()
		 public virtual void ShouldHandleLongPath()
		 {
			  // Given
			  Node a, b, c, d, e;
			  Relationship r1, r2, r3, r4;
			  using ( Transaction tx = Db.beginTx() )
			  {
					a = Db.createNode();
					b = Db.createNode();
					c = Db.createNode();
					d = Db.createNode();
					e = Db.createNode();
					r1 = a.CreateRelationshipTo( b, RelationshipType.withName( "R" ) );
					r2 = b.CreateRelationshipTo( c, RelationshipType.withName( "R" ) );
					r3 = c.CreateRelationshipTo( d, RelationshipType.withName( "R" ) );
					r4 = d.CreateRelationshipTo( e, RelationshipType.withName( "R" ) );
					tx.Success();
			  }

			  // When
			  Path mapped = _mapper.mapPath( path( AsNodeValues( a, b, c, d, e ), AsRelationshipsValues( r1, r2, r3, r4 ) ) );

			  // Then
			  using ( Transaction ignore = Db.beginTx() )
			  {
					assertThat( mapped.Length(), equalTo(4) );
					assertThat( mapped.StartNode(), equalTo(a) );
					assertThat( mapped.EndNode(), equalTo(e) );
					assertThat( Iterables.asList( mapped.Relationships() ), equalTo(Arrays.asList(r1, r2, r3, r4)) );
					assertThat( Iterables.asList( mapped.ReverseRelationships() ), equalTo(Arrays.asList(r4, r3, r2, r1)) );
					assertThat( Iterables.asList( mapped.Nodes() ), equalTo(Arrays.asList(a, b, c, d, e)) );
					assertThat( Iterables.asList( mapped.ReverseNodes() ), equalTo(Arrays.asList(e, d, c, b, a)) );
					assertThat( mapped.LastRelationship(), equalTo(r4) );
					assertThat( Iterators.asList( mapped.GetEnumerator() ), equalTo(Arrays.asList(a, r1, b, r2, c, r3, d, r4, e)) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMapDirectRelationship()
		 public virtual void ShouldMapDirectRelationship()
		 {
			  // Given
			  Node start, end;
			  Relationship relationship;
			  using ( Transaction tx = Db.beginTx() )
			  {
					start = Db.createNode();
					end = Db.createNode();
					relationship = start.CreateRelationshipTo( end, RelationshipType.withName( "R" ) );
					tx.Success();
			  }
			  RelationshipValue relationshipValue = VirtualValues.relationshipValue( relationship.Id, nodeValue( start.Id, Values.EMPTY_TEXT_ARRAY, EMPTY_MAP ), nodeValue( start.Id, Values.EMPTY_TEXT_ARRAY, EMPTY_MAP ), stringValue( "R" ), EMPTY_MAP );

			  // When
			  Relationship coreAPIRelationship = _mapper.mapRelationship( relationshipValue );

			  // Then
			  using ( Transaction ignore = Db.beginTx() )
			  {
					assertThat( coreAPIRelationship.Id, equalTo( relationship.Id ) );
					assertThat( coreAPIRelationship.StartNode, equalTo( start ) );
					assertThat( coreAPIRelationship.EndNode, equalTo( end ) );
			  }
		 }

		 private NodeValue[] AsNodeValues( params Node[] nodes )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return java.util.nodes.Select( ValueUtils.fromNodeProxy ).ToArray( NodeValue[]::new );
		 }

		 private RelationshipValue[] AsRelationshipsValues( params Relationship[] relationships )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return java.util.relationships.Select( ValueUtils.fromRelationshipProxy ).ToArray( RelationshipValue[]::new );
		 }
	}

}