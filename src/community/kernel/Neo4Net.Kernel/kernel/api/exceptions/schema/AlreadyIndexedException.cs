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

	public class AlreadyIndexedException : SchemaKernelException
	{
		 private const string NO_CONTEXT_FORMAT = "Already indexed %s.";

		 private const string INDEX_CONTEXT_FORMAT = "There already exists an index %s.";
		 private static readonly string _constraintContextFormat = "There already exists an index %s. " +
																					"A constraint cannot be created until the index has been dropped.";

		 private readonly SchemaDescriptor _descriptor;
		 private readonly OperationContext _context;

		 public AlreadyIndexedException( SchemaDescriptor descriptor, OperationContext context ) : base( Neo4Net.kernel.api.exceptions.Status_Schema.IndexAlreadyExists, ConstructUserMessage( context, SchemaUtil.idTokenNameLookup, descriptor ) )
		 {

			  this._descriptor = descriptor;
			  this._context = context;
		 }

		 private static string ConstructUserMessage( OperationContext context, TokenNameLookup tokenNameLookup, SchemaDescriptor descriptor )
		 {
			  switch ( context )
			  {
					case SchemaKernelException.OperationContext.INDEX_CREATION:
						 return MessageWithLabelAndPropertyName( tokenNameLookup, INDEX_CONTEXT_FORMAT, descriptor );
					case SchemaKernelException.OperationContext.CONSTRAINT_CREATION:
						 return MessageWithLabelAndPropertyName( tokenNameLookup, _constraintContextFormat, descriptor );
					default:
						 return string.format( NO_CONTEXT_FORMAT, descriptor );
			  }
		 }

		 public override string GetUserMessage( TokenNameLookup tokenNameLookup )
		 {
			  return ConstructUserMessage( _context, tokenNameLookup, _descriptor );
		 }
	}

}