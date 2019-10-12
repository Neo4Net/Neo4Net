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

	/// <summary>
	/// Limits the amount of items returned by an <seealso cref="System.Collections.IEnumerator"/>.
	/// 
	/// @author Mattias Persson
	/// </summary>
	/// @param <T> the type of items in this <seealso cref="System.Collections.IEnumerator"/>. </param>
	public class LimitingResourceIterator<T> : PrefetchingResourceIterator<T>
	{
		 private int _returned;
		 private readonly ResourceIterator<T> _source;
		 private readonly int _limit;

		 /// <summary>
		 /// Instantiates a new limiting iterator which iterates over {@code source}
		 /// and if {@code limit} items have been returned the next <seealso cref="hasNext()"/>
		 /// will return {@code false}.
		 /// </summary>
		 /// <param name="source"> the source of items. </param>
		 /// <param name="limit"> the limit, i.e. the max number of items to return. </param>
		 public LimitingResourceIterator( ResourceIterator<T> source, int limit )
		 {
			  this._source = source;
			  this._limit = limit;
		 }

		 protected internal override T FetchNextOrNull()
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  if ( !_source.hasNext() || _returned >= _limit )
			  {
					return default( T );
			  }
			  try
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					return _source.next();
			  }
			  finally
			  {
					_returned++;
			  }
		 }

		 /// <returns> {@code true} if the number of items returned up to this point
		 /// is equal to the limit given in the constructor, otherwise {@code false}. </returns>
		 public virtual bool LimitReached()
		 {
			  return _returned == _limit;
		 }

		 public override void Close()
		 {
			  _source.close();
		 }
	}

}