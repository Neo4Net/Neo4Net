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
namespace Neo4Net.Storageengine.Api.@lock
{
	using KernelException = Neo4Net.@internal.Kernel.Api.exceptions.KernelException;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;

	/// <summary>
	/// Acquiring a lock failed. This is a runtime exception now to ease the transition from the old lock interface, but
	/// it should be made into a <seealso cref="KernelException"/> asap.
	/// </summary>
	public class AcquireLockTimeoutException : Exception, Neo4Net.Kernel.Api.Exceptions.Status_HasStatus
	{
		 private readonly Status _statusCode;

		 public AcquireLockTimeoutException( Exception cause, string message, Status statusCode ) : base( message, cause )
		 {
			  this._statusCode = statusCode;
		 }

		 public AcquireLockTimeoutException( string message, Status statusCode ) : base( message )
		 {
			  this._statusCode = statusCode;
		 }

		 public override Status Status()
		 {
			  return _statusCode;
		 }
	}

}