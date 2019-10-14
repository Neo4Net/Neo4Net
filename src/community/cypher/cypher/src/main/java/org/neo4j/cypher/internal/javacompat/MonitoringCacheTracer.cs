using System;
using System.Collections.Generic;

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
namespace Neo4Net.Cypher.Internal.javacompat
{
	using Map = scala.collection.immutable.Map;

	using Neo4Net.Helpers.Collections;

	/// <summary>
	/// Adapter for passing CacheTraces into the Monitoring infrastructure.
	/// </summary>
	public class MonitoringCacheTracer : CacheTracer<Pair<string, IDictionary<string, Type>>>
	{
		 private readonly StringCacheMonitor _monitor;

		 public MonitoringCacheTracer( StringCacheMonitor monitor )
		 {
			  this._monitor = monitor;
		 }

		 public override void QueryCacheHit( Pair<string, Map<string, Type>> queryKey, string metaData )
		 {
			  _monitor.cacheHit( queryKey );
		 }

		 public override void QueryCacheMiss( Pair<string, Map<string, Type>> queryKey, string metaData )
		 {
			  _monitor.cacheMiss( queryKey );
		 }

		 public override void QueryCacheRecompile( Pair<string, Map<string, Type>> queryKey, string metaData )
		 {
			  _monitor.cacheRecompile( queryKey );
		 }

		 public override void QueryCacheStale( Pair<string, Map<string, Type>> queryKey, int secondsSincePlan, string metaData )
		 {
			  _monitor.cacheDiscard( queryKey, metaData, secondsSincePlan );
		 }

		 public override void QueryCacheFlush( long sizeOfCacheBeforeFlush )
		 {
			  _monitor.cacheFlushDetected( sizeOfCacheBeforeFlush );
		 }
	}

}