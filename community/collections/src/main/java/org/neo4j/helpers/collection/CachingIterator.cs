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
	/// An <seealso cref="System.Collections.IEnumerator"/> which lazily fetches and caches items from the
	/// underlying iterator when items are requested. This enables positioning
	/// as well as going backwards through the iteration.
	/// @author Mattias Persson
	/// </summary>
	/// @param <T> the type of items in the iterator. </param>
	public class CachingIterator<T> : IEnumerator<T>
	{
		 private readonly IEnumerator<T> _source;
		 private readonly IList<T> _visited = new List<T>();
		 private int _position;
		 private T _current;

		 /// <summary>
		 /// Creates a new caching iterator using {@code source} as its underlying
		 /// <seealso cref="System.Collections.IEnumerator"/> to get items lazily from. </summary>
		 /// <param name="source"> the underlying <seealso cref="System.Collections.IEnumerator"/> to lazily get items from. </param>
		 public CachingIterator( IEnumerator<T> source )
		 {
			  this._source = source;
		 }

		 /// <summary>
		 /// Returns whether a call to <seealso cref="next()"/> will be able to return
		 /// an item or not. If the current <seealso cref="position()"/> is beyond the size
		 /// of the cache (as will be the case if only calls to
		 /// <seealso cref="hasNext()"/>/<seealso cref="next()"/> has been made up to this point)
		 /// the underlying iterator is asked, else {@code true} since it can be
		 /// returned from the cache.
		 /// </summary>
		 /// <returns> whether or not there are more items in this iteration given the
		 /// current <seealso cref="position()"/>. </returns>
		 public override bool HasNext()
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  return _visited.Count > _position || _source.hasNext();
		 }

		 /// <summary>
		 /// Returns the next item given the current <seealso cref="position()"/>.
		 /// If the current <seealso cref="position()"/> is beyond the size
		 /// of the cache (as will be the case if only calls to
		 /// <seealso cref="hasNext()"/>/<seealso cref="next()"/> has been made up to this point) the
		 /// underlying iterator is asked for the next item (and cached if there
		 /// was one), else the item is returned from the cache.
		 /// </summary>
		 /// <returns> the next item given the current <seealso cref="position()"/>. </returns>
		 public override T Next()
		 {
			  if ( _visited.Count > _position )
			  {
					_current = _visited[_position];
			  }
			  else
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( !_source.hasNext() )
					{
						 throw new NoSuchElementException();
					}
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					_current = _source.next();
					_visited.Add( _current );
			  }
			  _position++;
			  return _current;
		 }

		 /// <summary>
		 /// Returns the current position of the iterator, initially 0. The position
		 /// represents the index of the item which will be returned by the next call
		 /// to <seealso cref="next()"/> and also the index of the next item returned by
		 /// <seealso cref="previous()"/> plus one. An example:
		 /// 
		 /// <ul>
		 /// <li>Instantiate an iterator which would iterate over the strings "first", "second" and "third".</li>
		 /// <li>Get the two first items ("first" and "second") from it by using <seealso cref="next()"/>,
		 /// <seealso cref="position()"/> will now return 2.</li>
		 /// <li>Call <seealso cref="previous()"/> (which will return "second") and <seealso cref="position()"/> will now be 1</li>
		 /// </ul>
		 /// </summary>
		 /// <returns> the position of the iterator. </returns>
		 public virtual int Position()
		 {
			  return _position;
		 }

		 /// <summary>
		 /// Sets the position of the iterator. {@code 0} means all the way back to
		 /// the beginning. It is also possible to set the position to one higher
		 /// than the last item, so that the next call to <seealso cref="previous()"/> would
		 /// return the last item. Items will be cached along the way if necessary.
		 /// </summary>
		 /// <param name="newPosition"> the position to set for the iterator, must be
		 /// non-negative. </param>
		 /// <returns> the position before changing to the new position. </returns>
		 public virtual int Position( int newPosition )
		 {
			  if ( newPosition < 0 )
			  {
					throw new System.ArgumentException( "Position must be non-negative, was " + newPosition );
			  }

			  int previousPosition = _position;
			  bool overTheEdge = false;
			  while ( _visited.Count < newPosition )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					T next = _source.hasNext() ? _source.next() : default(T);
					if ( next != default( T ) )
					{
						 _visited.Add( next );
					}
					else
					{
						 if ( !overTheEdge )
						 {
							  overTheEdge = true;
						 }
						 else
						 {
							  throw new NoSuchElementException( "Requested position " + newPosition + ", but didn't get further than to " + _visited.Count );
						 }
					}
			  }
			  _current = default( T );
			  _position = newPosition;
			  return previousPosition;
		 }

		 /// <summary>
		 /// Returns whether or not a call to <seealso cref="previous()"/> will be able to
		 /// return an item or not. So it will return {@code true} if
		 /// <seealso cref="position()"/> is bigger than 0.
		 /// 
		 /// {@inheritDoc}
		 /// </summary>
		 public override bool HasPrevious()
		 {
			  return _position > 0;
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
		 public override T Previous()
		 {
			  if ( !HasPrevious() )
			  {
					throw new NoSuchElementException( "Position is " + _position );
			  }
			  _current = _visited[--_position];
			  return _current;
		 }

		 /// <summary>
		 /// Returns the last item returned by <seealso cref="next()"/>/<seealso cref="previous()"/>.
		 /// If no call has been made to <seealso cref="next()"/> or <seealso cref="previous()"/> since
		 /// this iterator was created or since a call to <seealso cref="position(int)"/> has
		 /// been made a <seealso cref="NoSuchElementException"/> will be thrown.
		 /// </summary>
		 /// <returns> the last item returned by <seealso cref="next()"/>/<seealso cref="previous()"/>. </returns>
		 /// <exception cref="NoSuchElementException"> if no call has been made to <seealso cref="next()"/>
		 /// or <seealso cref="previous()"/> since this iterator was created or since a call to
		 /// <seealso cref="position(int)"/> has been made. </exception>
		 public virtual T Current()
		 {
			  if ( _current == default( T ) )
			  {
					throw new NoSuchElementException();
			  }
			  return _current;
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
		 public override int NextIndex()
		 {
			  return _position;
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
		 public override int PreviousIndex()
		 {
			  return _position - 1;
		 }

		 /// <summary>
		 /// Not supported by this implement.
		 /// 
		 /// {@inheritDoc}
		 /// </summary>
		 public override void Remove()
		 {
			  throw new System.NotSupportedException();
		 }

		 /// <summary>
		 /// Not supported by this implement.
		 /// 
		 /// {@inheritDoc}
		 /// </summary>
		 public override void Set( T e )
		 {
			  throw new System.NotSupportedException();
		 }

		 /// <summary>
		 /// Not supported by this implement.
		 /// 
		 /// {@inheritDoc}
		 /// </summary>
		 public override void Add( T e )
		 {
			  throw new System.NotSupportedException();
		 }
	}

}