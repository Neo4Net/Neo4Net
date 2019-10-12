using System;

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
namespace Neo4Net.Graphdb.security
{
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;

	/// <summary>
	/// Thrown when required authorization info has expired in the Neo4j auth cache
	/// </summary>
	public class AuthorizationExpiredException : Exception, Neo4Net.Kernel.Api.Exceptions.Status_HasStatus
	{
		 private const Status STATUS_CODE = Neo4Net.Kernel.Api.Exceptions.Status_Security.AuthorizationExpired;

		 public AuthorizationExpiredException( string msg ) : base( msg )
		 {
		 }

		 public AuthorizationExpiredException( string msg, Exception cause ) : base( msg, cause )
		 {
		 }

		 /// <summary>
		 /// The Neo4j status code associated with this exception type. </summary>
		 public override Status Status()
		 {
			  return STATUS_CODE;
		 }
	}

}