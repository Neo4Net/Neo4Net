using System;
using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.discovery
{

	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using LeaderInfo = Neo4Net.causalclustering.core.consensus.LeaderInfo;
	using ClusterId = Neo4Net.causalclustering.identity.ClusterId;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LogProvider = Neo4Net.Logging.LogProvider;

	internal class SharedDiscoveryCoreClient : AbstractCoreTopologyService, IComparable<SharedDiscoveryCoreClient>
	{
		 private readonly SharedDiscoveryService _sharedDiscoveryService;
		 private readonly CoreServerInfo _coreServerInfo;
		 private readonly string _localDBName;
		 private readonly bool _refusesToBeLeader;

		 private volatile LeaderInfo _leaderInfo = LeaderInfo.INITIAL;
		 private volatile ReadReplicaTopology _readReplicaTopology = ReadReplicaTopology.Empty;
		 private volatile CoreTopology _coreTopology = CoreTopology.Empty;
		 private volatile ReadReplicaTopology _localReadReplicaTopology = ReadReplicaTopology.Empty;
		 private volatile CoreTopology _localCoreTopology = CoreTopology.Empty;

		 internal SharedDiscoveryCoreClient( SharedDiscoveryService sharedDiscoveryService, MemberId member, LogProvider logProvider, Config config ) : base( config, member, logProvider, logProvider )
		 {
			  this._localDBName = config.Get( CausalClusteringSettings.database );
			  this._sharedDiscoveryService = sharedDiscoveryService;
			  this._coreServerInfo = CoreServerInfo.From( config );
			  this._refusesToBeLeader = config.Get( CausalClusteringSettings.refuse_to_be_leader );
		 }

		 public override int CompareTo( SharedDiscoveryCoreClient o )
		 {
			  return Optional.ofNullable( o ).map( c => c.myself.Uuid.compareTo( this.MyselfConflict.Uuid ) ).orElse( -1 );
		 }

		 public override bool SetClusterId( ClusterId clusterId, string dbName )
		 {
			  return _sharedDiscoveryService.casClusterId( clusterId, dbName );
		 }

		 public override IDictionary<MemberId, RoleInfo> AllCoreRoles()
		 {
			  return _sharedDiscoveryService.CoreRoles;
		 }

		 public override LeaderInfo Leader0
		 {
			 set
			 {
				  _leaderInfo = value;
				  _sharedDiscoveryService.casLeaders( value, _localDBName );
			 }
		 }

		 public override LeaderInfo Leader
		 {
			 get
			 {
				  return _leaderInfo;
			 }
		 }

		 public override void Init0()
		 {
			  // nothing to do
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start0() throws InterruptedException
		 public override void Start0()
		 {
			  _coreTopology = _sharedDiscoveryService.getCoreTopology( this );
			  _localCoreTopology = _coreTopology.filterTopologyByDb( _localDBName );
			  _readReplicaTopology = _sharedDiscoveryService.ReadReplicaTopology;
			  _localReadReplicaTopology = _readReplicaTopology.filterTopologyByDb( _localDBName );

			  _sharedDiscoveryService.registerCoreMember( this );
			  Log.info( "Registered core server %s", MyselfConflict );

			  _sharedDiscoveryService.waitForClusterFormation();
			  Log.info( "Cluster formed" );
		 }

		 public override void Stop0()
		 {
			  _sharedDiscoveryService.unRegisterCoreMember( this );
			  Log.info( "Unregistered core server %s", MyselfConflict );
		 }

		 public override void Shutdown0()
		 {
			  // nothing to do
		 }

		 public override string LocalDBName()
		 {
			  return _localDBName;
		 }

		 public override CoreTopology AllCoreServers()
		 {
			  return _coreTopology;
		 }

		 public override CoreTopology LocalCoreServers()
		 {
			  return _localCoreTopology;
		 }

		 public override ReadReplicaTopology AllReadReplicas()
		 {
			  return _readReplicaTopology;
		 }

		 public override ReadReplicaTopology LocalReadReplicas()
		 {
			  return _localReadReplicaTopology;
		 }

		 public override Optional<AdvertisedSocketAddress> FindCatchupAddress( MemberId upstream )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return LocalCoreServers().find(upstream).map(info => info.CatchupServer).orElseGet(() => _readReplicaTopology.find(upstream).map(ReadReplicaInfo::getCatchupServer));
		 }

		 public override void HandleStepDown0( LeaderInfo steppingDown )
		 {
			  _sharedDiscoveryService.casLeaders( steppingDown, _localDBName );
		 }

		 public virtual MemberId MemberId
		 {
			 get
			 {
				  return MyselfConflict;
			 }
		 }

		 public virtual CoreServerInfo CoreServerInfo
		 {
			 get
			 {
				  return _coreServerInfo;
			 }
		 }

		 internal virtual void OnCoreTopologyChange( CoreTopology coreTopology )
		 {
			  Log.info( "Notified of core topology change " + coreTopology );
			  this._coreTopology = coreTopology;
			  this._localCoreTopology = coreTopology.FilterTopologyByDb( _localDBName );
			  ListenerService.notifyListeners( coreTopology );
		 }

		 internal virtual void OnReadReplicaTopologyChange( ReadReplicaTopology readReplicaTopology )
		 {
			  Log.info( "Notified of read replica topology change " + readReplicaTopology );
			  this._readReplicaTopology = readReplicaTopology;
			  this._localReadReplicaTopology = readReplicaTopology.FilterTopologyByDb( _localDBName );
		 }

		 public virtual bool RefusesToBeLeader()
		 {
			  return _refusesToBeLeader;
		 }

		 public override string ToString()
		 {
			  return "SharedDiscoveryCoreClient{" + "myself=" + MyselfConflict + ", coreServerInfo=" + _coreServerInfo + ", refusesToBeLeader=" + _refusesToBeLeader +
						 ", localDBName='" + _localDBName + '\'' + ", leaderInfo=" + _leaderInfo + ", coreTopology=" + _coreTopology + '}';
		 }
	}

}