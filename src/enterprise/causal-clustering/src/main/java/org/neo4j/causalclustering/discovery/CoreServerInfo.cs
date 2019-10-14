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
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using Config = Neo4Net.Kernel.configuration.Config;

	public class CoreServerInfo : DiscoveryServerInfo
	{
		 private readonly AdvertisedSocketAddress _raftServer;
		 private readonly AdvertisedSocketAddress _catchupServer;
		 private readonly ClientConnectorAddresses _clientConnectorAddresses;
		 private readonly ISet<string> _groups;
		 private readonly string _dbName;
		 private readonly bool _refuseToBeLeader;

		 public CoreServerInfo( AdvertisedSocketAddress raftServer, AdvertisedSocketAddress catchupServer, ClientConnectorAddresses clientConnectors, string dbName, bool refuseToBeLeader ) : this( raftServer, catchupServer, clientConnectors, emptySet(), dbName, refuseToBeLeader )
		 {
		 }

		 public CoreServerInfo( AdvertisedSocketAddress raftServer, AdvertisedSocketAddress catchupServer, ClientConnectorAddresses clientConnectorAddresses, ISet<string> groups, string dbName, bool refuseToBeLeader )
		 {
			  this._raftServer = raftServer;
			  this._catchupServer = catchupServer;
			  this._clientConnectorAddresses = clientConnectorAddresses;
			  this._groups = groups;
			  this._dbName = dbName;
			  this._refuseToBeLeader = refuseToBeLeader;
		 }

		 public virtual string DatabaseName
		 {
			 get
			 {
				  return _dbName;
			 }
		 }

		 public virtual AdvertisedSocketAddress RaftServer
		 {
			 get
			 {
				  return _raftServer;
			 }
		 }

		 public virtual AdvertisedSocketAddress CatchupServer
		 {
			 get
			 {
				  return _catchupServer;
			 }
		 }

		 public override ClientConnectorAddresses Connectors()
		 {
			  return _clientConnectorAddresses;
		 }

		 public override ISet<string> Groups()
		 {
			  return _groups;
		 }

		 public virtual bool RefusesToBeLeader()
		 {
			  return _refuseToBeLeader;
		 }

		 public override string ToString()
		 {
			  return "CoreServerInfo{" +
						"raftServer=" + _raftServer +
						", catchupServer=" + _catchupServer +
						", clientConnectorAddresses=" + _clientConnectorAddresses +
						", groups=" + _groups +
						", database=" + _dbName +
						", refuseToBeLeader=" + _refuseToBeLeader +
						'}';
		 }

		 public static CoreServerInfo From( Config config )
		 {
			  AdvertisedSocketAddress raftAddress = config.Get( CausalClusteringSettings.raft_advertised_address );
			  AdvertisedSocketAddress transactionSource = config.Get( CausalClusteringSettings.transaction_advertised_address );
			  ClientConnectorAddresses clientConnectorAddresses = ClientConnectorAddresses.ExtractFromConfig( config );
			  string dbName = config.Get( CausalClusteringSettings.database );
			  IList<string> groupList = config.Get( CausalClusteringSettings.server_groups );
			  ISet<string> groups = new HashSet<string>( groupList );
			  bool refuseToBeLeader = config.Get( CausalClusteringSettings.refuse_to_be_leader );

			  return new CoreServerInfo( raftAddress, transactionSource, clientConnectorAddresses, groups, dbName, refuseToBeLeader );
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }
			  CoreServerInfo that = ( CoreServerInfo ) o;
			  return _refuseToBeLeader == that._refuseToBeLeader && Objects.Equals( _raftServer, that._raftServer ) && Objects.Equals( _catchupServer, that._catchupServer ) && Objects.Equals( _clientConnectorAddresses, that._clientConnectorAddresses ) && Objects.Equals( _groups, that._groups ) && Objects.Equals( _dbName, that._dbName );
		 }

		 public override int GetHashCode()
		 {

			  return Objects.hash( _raftServer, _catchupServer, _clientConnectorAddresses, _groups, _dbName, _refuseToBeLeader );
		 }
	}

}