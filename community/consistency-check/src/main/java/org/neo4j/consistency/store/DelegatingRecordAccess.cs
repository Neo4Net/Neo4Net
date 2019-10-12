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
namespace Org.Neo4j.Consistency.store
{

	using CacheAccess = Org.Neo4j.Consistency.checking.cache.CacheAccess;
	using MultiPassStore = Org.Neo4j.Consistency.checking.full.MultiPassStore;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using LabelTokenRecord = Org.Neo4j.Kernel.impl.store.record.LabelTokenRecord;
	using NeoStoreRecord = Org.Neo4j.Kernel.impl.store.record.NeoStoreRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PropertyKeyTokenRecord = Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using RelationshipGroupRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using RelationshipTypeTokenRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipTypeTokenRecord;

	public class DelegatingRecordAccess : RecordAccess
	{
		 private readonly RecordAccess @delegate;

		 public DelegatingRecordAccess( RecordAccess @delegate )
		 {
			  this.@delegate = @delegate;
		 }

		 public override RecordReference<DynamicRecord> Schema( long id )
		 {
			  return @delegate.Schema( id );
		 }

		 public override RecordReference<NodeRecord> Node( long id )
		 {
			  return @delegate.Node( id );
		 }

		 public override RecordReference<RelationshipRecord> Relationship( long id )
		 {
			  return @delegate.Relationship( id );
		 }

		 public override RecordReference<PropertyRecord> Property( long id )
		 {
			  return @delegate.Property( id );
		 }

		 public override IEnumerator<PropertyRecord> RawPropertyChain( long firstId )
		 {
			  return @delegate.RawPropertyChain( firstId );
		 }

		 public override RecordReference<RelationshipTypeTokenRecord> RelationshipType( int id )
		 {
			  return @delegate.RelationshipType( id );
		 }

		 public override RecordReference<PropertyKeyTokenRecord> PropertyKey( int id )
		 {
			  return @delegate.PropertyKey( id );
		 }

		 public override RecordReference<DynamicRecord> String( long id )
		 {
			  return @delegate.String( id );
		 }

		 public override RecordReference<DynamicRecord> Array( long id )
		 {
			  return @delegate.Array( id );
		 }

		 public override RecordReference<DynamicRecord> RelationshipTypeName( int id )
		 {
			  return @delegate.RelationshipTypeName( id );
		 }

		 public override RecordReference<DynamicRecord> NodeLabels( long id )
		 {
			  return @delegate.NodeLabels( id );
		 }

		 public override RecordReference<LabelTokenRecord> Label( int id )
		 {
			  return @delegate.Label( id );
		 }

		 public override RecordReference<DynamicRecord> LabelName( int id )
		 {
			  return @delegate.LabelName( id );
		 }

		 public override RecordReference<DynamicRecord> PropertyKeyName( int id )
		 {
			  return @delegate.PropertyKeyName( id );
		 }

		 public override RecordReference<NeoStoreRecord> Graph()
		 {
			  return @delegate.Graph();
		 }

		 public override RecordReference<RelationshipGroupRecord> RelationshipGroup( long id )
		 {
			  return @delegate.RelationshipGroup( id );
		 }

		 public override bool ShouldCheck( long id, MultiPassStore store )
		 {
			  return @delegate.ShouldCheck( id, store );
		 }

		 public override CacheAccess CacheAccess()
		 {
			  return @delegate.CacheAccess();
		 }
	}

}