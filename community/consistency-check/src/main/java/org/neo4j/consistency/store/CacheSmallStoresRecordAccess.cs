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
namespace Org.Neo4j.Consistency.store
{
	using LabelTokenRecord = Org.Neo4j.Kernel.impl.store.record.LabelTokenRecord;
	using PropertyKeyTokenRecord = Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using RelationshipTypeTokenRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipTypeTokenRecord;

	public class CacheSmallStoresRecordAccess : DelegatingRecordAccess
	{
		 private readonly PropertyKeyTokenRecord[] _propertyKeys;
		 private readonly RelationshipTypeTokenRecord[] _relationshipTypes;
		 private readonly LabelTokenRecord[] _labels;

		 public CacheSmallStoresRecordAccess( RecordAccess @delegate, PropertyKeyTokenRecord[] propertyKeys, RelationshipTypeTokenRecord[] relationshipTypes, LabelTokenRecord[] labels ) : base( @delegate )
		 {
			  this._propertyKeys = propertyKeys;
			  this._relationshipTypes = relationshipTypes;
			  this._labels = labels;
		 }

		 public override RecordReference<RelationshipTypeTokenRecord> RelationshipType( int id )
		 {
			  if ( id < _relationshipTypes.Length )
			  {
					return new DirectRecordReference<RelationshipTypeTokenRecord>( _relationshipTypes[id], this );
			  }
			  else
			  {
					return base.RelationshipType( id );
			  }
		 }

		 public override RecordReference<PropertyKeyTokenRecord> PropertyKey( int id )
		 {
			  if ( id < _propertyKeys.Length )
			  {
					return new DirectRecordReference<PropertyKeyTokenRecord>( _propertyKeys[id], this );
			  }
			  else
			  {
					return base.PropertyKey( id );
			  }
		 }

		 public override RecordReference<LabelTokenRecord> Label( int id )
		 {
			  if ( id < _labels.Length )
			  {
					return new DirectRecordReference<LabelTokenRecord>( _labels[id], this );
			  }
			  else
			  {
					return base.Label( id );
			  }
		 }
	}

}