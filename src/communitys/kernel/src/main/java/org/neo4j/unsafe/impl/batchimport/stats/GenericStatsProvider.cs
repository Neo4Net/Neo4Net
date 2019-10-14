using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.@unsafe.Impl.Batchimport.stats
{

	using Neo4Net.Helpers.Collections;

	/// <summary>
	/// Generic implementation for providing <seealso cref="Stat statistics"/>.
	/// </summary>
	public class GenericStatsProvider : StatsProvider
	{
		 private readonly ICollection<Pair<Key, Stat>> _stats = new List<Pair<Key, Stat>>();

		 public virtual void Add( Key key, Stat stat )
		 {
			  this._stats.Add( Pair.of( key, stat ) );
		 }

		 public override Stat Stat( Key key )
		 {
			  foreach ( Pair<Key, Stat> stat1 in _stats )
			  {
					if ( stat1.First().name().Equals(key.Name()) )
					{
						 return stat1.Other();
					}
			  }
			  return null;
		 }

		 public override Key[] Keys()
		 {
			  Key[] keys = new Key[_stats.Count];
			  int i = 0;
			  foreach ( Pair<Key, Stat> stat in _stats )
			  {
					keys[i++] = stat.First();
			  }
			  return keys;
		 }

		 public override string ToString()
		 {
			  StringBuilder builder = new StringBuilder();
			  foreach ( Pair<Key, Stat> stat in _stats )
			  {
					builder.Append( builder.Length > 0 ? ", " : "" ).Append( format( "%s: %s", stat.First().shortName(), stat.Other() ) );
			  }
			  return builder.ToString();
		 }
	}

}