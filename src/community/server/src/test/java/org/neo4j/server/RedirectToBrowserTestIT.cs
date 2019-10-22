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
namespace Neo4Net.Server
{
	using Client = com.sun.jersey.api.client.Client;
	using AfterClass = org.junit.AfterClass;
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;


	using ServerHelper = Neo4Net.Server.helpers.ServerHelper;
	using JaxRsResponse = Neo4Net.Server.rest.JaxRsResponse;
	using RestRequest = Neo4Net.Server.rest.RestRequest;
	using ExclusiveServerTestBase = Neo4Net.Test.server.ExclusiveServerTestBase;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class RedirectToBrowserTestIT : ExclusiveServerTestBase
	{
		 private static NeoServer _server;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void startServer() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public static void StartServer()
		 {
			  _server = ServerHelper.createNonPersistentServer();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void stopServer()
		 public static void StopServer()
		 {
			  if ( _server != null )
			  {
					_server.stop();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRedirectToBrowser() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRedirectToBrowser()
		 {
			  Client nonRedirectingClient = Client.create();
			  nonRedirectingClient.FollowRedirects = false;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.server.rest.JaxRsResponse response = new org.Neo4Net.server.rest.RestRequest(server.baseUri(), nonRedirectingClient).accept(javax.ws.rs.core.MediaType.TEXT_HTML_TYPE).get(server.baseUri().toString());
			  JaxRsResponse response = ( new RestRequest( _server.baseUri(), nonRedirectingClient ) ).accept(MediaType.TEXT_HTML_TYPE).get(_server.baseUri().ToString());

			  assertEquals( 303, response.Status );
			  assertEquals( new URI( _server.baseUri() + "browser/" ), response.Location );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRedirectToBrowserUsingXForwardedHeaders() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRedirectToBrowserUsingXForwardedHeaders()
		 {
			  Client nonRedirectingClient = Client.create();
			  nonRedirectingClient.FollowRedirects = false;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.server.rest.JaxRsResponse response = new org.Neo4Net.server.rest.RestRequest(server.baseUri(), nonRedirectingClient).accept(javax.ws.rs.core.MediaType.TEXT_HTML_TYPE).header("X-Forwarded-Host", "foo.bar:8734").header("X-Forwarded-Proto", "https").get(server.baseUri().toString());
			  JaxRsResponse response = ( new RestRequest( _server.baseUri(), nonRedirectingClient ) ).accept(MediaType.TEXT_HTML_TYPE).header("X-Forwarded-Host", "foo.bar:8734").header("X-Forwarded-Proto", "https").get(_server.baseUri().ToString());

			  assertEquals( 303, response.Status );
			  assertEquals( new URI( "https://foo.bar:8734/browser/" ), response.Location );
			  response.Close();
		 }
	}

}