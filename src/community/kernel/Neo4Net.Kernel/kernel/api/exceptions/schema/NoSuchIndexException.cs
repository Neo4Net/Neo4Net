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
namespace Neo4Net.Kernel.Api.Exceptions.schema
{
	using TokenNameLookup = Neo4Net.Kernel.Api.Internal.TokenNameLookup;
	using SchemaKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.SchemaKernelException;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor;
	using SchemaUtil = Neo4Net.Kernel.Api.Internal.Schema.SchemaUtil;

	public class NoSuchIndexException : SchemaKernelException
	{
		 private readonly SchemaDescriptor _descriptor;
		 private const string MESSAGE = "No such INDEX ON %s.";

		 public NoSuchIndexException( SchemaDescriptor descriptor ) : base( Neo4Net.kernel.api.exceptions.Status_Schema.IndexNotFound, format( MESSAGE, descriptor.UserDescription( SchemaUtil.idTokenNameLookup ) ) )
		 {
			  this._descriptor = descriptor;
		 }

		 public override string GetUserMessage( TokenNameLookup tokenNameLookup )
		 {
			  return format( MESSAGE, _descriptor.userDescription( tokenNameLookup ) );
		 }
	}

}