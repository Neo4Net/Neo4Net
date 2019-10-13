﻿/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
	using Test = org.junit.Test;

	using PageCacheTracer = Neo4Net.Io.pagecache.tracing.PageCacheTracer;
	using BufferingLog = Neo4Net.Logging.BufferingLog;
	using OnDemandJobScheduler = Neo4Net.Test.OnDemandJobScheduler;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class VerboseTracerFactoryTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void verboseTracerFactoryRegisterTracerWithCodeNameVerbose()
		 public virtual void VerboseTracerFactoryRegisterTracerWithCodeNameVerbose()
		 {
			  assertEquals( "verbose", TracerFactory().ImplementationName );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void verboseFactoryCreateVerboseTracer()
		 public virtual void VerboseFactoryCreateVerboseTracer()
		 {
			  BufferingLog msgLog = new BufferingLog();
			  PageCacheTracer pageCacheTracer = TracerFactory().createPageCacheTracer(new Monitors(), new OnDemandJobScheduler(), Clocks.nanoClock(), msgLog);
			  pageCacheTracer.BeginCacheFlush();
			  assertEquals( "Start whole page cache flush.", msgLog.ToString().Trim() );
		 }

		 private VerboseTracerFactory TracerFactory()
		 {
			  return new VerboseTracerFactory();
		 }
	}

}