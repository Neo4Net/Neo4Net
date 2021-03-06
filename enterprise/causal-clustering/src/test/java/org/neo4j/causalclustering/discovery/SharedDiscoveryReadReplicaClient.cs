﻿using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.discovery
{

	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using SafeLifecycle = Org.Neo4j.Kernel.Lifecycle.SafeLifecycle;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.SocketAddressParser.socketAddress;

	internal class SharedDiscoveryReadReplicaClient : SafeLifecycle, TopologyService
	{
		 private readonly SharedDiscoveryService _sharedDiscoveryService;
		 private readonly ReadReplicaInfo _addresses;
		 private readonly MemberId _memberId;
		 private readonly Log _log;
		 private readonly string _dbName;

		 internal SharedDiscoveryReadReplicaClient( SharedDiscoveryService sharedDiscoveryService, Config config, MemberId memberId, LogProvider logProvider )
		 {
			  this._sharedDiscoveryService = sharedDiscoveryService;
			  this._dbName = config.Get( CausalClusteringSettings.database );
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  this._addresses = new ReadReplicaInfo( ClientConnectorAddresses.ExtractFromConfig( config ), socketAddress( config.Get( CausalClusteringSettings.transaction_advertised_address ).ToString(), AdvertisedSocketAddress::new ), _dbName );
			  this._memberId = memberId;
			  this._log = logProvider.getLog( this.GetType() );
		 }

		 public override void Init0()
		 {
			  // nothing to do
		 }

		 public override void Start0()
		 {
			  _sharedDiscoveryService.registerReadReplica( this );
			  _log.info( "Registered read replica member id: %s at %s", _memberId, _addresses );
		 }

		 public override void Stop0()
		 {
			  _sharedDiscoveryService.unRegisterReadReplica( this );
		 }

		 public override void Shutdown0()
		 {
			  // nothing to do
		 }

		 public override CoreTopology AllCoreServers()
		 {
			  return _sharedDiscoveryService.getCoreTopology( _dbName, false );
		 }

		 public override CoreTopology LocalCoreServers()
		 {
			  return AllCoreServers().filterTopologyByDb(_dbName);
		 }

		 public override ReadReplicaTopology AllReadReplicas()
		 {
			  return _sharedDiscoveryService.ReadReplicaTopology;
		 }

		 public override ReadReplicaTopology LocalReadReplicas()
		 {
			  return AllReadReplicas().filterTopologyByDb(_dbName);
		 }

		 public override Optional<AdvertisedSocketAddress> FindCatchupAddress( MemberId upstream )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return _sharedDiscoveryService.getCoreTopology( _dbName, false ).find( upstream ).map( info => info.CatchupServer ).orElseGet( () => _sharedDiscoveryService.ReadReplicaTopology.find(upstream).map(ReadReplicaInfo::getCatchupServer) );
		 }

		 public override string LocalDBName()
		 {
			  return _dbName;
		 }

		 public override IDictionary<MemberId, RoleInfo> AllCoreRoles()
		 {
			  return _sharedDiscoveryService.CoreRoles;
		 }

		 public override MemberId Myself()
		 {
			  return _memberId;
		 }

		 public virtual MemberId MemberId
		 {
			 get
			 {
				  return _memberId;
			 }
		 }

		 public virtual ReadReplicaInfo ReadReplicainfo
		 {
			 get
			 {
				  return _addresses;
			 }
		 }
	}

}