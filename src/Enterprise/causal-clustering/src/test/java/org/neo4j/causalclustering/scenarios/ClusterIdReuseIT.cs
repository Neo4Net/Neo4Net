using System;

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
namespace Neo4Net.causalclustering.scenarios
{
	using MutableLong = org.apache.commons.lang3.mutable.MutableLong;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using CausalClusteringSettings = Neo4Net.causalclustering.core.CausalClusteringSettings;
	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;
	using Node = Neo4Net.Graphdb.Node;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using IdController = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.id.IdController;
	using IdGenerator = Neo4Net.Kernel.impl.store.id.IdGenerator;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using ClusterRule = Neo4Net.Test.causalclustering.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assume.assumeTrue;

	public class ClusterIdReuseIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.causalclustering.ClusterRule clusterRule = new org.neo4j.test.causalclustering.ClusterRule().withNumberOfCoreMembers(3).withSharedCoreParam(org.neo4j.causalclustering.core.CausalClusteringSettings.leader_election_timeout, "2s").withSharedCoreParam(org.neo4j.graphdb.factory.GraphDatabaseSettings.record_id_batch_size, System.Convert.ToString(1)).withNumberOfReadReplicas(0);
		 public readonly ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(3).withSharedCoreParam(CausalClusteringSettings.leader_election_timeout, "2s").withSharedCoreParam(GraphDatabaseSettings.record_id_batch_size, Convert.ToString(1)).withNumberOfReadReplicas(0);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.causalclustering.discovery.Cluster<?> cluster;
		 private Cluster<object> _cluster;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReuseIdsInCluster() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReuseIdsInCluster()
		 {
			  _cluster = ClusterRule.startCluster();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.commons.lang3.mutable.MutableLong first = new org.apache.commons.lang3.mutable.MutableLong();
			  MutableLong first = new MutableLong();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.commons.lang3.mutable.MutableLong second = new org.apache.commons.lang3.mutable.MutableLong();
			  MutableLong second = new MutableLong();

			  CoreClusterMember leader1 = CreateThreeNodes( _cluster, first, second );
			  CoreClusterMember leader2 = RemoveTwoNodes( _cluster, first, second );

			  assumeTrue( leader1 != null && leader1.Equals( leader2 ) );

			  // Force maintenance on leader
			  IdMaintenanceOnLeader( leader1 );
			  IdGeneratorFactory idGeneratorFactory = ResolveDependency( leader1, typeof( IdGeneratorFactory ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.store.id.IdGenerator idGenerator = idGeneratorFactory.get(org.neo4j.kernel.impl.store.id.IdType.NODE);
			  IdGenerator idGenerator = idGeneratorFactory.Get( IdType.NODE );
			  assertEquals( 2, idGenerator.DefragCount );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.commons.lang3.mutable.MutableLong node1id = new org.apache.commons.lang3.mutable.MutableLong();
			  MutableLong node1id = new MutableLong();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.commons.lang3.mutable.MutableLong node2id = new org.apache.commons.lang3.mutable.MutableLong();
			  MutableLong node2id = new MutableLong();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.commons.lang3.mutable.MutableLong node3id = new org.apache.commons.lang3.mutable.MutableLong();
			  MutableLong node3id = new MutableLong();

			  CoreClusterMember clusterMember = _cluster.coreTx((db, tx) =>
			  {
				Node node1 = Db.createNode();
				Node node2 = Db.createNode();
				Node node3 = Db.createNode();

				node1id.Value = node1.Id;
				node2id.Value = node2.Id;
				node3id.Value = node3.Id;

				tx.success();
			  });

			  assumeTrue( leader1.Equals( clusterMember ) );

			  assertEquals( first.longValue(), node1id.longValue() );
			  assertEquals( second.longValue(), node2id.longValue() );
			  assertEquals( idGenerator.HighestPossibleIdInUse, node3id.longValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void newLeaderShouldNotReuseIds() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NewLeaderShouldNotReuseIds()
		 {
			  _cluster = ClusterRule.startCluster();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.commons.lang3.mutable.MutableLong first = new org.apache.commons.lang3.mutable.MutableLong();
			  MutableLong first = new MutableLong();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.commons.lang3.mutable.MutableLong second = new org.apache.commons.lang3.mutable.MutableLong();
			  MutableLong second = new MutableLong();

			  CoreClusterMember creationLeader = CreateThreeNodes( _cluster, first, second );
			  CoreClusterMember deletionLeader = RemoveTwoNodes( _cluster, first, second );

			  // the following assumption is not sufficient for the subsequent assertions, since leadership is a volatile state
			  assumeTrue( creationLeader != null && creationLeader.Equals( deletionLeader ) );

			  IdMaintenanceOnLeader( creationLeader );
			  IdGeneratorFactory idGeneratorFactory = ResolveDependency( creationLeader, typeof( IdGeneratorFactory ) );
			  IdGenerator creationLeaderIdGenerator = idGeneratorFactory.Get( IdType.NODE );
			  assertEquals( 2, creationLeaderIdGenerator.DefragCount );

			  // Force leader switch
			  _cluster.removeCoreMemberWithServerId( creationLeader.ServerId() );

			  // waiting for new leader
			  CoreClusterMember newLeader = _cluster.awaitLeader();
			  assertNotSame( creationLeader.ServerId(), newLeader.ServerId() );
			  IdMaintenanceOnLeader( newLeader );

			  IdGeneratorFactory newLeaderIdGeneratorFactory = ResolveDependency( newLeader, typeof( IdGeneratorFactory ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.store.id.IdGenerator idGenerator = newLeaderIdGeneratorFactory.get(org.neo4j.kernel.impl.store.id.IdType.NODE);
			  IdGenerator idGenerator = newLeaderIdGeneratorFactory.Get( IdType.NODE );
			  assertEquals( 0, idGenerator.DefragCount );

			  CoreClusterMember newCreationLeader = _cluster.coreTx((db, tx) =>
			  {
				Node node = Db.createNode();
				assertEquals( idGenerator.HighestPossibleIdInUse, node.Id );

				tx.success();
			  });
			  assumeTrue( newLeader.Equals( newCreationLeader ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void reusePreviouslyFreedIds() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReusePreviouslyFreedIds()
		 {
			  _cluster = ClusterRule.startCluster();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.commons.lang3.mutable.MutableLong first = new org.apache.commons.lang3.mutable.MutableLong();
			  MutableLong first = new MutableLong();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.commons.lang3.mutable.MutableLong second = new org.apache.commons.lang3.mutable.MutableLong();
			  MutableLong second = new MutableLong();

			  CoreClusterMember creationLeader = CreateThreeNodes( _cluster, first, second );
			  CoreClusterMember deletionLeader = RemoveTwoNodes( _cluster, first, second );

			  assumeTrue( creationLeader != null && creationLeader.Equals( deletionLeader ) );
			  IdGeneratorFactory idGeneratorFactory = ResolveDependency( creationLeader, typeof( IdGeneratorFactory ) );
			  IdMaintenanceOnLeader( creationLeader );
			  IdGenerator creationLeaderIdGenerator = idGeneratorFactory.Get( IdType.NODE );
			  assertEquals( 2, creationLeaderIdGenerator.DefragCount );

			  // Restart and re-elect first leader
			  _cluster.removeCoreMemberWithServerId( creationLeader.ServerId() );
			  _cluster.addCoreMemberWithId( creationLeader.ServerId() ).start();

			  CoreClusterMember leader = _cluster.awaitLeader();
			  while ( leader.ServerId() != creationLeader.ServerId() )
			  {
					_cluster.removeCoreMemberWithServerId( leader.ServerId() );
					_cluster.addCoreMemberWithId( leader.ServerId() ).start();
					leader = _cluster.awaitLeader();
			  }

			  IdMaintenanceOnLeader( leader );
			  IdGeneratorFactory leaderIdGeneratorFactory = ResolveDependency( leader, typeof( IdGeneratorFactory ) );
			  creationLeaderIdGenerator = leaderIdGeneratorFactory.Get( IdType.NODE );
			  assertEquals( 2, creationLeaderIdGenerator.DefragCount );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.commons.lang3.mutable.MutableLong node1id = new org.apache.commons.lang3.mutable.MutableLong();
			  MutableLong node1id = new MutableLong();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.commons.lang3.mutable.MutableLong node2id = new org.apache.commons.lang3.mutable.MutableLong();
			  MutableLong node2id = new MutableLong();
			  CoreClusterMember reuseLeader = _cluster.coreTx((db, tx) =>
			  {
				Node node1 = Db.createNode();
				Node node2 = Db.createNode();

				node1id.Value = node1.Id;
				node2id.Value = node2.Id;

				tx.success();
			  });
			  assumeTrue( leader.Equals( reuseLeader ) );

			  assertEquals( first.longValue(), node1id.longValue() );
			  assertEquals( second.longValue(), node2id.longValue() );
		 }

		 private void IdMaintenanceOnLeader( CoreClusterMember leader )
		 {
			  IdController idController = ResolveDependency( leader, typeof( IdController ) );
			  idController.Maintenance();
		 }

		 private T ResolveDependency<T>( CoreClusterMember leader, Type clazz )
		 {
				 clazz = typeof( T );
			  return leader.Database().DependencyResolver.resolveDependency(clazz);
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.causalclustering.discovery.CoreClusterMember removeTwoNodes(org.neo4j.causalclustering.discovery.Cluster<?> cluster, org.apache.commons.lang3.mutable.MutableLong first, org.apache.commons.lang3.mutable.MutableLong second) throws Exception
		 private CoreClusterMember RemoveTwoNodes<T1>( Cluster<T1> cluster, MutableLong first, MutableLong second )
		 {
			  return cluster.CoreTx((db, tx) =>
			  {
				Node node1 = Db.getNodeById( first.longValue() );
				node1.delete();

				Db.getNodeById( second.longValue() ).delete();

				tx.success();
			  });
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.causalclustering.discovery.CoreClusterMember createThreeNodes(org.neo4j.causalclustering.discovery.Cluster<?> cluster, org.apache.commons.lang3.mutable.MutableLong first, org.apache.commons.lang3.mutable.MutableLong second) throws Exception
		 private CoreClusterMember CreateThreeNodes<T1>( Cluster<T1> cluster, MutableLong first, MutableLong second )
		 {
			  return cluster.CoreTx((db, tx) =>
			  {
				Node node1 = Db.createNode();
				first.Value = node1.Id;

				Node node2 = Db.createNode();
				second.Value = node2.Id;

				Db.createNode();

				tx.success();
			  });
		 }
	}

}