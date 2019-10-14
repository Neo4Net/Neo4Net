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

	using PropertyType = Neo4Net.Kernel.impl.store.PropertyType;
	using PrimitiveRecord = Neo4Net.Kernel.Impl.Store.Records.PrimitiveRecord;
	using PropertyBlock = Neo4Net.Kernel.Impl.Store.Records.PropertyBlock;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;
	using Neo4Net.Kernel.impl.transaction.state;
	using Neo4Net.Kernel.impl.util;

	public class PropertyTraverser
	{
		 /// <summary>
		 /// Traverses a property record chain and finds the record containing the property with key {@code propertyKey}.
		 /// If none is found and {@code strict} is {@code true} then <seealso cref="System.InvalidOperationException"/> is thrown,
		 /// otherwise id value of <seealso cref="Record.NO_NEXT_PROPERTY"/> is returned.
		 /// </summary>
		 /// <param name="primitive"> <seealso cref="PrimitiveRecord"/> which is the owner of the chain. </param>
		 /// <param name="propertyKey"> property key token id to look for. </param>
		 /// <param name="propertyRecords"> access to records. </param>
		 /// <param name="strict"> dictates behavior on property key not found. If {@code true} then <seealso cref="System.InvalidOperationException"/>
		 /// is thrown, otherwise value of <seealso cref="Record.NO_NEXT_PROPERTY"/> is returned. </param>
		 /// <returns> property record id containing property with the given {@code propertyKey}, otherwise if
		 /// {@code strict} is false value of <seealso cref="Record.NO_NEXT_PROPERTY"/>. </returns>
		 public virtual long FindPropertyRecordContaining( PrimitiveRecord primitive, int propertyKey, RecordAccess<PropertyRecord, PrimitiveRecord> propertyRecords, bool strict )
		 {
			  long propertyRecordId = primitive.NextProp;
			  while ( !Record.NO_NEXT_PROPERTY.@is( propertyRecordId ) )
			  {
					PropertyRecord propertyRecord = propertyRecords.GetOrLoad( propertyRecordId, primitive ).forReadingLinkage();
					if ( propertyRecord.GetPropertyBlock( propertyKey ) != null )
					{
						 return propertyRecordId;
					}
					propertyRecordId = propertyRecord.NextProp;
			  }

			  if ( strict )
			  {
					throw new System.InvalidOperationException( "No property record in property chain for " + primitive + " contained property with key " + propertyKey );
			  }

			  return Record.NO_NEXT_PROPERTY.intValue();
		 }

		 public virtual void GetPropertyChain( long nextProp, RecordAccess<PropertyRecord, PrimitiveRecord> propertyRecords, Listener<PropertyBlock> collector )
		 {
			  while ( nextProp != Record.NO_NEXT_PROPERTY.intValue() )
			  {
					PropertyRecord propRecord = propertyRecords.GetOrLoad( nextProp, null ).forReadingData();
					foreach ( PropertyBlock propBlock in propRecord )
					{
						 collector.Receive( propBlock );
					}
					nextProp = propRecord.NextProp;
			  }
		 }

		 public virtual bool AssertPropertyChain( PrimitiveRecord primitive, RecordAccess<PropertyRecord, PrimitiveRecord> propertyRecords )
		 {
			  IList<PropertyRecord> toCheck = new LinkedList<PropertyRecord>();
			  long nextIdToFetch = primitive.NextProp;
			  while ( nextIdToFetch != Record.NO_NEXT_PROPERTY.intValue() )
			  {
					PropertyRecord propRecord = propertyRecords.GetOrLoad( nextIdToFetch, primitive ).forReadingLinkage();
					toCheck.Add( propRecord );
					Debug.Assert( propRecord.InUse(), primitive + "->" );
														 + Arrays.ToString( toCheck.ToArray() );
					Debug.Assert( propRecord.Size() <= PropertyType.PayloadSize, propRecord + " size " + propRecord.Size() );
					nextIdToFetch = propRecord.NextProp;
			  }
			  if ( toCheck.Count == 0 )
			  {
					Debug.Assert( primitive.NextProp == Record.NO_NEXT_PROPERTY.intValue(), primitive );
					return true;
			  }
			  PropertyRecord first = toCheck[0];
			  PropertyRecord last = toCheck[toCheck.Count - 1];
			  Debug.Assert( first.PrevProp == Record.NO_PREVIOUS_PROPERTY.intValue(), primitive + "->" );
																											 + Arrays.ToString( toCheck.ToArray() );
			  Debug.Assert( last.NextProp == Record.NO_NEXT_PROPERTY.intValue(), primitive + "->" );
																									  + Arrays.ToString( toCheck.ToArray() );
			  PropertyRecord current;
			  PropertyRecord previous = first;
			  for ( int i = 1; i < toCheck.Count; i++ )
			  {
					current = toCheck[i];
					Debug.Assert( current.PrevProp == previous.Id, primitive + "->" );
																						+ Arrays.ToString( toCheck.ToArray() );
					Debug.Assert( previous.NextProp == current.Id, primitive + "->" );
																						+ Arrays.ToString( toCheck.ToArray() );
					previous = current;
			  }
			  return true;
		 }
	}

}