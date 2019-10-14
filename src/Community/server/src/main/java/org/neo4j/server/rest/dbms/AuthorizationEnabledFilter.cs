using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.Server.rest.dbms
{

	using Neo4Net.Functions;
	using AuthProviderFailedException = Neo4Net.Graphdb.security.AuthProviderFailedException;
	using AuthProviderTimeoutException = Neo4Net.Graphdb.security.AuthProviderTimeoutException;
	using AuthorizationViolationException = Neo4Net.Graphdb.security.AuthorizationViolationException;
	using LoginContext = Neo4Net.@internal.Kernel.Api.security.LoginContext;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using AuthManager = Neo4Net.Kernel.api.security.AuthManager;
	using InvalidAuthTokenException = Neo4Net.Kernel.api.security.exception.InvalidAuthTokenException;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using JettyHttpConnection = Neo4Net.Server.web.JettyHttpConnection;
	using XForwardUtil = Neo4Net.Server.web.XForwardUtil;
	using UTF8 = Neo4Net.Strings.UTF8;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.security.AuthToken.newBasicAuthToken;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.web.XForwardUtil.X_FORWARD_HOST_HEADER_KEY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.web.XForwardUtil.X_FORWARD_PROTO_HEADER_KEY;

	public class AuthorizationEnabledFilter : AuthorizationFilter
	{
		 private static readonly Pattern _passwordChangeWhitelist = Pattern.compile( "/user/.*" );

		 private readonly System.Func<AuthManager> _authManagerSupplier;
		 private readonly Log _log;
		 private readonly Pattern[] _uriWhitelist;

		 public AuthorizationEnabledFilter( System.Func<AuthManager> authManager, LogProvider logProvider, params Pattern[] uriWhitelist )
		 {
			  this._authManagerSupplier = authManager;
			  this._log = logProvider.getLog( this.GetType() );
			  this._uriWhitelist = uriWhitelist;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void doFilter(javax.servlet.ServletRequest servletRequest, javax.servlet.ServletResponse servletResponse, javax.servlet.FilterChain filterChain) throws java.io.IOException, javax.servlet.ServletException
		 public override void DoFilter( ServletRequest servletRequest, ServletResponse servletResponse, FilterChain filterChain )
		 {
			  ValidateRequestType( servletRequest );
			  ValidateResponseType( servletResponse );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.servlet.http.HttpServletRequest request = (javax.servlet.http.HttpServletRequest) servletRequest;
			  HttpServletRequest request = ( HttpServletRequest ) servletRequest;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final javax.servlet.http.HttpServletResponse response = (javax.servlet.http.HttpServletResponse) servletResponse;
			  HttpServletResponse response = ( HttpServletResponse ) servletResponse;

			  string userAgent = request.getHeader( HttpHeaders.USER_AGENT );
			  // username is only known after authentication, make connection aware of the user-agent
			  JettyHttpConnection.updateUserForCurrentConnection( null, userAgent );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String path = request.getContextPath() + (request.getPathInfo() == null ? "" : request.getPathInfo());
			  string path = request.ContextPath + ( request.PathInfo == null ? "" : request.PathInfo );

			  if ( request.Method.Equals( "OPTIONS" ) || Whitelisted( path ) )
			  {
					// NOTE: If starting transactions with access mode on whitelisted uris should be possible we need to
					//       wrap servletRequest in an AuthorizedRequestWrapper here
					filterChain.doFilter( servletRequest, servletResponse );
					return;
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String header = request.getHeader(javax.ws.rs.core.HttpHeaders.AUTHORIZATION);
			  string header = request.getHeader( HttpHeaders.AUTHORIZATION );
			  if ( string.ReferenceEquals( header, null ) )
			  {
					RequestAuthentication( request, _noHeader ).accept( response );
					return;
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String[] usernameAndPassword = extractCredential(header);
			  string[] usernameAndPassword = ExtractCredential( header );
			  if ( usernameAndPassword == null )
			  {
					_badHeader.accept( response );
					return;
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String username = usernameAndPassword[0];
			  string username = usernameAndPassword[0];
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String password = usernameAndPassword[1];
			  string password = usernameAndPassword[1];

			  try
			  {
					LoginContext securityContext = Authenticate( username, password );
					// username is now known, make connection aware of both username and user-agent
					JettyHttpConnection.updateUserForCurrentConnection( username, userAgent );

					switch ( securityContext.Subject().AuthenticationResult )
					{
					case PASSWORD_CHANGE_REQUIRED:
						 if ( !_passwordChangeWhitelist.matcher( path ).matches() )
						 {
							  PasswordChangeRequired( username, BaseURL( request ) ).accept( response );
							  return;
						 }
						 // fall through
					case SUCCESS:
						 try
						 {
							  filterChain.doFilter( new AuthorizedRequestWrapper( BASIC_AUTH, username, request, securityContext ), servletResponse );
						 }
						 catch ( AuthorizationViolationException e )
						 {
							  UnauthorizedAccess( e.Message ).accept( response );
						 }
						 return;
					case TOO_MANY_ATTEMPTS:
						 _tooManyAttempts.accept( response );
						 return;
					default:
						 _log.warn( "Failed authentication attempt for '%s' from %s", username, request.RemoteAddr );
						 RequestAuthentication( request, _invalidCredential ).accept( response );
					 break;
					}
			  }
			  catch ( InvalidAuthTokenException e )
			  {
					RequestAuthentication( request, InvalidAuthToken( e.Message ) ).accept( response );
			  }
			  catch ( AuthProviderTimeoutException )
			  {
					_authProviderTimeout.accept( response );
			  }
			  catch ( AuthProviderFailedException )
			  {
					_authProviderFailed.accept( response );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.internal.kernel.api.security.LoginContext authenticate(String username, String password) throws org.neo4j.kernel.api.security.exception.InvalidAuthTokenException
		 private LoginContext Authenticate( string username, string password )
		 {
			  AuthManager authManager = _authManagerSupplier.get();
			  IDictionary<string, object> authToken = newBasicAuthToken( username, !string.ReferenceEquals( password, null ) ? UTF8.encode( password ) : null );
			  return authManager.Login( authToken );
		 }

		 private static readonly ThrowingConsumer<HttpServletResponse, IOException> _noHeader = Error( 401, map( "errors", singletonList( map( "code", Neo4Net.Kernel.Api.Exceptions.Status_Security.Unauthorized.code().serialize(), "message", "No authentication header supplied." ) ) ) );

		 private static readonly ThrowingConsumer<HttpServletResponse, IOException> _badHeader = Error( 400, map( "errors", singletonList( map( "code", Neo4Net.Kernel.Api.Exceptions.Status_Request.InvalidFormat.code().serialize(), "message", "Invalid authentication header." ) ) ) );

		 private static readonly ThrowingConsumer<HttpServletResponse, IOException> _invalidCredential = Error( 401, map( "errors", singletonList( map( "code", Neo4Net.Kernel.Api.Exceptions.Status_Security.Unauthorized.code().serialize(), "message", "Invalid username or password." ) ) ) );

		 private static readonly ThrowingConsumer<HttpServletResponse, IOException> _tooManyAttempts = Error( 429, map( "errors", singletonList( map( "code", Neo4Net.Kernel.Api.Exceptions.Status_Security.AuthenticationRateLimit.code().serialize(), "message", "Too many failed authentication requests. Please wait 5 seconds and try again." ) ) ) );

		 private static readonly ThrowingConsumer<HttpServletResponse, IOException> _authProviderFailed = Error( 502, map( "errors", singletonList( map( "code", Neo4Net.Kernel.Api.Exceptions.Status_Security.AuthProviderFailed.code().serialize(), "message", "An auth provider request failed." ) ) ) );

		 private static readonly ThrowingConsumer<HttpServletResponse, IOException> _authProviderTimeout = Error( 504, map( "errors", singletonList( map( "code", Neo4Net.Kernel.Api.Exceptions.Status_Security.AuthProviderTimeout.code().serialize(), "message", "An auth provider request timed out." ) ) ) );

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.neo4j.function.ThrowingConsumer<javax.servlet.http.HttpServletResponse, java.io.IOException> invalidAuthToken(final String message)
		 private static ThrowingConsumer<HttpServletResponse, IOException> InvalidAuthToken( string message )
		 {
			  return Error( 401, map( "errors", singletonList( map( "code", Neo4Net.Kernel.Api.Exceptions.Status_Security.Unauthorized.code().serialize(), "message", message ) ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.neo4j.function.ThrowingConsumer<javax.servlet.http.HttpServletResponse, java.io.IOException> passwordChangeRequired(final String username, final String baseURL)
		 private static ThrowingConsumer<HttpServletResponse, IOException> PasswordChangeRequired( string username, string baseURL )
		 {
			  URI path = UriBuilder.fromUri( baseURL ).path( format( "/user/%s/password", username ) ).build();
			  return Error( 403, map( "errors", singletonList( map( "code", Neo4Net.Kernel.Api.Exceptions.Status_Security.Forbidden.code().serialize(), "message", "User is required to change their password." ) ), "password_change", path.ToString() ) );
		 }

		 /// <summary>
		 /// In order to avoid browsers popping up an auth box when using the Neo4j Browser, it sends us a special header.
		 /// When we get that special header, we send a crippled authentication challenge back that the browser does not
		 /// understand, which lets the Neo4j Browser handle auth on its own.
		 /// 
		 /// Otherwise, we send a regular basic auth challenge. This method adds the appropriate header depending on the
		 /// inbound request.
		 /// </summary>
		 private static ThrowingConsumer<HttpServletResponse, IOException> RequestAuthentication( HttpServletRequest req, ThrowingConsumer<HttpServletResponse, IOException> responseGen )
		 {
			  if ( "true".Equals( req.getHeader( "X-Ajax-Browser-Auth" ) ) )
			  {
					return res =>
					{
					 responseGen.Accept( res );
					 res.addHeader( HttpHeaders.WWW_AUTHENTICATE, "None" );
					};
			  }
			  else
			  {
					return res =>
					{
					 responseGen.Accept( res );
					 res.addHeader( HttpHeaders.WWW_AUTHENTICATE, "Basic realm=\"Neo4j\"" );
					};
			  }
		 }

		 private string BaseURL( HttpServletRequest request )
		 {
			  StringBuilder url = request.RequestURL;
			  string baseURL = url.substring( 0, url.Length - request.RequestURI.length() ) + "/";

			  return XForwardUtil.externalUri( baseURL, request.getHeader( X_FORWARD_HOST_HEADER_KEY ), request.getHeader( X_FORWARD_PROTO_HEADER_KEY ) );
		 }

		 private bool Whitelisted( string path )
		 {
			  foreach ( Pattern pattern in _uriWhitelist )
			  {
					if ( pattern.matcher( path ).matches() )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 private string[] ExtractCredential( string header )
		 {
			  if ( string.ReferenceEquals( header, null ) )
			  {
					return null;
			  }
			  else
			  {
					return AuthorizationHeaders.Decode( header );
			  }
		 }
	}

}