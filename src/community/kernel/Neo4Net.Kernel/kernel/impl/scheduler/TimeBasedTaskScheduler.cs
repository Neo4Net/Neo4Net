using System.Collections.Generic;
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
namespace Neo4Net.Kernel.impl.scheduler
{

	using Group = Neo4Net.Scheduler.Group;
	using JobHandle = Neo4Net.Scheduler.JobHandle;
	using SystemNanoClock = Neo4Net.Time.SystemNanoClock;

	internal sealed class TimeBasedTaskScheduler : ThreadStart
	{
		 private static readonly long _noTasksPark = TimeUnit.MINUTES.toNanos( 10 );
		 private static readonly IComparer<ScheduledJobHandle> _deadlineComparator = System.Collections.IComparer.comparingLong( handle => handle.nextDeadlineNanos );

		 private readonly SystemNanoClock _clock;
		 private readonly ThreadPoolManager _pools;
		 private readonly PriorityBlockingQueue<ScheduledJobHandle> _delayedTasks;
		 private volatile Thread _timeKeeper;
		 private volatile bool _stopped;

		 internal TimeBasedTaskScheduler( SystemNanoClock clock, ThreadPoolManager pools )
		 {
			  this._clock = clock;
			  this._pools = pools;
			  _delayedTasks = new PriorityBlockingQueue<ScheduledJobHandle>( 42, _deadlineComparator );
		 }

		 public JobHandle Submit( Group group, ThreadStart job, long initialDelayNanos, long reschedulingDelayNanos )
		 {
			  long now = _clock.nanos();
			  long nextDeadlineNanos = now + initialDelayNanos;
			  ScheduledJobHandle task = new ScheduledJobHandle( this, group, job, nextDeadlineNanos, reschedulingDelayNanos );
			  EnqueueTask( task );
			  return task;
		 }

		 internal void EnqueueTask( ScheduledJobHandle newTasks )
		 {
			  _delayedTasks.offer( newTasks );
			  LockSupport.unpark( _timeKeeper );
		 }

		 public override void Run()
		 {
			  _timeKeeper = Thread.CurrentThread;
			  while ( !_stopped )
			  {
					long timeToNextTickNanos = Tick();
					if ( _stopped )
					{
						 return;
					}
					LockSupport.parkNanos( this, timeToNextTickNanos );
			  }
		 }

		 public long Tick()
		 {
			  long now = _clock.nanos();
			  long timeToNextDeadlineSinceStart = ScheduleDueTasks( now );
			  long processingTime = _clock.nanos() - now;
			  return timeToNextDeadlineSinceStart - processingTime;
		 }

		 private long ScheduleDueTasks( long now )
		 {
			  if ( _delayedTasks.Empty )
			  {
					// We have no tasks to run. Park until we're woken up by an enqueueTask() call.
					return _noTasksPark;
			  }
			  while ( !_stopped && !_delayedTasks.Empty && _delayedTasks.peek().nextDeadlineNanos <= now )
			  {
					ScheduledJobHandle task = _delayedTasks.poll();
					task.SubmitIfRunnable( _pools );
			  }
			  return _delayedTasks.Empty ? _noTasksPark : _delayedTasks.peek().nextDeadlineNanos - now;
		 }

		 public void Stop()
		 {
			  _stopped = true;
			  LockSupport.unpark( _timeKeeper );
		 }
	}

}