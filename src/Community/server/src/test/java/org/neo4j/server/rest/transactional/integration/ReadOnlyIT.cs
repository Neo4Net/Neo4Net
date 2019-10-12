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
namespace Neo4Net.Server.rest.transactional.integration
{
	using JsonNode = org.codehaus.jackson.JsonNode;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using ServerHelper = Neo4Net.Server.helpers.ServerHelper;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using ExclusiveServerTestBase = Neo4Net.Test.server.ExclusiveServerTestBase;
	using HTTP = Neo4Net.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.server.HTTP.RawPayload.quotedJson;

	public class ReadOnlyIT : ExclusiveServerTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory dir = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory Dir = TestDirectory.testDirectory();
		 private NeoServer _readOnlyServer;
		 private HTTP.Builder _http;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  ServerHelper.cleanTheDatabase( _readOnlyServer );
			  _readOnlyServer = ServerHelper.createReadOnlyServer( Dir.storeDir() );
			  _http = HTTP.withBaseUri( _readOnlyServer.baseUri() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void teardown()
		 public virtual void Teardown()
		 {
			  if ( _readOnlyServer != null )
			  {
					_readOnlyServer.stop();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnReadOnlyStatusWhenCreatingNodes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnReadOnlyStatusWhenCreatingNodes()
		 {
			  // Given
			  HTTP.Response response = _http.POST( "db/data/transaction/commit", quotedJson( "{ 'statements': [ { 'statement': 'CREATE (node)' } ] }" ) );

			  // Then
			  JsonNode error = response.Get( "errors" ).get( 0 );
			  string code = error.get( "code" ).asText();
			  string message = error.get( "message" ).asText();

			  assertEquals( "Neo.ClientError.General.ForbiddenOnReadOnlyDatabase", code );
			  assertThat( message, containsString( "This is a read only Neo4j instance" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnReadOnlyStatusWhenCreatingNodesWhichTransitivelyCreateTokens() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnReadOnlyStatusWhenCreatingNodesWhichTransitivelyCreateTokens()
		 {
			  // Given
			  // When
			  HTTP.Response response = _http.POST( "db/data/transaction/commit", quotedJson( "{ 'statements': [ { 'statement': 'CREATE (node:Node)' } ] }" ) );

			  // Then
			  JsonNode error = response.Get( "errors" ).get( 0 );
			  string code = error.get( "code" ).asText();
			  string message = error.get( "message" ).asText();

			  assertEquals( "Neo.ClientError.General.ForbiddenOnReadOnlyDatabase", code );
			  assertThat( message, containsString( "This is a read only Neo4j instance" ) );
		 }

	}

}