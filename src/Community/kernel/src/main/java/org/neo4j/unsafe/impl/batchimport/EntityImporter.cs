using System.Diagnostics;

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
namespace Neo4Net.@unsafe.Impl.Batchimport
{

	using DynamicRecordAllocator = Neo4Net.Kernel.impl.store.DynamicRecordAllocator;
	using PropertyStore = Neo4Net.Kernel.impl.store.PropertyStore;
	using PropertyType = Neo4Net.Kernel.impl.store.PropertyType;
	using StandardDynamicRecordAllocator = Neo4Net.Kernel.impl.store.StandardDynamicRecordAllocator;
	using PrimitiveRecord = Neo4Net.Kernel.impl.store.record.PrimitiveRecord;
	using PropertyBlock = Neo4Net.Kernel.impl.store.record.PropertyBlock;
	using PropertyRecord = Neo4Net.Kernel.impl.store.record.PropertyRecord;
	using Record = Neo4Net.Kernel.impl.store.record.Record;
	using ValueUtils = Neo4Net.Kernel.impl.util.ValueUtils;
	using Monitor = Neo4Net.@unsafe.Impl.Batchimport.DataImporter.Monitor;
	using InputEntityVisitor = Neo4Net.@unsafe.Impl.Batchimport.input.InputEntityVisitor;
	using BatchingNeoStores = Neo4Net.@unsafe.Impl.Batchimport.store.BatchingNeoStores;
	using BatchingPropertyKeyTokenRepository = Neo4Net.@unsafe.Impl.Batchimport.store.BatchingTokenRepository.BatchingPropertyKeyTokenRepository;

	/// <summary>
	/// Abstract class containing logic for importing properties for an entity (node/relationship).
	/// </summary>
	internal abstract class EntityImporter : Neo4Net.@unsafe.Impl.Batchimport.input.InputEntityVisitor_Adapter
	{
		 private readonly BatchingPropertyKeyTokenRepository _propertyKeyTokenRepository;
		 private readonly PropertyStore _propertyStore;
		 private readonly PropertyRecord _propertyRecord;
		 private PropertyBlock[] _propertyBlocks = new PropertyBlock[100];
		 private int _propertyBlocksCursor;
		 private readonly BatchingIdGetter _propertyIds;
		 protected internal readonly Monitor Monitor;
		 private long _propertyCount;
		 protected internal int EntityPropertyCount; // just for the current entity
		 private bool _hasPropertyId;
		 private long _propertyId;
		 private readonly DynamicRecordAllocator _dynamicStringRecordAllocator;
		 private readonly DynamicRecordAllocator _dynamicArrayRecordAllocator;

		 protected internal EntityImporter( BatchingNeoStores stores, Monitor monitor )
		 {
			  this._propertyStore = stores.PropertyStore;
			  this._propertyKeyTokenRepository = stores.PropertyKeyRepository;
			  this.Monitor = monitor;
			  for ( int i = 0; i < _propertyBlocks.Length; i++ )
			  {
					_propertyBlocks[i] = new PropertyBlock();
			  }
			  this._propertyRecord = _propertyStore.newRecord();
			  this._propertyIds = new BatchingIdGetter( _propertyStore );
			  this._dynamicStringRecordAllocator = new StandardDynamicRecordAllocator( new BatchingIdGetter( _propertyStore.StringStore, _propertyStore.StringStore.RecordsPerPage ), _propertyStore.StringStore.RecordDataSize );
			  this._dynamicArrayRecordAllocator = new StandardDynamicRecordAllocator( new BatchingIdGetter( _propertyStore.ArrayStore, _propertyStore.ArrayStore.RecordsPerPage ), _propertyStore.StringStore.RecordDataSize );
		 }

		 public override bool Property( string key, object value )
		 {
			  Debug.Assert( !_hasPropertyId );
			  return Property( _propertyKeyTokenRepository.getOrCreateId( key ), value );
		 }

		 public override bool Property( int propertyKeyId, object value )
		 {
			  Debug.Assert( !_hasPropertyId );
			  EncodeProperty( NextPropertyBlock(), propertyKeyId, value );
			  EntityPropertyCount++;
			  return true;
		 }

		 public override bool PropertyId( long nextProp )
		 {
			  Debug.Assert( !_hasPropertyId );
			  _hasPropertyId = true;
			  _propertyId = nextProp;
			  return true;
		 }

		 public override void EndOfEntity()
		 {
			  _propertyBlocksCursor = 0;
			  _hasPropertyId = false;
			  _propertyCount += EntityPropertyCount;
			  EntityPropertyCount = 0;
		 }

		 private PropertyBlock NextPropertyBlock()
		 {
			  if ( _propertyBlocksCursor == _propertyBlocks.Length )
			  {
					_propertyBlocks = Arrays.copyOf( _propertyBlocks, _propertyBlocksCursor * 2 );
					for ( int i = _propertyBlocksCursor; i < _propertyBlocks.Length; i++ )
					{
						 _propertyBlocks[i] = new PropertyBlock();
					}
			  }
			  return _propertyBlocks[_propertyBlocksCursor++];
		 }

		 private void EncodeProperty( PropertyBlock block, int key, object value )
		 {
			  PropertyStore.encodeValue( block, key, ValueUtils.asValue( value ), _dynamicStringRecordAllocator, _dynamicArrayRecordAllocator, _propertyStore.allowStorePointsAndTemporal() );
		 }

		 protected internal virtual long CreateAndWritePropertyChain()
		 {
			  if ( _hasPropertyId )
			  {
					return _propertyId;
			  }

			  if ( _propertyBlocksCursor == 0 )
			  {
					return Record.NO_NEXT_PROPERTY.longValue();
			  }

			  PropertyRecord currentRecord = PropertyRecord( _propertyIds.next() );
			  long firstRecordId = currentRecord.Id;
			  for ( int i = 0; i < _propertyBlocksCursor; i++ )
			  {
					PropertyBlock block = _propertyBlocks[i];
					if ( currentRecord.Size() + block.Size > PropertyType.PayloadSize )
					{
						 // This record is full or couldn't fit this block, write it to property store
						 long nextPropertyId = _propertyIds.next();
						 long prevId = currentRecord.Id;
						 currentRecord.NextProp = nextPropertyId;
						 _propertyStore.updateRecord( currentRecord );
						 currentRecord = PropertyRecord( nextPropertyId );
						 currentRecord.PrevProp = prevId;
					}

					// Add this block, there's room for it
					currentRecord.AddPropertyBlock( block );
			  }

			  if ( currentRecord.Size() > 0 )
			  {
					_propertyStore.updateRecord( currentRecord );
			  }

			  return firstRecordId;
		 }

		 protected internal abstract PrimitiveRecord PrimitiveRecord();

		 private PropertyRecord PropertyRecord( long nextPropertyId )
		 {
			  _propertyRecord.clear();
			  _propertyRecord.InUse = true;
			  _propertyRecord.Id = nextPropertyId;
			  PrimitiveRecord().IdTo = _propertyRecord;
			  _propertyRecord.setCreated();
			  return _propertyRecord;
		 }

		 public override void Close()
		 {
			  Monitor.propertiesImported( _propertyCount );
		 }
	}

}