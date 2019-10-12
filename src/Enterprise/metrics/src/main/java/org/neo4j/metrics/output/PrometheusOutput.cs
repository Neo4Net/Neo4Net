using System.Collections.Concurrent;
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
namespace Neo4Net.metrics.output
{
	using Counter = com.codahale.metrics.Counter;
	using Gauge = com.codahale.metrics.Gauge;
	using Histogram = com.codahale.metrics.Histogram;
	using Meter = com.codahale.metrics.Meter;
	using MetricRegistry = com.codahale.metrics.MetricRegistry;
	using Timer = com.codahale.metrics.Timer;
	using CollectorRegistry = io.prometheus.client.CollectorRegistry;
	using DropwizardExports = io.prometheus.client.dropwizard.DropwizardExports;


	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using ConnectorPortRegister = Neo4Net.Kernel.configuration.ConnectorPortRegister;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using Log = Neo4Net.Logging.Log;

	/// <summary>
	/// Prometheus poll data from clients, this exposes a HTTP endpoint at a configurable port.
	/// </summary>
	public class PrometheusOutput : Lifecycle, EventReporter
	{
		 protected internal PrometheusHttpServer Server;
		 private readonly HostnamePort _hostnamePort;
		 private readonly MetricRegistry _registry;
		 private readonly Log _logger;
		 private readonly ConnectorPortRegister _portRegister;
		 private readonly IDictionary<string, object> _registeredEvents = new ConcurrentDictionary<string, object>();
		 private readonly MetricRegistry _eventRegistry;

		 internal PrometheusOutput( HostnamePort hostnamePort, MetricRegistry registry, Log logger, ConnectorPortRegister portRegister )
		 {
			  this._hostnamePort = hostnamePort;
			  this._registry = registry;
			  this._logger = logger;
			  this._portRegister = portRegister;
			  this._eventRegistry = new MetricRegistry();
		 }

		 public override void Init()
		 {
			  // Setup prometheus collector
			  CollectorRegistry.defaultRegistry.register( new DropwizardExports( _registry ) );

			  // We have to have a separate registry to not pollute the default one
			  CollectorRegistry.defaultRegistry.register( new DropwizardExports( _eventRegistry ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
		 public override void Start()
		 {
			  if ( Server == null )
			  {
					Server = new PrometheusHttpServer( _hostnamePort.Host, _hostnamePort.Port );
					_portRegister.register( "prometheus", Server.Address );
					_logger.info( "Started publishing Prometheus metrics at http://" + Server.Address + "/metrics" );
			  }
		 }

		 public override void Stop()
		 {
			  if ( Server != null )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String address = server.getAddress().toString();
					string address = Server.Address.ToString();
					Server.stop();
					Server = null;
					_logger.info( "Stopped Prometheus endpoint at http://" + address + "/metrics" );
			  }
		 }

		 public override void Shutdown()
		 {
			  this.Stop();
		 }

		 public override void Report( SortedDictionary<string, Gauge> gauges, SortedDictionary<string, Counter> counters, SortedDictionary<string, Histogram> histograms, SortedDictionary<string, Meter> meters, SortedDictionary<string, Timer> timers )
		 {
			  // Prometheus does not support events, just output the latest event that occurred
			  string metricKey = gauges.firstKey();
			  if ( !_registeredEvents.ContainsKey( metricKey ) )
			  {
					_eventRegistry.register( metricKey, ( Gauge )() => _registeredEvents[metricKey] );
			  }

			  _registeredEvents[metricKey] = gauges[metricKey].Value;
		 }
	}

}