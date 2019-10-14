using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.Kernel.Impl.Api.integrationtest
{
	using Test = org.junit.Test;

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Neo4Net.Graphdb;
	using SchemaWrite = Neo4Net.Internal.Kernel.Api.SchemaWrite;
	using TokenNameLookup = Neo4Net.Internal.Kernel.Api.TokenNameLookup;
	using TokenWrite = Neo4Net.Internal.Kernel.Api.TokenWrite;
	using Transaction = Neo4Net.Internal.Kernel.Api.Transaction;
	using KernelException = Neo4Net.Internal.Kernel.Api.exceptions.KernelException;
	using TransactionFailureException = Neo4Net.Internal.Kernel.Api.exceptions.TransactionFailureException;
	using ConstraintValidationException = Neo4Net.Internal.Kernel.Api.exceptions.schema.ConstraintValidationException;
	using CreateConstraintFailureException = Neo4Net.Internal.Kernel.Api.exceptions.schema.CreateConstraintFailureException;
	using LabelSchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using ConstraintDescriptor = Neo4Net.Internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using LoginContext = Neo4Net.Internal.Kernel.Api.security.LoginContext;
	using SilentTokenNameLookup = Neo4Net.Kernel.api.SilentTokenNameLookup;
	using DropConstraintFailureException = Neo4Net.Kernel.Api.Exceptions.schema.DropConstraintFailureException;
	using NoSuchConstraintException = Neo4Net.Kernel.Api.Exceptions.schema.NoSuchConstraintException;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using UniquenessConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.UniquenessConstraintDescriptor;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using AnonymousContext = Neo4Net.Kernel.api.security.AnonymousContext;
	using RecordStorageEngine = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using SchemaStorage = Neo4Net.Kernel.impl.store.SchemaStorage;
	using ConstraintRule = Neo4Net.Kernel.Impl.Store.Records.ConstraintRule;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.single;

	public class UniquenessConstraintCreationIT : AbstractConstraintCreationIT<ConstraintDescriptor, LabelSchemaDescriptor>
	{
		 private const string DUPLICATED_VALUE = "apa";
		 private IndexDescriptor _uniqueIndex;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: int initializeLabelOrRelType(org.neo4j.internal.kernel.api.TokenWrite tokenWrite, String name) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 internal override int InitializeLabelOrRelType( TokenWrite tokenWrite, string name )
		 {
			  return tokenWrite.LabelGetOrCreateForName( KEY );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ConstraintDescriptor createConstraint(org.neo4j.internal.kernel.api.SchemaWrite writeOps, org.neo4j.internal.kernel.api.schema.LabelSchemaDescriptor descriptor) throws Exception
		 internal override ConstraintDescriptor CreateConstraint( SchemaWrite writeOps, LabelSchemaDescriptor descriptor )
		 {
			  return writeOps.UniquePropertyConstraintCreate( descriptor );
		 }

		 internal override void CreateConstraintInRunningTx( GraphDatabaseService db, string type, string property )
		 {
			  SchemaHelper.createUniquenessConstraint( db, type, property );
		 }

		 internal override UniquenessConstraintDescriptor NewConstraintObject( LabelSchemaDescriptor descriptor )
		 {
			  return ConstraintDescriptorFactory.uniqueForSchema( descriptor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void dropConstraint(org.neo4j.internal.kernel.api.SchemaWrite writeOps, org.neo4j.internal.kernel.api.schema.constraints.ConstraintDescriptor constraint) throws Exception
		 internal override void DropConstraint( SchemaWrite writeOps, ConstraintDescriptor constraint )
		 {
			  writeOps.ConstraintDrop( constraint );
		 }

		 internal override void CreateOffendingDataInRunningTx( GraphDatabaseService db )
		 {
			  Db.createNode( label( KEY ) ).setProperty( PROP, DUPLICATED_VALUE );
			  Db.createNode( label( KEY ) ).setProperty( PROP, DUPLICATED_VALUE );
		 }

		 internal override void RemoveOffendingDataInRunningTx( GraphDatabaseService db )
		 {
			  using ( ResourceIterator<Node> nodes = Db.findNodes( label( KEY ), PROP, DUPLICATED_VALUE ) )
			  {
					while ( nodes.MoveNext() )
					{
						 nodes.Current.delete();
					}
			  }
		 }

		 internal override LabelSchemaDescriptor MakeDescriptor( int typeId, int propertyKeyId )
		 {
			  _uniqueIndex = TestIndexDescriptorFactory.uniqueForLabel( typeId, propertyKeyId );
			  return SchemaDescriptorFactory.forLabel( typeId, propertyKeyId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAbortConstraintCreationWhenDuplicatesExist() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAbortConstraintCreationWhenDuplicatesExist()
		 {
			  // given
			  Transaction transaction = newTransaction( AnonymousContext.writeToken() );
			  // name is not unique for Foo in the existing data

			  int foo = transaction.TokenWrite().labelGetOrCreateForName("Foo");
			  int name = transaction.TokenWrite().propertyKeyGetOrCreateForName("name");

			  long node1 = transaction.DataWrite().nodeCreate();

			  transaction.DataWrite().nodeAddLabel(node1, foo);
			  transaction.DataWrite().nodeSetProperty(node1, name, Values.of("foo"));

			  long node2 = transaction.DataWrite().nodeCreate();
			  transaction.DataWrite().nodeAddLabel(node2, foo);

			  transaction.DataWrite().nodeSetProperty(node2, name, Values.of("foo"));
			  commit();

			  // when
			  LabelSchemaDescriptor descriptor = SchemaDescriptorFactory.forLabel( foo, name );
			  try
			  {
					SchemaWrite schemaWriteOperations = schemaWriteInNewTransaction();
					schemaWriteOperations.UniquePropertyConstraintCreate( descriptor );

					fail( "expected exception" );
			  }
			  // then
			  catch ( CreateConstraintFailureException ex )
			  {
					assertEquals( ConstraintDescriptorFactory.uniqueForSchema( descriptor ), ex.Constraint() );
					Exception cause = ex.InnerException;
					assertThat( cause, instanceOf( typeof( ConstraintValidationException ) ) );

					string expectedMessage = string.Format( "Both Node({0:D}) and Node({1:D}) have the label `Foo` and property `name` = 'foo'", node1, node2 );
					string actualMessage = UserMessage( ( ConstraintValidationException ) cause );
					assertEquals( expectedMessage, actualMessage );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateAnIndexToGoAlongWithAUniquePropertyConstraint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateAnIndexToGoAlongWithAUniquePropertyConstraint()
		 {
			  // when
			  SchemaWrite schemaWriteOperations = schemaWriteInNewTransaction();
			  schemaWriteOperations.UniquePropertyConstraintCreate( Descriptor );
			  commit();

			  // then
			  Transaction transaction = newTransaction();
			  assertEquals( asSet( _uniqueIndex ), asSet( transaction.SchemaRead().indexesGetAll() ) );
			  commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDropCreatedConstraintIndexWhenRollingBackConstraintCreation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDropCreatedConstraintIndexWhenRollingBackConstraintCreation()
		 {
			  // given
			  Transaction transaction = newTransaction( LoginContext.AUTH_DISABLED );
			  transaction.SchemaWrite().uniquePropertyConstraintCreate(Descriptor);
			  assertEquals( asSet( _uniqueIndex ), asSet( transaction.SchemaRead().indexesGetAll() ) );

			  // when
			  rollback();

			  // then
			  transaction = newTransaction();
			  assertEquals( emptySet(), asSet(transaction.SchemaRead().indexesGetAll()) );
			  commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotDropUniquePropertyConstraintThatDoesNotExistWhenThereIsAPropertyExistenceConstraint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotDropUniquePropertyConstraintThatDoesNotExistWhenThereIsAPropertyExistenceConstraint()
		 {
			  // given
			  SchemaWrite schemaWriteOperations = schemaWriteInNewTransaction();
			  schemaWriteOperations.NodePropertyExistenceConstraintCreate( Descriptor );
			  commit();

			  // when
			  try
			  {
					SchemaWrite statement = schemaWriteInNewTransaction();
					statement.ConstraintDrop( ConstraintDescriptorFactory.uniqueForSchema( Descriptor ) );

					fail( "expected exception" );
			  }
			  // then
			  catch ( DropConstraintFailureException e )
			  {
					assertThat( e.InnerException, instanceOf( typeof( NoSuchConstraintException ) ) );
			  }
			  finally
			  {
					rollback();
			  }

			  {
			  // then
					Transaction transaction = newTransaction();

					IEnumerator<ConstraintDescriptor> constraints = transaction.SchemaRead().constraintsGetForSchema(Descriptor);

					assertEquals( ConstraintDescriptorFactory.existsForSchema( Descriptor ), single( constraints ) );
					commit();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void committedConstraintRuleShouldCrossReferenceTheCorrespondingIndexRule() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CommittedConstraintRuleShouldCrossReferenceTheCorrespondingIndexRule()
		 {
			  // when
			  SchemaWrite statement = schemaWriteInNewTransaction();
			  statement.UniquePropertyConstraintCreate( Descriptor );
			  commit();

			  // then
			  SchemaStorage schema = new SchemaStorage( NeoStores().SchemaStore );
			  StoreIndexDescriptor indexRule = Schema.indexGetForSchema( TestIndexDescriptorFactory.uniqueForLabel( TypeId, PropertyKeyId ) );
			  ConstraintRule constraintRule = Schema.constraintsGetSingle( ConstraintDescriptorFactory.uniqueForLabel( TypeId, PropertyKeyId ) );
			  assertEquals( constraintRule.Id, indexRule.OwningConstraint.Value );
			  assertEquals( indexRule.Id, constraintRule.OwnedIndex );
		 }

		 private NeoStores NeoStores()
		 {
			  return Db.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) ).testAccessNeoStores();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDropConstraintIndexWhenDroppingConstraint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDropConstraintIndexWhenDroppingConstraint()
		 {
			  // given
			  Transaction transaction = newTransaction( LoginContext.AUTH_DISABLED );
			  ConstraintDescriptor constraint = transaction.SchemaWrite().uniquePropertyConstraintCreate(Descriptor);
			  assertEquals( asSet( _uniqueIndex ), asSet( transaction.SchemaRead().indexesGetAll() ) );
			  commit();

			  // when
			  SchemaWrite schemaWriteOperations = schemaWriteInNewTransaction();
			  schemaWriteOperations.ConstraintDrop( constraint );
			  commit();

			  // then
			  transaction = newTransaction();
			  assertEquals( emptySet(), asSet(transaction.SchemaRead().indexesGetAll()) );
			  commit();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String userMessage(org.neo4j.internal.kernel.api.exceptions.schema.ConstraintValidationException cause) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 private string UserMessage( ConstraintValidationException cause )
		 {
			  using ( Transaction tx = newTransaction() )
			  {
					TokenNameLookup lookup = new SilentTokenNameLookup( tx.TokenRead() );
					return cause.GetUserMessage( lookup );
			  }
		 }
	}

}