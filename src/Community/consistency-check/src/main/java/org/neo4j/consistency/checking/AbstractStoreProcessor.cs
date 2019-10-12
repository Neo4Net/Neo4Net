using System;

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
namespace Neo4Net.Consistency.checking
{
	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using ConsistencyReport_LabelTokenConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport_LabelTokenConsistencyReport;
	using ConsistencyReport_NodeConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport;
	using ConsistencyReport_PropertyConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport_PropertyConsistencyReport;
	using ConsistencyReport_PropertyKeyTokenConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport_PropertyKeyTokenConsistencyReport;
	using ConsistencyReport_RelationshipConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport;
	using ConsistencyReport_RelationshipGroupConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport_RelationshipGroupConsistencyReport;
	using ConsistencyReport_RelationshipTypeConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport_RelationshipTypeConsistencyReport;
	using Neo4Net.Kernel.impl.store;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using DynamicRecord = Neo4Net.Kernel.impl.store.record.DynamicRecord;
	using LabelTokenRecord = Neo4Net.Kernel.impl.store.record.LabelTokenRecord;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using PropertyKeyTokenRecord = Neo4Net.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using PropertyRecord = Neo4Net.Kernel.impl.store.record.PropertyRecord;
	using RelationshipGroupRecord = Neo4Net.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;
	using RelationshipTypeTokenRecord = Neo4Net.Kernel.impl.store.record.RelationshipTypeTokenRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.DynamicStore.ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.DynamicStore.NODE_LABEL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.DynamicStore.SCHEMA;

	public abstract class AbstractStoreProcessor : Neo4Net.Kernel.impl.store.RecordStore_Processor<Exception>
	{
		 private RecordCheck<NodeRecord, ConsistencyReport_NodeConsistencyReport> _sparseNodeChecker;
		 private RecordCheck<NodeRecord, ConsistencyReport_NodeConsistencyReport> _denseNodeChecker;
		 private RecordCheck<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport> _relationshipChecker;
		 private readonly RecordCheck<PropertyRecord, ConsistencyReport_PropertyConsistencyReport> _propertyChecker;
		 private readonly RecordCheck<PropertyKeyTokenRecord, ConsistencyReport_PropertyKeyTokenConsistencyReport> _propertyKeyTokenChecker;
		 private readonly RecordCheck<RelationshipTypeTokenRecord, ConsistencyReport_RelationshipTypeConsistencyReport> _relationshipTypeTokenChecker;
		 private readonly RecordCheck<LabelTokenRecord, ConsistencyReport_LabelTokenConsistencyReport> _labelTokenChecker;
		 private readonly RecordCheck<RelationshipGroupRecord, ConsistencyReport_RelationshipGroupConsistencyReport> _relationshipGroupChecker;

		 public AbstractStoreProcessor( CheckDecorator decorator )
		 {
			  this._sparseNodeChecker = decorator.DecorateNodeChecker( NodeRecordCheck.ForSparseNodes() );
			  this._denseNodeChecker = decorator.DecorateNodeChecker( NodeRecordCheck.ForDenseNodes() );
			  this._relationshipChecker = decorator.DecorateRelationshipChecker( new RelationshipRecordCheck() );
			  this._propertyChecker = decorator.DecoratePropertyChecker( new PropertyRecordCheck() );
			  this._propertyKeyTokenChecker = decorator.DecoratePropertyKeyTokenChecker( new PropertyKeyTokenRecordCheck() );
			  this._relationshipTypeTokenChecker = decorator.DecorateRelationshipTypeTokenChecker( new RelationshipTypeTokenRecordCheck() );
			  this._labelTokenChecker = decorator.DecorateLabelTokenChecker( new LabelTokenRecordCheck() );
			  this._relationshipGroupChecker = decorator.DecorateRelationshipGroupChecker( new RelationshipGroupRecordCheck() );
		 }

		 public virtual void ReDecorateRelationship( CheckDecorator decorator, RelationshipRecordCheck newChecker )
		 {
			  this._relationshipChecker = decorator.DecorateRelationshipChecker( newChecker );
		 }

		 public virtual void ReDecorateNode( CheckDecorator decorator, NodeRecordCheck newChecker, bool sparseNode )
		 {
			  if ( sparseNode )
			  {
					this._sparseNodeChecker = decorator.DecorateNodeChecker( newChecker );
			  }
			  else
			  {
					this._denseNodeChecker = decorator.DecorateNodeChecker( newChecker );
			  }
		 }

		 protected internal abstract void CheckNode( RecordStore<NodeRecord> store, NodeRecord node, RecordCheck<NodeRecord, ConsistencyReport_NodeConsistencyReport> checker );

		 protected internal abstract void CheckRelationship( RecordStore<RelationshipRecord> store, RelationshipRecord rel, RecordCheck<RelationshipRecord, ConsistencyReport_RelationshipConsistencyReport> checker );

		 protected internal abstract void CheckProperty( RecordStore<PropertyRecord> store, PropertyRecord property, RecordCheck<PropertyRecord, ConsistencyReport_PropertyConsistencyReport> checker );

		 protected internal abstract void CheckRelationshipTypeToken( RecordStore<RelationshipTypeTokenRecord> store, RelationshipTypeTokenRecord record, RecordCheck<RelationshipTypeTokenRecord, ConsistencyReport_RelationshipTypeConsistencyReport> checker );

		 protected internal abstract void CheckLabelToken( RecordStore<LabelTokenRecord> store, LabelTokenRecord record, RecordCheck<LabelTokenRecord, ConsistencyReport_LabelTokenConsistencyReport> checker );

		 protected internal abstract void CheckPropertyKeyToken( RecordStore<PropertyKeyTokenRecord> store, PropertyKeyTokenRecord record, RecordCheck<PropertyKeyTokenRecord, ConsistencyReport_PropertyKeyTokenConsistencyReport> checker );

		 protected internal abstract void CheckDynamic( RecordType type, RecordStore<DynamicRecord> store, DynamicRecord @string, RecordCheck<DynamicRecord, Neo4Net.Consistency.report.ConsistencyReport_DynamicConsistencyReport> checker );

		 protected internal abstract void CheckDynamicLabel( RecordType type, RecordStore<DynamicRecord> store, DynamicRecord @string, RecordCheck<DynamicRecord, Neo4Net.Consistency.report.ConsistencyReport_DynamicLabelConsistencyReport> checker );

		 protected internal abstract void CheckRelationshipGroup( RecordStore<RelationshipGroupRecord> store, RelationshipGroupRecord record, RecordCheck<RelationshipGroupRecord, ConsistencyReport_RelationshipGroupConsistencyReport> checker );

		 public override void ProcessSchema( RecordStore<DynamicRecord> store, DynamicRecord schema )
		 {
			  // cf. StoreProcessor
			  CheckDynamic( RecordType.SCHEMA, store, schema, new DynamicRecordCheck( store, SCHEMA ) );
		 }

		 public override void ProcessNode( RecordStore<NodeRecord> store, NodeRecord node )
		 {
			  if ( node.Dense )
			  {
					CheckNode( store, node, _denseNodeChecker );
			  }
			  else
			  {
					CheckNode( store, node, _sparseNodeChecker );
			  }
		 }

		 public override void ProcessRelationship( RecordStore<RelationshipRecord> store, RelationshipRecord rel )
		 {
			  CheckRelationship( store, rel, _relationshipChecker );
		 }

		 public override void ProcessProperty( RecordStore<PropertyRecord> store, PropertyRecord property )
		 {
			  CheckProperty( store, property, _propertyChecker );
		 }

		 public override void ProcessString( RecordStore<DynamicRecord> store, DynamicRecord @string, IdType idType )
		 {
			  RecordType type;
			  DynamicStore dereference;
			  switch ( idType )
			  {
			  case IdType.STRING_BLOCK:
					type = RecordType.STRING_PROPERTY;
					dereference = DynamicStore.String;
					break;
			  case IdType.RELATIONSHIP_TYPE_TOKEN_NAME:
					type = RecordType.RELATIONSHIP_TYPE_NAME;
					dereference = DynamicStore.RelationshipType;
					break;
			  case IdType.PROPERTY_KEY_TOKEN_NAME:
					type = RecordType.PROPERTY_KEY_NAME;
					dereference = DynamicStore.PropertyKey;
					break;
			  case IdType.LABEL_TOKEN_NAME:
					type = RecordType.LABEL_NAME;
					dereference = DynamicStore.Label;
					break;
			  default:
					throw new System.ArgumentException( format( "The id type [%s] is not valid for String records.", idType ) );
			  }
			  CheckDynamic( type, store, @string, new DynamicRecordCheck( store, dereference ) );
		 }

		 public override void ProcessArray( RecordStore<DynamicRecord> store, DynamicRecord array )
		 {
			  CheckDynamic( RecordType.ARRAY_PROPERTY, store, array, new DynamicRecordCheck( store, ARRAY ) );
		 }

		 public override void ProcessLabelArrayWithOwner( RecordStore<DynamicRecord> store, DynamicRecord array )
		 {
			  CheckDynamic( RecordType.NODE_DYNAMIC_LABEL, store, array, new DynamicRecordCheck( store, NODE_LABEL ) );
			  CheckDynamicLabel( RecordType.NODE_DYNAMIC_LABEL, store, array, new NodeDynamicLabelOrphanChainStartCheck() );
		 }

		 public override void ProcessRelationshipTypeToken( RecordStore<RelationshipTypeTokenRecord> store, RelationshipTypeTokenRecord record )
		 {
			  CheckRelationshipTypeToken( store, record, _relationshipTypeTokenChecker );
		 }

		 public override void ProcessPropertyKeyToken( RecordStore<PropertyKeyTokenRecord> store, PropertyKeyTokenRecord record )
		 {
			  CheckPropertyKeyToken( store, record, _propertyKeyTokenChecker );
		 }

		 public override void ProcessLabelToken( RecordStore<LabelTokenRecord> store, LabelTokenRecord record )
		 {
			  CheckLabelToken( store, record, _labelTokenChecker );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void processRelationshipGroup(org.neo4j.kernel.impl.store.RecordStore<org.neo4j.kernel.impl.store.record.RelationshipGroupRecord> store, org.neo4j.kernel.impl.store.record.RelationshipGroupRecord record) throws RuntimeException
		 public override void ProcessRelationshipGroup( RecordStore<RelationshipGroupRecord> store, RelationshipGroupRecord record )
		 {
			  CheckRelationshipGroup( store, record, _relationshipGroupChecker );
		 }

	}

}