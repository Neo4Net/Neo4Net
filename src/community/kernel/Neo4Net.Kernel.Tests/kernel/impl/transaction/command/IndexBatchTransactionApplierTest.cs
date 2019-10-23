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
	using Test = org.junit.Test;


	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using IndexProviderDescriptor = Neo4Net.Kernel.Api.Internal.schema.IndexProviderDescriptor;
	using LabelScanWriter = Neo4Net.Kernel.api.labelscan.LabelScanWriter;
	using NodeLabelUpdate = Neo4Net.Kernel.api.labelscan.NodeLabelUpdate;
	using TransactionApplier = Neo4Net.Kernel.Impl.Api.TransactionApplier;
	using TransactionToApply = Neo4Net.Kernel.Impl.Api.TransactionToApply;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using IndexingUpdateService = Neo4Net.Kernel.Impl.Api.index.IndexingUpdateService;
	using NodeLabelsField = Neo4Net.Kernel.impl.store.NodeLabelsField;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using PropertyStore = Neo4Net.Kernel.impl.store.PropertyStore;
	using RelationshipStore = Neo4Net.Kernel.impl.store.RelationshipStore;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using NodeCommand = Neo4Net.Kernel.impl.transaction.command.Command.NodeCommand;
	using EntityType = Neo4Net.Kernel.Api.StorageEngine.EntityType;
	using SchemaRule = Neo4Net.Kernel.Api.StorageEngine.schema.SchemaRule;
	using StoreIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor;
	using Neo4Net.Utils.Concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.schema.SchemaDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.record.Record.NO_NEXT_PROPERTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.record.Record.NO_NEXT_RELATIONSHIP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptorFactory.uniqueForSchema;

	public class IndexBatchTransactionApplierTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProvideLabelScanStoreUpdatesSortedByNodeId() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProvideLabelScanStoreUpdatesSortedByNodeId()
		 {
			  // GIVEN
			  IndexingService indexing = mock( typeof( IndexingService ) );
			  when( indexing.ConvertToIndexUpdates( any(), eq(EntityType.NODE) ) ).thenAnswer(o => Iterables.empty());
			  LabelScanWriter writer = new OrderVerifyingLabelScanWriter( 10, 15, 20 );
			  WorkSync<System.Func<LabelScanWriter>, LabelUpdateWork> labelScanSync = spy( new WorkSync<System.Func<LabelScanWriter>, LabelUpdateWork>( SingletonProvider( writer ) ) );
			  WorkSync<IndexingUpdateService, IndexUpdatesWork> indexUpdatesSync = new WorkSync<IndexingUpdateService, IndexUpdatesWork>( indexing );
			  TransactionToApply tx = mock( typeof( TransactionToApply ) );
			  PropertyStore propertyStore = mock( typeof( PropertyStore ) );
			  using ( IndexBatchTransactionApplier applier = new IndexBatchTransactionApplier( indexing, labelScanSync, indexUpdatesSync, mock( typeof( NodeStore ) ), mock( typeof( RelationshipStore ) ), propertyStore, new IndexActivator( indexing ) ) )
			  {
					using ( TransactionApplier txApplier = applier.StartTx( tx ) )
					{
						 // WHEN
						 txApplier.VisitNodeCommand( Node( 15 ) );
						 txApplier.VisitNodeCommand( Node( 20 ) );
						 txApplier.VisitNodeCommand( Node( 10 ) );
					}
			  }
			  // THEN all assertions happen inside the LabelScanWriter#write and #close
			  verify( labelScanSync ).applyAsync( any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRegisterIndexesToActivateIntoTheActivator() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRegisterIndexesToActivateIntoTheActivator()
		 {
			  // given
			  IndexingService indexing = mock( typeof( IndexingService ) );
			  LabelScanWriter writer = new OrderVerifyingLabelScanWriter( 10, 15, 20 );
			  WorkSync<System.Func<LabelScanWriter>, LabelUpdateWork> labelScanSync = spy( new WorkSync<System.Func<LabelScanWriter>, LabelUpdateWork>( SingletonProvider( writer ) ) );
			  WorkSync<IndexingUpdateService, IndexUpdatesWork> indexUpdatesSync = new WorkSync<IndexingUpdateService, IndexUpdatesWork>( indexing );
			  PropertyStore propertyStore = mock( typeof( PropertyStore ) );
			  TransactionToApply tx = mock( typeof( TransactionToApply ) );
			  IndexActivator indexActivator = new IndexActivator( indexing );
			  long indexId1 = 1;
			  long indexId2 = 2;
			  long indexId3 = 3;
			  long constraintId1 = 10;
			  long constraintId2 = 11;
			  long constraintId3 = 12;
			  IndexProviderDescriptor providerDescriptor = new IndexProviderDescriptor( "index-key", "v1" );
			  StoreIndexDescriptor rule1 = uniqueForSchema( forLabel( 1, 1 ), providerDescriptor ).withIds( indexId1, constraintId1 );
			  StoreIndexDescriptor rule2 = uniqueForSchema( forLabel( 2, 1 ), providerDescriptor ).withIds( indexId2, constraintId2 );
			  StoreIndexDescriptor rule3 = uniqueForSchema( forLabel( 3, 1 ), providerDescriptor ).withIds( indexId3, constraintId3 );
			  using ( IndexBatchTransactionApplier applier = new IndexBatchTransactionApplier( indexing, labelScanSync, indexUpdatesSync, mock( typeof( NodeStore ) ), mock( typeof( RelationshipStore ) ), propertyStore, indexActivator ) )
			  {
					using ( TransactionApplier txApplier = applier.StartTx( tx ) )
					{
						 // WHEN
						 // activate index 1
						 txApplier.VisitSchemaRuleCommand( new Command.SchemaRuleCommand( Collections.emptyList(), AsRecords(rule1, true), rule1 ) );

						 // activate index 2
						 txApplier.VisitSchemaRuleCommand( new Command.SchemaRuleCommand( Collections.emptyList(), AsRecords(rule2, true), rule2 ) );

						 // activate index 3
						 txApplier.VisitSchemaRuleCommand( new Command.SchemaRuleCommand( Collections.emptyList(), AsRecords(rule3, true), rule3 ) );

						 // drop index 2
						 txApplier.VisitSchemaRuleCommand( new Command.SchemaRuleCommand( AsRecords( rule2, true ), AsRecords( rule2, false ), rule2 ) );
					}
			  }

			  verify( indexing ).dropIndex( rule2 );
			  indexActivator.Close();
			  verify( indexing ).activateIndex( indexId1 );
			  verify( indexing ).activateIndex( indexId3 );
			  verifyNoMoreInteractions( indexing );
		 }

		 private ICollection<DynamicRecord> AsRecords( SchemaRule rule, bool inUse )
		 {
			  // Only used to transfer
			  IList<DynamicRecord> records = new List<DynamicRecord>();
			  DynamicRecord dynamicRecord = new DynamicRecord( rule.Id );
			  dynamicRecord.InUse = inUse;
			  records.Add( dynamicRecord );
			  return records;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private System.Func<org.Neo4Net.kernel.api.labelscan.LabelScanWriter> singletonProvider(final org.Neo4Net.kernel.api.labelscan.LabelScanWriter writer)
		 private System.Func<LabelScanWriter> SingletonProvider( LabelScanWriter writer )
		 {
			  return () => writer;
		 }

		 private NodeCommand Node( long nodeId )
		 {
			  NodeRecord after = new NodeRecord( nodeId, true, false, NO_NEXT_RELATIONSHIP.intValue(),NO_NEXT_PROPERTY.intValue(), 0 );
			  NodeLabelsField.parseLabelsField( after ).add( 1, null, null );

			  return new NodeCommand( new NodeRecord( nodeId ), after );
		 }

		 private class OrderVerifyingLabelScanWriter : LabelScanWriter
		 {
			  internal readonly long[] ExpectedNodeIds;
			  internal int Cursor;

			  internal OrderVerifyingLabelScanWriter( params long[] expectedNodeIds )
			  {
					this.ExpectedNodeIds = expectedNodeIds;
			  }

			  public override void Write( NodeLabelUpdate update )
			  {
					assertEquals( ExpectedNodeIds[Cursor], update.NodeId );
					Cursor++;
			  }

			  public override void Close()
			  {
					assertEquals( Cursor, ExpectedNodeIds.Length );
			  }
		 }
	}

}