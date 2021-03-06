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
namespace Org.Neo4j.causalclustering.upstream.strategies
{

	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using DiscoveryServerInfo = Org.Neo4j.causalclustering.discovery.DiscoveryServerInfo;
	using Org.Neo4j.causalclustering.discovery;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using Org.Neo4j.causalclustering.routing.load_balancing.filters;
	using FilterConfigParser = Org.Neo4j.causalclustering.routing.load_balancing.plugins.server_policies.FilterConfigParser;
	using InvalidFilterSpecification = Org.Neo4j.causalclustering.routing.load_balancing.plugins.server_policies.InvalidFilterSpecification;
	using ServerInfo = Org.Neo4j.causalclustering.routing.load_balancing.plugins.server_policies.ServerInfo;
	using Service = Org.Neo4j.Helpers.Service;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(UpstreamDatabaseSelectionStrategy.class) public class UserDefinedConfigurationStrategy extends org.neo4j.causalclustering.upstream.UpstreamDatabaseSelectionStrategy
	public class UserDefinedConfigurationStrategy : UpstreamDatabaseSelectionStrategy
	{

		 public const string IDENTITY = "user-defined";
		 // Empty if provided filter config cannot be parsed.
		 // Ideally this class would not be created until config has been successfully parsed
		 // in which case there would be no need for Optional
		 private Optional<Filter<ServerInfo>> _filters;

		 public UserDefinedConfigurationStrategy() : base(IDENTITY)
		 {
		 }

		 public override void Init()
		 {
			  string filterConfig = Config.get( CausalClusteringSettings.user_defined_upstream_selection_strategy );
			  try
			  {
					Filter<ServerInfo> parsed = FilterConfigParser.parse( filterConfig );
					_filters = parsed;
					Log.info( "Upstream selection strategy " + ReadableName + " configured with " + filterConfig );
			  }
			  catch ( InvalidFilterSpecification invalidFilterSpecification )
			  {
					_filters = null;
					Log.warn( "Cannot parse configuration '" + filterConfig + "' for upstream selection strategy " + ReadableName + ". " + invalidFilterSpecification.Message );
			  }
		 }

		 public override Optional<MemberId> UpstreamDatabase()
		 {
			  return _filters.flatMap(_filters =>
			  {
				ISet<ServerInfo> possibleServers = possibleServers();

				return _filters.apply( possibleServers ).Select( ServerInfo.memberId ).Where( memberId => !Objects.Equals( Myself, memberId ) ).First();
			  });
		 }

		 private ISet<ServerInfo> PossibleServers()
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.stream.Stream<java.util.Map.Entry<org.neo4j.causalclustering.identity.MemberId,? extends org.neo4j.causalclustering.discovery.DiscoveryServerInfo>> infoMap = java.util.stream.Stream.of(topologyService.localReadReplicas(), topologyService.localCoreServers()).map(org.neo4j.causalclustering.discovery.Topology::members).map(java.util.Map::entrySet).flatMap(java.util.Set::stream);
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  Stream<KeyValuePair<MemberId, ? extends DiscoveryServerInfo>> infoMap = Stream.of( TopologyService.localReadReplicas(), TopologyService.localCoreServers() ).map(Topology::members).map(System.Collections.IDictionary.entrySet).flatMap(ISet<object>.stream);

			  return infoMap.map( this.toServerInfo ).collect( Collectors.toSet() );
		 }

		 private ServerInfo ToServerInfo<T>( KeyValuePair<MemberId, T> entry ) where T : Org.Neo4j.causalclustering.discovery.DiscoveryServerInfo
		 {
			  T server = entry.Value;
			  MemberId memberId = entry.Key;
			  return new ServerInfo( server.connectors().boltAddress(), memberId, server.groups() );
		 }
	}

}