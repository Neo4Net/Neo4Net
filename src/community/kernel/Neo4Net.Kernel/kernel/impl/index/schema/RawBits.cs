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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using NumberValue = Neo4Net.Values.Storable.NumberValue;
	using NumberValues = Neo4Net.Values.Storable.NumberValues;
	using Values = Neo4Net.Values.Storable.Values;

	/// <summary>
	/// Useful to compare values stored as raw bits and value type without having to box them as <seealso cref="NumberValue number values"/>.
	/// </summary>
	internal class RawBits
	{
		 internal const sbyte BYTE = 0;
		 internal const sbyte SHORT = 1;
		 internal const sbyte INT = 2;
		 internal const sbyte LONG = 3;
		 internal const sbyte FLOAT = 4;
		 internal const sbyte DOUBLE = 5;

		 /// <summary>
		 /// Convert value represented by type and raw bits to corresponding <seealso cref="NumberValue"/>. If type is not <seealso cref="BYTE"/>, <seealso cref="SHORT"/>,
		 /// <seealso cref="INT"/>, <seealso cref="LONG"/>, <seealso cref="FLOAT"/> or <seealso cref="DOUBLE"/>, the raw bits will be interpreted as a long.
		 /// </summary>
		 /// <param name="rawBits"> Raw bits of value </param>
		 /// <param name="type"> Type of value </param>
		 /// <returns> <seealso cref="NumberValue"/> with type and value given by provided raw bits and type. </returns>
		 internal static NumberValue AsNumberValue( long rawBits, sbyte type )
		 {
			  switch ( type )
			  {
			  case BYTE:
					return Values.byteValue( ( sbyte ) rawBits );
			  case SHORT:
					return Values.shortValue( ( short ) rawBits );
			  case INT:
					return Values.intValue( ( int ) rawBits );
			  case LONG:
					return Values.longValue( rawBits );
			  case FLOAT:
					return Values.floatValue( Float.intBitsToFloat( ( int ) rawBits ) );
			  case DOUBLE:
					return Values.doubleValue( Double.longBitsToDouble( rawBits ) );
			  default:
					// If type is not recognized, interpret as long.
					return Values.longValue( rawBits );
			  }
		 }

		 /// <summary>
		 /// Compare number values represented by type and raw bits. If type is not <seealso cref="BYTE"/>, <seealso cref="SHORT"/>, <seealso cref="INT"/>, <seealso cref="LONG"/>,
		 /// <seealso cref="FLOAT"/> or <seealso cref="DOUBLE"/>, the raw bits will be compared as long.
		 /// </summary>
		 /// <param name="lhsRawBits"> Raw bits of left hand side value </param>
		 /// <param name="lhsType"> Type of left hand side value </param>
		 /// <param name="rhsRawBits"> Raw bits of right hand side value </param>
		 /// <param name="rhsType"> Type of right hand side value </param>
		 /// <returns> An int less that 0 if lhs value is numerically less than rhs value. An int equal to 0 if lhs and rhs value are
		 /// numerically equal (independent of type) and an int greater than 0 if lhs value is greater than rhs value. </returns>
		 internal static int Compare( long lhsRawBits, sbyte lhsType, long rhsRawBits, sbyte rhsType )
		 {
			  // case integral - integral
			  if ( lhsType == BYTE || lhsType == SHORT || lhsType == INT || lhsType == LONG )
			  {
					return CompareLongAgainstRawType( lhsRawBits, rhsRawBits, rhsType );
			  }
			  else if ( lhsType == FLOAT )
			  {
					double lhsFloat = Float.intBitsToFloat( ( int ) lhsRawBits );
					return CompareDoubleAgainstRawType( lhsFloat, rhsRawBits, rhsType );
			  }
			  else if ( lhsType == DOUBLE )
			  {
					double lhsDouble = Double.longBitsToDouble( lhsRawBits );
					return CompareDoubleAgainstRawType( lhsDouble, rhsRawBits, rhsType );
			  }
			  // We can not throw here because we will visit this method inside a pageCursor.shouldRetry() block.
			  // Just return a comparison that at least will be commutative.
			  return Long.compare( lhsRawBits, rhsRawBits );
		 }

		 private static int CompareLongAgainstRawType( long lhs, long rhsRawBits, sbyte rhsType )
		 {
			  if ( rhsType == BYTE || rhsType == SHORT || rhsType == INT || rhsType == LONG )
			  {
					return Long.compare( lhs, rhsRawBits );
			  }
			  else if ( rhsType == FLOAT )
			  {
					return NumberValues.compareLongAgainstDouble( lhs, Float.intBitsToFloat( ( int ) rhsRawBits ) );
			  }
			  else if ( rhsType == DOUBLE )
			  {
					return NumberValues.compareLongAgainstDouble( lhs, Double.longBitsToDouble( rhsRawBits ) );
			  }
			  // We can not throw here because we will visit this method inside a pageCursor.shouldRetry() block.
			  // Just return a comparison that at least will be commutative.
			  return Long.compare( lhs, rhsRawBits );
		 }

		 private static int CompareDoubleAgainstRawType( double lhsDouble, long rhsRawBits, sbyte rhsType )
		 {
			  if ( rhsType == BYTE || rhsType == SHORT || rhsType == INT || rhsType == LONG )
			  {
					return NumberValues.compareDoubleAgainstLong( lhsDouble, rhsRawBits );
			  }
			  else if ( rhsType == FLOAT )
			  {
					return lhsDouble.CompareTo( Float.intBitsToFloat( ( int ) rhsRawBits ) );
			  }
			  else if ( rhsType == DOUBLE )
			  {
					return lhsDouble.CompareTo( Double.longBitsToDouble( rhsRawBits ) );
			  }
			  // We can not throw here because we will visit this method inside a pageCursor.shouldRetry() block.
			  // Just return a comparison that at least will be commutative.
			  return Long.compare( System.BitConverter.DoubleToInt64Bits( lhsDouble ), rhsRawBits );
		 }
	}

}