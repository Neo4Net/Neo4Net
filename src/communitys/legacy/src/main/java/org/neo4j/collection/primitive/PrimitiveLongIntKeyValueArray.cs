using System;

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
namespace Neo4Net.Collections.primitive
{

	/// <summary>
	/// This collection class implements a minimal map-like interface
	/// for an ordered, primitive-based key-value array. The array both
	/// maintains insertion order and ensures key values are unique.
	/// </summary>
	public class PrimitiveLongIntKeyValueArray
	{
		 public const int DEFAULT_INITIAL_CAPACITY = 100;
		 public const double DEFAULT_GROWTH_FACTOR = 0.2;

		 private long[] _naturalKeys = new long[DEFAULT_INITIAL_CAPACITY];
		 private int[] _naturalValues = new int[DEFAULT_INITIAL_CAPACITY];
		 private long[] _sortedKeys = new long[DEFAULT_INITIAL_CAPACITY];
		 private int[] _sortedValues = new int[DEFAULT_INITIAL_CAPACITY];
		 private double _growthFactor;
		 private int _size;

		 public PrimitiveLongIntKeyValueArray( int initialCapacity, double growthFactor )
		 {
			  if ( initialCapacity <= 0 )
			  {
					throw new System.ArgumentException( "Illegal initial capacity: " + initialCapacity );
			  }
			  if ( growthFactor <= 0 )
			  {
					throw new System.ArgumentException( "Illegal growth factor: " + growthFactor );
			  }
			  _naturalKeys = new long[DEFAULT_INITIAL_CAPACITY];
			  _naturalValues = new int[DEFAULT_INITIAL_CAPACITY];
			  _sortedKeys = new long[DEFAULT_INITIAL_CAPACITY];
			  _sortedValues = new int[DEFAULT_INITIAL_CAPACITY];
			  this._growthFactor = growthFactor;
		 }

		 public PrimitiveLongIntKeyValueArray( int initialCapacity ) : this( initialCapacity, DEFAULT_GROWTH_FACTOR )
		 {
		 }

		 public PrimitiveLongIntKeyValueArray() : this(DEFAULT_INITIAL_CAPACITY, DEFAULT_GROWTH_FACTOR)
		 {
		 }

		 /// <summary>
		 /// The current capacity.
		 /// </summary>
		 /// <returns> current capacity </returns>
		 public virtual int Capacity()
		 {
			  return _naturalKeys.Length;
		 }

		 /// <summary>
		 /// The proportion by which this array will automatically grow when full.
		 /// </summary>
		 /// <returns> the growth factor </returns>
		 public virtual double GrowthFactor()
		 {
			  return _growthFactor;
		 }

		 /// <summary>
		 /// Ensure the array has at least the capacity requested. The
		 /// capacity will only ever increase or stay the same.
		 /// </summary>
		 /// <param name="newCapacity"> the new capacity requirement </param>
		 public virtual void EnsureCapacity( int newCapacity )
		 {
			  int capacity = _naturalKeys.Length;
			  if ( newCapacity > capacity )
			  {
					long[] newNaturalKeys = new long[newCapacity];
					int[] newNaturalValues = new int[newCapacity];
					long[] newSortedKeys = new long[newCapacity];
					int[] newSortedValues = new int[newCapacity];
					for ( int i = 0; i < capacity; i++ )
					{
						 newNaturalKeys[i] = _naturalKeys[i];
						 newNaturalValues[i] = _naturalValues[i];
						 newSortedKeys[i] = _sortedKeys[i];
						 newSortedValues[i] = _sortedValues[i];
					}
					_naturalKeys = newNaturalKeys;
					_naturalValues = newNaturalValues;
					_sortedKeys = newSortedKeys;
					_sortedValues = newSortedValues;
			  }
		 }

		 /// <summary>
		 /// The number of items in this array.
		 /// </summary>
		 /// <returns> number of items in the array </returns>
		 public virtual int Size()
		 {
			  return _size;
		 }

		 /// <summary>
		 /// Fetch the integer mapped to the given key or defaultValue if
		 /// that key does not exist.
		 /// </summary>
		 /// <param name="key"> the handle for the required value </param>
		 /// <param name="defaultValue"> value to return if the key is not found </param>
		 /// <returns> the integer value mapped to the key provided </returns>
		 public virtual int GetOrDefault( long key, int defaultValue )
		 {
			  int index = Arrays.binarySearch( _sortedKeys, 0, _size, key );
			  if ( index >= 0 )
			  {
					return _sortedValues[index];
			  }
			  else
			  {
					return defaultValue;
			  }
		 }

		 /// <summary>
		 /// Set the value for a given key if that key is not already in use.
		 /// </summary>
		 /// <param name="key"> the key against which to put the value </param>
		 /// <param name="value"> the value to include </param>
		 /// <returns> true if the value was successfully included, false otherwise </returns>
		 public virtual bool PutIfAbsent( long key, int value )
		 {
			  int capacity = _naturalKeys.Length;
			  if ( _size == capacity )
			  {
					EnsureCapacity( ( int ) Math.Floor( _growthFactor * capacity ) );
			  }

			  int index = Arrays.binarySearch( _sortedKeys, 0, _size, key );
			  if ( index >= 0 )
			  {
					return false;
			  }
			  else
			  {
					index = -index - 1;
					for ( int i = _size; i > index; i-- )
					{
						 int j = i - 1;
						 _sortedKeys[i] = _sortedKeys[j];
						 _sortedValues[i] = _sortedValues[j];
					}
					_naturalKeys[_size] = key;
					_naturalValues[_size] = value;
					_sortedKeys[index] = key;
					_sortedValues[index] = value;

					_size += 1;
					return true;
			  }
		 }

		 /// <summary>
		 /// Clear the array and set a new capacity if not already large enough.
		 /// </summary>
		 /// <param name="newCapacity"> the new capacity requirement </param>
		 public virtual void Reset( int newCapacity )
		 {
			  _size = 0;
			  EnsureCapacity( newCapacity );
		 }

		 /// <summary>
		 /// Return an array of all key values, in order of insertion.
		 /// </summary>
		 /// <returns> array of key values </returns>
		 public virtual long[] Keys()
		 {
			  return Arrays.copyOfRange( _naturalKeys, 0, _size );
		 }
	}

}