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
namespace Org.Neo4j.Kernel.impl.pagecache.monitor
{
	using Log = Org.Neo4j.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static System.currentTimeMillis;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Format.duration;

	public class PageCacheWarmerLoggingMonitor : PageCacheWarmerMonitorAdapter
	{
		 private readonly Log _log;
		 private long _warmupStartMillis;

		 public PageCacheWarmerLoggingMonitor( Log log )
		 {
			  this._log = log;
		 }

		 public override void WarmupStarted()
		 {
			  _warmupStartMillis = currentTimeMillis();
			  _log.info( "Page cache warmup started." );
		 }

		 public override void WarmupCompleted( long pagesLoaded )
		 {
			  _log.info( "Page cache warmup completed. %d pages loaded. Duration: %s.", pagesLoaded, GetDuration( _warmupStartMillis ) );
		 }

		 private static string GetDuration( long startTimeMillis )
		 {
			  return duration( currentTimeMillis() - startTimeMillis );
		 }
	}

}