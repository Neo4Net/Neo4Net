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
namespace Neo4Net.Kernel.Impl.Api.index
{
	using MutableIntObjectMap = org.eclipse.collections.api.map.primitive.MutableIntObjectMap;
	using IntObjectMaps = org.eclipse.collections.impl.factory.primitive.IntObjectMaps;


	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor;
	using SchemaDescriptorSupplier = Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptorSupplier;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.collection.PrimitiveArrays.isSortedSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor_PropertySchemaType.COMPLETE_ALL_TOKENS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor_PropertySchemaType.PARTIAL_ANY_TOKEN;

	/// <summary>
	/// Collects and provides efficient access to <seealso cref="SchemaDescriptor"/>, based on complete list of IEntity tokens and partial or complete list of property keys.
	/// The descriptors are first grouped by IEntity token and then further grouped by property key. The grouping works on sorted token lists
	/// to minimize deduplication when doing lookups, especially for composite and multi-entity descriptors.
	/// <para>
	/// The selection works best when providing a complete list of property keys and will have to resort to some amount of over-selection
	/// when caller can only provide partial list of property keys. Selection starts by traversing through the IEntity tokens, and for each
	/// found leaf continues traversing through the property keys. When adding descriptors the token lists are sorted and token lists passed
	/// into lookup methods also have to be sorted. This way the internal data structure can be represented as chains of tokens and traversal only
	/// needs to go through the token chains in one order (the sorted order). A visualization of the internal data structure of added descriptors:
	/// 
	/// <pre>
	///     Legend: ids inside [] are IEntity tokens, ids inside () are properties
	///     Single-entity token descriptors
	///     A: [0](4, 7, 3)
	///     B: [0](7, 4)
	///     C: [0](3, 4)
	///     D: [0](3, 4, 7)
	///     E: [1](7)
	///     F: [1](5, 6)
	///     Multi-entity token descriptors (matches are made on any of the IEntity/property key tokens)
	///     G: [0, 1](3, 4)
	///     H: [1, 0](3)
	/// 
	///     Will result in a data structure (for the optimized path):
	///     [0]
	///        -> (3): G, H
	///           -> (4): C
	///              -> (7): A, D
	///        -> (4): G
	///           -> (7): B
	///     [1]
	///        -> (3): G, H
	///        -> (4): G
	///        -> (5)
	///           -> (6): F
	///        -> (7): E
	/// </pre>
	/// </para>
	/// </summary>
	public class SchemaDescriptorLookupSet<T> where T : Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptorSupplier
	{
		 private readonly MutableIntObjectMap<PropertyMultiSet> _byEntityToken = IntObjectMaps.mutable.empty();

		 /// <returns> whether or not this set is empty, i.e. {@code true} if no descriptors have been added. </returns>
		 internal virtual bool Empty
		 {
			 get
			 {
				  return _byEntityToken.Empty;
			 }
		 }

		 /// <summary>
		 /// Cheap way of finding out whether or not there are any descriptors matching the set of IEntity token ids and the property key id.
		 /// </summary>
		 /// <param name="entityTokenIds"> complete list of IEntity token ids for the IEntity to check. </param>
		 /// <param name="propertyKey"> a property key id to check. </param>
		 /// <returns> {@code true} if there are one or more descriptors matching the given tokens. </returns>
		 internal virtual bool Has( long[] IEntityTokenIds, int propertyKey )
		 {
			  // Abort right away if there are no descriptors at all
			  if ( Empty )
			  {
					return false;
			  }

			  // Check if there are any descriptors that matches any of the first (or only) IEntity token
			  foreach ( long IEntityTokenId in IEntityTokenIds )
			  {
					PropertyMultiSet set = _byEntityToken.get( toIntExact( IEntityTokenId ) );
					if ( set != null && set.Has( propertyKey ) )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 /// <summary>
		 /// Cheap way of finding out whether or not there are any descriptors matching the given IEntity token id.
		 /// </summary>
		 /// <param name="entityTokenId"> IEntity token id to check. </param>
		 /// <returns> {@code true} if there are one or more descriptors matching the given IEntity token. </returns>
		 internal virtual bool Has( int IEntityTokenId )
		 {
			  return _byEntityToken.containsKey( IEntityTokenId );
		 }

		 /// <summary>
		 /// Adds the given descriptor to this set so that it can be looked up from any of the lookup methods.
		 /// </summary>
		 /// <param name="schemaDescriptor"> the descriptor to add. </param>
		 public virtual void Add( T schemaDescriptor )
		 {
			  foreach ( int IEntityTokenId in schemaDescriptor.schema().EntityTokenIds )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					_byEntityToken.getIfAbsentPut( IEntityTokenId, PropertyMultiSet::new ).add( schemaDescriptor );
			  }
		 }

		 /// <summary>
		 /// Removes the given descriptor from this set so that it can no longer be looked up from the lookup methods.
		 /// This operation is idempotent.
		 /// </summary>
		 /// <param name="schemaDescriptor"> the descriptor to remove. </param>
		 public virtual void Remove( T schemaDescriptor )
		 {
			  foreach ( int IEntityTokenId in schemaDescriptor.schema().EntityTokenIds )
			  {
					PropertyMultiSet any = _byEntityToken.get( IEntityTokenId );
					if ( any != null && any.Remove( schemaDescriptor ) )
					{
						 _byEntityToken.remove( IEntityTokenId );
					}
			  }
		 }

		 /// <summary>
		 /// Collects descriptors matching the given complete list of IEntity tokens and property key tokens.
		 /// I.e. all tokens of the matching descriptors can be found in the given lists of tokens.
		 /// </summary>
		 /// <param name="into"> <seealso cref="System.Collections.ICollection"/> to add matching descriptors into. </param>
		 /// <param name="entityTokenIds"> complete and sorted array of IEntity token ids for the IEntity. </param>
		 /// <param name="sortedProperties"> complete and sorted array of property key token ids for the IEntity. </param>
		 internal virtual void MatchingDescriptorsForCompleteListOfProperties( ICollection<T> into, long[] IEntityTokenIds, int[] sortedProperties )
		 {
			  Debug.Assert( isSortedSet( sortedProperties ) );
			  foreach ( long IEntityTokenId in IEntityTokenIds )
			  {
					PropertyMultiSet first = _byEntityToken.get( toIntExact( IEntityTokenId ) );
					if ( first != null )
					{
						 first.CollectForCompleteListOfProperties( into, sortedProperties );
					}
			  }
		 }

		 /// <summary>
		 /// Collects descriptors matching the given complete list of IEntity tokens, but only partial list of property key tokens.
		 /// I.e. all IEntity tokens of the matching descriptors can be found in the given lists of tokens,
		 /// but some matching descriptors may have other property key tokens in addition to those found in the given properties of the IEntity.
		 /// This is for a scenario where the complete list of property key tokens isn't known when calling this method and may
		 /// collect additional descriptors that in the end isn't relevant for the specific IEntity.
		 /// </summary>
		 /// <param name="into"> <seealso cref="System.Collections.ICollection"/> to add matching descriptors into. </param>
		 /// <param name="entityTokenIds"> complete and sorted array of IEntity token ids for the IEntity. </param>
		 /// <param name="sortedProperties"> complete and sorted array of property key token ids for the IEntity. </param>
		 internal virtual void MatchingDescriptorsForPartialListOfProperties( ICollection<T> into, long[] IEntityTokenIds, int[] sortedProperties )
		 {
			  Debug.Assert( isSortedSet( sortedProperties ) );
			  foreach ( long IEntityTokenId in IEntityTokenIds )
			  {
					PropertyMultiSet first = _byEntityToken.get( toIntExact( IEntityTokenId ) );
					if ( first != null )
					{
						 first.CollectForPartialListOfProperties( into, sortedProperties );
					}
			  }
		 }

		 /// <summary>
		 /// Collects descriptors matching the given complete list of IEntity tokens.
		 /// </summary>
		 /// <param name="into"> <seealso cref="System.Collections.ICollection"/> to add matching descriptors into. </param>
		 /// <param name="entityTokenIds"> complete and sorted array of IEntity token ids for the IEntity. </param>
		 internal virtual void MatchingDescriptors( ICollection<T> into, long[] IEntityTokenIds )
		 {
			  Debug.Assert( isSortedSet( IEntityTokenIds ) );
			  foreach ( long IEntityTokenId in IEntityTokenIds )
			  {
					PropertyMultiSet set = _byEntityToken.get( toIntExact( IEntityTokenId ) );
					if ( set != null )
					{
						 set.CollectAll( into );
					}
			  }
		 }

		 /// <summary>
		 /// A starting point for traversal of property key tokens. Contains starting points of property key id chains
		 /// as well as lookup by any property in the chain.
		 /// Roughly like this:
		 /// 
		 /// <pre>
		 ///     Descriptors:
		 ///     A: (5, 7)
		 ///     B: (5, 4)
		 ///     C: (4)
		 ///     D: (6)
		 /// 
		 ///     Data structure:
		 ///     (4): C
		 ///        -> (5): B
		 ///     (5)
		 ///        -> (7): A
		 ///     (6): D
		 /// </pre>
		 /// </summary>
		 private class PropertyMultiSet
		 {
			 private readonly SchemaDescriptorLookupSet<T> _outerInstance;

			 public PropertyMultiSet( SchemaDescriptorLookupSet<T> outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal readonly ISet<T> Descriptors = new HashSet<T>();
			  internal readonly MutableIntObjectMap<PropertySet> Next = IntObjectMaps.mutable.empty();
			  internal readonly MutableIntObjectMap<ISet<T>> ByAnyProperty = IntObjectMaps.mutable.empty();

			  internal virtual void Add( T schemaDescriptor )
			  {
					// Add optimized path for when property list is fully known
					Descriptors.Add( schemaDescriptor );
					int[] propertyKeyIds = SortedPropertyKeyIds( schemaDescriptor.schema() );
					Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor_PropertySchemaType propertySchemaType = schemaDescriptor.schema().propertySchemaType();
					if ( propertySchemaType == COMPLETE_ALL_TOKENS )
					{
						 // Just add the first token id to the top level set
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
						 Next.getIfAbsentPut( propertyKeyIds[0], PropertySet::new ).add( schemaDescriptor, propertyKeyIds, 0 );
					}
					else if ( propertySchemaType == PARTIAL_ANY_TOKEN )
					{
						 // The internal data structure is built and optimized for when all property key tokens are required to match
						 // a particular descriptor. However to support the partial type, where any property key may match
						 // we will have to add such descriptors to all property key sets and pretend that each is the only one.
						 foreach ( int propertyKeyId in propertyKeyIds )
						 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
							  Next.getIfAbsentPut( propertyKeyId, PropertySet::new ).add( schemaDescriptor, new int[]{ propertyKeyId }, 0 );
						 }
					}
					else
					{
						 throw new System.NotSupportedException( "Unknown property schema type " + propertySchemaType );
					}

					// Add fall-back path for when property list is only partly known
					foreach ( int keyId in propertyKeyIds )
					{
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
						 ByAnyProperty.getIfAbsentPut( keyId, HashSet<object>::new ).add( schemaDescriptor );
					}
			  }

			  /// <summary>
			  /// Removes the <seealso cref="SchemaDescriptor"/> from this multi-set. </summary>
			  /// <param name="schemaDescriptor"> the <seealso cref="SchemaDescriptor"/> to remove. </param>
			  /// <returns> {@code true} if this multi-set ended up empty after removing this descriptor. </returns>
			  internal virtual bool Remove( T schemaDescriptor )
			  {
					// Remove from the optimized path
					Descriptors.remove( schemaDescriptor );
					int[] propertyKeyIds = SortedPropertyKeyIds( schemaDescriptor.schema() );
					Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor_PropertySchemaType propertySchemaType = schemaDescriptor.schema().propertySchemaType();
					if ( propertySchemaType == COMPLETE_ALL_TOKENS )
					{
						 int firstPropertyKeyId = propertyKeyIds[0];
						 PropertySet firstPropertySet = Next.get( firstPropertyKeyId );
						 if ( firstPropertySet != null && firstPropertySet.Remove( schemaDescriptor, propertyKeyIds, 0 ) )
						 {
							  Next.remove( firstPropertyKeyId );
						 }
					}
					else if ( propertySchemaType == PARTIAL_ANY_TOKEN )
					{
						 foreach ( int propertyKeyId in propertyKeyIds )
						 {
							  PropertySet propertySet = Next.get( propertyKeyId );
							  if ( propertySet != null && propertySet.Remove( schemaDescriptor, new int[]{ propertyKeyId }, 0 ) )
							  {
									Next.remove( propertyKeyId );
							  }
						 }
					}
					else
					{
						 throw new System.NotSupportedException( "Unknown property schema type " + propertySchemaType );
					}

					// Remove from the fall-back path
					foreach ( int keyId in propertyKeyIds )
					{
						 ISet<T> byProperty = ByAnyProperty.get( keyId );
						 if ( byProperty != null )
						 {
							  byProperty.remove( schemaDescriptor );
							  if ( byProperty.Count == 0 )
							  {
									ByAnyProperty.remove( keyId );
							  }
						 }
					}
					return Descriptors.Count == 0 && Next.Empty;
			  }

			  internal virtual void CollectForCompleteListOfProperties( ICollection<T> descriptors, int[] sortedProperties )
			  {
					for ( int i = 0; i < sortedProperties.Length; i++ )
					{
						 PropertySet firstSet = Next.get( sortedProperties[i] );
						 if ( firstSet != null )
						 {
							  firstSet.CollectForCompleteListOfProperties( descriptors, sortedProperties, i );
						 }
					}
			  }

			  internal virtual void CollectForPartialListOfProperties( ICollection<T> descriptors, int[] sortedProperties )
			  {
					foreach ( int propertyKeyId in sortedProperties )
					{
						 ISet<T> propertyDescriptors = ByAnyProperty.get( propertyKeyId );
						 if ( propertyDescriptors != null )
						 {
							  descriptors.addAll( propertyDescriptors );
						 }
					}
			  }

			  internal virtual void CollectAll( ICollection<T> descriptors )
			  {
					descriptors.addAll( this.Descriptors );
			  }

			  internal virtual bool Has( int propertyKey )
			  {
					return ByAnyProperty.containsKey( propertyKey );
			  }

			  internal virtual bool Empty
			  {
				  get
				  {
						return Descriptors.Count == 0 && Next.Empty;
				  }
			  }
		 }

		 /// <summary>
		 /// A single item in a property key chain. Sort of a subset, or a more specific version, of <seealso cref="PropertyMultiSet"/>.
		 /// It has a set containing descriptors that have their property chain end with this property key token and next pointers
		 /// to other property key tokens in known chains, but no way of looking up descriptors by any part of the chain,
		 /// like <seealso cref="PropertyMultiSet"/> has.
		 /// </summary>
		 private class PropertySet
		 {
			 private readonly SchemaDescriptorLookupSet<T> _outerInstance;

			 public PropertySet( SchemaDescriptorLookupSet<T> outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal readonly ISet<T> FullDescriptors = new HashSet<T>();
			  internal readonly MutableIntObjectMap<PropertySet> Next = IntObjectMaps.mutable.empty();

			  internal virtual void Add( T schemaDescriptor, int[] propertyKeyIds, int cursor )
			  {
					if ( cursor == propertyKeyIds.Length - 1 )
					{
						 FullDescriptors.Add( schemaDescriptor );
					}
					else
					{
						 int nextPropertyKeyId = propertyKeyIds[++cursor];
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
						 Next.getIfAbsentPut( nextPropertyKeyId, PropertySet::new ).add( schemaDescriptor, propertyKeyIds, cursor );
					}
			  }

			  /// <param name="schemaDescriptor"> <seealso cref="SchemaDescriptor"/> to remove. </param>
			  /// <param name="propertyKeyIds"> the sorted property key ids for this schema. </param>
			  /// <param name="cursor"> which property key among the sorted property keys that this set deals with. </param>
			  /// <returns> {@code true} if this <seealso cref="PropertySet"/> ends up empty after this removal. </returns>
			  internal virtual bool Remove( T schemaDescriptor, int[] propertyKeyIds, int cursor )
			  {
					if ( cursor == propertyKeyIds.Length - 1 )
					{
						 FullDescriptors.remove( schemaDescriptor );
					}
					else
					{
						 int nextPropertyKeyId = propertyKeyIds[++cursor];
						 PropertySet propertySet = Next.get( nextPropertyKeyId );
						 if ( propertySet != null && propertySet.Remove( schemaDescriptor, propertyKeyIds, cursor ) )
						 {
							  Next.remove( nextPropertyKeyId );
						 }
					}
					return FullDescriptors.Count == 0 && Next.Empty;
			  }

			  internal virtual void CollectForCompleteListOfProperties( ICollection<T> descriptors, int[] sortedProperties, int cursor )
			  {
					descriptors.addAll( FullDescriptors );
					if ( !Next.Empty )
					{
						 for ( int i = cursor + 1; i < sortedProperties.Length; i++ )
						 {
							  PropertySet nextSet = Next.get( sortedProperties[i] );
							  if ( nextSet != null )
							  {
									nextSet.CollectForCompleteListOfProperties( descriptors, sortedProperties, i );
							  }
						 }
					}
			  }
		 }

		 private static int[] SortedPropertyKeyIds( SchemaDescriptor schemaDescriptor )
		 {
			  int[] tokenIds = schemaDescriptor.PropertyIds;
			  if ( tokenIds.Length > 1 )
			  {
					// Clone it because we don't know if the array was an internal array that the descriptor handed out
					tokenIds = tokenIds.Clone();
					Arrays.sort( tokenIds );
			  }
			  return tokenIds;
		 }
	}

}