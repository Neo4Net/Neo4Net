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
namespace Neo4Net.Kernel.ha
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using IndexDefinition = Neo4Net.GraphDb.schema.IndexDefinition;
	using IndexReference = Neo4Net.Internal.Kernel.Api.IndexReference;
	using Kernel = Neo4Net.Internal.Kernel.Api.Kernel;
	using KernelException = Neo4Net.Internal.Kernel.Api.exceptions.KernelException;
	using TransactionFailureException = Neo4Net.Internal.Kernel.Api.exceptions.TransactionFailureException;
	using IndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using ManagedCluster = Neo4Net.Kernel.impl.ha.ClusterManager.ManagedCluster;
	using Register_DoubleLongRegister = Neo4Net.Register.Register_DoubleLongRegister;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.Transaction_Type.@explicit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.security.LoginContext.AUTH_DISABLED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.register.Registers.newDoubleLongRegister;

	public class HaCountsIT
	{
		 private static readonly Label _label = Label.label( "label" );
		 private const string PROPERTY_NAME = "prop";
		 private const string PROPERTY_VALUE = "value";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.ha.ClusterRule clusterRule = new org.Neo4Net.test.ha.ClusterRule();
		 public readonly ClusterRule ClusterRule = new ClusterRule();

		 private ManagedCluster _cluster;
		 private HighlyAvailableGraphDatabase _master;
		 private HighlyAvailableGraphDatabase _slave1;
		 private HighlyAvailableGraphDatabase _slave2;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _cluster = ClusterRule.startCluster();
			  _master = _cluster.Master;
			  _slave1 = _cluster.AnySlave;
			  _slave2 = _cluster.getAnySlave( _slave1 );
			  ClearDatabase();
		 }

		 private void ClearDatabase()
		 {
			  using ( Transaction tx = _master.beginTx() )
			  {
					foreach ( IndexDefinition index in _master.schema().Indexes )
					{
						 index.Drop();
					}
					tx.Success();
			  }

			  using ( Transaction tx = _master.beginTx() )
			  {
					foreach ( Node node in _master.AllNodes )
					{
						 foreach ( Relationship relationship in node.Relationships )
						 {
							  relationship.Delete();
						 }
						 node.Delete();
					}
					tx.Success();
			  }
			  _cluster.sync();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateCountsOnSlavesWhenCreatingANodeOnMaster()
		 public virtual void ShouldUpdateCountsOnSlavesWhenCreatingANodeOnMaster()
		 {
			  // when creating a node on the master
			  CreateANode( _master, _label, PROPERTY_VALUE, PROPERTY_NAME );

			  // and the slaves got the updates
			  _cluster.sync( _master );

			  // then the slaves has updated counts
			  AssertOnNodeCounts( 1, 1, _label, _master );
			  AssertOnNodeCounts( 1, 1, _label, _slave1 );
			  AssertOnNodeCounts( 1, 1, _label, _slave2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateCountsOnMasterAndSlaveWhenCreatingANodeOnSlave()
		 public virtual void ShouldUpdateCountsOnMasterAndSlaveWhenCreatingANodeOnSlave()
		 {
			  // when creating a node on the slave
			  CreateANode( _slave1, _label, PROPERTY_VALUE, PROPERTY_NAME );

			  // and the master and slave2 got the updates
			  _cluster.sync( _slave1 );

			  // then the master and the other slave have updated counts
			  AssertOnNodeCounts( 1, 1, _label, _master );
			  AssertOnNodeCounts( 1, 1, _label, _slave1 );
			  AssertOnNodeCounts( 1, 1, _label, _slave2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateCountsOnSlavesWhenCreatingAnIndexOnMaster() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpdateCountsOnSlavesWhenCreatingAnIndexOnMaster()
		 {
			  // when creating a node on the master
			  CreateANode( _master, _label, PROPERTY_VALUE, PROPERTY_NAME );
			  IndexDescriptor schemaIndexDescriptor = CreateAnIndex( _master, _label, PROPERTY_NAME );
			  AwaitOnline( _master );

			  // and the slaves got the updates
			  _cluster.sync( _master );

			  AwaitOnline( _slave1 );
			  AwaitOnline( _slave2 );

			  // then the slaves has updated counts
			  AssertOnIndexCounts( 0, 1, 1, 1, schemaIndexDescriptor, _master );
			  AssertOnIndexCounts( 0, 1, 1, 1, schemaIndexDescriptor, _slave1 );
			  AssertOnIndexCounts( 0, 1, 1, 1, schemaIndexDescriptor, _slave2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateCountsOnClusterWhenCreatingANodeOnSlaveAndAnIndexOnMaster() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpdateCountsOnClusterWhenCreatingANodeOnSlaveAndAnIndexOnMaster()
		 {
			  // when creating a node on the master
			  CreateANode( _slave1, _label, PROPERTY_VALUE, PROPERTY_NAME );
			  IndexDescriptor schemaIndexDescriptor = CreateAnIndex( _master, _label, PROPERTY_NAME );
			  AwaitOnline( _master );

			  // and the updates are propagate in the cluster
			  _cluster.sync();

			  AwaitOnline( _slave1 );
			  AwaitOnline( _slave2 );

			  // then the slaves has updated counts
			  AssertOnIndexCounts( 0, 1, 1, 1, schemaIndexDescriptor, _master );
			  AssertOnIndexCounts( 0, 1, 1, 1, schemaIndexDescriptor, _slave1 );
			  AssertOnIndexCounts( 0, 1, 1, 1, schemaIndexDescriptor, _slave2 );
		 }

		 private static void CreateANode( HighlyAvailableGraphDatabase db, Label label, string value, string property )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode( label );
					node.SetProperty( property, value );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.Neo4Net.storageengine.api.schema.IndexDescriptor createAnIndex(HighlyAvailableGraphDatabase db, org.Neo4Net.graphdb.Label label, String propertyName) throws org.Neo4Net.internal.kernel.api.exceptions.KernelException
		 private static IndexDescriptor CreateAnIndex( HighlyAvailableGraphDatabase db, Label label, string propertyName )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					KernelTransaction ktx = KernelTransaction( db );
					int labelId = ktx.TokenWrite().labelGetOrCreateForName(label.Name());
					int propertyKeyId = ktx.TokenWrite().propertyKeyGetOrCreateForName(propertyName);
					IndexReference index = ktx.SchemaWrite().indexCreate(SchemaDescriptorFactory.forLabel(labelId, propertyKeyId));
					tx.Success();
					return ( IndexDescriptor ) index;
			  }
		 }

		 private static void AssertOnNodeCounts( int expectedTotalNodes, int expectedLabelledNodes, Label label, HighlyAvailableGraphDatabase db )
		 {
			  using ( Transaction ignored = Db.beginTx() )
			  {
					KernelTransaction transaction = KernelTransaction( db );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int labelId = transaction.tokenRead().nodeLabel(label.name());
					int labelId = transaction.TokenRead().nodeLabel(label.Name());
					assertEquals( expectedTotalNodes, transaction.DataRead().countsForNode(-1) );
					assertEquals( expectedLabelledNodes, transaction.DataRead().countsForNode(labelId) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void assertOnIndexCounts(int expectedIndexUpdates, int expectedIndexSize, int expectedUniqueValues, int expectedSampleSize, org.Neo4Net.storageengine.api.schema.IndexDescriptor indexDescriptor, HighlyAvailableGraphDatabase db) throws org.Neo4Net.internal.kernel.api.exceptions.TransactionFailureException, org.Neo4Net.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 private static void AssertOnIndexCounts( int expectedIndexUpdates, int expectedIndexSize, int expectedUniqueValues, int expectedSampleSize, IndexDescriptor indexDescriptor, HighlyAvailableGraphDatabase db )
		 {
			  using ( Neo4Net.Internal.Kernel.Api.Transaction tx = Db.DependencyResolver.resolveDependency( typeof( Kernel ) ).beginTransaction( @explicit, AUTH_DISABLED ) )
			  {
					IndexReference indexReference = tx.SchemaRead().index(indexDescriptor.Schema());
					AssertDoubleLongEquals( expectedIndexUpdates, expectedIndexSize, tx.SchemaRead().indexUpdatesAndSize(indexReference, newDoubleLongRegister()) );
					AssertDoubleLongEquals( expectedUniqueValues, expectedSampleSize, tx.SchemaRead().indexSample(indexReference, newDoubleLongRegister()) );
			  }
		 }

		 private static void AssertDoubleLongEquals( int expectedFirst, int expectedSecond, Register_DoubleLongRegister actualValues )
		 {
			  string msg = string.Format( "Expected ({0:D},{1:D}) but was ({2:D},{3:D})", expectedFirst, expectedSecond, actualValues.ReadFirst(), actualValues.ReadSecond() );
			  assertTrue( msg, actualValues.HasValues( expectedFirst, expectedSecond ) );
		 }

		 private static KernelTransaction KernelTransaction( HighlyAvailableGraphDatabase db )
		 {
			  return Db.DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) ).getKernelTransactionBoundToThisThread( true );
		 }

		 private static void AwaitOnline( HighlyAvailableGraphDatabase db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(60, TimeUnit.SECONDS);
					tx.Success();
			  }
		 }
	}

}