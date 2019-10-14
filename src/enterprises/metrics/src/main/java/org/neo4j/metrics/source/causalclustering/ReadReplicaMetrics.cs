/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.metrics.source.causalclustering
{
	using Gauge = com.codahale.metrics.Gauge;
	using MetricRegistry = com.codahale.metrics.MetricRegistry;

	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.codahale.metrics.MetricRegistry.name;

	[Documented(".Read Replica Metrics")]
	public class ReadReplicaMetrics : LifecycleAdapter
	{
		 private const string CAUSAL_CLUSTERING_PREFIX = "neo4j.causal_clustering.read_replica";

		 [Documented("The total number of pull requests made by this instance")]
		 public static readonly string PullUpdates = name( CAUSAL_CLUSTERING_PREFIX, "pull_updates" );
		 [Documented("The highest transaction id requested in a pull update by this instance")]
		 public static readonly string PullUpdateHighestTxIdRequested = name( CAUSAL_CLUSTERING_PREFIX, "pull_update_highest_tx_id_requested" );
		 [Documented("The highest transaction id that has been pulled in the last pull updates by this instance")]
		 public static readonly string PullUpdateHighestTxIdReceived = name( CAUSAL_CLUSTERING_PREFIX, "pull_update_highest_tx_id_received" );

		 private Monitors _monitors;
		 private MetricRegistry _registry;

		 private readonly PullRequestMetric _pullRequestMetric = new PullRequestMetric();

		 public ReadReplicaMetrics( Monitors monitors, MetricRegistry registry )
		 {
			  this._monitors = monitors;
			  this._registry = registry;
		 }

		 public override void Start()
		 {
			  _monitors.addMonitorListener( _pullRequestMetric );

			  _registry.register( PullUpdates, ( Gauge<long> ) _pullRequestMetric.numberOfRequests );
			  _registry.register( PullUpdateHighestTxIdRequested, ( Gauge<long> ) _pullRequestMetric.lastRequestedTxId );
			  _registry.register( PullUpdateHighestTxIdReceived, ( Gauge<long> ) _pullRequestMetric.lastReceivedTxId );
		 }

		 public override void Stop()
		 {
			  _registry.remove( PullUpdates );
			  _registry.remove( PullUpdateHighestTxIdRequested );
			  _registry.remove( PullUpdateHighestTxIdReceived );

			  _monitors.removeMonitorListener( _pullRequestMetric );
		 }
	}

}