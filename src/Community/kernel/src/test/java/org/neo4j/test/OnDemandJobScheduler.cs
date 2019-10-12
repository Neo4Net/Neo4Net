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
namespace Neo4Net.Test
{

	using Group = Neo4Net.Scheduler.Group;
	using JobHandle = Neo4Net.Scheduler.JobHandle;
	using JobSchedulerAdapter = Neo4Net.Scheduler.JobSchedulerAdapter;

	public class OnDemandJobScheduler : JobSchedulerAdapter
	{
		 private IList<ThreadStart> _jobs = new CopyOnWriteArrayList<ThreadStart>();

		 private readonly bool _removeJobsAfterExecution;

		 public OnDemandJobScheduler() : this(true)
		 {
		 }

		 public OnDemandJobScheduler( bool removeJobsAfterExecution )
		 {
			  this._removeJobsAfterExecution = removeJobsAfterExecution;
		 }

		 public override Executor Executor( Group group )
		 {
			  return command => _jobs.Add( command );
		 }

		 public override JobHandle Schedule( Group group, ThreadStart job )
		 {
			  _jobs.Add( job );
			  return new OnDemandJobHandle( this );
		 }

		 public override JobHandle Schedule( Group group, ThreadStart job, long initialDelay, TimeUnit timeUnit )
		 {
			  _jobs.Add( job );
			  return new OnDemandJobHandle( this );
		 }

		 public override JobHandle ScheduleRecurring( Group group, ThreadStart runnable, long period, TimeUnit timeUnit )
		 {
			  _jobs.Add( runnable );
			  return new OnDemandJobHandle( this );
		 }

		 public override JobHandle ScheduleRecurring( Group group, ThreadStart runnable, long initialDelay, long period, TimeUnit timeUnit )
		 {
			  _jobs.Add( runnable );
			  return new OnDemandJobHandle( this );
		 }

		 public virtual ThreadStart Job
		 {
			 get
			 {
				  return _jobs.Count > 0 ? _jobs[0] : null;
			 }
		 }

		 public virtual void RunJob()
		 {
			  foreach ( ThreadStart job in _jobs )
			  {
					job.run();
					if ( _removeJobsAfterExecution )
					{
						 _jobs.Remove( job );
					}
			  }
		 }

		 private class OnDemandJobHandle : JobHandle
		 {
			 private readonly OnDemandJobScheduler _outerInstance;

			 public OnDemandJobHandle( OnDemandJobScheduler outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override void Cancel( bool mayInterruptIfRunning )
			  {
					outerInstance.jobs.Clear();
			  }

			  public override void WaitTermination()
			  {
					// on demand
			  }
		 }
	}

}