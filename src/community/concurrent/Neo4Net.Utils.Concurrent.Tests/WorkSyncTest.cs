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
namespace Neo4Net.Utils.Concurrent
{
	using AfterAll = org.junit.jupiter.api.AfterAll;
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using BeforeAll = org.junit.jupiter.api.BeforeAll;
	using Test = org.junit.jupiter.api.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.sameInstance;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.fail;

	internal class WorkSyncTest
	{
		private bool InstanceFieldsInitialized = false;

		public WorkSyncTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_adder = new Adder( this );
			_sync = new WorkSync<Adder, AddWork>( _adder );
		}

		 private static ExecutorService _executor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeAll static void startExecutor()
		 internal static void StartExecutor()
		 {
			  _executor = Executors.newCachedThreadPool();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterAll static void stopExecutor()
		 internal static void StopExecutor()
		 {
			  _executor.shutdownNow();
		 }

		 private static void Usleep( long micros )
		 {
			  long deadline = System.nanoTime() + TimeUnit.MICROSECONDS.toNanos(micros);
			  long now;
			  do
			  {
					now = System.nanoTime();
			  } while ( now < deadline );
		 }

		 private class AddWork : Work<Adder, AddWork>
		 {
			  internal int Delta;

			  internal AddWork( int delta )
			  {
					this.Delta = delta;
			  }

			  public override AddWork Combine( AddWork work )
			  {
					Delta += work.Delta;
					return this;
			  }

			  public override void Apply( Adder adder )
			  {
					Usleep( 50 );
					adder.Add( Delta );
			  }
		 }

		 private class Adder
		 {
			 private readonly WorkSyncTest _outerInstance;

			 public Adder( WorkSyncTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public virtual void Add( int delta )
			  {
					outerInstance.semaphore.acquireUninterruptibly();
					outerInstance.sum.Add( delta );
					outerInstance.count.Increment();
			  }
		 }

		 private class CallableWork : Callable<Void>
		 {
			 private readonly WorkSyncTest _outerInstance;

			  internal readonly AddWork AddWork;

			  internal CallableWork( WorkSyncTest outerInstance, AddWork addWork )
			  {
				  this._outerInstance = outerInstance;
					this.AddWork = addWork;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Void call() throws java.util.concurrent.ExecutionException
			  public override Void Call()
			  {
					outerInstance.sync.Apply( AddWork );
					return null;
			  }
		 }

		 private class UnsynchronisedAdder
		 {
			  // The volatile modifier prevents hoisting and reordering optimisations that could *hide* races
			  internal volatile long Value;

			  public virtual void Add( long delta )
			  {
					long v = Value;
					// Make sure other threads have a chance to run and race with our update
					Thread.yield();
					// Allow an up to ~50 micro-second window for racing and losing updates
					Usleep( ThreadLocalRandom.current().Next(50) );
					Value = v + delta;
			  }

			  public virtual void Increment()
			  {
					Add( 1 );
			  }

			  public virtual long Sum()
			  {
					return Value;
			  }
		 }

		 private UnsynchronisedAdder _sum = new UnsynchronisedAdder();
		 private UnsynchronisedAdder _count = new UnsynchronisedAdder();
		 private Adder _adder;
		 private WorkSync<Adder, AddWork> _sync;
		 private Semaphore _semaphore = new Semaphore( int.MaxValue );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach private void refillSemaphore()
		 private void RefillSemaphore()
		 {
			  // This ensures that no threads end up stuck
			  _semaphore.drainPermits();
			  _semaphore.release( int.MaxValue );
		 }

		 private Future<Void> MakeWorkStuckAtSemaphore( int delta )
		 {
			  _semaphore.drainPermits();
			  Future<Void> concurrentWork = _executor.submit( new CallableWork( this, new AddWork( delta ) ) );
			  assertThrows( typeof( TimeoutException ), () => concurrentWork.get(10, TimeUnit.MILLISECONDS) );
			  while ( !_semaphore.hasQueuedThreads() )
			  {
					Usleep( 1 );
			  }
			  // good, the concurrent AddWork is now stuck on the semaphore
			  return concurrentWork;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustApplyWork() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustApplyWork()
		 {
			  _sync.apply( new AddWork( 10 ) );
			  assertThat( _sum.sum(), @is(10L) );

			  _sync.apply( new AddWork( 20 ) );
			  assertThat( _sum.sum(), @is(30L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustCombineWork() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustCombineWork()
		 {
			  BinaryLatch startLatch = new BinaryLatch();
			  BinaryLatch blockLatch = new BinaryLatch();
			  FutureTask<Void> blocker = new FutureTask<Void>( new CallableWork( this, new AddWorkAnonymousInnerClass( this, startLatch, blockLatch ) ) );
			  ( new Thread( blocker ) ).Start();
			  startLatch.Await();
			  ICollection<FutureTask<Void>> tasks = new List<FutureTask<Void>>();
			  tasks.Add( blocker );
			  for ( int i = 0; i < 20; i++ )
			  {

					CallableWork task = new CallableWork( this, new AddWork( 1 ) );
					FutureTask<Void> futureTask = new FutureTask<Void>( task );
					tasks.Add( futureTask );
					Thread thread = new Thread( futureTask );
					thread.Start();
					//noinspection StatementWithEmptyBody,LoopConditionNotUpdatedInsideLoop
					while ( thread.State != Thread.State.TIMED_WAITING )
					{
						 // Wait for the thread to reach the lock.
					}
			  }
			  blockLatch.Release();
			  foreach ( FutureTask<Void> task in tasks )
			  {
					task.get();
			  }
			  assertThat( _count.sum(), lessThan(_sum.sum()) );
		 }

		 private class AddWorkAnonymousInnerClass : AddWork
		 {
			 private readonly WorkSyncTest _outerInstance;

			 private Neo4Net.Utils.Concurrent.BinaryLatch _startLatch;
			 private Neo4Net.Utils.Concurrent.BinaryLatch _blockLatch;

			 public AddWorkAnonymousInnerClass( WorkSyncTest outerInstance, Neo4Net.Utils.Concurrent.BinaryLatch startLatch, Neo4Net.Utils.Concurrent.BinaryLatch blockLatch ) : base( 1 )
			 {
				 this.outerInstance = outerInstance;
				 this._startLatch = startLatch;
				 this._blockLatch = blockLatch;
			 }

			 public override void apply( Adder adder )
			 {
				  base.apply( adder );
				  _startLatch.release();
				  _blockLatch.await();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustApplyWorkEvenWhenInterrupted() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustApplyWorkEvenWhenInterrupted()
		 {
			  Thread.CurrentThread.Interrupt();

			  _sync.apply( new AddWork( 10 ) );

			  assertThat( _sum.sum(), @is(10L) );
			  assertTrue( Thread.interrupted() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustRecoverFromExceptions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustRecoverFromExceptions()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean broken = new java.util.concurrent.atomic.AtomicBoolean(true);
			  AtomicBoolean broken = new AtomicBoolean( true );
			  Adder adder = new AdderAnonymousInnerClass( this, broken );
			  _sync = new WorkSync<Adder, AddWork>( adder );

			  try
			  {
					// Run this in a different thread to account for reentrant locks.
					_executor.submit( new CallableWork( this, new AddWork( 10 ) ) ).get();
					fail( "Should have thrown" );
			  }
			  catch ( ExecutionException exception )
			  {
					// Outermost ExecutionException from the ExecutorService
					assertThat( exception.InnerException, instanceOf( typeof( ExecutionException ) ) );

					// Inner ExecutionException from the WorkSync
					exception = ( ExecutionException ) exception.InnerException;
					assertThat( exception.InnerException, instanceOf( typeof( System.InvalidOperationException ) ) );
			  }

			  broken.set( false );
			  _sync.apply( new AddWork( 20 ) );

			  assertThat( _sum.sum(), @is(20L) );
			  assertThat( _count.sum(), @is(1L) );
		 }

		 private class AdderAnonymousInnerClass : Adder
		 {
			 private readonly WorkSyncTest _outerInstance;

			 private AtomicBoolean _broken;

			 public AdderAnonymousInnerClass( WorkSyncTest outerInstance, AtomicBoolean broken ) : base( outerInstance )
			 {
				 this.outerInstance = outerInstance;
				 this._broken = broken;
			 }

			 public override void add( int delta )
			 {
				  if ( _broken.get() )
				  {
						throw new System.InvalidOperationException( "boom!" );
				  }
				  base.add( delta );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustNotApplyWorkInParallelUnderStress() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustNotApplyWorkInParallelUnderStress()
		 {
			  int workers = Runtime.Runtime.availableProcessors() * 5;
			  int iterations = 1_000;
			  int incrementValue = 42;
			  System.Threading.CountdownEvent startLatch = new System.Threading.CountdownEvent( workers );
			  System.Threading.CountdownEvent endLatch = new System.Threading.CountdownEvent( workers );
			  AtomicBoolean start = new AtomicBoolean();
			  Callable<Void> work = () =>
			  {
				startLatch.Signal();
				bool spin;
				do
				{
					 spin = !start.get();
				} while ( spin );

				for ( int i = 0; i < iterations; i++ )
				{
					 _sync.apply( new AddWork( incrementValue ) );
				}

				endLatch.Signal();
				return null;
			  };

			  IList<Future<Void>> futureList = new List<Future<Void>>();
			  for ( int i = 0; i < workers; i++ )
			  {
					futureList.Add( _executor.submit( work ) );
			  }
			  startLatch.await();
			  start.set( true );
			  endLatch.await();

			  foreach ( Future<Void> future in futureList )
			  {
					future.get(); // check for any exceptions
			  }

			  assertThat( _count.sum(), lessThan((long)(workers * iterations)) );
			  assertThat( _sum.sum(), @is((long)(incrementValue * workers * iterations)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustNotApplyAsyncWorkInParallelUnderStress() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustNotApplyAsyncWorkInParallelUnderStress()
		 {
			  int workers = Runtime.Runtime.availableProcessors() * 5;
			  int iterations = 1_000;
			  int incrementValue = 42;
			  System.Threading.CountdownEvent startLatch = new System.Threading.CountdownEvent( workers );
			  System.Threading.CountdownEvent endLatch = new System.Threading.CountdownEvent( workers );
			  AtomicBoolean start = new AtomicBoolean();
			  Callable<Void> work = () =>
			  {
				startLatch.Signal();
				bool spin;
				do
				{
					 spin = !start.get();
				} while ( spin );

				ThreadLocalRandom rng = ThreadLocalRandom.current();
				IList<AsyncApply> asyncs = new List<AsyncApply>();
				for ( int i = 0; i < iterations; i++ )
				{
					 asyncs.add( _sync.applyAsync( new AddWork( incrementValue ) ) );
					 if ( rng.Next( 10 ) == 0 )
					 {
						  foreach ( AsyncApply async in asyncs )
						  {
								async.Await();
						  }
						  asyncs.clear();
					 }
				}

				foreach ( AsyncApply async in asyncs )
				{
					 async.Await();
				}
				endLatch.Signal();
				return null;
			  };

			  IList<Future<Void>> futureList = new List<Future<Void>>();
			  for ( int i = 0; i < workers; i++ )
			  {
					futureList.Add( _executor.submit( work ) );
			  }
			  startLatch.await();
			  start.set( true );
			  endLatch.await();

			  foreach ( Future<Void> future in futureList )
			  {
					future.get(); // check for any exceptions
			  }

			  assertThat( _count.sum(), lessThan((long)(workers * iterations)) );
			  assertThat( _sum.sum(), @is((long)(incrementValue * workers * iterations)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustApplyWorkAsync() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustApplyWorkAsync()
		 {
			  AsyncApply a = _sync.applyAsync( new AddWork( 10 ) );
			  a.Await();
			  assertThat( _sum.sum(), @is(10L) );

			  AsyncApply b = _sync.applyAsync( new AddWork( 20 ) );
			  AsyncApply c = _sync.applyAsync( new AddWork( 30 ) );
			  b.Await();
			  c.Await();
			  assertThat( _sum.sum(), @is(60L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustCombineWorkAsync() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustCombineWorkAsync()
		 {
			  MakeWorkStuckAtSemaphore( 1 );

			  AsyncApply a = _sync.applyAsync( new AddWork( 1 ) );
			  AsyncApply b = _sync.applyAsync( new AddWork( 1 ) );
			  AsyncApply c = _sync.applyAsync( new AddWork( 1 ) );
			  _semaphore.release( 2 );
			  a.Await();
			  b.Await();
			  c.Await();
			  assertThat( _sum.sum(), @is(4L) );
			  assertThat( _count.sum(), @is(2L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void mustApplyWorkAsyncEvenWhenInterrupted() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void MustApplyWorkAsyncEvenWhenInterrupted()
		 {
			  Thread.CurrentThread.Interrupt();

			  _sync.applyAsync( new AddWork( 10 ) ).await();

			  assertThat( _sum.sum(), @is(10L) );
			  assertTrue( Thread.interrupted() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void asyncWorkThatThrowsMustRememberException()
		 internal virtual void AsyncWorkThatThrowsMustRememberException()
		 {
			  Exception boo = new Exception( "boo" );
			  AsyncApply asyncApply = _sync.applyAsync( new AddWorkAnonymousInnerClass2( this, boo ) );

			  try
			  {
					asyncApply.Await();
					fail( "Should have thrown" );
			  }
			  catch ( ExecutionException e )
			  {
					assertThat( e.InnerException, sameInstance( boo ) );
			  }

			  assertThat( _sum.sum(), @is(10L) );
			  assertThat( _count.sum(), @is(1L) );

			  try
			  {
					asyncApply.Await();
					fail( "Should have thrown" );
			  }
			  catch ( ExecutionException e )
			  {
					assertThat( e.InnerException, sameInstance( boo ) );
			  }

			  assertThat( _sum.sum(), @is(10L) );
			  assertThat( _count.sum(), @is(1L) );
		 }

		 private class AddWorkAnonymousInnerClass2 : AddWork
		 {
			 private readonly WorkSyncTest _outerInstance;

			 private Exception _boo;

			 public AddWorkAnonymousInnerClass2( WorkSyncTest outerInstance, Exception boo ) : base( 10 )
			 {
				 this.outerInstance = outerInstance;
				 this._boo = boo;
			 }

			 public override void apply( Adder adder )
			 {
				  base.apply( adder );
				  throw _boo;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void asyncWorkThatThrowsInAwaitMustRememberException() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void AsyncWorkThatThrowsInAwaitMustRememberException()
		 {
			  Future<Void> stuckAtSemaphore = MakeWorkStuckAtSemaphore( 1 );

			  Exception boo = new Exception( "boo" );
			  AsyncApply asyncApply = _sync.applyAsync( new AddWorkAnonymousInnerClass3( this, boo ) );

			  RefillSemaphore();
			  stuckAtSemaphore.get();

			  try
			  {
					asyncApply.Await();
					fail( "Should have thrown" );
			  }
			  catch ( ExecutionException e )
			  {
					assertThat( e.InnerException, sameInstance( boo ) );
			  }

			  assertThat( _sum.sum(), @is(11L) );
			  assertThat( _count.sum(), @is(2L) );

			  try
			  {
					asyncApply.Await();
					fail( "Should have thrown" );
			  }
			  catch ( ExecutionException e )
			  {
					assertThat( e.InnerException, sameInstance( boo ) );
			  }

			  assertThat( _sum.sum(), @is(11L) );
			  assertThat( _count.sum(), @is(2L) );
		 }

		 private class AddWorkAnonymousInnerClass3 : AddWork
		 {
			 private readonly WorkSyncTest _outerInstance;

			 private Exception _boo;

			 public AddWorkAnonymousInnerClass3( WorkSyncTest outerInstance, Exception boo ) : base( 10 )
			 {
				 this.outerInstance = outerInstance;
				 this._boo = boo;
			 }

			 public override void apply( Adder adder )
			 {
				  base.apply( adder );
				  throw _boo;
			 }
		 }
	}

}