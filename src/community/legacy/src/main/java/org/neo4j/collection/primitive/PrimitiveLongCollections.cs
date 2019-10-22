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
namespace Neo4Net.Collections.primitive
{

	using Empty = Neo4Net.Collections.primitive.@base.Empty;
	using Resource = Neo4Net.GraphDb.Resource;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.collection.primitive.PrimitiveCommons.closeSafely;

	/// <summary>
	/// Basic and common primitive int collection utils and manipulations.
	/// </summary>
	/// <seealso cref= PrimitiveIntCollections </seealso>
	/// <seealso cref= Primitive </seealso>
	public class PrimitiveLongCollections
	{
		 public static readonly long[] EmptyLongArray = new long[0];

		 private static readonly PrimitiveLongIterator EMPTY = new PrimitiveLongBaseIteratorAnonymousInnerClass();

		 private class PrimitiveLongBaseIteratorAnonymousInnerClass : PrimitiveLongBaseIterator
		 {
			 protected internal override bool fetchNext()
			 {
				  return false;
			 }
		 }

		 private PrimitiveLongCollections()
		 {
			  throw new AssertionError( "no instance" );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static PrimitiveLongIterator iterator(final long... items)
		 public static PrimitiveLongIterator Iterator( params long[] items )
		 {
			  return new PrimitiveLongBaseResourceIteratorAnonymousInnerClass( Neo4Net.GraphDb.Resource_Fields.Empty, items );
		 }

		 private class PrimitiveLongBaseResourceIteratorAnonymousInnerClass : PrimitiveLongResourceCollections.PrimitiveLongBaseResourceIterator
		 {
			 private long[] _items;

			 public PrimitiveLongBaseResourceIteratorAnonymousInnerClass( Resource org, long[] items ) : base( org.Neo4Net.graphdb.Resource_Fields.Empty )
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
		 public static PrimitiveLongIterator Concat( params PrimitiveLongIterator[] primitiveLongIterators )
		 {
			  return Concat( Arrays.asList( primitiveLongIterators ) );
		 }

		 public static PrimitiveLongIterator Concat( IEnumerable<PrimitiveLongIterator> primitiveLongIterators )
		 {
			  return new PrimitiveLongConcatingIterator( primitiveLongIterators.GetEnumerator() );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static PrimitiveLongIterator filter(PrimitiveLongIterator source, final System.Func<long, boolean> filter)
		 public static PrimitiveLongIterator Filter( PrimitiveLongIterator source, System.Func<long, bool> filter )
		 {
			  return new PrimitiveLongFilteringIteratorAnonymousInnerClass( source, filter );
		 }

		 private class PrimitiveLongFilteringIteratorAnonymousInnerClass : PrimitiveLongFilteringIterator
		 {
			 private System.Func<long, bool> _filter;

			 public PrimitiveLongFilteringIteratorAnonymousInnerClass( Neo4Net.Collections.primitive.PrimitiveLongIterator source, System.Func<long, bool> filter ) : base( source )
			 {
				 this._filter = filter;
			 }

			 public override bool test( long item )
			 {
				  return _filter( item );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static PrimitiveLongResourceIterator filter(PrimitiveLongResourceIterator source, final System.Func<long, boolean> filter)
		 public static PrimitiveLongResourceIterator Filter( PrimitiveLongResourceIterator source, System.Func<long, bool> filter )
		 {
			  return new PrimitiveLongResourceFilteringIteratorAnonymousInnerClass( source, filter );
		 }

		 private class PrimitiveLongResourceFilteringIteratorAnonymousInnerClass : PrimitiveLongResourceFilteringIterator
		 {
			 private System.Func<long, bool> _filter;

			 public PrimitiveLongResourceFilteringIteratorAnonymousInnerClass( Neo4Net.Collections.PrimitiveLongResourceIterator source, System.Func<long, bool> filter ) : base( source )
			 {
				 this._filter = filter;
			 }

			 public override bool test( long item )
			 {
				  return _filter( item );
			 }
		 }

		 // Range
		 public static PrimitiveLongIterator Range( long start, long end )
		 {
			  return new PrimitiveLongRangeIterator( start, end );
		 }

		 public static long Single( PrimitiveLongIterator iterator, long defaultItem )
		 {
			  try
			  {
					if ( !iterator.HasNext() )
					{
						 closeSafely( iterator );
						 return defaultItem;
					}
					long item = iterator.Next();
					if ( iterator.HasNext() )
					{
						 throw new NoSuchElementException( "More than one item in " + iterator + ", first:" + item + ", second:" + iterator.Next() );
					}
					closeSafely( iterator );
					return item;
			  }
			  catch ( NoSuchElementException exception )
			  {
					closeSafely( iterator, exception );
					throw exception;
			  }
		 }

		 /// <summary>
		 /// Returns the index of the given item in the iterator(zero-based). If no items in {@code iterator}
		 /// equals {@code item} {@code -1} is returned.
		 /// </summary>
		 /// <param name="item"> the item to look for. </param>
		 /// <param name="iterator"> of items. </param>
		 /// <returns> index of found item or -1 if not found. </returns>
		 public static int IndexOf( PrimitiveLongIterator iterator, long item )
		 {
			  for ( int i = 0; iterator.HasNext(); i++ )
			  {
					if ( item == iterator.Next() )
					{
						 return i;
					}
			  }
			  return -1;
		 }

		 public static PrimitiveLongSet AsSet( ICollection<long> collection )
		 {
			  PrimitiveLongSet set = Primitive.LongSet( collection.Count );
			  foreach ( long? next in collection )
			  {
					set.Add( next.Value );
			  }
			  return set;
		 }

		 public static PrimitiveLongSet AsSet( PrimitiveLongIterator iterator )
		 {
			  PrimitiveLongSet set = Primitive.LongSet();
			  while ( iterator.HasNext() )
			  {
					set.Add( iterator.Next() );
			  }
			  return set;
		 }

		 public static PrimitiveLongSet AsSet( PrimitiveLongSet set )
		 {
			  PrimitiveLongSet result = Primitive.LongSet( set.Size() );
			  PrimitiveLongIterator iterator = set.GetEnumerator();
			  while ( iterator.HasNext() )
			  {
					result.Add( iterator.Next() );
			  }
			  return result;
		 }

		 public static PrimitiveLongSet AsSet( params long[] values )
		 {
			  PrimitiveLongSet result = Primitive.LongSet( values.Length );
			  foreach ( long value in values )
			  {
					result.Add( value );
			  }
			  return result;
		 }

		 public static PrimitiveLongObjectMap<T> Copy<T>( PrimitiveLongObjectMap<T> original )
		 {
			  PrimitiveLongObjectMap<T> copy = Primitive.LongObjectMap( original.Size() );
			  original.VisitEntries((key, value) =>
			  {
				copy.Put( key, value );
				return false;
			  });
			  return copy;
		 }

		 public static int Count( PrimitiveLongIterator iterator )
		 {
			  int count = 0;
			  for ( ; iterator.HasNext(); iterator.Next(), count++ )
			  { // Just loop through this
			  }
			  return count;
		 }

		 public static long[] AsArray( PrimitiveLongIterator iterator )
		 {
			  long[] array = new long[8];
			  int i = 0;
			  for ( ; iterator.HasNext(); i++ )
			  {
					if ( i >= array.Length )
					{
						 array = copyOf( array, i << 1 );
					}
					array[i] = iterator.Next();
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

		 public static PrimitiveLongIterator EmptyIterator()
		 {
			  return EMPTY;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static PrimitiveLongIterator toPrimitiveIterator(final java.util.Iterator<long> iterator)
		 public static PrimitiveLongIterator ToPrimitiveIterator( IEnumerator<long> iterator )
		 {
			  return new PrimitiveLongBaseIteratorAnonymousInnerClass2( iterator );
		 }

		 private class PrimitiveLongBaseIteratorAnonymousInnerClass2 : PrimitiveLongBaseIterator
		 {
			 private IEnumerator<long> _iterator;

			 public PrimitiveLongBaseIteratorAnonymousInnerClass2( IEnumerator<long> iterator )
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

		 public static PrimitiveLongSet EmptySet()
		 {
			  return Empty.EMPTY_PRIMITIVE_LONG_SET;
		 }

		 public static PrimitiveLongSet setOf( params long[] values )
		 {
			  Objects.requireNonNull( values, "Values array is null" );
			  PrimitiveLongSet set = Primitive.LongSet( values.Length );
			  foreach ( long value in values )
			  {
					set.Add( value );
			  }
			  return set;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T> java.util.Iterator<T> map(final System.Func<long, T> mapFunction, final PrimitiveLongIterator source)
		 public static IEnumerator<T> Map<T>( System.Func<long, T> mapFunction, PrimitiveLongIterator source )
		 {
			  return new IteratorAnonymousInnerClass( mapFunction, source );
		 }

		 private class IteratorAnonymousInnerClass : IEnumerator<T>
		 {
			 private System.Func<long, T> _mapFunction;
			 private Neo4Net.Collections.primitive.PrimitiveLongIterator _source;

			 public IteratorAnonymousInnerClass( System.Func<long, T> mapFunction, Neo4Net.Collections.primitive.PrimitiveLongIterator source )
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
		 /// <param name="iterator"> <seealso cref="PrimitiveLongIterator"/> to pull values from. </param>
		 /// <returns> a <seealso cref="System.Collections.IList"/> containing all items. </returns>
		 public static IList<long> AsList( PrimitiveLongIterator iterator )
		 {
			  IList<long> @out = new List<long>();
			  while ( iterator.HasNext() )
			  {
					@out.Add( iterator.Next() );
			  }
			  return @out;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static java.util.Iterator<long> toIterator(final PrimitiveLongIterator primIterator)
		 public static IEnumerator<long> ToIterator( PrimitiveLongIterator primIterator )
		 {
			  return new IteratorAnonymousInnerClass2( primIterator );
		 }

		 private class IteratorAnonymousInnerClass2 : IEnumerator<long>
		 {
			 private Neo4Net.Collections.primitive.PrimitiveLongIterator _primIterator;

			 public IteratorAnonymousInnerClass2( Neo4Net.Collections.primitive.PrimitiveLongIterator primIterator )
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
		 /// Wraps a <seealso cref="PrimitiveLongIterator"/> in a <seealso cref="PrimitiveLongResourceIterator"/> which closes
		 /// the provided {@code resource} in <seealso cref="PrimitiveLongResourceIterator.close()"/>.
		 /// </summary>
		 /// <param name="iterator"> <seealso cref="PrimitiveLongIterator"/> to convert </param>
		 /// <param name="resource"> <seealso cref="Resource"/> to close in <seealso cref="PrimitiveLongResourceIterator.close()"/> </param>
		 /// <returns> Wrapped <seealso cref="PrimitiveLongIterator"/>. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static PrimitiveLongResourceIterator resourceIterator(final PrimitiveLongIterator iterator, final org.Neo4Net.graphdb.Resource resource)
		 public static PrimitiveLongResourceIterator ResourceIterator( PrimitiveLongIterator iterator, Resource resource )
		 {
			  return new PrimitiveLongResourceIteratorAnonymousInnerClass( iterator, resource );
		 }

		 private class PrimitiveLongResourceIteratorAnonymousInnerClass : PrimitiveLongResourceIterator
		 {
			 private Neo4Net.Collections.primitive.PrimitiveLongIterator _iterator;
			 private Resource _resource;

			 public PrimitiveLongResourceIteratorAnonymousInnerClass( Neo4Net.Collections.primitive.PrimitiveLongIterator iterator, Resource resource )
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
		 /// <param name="set"> <seealso cref="PrimitiveLongSet"/> set of primitive values. </param>
		 /// <returns> a <seealso cref="System.Collections.Generic.ISet<object>"/> containing all items. </returns>
		 public static ISet<long> ToSet( PrimitiveLongSet set )
		 {
			  return ToSet( set.GetEnumerator() );
		 }

		 /// <summary>
		 /// Pulls all items from the {@code iterator} and puts them into a <seealso cref="System.Collections.Generic.ISet<object>"/>, boxing each long.
		 /// </summary>
		 /// <param name="iterator"> <seealso cref="PrimitiveLongIterator"/> to pull values from. </param>
		 /// <returns> a <seealso cref="System.Collections.Generic.ISet<object>"/> containing all items. </returns>
		 public static ISet<long> ToSet( PrimitiveLongIterator iterator )
		 {
			  ISet<long> set = new HashSet<long>();
			  while ( iterator.HasNext() )
			  {
					AddUnique( set, iterator.Next() );
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
			  int unique = 0;
			  for ( int i = 0; i < values.Length; i++ )
			  {
					long value = values[i];
					for ( int j = 0; j < unique; j++ )
					{
						 if ( value == values[j] )
						 {
							  value = -1; // signal that this value is not unique
							  break; // we will not find more than one conflict
						 }
					}
					if ( value != -1 )
					{ // this has to be done outside the inner loop, otherwise we'd never accept a single one...
						 values[unique++] = values[i];
					}
			  }
			  return unique < values.Length ? Arrays.copyOf( values, unique ) : values;
		 }

		 /// <summary>
		 /// Base iterator for simpler implementations of <seealso cref="PrimitiveLongIterator"/>s.
		 /// </summary>
		 public abstract class PrimitiveLongBaseIterator : PrimitiveLongIterator
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
			  /// </pre>
			  /// </summary>
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
//ORIGINAL LINE: private final java.util.Iterator<? extends PrimitiveLongIterator> iterators;
			  internal readonly IEnumerator<PrimitiveLongIterator> Iterators;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal PrimitiveLongIterator CurrentIteratorConflict;

			  public PrimitiveLongConcatingIterator<T1>( IEnumerator<T1> iterators ) where T1 : PrimitiveLongIterator
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

			  protected internal PrimitiveLongIterator CurrentIterator()
			  {
					return CurrentIteratorConflict;
			  }
		 }

		 public abstract class PrimitiveLongFilteringIterator : PrimitiveLongBaseIterator, System.Func<long, bool>
		 {
			  protected internal readonly PrimitiveLongIterator Source;

			  internal PrimitiveLongFilteringIterator( PrimitiveLongIterator source )
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

		 public abstract class PrimitiveLongResourceFilteringIterator : PrimitiveLongFilteringIterator, PrimitiveLongResourceIterator
		 {
			  internal PrimitiveLongResourceFilteringIterator( PrimitiveLongIterator source ) : base( source )
			  {
			  }

			  public override void Close()
			  {
					if ( Source is Resource )
					{
						 ( ( Resource ) Source ).close();
					}
			  }
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
	}

}