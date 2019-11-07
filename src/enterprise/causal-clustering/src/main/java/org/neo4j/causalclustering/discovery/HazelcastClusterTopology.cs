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
namespace Neo4Net.causalclustering.discovery
{
	using MemberAttributeConfig = com.hazelcast.config.MemberAttributeConfig;
	using HazelcastInstance = com.hazelcast.core.HazelcastInstance;
	using IAtomicReference = com.hazelcast.core.IAtomicReference;
	using IMap = com.hazelcast.core.IMap;
	using Member = com.hazelcast.core.Member;
	using MultiMap = com.hazelcast.core.MultiMap;


	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using LeaderInfo = Neo4Net.causalclustering.core.consensus.LeaderInfo;
	using ClusterId = Neo4Net.causalclustering.identity.ClusterId;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using CollectorsUtil = Neo4Net.Collections.Helpers.CollectorsUtil;
	using Neo4Net.Collections.Helpers;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Log = Neo4Net.Logging.Log;
	using Streams = Neo4Net.Stream.Streams;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.CausalClusteringSettings.refuse_to_be_leader;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.SocketAddressParser.socketAddress;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterables.asSet;

	public sealed class HazelcastClusterTopology
	{
		 // per server attributes
		 private const string DISCOVERY_SERVER = "discovery_server"; // not currently used
		 internal const string MEMBER_UUID = "member_uuid";
		 internal const string TRANSACTION_SERVER = "transaction_server";
		 internal const string RAFT_SERVER = "raft_server";
		 internal const string CLIENT_CONNECTOR_ADDRESSES = "client_connector_addresses";
		 internal const string MEMBER_DB_NAME = "member_database_name";

		 private const string REFUSE_TO_BE_LEADER_KEY = "refuseToBeLeader";

		 // cluster-wide attributes
		 internal const string CLUSTER_UUID_DB_NAME_MAP = "cluster_uuid";
		 internal const string SERVER_GROUPS_MULTIMAP = "groups";
		 internal const string READ_REPLICA_TRANSACTION_SERVER_ADDRESS_MAP = "read-replica-transaction-servers";
		 internal const string READ_REPLICA_BOLT_ADDRESS_MAP = "read_replicas"; // hz client uuid string -> boltAddress string
		 internal const string READ_REPLICA_MEMBER_ID_MAP = "read-replica-member-ids";
		 internal const string READ_REPLICAS_DB_NAME_MAP = "read_replicas_database_names";
		 internal const string DB_NAME_LEADER_TERM_PREFIX = "leader_term_for_database_name_";

		 // the attributes used for reconstructing read replica information
		 internal static readonly ISet<string> RrAttrKeys = Stream.of( READ_REPLICA_BOLT_ADDRESS_MAP, READ_REPLICA_TRANSACTION_SERVER_ADDRESS_MAP, READ_REPLICA_MEMBER_ID_MAP, READ_REPLICAS_DB_NAME_MAP ).collect( Collectors.toSet() );

		 // the attributes used for reconstructing core member information
		 internal static readonly ISet<string> CoreAttrKeys = Stream.of( MEMBER_UUID, RAFT_SERVER, TRANSACTION_SERVER, CLIENT_CONNECTOR_ADDRESSES, MEMBER_DB_NAME ).collect( Collectors.toSet() );

		 private HazelcastClusterTopology()
		 {
		 }

		 internal static ReadReplicaTopology GetReadReplicaTopology( HazelcastInstance hazelcastInstance, Log log )
		 {
			  IDictionary<MemberId, ReadReplicaInfo> readReplicas = emptyMap();

			  if ( hazelcastInstance != null )
			  {
					readReplicas = readReplicas( hazelcastInstance, log );
			  }
			  else
			  {
					log.Info( "Cannot currently bind to distributed discovery service." );
			  }

			  return new ReadReplicaTopology( readReplicas );
		 }

		 internal static CoreTopology GetCoreTopology( HazelcastInstance hazelcastInstance, Config config, Log log )
		 {
			  IDictionary<MemberId, CoreServerInfo> coreMembers = emptyMap();
			  bool canBeBootstrapped = false;
			  ClusterId clusterId = null;
			  string dbName = config.Get( CausalClusteringSettings.database );

			  if ( hazelcastInstance != null )
			  {
					ISet<Member> hzMembers = hazelcastInstance.Cluster.Members;
					canBeBootstrapped = canBeBootstrapped( hazelcastInstance, config );

					coreMembers = ToCoreMemberMap( hzMembers, log, hazelcastInstance );

					clusterId = GetClusterId( hazelcastInstance, dbName );
			  }
			  else
			  {
					log.Info( "Cannot currently bind to distributed discovery service." );
			  }

			  return new CoreTopology( clusterId, canBeBootstrapped, coreMembers );
		 }

		 public static IDictionary<MemberId, AdvertisedSocketAddress> ExtractCatchupAddressesMap( CoreTopology coreTopology, ReadReplicaTopology rrTopology )
		 {
			  IDictionary<MemberId, AdvertisedSocketAddress> catchupAddressMap = new Dictionary<MemberId, AdvertisedSocketAddress>();

			  foreach ( KeyValuePair<MemberId, CoreServerInfo> entry in coreTopology.Members().SetOfKeyValuePairs() )
			  {
					catchupAddressMap[entry.Key] = entry.Value.CatchupServer;
			  }

			  foreach ( KeyValuePair<MemberId, ReadReplicaInfo> entry in rrTopology.Members().SetOfKeyValuePairs() )
			  {
					catchupAddressMap[entry.Key] = entry.Value.CatchupServer;

			  }

			  return catchupAddressMap;
		 }

		 private static ClusterId GetClusterId( HazelcastInstance hazelcastInstance, string dbName )
		 {
			  IMap<string, System.Guid> uuidPerDbCluster = hazelcastInstance.getMap( CLUSTER_UUID_DB_NAME_MAP );
			  System.Guid uuid = uuidPerDbCluster.get( dbName );
			  return uuid != null ? new ClusterId( uuid ) : null;
		 }

		 private static ISet<string> GetDBNames( HazelcastInstance hazelcastInstance )
		 {
			  IMap<string, System.Guid> uuidPerDbCluster = hazelcastInstance.getMap( CLUSTER_UUID_DB_NAME_MAP );
			  return uuidPerDbCluster.Keys;
		 }

		 public static IDictionary<MemberId, RoleInfo> GetCoreRoles( HazelcastInstance hazelcastInstance, ISet<MemberId> coreMembers )
		 {

			  ISet<string> dbNames = GetDBNames( hazelcastInstance );
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  ISet<MemberId> allLeaders = dbNames.Select( n => GetLeaderForDBName( hazelcastInstance, n ) ).Where( Optional.isPresent ).Select( l => l.get().memberId() ).collect(Collectors.toSet());

			  System.Func<MemberId, RoleInfo> roleMapper = m => allLeaders.Contains( m ) ? RoleInfo.Leader : RoleInfo.Follower;

			  return coreMembers.ToDictionary( System.Func.identity(), roleMapper );
		 }

		 internal static bool CasClusterId( HazelcastInstance hazelcastInstance, ClusterId clusterId, string dbName )
		 {
			  IMap<string, System.Guid> uuidPerDbCluster = hazelcastInstance.getMap( CLUSTER_UUID_DB_NAME_MAP );
			  System.Guid uuid = uuidPerDbCluster.putIfAbsent( dbName, clusterId.Uuid() );
			  return uuid == null || clusterId.Uuid().Equals(uuid);
		 }

		 internal static IDictionary<MemberId, ReadReplicaInfo> ReadReplicas( HazelcastInstance hazelcastInstance, Log log )
		 {
			  Pair<ISet<string>, IDictionary<string, IMap<string, string>>> validatedSimpleAttrMaps = validatedSimpleAttrMaps( hazelcastInstance );
			  ISet<string> missingAttrKeys = validatedSimpleAttrMaps.First();
			  IDictionary<string, IMap<string, string>> simpleAttrMaps = validatedSimpleAttrMaps.Other();

			  MultiMap<string, string> serverGroupsMap = hazelcastInstance.getMultiMap( SERVER_GROUPS_MULTIMAP );

			  if ( serverGroupsMap == null )
			  {
					missingAttrKeys.Add( SERVER_GROUPS_MULTIMAP );
			  }

			  if ( missingAttrKeys.Count > 0 )
			  {
					// We might well not have any read replicas, in which case missing maps is not an error, but we *can't* have some maps and not others
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'containsAll' method:
					bool missingAllKeys = missingAttrKeys.containsAll( RrAttrKeys ) && missingAttrKeys.Contains( SERVER_GROUPS_MULTIMAP );
					if ( !missingAllKeys )
					{
						 string missingAttrs = string.join( ", ", missingAttrKeys );
						 log.Warn( "Some, but not all, of the read replica attribute maps are null, including %s", missingAttrs );
					}

					return emptyMap();
			  }

			  Stream<string> readReplicaHzIds = simpleAttrMaps[READ_REPLICA_BOLT_ADDRESS_MAP].Keys.stream();

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IDictionary<MemberId, ReadReplicaInfo> validatedReadReplicas = readReplicaHzIds.flatMap( hzId => Streams.ofNullable( BuildReadReplicaFromAttrMap( hzId, simpleAttrMaps, serverGroupsMap, log ) ) ).collect( Collectors.toMap( Pair::first, Pair::other ) );

			  return validatedReadReplicas;
		 }

		 /// <summary>
		 /// Retrieves the various maps containing attributes about read replicas from hazelcast. If any maps do not exist, keep track of their keys for logging.
		 /// </summary>
		 private static Pair<ISet<string>, IDictionary<string, IMap<string, string>>> ValidatedSimpleAttrMaps( HazelcastInstance hazelcastInstance )
		 {
			  ISet<string> missingAttrKeys = new HashSet<string>();
			  IDictionary<string, IMap<string, string>> validatedSimpleAttrMaps = new Dictionary<string, IMap<string, string>>();

			  foreach ( string attrMapKey in RrAttrKeys )
			  {
					IMap<string, string> attrMap = hazelcastInstance.getMap( attrMapKey );
					if ( attrMap == null )
					{
						 missingAttrKeys.Add( attrMapKey );
					}
					else
					{
						 validatedSimpleAttrMaps[attrMapKey] = attrMap;
					}
			  }

			  return Pair.of( missingAttrKeys, validatedSimpleAttrMaps );
		 }

		 /// <summary>
		 /// Given a hazelcast member id and a set of non-null attribute maps, this method builds a discovery representation of a read replica
		 /// (i.e. `Pair<MemberId,ReadReplicaInfo>`). Any missing attributes which are missing for a given hazelcast member id are logged and this
		 /// method will return null.
		 /// </summary>
		 private static Pair<MemberId, ReadReplicaInfo> BuildReadReplicaFromAttrMap( string hzId, IDictionary<string, IMap<string, string>> simpleAttrMaps, MultiMap<string, string> serverGroupsMap, Log log )
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  IDictionary<string, string> memberAttrs = simpleAttrMaps.SetOfKeyValuePairs().Select(e => Pair.of(e.Key, e.Value.get(hzId))).Where(p => HasAttribute(p, hzId, log)).collect(CollectorsUtil.pairsToMap());

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'containsAll' method:
			  if ( !memberAttrs.Keys.containsAll( RrAttrKeys ) )
			  {
					return null;
			  }

			  ICollection<string> memberServerGroups = serverGroupsMap.get( hzId );
			  if ( memberServerGroups == null )
			  {
					log.Warn( "Missing attribute %s for read replica with hz id %s", SERVER_GROUPS_MULTIMAP, hzId );
					return null;
			  }

			  ClientConnectorAddresses boltAddresses = ClientConnectorAddresses.FromString( memberAttrs[READ_REPLICA_BOLT_ADDRESS_MAP] );
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  AdvertisedSocketAddress catchupAddress = socketAddress( memberAttrs[READ_REPLICA_TRANSACTION_SERVER_ADDRESS_MAP], AdvertisedSocketAddress::new );
			  MemberId memberId = new MemberId( System.Guid.Parse( memberAttrs[READ_REPLICA_MEMBER_ID_MAP] ) );
			  string memberDbName = memberAttrs[READ_REPLICAS_DB_NAME_MAP];
			  ISet<string> serverGroupSet = asSet( memberServerGroups );

			  ReadReplicaInfo rrInfo = new ReadReplicaInfo( boltAddresses, catchupAddress, serverGroupSet, memberDbName );
			  return Pair.of( memberId, rrInfo );
		 }

		 private static bool HasAttribute( Pair<string, string> memberAttr, string hzId, Log log )
		 {
			  if ( string.ReferenceEquals( memberAttr.Other(), null ) )
			  {
					log.Warn( "Missing attribute %s for read replica with hz id %s", memberAttr.First(), hzId );
					return false;
			  }
			  return true;
		 }

		 internal static void CasLeaders( HazelcastInstance hazelcastInstance, LeaderInfo leaderInfo, string dbName, Log log )
		 {
			  IAtomicReference<LeaderInfo> leaderRef = hazelcastInstance.getAtomicReference( DB_NAME_LEADER_TERM_PREFIX + dbName );

			  LeaderInfo current = leaderRef.get();
			  Optional<LeaderInfo> currentOpt = Optional.ofNullable( current );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  bool sameLeader = currentOpt.map( LeaderInfo::memberId ).Equals( Optional.ofNullable( leaderInfo.MemberId() ) );

			  int termComparison = currentOpt.map( l => Long.compare( l.term(), leaderInfo.Term() ) ).orElse(-1);

			  bool greaterTermExists = termComparison > 0;

			  bool sameTermButNoStepdown = termComparison == 0 && !leaderInfo.SteppingDown;

			  if ( sameLeader || greaterTermExists || sameTermButNoStepdown )
			  {
					return;
			  }

			  bool success = leaderRef.compareAndSet( current, leaderInfo );
			  if ( !success )
			  {
					log.Warn( "Fail to set new leader info: %s. Latest leader info: %s.", leaderInfo, leaderRef.get() );
			  }
		 }

		 private static Optional<LeaderInfo> GetLeaderForDBName( HazelcastInstance hazelcastInstance, string dbName )
		 {
			  IAtomicReference<LeaderInfo> leader = hazelcastInstance.getAtomicReference( DB_NAME_LEADER_TERM_PREFIX + dbName );
			  return Optional.ofNullable( leader.get() );
		 }

		 private static bool CanBeBootstrapped( HazelcastInstance hazelcastInstance, Config config )
		 {
			  ISet<Member> members = hazelcastInstance.Cluster.Members;
			  string dbName = config.Get( CausalClusteringSettings.database );

			  System.Predicate<Member> acceptsToBeLeader = m => !m.getBooleanAttribute( REFUSE_TO_BE_LEADER_KEY );
			  System.Predicate<Member> hostsMyDb = m => dbName.Equals( m.getStringAttribute( MEMBER_DB_NAME ) );

			  Stream<Member> membersWhoCanLeadForMyDb = members.Where( acceptsToBeLeader ).Where( hostsMyDb );

			  Optional<Member> firstAppropriateMember = membersWhoCanLeadForMyDb.findFirst();

			  return firstAppropriateMember.map( Member.localMember ).orElse( false );
		 }

		 internal static IDictionary<MemberId, CoreServerInfo> ToCoreMemberMap( ISet<Member> members, Log log, HazelcastInstance hazelcastInstance )
		 {
			  IDictionary<MemberId, CoreServerInfo> coreMembers = new Dictionary<MemberId, CoreServerInfo>();
			  MultiMap<string, string> serverGroupsMMap = hazelcastInstance.getMultiMap( SERVER_GROUPS_MULTIMAP );

			  foreach ( Member member in members )
			  {
					IDictionary<string, string> attrMap = new Dictionary<string, string>();
					bool incomplete = false;
					foreach ( string attrKey in CoreAttrKeys )
					{
						 string attrValue = member.getStringAttribute( attrKey );
						 if ( string.ReferenceEquals( attrValue, null ) )
						 {
							  log.Warn( "Missing member attribute '%s' for member %s", attrKey, member );
							  incomplete = true;
						 }
						 else
						 {
							  attrMap[attrKey] = attrValue;
						 }
					}

					if ( incomplete )
					{
						 continue;
					}

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					CoreServerInfo coreServerInfo = new CoreServerInfo( socketAddress( attrMap[RAFT_SERVER], AdvertisedSocketAddress::new ), socketAddress( attrMap[TRANSACTION_SERVER], AdvertisedSocketAddress::new ), ClientConnectorAddresses.FromString( attrMap[CLIENT_CONNECTOR_ADDRESSES] ), asSet( serverGroupsMMap.get( attrMap[MEMBER_UUID] ) ), attrMap[MEMBER_DB_NAME], member.getBooleanAttribute( REFUSE_TO_BE_LEADER_KEY ) );

					MemberId memberId = new MemberId( System.Guid.Parse( attrMap[MEMBER_UUID] ) );
					coreMembers[memberId] = coreServerInfo;
			  }

			  return coreMembers;
		 }

		 internal static void RefreshGroups( HazelcastInstance hazelcastInstance, string memberId, IList<string> groups )
		 {
			  MultiMap<string, string> groupsMap = hazelcastInstance.getMultiMap( SERVER_GROUPS_MULTIMAP );
			  ICollection<string> existing = groupsMap.get( memberId );

//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  ISet<string> superfluous = existing.Where( t => !groups.Contains( t ) ).collect( Collectors.toSet() );
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  ISet<string> missing = groups.Where( t => !existing.Contains( t ) ).collect( Collectors.toSet() );

			  missing.forEach( group => groupsMap.put( memberId, group ) );
			  superfluous.forEach( group => groupsMap.remove( memberId, group ) );
		 }

		 internal static MemberAttributeConfig BuildMemberAttributesForCore( MemberId myself, Config config )
		 {

			  MemberAttributeConfig memberAttributeConfig = new MemberAttributeConfig();
			  memberAttributeConfig.setStringAttribute( MEMBER_UUID, myself.Uuid.ToString() );

			  AdvertisedSocketAddress discoveryAddress = config.Get( CausalClusteringSettings.discovery_advertised_address );
			  memberAttributeConfig.setStringAttribute( DISCOVERY_SERVER, discoveryAddress.ToString() );

			  AdvertisedSocketAddress transactionSource = config.Get( CausalClusteringSettings.transaction_advertised_address );
			  memberAttributeConfig.setStringAttribute( TRANSACTION_SERVER, transactionSource.ToString() );

			  AdvertisedSocketAddress raftAddress = config.Get( CausalClusteringSettings.raft_advertised_address );
			  memberAttributeConfig.setStringAttribute( RAFT_SERVER, raftAddress.ToString() );

			  ClientConnectorAddresses clientConnectorAddresses = ClientConnectorAddresses.ExtractFromConfig( config );
			  memberAttributeConfig.setStringAttribute( CLIENT_CONNECTOR_ADDRESSES, clientConnectorAddresses.ToString() );

			  memberAttributeConfig.setBooleanAttribute( REFUSE_TO_BE_LEADER_KEY, config.Get( refuse_to_be_leader ) );

			  memberAttributeConfig.setStringAttribute( MEMBER_DB_NAME, config.Get( CausalClusteringSettings.database ) );

			  return memberAttributeConfig;
		 }
	}

}