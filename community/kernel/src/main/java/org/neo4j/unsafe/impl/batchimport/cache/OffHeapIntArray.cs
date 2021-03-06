﻿/*
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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.cache
{
	using MemoryAllocationTracker = Org.Neo4j.Memory.MemoryAllocationTracker;
	using UnsafeUtil = Org.Neo4j.@unsafe.Impl.@internal.Dragons.UnsafeUtil;

	/// <summary>
	/// Off-heap version of <seealso cref="IntArray"/> using {@code sun.misc.Unsafe}. Supports arrays with length beyond
	/// Integer.MAX_VALUE.
	/// </summary>
	public class OffHeapIntArray : OffHeapRegularNumberArray<IntArray>, IntArray
	{
		 private readonly int _defaultValue;

		 public OffHeapIntArray( long length, int defaultValue, long @base, MemoryAllocationTracker allocationTracker ) : base( length, 2, @base, allocationTracker )
		 {
			  this._defaultValue = defaultValue;
			  Clear();
		 }

		 public override int Get( long index )
		 {
			  return UnsafeUtil.getInt( AddressOf( index ) );
		 }

		 public override void Set( long index, int value )
		 {
			  UnsafeUtil.putInt( AddressOf( index ), value );
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
						 UnsafeUtil.putInt( adr, _defaultValue );
					}
			  }
		 }
	}

}