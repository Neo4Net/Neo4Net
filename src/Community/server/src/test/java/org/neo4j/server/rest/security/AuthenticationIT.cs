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
namespace Neo4Net.Server.rest.security
{
	using JsonNode = org.codehaus.jackson.JsonNode;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;
	using JsonParseException = Neo4Net.Server.rest.domain.JsonParseException;
	using Neo4Net.Test;
	using HTTP = Neo4Net.Test.server.HTTP;
	using RawPayload = Neo4Net.Test.server.HTTP.RawPayload;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class AuthenticationIT : CommunityServerTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.TestData<org.neo4j.server.rest.RESTRequestGenerator> gen = org.neo4j.test.TestData.producedThrough(org.neo4j.server.rest.RESTRequestGenerator.PRODUCER);
		 public TestData<RESTRequestGenerator> Gen = TestData.producedThrough( RESTRequestGenerator.PRODUCER );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Documented("Missing authorization\n" + "\n" + "If an +Authorization+ header is not supplied, the server will reply with an error.") public void missing_authorization() throws org.neo4j.server.rest.domain.JsonParseException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Missing authorization\n" + "\n" + "If an +Authorization+ header is not supplied, the server will reply with an error.")]
		 public virtual void MissingAuthorization()
		 {
			  // Given
			  StartServerWithConfiguredUser();

			  // Document
			  RESTRequestGenerator.ResponseEntity response = Gen.get().expectedStatus(401).expectedHeader("WWW-Authenticate", "Basic realm=\"Neo4j\"").get(DataURL());

			  // Then
			  JsonNode data = JsonHelper.jsonNode( response.Entity() );
			  JsonNode firstError = data.get( "errors" ).get( 0 );
			  assertThat( firstError.get( "code" ).asText(), equalTo("Neo.ClientError.Security.Unauthorized") );
			  assertThat( firstError.get( "message" ).asText(), equalTo("No authentication header supplied.") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Documented("Authenticate to access the server\n" + "\n" + "Authenticate by sending a username and a password to Neo4j using HTTP Basic Auth.\n" + "Requests should include an +Authorization+ header, with a value of +Basic <payload>+,\n" + "where \"payload\" is a base64 encoded string of \"username:password\".") public void successful_authentication() throws org.neo4j.server.rest.domain.JsonParseException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Authenticate to access the server\n" + "\n" + "Authenticate by sending a username and a password to Neo4j using HTTP Basic Auth.\n" + "Requests should include an +Authorization+ header, with a value of +Basic <payload>+,\n" + "where \"payload\" is a base64 encoded string of \"username:password\".")]
		 public virtual void SuccessfulAuthentication()
		 {
			  // Given
			  StartServerWithConfiguredUser();

			  // Document
			  RESTRequestGenerator.ResponseEntity response = Gen.get().expectedStatus(200).withHeader(HttpHeaders.AUTHORIZATION, HTTP.basicAuthHeader("neo4j", "secret")).get(UserURL("neo4j"));

			  // Then
			  JsonNode data = JsonHelper.jsonNode( response.Entity() );
			  assertThat( data.get( "username" ).asText(), equalTo("neo4j") );
			  assertThat( data.get( "password_change_required" ).asBoolean(), equalTo(false) );
			  assertThat( data.get( "password_change" ).asText(), equalTo(PasswordURL("neo4j")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Documented("Incorrect authentication\n" + "\n" + "If an incorrect username or password is provided, the server replies with an error.") public void incorrect_authentication() throws org.neo4j.server.rest.domain.JsonParseException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Incorrect authentication\n" + "\n" + "If an incorrect username or password is provided, the server replies with an error.")]
		 public virtual void IncorrectAuthentication()
		 {
			  // Given
			  StartServerWithConfiguredUser();

			  // Document
			  RESTRequestGenerator.ResponseEntity response = Gen.get().expectedStatus(401).withHeader(HttpHeaders.AUTHORIZATION, HTTP.basicAuthHeader("neo4j", "incorrect")).expectedHeader("WWW-Authenticate", "Basic realm=\"Neo4j\"").post(DataURL());

			  // Then
			  JsonNode data = JsonHelper.jsonNode( response.Entity() );
			  JsonNode firstError = data.get( "errors" ).get( 0 );
			  assertThat( firstError.get( "code" ).asText(), equalTo("Neo.ClientError.Security.Unauthorized") );
			  assertThat( firstError.get( "message" ).asText(), equalTo("Invalid username or password.") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Documented("Required password changes\n" + "\n" + "In some cases, like the very first time Neo4j is accessed, the user will be required to choose\n" + "a new password. The database will signal that a new password is required and deny access.\n" + "\n" + "See <<rest-api-security-user-status-and-password-changing>> for how to set a new password.") public void password_change_required() throws org.neo4j.server.rest.domain.JsonParseException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("Required password changes\n" + "\n" + "In some cases, like the very first time Neo4j is accessed, the user will be required to choose\n" + "a new password. The database will signal that a new password is required and deny access.\n" + "\n" + "See <<rest-api-security-user-status-and-password-changing>> for how to set a new password.")]
		 public virtual void PasswordChangeRequired()
		 {
			  // Given
			  StartServer( true );

			  // Document
			  RESTRequestGenerator.ResponseEntity response = Gen.get().expectedStatus(403).withHeader(HttpHeaders.AUTHORIZATION, HTTP.basicAuthHeader("neo4j", "neo4j")).get(DataURL());

			  // Then
			  JsonNode data = JsonHelper.jsonNode( response.Entity() );
			  JsonNode firstError = data.get( "errors" ).get( 0 );
			  assertThat( firstError.get( "code" ).asText(), equalTo("Neo.ClientError.Security.Forbidden") );
			  assertThat( firstError.get( "message" ).asText(), equalTo("User is required to change their password.") );
			  assertThat( data.get( "password_change" ).asText(), equalTo(PasswordURL("neo4j")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Documented("When auth is disabled\n" + "\n" + "When auth has been disabled in the configuration, requests can be sent without an +Authorization+ header.") public void auth_disabled() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Documented("When auth is disabled\n" + "\n" + "When auth has been disabled in the configuration, requests can be sent without an +Authorization+ header.")]
		 public virtual void AuthDisabled()
		 {
			  // Given
			  StartServer( false );

			  // Document
			  Gen.get().expectedStatus(200).get(DataURL());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSayMalformedHeaderIfMalformedAuthorization() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSayMalformedHeaderIfMalformedAuthorization()
		 {
			  // Given
			  StartServerWithConfiguredUser();

			  // When
			  HTTP.Response response = HTTP.withHeaders( HttpHeaders.AUTHORIZATION, "This makes no sense" ).GET( DataURL() );

			  // Then
			  assertThat( response.Status(), equalTo(400) );
			  assertThat( response.Get( "errors" ).get( 0 ).get( "code" ).asText(), equalTo("Neo.ClientError.Request.InvalidFormat") );
			  assertThat( response.Get( "errors" ).get( 0 ).get( "message" ).asText(), equalTo("Invalid authentication header.") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowDataAccess() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowDataAccess()
		 {
			  // Given
			  StartServerWithConfiguredUser();

			  // When & then
			  AssertAuthorizationRequired( "POST", "db/data/node", HTTP.RawPayload.quotedJson( "{'name':'jake'}" ), 201 );
			  AssertAuthorizationRequired( "GET", "db/data/node/1234", 404 );
			  AssertAuthorizationRequired( "POST", "db/data/transaction/commit", HTTP.RawPayload.quotedJson( "{'statements':[{'statement':'MATCH (n) RETURN n'}]}" ), 200 );

			  assertEquals( 200, HTTP.GET( Server.baseUri().resolve("").ToString() ).status() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowAllAccessIfAuthenticationIsDisabled() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowAllAccessIfAuthenticationIsDisabled()
		 {
			  // Given
			  StartServer( false );

			  // When & then
			  assertEquals( 201, HTTP.POST( Server.baseUri().resolve("db/data/node").ToString(), HTTP.RawPayload.quotedJson("{'name':'jake'}") ).status() );
			  assertEquals( 404, HTTP.GET( Server.baseUri().resolve("db/data/node/1234").ToString() ).status() );
			  assertEquals( 200, HTTP.POST( Server.baseUri().resolve("db/data/transaction/commit").ToString(), HTTP.RawPayload.quotedJson("{'statements':[{'statement':'MATCH (n) RETURN n'}]}") ).status() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReplyNicelyToTooManyFailedAuthAttempts() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReplyNicelyToTooManyFailedAuthAttempts()
		 {
			  // Given
			  StartServerWithConfiguredUser();
			  long timeout = DateTimeHelper.CurrentUnixTimeMillis() + 30_000;

			  // When
			  HTTP.Response response = null;
			  while ( DateTimeHelper.CurrentUnixTimeMillis() < timeout )
			  {
					// Done in a loop because we're racing with the clock to get enough failed requests into 5 seconds
					response = HTTP.withBasicAuth( "neo4j", "incorrect" ).POST( Server.baseUri().resolve("authentication").ToString(), HTTP.RawPayload.quotedJson("{'username':'neo4j', 'password':'something that is wrong'}") );

					if ( response.Status() == 429 )
					{
						 break;
					}
			  }

			  // Then
			  assertThat( response.Status(), equalTo(429) );
			  JsonNode firstError = response.Get( "errors" ).get( 0 );
			  assertThat( firstError.get( "code" ).asText(), equalTo("Neo.ClientError.Security.AuthenticationRateLimit") );
			  assertThat( firstError.get( "message" ).asText(), equalTo("Too many failed authentication requests. Please wait 5 seconds and try again.") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowDataAccessForUnauthorizedUser() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowDataAccessForUnauthorizedUser()
		 {
			  // Given
			  StartServer( true ); // The user should not have read access before changing the password

			  // When
			  HTTP.Response response = HTTP.withBasicAuth( "neo4j", "neo4j" ).POST( Server.baseUri().resolve("authentication").ToString(), HTTP.RawPayload.quotedJson("{'username':'neo4j', 'password':'neo4j'}") );

			  // When & then
			  assertEquals( 403, HTTP.withBasicAuth( "neo4j", "neo4j" ).POST( Server.baseUri().resolve("db/data/node").ToString(), HTTP.RawPayload.quotedJson("{'name':'jake'}") ).status() );
			  assertEquals( 403, HTTP.withBasicAuth( "neo4j", "neo4j" ).GET( Server.baseUri().resolve("db/data/node/1234").ToString() ).status() );
			  assertEquals( 403, HTTP.withBasicAuth( "neo4j", "neo4j" ).POST( Server.baseUri().resolve("db/data/transaction/commit").ToString(), HTTP.RawPayload.quotedJson("{'statements':[{'statement':'MATCH (n) RETURN n'}]}") ).status() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertAuthorizationRequired(String method, String path, int expectedAuthorizedStatus) throws org.neo4j.server.rest.domain.JsonParseException
		 private void AssertAuthorizationRequired( string method, string path, int expectedAuthorizedStatus )
		 {
			  AssertAuthorizationRequired( method, path, null, expectedAuthorizedStatus );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertAuthorizationRequired(String method, String path, Object payload, int expectedAuthorizedStatus) throws org.neo4j.server.rest.domain.JsonParseException
		 private void AssertAuthorizationRequired( string method, string path, object payload, int expectedAuthorizedStatus )
		 {
			  // When no header
			  HTTP.Response response = HTTP.request( method, Server.baseUri().resolve(path).ToString(), payload );
			  assertThat( response.Status(), equalTo(401) );
			  assertThat( response.Get( "errors" ).get( 0 ).get( "code" ).asText(), equalTo("Neo.ClientError.Security.Unauthorized") );
			  assertThat( response.Get( "errors" ).get( 0 ).get( "message" ).asText(), equalTo("No authentication header supplied.") );
			  assertThat( response.Header( HttpHeaders.WWW_AUTHENTICATE ), equalTo( "Basic realm=\"Neo4j\"" ) );

			  // When malformed header
			  response = HTTP.withHeaders( HttpHeaders.AUTHORIZATION, "This makes no sense" ).request( method, Server.baseUri().resolve(path).ToString(), payload );
			  assertThat( response.Status(), equalTo(400) );
			  assertThat( response.Get( "errors" ).get( 0 ).get( "code" ).asText(), equalTo("Neo.ClientError.Request.InvalidFormat") );
			  assertThat( response.Get( "errors" ).get( 0 ).get( "message" ).asText(), equalTo("Invalid authentication header.") );

			  // When invalid credential
			  response = HTTP.withBasicAuth( "neo4j", "incorrect" ).request( method, Server.baseUri().resolve(path).ToString(), payload );
			  assertThat( response.Status(), equalTo(401) );
			  assertThat( response.Get( "errors" ).get( 0 ).get( "code" ).asText(), equalTo("Neo.ClientError.Security.Unauthorized") );
			  assertThat( response.Get( "errors" ).get( 0 ).get( "message" ).asText(), equalTo("Invalid username or password.") );
			  assertThat( response.Header( HttpHeaders.WWW_AUTHENTICATE ), equalTo( "Basic realm=\"Neo4j\"" ) );

			  // When authorized
			  response = HTTP.withBasicAuth( "neo4j", "secret" ).request( method, Server.baseUri().resolve(path).ToString(), payload );
			  assertThat( response.Status(), equalTo(expectedAuthorizedStatus) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void startServerWithConfiguredUser() throws java.io.IOException
		 public virtual void StartServerWithConfiguredUser()
		 {
			  StartServer( true );
			  // Set the password
			  HTTP.Response post = HTTP.withBasicAuth( "neo4j", "neo4j" ).POST( Server.baseUri().resolve("/user/neo4j/password").ToString(), HTTP.RawPayload.quotedJson("{'password':'secret'}") );
			  assertEquals( 200, post.Status() );
		 }
	}

}