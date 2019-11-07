using System;
using System.Diagnostics;

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
namespace Neo4Net.Collections
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.collection.PrimitiveLongCollections.EMPTY_LONG_ARRAY;

	/// <summary>
	/// Specialized methods for operations on primitive arrays.
	/// 
	/// For set operations (union, intersect, symmetricDifference), input and output arrays
	/// are arrays containing unique values in sorted ascending order.
	/// </summary>
	public class PrimitiveArrays
	{
		 /// <summary>
		 /// Compute union of two sets of integers represented as sorted arrays.
		 /// </summary>
		 /// <param name="lhs">
		 ///         a set of integers, represented as a sorted array. </param>
		 /// <param name="rhs">
		 ///         a set of integers, represented as a sorted array. </param>
		 /// <returns> a set of integers, represented as a sorted array. </returns>
		 // NOTE: this implementation was measured to be faster than an implementation
		 // with countUnique for arrays on size 100+.
		 public static int[] Union( int[] lhs, int[] rhs )
		 {
			  if ( lhs == null || rhs == null )
			  {
					return lhs == null ? rhs : lhs;
			  }

			  Debug.Assert( IsSortedSet( lhs ) && IsSortedSet( rhs ) );
			  if ( lhs.Length < rhs.Length )
			  {
					return Union( rhs, lhs );
			  }
			  int[] merged = null;
			  int m = 0;
			  int l = 0;
			  for ( int r = 0; l <= lhs.Length && r < rhs.Length; )
			  {
					while ( l < lhs.Length && lhs[l] < rhs[r] )
					{
						 if ( merged != null )
						 {
							  merged[m++] = lhs[l];
						 }
						 l++;
					}
					if ( l == lhs.Length )
					{
						 if ( merged == null )
						 {
							  merged = Arrays.copyOf( lhs, lhs.Length + rhs.Length - r );
							  m = l;
						 }
						 Array.Copy( rhs, r, merged, m, rhs.Length - r );
						 m += rhs.Length - r;
						 break;
					}
					if ( lhs[l] > rhs[r] )
					{
						 if ( merged == null )
						 {
							  merged = Arrays.copyOf( lhs, lhs.Length + rhs.Length - r );
							  m = l;
						 }
						 merged[m++] = rhs[r++];
					}
					else // i.e. ( lhs[l] == rhs[r] )
					{
						 if ( merged != null )
						 {
							  merged[m++] = lhs[l];
						 }
						 l++;
						 r++;
					}
			  }
			  if ( merged == null )
			  {
					return lhs;
			  }
			  if ( l < lhs.Length ) // get tail of lhs
			  {
					Array.Copy( lhs, l, merged, m, lhs.Length - l );
					m += lhs.Length - l;
			  }
			  if ( m < merged.Length ) // truncate extra elements
			  {
					merged = Arrays.copyOf( merged, m );
			  }
			  return merged;
		 }

		 /// <summary>
		 /// Compute the intersection of two sorted long array sets. </summary>
		 /// <param name="left"> a sorted array set </param>
		 /// <param name="right"> another sorted array set </param>
		 /// <returns> the intersection, represented as a sorted long array </returns>
		 public static long[] Intersect( long[] left, long[] right )
		 {
			  if ( left == null || right == null )
			  {
					return EMPTY_LONG_ARRAY;
			  }

			  Debug.Assert( IsSortedSet( left ) && IsSortedSet( right ) );

			  long uniqueCounts = CountUnique( left, right );
			  if ( uniqueCounts == 0 ) // complete intersection
			  {
					return right;
			  }
			  if ( right( uniqueCounts ) == right.Length || left( uniqueCounts ) == left.Length ) // non-intersecting
			  {
					return EMPTY_LONG_ARRAY;
			  }

			  long[] intersect = new long[left.Length - left( uniqueCounts )];

			  int cursor = 0;
			  int l = 0;
			  int r = 0;
			  while ( l < left.Length && r < right.Length )
			  {
					if ( left[l] == right[r] )
					{
						 intersect[cursor++] = left[l];
						 l++;
						 r++;
					}
					else if ( left[l] < right[r] )
					{
						 l++;
					}
					else
					{
						 r++;
					}
			  }

			  Debug.Assert( cursor == intersect.Length );
			  return intersect;
		 }
		 /// <summary>
		 /// Compute the symmetric difference (set XOR basically) of two sorted long array sets. </summary>
		 /// <param name="left"> a sorted array set </param>
		 /// <param name="right"> another sorted array set </param>
		 /// <returns> the symmetric difference, represented as a sorted long array </returns>
		 public static long[] SymmetricDifference( long[] left, long[] right )
		 {
			  if ( left == null || right == null )
			  {
					return left == null ? right : left;
			  }

			  Debug.Assert( IsSortedSet( left ) && IsSortedSet( right ) );

			  long uniqueCounts = CountUnique( left, right );
			  if ( uniqueCounts == 0 ) // complete intersection
			  {
					return EMPTY_LONG_ARRAY;
			  }

			  long[] difference = new long[left( uniqueCounts ) + right( uniqueCounts )];

			  int cursor = 0;
			  int l = 0;
			  int r = 0;
			  while ( l < left.Length && r < right.Length )
			  {
					if ( left[l] == right[r] )
					{
						 l++;
						 r++;
					}
					else if ( left[l] < right[r] )
					{
						 difference[cursor++] = left[l];
						 l++;
					}
					else
					{
						 difference[cursor++] = right[r];
						 r++;
					}
			  }
			  while ( l < left.Length )
			  {
					difference[cursor++] = left[l];
					l++;
			  }
			  while ( r < right.Length )
			  {
					difference[cursor++] = right[r];
					r++;
			  }

			  Debug.Assert( cursor == difference.Length );
			  return difference;
		 }

		 /// <summary>
		 /// Compute the number of unique values in two sorted long array sets </summary>
		 /// <param name="left"> a sorted array set </param>
		 /// <param name="right"> another sorted array set </param>
		 /// <returns> int pair packed into long </returns>
		 internal static long CountUnique( long[] left, long[] right )
		 {
			  int l = 0;
			  int r = 0;
			  int uniqueInLeft = 0;
			  int uniqueInRight = 0;
			  while ( l < left.Length && r < right.Length )
			  {
					if ( left[l] == right[r] )
					{
						 l++;
						 r++;
					}
					else if ( left[l] < right[r] )
					{
						 uniqueInLeft++;
						 l++;
					}
					else
					{
						 uniqueInRight++;
						 r++;
					}
			  }
			  uniqueInLeft += left.Length - l;
			  uniqueInRight += right.Length - r;
			  return IntPair( uniqueInLeft, uniqueInRight );
		 }

		 private static long IntPair( int left, int right )
		 {
			  return ( ( ( long ) left ) << ( sizeof( int ) * 8 ) ) | right;
		 }

		 private static int Left( long pair )
		 {
			  return ( int )( pair >> ( sizeof( int ) * 8 ) );
		 }

		 private static int Right( long pair )
		 {
			  return unchecked( ( int )( pair & 0xFFFF_FFFFL ) );
		 }

		 /// <param name="set"> the int[] to be check whether or not it's a sorted set. </param>
		 /// <returns> whether or not the given int[] is a sorted set. </returns>
		 public static bool IsSortedSet( int[] set )
		 {
			  for ( int i = 0; i < set.Length - 1; i++ )
			  {
					AssertSortedSetItem( i, set[i], set[i + 1] );
			  }
			  return true;
		 }

		 /// <param name="set"> the long[] to be checked whether or not it's a sorted set. </param>
		 /// <returns> whether or not the given long[] is a sorted set. </returns>
		 public static bool IsSortedSet( long[] set )
		 {
			  for ( int i = 0; i < set.Length - 1; i++ )
			  {
					AssertSortedSetItem( i, set[i], set[i + 1] );
			  }
			  return true;
		 }

		 private static void AssertSortedSetItem( int i, long item, long next )
		 {
			  if ( item >= next )
			  {
					throw new System.ArgumentException( format( "Array is not a sorted set: has %d before %d at i:%d", item, next, i ) );
			  }
		 }

		 private PrimitiveArrays()
		 { // No instances allowed
		 }
	}

}