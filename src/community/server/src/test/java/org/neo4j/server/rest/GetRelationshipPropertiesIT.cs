﻿using System.Collections.Generic;

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
namespace Neo4Net.Server.rest
{
	using MatcherAssert = org.hamcrest.MatcherAssert;
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;


	using FunctionalTestHelper = Neo4Net.Server.helpers.FunctionalTestHelper;
	using GraphDbHelper = Neo4Net.Server.rest.domain.GraphDbHelper;
	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using HTTP = Neo4Net.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.server.HTTP.RawPayload.quotedJson;

	public class GetRelationshipPropertiesIT : AbstractRestFunctionalTestBase
	{
		 private static string _baseRelationshipUri;

		 private static FunctionalTestHelper _functionalTestHelper;
		 private static GraphDbHelper _helper;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setupServer()
		 public static void SetupServer()
		 {
			  _functionalTestHelper = new FunctionalTestHelper( Server() );
			  _helper = _functionalTestHelper.GraphDbHelper;
			  SetupTheDatabase();
		 }

		 private static void SetupTheDatabase()
		 {
			  long relationship = _helper.createRelationship( "LIKES" );
			  IDictionary<string, object> map = new Dictionary<string, object>();
			  map["foo"] = "bar";
			  _helper.setRelationshipProperties( relationship, map );
			  _baseRelationshipUri = _functionalTestHelper.dataUri() + "relationship/" + relationship + "/properties/";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGet200AndContentLengthForProperties()
		 public virtual void ShouldGet200AndContentLengthForProperties()
		 {
			  long relId = _helper.createRelationship( "LIKES" );
			  _helper.setRelationshipProperties( relId, Collections.singletonMap( "foo", "bar" ) );
			  JaxRsResponse response = RestRequest.Req().get(_functionalTestHelper.dataUri() + "relationship/" + relId + "/properties");
			  assertEquals( 200, response.Status );
			  assertNotNull( response.Headers.get( "Content-Length" ) );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGet404ForPropertiesOnNonExistentRelationship()
		 public virtual void ShouldGet404ForPropertiesOnNonExistentRelationship()
		 {
			  JaxRsResponse response = RestRequest.Req().get(_functionalTestHelper.dataUri() + "relationship/999999/properties");
			  assertEquals( 404, response.Status );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeJSONContentTypeOnPropertiesResponse()
		 public virtual void ShouldBeJSONContentTypeOnPropertiesResponse()
		 {
			  long relId = _helper.createRelationship( "LIKES" );
			  _helper.setRelationshipProperties( relId, Collections.singletonMap( "foo", "bar" ) );
			  JaxRsResponse response = RestRequest.Req().get(_functionalTestHelper.dataUri() + "relationship/" + relId + "/properties");
			  assertThat( response.Type.ToString(), containsString(MediaType.APPLICATION_JSON) );
			  response.Close();
		 }

		 private string GetPropertyUri( string key )
		 {
			  return _baseRelationshipUri + key;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGet404ForNoProperty()
		 public virtual void ShouldGet404ForNoProperty()
		 {
			  JaxRsResponse response = RestRequest.Req().get(GetPropertyUri("baz"));
			  assertEquals( 404, response.Status );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGet404ForNonExistingRelationship()
		 public virtual void ShouldGet404ForNonExistingRelationship()
		 {
			  string uri = _functionalTestHelper.dataUri() + "relationship/999999/properties/foo";
			  JaxRsResponse response = RestRequest.Req().get(uri);
			  assertEquals( 404, response.Status );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeValidJSONOnResponse()
		 public virtual void ShouldBeValidJSONOnResponse()
		 {
			  JaxRsResponse response = RestRequest.Req().get(GetPropertyUri("foo"));
			  assertThat( response.Type.ToString(), containsString(MediaType.APPLICATION_JSON) );
			  assertNotNull( JsonHelper.createJsonFrom( response.Entity ) );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnEmptyMapForEmptyProperties()
		 public virtual void ShouldReturnEmptyMapForEmptyProperties()
		 {
			  // Given
			  string node = HTTP.POST( Server().baseUri().resolve("db/data/node").ToString() ).location();
			  string rel = HTTP.POST( node + "/relationships", quotedJson( "{'to':'" + node + "', " + "'type':'LOVES'}" ) ).location();

			  // When
			  HTTP.Response res = HTTP.GET( rel + "/properties" );

			  // Then
			  MatcherAssert.assertThat( res.RawContent(), equalTo("{ }") );
		 }
	}

}