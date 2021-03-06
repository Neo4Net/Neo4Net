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
namespace Org.Neo4j.Helpers.Collection
{

	using Predicates = Org.Neo4j.Function.Predicates;

	/// <summary>
	/// An iterator which filters another iterator, only letting items with certain
	/// criteria pass through. All iteration/filtering is done lazily.
	/// </summary>
	/// @param <T> the type of items in the iteration. </param>
	public class FilteringIterator<T> : PrefetchingIterator<T>
	{
		 private readonly IEnumerator<T> _source;
		 private readonly System.Predicate<T> _predicate;

		 public FilteringIterator( IEnumerator<T> source, System.Predicate<T> predicate )
		 {
			  this._source = source;
			  this._predicate = predicate;
		 }

		 protected internal override T FetchNextOrNull()
		 {
			  while ( _source.MoveNext() )
			  {
					T testItem = _source.Current;
					if ( _predicate.test( testItem ) )
					{
						 return testItem;
					}
			  }
			  return default( T );
		 }

		 public static IEnumerator<T> NotNull<T>( IEnumerator<T> source )
		 {
			  return new FilteringIterator<T>( source, Predicates.notNull() );
		 }

		 public static IEnumerator<T> NoDuplicates<T>( IEnumerator<T> source )
		 {
			  return new FilteringIterator<T>( source, Predicates.noDuplicates() );
		 }
	}

}