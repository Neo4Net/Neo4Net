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
namespace Neo4Net.Server.rest
{
	using IOUtils = org.apache.commons.io.IOUtils;
	using HttpEntity = org.apache.http.HttpEntity;
	using HttpResponse = org.apache.http.HttpResponse;
	using HttpClient = org.apache.http.client.HttpClient;
	using HttpGet = org.apache.http.client.methods.HttpGet;
	using DefaultHttpClient = org.apache.http.impl.client.DefaultHttpClient;
	using Before = org.junit.Before;
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;


	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using FunctionalTestHelper = Neo4Net.Server.helpers.FunctionalTestHelper;
	using GraphDbHelper = Neo4Net.Server.rest.domain.GraphDbHelper;
	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using RelationshipRepresentationTest = Neo4Net.Server.rest.repr.RelationshipRepresentationTest;
	using StreamingJsonFormat = Neo4Net.Server.rest.repr.formats.StreamingJsonFormat;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class RetrieveRelationshipsFromNodeIT : AbstractRestFunctionalDocTestBase
	{
		 private long _nodeWithRelationships;
		 private long _nodeWithoutRelationships;
		 private long _nonExistingNode;

		 private static FunctionalTestHelper _functionalTestHelper;
		 private static GraphDbHelper _helper;
		 private long _likes;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setupServer()
		 public static void SetupServer()
		 {
			  _functionalTestHelper = new FunctionalTestHelper( Server() );
			  _helper = _functionalTestHelper.GraphDbHelper;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setupTheDatabase()
		 public virtual void SetupTheDatabase()
		 {
			  _nodeWithRelationships = _helper.createNode();
			  _likes = _helper.createRelationship( "LIKES", _nodeWithRelationships, _helper.createNode() );
			  _helper.createRelationship( "LIKES", _helper.createNode(), _nodeWithRelationships );
			  _helper.createRelationship( "HATES", _nodeWithRelationships, _helper.createNode() );
			  _nodeWithoutRelationships = _helper.createNode();
			  _nonExistingNode = _helper.createNode();
			  _helper.deleteNode( _nonExistingNode );
		 }

		 private JaxRsResponse SendRetrieveRequestToServer( long nodeId, string path )
		 {
			  return RestRequest.Req().get(_functionalTestHelper.nodeUri() + "/" + nodeId + "/relationships" + path);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyRelReps(int expectedSize, String json) throws org.Neo4Net.server.rest.domain.JsonParseException
		 private void VerifyRelReps( int expectedSize, string json )
		 {
			  IList<IDictionary<string, object>> relreps = JsonHelper.jsonToList( json );
			  assertEquals( expectedSize, relreps.Count );
			  foreach ( IDictionary<string, object> relrep in relreps )
			  {
					RelationshipRepresentationTest.verifySerialisation( relrep );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParameteriseUrisInRelationshipRepresentationWithHostHeaderValue() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParameteriseUrisInRelationshipRepresentationWithHostHeaderValue()
		 {
			  HttpClient httpclient = new DefaultHttpClient();
			  try
			  {
					HttpGet httpget = new HttpGet( ServerUri + "db/data/relationship/" + _likes );
					httpget.setHeader( "Accept", "application/json" );
					httpget.setHeader( "Host", "dummy.Neo4Net.org" );
					HttpResponse response = httpclient.execute( httpget );
					HttpEntity IEntity = response.Entity;

					string IEntityBody = IOUtils.ToString( IEntity.Content, StandardCharsets.UTF_8 );

					Console.WriteLine( IEntityBody );

					assertThat( IEntityBody, containsString( "http://dummy.Neo4Net.org/db/data/relationship/" + _likes ) );
					assertThat( IEntityBody, not( containsString( ServerUri ) ) );
			  }
			  finally
			  {
					httpclient.ConnectionManager.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParameteriseUrisInRelationshipRepresentationWithoutHostHeaderUsingRequestUri() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParameteriseUrisInRelationshipRepresentationWithoutHostHeaderUsingRequestUri()
		 {
			  HttpClient httpclient = new DefaultHttpClient();
			  try
			  {
					HttpGet httpget = new HttpGet( ServerUri + "db/data/relationship/" + _likes );

					httpget.setHeader( "Accept", "application/json" );
					HttpResponse response = httpclient.execute( httpget );
					HttpEntity IEntity = response.Entity;

					string IEntityBody = IOUtils.ToString( IEntity.Content, StandardCharsets.UTF_8 );

					assertThat( IEntityBody, containsString( ServerUri + "db/data/relationship/" + _likes ) );
			  }
			  finally
			  {
					httpclient.ConnectionManager.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Get all relationships.") @Test public void shouldRespondWith200AndListOfRelationshipRepresentationsWhenGettingAllRelationshipsForANode() throws org.Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Get all relationships.")]
		 public virtual void ShouldRespondWith200AndListOfRelationshipRepresentationsWhenGettingAllRelationshipsForANode()
		 {
			  string IEntity = GenConflict.get().expectedStatus(200).get(_functionalTestHelper.nodeUri() + "/" + _nodeWithRelationships + "/relationships" + "/all").entity();
			  VerifyRelReps( 3, IEntity );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith200AndListOfRelationshipRepresentationsWhenGettingAllRelationshipsForANodeStreaming() throws org.Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith200AndListOfRelationshipRepresentationsWhenGettingAllRelationshipsForANodeStreaming()
		 {
			  string IEntity = GenConflict.get().withHeader(StreamingJsonFormat.STREAM_HEADER,"true").expectedStatus(200).get(_functionalTestHelper.nodeUri() + "/" + _nodeWithRelationships + "/relationships" + "/all").entity();
			  VerifyRelReps( 3, IEntity );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Get incoming relationships.") @Test public void shouldRespondWith200AndListOfRelationshipRepresentationsWhenGettingIncomingRelationshipsForANode() throws org.Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Get incoming relationships.")]
		 public virtual void ShouldRespondWith200AndListOfRelationshipRepresentationsWhenGettingIncomingRelationshipsForANode()
		 {
			  string IEntity = GenConflict.get().expectedStatus(200).get(_functionalTestHelper.nodeUri() + "/" + _nodeWithRelationships + "/relationships" + "/in").entity();
			  VerifyRelReps( 1, IEntity );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Get outgoing relationships.") @Test public void shouldRespondWith200AndListOfRelationshipRepresentationsWhenGettingOutgoingRelationshipsForANode() throws org.Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Get outgoing relationships.")]
		 public virtual void ShouldRespondWith200AndListOfRelationshipRepresentationsWhenGettingOutgoingRelationshipsForANode()
		 {
			  string IEntity = GenConflict.get().expectedStatus(200).get(_functionalTestHelper.nodeUri() + "/" + _nodeWithRelationships + "/relationships" + "/out").entity();
			  VerifyRelReps( 2, IEntity );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Get typed relationships.\n" + "\n" + "Note that the \"+&+\" needs to be encoded like \"+%26+\" for example when\n" + "using http://curl.haxx.se/[cURL] from the terminal.") @Test public void shouldRespondWith200AndListOfRelationshipRepresentationsWhenGettingAllTypedRelationshipsForANode() throws org.Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Get typed relationships.\n" + "\n" + "Note that the \"+&+\" needs to be encoded like \"+%26+\" for example when\n" + "using http://curl.haxx.se/[cURL] from the terminal.")]
		 public virtual void ShouldRespondWith200AndListOfRelationshipRepresentationsWhenGettingAllTypedRelationshipsForANode()
		 {
			  string IEntity = GenConflict.get().expectedStatus(200).get(_functionalTestHelper.nodeUri() + "/" + _nodeWithRelationships + "/relationships" + "/all/LIKES&HATES").entity();
			  VerifyRelReps( 3, IEntity );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith200AndListOfRelationshipRepresentationsWhenGettingIncomingTypedRelationshipsForANode() throws org.Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith200AndListOfRelationshipRepresentationsWhenGettingIncomingTypedRelationshipsForANode()
		 {
			  JaxRsResponse response = SendRetrieveRequestToServer( _nodeWithRelationships, "/in/LIKES" );
			  assertEquals( 200, response.Status );
			  assertThat( response.Type.ToString(), containsString(MediaType.APPLICATION_JSON) );
			  VerifyRelReps( 1, response.Entity );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith200AndListOfRelationshipRepresentationsWhenGettingOutgoingTypedRelationshipsForANode() throws org.Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith200AndListOfRelationshipRepresentationsWhenGettingOutgoingTypedRelationshipsForANode()
		 {
			  JaxRsResponse response = SendRetrieveRequestToServer( _nodeWithRelationships, "/out/HATES" );
			  assertEquals( 200, response.Status );
			  assertThat( response.Type.ToString(), containsString(MediaType.APPLICATION_JSON) );
			  VerifyRelReps( 1, response.Entity );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Documented("Get relationships on a node without relationships.") @Test public void shouldRespondWith200AndEmptyListOfRelationshipRepresentationsWhenGettingAllRelationshipsForANodeWithoutRelationships() throws org.Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Get relationships on a node without relationships.")]
		 public virtual void ShouldRespondWith200AndEmptyListOfRelationshipRepresentationsWhenGettingAllRelationshipsForANodeWithoutRelationships()
		 {
			  string IEntity = GenConflict.get().expectedStatus(200).get(_functionalTestHelper.nodeUri() + "/" + _nodeWithoutRelationships + "/relationships" + "/all").entity();
			  VerifyRelReps( 0, IEntity );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith200AndEmptyListOfRelationshipRepresentationsWhenGettingIncomingRelationshipsForANodeWithoutRelationships() throws org.Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith200AndEmptyListOfRelationshipRepresentationsWhenGettingIncomingRelationshipsForANodeWithoutRelationships()
		 {
			  JaxRsResponse response = SendRetrieveRequestToServer( _nodeWithoutRelationships, "/in" );
			  assertEquals( 200, response.Status );
			  assertThat( response.Type.ToString(), containsString(MediaType.APPLICATION_JSON) );
			  VerifyRelReps( 0, response.Entity );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith200AndEmptyListOfRelationshipRepresentationsWhenGettingOutgoingRelationshipsForANodeWithoutRelationships() throws org.Neo4Net.server.rest.domain.JsonParseException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRespondWith200AndEmptyListOfRelationshipRepresentationsWhenGettingOutgoingRelationshipsForANodeWithoutRelationships()
		 {
			  JaxRsResponse response = SendRetrieveRequestToServer( _nodeWithoutRelationships, "/out" );
			  assertEquals( 200, response.Status );
			  assertThat( response.Type.ToString(), containsString(MediaType.APPLICATION_JSON) );
			  VerifyRelReps( 0, response.Entity );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith404WhenGettingAllRelationshipsForNonExistingNode()
		 public virtual void ShouldRespondWith404WhenGettingAllRelationshipsForNonExistingNode()
		 {
			  JaxRsResponse response = SendRetrieveRequestToServer( _nonExistingNode, "/all" );
			  assertEquals( 404, response.Status );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith404WhenGettingIncomingRelationshipsForNonExistingNode()
		 public virtual void ShouldRespondWith404WhenGettingIncomingRelationshipsForNonExistingNode()
		 {
			  JaxRsResponse response = SendRetrieveRequestToServer( _nonExistingNode, "/in" );
			  assertEquals( 404, response.Status );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith404WhenGettingIncomingRelationshipsForNonExistingNodeStreaming()
		 public virtual void ShouldRespondWith404WhenGettingIncomingRelationshipsForNonExistingNodeStreaming()
		 {
			  JaxRsResponse response = RestRequest.Req().header(StreamingJsonFormat.STREAM_HEADER, "true").get(_functionalTestHelper.nodeUri() + "/" + _nonExistingNode + "/relationships" + "/in");
			  assertEquals( 404, response.Status );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRespondWith404WhenGettingOutgoingRelationshipsForNonExistingNode()
		 public virtual void ShouldRespondWith404WhenGettingOutgoingRelationshipsForNonExistingNode()
		 {
			  JaxRsResponse response = SendRetrieveRequestToServer( _nonExistingNode, "/out" );
			  assertEquals( 404, response.Status );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGet200WhenRetrievingValidRelationship()
		 public virtual void ShouldGet200WhenRetrievingValidRelationship()
		 {
			  long relationshipId = _helper.createRelationship( "LIKES" );

			  JaxRsResponse response = RestRequest.Req().get(_functionalTestHelper.relationshipUri(relationshipId));

			  assertEquals( 200, response.Status );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGetARelationshipRepresentationInJsonWhenRetrievingValidRelationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGetARelationshipRepresentationInJsonWhenRetrievingValidRelationship()
		 {
			  long relationshipId = _helper.createRelationship( "LIKES" );

			  JaxRsResponse response = RestRequest.Req().get(_functionalTestHelper.relationshipUri(relationshipId));

			  string IEntity = response.Entity;
			  assertNotNull( IEntity );
			  IsLegalJson( IEntity );
			  response.Close();
		 }

		 private string ServerUri
		 {
			 get
			 {
				  return Server().baseUri().ToString();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void isLegalJson(String IEntity) throws org.Neo4Net.server.rest.domain.JsonParseException
		 private void IsLegalJson( string IEntity )
		 {
			  JsonHelper.jsonToMap( IEntity );
		 }
	}

}