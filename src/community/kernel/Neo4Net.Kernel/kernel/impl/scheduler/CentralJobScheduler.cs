using System;
using System.Collections.Concurrent;
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

	using Exceptions = Neo4Net.Helpers.Exceptions;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Group = Neo4Net.Scheduler.Group;
	using JobHandle = Neo4Net.Scheduler.JobHandle;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using Clocks = Neo4Net.Time.Clocks;

	public class CentralJobScheduler : LifecycleAdapter, IJobScheduler, IDisposable
	{
		 private static readonly AtomicInteger _instanceCounter = new AtomicInteger();

		 private readonly TimeBasedTaskScheduler _scheduler;
		 private readonly Thread _schedulerThread;

		 // Contains workStealingExecutors for each group that have asked for one.
		 // If threads need to be created, they need to be inside one of these pools.
		 // We also need to remember to shutdown all pools when we shutdown the database to shutdown queries in an orderly
		 // fashion.
		 private readonly ConcurrentDictionary<Group, ExecutorService> _workStealingExecutors;

		 private readonly TopLevelGroup _topLevelGroup;
		 private readonly ThreadPoolManager _pools;

		 private volatile bool _started;

		 private class TopLevelGroup : ThreadGroup
		 {
			  internal TopLevelGroup() : base("Neo4j-" + _instanceCounter.incrementAndGet())
			  {
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void setName(String name) throws Exception
			  public virtual string Name
			  {
				  set
				  {
						System.Reflection.FieldInfo field = typeof( ThreadGroup ).getDeclaredField( "name" );
						field.Accessible = true;
						field.set( this, value );
				  }
			  }
		 }

		 protected internal CentralJobScheduler()
		 {
			  _workStealingExecutors = new ConcurrentDictionary<Group, ExecutorService>( 1 );
			  _topLevelGroup = new TopLevelGroup();
			  _pools = new ThreadPoolManager( _topLevelGroup );
			  ThreadFactory threadFactory = new GroupedDaemonThreadFactory( Group.TASK_SCHEDULER, _topLevelGroup );
			  _scheduler = new TimeBasedTaskScheduler( Clocks.nanoClock(), _pools );

			  // The scheduler thread runs at slightly elevated priority for timeliness, and is started in init().
			  _schedulerThread = threadFactory.newThread( _scheduler );
			  int priority = Thread.NORM_PRIORITY + 1;
			  _schedulerThread.Priority = priority;
		 }

		 public virtual string TopLevelGroupName
		 {
			 set
			 {
				  try
				  {
						_topLevelGroup.Name = value;
				  }
				  catch ( Exception )
				  {
				  }
			 }
		 }

		 public override void Init()
		 {
			  if ( !_started )
			  {
					_schedulerThread.Start();
					_started = true;
			  }
		 }

		 public override Executor Executor( Group group )
		 {
			  return job => Schedule( group, job );
		 }

		 public override ExecutorService WorkStealingExecutor( Group group, int parallelism )
		 {
			  return WorkStealingExecutor( group, parallelism, false );
		 }

		 public override ExecutorService WorkStealingExecutorAsyncMode( Group group, int parallelism )
		 {
			  return WorkStealingExecutor( group, parallelism, true );
		 }

		 private ExecutorService WorkStealingExecutor( Group group, int parallelism, bool asyncMode )
		 {
			  return _workStealingExecutors.computeIfAbsent( group, g => CreateNewWorkStealingExecutor( g, parallelism, asyncMode ) );
		 }

		 public override ThreadFactory ThreadFactory( Group group )
		 {
			  return _pools.getThreadPool( group ).ThreadFactory;
		 }

		 private ExecutorService CreateNewWorkStealingExecutor( Group group, int parallelism, bool asyncMode )
		 {
			  ForkJoinPool.ForkJoinWorkerThreadFactory factory = new GroupedDaemonThreadFactory( group, _topLevelGroup );
			  return new ForkJoinPool( parallelism, factory, null, asyncMode );
		 }

		 public override JobHandle Schedule( Group group, ThreadStart job )
		 {
			  if ( !_started )
			  {
					throw new RejectedExecutionException( "Scheduler is not started" );
			  }
			  return _pools.submit( group, job );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.neo4j.scheduler.JobHandle scheduleRecurring(org.neo4j.scheduler.Group group, final Runnable runnable, long period, java.util.concurrent.TimeUnit timeUnit)
		 public override JobHandle ScheduleRecurring( Group group, ThreadStart runnable, long period, TimeUnit timeUnit )
		 {
			  return ScheduleRecurring( group, runnable, 0, period, timeUnit );
		 }

		 public override JobHandle ScheduleRecurring( Group group, ThreadStart runnable, long initialDelay, long period, TimeUnit unit )
		 {
			  return _scheduler.submit( group, runnable, unit.toNanos( initialDelay ), unit.toNanos( period ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.neo4j.scheduler.JobHandle schedule(org.neo4j.scheduler.Group group, final Runnable runnable, long initialDelay, java.util.concurrent.TimeUnit unit)
		 public override JobHandle Schedule( Group group, ThreadStart runnable, long initialDelay, TimeUnit unit )
		 {
			  return _scheduler.submit( group, runnable, unit.toNanos( initialDelay ), 0 );
		 }

		 public override void Shutdown()
		 {
			  _started = false;

			  // First shut down the scheduler, so no new tasks are queued up in the pools.
			  InterruptedException exception = ShutDownScheduler();

			  // Then shut down the thread pools. This involves cancelling jobs which hasn't been cancelled already,
			  // so we avoid having to wait the full maximum wait time on the executor service shut-downs.
			  exception = Exceptions.chain( exception, _pools.shutDownAll() );

			  // Finally, we shut the work-stealing executors down.
			  foreach ( ExecutorService workStealingExecutor in _workStealingExecutors.Values )
			  {
					exception = ShutdownPool( workStealingExecutor, exception );
			  }
			  _workStealingExecutors.Clear();

			  if ( exception != null )
			  {
					throw new Exception( "Unable to shut down job scheduler properly.", exception );
			  }
		 }

		 public override void Close()
		 {
			  Shutdown();
		 }

		 private InterruptedException ShutDownScheduler()
		 {
			  _scheduler.stop();
			  try
			  {
					_schedulerThread.Join();
			  }
			  catch ( InterruptedException e )
			  {
					return e;
			  }
			  return null;
		 }

		 private InterruptedException ShutdownPool( ExecutorService pool, InterruptedException exception )
		 {
			  if ( pool != null )
			  {
					pool.shutdown();
					try
					{
						 pool.awaitTermination( 30, TimeUnit.SECONDS );
					}
					catch ( InterruptedException e )
					{
						 return Exceptions.chain( exception, e );
					}
			  }
			  return exception;
		 }
	}

}