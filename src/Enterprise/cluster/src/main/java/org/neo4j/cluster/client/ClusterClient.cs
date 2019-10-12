using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.cluster.client
{

	using BindingNotifier = Neo4Net.cluster.com.BindingNotifier;
	using AtomicBroadcast = Neo4Net.cluster.protocol.atomicbroadcast.AtomicBroadcast;
	using AtomicBroadcastListener = Neo4Net.cluster.protocol.atomicbroadcast.AtomicBroadcastListener;
	using Payload = Neo4Net.cluster.protocol.atomicbroadcast.Payload;
	using Cluster = Neo4Net.cluster.protocol.cluster.Cluster;
	using ClusterConfiguration = Neo4Net.cluster.protocol.cluster.ClusterConfiguration;
	using ClusterListener = Neo4Net.cluster.protocol.cluster.ClusterListener;
	using Election = Neo4Net.cluster.protocol.election.Election;
	using Heartbeat = Neo4Net.cluster.protocol.heartbeat.Heartbeat;
	using HeartbeatListener = Neo4Net.cluster.protocol.heartbeat.HeartbeatListener;
	using Snapshot = Neo4Net.cluster.protocol.snapshot.Snapshot;
	using SnapshotProvider = Neo4Net.cluster.protocol.snapshot.SnapshotProvider;
	using Neo4Net.cluster.statemachine;
	using Timeouts = Neo4Net.cluster.timeout.Timeouts;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;

	/// <summary>
	/// These are used as clients for a Neo4j cluster. From here you can perform cluster management operations, like joining
	/// and leaving clusters, as well as adding listeners for cluster events such as elections and heartbeart failures.
	/// <p/>
	/// Instances of this class mainly acts as a facade for the internal distributed state machines, represented by the
	/// individual
	/// interfaces implemented here. See the respective interfaces it implements for details on operations.
	/// <p/>
	/// To create one you should use the <seealso cref="ClusterClientModule"/>.
	/// </summary>
	public class ClusterClient : ClusterMonitor, Cluster, AtomicBroadcast, Snapshot, Election, BindingNotifier
	{
		 private readonly Cluster _cluster;
		 private readonly AtomicBroadcast _broadcast;
		 private readonly Heartbeat _heartbeat;
		 private readonly Snapshot _snapshot;
		 private readonly Election _election;
		 private LifeSupport _life;
		 private ProtocolServer _protocolServer;

		 public ClusterClient( LifeSupport life, ProtocolServer protocolServer )
		 {
			  this._life = life;
			  this._protocolServer = protocolServer;

			  _cluster = protocolServer.NewClient( typeof( Cluster ) );
			  _broadcast = protocolServer.NewClient( typeof( AtomicBroadcast ) );
			  _heartbeat = protocolServer.NewClient( typeof( Heartbeat ) );
			  _snapshot = protocolServer.NewClient( typeof( Snapshot ) );
			  _election = protocolServer.NewClient( typeof( Election ) );
		 }

		 public override void Broadcast( Payload payload )
		 {
			  _broadcast.broadcast( payload );
		 }

		 public override void AddAtomicBroadcastListener( AtomicBroadcastListener listener )
		 {
			  _broadcast.addAtomicBroadcastListener( listener );
		 }

		 public override void RemoveAtomicBroadcastListener( AtomicBroadcastListener listener )
		 {
			  _broadcast.removeAtomicBroadcastListener( listener );
		 }

		 public override void Create( string clusterName )
		 {
			  _cluster.create( clusterName );
		 }

		 public override Future<ClusterConfiguration> Join( string clusterName, params URI[] otherServerUrls )
		 {
			  return _cluster.join( clusterName, otherServerUrls );
		 }

		 public override void Leave()
		 {
			  _cluster.leave();
		 }

		 public override void AddClusterListener( ClusterListener listener )
		 {
			  _cluster.addClusterListener( listener );
		 }

		 public override void RemoveClusterListener( ClusterListener listener )
		 {
			  _cluster.removeClusterListener( listener );
		 }

		 public override void AddHeartbeatListener( HeartbeatListener listener )
		 {
			  _heartbeat.addHeartbeatListener( listener );
		 }

		 public override void RemoveHeartbeatListener( HeartbeatListener listener )
		 {
			  _heartbeat.removeHeartbeatListener( listener );
		 }

		 public override void Demote( InstanceId node )
		 {
			  _election.demote( node );
		 }

		 public override void PerformRoleElections()
		 {
			  _election.performRoleElections();
		 }

		 public virtual SnapshotProvider SnapshotProvider
		 {
			 set
			 {
				  _snapshot.SnapshotProvider = value;
			 }
		 }

		 public override void RefreshSnapshot()
		 {
			  _snapshot.refreshSnapshot();
		 }

		 public override void AddBindingListener( BindingListener bindingListener )
		 {
			  _protocolServer.addBindingListener( bindingListener );
		 }

		 public override void RemoveBindingListener( BindingListener listener )
		 {
			  _protocolServer.removeBindingListener( listener );
		 }

		 public virtual void DumpDiagnostics( StringBuilder appendTo )
		 {
			  StateMachines stateMachines = _protocolServer.StateMachines;
			  foreach ( StateMachine stateMachine in stateMachines.GetStateMachines() )
			  {
					appendTo.Append( "   " ).Append( stateMachine.MessageType.SimpleName ).Append( ":" ).Append( stateMachine.State.ToString() ).Append("\n");
			  }

			  appendTo.Append( "Current timeouts:\n" );
			  foreach ( KeyValuePair<object, Timeouts.Timeout> objectTimeoutEntry in stateMachines.Timeouts.Timeouts.SetOfKeyValuePairs() )
			  {
					appendTo.Append( objectTimeoutEntry.Key.ToString() ).Append(":").Append(objectTimeoutEntry.Value.TimeoutMessage.ToString());
			  }
		 }

		 public virtual InstanceId ServerId
		 {
			 get
			 {
				  return _protocolServer.ServerId;
			 }
		 }

		 public virtual URI ClusterServer
		 {
			 get
			 {
				  return _protocolServer.boundAt();
			 }
		 }

		 public virtual void Stop()
		 {
			  _life.stop();
		 }
	}

}