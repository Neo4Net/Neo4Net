using System;
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
namespace Neo4Net.cluster.member.paxos
{

	using AtomicBroadcast = Neo4Net.cluster.protocol.atomicbroadcast.AtomicBroadcast;
	using AtomicBroadcastListener = Neo4Net.cluster.protocol.atomicbroadcast.AtomicBroadcastListener;
	using AtomicBroadcastSerializer = Neo4Net.cluster.protocol.atomicbroadcast.AtomicBroadcastSerializer;
	using ObjectInputStreamFactory = Neo4Net.cluster.protocol.atomicbroadcast.ObjectInputStreamFactory;
	using ObjectOutputStreamFactory = Neo4Net.cluster.protocol.atomicbroadcast.ObjectOutputStreamFactory;
	using Payload = Neo4Net.cluster.protocol.atomicbroadcast.Payload;
	using Cluster = Neo4Net.cluster.protocol.cluster.Cluster;
	using ClusterConfiguration = Neo4Net.cluster.protocol.cluster.ClusterConfiguration;
	using ClusterListener = Neo4Net.cluster.protocol.cluster.ClusterListener;
	using Heartbeat = Neo4Net.cluster.protocol.heartbeat.Heartbeat;
	using HeartbeatListener = Neo4Net.cluster.protocol.heartbeat.HeartbeatListener;
	using Snapshot = Neo4Net.cluster.protocol.snapshot.Snapshot;
	using SnapshotProvider = Neo4Net.cluster.protocol.snapshot.SnapshotProvider;
	using Neo4Net.Helpers;
	using NamedThreadFactory = Neo4Net.Helpers.NamedThreadFactory;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.function.Predicates.@in;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterables.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterables.filter;

	/// <summary>
	/// Paxos based implementation of <seealso cref="org.Neo4Net.cluster.member.ClusterMemberEvents"/>
	/// </summary>
	public class PaxosClusterMemberEvents : ClusterMemberEvents, Lifecycle
	{
		 private Cluster _cluster;
		 private AtomicBroadcast _atomicBroadcast;
		 private Log _log;
		 protected internal AtomicBroadcastSerializer Serializer;
		 protected internal readonly Listeners<ClusterMemberListener> Listeners = new Listeners<ClusterMemberListener>();
		 private ClusterMembersSnapshot _clusterMembersSnapshot;
		 private Neo4Net.cluster.protocol.cluster.ClusterListener_Adapter _clusterListener;
		 private Snapshot _snapshot;
		 private AtomicBroadcastListener _atomicBroadcastListener;
		 private ExecutorService _executor;
		 private readonly System.Predicate<ClusterMembersSnapshot> _snapshotValidator;
		 private readonly Heartbeat _heartbeat;
		 private HeartbeatListenerImpl _heartbeatListener;
		 private ObjectInputStreamFactory _lenientObjectInputStream;
		 private ObjectOutputStreamFactory _lenientObjectOutputStream;
		 private readonly NamedThreadFactory.Monitor _namedThreadFactoryMonitor;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public PaxosClusterMemberEvents(final org.Neo4Net.cluster.protocol.snapshot.Snapshot snapshot, org.Neo4Net.cluster.protocol.cluster.Cluster cluster, org.Neo4Net.cluster.protocol.heartbeat.Heartbeat heartbeat, org.Neo4Net.cluster.protocol.atomicbroadcast.AtomicBroadcast atomicBroadcast, org.Neo4Net.logging.LogProvider logProvider, System.Predicate<ClusterMembersSnapshot> validator, System.Func<Iterable<MemberIsAvailable>, MemberIsAvailable, Iterable<MemberIsAvailable>> snapshotFilter, org.Neo4Net.cluster.protocol.atomicbroadcast.ObjectInputStreamFactory lenientObjectInputStream, org.Neo4Net.cluster.protocol.atomicbroadcast.ObjectOutputStreamFactory lenientObjectOutputStream, org.Neo4Net.helpers.NamedThreadFactory.Monitor namedThreadFactoryMonitor)
		 public PaxosClusterMemberEvents( Snapshot snapshot, Cluster cluster, Heartbeat heartbeat, AtomicBroadcast atomicBroadcast, LogProvider logProvider, System.Predicate<ClusterMembersSnapshot> validator, System.Func<IEnumerable<MemberIsAvailable>, MemberIsAvailable, IEnumerable<MemberIsAvailable>> snapshotFilter, ObjectInputStreamFactory lenientObjectInputStream, ObjectOutputStreamFactory lenientObjectOutputStream, NamedThreadFactory.Monitor namedThreadFactoryMonitor )
		 {
			  this._snapshot = snapshot;
			  this._cluster = cluster;
			  this._heartbeat = heartbeat;
			  this._atomicBroadcast = atomicBroadcast;
			  this._lenientObjectInputStream = lenientObjectInputStream;
			  this._lenientObjectOutputStream = lenientObjectOutputStream;
			  this._namedThreadFactoryMonitor = namedThreadFactoryMonitor;
			  this._log = logProvider.getLog( this.GetType() );

			  _clusterListener = new ClusterListenerImpl( this );

			  _atomicBroadcastListener = new AtomicBroadcastListenerImpl( this );

			  this._snapshotValidator = validator;

			  _clusterMembersSnapshot = new ClusterMembersSnapshot( snapshotFilter );
		 }

		 public override void AddClusterMemberListener( ClusterMemberListener listener )
		 {
			  Listeners.add( listener );
		 }

		 public override void RemoveClusterMemberListener( ClusterMemberListener listener )
		 {
			  Listeners.remove( listener );
		 }

		 public override void Init()
		 {
			  Serializer = new AtomicBroadcastSerializer( _lenientObjectInputStream, _lenientObjectOutputStream );

			  _cluster.addClusterListener( _clusterListener );

			  _atomicBroadcast.addAtomicBroadcastListener( _atomicBroadcastListener );

			  _snapshot.SnapshotProvider = new HighAvailabilitySnapshotProvider( this );

			  _heartbeat.addHeartbeatListener( _heartbeatListener = new HeartbeatListenerImpl( this ) );

			  _executor = Executors.newSingleThreadExecutor( new NamedThreadFactory( "Paxos event notification", _namedThreadFactoryMonitor ) );
		 }

		 public override void Start()
		 {
		 }

		 public override void Stop()
		 {
		 }

		 public override void Shutdown()
		 {
			  _snapshot.SnapshotProvider = null;

			  if ( _executor != null )
			  {
					_executor.shutdown();
					_executor = null;
			  }

			  _cluster.removeClusterListener( _clusterListener );

			  _atomicBroadcast.removeAtomicBroadcastListener( _atomicBroadcastListener );

			  _heartbeat.removeHeartbeatListener( _heartbeatListener );
		 }

		 private class HighAvailabilitySnapshotProvider : SnapshotProvider
		 {
			 private readonly PaxosClusterMemberEvents _outerInstance;

			 public HighAvailabilitySnapshotProvider( PaxosClusterMemberEvents outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void getState(java.io.ObjectOutputStream output) throws java.io.IOException
			  public override void GetState( ObjectOutputStream output )
			  {
					output.writeObject( outerInstance.clusterMembersSnapshot );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void setState(java.io.ObjectInputStream input) throws java.io.IOException, ClassNotFoundException
			  public virtual ObjectInputStream State
			  {
				  set
				  {
						outerInstance.clusterMembersSnapshot = typeof( ClusterMembersSnapshot ).cast( value.readObject() );
   
						if ( !outerInstance.snapshotValidator.test( outerInstance.clusterMembersSnapshot ) )
						{
							 outerInstance.executor.submit( () => outerInstance.cluster.leave() );
						}
						else
						{
							 // Send current availability events to listeners
							 outerInstance.Listeners.notify(outerInstance.executor, listener =>
							 {
							  foreach ( MemberIsAvailable memberIsAvailable in outerInstance.clusterMembersSnapshot.CurrentAvailableMembers )
							  {
									listener.memberIsAvailable( memberIsAvailable.Role, memberIsAvailable.InstanceId, memberIsAvailable.RoleUri, memberIsAvailable.StoreId );
							  }
							 });
						}
				  }
			  }
		 }

		 public class UniqueRoleFilter : System.Func<IEnumerable<MemberIsAvailable>, MemberIsAvailable, IEnumerable<MemberIsAvailable>>
		 {
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Iterable<MemberIsAvailable> apply(final Iterable<MemberIsAvailable> previousSnapshot, final MemberIsAvailable newMessage)
			  public override IEnumerable<MemberIsAvailable> Apply( IEnumerable<MemberIsAvailable> previousSnapshot, MemberIsAvailable newMessage )
			  {
					return Iterables.append( newMessage, Iterables.filter( item => @in( newMessage.InstanceId ).negate().test(item.InstanceId), previousSnapshot ) );
			  }
		 }

		 [Serializable]
		 public class ClusterMembersSnapshot
		 {
			  internal const long SERIAL_VERSION_UID = -4638991834604077187L;

			  internal System.Func<IEnumerable<MemberIsAvailable>, MemberIsAvailable, IEnumerable<MemberIsAvailable>> NextSnapshotFunction;

			  internal IEnumerable<MemberIsAvailable> AvailableMembers = new List<MemberIsAvailable>();

			  public ClusterMembersSnapshot( System.Func<IEnumerable<MemberIsAvailable>, MemberIsAvailable, IEnumerable<MemberIsAvailable>> nextSnapshotFunction )
			  {
					this.NextSnapshotFunction = nextSnapshotFunction;
			  }

			  public virtual void AvailableMember( MemberIsAvailable memberIsAvailable )
			  {
					AvailableMembers = asList( NextSnapshotFunction.apply( AvailableMembers, memberIsAvailable ) );
			  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void unavailableMember(final org.Neo4Net.cluster.InstanceId member)
			  public virtual void UnavailableMember( InstanceId member )
			  {
					AvailableMembers = asList( filter( item => !item.InstanceId.Equals( member ), AvailableMembers ) );
			  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void unavailableMember(final java.net.URI member, final org.Neo4Net.cluster.InstanceId id, final String role)
			  public virtual void UnavailableMember( URI member, InstanceId id, string role )
			  {
					AvailableMembers = asList(filter(item =>
					{
					 bool matchByUriOrId = item.ClusterUri.Equals( member ) || item.InstanceId.Equals( id );
					 bool matchByRole = item.Role.Equals( role );

					 return !( matchByUriOrId && matchByRole );
					}, AvailableMembers));
			  }

			  public virtual IEnumerable<MemberIsAvailable> CurrentAvailableMembers
			  {
				  get
				  {
						return AvailableMembers;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Iterable<MemberIsAvailable> getCurrentAvailable(final org.Neo4Net.cluster.InstanceId memberId)
			  public virtual IEnumerable<MemberIsAvailable> GetCurrentAvailable( InstanceId memberId )
			  {
					return asList( Iterables.filter( item => item.InstanceId.Equals( memberId ), AvailableMembers ) );
			  }

		 }

		 private class ClusterListenerImpl : Neo4Net.cluster.protocol.cluster.ClusterListener_Adapter
		 {
			 private readonly PaxosClusterMemberEvents _outerInstance;

			 public ClusterListenerImpl( PaxosClusterMemberEvents outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override void EnteredCluster( ClusterConfiguration clusterConfiguration )
			  {
					// Catch up with elections
					foreach ( KeyValuePair<string, InstanceId> memberRoles in clusterConfiguration.Roles.SetOfKeyValuePairs() )
					{
						 Elected( memberRoles.Key, memberRoles.Value, clusterConfiguration.GetUriForId( memberRoles.Value ) );
					}
			  }

			  public override void Elected( string role, InstanceId instanceId, URI electedMember )
			  {
					if ( role.Equals( ClusterConfiguration.COORDINATOR ) )
					{
						 // Use the cluster coordinator as master for HA
						 outerInstance.Listeners.notify( listener => listener.coordinatorIsElected( instanceId ) );
					}
			  }

			  public override void LeftCluster( InstanceId instanceId, URI member )
			  {
					// Notify unavailability of members
					outerInstance.Listeners.notify(listener =>
					{
					 foreach ( MemberIsAvailable memberIsAvailable in outerInstance.clusterMembersSnapshot.GetCurrentAvailable( instanceId ) )
					 {
						  listener.memberIsUnavailable( memberIsAvailable.Role, instanceId );
					 }
					});

					outerInstance.clusterMembersSnapshot.UnavailableMember( instanceId );
			  }
		 }

		 private class AtomicBroadcastListenerImpl : AtomicBroadcastListener
		 {
			 private readonly PaxosClusterMemberEvents _outerInstance;

			 public AtomicBroadcastListenerImpl( PaxosClusterMemberEvents outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override void Receive( Payload payload )
			  {
					try
					{
						 object value = outerInstance.Serializer.receive( payload );
						 if ( value is MemberIsAvailable )
						 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final MemberIsAvailable memberIsAvailable = (MemberIsAvailable) value;
							  MemberIsAvailable memberIsAvailable = ( MemberIsAvailable ) value;

							  // Update snapshot
							  outerInstance.clusterMembersSnapshot.AvailableMember( memberIsAvailable );

							  outerInstance.log.Info( "Snapshot:" + outerInstance.clusterMembersSnapshot.CurrentAvailableMembers );

							  outerInstance.Listeners.notify( listener => listener.memberIsAvailable( memberIsAvailable.Role, memberIsAvailable.InstanceId, memberIsAvailable.RoleUri, memberIsAvailable.StoreId ) );
						 }
						 else if ( value is MemberIsUnavailable )
						 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final MemberIsUnavailable memberIsUnavailable = (MemberIsUnavailable) value;
							  MemberIsUnavailable memberIsUnavailable = ( MemberIsUnavailable ) value;

							  // Update snapshot
							  outerInstance.clusterMembersSnapshot.UnavailableMember( memberIsUnavailable.ClusterUri, memberIsUnavailable.InstanceId, memberIsUnavailable.Role );

							  outerInstance.Listeners.notify( listener => listener.memberIsUnavailable( memberIsUnavailable.Role, memberIsUnavailable.InstanceId ) );
						 }
					}
					catch ( Exception t )
					{
						 outerInstance.log.Error( string.Format( "Could not handle cluster member available message: {0} ({1:D})", Base64.Encoder.encodeToString( payload.Buf ), payload.Len ), t );
					}
			  }
		 }

		 private class HeartbeatListenerImpl : HeartbeatListener
		 {
			 private readonly PaxosClusterMemberEvents _outerInstance;

			 public HeartbeatListenerImpl( PaxosClusterMemberEvents outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override void Failed( InstanceId server )
			  {
					outerInstance.Listeners.notify( listener => listener.memberIsFailed( server ) );
			  }

			  public override void Alive( InstanceId server )
			  {
					outerInstance.Listeners.notify( listener => listener.memberIsAlive( server ) );
			  }
		 }
	}

}