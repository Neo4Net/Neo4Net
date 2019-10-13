using System;

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
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Answers = org.mockito.Answers;
	using Mock = org.mockito.Mock;
	using Mockito = org.mockito.Mockito;
	using MockitoJUnitRunner = org.mockito.junit.MockitoJUnitRunner;


	using Neo4Net.Helpers.Collections;
	using LabelSchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using IndexPopulationFailedKernelException = Neo4Net.Kernel.Api.Exceptions.index.IndexPopulationFailedKernelException;
	using Neo4Net.Kernel.Api.Index;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using NodeLabelUpdate = Neo4Net.Kernel.api.labelscan.NodeLabelUpdate;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using LockService = Neo4Net.Kernel.impl.locking.LockService;
	using RecordStorageReader = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageReader;
	using InlineNodeLabels = Neo4Net.Kernel.impl.store.InlineNodeLabels;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using NeoStoreIndexStoreView = Neo4Net.Kernel.impl.transaction.state.storeview.NeoStoreIndexStoreView;
	using Neo4Net.Kernel.impl.transaction.state.storeview;
	using Neo4Net.Kernel.impl.util;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using EntityType = Neo4Net.Storageengine.Api.EntityType;
	using StorageNodeCursor = Neo4Net.Storageengine.Api.StorageNodeCursor;
	using StorageReader = Neo4Net.Storageengine.Api.StorageReader;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(MockitoJUnitRunner.class) public class MultipleIndexPopulatorUpdatesTest
	public class MultipleIndexPopulatorUpdatesTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Mock(answer = org.mockito.Answers.RETURNS_MOCKS) private org.neo4j.logging.LogProvider logProvider;
		 private LogProvider _logProvider;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void updateForHigherNodeIgnoredWhenUsingFullNodeStoreScan() throws org.neo4j.kernel.api.exceptions.index.IndexPopulationFailedKernelException, java.io.IOException, org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void UpdateForHigherNodeIgnoredWhenUsingFullNodeStoreScan()
		 {
			  NeoStores neoStores = Mockito.mock( typeof( NeoStores ) );
			  NodeStore nodeStore = mock( typeof( NodeStore ) );

			  when( neoStores.NodeStore ).thenReturn( nodeStore );

			  ProcessListenableNeoStoreIndexView storeView = new ProcessListenableNeoStoreIndexView( this, LockService.NO_LOCK_SERVICE, neoStores );
			  MultipleIndexPopulator indexPopulator = new MultipleIndexPopulator( storeView, _logProvider, EntityType.NODE, mock( typeof( SchemaState ) ) );

			  storeView.ProcessListener = new NodeUpdateProcessListener( indexPopulator );

			  IndexPopulator populator = CreateIndexPopulator();
			  IndexUpdater indexUpdater = mock( typeof( IndexUpdater ) );

			  AddPopulator( indexPopulator, populator, 1, TestIndexDescriptorFactory.forLabel( 1, 1 ) );

			  indexPopulator.Create();
			  StoreScan<IndexPopulationFailedKernelException> storeScan = indexPopulator.IndexAllEntities();
			  storeScan.Run();

			  Mockito.verify( indexUpdater, never() ).process(any(typeof(IndexEntryUpdate)));
		 }

		 private NodeRecord NodeRecord
		 {
			 get
			 {
				  NodeRecord nodeRecord = new NodeRecord( 1L );
				  nodeRecord.Initialize( true, 0, false, 1, 0x0000000001L );
				  InlineNodeLabels.putSorted( nodeRecord, new long[]{ 1 }, null, null );
				  return nodeRecord;
			 }
		 }

		 private IndexPopulator CreateIndexPopulator()
		 {
			  return mock( typeof( IndexPopulator ) );
		 }

		 private MultipleIndexPopulator.IndexPopulation AddPopulator( MultipleIndexPopulator multipleIndexPopulator, IndexPopulator indexPopulator, long indexId, IndexDescriptor descriptor )
		 {
			  return AddPopulator( multipleIndexPopulator, descriptor.WithId( indexId ), indexPopulator, mock( typeof( FlippableIndexProxy ) ), mock( typeof( FailedIndexProxyFactory ) ) );
		 }

		 private MultipleIndexPopulator.IndexPopulation AddPopulator( MultipleIndexPopulator multipleIndexPopulator, StoreIndexDescriptor descriptor, IndexPopulator indexPopulator, FlippableIndexProxy flippableIndexProxy, FailedIndexProxyFactory failedIndexProxyFactory )
		 {
			  return multipleIndexPopulator.AddPopulator( indexPopulator, descriptor.WithoutCapabilities(), flippableIndexProxy, failedIndexProxyFactory, "userIndexDescription" );
		 }

		 private class NodeUpdateProcessListener : Listener<StorageNodeCursor>
		 {
			  internal readonly MultipleIndexPopulator IndexPopulator;
			  internal readonly LabelSchemaDescriptor Index;

			  internal NodeUpdateProcessListener( MultipleIndexPopulator indexPopulator )
			  {
					this.IndexPopulator = indexPopulator;
					this.Index = SchemaDescriptorFactory.forLabel( 1, 1 );
			  }

			  public override void Receive( StorageNodeCursor node )
			  {
					if ( node.EntityReference() == 7 )
					{
						 IndexPopulator.queueUpdate( IndexEntryUpdate.change( 8L, Index, Values.of( "a" ), Values.of( "b" ) ) );
					}
			  }
		 }

		 private class ProcessListenableNeoStoreIndexView : NeoStoreIndexStoreView
		 {
			 private readonly MultipleIndexPopulatorUpdatesTest _outerInstance;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal Listener<StorageNodeCursor> ProcessListenerConflict;
			  internal NeoStores NeoStores;

			  internal ProcessListenableNeoStoreIndexView( MultipleIndexPopulatorUpdatesTest outerInstance, LockService locks, NeoStores neoStores ) : base( locks, neoStores )
			  {
				  this._outerInstance = outerInstance;
					this.NeoStores = neoStores;
			  }

			  public override StoreScan<FAILURE> VisitNodes<FAILURE>( int[] labelIds, System.Func<int, bool> propertyKeyIdFilter, Visitor<EntityUpdates, FAILURE> propertyUpdatesVisitor, Visitor<NodeLabelUpdate, FAILURE> labelUpdateVisitor, bool forceStoreScan ) where FAILURE : Exception
			  {

					return new ListenableNodeScanViewNodeStoreScan<FAILURE>( _outerInstance, new RecordStorageReader( NeoStores ), Locks, labelUpdateVisitor, propertyUpdatesVisitor, labelIds, propertyKeyIdFilter, ProcessListenerConflict );
			  }

			  internal virtual Listener<StorageNodeCursor> ProcessListener
			  {
				  set
				  {
						this.ProcessListenerConflict = value;
				  }
			  }
		 }

		 private class ListenableNodeScanViewNodeStoreScan<FAILURE> : StoreViewNodeStoreScan<FAILURE> where FAILURE : Exception
		 {
			 private readonly MultipleIndexPopulatorUpdatesTest _outerInstance;

			  internal readonly Listener<StorageNodeCursor> ProcessListener;

			  internal ListenableNodeScanViewNodeStoreScan( MultipleIndexPopulatorUpdatesTest outerInstance, StorageReader storageReader, LockService locks, Visitor<NodeLabelUpdate, FAILURE> labelUpdateVisitor, Visitor<EntityUpdates, FAILURE> propertyUpdatesVisitor, int[] labelIds, System.Func<int, bool> propertyKeyIdFilter, Listener<StorageNodeCursor> processListener ) : base( storageReader, locks, labelUpdateVisitor, propertyUpdatesVisitor, labelIds, propertyKeyIdFilter )
			  {
				  this._outerInstance = outerInstance;
					this.ProcessListener = processListener;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean process(org.neo4j.storageengine.api.StorageNodeCursor cursor) throws FAILURE
			  public override bool Process( StorageNodeCursor cursor )
			  {
					ProcessListener.receive( cursor );
					return base.Process( cursor );
			  }
		 }
	}

}