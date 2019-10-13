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
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;

	public abstract class AuthorizationFilter : Filter
	{
		 protected internal static ThrowingConsumer<HttpServletResponse, IOException> Error( int code, object body )
		 {
			  return response =>
			  {
				response.Status = code;
				response.addHeader( HttpHeaders.CONTENT_TYPE, "application/json; charset=UTF-8" );
				response.OutputStream.write( JsonHelper.createJsonFrom( body ).getBytes( StandardCharsets.UTF_8 ) );
			  };
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected static org.neo4j.function.ThrowingConsumer<javax.servlet.http.HttpServletResponse, java.io.IOException> unauthorizedAccess(final String message)
		 protected internal static ThrowingConsumer<HttpServletResponse, IOException> UnauthorizedAccess( string message )
		 {
			  return Error( 403, map( "errors", singletonList( map( "code", Neo4Net.Kernel.Api.Exceptions.Status_Security.Forbidden.code().serialize(), "message", string.Format("Unauthorized access violation: {0}.", message) ) ) ) );
		 }

		 public override void Init( FilterConfig filterConfig )
		 {
		 }

		 public override void Destroy()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void validateRequestType(javax.servlet.ServletRequest request) throws javax.servlet.ServletException
		 protected internal virtual void ValidateRequestType( ServletRequest request )
		 {
			  if ( !( request is HttpServletRequest ) )
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getCanonicalName method:
					throw new ServletException( format( "Expected HttpServletRequest, received [%s]", request.GetType().FullName ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void validateResponseType(javax.servlet.ServletResponse response) throws javax.servlet.ServletException
		 protected internal virtual void ValidateResponseType( ServletResponse response )
		 {
			  if ( !( response is HttpServletResponse ) )
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getCanonicalName method:
					throw new ServletException( format( "Expected HttpServletResponse, received [%s]", response.GetType().FullName ) );
			  }
		 }
	}

}