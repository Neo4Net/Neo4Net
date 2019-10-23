using System;

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

	using BindingNotifier = Neo4Net.cluster.com.BindingNotifier;
	using AtomicBroadcast = Neo4Net.cluster.protocol.atomicbroadcast.AtomicBroadcast;
	using AtomicBroadcastSerializer = Neo4Net.cluster.protocol.atomicbroadcast.AtomicBroadcastSerializer;
	using ObjectInputStreamFactory = Neo4Net.cluster.protocol.atomicbroadcast.ObjectInputStreamFactory;
	using ObjectOutputStreamFactory = Neo4Net.cluster.protocol.atomicbroadcast.ObjectOutputStreamFactory;
	using Payload = Neo4Net.cluster.protocol.atomicbroadcast.Payload;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using StoreId = Neo4Net.Kernel.Api.StorageEngine.StoreId;

	/// <summary>
	/// Paxos based implementation of <seealso cref="org.Neo4Net.cluster.member.ClusterMemberAvailability"/>
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