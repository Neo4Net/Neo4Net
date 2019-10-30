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
namespace Neo4Net.Collections.Helpers
{

	using Predicates = Neo4Net.Functions.Predicates;

	/// <summary>
	/// An iterable which filters another iterable, only letting items with certain
	/// criteria pass through. All iteration/filtering is done lazily.
	/// </summary>
	/// @param <T> the type of items in the iteration. </param>
	public class FilteringIterable<T> : IEnumerable<T>
	{
		 private readonly IEnumerable<T> _source;
		 private readonly System.Predicate<T> _predicate;

		 public FilteringIterable( IEnumerable<T> source, System.Predicate<T> predicate )
		 {
			  this._source = source;
			  this._predicate = predicate;
		 }

		 public override IEnumerator<T> Iterator()
		 {
			  return new FilteringIterator<T>( _source.GetEnumerator(), _predicate );
		 }

		 public static IEnumerable<T> NotNull<T>( IEnumerable<T> source )
		 {
			  return new FilteringIterable<T>( source, Predicates.notNull() );
		 }

		 public static IEnumerable<T> NoDuplicates<T>( IEnumerable<T> source )
		 {
			  return new FilteringIterable<T>( source, Predicates.noDuplicates() );
		 }
	}

}