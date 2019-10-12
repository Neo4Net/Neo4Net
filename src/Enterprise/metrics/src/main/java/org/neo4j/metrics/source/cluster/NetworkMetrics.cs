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

	using ToNetworkStoreWriter = Neo4Net.com.storecopy.ToNetworkStoreWriter;
	using MasterClient320 = Neo4Net.Kernel.ha.MasterClient320;
	using MasterServer = Neo4Net.Kernel.ha.com.master.MasterServer;
	using Documented = Neo4Net.Kernel.Impl.Annotations.Documented;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.codahale.metrics.MetricRegistry.name;

	[Documented(".Network metrics")]
	public class NetworkMetrics : LifecycleAdapter
	{
		 private const string NAME_PREFIX = "neo4j.network";

		 [Documented("The amount of bytes transmitted on the network containing the transaction data from a slave " + "to the master in order to be committed")]
		 public static readonly string SlaveNetworkTxWrites = name( NAME_PREFIX, "slave_network_tx_writes" );
		 [Documented("The amount of bytes transmitted on the network while copying stores from a machines to another")]
		 public static readonly string MasterNetworkStoreWrites = name( NAME_PREFIX, "master_network_store_writes" );
		 [Documented("The amount of bytes transmitted on the network containing the transaction data from a master " + "to the slaves in order to propagate committed transactions")]
		 public static readonly string MasterNetworkTxWrites = name( NAME_PREFIX, "master_network_tx_writes" );

		 private readonly MetricRegistry _registry;
		 private readonly Monitors _monitors;
		 private readonly ByteCountsMetric _masterNetworkTransactionWrites = new ByteCountsMetric();
		 private readonly ByteCountsMetric _masterNetworkStoreWrites = new ByteCountsMetric();
		 private readonly ByteCountsMetric _slaveNetworkTransactionWrites = new ByteCountsMetric();

		 public NetworkMetrics( MetricRegistry registry, Monitors monitors )
		 {
			  this._registry = registry;
			  this._monitors = monitors;
		 }

		 public override void Start()
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  _monitors.addMonitorListener( _masterNetworkTransactionWrites, typeof( MasterServer ).FullName );
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  _monitors.addMonitorListener( _masterNetworkStoreWrites, typeof( ToNetworkStoreWriter ).FullName, ToNetworkStoreWriter.STORE_COPIER_MONITOR_TAG );
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  _monitors.addMonitorListener( _slaveNetworkTransactionWrites, typeof( MasterClient320 ).FullName );

			  _registry.register( MasterNetworkTxWrites, ( Gauge<long> ) _masterNetworkTransactionWrites.getBytesWritten );
			  _registry.register( MasterNetworkStoreWrites, ( Gauge<long> ) _masterNetworkStoreWrites.getBytesWritten );
			  _registry.register( SlaveNetworkTxWrites, ( Gauge<long> ) _slaveNetworkTransactionWrites.getBytesWritten );
		 }

		 public override void Stop()
		 {
			  _registry.remove( MasterNetworkTxWrites );
			  _registry.remove( MasterNetworkStoreWrites );
			  _registry.remove( SlaveNetworkTxWrites );

			  _monitors.removeMonitorListener( _masterNetworkTransactionWrites );
			  _monitors.removeMonitorListener( _masterNetworkStoreWrites );
			  _monitors.removeMonitorListener( _slaveNetworkTransactionWrites );
		 }
	}

}