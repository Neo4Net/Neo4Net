﻿/*
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
	using Assert = org.junit.Assert;
	using Test = org.junit.Test;


	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using TopologyService = Org.Neo4j.causalclustering.discovery.TopologyService;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static co.unruly.matchers.OptionalMatchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.isIn;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.upstream.strategies.UserDefinedConfigurationStrategyTest.memberIDs;

	public class ConnectRandomlyWithinServerGroupStrategyTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseServerGroupsFromConfig()
		 public virtual void ShouldUseServerGroupsFromConfig()
		 {
			  // given
			  const string myServerGroup = "my_server_group";
			  Config configWithMyServerGroup = Config.defaults( CausalClusteringSettings.server_groups, myServerGroup );
			  MemberId[] myGroupMemberIds = memberIDs( 10 );
			  TopologyService topologyService = ConnectRandomlyToServerGroupStrategyImplTest.GetTopologyService( Collections.singletonList( myServerGroup ), myGroupMemberIds, Collections.singletonList( "your_server_group" ) );

			  ConnectRandomlyWithinServerGroupStrategy strategy = new ConnectRandomlyWithinServerGroupStrategy();
			  strategy.Inject( topologyService, configWithMyServerGroup, NullLogProvider.Instance, myGroupMemberIds[0] );

			  // when
			  Optional<MemberId> result = strategy.UpstreamDatabase();

			  // then
			  assertThat( result, contains( isIn( myGroupMemberIds ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void filtersSelf()
		 public virtual void FiltersSelf()
		 {
			  // given
			  string groupName = "groupName";
			  Config config = Config.defaults();
			  config.Augment( CausalClusteringSettings.server_groups, groupName );

			  // and
			  ConnectRandomlyWithinServerGroupStrategy connectRandomlyWithinServerGroupStrategy = new ConnectRandomlyWithinServerGroupStrategy();
			  MemberId myself = new MemberId( new System.Guid( 123, 456 ) );
			  connectRandomlyWithinServerGroupStrategy.Inject( new TopologyServiceThatPrioritisesItself( myself, groupName ), config, NullLogProvider.Instance, myself );

			  // when
			  Optional<MemberId> result = connectRandomlyWithinServerGroupStrategy.UpstreamDatabase();

			  // then
			  Assert.assertTrue( result.Present );
			  Assert.assertNotEquals( myself, result.get() );
		 }
	}

}