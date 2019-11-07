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
namespace Neo4Net.Values.utils
{
	using DoubleValue = Neo4Net.Values.Storable.DoubleValue;
	using IntegralValue = Neo4Net.Values.Storable.IntegralValue;
	using LongValue = Neo4Net.Values.Storable.LongValue;
	using NumberValue = Neo4Net.Values.Storable.NumberValue;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.doubleValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.longValue;

	/// <summary>
	/// Helper methods for doing math on Values
	/// </summary>
	public sealed class ValueMath
	{
		 private ValueMath()
		 {
			  throw new System.NotSupportedException( "Do not instantiate" );
		 }

		 /// <summary>
		 /// Overflow safe addition of two longs
		 /// </summary>
		 /// <param name="a"> left-hand operand </param>
		 /// <param name="b"> right-hand operand </param>
		 /// <returns> a + b </returns>
		 public static LongValue Add( long a, long b )
		 {
			  return longValue( Math.addExact( a, b ) );
		 }

		 /// <summary>
		 /// Overflow safe addition of two number values.
		 /// 
		 /// Will not overflow but instead widen the type as necessary. </summary>
		 /// <param name="a"> left-hand operand </param>
		 /// <param name="b"> right-hand operand </param>
		 /// <returns> a + b </returns>
		 public static NumberValue OverflowSafeAdd( NumberValue a, NumberValue b )
		 {
			  if ( a is IntegralValue && b is IntegralValue )
			  {
					return OverflowSafeAdd( a.LongValue(), b.LongValue() );
			  }
			  else
			  {
					return a.Plus( b );
			  }
		 }

		 /// <summary>
		 /// Overflow safe addition of two longs
		 /// <para>
		 /// If the result doesn't fit in a long we widen type to use double instead.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="a"> left-hand operand </param>
		 /// <param name="b"> right-hand operand </param>
		 /// <returns> a + b </returns>
		 public static NumberValue OverflowSafeAdd( long a, long b )
		 {
			  long r = a + b;
			  //Check if result overflows
			  if ( ( ( a ^ r ) & ( b ^ r ) ) < 0 )
			  {
					return Values.doubleValue( ( double ) a + ( double ) b );
			  }
			  return longValue( r );
		 }

		 /// <summary>
		 /// Addition of two doubles
		 /// </summary>
		 /// <param name="a"> left-hand operand </param>
		 /// <param name="b"> right-hand operand </param>
		 /// <returns> a + b </returns>
		 public static DoubleValue Add( double a, double b )
		 {
			  return Values.doubleValue( a + b );
		 }

		 /// <summary>
		 /// Overflow safe subtraction of two longs
		 /// </summary>
		 /// <param name="a"> left-hand operand </param>
		 /// <param name="b"> right-hand operand </param>
		 /// <returns> a - b </returns>
		 public static LongValue Subtract( long a, long b )
		 {
			  return longValue( Math.subtractExact( a, b ) );
		 }

		 /// <summary>
		 /// Overflow safe subtraction of two longs
		 /// <para>
		 /// If the result doesn't fit in a long we widen type to use double instead.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="a"> left-hand operand </param>
		 /// <param name="b"> right-hand operand </param>
		 /// <returns> a - b </returns>
		 public static NumberValue OverflowSafeSubtract( long a, long b )
		 {
			  long r = a - b;
			  //Check if result overflows
			  if ( ( ( a ^ b ) & ( a ^ r ) ) < 0 )
			  {
					return Values.doubleValue( ( double ) a - ( double ) b );
			  }
			  return longValue( r );
		 }

		 /// <summary>
		 /// Subtraction of two doubles
		 /// </summary>
		 /// <param name="a"> left-hand operand </param>
		 /// <param name="b"> right-hand operand </param>
		 /// <returns> a - b </returns>
		 public static DoubleValue Subtract( double a, double b )
		 {
			  return Values.doubleValue( a - b );
		 }

		 /// <summary>
		 /// Overflow safe multiplication of two longs
		 /// </summary>
		 /// <param name="a"> left-hand operand </param>
		 /// <param name="b"> right-hand operand </param>
		 /// <returns> a * b </returns>
		 public static LongValue Multiply( long a, long b )
		 {
			  return longValue( Math.multiplyExact( a, b ) );
		 }

		 /// <summary>
		 /// Overflow safe multiplication of two longs
		 /// <para>
		 /// If the result doesn't fit in a long we widen type to use double instead.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="a"> left-hand operand </param>
		 /// <param name="b"> right-hand operand </param>
		 /// <returns> a * b </returns>
		 public static NumberValue OverflowSafeMultiply( long a, long b )
		 {
			  long r = a * b;
			  //Check if result overflows
			  long aa = Math.Abs( a );
			  long ab = Math.Abs( b );
			  if ( ( long )( ( ulong )( aa | ab ) >> 31 ) != 0 )
			  {
					if ( ( ( b != 0 ) && ( r / b != a ) ) || ( a == long.MinValue && b == -1 ) )
					{
						 return doubleValue( ( double ) a * ( double ) b );
					}
			  }
			  return longValue( r );
		 }

		 /// <summary>
		 /// Multiplication of two doubles
		 /// </summary>
		 /// <param name="a"> left-hand operand </param>
		 /// <param name="b"> right-hand operand </param>
		 /// <returns> a * b </returns>
		 public static DoubleValue Multiply( double a, double b )
		 {
			  return Values.doubleValue( a * b );
		 }
	}

}