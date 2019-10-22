using System.Collections.Generic;

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
namespace Neo4Net.Consistency.store
{

	using CacheAccess = Neo4Net.Consistency.checking.cache.CacheAccess;
	using MultiPassStore = Neo4Net.Consistency.checking.full.MultiPassStore;
	using Neo4Net.Helpers.Collections;
	using Neo4Net.Kernel.impl.store;
	using StoreAccess = Neo4Net.Kernel.impl.store.StoreAccess;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using LabelTokenRecord = Neo4Net.Kernel.Impl.Store.Records.LabelTokenRecord;
	using NeoStoreRecord = Neo4Net.Kernel.Impl.Store.Records.NeoStoreRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyKeyTokenRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipTypeTokenRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.store.record.RecordLoad.FORCE;

	public class DirectRecordAccess : RecordAccess
	{
		 internal readonly StoreAccess Access;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 internal readonly CacheAccess CacheAccessConflict;

		 public DirectRecordAccess( StoreAccess access, CacheAccess cacheAccess )
		 {
			  this.Access = access;
			  this.CacheAccessConflict = cacheAccess;
		 }

		 public override RecordReference<DynamicRecord> Schema( long id )
		 {
			  return ReferenceTo( Access.SchemaStore, id );
		 }

		 public override RecordReference<NodeRecord> Node( long id )
		 {
			  return ReferenceTo( Access.NodeStore, id );
		 }

		 public override RecordReference<RelationshipRecord> Relationship( long id )
		 {
			  return ReferenceTo( Access.RelationshipStore, id );
		 }

		 public override RecordReference<RelationshipGroupRecord> RelationshipGroup( long id )
		 {
			  return ReferenceTo( Access.RelationshipGroupStore, id );
		 }

		 public override RecordReference<PropertyRecord> Property( long id )
		 {
			  return ReferenceTo( Access.PropertyStore, id );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public java.util.Iterator<org.Neo4Net.kernel.impl.store.record.PropertyRecord> rawPropertyChain(final long firstId)
		 public override IEnumerator<PropertyRecord> RawPropertyChain( long firstId )
		 {
			  return new PrefetchingIteratorAnonymousInnerClass( this, firstId );
		 }

		 private class PrefetchingIteratorAnonymousInnerClass : PrefetchingIterator<PropertyRecord>
		 {
			 private readonly DirectRecordAccess _outerInstance;

			 private long _firstId;

			 public PrefetchingIteratorAnonymousInnerClass( DirectRecordAccess outerInstance, long firstId )
			 {
				 this.outerInstance = outerInstance;
				 this._firstId = firstId;
				 next = firstId;
			 }

			 private long next;

			 protected internal override PropertyRecord fetchNextOrNull()
			 {
				  if ( Record.NO_NEXT_PROPERTY.@is( next ) )
				  {
						return null;
				  }

				  PropertyRecord record = _outerInstance.referenceTo( _outerInstance.access.PropertyStore, next ).record();
				  next = record.NextProp;
				  return record;
			 }
		 }

		 public override RecordReference<RelationshipTypeTokenRecord> RelationshipType( int id )
		 {
			  return ReferenceTo( Access.RelationshipTypeTokenStore, id );
		 }

		 public override RecordReference<PropertyKeyTokenRecord> PropertyKey( int id )
		 {
			  return ReferenceTo( Access.PropertyKeyTokenStore, id );
		 }

		 public override RecordReference<DynamicRecord> String( long id )
		 {
			  return ReferenceTo( Access.StringStore, id );
		 }

		 public override RecordReference<DynamicRecord> Array( long id )
		 {
			  return ReferenceTo( Access.ArrayStore, id );
		 }

		 public override RecordReference<DynamicRecord> RelationshipTypeName( int id )
		 {
			  return ReferenceTo( Access.RelationshipTypeNameStore, id );
		 }

		 public override RecordReference<DynamicRecord> NodeLabels( long id )
		 {
			  return ReferenceTo( Access.NodeDynamicLabelStore, id );
		 }

		 public override RecordReference<LabelTokenRecord> Label( int id )
		 {
			  return ReferenceTo( Access.LabelTokenStore, id );
		 }

		 public override RecordReference<DynamicRecord> LabelName( int id )
		 {
			  return ReferenceTo( Access.LabelNameStore, id );
		 }

		 public override RecordReference<DynamicRecord> PropertyKeyName( int id )
		 {
			  return ReferenceTo( Access.PropertyKeyNameStore, id );
		 }

		 public override RecordReference<NeoStoreRecord> Graph()
		 {
			  return new DirectRecordReference<NeoStoreRecord>( Access.RawNeoStores.MetaDataStore.graphPropertyRecord(), this );
		 }

		 public override bool ShouldCheck( long id, MultiPassStore store )
		 {
			  return true;
		 }

		 internal virtual DirectRecordReference<RECORD> ReferenceTo<RECORD>( RecordStore<RECORD> store, long id ) where RECORD : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
		 {
			  return new DirectRecordReference<RECORD>( store.GetRecord( id, store.NewRecord(), FORCE ), this );
		 }

		 public override CacheAccess CacheAccess()
		 {
			  return CacheAccessConflict;
		 }
	}

}