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
namespace Neo4Net.Kernel.Impl.Newapi
{
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;
	using MutableIntObjectMap = org.eclipse.collections.api.map.primitive.MutableIntObjectMap;
	using IntObjectMaps = org.eclipse.collections.impl.factory.primitive.IntObjectMaps;

	using NodeCursor = Neo4Net.@internal.Kernel.Api.NodeCursor;
	using PropertyCursor = Neo4Net.@internal.Kernel.Api.PropertyCursor;
	using SchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.SchemaDescriptor;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueTuple = Neo4Net.Values.Storable.ValueTuple;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.StatementConstants.NO_SUCH_PROPERTY_KEY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.EntityType.NODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.NO_VALUE;

	/// <summary>
	/// Utility class that performs necessary updates for the transaction state.
	/// </summary>
	public class IndexTxStateUpdater
	{
		 private readonly Read _read;
		 private readonly IndexingService _indexingService;

		 public IndexTxStateUpdater( Read read, IndexingService indexingService )
		 {
			  this._read = read;
			  this._indexingService = indexingService;
		 }

		 // LABEL CHANGES

		 public enum LabelChangeType
		 {
			  AddedLabel,
			  RemovedLabel
		 }

		 /// <summary>
		 /// A label has been changed, figure out what updates are needed to tx state.
		 /// </summary>
		 /// <param name="labelId"> The id of the changed label </param>
		 /// <param name="existingPropertyKeyIds"> all property key ids the node has, sorted by id </param>
		 /// <param name="node"> cursor to the node where the change was applied </param>
		 /// <param name="propertyCursor"> cursor to the properties of node </param>
		 /// <param name="changeType"> The type of change event </param>
		 internal virtual void OnLabelChange( int labelId, int[] existingPropertyKeyIds, NodeCursor node, PropertyCursor propertyCursor, LabelChangeType changeType )
		 {
			  Debug.Assert( NoSchemaChangedInTx() );

			  // Check all indexes of the changed label
			  ICollection<SchemaDescriptor> indexes = _indexingService.getRelatedIndexes( new long[]{ labelId }, existingPropertyKeyIds, NODE );
			  if ( indexes.Count > 0 )
			  {
					MutableIntObjectMap<Value> materializedProperties = IntObjectMaps.mutable.empty();
					foreach ( SchemaDescriptor index in indexes )
					{
						 int[] indexPropertyIds = index.Schema().PropertyIds;
						 Value[] values = GetValueTuple( node, propertyCursor, NO_SUCH_PROPERTY_KEY, NO_VALUE, indexPropertyIds, materializedProperties );
						 switch ( changeType )
						 {
						 case Neo4Net.Kernel.Impl.Newapi.IndexTxStateUpdater.LabelChangeType.AddedLabel:
							  _indexingService.validateBeforeCommit( index.Schema(), values );
							  _read.txState().indexDoUpdateEntry(index.Schema(), node.NodeReference(), null, ValueTuple.of(values));
							  break;
						 case Neo4Net.Kernel.Impl.Newapi.IndexTxStateUpdater.LabelChangeType.RemovedLabel:
							  _read.txState().indexDoUpdateEntry(index.Schema(), node.NodeReference(), ValueTuple.of(values), null);
							  break;
						 default:
							  throw new System.InvalidOperationException( changeType + " is not a supported event" );
						 }
					}
			  }
		 }

		 private bool NoSchemaChangedInTx()
		 {
			  return !( _read.txState().hasChanges() && !_read.txState().hasDataChanges() );
		 }

		 //PROPERTY CHANGES

		 internal virtual void OnPropertyAdd( NodeCursor node, PropertyCursor propertyCursor, long[] labels, int propertyKeyId, int[] existingPropertyKeyIds, Value value )
		 {
			  Debug.Assert( NoSchemaChangedInTx() );
			  ICollection<SchemaDescriptor> indexes = _indexingService.getRelatedIndexes( labels, propertyKeyId, NODE );
			  if ( indexes.Count > 0 )
			  {
					MutableIntObjectMap<Value> materializedProperties = IntObjectMaps.mutable.empty();
					NodeSchemaMatcher.OnMatchingSchema(indexes.GetEnumerator(), propertyKeyId, existingPropertyKeyIds, index =>
					{
								Value[] values = GetValueTuple( node, propertyCursor, propertyKeyId, value, index.schema().PropertyIds, materializedProperties );
								_indexingService.validateBeforeCommit( index.schema(), values );
								_read.txState().indexDoUpdateEntry(index.schema(), node.NodeReference(), null, ValueTuple.of(values));
					});
			  }
		 }

		 internal virtual void OnPropertyRemove( NodeCursor node, PropertyCursor propertyCursor, long[] labels, int propertyKeyId, int[] existingPropertyKeyIds, Value value )
		 {
			  Debug.Assert( NoSchemaChangedInTx() );
			  ICollection<SchemaDescriptor> indexes = _indexingService.getRelatedIndexes( labels, propertyKeyId, NODE );
			  if ( indexes.Count > 0 )
			  {
					MutableIntObjectMap<Value> materializedProperties = IntObjectMaps.mutable.empty();
					NodeSchemaMatcher.OnMatchingSchema(indexes.GetEnumerator(), propertyKeyId, existingPropertyKeyIds, index =>
					{
								Value[] values = GetValueTuple( node, propertyCursor, propertyKeyId, value, index.schema().PropertyIds, materializedProperties );
								_read.txState().indexDoUpdateEntry(index.schema(), node.NodeReference(), ValueTuple.of(values), null);
					});
			  }
		 }

		 internal virtual void OnPropertyChange( NodeCursor node, PropertyCursor propertyCursor, long[] labels, int propertyKeyId, int[] existingPropertyKeyIds, Value beforeValue, Value afterValue )
		 {
			  Debug.Assert( NoSchemaChangedInTx() );
			  ICollection<SchemaDescriptor> indexes = _indexingService.getRelatedIndexes( labels, propertyKeyId, NODE );
			  if ( indexes.Count > 0 )
			  {
					MutableIntObjectMap<Value> materializedProperties = IntObjectMaps.mutable.empty();
					NodeSchemaMatcher.OnMatchingSchema(indexes.GetEnumerator(), propertyKeyId, existingPropertyKeyIds, index =>
					{
								int[] propertyIds = index.PropertyIds;
								Value[] valuesAfter = GetValueTuple( node, propertyCursor, propertyKeyId, afterValue, propertyIds, materializedProperties );

								// The valuesBefore tuple is just like valuesAfter, except is has the afterValue instead of the beforeValue
								Value[] valuesBefore = valuesAfter.Clone();
								int k = ArrayUtils.IndexOf( propertyIds, propertyKeyId );
								valuesBefore[k] = beforeValue;

								_indexingService.validateBeforeCommit( index, valuesAfter );
								_read.txState().indexDoUpdateEntry(index, node.NodeReference(), ValueTuple.of(valuesBefore), ValueTuple.of(valuesAfter));
					});
			  }
		 }

		 private Value[] GetValueTuple( NodeCursor node, PropertyCursor propertyCursor, int changedPropertyKeyId, Value changedValue, int[] indexPropertyIds, MutableIntObjectMap<Value> materializedValues )
		 {
			  Value[] values = new Value[indexPropertyIds.Length];
			  int missing = 0;

			  // First get whatever values we already have on the stack, like the value change that provoked this update in the first place
			  // and already loaded values that we can get from the map of materialized values.
			  for ( int k = 0; k < indexPropertyIds.Length; k++ )
			  {
					values[k] = indexPropertyIds[k] == changedPropertyKeyId ? changedValue : materializedValues.get( indexPropertyIds[k] );
					if ( values[k] == null )
					{
						 missing++;
					}
			  }

			  // If we couldn't get all values that we wanted we need to load from the node. While we're loading values
			  // we'll place those values in the map so that other index updates from this change can just used them.
			  if ( missing > 0 )
			  {
					node.Properties( propertyCursor );
					while ( missing > 0 && propertyCursor.Next() )
					{
						 int k = ArrayUtils.IndexOf( indexPropertyIds, propertyCursor.PropertyKey() );
						 if ( k >= 0 && values[k] == null )
						 {
							  int propertyKeyId = indexPropertyIds[k];
							  bool thisIsTheChangedProperty = propertyKeyId == changedPropertyKeyId;
							  values[k] = thisIsTheChangedProperty ? changedValue : propertyCursor.PropertyValue();
							  if ( !thisIsTheChangedProperty )
							  {
									materializedValues.put( propertyKeyId, values[k] );
							  }
							  missing--;
						 }
					}
			  }

			  return values;
		 }
	}

}