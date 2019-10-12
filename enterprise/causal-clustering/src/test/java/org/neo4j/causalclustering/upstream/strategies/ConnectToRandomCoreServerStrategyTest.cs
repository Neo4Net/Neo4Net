using System.Collections.Generic;
using System.Diagnostics;

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
	using Assert = org.junit.Assert;
	using Test = org.junit.Test;


	using CoreServerInfo = Org.Neo4j.causalclustering.discovery.CoreServerInfo;
	using CoreTopology = Org.Neo4j.causalclustering.discovery.CoreTopology;
	using TestTopology = Org.Neo4j.causalclustering.discovery.TestTopology;
	using TopologyService = Org.Neo4j.causalclustering.discovery.TopologyService;
	using ClusterId = Org.Neo4j.causalclustering.identity.ClusterId;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.AnyOf.anyOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class ConnectToRandomCoreServerStrategyTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConnectToRandomCoreServer() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldConnectToRandomCoreServer()
		 {
			  // given
			  MemberId memberId1 = new MemberId( System.Guid.randomUUID() );
			  MemberId memberId2 = new MemberId( System.Guid.randomUUID() );
			  MemberId memberId3 = new MemberId( System.Guid.randomUUID() );

			  TopologyService topologyService = mock( typeof( TopologyService ) );
			  when( topologyService.LocalCoreServers() ).thenReturn(FakeCoreTopology(memberId1, memberId2, memberId3));

			  ConnectToRandomCoreServerStrategy connectionStrategy = new ConnectToRandomCoreServerStrategy();
			  connectionStrategy.Inject( topologyService, Config.defaults(), NullLogProvider.Instance, null );

			  // when
			  Optional<MemberId> memberId = connectionStrategy.UpstreamDatabase();

			  // then
			  assertTrue( memberId.Present );
			  assertThat( memberId.get(), anyOf(equalTo(memberId1), equalTo(memberId2), equalTo(memberId3)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void filtersSelf() throws org.neo4j.causalclustering.upstream.UpstreamDatabaseSelectionException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FiltersSelf()
		 {
			  // given
			  MemberId myself = new MemberId( new System.Guid( 1234, 5678 ) );
			  Config config = Config.defaults();
			  string groupName = "groupName";

			  // and
			  ConnectToRandomCoreServerStrategy connectToRandomCoreServerStrategy = new ConnectToRandomCoreServerStrategy();
			  connectToRandomCoreServerStrategy.Inject( new TopologyServiceThatPrioritisesItself( myself, groupName ), config, NullLogProvider.Instance, myself );

			  // when
			  Optional<MemberId> found = connectToRandomCoreServerStrategy.UpstreamDatabase();

			  // then
			  Assert.assertTrue( found.Present );
			  Assert.assertNotEquals( myself, found );
		 }

		 internal static CoreTopology FakeCoreTopology( params MemberId[] memberIds )
		 {
			  Debug.Assert( memberIds.Length > 0 );

			  ClusterId clusterId = new ClusterId( System.Guid.randomUUID() );
			  IDictionary<MemberId, CoreServerInfo> coreMembers = new Dictionary<MemberId, CoreServerInfo>();

			  int offset = 0;

			  foreach ( MemberId memberId in memberIds )
			  {
					coreMembers[memberId] = TestTopology.addressesForCore( offset, false );
					offset++;
			  }

			  return new CoreTopology( clusterId, false, coreMembers );
		 }
	}

}