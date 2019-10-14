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
namespace Neo4Net.Kernel.Impl.Index.Schema
{

	using Neo4Net.Cursors;
	using Neo4Net.Index.Internal.gbptree;
	using IndexQuery = Neo4Net.Internal.Kernel.Api.IndexQuery;
	using Value = Neo4Net.Values.Storable.Value;

	internal class FilteringNativeHitIterator<KEY, VALUE> : NativeHitIterator<KEY, VALUE> where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
	{
		 private readonly IndexQuery[] _filters;

		 internal FilteringNativeHitIterator( RawCursor<Hit<KEY, VALUE>, IOException> seeker, ICollection<RawCursor<Hit<KEY, VALUE>, IOException>> toRemoveFromWhenExhausted, IndexQuery[] filters ) : base( seeker, toRemoveFromWhenExhausted )
		 {
			  this._filters = filters;
		 }

		 internal override bool AcceptValues( Value[] values )
		 {
			  for ( int i = 0; i < values.Length; i++ )
			  {
					if ( !_filters[i].acceptsValue( values[i] ) )
					{
						 return false;
					}
			  }
			  return true;
		 }
	}

}