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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{
	using DynamicRecord = Neo4Net.Kernel.impl.store.record.DynamicRecord;
	using PrimitiveRecord = Neo4Net.Kernel.impl.store.record.PrimitiveRecord;
	using PropertyBlock = Neo4Net.Kernel.impl.store.record.PropertyBlock;
	using PropertyRecord = Neo4Net.Kernel.impl.store.record.PropertyRecord;
	using Record = Neo4Net.Kernel.impl.store.record.Record;
	using Neo4Net.Kernel.impl.transaction.state;
	using Neo4Net.Kernel.impl.transaction.state;

	public class PropertyDeleter
	{
		 private readonly PropertyTraverser _traverser;

		 public PropertyDeleter( PropertyTraverser traverser )
		 {
			  this._traverser = traverser;
		 }

		 public virtual void DeletePropertyChain( PrimitiveRecord primitive, RecordAccess<PropertyRecord, PrimitiveRecord> propertyRecords )
		 {
			  long nextProp = primitive.NextProp;
			  while ( nextProp != Record.NO_NEXT_PROPERTY.intValue() )
			  {
					RecordAccess_RecordProxy<PropertyRecord, PrimitiveRecord> propertyChange = propertyRecords.GetOrLoad( nextProp, primitive );

					// TODO forChanging/forReading piggy-backing
					PropertyRecord propRecord = propertyChange.ForChangingData();
					DeletePropertyRecordIncludingValueRecords( propRecord );
					nextProp = propRecord.NextProp;
					propRecord.Changed = primitive;
			  }
			  primitive.NextProp = Record.NO_NEXT_PROPERTY.intValue();
		 }

		 public static void DeletePropertyRecordIncludingValueRecords( PropertyRecord record )
		 {
			  foreach ( PropertyBlock block in record )
			  {
					foreach ( DynamicRecord valueRecord in block.ValueRecords )
					{
						 Debug.Assert( valueRecord.InUse() );
						 valueRecord.InUse = false;
						 record.AddDeletedRecord( valueRecord );
					}
			  }
			  record.ClearPropertyBlocks();
			  record.InUse = false;
		 }

		 /// <summary>
		 /// Removes property with given {@code propertyKey} from property chain owner by the primitive found in
		 /// {@code primitiveProxy} if it exists.
		 /// </summary>
		 /// <param name="primitiveProxy"> access to the primitive record pointing to the start of the property chain. </param>
		 /// <param name="propertyKey"> the property key token id to look for and remove. </param>
		 /// <param name="propertyRecords"> access to records. </param>
		 /// <returns> {@code true} if the property was found and removed, otherwise {@code false}. </returns>
		 public virtual bool RemovePropertyIfExists<P>( RecordAccess_RecordProxy<P, Void> primitiveProxy, int propertyKey, RecordAccess<PropertyRecord, PrimitiveRecord> propertyRecords ) where P : Neo4Net.Kernel.impl.store.record.PrimitiveRecord
		 {
			  PrimitiveRecord primitive = primitiveProxy.ForReadingData();
			  long propertyId = _traverser.findPropertyRecordContaining( primitive, propertyKey, propertyRecords, false );
			  if ( !Record.NO_NEXT_PROPERTY.@is( propertyId ) )
			  {
					RemoveProperty( primitiveProxy, propertyKey, propertyRecords, primitive, propertyId );
					return true;
			  }
			  return false;
		 }

		 /// <summary>
		 /// Removes property with given {@code propertyKey} from property chain owner by the primitive found in
		 /// {@code primitiveProxy}.
		 /// </summary>
		 /// <param name="primitiveProxy"> access to the primitive record pointing to the start of the property chain. </param>
		 /// <param name="propertyKey"> the property key token id to look for and remove. </param>
		 /// <param name="propertyRecords"> access to records. </param>
		 /// <exception cref="IllegalStateException"> if property key was not found in the property chain. </exception>
		 public virtual void RemoveProperty<P>( RecordAccess_RecordProxy<P, Void> primitiveProxy, int propertyKey, RecordAccess<PropertyRecord, PrimitiveRecord> propertyRecords ) where P : Neo4Net.Kernel.impl.store.record.PrimitiveRecord
		 {
			  PrimitiveRecord primitive = primitiveProxy.ForReadingData();
			  long propertyId = _traverser.findPropertyRecordContaining( primitive, propertyKey, propertyRecords, true );
			  RemoveProperty( primitiveProxy, propertyKey, propertyRecords, primitive, propertyId );
		 }

		 private void RemoveProperty<P>( RecordAccess_RecordProxy<P, Void> primitiveProxy, int propertyKey, RecordAccess<PropertyRecord, PrimitiveRecord> propertyRecords, PrimitiveRecord primitive, long propertyId ) where P : Neo4Net.Kernel.impl.store.record.PrimitiveRecord
		 {
			  RecordAccess_RecordProxy<PropertyRecord, PrimitiveRecord> recordChange = propertyRecords.GetOrLoad( propertyId, primitive );
			  PropertyRecord propRecord = recordChange.ForChangingData();
			  if ( !propRecord.InUse() )
			  {
					throw new System.InvalidOperationException( "Unable to delete property[" + propertyId + "] since it is already deleted." );
			  }

			  PropertyBlock block = propRecord.RemovePropertyBlock( propertyKey );
			  if ( block == null )
			  {
					throw new System.InvalidOperationException( "Property with index[" + propertyKey + "] is not present in property[" + propertyId + "]" );
			  }

			  foreach ( DynamicRecord valueRecord in block.ValueRecords )
			  {
					Debug.Assert( valueRecord.InUse() );
					valueRecord.SetInUse( false, block.Type.intValue() );
					propRecord.AddDeletedRecord( valueRecord );
			  }
			  if ( propRecord.Size() > 0 )
			  {
					/*
					 * There are remaining blocks in the record. We do not unlink yet.
					 */
					propRecord.Changed = primitive;
					Debug.Assert( _traverser.assertPropertyChain( primitive, propertyRecords ) );
			  }
			  else
			  {
					UnlinkPropertyRecord( propRecord, propertyRecords, primitiveProxy );
			  }
		 }

		 private void UnlinkPropertyRecord<P>( PropertyRecord propRecord, RecordAccess<PropertyRecord, PrimitiveRecord> propertyRecords, RecordAccess_RecordProxy<P, Void> primitiveRecordChange ) where P : Neo4Net.Kernel.impl.store.record.PrimitiveRecord
		 {
			  P primitive = primitiveRecordChange.ForReadingLinkage();
			  Debug.Assert( _traverser.assertPropertyChain( primitive, propertyRecords ) );
			  Debug.Assert( propRecord.Size() == 0 );
			  long prevProp = propRecord.PrevProp;
			  long nextProp = propRecord.NextProp;
			  if ( primitive.NextProp == propRecord.Id )
			  {
					Debug.Assert( propRecord.PrevProp == Record.NO_PREVIOUS_PROPERTY.intValue(), propRecord + " for " );
							  + primitive;
					primitiveRecordChange.ForChangingLinkage().NextProp = nextProp;
			  }
			  if ( prevProp != Record.NO_PREVIOUS_PROPERTY.intValue() )
			  {
					PropertyRecord prevPropRecord = propertyRecords.GetOrLoad( prevProp, primitive ).forChangingLinkage();
					Debug.Assert( prevPropRecord.InUse(), prevPropRecord + "->" + propRecord + " for " + primitive );
					prevPropRecord.NextProp = nextProp;
					prevPropRecord.Changed = primitive;
			  }
			  if ( nextProp != Record.NO_NEXT_PROPERTY.intValue() )
			  {
					PropertyRecord nextPropRecord = propertyRecords.GetOrLoad( nextProp, primitive ).forChangingLinkage();
					Debug.Assert( nextPropRecord.InUse(), propRecord + "->" + nextPropRecord + " for " + primitive );
					nextPropRecord.PrevProp = prevProp;
					nextPropRecord.Changed = primitive;
			  }
			  propRecord.InUse = false;
			  /*
			   *  The following two are not needed - the above line does all the work (PropertyStore
			   *  does not write out the prev/next for !inUse records). It is nice to set this
			   *  however to check for consistency when assertPropertyChain().
			   */
			  propRecord.PrevProp = Record.NO_PREVIOUS_PROPERTY.intValue();
			  propRecord.NextProp = Record.NO_NEXT_PROPERTY.intValue();
			  propRecord.Changed = primitive;
			  Debug.Assert( _traverser.assertPropertyChain( primitive, propertyRecords ) );
		 }
	}

}