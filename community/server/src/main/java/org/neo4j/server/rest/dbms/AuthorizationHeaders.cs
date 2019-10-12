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
	using Base64 = com.sun.jersey.core.util.Base64;

	public class AuthorizationHeaders
	{
		 private AuthorizationHeaders()
		 {
		 }

		 /// <summary>
		 /// Extract the encoded username and password from a HTTP Authorization header value.
		 /// </summary>
		 public static string[] Decode( string authorizationHeader )
		 {
			  string[] parts = authorizationHeader.Trim().Split(" ", true);
			  string tokenSegment = parts[parts.Length - 1];

			  if ( tokenSegment.Trim().Length == 0 )
			  {
					return null;
			  }

			  string decoded = Base64.base64Decode( tokenSegment );
			  if ( decoded.Length < 1 )
			  {
					return null;
			  }

			  string[] userAndPassword = decoded.Split( ":", 2 );
			  if ( userAndPassword.Length != 2 )
			  {
					return null;
			  }

			  return userAndPassword;
		 }
	}

}