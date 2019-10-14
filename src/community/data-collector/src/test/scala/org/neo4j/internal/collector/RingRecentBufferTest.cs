using System.Threading;

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
namespace Neo4Net.Internal.Collector
{
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongSets = org.eclipse.collections.impl.factory.primitive.LongSets;
	using Assertions = org.junit.jupiter.api.Assertions;
	using Test = org.junit.jupiter.api.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.fail;

	internal class RingRecentBufferTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldJustWork()
		 internal virtual void ShouldJustWork()
		 {
			  int bufferSize = 4;
			  RingRecentBuffer<long> buffer = new RingRecentBuffer<long>( bufferSize );

			  buffer.Foreach( l => fail( "boom" ) );

			  for ( long i = 0; i < 10; i++ )
			  {
					buffer.Produce( i );
					buffer.Foreach( Assertions.assertNotNull );
			  }

			  buffer.Clear();
			  buffer.Foreach( l => fail( "boom" ) );

			  for ( long i = 0; i < 10; i++ )
			  {
					buffer.Produce( i );
			  }
			  buffer.Foreach( Assertions.assertNotNull );

			  assertEquals( 0, buffer.NumSilentQueryDrops() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleSize0()
		 internal virtual void ShouldHandleSize0()
		 {
			  RingRecentBuffer<long> buffer = new RingRecentBuffer<long>( 0 );

			  buffer.Foreach( l => fail( "boom" ) );
			  buffer.Clear();

			  buffer.Produce( 0L );
			  buffer.Foreach( l => fail( "boom" ) );
			  buffer.Clear();

			  assertEquals( 0, buffer.NumSilentQueryDrops() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotReadSameElementTwice() throws java.util.concurrent.ExecutionException, InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldNotReadSameElementTwice()
		 {
			  // given
			  int n = 1000;
			  int bufferSize = 16;
			  RingRecentBuffer<long> buffer = new RingRecentBuffer<long>( bufferSize );
			  ExecutorService executor = Executors.newFixedThreadPool( 2 );

			  try
			  {
					UniqueElementsConsumer consumer = new UniqueElementsConsumer();

					// when
					// producer thread
					System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> produce = executor.submit(stressUntil(latch, buffer::produce));
					Future<object> produce = executor.submit( StressUntil( latch, buffer.produce ) );

					// consumer thread
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> consume = executor.submit(stress(n, i ->
					Future<object> consume = executor.submit(Stress(n, i =>
					{
					consumer.Reset();
					buffer.Foreach( consumer );
					assertTrue( consumer.Values.size() <= bufferSize, format("Should see at most %d elements", bufferSize) );
					}));

					// then without illegal transitions or exceptions
					consume.get();
					latch.Signal();
					produce.get();
			  }
			  finally
			  {
					executor.shutdown();
			  }
			  assertEquals( 0, buffer.NumSilentQueryDrops() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNeverReadUnwrittenElements() throws java.util.concurrent.ExecutionException, InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldNeverReadUnwrittenElements()
		 {
			  // given
			  int n = 1000000;
			  int bufferSize = 16;
			  RingRecentBuffer<long> buffer = new RingRecentBuffer<long>( bufferSize );
			  ExecutorService executor = Executors.newFixedThreadPool( 2 );

			  try
			  {
					// when
					// producer thread
					System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> produce = executor.submit(stressUntil(latch, buffer::produce));
					Future<object> produce = executor.submit( StressUntil( latch, buffer.produce ) );
					// consumer thread
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> consume = executor.submit(stress(n, i ->
					Future<object> consume = executor.submit(Stress(n, i =>
					{
					buffer.Clear();
					buffer.Foreach( Assertions.assertNotNull );
					}));

					// then without illegal transitions or exceptions
					consume.get();
					latch.Signal();
					produce.get();
			  }
			  finally
			  {
					executor.shutdown();
			  }
			  assertEquals( 0, buffer.NumSilentQueryDrops() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldWorkWithManyConcurrentProducers() throws java.util.concurrent.ExecutionException, InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldWorkWithManyConcurrentProducers()
		 {
			  // given
			  int n = 1000000;
			  int bufferSize = 16;
			  RingRecentBuffer<long> buffer = new RingRecentBuffer<long>( bufferSize );
			  ExecutorService executor = Executors.newFixedThreadPool( 4 );

			  try
			  {
					// when
					// producer threads
					System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> produce1 = executor.submit(stressUntil(latch, buffer::produce));
					Future<object> produce1 = executor.submit( StressUntil( latch, buffer.produce ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> produce2 = executor.submit(stressUntil(latch, buffer::produce));
					Future<object> produce2 = executor.submit( StressUntil( latch, buffer.produce ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> produce3 = executor.submit(stressUntil(latch, buffer::produce));
					Future<object> produce3 = executor.submit( StressUntil( latch, buffer.produce ) );
					// consumer thread
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> consume = executor.submit(stress(n, i ->
					Future<object> consume = executor.submit(Stress(n, i =>
					{
					buffer.Clear();
					buffer.Foreach( Assertions.assertNotNull );
					}));

					// then without illegal transitions or exceptions
					consume.get();
					latch.Signal();
					produce1.get();
					produce2.get();
					produce3.get();
			  }
			  finally
			  {
					executor.shutdown();
			  }
			  // on some systems thread scheduling variance actually causes ~100 silent drops in this test
			  assertTrue( buffer.NumSilentQueryDrops() < 1000, "only a few silent drops expected" );
		 }

		 private ThreadStart Stress<T>( int n, System.Action<long> action )
		 {
			  return () =>
			  {
				for ( long i = 0; i < n; i++ )
				{
					 action( i );
				}
			  };
		 }

		 private ThreadStart StressUntil<T>( System.Threading.CountdownEvent latch, System.Action<long> action )
		 {
			  return () =>
			  {
				long i = 0;
				while ( latch.CurrentCount != 0 )
				{
					 action( i++ );
				}
			  };
		 }

		 internal class UniqueElementsConsumer : System.Action<long>
		 {
			  internal MutableLongSet Values = LongSets.mutable.empty();

			  internal virtual void Reset()
			  {
					Values.clear();
			  }

			  public override void Accept( long? newValue )
			  {
					assertTrue( Values.add( newValue ), format( "Value %d was seen twice", newValue ) );
			  }
		 }
	}

}