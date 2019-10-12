/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Bolt.runtime
{

	using TransportThrottleGroup = Org.Neo4j.Bolt.transport.TransportThrottleGroup;
	using ChunkedOutput = Org.Neo4j.Bolt.v1.transport.ChunkedOutput;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LogService = Org.Neo4j.Logging.@internal.LogService;

	public class DefaultBoltConnectionFactory : BoltConnectionFactory
	{
		 private readonly BoltSchedulerProvider _schedulerProvider;
		 private readonly TransportThrottleGroup _throttleGroup;
		 private readonly LogService _logService;
		 private readonly Clock _clock;
		 private readonly Config _config;
		 private readonly Monitors _monitors;
		 private readonly BoltConnectionMetricsMonitor _metricsMonitor;

		 public DefaultBoltConnectionFactory( BoltSchedulerProvider schedulerProvider, TransportThrottleGroup throttleGroup, Config config, LogService logService, Clock clock, Monitors monitors )
		 {
			  this._schedulerProvider = schedulerProvider;
			  this._throttleGroup = throttleGroup;
			  this._config = config;
			  this._logService = logService;
			  this._clock = clock;
			  this._monitors = monitors;
			  this._metricsMonitor = monitors.NewMonitor( typeof( BoltConnectionMetricsMonitor ) );
		 }

		 public override BoltConnection NewConnection( BoltChannel channel, BoltStateMachine stateMachine )
		 {
			  requireNonNull( channel );
			  requireNonNull( stateMachine );

			  BoltScheduler scheduler = _schedulerProvider.get( channel );
			  BoltConnectionReadLimiter readLimiter = CreateReadLimiter( _config, _logService );
			  BoltConnectionQueueMonitor connectionQueueMonitor = new BoltConnectionQueueMonitorAggregate( scheduler, readLimiter );
			  ChunkedOutput chunkedOutput = new ChunkedOutput( channel.RawChannel(), _throttleGroup );

			  BoltConnection connection;
			  if ( _monitors.hasListeners( typeof( BoltConnectionMetricsMonitor ) ) )
			  {
					connection = new MetricsReportingBoltConnection( channel, chunkedOutput, stateMachine, _logService, scheduler, connectionQueueMonitor, _metricsMonitor, _clock );
			  }
			  else
			  {
					connection = new DefaultBoltConnection( channel, chunkedOutput, stateMachine, _logService, scheduler, connectionQueueMonitor );
			  }

			  connection.Start();

			  return connection;
		 }

		 private static BoltConnectionReadLimiter CreateReadLimiter( Config config, LogService logService )
		 {
			  int lowWatermark = config.Get( GraphDatabaseSettings.bolt_inbound_message_throttle_low_water_mark );
			  int highWatermark = config.Get( GraphDatabaseSettings.bolt_inbound_message_throttle_high_water_mark );
			  return new BoltConnectionReadLimiter( logService, lowWatermark, highWatermark );
		 }
	}

}