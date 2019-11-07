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
namespace Neo4Net.Kernel.impl.transaction.state.storeview
{
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;
	using MutableIntSet = org.eclipse.collections.api.set.primitive.MutableIntSet;

	using Neo4Net.Collections.Helpers;
	using IEntityNotFoundException = Neo4Net.Kernel.Api.Internal.Exceptions.EntityNotFoundException;
	using LabelScanStore = Neo4Net.Kernel.Api.LabelScan.LabelScanStore;
	using NodeLabelUpdate = Neo4Net.Kernel.Api.LabelScan.NodeLabelUpdate;
	using IEntityUpdates = Neo4Net.Kernel.Impl.Api.index.EntityUpdates;
	using IndexStoreView = Neo4Net.Kernel.Impl.Api.index.IndexStoreView;
	using Neo4Net.Kernel.Impl.Api.index;
	using LockService = Neo4Net.Kernel.impl.locking.LockService;
	using RecordStorageReader = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageReader;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using Register = Neo4Net.Register.Register;
	using EntityType = Neo4Net.Kernel.Api.StorageEngine.EntityType;
	using FeatureToggles = Neo4Net.Utils.FeatureToggles;
	using Value = Neo4Net.Values.Storable.Value;

	/// <summary>
	/// Store view that will try to use label scan store <seealso cref="LabelScanStore"/> to produce the view unless label scan
	/// store is empty or explicitly told to use store in which cases it will fallback to whole store scan.
	/// </summary>
	public class DynamicIndexStoreView : IndexStoreView
	{
		 private static bool _useLabelIndexForSchemaIndexPopulation = FeatureToggles.flag( typeof( DynamicIndexStoreView ), "use.label.index", true );

		 private readonly NeoStoreIndexStoreView _neoStoreIndexStoreView;
		 private readonly LabelScanStore _labelScanStore;
		 protected internal readonly LockService Locks;
		 private readonly Log _log;
		 private readonly NeoStores _neoStores;

		 public DynamicIndexStoreView( NeoStoreIndexStoreView neoStoreIndexStoreView, LabelScanStore labelScanStore, LockService locks, NeoStores neoStores, LogProvider logProvider )
		 {
			  this._neoStores = neoStores;
			  this._neoStoreIndexStoreView = neoStoreIndexStoreView;
			  this.Locks = locks;
			  this._labelScanStore = labelScanStore;
			  this._log = logProvider.getLog( this.GetType() );
		 }

		 public override StoreScan<FAILURE> VisitNodes<FAILURE>( int[] labelIds, System.Func<int, bool> propertyKeyIdFilter, Visitor<EntityUpdates, FAILURE> propertyUpdatesVisitor, Visitor<NodeLabelUpdate, FAILURE> labelUpdateVisitor, bool forceStoreScan ) where FAILURE : Exception
		 {
			  if ( forceStoreScan || !_useLabelIndexForSchemaIndexPopulation || UseAllNodeStoreScan( labelIds ) )
			  {
					return _neoStoreIndexStoreView.visitNodes( labelIds, propertyKeyIdFilter, propertyUpdatesVisitor, labelUpdateVisitor, forceStoreScan );
			  }
			  return new LabelScanViewNodeStoreScan<FAILURE>( new RecordStorageReader( _neoStores ), Locks, _labelScanStore, labelUpdateVisitor, propertyUpdatesVisitor, labelIds, propertyKeyIdFilter );
		 }

		 public override StoreScan<FAILURE> VisitRelationships<FAILURE>( int[] relationshipTypeIds, System.Func<int, bool> propertyKeyIdFilter, Visitor<EntityUpdates, FAILURE> propertyUpdateVisitor ) where FAILURE : Exception
		 {
			  return new RelationshipStoreScan<FAILURE>( new RecordStorageReader( _neoStores ), Locks, propertyUpdateVisitor, relationshipTypeIds, propertyKeyIdFilter );
		 }

		 public override IEntityUpdates NodeAsUpdates( long nodeId )
		 {
			  return _neoStoreIndexStoreView.nodeAsUpdates( nodeId );
		 }

		 public override Neo4Net.Register.Register_DoubleLongRegister IndexUpdatesAndSize( long indexId, Neo4Net.Register.Register_DoubleLongRegister output )
		 {
			  return _neoStoreIndexStoreView.indexUpdatesAndSize( indexId, output );
		 }

		 public override Neo4Net.Register.Register_DoubleLongRegister IndexSample( long indexId, Neo4Net.Register.Register_DoubleLongRegister output )
		 {
			  return _neoStoreIndexStoreView.indexSample( indexId, output );
		 }

		 public override void ReplaceIndexCounts( long indexId, long uniqueElements, long maxUniqueElements, long indexSize )
		 {
			  _neoStoreIndexStoreView.replaceIndexCounts( indexId, uniqueElements, maxUniqueElements, indexSize );
		 }

		 public override void IncrementIndexUpdates( long indexId, long updatesDelta )
		 {
			  _neoStoreIndexStoreView.incrementIndexUpdates( indexId, updatesDelta );
		 }

		 private bool UseAllNodeStoreScan( int[] labelIds )
		 {
			  try
			  {
					return ArrayUtils.isEmpty( labelIds ) || EmptyLabelScanStore;
			  }
			  catch ( Exception e )
			  {
					_log.error( "Can not determine number of labeled nodes, falling back to all nodes scan.", e );
					return true;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean isEmptyLabelScanStore() throws Exception
		 private bool EmptyLabelScanStore
		 {
			 get
			 {
				  return _labelScanStore.Empty;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.values.storable.Value getNodePropertyValue(long nodeId, int propertyKeyId) throws Neo4Net.Kernel.Api.Internal.Exceptions.EntityNotFoundException
		 public override Value GetNodePropertyValue( long nodeId, int propertyKeyId )
		 {
			  return _neoStoreIndexStoreView.getNodePropertyValue( nodeId, propertyKeyId );
		 }

		 public override void LoadProperties( long IEntityId, EntityType type, MutableIntSet propertyIds, PropertyLoadSink sink )
		 {
			  _neoStoreIndexStoreView.loadProperties( IEntityId, type, propertyIds, sink );
		 }
	}

}