using System.Collections.Generic;

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
namespace Org.Neo4j.Values.Storable
{
	public sealed class Comparison
	{
		 public static readonly Comparison LhsSmallerThanRhs = new Comparison( "LhsSmallerThanRhs", InnerEnum.LhsSmallerThanRhs, 1 );
		 public static readonly Comparison LhsEqualToRhs = new Comparison( "LhsEqualToRhs", InnerEnum.LhsEqualToRhs, 0 );
		 public static readonly Comparison LhsGreaterThanRhs = new Comparison( "LhsGreaterThanRhs", InnerEnum.LhsGreaterThanRhs, -1 );

		 private static readonly IList<Comparison> valueList = new List<Comparison>();

		 static Comparison()
		 {
			 valueList.Add( LhsSmallerThanRhs );
			 valueList.Add( LhsEqualToRhs );
			 valueList.Add( LhsGreaterThanRhs );
		 }

		 public enum InnerEnum
		 {
			 LhsSmallerThanRhs,
			 LhsEqualToRhs,
			 LhsGreaterThanRhs
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;
		 internal Private readonly;

		 internal Comparison( string name, InnerEnum innerEnum, int cmp )
		 {
			  this._cmp = cmp;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static Comparison ComparisonConflict( long cmp )
		 {
			  if ( cmp == 0 )
			  {
					return LHS_EQUAL_TO_RHS;
			  }
			  if ( cmp < 0 )
			  {
					return LHS_SMALLER_THAN_RHS;
			  }
			  else
			  {
					return LHS_GREATER_THAN_RHS;
			  }
		 }

		public static IList<Comparison> values()
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

		public static Comparison valueOf( string name )
		{
			foreach ( Comparison enumInstance in Comparison.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}