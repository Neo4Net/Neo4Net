﻿using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Org.Neo4j.Kernel.Api.Impl.Fulltext
{
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Org.Neo4j.causalclustering.discovery;
	using Org.Neo4j.causalclustering.discovery;
	using CoreClusterMember = Org.Neo4j.causalclustering.discovery.CoreClusterMember;
	using ReadReplica = Org.Neo4j.causalclustering.discovery.ReadReplica;
	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using Entity = Org.Neo4j.Graphdb.Entity;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using NotFoundException = Org.Neo4j.Graphdb.NotFoundException;
	using QueryExecutionException = Org.Neo4j.Graphdb.QueryExecutionException;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Result = Org.Neo4j.Graphdb.Result;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using ClusterRule = Org.Neo4j.Test.causalclustering.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.FulltextProceduresTest.AWAIT_REFRESH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.FulltextProceduresTest.NODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.FulltextProceduresTest.NODE_CREATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.FulltextProceduresTest.QUERY_NODES;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.FulltextProceduresTest.QUERY_RELS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.FulltextProceduresTest.RELATIONSHIP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.FulltextProceduresTest.RELATIONSHIP_CREATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.FulltextProceduresTest.array;

	public class FulltextIndexCausalClusterIT
	{
		 private static readonly Label _label = Label.label( "LABEL" );
		 private const string PROP = "prop";
		 private const string PROP2 = "otherprop";
		 // The "ec_prop" property is added because the EC indexes cannot have exactly the same entity-token/property-token sets as the non-EC indexes:
		 private const string EC_PROP = "ec_prop";
		 private static readonly RelationshipType _rel = RelationshipType.withName( "REL" );
		 private const string NODE_INDEX = "nodeIndex";
		 private const string REL_INDEX = "relIndex";
		 private const string NODE_INDEX_EC = "nodeIndexEventuallyConsistent";
		 private const string REL_INDEX_EC = "relIndexEventuallyConsistent";
		 private static readonly string _eventuallyConsistentSetting = ", {" + FulltextIndexSettings.INDEX_CONFIG_EVENTUALLY_CONSISTENT + ": 'true'}";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.causalclustering.ClusterRule clusterRule = new org.neo4j.test.causalclustering.ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(1);
		 public ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(1);

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.causalclustering.discovery.Cluster<?> cluster;
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
//ORIGINAL LINE: private void verifyIndexContents(String index, String queryString, boolean queryNodes, long... entityIds) throws Exception
		 private void VerifyIndexContents( string index, string queryString, bool queryNodes, params long[] entityIds )
		 {
			  foreach ( CoreClusterMember member in _cluster.coreMembers() )
			  {
					VerifyIndexContents( member.Database(), index, queryString, entityIds, queryNodes );
			  }
			  foreach ( ReadReplica member in _cluster.readReplicas() )
			  {
					VerifyIndexContents( member.Database(), index, queryString, entityIds, queryNodes );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyIndexContents(org.neo4j.graphdb.GraphDatabaseService db, String index, String queryString, long[] entityIds, boolean queryNodes) throws Exception
		 private void VerifyIndexContents( GraphDatabaseService db, string index, string queryString, long[] entityIds, bool queryNodes )
		 {
			  IList<long> expected = Arrays.stream( entityIds ).boxed().collect(Collectors.toList());
			  string queryCall = queryNodes ? QUERY_NODES : QUERY_RELS;
			  using ( Result result = Db.execute( format( queryCall, index, queryString ) ) )
			  {
					ISet<long> results = new HashSet<long>();
					while ( result.MoveNext() )
					{
						 results.Add( ( ( Entity ) result.Current.get( queryNodes ? NODE : RELATIONSHIP ) ).Id );
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