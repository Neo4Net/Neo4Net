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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache
{
	using Test = org.junit.Test;

	using NativeMemoryAllocationRefusedError = Neo4Net.@unsafe.Impl.@internal.Dragons.NativeMemoryAllocationRefusedError;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory_Fields.NO_MONITOR;

	public class NumberArrayFactoryTest
	{
		 private const long KILO = 1024;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPickFirstAvailableCandidateLongArray()
		 public virtual void ShouldPickFirstAvailableCandidateLongArray()
		 {
			  // GIVEN
			  NumberArrayFactory factory = new NumberArrayFactory_Auto( NO_MONITOR, NumberArrayFactory.HEAP );

			  // WHEN
			  LongArray array = factory.NewLongArray( KILO, -1 );
			  array.Set( KILO - 10, 12345 );

			  // THEN
			  assertTrue( array is HeapLongArray );
			  assertEquals( 12345, array.Get( KILO - 10 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPickFirstAvailableCandidateLongArrayWhenSomeDontHaveEnoughMemory()
		 public virtual void ShouldPickFirstAvailableCandidateLongArrayWhenSomeDontHaveEnoughMemory()
		 {
			  // GIVEN
			  NumberArrayFactory lowMemoryFactory = mock( typeof( NumberArrayFactory ) );
			  doThrow( typeof( System.OutOfMemoryException ) ).when( lowMemoryFactory ).newLongArray( anyLong(), anyLong(), anyLong() );
			  NumberArrayFactory factory = new NumberArrayFactory_Auto( NO_MONITOR, lowMemoryFactory, NumberArrayFactory.HEAP );

			  // WHEN
			  LongArray array = factory.NewLongArray( KILO, -1 );
			  array.Set( KILO - 10, 12345 );

			  // THEN
			  verify( lowMemoryFactory, times( 1 ) ).newLongArray( KILO, -1, 0 );
			  assertTrue( array is HeapLongArray );
			  assertEquals( 12345, array.Get( KILO - 10 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowOomOnNotEnoughMemory()
		 public virtual void ShouldThrowOomOnNotEnoughMemory()
		 {
			  // GIVEN
			  FailureMonitor monitor = new FailureMonitor();
			  NumberArrayFactory lowMemoryFactory = mock( typeof( NumberArrayFactory ) );
			  doThrow( typeof( System.OutOfMemoryException ) ).when( lowMemoryFactory ).newLongArray( anyLong(), anyLong(), anyLong() );
			  NumberArrayFactory factory = new NumberArrayFactory_Auto( monitor, lowMemoryFactory );

			  // WHEN
			  try
			  {
					factory.NewLongArray( KILO, -1 );
					fail( "Should have thrown" );
			  }
			  catch ( System.OutOfMemoryException )
			  {
					// THEN OK
					assertFalse( monitor.Called );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPickFirstAvailableCandidateIntArray()
		 public virtual void ShouldPickFirstAvailableCandidateIntArray()
		 {
			  // GIVEN
			  FailureMonitor monitor = new FailureMonitor();
			  NumberArrayFactory factory = new NumberArrayFactory_Auto( monitor, NumberArrayFactory.HEAP );

			  // WHEN
			  IntArray array = factory.NewIntArray( KILO, -1 );
			  array.Set( KILO - 10, 12345 );

			  // THEN
			  assertTrue( array is HeapIntArray );
			  assertEquals( 12345, array.Get( KILO - 10 ) );
			  assertEquals( NumberArrayFactory.HEAP, monitor.SuccessfulFactory );
			  assertFalse( monitor.AttemptedAllocationFailures.GetEnumerator().hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPickFirstAvailableCandidateIntArrayWhenSomeThrowOutOfMemoryError()
		 public virtual void ShouldPickFirstAvailableCandidateIntArrayWhenSomeThrowOutOfMemoryError()
		 {
			  // GIVEN
			  NumberArrayFactory lowMemoryFactory = mock( typeof( NumberArrayFactory ) );
			  doThrow( typeof( System.OutOfMemoryException ) ).when( lowMemoryFactory ).newIntArray( anyLong(), anyInt(), anyLong() );
			  NumberArrayFactory factory = new NumberArrayFactory_Auto( NO_MONITOR, lowMemoryFactory, NumberArrayFactory.HEAP );

			  // WHEN
			  IntArray array = factory.NewIntArray( KILO, -1 );
			  array.Set( KILO - 10, 12345 );

			  // THEN
			  verify( lowMemoryFactory, times( 1 ) ).newIntArray( KILO, -1, 0 );
			  assertTrue( array is HeapIntArray );
			  assertEquals( 12345, array.Get( KILO - 10 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPickFirstAvailableCandidateIntArrayWhenSomeThrowNativeMemoryAllocationRefusedError()
		 public virtual void ShouldPickFirstAvailableCandidateIntArrayWhenSomeThrowNativeMemoryAllocationRefusedError()
		 {
			  // GIVEN
			  NumberArrayFactory lowMemoryFactory = mock( typeof( NumberArrayFactory ) );
			  doThrow( typeof( NativeMemoryAllocationRefusedError ) ).when( lowMemoryFactory ).newIntArray( anyLong(), anyInt(), anyLong() );
			  NumberArrayFactory factory = new NumberArrayFactory_Auto( NO_MONITOR, lowMemoryFactory, NumberArrayFactory.HEAP );

			  // WHEN
			  IntArray array = factory.NewIntArray( KILO, -1 );
			  array.Set( KILO - 10, 12345 );

			  // THEN
			  verify( lowMemoryFactory, times( 1 ) ).newIntArray( KILO, -1, 0 );
			  assertTrue( array is HeapIntArray );
			  assertEquals( 12345, array.Get( KILO - 10 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCatchArithmeticExceptionsAndTryNext()
		 public virtual void ShouldCatchArithmeticExceptionsAndTryNext()
		 {
			  // GIVEN
			  NumberArrayFactory throwingMemoryFactory = mock( typeof( NumberArrayFactory ) );
			  ArithmeticException failure = new ArithmeticException( "This is an artificial failure" );
			  doThrow( failure ).when( throwingMemoryFactory ).newByteArray( anyLong(), any(typeof(sbyte[])), anyLong() );
			  FailureMonitor monitor = new FailureMonitor();
			  NumberArrayFactory factory = new NumberArrayFactory_Auto( monitor, throwingMemoryFactory, NumberArrayFactory.HEAP );
			  int itemSize = 4;

			  // WHEN
			  ByteArray array = factory.NewByteArray( KILO, new sbyte[itemSize], 0 );
			  array.SetInt( KILO - 10, 0, 12345 );

			  // THEN
			  verify( throwingMemoryFactory, times( 1 ) ).newByteArray( eq( KILO ), any( typeof( sbyte[] ) ), eq( 0L ) );
			  assertTrue( array is HeapByteArray );
			  assertEquals( 12345, array.GetInt( KILO - 10, 0 ) );
			  assertEquals( KILO * itemSize, monitor.Memory );
			  assertEquals( NumberArrayFactory.HEAP, monitor.SuccessfulFactory );
			  assertEquals( throwingMemoryFactory, single( monitor.AttemptedAllocationFailures ).Factory );
			  assertThat( single( monitor.AttemptedAllocationFailures ).Failure.Message, containsString( failure.Message ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void heapArrayShouldAllowVeryLargeBases()
		 public virtual void HeapArrayShouldAllowVeryLargeBases()
		 {
			  NumberArrayFactory factory = new NumberArrayFactory_Auto( NO_MONITOR, NumberArrayFactory.HEAP );
			  VerifyVeryLargeBaseSupport( factory );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void offHeapArrayShouldAllowVeryLargeBases()
		 public virtual void OffHeapArrayShouldAllowVeryLargeBases()
		 {
			  NumberArrayFactory factory = new NumberArrayFactory_Auto( NO_MONITOR, NumberArrayFactory.OFF_HEAP );
			  VerifyVeryLargeBaseSupport( factory );
		 }

		 private void VerifyVeryLargeBaseSupport( NumberArrayFactory factory )
		 {
			  long @base = int.MaxValue * 1337L;
			  sbyte[] into = new sbyte[1];
			  into[0] = 1;
			  factory.NewByteArray( 10, new sbyte[1], @base ).get( @base + 1, into );
			  assertThat( into[0], @is( ( sbyte ) 0 ) );
			  assertThat( factory.NewIntArray( 10, 1, @base ).get( @base + 1 ), @is( 1 ) );
			  assertThat( factory.NewLongArray( 10, 1, @base ).get( @base + 1 ), @is( 1L ) );
		 }

		 private class FailureMonitor : NumberArrayFactory_Monitor
		 {
			  internal bool Called;
			  internal long Memory;
			  internal NumberArrayFactory SuccessfulFactory;
			  internal IEnumerable<NumberArrayFactory_AllocationFailure> AttemptedAllocationFailures;

			  public override void AllocationSuccessful( long memory, NumberArrayFactory successfulFactory, IEnumerable<NumberArrayFactory_AllocationFailure> attemptedAllocationFailures )
			  {
					this.Memory = memory;
					this.SuccessfulFactory = successfulFactory;
					this.AttemptedAllocationFailures = attemptedAllocationFailures;
					this.Called = true;
			  }
		 }
	}

}