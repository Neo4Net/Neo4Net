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
namespace Neo4Net.metrics.output
{
	using Counter = com.codahale.metrics.Counter;
	using Gauge = com.codahale.metrics.Gauge;
	using Histogram = com.codahale.metrics.Histogram;
	using Meter = com.codahale.metrics.Meter;
	using MetricFilter = com.codahale.metrics.MetricFilter;
	using MetricRegistry = com.codahale.metrics.MetricRegistry;
	using Timer = com.codahale.metrics.Timer;
	using Graphite = com.codahale.metrics.graphite.Graphite;
	using GraphiteReporter = com.codahale.metrics.graphite.GraphiteReporter;


	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using Log = Neo4Net.Logging.Log;

	public class GraphiteOutput : Lifecycle, EventReporter
	{
		 private readonly HostnamePort _hostnamePort;
		 private readonly long _period;
		 private readonly MetricRegistry _registry;
		 private readonly Log _logger;
		 private readonly string _prefix;

		 private GraphiteReporter _graphiteReporter;

		 public GraphiteOutput( HostnamePort hostnamePort, long period, MetricRegistry registry, Log logger, string prefix )
		 {
			  this._hostnamePort = hostnamePort;
			  this._period = period;
			  this._registry = registry;
			  this._logger = logger;
			  this._prefix = prefix;
		 }

		 public override void Init()
		 {
			  // Setup Graphite reporting
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final com.codahale.metrics.graphite.Graphite graphite = new com.codahale.metrics.graphite.Graphite(hostnamePort.getHost(), hostnamePort.getPort());
			  Graphite graphite = new Graphite( _hostnamePort.Host, _hostnamePort.Port );

			  _graphiteReporter = GraphiteReporter.forRegistry( _registry ).prefixedWith( _prefix ).convertRatesTo( TimeUnit.SECONDS ).convertDurationsTo( TimeUnit.MILLISECONDS ).filter( MetricFilter.ALL ).build( graphite );
		 }

		 public override void Start()
		 {
			  _graphiteReporter.start( _period, TimeUnit.MILLISECONDS );
			  _logger.info( "Sending metrics to Graphite server at " + _hostnamePort );
		 }

		 public override void Stop()
		 {
			  _graphiteReporter.close();
		 }

		 public override void Shutdown()
		 {
			  _graphiteReporter = null;
		 }

		 public override void Report( SortedDictionary<string, Gauge> gauges, SortedDictionary<string, Counter> counters, SortedDictionary<string, Histogram> histograms, SortedDictionary<string, Meter> meters, SortedDictionary<string, Timer> timers )
		 {
			  /*
			   * The synchronized is needed here since the `report` method called below is also called by the recurring
			   * scheduled thread.  In order to avoid races with that thread we synchronize on the same monitor
			   * before reporting.
			   */
			  lock ( _graphiteReporter )
			  {
					_graphiteReporter.report( gauges, counters, histograms, meters, timers );
			  }
		 }
	}

}