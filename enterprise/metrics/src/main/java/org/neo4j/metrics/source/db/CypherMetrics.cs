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

	using PlanCacheMetricsMonitor = Org.Neo4j.Cypher.PlanCacheMetricsMonitor;
	using Documented = Org.Neo4j.Kernel.Impl.Annotations.Documented;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.codahale.metrics.MetricRegistry.name;

	[Documented(".Cypher metrics")]
	public class CypherMetrics : LifecycleAdapter
	{
		 private const string NAME_PREFIX = "neo4j.cypher";

		 [Documented("The total number of times Cypher has decided to re-plan a query")]
		 public static readonly string ReplanEvents = name( NAME_PREFIX, "replan_events" );

		 [Documented("The total number of seconds waited between query replans")]
		 public static readonly string ReplanWaitTime = name( NAME_PREFIX, "replan_wait_time" );

		 private readonly MetricRegistry _registry;
		 private readonly Monitors _monitors;
		 private readonly PlanCacheMetricsMonitor _cacheMonitor = new PlanCacheMetricsMonitor();

		 public CypherMetrics( MetricRegistry registry, Monitors monitors )
		 {
			  this._registry = registry;
			  this._monitors = monitors;
		 }

		 public override void Start()
		 {
			  _monitors.addMonitorListener( _cacheMonitor );
			  _registry.register( ReplanEvents, ( Gauge<long> ) _cacheMonitor.numberOfReplans );
			  _registry.register( ReplanWaitTime, ( Gauge<long> ) _cacheMonitor.replanWaitTime );
		 }

		 public override void Stop()
		 {
			  _registry.remove( ReplanEvents );
			  _registry.remove( ReplanWaitTime );
			  _monitors.removeMonitorListener( _cacheMonitor );
		 }
	}


}