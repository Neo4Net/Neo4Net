﻿using System;

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
namespace Org.Neo4j.Kernel.impl.transaction.state.storeview
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using Mockito = org.mockito.Mockito;

	using PrimitiveLongResourceCollections = Org.Neo4j.Collection.PrimitiveLongResourceCollections;
	using PrimitiveLongResourceIterator = Org.Neo4j.Collection.PrimitiveLongResourceIterator;
	using Org.Neo4j.Helpers.Collection;
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using AllEntriesLabelScanReader = Org.Neo4j.Kernel.api.labelscan.AllEntriesLabelScanReader;
	using LabelScanStore = Org.Neo4j.Kernel.api.labelscan.LabelScanStore;
	using NodeLabelUpdate = Org.Neo4j.Kernel.api.labelscan.NodeLabelUpdate;
	using EntityUpdates = Org.Neo4j.Kernel.Impl.Api.index.EntityUpdates;
	using Org.Neo4j.Kernel.Impl.Api.index;
	using LockService = Org.Neo4j.Kernel.impl.locking.LockService;
	using NeoStores = Org.Neo4j.Kernel.impl.store.NeoStores;
	using NodeStore = Org.Neo4j.Kernel.impl.store.NodeStore;
	using CountsTracker = Org.Neo4j.Kernel.impl.store.counts.CountsTracker;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using RecordLoad = Org.Neo4j.Kernel.impl.store.record.RecordLoad;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using Register = Org.Neo4j.Register.Register;
	using Registers = Org.Neo4j.Register.Registers;
	using LabelScanReader = Org.Neo4j.Storageengine.Api.schema.LabelScanReader;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class DynamicIndexStoreViewTest
	{
		 private readonly LabelScanStore _labelScanStore = mock( typeof( LabelScanStore ) );
		 private readonly NeoStores _neoStores = mock( typeof( NeoStores ) );
		 private readonly NodeStore _nodeStore = mock( typeof( NodeStore ) );
		 private readonly CountsTracker _countStore = mock( typeof( CountsTracker ) );
		 private readonly Visitor<EntityUpdates, Exception> _propertyUpdateVisitor = mock( typeof( Visitor ) );
		 private readonly Visitor<NodeLabelUpdate, Exception> _labelUpdateVisitor = mock( typeof( Visitor ) );
		 private readonly System.Func<int, bool> _propertyKeyIdFilter = mock( typeof( System.Func<int, bool> ) );
		 private readonly AllEntriesLabelScanReader _nodeLabelRanges = mock( typeof( AllEntriesLabelScanReader ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  NodeRecord nodeRecord = NodeRecord;
			  when( _labelScanStore.allNodeLabelRanges() ).thenReturn(_nodeLabelRanges);
			  when( _neoStores.Counts ).thenReturn( _countStore );
			  when( _neoStores.NodeStore ).thenReturn( _nodeStore );
			  when( _nodeStore.newRecord() ).thenReturn(nodeRecord);
			  doAnswer(invocation =>
			  {
				NodeRecord record = invocation.getArgument( 1 );
				record.initialize( true, 1L, false, 1L, 0L );
				record.Id = invocation.getArgument( 0 );
				return null;
			  }).when( _nodeStore ).getRecordByCursor( anyLong(), any(typeof(NodeRecord)), any(typeof(RecordLoad)), any(typeof(PageCursor)) );
			  doAnswer(invocation =>
			  {
				NodeRecord record = invocation.getArgument( 0 );
				record.initialize( true, 1L, false, 1L, 0L );
				record.Id = record.Id + 1;
				return null;
			  }).when( _nodeStore ).nextRecordByCursor( any( typeof( NodeRecord ) ), any( typeof( RecordLoad ) ), any( typeof( PageCursor ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void visitOnlyLabeledNodes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void VisitOnlyLabeledNodes()
		 {
			  LabelScanReader labelScanReader = mock( typeof( LabelScanReader ) );
			  when( _labelScanStore.newReader() ).thenReturn(labelScanReader);
			  when( _nodeLabelRanges.maxCount() ).thenReturn(1L);

			  PrimitiveLongResourceIterator labeledNodesIterator = PrimitiveLongResourceCollections.iterator( null, 1, 2, 3, 4, 5, 6, 7, 8 );
			  when( _nodeStore.HighestPossibleIdInUse ).thenReturn( 200L );
			  when( _nodeStore.HighId ).thenReturn( 20L );
			  when( labelScanReader.NodesWithAnyOfLabels( new int[] { 2, 6 } ) ).thenReturn( labeledNodesIterator );
			  when( _nodeStore.openPageCursorForReading( anyLong() ) ).thenReturn(mock(typeof(PageCursor)));

			  MockLabelNodeCount( _countStore, 2 );
			  MockLabelNodeCount( _countStore, 6 );

			  DynamicIndexStoreView storeView = DynamicIndexStoreView();

			  StoreScan<Exception> storeScan = storeView.VisitNodes( new int[]{ 2, 6 }, _propertyKeyIdFilter, _propertyUpdateVisitor, _labelUpdateVisitor, false );

			  storeScan.Run();

			  Mockito.verify( _nodeStore, times( 8 ) ).getRecordByCursor( anyLong(), any(typeof(NodeRecord)), any(typeof(RecordLoad)), any(typeof(PageCursor)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToForceStoreScan() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToForceStoreScan()
		 {
			  when( _labelScanStore.newReader() ).thenThrow(new Exception("Should not be used"));

			  when( _nodeStore.HighestPossibleIdInUse ).thenReturn( 200L );
			  when( _nodeStore.HighId ).thenReturn( 20L );
			  when( _nodeStore.openPageCursorForReading( anyLong() ) ).thenReturn(mock(typeof(PageCursor)));

			  MockLabelNodeCount( _countStore, 2 );
			  MockLabelNodeCount( _countStore, 6 );

			  DynamicIndexStoreView storeView = DynamicIndexStoreView();

			  StoreScan<Exception> storeScan = storeView.VisitNodes( new int[]{ 2, 6 }, _propertyKeyIdFilter, _propertyUpdateVisitor, _labelUpdateVisitor, true );

			  storeScan.Run();

			  Mockito.verify( _nodeStore, times( 1 ) ).getRecordByCursor( anyLong(), any(typeof(NodeRecord)), any(typeof(RecordLoad)), any(typeof(PageCursor)) );
			  Mockito.verify( _nodeStore, times( 200 ) ).nextRecordByCursor( any( typeof( NodeRecord ) ), any( typeof( RecordLoad ) ), any( typeof( PageCursor ) ) );
		 }

		 private DynamicIndexStoreView DynamicIndexStoreView()
		 {
			  LockService locks = LockService.NO_LOCK_SERVICE;
			  return new DynamicIndexStoreView( new NeoStoreIndexStoreView( locks, _neoStores ), _labelScanStore, locks, _neoStores, NullLogProvider.Instance );
		 }

		 private NodeRecord NodeRecord
		 {
			 get
			 {
				  NodeRecord nodeRecord = new NodeRecord( 0L );
				  nodeRecord.Initialize( true, 1L, false, 1L, 0L );
				  return nodeRecord;
			 }
		 }

		 private void MockLabelNodeCount( CountsTracker countStore, int labelId )
		 {
			  Org.Neo4j.Register.Register_DoubleLongRegister register = Registers.newDoubleLongRegister( labelId, labelId );
			  when( countStore.NodeCount( eq( labelId ), any( typeof( Org.Neo4j.Register.Register_DoubleLongRegister ) ) ) ).thenReturn( register );
		 }

	}

}