using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.impl.util.collection
{

	using ByteUnit = Org.Neo4j.Io.ByteUnit;
	using MemoryAllocationTracker = Org.Neo4j.Memory.MemoryAllocationTracker;
	using UnsafeUtil = Org.Neo4j.@unsafe.Impl.@internal.Dragons.UnsafeUtil;
	using VisibleForTesting = Org.Neo4j.Util.VisibleForTesting;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Numbers.isPowerOfTwo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Numbers.log2floor;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.Preconditions.checkState;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.Preconditions.requirePositive;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.Preconditions.requirePowerOfTwo;

	/// <summary>
	/// Block allocator that caches freed blocks matching following criteria:
	/// <ul>
	///     <li>Size must be power of 2
	///     <li>Size must be less or equal to <seealso cref="maxCacheableBlockSize"/>
	/// </ul>
	/// 
	/// This class is thread safe.
	/// </summary>
	public class CachingOffHeapBlockAllocator : OffHeapBlockAllocator
	{
		 /// <summary>
		 /// Max size of cached blocks, a power of 2
		 /// </summary>
		 private readonly long _maxCacheableBlockSize;
		 private volatile bool _released;
		 private readonly BlockingQueue<OffHeapBlockAllocator_MemoryBlock>[] _caches;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting public CachingOffHeapBlockAllocator()
		 public CachingOffHeapBlockAllocator() : this(ByteUnit.kibiBytes(512), 128)
		 {
		 }

		 /// <param name="maxCacheableBlockSize"> Max size of cached blocks including alignment padding, must be a power of 2 </param>
		 /// <param name="maxCachedBlocks"> Max number of blocks of each size to store </param>
		 public CachingOffHeapBlockAllocator( long maxCacheableBlockSize, int maxCachedBlocks )
		 {
			  requirePositive( maxCachedBlocks );
			  this._maxCacheableBlockSize = requirePowerOfTwo( maxCacheableBlockSize );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int numOfCaches = log2floor(maxCacheableBlockSize) + 1;
			  int numOfCaches = log2floor( maxCacheableBlockSize ) + 1;
			  //noinspection unchecked
			  this._caches = new BlockingQueue[numOfCaches];
			  for ( int i = 0; i < _caches.Length; i++ )
			  {
					_caches[i] = new ArrayBlockingQueue<OffHeapBlockAllocator_MemoryBlock>( maxCachedBlocks );
			  }
		 }

		 public override OffHeapBlockAllocator_MemoryBlock Allocate( long size, MemoryAllocationTracker tracker )
		 {
			  requirePositive( size );
			  checkState( !_released, "Allocator is already released" );
			  if ( !IsCacheable( size ) )
			  {
					return AllocateNew( size, tracker );
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.BlockingQueue<OffHeapBlockAllocator_MemoryBlock> cache = caches[log2floor(size)];
			  BlockingQueue<OffHeapBlockAllocator_MemoryBlock> cache = _caches[log2floor( size )];
			  OffHeapBlockAllocator_MemoryBlock block = cache.poll();
			  if ( block == null )
			  {
					block = AllocateNew( size, tracker );
			  }
			  else
			  {
					tracker.Allocated( block.UnalignedSize );
			  }
			  return block;
		 }

		 public override void Free( OffHeapBlockAllocator_MemoryBlock block, MemoryAllocationTracker tracker )
		 {
			  if ( _released || !IsCacheable( block.Size ) )
			  {
					DoFree( block, tracker );
					return;
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.BlockingQueue<OffHeapBlockAllocator_MemoryBlock> cache = caches[log2floor(block.size)];
			  BlockingQueue<OffHeapBlockAllocator_MemoryBlock> cache = _caches[log2floor( block.Size )];
			  if ( !cache.offer( block ) )
			  {
					DoFree( block, tracker );
					return;
			  }

			  // it is possible that allocator is released just before we put the block into queue;
			  // in such case case we need to free memory right away, since release() will never be called again
			  if ( _released && cache.remove( block ) )
			  {
					DoFree( block, tracker );
					return;
			  }

			  tracker.Deallocated( block.UnalignedSize );
		 }

		 public override void Release()
		 {
			  _released = true;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<OffHeapBlockAllocator_MemoryBlock> blocks = new java.util.ArrayList<>();
			  IList<OffHeapBlockAllocator_MemoryBlock> blocks = new List<OffHeapBlockAllocator_MemoryBlock>();
			  foreach ( BlockingQueue<OffHeapBlockAllocator_MemoryBlock> cache in _caches )
			  {
					cache.drainTo( blocks );
					blocks.ForEach( block => UnsafeUtil.free( block.unalignedAddr, block.unalignedSize ) );
					blocks.Clear();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting void doFree(OffHeapBlockAllocator_MemoryBlock block, org.neo4j.memory.MemoryAllocationTracker tracker)
		 internal virtual void DoFree( OffHeapBlockAllocator_MemoryBlock block, MemoryAllocationTracker tracker )
		 {
			  UnsafeUtil.free( block.UnalignedAddr, block.UnalignedSize, tracker );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting MemoryBlock allocateNew(long size, org.neo4j.memory.MemoryAllocationTracker tracker)
		 internal virtual MemoryBlock AllocateNew( long size, MemoryAllocationTracker tracker )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long unalignedSize = requirePositive(size) + Long.BYTES - 1;
			  long unalignedSize = requirePositive( size ) + Long.BYTES - 1;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long unalignedAddr = org.neo4j.unsafe.impl.internal.dragons.UnsafeUtil.allocateMemory(unalignedSize, tracker);
			  long unalignedAddr = UnsafeUtil.allocateMemory( unalignedSize, tracker );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long addr = org.neo4j.unsafe.impl.internal.dragons.UnsafeUtil.alignedMemory(unalignedAddr, Long.BYTES);
			  long addr = UnsafeUtil.alignedMemory( unalignedAddr, Long.BYTES );
			  return new OffHeapBlockAllocator_MemoryBlock( addr, size, unalignedAddr, unalignedSize );
		 }

		 private bool IsCacheable( long size )
		 {
			  return isPowerOfTwo( size ) && size <= _maxCacheableBlockSize;
		 }
	}

}