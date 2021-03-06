﻿using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Org.Neo4j.Server.security.enterprise.auth
{

	using Org.Neo4j.Server.Security.Auth;
	using FormatException = Org.Neo4j.Server.Security.Auth.exception.FormatException;

	/// <summary>
	/// Serializes role authorization and authentication data to a format similar to unix passwd files.
	/// </summary>
	public class RoleSerialization : FileRepositorySerializer<RoleRecord>
	{
		 private const string ROLE_SEPARATOR = ":";
		 private const string USER_SEPARATOR = ",";

		 protected internal override string Serialize( RoleRecord role )
		 {
			  return string.join( ROLE_SEPARATOR, role.Name(), string.join(USER_SEPARATOR, role.Users()) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected RoleRecord deserializeRecord(String line, int lineNumber) throws org.neo4j.server.security.auth.exception.FormatException
		 protected internal override RoleRecord DeserializeRecord( string line, int lineNumber )
		 {
			  string[] parts = line.Split( ROLE_SEPARATOR, false );
			  if ( parts.Length != 2 )
			  {
					throw new FormatException( format( "wrong number of line fields [line %d]", lineNumber ) );
			  }
			  return ( new RoleRecord.Builder() ).WithName(parts[0]).withUsers(DeserializeUsers(parts[1])).build();
		 }

		 private SortedSet<string> DeserializeUsers( string part )
		 {
			  string[] splits = part.Split( USER_SEPARATOR, false );

			  SortedSet<string> users = new SortedSet<string>();

			  foreach ( string user in splits )
			  {
					if ( user.Trim().Length > 0 )
					{
						 users.Add( user );
					}
			  }

			  return users;
		 }
	}

}