using System;
using System.Collections.Generic;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.com
{
	using Test = org.junit.Test;


	using Clocks = Neo4Net.Time.Clocks;
	using FakeClock = Neo4Net.Time.FakeClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class ResourcePoolTest
	{
		 private const int TIMEOUT_MILLIS = 100;
		 private static readonly int _timeoutExceedMillis = TIMEOUT_MILLIS + 10;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReuseBrokenInstances() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotReuseBrokenInstances()
		 {
			  ResourcePool<Something> pool = new ResourcePoolAnonymousInnerClass( this );

			  Something somethingFirst = pool.Acquire();
			  somethingFirst.DoStuff();
			  pool.Release();

			  Something something = pool.Acquire();
			  assertEquals( somethingFirst, something );
			  something.DoStuff();
			  something.Close();
			  pool.Release();

			  Something somethingElse = pool.Acquire();
			  assertNotSame( something, somethingElse );
			  somethingElse.DoStuff();
		 }

		 private class ResourcePoolAnonymousInnerClass : ResourcePool<Something>
		 {
			 private readonly ResourcePoolTest _outerInstance;

			 public ResourcePoolAnonymousInnerClass( ResourcePoolTest outerInstance ) : base( 5 )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override Something create()
			 {
				  return new Something();
			 }

			 protected internal override bool isAlive( Something resource )
			 {
				  return !resource.Closed;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTimeoutGracefully()
		 public virtual void ShouldTimeoutGracefully()
		 {
			  FakeClock clock = FakeClocks;

			  ResourcePool.CheckStrategy timeStrategy = new ResourcePool.CheckStrategy_TimeoutCheckStrategy( TIMEOUT_MILLIS, clock );

			  while ( clock.Millis() <= TIMEOUT_MILLIS )
			  {
					assertFalse( timeStrategy.ShouldCheck() );
					clock.Forward( 10, TimeUnit.MILLISECONDS );
			  }

			  assertTrue( timeStrategy.ShouldCheck() );

			  clock.Forward( 1, TimeUnit.MILLISECONDS );
			  assertFalse( timeStrategy.ShouldCheck() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBuildUpGracefullyUntilReachedMinPoolSize() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBuildUpGracefullyUntilReachedMinPoolSize()
		 {
			  // GIVEN
			  StatefulMonitor stateMonitor = new StatefulMonitor( this );
			  FakeClock clock = FakeClocks;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ResourcePool<Something> pool = getResourcePool(stateMonitor, clock, 5);
			  ResourcePool<Something> pool = GetResourcePool( stateMonitor, clock, 5 );

			  // WHEN
			  AcquireFromPool( pool, 5 );

			  // THEN
			  assertEquals( -1, stateMonitor.CurrentPeakSize.get() );
			  assertEquals( -1, stateMonitor.TargetSize.get() ); // that means the target size was not updated
			  assertEquals( 0, stateMonitor.DisposedConflict.get() ); // no disposed happened, since the count to update is 10
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBuildUpGracefullyWhilePassingMinPoolSizeBeforeTimerRings() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBuildUpGracefullyWhilePassingMinPoolSizeBeforeTimerRings()
		 {
			  // GIVEN
			  StatefulMonitor stateMonitor = new StatefulMonitor( this );
			  FakeClock clock = FakeClocks;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ResourcePool<Something> pool = getResourcePool(stateMonitor, clock, 5);
			  ResourcePool<Something> pool = GetResourcePool( stateMonitor, clock, 5 );

			  // WHEN
			  AcquireFromPool( pool, 15 );

			  // THEN
			  assertEquals( -1, stateMonitor.CurrentPeakSize.get() );
			  assertEquals( 15, stateMonitor.CreatedConflict.get() );
			  assertEquals( -1, stateMonitor.TargetSize.get() );
			  assertEquals( 0, stateMonitor.DisposedConflict.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateTargetSizeWhenSpikesOccur() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpdateTargetSizeWhenSpikesOccur()
		 {
			  // given
			  const int poolMinSize = 5;
			  const int poolMaxSize = 10;

			  StatefulMonitor stateMonitor = new StatefulMonitor( this );
			  FakeClock clock = FakeClocks;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ResourcePool<Something> pool = getResourcePool(stateMonitor, clock, poolMinSize);
			  ResourcePool<Something> pool = GetResourcePool( stateMonitor, clock, poolMinSize );

			  // when
			  IList<ResourceHolder> holders = AcquireFromPool( pool, poolMaxSize );
			  ExceedTimeout( clock );
			  ( ( IList<ResourceHolder> )holders ).AddRange( AcquireFromPool( pool, 1 ) ); // Needed to trigger the alarm

			  // then
			  assertEquals( poolMaxSize + 1, stateMonitor.CurrentPeakSize.get() );
			  // We have not released anything, so targetSize will not be reduced
			  assertEquals( poolMaxSize + 1, stateMonitor.TargetSize.get() ); // + 1 from the acquire

			  foreach ( ResourceHolder holder in holders )
			  {
					holder.End();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldKeepSmallPeakAndNeverDisposeIfAcquireAndReleaseContinuously() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldKeepSmallPeakAndNeverDisposeIfAcquireAndReleaseContinuously()
		 {
			  // given
			  const int poolMinSize = 1;

			  StatefulMonitor stateMonitor = new StatefulMonitor( this );
			  FakeClock clock = FakeClocks;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ResourcePool<Something> pool = getResourcePool(stateMonitor, clock, poolMinSize);
			  ResourcePool<Something> pool = GetResourcePool( stateMonitor, clock, poolMinSize );

			  // when
			  for ( int i = 0; i < 200; i++ )
			  {
					IList<ResourceHolder> newOnes = AcquireFromPool( pool, 1 );
					System.Threading.CountdownEvent release = new System.Threading.CountdownEvent( newOnes.Count );
					foreach ( ResourceHolder newOne in newOnes )
					{
						 newOne.Release( release );
					}
					release.await();
			  }

			  // then
			  assertEquals( -1, stateMonitor.CurrentPeakSize.get() ); // no alarm has rung, -1 is the default
			  assertEquals( 1, stateMonitor.CreatedConflict.get() );
			  assertEquals( 0, stateMonitor.DisposedConflict.get() ); // we should always be below min size, so 0 dispose calls
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSlowlyReduceTheNumberOfResourcesInThePoolWhenResourcesAreReleased() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSlowlyReduceTheNumberOfResourcesInThePoolWhenResourcesAreReleased()
		 {
			  // given
			  const int poolMinSize = 50;
			  const int poolMaxSize = 200;

			  StatefulMonitor stateMonitor = new StatefulMonitor( this );
			  FakeClock clock = FakeClocks;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final SomethingResourcePool pool = getResourcePool(stateMonitor, clock, poolMinSize);
			  SomethingResourcePool pool = GetResourcePool( stateMonitor, clock, poolMinSize );

			  AcquireResourcesAndExceedTimeout( pool, clock, poolMaxSize );

			  // when
			  // After the peak, stay below MIN_SIZE concurrent usage, using up all already present resources.
			  ExceedTimeout( clock );
			  for ( int i = 0; i < poolMaxSize; i++ )
			  {
					AcquireFromPool( pool, 1 )[0].release();
			  }

			  // then
			  // currentPeakSize must have reset from the latest check to minimum size.
			  assertEquals( 1, stateMonitor.CurrentPeakSize.get() ); // because of timeout
			  // targetSize must be set to MIN_SIZE since currentPeakSize was that 2 checks ago and didn't increase
			  assertEquals( poolMinSize, stateMonitor.TargetSize.get() );
			  // Only pooled resources must be used, disposing what is in excess
			  // +1 that was used to trigger exceed timeout check
			  assertEquals( poolMinSize, pool.UnusedSize() );
			  assertEquals( poolMaxSize - poolMinSize + 1, stateMonitor.DisposedConflict.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMaintainPoolHigherThenMinSizeWhenPeekUsagePasses() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMaintainPoolHigherThenMinSizeWhenPeekUsagePasses()
		 {
			  // given
			  const int poolMinSize = 50;
			  const int poolMaxSize = 200;
			  const int afterPeekPoolSize = 90;

			  StatefulMonitor stateMonitor = new StatefulMonitor( this );
			  FakeClock clock = FakeClocks;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final SomethingResourcePool pool = getResourcePool(stateMonitor, clock, poolMinSize);
			  SomethingResourcePool pool = GetResourcePool( stateMonitor, clock, poolMinSize );

			  AcquireResourcesAndExceedTimeout( pool, clock, poolMaxSize );

			  // when
			  // After the peak, stay at afterPeekPoolSize concurrent usage, using up all already present resources in the process
			  // but also keeping the high watermark above the minimum size
			  ExceedTimeout( clock );
			  // Requires some rounds to happen, since there is constant racing between releasing and acquiring which does
			  // not always result in reaping of resources, as there is reuse
			  for ( int i = 0; i < 10; i++ )
			  {
					// The latch is necessary to reduce races between batches
					System.Threading.CountdownEvent release = new System.Threading.CountdownEvent( afterPeekPoolSize );
					foreach ( ResourceHolder holder in AcquireFromPool( pool, afterPeekPoolSize ) )
					{
						 holder.Release( release );
					}
					release.await();
					ExceedTimeout( clock );
			  }

			  // then
			  // currentPeakSize should be at afterPeekPoolSize
			  assertEquals( afterPeekPoolSize, stateMonitor.CurrentPeakSize.get() );
			  // target size too
			  assertEquals( afterPeekPoolSize, stateMonitor.TargetSize.get() );
			  // only the excess from the maximum size down to after peek usage size must have been disposed
			  // +1 that was used to trigger exceed timeout check
			  assertEquals( afterPeekPoolSize, pool.UnusedSize() );
			  assertThat( stateMonitor.DisposedConflict.get(), greaterThanOrEqualTo(poolMaxSize - afterPeekPoolSize + 1) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReclaimAndRecreateWhenUsageGoesDownBetweenSpikes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReclaimAndRecreateWhenUsageGoesDownBetweenSpikes()
		 {
			  // given
			  const int poolMinSize = 50;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int bellowPoolMinSize = poolMinSize / 5;
			  int bellowPoolMinSize = poolMinSize / 5;
			  const int poolMaxSize = 200;

			  StatefulMonitor stateMonitor = new StatefulMonitor( this );
			  FakeClock clock = FakeClocks;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final SomethingResourcePool pool = getResourcePool(stateMonitor, clock, poolMinSize);
			  SomethingResourcePool pool = GetResourcePool( stateMonitor, clock, poolMinSize );

			  AcquireResourcesAndExceedTimeout( pool, clock, poolMaxSize );

			  // when
			  // After the peak, stay well below concurrent usage, using up all already present resources in the process
			  ExceedTimeout( clock );
			  // Requires some rounds to happen, since there is constant racing between releasing and acquiring which does
			  // not always result in reaping of resources, as there is reuse
			  for ( int i = 0; i < 30; i++ )
			  {
					// The latch is necessary to reduce races between batches
					System.Threading.CountdownEvent release = new System.Threading.CountdownEvent( bellowPoolMinSize );
					foreach ( ResourceHolder holder in AcquireFromPool( pool, bellowPoolMinSize ) )
					{
						 holder.Release( release );
					}
					release.await();
					ExceedTimeout( clock );
			  }

			  // then
			  // currentPeakSize should not be higher than bellowPoolMinSize
			  assertTrue( stateMonitor.CurrentPeakSize.get().ToString(), stateMonitor.CurrentPeakSize.get() <= bellowPoolMinSize );
			  // target size should remain at pool min size
			  assertEquals( poolMinSize, stateMonitor.TargetSize.get() );
			  assertEquals( poolMinSize, pool.UnusedSize() );
			  // only the excess from the pool max size down to min size must have been disposed
			  // +1 that was used to trigger initial exceed timeout check
			  assertEquals( poolMaxSize - poolMinSize + 1, stateMonitor.DisposedConflict.get() );

			  stateMonitor.CreatedConflict.set( 0 );
			  stateMonitor.DisposedConflict.set( 0 );

			  // when
			  // After the lull, recreate a peak
			  AcquireResourcesAndExceedTimeout( pool, clock, poolMaxSize );

			  // then
			  assertEquals( poolMaxSize - poolMinSize + 1, stateMonitor.CreatedConflict.get() );
			  assertEquals( 0, stateMonitor.DisposedConflict.get() );
		 }

		 private void ExceedTimeout( FakeClock clock )
		 {
			  clock.Forward( _timeoutExceedMillis, TimeUnit.MILLISECONDS );
		 }

		 private FakeClock FakeClocks
		 {
			 get
			 {
				  return Clocks.fakeClock();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void acquireResourcesAndExceedTimeout(ResourcePool<Something> pool, Neo4Net.time.FakeClock clock, int resourcesToAcquire) throws InterruptedException
		 private void AcquireResourcesAndExceedTimeout( ResourcePool<Something> pool, FakeClock clock, int resourcesToAcquire )
		 {
			  IList<ResourceHolder> holders = new LinkedList<ResourceHolder>();
			  ( ( IList<ResourceHolder> )holders ).AddRange( AcquireFromPool( pool, resourcesToAcquire ) );

			  ExceedTimeout( clock );

			  // "Ring the bell" only on acquisition, of course.
			  ( ( IList<ResourceHolder> )holders ).AddRange( AcquireFromPool( pool, 1 ) );

			  foreach ( ResourceHolder holder in holders )
			  {
					holder.Release();
			  }
		 }

		 private SomethingResourcePool GetResourcePool( StatefulMonitor stateMonitor, FakeClock clock, int minSize )
		 {
			  ResourcePool.CheckStrategy_TimeoutCheckStrategy timeoutCheckStrategy = new ResourcePool.CheckStrategy_TimeoutCheckStrategy( TIMEOUT_MILLIS, clock );
			  return new SomethingResourcePool( minSize, timeoutCheckStrategy, stateMonitor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.List<ResourceHolder> acquireFromPool(ResourcePool pool, int resourcesToAcquire) throws InterruptedException
		 private IList<ResourceHolder> AcquireFromPool( ResourcePool pool, int resourcesToAcquire )
		 {
			  IList<ResourceHolder> acquirers = new LinkedList<ResourceHolder>();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.CountDownLatch latch = new java.util.concurrent.CountDownLatch(resourcesToAcquire);
			  System.Threading.CountdownEvent latch = new System.Threading.CountdownEvent( resourcesToAcquire );
			  for ( int i = 0; i < resourcesToAcquire; i++ )
			  {
					ResourceHolder holder = new ResourceHolder( this, pool, latch );
					Thread t = new Thread( holder );
					acquirers.Add( holder );
					t.Start();
			  }
			  latch.await();
			  return acquirers;
		 }

		 private class SomethingResourcePool : ResourcePool<Something>
		 {
			  internal SomethingResourcePool( int minSize, CheckStrategy checkStrategy, StatefulMonitor stateMonitor ) : base( minSize, checkStrategy, stateMonitor )
			  {
			  }

			  protected internal override Something Create()
			  {
					return new Something();
			  }

			  protected internal override bool IsAlive( Something resource )
			  {
					return !resource.Closed;
			  }

			  public virtual int UnusedSize()
			  {
					return Unused.Count;
			  }
		 }

		 private class ResourceHolder : ThreadStart
		 {
			 private readonly ResourcePoolTest _outerInstance;

			  internal readonly Semaphore Latch = new Semaphore( 0 );
			  internal readonly System.Threading.CountdownEvent Released = new System.Threading.CountdownEvent( 1 );
			  internal readonly System.Threading.CountdownEvent OnAcquire;
			  internal readonly ResourcePool Pool;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly AtomicBoolean ReleaseConflict = new AtomicBoolean();
			  internal volatile Thread Runner;

			  internal ResourceHolder( ResourcePoolTest outerInstance, ResourcePool pool, System.Threading.CountdownEvent onAcquire )
			  {
				  this._outerInstance = outerInstance;
					this.Pool = pool;
					this.OnAcquire = onAcquire;
			  }

			  public override void Run()
			  {
					Runner = Thread.CurrentThread;
					try
					{
						 Pool.acquire();
						 OnAcquire.Signal();
						 try
						 {
							  Latch.acquire();
						 }
						 catch ( InterruptedException e )
						 {
							  throw new Exception( e );
						 }
						 if ( ReleaseConflict.get() )
						 {
							  Pool.release();
							  Released.Signal();
						 }
					}
					catch ( Exception e )
					{
						 Console.WriteLine( e.ToString() );
						 Console.Write( e.StackTrace );
					}
			  }

			  public virtual void Release()
			  {
					this.ReleaseConflict.set( true );
					Latch.release();
					try
					{
						 Released.await();

						 Thread runner;
						 do
						 {
							  // Wait to observe thread running this ResourceHolder.
							  // If we don't, then the thread can continue running for a little while after releasing, which can
							  // result in racy changes to the StatefulMonitor.
							  runner = this.Runner;
						 } while ( runner == null );
						 runner.Join();
					}
					catch ( InterruptedException e )
					{
						 Console.WriteLine( e.ToString() );
						 Console.Write( e.StackTrace );
					}
			  }

			  public virtual void Release( System.Threading.CountdownEvent releaseLatch )
			  {
					Release();
					releaseLatch.Signal();
			  }

			  public virtual void End()
			  {
					this.ReleaseConflict.set( false );
					Latch.release();
			  }
		 }

		 private class StatefulMonitor : ResourcePool.Monitor<Something>
		 {
			 private readonly ResourcePoolTest _outerInstance;

			 public StatefulMonitor( ResourcePoolTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public AtomicInteger CurrentPeakSize = new AtomicInteger( -1 );
			  public AtomicInteger TargetSize = new AtomicInteger( -1 );
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  public AtomicInteger CreatedConflict = new AtomicInteger( 0 );
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  public AtomicInteger AcquiredConflict = new AtomicInteger( 0 );
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  public AtomicInteger DisposedConflict = new AtomicInteger( 0 );

			  public override void UpdatedCurrentPeakSize( int currentPeakSize )
			  {
					this.CurrentPeakSize.set( currentPeakSize );
			  }

			  public override void UpdatedTargetSize( int targetSize )
			  {
					this.TargetSize.set( targetSize );
			  }

			  public override void Created( Something something )
			  {
					this.CreatedConflict.incrementAndGet();
			  }

			  public override void Acquired( Something something )
			  {
					this.AcquiredConflict.incrementAndGet();
			  }

			  public override void Disposed( Something something )
			  {
					this.DisposedConflict.incrementAndGet();
			  }
		 }

		 private class Something
		 {
			  internal bool Closed;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void doStuff() throws Exception
			  public virtual void DoStuff()
			  {
					if ( Closed )
					{
						 throw new Exception( "Closed" );
					}
			  }

			  public virtual void Close()
			  {
					this.Closed = true;
			  }
		 }

	}

}