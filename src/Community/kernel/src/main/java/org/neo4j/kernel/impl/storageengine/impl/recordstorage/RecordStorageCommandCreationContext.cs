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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using StandardDynamicRecordAllocator = Neo4Net.Kernel.impl.store.StandardDynamicRecordAllocator;
	using StoreType = Neo4Net.Kernel.impl.store.StoreType;
	using RenewableBatchIdSequences = Neo4Net.Kernel.impl.store.id.RenewableBatchIdSequences;
	using IntegrityValidator = Neo4Net.Kernel.impl.transaction.state.IntegrityValidator;
	using RecordChangeSet = Neo4Net.Kernel.impl.transaction.state.RecordChangeSet;
	using CommandCreationContext = Neo4Net.Storageengine.Api.CommandCreationContext;
	using ResourceLocker = Neo4Net.Storageengine.Api.@lock.ResourceLocker;

	/// <summary>
	/// Holds commit data structures for creating records in a <seealso cref="NeoStores"/>.
	/// </summary>
	internal class RecordStorageCommandCreationContext : CommandCreationContext
	{
		 private readonly NeoStores _neoStores;
		 private readonly Loaders _loaders;
		 private readonly RelationshipCreator _relationshipCreator;
		 private readonly RelationshipDeleter _relationshipDeleter;
		 private readonly PropertyCreator _propertyCreator;
		 private readonly PropertyDeleter _propertyDeleter;
		 private readonly RenewableBatchIdSequences _idBatches;

		 internal RecordStorageCommandCreationContext( NeoStores neoStores, int denseNodeThreshold, int idBatchSize )
		 {
			  this._neoStores = neoStores;
			  this._idBatches = new RenewableBatchIdSequences( neoStores, idBatchSize );

			  this._loaders = new Loaders( neoStores );
			  RelationshipGroupGetter relationshipGroupGetter = new RelationshipGroupGetter( _idBatches.idGenerator( StoreType.RELATIONSHIP_GROUP ) );
			  this._relationshipCreator = new RelationshipCreator( relationshipGroupGetter, denseNodeThreshold );
			  PropertyTraverser propertyTraverser = new PropertyTraverser();
			  this._propertyDeleter = new PropertyDeleter( propertyTraverser );
			  this._relationshipDeleter = new RelationshipDeleter( relationshipGroupGetter, _propertyDeleter );
			  this._propertyCreator = new PropertyCreator( new StandardDynamicRecordAllocator( _idBatches.idGenerator( StoreType.PROPERTY_STRING ), neoStores.PropertyStore.StringStore.RecordDataSize ), new StandardDynamicRecordAllocator( _idBatches.idGenerator( StoreType.PROPERTY_ARRAY ), neoStores.PropertyStore.ArrayStore.RecordDataSize ), _idBatches.idGenerator( StoreType.PROPERTY ), propertyTraverser, neoStores.PropertyStore.allowStorePointsAndTemporal() );
		 }

		 public virtual long NextId( StoreType storeType )
		 {
			  return _idBatches.nextId( storeType );
		 }

		 public override void Close()
		 {
			  this._idBatches.close();
		 }

		 internal virtual TransactionRecordState CreateTransactionRecordState( IntegrityValidator integrityValidator, long lastTransactionIdWhenStarted, ResourceLocker locks )
		 {
			  RecordChangeSet recordChangeSet = new RecordChangeSet( _loaders );
			  return new TransactionRecordState( _neoStores, integrityValidator, recordChangeSet, lastTransactionIdWhenStarted, locks, _relationshipCreator, _relationshipDeleter, _propertyCreator, _propertyDeleter );
		 }
	}

}