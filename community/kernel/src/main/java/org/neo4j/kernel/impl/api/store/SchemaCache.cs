using System;
using System.Collections.Concurrent;
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
namespace Org.Neo4j.Kernel.Impl.Api.store
{
	using MutableIntObjectMap = org.eclipse.collections.api.map.primitive.MutableIntObjectMap;
	using MutableLongObjectMap = org.eclipse.collections.api.map.primitive.MutableLongObjectMap;
	using IntObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.IntObjectHashMap;
	using LongObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.LongObjectHashMap;


	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;
	using SchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptor;
	using SchemaDescriptorPredicates = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptorPredicates;
	using ConstraintDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using IndexProviderMap = Org.Neo4j.Kernel.Impl.Api.index.IndexProviderMap;
	using ConstraintSemantics = Org.Neo4j.Kernel.impl.constraints.ConstraintSemantics;
	using ConstraintRule = Org.Neo4j.Kernel.impl.store.record.ConstraintRule;
	using CapableIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.CapableIndexDescriptor;
	using SchemaRule = Org.Neo4j.Storageengine.Api.schema.SchemaRule;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;

	/// <summary>
	/// A cache of <seealso cref="SchemaRule schema rules"/> as well as enforcement of schema consistency.
	/// Will always reflect the committed state of the schema store.
	/// </summary>
	public class SchemaCache
	{
		 private readonly Lock _cacheUpdateLock;
		 private readonly IndexProviderMap _indexProviderMap;
		 private volatile SchemaCacheState _schemaCacheState;

		 public SchemaCache( ConstraintSemantics constraintSemantics, IEnumerable<SchemaRule> initialRules, IndexProviderMap indexProviderMap )
		 {
			  this._cacheUpdateLock = ( new StampedLock() ).asWriteLock();
			  this._indexProviderMap = indexProviderMap;
			  this._schemaCacheState = new SchemaCacheState( constraintSemantics, initialRules, indexProviderMap );
		 }

		 /// <summary>
		 /// Snapshot constructor. This is only used by the <seealso cref="snapshot()"/> method.
		 /// </summary>
		 private SchemaCache( SchemaCacheState schemaCacheState )
		 {
			  this._cacheUpdateLock = new InaccessibleLock( "Schema cache snapshots are read-only." );
			  this._indexProviderMap = null;
			  this._schemaCacheState = schemaCacheState;
		 }

		 public virtual IEnumerable<CapableIndexDescriptor> IndexDescriptors()
		 {
			  return _schemaCacheState.indexDescriptors();
		 }

		 public virtual IEnumerable<ConstraintRule> ConstraintRules()
		 {
			  return _schemaCacheState.constraintRules();
		 }

		 public virtual bool HasConstraintRule( long? constraintRuleId )
		 {
			  return _schemaCacheState.hasConstraintRule( constraintRuleId );
		 }

		 public virtual bool HasConstraintRule( ConstraintDescriptor descriptor )
		 {
			  return _schemaCacheState.hasConstraintRule( descriptor );
		 }

		 public virtual bool HasIndex( SchemaDescriptor descriptor )
		 {
			  return _schemaCacheState.hasIndex( descriptor );
		 }

		 public virtual IEnumerator<ConstraintDescriptor> Constraints()
		 {
			  return _schemaCacheState.constraints();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public java.util.Iterator<org.neo4j.internal.kernel.api.schema.constraints.ConstraintDescriptor> constraintsForLabel(final int label)
		 public virtual IEnumerator<ConstraintDescriptor> ConstraintsForLabel( int label )
		 {
			  return Iterators.filter( SchemaDescriptorPredicates.hasLabel( label ), Constraints() );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public java.util.Iterator<org.neo4j.internal.kernel.api.schema.constraints.ConstraintDescriptor> constraintsForRelationshipType(final int relTypeId)
		 public virtual IEnumerator<ConstraintDescriptor> ConstraintsForRelationshipType( int relTypeId )
		 {
			  return Iterators.filter( SchemaDescriptorPredicates.hasRelType( relTypeId ), Constraints() );
		 }

		 public virtual IEnumerator<ConstraintDescriptor> ConstraintsForSchema( SchemaDescriptor descriptor )
		 {
			  return Iterators.filter( SchemaDescriptor.equalTo( descriptor ), Constraints() );
		 }

		 public virtual T GetOrCreateDependantState<P, T>( Type type, System.Func<P, T> factory, P parameter )
		 {
				 type = typeof( T );
			  return _schemaCacheState.getOrCreateDependantState( type, factory, parameter );
		 }

		 public virtual void Load( IEnumerable<SchemaRule> rules )
		 {
			  _cacheUpdateLock.@lock();
			  try
			  {
					ConstraintSemantics constraintSemantics = _schemaCacheState.constraintSemantics;
					this._schemaCacheState = new SchemaCacheState( constraintSemantics, rules, _indexProviderMap );
			  }
			  finally
			  {
					_cacheUpdateLock.unlock();
			  }
		 }

		 public virtual void AddSchemaRule( SchemaRule rule )
		 {
			  _cacheUpdateLock.@lock();
			  try
			  {
					SchemaCacheState updatedSchemaState = new SchemaCacheState( _schemaCacheState );
					updatedSchemaState.AddSchemaRule( rule );
					this._schemaCacheState = updatedSchemaState;
			  }
			  finally
			  {
					_cacheUpdateLock.unlock();
			  }
		 }

		 public virtual void RemoveSchemaRule( long id )
		 {
			  _cacheUpdateLock.@lock();
			  try
			  {
					SchemaCacheState updatedSchemaState = new SchemaCacheState( _schemaCacheState );
					updatedSchemaState.RemoveSchemaRule( id );
					this._schemaCacheState = updatedSchemaState;
			  }
			  finally
			  {
					_cacheUpdateLock.unlock();
			  }
		 }

		 public virtual CapableIndexDescriptor IndexDescriptor( SchemaDescriptor descriptor )
		 {
			  return _schemaCacheState.indexDescriptor( descriptor );
		 }

		 public virtual IEnumerator<CapableIndexDescriptor> IndexDescriptorsForLabel( int labelId )
		 {
			  return _schemaCacheState.indexDescriptorsForLabel( labelId );
		 }

		 public virtual IEnumerator<CapableIndexDescriptor> IndexDescriptorsForRelationshipType( int relationshipType )
		 {
			  return _schemaCacheState.indexDescriptorsForRelationshipType( relationshipType );
		 }

		 public virtual IEnumerator<CapableIndexDescriptor> IndexesByProperty( int propertyId )
		 {
			  return _schemaCacheState.indexesByProperty( propertyId );
		 }

		 public virtual CapableIndexDescriptor IndexDescriptorForName( string name )
		 {
			  return _schemaCacheState.indexDescriptorByName( name );
		 }

		 public virtual SchemaCache Snapshot()
		 {
			  return new SchemaCache( _schemaCacheState );
		 }

		 private class SchemaCacheState
		 {
			  internal readonly ConstraintSemantics ConstraintSemantics;
			  internal readonly IndexProviderMap IndexProviderMap;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly ISet<ConstraintDescriptor> ConstraintsConflict;
			  internal readonly MutableLongObjectMap<CapableIndexDescriptor> IndexDescriptorById;
			  internal readonly MutableLongObjectMap<ConstraintRule> ConstraintRuleById;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly IDictionary<SchemaDescriptor, CapableIndexDescriptor> IndexDescriptorsConflict;
			  internal readonly MutableIntObjectMap<ISet<CapableIndexDescriptor>> IndexDescriptorsByLabel;
			  internal readonly MutableIntObjectMap<ISet<CapableIndexDescriptor>> IndexDescriptorsByRelationshipType;
			  internal readonly IDictionary<string, CapableIndexDescriptor> IndexDescriptorsByName;

			  internal readonly IDictionary<Type, object> DependantState;
			  internal readonly MutableIntObjectMap<IList<CapableIndexDescriptor>> IndexByProperty;

			  internal SchemaCacheState( ConstraintSemantics constraintSemantics, IEnumerable<SchemaRule> rules, IndexProviderMap indexProviderMap )
			  {
					this.ConstraintSemantics = constraintSemantics;
					this.IndexProviderMap = indexProviderMap;
					this.ConstraintsConflict = new HashSet<ConstraintDescriptor>();
					this.IndexDescriptorById = new LongObjectHashMap<CapableIndexDescriptor>();
					this.ConstraintRuleById = new LongObjectHashMap<ConstraintRule>();

					this.IndexDescriptorsConflict = new Dictionary<SchemaDescriptor, CapableIndexDescriptor>();
					this.IndexDescriptorsByLabel = new IntObjectHashMap<ISet<CapableIndexDescriptor>>();
					this.IndexDescriptorsByRelationshipType = new IntObjectHashMap<ISet<CapableIndexDescriptor>>();
					this.IndexDescriptorsByName = new Dictionary<string, CapableIndexDescriptor>();
					this.DependantState = new ConcurrentDictionary<Type, object>();
					this.IndexByProperty = new IntObjectHashMap<IList<CapableIndexDescriptor>>();
					Load( rules );
			  }

			  internal SchemaCacheState( SchemaCacheState schemaCacheState )
			  {
					this.ConstraintSemantics = schemaCacheState.ConstraintSemantics;
					this.IndexDescriptorById = LongObjectHashMap.newMap( schemaCacheState.IndexDescriptorById );
					this.ConstraintRuleById = LongObjectHashMap.newMap( schemaCacheState.ConstraintRuleById );
					this.ConstraintsConflict = new HashSet<ConstraintDescriptor>( schemaCacheState.ConstraintsConflict );

					this.IndexDescriptorsConflict = new Dictionary<SchemaDescriptor, CapableIndexDescriptor>( schemaCacheState.IndexDescriptorsConflict );
					this.IndexDescriptorsByLabel = new IntObjectHashMap<ISet<CapableIndexDescriptor>>( schemaCacheState.IndexDescriptorsByLabel.size() );
					schemaCacheState.IndexDescriptorsByLabel.forEachKeyValue( ( k, v ) => IndexDescriptorsByLabel.put( k, new HashSet<>( v ) ) );
					this.IndexDescriptorsByRelationshipType = new IntObjectHashMap<ISet<CapableIndexDescriptor>>( schemaCacheState.IndexDescriptorsByRelationshipType.size() );
					schemaCacheState.IndexDescriptorsByRelationshipType.forEachKeyValue( ( k, v ) => IndexDescriptorsByRelationshipType.put( k, new HashSet<>( v ) ) );
					this.IndexDescriptorsByName = new Dictionary<string, CapableIndexDescriptor>( schemaCacheState.IndexDescriptorsByName );
					this.DependantState = new ConcurrentDictionary<Type, object>();
					this.IndexByProperty = new IntObjectHashMap<IList<CapableIndexDescriptor>>( schemaCacheState.IndexByProperty.size() );
					schemaCacheState.IndexByProperty.forEachKeyValue( ( k, v ) => IndexByProperty.put( k, new List<>( v ) ) );
					this.IndexProviderMap = schemaCacheState.IndexProviderMap;
			  }

			  internal virtual void Load( IEnumerable<SchemaRule> schemaRuleIterator )
			  {
					foreach ( SchemaRule schemaRule in schemaRuleIterator )
					{
						 AddSchemaRule( schemaRule );
					}
			  }

			  internal virtual IEnumerable<CapableIndexDescriptor> IndexDescriptors()
			  {
					return IndexDescriptorById.values();
			  }

			  internal virtual IEnumerable<ConstraintRule> ConstraintRules()
			  {
					return ConstraintRuleById.values();
			  }

			  internal virtual bool HasConstraintRule( long? constraintRuleId )
			  {
					return constraintRuleId != null && ConstraintRuleById.containsKey( constraintRuleId );
			  }

			  internal virtual bool HasConstraintRule( ConstraintDescriptor descriptor )
			  {
					return ConstraintsConflict.Contains( descriptor );
			  }

			  internal virtual bool HasIndex( SchemaDescriptor descriptor )
			  {
					return IndexDescriptorsConflict.ContainsKey( descriptor );
			  }

			  internal virtual IEnumerator<ConstraintDescriptor> Constraints()
			  {
					return ConstraintsConflict.GetEnumerator();
			  }

			  internal virtual CapableIndexDescriptor IndexDescriptor( SchemaDescriptor descriptor )
			  {
					return IndexDescriptorsConflict[descriptor];
			  }

			  internal virtual CapableIndexDescriptor IndexDescriptorByName( string name )
			  {
					return IndexDescriptorsByName[name];
			  }

			  internal virtual IEnumerator<CapableIndexDescriptor> IndexesByProperty( int propertyId )
			  {
					IList<CapableIndexDescriptor> indexes = IndexByProperty.get( propertyId );
					return ( indexes == null ) ? emptyIterator() : indexes.GetEnumerator();
			  }

			  internal virtual IEnumerator<CapableIndexDescriptor> IndexDescriptorsForLabel( int labelId )
			  {
					ISet<CapableIndexDescriptor> forLabel = IndexDescriptorsByLabel.get( labelId );
					return forLabel == null ? emptyIterator() : forLabel.GetEnumerator();
			  }

			  internal virtual IEnumerator<CapableIndexDescriptor> IndexDescriptorsForRelationshipType( int relationshipType )
			  {
					ISet<CapableIndexDescriptor> forLabel = IndexDescriptorsByRelationshipType.get( relationshipType );
					return forLabel == null ? emptyIterator() : forLabel.GetEnumerator();
			  }

			  internal virtual T GetOrCreateDependantState<P, T>( Type type, System.Func<P, T> factory, P parameter )
			  {
					  type = typeof( T );
					return type.cast( DependantState.computeIfAbsent( type, key => factory( parameter ) ) );
			  }

			  internal virtual void AddSchemaRule( SchemaRule rule )
			  {
					if ( rule is ConstraintRule )
					{
						 ConstraintRule constraintRule = ( ConstraintRule ) rule;
						 ConstraintRuleById.put( constraintRule.Id, constraintRule );
						 ConstraintsConflict.Add( ConstraintSemantics.readConstraint( constraintRule ) );
					}
					else if ( rule is StoreIndexDescriptor )
					{
						 CapableIndexDescriptor index = IndexProviderMap.withCapabilities( ( StoreIndexDescriptor ) rule );
						 IndexDescriptorById.put( index.Id, index );
						 SchemaDescriptor schemaDescriptor = index.Schema();
						 IndexDescriptorsConflict[schemaDescriptor] = index;
						 IndexDescriptorsByName[rule.Name] = index;
						 foreach ( int entityTokenId in schemaDescriptor.EntityTokenIds )
						 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
							  ISet<CapableIndexDescriptor> forLabel = IndexDescriptorsByLabel.getIfAbsentPut( entityTokenId, HashSet<object>::new );
							  forLabel.Add( index );
						 }

						 foreach ( int propertyId in index.Schema().PropertyIds )
						 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
							  IList<CapableIndexDescriptor> indexesForProperty = IndexByProperty.getIfAbsentPut( propertyId, List<object>::new );
							  indexesForProperty.Add( index );
						 }
					}
			  }

			  internal virtual void RemoveSchemaRule( long id )
			  {
					if ( ConstraintRuleById.containsKey( id ) )
					{
						 ConstraintRule rule = ConstraintRuleById.remove( id );
						 ConstraintsConflict.remove( rule.ConstraintDescriptor );
					}
					else if ( IndexDescriptorById.containsKey( id ) )
					{
						 CapableIndexDescriptor index = IndexDescriptorById.remove( id );
						 SchemaDescriptor schema = index.Schema();
						 IndexDescriptorsConflict.Remove( schema );
						 IndexDescriptorsByName.Remove( index.Name, index );

						 foreach ( int entityTokenId in Schema.EntityTokenIds )
						 {
							  ISet<CapableIndexDescriptor> forLabel = IndexDescriptorsByLabel.get( entityTokenId );
							  /* Previously, a bug made it possible to create fulltext indexes with repeated labels or relationship types
							     which would cause us to try and remove the same entity token twice which could cause a NPE if the 'forLabel'
							     set would be empty after the first removal such that the set would be completely removed from 'indexDescriptorsByLabel'.
							     Fixed as of 3.5.10 */
							  if ( forLabel != null )
							  {
									forLabel.remove( index );
									if ( forLabel.Count == 0 )
									{
										 IndexDescriptorsByLabel.remove( entityTokenId );
									}
							  }
						 }

						 foreach ( int propertyId in index.Schema().PropertyIds )
						 {
							  IList<CapableIndexDescriptor> forProperty = IndexByProperty.get( propertyId );
							  forProperty.Remove( index );
							  if ( forProperty.Count == 0 )
							  {
									IndexByProperty.remove( propertyId );
							  }
						 }
					}
			  }
		 }
	}

}