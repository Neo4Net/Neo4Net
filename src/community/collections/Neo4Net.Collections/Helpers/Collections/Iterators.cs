using System;
using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.Helpers.Collections
{

	using Neo4Net.Collections;
	using Predicates = Neo4Net.Functions.Predicates;
	using Neo4Net.Functions;
	using Resource = Neo4Net.Graphdb.Resource;
	using Neo4Net.Graphdb;
	using Neo4Net.Graphdb;

	/// <summary>
	/// Contains common functionality regarding <seealso cref="System.Collections.IEnumerator"/>s and
	/// <seealso cref="System.Collections.IEnumerable"/>s.
	/// </summary>
	public sealed class Iterators
	{
		 private Iterators()
		 {
			  throw new AssertionError( "no instance" );
		 }

		 /// <summary>
		 /// Returns the given iterator's first element or {@code null} if no
		 /// element found.
		 /// </summary>
		 /// @param <T> the type of elements in {@code iterator}. </param>
		 /// <param name="iterator"> the <seealso cref="System.Collections.IEnumerator"/> to get elements from. </param>
		 /// <returns> the first element in the {@code iterator}, or {@code null} if no
		 /// element found. </returns>
		 public static T FirstOrNull<T>( IEnumerator<T> iterator )
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  return iterator.hasNext() ? iterator.next() : default(T);
		 }

		 /// <summary>
		 /// Returns the given iterator's first element. If no element is found a
		 /// <seealso cref="NoSuchElementException"/> is thrown.
		 /// </summary>
		 /// @param <T> the type of elements in {@code iterator}. </param>
		 /// <param name="iterator"> the <seealso cref="System.Collections.IEnumerator"/> to get elements from. </param>
		 /// <returns> the first element in the {@code iterator}. </returns>
		 /// <exception cref="NoSuchElementException"> if no element found. </exception>
		 public static T First<T>( IEnumerator<T> iterator )
		 {
			  return AssertNotNull( iterator, FirstOrNull( iterator ) );
		 }

		 /// <summary>
		 /// Returns the given iterator's last element or {@code null} if no
		 /// element found.
		 /// </summary>
		 /// @param <T> the type of elements in {@code iterator}. </param>
		 /// <param name="iterator"> the <seealso cref="System.Collections.IEnumerator"/> to get elements from. </param>
		 /// <returns> the last element in the {@code iterator}, or {@code null} if no
		 /// element found. </returns>
		 public static T LastOrNull<T>( IEnumerator<T> iterator )
		 {
			  T result = default( T );
			  while ( iterator.MoveNext() )
			  {
					result = iterator.Current;
			  }
			  return result;
		 }

		 /// <summary>
		 /// Returns the given iterator's last element. If no element is found a
		 /// <seealso cref="NoSuchElementException"/> is thrown.
		 /// </summary>
		 /// @param <T> the type of elements in {@code iterator}. </param>
		 /// <param name="iterator"> the <seealso cref="System.Collections.IEnumerator"/> to get elements from. </param>
		 /// <returns> the last element in the {@code iterator}. </returns>
		 /// <exception cref="NoSuchElementException"> if no element found. </exception>
		 public static T Last<T>( IEnumerator<T> iterator )
		 {
			  return AssertNotNull( iterator, LastOrNull( iterator ) );
		 }

		 /// <summary>
		 /// Returns the given iterator's single element or {@code null} if no
		 /// element found. If there is more than one element in the iterator a
		 /// <seealso cref="NoSuchElementException"/> will be thrown.
		 /// 
		 /// If the {@code iterator} implements <seealso cref="Resource"/> it will be <seealso cref="Resource.close() closed"/>
		 /// in a {@code finally} block after the single item has been retrieved, or failed to be retrieved.
		 /// </summary>
		 /// @param <T> the type of elements in {@code iterator}. </param>
		 /// <param name="iterator"> the <seealso cref="System.Collections.IEnumerator"/> to get elements from. </param>
		 /// <returns> the single element in {@code iterator}, or {@code null} if no
		 /// element found. </returns>
		 /// <exception cref="NoSuchElementException"> if more than one element was found. </exception>
		 public static T SingleOrNull<T>( IEnumerator<T> iterator )
		 {
			  return Single( iterator, null );
		 }

		 /// <summary>
		 /// Returns the given iterator's single element. If there are no elements
		 /// or more than one element in the iterator a <seealso cref="NoSuchElementException"/>
		 /// will be thrown.
		 /// 
		 /// If the {@code iterator} implements <seealso cref="Resource"/> it will be <seealso cref="Resource.close() closed"/>
		 /// in a {@code finally} block after the single item has been retrieved, or failed to be retrieved.
		 /// </summary>
		 /// @param <T> the type of elements in {@code iterator}. </param>
		 /// <param name="iterator"> the <seealso cref="System.Collections.IEnumerator"/> to get elements from. </param>
		 /// <returns> the single element in the {@code iterator}. </returns>
		 /// <exception cref="NoSuchElementException"> if there isn't exactly one element. </exception>
		 public static T Single<T>( IEnumerator<T> iterator )
		 {
			  return AssertNotNull( iterator, SingleOrNull( iterator ) );
		 }

		 /// <summary>
		 /// Returns the iterator's n:th item from the end of the iteration.
		 /// If the iterator has got less than n-1 items in it
		 /// <seealso cref="NoSuchElementException"/> is thrown.
		 /// </summary>
		 /// @param <T> the type of elements in {@code iterator}. </param>
		 /// <param name="iterator"> the <seealso cref="System.Collections.IEnumerator"/> to get elements from. </param>
		 /// <param name="n"> the n:th item from the end to get. </param>
		 /// <returns> the iterator's n:th item from the end of the iteration. </returns>
		 /// <exception cref="NoSuchElementException"> if the iterator contains less than n-1 items. </exception>
		 public static T FromEnd<T>( IEnumerator<T> iterator, int n )
		 {
			  return AssertNotNull( iterator, FromEndOrNull( iterator, n ) );
		 }

		 /// <summary>
		 /// Returns the iterator's n:th item from the end of the iteration.
		 /// If the iterator has got less than n-1 items in it {@code null} is returned.
		 /// </summary>
		 /// @param <T> the type of elements in {@code iterator}. </param>
		 /// <param name="iterator"> the <seealso cref="System.Collections.IEnumerator"/> to get elements from. </param>
		 /// <param name="n"> the n:th item from the end to get. </param>
		 /// <returns> the iterator's n:th item from the end of the iteration,
		 /// or {@code null} if the iterator doesn't contain that many items. </returns>
		 public static T FromEndOrNull<T>( IEnumerator<T> iterator, int n )
		 {
			  Deque<T> trail = new LinkedList<T>( n );
			  while ( iterator.MoveNext() )
			  {
					if ( trail.size() > n )
					{
						 trail.removeLast();
					}
					trail.addFirst( iterator.Current );
			  }
			  return trail.size() == n + 1 ? trail.Last : default(T);
		 }

		 /// <summary>
		 /// Iterates over the full iterators, and checks equality for each item in them. Note that this
		 /// will consume the iterators.
		 /// </summary>
		 /// <param name="first"> the first iterator </param>
		 /// <param name="other"> the other iterator </param>
		 /// <returns> {@code true} if all items are equal; otherwise </returns>
		 public static bool IteratorsEqual<T1, T2>( IEnumerator<T1> first, IEnumerator<T2> other )
		 {
			  while ( true )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( first.hasNext() && other.hasNext() )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 if ( !first.next().Equals(other.next()) )
						 {
							  return false;
						 }
					}
					else
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 return first.hasNext() == other.hasNext();
					}
			  }
		 }

		 private static T AssertNotNull<T>( IEnumerator<T> iterator, T result )
		 {
			  if ( result == default( T ) )
			  {
					throw new NoSuchElementException( "No element found in " + iterator );
			  }
			  return result;
		 }

		 /// <summary>
		 /// Returns the given iterator's single element or {@code itemIfNone} if no
		 /// element found. If there is more than one element in the iterator a
		 /// <seealso cref="NoSuchElementException"/> will be thrown.
		 /// 
		 /// If the {@code iterator} implements <seealso cref="Resource"/> it will be <seealso cref="Resource.close() closed"/>
		 /// in a {@code finally} block after the single item has been retrieved, or failed to be retrieved.
		 /// </summary>
		 /// @param <T> the type of elements in {@code iterator}. </param>
		 /// <param name="iterator"> the <seealso cref="System.Collections.IEnumerator"/> to get elements from. </param>
		 /// <param name="itemIfNone"> item to use if none is found </param>
		 /// <returns> the single element in {@code iterator}, or {@code itemIfNone} if no
		 /// element found. </returns>
		 /// <exception cref="NoSuchElementException"> if more than one element was found. </exception>
		 public static T Single<T>( IEnumerator<T> iterator, T itemIfNone )
		 {
			  try
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					T result = iterator.hasNext() ? iterator.next() : itemIfNone;
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( iterator.hasNext() )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 throw new NoSuchElementException( "More than one element in " + iterator + ". First element is '" + result + "' and the second element is '" + iterator.next() + "'" );
					}
					return result;
			  }
			  finally
			  {
					if ( iterator is Resource )
					{
						 ( ( Resource ) iterator ).close();
					}
			  }
		 }

		 /// <summary>
		 /// Adds all the items in {@code iterator} to {@code collection}. </summary>
		 /// @param <C> the type of <seealso cref="System.Collections.ICollection"/> to add to items to. </param>
		 /// @param <T> the type of items in the collection and iterator. </param>
		 /// <param name="iterator"> the <seealso cref="System.Collections.IEnumerator"/> to grab the items from. </param>
		 /// <param name="collection"> the <seealso cref="System.Collections.ICollection"/> to add the items to. </param>
		 /// <returns> the {@code collection} which was passed in, now filled
		 /// with the items from {@code iterator}. </returns>
		 public static C AddToCollection<C, T>( IEnumerator<T> iterator, C collection ) where C : ICollection<T>
		 {
			  while ( iterator.MoveNext() )
			  {
					collection.add( iterator.Current );
			  }
			  return collection;
		 }

		 /// <summary>
		 /// Adds all the items in {@code iterator} to {@code collection}. </summary>
		 /// @param <C> the type of <seealso cref="System.Collections.ICollection"/> to add to items to. </param>
		 /// @param <T> the type of items in the collection and iterator. </param>
		 /// <param name="iterator"> the <seealso cref="System.Collections.IEnumerator"/> to grab the items from. </param>
		 /// <param name="collection"> the <seealso cref="System.Collections.ICollection"/> to add the items to. </param>
		 /// <returns> the {@code collection} which was passed in, now filled
		 /// with the items from {@code iterator}. </returns>
		 public static C AddToCollectionUnique<C, T>( IEnumerator<T> iterator, C collection ) where C : ICollection<T>
		 {
			  while ( iterator.MoveNext() )
			  {
					AddUnique( collection, iterator.Current );
			  }
			  return collection;
		 }

		 private static void AddUnique<T, C>( C collection, T item ) where C : ICollection<T>
		 {
			  if ( !collection.add( item ) )
			  {
					throw new System.InvalidOperationException( "Encountered an already added item:" + item + " when adding items uniquely to a collection:" + collection );
			  }
		 }

		 /// <summary>
		 /// Adds all the items in {@code iterator} to {@code collection}. </summary>
		 /// @param <C> the type of <seealso cref="System.Collections.ICollection"/> to add to items to. </param>
		 /// @param <T> the type of items in the collection and iterator. </param>
		 /// <param name="iterable"> the <seealso cref="System.Collections.IEnumerator"/> to grab the items from. </param>
		 /// <param name="collection"> the <seealso cref="System.Collections.ICollection"/> to add the items to. </param>
		 /// <returns> the {@code collection} which was passed in, now filled
		 /// with the items from {@code iterator}. </returns>
		 public static C AddToCollectionUnique<C, T>( IEnumerable<T> iterable, C collection ) where C : ICollection<T>
		 {
			  return AddToCollectionUnique( iterable.GetEnumerator(), collection );
		 }

		 /// <summary>
		 /// Convenience method for looping over an <seealso cref="System.Collections.IEnumerator"/>. Converts the
		 /// <seealso cref="System.Collections.IEnumerator"/> to an <seealso cref="System.Collections.IEnumerable"/> by wrapping it in an
		 /// <seealso cref="System.Collections.IEnumerable"/> that returns the <seealso cref="System.Collections.IEnumerator"/>. It breaks the
		 /// contract of <seealso cref="System.Collections.IEnumerable"/> in that it returns the supplied iterator
		 /// instance for each call to {@code iterator()} on the returned
		 /// <seealso cref="System.Collections.IEnumerable"/> instance. This method exists to make it easy to use an
		 /// <seealso cref="System.Collections.IEnumerator"/> in a for-loop.
		 /// </summary>
		 /// @param <T> the type of items in the iterator. </param>
		 /// <param name="iterator"> the iterator to expose as an <seealso cref="System.Collections.IEnumerable"/>. </param>
		 /// <returns> the supplied iterator posing as an <seealso cref="System.Collections.IEnumerable"/>. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T> Iterable<T> loop(final java.util.Iterator<T> iterator)
		 public static IEnumerable<T> Loop<T>( IEnumerator<T> iterator )
		 {
			  return () => iterator;
		 }

		 /// <summary>
		 /// Exposes {@code iterator} as an <seealso cref="System.Collections.IEnumerable"/>. It breaks the contract
		 /// of <seealso cref="System.Collections.IEnumerable"/> in that it returns the supplied iterator instance for
		 /// each call to {@code iterator()} on the returned <seealso cref="System.Collections.IEnumerable"/>
		 /// instance. This method mostly exists to make it easy to use an
		 /// <seealso cref="System.Collections.IEnumerator"/> in a for-loop.
		 /// </summary>
		 /// @param <T> the type of items in the iterator. </param>
		 /// <param name="iterator"> the iterator to expose as an <seealso cref="System.Collections.IEnumerable"/>. </param>
		 /// <returns> the supplied iterator posing as an <seealso cref="System.Collections.IEnumerable"/>. </returns>
		 //@Deprecated * @deprecated use {@link #loop(Iterator) the loop method} instead.
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T> Iterable<T> asIterable(final java.util.Iterator<T> iterator)
		 public static IEnumerable<T> AsIterable<T>( IEnumerator<T> iterator )
		 {
			  return Loop( iterator );
		 }

		 public static long Count<T>( IEnumerator<T> iterator )
		 {
			  return Count( iterator, Predicates.alwaysTrue() );
		 }

		 /// <summary>
		 /// Counts the number of filtered in the {@code iterator} by looping
		 /// through it. </summary>
		 /// @param <T> the type of items in the iterator. </param>
		 /// <param name="iterator"> the <seealso cref="System.Collections.IEnumerator"/> to count items in. </param>
		 /// <param name="filter"> the filter to test items against </param>
		 /// <returns> the number of filtered items found in {@code iterator}. </returns>
		 public static long Count<T>( IEnumerator<T> iterator, System.Predicate<T> filter )
		 {
			  long result = 0;
			  while ( iterator.MoveNext() )
			  {
					if ( filter( iterator.Current ) )
					{
						 result++;
					}
			  }
			  return result;
		 }

		 public static ICollection<T> AsCollection<T>( IEnumerator<T> iterable )
		 {
			  return AddToCollection( iterable, new List<>() );
		 }

		 public static IList<T> AsList<T>( IEnumerator<T> iterator )
		 {
			  return AddToCollection( iterator, new List<>() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static <T, EX extends Exception> java.util.List<T> asList(org.neo4j.collection.RawIterator<T, EX> iterator) throws EX
		 public static IList<T> AsList<T, EX>( RawIterator<T, EX> iterator ) where EX : Exception
		 {
			  IList<T> @out = new List<T>();
			  while ( iterator.HasNext() )
			  {
					@out.Add( iterator.Next() );
			  }
			  return @out;
		 }

		 public static ISet<T> AsSet<T>( IEnumerator<T> iterator )
		 {
			  return AddToCollection( iterator, new HashSet<>() );
		 }

		 /// <summary>
		 /// Creates a <seealso cref="System.Collections.Generic.ISet<object>"/> from an array of items.an
		 /// </summary>
		 /// <param name="items"> the items to add to the set. </param>
		 /// @param <T> the type of the items </param>
		 /// <returns> the <seealso cref="System.Collections.Generic.ISet<object>"/> containing the items. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public static <T> java.util.Set<T> asSet(T... items)
		 public static ISet<T> AsSet<T>( params T[] items )
		 {
			  return new HashSet<T>( Arrays.asList( items ) );
		 }

		 /// <summary>
		 /// Alias for asSet()
		 /// </summary>
		 /// <param name="items"> the items to add to the set. </param>
		 /// @param <T> the type of the items </param>
		 /// <returns> the <seealso cref="System.Collections.Generic.ISet<object>"/> containing the items. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public static <T> java.util.Set<T> set(T... items)
		 public static ISet<T> Set<T>( params T[] items )
		 {
			  return AsSet( items );
		 }

		 /// <summary>
		 /// Creates a <seealso cref="System.Collections.Generic.ISet<object>"/> from an array of items.
		 /// </summary>
		 /// <param name="items"> the items to add to the set. </param>
		 /// @param <T> the type of the items </param>
		 /// <returns> the <seealso cref="System.Collections.Generic.ISet<object>"/> containing the items. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public static <T> java.util.Set<T> asUniqueSet(T... items)
		 public static ISet<T> AsUniqueSet<T>( params T[] items )
		 {
			  HashSet<T> set = new HashSet<T>();
			  foreach ( T item in items )
			  {
					AddUnique( set, item );
			  }
			  return set;
		 }

		 /// <summary>
		 /// Creates a <seealso cref="System.Collections.Generic.ISet<object>"/> from an array of items.
		 /// </summary>
		 /// <param name="items"> the items to add to the set. </param>
		 /// @param <T> the type of the items </param>
		 /// <returns> the <seealso cref="System.Collections.Generic.ISet<object>"/> containing the items. </returns>
		 public static ISet<T> AsUniqueSet<T>( IEnumerator<T> items )
		 {
			  HashSet<T> set = new HashSet<T>();
			  while ( items.MoveNext() )
			  {
					AddUnique( set, items.Current );
			  }
			  return set;
		 }

		 public static SortedSet<T> AsSortedSet<T>( IComparer<T> comparator, params T[] items )
		 {
			  SortedSet<T> set = new SortedSet<T>( comparator );
			  Collections.addAll( set, items );
			  return set;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static java.util.Iterator<long> asIterator(final long... array)
		 public static IEnumerator<long> AsIterator( params long[] array )
		 {
			  return new PrefetchingIteratorAnonymousInnerClass( array );
		 }

		 private class PrefetchingIteratorAnonymousInnerClass : PrefetchingIterator<long>
		 {
			 private long[] _array;

			 public PrefetchingIteratorAnonymousInnerClass( long[] array )
			 {
				 this._array = array;
			 }

			 private int index;

			 protected internal override long? fetchNextOrNull()
			 {
				  try
				  {
						return index < _array.Length ? _array[index] : null;
				  }
				  finally
				  {
						index++;
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static java.util.Iterator<int> asIterator(final int... array)
		 public static IEnumerator<int> AsIterator( params int[] array )
		 {
			  return new PrefetchingIteratorAnonymousInnerClass2( array );
		 }

		 private class PrefetchingIteratorAnonymousInnerClass2 : PrefetchingIterator<int>
		 {
			 private int[] _array;

			 public PrefetchingIteratorAnonymousInnerClass2( int[] array )
			 {
				 this._array = array;
			 }

			 private int index;

			 protected internal override int? fetchNextOrNull()
			 {
				  try
				  {
						return index < _array.Length ? _array[index] : null;
				  }
				  finally
				  {
						index++;
				  }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public static <T> java.util.Iterator<T> asIterator(final int maxItems, final T... array)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public static IEnumerator<T> AsIterator<T>( int maxItems, params T[] array )
		 {
			  return new PrefetchingIteratorAnonymousInnerClass3( maxItems, array );
		 }

		 private class PrefetchingIteratorAnonymousInnerClass3 : PrefetchingIterator<T>
		 {
			 private int _maxItems;
			 private T[] _array;

			 public PrefetchingIteratorAnonymousInnerClass3( int maxItems, T[] array )
			 {
				 this._maxItems = maxItems;
				 this._array = array;
			 }

			 private int index;

			 protected internal override T fetchNextOrNull()
			 {
				  try
				  {
						return index < _array.Length && index < _maxItems ? _array[index] : null;
				  }
				  finally
				  {
						index++;
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T> java.util.Iterator<T> iterator(final T item)
		 public static IEnumerator<T> Iterator<T>( T item )
		 {
			  if ( item == default( T ) )
			  {
					return emptyIterator();
			  }

			  return new IteratorAnonymousInnerClass( item );
		 }

		 private class IteratorAnonymousInnerClass : IEnumerator<T>
		 {
			 private T _item;

			 public IteratorAnonymousInnerClass( T item )
			 {
				 this._item = item;
			 }

			 internal T myItem = _item;

			 public bool hasNext()
			 {
				  return myItem != null;
			 }

			 public T next()
			 {
				  if ( !hasNext() )
				  {
						throw new NoSuchElementException();
				  }
				  T toReturn = myItem;
				  myItem = null;
				  return toReturn;
			 }

			 public void remove()
			 {
				  throw new System.NotSupportedException();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public static <T> java.util.Iterator<T> iterator(T... items)
		 public static IEnumerator<T> Iterator<T>( params T[] items )
		 {
			  return AsIterator( items.Length, items );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public static <T> java.util.Iterator<T> iterator(int maxItems, T... items)
		 public static IEnumerator<T> Iterator<T>( int maxItems, params T[] items )
		 {
			  return AsIterator( maxItems, items );
		 }

		 public static IEnumerator<T> AppendTo<T>( IEnumerator<T> iterator, params T[] appended )
		 {
			  return new IteratorAnonymousInnerClass2( iterator, appended );
		 }

		 private class IteratorAnonymousInnerClass2 : IEnumerator<T>
		 {
			 private IEnumerator<T> _iterator;
			 private T[] _appended;

			 public IteratorAnonymousInnerClass2( IEnumerator<T> iterator, T[] appended )
			 {
				 this._iterator = iterator;
				 this._appended = appended;
			 }

			 private int index;

			 public bool hasNext()
			 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  return _iterator.hasNext() || index < _appended.Length;
			 }

			 public T next()
			 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  if ( _iterator.hasNext() )
				  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						return _iterator.next();
				  }
				  else if ( index < _appended.Length )
				  {
						return _appended[index++];
				  }
				  else
				  {
						throw new NoSuchElementException();
				  }
			 }
		 }

		 public static IEnumerator<T> PrependTo<T>( IEnumerator<T> iterator, params T[] prepended )
		 {
			  return new IteratorAnonymousInnerClass3( iterator, prepended );
		 }

		 private class IteratorAnonymousInnerClass3 : IEnumerator<T>
		 {
			 private IEnumerator<T> _iterator;
			 private T[] _prepended;

			 public IteratorAnonymousInnerClass3( IEnumerator<T> iterator, T[] prepended )
			 {
				 this._iterator = iterator;
				 this._prepended = prepended;
			 }

			 private int index;

			 public bool hasNext()
			 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  return index < _prepended.Length || _iterator.hasNext();
			 }

			 public T next()
			 {
				  if ( index < _prepended.Length )
				  {
						return _prepended[index++];
				  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  else if ( _iterator.hasNext() )
				  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						return _iterator.next();
				  }
				  else
				  {
						throw new NoSuchElementException();
				  }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static <T> org.neo4j.graphdb.ResourceIterator<T> emptyResourceIterator()
		 public static ResourceIterator<T> EmptyResourceIterator<T>()
		 {
			  return ( ResourceIterator<T> ) EmptyResourceIterator.EmptyResourceIteratorConflict;
		 }

		 public static bool Contains<T>( IEnumerator<T> iterator, T item )
		 {
			  try
			  {
					foreach ( T element in Loop( iterator ) )
					{
						 if ( item == default( T ) ? element == default( T ) : item.Equals( element ) )
						 {
							  return true;
						 }
					}
					return false;
			  }
			  finally
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: if (iterator instanceof org.neo4j.graphdb.ResourceIterator<?>)
					if ( iterator is ResourceIterator<object> )
					{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ((org.neo4j.graphdb.ResourceIterator<?>) iterator).close();
						 ( ( ResourceIterator<object> ) iterator ).close();
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T> org.neo4j.graphdb.ResourceIterator<T> asResourceIterator(final java.util.Iterator<T> iterator)
		 public static ResourceIterator<T> AsResourceIterator<T>( IEnumerator<T> iterator )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: if (iterator instanceof org.neo4j.graphdb.ResourceIterator<?>)
			  if ( iterator is ResourceIterator<object> )
			  {
					return ( ResourceIterator<T> ) iterator;
			  }
			  return new WrappingResourceIterator<T>( iterator );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T> org.neo4j.graphdb.ResourceIterator<T> resourceIterator(final java.util.Iterator<T> iterator, final org.neo4j.graphdb.Resource resource)
		 public static ResourceIterator<T> ResourceIterator<T>( IEnumerator<T> iterator, Resource resource )
		 {
			  return new PrefetchingResourceIteratorAnonymousInnerClass( iterator, resource );
		 }

		 private class PrefetchingResourceIteratorAnonymousInnerClass : PrefetchingResourceIterator<T>
		 {
			 private IEnumerator<T> _iterator;
			 private Resource _resource;

			 public PrefetchingResourceIteratorAnonymousInnerClass( IEnumerator<T> iterator, Resource resource )
			 {
				 this._iterator = iterator;
				 this._resource = resource;
			 }

			 public override void close()
			 {
				  _resource.close();
			 }

			 protected internal override T fetchNextOrNull()
			 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  return _iterator.hasNext() ? _iterator.next() : null;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public static <T> T[] array(T... items)
		 public static T[] Array<T>( params T[] items )
		 {
			  return items;
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static <X> java.util.Iterator<X> filter(System.Predicate<? super X> specification, java.util.Iterator<X> i)
		 public static IEnumerator<X> Filter<X, T1>( System.Predicate<T1> specification, IEnumerator<X> i )
		 {
			  return new FilterIterable.FilterIterator<X>( i, specification );
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static <FROM, TO> java.util.Iterator<TO> map(System.Func<? super FROM, ? extends TO> function, java.util.Iterator<FROM> from)
		 public static IEnumerator<TO> Map<FROM, TO, T1>( System.Func<T1> function, IEnumerator<FROM> from ) where T1 : TO
		 {
			  return new MapIterable.MapIterator<TO>( from, function );
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static <FROM, TO, EX extends Exception> org.neo4j.collection.RawIterator<TO, EX> map(org.neo4j.function.ThrowingFunction<? super FROM, ? extends TO, EX> function, org.neo4j.collection.RawIterator<FROM, EX> from)
		 public static RawIterator<TO, EX> Map<FROM, TO, EX, T1>( ThrowingFunction<T1> function, RawIterator<FROM, EX> from ) where EX : Exception where T1 : TO
		 {
			  return new RawMapIterator<TO, EX>( from, function );
		 }

		 public static RawIterator<T, EX> AsRawIterator<T, EX>( IEnumerator<T> iter ) where EX : Exception
		 {
			  return new RawIteratorAnonymousInnerClass( iter );
		 }

		 private class RawIteratorAnonymousInnerClass : RawIterator<T, EX>
		 {
			 private IEnumerator<T> _iter;

			 public RawIteratorAnonymousInnerClass( IEnumerator<T> iter )
			 {
				 this._iter = iter;
			 }

			 public bool hasNext()
			 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  return _iter.hasNext();
			 }

			 public T next()
			 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  return _iter.next();
			 }
		 }

		 public static RawIterator<T, EX> AsRawIterator<T, EX>( Stream<T> stream ) where EX : Exception
		 {
			  return AsRawIterator( stream.GetEnumerator() );
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static <FROM, TO> java.util.Iterator<TO> flatMap(System.Func<? super FROM, ? extends java.util.Iterator<TO>> function, java.util.Iterator<FROM> from)
		 public static IEnumerator<TO> FlatMap<FROM, TO, T1>( System.Func<T1> function, IEnumerator<FROM> from ) where T1 : IEnumerator<TO>
		 {
			  return new CombiningIterator<TO>( Map( function, from ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs @SuppressWarnings("unchecked") public static <T> java.util.Iterator<T> concat(java.util.Iterator<? extends T>... iterators)
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static IEnumerator<T> Concat<T>( params IEnumerator<T>[] iterators )
		 {
			  return Concat( Arrays.asList( ( IEnumerator<T>[] ) iterators ).GetEnumerator() );
		 }

		 public static ResourceIterator<T> ConcatResourceIterators<T>( IEnumerator<ResourceIterator<T>> iterators )
		 {
			  return new CombiningResourceIterator<T>( iterators );
		 }

		 public static IEnumerator<T> Concat<T>( IEnumerator<IEnumerator<T>> iterators )
		 {
			  return new CombiningIterator<T>( iterators );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T> org.neo4j.graphdb.ResourceIterable<T> asResourceIterable(final org.neo4j.graphdb.ResourceIterator<T> it)
		 public static ResourceIterable<T> AsResourceIterable<T>( ResourceIterator<T> it )
		 {
			  return () => it;
		 }

		 public static string Join<T1>( string joinString, IEnumerator<T1> iter )
		 {
			  StringBuilder sb = new StringBuilder();
			  while ( iter.MoveNext() )
			  {
					sb.Append( iter.Current.ToString() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( iter.hasNext() )
					{
						 sb.Append( joinString );
					}
			  }
			  return sb.ToString();
		 }

		 public static PrefetchingIterator<T> Prefetching<T>( IEnumerator<T> iterator )
		 {
			  return iterator is PrefetchingIterator ? ( PrefetchingIterator<T> ) iterator : new PrefetchingIteratorAnonymousInnerClass4( iterator );
		 }

		 private class PrefetchingIteratorAnonymousInnerClass4 : PrefetchingIterator<T>
		 {
			 private IEnumerator<T> _iterator;

			 public PrefetchingIteratorAnonymousInnerClass4( IEnumerator<T> iterator )
			 {
				 this._iterator = iterator;
			 }

			 protected internal override T fetchNextOrNull()
			 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  return _iterator.hasNext() ? _iterator.next() : null;
			 }
		 }

		 /// <summary>
		 /// Create a stream from the given iterator.
		 /// <para>
		 /// <b>Note:</b> returned stream needs to be closed via <seealso cref="Stream.close()"/> if the given iterator implements
		 /// <seealso cref="Resource"/>.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="iterator"> the iterator to convert to stream </param>
		 /// @param <T> the type of elements in the given iterator </param>
		 /// <returns> stream over the iterator elements </returns>
		 /// <exception cref="NullPointerException"> when the given stream is {@code null} </exception>
		 public static Stream<T> Stream<T>( IEnumerator<T> iterator )
		 {
			  return Stream( iterator, 0 );
		 }

		 /// <summary>
		 /// Create a stream from the given iterator with given characteristics.
		 /// <para>
		 /// <b>Note:</b> returned stream needs to be closed via <seealso cref="Stream.close()"/> if the given iterator implements
		 /// <seealso cref="Resource"/>.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="iterator"> the iterator to convert to stream </param>
		 /// <param name="characteristics"> the logical OR of characteristics for the underlying <seealso cref="Spliterator"/> </param>
		 /// @param <T> the type of elements in the given iterator </param>
		 /// <returns> stream over the iterator elements </returns>
		 /// <exception cref="NullPointerException"> when the given iterator is {@code null} </exception>
		 public static Stream<T> Stream<T>( IEnumerator<T> iterator, int characteristics )
		 {
			  Objects.requireNonNull( iterator );
			  Spliterator<T> spliterator = Spliterators.spliteratorUnknownSize( iterator, characteristics );
			  Stream<T> stream = StreamSupport.stream( spliterator, false );
			  if ( iterator is Resource )
			  {
					return stream.onClose( ( ( Resource ) iterator ).close );
			  }
			  return stream;
		 }

		 private class EmptyResourceIterator<E> : ResourceIterator<E>
		 {
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  internal static readonly ResourceIterator<object> EmptyResourceIteratorConflict = new Iterators.EmptyResourceIterator<object>();

			  public override void Close()
			  {
			  }

			  public override bool HasNext()
			  {
					return false;
			  }

			  public override E Next()
			  {
					throw new NoSuchElementException();
			  }
		 }
	}

}