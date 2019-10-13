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
namespace Neo4Net.Consistency.checking.full
{
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;
	using IntObjectMap = org.eclipse.collections.api.map.primitive.IntObjectMap;


	using Neo4Net.Consistency.checking;
	using Neo4Net.Consistency.checking;
	using IndexAccessors = Neo4Net.Consistency.checking.index.IndexAccessors;
	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using RecordAccess = Neo4Net.Consistency.store.RecordAccess;
	using SchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.SchemaDescriptor;
	using PropertyBlock = Neo4Net.Kernel.impl.store.record.PropertyBlock;
	using PropertyRecord = Neo4Net.Kernel.impl.store.record.PropertyRecord;
	using RelationshipRecord = Neo4Net.Kernel.impl.store.record.RelationshipRecord;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.full.PropertyAndNodeIndexedCheck.entityIntersectsSchema;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.full.PropertyAndNodeIndexedCheck.getPropertyValues;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.consistency.checking.full.PropertyAndNodeIndexedCheck.properties;

	public class RelationshipToIndexCheck : RecordCheck<RelationshipRecord, Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport>
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

		 public override void Check( RelationshipRecord record, CheckerEngine<RelationshipRecord, Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport> engine, RecordAccess records )
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

		 private void ReportIncorrectIndexCount( Value[] values, CheckerEngine<RelationshipRecord, Neo4Net.Consistency.report.ConsistencyReport_RelationshipConsistencyReport> engine, StoreIndexDescriptor index, long count )
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