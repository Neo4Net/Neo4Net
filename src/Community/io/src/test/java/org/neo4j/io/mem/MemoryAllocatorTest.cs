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
namespace Neo4Net.Io.mem
{
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using Test = org.junit.jupiter.api.Test;

	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using LocalMemoryTracker = Neo4Net.Memory.LocalMemoryTracker;
	using UnsafeUtil = Neo4Net.@unsafe.Impl.@internal.Dragons.UnsafeUtil;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;

	internal class MemoryAllocatorTest
	{
		 private static readonly string _onePage = Neo4Net.Io.pagecache.PageCache_Fields.PAGE_SIZE + "";
		 private static readonly string _eightPages = ( 8 * Neo4Net.Io.pagecache.PageCache_Fields.PAGE_SIZE ) + "";

		 private MemoryAllocator _allocator;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach void tearDown()
		 internal virtual void TearDown()
		 {
			  CloseAllocator();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void allocatedPointerMustNotBeNull()
		 internal virtual void AllocatedPointerMustNotBeNull()
		 {
			  MemoryAllocator mman = CreateAllocator( _eightPages );
			  long address = mman.AllocateAligned( Neo4Net.Io.pagecache.PageCache_Fields.PAGE_SIZE, 8 );
			  assertThat( address, @is( not( 0L ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void allocatedPointerMustBePageAligned()
		 internal virtual void AllocatedPointerMustBePageAligned()
		 {
			  MemoryAllocator mman = CreateAllocator( _eightPages );
			  long address = mman.AllocateAligned( Neo4Net.Io.pagecache.PageCache_Fields.PAGE_SIZE, UnsafeUtil.pageSize() );
			  assertThat( address % UnsafeUtil.pageSize(), @is(0L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void allocatedPointerMustBeAlignedToArbitraryByte()
		 internal virtual void AllocatedPointerMustBeAlignedToArbitraryByte()
		 {
			  int pageSize = UnsafeUtil.pageSize();
			  for ( int initialOffset = 0; initialOffset < 8; initialOffset++ )
			  {
					for ( int i = 0; i < pageSize - 1; i++ )
					{
						 MemoryAllocator mman = CreateAllocator( _onePage );
						 mman.AllocateAligned( initialOffset, 1 );
						 long alignment = 1 + i;
						 long address = mman.AllocateAligned( Neo4Net.Io.pagecache.PageCache_Fields.PAGE_SIZE, alignment );
						 assertThat( "With initial offset " + initialOffset + ", iteration " + i + ", aligning to " + alignment + " and got address " + address, address % alignment, @is( 0L ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustBeAbleToAllocatePastMemoryLimit()
		 internal virtual void MustBeAbleToAllocatePastMemoryLimit()
		 {
			  MemoryAllocator mman = CreateAllocator( _onePage );
			  for ( int i = 0; i < 4100; i++ )
			  {
					assertThat( mman.AllocateAligned( 1, 2 ) % 2, @is( 0L ) );
			  }
			  // Also asserts that no OutOfMemoryError is thrown.
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void allocatedPointersMustBeAlignedPastMemoryLimit()
		 internal virtual void AllocatedPointersMustBeAlignedPastMemoryLimit()
		 {
			  MemoryAllocator mman = CreateAllocator( _onePage );
			  for ( int i = 0; i < 4100; i++ )
			  {
					assertThat( mman.AllocateAligned( 1, 2 ) % 2, @is( 0L ) );
			  }

			  int pageSize = UnsafeUtil.pageSize();
			  for ( int i = 0; i < pageSize - 1; i++ )
			  {
					int alignment = pageSize - i;
					long address = mman.AllocateAligned( Neo4Net.Io.pagecache.PageCache_Fields.PAGE_SIZE, alignment );
					assertThat( "iteration " + i + ", aligning to " + alignment, address % alignment, @is( 0L ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void alignmentCannotBeZero()
		 internal virtual void AlignmentCannotBeZero()
		 {
			  assertThrows( typeof( System.ArgumentException ), () => CreateAllocator(_onePage).allocateAligned(8, 0) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustBeAbleToAllocateSlabsLargerThanGrabSize()
		 internal virtual void MustBeAbleToAllocateSlabsLargerThanGrabSize()
		 {
			  MemoryAllocator mman = CreateAllocator( "2 MiB" );
			  long page1 = mman.AllocateAligned( UnsafeUtil.pageSize(), 1 );
			  long largeBlock = mman.AllocateAligned( 1024 * 1024, 1 ); // 1 MiB
			  long page2 = mman.AllocateAligned( UnsafeUtil.pageSize(), 1 );
			  assertThat( page1, @is( not( 0L ) ) );
			  assertThat( largeBlock, @is( not( 0L ) ) );
			  assertThat( page2, @is( not( 0L ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void allocatingMustIncreaseMemoryUsedAndDecreaseAvailableMemory()
		 internal virtual void AllocatingMustIncreaseMemoryUsedAndDecreaseAvailableMemory()
		 {
			  MemoryAllocator mman = CreateAllocator( _onePage );
			  // We haven't allocated anything, so usedMemory should be zero, and the available memory should be the
			  // initial capacity.
			  assertThat( mman.UsedMemory(), @is(0L) );
			  assertThat( mman.AvailableMemory(), @is((long) Neo4Net.Io.pagecache.PageCache_Fields.PAGE_SIZE) );

			  // Allocate 32 bytes of unaligned memory. Ideally there would be no memory wasted on this allocation,
			  // but in principle we cannot rule it out.
			  mman.AllocateAligned( 32, 1 );
			  assertThat( mman.UsedMemory(), @is(greaterThanOrEqualTo(32L)) );
			  assertThat( mman.AvailableMemory(), @is(lessThanOrEqualTo(Neo4Net.Io.pagecache.PageCache_Fields.PAGE_SIZE - 32L)) );

			  // Allocate another 32 bytes of unaligned memory.
			  mman.AllocateAligned( 32, 1 );
			  assertThat( mman.UsedMemory(), @is(greaterThanOrEqualTo(64L)) );
			  assertThat( mman.AvailableMemory(), @is(lessThanOrEqualTo(Neo4Net.Io.pagecache.PageCache_Fields.PAGE_SIZE - 64L)) );

			  // Allocate 1 byte to throw off any subsequent accidental alignment.
			  mman.AllocateAligned( 1, 1 );

			  // Allocate 32 bytes memory, but this time it is aligned to a 16 byte boundary.
			  mman.AllocateAligned( 32, 16 );
			  // Don't count the 16 byte alignment in our assertions since we might already be accidentally aligned.
			  assertThat( mman.UsedMemory(), @is(greaterThanOrEqualTo(97L)) );
			  assertThat( mman.AvailableMemory(), @is(lessThanOrEqualTo(Neo4Net.Io.pagecache.PageCache_Fields.PAGE_SIZE - 97L)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void trackMemoryAllocations()
		 internal virtual void TrackMemoryAllocations()
		 {
			  LocalMemoryTracker memoryTracker = new LocalMemoryTracker();
			  GrabAllocator allocator = ( GrabAllocator ) MemoryAllocator.createAllocator( "2m", memoryTracker );

			  assertEquals( 0, memoryTracker.UsedDirectMemory() );

			  long pointer = allocator.AllocateAligned( ByteUnit.mebiBytes( 1 ), 1 );

			  assertEquals( ByteUnit.mebiBytes( 1 ), memoryTracker.UsedDirectMemory() );

			  allocator.Close();
			  assertEquals( 0, memoryTracker.UsedDirectMemory() );
		 }

		 private void CloseAllocator()
		 {
			  if ( _allocator != null )
			  {
					_allocator.close();
					_allocator = null;
			  }
		 }

		 private MemoryAllocator CreateAllocator( string expectedMaxMemory )
		 {
			  CloseAllocator();
			  _allocator = MemoryAllocator.createAllocator( expectedMaxMemory, new LocalMemoryTracker() );
			  return _allocator;
		 }
	}

}