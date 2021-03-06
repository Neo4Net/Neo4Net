﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Impl.Api.state
{
	using IntIterable = org.eclipse.collections.api.IntIterable;
	using LongObjectMap = org.eclipse.collections.api.map.primitive.LongObjectMap;
	using MutableLongObjectMap = org.eclipse.collections.api.map.primitive.MutableLongObjectMap;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using IntSets = org.eclipse.collections.impl.factory.primitive.IntSets;

	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;
	using PropertyKeyValue = Org.Neo4j.Kernel.api.properties.PropertyKeyValue;
	using CollectionsFactory = Org.Neo4j.Kernel.impl.util.collection.CollectionsFactory;
	using StorageProperty = Org.Neo4j.Storageengine.Api.StorageProperty;
	using PropertyContainerState = Org.Neo4j.Storageengine.Api.txstate.PropertyContainerState;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;

	internal class PropertyContainerStateImpl : PropertyContainerState
	{
		 private readonly long _id;
		 private MutableLongObjectMap<Value> _addedProperties;
		 private MutableLongObjectMap<Value> _changedProperties;
		 private MutableLongSet _removedProperties;

		 protected internal readonly CollectionsFactory CollectionsFactory;

		 internal PropertyContainerStateImpl( long id, CollectionsFactory collectionsFactory )
		 {
			  this._id = id;
			  this.CollectionsFactory = requireNonNull( collectionsFactory );
		 }

		 public virtual long Id
		 {
			 get
			 {
				  return _id;
			 }
		 }

		 internal virtual void Clear()
		 {
			  if ( _changedProperties != null )
			  {
					_changedProperties.clear();
			  }
			  if ( _addedProperties != null )
			  {
					_addedProperties.clear();
			  }
			  if ( _removedProperties != null )
			  {
					_removedProperties.clear();
			  }
		 }

		 internal virtual void ChangeProperty( int propertyKeyId, Value value )
		 {
			  if ( _addedProperties != null && _addedProperties.containsKey( propertyKeyId ) )
			  {
					_addedProperties.put( propertyKeyId, value );
					return;
			  }

			  if ( _changedProperties == null )
			  {
					_changedProperties = CollectionsFactory.newValuesMap();
			  }
			  _changedProperties.put( propertyKeyId, value );

			  if ( _removedProperties != null )
			  {
					_removedProperties.remove( propertyKeyId );
			  }
		 }

		 internal virtual void AddProperty( int propertyKeyId, Value value )
		 {
			  if ( _removedProperties != null && _removedProperties.remove( propertyKeyId ) )
			  {
					// This indicates the user did remove+add as two discrete steps, which should be translated to
					// a single change operation.
					ChangeProperty( propertyKeyId, value );
					return;
			  }
			  if ( _addedProperties == null )
			  {
					_addedProperties = CollectionsFactory.newValuesMap();
			  }
			  _addedProperties.put( propertyKeyId, value );
		 }

		 internal virtual void RemoveProperty( int propertyKeyId )
		 {
			  if ( _addedProperties != null && _addedProperties.remove( propertyKeyId ) != null )
			  {
					return;
			  }
			  if ( _removedProperties == null )
			  {
					_removedProperties = CollectionsFactory.newLongSet();
			  }
			  _removedProperties.add( propertyKeyId );
			  if ( _changedProperties != null )
			  {
					_changedProperties.remove( propertyKeyId );
			  }
		 }

		 public override IEnumerator<StorageProperty> AddedProperties()
		 {
			  return ToPropertyIterator( _addedProperties );
		 }

		 public override IEnumerator<StorageProperty> ChangedProperties()
		 {
			  return ToPropertyIterator( _changedProperties );
		 }

		 public override IntIterable RemovedProperties()
		 {
			  return _removedProperties == null ? IntSets.immutable.empty() : _removedProperties.asLazy().collectInt(Math.toIntExact);
		 }

		 public override IEnumerator<StorageProperty> AddedAndChangedProperties()
		 {
			  if ( _addedProperties == null )
			  {
					return ToPropertyIterator( _changedProperties );
			  }
			  if ( _changedProperties == null )
			  {
					return ToPropertyIterator( _addedProperties );
			  }
			  return Iterators.concat( ToPropertyIterator( _addedProperties ), ToPropertyIterator( _changedProperties ) );
		 }

		 public override bool HasPropertyChanges()
		 {
			  return _addedProperties != null || _removedProperties != null || _changedProperties != null;
		 }

		 public override bool IsPropertyChangedOrRemoved( int propertyKey )
		 {
			  return ( _removedProperties != null && _removedProperties.contains( propertyKey ) ) || ( _changedProperties != null && _changedProperties.containsKey( propertyKey ) );
		 }

		 public override Value PropertyValue( int propertyKey )
		 {
			  if ( _removedProperties != null && _removedProperties.contains( propertyKey ) )
			  {
					return Values.NO_VALUE;
			  }
			  if ( _addedProperties != null )
			  {
					Value addedValue = _addedProperties.get( propertyKey );
					if ( addedValue != null )
					{
						 return addedValue;
					}
			  }
			  if ( _changedProperties != null )
			  {
					return _changedProperties.get( propertyKey );
			  }
			  return null;
		 }

		 private IEnumerator<StorageProperty> ToPropertyIterator( LongObjectMap<Value> propertyMap )
		 {
			  return propertyMap == null ? emptyIterator() : propertyMap.keyValuesView().collect(e => (StorageProperty) new PropertyKeyValue(toIntExact(e.One), e.Two)).GetEnumerator();
		 }
	}

}