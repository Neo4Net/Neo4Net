using System.Collections.Concurrent;
using System.Collections.Generic;

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

	/// <summary>
	/// A concurrent pool that attempts to use thread-local pooling (puddles!) rather than a single big pool of objects to
	/// lower contention. Falls back to a delegate pool if no local object can be claimed. This means this can be used as
	/// a wrapper around a contended pool to alleviate contention in cases where threads generally claim and release one object
	/// at a time.
	/// </summary>
	public class MarshlandPool<T> : Pool<T>
	{
		 /*
		  * This is a somewhat complicated class. What it does is to keep a single-slot local pool for each thread that
		  * uses it, to allow very rapid claim calls that don't need to communicate with other threads. However, this is
		  * dangerous, since pooled objects may then be lost when threads die.
		  *
		  * To mitigate this, the local slots are tracked by phantom references, which allows us to use a reference queue
		  * to find objects that used to "belong" to now-dead threads.
		  *
		  * So, our algo for claiming is:
		  *  - Check thread local for available object.
		  *  - If none found, check the reference queue
		  *  - If none found, use the delegate pool.
		  */

		 private readonly Pool<T> @delegate;

		 // Used to reclaim objects from dead threads
		 private readonly ISet<LocalSlotReference<T>> _slotReferences = newSetFromMap( new ConcurrentDictionary<LocalSlotReference<T>>() );
		 private readonly ReferenceQueue<LocalSlot<T>> _objectsFromDeadThreads = new ReferenceQueue<LocalSlot<T>>();

		 private readonly ThreadLocal<LocalSlot<T>> _puddle = ThreadLocal.withInitial(() =>
		 {
		  LocalSlot<T> localSlot = new LocalSlot<T>( _objectsFromDeadThreads );
		  _slotReferences.Add( localSlot.SlotWeakReference );
		  return localSlot;
		 });

		 public MarshlandPool( Pool<T> delegatePool )
		 {
			  this.@delegate = delegatePool;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public T acquire()
		 public override T Acquire()
		 {
			  // Try and get it from the thread local
			  LocalSlot<T> localSlot = _puddle.get();

			  T obj = localSlot.LocalSlotObject;
			  if ( obj != default( T ) )
			  {
					localSlot.Set( default( T ) );
					return obj;
			  }

			  // Try the reference queue, containing objects from dead threads
			  LocalSlotReference<T> slotReference = ( LocalSlotReference<T> ) _objectsFromDeadThreads.poll();
			  if ( slotReference != null && slotReference.LocalSlotReferenceObject != default( T ) )
			  {
					_slotReferences.remove( slotReference ); // remove from old threads
					return slotReference.LocalSlotReferenceObject;
			  }

			  // Fall back to the delegate pool
			  return @delegate.Acquire();
		 }

		 public override void Release( T obj )
		 {
			  // Return it locally if possible
			  LocalSlot<T> localSlot = _puddle.get();

			  if ( localSlot.LocalSlotObject == default( T ) )
			  {
					localSlot.Set( obj );
			  }
			  else // Fall back to the delegate pool
			  {
					@delegate.Release( obj );
			  }
		 }

		 /// <summary>
		 /// Dispose of all objects in this pool, releasing them back to the delegate pool
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public void close()
		 public override void Close()
		 {
			  foreach ( LocalSlotReference<T> slotReference in _slotReferences )
			  {
					LocalSlot<T> slot = slotReference.get();
					if ( slot != null )
					{
						 T obj = slot.LocalSlotObject;
						 if ( obj != default( T ) )
						 {
							  slot.Set( default( T ) );
							  @delegate.Release( obj );
						 }
					}
			  }

			  for ( LocalSlotReference<T> reference; ( reference = ( LocalSlotReference<T> ) _objectsFromDeadThreads.poll() ) != null; )
			  {
					T obj = reference.localSlotReferenceObject;
					if ( obj != default( T ) )
					{
						 @delegate.Release( obj );
					}
			  }
		 }

		 /// <summary>
		 /// Container for the "puddle", the small local pool each thread keeps.
		 /// </summary>
		 private sealed class LocalSlot<T>
		 {
			  internal T LocalSlotObject;
			  internal readonly LocalSlotReference<T> SlotWeakReference;

			  internal LocalSlot( ReferenceQueue<LocalSlot<T>> referenceQueue )
			  {
					SlotWeakReference = new LocalSlotReference<T>( this, referenceQueue );
			  }

			  public void Set( T obj )
			  {
					SlotWeakReference.localSlotReferenceObject = obj;
					this.LocalSlotObject = obj;
			  }
		 }

		 /// <summary>
		 /// This is used to trigger the GC to notify us whenever the thread local has been garbage collected.
		 /// </summary>
		 private sealed class LocalSlotReference<T> : WeakReference<LocalSlot<T>>
		 {
			  internal T LocalSlotReferenceObject;

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: private LocalSlotReference(LocalSlot<T> referent, ReferenceQueue<? super LocalSlot<T>> q)
			  internal LocalSlotReference<T1>( LocalSlot<T> referent, ReferenceQueue<T1> q ) : base( referent, q )
			  {
			  }
		 }
	}

}