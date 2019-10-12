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
namespace Org.Neo4j.metrics.source.db
{
	using Gauge = com.codahale.metrics.Gauge;
	using MetricRegistry = com.codahale.metrics.MetricRegistry;

	using PageCacheCounters = Org.Neo4j.Io.pagecache.monitoring.PageCacheCounters;
	using Documented = Org.Neo4j.Kernel.Impl.Annotations.Documented;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.codahale.metrics.MetricRegistry.name;

	[Documented(".Database page cache metrics")]
	public class PageCacheMetrics : LifecycleAdapter
	{
		 private const string PAGE_CACHE_PREFIX = "neo4j.page_cache";

		 [Documented("The total number of exceptions seen during the eviction process in the page cache")]
		 public static readonly string PcEvictionExceptions = name( PAGE_CACHE_PREFIX, "eviction_exceptions" );
		 [Documented("The total number of flushes executed by the page cache")]
		 public static readonly string PcFlushes = name( PAGE_CACHE_PREFIX, "flushes" );
		 [Documented("The total number of page unpins executed by the page cache")]
		 public static readonly string PcUnpins = name( PAGE_CACHE_PREFIX, "unpins" );
		 [Documented("The total number of page pins executed by the page cache")]
		 public static readonly string PcPins = name( PAGE_CACHE_PREFIX, "pins" );
		 [Documented("The total number of page evictions executed by the page cache")]
		 public static readonly string PcEvictions = name( PAGE_CACHE_PREFIX, "evictions" );
		 [Documented("The total number of page faults happened in the page cache")]
		 public static readonly string PcPageFaults = name( PAGE_CACHE_PREFIX, "page_faults" );
		 [Documented("The total number of page hits happened in the page cache")]
		 public static readonly string PcHits = name( PAGE_CACHE_PREFIX, "hits" );
		 [Documented("The ratio of hits to the total number of lookups in the page cache")]
		 public static readonly string PcHitRatio = name( PAGE_CACHE_PREFIX, "hit_ratio" );
		 [Documented("The ratio of number of used pages to total number of available pages")]
		 public static readonly string PcUsageRatio = name( PAGE_CACHE_PREFIX, "usage_ratio" );

		 private readonly MetricRegistry _registry;
		 private readonly PageCacheCounters _pageCacheCounters;

		 public PageCacheMetrics( MetricRegistry registry, PageCacheCounters pageCacheCounters )
		 {
			  this._registry = registry;
			  this._pageCacheCounters = pageCacheCounters;
		 }

		 public override void Start()
		 {
			  _registry.register( PcPageFaults, ( Gauge<long> ) _pageCacheCounters.faults );
			  _registry.register( PcEvictions, ( Gauge<long> ) _pageCacheCounters.evictions );
			  _registry.register( PcPins, ( Gauge<long> ) _pageCacheCounters.pins );
			  _registry.register( PcUnpins, ( Gauge<long> ) _pageCacheCounters.unpins );
			  _registry.register( PcHits, ( Gauge<long> ) _pageCacheCounters.hits );
			  _registry.register( PcFlushes, ( Gauge<long> ) _pageCacheCounters.flushes );
			  _registry.register( PcEvictionExceptions, ( Gauge<long> ) _pageCacheCounters.evictionExceptions );
			  _registry.register( PcHitRatio, ( Gauge<double> ) _pageCacheCounters.hitRatio );
			  _registry.register( PcUsageRatio, ( Gauge<double> ) _pageCacheCounters.usageRatio );
		 }

		 public override void Stop()
		 {
			  _registry.remove( PcPageFaults );
			  _registry.remove( PcEvictions );
			  _registry.remove( PcPins );
			  _registry.remove( PcUnpins );
			  _registry.remove( PcHits );
			  _registry.remove( PcFlushes );
			  _registry.remove( PcEvictionExceptions );
			  _registry.remove( PcHitRatio );
			  _registry.remove( PcUsageRatio );
		 }
	}

}