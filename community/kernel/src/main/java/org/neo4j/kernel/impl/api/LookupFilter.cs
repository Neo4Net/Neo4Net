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
namespace Org.Neo4j.Kernel.Impl.Api
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;


	using PrimitiveLongCollections = Org.Neo4j.Collection.PrimitiveLongCollections;
	using IndexQuery = Org.Neo4j.@internal.Kernel.Api.IndexQuery;
	using EntityNotFoundException = Org.Neo4j.@internal.Kernel.Api.exceptions.EntityNotFoundException;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using Value = Org.Neo4j.Values.Storable.Value;
	using ValueGroup = Org.Neo4j.Values.Storable.ValueGroup;
	using Values = Org.Neo4j.Values.Storable.Values;

	/// <summary>
	/// When looking up nodes by a property value, we have to do a two-stage check.
	/// The first stage is to look up the value in lucene, that will find us nodes that may have
	/// the correct property value.
	/// Then the second stage is to ensure the values actually match the value we are looking for,
	/// which requires us to load the actual property value and filter the result we got in the first stage.
	/// <para>This class defines the methods for the second stage check.<p>
	/// </para>
	/// </summary>
	public class LookupFilter
	{
		 private LookupFilter()
		 {
		 }

		 /// <summary>
		 /// used by the consistency checker
		 /// </summary>
		 public static LongIterator ExactIndexMatches( NodePropertyAccessor accessor, LongIterator indexedNodeIds, params IndexQuery[] predicates )
		 {
			  if ( !indexedNodeIds.hasNext() )
			  {
					return indexedNodeIds;
			  }

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  IndexQuery[] filteredPredicates = java.util.predicates.Where( LookupFilter.isNumericOrGeometricPredicate ).ToArray( IndexQuery[]::new );

			  if ( filteredPredicates.Length > 0 )
			  {
					System.Func<long, bool> combinedPredicate = nodeId =>
					{
					 try
					 {
						  foreach ( IndexQuery predicate in filteredPredicates )
						  {
								int propertyKeyId = predicate.propertyKeyId();
								Value value = accessor.GetNodePropertyValue( nodeId, propertyKeyId );
								if ( !predicate.acceptsValue( value ) )
								{
									 return false;
								}
						  }
						  return true;
					 }
					 catch ( EntityNotFoundException )
					 {
						  return false; // The node has been deleted but was still reported from the index. CC will catch
											 // this through other mechanism (NodeInUseWithCorrectLabelsCheck), so we can
											 // silently ignore here
					 }
					};
					return PrimitiveLongCollections.filter( indexedNodeIds, combinedPredicate );
			  }
			  return indexedNodeIds;
		 }

		 private static bool IsNumericOrGeometricPredicate( IndexQuery predicate )
		 {

			  if ( predicate.Type() == IndexQuery.IndexQueryType.exact )
			  {
					IndexQuery.ExactPredicate exactPredicate = ( IndexQuery.ExactPredicate ) predicate;
					return IsNumberGeometryOrArray( exactPredicate.Value() );
			  }
			  else
			  {
					return predicate.Type() == IndexQuery.IndexQueryType.range && (predicate.ValueGroup() == ValueGroup.NUMBER || predicate.ValueGroup() == ValueGroup.GEOMETRY);
			  }
		 }

		 private static bool IsNumberGeometryOrArray( Value value )
		 {
			  return Values.isNumberValue( value ) || Values.isGeometryValue( value ) || Values.isArrayValue( value );
		 }
	}

}