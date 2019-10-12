using System;
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
namespace Neo4Net.Helpers.Collection
{

	/// <summary>
	/// An iterator which filters for elements of a given subtype, and casts to this type.
	/// </summary>
	/// @param <T> the type of elements returned by this iterator. </param>
	/// @param <A> the type of elements read by this iterator. This must be a supertype of T. </param>
	public class CastingIterator<T, A> : PrefetchingIterator<T> where T : A
	{
		 private readonly IEnumerator<A> _source;
		 private Type<T> _outClass;

		 public CastingIterator( IEnumerator<A> source, Type outClass )
		 {
				 outClass = typeof( T );
			  this._source = source;
			  this._outClass = outClass;
		 }

		 protected internal override T FetchNextOrNull()
		 {
			  while ( _source.MoveNext() )
			  {
					A testItem = _source.Current;
					if ( _outClass.IsInstanceOfType( testItem ) )
					{
						 return _outClass.cast( testItem );
					}
			  }
			  return default( T );
		 }
	}

}