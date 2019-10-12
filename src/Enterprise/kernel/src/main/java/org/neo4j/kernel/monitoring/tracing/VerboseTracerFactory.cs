/*
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
namespace Neo4Net.Kernel.monitoring.tracing
{
	using Service = Neo4Net.Helpers.Service;
	using PageCacheTracer = Neo4Net.Io.pagecache.tracing.PageCacheTracer;
	using Log = Neo4Net.Logging.Log;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using SystemNanoClock = Neo4Net.Time.SystemNanoClock;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(TracerFactory.class) public class VerboseTracerFactory extends DefaultTracerFactory
	public class VerboseTracerFactory : DefaultTracerFactory
	{
		 public override string ImplementationName
		 {
			 get
			 {
				  return "verbose";
			 }
		 }

		 public override PageCacheTracer CreatePageCacheTracer( Monitors monitors, JobScheduler jobScheduler, SystemNanoClock clock, Log msgLog )
		 {
			  return new VerbosePageCacheTracer( msgLog, clock );
		 }
	}

}