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

	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;

	/// <summary>
	/// To be expanded, the idea here is to have a database-global service for running jobs, handling jobs crashing and so
	/// on.
	/// </summary>
	public interface JobScheduler : Lifecycle, AutoCloseable
	{
		 /// <summary>
		 /// Assign a specific name to the top-most scheduler group.
		 /// <para>
		 /// This is just a suggestion for debugging purpose. The specific scheduler implementation is free to ignore calls
		 /// to this method.
		 /// </para>
		 /// </summary>
		 string TopLevelGroupName { set; }

		 /// <summary>
		 /// Expose a group scheduler as an <seealso cref="Executor"/> </summary>
		 Executor Executor( Group group );

		 /// <summary>
		 /// Creates an <seealso cref="ExecutorService"/> that does works-stealing - read more about this in <seealso cref="ForkJoinPool"/>
		 /// </summary>
		 ExecutorService WorkStealingExecutor( Group group, int parallelism );

		 /// <summary>
		 /// Creates an <seealso cref="ExecutorService"/> that does works-stealing with asyncMode set to true - read more about this in <seealso cref="ForkJoinPool"/>
		 /// <para>
		 /// This may be more suitable for systems where worker threads only process event-style asynchronous tasks.
		 /// </para>
		 /// </summary>
		 ExecutorService WorkStealingExecutorAsyncMode( Group group, int parallelism );

		 /// <summary>
		 /// Expose a group scheduler as a <seealso cref="java.util.concurrent.ThreadFactory"/>.
		 /// This is a lower-level alternative than <seealso cref="executor(Group)"/>, where you are in control of when to spin
		 /// up new threads for your jobs.
		 /// <para>
		 /// The lifecycle of the threads you get out of here are not managed by the JobScheduler, you own the lifecycle and
		 /// must start the thread before it can be used.
		 /// </para>
		 /// <para>
		 /// This mechanism is strongly preferred over manually creating threads, as it allows a central place for record
		 /// keeping of thread creation, central place for customizing the threads based on their groups, and lays a
		 /// foundation for controlling things like thread affinity and priorities in a coordinated manner in the future.
		 /// </para>
		 /// </summary>
		 ThreadFactory ThreadFactory( Group group );

		 /// <summary>
		 /// Schedule a new job in the specified group. </summary>
		 JobHandle Schedule( Group group, ThreadStart job );

		 /// <summary>
		 /// Schedule a new job in the specified group with the given delay </summary>
		 JobHandle Schedule( Group group, ThreadStart runnable, long initialDelay, TimeUnit timeUnit );

		 /// <summary>
		 /// Schedule a recurring job </summary>
		 JobHandle ScheduleRecurring( Group group, ThreadStart runnable, long period, TimeUnit timeUnit );

		 /// <summary>
		 /// Schedule a recurring job where the first invocation is delayed the specified time </summary>
		 JobHandle ScheduleRecurring( Group group, ThreadStart runnable, long initialDelay, long period, TimeUnit timeUnit );
	}

}