using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{

	using DynamicRecordAllocator = Neo4Net.Kernel.impl.store.DynamicRecordAllocator;
	using PropertyStore = Neo4Net.Kernel.impl.store.PropertyStore;
	using PropertyType = Neo4Net.Kernel.impl.store.PropertyType;
	using IdSequence = Neo4Net.Kernel.impl.store.id.IdSequence;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using PrimitiveRecord = Neo4Net.Kernel.Impl.Store.Records.PrimitiveRecord;
	using PropertyBlock = Neo4Net.Kernel.Impl.Store.Records.PropertyBlock;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;
	using Neo4Net.Kernel.impl.transaction.state;
	using Neo4Net.Kernel.impl.transaction.state;
	using Value = Neo4Net.Values.Storable.Value;

	public class PropertyCreator
	{
		 private readonly DynamicRecordAllocator _stringRecordAllocator;
		 private readonly DynamicRecordAllocator _arrayRecordAllocator;
		 private readonly IdSequence _propertyRecordIdGenerator;
		 private readonly PropertyTraverser _traverser;
		 private readonly bool _allowStorePointsAndTemporal;

		 public PropertyCreator( PropertyStore propertyStore, PropertyTraverser traverser ) : this( propertyStore.StringStore, propertyStore.ArrayStore, propertyStore, traverser, propertyStore.AllowStorePointsAndTemporal() )
		 {
		 }

		 internal PropertyCreator( DynamicRecordAllocator stringRecordAllocator, DynamicRecordAllocator arrayRecordAllocator, IdSequence propertyRecordIdGenerator, PropertyTraverser traverser, bool allowStorePointsAndTemporal )
		 {
			  this._stringRecordAllocator = stringRecordAllocator;
			  this._arrayRecordAllocator = arrayRecordAllocator;
			  this._propertyRecordIdGenerator = propertyRecordIdGenerator;
			  this._traverser = traverser;
			  this._allowStorePointsAndTemporal = allowStorePointsAndTemporal;
		 }

		 public virtual void PrimitiveSetProperty<P>( RecordAccess_RecordProxy<P, Void> primitiveRecordChange, int propertyKey, Value value, RecordAccess<PropertyRecord, PrimitiveRecord> propertyRecords ) where P : Neo4Net.Kernel.Impl.Store.Records.PrimitiveRecord
		 {
			  PropertyBlock block = EncodePropertyValue( propertyKey, value );
			  P primitive = primitiveRecordChange.ForReadingLinkage();
			  Debug.Assert( _traverser.assertPropertyChain( primitive, propertyRecords ) );
			  int newBlockSizeInBytes = block.Size;

			  // Traverse the existing property chain. Tracking two things along the way:
			  // - (a) Free space for this block (candidateHost)
			  // - (b) Existence of a block with the property key
			  // Chain traversal can be aborted only if:
			  // - (1) (b) occurs and new property block fits where the current is
			  // - (2) (a) occurs and (b) has occurred, but new property block didn't fit
			  // - (3) (b) occurs and (a) has occurred
			  // - (4) Chain ends
			  RecordAccess_RecordProxy<PropertyRecord, PrimitiveRecord> freeHostProxy = null;
			  RecordAccess_RecordProxy<PropertyRecord, PrimitiveRecord> existingHostProxy = null;
			  long prop = primitive.NextProp;
			  while ( prop != Record.NO_NEXT_PROPERTY.intValue() ) // <-- (4)
			  {
					RecordAccess_RecordProxy<PropertyRecord, PrimitiveRecord> proxy = propertyRecords.GetOrLoad( prop, primitive );
					PropertyRecord propRecord = proxy.ForReadingLinkage();
					Debug.Assert( propRecord.InUse(), propRecord );

					// (a) search for free space
					if ( PropertyFitsInside( newBlockSizeInBytes, propRecord ) )
					{
						 freeHostProxy = proxy;
						 if ( existingHostProxy != null )
						 {
							  // (2)
							  PropertyRecord freeHost = proxy.ForChangingData();
							  freeHost.AddPropertyBlock( block );
							  freeHost.Changed = primitive;
							  Debug.Assert( _traverser.assertPropertyChain( primitive, propertyRecords ) );
							  return;
						 }
					}

					// (b) search for existence of property key
					PropertyBlock existingBlock = propRecord.GetPropertyBlock( propertyKey );
					if ( existingBlock != null )
					{
						 // We found an existing property and whatever happens we have to remove the existing
						 // block so that we can add the new one, where ever we decide to place it
						 existingHostProxy = proxy;
						 PropertyRecord existingHost = existingHostProxy.ForChangingData();
						 RemoveProperty( primitive, existingHost, existingBlock );

						 // Now see if we at this point can add the new block
						 if ( newBlockSizeInBytes <= existingBlock.Size || PropertyFitsInside( newBlockSizeInBytes, existingHost ) ) // fallback check
						 {
							  // (1) yes we could add it right into the host of the existing block
							  existingHost.AddPropertyBlock( block );
							  Debug.Assert( _traverser.assertPropertyChain( primitive, propertyRecords ) );
							  return;
						 }
						 else if ( freeHostProxy != null )
						 {
							  // (3) yes we could add it to a previously found host with sufficiently free space in it
							  PropertyRecord freeHost = freeHostProxy.ForChangingData();
							  freeHost.AddPropertyBlock( block );
							  freeHost.Changed = primitive;
							  Debug.Assert( _traverser.assertPropertyChain( primitive, propertyRecords ) );
							  return;
						 }
						 // else we can't add it at this point
					}

					// Continue down the chain
					prop = propRecord.NextProp;
			  }

			  // At this point we haven't added the property block, although we may have found room for it
			  // along the way. If we didn't then just create a new record, it's fine
			  PropertyRecord freeHost;
			  if ( freeHostProxy == null )
			  {
					// We couldn't find free space along the way, so create a new host record
					freeHost = propertyRecords.Create( _propertyRecordIdGenerator.nextId(), primitive ).forChangingData();
					freeHost.InUse = true;
					if ( primitive.NextProp != Record.NO_NEXT_PROPERTY.intValue() )
					{
						 // This isn't the first property record for the entity, re-shuffle the first one so that
						 // the new one becomes the first
						 PropertyRecord prevProp = propertyRecords.GetOrLoad( primitive.NextProp, primitive ).forChangingLinkage();
						 Debug.Assert( prevProp.PrevProp == Record.NO_PREVIOUS_PROPERTY.intValue() );
						 prevProp.PrevProp = freeHost.Id;
						 freeHost.NextProp = prevProp.Id;
						 prevProp.Changed = primitive;
					}

					// By the way, this is the only condition where the primitive record also needs to change
					primitiveRecordChange.ForChangingLinkage().NextProp = freeHost.Id;
			  }
			  else
			  {
					freeHost = freeHostProxy.ForChangingData();
			  }

			  // At this point we know that we have a host record with sufficient space in it for the block
			  // to add, so simply add it
			  freeHost.AddPropertyBlock( block );
			  Debug.Assert( _traverser.assertPropertyChain( primitive, propertyRecords ) );
		 }

		 private void RemoveProperty( PrimitiveRecord primitive, PropertyRecord host, PropertyBlock block )
		 {
			  host.RemovePropertyBlock( block.KeyIndexId );
			  host.Changed = primitive;
			  foreach ( DynamicRecord record in block.ValueRecords )
			  {
					Debug.Assert( record.InUse() );
					record.SetInUse( false, block.Type.intValue() );
					host.AddDeletedRecord( record );
			  }
		 }

		 private bool PropertyFitsInside( int newBlockSizeInBytes, PropertyRecord propRecord )
		 {
			  int propSize = propRecord.Size();
			  Debug.Assert( propSize >= 0, propRecord );
			  return propSize + newBlockSizeInBytes <= PropertyType.PayloadSize;
		 }

		 public virtual PropertyBlock EncodePropertyValue( int propertyKey, Value value )
		 {
			  return EncodeValue( new PropertyBlock(), propertyKey, value );
		 }

		 public virtual PropertyBlock EncodeValue( PropertyBlock block, int propertyKey, Value value )
		 {
			  PropertyStore.encodeValue( block, propertyKey, value, _stringRecordAllocator, _arrayRecordAllocator, _allowStorePointsAndTemporal );
			  return block;
		 }

		 public virtual long CreatePropertyChain( PrimitiveRecord owner, IEnumerator<PropertyBlock> properties, RecordAccess<PropertyRecord, PrimitiveRecord> propertyRecords )
		 {
			  return CreatePropertyChain(owner, properties, propertyRecords, p =>
			  {
			  });
		 }

		 private long CreatePropertyChain( PrimitiveRecord owner, IEnumerator<PropertyBlock> properties, RecordAccess<PropertyRecord, PrimitiveRecord> propertyRecords, System.Action<PropertyRecord> createdPropertyRecords )
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  if ( properties == null || !properties.hasNext() )
			  {
					return Record.NO_NEXT_PROPERTY.intValue();
			  }
			  PropertyRecord currentRecord = propertyRecords.Create( _propertyRecordIdGenerator.nextId(), owner ).forChangingData();
			  createdPropertyRecords( currentRecord );
			  currentRecord.InUse = true;
			  currentRecord.SetCreated();
			  PropertyRecord firstRecord = currentRecord;
			  while ( properties.MoveNext() )
			  {
					PropertyBlock block = properties.Current;
					if ( currentRecord.Size() + block.Size > PropertyType.PayloadSize )
					{
						 // Here it means the current block is done for
						 PropertyRecord prevRecord = currentRecord;
						 // Create new record
						 long propertyId = _propertyRecordIdGenerator.nextId();
						 currentRecord = propertyRecords.Create( propertyId, owner ).forChangingData();
						 createdPropertyRecords( currentRecord );
						 currentRecord.InUse = true;
						 currentRecord.SetCreated();
						 // Set up links
						 prevRecord.NextProp = propertyId;
						 currentRecord.PrevProp = prevRecord.Id;
						 // Now current is ready to start picking up blocks
					}
					currentRecord.AddPropertyBlock( block );
			  }
			  return firstRecord.Id;
		 }
	}

}