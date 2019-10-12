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
namespace Neo4Net.metrics.source.cluster
{
	using Gauge = com.codahale.metrics.Gauge;
	using MetricRegistry = com.codahale.metrics.MetricRegistry;


	using SlaveUpdatePuller = Neo4Net.Kernel.ha.SlaveUpdatePuller;
	using ClusterMembers = Neo4Net.Kernel.ha.cluster.member.ClusterMembers;
	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.codahale.metrics.MetricRegistry.name;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher.MASTER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher.UNKNOWN;

	[Documented(".Cluster metrics")]
	public class ClusterMetrics : LifecycleAdapter
	{
		 private const string NAME_PREFIX = "neo4j.cluster";

		 [Documented("The total number of update pulls executed by this instance")]
		 public static readonly string SlavePullUpdates = name( NAME_PREFIX, "slave_pull_updates" );
		 [Documented("The highest transaction id that has been pulled in the last pull updates by this instance")]
		 public static readonly string SlavePullUpdateUpToTx = name( NAME_PREFIX, "slave_pull_update_up_to_tx" );
		 [Documented("Whether or not this instance is the master in the cluster")]
		 public static readonly string IsMaster = name( NAME_PREFIX, "is_master" );
		 [Documented("Whether or not this instance is available in the cluster")]
		 public static readonly string IsAvailable = name( NAME_PREFIX, "is_available" );

		 private readonly Monitors _monitors;
		 private readonly MetricRegistry _registry;
		 private readonly SlaveUpdatePullerMonitor _monitor = new SlaveUpdatePullerMonitor();
		 private readonly System.Func<ClusterMembers> _clusterMembers;

		 public ClusterMetrics( Monitors monitors, MetricRegistry registry, System.Func<ClusterMembers> clusterMembers )
		 {
			  this._monitors = monitors;
			  this._registry = registry;
			  this._clusterMembers = clusterMembers;
		 }

		 public override void Start()
		 {
			  _monitors.addMonitorListener( _monitor );

			  _registry.register( IsMaster, new RoleGauge( this, MASTER.equals ) );
			  _registry.register( IsAvailable, new RoleGauge( this, s => !UNKNOWN.Equals( s ) ) );

			  _registry.register( SlavePullUpdates, ( Gauge<long> )() => _monitor.events.get() );
			  _registry.register( SlavePullUpdateUpToTx, ( Gauge<long> )() => _monitor.lastAppliedTxId );
		 }

		 public override void Stop()
		 {
			  _registry.remove( SlavePullUpdates );
			  _registry.remove( SlavePullUpdateUpToTx );

			  _registry.remove( IsMaster );
			  _registry.remove( IsAvailable );

			  _monitors.removeMonitorListener( _monitor );
		 }

		 private class SlaveUpdatePullerMonitor : SlaveUpdatePuller.Monitor
		 {
			  internal AtomicLong Events = new AtomicLong();
			  internal volatile long LastAppliedTxId;

			  public override void PulledUpdates( long lastAppliedTxId )
			  {
					Events.incrementAndGet();
					this.LastAppliedTxId = lastAppliedTxId;
			  }
		 }

		 private class RoleGauge : Gauge<int>
		 {
			 private readonly ClusterMetrics _outerInstance;

			  internal System.Predicate<string> RolePredicate;

			  internal RoleGauge( ClusterMetrics outerInstance, System.Predicate<string> rolePredicate )
			  {
				  this._outerInstance = outerInstance;
					this.RolePredicate = rolePredicate;
			  }

			  public override int? Value
			  {
				  get
				  {
						ClusterMembers clusterMembers = _outerInstance.clusterMembers.get();
						return clusterMembers != null && RolePredicate.test( clusterMembers.CurrentMemberRole ) ? 1 : 0;
				  }
			  }
		 }
	}

}