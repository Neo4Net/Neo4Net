using System;
using System.Diagnostics;
using System.Text;

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

	/// <summary>
	/// Methods "missing" from <seealso cref="Arrays"/> are provided here.
	/// </summary>
	/// @deprecated This is mostly an external deprecation, and the class will be moved to an internal package eventually.
	/// However, if you can find an external utility library providing the functionality, then please use that instead. 
	[Obsolete("This is mostly an external deprecation, and the class will be moved to an internal package eventually.")]
	public abstract class ArrayUtil
	{
		 [Obsolete]
		 public static int GetHashCode( object array )
		 {
			  Debug.Assert( array.GetType().IsArray, array + " is not an array" );

			  int length = Array.getLength( array );
			  int result = length;
			  for ( int i = 0; i < length; i++ )
			  {
					result = 31 * result + Array.get( array, i ).GetHashCode();
			  }
			  return result;
		 }

		 [Obsolete]
		 public interface ArrayEquality
		 {
			  bool TypeEquals( Type firstType, Type otherType );

			  bool ItemEquals( object firstArray, object otherArray );
		 }

		 [Obsolete]
		 public static readonly ArrayEquality DEFAULT_ARRAY_EQUALITY = new ArrayEqualityAnonymousInnerClass();

		 private class ArrayEqualityAnonymousInnerClass : ArrayEquality
		 {
			 public bool typeEquals( Type firstType, Type otherType )
			 {
				  return firstType == otherType;
			 }

			 public bool itemEquals( object lhs, object rhs )
			 {
				  return lhs == rhs || lhs != null && lhs.Equals( rhs );
			 }
		 }

		 [Obsolete]
		 public static readonly ArrayEquality BOXING_AWARE_ARRAY_EQUALITY = new ArrayEqualityAnonymousInnerClass2();

		 private class ArrayEqualityAnonymousInnerClass2 : ArrayEquality
		 {
			 public bool typeEquals( Type firstType, Type otherType )
			 {
				  return boxedType( firstType ) == boxedType( otherType );
			 }

			 private Type boxedType( Type type )
			 {
				  if ( !type.IsPrimitive )
				  {
						return type;
				  }

				  if ( type.Equals( Boolean.TYPE ) )
				  {
						return typeof( Boolean );
				  }
				  if ( type.Equals( Byte.TYPE ) )
				  {
						return typeof( Byte );
				  }
				  if ( type.Equals( Short.TYPE ) )
				  {
						return typeof( Short );
				  }
				  if ( type.Equals( Character.TYPE ) )
				  {
						return typeof( Character );
				  }
				  if ( type.Equals( Integer.TYPE ) )
				  {
						return typeof( Integer );
				  }
				  if ( type.Equals( Long.TYPE ) )
				  {
						return typeof( Long );
				  }
				  if ( type.Equals( Float.TYPE ) )
				  {
						return typeof( Float );
				  }
				  if ( type.Equals( Double.TYPE ) )
				  {
						return typeof( Double );
				  }
				  throw new System.ArgumentException( "Oops, forgot to include a primitive type " + type );
			 }

			 public bool itemEquals( object lhs, object rhs )
			 {
				  return lhs == rhs || lhs != null && lhs.Equals( rhs );
			 }
		 }

		 [Obsolete]
		 public static bool Equals( object firstArray, object otherArray )
		 {
			  return Equals( firstArray, otherArray, DEFAULT_ARRAY_EQUALITY );
		 }

		 /// <summary>
		 /// Check if two arrays are equal.
		 /// I also can't believe this method is missing from <seealso cref="Arrays"/>.
		 /// Both arguments must be arrays of some type.
		 /// </summary>
		 /// <param name="firstArray"> value to compare to the other value </param>
		 /// <param name="otherArray"> value to compare to the first value </param>
		 /// <param name="equality"> equality logic </param>
		 /// <returns> Returns {@code true} if the arrays are equal
		 /// </returns>
		 /// <seealso cref= Arrays#equals(byte[], byte[]) for similar functionality. </seealso>
		 [Obsolete]
		 public static bool Equals( object firstArray, object otherArray, ArrayEquality equality )
		 {
			  Debug.Assert( firstArray.GetType().IsArray, firstArray + " is not an array" );
			  Debug.Assert( otherArray.GetType().IsArray, otherArray + " is not an array" );

			  int length;
			  if ( equality.TypeEquals( firstArray.GetType().GetElementType(), otherArray.GetType().GetElementType() ) && (length = Array.getLength(firstArray)) == Array.getLength(otherArray) )
			  {
					for ( int i = 0; i < length; i++ )
					{
						 if ( !equality.ItemEquals( Array.get( firstArray, i ), Array.get( otherArray, i ) ) )
						 {
							  return false;
						 }
					}
					return true;
			  }
			  return false;
		 }

		 [Obsolete]
		 public static object Clone( object array )
		 {
			  if ( array is object[] )
			  {
					return ( ( object[] ) array ).Clone();
			  }
			  if ( array is bool[] )
			  {
					return ( ( bool[] ) array ).Clone();
			  }
			  if ( array is sbyte[] )
			  {
					return ( ( sbyte[] ) array ).Clone();
			  }
			  if ( array is short[] )
			  {
					return ( ( short[] ) array ).Clone();
			  }
			  if ( array is char[] )
			  {
					return ( ( char[] ) array ).Clone();
			  }
			  if ( array is int[] )
			  {
					return ( ( int[] ) array ).Clone();
			  }
			  if ( array is long[] )
			  {
					return ( ( long[] ) array ).Clone();
			  }
			  if ( array is float[] )
			  {
					return ( ( float[] ) array ).Clone();
			  }
			  if ( array is double[] )
			  {
					return ( ( double[] ) array ).Clone();
			  }
			  throw new System.ArgumentException( "Not an array type: " + array.GetType() );
		 }

		 /// <summary>
		 /// Count missing items in an array.
		 /// The order of items doesn't matter.
		 /// </summary>
		 /// <param name="array"> Array to examine </param>
		 /// <param name="contains"> Items to look for </param>
		 /// @param <T> The type of the array items </param>
		 /// <returns> how many of the items in {@code contains} are missing from {@code array}. </returns>
		 [Obsolete]
		 public static int Missing<T>( T[] array, T[] contains )
		 {
			  int missing = 0;
			  foreach ( T check in contains )
			  {
					if ( !contains( array, check ) )
					{
						 missing++;
					}
			  }
			  return missing;
		 }

		 /// <summary>
		 /// Count items from a different array contained in an array.
		 /// The order of items doesn't matter.
		 /// </summary>
		 /// <param name="array"> Array to examine </param>
		 /// <param name="contains"> Items to look for </param>
		 /// @param <T> The type of the array items </param>
		 /// <returns> {@code true} if all items in {@code contains} exists in {@code array}, otherwise {@code false}. </returns>
		 [Obsolete]
		 public static bool ContainsAll<T>( T[] array, T[] contains )
		 {
			  foreach ( T check in contains )
			  {
					if ( !contains( array, check ) )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 /// <summary>
		 /// Check if array contains item.
		 /// </summary>
		 /// <param name="array"> Array to examine </param>
		 /// <param name="contains"> Single item to look for </param>
		 /// @param <T> The type of the array items </param>
		 /// <returns> {@code true} if {@code contains} exists in {@code array}, otherwise {@code false}. </returns>
		 [Obsolete]
		 public static bool Contains<T>( T[] array, T contains )
		 {
			  return contains( array, array.Length, contains );
		 }

		 /// <summary>
		 /// Check if array contains item.
		 /// </summary>
		 /// <param name="array"> Array to examine </param>
		 /// <param name="arrayLength"> Number of items to check, from the start of the array </param>
		 /// <param name="contains"> Single item to look for </param>
		 /// @param <T> The type of the array items </param>
		 /// <returns> {@code true} if {@code contains} exists in {@code array}, otherwise {@code false}. </returns>
		 [Obsolete]
		 public static bool Contains<T>( T[] array, int arrayLength, T contains )
		 {
			  for ( int i = 0; i < arrayLength; i++ )
			  {
					T item = array[i];
					if ( NullSafeEquals( item, contains ) )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 /// <summary>
		 /// Compare two items for equality; if both are {@code null} they are regarded as equal.
		 /// </summary>
		 /// <param name="first"> First item to compare </param>
		 /// <param name="other"> Other item to compare </param>
		 /// @param <T> The type of the items </param>
		 /// <returns> {@code true} if {@code first} and {@code other} are both {@code null} or are both equal. </returns>
		 [Obsolete]
		 public static bool NullSafeEquals<T>( T first, T other )
		 {
			  return first == null ? first == other : first.Equals( other );
		 }

		 /// <summary>
		 /// Get the union of two arrays.
		 /// The resulting array will not contain any duplicates.
		 /// </summary>
		 /// <param name="first"> First array </param>
		 /// <param name="other"> Other array </param>
		 /// @param <T> The type of the arrays </param>
		 /// <returns> an array containing the union of {@code first} and {@code other}. Items occurring in
		 /// both {@code first} and {@code other} will only have of the two in the resulting union. </returns>
		 [Obsolete]
		 public static T[] Union<T>( T[] first, T[] other )
		 {
			  if ( first == null || other == null )
			  {
					return first == null ? other : first;
			  }

			  int missing = missing( first, other );
			  if ( missing == 0 )
			  {
					return first;
			  }

			  // An attempt to add the labels as efficiently as possible
			  T[] union = copyOf( first, first.Length + missing );
			  int cursor = first.Length;
			  foreach ( T candidate in other )
			  {
					if ( !Contains( first, candidate ) )
					{
						 union[cursor++] = candidate;
						 missing--;
					}
			  }
			  Debug.Assert( missing == 0 );
			  return union;
		 }

		 /// <summary>
		 /// Check if provided array is empty </summary>
		 /// <param name="array"> - array to check </param>
		 /// <returns> true if array is null or empty </returns>
		 [Obsolete]
		 public static bool IsEmpty( object[] array )
		 {
			  return ( array == null ) || ( array.Length == 0 );
		 }

		 /// <summary>
		 /// Convert an array to a String using a custom delimiter.
		 /// </summary>
		 /// <param name="items"> The array to convert </param>
		 /// <param name="delimiter"> The delimiter to use </param>
		 /// @param <T> The type of the array </param>
		 /// <returns> a <seealso cref="string"/> representation of {@code items} with a custom delimiter in between. </returns>
		 [Obsolete]
		 public static string Join<T>( T[] items, string delimiter )
		 {
			  StringBuilder builder = new StringBuilder();
			  for ( int i = 0; i < items.Length; i++ )
			  {
					builder.Append( i > 0 ? delimiter : "" ).Append( items[i] );
			  }
			  return builder.ToString();
		 }

		 /// <summary>
		 /// Create new array with all items converted into a new type using a supplied transformer.
		 /// </summary>
		 /// <param name="from"> original array </param>
		 /// <param name="transformer"> transformer that converts an item from the original to the target type </param>
		 /// <param name="toClass"> target type for items </param>
		 /// @param <FROM> type of original items </param>
		 /// @param <TO> type of the converted items </param>
		 /// <returns> a new array with all items from {@code from} converted into type {@code toClass}. </returns>
		 [Obsolete]
		 public static TO[] Map<FROM, TO>( FROM[] from, System.Func<FROM, TO> transformer, Type toClass )
		 {
				 toClass = typeof( TO );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") TO[] result = (TO[]) Array.newInstance(toClass, from.length);
			  TO[] result = ( TO[] ) Array.CreateInstance( toClass, from.Length );
			  for ( int i = 0; i < from.Length; i++ )
			  {
					result[i] = transformer( from[i] );
			  }
			  return result;
		 }

		 /// <summary>
		 /// Create an array from a single first item and additional items following it.
		 /// </summary>
		 /// <param name="first"> the item to put first </param>
		 /// <param name="additional"> the additional items to add to the array </param>
		 /// @param <T> the type of the items </param>
		 /// <returns> a concatenated array where {@code first} as the item at index {@code 0} and the additional
		 /// items following it. </returns>
		 [Obsolete]
		 public static T[] Concat<T>( T first, params T[] additional )
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") T[] result = (T[]) Array.newInstance(additional.getClass().getComponentType(), additional.length + 1);
			  T[] result = ( T[] ) Array.CreateInstance( additional.GetType().GetElementType(), additional.Length + 1 );
			  result[0] = first;
			  Array.Copy( additional, 0, result, 1, additional.Length );
			  return result;
		 }

		 /// <summary>
		 /// Create a array from a existing array and additional items following it.
		 /// </summary>
		 /// <param name="initial"> the initial array </param>
		 /// <param name="additional"> the additional items that would be added into the initial array </param>
		 /// @param <T> the type of the array items </param>
		 /// <returns> a concatenated array and the additional items following it. </returns>
		 [Obsolete]
		 public static T[] Concat<T>( T[] initial, params T[] additional )
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") T[] result = (T[]) Array.newInstance(additional.getClass().getComponentType(), initial.length + additional.length);
			  T[] result = ( T[] ) Array.CreateInstance( additional.GetType().GetElementType(), initial.Length + additional.Length );
			  Array.Copy( initial, 0, result, 0, initial.Length );
			  Array.Copy( additional, 0, result, initial.Length, additional.Length );
			  return result;
		 }

		 /// <summary>
		 /// Create a single array from many arrays.
		 /// </summary>
		 /// <param name="initial"> an initial array </param>
		 /// <param name="additional"> additional arrays to be concatenated with the initial array </param>
		 /// @param <T> the type of the array items </param>
		 /// <returns> the concatenated array </returns>
		 public static T[] ConcatArrays<T>( T[] initial, params T[][] additional )
		 {
			  int length = initial.Length;
			  foreach ( T[] array in additional )
			  {
					length += array.Length;
			  }
			  T[] result = Arrays.copyOf( initial, length );
			  int offset = initial.Length;
			  foreach ( T[] array in additional )
			  {
					Array.Copy( array, 0, result, offset, array.Length );
					offset += array.Length;
			  }
			  return result;
		 }

		 /// <summary>
		 /// Returns the array version of the vararg argument.
		 /// </summary>
		 /// <param name="varargs"> the items </param>
		 /// @param <T> the type of the items </param>
		 /// <returns> the array version of the vararg argument. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @SafeVarargs public static <T> T[] array(T... varargs)
		 [Obsolete]
		 public static T[] Array<T>( params T[] varargs )
		 {
			  return varargs;
		 }

		 [Obsolete]
		 public static T LastOf<T>( T[] array )
		 {
			  return array[array.Length - 1];
		 }

		 [Obsolete]
		 public static int IndexOf<T>( T[] array, T item )
		 {
			  for ( int i = 0; i < array.Length; i++ )
			  {
					if ( array[i].Equals( item ) )
					{
						 return i;
					}
			  }
			  return -1;
		 }

		 [Obsolete]
		 public static T[] Without<T>( T[] source, params T[] toRemove )
		 {
			  T[] result = source.Clone();
			  int length = result.Length;
			  foreach ( T candidate in toRemove )
			  {
					int index = IndexOf( result, candidate );
					if ( index != -1 )
					{
						 if ( index + 1 < length )
						 { // not the last one
							  result[index] = result[length - 1];
						 }
						 length--;
					}
			  }
			  return length == result.Length ? result : Arrays.copyOf( result, length );
		 }

		 [Obsolete]
		 public static void Reverse<T>( T[] array )
		 {
			  for ( int low = 0, high = array.Length - 1; high - low > 0; low++, high-- )
			  {
					T lowItem = array[low];
					array[low] = array[high];
					array[high] = lowItem;
			  }
		 }

		 private ArrayUtil()
		 { // No instances allowed
		 }
	}

}