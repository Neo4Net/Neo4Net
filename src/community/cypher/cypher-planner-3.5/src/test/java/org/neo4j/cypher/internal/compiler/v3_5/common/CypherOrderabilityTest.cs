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
namespace Neo4Net.Cypher.Internal.compiler.v3_5.common
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using IncomparableValuesException = Neo4Net.Cypher.Internal.v3_5.util.IncomparableValuesException;
	using ValueUtils = Neo4Net.Kernel.impl.util.ValueUtils;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

	/// <summary>
	/// Inspired by <seealso cref="Neo4Net.kernel.impl.api.PropertyValueComparisonTest"/>
	/// </summary>
	public class CypherOrderabilityTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException thrown = org.junit.rules.ExpectedException.none();
		 public ExpectedException Thrown = ExpectedException.none();

		 public static object[] Values = new object[]
		 {
			 new Dictionary<long, long>(),
			 VirtualValues.node( 1 ),
			 VirtualValues.node( 2 ),
			 VirtualValues.relationship( 1 ),
			 VirtualValues.relationship( 2 ),
			 new string[]{ "boo" },
			 new string[]{ "foo" },
			 new bool[]{ false },
			 new bool?[]{ true },
			 new object[]{ 1, "foo" },
			 new object[]{ 1, "foo", 3 },
			 new object[]{ 1, true, "car" },
			 new object[]{ 1, 2, "bar" },
			 new object[]{ 1, 2, "car" },
			 new int[]{ 1, 2, 3 },
			 new object[]{ 1, 2, 3L, double.NegativeInfinity },
			 new long[]{ 1, 2, 3, long.MinValue },
			 new int[]{ 1, 2, 3, int.MinValue },
			 new object[]{ 1L, 2, 3, Double.NaN },
			 ValueUtils.of( new object[]{ 1L, 2, 3, null } ),
			 new long?[]{ 1L, 2L, 4L },
			 new int[]{ 2 },
			 new int?[]{ 3 },
			 new double[]{ 4D },
			 new double?[]{ 5D },
			 new float[]{ 6 },
			 new float?[]{ 7F },
			 ValueUtils.of( new object[]{ null } ),
			 "",
			 char.MinValue,
			 " ",
			 "20",
			 "X",
			 "Y",
			 "x",
			 "y",
			 Character.MIN_HIGH_SURROGATE,
			 Character.MAX_HIGH_SURROGATE,
			 Character.MIN_LOW_SURROGATE,
			 Character.MAX_LOW_SURROGATE,
			 char.MaxValue,
			 false,
			 true,
			 double.NegativeInfinity,
			 -double.MaxValue,
			 long.MinValue,
			 long.MinValue + 1,
			 int.MinValue,
			 short.MinValue,
			 sbyte.MinValue,
			 0,
			 double.Epsilon,
			 Double.MIN_NORMAL,
			 float.Epsilon,
			 Float.MIN_NORMAL,
			 1L,
			 1.1d,
			 1.2f,
			 Math.E,
			 Math.PI,
			 ( sbyte ) 10,
			 ( short ) 20,
			 sbyte.MaxValue,
			 short.MaxValue,
			 int.MaxValue,
			 9007199254740992D,
			 9007199254740993L,
			 long.MaxValue,
			 float.MaxValue,
			 double.MaxValue,
			 double.PositiveInfinity,
			 Double.NaN,
			 null
		 };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOrderValuesCorrectly()
		 public virtual void ShouldOrderValuesCorrectly()
		 {
			  for ( int i = 2; i < Values.Length; i++ )
			  {
					for ( int j = 2; j < Values.Length; j++ )
					{
						 object left = Values[i];
						 object right = Values[j];

						 int cmpPos = Sign( i - j );
						 int cmpVal = Sign( Compare( left, right ) );

						 if ( cmpPos != cmpVal )
						 {
							  throw new AssertionError( format( "Comparing %s against %s does not agree with their positions in the sorted list (%d and " + "%d)", ToString( left ), ToString( right ), i, j ) );
						 }
					}
			  }
		 }

		 private string ToString( object o )
		 {
			  if ( o == null )
			  {
					return "null";
			  }

			  Type clazz = o.GetType();
			  if ( clazz.Equals( typeof( object[] ) ) )
			  {
					return Arrays.ToString( ( object[] ) o );
			  }
			  else if ( clazz.Equals( typeof( int[] ) ) )
			  {
					return Arrays.ToString( ( int[] ) o );
			  }
			  else if ( clazz.Equals( typeof( int?[] ) ) )
			  {
					return Arrays.ToString( ( int?[] ) o );
			  }
			  else if ( clazz.Equals( typeof( long[] ) ) )
			  {
					return Arrays.ToString( ( long[] ) o );
			  }
			  else if ( clazz.Equals( typeof( long?[] ) ) )
			  {
					return Arrays.ToString( ( long?[] ) o );
			  }
			  else if ( clazz.Equals( typeof( string[] ) ) )
			  {
					return Arrays.ToString( ( string[] ) o );
			  }
			  else if ( clazz.Equals( typeof( bool[] ) ) )
			  {
					return Arrays.ToString( ( bool[] ) o );
			  }
			  else if ( clazz.Equals( typeof( bool?[] ) ) )
			  {
					return Arrays.ToString( ( bool?[] ) o );
			  }
			  else
			  {
					return o.ToString();
			  }
		 }

		 private int Compare<T>( T left, T right )
		 {
			  try
			  {
					int cmp1 = CypherOrderability.Compare( left, right );
					int cmp2 = CypherOrderability.Compare( right, left );
					if ( Sign( cmp1 ) != -Sign( cmp2 ) )
					{
						 throw new AssertionError( format( "Comparator is not symmetric on %s and %s", left, right ) );
					}
					return cmp1;
			  }
			  catch ( IncomparableValuesException e )
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					throw new AssertionError( format( "Failed to compare %s:%s and %s:%s", left, left.GetType().FullName, right, right.GetType().FullName ), e );
			  }
		 }

		 private int Sign( int value )
		 {
			  return Integer.compare( value, 0 );
		 }
	}

}