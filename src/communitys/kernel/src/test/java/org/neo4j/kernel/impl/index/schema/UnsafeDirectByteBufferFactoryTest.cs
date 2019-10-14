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
	using Test = org.junit.jupiter.api.Test;

	using LocalMemoryTracker = Neo4Net.Memory.LocalMemoryTracker;
	using MemoryAllocationTracker = Neo4Net.Memory.MemoryAllocationTracker;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;

	internal class UnsafeDirectByteBufferFactoryTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAllocateBuffer()
		 internal virtual void ShouldAllocateBuffer()
		 {
			  // given
			  MemoryAllocationTracker tracker = new LocalMemoryTracker();
			  using ( UnsafeDirectByteBufferAllocator factory = new UnsafeDirectByteBufferAllocator( tracker ) )
			  {
					// when
					int bufferSize = 128;
					factory.Allocate( bufferSize );

					// then
					assertEquals( bufferSize, tracker.UsedDirectMemory() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFreeOnClose()
		 internal virtual void ShouldFreeOnClose()
		 {
			  // given
			  MemoryAllocationTracker tracker = new LocalMemoryTracker();
			  using ( UnsafeDirectByteBufferAllocator factory = new UnsafeDirectByteBufferAllocator( tracker ) )
			  {
					// when
					factory.Allocate( 256 );
			  }

			  // then
			  assertEquals( 0, tracker.UsedDirectMemory() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleMultipleClose()
		 internal virtual void ShouldHandleMultipleClose()
		 {
			  // given
			  MemoryAllocationTracker tracker = new LocalMemoryTracker();
			  UnsafeDirectByteBufferAllocator factory = new UnsafeDirectByteBufferAllocator( tracker );

			  // when
			  factory.Allocate( 256 );
			  factory.Close();

			  // then
			  assertEquals( 0, tracker.UsedDirectMemory() );
			  factory.Close();
			  assertEquals( 0, tracker.UsedDirectMemory() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotAllocateAfterClosed()
		 internal virtual void ShouldNotAllocateAfterClosed()
		 {
			  // given
			  UnsafeDirectByteBufferAllocator factory = new UnsafeDirectByteBufferAllocator( new LocalMemoryTracker() );
			  factory.Close();

			  // when
			  try
			  {
					factory.Allocate( 8 );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// then good
			  }
		 }
	}

}