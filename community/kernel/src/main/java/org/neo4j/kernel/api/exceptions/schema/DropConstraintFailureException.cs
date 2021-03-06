﻿using System;

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
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using SchemaKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.SchemaKernelException;
	using ConstraintDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;

	public class DropConstraintFailureException : SchemaKernelException
	{
		 private readonly ConstraintDescriptor _constraint;

		 public DropConstraintFailureException( ConstraintDescriptor constraint, Exception cause ) : base( org.neo4j.kernel.api.exceptions.Status_Schema.ConstraintDropFailed, cause, "Unable to drop constraint %s: %s", constraint, cause.Message )
		 {
			  this._constraint = constraint;
		 }

		 public virtual ConstraintDescriptor Constraint()
		 {
			  return _constraint;
		 }

		 public override string GetUserMessage( TokenNameLookup tokenNameLookup )
		 {
			  string message = "Unable to drop " + _constraint.userDescription( tokenNameLookup );
			  if ( Cause is KernelException )
			  {
					KernelException cause = ( KernelException ) Cause;

//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: return String.format("%s:%n%s", message, cause.getUserMessage(tokenNameLookup));
					return string.Format( "%s:%n%s", message, cause.GetUserMessage( tokenNameLookup ) );
			  }
			  return message;
		 }
	}

}