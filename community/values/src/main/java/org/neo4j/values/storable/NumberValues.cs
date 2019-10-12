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
namespace Org.Neo4j.Values.Storable
{

	/// <summary>
	/// Static methods for computing the hashCode of primitive numbers and arrays of primitive numbers.
	/// <para>
	/// Also compares Value typed number arrays.
	/// </para>
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public final class NumberValues
	public sealed class NumberValues
	{
		 private NumberValues()
		 {
		 }

		 /*
		  * Using the fact that the hashcode ∑x_i * 31^(i-1) can be expressed as
		  * a dot product, [1, v_1, v_2, v_2, ..., v_n] • [31^n, 31^{n-1}, ..., 31, 1]. By expressing
		  * it in that way the compiler is smart enough to better parallelize the
		  * computation of the hash code.
		  */
		 internal const int MAX_LENGTH = 10000;
		 private static readonly int[] _coefficients = new int[MAX_LENGTH + 1];
		 private const long NON_DOUBLE_LONG = unchecked( ( long )0xFFE0_0000_0000_0000L ); // doubles are exact integers up to 53 bits

		 static NumberValues()
		 {
			  //We are defining the coefficient vector backwards, [1, 31, 31^2,...]
			  //makes it easier and faster do find the starting position later
			  _coefficients[0] = 1;
			  for ( int i = 1; i <= MAX_LENGTH; ++i )
			  {
					_coefficients[i] = 31 * _coefficients[i - 1];
			  }
		 }

		 /*
		  * For equality semantics it is important that the hashcode of a long
		  * is the same as the hashcode of an int as long as the long can fit in 32 bits.
		  */
		 public static int Hash( long number )
		 {
			  int asInt = ( int ) number;
			  if ( asInt == number )
			  {
					return asInt;
			  }
			  return Long.GetHashCode( number );
		 }

		 public static int Hash( double number )
		 {
			  long asLong = ( long ) number;
			  if ( asLong == number )
			  {
					return Hash( asLong );
			  }
			  long bits = System.BitConverter.DoubleToInt64Bits( number );
			  return ( int )( bits ^ ( ( long )( ( ulong )bits >> 32 ) ) );
		 }

		 /*
		  * This is a slightly silly optimization but by turning the computation
		  * of the hashcode into a dot product we trick the jit compiler to use SIMD
		  * instructions and performance doubles.
		  */
		 public static int Hash( sbyte[] values )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int max = Math.min(values.length, MAX_LENGTH);
			  int max = Math.Min( values.Length, MAX_LENGTH );
			  int result = _coefficients[max];
			  for ( int i = 0; i < values.Length && i < _coefficients.Length - 1; ++i )
			  {
					result += _coefficients[max - i - 1] * values[i];
			  }
			  return result;
		 }

		 public static int Hash( short[] values )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int max = Math.min(values.length, MAX_LENGTH);
			  int max = Math.Min( values.Length, MAX_LENGTH );
			  int result = _coefficients[max];
			  for ( int i = 0; i < values.Length && i < _coefficients.Length - 1; ++i )
			  {
					result += _coefficients[max - i - 1] * values[i];
			  }
			  return result;
		 }

		 public static int Hash( char[] values )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int max = Math.min(values.length, MAX_LENGTH);
			  int max = Math.Min( values.Length, MAX_LENGTH );
			  int result = _coefficients[max];
			  for ( int i = 0; i < values.Length && i < _coefficients.Length - 1; ++i )
			  {
					result += _coefficients[max - i - 1] * values[i];
			  }
			  return result;
		 }

		 public static int Hash( int[] values )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int max = Math.min(values.length, MAX_LENGTH);
			  int max = Math.Min( values.Length, MAX_LENGTH );
			  int result = _coefficients[max];
			  for ( int i = 0; i < values.Length && i < _coefficients.Length - 1; ++i )
			  {
					result += _coefficients[max - i - 1] * values[i];
			  }
			  return result;
		 }

		 public static int Hash( long[] values )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int max = Math.min(values.length, MAX_LENGTH);
			  int max = Math.Min( values.Length, MAX_LENGTH );
			  int result = _coefficients[max];
			  for ( int i = 0; i < values.Length && i < _coefficients.Length - 1; ++i )
			  {
					result += _coefficients[max - i - 1] * NumberValues.Hash( values[i] );
			  }
			  return result;
		 }

		 public static int Hash( float[] values )
		 {
			  int result = 1;
			  foreach ( float value in values )
			  {
					int elementHash = NumberValues.hash( value );
					result = 31 * result + elementHash;
			  }
			  return result;
		 }

		 public static int Hash( double[] values )
		 {
			  int result = 1;
			  foreach ( double value in values )
			  {
					int elementHash = NumberValues.Hash( value );
					result = 31 * result + elementHash;
			  }
			  return result;
		 }

		 public static int Hash( bool[] value )
		 {
			  return Arrays.GetHashCode( value );
		 }

		 public static bool NumbersEqual( double fpn, long @in )
		 {
			  if ( @in < 0 )
			  {
					if ( fpn < 0.0 )
					{
						 if ( ( NON_DOUBLE_LONG & @in ) == 0L ) // the high order bits are only sign bits
						 { // no loss of precision if converting the long to a double, so it's safe to compare as double
							  return fpn == @in;
						 }
						 else if ( fpn < long.MinValue )
						 { // the double is too big to fit in a long, they cannot be equal
							  return false;
						 }
						 else if ( ( fpn == Math.Floor( fpn ) ) && !double.IsInfinity( fpn ) ) // no decimals
						 { // safe to compare as long
							  return @in == ( long ) fpn;
						 }
					}
			  }
			  else
			  {
					if ( !( fpn < 0.0 ) )
					{
						 if ( ( NON_DOUBLE_LONG & @in ) == 0L ) // the high order bits are only sign bits
						 { // no loss of precision if converting the long to a double, so it's safe to compare as double
							  return fpn == @in;
						 }
						 else if ( fpn > long.MaxValue )
						 { // the double is too big to fit in a long, they cannot be equal
							  return false;
						 }
						 else if ( ( fpn == Math.Floor( fpn ) ) && !double.IsInfinity( fpn ) ) // no decimals
						 { // safe to compare as long
							  return @in == ( long ) fpn;
						 }
					}
			  }
			  return false;
		 }

		 // Tested by PropertyValueComparisonTest
		 public static int CompareDoubleAgainstLong( double lhs, long rhs )
		 {
			  if ( ( NON_DOUBLE_LONG & rhs ) != 0L )
			  {
					if ( double.IsNaN( lhs ) )
					{
						 return +1;
					}
					if ( double.IsInfinity( lhs ) )
					{
						 return lhs < 0 ? -1 : +1;
					}
					return decimal.valueOf( lhs ).CompareTo( decimal.valueOf( rhs ) );
			  }
			  return lhs.CompareTo( rhs );
		 }

		 // Tested by PropertyValueComparisonTest
		 public static int CompareLongAgainstDouble( long lhs, double rhs )
		 {
			  return -CompareDoubleAgainstLong( rhs, lhs );
		 }

		 public static bool NumbersEqual( IntegralArray lhs, IntegralArray rhs )
		 {
			  int length = lhs.Length();
			  if ( length != rhs.Length() )
			  {
					return false;
			  }
			  for ( int i = 0; i < length; i++ )
			  {
					if ( lhs.LongValue( i ) != rhs.LongValue( i ) )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public static bool NumbersEqual( FloatingPointArray lhs, FloatingPointArray rhs )
		 {
			  int length = lhs.Length();
			  if ( length != rhs.Length() )
			  {
					return false;
			  }
			  for ( int i = 0; i < length; i++ )
			  {
					if ( lhs.DoubleValue( i ) != rhs.DoubleValue( i ) )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public static bool NumbersEqual( FloatingPointArray fps, IntegralArray ins )
		 {
			  int length = ins.Length();
			  if ( length != fps.Length() )
			  {
					return false;
			  }
			  for ( int i = 0; i < length; i++ )
			  {
					if ( !NumbersEqual( fps.DoubleValue( i ), ins.LongValue( i ) ) )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public static int CompareIntegerArrays( IntegralArray a, IntegralArray b )
		 {
			  int i = 0;
			  int x = 0;
			  int length = Math.Min( a.Length(), b.Length() );

			  while ( x == 0 && i < length )
			  {
					x = Long.compare( a.LongValue( i ), b.LongValue( i ) );
					i++;
			  }

			  if ( x == 0 )
			  {
					x = a.Length() - b.Length();
			  }

			  return x;
		 }

		 public static int CompareIntegerVsFloatArrays( IntegralArray a, FloatingPointArray b )
		 {
			  int i = 0;
			  int x = 0;
			  int length = Math.Min( a.Length(), b.Length() );

			  while ( x == 0 && i < length )
			  {
					x = CompareLongAgainstDouble( a.LongValue( i ), b.DoubleValue( i ) );
					i++;
			  }

			  if ( x == 0 )
			  {
					x = a.Length() - b.Length();
			  }

			  return x;
		 }

		 public static int CompareFloatArrays( FloatingPointArray a, FloatingPointArray b )
		 {
			  int i = 0;
			  int x = 0;
			  int length = Math.Min( a.Length(), b.Length() );

			  while ( x == 0 && i < length )
			  {
					x = a.DoubleValue( i ).CompareTo( b.DoubleValue( i ) );
					i++;
			  }

			  if ( x == 0 )
			  {
					x = a.Length() - b.Length();
			  }

			  return x;
		 }

		 public static int CompareBooleanArrays( BooleanArray a, BooleanArray b )
		 {
			  int i = 0;
			  int x = 0;
			  int length = Math.Min( a.Length(), b.Length() );

			  while ( x == 0 && i < length )
			  {
					x = Boolean.compare( a.BooleanValue( i ), b.BooleanValue( i ) );
					i++;
			  }

			  if ( x == 0 )
			  {
					x = a.Length() - b.Length();
			  }

			  return x;
		 }
	}

}