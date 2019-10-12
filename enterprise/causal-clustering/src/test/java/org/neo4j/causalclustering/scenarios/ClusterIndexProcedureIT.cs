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
namespace Org.Neo4j.causalclustering.scenarios
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Org.Neo4j.causalclustering.discovery;
	using Org.Neo4j.causalclustering.discovery;
	using CoreClusterMember = Org.Neo4j.causalclustering.discovery.CoreClusterMember;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using ConstraintDefinition = Org.Neo4j.Graphdb.schema.ConstraintDefinition;
	using ConstraintType = Org.Neo4j.Graphdb.schema.ConstraintType;
	using IndexDefinition = Org.Neo4j.Graphdb.schema.IndexDefinition;
	using IndexReference = Org.Neo4j.@internal.Kernel.Api.IndexReference;
	using SchemaRead = Org.Neo4j.@internal.Kernel.Api.SchemaRead;
	using TokenRead = Org.Neo4j.@internal.Kernel.Api.TokenRead;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using ThreadToStatementContextBridge = Org.Neo4j.Kernel.impl.core.ThreadToStatementContextBridge;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using ClusterRule = Org.Neo4j.Test.causalclustering.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.Cluster.dataMatchesEventually;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.schema.ConstraintType.NODE_KEY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.schema.ConstraintType.UNIQUENESS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.single;

	public class ClusterIndexProcedureIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.causalclustering.ClusterRule clusterRule = new org.neo4j.test.causalclustering.ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(3).withTimeout(1000, SECONDS);
		 public readonly ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(3).withTimeout(1000, SECONDS);

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.causalclustering.discovery.Cluster<?> cluster;
		 private Cluster<object> _cluster;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _cluster = ClusterRule.startCluster();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createIndexProcedureMustPropagate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateIndexProcedureMustPropagate()
		 {
			  // create an index
			  _cluster.coreTx((db, tx) =>
			  {
				Db.execute( "CALL db.createIndex( \":Person(name)\", \"lucene+native-1.0\")" ).close();
				tx.success();
			  });

			  // node created just to be able to use dataMatchesEventually as a barrier
			  CoreClusterMember leader = _cluster.coreTx((db, tx) =>
			  {
				Node person = Db.createNode( Label.label( "Person" ) );
				person.setProperty( "name", "Bo Burnham" );
				tx.success();
			  });

			  // node creation is guaranteed to happen after index creation
			  dataMatchesEventually( leader, _cluster.coreMembers() );
			  dataMatchesEventually( leader, _cluster.readReplicas() );

			  // now the indexes must exist, so we wait for them to come online
			  _cluster.coreMembers().forEach(this.awaitIndexOnline);
			  _cluster.readReplicas().forEach(this.awaitIndexOnline);

			  // verify indexes
			  _cluster.coreMembers().forEach(core => verifyIndexes(core.database()));
			  _cluster.readReplicas().forEach(rr => verifyIndexes(rr.database()));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createUniquePropertyConstraintMustPropagate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateUniquePropertyConstraintMustPropagate()
		 {
			  // create a constraint
			  CoreClusterMember leader = _cluster.coreTx((db, tx) =>
			  {
				Db.execute( "CALL db.createUniquePropertyConstraint( \":Person(name)\", \"lucene+native-1.0\")" ).close();
				tx.success();
			  });

			  // node created just to be able to use dataMatchesEventually as a barrier
			  _cluster.coreTx((db, tx) =>
			  {
				Node person = Db.createNode( Label.label( "Person" ) );
				person.setProperty( "name", "Bo Burnham" );
				tx.success();
			  });

			  // node creation is guaranteed to happen after constraint creation
			  dataMatchesEventually( leader, _cluster.coreMembers() );
			  dataMatchesEventually( leader, _cluster.readReplicas() );

			  // verify indexes
			  _cluster.coreMembers().forEach(core => verifyIndexes(core.database()));
			  _cluster.readReplicas().forEach(rr => verifyIndexes(rr.database()));

			  // verify constraints
			  _cluster.coreMembers().forEach(core => verifyConstraints(core.database(), UNIQUENESS));
			  _cluster.readReplicas().forEach(rr => verifyConstraints(rr.database(), UNIQUENESS));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createNodeKeyConstraintMustPropagate() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateNodeKeyConstraintMustPropagate()
		 {
			  // create a node key
			  CoreClusterMember leader = _cluster.coreTx((db, tx) =>
			  {
				Db.execute( "CALL db.createNodeKey( \":Person(name)\", \"lucene+native-1.0\")" ).close();
				tx.success();
			  });

			  // node created just to be able to use dataMatchesEventually as a barrier
			  _cluster.coreTx((db, tx) =>
			  {
				Node person = Db.createNode( Label.label( "Person" ) );
				person.setProperty( "name", "Bo Burnham" );
				tx.success();
			  });

			  // node creation is guaranteed to happen after constraint creation
			  dataMatchesEventually( leader, _cluster.coreMembers() );
			  dataMatchesEventually( leader, _cluster.readReplicas() );

			  // verify indexes
			  _cluster.coreMembers().forEach(core => verifyIndexes(core.database()));
			  _cluster.readReplicas().forEach(rr => verifyIndexes(rr.database()));

			  // verify node keys
			  _cluster.coreMembers().forEach(core => verifyConstraints(core.database(), NODE_KEY));
			  _cluster.readReplicas().forEach(rr => verifyConstraints(rr.database(), NODE_KEY));
		 }

		 private void AwaitIndexOnline( ClusterMember member )
		 {
			  GraphDatabaseAPI db = member.database();
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					tx.Success();
			  }
		 }

		 private void VerifyIndexes( GraphDatabaseAPI db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					// only one index
					IEnumerator<IndexDefinition> indexes = Db.schema().Indexes.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( "has one index", indexes.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					IndexDefinition indexDefinition = indexes.next();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( "not more than one index", indexes.hasNext() );

					Label label = single( indexDefinition.Labels );
					string property = indexDefinition.PropertyKeys.GetEnumerator().next();

					// with correct pattern and provider
					assertEquals( "correct label", "Person", label.Name() );
					assertEquals( "correct property", "name", property );
					AssertCorrectProvider( db, label, property );

					tx.Success();
			  }
		 }

		 private void VerifyConstraints( GraphDatabaseAPI db, ConstraintType expectedConstraintType )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					// only one index
					IEnumerator<ConstraintDefinition> constraints = Db.schema().Constraints.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( "has one index", constraints.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					ConstraintDefinition constraint = constraints.next();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( "not more than one index", constraints.hasNext() );

					Label label = constraint.Label;
					string property = constraint.PropertyKeys.GetEnumerator().next();
					ConstraintType constraintType = constraint.ConstraintType;

					// with correct pattern and provider
					assertEquals( "correct label", "Person", label.Name() );
					assertEquals( "correct property", "name", property );
					assertEquals( "correct constraint type", expectedConstraintType, constraintType );

					tx.Success();
			  }
		 }

		 private void AssertCorrectProvider( GraphDatabaseAPI db, Label label, string property )
		 {
			  KernelTransaction kernelTransaction = Db.DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) ).getKernelTransactionBoundToThisThread( false );
			  TokenRead tokenRead = kernelTransaction.TokenRead();
			  int labelId = tokenRead.NodeLabel( label.Name() );
			  int propId = tokenRead.PropertyKey( property );
			  SchemaRead schemaRead = kernelTransaction.SchemaRead();
			  IndexReference index = schemaRead.Index( labelId, propId );
			  assertEquals( "correct provider key", "lucene+native", index.ProviderKey() );
			  assertEquals( "correct provider version", "1.0", index.ProviderVersion() );
		 }
	}

}