using System;
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
namespace Neo4Net.Kernel.impl.transaction.command
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using NamedToken = Neo4Net.Kernel.Api.Internal.NamedToken;
	using IndexNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException;
	using IndexProviderDescriptor = Neo4Net.Kernel.Api.Internal.schema.IndexProviderDescriptor;
	using IndexActivationFailedKernelException = Neo4Net.Kernel.Api.Exceptions.index.IndexActivationFailedKernelException;
	using IndexPopulationFailedKernelException = Neo4Net.Kernel.Api.Exceptions.index.IndexPopulationFailedKernelException;
	using LabelScanWriter = Neo4Net.Kernel.api.labelscan.LabelScanWriter;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using BatchTransactionApplier = Neo4Net.Kernel.Impl.Api.BatchTransactionApplier;
	using BatchTransactionApplierFacade = Neo4Net.Kernel.Impl.Api.BatchTransactionApplierFacade;
	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using IndexingUpdateService = Neo4Net.Kernel.Impl.Api.index.IndexingUpdateService;
	using CacheAccessBackDoor = Neo4Net.Kernel.impl.core.CacheAccessBackDoor;
	using LockService = Neo4Net.Kernel.impl.locking.LockService;
	using DynamicArrayStore = Neo4Net.Kernel.impl.store.DynamicArrayStore;
	using LabelTokenStore = Neo4Net.Kernel.impl.store.LabelTokenStore;
	using MetaDataStore = Neo4Net.Kernel.impl.store.MetaDataStore;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using PropertyKeyTokenStore = Neo4Net.Kernel.impl.store.PropertyKeyTokenStore;
	using PropertyStore = Neo4Net.Kernel.impl.store.PropertyStore;
	using RelationshipGroupStore = Neo4Net.Kernel.impl.store.RelationshipGroupStore;
	using RelationshipStore = Neo4Net.Kernel.impl.store.RelationshipStore;
	using RelationshipTypeTokenStore = Neo4Net.Kernel.impl.store.RelationshipTypeTokenStore;
	using SchemaStore = Neo4Net.Kernel.impl.store.SchemaStore;
	using ConstraintRule = Neo4Net.Kernel.Impl.Store.Records.ConstraintRule;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using LabelTokenRecord = Neo4Net.Kernel.Impl.Store.Records.LabelTokenRecord;
	using NeoStoreRecord = Neo4Net.Kernel.Impl.Store.Records.NeoStoreRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyKeyTokenRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipTypeTokenRecord;
	using LabelTokenCommand = Neo4Net.Kernel.impl.transaction.command.Command.LabelTokenCommand;
	using PropertyKeyTokenCommand = Neo4Net.Kernel.impl.transaction.command.Command.PropertyKeyTokenCommand;
	using RelationshipTypeTokenCommand = Neo4Net.Kernel.impl.transaction.command.Command.RelationshipTypeTokenCommand;
	using ApplyFunction = Neo4Net.Kernel.impl.transaction.command.CommandHandlerContract.ApplyFunction;
	using IndexDescriptorFactory = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptorFactory;
	using StoreIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor;
	using Neo4Net.Utils.Concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.schema.SchemaDescriptorFactory.forLabel;

	public class NeoStoreTransactionApplierTest
	{
		private bool InstanceFieldsInitialized = false;

		public NeoStoreTransactionApplierTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_labelScanStoreSynchronizer = new WorkSync<System.Func<LabelScanWriter>, LabelUpdateWork>( _labelScanStore );
			_indexUpdatesSync = new WorkSync<IndexingUpdateService, IndexUpdatesWork>( _indexingService );
			_indexActivator = new IndexActivator( _indexingService );
		}

		 private readonly NeoStores _neoStores = mock( typeof( NeoStores ) );
		 private readonly IndexingService _indexingService = mock( typeof( IndexingService ) );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private final System.Func<org.Neo4Net.kernel.api.labelscan.LabelScanWriter> labelScanStore = mock(System.Func.class);
		 private readonly System.Func<LabelScanWriter> _labelScanStore = mock( typeof( System.Func ) );
		 private readonly CacheAccessBackDoor _cacheAccess = mock( typeof( CacheAccessBackDoor ) );
		 private readonly LockService _lockService = mock( typeof( LockService ) );

		 private readonly MetaDataStore _metaDataStore = mock( typeof( MetaDataStore ) );
		 private readonly NodeStore _nodeStore = mock( typeof( NodeStore ) );
		 private readonly RelationshipStore _relationshipStore = mock( typeof( RelationshipStore ) );
		 private readonly PropertyStore _propertyStore = mock( typeof( PropertyStore ) );
		 private readonly RelationshipGroupStore _relationshipGroupStore = mock( typeof( RelationshipGroupStore ) );
		 private readonly RelationshipTypeTokenStore _relationshipTypeTokenStore = mock( typeof( RelationshipTypeTokenStore ) );
		 private readonly LabelTokenStore _labelTokenStore = mock( typeof( LabelTokenStore ) );
		 private readonly PropertyKeyTokenStore _propertyKeyTokenStore = mock( typeof( PropertyKeyTokenStore ) );
		 private readonly SchemaStore _schemaStore = mock( typeof( SchemaStore ) );
		 private readonly DynamicArrayStore _dynamicLabelStore = mock( typeof( DynamicArrayStore ) );

		 private readonly long _transactionId = 55555;
		 private readonly DynamicRecord _one = DynamicRecord.dynamicRecord( 1, true );
		 private readonly DynamicRecord _two = DynamicRecord.dynamicRecord( 2, true );
		 private readonly DynamicRecord _three = DynamicRecord.dynamicRecord( 3, true );
		 private WorkSync<System.Func<LabelScanWriter>, LabelUpdateWork> _labelScanStoreSynchronizer = new WorkSync<System.Func<LabelScanWriter>, LabelUpdateWork>( _labelScanStore );
		 private readonly TransactionToApply _transactionToApply = mock( typeof( TransactionToApply ) );
		 private WorkSync<IndexingUpdateService, IndexUpdatesWork> _indexUpdatesSync;
		 private IndexActivator _indexActivator;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  when( _neoStores.MetaDataStore ).thenReturn( _metaDataStore );
			  when( _neoStores.NodeStore ).thenReturn( _nodeStore );
			  when( _neoStores.RelationshipStore ).thenReturn( _relationshipStore );
			  when( _neoStores.PropertyStore ).thenReturn( _propertyStore );
			  when( _neoStores.RelationshipGroupStore ).thenReturn( _relationshipGroupStore );
			  when( _neoStores.RelationshipTypeTokenStore ).thenReturn( _relationshipTypeTokenStore );
			  when( _neoStores.LabelTokenStore ).thenReturn( _labelTokenStore );
			  when( _neoStores.PropertyKeyTokenStore ).thenReturn( _propertyKeyTokenStore );
			  when( _neoStores.SchemaStore ).thenReturn( _schemaStore );
			  when( _nodeStore.DynamicLabelStore ).thenReturn( _dynamicLabelStore );
			  when( _lockService.acquireNodeLock( anyLong(), any() ) ).thenReturn(LockService.NO_LOCK);
			  when( _lockService.acquireRelationshipLock( anyLong(), any() ) ).thenReturn(LockService.NO_LOCK);
			  when( _transactionToApply.transactionId() ).thenReturn(_transactionId);
		 }

		 // NODE COMMAND

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyNodeCommandToTheStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyNodeCommandToTheStore()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplier(false);
			  BatchTransactionApplier applier = NewApplier( false );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.NodeRecord before = new org.Neo4Net.kernel.impl.store.record.NodeRecord(11);
			  NodeRecord before = new NodeRecord( 11 );
			  before.SetLabelField( 42, asList( _one, _two ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.NodeRecord after = new org.Neo4Net.kernel.impl.store.record.NodeRecord(12);
			  NodeRecord after = new NodeRecord( 12 );
			  after.InUse = true;
			  after.SetLabelField( 42, asList( _one, _two, _three ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command.NodeCommand command = new Command.NodeCommand(before, after);
			  Command.NodeCommand command = new Command.NodeCommand( before, after );

			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _lockService, times( 1 ) ).acquireNodeLock( command.Key, Neo4Net.Kernel.impl.locking.LockService_LockType.WriteLock );
			  verify( _nodeStore, times( 1 ) ).updateRecord( after );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyNodeCommandToTheStoreAndInvalidateTheCache() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyNodeCommandToTheStoreAndInvalidateTheCache()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplier(false);
			  BatchTransactionApplier applier = NewApplier( false );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.NodeRecord before = new org.Neo4Net.kernel.impl.store.record.NodeRecord(11);
			  NodeRecord before = new NodeRecord( 11 );
			  before.SetLabelField( 42, asList( _one, _two ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.NodeRecord after = new org.Neo4Net.kernel.impl.store.record.NodeRecord(12);
			  NodeRecord after = new NodeRecord( 12 );
			  after.InUse = false;
			  after.SetLabelField( 42, asList( _one, _two, _three ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command.NodeCommand command = new Command.NodeCommand(before, after);
			  Command.NodeCommand command = new Command.NodeCommand( before, after );

			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _lockService, times( 1 ) ).acquireNodeLock( command.Key, Neo4Net.Kernel.impl.locking.LockService_LockType.WriteLock );
			  verify( _nodeStore, times( 1 ) ).updateRecord( after );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyNodeCommandToTheStoreInRecoveryMode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyNodeCommandToTheStoreInRecoveryMode()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplier(true);
			  BatchTransactionApplier applier = NewApplier( true );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.NodeRecord before = new org.Neo4Net.kernel.impl.store.record.NodeRecord(11);
			  NodeRecord before = new NodeRecord( 11 );
			  before.SetLabelField( 42, asList( _one, _two ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.NodeRecord after = new org.Neo4Net.kernel.impl.store.record.NodeRecord(12);
			  NodeRecord after = new NodeRecord( 12 );
			  after.InUse = true;
			  after.SetLabelField( 42, asList( _one, _two, _three ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command.NodeCommand command = new Command.NodeCommand(before, after);
			  Command.NodeCommand command = new Command.NodeCommand( before, after );

			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _lockService, times( 1 ) ).acquireNodeLock( command.Key, Neo4Net.Kernel.impl.locking.LockService_LockType.WriteLock );
			  verify( _nodeStore, times( 1 ) ).HighestPossibleIdInUse = after.Id;
			  verify( _nodeStore, times( 1 ) ).updateRecord( after );
			  verify( _dynamicLabelStore, times( 1 ) ).HighestPossibleIdInUse = _three.Id;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInvalidateTheCacheWhenTheNodeBecomesDense() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInvalidateTheCacheWhenTheNodeBecomesDense()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplier(false);
			  BatchTransactionApplier applier = NewApplier( false );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.NodeRecord before = new org.Neo4Net.kernel.impl.store.record.NodeRecord(11);
			  NodeRecord before = new NodeRecord( 11 );
			  before.SetLabelField( 42, singletonList( _one ) );
			  before.InUse = true;
			  before.Dense = false;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.NodeRecord after = new org.Neo4Net.kernel.impl.store.record.NodeRecord(12);
			  NodeRecord after = new NodeRecord( 12 );
			  after.InUse = true;
			  after.Dense = true;
			  after.SetLabelField( 42, asList( _one, _two, _three ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command.NodeCommand command = new Command.NodeCommand(before, after);
			  Command.NodeCommand command = new Command.NodeCommand( before, after );

			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _lockService, times( 1 ) ).acquireNodeLock( command.Key, Neo4Net.Kernel.impl.locking.LockService_LockType.WriteLock );
			  verify( _nodeStore, times( 1 ) ).updateRecord( after );
		 }

		 // RELATIONSHIP COMMAND

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyRelationshipCommandToTheStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyRelationshipCommandToTheStore()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplier(false);
			  BatchTransactionApplier applier = NewApplier( false );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.RelationshipRecord before = new org.Neo4Net.kernel.impl.store.record.RelationshipRecord(12);
			  RelationshipRecord before = new RelationshipRecord( 12 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.RelationshipRecord record = new org.Neo4Net.kernel.impl.store.record.RelationshipRecord(12, 3, 4, 5);
			  RelationshipRecord record = new RelationshipRecord( 12, 3, 4, 5 );
			  record.InUse = true;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command command = new Command.RelationshipCommand(before, record);
			  Command command = new Command.RelationshipCommand( before, record );
			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _relationshipStore, times( 1 ) ).updateRecord( record );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyRelationshipCommandToTheStoreAndInvalidateTheCache() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyRelationshipCommandToTheStoreAndInvalidateTheCache()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplier(false);
			  BatchTransactionApplier applier = NewApplier( false );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.RelationshipRecord before = new org.Neo4Net.kernel.impl.store.record.RelationshipRecord(12);
			  RelationshipRecord before = new RelationshipRecord( 12 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.RelationshipRecord record = new org.Neo4Net.kernel.impl.store.record.RelationshipRecord(12, 3, 4, 5);
			  RelationshipRecord record = new RelationshipRecord( 12, 3, 4, 5 );
			  record.InUse = false;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command command = new Command.RelationshipCommand(before, record);
			  Command command = new Command.RelationshipCommand( before, record );

			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _relationshipStore, times( 1 ) ).updateRecord( record );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyRelationshipCommandToTheStoreInRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyRelationshipCommandToTheStoreInRecovery()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplier(true);
			  BatchTransactionApplier applier = NewApplier( true );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.RelationshipRecord before = new org.Neo4Net.kernel.impl.store.record.RelationshipRecord(12);
			  RelationshipRecord before = new RelationshipRecord( 12 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.RelationshipRecord record = new org.Neo4Net.kernel.impl.store.record.RelationshipRecord(12, 3, 4, 5);
			  RelationshipRecord record = new RelationshipRecord( 12, 3, 4, 5 );
			  record.InUse = true;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command command = new Command.RelationshipCommand(before, record);
			  Command command = new Command.RelationshipCommand( before, record );

			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _relationshipStore, times( 1 ) ).HighestPossibleIdInUse = record.Id;
			  verify( _relationshipStore, times( 1 ) ).updateRecord( record );
		 }

		 // PROPERTY COMMAND

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyNodePropertyCommandToTheStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyNodePropertyCommandToTheStore()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplier(false);
			  BatchTransactionApplier applier = NewApplier( false );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.PropertyRecord before = new org.Neo4Net.kernel.impl.store.record.PropertyRecord(11);
			  PropertyRecord before = new PropertyRecord( 11 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.PropertyRecord after = new org.Neo4Net.kernel.impl.store.record.PropertyRecord(12);
			  PropertyRecord after = new PropertyRecord( 12 );
			  after.NodeId = 42;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command command = new Command.PropertyCommand(before, after);
			  Command command = new Command.PropertyCommand( before, after );

			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _lockService, times( 1 ) ).acquireNodeLock( 42, Neo4Net.Kernel.impl.locking.LockService_LockType.WriteLock );
			  verify( _propertyStore, times( 1 ) ).updateRecord( after );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyNodePropertyCommandToTheStoreInRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyNodePropertyCommandToTheStoreInRecovery()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplier(true);
			  BatchTransactionApplier applier = NewApplier( true );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.PropertyRecord before = new org.Neo4Net.kernel.impl.store.record.PropertyRecord(11);
			  PropertyRecord before = new PropertyRecord( 11 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.PropertyRecord after = new org.Neo4Net.kernel.impl.store.record.PropertyRecord(12);
			  PropertyRecord after = new PropertyRecord( 12 );
			  after.NodeId = 42;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command command = new Command.PropertyCommand(before, after);
			  Command command = new Command.PropertyCommand( before, after );

			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _lockService, times( 1 ) ).acquireNodeLock( 42, Neo4Net.Kernel.impl.locking.LockService_LockType.WriteLock );
			  verify( _propertyStore, times( 1 ) ).HighestPossibleIdInUse = after.Id;
			  verify( _propertyStore, times( 1 ) ).updateRecord( after );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyRelPropertyCommandToTheStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyRelPropertyCommandToTheStore()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplier(false);
			  BatchTransactionApplier applier = NewApplier( false );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.PropertyRecord before = new org.Neo4Net.kernel.impl.store.record.PropertyRecord(11);
			  PropertyRecord before = new PropertyRecord( 11 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.PropertyRecord after = new org.Neo4Net.kernel.impl.store.record.PropertyRecord(12);
			  PropertyRecord after = new PropertyRecord( 12 );
			  after.RelId = 42;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command command = new Command.PropertyCommand(before, after);
			  Command command = new Command.PropertyCommand( before, after );

			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _propertyStore, times( 1 ) ).updateRecord( after );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyRelPropertyCommandToTheStoreInRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyRelPropertyCommandToTheStoreInRecovery()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplier(true);
			  BatchTransactionApplier applier = NewApplier( true );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.PropertyRecord before = new org.Neo4Net.kernel.impl.store.record.PropertyRecord(11);
			  PropertyRecord before = new PropertyRecord( 11 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.PropertyRecord after = new org.Neo4Net.kernel.impl.store.record.PropertyRecord(12);
			  PropertyRecord after = new PropertyRecord( 12 );
			  after.RelId = 42;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command command = new Command.PropertyCommand(before, after);
			  Command command = new Command.PropertyCommand( before, after );

			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _propertyStore, times( 1 ) ).HighestPossibleIdInUse = 12;
			  verify( _propertyStore, times( 1 ) ).updateRecord( after );
		 }

		 // RELATIONSHIP GROUP COMMAND

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyRelationshipGroupCommandToTheStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyRelationshipGroupCommandToTheStore()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplier(false);
			  BatchTransactionApplier applier = NewApplier( false );
			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.RelationshipGroupRecord before = new org.Neo4Net.kernel.impl.store.record.RelationshipGroupRecord(42, 1);
			  RelationshipGroupRecord before = new RelationshipGroupRecord( 42, 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.RelationshipGroupRecord after = new org.Neo4Net.kernel.impl.store.record.RelationshipGroupRecord(42, 1, 2, 3, 4, 5, 6, true);
			  RelationshipGroupRecord after = new RelationshipGroupRecord( 42, 1, 2, 3, 4, 5, 6, true );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command command = new Command.RelationshipGroupCommand(before, after);
			  Command command = new Command.RelationshipGroupCommand( before, after );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean result = apply(applier, command::handle, transactionToApply);
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _relationshipGroupStore, times( 1 ) ).updateRecord( after );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyRelationshipGroupCommandToTheStoreInRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyRelationshipGroupCommandToTheStoreInRecovery()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplier(true);
			  BatchTransactionApplier applier = NewApplier( true );
			  // when
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.RelationshipGroupRecord before = new org.Neo4Net.kernel.impl.store.record.RelationshipGroupRecord(42, 1);
			  RelationshipGroupRecord before = new RelationshipGroupRecord( 42, 1 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.RelationshipGroupRecord after = new org.Neo4Net.kernel.impl.store.record.RelationshipGroupRecord(42, 1, 2, 3, 4, 5, 6, true);
			  RelationshipGroupRecord after = new RelationshipGroupRecord( 42, 1, 2, 3, 4, 5, 6, true );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command command = new Command.RelationshipGroupCommand(before, after);
			  Command command = new Command.RelationshipGroupCommand( before, after );

			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _relationshipGroupStore, times( 1 ) ).HighestPossibleIdInUse = after.Id;
			  verify( _relationshipGroupStore, times( 1 ) ).updateRecord( after );
		 }

		 // RELATIONSHIP TYPE TOKEN COMMAND

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyRelationshipTypeTokenCommandToTheStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyRelationshipTypeTokenCommandToTheStore()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplier(false);
			  BatchTransactionApplier applier = NewApplier( false );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.RelationshipTypeTokenRecord before = new org.Neo4Net.kernel.impl.store.record.RelationshipTypeTokenRecord(42);
			  RelationshipTypeTokenRecord before = new RelationshipTypeTokenRecord( 42 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.RelationshipTypeTokenRecord after = new org.Neo4Net.kernel.impl.store.record.RelationshipTypeTokenRecord(42);
			  RelationshipTypeTokenRecord after = new RelationshipTypeTokenRecord( 42 );
			  after.InUse = true;
			  after.NameId = 323;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command command = new org.Neo4Net.kernel.impl.transaction.command.Command.RelationshipTypeTokenCommand(before, after);
			  Command command = new RelationshipTypeTokenCommand( before, after );

			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _relationshipTypeTokenStore, times( 1 ) ).updateRecord( after );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyRelationshipTypeTokenCommandToTheStoreInRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyRelationshipTypeTokenCommandToTheStoreInRecovery()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplier(true);
			  BatchTransactionApplier applier = NewApplier( true );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.RelationshipTypeTokenRecord before = new org.Neo4Net.kernel.impl.store.record.RelationshipTypeTokenRecord(42);
			  RelationshipTypeTokenRecord before = new RelationshipTypeTokenRecord( 42 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.RelationshipTypeTokenRecord after = new org.Neo4Net.kernel.impl.store.record.RelationshipTypeTokenRecord(42);
			  RelationshipTypeTokenRecord after = new RelationshipTypeTokenRecord( 42 );
			  after.InUse = true;
			  after.NameId = 323;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command.RelationshipTypeTokenCommand command = new Command.RelationshipTypeTokenCommand(before, after);
			  Command.RelationshipTypeTokenCommand command = new Command.RelationshipTypeTokenCommand( before, after );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.Kernel.Api.Internal.NamedToken token = new org.Neo4Net.Kernel.Api.Internal.NamedToken("token", 21);
			  NamedToken token = new NamedToken( "token", 21 );
			  when( _relationshipTypeTokenStore.getToken( ( int ) command.Key ) ).thenReturn( token );

			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _relationshipTypeTokenStore, times( 1 ) ).HighestPossibleIdInUse = after.Id;
			  verify( _relationshipTypeTokenStore, times( 1 ) ).updateRecord( after );
			  verify( _cacheAccess, times( 1 ) ).addRelationshipTypeToken( token );
		 }

		 // LABEL TOKEN COMMAND

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyLabelTokenCommandToTheStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyLabelTokenCommandToTheStore()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplier(false);
			  BatchTransactionApplier applier = NewApplier( false );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.LabelTokenRecord before = new org.Neo4Net.kernel.impl.store.record.LabelTokenRecord(42);
			  LabelTokenRecord before = new LabelTokenRecord( 42 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.LabelTokenRecord after = new org.Neo4Net.kernel.impl.store.record.LabelTokenRecord(42);
			  LabelTokenRecord after = new LabelTokenRecord( 42 );
			  after.InUse = true;
			  after.NameId = 323;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command command = new org.Neo4Net.kernel.impl.transaction.command.Command.LabelTokenCommand(before, after);
			  Command command = new LabelTokenCommand( before, after );

			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _labelTokenStore, times( 1 ) ).updateRecord( after );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyLabelTokenCommandToTheStoreInRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyLabelTokenCommandToTheStoreInRecovery()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplier(true);
			  BatchTransactionApplier applier = NewApplier( true );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.LabelTokenRecord before = new org.Neo4Net.kernel.impl.store.record.LabelTokenRecord(42);
			  LabelTokenRecord before = new LabelTokenRecord( 42 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.LabelTokenRecord after = new org.Neo4Net.kernel.impl.store.record.LabelTokenRecord(42);
			  LabelTokenRecord after = new LabelTokenRecord( 42 );
			  after.InUse = true;
			  after.NameId = 323;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command.LabelTokenCommand command = new Command.LabelTokenCommand(before, after);
			  Command.LabelTokenCommand command = new Command.LabelTokenCommand( before, after );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.Kernel.Api.Internal.NamedToken token = new org.Neo4Net.Kernel.Api.Internal.NamedToken("token", 21);
			  NamedToken token = new NamedToken( "token", 21 );
			  when( _labelTokenStore.getToken( ( int ) command.Key ) ).thenReturn( token );

			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _labelTokenStore, times( 1 ) ).HighestPossibleIdInUse = after.Id;
			  verify( _labelTokenStore, times( 1 ) ).updateRecord( after );
			  verify( _cacheAccess, times( 1 ) ).addLabelToken( token );
		 }

		 // PROPERTY KEY TOKEN COMMAND

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyPropertyKeyTokenCommandToTheStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyPropertyKeyTokenCommandToTheStore()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplier(false);
			  BatchTransactionApplier applier = NewApplier( false );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.PropertyKeyTokenRecord before = new org.Neo4Net.kernel.impl.store.record.PropertyKeyTokenRecord(42);
			  PropertyKeyTokenRecord before = new PropertyKeyTokenRecord( 42 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.PropertyKeyTokenRecord after = new org.Neo4Net.kernel.impl.store.record.PropertyKeyTokenRecord(42);
			  PropertyKeyTokenRecord after = new PropertyKeyTokenRecord( 42 );
			  after.InUse = true;
			  after.NameId = 323;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command command = new org.Neo4Net.kernel.impl.transaction.command.Command.PropertyKeyTokenCommand(before, after);
			  Command command = new PropertyKeyTokenCommand( before, after );

			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _propertyKeyTokenStore, times( 1 ) ).updateRecord( after );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyPropertyKeyTokenCommandToTheStoreInRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyPropertyKeyTokenCommandToTheStoreInRecovery()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplier(true);
			  BatchTransactionApplier applier = NewApplier( true );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.PropertyKeyTokenRecord before = new org.Neo4Net.kernel.impl.store.record.PropertyKeyTokenRecord(42);
			  PropertyKeyTokenRecord before = new PropertyKeyTokenRecord( 42 );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.PropertyKeyTokenRecord after = new org.Neo4Net.kernel.impl.store.record.PropertyKeyTokenRecord(42);
			  PropertyKeyTokenRecord after = new PropertyKeyTokenRecord( 42 );
			  after.InUse = true;
			  after.NameId = 323;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command.PropertyKeyTokenCommand command = new Command.PropertyKeyTokenCommand(before, after);
			  Command.PropertyKeyTokenCommand command = new Command.PropertyKeyTokenCommand( before, after );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.Kernel.Api.Internal.NamedToken token = new org.Neo4Net.Kernel.Api.Internal.NamedToken("token", 21);
			  NamedToken token = new NamedToken( "token", 21 );
			  when( _propertyKeyTokenStore.getToken( ( int ) command.Key ) ).thenReturn( token );

			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _propertyKeyTokenStore, times( 1 ) ).HighestPossibleIdInUse = after.Id;
			  verify( _propertyKeyTokenStore, times( 1 ) ).updateRecord( after );
			  verify( _cacheAccess, times( 1 ) ).addPropertyKeyToken( token );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyCreateIndexRuleSchemaRuleCommandToTheStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyCreateIndexRuleSchemaRuleCommandToTheStore()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplierFacade(newApplier(false), newIndexApplier());
			  BatchTransactionApplier applier = NewApplierFacade( NewApplier( false ), NewIndexApplier() );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.DynamicRecord record = org.Neo4Net.kernel.impl.store.record.DynamicRecord.dynamicRecord(21, true);
			  DynamicRecord record = DynamicRecord.dynamicRecord( 21, true );
			  record.SetCreated();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Collection<org.Neo4Net.kernel.impl.store.record.DynamicRecord> recordsAfter = singletonList(record);
			  ICollection<DynamicRecord> recordsAfter = singletonList( record );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor rule = indexRule(0, 1, 2, new org.Neo4Net.Kernel.Api.Internal.schema.IndexProviderDescriptor("K", "X.Y"));
			  StoreIndexDescriptor rule = IndexRule( 0, 1, 2, new IndexProviderDescriptor( "K", "X.Y" ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command.SchemaRuleCommand command = new Command.SchemaRuleCommand(java.util.Collections.emptyList(), recordsAfter, rule);
			  Command.SchemaRuleCommand command = new Command.SchemaRuleCommand( Collections.emptyList(), recordsAfter, rule );

			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _schemaStore, times( 1 ) ).updateRecord( record );
			  verify( _indexingService, times( 1 ) ).createIndexes( rule );
			  verify( _cacheAccess, times( 1 ) ).addSchemaRule( rule );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyCreateIndexRuleSchemaRuleCommandToTheStoreInRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyCreateIndexRuleSchemaRuleCommandToTheStoreInRecovery()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplierFacade(newIndexApplier(), newApplier(true));
			  BatchTransactionApplier applier = NewApplierFacade( NewIndexApplier(), NewApplier(true) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.DynamicRecord record = org.Neo4Net.kernel.impl.store.record.DynamicRecord.dynamicRecord(21, true);
			  DynamicRecord record = DynamicRecord.dynamicRecord( 21, true );
			  record.SetCreated();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Collection<org.Neo4Net.kernel.impl.store.record.DynamicRecord> recordsAfter = singletonList(record);
			  ICollection<DynamicRecord> recordsAfter = singletonList( record );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor rule = indexRule(0, 1, 2, new org.Neo4Net.Kernel.Api.Internal.schema.IndexProviderDescriptor("K", "X.Y"));
			  StoreIndexDescriptor rule = IndexRule( 0, 1, 2, new IndexProviderDescriptor( "K", "X.Y" ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command.SchemaRuleCommand command = new Command.SchemaRuleCommand(java.util.Collections.emptyList(), recordsAfter, rule);
			  Command.SchemaRuleCommand command = new Command.SchemaRuleCommand( Collections.emptyList(), recordsAfter, rule );

			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _schemaStore, times( 1 ) ).HighestPossibleIdInUse = record.Id;
			  verify( _schemaStore, times( 1 ) ).updateRecord( record );
			  verify( _indexingService, times( 1 ) ).createIndexes( rule );
			  verify( _cacheAccess, times( 1 ) ).addSchemaRule( rule );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyUpdateIndexRuleSchemaRuleCommandToTheStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyUpdateIndexRuleSchemaRuleCommandToTheStore()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplierFacade(newIndexApplier(), newApplier(false));
			  BatchTransactionApplier applier = NewApplierFacade( NewIndexApplier(), NewApplier(false) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.DynamicRecord record = org.Neo4Net.kernel.impl.store.record.DynamicRecord.dynamicRecord(21, true);
			  DynamicRecord record = DynamicRecord.dynamicRecord( 21, true );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Collection<org.Neo4Net.kernel.impl.store.record.DynamicRecord> recordsAfter = singletonList(record);
			  ICollection<DynamicRecord> recordsAfter = singletonList( record );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor rule = constraintIndexRule(0, 1, 2, new org.Neo4Net.Kernel.Api.Internal.schema.IndexProviderDescriptor("K", "X.Y"), 42L);
			  StoreIndexDescriptor rule = ConstraintIndexRule( 0, 1, 2, new IndexProviderDescriptor( "K", "X.Y" ), 42L );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command.SchemaRuleCommand command = new Command.SchemaRuleCommand(java.util.Collections.emptyList(), recordsAfter, rule);
			  Command.SchemaRuleCommand command = new Command.SchemaRuleCommand( Collections.emptyList(), recordsAfter, rule );

			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _schemaStore, times( 1 ) ).updateRecord( record );
			  verify( _indexingService, times( 1 ) ).activateIndex( rule.Id );
			  verify( _cacheAccess, times( 1 ) ).addSchemaRule( rule );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyUpdateIndexRuleSchemaRuleCommandToTheStoreInRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyUpdateIndexRuleSchemaRuleCommandToTheStoreInRecovery()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplierFacade(newIndexApplier(), newApplier(true));
			  BatchTransactionApplier applier = NewApplierFacade( NewIndexApplier(), NewApplier(true) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.DynamicRecord record = org.Neo4Net.kernel.impl.store.record.DynamicRecord.dynamicRecord(21, true);
			  DynamicRecord record = DynamicRecord.dynamicRecord( 21, true );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Collection<org.Neo4Net.kernel.impl.store.record.DynamicRecord> recordsAfter = singletonList(record);
			  ICollection<DynamicRecord> recordsAfter = singletonList( record );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor rule = constraintIndexRule(0, 1, 2, new org.Neo4Net.Kernel.Api.Internal.schema.IndexProviderDescriptor("K", "X.Y"), 42L);
			  StoreIndexDescriptor rule = ConstraintIndexRule( 0, 1, 2, new IndexProviderDescriptor( "K", "X.Y" ), 42L );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command.SchemaRuleCommand command = new Command.SchemaRuleCommand(java.util.Collections.emptyList(), recordsAfter, rule);
			  Command.SchemaRuleCommand command = new Command.SchemaRuleCommand( Collections.emptyList(), recordsAfter, rule );

			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _schemaStore, times( 1 ) ).HighestPossibleIdInUse = record.Id;
			  verify( _schemaStore, times( 1 ) ).updateRecord( record );
			  verify( _indexingService, times( 1 ) ).activateIndex( rule.Id );
			  verify( _cacheAccess, times( 1 ) ).addSchemaRule( rule );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyUpdateIndexRuleSchemaRuleCommandToTheStoreThrowingIndexProblem() throws org.Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException, org.Neo4Net.kernel.api.exceptions.index.IndexPopulationFailedKernelException, org.Neo4Net.kernel.api.exceptions.index.IndexActivationFailedKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyUpdateIndexRuleSchemaRuleCommandToTheStoreThrowingIndexProblem()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newIndexApplier();
			  BatchTransactionApplier applier = NewIndexApplier();
			  doThrow( new IndexNotFoundKernelException( "" ) ).when( _indexingService ).activateIndex( anyLong() );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.DynamicRecord record = org.Neo4Net.kernel.impl.store.record.DynamicRecord.dynamicRecord(21, true);
			  DynamicRecord record = DynamicRecord.dynamicRecord( 21, true );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Collection<org.Neo4Net.kernel.impl.store.record.DynamicRecord> recordsAfter = singletonList(record);
			  ICollection<DynamicRecord> recordsAfter = singletonList( record );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor rule = constraintIndexRule(0, 1, 2, new org.Neo4Net.Kernel.Api.Internal.schema.IndexProviderDescriptor("K", "X.Y"), 42L);
			  StoreIndexDescriptor rule = ConstraintIndexRule( 0, 1, 2, new IndexProviderDescriptor( "K", "X.Y" ), 42L );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command.SchemaRuleCommand command = new Command.SchemaRuleCommand(java.util.Collections.emptyList(), recordsAfter, rule);
			  Command.SchemaRuleCommand command = new Command.SchemaRuleCommand( Collections.emptyList(), recordsAfter, rule );

			  // when
			  try
			  {
					Apply( applier, command.handle, _transactionToApply );
					fail( "should have thrown" );
			  }
			  catch ( Exception e )
			  {
					// then
					assertTrue( e.InnerException is IndexNotFoundKernelException );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyDeleteIndexRuleSchemaRuleCommandToTheStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyDeleteIndexRuleSchemaRuleCommandToTheStore()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier super = newApplier(false);
			  BatchTransactionApplier @base = NewApplier( false );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier indexApplier = newIndexApplier();
			  BatchTransactionApplier indexApplier = NewIndexApplier();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplierFacade applier = new org.Neo4Net.kernel.impl.api.BatchTransactionApplierFacade(super, indexApplier);
			  BatchTransactionApplierFacade applier = new BatchTransactionApplierFacade( @base, indexApplier );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.DynamicRecord record = org.Neo4Net.kernel.impl.store.record.DynamicRecord.dynamicRecord(21, true);
			  DynamicRecord record = DynamicRecord.dynamicRecord( 21, true );
			  record.InUse = false;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Collection<org.Neo4Net.kernel.impl.store.record.DynamicRecord> recordsAfter = singletonList(record);
			  ICollection<DynamicRecord> recordsAfter = singletonList( record );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor rule = indexRule(0, 1, 2, new org.Neo4Net.Kernel.Api.Internal.schema.IndexProviderDescriptor("K", "X.Y"));
			  StoreIndexDescriptor rule = IndexRule( 0, 1, 2, new IndexProviderDescriptor( "K", "X.Y" ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command.SchemaRuleCommand command = new Command.SchemaRuleCommand(java.util.Collections.emptyList(), recordsAfter, rule);
			  Command.SchemaRuleCommand command = new Command.SchemaRuleCommand( Collections.emptyList(), recordsAfter, rule );

			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _schemaStore, times( 1 ) ).updateRecord( record );
			  verify( _indexingService, times( 1 ) ).dropIndex( rule );
			  verify( _cacheAccess, times( 1 ) ).removeSchemaRuleFromCache( command.Key );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyDeleteIndexRuleSchemaRuleCommandToTheStoreInRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyDeleteIndexRuleSchemaRuleCommandToTheStoreInRecovery()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplierFacade(newIndexApplier(), newApplier(true));
			  BatchTransactionApplier applier = NewApplierFacade( NewIndexApplier(), NewApplier(true) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.DynamicRecord record = org.Neo4Net.kernel.impl.store.record.DynamicRecord.dynamicRecord(21, true);
			  DynamicRecord record = DynamicRecord.dynamicRecord( 21, true );
			  record.InUse = false;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Collection<org.Neo4Net.kernel.impl.store.record.DynamicRecord> recordsAfter = singletonList(record);
			  ICollection<DynamicRecord> recordsAfter = singletonList( record );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor rule = indexRule(0, 1, 2, new org.Neo4Net.Kernel.Api.Internal.schema.IndexProviderDescriptor("K", "X.Y"));
			  StoreIndexDescriptor rule = IndexRule( 0, 1, 2, new IndexProviderDescriptor( "K", "X.Y" ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command.SchemaRuleCommand command = new Command.SchemaRuleCommand(java.util.Collections.emptyList(), recordsAfter, rule);
			  Command.SchemaRuleCommand command = new Command.SchemaRuleCommand( Collections.emptyList(), recordsAfter, rule );

			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _schemaStore, times( 1 ) ).HighestPossibleIdInUse = record.Id;
			  verify( _schemaStore, times( 1 ) ).updateRecord( record );
			  verify( _indexingService, times( 1 ) ).dropIndex( rule );
			  verify( _cacheAccess, times( 1 ) ).removeSchemaRuleFromCache( command.Key );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyCreateUniquenessConstraintRuleSchemaRuleCommandToTheStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyCreateUniquenessConstraintRuleSchemaRuleCommandToTheStore()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplier(false);
			  BatchTransactionApplier applier = NewApplier( false );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.DynamicRecord record = org.Neo4Net.kernel.impl.store.record.DynamicRecord.dynamicRecord(21, true);
			  DynamicRecord record = DynamicRecord.dynamicRecord( 21, true );
			  record.SetCreated();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Collection<org.Neo4Net.kernel.impl.store.record.DynamicRecord> recordsAfter = singletonList(record);
			  ICollection<DynamicRecord> recordsAfter = singletonList( record );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.ConstraintRule rule = uniquenessConstraintRule(0L, 1, 2, 3L);
			  ConstraintRule rule = UniquenessConstraintRule( 0L, 1, 2, 3L );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command.SchemaRuleCommand command = new Command.SchemaRuleCommand(java.util.Collections.emptyList(), recordsAfter, rule);
			  Command.SchemaRuleCommand command = new Command.SchemaRuleCommand( Collections.emptyList(), recordsAfter, rule );

			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _schemaStore, times( 1 ) ).updateRecord( record );
			  verify( _metaDataStore, times( 1 ) ).LatestConstraintIntroducingTx = _transactionId;
			  verify( _cacheAccess, times( 1 ) ).addSchemaRule( rule );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyCreateUniquenessConstraintRuleSchemaRuleCommandToTheStoreInRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyCreateUniquenessConstraintRuleSchemaRuleCommandToTheStoreInRecovery()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplier(true);
			  BatchTransactionApplier applier = NewApplier( true );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.DynamicRecord record = org.Neo4Net.kernel.impl.store.record.DynamicRecord.dynamicRecord(21, true);
			  DynamicRecord record = DynamicRecord.dynamicRecord( 21, true );
			  record.SetCreated();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Collection<org.Neo4Net.kernel.impl.store.record.DynamicRecord> recordsAfter = singletonList(record);
			  ICollection<DynamicRecord> recordsAfter = singletonList( record );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.ConstraintRule rule = uniquenessConstraintRule(0L, 1, 2, 3L);
			  ConstraintRule rule = UniquenessConstraintRule( 0L, 1, 2, 3L );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command.SchemaRuleCommand command = new Command.SchemaRuleCommand(java.util.Collections.emptyList(), recordsAfter, rule);
			  Command.SchemaRuleCommand command = new Command.SchemaRuleCommand( Collections.emptyList(), recordsAfter, rule );

			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _schemaStore, times( 1 ) ).HighestPossibleIdInUse = record.Id;
			  verify( _schemaStore, times( 1 ) ).updateRecord( record );
			  verify( _metaDataStore, times( 1 ) ).LatestConstraintIntroducingTx = _transactionId;
			  verify( _cacheAccess, times( 1 ) ).addSchemaRule( rule );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyUpdateUniquenessConstraintRuleSchemaRuleCommandToTheStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyUpdateUniquenessConstraintRuleSchemaRuleCommandToTheStore()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplier(false);
			  BatchTransactionApplier applier = NewApplier( false );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.DynamicRecord record = org.Neo4Net.kernel.impl.store.record.DynamicRecord.dynamicRecord(21, true);
			  DynamicRecord record = DynamicRecord.dynamicRecord( 21, true );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Collection<org.Neo4Net.kernel.impl.store.record.DynamicRecord> recordsAfter = singletonList(record);
			  ICollection<DynamicRecord> recordsAfter = singletonList( record );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.ConstraintRule rule = uniquenessConstraintRule(0L, 1, 2, 3L);
			  ConstraintRule rule = UniquenessConstraintRule( 0L, 1, 2, 3L );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command.SchemaRuleCommand command = new Command.SchemaRuleCommand(java.util.Collections.emptyList(), recordsAfter, rule);
			  Command.SchemaRuleCommand command = new Command.SchemaRuleCommand( Collections.emptyList(), recordsAfter, rule );

			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _schemaStore, times( 1 ) ).updateRecord( record );
			  verify( _metaDataStore, times( 1 ) ).LatestConstraintIntroducingTx = _transactionId;
			  verify( _cacheAccess, times( 1 ) ).addSchemaRule( rule );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyUpdateUniquenessConstraintRuleSchemaRuleCommandToTheStoreInRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyUpdateUniquenessConstraintRuleSchemaRuleCommandToTheStoreInRecovery()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplier(true);
			  BatchTransactionApplier applier = NewApplier( true );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.DynamicRecord record = org.Neo4Net.kernel.impl.store.record.DynamicRecord.dynamicRecord(21, true);
			  DynamicRecord record = DynamicRecord.dynamicRecord( 21, true );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Collection<org.Neo4Net.kernel.impl.store.record.DynamicRecord> recordsAfter = singletonList(record);
			  ICollection<DynamicRecord> recordsAfter = singletonList( record );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.ConstraintRule rule = uniquenessConstraintRule(0L, 1, 2, 3L);
			  ConstraintRule rule = UniquenessConstraintRule( 0L, 1, 2, 3L );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command.SchemaRuleCommand command = new Command.SchemaRuleCommand(java.util.Collections.emptyList(), recordsAfter, rule);
			  Command.SchemaRuleCommand command = new Command.SchemaRuleCommand( Collections.emptyList(), recordsAfter, rule );

			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _schemaStore, times( 1 ) ).HighestPossibleIdInUse = record.Id;
			  verify( _schemaStore, times( 1 ) ).updateRecord( record );
			  verify( _metaDataStore, times( 1 ) ).LatestConstraintIntroducingTx = _transactionId;
			  verify( _cacheAccess, times( 1 ) ).addSchemaRule( rule );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyDeleteUniquenessConstraintRuleSchemaRuleCommandToTheStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyDeleteUniquenessConstraintRuleSchemaRuleCommandToTheStore()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplier(false);
			  BatchTransactionApplier applier = NewApplier( false );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.DynamicRecord record = org.Neo4Net.kernel.impl.store.record.DynamicRecord.dynamicRecord(21, true);
			  DynamicRecord record = DynamicRecord.dynamicRecord( 21, true );
			  record.InUse = false;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Collection<org.Neo4Net.kernel.impl.store.record.DynamicRecord> recordsAfter = singletonList(record);
			  ICollection<DynamicRecord> recordsAfter = singletonList( record );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.ConstraintRule rule = uniquenessConstraintRule(0L, 1, 2, 3L);
			  ConstraintRule rule = UniquenessConstraintRule( 0L, 1, 2, 3L );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command.SchemaRuleCommand command = new Command.SchemaRuleCommand(java.util.Collections.emptyList(), recordsAfter, rule);
			  Command.SchemaRuleCommand command = new Command.SchemaRuleCommand( Collections.emptyList(), recordsAfter, rule );

			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _schemaStore, times( 1 ) ).updateRecord( record );
			  verify( _metaDataStore, never() ).LatestConstraintIntroducingTx = _transactionId;
			  verify( _cacheAccess, times( 1 ) ).removeSchemaRuleFromCache( command.Key );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyDeleteUniquenessConstraintRuleSchemaRuleCommandToTheStoreInRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyDeleteUniquenessConstraintRuleSchemaRuleCommandToTheStoreInRecovery()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplier(true);
			  BatchTransactionApplier applier = NewApplier( true );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.DynamicRecord record = org.Neo4Net.kernel.impl.store.record.DynamicRecord.dynamicRecord(21, true);
			  DynamicRecord record = DynamicRecord.dynamicRecord( 21, true );
			  record.InUse = false;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Collection<org.Neo4Net.kernel.impl.store.record.DynamicRecord> recordsAfter = singletonList(record);
			  ICollection<DynamicRecord> recordsAfter = singletonList( record );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.ConstraintRule rule = uniquenessConstraintRule(0L, 1, 2, 3L);
			  ConstraintRule rule = UniquenessConstraintRule( 0L, 1, 2, 3L );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command.SchemaRuleCommand command = new Command.SchemaRuleCommand(java.util.Collections.emptyList(), recordsAfter, rule);
			  Command.SchemaRuleCommand command = new Command.SchemaRuleCommand( Collections.emptyList(), recordsAfter, rule );

			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _schemaStore, times( 1 ) ).HighestPossibleIdInUse = record.Id;
			  verify( _schemaStore, times( 1 ) ).updateRecord( record );
			  verify( _metaDataStore, never() ).LatestConstraintIntroducingTx = _transactionId;
			  verify( _cacheAccess, times( 1 ) ).removeSchemaRuleFromCache( command.Key );
		 }

		 // NEO STORE COMMAND

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyNeoStoreCommandToTheStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyNeoStoreCommandToTheStore()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplier(false);
			  BatchTransactionApplier applier = NewApplier( false );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.NeoStoreRecord before = new org.Neo4Net.kernel.impl.store.record.NeoStoreRecord();
			  NeoStoreRecord before = new NeoStoreRecord();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.NeoStoreRecord after = new org.Neo4Net.kernel.impl.store.record.NeoStoreRecord();
			  NeoStoreRecord after = new NeoStoreRecord();
			  after.NextProp = 42;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command command = new Command.NeoStoreCommand(before, after);
			  Command command = new Command.NeoStoreCommand( before, after );

			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _metaDataStore, times( 1 ) ).GraphNextProp = after.NextProp;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldApplyNeoStoreCommandToTheStoreInRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldApplyNeoStoreCommandToTheStoreInRecovery()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier = newApplier(true);
			  BatchTransactionApplier applier = NewApplier( true );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.NeoStoreRecord before = new org.Neo4Net.kernel.impl.store.record.NeoStoreRecord();
			  NeoStoreRecord before = new NeoStoreRecord();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.store.record.NeoStoreRecord after = new org.Neo4Net.kernel.impl.store.record.NeoStoreRecord();
			  NeoStoreRecord after = new NeoStoreRecord();
			  after.NextProp = 42;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command command = new Command.NeoStoreCommand(before, after);
			  Command command = new Command.NeoStoreCommand( before, after );

			  // when
			  bool result = Apply( applier, command.handle, _transactionToApply );

			  // then
			  assertFalse( result );

			  verify( _metaDataStore, times( 1 ) ).GraphNextProp = after.NextProp;
		 }

		 private BatchTransactionApplier NewApplier( bool recovery )
		 {
			  BatchTransactionApplier applier = new NeoStoreBatchTransactionApplier( _neoStores, _cacheAccess, _lockService );
			  if ( recovery )
			  {
					applier = NewApplierFacade( new HighIdBatchTransactionApplier( _neoStores ), applier, new CacheInvalidationBatchTransactionApplier( _neoStores, _cacheAccess ) );
			  }
			  return applier;
		 }

		 private BatchTransactionApplier NewApplierFacade( params BatchTransactionApplier[] appliers )
		 {
			  return new BatchTransactionApplierFacade( appliers );
		 }

		 private BatchTransactionApplier NewIndexApplier()
		 {
			  return new IndexBatchTransactionApplier( _indexingService, _labelScanStoreSynchronizer, _indexUpdatesSync, _nodeStore, _neoStores.RelationshipStore, _propertyStore, _indexActivator );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean apply(org.Neo4Net.kernel.impl.api.BatchTransactionApplier applier, org.Neo4Net.kernel.impl.transaction.command.CommandHandlerContract.ApplyFunction function, org.Neo4Net.kernel.impl.api.TransactionToApply transactionToApply) throws Exception
		 private bool Apply( BatchTransactionApplier applier, ApplyFunction function, TransactionToApply transactionToApply )
		 {
			  try
			  {
					return CommandHandlerContract.Apply( applier, function, transactionToApply );
			  }
			  finally
			  {
					_indexActivator.close();
			  }
		 }

		 // SCHEMA RULE COMMAND

		 public static StoreIndexDescriptor IndexRule( long id, int label, int propertyKeyId, IndexProviderDescriptor providerDescriptor )
		 {
			  return IndexDescriptorFactory.forSchema( forLabel( label, propertyKeyId ), providerDescriptor ).withId( id );
		 }

		 private static StoreIndexDescriptor ConstraintIndexRule( long id, int label, int propertyKeyId, IndexProviderDescriptor providerDescriptor, long? owningConstraint )
		 {
			  return IndexDescriptorFactory.uniqueForSchema( forLabel( label, propertyKeyId ), providerDescriptor ).withIds( id, owningConstraint.Value );
		 }

		 private static ConstraintRule UniquenessConstraintRule( long id, int labelId, int propertyKeyId, long ownedIndexRule )
		 {
			  return ConstraintRule.constraintRule( id, ConstraintDescriptorFactory.uniqueForLabel( labelId, propertyKeyId ), ownedIndexRule );
		 }
	}

}