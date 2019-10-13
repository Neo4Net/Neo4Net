using System;

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
namespace Neo4Net.Bolt.runtime
{

	using PackOutput = Neo4Net.Bolt.v1.packstream.PackOutput;
	using Job = Neo4Net.Bolt.v1.runtime.Job;
	using LogService = Neo4Net.Logging.@internal.LogService;

	public class MetricsReportingBoltConnection : DefaultBoltConnection
	{
		 private readonly BoltConnectionMetricsMonitor _metricsMonitor;
		 private readonly Clock _clock;

		 internal MetricsReportingBoltConnection( BoltChannel channel, PackOutput output, BoltStateMachine machine, LogService logService, BoltConnectionLifetimeListener listener, BoltConnectionQueueMonitor queueMonitor, BoltConnectionMetricsMonitor metricsMonitor, Clock clock ) : this( channel, output, machine, logService, listener, queueMonitor, DefaultMaxBatchSize, metricsMonitor, clock )
		 {
		 }

		 internal MetricsReportingBoltConnection( BoltChannel channel, PackOutput output, BoltStateMachine machine, LogService logService, BoltConnectionLifetimeListener listener, BoltConnectionQueueMonitor queueMonitor, int maxBatchSize, BoltConnectionMetricsMonitor metricsMonitor, Clock clock ) : base( channel, output, machine, logService, listener, queueMonitor, maxBatchSize )
		 {
			  this._metricsMonitor = metricsMonitor;
			  this._clock = clock;
		 }

		 public override void Start()
		 {
			  base.Start();
			  _metricsMonitor.connectionOpened();
		 }

		 public override void Enqueue( Job job )
		 {
			  _metricsMonitor.messageReceived();
			  long queuedAt = _clock.millis();
			  base.Enqueue(machine =>
			  {
				long queueTime = _clock.millis() - queuedAt;
				_metricsMonitor.messageProcessingStarted( queueTime );
				try
				{
					 job.Perform( machine );
					 _metricsMonitor.messageProcessingCompleted( _clock.millis() - queuedAt - queueTime );
				}
				catch ( Exception t )
				{
					 _metricsMonitor.messageProcessingFailed();
					 throw t;
				}
			  });
		 }

		 public override bool ProcessNextBatch( int batchCount, bool exitIfNoJobsAvailable )
		 {
			  _metricsMonitor.connectionActivated();

			  try
			  {
					bool continueProcessing = base.ProcessNextBatch( batchCount, exitIfNoJobsAvailable );

					if ( !continueProcessing )
					{
						 _metricsMonitor.connectionClosed();
					}

					return continueProcessing;
			  }
			  finally
			  {
					_metricsMonitor.connectionWaiting();
			  }
		 }

	}

}