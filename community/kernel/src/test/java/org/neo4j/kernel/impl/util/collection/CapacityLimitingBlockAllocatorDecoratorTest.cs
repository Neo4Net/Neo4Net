using System;
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
	using Test = org.junit.jupiter.api.Test;


	using MemoryAllocationTracker = Org.Neo4j.Memory.MemoryAllocationTracker;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertDoesNotThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class CapacityLimitingBlockAllocatorDecoratorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void maxMemoryLimit()
		 internal virtual void MaxMemoryLimit()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.memory.MemoryAllocationTracker tracker = mock(org.neo4j.memory.MemoryAllocationTracker.class);
			  MemoryAllocationTracker tracker = mock( typeof( MemoryAllocationTracker ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final OffHeapBlockAllocator allocator = mock(OffHeapBlockAllocator.class);
			  OffHeapBlockAllocator allocator = mock( typeof( OffHeapBlockAllocator ) );
			  when( allocator.Allocate( anyLong(), any(typeof(MemoryAllocationTracker)) ) ).then(invocation =>
			  {
				long size = invocation.getArgument<long>( 0 );
				return new MemoryBlock( 0, size, 0, size );
			  });
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final CapacityLimitingBlockAllocatorDecorator decorator = new CapacityLimitingBlockAllocatorDecorator(allocator, 1024);
			  CapacityLimitingBlockAllocatorDecorator decorator = new CapacityLimitingBlockAllocatorDecorator( allocator, 1024 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<org.neo4j.kernel.impl.util.collection.OffHeapBlockAllocator_MemoryBlock> blocks = new java.util.ArrayList<>();
			  IList<OffHeapBlockAllocator_MemoryBlock> blocks = new List<OffHeapBlockAllocator_MemoryBlock>();
			  for ( int i = 0; i < 8; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.util.collection.OffHeapBlockAllocator_MemoryBlock block = decorator.allocate(128, tracker);
					OffHeapBlockAllocator_MemoryBlock block = decorator.Allocate( 128, tracker );
					blocks.Add( block );
			  }

			  assertThrows( typeof( Exception ), () => decorator.Allocate(128, tracker) );

			  decorator.Free( blocks.RemoveAt( 0 ), tracker );
			  assertDoesNotThrow( () => decorator.Allocate(128, tracker) );

			  assertThrows( typeof( Exception ), () => decorator.Allocate(256, tracker) );
			  decorator.Free( blocks.RemoveAt( 0 ), tracker );
			  assertThrows( typeof( Exception ), () => decorator.Allocate(256, tracker) );

			  decorator.Free( blocks.RemoveAt( 0 ), tracker );
			  assertDoesNotThrow( () => decorator.Allocate(256, tracker) );
		 }
	}

}