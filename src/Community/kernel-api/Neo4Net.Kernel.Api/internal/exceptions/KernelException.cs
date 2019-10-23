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
namespace Neo4Net.Kernel.Api.Internal.Exceptions
{
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;

	/// <summary>
	/// A super class of checked exceptions. </summary>
	public abstract class KernelException : Exception, Neo4Net.Kernel.Api.Exceptions.Status_HasStatus
	{
		 private readonly Status _statusCode;

		 protected internal KernelException( Status statusCode, Exception cause, string message, params object[] parameters ) : base( ( parameters.Length > 0 ) ? string.format( message, parameters ) : message )
		 {
			  this._statusCode = statusCode;
			  initCause( cause );
		 }

		 protected internal KernelException( Status statusCode, Exception cause ) : base( cause )
		 {
			  this._statusCode = statusCode;
		 }

		 protected internal KernelException( Status statusCode, string message, params object[] parameters ) : base( string.format( message, parameters ) )
		 {
			  this._statusCode = statusCode;
		 }

		 /// <summary>
		 /// The Neo4Net status code associated with this exception type. </summary>
		 public override Status Status()
		 {
			  return _statusCode;
		 }

		 public virtual string GetUserMessage( ITokenNameLookup tokenNameLookup )
		 {
			  return Message;
		 }
	}

}