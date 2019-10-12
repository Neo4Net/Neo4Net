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
namespace Org.Neo4j.Kernel.impl.transaction.command
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using IndexProviderDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using LabelScanWriter = Org.Neo4j.Kernel.api.labelscan.LabelScanWriter;
	using TransactionApplier = Org.Neo4j.Kernel.Impl.Api.TransactionApplier;
	using TransactionToApply = Org.Neo4j.Kernel.Impl.Api.TransactionToApply;
	using IndexingService = Org.Neo4j.Kernel.Impl.Api.index.IndexingService;
	using IndexingUpdateService = Org.Neo4j.Kernel.Impl.Api.index.IndexingUpdateService;
	using NodeStore = Org.Neo4j.Kernel.impl.store.NodeStore;
	using PropertyStore = Org.Neo4j.Kernel.impl.store.PropertyStore;
	using RelationshipStore = Org.Neo4j.Kernel.impl.store.RelationshipStore;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using EntityType = Org.Neo4j.Storageengine.Api.EntityType;
	using IndexDescriptorFactory = Org.Neo4j.Storageengine.Api.schema.IndexDescriptorFactory;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;
	using Org.Neo4j.Util.concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.DynamicRecord.dynamicRecord;

	public class NeoTransactionIndexApplierTest
	{
		private bool InstanceFieldsInitialized = false;

		public NeoTransactionIndexApplierTest()
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
		}

		 private static readonly IndexProviderDescriptor _indexDescriptor = new IndexProviderDescriptor( "in-memory", "1.0" );

		 private readonly IndexingService _indexingService = mock( typeof( IndexingService ) );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private final System.Func<org.neo4j.kernel.api.labelscan.LabelScanWriter> labelScanStore = mock(System.Func.class);
		 private readonly System.Func<LabelScanWriter> _labelScanStore = mock( typeof( System.Func ) );
		 private readonly ICollection<DynamicRecord> _emptyDynamicRecords = Collections.emptySet();
		 private WorkSync<System.Func<LabelScanWriter>, LabelUpdateWork> _labelScanStoreSynchronizer = new WorkSync<System.Func<LabelScanWriter>, LabelUpdateWork>( _labelScanStore );
		 private WorkSync<IndexingUpdateService, IndexUpdatesWork> _indexUpdatesSync;
		 private readonly TransactionToApply _transactionToApply = mock( typeof( TransactionToApply ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  when( _transactionToApply.transactionId() ).thenReturn(1L);
			  when( _indexingService.convertToIndexUpdates( any(), eq(EntityType.NODE) ) ).thenAnswer(o => Iterables.empty());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateLabelStoreScanOnNodeCommands() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUpdateLabelStoreScanOnNodeCommands()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IndexBatchTransactionApplier applier = newIndexTransactionApplier();
			  IndexBatchTransactionApplier applier = NewIndexTransactionApplier();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.store.record.NodeRecord before = new org.neo4j.kernel.impl.store.record.NodeRecord(11);
			  NodeRecord before = new NodeRecord( 11 );
			  before.SetLabelField( 17, _emptyDynamicRecords );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.store.record.NodeRecord after = new org.neo4j.kernel.impl.store.record.NodeRecord(12);
			  NodeRecord after = new NodeRecord( 12 );
			  after.SetLabelField( 18, _emptyDynamicRecords );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command.NodeCommand command = new Command.NodeCommand(before, after);
			  Command.NodeCommand command = new Command.NodeCommand( before, after );

			  LabelScanWriter labelScanWriter = mock( typeof( LabelScanWriter ) );
			  when( _labelScanStore.get() ).thenReturn(labelScanWriter);

			  // when
			  bool result;
			  using ( TransactionApplier txApplier = applier.StartTx( _transactionToApply ) )
			  {
					result = txApplier.VisitNodeCommand( command );
			  }
			  // then
			  assertFalse( result );
		 }

		 private IndexBatchTransactionApplier NewIndexTransactionApplier()
		 {
			  PropertyStore propertyStore = mock( typeof( PropertyStore ) );
			  return new IndexBatchTransactionApplier( _indexingService, _labelScanStoreSynchronizer, _indexUpdatesSync, mock( typeof( NodeStore ) ), mock( typeof( RelationshipStore ) ), propertyStore, new IndexActivator( _indexingService ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateIndexGivenCreateSchemaRuleCommand() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateIndexGivenCreateSchemaRuleCommand()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.storageengine.api.schema.StoreIndexDescriptor indexRule = indexRule(1, 42, 42, INDEX_DESCRIPTOR);
			  StoreIndexDescriptor indexRule = indexRule( 1, 42, 42, _indexDescriptor );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IndexBatchTransactionApplier applier = newIndexTransactionApplier();
			  IndexBatchTransactionApplier applier = NewIndexTransactionApplier();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command.SchemaRuleCommand command = new Command.SchemaRuleCommand(emptyDynamicRecords, singleton(createdDynamicRecord(1)), indexRule);
			  Command.SchemaRuleCommand command = new Command.SchemaRuleCommand( _emptyDynamicRecords, singleton( CreatedDynamicRecord( 1 ) ), indexRule );

			  // When
			  bool result;
			  using ( TransactionApplier txApplier = applier.StartTx( _transactionToApply ) )
			  {
					result = txApplier.VisitSchemaRuleCommand( command );
			  }

			  // Then
			  assertFalse( result );
			  verify( _indexingService ).createIndexes( indexRule );
		 }

		 private StoreIndexDescriptor IndexRule( long ruleId, int labelId, int propertyId, IndexProviderDescriptor descriptor )
		 {
			  return IndexDescriptorFactory.forSchema( forLabel( labelId, propertyId ), descriptor ).withId( ruleId );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDropIndexGivenDropSchemaRuleCommand() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDropIndexGivenDropSchemaRuleCommand()
		 {
			  // Given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.storageengine.api.schema.StoreIndexDescriptor indexRule = indexRule(1, 42, 42, INDEX_DESCRIPTOR);
			  StoreIndexDescriptor indexRule = indexRule( 1, 42, 42, _indexDescriptor );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final IndexBatchTransactionApplier applier = newIndexTransactionApplier();
			  IndexBatchTransactionApplier applier = NewIndexTransactionApplier();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Command.SchemaRuleCommand command = new Command.SchemaRuleCommand(singleton(createdDynamicRecord(1)), singleton(dynamicRecord(1, false)), indexRule);
			  Command.SchemaRuleCommand command = new Command.SchemaRuleCommand( singleton( CreatedDynamicRecord( 1 ) ), singleton( dynamicRecord( 1, false ) ), indexRule );

			  // When
			  bool result;
			  using ( TransactionApplier txApplier = applier.StartTx( _transactionToApply ) )
			  {
					result = txApplier.VisitSchemaRuleCommand( command );
			  }

			  // Then
			  assertFalse( result );
			  verify( _indexingService ).dropIndex( indexRule );
		 }

		 private static DynamicRecord CreatedDynamicRecord( long id )
		 {
			  DynamicRecord record = dynamicRecord( id, true );
			  record.SetCreated();
			  return record;
		 }
	}

}