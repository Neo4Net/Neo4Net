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
	/// A {@code long[]} on heap, abstracted into a <seealso cref="LongArray"/>.
	/// </summary>
	public class HeapLongArray : HeapNumberArray<LongArray>, LongArray
	{
		 private readonly long[] _array;
		 private readonly long _defaultValue;

		 public HeapLongArray( int length, long defaultValue, long @base ) : base( 8, @base )
		 {
			  this._defaultValue = defaultValue;
			  this._array = new long[length];
			  Clear();
		 }

		 public override long Length()
		 {
			  return _array.Length;
		 }

		 public override long Get( long index )
		 {
			  return _array[index( index )];
		 }

		 public override void Set( long index, long value )
		 {
			  _array[index( index )] = value;
		 }

		 public override void Clear()
		 {
			  Arrays.fill( _array, _defaultValue );
		 }
	}

}