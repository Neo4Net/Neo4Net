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
namespace Neo4Net.causalclustering.scenarios
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;
	using Timeout = org.junit.rules.Timeout;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using Role = Neo4Net.causalclustering.core.consensus.roles.Role;
	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;
	using DataCreator = Neo4Net.causalclustering.helpers.DataCreator;
	using StoreId = Neo4Net.causalclustering.identity.StoreId;
	using Node = Neo4Net.GraphDb.Node;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using ClusterRule = Neo4Net.Test.causalclustering.ClusterRule;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.TestStoreId.getStoreIds;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.discovery.Cluster.dataMatchesEventually;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Label.label;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public abstract class BaseMultiClusteringIT
	public abstract class BaseMultiClusteringIT
	{
		 protected internal static ISet<string> DbNames_1 = Collections.singleton( "default" );
		 protected internal static ISet<string> DbNames_2 = Stream.of( "foo", "bar" ).collect( Collectors.toSet() );
		 protected internal static ISet<string> DbNames_3 = Stream.of( "foo", "bar", "baz" ).collect( Collectors.toSet() );

		 private readonly ISet<string> _dbNames;
		 private readonly ClusterRule _clusterRule;
		 private readonly DefaultFileSystemRule _fileSystemRule;
		 private readonly DiscoveryServiceType _discoveryType;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain;
		 public readonly RuleChain RuleChain;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.Neo4Net.causalclustering.discovery.Cluster<?> cluster;
		 private Cluster<object> _cluster;
		 private FileSystemAbstraction _fs;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.Timeout globalTimeout = org.junit.rules.Timeout.seconds(300);
		 public Timeout GlobalTimeout = Timeout.seconds( 300 );

		 protected internal BaseMultiClusteringIT( string ignoredName, int numCores, int numReplicas, ISet<string> dbNames, DiscoveryServiceType discoveryServiceType )
		 {
			  this._dbNames = dbNames;
			  this._discoveryType = discoveryServiceType;

			  this._clusterRule = ( new ClusterRule() ).withNumberOfCoreMembers(numCores).withNumberOfReadReplicas(numReplicas).withDatabaseNames(dbNames);

			  this._fileSystemRule = new DefaultFileSystemRule();

			  this.RuleChain = RuleChain.outerRule( _fileSystemRule ).around( _clusterRule );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _clusterRule.withDiscoveryServiceType( _discoveryType );
			  _fs = _fileSystemRule.get();
			  _cluster = _clusterRule.startCluster();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRunDistinctTransactionsAndDiverge() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRunDistinctTransactionsAndDiverge()
		 {
			  int numNodes = 1;
			  IDictionary<CoreClusterMember, IList<CoreClusterMember>> leaderMap = new Dictionary<CoreClusterMember, IList<CoreClusterMember>>();
			  foreach ( string dbName in _dbNames )
			  {
					int i = 0;
					CoreClusterMember leader;

					do
					{
						 leader = _cluster.coreTx(dbName, (db, tx) =>
						 {
						  Node node = Db.createNode( label( "database" ) );
						  node.setProperty( "name", dbName );
						  tx.success();
						 });
						 i++;
					} while ( i < numNodes );

					int leaderId = leader.ServerId();
					IList<CoreClusterMember> notLeaders = _cluster.coreMembers().Where(m => m.dbName().Equals(dbName) && m.serverId() != leaderId).ToList();

					leaderMap[leader] = notLeaders;
					numNodes++;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  ISet<long> nodesPerDb = leaderMap.Keys.Select( DataCreator.countNodes ).collect( Collectors.toSet() );
			  assertEquals( "Each logical database in the multicluster should have a unique number of nodes.", nodesPerDb.Count, _dbNames.Count );
			  foreach ( KeyValuePair<CoreClusterMember, IList<CoreClusterMember>> subCluster in leaderMap.SetOfKeyValuePairs() )
			  {
					dataMatchesEventually( subCluster.Key, subCluster.Value );
			  }

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void distinctDatabasesShouldHaveDistinctStoreIds() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DistinctDatabasesShouldHaveDistinctStoreIds()
		 {
			  foreach ( string dbName in _dbNames )
			  {
					_cluster.coreTx(dbName, (db, tx) =>
					{
					 Node node = Db.createNode( label( "database" ) );
					 node.setProperty( "name", dbName );
					 tx.success();
					});
			  }

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IList<File> storeDirs = _cluster.coreMembers().Select(CoreClusterMember::databaseDirectory).ToList();

			  _cluster.shutdown();

			  ISet<StoreId> storeIds = getStoreIds( storeDirs, _fs );
			  int expectedNumStoreIds = _dbNames.Count;
			  assertEquals( "Expected distinct store ids for distinct sub clusters.", expectedNumStoreIds, storeIds.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void rejoiningFollowerShouldDownloadSnapshotFromCorrectDatabase() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RejoiningFollowerShouldDownloadSnapshotFromCorrectDatabase()
		 {
			  string dbName = GetFirstDbName( _dbNames );
			  int followerId = _cluster.getMemberWithAnyRole( dbName, Role.FOLLOWER ).serverId();
			  _cluster.removeCoreMemberWithServerId( followerId );

			  for ( int i = 0; i < 100; i++ )
			  {
					_cluster.coreTx(dbName, (db, tx) =>
					{
					 Node node = Db.createNode( label( dbName + "Node" ) );
					 node.setProperty( "name", dbName );
					 tx.success();
					});
			  }

			  foreach ( CoreClusterMember m in _cluster.coreMembers() )
			  {
					m.RaftLogPruner().prune();
			  }

			  _cluster.addCoreMemberWithId( followerId ).start();

			  CoreClusterMember dbLeader = _cluster.awaitLeader( dbName );

			  bool followerIsHealthy = _cluster.healthyCoreMembers().Any(m => m.serverId() == followerId);

			  assertTrue( "Rejoining / lagging follower is expected to be healthy.", followerIsHealthy );

			  CoreClusterMember follower = _cluster.getCoreMemberById( followerId );

			  dataMatchesEventually( dbLeader, Collections.singleton( follower ) );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IList<File> storeDirs = _cluster.coreMembers().Where(m => dbName.Equals(m.dbName())).Select(CoreClusterMember::databaseDirectory).ToList();

			  _cluster.shutdown();

			  ISet<StoreId> storeIds = getStoreIds( storeDirs, _fs );
			  string message = "All members of a sub-cluster should have the same store Id after downloading a snapshot.";
			  assertEquals( message, 1, storeIds.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToChangeClusterMembersDatabaseName() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBeAbleToChangeClusterMembersDatabaseName()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  CoreClusterMember member = _cluster.coreMembers().First().orElseThrow(System.ArgumentException::new);

			  Cluster.shutdownCoreMember( member );

			  //given
			  member.updateConfig( CausalClusteringSettings.database, "new_name" );

			  try
			  {
					//when
					Cluster.startCoreMember( member );
					fail( "Cluster member should fail to restart after database name change." );
			  }
			  catch ( ExecutionException )
			  {
					//expected
			  }
		 }

		 //TODO: Test that rejoining followers wait for majority of hosts *for each database* to be available before joining

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static String getFirstDbName(java.util.Set<String> dbNames) throws Exception
		 private static string GetFirstDbName( ISet<string> dbNames )
		 {
			  return dbNames.First().orElseThrow(() => new System.ArgumentException("The dbNames parameter must not be empty."));
		 }
	}

}