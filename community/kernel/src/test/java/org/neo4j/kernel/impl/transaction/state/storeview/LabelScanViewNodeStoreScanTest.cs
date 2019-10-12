using System;
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
namespace Org.Neo4j.Kernel.impl.transaction.state.storeview
{
	using Matchers = org.hamcrest.Matchers;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using PrimitiveLongCollections = Org.Neo4j.Collection.PrimitiveLongCollections;
	using PrimitiveLongResourceCollections = Org.Neo4j.Collection.PrimitiveLongResourceCollections;
	using PrimitiveLongResourceIterator = Org.Neo4j.Collection.PrimitiveLongResourceIterator;
	using Org.Neo4j.Helpers.Collection;
	using LabelScanStore = Org.Neo4j.Kernel.api.labelscan.LabelScanStore;
	using NodeLabelUpdate = Org.Neo4j.Kernel.api.labelscan.NodeLabelUpdate;
	using EntityUpdates = Org.Neo4j.Kernel.Impl.Api.index.EntityUpdates;
	using LockService = Org.Neo4j.Kernel.impl.locking.LockService;
	using RecordStorageReader = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.RecordStorageReader;
	using NeoStores = Org.Neo4j.Kernel.impl.store.NeoStores;
	using NodeStore = Org.Neo4j.Kernel.impl.store.NodeStore;
	using LabelScanReader = Org.Neo4j.Storageengine.Api.schema.LabelScanReader;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class LabelScanViewNodeStoreScanTest
	{
		 private NodeStore _nodeStore = mock( typeof( NodeStore ) );
		 private NeoStores _neoStores = mock( typeof( NeoStores ) );
		 private LabelScanStore _labelScanStore = mock( typeof( LabelScanStore ) );
		 private LabelScanReader _labelScanReader = mock( typeof( LabelScanReader ) );
		 private System.Func<int, bool> _propertyKeyIdFilter = mock( typeof( System.Func<int, bool> ) );
		 private Visitor<NodeLabelUpdate, Exception> _labelUpdateVisitor = mock( typeof( Visitor ) );
		 private Visitor<EntityUpdates, Exception> _propertyUpdateVisitor = mock( typeof( Visitor ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  when( _labelScanStore.newReader() ).thenReturn(_labelScanReader);
			  when( _neoStores.NodeStore ).thenReturn( _nodeStore );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void iterateOverLabeledNodeIds()
		 public virtual void IterateOverLabeledNodeIds()
		 {
			  PrimitiveLongResourceIterator labeledNodes = PrimitiveLongResourceCollections.iterator( null, 1, 2, 4, 8 );

			  when( _nodeStore.HighId ).thenReturn( 15L );
			  int[] labelIds = new int[]{ 1, 2 };
			  when( _labelScanReader.nodesWithAnyOfLabels( labelIds ) ).thenReturn( labeledNodes );

			  LabelScanViewNodeStoreScan<Exception> storeScan = GetLabelScanViewStoreScan( labelIds );
			  PrimitiveLongResourceIterator idIterator = storeScan.EntityIdIterator;
			  IList<long> visitedNodeIds = PrimitiveLongCollections.asList( idIterator );

			  assertThat( visitedNodeIds, Matchers.hasSize( 4 ) );
			  assertThat( visitedNodeIds, Matchers.hasItems( 1L, 2L, 4L, 8L ) );
		 }

		 private LabelScanViewNodeStoreScan<Exception> GetLabelScanViewStoreScan( int[] labelIds )
		 {
			  return new LabelScanViewNodeStoreScan<Exception>( new RecordStorageReader( _neoStores ), LockService.NO_LOCK_SERVICE, _labelScanStore, _labelUpdateVisitor, _propertyUpdateVisitor, labelIds, _propertyKeyIdFilter );
		 }
	}

}