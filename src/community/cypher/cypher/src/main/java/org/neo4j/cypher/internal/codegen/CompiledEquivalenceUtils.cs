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
namespace Neo4Net.Cypher.Internal.codegen
{

	using ValueUtils = Neo4Net.Kernel.impl.util.ValueUtils;
	using AnyValue = Neo4Net.Values.AnyValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.NO_VALUE;

	/// <summary>
	/// Helper class for dealing with equivalence an hash code in compiled code.
	/// 
	/// Note this class contains a lot of duplicated code in order to minimize boxing.
	/// </summary>
	public sealed class CompiledEquivalenceUtils
	{
		 /// <summary>
		 /// Do not instantiate this class
		 /// </summary>
		 private CompiledEquivalenceUtils()
		 {
			  throw new System.NotSupportedException();
		 }

		 /// <summary>
		 /// Checks if two objects are equal according to Cypher semantics </summary>
		 /// <param name="lhs"> the left-hand side to check </param>
		 /// <param name="rhs"> the right-hand sid to check </param>
		 /// <returns> {@code true} if the two objects are equal otherwise {@code false} </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static boolean equals(Object lhs, Object rhs)
		 public static bool Equals( object lhs, object rhs )
		 {
			  if ( lhs == rhs )
			  {
					return true;
			  }
			  else if ( lhs == null || rhs == null || lhs == NO_VALUE || rhs == NO_VALUE )
			  {
					return false;
			  }

			  AnyValue lhsValue = lhs is AnyValue ? ( AnyValue ) lhs : ValueUtils.of( lhs );
			  AnyValue rhsValue = rhs is AnyValue ? ( AnyValue ) rhs : ValueUtils.of( rhs );

			  return lhsValue.Equals( rhsValue );
		 }

		 /// <summary>
		 /// Calculates hash code of a given object </summary>
		 /// <param name="element"> the element to calculate hash code for </param>
		 /// <returns> the hash code of the given object </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static int hashCode(Object element)
		 public static int GetHashCode( object element )
		 {
			  if ( element == null )
			  {
					return 0;
			  }
			  else if ( element is AnyValue )
			  {
					return element.GetHashCode();
			  }
			  else if ( element is Number )
			  {
					return GetHashCode( ( ( Number ) element ).longValue() );
			  }
			  else if ( element is char? )
			  {
					return GetHashCode( ( char ) element );
			  }
			  else if ( element is bool? )
			  {
					return GetHashCode( ( bool ) element );
			  }
			  else if ( element is AnyValue[] )
			  {
					return GetHashCode( ( AnyValue[] ) element );
			  }
			  else if ( element is object[] )
			  {
					return GetHashCode( ( object[] ) element );
			  }
			  else if ( element is long[] )
			  {
					return GetHashCode( ( long[] ) element );
			  }
			  else if ( element is double[] )
			  {
					return GetHashCode( ( double[] ) element );
			  }
			  else if ( element is bool[] )
			  {
					return GetHashCode( ( bool[] ) element );
			  }
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: else if (element instanceof java.util.List<?>)
			  else if ( element is IList<object> )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return hashCode((java.util.List<?>) element);
					return GetHashCode( ( IList<object> ) element );
			  }
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: else if (element instanceof java.util.Map<?,?>)
			  else if ( element is IDictionary<object, ?> )
			  {
					return GetHashCode( ( IDictionary<string, object> ) element );
			  }
			  else if ( element is sbyte[] )
			  {
					return GetHashCode( ( sbyte[] ) element );
			  }
			  else if ( element is short[] )
			  {
					return GetHashCode( ( short[] ) element );
			  }
			  else if ( element is int[] )
			  {
					return GetHashCode( ( int[] ) element );
			  }
			  else if ( element is char[] )
			  {
					return GetHashCode( ( char[] ) element );
			  }
			  else if ( element is float[] )
			  {
					return GetHashCode( ( float[] ) element );
			  }
			  else
			  {
					return element.GetHashCode();
			  }
		 }

		 /// <summary>
		 /// Calculate hash code of a map </summary>
		 /// <param name="map"> the element to calculate hash code for </param>
		 /// <returns> the hash code of the given map </returns>
		 public static int GetHashCode( IDictionary<string, object> map )
		 {
			  int h = 0;
			  foreach ( KeyValuePair<string, object> next in map.SetOfKeyValuePairs() )
			  {
					string k = next.Key;
					object v = next.Value;
					h += ( string.ReferenceEquals( k, null ) ? 0 : k.GetHashCode() ) ^ (v == null ? 0 : GetHashCode(v));
			  }
			  return h;
		 }

		 /// <summary>
		 /// Calculate hash code of a long value </summary>
		 /// <param name="value"> the value to compute hash code for </param>
		 /// <returns> the hash code of the given value </returns>
		 public static int GetHashCode( long value )
		 {
			  return Long.GetHashCode( value );
		 }

		 /// <summary>
		 /// Calculate hash code of a boolean value </summary>
		 /// <param name="value"> the value to compute hash code for </param>
		 /// <returns> the hash code of the given value </returns>
		 public static int GetHashCode( bool value )
		 {
			  return Boolean.GetHashCode( value );
		 }

		 /// <summary>
		 /// Calculate hash code of a char value </summary>
		 /// <param name="value"> the value to compute hash code for </param>
		 /// <returns> the hash code of the given value </returns>
		 public static int GetHashCode( char value )
		 {
			  return Character.GetHashCode( value );
		 }

		 /// <summary>
		 /// Calculate hash code of a char[] value </summary>
		 /// <param name="array"> the value to compute hash code for </param>
		 /// <returns> the hash code of the given value </returns>
		 public static int GetHashCode( char[] array )
		 {
			  int len = array.Length;
			  switch ( len )
			  {
			  case 0:
					return 42;
			  case 1:
					return GetHashCode( array[0] );
			  case 2:
					return 31 * GetHashCode( array[0] ) + GetHashCode( array[1] );
			  case 3:
					return ( 31 * GetHashCode( array[0] ) + GetHashCode( array[1] ) ) * 31 + GetHashCode( array[2] );
			  default:
					return len * ( 31 * GetHashCode( array[0] ) + GetHashCode( array[len / 2] ) * 31 + GetHashCode( array[len - 1] ) );
			  }
		 }

		 /// <summary>
		 /// Calculate hash code of a list value </summary>
		 /// <param name="list"> the value to compute hash code for </param>
		 /// <returns> the hash code of the given value </returns>
		 public static int GetHashCode<T1>( IList<T1> list )
		 {
			  int len = list.Count;
			  switch ( len )
			  {
			  case 0:
					return 42;
			  case 1:
					return GetHashCode( list[0] );
			  case 2:
					return 31 * GetHashCode( list[0] ) + GetHashCode( list[1] );
			  case 3:
					return ( 31 * GetHashCode( list[0] ) + GetHashCode( list[1] ) ) * 31 + GetHashCode( list[2] );
			  default:
					return len * ( 31 * GetHashCode( list[0] ) + GetHashCode( list[len / 2] ) * 31 + GetHashCode( list[len - 1] ) );
			  }
		 }

		 /// <summary>
		 /// Calculate hash code of a Object[] value </summary>
		 /// <param name="array"> the value to compute hash code for </param>
		 /// <returns> the hash code of the given value </returns>
		 public static int GetHashCode( object[] array )
		 {
			  int len = array.Length;
			  switch ( len )
			  {
			  case 0:
					return 42;
			  case 1:
					return GetHashCode( array[0] );
			  case 2:
					return 31 * GetHashCode( array[0] ) + GetHashCode( array[1] );
			  case 3:
					return ( 31 * GetHashCode( array[0] ) + GetHashCode( array[1] ) ) * 31 + GetHashCode( array[2] );
			  default:
					return len * ( 31 * GetHashCode( array[0] ) + GetHashCode( array[len / 2] ) * 31 + GetHashCode( array[len - 1] ) );
			  }
		 }

		 /// <summary>
		 /// Calculate hash code of a AnyValue[] value </summary>
		 /// <param name="array"> the value to compute hash code for </param>
		 /// <returns> the hash code of the given value </returns>
		 public static int GetHashCode( AnyValue[] array )
		 {
			  return Arrays.GetHashCode( array );
		 }

		 /// <summary>
		 /// Calculate hash code of a byte[] value </summary>
		 /// <param name="array"> the value to compute hash code for </param>
		 /// <returns> the hash code of the given value </returns>
		 public static int GetHashCode( sbyte[] array )
		 {
			  int len = array.Length;
			  switch ( len )
			  {
			  case 0:
					return 42;
			  case 1:
					return GetHashCode( array[0] );
			  case 2:
					return 31 * GetHashCode( array[0] ) + GetHashCode( array[1] );
			  case 3:
					return ( 31 * GetHashCode( array[0] ) + GetHashCode( array[1] ) ) * 31 + GetHashCode( array[2] );
			  default:
					return len * ( 31 * GetHashCode( array[0] ) + GetHashCode( array[len / 2] ) * 31 + GetHashCode( array[len - 1] ) );
			  }
		 }

		 /// <summary>
		 /// Calculate hash code of a short[] value </summary>
		 /// <param name="array"> the value to compute hash code for </param>
		 /// <returns> the hash code of the given value </returns>
		 public static int GetHashCode( short[] array )
		 {
			  int len = array.Length;
			  switch ( len )
			  {
			  case 0:
					return 42;
			  case 1:
					return GetHashCode( array[0] );
			  case 2:
					return 31 * GetHashCode( array[0] ) + GetHashCode( array[1] );
			  case 3:
					return ( 31 * GetHashCode( array[0] ) + GetHashCode( array[1] ) ) * 31 + GetHashCode( array[2] );
			  default:
					return len * ( 31 * GetHashCode( array[0] ) + GetHashCode( array[len / 2] ) * 31 + GetHashCode( array[len - 1] ) );
			  }
		 }

		 /// <summary>
		 /// Calculate hash code of a int[] value </summary>
		 /// <param name="array"> the value to compute hash code for </param>
		 /// <returns> the hash code of the given value </returns>
		 public static int GetHashCode( int[] array )
		 {
			  int len = array.Length;
			  switch ( len )
			  {
			  case 0:
					return 42;
			  case 1:
					return GetHashCode( array[0] );
			  case 2:
					return 31 * GetHashCode( array[0] ) + GetHashCode( array[1] );
			  case 3:
					return ( 31 * GetHashCode( array[0] ) + GetHashCode( array[1] ) ) * 31 + GetHashCode( array[2] );
			  default:
					return len * ( 31 * GetHashCode( array[0] ) + GetHashCode( array[len / 2] ) * 31 + GetHashCode( array[len - 1] ) );
			  }
		 }

		 /// <summary>
		 /// Calculate hash code of a long[] value </summary>
		 /// <param name="array"> the value to compute hash code for </param>
		 /// <returns> the hash code of the given value </returns>
		 public static int GetHashCode( long[] array )
		 {
			  int len = array.Length;
			  switch ( len )
			  {
			  case 0:
					return 42;
			  case 1:
					return GetHashCode( array[0] );
			  case 2:
					return 31 * GetHashCode( array[0] ) + GetHashCode( array[1] );
			  case 3:
					return ( 31 * GetHashCode( array[0] ) + GetHashCode( array[1] ) ) * 31 + GetHashCode( array[2] );
			  default:
					return len * ( 31 * GetHashCode( array[0] ) + GetHashCode( array[len / 2] ) * 31 + GetHashCode( array[len - 1] ) );
			  }
		 }

		 /// <summary>
		 /// Calculate hash code of a float[] value </summary>
		 /// <param name="array"> the value to compute hash code for </param>
		 /// <returns> the hash code of the given value </returns>
		 public static int GetHashCode( float[] array )
		 {
			  int len = array.Length;
			  switch ( len )
			  {
			  case 0:
					return 42;
			  case 1:
					return GetHashCode( ( long ) array[0] );
			  case 2:
					return 31 * GetHashCode( ( long ) array[0] ) + GetHashCode( ( long ) array[1] );
			  case 3:
					return ( 31 * GetHashCode( ( long ) array[0] ) + GetHashCode( ( long ) array[1] ) ) * 31 + GetHashCode( ( long ) array[2] );
			  default:
					return len * ( 31 * GetHashCode( ( long ) array[0] ) + GetHashCode( ( long ) array[len / 2] ) * 31 + GetHashCode( ( long ) array[len - 1] ) );
			  }
		 }

		 /// <summary>
		 /// Calculate hash code of a double[] value </summary>
		 /// <param name="array"> the value to compute hash code for </param>
		 /// <returns> the hash code of the given value </returns>
		 public static int GetHashCode( double[] array )
		 {
			  int len = array.Length;
			  switch ( len )
			  {
			  case 0:
					return 42;
			  case 1:
					return GetHashCode( ( long ) array[0] );
			  case 2:
					return 31 * GetHashCode( ( long ) array[0] ) + GetHashCode( ( long ) array[1] );
			  case 3:
					return ( 31 * GetHashCode( ( long ) array[0] ) + GetHashCode( ( long ) array[1] ) ) * 31 + GetHashCode( ( long ) array[2] );
			  default:
					return len * ( 31 * GetHashCode( ( long ) array[0] ) + GetHashCode( ( long ) array[len / 2] ) * 31 + GetHashCode( ( long ) array[len - 1] ) );
			  }
		 }

		 /// <summary>
		 /// Calculate hash code of a boolean[] value </summary>
		 /// <param name="array"> the value to compute hash code for </param>
		 /// <returns> the hash code of the given value </returns>
		 public static int GetHashCode( bool[] array )
		 {
			  int len = array.Length;
			  switch ( len )
			  {
			  case 0:
					return 42;
			  case 1:
					return GetHashCode( array[0] );
			  case 2:
					return 31 * GetHashCode( array[0] ) + GetHashCode( array[1] );
			  case 3:
					return ( 31 * GetHashCode( array[0] ) + GetHashCode( array[1] ) ) * 31 + GetHashCode( array[2] );
			  default:
					return len * ( 31 * GetHashCode( array[0] ) + GetHashCode( array[len / 2] ) * 31 + GetHashCode( array[len - 1] ) );
			  }
		 }

		 private static bool? CompareArrayAndList<T1>( object array, IList<T1> list )
		 {
			  int length = Array.getLength( array );
			  if ( length != list.Count )
			  {
					return false;
			  }

			  int i = 0;
			  foreach ( object o in list )
			  {
					if ( !Equals( o, Array.get( array, i++ ) ) )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 private static bool MixedFloatEquality( float? a, double? b )
		 {
			  return a.Value == b;
		 }
	}


}