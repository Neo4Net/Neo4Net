using System;

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
namespace Org.Neo4j.Server
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using DataPoints = org.junit.experimental.theories.DataPoints;
	using Theories = org.junit.experimental.theories.Theories;
	using Theory = org.junit.experimental.theories.Theory;
	using RunWith = org.junit.runner.RunWith;


	using ServerSettings = Org.Neo4j.Server.configuration.ServerSettings;
	using ExclusiveServerTestBase = Org.Neo4j.Test.server.ExclusiveServerTestBase;
	using HTTP = Org.Neo4j.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.helpers.CommunityServerBuilder.serverOnRandomPorts;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.server.HTTP.RawPayload.quotedJson;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Theories.class) public class ExplicitIndexIT extends org.neo4j.test.server.ExclusiveServerTestBase
	public class ExplicitIndexIT : ExclusiveServerTestBase
	{
		 private CommunityNeoServer _server;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @DataPoints @SuppressWarnings("unused") public static String[] candidates = {"", "get_or_create", "create_or_fail"};
		 public static string[] Candidates = new string[] { "", "get_or_create", "create_or_fail" };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void stopTheServer()
		 public virtual void StopTheServer()
		 {
			  _server.stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void startServer() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StartServer()
		 {
			  _server = serverOnRandomPorts().withHttpsEnabled().withProperty("dbms.shell.enabled", "false").withProperty("dbms.security.auth_enabled", "false").withProperty(ServerSettings.maximum_response_header_size.name(), "5000").usingDataDir(Folder.directory(Name.MethodName).AbsolutePath).build();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Theory public void shouldRejectIndexValueLargerThanConfiguredSize(String uniqueness)
		 public virtual void ShouldRejectIndexValueLargerThanConfiguredSize( string uniqueness )
		 {
			  //Given
			  _server.start();

			  // When
			  string nodeURI = HTTP.POST( _server.baseUri().ToString() + "db/data/node" ).header("Location");

			  Random r = new Random();
			  string value = "";
			  for ( int i = 0; i < 6_000; i++ )
			  {
					value += ( char )( r.Next( 26 ) + 'a' );
			  }
			  HTTP.Response response = HTTP.POST( _server.baseUri().ToString() + "db/data/index/node/favorites?uniqueness=" + uniqueness, quotedJson("{ 'value': '" + value + " ', 'uri':'" + nodeURI + "', 'key': 'some-key' }") );

			  // Then
			  assertThat( response.Status(), @is(413) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Theory public void shouldNotRejectIndexValueThatIsJustSmallerThanConfiguredSize(String uniqueness)
		 public virtual void ShouldNotRejectIndexValueThatIsJustSmallerThanConfiguredSize( string uniqueness )
		 {
			  //Given
			  _server.start();

			  // When
			  string nodeURI = HTTP.POST( _server.baseUri().ToString() + "db/data/node" ).header("Location");

			  Random r = new Random();
			  string value = "";
			  for ( int i = 0; i < 4_000; i++ )
			  {
					value += ( char )( r.Next( 26 ) + 'a' );
			  }
			  HTTP.Response response = HTTP.POST( _server.baseUri().ToString() + "db/data/index/node/favorites?uniqueness=" + uniqueness, quotedJson("{ 'value': '" + value + " ', 'uri':'" + nodeURI + "', 'key': 'some-key' }") );

			  // Then
			  assertThat( response.Status(), @is(201) );
		 }
	}

}