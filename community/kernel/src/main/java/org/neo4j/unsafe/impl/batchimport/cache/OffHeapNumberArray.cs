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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.cache
{
	using MemoryAllocationTracker = Org.Neo4j.Memory.MemoryAllocationTracker;
	using UnsafeUtil = Org.Neo4j.@unsafe.Impl.@internal.Dragons.UnsafeUtil;

	public abstract class OffHeapNumberArray<N> : BaseNumberArray<N> where N : NumberArray<N>
	{
		 private readonly long _allocatedAddress;
		 protected internal readonly long Address;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly long LengthConflict;
		 protected internal readonly MemoryAllocationTracker AllocationTracker;
		 private readonly long _allocatedBytes;
		 private bool _closed;

		 protected internal OffHeapNumberArray( long length, int itemSize, long @base, MemoryAllocationTracker allocationTracker ) : base( itemSize, @base )
		 {
			  UnsafeUtil.assertHasUnsafe();
			  this.LengthConflict = length;
			  this.AllocationTracker = allocationTracker;

			  long dataSize = length * itemSize;
			  bool itemSizeIsPowerOfTwo = Integer.bitCount( itemSize ) == 1;
			  if ( UnsafeUtil.allowUnalignedMemoryAccess || !itemSizeIsPowerOfTwo )
			  {
					// we can end up here even if we require aligned memory access. Reason is that item size
					// isn't power of two anyway and so we have to fallback to safer means of accessing the memory,
					// i.e. byte for byte.
					_allocatedBytes = dataSize;
					this._allocatedAddress = this.Address = UnsafeUtil.allocateMemory( _allocatedBytes, allocationTracker );
			  }
			  else
			  {
					// the item size is a power of two and we're required to access memory aligned
					// so we can allocate a bit more to ensure we can get an aligned memory address to start from.
					_allocatedBytes = dataSize + itemSize - 1;
					this._allocatedAddress = UnsafeUtil.allocateMemory( _allocatedBytes, allocationTracker );
					this.Address = UnsafeUtil.alignedMemory( _allocatedAddress, itemSize );
			  }
		 }

		 public override long Length()
		 {
			  return LengthConflict;
		 }

		 public override void AcceptMemoryStatsVisitor( MemoryStatsVisitor visitor )
		 {
			  visitor.OffHeapUsage( _allocatedBytes );
		 }

		 public override void Close()
		 {
			  if ( !_closed )
			  {
					if ( LengthConflict > 0 )
					{
						 // Allocating 0 bytes actually returns address 0
						 UnsafeUtil.free( _allocatedAddress, _allocatedBytes, AllocationTracker );
					}
					_closed = true;
			  }
		 }
	}

}