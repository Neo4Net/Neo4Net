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
namespace Neo4Net.Kernel.Impl.Index.Schema
{

	using EntityNotFoundException = Neo4Net.@internal.Kernel.Api.exceptions.EntityNotFoundException;
	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using Values = Neo4Net.Values.Storable.Values;

	/// <summary>
	/// Compares <seealso cref="NativeIndexKey"/>, but will consult <seealso cref="NodePropertyAccessor"/> on coming across a comparison of zero.
	/// This is useful for e.g. spatial keys which are indexed lossily. </summary>
	/// @param <KEY> type of index key. </param>
	internal class PropertyLookupFallbackComparator<KEY, VALUE> : IComparer<KEY> where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
	{
		 private readonly IndexLayout<KEY, VALUE> _schemaLayout;
		 private readonly NodePropertyAccessor _propertyAccessor;
		 private readonly int _propertyKeyId;

		 internal PropertyLookupFallbackComparator( IndexLayout<KEY, VALUE> schemaLayout, NodePropertyAccessor propertyAccessor, int propertyKeyId )
		 {
			  this._schemaLayout = schemaLayout;
			  this._propertyAccessor = propertyAccessor;
			  this._propertyKeyId = propertyKeyId;
		 }

		 public override int Compare( KEY k1, KEY k2 )
		 {
			  int comparison = _schemaLayout.compareValue( k1, k2 );
			  if ( comparison != 0 )
			  {
					return comparison;
			  }
			  try
			  {
					return Values.COMPARATOR.Compare( _propertyAccessor.getNodePropertyValue( k1.EntityId, _propertyKeyId ), _propertyAccessor.getNodePropertyValue( k2.EntityId, _propertyKeyId ) );
			  }
			  catch ( EntityNotFoundException )
			  {
					// We don't want this operation to fail since it's merely counting distinct values.
					// This entity not being there is most likely a result of a concurrent deletion happening as we speak.
					return comparison;
			  }
		 }
	}

}