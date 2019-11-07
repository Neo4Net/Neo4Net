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
	using HttpContext = com.sun.jersey.api.core.HttpContext;
	using HttpRequestContext = com.sun.jersey.api.core.HttpRequestContext;


	using LoginContext = Neo4Net.Kernel.Api.Internal.security.LoginContext;
	using AnonymousContext = Neo4Net.Kernel.Api.security.AnonymousContext;

	public class AuthorizedRequestWrapper : HttpServletRequestWrapper
	{
		 public static LoginContext GetLoginContextFromHttpServletRequest( HttpServletRequest request )
		 {
			  Principal principal = request.UserPrincipal;
			  return GetLoginContextFromUserPrincipal( principal );
		 }

		 public static LoginContext GetLoginContextFromHttpContext( HttpContext httpContext )
		 {
			  HttpRequestContext requestContext = httpContext.Request;
			  Principal principal = requestContext.UserPrincipal;
			  return GetLoginContextFromUserPrincipal( principal );
		 }

		 public static LoginContext GetLoginContextFromUserPrincipal( Principal principal )
		 {
			  if ( principal is DelegatingPrincipal )
			  {
					return ( ( DelegatingPrincipal ) principal ).LoginContext;
			  }
			  // If whitelisted uris can start transactions we cannot throw exception here
			  //throw new IllegalArgumentException( "Tried to get access mode on illegal user principal" );
			  return AnonymousContext.none();
		 }

		 private readonly string _authType;
		 private readonly DelegatingPrincipal _principal;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public AuthorizedRequestWrapper(final String authType, final String username, final javax.servlet.http.HttpServletRequest request, Neo4Net.Kernel.Api.Internal.security.LoginContext loginContext)
		 public AuthorizedRequestWrapper( string authType, string username, HttpServletRequest request, LoginContext loginContext ) : base( request )
		 {
			  this._authType = authType;
			  this._principal = new DelegatingPrincipal( username, loginContext );
		 }

		 public override string AuthType
		 {
			 get
			 {
				  return _authType;
			 }
		 }

		 public override Principal UserPrincipal
		 {
			 get
			 {
				  return _principal;
			 }
		 }

		 public override bool IsUserInRole( string role )
		 {
			  return true;
		 }

		 public override string ToString()
		 {
			  return "AuthorizedRequestWrapper{" +
						"authType='" + _authType + '\'' +
						", principal=" + _principal +
						'}';
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  AuthorizedRequestWrapper that = ( AuthorizedRequestWrapper ) o;
			  if ( !_authType.Equals( that._authType ) )
			  {
					return false;
			  }
			  return _principal.Equals( that._principal );
		 }

		 public override int GetHashCode()
		 {
			  int result = _authType.GetHashCode();
			  result = 31 * result + _principal.GetHashCode();
			  return result;
		 }
	}

}