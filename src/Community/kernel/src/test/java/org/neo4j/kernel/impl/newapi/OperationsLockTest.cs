using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Impl.Newapi
{
	using Matchers = org.hamcrest.Matchers;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using InOrder = org.mockito.InOrder;
	using Mockito = org.mockito.Mockito;


	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using LabelSet = Neo4Net.@internal.Kernel.Api.LabelSet;
	using NamedToken = Neo4Net.@internal.Kernel.Api.NamedToken;
	using Write = Neo4Net.@internal.Kernel.Api.Write;
	using EntityNotFoundException = Neo4Net.@internal.Kernel.Api.exceptions.EntityNotFoundException;
	using InvalidTransactionTypeKernelException = Neo4Net.@internal.Kernel.Api.exceptions.InvalidTransactionTypeKernelException;
	using KernelException = Neo4Net.@internal.Kernel.Api.exceptions.KernelException;
	using AutoIndexingKernelException = Neo4Net.@internal.Kernel.Api.exceptions.explicitindex.AutoIndexingKernelException;
	using StubNodeCursor = Neo4Net.@internal.Kernel.Api.helpers.StubNodeCursor;
	using TestRelationshipChain = Neo4Net.@internal.Kernel.Api.helpers.TestRelationshipChain;
	using LabelSchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using ConstraintDescriptor = Neo4Net.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using SecurityContext = Neo4Net.@internal.Kernel.Api.security.SecurityContext;
	using AlreadyConstrainedException = Neo4Net.Kernel.Api.Exceptions.schema.AlreadyConstrainedException;
	using AutoIndexOperations = Neo4Net.Kernel.api.explicitindex.AutoIndexOperations;
	using AutoIndexing = Neo4Net.Kernel.api.explicitindex.AutoIndexing;
	using RelationTypeSchemaDescriptor = Neo4Net.Kernel.api.schema.RelationTypeSchemaDescriptor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using NodeExistenceConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.NodeExistenceConstraintDescriptor;
	using NodeKeyConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.NodeKeyConstraintDescriptor;
	using RelExistenceConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.RelExistenceConstraintDescriptor;
	using UniquenessConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.UniquenessConstraintDescriptor;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using TransactionState = Neo4Net.Kernel.api.txstate.TransactionState;
	using Config = Neo4Net.Kernel.configuration.Config;
	using KernelTransactionImplementation = Neo4Net.Kernel.Impl.Api.KernelTransactionImplementation;
	using SchemaState = Neo4Net.Kernel.Impl.Api.SchemaState;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using ConstraintIndexCreator = Neo4Net.Kernel.Impl.Api.state.ConstraintIndexCreator;
	using TxState = Neo4Net.Kernel.Impl.Api.state.TxState;
	using ConstraintSemantics = Neo4Net.Kernel.impl.constraints.ConstraintSemantics;
	using TokenHolders = Neo4Net.Kernel.impl.core.TokenHolders;
	using ExplicitIndexStore = Neo4Net.Kernel.impl.index.ExplicitIndexStore;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using ResourceTypes = Neo4Net.Kernel.impl.locking.ResourceTypes;
	using SimpleStatementLocks = Neo4Net.Kernel.impl.locking.SimpleStatementLocks;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using StorageEngine = Neo4Net.Storageengine.Api.StorageEngine;
	using StorageReader = Neo4Net.Storageengine.Api.StorageReader;
	using StorageSchemaReader = Neo4Net.Storageengine.Api.StorageSchemaReader;
	using LockTracer = Neo4Net.Storageengine.Api.@lock.LockTracer;
	using CapableIndexDescriptor = Neo4Net.Storageengine.Api.schema.CapableIndexDescriptor;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.reset;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongCollections.EMPTY_LONG_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.default_schema_provider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.constraints.ConstraintDescriptorFactory.existsForRelType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.constraints.ConstraintDescriptorFactory.existsForSchema;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.constraints.ConstraintDescriptorFactory.nodeKeyForSchema;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.constraints.ConstraintDescriptorFactory.uniqueForLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.constraints.ConstraintDescriptorFactory.uniqueForSchema;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.TwoPhaseNodeForRelationshipLockingTest.returnRelationships;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.MockedNeoStores.mockedTokenHolders;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.NO_VALUE;

	public class OperationsLockTest
	{
		 private KernelTransactionImplementation _transaction = mock( typeof( KernelTransactionImplementation ) );
		 private Operations _operations;
		 private readonly Neo4Net.Kernel.impl.locking.Locks_Client _locks = mock( typeof( Neo4Net.Kernel.impl.locking.Locks_Client ) );
		 private readonly Write _write = mock( typeof( Write ) );
		 private InOrder _order;
		 private DefaultNodeCursor _nodeCursor;
		 private DefaultPropertyCursor _propertyCursor;
		 private DefaultRelationshipScanCursor _relationshipCursor;
		 private TransactionState _txState;
		 private AllStoreHolder _allStoreHolder;
		 private readonly LabelSchemaDescriptor _descriptor = SchemaDescriptorFactory.forLabel( 123, 456 );
		 private StorageReader _storageReader;
		 private StorageSchemaReader _storageReaderSnapshot;
		 private ConstraintIndexCreator _constraintIndexCreator;
		 private TokenHolders _tokenHolders;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws org.neo4j.internal.kernel.api.exceptions.InvalidTransactionTypeKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  _txState = Mockito.spy( new TxState() );
			  when( _transaction.ReasonIfTerminated ).thenReturn( null );
			  when( _transaction.statementLocks() ).thenReturn(new SimpleStatementLocks(_locks));
			  when( _transaction.dataWrite() ).thenReturn(_write);
			  when( _transaction.Open ).thenReturn( true );
			  when( _transaction.lockTracer() ).thenReturn(LockTracer.NONE);
			  when( _transaction.txState() ).thenReturn(_txState);
			  when( _transaction.securityContext() ).thenReturn(SecurityContext.AUTH_DISABLED);

			  DefaultCursors cursors = mock( typeof( DefaultCursors ) );
			  _nodeCursor = mock( typeof( DefaultNodeCursor ) );
			  _propertyCursor = mock( typeof( DefaultPropertyCursor ) );
			  _relationshipCursor = mock( typeof( DefaultRelationshipScanCursor ) );
			  when( cursors.AllocateNodeCursor() ).thenReturn(_nodeCursor);
			  when( cursors.AllocatePropertyCursor() ).thenReturn(_propertyCursor);
			  when( cursors.AllocateRelationshipScanCursor() ).thenReturn(_relationshipCursor);
			  AutoIndexing autoindexing = mock( typeof( AutoIndexing ) );
			  AutoIndexOperations autoIndexOperations = mock( typeof( AutoIndexOperations ) );
			  when( autoindexing.Nodes() ).thenReturn(autoIndexOperations);
			  when( autoindexing.Relationships() ).thenReturn(autoIndexOperations);
			  StorageEngine engine = mock( typeof( StorageEngine ) );
			  _storageReader = mock( typeof( StorageReader ) );
			  _storageReaderSnapshot = mock( typeof( StorageSchemaReader ) );
			  when( _storageReader.nodeExists( anyLong() ) ).thenReturn(true);
			  when( _storageReader.constraintsGetForLabel( anyInt() ) ).thenReturn(Collections.emptyIterator());
			  when( _storageReader.constraintsGetAll() ).thenReturn(Collections.emptyIterator());
			  when( _storageReader.schemaSnapshot() ).thenReturn(_storageReaderSnapshot);
			  when( engine.NewReader() ).thenReturn(_storageReader);
			  _allStoreHolder = new AllStoreHolder( _storageReader, _transaction, cursors, mock( typeof( ExplicitIndexStore ) ), mock( typeof( Procedures ) ), mock( typeof( SchemaState ) ), new Dependencies() );
			  _constraintIndexCreator = mock( typeof( ConstraintIndexCreator ) );
			  _tokenHolders = mockedTokenHolders();
			  _operations = new Operations( _allStoreHolder, mock( typeof( IndexTxStateUpdater ) ), _storageReader, _transaction, new KernelToken( _storageReader, _transaction, _tokenHolders ), cursors, autoindexing, _constraintIndexCreator, mock( typeof( ConstraintSemantics ) ), mock( typeof( IndexingService ) ), Config.defaults() );
			  _operations.initialize();

			  this._order = inOrder( _locks, _txState, _storageReader, _storageReaderSnapshot );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _operations.release();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcquireEntityWriteLockCreatingRelationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcquireEntityWriteLockCreatingRelationship()
		 {
			  // when
			  long rId = _operations.relationshipCreate( 1, 2, 3 );

			  // then
			  _order.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.NODE, 1 );
			  _order.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.NODE, 3 );
			  _order.verify( _txState ).relationshipDoCreate( rId, 2, 1, 3 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcquireNodeLocksWhenCreatingRelationshipInOrderOfAscendingId() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcquireNodeLocksWhenCreatingRelationshipInOrderOfAscendingId()
		 {
			  // GIVEN
			  long lowId = 3;
			  long highId = 5;
			  int relationshipLabel = 0;

			  {
					// WHEN
					_operations.relationshipCreate( lowId, relationshipLabel, highId );

					// THEN
					InOrder lockingOrder = inOrder( _locks );
					lockingOrder.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.NODE, lowId );
					lockingOrder.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.NODE, highId );
					lockingOrder.verifyNoMoreInteractions();
					reset( _locks );
			  }

			  {
					// WHEN
					_operations.relationshipCreate( highId, relationshipLabel, lowId );

					// THEN
					InOrder lockingOrder = inOrder( _locks );
					lockingOrder.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.NODE, lowId );
					lockingOrder.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.NODE, highId );
					lockingOrder.verifyNoMoreInteractions();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcquireNodeLocksWhenDeletingRelationshipInOrderOfAscendingId() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcquireNodeLocksWhenDeletingRelationshipInOrderOfAscendingId()
		 {
			  // GIVEN
			  const long relationshipId = 10;
			  const long lowId = 3;
			  const long highId = 5;
			  int relationshipLabel = 0;

			  {
					// and GIVEN
					SetStoreRelationship( relationshipId, lowId, highId, relationshipLabel );

					// WHEN
					_operations.relationshipDelete( relationshipId );

					// THEN
					InOrder lockingOrder = inOrder( _locks );
					lockingOrder.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.NODE, lowId );
					lockingOrder.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.NODE, highId );
					lockingOrder.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.RELATIONSHIP, relationshipId );
					lockingOrder.verifyNoMoreInteractions();
					reset( _locks );
			  }

			  {
					// and GIVEN
					SetStoreRelationship( relationshipId, highId, lowId, relationshipLabel );

					// WHEN
					_operations.relationshipDelete( relationshipId );

					// THEN
					InOrder lockingOrder = inOrder( _locks );
					lockingOrder.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.NODE, lowId );
					lockingOrder.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.NODE, highId );
					lockingOrder.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.RELATIONSHIP, relationshipId );
					lockingOrder.verifyNoMoreInteractions();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcquireEntityWriteLockBeforeAddingLabelToNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcquireEntityWriteLockBeforeAddingLabelToNode()
		 {
			  // given
			  when( _nodeCursor.next() ).thenReturn(true);
			  when( _nodeCursor.labels() ).thenReturn(LabelSet.NONE);

			  // when
			  _operations.nodeAddLabel( 123L, 456 );

			  // then
			  _order.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.NODE, 123L );
			  _order.verify( _txState ).nodeDoAddLabel( 456, 123L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAcquireEntityWriteLockBeforeAddingLabelToJustCreatedNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAcquireEntityWriteLockBeforeAddingLabelToJustCreatedNode()
		 {
			  // given
			  when( _nodeCursor.next() ).thenReturn(true);
			  when( _nodeCursor.labels() ).thenReturn(LabelSet.NONE);
			  when( _transaction.hasTxStateWithChanges() ).thenReturn(true);

			  // when
			  _txState.nodeDoCreate( 123 );
			  _operations.nodeAddLabel( 123, 456 );

			  // then
			  verify( _locks, never() ).acquireExclusive(LockTracer.NONE, ResourceTypes.NODE, 123);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcquireSchemaReadLockBeforeAddingLabelToNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcquireSchemaReadLockBeforeAddingLabelToNode()
		 {
			  // given
			  when( _nodeCursor.next() ).thenReturn(true);
			  when( _nodeCursor.labels() ).thenReturn(LabelSet.NONE);

			  // when
			  int labelId = 456;
			  _operations.nodeAddLabel( 123, labelId );

			  // then
			  _order.verify( _locks ).acquireShared( LockTracer.NONE, ResourceTypes.LABEL, labelId );
			  _order.verify( _txState ).nodeDoAddLabel( labelId, 123 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcquireEntityWriteLockBeforeSettingPropertyOnNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcquireEntityWriteLockBeforeSettingPropertyOnNode()
		 {
			  // given
			  when( _nodeCursor.next() ).thenReturn(true);
			  when( _nodeCursor.labels() ).thenReturn(LabelSet.NONE);
			  int propertyKeyId = 8;
			  Value value = Values.of( 9 );
			  when( _propertyCursor.next() ).thenReturn(true);
			  when( _propertyCursor.propertyKey() ).thenReturn(propertyKeyId);
			  when( _propertyCursor.propertyValue() ).thenReturn(NO_VALUE);

			  // when
			  _operations.nodeSetProperty( 123, propertyKeyId, value );

			  // then
			  _order.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.NODE, 123 );
			  _order.verify( _txState ).nodeDoAddProperty( 123, propertyKeyId, value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcquireSchemaReadLockBeforeSettingPropertyOnNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcquireSchemaReadLockBeforeSettingPropertyOnNode()
		 {
			  // given
			  int relatedLabelId = 50;
			  int unrelatedLabelId = 51;
			  int propertyKeyId = 8;
			  when( _nodeCursor.next() ).thenReturn(true);
			  LabelSet labelSet = mock( typeof( LabelSet ) );
			  when( labelSet.All() ).thenReturn(new long[]{ relatedLabelId });
			  when( _nodeCursor.labels() ).thenReturn(labelSet);
			  Value value = Values.of( 9 );
			  when( _propertyCursor.next() ).thenReturn(true);
			  when( _propertyCursor.propertyKey() ).thenReturn(propertyKeyId);
			  when( _propertyCursor.propertyValue() ).thenReturn(NO_VALUE);

			  // when
			  _operations.nodeSetProperty( 123, propertyKeyId, value );

			  // then
			  _order.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.NODE, 123 );
			  _order.verify( _locks ).acquireShared( LockTracer.NONE, ResourceTypes.LABEL, relatedLabelId );
			  _order.verify( _locks, never() ).acquireShared(LockTracer.NONE, ResourceTypes.LABEL, unrelatedLabelId);
			  _order.verify( _txState ).nodeDoAddProperty( 123, propertyKeyId, value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcquireEntityWriteLockBeforeSettingPropertyOnRelationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcquireEntityWriteLockBeforeSettingPropertyOnRelationship()
		 {
			  // given
			  when( _relationshipCursor.next() ).thenReturn(true);
			  int propertyKeyId = 8;
			  Value value = Values.of( 9 );
			  when( _propertyCursor.next() ).thenReturn(true);
			  when( _propertyCursor.propertyKey() ).thenReturn(propertyKeyId);
			  when( _propertyCursor.propertyValue() ).thenReturn(NO_VALUE);

			  // when
			  _operations.relationshipSetProperty( 123, propertyKeyId, value );

			  // then
			  _order.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.RELATIONSHIP, 123 );
			  _order.verify( _txState ).relationshipDoReplaceProperty( 123, propertyKeyId, NO_VALUE, value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAcquireEntityWriteLockBeforeSettingPropertyOnJustCreatedNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAcquireEntityWriteLockBeforeSettingPropertyOnJustCreatedNode()
		 {
			  // given
			  when( _nodeCursor.next() ).thenReturn(true);
			  when( _nodeCursor.labels() ).thenReturn(LabelSet.NONE);
			  when( _transaction.hasTxStateWithChanges() ).thenReturn(true);
			  _txState.nodeDoCreate( 123 );
			  int propertyKeyId = 8;
			  Value value = Values.of( 9 );

			  // when
			  _operations.nodeSetProperty( 123, propertyKeyId, value );

			  // then
			  verify( _locks, never() ).acquireExclusive(LockTracer.NONE, ResourceTypes.NODE, 123);
			  verify( _txState ).nodeDoAddProperty( 123, propertyKeyId, value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAcquireEntityWriteLockBeforeSettingPropertyOnJustCreatedRelationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAcquireEntityWriteLockBeforeSettingPropertyOnJustCreatedRelationship()
		 {
			  // given
			  when( _relationshipCursor.next() ).thenReturn(true);
			  when( _transaction.hasTxStateWithChanges() ).thenReturn(true);
			  _txState.relationshipDoCreate( 123, 42, 43, 45 );
			  int propertyKeyId = 8;
			  Value value = Values.of( 9 );

			  // when
			  _operations.relationshipSetProperty( 123, propertyKeyId, value );

			  // then
			  verify( _locks, never() ).acquireExclusive(LockTracer.NONE, ResourceTypes.RELATIONSHIP, 123);
			  verify( _txState ).relationshipDoReplaceProperty( 123, propertyKeyId, NO_VALUE, value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcquireEntityWriteLockBeforeDeletingNode() throws org.neo4j.internal.kernel.api.exceptions.explicitindex.AutoIndexingKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcquireEntityWriteLockBeforeDeletingNode()
		 {
			  // GIVEN
			  when( _nodeCursor.next() ).thenReturn(true);
			  when( _nodeCursor.labels() ).thenReturn(LabelSet.NONE);
			  when( _allStoreHolder.nodeExistsInStore( 123 ) ).thenReturn( true );

			  // WHEN
			  _operations.nodeDelete( 123 );

			  //THEN
			  _order.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.NODE, 123 );
			  _order.verify( _txState ).nodeDoDelete( 123 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAcquireEntityWriteLockBeforeDeletingJustCreatedNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAcquireEntityWriteLockBeforeDeletingJustCreatedNode()
		 {
			  // THEN
			  _txState.nodeDoCreate( 123 );
			  when( _transaction.hasTxStateWithChanges() ).thenReturn(true);

			  // WHEN
			  _operations.nodeDelete( 123 );

			  //THEN
			  verify( _locks, never() ).acquireExclusive(LockTracer.NONE, ResourceTypes.NODE, 123);
			  verify( _txState ).nodeDoDelete( 123 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcquireSchemaReadLockBeforeGettingConstraintsByLabelAndProperty()
		 public virtual void ShouldAcquireSchemaReadLockBeforeGettingConstraintsByLabelAndProperty()
		 {
			  // WHEN
			  _allStoreHolder.constraintsGetForSchema( _descriptor );

			  // THEN
			  _order.verify( _locks ).acquireShared( LockTracer.NONE, ResourceTypes.LABEL, _descriptor.LabelId );
			  _order.verify( _storageReader ).constraintsGetForSchema( _descriptor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAcquireSchemaReadLockBeforeGettingIndexesByLabelAndProperty()
		 public virtual void ShouldNotAcquireSchemaReadLockBeforeGettingIndexesByLabelAndProperty()
		 {
			  // WHEN
			  _allStoreHolder.index( _descriptor );

			  // THEN
			  verifyNoMoreInteractions( _locks );
			  verify( _storageReader ).indexGetForSchema( _descriptor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAcquireSchemaReadLockWhenGettingIndexesByLabelAndPropertyFromSnapshot()
		 public virtual void ShouldNotAcquireSchemaReadLockWhenGettingIndexesByLabelAndPropertyFromSnapshot()
		 {
			  // WHEN
			  _allStoreHolder.snapshot().index(_descriptor);

			  // THEN
			  verifyNoMoreInteractions( _locks );
			  verify( _storageReaderSnapshot ).indexGetForSchema( _descriptor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcquireSchemaReadLockBeforeGettingConstraintsByLabel()
		 public virtual void ShouldAcquireSchemaReadLockBeforeGettingConstraintsByLabel()
		 {
			  // WHEN
			  _allStoreHolder.constraintsGetForLabel( 42 );

			  // THEN
			  _order.verify( _locks ).acquireShared( LockTracer.NONE, ResourceTypes.LABEL, 42 );
			  _order.verify( _storageReader ).constraintsGetForLabel( 42 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcquireSchemaReadLockBeforeGettingConstraintsByRelationshipType()
		 public virtual void ShouldAcquireSchemaReadLockBeforeGettingConstraintsByRelationshipType()
		 {
			  // WHEN
			  _allStoreHolder.constraintsGetForRelationshipType( 42 );

			  // THEN
			  _order.verify( _locks ).acquireShared( LockTracer.NONE, ResourceTypes.RELATIONSHIP_TYPE, 42 );
			  _order.verify( _storageReader ).constraintsGetForRelationshipType( 42 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAcquireSchemaReadLockBeforeGettingConstraintsByLabel()
		 public virtual void ShouldNotAcquireSchemaReadLockBeforeGettingConstraintsByLabel()
		 {
			  // WHEN
			  _allStoreHolder.snapshot().constraintsGetForLabel(42);

			  // THEN
			  verifyNoMoreInteractions( _locks );
			  verify( _storageReaderSnapshot ).constraintsGetForLabel( 42 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAcquireSchemaReadLockBeforeGettingConstraintsByRelationshipType()
		 public virtual void ShouldNotAcquireSchemaReadLockBeforeGettingConstraintsByRelationshipType()
		 {
			  // WHEN
			  _allStoreHolder.snapshot().constraintsGetForRelationshipType(42);

			  // THEN
			  verifyNoMoreInteractions( _locks );
			  verify( _storageReaderSnapshot ).constraintsGetForRelationshipType( 42 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcquireSchemaReadLockBeforeCheckingExistenceConstraints()
		 public virtual void ShouldAcquireSchemaReadLockBeforeCheckingExistenceConstraints()
		 {
			  // WHEN
			  _allStoreHolder.constraintExists( ConstraintDescriptorFactory.uniqueForSchema( _descriptor ) );

			  // THEN
			  _order.verify( _locks ).acquireShared( LockTracer.NONE, ResourceTypes.LABEL, 123 );
			  _order.verify( _storageReader ).constraintExists( any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcquireSchemaReadLockLazilyBeforeGettingAllConstraints()
		 public virtual void ShouldAcquireSchemaReadLockLazilyBeforeGettingAllConstraints()
		 {
			  // given
			  int labelId = 1;
			  int relTypeId = 2;
			  UniquenessConstraintDescriptor uniquenessConstraint = uniqueForLabel( labelId, 2, 3, 3 );
			  RelExistenceConstraintDescriptor existenceConstraint = existsForRelType( relTypeId, 3, 4, 5 );
			  when( _storageReader.constraintsGetAll() ).thenReturn(Iterators.iterator(uniquenessConstraint, existenceConstraint));

			  // when
			  IEnumerator<ConstraintDescriptor> result = _allStoreHolder.constraintsGetAll();

			  // then
			  assertThat( Iterators.count( result ), Matchers.@is( 2L ) );
			  assertThat( asList( result ), empty() );
			  _order.verify( _storageReader ).constraintsGetAll();
			  _order.verify( _locks ).acquireShared( LockTracer.NONE, ResourceTypes.LABEL, labelId );
			  _order.verify( _locks ).acquireShared( LockTracer.NONE, ResourceTypes.RELATIONSHIP_TYPE, relTypeId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAcquireSchemaReadLockLazilyBeforeGettingAllConstraintsFromSnapshot()
		 public virtual void ShouldNotAcquireSchemaReadLockLazilyBeforeGettingAllConstraintsFromSnapshot()
		 {
			  // given
			  int labelId = 1;
			  int relTypeId = 2;
			  UniquenessConstraintDescriptor uniquenessConstraint = uniqueForLabel( labelId, 2, 3, 3 );
			  RelExistenceConstraintDescriptor existenceConstraint = existsForRelType( relTypeId, 3, 4, 5 );
			  when( _storageReaderSnapshot.constraintsGetAll() ).thenReturn(Iterators.iterator(uniquenessConstraint, existenceConstraint));

			  // when
			  IEnumerator<ConstraintDescriptor> result = _allStoreHolder.snapshot().constraintsGetAll();

			  // then
			  assertThat( Iterators.count( result ), Matchers.@is( 2L ) );
			  assertThat( asList( result ), empty() );
			  verify( _storageReaderSnapshot ).constraintsGetAll();
			  verifyNoMoreInteractions( _locks );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcquireSchemaWriteLockBeforeRemovingIndexRule() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcquireSchemaWriteLockBeforeRemovingIndexRule()
		 {
			  // given
			  CapableIndexDescriptor index = TestIndexDescriptorFactory.forLabel( 0, 0 ).withId( 0 ).withoutCapabilities();
			  when( _storageReader.indexGetForSchema( any() ) ).thenReturn(index);

			  // when
			  _operations.indexDrop( index );

			  // then
			  _order.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.LABEL, 0 );
			  _order.verify( _txState ).indexDoDrop( index );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcquireSchemaWriteLockBeforeCreatingUniquenessConstraint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcquireSchemaWriteLockBeforeCreatingUniquenessConstraint()
		 {
			  // given
			  string defaultProvider = Config.defaults().get(default_schema_provider);
			  when( _constraintIndexCreator.createUniquenessConstraintIndex( _transaction, _descriptor, defaultProvider ) ).thenReturn( 42L );
			  when( _storageReader.constraintsGetForSchema( _descriptor.schema() ) ).thenReturn(Collections.emptyIterator());

			  // when
			  _operations.uniquePropertyConstraintCreate( _descriptor );

			  // then
			  _order.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.LABEL, _descriptor.LabelId );
			  _order.verify( _txState ).constraintDoAdd( ConstraintDescriptorFactory.uniqueForSchema( _descriptor ), 42L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReleaseAcquiredSchemaWriteLockIfConstraintCreationFails() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReleaseAcquiredSchemaWriteLockIfConstraintCreationFails()
		 {
			  // given
			  UniquenessConstraintDescriptor constraint = uniqueForSchema( _descriptor );
			  when( _storageReader.constraintExists( constraint ) ).thenReturn( true );
			  int labelId = _descriptor.LabelId;
			  int propertyId = _descriptor.PropertyId;
			  when( _tokenHolders.labelTokens().getTokenById(labelId) ).thenReturn(new NamedToken("Label", labelId));
			  when( _tokenHolders.propertyKeyTokens().getTokenById(propertyId) ).thenReturn(new NamedToken("prop", labelId));

			  // when
			  try
			  {
					_operations.uniquePropertyConstraintCreate( _descriptor );
					fail( "Expected an exception because this schema should already be constrained." );
			  }
			  catch ( AlreadyConstrainedException )
			  {
					// Good.
			  }

			  // then
			  _order.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.LABEL, labelId );
			  _order.verify( _storageReader ).constraintExists( constraint );
			  _order.verify( _locks ).releaseExclusive( ResourceTypes.LABEL, labelId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReleaseAcquiredSchemaWriteLockIfConstraintWithIndexProviderCreationFails() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReleaseAcquiredSchemaWriteLockIfConstraintWithIndexProviderCreationFails()
		 {
			  // given
			  string indexProvider = Config.defaults().get(default_schema_provider);
			  UniquenessConstraintDescriptor constraint = uniqueForSchema( _descriptor );
			  when( _storageReader.constraintExists( constraint ) ).thenReturn( true );
			  int labelId = _descriptor.LabelId;
			  int propertyId = _descriptor.PropertyId;
			  when( _tokenHolders.labelTokens().getTokenById(labelId) ).thenReturn(new NamedToken("Label", labelId));
			  when( _tokenHolders.propertyKeyTokens().getTokenById(propertyId) ).thenReturn(new NamedToken("prop", labelId));

			  // when
			  try
			  {
					_operations.uniquePropertyConstraintCreate( _descriptor, indexProvider );
					fail( "Expected an exception because this schema should already be constrained." );
			  }
			  catch ( AlreadyConstrainedException )
			  {
					// Good.
			  }

			  // then
			  _order.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.LABEL, labelId );
			  _order.verify( _storageReader ).constraintExists( constraint );
			  _order.verify( _locks ).releaseExclusive( ResourceTypes.LABEL, labelId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReleaseAcquiredSchemaWriteLockIfNodeKeyConstraintCreationFails() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReleaseAcquiredSchemaWriteLockIfNodeKeyConstraintCreationFails()
		 {
			  // given
			  NodeKeyConstraintDescriptor constraint = nodeKeyForSchema( _descriptor );
			  when( _storageReader.constraintExists( constraint ) ).thenReturn( true );
			  int labelId = _descriptor.LabelId;
			  int propertyId = _descriptor.PropertyId;
			  when( _tokenHolders.labelTokens().getTokenById(labelId) ).thenReturn(new NamedToken("Label", labelId));
			  when( _tokenHolders.propertyKeyTokens().getTokenById(propertyId) ).thenReturn(new NamedToken("prop", labelId));

			  // when
			  try
			  {
					_operations.nodeKeyConstraintCreate( _descriptor );
					fail( "Expected an exception because this schema should already be constrained." );
			  }
			  catch ( AlreadyConstrainedException )
			  {
					// Good.
			  }

			  // then
			  _order.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.LABEL, labelId );
			  _order.verify( _storageReader ).constraintExists( constraint );
			  _order.verify( _locks ).releaseExclusive( ResourceTypes.LABEL, labelId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReleaseAcquiredSchemaWriteLockIfNodeKeyConstraintWithIndexProviderCreationFails() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReleaseAcquiredSchemaWriteLockIfNodeKeyConstraintWithIndexProviderCreationFails()
		 {
			  // given
			  string indexProvider = Config.defaults().get(default_schema_provider);
			  NodeKeyConstraintDescriptor constraint = nodeKeyForSchema( _descriptor );
			  when( _storageReader.constraintExists( constraint ) ).thenReturn( true );
			  int labelId = _descriptor.LabelId;
			  int propertyId = _descriptor.PropertyId;
			  when( _tokenHolders.labelTokens().getTokenById(labelId) ).thenReturn(new NamedToken("Label", labelId));
			  when( _tokenHolders.propertyKeyTokens().getTokenById(propertyId) ).thenReturn(new NamedToken("prop", labelId));

			  // when
			  try
			  {
					_operations.nodeKeyConstraintCreate( _descriptor, indexProvider );
					fail( "Expected an exception because this schema should already be constrained." );
			  }
			  catch ( AlreadyConstrainedException )
			  {
					// Good.
			  }

			  // then
			  _order.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.LABEL, labelId );
			  _order.verify( _storageReader ).constraintExists( constraint );
			  _order.verify( _locks ).releaseExclusive( ResourceTypes.LABEL, labelId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReleaseAcquiredSchemaWriteLockIfNodePropertyExistenceConstraintCreationFails() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReleaseAcquiredSchemaWriteLockIfNodePropertyExistenceConstraintCreationFails()
		 {
			  // given
			  NodeExistenceConstraintDescriptor constraint = existsForSchema( _descriptor );
			  when( _storageReader.constraintExists( constraint ) ).thenReturn( true );
			  int labelId = _descriptor.LabelId;
			  int propertyId = _descriptor.PropertyId;
			  when( _tokenHolders.labelTokens().getTokenById(labelId) ).thenReturn(new NamedToken("Label", labelId));
			  when( _tokenHolders.propertyKeyTokens().getTokenById(propertyId) ).thenReturn(new NamedToken("prop", labelId));

			  // when
			  try
			  {
					_operations.nodePropertyExistenceConstraintCreate( _descriptor );
					fail( "Expected an exception because this schema should already be constrained." );
			  }
			  catch ( AlreadyConstrainedException )
			  {
					// Good.
			  }

			  // then
			  _order.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.LABEL, labelId );
			  _order.verify( _storageReader ).constraintExists( constraint );
			  _order.verify( _locks ).releaseExclusive( ResourceTypes.LABEL, labelId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReleaseAcquiredSchemaWriteLockIfRelationshipPropertyExistenceConstraintCreationFails() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReleaseAcquiredSchemaWriteLockIfRelationshipPropertyExistenceConstraintCreationFails()
		 {
			  // given
			  RelationTypeSchemaDescriptor descriptor = SchemaDescriptorFactory.forRelType( 11, 13 );
			  RelExistenceConstraintDescriptor constraint = existsForSchema( descriptor );
			  when( _storageReader.constraintExists( constraint ) ).thenReturn( true );
			  int relTypeId = descriptor.RelTypeId;
			  int propertyId = descriptor.PropertyId;
			  when( _tokenHolders.relationshipTypeTokens().getTokenById(relTypeId) ).thenReturn(new NamedToken("Label", relTypeId));
			  when( _tokenHolders.propertyKeyTokens().getTokenById(propertyId) ).thenReturn(new NamedToken("prop", relTypeId));

			  // when
			  try
			  {
					_operations.relationshipPropertyExistenceConstraintCreate( descriptor );
					fail( "Expected an exception because this schema should already be constrained." );
			  }
			  catch ( AlreadyConstrainedException )
			  {
					// Good.
			  }

			  // then
			  _order.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.RELATIONSHIP_TYPE, relTypeId );
			  _order.verify( _storageReader ).constraintExists( constraint );
			  _order.verify( _locks ).releaseExclusive( ResourceTypes.RELATIONSHIP_TYPE, relTypeId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcquireSchemaWriteLockBeforeDroppingConstraint() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcquireSchemaWriteLockBeforeDroppingConstraint()
		 {
			  // given
			  UniquenessConstraintDescriptor constraint = uniqueForSchema( _descriptor );
			  when( _storageReader.constraintExists( constraint ) ).thenReturn( true );

			  // when
			  _operations.constraintDrop( constraint );

			  // then
			  _order.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.LABEL, _descriptor.LabelId );
			  _order.verify( _txState ).constraintDoDrop( constraint );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void detachDeleteNodeWithoutRelationshipsExclusivelyLockNode() throws org.neo4j.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DetachDeleteNodeWithoutRelationshipsExclusivelyLockNode()
		 {
			  long nodeId = 1L;
			  returnRelationships( _transaction, false, new TestRelationshipChain( nodeId ) );
			  when( _transaction.ambientNodeCursor() ).thenReturn((new StubNodeCursor(false)).withNode(nodeId));
			  when( _nodeCursor.next() ).thenReturn(true);
			  LabelSet labels = mock( typeof( LabelSet ) );
			  when( labels.All() ).thenReturn(EMPTY_LONG_ARRAY);
			  when( _nodeCursor.labels() ).thenReturn(labels);

			  _operations.nodeDetachDelete( nodeId );

			  _order.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.NODE, nodeId );
			  _order.verify( _locks, never() ).releaseExclusive(ResourceTypes.NODE, nodeId);
			  _order.verify( _txState ).nodeDoDelete( nodeId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void detachDeleteNodeExclusivelyLockNodes() throws org.neo4j.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DetachDeleteNodeExclusivelyLockNodes()
		 {
			  long nodeId = 1L;
			  returnRelationships( _transaction, false, ( new TestRelationshipChain( nodeId ) ).outgoing( 1, 2L, 42 ) );
			  when( _transaction.ambientNodeCursor() ).thenReturn((new StubNodeCursor(false)).withNode(nodeId));
			  LabelSet labels = mock( typeof( LabelSet ) );
			  when( labels.All() ).thenReturn(EMPTY_LONG_ARRAY);
			  when( _nodeCursor.labels() ).thenReturn(labels);
			  when( _nodeCursor.next() ).thenReturn(true);

			  _operations.nodeDetachDelete( nodeId );

			  _order.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.NODE, nodeId, 2L );
			  _order.verify( _locks, never() ).releaseExclusive(ResourceTypes.NODE, nodeId);
			  _order.verify( _locks, never() ).releaseExclusive(ResourceTypes.NODE, 2L);
			  _order.verify( _txState ).nodeDoDelete( nodeId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcquiredSharedLabelLocksWhenDeletingNode() throws org.neo4j.internal.kernel.api.exceptions.explicitindex.AutoIndexingKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcquiredSharedLabelLocksWhenDeletingNode()
		 {
			  // given
			  long nodeId = 1L;
			  long labelId1 = 1;
			  long labelId2 = 2;
			  when( _nodeCursor.next() ).thenReturn(true);
			  LabelSet labels = mock( typeof( LabelSet ) );
			  when( labels.All() ).thenReturn(new long[]{ labelId1, labelId2 });
			  when( _nodeCursor.labels() ).thenReturn(labels);

			  // when
			  _operations.nodeDelete( nodeId );

			  // then
			  InOrder order = inOrder( _locks );
			  order.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.NODE, nodeId );
			  order.verify( _locks ).acquireShared( LockTracer.NONE, ResourceTypes.LABEL, labelId1, labelId2 );
			  order.verifyNoMoreInteractions();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcquiredSharedLabelLocksWhenDetachDeletingNode() throws org.neo4j.internal.kernel.api.exceptions.KernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcquiredSharedLabelLocksWhenDetachDeletingNode()
		 {
			  // given
			  long nodeId = 1L;
			  long labelId1 = 1;
			  long labelId2 = 2;

			  returnRelationships( _transaction, false, new TestRelationshipChain( nodeId ) );
			  when( _transaction.ambientNodeCursor() ).thenReturn((new StubNodeCursor(false)).withNode(nodeId));
			  when( _nodeCursor.next() ).thenReturn(true);
			  LabelSet labels = mock( typeof( LabelSet ) );
			  when( labels.All() ).thenReturn(new long[]{ labelId1, labelId2 });
			  when( _nodeCursor.labels() ).thenReturn(labels);

			  // when
			  _operations.nodeDetachDelete( nodeId );

			  // then
			  InOrder order = inOrder( _locks );
			  order.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.NODE, nodeId );
			  order.verify( _locks ).acquireShared( LockTracer.NONE, ResourceTypes.LABEL, labelId1, labelId2 );
			  order.verifyNoMoreInteractions();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcquiredSharedLabelLocksWhenRemovingNodeLabel() throws org.neo4j.internal.kernel.api.exceptions.EntityNotFoundException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcquiredSharedLabelLocksWhenRemovingNodeLabel()
		 {
			  // given
			  long nodeId = 1L;
			  int labelId = 1;
			  when( _nodeCursor.next() ).thenReturn(true);
			  when( _nodeCursor.hasLabel( labelId ) ).thenReturn( true );

			  // when
			  _operations.nodeRemoveLabel( nodeId, labelId );

			  // then
			  InOrder order = inOrder( _locks );
			  order.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.NODE, nodeId );
			  order.verify( _locks ).acquireShared( LockTracer.NONE, ResourceTypes.LABEL, labelId );
			  order.verifyNoMoreInteractions();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcquiredSharedLabelLocksWhenRemovingNodeProperty() throws org.neo4j.internal.kernel.api.exceptions.explicitindex.AutoIndexingKernelException, org.neo4j.internal.kernel.api.exceptions.EntityNotFoundException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcquiredSharedLabelLocksWhenRemovingNodeProperty()
		 {
			  // given
			  long nodeId = 1L;
			  long labelId1 = 1;
			  long labelId2 = 1;
			  int propertyKeyId = 5;
			  when( _nodeCursor.next() ).thenReturn(true);
			  LabelSet labels = mock( typeof( LabelSet ) );
			  when( labels.All() ).thenReturn(new long[]{ labelId1, labelId2 });
			  when( _nodeCursor.labels() ).thenReturn(labels);
			  when( _propertyCursor.next() ).thenReturn(true);
			  when( _propertyCursor.propertyKey() ).thenReturn(propertyKeyId);
			  when( _propertyCursor.propertyValue() ).thenReturn(Values.of("abc"));

			  // when
			  _operations.nodeRemoveProperty( nodeId, propertyKeyId );

			  // then
			  InOrder order = inOrder( _locks );
			  order.verify( _locks ).acquireExclusive( LockTracer.NONE, ResourceTypes.NODE, nodeId );
			  order.verify( _locks ).acquireShared( LockTracer.NONE, ResourceTypes.LABEL, labelId1, labelId2 );
			  order.verifyNoMoreInteractions();
		 }

		 private void SetStoreRelationship( long relationshipId, long sourceNode, long targetNode, int relationshipLabel )
		 {
			  when( _relationshipCursor.next() ).thenReturn(true);
			  when( _relationshipCursor.relationshipReference() ).thenReturn(relationshipId);
			  when( _relationshipCursor.sourceNodeReference() ).thenReturn(sourceNode);
			  when( _relationshipCursor.targetNodeReference() ).thenReturn(targetNode);
			  when( _relationshipCursor.type() ).thenReturn(relationshipLabel);
		 }
	}

}