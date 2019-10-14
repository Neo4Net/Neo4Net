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
	using Test = org.junit.jupiter.api.Test;
	using InOrder = org.mockito.InOrder;


	using Race = Neo4Net.Test.Race;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNotSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.ByteBufferFactory.HEAP_ALLOCATOR;

	internal class ByteBufferFactoryTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCloseGlobalAllocationsOnClose()
		 internal virtual void ShouldCloseGlobalAllocationsOnClose()
		 {
			  // given
			  ByteBufferFactory.Allocator allocator = mock( typeof( ByteBufferFactory.Allocator ) );
			  when( allocator.Allocate( anyInt() ) ).thenAnswer(invocationOnMock => ByteBuffer.allocate(invocationOnMock.getArgument(0)));
			  ByteBufferFactory factory = new ByteBufferFactory( () => allocator, 100 );

			  // when doing some allocations that are counted as global
			  factory.AcquireThreadLocalBuffer();
			  factory.ReleaseThreadLocalBuffer();
			  factory.AcquireThreadLocalBuffer();
			  factory.ReleaseThreadLocalBuffer();
			  factory.GlobalAllocator().allocate(123);
			  factory.GlobalAllocator().allocate(456);
			  // and closing it
			  factory.Close();

			  // then
			  InOrder inOrder = inOrder( allocator );
			  inOrder.verify( allocator, times( 1 ) ).allocate( 100 );
			  inOrder.verify( allocator, times( 1 ) ).allocate( 123 );
			  inOrder.verify( allocator, times( 1 ) ).allocate( 456 );
			  inOrder.verify( allocator, times( 1 ) ).close();
			  inOrder.verifyNoMoreInteractions();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCreateNewInstancesOfLocalAllocators()
		 internal virtual void ShouldCreateNewInstancesOfLocalAllocators()
		 {
			  // given
			  System.Func<ByteBufferFactory.Allocator> allocator = mock( typeof( System.Func ) );
			  when( allocator() ).thenAnswer(invocationOnMock => mock(typeof(ByteBufferFactory.Allocator)));
			  ByteBufferFactory factory = new ByteBufferFactory( allocator, 100 );

			  // when
			  ByteBufferFactory.Allocator localAllocator1 = factory.NewLocalAllocator();
			  ByteBufferFactory.Allocator localAllocator2 = factory.NewLocalAllocator();
			  localAllocator2.Close();
			  ByteBufferFactory.Allocator localAllocator3 = factory.NewLocalAllocator();

			  // then
			  assertNotSame( localAllocator1, localAllocator2 );
			  assertNotSame( localAllocator2, localAllocator3 );
			  assertNotSame( localAllocator1, localAllocator3 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailAcquireThreadLocalBufferIfAlreadyAcquired()
		 internal virtual void ShouldFailAcquireThreadLocalBufferIfAlreadyAcquired()
		 {
			  // given
			  ByteBufferFactory factory = new ByteBufferFactory( () => HEAP_ALLOCATOR, 1024 );
			  factory.AcquireThreadLocalBuffer();

			  // when/then
			  assertThrows( typeof( System.InvalidOperationException ), factory.acquireThreadLocalBuffer );
			  factory.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailReleaseThreadLocalBufferIfNotAcquired()
		 internal virtual void ShouldFailReleaseThreadLocalBufferIfNotAcquired()
		 {
			  // given
			  ByteBufferFactory factory = new ByteBufferFactory( () => HEAP_ALLOCATOR, 1024 );
			  factory.AcquireThreadLocalBuffer();
			  factory.ReleaseThreadLocalBuffer();

			  // when/then
			  assertThrows( typeof( System.InvalidOperationException ), factory.releaseThreadLocalBuffer );
			  factory.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldShareThreadLocalBuffersStressfully() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldShareThreadLocalBuffersStressfully()
		 {
			  // given
			  ByteBufferFactory factory = new ByteBufferFactory( () => HEAP_ALLOCATOR, 1024 );
			  Race race = new Race();
			  int threads = 10;
			  IList<ISet<ByteBuffer>> seenBuffers = new List<ISet<ByteBuffer>>();
			  for ( int i = 0; i < threads; i++ )
			  {
					HashSet<ByteBuffer> seen = new HashSet<ByteBuffer>();
					seenBuffers.Add( seen );
					race.AddContestant(() =>
					{
					 for ( int j = 0; j < 1000; j++ )
					 {
						  ByteBuffer buffer = factory.AcquireThreadLocalBuffer();
						  assertNotNull( buffer );
						  seen.Add( buffer );
						  factory.ReleaseThreadLocalBuffer();
					 }
					}, 1);
			  }

			  // when
			  race.Go();

			  // then
			  for ( int i = 0; i < threads; i++ )
			  {
					assertEquals( 1, seenBuffers[i].Count );
			  }
			  factory.Close();
		 }
	}

}