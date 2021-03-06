﻿using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Kernel.Impl.Api.constraints
{
	using Test = org.junit.Test;


	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using IndexReference = Org.Neo4j.@internal.Kernel.Api.IndexReference;
	using Kernel = Org.Neo4j.@internal.Kernel.Api.Kernel;
	using SchemaRead = Org.Neo4j.@internal.Kernel.Api.SchemaRead;
	using SchemaWrite = Org.Neo4j.@internal.Kernel.Api.SchemaWrite;
	using TokenRead = Org.Neo4j.@internal.Kernel.Api.TokenRead;
	using Transaction = Org.Neo4j.@internal.Kernel.Api.Transaction;
	using InvalidTransactionTypeKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.InvalidTransactionTypeKernelException;
	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using IndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using SchemaKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.SchemaKernelException;
	using IndexProviderDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using LabelSchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using SchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptor;
	using LoginContext = Org.Neo4j.@internal.Kernel.Api.security.LoginContext;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using IndexEntryConflictException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexPopulationFailedKernelException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexPopulationFailedKernelException;
	using AlreadyConstrainedException = Org.Neo4j.Kernel.Api.Exceptions.schema.AlreadyConstrainedException;
	using UniquePropertyValueValidationException = Org.Neo4j.Kernel.Api.Exceptions.schema.UniquePropertyValueValidationException;
	using SchemaDescriptorFactory = Org.Neo4j.Kernel.api.schema.SchemaDescriptorFactory;
	using TestIndexDescriptorFactory = Org.Neo4j.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using TransactionState = Org.Neo4j.Kernel.api.txstate.TransactionState;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using IndexProxy = Org.Neo4j.Kernel.Impl.Api.index.IndexProxy;
	using IndexingService = Org.Neo4j.Kernel.Impl.Api.index.IndexingService;
	using ConstraintIndexCreator = Org.Neo4j.Kernel.Impl.Api.state.ConstraintIndexCreator;
	using ResourceTypes = Org.Neo4j.Kernel.impl.locking.ResourceTypes;
	using SimpleStatementLocks = Org.Neo4j.Kernel.impl.locking.SimpleStatementLocks;
	using TransactionHeaderInformationFactory = Org.Neo4j.Kernel.impl.transaction.TransactionHeaderInformationFactory;
	using AssertableLogProvider = Org.Neo4j.Logging.AssertableLogProvider;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using StorageEngine = Org.Neo4j.Storageengine.Api.StorageEngine;
	using StorageReader = Org.Neo4j.Storageengine.Api.StorageReader;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using IndexDescriptorFactory = Org.Neo4j.Storageengine.Api.schema.IndexDescriptorFactory;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class ConstraintIndexCreatorTest
	{
		private bool InstanceFieldsInitialized = false;

		public ConstraintIndexCreatorTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_schemaRead = _schemaRead();
		}

		 private const int PROPERTY_KEY_ID = 456;
		 private const int LABEL_ID = 123;
		 private const long INDEX_ID = 0L;

		 private readonly LabelSchemaDescriptor _descriptor = SchemaDescriptorFactory.forLabel( LABEL_ID, PROPERTY_KEY_ID );
		 private readonly IndexDescriptor _index = TestIndexDescriptorFactory.uniqueForLabel( LABEL_ID, PROPERTY_KEY_ID );
		 private readonly IndexReference _indexReference = TestIndexDescriptorFactory.uniqueForLabel( LABEL_ID, PROPERTY_KEY_ID );
		 private SchemaRead _schemaRead;
		 private readonly SchemaWrite _schemaWrite = mock( typeof( SchemaWrite ) );
		 private readonly TokenRead _tokenRead = mock( typeof( TokenRead ) );
		 private readonly AssertableLogProvider _logProvider = new AssertableLogProvider();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateIndexInAnotherTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateIndexInAnotherTransaction()
		 {
			  // given
			  StubKernel kernel = new StubKernel( this );
			  IndexProxy indexProxy = mock( typeof( IndexProxy ) );
			  IndexingService indexingService = mock( typeof( IndexingService ) );
			  when( indexingService.GetIndexProxy( INDEX_ID ) ).thenReturn( indexProxy );
			  when( indexingService.getIndexProxy( _descriptor ) ).thenReturn( indexProxy );
			  when( indexProxy.Descriptor ).thenReturn( _index.withId( INDEX_ID ).withoutCapabilities() );
			  NodePropertyAccessor nodePropertyAccessor = mock( typeof( NodePropertyAccessor ) );
			  ConstraintIndexCreator creator = new ConstraintIndexCreator( () => kernel, indexingService, nodePropertyAccessor, _logProvider );

			  // when
			  long indexId = creator.CreateUniquenessConstraintIndex( CreateTransaction(), _descriptor, DefaultProvider );

			  // then
			  assertEquals( INDEX_ID, indexId );
			  verify( _schemaRead ).indexGetCommittedId( _indexReference );
			  verify( _schemaRead ).index( _descriptor );
			  verifyNoMoreInteractions( _schemaRead );
			  verify( indexProxy ).awaitStoreScanCompleted( anyLong(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDropIndexIfPopulationFails() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDropIndexIfPopulationFails()
		 {
			  // given

			  StubKernel kernel = new StubKernel( this );

			  IndexingService indexingService = mock( typeof( IndexingService ) );
			  IndexProxy indexProxy = mock( typeof( IndexProxy ) );
			  when( indexingService.GetIndexProxy( INDEX_ID ) ).thenReturn( indexProxy );
			  when( indexingService.getIndexProxy( _descriptor ) ).thenReturn( indexProxy );
			  when( indexProxy.Descriptor ).thenReturn( _index.withId( INDEX_ID ).withoutCapabilities() );

			  IndexEntryConflictException cause = new IndexEntryConflictException( 2, 1, Values.of( "a" ) );
			  doThrow( new IndexPopulationFailedKernelException( "some index", cause ) ).when( indexProxy ).awaitStoreScanCompleted( anyLong(), any() );
			  NodePropertyAccessor nodePropertyAccessor = mock( typeof( NodePropertyAccessor ) );
			  when( _schemaRead.index( any( typeof( SchemaDescriptor ) ) ) ).thenReturn( IndexReference.NO_INDEX ).thenReturn( _indexReference ); // then after it failed claim it does exist
			  ConstraintIndexCreator creator = new ConstraintIndexCreator( () => kernel, indexingService, nodePropertyAccessor, _logProvider );

			  // when
			  KernelTransactionImplementation transaction = CreateTransaction();
			  try
			  {
					creator.CreateUniquenessConstraintIndex( transaction, _descriptor, DefaultProvider );

					fail( "expected exception" );
			  }
			  // then
			  catch ( UniquePropertyValueValidationException e )
			  {
					assertEquals( "Existing data does not satisfy CONSTRAINT ON ( label[123]:label[123] ) " + "ASSERT label[123].property[456] IS UNIQUE: Both node 2 and node 1 share the property value ( String(\"a\") )", e.Message );
			  }
			  assertEquals( 2, kernel.Transactions.Count );
			  KernelTransactionImplementation tx1 = kernel.Transactions[0];
			  SchemaDescriptor newIndex = _index.schema();
			  verify( tx1 ).indexUniqueCreate( eq( newIndex ), eq( DefaultProvider ) );
			  verify( _schemaRead ).indexGetCommittedId( _indexReference );
			  verify( _schemaRead, times( 2 ) ).index( _descriptor );
			  verifyNoMoreInteractions( _schemaRead );
			  TransactionState tx2 = kernel.Transactions[1].txState();
			  verify( tx2 ).indexDoDrop( _index );
			  verifyNoMoreInteractions( tx2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDropIndexInAnotherTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDropIndexInAnotherTransaction()
		 {
			  // given
			  StubKernel kernel = new StubKernel( this );
			  IndexingService indexingService = mock( typeof( IndexingService ) );

			  NodePropertyAccessor nodePropertyAccessor = mock( typeof( NodePropertyAccessor ) );
			  ConstraintIndexCreator creator = new ConstraintIndexCreator( () => kernel, indexingService, nodePropertyAccessor, _logProvider );

			  // when
			  creator.DropUniquenessConstraintIndex( _index );

			  // then
			  assertEquals( 1, kernel.Transactions.Count );
			  verify( kernel.Transactions[0].txState() ).indexDoDrop(_index);
			  verifyZeroInteractions( indexingService );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReleaseLabelLockWhileAwaitingIndexPopulation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReleaseLabelLockWhileAwaitingIndexPopulation()
		 {
			  // given
			  StubKernel kernel = new StubKernel( this );
			  IndexingService indexingService = mock( typeof( IndexingService ) );

			  NodePropertyAccessor nodePropertyAccessor = mock( typeof( NodePropertyAccessor ) );

			  when( _schemaRead.indexGetCommittedId( _indexReference ) ).thenReturn( INDEX_ID );
			  IndexProxy indexProxy = mock( typeof( IndexProxy ) );
			  when( indexingService.getIndexProxy( anyLong() ) ).thenReturn(indexProxy);
			  when( indexingService.getIndexProxy( _descriptor ) ).thenReturn( indexProxy );

			  when( _schemaRead.index( LABEL_ID, PROPERTY_KEY_ID ) ).thenReturn( IndexReference.NO_INDEX );

			  ConstraintIndexCreator creator = new ConstraintIndexCreator( () => kernel, indexingService, nodePropertyAccessor, _logProvider );

			  // when
			  KernelTransactionImplementation transaction = CreateTransaction();
			  creator.CreateUniquenessConstraintIndex( transaction, _descriptor, DefaultProvider );

			  // then
			  verify( transaction.StatementLocks().pessimistic() ).releaseExclusive(ResourceTypes.LABEL, _descriptor.LabelId);

			  verify( transaction.StatementLocks().pessimistic() ).acquireExclusive(transaction.LockTracer(), ResourceTypes.LABEL, _descriptor.LabelId);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReuseExistingOrphanedConstraintIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReuseExistingOrphanedConstraintIndex()
		 {
			  // given
			  IndexingService indexingService = mock( typeof( IndexingService ) );
			  StubKernel kernel = new StubKernel( this );

			  long orphanedConstraintIndexId = 111;
			  when( _schemaRead.indexGetCommittedId( _indexReference ) ).thenReturn( orphanedConstraintIndexId );
			  IndexProxy indexProxy = mock( typeof( IndexProxy ) );
			  when( indexingService.GetIndexProxy( orphanedConstraintIndexId ) ).thenReturn( indexProxy );
			  NodePropertyAccessor nodePropertyAccessor = mock( typeof( NodePropertyAccessor ) );
			  when( _schemaRead.index( _descriptor ) ).thenReturn( _indexReference );
			  when( _schemaRead.indexGetOwningUniquenessConstraintId( _indexReference ) ).thenReturn( null ); // which means it has no owner
			  ConstraintIndexCreator creator = new ConstraintIndexCreator( () => kernel, indexingService, nodePropertyAccessor, _logProvider );

			  // when
			  KernelTransactionImplementation transaction = CreateTransaction();
			  long indexId = creator.CreateUniquenessConstraintIndex( transaction, _descriptor, DefaultProvider );

			  // then
			  assertEquals( orphanedConstraintIndexId, indexId );
			  assertEquals( "There should have been no need to acquire a statement to create the constraint index", 0, kernel.Transactions.Count );
			  verify( _schemaRead ).indexGetCommittedId( _indexReference );
			  verify( _schemaRead ).index( _descriptor );
			  verify( _schemaRead ).indexGetOwningUniquenessConstraintId( _indexReference );
			  verifyNoMoreInteractions( _schemaRead );
			  verify( indexProxy ).awaitStoreScanCompleted( anyLong(), any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnExistingOwnedConstraintIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailOnExistingOwnedConstraintIndex()
		 {
			  // given
			  IndexingService indexingService = mock( typeof( IndexingService ) );
			  StubKernel kernel = new StubKernel( this );

			  long constraintIndexId = 111;
			  long constraintIndexOwnerId = 222;
			  when( _schemaRead.indexGetCommittedId( _indexReference ) ).thenReturn( constraintIndexId );
			  IndexProxy indexProxy = mock( typeof( IndexProxy ) );
			  when( indexingService.GetIndexProxy( constraintIndexId ) ).thenReturn( indexProxy );
			  NodePropertyAccessor nodePropertyAccessor = mock( typeof( NodePropertyAccessor ) );
			  when( _schemaRead.index( _descriptor ) ).thenReturn( _indexReference );
			  when( _schemaRead.indexGetOwningUniquenessConstraintId( _indexReference ) ).thenReturn( constraintIndexOwnerId ); // which means there's an owner
			  when( _tokenRead.nodeLabelName( LABEL_ID ) ).thenReturn( "MyLabel" );
			  when( _tokenRead.propertyKeyName( PROPERTY_KEY_ID ) ).thenReturn( "MyKey" );
			  ConstraintIndexCreator creator = new ConstraintIndexCreator( () => kernel, indexingService, nodePropertyAccessor, _logProvider );

			  // when
			  try
			  {
					KernelTransactionImplementation transaction = CreateTransaction();
					creator.CreateUniquenessConstraintIndex( transaction, _descriptor, DefaultProvider );
					fail( "Should've failed" );
			  }
			  catch ( AlreadyConstrainedException )
			  {
					// THEN good
			  }

			  // then
			  assertEquals( "There should have been no need to acquire a statement to create the constraint index", 0, kernel.Transactions.Count );
			  verify( _schemaRead ).index( _descriptor );
			  verify( _schemaRead ).indexGetOwningUniquenessConstraintId( _indexReference );
			  verifyNoMoreInteractions( _schemaRead );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateConstraintIndexForSpecifiedProvider() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateConstraintIndexForSpecifiedProvider()
		 {
			  // given
			  IndexingService indexingService = mock( typeof( IndexingService ) );
			  StubKernel kernel = new StubKernel( this );

			  when( _schemaRead.indexGetCommittedId( _indexReference ) ).thenReturn( INDEX_ID );
			  IndexProxy indexProxy = mock( typeof( IndexProxy ) );
			  when( indexingService.GetIndexProxy( INDEX_ID ) ).thenReturn( indexProxy );
			  when( indexingService.getIndexProxy( _descriptor ) ).thenReturn( indexProxy );
			  NodePropertyAccessor nodePropertyAccessor = mock( typeof( NodePropertyAccessor ) );
			  ConstraintIndexCreator creator = new ConstraintIndexCreator( () => kernel, indexingService, nodePropertyAccessor, _logProvider );
			  IndexProviderDescriptor providerDescriptor = new IndexProviderDescriptor( "Groovy", "1.2" );

			  // when
			  KernelTransactionImplementation transaction = CreateTransaction();
			  creator.CreateUniquenessConstraintIndex( transaction, _descriptor, providerDescriptor.Name() );

			  // then
			  assertEquals( 1, kernel.Transactions.Count );
			  KernelTransactionImplementation transactionInstance = kernel.Transactions[0];
			  verify( transactionInstance ).indexUniqueCreate( eq( _descriptor ), eq( providerDescriptor.Name() ) );
			  verify( _schemaRead ).index( _descriptor );
			  verify( _schemaRead ).indexGetCommittedId( any() );
			  verifyNoMoreInteractions( _schemaRead );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void logMessagesAboutConstraintCreation() throws org.neo4j.internal.kernel.api.exceptions.schema.SchemaKernelException, org.neo4j.kernel.api.exceptions.schema.UniquePropertyValueValidationException, org.neo4j.internal.kernel.api.exceptions.TransactionFailureException, org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LogMessagesAboutConstraintCreation()
		 {
			  StubKernel kernel = new StubKernel( this );
			  IndexProxy indexProxy = mock( typeof( IndexProxy ) );
			  IndexingService indexingService = mock( typeof( IndexingService ) );
			  when( indexingService.GetIndexProxy( INDEX_ID ) ).thenReturn( indexProxy );
			  when( indexingService.getIndexProxy( _descriptor ) ).thenReturn( indexProxy );
			  when( indexProxy.Descriptor ).thenReturn( _index.withId( INDEX_ID ).withoutCapabilities() );
			  NodePropertyAccessor propertyAccessor = mock( typeof( NodePropertyAccessor ) );
			  ConstraintIndexCreator creator = new ConstraintIndexCreator( () => kernel, indexingService, propertyAccessor, _logProvider );
			  KernelTransactionImplementation transaction = CreateTransaction();

			  creator.CreateUniquenessConstraintIndex( transaction, _descriptor, "indexProviderByName-1.0" );

			  _logProvider.rawMessageMatcher().assertContains("Starting constraint creation: %s.");
			  _logProvider.rawMessageMatcher().assertContains("Constraint %s populated, starting verification.");
			  _logProvider.rawMessageMatcher().assertContains("Constraint %s verified.");
		 }

		 private class StubKernel : Kernel
		 {
			 private readonly ConstraintIndexCreatorTest _outerInstance;

			 public StubKernel( ConstraintIndexCreatorTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal readonly IList<KernelTransactionImplementation> Transactions = new List<KernelTransactionImplementation>();

			  internal virtual KernelTransaction Remember( KernelTransactionImplementation kernelTransaction )
			  {
					Transactions.Add( kernelTransaction );
					return kernelTransaction;
			  }

			  public override Transaction BeginTransaction( Org.Neo4j.@internal.Kernel.Api.Transaction_Type type, LoginContext loginContext )
			  {
					return Remember( outerInstance.createTransaction() );
			  }
		 }

		 private SchemaRead SchemaRead()
		 {
			  SchemaRead schemaRead = mock( typeof( SchemaRead ) );
			  when( schemaRead.Index( _descriptor ) ).thenReturn( IndexReference.NO_INDEX );
			  try
			  {
					when( schemaRead.IndexGetCommittedId( _indexReference ) ).thenReturn( INDEX_ID );
			  }
			  catch ( SchemaKernelException e )
			  {
					throw new AssertionError( e );
			  }
			  return schemaRead;
		 }

		 private KernelTransactionImplementation CreateTransaction()
		 {
			  KernelTransactionImplementation transaction = mock( typeof( KernelTransactionImplementation ) );
			  try
			  {
					TransactionHeaderInformation headerInformation = new TransactionHeaderInformation( -1, -1, new sbyte[0] );
					TransactionHeaderInformationFactory headerInformationFactory = mock( typeof( TransactionHeaderInformationFactory ) );
					when( headerInformationFactory.Create() ).thenReturn(headerInformation);
					StorageEngine storageEngine = mock( typeof( StorageEngine ) );
					StorageReader storageReader = mock( typeof( StorageReader ) );
					when( storageEngine.NewReader() ).thenReturn(storageReader);

					SimpleStatementLocks locks = new SimpleStatementLocks( mock( typeof( Org.Neo4j.Kernel.impl.locking.Locks_Client ) ) );
					when( transaction.StatementLocks() ).thenReturn(locks);
					when( transaction.TokenRead() ).thenReturn(_tokenRead);
					when( transaction.SchemaRead() ).thenReturn(_schemaRead);
					when( transaction.SchemaWrite() ).thenReturn(_schemaWrite);
					TransactionState transactionState = mock( typeof( TransactionState ) );
					when( transaction.TxState() ).thenReturn(transactionState);
					when( transaction.IndexUniqueCreate( any( typeof( SchemaDescriptor ) ), any( typeof( string ) ) ) ).thenAnswer( i => IndexDescriptorFactory.uniqueForSchema( i.getArgument( 0 ) ) );
			  }
			  catch ( InvalidTransactionTypeKernelException )
			  {
					fail( "Expected write transaction" );
			  }
			  catch ( SchemaKernelException e )
			  {
					throw new Exception( e );
			  }
			  return transaction;
		 }

		 private static string DefaultProvider
		 {
			 get
			 {
				  return Config.defaults().get(GraphDatabaseSettings.default_schema_provider);
			 }
		 }
	}

}