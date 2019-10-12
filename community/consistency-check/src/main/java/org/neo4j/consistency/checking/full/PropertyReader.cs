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
namespace Org.Neo4j.Consistency.checking.full
{
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;


	using Org.Neo4j.Helpers.Collection;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using NodeStore = Org.Neo4j.Kernel.impl.store.NodeStore;
	using PropertyStore = Org.Neo4j.Kernel.impl.store.PropertyStore;
	using StoreAccess = Org.Neo4j.Kernel.impl.store.StoreAccess;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PropertyBlock = Org.Neo4j.Kernel.impl.store.record.PropertyBlock;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using Record = Org.Neo4j.Kernel.impl.store.record.Record;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.FORCE;

	internal class PropertyReader : NodePropertyAccessor
	{
		 private readonly PropertyStore _propertyStore;
		 private readonly NodeStore _nodeStore;

		 internal PropertyReader( StoreAccess storeAccess )
		 {
			  this._propertyStore = storeAccess.RawNeoStores.PropertyStore;
			  this._nodeStore = storeAccess.RawNeoStores.NodeStore;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: java.util.Collection<org.neo4j.kernel.impl.store.record.PropertyRecord> getPropertyRecordChain(long firstPropertyRecordId) throws CircularPropertyRecordChainException
		 internal virtual ICollection<PropertyRecord> GetPropertyRecordChain( long firstPropertyRecordId )
		 {
			  IList<PropertyRecord> records = new List<PropertyRecord>();
			  VisitPropertyRecordChain(firstPropertyRecordId, record =>
			  {
				records.Add( record );
				return false; // please continue
			  });
			  return records;
		 }

		 internal virtual IList<PropertyBlock> PropertyBlocks( ICollection<PropertyRecord> records )
		 {
			  IList<PropertyBlock> propertyBlocks = new List<PropertyBlock>();
			  foreach ( PropertyRecord record in records )
			  {
					foreach ( PropertyBlock block in record )
					{
						 propertyBlocks.Add( block );
					}
			  }
			  return propertyBlocks;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean visitPropertyRecordChain(long firstPropertyRecordId, org.neo4j.helpers.collection.Visitor<org.neo4j.kernel.impl.store.record.PropertyRecord,RuntimeException> visitor) throws CircularPropertyRecordChainException
		 private bool VisitPropertyRecordChain( long firstPropertyRecordId, Visitor<PropertyRecord, Exception> visitor )
		 {
			  if ( Record.NO_NEXT_PROPERTY.@is( firstPropertyRecordId ) )
			  {
					return false;
			  }

			  MutableLongSet visitedPropertyRecordIds = new LongHashSet( 8 );
			  visitedPropertyRecordIds.add( firstPropertyRecordId );
			  long nextProp = firstPropertyRecordId;
			  while ( !Record.NO_NEXT_PROPERTY.@is( nextProp ) )
			  {
					PropertyRecord propRecord = _propertyStore.getRecord( nextProp, _propertyStore.newRecord(), FORCE );
					nextProp = propRecord.NextProp;
					if ( !Record.NO_NEXT_PROPERTY.@is( nextProp ) && !visitedPropertyRecordIds.add( nextProp ) )
					{
						 throw new CircularPropertyRecordChainException( propRecord );
					}
					if ( visitor.Visit( propRecord ) )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 public virtual Value PropertyValue( PropertyBlock block )
		 {
			  return block.Type.value( block, _propertyStore );
		 }

		 public override Value GetNodePropertyValue( long nodeId, int propertyKeyId )
		 {
			  NodeRecord nodeRecord = _nodeStore.newRecord();
			  if ( _nodeStore.getRecord( nodeId, nodeRecord, FORCE ).inUse() )
			  {
					SpecificValueVisitor visitor = new SpecificValueVisitor( this, propertyKeyId );
					try
					{
						 if ( VisitPropertyRecordChain( nodeRecord.NextProp, visitor ) )
						 {
							  return visitor.FoundPropertyValue;
						 }
					}
					catch ( CircularPropertyRecordChainException )
					{
						 // If we discover a circular reference and still haven't found the property then we won't find it.
						 // There are other places where this circular reference will be logged as an inconsistency,
						 // so simply catch this exception here and let this method return NO_VALUE below.
					}
			  }
			  return Values.NO_VALUE;
		 }

		 private class SpecificValueVisitor : Visitor<PropertyRecord, Exception>
		 {
			 private readonly PropertyReader _outerInstance;

			  internal readonly int PropertyKeyId;
			  internal Value FoundPropertyValue;

			  internal SpecificValueVisitor( PropertyReader outerInstance, int propertyKeyId )
			  {
				  this._outerInstance = outerInstance;
					this.PropertyKeyId = propertyKeyId;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visit(org.neo4j.kernel.impl.store.record.PropertyRecord element) throws RuntimeException
			  public override bool Visit( PropertyRecord element )
			  {
					foreach ( PropertyBlock block in element )
					{
						 if ( block.KeyIndexId == PropertyKeyId )
						 {
							  FoundPropertyValue = outerInstance.PropertyValue( block );
							  return true;
						 }
					}
					return false;
			  }
		 }

		 internal class CircularPropertyRecordChainException : Exception
		 {
			  internal readonly PropertyRecord PropertyRecord;

			  internal CircularPropertyRecordChainException( PropertyRecord propertyRecord )
			  {
					this.PropertyRecord = propertyRecord;
			  }

			  internal virtual PropertyRecord PropertyRecordClosingTheCircle()
			  {
					return PropertyRecord;
			  }
		 }
	}

}