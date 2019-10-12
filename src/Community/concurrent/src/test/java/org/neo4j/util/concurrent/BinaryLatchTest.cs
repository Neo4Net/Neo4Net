using System.Threading;

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
namespace Neo4Net.Util.concurrent
{
	using AfterAll = org.junit.jupiter.api.AfterAll;
	using Test = org.junit.jupiter.api.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTimeout;

	internal class BinaryLatchTest
	{
		 private static readonly ExecutorService _executor = Executors.newCachedThreadPool();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterAll static void shutDownExecutor()
		 internal static void ShutDownExecutor()
		 {
			  _executor.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void releaseThenAwaitDoesNotBlock()
		 internal virtual void ReleaseThenAwaitDoesNotBlock()
		 {
			  assertTimeout(ofSeconds(3), () =>
			  {
				BinaryLatch latch = new BinaryLatch();
				latch.Release();
				latch.Await();
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void releaseMustUnblockAwaiters()
		 internal virtual void ReleaseMustUnblockAwaiters()
		 {
			  assertTimeout(ofSeconds(10), () =>
			  {
				BinaryLatch latch = new BinaryLatch();
				ThreadStart awaiter = latch.await;
				int awaiters = 24;
				Future<object>[] futures = new Future<object>[awaiters];
				for ( int i = 0; i < awaiters; i++ )
				{
					 futures[i] = _executor.submit( awaiter );
				}

				assertThrows( typeof( TimeoutException ), () => futures[0].get(10, TimeUnit.MILLISECONDS) );

				latch.Release();

				foreach ( Future<object> future in futures )
				{
					 future.get();
				}
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void stressLatch()
		 internal virtual void StressLatch()
		 {
			  assertTimeout(ofSeconds(60), () =>
			  {
				AtomicReference<BinaryLatch> latchRef = new AtomicReference<BinaryLatch>( new BinaryLatch() );
				ThreadStart awaiter = () =>
				{
					 BinaryLatch latch;
					 while ( ( latch = latchRef.get() ) != null )
					 {
						  latch.Await();
					 }
				};

				int awaiters = 6;
				Future<object>[] futures = new Future<object>[awaiters];
				for ( int i = 0; i < awaiters; i++ )
				{
					 futures[i] = _executor.submit( awaiter );
				}

				ThreadLocalRandom rng = ThreadLocalRandom.current();
				for ( int i = 0; i < 500000; i++ )
				{
					 latchRef.getAndSet( new BinaryLatch() ).release();
					 Spin( rng.nextLong( 0, 10 ) );
				}

				latchRef.getAndSet( null ).release();

				// None of the tasks we started should get stuck, e.g. miss a release signal:
				foreach ( Future<object> future in futures )
				{
					 future.get();
				}
			  });
		 }

		 private static void Spin( long micros )
		 {
			  if ( micros == 0 )
			  {
					return;
			  }

			  long now;
			  long deadline = System.nanoTime() + TimeUnit.MICROSECONDS.toNanos(micros);
			  do
			  {
					now = System.nanoTime();
			  } while ( now < deadline );
		 }
	}

}