using System;

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
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Neo4Net.Function;
	using Resource = Neo4Net.Graphdb.Resource;
	using Barrier = Neo4Net.Test.Barrier;
	using Race = Neo4Net.Test.Race;
	using Neo4Net.Test.rule.concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.function.ThrowingAction.noop;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.Race.throwing;

	public class StoreCopyCheckPointMutexTest
	{
		 private static readonly ThrowingAction<IOException> _assertNotCalled = () => fail("Should not be called");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.concurrent.OtherThreadRule<Void> t2 = new org.neo4j.test.rule.concurrent.OtherThreadRule<>("T2");
		 public readonly OtherThreadRule<Void> T2 = new OtherThreadRule<Void>( "T2" );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.concurrent.OtherThreadRule<Void> t3 = new org.neo4j.test.rule.concurrent.OtherThreadRule<>("T3");
		 public readonly OtherThreadRule<Void> T3 = new OtherThreadRule<Void>( "T3" );

		 private readonly StoreCopyCheckPointMutex _mutex = new StoreCopyCheckPointMutex();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void checkPointShouldBlockStoreCopy() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CheckPointShouldBlockStoreCopy()
		 {
			  // GIVEN
			  using ( Resource @lock = _mutex.checkPoint() )
			  {
					// WHEN
					T2.execute( state => _mutex.storeCopy( noop() ) );

					// THEN
					T2.get().waitUntilWaiting(details => details.isAt(typeof(StoreCopyCheckPointMutex), "storeCopy"));
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void checkPointShouldBlockAnotherCheckPoint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CheckPointShouldBlockAnotherCheckPoint()
		 {
			  // GIVEN
			  using ( Resource @lock = _mutex.checkPoint() )
			  {
					// WHEN
					T2.execute( state => _mutex.checkPoint() );

					// THEN
					T2.get().waitUntilWaiting(details => details.isAt(typeof(StoreCopyCheckPointMutex), "checkPoint"));
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void storeCopyShouldBlockCheckPoint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StoreCopyShouldBlockCheckPoint()
		 {
			  // GIVEN
			  using ( Resource @lock = _mutex.storeCopy( noop() ) )
			  {
					// WHEN
					T2.execute( state => _mutex.checkPoint() );

					// THEN
					T2.get().waitUntilWaiting(details => details.isAt(typeof(StoreCopyCheckPointMutex), "checkPoint"));
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void storeCopyShouldHaveTryCheckPointBackOff() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StoreCopyShouldHaveTryCheckPointBackOff()
		 {
			  // GIVEN
			  using ( Resource @lock = _mutex.storeCopy( noop() ) )
			  {
					// WHEN
					assertNull( _mutex.tryCheckPoint() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void storeCopyShouldAllowAnotherStoreCopy() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StoreCopyShouldAllowAnotherStoreCopy()
		 {
			  // GIVEN
			  using ( Resource @lock = _mutex.storeCopy( noop() ) )
			  {
					// WHEN
					using ( Resource otherLock = _mutex.storeCopy( noop() ) )
					{
						 // THEN good
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void storeCopyShouldAllowAnotherStoreCopyButOnlyFirstShouldPerformBeforeAction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StoreCopyShouldAllowAnotherStoreCopyButOnlyFirstShouldPerformBeforeAction()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") org.neo4j.function.ThrowingAction<java.io.IOException> action = mock(org.neo4j.function.ThrowingAction.class);
			  ThrowingAction<IOException> action = mock( typeof( ThrowingAction ) );
			  using ( Resource @lock = _mutex.storeCopy( action ) )
			  {
					verify( action, times( 1 ) ).apply();

					// WHEN
					using ( Resource otherLock = _mutex.storeCopy( action ) )
					{
						 // THEN good
						 verify( action, times( 1 ) ).apply();
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleMultipleConcurrentStoreCopyWhenBeforeActionPerformsCheckPoint() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleMultipleConcurrentStoreCopyWhenBeforeActionPerformsCheckPoint()
		 {
			  // GIVEN a check-point action which asserts calls to it along the way
			  CheckPointingAction checkPointingAction = new CheckPointingAction( _mutex );
			  for ( int i = 0; i < 2; i++ )
			  {
					// Start first store-copy and assert that the check-point action is triggered
					Resource firstLock = _mutex.storeCopy( checkPointingAction );
					assertNotNull( checkPointingAction.Lock );

					// A second store-copy starts while the first is still going
					Resource secondLock = _mutex.storeCopy( checkPointingAction );

					// The first store-copy completes
					firstLock.Close();

					// A third store-copy starts and completes
					Resource thirdLock = _mutex.storeCopy( checkPointingAction );
					thirdLock.Close();

					// Second store-copy completes
					secondLock.Close();
					checkPointingAction.Unlock();

					// Go another round, now that the check-point action has been reset.
					// Next round will assert that the mutex got the counting of store-copy jobs right
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleMultipleConcurrentStoreCopyRequests() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleMultipleConcurrentStoreCopyRequests()
		 {
			  // GIVEN
			  Race race = new Race();
			  CountingAction action = new CountingAction();
			  int threads = Runtime.Runtime.availableProcessors() * 10;
			  race.AddContestants(threads, throwing(() =>
			  {
				ParkARandomWhile();
				using ( Resource @lock = _mutex.storeCopy( action ) )
				{
					 ParkARandomWhile();
				}
			  }));
			  race.Go();

			  // THEN
			  // It's hard to make predictions about what should have been seen. Most importantly is that
			  // The lock doesn't hang any requests and that number of calls to the action less than number of threads
			  assertThat( action.Count(), lessThan(threads) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPropagateStoreCopyActionFailureToOtherStoreCopyRequests() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPropagateStoreCopyActionFailureToOtherStoreCopyRequests()
		 {
			  // GIVEN
			  Neo4Net.Test.Barrier_Control barrier = new Neo4Net.Test.Barrier_Control();
			  IOException controlledFailure = new IOException( "My own fault" );
			  AtomicReference<Future<object>> secondRequest = new AtomicReference<Future<object>>();
			  ThrowingAction<IOException> controllableAndFailingAction = () =>
			  {
				// Now that we know we're first, start the second request...
				secondRequest.set( T3.execute( state => _mutex.storeCopy( _assertNotCalled ) ) );
				// ...and wait for it to reach its destination
				barrier.AwaitUninterruptibly();
				try
				{
					 // OK, second request has made progress into the request, so we can now produce our failure
					 throw controlledFailure;
				}
				finally
				{
					 barrier.Release();
				}
			  };

			  Future<object> firstRequest = T2.execute( state => _mutex.storeCopy( controllableAndFailingAction ) );
			  while ( secondRequest.get() == null )
			  {
					ParkARandomWhile();
			  }
			  T3.get().waitUntilWaiting(details => details.isAt(typeof(StoreCopyCheckPointMutex), "waitForFirstStoreCopyActionToComplete"));

			  // WHEN
			  barrier.Reached();

			  // THEN
			  try
			  {
					firstRequest.get();
			  }
			  catch ( ExecutionException e )
			  {
					assertSame( controlledFailure, e.InnerException );
			  }
			  try
			  {
					secondRequest.get().get();
			  }
			  catch ( ExecutionException e )
			  {
					Exception cooperativeActionFailure = e.InnerException;
					assertThat( cooperativeActionFailure.Message, containsString( "Co-operative" ) );
					assertSame( controlledFailure, cooperativeActionFailure.InnerException );
			  }

			  // WHEN afterwards trying another store-copy
			  CountingAction action = new CountingAction();
			  using ( Resource @lock = _mutex.storeCopy( action ) )
			  {
					// THEN
					assertEquals( 1, action.Count() );
			  }
		 }

		 private static void ParkARandomWhile()
		 {
			  LockSupport.parkNanos( MILLISECONDS.toNanos( ThreadLocalRandom.current().Next(10) ) );
		 }

		 private class CheckPointingAction : ThrowingAction<IOException>
		 {
			  internal readonly StoreCopyCheckPointMutex Mutex;
			  internal Resource Lock;

			  internal CheckPointingAction( StoreCopyCheckPointMutex mutex )
			  {
					this.Mutex = mutex;
			  }

			  public override void Apply()
			  {
					assertNull( Lock );
					Lock = Mutex.checkPoint();
			  }

			  internal virtual void Unlock()
			  {
					assertNotNull( Lock );
					Lock.close();
					Lock = null;
			  }
		 }

		 private class CountingAction : ThrowingAction<IOException>
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly AtomicInteger CountConflict = new AtomicInteger();

			  public override void Apply()
			  {
					ParkARandomWhile();
					CountConflict.incrementAndGet();
			  }

			  internal virtual int Count()
			  {
					return CountConflict.get();
			  }
		 }
	}

}