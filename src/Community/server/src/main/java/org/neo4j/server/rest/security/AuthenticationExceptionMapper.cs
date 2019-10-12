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

	/// <summary>
	/// <para>
	/// Map an authentication exception to an HTTP 401 response, optionally including
	/// the realm for a credentials challenge at the client.
	/// </para>
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Provider public class AuthenticationExceptionMapper implements javax.ws.rs.ext.ExceptionMapper<AuthenticationException>
	public class AuthenticationExceptionMapper : ExceptionMapper<AuthenticationException>
	{

		 public override Response ToResponse( AuthenticationException e )
		 {
			  if ( !string.ReferenceEquals( e.Realm, null ) )
			  {
					return Response.status( Response.Status.UNAUTHORIZED ).header( "WWW-Authenticate", "Basic realm=\"" + e.Realm + "\"" ).type( "text/plain" ).entity( e.Message ).build();
			  }
			  else
			  {
					return Response.status( Response.Status.UNAUTHORIZED ).type( "text/plain" ).entity( e.Message ).build();
			  }
		 }

	}

}