﻿using System;

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
namespace Neo4Net.Graphdb.security
{
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;

	/// <summary>
	/// Thrown when a request for authentication or authorization against an external server failed.
	/// 
	/// <para>This is not used for expected failures like wrong principal, credentials or authorization information,
	/// but for failures where such information could not be established because of an external server problem,
	/// misconfiguration etc.
	/// </para>
	/// </summary>
	public class AuthProviderFailedException : Exception, Neo4Net.Kernel.Api.Exceptions.Status_HasStatus
	{
		 private const Status STATUS_CODE = Neo4Net.Kernel.Api.Exceptions.Status_Security.AuthProviderFailed;

		 public AuthProviderFailedException( string msg ) : base( msg )
		 {
		 }

		 public AuthProviderFailedException( string msg, Exception cause ) : base( msg, cause )
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