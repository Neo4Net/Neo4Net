using System;

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
namespace Neo4Net.Kernel.Api.security.exception
{
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;

	public class InvalidAuthTokenException : Exception, Neo4Net.Kernel.Api.Exceptions.Status_HasStatus
	{
		 private readonly Status _status;

		 public InvalidAuthTokenException( string message ) : base( message )
		 {
			  this._status = Neo4Net.Kernel.Api.Exceptions.Status_Security.Unauthorized;
		 }

		 public override Status Status()
		 {
			  return _status;
		 }
	}

}