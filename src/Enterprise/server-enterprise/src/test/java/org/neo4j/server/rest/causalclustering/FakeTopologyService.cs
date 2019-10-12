using System.Collections.Generic;

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
namespace Neo4Net.Server.rest.causalclustering
{

	using ClientConnectorAddresses = Neo4Net.causalclustering.discovery.ClientConnectorAddresses;
	using CoreServerInfo = Neo4Net.causalclustering.discovery.CoreServerInfo;
	using CoreTopology = Neo4Net.causalclustering.discovery.CoreTopology;
	using ReadReplicaInfo = Neo4Net.causalclustering.discovery.ReadReplicaInfo;
	using ReadReplicaTopology = Neo4Net.causalclustering.discovery.ReadReplicaTopology;
	using RoleInfo = Neo4Net.causalclustering.discovery.RoleInfo;
	using TopologyService = Neo4Net.causalclustering.discovery.TopologyService;
	using ClusterId = Neo4Net.causalclustering.identity.ClusterId;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;

	internal class FakeTopologyService : TopologyService
	{
		 private readonly ClusterId _clusterId;
		 private readonly IDictionary<MemberId, CoreServerInfo> _coreMembers;
		 private readonly IDictionary<MemberId, RoleInfo> _roles;
		 private readonly IDictionary<MemberId, ReadReplicaInfo> _replicaMembers;
		 private readonly string _dbName = "dbName";
		 private readonly MemberId _myself;

		 internal FakeTopologyService( ICollection<MemberId> cores, ICollection<MemberId> replicas, MemberId myself, RoleInfo myselfRole )
		 {
			  this._myself = myself;
			  _clusterId = new ClusterId( System.Guid.randomUUID() );
			  _roles = new Dictionary<MemberId, RoleInfo>();
			  _coreMembers = new Dictionary<MemberId, CoreServerInfo>();

			  foreach ( MemberId coreMemberId in cores )
			  {
					CoreServerInfo coreServerInfo = coreServerInfo( _dbName );
					_coreMembers[coreMemberId] = coreServerInfo;
					_roles[coreMemberId] = RoleInfo.FOLLOWER;
			  }

			  _replicaMembers = new Dictionary<MemberId, ReadReplicaInfo>();
			  foreach ( MemberId replicaMemberId in replicas )
			  {
					ReadReplicaInfo readReplicaInfo = readReplicaInfo( _dbName );
					_replicaMembers[replicaMemberId] = readReplicaInfo;
					_roles[replicaMemberId] = RoleInfo.READ_REPLICA;
			  }

			  if ( RoleInfo.READ_REPLICA.Equals( myselfRole ) )
			  {
					_replicaMembers[myself] = ReadReplicaInfo( _dbName );
					_roles[myself] = RoleInfo.READ_REPLICA;
			  }
			  else
			  {
					_coreMembers[myself] = CoreServerInfo( _dbName );
					_roles[myself] = RoleInfo.FOLLOWER;
			  }
			  _roles[myself] = myselfRole;
		 }

		 private CoreServerInfo CoreServerInfo( string dbName )
		 {
			  AdvertisedSocketAddress raftServer = new AdvertisedSocketAddress( "hostname", 1234 );
			  AdvertisedSocketAddress catchupServer = new AdvertisedSocketAddress( "hostname", 1234 );
			  ClientConnectorAddresses clientConnectors = new ClientConnectorAddresses( Collections.emptyList() );
			  bool refuseToBeLeader = false;
			  return new CoreServerInfo( raftServer, catchupServer, clientConnectors, dbName, refuseToBeLeader );
		 }

		 private ReadReplicaInfo ReadReplicaInfo( string dbName )
		 {
			  ClientConnectorAddresses clientConnectorAddresses = new ClientConnectorAddresses( Collections.emptyList() );
			  AdvertisedSocketAddress catchupServerAddress = new AdvertisedSocketAddress( "hostname", 1234 );
			  return new ReadReplicaInfo( clientConnectorAddresses, catchupServerAddress, dbName );
		 }

		 public override string LocalDBName()
		 {
			  return _dbName;
		 }

		 public override CoreTopology AllCoreServers()
		 {
			  return new CoreTopology( _clusterId, true, _coreMembers );
		 }

		 public override CoreTopology LocalCoreServers()
		 {
			  return new CoreTopology( _clusterId, true, _coreMembers );
		 }

		 public override ReadReplicaTopology AllReadReplicas()
		 {
			  return new ReadReplicaTopology( _replicaMembers );
		 }

		 public override ReadReplicaTopology LocalReadReplicas()
		 {
			  return new ReadReplicaTopology( _replicaMembers );
		 }

		 public override Optional<AdvertisedSocketAddress> FindCatchupAddress( MemberId upstream )
		 {
			  return null;
		 }

		 public override IDictionary<MemberId, RoleInfo> AllCoreRoles()
		 {
			  IDictionary<MemberId, RoleInfo> roles = new Dictionary<MemberId, RoleInfo>();
			  foreach ( MemberId memberId in _coreMembers.Keys )
			  {
					roles[memberId] = this._roles[memberId];
			  }
			  return roles;
		 }

		 public override MemberId Myself()
		 {
			  return _myself;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void init() throws Throwable
		 public override void Init()
		 {

		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
		 public override void Start()
		 {

		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws Throwable
		 public override void Stop()
		 {

		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown() throws Throwable
		 public override void Shutdown()
		 {

		 }

		 public virtual void ReplaceWithRole( MemberId memberId, RoleInfo role )
		 {
			  IList<MemberId> membersWithRole = _roles.Keys.Where( member => _roles[member].Equals( role ) ).ToList();
			  if ( membersWithRole.Count == 1 )
			  {
					_roles[membersWithRole[0]] = RoleInfo.FOLLOWER;
			  }
			  if ( memberId != null )
			  {
					_roles[memberId] = role;
			  }
		 }
	}

}