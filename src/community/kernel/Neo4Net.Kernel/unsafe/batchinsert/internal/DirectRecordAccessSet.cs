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
namespace Neo4Net.@unsafe.Batchinsert.Internal
{
	using Loaders = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.Loaders;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using PropertyStore = Neo4Net.Kernel.impl.store.PropertyStore;
	using Neo4Net.Kernel.impl.store;
	using SchemaStore = Neo4Net.Kernel.impl.store.SchemaStore;
	using LabelTokenRecord = Neo4Net.Kernel.Impl.Store.Records.LabelTokenRecord;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PrimitiveRecord = Neo4Net.Kernel.Impl.Store.Records.PrimitiveRecord;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyKeyTokenRecord;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipTypeTokenRecord;
	using SchemaRecord = Neo4Net.Kernel.Impl.Store.Records.SchemaRecord;
	using Neo4Net.Kernel.impl.transaction.state;
	using RecordAccessSet = Neo4Net.Kernel.impl.transaction.state.RecordAccessSet;
	using SchemaRule = Neo4Net.Storageengine.Api.schema.SchemaRule;

	public class DirectRecordAccessSet : RecordAccessSet
	{
		 private readonly DirectRecordAccess<NodeRecord, Void> _nodeRecords;
		 private readonly DirectRecordAccess<PropertyRecord, PrimitiveRecord> _propertyRecords;
		 private readonly DirectRecordAccess<RelationshipRecord, Void> _relationshipRecords;
		 private readonly DirectRecordAccess<RelationshipGroupRecord, int> _relationshipGroupRecords;
		 private readonly DirectRecordAccess<PropertyKeyTokenRecord, Void> _propertyKeyTokenRecords;
		 private readonly DirectRecordAccess<RelationshipTypeTokenRecord, Void> _relationshipTypeTokenRecords;
		 private readonly DirectRecordAccess<LabelTokenRecord, Void> _labelTokenRecords;
		 private readonly DirectRecordAccess[] _all;

		 public DirectRecordAccessSet( NeoStores neoStores ) : this( neoStores.NodeStore, neoStores.PropertyStore, neoStores.RelationshipStore, neoStores.RelationshipGroupStore, neoStores.PropertyKeyTokenStore, neoStores.RelationshipTypeTokenStore, neoStores.LabelTokenStore, neoStores.SchemaStore )
		 {
		 }

		 public DirectRecordAccessSet( RecordStore<NodeRecord> nodeStore, PropertyStore propertyStore, RecordStore<RelationshipRecord> relationshipStore, RecordStore<RelationshipGroupRecord> relationshipGroupStore, RecordStore<PropertyKeyTokenRecord> propertyKeyTokenStore, RecordStore<RelationshipTypeTokenRecord> relationshipTypeTokenStore, RecordStore<LabelTokenRecord> labelTokenStore, SchemaStore schemaStore )
		 {
			  Loaders loaders = new Loaders( nodeStore, propertyStore, relationshipStore, relationshipGroupStore, propertyKeyTokenStore, relationshipTypeTokenStore, labelTokenStore, schemaStore );
			  _nodeRecords = new DirectRecordAccess<NodeRecord, Void>( nodeStore, loaders.NodeLoader() );
			  _propertyRecords = new DirectRecordAccess<PropertyRecord, PrimitiveRecord>( propertyStore, loaders.PropertyLoader() );
			  _relationshipRecords = new DirectRecordAccess<RelationshipRecord, Void>( relationshipStore, loaders.RelationshipLoader() );
			  _relationshipGroupRecords = new DirectRecordAccess<RelationshipGroupRecord, int>( relationshipGroupStore, loaders.RelationshipGroupLoader() );
			  _propertyKeyTokenRecords = new DirectRecordAccess<PropertyKeyTokenRecord, Void>( propertyKeyTokenStore, loaders.PropertyKeyTokenLoader() );
			  _relationshipTypeTokenRecords = new DirectRecordAccess<RelationshipTypeTokenRecord, Void>( relationshipTypeTokenStore, loaders.RelationshipTypeTokenLoader() );
			  _labelTokenRecords = new DirectRecordAccess<LabelTokenRecord, Void>( labelTokenStore, loaders.LabelTokenLoader() );
			  _all = new DirectRecordAccess[] { _nodeRecords, _propertyRecords, _relationshipRecords, _relationshipGroupRecords, _propertyKeyTokenRecords, _relationshipTypeTokenRecords, _labelTokenRecords };
		 }

		 public virtual RecordAccess<NodeRecord, Void> NodeRecords
		 {
			 get
			 {
				  return _nodeRecords;
			 }
		 }

		 public virtual RecordAccess<PropertyRecord, PrimitiveRecord> PropertyRecords
		 {
			 get
			 {
				  return _propertyRecords;
			 }
		 }

		 public virtual RecordAccess<RelationshipRecord, Void> RelRecords
		 {
			 get
			 {
				  return _relationshipRecords;
			 }
		 }

		 public virtual RecordAccess<RelationshipGroupRecord, int> RelGroupRecords
		 {
			 get
			 {
				  return _relationshipGroupRecords;
			 }
		 }

		 public virtual RecordAccess<SchemaRecord, SchemaRule> SchemaRuleChanges
		 {
			 get
			 {
				  throw new System.NotSupportedException( "Not needed. Implement if needed" );
			 }
		 }

		 public virtual RecordAccess<PropertyKeyTokenRecord, Void> PropertyKeyTokenChanges
		 {
			 get
			 {
				  return _propertyKeyTokenRecords;
			 }
		 }

		 public virtual RecordAccess<LabelTokenRecord, Void> LabelTokenChanges
		 {
			 get
			 {
				  return _labelTokenRecords;
			 }
		 }

		 public virtual RecordAccess<RelationshipTypeTokenRecord, Void> RelationshipTypeTokenChanges
		 {
			 get
			 {
				  return _relationshipTypeTokenRecords;
			 }
		 }

		 public override void Close()
		 {
			  Commit();
			  foreach ( DirectRecordAccess access in _all )
			  {
					access.close();
			  }
		 }

		 public virtual void Commit()
		 {
			  foreach ( DirectRecordAccess access in _all )
			  {
					access.commit();
			  }
		 }

		 public override bool HasChanges()
		 {
			  foreach ( DirectRecordAccess access in _all )
			  {
					if ( access.changeSize() > 0 )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 public override int ChangeSize()
		 {
			  int total = 0;
			  foreach ( DirectRecordAccess access in _all )
			  {
					total += access.changeSize();
			  }
			  return total;
		 }
	}

}