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
namespace Neo4Net.Kernel.impl.util.collection
{

	using IMemoryAllocationTracker = Neo4Net.Memory.IMemoryAllocationTracker;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.util.Preconditions.requirePositive;

	public class CapacityLimitingBlockAllocatorDecorator : OffHeapBlockAllocator
	{
		 private readonly OffHeapBlockAllocator _impl;
		 private readonly long _maxMemory;
		 private readonly AtomicLong _usedMemory = new AtomicLong();

		 public CapacityLimitingBlockAllocatorDecorator( OffHeapBlockAllocator impl, long maxMemory )
		 {
			  this._impl = requireNonNull( impl );
			  this._maxMemory = requirePositive( maxMemory );
		 }

		 public override OffHeapBlockAllocator_MemoryBlock Allocate( long size, IMemoryAllocationTracker tracker )
		 {
			  while ( true )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long usedMemoryBefore = usedMemory.get();
					long usedMemoryBefore = _usedMemory.get();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long usedMemoryAfter = usedMemoryBefore + size;
					long usedMemoryAfter = usedMemoryBefore + size;
					if ( usedMemoryAfter > _maxMemory )
					{
						 throw new Exception( format( "Can't allocate %d bytes due to exceeding memory limit; used=%d, max=%d", size, usedMemoryBefore, _maxMemory ) );
					}
					if ( _usedMemory.compareAndSet( usedMemoryBefore, usedMemoryAfter ) )
					{
						 break;
					}
			  }
			  try
			  {
					return _impl.allocate( size, tracker );
			  }
			  catch ( Exception t )
			  {
					_usedMemory.addAndGet( -size );
					throw t;
			  }
		 }

		 public override void Free( OffHeapBlockAllocator_MemoryBlock block, IMemoryAllocationTracker tracker )
		 {
			  try
			  {
					_impl.free( block, tracker );
			  }
			  finally
			  {
					_usedMemory.addAndGet( -block.Size );
			  }
		 }

		 public override void Release()
		 {
			  try
			  {
					_impl.release();
			  }
			  finally
			  {
					_usedMemory.set( 0 );
			  }
		 }
	}

}