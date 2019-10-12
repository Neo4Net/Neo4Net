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
	/// Static methods for checking the equality of arrays of primitives.
	/// 
	/// This class handles only evaluation of a[] == b[] where type( a ) != type( b ), ei. byte[] == int[] and such.
	/// byte[] == byte[] evaluation can be done using Arrays.equals().
	/// </summary>
	public sealed class PrimitiveArrayValues
	{
		 private PrimitiveArrayValues()
		 {
		 }

		 // TYPED COMPARISON

		 public static bool Equals( sbyte[] a, short[] b )
		 {
			  if ( a.Length != b.Length )
			  {
					return false;
			  }

			  for ( int i = 0; i < a.Length; i++ )
			  {
					if ( a[i] != b[i] )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public static bool Equals( sbyte[] a, int[] b )
		 {
			  if ( a.Length != b.Length )
			  {
					return false;
			  }

			  for ( int i = 0; i < a.Length; i++ )
			  {
					if ( a[i] != b[i] )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public static bool Equals( sbyte[] a, long[] b )
		 {
			  if ( a.Length != b.Length )
			  {
					return false;
			  }

			  for ( int i = 0; i < a.Length; i++ )
			  {
					if ( a[i] != b[i] )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public static bool Equals( sbyte[] a, float[] b )
		 {
			  if ( a.Length != b.Length )
			  {
					return false;
			  }

			  for ( int i = 0; i < a.Length; i++ )
			  {
					if ( a[i] != b[i] )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public static bool Equals( sbyte[] a, double[] b )
		 {
			  if ( a.Length != b.Length )
			  {
					return false;
			  }

			  for ( int i = 0; i < a.Length; i++ )
			  {
					if ( a[i] != b[i] )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public static bool Equals( short[] a, int[] b )
		 {
			  if ( a.Length != b.Length )
			  {
					return false;
			  }

			  for ( int i = 0; i < a.Length; i++ )
			  {
					if ( a[i] != b[i] )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public static bool Equals( short[] a, long[] b )
		 {
			  if ( a.Length != b.Length )
			  {
					return false;
			  }

			  for ( int i = 0; i < a.Length; i++ )
			  {
					if ( a[i] != b[i] )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public static bool Equals( short[] a, float[] b )
		 {
			  if ( a.Length != b.Length )
			  {
					return false;
			  }

			  for ( int i = 0; i < a.Length; i++ )
			  {
					if ( a[i] != b[i] )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public static bool Equals( short[] a, double[] b )
		 {
			  if ( a.Length != b.Length )
			  {
					return false;
			  }

			  for ( int i = 0; i < a.Length; i++ )
			  {
					if ( a[i] != b[i] )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public static bool Equals( int[] a, long[] b )
		 {
			  if ( a.Length != b.Length )
			  {
					return false;
			  }

			  for ( int i = 0; i < a.Length; i++ )
			  {
					if ( a[i] != b[i] )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public static bool Equals( int[] a, float[] b )
		 {
			  if ( a.Length != b.Length )
			  {
					return false;
			  }

			  for ( int i = 0; i < a.Length; i++ )
			  {
					if ( a[i] != b[i] )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public static bool Equals( int[] a, double[] b )
		 {
			  if ( a.Length != b.Length )
			  {
					return false;
			  }

			  for ( int i = 0; i < a.Length; i++ )
			  {
					if ( a[i] != b[i] )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public static bool Equals( long[] a, float[] b )
		 {
			  if ( a.Length != b.Length )
			  {
					return false;
			  }

			  for ( int i = 0; i < a.Length; i++ )
			  {
					if ( a[i] != b[i] )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public static bool Equals( long[] a, double[] b )
		 {
			  if ( a.Length != b.Length )
			  {
					return false;
			  }

			  for ( int i = 0; i < a.Length; i++ )
			  {
					if ( a[i] != b[i] )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public static bool Equals( float[] a, double[] b )
		 {
			  if ( a.Length != b.Length )
			  {
					return false;
			  }

			  for ( int i = 0; i < a.Length; i++ )
			  {
					if ( a[i] != b[i] )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public static bool Equals( char[] a, string[] b )
		 {
			  if ( a.Length != b.Length )
			  {
					return false;
			  }

			  for ( int i = 0; i < a.Length; i++ )
			  {
					string str = b[i];
					if ( string.ReferenceEquals( str, null ) || str.Length != 1 || str[0] != a[i] )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 // NON-TYPED COMPARISON

		 public static bool EqualsObject( sbyte[] a, object b )
		 {
			  if ( b is sbyte[] )
			  {
					return Arrays.Equals( a, ( sbyte[] ) b );
			  }
			  else if ( b is short[] )
			  {
					return Equals( a, ( short[] ) b );
			  }
			  else if ( b is int[] )
			  {
					return Equals( a, ( int[] ) b );
			  }
			  else if ( b is long[] )
			  {
					return Equals( a, ( long[] ) b );
			  }
			  else if ( b is float[] )
			  {
					return Equals( a, ( float[] ) b );
			  }
			  else if ( b is double[] )
			  {
					return Equals( a, ( double[] ) b );
			  }
			  return false;
		 }

		 public static bool EqualsObject( short[] a, object b )
		 {
			  if ( b is sbyte[] )
			  {
					return Equals( ( sbyte[] ) b, a );
			  }
			  else if ( b is short[] )
			  {
					return Arrays.Equals( a, ( short[] ) b );
			  }
			  else if ( b is int[] )
			  {
					return Equals( a, ( int[] ) b );
			  }
			  else if ( b is long[] )
			  {
					return Equals( a, ( long[] ) b );
			  }
			  else if ( b is float[] )
			  {
					return Equals( a, ( float[] ) b );
			  }
			  else if ( b is double[] )
			  {
					return Equals( a, ( double[] ) b );
			  }
			  return false;
		 }

		 public static bool EqualsObject( int[] a, object b )
		 {
			  if ( b is sbyte[] )
			  {
					return Equals( ( sbyte[] ) b, a );
			  }
			  else if ( b is short[] )
			  {
					return Equals( ( short[] ) b, a );
			  }
			  else if ( b is int[] )
			  {
					return Arrays.Equals( a, ( int[] ) b );
			  }
			  else if ( b is long[] )
			  {
					return Equals( a, ( long[] ) b );
			  }
			  else if ( b is float[] )
			  {
					return Equals( a, ( float[] ) b );
			  }
			  else if ( b is double[] )
			  {
					return Equals( a, ( double[] ) b );
			  }
			  return false;
		 }

		 public static bool EqualsObject( long[] a, object b )
		 {
			  if ( b is sbyte[] )
			  {
					return Equals( ( sbyte[] ) b, a );
			  }
			  else if ( b is short[] )
			  {
					return Equals( ( short[] ) b, a );
			  }
			  else if ( b is int[] )
			  {
					return Equals( ( int[] ) b, a );
			  }
			  else if ( b is long[] )
			  {
					return Arrays.Equals( a, ( long[] ) b );
			  }
			  else if ( b is float[] )
			  {
					return Equals( a, ( float[] ) b );
			  }
			  else if ( b is double[] )
			  {
					return Equals( a, ( double[] ) b );
			  }
			  return false;
		 }

		 public static bool EqualsObject( float[] a, object b )
		 {
			  if ( b is sbyte[] )
			  {
					return Equals( ( sbyte[] ) b, a );
			  }
			  else if ( b is short[] )
			  {
					return Equals( ( short[] ) b, a );
			  }
			  else if ( b is int[] )
			  {
					return Equals( ( int[] ) b, a );
			  }
			  else if ( b is long[] )
			  {
					return Equals( ( long[] ) b, a );
			  }
			  else if ( b is float[] )
			  {
					return Arrays.Equals( a, ( float[] ) b );
			  }
			  else if ( b is double[] )
			  {
					return Equals( a, ( double[] ) b );
			  }
			  return false;
		 }

		 public static bool EqualsObject( double[] a, object b )
		 {
			  if ( b is sbyte[] )
			  {
					return Equals( ( sbyte[] ) b, a );
			  }
			  else if ( b is short[] )
			  {
					return Equals( ( short[] ) b, a );
			  }
			  else if ( b is int[] )
			  {
					return Equals( ( int[] ) b, a );
			  }
			  else if ( b is long[] )
			  {
					return Equals( ( long[] ) b, a );
			  }
			  else if ( b is float[] )
			  {
					return Equals( ( float[] ) b, a );
			  }
			  else if ( b is double[] )
			  {
					return Arrays.Equals( a, ( double[] ) b );
			  }
			  return false;
		 }

		 public static bool EqualsObject( char[] a, object b )
		 {
			  if ( b is char[] )
			  {
					return Arrays.Equals( a, ( char[] ) b );
			  }
			  else if ( b is string[] )
			  {
					return Equals( a, ( string[] ) b );
			  }
			  // else if ( other instanceof String ) // should we perhaps support this?
			  return false;
		 }

		 public static bool EqualsObject( string[] a, object b )
		 {
			  if ( b is char[] )
			  {
					return Equals( ( char[] ) b, a );
			  }
			  else if ( b is string[] )
			  {
					return Arrays.Equals( a, ( string[] ) b );
			  }
			  // else if ( other instanceof String ) // should we perhaps support this?
			  return false;
		 }
	}

}