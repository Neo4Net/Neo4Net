using System.Collections.Concurrent;
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
namespace Neo4Net.Kernel.impl.locking
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Thread.currentThread;

	/// <summary>
	/// Fairness in this implementation is achieved through a <seealso cref="OwnerQueueElement queue"/> of waiting threads for
	/// <seealso cref="locks each lock"/>. It guarantees that readers are not allowed before a waiting writer by not differentiating
	/// between readers and writers, the locks are mutex locks, but reentrant from the same thread.
	/// </summary>
	public sealed class ReentrantLockService : AbstractLockService<ReentrantLockService.OwnerQueueElement<Thread>>
	{
		 private readonly ConcurrentMap<LockedEntity, OwnerQueueElement<Thread>> _locks = new ConcurrentDictionary<LockedEntity, OwnerQueueElement<Thread>>();
		 private readonly long _maxParkNanos;

		 internal int LockCount()
		 {
			  return _locks.size();
		 }

		 public ReentrantLockService() : this(1, TimeUnit.MILLISECONDS)
		 {
		 }

		 public ReentrantLockService( long maxParkTime, TimeUnit unit )
		 {
			  this._maxParkNanos = unit.toNanos( maxParkTime );
		 }

		 protected internal override OwnerQueueElement<Thread> Acquire( LockedEntity key )
		 {
			  OwnerQueueElement<Thread> suggestion = new OwnerQueueElement<Thread>( currentThread() );
			  for ( ; ; )
			  {
					OwnerQueueElement<Thread> owner = _locks.putIfAbsent( key, suggestion );
					if ( owner == null )
					{ // Our suggestion was accepted, we got the lock
						 return suggestion;
					}

					Thread other = owner.Owner;
					if ( other == currentThread() )
					{ // the lock has been handed to us (or we are re-entering), claim it!
						 owner.Count++;
						 return owner;
					}

					// Make sure that we only add to the queue once, and if that addition fails (because the queue is dead
					// - i.e. has been removed from the map), retry form the top of the loop immediately.
					if ( suggestion.Head == suggestion ) // true if enqueue() has not been invoked (i.e. first time around)
					{ // otherwise it has already been enqueued, and we are in a spurious (or timed) wake up
						 if ( !owner.Enqueue( suggestion ) )
						 {
							  continue; // the lock has already been released, the queue is dead, retry!
						 }
					}
					parkNanos( key, _maxParkNanos );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("SynchronizationOnLocalVariableOrMethodParameter") protected void release(LockedEntity key, OwnerQueueElement<Thread> ownerQueueElement)
		 protected internal override void Release( LockedEntity key, OwnerQueueElement<Thread> ownerQueueElement )
		 {
			  if ( 0 == --ownerQueueElement.Count )
			  {
					Thread nextThread;
					lock ( ownerQueueElement )
					{
						 nextThread = ownerQueueElement.Dequeue();
						 if ( nextThread == currentThread() )
						 { // no more threads in the queue, remove this list
							  _locks.remove( key, ownerQueueElement ); // done under synchronization to honour definition of 'dead'
							  nextThread = null; // to make unpark() a no-op.
						 }
					}
					unpark( nextThread );
			  }
		 }

		 /// <summary>
		 /// Element in a queue of owners. Contains two fields <seealso cref="head"/> and <seealso cref="tail"/> which form the queue.
		 /// 
		 /// Example queue with 3 members:
		 /// 
		 /// <pre>
		 /// locks -> [H]--+ <+
		 ///          [T]  |  |
		 ///          ^|   V  |
		 ///          ||  [H]-+
		 ///          ||  [T] ^
		 ///          ||   |  |
		 ///          ||   V  |
		 ///          |+->[H]-+
		 ///          +---[T]
		 /// </pre> </summary>
		 /// @param <OWNER> Type of the object that owns (or wishes to own) the lock.
		 ///               In practice this is always <seealso cref="System.Threading.Thread"/>, only a parameter for testing purposes. </param>
		 internal sealed class OwnerQueueElement<OWNER>
		 {
			 internal bool InstanceFieldsInitialized = false;

			 internal void InitializeInstanceFields()
			 {
				 Head = this;
				 Tail = this;
			 }

			  internal volatile OWNER Owner;
			  internal int Count = 1; // does not need to be volatile, only updated by the owning thread.

			  internal OwnerQueueElement( OWNER owner )
			  {
				  if ( !InstanceFieldsInitialized )
				  {
					  InitializeInstanceFields();
					  InstanceFieldsInitialized = true;
				  }
					this.Owner = owner;
			  }

			  /// <summary>
			  /// In the first element, head will point to the next waiting element, and tail is where we enqueue new elements.
			  /// In the waiting elements, head will point to the first element, and tail to the next element.
			  /// </summary>
			  internal OwnerQueueElement<OWNER> Head;
			  internal OwnerQueueElement<OWNER> Tail;

			  /// <summary>
			  /// Return true if the item was enqueued, or false if this LockOwner is dead.
			  /// A dead LockOwner is no longer reachable from the map, and so no longer participates in the lock.
			  /// </summary>
			  internal bool Enqueue( OwnerQueueElement<OWNER> last )
			  {
				  lock ( this )
				  {
						if ( Owner == default( OWNER ) )
						{
							 return false; // don't enqueue into a dead queue
						}
						last.Head = this;
						last.Tail = this;
						Tail.tail = last;
						this.Tail = last;
						if ( Head == this )
						{
							 Head = last;
						}
						return true;
				  }
			  }

			  internal OWNER Dequeue()
			  {
				  lock ( this )
				  {
						OwnerQueueElement<OWNER> first = this.Head;
						( this.Head = first.Tail ).head = this;
						first.Tail = this;
						if ( this.Head == this )
						{
							 this.Tail = this; // don't leave junk references around!
						}
						try
						{
							 return this.Owner = first.Owner;
						}
						finally
						{
							 first.Owner = default( OWNER ); // mark 'first' as dead.
						}
				  }
			  }

			  public override string ToString()
			  {
					return string.Format( "{0}*{1}", Count, Owner );
			  }
		 }
	}

}