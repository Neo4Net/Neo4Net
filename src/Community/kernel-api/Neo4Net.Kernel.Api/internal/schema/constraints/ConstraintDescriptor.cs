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
namespace Neo4Net.Kernel.Api.Internal.Schema.constraints
{

	public interface IConstraintDescriptor : SchemaDescriptorSupplier
	{

		 SchemaDescriptor Schema();

		 ConstraintDescriptor_Type Type();

		 bool EnforcesUniqueness();

		 bool EnforcesPropertyExistence();

		 string UserDescription( ITokenNameLookup tokenNameLookup );

		 /// <summary>
		 /// Checks whether a constraint descriptor Supplier supplies this constraint descriptor. </summary>
		 /// <param name="supplier"> supplier to get a constraint descriptor from </param>
		 /// <returns> true if the supplied constraint descriptor equals this constraint descriptor </returns>
		 bool IsSame( ConstraintDescriptor_Supplier supplier );

		 string PrettyPrint( ITokenNameLookup tokenNameLookup );
	}

	 public sealed class ConstraintDescriptor_Type
	 {
		  public static readonly ConstraintDescriptor_Type Unique = new ConstraintDescriptor_Type( "Unique", InnerEnum.Unique, true, false );
		  public static readonly ConstraintDescriptor_Type Exists = new ConstraintDescriptor_Type( "Exists", InnerEnum.Exists, false, true );
		  public static readonly ConstraintDescriptor_Type UniqueExists = new ConstraintDescriptor_Type( "UniqueExists", InnerEnum.UniqueExists, true, true );

		  private static readonly IList<ConstraintDescriptor_Type> valueList = new List<ConstraintDescriptor_Type>();

		  static ConstraintDescriptor_Type()
		  {
			  valueList.Add( Unique );
			  valueList.Add( Exists );
			  valueList.Add( UniqueExists );
		  }

		  public enum InnerEnum
		  {
			  Unique,
			  Exists,
			  UniqueExists
		  }

		  public readonly InnerEnum innerEnumValue;
		  private readonly string nameValue;
		  private readonly int ordinalValue;
		  private static int nextOrdinal = 0;

		  internal Final boolean;
		  internal Final boolean;

		  internal ConstraintDescriptor_Type( string name, InnerEnum innerEnum, bool isUnique, bool mustExist )
		  {
				this.IsUnique = isUnique;
				this.MustExist = mustExist;

			  nameValue = name;
			  ordinalValue = nextOrdinal++;
			  innerEnumValue = innerEnum;
		  }

		  public bool EnforcesUniqueness()
		  {
				return IsUnique;
		  }

		  public bool EnforcesPropertyExistence()
		  {
				return MustExist;
		  }

		 public static IList<ConstraintDescriptor_Type> values()
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

		 public static ConstraintDescriptor_Type ValueOf( string name )
		 {
			 foreach ( ConstraintDescriptor_Type enumInstance in ConstraintDescriptor_Type.valueList )
			 {
				 if ( enumInstance.nameValue == name )
				 {
					 return enumInstance;
				 }
			 }
			 throw new System.ArgumentException( name );
		 }
	 }

	 public interface IConstraintDescriptor_Supplier
	 {
		  ConstraintDescriptor ConstraintDescriptor { get; }
	 }

}