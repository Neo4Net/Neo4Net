using System.Collections.Generic;

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
namespace Neo4Net.Server.rest.web
{

	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using HttpMethod = Neo4Net.Server.web.HttpMethod;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.web.HttpHeaderUtils.isValidHttpHeaderName;

	/// <summary>
	/// This filter adds the header "Access-Control-Allow-Origin : *" to all
	/// responses that goes through it. This allows modern browsers to do cross-site
	/// requests to us via javascript.
	/// </summary>
	public class CorsFilter : Filter
	{
		 public const string ACCESS_CONTROL_ALLOW_ORIGIN = "Access-Control-Allow-Origin";
		 public const string ACCESS_CONTROL_ALLOW_METHODS = "Access-Control-Allow-Methods";
		 public const string ACCESS_CONTROL_ALLOW_HEADERS = "Access-Control-Allow-Headers";
		 public const string ACCESS_CONTROL_REQUEST_METHOD = "Access-Control-Request-Method";
		 public const string ACCESS_CONTROL_REQUEST_HEADERS = "Access-Control-Request-Headers";
		 public const string VARY = "Vary";

		 private readonly Log _log;
		 private readonly string _accessControlAllowOrigin;
		 private readonly string _vary;

		 public CorsFilter( LogProvider logProvider, string accessControlAllowOrigin )
		 {
			  this._log = logProvider.getLog( this.GetType() );
			  this._accessControlAllowOrigin = accessControlAllowOrigin;
			  if ( "*".Equals( accessControlAllowOrigin ) )
			  {
					_vary = null;
			  }
			  else
			  {
					// If the server specifies an origin host rather than "*", then it must also include Origin in
					// the Vary response header to indicate to clients that server responses will differ based on
					// the value of the Origin request header.
					//
					// -- https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Access-Control-Allow-Origin
					//
					_vary = "Origin";
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void init(javax.servlet.FilterConfig filterConfig) throws javax.servlet.ServletException
		 public override void Init( FilterConfig filterConfig )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void doFilter(javax.servlet.ServletRequest servletRequest, javax.servlet.ServletResponse servletResponse, javax.servlet.FilterChain chain) throws java.io.IOException, javax.servlet.ServletException
		 public override void DoFilter( ServletRequest servletRequest, ServletResponse servletResponse, FilterChain chain )
		 {
			  HttpServletRequest request = ( HttpServletRequest ) servletRequest;
			  HttpServletResponse response = ( HttpServletResponse ) servletResponse;

			  response.setHeader( ACCESS_CONTROL_ALLOW_ORIGIN, _accessControlAllowOrigin );
			  if ( !string.ReferenceEquals( _vary, null ) )
			  {
					response.setHeader( VARY, _vary );
			  }

			  IEnumerator<string> requestMethodEnumeration = request.getHeaders( ACCESS_CONTROL_REQUEST_METHOD );
			  if ( requestMethodEnumeration != null )
			  {
					while ( requestMethodEnumeration.MoveNext() )
					{
						 string requestMethod = requestMethodEnumeration.Current;
						 AddAllowedMethodIfValid( requestMethod, response );
					}
			  }

			  IEnumerator<string> requestHeaderEnumeration = request.getHeaders( ACCESS_CONTROL_REQUEST_HEADERS );
			  if ( requestHeaderEnumeration != null )
			  {
					while ( requestHeaderEnumeration.MoveNext() )
					{
						 string requestHeader = requestHeaderEnumeration.Current;
						 AddAllowedHeaderIfValid( requestHeader, response );
					}
			  }

			  chain.doFilter( request, response );
		 }

		 public override void Destroy()
		 {
		 }

		 private void AddAllowedMethodIfValid( string methodName, HttpServletResponse response )
		 {
			  HttpMethod method = HttpMethod.valueOfOrNull( methodName );
			  if ( method != null )
			  {
					response.addHeader( ACCESS_CONTROL_ALLOW_METHODS, methodName );
			  }
			  else
			  {
					_log.warn( "Unknown HTTP method specified in " + ACCESS_CONTROL_REQUEST_METHOD + " '" + methodName + "'. " + "It will be ignored and not attached to the " + ACCESS_CONTROL_ALLOW_METHODS + " response header" );
			  }
		 }

		 private void AddAllowedHeaderIfValid( string headerName, HttpServletResponse response )
		 {
			  if ( isValidHttpHeaderName( headerName ) )
			  {
					response.addHeader( ACCESS_CONTROL_ALLOW_HEADERS, headerName );
			  }
			  else
			  {
					_log.warn( "Invalid HTTP header specified in " + ACCESS_CONTROL_REQUEST_HEADERS + " '" + headerName + "'. " + "It will be ignored and not attached to the " + ACCESS_CONTROL_ALLOW_HEADERS + " response header" );
			  }
		 }
	}

}