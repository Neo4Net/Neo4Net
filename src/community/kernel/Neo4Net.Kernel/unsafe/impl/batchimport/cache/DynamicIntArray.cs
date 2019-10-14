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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache
{
	/// <summary>
	/// Dynamically growing <seealso cref="LongArray"/>. Is given a chunk size and chunks are added as higher and higher
	/// items are requested.
	/// </summary>
	/// <seealso cref= NumberArrayFactory#newDynamicIntArray(long, int) </seealso>
	public class DynamicIntArray : DynamicNumberArray<IntArray>, IntArray
	{
		 private readonly int _defaultValue;

		 public DynamicIntArray( NumberArrayFactory factory, long chunkSize, int defaultValue ) : base( factory, chunkSize, new IntArray[0] )
		 {
			  this._defaultValue = defaultValue;
		 }

		 public override int Get( long index )
		 {
			  IntArray chunk = ChunkOrNullAt( index );
			  return chunk != null ? chunk.Get( index ) : _defaultValue;
		 }

		 public override void Set( long index, int value )
		 {
			  At( index ).set( index, value );
		 }

		 protected internal override IntArray AddChunk( long chunkSize, long @base )
		 {
			  return Factory.newIntArray( chunkSize, _defaultValue, @base );
		 }
	}

}