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
	/// Concatenates sub-iterables of an iterable.
	/// </summary>
	/// <seealso cref= NestingIterator
	/// </seealso>
	/// @param <T> the type of items. </param>
	/// @param <U> the type of items in the surface item iterator </param>
	public abstract class NestingIterable<T, U> : IEnumerable<T>
	{
		 private readonly IEnumerable<U> _source;

		 public NestingIterable( IEnumerable<U> source )
		 {
			  this._source = source;
		 }

		 public override IEnumerator<T> Iterator()
		 {
			  return new NestingIteratorAnonymousInnerClass( this, _source.GetEnumerator() );
		 }

		 private class NestingIteratorAnonymousInnerClass : NestingIterator<T, U>
		 {
			 private readonly NestingIterable<T, U> _outerInstance;

			 public NestingIteratorAnonymousInnerClass( NestingIterable<T, U> outerInstance, UnknownType iterator ) : base( iterator )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override IEnumerator<T> createNestedIterator( U item )
			 {
				  return _outerInstance.createNestedIterator( item );
			 }
		 }

		 protected internal abstract IEnumerator<T> CreateNestedIterator( U item );
	}

}