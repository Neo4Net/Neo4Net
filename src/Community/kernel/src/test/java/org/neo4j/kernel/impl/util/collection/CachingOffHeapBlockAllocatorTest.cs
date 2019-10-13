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
namespace Neo4Net.Kernel.impl.util.collection
{
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using Test = org.junit.jupiter.api.Test;
	using ParameterizedTest = org.junit.jupiter.@params.ParameterizedTest;
	using ValueSource = org.junit.jupiter.@params.provider.ValueSource;


	using LocalMemoryTracker = Neo4Net.Memory.LocalMemoryTracker;
	using MemoryAllocationTracker = Neo4Net.Memory.MemoryAllocationTracker;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;

	internal class CachingOffHeapBlockAllocatorTest
	{
		 private const int CACHE_SIZE = 4;
		 private const int MAX_CACHEABLE_BLOCK_SIZE = 128;

		 private readonly MemoryAllocationTracker _memoryTracker = new LocalMemoryTracker();
		 private readonly CachingOffHeapBlockAllocator _allocator = spy( new CachingOffHeapBlockAllocator( MAX_CACHEABLE_BLOCK_SIZE, CACHE_SIZE ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach void afterEach()
		 internal virtual void AfterEach()
		 {
			  _allocator.release();
			  assertEquals( 0, _memoryTracker.usedDirectMemory(), "Native memory is leaking" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void allocateAfterRelease()
		 internal virtual void AllocateAfterRelease()
		 {
			  _allocator.release();
			  assertThrows( typeof( System.InvalidOperationException ), () => _allocator.allocate(128, _memoryTracker) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void freeAfterRelease()
		 internal virtual void FreeAfterRelease()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.util.collection.OffHeapBlockAllocator_MemoryBlock block = allocator.allocate(128, memoryTracker);
			  OffHeapBlockAllocator_MemoryBlock block = _allocator.allocate( 128, _memoryTracker );
			  _allocator.release();
			  _allocator.free( block, _memoryTracker );
			  verify( _allocator ).doFree( eq( block ), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void allocateAndFree()
		 internal virtual void AllocateAndFree()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.util.collection.OffHeapBlockAllocator_MemoryBlock block1 = allocator.allocate(128, memoryTracker);
			  OffHeapBlockAllocator_MemoryBlock block1 = _allocator.allocate( 128, _memoryTracker );
			  assertEquals( block1.Size, 128 );
			  assertEquals( 128 + Long.BYTES - 1, block1.UnalignedSize );
			  assertEquals( block1.UnalignedSize, _memoryTracker.usedDirectMemory() );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.util.collection.OffHeapBlockAllocator_MemoryBlock block2 = allocator.allocate(256, memoryTracker);
			  OffHeapBlockAllocator_MemoryBlock block2 = _allocator.allocate( 256, _memoryTracker );
			  assertEquals( block2.Size, 256 );
			  assertEquals( 256 + Long.BYTES - 1, block2.UnalignedSize );
			  assertEquals( block1.UnalignedSize + block2.UnalignedSize, _memoryTracker.usedDirectMemory() );

			  _allocator.free( block1, _memoryTracker );
			  _allocator.free( block2, _memoryTracker );
			  assertEquals( 0, _memoryTracker.usedDirectMemory() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @ValueSource(longs = {10, 100, 256}) void allocateNonCacheableSize(long bytes)
		 internal virtual void AllocateNonCacheableSize( long bytes )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.util.collection.OffHeapBlockAllocator_MemoryBlock block1 = allocator.allocate(bytes, memoryTracker);
			  OffHeapBlockAllocator_MemoryBlock block1 = _allocator.allocate( bytes, _memoryTracker );
			  _allocator.free( block1, _memoryTracker );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.util.collection.OffHeapBlockAllocator_MemoryBlock block2 = allocator.allocate(bytes, memoryTracker);
			  OffHeapBlockAllocator_MemoryBlock block2 = _allocator.allocate( bytes, _memoryTracker );
			  _allocator.free( block2, _memoryTracker );

			  verify( _allocator, times( 2 ) ).allocateNew( eq( bytes ), any() );
			  verify( _allocator ).doFree( eq( block1 ), any() );
			  verify( _allocator ).doFree( eq( block2 ), any() );
			  assertEquals( 0, _memoryTracker.usedDirectMemory() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @ValueSource(longs = {8, 64, 128}) void allocateCacheableSize(long bytes)
		 internal virtual void AllocateCacheableSize( long bytes )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.util.collection.OffHeapBlockAllocator_MemoryBlock block1 = allocator.allocate(bytes, memoryTracker);
			  OffHeapBlockAllocator_MemoryBlock block1 = _allocator.allocate( bytes, _memoryTracker );
			  _allocator.free( block1, _memoryTracker );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.util.collection.OffHeapBlockAllocator_MemoryBlock block2 = allocator.allocate(bytes, memoryTracker);
			  OffHeapBlockAllocator_MemoryBlock block2 = _allocator.allocate( bytes, _memoryTracker );
			  _allocator.free( block2, _memoryTracker );

			  verify( _allocator ).allocateNew( eq( bytes ), any() );
			  verify( _allocator, never() ).doFree(any(), any());
			  assertEquals( 0, _memoryTracker.usedDirectMemory() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void cacheCapacityPerBlockSize()
		 internal virtual void CacheCapacityPerBlockSize()
		 {
			  const int extra = 3;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<org.neo4j.kernel.impl.util.collection.OffHeapBlockAllocator_MemoryBlock> blocks64 = new java.util.ArrayList<>();
			  IList<OffHeapBlockAllocator_MemoryBlock> blocks64 = new List<OffHeapBlockAllocator_MemoryBlock>();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<org.neo4j.kernel.impl.util.collection.OffHeapBlockAllocator_MemoryBlock> blocks128 = new java.util.ArrayList<>();
			  IList<OffHeapBlockAllocator_MemoryBlock> blocks128 = new List<OffHeapBlockAllocator_MemoryBlock>();
			  for ( int i = 0; i < CACHE_SIZE + extra; i++ )
			  {
					blocks64.Add( _allocator.allocate( 64, _memoryTracker ) );
					blocks128.Add( _allocator.allocate( 128, _memoryTracker ) );
			  }

			  verify( _allocator, times( CACHE_SIZE + extra ) ).allocateNew( eq( 64L ), any() );
			  verify( _allocator, times( CACHE_SIZE + extra ) ).allocateNew( eq( 128L ), any() );
			  assertEquals( ( CACHE_SIZE + extra ) * ( 64 + 128 + 2 * ( Long.BYTES - 1 ) ), _memoryTracker.usedDirectMemory() );

			  blocks64.ForEach( it => _allocator.free( it, _memoryTracker ) );
			  assertEquals( ( CACHE_SIZE + extra ) * ( 128 + Long.BYTES - 1 ), _memoryTracker.usedDirectMemory() );

			  blocks128.ForEach( it => _allocator.free( it, _memoryTracker ) );
			  assertEquals( 0, _memoryTracker.usedDirectMemory() );

			  verify( _allocator, times( extra * 2 ) ).doFree( any(), any() );
		 }
	}

}