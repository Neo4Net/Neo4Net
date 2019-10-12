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
namespace Org.Neo4j.@internal.Kernel.Api.exceptions.schema
{
	using IndexProviderDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;

	/// <summary>
	/// Thrown when an index cannot be created with a particular configuration, or cannot be support by a given provider.
	/// </summary>
	public class MisconfiguredIndexException : SchemaKernelException
	{
		 protected internal MisconfiguredIndexException( Status statusCode, Exception cause, string message, params object[] parameters ) : base( statusCode, cause, message, parameters )
		 {
		 }

		 public MisconfiguredIndexException( Status statusCode, string message, Exception cause ) : base( statusCode, message, cause )
		 {
		 }

		 public MisconfiguredIndexException( Status statusCode, string message ) : base( statusCode, message )
		 {
		 }
	}

}