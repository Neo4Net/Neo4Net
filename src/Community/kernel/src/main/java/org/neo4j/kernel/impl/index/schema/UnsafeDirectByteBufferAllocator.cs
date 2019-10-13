using System;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Impl.Index.Schema
{

	using MemoryAllocationTracker = Neo4Net.Memory.MemoryAllocationTracker;
	using NativeMemoryAllocationRefusedError = Neo4Net.@unsafe.Impl.@internal.Dragons.NativeMemoryAllocationRefusedError;
	using UnsafeUtil = Neo4Net.@unsafe.Impl.@internal.Dragons.UnsafeUtil;
	using Preconditions = Neo4Net.Utils.Preconditions;

	/// <summary>
	/// Allocates <seealso cref="ByteBuffer"/> instances using <seealso cref="UnsafeUtil.newDirectByteBuffer(long, int)"/>/<seealso cref="UnsafeUtil.initDirectByteBuffer(object, long, int)"/>
	/// and frees all allocated memory in <seealso cref="close()"/>.
	/// </summary>
	public class UnsafeDirectByteBufferAllocator : ByteBufferFactory.Allocator
	{
		 private readonly MemoryAllocationTracker _memoryAllocationTracker;
		 private readonly IList<Allocation> _allocations = new List<Allocation>();
		 private bool _closed;

		 public UnsafeDirectByteBufferAllocator( MemoryAllocationTracker memoryAllocationTracker )
		 {
			  this._memoryAllocationTracker = memoryAllocationTracker;
		 }

		 public override ByteBuffer Allocate( int bufferSize )
		 {
			 lock ( this )
			 {
				  AssertOpen();
				  try
				  {
						long address = UnsafeUtil.allocateMemory( bufferSize, _memoryAllocationTracker );
						try
						{
							 ByteBuffer buffer = UnsafeUtil.newDirectByteBuffer( address, bufferSize );
							 UnsafeUtil.initDirectByteBuffer( buffer, address, bufferSize );
							 _allocations.Add( new Allocation( address, bufferSize ) );
							 return buffer;
						}
						catch ( Exception )
						{
							 // What ever went wrong we can safely fall back to on-heap buffer. Free the allocated memory right away first.
							 UnsafeUtil.free( address, bufferSize, _memoryAllocationTracker );
							 return AllocateHeapBuffer( bufferSize );
						}
				  }
				  catch ( NativeMemoryAllocationRefusedError )
				  {
						// What ever went wrong we can safely fall back to on-heap buffer.
						return AllocateHeapBuffer( bufferSize );
				  }
			 }
		 }

		 private ByteBuffer AllocateHeapBuffer( int bufferSize )
		 {
			  return ByteBuffer.allocate( bufferSize );
		 }

		 public override void Close()
		 {
			 lock ( this )
			 {
				  // Idempotent close due to the way the population lifecycle works sometimes
				  if ( !_closed )
				  {
						_allocations.ForEach( allocation => UnsafeUtil.free( allocation.address, allocation.bytes, _memoryAllocationTracker ) );
						_closed = true;
				  }
			 }
		 }

		 private void AssertOpen()
		 {
			  Preconditions.checkState( !_closed, "Already closed" );
		 }

		 private class Allocation
		 {
			  internal readonly long Address;
			  internal readonly int Bytes;

			  internal Allocation( long address, int bytes )
			  {
					this.Address = address;
					this.Bytes = bytes;
			  }
		 }
	}

}