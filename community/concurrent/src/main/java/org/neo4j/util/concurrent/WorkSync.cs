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
namespace Org.Neo4j.Util.concurrent
{

	/// <summary>
	/// Turns multi-threaded unary work into single-threaded stack work.
	/// <para>
	/// The technique used here is inspired in part both by the Flat Combining
	/// concept from Hendler, Incze, Shavit &amp; Tzafrir, and in part by the
	/// wait-free linked queue design by Vyukov.
	/// </para>
	/// <para>
	/// In a sense, this turns many small, presumably concurrent, pieces of work
	/// into fewer, larger batches of work, that is then applied to the material
	/// under synchronisation.
	/// </para>
	/// <para>
	/// Obviously this only makes sense for work that a) can be combined, and b)
	/// where the performance improvements from batching effects is large enough
	/// to overcome the overhead of collecting and batching up the work units.
	/// </para>
	/// </summary>
	/// <seealso cref= Work </seealso>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({"unchecked", "NumberEquality"}) public class WorkSync<Material, W extends Work<Material,W>>
	public class WorkSync<Material, W> where W : Work<Material,W>
	{
		 private readonly Material _material;
		 private readonly AtomicReference<WorkUnit<Material, W>> _stack;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private static final WorkUnit<?,?> stackEnd = new WorkUnit<>(null, null);
		 private static readonly WorkUnit<object, ?> _stackEnd = new WorkUnit<object, ?>( null, null );
		 private readonly AtomicReference<Thread> @lock;

		 /// <summary>
		 /// Create a new WorkSync that will synchronize the application of work to
		 /// the given material.
		 /// </summary>
		 /// <param name="material"> The material we want to apply work to, in a thread-safe
		 /// way. </param>
		 public WorkSync( Material material )
		 {
			  this._material = material;
			  this._stack = new AtomicReference<WorkUnit<Material, W>>( ( WorkUnit<Material, W> ) _stackEnd );
			  this.@lock = new AtomicReference<Thread>();
		 }

		 /// <summary>
		 /// Apply the given work to the material in a thread-safe way, possibly by
		 /// combining it with other work.
		 /// </summary>
		 /// <param name="work"> The work to be done. </param>
		 /// <exception cref="ExecutionException"> if this thread ends up performing the piled up work,
		 /// and any work unit in the pile throws an exception. Thus the current thread is not
		 /// guaranteed to observe any exception its unit of work might throw, since the
		 /// exception will be thrown in whichever thread that ends up actually performing the work. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void apply(W work) throws java.util.concurrent.ExecutionException
		 public virtual void Apply( W work )
		 {
			  // Schedule our work on the stack.
			  WorkUnit<Material, W> unit = EnqueueWork( work );

			  // Try grabbing the lock to do all the work, until our work unit
			  // has been completed.
			  int tryCount = 0;
			  do
			  {
					tryCount++;
					CheckFailure( TryDoWork( unit, tryCount, true ) );
			  } while ( !unit.Done );
		 }

		 /// <summary>
		 /// Apply the given work to the material in a thread-safe way, possibly asynchronously if contention is observed
		 /// with other threads, and possibly by combining it with other work.
		 /// <para>
		 /// The work will be applied immediately, if no other thread is contending for the material. Otherwise, the work
		 /// will be enqueued for later application, which may occur on the next call to <seealso cref="apply(Work)"/> on this
		 /// {@code WorkSync}, or the next call to <seealso cref="AsyncApply.await()"/> from an {@code AsyncApply} instance created
		 /// from this {@code WorkSync}. These calls, and thus the application of the enqueued work, may occur in an
		 /// arbitrary thread.
		 /// </para>
		 /// <para>
		 /// The returned <seealso cref="AsyncApply"/> instance is not thread-safe. If so desired, its ownership can be transferred to
		 /// other threads, but only in a way that ensures safe publication.
		 /// </para>
		 /// <para>
		 /// If the given work causes an exception to be thrown, then that exception will only be observed by the thread that
		 /// ultimately applies the work. Thus, exceptions caused by this work are not guaranteed to be associated with, or
		 /// made visible via, the returned <seealso cref="AsyncApply"/> instance.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="work"> The work to be done. </param>
		 /// <returns> An <seealso cref="AsyncApply"/> instance representing the enqueued - and possibly completed - work. </returns>
		 public virtual AsyncApply ApplyAsync( W work )
		 {
			  // Schedule our work on the stack.
			  WorkUnit<Material, W> unit = EnqueueWork( work );

			  // Apply the work if the lock is immediately available.
			  Exception initialThrowable = TryDoWork( unit, 100, false );

			  return new AsyncApplyAnonymousInnerClass( this, unit, initialThrowable );
		 }

		 private class AsyncApplyAnonymousInnerClass : AsyncApply
		 {
			 private readonly WorkSync<Material, W> _outerInstance;

			 private Org.Neo4j.Util.concurrent.WorkSync.WorkUnit<Material, W> _unit;
			 private Exception _initialThrowable;

			 public AsyncApplyAnonymousInnerClass( WorkSync<Material, W> outerInstance, Org.Neo4j.Util.concurrent.WorkSync.WorkUnit<Material, W> unit, Exception initialThrowable )
			 {
				 this.outerInstance = outerInstance;
				 this._unit = unit;
				 this._initialThrowable = initialThrowable;
				 throwable = initialThrowable;
			 }

			 internal Exception throwable;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void await() throws java.util.concurrent.ExecutionException
			 public void await()
			 {

				  outerInstance.checkFailure( throwable );
				  int tryCount = 0;
				  while ( !_unit.Done )
				  {
						tryCount++;
						outerInstance.checkFailure( throwable = outerInstance.tryDoWork( _unit, tryCount, true ) );
				  }
			 }
		 }

		 private WorkUnit<Material, W> EnqueueWork( W work )
		 {
			  WorkUnit<Material, W> unit = new WorkUnit<Material, W>( work, Thread.CurrentThread );
			  unit.Next = _stack.getAndSet( unit ); // benign race, see the batch.next read-loop in combine()
			  return unit;
		 }

		 private Exception TryDoWork( WorkUnit<Material, W> unit, int tryCount, bool block )
		 {
			  if ( TryLock( tryCount, unit, block ) )
			  {
					WorkUnit<Material, W> batch = GrabBatch();
					try
					{
						 return DoSynchronizedWork( batch );
					}
					finally
					{
						 Unlock();
						 UnparkAnyWaiters();
						 MarkAsDone( batch );
					}
			  }
			  return null;
		 }

		 private void UnparkAnyWaiters()
		 {
			  WorkUnit<Material, W> waiter = _stack.get();
			  if ( waiter != _stackEnd )
			  {
					waiter.Unpark();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void checkFailure(Throwable failure) throws java.util.concurrent.ExecutionException
		 private void CheckFailure( Exception failure )
		 {
			  if ( failure != null )
			  {
					throw new ExecutionException( failure );
			  }
		 }

		 private bool TryLock( int tryCount, WorkUnit<Material, W> unit, bool block )
		 {
			  if ( @lock.compareAndSet( null, Thread.CurrentThread ) )
			  {
					// Got the lock!
					return true;
			  }

			  // Did not get the lock, spend some time until our work has either been completed,
			  // or we get the lock.
			  if ( tryCount < 10 )
			  {
					// todo Java9: Thread.onSpinWait() ?
					Thread.yield();
			  }
			  else if ( block )
			  {
					unit.Park( 10, TimeUnit.MILLISECONDS );
			  }
			  return false;
		 }

		 private void Unlock()
		 {
			  if ( @lock.getAndSet( null ) != Thread.CurrentThread )
			  {
					throw new IllegalMonitorStateException( "WorkSync accidentally released a lock not owned by the current thread" );
			  }
		 }

		 private WorkUnit<Material, W> GrabBatch()
		 {
			  return _stack.getAndSet( ( WorkUnit<Material, W> ) _stackEnd );
		 }

		 private Exception DoSynchronizedWork( WorkUnit<Material, W> batch )
		 {
			  W combinedWork = Combine( batch );
			  Exception failure = null;

			  if ( combinedWork != default( W ) )
			  {
					try
					{
						 combinedWork.apply( _material );
					}
					catch ( Exception throwable )
					{
						 failure = throwable;
					}
			  }
			  return failure;
		 }

		 private W Combine( WorkUnit<Material, W> batch )
		 {
			  W result = default( W );
			  while ( batch != _stackEnd )
			  {
					if ( result == default( W ) )
					{
						 result = batch.Work;
					}
					else
					{
						 result = result.combine( batch.Work );
					}

					WorkUnit<Material, W> tmp = batch.Next;
					while ( tmp == null )
					{
						 // We may see 'null' via race, as work units are put on the
						 // stack before their 'next' pointers are updated. We just spin
						 // until we observe their volatile write to 'next'.
						 // todo Java9: Thread.onSpinWait() ?
						 Thread.yield();
						 tmp = batch.Next;
					}
					batch = tmp;
			  }
			  return result;
		 }

		 private void MarkAsDone( WorkUnit<Material, W> batch )
		 {
			  while ( batch != _stackEnd )
			  {
					batch.Complete();
					batch = batch.Next;
			  }
		 }

		 private class WorkUnit<Material, W> : AtomicInteger where W : Work<Material,W>
		 {
			  internal const int STATE_QUEUED = 0;
			  internal const int STATE_PARKED = 1;
			  internal const int STATE_DONE = 2;

			  internal readonly W Work;
			  internal readonly Thread Owner;
			  internal volatile WorkUnit<Material, W> Next;

			  internal WorkUnit( W work, Thread owner )
			  {
					this.Work = work;
					this.Owner = owner;
			  }

			  internal virtual void Park( long time, TimeUnit unit )
			  {
					if ( compareAndSet( STATE_QUEUED, STATE_PARKED ) )
					{
						 LockSupport.parkNanos( unit.toNanos( time ) );
						 compareAndSet( STATE_PARKED, STATE_QUEUED );
					}
			  }

			  internal virtual bool Done
			  {
				  get
				  {
						return get() == STATE_DONE;
				  }
			  }

			  internal virtual void Complete()
			  {
					int previousState = getAndSet( STATE_DONE );
					if ( previousState == STATE_PARKED )
					{
						 Unpark();
					}
			  }

			  internal virtual void Unpark()
			  {
					LockSupport.unpark( Owner );
			  }
		 }
	}

}