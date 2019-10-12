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
namespace Org.Neo4j.Kernel.Api.Exceptions.schema
{
	using TokenNameLookup = Org.Neo4j.@internal.Kernel.Api.TokenNameLookup;
	using ConstraintValidationException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.ConstraintValidationException;
	using SchemaUtil = Org.Neo4j.@internal.Kernel.Api.schema.SchemaUtil;
	using ConstraintDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;

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