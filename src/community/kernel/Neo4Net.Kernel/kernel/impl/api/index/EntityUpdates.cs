using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

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
namespace Neo4Net.Kernel.Impl.Api.index
{
	using IntIterator = org.eclipse.collections.api.iterator.IntIterator;
	using MutableIntObjectMap = org.eclipse.collections.api.map.primitive.MutableIntObjectMap;
	using MutableIntSet = org.eclipse.collections.api.set.primitive.MutableIntSet;
	using IntObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.IntObjectHashMap;
	using IntHashSet = org.eclipse.collections.impl.set.mutable.primitive.IntHashSet;


	using PrimitiveArrays = Neo4Net.Collections.PrimitiveArrays;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;
	using SchemaDescriptorSupplier = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptorSupplier;
	using Neo4Net.Kernel.Api.Index;
	using IEntityType = Neo4Net.Storageengine.Api.EntityType;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Internal.kernel.api.schema.SchemaDescriptor_PropertySchemaType.COMPLETE_ALL_TOKENS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.api.index.EntityUpdates.PropertyValueType.Changed;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.api.index.EntityUpdates.PropertyValueType.NoValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.api.index.EntityUpdates.PropertyValueType.UnChanged;

	/// <summary>
	/// Subclasses of this represent events related to property changes due to IEntity addition, deletion or update.
	/// This is of use in populating indexes that might be relevant to label/reltype and property combinations.
	/// </summary>
	public class IEntityUpdates : PropertyLoader_PropertyLoadSink
	{
		 private readonly long _entityId;
		 private static readonly long[] _emptyLongArray = new long[0];

		 // ASSUMPTION: these long arrays are actually sorted sets
		 private long[] _entityTokensBefore;
		 private long[] _entityTokensAfter;
		 private readonly bool _propertyListComplete;
		 private readonly MutableIntObjectMap<PropertyValue> _knownProperties;
		 private int[] _propertyKeyIds;
		 private int _propertyKeyIdsCursor;
		 private bool _hasLoadedAdditionalProperties;

		 public class Builder
		 {
			  internal IEntityUpdates Updates;

			  internal Builder( IEntityUpdates updates )
			  {
					this.Updates = updates;
			  }

			  public virtual Builder Added( int propertyKeyId, Value value )
			  {
					Updates.put( propertyKeyId, IEntityUpdates.After( value ) );
					return this;
			  }

			  public virtual Builder Removed( int propertyKeyId, Value value )
			  {
					Updates.put( propertyKeyId, IEntityUpdates.Before( value ) );
					return this;
			  }

			  public virtual Builder Changed( int propertyKeyId, Value before, Value after )
			  {
					Updates.put( propertyKeyId, IEntityUpdates.changed( before, after ) );
					return this;
			  }

			  public virtual Builder Existing( int propertyKeyId, Value value )
			  {
					Updates.put( propertyKeyId, IEntityUpdates.Unchanged( value ) );
					return this;
			  }

			  public virtual Builder WithTokens( params long[] IEntityTokens )
			  {
					this.Updates.entityTokensBefore = IEntityTokens;
					this.Updates.entityTokensAfter = IEntityTokens;
					return this;
			  }

			  public virtual Builder WithTokensAfter( params long[] IEntityTokensAfter )
			  {
					this.Updates.entityTokensAfter = IEntityTokensAfter;
					return this;
			  }

			  public virtual IEntityUpdates Build()
			  {
					return Updates;
			  }
		 }

		 private void Put( int propertyKeyId, PropertyValue propertyValue )
		 {
			  PropertyValue existing = _knownProperties.put( propertyKeyId, propertyValue );
			  if ( existing == null )
			  {
					if ( _propertyKeyIdsCursor >= _propertyKeyIds.Length )
					{
						 _propertyKeyIds = Arrays.copyOf( _propertyKeyIds, _propertyKeyIdsCursor * 2 );
					}
					_propertyKeyIds[_propertyKeyIdsCursor++] = propertyKeyId;
			  }
		 }

		 public static Builder ForEntity( long IEntityId, bool propertyListIsComplete )
		 {
			  return new Builder( new IEntityUpdates( IEntityId, _emptyLongArray, _emptyLongArray, propertyListIsComplete ) );
		 }

		 private IEntityUpdates( long IEntityId, long[] IEntityTokensBefore, long[] IEntityTokensAfter, bool propertyListComplete )
		 {
			  this._entityId = IEntityId;
			  this._entityTokensBefore = IEntityTokensBefore;
			  this._entityTokensAfter = IEntityTokensAfter;
			  this._propertyListComplete = propertyListComplete;
			  this._knownProperties = new IntObjectHashMap<PropertyValue>();
			  this._propertyKeyIds = new int[8];
		 }

		 public long IEntityId
		 {
			 get
			 {
				  return _entityId;
			 }
		 }

		 internal virtual long[] IEntityTokensChanged()
		 {
			  return PrimitiveArrays.symmetricDifference( _entityTokensBefore, _entityTokensAfter );
		 }

		 internal virtual long[] IEntityTokensUnchanged()
		 {
			  return PrimitiveArrays.intersect( _entityTokensBefore, _entityTokensAfter );
		 }

		 internal virtual int[] PropertiesChanged()
		 {
			  Debug.Assert( !_hasLoadedAdditionalProperties, "Calling propertiesChanged() is not valid after non-changed " + );
																	"properties have already been loaded.";
			  Arrays.sort( _propertyKeyIds, 0, _propertyKeyIdsCursor );
			  return _propertyKeyIdsCursor == _propertyKeyIds.Length ? _propertyKeyIds : Arrays.copyOf( _propertyKeyIds, _propertyKeyIdsCursor );
		 }

		 /// <returns> whether or not the list provided from <seealso cref="propertiesChanged()"/> is the complete list of properties on this node.
		 /// If {@code false} then the list may contain some properties, whereas there may be other unloaded properties on the persisted existing node. </returns>
		 internal virtual bool PropertyListComplete
		 {
			 get
			 {
				  return _propertyListComplete;
			 }
		 }

		 public override void OnProperty( int propertyId, Value value )
		 {
			  Put( propertyId, Unchanged( value ) );
		 }

		 /// <summary>
		 /// Matches the provided schema descriptors to the node updates in this object, and generates an IndexEntryUpdate
		 /// for any index that needs to be updated.
		 /// 
		 /// Note that unless this object contains a full representation of the node state after the update, the results
		 /// from this methods will not be correct. In that case, use the propertyLoader variant.
		 /// </summary>
		 /// <param name="indexKeys"> The index keys to generate entry updates for </param>
		 /// <returns> IndexEntryUpdates for all relevant index keys </returns>
		 public virtual IEnumerable<IndexEntryUpdate<INDEX_KEY>> ForIndexKeys<INDEX_KEY>( IEnumerable<INDEX_KEY> indexKeys ) where INDEX_KEY : Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptorSupplier
		 {
			  IEnumerable<INDEX_KEY> potentiallyRelevant = Iterables.filter( indexKey => AtLeastOneRelevantChange( indexKey.schema() ), indexKeys );

			  return GatherUpdatesForPotentials( potentiallyRelevant );
		 }

		 /// <summary>
		 /// Matches the provided schema descriptors to the IEntity updates in this object, and generates an IndexEntryUpdate
		 /// for any index that needs to be updated.
		 /// 
		 /// In some cases the updates to an IEntity are not enough to determine whether some index should be affected. For
		 /// example if we have and index of label :A and property p1, and :A is added to this node, we cannot say whether
		 /// this should affect the index unless we know if this node has property p1. This get even more complicated for
		 /// composite indexes. To solve this problem, a propertyLoader is used to load any additional properties needed to
		 /// make these calls.
		 /// </summary>
		 /// <param name="indexKeys"> The index keys to generate entry updates for </param>
		 /// <param name="propertyLoader"> The property loader used to fetch needed additional properties </param>
		 /// <param name="type"> IEntityType of the indexes </param>
		 /// <returns> IndexEntryUpdates for all relevant index keys </returns>
		 public virtual IEnumerable<IndexEntryUpdate<INDEX_KEY>> ForIndexKeys<INDEX_KEY>( IEnumerable<INDEX_KEY> indexKeys, PropertyLoader propertyLoader, IEntityType type ) where INDEX_KEY : Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptorSupplier
		 {
			  IList<INDEX_KEY> potentiallyRelevant = new List<INDEX_KEY>();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.primitive.MutableIntSet additionalPropertiesToLoad = new org.eclipse.collections.impl.set.mutable.primitive.IntHashSet();
			  MutableIntSet additionalPropertiesToLoad = new IntHashSet();

			  foreach ( INDEX_KEY indexKey in indexKeys )
			  {
					if ( AtLeastOneRelevantChange( indexKey.schema() ) )
					{
						 potentiallyRelevant.Add( indexKey );
						 GatherPropsToLoad( indexKey.schema(), additionalPropertiesToLoad );
					}
			  }

			  if ( !additionalPropertiesToLoad.Empty )
			  {
					LoadProperties( propertyLoader, additionalPropertiesToLoad, type );
			  }

			  return GatherUpdatesForPotentials( potentiallyRelevant );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("ConstantConditions") private <INDEX_KEY extends org.Neo4Net.internal.kernel.api.schema.SchemaDescriptorSupplier> Iterable<org.Neo4Net.kernel.api.index.IndexEntryUpdate<INDEX_KEY>> gatherUpdatesForPotentials(Iterable<INDEX_KEY> potentiallyRelevant)
		 private IEnumerable<IndexEntryUpdate<INDEX_KEY>> GatherUpdatesForPotentials<INDEX_KEY>( IEnumerable<INDEX_KEY> potentiallyRelevant ) where INDEX_KEY : Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptorSupplier
		 {
			  IList<IndexEntryUpdate<INDEX_KEY>> indexUpdates = new List<IndexEntryUpdate<INDEX_KEY>>();
			  foreach ( INDEX_KEY indexKey in potentiallyRelevant )
			  {
					SchemaDescriptor schema = indexKey.schema();
					bool relevantBefore = relevantBefore( schema );
					bool relevantAfter = relevantAfter( schema );
					int[] propertyIds = Schema.PropertyIds;
					if ( relevantBefore && !relevantAfter )
					{
						 indexUpdates.Add( IndexEntryUpdate.remove( _entityId, indexKey, ValuesBefore( propertyIds ) ) );
					}
					else if ( !relevantBefore && relevantAfter )
					{
						 indexUpdates.Add( IndexEntryUpdate.add( _entityId, indexKey, ValuesAfter( propertyIds ) ) );
					}
					else if ( relevantBefore && relevantAfter )
					{
						 if ( ValuesChanged( propertyIds, Schema.propertySchemaType() ) )
						 {
							  indexUpdates.Add( IndexEntryUpdate.change( _entityId, indexKey, ValuesBefore( propertyIds ), ValuesAfter( propertyIds ) ) );
						 }
					}
			  }
			  return indexUpdates;
		 }

		 private bool RelevantBefore( SchemaDescriptor schema )
		 {
			  return Schema.isAffected( _entityTokensBefore ) && HasPropsBefore( Schema.PropertyIds, Schema.propertySchemaType() );
		 }

		 private bool RelevantAfter( SchemaDescriptor schema )
		 {
			  return Schema.isAffected( _entityTokensAfter ) && HasPropsAfter( Schema.PropertyIds, Schema.propertySchemaType() );
		 }

		 private void LoadProperties( PropertyLoader propertyLoader, MutableIntSet additionalPropertiesToLoad, IEntityType type )
		 {
			  _hasLoadedAdditionalProperties = true;
			  propertyLoader.LoadProperties( _entityId, type, additionalPropertiesToLoad, this );

			  // loadProperties removes loaded properties from the input set, so the remaining ones were not on the node
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.iterator.IntIterator propertiesWithNoValue = additionalPropertiesToLoad.intIterator();
			  IntIterator propertiesWithNoValue = additionalPropertiesToLoad.intIterator();
			  while ( propertiesWithNoValue.hasNext() )
			  {
					Put( propertiesWithNoValue.next(), _noValue );
			  }
		 }

		 private void GatherPropsToLoad( SchemaDescriptor schema, MutableIntSet target )
		 {
			  foreach ( int propertyId in Schema.PropertyIds )
			  {
					if ( _knownProperties.get( propertyId ) == null )
					{
						 target.add( propertyId );
					}
			  }
		 }

		 private bool AtLeastOneRelevantChange( SchemaDescriptor schema )
		 {
			  bool affectedBefore = Schema.isAffected( _entityTokensBefore );
			  bool affectedAfter = Schema.isAffected( _entityTokensAfter );
			  if ( affectedBefore && affectedAfter )
			  {
					foreach ( int propertyId in Schema.PropertyIds )
					{
						 if ( _knownProperties.containsKey( propertyId ) )
						 {
							  return true;
						 }
					}
					return false;
			  }
			  return affectedBefore || affectedAfter;
		 }

		 private bool HasPropsBefore( int[] propertyIds, Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor_PropertySchemaType propertySchemaType )
		 {
			  bool found = false;
			  foreach ( int propertyId in propertyIds )
			  {
					PropertyValue propertyValue = _knownProperties.getIfAbsent( propertyId, () => _noValue );
					if ( !propertyValue.HasBefore() )
					{
						 if ( propertySchemaType == COMPLETE_ALL_TOKENS )
						 {
							  return false;
						 }
					}
					else
					{
						 found = true;
					}
			  }
			  return found;
		 }

		 private bool HasPropsAfter( int[] propertyIds, Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor_PropertySchemaType propertySchemaType )
		 {
			  bool found = false;
			  foreach ( int propertyId in propertyIds )
			  {
					PropertyValue propertyValue = _knownProperties.getIfAbsent( propertyId, () => _noValue );
					if ( !propertyValue.HasAfter() )
					{
						 if ( propertySchemaType == COMPLETE_ALL_TOKENS )
						 {
							  return false;
						 }
					}
					else
					{
						 found = true;
					}
			  }
			  return found;
		 }

		 private Value[] ValuesBefore( int[] propertyIds )
		 {
			  Value[] values = new Value[propertyIds.Length];
			  for ( int i = 0; i < propertyIds.Length; i++ )
			  {
					values[i] = _knownProperties.get( propertyIds[i] ).before;
			  }
			  return values;
		 }

		 private Value[] ValuesAfter( int[] propertyIds )
		 {
			  Value[] values = new Value[propertyIds.Length];
			  for ( int i = 0; i < propertyIds.Length; i++ )
			  {
					PropertyValue propertyValue = _knownProperties.get( propertyIds[i] );
					values[i] = propertyValue == null ? null : propertyValue.After;
			  }
			  return values;
		 }

		 /// <summary>
		 /// This method should only be called in a context where you know that your IEntity is relevant both before and after
		 /// </summary>
		 private bool ValuesChanged( int[] propertyIds, Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor_PropertySchemaType propertySchemaType )
		 {
			  if ( propertySchemaType == COMPLETE_ALL_TOKENS )
			  {
					// In the case of indexes were all entries must have all indexed tokens, one of the properties must have changed for us to generate a change.
					foreach ( int propertyId in propertyIds )
					{
						 if ( _knownProperties.get( propertyId ).type == Changed )
						 {
							  return true;
						 }
					}
					return false;
			  }
			  else
			  {
					// In the case of indexes were we index incomplete index entries, we need to update as long as _anything_ happened to one of the indexed properties.
					foreach ( int propertyId in propertyIds )
					{
						 PropertyValueType type = _knownProperties.get( propertyId ).type;
						 if ( type != UnChanged && type != NoValue )
						 {
							  return true;
						 }
					}
					return false;
			  }
		 }

		 public override string ToString()
		 {
			  StringBuilder result = ( new StringBuilder( this.GetType().Name ) ).Append("[").Append(_entityId);
			  result.Append( ", IEntityTokensBefore:" ).Append( Arrays.ToString( _entityTokensBefore ) );
			  result.Append( ", IEntityTokensAfter:" ).Append( Arrays.ToString( _entityTokensAfter ) );
			  _knownProperties.forEachKeyValue((key, propertyValue) =>
			  {
				result.Append( ", " );
				result.Append( key );
				result.Append( " -> " );
				result.Append( propertyValue );
			  });
			  return result.Append( ']' ).ToString();
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }
			  IEntityUpdates that = ( IEntityUpdates ) o;
			  return _entityId == that._entityId && Arrays.Equals( _entityTokensBefore, that._entityTokensBefore ) && Arrays.Equals( _entityTokensAfter, that._entityTokensAfter );
		 }

		 public override int GetHashCode()
		 {

			  int result = Objects.hash( _entityId );
			  result = 31 * result + Arrays.GetHashCode( _entityTokensBefore );
			  result = 31 * result + Arrays.GetHashCode( _entityTokensAfter );
			  return result;
		 }

		 internal enum PropertyValueType
		 {
			  NoValue,
			  Before,
			  After,
			  UnChanged,
			  Changed
		 }

		 private class PropertyValue
		 {
			  internal readonly Value Before;
			  internal readonly Value After;
			  internal readonly PropertyValueType Type;

			  internal PropertyValue( Value before, Value after, PropertyValueType type )
			  {
					this.Before = before;
					this.After = after;
					this.Type = type;
			  }

			  internal virtual bool HasBefore()
			  {
					return Before != null;
			  }

			  internal virtual bool HasAfter()
			  {
					return After != null;
			  }

			  public override string ToString()
			  {
					switch ( Type )
					{
					case Neo4Net.Kernel.Impl.Api.index.EntityUpdates.PropertyValueType.NoValue:
						return "NoValue";
					case Neo4Net.Kernel.Impl.Api.index.EntityUpdates.PropertyValueType.Before:
						return format( "Before(%s)", Before );
					case Neo4Net.Kernel.Impl.Api.index.EntityUpdates.PropertyValueType.After:
						return format( "After(%s)", After );
					case Neo4Net.Kernel.Impl.Api.index.EntityUpdates.PropertyValueType.UnChanged:
						return format( "UnChanged(%s)", After );
					case Neo4Net.Kernel.Impl.Api.index.EntityUpdates.PropertyValueType.Changed:
						return format( "Changed(from=%s, to=%s)", Before, After );
					default:
						throw new System.InvalidOperationException( "This cannot happen!" );
					}
			  }

			  public override bool Equals( object o )
			  {
					if ( this == o )
					{
						 return true;
					}
					if ( o == null || this.GetType() != o.GetType() )
					{
						 return false;
					}

					PropertyValue that = ( PropertyValue ) o;
					if ( Type != that.Type )
					{
						 return false;
					}

					switch ( Type )
					{
					case Neo4Net.Kernel.Impl.Api.index.EntityUpdates.PropertyValueType.NoValue:
						return true;
					case Neo4Net.Kernel.Impl.Api.index.EntityUpdates.PropertyValueType.Before:
						return Before.Equals( that.Before );
					case Neo4Net.Kernel.Impl.Api.index.EntityUpdates.PropertyValueType.After:
						return After.Equals( that.After );
					case Neo4Net.Kernel.Impl.Api.index.EntityUpdates.PropertyValueType.UnChanged:
						return After.Equals( that.After );
					case Neo4Net.Kernel.Impl.Api.index.EntityUpdates.PropertyValueType.Changed:
						return Before.Equals( that.Before ) && After.Equals( that.After );
					default:
						throw new System.InvalidOperationException( "This cannot happen!" );
					}
			  }

			  public override int GetHashCode()
			  {
					int result = Before != null ? Before.GetHashCode() : 0;
					result = 31 * result + ( After != null ? After.GetHashCode() : 0 );
					result = 31 * result + ( Type != null ? Type.GetHashCode() : 0 );
					return result;
			  }
		 }

		 private static PropertyValue _noValue = new PropertyValue( null, null, NoValue );

		 private static PropertyValue Before( Value value )
		 {
			  return new PropertyValue( value, null, PropertyValueType.Before );
		 }

		 private static PropertyValue After( Value value )
		 {
			  return new PropertyValue( null, value, PropertyValueType.After );
		 }

		 private static PropertyValue Unchanged( Value value )
		 {
			  return new PropertyValue( value, value, PropertyValueType.UnChanged );
		 }

		 private static PropertyValue Changed( Value before, Value after )
		 {
			  return new PropertyValue( before, after, PropertyValueType.Changed );
		 }
	}

}