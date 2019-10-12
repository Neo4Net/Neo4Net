using System;
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
namespace Neo4Net.Kernel.Impl.Api.index
{
	using LongObjectProcedure = org.eclipse.collections.api.block.procedure.primitive.LongObjectProcedure;
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using LongObjectMap = org.eclipse.collections.api.map.primitive.LongObjectMap;
	using MutableLongObjectMap = org.eclipse.collections.api.map.primitive.MutableLongObjectMap;
	using MutableObjectLongMap = org.eclipse.collections.api.map.primitive.MutableObjectLongMap;
	using Functions = org.eclipse.collections.impl.block.factory.Functions;
	using LongObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.LongObjectHashMap;
	using ObjectLongHashMap = org.eclipse.collections.impl.map.mutable.primitive.ObjectLongHashMap;


	using SchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.SchemaDescriptor;
	using SchemaDescriptorSupplier = Neo4Net.@internal.Kernel.Api.schema.SchemaDescriptorSupplier;
	using IndexBackedConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.IndexBackedConstraintDescriptor;
	using ConstraintRule = Neo4Net.Kernel.impl.store.record.ConstraintRule;
	using EntityType = Neo4Net.Storageengine.Api.EntityType;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;

	/// <summary>
	/// Bundles various mappings to IndexProxy. Used by IndexingService via IndexMapReference.
	/// 
	/// IndexingService is expected to either make a copy before making any changes or update this
	/// while being single threaded.
	/// </summary>
	public sealed class IndexMap : ICloneable
	{
		 private readonly MutableLongObjectMap<IndexProxy> _indexesById;
		 private readonly MutableLongObjectMap<IndexBackedConstraintDescriptor> _uniquenessConstraintsById;
		 private readonly IDictionary<SchemaDescriptor, IndexProxy> _indexesByDescriptor;
		 private readonly MutableObjectLongMap<SchemaDescriptor> _indexIdsByDescriptor;
		 private readonly SchemaDescriptorLookupSet<SchemaDescriptor> _descriptorsByLabelThenProperty;
		 private readonly SchemaDescriptorLookupSet<SchemaDescriptor> _descriptorsByReltypeThenProperty;
		 private readonly SchemaDescriptorLookupSet<IndexBackedConstraintDescriptor> _constraintsByLabelThenProperty;
		 private readonly SchemaDescriptorLookupSet<IndexBackedConstraintDescriptor> _constraintsByRelTypeThenProperty;

		 public IndexMap() : this(new LongObjectHashMap<IndexProxy>(), new Dictionary<SchemaDescriptor, IndexProxy>(), new ObjectLongHashMap<SchemaDescriptor>(), new LongObjectHashMap<IndexBackedConstraintDescriptor>())
		 {
		 }

		 private IndexMap( MutableLongObjectMap<IndexProxy> indexesById, IDictionary<SchemaDescriptor, IndexProxy> indexesByDescriptor, MutableObjectLongMap<SchemaDescriptor> indexIdsByDescriptor, MutableLongObjectMap<IndexBackedConstraintDescriptor> uniquenessConstraintsById )
		 {
			  this._indexesById = indexesById;
			  this._indexesByDescriptor = indexesByDescriptor;
			  this._indexIdsByDescriptor = indexIdsByDescriptor;
			  this._uniquenessConstraintsById = uniquenessConstraintsById;
			  this._descriptorsByLabelThenProperty = new SchemaDescriptorLookupSet<SchemaDescriptor>();
			  this._descriptorsByReltypeThenProperty = new SchemaDescriptorLookupSet<SchemaDescriptor>();
			  foreach ( SchemaDescriptor schema in indexesByDescriptor.Keys )
			  {
					AddDescriptorToLookups( schema );
			  }
			  this._constraintsByLabelThenProperty = new SchemaDescriptorLookupSet<IndexBackedConstraintDescriptor>();
			  this._constraintsByRelTypeThenProperty = new SchemaDescriptorLookupSet<IndexBackedConstraintDescriptor>();
			  foreach ( IndexBackedConstraintDescriptor constraint in uniquenessConstraintsById.values() )
			  {
					AddConstraintToLookups( constraint );
			  }
		 }

		 public IndexProxy GetIndexProxy( long indexId )
		 {
			  return _indexesById.get( indexId );
		 }

		 public IndexProxy GetIndexProxy( SchemaDescriptor descriptor )
		 {
			  return _indexesByDescriptor[descriptor];
		 }

		 public long GetIndexId( SchemaDescriptor descriptor )
		 {
			  return _indexIdsByDescriptor.get( descriptor );
		 }

		 public void PutIndexProxy( IndexProxy indexProxy )
		 {
			  StoreIndexDescriptor descriptor = indexProxy.Descriptor;
			  SchemaDescriptor schema = descriptor.Schema();
			  _indexesById.put( descriptor.Id, indexProxy );
			  _indexesByDescriptor[schema] = indexProxy;
			  _indexIdsByDescriptor.put( schema, descriptor.Id );
			  AddDescriptorToLookups( schema );
		 }

		 internal IndexProxy RemoveIndexProxy( long indexId )
		 {
			  IndexProxy removedProxy = _indexesById.remove( indexId );
			  if ( removedProxy == null )
			  {
					return null;
			  }

			  SchemaDescriptor schema = removedProxy.Descriptor.schema();
			  _indexesByDescriptor.Remove( schema );
			  SelectIndexesByEntityType( Schema.entityType() ).remove(schema);

			  return removedProxy;
		 }

		 internal void ForEachIndexProxy( LongObjectProcedure<IndexProxy> consumer )
		 {
			  _indexesById.forEachKeyValue( consumer );
		 }

		 internal IEnumerable<IndexProxy> AllIndexProxies
		 {
			 get
			 {
				  return _indexesById.values();
			 }
		 }

		 internal void PutUniquenessConstraint( ConstraintRule rule )
		 {
			  IndexBackedConstraintDescriptor constraintDescriptor = ( IndexBackedConstraintDescriptor ) rule.ConstraintDescriptor;
			  _uniquenessConstraintsById.put( rule.Id, constraintDescriptor );
			  _constraintsByLabelThenProperty.add( constraintDescriptor );
		 }

		 internal void RemoveUniquenessConstraint( long constraintId )
		 {
			  IndexBackedConstraintDescriptor constraint = _uniquenessConstraintsById.remove( constraintId );
			  if ( constraint != null )
			  {
					SelectConstraintsByEntityType( constraint.Schema().entityType() ).remove(constraint);
			  }
		 }

		 private SchemaDescriptorLookupSet<IndexBackedConstraintDescriptor> SelectConstraintsByEntityType( EntityType entityType )
		 {
			  switch ( entityType.innerEnumValue )
			  {
			  case EntityType.InnerEnum.NODE:
					return _constraintsByLabelThenProperty;
			  case EntityType.InnerEnum.RELATIONSHIP:
					return _constraintsByRelTypeThenProperty;
			  default:
					throw new System.ArgumentException( "Unknown entity type " + entityType );
			  }
		 }

		 private SchemaDescriptorLookupSet<SchemaDescriptor> SelectIndexesByEntityType( EntityType entityType )
		 {
			  switch ( entityType.innerEnumValue )
			  {
			  case EntityType.InnerEnum.NODE:
					return _descriptorsByLabelThenProperty;
			  case EntityType.InnerEnum.RELATIONSHIP:
					return _descriptorsByReltypeThenProperty;
			  default:
					throw new System.ArgumentException( "Unknown entity type " + entityType );
			  }
		 }

		 internal bool HasRelatedSchema( long[] labels, int propertyKey, EntityType entityType )
		 {
			  return SelectIndexesByEntityType( entityType ).has( labels, propertyKey ) || SelectConstraintsByEntityType( entityType ).has( labels, propertyKey );
		 }

		 internal bool HasRelatedSchema( int label, EntityType entityType )
		 {
			  return SelectIndexesByEntityType( entityType ).has( label ) || SelectConstraintsByEntityType( entityType ).has( label );
		 }

		 /// <summary>
		 /// Get all descriptors that would be affected by changes in the input labels and/or properties. The returned
		 /// descriptors are guaranteed to contain all affected indexes, but might also contain unaffected indexes as
		 /// we cannot provide matching without checking unaffected properties for composite indexes.
		 /// </summary>
		 /// <param name="changedEntityTokens"> set of labels that have changed </param>
		 /// <param name="unchangedEntityTokens"> set of labels that are unchanged </param>
		 /// <param name="sortedProperties"> sorted list of properties </param>
		 /// <param name="entityType"> type of indexes to get </param>
		 /// <returns> set of SchemaDescriptors describing the potentially affected indexes </returns>
		 public ISet<SchemaDescriptor> GetRelatedIndexes( long[] changedEntityTokens, long[] unchangedEntityTokens, int[] sortedProperties, bool propertyListIsComplete, EntityType entityType )
		 {
			  return GetRelatedDescriptors( SelectIndexesByEntityType( entityType ), changedEntityTokens, unchangedEntityTokens, sortedProperties, propertyListIsComplete );
		 }

		 /// <summary>
		 /// Get all uniqueness constraints that would be affected by changes in the input labels and/or properties. The returned
		 /// set is guaranteed to contain all affected constraints, but might also contain unaffected constraints as
		 /// we cannot provide matching without checking unaffected properties for composite indexes.
		 /// </summary>
		 /// <param name="changedEntityTokens"> set of labels that have changed </param>
		 /// <param name="unchangedEntityTokens"> set of labels that are unchanged </param>
		 /// <param name="sortedProperties"> sorted list of properties </param>
		 /// <param name="entityType"> type of indexes to get </param>
		 /// <returns> set of SchemaDescriptors describing the potentially affected indexes </returns>
		 public ISet<IndexBackedConstraintDescriptor> GetRelatedConstraints( long[] changedEntityTokens, long[] unchangedEntityTokens, int[] sortedProperties, bool propertyListIsComplete, EntityType entityType )
		 {
			  return GetRelatedDescriptors( SelectConstraintsByEntityType( entityType ), changedEntityTokens, unchangedEntityTokens, sortedProperties, propertyListIsComplete );
		 }

		 /// <param name="changedLabels"> set of labels that have changed </param>
		 /// <param name="unchangedLabels"> set of labels that are unchanged </param>
		 /// <param name="sortedProperties"> set of properties </param>
		 /// <param name="propertyListIsComplete"> whether or not the property list is complete. For CREATE/DELETE the list is complete, but may not be for UPDATEs. </param>
		 /// <returns> set of SchemaDescriptors describing the potentially affected indexes </returns>
		 private ISet<T> GetRelatedDescriptors<T>( SchemaDescriptorLookupSet<T> set, long[] changedLabels, long[] unchangedLabels, int[] sortedProperties, bool propertyListIsComplete ) where T : Neo4Net.@internal.Kernel.Api.schema.SchemaDescriptorSupplier
		 {
			  if ( set.Empty )
			  {
					return Collections.emptySet();
			  }

			  ISet<T> descriptors = new HashSet<T>();
			  if ( propertyListIsComplete )
			  {
					set.MatchingDescriptorsForCompleteListOfProperties( descriptors, changedLabels, sortedProperties );
			  }
			  else
			  {
					// At the time of writing this the commit process won't load the complete list of property keys for an entity.
					// Because of this the matching cannot be as precise as if the complete list was known.
					// Anyway try to make the best out of it and narrow down the list of potentially related indexes as much as possible.
					if ( sortedProperties.Length == 0 )
					{
						 // Only labels changed. Since we don't know which properties this entity has let's include all indexes for the changed labels.
						 set.MatchingDescriptors( descriptors, changedLabels );
					}
					else if ( changedLabels.Length == 0 )
					{
						 // Only properties changed. Since we don't know which other properties this entity has let's include all indexes
						 // for the (unchanged) labels on this entity that has any match on any of the changed properties.
						 set.MatchingDescriptorsForPartialListOfProperties( descriptors, unchangedLabels, sortedProperties );
					}
					else
					{
						 // Both labels and properties changed.
						 // All indexes for the changed labels must be included.
						 // Also include all indexes for any of the changed or unchanged labels that has any match on any of the changed properties.
						 set.MatchingDescriptors( descriptors, changedLabels );
						 set.MatchingDescriptorsForPartialListOfProperties( descriptors, unchangedLabels, sortedProperties );
					}
			  }
			  return descriptors;
		 }

		 public override IndexMap Clone()
		 {
			  return new IndexMap( LongObjectHashMap.newMap( _indexesById ), CloneMap( _indexesByDescriptor ), new ObjectLongHashMap<>( _indexIdsByDescriptor ), LongObjectHashMap.newMap( _uniquenessConstraintsById ) );
		 }

		 public IEnumerator<SchemaDescriptor> Descriptors()
		 {
			  return _indexesByDescriptor.Keys.GetEnumerator();
		 }

		 public LongIterator IndexIds()
		 {
			  return _indexesById.Keys.longIterator();
		 }

		 public int Size()
		 {
			  return _indexesById.size();
		 }

		 // HELPERS

		 private IDictionary<K, V> CloneMap<K, V>( IDictionary<K, V> map )
		 {
			  IDictionary<K, V> shallowCopy = new Dictionary<K, V>( map.Count );
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			  shallowCopy.putAll( map );
			  return shallowCopy;
		 }

		 private void AddDescriptorToLookups( SchemaDescriptor schema )
		 {
			  SelectIndexesByEntityType( Schema.entityType() ).add(schema);
		 }

		 private void AddConstraintToLookups( IndexBackedConstraintDescriptor constraint )
		 {
			  SelectConstraintsByEntityType( constraint.Schema().entityType() ).add(constraint);
		 }

		 private static IDictionary<SchemaDescriptor, IndexProxy> IndexesByDescriptor( LongObjectMap<IndexProxy> indexesById )
		 {
			  return indexesById.toMap( indexProxy => indexProxy.Descriptor.schema(), Functions.identity() );
		 }

		 private static MutableObjectLongMap<SchemaDescriptor> IndexIdsByDescriptor( LongObjectMap<IndexProxy> indexesById )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.map.primitive.MutableObjectLongMap<org.neo4j.internal.kernel.api.schema.SchemaDescriptor> map = new org.eclipse.collections.impl.map.mutable.primitive.ObjectLongHashMap<>(indexesById.size());
			  MutableObjectLongMap<SchemaDescriptor> map = new ObjectLongHashMap<SchemaDescriptor>( indexesById.size() );
			  indexesById.forEachKeyValue( ( id, indexProxy ) => map.put( indexProxy.Descriptor.schema(), id ) );
			  return map;
		 }
	}

}