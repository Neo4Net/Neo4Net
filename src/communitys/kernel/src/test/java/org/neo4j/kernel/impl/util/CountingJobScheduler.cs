using System;
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
namespace Neo4Net.Kernel.impl.util
{

	using Group = Neo4Net.Scheduler.Group;
	using JobHandle = Neo4Net.Scheduler.JobHandle;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;

	public class CountingJobScheduler : JobScheduler
	{
		 private readonly AtomicInteger _counter;
		 private readonly JobScheduler @delegate;

		 public CountingJobScheduler( AtomicInteger counter, JobScheduler @delegate )
		 {
			  this._counter = counter;
			  this.@delegate = @delegate;
		 }

		 public virtual string TopLevelGroupName
		 {
			 set
			 {
				  @delegate.TopLevelGroupName = value;
			 }
		 }

		 public override Executor Executor( Group group )
		 {
			  return @delegate.Executor( group );
		 }

		 public override ThreadFactory ThreadFactory( Group group )
		 {
			  return @delegate.ThreadFactory( group );
		 }

		 public override ExecutorService WorkStealingExecutor( Group group, int parallelism )
		 {
			  return @delegate.WorkStealingExecutor( group, parallelism );
		 }

		 public override ExecutorService WorkStealingExecutorAsyncMode( Group group, int parallelism )
		 {
			  return @delegate.WorkStealingExecutorAsyncMode( group, parallelism );
		 }

		 public override JobHandle Schedule( Group group, ThreadStart job )
		 {
			  _counter.AndIncrement;
			  return @delegate.Schedule( group, job );
		 }

		 public override JobHandle Schedule( Group group, ThreadStart runnable, long initialDelay, TimeUnit timeUnit )
		 {
			  _counter.AndIncrement;
			  return @delegate.Schedule( group, runnable, initialDelay, timeUnit );
		 }

		 public override JobHandle ScheduleRecurring( Group group, ThreadStart runnable, long period, TimeUnit timeUnit )
		 {
			  _counter.AndIncrement;
			  return @delegate.ScheduleRecurring( group, runnable, period, timeUnit );
		 }

		 public override JobHandle ScheduleRecurring( Group group, ThreadStart runnable, long initialDelay, long period, TimeUnit timeUnit )
		 {
			  _counter.AndIncrement;
			  return @delegate.ScheduleRecurring( group, runnable, initialDelay, period, timeUnit );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void init() throws Throwable
		 public override void Init()
		 {
			  @delegate.Init();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
		 public override void Start()
		 {
			  @delegate.Start();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws Throwable
		 public override void Stop()
		 {
			  @delegate.Stop();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown() throws Throwable
		 public override void Shutdown()
		 {
			  @delegate.Shutdown();
		 }

		 public override void Close()
		 {
			  try
			  {
					Shutdown();
			  }
			  catch ( Exception throwable )
			  {
					throw new Exception( throwable );
			  }
		 }
	}

}