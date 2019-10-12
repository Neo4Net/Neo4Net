using System;
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
	using Test = org.junit.Test;


	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using ClientConnectorAddresses = Org.Neo4j.causalclustering.discovery.ClientConnectorAddresses;
	using CoreTopology = Org.Neo4j.causalclustering.discovery.CoreTopology;
	using ReadReplicaInfo = Org.Neo4j.causalclustering.discovery.ReadReplicaInfo;
	using ReadReplicaTopology = Org.Neo4j.causalclustering.discovery.ReadReplicaTopology;
	using RoleInfo = Org.Neo4j.causalclustering.discovery.RoleInfo;
	using TopologyService = Org.Neo4j.causalclustering.discovery.TopologyService;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static co.unruly.matchers.OptionalMatchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static co.unruly.matchers.OptionalMatchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.isIn;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.HazelcastClusterTopology.extractCatchupAddressesMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.upstream.strategies.ConnectToRandomCoreServerStrategyTest.fakeCoreTopology;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;

	public class UserDefinedConfigurationStrategyTest

	{
		private bool InstanceFieldsInitialized = false;

		public UserDefinedConfigurationStrategyTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_noEastGroup = Arrays.asList( _northGroup, _southGroup, _westGroup );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPickTheFirstMatchingServerIfCore()
		 public virtual void ShouldPickTheFirstMatchingServerIfCore()
		 {
			  // given
			  MemberId theCoreMemberId = new MemberId( System.Guid.randomUUID() );
			  TopologyService topologyService = FakeTopologyService( fakeCoreTopology( theCoreMemberId ), FakeReadReplicaTopology( MemberIDs( 100 ), this.noEastGroupGenerator ) );

			  UserDefinedConfigurationStrategy strategy = new UserDefinedConfigurationStrategy();
			  Config config = Config.defaults( CausalClusteringSettings.user_defined_upstream_selection_strategy, "groups(east); groups(core); halt()" );

			  strategy.Inject( topologyService, config, NullLogProvider.Instance, null );

			  //when

			  Optional<MemberId> memberId = strategy.UpstreamDatabase();

			  // then
			  assertThat( memberId, contains( theCoreMemberId ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPickTheFirstMatchingServerIfReadReplica()
		 public virtual void ShouldPickTheFirstMatchingServerIfReadReplica()
		 {
			  // given
			  MemberId[] readReplicaIds = MemberIDs( 100 );
			  TopologyService topologyService = FakeTopologyService( fakeCoreTopology( new MemberId( System.Guid.randomUUID() ) ), FakeReadReplicaTopology(readReplicaIds, this.noEastGroupGenerator) );

			  UserDefinedConfigurationStrategy strategy = new UserDefinedConfigurationStrategy();
			  string wantedGroup = _noEastGroup[1];
			  Config config = ConfigWithFilter( "groups(" + wantedGroup + "); halt()" );

			  strategy.Inject( topologyService, config, NullLogProvider.Instance, null );

			  //when

			  Optional<MemberId> memberId = strategy.UpstreamDatabase();

			  // then
			  assertThat( memberId, contains( isIn( readReplicaIds ) ) );
			  assertThat( memberId.map( this.noEastGroupGenerator ), contains( equalTo( asSet( wantedGroup ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnEmptyIfNoMatchingServers()
		 public virtual void ShouldReturnEmptyIfNoMatchingServers()
		 {
			  // given
			  MemberId[] readReplicaIds = MemberIDs( 100 );
			  TopologyService topologyService = FakeTopologyService( fakeCoreTopology( new MemberId( System.Guid.randomUUID() ) ), FakeReadReplicaTopology(readReplicaIds, this.noEastGroupGenerator) );

			  UserDefinedConfigurationStrategy strategy = new UserDefinedConfigurationStrategy();
			  string wantedGroup = _eastGroup;
			  Config config = ConfigWithFilter( "groups(" + wantedGroup + "); halt()" );

			  strategy.Inject( topologyService, config, NullLogProvider.Instance, null );

			  //when

			  Optional<MemberId> memberId = strategy.UpstreamDatabase();

			  // then
			  assertThat( memberId, empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnEmptyIfInvalidFilterSpecification()
		 public virtual void ShouldReturnEmptyIfInvalidFilterSpecification()
		 {
			  // given
			  TopologyService topologyService = FakeTopologyService( fakeCoreTopology( new MemberId( System.Guid.randomUUID() ) ), FakeReadReplicaTopology(MemberIDs(100), this.noEastGroupGenerator) );

			  UserDefinedConfigurationStrategy strategy = new UserDefinedConfigurationStrategy();
			  Config config = ConfigWithFilter( "invalid filter specification" );

			  strategy.Inject( topologyService, config, NullLogProvider.Instance, null );

			  //when

			  Optional<MemberId> memberId = strategy.UpstreamDatabase();

			  // then
			  assertThat( memberId, empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReturnSelf()
		 public virtual void ShouldNotReturnSelf()
		 {
			  // given
			  string wantedGroup = _eastGroup;
			  MemberId[] readReplicaIds = MemberIDs( 1 );
			  TopologyService topologyService = FakeTopologyService( fakeCoreTopology( new MemberId( System.Guid.randomUUID() ) ), FakeReadReplicaTopology(readReplicaIds, memberId => asSet(wantedGroup)) );

			  UserDefinedConfigurationStrategy strategy = new UserDefinedConfigurationStrategy();
			  Config config = ConfigWithFilter( "groups(" + wantedGroup + "); halt()" );

			  strategy.Inject( topologyService, config, NullLogProvider.Instance, readReplicaIds[0] );

			  //when

			  Optional<MemberId> memberId = strategy.UpstreamDatabase();

			  // then
			  assertThat( memberId, empty() );
		 }

		 private Config ConfigWithFilter( string filter )
		 {
			  return Config.defaults( CausalClusteringSettings.user_defined_upstream_selection_strategy, filter );
		 }

		 internal static ReadReplicaTopology FakeReadReplicaTopology( params MemberId[] readReplicaIds )
		 {
			  return FakeReadReplicaTopology( readReplicaIds, ignored => Collections.emptySet() );
		 }

		 internal static ReadReplicaTopology FakeReadReplicaTopology( MemberId[] readReplicaIds, System.Func<MemberId, ISet<string>> groupGenerator )
		 {
			  Debug.Assert( readReplicaIds.Length > 0 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger offset = new java.util.concurrent.atomic.AtomicInteger(10_000);
			  AtomicInteger offset = new AtomicInteger( 10_000 );

			  System.Func<MemberId, ReadReplicaInfo> toReadReplicaInfo = memberId => ReadReplicaInfo( memberId, offset, groupGenerator );

			  IDictionary<MemberId, ReadReplicaInfo> readReplicas = Stream.of( readReplicaIds ).collect( Collectors.toMap( System.Func.identity(), toReadReplicaInfo ) );

			  return new ReadReplicaTopology( readReplicas );
		 }

		 private static ReadReplicaInfo ReadReplicaInfo( MemberId memberId, AtomicInteger offset, System.Func<MemberId, ISet<string>> groupGenerator )
		 {
			  return new ReadReplicaInfo( new ClientConnectorAddresses( singletonList( new ClientConnectorAddresses.ConnectorUri( ClientConnectorAddresses.Scheme.bolt, new AdvertisedSocketAddress( "localhost", offset.AndIncrement ) ) ) ), new AdvertisedSocketAddress( "localhost", offset.AndIncrement ), groupGenerator( memberId ), "default" );
		 }

		 internal static TopologyService FakeTopologyService( CoreTopology coreTopology, ReadReplicaTopology readReplicaTopology )
		 {
			  return new TopologyServiceAnonymousInnerClass( coreTopology, readReplicaTopology );
		 }

		 private class TopologyServiceAnonymousInnerClass : TopologyService
		 {
			 private CoreTopology _coreTopology;
			 private ReadReplicaTopology _readReplicaTopology;

			 public TopologyServiceAnonymousInnerClass( CoreTopology coreTopology, ReadReplicaTopology readReplicaTopology )
			 {
				 this._coreTopology = coreTopology;
				 this._readReplicaTopology = readReplicaTopology;
			 }

			 private IDictionary<MemberId, AdvertisedSocketAddress> catchupAddresses = extractCatchupAddressesMap( _coreTopology, _readReplicaTopology );

			 public CoreTopology allCoreServers()
			 {
				  return _coreTopology;
			 }

			 public CoreTopology localCoreServers()
			 {
				  return _coreTopology; // N.B: not supporting local db!
			 }

			 public ReadReplicaTopology allReadReplicas()
			 {
				  return _readReplicaTopology;
			 }

			 public ReadReplicaTopology localReadReplicas()
			 {
				  return _readReplicaTopology; // N.B: not supporting local db!
			 }

			 public Optional<AdvertisedSocketAddress> findCatchupAddress( MemberId upstream )
			 {
				  return Optional.ofNullable( catchupAddresses.get( upstream ) );
			 }

			 public IDictionary<MemberId, RoleInfo> allCoreRoles()
			 {
				  return emptyMap();
			 }

			 public MemberId myself()
			 {
				  return new MemberId( new System.Guid( 0, 0 ) );
			 }

			 public string localDBName()
			 {
				  return "default";
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void init() throws Throwable
			 public void init()
			 {
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
			 public void start()
			 {
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws Throwable
			 public void stop()
			 {
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown() throws Throwable
			 public void shutdown()
			 {
			 }
		 }

		 internal static MemberId[] MemberIDs( int howMany )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return Stream.generate( () => new MemberId(System.Guid.randomUUID()) ).limit(howMany).toArray(MemberId[]::new);
		 }

		 private readonly string _northGroup = "north";
		 private readonly string _southGroup = "south";
		 private readonly string _westGroup = "west";
		 private readonly string _eastGroup = "east";
		 private IList<string> _noEastGroup;

		 private ISet<string> NoEastGroupGenerator( MemberId memberId )
		 {
			  int index = Math.Abs( memberId.GetHashCode() ) % _noEastGroup.Count;
			  return asSet( _noEastGroup[index] );
		 }
	}

}