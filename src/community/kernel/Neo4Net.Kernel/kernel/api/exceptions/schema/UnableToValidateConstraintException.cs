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
namespace Neo4Net.Kernel.Api.Exceptions.schema
{
	using TokenNameLookup = Neo4Net.Kernel.Api.Internal.TokenNameLookup;
	using ConstraintValidationException = Neo4Net.Kernel.Api.Internal.Exceptions.schema.ConstraintValidationException;
	using SchemaUtil = Neo4Net.Kernel.Api.Internal.schema.SchemaUtil;
	using ConstraintDescriptor = Neo4Net.Kernel.Api.Internal.schema.constraints.ConstraintDescriptor;

	/// <summary>
	/// Attempting to validate constraints but the apparatus for validation was not available. For example,
	/// this exception is thrown when an index required to implement a uniqueness constraint is not available.
	/// </summary>
	public class UnableToValidateConstraintException : ConstraintValidationException
	{
		 public UnableToValidateConstraintException( ConstraintDescriptor constraint, Exception cause ) : base( constraint, Phase.Verification, format( "Unable to validate constraint %s", constraint.UserDescription( SchemaUtil.idTokenNameLookup ) ), cause )
		 {
		 }

		 public override string GetUserMessage( TokenNameLookup tokenNameLookup )
		 {
			  return format( "Unable to validate constraint %s", ConstraintConflict.userDescription( tokenNameLookup ) );
		 }
	}

}