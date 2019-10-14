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
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.repr.RepresentationTestAccess.serialize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.repr.RepresentationTestBase.NODE_URI_PATTERN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.repr.RepresentationTestBase.RELATIONSHIP_URI_PATTERN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.repr.RepresentationTestBase.assertUriMatches;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.mock.GraphMock.node;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.mock.GraphMock.relationship;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.mock.Properties.properties;

	public class RelationshipRepresentationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveSelfLink()
		 public virtual void ShouldHaveSelfLink()
		 {
			  assertUriMatches( RELATIONSHIP_URI_PATTERN, Relrep( 1234 ).selfUri() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveType()
		 public virtual void ShouldHaveType()
		 {
			  assertNotNull( Relrep( 1234 ).Type );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveStartNodeLink()
		 public virtual void ShouldHaveStartNodeLink()
		 {
			  assertUriMatches( NODE_URI_PATTERN, Relrep( 1234 ).startNodeUri() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveEndNodeLink()
		 public virtual void ShouldHaveEndNodeLink()
		 {
			  assertUriMatches( NODE_URI_PATTERN, Relrep( 1234 ).endNodeUri() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHavePropertiesLink()
		 public virtual void ShouldHavePropertiesLink()
		 {
			  assertUriMatches( RELATIONSHIP_URI_PATTERN + "/properties", Relrep( 1234 ).propertiesUri() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHavePropertyLinkTemplate()
		 public virtual void ShouldHavePropertyLinkTemplate()
		 {
			  assertUriMatches( RELATIONSHIP_URI_PATTERN + "/properties/\\{key\\}", Relrep( 1234 ).propertyUriTemplate() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerialiseToMap()
		 public virtual void ShouldSerialiseToMap()
		 {
			  IDictionary<string, object> repr = serialize( Relrep( 1234 ) );
			  assertNotNull( repr );
			  VerifySerialisation( repr );
		 }

		 private RelationshipRepresentation Relrep( long id )
		 {
			  return new RelationshipRepresentation( relationship( id, node( 0, properties() ), "LOVES", node(1, properties()) ) );
		 }

		 public static void VerifySerialisation( IDictionary<string, object> relrep )
		 {
			  assertUriMatches( RELATIONSHIP_URI_PATTERN, relrep["self"].ToString() );
			  assertUriMatches( NODE_URI_PATTERN, relrep["start"].ToString() );
			  assertUriMatches( NODE_URI_PATTERN, relrep["end"].ToString() );
			  assertNotNull( relrep["type"] );
			  assertUriMatches( RELATIONSHIP_URI_PATTERN + "/properties", relrep["properties"].ToString() );
			  assertUriMatches( RELATIONSHIP_URI_PATTERN + "/properties/\\{key\\}", ( string ) relrep["property"] );
			  assertNotNull( relrep["data"] );
			  assertNotNull( relrep["metadata"] );
			  System.Collections.IDictionary metadata = ( System.Collections.IDictionary ) relrep["metadata"];
			  assertNotNull( metadata["type"] );
			  assertTrue( ( ( Number ) metadata["id"] ).longValue() >= 0 );
		 }
	}

}