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
namespace Org.Neo4j.metrics.source.causalclustering
{
	using Gauge = com.codahale.metrics.Gauge;
	using MetricRegistry = com.codahale.metrics.MetricRegistry;

	using Documented = Org.Neo4j.Kernel.Impl.Annotations.Documented;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.codahale.metrics.MetricRegistry.name;

	[Documented(".CatchUp Metrics")]
	public class CatchUpMetrics : LifecycleAdapter
	{
		 private const string CAUSAL_CLUSTERING_PREFIX = "neo4j.causal_clustering.catchup";

		 [Documented("TX pull requests received from read replicas")]
		 public static readonly string TxPullRequestsReceived = name( CAUSAL_CLUSTERING_PREFIX, "tx_pull_requests_received" );

		 private Monitors _monitors;
		 private MetricRegistry _registry;
		 private readonly TxPullRequestsMetric _txPullRequestsMetric = new TxPullRequestsMetric();

		 public CatchUpMetrics( Monitors monitors, MetricRegistry registry )
		 {
			  this._monitors = monitors;
			  this._registry = registry;
		 }

		 public override void Start()
		 {
			  _monitors.addMonitorListener( _txPullRequestsMetric );
			  _registry.register( TxPullRequestsReceived, ( Gauge<long> ) _txPullRequestsMetric.txPullRequestsReceived );
		 }

		 public override void Stop()
		 {
			  _registry.remove( TxPullRequestsReceived );
			  _monitors.removeMonitorListener( _txPullRequestsMetric );
		 }
	}

}