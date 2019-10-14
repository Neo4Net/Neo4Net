using System;
using System.Collections.Generic;

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
namespace Neo4Net.@internal.Kernel.Api.exceptions.schema
{
	using SchemaUtil = Neo4Net.@internal.Kernel.Api.schema.SchemaUtil;
	using ConstraintDescriptor = Neo4Net.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;

	/// <summary>
	/// Constraint verification happens when a new constraint is created, and the database verifies that existing
	/// data adheres to the new constraint.
	/// </summary>
	public abstract class ConstraintValidationException : KernelException
	{
		 /// <summary>
		 /// Constraint validation failures can happen during one of two phases of the constraint lifecycle.
		 /// 
		 /// VERIFICATION is the process to control that a constraint holds with respect to all the data in the graph. This
		 /// happens before creating a constraint for example, and if the verification fails the constraint is not created.
		 /// Verification can also occur during batch import for example.
		 /// 
		 /// VALIDATION is what happens when the graph is modified, and the resulting state is controlled against a
		 /// constraint to see that the modified state does not violate the constraint. If validation fails the modifying
		 /// transaction is rolled back.
		 /// </summary>
		 public sealed class Phase
		 {
			  public static readonly Phase Verification = new Phase( "Verification", InnerEnum.Verification, Neo4Net.Kernel.Api.Exceptions.Status_Statement.ConstraintVerificationFailed );
			  public static readonly Phase Validation = new Phase( "Validation", InnerEnum.Validation, Neo4Net.Kernel.Api.Exceptions.Status_Schema.ConstraintValidationFailed );

			  private static readonly IList<Phase> valueList = new List<Phase>();

			  static Phase()
			  {
				  valueList.Add( Verification );
				  valueList.Add( Validation );
			  }

			  public enum InnerEnum
			  {
				  Verification,
				  Validation
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  internal Private readonly;

			  internal Phase( string name, InnerEnum innerEnum, Neo4Net.Kernel.Api.Exceptions.Status status )
			  {
					this._status = status;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  public Neo4Net.Kernel.Api.Exceptions.Status Status
			  {
				  get
				  {
						return _status;
				  }
			  }

			 public static IList<Phase> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static Phase valueOf( string name )
			 {
				 foreach ( Phase enumInstance in Phase.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly ConstraintDescriptor ConstraintConflict;

		 protected internal ConstraintValidationException( ConstraintDescriptor constraint, Phase phase, string subject ) : base( phase.Status, "%s does not satisfy %s.", subject, constraint.PrettyPrint( SchemaUtil.idTokenNameLookup ) )
		 {
			  this.ConstraintConflict = constraint;
		 }

		 protected internal ConstraintValidationException( ConstraintDescriptor constraint, Phase phase, string subject, Exception failure ) : base( phase.Status, failure, "%s does not satisfy %s: %s", subject, constraint.PrettyPrint( SchemaUtil.idTokenNameLookup ), failure.Message )
		 {
			  this.ConstraintConflict = constraint;
		 }

		 public override abstract string GetUserMessage( TokenNameLookup tokenNameLookup );

		 public virtual ConstraintDescriptor Constraint()
		 {
			  return ConstraintConflict;
		 }
	}

}