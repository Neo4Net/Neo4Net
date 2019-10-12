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
namespace Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage
{
	using NeoStores = Org.Neo4j.Kernel.impl.store.NeoStores;
	using PropertyStore = Org.Neo4j.Kernel.impl.store.PropertyStore;
	using Org.Neo4j.Kernel.impl.store;
	using SchemaStore = Org.Neo4j.Kernel.impl.store.SchemaStore;
	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using LabelTokenRecord = Org.Neo4j.Kernel.impl.store.record.LabelTokenRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PrimitiveRecord = Org.Neo4j.Kernel.impl.store.record.PrimitiveRecord;
	using PropertyBlock = Org.Neo4j.Kernel.impl.store.record.PropertyBlock;
	using PropertyKeyTokenRecord = Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using RecordLoad = Org.Neo4j.Kernel.impl.store.record.RecordLoad;
	using RelationshipGroupRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using RelationshipTypeTokenRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipTypeTokenRecord;
	using SchemaRecord = Org.Neo4j.Kernel.impl.store.record.SchemaRecord;
	using Org.Neo4j.Kernel.impl.transaction.state;
	using SchemaRule = Org.Neo4j.Storageengine.Api.schema.SchemaRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.NORMAL;

	public class Loaders
	{
		 private readonly RecordAccess_Loader<NodeRecord, Void> _nodeLoader;
		 private readonly RecordAccess_Loader<PropertyRecord, PrimitiveRecord> _propertyLoader;
		 private readonly RecordAccess_Loader<RelationshipRecord, Void> _relationshipLoader;
		 private readonly RecordAccess_Loader<RelationshipGroupRecord, int> _relationshipGroupLoader;
		 private readonly RecordAccess_Loader<SchemaRecord, SchemaRule> _schemaRuleLoader;
		 private readonly RecordAccess_Loader<PropertyKeyTokenRecord, Void> _propertyKeyTokenLoader;
		 private readonly RecordAccess_Loader<LabelTokenRecord, Void> _labelTokenLoader;
		 private readonly RecordAccess_Loader<RelationshipTypeTokenRecord, Void> _relationshipTypeTokenLoader;

		 public Loaders( NeoStores neoStores ) : this( neoStores.NodeStore, neoStores.PropertyStore, neoStores.RelationshipStore, neoStores.RelationshipGroupStore, neoStores.PropertyKeyTokenStore, neoStores.RelationshipTypeTokenStore, neoStores.LabelTokenStore, neoStores.SchemaStore )
		 {
		 }

		 public Loaders( RecordStore<NodeRecord> nodeStore, PropertyStore propertyStore, RecordStore<RelationshipRecord> relationshipStore, RecordStore<RelationshipGroupRecord> relationshipGroupStore, RecordStore<PropertyKeyTokenRecord> propertyKeyTokenStore, RecordStore<RelationshipTypeTokenRecord> relationshipTypeTokenStore, RecordStore<LabelTokenRecord> labelTokenStore, SchemaStore schemaStore )
		 {
			  _nodeLoader = _nodeLoader( nodeStore );
			  _propertyLoader = _propertyLoader( propertyStore );
			  _relationshipLoader = _relationshipLoader( relationshipStore );
			  _relationshipGroupLoader = _relationshipGroupLoader( relationshipGroupStore );
			  _schemaRuleLoader = _schemaRuleLoader( schemaStore );
			  _propertyKeyTokenLoader = _propertyKeyTokenLoader( propertyKeyTokenStore );
			  _labelTokenLoader = _labelTokenLoader( labelTokenStore );
			  _relationshipTypeTokenLoader = _relationshipTypeTokenLoader( relationshipTypeTokenStore );
		 }

		 public virtual RecordAccess_Loader<NodeRecord, Void> NodeLoader()
		 {
			  return _nodeLoader;
		 }

		 public virtual RecordAccess_Loader<PropertyRecord, PrimitiveRecord> PropertyLoader()
		 {
			  return _propertyLoader;
		 }

		 public virtual RecordAccess_Loader<RelationshipRecord, Void> RelationshipLoader()
		 {
			  return _relationshipLoader;
		 }

		 public virtual RecordAccess_Loader<RelationshipGroupRecord, int> RelationshipGroupLoader()
		 {
			  return _relationshipGroupLoader;
		 }

		 public virtual RecordAccess_Loader<SchemaRecord, SchemaRule> SchemaRuleLoader()
		 {
			  return _schemaRuleLoader;
		 }

		 public virtual RecordAccess_Loader<PropertyKeyTokenRecord, Void> PropertyKeyTokenLoader()
		 {
			  return _propertyKeyTokenLoader;
		 }

		 public virtual RecordAccess_Loader<LabelTokenRecord, Void> LabelTokenLoader()
		 {
			  return _labelTokenLoader;
		 }

		 public virtual RecordAccess_Loader<RelationshipTypeTokenRecord, Void> RelationshipTypeTokenLoader()
		 {
			  return _relationshipTypeTokenLoader;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.neo4j.kernel.impl.transaction.state.RecordAccess_Loader<org.neo4j.kernel.impl.store.record.NodeRecord,Void> nodeLoader(final org.neo4j.kernel.impl.store.RecordStore<org.neo4j.kernel.impl.store.record.NodeRecord> store)
		 public static RecordAccess_Loader<NodeRecord, Void> NodeLoader( RecordStore<NodeRecord> store )
		 {
			  return new RecordAccess_LoaderAnonymousInnerClass( store );
		 }

		 private class RecordAccess_LoaderAnonymousInnerClass : RecordAccess_Loader<NodeRecord, Void>
		 {
			 private RecordStore<NodeRecord> _store;

			 public RecordAccess_LoaderAnonymousInnerClass( RecordStore<NodeRecord> store )
			 {
				 this._store = store;
			 }

			 public NodeRecord newUnused( long key, Void additionalData )
			 {
				  return AndMarkAsCreated( new NodeRecord( key ) );
			 }

			 public NodeRecord load( long key, Void additionalData )
			 {
				  return _store.getRecord( key, _store.newRecord(), NORMAL );
			 }

			 public void ensureHeavy( NodeRecord record )
			 {
				  _store.ensureHeavy( record );
			 }

			 public NodeRecord clone( NodeRecord nodeRecord )
			 {
				  return nodeRecord.Clone();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.neo4j.kernel.impl.transaction.state.RecordAccess_Loader<org.neo4j.kernel.impl.store.record.PropertyRecord,org.neo4j.kernel.impl.store.record.PrimitiveRecord> propertyLoader(final org.neo4j.kernel.impl.store.PropertyStore store)
		 public static RecordAccess_Loader<PropertyRecord, PrimitiveRecord> PropertyLoader( PropertyStore store )
		 {
			  return new RecordAccess_LoaderAnonymousInnerClass2( store );
		 }

		 private class RecordAccess_LoaderAnonymousInnerClass2 : RecordAccess_Loader<PropertyRecord, PrimitiveRecord>
		 {
			 private PropertyStore _store;

			 public RecordAccess_LoaderAnonymousInnerClass2( PropertyStore store )
			 {
				 this._store = store;
			 }

			 public PropertyRecord newUnused( long key, PrimitiveRecord additionalData )
			 {
				  PropertyRecord record = new PropertyRecord( key );
				  setOwner( record, additionalData );
				  return AndMarkAsCreated( record );
			 }

			 private void setOwner( PropertyRecord record, PrimitiveRecord owner )
			 {
				  if ( owner != null )
				  {
						owner.IdTo = record;
				  }
			 }

			 public PropertyRecord load( long key, PrimitiveRecord additionalData )
			 {
				  PropertyRecord record = _store.getRecord( key, _store.newRecord(), NORMAL );
				  setOwner( record, additionalData );
				  return record;
			 }

			 public void ensureHeavy( PropertyRecord record )
			 {
				  foreach ( PropertyBlock block in record )
				  {
						_store.ensureHeavy( block );
				  }
			 }

			 public PropertyRecord clone( PropertyRecord propertyRecord )
			 {
				  return propertyRecord.Clone();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.neo4j.kernel.impl.transaction.state.RecordAccess_Loader<org.neo4j.kernel.impl.store.record.RelationshipRecord,Void> relationshipLoader(final org.neo4j.kernel.impl.store.RecordStore<org.neo4j.kernel.impl.store.record.RelationshipRecord> store)
		 public static RecordAccess_Loader<RelationshipRecord, Void> RelationshipLoader( RecordStore<RelationshipRecord> store )
		 {
			  return new RecordAccess_LoaderAnonymousInnerClass3( store );
		 }

		 private class RecordAccess_LoaderAnonymousInnerClass3 : RecordAccess_Loader<RelationshipRecord, Void>
		 {
			 private RecordStore<RelationshipRecord> _store;

			 public RecordAccess_LoaderAnonymousInnerClass3( RecordStore<RelationshipRecord> store )
			 {
				 this._store = store;
			 }

			 public RelationshipRecord newUnused( long key, Void additionalData )
			 {
				  return AndMarkAsCreated( new RelationshipRecord( key ) );
			 }

			 public RelationshipRecord load( long key, Void additionalData )
			 {
				  return _store.getRecord( key, _store.newRecord(), NORMAL );
			 }

			 public void ensureHeavy( RelationshipRecord record )
			 { // Nothing to load
			 }

			 public RelationshipRecord clone( RelationshipRecord relationshipRecord )
			 {
				  return relationshipRecord.Clone();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.neo4j.kernel.impl.transaction.state.RecordAccess_Loader<org.neo4j.kernel.impl.store.record.RelationshipGroupRecord,int> relationshipGroupLoader(final org.neo4j.kernel.impl.store.RecordStore<org.neo4j.kernel.impl.store.record.RelationshipGroupRecord> store)
		 public static RecordAccess_Loader<RelationshipGroupRecord, int> RelationshipGroupLoader( RecordStore<RelationshipGroupRecord> store )
		 {
			  return new RecordAccess_LoaderAnonymousInnerClass4( store );
		 }

		 private class RecordAccess_LoaderAnonymousInnerClass4 : RecordAccess_Loader<RelationshipGroupRecord, int>
		 {
			 private RecordStore<RelationshipGroupRecord> _store;

			 public RecordAccess_LoaderAnonymousInnerClass4( RecordStore<RelationshipGroupRecord> store )
			 {
				 this._store = store;
			 }

			 public RelationshipGroupRecord newUnused( long key, int? type )
			 {
				  RelationshipGroupRecord record = new RelationshipGroupRecord( key );
				  record.Type = type.Value;
				  return AndMarkAsCreated( record );
			 }

			 public RelationshipGroupRecord load( long key, int? type )
			 {
				  return _store.getRecord( key, _store.newRecord(), NORMAL );
			 }

			 public void ensureHeavy( RelationshipGroupRecord record )
			 { // Not needed
			 }

			 public RelationshipGroupRecord clone( RelationshipGroupRecord record )
			 {
				  return record.Clone();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.neo4j.kernel.impl.transaction.state.RecordAccess_Loader<org.neo4j.kernel.impl.store.record.SchemaRecord,org.neo4j.storageengine.api.schema.SchemaRule> schemaRuleLoader(final org.neo4j.kernel.impl.store.SchemaStore store)
		 public static RecordAccess_Loader<SchemaRecord, SchemaRule> SchemaRuleLoader( SchemaStore store )
		 {
			  return new RecordAccess_LoaderAnonymousInnerClass5( store );
		 }

		 private class RecordAccess_LoaderAnonymousInnerClass5 : RecordAccess_Loader<SchemaRecord, SchemaRule>
		 {
			 private SchemaStore _store;

			 public RecordAccess_LoaderAnonymousInnerClass5( SchemaStore store )
			 {
				 this._store = store;
			 }

			 public SchemaRecord newUnused( long key, SchemaRule additionalData )
			 {
				  // Don't blindly mark as created here since some records may be reused.
				  return new SchemaRecord( _store.allocateFrom( additionalData ) );
			 }

			 public SchemaRecord load( long key, SchemaRule additionalData )
			 {
				  return new SchemaRecord( _store.getRecords( key, RecordLoad.NORMAL ) );
			 }

			 public void ensureHeavy( SchemaRecord records )
			 {
				  foreach ( DynamicRecord record in records )
				  {
						_store.ensureHeavy( record );
				  }
			 }

			 public SchemaRecord clone( SchemaRecord records )
			 {
				  return records.Clone();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.neo4j.kernel.impl.transaction.state.RecordAccess_Loader<org.neo4j.kernel.impl.store.record.PropertyKeyTokenRecord,Void> propertyKeyTokenLoader(final org.neo4j.kernel.impl.store.RecordStore<org.neo4j.kernel.impl.store.record.PropertyKeyTokenRecord> store)
		 public static RecordAccess_Loader<PropertyKeyTokenRecord, Void> PropertyKeyTokenLoader( RecordStore<PropertyKeyTokenRecord> store )
		 {
			  return new RecordAccess_LoaderAnonymousInnerClass6( store );
		 }

		 private class RecordAccess_LoaderAnonymousInnerClass6 : RecordAccess_Loader<PropertyKeyTokenRecord, Void>
		 {
			 private RecordStore<PropertyKeyTokenRecord> _store;

			 public RecordAccess_LoaderAnonymousInnerClass6( RecordStore<PropertyKeyTokenRecord> store )
			 {
				 this._store = store;
			 }

			 public PropertyKeyTokenRecord newUnused( long key, Void additionalData )
			 {
				  return AndMarkAsCreated( new PropertyKeyTokenRecord( toIntExact( key ) ) );
			 }

			 public PropertyKeyTokenRecord load( long key, Void additionalData )
			 {
				  return _store.getRecord( key, _store.newRecord(), NORMAL );
			 }

			 public void ensureHeavy( PropertyKeyTokenRecord record )
			 {
				  _store.ensureHeavy( record );
			 }

			 public PropertyKeyTokenRecord clone( PropertyKeyTokenRecord record )
			 {
				  return record.Clone();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.neo4j.kernel.impl.transaction.state.RecordAccess_Loader<org.neo4j.kernel.impl.store.record.LabelTokenRecord,Void> labelTokenLoader(final org.neo4j.kernel.impl.store.RecordStore<org.neo4j.kernel.impl.store.record.LabelTokenRecord> store)
		 public static RecordAccess_Loader<LabelTokenRecord, Void> LabelTokenLoader( RecordStore<LabelTokenRecord> store )
		 {
			  return new RecordAccess_LoaderAnonymousInnerClass7( store );
		 }

		 private class RecordAccess_LoaderAnonymousInnerClass7 : RecordAccess_Loader<LabelTokenRecord, Void>
		 {
			 private RecordStore<LabelTokenRecord> _store;

			 public RecordAccess_LoaderAnonymousInnerClass7( RecordStore<LabelTokenRecord> store )
			 {
				 this._store = store;
			 }

			 public LabelTokenRecord newUnused( long key, Void additionalData )
			 {
				  return AndMarkAsCreated( new LabelTokenRecord( toIntExact( key ) ) );
			 }

			 public LabelTokenRecord load( long key, Void additionalData )
			 {
				  return _store.getRecord( key, _store.newRecord(), NORMAL );
			 }

			 public void ensureHeavy( LabelTokenRecord record )
			 {
				  _store.ensureHeavy( record );
			 }

			 public LabelTokenRecord clone( LabelTokenRecord record )
			 {
				  return record.Clone();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.neo4j.kernel.impl.transaction.state.RecordAccess_Loader<org.neo4j.kernel.impl.store.record.RelationshipTypeTokenRecord,Void> relationshipTypeTokenLoader(final org.neo4j.kernel.impl.store.RecordStore<org.neo4j.kernel.impl.store.record.RelationshipTypeTokenRecord> store)
		 public static RecordAccess_Loader<RelationshipTypeTokenRecord, Void> RelationshipTypeTokenLoader( RecordStore<RelationshipTypeTokenRecord> store )
		 {
			  return new RecordAccess_LoaderAnonymousInnerClass8( store );
		 }

		 private class RecordAccess_LoaderAnonymousInnerClass8 : RecordAccess_Loader<RelationshipTypeTokenRecord, Void>
		 {
			 private RecordStore<RelationshipTypeTokenRecord> _store;

			 public RecordAccess_LoaderAnonymousInnerClass8( RecordStore<RelationshipTypeTokenRecord> store )
			 {
				 this._store = store;
			 }

			 public RelationshipTypeTokenRecord newUnused( long key, Void additionalData )
			 {
				  return AndMarkAsCreated( new RelationshipTypeTokenRecord( toIntExact( key ) ) );
			 }

			 public RelationshipTypeTokenRecord load( long key, Void additionalData )
			 {
				  return _store.getRecord( key, _store.newRecord(), NORMAL );
			 }

			 public void ensureHeavy( RelationshipTypeTokenRecord record )
			 {
				  _store.ensureHeavy( record );
			 }

			 public RelationshipTypeTokenRecord clone( RelationshipTypeTokenRecord record )
			 {
				  return record.Clone();
			 }
		 }

		 protected internal static RECORD AndMarkAsCreated<RECORD>( RECORD record ) where RECORD : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
		 {
			  record.setCreated();
			  return record;
		 }
	}

}