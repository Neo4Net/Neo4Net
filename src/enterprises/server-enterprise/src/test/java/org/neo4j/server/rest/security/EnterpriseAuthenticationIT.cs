using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Server.rest.security
{
	using ArrayNode = org.codehaus.jackson.node.ArrayNode;
	using Test = org.junit.Test;


	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using EnterpriseServerBuilder = Neo4Net.Server.enterprise.helpers.EnterpriseServerBuilder;
	using HTTP = Neo4Net.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.hasItems;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class EnterpriseAuthenticationIT : AuthenticationIT
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void startServer(boolean authEnabled) throws java.io.IOException
		 public override void StartServer( bool authEnabled )
		 {
			  Server = EnterpriseServerBuilder.serverOnRandomPorts().withProperty(GraphDatabaseSettings.auth_enabled.name(), Convert.ToString(authEnabled)).build();
			  Server.start();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHavePredefinedRoles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHavePredefinedRoles()
		 {
			  // Given
			  StartServerWithConfiguredUser();

			  // When
			  string method = "POST";
			  string path = "db/data/transaction/commit";
			  HTTP.RawPayload payload = HTTP.RawPayload.quotedJson( "{'statements':[{'statement':'CALL dbms.security.listRoles()'}]}" );
			  HTTP.Response response = HTTP.withBasicAuth( "neo4j", "secret" ).request( method, Server.baseUri().resolve(path).ToString(), payload );

			  // Then
			  assertThat( response.Status(), equalTo(200) );
			  ArrayNode errors = ( ArrayNode ) response.Get( "errors" );
			  assertThat( "Should have no errors", errors.size(), equalTo(0) );
			  ArrayNode results = ( ArrayNode ) response.Get( "results" );
			  ArrayNode data = ( ArrayNode ) results.get( 0 ).get( "data" );
			  assertThat( "Should have 5 predefined roles", data.size(), equalTo(5) );
			  Stream<string> values = data.findValues( "row" ).Select( row => row.get( 0 ).asText() );
			  assertThat( "Expected specific roles", values.collect( Collectors.toList() ), hasItems("admin", "architect", "publisher", "editor", "reader") );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowExecutingEnterpriseBuiltInProceduresWithAuthDisabled() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowExecutingEnterpriseBuiltInProceduresWithAuthDisabled()
		 {
			  // Given
			  StartServerWithAuthDisabled();

			  // When
			  string method = "POST";
			  string path = "db/data/transaction/commit";
			  HTTP.RawPayload payload = HTTP.RawPayload.quotedJson( "{'statements':[{'statement':'CALL dbms.listQueries()'}]}" );
			  HTTP.Response response = HTTP.request( method, Server.baseUri().resolve(path).ToString(), payload );

			  // Then
			  assertThat( response.Status(), equalTo(200) );
			  ArrayNode errors = ( ArrayNode ) response.Get( "errors" );
			  assertThat( "Should have no errors", errors.size(), equalTo(0) );
			  ArrayNode results = ( ArrayNode ) response.Get( "results" );
			  ArrayNode data = ( ArrayNode ) results.get( 0 ).get( "data" );
			  assertThat( "Should see our own query", data.size(), equalTo(1) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void startServerWithAuthDisabled() throws java.io.IOException
		 private void StartServerWithAuthDisabled()
		 {
			  Server = EnterpriseServerBuilder.serverOnRandomPorts().withProperty(GraphDatabaseSettings.auth_enabled.name(), Convert.ToString(false)).build();
			  Server.start();
		 }
	}

}