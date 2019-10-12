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
namespace Org.Neo4j.Kernel.impl.locking
{
	using MutableIntObjectMap = org.eclipse.collections.api.map.primitive.MutableIntObjectMap;
	using IntObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.IntObjectHashMap;

	using HashFunction = Org.Neo4j.Hashing.HashFunction;
	using Strings = Org.Neo4j.Helpers.Strings;
	using IndexQuery = Org.Neo4j.@internal.Kernel.Api.IndexQuery;
	using LockWaitStrategies = Org.Neo4j.Kernel.impl.util.concurrent.LockWaitStrategies;
	using ResourceType = Org.Neo4j.Storageengine.Api.@lock.ResourceType;
	using Org.Neo4j.Storageengine.Api.@lock;
	using FeatureToggles = Org.Neo4j.Util.FeatureToggles;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;

	public sealed class ResourceTypes : ResourceType
	{
		 public static readonly ResourceTypes Node = new ResourceTypes( "Node", InnerEnum.Node, 0, Org.Neo4j.Kernel.impl.util.concurrent.LockWaitStrategies.IncrementalBackoff );
		 public static readonly ResourceTypes Relationship = new ResourceTypes( "Relationship", InnerEnum.Relationship, 1, Org.Neo4j.Kernel.impl.util.concurrent.LockWaitStrategies.IncrementalBackoff );
		 public static readonly ResourceTypes GraphProps = new ResourceTypes( "GraphProps", InnerEnum.GraphProps, 2, Org.Neo4j.Kernel.impl.util.concurrent.LockWaitStrategies.IncrementalBackoff );
		 // SCHEMA resource type had typeId 3 - skip it to avoid resource types conflicts
		 public static readonly ResourceTypes IndexEntry = new ResourceTypes( "IndexEntry", InnerEnum.IndexEntry, 4, Org.Neo4j.Kernel.impl.util.concurrent.LockWaitStrategies.IncrementalBackoff );
		 public static readonly ResourceTypes ExplicitIndex = new ResourceTypes( "ExplicitIndex", InnerEnum.ExplicitIndex, 5, Org.Neo4j.Kernel.impl.util.concurrent.LockWaitStrategies.IncrementalBackoff );
		 public static readonly ResourceTypes Label = new ResourceTypes( "Label", InnerEnum.Label, 6, Org.Neo4j.Kernel.impl.util.concurrent.LockWaitStrategies.IncrementalBackoff );
		 public static readonly ResourceTypes RelationshipType = new ResourceTypes( "RelationshipType", InnerEnum.RelationshipType, 7, Org.Neo4j.Kernel.impl.util.concurrent.LockWaitStrategies.IncrementalBackoff );

		 private static readonly IList<ResourceTypes> valueList = new List<ResourceTypes>();

		 public enum InnerEnum
		 {
			 Node,
			 Relationship,
			 GraphProps,
			 IndexEntry,
			 ExplicitIndex,
			 Label,
			 RelationshipType
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private const;

		 internal Private const;
		 internal Private const;
		 internal Private const;

		 static ResourceTypes()
		 {
			  foreach ( ResourceTypes resourceTypes in ResourceTypes.values() )
			  {
					_idToType.put( resourceTypes._typeId, resourceTypes );
			  }

			 valueList.Add( Node );
			 valueList.Add( Relationship );
			 valueList.Add( GraphProps );
			 valueList.Add( IndexEntry );
			 valueList.Add( ExplicitIndex );
			 valueList.Add( Label );
			 valueList.Add( RelationshipType );
		 }

		 internal Private readonly;

		 internal Private readonly;

		 internal ResourceTypes( string name, InnerEnum innerEnum, int typeId, Org.Neo4j.Storageengine.Api.@lock.WaitStrategy waitStrategy )
		 {
			  this._typeId = typeId;
			  this._waitStrategy = waitStrategy;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public int TypeId()
		 {
			  return _typeId;
		 }

		 public Org.Neo4j.Storageengine.Api.@lock.WaitStrategy WaitStrategy()
		 {
			  return _waitStrategy;
		 }

		 /// <summary>
		 /// The index entry hashing method used for entries in explicit indexes.
		 /// </summary>
		 public static long ExplicitIndexResourceId( string name, string key )
		 {
			  return ( long ) name.GetHashCode() << 32 | key.GetHashCode();
		 }

		 /// <summary>
		 /// This is the schema index entry hashing method used since 2.2.0 and onwards.
		 /// <para>
		 /// Use the <seealso cref="ResourceTypes.useStrongHashing"/> feature toggle to use a stronger hash function, which will become
		 /// the default in a future release. <strong>Note</strong> that changing this hash function is effectively a
		 /// clustering protocol change in HA setups. Causal cluster setups are unaffected because followers do not take any
		 /// locks on the cluster leader.
		 /// </para>
		 /// </summary>
		 public static long IndexEntryResourceId( long labelId, params Org.Neo4j.@internal.Kernel.Api.IndexQuery.ExactPredicate[] predicates )
		 {
			  if ( !_useStrongHashing )
			  {
					// Default
					return IndexEntryResourceId_2_2_0( labelId, predicates );
			  }
			  else
			  {
					// Opt-in
					return IndexEntryResourceId_4X( labelId, predicates );
			  }
		 }

		 internal static long IndexEntryResourceId_2_2_0( long labelId, Org.Neo4j.@internal.Kernel.Api.IndexQuery.ExactPredicate[] predicates )
		 {
			  return IndexEntryResourceId_2_2_0( labelId, predicates, 0 );
		 }

		 private static long IndexEntryResourceId_2_2_0( long labelId, Org.Neo4j.@internal.Kernel.Api.IndexQuery.ExactPredicate[] predicates, int i )
		 {
			  int propertyKeyId = predicates[i].PropertyKeyId();
			  Value value = predicates[i].Value();
			  // Note:
			  // It is important that single-property indexes only hash with this particular call; no additional hashing!
			  long hash = IndexEntryResourceId_2_2_0( labelId, propertyKeyId, StringOf( value ) );
			  i++;
			  if ( i < predicates.Length )
			  {
					hash = hash( hash + IndexEntryResourceId_2_2_0( labelId, predicates, i ) );
			  }
			  return hash;
		 }

		 private static long IndexEntryResourceId_2_2_0( long labelId, long propertyKeyId, string propertyValue )
		 {
			  long hob = Hash( labelId + Hash( propertyKeyId ) );
			  hob <<= 32;
			  return hob + propertyValue.GetHashCode();
		 }

		 private static string StringOf( Org.Neo4j.Values.Storable.Value value )
		 {
			  if ( value != null && value != Values.NO_VALUE )
			  {
					return Strings.prettyPrint( value.AsObject() );
			  }
			  return "";
		 }

		 private static int Hash( long value )
		 {
			  return _indexEntryHash_2_2_0.hashSingleValueToInt( value );
		 }

		 public static long GraphPropertyResource()
		 {
			  return 0L;
		 }

		 public static Org.Neo4j.Storageengine.Api.@lock.ResourceType FromId( int typeId )
		 {
			  return _idToType.get( typeId );
		 }

		 /// <summary>
		 /// This is a stronger, full 64-bit hashing method for schema index entries that we should use by default in a
		 /// future release, where we will also upgrade the HA protocol version. Currently this is indicated by the "4_x"
		 /// name suffix, but any version where the HA protocol version changes anyway would be just as good an opportunity.
		 /// </summary>
		 /// <seealso cref= HashFunction#incrementalXXH64() </seealso>
		 internal static long IndexEntryResourceId_4X( long labelId, Org.Neo4j.@internal.Kernel.Api.IndexQuery.ExactPredicate[] predicates )
		 {
			  long hash = _indexEntryHash_4X.initialise( 0x0123456789abcdefL );

			  hash = _indexEntryHash_4X.update( hash, labelId );

			  foreach ( IndexQuery.ExactPredicate predicate in predicates )
			  {
					int propertyKeyId = predicate.PropertyKeyId();
					hash = _indexEntryHash_4X.update( hash, propertyKeyId );
					Value value = predicate.Value();
					hash = value.UpdateHash( _indexEntryHash_4X, hash );
			  }

			  return _indexEntryHash_4X.finalise( hash );
		 }

		public static IList<ResourceTypes> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static ResourceTypes valueOf( string name )
		{
			foreach ( ResourceTypes enumInstance in ResourceTypes.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}