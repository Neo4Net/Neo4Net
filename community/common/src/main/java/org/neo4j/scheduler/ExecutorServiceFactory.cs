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
namespace Org.Neo4j.Scheduler
{


	/// <summary>
	/// Implementations of this interface are used by the <seealso cref="JobHandle"/> implememtation to create the underlying <seealso cref="ExecutorService"/>s that actually run the
	/// scheduled jobs. The choice of implementation is decided by the scheduling <seealso cref="Group"/>, which can thereby influence how jobs in the particular group are
	/// executed.
	/// </summary>
	internal interface ExecutorServiceFactory
	{
		 /// <summary>
		 /// Create an <seealso cref="ExecutorService"/> with a default thread count.
		 /// </summary>
		 ExecutorService Build( Group group, SchedulerThreadFactory factory );

		 /// <summary>
		 /// Create an <seealso cref="ExecutorService"/>, ideally with the desired thread count if possible.
		 /// Implementations are allowed to ignore the given thread count.
		 /// </summary>
		 ExecutorService Build( Group group, SchedulerThreadFactory factory, int threadCount );

		 /// <summary>
		 /// This factory actually prevents the scheduling and execution of any jobs, which is useful for groups that are not meant to be scheduled directly.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static ExecutorServiceFactory unschedulable()
	//	 {
	//		  return new ExecutorServiceFactory()
	//		  {
	//				@@Override public ExecutorService build(Group group, SchedulerThreadFactory factory)
	//				{
	//					 throw newUnschedulableException(group);
	//				}
	//
	//				@@Override public ExecutorService build(Group group, SchedulerThreadFactory factory, int threadCount)
	//				{
	//					 throw newUnschedulableException(group);
	//				}
	//
	//				private IllegalArgumentException newUnschedulableException(Group group)
	//				{
	//					 return new IllegalArgumentException("Tasks cannot be scheduled directly to the " + group.groupName() + " group.");
	//				}
	//		  };
	//	 }

		 /// <summary>
		 /// Executes all jobs in the same single thread.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static ExecutorServiceFactory singleThread()
	//	 {
	//		  return new ExecutorServiceFactory()
	//		  {
	//				@@Override public ExecutorService build(Group group, SchedulerThreadFactory factory)
	//				{
	//					 return newSingleThreadExecutor(factory);
	//				}
	//
	//				@@Override public ExecutorService build(Group group, SchedulerThreadFactory factory, int threadCount)
	//				{
	//					 return build(group, factory); // Just ignore the thread count.
	//				}
	//		  };
	//	 }

		 /// <summary>
		 /// Execute jobs in a dynamically growing pool of threads. The threads will be cached and kept around for a little while to cope with work load spikes
		 /// and troughs.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static ExecutorServiceFactory cached()
	//	 {
	//		  return new ExecutorServiceFactory()
	//		  {
	//				@@Override public ExecutorService build(Group group, SchedulerThreadFactory factory)
	//				{
	//					 return newCachedThreadPool(factory);
	//				}
	//
	//				@@Override public ExecutorService build(Group group, SchedulerThreadFactory factory, int threadCount)
	//				{
	//					 return newFixedThreadPool(threadCount, factory);
	//				}
	//		  };
	//	 }

		 /// <summary>
		 /// Schedules jobs in a work-stealing (ForkJoin) thread pool. <seealso cref="java.util.stream.Stream.parallel Parallel streams"/> and <seealso cref="ForkJoinTask"/>s started
		 /// from within the scheduled jobs will also run inside the same <seealso cref="ForkJoinPool"/>.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static ExecutorServiceFactory workStealing()
	//	 {
	//		  return new ExecutorServiceFactory()
	//		  {
	//				@@Override public ExecutorService build(Group group, SchedulerThreadFactory factory)
	//				{
	//					 return new ForkJoinPool(getRuntime().availableProcessors(), factory, null, false);
	//				}
	//
	//				@@Override public ExecutorService build(Group group, SchedulerThreadFactory factory, int threadCount)
	//				{
	//					 return new ForkJoinPool(threadCount, factory, null, false);
	//				}
	//		  };
	//	 }
	}

}