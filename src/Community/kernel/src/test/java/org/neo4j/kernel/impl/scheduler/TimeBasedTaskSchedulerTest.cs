using System;
using System.Collections.Generic;
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
namespace Neo4Net.Kernel.impl.scheduler
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using Group = Neo4Net.Scheduler.Group;
	using JobHandle = Neo4Net.Scheduler.JobHandle;
	using FakeClock = Neo4Net.Time.FakeClock;
	using BinaryLatch = Neo4Net.Util.concurrent.BinaryLatch;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class TimeBasedTaskSchedulerTest
	{
		 private FakeClock _clock;
		 private ThreadPoolManager _pools;
		 private TimeBasedTaskScheduler _scheduler;
		 private AtomicInteger _counter;
		 private Semaphore _semaphore;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _clock = new FakeClock();
			  _pools = new ThreadPoolManager( new ThreadGroup( "TestPool" ) );
			  _scheduler = new TimeBasedTaskScheduler( _clock, _pools );
			  _counter = new AtomicInteger();
			  _semaphore = new Semaphore( 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  InterruptedException exception = _pools.shutDownAll();
			  if ( exception != null )
			  {
					throw new Exception( "Test was interrupted?", exception );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertSemaphoreAcquire() throws InterruptedException
		 private void AssertSemaphoreAcquire()
		 {
			  // We do this in a loop, while calling tick after each iteration, because the task might have a previously
			  // start run that hasn't yet finished. And in that case, tick() won't start another. So we have to loop
			  // and call tick() until the task gets scheduled and releases our semaphore.
			  long timeoutMillis = TimeUnit.SECONDS.toMillis( 10 );
			  long sleepIntervalMillis = 10;
			  long iterations = timeoutMillis / sleepIntervalMillis;
			  for ( int i = 0; i < iterations; i++ )
			  {
					if ( _semaphore.tryAcquire( sleepIntervalMillis, TimeUnit.MILLISECONDS ) )
					{
						 return; // All good.
					}
					_scheduler.tick();
			  }
			  fail( "Semaphore acquire timeout" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustDelayExecution() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustDelayExecution()
		 {
			  JobHandle handle = _scheduler.submit( Group.STORAGE_MAINTENANCE, _counter.incrementAndGet, 100, 0 );
			  _scheduler.tick();
			  assertThat( _counter.get(), @is(0) );
			  _clock.forward( 99, TimeUnit.NANOSECONDS );
			  _scheduler.tick();
			  assertThat( _counter.get(), @is(0) );
			  _clock.forward( 1, TimeUnit.NANOSECONDS );
			  _scheduler.tick();
			  handle.WaitTermination();
			  assertThat( _counter.get(), @is(1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustOnlyScheduleTasksThatAreDue() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustOnlyScheduleTasksThatAreDue()
		 {
			  JobHandle handle1 = _scheduler.submit( Group.STORAGE_MAINTENANCE, () => _counter.addAndGet(10), 100, 0 );
			  JobHandle handle2 = _scheduler.submit( Group.STORAGE_MAINTENANCE, () => _counter.addAndGet(100), 200, 0 );
			  _scheduler.tick();
			  assertThat( _counter.get(), @is(0) );
			  _clock.forward( 199, TimeUnit.NANOSECONDS );
			  _scheduler.tick();
			  handle1.WaitTermination();
			  assertThat( _counter.get(), @is(10) );
			  _clock.forward( 1, TimeUnit.NANOSECONDS );
			  _scheduler.tick();
			  handle2.WaitTermination();
			  assertThat( _counter.get(), @is(110) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustNotRescheduleDelayedTasks() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustNotRescheduleDelayedTasks()
		 {
			  JobHandle handle = _scheduler.submit( Group.STORAGE_MAINTENANCE, _counter.incrementAndGet, 100, 0 );
			  _clock.forward( 100, TimeUnit.NANOSECONDS );
			  _scheduler.tick();
			  handle.WaitTermination();
			  assertThat( _counter.get(), @is(1) );
			  _clock.forward( 100, TimeUnit.NANOSECONDS );
			  _scheduler.tick();
			  handle.WaitTermination();
			  _pools.getThreadPool( Group.STORAGE_MAINTENANCE ).shutDown();
			  assertThat( _counter.get(), @is(1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustRescheduleRecurringTasks() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustRescheduleRecurringTasks()
		 {
			  _scheduler.submit( Group.STORAGE_MAINTENANCE, _semaphore.release, 100, 100 );
			  _clock.forward( 100, TimeUnit.NANOSECONDS );
			  _scheduler.tick();
			  AssertSemaphoreAcquire();
			  _clock.forward( 100, TimeUnit.NANOSECONDS );
			  _scheduler.tick();
			  AssertSemaphoreAcquire();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustNotRescheduleRecurringTasksThatThrows() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustNotRescheduleRecurringTasksThatThrows()
		 {
			  ThreadStart runnable = () =>
			  {
				_semaphore.release();
				throw new Exception( "boom" );
			  };
			  JobHandle handle = _scheduler.submit( Group.STORAGE_MAINTENANCE, runnable, 100, 100 );
			  _clock.forward( 100, TimeUnit.NANOSECONDS );
			  _scheduler.tick();
			  AssertSemaphoreAcquire();
			  _clock.forward( 100, TimeUnit.NANOSECONDS );
			  _scheduler.tick();
			  try
			  {
					handle.WaitTermination();
					fail( "waitTermination should have thrown because the task should have failed." );
			  }
			  catch ( ExecutionException e )
			  {
					assertThat( e.InnerException.Message, @is( "boom" ) );
			  }
			  assertThat( _semaphore.drainPermits(), @is(0) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustNotStartRecurringTasksWherePriorExecutionHasNotYetFinished()
		 public virtual void MustNotStartRecurringTasksWherePriorExecutionHasNotYetFinished()
		 {
			  ThreadStart runnable = () =>
			  {
				_counter.incrementAndGet();
				_semaphore.acquireUninterruptibly();
			  };
			  _scheduler.submit( Group.STORAGE_MAINTENANCE, runnable, 100, 100 );
			  for ( int i = 0; i < 4; i++ )
			  {
					_scheduler.tick();
					_clock.forward( 100, TimeUnit.NANOSECONDS );
			  }
			  _semaphore.release( int.MaxValue );
			  _pools.getThreadPool( Group.STORAGE_MAINTENANCE ).shutDown();
			  assertThat( _counter.get(), @is(1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void longRunningTasksMustNotDelayExecutionOfOtherTasks() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LongRunningTasksMustNotDelayExecutionOfOtherTasks()
		 {
			  BinaryLatch latch = new BinaryLatch();
			  ThreadStart longRunning = latch.await;
			  ThreadStart shortRunning = _semaphore.release;
			  _scheduler.submit( Group.STORAGE_MAINTENANCE, longRunning, 100, 100 );
			  _scheduler.submit( Group.STORAGE_MAINTENANCE, shortRunning, 100, 100 );
			  for ( int i = 0; i < 4; i++ )
			  {
					_clock.forward( 100, TimeUnit.NANOSECONDS );
					_scheduler.tick();
					AssertSemaphoreAcquire();
			  }
			  latch.Release();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void delayedTasksMustNotRunIfCancelledFirst() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DelayedTasksMustNotRunIfCancelledFirst()
		 {
			  IList<bool> cancelListener = new List<bool>();
			  JobHandle handle = _scheduler.submit( Group.STORAGE_MAINTENANCE, _counter.incrementAndGet, 100, 0 );
			  handle.RegisterCancelListener( cancelListener.add );
			  _clock.forward( 90, TimeUnit.NANOSECONDS );
			  _scheduler.tick();
			  handle.Cancel( false );
			  _clock.forward( 10, TimeUnit.NANOSECONDS );
			  _scheduler.tick();
			  _pools.getThreadPool( Group.STORAGE_MAINTENANCE ).shutDown();
			  assertThat( _counter.get(), @is(0) );
			  assertThat( cancelListener, contains( false ) );
			  try
			  {
					handle.WaitTermination();
					fail( "waitTermination should have thrown a CancellationException." );
			  }
			  catch ( CancellationException )
			  {
					// Good stuff.
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void recurringTasksMustStopWhenCancelled() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RecurringTasksMustStopWhenCancelled()
		 {
			  IList<bool> cancelListener = new List<bool>();
			  ThreadStart recurring = () =>
			  {
				_counter.incrementAndGet();
				_semaphore.release();
			  };
			  JobHandle handle = _scheduler.submit( Group.STORAGE_MAINTENANCE, recurring, 100, 100 );
			  handle.RegisterCancelListener( cancelListener.add );
			  _clock.forward( 100, TimeUnit.NANOSECONDS );
			  _scheduler.tick();
			  AssertSemaphoreAcquire();
			  _clock.forward( 100, TimeUnit.NANOSECONDS );
			  _scheduler.tick();
			  AssertSemaphoreAcquire();
			  handle.Cancel( true );
			  _clock.forward( 100, TimeUnit.NANOSECONDS );
			  _scheduler.tick();
			  _clock.forward( 100, TimeUnit.NANOSECONDS );
			  _scheduler.tick();
			  _pools.getThreadPool( Group.STORAGE_MAINTENANCE ).shutDown();
			  assertThat( _counter.get(), @is(2) );
			  assertThat( cancelListener, contains( true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void overdueRecurringTasksMustStartAsSoonAsPossible()
		 public virtual void OverdueRecurringTasksMustStartAsSoonAsPossible()
		 {
			  ThreadStart recurring = () =>
			  {
				_counter.incrementAndGet();
				_semaphore.acquireUninterruptibly();
			  };
			  JobHandle handle = _scheduler.submit( Group.STORAGE_MAINTENANCE, recurring, 100, 100 );
			  _clock.forward( 100, TimeUnit.NANOSECONDS );
			  _scheduler.tick();
			  while ( _counter.get() < 1 )
			  {
					// Spin.
					Thread.yield();
			  }
			  _clock.forward( 100, TimeUnit.NANOSECONDS );
			  _scheduler.tick();
			  _clock.forward( 100, TimeUnit.NANOSECONDS );
			  _semaphore.release();
			  _scheduler.tick();
			  long deadline = System.nanoTime() + TimeUnit.SECONDS.toNanos(10);
			  while ( _counter.get() < 2 && System.nanoTime() < deadline )
			  {
					_scheduler.tick();
					Thread.yield();
			  }
			  assertThat( _counter.get(), @is(2) );
			  _semaphore.release( int.MaxValue );
			  handle.Cancel( false );
		 }
	}

}