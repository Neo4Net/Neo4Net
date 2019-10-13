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
namespace Neo4Net.causalclustering.upstream.strategies
{
	using Test = org.junit.Test;


	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using TopologyService = Neo4Net.causalclustering.discovery.TopologyService;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static co.unruly.matchers.OptionalMatchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.isIn;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.upstream.strategies.UserDefinedConfigurationStrategyTest.memberIDs;

	public class ConnectRandomlyToServerGroupStrategyTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConnectToGroupDefinedInStrategySpecificConfig()
		 public virtual void ShouldConnectToGroupDefinedInStrategySpecificConfig()
		 {
			  // given
			  const string targetServerGroup = "target_server_group";
			  Config configWithTargetServerGroup = Config.defaults( CausalClusteringSettings.connect_randomly_to_server_group_strategy, targetServerGroup );
			  MemberId[] targetGroupMemberIds = memberIDs( 10 );
			  TopologyService topologyService = ConnectRandomlyToServerGroupStrategyImplTest.GetTopologyService( Collections.singletonList( targetServerGroup ), targetGroupMemberIds, Collections.singletonList( "your_server_group" ) );

			  ConnectRandomlyToServerGroupStrategy strategy = new ConnectRandomlyToServerGroupStrategy();
			  strategy.Inject( topologyService, configWithTargetServerGroup, NullLogProvider.Instance, targetGroupMemberIds[0] );

			  // when
			  Optional<MemberId> result = strategy.UpstreamDatabase();

			  // then
			  assertThat( result, contains( isIn( targetGroupMemberIds ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void doesNotConnectToSelf()
		 public virtual void DoesNotConnectToSelf()
		 {
			  // given
			  ConnectRandomlyToServerGroupStrategy connectRandomlyToServerGroupStrategy = new ConnectRandomlyToServerGroupStrategy();
			  MemberId myself = new MemberId( new System.Guid( 1234, 5678 ) );

			  // and
			  LogProvider logProvider = NullLogProvider.Instance;
			  Config config = Config.defaults();
			  config.Augment( CausalClusteringSettings.connect_randomly_to_server_group_strategy, "firstGroup" );
			  TopologyService topologyService = new TopologyServiceThatPrioritisesItself( myself, "firstGroup" );
			  connectRandomlyToServerGroupStrategy.Inject( topologyService, config, logProvider, myself );

			  // when
			  Optional<MemberId> found = connectRandomlyToServerGroupStrategy.UpstreamDatabase();

			  // then
			  assertTrue( found.Present );
			  assertNotEquals( myself, found.get() );
		 }
	}

}