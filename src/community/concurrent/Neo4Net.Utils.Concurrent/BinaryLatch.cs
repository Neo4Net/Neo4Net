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

	using UnsafeUtil = Neo4Net.@unsafe.Impl.@internal.Dragons.UnsafeUtil;

	/// <summary>
	/// This class is similar in many ways to a CountDownLatch(1).
	/// <para>
	/// The main difference is that instances of this specialized latch implementation are much quicker to allocate and
	/// construct. Each instance also takes up less memory on the heap, and enqueueing wait nodes on the latch is faster.
	/// </para>
	/// <para>
	/// There are two reasons why this class is faster to construct: 1. it performs no volatile write during its
	/// construction, and 2. it does not need to allocate an internal Sync object, like CountDownLatch does.
	/// </para>
	/// </summary>
	public class BinaryLatch
	{
		 private class Node
		 {
			  internal volatile Node Next;
		 }

		 private sealed class Waiter : Node
		 {
			  internal readonly Thread WaitingThread = Thread.CurrentThread;
			  internal volatile sbyte State;
		 }

		 private static readonly long _stackOffset = UnsafeUtil.getFieldOffset( typeof( BinaryLatch ), "stack" );
		 private static readonly Node _end = new Node();
		 private static readonly Node _released = new Node();
		 private const sbyte WAITER_STATE_SUCCESSOR = 1;
		 private const sbyte WAITER_STATE_RELEASED = 2;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private volatile Node stack;
		 private volatile Node _stack; // written to via unsafe

		 /// <summary>
		 /// Release the latch, thereby unblocking all current and future calls to <seealso cref="await()"/>.
		 /// </summary>
		 public virtual void Release()
		 {
			  // Once the release sentinel is on the stack, it can never (observably) leave.
			  // Waiters might accidentally remove the released sentinel from the stack for brief periods of time, but then
			  // they are required to fix the situation and put it back.
			  // Atomically swapping the release sentinel onto the stack will give us back all the waiters, if any.
			  Node waiters = ( Node ) UnsafeUtil.getAndSetObject( this, _stackOffset, _released );
			  if ( waiters == null )
			  {
					// There are no waiters to unpark, so don't bother.
					return;
			  }
			  UnparkSuccessor( waiters );
		 }

		 private void UnparkSuccessor( Node waiters )
		 {
			  if ( waiters.GetType() == typeof(Waiter) )
			  {
					Waiter waiter = ( Waiter ) waiters;
					waiter.State = WAITER_STATE_SUCCESSOR;
					LockSupport.unpark( waiter.WaitingThread );
			  }
		 }

		 /// <summary>
		 /// Wait for the latch to be released, blocking the current thread if necessary.
		 /// <para>
		 /// This method returns immediately if the latch has already been released.
		 /// </para>
		 /// </summary>
		 public virtual void Await()
		 {
			  // Put in a local variable to avoid volatile reads we don't need.
			  Node state = _stack;
			  if ( state != _released )
			  {
					// The latch hasn't obviously already been released, so we want to add a waiter to the stack. Trouble is,
					// we might race with release here, so we need to re-check for release after we've modified the stack.
					Waiter waiter = new Waiter();
					state = ( Node ) UnsafeUtil.getAndSetObject( this, _stackOffset, waiter );
					if ( state == _released )
					{
						 // If we get 'released' back from the swap, then we raced with release, and it is our job to put the
						 // released sentinel back. Doing so can, however, return more waiters that have added themselves in
						 // the mean time. If we find such waiters, then we must make sure to unpark them. Note that we will
						 // never get a null back from this swap, because we at least added our own waiter earlier.
						 Node others = ( Node ) UnsafeUtil.getAndSetObject( this, _stackOffset, _released );
						 // Set our next pointer to 'released' as a signal to other threads who might be going through the
						 // stack in the isReleased check.
						 waiter.Next = _released;
						 UnparkAll( others );
					}
					else
					{
						 // It looks like the latch hasn't yet been released, so we are going to park. Before that, we must
						 // assign a non-null value to our next pointer, so other threads will know that we have been properly
						 // enqueued. We use the 'end' sentinel as a marker when there's otherwise no other next node.
						 waiter.Next = state == null ? _end : state;
						 do
						 {
							  // Park may wake up spuriously, so we have to loop on it until we observe from the state of the
							  // stack, that the latch has been released.
							  LockSupport.park( this );
						 } while ( !IsReleased( waiter ) );
					}
			  }
		 }

		 private bool IsReleased( Waiter waiter )
		 {
			  // If we are the most recently enqueued waiter on the stack before the release, then that makes us the
			  // successor. As the successor, it is our job to wake up all the other threads. We can *only* become the
			  // successor if the latch has been released, so there's no need to check anything else in this case.
			  if ( waiter.State == WAITER_STATE_SUCCESSOR )
			  {
					UnparkAll( waiter.Next );
					return true;
			  }

			  // Otherwise we have to go through the entire stack and look for the 'released' sentinel, since we might be
			  // racing with the 'state == released' branch in await.
			  Node state = _stack;
			  do
			  {
					if ( state == _released )
					{
						 // We've been released!
						 if ( waiter.State != WAITER_STATE_RELEASED )
						 {
							  // But it doesn't look like someone else is unparking the threads, so let's do that.
							  // This can happen if we missed the signal to become the successor.
							  UnparkAll( waiter.Next );
						 }
						 return true;
					}

					Node next;
					do
					{
						 // We loop on reading the next pointer because we might observe an enqueued node before its next
						 // pointer has been properly assigned. This is a benign race because we know that the next pointer of a
						 // properly enqueued node is never null.
						 next = state.Next;
					} while ( next == null );
					state = next;
			  } while ( state != _end );
			  // Reaching the end of the stack without seeing 'released' means we're not released.
			  return false;
		 }

		 private void UnparkAll( Node waiters )
		 {
			  // If we find a node that is not a waiter, then it is either 'end' or 'released'. Looking at the type pointer
			  // is the cheapest way to make this check.
			  while ( waiters.GetType() == typeof(Waiter) )
			  {
					Waiter waiter = ( Waiter ) waiters;
					waiter.State = WAITER_STATE_RELEASED;
					LockSupport.unpark( waiter.WaitingThread );
					Node next;
					do
					{
						 // Just like in isReleased, loop if the next pointer is null.
						 next = waiters.Next;
					} while ( next == null );
					waiters = next;
			  }
		 }
	}

}