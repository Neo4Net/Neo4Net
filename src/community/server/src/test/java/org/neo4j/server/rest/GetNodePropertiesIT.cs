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
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;


	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using FunctionalTestHelper = Neo4Net.Server.helpers.FunctionalTestHelper;
	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using StreamingJsonFormat = Neo4Net.Server.rest.repr.formats.StreamingJsonFormat;
	using HTTP = Neo4Net.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;

	public class GetNodePropertiesIT : AbstractRestFunctionalDocTestBase
	{
		 private static FunctionalTestHelper _functionalTestHelper;
		 private RestRequest _req = RestRequest.Req();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setupServer()
		 public static void SetupServer()
		 {
			  _functionalTestHelper = new FunctionalTestHelper( Server() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Get properties for node.") @Test public void shouldGet200ForProperties()
		 [Documented("Get properties for node.")]
		 public virtual void ShouldGet200ForProperties()
		 {
			  string entity = JsonHelper.createJsonFrom( Collections.singletonMap( "foo", "bar" ) );
			  JaxRsResponse createResponse = _req.post( _functionalTestHelper.dataUri() + "node/", entity );
			  GenConflict.get().expectedStatus(200).get(createResponse.Location.ToString() + "/properties");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetContentLengthHeaderForRetrievingProperties()
		 public virtual void ShouldGetContentLengthHeaderForRetrievingProperties()
		 {
			  string entity = JsonHelper.createJsonFrom( Collections.singletonMap( "foo", "bar" ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RestRequest request = req;
			  RestRequest request = _req;
			  JaxRsResponse createResponse = request.Post( _functionalTestHelper.dataUri() + "node/", entity );
			  JaxRsResponse response = request.Get( createResponse.Location.ToString() + "/properties" );
			  assertNotNull( response.Headers.get( "Content-Length" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetCorrectContentEncodingRetrievingProperties() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGetCorrectContentEncodingRetrievingProperties()
		 {
			  string asianText = "\u4f8b\u5b50";
			  string germanText = "öäüÖÄÜß";

			  string complicatedString = asianText + germanText;

			  string entity = JsonHelper.createJsonFrom( Collections.singletonMap( "foo", complicatedString ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RestRequest request = req;
			  RestRequest request = _req;
			  JaxRsResponse createResponse = request.Post( _functionalTestHelper.dataUri() + "node/", entity );
			  string response = ( string ) JsonHelper.readJson( request.Get( GetPropertyUri( createResponse.Location.ToString(), "foo" ) ).Entity );
			  assertEquals( complicatedString, response );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetCorrectContentEncodingRetrievingPropertiesWithStreaming() throws org.neo4j.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGetCorrectContentEncodingRetrievingPropertiesWithStreaming()
		 {
			  string asianText = "\u4f8b\u5b50";
			  string germanText = "öäüÖÄÜß";

			  string complicatedString = asianText + germanText;

			  string entity = JsonHelper.createJsonFrom( Collections.singletonMap( "foo", complicatedString ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final RestRequest request = req.header(org.neo4j.server.rest.repr.formats.StreamingJsonFormat.STREAM_HEADER, "true");
			  RestRequest request = _req.header( StreamingJsonFormat.STREAM_HEADER, "true" );
			  JaxRsResponse createResponse = request.Post( _functionalTestHelper.dataUri() + "node/", entity );
			  string response = ( string ) JsonHelper.readJson( request.Get( GetPropertyUri( createResponse.Location.ToString(), "foo" ), new MediaType("application", "json", stringMap("stream", "true")) ).Entity );
			  assertEquals( complicatedString, response );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGet404ForPropertiesOnNonExistentNode()
		 public virtual void ShouldGet404ForPropertiesOnNonExistentNode()
		 {
			  JaxRsResponse response = RestRequest.Req().get(_functionalTestHelper.dataUri() + "node/999999/properties");
			  assertEquals( 404, response.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeJSONContentTypeOnPropertiesResponse()
		 public virtual void ShouldBeJSONContentTypeOnPropertiesResponse()
		 {
			  string entity = JsonHelper.createJsonFrom( Collections.singletonMap( "foo", "bar" ) );
			  JaxRsResponse createResource = _req.post( _functionalTestHelper.dataUri() + "node/", entity );
			  JaxRsResponse response = _req.get( createResource.Location.ToString() + "/properties" );
			  assertThat( response.Type.ToString(), containsString(MediaType.APPLICATION_JSON) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGet404ForNoProperty()
		 public virtual void ShouldGet404ForNoProperty()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final JaxRsResponse createResponse = req.post(functionalTestHelper.dataUri() + "node/", "");
			  JaxRsResponse createResponse = _req.post( _functionalTestHelper.dataUri() + "node/", "" );
			  JaxRsResponse response = _req.get( GetPropertyUri( createResponse.Location.ToString(), "foo" ) );
			  assertEquals( 404, response.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Get property for node.\n" + "\n" + "Get a single node property from a node.") @Test public void shouldGet200ForProperty()
		 [Documented("Get property for node.\n" + "\n" + "Get a single node property from a node.")]
		 public virtual void ShouldGet200ForProperty()
		 {
			  string entity = JsonHelper.createJsonFrom( Collections.singletonMap( "foo", "bar" ) );
			  JaxRsResponse createResponse = _req.post( _functionalTestHelper.dataUri() + "node/", entity );
			  JaxRsResponse response = _req.get( GetPropertyUri( createResponse.Location.ToString(), "foo" ) );
			  assertEquals( 200, response.Status );

			  GenConflict.get().expectedStatus(200).get(GetPropertyUri(createResponse.Location.ToString(), "foo"));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGet404ForPropertyOnNonExistentNode()
		 public virtual void ShouldGet404ForPropertyOnNonExistentNode()
		 {
			  JaxRsResponse response = RestRequest.Req().get(GetPropertyUri(_functionalTestHelper.dataUri() + "node/" + "999999", "foo"));
			  assertEquals( 404, response.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeJSONContentTypeOnPropertyResponse()
		 public virtual void ShouldBeJSONContentTypeOnPropertyResponse()
		 {
			  string entity = JsonHelper.createJsonFrom( Collections.singletonMap( "foo", "bar" ) );

			  JaxRsResponse createResponse = _req.post( _functionalTestHelper.dataUri() + "node/", entity );

			  JaxRsResponse response = _req.get( GetPropertyUri( createResponse.Location.ToString(), "foo" ) );

			  assertThat( response.Type.ToString(), containsString(MediaType.APPLICATION_JSON) );

			  createResponse.Close();
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnEmptyMapForEmptyProperties()
		 public virtual void ShouldReturnEmptyMapForEmptyProperties()
		 {
			  // Given
			  string location = HTTP.POST( Server().baseUri().resolve("db/data/node").ToString() ).location();

			  // When
			  HTTP.Response res = HTTP.GET( location + "/properties" );

			  // Then
			  assertThat( res.RawContent(), equalTo("{ }") );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private String getPropertyUri(final String baseUri, final String key)
		 private string GetPropertyUri( string baseUri, string key )
		 {
			  return baseUri + "/properties/" + key;
		 }
	}

}