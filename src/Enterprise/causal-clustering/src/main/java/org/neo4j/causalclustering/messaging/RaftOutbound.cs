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
namespace Neo4Net.causalclustering.messaging
{

	using RaftMessages = Neo4Net.causalclustering.core.consensus.RaftMessages;
	using RaftMessages_RaftMessage = Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage;
	using CoreServerInfo = Neo4Net.causalclustering.discovery.CoreServerInfo;
	using CoreTopologyService = Neo4Net.causalclustering.discovery.CoreTopologyService;
	using ClusterId = Neo4Net.causalclustering.identity.ClusterId;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using UnknownAddressMonitor = Neo4Net.causalclustering.messaging.address.UnknownAddressMonitor;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using Clocks = Neo4Net.Time.Clocks;

	public class RaftOutbound : Outbound<MemberId, RaftMessages_RaftMessage>
	{
		 private readonly CoreTopologyService _coreTopologyService;
		 private readonly Outbound<AdvertisedSocketAddress, Message> _outbound;
		 private readonly System.Func<Optional<ClusterId>> _clusterIdentity;
		 private readonly UnknownAddressMonitor _unknownAddressMonitor;
		 private readonly Log _log;

		 public RaftOutbound( CoreTopologyService coreTopologyService, Outbound<AdvertisedSocketAddress, Message> outbound, System.Func<Optional<ClusterId>> clusterIdentity, LogProvider logProvider, long logThresholdMillis )
		 {
			  this._coreTopologyService = coreTopologyService;
			  this._outbound = outbound;
			  this._clusterIdentity = clusterIdentity;
			  this._log = logProvider.getLog( this.GetType() );
			  this._unknownAddressMonitor = new UnknownAddressMonitor( _log, Clocks.systemClock(), logThresholdMillis );
		 }

		 public override void Send( MemberId to, RaftMessages_RaftMessage message, bool block )
		 {
			  Optional<ClusterId> clusterId = _clusterIdentity.get();
			  if ( !clusterId.Present )
			  {
					_log.warn( "Attempting to send a message before bound to a cluster" );
					return;
			  }

			  Optional<CoreServerInfo> coreServerInfo = _coreTopologyService.localCoreServers().find(to);
			  if ( coreServerInfo.Present )
			  {
					_outbound.send( coreServerInfo.get().RaftServer, Neo4Net.causalclustering.core.consensus.RaftMessages_ClusterIdAwareMessage.of(clusterId.get(), message), block );
			  }
			  else
			  {
					_unknownAddressMonitor.logAttemptToSendToMemberWithNoKnownAddress( to );
			  }
		 }
	}

}