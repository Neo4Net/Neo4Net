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
namespace Neo4Net.Kernel.impl.scheduler
{

	using Group = Neo4Net.Scheduler.Group;
	using CancelListener = Neo4Net.Scheduler.CancelListener;
	using JobHandle = Neo4Net.Scheduler.JobHandle;
	using BinaryLatch = Neo4Net.Util.concurrent.BinaryLatch;

	/// <summary>
	/// The JobHandle implementation for jobs scheduled with the <seealso cref="TimeBasedTaskScheduler"/>.
	/// <para>
	/// As the handle gets scheduled, it transitions through various states:
	/// <ul>
	/// <li>The handle is initially in the RUNNABLE state, which means that it is ready to be executed but isn't
	/// scheduled to do so yet.</li>
	/// <li>When it gets scheduled, it transitions into the SUBMITTED state, and remains there until it has finished
	/// executing.</li>
	/// <li>A handle that is in the SUBMITTED state cannot be submitted again, even if it comes due.</li>
	/// <li>A handle that is both due and SUBMITTED is <em>overdue</em>, and its execution will be delayed until it
	/// changes out of the SUBMITTED state.</li>
	/// <li>If a scheduled handle successfully finishes its execution, it will transition back to the RUNNABLE state.</li>
	/// <li>If an exception is thrown during the execution, then the handle transitions to the FAILED state, which is a
	/// terminal state.</li>
	/// <li>Failed handles will not be scheduled again.</li>
	/// </ul>
	/// </para>
	/// </summary>
	internal sealed class ScheduledJobHandle : AtomicInteger, JobHandle
	{
		 // We extend AtomicInteger to inline our state field.
		 // These are the possible state values:
		 private const int RUNNABLE = 0;
		 private const int SUBMITTED = 1;
		 private const int FAILED = 2;

		 // Access is synchronised via the PriorityBlockingQueue in TimeBasedTaskScheduler:
		 // - Write to this field happens before the handle is added to the queue.
		 // - Reads of this field happens after the handle has been read from the queue.
		 // - Reads of this field for the purpose of ordering the queue are either thread local,
		 //   or happens after the relevant handles have been added to the queue.
		 internal long NextDeadlineNanos;

		 private readonly Group _group;
		 private readonly CopyOnWriteArrayList<CancelListener> _cancelListeners;
		 private readonly BinaryLatch _handleRelease;
		 private readonly ThreadStart _task;
		 private volatile JobHandle _latestHandle;
		 private volatile Exception _lastException;

		 internal ScheduledJobHandle( TimeBasedTaskScheduler scheduler, Group group, ThreadStart task, long nextDeadlineNanos, long reschedulingDelayNanos )
		 {
			  this._group = group;
			  this.NextDeadlineNanos = nextDeadlineNanos;
			  _handleRelease = new BinaryLatch();
			  _cancelListeners = new CopyOnWriteArrayList<CancelListener>();
			  this._task = () =>
			  {
				try
				{
					 task.run();
					 // Use compareAndSet to avoid overriding any cancellation state.
					 if ( compareAndSet( SUBMITTED, RUNNABLE ) && reschedulingDelayNanos > 0 )
					 {
						  // We only reschedule if the rescheduling delay is greater than zero.
						  // A rescheduling delay of zero means this is a delayed task.
						  // If the rescheduling delay is greater than zero, then this is a recurring task.
						  this.NextDeadlineNanos += reschedulingDelayNanos;
						  scheduler.EnqueueTask( this );
					 }
				}
				catch ( Exception e )
				{
					 _lastException = e;
					 set( FAILED );
				}
			  };
		 }

		 internal void SubmitIfRunnable( ThreadPoolManager pools )
		 {
			  if ( compareAndSet( RUNNABLE, SUBMITTED ) )
			  {
					_latestHandle = pools.Submit( _group, _task );
					_handleRelease.release();
			  }
		 }

		 public override void Cancel( bool mayInterruptIfRunning )
		 {
			  set( FAILED );
			  JobHandle handle = _latestHandle;
			  if ( handle != null )
			  {
					handle.Cancel( mayInterruptIfRunning );
			  }
			  foreach ( CancelListener cancelListener in _cancelListeners )
			  {
					cancelListener.Cancelled( mayInterruptIfRunning );
			  }
			  // Release the handle to allow waitTermination() to observe the cancellation.
			  _handleRelease.release();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void waitTermination() throws java.util.concurrent.ExecutionException, InterruptedException
		 public override void WaitTermination()
		 {
			  _handleRelease.await();
			  JobHandle handleDelegate = this._latestHandle;
			  if ( handleDelegate != null )
			  {
					handleDelegate.WaitTermination();
			  }
			  if ( get() == FAILED )
			  {
					Exception exception = this._lastException;
					if ( exception != null )
					{
						 throw new ExecutionException( exception );
					}
					else
					{
						 throw new CancellationException();
					}
			  }
		 }

		 public override void RegisterCancelListener( CancelListener listener )
		 {
			  _cancelListeners.add( listener );
		 }
	}

}