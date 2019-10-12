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
namespace Org.Neo4j.Server.rest.dbms
{

	using AuthorizationViolationException = Org.Neo4j.Graphdb.security.AuthorizationViolationException;
	using LoginContext = Org.Neo4j.@internal.Kernel.Api.security.LoginContext;
	using JettyHttpConnection = Org.Neo4j.Server.web.JettyHttpConnection;

	public class AuthorizationDisabledFilter : AuthorizationFilter
	{
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

			  try
			  {
					LoginContext loginContext = AuthDisabledLoginContext;
					string userAgent = request.getHeader( HttpHeaders.USER_AGENT );

					JettyHttpConnection.updateUserForCurrentConnection( loginContext.Subject().username(), userAgent );

					filterChain.doFilter( new AuthorizedRequestWrapper( BASIC_AUTH, "neo4j", request, loginContext ), servletResponse );
			  }
			  catch ( AuthorizationViolationException e )
			  {
					UnauthorizedAccess( e.Message ).accept( response );
			  }
		 }

		 protected internal virtual LoginContext AuthDisabledLoginContext
		 {
			 get
			 {
				  return LoginContext.AUTH_DISABLED;
			 }
		 }
	}

}