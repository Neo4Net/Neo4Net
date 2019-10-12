using System;

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
namespace Neo4Net.Kernel.impl.util.collection
{
	using MutableInt = org.apache.commons.lang3.mutable.MutableInt;
	using LongToObjectFunction = org.eclipse.collections.api.block.function.primitive.LongToObjectFunction;
	using ObjectLongToObjectFunction = org.eclipse.collections.api.block.function.primitive.ObjectLongToObjectFunction;
	using LongPredicate = org.eclipse.collections.api.block.predicate.primitive.LongPredicate;
	using LongProcedure = org.eclipse.collections.api.block.procedure.primitive.LongProcedure;
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using MutableLongIterator = org.eclipse.collections.api.iterator.MutableLongIterator;
	using MutableSet = org.eclipse.collections.api.set.MutableSet;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using SpreadFunctions = org.eclipse.collections.impl.SpreadFunctions;
	using AbstractLongIterable = org.eclipse.collections.impl.primitive.AbstractLongIterable;
	using UnifiedSet = org.eclipse.collections.impl.set.mutable.UnifiedSet;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;


	using VisibleForTesting = Neo4Net.Util.VisibleForTesting;

	internal abstract class AbstractLinearProbeLongHashSet : AbstractLongIterable, LongSet
	{
		 private const long EMPTY = 0;
		 internal const long REMOVED = 1;

		 internal Memory Memory;
		 internal int Capacity;
		 internal int ElementsInMemory;
		 internal long ModCount;
		 internal bool HasZero;
		 internal bool HasOne;

		 internal AbstractLinearProbeLongHashSet()
		 {
			  // nop
		 }

		 internal AbstractLinearProbeLongHashSet( AbstractLinearProbeLongHashSet src )
		 {
			  this.Memory = src.Memory;
			  this.Capacity = src.Capacity;
			  this.HasZero = src.HasZero;
			  this.HasOne = src.HasOne;
			  this.ElementsInMemory = src.ElementsInMemory;
		 }

		 public override LongIterator LongIterator()
		 {
			  return new FailFastIterator( this );
		 }

		 public override long[] ToArray()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.commons.lang3.mutable.MutableInt idx = new org.apache.commons.lang3.mutable.MutableInt();
			  MutableInt idx = new MutableInt();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long[] array = new long[size()];
			  long[] array = new long[Size()];
			  Each( element => array[idx.AndIncrement] = element );
			  return array;
		 }

		 public override bool Contains( long element )
		 {
			  if ( element == 0 )
			  {
					return HasZero;
			  }
			  if ( element == 1 )
			  {
					return HasOne;
			  }

			  int idx = IndexOf( element );
			  return ValueAt( idx ) == element;
		 }

		 public override void ForEach( LongProcedure procedure )
		 {
			  Each( procedure );
		 }

		 public override void Each( LongProcedure procedure )
		 {
			  if ( HasZero )
			  {
					procedure.accept( 0 );
			  }
			  if ( HasOne )
			  {
					procedure.accept( 1 );
			  }

			  int visited = 0;
			  for ( int i = 0; i < Capacity && visited < ElementsInMemory; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long value = valueAt(i);
					long value = ValueAt( i );
					if ( IsRealValue( value ) )
					{
						 procedure.accept( value );
						 ++visited;
					}
			  }
		 }

		 public override bool AnySatisfy( LongPredicate predicate )
		 {
			  if ( ( HasZero && predicate.test( 0 ) ) || ( HasOne && predicate.test( 1 ) ) )
			  {
					return true;
			  }

			  int visited = 0;
			  for ( int i = 0; i < Capacity && visited < ElementsInMemory; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long value = valueAt(i);
					long value = ValueAt( i );
					if ( IsRealValue( value ) )
					{
						 if ( predicate.test( value ) )
						 {
							  return true;
						 }
						 ++visited;
					}
			  }
			  return false;
		 }

		 public override bool AllSatisfy( LongPredicate predicate )
		 {
			  if ( ( HasZero && !predicate.test( 0 ) ) || ( HasOne && !predicate.test( 1 ) ) )
			  {
					return false;
			  }

			  int visited = 0;
			  for ( int i = 0; i < Capacity && visited < ElementsInMemory; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long value = valueAt(i);
					long value = ValueAt( i );
					if ( IsRealValue( value ) )
					{
						 if ( !predicate.test( value ) )
						 {
							  return false;
						 }
						 ++visited;
					}
			  }
			  return true;
		 }

		 public override bool NoneSatisfy( LongPredicate predicate )
		 {
			  return !AnySatisfy( predicate );
		 }

		 public override long DetectIfNone( LongPredicate predicate, long ifNone )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <T> T injectInto(T injectedValue, org.eclipse.collections.api.block.function.primitive.ObjectLongToObjectFunction<? super T, ? extends T> function)
		 public override T InjectInto<T, T1>( T injectedValue, ObjectLongToObjectFunction<T1> function ) where T1 : T
		 {
			  throw new System.NotSupportedException();
		 }

		 public override int Count( LongPredicate predicate )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override long Sum()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override long Max()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override long Min()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override MutableLongSet Select( LongPredicate predicate )
		 {
			  return select( predicate, new LongHashSet() );
		 }

		 public override MutableLongSet Reject( LongPredicate predicate )
		 {
			  return reject( predicate, new LongHashSet() );
		 }

		 public override MutableSet<V> Collect<V, T1>( LongToObjectFunction<T1> function ) where T1 : V
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.MutableSet<V> result = new org.eclipse.collections.impl.set.mutable.UnifiedSet<>(size());
			  MutableSet<V> result = new UnifiedSet<V>( Size() );
			  Each(element =>
			  {
				result.add( function.apply( element ) );
			  });
			  return result;
		 }

		 public override int GetHashCode()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.apache.commons.lang3.mutable.MutableInt h = new org.apache.commons.lang3.mutable.MutableInt();
			  MutableInt h = new MutableInt();
			  Each( element => h.add( ( int )( element ^ ( int )( ( uint )element >> 32 ) ) ) );
			  return h.intValue();
		 }

		 public override bool Equals( object obj )
		 {
			  if ( this == obj )
			  {
					return true;
			  }
			  if ( !( obj is LongSet ) )
			  {
					return false;
			  }
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.primitive.LongSet other = (org.eclipse.collections.api.set.primitive.LongSet) obj;
			  LongSet other = ( LongSet ) obj;
			  return Size() == other.size() && containsAll(other);
		 }

		 public override int Size()
		 {
			  return ElementsInMemory + ( HasZero ? 1 : 0 ) + ( HasOne ? 1 : 0 );
		 }

		 public override void AppendString( Appendable appendable, string start, string separator, string end )
		 {
			  try
			  {
					appendable.append( start );
					appendable.append( "offheap,size=" ).append( Size().ToString() ).append("; ");

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.iterator.LongIterator iterator = longIterator();
					LongIterator iterator = LongIterator();
					for ( int i = 0; i < 100 && iterator.hasNext(); i++ )
					{
						 appendable.append( Convert.ToString( iterator.next() ) );
						 if ( iterator.hasNext() )
						 {
							  appendable.append( ", " );
						 }
					}

					if ( iterator.hasNext() )
					{
						 appendable.append( "..." );
					}

					appendable.append( end );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 internal static bool IsRealValue( long value )
		 {
			  return value != REMOVED && value != EMPTY;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting int hashAndMask(long element)
		 internal virtual int HashAndMask( long element )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long h = org.eclipse.collections.impl.SpreadFunctions.longSpreadOne(element);
			  long h = SpreadFunctions.longSpreadOne( element );
			  return Long.GetHashCode( h ) & ( Capacity - 1 );
		 }

		 internal virtual long ValueAt( int idx )
		 {
			  return Memory.readLong( ( long ) idx * Long.BYTES );
		 }

		 internal virtual int IndexOf( long element )
		 {
			  int idx = HashAndMask( element );
			  int firstRemovedIdx = -1;

			  for ( int i = 0; i < Capacity; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long valueAtIdx = valueAt(idx);
					long valueAtIdx = ValueAt( idx );

					if ( valueAtIdx == element )
					{
						 return idx;
					}

					if ( valueAtIdx == EMPTY )
					{
						 return firstRemovedIdx == -1 ? idx : firstRemovedIdx;
					}

					if ( valueAtIdx == REMOVED && firstRemovedIdx == -1 )
					{
						 firstRemovedIdx = idx;
					}

					idx = ( idx + 1 ) & ( Capacity - 1 );
			  }

			  throw new AssertionError( "Failed to determine index for " + element );
		 }

		 internal class FailFastIterator : MutableLongIterator
		 {
			 private readonly AbstractLinearProbeLongHashSet _outerInstance;

			  internal readonly long ModCount;
			  internal int Visited;
			  internal int Idx;
			  internal bool HandledZero;
			  internal bool HandledOne;

			  internal FailFastIterator( AbstractLinearProbeLongHashSet outerInstance )
			  {
				  this._outerInstance = outerInstance;
					this.ModCount = outerInstance.ModCount;
			  }

			  public override void Remove()
			  {
					throw new System.NotSupportedException();
			  }

			  public override long Next()
			  {
					if ( !HasNext() )
					{
						 throw new NoSuchElementException( "iterator is exhausted" );
					}

					++Visited;

					if ( !HandledZero )
					{
						 HandledZero = true;
						 if ( outerInstance.HasZero )
						 {
							  return 0;
						 }
					}
					if ( !HandledOne )
					{
						 HandledOne = true;
						 if ( outerInstance.HasOne )
						 {
							  return 1;
						 }
					}

					long value;
					do
					{
						 value = outerInstance.ValueAt( Idx++ );
					} while ( !IsRealValue( value ) );

					return value;
			  }

			  public override bool HasNext()
			  {
					CheckState();
					return Visited < outerInstance.Size();
			  }

			  internal virtual void CheckState()
			  {
					if ( ModCount != _outerInstance.modCount )
					{
						 throw new ConcurrentModificationException();
					}
			  }
		 }
	}

}