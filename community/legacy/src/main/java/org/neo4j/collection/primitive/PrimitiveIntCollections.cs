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
namespace Org.Neo4j.Collection.primitive
{

	using Empty = Org.Neo4j.Collection.primitive.@base.Empty;

	/// <summary>
	/// Basic and common primitive int collection utils and manipulations.
	/// </summary>
	/// <seealso cref= PrimitiveLongCollections </seealso>
	/// <seealso cref= Primitive </seealso>
	public class PrimitiveIntCollections
	{
		 private PrimitiveIntCollections()
		 {
		 }

		 /// <summary>
		 /// Base iterator for simpler implementations of <seealso cref="PrimitiveIntIterator"/>s.
		 /// </summary>
		 public abstract class PrimitiveIntBaseIterator : PrimitiveIntIterator
		 {
			  internal bool HasNextDecided;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool HasNextConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal int NextConflict;

			  public override bool HasNext()
			  {
					if ( !HasNextDecided )
					{
						 HasNextConflict = FetchNext();
						 HasNextDecided = true;
					}
					return HasNextConflict;
			  }

			  public override int Next()
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
			  /// using <seealso cref="next(int)"/>.
			  /// </summary>
			  protected internal abstract bool FetchNext();

			  /// <summary>
			  /// Called from inside an implementation of <seealso cref="fetchNext()"/> if a next item was found.
			  /// This method returns {@code true} so that it can be used in short-hand conditionals
			  /// (TODO what are they called?), like:
			  /// <pre>
			  /// @Override
			  /// protected boolean fetchNext()
			  /// {
			  ///     return source.hasNext() ? next( source.next() ) : false;
			  /// }
			  /// </pre>
			  /// </summary>
			  /// <param name="nextItem"> the next item found. </param>
			  protected internal virtual bool Next( int nextItem )
			  {
					NextConflict = nextItem;
					HasNextConflict = true;
					return true;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static PrimitiveIntIterator iterator(final int... items)
		 public static PrimitiveIntIterator Iterator( params int[] items )
		 {
			  return new PrimitiveIntBaseIteratorAnonymousInnerClass( items );
		 }

		 private class PrimitiveIntBaseIteratorAnonymousInnerClass : PrimitiveIntBaseIterator
		 {
			 private int[] _items;

			 public PrimitiveIntBaseIteratorAnonymousInnerClass( int[] items )
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
		 public static PrimitiveIntIterator Concat( IEnumerator<PrimitiveIntIterator> iterators )
		 {
			  return new PrimitiveIntConcatingIterator( iterators );
		 }

		 public class PrimitiveIntConcatingIterator : PrimitiveIntBaseIterator
		 {
			  internal readonly IEnumerator<PrimitiveIntIterator> Iterators;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal PrimitiveIntIterator CurrentIteratorConflict;

			  public PrimitiveIntConcatingIterator( IEnumerator<PrimitiveIntIterator> iterators )
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

			  protected internal PrimitiveIntIterator CurrentIterator()
			  {
					return CurrentIteratorConflict;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static PrimitiveIntIterator filter(PrimitiveIntIterator source, final System.Func<int, boolean> filter)
		 public static PrimitiveIntIterator Filter( PrimitiveIntIterator source, System.Func<int, bool> filter )
		 {
			  return new PrimitiveIntFilteringIteratorAnonymousInnerClass( source, filter );
		 }

		 private class PrimitiveIntFilteringIteratorAnonymousInnerClass : PrimitiveIntFilteringIterator
		 {
			 private System.Func<int, bool> _filter;

			 public PrimitiveIntFilteringIteratorAnonymousInnerClass( Org.Neo4j.Collection.primitive.PrimitiveIntIterator source, System.Func<int, bool> filter ) : base( source )
			 {
				 this._filter = filter;
			 }

			 public override bool test( int item )
			 {
				  return _filter( item );
			 }
		 }

		 public static PrimitiveIntIterator Deduplicate( PrimitiveIntIterator source )
		 {
			  return new PrimitiveIntFilteringIteratorAnonymousInnerClass2( source );
		 }

		 private class PrimitiveIntFilteringIteratorAnonymousInnerClass2 : PrimitiveIntFilteringIterator
		 {
			 public PrimitiveIntFilteringIteratorAnonymousInnerClass2( Org.Neo4j.Collection.primitive.PrimitiveIntIterator source ) : base( source )
			 {
			 }

			 private readonly PrimitiveIntSet visited = Primitive.IntSet();

			 public override bool test( int testItem )
			 {
				  return visited.add( testItem );
			 }
		 }

		 public abstract class PrimitiveIntFilteringIterator : PrimitiveIntBaseIterator, System.Func<int, bool>
		 {
			  internal readonly PrimitiveIntIterator Source;

			  public PrimitiveIntFilteringIterator( PrimitiveIntIterator source )
			  {
					this.Source = source;
			  }

			  protected internal override bool FetchNext()
			  {
					while ( Source.hasNext() )
					{
						 int testItem = Source.next();
						 if ( Test( testItem ) )
						 {
							  return Next( testItem );
						 }
					}
					return false;
			  }

			  public override abstract bool Test( int testItem );
		 }

		 public static PrimitiveIntSet AsSet( PrimitiveIntIterator iterator )
		 {
			  PrimitiveIntSet set = Primitive.IntSet();
			  while ( iterator.HasNext() )
			  {
					int next = iterator.Next();
					if ( !set.Add( next ) )
					{
						 throw new System.InvalidOperationException( "Duplicate " + next + " from " + iterator );
					}
			  }
			  return set;
		 }

		 public static long[] AsLongArray( PrimitiveIntCollection values )
		 {
			  long[] array = new long[values.Size()];
			  PrimitiveIntIterator iterator = values.GetEnumerator();
			  int i = 0;
			  while ( iterator.HasNext() )
			  {
					array[i++] = iterator.Next();
			  }
			  return array;
		 }

		 private static readonly PrimitiveIntIterator EMPTY = new PrimitiveIntBaseIteratorAnonymousInnerClass();

		 private class PrimitiveIntBaseIteratorAnonymousInnerClass : PrimitiveIntBaseIterator
		 {
			 protected internal override bool fetchNext()
			 {
				  return false;
			 }
		 }

		 public static PrimitiveIntIterator EmptyIterator()
		 {
			  return EMPTY;
		 }

		 public static PrimitiveIntSet EmptySet()
		 {
			  return Empty.EMPTY_PRIMITIVE_INT_SET;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static PrimitiveIntIterator toPrimitiveIterator(final java.util.Iterator<int> iterator)
		 public static PrimitiveIntIterator ToPrimitiveIterator( IEnumerator<int> iterator )
		 {
			  return new PrimitiveIntBaseIteratorAnonymousInnerClass2( iterator );
		 }

		 private class PrimitiveIntBaseIteratorAnonymousInnerClass2 : PrimitiveIntBaseIterator
		 {
			 private IEnumerator<int> _iterator;

			 public PrimitiveIntBaseIteratorAnonymousInnerClass2( IEnumerator<int> iterator )
			 {
				 this._iterator = iterator;
			 }

			 protected internal override bool fetchNext()
			 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  if ( _iterator.hasNext() )
				  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						int? nextValue = _iterator.next();
						if ( null == nextValue )
						{
							 throw new System.ArgumentException( "Cannot convert null Integer to primitive int" );
						}
						return next( nextValue.Value );
				  }
				  return false;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static <T> java.util.Iterator<T> map(final System.Func<int, T> mapFunction, final PrimitiveIntIterator source)
		 public static IEnumerator<T> Map<T>( System.Func<int, T> mapFunction, PrimitiveIntIterator source )
		 {
			  return new IteratorAnonymousInnerClass( mapFunction, source );
		 }

		 private class IteratorAnonymousInnerClass : IEnumerator<T>
		 {
			 private System.Func<int, T> _mapFunction;
			 private Org.Neo4j.Collection.primitive.PrimitiveIntIterator _source;

			 public IteratorAnonymousInnerClass( System.Func<int, T> mapFunction, Org.Neo4j.Collection.primitive.PrimitiveIntIterator source )
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

		 public static void Consume( PrimitiveIntIterator source, System.Action<int> consumer )
		 {
			  while ( source.HasNext() )
			  {
					consumer( source.Next() );
			  }
		 }

		 public static PrimitiveIntSet AsSet( int[] values )
		 {
			  PrimitiveIntSet set = Primitive.IntSet( values.Length );
			  foreach ( int value in values )
			  {
					set.Add( value );
			  }
			  return set;
		 }

		 public static PrimitiveIntSet AsSet( long[] values, System.Func<long, int> converter )
		 {
			  PrimitiveIntSet set = Primitive.IntSet( values.Length );
			  foreach ( long value in values )
			  {
					set.Add( converter( value ) );
			  }
			  return set;
		 }

		 public static PrimitiveIntObjectMap<T> Copy<T>( PrimitiveIntObjectMap<T> original )
		 {
			  PrimitiveIntObjectMap<T> copy = Primitive.IntObjectMap( original.Size() );
			  original.VisitEntries((key, value) =>
			  {
				copy.Put( key, value );
				return false;
			  });
			  return copy;
		 }

		 public static bool Contains( int[] values, int candidate )
		 {
			  foreach ( int value in values )
			  {
					if ( value == candidate )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 /// <summary>
		 /// Pulls all items from the {@code iterator} and puts them into a <seealso cref="System.Collections.IList"/>, boxing each int.
		 /// </summary>
		 /// <param name="iterator"> <seealso cref="PrimitiveIntIterator"/> to pull values from. </param>
		 /// <returns> a <seealso cref="System.Collections.IList"/> containing all items. </returns>
		 public static IList<int> ToList( PrimitiveIntIterator iterator )
		 {
			  IList<int> @out = new List<int>();
			  while ( iterator.HasNext() )
			  {
					@out.Add( iterator.Next() );
			  }
			  return @out;
		 }

		 /// <summary>
		 /// Pulls all items from the {@code iterator} and puts them into a <seealso cref="System.Collections.Generic.ISet<object>"/>, boxing each int.
		 /// Any duplicate value will throw <seealso cref="System.InvalidOperationException"/>.
		 /// </summary>
		 /// <param name="iterator"> <seealso cref="PrimitiveIntIterator"/> to pull values from. </param>
		 /// <returns> a <seealso cref="System.Collections.Generic.ISet<object>"/> containing all items. </returns>
		 /// <exception cref="IllegalStateException"> for the first encountered duplicate. </exception>
		 public static ISet<int> ToSet( PrimitiveIntIterator iterator )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return MapToSet( iterator, int?::new );
		 }

		 public static ISet<T> MapToSet<T>( PrimitiveIntIterator iterator, System.Func<int, T> map )
		 {
			  ISet<T> set = new HashSet<T>();
			  while ( iterator.HasNext() )
			  {
					AddUnique( set, map( iterator.Next() ) );
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
		 /// Deduplicates values in the {@code values} array.
		 /// </summary>
		 /// <param name="values"> sorted array of int values. </param>
		 /// <returns> the provided array if no duplicates were found, otherwise a new shorter array w/o duplicates. </returns>
		 public static int[] Deduplicate( int[] values )
		 {
			  int unique = 0;
			  for ( int i = 0; i < values.Length; i++ )
			  {
					int value = values[i];
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
	}

}