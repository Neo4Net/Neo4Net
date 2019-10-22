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
namespace Neo4Net.Server.rest.repr
{
	using Test = org.junit.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.repr.RepresentationTestAccess.serialize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.repr.RepresentationTestBase.assertUriMatches;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.repr.RepresentationTestBase.uriPattern;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.mock.GraphMock.node;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.mock.Properties.properties;

	public class NodeRepresentationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveSelfLink()
		 public virtual void ShouldHaveSelfLink()
		 {
			  assertUriMatches( uriPattern( "" ), Noderep( 1234 ).selfUri() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveAllRelationshipsLink()
		 public virtual void ShouldHaveAllRelationshipsLink()
		 {
			  assertUriMatches( uriPattern( "/relationships/all" ), Noderep( 1234 ).allRelationshipsUri() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveIncomingRelationshipsLink()
		 public virtual void ShouldHaveIncomingRelationshipsLink()
		 {
			  assertUriMatches( uriPattern( "/relationships/in" ), Noderep( 1234 ).incomingRelationshipsUri() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveOutgoingRelationshipsLink()
		 public virtual void ShouldHaveOutgoingRelationshipsLink()
		 {
			  assertUriMatches( uriPattern( "/relationships/out" ), Noderep( 1234 ).outgoingRelationshipsUri() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveAllTypedRelationshipsLinkTemplate()
		 public virtual void ShouldHaveAllTypedRelationshipsLinkTemplate()
		 {
			  assertUriMatches( uriPattern( "/relationships/all/\\{-list\\|&\\|types\\}" ), Noderep( 1234 ).allTypedRelationshipsUriTemplate() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveIncomingTypedRelationshipsLinkTemplate()
		 public virtual void ShouldHaveIncomingTypedRelationshipsLinkTemplate()
		 {
			  assertUriMatches( uriPattern( "/relationships/in/\\{-list\\|&\\|types\\}" ), Noderep( 1234 ).incomingTypedRelationshipsUriTemplate() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveOutgoingTypedRelationshipsLinkTemplate()
		 public virtual void ShouldHaveOutgoingTypedRelationshipsLinkTemplate()
		 {
			  assertUriMatches( uriPattern( "/relationships/out/\\{-list\\|&\\|types\\}" ), Noderep( 1234 ).outgoingTypedRelationshipsUriTemplate() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveRelationshipCreationLink()
		 public virtual void ShouldHaveRelationshipCreationLink()
		 {
			  assertUriMatches( uriPattern( "/relationships" ), Noderep( 1234 ).relationshipCreationUri() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHavePropertiesLink()
		 public virtual void ShouldHavePropertiesLink()
		 {
			  assertUriMatches( uriPattern( "/properties" ), Noderep( 1234 ).propertiesUri() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHavePropertyLinkTemplate()
		 public virtual void ShouldHavePropertyLinkTemplate()
		 {
			  assertUriMatches( uriPattern( "/properties/\\{key\\}" ), Noderep( 1234 ).propertyUriTemplate() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveTraverseLinkTemplate()
		 public virtual void ShouldHaveTraverseLinkTemplate()
		 {
			  assertUriMatches( uriPattern( "/traverse/\\{returnType\\}" ), Noderep( 1234 ).traverseUriTemplate() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerialiseToMap()
		 public virtual void ShouldSerialiseToMap()
		 {
			  IDictionary<string, object> repr = serialize( Noderep( 1234 ) );
			  assertNotNull( repr );
			  VerifySerialisation( repr );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveLabelsLink()
		 public virtual void ShouldHaveLabelsLink()
		 {
			  assertUriMatches( uriPattern( "/labels" ), Noderep( 1234 ).labelsUriTemplate() );
		 }

		 private NodeRepresentation Noderep( long id )
		 {
			  return new NodeRepresentation( node( id, properties(), "Label" ) );
		 }

		 public static void VerifySerialisation( IDictionary<string, object> noderep )
		 {
			  assertUriMatches( uriPattern( "" ), noderep["self"].ToString() );
			  assertUriMatches( uriPattern( "/relationships" ), noderep["create_relationship"].ToString() );
			  assertUriMatches( uriPattern( "/relationships/all" ), noderep["all_relationships"].ToString() );
			  assertUriMatches( uriPattern( "/relationships/in" ), noderep["incoming_relationships"].ToString() );
			  assertUriMatches( uriPattern( "/relationships/out" ), noderep["outgoing_relationships"].ToString() );
			  assertUriMatches( uriPattern( "/relationships/all/\\{-list\\|&\\|types\\}" ), ( string ) noderep["all_typed_relationships"] );
			  assertUriMatches( uriPattern( "/relationships/in/\\{-list\\|&\\|types\\}" ), ( string ) noderep["incoming_typed_relationships"] );
			  assertUriMatches( uriPattern( "/relationships/out/\\{-list\\|&\\|types\\}" ), ( string ) noderep["outgoing_typed_relationships"] );
			  assertUriMatches( uriPattern( "/properties" ), noderep["properties"].ToString() );
			  assertUriMatches( uriPattern( "/properties/\\{key\\}" ), ( string ) noderep["property"] );
			  assertUriMatches( uriPattern( "/traverse/\\{returnType\\}" ), ( string ) noderep["traverse"] );
			  assertUriMatches( uriPattern( "/labels" ), ( string ) noderep["labels"] );
			  assertNotNull( noderep["data"] );
			  System.Collections.IDictionary metadata = ( System.Collections.IDictionary ) noderep["metadata"];
			  System.Collections.IList labels = ( System.Collections.IList ) metadata["labels"];
//JAVA TO C# CONVERTER WARNING: LINQ 'SequenceEqual' is not always identical to Java AbstractList 'equals':
//ORIGINAL LINE: assertTrue(labels.isEmpty() || labels.equals(asList("Label")));
			  assertTrue( labels.Count == 0 || labels.SequenceEqual( asList( "Label" ) ) );
			  assertTrue( ( ( Number ) metadata["id"] ).longValue() >= 0 );
		 }
	}

}