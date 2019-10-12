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
namespace Org.Neo4j.Kernel.impl.transaction.state
{
	using Loaders = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.Loaders;
	using LabelTokenRecord = Org.Neo4j.Kernel.impl.store.record.LabelTokenRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PrimitiveRecord = Org.Neo4j.Kernel.impl.store.record.PrimitiveRecord;
	using PropertyKeyTokenRecord = Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using RelationshipGroupRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using RelationshipTypeTokenRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipTypeTokenRecord;
	using SchemaRecord = Org.Neo4j.Kernel.impl.store.record.SchemaRecord;
	using Org.Neo4j.Kernel.impl.transaction.state;
	using IntCounter = Org.Neo4j.Kernel.impl.util.statistics.IntCounter;
	using SchemaRule = Org.Neo4j.Storageengine.Api.schema.SchemaRule;

	public class RecordChangeSet : RecordAccessSet
	{
		 private readonly RecordAccess<NodeRecord, Void> _nodeRecords;
		 private readonly RecordAccess<PropertyRecord, PrimitiveRecord> _propertyRecords;
		 private readonly RecordAccess<RelationshipRecord, Void> _relRecords;
		 private readonly RecordAccess<RelationshipGroupRecord, int> _relGroupRecords;
		 private readonly RecordAccess<SchemaRecord, SchemaRule> _schemaRuleChanges;
		 private readonly RecordAccess<PropertyKeyTokenRecord, Void> _propertyKeyTokenChanges;
		 private readonly RecordAccess<LabelTokenRecord, Void> _labelTokenChanges;
		 private readonly RecordAccess<RelationshipTypeTokenRecord, Void> _relationshipTypeTokenChanges;
		 private readonly IntCounter _changeCounter = new IntCounter();

		 public RecordChangeSet( Loaders loaders ) : this( loaders.NodeLoader(), loaders.PropertyLoader(), loaders.RelationshipLoader(), loaders.RelationshipGroupLoader(), loaders.SchemaRuleLoader(), loaders.PropertyKeyTokenLoader(), loaders.LabelTokenLoader(), loaders.RelationshipTypeTokenLoader() )
		 {
		 }

		 public RecordChangeSet( RecordAccess_Loader<NodeRecord, Void> nodeLoader, RecordAccess_Loader<PropertyRecord, PrimitiveRecord> propertyLoader, RecordAccess_Loader<RelationshipRecord, Void> relationshipLoader, RecordAccess_Loader<RelationshipGroupRecord, int> relationshipGroupLoader, RecordAccess_Loader<SchemaRecord, SchemaRule> schemaRuleLoader, RecordAccess_Loader<PropertyKeyTokenRecord, Void> propertyKeyTokenLoader, RecordAccess_Loader<LabelTokenRecord, Void> labelTokenLoader, RecordAccess_Loader<RelationshipTypeTokenRecord, Void> relationshipTypeTokenLoader )
		 {
			  this._nodeRecords = new RecordChanges<NodeRecord, Void>( nodeLoader, _changeCounter );
			  this._propertyRecords = new RecordChanges<PropertyRecord, PrimitiveRecord>( propertyLoader, _changeCounter );
			  this._relRecords = new RecordChanges<RelationshipRecord, Void>( relationshipLoader, _changeCounter );
			  this._relGroupRecords = new RecordChanges<RelationshipGroupRecord, int>( relationshipGroupLoader, _changeCounter );
			  this._schemaRuleChanges = new RecordChanges<SchemaRecord, SchemaRule>( schemaRuleLoader, _changeCounter );
			  this._propertyKeyTokenChanges = new RecordChanges<PropertyKeyTokenRecord, Void>( propertyKeyTokenLoader, _changeCounter );
			  this._labelTokenChanges = new RecordChanges<LabelTokenRecord, Void>( labelTokenLoader, _changeCounter );
			  this._relationshipTypeTokenChanges = new RecordChanges<RelationshipTypeTokenRecord, Void>( relationshipTypeTokenLoader, _changeCounter );
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
				  return _relRecords;
			 }
		 }

		 public virtual RecordAccess<RelationshipGroupRecord, int> RelGroupRecords
		 {
			 get
			 {
				  return _relGroupRecords;
			 }
		 }

		 public virtual RecordAccess<SchemaRecord, SchemaRule> SchemaRuleChanges
		 {
			 get
			 {
				  return _schemaRuleChanges;
			 }
		 }

		 public virtual RecordAccess<PropertyKeyTokenRecord, Void> PropertyKeyTokenChanges
		 {
			 get
			 {
				  return _propertyKeyTokenChanges;
			 }
		 }

		 public virtual RecordAccess<LabelTokenRecord, Void> LabelTokenChanges
		 {
			 get
			 {
				  return _labelTokenChanges;
			 }
		 }

		 public virtual RecordAccess<RelationshipTypeTokenRecord, Void> RelationshipTypeTokenChanges
		 {
			 get
			 {
				  return _relationshipTypeTokenChanges;
			 }
		 }

		 public override bool HasChanges()
		 {
			  return _changeCounter.value() > 0;
		 }

		 public override int ChangeSize()
		 {
			  return _changeCounter.value();
		 }

		 public override void Close()
		 {
			  if ( HasChanges() )
			  {
					_nodeRecords.close();
					_propertyRecords.close();
					_relRecords.close();
					_schemaRuleChanges.close();
					_relGroupRecords.close();
					_propertyKeyTokenChanges.close();
					_labelTokenChanges.close();
					_relationshipTypeTokenChanges.close();
					_changeCounter.clear();
			  }
		 }
	}

}