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
namespace Neo4Net.Helpers
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.Preconditions.requirePositive;

	/// @deprecated This class will be removed from public API in 4.0. 
	[Obsolete("This class will be removed from public API in 4.0.")]
	public class Numbers
	{

		 /// <summary>
		 /// Checks if {@code value} is a power of 2. </summary>
		 /// <param name="value"> the value to check </param>
		 /// <returns> {@code true} if {@code value} is a power of 2. </returns>
		 public static bool IsPowerOfTwo( long value )
		 {
			  return value > 0 && ( value & ( value - 1 ) ) == 0;
		 }

		 /// <summary>
		 /// Returns base 2 logarithm of the closest power of 2 that is less or equal to the {@code value}.
		 /// </summary>
		 /// <param name="value"> a positive long value </param>
		 public static int Log2floor( long value )
		 {
			  return ( ( sizeof( long ) * 8 ) - 1 ) - Long.numberOfLeadingZeros( requirePositive( value ) );
		 }

		 public static short SafeCastIntToUnsignedShort( int value )
		 {
			  if ( ( value & ~0xFFFF ) != 0 )
			  {
					throw new ArithmeticException( GetOverflowMessage( value, "unsigned short" ) );
			  }
			  return ( short ) value;
		 }

		 public static sbyte SafeCastIntToUnsignedByte( int value )
		 {
			  if ( ( value & ~0xFF ) != 0 )
			  {
					throw new ArithmeticException( GetOverflowMessage( value, "unsigned byte" ) );
			  }
			  return ( sbyte ) value;
		 }

		 public static int SafeCastLongToInt( long value )
		 {
			  if ( ( int ) value != value )
			  {
					throw new ArithmeticException( getOverflowMessage( value, Integer.TYPE ) );
			  }
			  return ( int ) value;
		 }

		 public static short SafeCastLongToShort( long value )
		 {
			  if ( ( short ) value != value )
			  {
					throw new ArithmeticException( getOverflowMessage( value, Short.TYPE ) );
			  }
			  return ( short ) value;
		 }

		 public static short SafeCastIntToShort( int value )
		 {
			  if ( ( short ) value != value )
			  {
					throw new ArithmeticException( getOverflowMessage( value, Short.TYPE ) );
			  }
			  return ( short ) value;
		 }

		 public static sbyte SafeCastLongToByte( long value )
		 {
			  if ( ( sbyte ) value != value )
			  {
					throw new ArithmeticException( getOverflowMessage( value, Byte.TYPE ) );
			  }
			  return ( sbyte ) value;
		 }

		 public static int UnsignedShortToInt( short value )
		 {
			  return value & 0xFFFF;
		 }

		 public static int UnsignedByteToInt( sbyte value )
		 {
			  return value & 0xFF;
		 }

		 private static string GetOverflowMessage( long value, Type clazz )
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  return getOverflowMessage( value, clazz.FullName );
		 }

		 private static string GetOverflowMessage( long value, string numericType )
		 {
			  return "Value " + value + " is too big to be represented as " + numericType;
		 }
	}

}