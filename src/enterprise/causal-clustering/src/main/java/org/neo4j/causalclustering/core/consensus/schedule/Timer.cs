using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.core.consensus.schedule
{
	using TimerName = Neo4Net.causalclustering.core.consensus.schedule.TimerService.TimerName;
	using Log = Neo4Net.Logging.Log;
	using Group = Neo4Net.Scheduler.Group;
	using JobHandle = Neo4Net.Scheduler.JobHandle;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

	/// <summary>
	/// A timer which can be set to go off at a future point in time.
	/// <para>
	/// When the timer goes off a timeout event is said to occur and the registered
	/// <seealso cref="TimeoutHandler"/> will be invoked.
	/// </para>
	/// </summary>
	public class Timer
	{
		 private readonly TimerName _name;
		 private readonly IJobScheduler _scheduler;
		 private readonly Log _log;
		 private readonly Group _group;
		 private readonly TimeoutHandler _handler;

		 private Timeout _timeout;
		 private Delay _delay;
		 private JobHandle _job;
		 private long _activeJobId;

		 /// <summary>
		 /// Creates a timer in the deactivated state.
		 /// </summary>
		 /// <param name="name"> The name of the timer. </param>
		 /// <param name="scheduler"> The underlying scheduler used. </param>
		 /// <param name="group"> The scheduler group used. </param>
		 /// <param name="handler"> The timeout handler. </param>
		 internal Timer( TimerName name, IJobScheduler scheduler, Log log, Group group, TimeoutHandler handler )
		 {
			  this._name = name;
			  this._scheduler = scheduler;
			  this._log = log;
			  this._group = group;
			  this._handler = handler;
		 }

		 /// <summary>
		 /// Activates the timer to go off at the specified timeout. Calling this method
		 /// when the timer already is active will shift the timeout to the new value.
		 /// </summary>
		 /// <param name="newTimeout"> The new timeout value. </param>
		 public virtual void Set( Timeout newTimeout )
		 {
			 lock ( this )
			 {
				  _delay = newTimeout.Next();
				  _timeout = newTimeout;
				  long jobId = NewJobId();
				  _job = _scheduler.schedule( _group, () => handle(jobId), _delay.amount(), _delay.unit() );
			 }
		 }

		 private long NewJobId()
		 {
			  _activeJobId = _activeJobId + 1;
			  return _activeJobId;
		 }

		 private void Handle( long jobId )
		 {
			  lock ( this )
			  {
					if ( _activeJobId != jobId )
					{
						 return;
					}
			  }

			  try
			  {
					_handler( this );
			  }
			  catch ( Exception e )
			  {
					_log.error( format( "[%s] Handler threw exception", CanonicalName() ), e );
			  }
		 }

		 /// <summary>
		 /// Resets the timer based on the currently programmed timeout.
		 /// </summary>
		 public virtual void Reset()
		 {
			 lock ( this )
			 {
				  if ( _timeout == null )
				  {
						throw new System.InvalidOperationException( "You can't reset until you have set a timeout" );
				  }
				  Set( _timeout );
			 }
		 }

		 /// <summary>
		 /// Deactivates the timer and cancels a currently running job.
		 /// <para>
		 /// Be careful to not have a timeout handler executing in parallel with a
		 /// timer, because this will just cancel the timer. If you for example
		 /// <seealso cref="reset()"/> in the timeout handler, but keep executing the handler,
		 /// then a subsequent cancel will not ensure that the first execution of the
		 /// handler was cancelled.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="cancelMode"> The mode of cancelling. </param>
		 public virtual void Cancel( CancelMode cancelMode )
		 {
			  JobHandle job;

			  lock ( this )
			  {
					_activeJobId++;
					job = this._job;
			  }

			  if ( job != null )
			  {
					try
					{
						 if ( cancelMode == CancelMode.SyncWait )
						 {
							  job.WaitTermination();
						 }
						 else if ( cancelMode == CancelMode.AsyncInterrupt )
						 {
							  job.Cancel( true );
						 }
					}
					catch ( Exception e )
					{
						 _log.warn( format( "[%s] Cancelling timer threw exception", CanonicalName() ), e );
					}
			  }
		 }

		 /// <summary>
		 /// Schedules the timer for an immediate timeout.
		 /// </summary>
		 public virtual void Invoke()
		 {
			 lock ( this )
			 {
				  long jobId = NewJobId();
				  _job = _scheduler.schedule( _group, () => handle(jobId) );
			 }
		 }

		 internal virtual Delay Delay()
		 {
			 lock ( this )
			 {
				  return _delay;
			 }
		 }

		 public virtual TimerName Name()
		 {
			  return _name;
		 }

		 private string CanonicalName()
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getCanonicalName method:
			  return _name.GetType().FullName + "." + _name.name();
		 }

		 public enum CancelMode
		 {
			  /// <summary>
			  /// Asynchronously cancels.
			  /// </summary>
			  Async,

			  /// <summary>
			  /// Asynchronously cancels and interrupts the handler.
			  /// </summary>
			  AsyncInterrupt,

			  /// <summary>
			  /// Synchronously cancels and waits for the handler to finish.
			  /// </summary>
			  SyncWait,

			  /*
			   * Note that SYNC_INTERRUPT cannot be supported, since the underlying
			   * primitive is a future which cannot be cancelled/interrupted and awaited.
			   */
		 }
	}

}