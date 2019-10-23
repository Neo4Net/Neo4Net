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
namespace Neo4Net.Kernel.impl.locking
{
	using MutableIntObjectMap = org.eclipse.collections.api.map.primitive.MutableIntObjectMap;
	using IntObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.IntObjectHashMap;

	using IHashFunction = Neo4Net.Hashing.HashFunction;
	using Strings = Neo4Net.Helpers.Strings;
	using IndexQuery = Neo4Net.Kernel.Api.Internal.IndexQuery;
	using LockWaitStrategies = Neo4Net.Kernel.impl.util.concurrent.LockWaitStrategies;
	using ResourceType = Neo4Net.Kernel.Api.StorageEngine.@lock.ResourceType;
	using Neo4Net.Kernel.Api.StorageEngine.@lock;
	using FeatureToggles = Neo4Net.Utils.FeatureToggles;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

	public sealed class ResourceTypes : ResourceType
	{
		 public static readonly ResourceTypes Node = new ResourceTypes( "Node", InnerEnum.Node, 0, Neo4Net.Kernel.impl.util.concurrent.LockWaitStrategies.IncrementalBackoff );
		 public static readonly ResourceTypes Relationship = new ResourceTypes( "Relationship", InnerEnum.Relationship, 1, Neo4Net.Kernel.impl.util.concurrent.LockWaitStrategies.IncrementalBackoff );
		 public static readonly ResourceTypes GraphProps = new ResourceTypes( "GraphProps", InnerEnum.GraphProps, 2, Neo4Net.Kernel.impl.util.concurrent.LockWaitStrategies.IncrementalBackoff );
		 // SCHEMA resource type had typeId 3 - skip it to avoid resource types conflicts
		 public static readonly ResourceTypes IndexEntry = new ResourceTypes( "IndexEntry", InnerEnum.IndexEntry, 4, Neo4Net.Kernel.impl.util.concurrent.LockWaitStrategies.IncrementalBackoff );
		 public static readonly ResourceTypes ExplicitIndex = new ResourceTypes( "ExplicitIndex", InnerEnum.ExplicitIndex, 5, Neo4Net.Kernel.impl.util.concurrent.LockWaitStrategies.IncrementalBackoff );
		 public static readonly ResourceTypes Label = new ResourceTypes( "Label", InnerEnum.Label, 6, Neo4Net.Kernel.impl.util.concurrent.LockWaitStrategies.IncrementalBackoff );
		 public static readonly ResourceTypes RelationshipType = new ResourceTypes( "RelationshipType", InnerEnum.RelationshipType, 7, Neo4Net.Kernel.impl.util.concurrent.LockWaitStrategies.IncrementalBackoff );

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

		 internal ResourceTypes( string name, InnerEnum innerEnum, int typeId, Neo4Net.Kernel.Api.StorageEngine.@lock.WaitStrategy waitStrategy )
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

		 public Neo4Net.Kernel.Api.StorageEngine.@lock.WaitStrategy WaitStrategy()
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
		 public static long IndexEntryResourceId( long labelId, params Neo4Net.Kernel.Api.Internal.IndexQuery.ExactPredicate[] predicates )
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

		 internal static long IndexEntryResourceId_2_2_0( long labelId, Neo4Net.Kernel.Api.Internal.IndexQuery.ExactPredicate[] predicates )
		 {
			  return IndexEntryResourceId_2_2_0( labelId, predicates, 0 );
		 }

		 private static long IndexEntryResourceId_2_2_0( long labelId, Neo4Net.Kernel.Api.Internal.IndexQuery.ExactPredicate[] predicates, int i )
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

		 private static string StringOf( Neo4Net.Values.Storable.Value value )
		 {
			  if ( value != null && value != Values.NO_VALUE )
			  {
					return Strings.prettyPrint( value.AsObject() );
			  }
			  return "";
		 }

		 private static int Hash( long value )
		 {
			  return _indexEntryHash_2_2_0.HashSingleValueToInt( value );
		 }

		 public static long GraphPropertyResource()
		 {
			  return 0L;
		 }

		 public static Neo4Net.Kernel.Api.StorageEngine.@lock.ResourceType FromId( int typeId )
		 {
			  return _idToType.get( typeId );
		 }

		 /// <summary>
		 /// This is a stronger, full 64-bit hashing method for schema index entries that we should use by default in a
		 /// future release, where we will also upgrade the HA protocol version. Currently this is indicated by the "4_x"
		 /// name suffix, but any version where the HA protocol version changes anyway would be just as good an opportunity.
		 /// </summary>
		 /// <seealso cref= HashFunctionHelper#IncrementalXXH64() </seealso>
		 internal static long IndexEntryResourceId_4X( long labelId, Neo4Net.Kernel.Api.Internal.IndexQuery.ExactPredicate[] predicates )
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

		public static ResourceTypes ValueOf( string name )
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