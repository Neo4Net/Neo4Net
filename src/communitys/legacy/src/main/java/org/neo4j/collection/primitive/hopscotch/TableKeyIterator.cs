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
namespace Neo4Net.Collections.primitive.hopscotch
{
	using PrimitiveLongBaseIterator = Neo4Net.Collections.primitive.PrimitiveLongCollections.PrimitiveLongBaseIterator;

	public class TableKeyIterator<VALUE> : PrimitiveLongBaseIterator
	{
		 protected internal readonly Table<VALUE> Stable;
		 protected internal readonly AbstractHopScotchCollection<VALUE> Collection;
		 protected internal readonly long NullKey;
		 protected internal readonly int Version;
		 private readonly int _max;
		 private int _i;

		 internal TableKeyIterator( Table<VALUE> table, AbstractHopScotchCollection<VALUE> collection )
		 {
			  this.Stable = table;
			  this.Collection = collection;
			  this.NullKey = Stable.nullKey();
			  this._max = Stable.capacity();
			  this.Version = Stable.version();
		 }

		 protected internal virtual bool IsVisible( int index, long key )
		 {
			  return key != NullKey;
		 }

		 protected internal override bool FetchNext()
		 {
			  while ( _i < _max )
			  {
					int index = _i++;
					long key = Stable.key( index );
					if ( IsVisible( index, key ) )
					{
						 return Next( key );
					}
			  }
			  return false;
		 }
	}

}