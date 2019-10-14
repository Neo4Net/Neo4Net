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
namespace Neo4Net.Scheduler
{

	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;

	/// <summary>
	/// Simple test scheduler implementation that is based on a cached thread pool.
	/// All threads created by this scheduler can be identified by <i>ThreadPoolScheduler</i> prefix.
	/// </summary>
	public class ThreadPoolJobScheduler : LifecycleAdapter, JobScheduler
	{
		 private readonly ExecutorService _executor = newCachedThreadPool( new DaemonThreadFactory( "ThreadPoolScheduler" ) );

		 public virtual string TopLevelGroupName
		 {
			 set
			 {
				  throw new System.NotSupportedException();
			 }
		 }

		 public override Executor Executor( Group group )
		 {
			  return _executor;
		 }

		 public override ExecutorService WorkStealingExecutor( Group group, int parallelism )
		 {
			  return _executor;
		 }

		 public override ExecutorService WorkStealingExecutorAsyncMode( Group group, int parallelism )
		 {
			  return _executor;
		 }

		 public override ThreadFactory ThreadFactory( Group group )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override JobHandle Schedule( Group group, ThreadStart job )
		 {
			  return new FutureJobHandle<>( _executor.submit( job ) );
		 }

		 public override JobHandle Schedule( Group group, ThreadStart runnable, long initialDelay, TimeUnit timeUnit )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override JobHandle ScheduleRecurring( Group group, ThreadStart runnable, long period, TimeUnit timeUnit )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override JobHandle ScheduleRecurring( Group group, ThreadStart runnable, long initialDelay, long period, TimeUnit timeUnit )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void Close()
		 {
			  Shutdown();
		 }

		 public override void Shutdown()
		 {
			  _executor.shutdown();
		 }

		 private class FutureJobHandle<V> : JobHandle
		 {
			  internal readonly Future<V> Future;

			  internal FutureJobHandle( Future<V> future )
			  {
					this.Future = future;
			  }

			  public override void Cancel( bool mayInterruptIfRunning )
			  {
					Future.cancel( mayInterruptIfRunning );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void waitTermination() throws InterruptedException, java.util.concurrent.ExecutionException, java.util.concurrent.CancellationException
			  public override void WaitTermination()
			  {
					Future.get();
			  }
		 }
	}

}