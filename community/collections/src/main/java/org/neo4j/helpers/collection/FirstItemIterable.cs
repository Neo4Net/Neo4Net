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

	/// <summary>
	/// Wraps the given iterator but keeps the first item to allow later
	/// access to it, like CachingIterator but with less memory overhead.
	/// </summary>
	/// @param <T> the type of elements </param>
	public class FirstItemIterable<T> : IEnumerable<T>
	{
		 private readonly T _first;
		 private readonly IEnumerator<T> _iterator;
		 private int _pos = -1;

		 public FirstItemIterable( IEnumerable<T> data ) : this( data.GetEnumerator() )
		 {
		 }

		 public FirstItemIterable( IEnumerator<T> iterator )
		 {
			  this._iterator = iterator;
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  if ( iterator.hasNext() )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					this._first = iterator.next();
					this._pos = 0;
			  }
			  else
			  {
					this._first = default( T );
			  }
		 }

		 public override IEnumerator<T> Iterator()
		 {
			  return new IteratorAnonymousInnerClass( this );
		 }

		 private class IteratorAnonymousInnerClass : IEnumerator<T>
		 {
			 private readonly FirstItemIterable<T> _outerInstance;

			 public IteratorAnonymousInnerClass( FirstItemIterable<T> outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public bool hasNext()
			 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  return _outerInstance.pos == 0 || _outerInstance.iterator.hasNext();
			 }

			 public T next()
			 {
				  if ( _outerInstance.pos < 0 )
				  {
						throw new NoSuchElementException();
				  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  return _outerInstance.pos++ == 0 ? _outerInstance.first : _outerInstance.iterator.next();
			 }

			 public void remove()
			 {

			 }
		 }

		 public virtual T First
		 {
			 get
			 {
				  return _first;
			 }
		 }
	}

}