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
namespace Org.Neo4j.Kernel.impl.util
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;

	public class SequenceArray
	{
		 private const long UNSET = -1L;
		 // This is the backing store, treated as a ring, courtesy of cursor
		 private long[] _array;
		 private int _cursor;
		 private int _itemsAhead;
		 private readonly int _longsPerItem;
		 private int _capacity;

		 internal SequenceArray( int longsPerItem, int initialCapacity )
		 {
			  this._longsPerItem = longsPerItem;
			  this._capacity = initialCapacity;
			  this._array = new long[_capacity * longsPerItem];
		 }

		 public virtual void Clear()
		 {
			  _cursor = 0;
			  _itemsAhead = 0;
		 }

		 internal virtual void Offer( long baseNumber, long number, long[] meta )
		 {
			  int diff = ( int )( number - baseNumber );
			  EnsureArrayCapacity( diff );
			  int index = _cursor + diff - 1;

			  // If we offer a value a bit ahead of the last offered value then clear the values in between
			  for ( int i = _cursor + _itemsAhead; i < index; i++ )
			  {
					_array[index( i )] = UNSET;
			  }

			  int absIndex = index( index );
			  _array[absIndex] = number;
			  Array.Copy( meta, 0, _array, absIndex + 1, _longsPerItem - 1 );
			  _itemsAhead = max( _itemsAhead, diff );
		 }

		 private int Index( int logicalIndex )
		 {
			  return ( logicalIndex % _capacity ) * _longsPerItem;
		 }

		 internal virtual long PollHighestGapFree( long given, long[] meta )
		 {
			  // assume that "given" would be placed at cursor
			  long number = given;
			  int length = _itemsAhead - 1;
			  int absIndex = 0;
			  for ( int i = 0; i < length; i++ )
			  {
					// Advance the cursor first because this method is only assumed to be called when offering the number immediately after
					// the current highest gap-free number
					AdvanceCursor();
					int tentativeAbsIndex = Index( _cursor );
					if ( _array[tentativeAbsIndex] == UNSET )
					{ // we found a gap, return the number before the gap
						 break;
					}

					absIndex = tentativeAbsIndex;
					number++;
					Debug.Assert( _array[absIndex] == number, "Expected index " + _cursor + " to be " + number + ", but was " + _array[absIndex] + );
										 ". This is for i=" + i;
			  }

			  // copy the meta values into the supplied meta
			  Array.Copy( _array, absIndex + 1, meta, 0, _longsPerItem - 1 );
			  return number;
		 }

		 private void AdvanceCursor()
		 {
			  Debug.Assert( _itemsAhead > 0 );
			  _itemsAhead--;
			  _cursor = ( _cursor + 1 ) % _capacity;
		 }

		 private void EnsureArrayCapacity( int capacity )
		 {
			  while ( capacity > this._capacity )
			  {
					int newCapacity = this._capacity * 2;
					long[] newArray = new long[newCapacity * _longsPerItem];
					// Copy contents to new array, newArray starting at 0
					for ( int i = 0; i < _itemsAhead; i++ )
					{
						 Array.Copy( _array, Index( _cursor + i ), newArray, Index( i ), _longsPerItem );
					}
					this._array = newArray;
					this._capacity = newCapacity;
					this._cursor = 0;
			  }
		 }

		 public override string ToString()
		 {
			  StringBuilder builder = new StringBuilder();
			  for ( int i = 0; i < _itemsAhead; i++ )
			  {
					long value = _array[Index( _cursor + i )];
					if ( value != UNSET )
					{
						 builder.Append( builder.Length > 0 ? "," : "" ).Append( value );
					}
			  }
			  return builder.ToString();
		 }

		 public virtual bool Seen( long baseNumber, long number, long[] meta )
		 {
			  int diff = ( int )( number - baseNumber );
			  int index = _cursor + diff - 1;

			  if ( index >= _cursor + _itemsAhead )
			  {
					return false;
			  }

			  int absIndex = index( index );
			  long[] arrayRef = _array;
			  long num = arrayRef[absIndex];
			  if ( num != number )
			  {
					return false;
			  }

			  long[] metaCopy = Arrays.copyOfRange( arrayRef, absIndex + 1, absIndex + _longsPerItem );
			  return Arrays.Equals( meta, metaCopy );
		 }
	}

}