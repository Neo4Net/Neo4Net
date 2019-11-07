using System.Collections.Generic;
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
namespace Neo4Net.Bolt.runtime
{
	using After = org.junit.After;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Predicates = Neo4Net.Functions.Predicates;
	using NullLog = Neo4Net.Logging.NullLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.runners.Parameterized.Parameter;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.runners.Parameterized.Parameters;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.runtime.CachedThreadPoolExecutorFactory.SYNCHRONOUS_QUEUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.bolt.runtime.CachedThreadPoolExecutorFactory.UNBOUNDED_QUEUE;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class CachedThreadPoolExecutorFactoryTest
	public class CachedThreadPoolExecutorFactoryTest
	{
		 private const int TEST_BOUNDED_QUEUE_SIZE = 5;

		 private readonly ExecutorFactory _factory = new CachedThreadPoolExecutorFactory( NullLog.Instance );
		 private ExecutorService _executorService;

		 [Parameter(0)]
		 public int QueueSize;

		 [Parameter(1)]
		 public string Name;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters(name = "{1}") public static java.util.List<Object[]> parameters()
		 public static IList<object[]> Parameters()
		 {
			  return Arrays.asList( new object[]{ UNBOUNDED_QUEUE, "Unbounded Queue" }, new object[]{ SYNCHRONOUS_QUEUE, "Synchronous Queue" }, new object[]{ TEST_BOUNDED_QUEUE_SIZE, "Bounded Queue" } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanup()
		 public virtual void Cleanup()
		 {
			  if ( _executorService != null && !_executorService.Terminated )
			  {
					_executorService.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createShouldAssignCorrectQueue()
		 public virtual void CreateShouldAssignCorrectQueue()
		 {
			  _executorService = _factory.create( 0, 1, Duration.ZERO, QueueSize, false, NewThreadFactory() );

			  if ( _executorService is ThreadPoolExecutor )
			  {
					BlockingQueue<ThreadStart> queue = ( ( ThreadPoolExecutor ) _executorService ).Queue;

					switch ( QueueSize )
					{
					case UNBOUNDED_QUEUE:
						 assertThat( queue, instanceOf( typeof( LinkedBlockingQueue ) ) );
						 assertEquals( int.MaxValue, queue.remainingCapacity() );
						 break;
					case SYNCHRONOUS_QUEUE:
						 assertThat( queue, instanceOf( typeof( SynchronousQueue ) ) );
						 break;
					case TEST_BOUNDED_QUEUE_SIZE:
						 assertThat( queue, instanceOf( typeof( ArrayBlockingQueue ) ) );
						 assertEquals( QueueSize, queue.remainingCapacity() );
						 break;
					default:
						 fail( string.Format( "Unexpected queue size {0:D}", QueueSize ) );
					 break;
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createShouldCreateExecutor()
		 public virtual void CreateShouldCreateExecutor()
		 {
			  _executorService = _factory.create( 0, 1, Duration.ZERO, QueueSize, false, NewThreadFactory() );

			  assertNotNull( _executorService );
			  assertFalse( _executorService.Shutdown );
			  assertFalse( _executorService.Terminated );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createShouldNotCreateExecutorWhenCorePoolSizeIsNegative()
		 public virtual void CreateShouldNotCreateExecutorWhenCorePoolSizeIsNegative()
		 {
			  try
			  {
					_factory.create( -1, 10, Duration.ZERO, 0, false, NewThreadFactory() );
					fail( "should throw exception" );
			  }
			  catch ( System.ArgumentException )
			  {
					// expected
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createShouldNotCreateExecutorWhenMaxPoolSizeIsNegative()
		 public virtual void CreateShouldNotCreateExecutorWhenMaxPoolSizeIsNegative()
		 {
			  try
			  {
					_factory.create( 0, -1, Duration.ZERO, 0, false, NewThreadFactory() );
					fail( "should throw exception" );
			  }
			  catch ( System.ArgumentException )
			  {
					// expected
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createShouldNotCreateExecutorWhenMaxPoolSizeIsZero()
		 public virtual void CreateShouldNotCreateExecutorWhenMaxPoolSizeIsZero()
		 {
			  try
			  {
					_factory.create( 0, 0, Duration.ZERO, 0, false, NewThreadFactory() );
					fail( "should throw exception" );
			  }
			  catch ( System.ArgumentException )
			  {
					// expected
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createShouldStartCoreThreadsIfAsked()
		 public virtual void CreateShouldStartCoreThreadsIfAsked()
		 {
			  AtomicInteger threadCounter = new AtomicInteger();

			  _factory.create( 5, 10, Duration.ZERO, 0, true, NewThreadFactoryWithCounter( threadCounter ) );

			  assertEquals( 5, threadCounter.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createShouldNotStartCoreThreadsIfNotAsked()
		 public virtual void CreateShouldNotStartCoreThreadsIfNotAsked()
		 {
			  AtomicInteger threadCounter = new AtomicInteger();

			  _factory.create( 5, 10, Duration.ZERO, 0, false, NewThreadFactoryWithCounter( threadCounter ) );

			  assertEquals( 0, threadCounter.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createShouldNotCreateExecutorWhenMaxPoolSizeIsLessThanCorePoolSize()
		 public virtual void CreateShouldNotCreateExecutorWhenMaxPoolSizeIsLessThanCorePoolSize()
		 {
			  try
			  {
					_factory.create( 10, 5, Duration.ZERO, 0, false, NewThreadFactory() );
					fail( "should throw exception" );
			  }
			  catch ( System.ArgumentException )
			  {
					// expected
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createdExecutorShouldExecuteSubmittedTasks() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreatedExecutorShouldExecuteSubmittedTasks()
		 {
			  AtomicBoolean exitCondition = new AtomicBoolean( false );
			  AtomicInteger threadCounter = new AtomicInteger( 0 );

			  _executorService = _factory.create( 0, 1, Duration.ZERO, 0, false, NewThreadFactoryWithCounter( threadCounter ) );

			  assertNotNull( _executorService );
			  assertEquals( 0, threadCounter.get() );

			  Future task1 = _executorService.submit( NewInfiniteWaitingRunnable( exitCondition ) );
			  assertEquals( 1, threadCounter.get() );

			  exitCondition.set( true );

			  assertNull( task1.get( 1, MINUTES ) );
			  assertTrue( task1.Done );
			  assertFalse( task1.Cancelled );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createdExecutorShouldFavorPoolSizes()
		 public virtual void CreatedExecutorShouldFavorPoolSizes()
		 {
			  AtomicBoolean exitCondition = new AtomicBoolean( false );
			  AtomicInteger threadCounter = new AtomicInteger( 0 );

			  _executorService = _factory.create( 0, 5, Duration.ZERO, 0, false, NewThreadFactoryWithCounter( threadCounter ) );

			  assertNotNull( _executorService );
			  assertEquals( 0, threadCounter.get() );

			  try
			  {
					for ( int i = 0; i < 6; i++ )
					{
						 _executorService.submit( NewInfiniteWaitingRunnable( exitCondition ) );
					}

					fail( "should throw exception" );
			  }
			  catch ( RejectedExecutionException )
			  {
					// expected
			  }

			  assertEquals( 5, threadCounter.get() );
		 }

		 private static ThreadStart NewInfiniteWaitingRunnable( AtomicBoolean exitCondition )
		 {
			  return () => Predicates.awaitForever(() => Thread.CurrentThread.Interrupted || exitCondition.get(), 500, MILLISECONDS);
		 }

		 private static ThreadFactory NewThreadFactory()
		 {
			  return Executors.defaultThreadFactory();
		 }

		 private static ThreadFactory NewThreadFactoryWithCounter( AtomicInteger counter )
		 {
			  return job =>
			  {
				counter.incrementAndGet();
				return Executors.defaultThreadFactory().newThread(job);
			  };
		 }
	}

}