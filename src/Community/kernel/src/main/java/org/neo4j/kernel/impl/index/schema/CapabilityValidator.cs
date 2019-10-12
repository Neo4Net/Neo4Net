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
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;

	using ArrayUtil = Neo4Net.Helpers.ArrayUtil;
	using IndexCapability = Neo4Net.@internal.Kernel.Api.IndexCapability;
	using IndexOrder = Neo4Net.@internal.Kernel.Api.IndexOrder;
	using IndexQuery = Neo4Net.@internal.Kernel.Api.IndexQuery;
	using ValueCategory = Neo4Net.Values.Storable.ValueCategory;

	internal class CapabilityValidator
	{
		 internal static void ValidateQuery( IndexCapability capability, IndexOrder indexOrder, IndexQuery[] predicates )
		 {
			  if ( indexOrder != IndexOrder.NONE )
			  {
					ValueCategory valueCategory = predicates[0].ValueGroup().category();
					IndexOrder[] orderCapability = capability.OrderCapability( valueCategory );
					if ( !ArrayUtil.contains( orderCapability, indexOrder ) )
					{
						 orderCapability = ArrayUtils.add( orderCapability, IndexOrder.NONE );
						 throw new System.NotSupportedException( format( "Tried to query index with unsupported order %s. Supported orders for query %s are %s.", indexOrder, Arrays.ToString( predicates ), Arrays.ToString( orderCapability ) ) );
					}
			  }
		 }
	}

}