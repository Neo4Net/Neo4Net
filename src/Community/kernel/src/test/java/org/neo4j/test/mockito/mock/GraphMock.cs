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
namespace Neo4Net.Test.mockito.mock
{
	using Answer = org.mockito.stubbing.Answer;


	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Path = Neo4Net.Graphdb.Path;
	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Iterables = Neo4Net.Helpers.Collection.Iterables;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.reverse;

	public class GraphMock
	{
		 private GraphMock()
		 {
		 }

		 public static Label[] Labels( params string[] names )
		 {
			  Label[] labels = new Label[names.Length];
			  for ( int i = 0; i < labels.Length; i++ )
			  {
					labels[i] = Label.label( names[i] );
			  }
			  return labels;
		 }

		 public static Node Node( long id, Label[] labels, params Property[] properties )
		 {
			  return MockNode( id, labels, Properties.PropertiesConflict( properties ) );
		 }

		 public static Node Node( long id, Properties properties, params string[] labels )
		 {
			  return MockNode( id, labels( labels ), properties );
		 }

		 public static Relationship Relationship( long id, Properties properties, Node start, string type, Node end )
		 {
			  return MockRelationship( id, start, type, end, properties );
		 }

		 public static Relationship Relationship( long id, Node start, string type, Node end, params Property[] properties )
		 {
			  return MockRelationship( id, start, type, end, Properties.PropertiesConflict( properties ) );
		 }

		 public static Link Link( Relationship relationship, Node node )
		 {
			  return Link.LinkConflict( relationship, node );
		 }

		 public static Path Path( Node node, params Link[] links )
		 {
			  IList<Node> nodes = new List<Node>( links.Length + 1 );
			  IList<Relationship> relationships = new List<Relationship>( links.Length );
			  IList<PropertyContainer> mixed = new List<PropertyContainer>( links.Length * 2 + 1 );
			  nodes.Add( node );
			  mixed.Add( node );
			  Path path = mock( typeof( Path ) );
			  when( path.StartNode() ).thenReturn(node);
			  Relationship last = null;
			  foreach ( Link link in links )
			  {
					last = link.Relationship;
					relationships.Add( last );
					mixed.Add( last );

					node = link.CheckNode( node );
					nodes.Add( node );
					mixed.Add( node );
			  }
			  when( path.EndNode() ).thenReturn(node);
			  when( path.GetEnumerator() ).thenAnswer(WithIteratorOf(mixed));
			  when( path.Nodes() ).thenReturn(nodes);
			  when( path.Relationships() ).thenReturn(relationships);
			  when( path.LastRelationship() ).thenReturn(last);
			  when( path.Length() ).thenReturn(links.Length);
			  when( path.ReverseNodes() ).thenReturn(reverse(nodes));
			  when( path.ReverseRelationships() ).thenReturn(reverse(relationships));
			  return path;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static <T> org.mockito.stubbing.Answer<java.util.Iterator<T>> withIteratorOf(final Iterable<T> iterable)
		 private static Answer<IEnumerator<T>> WithIteratorOf<T>( IEnumerable<T> iterable )
		 {
			  return invocation => iterable.GetEnumerator();
		 }

		 private static Node MockNode( long id, Label[] labels, Properties properties )
		 {
			  Node node = MockPropertyContainer( typeof( Node ), properties );
			  when( node.Id ).thenReturn( id );
			  when( node.Labels ).thenReturn( Iterables.asResourceIterable( asList( labels ) ) );
			  return node;
		 }

		 private static Relationship MockRelationship( long id, Node start, string type, Node end, Properties properties )
		 {
			  Relationship relationship = MockPropertyContainer( typeof( Relationship ), properties );
			  when( relationship.Id ).thenReturn( id );
			  when( relationship.StartNode ).thenReturn( start );
			  when( relationship.EndNode ).thenReturn( end );
			  when( relationship.Type ).thenReturn( RelationshipType.withName( type ) );
			  return relationship;
		 }

		 private static T MockPropertyContainer<T>( Type type, Properties properties ) where T : Neo4Net.Graphdb.PropertyContainer
		 {
				 type = typeof( T );
			  T container = mock( type );
			  when( container.getProperty( anyString() ) ).thenAnswer(properties);
			  when( container.getProperty( anyString(), any() ) ).thenAnswer(properties);
			  when( container.PropertyKeys ).thenReturn( properties );
			  when( container.AllProperties ).thenReturn( properties.GetProperties() );
			  return container;
		 }
	}

}