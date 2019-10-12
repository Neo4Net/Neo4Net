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
namespace Org.Neo4j.Kernel.monitoring.tracing
{
	using PageCacheTracer = Org.Neo4j.Io.pagecache.tracing.PageCacheTracer;
	using DefaultPageCursorTracerSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.DefaultPageCursorTracerSupplier;
	using PageCursorTracerSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using CheckPointTracer = Org.Neo4j.Kernel.impl.transaction.tracing.CheckPointTracer;
	using TransactionTracer = Org.Neo4j.Kernel.impl.transaction.tracing.TransactionTracer;
	using Log = Org.Neo4j.Logging.Log;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using LockTracer = Org.Neo4j.Storageengine.Api.@lock.LockTracer;
	using SystemNanoClock = Org.Neo4j.Time.SystemNanoClock;

	/// <summary>
	/// A TracerFactory determines the implementation of the tracers, that a database should use. Each implementation has
	/// a particular name, which is given by the getImplementationName method, and is used for identifying it in the
	/// <seealso cref="org.neo4j.kernel.impl.factory.GraphDatabaseFacadeFactory.Configuration.tracer"/> setting.
	/// </summary>
	public interface TracerFactory
	{
		 /// <returns> The name this implementation is identified by in the
		 /// <seealso cref="org.neo4j.kernel.impl.factory.GraphDatabaseFacadeFactory.Configuration.tracer"/> setting. </returns>
		 string ImplementationName { get; }

		 /// <summary>
		 /// Create a new PageCacheTracer instance.
		 /// </summary>
		 /// <param name="monitors"> the monitoring manager </param>
		 /// <param name="jobScheduler"> a scheduler for async jobs </param>
		 /// <param name="clock"> system nano clock </param>
		 /// <param name="log"> log </param>
		 /// <returns> The created instance. </returns>
		 PageCacheTracer CreatePageCacheTracer( Monitors monitors, JobScheduler jobScheduler, SystemNanoClock clock, Log log );

		 /// <summary>
		 /// Create a new TransactionTracer instance.
		 /// </summary>
		 /// <param name="monitors"> the monitoring manager </param>
		 /// <param name="jobScheduler"> a scheduler for async jobs </param>
		 /// <returns> The created instance. </returns>
		 TransactionTracer CreateTransactionTracer( Monitors monitors, JobScheduler jobScheduler );

		 /// <summary>
		 /// Create a new CheckPointTracer instance.
		 /// </summary>
		 /// <param name="monitors"> the monitoring manager </param>
		 /// <param name="jobScheduler"> a scheduler for async jobs </param>
		 /// <returns> The created instance. </returns>
		 CheckPointTracer CreateCheckPointTracer( Monitors monitors, JobScheduler jobScheduler );

		 /// <summary>
		 /// Create a new LockTracer instance.
		 /// </summary>
		 /// <param name="monitors"> the monitoring manager </param>
		 /// <param name="jobScheduler"> a scheduler for async jobs </param>
		 /// <returns> The created instance. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default org.neo4j.storageengine.api.@lock.LockTracer createLockTracer(org.neo4j.kernel.monitoring.Monitors monitors, org.neo4j.scheduler.JobScheduler jobScheduler)
	//	 {
	//		  return LockTracer.NONE;
	//	 }

		 /// <summary>
		 /// Create a new PageCursorTracerSupplier instance. </summary>
		 /// <param name="monitors"> the monitoring manager </param>
		 /// <param name="jobScheduler"> a scheduler for async jobs </param>
		 /// <returns> The created instance. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default org.neo4j.io.pagecache.tracing.cursor.PageCursorTracerSupplier createPageCursorTracerSupplier(org.neo4j.kernel.monitoring.Monitors monitors, org.neo4j.scheduler.JobScheduler jobScheduler)
	//	 {
	//		  return DefaultPageCursorTracerSupplier.INSTANCE;
	//	 }
	}

}