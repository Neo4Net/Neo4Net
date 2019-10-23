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
	using SchemaKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.schema.SchemaKernelException;
	using ConstraintDescriptor = Neo4Net.Kernel.Api.Internal.schema.constraints.ConstraintDescriptor;

	public class AlreadyConstrainedException : SchemaKernelException
	{
		 private const string NO_CONTEXT_FORMAT = "Already constrained %s.";

		 private const string ALREADY_CONSTRAINED_MESSAGE_PREFIX = "Constraint already exists: ";

		 private static readonly string _indexContextFormat = "There is a uniqueness constraint on %s, so an index is " +
																			 "already created that matches this.";

		 private readonly ConstraintDescriptor _constraint;
		 private readonly OperationContext _context;

		 public AlreadyConstrainedException( ConstraintDescriptor constraint, OperationContext context, TokenNameLookup tokenNameLookup ) : base( org.Neo4Net.kernel.api.exceptions.Status_Schema.ConstraintAlreadyExists, ConstructUserMessage( context, tokenNameLookup, constraint ) )
		 {
			  this._constraint = constraint;
			  this._context = context;
		 }

		 public virtual ConstraintDescriptor Constraint()
		 {
			  return _constraint;
		 }

		 private static string ConstructUserMessage( OperationContext context, TokenNameLookup tokenNameLookup, ConstraintDescriptor constraint )
		 {
			  switch ( context )
			  {
					case SchemaKernelException.OperationContext.INDEX_CREATION:
						 return MessageWithLabelAndPropertyName( tokenNameLookup, _indexContextFormat, constraint.Schema() );

					case SchemaKernelException.OperationContext.CONSTRAINT_CREATION:
						 return ALREADY_CONSTRAINED_MESSAGE_PREFIX + constraint.PrettyPrint( tokenNameLookup );

					default:
						 return format( NO_CONTEXT_FORMAT, constraint );
			  }
		 }

		 public override string GetUserMessage( TokenNameLookup tokenNameLookup )
		 {
			  return ConstructUserMessage( _context, tokenNameLookup, _constraint );
		 }
	}

}