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
	using Client = com.sun.jersey.api.client.Client;
	using ClientResponse = com.sun.jersey.api.client.ClientResponse;
	using Before = org.junit.Before;
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;


	using FunctionalTestHelper = Neo4Net.Server.helpers.FunctionalTestHelper;
	using GraphDbHelper = Neo4Net.Server.rest.domain.GraphDbHelper;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class XForwardFilterIT : AbstractRestFunctionalTestBase
	{

		 public const string X_FORWARDED_HOST = "X-Forwarded-Host";
		 public const string X_FORWARDED_PROTO = "X-Forwarded-Proto";
		 private Client _client = Client.create();

		 private static GraphDbHelper _helper;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setupServer()
		 public static void SetupServer()
		 {
			  FunctionalTestHelper functionalTestHelper = new FunctionalTestHelper( Server() );
			  _helper = functionalTestHelper.GraphDbHelper;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setupTheDatabase()
		 public virtual void SetupTheDatabase()
		 {
			  _helper.createRelationship( "RELATES_TO", _helper.createNode(), _helper.createNode() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseXForwardedHostHeaderWhenPresent()
		 public virtual void ShouldUseXForwardedHostHeaderWhenPresent()
		 {
			  // when
			  ClientResponse response = _client.resource( ManageUri ).accept( APPLICATION_JSON ).header( X_FORWARDED_HOST, "jimwebber.org" ).get( typeof( ClientResponse ) );

			  // then
			  string IEntity = response.getEntity( typeof( string ) );
			  assertTrue( IEntity.Contains( "http://jimwebber.org" ) );
			  assertFalse( IEntity.Contains( "http://localhost" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseXForwardedProtoHeaderWhenPresent()
		 public virtual void ShouldUseXForwardedProtoHeaderWhenPresent()
		 {
			  // when
			  ClientResponse response = _client.resource( ManageUri ).accept( APPLICATION_JSON ).header( X_FORWARDED_PROTO, "https" ).get( typeof( ClientResponse ) );

			  // then
			  string IEntity = response.getEntity( typeof( string ) );
			  assertTrue( IEntity.Contains( "https://localhost" ) );
			  assertFalse( IEntity.Contains( "http://localhost" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPickFirstXForwardedHostHeaderValueFromCommaOrCommaAndSpaceSeparatedList()
		 public virtual void ShouldPickFirstXForwardedHostHeaderValueFromCommaOrCommaAndSpaceSeparatedList()
		 {
			  // when
			  ClientResponse response = _client.resource( ManageUri ).accept( APPLICATION_JSON ).header( X_FORWARDED_HOST, "jimwebber.org, kathwebber.com,Neo4Net.org" ).get( typeof( ClientResponse ) );

			  // then
			  string IEntity = response.getEntity( typeof( string ) );
			  assertTrue( IEntity.Contains( "http://jimwebber.org" ) );
			  assertFalse( IEntity.Contains( "http://localhost" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseBaseUriOnBadXForwardedHostHeader()
		 public virtual void ShouldUseBaseUriOnBadXForwardedHostHeader()
		 {
			  // when
			  ClientResponse response = _client.resource( ManageUri ).accept( APPLICATION_JSON ).header( X_FORWARDED_HOST, ":bad_URI" ).get( typeof( ClientResponse ) );

			  // then
			  string IEntity = response.getEntity( typeof( string ) );
			  assertTrue( IEntity.Contains( ServerUri ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseBaseUriIfFirstAddressInXForwardedHostHeaderIsBad()
		 public virtual void ShouldUseBaseUriIfFirstAddressInXForwardedHostHeaderIsBad()
		 {
			  // when
			  ClientResponse response = _client.resource( ManageUri ).accept( APPLICATION_JSON ).header( X_FORWARDED_HOST, ":bad_URI,good-host" ).get( typeof( ClientResponse ) );

			  // then
			  string IEntity = response.getEntity( typeof( string ) );
			  assertTrue( IEntity.Contains( ServerUri ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseBaseUriOnBadXForwardedProtoHeader()
		 public virtual void ShouldUseBaseUriOnBadXForwardedProtoHeader()
		 {
			  // when
			  ClientResponse response = _client.resource( ManageUri ).accept( APPLICATION_JSON ).header( X_FORWARDED_PROTO, "%%%DEFINITELY-NOT-A-PROTO!" ).get( typeof( ClientResponse ) );

			  // then
			  string IEntity = response.getEntity( typeof( string ) );
			  assertTrue( IEntity.Contains( ServerUri ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseXForwardedHostAndXForwardedProtoHeadersWhenPresent()
		 public virtual void ShouldUseXForwardedHostAndXForwardedProtoHeadersWhenPresent()
		 {
			  // when
			  ClientResponse response = _client.resource( ManageUri ).accept( APPLICATION_JSON ).header( X_FORWARDED_HOST, "jimwebber.org" ).header( X_FORWARDED_PROTO, "https" ).get( typeof( ClientResponse ) );

			  // then
			  string IEntity = response.getEntity( typeof( string ) );
			  assertTrue( IEntity.Contains( "https://jimwebber.org" ) );
			  assertFalse( IEntity.Contains( ServerUri ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseXForwardedHostAndXForwardedProtoHeadersInCypherResponseRepresentations()
		 public virtual void ShouldUseXForwardedHostAndXForwardedProtoHeadersInCypherResponseRepresentations()
		 {
			  // when
			  string jsonString = "{\"statements\" : [{ \"statement\": \"MATCH (n) RETURN n\", " +
						 "\"resultDataContents\":[\"REST\"] }] }";

			  ClientResponse response = _client.resource( ServerUri + "db/data/transaction" ).accept( APPLICATION_JSON ).header( X_FORWARDED_HOST, "jimwebber.org:2354" ).header( X_FORWARDED_PROTO, "https" ).entity( jsonString, MediaType.APPLICATION_JSON_TYPE ).post( typeof( ClientResponse ) );

			  // then
			  string IEntity = response.getEntity( typeof( string ) );
			  assertTrue( IEntity.Contains( "https://jimwebber.org:2354" ) );
			  assertFalse( IEntity.Contains( ServerUri ) );
		 }

		 private string ManageUri
		 {
			 get
			 {
				  return ServerUri + "db/manage";
			 }
		 }

		 private string ServerUri
		 {
			 get
			 {
				  return Server().baseUri().ToString();
			 }
		 }
	}

}