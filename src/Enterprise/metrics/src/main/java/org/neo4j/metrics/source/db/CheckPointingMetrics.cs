using System.Collections.Generic;

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
namespace Neo4Net.metrics.source.db
{
	using Gauge = com.codahale.metrics.Gauge;
	using MetricRegistry = com.codahale.metrics.MetricRegistry;


	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using CheckPointerMonitor = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointerMonitor;
	using DefaultCheckPointerTracer = Neo4Net.Kernel.impl.transaction.log.checkpoint.DefaultCheckPointerTracer;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using EventReporter = Neo4Net.metrics.output.EventReporter;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.codahale.metrics.MetricRegistry.name;

	[Documented(".Database checkpointing metrics")]
	public class CheckPointingMetrics : LifecycleAdapter
	{
		 private const string CHECK_POINT_PREFIX = "neo4j.check_point";

		 [Documented("The total number of check point events executed so far")]
		 public static readonly string CheckPointEvents = name( CHECK_POINT_PREFIX, "events" );
		 [Documented("The total time spent in check pointing so far")]
		 public static readonly string CheckPointTotalTime = name( CHECK_POINT_PREFIX, "total_time" );
		 [Documented("The duration of the check point event")]
		 public static readonly string CheckPointDuration = name( CHECK_POINT_PREFIX, "check_point_duration" );

		 private readonly MetricRegistry _registry;
		 private readonly Monitors _monitors;
		 private readonly System.Func<CheckPointerMonitor> _checkPointerMonitorSupplier;
		 private readonly DefaultCheckPointerTracer.Monitor _listener;

		 public CheckPointingMetrics( EventReporter reporter, MetricRegistry registry, Monitors monitors, System.Func<CheckPointerMonitor> checkPointerMonitorSupplier )
		 {
			  this._registry = registry;
			  this._monitors = monitors;
			  this._checkPointerMonitorSupplier = checkPointerMonitorSupplier;
			  this._listener = durationMillis =>
			  {
				SortedDictionary<string, Gauge> gauges = new SortedDictionary<string, Gauge>();
				gauges.put( CheckPointDuration, () => durationMillis );
				reporter.Report( gauges, emptySortedMap(), emptySortedMap(), emptySortedMap(), emptySortedMap() );
			  };
		 }

		 public override void Start()
		 {
			  _monitors.addMonitorListener( _listener );

			  CheckPointerMonitor checkPointerMonitor = _checkPointerMonitorSupplier.get();
			  _registry.register( CheckPointEvents, ( Gauge<long> ) checkPointerMonitor.numberOfCheckPointEvents );
			  _registry.register( CheckPointTotalTime, ( Gauge<long> ) checkPointerMonitor.checkPointAccumulatedTotalTimeMillis );
		 }

		 public override void Stop()
		 {
			  _monitors.removeMonitorListener( _listener );

			  _registry.remove( CheckPointEvents );
			  _registry.remove( CheckPointTotalTime );
		 }
	}

}