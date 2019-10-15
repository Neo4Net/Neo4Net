using System;
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
namespace Neo4Net.Kernel.Impl.Api.index.sampling
{
	using Test = org.junit.Test;


	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using DoubleLatch = Neo4Net.Test.DoubleLatch;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createInitializedScheduler;

	public class IndexSamplingJobTrackerTest
	{
		 private readonly IndexSamplingConfig _config = mock( typeof( IndexSamplingConfig ) );
		 internal long IndexId11;
		 internal long IndexId12 = 1;
		 internal long IndexId22 = 2;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotRunASampleJobWhichIsAlreadyRunning() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotRunASampleJobWhichIsAlreadyRunning()
		 {
			  // given
			  when( _config.jobLimit() ).thenReturn(2);
			  IJobScheduler jobScheduler = createInitializedScheduler();
			  IndexSamplingJobTracker jobTracker = new IndexSamplingJobTracker( _config, jobScheduler );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.test.DoubleLatch latch = new org.neo4j.test.DoubleLatch();
			  DoubleLatch latch = new DoubleLatch();

			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger count = new java.util.concurrent.atomic.AtomicInteger(0);
			  AtomicInteger count = new AtomicInteger( 0 );

			  assertTrue( jobTracker.CanExecuteMoreSamplingJobs() );
			  IndexSamplingJob job = new IndexSamplingJobAnonymousInnerClass( this, latch, count );

			  jobTracker.ScheduleSamplingJob( job );
			  jobTracker.ScheduleSamplingJob( job );

			  latch.StartAndWaitForAllToStart();
			  latch.WaitForAllToFinish();

			  assertEquals( 1, count.get() );
		 }

		 private class IndexSamplingJobAnonymousInnerClass : IndexSamplingJob
		 {
			 private readonly IndexSamplingJobTrackerTest _outerInstance;

			 private DoubleLatch _latch;
			 private AtomicInteger _count;

			 public IndexSamplingJobAnonymousInnerClass( IndexSamplingJobTrackerTest outerInstance, DoubleLatch latch, AtomicInteger count )
			 {
				 this.outerInstance = outerInstance;
				 this._latch = latch;
				 this._count = count;
			 }

			 public void run()
			 {
				  _count.incrementAndGet();

				  _latch.waitForAllToStart();
				  _latch.finish();
			 }

			 public long indexId()
			 {
				  return _outerInstance.indexId12;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAcceptMoreJobsThanAllowed() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAcceptMoreJobsThanAllowed()
		 {
			  // given
			  when( _config.jobLimit() ).thenReturn(1);
			  IJobScheduler jobScheduler = createInitializedScheduler();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IndexSamplingJobTracker jobTracker = new IndexSamplingJobTracker(config, jobScheduler);
			  IndexSamplingJobTracker jobTracker = new IndexSamplingJobTracker( _config, jobScheduler );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.test.DoubleLatch latch = new org.neo4j.test.DoubleLatch();
			  DoubleLatch latch = new DoubleLatch();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.test.DoubleLatch waitingLatch = new org.neo4j.test.DoubleLatch();
			  DoubleLatch waitingLatch = new DoubleLatch();

			  // when
			  assertTrue( jobTracker.CanExecuteMoreSamplingJobs() );

			  jobTracker.ScheduleSamplingJob( new IndexSamplingJobAnonymousInnerClass2( this, latch ) );

			  // then
			  latch.WaitForAllToStart();

			  assertFalse( jobTracker.CanExecuteMoreSamplingJobs() );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean waiting = new java.util.concurrent.atomic.AtomicBoolean(false);
			  AtomicBoolean waiting = new AtomicBoolean( false );
			  (new Thread(() =>
			  {
				waiting.set( true );
				waitingLatch.StartAndWaitForAllToStart();
				jobTracker.WaitUntilCanExecuteMoreSamplingJobs();
				waiting.set( false );
				waitingLatch.Finish();
			  })).Start();

			  waitingLatch.WaitForAllToStart();

			  assertTrue( waiting.get() );

			  latch.Finish();

			  waitingLatch.WaitForAllToFinish();

			  assertFalse( waiting.get() );

			  // eventually we accept new jobs
			  while ( !jobTracker.CanExecuteMoreSamplingJobs() )
			  {
					Thread.yield();
			  }
		 }

		 private class IndexSamplingJobAnonymousInnerClass2 : IndexSamplingJob
		 {
			 private readonly IndexSamplingJobTrackerTest _outerInstance;

			 private DoubleLatch _latch;

			 public IndexSamplingJobAnonymousInnerClass2( IndexSamplingJobTrackerTest outerInstance, DoubleLatch latch )
			 {
				 this.outerInstance = outerInstance;
				 this._latch = latch;
			 }

			 public void run()
			 {
				  _latch.startAndWaitForAllToStart();
				  _latch.waitForAllToFinish();
			 }

			 public long indexId()
			 {
				  return _outerInstance.indexId12;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 5_000) public void shouldAcceptNewJobWhenRunningJobFinishes() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcceptNewJobWhenRunningJobFinishes()
		 {
			  // Given
			  when( _config.jobLimit() ).thenReturn(1);

			  IJobScheduler jobScheduler = createInitializedScheduler();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IndexSamplingJobTracker jobTracker = new IndexSamplingJobTracker(config, jobScheduler);
			  IndexSamplingJobTracker jobTracker = new IndexSamplingJobTracker( _config, jobScheduler );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.test.DoubleLatch latch = new org.neo4j.test.DoubleLatch();
			  DoubleLatch latch = new DoubleLatch();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean lastJobExecuted = new java.util.concurrent.atomic.AtomicBoolean();
			  AtomicBoolean lastJobExecuted = new AtomicBoolean();

			  jobTracker.ScheduleSamplingJob( new IndexSamplingJobAnonymousInnerClass3( this, latch ) );

			  // When
			  Executors.newSingleThreadExecutor().execute(() =>
			  {
				jobTracker.WaitUntilCanExecuteMoreSamplingJobs();
				jobTracker.ScheduleSamplingJob( new IndexSamplingJobAnonymousInnerClass4( this, latch, lastJobExecuted ) );
			  });

			  assertFalse( jobTracker.CanExecuteMoreSamplingJobs() );
			  latch.StartAndWaitForAllToStart();
			  latch.WaitForAllToFinish();

			  // Then
			  assertTrue( lastJobExecuted.get() );
		 }

		 private class IndexSamplingJobAnonymousInnerClass3 : IndexSamplingJob
		 {
			 private readonly IndexSamplingJobTrackerTest _outerInstance;

			 private DoubleLatch _latch;

			 public IndexSamplingJobAnonymousInnerClass3( IndexSamplingJobTrackerTest outerInstance, DoubleLatch latch )
			 {
				 this.outerInstance = outerInstance;
				 this._latch = latch;
			 }

			 public long indexId()
			 {
				  return _outerInstance.indexId11;
			 }

			 public void run()
			 {
				  _latch.waitForAllToStart();
			 }
		 }

		 private class IndexSamplingJobAnonymousInnerClass4 : IndexSamplingJob
		 {
			 private readonly IndexSamplingJobTrackerTest _outerInstance;

			 private DoubleLatch _latch;
			 private AtomicBoolean _lastJobExecuted;

			 public IndexSamplingJobAnonymousInnerClass4( IndexSamplingJobTrackerTest outerInstance, DoubleLatch latch, AtomicBoolean lastJobExecuted )
			 {
				 this.outerInstance = outerInstance;
				 this._latch = latch;
				 this._lastJobExecuted = lastJobExecuted;
			 }

			 public long indexId()
			 {
				  return _outerInstance.indexId22;
			 }

			 public void run()
			 {
				  _lastJobExecuted.set( true );
				  _latch.finish();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 5_000) public void shouldDoNothingWhenUsedAfterBeingStopped()
		 public virtual void ShouldDoNothingWhenUsedAfterBeingStopped()
		 {
			  // Given
			  IJobScheduler scheduler = mock( typeof( IJobScheduler ) );
			  IndexSamplingJobTracker jobTracker = new IndexSamplingJobTracker( _config, scheduler );
			  jobTracker.StopAndAwaitAllJobs();

			  // When
			  jobTracker.ScheduleSamplingJob( mock( typeof( IndexSamplingJob ) ) );

			  // Then
			  verifyZeroInteractions( scheduler );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 5_000) public void shouldNotAllowNewJobsAfterBeingStopped()
		 public virtual void ShouldNotAllowNewJobsAfterBeingStopped()
		 {
			  // Given
			  IndexSamplingJobTracker jobTracker = new IndexSamplingJobTracker( _config, mock( typeof( IJobScheduler ) ) );

			  // When
			  jobTracker.StopAndAwaitAllJobs();

			  // Then
			  assertFalse( jobTracker.CanExecuteMoreSamplingJobs() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 5_000) public void shouldStopAndWaitForAllJobsToFinish() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStopAndWaitForAllJobsToFinish()
		 {
			  // Given
			  when( _config.jobLimit() ).thenReturn(2);

			  IJobScheduler jobScheduler = createInitializedScheduler();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IndexSamplingJobTracker jobTracker = new IndexSamplingJobTracker(config, jobScheduler);
			  IndexSamplingJobTracker jobTracker = new IndexSamplingJobTracker( _config, jobScheduler );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch latch1 = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent latch1 = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch latch2 = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent latch2 = new System.Threading.CountdownEvent( 1 );

			  WaitingIndexSamplingJob job1 = new WaitingIndexSamplingJob( IndexId11, latch1 );
			  WaitingIndexSamplingJob job2 = new WaitingIndexSamplingJob( IndexId22, latch1 );

			  jobTracker.ScheduleSamplingJob( job1 );
			  jobTracker.ScheduleSamplingJob( job2 );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> stopping = java.util.concurrent.Executors.newSingleThreadExecutor().submit(() ->
			  Future<object> stopping = Executors.newSingleThreadExecutor().submit(() =>
			  {
				latch2.Signal();
				jobTracker.StopAndAwaitAllJobs();
			  });

			  // When
			  latch2.await();
			  assertFalse( stopping.Done );
			  latch1.Signal();
			  stopping.get( 10, SECONDS );

			  // Then
			  assertTrue( stopping.Done );
			  assertNull( stopping.get() );
			  assertTrue( job1.Executed );
			  assertTrue( job2.Executed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 5_000) public void shouldWaitForAllJobsToFinish() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWaitForAllJobsToFinish()
		 {
			  // Given
			  when( _config.jobLimit() ).thenReturn(2);

			  IJobScheduler jobScheduler = createInitializedScheduler();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IndexSamplingJobTracker jobTracker = new IndexSamplingJobTracker(config, jobScheduler);
			  IndexSamplingJobTracker jobTracker = new IndexSamplingJobTracker( _config, jobScheduler );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch latch1 = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent latch1 = new System.Threading.CountdownEvent( 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch latch2 = new java.util.concurrent.CountDownLatch(1);
			  System.Threading.CountdownEvent latch2 = new System.Threading.CountdownEvent( 1 );

			  WaitingIndexSamplingJob job1 = new WaitingIndexSamplingJob( IndexId11, latch1 );
			  WaitingIndexSamplingJob job2 = new WaitingIndexSamplingJob( IndexId22, latch1 );

			  jobTracker.ScheduleSamplingJob( job1 );
			  jobTracker.ScheduleSamplingJob( job2 );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> stopping = java.util.concurrent.Executors.newSingleThreadExecutor().submit(() ->
			  Future<object> stopping = Executors.newSingleThreadExecutor().submit(() =>
			  {
				latch2.Signal();
				try
				{
					 jobTracker.AwaitAllJobs( 10, TimeUnit.SECONDS );
				}
				catch ( InterruptedException e )
				{
					 throw new Exception( e );
				}
			  });

			  // When
			  latch2.await();
			  assertFalse( stopping.Done );
			  latch1.Signal();
			  stopping.get( 10, SECONDS );

			  // Then
			  assertTrue( stopping.Done );
			  assertNull( stopping.get() );
			  assertTrue( job1.Executed );
			  assertTrue( job2.Executed );
		 }

		 private class WaitingIndexSamplingJob : IndexSamplingJob
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly long IndexIdConflict;
			  internal readonly System.Threading.CountdownEvent Latch;

			  internal volatile bool Executed;

			  internal WaitingIndexSamplingJob( long indexId, System.Threading.CountdownEvent latch )
			  {
					this.IndexIdConflict = indexId;
					this.Latch = latch;
			  }

			  public override long IndexId()
			  {
					return IndexIdConflict;
			  }

			  public override void Run()
			  {
					try
					{
						 Latch.await();
						 Executed = true;
					}
					catch ( InterruptedException e )
					{
						 Thread.CurrentThread.Interrupt();
						 throw new Exception( e );
					}
			  }
		 }
	}

}