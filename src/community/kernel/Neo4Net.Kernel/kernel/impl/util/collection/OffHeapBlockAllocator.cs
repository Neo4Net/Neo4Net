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
namespace Neo4Net.Kernel.impl.util.collection
{
	using IMemoryAllocationTracker = Neo4Net.Memory.IMemoryAllocationTracker;

	public interface OffHeapBlockAllocator
	{
		 /// <returns> memory block of requested size; there's no guarantee whether allocated memory is zero-filled or dirty </returns>
		 OffHeapBlockAllocator_MemoryBlock Allocate( long size, IMemoryAllocationTracker tracker );

		 void Free( OffHeapBlockAllocator_MemoryBlock block, IMemoryAllocationTracker tracker );

		 void Release();
	}

	 public class OffHeapBlockAllocator_MemoryBlock
	 {
		  internal readonly long Addr;
		  internal readonly long Size;
		  internal readonly long UnalignedAddr;
		  internal readonly long UnalignedSize;

		  internal OffHeapBlockAllocator_MemoryBlock( long addr, long size, long unalignedAddr, long unalignedSize )
		  {
				this.Size = size;
				this.Addr = addr;
				this.UnalignedSize = unalignedSize;
				this.UnalignedAddr = unalignedAddr;
		  }
	 }

}