using System;
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
namespace Neo4Net.Kernel.impl.scheduler
{
	using After = org.junit.After;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Group = Neo4Net.Scheduler.Group;
	using JobHandle = Neo4Net.Scheduler.JobHandle;
	using BinaryLatch = Neo4Net.Utils.Concurrent.BinaryLatch;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Thread.sleep;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.both;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class CentralJobSchedulerTest
	{
		private bool InstanceFieldsInitialized = false;

		public CentralJobSchedulerTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_scheduler = _life.add( new CentralJobScheduler() );
			_countInvocationsJob = _invocations.incrementAndGet;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public readonly ExpectedException ExpectedException = ExpectedException.none();

		 private readonly AtomicInteger _invocations = new AtomicInteger();
		 private readonly LifeSupport _life = new LifeSupport();
		 private CentralJobScheduler _scheduler;

		 private ThreadStart _countInvocationsJob;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void stopScheduler()
		 public virtual void StopScheduler()
		 {
			  _life.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void taskSchedulerGroupMustNotBeDirectlySchedulable()
		 public virtual void TaskSchedulerGroupMustNotBeDirectlySchedulable()
		 {
			  _life.start();
			  ExpectedException.expect( typeof( System.ArgumentException ) );
			  _scheduler.schedule( Group.TASK_SCHEDULER, () => fail("This task should not have been executed.") );
		 }

		 // Tests schedules a recurring job to run 5 times with 100ms in between.
		 // The timeout of 10s should be enough.
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 10_000) public void shouldRunRecurringJob() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRunRecurringJob()
		 {
			  // Given
			  long period = 10;
			  int count = 5;
			  _life.start();

			  // When
			  _scheduler.scheduleRecurring( Group.INDEX_POPULATION, _countInvocationsJob, period, MILLISECONDS );
			  AwaitInvocationCount( count );
			  _scheduler.shutdown();

			  // Then assert that the recurring job was stopped (when the scheduler was shut down)
			  int actualInvocations = _invocations.get();
			  sleep( period * 5 );
			  assertThat( _invocations.get(), equalTo(actualInvocations) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCancelRecurringJob() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCancelRecurringJob()
		 {
			  // Given
			  long period = 2;
			  _life.start();
			  JobHandle jobHandle = _scheduler.scheduleRecurring( Group.INDEX_POPULATION, _countInvocationsJob, period, MILLISECONDS );
			  AwaitFirstInvocation();

			  // When
			  jobHandle.Cancel( false );

			  try
			  {
					jobHandle.WaitTermination();
					fail( "Task should be terminated" );
			  }
			  catch ( CancellationException )
			  {
					// task should be canceled
			  }

			  // Then
			  int recorded = _invocations.get();
			  sleep( period * 100 );
			  // we can have task that is already running during cancellation so lets count it as well
			  assertThat( _invocations.get(), both(greaterThanOrEqualTo(recorded)).and(lessThanOrEqualTo(recorded + 1)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRunWithDelay() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRunWithDelay()
		 {
			  // Given
			  _life.start();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicLong runTime = new java.util.concurrent.atomic.AtomicLong();
			  AtomicLong runTime = new AtomicLong();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch latch = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( 1 );

			  long time = System.nanoTime();

			  _scheduler.schedule(Group.INDEX_POPULATION, () =>
			  {
				runTime.set( System.nanoTime() );
				latch.Signal();
			  }, 100, TimeUnit.MILLISECONDS);

			  latch.await();

			  assertTrue( time + TimeUnit.MILLISECONDS.toNanos( 100 ) <= runTime.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void longRunningScheduledJobsMustNotDelayOtherLongRunningJobs()
		 public virtual void LongRunningScheduledJobsMustNotDelayOtherLongRunningJobs()
		 {
			  _life.start();

			  IList<JobHandle> handles = new List<JobHandle>( 30 );
			  AtomicLong startedCounter = new AtomicLong();
			  BinaryLatch blockLatch = new BinaryLatch();
			  ThreadStart task = () =>
			  {
				startedCounter.incrementAndGet();
				blockLatch.Await();
			  };

			  for ( int i = 0; i < 10; i++ )
			  {
					handles.Add( _scheduler.schedule( Group.INDEX_POPULATION, task, 0, TimeUnit.MILLISECONDS ) );
			  }
			  for ( int i = 0; i < 10; i++ )
			  {
					handles.Add( _scheduler.scheduleRecurring( Group.INDEX_POPULATION, task, int.MaxValue, TimeUnit.MILLISECONDS ) );
			  }
			  for ( int i = 0; i < 10; i++ )
			  {
					handles.Add( _scheduler.scheduleRecurring( Group.INDEX_POPULATION, task, 0, int.MaxValue, TimeUnit.MILLISECONDS ) );
			  }

			  long deadline = TimeUnit.SECONDS.toNanos( 10 ) + System.nanoTime();
			  do
			  {
					if ( startedCounter.get() == handles.Count )
					{
						 // All jobs got started. We're good!
						 blockLatch.Release();
						 foreach ( JobHandle handle in handles )
						 {
							  handle.Cancel( false );
						 }
						 return;
					}
			  } while ( System.nanoTime() < deadline );
			  fail( "Only managed to start " + startedCounter.get() + " tasks in 10 seconds, when " + handles.Count + " was expected." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotifyCancelListeners()
		 public virtual void ShouldNotifyCancelListeners()
		 {
			  // GIVEN
			  CentralJobScheduler centralJobScheduler = new CentralJobScheduler();
			  centralJobScheduler.Init();

			  // WHEN
			  AtomicBoolean halted = new AtomicBoolean();
			  ThreadStart job = () =>
			  {
				while ( !halted.get() )
				{
					 LockSupport.parkNanos( MILLISECONDS.toNanos( 10 ) );
				}
			  };
			  JobHandle handle = centralJobScheduler.Schedule( Group.INDEX_POPULATION, job );
			  handle.RegisterCancelListener( mayBeInterrupted => halted.set( true ) );
			  handle.Cancel( false );

			  // THEN
			  assertTrue( halted.get() );
			  centralJobScheduler.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 10_000) public void waitTerminationOnDelayedJobMustWaitUntilJobCompletion() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void WaitTerminationOnDelayedJobMustWaitUntilJobCompletion()
		 {
			  CentralJobScheduler scheduler = new CentralJobScheduler();
			  scheduler.Init();

			  AtomicBoolean triggered = new AtomicBoolean();
			  ThreadStart job = () =>
			  {
				LockSupport.parkNanos( TimeUnit.MILLISECONDS.toNanos( 10 ) );
				triggered.set( true );
			  };

			  JobHandle handle = scheduler.Schedule( Group.INDEX_POPULATION, job, 10, TimeUnit.MILLISECONDS );

			  handle.WaitTermination();
			  assertTrue( triggered.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 10_000) public void scheduledTasksThatThrowsMustPropagateException() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ScheduledTasksThatThrowsMustPropagateException()
		 {
			  CentralJobScheduler scheduler = new CentralJobScheduler();
			  scheduler.Init();

			  Exception boom = new Exception( "boom" );
			  AtomicInteger triggerCounter = new AtomicInteger();
			  ThreadStart job = () =>
			  {
				triggerCounter.incrementAndGet();
				throw boom;
			  };

			  JobHandle handle = scheduler.ScheduleRecurring( Group.INDEX_POPULATION, job, 1, TimeUnit.MILLISECONDS );
			  try
			  {
					handle.WaitTermination();
					fail( "waitTermination should have failed." );
			  }
			  catch ( ExecutionException e )
			  {
					assertThat( e.InnerException, @is( boom ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 10_000) public void scheduledTasksThatThrowsShouldStop() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ScheduledTasksThatThrowsShouldStop()
		 {
			  CentralJobScheduler scheduler = new CentralJobScheduler();
			  scheduler.Init();

			  BinaryLatch triggerLatch = new BinaryLatch();
			  Exception boom = new Exception( "boom" );
			  AtomicInteger triggerCounter = new AtomicInteger();
			  ThreadStart job = () =>
			  {
				triggerCounter.incrementAndGet();
				triggerLatch.Release();
				throw boom;
			  };

			  scheduler.ScheduleRecurring( Group.INDEX_POPULATION, job, 1, TimeUnit.MILLISECONDS );

			  triggerLatch.Await();
			  Thread.Sleep( 50 );

			  assertThat( triggerCounter.get(), @is(1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 10_000) public void shutDownMustKillCancelledJobs()
		 public virtual void ShutDownMustKillCancelledJobs()
		 {
			  CentralJobScheduler scheduler = new CentralJobScheduler();
			  scheduler.Init();

			  BinaryLatch startLatch = new BinaryLatch();
			  BinaryLatch stopLatch = new BinaryLatch();
			  scheduler.Schedule(Group.INDEX_POPULATION, () =>
			  {
				try
				{
					 startLatch.Release();
					 Thread.Sleep( 100_000 );
				}
				catch ( InterruptedException e )
				{
					 stopLatch.Release();
					 throw new Exception( e );
				}
			  });
			  startLatch.Await();
			  scheduler.Shutdown();
			  stopLatch.Await();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void awaitFirstInvocation() throws InterruptedException
		 private void AwaitFirstInvocation()
		 {
			  AwaitInvocationCount( 1 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void awaitInvocationCount(int count) throws InterruptedException
		 private void AwaitInvocationCount( int count )
		 {
			  while ( _invocations.get() < count )
			  {
					Thread.Sleep( 10 );
			  }
		 }
	}

}