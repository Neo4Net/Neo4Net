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
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using FakeClock = Neo4Net.Time.FakeClock;

	/// <summary>
	/// N.B - Do not use this with time resolutions of less than 1 ms!
	/// </summary>
	public class FakeClockJobScheduler : FakeClock, JobScheduler
	{
		 private readonly AtomicLong _jobIdGen = new AtomicLong();
		 private readonly ICollection<JobHandle> _jobs = new CopyOnWriteArrayList<JobHandle>();

		 public FakeClockJobScheduler() : base()
		 {
		 }

		 private JobHandle Schedule( ThreadStart job, long firstDeadline )
		 {
			  JobHandle jobHandle = new JobHandle( this, job, firstDeadline, 0 );
			  _jobs.Add( jobHandle );
			  return jobHandle;
		 }

		 private JobHandle ScheduleRecurring( ThreadStart job, long firstDeadline, long period )
		 {
			  JobHandle jobHandle = new JobHandle( this, job, firstDeadline, period );
			  _jobs.Add( jobHandle );
			  return jobHandle;
		 }

		 public override FakeClock Forward( long delta, TimeUnit unit )
		 {
			  base.Forward( delta, unit );
			  ProcessSchedule();
			  return this;
		 }

		 private void ProcessSchedule()
		 {
			  bool anyTriggered;
			  do
			  {
					anyTriggered = false;
					foreach ( JobHandle job in _jobs )
					{
						 if ( job.TryTrigger() )
						 {
							  anyTriggered = true;
						 }
					}
			  } while ( anyTriggered );
		 }

		 private long Now()
		 {
			  return Instant().toEpochMilli();
		 }

		 public virtual string TopLevelGroupName
		 {
			 set
			 {
			 }
		 }

		 public override Executor Executor( Group group )
		 {
			  return job => schedule( job, Now() );
		 }

		 public override ExecutorService WorkStealingExecutor( Group group, int parallelism )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override ExecutorService WorkStealingExecutorAsyncMode( Group group, int parallelism )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override ThreadFactory ThreadFactory( Group group )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override JobHandle Schedule( Group group, ThreadStart job )
		 {
			  JobHandle handle = Schedule( job, Now() );
			  ProcessSchedule();
			  return handle;
		 }

		 public override JobHandle Schedule( Group group, ThreadStart job, long initialDelay, TimeUnit timeUnit )
		 {
			  JobHandle handle = schedule( job, Now() + timeUnit.toMillis(initialDelay) );
			  if ( initialDelay <= 0 )
			  {
					ProcessSchedule();
			  }
			  return handle;
		 }

		 public override JobHandle ScheduleRecurring( Group group, ThreadStart job, long period, TimeUnit timeUnit )
		 {
			  JobHandle handle = ScheduleRecurring( job, Now(), timeUnit.toMillis(period) );
			  ProcessSchedule();
			  return handle;
		 }

		 public override JobHandle ScheduleRecurring( Group group, ThreadStart job, long initialDelay, long period, TimeUnit timeUnit )
		 {
			  JobHandle handle = ScheduleRecurring( job, Now() + timeUnit.toMillis(initialDelay), timeUnit.toMillis(period) );
			  if ( initialDelay <= 0 )
			  {
					ProcessSchedule();
			  }
			  return handle;
		 }

		 public override void Init()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void Start()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void Stop()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void Shutdown()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void Close()
		 {
			  Shutdown();
		 }

		 internal class JobHandle : Neo4Net.Scheduler.JobHandle
		 {
			 internal bool InstanceFieldsInitialized = false;

			 internal virtual void InitializeInstanceFields()
			 {
				 Id = outerInstance.jobIdGen.incrementAndGet();
			 }

			 private readonly FakeClockJobScheduler _outerInstance;

			  internal long Id;
			  internal readonly ThreadStart Runnable;
			  internal readonly long Period;

			  internal long Deadline;

			  internal JobHandle( FakeClockJobScheduler outerInstance, ThreadStart runnable, long firstDeadline, long period )
			  {
				  this._outerInstance = outerInstance;

				  if ( !InstanceFieldsInitialized )
				  {
					  InitializeInstanceFields();
					  InstanceFieldsInitialized = true;
				  }
					this.Runnable = runnable;
					this.Deadline = firstDeadline;
					this.Period = period;
			  }

			  internal virtual bool TryTrigger()
			  {
					if ( outerInstance.now() >= Deadline )
					{
						 Runnable.run();
						 if ( Period != 0 )
						 {
							  Deadline += Period;
						 }
						 else
						 {
							  outerInstance.jobs.remove( this );
						 }
						 return true;
					}
					return false;
			  }

			  public override void Cancel( bool mayInterruptIfRunning )
			  {
					outerInstance.jobs.remove( this );
			  }

			  public override void WaitTermination()
			  {
					throw new System.NotSupportedException();
			  }

			  public override bool Equals( object o )
			  {
					if ( this == o )
					{
						 return true;
					}
					if ( o == null || this.GetType() != o.GetType() )
					{
						 return false;
					}
					JobHandle jobHandle = ( JobHandle ) o;
					return Id == jobHandle.Id;
			  }

			  public override int GetHashCode()
			  {
					return Objects.hash( Id );
			  }
		 }
	}

}