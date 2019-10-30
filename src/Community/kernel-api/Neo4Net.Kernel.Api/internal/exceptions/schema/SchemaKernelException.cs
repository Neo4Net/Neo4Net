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
namespace Neo4Net.Kernel.Api.Internal.Exceptions.Schema
{
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;

	/// <summary>
	/// Signals that some constraint has been violated, for example a name containing invalid characters or length.
	/// </summary>
	public abstract class SchemaKernelException : KernelException
	{
		 public enum OperationContext
		 {
			  IndexCreation,
			  ConstraintCreation
		 }

		 protected internal SchemaKernelException( Status statusCode, Exception cause, string message, params object[] parameters ) : base( statusCode, cause, message, parameters )
		 {
		 }

		 public SchemaKernelException( Status statusCode, string message, Exception cause ) : base( statusCode, cause, message )
		 {
		 }

		 public SchemaKernelException( Status statusCode, string message ) : base( statusCode, message )
		 {
		 }

		 protected internal static string MessageWithLabelAndPropertyName( ITokenNameLookup tokenNameLookup, string formatString, SchemaDescriptor descriptor )
		 {
			  return string.format( formatString, descriptor.UserDescription( tokenNameLookup ) );
		 }
	}

}