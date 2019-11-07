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
	using Test = org.junit.Test;


	using ClientConnectorAddresses = Neo4Net.causalclustering.discovery.ClientConnectorAddresses;
	using ReadReplicaInfo = Neo4Net.causalclustering.discovery.ReadReplicaInfo;
	using ReadReplicaTopology = Neo4Net.causalclustering.discovery.ReadReplicaTopology;
	using TopologyService = Neo4Net.causalclustering.discovery.TopologyService;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static co.unruly.matchers.OptionalMatchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static co.unruly.matchers.OptionalMatchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.isIn;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.upstream.strategies.ConnectToRandomCoreServerStrategyTest.fakeCoreTopology;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.upstream.strategies.UserDefinedConfigurationStrategyTest.fakeTopologyService;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.upstream.strategies.UserDefinedConfigurationStrategyTest.memberIDs;

	public class ConnectRandomlyToServerGroupStrategyImplTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStayWithinGivenSingleServerGroup()
		 public virtual void ShouldStayWithinGivenSingleServerGroup()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<String> myServerGroup = java.util.Collections.singletonList("my_server_group");
			  IList<string> myServerGroup = Collections.singletonList( "my_server_group" );

			  MemberId[] myGroupMemberIds = memberIDs( 10 );
			  TopologyService topologyService = GetTopologyService( myServerGroup, myGroupMemberIds, Collections.singletonList( "your_server_group" ) );

			  ConnectRandomlyToServerGroupImpl strategy = new ConnectRandomlyToServerGroupImpl( myServerGroup, topologyService, myGroupMemberIds[0] );

			  // when
			  Optional<MemberId> memberId = strategy.UpstreamDatabase();

			  // then
			  assertThat( memberId, contains( isIn( myGroupMemberIds ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSelectAnyFromMultipleServerGroups()
		 public virtual void ShouldSelectAnyFromMultipleServerGroups()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<String> myServerGroups = java.util.Arrays.asList("a", "b", "c");
			  IList<string> myServerGroups = Arrays.asList( "a", "b", "c" );

			  MemberId[] myGroupMemberIds = memberIDs( 10 );
			  TopologyService topologyService = GetTopologyService( myServerGroups, myGroupMemberIds, Arrays.asList( "x", "y", "z" ) );

			  ConnectRandomlyToServerGroupImpl strategy = new ConnectRandomlyToServerGroupImpl( myServerGroups, topologyService, myGroupMemberIds[0] );

			  // when
			  Optional<MemberId> memberId = strategy.UpstreamDatabase();

			  // then
			  assertThat( memberId, contains( isIn( myGroupMemberIds ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnEmptyIfNoGroupsInConfig()
		 public virtual void ShouldReturnEmptyIfNoGroupsInConfig()
		 {
			  // given
			  MemberId[] myGroupMemberIds = memberIDs( 10 );
			  TopologyService topologyService = GetTopologyService( Collections.singletonList( "my_server_group" ), myGroupMemberIds, Arrays.asList( "x", "y", "z" ) );
			  ConnectRandomlyToServerGroupImpl strategy = new ConnectRandomlyToServerGroupImpl( Collections.emptyList(), topologyService, null );

			  // when
			  Optional<MemberId> memberId = strategy.UpstreamDatabase();

			  // then
			  assertThat( memberId, empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnEmptyIfGroupOnlyContainsSelf()
		 public virtual void ShouldReturnEmptyIfGroupOnlyContainsSelf()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<String> myServerGroup = java.util.Collections.singletonList("group");
			  IList<string> myServerGroup = Collections.singletonList( "group" );

			  MemberId[] myGroupMemberIds = memberIDs( 1 );
			  TopologyService topologyService = GetTopologyService( myServerGroup, myGroupMemberIds, Arrays.asList( "x", "y", "z" ) );

			  ConnectRandomlyToServerGroupImpl strategy = new ConnectRandomlyToServerGroupImpl( myServerGroup, topologyService, myGroupMemberIds[0] );

			  // when
			  Optional<MemberId> memberId = strategy.UpstreamDatabase();

			  // then
			  assertThat( memberId, empty() );
		 }

		 internal static TopologyService GetTopologyService( IList<string> myServerGroups, MemberId[] myGroupMemberIds, IList<string> unwanted )
		 {
			  return fakeTopologyService( fakeCoreTopology( new MemberId( System.Guid.randomUUID() ) ), FakeReadReplicaTopology(myServerGroups, myGroupMemberIds, unwanted, 10) );
		 }

		 internal static ReadReplicaTopology FakeReadReplicaTopology( IList<string> wanted, MemberId[] memberIds, IList<string> unwanted, int unwantedNumber )
		 {
			  IDictionary<MemberId, ReadReplicaInfo> readReplicas = new Dictionary<MemberId, ReadReplicaInfo>();

			  int offset = 0;

			  foreach ( MemberId memberId in memberIds )
			  {
					readReplicas[memberId] = new ReadReplicaInfo( new ClientConnectorAddresses( singletonList( new ClientConnectorAddresses.ConnectorUri( ClientConnectorAddresses.Scheme.bolt, new AdvertisedSocketAddress( "localhost", 11000 + offset ) ) ) ), new AdvertisedSocketAddress( "localhost", 10000 + offset ), new HashSet<string>( wanted ), "default" );

					offset++;
			  }

			  for ( int i = 0; i < unwantedNumber; i++ )
			  {
					readReplicas[new MemberId( System.Guid.randomUUID() )] = new ReadReplicaInfo(new ClientConnectorAddresses(singletonList(new ClientConnectorAddresses.ConnectorUri(ClientConnectorAddresses.Scheme.bolt, new AdvertisedSocketAddress("localhost", 11000 + offset)))), new AdvertisedSocketAddress("localhost", 10000 + offset), new HashSet<string>(unwanted), "default");

					offset++;
			  }

			  return new ReadReplicaTopology( readReplicas );
		 }
	}

}