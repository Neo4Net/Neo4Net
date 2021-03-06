﻿/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.Kernel.stresstests.transaction.checkpoint.tracers
{
	using PageCacheTracer = Org.Neo4j.Io.pagecache.tracing.PageCacheTracer;
	using CheckPointTracer = Org.Neo4j.Kernel.impl.transaction.tracing.CheckPointTracer;
	using TransactionTracer = Org.Neo4j.Kernel.impl.transaction.tracing.TransactionTracer;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using TracerFactory = Org.Neo4j.Kernel.monitoring.tracing.TracerFactory;
	using Log = Org.Neo4j.Logging.Log;
	using SystemNanoClock = Org.Neo4j.Time.SystemNanoClock;

	public class TimerTracerFactory : TracerFactory
	{
		 private TimerTransactionTracer _timerTransactionTracer = new TimerTransactionTracer();

		 public virtual string ImplementationName
		 {
			 get
			 {
				  return "timer";
			 }
		 }

		 public override PageCacheTracer CreatePageCacheTracer( Monitors monitors, JobScheduler jobScheduler, SystemNanoClock clock, Log log )
		 {
			  return PageCacheTracer.NULL;
		 }

		 public override TransactionTracer CreateTransactionTracer( Monitors monitors, JobScheduler jobScheduler )
		 {
			  return _timerTransactionTracer;
		 }

		 public override CheckPointTracer CreateCheckPointTracer( Monitors monitors, JobScheduler jobScheduler )
		 {
			  return _timerTransactionTracer;
		 }
	}

}