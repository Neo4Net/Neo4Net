using System.Collections.Concurrent;
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
	using JobHandle = Neo4Net.Scheduler.JobHandle;

	internal sealed class ThreadPool
	{
		 private readonly GroupedDaemonThreadFactory _threadFactory;
		 private readonly ExecutorService _executor;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.concurrent.ConcurrentHashMap<Object,java.util.concurrent.Future<?>> registry;
		 private readonly ConcurrentDictionary<object, Future<object>> _registry;
		 private InterruptedException _shutdownInterrupted;

		 internal ThreadPool( Group group, ThreadGroup parentThreadGroup )
		 {
			  _threadFactory = new GroupedDaemonThreadFactory( group, parentThreadGroup );
			  _executor = group.buildExecutorService( _threadFactory );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: registry = new java.util.concurrent.ConcurrentHashMap<>();
			  _registry = new ConcurrentDictionary<object, Future<object>>();
		 }

		 public ThreadFactory ThreadFactory
		 {
			 get
			 {
				  return _threadFactory;
			 }
		 }

		 public JobHandle Submit( ThreadStart job )
		 {
			  object registryKey = new object();
			  ThreadStart registeredJob = () =>
			  {
				try
				{
					 job.run();
				}
				finally
				{
					 _registry.Remove( registryKey );
				}
			  };
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> future = executor.submit(registeredJob);
			  Future<object> future = _executor.submit( registeredJob );
			  _registry[registryKey] = future;
			  return new PooledJobHandle( future, registryKey, _registry );
		 }

		 internal void CancelAllJobs()
		 {
			  _registry.Values.removeIf(future =>
			  {
				future.cancel( true );
				return true;
			  });
		 }

		 internal void ShutDown()
		 {
			  _executor.shutdown();
			  try
			  {
					_executor.awaitTermination( 30, TimeUnit.SECONDS );
			  }
			  catch ( InterruptedException e )
			  {
					_shutdownInterrupted = e;
			  }
		 }

		 internal InterruptedException ShutdownException
		 {
			 get
			 {
				  return _shutdownInterrupted;
			 }
		 }
	}

}