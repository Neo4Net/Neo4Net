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
	using Test = org.junit.Test;

	using HttpMethod = Neo4Net.Server.web.HttpMethod;
	using HTTP = Neo4Net.Test.server.HTTP;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.sun.jersey.api.client.ClientResponse.Status.FORBIDDEN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.sun.jersey.api.client.ClientResponse.Status.OK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.sun.jersey.api.client.ClientResponse.Status.UNAUTHORIZED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.web.CorsFilter.ACCESS_CONTROL_ALLOW_HEADERS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.web.CorsFilter.ACCESS_CONTROL_ALLOW_METHODS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.web.CorsFilter.ACCESS_CONTROL_ALLOW_ORIGIN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.web.CorsFilter.ACCESS_CONTROL_REQUEST_HEADERS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.web.CorsFilter.ACCESS_CONTROL_REQUEST_METHOD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.web.HttpMethod.DELETE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.web.HttpMethod.GET;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.web.HttpMethod.PATCH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.web.HttpMethod.POST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.server.HTTP.RawPayload.quotedJson;

	public class AuthorizationCorsIT : CommunityServerTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddCorsHeaderWhenAuthDisabled() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAddCorsHeaderWhenAuthDisabled()
		 {
			  StartServer( false );

			  HTTP.Response response = RunQuery( "authDisabled", "authDisabled" );

			  assertEquals( OK.StatusCode, response.Status() );
			  AssertCorsHeaderPresent( response );
			  assertThat( response.Content().ToString(), containsString("42") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddCorsHeaderWhenAuthEnabledAndPasswordChangeRequired() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAddCorsHeaderWhenAuthEnabledAndPasswordChangeRequired()
		 {
			  StartServer( true );

			  HTTP.Response response = RunQuery( "Neo4Net", "Neo4Net" );

			  assertEquals( FORBIDDEN.StatusCode, response.Status() );
			  AssertCorsHeaderPresent( response );
			  assertThat( response.Content().ToString(), containsString("password_change") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddCorsHeaderWhenAuthEnabledAndPasswordChangeNotRequired() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAddCorsHeaderWhenAuthEnabledAndPasswordChangeNotRequired()
		 {
			  StartServer( true );
			  HTTP.Response passwordChangeResponse = ChangePassword( "Neo4Net", "Neo4Net", "newPassword" );
			  assertEquals( OK.StatusCode, passwordChangeResponse.Status() );
			  AssertCorsHeaderPresent( passwordChangeResponse );

			  HTTP.Response queryResponse = RunQuery( "Neo4Net", "newPassword" );

			  assertEquals( OK.StatusCode, queryResponse.Status() );
			  AssertCorsHeaderPresent( queryResponse );
			  assertThat( queryResponse.Content().ToString(), containsString("42") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddCorsHeaderWhenAuthEnabledAndIncorrectPassword() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAddCorsHeaderWhenAuthEnabledAndIncorrectPassword()
		 {
			  StartServer( true );

			  HTTP.Response response = RunQuery( "Neo4Net", "wrongPassword" );

			  assertEquals( UNAUTHORIZED.StatusCode, response.Status() );
			  AssertCorsHeaderPresent( response );
			  assertThat( response.Content().ToString(), containsString("Neo.ClientError.Security.Unauthorized") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddCorsMethodsHeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAddCorsMethodsHeader()
		 {
			  StartServer( false );

			  TestCorsAllowMethods( POST );
			  TestCorsAllowMethods( GET );
			  TestCorsAllowMethods( PATCH );
			  TestCorsAllowMethods( DELETE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddCorsHeaderWhenConfigured() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAddCorsHeaderWhenConfigured()
		 {
			  string origin = "https://example.com:7687";
			  StartServer( false, origin );

			  TestCorsAllowMethods( POST, origin );
			  TestCorsAllowMethods( GET, origin );
			  TestCorsAllowMethods( PATCH, origin );
			  TestCorsAllowMethods( DELETE, origin );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddCorsRequestHeaders() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAddCorsRequestHeaders()
		 {
			  StartServer( false );

			  HTTP.Builder requestBuilder = RequestWithHeaders( "authDisabled", "authDisabled" ).withHeaders( ACCESS_CONTROL_REQUEST_HEADERS, "Accept, X-Not-Accept" );
			  HTTP.Response response = RunQuery( requestBuilder );

			  assertEquals( OK.StatusCode, response.Status() );
			  AssertCorsHeaderPresent( response );
			  assertEquals( "Accept, X-Not-Accept", response.Header( ACCESS_CONTROL_ALLOW_HEADERS ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testCorsAllowMethods(org.Neo4Net.server.web.HttpMethod method) throws Exception
		 private void TestCorsAllowMethods( HttpMethod method )
		 {
			  TestCorsAllowMethods( method, "*" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testCorsAllowMethods(org.Neo4Net.server.web.HttpMethod method, String origin) throws Exception
		 private void TestCorsAllowMethods( HttpMethod method, string origin )
		 {
			  HTTP.Builder requestBuilder = RequestWithHeaders( "authDisabled", "authDisabled" ).withHeaders( ACCESS_CONTROL_REQUEST_METHOD, method.ToString() );
			  HTTP.Response response = RunQuery( requestBuilder );

			  assertEquals( OK.StatusCode, response.Status() );
			  AssertCorsHeaderEquals( response, origin );
			  assertEquals( method, HttpMethod.ValueOf( response.Header( ACCESS_CONTROL_ALLOW_METHODS ) ) );
		 }

		 private HTTP.Response ChangePassword( string username, string oldPassword, string newPassword )
		 {
			  HTTP.RawPayload passwordChange = quotedJson( "{'password': '" + newPassword + "'}" );
			  return RequestWithHeaders( username, oldPassword ).POST( PasswordURL( username ), passwordChange );
		 }

		 private HTTP.Response RunQuery( string username, string password )
		 {
			  return RunQuery( RequestWithHeaders( username, password ) );
		 }

		 private HTTP.Response RunQuery( HTTP.Builder requestBuilder )
		 {
			  HTTP.RawPayload statements = quotedJson( "{'statements': [{'statement': 'RETURN 42'}]}" );
			  return requestBuilder.Post( TxCommitURL(), statements );
		 }

		 private static HTTP.Builder RequestWithHeaders( string username, string password )
		 {
			  return HTTP.withBasicAuth( username, password ).withHeaders( HttpHeaders.ACCEPT, "application/json; charset=UTF-8", HttpHeaders.CONTENT_TYPE, "application/json" );
		 }

		 private static void AssertCorsHeaderPresent( HTTP.Response response )
		 {
			  AssertCorsHeaderEquals( response, "*" );
		 }

		 private static void AssertCorsHeaderEquals( HTTP.Response response, string origin )
		 {
			  assertEquals( origin, response.Header( ACCESS_CONTROL_ALLOW_ORIGIN ) );
		 }
	}

}