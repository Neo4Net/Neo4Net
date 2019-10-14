using System.Collections.Generic;

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
namespace Neo4Net.Server.security.enterprise.auth
{
	using Test = org.junit.Test;

	using AuthToken = Neo4Net.Kernel.api.security.AuthToken;
	using InvalidAuthTokenException = Neo4Net.Kernel.api.security.exception.InvalidAuthTokenException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.auth.BasicAuthManagerTest.password;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.AuthTokenUtil.authTokenMatcher;

	public class ShiroAuthTokenTest
	{
		 private const string USERNAME = "myuser";
		 private const string PASSWORD = "mypw123";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSupportBasicAuthToken() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSupportBasicAuthToken()
		 {
			  ShiroAuthToken token = new ShiroAuthToken( AuthToken.newBasicAuthToken( USERNAME, PASSWORD ) );
			  TestBasicAuthToken( token, USERNAME, PASSWORD, Neo4Net.Kernel.api.security.AuthToken_Fields.BASIC_SCHEME );
			  assertThat( "Token map should have only expected values", token.AuthTokenMap, authTokenMatcher( map( Neo4Net.Kernel.api.security.AuthToken_Fields.PRINCIPAL, USERNAME, Neo4Net.Kernel.api.security.AuthToken_Fields.CREDENTIALS, PASSWORD, Neo4Net.Kernel.api.security.AuthToken_Fields.SCHEME_KEY, Neo4Net.Kernel.api.security.AuthToken_Fields.BASIC_SCHEME ) ) );
			  TestTokenSupportsRealm( token, true, "unknown", "native", "ldap" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSupportBasicAuthTokenWithEmptyRealm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSupportBasicAuthTokenWithEmptyRealm()
		 {
			  ShiroAuthToken token = new ShiroAuthToken( AuthToken.newBasicAuthToken( USERNAME, PASSWORD, "" ) );
			  TestBasicAuthToken( token, USERNAME, PASSWORD, Neo4Net.Kernel.api.security.AuthToken_Fields.BASIC_SCHEME );
			  assertThat( "Token map should have only expected values", token.AuthTokenMap, authTokenMatcher( map( Neo4Net.Kernel.api.security.AuthToken_Fields.PRINCIPAL, USERNAME, Neo4Net.Kernel.api.security.AuthToken_Fields.CREDENTIALS, PASSWORD, Neo4Net.Kernel.api.security.AuthToken_Fields.SCHEME_KEY, Neo4Net.Kernel.api.security.AuthToken_Fields.BASIC_SCHEME, Neo4Net.Kernel.api.security.AuthToken_Fields.REALM_KEY, "" ) ) );
			  TestTokenSupportsRealm( token, true, "unknown", "native", "ldap" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSupportBasicAuthTokenWithNullRealm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSupportBasicAuthTokenWithNullRealm()
		 {
			  ShiroAuthToken token = new ShiroAuthToken( AuthToken.newBasicAuthToken( USERNAME, PASSWORD, null ) );
			  TestBasicAuthToken( token, USERNAME, PASSWORD, Neo4Net.Kernel.api.security.AuthToken_Fields.BASIC_SCHEME );
			  assertThat( "Token map should have only expected values", token.AuthTokenMap, authTokenMatcher( map( Neo4Net.Kernel.api.security.AuthToken_Fields.PRINCIPAL, USERNAME, Neo4Net.Kernel.api.security.AuthToken_Fields.CREDENTIALS, PASSWORD, Neo4Net.Kernel.api.security.AuthToken_Fields.SCHEME_KEY, Neo4Net.Kernel.api.security.AuthToken_Fields.BASIC_SCHEME, Neo4Net.Kernel.api.security.AuthToken_Fields.REALM_KEY, null ) ) );
			  TestTokenSupportsRealm( token, true, "unknown", "native", "ldap" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSupportBasicAuthTokenWithWildcardRealm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSupportBasicAuthTokenWithWildcardRealm()
		 {
			  ShiroAuthToken token = new ShiroAuthToken( AuthToken.newBasicAuthToken( USERNAME, PASSWORD, "*" ) );
			  TestBasicAuthToken( token, USERNAME, PASSWORD, Neo4Net.Kernel.api.security.AuthToken_Fields.BASIC_SCHEME );
			  assertThat( "Token map should have only expected values", token.AuthTokenMap, authTokenMatcher( map( Neo4Net.Kernel.api.security.AuthToken_Fields.PRINCIPAL, USERNAME, Neo4Net.Kernel.api.security.AuthToken_Fields.CREDENTIALS, PASSWORD, Neo4Net.Kernel.api.security.AuthToken_Fields.SCHEME_KEY, Neo4Net.Kernel.api.security.AuthToken_Fields.BASIC_SCHEME, Neo4Net.Kernel.api.security.AuthToken_Fields.REALM_KEY, "*" ) ) );
			  TestTokenSupportsRealm( token, true, "unknown", "native", "ldap" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSupportBasicAuthTokenWithSpecificRealm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSupportBasicAuthTokenWithSpecificRealm()
		 {
			  string realm = "ldap";
			  ShiroAuthToken token = new ShiroAuthToken( AuthToken.newBasicAuthToken( USERNAME, PASSWORD, realm ) );
			  TestBasicAuthToken( token, USERNAME, PASSWORD, Neo4Net.Kernel.api.security.AuthToken_Fields.BASIC_SCHEME );
			  assertThat( "Token map should have only expected values", token.AuthTokenMap, authTokenMatcher( map( Neo4Net.Kernel.api.security.AuthToken_Fields.PRINCIPAL, USERNAME, Neo4Net.Kernel.api.security.AuthToken_Fields.CREDENTIALS, PASSWORD, Neo4Net.Kernel.api.security.AuthToken_Fields.SCHEME_KEY, Neo4Net.Kernel.api.security.AuthToken_Fields.BASIC_SCHEME, Neo4Net.Kernel.api.security.AuthToken_Fields.REALM_KEY, "ldap" ) ) );
			  TestTokenSupportsRealm( token, true, realm );
			  TestTokenSupportsRealm( token, false, "unknown", "native" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSupportCustomAuthTokenWithSpecificRealm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSupportCustomAuthTokenWithSpecificRealm()
		 {
			  string realm = "ldap";
			  ShiroAuthToken token = new ShiroAuthToken( AuthToken.newCustomAuthToken( USERNAME, PASSWORD, realm, Neo4Net.Kernel.api.security.AuthToken_Fields.BASIC_SCHEME ) );
			  TestBasicAuthToken( token, USERNAME, PASSWORD, Neo4Net.Kernel.api.security.AuthToken_Fields.BASIC_SCHEME );
			  assertThat( "Token map should have only expected values", token.AuthTokenMap, authTokenMatcher( map( Neo4Net.Kernel.api.security.AuthToken_Fields.PRINCIPAL, USERNAME, Neo4Net.Kernel.api.security.AuthToken_Fields.CREDENTIALS, PASSWORD, Neo4Net.Kernel.api.security.AuthToken_Fields.SCHEME_KEY, Neo4Net.Kernel.api.security.AuthToken_Fields.BASIC_SCHEME, Neo4Net.Kernel.api.security.AuthToken_Fields.REALM_KEY, "ldap" ) ) );
			  TestTokenSupportsRealm( token, true, realm );
			  TestTokenSupportsRealm( token, false, "unknown", "native" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSupportCustomAuthTokenWithSpecificRealmAndParameters() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSupportCustomAuthTokenWithSpecificRealmAndParameters()
		 {
			  string realm = "ldap";
			  IDictionary<string, object> @params = map( "a", "A", "b", "B" );
			  ShiroAuthToken token = new ShiroAuthToken( AuthToken.newCustomAuthToken( USERNAME, PASSWORD, realm, Neo4Net.Kernel.api.security.AuthToken_Fields.BASIC_SCHEME, @params ) );
			  TestBasicAuthToken( token, USERNAME, PASSWORD, Neo4Net.Kernel.api.security.AuthToken_Fields.BASIC_SCHEME );
			  assertThat( "Token map should have only expected values", token.AuthTokenMap, authTokenMatcher( map( Neo4Net.Kernel.api.security.AuthToken_Fields.PRINCIPAL, USERNAME, Neo4Net.Kernel.api.security.AuthToken_Fields.CREDENTIALS, PASSWORD, Neo4Net.Kernel.api.security.AuthToken_Fields.SCHEME_KEY, Neo4Net.Kernel.api.security.AuthToken_Fields.BASIC_SCHEME, Neo4Net.Kernel.api.security.AuthToken_Fields.REALM_KEY, "ldap", "parameters", @params ) ) );
			  TestTokenSupportsRealm( token, true, realm );
			  TestTokenSupportsRealm( token, false, "unknown", "native" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveStringRepresentationWithNullRealm() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHaveStringRepresentationWithNullRealm()
		 {
			  ShiroAuthToken token = new ShiroAuthToken( AuthToken.newBasicAuthToken( USERNAME, PASSWORD, null ) );
			  TestBasicAuthToken( token, USERNAME, PASSWORD, Neo4Net.Kernel.api.security.AuthToken_Fields.BASIC_SCHEME );

			  string stringRepresentation = token.ToString();
			  assertThat( stringRepresentation, containsString( "realm='null'" ) );
		 }

		 private void TestTokenSupportsRealm( ShiroAuthToken token, bool supports, params string[] realms )
		 {
			  foreach ( string realm in realms )
			  {
					assertThat( "Token should support '" + realm + "' realm", token.SupportsRealm( realm ), equalTo( supports ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void testBasicAuthToken(ShiroAuthToken token, String username, String password, String scheme) throws org.neo4j.kernel.api.security.exception.InvalidAuthTokenException
		 private void TestBasicAuthToken( ShiroAuthToken token, string username, string password, string scheme )
		 {
			  assertThat( "Token should have basic scheme", token.Scheme, equalTo( scheme ) );
			  assertThat( "Token have correct principal", token.Principal, equalTo( username ) );
			  assertThat( "Token have correct credentials", token.Credentials, equalTo( password( password ) ) );
		 }
	}

}