﻿/*
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
namespace Neo4Net.Consistency.checking.full
{
	using Neo4Net.Consistency.checking;
	using Neo4Net.Consistency.checking;
	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using RecordAccess = Neo4Net.Consistency.store.RecordAccess;
	using IndexEntry = Neo4Net.Consistency.store.synthetic.IndexEntry;
	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;
	using IEntityType = Neo4Net.Storageengine.Api.EntityType;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;

	public class IndexCheck : RecordCheck<IndexEntry, Neo4Net.Consistency.report.ConsistencyReport_IndexConsistencyReport>
	{
		 private readonly IEntityType _entityType;
		 private readonly StoreIndexDescriptor _indexRule;
		 private NodeInUseWithCorrectLabelsCheck<IndexEntry, Neo4Net.Consistency.report.ConsistencyReport_IndexConsistencyReport> _nodeChecker;
		 private RelationshipInUseWithCorrectRelationshipTypeCheck<IndexEntry, Neo4Net.Consistency.report.ConsistencyReport_IndexConsistencyReport> _relationshipChecker;

		 internal IndexCheck( StoreIndexDescriptor indexRule )
		 {
			  this._indexRule = indexRule;
			  SchemaDescriptor schema = indexRule.Schema();
			  int[] IEntityTokenIntIds = Schema.EntityTokenIds;
			  long[] IEntityTokenLongIds = new long[entityTokenIntIds.Length];
			  for ( int i = 0; i < IEntityTokenIntIds.Length; i++ )
			  {
					entityTokenLongIds[i] = IEntityTokenIntIds[i];
			  }
			  Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor_PropertySchemaType propertySchemaType = Schema.propertySchemaType();
			  _entityType = Schema.entityType();
			  if ( _entityType == IEntityType.NODE )
			  {
					_nodeChecker = new NodeInUseWithCorrectLabelsCheck<IndexEntry, Neo4Net.Consistency.report.ConsistencyReport_IndexConsistencyReport>( IEntityTokenLongIds, propertySchemaType, false );
			  }
			  if ( _entityType == IEntityType.RELATIONSHIP )
			  {
					_relationshipChecker = new RelationshipInUseWithCorrectRelationshipTypeCheck<IndexEntry, Neo4Net.Consistency.report.ConsistencyReport_IndexConsistencyReport>( IEntityTokenLongIds );
			  }
		 }

		 public override void Check( IndexEntry record, CheckerEngine<IndexEntry, Neo4Net.Consistency.report.ConsistencyReport_IndexConsistencyReport> engine, RecordAccess records )
		 {
			  long id = record.Id;
			  switch ( _entityType.innerEnumValue )
			  {
			  case IEntityType.InnerEnum.NODE:
					engine.ComparativeCheck( records.Node( id ), _nodeChecker );
					break;
			  case IEntityType.InnerEnum.RELATIONSHIP:
					if ( _indexRule.canSupportUniqueConstraint() )
					{
						 engine.Report().relationshipConstraintIndex();
					}
					engine.ComparativeCheck( records.Relationship( id ), _relationshipChecker );
					break;
			  default:
					throw new System.InvalidOperationException( "Don't know how to check index entry of IEntity type " + _entityType );
			  }
		 }
	}

}