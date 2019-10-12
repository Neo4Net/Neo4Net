using System;

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
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;
	using MutableIntSet = org.eclipse.collections.api.set.primitive.MutableIntSet;

	using Org.Neo4j.Helpers.Collection;
	using EntityNotFoundException = Org.Neo4j.@internal.Kernel.Api.exceptions.EntityNotFoundException;
	using LabelScanStore = Org.Neo4j.Kernel.api.labelscan.LabelScanStore;
	using NodeLabelUpdate = Org.Neo4j.Kernel.api.labelscan.NodeLabelUpdate;
	using EntityUpdates = Org.Neo4j.Kernel.Impl.Api.index.EntityUpdates;
	using IndexStoreView = Org.Neo4j.Kernel.Impl.Api.index.IndexStoreView;
	using Org.Neo4j.Kernel.Impl.Api.index;
	using LockService = Org.Neo4j.Kernel.impl.locking.LockService;
	using RecordStorageReader = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.RecordStorageReader;
	using NeoStores = Org.Neo4j.Kernel.impl.store.NeoStores;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using Register = Org.Neo4j.Register.Register;
	using EntityType = Org.Neo4j.Storageengine.Api.EntityType;
	using FeatureToggles = Org.Neo4j.Util.FeatureToggles;
	using Value = Org.Neo4j.Values.Storable.Value;

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

		 public override EntityUpdates NodeAsUpdates( long nodeId )
		 {
			  return _neoStoreIndexStoreView.nodeAsUpdates( nodeId );
		 }

		 public override Org.Neo4j.Register.Register_DoubleLongRegister IndexUpdatesAndSize( long indexId, Org.Neo4j.Register.Register_DoubleLongRegister output )
		 {
			  return _neoStoreIndexStoreView.indexUpdatesAndSize( indexId, output );
		 }

		 public override Org.Neo4j.Register.Register_DoubleLongRegister IndexSample( long indexId, Org.Neo4j.Register.Register_DoubleLongRegister output )
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
//ORIGINAL LINE: public org.neo4j.values.storable.Value getNodePropertyValue(long nodeId, int propertyKeyId) throws org.neo4j.internal.kernel.api.exceptions.EntityNotFoundException
		 public override Value GetNodePropertyValue( long nodeId, int propertyKeyId )
		 {
			  return _neoStoreIndexStoreView.getNodePropertyValue( nodeId, propertyKeyId );
		 }

		 public override void LoadProperties( long entityId, EntityType type, MutableIntSet propertyIds, PropertyLoadSink sink )
		 {
			  _neoStoreIndexStoreView.loadProperties( entityId, type, propertyIds, sink );
		 }
	}

}