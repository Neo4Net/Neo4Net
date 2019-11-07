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
	using Before = org.junit.Before;
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;


	using FormattedLogProvider = Neo4Net.Logging.FormattedLogProvider;
	using ServerHelper = Neo4Net.Server.helpers.ServerHelper;
	using JaxRsResponse = Neo4Net.Server.rest.JaxRsResponse;
	using RestRequest = Neo4Net.Server.rest.RestRequest;
	using ExclusiveServerTestBase = Neo4Net.Test.server.ExclusiveServerTestBase;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.server.AbstractNeoServer.Neo4Net_IS_STARTING_MESSAGE;

	public class NeoServerStartupLoggingIT : ExclusiveServerTestBase
	{
		 private static MemoryStream @out;
		 private static NeoServer _server;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setupServer() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public static void SetupServer()
		 {
			  @out = new MemoryStream();
			  _server = ServerHelper.createNonPersistentServer( FormattedLogProvider.toOutputStream( @out ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void cleanTheDatabase()
		 public virtual void CleanTheDatabase()
		 {
			  ServerHelper.cleanTheDatabase( _server );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void stopServer()
		 public static void StopServer()
		 {
			  _server.stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogStartup()
		 public virtual void ShouldLogStartup()
		 {
			  // Check the logs
			  string logContent = @out.ToString();
			  assertThat( logContent.Length, @is( greaterThan( 0 ) ) );
			  assertThat( logContent, containsString( Neo4Net_IS_STARTING_MESSAGE ) );
			  // Check the server is alive
			  Client nonRedirectingClient = Client.create();
			  nonRedirectingClient.FollowRedirects = false;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.server.rest.JaxRsResponse response = new Neo4Net.server.rest.RestRequest(server.baseUri(), nonRedirectingClient).get();
			  JaxRsResponse response = ( new RestRequest( _server.baseUri(), nonRedirectingClient ) ).get();
			  assertThat( response.Status, @is( greaterThan( 199 ) ) );

		 }
	}

}