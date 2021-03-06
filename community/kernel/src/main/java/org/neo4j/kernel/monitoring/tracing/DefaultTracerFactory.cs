﻿/*
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
	using DefaultPageCacheTracer = Org.Neo4j.Io.pagecache.tracing.DefaultPageCacheTracer;
	using PageCacheTracer = Org.Neo4j.Io.pagecache.tracing.PageCacheTracer;
	using DefaultTransactionTracer = Org.Neo4j.Kernel.Impl.Api.DefaultTransactionTracer;
	using DefaultCheckPointerTracer = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.DefaultCheckPointerTracer;
	using CheckPointTracer = Org.Neo4j.Kernel.impl.transaction.tracing.CheckPointTracer;
	using TransactionTracer = Org.Neo4j.Kernel.impl.transaction.tracing.TransactionTracer;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using Log = Org.Neo4j.Logging.Log;
	using SystemNanoClock = Org.Neo4j.Time.SystemNanoClock;

	/// <summary>
	/// The default TracerFactory, when nothing else is otherwise configured.
	/// </summary>
	public class DefaultTracerFactory : TracerFactory
	{
		 public virtual string ImplementationName
		 {
			 get
			 {
				  return "default";
			 }
		 }

		 public override PageCacheTracer CreatePageCacheTracer( Monitors monitors, JobScheduler jobScheduler, SystemNanoClock clock, Log log )
		 {
			  return new DefaultPageCacheTracer();
		 }

		 public override TransactionTracer CreateTransactionTracer( Monitors monitors, JobScheduler jobScheduler )
		 {
			  DefaultTransactionTracer.Monitor monitor = monitors.NewMonitor( typeof( DefaultTransactionTracer.Monitor ) );
			  return new DefaultTransactionTracer( monitor, jobScheduler );
		 }

		 public override CheckPointTracer CreateCheckPointTracer( Monitors monitors, JobScheduler jobScheduler )
		 {
			  DefaultCheckPointerTracer.Monitor monitor = monitors.NewMonitor( typeof( DefaultCheckPointerTracer.Monitor ) );
			  return new DefaultCheckPointerTracer( monitor, jobScheduler );
		 }
	}

}