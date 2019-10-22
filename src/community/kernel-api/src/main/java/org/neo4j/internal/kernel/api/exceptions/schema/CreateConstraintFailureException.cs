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
namespace Neo4Net.Internal.Kernel.Api.exceptions.schema
{
	using ConstraintDescriptor = Neo4Net.Internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;

	public class CreateConstraintFailureException : SchemaKernelException
	{
		 private readonly ConstraintDescriptor _constraint;

		 private readonly string _cause;
		 public CreateConstraintFailureException( ConstraintDescriptor constraint, Exception cause ) : base( org.Neo4Net.kernel.api.exceptions.Status_Schema.ConstraintCreationFailed, cause, "Unable to create constraint %s: %s", constraint, cause.Message )
		 {
			  this._constraint = constraint;
			  this._cause = null;
		 }

		 public CreateConstraintFailureException( ConstraintDescriptor constraint, string cause ) : base( org.Neo4Net.kernel.api.exceptions.Status_Schema.ConstraintCreationFailed, null, "Unable to create constraint %s: %s", constraint, cause )
		 {
			  this._constraint = constraint;
			  this._cause = cause;
		 }

		 public virtual ConstraintDescriptor Constraint()
		 {
			  return _constraint;
		 }

		 public override string GetUserMessage( TokenNameLookup tokenNameLookup )
		 {
			  string message = "Unable to create " + _constraint.prettyPrint( tokenNameLookup );
			  if ( !string.ReferenceEquals( _cause, null ) )
			  {
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: message = String.format("%s:%n%s", message, cause);
					message = string.Format( "%s:%n%s", message, _cause );
			  }
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