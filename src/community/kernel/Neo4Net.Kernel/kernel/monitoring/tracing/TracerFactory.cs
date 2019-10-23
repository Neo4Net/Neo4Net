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
namespace Neo4Net.Kernel.monitoring.tracing
{
	using PageCacheTracer = Neo4Net.Io.pagecache.tracing.PageCacheTracer;
	using DefaultPageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.DefaultPageCursorTracerSupplier;
	using PageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using CheckPointTracer = Neo4Net.Kernel.impl.transaction.tracing.CheckPointTracer;
	using TransactionTracer = Neo4Net.Kernel.impl.transaction.tracing.TransactionTracer;
	using Log = Neo4Net.Logging.Log;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using LockTracer = Neo4Net.Kernel.Api.StorageEngine.@lock.LockTracer;
	using SystemNanoClock = Neo4Net.Time.SystemNanoClock;

	/// <summary>
	/// A TracerFactory determines the implementation of the tracers, that a database should use. Each implementation has
	/// a particular name, which is given by the getImplementationName method, and is used for identifying it in the
	/// <seealso cref="org.Neo4Net.kernel.impl.factory.GraphDatabaseFacadeFactory.Configuration.tracer"/> setting.
	/// </summary>
	public interface TracerFactory
	{
		 /// <returns> The name this implementation is identified by in the
		 /// <seealso cref="org.Neo4Net.kernel.impl.factory.GraphDatabaseFacadeFactory.Configuration.tracer"/> setting. </returns>
		 string ImplementationName { get; }

		 /// <summary>
		 /// Create a new PageCacheTracer instance.
		 /// </summary>
		 /// <param name="monitors"> the monitoring manager </param>
		 /// <param name="jobScheduler"> a scheduler for async jobs </param>
		 /// <param name="clock"> system nano clock </param>
		 /// <param name="log"> log </param>
		 /// <returns> The created instance. </returns>
		 PageCacheTracer CreatePageCacheTracer( Monitors monitors, IJobScheduler jobScheduler, SystemNanoClock clock, Log log );

		 /// <summary>
		 /// Create a new TransactionTracer instance.
		 /// </summary>
		 /// <param name="monitors"> the monitoring manager </param>
		 /// <param name="jobScheduler"> a scheduler for async jobs </param>
		 /// <returns> The created instance. </returns>
		 TransactionTracer CreateTransactionTracer( Monitors monitors, IJobScheduler jobScheduler );

		 /// <summary>
		 /// Create a new CheckPointTracer instance.
		 /// </summary>
		 /// <param name="monitors"> the monitoring manager </param>
		 /// <param name="jobScheduler"> a scheduler for async jobs </param>
		 /// <returns> The created instance. </returns>
		 CheckPointTracer CreateCheckPointTracer( Monitors monitors, IJobScheduler jobScheduler );

		 /// <summary>
		 /// Create a new LockTracer instance.
		 /// </summary>
		 /// <param name="monitors"> the monitoring manager </param>
		 /// <param name="jobScheduler"> a scheduler for async jobs </param>
		 /// <returns> The created instance. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default org.Neo4Net.Kernel.Api.StorageEngine.@lock.LockTracer createLockTracer(org.Neo4Net.kernel.monitoring.Monitors monitors, org.Neo4Net.scheduler.JobScheduler jobScheduler)
	//	 {
	//		  return LockTracer.NONE;
	//	 }

		 /// <summary>
		 /// Create a new PageCursorTracerSupplier instance. </summary>
		 /// <param name="monitors"> the monitoring manager </param>
		 /// <param name="jobScheduler"> a scheduler for async jobs </param>
		 /// <returns> The created instance. </returns>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default org.Neo4Net.io.pagecache.tracing.cursor.PageCursorTracerSupplier createPageCursorTracerSupplier(org.Neo4Net.kernel.monitoring.Monitors monitors, org.Neo4Net.scheduler.JobScheduler jobScheduler)
	//	 {
	//		  return DefaultPageCursorTracerSupplier.INSTANCE;
	//	 }
	}

}