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
namespace Org.Neo4j.causalclustering.routing.load_balancing.procedure
{
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using LeaderLocator = Org.Neo4j.causalclustering.core.consensus.LeaderLocator;
	using NoLeaderFoundException = Org.Neo4j.causalclustering.core.consensus.NoLeaderFoundException;
	using CoreServerInfo = Org.Neo4j.causalclustering.discovery.CoreServerInfo;
	using CoreTopology = Org.Neo4j.causalclustering.discovery.CoreTopology;
	using CoreTopologyService = Org.Neo4j.causalclustering.discovery.CoreTopologyService;
	using ReadReplicaTopology = Org.Neo4j.causalclustering.discovery.ReadReplicaTopology;
	using ClusterId = Org.Neo4j.causalclustering.identity.ClusterId;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;
	using ProcedureException = Org.Neo4j.@internal.Kernel.Api.exceptions.ProcedureException;
	using FieldSignature = Org.Neo4j.@internal.Kernel.Api.procs.FieldSignature;
	using ProcedureSignature = Org.Neo4j.@internal.Kernel.Api.procs.ProcedureSignature;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.cluster_allow_reads_on_followers;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.cluster_routing_ttl;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.TestTopology.addressesForCore;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.TestTopology.addressesForReadReplica;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.TestTopology.readReplicaInfoMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.identity.RaftTestMember.member;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTInteger;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.procs.Neo4jTypes.NTMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.NullLogProvider.getInstance;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class GetServersProcedureV1Test
	public class GetServersProcedureV1Test
	{
		 private readonly ClusterId _clusterId = new ClusterId( System.Guid.randomUUID() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(0) public String description;
		 public string Description;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(1) public org.neo4j.kernel.configuration.Config config;
		 public Config Config;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(2) public boolean expectFollowersAsReadEndPoints;
		 public bool ExpectFollowersAsReadEndPoints;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.Collection<Object[]> params()
		 public static ICollection<object[]> Params()
		 {
			  return Arrays.asList( new object[]{ "with followers as read end points", Config.defaults( cluster_allow_reads_on_followers, Settings.TRUE ), true }, new object[]{ "no followers as read end points", Config.defaults( cluster_allow_reads_on_followers, Settings.FALSE ), false } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void ttlShouldBeInSeconds() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TtlShouldBeInSeconds()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.discovery.CoreTopologyService coreTopologyService = mock(org.neo4j.causalclustering.discovery.CoreTopologyService.class);
			  CoreTopologyService coreTopologyService = mock( typeof( CoreTopologyService ) );

			  LeaderLocator leaderLocator = mock( typeof( LeaderLocator ) );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.discovery.CoreTopology clusterTopology = new org.neo4j.causalclustering.discovery.CoreTopology(clusterId, false, new java.util.HashMap<>());
			  CoreTopology clusterTopology = new CoreTopology( _clusterId, false, new Dictionary<MemberId, CoreServerInfo>() );
			  when( coreTopologyService.LocalCoreServers() ).thenReturn(clusterTopology);
			  when( coreTopologyService.LocalReadReplicas() ).thenReturn(new ReadReplicaTopology(emptyMap()));

			  // set the TTL in minutes
			  Config.augment( cluster_routing_ttl, "10m" );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LegacyGetServersProcedure proc = new LegacyGetServersProcedure(coreTopologyService, leaderLocator, config, getInstance());
			  LegacyGetServersProcedure proc = new LegacyGetServersProcedure( coreTopologyService, leaderLocator, Config, Instance );

			  // when
			  IList<object[]> results = new IList<object[]> { proc.Apply( null, new object[0], null ) };

			  // then
			  object[] rows = results[0];
			  long ttlInSeconds = ( long ) rows[0];
			  assertEquals( 600, ttlInSeconds );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveCorrectSignature()
		 public virtual void ShouldHaveCorrectSignature()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LegacyGetServersProcedure proc = new LegacyGetServersProcedure(null, null, config, getInstance());
			  LegacyGetServersProcedure proc = new LegacyGetServersProcedure( null, null, Config, Instance );

			  // when
			  ProcedureSignature signature = proc.Signature();

			  // then
			  assertThat( signature.OutputSignature(), containsInAnyOrder(FieldSignature.outputField("ttl", NTInteger), FieldSignature.outputField("servers", NTList(NTMap))) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideReaderAndRouterForSingleCoreSetup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProvideReaderAndRouterForSingleCoreSetup()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.discovery.CoreTopologyService coreTopologyService = mock(org.neo4j.causalclustering.discovery.CoreTopologyService.class);
			  CoreTopologyService coreTopologyService = mock( typeof( CoreTopologyService ) );

			  LeaderLocator leaderLocator = mock( typeof( LeaderLocator ) );

			  IDictionary<MemberId, CoreServerInfo> coreMembers = new Dictionary<MemberId, CoreServerInfo>();
			  coreMembers[member( 0 )] = addressesForCore( 0, false );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.discovery.CoreTopology clusterTopology = new org.neo4j.causalclustering.discovery.CoreTopology(clusterId, false, coreMembers);
			  CoreTopology clusterTopology = new CoreTopology( _clusterId, false, coreMembers );
			  when( coreTopologyService.LocalCoreServers() ).thenReturn(clusterTopology);
			  when( coreTopologyService.LocalReadReplicas() ).thenReturn(new ReadReplicaTopology(emptyMap()));

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LegacyGetServersProcedure proc = new LegacyGetServersProcedure(coreTopologyService, leaderLocator, config, getInstance());
			  LegacyGetServersProcedure proc = new LegacyGetServersProcedure( coreTopologyService, leaderLocator, Config, Instance );

			  // when
			  ClusterView clusterView = Run( proc );

			  // then
			  ClusterView.Builder builder = new ClusterView.Builder();
			  builder.ReadAddress( addressesForCore( 0, false ).connectors().boltAddress() );
			  builder.RouteAddress( addressesForCore( 0, false ).connectors().boltAddress() );

			  assertEquals( builder.Build(), clusterView );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnCoreServersWithRouteAllCoresButLeaderAsReadAndSingleWriteActions() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnCoreServersWithRouteAllCoresButLeaderAsReadAndSingleWriteActions()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.discovery.CoreTopologyService coreTopologyService = mock(org.neo4j.causalclustering.discovery.CoreTopologyService.class);
			  CoreTopologyService coreTopologyService = mock( typeof( CoreTopologyService ) );

			  LeaderLocator leaderLocator = mock( typeof( LeaderLocator ) );
			  when( leaderLocator.Leader ).thenReturn( member( 0 ) );

			  IDictionary<MemberId, CoreServerInfo> coreMembers = new Dictionary<MemberId, CoreServerInfo>();
			  coreMembers[member( 0 )] = addressesForCore( 0, false );
			  coreMembers[member( 1 )] = addressesForCore( 1, false );
			  coreMembers[member( 2 )] = addressesForCore( 2, false );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.discovery.CoreTopology clusterTopology = new org.neo4j.causalclustering.discovery.CoreTopology(clusterId, false, coreMembers);
			  CoreTopology clusterTopology = new CoreTopology( _clusterId, false, coreMembers );
			  when( coreTopologyService.LocalCoreServers() ).thenReturn(clusterTopology);
			  when( coreTopologyService.LocalReadReplicas() ).thenReturn(new ReadReplicaTopology(emptyMap()));

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LegacyGetServersProcedure proc = new LegacyGetServersProcedure(coreTopologyService, leaderLocator, config, getInstance());
			  LegacyGetServersProcedure proc = new LegacyGetServersProcedure( coreTopologyService, leaderLocator, Config, Instance );

			  // when
			  ClusterView clusterView = Run( proc );

			  // then
			  ClusterView.Builder builder = new ClusterView.Builder();
			  builder.WriteAddress( addressesForCore( 0, false ).connectors().boltAddress() );
			  builder.ReadAddress( addressesForCore( 1, false ).connectors().boltAddress() );
			  builder.ReadAddress( addressesForCore( 2, false ).connectors().boltAddress() );
			  builder.RouteAddress( addressesForCore( 0, false ).connectors().boltAddress() );
			  builder.RouteAddress( addressesForCore( 1, false ).connectors().boltAddress() );
			  builder.RouteAddress( addressesForCore( 2, false ).connectors().boltAddress() );

			  assertEquals( builder.Build(), clusterView );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnSelfIfOnlyMemberOfTheCluster() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnSelfIfOnlyMemberOfTheCluster()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.discovery.CoreTopologyService coreTopologyService = mock(org.neo4j.causalclustering.discovery.CoreTopologyService.class);
			  CoreTopologyService coreTopologyService = mock( typeof( CoreTopologyService ) );

			  LeaderLocator leaderLocator = mock( typeof( LeaderLocator ) );
			  when( leaderLocator.Leader ).thenReturn( member( 0 ) );

			  IDictionary<MemberId, CoreServerInfo> coreMembers = new Dictionary<MemberId, CoreServerInfo>();
			  coreMembers[member( 0 )] = addressesForCore( 0, false );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.discovery.CoreTopology clusterTopology = new org.neo4j.causalclustering.discovery.CoreTopology(clusterId, false, coreMembers);
			  CoreTopology clusterTopology = new CoreTopology( _clusterId, false, coreMembers );
			  when( coreTopologyService.LocalCoreServers() ).thenReturn(clusterTopology);
			  when( coreTopologyService.LocalReadReplicas() ).thenReturn(new ReadReplicaTopology(emptyMap()));

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LegacyGetServersProcedure proc = new LegacyGetServersProcedure(coreTopologyService, leaderLocator, config, getInstance());
			  LegacyGetServersProcedure proc = new LegacyGetServersProcedure( coreTopologyService, leaderLocator, Config, Instance );

			  // when
			  ClusterView clusterView = Run( proc );

			  // then
			  ClusterView.Builder builder = new ClusterView.Builder();
			  builder.WriteAddress( addressesForCore( 0, false ).connectors().boltAddress() );
			  builder.ReadAddress( addressesForCore( 0, false ).connectors().boltAddress() );
			  builder.RouteAddress( addressesForCore( 0, false ).connectors().boltAddress() );

			  assertEquals( builder.Build(), clusterView );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnTheCoreLeaderForWriteAndReadReplicasAndCoresForReads() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnTheCoreLeaderForWriteAndReadReplicasAndCoresForReads()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.discovery.CoreTopologyService topologyService = mock(org.neo4j.causalclustering.discovery.CoreTopologyService.class);
			  CoreTopologyService topologyService = mock( typeof( CoreTopologyService ) );

			  IDictionary<MemberId, CoreServerInfo> coreMembers = new Dictionary<MemberId, CoreServerInfo>();
			  MemberId theLeader = member( 0 );
			  coreMembers[theLeader] = addressesForCore( 0, false );

			  when( topologyService.LocalCoreServers() ).thenReturn(new CoreTopology(_clusterId, false, coreMembers));
			  when( topologyService.LocalReadReplicas() ).thenReturn(new ReadReplicaTopology(readReplicaInfoMap(1)));

			  LeaderLocator leaderLocator = mock( typeof( LeaderLocator ) );
			  when( leaderLocator.Leader ).thenReturn( theLeader );

			  LegacyGetServersProcedure procedure = new LegacyGetServersProcedure( topologyService, leaderLocator, Config, Instance );

			  // when
			  ClusterView clusterView = Run( procedure );

			  // then
			  ClusterView.Builder builder = new ClusterView.Builder();
			  builder.WriteAddress( addressesForCore( 0, false ).connectors().boltAddress() );
			  if ( ExpectFollowersAsReadEndPoints )
			  {
					builder.ReadAddress( addressesForCore( 0, false ).connectors().boltAddress() );
			  }
			  builder.ReadAddress( addressesForReadReplica( 1 ).connectors().boltAddress() );
			  builder.RouteAddress( addressesForCore( 0, false ).connectors().boltAddress() );

			  assertEquals( builder.Build(), clusterView );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnCoreMemberAsReadServerIfNoReadReplicasAvailable() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnCoreMemberAsReadServerIfNoReadReplicasAvailable()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.discovery.CoreTopologyService topologyService = mock(org.neo4j.causalclustering.discovery.CoreTopologyService.class);
			  CoreTopologyService topologyService = mock( typeof( CoreTopologyService ) );

			  IDictionary<MemberId, CoreServerInfo> coreMembers = new Dictionary<MemberId, CoreServerInfo>();
			  MemberId theLeader = member( 0 );
			  coreMembers[theLeader] = addressesForCore( 0, false );

			  when( topologyService.LocalCoreServers() ).thenReturn(new CoreTopology(_clusterId, false, coreMembers));
			  when( topologyService.LocalReadReplicas() ).thenReturn(new ReadReplicaTopology(emptyMap()));

			  LeaderLocator leaderLocator = mock( typeof( LeaderLocator ) );
			  when( leaderLocator.Leader ).thenReturn( theLeader );

			  LegacyGetServersProcedure procedure = new LegacyGetServersProcedure( topologyService, leaderLocator, Config, Instance );

			  // when
			  ClusterView clusterView = Run( procedure );

			  // then
			  ClusterView.Builder builder = new ClusterView.Builder();
			  builder.WriteAddress( addressesForCore( 0, false ).connectors().boltAddress() );
			  builder.ReadAddress( addressesForCore( 0, false ).connectors().boltAddress() );
			  builder.RouteAddress( addressesForCore( 0, false ).connectors().boltAddress() );

			  assertEquals( builder.Build(), clusterView );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnNoWriteEndpointsIfThereIsNoLeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnNoWriteEndpointsIfThereIsNoLeader()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.discovery.CoreTopologyService topologyService = mock(org.neo4j.causalclustering.discovery.CoreTopologyService.class);
			  CoreTopologyService topologyService = mock( typeof( CoreTopologyService ) );

			  IDictionary<MemberId, CoreServerInfo> coreMembers = new Dictionary<MemberId, CoreServerInfo>();
			  coreMembers[member( 0 )] = addressesForCore( 0, false );

			  when( topologyService.LocalCoreServers() ).thenReturn(new CoreTopology(_clusterId, false, coreMembers));
			  when( topologyService.LocalReadReplicas() ).thenReturn(new ReadReplicaTopology(emptyMap()));

			  LeaderLocator leaderLocator = mock( typeof( LeaderLocator ) );
			  when( leaderLocator.Leader ).thenThrow( new NoLeaderFoundException() );

			  LegacyGetServersProcedure procedure = new LegacyGetServersProcedure( topologyService, leaderLocator, Config, Instance );

			  // when
			  ClusterView clusterView = Run( procedure );

			  // then
			  ClusterView.Builder builder = new ClusterView.Builder();
			  builder.ReadAddress( addressesForCore( 0, false ).connectors().boltAddress() );
			  builder.RouteAddress( addressesForCore( 0, false ).connectors().boltAddress() );

			  assertEquals( builder.Build(), clusterView );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnNoWriteEndpointsIfThereIsNoAddressForTheLeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnNoWriteEndpointsIfThereIsNoAddressForTheLeader()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.discovery.CoreTopologyService topologyService = mock(org.neo4j.causalclustering.discovery.CoreTopologyService.class);
			  CoreTopologyService topologyService = mock( typeof( CoreTopologyService ) );

			  IDictionary<MemberId, CoreServerInfo> coreMembers = new Dictionary<MemberId, CoreServerInfo>();
			  coreMembers[member( 0 )] = addressesForCore( 0, false );

			  when( topologyService.LocalCoreServers() ).thenReturn(new CoreTopology(_clusterId, false, coreMembers));
			  when( topologyService.LocalReadReplicas() ).thenReturn(new ReadReplicaTopology(emptyMap()));

			  LeaderLocator leaderLocator = mock( typeof( LeaderLocator ) );
			  when( leaderLocator.Leader ).thenReturn( member( 1 ) );

			  LegacyGetServersProcedure procedure = new LegacyGetServersProcedure( topologyService, leaderLocator, Config, Instance );

			  // when
			  ClusterView clusterView = Run( procedure );

			  // then

			  ClusterView.Builder builder = new ClusterView.Builder();
			  builder.ReadAddress( addressesForCore( 0, false ).connectors().boltAddress() );
			  builder.RouteAddress( addressesForCore( 0, false ).connectors().boltAddress() );

			  assertEquals( builder.Build(), clusterView );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private ClusterView run(LegacyGetServersProcedure proc) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private ClusterView Run( LegacyGetServersProcedure proc )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Object[] rows = asList(proc.apply(null, new Object[0], null)).get(0);
			  object[] rows = asList( proc.Apply( null, new object[0], null ) ).get( 0 );
			  assertEquals( Config.get( cluster_routing_ttl ).Seconds, ( long ) rows[0] );
			  return ClusterView.parse( ( IList<IDictionary<string, object>> ) rows[1] );
		 }

		 private class ClusterView
		 {
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  internal readonly IDictionary<Role, ISet<AdvertisedSocketAddress>> ClusterViewConflict;

			  internal ClusterView( IDictionary<Role, ISet<AdvertisedSocketAddress>> clusterView )
			  {
					this.ClusterViewConflict = clusterView;
			  }

			  public override bool Equals( object o )
			  {
					if ( this == o )
					{
						 return true;
					}
					if ( o == null || this.GetType() != o.GetType() )
					{
						 return false;
					}
					ClusterView that = ( ClusterView ) o;
					return Objects.Equals( ClusterViewConflict, that.ClusterViewConflict );
			  }

			  public override int GetHashCode()
			  {
					return Objects.hash( ClusterViewConflict );
			  }

			  public override string ToString()
			  {
					return "ClusterView{" + "clusterView=" + ClusterViewConflict + '}';
			  }

			  internal static ClusterView Parse( IList<IDictionary<string, object>> result )
			  {
					IDictionary<Role, ISet<AdvertisedSocketAddress>> view = new Dictionary<Role, ISet<AdvertisedSocketAddress>>();
					foreach ( IDictionary<string, object> single in result )
					{
						 Role role = Enum.Parse( typeof( Role ), ( string ) single["role"] );
						 ISet<AdvertisedSocketAddress> addresses = parse( ( object[] ) single["addresses"] );
						 assertFalse( view.ContainsKey( role ) );
						 view[role] = addresses;
					}

					return new ClusterView( view );
			  }

			  internal static ISet<AdvertisedSocketAddress> Parse( object[] addresses )
			  {
					IList<AdvertisedSocketAddress> list = Stream.of( addresses ).map( address => Parse( ( string ) address ) ).collect( toList() );
					ISet<AdvertisedSocketAddress> set = new HashSet<AdvertisedSocketAddress>( list );
					assertEquals( list.Count, set.Count );
					return set;
			  }

			  internal static AdvertisedSocketAddress Parse( string address )
			  {
					string[] split = address.Split( ":", true );
					assertEquals( 2, split.Length );
					return new AdvertisedSocketAddress( split[0], Convert.ToInt32( split[1] ) );
			  }

			  internal class Builder
			  {
					internal readonly IDictionary<Role, ISet<AdvertisedSocketAddress>> View = new Dictionary<Role, ISet<AdvertisedSocketAddress>>();

					internal virtual Builder ReadAddress( AdvertisedSocketAddress address )
					{
						 AddAddress( Role.READ, address );
						 return this;
					}

					internal virtual Builder WriteAddress( AdvertisedSocketAddress address )
					{
						 AddAddress( Role.WRITE, address );
						 return this;
					}

					internal virtual Builder RouteAddress( AdvertisedSocketAddress address )
					{
						 AddAddress( Role.ROUTE, address );
						 return this;
					}

					internal virtual void AddAddress( Role role, AdvertisedSocketAddress address )
					{
						 ISet<AdvertisedSocketAddress> advertisedSocketAddresses = View.computeIfAbsent( role, k => new HashSet<AdvertisedSocketAddress>() );
						 advertisedSocketAddresses.Add( address );
					}

					public virtual ClusterView Build()
					{
						 return new ClusterView( View );
					}
			  }
		 }
	}

}