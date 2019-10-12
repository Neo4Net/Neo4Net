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
namespace Org.Neo4j.causalclustering.discovery
{
	using MemberImpl = com.hazelcast.client.impl.MemberImpl;
	using HazelcastInstance = com.hazelcast.core.HazelcastInstance;
	using IAtomicReference = com.hazelcast.core.IAtomicReference;
	using IMap = com.hazelcast.core.IMap;
	using Member = com.hazelcast.core.Member;
	using MultiMap = com.hazelcast.core.MultiMap;
	using Address = com.hazelcast.nio.Address;
	using Matchers = org.hamcrest.Matchers;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using LeaderInfo = Org.Neo4j.causalclustering.core.consensus.LeaderInfo;
	using CausalClusteringTestHelpers = Org.Neo4j.causalclustering.helpers.CausalClusteringTestHelpers;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;
	using CollectorsUtil = Org.Neo4j.Helpers.Collection.CollectorsUtil;
	using Org.Neo4j.Helpers.Collection;
	using BoltConnector = Org.Neo4j.Kernel.configuration.BoltConnector;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using AssertableLogProvider = Org.Neo4j.Logging.AssertableLogProvider;
	using Log = Org.Neo4j.Logging.Log;
	using NullLog = Org.Neo4j.Logging.NullLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.hasItems;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.HazelcastClusterTopology.CLUSTER_UUID_DB_NAME_MAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.HazelcastClusterTopology.DB_NAME_LEADER_TERM_PREFIX;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.HazelcastClusterTopology.READ_REPLICAS_DB_NAME_MAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.HazelcastClusterTopology.READ_REPLICA_BOLT_ADDRESS_MAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.HazelcastClusterTopology.READ_REPLICA_MEMBER_ID_MAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.HazelcastClusterTopology.READ_REPLICA_TRANSACTION_SERVER_ADDRESS_MAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.HazelcastClusterTopology.RR_ATTR_KEYS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.HazelcastClusterTopology.buildMemberAttributesForCore;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.HazelcastClusterTopology.toCoreMemberMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;

	public class HazelcastClusterTopologyTest
	{
		 private static readonly ISet<string> _groups = asSet( "group1", "group2", "group3" );

		 private static readonly ISet<string> _dbNames = Stream.of( "foo", "bar", "baz" ).collect( Collectors.toSet() );
		 private const string DEFAULT_DB_NAME = "default";

		 private static readonly System.Func<int, Dictionary<string, string>> _defaultSettingsGenerator = i =>
		 {
		  Dictionary<string, string> settings = new Dictionary<string, string>();
		  settings.put( CausalClusteringSettings.transaction_advertised_address.name(), "tx:" + (i + 1) );
		  settings.put( CausalClusteringSettings.raft_advertised_address.name(), "raft:" + (i + 1) );
		  settings.put( ( new BoltConnector( "bolt" ) ).type.name(), "BOLT" );
		  settings.put( ( new BoltConnector( "bolt" ) ).enabled.name(), "true" );
		  settings.put( ( new BoltConnector( "bolt" ) ).advertised_address.name(), "bolt:" + (i + 1) );
		  settings.put( ( new BoltConnector( "http" ) ).type.name(), "HTTP" );
		  settings.put( ( new BoltConnector( "http" ) ).enabled.name(), "true" );
		  settings.put( ( new BoltConnector( "http" ) ).advertised_address.name(), "http:" + (i + 1) );

		  return settings;
		 };

		 private readonly HazelcastInstance _hzInstance = mock( typeof( HazelcastInstance ) );
		 private IDictionary<string, IMap<string, string>> _rrAttributeMaps;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") com.hazelcast.core.MultiMap<String,String> serverGroupsMMap = mock(com.hazelcast.core.MultiMap.class);
			  MultiMap<string, string> serverGroupsMMap = mock( typeof( MultiMap ) );
			  when( serverGroupsMMap.get( any() ) ).thenReturn(_groups);
			  when( _hzInstance.getMultiMap( anyString() ) ).thenReturn((MultiMap) serverGroupsMMap);
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  _rrAttributeMaps = RR_ATTR_KEYS.Select( k => Pair.of( k, ( IMap<string, string> ) mock( typeof( IMap ) ) ) ).collect( CollectorsUtil.pairsToMap() );
		 }

		 private static IList<Config> GenerateConfigs( int numConfigs )
		 {
			  return GenerateConfigs( numConfigs, _defaultSettingsGenerator );
		 }

		 private static IList<Config> GenerateConfigs( int numConfigs, System.Func<int, Dictionary<string, string>> generator )
		 {
			  return IntStream.range( 0, numConfigs ).mapToObj( generator ).map( Config.defaults ).collect( Collectors.toList() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCollectReadReplicasAsMap()
		 public virtual void ShouldCollectReadReplicasAsMap()
		 {
			  // given
			  MemberId memberId = new MemberId( System.Guid.randomUUID() );
			  ReadReplicaInfo readReplicaInfo = GenerateReadReplicaInfo();
			  IDictionary<MemberId, ReadReplicaInfo> mockedRRs = singletonMap( memberId, readReplicaInfo );
			  MockReadReplicaAttributes( mockedRRs );

			  // when
			  IDictionary<MemberId, ReadReplicaInfo> rrMap = HazelcastClusterTopology.ReadReplicas( _hzInstance, NullLog.Instance );

			  // then
			  assertEquals( mockedRRs, rrMap );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldValidateNullReadReplicaAttrMaps()
		 public virtual void ShouldValidateNullReadReplicaAttrMaps()
		 {
			  // given
			  MemberId memberId = new MemberId( System.Guid.randomUUID() );
			  ReadReplicaInfo readReplicaInfo = GenerateReadReplicaInfo();
			  MockReadReplicaAttributes( singletonMap( memberId, readReplicaInfo ), singleton( READ_REPLICAS_DB_NAME_MAP ), emptyMap() );

			  // when
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  Log log = logProvider.getLog( this.GetType() );
			  IDictionary<MemberId, ReadReplicaInfo> rrMap = HazelcastClusterTopology.ReadReplicas( _hzInstance, log );

			  // then
			  assertEquals( emptyMap(), rrMap );
			  logProvider.FormattedMessageMatcher().assertContains("Some, but not all, of the read replica attribute maps are null");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldValidateReadReplicaAttrMapNullValues()
		 public virtual void ShouldValidateReadReplicaAttrMapNullValues()
		 {
			  // given
			  IDictionary<MemberId, ReadReplicaInfo> mockedRRs = new Dictionary<MemberId, ReadReplicaInfo>();

			  MemberId validMemberId = new MemberId( System.Guid.randomUUID() );
			  MemberId invalidMemberId = new MemberId( System.Guid.randomUUID() );
			  ReadReplicaInfo validReadReplicaInfo = GenerateReadReplicaInfo();
			  ReadReplicaInfo invalidReadReplicaInfo = GenerateReadReplicaInfo();

			  mockedRRs[validMemberId] = validReadReplicaInfo;
			  mockedRRs[invalidMemberId] = invalidReadReplicaInfo;

			  IDictionary<MemberId, ISet<string>> nullAttrValues = singletonMap( invalidMemberId, singleton( READ_REPLICA_TRANSACTION_SERVER_ADDRESS_MAP ) );

			  MockReadReplicaAttributes( mockedRRs, emptySet(), nullAttrValues );

			  // when
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  Log log = logProvider.getLog( this.GetType() );
			  IDictionary<MemberId, ReadReplicaInfo> rrMap = HazelcastClusterTopology.ReadReplicas( _hzInstance, log );

			  // then
			  assertEquals( singletonMap( validMemberId, validReadReplicaInfo ), rrMap );

			  logProvider.RawMessageMatcher().assertContains(Matchers.allOf(Matchers.containsString("Missing attribute %s for read replica")));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCollectMembersAsAMap() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCollectMembersAsAMap()
		 {
			  // given
			  int numMembers = 5;
			  ISet<Member> hazelcastMembers = new HashSet<Member>();
			  IList<MemberId> coreMembers = new List<MemberId>();

			  IList<Config> configs = GenerateConfigs( numMembers );

			  for ( int i = 0; i < configs.Count; i++ )
			  {
					MemberId mId = new MemberId( System.Guid.randomUUID() );
					coreMembers.Add( mId );

					Config c = configs[i];
					IDictionary<string, object> attributes = buildMemberAttributesForCore( mId, c ).Attributes;
					hazelcastMembers.Add( new MemberImpl( new Address( "localhost", i ), null, attributes, false ) );
			  }

			  // when
			  IDictionary<MemberId, CoreServerInfo> coreMemberMap = toCoreMemberMap( hazelcastMembers, NullLog.Instance, _hzInstance );

			  // then
			  for ( int i = 0; i < numMembers; i++ )
			  {
					CoreServerInfo coreServerInfo = coreMemberMap[coreMembers[i]];
					assertEquals( new AdvertisedSocketAddress( "tx", i + 1 ), coreServerInfo.CatchupServer );
					assertEquals( new AdvertisedSocketAddress( "raft", i + 1 ), coreServerInfo.RaftServer );
					assertEquals( new AdvertisedSocketAddress( "bolt", i + 1 ), coreServerInfo.Connectors().boltAddress() );
					assertEquals( coreServerInfo.DatabaseName, DEFAULT_DB_NAME );
					assertEquals( coreServerInfo.Groups(), _groups );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBuildMemberAttributedWithSpecifiedDBNames() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBuildMemberAttributedWithSpecifiedDBNames()
		 {
			  //given
			  int numMembers = 10;
			  ISet<Member> hazelcastMembers = new HashSet<Member>();
			  IList<MemberId> coreMembers = new List<MemberId>();

			  IDictionary<int, string> dbNames = CausalClusteringTestHelpers.distributeDatabaseNamesToHostNums( numMembers, _dbNames );
			  System.Func<int, Dictionary<string, string>> generator = i =>
			  {
				Dictionary<string, string> settings = _defaultSettingsGenerator.apply( i );
				settings.put( CausalClusteringSettings.database.name(), dbNames[i] );
				return settings;
			  };

			  IList<Config> configs = GenerateConfigs( numMembers, generator );

			  for ( int i = 0; i < configs.Count; i++ )
			  {
					MemberId mId = new MemberId( System.Guid.randomUUID() );
					coreMembers.Add( mId );

					Config c = configs[i];
					IDictionary<string, object> attributes = buildMemberAttributesForCore( mId, c ).Attributes;
					hazelcastMembers.Add( new MemberImpl( new Address( "localhost", i ), null, attributes, false ) );
			  }

			  // when
			  IDictionary<MemberId, CoreServerInfo> coreMemberMap = toCoreMemberMap( hazelcastMembers, NullLog.Instance, _hzInstance );

			  // then
			  for ( int i = 0; i < numMembers; i++ )
			  {
					CoreServerInfo coreServerInfo = coreMemberMap[coreMembers[i]];
					string expectedDBName = dbNames[i];
					assertEquals( expectedDBName, coreServerInfo.DatabaseName );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogAndExcludeMembersWithMissingAttributes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogAndExcludeMembersWithMissingAttributes()
		 {
			  // given
			  int numMembers = 4;
			  ISet<Member> hazelcastMembers = new HashSet<Member>();
			  IList<MemberId> coreMembers = new List<MemberId>();

			  System.Func<int, Dictionary<string, string>> generator = i =>
			  {
				Dictionary<string, string> settings = _defaultSettingsGenerator.apply( i );
				settings.remove( CausalClusteringSettings.transaction_advertised_address.name() );
				settings.remove( CausalClusteringSettings.raft_advertised_address.name() );
				return settings;
			  };

			  IList<Config> configs = GenerateConfigs( numMembers, generator );

			  for ( int i = 0; i < configs.Count; i++ )
			  {
					MemberId memberId = new MemberId( System.Guid.randomUUID() );
					coreMembers.Add( memberId );
					Config c = configs[i];
					IDictionary<string, object> attributes = buildMemberAttributesForCore( memberId, c ).Attributes;
					if ( i == 2 )
					{
						 attributes.Remove( HazelcastClusterTopology.RAFT_SERVER );
					}
					hazelcastMembers.Add( new MemberImpl( new Address( "localhost", i ), null, attributes, false ) );
			  }

			  // when
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  Log log = logProvider.getLog( this.GetType() );
			  IDictionary<MemberId, CoreServerInfo> map = toCoreMemberMap( hazelcastMembers, log, _hzInstance );

			  // then
			  assertThat( map.Keys, hasItems( coreMembers[0], coreMembers[1], coreMembers[3] ) );
			  assertThat( map.Keys, not( hasItems( coreMembers[2] ) ) );
			  logProvider.FormattedMessageMatcher().assertContains("Missing member attribute");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCorrectlyReturnCoreMemberRoles()
		 public virtual void ShouldCorrectlyReturnCoreMemberRoles()
		 {
			  //given
			  int numMembers = 3;

			  IList<MemberId> members = IntStream.range( 0, numMembers ).mapToObj( ignored => new MemberId( System.Guid.randomUUID() ) ).collect(Collectors.toList());

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") com.hazelcast.core.IAtomicReference<org.neo4j.causalclustering.core.consensus.LeaderInfo> leaderRef = mock(com.hazelcast.core.IAtomicReference.class);
			  IAtomicReference<LeaderInfo> leaderRef = mock( typeof( IAtomicReference ) );
			  MemberId chosenLeaderId = members[0];
			  when( leaderRef.get() ).thenReturn(new LeaderInfo(chosenLeaderId, 0L));

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") com.hazelcast.core.IMap<String,java.util.UUID> uuidDBMap = mock(com.hazelcast.core.IMap.class);
			  IMap<string, System.Guid> uuidDBMap = mock( typeof( IMap ) );
			  when( uuidDBMap.Keys ).thenReturn( Collections.singleton( DEFAULT_DB_NAME ) );
			  when( _hzInstance.getAtomicReference<LeaderInfo>( startsWith( DB_NAME_LEADER_TERM_PREFIX ) ) ).thenReturn( leaderRef );
			  when( _hzInstance.getMap<string, System.Guid>( eq( CLUSTER_UUID_DB_NAME_MAP ) ) ).thenReturn( uuidDBMap );

			  // when
			  IDictionary<MemberId, RoleInfo> roleMap = HazelcastClusterTopology.GetCoreRoles( _hzInstance, new HashSet<MemberId, RoleInfo>( members ) );

			  // then
			  assertEquals( "First member was expected to be leader.", RoleInfo.Leader, roleMap[chosenLeaderId] );
		 }

		 private void MockReadReplicaAttributes( IDictionary<MemberId, ReadReplicaInfo> readReplicaInfos )
		 {
			  MockReadReplicaAttributes( readReplicaInfos, emptySet(), emptyMap() );
		 }

		 private void MockReadReplicaAttributes( IDictionary<MemberId, ReadReplicaInfo> readReplicaInfos, ISet<string> missingAttrsMaps, IDictionary<MemberId, ISet<string>> nullAttrs )
		 {
			  ISet<string> hzIds = new HashSet<string>();
			  readReplicaInfos.forEach((memberId, readReplicaInfo) =>
			  {
												  System.Guid hzId = System.Guid.randomUUID();
												  hzIds.Add( hzId.ToString() );
												  GenerateReadReplicaAttributes( hzId, memberId, readReplicaInfo, missingAttrsMaps, nullAttrs.getOrDefault( memberId, emptySet() ) );
			  });
			  _rrAttributeMaps.forEach( ( ignored, attrs ) => when( attrs.Keys ).thenReturn( hzIds ) );
		 }

		 private void GenerateReadReplicaAttributes( System.Guid hzId, MemberId memberId, ReadReplicaInfo readReplicaInfo, ISet<string> missingAttrsMaps, ISet<string> nullAttrs )
		 {
			  IDictionary<string, System.Func<MemberId, ReadReplicaInfo, string>> attributeFactories = new Dictionary<string, System.Func<MemberId, ReadReplicaInfo, string>>();
			  attributeFactories[READ_REPLICAS_DB_NAME_MAP] = ( ignored, rr ) => rr.DatabaseName;
			  attributeFactories[READ_REPLICA_TRANSACTION_SERVER_ADDRESS_MAP] = ( ignored, rr ) => rr.CatchupServer.ToString();
			  attributeFactories[READ_REPLICA_MEMBER_ID_MAP] = ( mId, ignored ) => mId.Uuid.ToString();
			  attributeFactories[READ_REPLICA_BOLT_ADDRESS_MAP] = ( ignored, rr ) => rr.connectors().ToString();

			  attributeFactories.SetOfKeyValuePairs().Where(e => !missingAttrsMaps.Contains(e.Key)).ForEach(e =>
			  {
													 string attrValue = nullAttrs.Contains( e.Key ) ? null : e.Value.apply( memberId, readReplicaInfo );
													 MockReadReplicaAttribute( e.Key, hzId, attrValue );
			  });
		 }

		 private void MockReadReplicaAttribute( string attrKey, System.Guid hzId, string attrValue )
		 {
			  IMap<string, string> attrs = _rrAttributeMaps[attrKey];
			  when( attrs.get( hzId.ToString() ) ).thenReturn(attrValue);
			  when( _hzInstance.getMap<string, string>( attrKey ) ).thenReturn( attrs );
		 }

		 private ReadReplicaInfo GenerateReadReplicaInfo()
		 {
			  System.Func<int> portFactory = () => ThreadLocalRandom.current().Next(1000, 10000);

			  IList<ClientConnectorAddresses.ConnectorUri> connectorUris = singletonList( new ClientConnectorAddresses.ConnectorUri( ClientConnectorAddresses.Scheme.Bolt, new AdvertisedSocketAddress( "losthost", portFactory() ) ) );
			  ClientConnectorAddresses addresses = new ClientConnectorAddresses( connectorUris );
			  return new ReadReplicaInfo( addresses, new AdvertisedSocketAddress( "localhost", portFactory() ), _groups, "foo" );
		 }
	}

}