using System.Collections.Generic;
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
namespace Neo4Net.Values.@virtual
{

	using Value = Neo4Net.Values.Storable.Value;

	/// <summary>
	/// This class is way too similar to org.neo4j.collection.PrimitiveArrays.
	/// <para>
	/// Should we introduce dependency on primitive collections?
	/// </para>
	/// </summary>
	internal sealed class ArrayHelpers
	{
		 private ArrayHelpers()
		 {
		 }

		 internal static bool IsSortedSet( int[] keys )
		 {
			  for ( int i = 0; i < keys.Length - 1; i++ )
			  {
					if ( keys[i] >= keys[i + 1] )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 internal static bool IsSortedSet( VirtualValue[] keys, IComparer<AnyValue> comparator )
		 {
			  for ( int i = 0; i < keys.Length - 1; i++ )
			  {
					if ( comparator.Compare( keys[i], keys[i + 1] ) >= 0 )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 internal static bool IsSortedSet( Value[] keys, IComparer<AnyValue> comparator )
		 {
			  for ( int i = 0; i < keys.Length - 1; i++ )
			  {
					if ( comparator.Compare( keys[i], keys[i + 1] ) >= 0 )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 internal static bool ContainsNull( AnyValue[] values )
		 {
			  foreach ( AnyValue value in values )
			  {
					if ( value == null )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 internal static bool ContainsNull( IList<AnyValue> values )
		 {
			  foreach ( AnyValue value in values )
			  {
					if ( value == null )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 internal static IEnumerator<T> AsIterator<T>( T[] array )
		 {
			  Debug.Assert( array != null );
			  return new IteratorAnonymousInnerClass( array );
		 }

		 private class IteratorAnonymousInnerClass : IEnumerator<T>
		 {
			 private T[] _array;

			 public IteratorAnonymousInnerClass( T[] array )
			 {
				 this._array = array;
			 }

			 private int index;

			 public bool hasNext()
			 {
				  return index < _array.Length;
			 }

			 public T next()
			 {
				  if ( !hasNext() )
				  {
						throw new NoSuchElementException();
				  }
				  return _array[index++];
			 }
		 }
	}

}