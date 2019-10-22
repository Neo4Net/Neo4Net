using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.metrics.source.db
{
	using Gauge = com.codahale.metrics.Gauge;
	using MetricRegistry = com.codahale.metrics.MetricRegistry;


	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using DefaultTransactionTracer = Neo4Net.Kernel.Impl.Api.DefaultTransactionTracer;
	using LogRotationMonitor = Neo4Net.Kernel.Impl.Api.LogRotationMonitor;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using EventReporter = Neo4Net.metrics.output.EventReporter;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.codahale.metrics.MetricRegistry.name;

	[Documented(".Database log rotation metrics")]
	public class LogRotationMetrics : LifecycleAdapter
	{
		 private const string LOG_ROTATION_PREFIX = "Neo4Net.log_rotation";

		 [Documented("The total number of transaction log rotations executed so far")]
		 public static readonly string LogRotationEvents = name( LOG_ROTATION_PREFIX, "events" );
		 [Documented("The total time spent in rotating transaction logs so far")]
		 public static readonly string LogRotationTotalTime = name( LOG_ROTATION_PREFIX, "total_time" );
		 [Documented("The duration of the log rotation event")]
		 public static readonly string LogRotationDuration = name( LOG_ROTATION_PREFIX, "log_rotation_duration" );

		 private readonly MetricRegistry _registry;
		 private readonly Monitors _monitors;
		 private readonly System.Func<LogRotationMonitor> _logRotationMonitorSupplier;
		 private readonly DefaultTransactionTracer.Monitor _listener;

		 public LogRotationMetrics( EventReporter reporter, MetricRegistry registry, Monitors monitors, System.Func<LogRotationMonitor> logRotationMonitorSupplier )
		 {
			  this._registry = registry;
			  this._monitors = monitors;
			  this._logRotationMonitorSupplier = logRotationMonitorSupplier;
			  this._listener = durationMillis =>
			  {
				SortedDictionary<string, Gauge> gauges = new SortedDictionary<string, Gauge>();
				gauges.put( LogRotationDuration, () => durationMillis );
				reporter.Report( gauges, emptySortedMap(), emptySortedMap(), emptySortedMap(), emptySortedMap() );
			  };
		 }

		 public override void Start()
		 {
			  _monitors.addMonitorListener( _listener );

			  LogRotationMonitor monitor = this._logRotationMonitorSupplier.get();
			  _registry.register( LogRotationEvents, ( Gauge<long> ) monitor.numberOfLogRotationEvents );
			  _registry.register( LogRotationTotalTime, ( Gauge<long> ) monitor.logRotationAccumulatedTotalTimeMillis );
		 }

		 public override void Stop()
		 {
			  _monitors.removeMonitorListener( _listener );

			  _registry.remove( LogRotationEvents );
			  _registry.remove( LogRotationTotalTime );
		 }
	}

}