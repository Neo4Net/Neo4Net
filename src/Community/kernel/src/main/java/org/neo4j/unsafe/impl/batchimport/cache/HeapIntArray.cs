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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache
{

	/// <summary>
	/// A {@code long[]} on heap, abstracted into a <seealso cref="IntArray"/>.
	/// </summary>
	public class HeapIntArray : HeapNumberArray<IntArray>, IntArray
	{
		 private readonly int[] _array;
		 private readonly int _defaultValue;

		 public HeapIntArray( int length, int defaultValue, long @base ) : base( 4, @base )
		 {
			  this._defaultValue = defaultValue;
			  this._array = new int[length];
			  Clear();
		 }

		 public override long Length()
		 {
			  return _array.Length;
		 }

		 public override int Get( long index )
		 {
			  return _array[index( index )];
		 }

		 public override void Set( long index, int value )
		 {
			  _array[index( index )] = value;
		 }

		 public override void Clear()
		 {
			  Arrays.fill( _array, _defaultValue );
		 }
	}

}