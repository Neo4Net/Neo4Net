using System;
using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{
	using LongIterable = org.eclipse.collections.api.LongIterable;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongArrayList = org.eclipse.collections.impl.list.mutable.primitive.LongArrayList;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using InvocationOnMock = org.mockito.invocation.InvocationOnMock;
	using Answer = org.mockito.stubbing.Answer;


	using Direction = Neo4Net.Graphdb.Direction;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using TransactionFailureException = Neo4Net.Internal.Kernel.Api.exceptions.TransactionFailureException;
	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;
	using Neo4Net.Kernel.Api.Index;
	using BatchTransactionApplier = Neo4Net.Kernel.Impl.Api.BatchTransactionApplier;
	using CommandVisitor = Neo4Net.Kernel.Impl.Api.CommandVisitor;
	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using Neo4Net.Kernel.Impl.Api.index;
	using EntityUpdates = Neo4Net.Kernel.Impl.Api.index.EntityUpdates;
	using IndexingUpdateService = Neo4Net.Kernel.Impl.Api.index.IndexingUpdateService;
	using PropertyCommandsExtractor = Neo4Net.Kernel.Impl.Api.index.PropertyCommandsExtractor;
	using PropertyPhysicalToLogicalConverter = Neo4Net.Kernel.Impl.Api.index.PropertyPhysicalToLogicalConverter;
	using CacheAccessBackDoor = Neo4Net.Kernel.impl.core.CacheAccessBackDoor;
	using Lock = Neo4Net.Kernel.impl.locking.Lock;
	using LockService = Neo4Net.Kernel.impl.locking.LockService;
	using NoOpClient = Neo4Net.Kernel.impl.locking.NoOpClient;
	using DynamicArrayStore = Neo4Net.Kernel.impl.store.DynamicArrayStore;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using Neo4Net.Kernel.impl.store;
	using Standard = Neo4Net.Kernel.impl.store.format.standard.Standard;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PropertyBlock = Neo4Net.Kernel.Impl.Store.Records.PropertyBlock;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using CommittedTransactionRepresentation = Neo4Net.Kernel.impl.transaction.CommittedTransactionRepresentation;
	using TransactionRepresentation = Neo4Net.Kernel.impl.transaction.TransactionRepresentation;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using NodeCommand = Neo4Net.Kernel.impl.transaction.command.Command.NodeCommand;
	using PropertyCommand = Neo4Net.Kernel.impl.transaction.command.Command.PropertyCommand;
	using RelationshipCommand = Neo4Net.Kernel.impl.transaction.command.Command.RelationshipCommand;
	using RelationshipGroupCommand = Neo4Net.Kernel.impl.transaction.command.Command.RelationshipGroupCommand;
	using CommandHandlerContract = Neo4Net.Kernel.impl.transaction.command.CommandHandlerContract;
	using NeoStoreBatchTransactionApplier = Neo4Net.Kernel.impl.transaction.command.NeoStoreBatchTransactionApplier;
	using FlushableChannel = Neo4Net.Kernel.impl.transaction.log.FlushableChannel;
	using InMemoryVersionableReadableClosablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.InMemoryVersionableReadableClosablePositionAwareChannel;
	using Neo4Net.Kernel.impl.transaction.log;
	using PhysicalTransactionRepresentation = Neo4Net.Kernel.impl.transaction.log.PhysicalTransactionRepresentation;
	using ReadableLogChannel = Neo4Net.Kernel.impl.transaction.log.ReadableLogChannel;
	using TransactionLogWriter = Neo4Net.Kernel.impl.transaction.log.TransactionLogWriter;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogEntryWriter = Neo4Net.Kernel.impl.transaction.log.entry.LogEntryWriter;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using IndexUpdates = Neo4Net.Kernel.impl.transaction.state.IndexUpdates;
	using IntegrityValidator = Neo4Net.Kernel.impl.transaction.state.IntegrityValidator;
	using OnlineIndexUpdates = Neo4Net.Kernel.impl.transaction.state.OnlineIndexUpdates;
	using PrepareTrackingRecordFormats = Neo4Net.Kernel.impl.transaction.state.PrepareTrackingRecordFormats;
	using Neo4Net.Kernel.impl.transaction.state;
	using RecordChangeSet = Neo4Net.Kernel.impl.transaction.state.RecordChangeSet;
	using EntityType = Neo4Net.Storageengine.Api.EntityType;
	using StorageCommand = Neo4Net.Storageengine.Api.StorageCommand;
	using SchemaRule = Neo4Net.Storageengine.Api.schema.SchemaRule;
	using NeoStoresRule = Neo4Net.Test.rule.NeoStoresRule;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.reset;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Direction.INCOMING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Direction.OUTGOING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.count;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.filter;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.SchemaDescriptorFactory.forRelType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.constraints.ConstraintDescriptorFactory.uniqueForLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.TestIndexProviderDescriptor.PROVIDER_DESCRIPTOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.ConstraintRule.constraintRule;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.FORCE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.NORMAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.schema.IndexDescriptorFactory.forSchema;

	public class TransactionRecordStateTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.NeoStoresRule neoStoresRule = new org.neo4j.test.rule.NeoStoresRule(getClass());
		 public readonly NeoStoresRule NeoStoresRule = new NeoStoresRule( this.GetType() );

		 private const string LONG_STRING = "string value long enough not to be stored as a short string";
		 private const int PROPERTY_ID1 = 1;
		 private const int PROPERTY_ID2 = 2;
		 private static readonly Value _value1 = Values.of( "first" );
		 private static readonly Value _value2 = Values.of( 4 );
		 private static readonly long[] _noLabels = new long[0];
		 private readonly long[] _oneLabelId = new long[]{ 3 };
		 private readonly long[] _secondLabelId = new long[]{ 4 };
		 private readonly long[] _bothLabelIds = new long[]{ 3, 4 };
		 private readonly IntegrityValidator _integrityValidator = mock( typeof( IntegrityValidator ) );
		 private RecordChangeSet _recordChangeSet;

		 private static void AssertRelationshipGroupDoesNotExist( RecordChangeSet recordChangeSet, NodeRecord node, int type )
		 {
			  assertNull( GetRelationshipGroup( recordChangeSet, node, type ) );
		 }

		 private static void AssertDenseRelationshipCounts( RecordChangeSet recordChangeSet, long nodeId, int type, int outCount, int inCount )
		 {
			  RelationshipGroupRecord group = GetRelationshipGroup( recordChangeSet, recordChangeSet.NodeRecords.getOrLoad( nodeId, null ).forReadingData(), type ).forReadingData();
			  assertNotNull( group );

			  RelationshipRecord rel;
			  long relId = group.FirstOut;
			  if ( relId != Record.NO_NEXT_RELATIONSHIP.intValue() )
			  {
					rel = recordChangeSet.RelRecords.getOrLoad( relId, null ).forReadingData();
					// count is stored in the back pointer of the first relationship in the chain
					assertEquals( "Stored relationship count for OUTGOING differs", outCount, rel.FirstPrevRel );
					assertEquals( "Manually counted relationships for OUTGOING differs", outCount, ManuallyCountRelationships( recordChangeSet, nodeId, relId ) );
			  }

			  relId = group.FirstIn;
			  if ( relId != Record.NO_NEXT_RELATIONSHIP.intValue() )
			  {
					rel = recordChangeSet.RelRecords.getOrLoad( relId, null ).forReadingData();
					assertEquals( "Stored relationship count for INCOMING differs", inCount, rel.SecondPrevRel );
					assertEquals( "Manually counted relationships for INCOMING differs", inCount, ManuallyCountRelationships( recordChangeSet, nodeId, relId ) );
			  }
		 }

		 private static RecordAccess_RecordProxy<RelationshipGroupRecord, int> GetRelationshipGroup( RecordChangeSet recordChangeSet, NodeRecord node, int type )
		 {
			  long groupId = node.NextRel;
			  long previousGroupId = Record.NO_NEXT_RELATIONSHIP.intValue();
			  while ( groupId != Record.NO_NEXT_RELATIONSHIP.intValue() )
			  {
					RecordAccess_RecordProxy<RelationshipGroupRecord, int> change = recordChangeSet.RelGroupRecords.getOrLoad( groupId, type );
					RelationshipGroupRecord record = change.ForReadingData();
					record.Prev = previousGroupId; // not persistent so not a "change"
					if ( record.Type == type )
					{
						 return change;
					}
					previousGroupId = groupId;
					groupId = record.Next;
			  }
			  return null;
		 }

		 private static int ManuallyCountRelationships( RecordChangeSet recordChangeSet, long nodeId, long firstRelId )
		 {
			  int count = 0;
			  long relId = firstRelId;
			  while ( relId != Record.NO_NEXT_RELATIONSHIP.intValue() )
			  {
					count++;
					RelationshipRecord record = recordChangeSet.RelRecords.getOrLoad( relId, null ).forReadingData();
					relId = record.FirstNode == nodeId ? record.FirstNextRel : record.SecondNextRel;
			  }
			  return count;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateEqualEntityPropertyUpdatesOnRecoveryOfCreatedEntities() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateEqualEntityPropertyUpdatesOnRecoveryOfCreatedEntities()
		 {
			  /* There was an issue where recovering a tx where a node with a label and a property
			   * was created resulted in two exact copies of NodePropertyUpdates. */

			  // GIVEN
			  NeoStores neoStores = NeoStoresRule.builder().build();
			  long nodeId = 0;
			  long relId = 1;
			  int labelId = 5;
			  int relTypeId = 4;
			  int propertyKeyId = 7;

			  // -- indexes
			  long nodeRuleId = 0;
			  TransactionRecordState recordState = NewTransactionRecordState( neoStores );
			  SchemaRule nodeRule = forSchema( forLabel( labelId, propertyKeyId ), PROVIDER_DESCRIPTOR ).withId( nodeRuleId );
			  recordState.CreateSchemaRule( nodeRule );
			  long relRuleId = 1;
			  SchemaRule relRule = forSchema( forRelType( relTypeId, propertyKeyId ), PROVIDER_DESCRIPTOR ).withId( relRuleId );
			  recordState.CreateSchemaRule( relRule );
			  Apply( neoStores, recordState );

			  // -- and a tx creating a node and a rel for those indexes
			  recordState = NewTransactionRecordState( neoStores );
			  recordState.NodeCreate( nodeId );
			  recordState.AddLabelToNode( labelId, nodeId );
			  recordState.NodeAddProperty( nodeId, propertyKeyId, Values.of( "Neo" ) );
			  recordState.RelCreate( relId, relTypeId, nodeId, nodeId );
			  recordState.RelAddProperty( relId, propertyKeyId, Values.of( "Oen" ) );

			  // WHEN
			  PhysicalTransactionRepresentation transaction = TransactionRepresentationOf( recordState );
			  PropertyCommandsExtractor extractor = new PropertyCommandsExtractor();
			  transaction.Accept( extractor );

			  // THEN
			  // -- later recovering that tx, there should be only one update for each type
			  assertTrue( extractor.ContainsAnyEntityOrPropertyUpdate() );
			  MutableLongSet recoveredNodeIds = new LongHashSet();
			  recoveredNodeIds.addAll( EntityIds( extractor.NodeCommands ) );
			  assertEquals( 1, recoveredNodeIds.size() );
			  assertEquals( nodeId, recoveredNodeIds.longIterator().next() );

			  MutableLongSet recoveredRelIds = new LongHashSet();
			  recoveredRelIds.addAll( EntityIds( extractor.RelationshipCommands ) );
			  assertEquals( 1, recoveredRelIds.size() );
			  assertEquals( relId, recoveredRelIds.longIterator().next() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteProperPropertyRecordsWhenOnlyChangingLinkage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldWriteProperPropertyRecordsWhenOnlyChangingLinkage()
		 {
			  /* There was an issue where GIVEN:
			   *
			   *   Legend: () = node, [] = property record
			   *
			   *   ()-->[0:block{size:1}]
			   *
			   * WHEN adding a new property record in front of if, not changing any data in that record i.e:
			   *
			   *   ()-->[1:block{size:4}]-->[0:block{size:1}]
			   *
			   * The state of property record 0 would be that it had loaded value records for that block,
			   * but those value records weren't heavy, so writing that record to the log would fail
			   * w/ an assertion data != null.
			   */

			  // GIVEN
			  NeoStores neoStores = NeoStoresRule.builder().build();
			  TransactionRecordState recordState = NewTransactionRecordState( neoStores );
			  int nodeId = 0;
			  recordState.NodeCreate( nodeId );
			  int index = 0;
			  recordState.NodeAddProperty( nodeId, index, String( 70 ) ); // will require a block of size 1
			  Apply( neoStores, recordState );

			  // WHEN
			  recordState = NewTransactionRecordState( neoStores );
			  int index2 = 1;
			  recordState.NodeAddProperty( nodeId, index2, String( 40 ) ); // will require a block of size 4

			  // THEN
			  PhysicalTransactionRepresentation representation = TransactionRepresentationOf( recordState );
			  representation.Accept( command => ( ( Command )command ).handle( new CommandVisitor_AdapterAnonymousInnerClass( this ) ) );
		 }

		 private class CommandVisitor_AdapterAnonymousInnerClass : Neo4Net.Kernel.Impl.Api.CommandVisitor_Adapter
		 {
			 private readonly TransactionRecordStateTest _outerInstance;

			 public CommandVisitor_AdapterAnonymousInnerClass( TransactionRecordStateTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override bool visitPropertyCommand( Command.PropertyCommand command )
			 {
				  // THEN
				  verifyPropertyRecord( command.Before );
				  verifyPropertyRecord( command.After );
				  return false;
			 }

			 private void verifyPropertyRecord( PropertyRecord record )
			 {
				  if ( record.PrevProp != Record.NO_NEXT_PROPERTY.intValue() )
				  {
						foreach ( PropertyBlock block in record )
						{
							 assertTrue( block.Light );
						}
				  }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConvertLabelAdditionToNodePropertyUpdates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldConvertLabelAdditionToNodePropertyUpdates()
		 {
			  // GIVEN
			  NeoStores neoStores = NeoStoresRule.builder().build();
			  long nodeId = 0;
			  TransactionRecordState recordState = NewTransactionRecordState( neoStores );
			  Value value1 = Values.of( LONG_STRING );
			  Value value2 = Values.of( LONG_STRING.GetBytes() );
			  recordState.NodeCreate( nodeId );
			  recordState.NodeAddProperty( nodeId, PROPERTY_ID1, value1 );
			  recordState.NodeAddProperty( nodeId, PROPERTY_ID2, value2 );
			  Apply( neoStores, recordState );

			  // WHEN
			  recordState = NewTransactionRecordState( neoStores );
			  AddLabelsToNode( recordState, nodeId, _oneLabelId );
			  IEnumerable<EntityUpdates> indexUpdates = IndexUpdatesOf( neoStores, recordState );

			  // THEN
			  EntityUpdates expected = EntityUpdates.forEntity( nodeId, false ).withTokens( _noLabels ).withTokensAfter( _oneLabelId ).build();
			  assertEquals( expected, Iterables.single( indexUpdates ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConvertMixedLabelAdditionAndSetPropertyToNodePropertyUpdates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldConvertMixedLabelAdditionAndSetPropertyToNodePropertyUpdates()
		 {
			  // GIVEN
			  NeoStores neoStores = NeoStoresRule.builder().build();
			  long nodeId = 0;
			  TransactionRecordState recordState = NewTransactionRecordState( neoStores );
			  recordState.NodeCreate( nodeId );
			  recordState.NodeAddProperty( nodeId, PROPERTY_ID1, _value1 );
			  AddLabelsToNode( recordState, nodeId, _oneLabelId );
			  Apply( neoStores, recordState );

			  // WHEN
			  recordState = NewTransactionRecordState( neoStores );
			  recordState.NodeAddProperty( nodeId, PROPERTY_ID2, _value2 );
			  AddLabelsToNode( recordState, nodeId, _secondLabelId );
			  IEnumerable<EntityUpdates> indexUpdates = IndexUpdatesOf( neoStores, recordState );

			  // THEN
			  EntityUpdates expected = EntityUpdates.forEntity( nodeId, false ).withTokens( _oneLabelId ).withTokensAfter( _bothLabelIds ).added( PROPERTY_ID2, _value2 ).build();
			  assertEquals( expected, Iterables.single( indexUpdates ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConvertLabelRemovalToNodePropertyUpdates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldConvertLabelRemovalToNodePropertyUpdates()
		 {
			  // GIVEN
			  NeoStores neoStores = NeoStoresRule.builder().build();
			  long nodeId = 0;
			  TransactionRecordState recordState = NewTransactionRecordState( neoStores );
			  recordState.NodeCreate( nodeId );
			  recordState.NodeAddProperty( nodeId, PROPERTY_ID1, _value1 );
			  recordState.NodeAddProperty( nodeId, PROPERTY_ID2, _value2 );
			  AddLabelsToNode( recordState, nodeId, _oneLabelId );
			  Apply( neoStores, recordState );

			  // WHEN
			  recordState = NewTransactionRecordState( neoStores );
			  RemoveLabelsFromNode( recordState, nodeId, _oneLabelId );
			  IEnumerable<EntityUpdates> indexUpdates = IndexUpdatesOf( neoStores, recordState );

			  // THEN
			  EntityUpdates expected = EntityUpdates.forEntity( nodeId, false ).withTokens( _oneLabelId ).withTokensAfter( _noLabels ).build();
			  assertEquals( expected, Iterables.single( indexUpdates ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConvertMixedLabelRemovalAndRemovePropertyToNodePropertyUpdates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldConvertMixedLabelRemovalAndRemovePropertyToNodePropertyUpdates()
		 {
			  // GIVEN
			  NeoStores neoStores = NeoStoresRule.builder().build();
			  long nodeId = 0;
			  TransactionRecordState recordState = NewTransactionRecordState( neoStores );
			  recordState.NodeCreate( nodeId );
			  recordState.NodeAddProperty( nodeId, PROPERTY_ID1, _value1 );
			  AddLabelsToNode( recordState, nodeId, _bothLabelIds );
			  Apply( neoStores, recordState );

			  // WHEN
			  recordState = NewTransactionRecordState( neoStores );
			  recordState.NodeRemoveProperty( nodeId, PROPERTY_ID1 );
			  RemoveLabelsFromNode( recordState, nodeId, _secondLabelId );
			  IEnumerable<EntityUpdates> indexUpdates = IndexUpdatesOf( neoStores, recordState );

			  // THEN
			  EntityUpdates expected = EntityUpdates.forEntity( nodeId, false ).withTokens( _bothLabelIds ).withTokensAfter( _oneLabelId ).removed( PROPERTY_ID1, _value1 ).build();
			  assertEquals( expected, Iterables.single( indexUpdates ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConvertMixedLabelRemovalAndAddPropertyToNodePropertyUpdates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldConvertMixedLabelRemovalAndAddPropertyToNodePropertyUpdates()
		 {
			  // GIVEN
			  NeoStores neoStores = NeoStoresRule.builder().build();
			  long nodeId = 0;
			  TransactionRecordState recordState = NewTransactionRecordState( neoStores );
			  recordState.NodeCreate( nodeId );
			  recordState.NodeAddProperty( nodeId, PROPERTY_ID1, _value1 );
			  AddLabelsToNode( recordState, nodeId, _bothLabelIds );
			  Apply( neoStores, recordState );

			  // WHEN
			  recordState = NewTransactionRecordState( neoStores );
			  recordState.NodeAddProperty( nodeId, PROPERTY_ID2, _value2 );
			  RemoveLabelsFromNode( recordState, nodeId, _secondLabelId );
			  IEnumerable<EntityUpdates> indexUpdates = IndexUpdatesOf( neoStores, recordState );

			  // THEN
			  EntityUpdates expected = EntityUpdates.forEntity( nodeId, false ).withTokens( _bothLabelIds ).withTokensAfter( _oneLabelId ).added( PROPERTY_ID2, _value2 ).build();
			  assertEquals( expected, Iterables.single( indexUpdates ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConvertChangedPropertyToNodePropertyUpdates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldConvertChangedPropertyToNodePropertyUpdates()
		 {
			  // GIVEN
			  NeoStores neoStores = NeoStoresRule.builder().build();
			  int nodeId = 0;
			  TransactionRecordState recordState = NewTransactionRecordState( neoStores );
			  recordState.NodeCreate( nodeId );
			  recordState.NodeAddProperty( nodeId, PROPERTY_ID1, _value1 );
			  recordState.NodeAddProperty( nodeId, PROPERTY_ID2, _value2 );
			  Apply( neoStores, TransactionRepresentationOf( recordState ) );

			  // WHEN
			  Value newValue1 = Values.of( "new" );
			  Value newValue2 = Values.of( "new 2" );
			  recordState = NewTransactionRecordState( neoStores );
			  recordState.NodeChangeProperty( nodeId, PROPERTY_ID1, newValue1 );
			  recordState.NodeChangeProperty( nodeId, PROPERTY_ID2, newValue2 );
			  IEnumerable<EntityUpdates> indexUpdates = IndexUpdatesOf( neoStores, recordState );

			  // THEN
			  EntityUpdates expected = EntityUpdates.forEntity( nodeId, false ).changed( PROPERTY_ID1, _value1, newValue1 ).changed( PROPERTY_ID2, _value2, newValue2 ).build();
			  assertEquals( expected, Iterables.single( indexUpdates ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConvertRemovedPropertyToNodePropertyUpdates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldConvertRemovedPropertyToNodePropertyUpdates()
		 {
			  // GIVEN
			  NeoStores neoStores = NeoStoresRule.builder().build();
			  int nodeId = 0;
			  TransactionRecordState recordState = NewTransactionRecordState( neoStores );
			  recordState.NodeCreate( nodeId );
			  AddLabelsToNode( recordState, nodeId, _oneLabelId );
			  recordState.NodeAddProperty( nodeId, PROPERTY_ID1, _value1 );
			  recordState.NodeAddProperty( nodeId, PROPERTY_ID2, _value2 );
			  Apply( neoStores, TransactionRepresentationOf( recordState ) );

			  // WHEN
			  recordState = NewTransactionRecordState( neoStores );
			  recordState.NodeRemoveProperty( nodeId, PROPERTY_ID1 );
			  recordState.NodeRemoveProperty( nodeId, PROPERTY_ID2 );
			  IEnumerable<EntityUpdates> indexUpdates = IndexUpdatesOf( neoStores, recordState );

			  // THEN
			  EntityUpdates expected = EntityUpdates.forEntity( ( long ) nodeId, false ).withTokens( _oneLabelId ).removed( PROPERTY_ID1, _value1 ).removed( PROPERTY_ID2, _value2 ).build();
			  assertEquals( expected, Iterables.single( indexUpdates ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeleteDynamicLabelsForDeletedNode() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDeleteDynamicLabelsForDeletedNode()
		 {
			  // GIVEN a store that has got a node with a dynamic label record
			  NeoStores store = NeoStoresRule.builder().build();
			  BatchTransactionApplier applier = new NeoStoreBatchTransactionApplier( store, mock( typeof( CacheAccessBackDoor ) ), LockService.NO_LOCK_SERVICE );
			  AtomicLong nodeId = new AtomicLong();
			  AtomicLong dynamicLabelRecordId = new AtomicLong();
			  Apply( applier, Transaction( NodeWithDynamicLabelRecord( store, nodeId, dynamicLabelRecordId ) ) );
			  AssertDynamicLabelRecordInUse( store, dynamicLabelRecordId.get(), true );

			  // WHEN applying a transaction where the node is deleted
			  Apply( applier, Transaction( DeleteNode( store, nodeId.get() ) ) );

			  // THEN the dynamic label record should also be deleted
			  AssertDynamicLabelRecordInUse( store, dynamicLabelRecordId.get(), false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDeleteDynamicLabelsForDeletedNodeForRecoveredTransaction() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDeleteDynamicLabelsForDeletedNodeForRecoveredTransaction()
		 {
			  // GIVEN a store that has got a node with a dynamic label record
			  NeoStores store = NeoStoresRule.builder().build();
			  BatchTransactionApplier applier = new NeoStoreBatchTransactionApplier( store, mock( typeof( CacheAccessBackDoor ) ), LockService.NO_LOCK_SERVICE );
			  AtomicLong nodeId = new AtomicLong();
			  AtomicLong dynamicLabelRecordId = new AtomicLong();
			  Apply( applier, Transaction( NodeWithDynamicLabelRecord( store, nodeId, dynamicLabelRecordId ) ) );
			  AssertDynamicLabelRecordInUse( store, dynamicLabelRecordId.get(), true );

			  // WHEN applying a transaction, which has first round-tripped through a log (written then read)
			  TransactionRepresentation transaction = transaction( DeleteNode( store, nodeId.get() ) );
			  InMemoryVersionableReadableClosablePositionAwareChannel channel = new InMemoryVersionableReadableClosablePositionAwareChannel();
			  WriteToChannel( transaction, channel );
			  CommittedTransactionRepresentation recoveredTransaction = ReadFromChannel( channel );
			  // and applying that recovered transaction
			  Apply( applier, recoveredTransaction.TransactionRepresentation );

			  // THEN should have the dynamic label record should be deleted as well
			  AssertDynamicLabelRecordInUse( store, dynamicLabelRecordId.get(), false );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExtractCreatedCommandsInCorrectOrder() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldExtractCreatedCommandsInCorrectOrder()
		 {
			  // GIVEN
			  NeoStores neoStores = NeoStoresRule.builder().with(GraphDatabaseSettings.dense_node_threshold.name(), "1").build();
			  TransactionRecordState recordState = NewTransactionRecordState( neoStores );
			  long nodeId = 0;
			  long relId = 1;
			  recordState.NodeCreate( nodeId );
			  recordState.RelCreate( relId++, 0, nodeId, nodeId );
			  recordState.RelCreate( relId, 0, nodeId, nodeId );
			  recordState.NodeAddProperty( nodeId, 0, _value2 );

			  // WHEN
			  ICollection<StorageCommand> commands = new List<StorageCommand>();
			  recordState.ExtractCommands( commands );

			  // THEN
			  IEnumerator<StorageCommand> commandIterator = commands.GetEnumerator();

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCommand( commandIterator.next(), typeof(Command.PropertyCommand) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCommand( commandIterator.next(), typeof(Command.RelationshipCommand) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCommand( commandIterator.next(), typeof(Command.RelationshipCommand) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCommand( commandIterator.next(), typeof(Command.RelationshipGroupCommand) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCommand( commandIterator.next(), typeof(Command.NodeCommand) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( commandIterator.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExtractUpdateCommandsInCorrectOrder() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldExtractUpdateCommandsInCorrectOrder()
		 {
			  // GIVEN
			  NeoStores neoStores = NeoStoresRule.builder().with(GraphDatabaseSettings.dense_node_threshold.name(), "1").build();
			  TransactionRecordState recordState = NewTransactionRecordState( neoStores );
			  long nodeId = 0;
			  long relId1 = 1;
			  long relId2 = 2;
			  long relId3 = 3;
			  recordState.NodeCreate( nodeId );
			  recordState.RelCreate( relId1, 0, nodeId, nodeId );
			  recordState.RelCreate( relId2, 0, nodeId, nodeId );
			  recordState.NodeAddProperty( nodeId, 0, Values.of( 101 ) );
			  BatchTransactionApplier applier = new NeoStoreBatchTransactionApplier( neoStores, mock( typeof( CacheAccessBackDoor ) ), LockService.NO_LOCK_SERVICE );
			  Apply( applier, Transaction( recordState ) );

			  recordState = NewTransactionRecordState( neoStores );
			  recordState.NodeChangeProperty( nodeId, 0, Values.of( 102 ) );
			  recordState.RelCreate( relId3, 0, nodeId, nodeId );
			  recordState.RelAddProperty( relId1, 0, Values.of( 123 ) );

			  // WHEN
			  ICollection<StorageCommand> commands = new List<StorageCommand>();
			  recordState.ExtractCommands( commands );

			  // THEN
			  IEnumerator<StorageCommand> commandIterator = commands.GetEnumerator();

			  // added rel property
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCommand( commandIterator.next(), typeof(Command.PropertyCommand) );
			  // created relationship relId3
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCommand( commandIterator.next(), typeof(Command.RelationshipCommand) );
			  // rest is updates...
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCommand( commandIterator.next(), typeof(Command.PropertyCommand) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCommand( commandIterator.next(), typeof(Command.RelationshipCommand) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCommand( commandIterator.next(), typeof(Command.RelationshipCommand) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCommand( commandIterator.next(), typeof(Command.RelationshipGroupCommand) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCommand( commandIterator.next(), typeof(Command.NodeCommand) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( commandIterator.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIgnoreRelationshipGroupCommandsForGroupThatIsCreatedAndDeletedInThisTx() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIgnoreRelationshipGroupCommandsForGroupThatIsCreatedAndDeletedInThisTx()
		 {
			  /*
			   * This test verifies that there are no transaction commands generated for a state diff that contains a
			   * relationship group that is created and deleted in this tx. This case requires special handling because
			   * relationship groups can be created and then deleted from disjoint code paths. Look at
			   * TransactionRecordState.extractCommands() for more details.
			   *
			   * The test setup looks complicated but all it does is mock properly a NeoStoreTransactionContext to
			   * return an Iterable<RecordSet< that contains a RelationshipGroup record which has been created in this
			   * tx and also is set notInUse.
			   */
			  // Given:
			  // - dense node threshold of 5
			  // - node with 4 rels of type relationshipB and 1 rel of type relationshipB
			  NeoStores neoStore = NeoStoresRule.builder().with(GraphDatabaseSettings.dense_node_threshold.name(), "5").build();
			  int relationshipA = 0;
			  int relationshipB = 1;
			  TransactionRecordState state = NewTransactionRecordState( neoStore );
			  state.NodeCreate( 0 );
			  state.RelCreate( 0, relationshipA, 0, 0 );
			  state.RelCreate( 1, relationshipA, 0, 0 );
			  state.RelCreate( 2, relationshipA, 0, 0 );
			  state.RelCreate( 3, relationshipA, 0, 0 );
			  state.RelCreate( 4, relationshipB, 0, 0 );
			  Apply( neoStore, state );

			  // When doing a tx where a relationship of type A for the node is create and rel of type relationshipB is deleted
			  state = NewTransactionRecordState( neoStore );
			  state.RelCreate( 5, relationshipA, 0, 0 ); // here this node should be converted to dense and the groups should be created
			  state.RelDelete( 4 ); // here the group relationshipB should be delete

			  // Then
			  ICollection<StorageCommand> commands = new List<StorageCommand>();
			  state.ExtractCommands( commands );
			  Command.RelationshipGroupCommand group = SingleRelationshipGroupCommand( commands );
			  assertEquals( relationshipA, group.After.Type );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExtractDeleteCommandsInCorrectOrder() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldExtractDeleteCommandsInCorrectOrder()
		 {
			  // GIVEN
			  NeoStores neoStores = NeoStoresRule.builder().with(GraphDatabaseSettings.dense_node_threshold.name(), "1").build();
			  TransactionRecordState recordState = NewTransactionRecordState( neoStores );
			  long nodeId1 = 0;
			  long nodeId2 = 1;
			  long relId1 = 1;
			  long relId2 = 2;
			  long relId4 = 10;
			  recordState.NodeCreate( nodeId1 );
			  recordState.NodeCreate( nodeId2 );
			  recordState.RelCreate( relId1, 0, nodeId1, nodeId1 );
			  recordState.RelCreate( relId2, 0, nodeId1, nodeId1 );
			  recordState.RelCreate( relId4, 1, nodeId1, nodeId1 );
			  recordState.NodeAddProperty( nodeId1, 0, _value1 );
			  BatchTransactionApplier applier = new NeoStoreBatchTransactionApplier( neoStores, mock( typeof( CacheAccessBackDoor ) ), LockService.NO_LOCK_SERVICE );
			  Apply( applier, Transaction( recordState ) );

			  recordState = NewTransactionRecordState( neoStores );
			  recordState.RelDelete( relId4 );
			  recordState.NodeDelete( nodeId2 );
			  recordState.NodeRemoveProperty( nodeId1, 0 );

			  // WHEN
			  ICollection<StorageCommand> commands = new List<StorageCommand>();
			  recordState.ExtractCommands( commands );

			  // THEN
			  IEnumerator<StorageCommand> commandIterator = commands.GetEnumerator();

			  // updated rel group to not point to the deleted one below
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCommand( commandIterator.next(), typeof(Command.RelationshipGroupCommand) );
			  // updated node to point to the group after the deleted one
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCommand( commandIterator.next(), typeof(Command.NodeCommand) );
			  // rest is deletions below...
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCommand( commandIterator.next(), typeof(Command.PropertyCommand) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCommand( commandIterator.next(), typeof(Command.RelationshipCommand) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCommand( commandIterator.next(), typeof(Command.RelationshipGroupCommand) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCommand( commandIterator.next(), typeof(Command.NodeCommand) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( commandIterator.hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldValidateConstraintIndexAsPartOfExtraction() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldValidateConstraintIndexAsPartOfExtraction()
		 {
			  // GIVEN
			  NeoStores neoStores = NeoStoresRule.builder().build();
			  TransactionRecordState recordState = NewTransactionRecordState( neoStores );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long indexId = neoStores.getSchemaStore().nextId();
			  long indexId = neoStores.SchemaStore.nextId();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long constraintId = neoStores.getSchemaStore().nextId();
			  long constraintId = neoStores.SchemaStore.nextId();

			  recordState.CreateSchemaRule( constraintRule( constraintId, uniqueForLabel( 1, 1 ), indexId ) );

			  // WHEN
			  recordState.ExtractCommands( new List<StorageCommand>() );

			  // THEN
			  verify( _integrityValidator ).validateSchemaRule( any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateProperBeforeAndAfterPropertyCommandsWhenAddingProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateProperBeforeAndAfterPropertyCommandsWhenAddingProperty()
		 {
			  // GIVEN
			  NeoStores neoStores = NeoStoresRule.builder().build();
			  TransactionRecordState recordState = NewTransactionRecordState( neoStores );

			  int nodeId = 1;
			  recordState.NodeCreate( nodeId );

			  // WHEN
			  recordState.NodeAddProperty( nodeId, PROPERTY_ID1, _value1 );
			  ICollection<StorageCommand> commands = new List<StorageCommand>();
			  recordState.ExtractCommands( commands );
			  Command.PropertyCommand propertyCommand = SinglePropertyCommand( commands );

			  // THEN
			  PropertyRecord before = propertyCommand.Before;
			  assertFalse( before.InUse() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( before.GetEnumerator().hasNext() );

			  PropertyRecord after = propertyCommand.After;
			  assertTrue( after.InUse() );
			  assertEquals( 1, count( after ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConvertAddedPropertyToNodePropertyUpdates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldConvertAddedPropertyToNodePropertyUpdates()
		 {
			  // GIVEN
			  NeoStores neoStores = NeoStoresRule.builder().build();
			  long nodeId = 0;
			  TransactionRecordState recordState = NewTransactionRecordState( neoStores );

			  // WHEN
			  recordState.NodeCreate( nodeId );
			  AddLabelsToNode( recordState, nodeId, _oneLabelId );
			  recordState.NodeAddProperty( nodeId, PROPERTY_ID1, _value1 );
			  recordState.NodeAddProperty( nodeId, PROPERTY_ID2, _value2 );
			  IEnumerable<EntityUpdates> updates = IndexUpdatesOf( neoStores, recordState );

			  // THEN
			  EntityUpdates expected = EntityUpdates.forEntity( nodeId, false ).withTokens( _noLabels ).withTokensAfter( _oneLabelId ).added( PROPERTY_ID1, _value1 ).added( PROPERTY_ID2, _value2 ).build();
			  assertEquals( expected, Iterables.single( updates ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLockUpdatedNodes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLockUpdatedNodes()
		 {
			  // given
			  LockService locks = mock( typeof( LockService ), new AnswerAnonymousInnerClass( this ) );
			  NeoStores neoStores = NeoStoresRule.builder().build();
			  NodeStore nodeStore = neoStores.NodeStore;
			  long[] nodes = new long[] { nodeStore.NextId(), nodeStore.NextId(), nodeStore.NextId(), nodeStore.NextId(), nodeStore.NextId(), nodeStore.NextId(), nodeStore.NextId() };

			  {
					// create the node records that we will modify in our main tx.
					TransactionRecordState tx = NewTransactionRecordState( neoStores );
					for ( int i = 1; i < nodes.Length - 1; i++ )
					{
						 tx.NodeCreate( nodes[i] );
					}
					tx.NodeAddProperty( nodes[3], 0, Values.of( "old" ) );
					tx.NodeAddProperty( nodes[4], 0, Values.of( "old" ) );
					BatchTransactionApplier applier = new NeoStoreBatchTransactionApplier( neoStores, mock( typeof( CacheAccessBackDoor ) ), locks );
					Apply( applier, Transaction( tx ) );
			  }
			  reset( locks );

			  // These are the changes we want to assert locking on
			  TransactionRecordState tx = NewTransactionRecordState( neoStores );
			  tx.NodeCreate( nodes[0] );
			  tx.AddLabelToNode( 0, nodes[1] );
			  tx.NodeAddProperty( nodes[2], 0, Values.of( "value" ) );
			  tx.NodeChangeProperty( nodes[3], 0, Values.of( "value" ) );
			  tx.NodeRemoveProperty( nodes[4], 0 );
			  tx.NodeDelete( nodes[5] );

			  tx.NodeCreate( nodes[6] );
			  tx.AddLabelToNode( 0, nodes[6] );
			  tx.NodeAddProperty( nodes[6], 0, Values.of( "value" ) );

			  //commit( tx );
			  BatchTransactionApplier applier = new NeoStoreBatchTransactionApplier( neoStores, mock( typeof( CacheAccessBackDoor ) ), locks );
			  Apply( applier, Transaction( tx ) );

			  // then
			  // create node, NodeCommand == 1 update
			  verify( locks, times( 1 ) ).acquireNodeLock( nodes[0], Neo4Net.Kernel.impl.locking.LockService_LockType.WriteLock );
			  // add label, NodeCommand == 1 update
			  verify( locks, times( 1 ) ).acquireNodeLock( nodes[1], Neo4Net.Kernel.impl.locking.LockService_LockType.WriteLock );
			  // add property, NodeCommand and PropertyCommand == 2 updates
			  verify( locks, times( 2 ) ).acquireNodeLock( nodes[2], Neo4Net.Kernel.impl.locking.LockService_LockType.WriteLock );
			  // update property, in place, PropertyCommand == 1 update
			  verify( locks, times( 1 ) ).acquireNodeLock( nodes[3], Neo4Net.Kernel.impl.locking.LockService_LockType.WriteLock );
			  // remove property, updates the Node and the Property == 2 updates
			  verify( locks, times( 2 ) ).acquireNodeLock( nodes[4], Neo4Net.Kernel.impl.locking.LockService_LockType.WriteLock );
			  // delete node, single NodeCommand == 1 update
			  verify( locks, times( 1 ) ).acquireNodeLock( nodes[5], Neo4Net.Kernel.impl.locking.LockService_LockType.WriteLock );
			  // create and add-label goes into the NodeCommand, add property is a PropertyCommand == 2 updates
			  verify( locks, times( 2 ) ).acquireNodeLock( nodes[6], Neo4Net.Kernel.impl.locking.LockService_LockType.WriteLock );
		 }

		 private class AnswerAnonymousInnerClass : Answer<object>
		 {
			 private readonly TransactionRecordStateTest _outerInstance;

			 public AnswerAnonymousInnerClass( TransactionRecordStateTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public synchronized Object answer(final org.mockito.invocation.InvocationOnMock invocation)
			 public override object answer( InvocationOnMock invocation )
			 {
				 lock ( this )
				 {
					  // This is necessary because finalize() will also be called
					  string name = invocation.Method.Name;
					  if ( name.Equals( "acquireNodeLock" ) || name.Equals( "acquireRelationshipLock" ) )
					  {
							return mock( typeof( Lock ), invocationOnMock => null );
					  }
					  return null;
				 }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void movingBilaterallyOfTheDenseNodeThresholdIsConsistent() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MovingBilaterallyOfTheDenseNodeThresholdIsConsistent()
		 {
			  // GIVEN
			  NeoStores neoStores = NeoStoresRule.builder().with(GraphDatabaseSettings.dense_node_threshold.name(), "10").build();
			  TransactionRecordState tx = NewTransactionRecordState( neoStores );
			  long nodeId = neoStores.NodeStore.nextId();

			  tx.NodeCreate( nodeId );

			  int typeA = ( int ) neoStores.RelationshipTypeTokenStore.nextId();
			  tx.CreateRelationshipTypeToken( "A", typeA );
			  CreateRelationships( neoStores, tx, nodeId, typeA, INCOMING, 20 );

			  BatchTransactionApplier applier = new NeoStoreBatchTransactionApplier( neoStores, mock( typeof( CacheAccessBackDoor ) ), LockService.NO_LOCK_SERVICE );
			  Apply( applier, Transaction( tx ) );

			  tx = NewTransactionRecordState( neoStores );

			  int typeB = 1;
			  tx.CreateRelationshipTypeToken( "B", typeB );

			  // WHEN
			  // i remove enough relationships to become dense and remove enough to become not dense
			  long[] relationshipsOfTypeB = CreateRelationships( neoStores, tx, nodeId, typeB, OUTGOING, 5 );
			  foreach ( long relationshipToDelete in relationshipsOfTypeB )
			  {
					tx.RelDelete( relationshipToDelete );
			  }

			  PhysicalTransactionRepresentation ptx = TransactionRepresentationOf( tx );
			  Apply( applier, ptx );

			  // THEN
			  // The dynamic label record in before should be the same id as in after, and should be in use
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean foundRelationshipGroupInUse = new java.util.concurrent.atomic.AtomicBoolean();
			  AtomicBoolean foundRelationshipGroupInUse = new AtomicBoolean();

			  ptx.Accept( command => ( ( Command )command ).handle( new CommandVisitor_AdapterAnonymousInnerClass2( this, foundRelationshipGroupInUse ) ) );

			  assertTrue( "Did not create relationship group command", foundRelationshipGroupInUse.get() );
		 }

		 private class CommandVisitor_AdapterAnonymousInnerClass2 : Neo4Net.Kernel.Impl.Api.CommandVisitor_Adapter
		 {
			 private readonly TransactionRecordStateTest _outerInstance;

			 private AtomicBoolean _foundRelationshipGroupInUse;

			 public CommandVisitor_AdapterAnonymousInnerClass2( TransactionRecordStateTest outerInstance, AtomicBoolean foundRelationshipGroupInUse )
			 {
				 this.outerInstance = outerInstance;
				 this._foundRelationshipGroupInUse = foundRelationshipGroupInUse;
			 }

			 public override bool visitRelationshipGroupCommand( Command.RelationshipGroupCommand command )
			 {
				  if ( command.After.inUse() )
				  {
						if ( !_foundRelationshipGroupInUse.get() )
						{
							 _foundRelationshipGroupInUse.set( true );
						}
						else
						{
							 fail();
						}
				  }
				  return false;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConvertToDenseNodeRepresentationWhenHittingThresholdWithDifferentTypes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldConvertToDenseNodeRepresentationWhenHittingThresholdWithDifferentTypes()
		 {
			  // GIVEN a node with a total of denseNodeThreshold-1 relationships
			  NeoStores neoStores = NeoStoresRule.builder().with(GraphDatabaseSettings.dense_node_threshold.name(), "50").build();
			  TransactionRecordState tx = NewTransactionRecordState( neoStores );
			  long nodeId = neoStores.NodeStore.nextId();
			  int typeA = 0;
			  int typeB = 1;
			  int typeC = 2;
			  tx.NodeCreate( nodeId );
			  tx.CreateRelationshipTypeToken( "A", typeA );
			  CreateRelationships( neoStores, tx, nodeId, typeA, OUTGOING, 6 );
			  CreateRelationships( neoStores, tx, nodeId, typeA, INCOMING, 7 );

			  tx.CreateRelationshipTypeToken( "B", typeB );
			  CreateRelationships( neoStores, tx, nodeId, typeB, OUTGOING, 8 );
			  CreateRelationships( neoStores, tx, nodeId, typeB, INCOMING, 9 );

			  tx.CreateRelationshipTypeToken( "C", typeC );
			  CreateRelationships( neoStores, tx, nodeId, typeC, OUTGOING, 10 );
			  CreateRelationships( neoStores, tx, nodeId, typeC, INCOMING, 10 );
			  // here we're at the edge
			  assertFalse( _recordChangeSet.NodeRecords.getOrLoad( nodeId, null ).forReadingData().Dense );

			  // WHEN creating the relationship that pushes us over the threshold
			  CreateRelationships( neoStores, tx, nodeId, typeC, INCOMING, 1 );

			  // THEN the node should have been converted into a dense node
			  assertTrue( _recordChangeSet.NodeRecords.getOrLoad( nodeId, null ).forReadingData().Dense );
			  AssertDenseRelationshipCounts( _recordChangeSet, nodeId, typeA, 6, 7 );
			  AssertDenseRelationshipCounts( _recordChangeSet, nodeId, typeB, 8, 9 );
			  AssertDenseRelationshipCounts( _recordChangeSet, nodeId, typeC, 10, 11 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConvertToDenseNodeRepresentationWhenHittingThresholdWithTheSameTypeDifferentDirection() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldConvertToDenseNodeRepresentationWhenHittingThresholdWithTheSameTypeDifferentDirection()
		 {
			  // GIVEN a node with a total of denseNodeThreshold-1 relationships
			  NeoStores neoStores = NeoStoresRule.builder().with(GraphDatabaseSettings.dense_node_threshold.name(), "49").build();
			  TransactionRecordState tx = NewTransactionRecordState( neoStores );
			  long nodeId = neoStores.NodeStore.nextId();
			  int typeA = 0;
			  tx.NodeCreate( nodeId );
			  tx.CreateRelationshipTypeToken( "A", typeA );
			  CreateRelationships( neoStores, tx, nodeId, typeA, OUTGOING, 24 );
			  CreateRelationships( neoStores, tx, nodeId, typeA, INCOMING, 25 );

			  // here we're at the edge
			  assertFalse( _recordChangeSet.NodeRecords.getOrLoad( nodeId, null ).forReadingData().Dense );

			  // WHEN creating the relationship that pushes us over the threshold
			  CreateRelationships( neoStores, tx, nodeId, typeA, INCOMING, 1 );

			  // THEN the node should have been converted into a dense node
			  assertTrue( _recordChangeSet.NodeRecords.getOrLoad( nodeId, null ).forReadingData().Dense );
			  AssertDenseRelationshipCounts( _recordChangeSet, nodeId, typeA, 24, 26 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConvertToDenseNodeRepresentationWhenHittingThresholdWithTheSameTypeSameDirection() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldConvertToDenseNodeRepresentationWhenHittingThresholdWithTheSameTypeSameDirection()
		 {
			  // GIVEN a node with a total of denseNodeThreshold-1 relationships
			  NeoStores neoStores = NeoStoresRule.builder().with(GraphDatabaseSettings.dense_node_threshold.name(), "8").build();
			  TransactionRecordState tx = NewTransactionRecordState( neoStores );
			  long nodeId = neoStores.NodeStore.nextId();
			  int typeA = 0;
			  tx.NodeCreate( nodeId );
			  tx.CreateRelationshipTypeToken( "A", typeA );
			  CreateRelationships( neoStores, tx, nodeId, typeA, OUTGOING, 8 );

			  // here we're at the edge
			  assertFalse( _recordChangeSet.NodeRecords.getOrLoad( nodeId, null ).forReadingData().Dense );

			  // WHEN creating the relationship that pushes us over the threshold
			  CreateRelationships( neoStores, tx, nodeId, typeA, OUTGOING, 1 );

			  // THEN the node should have been converted into a dense node
			  assertTrue( _recordChangeSet.NodeRecords.getOrLoad( nodeId, null ).forReadingData().Dense );
			  AssertDenseRelationshipCounts( _recordChangeSet, nodeId, typeA, 9, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMaintainCorrectDataWhenDeletingFromDenseNodeWithOneType() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMaintainCorrectDataWhenDeletingFromDenseNodeWithOneType()
		 {
			  // GIVEN a node with a total of denseNodeThreshold-1 relationships
			  NeoStores neoStores = NeoStoresRule.builder().with(GraphDatabaseSettings.dense_node_threshold.name(), "13").build();
			  TransactionRecordState tx = NewTransactionRecordState( neoStores );
			  int nodeId = ( int ) neoStores.NodeStore.nextId();
			  int typeA = 0;
			  tx.NodeCreate( nodeId );
			  tx.CreateRelationshipTypeToken( "A", typeA );
			  long[] relationshipsCreated = CreateRelationships( neoStores, tx, nodeId, typeA, INCOMING, 15 );

			  //WHEN
			  tx.RelDelete( relationshipsCreated[0] );

			  // THEN the node should have been converted into a dense node
			  AssertDenseRelationshipCounts( _recordChangeSet, nodeId, typeA, 0, 14 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMaintainCorrectDataWhenDeletingFromDenseNodeWithManyTypes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldMaintainCorrectDataWhenDeletingFromDenseNodeWithManyTypes()
		 {
			  // GIVEN a node with a total of denseNodeThreshold-1 relationships
			  NeoStores neoStores = NeoStoresRule.builder().with(GraphDatabaseSettings.dense_node_threshold.name(), "1").build();
			  TransactionRecordState tx = NewTransactionRecordState( neoStores );
			  long nodeId = neoStores.NodeStore.nextId();
			  int typeA = 0;
			  int typeB = 12;
			  int typeC = 600;
			  tx.NodeCreate( nodeId );
			  tx.CreateRelationshipTypeToken( "A", typeA );
			  long[] relationshipsCreatedAIncoming = CreateRelationships( neoStores, tx, nodeId, typeA, INCOMING, 1 );
			  long[] relationshipsCreatedAOutgoing = CreateRelationships( neoStores, tx, nodeId, typeA, OUTGOING, 1 );

			  tx.CreateRelationshipTypeToken( "B", typeB );
			  long[] relationshipsCreatedBIncoming = CreateRelationships( neoStores, tx, nodeId, typeB, INCOMING, 1 );
			  long[] relationshipsCreatedBOutgoing = CreateRelationships( neoStores, tx, nodeId, typeB, OUTGOING, 1 );

			  tx.CreateRelationshipTypeToken( "C", typeC );
			  long[] relationshipsCreatedCIncoming = CreateRelationships( neoStores, tx, nodeId, typeC, INCOMING, 1 );
			  long[] relationshipsCreatedCOutgoing = CreateRelationships( neoStores, tx, nodeId, typeC, OUTGOING, 1 );

			  // WHEN
			  tx.RelDelete( relationshipsCreatedAIncoming[0] );

			  // THEN
			  AssertDenseRelationshipCounts( _recordChangeSet, nodeId, typeA, 1, 0 );
			  AssertDenseRelationshipCounts( _recordChangeSet, nodeId, typeB, 1, 1 );
			  AssertDenseRelationshipCounts( _recordChangeSet, nodeId, typeC, 1, 1 );

			  // WHEN
			  tx.RelDelete( relationshipsCreatedAOutgoing[0] );

			  // THEN
			  AssertRelationshipGroupDoesNotExist( _recordChangeSet, _recordChangeSet.NodeRecords.getOrLoad( nodeId, null ).forReadingData(), typeA );
			  AssertDenseRelationshipCounts( _recordChangeSet, nodeId, typeB, 1, 1 );
			  AssertDenseRelationshipCounts( _recordChangeSet, nodeId, typeC, 1, 1 );

			  // WHEN
			  tx.RelDelete( relationshipsCreatedBIncoming[0] );

			  // THEN
			  AssertRelationshipGroupDoesNotExist( _recordChangeSet, _recordChangeSet.NodeRecords.getOrLoad( nodeId, null ).forReadingData(), typeA );
			  AssertDenseRelationshipCounts( _recordChangeSet, nodeId, typeB, 1, 0 );
			  AssertDenseRelationshipCounts( _recordChangeSet, nodeId, typeC, 1, 1 );

			  // WHEN
			  tx.RelDelete( relationshipsCreatedBOutgoing[0] );

			  // THEN
			  AssertRelationshipGroupDoesNotExist( _recordChangeSet, _recordChangeSet.NodeRecords.getOrLoad( nodeId, null ).forReadingData(), typeA );
			  AssertRelationshipGroupDoesNotExist( _recordChangeSet, _recordChangeSet.NodeRecords.getOrLoad( nodeId, null ).forReadingData(), typeB );
			  AssertDenseRelationshipCounts( _recordChangeSet, nodeId, typeC, 1, 1 );

			  // WHEN
			  tx.RelDelete( relationshipsCreatedCIncoming[0] );

			  // THEN
			  AssertRelationshipGroupDoesNotExist( _recordChangeSet, _recordChangeSet.NodeRecords.getOrLoad( nodeId, null ).forReadingData(), typeA );
			  AssertRelationshipGroupDoesNotExist( _recordChangeSet, _recordChangeSet.NodeRecords.getOrLoad( nodeId, null ).forReadingData(), typeB );
			  AssertDenseRelationshipCounts( _recordChangeSet, nodeId, typeC, 1, 0 );

			  // WHEN
			  tx.RelDelete( relationshipsCreatedCOutgoing[0] );

			  // THEN
			  AssertRelationshipGroupDoesNotExist( _recordChangeSet, _recordChangeSet.NodeRecords.getOrLoad( nodeId, null ).forReadingData(), typeA );
			  AssertRelationshipGroupDoesNotExist( _recordChangeSet, _recordChangeSet.NodeRecords.getOrLoad( nodeId, null ).forReadingData(), typeB );
			  AssertRelationshipGroupDoesNotExist( _recordChangeSet, _recordChangeSet.NodeRecords.getOrLoad( nodeId, null ).forReadingData(), typeC );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSortRelationshipGroups() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSortRelationshipGroups()
		 {
			  // GIVEN
			  int type5 = 5;
			  int type10 = 10;
			  int type15 = 15;
			  NeoStores neoStores = NeoStoresRule.builder().with(GraphDatabaseSettings.dense_node_threshold.name(), "1").build();
			  {
					TransactionRecordState recordState = NewTransactionRecordState( neoStores );
					neoStores.RelationshipTypeTokenStore.HighId = 16;

					recordState.CreateRelationshipTypeToken( "5", type5 );
					recordState.CreateRelationshipTypeToken( "10", type10 );
					recordState.CreateRelationshipTypeToken( "15", type15 );
					BatchTransactionApplier applier = new NeoStoreBatchTransactionApplier( neoStores, mock( typeof( CacheAccessBackDoor ) ), LockService.NO_LOCK_SERVICE );
					Apply( applier, Transaction( recordState ) );
			  }

			  long nodeId = neoStores.NodeStore.nextId();
			  {
					long otherNode1Id = neoStores.NodeStore.nextId();
					long otherNode2Id = neoStores.NodeStore.nextId();
					TransactionRecordState recordState = NewTransactionRecordState( neoStores );
					recordState.NodeCreate( nodeId );
					recordState.NodeCreate( otherNode1Id );
					recordState.NodeCreate( otherNode2Id );
					recordState.RelCreate( neoStores.RelationshipStore.nextId(), type10, nodeId, otherNode1Id );
					// This relationship will cause the switch to dense
					recordState.RelCreate( neoStores.RelationshipStore.nextId(), type10, nodeId, otherNode2Id );

					BatchTransactionApplier applier = new NeoStoreBatchTransactionApplier( neoStores, mock( typeof( CacheAccessBackDoor ) ), LockService.NO_LOCK_SERVICE );
					Apply( applier, Transaction( recordState ) );

					// Just a little validation of assumptions
					AssertRelationshipGroupsInOrder( neoStores, nodeId, type10 );
			  }

			  {
			  // WHEN inserting a relationship of type 5
					TransactionRecordState recordState = NewTransactionRecordState( neoStores );
					long otherNodeId = neoStores.NodeStore.nextId();
					recordState.NodeCreate( otherNodeId );
					recordState.RelCreate( neoStores.RelationshipStore.nextId(), type5, nodeId, otherNodeId );
					BatchTransactionApplier applier = new NeoStoreBatchTransactionApplier( neoStores, mock( typeof( CacheAccessBackDoor ) ), LockService.NO_LOCK_SERVICE );
					Apply( applier, Transaction( recordState ) );

					// THEN that group should end up first in the chain
					AssertRelationshipGroupsInOrder( neoStores, nodeId, type5, type10 );
			  }

			  {
			  // WHEN inserting a relationship of type 15
					TransactionRecordState recordState = NewTransactionRecordState( neoStores );
					long otherNodeId = neoStores.NodeStore.nextId();
					recordState.NodeCreate( otherNodeId );
					recordState.RelCreate( neoStores.RelationshipStore.nextId(), type15, nodeId, otherNodeId );
					BatchTransactionApplier applier = new NeoStoreBatchTransactionApplier( neoStores, mock( typeof( CacheAccessBackDoor ) ), LockService.NO_LOCK_SERVICE );
					Apply( applier, Transaction( recordState ) );

					// THEN that group should end up last in the chain
					AssertRelationshipGroupsInOrder( neoStores, nodeId, type5, type10, type15 );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPrepareRelevantRecords() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPrepareRelevantRecords()
		 {
			  // GIVEN
			  PrepareTrackingRecordFormats format = new PrepareTrackingRecordFormats( Standard.LATEST_RECORD_FORMATS );
			  NeoStores neoStores = NeoStoresRule.builder().with(format).with(GraphDatabaseSettings.dense_node_threshold.name(), "1").build();

			  // WHEN
			  TransactionRecordState state = NewTransactionRecordState( neoStores );
			  state.NodeCreate( 0 );
			  state.RelCreate( 0, 0, 0, 0 );
			  state.RelCreate( 1, 0, 0, 0 );
			  state.RelCreate( 2, 0, 0, 0 );
			  IList<StorageCommand> commands = new List<StorageCommand>();
			  state.ExtractCommands( commands );

			  // THEN
			  int nodes = 0;
			  int rels = 0;
			  int groups = 0;
			  foreach ( StorageCommand command in commands )
			  {
					if ( command is Command.NodeCommand )
					{
						 assertTrue( format.Node().prepared(((Command.NodeCommand) command).After) );
						 nodes++;
					}
					else if ( command is Command.RelationshipCommand )
					{
						 assertTrue( format.Relationship().prepared(((Command.RelationshipCommand) command).After) );
						 rels++;
					}
					else if ( command is Command.RelationshipGroupCommand )
					{
						 assertTrue( format.RelationshipGroup().prepared(((Command.RelationshipGroupCommand) command).After) );
						 groups++;
					}
			  }
			  assertEquals( 1, nodes );
			  assertEquals( 3, rels );
			  assertEquals( 1, groups );
		 }

		 private void AddLabelsToNode( TransactionRecordState recordState, long nodeId, long[] labelIds )
		 {
			  foreach ( long labelId in labelIds )
			  {
					recordState.AddLabelToNode( ( int )labelId, nodeId );
			  }
		 }

		 private void RemoveLabelsFromNode( TransactionRecordState recordState, long nodeId, long[] labelIds )
		 {
			  foreach ( long labelId in labelIds )
			  {
					recordState.RemoveLabelFromNode( ( int )labelId, nodeId );
			  }
		 }

		 private long[] CreateRelationships( NeoStores neoStores, TransactionRecordState tx, long nodeId, int type, Direction direction, int count )
		 {
			  long[] result = new long[count];
			  for ( int i = 0; i < count; i++ )
			  {
					long otherNodeId = neoStores.NodeStore.nextId();
					tx.NodeCreate( otherNodeId );
					long first = direction == OUTGOING ? nodeId : otherNodeId;
					long other = direction == INCOMING ? nodeId : otherNodeId;
					long relId = neoStores.RelationshipStore.nextId();
					result[i] = relId;
					tx.RelCreate( relId, type, first, other );
			  }
			  return result;
		 }

		 private void AssertRelationshipGroupsInOrder( NeoStores neoStores, long nodeId, params int[] types )
		 {
			  NodeStore nodeStore = neoStores.NodeStore;
			  NodeRecord node = nodeStore.GetRecord( nodeId, nodeStore.NewRecord(), NORMAL );
			  assertTrue( "Node should be dense, is " + node, node.Dense );
			  long groupId = node.NextRel;
			  int cursor = 0;
			  IList<RelationshipGroupRecord> seen = new List<RelationshipGroupRecord>();
			  while ( groupId != Record.NO_NEXT_RELATIONSHIP.intValue() )
			  {
					RecordStore<RelationshipGroupRecord> relationshipGroupStore = neoStores.RelationshipGroupStore;
					RelationshipGroupRecord group = relationshipGroupStore.GetRecord( groupId, relationshipGroupStore.NewRecord(), NORMAL );
					seen.Add( group );
					assertEquals( "Invalid type, seen groups so far " + seen, types[cursor++], group.Type );
					groupId = group.Next;
			  }
			  assertEquals( "Not enough relationship group records found in chain for " + node, types.Length, cursor );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Iterable<org.neo4j.kernel.impl.api.index.EntityUpdates> indexUpdatesOf(org.neo4j.kernel.impl.store.NeoStores neoStores, TransactionRecordState state) throws java.io.IOException, org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 private IEnumerable<EntityUpdates> IndexUpdatesOf( NeoStores neoStores, TransactionRecordState state )
		 {
			  return IndexUpdatesOf( neoStores, TransactionRepresentationOf( state ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Iterable<org.neo4j.kernel.impl.api.index.EntityUpdates> indexUpdatesOf(org.neo4j.kernel.impl.store.NeoStores neoStores, org.neo4j.kernel.impl.transaction.TransactionRepresentation transaction) throws java.io.IOException
		 private IEnumerable<EntityUpdates> IndexUpdatesOf( NeoStores neoStores, TransactionRepresentation transaction )
		 {
			  PropertyCommandsExtractor extractor = new PropertyCommandsExtractor();
			  transaction.Accept( extractor );

			  CollectingIndexingUpdateService indexingUpdateService = new CollectingIndexingUpdateService( this );
			  OnlineIndexUpdates onlineIndexUpdates = new OnlineIndexUpdates( neoStores.NodeStore, neoStores.RelationshipStore, indexingUpdateService, new PropertyPhysicalToLogicalConverter( neoStores.PropertyStore ) );
			  onlineIndexUpdates.Feed( extractor.NodeCommands, extractor.RelationshipCommands );
			  return indexingUpdateService.EntityUpdatesList;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.impl.transaction.log.PhysicalTransactionRepresentation transactionRepresentationOf(TransactionRecordState writeTransaction) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 private PhysicalTransactionRepresentation TransactionRepresentationOf( TransactionRecordState writeTransaction )
		 {
			  IList<StorageCommand> commands = new List<StorageCommand>();
			  writeTransaction.ExtractCommands( commands );
			  PhysicalTransactionRepresentation tx = new PhysicalTransactionRepresentation( commands );
			  tx.SetHeader( new sbyte[0], 0, 0, 0, 0, 0, 0 );
			  return tx;
		 }

		 private void AssertCommand( StorageCommand next, Type klass )
		 {
			  assertTrue( "Expected " + klass + ". was: " + next, klass.IsInstanceOfType( next ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.impl.transaction.CommittedTransactionRepresentation readFromChannel(org.neo4j.kernel.impl.transaction.log.ReadableLogChannel channel) throws java.io.IOException
		 private CommittedTransactionRepresentation ReadFromChannel( ReadableLogChannel channel )
		 {
			  LogEntryReader<ReadableLogChannel> logEntryReader = new VersionAwareLogEntryReader<ReadableLogChannel>();
			  using ( PhysicalTransactionCursor<ReadableLogChannel> cursor = new PhysicalTransactionCursor<ReadableLogChannel>( channel, logEntryReader ) )
			  {
					assertTrue( cursor.Next() );
					return cursor.Get();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeToChannel(org.neo4j.kernel.impl.transaction.TransactionRepresentation transaction, org.neo4j.kernel.impl.transaction.log.FlushableChannel channel) throws java.io.IOException
		 private void WriteToChannel( TransactionRepresentation transaction, FlushableChannel channel )
		 {
			  TransactionLogWriter writer = new TransactionLogWriter( new LogEntryWriter( channel ) );
			  writer.Append( transaction, 2 );
		 }

		 private TransactionRecordState NodeWithDynamicLabelRecord( NeoStores store, AtomicLong nodeId, AtomicLong dynamicLabelRecordId )
		 {
			  TransactionRecordState recordState = NewTransactionRecordState( store );

			  nodeId.set( store.NodeStore.nextId() );
			  int[] labelIds = new int[20];
			  for ( int i = 0; i < labelIds.Length; i++ )
			  {
					int labelId = ( int ) store.LabelTokenStore.nextId();
					recordState.CreateLabelToken( "Label" + i, labelId );
					labelIds[i] = labelId;
			  }
			  recordState.NodeCreate( nodeId.get() );
			  foreach ( int labelId in labelIds )
			  {
					recordState.AddLabelToNode( labelId, nodeId.get() );
			  }

			  // Extract the dynamic label record id (which is also a verification that we allocated one)
			  NodeRecord node = Iterables.single( _recordChangeSet.NodeRecords.changes() ).forReadingData();
			  dynamicLabelRecordId.set( Iterables.single( node.DynamicLabelRecords ).Id );

			  return recordState;
		 }

		 private TransactionRecordState DeleteNode( NeoStores store, long nodeId )
		 {
			  TransactionRecordState recordState = NewTransactionRecordState( store );
			  recordState.NodeDelete( nodeId );
			  return recordState;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void apply(org.neo4j.kernel.impl.api.BatchTransactionApplier applier, org.neo4j.kernel.impl.transaction.TransactionRepresentation transaction) throws Exception
		 private void Apply( BatchTransactionApplier applier, TransactionRepresentation transaction )
		 {
			  CommandHandlerContract.apply( applier, new TransactionToApply( transaction ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void apply(org.neo4j.kernel.impl.store.NeoStores neoStores, org.neo4j.kernel.impl.transaction.TransactionRepresentation transaction) throws Exception
		 private void Apply( NeoStores neoStores, TransactionRepresentation transaction )
		 {
			  BatchTransactionApplier applier = new NeoStoreBatchTransactionApplier( neoStores, mock( typeof( CacheAccessBackDoor ) ), LockService.NO_LOCK_SERVICE );
			  Apply( applier, transaction );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void apply(org.neo4j.kernel.impl.store.NeoStores neoStores, TransactionRecordState state) throws Exception
		 private void Apply( NeoStores neoStores, TransactionRecordState state )
		 {
			  BatchTransactionApplier applier = new NeoStoreBatchTransactionApplier( neoStores, mock( typeof( CacheAccessBackDoor ) ), LockService.NO_LOCK_SERVICE );
			  Apply( applier, TransactionRepresentationOf( state ) );
		 }

		 private TransactionRecordState NewTransactionRecordState( NeoStores neoStores )
		 {
			  Loaders loaders = new Loaders( neoStores );
			  _recordChangeSet = new RecordChangeSet( loaders );
			  PropertyTraverser propertyTraverser = new PropertyTraverser();
			  RelationshipGroupGetter relationshipGroupGetter = new RelationshipGroupGetter( neoStores.RelationshipGroupStore );
			  PropertyDeleter propertyDeleter = new PropertyDeleter( propertyTraverser );
			  return new TransactionRecordState( neoStores, _integrityValidator, _recordChangeSet, 0, new NoOpClient(), new RelationshipCreator(relationshipGroupGetter, neoStores.RelationshipGroupStore.StoreHeaderInt), new RelationshipDeleter(relationshipGroupGetter, propertyDeleter), new PropertyCreator(neoStores.PropertyStore, propertyTraverser), propertyDeleter );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.impl.transaction.TransactionRepresentation transaction(TransactionRecordState recordState) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException
		 private TransactionRepresentation Transaction( TransactionRecordState recordState )
		 {
			  IList<StorageCommand> commands = new List<StorageCommand>();
			  recordState.ExtractCommands( commands );
			  PhysicalTransactionRepresentation transaction = new PhysicalTransactionRepresentation( commands );
			  transaction.SetHeader( new sbyte[0], 0, 0, 0, 0, 0, 0 );
			  return transaction;
		 }

		 private void AssertDynamicLabelRecordInUse( NeoStores store, long id, bool inUse )
		 {
			  DynamicArrayStore dynamicLabelStore = store.NodeStore.DynamicLabelStore;
			  DynamicRecord record = dynamicLabelStore.GetRecord( id, dynamicLabelStore.NextRecord(), FORCE );
			  assertEquals( inUse, record.InUse() );
		 }

		 private Value String( int length )
		 {
			  StringBuilder result = new StringBuilder();
			  char ch = 'a';
			  for ( int i = 0; i < length; i++ )
			  {
					result.Append( ( char )( ( ch + ( i % 10 ) ) ) );
			  }
			  return Values.of( result.ToString() );
		 }

		 private Command.PropertyCommand SinglePropertyCommand( ICollection<StorageCommand> commands )
		 {
			  return ( Command.PropertyCommand ) Iterables.single( filter( t => t is Command.PropertyCommand, commands ) );
		 }

		 private Command.RelationshipGroupCommand SingleRelationshipGroupCommand( ICollection<StorageCommand> commands )
		 {
			  return ( Command.RelationshipGroupCommand ) Iterables.single( filter( t => t is Command.RelationshipGroupCommand, commands ) );
		 }

		 public virtual LongIterable EntityIds( EntityCommandGrouper.Cursor cursor )
		 {
			  LongArrayList list = new LongArrayList();
			  if ( cursor.NextEntity() )
			  {
					while ( cursor.NextProperty() != null )
					{
						 // Just get any potential property commands out of the way
					}
					list.add( cursor.CurrentEntityId() );
			  }
			  return list;
		 }

		 private class CollectingIndexingUpdateService : IndexingUpdateService
		 {
			 private readonly TransactionRecordStateTest _outerInstance;

			 public CollectingIndexingUpdateService( TransactionRecordStateTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal readonly IList<EntityUpdates> EntityUpdatesList = new List<EntityUpdates>();

			  public override void Apply( IndexUpdates updates )
			  {
			  }

			  public override IEnumerable<IndexEntryUpdate<SchemaDescriptor>> ConvertToIndexUpdates( EntityUpdates entityUpdates, EntityType type )
			  {
					EntityUpdatesList.Add( entityUpdates );
					return Iterables.empty();
			  }
		 }
	}

}