using System;
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
namespace Org.Neo4j.causalclustering.upstream.strategies
{

	using ClientConnectorAddresses = Org.Neo4j.causalclustering.discovery.ClientConnectorAddresses;
	using CoreServerInfo = Org.Neo4j.causalclustering.discovery.CoreServerInfo;
	using CoreTopology = Org.Neo4j.causalclustering.discovery.CoreTopology;
	using ReadReplicaInfo = Org.Neo4j.causalclustering.discovery.ReadReplicaInfo;
	using ReadReplicaTopology = Org.Neo4j.causalclustering.discovery.ReadReplicaTopology;
	using RoleInfo = Org.Neo4j.causalclustering.discovery.RoleInfo;
	using TopologyService = Org.Neo4j.causalclustering.discovery.TopologyService;
	using ClusterId = Org.Neo4j.causalclustering.identity.ClusterId;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;

	internal class TopologyServiceThatPrioritisesItself : TopologyService
	{
		 private readonly MemberId _myself;
		 private readonly string _matchingGroupName;

		 internal MemberId CoreNotSelf = new MemberId( new System.Guid( 321, 654 ) );
		 internal MemberId ReadReplicaNotSelf = new MemberId( new System.Guid( 432, 543 ) );

		 internal TopologyServiceThatPrioritisesItself( MemberId myself, string matchingGroupName )
		 {
			  this._myself = myself;
			  this._matchingGroupName = matchingGroupName;
		 }

		 public override string LocalDBName()
		 {
			  throw new Exception( "Unimplemented" );
		 }

		 public override CoreTopology AllCoreServers()
		 {
			  bool canBeBootstrapped = true;
			  IDictionary<MemberId, CoreServerInfo> coreMembers = new Dictionary<MemberId, CoreServerInfo>();
			  coreMembers[_myself] = CoreServerInfo();
			  coreMembers[CoreNotSelf] = CoreServerInfo();
			  return new CoreTopology( new ClusterId( new System.Guid( 99, 88 ) ), canBeBootstrapped, coreMembers );
		 }

		 public override CoreTopology LocalCoreServers()
		 {
			  return AllCoreServers();
		 }

		 public override ReadReplicaTopology AllReadReplicas()
		 {
			  IDictionary<MemberId, ReadReplicaInfo> readReplicaMembers = new Dictionary<MemberId, ReadReplicaInfo>();
			  readReplicaMembers[_myself] = ReadReplicaInfo( _matchingGroupName );
			  readReplicaMembers[ReadReplicaNotSelf] = ReadReplicaInfo( _matchingGroupName );
			  return new ReadReplicaTopology( readReplicaMembers );
		 }

		 public override ReadReplicaTopology LocalReadReplicas()
		 {
			  return AllReadReplicas();
		 }

		 public override Optional<AdvertisedSocketAddress> FindCatchupAddress( MemberId upstream )
		 {
			  throw new Exception( "Unimplemented" );
		 }

		 public override IDictionary<MemberId, RoleInfo> AllCoreRoles()
		 {
			  throw new Exception( "Unimplemented" );
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

		 private static CoreServerInfo CoreServerInfo( params string[] groupNames )
		 {
			  AdvertisedSocketAddress anyRaftAddress = new AdvertisedSocketAddress( "hostname", 1234 );
			  AdvertisedSocketAddress anyCatchupServer = new AdvertisedSocketAddress( "hostname", 5678 );
			  ClientConnectorAddresses clientConnectorAddress = new ClientConnectorAddresses( Collections.emptyList() );
			  ISet<string> groups = new HashSet<string>( Arrays.asList( groupNames ) );
			  return new CoreServerInfo( anyRaftAddress, anyCatchupServer, clientConnectorAddress, groups, "dbName", false );
		 }

		 private static ReadReplicaInfo ReadReplicaInfo( params string[] groupNames )
		 {
			  ClientConnectorAddresses clientConnectorAddresses = new ClientConnectorAddresses( Collections.emptyList() );
			  AdvertisedSocketAddress catchupServerAddress = new AdvertisedSocketAddress( "hostname", 2468 );
			  ISet<string> groups = new HashSet<string>( Arrays.asList( groupNames ) );
			  ReadReplicaInfo readReplicaInfo = new ReadReplicaInfo( clientConnectorAddresses, catchupServerAddress, groups, "dbName" );
			  return readReplicaInfo;
		 }
	}

}