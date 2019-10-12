using System;

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
namespace Neo4Net.Kernel.impl.util.collection
{

	using MemoryAllocationTracker = Neo4Net.Memory.MemoryAllocationTracker;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.copyMemory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.getLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.newDirectByteBuffer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.putLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.@internal.dragons.UnsafeUtil.setMemory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.Preconditions.checkState;

	public class OffHeapMemoryAllocator : MemoryAllocator
	{
		 private readonly MemoryAllocationTracker _tracker;
		 private readonly OffHeapBlockAllocator _blockAllocator;

		 public OffHeapMemoryAllocator( MemoryAllocationTracker tracker, OffHeapBlockAllocator blockAllocator )
		 {
			  this._tracker = requireNonNull( tracker );
			  this._blockAllocator = requireNonNull( blockAllocator );
		 }

		 public override Memory Allocate( long size, bool zeroed )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.util.collection.OffHeapBlockAllocator_MemoryBlock block = blockAllocator.allocate(size, tracker);
			  OffHeapBlockAllocator_MemoryBlock block = _blockAllocator.allocate( size, _tracker );
			  if ( zeroed )
			  {
					setMemory( block.UnalignedAddr, block.UnalignedSize, ( sbyte ) 0 );
			  }
			  return new OffHeapMemory( this, block );
		 }

		 internal class OffHeapMemory : Memory
		 {
			 private readonly OffHeapMemoryAllocator _outerInstance;

			  internal readonly OffHeapBlockAllocator_MemoryBlock Block;

			  internal OffHeapMemory( OffHeapMemoryAllocator outerInstance, OffHeapBlockAllocator_MemoryBlock block )
			  {
				  this._outerInstance = outerInstance;
					this.Block = block;
			  }

			  public override long ReadLong( long offset )
			  {
					return getLong( Block.addr + offset );
			  }

			  public override void WriteLong( long offset, long value )
			  {
					putLong( Block.addr + offset, value );
			  }

			  public override void Clear()
			  {
					setMemory( Block.unalignedAddr, Block.unalignedSize, ( sbyte ) 0 );
			  }

			  public override long Size()
			  {
					return Block.size;
			  }

			  public override void Free()
			  {
					outerInstance.blockAllocator.Free( Block, outerInstance.tracker );
			  }

			  public override Memory Copy()
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.util.collection.OffHeapBlockAllocator_MemoryBlock copy = blockAllocator.allocate(block.size, tracker);
					OffHeapBlockAllocator_MemoryBlock copy = outerInstance.blockAllocator.Allocate( Block.size, outerInstance.tracker );
					copyMemory( Block.addr, copy.Addr, Block.size );
					return new OffHeapMemory( _outerInstance, copy );
			  }

			  public override ByteBuffer AsByteBuffer()
			  {
					checkState( Block.size <= int.MaxValue, "Can't create ByteBuffer: memory size exceeds integer limit" );
					try
					{
						 return newDirectByteBuffer( Block.addr, toIntExact( Block.size ) );
					}
					catch ( Exception e )
					{
						 throw new Exception( e );
					}
			  }
		 }
	}

}