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
namespace Neo4Net.Server.Security.Auth
{
	using After = org.junit.After;
	using Test = org.junit.Test;

	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using CommunityServerBuilder = Neo4Net.Server.helpers.CommunityServerBuilder;
	using ExclusiveServerTestBase = Neo4Net.Test.server.ExclusiveServerTestBase;
	using HTTP = Neo4Net.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.server.HTTP.RawPayload.quotedJson;

	public class AuthorizationDisabledIT : ExclusiveServerTestBase
	{

		 private CommunityNeoServer _server;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowDisablingAuthorization() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowDisablingAuthorization()
		 {
			  // Given
			  _server = CommunityServerBuilder.serverOnRandomPorts().withProperty(GraphDatabaseSettings.auth_enabled.name(), "false").build();

			  // When
			  _server.start();

			  // Then I should have write access
			  HTTP.Response response = HTTP.POST( _server.baseUri().resolve("db/data/node").ToString(), quotedJson("{'name':'My Node'}") );
			  assertThat( response.Status(), equalTo(201) );
			  string node = response.Location();

			  // Then I should have read access
			  assertThat( HTTP.GET( node ).status(), equalTo(200) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanup()
		 public virtual void Cleanup()
		 {
			  if ( _server != null )
			  {
					_server.stop();
			  }
		 }
	}

}