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
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;
	using IntObjectMap = org.eclipse.collections.api.map.primitive.IntObjectMap;


	using Org.Neo4j.Consistency.checking;
	using Org.Neo4j.Consistency.checking;
	using IndexAccessors = Org.Neo4j.Consistency.checking.index.IndexAccessors;
	using ConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport;
	using RecordAccess = Org.Neo4j.Consistency.store.RecordAccess;
	using SchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptor;
	using PropertyBlock = Org.Neo4j.Kernel.impl.store.record.PropertyBlock;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.full.PropertyAndNodeIndexedCheck.entityIntersectsSchema;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.full.PropertyAndNodeIndexedCheck.getPropertyValues;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.full.PropertyAndNodeIndexedCheck.properties;

	public class RelationshipToIndexCheck : RecordCheck<RelationshipRecord, Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipConsistencyReport>
	{
		 private readonly IndexAccessors _indexes;
		 private readonly StoreIndexDescriptor[] _relationshipIndexes;
		 private readonly PropertyReader _propertyReader;

		 internal RelationshipToIndexCheck( IList<StoreIndexDescriptor> relationshipIndexes, IndexAccessors indexes, PropertyReader propertyReader )
		 {
			  this._relationshipIndexes = relationshipIndexes.ToArray();
			  this._indexes = indexes;
			  this._propertyReader = propertyReader;
		 }

		 public override void Check( RelationshipRecord record, CheckerEngine<RelationshipRecord, Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipConsistencyReport> engine, RecordAccess records )
		 {
			  try
			  {
					IntObjectMap<PropertyBlock> propertyMap = null;
					foreach ( StoreIndexDescriptor index in _relationshipIndexes )
					{
						 SchemaDescriptor schema = index.Schema();
						 if ( ArrayUtils.contains( Schema.EntityTokenIds, record.Type ) )
						 {
							  if ( propertyMap == null )
							  {
									ICollection<PropertyRecord> propertyRecs = _propertyReader.getPropertyRecordChain( record.NextProp );
									propertyMap = properties( _propertyReader.propertyBlocks( propertyRecs ) );
							  }

							  if ( entityIntersectsSchema( propertyMap, schema ) )
							  {
									Value[] values = getPropertyValues( _propertyReader, propertyMap, Schema.PropertyIds );
									using ( IndexReader reader = _indexes.accessorFor( index ).newReader() )
									{
										 long entityId = record.Id;
										 long count = reader.CountIndexedNodes( entityId, Schema.PropertyIds, values );
										 ReportIncorrectIndexCount( values, engine, index, count );
									}
							  }
						 }
					}
			  }
			  catch ( PropertyReader.CircularPropertyRecordChainException )
			  {
					// The property chain contains a circular reference and is therefore not sane.
					// Skip it, since this inconsistency has been reported by PropertyChain checker already.
			  }
		 }

		 private void ReportIncorrectIndexCount( Value[] values, CheckerEngine<RelationshipRecord, Org.Neo4j.Consistency.report.ConsistencyReport_RelationshipConsistencyReport> engine, StoreIndexDescriptor index, long count )
		 {
			  if ( count == 0 )
			  {
					engine.Report().notIndexed(index, Values.asObjects(values));
			  }
			  else if ( count != 1 )
			  {
					engine.Report().indexedMultipleTimes(index, Values.asObjects(values), count);
			  }
		 }
	}

}