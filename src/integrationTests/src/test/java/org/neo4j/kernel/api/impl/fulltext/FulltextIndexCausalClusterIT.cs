using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Kernel.Api.Impl.Fulltext
{
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Neo4Net.causalclustering.discovery;
	using Neo4Net.causalclustering.discovery;
	using CoreClusterMember = Neo4Net.causalclustering.discovery.CoreClusterMember;
	using ReadReplica = Neo4Net.causalclustering.discovery.ReadReplica;
	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using IEntity = Neo4Net.GraphDb.Entity;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using NotFoundException = Neo4Net.GraphDb.NotFoundException;
	using QueryExecutionException = Neo4Net.GraphDb.QueryExecutionException;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Result = Neo4Net.GraphDb.Result;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using ClusterRule = Neo4Net.Test.causalclustering.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.api.impl.fulltext.FulltextProceduresTest.AWAIT_REFRESH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.api.impl.fulltext.FulltextProceduresTest.NODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.api.impl.fulltext.FulltextProceduresTest.NODE_CREATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.api.impl.fulltext.FulltextProceduresTest.QUERY_NODES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.api.impl.fulltext.FulltextProceduresTest.QUERY_RELS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.api.impl.fulltext.FulltextProceduresTest.RELATIONSHIP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.api.impl.fulltext.FulltextProceduresTest.RELATIONSHIP_CREATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.api.impl.fulltext.FulltextProceduresTest.array;

	public class FulltextIndexCausalClusterIT
	{
		 private static readonly Label _label = Label.label( "LABEL" );
		 private const string PROP = "prop";
		 private const string PROP2 = "otherprop";
		 // The "ec_prop" property is added because the EC indexes cannot have exactly the same IEntity-token/property-token sets as the non-EC indexes:
		 private const string EC_PROP = "ec_prop";
		 private static readonly RelationshipType _rel = RelationshipType.withName( "REL" );
		 private const string NODE_INDEX = "nodeIndex";
		 private const string REL_INDEX = "relIndex";
		 private const string NODE_INDEX_EC = "nodeIndexEventuallyConsistent";
		 private const string REL_INDEX_EC = "relIndexEventuallyConsistent";
		 private static readonly string _eventuallyConsistentSetting = ", {" + FulltextIndexSettings.INDEX_CONFIG_EVENTUALLY_CONSISTENT + ": 'true'}";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.test.causalclustering.ClusterRule clusterRule = new Neo4Net.test.causalclustering.ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(1);
		 public ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(1);

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private Neo4Net.causalclustering.discovery.Cluster<?> cluster;
		 private Cluster<object> _cluster;
		 private long _nodeId1;
		 private long _nodeId2;
		 private long _relId1;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  _cluster = ClusterRule.startCluster();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fulltextIndexContentsMustBeReplicatedWhenPopulaing() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FulltextIndexContentsMustBeReplicatedWhenPopulaing()
		 {
			  _cluster.coreTx((db, tx) =>
			  {
				Node node1 = Db.createNode( _label );
				node1.setProperty( PROP, "This is an integration test." );
				node1.setProperty( EC_PROP, true );
				Node node2 = Db.createNode( _label );
				node2.setProperty( PROP2, "This is a related integration test." );
				node2.setProperty( EC_PROP, true );
				Relationship rel = node1.createRelationshipTo( node2, _rel );
				rel.setProperty( PROP, "They relate" );
				rel.setProperty( EC_PROP, true );
				_nodeId1 = node1.Id;
				_nodeId2 = node2.Id;
				_relId1 = rel.Id;
				tx.success();
			  });
			  _cluster.coreTx((db, tx) =>
			  {
				Db.execute( format( NODE_CREATE, NODE_INDEX, array( _label.name() ), array(PROP, PROP2) ) ).close();
				Db.execute( format( RELATIONSHIP_CREATE, REL_INDEX, array( _rel.name() ), array(PROP) ) ).close();
				Db.execute( format( NODE_CREATE, NODE_INDEX_EC, array( _label.name() ), array(PROP, PROP2, EC_PROP) + _eventuallyConsistentSetting ) ).close();
				Db.execute( format( RELATIONSHIP_CREATE, REL_INDEX_EC, array( _rel.name() ), array(PROP, EC_PROP) + _eventuallyConsistentSetting ) ).close();
				tx.success();
			  });

			  AwaitCatchup();

			  VerifyIndexContents( NODE_INDEX, "integration", true, _nodeId1, _nodeId2 );
			  VerifyIndexContents( NODE_INDEX_EC, "integration", true, _nodeId1, _nodeId2 );
			  VerifyIndexContents( NODE_INDEX, "test", true, _nodeId1, _nodeId2 );
			  VerifyIndexContents( NODE_INDEX_EC, "test", true, _nodeId1, _nodeId2 );
			  VerifyIndexContents( NODE_INDEX, "related", true, _nodeId2 );
			  VerifyIndexContents( NODE_INDEX_EC, "related", true, _nodeId2 );
			  VerifyIndexContents( REL_INDEX, "relate", false, _relId1 );
			  VerifyIndexContents( REL_INDEX_EC, "relate", false, _relId1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fulltextIndexContentsMustBeReplicatedWhenUpdating() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FulltextIndexContentsMustBeReplicatedWhenUpdating()
		 {
			  _cluster.coreTx((db, tx) =>
			  {
				Db.execute( format( NODE_CREATE, NODE_INDEX, array( _label.name() ), array(PROP, PROP2) ) ).close();
				Db.execute( format( RELATIONSHIP_CREATE, REL_INDEX, array( _rel.name() ), array(PROP) ) ).close();
				Db.execute( format( NODE_CREATE, NODE_INDEX_EC, array( _label.name() ), array(PROP, PROP2, EC_PROP) ) ).close();
				Db.execute( format( RELATIONSHIP_CREATE, REL_INDEX_EC, array( _rel.name() ), array(PROP, EC_PROP) ) ).close();
				tx.success();
			  });

			  AwaitCatchup();

			  _cluster.coreTx((db, tx) =>
			  {
				Node node1 = Db.createNode( _label );
				node1.setProperty( PROP, "This is an integration test." );
				node1.setProperty( EC_PROP, true );
				Node node2 = Db.createNode( _label );
				node2.setProperty( PROP2, "This is a related integration test." );
				node2.setProperty( EC_PROP, true );
				Relationship rel = node1.createRelationshipTo( node2, _rel );
				rel.setProperty( PROP, "They relate" );
				rel.setProperty( EC_PROP, true );
				_nodeId1 = node1.Id;
				_nodeId2 = node2.Id;
				_relId1 = rel.Id;
				tx.success();
			  });

			  AwaitCatchup();

			  VerifyIndexContents( NODE_INDEX, "integration", true, _nodeId1, _nodeId2 );
			  VerifyIndexContents( NODE_INDEX_EC, "integration", true, _nodeId1, _nodeId2 );
			  VerifyIndexContents( NODE_INDEX, "test", true, _nodeId1, _nodeId2 );
			  VerifyIndexContents( NODE_INDEX_EC, "test", true, _nodeId1, _nodeId2 );
			  VerifyIndexContents( NODE_INDEX, "related", true, _nodeId2 );
			  VerifyIndexContents( NODE_INDEX_EC, "related", true, _nodeId2 );
			  VerifyIndexContents( REL_INDEX, "relate", false, _relId1 );
			  VerifyIndexContents( REL_INDEX_EC, "relate", false, _relId1 );
		 }

		 // TODO analyzer setting must be replicates to all cluster members
		 // TODO eventually_consistent setting must be replicated to all cluster members.

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void awaitCatchup() throws InterruptedException
		 private void AwaitCatchup()
		 {
			  MutableLongSet appliedTransactions = new LongHashSet();
			  System.Action<ClusterMember> awaitPopulationAndCollectionAppliedTransactionId = member =>
			  {
				GraphDatabaseAPI db = member.database();
				try
				{
					using ( Transaction ignore = Db.beginTx() )
					{
						 Db.schema().awaitIndexesOnline(20, TimeUnit.SECONDS);
						 Db.execute( AWAIT_REFRESH ).close();
						 DependencyResolver dependencyResolver = Db.DependencyResolver;
						 TransactionIdStore transactionIdStore = dependencyResolver.resolveDependency( typeof( TransactionIdStore ) );
						 appliedTransactions.add( transactionIdStore.LastClosedTransactionId );
					}
				}
				catch ( Exception e ) when ( e is QueryExecutionException || e is System.ArgumentException )
				{
					 if ( e.Message.Equals( "No index was found" ) )
					 {
						  // Looks like the index creation hasn't been replicated yet, so we force a retry by making sure that
						  // the 'appliedTransactions' set will definitely contain more than one element.
						  appliedTransactions.add( -1L );
						  appliedTransactions.add( -2L );
					 }
				}
				catch ( NotFoundException )
				{
					 // This can happen due to a race inside `db.schema().awaitIndexesOnline`, where the list of indexes are provided by the SchemaCache, which is
					 // updated during command application in commit, but the index status is then fetched from the IndexMap, which is updated when the applier is
					 // closed during commit (which comes later in the commit process than command application). So we are told by the SchemaCache that an index
					 // exists, but we are then also told by the IndexMap that the index does not exist, hence this NotFoundException. Normally, these two are
					 // protected by the schema locks that are taken on index create and index status check. However, those locks are 1) incorrectly managed around
					 // index population, and 2) those locks are NOT TAKEN in Causal Cluster command replication!
					 // So we need to anticipate this, and if the race happens, we simply have to try again. But yeah, it needs to be fixed properly at some point.
					 appliedTransactions.add( -1L );
					 appliedTransactions.add( -2L );
				}
			  };
			  do
			  {
					appliedTransactions.clear();
					Thread.Sleep( 25 );
					ICollection<CoreClusterMember> cores = _cluster.coreMembers();
					ICollection<ReadReplica> readReplicas = _cluster.readReplicas();
					cores.forEach( awaitPopulationAndCollectionAppliedTransactionId );
					readReplicas.forEach( awaitPopulationAndCollectionAppliedTransactionId );
			  } while ( appliedTransactions.size() != 1 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyIndexContents(String index, String queryString, boolean queryNodes, long... IEntityIds) throws Exception
		 private void VerifyIndexContents( string index, string queryString, bool queryNodes, params long[] IEntityIds )
		 {
			  foreach ( CoreClusterMember member in _cluster.coreMembers() )
			  {
					VerifyIndexContents( member.Database(), index, queryString, IEntityIds, queryNodes );
			  }
			  foreach ( ReadReplica member in _cluster.readReplicas() )
			  {
					VerifyIndexContents( member.Database(), index, queryString, IEntityIds, queryNodes );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyIndexContents(Neo4Net.graphdb.GraphDatabaseService db, String index, String queryString, long[] IEntityIds, boolean queryNodes) throws Exception
		 private void VerifyIndexContents( IGraphDatabaseService db, string index, string queryString, long[] IEntityIds, bool queryNodes )
		 {
			  IList<long> expected = Arrays.stream( IEntityIds ).boxed().collect(Collectors.toList());
			  string queryCall = queryNodes ? QUERY_NODES : QUERY_RELS;
			  using ( Result result = Db.execute( format( queryCall, index, queryString ) ) )
			  {
					ISet<long> results = new HashSet<long>();
					while ( result.MoveNext() )
					{
						 results.Add( ( ( IEntity ) result.Current.get( queryNodes ? NODE : RELATIONSHIP ) ).Id );
					}
					string errorMessage = errorMessage( results, expected ) + " (" + db + ", leader is " + _cluster.awaitLeader() + ") query = " + queryString;
					assertEquals( errorMessage, expected.Count, results.Count );
					int i = 0;
					while ( results.Count > 0 )
					{
						 assertTrue( errorMessage, results.remove( expected[i++] ) );
					}
			  }
		 }

		 private static string ErrorMessage( ISet<long> actual, IList<long> expected )
		 {
			  return format( "Query results differ from expected, expected %s but got %s", expected, actual );
		 }
	}

}