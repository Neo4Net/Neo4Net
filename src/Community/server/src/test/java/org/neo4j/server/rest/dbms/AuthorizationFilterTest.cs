using System.Text;

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
namespace Neo4Net.Server.rest.dbms
{
	using Base64 = org.apache.commons.codec.binary.Base64;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using AuthSubject = Neo4Net.@internal.Kernel.Api.security.AuthSubject;
	using AuthenticationResult = Neo4Net.@internal.Kernel.Api.security.AuthenticationResult;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using BasicAuthManager = Neo4Net.Server.Security.Auth.BasicAuthManager;
	using BasicLoginContext = Neo4Net.Server.Security.Auth.BasicLoginContext;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.same;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.security.LoginContext.AUTH_DISABLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.inLog;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.security.auth.SecurityTestUtils.authToken;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.AuthTokenUtil.authTokenArgumentMatcher;

	public class AuthorizationFilterTest
	{
		 private readonly BasicAuthManager _authManager = mock( typeof( BasicAuthManager ) );
		 private readonly AssertableLogProvider _logProvider = new AssertableLogProvider();
		 private readonly MemoryStream _outputStream = new MemoryStream();
		 private readonly HttpServletRequest _servletRequest = mock( typeof( HttpServletRequest ) );
		 private readonly HttpServletResponse _servletResponse = mock( typeof( HttpServletResponse ) );
		 private readonly FilterChain _filterChain = mock( typeof( FilterChain ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  when( _servletResponse.OutputStream ).thenReturn( new ServletOutputStreamAnonymousInnerClass( this ) );
		 }

		 private class ServletOutputStreamAnonymousInnerClass : ServletOutputStream
		 {
			 private readonly AuthorizationFilterTest _outerInstance;

			 public ServletOutputStreamAnonymousInnerClass( AuthorizationFilterTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void write( int b )
			 {
				  _outerInstance.outputStream.WriteByte( b );
			 }

			 public override bool Ready
			 {
				 get
				 {
					  return true;
				 }
			 }

			 public override WriteListener WriteListener
			 {
				 set
				 {
					  throw new System.NotSupportedException();
				 }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowOptionsRequests() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowOptionsRequests()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final AuthorizationEnabledFilter filter = new AuthorizationEnabledFilter(() -> authManager, logProvider);
			  AuthorizationEnabledFilter filter = new AuthorizationEnabledFilter( () => _authManager, _logProvider );
			  when( _servletRequest.Method ).thenReturn( "OPTIONS" );

			  // When
			  filter.DoFilter( _servletRequest, _servletResponse, _filterChain );

			  // Then
			  verify( _filterChain ).doFilter( same( _servletRequest ), same( _servletResponse ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWhitelistMatchingUris() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWhitelistMatchingUris()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final AuthorizationEnabledFilter filter = new AuthorizationEnabledFilter(() -> authManager, logProvider, java.util.regex.Pattern.compile("/"), java.util.regex.Pattern.compile("/browser.*"));
			  AuthorizationEnabledFilter filter = new AuthorizationEnabledFilter( () => _authManager, _logProvider, Pattern.compile("/"), Pattern.compile("/browser.*") );
			  when( _servletRequest.Method ).thenReturn( "GET" );
			  when( _servletRequest.ContextPath ).thenReturn( "/", "/browser/index.html" );

			  // When
			  filter.DoFilter( _servletRequest, _servletResponse, _filterChain );
			  filter.DoFilter( _servletRequest, _servletResponse, _filterChain );

			  // Then
			  verify( _filterChain, times( 2 ) ).doFilter( same( _servletRequest ), same( _servletResponse ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRequireAuthorizationForNonWhitelistedUris() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRequireAuthorizationForNonWhitelistedUris()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final AuthorizationEnabledFilter filter = new AuthorizationEnabledFilter(() -> authManager, logProvider, java.util.regex.Pattern.compile("/"), java.util.regex.Pattern.compile("/browser.*"));
			  AuthorizationEnabledFilter filter = new AuthorizationEnabledFilter( () => _authManager, _logProvider, Pattern.compile("/"), Pattern.compile("/browser.*") );
			  when( _servletRequest.Method ).thenReturn( "GET" );
			  when( _servletRequest.ContextPath ).thenReturn( "/db/data" );

			  // When
			  filter.DoFilter( _servletRequest, _servletResponse, _filterChain );

			  // Then
			  verifyNoMoreInteractions( _filterChain );
			  verify( _servletResponse ).Status = 401;
			  verify( _servletResponse ).addHeader( HttpHeaders.WWW_AUTHENTICATE, "Basic realm=\"Neo4j\"" );
			  verify( _servletResponse ).addHeader( HttpHeaders.CONTENT_TYPE, "application/json; charset=UTF-8" );
			  assertThat( _outputStream.ToString( StandardCharsets.UTF_8.name() ), containsString("\"code\" : \"Neo" + ".ClientError.Security.Unauthorized\"") );
			  assertThat( _outputStream.ToString( StandardCharsets.UTF_8.name() ), containsString("\"message\" : \"No authentication header supplied.\"") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRequireValidAuthorizationHeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRequireValidAuthorizationHeader()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final AuthorizationEnabledFilter filter = new AuthorizationEnabledFilter(() -> authManager, logProvider);
			  AuthorizationEnabledFilter filter = new AuthorizationEnabledFilter( () => _authManager, _logProvider );
			  when( _servletRequest.Method ).thenReturn( "GET" );
			  when( _servletRequest.ContextPath ).thenReturn( "/db/data" );
			  when( _servletRequest.getHeader( HttpHeaders.AUTHORIZATION ) ).thenReturn( "NOT A VALID VALUE" );

			  // When
			  filter.DoFilter( _servletRequest, _servletResponse, _filterChain );

			  // Then
			  verifyNoMoreInteractions( _filterChain );
			  verify( _servletResponse ).Status = 400;
			  verify( _servletResponse ).addHeader( HttpHeaders.CONTENT_TYPE, "application/json; charset=UTF-8" );
			  assertThat( _outputStream.ToString( StandardCharsets.UTF_8.name() ), containsString("\"code\" : \"Neo.ClientError.Request.InvalidFormat\"") );
			  assertThat( _outputStream.ToString( StandardCharsets.UTF_8.name() ), containsString("\"message\" : \"Invalid authentication header.\"") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAuthorizeInvalidCredentials() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAuthorizeInvalidCredentials()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final AuthorizationEnabledFilter filter = new AuthorizationEnabledFilter(() -> authManager, logProvider);
			  AuthorizationEnabledFilter filter = new AuthorizationEnabledFilter( () => _authManager, _logProvider );
			  string credentials = Base64.encodeBase64String( "foo:bar".GetBytes( Encoding.UTF8 ) );
			  BasicLoginContext loginContext = mock( typeof( BasicLoginContext ) );
			  AuthSubject authSubject = mock( typeof( AuthSubject ) );
			  when( _servletRequest.Method ).thenReturn( "GET" );
			  when( _servletRequest.ContextPath ).thenReturn( "/db/data" );
			  when( _servletRequest.getHeader( HttpHeaders.AUTHORIZATION ) ).thenReturn( "BASIC " + credentials );
			  when( _servletRequest.RemoteAddr ).thenReturn( "remote_ip_address" );
			  when( _authManager.login( authTokenArgumentMatcher( authToken( "foo", "bar" ) ) ) ).thenReturn( loginContext );
			  when( loginContext.Subject() ).thenReturn(authSubject);
			  when( authSubject.AuthenticationResult ).thenReturn( AuthenticationResult.FAILURE );

			  // When
			  filter.DoFilter( _servletRequest, _servletResponse, _filterChain );

			  // Then
			  verifyNoMoreInteractions( _filterChain );
			  _logProvider.assertExactly( inLog( typeof( AuthorizationEnabledFilter ) ).warn( "Failed authentication attempt for '%s' from %s", "foo", "remote_ip_address" ) );
			  verify( _servletResponse ).Status = 401;
			  verify( _servletResponse ).addHeader( HttpHeaders.CONTENT_TYPE, "application/json; charset=UTF-8" );
			  assertThat( _outputStream.ToString( StandardCharsets.UTF_8.name() ), containsString("\"code\" : \"Neo.ClientError.Security.Unauthorized\"") );
			  assertThat( _outputStream.ToString( StandardCharsets.UTF_8.name() ), containsString("\"message\" : \"Invalid username or password.\"") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAuthorizeWhenPasswordChangeRequiredForWhitelistedPath() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAuthorizeWhenPasswordChangeRequiredForWhitelistedPath()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final AuthorizationEnabledFilter filter = new AuthorizationEnabledFilter(() -> authManager, logProvider);
			  AuthorizationEnabledFilter filter = new AuthorizationEnabledFilter( () => _authManager, _logProvider );
			  string credentials = Base64.encodeBase64String( "foo:bar".GetBytes( Encoding.UTF8 ) );
			  BasicLoginContext loginContext = mock( typeof( BasicLoginContext ) );
			  AuthSubject authSubject = mock( typeof( AuthSubject ) );
			  when( _servletRequest.Method ).thenReturn( "GET" );
			  when( _servletRequest.ContextPath ).thenReturn( "/user/foo" );
			  when( _servletRequest.getHeader( HttpHeaders.AUTHORIZATION ) ).thenReturn( "BASIC " + credentials );
			  when( _authManager.login( authTokenArgumentMatcher( authToken( "foo", "bar" ) ) ) ).thenReturn( loginContext );
			  when( loginContext.Subject() ).thenReturn(authSubject);
			  when( authSubject.AuthenticationResult ).thenReturn( AuthenticationResult.PASSWORD_CHANGE_REQUIRED );

			  // When
			  filter.DoFilter( _servletRequest, _servletResponse, _filterChain );

			  // Then
			  verify( _filterChain ).doFilter( eq( new AuthorizedRequestWrapper( BASIC_AUTH, "foo", _servletRequest, AUTH_DISABLED ) ), same( _servletResponse ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAuthorizeWhenPasswordChangeRequired() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAuthorizeWhenPasswordChangeRequired()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final AuthorizationEnabledFilter filter = new AuthorizationEnabledFilter(() -> authManager, logProvider);
			  AuthorizationEnabledFilter filter = new AuthorizationEnabledFilter( () => _authManager, _logProvider );
			  string credentials = Base64.encodeBase64String( "foo:bar".GetBytes( Encoding.UTF8 ) );
			  BasicLoginContext loginContext = mock( typeof( BasicLoginContext ) );
			  AuthSubject authSubject = mock( typeof( AuthSubject ) );
			  when( _servletRequest.Method ).thenReturn( "GET" );
			  when( _servletRequest.ContextPath ).thenReturn( "/db/data" );
			  when( _servletRequest.RequestURL ).thenReturn( new StringBuilder( "http://bar.baz:7474/db/data/" ) );
			  when( _servletRequest.RequestURI ).thenReturn( "/db/data/" );
			  when( _servletRequest.getHeader( HttpHeaders.AUTHORIZATION ) ).thenReturn( "BASIC " + credentials );
			  when( _authManager.login( authTokenArgumentMatcher( authToken( "foo", "bar" ) ) ) ).thenReturn( loginContext );
			  when( loginContext.Subject() ).thenReturn(authSubject);
			  when( authSubject.AuthenticationResult ).thenReturn( AuthenticationResult.PASSWORD_CHANGE_REQUIRED );

			  // When
			  filter.DoFilter( _servletRequest, _servletResponse, _filterChain );

			  // Then
			  verifyNoMoreInteractions( _filterChain );
			  verify( _servletResponse ).Status = 403;
			  verify( _servletResponse ).addHeader( HttpHeaders.CONTENT_TYPE, "application/json; charset=UTF-8" );
			  assertThat( _outputStream.ToString( StandardCharsets.UTF_8.name() ), containsString("\"password_change\" : \"http://bar.baz:7474/user/foo/password\"") );
			  assertThat( _outputStream.ToString( StandardCharsets.UTF_8.name() ), containsString("\"code\" : \"Neo.ClientError.Security.Forbidden\"") );
			  assertThat( _outputStream.ToString( StandardCharsets.UTF_8.name() ), containsString("\"message\" : \"User is required to change their password.\"") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAuthorizeWhenTooManyAttemptsMade() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAuthorizeWhenTooManyAttemptsMade()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final AuthorizationEnabledFilter filter = new AuthorizationEnabledFilter(() -> authManager, logProvider);
			  AuthorizationEnabledFilter filter = new AuthorizationEnabledFilter( () => _authManager, _logProvider );
			  string credentials = Base64.encodeBase64String( "foo:bar".GetBytes( Encoding.UTF8 ) );
			  BasicLoginContext loginContext = mock( typeof( BasicLoginContext ) );
			  AuthSubject authSubject = mock( typeof( AuthSubject ) );
			  when( _servletRequest.Method ).thenReturn( "GET" );
			  when( _servletRequest.ContextPath ).thenReturn( "/db/data" );
			  when( _servletRequest.getHeader( HttpHeaders.AUTHORIZATION ) ).thenReturn( "BASIC " + credentials );
			  when( _authManager.login( authTokenArgumentMatcher( authToken( "foo", "bar" ) ) ) ).thenReturn( loginContext );
			  when( loginContext.Subject() ).thenReturn(authSubject);
			  when( authSubject.AuthenticationResult ).thenReturn( AuthenticationResult.TOO_MANY_ATTEMPTS );

			  // When
			  filter.DoFilter( _servletRequest, _servletResponse, _filterChain );

			  // Then
			  verifyNoMoreInteractions( _filterChain );
			  verify( _servletResponse ).Status = 429;
			  verify( _servletResponse ).addHeader( HttpHeaders.CONTENT_TYPE, "application/json; charset=UTF-8" );
			  assertThat( _outputStream.ToString( StandardCharsets.UTF_8.name() ), containsString("\"code\" : \"Neo.ClientError.Security.AuthenticationRateLimit\"") );
			  assertThat( _outputStream.ToString( StandardCharsets.UTF_8.name() ), containsString("\"message\" : \"Too many failed authentication requests. " + "Please wait 5 seconds and try again.\"") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAuthorizeWhenValidCredentialsSupplied() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAuthorizeWhenValidCredentialsSupplied()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final AuthorizationEnabledFilter filter = new AuthorizationEnabledFilter(() -> authManager, logProvider);
			  AuthorizationEnabledFilter filter = new AuthorizationEnabledFilter( () => _authManager, _logProvider );
			  string credentials = Base64.encodeBase64String( "foo:bar".GetBytes( Encoding.UTF8 ) );
			  BasicLoginContext loginContext = mock( typeof( BasicLoginContext ) );
			  AuthSubject authSubject = mock( typeof( AuthSubject ) );
			  when( _servletRequest.Method ).thenReturn( "GET" );
			  when( _servletRequest.ContextPath ).thenReturn( "/db/data" );
			  when( _servletRequest.getHeader( HttpHeaders.AUTHORIZATION ) ).thenReturn( "BASIC " + credentials );
			  when( _authManager.login( authTokenArgumentMatcher( authToken( "foo", "bar" ) ) ) ).thenReturn( loginContext );
			  when( loginContext.Subject() ).thenReturn(authSubject);
			  when( authSubject.AuthenticationResult ).thenReturn( AuthenticationResult.SUCCESS );

			  // When
			  filter.DoFilter( _servletRequest, _servletResponse, _filterChain );

			  // Then
			  verify( _filterChain ).doFilter( eq( new AuthorizedRequestWrapper( BASIC_AUTH, "foo", _servletRequest, AUTH_DISABLED ) ), same( _servletResponse ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludeCrippledAuthHeaderIfBrowserIsTheOneCalling() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIncludeCrippledAuthHeaderIfBrowserIsTheOneCalling()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final AuthorizationEnabledFilter filter = new AuthorizationEnabledFilter(() -> authManager, logProvider, java.util.regex.Pattern.compile("/"), java.util.regex.Pattern.compile("/browser.*"));
			  AuthorizationEnabledFilter filter = new AuthorizationEnabledFilter( () => _authManager, _logProvider, Pattern.compile("/"), Pattern.compile("/browser.*") );
			  when( _servletRequest.Method ).thenReturn( "GET" );
			  when( _servletRequest.ContextPath ).thenReturn( "/db/data" );
			  when( _servletRequest.getHeader( "X-Ajax-Browser-Auth" ) ).thenReturn( "true" );

			  // When
			  filter.DoFilter( _servletRequest, _servletResponse, _filterChain );

			  // Then
			  verifyNoMoreInteractions( _filterChain );
			  verify( _servletResponse ).Status = 401;
			  verify( _servletResponse ).addHeader( HttpHeaders.WWW_AUTHENTICATE, "None" );
			  verify( _servletResponse ).addHeader( HttpHeaders.CONTENT_TYPE, "application/json; charset=UTF-8" );
			  assertThat( _outputStream.ToString( StandardCharsets.UTF_8.name() ), containsString("\"code\" : \"Neo.ClientError.Security.Unauthorized\"") );
			  assertThat( _outputStream.ToString( StandardCharsets.UTF_8.name() ), containsString("\"message\" : \"No authentication header supplied.\"") );
		 }
	}

}