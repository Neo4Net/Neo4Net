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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{

	using Org.Neo4j.Cursor;
	using Org.Neo4j.Index.@internal.gbptree;
	using IndexQuery = Org.Neo4j.@internal.Kernel.Api.IndexQuery;
	using Value = Org.Neo4j.Values.Storable.Value;

	internal class FilteringNativeHitIndexProgressor<KEY, VALUE> : NativeHitIndexProgressor<KEY, VALUE> where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
	{
		 private readonly IndexQuery[] _filter;

		 internal FilteringNativeHitIndexProgressor( RawCursor<Hit<KEY, VALUE>, IOException> seeker, NodeValueClient client, ICollection<RawCursor<Hit<KEY, VALUE>, IOException>> toRemoveFromOnClose, IndexQuery[] filter ) : base( seeker, client, toRemoveFromOnClose )
		 {
			  this._filter = filter;
		 }

		 protected internal override bool AcceptValue( Value[] values )
		 {
			  for ( int i = 0; i < values.Length; i++ )
			  {
					if ( !_filter[i].acceptsValue( values[i] ) )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 // We need to make sure to always deserialize, even if the client doesn't need the value, to be able to filter
		 internal override Value[] ExtractValues( KEY key )
		 {
			  return key.asValues();
		 }
	}

}