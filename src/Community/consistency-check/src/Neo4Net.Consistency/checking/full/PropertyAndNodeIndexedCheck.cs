using System;
using System.Collections.Generic;
using System.Diagnostics;

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
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using IntObjectMap = org.eclipse.collections.api.map.primitive.IntObjectMap;
	using MutableIntObjectMap = org.eclipse.collections.api.map.primitive.MutableIntObjectMap;
	using MutableIntSet = org.eclipse.collections.api.set.primitive.MutableIntSet;
	using IntObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.IntObjectHashMap;
	using IntHashSet = org.eclipse.collections.impl.set.mutable.primitive.IntHashSet;


	using Neo4Net.Consistency.checking;
	using Neo4Net.Consistency.checking;
	using Neo4Net.Consistency.checking;
	using CacheAccess = Neo4Net.Consistency.checking.cache.CacheAccess;
	using IndexAccessors = Neo4Net.Consistency.checking.index.IndexAccessors;
	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using RecordAccess = Neo4Net.Consistency.Store.RecordAccess;
	using IndexQuery = Neo4Net.Kernel.Api.Internal.IndexQuery;
	using IndexNotApplicableKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotApplicableKernelException;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor;
	using LookupFilter = Neo4Net.Kernel.Impl.Api.LookupFilter;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using PropertyBlock = Neo4Net.Kernel.Impl.Store.Records.PropertyBlock;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using Record = Neo4Net.Kernel.Impl.Store.Records.Record;
	using EntityType = Neo4Net.Kernel.Api.StorageEngine.EntityType;
	using IndexReader = Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader;
	using StoreIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

	/// <summary>
	/// Checks nodes and how they're indexed in one go. Reports any found inconsistencies.
	/// </summary>
	public class PropertyAndNodeIndexedCheck : RecordCheck<NodeRecord, Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport>
	{
		 private readonly IndexAccessors _indexes;
		 private readonly PropertyReader _propertyReader;
		 private readonly CacheAccess _cacheAccess;

		 internal PropertyAndNodeIndexedCheck( IndexAccessors indexes, PropertyReader propertyReader, CacheAccess cacheAccess )
		 {
			  this._indexes = indexes;
			  this._propertyReader = propertyReader;
			  this._cacheAccess = cacheAccess;
		 }

		 public override void Check( NodeRecord record, CheckerEngine<NodeRecord, Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport> engine, RecordAccess records )
		 {
			  try
			  {
					ICollection<PropertyRecord> properties = _propertyReader.getPropertyRecordChain( record.NextProp );
					_cacheAccess.client().putPropertiesToCache(properties);
					if ( _indexes != null )
					{
						 MatchIndexesToNode( record, engine, records, properties );
					}
					CheckProperty( record, engine, properties );
			  }
			  catch ( PropertyReader.CircularPropertyRecordChainException e )
			  {
					engine.Report().propertyChainContainsCircularReference(e.PropertyRecordClosingTheCircle());
			  }
		 }

		 /// <summary>
		 /// Matches indexes to a node.
		 /// </summary>
		 private void MatchIndexesToNode( NodeRecord record, CheckerEngine<NodeRecord, Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport> engine, RecordAccess records, ICollection<PropertyRecord> propertyRecs )
		 {
			  long[] labels = NodeLabelReader.GetListOfLabels( record, records, engine ).Select( long?.longValue ).ToArray();
			  IntObjectMap<PropertyBlock> nodePropertyMap = null;
			  foreach ( StoreIndexDescriptor indexRule in _indexes.onlineRules() )
			  {
					SchemaDescriptor schema = indexRule.Schema();
					if ( Schema.entityType() == EntityType.NODE && Schema.isAffected(labels) )
					{
						 if ( nodePropertyMap == null )
						 {
							  nodePropertyMap = Properties( _propertyReader.propertyBlocks( propertyRecs ) );
						 }

						 if ( IEntityIntersectsSchema( nodePropertyMap, schema ) )
						 {
							  Value[] values = GetPropertyValues( _propertyReader, nodePropertyMap, Schema.PropertyIds );
							  using ( IndexReader reader = _indexes.accessorFor( indexRule ).newReader() )
							  {
									long nodeId = record.Id;

									if ( indexRule.CanSupportUniqueConstraint() )
									{
										 VerifyNodeCorrectlyIndexedUniquely( nodeId, values, engine, indexRule, reader );
									}
									else
									{
										 long count = reader.CountIndexedNodes( nodeId, Schema.PropertyIds, values );
										 ReportIncorrectIndexCount( values, engine, indexRule, count );
									}
							  }
						 }
					}
			  }
		 }

		 private void VerifyNodeCorrectlyIndexedUniquely( long nodeId, Value[] propertyValues, CheckerEngine<NodeRecord, Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport> engine, StoreIndexDescriptor indexRule, IndexReader reader )
		 {
			  IndexQuery[] query = Seek( indexRule.Schema(), propertyValues );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.iterator.LongIterator indexedNodeIds = queryIndexOrEmpty(reader, query);
			  LongIterator indexedNodeIds = QueryIndexOrEmpty( reader, query );

			  long count = 0;
			  while ( indexedNodeIds.hasNext() )
			  {
					long indexedNodeId = indexedNodeIds.next();

					if ( nodeId == indexedNodeId )
					{
						 count++;
					}
					else
					{
						 engine.Report().uniqueIndexNotUnique(indexRule, Values.asObjects(propertyValues), indexedNodeId);
					}
			  }

			  ReportIncorrectIndexCount( propertyValues, engine, indexRule, count );
		 }

		 private void ReportIncorrectIndexCount( Value[] propertyValues, CheckerEngine<NodeRecord, Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport> engine, StoreIndexDescriptor indexRule, long count )
		 {
			  if ( count == 0 )
			  {
					engine.Report().notIndexed(indexRule, Values.asObjects(propertyValues));
			  }
			  else if ( count != 1 )
			  {
					engine.Report().indexedMultipleTimes(indexRule, Values.asObjects(propertyValues), count);
			  }
		 }

		 private void CheckProperty( NodeRecord record, CheckerEngine<NodeRecord, Neo4Net.Consistency.report.ConsistencyReport_NodeConsistencyReport> engine, ICollection<PropertyRecord> props )
		 {
			  if ( !Record.NO_NEXT_PROPERTY.@is( record.NextProp ) )
			  {
					PropertyRecord firstProp = props.GetEnumerator().next();
					if ( !Record.NO_PREVIOUS_PROPERTY.@is( firstProp.PrevProp ) )
					{
						 engine.Report().propertyNotFirstInChain(firstProp);
					}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.primitive.MutableIntSet keys = new org.eclipse.collections.impl.set.mutable.primitive.IntHashSet();
					MutableIntSet keys = new IntHashSet();
					foreach ( PropertyRecord property in props )
					{
						 if ( !property.InUse() )
						 {
							  engine.Report().propertyNotInUse(property);
						 }
						 else
						 {
							  foreach ( int key in ChainCheck.keys( property ) )
							  {
									if ( !keys.add( key ) )
									{
										 engine.Report().propertyKeyNotUniqueInChain();
									}
							  }
						 }
					}
			  }
		 }

		 internal static Value[] GetPropertyValues( PropertyReader propertyReader, IntObjectMap<PropertyBlock> propertyMap, int[] indexPropertyIds )
		 {
			  Value[] values = new Value[indexPropertyIds.Length];
			  for ( int i = 0; i < indexPropertyIds.Length; i++ )
			  {
					PropertyBlock propertyBlock = propertyMap.get( indexPropertyIds[i] );
					values[i] = propertyReader.PropertyValue( propertyBlock );
			  }
			  return values;
		 }

		 internal static IntObjectMap<PropertyBlock> Properties( IList<PropertyBlock> propertyBlocks )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.map.primitive.MutableIntObjectMap<Neo4Net.kernel.impl.store.record.PropertyBlock> propertyIds = new org.eclipse.collections.impl.map.mutable.primitive.IntObjectHashMap<>();
			  MutableIntObjectMap<PropertyBlock> propertyIds = new IntObjectHashMap<PropertyBlock>();
			  foreach ( PropertyBlock propertyBlock in propertyBlocks )
			  {
					propertyIds.put( propertyBlock.KeyIndexId, propertyBlock );
			  }
			  return propertyIds;
		 }

		 private IndexQuery[] Seek( SchemaDescriptor schema, Value[] propertyValues )
		 {
			  int[] propertyIds = Schema.PropertyIds;
			  Debug.Assert( propertyIds.Length == propertyValues.Length );
			  IndexQuery[] query = new IndexQuery[propertyValues.Length];
			  for ( int i = 0; i < query.Length; i++ )
			  {
					query[i] = IndexQuery.exact( propertyIds[i], propertyValues[i] );
			  }
			  return query;
		 }

		 private LongIterator QueryIndexOrEmpty( IndexReader reader, IndexQuery[] query )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.iterator.LongIterator indexedNodeIds;
			  LongIterator indexedNodeIds;
			  try
			  {
					indexedNodeIds = reader.Query( query );
			  }
			  catch ( IndexNotApplicableKernelException e )
			  {
					throw new Exception( format( "Consistency checking error: index provider does not support exact query %s", Arrays.ToString( query ) ), e );
			  }

			  return reader.HasFullValuePrecision( query ) ? indexedNodeIds : LookupFilter.exactIndexMatches( _propertyReader, indexedNodeIds, query );
		 }

		 internal static bool IEntityIntersectsSchema( IntObjectMap<PropertyBlock> IEntityPropertyMap, SchemaDescriptor schema )
		 {
			  bool requireAllTokens = Schema.propertySchemaType() == Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor_PropertySchemaType.CompleteAllTokens;
			  if ( requireAllTokens )
			  {
					return HasAllProperties( IEntityPropertyMap, Schema.PropertyIds );
			  }
			  else
			  {
					return HasAnyProperty( IEntityPropertyMap, Schema.PropertyIds );
			  }
		 }

		 private static bool HasAllProperties( IntObjectMap<PropertyBlock> blockMap, int[] indexPropertyIds )
		 {
			  foreach ( int indexPropertyId in indexPropertyIds )
			  {
					if ( !blockMap.containsKey( indexPropertyId ) )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 private static bool HasAnyProperty( IntObjectMap<PropertyBlock> blockMap, int[] indexPropertyIds )
		 {
			  foreach ( int indexPropertyId in indexPropertyIds )
			  {
					if ( blockMap.containsKey( indexPropertyId ) )
					{
						 return true;
					}
			  }
			  return false;
		 }
	}

}