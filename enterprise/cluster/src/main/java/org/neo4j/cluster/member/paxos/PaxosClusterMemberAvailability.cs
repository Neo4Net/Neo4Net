using System;

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
namespace Org.Neo4j.cluster.member.paxos
{

	using BindingNotifier = Org.Neo4j.cluster.com.BindingNotifier;
	using AtomicBroadcast = Org.Neo4j.cluster.protocol.atomicbroadcast.AtomicBroadcast;
	using AtomicBroadcastSerializer = Org.Neo4j.cluster.protocol.atomicbroadcast.AtomicBroadcastSerializer;
	using ObjectInputStreamFactory = Org.Neo4j.cluster.protocol.atomicbroadcast.ObjectInputStreamFactory;
	using ObjectOutputStreamFactory = Org.Neo4j.cluster.protocol.atomicbroadcast.ObjectOutputStreamFactory;
	using Payload = Org.Neo4j.cluster.protocol.atomicbroadcast.Payload;
	using Lifecycle = Org.Neo4j.Kernel.Lifecycle.Lifecycle;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using StoreId = Org.Neo4j.Storageengine.Api.StoreId;

	/// <summary>
	/// Paxos based implementation of <seealso cref="org.neo4j.cluster.member.ClusterMemberAvailability"/>
	/// </summary>
	public class PaxosClusterMemberAvailability : ClusterMemberAvailability, Lifecycle
	{
		 private volatile URI _serverClusterId;
		 private Log _log;
		 protected internal AtomicBroadcastSerializer Serializer;
		 private readonly InstanceId _myId;
		 private BindingNotifier _binding;
		 private AtomicBroadcast _atomicBroadcast;
		 private BindingListener _bindingListener;
		 private ObjectInputStreamFactory _objectInputStreamFactory;
		 private ObjectOutputStreamFactory _objectOutputStreamFactory;

		 public PaxosClusterMemberAvailability( InstanceId myId, BindingNotifier binding, AtomicBroadcast atomicBroadcast, LogProvider logProvider, ObjectInputStreamFactory objectInputStreamFactory, ObjectOutputStreamFactory objectOutputStreamFactory )
		 {
			  this._myId = myId;
			  this._binding = binding;
			  this._atomicBroadcast = atomicBroadcast;
			  this._objectInputStreamFactory = objectInputStreamFactory;
			  this._objectOutputStreamFactory = objectOutputStreamFactory;
			  this._log = logProvider.getLog( this.GetType() );

			  _bindingListener = me =>
			  {
				_serverClusterId = me;
				PaxosClusterMemberAvailability.this._log.info( "Listening at:" + me );
			  };
		 }

		 public override void Init()
		 {
			  Serializer = new AtomicBroadcastSerializer( _objectInputStreamFactory, _objectOutputStreamFactory );

			  _binding.addBindingListener( _bindingListener );
		 }

		 public override void Start()
		 {
		 }

		 public override void Stop()
		 {
		 }

		 public override void Shutdown()
		 {
			  _binding.removeBindingListener( _bindingListener );
		 }

		 public override void MemberIsAvailable( string role, URI roleUri, StoreId storeId )
		 {
			  try
			  {
					MemberIsAvailable message = new MemberIsAvailable( role, _myId, _serverClusterId, roleUri, storeId );
					Payload payload = Serializer.broadcast( message );
					_atomicBroadcast.broadcast( payload );
			  }
			  catch ( Exception e )
			  {
					_log.warn( "Could not distribute member availability", e );
			  }
		 }

		 public override void MemberIsUnavailable( string role )
		 {
			  try
			  {
					MemberIsUnavailable message = new MemberIsUnavailable( role, _myId, _serverClusterId );
					Payload payload = Serializer.broadcast( message );
					_atomicBroadcast.broadcast( payload );
			  }
			  catch ( Exception e )
			  {
					_log.warn( "Could not distribute member unavailability", e );
			  }
		 }
	}

}