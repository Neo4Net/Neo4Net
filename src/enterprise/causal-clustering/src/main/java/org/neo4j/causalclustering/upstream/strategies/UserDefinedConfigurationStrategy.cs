using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.upstream.strategies
{

	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using DiscoveryServerInfo = Neo4Net.causalclustering.discovery.DiscoveryServerInfo;
	using Neo4Net.causalclustering.discovery;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Neo4Net.causalclustering.routing.load_balancing.filters;
	using FilterConfigParser = Neo4Net.causalclustering.routing.load_balancing.plugins.server_policies.FilterConfigParser;
	using InvalidFilterSpecification = Neo4Net.causalclustering.routing.load_balancing.plugins.server_policies.InvalidFilterSpecification;
	using ServerInfo = Neo4Net.causalclustering.routing.load_balancing.plugins.server_policies.ServerInfo;
	using Service = Neo4Net.Helpers.Service;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(UpstreamDatabaseSelectionStrategy.class) public class UserDefinedConfigurationStrategy extends org.Neo4Net.causalclustering.upstream.UpstreamDatabaseSelectionStrategy
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
//ORIGINAL LINE: java.util.stream.Stream<java.util.Map.Entry<org.Neo4Net.causalclustering.identity.MemberId,? extends org.Neo4Net.causalclustering.discovery.DiscoveryServerInfo>> infoMap = java.util.stream.Stream.of(topologyService.localReadReplicas(), topologyService.localCoreServers()).map(org.Neo4Net.causalclustering.discovery.Topology::members).map(java.util.Map::entrySet).flatMap(java.util.Set::stream);
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  Stream<KeyValuePair<MemberId, ? extends DiscoveryServerInfo>> infoMap = Stream.of( TopologyService.localReadReplicas(), TopologyService.localCoreServers() ).map(Topology::members).map(System.Collections.IDictionary.entrySet).flatMap(ISet<object>.stream);

			  return infoMap.map( this.toServerInfo ).collect( Collectors.toSet() );
		 }

		 private ServerInfo ToServerInfo<T>( KeyValuePair<MemberId, T> entry ) where T : Neo4Net.causalclustering.discovery.DiscoveryServerInfo
		 {
			  T server = entry.Value;
			  MemberId memberId = entry.Key;
			  return new ServerInfo( server.connectors().boltAddress(), memberId, server.groups() );
		 }
	}

}