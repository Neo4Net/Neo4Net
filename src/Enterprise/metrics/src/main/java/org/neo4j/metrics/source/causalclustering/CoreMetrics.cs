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
namespace Neo4Net.metrics.source.causalclustering
{
	using Gauge = com.codahale.metrics.Gauge;
	using MetricRegistry = com.codahale.metrics.MetricRegistry;

	using CoreMetaData = Neo4Net.causalclustering.core.consensus.CoreMetaData;
	using RaftMessages = Neo4Net.causalclustering.core.consensus.RaftMessages;
	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.codahale.metrics.MetricRegistry.name;

	[Documented(".Core metrics")]
	public class CoreMetrics : LifecycleAdapter
	{
		 private const string CAUSAL_CLUSTERING_PREFIX = "neo4j.causal_clustering.core";

		 [Documented("Append index of the RAFT log")]
		 public static readonly string AppendIndex = name( CAUSAL_CLUSTERING_PREFIX, "append_index" );
		 [Documented("Commit index of the RAFT log")]
		 public static readonly string CommitIndex = name( CAUSAL_CLUSTERING_PREFIX, "commit_index" );
		 [Documented("RAFT Term of this server")]
		 public static readonly string Term = name( CAUSAL_CLUSTERING_PREFIX, "term" );
		 [Documented("Transaction retries")]
		 public static readonly string TxRetries = name( CAUSAL_CLUSTERING_PREFIX, "tx_retries" );
		 [Documented("Is this server the leader?")]
		 public static readonly string IsLeader = name( CAUSAL_CLUSTERING_PREFIX, "is_leader" );
		 [Documented("In-flight cache total bytes")]
		 public static readonly string TotalBytes = name( CAUSAL_CLUSTERING_PREFIX, "in_flight_cache", "total_bytes" );
		 [Documented("In-flight cache max bytes")]
		 public static readonly string MaxBytes = name( CAUSAL_CLUSTERING_PREFIX, "in_flight_cache", "max_bytes" );
		 [Documented("In-flight cache element count")]
		 public static readonly string ElementCount = name( CAUSAL_CLUSTERING_PREFIX, "in_flight_cache", "element_count" );
		 [Documented("In-flight cache maximum elements")]
		 public static readonly string MaxElements = name( CAUSAL_CLUSTERING_PREFIX, "in_flight_cache", "max_elements" );
		 [Documented("In-flight cache hits")]
		 public static readonly string Hits = name( CAUSAL_CLUSTERING_PREFIX, "in_flight_cache", "hits" );
		 [Documented("In-flight cache misses")]
		 public static readonly string Misses = name( CAUSAL_CLUSTERING_PREFIX, "in_flight_cache", "misses" );
		 [Documented("Delay between RAFT message receive and process")]
		 public static readonly string Delay = name( CAUSAL_CLUSTERING_PREFIX, "message_processing_delay" );
		 [Documented("Timer for RAFT message processing")]
		 public static readonly string Timer = name( CAUSAL_CLUSTERING_PREFIX, "message_processing_timer" );
		 [Documented("Raft replication new request count")]
		 public static readonly string ReplicationNew = name( CAUSAL_CLUSTERING_PREFIX, "replication_new" );
		 [Documented("Raft replication attempt count")]
		 public static readonly string ReplicationAttempt = name( CAUSAL_CLUSTERING_PREFIX, "replication_attempt" );
		 [Documented("Raft Replication success count")]
		 public static readonly string ReplicationSuccess = name( CAUSAL_CLUSTERING_PREFIX, "replication_success" );
		 [Documented("Raft Replication fail count")]
		 public static readonly string ReplicationFail = name( CAUSAL_CLUSTERING_PREFIX, "replication_fail" );

		 private Monitors _monitors;
		 private MetricRegistry _registry;
		 private System.Func<CoreMetaData> _coreMetaData;

		 private readonly RaftLogCommitIndexMetric _raftLogCommitIndexMetric = new RaftLogCommitIndexMetric();
		 private readonly RaftLogAppendIndexMetric _raftLogAppendIndexMetric = new RaftLogAppendIndexMetric();
		 private readonly RaftTermMetric _raftTermMetric = new RaftTermMetric();
		 private readonly TxPullRequestsMetric _txPullRequestsMetric = new TxPullRequestsMetric();
		 private readonly TxRetryMetric _txRetryMetric = new TxRetryMetric();
		 private readonly InFlightCacheMetric _inFlightCacheMetric = new InFlightCacheMetric();
		 private readonly RaftMessageProcessingMetric _raftMessageProcessingMetric = RaftMessageProcessingMetric.Create();
		 private readonly ReplicationMetric _replicationMetric = new ReplicationMetric();

		 public CoreMetrics( Monitors monitors, MetricRegistry registry, System.Func<CoreMetaData> coreMetaData )
		 {
			  this._monitors = monitors;
			  this._registry = registry;
			  this._coreMetaData = coreMetaData;
		 }

		 public override void Start()
		 {
			  _monitors.addMonitorListener( _raftLogCommitIndexMetric );
			  _monitors.addMonitorListener( _raftLogAppendIndexMetric );
			  _monitors.addMonitorListener( _raftTermMetric );
			  _monitors.addMonitorListener( _txPullRequestsMetric );
			  _monitors.addMonitorListener( _txRetryMetric );
			  _monitors.addMonitorListener( _inFlightCacheMetric );
			  _monitors.addMonitorListener( _raftMessageProcessingMetric );
			  _monitors.addMonitorListener( _replicationMetric );

			  _registry.register( CommitIndex, ( Gauge<long> ) _raftLogCommitIndexMetric.commitIndex );
			  _registry.register( AppendIndex, ( Gauge<long> ) _raftLogAppendIndexMetric.appendIndex );
			  _registry.register( Term, ( Gauge<long> ) _raftTermMetric.term );
			  _registry.register( TxRetries, ( Gauge<long> ) _txRetryMetric.transactionsRetries );
			  _registry.register( IsLeader, new LeaderGauge( this ) );
			  _registry.register( TotalBytes, ( Gauge<long> ) _inFlightCacheMetric.getTotalBytes );
			  _registry.register( Hits, ( Gauge<long> ) _inFlightCacheMetric.getHits );
			  _registry.register( Misses, ( Gauge<long> ) _inFlightCacheMetric.getMisses );
			  _registry.register( MaxBytes, ( Gauge<long> ) _inFlightCacheMetric.getMaxBytes );
			  _registry.register( MaxElements, ( Gauge<long> ) _inFlightCacheMetric.getMaxElements );
			  _registry.register( ElementCount, ( Gauge<long> ) _inFlightCacheMetric.getElementCount );
			  _registry.register( Delay, ( Gauge<long> ) _raftMessageProcessingMetric.delay );
			  _registry.register( Timer, _raftMessageProcessingMetric.timer() );
			  _registry.register( ReplicationNew, ( Gauge<long> ) _replicationMetric.newReplicationCount );
			  _registry.register( ReplicationAttempt, ( Gauge<long> ) _replicationMetric.attemptCount );
			  _registry.register( ReplicationSuccess, ( Gauge<long> ) _replicationMetric.successCount );
			  _registry.register( ReplicationFail, ( Gauge<long> ) _replicationMetric.failCount );

			  foreach ( Neo4Net.causalclustering.core.consensus.RaftMessages_Type type in Enum.GetValues( typeof( Neo4Net.causalclustering.core.consensus.RaftMessages_Type ) ) )
			  {
					_registry.register( MessageTimerName( type ), _raftMessageProcessingMetric.timer( type ) );
			  }
		 }

		 public override void Stop()
		 {
			  _registry.remove( CommitIndex );
			  _registry.remove( AppendIndex );
			  _registry.remove( Term );
			  _registry.remove( TxRetries );
			  _registry.remove( IsLeader );
			  _registry.remove( TotalBytes );
			  _registry.remove( Hits );
			  _registry.remove( Misses );
			  _registry.remove( MaxBytes );
			  _registry.remove( MaxElements );
			  _registry.remove( ElementCount );
			  _registry.remove( Delay );
			  _registry.remove( Timer );
			  _registry.remove( ReplicationNew );
			  _registry.remove( ReplicationAttempt );
			  _registry.remove( ReplicationSuccess );
			  _registry.remove( ReplicationFail );

			  foreach ( Neo4Net.causalclustering.core.consensus.RaftMessages_Type type in Enum.GetValues( typeof( Neo4Net.causalclustering.core.consensus.RaftMessages_Type ) ) )
			  {
					_registry.remove( MessageTimerName( type ) );
			  }

			  _monitors.removeMonitorListener( _raftLogCommitIndexMetric );
			  _monitors.removeMonitorListener( _raftLogAppendIndexMetric );
			  _monitors.removeMonitorListener( _raftTermMetric );
			  _monitors.removeMonitorListener( _txPullRequestsMetric );
			  _monitors.removeMonitorListener( _txRetryMetric );
			  _monitors.removeMonitorListener( _inFlightCacheMetric );
			  _monitors.removeMonitorListener( _raftMessageProcessingMetric );
			  _monitors.removeMonitorListener( _replicationMetric );
		 }

		 private string MessageTimerName( Neo4Net.causalclustering.core.consensus.RaftMessages_Type type )
		 {
			  return name( Timer, type.name().ToLower() );
		 }

		 private class LeaderGauge : Gauge<int>
		 {
			 private readonly CoreMetrics _outerInstance;

			 public LeaderGauge( CoreMetrics outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override int? Value
			  {
				  get
				  {
						return outerInstance.coreMetaData.get().Leader ? 1 : 0;
				  }
			  }
		 }
	}

}