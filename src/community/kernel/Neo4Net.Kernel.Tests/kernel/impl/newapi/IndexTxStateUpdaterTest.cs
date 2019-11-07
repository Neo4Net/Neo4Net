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
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using IndexNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException;
	using StubNodeCursor = Neo4Net.Kernel.Api.Internal.Helpers.StubNodeCursor;
	using StubPropertyCursor = Neo4Net.Kernel.Api.Internal.Helpers.StubPropertyCursor;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.Api.schema.index.TestIndexDescriptorFactory;
	using TransactionState = Neo4Net.Kernel.Api.txstate.TransactionState;
	using IndexProxy = Neo4Net.Kernel.Impl.Api.index.IndexProxy;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueTuple = Neo4Net.Values.Storable.ValueTuple;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.ArrayUtils.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.isNull;
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
//	import static Neo4Net.kernel.impl.newapi.IndexTxStateUpdater.LabelChangeType.ADDED_LABEL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.newapi.IndexTxStateUpdater.LabelChangeType.REMOVED_LABEL;

	public class IndexTxStateUpdaterTest
	{
		private bool InstanceFieldsInitialized = false;

		public IndexTxStateUpdaterTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_indexes = Arrays.asList( _indexOn1_1, _indexOn2New, _uniqueOn1_2, _indexOn1_1New, _uniqueOn2_2_3 );
		}

		 private const int LABEL_ID1 = 10;
		 private const int LABEL_ID2 = 11;
		 private const int UN_INDEXED_LABEL_ID = 12;
		 private const int PROP_ID1 = 20;
		 private const int PROP_ID2 = 21;
		 private const int PROP_ID3 = 22;
		 private const int NEW_PROP_ID = 23;
		 private const int UN_INDEXED_PROP_ID = 24;
		 private static readonly int[] _props = new int[]{ PROP_ID1, PROP_ID2, PROP_ID3 };

		 private TransactionState _txState;
		 private IndexTxStateUpdater _indexTxUpdater;

		 private IndexDescriptor _indexOn1_1 = TestIndexDescriptorFactory.forLabel( LABEL_ID1, PROP_ID1 );
		 private IndexDescriptor _indexOn2New = TestIndexDescriptorFactory.forLabel( LABEL_ID2, NEW_PROP_ID );
		 private IndexDescriptor _uniqueOn1_2 = TestIndexDescriptorFactory.uniqueForLabel( LABEL_ID1, PROP_ID2 );
		 private IndexDescriptor _indexOn1_1New = TestIndexDescriptorFactory.forLabel( LABEL_ID1, PROP_ID1, NEW_PROP_ID );
		 private IndexDescriptor _uniqueOn2_2_3 = TestIndexDescriptorFactory.uniqueForLabel( LABEL_ID2, PROP_ID2, PROP_ID3 );
		 private IList<IndexDescriptor> _indexes;
		 private StubNodeCursor _node;
		 private StubPropertyCursor _propertyCursor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _txState = mock( typeof( TransactionState ) );

			  Dictionary<int, Value> map = new Dictionary<int, Value>();
			  map[PROP_ID1] = Values.of( "hi1" );
			  map[PROP_ID2] = Values.of( "hi2" );
			  map[PROP_ID3] = Values.of( "hi3" );
			  _node = ( new StubNodeCursor() ).withNode(0, new long[]{ LABEL_ID1, LABEL_ID2 }, map);
			  _node.next();

			  _propertyCursor = new StubPropertyCursor();

			  Read readOps = mock( typeof( Read ) );
			  when( readOps.TxState() ).thenReturn(_txState);

			  IndexingService indexingService = mock( typeof( IndexingService ) );
			  IndexProxy indexProxy = mock( typeof( IndexProxy ) );
			  when( indexingService.getIndexProxy( any( typeof( SchemaDescriptor ) ) ) ).thenReturn( indexProxy );
			  when( indexingService.getRelatedIndexes( any(), anyInt(), any() ) ).thenAnswer(invocationOnMock =>
			  {
				long[] labels = invocationOnMock.getArgument( 0 );
				int propertyKeyId = invocationOnMock.getArgument( 1 );
				ISet<SchemaDescriptor> descriptors = new HashSet<SchemaDescriptor>();
				foreach ( IndexDescriptor index in _indexes )
				{
					 if ( contains( labels, index.schema().keyId() ) && contains(index.schema().PropertyIds, propertyKeyId) )
					 {
						  descriptors.add( index.schema() );
					 }
				}
				return descriptors;
			  });
			  when( indexingService.getRelatedIndexes( any(), any(typeof(int[])), any() ) ).thenAnswer(invocationOnMock =>
			  {
				long[] labels = invocationOnMock.getArgument( 0 );
				int[] propertyKeyIds = invocationOnMock.getArgument( 1 );
				ISet<SchemaDescriptor> descriptors = new HashSet<SchemaDescriptor>();
				foreach ( IndexDescriptor index in _indexes )
				{
					 if ( contains( labels, index.schema().keyId() ) )
					 {
						  bool containsAll = true;
						  foreach ( int propertyId in index.schema().PropertyIds )
						  {
								containsAll &= contains( propertyKeyIds, propertyId );
						  }
						  if ( containsAll )
						  {
								descriptors.add( index.schema() );
						  }
					 }
				}
				return descriptors;
			  });

			  _indexTxUpdater = new IndexTxStateUpdater( readOps, indexingService );
		 }

		 // LABELS

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotUpdateIndexesOnChangedIrrelevantLabel()
		 public virtual void ShouldNotUpdateIndexesOnChangedIrrelevantLabel()
		 {
			  // WHEN
			  _indexTxUpdater.onLabelChange( UN_INDEXED_LABEL_ID, _props, _node, _propertyCursor, ADDED_LABEL );
			  _indexTxUpdater.onLabelChange( UN_INDEXED_LABEL_ID, _props, _node, _propertyCursor, REMOVED_LABEL );

			  // THEN
			  verify( _txState, never() ).indexDoUpdateEntry(any(), anyInt(), any(), any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateIndexesOnAddedLabel()
		 public virtual void ShouldUpdateIndexesOnAddedLabel()
		 {
			  // WHEN
			  _indexTxUpdater.onLabelChange( LABEL_ID1, _props, _node, _propertyCursor, ADDED_LABEL );

			  // THEN
			  VerifyIndexUpdate( _indexOn1_1.schema(), _node.nodeReference(), null, Values("hi1") );
			  VerifyIndexUpdate( _uniqueOn1_2.schema(), _node.nodeReference(), null, Values("hi2") );
			  verify( _txState, times( 2 ) ).indexDoUpdateEntry( any(), anyLong(), Null, any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateIndexesOnRemovedLabel()
		 public virtual void ShouldUpdateIndexesOnRemovedLabel()
		 {
			  // WHEN
			  _indexTxUpdater.onLabelChange( LABEL_ID2, _props, _node, _propertyCursor, REMOVED_LABEL );

			  // THEN
			  VerifyIndexUpdate( _uniqueOn2_2_3.schema(), _node.nodeReference(), Values("hi2", "hi3"), null );
			  verify( _txState, times( 1 ) ).indexDoUpdateEntry( any(), anyLong(), any(), Null );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotUpdateIndexesOnChangedIrrelevantProperty()
		 public virtual void ShouldNotUpdateIndexesOnChangedIrrelevantProperty()
		 {
			  // WHEN
			  _indexTxUpdater.onPropertyAdd( _node, _propertyCursor, _node.labels().all(), UN_INDEXED_PROP_ID, _props, Values.of("whAt") );
			  _indexTxUpdater.onPropertyRemove( _node, _propertyCursor, _node.labels().all(), UN_INDEXED_PROP_ID, _props, Values.of("whAt") );
			  _indexTxUpdater.onPropertyChange( _node, _propertyCursor, _node.labels().all(), UN_INDEXED_PROP_ID, _props, Values.of("whAt"), Values.of("whAt2") );

			  // THEN
			  verify( _txState, never() ).indexDoUpdateEntry(any(), anyInt(), any(), any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateIndexesOnAddedProperty()
		 public virtual void ShouldUpdateIndexesOnAddedProperty()
		 {
			  // WHEN
			  _indexTxUpdater.onPropertyAdd( _node, _propertyCursor, _node.labels().all(), NEW_PROP_ID, _props, Values.of("newHi") );

			  // THEN
			  VerifyIndexUpdate( _indexOn2New.schema(), _node.nodeReference(), null, Values("newHi") );
			  VerifyIndexUpdate( _indexOn1_1New.schema(), _node.nodeReference(), null, Values("hi1", "newHi") );
			  verify( _txState, times( 2 ) ).indexDoUpdateEntry( any(), anyLong(), Null, any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateIndexesOnRemovedProperty()
		 public virtual void ShouldUpdateIndexesOnRemovedProperty()
		 {
			  // WHEN
			  _indexTxUpdater.onPropertyRemove( _node, _propertyCursor, _node.labels().all(), PROP_ID2, _props, Values.of("hi2") );

			  // THEN
			  VerifyIndexUpdate( _uniqueOn1_2.schema(), _node.nodeReference(), Values("hi2"), null );
			  VerifyIndexUpdate( _uniqueOn2_2_3.schema(), _node.nodeReference(), Values("hi2", "hi3"), null );
			  verify( _txState, times( 2 ) ).indexDoUpdateEntry( any(), anyLong(), any(), Null );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUpdateIndexesOnChangesProperty()
		 public virtual void ShouldUpdateIndexesOnChangesProperty()
		 {
			  // WHEN
			  _indexTxUpdater.onPropertyChange( _node, _propertyCursor, _node.labels().all(), PROP_ID2, _props, Values.of("hi2"), Values.of("new2") );

			  // THEN
			  VerifyIndexUpdate( _uniqueOn1_2.schema(), _node.nodeReference(), Values("hi2"), Values("new2") );
			  VerifyIndexUpdate( _uniqueOn2_2_3.schema(), _node.nodeReference(), Values("hi2", "hi3"), Values("new2", "hi3") );
			  verify( _txState, times( 2 ) ).indexDoUpdateEntry( any(), anyLong(), any(), any() );
		 }

		 private ValueTuple Values( params object[] values )
		 {
			  return ValueTuple.of( values );
		 }

		 private void VerifyIndexUpdate( SchemaDescriptor schema, long nodeId, ValueTuple before, ValueTuple after )
		 {
			  verify( _txState ).indexDoUpdateEntry( eq( schema ), eq( nodeId ), eq( before ), eq( after ) );
		 }
	}

}