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
namespace Neo4Net.Collections.Helpers
{
	using ImmutableList = org.eclipse.collections.api.list.ImmutableList;
	using ImmutableMap = org.eclipse.collections.api.map.ImmutableMap;
	using ImmutableListFactoryImpl = org.eclipse.collections.impl.list.immutable.ImmutableListFactoryImpl;
	using ImmutableMapFactoryImpl = org.eclipse.collections.impl.map.immutable.ImmutableMapFactoryImpl;


	using Predicates = Neo4Net.Functions.Predicates;
	using Neo4Net.Functions;
	using Resource = Neo4Net.GraphDb.Resource;
	using Neo4Net.GraphDb;
	using Neo4Net.GraphDb;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterators.asResourceIterator;

	public sealed class Iterables
	{
		 private Iterables()
		 {
			  throw new AssertionError( "no instance" );
		 }

		 public static IEnumerable<T> Empty<T>()
		 {
			  return Collections.emptyList();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static <T> Iterable<T> emptyResourceIterable()
		 public static IEnumerable<T> EmptyResourceIterable<T>()
		 {
			  return ( IEnumerable<T> ) EmptyResourceIterable.EmptyResourceIterableConflict;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T> Iterable<T> limit(final int limitItems, final Iterable<T> iterable)
		 public static IEnumerable<T> Limit<T>( int limitItems, IEnumerable<T> iterable )
		 {
			  return () =>
			  {
				IEnumerator<T> iterator = iterable.GetEnumerator();

				return new IteratorAnonymousInnerClass( limitItems, iterator );
			  };
		 }

		 private class IteratorAnonymousInnerClass : IEnumerator<T>
		 {
			 private int _limitItems;
			 private IEnumerator<T> _iterator;

			 public IteratorAnonymousInnerClass( int limitItems, IEnumerator<T> iterator )
			 {
				 this._limitItems = limitItems;
				 this._iterator = iterator;
			 }

			 internal int count;

			 public bool hasNext()
			 {
				  return count < _limitItems && _iterator.hasNext();
			 }

			 public T next()
			 {
				  count++;
				  return _iterator.next();
			 }

			 public void remove()
			 {
				  _iterator.remove();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T> System.Func<Iterable<T>, Iterable<T>> limit(final int limitItems)
		 public static System.Func<IEnumerable<T>, IEnumerable<T>> Limit<T>( int limitItems )
		 {
			  return ts => Limit( limitItems, ts );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T> Iterable<T> unique(final Iterable<T> iterable)
		 public static IEnumerable<T> Unique<T>( IEnumerable<T> iterable )
		 {
			  return () =>
			  {
				IEnumerator<T> iterator = iterable.GetEnumerator();

				return new IteratorAnonymousInnerClass2( iterator );
			  };
		 }

		 private class IteratorAnonymousInnerClass2 : IEnumerator<T>
		 {
			 private IEnumerator<T> _iterator;

			 public IteratorAnonymousInnerClass2( IEnumerator<T> iterator )
			 {
				 this._iterator = iterator;
			 }

			 internal ISet<T> items = new HashSet<T>();
			 internal T nextItem;

			 public bool hasNext()
			 {
				  while ( _iterator.hasNext() )
				  {
						nextItem = _iterator.next();
						if ( items.add( nextItem ) )
						{
							 return true;
						}
				  }

				  return false;
			 }

			 public T next()
			 {
				  if ( nextItem == null && !hasNext() )
				  {
						throw new NoSuchElementException();
				  }

				  return nextItem;
			 }

			 public void remove()
			 {
			 }
		 }

		 public static C AddAll<T, C, T1>( C collection, IEnumerable<T1> iterable ) where C : ICollection<T> where T1 : T
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<? extends T> iterator = iterable.iterator();
			  IEnumerator<T> iterator = iterable.GetEnumerator();
			  try
			  {
					while ( iterator.MoveNext() )
					{
						 collection.add( iterator.Current );
					}
			  }
			  finally
			  {
					if ( iterator is IDisposable )
					{
						 try
						 {
							  ( ( IDisposable ) iterator ).close();
						 }
						 catch ( Exception )
						 {
							  // Ignore
						 }
					}
			  }

			  return collection;
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static <X> Iterable<X> filter(System.Predicate<? super X> specification, Iterable<X> i)
		 public static IEnumerable<X> Filter<X, T1>( System.Predicate<T1> specification, IEnumerable<X> i )
		 {
			  return new FilterIterable<X>( i, specification );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <X> Iterable<X> skip(final int skip, final Iterable<X> iterable)
		 public static IEnumerable<X> Skip<X>( int skip, IEnumerable<X> iterable )
		 {
			  return () =>
			  {
				IEnumerator<X> iterator = iterable.GetEnumerator();

				for ( int i = 0; i < skip; i++ )
				{
					 if ( iterator.hasNext() )
					 {
						  iterator.next();
					 }
					 else
					 {
						  return Collections.emptyIterator();
					 }
				}

				return iterator;
			  };
		 }

		 public static IEnumerable<X> Reverse<X>( IEnumerable<X> iterable )
		 {
			  IList<X> list = new IList<X> { iterable };
			  list.Reverse();
			  return list;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public static <X, I extends Iterable<? extends X>> Iterable<X> flatten(I... multiIterator)
		 public static IEnumerable<X> Flatten<X, I>( params I[] multiIterator )
		 {
			  return new FlattenIterable<X>( Arrays.asList( multiIterator ) );
		 }

		 public static IEnumerable<X> FlattenIterable<X, S, I>( I multiIterator ) where I : IEnumerable<S>
		 {
			  return new FlattenIterable<X>( multiIterator );
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static <FROM, TO> Iterable<TO> map(System.Func<? super FROM, ? extends TO> function, Iterable<FROM> from)
		 public static IEnumerable<TO> Map<FROM, TO, T1>( System.Func<T1> function, IEnumerable<FROM> from ) where T1 : TO
		 {
			  return new MapIterable<TO>( from, function );
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static <FROM, TO> Iterable<TO> flatMap(System.Func<? super FROM, ? extends Iterable<TO>> function, Iterable<FROM> from)
		 public static IEnumerable<TO> FlatMap<FROM, TO, T1>( System.Func<T1> function, IEnumerable<FROM> from ) where T1 : IEnumerable<TO>
		 {
			  return new CombiningIterable<TO>( Map( function, from ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs @SuppressWarnings("unchecked") public static <T, C extends T> Iterable<T> iterable(C... items)
		 public static IEnumerable<T> Iterable<T, C>( params C[] items ) where C : T
		 {
			  return ( IEnumerable<T> ) Arrays.asList( items );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static <T, C> Iterable<T> cast(Iterable<C> iterable)
		 public static IEnumerable<T> Cast<T, C>( IEnumerable<C> iterable )
		 {
			  return ( IEnumerable<T> ) iterable;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs @SuppressWarnings("unchecked") public static <T> Iterable<T> concat(Iterable<? extends T>... iterables)
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static IEnumerable<T> Concat<T>( params IEnumerable<T>[] iterables )
		 {
			  return Concat( Arrays.asList( ( IEnumerable<T>[] ) iterables ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T> Iterable<T> concat(final Iterable<? extends Iterable<T>> iterables)
		 public static IEnumerable<T> Concat<T, T1>( IEnumerable<T1> iterables ) where T1 : IEnumerable<T>
		 {
			  return new CombiningIterable<T>( iterables );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T, C extends T> Iterable<T> prepend(final C item, final Iterable<T> iterable)
		 public static IEnumerable<T> Prepend<T, C>( C item, IEnumerable<T> iterable ) where C : T
		 {
			  return () => new IteratorAnonymousInnerClass(item, iterable);
		 }

		 private class IteratorAnonymousInnerClass : IEnumerator<T>
		 {
			 private T _item;
			 private IEnumerable<T> _iterable;

			 public IteratorAnonymousInnerClass( T item, IEnumerable<T> iterable )
			 {
				 this._item = item;
				 this._iterable = iterable;
			 }

			 internal T first = _item;
			 internal IEnumerator<T> _iterator;

			 public bool hasNext()
			 {
				  if ( first != null )
				  {
						return true;
				  }
				  if ( _iterator == null )
				  {
						_iterator = _iterable.GetEnumerator();
				  }

				  return _iterator.hasNext();
			 }

			 public T next()
			 {
				  if ( first != null )
				  {
						try
						{
							 return first;
						}
						finally
						{
							 first = null;
						}
				  }
				  return _iterator.next();
			 }

			 public void remove()
			 {
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T, C extends T> Iterable<T> append(final C item, final Iterable<T> iterable)
		 public static IEnumerable<T> Append<T, C>( C item, IEnumerable<T> iterable ) where C : T
		 {
			  return () =>
			  {
				IEnumerator<T> iterator = iterable.GetEnumerator();

				return new IteratorAnonymousInnerClass3( item, iterator );
			  };
		 }

		 private class IteratorAnonymousInnerClass3 : IEnumerator<T>
		 {
			 private T _item;
			 private IEnumerator<T> _iterator;

			 public IteratorAnonymousInnerClass3( T item, IEnumerator<T> iterator )
			 {
				 this._item = item;
				 this._iterator = iterator;
			 }

			 internal T last = _item;

			 public bool hasNext()
			 {
				  return _iterator.hasNext() || last != null;
			 }

			 public T next()
			 {
				  if ( _iterator.hasNext() )
				  {
						return _iterator.next();
				  }
				  try
				  {
						return last;
				  }
				  finally
				  {
						last = null;
				  }
			 }

			 public void remove()
			 {
			 }
		 }

		 public static IEnumerable<T> Cache<T>( IEnumerable<T> iterable )
		 {
			  return new CacheIterable<T>( iterable );
		 }

		 public static object[] AsArray( IEnumerable<object> iterable )
		 {
			  return AsArray( typeof( object ), iterable );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static <T> T[] asArray(Class<T> componentType, Iterable<T> iterable)
		 public static T[] AsArray<T>( Type componentType, IEnumerable<T> iterable )
		 {
				 componentType = typeof( T );
			  if ( iterable == null )
			  {
					return null;
			  }

			  IList<T> list = new IList<T> { iterable };
			  return list.toArray( ( T[] ) Array.CreateInstance( componentType, list.Count ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T> Neo4Net.graphdb.ResourceIterable<T> asResourceIterable(final Iterable<T> iterable)
		 public staticIResourceIterable<T> AsResourceIterable<T>( IEnumerable<T> iterable )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: if (iterable instanceof Neo4Net.graphdb.ResourceIterable<?>)
			  if ( iterable isIResourceIterable<object> )
			  {
					return (IResourceIterable<T> ) iterable;
			  }
			  return () => asResourceIterator(iterable.GetEnumerator());
		 }

		 public static string ToString<T1>( IEnumerable<T1> values, string separator )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.Iterator<?> it = values.iterator();
			  IEnumerator<object> it = values.GetEnumerator();
			  StringBuilder sb = new StringBuilder();
			  while ( it.MoveNext() )
			  {
					sb.Append( it.Current.ToString() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( it.hasNext() )
					{
						 sb.Append( separator );
					}
			  }
			  return sb.ToString();
		 }

		 /// <summary>
		 /// Returns the given iterable's first element or {@code null} if no
		 /// element found.
		 /// 
		 /// If the <seealso cref="Iterable.iterator() iterator"/> created by the {@code iterable} implements <seealso cref="Resource"/>
		 /// it will be <seealso cref="Resource.close() closed"/> in a {@code finally} block after the single item
		 /// has been retrieved, or failed to be retrieved.
		 /// </summary>
		 /// @param <T> the type of elements in {@code iterable}. </param>
		 /// <param name="iterable"> the <seealso cref="System.Collections.IEnumerable"/> to get elements from. </param>
		 /// <returns> the first element in the {@code iterable}, or {@code null} if no
		 /// element found. </returns>
		 public static T FirstOrNull<T>( IEnumerable<T> iterable )
		 {
			  IEnumerator<T> iterator = iterable.GetEnumerator();
			  try
			  {
					return Iterators.FirstOrNull( iterator );
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
		 /// Returns the given iterable's first element. If no element is found a
		 /// <seealso cref="NoSuchElementException"/> is thrown.
		 /// </summary>
		 /// @param <T> the type of elements in {@code iterable}. </param>
		 /// <param name="iterable"> the <seealso cref="System.Collections.IEnumerable"/> to get elements from. </param>
		 /// <returns> the first element in the {@code iterable}. </returns>
		 /// <exception cref="NoSuchElementException"> if no element found. </exception>
		 public static T First<T>( IEnumerable<T> iterable )
		 {
			  return Iterators.First( iterable.GetEnumerator() );
		 }

		 /// <summary>
		 /// Returns the given iterable's last element or {@code null} if no
		 /// element found.
		 /// </summary>
		 /// @param <T> the type of elements in {@code iterable}. </param>
		 /// <param name="iterable"> the <seealso cref="System.Collections.IEnumerable"/> to get elements from. </param>
		 /// <returns> the last element in the {@code iterable}, or {@code null} if no
		 /// element found. </returns>
		 public static T LastOrNull<T>( IEnumerable<T> iterable )
		 {
			  return Iterators.LastOrNull( iterable.GetEnumerator() );
		 }

		 /// <summary>
		 /// Returns the given iterable's last element. If no element is found a
		 /// <seealso cref="NoSuchElementException"/> is thrown.
		 /// </summary>
		 /// @param <T> the type of elements in {@code iterable}. </param>
		 /// <param name="iterable"> the <seealso cref="System.Collections.IEnumerable"/> to get elements from. </param>
		 /// <returns> the last element in the {@code iterable}. </returns>
		 /// <exception cref="NoSuchElementException"> if no element found. </exception>
		 public static T Last<T>( IEnumerable<T> iterable )
		 {
			  return Iterators.Last( iterable.GetEnumerator() );
		 }

		 /// <summary>
		 /// Returns the given iterable's single element or {@code null} if no
		 /// element found. If there is more than one element in the iterable a
		 /// <seealso cref="NoSuchElementException"/> will be thrown.
		 /// 
		 /// If the <seealso cref="Iterable.iterator() iterator"/> created by the {@code iterable} implements <seealso cref="Resource"/>
		 /// it will be <seealso cref="Resource.close() closed"/> in a {@code finally} block after the single item
		 /// has been retrieved, or failed to be retrieved.
		 /// </summary>
		 /// @param <T> the type of elements in {@code iterable}. </param>
		 /// <param name="iterable"> the <seealso cref="System.Collections.IEnumerable"/> to get elements from. </param>
		 /// <returns> the single element in {@code iterable}, or {@code null} if no
		 /// element found. </returns>
		 /// <exception cref="NoSuchElementException"> if more than one element was found. </exception>
		 public static T SingleOrNull<T>( IEnumerable<T> iterable )
		 {
			  return Iterators.SingleOrNull( iterable.GetEnumerator() );
		 }

		 /// <summary>
		 /// Returns the given iterable's single element. If there are no elements
		 /// or more than one element in the iterable a <seealso cref="NoSuchElementException"/>
		 /// will be thrown.
		 /// 
		 /// If the <seealso cref="Iterable.iterator() iterator"/> created by the {@code iterable} implements <seealso cref="Resource"/>
		 /// it will be <seealso cref="Resource.close() closed"/> in a {@code finally} block after the single item
		 /// has been retrieved, or failed to be retrieved.
		 /// </summary>
		 /// @param <T> the type of elements in {@code iterable}. </param>
		 /// <param name="iterable"> the <seealso cref="System.Collections.IEnumerable"/> to get elements from. </param>
		 /// <returns> the single element in the {@code iterable}. </returns>
		 /// <exception cref="NoSuchElementException"> if there isn't exactly one element. </exception>
		 public static T Single<T>( IEnumerable<T> iterable )
		 {
			  return Iterators.Single( iterable.GetEnumerator() );
		 }

		 /// <summary>
		 /// Returns the given iterable's single element or {@code itemIfNone} if no
		 /// element found. If there is more than one element in the iterable a
		 /// <seealso cref="NoSuchElementException"/> will be thrown.
		 /// 
		 /// If the <seealso cref="Iterable.iterator() iterator"/> created by the {@code iterable} implements <seealso cref="Resource"/>
		 /// it will be <seealso cref="Resource.close() closed"/> in a {@code finally} block after the single item
		 /// has been retrieved, or failed to be retrieved.
		 /// </summary>
		 /// @param <T> the type of elements in {@code iterable}. </param>
		 /// <param name="iterable"> the <seealso cref="System.Collections.IEnumerable"/> to get elements from. </param>
		 /// <param name="itemIfNone"> item to use if none is found </param>
		 /// <returns> the single element in {@code iterable}, or {@code null} if no
		 /// element found. </returns>
		 /// <exception cref="NoSuchElementException"> if more than one element was found. </exception>
		 public static T Single<T>( IEnumerable<T> iterable, T itemIfNone )
		 {
			  return Iterators.Single( iterable.GetEnumerator(), itemIfNone );
		 }

		 /// <summary>
		 /// Returns the iterator's n:th item from the end of the iteration.
		 /// If the iterator has got less than n-1 items in it
		 /// <seealso cref="NoSuchElementException"/> is thrown.
		 /// </summary>
		 /// @param <T> the type of elements in {@code iterator}. </param>
		 /// <param name="iterable"> the <seealso cref="System.Collections.IEnumerable"/> to get elements from. </param>
		 /// <param name="n"> the n:th item from the end to get. </param>
		 /// <returns> the iterator's n:th item from the end of the iteration. </returns>
		 /// <exception cref="NoSuchElementException"> if the iterator contains less than n-1 items. </exception>
		 public static T FromEnd<T>( IEnumerable<T> iterable, int n )
		 {
			  return Iterators.FromEnd( iterable.GetEnumerator(), n );
		 }

		 /// <summary>
		 /// Adds all the items in {@code iterator} to {@code collection}. </summary>
		 /// @param <C> the type of <seealso cref="System.Collections.ICollection"/> to add to items to. </param>
		 /// @param <T> the type of items in the collection and iterator. </param>
		 /// <param name="iterable"> the <seealso cref="System.Collections.IEnumerator"/> to grab the items from. </param>
		 /// <param name="collection"> the <seealso cref="System.Collections.ICollection"/> to add the items to. </param>
		 /// <returns> the {@code collection} which was passed in, now filled
		 /// with the items from {@code iterator}. </returns>
		 public static C AddToCollection<C, T>( IEnumerable<T> iterable, C collection ) where C : ICollection<T>
		 {
			  return Iterators.AddToCollection( iterable.GetEnumerator(), collection );
		 }

		 /// <summary>
		 /// Counts the number of items in the {@code iterator} by looping
		 /// through it. </summary>
		 /// @param <T> the type of items in the iterator. </param>
		 /// <param name="iterable"> the <seealso cref="System.Collections.IEnumerable"/> to count items in. </param>
		 /// <returns> the number of items found in {@code iterable}. </returns>
		 public static long Count<T>( IEnumerable<T> iterable )
		 {
			  return Count( iterable, Predicates.alwaysTrue() );
		 }

		 /// <summary>
		 /// Counts the number of filtered items in the {@code iterable} by looping through it.
		 /// </summary>
		 /// @param <T> the type of items in the iterator. </param>
		 /// <param name="iterable"> the <seealso cref="System.Collections.IEnumerable"/> to count items in. </param>
		 /// <param name="filter"> the filter to test items against </param>
		 /// <returns> the number of found in {@code iterable}. </returns>
		 public static long Count<T>( IEnumerable<T> iterable, System.Predicate<T> filter )
		 {
			  IEnumerator<T> iterator = iterable.GetEnumerator();
			  try
			  {
					return Iterators.Count( iterator, filter );
			  }
			  finally
			  {
					if ( iterator is ResourceIterator )
					{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ((Neo4Net.graphdb.ResourceIterator<?>) iterator).close();
						 ( ( IResourceIterator<object> ) iterator ).close();
					}
			  }
		 }

		 /// <summary>
		 /// Creates a collection from an iterable.
		 /// </summary>
		 /// <param name="iterable"> The iterable to create the collection from. </param>
		 /// @param <T> The generic type of both the iterable and the collection. </param>
		 /// <returns> a collection containing all items from the iterable. </returns>
		 public static ICollection<T> AsCollection<T>( IEnumerable<T> iterable )
		 {
			  return AddToCollection( iterable, new List<>() );
		 }

		 public static IList<T> AsList<T>( IEnumerable<T> iterator )
		 {
			  return AddToCollection( iterator, new List<>() );
		 }

		 public static ImmutableList<T> AsImmutableList<T, T1>( IEnumerable<T1> iterator ) where T1 : T
		 {
			  return ImmutableListFactoryImpl.INSTANCE.ofAll( iterator );
		 }

		 public static ImmutableList<T> AsImmutableList<T>( IEnumerator<T> iterator )
		 {
			  return AsImmutableList( () => iterator );
		 }

		 public static IDictionary<T, U> AsMap<T, U>( IEnumerable<Pair<T, U>> pairs )
		 {
			  IDictionary<T, U> map = new Dictionary<T, U>();
			  foreach ( Pair<T, U> pair in pairs )
			  {
					map[pair.First()] = pair.Other();
			  }
			  return map;
		 }

		 public static ImmutableMap<T, U> AsImmutableMap<T, U>( IDictionary<T, U> map )
		 {
			  return ImmutableMapFactoryImpl.INSTANCE.ofAll( map );
		 }

		 public static ImmutableMap<T, U> AsImmutableMap<T, U>( IEnumerable<Pair<T, U>> pairs )
		 {
			  return AsImmutableMap( AsMap( pairs ) );
		 }

		 /// <summary>
		 /// Creates a <seealso cref="System.Collections.Generic.ISet<object>"/> from an <seealso cref="System.Collections.IEnumerable"/>.
		 /// </summary>
		 /// <param name="iterable"> The items to create the set from. </param>
		 /// @param <T> The generic type of items. </param>
		 /// <returns> a set containing all items from the <seealso cref="System.Collections.IEnumerable"/>. </returns>
		 public static ISet<T> AsSet<T>( IEnumerable<T> iterable )
		 {
			  return AddToCollection( iterable, new HashSet<>() );
		 }

		 /// <summary>
		 /// Creates a <seealso cref="System.Collections.Generic.ISet<object>"/> from an <seealso cref="System.Collections.IEnumerable"/>.
		 /// </summary>
		 /// <param name="iterable"> The items to create the set from. </param>
		 /// @param <T> The generic type of items. </param>
		 /// <returns> a set containing all items from the <seealso cref="System.Collections.IEnumerable"/>. </returns>
		 public static ISet<T> AsUniqueSet<T>( IEnumerable<T> iterable )
		 {
			  return Iterators.AddToCollectionUnique( iterable, new HashSet<>() );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Iterable<long> asIterable(final long... array)
		 public static IEnumerable<long> AsIterable( params long[] array )
		 {
			  return () => Iterators.asIterator(array);
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Iterable<int> asIterable(final int... array)
		 public static IEnumerable<int> AsIterable( params int[] array )
		 {
			  return () => Iterators.asIterator(array);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public static <T> Iterable<T> asIterable(final T... array)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public static IEnumerable<T> AsIterable<T>( params T[] array )
		 {
			  return () => Iterators.Iterator(array);
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T> Neo4Net.graphdb.ResourceIterable<T> resourceIterable(final Iterable<T> iterable)
		 public staticIResourceIterable<T>IResourceIterable<T>( IEnumerable<T> iterable )
		 {
			  return () => Iterators.ResourceIterator(iterable.GetEnumerator(), Neo4Net.GraphDb.Resource_Fields.Empty);
		 }

		 private class FlattenIterable<T, I> : IEnumerable<T>
		 {
			  internal readonly IEnumerable<I> Iterable;

			  internal FlattenIterable( IEnumerable<I> iterable )
			  {
					this.Iterable = iterable;
			  }

			  public override IEnumerator<T> Iterator()
			  {
					return new FlattenIterator<T>( Iterable.GetEnumerator() );
			  }

			  internal class FlattenIterator<T, I> : IEnumerator<T>
			  {
					internal readonly IEnumerator<I> Iterator;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private java.util.Iterator<? extends T> currentIterator;
					internal IEnumerator<T> CurrentIterator;

					internal FlattenIterator( IEnumerator<I> iterator )
					{
						 this.Iterator = iterator;
						 CurrentIterator = null;
					}

					public override bool HasNext()
					{
						 if ( CurrentIterator == null )
						 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							  if ( Iterator.hasNext() )
							  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
									I next = Iterator.next();
									CurrentIterator = next.GetEnumerator();
							  }
							  else
							  {
									return false;
							  }
						 }

						 while ( !CurrentIterator.MoveNext() && Iterator.MoveNext() )
						 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							  CurrentIterator = Iterator.next().GetEnumerator();
						 }

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 return CurrentIterator.hasNext();
					}

					public override T Next()
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 return CurrentIterator.next();
					}

					public override void Remove()
					{
						 if ( CurrentIterator == null )
						 {
							  throw new System.InvalidOperationException();
						 }

//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						 CurrentIterator.remove();
					}
			  }
		 }

		 private class CacheIterable<T> : IEnumerable<T>
		 {
			  internal readonly IEnumerable<T> Iterable;
			  internal IEnumerable<T> Cache;

			  internal CacheIterable( IEnumerable<T> iterable )
			  {
					this.Iterable = iterable;
			  }

			  public override IEnumerator<T> Iterator()
			  {
					if ( Cache != null )
					{
						 return Cache.GetEnumerator();
					}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Iterator<T> source = iterable.iterator();
					IEnumerator<T> source = Iterable.GetEnumerator();

					return new IteratorAnonymousInnerClass( this, source );
			  }

			  private class IteratorAnonymousInnerClass : IEnumerator<T>
			  {
				  private readonly CacheIterable<T> _outerInstance;

				  private IEnumerator<T> _source;

				  public IteratorAnonymousInnerClass( CacheIterable<T> outerInstance, IEnumerator<T> source )
				  {
					  this.outerInstance = outerInstance;
					  this._source = source;
					  iteratorCache = new List<>();
				  }

				  internal IList<T> iteratorCache;

				  public bool hasNext()
				  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						bool hasNext = _source.hasNext();
						if ( !hasNext )
						{
							 _outerInstance.cache = iteratorCache;
						}
						return hasNext;
				  }

				  public T next()
				  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						T next = _source.next();
						iteratorCache.add( next );
						return next;
				  }

				  public void remove()
				  {

				  }
			  }
		 }

		 /// <summary>
		 /// Returns the index of the first occurrence of the specified element
		 /// in this iterable, or -1 if this iterable does not contain the element.
		 /// More formally, returns the lowest index {@code i} such that
		 /// {@code (o==null ? get(i)==null : o.equals(get(i)))},
		 /// or -1 if there is no such index.
		 /// </summary>
		 /// <param name="itemToFind"> element to find </param>
		 /// <param name="iterable"> iterable to look for the element in </param>
		 /// @param <T> the type of the elements </param>
		 /// <returns> the index of the first occurrence of the specified element
		 ///         (or {@code null} if that was specified) or {@code -1} </returns>
		 public static int IndexOf<T>( T itemToFind, IEnumerable<T> iterable )
		 {
			  if ( itemToFind == default( T ) )
			  {
					int index = 0;
					foreach ( T item in iterable )
					{
						 if ( item == default( T ) )
						 {
							  return index;
						 }
						 index++;
					}
			  }
			  else
			  {
					int index = 0;
					foreach ( T item in iterable )
					{
						 if ( itemToFind.Equals( item ) )
						 {
							  return index;
						 }
						 index++;
					}
			  }
			  return -1;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T> Iterable<T> option(final T item)
		 public static IEnumerable<T> Option<T>( T item )
		 {
			  if ( item == default( T ) )
			  {
					return Collections.emptyList();
			  }

			  return () => Iterators.Iterator(item);
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public static <T, S extends Comparable<? super S>> Iterable<T> sort(Iterable<T> iterable, final System.Func<T, S> compareFunction)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public static IEnumerable<T> Sort<T, S>( IEnumerable<T> iterable, System.Func<T, S> compareFunction )
		 {
			  IList<T> list = new IList<T> { iterable };
			  list.sort( System.Collections.IComparer.comparing( compareFunction ) );
			  return list;
		 }

		 public static string Join<T1>( string joinString, IEnumerable<T1> iter )
		 {
			  return Iterators.Join( joinString, iter.GetEnumerator() );
		 }

		 /// <summary>
		 /// Create a stream from the given iterable.
		 /// <para>
		 /// <b>Note:</b> returned stream needs to be closed via <seealso cref="Stream.close()"/> if the given iterable implements
		 /// <seealso cref="Resource"/>.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="iterable"> the iterable to convert to stream </param>
		 /// @param <T> the type of elements in the given iterable </param>
		 /// <returns> stream over the iterable elements </returns>
		 /// <exception cref="NullPointerException"> when the given iterable is {@code null} </exception>
		 public static Stream<T> Stream<T>( IEnumerable<T> iterable )
		 {
			  return Stream( iterable, 0 );
		 }

		 /// <summary>
		 /// Create a stream from the given iterable with given characteristics.
		 /// <para>
		 /// <b>Note:</b> returned stream needs to be closed via <seealso cref="Stream.close()"/> if the given iterable implements
		 /// <seealso cref="Resource"/>.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <param name="iterable"> the iterable to convert to stream </param>
		 /// <param name="characteristics"> the logical OR of characteristics for the underlying <seealso cref="Spliterator"/> </param>
		 /// @param <T> the type of elements in the given iterable </param>
		 /// <returns> stream over the iterable elements </returns>
		 /// <exception cref="NullPointerException"> when the given iterable is {@code null} </exception>
		 public static Stream<T> Stream<T>( IEnumerable<T> iterable, int characteristics )
		 {
			  Objects.requireNonNull( iterable );
			  return Iterators.Stream( iterable.GetEnumerator(), characteristics );
		 }

		 /// <summary>
		 /// Method for calling a lambda function on many objects when it is expected that the function might
		 /// throw an exception. First exception will be thrown and subsequent will be suppressed.
		 /// This method guarantees that all subjects will be consumed, unless <seealso cref="System.OutOfMemoryException"/> or some other serious error happens.
		 /// </summary>
		 /// <param name="consumer"> lambda function to call on each object passed </param>
		 /// <param name="subjects"> <seealso cref="System.Collections.IEnumerable"/> of objects to call the function on </param>
		 /// @param <E> the type of exception anticipated, inferred from the lambda </param>
		 /// <exception cref="E"> if consumption fails with this exception </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static <T, E extends Exception> void safeForAll(Neo4Net.function.ThrowingConsumer<T,E> consumer, Iterable<T> subjects) throws E
		 public static void SafeForAll<T, E>( ThrowingConsumer<T, E> consumer, IEnumerable<T> subjects ) where E : Exception
		 {
			  E exception = null;
			  foreach ( T instance in subjects )
			  {
					try
					{
						 consumer.Accept( instance );
					}
					catch ( Exception e )
					{
						 exception = Exceptions.chain( exception, ( E ) e );
					}
			  }
			  if ( exception != null )
			  {
					throw exception;
			  }
		 }

		 private class EmptyResourceIterable<T> :IResourceIterable<T>
		 {
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  internal static readonlyIResourceIterable<object> EmptyResourceIterableConflict = new EmptyResourceIterable<object>();

			  public override IResourceIterator<T> Iterator()
			  {
					return Iterators.EmptyResourceIterator();
			  }
		 }
	}

}