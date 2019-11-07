﻿using System.Collections.Generic;
using System.Threading;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Kernel.Impl.Api.index
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using Label = Neo4Net.GraphDb.Label;
	using IndexDefinition = Neo4Net.GraphDb.Schema.IndexDefinition;
	using Iterables = Neo4Net.Collections.Helpers.Iterables;
	using Iterators = Neo4Net.Collections.Helpers.Iterators;
	using IndexReference = Neo4Net.Kernel.Api.Internal.IndexReference;
	using SchemaRead = Neo4Net.Kernel.Api.Internal.SchemaRead;
	using SchemaWrite = Neo4Net.Kernel.Api.Internal.SchemaWrite;
	using TokenWrite = Neo4Net.Kernel.Api.Internal.TokenWrite;
	using Transaction = Neo4Net.Kernel.Api.Internal.Transaction;
	using Write = Neo4Net.Kernel.Api.Internal.Write;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
	using SchemaKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.SchemaKernelException;
	using LabelSchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.LabelSchemaDescriptor;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor;
	using NodePropertyAccessor = Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor;
	using MultiTokenSchemaDescriptor = Neo4Net.Kernel.Api.schema.MultiTokenSchemaDescriptor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.Api.schema.SchemaDescriptorFactory;
	using IndexBackedConstraintDescriptor = Neo4Net.Kernel.Api.schema.constraints.IndexBackedConstraintDescriptor;
	using Config = Neo4Net.Kernel.configuration.Config;
	using KernelIntegrationTest = Neo4Net.Kernel.Impl.Api.integrationtest.KernelIntegrationTest;
	using ConstraintIndexCreator = Neo4Net.Kernel.Impl.Api.state.ConstraintIndexCreator;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using EntityType = Neo4Net.Kernel.Api.StorageEngine.EntityType;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.RelationshipType.withName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.GraphDatabaseSettings.default_schema_provider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterables.single;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.Internal.security.LoginContext.AUTH_DISABLED;

	public class IndexIT : KernelIntegrationTest
	{
		 private const string LABEL = "Label";
		 private const string LABEL2 = "Label2";
		 private const string REL_TYPE = "RelType";
		 private const string REL_TYPE2 = "RelType2";
		 private const string PROPERTY_KEY = "prop";
		 private const string PROPERTY_KEY2 = "prop2";

		 private int _labelId;
		 private int _labelId2;
		 private int _relType;
		 private int _relType2;
		 private int _propertyKeyId;
		 private int _propertyKeyId2;
		 private LabelSchemaDescriptor _descriptor;
		 private LabelSchemaDescriptor _descriptor2;
		 private ExecutorService _executorService;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void createLabelAndProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateLabelAndProperty()
		 {
			  TokenWrite tokenWrites = TokenWriteInNewTransaction();
			  _labelId = tokenWrites.LabelGetOrCreateForName( LABEL );
			  _labelId2 = tokenWrites.LabelGetOrCreateForName( LABEL2 );
			  _relType = tokenWrites.RelationshipTypeGetOrCreateForName( REL_TYPE );
			  _relType2 = tokenWrites.RelationshipTypeGetOrCreateForName( REL_TYPE2 );
			  _propertyKeyId = tokenWrites.PropertyKeyGetOrCreateForName( PROPERTY_KEY );
			  _propertyKeyId2 = tokenWrites.PropertyKeyGetOrCreateForName( PROPERTY_KEY2 );
			  _descriptor = SchemaDescriptorFactory.forLabel( _labelId, _propertyKeyId );
			  _descriptor2 = SchemaDescriptorFactory.forLabel( _labelId, _propertyKeyId2 );
			  Commit();
			  _executorService = Executors.newCachedThreadPool();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _executorService.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void createIndexForAnotherLabelWhileHoldingSharedLockOnOtherLabel() throws Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateIndexForAnotherLabelWhileHoldingSharedLockOnOtherLabel()
		 {
			  TokenWrite tokenWrite = TokenWriteInNewTransaction();
			  int label2 = tokenWrite.LabelGetOrCreateForName( "Label2" );

			  Write write = DataWriteInNewTransaction();
			  long nodeId = write.NodeCreate();
			  write.NodeAddLabel( nodeId, label2 );

			  SchemaWriteInNewTransaction().indexCreate(_descriptor);
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 10_000) public void createIndexesForDifferentLabelsConcurrently() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CreateIndexesForDifferentLabelsConcurrently()
		 {
			  TokenWrite tokenWrite = TokenWriteInNewTransaction();
			  int label2 = tokenWrite.LabelGetOrCreateForName( "Label2" );

			  LabelSchemaDescriptor anotherLabelDescriptor = SchemaDescriptorFactory.forLabel( label2, _propertyKeyId );
			  SchemaWriteInNewTransaction().indexCreate(anotherLabelDescriptor);

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> indexFuture = executorService.submit(createIndex(db, label(LABEL), PROPERTY_KEY));
			  Future<object> indexFuture = _executorService.submit( CreateIndex( Db, label( LABEL ), PROPERTY_KEY ) );
			  indexFuture.get();
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void addIndexRuleInATransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AddIndexRuleInATransaction()
		 {
			  // GIVEN
			  SchemaWrite schemaWriteOperations = SchemaWriteInNewTransaction();

			  // WHEN
			  IndexReference expectedRule = schemaWriteOperations.IndexCreate( _descriptor );
			  Commit();

			  // THEN
			  SchemaRead schemaRead = NewTransaction().schemaRead();
			  assertEquals( asSet( expectedRule ), asSet( schemaRead.IndexesGetForLabel( _labelId ) ) );
			  assertEquals( expectedRule, schemaRead.Index( _descriptor.LabelId, _descriptor.PropertyIds ) );
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void committedAndTransactionalIndexRulesShouldBeMerged() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CommittedAndTransactionalIndexRulesShouldBeMerged()
		 {
			  // GIVEN
			  SchemaWrite schemaWriteOperations = SchemaWriteInNewTransaction();
			  IndexReference existingRule = schemaWriteOperations.IndexCreate( _descriptor );
			  Commit();

			  // WHEN
			  Transaction transaction = NewTransaction( AUTH_DISABLED );
			  IndexReference addedRule = transaction.SchemaWrite().indexCreate(SchemaDescriptorFactory.forLabel(_labelId, 10));
			  ISet<IndexReference> indexRulesInTx = asSet( transaction.SchemaRead().indexesGetForLabel(_labelId) );
			  Commit();

			  // THEN
			  assertEquals( asSet( existingRule, addedRule ), indexRulesInTx );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void rollBackIndexRuleShouldNotBeCommitted() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RollBackIndexRuleShouldNotBeCommitted()
		 {
			  // GIVEN
			  SchemaWrite schemaWrite = SchemaWriteInNewTransaction();

			  // WHEN
			  schemaWrite.IndexCreate( _descriptor );
			  // don't mark as success
			  Rollback();

			  // THEN
			  Transaction transaction = NewTransaction();
			  assertEquals( emptySet(), asSet(transaction.SchemaRead().indexesGetForLabel(_labelId)) );
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRemoveAConstraintIndexWithoutOwner() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToRemoveAConstraintIndexWithoutOwner()
		 {
			  // given
			  NodePropertyAccessor propertyAccessor = mock( typeof( NodePropertyAccessor ) );
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  ConstraintIndexCreator creator = new ConstraintIndexCreator( () => Kernel, IndexingService, propertyAccessor, logProvider );

			  string defaultProvider = Config.defaults().get(default_schema_provider);
			  IndexDescriptor constraintIndex = creator.CreateConstraintIndex( _descriptor, defaultProvider );
			  // then
			  Transaction transaction = NewTransaction();
			  assertEquals( emptySet(), asSet(transaction.SchemaRead().constraintsGetForLabel(_labelId)) );
			  Commit();

			  // when
			  SchemaWrite schemaWrite = SchemaWriteInNewTransaction();
			  schemaWrite.IndexDrop( constraintIndex );
			  Commit();

			  // then
			  transaction = NewTransaction();
			  assertEquals( emptySet(), asSet(transaction.SchemaRead().indexesGetForLabel(_labelId)) );
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDisallowDroppingIndexThatDoesNotExist() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDisallowDroppingIndexThatDoesNotExist()
		 {
			  // given
			  IndexReference index;
			  {
					SchemaWrite statement = SchemaWriteInNewTransaction();
					index = statement.IndexCreate( _descriptor );
					Commit();
			  }
			  {
					SchemaWrite statement = SchemaWriteInNewTransaction();
					statement.IndexDrop( index );
					Commit();
			  }

			  // when
			  try
			  {
					SchemaWrite statement = SchemaWriteInNewTransaction();
					statement.IndexDrop( index );
					Commit();
			  }
			  // then
			  catch ( SchemaKernelException e )
			  {
					assertEquals( "Unable to drop index on :label[" + _labelId + "](property[" + _propertyKeyId + "]): " + "No such INDEX ON :label[" + _labelId + "]uniquetempvar.", e.Message );
			  }
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToCreateIndexWhereAConstraintAlreadyExists() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToCreateIndexWhereAConstraintAlreadyExists()
		 {
			  {
			  // given
					SchemaWrite statement = SchemaWriteInNewTransaction();
					statement.UniquePropertyConstraintCreate( _descriptor );
					Commit();
			  }

			  // when
			  try
			  {
					SchemaWrite statement = SchemaWriteInNewTransaction();
					statement.IndexCreate( _descriptor );
					Commit();

					fail( "expected exception" );
			  }
			  // then
			  catch ( SchemaKernelException e )
			  {
					assertEquals( "There is a uniqueness constraint on :" + LABEL + "(" + PROPERTY_KEY + "), so an index is " + "already created that matches this.", e.Message );
			  }
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListConstraintIndexesInTheCoreAPI() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListConstraintIndexesInTheCoreAPI()
		 {
			  // given
			  Transaction transaction = NewTransaction( AUTH_DISABLED );
			  transaction.SchemaWrite().uniquePropertyConstraintCreate(SchemaDescriptorFactory.forLabel(transaction.TokenWrite().labelGetOrCreateForName("Label1"), transaction.TokenWrite().propertyKeyGetOrCreateForName("property1")));
			  Commit();

			  // when
			  using ( Neo4Net.GraphDb.Transaction ignore = Db.beginTx() )
			  {
					ISet<IndexDefinition> indexes = Iterables.asSet( Db.schema().Indexes );

					// then
					assertEquals( 1, indexes.Count );
					IndexDefinition index = indexes.GetEnumerator().next();
					assertEquals( "Label1", single( index.Labels ).name() );
					assertEquals( asSet( "property1" ), Iterables.asSet( index.PropertyKeys ) );
					assertTrue( "index should be a constraint index", index.ConstraintIndex );

					// when
					try
					{
						 index.Drop();

						 fail( "expected exception" );
					}
					// then
					catch ( System.InvalidOperationException e )
					{
						 assertEquals( "Constraint indexes cannot be dropped directly, " + "instead drop the owning uniqueness constraint.", e.Message );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListMultiTokenIndexesInTheCoreAPI() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListMultiTokenIndexesInTheCoreAPI()
		 {
			  Transaction transaction = NewTransaction( AUTH_DISABLED );
			  MultiTokenSchemaDescriptor descriptor = SchemaDescriptorFactory.multiToken( new int[]{ _labelId, _labelId2 }, EntityType.NODE, _propertyKeyId );
			  transaction.SchemaWrite().indexCreate(descriptor);
			  Commit();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: try (@SuppressWarnings("unused") Neo4Net.graphdb.Transaction tx = db.beginTx())
			  using ( Neo4Net.GraphDb.Transaction tx = Db.beginTx() )
			  {
					ISet<IndexDefinition> indexes = Iterables.asSet( Db.schema().Indexes );

					// then
					assertEquals( 1, indexes.Count );
					IndexDefinition index = indexes.GetEnumerator().next();
					try
					{
						 index.Label;
						 fail( "index.getLabel() should have thrown. " );
					}
					catch ( System.InvalidOperationException )
					{
					}
					try
					{
						 index.RelationshipType;
						 fail( "index.getRelationshipType() should have thrown. " );
					}
					catch ( System.InvalidOperationException )
					{
					}
					try
					{
						 index.RelationshipTypes;
						 fail( "index.getRelationshipTypes() should have thrown. " );
					}
					catch ( System.InvalidOperationException )
					{
					}
					assertThat( index.Labels, containsInAnyOrder( label( LABEL ), label( LABEL2 ) ) );
					assertFalse( "should not be a constraint index", index.ConstraintIndex );
					assertTrue( "should be a multi-token index", index.MultiTokenIndex );
					assertFalse( "should not be a composite index", index.CompositeIndex );
					assertTrue( "should be a node index", index.NodeIndex );
					assertFalse( "should not be a relationship index", index.RelationshipIndex );
					assertEquals( asSet( PROPERTY_KEY ), Iterables.asSet( index.PropertyKeys ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListCompositeIndexesInTheCoreAPI() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListCompositeIndexesInTheCoreAPI()
		 {
			  Transaction transaction = NewTransaction( AUTH_DISABLED );
			  SchemaDescriptor descriptor = SchemaDescriptorFactory.forLabel( _labelId, _propertyKeyId, _propertyKeyId2 );
			  transaction.SchemaWrite().indexCreate(descriptor);
			  Commit();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: try (@SuppressWarnings("unused") Neo4Net.graphdb.Transaction tx = db.beginTx())
			  using ( Neo4Net.GraphDb.Transaction tx = Db.beginTx() )
			  {
					ISet<IndexDefinition> indexes = Iterables.asSet( Db.schema().Indexes );

					// then
					assertEquals( 1, indexes.Count );
					IndexDefinition index = indexes.GetEnumerator().next();
					assertEquals( LABEL, single( index.Labels ).name() );
					assertThat( index.Labels, containsInAnyOrder( label( LABEL ) ) );
					try
					{
						 index.RelationshipType;
						 fail( "index.getRelationshipType() should have thrown. " );
					}
					catch ( System.InvalidOperationException )
					{
					}
					try
					{
						 index.RelationshipTypes;
						 fail( "index.getRelationshipTypes() should have thrown. " );
					}
					catch ( System.InvalidOperationException )
					{
					}
					assertFalse( "should not be a constraint index", index.ConstraintIndex );
					assertFalse( "should not be a multi-token index", index.MultiTokenIndex );
					assertTrue( "should be a composite index", index.CompositeIndex );
					assertTrue( "should be a node index", index.NodeIndex );
					assertFalse( "should not be a relationship index", index.RelationshipIndex );
					assertEquals( asSet( PROPERTY_KEY, PROPERTY_KEY2 ), Iterables.asSet( index.PropertyKeys ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListRelationshipIndexesInTheCoreAPI() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListRelationshipIndexesInTheCoreAPI()
		 {
			  Transaction transaction = NewTransaction( AUTH_DISABLED );
			  SchemaDescriptor descriptor = SchemaDescriptorFactory.forRelType( _relType, _propertyKeyId );
			  transaction.SchemaWrite().indexCreate(descriptor);
			  Commit();

			  using ( Neo4Net.GraphDb.Transaction tx = Db.beginTx() )
			  {
					ISet<IndexDefinition> indexes = Iterables.asSet( Db.schema().Indexes );

					// then
					assertEquals( 1, indexes.Count );
					IndexDefinition index = indexes.GetEnumerator().next();
					try
					{
						 index.Label;
						 fail( "index.getLabel() should have thrown. " );
					}
					catch ( System.InvalidOperationException )
					{
					}
					try
					{
						 index.Labels;
						 fail( "index.getLabels() should have thrown. " );
					}
					catch ( System.InvalidOperationException )
					{
					}
					assertEquals( REL_TYPE, index.RelationshipType.name() );
					assertEquals( singletonList( withName( REL_TYPE ) ), index.RelationshipTypes );
					assertFalse( "should not be a constraint index", index.ConstraintIndex );
					assertFalse( "should not be a multi-token index", index.MultiTokenIndex );
					assertFalse( "should not be a composite index", index.CompositeIndex );
					assertFalse( "should not be a node index", index.NodeIndex );
					assertTrue( "should be a relationship index", index.RelationshipIndex );
					assertEquals( asSet( PROPERTY_KEY ), Iterables.asSet( index.PropertyKeys ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListCompositeMultiTokenRelationshipIndexesInTheCoreAPI() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListCompositeMultiTokenRelationshipIndexesInTheCoreAPI()
		 {
			  Transaction transaction = NewTransaction( AUTH_DISABLED );
			  SchemaDescriptor descriptor = SchemaDescriptorFactory.multiToken( new int[]{ _relType, _relType2 }, EntityType.RELATIONSHIP, _propertyKeyId, _propertyKeyId2 );
			  transaction.SchemaWrite().indexCreate(descriptor);
			  Commit();

			  using ( Neo4Net.GraphDb.Transaction tx = Db.beginTx() )
			  {
					ISet<IndexDefinition> indexes = Iterables.asSet( Db.schema().Indexes );

					// then
					assertEquals( 1, indexes.Count );
					IndexDefinition index = indexes.GetEnumerator().next();
					try
					{
						 index.Label;
						 fail( "index.getLabel() should have thrown. " );
					}
					catch ( System.InvalidOperationException )
					{
					}
					try
					{
						 index.Labels;
						 fail( "index.getLabels() should have thrown. " );
					}
					catch ( System.InvalidOperationException )
					{
					}
					try
					{
						 index.RelationshipType;
						 fail( "index.getRelationshipType() should have thrown. " );
					}
					catch ( System.InvalidOperationException )
					{
					}
					assertThat( index.RelationshipTypes, containsInAnyOrder( withName( REL_TYPE ), withName( REL_TYPE2 ) ) );
					assertFalse( "should not be a constraint index", index.ConstraintIndex );
					assertTrue( "should be a multi-token index", index.MultiTokenIndex );
					assertTrue( "should be a composite index", index.CompositeIndex );
					assertFalse( "should not be a node index", index.NodeIndex );
					assertTrue( "should be a relationship index", index.RelationshipIndex );
					assertEquals( asSet( PROPERTY_KEY, PROPERTY_KEY2 ), Iterables.asSet( index.PropertyKeys ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAll() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListAll()
		 {
			  // given
			  SchemaWrite schemaWrite = SchemaWriteInNewTransaction();
			  IndexReference index1 = schemaWrite.IndexCreate( _descriptor );
			  IndexReference index2 = ( ( IndexBackedConstraintDescriptor ) schemaWrite.UniquePropertyConstraintCreate( _descriptor2 ) ).ownedIndexDescriptor();
			  Commit();

			  // then/when
			  SchemaRead schemaRead = NewTransaction().schemaRead();
			  IList<IndexReference> indexes = Iterators.asList( schemaRead.IndexesGetAll() );
			  assertThat( indexes, containsInAnyOrder( index1, index2 ) );
			  Commit();
		 }

		 private ThreadStart CreateIndex( GraphDatabaseAPI db, Label label, string propertyKey )
		 {
			  return () =>
			  {
				using ( Neo4Net.GraphDb.Transaction transaction = Db.beginTx() )
				{
					 Db.schema().indexFor(label).on(propertyKey).create();
					 transaction.Success();
				}

				using ( Neo4Net.GraphDb.Transaction transaction = Db.beginTx() )
				{
					 Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					 transaction.Success();
				}
			  };
		 }
	}

}