using System;

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
namespace Neo4Net.Server.rest.security
{
	using JsonNode = org.codehaus.jackson.JsonNode;
	using After = org.junit.After;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using CommunityServerBuilder = Neo4Net.Server.helpers.CommunityServerBuilder;
	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using Neo4Net.Test;
	using ExclusiveServerTestBase = Neo4Net.Test.server.ExclusiveServerTestBase;
	using HTTP = Neo4Net.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class UsersIT : ExclusiveServerTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.TestData<Neo4Net.server.rest.RESTRequestGenerator> gen = Neo4Net.test.TestData.producedThrough(Neo4Net.server.rest.RESTRequestGenerator.PRODUCER);
		 public TestData<RESTRequestGenerator> Gen = TestData.producedThrough( RESTRequestGenerator.PRODUCER );
		 private CommunityNeoServer _server;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Documented("User status\n" + "\n" + "Given that you know the current password, you can ask the server for the user status.") public void user_status() throws Neo4Net.server.rest.domain.JsonParseException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("User status\n" + "\n" + "Given that you know the current password, you can ask the server for the user status.")]
		 public virtual void UserStatus()
		 {
			  // Given
			  StartServerWithConfiguredUser();

			  // Document
			  RESTRequestGenerator.ResponseEntity response = Gen.get().expectedStatus(200).withHeader(HttpHeaders.AUTHORIZATION, HTTP.basicAuthHeader("Neo4Net", "secret")).get(UserURL("Neo4Net"));

			  // Then
			  JsonNode data = JsonHelper.jsonNode( response.Entity() );
			  assertThat( data.get( "username" ).asText(), equalTo("Neo4Net") );
			  assertThat( data.get( "password_change_required" ).asBoolean(), equalTo(false) );
			  assertThat( data.get( "password_change" ).asText(), equalTo(PasswordURL("Neo4Net")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Documented("User status on first access\n" + "\n" + "On first access, and using the default password, the user status will indicate " + "that the users password requires changing.") public void user_status_first_access() throws Neo4Net.server.rest.domain.JsonParseException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("User status on first access\n" + "\n" + "On first access, and using the default password, the user status will indicate " + "that the users password requires changing.")]
		 public virtual void UserStatusFirstAccess()
		 {
			  // Given
			  StartServer( true );

			  // Document
			  RESTRequestGenerator.ResponseEntity response = Gen.get().expectedStatus(200).withHeader(HttpHeaders.AUTHORIZATION, HTTP.basicAuthHeader("Neo4Net", "Neo4Net")).get(UserURL("Neo4Net"));

			  // Then
			  JsonNode data = JsonHelper.jsonNode( response.Entity() );
			  assertThat( data.get( "username" ).asText(), equalTo("Neo4Net") );
			  assertThat( data.get( "password_change_required" ).asBoolean(), equalTo(true) );
			  assertThat( data.get( "password_change" ).asText(), equalTo(PasswordURL("Neo4Net")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Documented("Changing the user password\n" + "\n" + "Given that you know the current password, you can ask the server to change a users password. " + "You can choose any\n" + "password you like, as long as it is different from the current password.") public void change_password() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Changing the user password\n" + "\n" + "Given that you know the current password, you can ask the server to change a users password. " + "You can choose any\n" + "password you like, as long as it is different from the current password.")]
		 public virtual void ChangePassword()
		 {
			  // Given
			  StartServer( true );

			  // Document
			  RESTRequestGenerator.ResponseEntity response = Gen.get().expectedStatus(200).withHeader(HttpHeaders.AUTHORIZATION, HTTP.basicAuthHeader("Neo4Net", "Neo4Net")).payload(QuotedJson("{'password':'secret'}")).post(_server.baseUri().resolve("/user/Neo4Net/password").ToString());

			  // Then the new password should work
			  assertEquals( 200, HTTP.withBasicAuth( "Neo4Net", "secret" ).GET( DataURL() ).status() );

			  // Then the old password should not be invalid
			  assertEquals( 401, HTTP.withBasicAuth( "Neo4Net", "Neo4Net" ).POST( DataURL() ).status() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void cantChangeToCurrentPassword() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CantChangeToCurrentPassword()
		 {
			  // Given
			  StartServer( true );

			  // When
			  HTTP.Response res = HTTP.withBasicAuth( "Neo4Net", "Neo4Net" ).POST( _server.baseUri().resolve("/user/Neo4Net/password").ToString(), HTTP.RawPayload.quotedJson("{'password':'Neo4Net'}") );

			  // Then
			  assertThat( res.Status(), equalTo(422) );
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

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void startServer(boolean authEnabled) throws java.io.IOException
		 public virtual void StartServer( bool authEnabled )
		 {
			  _server = CommunityServerBuilder.serverOnRandomPorts().withProperty(GraphDatabaseSettings.auth_enabled.name(), Convert.ToString(authEnabled)).build();
			  _server.start();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void startServerWithConfiguredUser() throws java.io.IOException
		 public virtual void StartServerWithConfiguredUser()
		 {
			  StartServer( true );
			  // Set the password
			  HTTP.Response post = HTTP.withBasicAuth( "Neo4Net", "Neo4Net" ).POST( _server.baseUri().resolve("/user/Neo4Net/password").ToString(), HTTP.RawPayload.quotedJson("{'password':'secret'}") );
			  assertEquals( 200, post.Status() );
		 }

		 private string DataURL()
		 {
			  return _server.baseUri().resolve("db/data/").ToString();
		 }

		 private string UserURL( string username )
		 {
			  return _server.baseUri().resolve("user/" + username).ToString();
		 }

		 private string PasswordURL( string username )
		 {
			  return _server.baseUri().resolve("user/" + username + "/password").ToString();
		 }

		 private string QuotedJson( string singleQuoted )
		 {
			  return singleQuoted.replaceAll( "'", "\"" );
		 }
	}

}