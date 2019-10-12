using System.Collections.Concurrent;
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

	using LeaderInfo = Org.Neo4j.causalclustering.core.consensus.LeaderInfo;
	using ClusterId = Org.Neo4j.causalclustering.identity.ClusterId;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;

	public sealed class SharedDiscoveryService
	{
		 private const int MIN_DISCOVERY_MEMBERS = 2;

		 private readonly ConcurrentMap<MemberId, CoreServerInfo> _coreMembers;
		 private readonly ConcurrentMap<MemberId, ReadReplicaInfo> _readReplicas;
		 private readonly IList<SharedDiscoveryCoreClient> _listeningClients;
		 private readonly ConcurrentMap<string, ClusterId> _clusterIdDbNames;
		 private readonly ConcurrentMap<string, LeaderInfo> _leaderMap;
		 private readonly System.Threading.CountdownEvent _enoughMembers;

		 internal SharedDiscoveryService()
		 {
			  _coreMembers = new ConcurrentDictionary<MemberId, CoreServerInfo>();
			  _readReplicas = new ConcurrentDictionary<MemberId, ReadReplicaInfo>();
			  _listeningClients = new CopyOnWriteArrayList<SharedDiscoveryCoreClient>();
			  _clusterIdDbNames = new ConcurrentDictionary<string, ClusterId>();
			  _leaderMap = new ConcurrentDictionary<string, LeaderInfo>();
			  _enoughMembers = new System.Threading.CountdownEvent( MIN_DISCOVERY_MEMBERS );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void waitForClusterFormation() throws InterruptedException
		 internal void WaitForClusterFormation()
		 {
			  _enoughMembers.await();
		 }

		 private bool CanBeBootstrapped( SharedDiscoveryCoreClient client )
		 {
			  Stream<SharedDiscoveryCoreClient> clientsWhoCanLeadForMyDb = _listeningClients.Where( c => !c.refusesToBeLeader() && c.localDBName().Equals(client.LocalDBName()) );

			  Optional<SharedDiscoveryCoreClient> firstAppropriateClient = clientsWhoCanLeadForMyDb.findFirst();

			  return firstAppropriateClient.map( c => c.Equals( client ) ).orElse( false );
		 }

		 internal CoreTopology GetCoreTopology( SharedDiscoveryCoreClient client )
		 {
			  //Extract config from client
			  string dbName = client.LocalDBName();
			  bool canBeBootstrapped = canBeBootstrapped( client );
			  return GetCoreTopology( dbName, canBeBootstrapped );
		 }

		 internal CoreTopology GetCoreTopology( string dbName, bool canBeBootstrapped )
		 {
			  return new CoreTopology( _clusterIdDbNames.get( dbName ), canBeBootstrapped, Collections.unmodifiableMap( _coreMembers ) );
		 }

		 internal ReadReplicaTopology ReadReplicaTopology
		 {
			 get
			 {
				  return new ReadReplicaTopology( Collections.unmodifiableMap( _readReplicas ) );
			 }
		 }

		 internal void RegisterCoreMember( SharedDiscoveryCoreClient client )
		 {
			  CoreServerInfo previousMember = _coreMembers.putIfAbsent( client.MemberId, client.CoreServerInfo );
			  if ( previousMember == null )
			  {

					_listeningClients.Add( client );
					_enoughMembers.Signal();
					NotifyCoreClients();
			  }
		 }

		 internal void RegisterReadReplica( SharedDiscoveryReadReplicaClient client )
		 {
			  ReadReplicaInfo previousRR = _readReplicas.putIfAbsent( client.MemberId, client.ReadReplicainfo );
			  if ( previousRR == null )
			  {
					NotifyCoreClients();
			  }
		 }

		 internal void UnRegisterCoreMember( SharedDiscoveryCoreClient client )
		 {
			  lock ( this )
			  {
					_listeningClients.Remove( client );
					_coreMembers.remove( client.MemberId );
			  }
			  NotifyCoreClients();
		 }

		 internal void UnRegisterReadReplica( SharedDiscoveryReadReplicaClient client )
		 {
			  _readReplicas.remove( client.MemberId );
			  NotifyCoreClients();
		 }

		 internal void CasLeaders( LeaderInfo leaderInfo, string dbName )
		 {
			  lock ( _leaderMap )
			  {
					Optional<LeaderInfo> current = Optional.ofNullable( _leaderMap.get( dbName ) );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					bool sameLeader = current.map( LeaderInfo::memberId ).Equals( Optional.ofNullable( leaderInfo.MemberId() ) );

					int termComparison = current.map( l => Long.compare( l.term(), leaderInfo.Term() ) ).orElse(-1);

					bool greaterTermExists = termComparison > 0;

					bool sameTermButNoStepDown = termComparison == 0 && !leaderInfo.SteppingDown;

					if ( !( greaterTermExists || sameTermButNoStepDown || sameLeader ) )
					{
						 _leaderMap.put( dbName, leaderInfo );
					}
			  }
		 }

		 internal bool CasClusterId( ClusterId clusterId, string dbName )
		 {
			  ClusterId previousId = _clusterIdDbNames.putIfAbsent( dbName, clusterId );

			  bool success = previousId == null || previousId.Equals( clusterId );

			  if ( success )
			  {
					NotifyCoreClients();
			  }
			  return success;
		 }

		 internal IDictionary<MemberId, RoleInfo> CoreRoles
		 {
			 get
			 {
				  ISet<string> dbNames = _clusterIdDbNames.Keys;
	//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
	//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
				  ISet<MemberId> allLeaders = dbNames.Select( dbName => Optional.ofNullable( _leaderMap.get( dbName ) ) ).Where( Optional.isPresent ).Select( Optional.get ).Select( LeaderInfo::memberId ).collect( Collectors.toSet() );
   
				  System.Func<MemberId, RoleInfo> roleMapper = m => allLeaders.Contains( m ) ? RoleInfo.Leader : RoleInfo.Follower;
				  return _coreMembers.Keys.ToDictionary( System.Func.identity(), roleMapper );
			 }
		 }

		 private void NotifyCoreClients()
		 {
			 lock ( this )
			 {
				  _listeningClients.ForEach(c =>
				  {
				  c.onCoreTopologyChange( GetCoreTopology( c ) );
				  c.onReadReplicaTopologyChange( ReadReplicaTopology );
				  });
			 }
		 }
	}

}