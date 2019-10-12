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
namespace Neo4Net.Collection.pool
{
	using Test = org.junit.jupiter.api.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	internal class LinkedQueuePoolTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldTimeoutGracefully()
		 internal virtual void ShouldTimeoutGracefully()
		 {
			  FakeClock clock = new FakeClock();

			  LinkedQueuePool.CheckStrategy timeStrategy = new LinkedQueuePool.CheckStrategy_TimeoutCheckStrategy( 100, clock );

			  while ( clock.AsLong <= 100 )
			  {
					assertFalse( timeStrategy.ShouldCheck() );
					clock.Forward( 10, TimeUnit.MILLISECONDS );
			  }

			  assertTrue( timeStrategy.ShouldCheck() );

			  clock.Forward( 1, TimeUnit.MILLISECONDS );
			  assertFalse( timeStrategy.ShouldCheck() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBuildUpGracefullyUntilReachedMinPoolSize()
		 internal virtual void ShouldBuildUpGracefullyUntilReachedMinPoolSize()
		 {
			  // GIVEN
			  StatefulMonitor stateMonitor = new StatefulMonitor();
			  FakeClock clock = new FakeClock();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LinkedQueuePool<Object> pool = getLinkedQueuePool(stateMonitor, clock, 5);
			  LinkedQueuePool<object> pool = GetLinkedQueuePool( stateMonitor, clock, 5 );

			  // WHEN
			  IList<FlyweightHolder<object>> flyweightHolders = AcquireFromPool( pool, 5 );
			  foreach ( FlyweightHolder<object> flyweightHolder in flyweightHolders )
			  {
					flyweightHolder.Release();
			  }

			  // THEN
			  // clock didn't tick, these two are not set
			  assertEquals( -1, stateMonitor.CurrentPeakSize.get() );
			  assertEquals( -1, stateMonitor.TargetSize.get() );
			  // no disposed happened, since the count to update is 5
			  assertEquals( 0, stateMonitor.DisposedConflict.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBuildUpGracefullyWhilePassingMinPoolSizeBeforeTimerRings()
		 internal virtual void ShouldBuildUpGracefullyWhilePassingMinPoolSizeBeforeTimerRings()
		 {
			  // GIVEN
			  StatefulMonitor stateMonitor = new StatefulMonitor();
			  FakeClock clock = new FakeClock();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LinkedQueuePool<Object> pool = getLinkedQueuePool(stateMonitor, clock, 5);
			  LinkedQueuePool<object> pool = GetLinkedQueuePool( stateMonitor, clock, 5 );

			  // WHEN
			  IList<FlyweightHolder<object>> flyweightHolders = AcquireFromPool( pool, 15 );
			  foreach ( FlyweightHolder<object> flyweightHolder in flyweightHolders )
			  {
					flyweightHolder.Release();
			  }

			  // THEN
			  // The clock hasn't ticked, so these two should be unset
			  assertEquals( -1, stateMonitor.CurrentPeakSize.get() );
			  assertEquals( -1, stateMonitor.TargetSize.get() );
			  // We obviously created 15 threads
			  assertEquals( 15, stateMonitor.CreatedConflict.get() );
			  // And of those 10 are not needed and therefore disposed on release (min size is 5)
			  assertEquals( 10, stateMonitor.DisposedConflict.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldUpdateTargetSizeWhenSpikesOccur()
		 internal virtual void ShouldUpdateTargetSizeWhenSpikesOccur()
		 {
			  // given
			  const int minSize = 5;
			  const int maxSize = 10;

			  StatefulMonitor stateMonitor = new StatefulMonitor();
			  FakeClock clock = new FakeClock();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LinkedQueuePool<Object> pool = getLinkedQueuePool(stateMonitor, clock, MIN_SIZE);
			  LinkedQueuePool<object> pool = GetLinkedQueuePool( stateMonitor, clock, minSize );
			  // when
			  IList<FlyweightHolder<object>> holders = AcquireFromPool( pool, maxSize );
			  clock.Forward( 110, TimeUnit.MILLISECONDS );
			  ( ( IList<FlyweightHolder<object>> )holders ).AddRange( AcquireFromPool( pool, 1 ) ); // Needed to trigger the alarm

			  // then
			  assertEquals( maxSize + 1, stateMonitor.CurrentPeakSize.get() );
			  // We have not released anything, so targetSize will not be reduced
			  assertEquals( maxSize + 1, stateMonitor.TargetSize.get() ); // + 1 from the acquire
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldKeepSmallPeakAndNeverDisposeIfAcquireAndReleaseContinuously()
		 internal virtual void ShouldKeepSmallPeakAndNeverDisposeIfAcquireAndReleaseContinuously()
		 {
			  // given
			  const int minSize = 1;

			  StatefulMonitor stateMonitor = new StatefulMonitor();
			  FakeClock clock = new FakeClock();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LinkedQueuePool<Object> pool = getLinkedQueuePool(stateMonitor, clock, MIN_SIZE);
			  LinkedQueuePool<object> pool = GetLinkedQueuePool( stateMonitor, clock, minSize );
			  // when
			  for ( int i = 0; i < 200; i++ )
			  {
					IList<FlyweightHolder<object>> newOnes = AcquireFromPool( pool, 1 );
					foreach ( FlyweightHolder newOne in newOnes )
					{
						 newOne.release();
					}
			  }

			  // then
			  assertEquals( -1, stateMonitor.CurrentPeakSize.get() ); // no alarm has rung, -1 is the default
			  assertEquals( 1, stateMonitor.CreatedConflict.get() );
			  assertEquals( 0, stateMonitor.DisposedConflict.get() ); // we should always be below min size, so 0 dispose calls
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSlowlyReduceTheNumberOfFlyweightsInThePoolWhenFlyweightsAreReleased()
		 internal virtual void ShouldSlowlyReduceTheNumberOfFlyweightsInThePoolWhenFlyweightsAreReleased()
		 {
			  // given
			  const int minSize = 50;
			  const int maxSize = 200;

			  StatefulMonitor stateMonitor = new StatefulMonitor();
			  FakeClock clock = new FakeClock();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LinkedQueuePool<Object> pool = getLinkedQueuePool(stateMonitor, clock, MIN_SIZE);
			  LinkedQueuePool<object> pool = GetLinkedQueuePool( stateMonitor, clock, minSize );
			  IList<FlyweightHolder<object>> holders = new LinkedList<FlyweightHolder<object>>();
			  BuildAPeakOfAcquiredFlyweightsAndTriggerAlarmWithSideEffects( maxSize, clock, pool, holders );

			  // when
			  // After the peak, stay below MIN_SIZE concurrent usage, using up all already present Flyweights.
			  clock.Forward( 110, TimeUnit.MILLISECONDS );
			  for ( int i = 0; i < maxSize; i++ )
			  {
					AcquireFromPool( pool, 1 )[0].release();
			  }

			  // then

			  // currentPeakSize must have reset from the latest alarm to MIN_SIZE.
			  assertEquals( 1, stateMonitor.CurrentPeakSize.get() ); // Alarm
			  // targetSize must be set to MIN_SIZE since currentPeakSize was that 2 alarms ago and didn't increase
			  assertEquals( minSize, stateMonitor.TargetSize.get() );
			  // Only pooled Flyweights must be used, disposing what is in excess
			  // +1 for the alarm from buildAPeakOfAcquiredFlyweightsAndTriggerAlarmWithSideEffects
			  assertEquals( maxSize - minSize + 1, stateMonitor.DisposedConflict.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldMaintainPoolAtHighWatermarkWhenConcurrentUsagePassesMinSize()
		 internal virtual void ShouldMaintainPoolAtHighWatermarkWhenConcurrentUsagePassesMinSize()
		 {
			  // given
			  const int minSize = 50;
			  const int maxSize = 200;
			  const int midSize = 90;

			  StatefulMonitor stateMonitor = new StatefulMonitor();
			  FakeClock clock = new FakeClock();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LinkedQueuePool<Object> pool = getLinkedQueuePool(stateMonitor, clock, MIN_SIZE);
			  LinkedQueuePool<object> pool = GetLinkedQueuePool( stateMonitor, clock, minSize );
			  IList<FlyweightHolder<object>> holders = new LinkedList<FlyweightHolder<object>>();

			  // when
			  BuildAPeakOfAcquiredFlyweightsAndTriggerAlarmWithSideEffects( maxSize, clock, pool, holders );

			  // then
			  assertEquals( maxSize + 1, stateMonitor.CurrentPeakSize.get() ); // the peak method above does +1 on the peak

			  // when
			  /* After the peak, stay at MID_SIZE concurrent usage, using up all already present Flyweights in the process
			   * but also keeping the high watermark above the MIN_SIZE
			   * We must do this at least twice, since the counter for disposed is incremented once per release, if appropriate,
			   * and only after the clock has ticked. Essentially this is one loop for reducing the watermark down to
			   * mid size and one more loop to dispose of all excess resources. That does indeed mean that there is a lag
			   * of one clock tick before resources are disposed. If this is a bug or not remains to be seen.
			   */
			  for ( int i = 0; i < 2; i++ )
			  {
					clock.Forward( 110, TimeUnit.MILLISECONDS );
					foreach ( FlyweightHolder holder in AcquireFromPool( pool, midSize ) )
					{
						 holder.release();
					}
					clock.Forward( 110, TimeUnit.MILLISECONDS );
					foreach ( FlyweightHolder holder in AcquireFromPool( pool, midSize ) )
					{
						 holder.release();
					}
			  }

			  // then
			  // currentPeakSize should be at MID_SIZE
			  assertEquals( midSize, stateMonitor.CurrentPeakSize.get() );
			  // target size too
			  assertEquals( midSize, stateMonitor.TargetSize.get() );
			  // only the excess from the MAX_SIZE down to mid size must have been disposed
			  // +1 for the alarm from buildAPeakOfAcquiredFlyweightsAndTriggerAlarmWithSideEffects
			  assertEquals( maxSize - midSize + 1, stateMonitor.DisposedConflict.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReclaimAndRecreateWhenLullBetweenSpikesOccurs()
		 internal virtual void ShouldReclaimAndRecreateWhenLullBetweenSpikesOccurs()
		 {
			  // given
			  const int minSize = 50;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int BELOW_MIN_SIZE = MIN_SIZE / 5;
			  int belowMinSize = minSize / 5;
			  const int maxSize = 200;

			  StatefulMonitor stateMonitor = new StatefulMonitor();
			  FakeClock clock = new FakeClock();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LinkedQueuePool<Object> pool = getLinkedQueuePool(stateMonitor, clock, MIN_SIZE);
			  LinkedQueuePool<object> pool = GetLinkedQueuePool( stateMonitor, clock, minSize );
			  IList<FlyweightHolder<object>> holders = new LinkedList<FlyweightHolder<object>>();
			  BuildAPeakOfAcquiredFlyweightsAndTriggerAlarmWithSideEffects( maxSize, clock, pool, holders );

			  // when
			  // After the peak, stay well below concurrent usage, using up all already present Flyweights in the process
			  clock.Forward( 110, TimeUnit.MILLISECONDS );
			  // Requires some rounds to happen, since there is constant racing between releasing and acquiring which does
			  // not always result in reaping of Flyweights, as there is reuse
			  for ( int i = 0; i < 30; i++ )
			  {
					// The latch is necessary to reduce races between batches
					foreach ( FlyweightHolder holder in AcquireFromPool( pool, belowMinSize ) )
					{
						 holder.release();
					}
					clock.Forward( 110, TimeUnit.MILLISECONDS );
			  }

			  // then
			  // currentPeakSize should be at MIN_SIZE / 5
			  assertTrue( stateMonitor.CurrentPeakSize.get() <= belowMinSize, "Expected " + stateMonitor.CurrentPeakSize.get() + " <= " + belowMinSize );
			  // target size should remain at MIN_SIZE
			  assertEquals( minSize, stateMonitor.TargetSize.get() );
			  // only the excess from the MAX_SIZE down to min size must have been disposed
			  // +1 for the alarm from buildAPeakOfAcquiredFlyweightsAndTriggerAlarmWithSideEffects
			  assertEquals( maxSize - minSize + 1, stateMonitor.DisposedConflict.get() );

			  stateMonitor.CreatedConflict.set( 0 );
			  stateMonitor.DisposedConflict.set( 0 );

			  // when
			  // After the lull, recreate a peak
			  ( ( IList<FlyweightHolder<object>> )holders ).AddRange( AcquireFromPool( pool, maxSize ) );

			  // then
			  assertEquals( maxSize - minSize, stateMonitor.CreatedConflict.get() );
			  assertEquals( 0, stateMonitor.DisposedConflict.get() );
		 }

		 private void BuildAPeakOfAcquiredFlyweightsAndTriggerAlarmWithSideEffects( int maxSize, FakeClock clock, LinkedQueuePool<object> pool, IList<FlyweightHolder<object>> holders )
		 {
			  ( ( IList<FlyweightHolder<object>> )holders ).AddRange( AcquireFromPool( pool, maxSize ) );

			  clock.Forward( 110, TimeUnit.MILLISECONDS );

			  // "Ring the bell" only on acquisition, of course.
			  ( ( IList<FlyweightHolder<object>> )holders ).AddRange( AcquireFromPool( pool, 1 ) );

			  foreach ( FlyweightHolder holder in holders )
			  {
					holder.release();
			  }
		 }

		 private LinkedQueuePool<object> GetLinkedQueuePool( StatefulMonitor stateMonitor, FakeClock clock, int minSize )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return new LinkedQueuePool<object>( minSize, object::new, new LinkedQueuePool.CheckStrategy_TimeoutCheckStrategy( 100, clock ), stateMonitor );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private <R> java.util.List<FlyweightHolder<R>> acquireFromPool(final LinkedQueuePool<R> pool, int times)
		 private IList<FlyweightHolder<R>> AcquireFromPool<R>( LinkedQueuePool<R> pool, int times )
		 {
			  IList<FlyweightHolder<R>> acquirers = new LinkedList<FlyweightHolder<R>>();
			  for ( int i = 0; i < times; i++ )
			  {
					FlyweightHolder<R> holder = new FlyweightHolder<R>( pool );
					acquirers.Add( holder );
					holder.Run();
			  }
			  return acquirers;
		 }

		 private class FlyweightHolder<R> : ThreadStart
		 {
			  internal readonly LinkedQueuePool<R> Pool;
			  internal R Resource;

			  internal FlyweightHolder( LinkedQueuePool<R> pool )
			  {
					this.Pool = pool;
			  }

			  public override void Run()
			  {
					Resource = Pool.acquire();
			  }

			  public virtual void Release()
			  {
					Pool.release( Resource );
			  }
		 }

		 private class StatefulMonitor : LinkedQueuePool.Monitor<object>
		 {
			  internal AtomicInteger CurrentPeakSize = new AtomicInteger( -1 );
			  internal AtomicInteger TargetSize = new AtomicInteger( -1 );
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal AtomicInteger CreatedConflict = new AtomicInteger( 0 );
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal AtomicInteger AcquiredConflict = new AtomicInteger( 0 );
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal AtomicInteger DisposedConflict = new AtomicInteger( 0 );

			  public override void UpdatedCurrentPeakSize( int currentPeakSize )
			  {
					this.CurrentPeakSize.set( currentPeakSize );
			  }

			  public override void UpdatedTargetSize( int targetSize )
			  {
					this.TargetSize.set( targetSize );
			  }

			  public override void Created( object @object )
			  {
					this.CreatedConflict.incrementAndGet();
			  }

			  public override void Acquired( object @object )
			  {
					this.AcquiredConflict.incrementAndGet();
			  }

			  public override void Disposed( object @object )
			  {
					this.DisposedConflict.incrementAndGet();
			  }
		 }

		 private class FakeClock : System.Func<long>
		 {
			  internal long Time;

			  public override long AsLong
			  {
				  get
				  {
						return Time;
				  }
			  }

			  public virtual void Forward( long amount, TimeUnit timeUnit )
			  {
					Time = Time + timeUnit.toMillis( amount );
			  }
		 }
	}

}