using System.Collections.Generic;

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
	using IsNot = org.hamcrest.core.IsNot;
	using Assert = org.junit.Assert;
	using Test = org.junit.Test;


	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using TopologyService = Neo4Net.causalclustering.discovery.TopologyService;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Config = Neo4Net.Kernel.configuration.Config;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.upstream.strategies.ConnectToRandomCoreServerStrategyTest.fakeCoreTopology;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.upstream.strategies.UserDefinedConfigurationStrategyTest.fakeReadReplicaTopology;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.upstream.strategies.UserDefinedConfigurationStrategyTest.fakeTopologyService;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.upstream.strategies.UserDefinedConfigurationStrategyTest.memberIDs;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.cypher.@internal.codegen.CompiledConversionUtils.not;

	public class TypicallyConnectToRandomReadReplicaStrategyTest
	{
		 internal MemberId Myself = new MemberId( new System.Guid( 1234, 5678 ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConnectToCoreOneInTenTimesByDefault()
		 public virtual void ShouldConnectToCoreOneInTenTimesByDefault()
		 {
			  // given
			  MemberId theCoreMemberId = new MemberId( System.Guid.randomUUID() );
			  TopologyService topologyService = fakeTopologyService( fakeCoreTopology( theCoreMemberId ), fakeReadReplicaTopology( memberIDs( 100 ) ) );

			  Config config = mock( typeof( Config ) );
			  when( config.Get( CausalClusteringSettings.database ) ).thenReturn( "default" );

			  TypicallyConnectToRandomReadReplicaStrategy connectionStrategy = new TypicallyConnectToRandomReadReplicaStrategy( 2 );
			  connectionStrategy.Inject( topologyService, config, NullLogProvider.Instance, Myself );

			  IList<MemberId> responses = new List<MemberId>();

			  // when
			  for ( int i = 0; i < 3; i++ )
			  {
					for ( int j = 0; j < 2; j++ )
					{
						 responses.Add( connectionStrategy.UpstreamDatabase().get() );
					}
					assertThat( responses, hasItem( theCoreMemberId ) );
					responses.Clear();
			  }

			  // then
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void filtersSelf()
		 public virtual void FiltersSelf()
		 {
			  // given
			  string groupName = "groupName";
			  Config config = Config.defaults();

			  TypicallyConnectToRandomReadReplicaStrategy typicallyConnectToRandomReadReplicaStrategy = new TypicallyConnectToRandomReadReplicaStrategy();
			  typicallyConnectToRandomReadReplicaStrategy.Inject( new TopologyServiceThatPrioritisesItself( Myself, groupName ), config, NullLogProvider.Instance, Myself );

			  // when
			  Optional<MemberId> found = typicallyConnectToRandomReadReplicaStrategy.UpstreamDatabase();

			  // then
			  assertTrue( found.Present );
			  assertNotEquals( Myself, found );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void onCounterTriggerFiltersSelf()
		 public virtual void OnCounterTriggerFiltersSelf()
		 {
			  // given counter always triggers to get a core member
			  TypicallyConnectToRandomReadReplicaStrategy connectionStrategy = new TypicallyConnectToRandomReadReplicaStrategy( 1 );

			  // and requesting core member will return self and another member
			  MemberId otherCoreMember = new MemberId( new System.Guid( 12, 34 ) );
			  TopologyService topologyService = fakeTopologyService( fakeCoreTopology( Myself, otherCoreMember ), fakeReadReplicaTopology( memberIDs( 2 ) ) );
			  connectionStrategy.Inject( topologyService, Config.defaults(), NullLogProvider.Instance, Myself );

			  // when
			  Optional<MemberId> found = connectionStrategy.UpstreamDatabase();

			  // then
			  assertTrue( found.Present );
			  assertNotEquals( Myself, found.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void randomCoreDoesNotReturnSameCoreTwice()
		 public virtual void RandomCoreDoesNotReturnSameCoreTwice()
		 {
			  // given counter always core member
			  TypicallyConnectToRandomReadReplicaStrategy connectionStrategy = new TypicallyConnectToRandomReadReplicaStrategy( 1 );

			  // and
			  MemberId firstOther = new MemberId( new System.Guid( 12, 34 ) );
			  MemberId secondOther = new MemberId( new System.Guid( 56, 78 ) );
			  TopologyService topologyService = fakeTopologyService( fakeCoreTopology( Myself, firstOther, secondOther ), fakeReadReplicaTopology( memberIDs( 2 ) ) );
			  connectionStrategy.Inject( topologyService, Config.defaults(), NullLogProvider.Instance, Myself );

			  // when we collect enough results to feel confident of random values
			  IList<MemberId> found = IntStream.range( 0, 20 ).mapToObj( i => connectionStrategy.UpstreamDatabase() ).filter(Optional.isPresent).map(Optional.get).collect(Collectors.toList());

			  // then
			  assertFalse( found.Contains( Myself ) );
			  assertTrue( found.Contains( firstOther ) );
			  assertTrue( found.Contains( secondOther ) );
		 }
	}

}