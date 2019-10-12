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
	using MemoryAllocationTracker = Neo4Net.Memory.MemoryAllocationTracker;
	using UnsafeUtil = Neo4Net.@unsafe.Impl.@internal.Dragons.UnsafeUtil;

	/// <summary>
	/// Off-heap version of <seealso cref="LongArray"/> using {@code sun.misc.Unsafe}. Supports arrays with length beyond
	/// Integer.MAX_VALUE.
	/// </summary>
	public class OffHeapLongArray : OffHeapRegularNumberArray<LongArray>, LongArray
	{
		 private readonly long _defaultValue;

		 public OffHeapLongArray( long length, long defaultValue, long @base, MemoryAllocationTracker allocationTracker ) : base( length, 3, @base, allocationTracker )
		 {
			  this._defaultValue = defaultValue;
			  Clear();
		 }

		 public override long Get( long index )
		 {
			  return UnsafeUtil.getLong( AddressOf( index ) );
		 }

		 public override void Set( long index, long value )
		 {
			  UnsafeUtil.putLong( AddressOf( index ), value );
		 }

		 public override void Clear()
		 {
			  if ( IsByteUniform( _defaultValue ) )
			  {
					UnsafeUtil.setMemory( address, length << Shift, ( sbyte )_defaultValue );
			  }
			  else
			  {
					for ( long i = 0, adr = address; i < length; i++, adr += itemSize )
					{
						 UnsafeUtil.putLong( adr, _defaultValue );
					}
			  }
		 }
	}

}