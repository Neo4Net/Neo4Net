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
	using HazelcastClient = com.hazelcast.client.HazelcastClient;
	using ClientConfig = com.hazelcast.client.config.ClientConfig;
	using ClientNetworkConfig = com.hazelcast.client.config.ClientNetworkConfig;
	using HazelcastInstance = com.hazelcast.core.HazelcastInstance;

	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LogProvider = Neo4Net.Logging.LogProvider;

	public class HazelcastClientConnector : HazelcastConnector
	{
		 private readonly Config _config;
		 private readonly LogProvider _logProvider;
		 private readonly RemoteMembersResolver _remoteMembersResolver;

		 public HazelcastClientConnector( Config config, LogProvider logProvider, RemoteMembersResolver remoteMembersResolver )
		 {
			  this._config = config;
			  this._logProvider = logProvider;
			  this._remoteMembersResolver = remoteMembersResolver;
		 }

		 public override HazelcastInstance ConnectToHazelcast()
		 {
			  ClientConfig clientConfig = new ClientConfig();

			  ClientNetworkConfig networkConfig = clientConfig.NetworkConfig;

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  _remoteMembersResolver.resolve( AdvertisedSocketAddress::toString ).forEach( networkConfig.addAddress );

			  AdditionalConfig( networkConfig, _logProvider );

			  return HazelcastClient.newHazelcastClient( clientConfig );
		 }

		 protected internal virtual void AdditionalConfig( ClientNetworkConfig networkConfig, LogProvider logProvider )
		 {

		 }
	}

}