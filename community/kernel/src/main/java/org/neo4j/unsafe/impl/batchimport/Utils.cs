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
namespace Org.Neo4j.@unsafe.Impl.Batchimport
{
	/// <summary>
	/// Common and cross-concern utilities.
	/// </summary>
	public class Utils
	{
		 public enum CompareType
		 {
			  Eq,
			  Gt,
			  Ge,
			  Lt,
			  Le,
			  Ne
		 }

		 public static bool UnsignedCompare( long dataA, long dataB, CompareType compareType )
		 { // works for signed and unsigned values
			  switch ( compareType )
			  {
			  case Org.Neo4j.@unsafe.Impl.Batchimport.Utils.CompareType.Eq:
					return dataA == dataB;
			  case Org.Neo4j.@unsafe.Impl.Batchimport.Utils.CompareType.Ge:
					if ( dataA == dataB )
					{
						 return true;
					}
					// fall through to GT
			  case Org.Neo4j.@unsafe.Impl.Batchimport.Utils.CompareType.Gt:
					return dataA < dataB == ( ( dataA < 0 ) != ( dataB < 0 ) );
			  case Org.Neo4j.@unsafe.Impl.Batchimport.Utils.CompareType.Le:
					if ( dataA == dataB )
					{
						 return true;
					}
					// fall through to LT
			  case Org.Neo4j.@unsafe.Impl.Batchimport.Utils.CompareType.Lt:
					return ( dataA < dataB ) ^ ( ( dataA < 0 ) != ( dataB < 0 ) );
			  case Org.Neo4j.@unsafe.Impl.Batchimport.Utils.CompareType.Ne:
					return false;

			  default:
					throw new System.ArgumentException( "Unknown compare type: " + compareType );
			  }
		 }

		 /// <summary>
		 /// Like <seealso cref="unsignedCompare(long, long, CompareType)"/> but reversed in that you get <seealso cref="CompareType"/>
		 /// from comparing data A and B, i.e. the difference between them.
		 /// </summary>
		 public static CompareType UnsignedDifference( long dataA, long dataB )
		 {
			  if ( dataA == dataB )
			  {
					return CompareType.Eq;
			  }
			  return ( ( dataA < dataB ) ^ ( ( dataA < 0 ) != ( dataB < 0 ) ) ) ? CompareType.Lt : CompareType.Gt;
		 }

		 // Values in the arrays are assumed to be sorted
		 public static bool AnyIdCollides( long[] first, int firstLength, long[] other, int otherLength )
		 {
			  int f = 0;
			  int o = 0;
			  while ( f < firstLength && o < otherLength )
			  {
					if ( first[f] == other[o] )
					{
						 return true;
					}

					if ( first[f] < other[o] )
					{
						 while ( ++f < firstLength && first[f] < other[o] )
						 {
						 }
					}
					else
					{
						 while ( ++o < otherLength && first[f] > other[o] )
						 {
						 }
					}
			  }

			  return false;
		 }

		 public static void MergeSortedInto( long[] values, long[] into, int intoLengthBefore )
		 {
			  int v = values.Length - 1;
			  int i = intoLengthBefore - 1;
			  int t = i + values.Length;
			  while ( v >= 0 || i >= 0 )
			  {
					if ( i == -1 )
					{
						 into[t--] = values[v--];
					}
					else if ( v == -1 )
					{
						 into[t--] = into[i--];
					}
					else if ( values[v] >= into[i] )
					{
						 into[t--] = values[v--];
					}
					else
					{
						 into[t--] = into[i--];
					}
			  }
		 }

		 private Utils()
		 { // No instances allowed
		 }
	}

}