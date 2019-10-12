using System;
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
namespace Neo4Net.Kernel.impl.transaction.log.checkpoint
{
	using BooleanPredicate = org.eclipse.collections.api.block.predicate.primitive.BooleanPredicate;
	using AfterClass = org.junit.AfterClass;
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;


	using Iterators = Neo4Net.Helpers.Collection.Iterators;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using UnderlyingStorageException = Neo4Net.Kernel.impl.store.UnderlyingStorageException;
	using DatabaseHealth = Neo4Net.Kernel.@internal.DatabaseHealth;
	using Group = Neo4Net.Scheduler.Group;
	using DoubleLatch = Neo4Net.Test.DoubleLatch;
	using OnDemandJobScheduler = Neo4Net.Test.OnDemandJobScheduler;
	using Neo4Net.Test;
	using Neo4Net.Test.OtherThreadExecutor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class CheckPointSchedulerTest
	{
		 private readonly IOLimiter _ioLimiter = mock( typeof( IOLimiter ) );
		 private readonly CheckPointer _checkPointer = mock( typeof( CheckPointer ) );
		 private readonly OnDemandJobScheduler _jobScheduler = spy( new OnDemandJobScheduler() );
		 private readonly DatabaseHealth _health = mock( typeof( DatabaseHealth ) );

		 private static ExecutorService _executor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setUpExecutor()
		 public static void SetUpExecutor()
		 {
			  _executor = Executors.newCachedThreadPool();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void tearDownExecutor() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public static void TearDownExecutor()
		 {
			  _executor.shutdown();
			  _executor.awaitTermination( 30, TimeUnit.SECONDS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldScheduleTheCheckPointerJobOnStart()
		 public virtual void ShouldScheduleTheCheckPointerJobOnStart()
		 {
			  // given
			  CheckPointScheduler scheduler = new CheckPointScheduler( _checkPointer, _ioLimiter, _jobScheduler, 20L, _health );

			  assertNull( _jobScheduler.Job );

			  // when
			  scheduler.Start();

			  // then
			  assertNotNull( _jobScheduler.Job );
			  verify( _jobScheduler, times( 1 ) ).schedule( eq( Group.CHECKPOINT ), any( typeof( ThreadStart ) ), eq( 20L ), eq( TimeUnit.MILLISECONDS ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRescheduleTheJobAfterARun() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRescheduleTheJobAfterARun()
		 {
			  // given
			  CheckPointScheduler scheduler = new CheckPointScheduler( _checkPointer, _ioLimiter, _jobScheduler, 20L, _health );

			  assertNull( _jobScheduler.Job );

			  scheduler.Start();

			  ThreadStart scheduledJob = _jobScheduler.Job;
			  assertNotNull( scheduledJob );

			  // when
			  _jobScheduler.runJob();

			  // then
			  verify( _jobScheduler, times( 2 ) ).schedule( eq( Group.CHECKPOINT ), any( typeof( ThreadStart ) ), eq( 20L ), eq( TimeUnit.MILLISECONDS ) );
			  verify( _checkPointer, times( 1 ) ).checkPointIfNeeded( any( typeof( TriggerInfo ) ) );
			  assertEquals( scheduledJob, _jobScheduler.Job );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotRescheduleAJobWhenStopped()
		 public virtual void ShouldNotRescheduleAJobWhenStopped()
		 {
			  // given
			  CheckPointScheduler scheduler = new CheckPointScheduler( _checkPointer, _ioLimiter, _jobScheduler, 20L, _health );

			  assertNull( _jobScheduler.Job );

			  scheduler.Start();

			  assertNotNull( _jobScheduler.Job );

			  // when
			  scheduler.Stop();

			  // then
			  assertNull( _jobScheduler.Job );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void stoppedJobCantBeInvoked() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StoppedJobCantBeInvoked()
		 {
			  CheckPointScheduler scheduler = new CheckPointScheduler( _checkPointer, _ioLimiter, _jobScheduler, 10L, _health );
			  scheduler.Start();
			  _jobScheduler.runJob();

			  // verify checkpoint was triggered
			  verify( _checkPointer ).checkPointIfNeeded( any( typeof( TriggerInfo ) ) );

			  // simulate scheduled run that was triggered just before stop
			  scheduler.Stop();
			  scheduler.Start();
			  _jobScheduler.runJob();

			  // checkpointer should not be invoked now because job stopped
			  verifyNoMoreInteractions( _checkPointer );
		 }

		 // Timeout as fallback safety if test deadlocks
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 60_000) public void shouldWaitOnStopUntilTheRunningCheckpointIsDone() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWaitOnStopUntilTheRunningCheckpointIsDone()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicReference<Throwable> ex = new java.util.concurrent.atomic.AtomicReference<>();
			  AtomicReference<Exception> ex = new AtomicReference<Exception>();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean stoppedCompleted = new java.util.concurrent.atomic.AtomicBoolean();
			  AtomicBoolean stoppedCompleted = new AtomicBoolean();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.test.DoubleLatch checkPointerLatch = new org.neo4j.test.DoubleLatch(1);
			  DoubleLatch checkPointerLatch = new DoubleLatch( 1 );
			  OtherThreadExecutor<Void> otherThreadExecutor = new OtherThreadExecutor<Void>( "scheduler stopper", null );
			  CheckPointer checkPointer = new CheckPointerAnonymousInnerClass( this, checkPointerLatch );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final CheckPointScheduler scheduler = new CheckPointScheduler(checkPointer, ioLimiter, jobScheduler, 20L, health);
			  CheckPointScheduler scheduler = new CheckPointScheduler( checkPointer, _ioLimiter, _jobScheduler, 20L, _health );

			  // when
			  scheduler.Start();

			  Thread runCheckPointer = new Thread( _jobScheduler.runJob );
			  runCheckPointer.Start();

			  checkPointerLatch.WaitForAllToStart();

			  otherThreadExecutor.ExecuteDontWait((OtherThreadExecutor.WorkerCommand<Void, Void>) state =>
			  {
				try
				{
					 scheduler.Stop();
					 stoppedCompleted.set( true );
				}
				catch ( Exception throwable )
				{
					 ex.set( throwable );
				}
				return null;
			  });
			  otherThreadExecutor.WaitUntilWaiting( details => details.isAt( typeof( CheckPointScheduler ), "waitOngoingCheckpointCompletion" ) );

			  // then
			  assertFalse( stoppedCompleted.get() );

			  checkPointerLatch.Finish();
			  runCheckPointer.Join();

			  while ( !stoppedCompleted.get() )
			  {
					Thread.Sleep( 1 );
			  }
			  otherThreadExecutor.Dispose();

			  assertNull( ex.get() );
		 }

		 private class CheckPointerAnonymousInnerClass : CheckPointer
		 {
			 private readonly CheckPointSchedulerTest _outerInstance;

			 private DoubleLatch _checkPointerLatch;

			 public CheckPointerAnonymousInnerClass( CheckPointSchedulerTest outerInstance, DoubleLatch checkPointerLatch )
			 {
				 this.outerInstance = outerInstance;
				 this._checkPointerLatch = checkPointerLatch;
			 }

			 public long checkPointIfNeeded( TriggerInfo triggerInfo )
			 {
				  _checkPointerLatch.startAndWaitForAllToStart();
				  _checkPointerLatch.waitForAllToFinish();
				  return 42;
			 }

			 public long tryCheckPoint( TriggerInfo triggerInfo )
			 {
				  throw new Exception( "this should have not been called" );
			 }

			 public long tryCheckPoint( TriggerInfo triggerInfo, System.Func<bool> timeout )
			 {
				  throw new Exception( "this should have not been called" );
			 }

			 public long forceCheckPoint( TriggerInfo triggerInfo )
			 {
				  throw new Exception( "this should have not been called" );
			 }

			 public long lastCheckPointedTransactionId()
			 {
				  return 42;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldContinueThroughSporadicFailures()
		 public virtual void ShouldContinueThroughSporadicFailures()
		 {
			  // GIVEN
			  ControlledCheckPointer checkPointer = new ControlledCheckPointer();
			  CheckPointScheduler scheduler = new CheckPointScheduler( checkPointer, _ioLimiter, _jobScheduler, 1, _health );
			  scheduler.Start();

			  // WHEN/THEN
			  for ( int i = 0; i < CheckPointScheduler.MaxConsecutiveFailuresTolerance * 2; i++ )
			  {
					// Fail
					checkPointer.Fail = true;
					_jobScheduler.runJob();
					verifyZeroInteractions( _health );

					// Succeed
					checkPointer.Fail = false;
					_jobScheduler.runJob();
					verifyZeroInteractions( _health );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 10_000) public void checkpointOnStopShouldFlushAsFastAsPossible() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CheckpointOnStopShouldFlushAsFastAsPossible()
		 {
			  CheckableIOLimiter ioLimiter = new CheckableIOLimiter();
			  System.Threading.CountdownEvent checkPointerLatch = new System.Threading.CountdownEvent( 1 );
			  WaitUnlimitedCheckPointer checkPointer = new WaitUnlimitedCheckPointer( ioLimiter, checkPointerLatch );
			  CheckPointScheduler scheduler = new CheckPointScheduler( checkPointer, ioLimiter, _jobScheduler, 0L, _health );
			  scheduler.Start();

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> checkpointerStarter = executor.submit(jobScheduler::runJob);
			  Future<object> checkpointerStarter = _executor.submit( _jobScheduler.runJob );

			  checkPointerLatch.await();
			  scheduler.Stop();
			  checkpointerStarter.get();

			  assertTrue( "Checkpointer should be created.", checkPointer.CheckpointCreated );
			  assertTrue( "Limiter should be enabled in the end.", ioLimiter.Limited );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCausePanicAfterSomeFailures() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCausePanicAfterSomeFailures()
		 {
			  // GIVEN
			  Exception[] failures = new Exception[]
			  {
				  new Exception( "First" ),
				  new Exception( "Second" ),
				  new Exception( "Third" )
			  };
			  when( _checkPointer.checkPointIfNeeded( any( typeof( TriggerInfo ) ) ) ).thenThrow( failures );
			  CheckPointScheduler scheduler = new CheckPointScheduler( _checkPointer, _ioLimiter, _jobScheduler, 1, _health );
			  scheduler.Start();

			  // WHEN
			  for ( int i = 0; i < CheckPointScheduler.MaxConsecutiveFailuresTolerance - 1; i++ )
			  {
					_jobScheduler.runJob();
					verifyZeroInteractions( _health );
			  }

			  try
			  {
					_jobScheduler.runJob();
					fail( "Should have failed" );
			  }
			  catch ( UnderlyingStorageException e )
			  {
					// THEN
					assertEquals( Iterators.asSet( failures ), Iterators.asSet( e.Suppressed ) );
					verify( _health ).panic( e );
			  }
		 }

		 private class ControlledCheckPointer : CheckPointer
		 {
			  internal volatile bool Fail;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long checkPointIfNeeded(TriggerInfo triggerInfo) throws java.io.IOException
			  public override long CheckPointIfNeeded( TriggerInfo triggerInfo )
			  {
					if ( Fail )
					{
						 throw new IOException( "Just failing" );
					}
					return 1;
			  }

			  public override long TryCheckPoint( TriggerInfo triggerInfo )
			  {
					throw new System.NotSupportedException();
			  }

			  public override long TryCheckPoint( TriggerInfo triggerInfo, System.Func<bool> timeout )
			  {
					throw new System.NotSupportedException();
			  }

			  public override long ForceCheckPoint( TriggerInfo triggerInfo )
			  {
					throw new System.NotSupportedException();
			  }

			  public override long LastCheckPointedTransactionId()
			  {
					throw new System.NotSupportedException();
			  }
		 }

		 private class CheckableIOLimiter : IOLimiter
		 {
			  internal volatile bool LimitEnabled;

			  public override long MaybeLimitIO( long previousStamp, int recentlyCompletedIOs, Flushable flushable )
			  {
					return 0;
			  }

			  public override void DisableLimit()
			  {
					LimitEnabled = false;
			  }

			  public override void EnableLimit()
			  {
					LimitEnabled = true;
			  }

			  public virtual bool Limited
			  {
				  get
				  {
						return LimitEnabled;
				  }
			  }
		 }

		 private class WaitUnlimitedCheckPointer : CheckPointer
		 {
			  internal readonly CheckableIOLimiter IoLimiter;
			  internal readonly System.Threading.CountdownEvent Latch;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal volatile bool CheckpointCreatedConflict;

			  internal WaitUnlimitedCheckPointer( CheckableIOLimiter ioLimiter, System.Threading.CountdownEvent latch )
			  {
					this.IoLimiter = ioLimiter;
					this.Latch = latch;
					CheckpointCreatedConflict = false;
			  }

			  public override long CheckPointIfNeeded( TriggerInfo triggerInfo )
			  {
					Latch.Signal();
					while ( IoLimiter.Limited )
					{
						 //spin while limiter enabled
					}
					CheckpointCreatedConflict = true;
					return 42;
			  }

			  public override long TryCheckPoint( TriggerInfo triggerInfo )
			  {
					throw new System.NotSupportedException( "This should have not been called" );
			  }

			  public override long TryCheckPoint( TriggerInfo triggerInfo, System.Func<bool> timeout )
			  {
					throw new System.NotSupportedException( "This should have not been called" );
			  }

			  public override long ForceCheckPoint( TriggerInfo triggerInfo )
			  {
					throw new System.NotSupportedException( "This should have not been called" );
			  }

			  public override long LastCheckPointedTransactionId()
			  {
					return 0;
			  }

			  internal virtual bool CheckpointCreated
			  {
				  get
				  {
						return CheckpointCreatedConflict;
				  }
			  }
		 }
	}

}