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
	using MutableIntSet = org.eclipse.collections.api.set.primitive.MutableIntSet;

	using Org.Neo4j.Helpers.Collection;
	using EntityNotFoundException = Org.Neo4j.@internal.Kernel.Api.exceptions.EntityNotFoundException;
	using NodeLabelUpdate = Org.Neo4j.Kernel.api.labelscan.NodeLabelUpdate;
	using CountsAccessor = Org.Neo4j.Kernel.Impl.Api.CountsAccessor;
	using EntityUpdates = Org.Neo4j.Kernel.Impl.Api.index.EntityUpdates;
	using IndexStoreView = Org.Neo4j.Kernel.Impl.Api.index.IndexStoreView;
	using Org.Neo4j.Kernel.Impl.Api.index;
	using LockService = Org.Neo4j.Kernel.impl.locking.LockService;
	using RecordStorageReader = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.RecordStorageReader;
	using NeoStores = Org.Neo4j.Kernel.impl.store.NeoStores;
	using NodeStore = Org.Neo4j.Kernel.impl.store.NodeStore;
	using PropertyStore = Org.Neo4j.Kernel.impl.store.PropertyStore;
	using RelationshipStore = Org.Neo4j.Kernel.impl.store.RelationshipStore;
	using CountsTracker = Org.Neo4j.Kernel.impl.store.counts.CountsTracker;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PrimitiveRecord = Org.Neo4j.Kernel.impl.store.record.PrimitiveRecord;
	using PropertyBlock = Org.Neo4j.Kernel.impl.store.record.PropertyBlock;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using Record = Org.Neo4j.Kernel.impl.store.record.Record;
	using Register_DoubleLongRegister = Org.Neo4j.Register.Register_DoubleLongRegister;
	using EntityType = Org.Neo4j.Storageengine.Api.EntityType;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.NodeLabelsField.parseLabelsField;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.FORCE;

	/// <summary>
	/// Node store view that will always visit all nodes during store scan.
	/// </summary>
	public class NeoStoreIndexStoreView : IndexStoreView
	{
		 protected internal readonly PropertyStore PropertyStore;
		 protected internal readonly NodeStore NodeStore;
		 protected internal readonly RelationshipStore RelationshipStore;
		 protected internal readonly LockService Locks;
		 private readonly CountsTracker _counts;
		 private readonly NeoStores _neoStores;

		 public NeoStoreIndexStoreView( LockService locks, NeoStores neoStores )
		 {
			  this.Locks = locks;
			  this._neoStores = neoStores;
			  this.PropertyStore = neoStores.PropertyStore;
			  this.NodeStore = neoStores.NodeStore;
			  this.RelationshipStore = neoStores.RelationshipStore;
			  this._counts = neoStores.Counts;
		 }

		 public override Register_DoubleLongRegister IndexUpdatesAndSize( long indexId, Register_DoubleLongRegister output )
		 {
			  return _counts.indexUpdatesAndSize( indexId, output );
		 }

		 public override void ReplaceIndexCounts( long indexId, long uniqueElements, long maxUniqueElements, long indexSize )
		 {
			  using ( Org.Neo4j.Kernel.Impl.Api.CountsAccessor_IndexStatsUpdater updater = _counts.updateIndexCounts() )
			  {
					updater.ReplaceIndexSample( indexId, uniqueElements, maxUniqueElements );
					updater.ReplaceIndexUpdateAndSize( indexId, 0L, indexSize );
			  }
		 }

		 public override void IncrementIndexUpdates( long indexId, long updatesDelta )
		 {
			  using ( Org.Neo4j.Kernel.Impl.Api.CountsAccessor_IndexStatsUpdater updater = _counts.updateIndexCounts() )
			  {
					updater.IncrementIndexUpdates( indexId, updatesDelta );
			  }
		 }

		 public override Register_DoubleLongRegister IndexSample( long indexId, Register_DoubleLongRegister output )
		 {
			  return _counts.indexSample( indexId, output );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public <FAILURE extends Exception> org.neo4j.kernel.impl.api.index.StoreScan<FAILURE> visitNodes(final int[] labelIds, System.Func<int, boolean> propertyKeyIdFilter, final org.neo4j.helpers.collection.Visitor<org.neo4j.kernel.impl.api.index.EntityUpdates, FAILURE> propertyUpdatesVisitor, final org.neo4j.helpers.collection.Visitor<org.neo4j.kernel.api.labelscan.NodeLabelUpdate, FAILURE> labelUpdateVisitor, boolean forceStoreScan)
		 public override StoreScan<FAILURE> VisitNodes<FAILURE>( int[] labelIds, System.Func<int, bool> propertyKeyIdFilter, Visitor<EntityUpdates, FAILURE> propertyUpdatesVisitor, Visitor<NodeLabelUpdate, FAILURE> labelUpdateVisitor, bool forceStoreScan ) where FAILURE : Exception
		 {
			  return new StoreViewNodeStoreScan<FAILURE>( new RecordStorageReader( _neoStores ), Locks, labelUpdateVisitor, propertyUpdatesVisitor, labelIds, propertyKeyIdFilter );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public <FAILURE extends Exception> org.neo4j.kernel.impl.api.index.StoreScan<FAILURE> visitRelationships(final int[] relationshipTypeIds, System.Func<int, boolean> propertyKeyIdFilter, final org.neo4j.helpers.collection.Visitor<org.neo4j.kernel.impl.api.index.EntityUpdates,FAILURE> propertyUpdatesVisitor)
		 public override StoreScan<FAILURE> VisitRelationships<FAILURE>( int[] relationshipTypeIds, System.Func<int, bool> propertyKeyIdFilter, Visitor<EntityUpdates, FAILURE> propertyUpdatesVisitor ) where FAILURE : Exception
		 {
			  return new RelationshipStoreScan<FAILURE>( new RecordStorageReader( _neoStores ), Locks, propertyUpdatesVisitor, relationshipTypeIds, propertyKeyIdFilter );
		 }

		 public override EntityUpdates NodeAsUpdates( long nodeId )
		 {
			  NodeRecord node = NodeStore.getRecord( nodeId, NodeStore.newRecord(), FORCE );
			  if ( !node.InUse() )
			  {
					return null;
			  }
			  long firstPropertyId = node.NextProp;
			  if ( firstPropertyId == Record.NO_NEXT_PROPERTY.intValue() )
			  {
					return null; // no properties => no updates (it's not going to be in any index)
			  }
			  long[] labels = parseLabelsField( node ).get( NodeStore );
			  if ( labels.Length == 0 )
			  {
					return null; // no labels => no updates (it's not going to be in any index)
			  }
			  EntityUpdates.Builder update = EntityUpdates.forEntity( nodeId, true ).withTokens( labels );
			  foreach ( PropertyRecord propertyRecord in PropertyStore.getPropertyRecordChain( firstPropertyId ) )
			  {
					foreach ( PropertyBlock property in propertyRecord )
					{
						 Value value = property.Type.value( property, PropertyStore );
						 update.Added( property.KeyIndexId, value );
					}
			  }
			  return update.Build();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.values.storable.Value getNodePropertyValue(long nodeId, int propertyKeyId) throws org.neo4j.internal.kernel.api.exceptions.EntityNotFoundException
		 public override Value GetNodePropertyValue( long nodeId, int propertyKeyId )
		 {
			  NodeRecord node = NodeStore.getRecord( nodeId, NodeStore.newRecord(), FORCE );
			  if ( !node.InUse() )
			  {
					throw new EntityNotFoundException( EntityType.NODE, nodeId );
			  }
			  long firstPropertyId = node.NextProp;
			  if ( firstPropertyId == Record.NO_NEXT_PROPERTY.intValue() )
			  {
					return Values.NO_VALUE;
			  }
			  foreach ( PropertyRecord propertyRecord in PropertyStore.getPropertyRecordChain( firstPropertyId ) )
			  {
					PropertyBlock propertyBlock = propertyRecord.GetPropertyBlock( propertyKeyId );
					if ( propertyBlock != null )
					{
						 return propertyBlock.NewPropertyValue( PropertyStore );
					}
			  }
			  return Values.NO_VALUE;
		 }

		 public override void LoadProperties( long entityId, EntityType type, MutableIntSet propertyIds, PropertyLoadSink sink )
		 {
			  PrimitiveRecord entity;
			  if ( type == EntityType.NODE )
			  {
					entity = NodeStore.getRecord( entityId, NodeStore.newRecord(), FORCE );
			  }
			  else
			  {
					entity = RelationshipStore.getRecord( entityId, RelationshipStore.newRecord(), FORCE );
			  }
			  if ( !entity.InUse() )
			  {
					return;
			  }
			  long firstPropertyId = entity.NextProp;
			  if ( firstPropertyId == Record.NO_NEXT_PROPERTY.intValue() )
			  {
					return;
			  }
			  foreach ( PropertyRecord propertyRecord in PropertyStore.getPropertyRecordChain( firstPropertyId ) )
			  {
					foreach ( PropertyBlock block in propertyRecord )
					{
						 int currentPropertyId = block.KeyIndexId;
						 if ( propertyIds.remove( currentPropertyId ) )
						 {
							  Value currentValue = block.Type.value( block, PropertyStore );
							  sink.onProperty( currentPropertyId, currentValue );
						 }
					}
			  }
		 }
	}

}