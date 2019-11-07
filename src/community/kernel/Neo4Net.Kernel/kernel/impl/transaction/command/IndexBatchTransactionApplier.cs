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

	using LabelScanWriter = Neo4Net.Kernel.Api.LabelScan.LabelScanWriter;
	using NodeLabelUpdate = Neo4Net.Kernel.Api.LabelScan.NodeLabelUpdate;
	using BatchTransactionApplier = Neo4Net.Kernel.Impl.Api.BatchTransactionApplier;
	using TransactionApplier = Neo4Net.Kernel.Impl.Api.TransactionApplier;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using IndexingUpdateService = Neo4Net.Kernel.Impl.Api.index.IndexingUpdateService;
	using PropertyCommandsExtractor = Neo4Net.Kernel.Impl.Api.index.PropertyCommandsExtractor;
	using PropertyPhysicalToLogicalConverter = Neo4Net.Kernel.Impl.Api.index.PropertyPhysicalToLogicalConverter;
	using NodeLabels = Neo4Net.Kernel.impl.store.NodeLabels;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using PropertyStore = Neo4Net.Kernel.impl.store.PropertyStore;
	using RelationshipStore = Neo4Net.Kernel.impl.store.RelationshipStore;
	using ConstraintRule = Neo4Net.Kernel.Impl.Store.Records.ConstraintRule;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PropertyCommand = Neo4Net.Kernel.impl.transaction.command.Command.PropertyCommand;
	using IndexUpdates = Neo4Net.Kernel.impl.transaction.state.IndexUpdates;
	using OnlineIndexUpdates = Neo4Net.Kernel.impl.transaction.state.OnlineIndexUpdates;
	using CommandsToApply = Neo4Net.Kernel.Api.StorageEngine.CommandsToApply;
	using SchemaRule = Neo4Net.Kernel.Api.StorageEngine.schema.SchemaRule;
	using StoreIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor;
	using AsyncApply = Neo4Net.Utils.Concurrent.AsyncApply;
	using Neo4Net.Utils.Concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.NodeLabelsField.parseLabelsField;

	/// <summary>
	/// Gather node and property changes, converting them into logical updates to the indexes. <seealso cref="close()"/> will actually
	/// apply the indexes.
	/// </summary>
	public class IndexBatchTransactionApplier : Neo4Net.Kernel.Impl.Api.BatchTransactionApplier_Adapter
	{
		 private readonly IndexingService _indexingService;
		 private readonly WorkSync<System.Func<LabelScanWriter>, LabelUpdateWork> _labelScanStoreSync;
		 private readonly WorkSync<IndexingUpdateService, IndexUpdatesWork> _indexUpdatesSync;
		 private readonly SingleTransactionApplier _transactionApplier;
		 private readonly IndexActivator _indexActivator;
		 private readonly PropertyStore _propertyStore;

		 private IList<NodeLabelUpdate> _labelUpdates;
		 private IndexUpdates _indexUpdates;
		 private long _txId;

		 public IndexBatchTransactionApplier( IndexingService indexingService, WorkSync<System.Func<LabelScanWriter>, LabelUpdateWork> labelScanStoreSync, WorkSync<IndexingUpdateService, IndexUpdatesWork> indexUpdatesSync, NodeStore nodeStore, RelationshipStore relationshipStore, PropertyStore propertyStore, IndexActivator indexActivator )
		 {
			  this._indexingService = indexingService;
			  this._labelScanStoreSync = labelScanStoreSync;
			  this._indexUpdatesSync = indexUpdatesSync;
			  this._propertyStore = propertyStore;
			  this._transactionApplier = new SingleTransactionApplier( this, nodeStore, relationshipStore );
			  this._indexActivator = indexActivator;
		 }

		 public override TransactionApplier StartTx( CommandsToApply transaction )
		 {
			  _txId = transaction.TransactionId();
			  return _transactionApplier;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void applyPendingLabelAndIndexUpdates() throws java.io.IOException
		 private void ApplyPendingLabelAndIndexUpdates()
		 {
			  AsyncApply labelUpdatesApply = null;
			  if ( _labelUpdates != null )
			  {
					// Updates are sorted according to node id here, an artifact of node commands being sorted
					// by node id when extracting from TransactionRecordState.
					labelUpdatesApply = _labelScanStoreSync.applyAsync( new LabelUpdateWork( _labelUpdates ) );
					_labelUpdates = null;
			  }
			  if ( _indexUpdates != null && _indexUpdates.hasUpdates() )
			  {
					try
					{
						 _indexUpdatesSync.apply( new IndexUpdatesWork( _indexUpdates ) );
					}
					catch ( ExecutionException e )
					{
						 throw new IOException( "Failed to flush index updates", e );
					}
					_indexUpdates = null;
			  }

			  if ( labelUpdatesApply != null )
			  {
					try
					{
						 labelUpdatesApply.Await();
					}
					catch ( ExecutionException e )
					{
						 throw new IOException( "Failed to flush label updates", e );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
		 public override void Close()
		 {
			  ApplyPendingLabelAndIndexUpdates();
		 }

		 /// <summary>
		 /// Made as an internal non-static class here since the batch applier has so much interaction with
		 /// the transaction applier such that keeping them apart would incur too much data structures and interfaces
		 /// purely for communicating between the two to make the code hard to read.
		 /// </summary>
		 private class SingleTransactionApplier : Neo4Net.Kernel.Impl.Api.TransactionApplier_Adapter
		 {
			 private readonly IndexBatchTransactionApplier _outerInstance;

			  internal readonly NodeStore NodeStore;
			  internal RelationshipStore RelationshipStore;
			  internal readonly PropertyCommandsExtractor IndexUpdatesExtractor = new PropertyCommandsExtractor();
			  internal IList<StoreIndexDescriptor> CreatedIndexes;

			  internal SingleTransactionApplier( IndexBatchTransactionApplier outerInstance, NodeStore nodeStore, RelationshipStore relationshipStore )
			  {
				  this._outerInstance = outerInstance;
					this.NodeStore = nodeStore;
					this.RelationshipStore = relationshipStore;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
			  public override void Close()
			  {
					if ( IndexUpdatesExtractor.containsAnyEntityOrPropertyUpdate() )
					{
						 // Queue the index updates. When index updates from all transactions in this batch have been accumulated
						 // we'll feed them to the index updates work sync at the end of the batch
						 IndexUpdates().feed(IndexUpdatesExtractor.NodeCommands, IndexUpdatesExtractor.RelationshipCommands);
						 IndexUpdatesExtractor.close();
					}

					// Created pending indexes
					if ( CreatedIndexes != null )
					{
						 outerInstance.indexingService.createIndexes( CreatedIndexes.ToArray() );
						 CreatedIndexes = null;
					}
			  }

			  internal virtual IndexUpdates IndexUpdates()
			  {
					if ( outerInstance.indexUpdates == null )
					{
						 outerInstance.indexUpdates = new OnlineIndexUpdates( NodeStore, RelationshipStore, outerInstance.indexingService, new PropertyPhysicalToLogicalConverter( outerInstance.propertyStore ) );
					}
					return outerInstance.indexUpdates;
			  }

			  public override bool VisitNodeCommand( Command.NodeCommand command )
			  {
					// for label store updates
					NodeRecord before = command.Before;
					NodeRecord after = command.After;

					NodeLabels labelFieldBefore = parseLabelsField( before );
					NodeLabels labelFieldAfter = parseLabelsField( after );
					if ( !( labelFieldBefore.Inlined && labelFieldAfter.Inlined && before.LabelField == after.LabelField ) )
					{
						 long[] labelsBefore = labelFieldBefore.IfLoaded;
						 long[] labelsAfter = labelFieldAfter.IfLoaded;
						 if ( labelsBefore != null && labelsAfter != null )
						 {
							  if ( outerInstance.labelUpdates == null )
							  {
									outerInstance.labelUpdates = new List<NodeLabelUpdate>();
							  }
							  outerInstance.labelUpdates.Add( NodeLabelUpdate.labelChanges( command.Key, labelsBefore, labelsAfter, outerInstance.txId ) );
						 }
					}

					// for indexes
					return IndexUpdatesExtractor.visitNodeCommand( command );
			  }

			  public override bool VisitRelationshipCommand( Command.RelationshipCommand command )
			  {
					return IndexUpdatesExtractor.visitRelationshipCommand( command );
			  }

			  public override bool VisitPropertyCommand( PropertyCommand command )
			  {
					return IndexUpdatesExtractor.visitPropertyCommand( command );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visitSchemaRuleCommand(Command.SchemaRuleCommand command) throws java.io.IOException
			  public override bool VisitSchemaRuleCommand( Command.SchemaRuleCommand command )
			  {
					SchemaRule schemaRule = command.SchemaRule;
					if ( command.SchemaRule is StoreIndexDescriptor )
					{
						 StoreIndexDescriptor indexRule = ( StoreIndexDescriptor ) schemaRule;
						 // Why apply index updates here? Here's the thing... this is a batch applier, which means that
						 // index updates are gathered throughout the batch and applied in the end of the batch.
						 // Assume there are some transactions creating or modifying nodes that may not be covered
						 // by an existing index, but a later transaction in the same batch creates such an index.
						 // In that scenario the index would be created, populated and then fed the [this time duplicate]
						 // update for the node created before the index. The most straight forward solution is to
						 // apply pending index updates up to this point in this batch before index schema changes occur.
						 outerInstance.applyPendingLabelAndIndexUpdates();

						 switch ( command.Mode )
						 {
						 case UPDATE:
							  // Shouldn't we be more clear about that we are waiting for an index to come online here?
							  // right now we just assume that an update to index records means wait for it to be online.
							  if ( indexRule.CanSupportUniqueConstraint() )
							  {
									// Register activations into the IndexActivator instead of IndexingService to avoid deadlock
									// that could insue for applying batches of transactions where a previous transaction in the same
									// batch acquires a low-level commit lock that prevents the very same index population to complete.
									outerInstance.indexActivator.ActivateIndex( schemaRule.Id );
							  }
							  break;
						 case CREATE:
							  // Add to list so that all these indexes will be created in one call later
							  CreatedIndexes = CreatedIndexes == null ? new List<StoreIndexDescriptor>() : CreatedIndexes;
							  CreatedIndexes.Add( indexRule );
							  break;
						 case DELETE:
							  outerInstance.indexingService.DropIndex( indexRule );
							  outerInstance.indexActivator.IndexDropped( schemaRule.Id );
							  break;
						 default:
							  throw new System.InvalidOperationException( command.Mode.name() );
						 }
					}
					// Keep IndexingService updated on constraint changes
					else if ( schemaRule is ConstraintRule )
					{
						 ConstraintRule constraintRule = ( ConstraintRule ) schemaRule;
						 switch ( command.Mode )
						 {
						 case CREATE:
						 case UPDATE:
							  outerInstance.indexingService.PutConstraint( constraintRule );
							  break;
						 case DELETE:
							  outerInstance.indexingService.RemoveConstraint( constraintRule.Id );
							  break;
						 default:
							  throw new System.InvalidOperationException( command.Mode.name() );
						 }
					}
					return false;
			  }
		 }
	}

}