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
namespace Neo4Net.Values
{
	/// <summary>
	/// Defines the result of a ternary comparison.
	/// <para>
	/// In a ternary comparison the result may not only be greater than, equal or smaller than but the
	/// result can also be undefined.
	/// </para>
	/// </summary>
	public sealed class Comparison
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       GREATER_THAN { public int value() { return 1; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       EQUAL { public int value() { return 0; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       SMALLER_THAN { public int value() { return -1; } },
		 public static readonly Comparison GreaterThanAndEqual = new Comparison( "GreaterThanAndEqual", InnerEnum.GreaterThanAndEqual );
		 public static readonly Comparison SmallerThanAndEqual = new Comparison( "SmallerThanAndEqual", InnerEnum.SmallerThanAndEqual );
		 public static readonly Comparison Undefined = new Comparison( "Undefined", InnerEnum.Undefined );

		 private static readonly IList<Comparison> valueList = new List<Comparison>();

		 static Comparison()
		 {
			 valueList.Add( GREATER_THAN );
			 valueList.Add( EQUAL );
			 valueList.Add( SMALLER_THAN );
			 valueList.Add( GreaterThanAndEqual );
			 valueList.Add( SmallerThanAndEqual );
			 valueList.Add( Undefined );
		 }

		 public enum InnerEnum
		 {
			 GREATER_THAN,
			 EQUAL,
			 SMALLER_THAN,
			 GreaterThanAndEqual,
			 SmallerThanAndEqual,
			 Undefined
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private Comparison( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 /// <summary>
		 /// Integer representation of comparison
		 /// <para>
		 /// Returns a positive integer if <seealso cref="Comparison.GREATER_THAN"/> than, negative integer for
		 /// <seealso cref="Comparison.SMALLER_THAN"/>,
		 /// and zero for <seealso cref="Comparison.EQUAL"/>
		 /// 
		 /// </para>
		 /// </summary>
		 /// <returns> a positive number if result is greater than, a negative number if the result is smaller than or zero
		 /// if equal. </returns>
		 /// <exception cref="IllegalStateException"> if the result is undefined. </exception>
		 public int Value()
		 {
			  throw new System.InvalidOperationException( "This value is undefined and can't handle primitive comparisons" );
		 }

		 /// <summary>
		 /// Maps an integer value to comparison result.
		 /// </summary>
		 /// <param name="i"> the integer to be mapped to a Comparison </param>
		 /// <returns> <seealso cref="Comparison.GREATER_THAN"/> than if positive, <seealso cref="Comparison.SMALLER_THAN"/> if negative or
		 /// <seealso cref="Comparison.EQUAL"/> if zero </returns>
		 public static Comparison From( int i )
		 {
			  if ( i > 0 )
			  {
					return GREATER_THAN;
			  }
			  else if ( i < 0 )
			  {
					return SMALLER_THAN;
			  }
			  else
			  {
					return EQUAL;
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