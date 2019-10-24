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

	/// <summary>
	/// Concatenates sub-iterators of an iterator.
	/// 
	/// Iterates through each item in an iterator. For each item, the
	/// <seealso cref="createNestedIterator(object)"/> is invoked to create a sub-iterator.
	/// The resulting iterator iterates over each item in each sub-iterator. In
	/// effect flattening the iteration.
	/// </summary>
	/// @param <T> the type of items to return </param>
	/// @param <U> the type of items in the surface item iterator </param>
	public abstract class NestingIterator<T, U> : PrefetchingIterator<T>
	{
		 private readonly IEnumerator<U> _source;
		 private IEnumerator<T> _currentNestedIterator;
		 private U _currentSurfaceItem;

		 public NestingIterator( IEnumerator<U> source )
		 {
			  this._source = source;
		 }

		 protected internal abstract IEnumerator<T> CreateNestedIterator( U item );

		 public virtual U CurrentSurfaceItem
		 {
			 get
			 {
				  if ( this._currentSurfaceItem == default( U ) )
				  {
						throw new System.InvalidOperationException( "Has no surface item right now," + " you must do at least one next() first" );
				  }
				  return this._currentSurfaceItem;
			 }
		 }

		 protected internal override T FetchNextOrNull()
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  if ( _currentNestedIterator == null || !_currentNestedIterator.hasNext() )
			  {
					while ( _source.MoveNext() )
					{
						 _currentSurfaceItem = _source.Current;
						 _currentNestedIterator = CreateNestedIterator( _currentSurfaceItem );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 if ( _currentNestedIterator.hasNext() )
						 {
							  break;
						 }
					}
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  return _currentNestedIterator != null && _currentNestedIterator.hasNext() ? _currentNestedIterator.next() : default(T);
		 }
	}

}