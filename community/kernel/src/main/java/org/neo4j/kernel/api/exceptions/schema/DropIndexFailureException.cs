﻿/*
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
namespace Org.Neo4j.Kernel.Api.Exceptions.schema
{
	using TokenNameLookup = Org.Neo4j.@internal.Kernel.Api.TokenNameLookup;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using SchemaKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.SchemaKernelException;
	using SchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptor;
	using SchemaUtil = Org.Neo4j.@internal.Kernel.Api.schema.SchemaUtil;

	public class DropIndexFailureException : SchemaKernelException
	{
		 private readonly SchemaDescriptor _descriptor;
		 private const string MESSAGE = "Unable to drop index on %s: %s";

		 public DropIndexFailureException( SchemaDescriptor descriptor, SchemaKernelException cause ) : base( org.neo4j.kernel.api.exceptions.Status_Schema.IndexDropFailed, format( MESSAGE, descriptor.UserDescription( SchemaUtil.idTokenNameLookup ), cause.Message ), cause )
		 {
			  this._descriptor = descriptor;
		 }

		 public override string GetUserMessage( TokenNameLookup tokenNameLookup )
		 {
			  return format( MESSAGE, _descriptor.userDescription( tokenNameLookup ), ( ( KernelException ) Cause ).getUserMessage( tokenNameLookup ) );
		 }
	}

}