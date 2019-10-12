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
	/// Combining one or more <seealso cref="System.Collections.IEnumerator"/>s, making them look like they were
	/// one big iterator. All iteration/combining is done lazily.
	/// </summary>
	/// @param <T> the type of items in the iteration. </param>
	public class CombiningIterator<T> : PrefetchingIterator<T>
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private java.util.Iterator<? extends java.util.Iterator<T>> iterators;
		 private IEnumerator<IEnumerator<T>> _iterators;
		 private IEnumerator<T> _currentIterator;

		 public CombiningIterator<T1>( IEnumerable<T1> iterators ) where T1 : IEnumerator<T> : this( iterators.GetEnumerator() )
		 {
		 }

		public CombiningIterator<T1>( IEnumerator<T1> iterators ) where T1 : IEnumerator<T>
		{
			  this._iterators = iterators;
		}

		 public CombiningIterator( T first, IEnumerator<T> rest ) : this( Collections.emptyList() )
		 {
			  this.HasFetchedNext = true;
			  this.NextObject = first;
			  this._currentIterator = rest;
		 }

		 protected internal override T FetchNextOrNull()
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  if ( _currentIterator == null || !_currentIterator.hasNext() )
			  {
					while ( ( _currentIterator = NextIteratorOrNull() ) != null )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 if ( _currentIterator.hasNext() )
						 {
							  break;
						 }
					}
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  return _currentIterator != null && _currentIterator.hasNext() ? _currentIterator.next() : default(T);
		 }

		 protected internal virtual IEnumerator<T> NextIteratorOrNull()
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  if ( _iterators.hasNext() )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					return _iterators.next();
			  }
			  return null;
		 }

		 protected internal virtual IEnumerator<T> CurrentIterator()
		 {
			  return _currentIterator;
		 }
	}

}