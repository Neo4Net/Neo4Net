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
namespace Neo4Net.Server.rest
{
	using IOUtils = org.apache.commons.io.IOUtils;
	using HttpEntity = org.apache.http.HttpEntity;
	using HttpResponse = org.apache.http.HttpResponse;
	using HttpGet = org.apache.http.client.methods.HttpGet;
	using CloseableHttpClient = org.apache.http.impl.client.CloseableHttpClient;
	using HttpClientBuilder = org.apache.http.impl.client.HttpClientBuilder;
	using Before = org.junit.Before;
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;


	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using FunctionalTestHelper = Neo4Net.Server.helpers.FunctionalTestHelper;
	using ResponseEntity = Neo4Net.Server.rest.RESTRequestGenerator.ResponseEntity;
	using GraphDbHelper = Neo4Net.Server.rest.domain.GraphDbHelper;
	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;
	using CompactJsonFormat = Neo4Net.Server.rest.repr.formats.CompactJsonFormat;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class RetrieveNodeIT : AbstractRestFunctionalDocTestBase
	{
		 private URI _nodeUri;
		 private GraphDbHelper _helper;
		 private static FunctionalTestHelper _functionalTestHelper;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setupServer()
		 public static void SetupServer()
		 {
			  _functionalTestHelper = new FunctionalTestHelper( Server() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void cleanTheDatabaseAndInitialiseTheNodeUri() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CleanTheDatabaseAndInitialiseTheNodeUri()
		 {
			  _helper = new GraphDbHelper( Server().Database );
			  _nodeUri = new URI( _functionalTestHelper.nodeUri() + "/" + _helper.createNode() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParameteriseUrisInNodeRepresentationWithHostHeaderValue() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParameteriseUrisInNodeRepresentationWithHostHeaderValue()
		 {
			  using ( CloseableHttpClient httpclient = HttpClientBuilder.create().build() )
			  {
					HttpGet httpget = new HttpGet( _nodeUri );

					httpget.setHeader( "Accept", "application/json" );
					httpget.setHeader( "Host", "dummy.neo4j.org" );
					HttpResponse response = httpclient.execute( httpget );
					HttpEntity entity = response.Entity;

					string entityBody = IOUtils.ToString( entity.Content, StandardCharsets.UTF_8 );

					assertThat( entityBody, containsString( "http://dummy.neo4j.org/db/data/node/" ) );

			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParameteriseUrisInNodeRepresentationWithoutHostHeaderUsingRequestUri() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParameteriseUrisInNodeRepresentationWithoutHostHeaderUsingRequestUri()
		 {
			  using ( CloseableHttpClient httpclient = HttpClientBuilder.create().build() )
			  {
					HttpGet httpget = new HttpGet( _nodeUri );

					httpget.setHeader( "Accept", "application/json" );
					HttpResponse response = httpclient.execute( httpget );
					HttpEntity entity = response.Entity;

					string entityBody = IOUtils.ToString( entity.Content, StandardCharsets.UTF_8 );

					assertThat( entityBody, containsString( _nodeUri.ToString() ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Get node.\n" + "\n" + "Note that the response contains URI/templates for the available\n" + "operations for getting properties and relationships.") @Test public void shouldGet200WhenRetrievingNode()
		 [Documented("Get node.\n" + "\n" + "Note that the response contains URI/templates for the available\n" + "operations for getting properties and relationships.")]
		 public virtual void ShouldGet200WhenRetrievingNode()
		 {
			  string uri = _nodeUri.ToString();
			  GenConflict.get().expectedStatus(200).get(uri);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Get node -- compact.\n" + "\n" + "Specifying the subformat in the requests media type yields a more compact\n" + "JSON response without metadata and templates.") @Test public void shouldGet200WhenRetrievingNodeCompact()
		 [Documented("Get node -- compact.\n" + "\n" + "Specifying the subformat in the requests media type yields a more compact\n" + "JSON response without metadata and templates.")]
		 public virtual void ShouldGet200WhenRetrievingNodeCompact()
		 {
			  string uri = _nodeUri.ToString();
			  ResponseEntity entity = GenConflict.get().expectedType(CompactJsonFormat.MEDIA_TYPE).expectedStatus(200).get(uri);
			  assertTrue( entity.Entity().Contains("self") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetContentLengthHeaderWhenRetrievingNode()
		 public virtual void ShouldGetContentLengthHeaderWhenRetrievingNode()
		 {
			  JaxRsResponse response = RetrieveNodeFromService( _nodeUri.ToString() );
			  assertNotNull( response.Headers.get( "Content-Length" ) );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveJsonMediaTypeOnResponse()
		 public virtual void ShouldHaveJsonMediaTypeOnResponse()
		 {
			  JaxRsResponse response = RetrieveNodeFromService( _nodeUri.ToString() );
			  assertThat( response.Type.ToString(), containsString(MediaType.APPLICATION_JSON) );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveJsonDataInResponse() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHaveJsonDataInResponse()
		 {
			  JaxRsResponse response = RetrieveNodeFromService( _nodeUri.ToString() );

			  IDictionary<string, object> map = JsonHelper.jsonToMap( response.Entity );
			  assertTrue( map.ContainsKey( "self" ) );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Get non-existent node.") @Test public void shouldGet404WhenRetrievingNonExistentNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Get non-existent node.")]
		 public virtual void ShouldGet404WhenRetrievingNonExistentNode()
		 {
			  long nonExistentNode = _helper.createNode();
			  _helper.deleteNode( nonExistentNode );
			  URI nonExistentNodeUri = new URI( _functionalTestHelper.nodeUri() + "/" + nonExistentNode );

			  GenConflict.get().expectedStatus(404).get(nonExistentNodeUri.ToString());
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private JaxRsResponse retrieveNodeFromService(final String uri)
		 private JaxRsResponse RetrieveNodeFromService( string uri )
		 {
			  return RestRequest.Req().get(uri);
		 }

	}

}