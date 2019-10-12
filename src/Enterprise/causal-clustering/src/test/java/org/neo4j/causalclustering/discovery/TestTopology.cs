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
namespace Neo4Net.causalclustering.discovery
{

	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using Config = Neo4Net.Kernel.configuration.Config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.ClientConnectorAddresses.Scheme.bolt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;

	public class TestTopology
	{
		 private TestTopology()
		 {
		 }

		 private static ClientConnectorAddresses WrapAsClientConnectorAddresses( AdvertisedSocketAddress advertisedSocketAddress )
		 {
			  return new ClientConnectorAddresses( singletonList( new ClientConnectorAddresses.ConnectorUri( bolt, advertisedSocketAddress ) ) );
		 }

		 public static CoreServerInfo AddressesForCore( int id, bool refuseToBeLeader )
		 {
			  AdvertisedSocketAddress raftServerAddress = new AdvertisedSocketAddress( "localhost", 3000 + id );
			  AdvertisedSocketAddress catchupServerAddress = new AdvertisedSocketAddress( "localhost", 4000 + id );
			  AdvertisedSocketAddress boltServerAddress = new AdvertisedSocketAddress( "localhost", 5000 + id );
			  return new CoreServerInfo( raftServerAddress, catchupServerAddress, WrapAsClientConnectorAddresses( boltServerAddress ), asSet( "core", "core" + id ), "default", refuseToBeLeader );
		 }

		 public static Config ConfigFor( CoreServerInfo coreServerInfo )
		 {
			  return Config.builder().withSetting(CausalClusteringSettings.raft_advertised_address, coreServerInfo.RaftServer.ToString()).withSetting(CausalClusteringSettings.transaction_advertised_address, coreServerInfo.CatchupServer.ToString()).withSetting("dbms.connector.bolt.listen_address", coreServerInfo.Connectors().boltAddress().ToString()).withSetting("dbms.connector.bolt.enabled", true.ToString()).withSetting(CausalClusteringSettings.database, coreServerInfo.DatabaseName).withSetting(CausalClusteringSettings.server_groups, string.join(",", coreServerInfo.Groups())).withSetting(CausalClusteringSettings.refuse_to_be_leader, coreServerInfo.RefusesToBeLeader().ToString()).build();
		 }

		 public static Config ConfigFor( ReadReplicaInfo readReplicaInfo )
		 {
			  return Config.builder().withSetting("dbms.connector.bolt.listen_address", readReplicaInfo.Connectors().boltAddress().ToString()).withSetting("dbms.connector.bolt.enabled", true.ToString()).withSetting(CausalClusteringSettings.transaction_advertised_address, readReplicaInfo.CatchupServer.ToString()).withSetting(CausalClusteringSettings.server_groups, string.join(",", readReplicaInfo.Groups())).withSetting(CausalClusteringSettings.database, readReplicaInfo.DatabaseName).build();
		 }

		 public static ReadReplicaInfo AddressesForReadReplica( int id )
		 {
			  AdvertisedSocketAddress clientConnectorSocketAddress = new AdvertisedSocketAddress( "localhost", 6000 + id );
			  ClientConnectorAddresses clientConnectorAddresses = new ClientConnectorAddresses( singletonList( new ClientConnectorAddresses.ConnectorUri( bolt, clientConnectorSocketAddress ) ) );
			  AdvertisedSocketAddress catchupSocketAddress = new AdvertisedSocketAddress( "localhost", 4000 + id );

			  return new ReadReplicaInfo( clientConnectorAddresses, catchupSocketAddress, asSet( "replica", "replica" + id ), "default" );
		 }

		 public static IDictionary<MemberId, ReadReplicaInfo> ReadReplicaInfoMap( params int[] ids )
		 {
			  return Arrays.stream( ids ).mapToObj( TestTopology.addressesForReadReplica ).collect( Collectors.toMap( p => new MemberId( System.Guid.randomUUID() ), System.Func.identity() ) );
		 }
	}

}