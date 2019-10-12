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
namespace Org.Neo4j.Collection
{
	using LongIterable = org.eclipse.collections.api.LongIterable;
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;


	using Resource = Org.Neo4j.Graphdb.Resource;

	/// <summary>
	/// Basic and common primitive int collection utils and manipulations.
	/// </summary>
	public class PrimitiveLongCollections
	{
		 public static readonly long[] EmptyLongArray = new long[0];

		 private PrimitiveLongCollections()
		 {
			  // nop
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.eclipse.collections.api.iterator.LongIterator iterator(final long... items)
		 public static LongIterator Iterator( params long[] items )
		 {
			  return new PrimitiveLongBaseResourceIteratorAnonymousInnerClass( Org.Neo4j.Graphdb.Resource_Fields.Empty, items );
		 }

		 private class PrimitiveLongBaseResourceIteratorAnonymousInnerClass : PrimitiveLongResourceCollections.PrimitiveLongBaseResourceIterator
		 {
			 private long[] _items;

			 public PrimitiveLongBaseResourceIteratorAnonymousInnerClass( Resource org, long[] items ) : base( org.neo4j.graphdb.Resource_Fields.Empty )
			 {
				 this._items = items;
			 }

			 private int index = -1;

			 protected internal override bool fetchNext()
			 {
				  return ++index < _items.Length && next( _items[index] );
			 }
		 }

		 // Concating
		 public static LongIterator Concat( params LongIterator[] longIterators )
		 {
			  return Concat( Arrays.asList( longIterators ) );
		 }

		 public static LongIterator Concat( IEnumerable<LongIterator> primitiveLongIterators )
		 {
			  return new PrimitiveLongConcatingIterator( primitiveLongIterators.GetEnumerator() );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.eclipse.collections.api.iterator.LongIterator filter(org.eclipse.collections.api.iterator.LongIterator source, final System.Func<long, boolean> filter)
		 public static LongIterator Filter( LongIterator source, System.Func<long, bool> filter )
		 {
			  return new PrimitiveLongFilteringIteratorAnonymousInnerClass( source, filter );
		 }

		 private class PrimitiveLongFilteringIteratorAnonymousInnerClass : PrimitiveLongFilteringIterator
		 {
			 private System.Func<long, bool> _filter;

			 public PrimitiveLongFilteringIteratorAnonymousInnerClass( LongIterator source, System.Func<long, bool> filter ) : base( source )
			 {
				 this._filter = filter;
			 }

			 public override bool test( long item )
			 {
				  return _filter( item );
			 }
		 }

		 // Range
		 public static LongIterator Range( long start, long end )
		 {
			  return new PrimitiveLongRangeIterator( start, end );
		 }

		 /// <summary>
		 /// Returns the index of the given item in the iterator(zero-based). If no items in {@code iterator}
		 /// equals {@code item} {@code -1} is returned.
		 /// </summary>
		 /// <param name="item"> the item to look for. </param>
		 /// <param name="iterator"> of items. </param>
		 /// <returns> index of found item or -1 if not found. </returns>
		 public static int IndexOf( LongIterator iterator, long item )
		 {
			  for ( int i = 0; iterator.hasNext(); i++ )
			  {
					if ( item == iterator.next() )
					{
						 return i;
					}
			  }
			  return -1;
		 }

		 public static MutableLongSet AsSet( ICollection<long> collection )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.primitive.MutableLongSet set = new org.eclipse.collections.impl.set.mutable.primitive.LongHashSet(collection.size());
			  MutableLongSet set = new LongHashSet( collection.Count );
			  foreach ( long? next in collection )
			  {
					set.add( next );
			  }
			  return set;
		 }

		 public static MutableLongSet AsSet( LongIterator iterator )
		 {
			  MutableLongSet set = new LongHashSet();
			  while ( iterator.hasNext() )
			  {
					set.add( iterator.next() );
			  }
			  return set;
		 }

		 public static int Count( LongIterator iterator )
		 {
			  int count = 0;
			  for ( ; iterator.hasNext(); iterator.next(), count++ )
			  { // Just loop through this
			  }
			  return count;
		 }

		 public static long[] AsArray( LongIterator iterator )
		 {
			  long[] array = new long[8];
			  int i = 0;
			  for ( ; iterator.hasNext(); i++ )
			  {
					if ( i >= array.Length )
					{
						 array = copyOf( array, i << 1 );
					}
					array[i] = iterator.next();
			  }

			  if ( i < array.Length )
			  {
					array = copyOf( array, i );
			  }
			  return array;
		 }

		 public static long[] AsArray( IEnumerator<long> iterator )
		 {
			  long[] array = new long[8];
			  int i = 0;
			  for ( ; iterator.MoveNext(); i++ )
			  {
					if ( i >= array.Length )
					{
						 array = copyOf( array, i << 1 );
					}
					array[i] = iterator.Current;
			  }

			  if ( i < array.Length )
			  {
					array = copyOf( array, i );
			  }
			  return array;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static org.eclipse.collections.api.iterator.LongIterator toPrimitiveIterator(final java.util.Iterator<long> iterator)
		 public static LongIterator ToPrimitiveIterator( IEnumerator<long> iterator )
		 {
			  return new PrimitiveLongBaseIteratorAnonymousInnerClass( iterator );
		 }

		 private class PrimitiveLongBaseIteratorAnonymousInnerClass : PrimitiveLongBaseIterator
		 {
			 private IEnumerator<long> _iterator;

			 public PrimitiveLongBaseIteratorAnonymousInnerClass( IEnumerator<long> iterator )
			 {
				 this._iterator = iterator;
			 }

			 protected internal override bool fetchNext()
			 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  if ( _iterator.hasNext() )
				  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						long? nextValue = _iterator.next();
						if ( null == nextValue )
						{
							 throw new System.ArgumentException( "Cannot convert null Long to primitive long" );
						}
						return next( nextValue.Value );
				  }
				  return false;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T> java.util.Iterator<T> map(final System.Func<long, T> mapFunction, final org.eclipse.collections.api.iterator.LongIterator source)
		 public static IEnumerator<T> Map<T>( System.Func<long, T> mapFunction, LongIterator source )
		 {
			  return new IteratorAnonymousInnerClass( mapFunction, source );
		 }

		 private class IteratorAnonymousInnerClass : IEnumerator<T>
		 {
			 private System.Func<long, T> _mapFunction;
			 private LongIterator _source;

			 public IteratorAnonymousInnerClass( System.Func<long, T> mapFunction, LongIterator source )
			 {
				 this._mapFunction = mapFunction;
				 this._source = source;
			 }

			 public bool hasNext()
			 {
				  return _source.hasNext();
			 }

			 public T next()
			 {
				  return _mapFunction( _source.next() );
			 }

			 public void remove()
			 {
				  throw new System.NotSupportedException();
			 }
		 }

		 /// <summary>
		 /// Pulls all items from the {@code iterator} and puts them into a <seealso cref="System.Collections.IList"/>, boxing each long.
		 /// </summary>
		 /// <param name="iterator"> <seealso cref="LongIterator"/> to pull values from. </param>
		 /// <returns> a <seealso cref="System.Collections.IList"/> containing all items. </returns>
		 public static IList<long> AsList( LongIterator iterator )
		 {
			  IList<long> @out = new List<long>();
			  while ( iterator.hasNext() )
			  {
					@out.Add( iterator.next() );
			  }
			  return @out;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static java.util.Iterator<long> toIterator(final org.eclipse.collections.api.iterator.LongIterator primIterator)
		 public static IEnumerator<long> ToIterator( LongIterator primIterator )
		 {
			  return new IteratorAnonymousInnerClass2( primIterator );
		 }

		 private class IteratorAnonymousInnerClass2 : IEnumerator<long>
		 {
			 private LongIterator _primIterator;

			 public IteratorAnonymousInnerClass2( LongIterator primIterator )
			 {
				 this._primIterator = primIterator;
			 }

			 public bool hasNext()
			 {
				  return _primIterator.hasNext();
			 }

			 public long? next()
			 {
				  return _primIterator.next();
			 }

			 public void remove()
			 {
				  throw new System.NotSupportedException();
			 }
		 }

		 /// <summary>
		 /// Wraps a <seealso cref="LongIterator"/> in a <seealso cref="PrimitiveLongResourceIterator"/> which closes
		 /// the provided {@code resource} in <seealso cref="PrimitiveLongResourceIterator.close()"/>.
		 /// </summary>
		 /// <param name="iterator"> <seealso cref="LongIterator"/> to convert </param>
		 /// <param name="resource"> <seealso cref="Resource"/> to close in <seealso cref="PrimitiveLongResourceIterator.close()"/> </param>
		 /// <returns> Wrapped <seealso cref="LongIterator"/>. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static PrimitiveLongResourceIterator resourceIterator(final org.eclipse.collections.api.iterator.LongIterator iterator, final org.neo4j.graphdb.Resource resource)
		 public static PrimitiveLongResourceIterator ResourceIterator( LongIterator iterator, Resource resource )
		 {
			  return new PrimitiveLongResourceIteratorAnonymousInnerClass( iterator, resource );
		 }

		 private class PrimitiveLongResourceIteratorAnonymousInnerClass : PrimitiveLongResourceIterator
		 {
			 private LongIterator _iterator;
			 private Resource _resource;

			 public PrimitiveLongResourceIteratorAnonymousInnerClass( LongIterator iterator, Resource resource )
			 {
				 this._iterator = iterator;
				 this._resource = resource;
			 }

			 public void close()
			 {
				  if ( _resource != null )
				  {
						_resource.close();
				  }
			 }

			 public long next()
			 {
				  return _iterator.next();
			 }

			 public bool hasNext()
			 {
				  return _iterator.hasNext();
			 }
		 }

		 /// <summary>
		 /// Convert primitive set into a plain old java <seealso cref="System.Collections.Generic.ISet<object>"/>, boxing each long.
		 /// </summary>
		 /// <param name="set"> <seealso cref="LongSet"/> set of primitive values. </param>
		 /// <returns> a <seealso cref="System.Collections.Generic.ISet<object>"/> containing all items. </returns>
		 public static ISet<long> ToSet( LongSet set )
		 {
			  return ToSet( set.longIterator() );
		 }

		 /// <summary>
		 /// Pulls all items from the {@code iterator} and puts them into a <seealso cref="System.Collections.Generic.ISet<object>"/>, boxing each long.
		 /// </summary>
		 /// <param name="iterator"> <seealso cref="LongIterator"/> to pull values from. </param>
		 /// <returns> a <seealso cref="System.Collections.Generic.ISet<object>"/> containing all items. </returns>
		 public static ISet<long> ToSet( LongIterator iterator )
		 {
			  ISet<long> set = new HashSet<long>();
			  while ( iterator.hasNext() )
			  {
					AddUnique( set, iterator.next() );
			  }
			  return set;
		 }

		 private static void AddUnique<T, C>( C collection, T item ) where C : ICollection<T>
		 {
			  if ( !collection.add( item ) )
			  {
					throw new System.InvalidOperationException( "Encountered an already added item:" + item + " when adding items uniquely to a collection:" + collection );
			  }
		 }

		 /// <summary>
		 /// Deduplicates values in the sorted {@code values} array.
		 /// </summary>
		 /// <param name="values"> sorted array of long values. </param>
		 /// <returns> the provided array if no duplicates were found, otherwise a new shorter array w/o duplicates. </returns>
		 public static long[] Deduplicate( long[] values )
		 {
			  if ( values.Length < 2 )
			  {
					return values;
			  }
			  long lastValue = values[0];
			  int uniqueIndex = 1;
			  for ( int i = 1; i < values.Length; i++ )
			  {
					long currentValue = values[i];
					if ( currentValue != lastValue )
					{
						 values[uniqueIndex] = currentValue;
						 lastValue = currentValue;
						 uniqueIndex++;
					}
			  }
			  return uniqueIndex < values.Length ? Arrays.copyOf( values, uniqueIndex ) : values;
		 }

		 /// <summary>
		 /// Base iterator for simpler implementations of <seealso cref="LongIterator"/>s.
		 /// </summary>
		 public abstract class PrimitiveLongBaseIterator : LongIterator
		 {
			  internal bool HasNextDecided;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool HasNextConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  protected internal long NextConflict;

			  public override bool HasNext()
			  {
					if ( !HasNextDecided )
					{
						 HasNextConflict = FetchNext();
						 HasNextDecided = true;
					}
					return HasNextConflict;
			  }

			  public override long Next()
			  {
					if ( !HasNext() )
					{
						 throw new NoSuchElementException( "No more elements in " + this );
					}
					HasNextDecided = false;
					return NextConflict;
			  }

			  /// <summary>
			  /// Fetches the next item in this iterator. Returns whether or not a next item was found. If a next
			  /// item was found, that value must have been set inside the implementation of this method
			  /// using <seealso cref="next(long)"/>.
			  /// </summary>
			  protected internal abstract bool FetchNext();

			  /// <summary>
			  /// Called from inside an implementation of <seealso cref="fetchNext()"/> if a next item was found.
			  /// This method returns {@code true} so that it can be used in short-hand conditionals
			  /// (TODO what are they called?), like:
			  /// <pre>
			  /// protected boolean fetchNext()
			  /// {
			  ///     return source.hasNext() ? next( source.next() ) : false;
			  /// }
			  /// </pre> </summary>
			  /// <param name="nextItem"> the next item found. </param>
			  protected internal virtual bool Next( long nextItem )
			  {
					NextConflict = nextItem;
					HasNextConflict = true;
					return true;
			  }
		 }

		 public class PrimitiveLongConcatingIterator : PrimitiveLongBaseIterator
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Iterator<? extends org.eclipse.collections.api.iterator.LongIterator> iterators;
			  internal readonly IEnumerator<LongIterator> Iterators;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal LongIterator CurrentIteratorConflict;

			  public PrimitiveLongConcatingIterator<T1>( IEnumerator<T1> iterators ) where T1 : org.eclipse.collections.api.iterator.LongIterator
			  {
					this.Iterators = iterators;
			  }

			  protected internal override bool FetchNext()
			  {
					if ( CurrentIteratorConflict == null || !CurrentIteratorConflict.hasNext() )
					{
						 while ( Iterators.MoveNext() )
						 {
							  CurrentIteratorConflict = Iterators.Current;
							  if ( CurrentIteratorConflict.hasNext() )
							  {
									break;
							  }
						 }
					}
					return ( CurrentIteratorConflict != null && CurrentIteratorConflict.hasNext() ) && Next(CurrentIteratorConflict.next());
			  }

			  protected internal LongIterator CurrentIterator()
			  {
					return CurrentIteratorConflict;
			  }
		 }

		 public abstract class PrimitiveLongFilteringIterator : PrimitiveLongBaseIterator, System.Func<long, bool>
		 {
			  protected internal readonly LongIterator Source;

			  internal PrimitiveLongFilteringIterator( LongIterator source )
			  {
					this.Source = source;
			  }

			  protected internal override bool FetchNext()
			  {
					while ( Source.hasNext() )
					{
						 long testItem = Source.next();
						 if ( Test( testItem ) )
						 {
							  return Next( testItem );
						 }
					}
					return false;
			  }

			  public override abstract bool Test( long testItem );
		 }

		 public class PrimitiveLongRangeIterator : PrimitiveLongBaseIterator
		 {
			  internal long Current;
			  internal readonly long End;

			  internal PrimitiveLongRangeIterator( long start, long end )
			  {
					this.Current = start;
					this.End = end;
			  }

			  protected internal override bool FetchNext()
			  {
					try
					{
						 return Current <= End && Next( Current );
					}
					finally
					{
						 Current++;
					}
			  }
		 }

		 public static MutableLongSet MergeToSet( LongIterable a, LongIterable b )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.primitive.MutableLongSet set = new org.eclipse.collections.impl.set.mutable.primitive.LongHashSet(a.size() + b.size());
			  MutableLongSet set = new LongHashSet( a.size() + b.size() );
			  set.addAll( a );
			  set.addAll( b );
			  return set;
		 }
	}

}