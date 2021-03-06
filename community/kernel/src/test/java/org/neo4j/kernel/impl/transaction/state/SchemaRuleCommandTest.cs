﻿/*
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
namespace Org.Neo4j.Kernel.impl.transaction.state
{
	using Test = org.junit.Test;

	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using SchemaDescriptorPredicates = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptorPredicates;
	using LabelScanWriter = Org.Neo4j.Kernel.api.labelscan.LabelScanWriter;
	using ConstraintDescriptorFactory = Org.Neo4j.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using TestIndexDescriptorFactory = Org.Neo4j.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using BatchTransactionApplier = Org.Neo4j.Kernel.Impl.Api.BatchTransactionApplier;
	using TransactionToApply = Org.Neo4j.Kernel.Impl.Api.TransactionToApply;
	using IndexingService = Org.Neo4j.Kernel.Impl.Api.index.IndexingService;
	using IndexingUpdateService = Org.Neo4j.Kernel.Impl.Api.index.IndexingUpdateService;
	using CacheAccessBackDoor = Org.Neo4j.Kernel.impl.core.CacheAccessBackDoor;
	using LockService = Org.Neo4j.Kernel.impl.locking.LockService;
	using MetaDataStore = Org.Neo4j.Kernel.impl.store.MetaDataStore;
	using NeoStores = Org.Neo4j.Kernel.impl.store.NeoStores;
	using NodeStore = Org.Neo4j.Kernel.impl.store.NodeStore;
	using PropertyStore = Org.Neo4j.Kernel.impl.store.PropertyStore;
	using SchemaStore = Org.Neo4j.Kernel.impl.store.SchemaStore;
	using ConstraintRule = Org.Neo4j.Kernel.impl.store.record.ConstraintRule;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using SchemaRecord = Org.Neo4j.Kernel.impl.store.record.SchemaRecord;
	using SchemaRuleSerialization = Org.Neo4j.Kernel.impl.store.record.SchemaRuleSerialization;
	using BaseCommandReader = Org.Neo4j.Kernel.impl.transaction.command.BaseCommandReader;
	using Command = Org.Neo4j.Kernel.impl.transaction.command.Command;
	using SchemaRuleCommand = Org.Neo4j.Kernel.impl.transaction.command.Command.SchemaRuleCommand;
	using CommandHandlerContract = Org.Neo4j.Kernel.impl.transaction.command.CommandHandlerContract;
	using IndexActivator = Org.Neo4j.Kernel.impl.transaction.command.IndexActivator;
	using IndexBatchTransactionApplier = Org.Neo4j.Kernel.impl.transaction.command.IndexBatchTransactionApplier;
	using IndexUpdatesWork = Org.Neo4j.Kernel.impl.transaction.command.IndexUpdatesWork;
	using LabelUpdateWork = Org.Neo4j.Kernel.impl.transaction.command.LabelUpdateWork;
	using NeoStoreBatchTransactionApplier = Org.Neo4j.Kernel.impl.transaction.command.NeoStoreBatchTransactionApplier;
	using PhysicalLogCommandReaderV3_0_2 = Org.Neo4j.Kernel.impl.transaction.command.PhysicalLogCommandReaderV3_0_2;
	using InMemoryClosableChannel = Org.Neo4j.Kernel.impl.transaction.log.InMemoryClosableChannel;
	using PhysicalTransactionRepresentation = Org.Neo4j.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using SchemaRule = Org.Neo4j.Storageengine.Api.schema.SchemaRule;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;
	using Org.Neo4j.Util.concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class SchemaRuleCommandTest
	{
		private bool InstanceFieldsInitialized = false;

		public SchemaRuleCommandTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_storeApplier = new NeoStoreBatchTransactionApplier( _neoStores, mock( typeof( CacheAccessBackDoor ) ), LockService.NO_LOCK_SERVICE );
			_labelScanStoreSynchronizer = new WorkSync<System.Func<LabelScanWriter>, LabelUpdateWork>( _labelScanStore );
			_indexUpdatesSync = new WorkSync<IndexingUpdateService, IndexUpdatesWork>( _indexes );
			_indexApplier = new IndexBatchTransactionApplier( _indexes, _labelScanStoreSynchronizer, _indexUpdatesSync, mock( typeof( NodeStore ) ), _neoStores.RelationshipStore, _propertyStore, new IndexActivator( _indexes ) );
			_rule = TestIndexDescriptorFactory.forLabel( _labelId, _propertyKey ).withId( _id );
		}


		 private readonly int _labelId = 2;
		 private readonly int _propertyKey = 8;
		 private readonly long _id = 0;
		 private readonly long _txId = 1337L;
		 private readonly NeoStores _neoStores = mock( typeof( NeoStores ) );
		 private readonly MetaDataStore _metaDataStore = mock( typeof( MetaDataStore ) );
		 private readonly SchemaStore _schemaStore = mock( typeof( SchemaStore ) );
		 private readonly IndexingService _indexes = mock( typeof( IndexingService ) );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private final System.Func<org.neo4j.kernel.api.labelscan.LabelScanWriter> labelScanStore = mock(System.Func.class);
		 private readonly System.Func<LabelScanWriter> _labelScanStore = mock( typeof( System.Func ) );
		 private NeoStoreBatchTransactionApplier _storeApplier;
		 private WorkSync<System.Func<LabelScanWriter>, LabelUpdateWork> _labelScanStoreSynchronizer = new WorkSync<System.Func<LabelScanWriter>, LabelUpdateWork>( _labelScanStore );
		 private WorkSync<IndexingUpdateService, IndexUpdatesWork> _indexUpdatesSync;
		 private readonly PropertyStore _propertyStore = mock( typeof( PropertyStore ) );
		 private IndexBatchTransactionApplier _indexApplier;
		 private readonly BaseCommandReader _reader = new PhysicalLogCommandReaderV3_0_2();
		 private StoreIndexDescriptor _rule;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteCreatedSchemaRuleToStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteCreatedSchemaRuleToStore()
		 {
			  // GIVEN
			  SchemaRecord beforeRecords = Serialize( _rule, _id, false, false );
			  SchemaRecord afterRecords = Serialize( _rule, _id, true, true );

			  when( _neoStores.SchemaStore ).thenReturn( _schemaStore );

			  // WHEN
			  VisitSchemaRuleCommand( _storeApplier, new Command.SchemaRuleCommand( beforeRecords, afterRecords, _rule ) );

			  // THEN
			  verify( _schemaStore ).updateRecord( Iterables.first( afterRecords ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateIndexForCreatedSchemaRule() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateIndexForCreatedSchemaRule()
		 {
			  // GIVEN
			  SchemaRecord beforeRecords = Serialize( _rule, _id, false, false );
			  SchemaRecord afterRecords = Serialize( _rule, _id, true, true );

			  when( _neoStores.SchemaStore ).thenReturn( _schemaStore );

			  // WHEN
			  VisitSchemaRuleCommand( _indexApplier, new Command.SchemaRuleCommand( beforeRecords, afterRecords, _rule ) );

			  // THEN
			  verify( _indexes ).createIndexes( _rule );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSetLatestConstraintRule() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSetLatestConstraintRule()
		 {
			  // Given
			  SchemaRecord beforeRecords = Serialize( _rule, _id, true, true );
			  SchemaRecord afterRecords = Serialize( _rule, _id, true, false );

			  when( _neoStores.SchemaStore ).thenReturn( _schemaStore );
			  when( _neoStores.MetaDataStore ).thenReturn( _metaDataStore );

			  ConstraintRule schemaRule = ConstraintRule.constraintRule( _id, ConstraintDescriptorFactory.uniqueForLabel( _labelId, _propertyKey ), 0 );

			  // WHEN
			  VisitSchemaRuleCommand( _storeApplier, new Command.SchemaRuleCommand( beforeRecords, afterRecords, schemaRule ) );

			  // THEN
			  verify( _schemaStore ).updateRecord( Iterables.first( afterRecords ) );
			  verify( _metaDataStore ).LatestConstraintIntroducingTx = _txId;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDropSchemaRuleFromStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDropSchemaRuleFromStore()
		 {
			  // GIVEN
			  SchemaRecord beforeRecords = Serialize( _rule, _id, true, true );
			  SchemaRecord afterRecords = Serialize( _rule, _id, false, false );

			  when( _neoStores.SchemaStore ).thenReturn( _schemaStore );

			  // WHEN
			  VisitSchemaRuleCommand( _storeApplier, new Command.SchemaRuleCommand( beforeRecords, afterRecords, _rule ) );

			  // THEN
			  verify( _schemaStore ).updateRecord( Iterables.first( afterRecords ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDropSchemaRuleFromIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDropSchemaRuleFromIndex()
		 {
			  // GIVEN
			  SchemaRecord beforeRecords = Serialize( _rule, _id, true, true );
			  SchemaRecord afterRecords = Serialize( _rule, _id, false, false );

			  when( _neoStores.SchemaStore ).thenReturn( _schemaStore );

			  // WHEN
			  VisitSchemaRuleCommand( _indexApplier, new Command.SchemaRuleCommand( beforeRecords, afterRecords, _rule ) );

			  // THEN
			  verify( _indexes ).dropIndex( _rule );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteSchemaRuleToLog() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteSchemaRuleToLog()
		 {
			  // GIVEN
			  SchemaRecord beforeRecords = Serialize( _rule, _id, false, false );
			  SchemaRecord afterRecords = Serialize( _rule, _id, true, true );

			  Command.SchemaRuleCommand command = new Command.SchemaRuleCommand( beforeRecords, afterRecords, _rule );
			  InMemoryClosableChannel buffer = new InMemoryClosableChannel();

			  when( _neoStores.SchemaStore ).thenReturn( _schemaStore );

			  // WHEN
			  command.Serialize( buffer );
			  Command readCommand = _reader.read( buffer );

			  // THEN
			  assertThat( readCommand, instanceOf( typeof( Command.SchemaRuleCommand ) ) );

			  AssertSchemaRule( ( Command.SchemaRuleCommand )readCommand );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecreateSchemaRuleWhenDeleteCommandReadFromDisk() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecreateSchemaRuleWhenDeleteCommandReadFromDisk()
		 {
			  // GIVEN
			  SchemaRecord beforeRecords = Serialize( _rule, _id, true, true );
			  SchemaRecord afterRecords = Serialize( _rule, _id, false, false );

			  Command.SchemaRuleCommand command = new Command.SchemaRuleCommand( beforeRecords, afterRecords, _rule );
			  InMemoryClosableChannel buffer = new InMemoryClosableChannel();
			  when( _neoStores.SchemaStore ).thenReturn( _schemaStore );

			  // WHEN
			  command.Serialize( buffer );
			  Command readCommand = _reader.read( buffer );

			  // THEN
			  assertThat( readCommand, instanceOf( typeof( Command.SchemaRuleCommand ) ) );

			  AssertSchemaRule( ( Command.SchemaRuleCommand )readCommand );
		 }

		 private SchemaRecord Serialize( SchemaRule rule, long id, bool inUse, bool created )
		 {
			  DynamicRecord record = new DynamicRecord( id );
			  record.Data = SchemaRuleSerialization.serialize( rule );
			  if ( created )
			  {
					record.SetCreated();
			  }
			  if ( inUse )
			  {
					record.InUse = true;
			  }
			  return new SchemaRecord( singletonList( record ) );
		 }

		 private void AssertSchemaRule( Command.SchemaRuleCommand readSchemaCommand )
		 {
			  assertEquals( _id, readSchemaCommand.Key );
			  assertTrue( SchemaDescriptorPredicates.hasLabel( readSchemaCommand.SchemaRule, _labelId ) );
			  assertTrue( SchemaDescriptorPredicates.hasProperty( readSchemaCommand.SchemaRule, _propertyKey ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void visitSchemaRuleCommand(org.neo4j.kernel.impl.api.BatchTransactionApplier applier, org.neo4j.kernel.impl.transaction.command.Command.SchemaRuleCommand command) throws Exception
		 private void VisitSchemaRuleCommand( BatchTransactionApplier applier, Command.SchemaRuleCommand command )
		 {
			  TransactionToApply tx = new TransactionToApply( new PhysicalTransactionRepresentation( singletonList( command ) ), _txId );
			  CommandHandlerContract.apply( applier, tx );
		 }
	}

}