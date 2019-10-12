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
	/// Thrown when the database is asked to perform an action that is not authorized based on the AccessMode settings.
	/// 
	/// For instance, if attempting to write with READ_ONLY rights.
	/// </summary>
	public class AuthorizationViolationException : Exception, Neo4Net.Kernel.Api.Exceptions.Status_HasStatus
	{
		 public const string PERMISSION_DENIED = "Permission denied.";

		 private Status _statusCode = Neo4Net.Kernel.Api.Exceptions.Status_Security.Forbidden;

		 public AuthorizationViolationException( string msg, Status statusCode ) : base( msg )
		 {
			  this._statusCode = statusCode;
		 }

		 public AuthorizationViolationException( string msg ) : base( msg )
		 {
		 }

		 public AuthorizationViolationException( string msg, Exception cause ) : base( msg, cause )
		 {
		 }

		 /// <summary>
		 /// The Neo4j status code associated with this exception type. </summary>
		 public override Status Status()
		 {
			  return _statusCode;
		 }
	}

}