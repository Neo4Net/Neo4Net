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
namespace Org.Neo4j.Helpers.Collection
{
	using Org.Neo4j.Graphdb;
	using Org.Neo4j.Graphdb;

	/// <summary>
	/// Limits the amount of items returned by an <seealso cref="ResourceIterable"/>, or rather
	/// <seealso cref="ResourceIterator"/>s spawned from it.
	/// </summary>
	/// @param <T> the type of items in this <seealso cref="System.Collections.IEnumerable"/>. </param>
	/// <seealso cref= LimitingResourceIterator </seealso>
	public class LimitingResourceIterable<T> : ResourceIterable<T>
	{
		 private readonly IEnumerable<T> _source;
		 private readonly int _limit;

		 /// <summary>
		 /// Instantiates a new limiting <seealso cref="System.Collections.IEnumerable"/> which can limit the number
		 /// of items returned from iterators it spawns.
		 /// </summary>
		 /// <param name="source"> the source of items. </param>
		 /// <param name="limit"> the limit, i.e. the max number of items to return. </param>
		 public LimitingResourceIterable( ResourceIterable<T> source, int limit )
		 {
			  this._source = source;
			  this._limit = limit;
		 }

		 public override ResourceIterator<T> Iterator()
		 {
			  return new LimitingResourceIterator<T>( Iterators.AsResourceIterator( _source.GetEnumerator() ), _limit );
		 }
	}

}