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
namespace Neo4Net.Kernel.impl.locking
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using ThreadRepository = Neo4Net.Test.rule.concurrent.ThreadRepository;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class ReentrantLockServiceTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.concurrent.ThreadRepository threads = new org.Neo4Net.test.rule.concurrent.ThreadRepository(5, java.util.concurrent.TimeUnit.SECONDS);
		 public readonly ThreadRepository Threads = new ThreadRepository( 5, TimeUnit.SECONDS );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFormLinkedListOfWaitingLockOwners()
		 public virtual void ShouldFormLinkedListOfWaitingLockOwners()
		 {
			  // given
			  ReentrantLockService.OwnerQueueElement<int> queue = new ReentrantLockService.OwnerQueueElement<int>( 0 );
			  ReentrantLockService.OwnerQueueElement<int> element1 = new ReentrantLockService.OwnerQueueElement<int>( 1 );
			  ReentrantLockService.OwnerQueueElement<int> element2 = new ReentrantLockService.OwnerQueueElement<int>( 2 );
			  ReentrantLockService.OwnerQueueElement<int> element3 = new ReentrantLockService.OwnerQueueElement<int>( 3 );
			  ReentrantLockService.OwnerQueueElement<int> element4 = new ReentrantLockService.OwnerQueueElement<int>( 4 );

			  // when
			  queue.Enqueue( element1 );
			  // then
			  assertEquals( 1, queue.Dequeue() );

			  // when
			  queue.Enqueue( element2 );
			  queue.Enqueue( element3 );
			  queue.Enqueue( element4 );
			  // then
			  assertEquals( 2, queue.Dequeue() );
			  assertEquals( 3, queue.Dequeue() );
			  assertEquals( 4, queue.Dequeue() );
			  assertEquals( "should get the current element when dequeuing the current head", 4, queue.Dequeue() );
			  assertNull( "should get null when dequeuing from a dead list", queue.Dequeue() );
			  assertNull( "should get null continuously when dequeuing from a dead list", queue.Dequeue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowReEntrance() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowReEntrance()
		 {
			  // given
			  LockService locks = new ReentrantLockService();

			  ThreadRepository.Events events = Threads.events();
			  LockNode lock1once = new LockNode( locks, 1 );
			  LockNode lock1again = new LockNode( locks, 1 );
			  LockNode lock1inOtherThread = new LockNode( locks, 1 );

			  ThreadRepository.Signal lockedOnce = Threads.signal();
			  ThreadRepository.Signal ready = Threads.signal();

			  // when
			  Threads.execute( lock1once, ready.Await(), lockedOnce, lock1again, events.Trigger("Double Locked"), lock1once.release, lock1again.release );
			  Threads.execute( ready, lockedOnce.Await(), lock1inOtherThread, events.Trigger("Other Thread"), lock1inOtherThread.release );

			  // then
			  events.AssertInOrder( "Double Locked", "Other Thread" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBlockOnLockedLock() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBlockOnLockedLock()
		 {
			  // given
			  LockService locks = new ReentrantLockService();
			  LockNode lockSameNode = new LockNode( locks, 17 );
			  ThreadRepository.Events events = Threads.events();
			  ThreadRepository.Signal ready = Threads.signal();

			  // when
			  using ( Lock ignored = locks.AcquireNodeLock( 17, LockService_LockType.WriteLock ) )
			  {
					ThreadRepository.ThreadInfo thread = Threads.execute( ready, lockSameNode, events.Trigger( "locked" ), lockSameNode.release );
					ready.AwaitNow();

					// then
					assertTrue( AwaitParked( thread, 5, TimeUnit.SECONDS ) );
					assertTrue( events.Snapshot().Count == 0 );
			  }
			  events.AssertInOrder( "locked" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLeaveResidualLockStateAfterAllLocksHaveBeenReleased()
		 public virtual void ShouldNotLeaveResidualLockStateAfterAllLocksHaveBeenReleased()
		 {
			  // given
			  ReentrantLockService locks = new ReentrantLockService();

			  // when
			  locks.AcquireNodeLock( 42, LockService_LockType.WriteLock ).release();

			  // then
			  assertEquals( 0, locks.LockCount() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPresentLockStateInStringRepresentationOfLock()
		 public virtual void ShouldPresentLockStateInStringRepresentationOfLock()
		 {
			  // given
			  LockService locks = new ReentrantLockService();
			  Lock first;
			  Lock second;

			  // when
			  using ( Lock @lock = first = locks.AcquireNodeLock( 666, LockService_LockType.WriteLock ) )
			  {
					// then
					assertEquals( "LockedNode[id=666; HELD_BY=1*" + Thread.CurrentThread + "]", @lock.ToString() );

					// when
					using ( Lock inner = second = locks.AcquireNodeLock( 666, LockService_LockType.WriteLock ) )
					{
						 assertEquals( "LockedNode[id=666; HELD_BY=2*" + Thread.CurrentThread + "]", @lock.ToString() );
						 assertEquals( @lock.ToString(), inner.ToString() );
					}

					// then
					assertEquals( "LockedNode[id=666; HELD_BY=1*" + Thread.CurrentThread + "]", @lock.ToString() );
					assertEquals( "LockedNode[id=666; RELEASED]", second.ToString() );
			  }

			  // then
			  assertEquals( "LockedNode[id=666; RELEASED]", first.ToString() );
			  assertEquals( "LockedNode[id=666; RELEASED]", second.ToString() );
		 }

		 private class LockNode : ThreadRepository.Task
		 {
			  internal readonly LockService Locks;
			  internal readonly long NodeId;
			  internal Lock Lock;

			  internal LockNode( LockService locks, long nodeId )
			  {
					this.Locks = locks;
					this.NodeId = nodeId;
			  }

			  internal readonly ThreadRepository.Task release = new TaskAnonymousInnerClass();

			  private class TaskAnonymousInnerClass : ThreadRepository.Task
			  {
				  public void perform()
				  {
						outerInstance.@lock.release();
				  }
			  }

			  public override void Perform()
			  {
					this.Lock = Locks.acquireNodeLock( NodeId, LockService_LockType.WriteLock );
			  }
		 }

		 private static bool AwaitParked( ThreadRepository.ThreadInfo thread, long timeout, TimeUnit unit )
		 {
			  bool parked = false;
			  for ( long end = DateTimeHelper.CurrentUnixTimeMillis() + unit.toMillis(timeout); DateTimeHelper.CurrentUnixTimeMillis() < end; )
			  {
					StackTraceElement frame = thread.StackTrace[0];
					if ( "park".Equals( frame.MethodName ) && frame.ClassName.EndsWith( "Unsafe" ) )
					{
						 if ( thread.State.name().EndsWith("WAITING") )
						 {
							  parked = true;
							  break;
						 }
					}
			  }
			  return parked;
		 }
	}

}