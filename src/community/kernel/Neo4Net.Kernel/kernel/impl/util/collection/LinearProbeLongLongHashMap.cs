using System;
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
namespace Neo4Net.Kernel.impl.util.collection
{
	using MutableInt = org.apache.commons.lang3.mutable.MutableInt;
	using LazyLongIterable = org.eclipse.collections.api.LazyLongIterable;
	using LongIterable = org.eclipse.collections.api.LongIterable;
	using RichIterable = org.eclipse.collections.api.RichIterable;
	using MutableBag = org.eclipse.collections.api.bag.MutableBag;
	using MutableLongBag = org.eclipse.collections.api.bag.primitive.MutableLongBag;
	using LongFunction = org.eclipse.collections.api.block.function.primitive.LongFunction;
	using LongFunction0 = org.eclipse.collections.api.block.function.primitive.LongFunction0;
	using LongToLongFunction = org.eclipse.collections.api.block.function.primitive.LongToLongFunction;
	using LongToObjectFunction = org.eclipse.collections.api.block.function.primitive.LongToObjectFunction;
	using ObjectLongToObjectFunction = org.eclipse.collections.api.block.function.primitive.ObjectLongToObjectFunction;
	using LongLongPredicate = org.eclipse.collections.api.block.predicate.primitive.LongLongPredicate;
	using LongPredicate = org.eclipse.collections.api.block.predicate.primitive.LongPredicate;
	using Procedure = org.eclipse.collections.api.block.procedure.Procedure;
	using Procedure2 = org.eclipse.collections.api.block.procedure.Procedure2;
	using LongLongProcedure = org.eclipse.collections.api.block.procedure.primitive.LongLongProcedure;
	using LongProcedure = org.eclipse.collections.api.block.procedure.primitive.LongProcedure;
	using ObjectIntProcedure = org.eclipse.collections.api.block.procedure.primitive.ObjectIntProcedure;
	using MutableLongCollection = org.eclipse.collections.api.collection.primitive.MutableLongCollection;
	using MutableLongIterator = org.eclipse.collections.api.iterator.MutableLongIterator;
	using ImmutableLongLongMap = org.eclipse.collections.api.map.primitive.ImmutableLongLongMap;
	using LongLongMap = org.eclipse.collections.api.map.primitive.LongLongMap;
	using MutableLongLongMap = org.eclipse.collections.api.map.primitive.MutableLongLongMap;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongLongPair = org.eclipse.collections.api.tuple.primitive.LongLongPair;
	using SpreadFunctions = org.eclipse.collections.impl.SpreadFunctions;
	using AbstractLazyIterable = org.eclipse.collections.impl.lazy.AbstractLazyIterable;
	using SynchronizedLongLongMap = org.eclipse.collections.impl.map.mutable.primitive.SynchronizedLongLongMap;
	using UnmodifiableLongLongMap = org.eclipse.collections.impl.map.mutable.primitive.UnmodifiableLongLongMap;
	using AbstractLongIterable = org.eclipse.collections.impl.primitive.AbstractLongIterable;


	using Resource = Neo4Net.Graphdb.Resource;
	using VisibleForTesting = Neo4Net.Utils.VisibleForTesting;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Integer.bitCount;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.eclipse.collections.impl.tuple.primitive.PrimitiveTuples.pair;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.Preconditions.checkArgument;

	/// <summary>
	/// Off heap implementation of long-long hash map.
	/// <ul>
	/// <li>It is <b>not thread-safe</b>
	/// <li>It has to be closed to prevent native memory leakage
	/// <li>Iterators returned by this map are fail-fast
	/// </ul>
	/// </summary>
	internal class LinearProbeLongLongHashMap : AbstractLongIterable, MutableLongLongMap, Resource
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting static final int DEFAULT_CAPACITY = 32;
		 internal const int DEFAULT_CAPACITY = 32;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting static final double REMOVALS_FACTOR = 0.25;
		 internal const double REMOVALS_FACTOR = 0.25;
		 private const double LOAD_FACTOR = 0.75;

		 private const long EMPTY_KEY = 0;
		 private const long REMOVED_KEY = 1;
		 private const long EMPTY_VALUE = 0;
		 private static readonly long _entrySize = 2 * Long.BYTES;

		 private readonly MemoryAllocator _allocator;

		 private Memory _memory;
		 private int _capacity;
		 private long _modCount;
		 private int _resizeOccupancyThreshold;
		 private int _resizeRemovalsThreshold;
		 private int _removals;
		 private int _entriesInMemory;

		 private bool _hasZeroKey;
		 private bool _hasOneKey;
		 private long _zeroValue;
		 private long _oneValue;

		 internal LinearProbeLongLongHashMap( MemoryAllocator allocator )
		 {
			  this._allocator = requireNonNull( allocator );
			  AllocateMemory( DEFAULT_CAPACITY );
		 }

		 public override void Put( long key, long value )
		 {
			  ++_modCount;

			  if ( IsSentinelKey( key ) )
			  {
					PutForSentinelKey( key, value );
					return;
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int idx = indexOf(key);
			  int idx = IndexOf( key );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long keyAtIdx = getKeyAt(idx);
			  long keyAtIdx = GetKeyAt( idx );

			  if ( keyAtIdx == key )
			  {
					SetValueAt( idx, value );
					return;
			  }

			  if ( keyAtIdx == REMOVED_KEY )
			  {
					--_removals;
			  }

			  SetKeyAt( idx, key );
			  SetValueAt( idx, value );

			  ++_entriesInMemory;
			  if ( _entriesInMemory >= _resizeOccupancyThreshold )
			  {
					GrowAndRehash();
			  }
		 }

		 public override void PutAll( LongLongMap map )
		 {
			  ++_modCount;
			  map.forEachKeyValue( this.put );
		 }

		 /// <param name="key"> </param>
		 /// <returns> value associated with the key, or {@code zero} if the map doesn't conain this key </returns>
		 public override long Get( long key )
		 {
			  return GetIfAbsent( key, EMPTY_VALUE );
		 }

		 public override long GetIfAbsent( long key, long ifAbsent )
		 {
			  if ( IsSentinelKey( key ) )
			  {
					return GetForSentinelKey( key, ifAbsent );
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int idx = indexOf(key);
			  int idx = IndexOf( key );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long keyAtIdx = getKeyAt(idx);
			  long keyAtIdx = GetKeyAt( idx );

			  if ( keyAtIdx == key )
			  {
					return GetValueAt( idx );
			  }

			  return ifAbsent;
		 }

		 public override long GetIfAbsentPut( long key, long value )
		 {
			  return getIfAbsentPut( key, () => value );
		 }

		 public override long GetIfAbsentPut( long key, LongFunction0 supplier )
		 {
			  if ( IsSentinelKey( key ) )
			  {
					return GetIfAbsentPutForSentinelKey( key, supplier );
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int idx = indexOf(key);
			  int idx = IndexOf( key );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long keyAtIdx = getKeyAt(idx);
			  long keyAtIdx = GetKeyAt( idx );

			  if ( keyAtIdx == key )
			  {
					return GetValueAt( idx );
			  }

			  ++_modCount;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long value = supplier.value();
			  long value = supplier.value();

			  if ( keyAtIdx == REMOVED_KEY )
			  {
					--_removals;
			  }

			  SetKeyAt( idx, key );
			  SetValueAt( idx, value );

			  ++_entriesInMemory;
			  if ( _entriesInMemory >= _resizeOccupancyThreshold )
			  {
					GrowAndRehash();
			  }

			  return value;
		 }

		 public override long GetIfAbsentPutWithKey( long key, LongToLongFunction function )
		 {
			  return getIfAbsentPut( key, () => function.valueOf(key) );
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <P> long getIfAbsentPutWith(long key, org.eclipse.collections.api.block.function.primitive.LongFunction<? super P> function, P parameter)
		 public override long GetIfAbsentPutWith<P, T1>( long key, LongFunction<T1> function, P parameter )
		 {
			  return getIfAbsentPut( key, () => function.longValueOf(parameter) );
		 }

		 public override long GetOrThrow( long key )
		 {
			  return getIfAbsentPut(key, () =>
			  {
				throw new System.InvalidOperationException( "Key not found: " + key );
			  });
		 }

		 public override void RemoveKey( long key )
		 {
			  RemoveKeyIfAbsent( key, EMPTY_VALUE );
		 }

		 public override void Remove( long key )
		 {
			  RemoveKeyIfAbsent( key, EMPTY_VALUE );
		 }

		 public override long RemoveKeyIfAbsent( long key, long ifAbsent )
		 {
			  ++_modCount;

			  if ( IsSentinelKey( key ) )
			  {
					return RemoveForSentinelKey( key, ifAbsent );
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int idx = indexOf(key);
			  int idx = IndexOf( key );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long keyAtIdx = getKeyAt(idx);
			  long keyAtIdx = GetKeyAt( idx );

			  if ( keyAtIdx != key )
			  {
					return ifAbsent;
			  }

			  SetKeyAt( idx, REMOVED_KEY );
			  --_entriesInMemory;
			  ++_removals;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long oldValue = getValueAt(idx);
			  long oldValue = GetValueAt( idx );

			  if ( _removals >= _resizeRemovalsThreshold )
			  {
					RehashWithoutGrow();
			  }

			  return oldValue;
		 }

		 public override bool ContainsKey( long key )
		 {
			  if ( IsSentinelKey( key ) )
			  {
					return ( key == EMPTY_KEY && _hasZeroKey ) || ( key == REMOVED_KEY && _hasOneKey );
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int idx = indexOf(key);
			  int idx = IndexOf( key );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long keyAtIdx = getKeyAt(idx);
			  long keyAtIdx = GetKeyAt( idx );
			  return key == keyAtIdx;
		 }

		 public override bool ContainsValue( long value )
		 {
			  if ( _hasZeroKey && _zeroValue == value )
			  {
					return true;
			  }
			  if ( _hasOneKey && _oneValue == value )
			  {
					return true;
			  }

			  for ( int i = 0; i < _capacity; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long key = getKeyAt(i);
					long key = GetKeyAt( i );
					if ( !IsSentinelKey( key ) && GetValueAt( i ) == value )
					{
						 return true;
					}
			  }

			  return false;
		 }

		 public override long UpdateValue( long key, long initialValueIfAbsent, LongToLongFunction function )
		 {
			  ++_modCount;

			  if ( IsSentinelKey( key ) )
			  {
					return UpdateValueForSentinelKey( key, initialValueIfAbsent, function );
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int idx = indexOf(key);
			  int idx = IndexOf( key );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long keyAtIdx = getKeyAt(idx);
			  long keyAtIdx = GetKeyAt( idx );

			  if ( keyAtIdx == key )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long newValue = function.applyAsLong(getValueAt(idx));
					long newValue = function.applyAsLong( GetValueAt( idx ) );
					SetValueAt( idx, newValue );
					return newValue;
			  }

			  if ( keyAtIdx == REMOVED_KEY )
			  {
					--_removals;
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long value = function.applyAsLong(initialValueIfAbsent);
			  long value = function.applyAsLong( initialValueIfAbsent );

			  SetKeyAt( idx, key );
			  SetValueAt( idx, value );

			  ++_entriesInMemory;
			  if ( _entriesInMemory >= _resizeOccupancyThreshold )
			  {
					GrowAndRehash();
			  }

			  return value;
		 }

		 public override long AddToValue( long key, long toBeAdded )
		 {
			  return UpdateValue( key, 0, v => v + toBeAdded );
		 }

		 public override void ForEachKey( LongProcedure procedure )
		 {
			  if ( _hasZeroKey )
			  {
					procedure.value( 0 );
			  }
			  if ( _hasOneKey )
			  {
					procedure.value( 1 );
			  }

			  int left = _entriesInMemory;
			  for ( int i = 0; i < _capacity && left > 0; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long key = getKeyAt(i);
					long key = GetKeyAt( i );
					if ( !IsSentinelKey( key ) )
					{
						 procedure.value( key );
						 --left;
					}
			  }
		 }

		 public override void ForEachValue( LongProcedure procedure )
		 {
			  ForEachKeyValue( ( key, value ) => procedure.value( value ) );
		 }

		 public override void ForEachKeyValue( LongLongProcedure procedure )
		 {
			  if ( _hasZeroKey )
			  {
					procedure.value( 0, _zeroValue );
			  }
			  if ( _hasOneKey )
			  {
					procedure.value( 1, _oneValue );
			  }

			  int left = _entriesInMemory;
			  for ( int i = 0; i < _capacity && left > 0; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long key = getKeyAt(i);
					long key = GetKeyAt( i );
					if ( !IsSentinelKey( key ) )
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long value = getValueAt(i);
						 long value = GetValueAt( i );
						 procedure.value( key, value );
						 --left;
					}
			  }
		 }

		 public override MutableLongCollection Values()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override void Clear()
		 {
			  ++_modCount;
			  _hasZeroKey = false;
			  _hasOneKey = false;
			  _entriesInMemory = 0;
			  _removals = 0;
			  _memory.free();
			  AllocateMemory( DEFAULT_CAPACITY );
		 }

		 public override MutableLongLongMap FlipUniqueValues()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override MutableLongLongMap Select( LongLongPredicate predicate )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override MutableLongLongMap Reject( LongLongPredicate predicate )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override MutableLongLongMap WithKeyValue( long key, long value )
		 {
			  Put( key, value );
			  return this;
		 }

		 public override MutableLongLongMap WithoutKey( long key )
		 {
			  RemoveKey( key );
			  return this;
		 }

		 public override MutableLongLongMap WithoutAllKeys( LongIterable keys )
		 {
			  keys.each( this.removeKey );
			  return this;
		 }

		 public override MutableLongLongMap AsUnmodifiable()
		 {
			  return new UnmodifiableLongLongMap( this );
		 }

		 public override MutableLongLongMap AsSynchronized()
		 {
			  return new SynchronizedLongLongMap( this );
		 }

		 public override LazyLongIterable KeysView()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override RichIterable<LongLongPair> KeyValuesView()
		 {
			  return new KeyValuesView( this );
		 }

		 public override ImmutableLongLongMap ToImmutable()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override MutableLongSet KeySet()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override MutableLongIterator LongIterator()
		 {
			  return new KeysIterator( this );
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

		 public override bool Contains( long value )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override void ForEach( LongProcedure procedure )
		 {
			  Each( procedure );
		 }

		 public override void Each( LongProcedure procedure )
		 {
			  if ( _hasZeroKey )
			  {
					procedure.value( 0 );
			  }
			  if ( _hasOneKey )
			  {
					procedure.value( 1 );
			  }

			  int left = _entriesInMemory;
			  for ( int i = 0; i < _capacity && left > 0; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long key = getKeyAt(i);
					long key = GetKeyAt( i );
					if ( !IsSentinelKey( key ) )
					{
						 procedure.value( key );
						 --left;
					}
			  }
		 }

		 public override MutableLongBag Select( LongPredicate predicate )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override MutableLongBag Reject( LongPredicate predicate )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override MutableBag<V> Collect<V, T1>( LongToObjectFunction<T1> function ) where T1 : V
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override long DetectIfNone( LongPredicate predicate, long ifNone )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override int Count( LongPredicate predicate )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool AnySatisfy( LongPredicate predicate )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool AllSatisfy( LongPredicate predicate )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool NoneSatisfy( LongPredicate predicate )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <T> T injectInto(T injectedValue, org.eclipse.collections.api.block.function.primitive.ObjectLongToObjectFunction<? super T, ? extends T> function)
		 public override T InjectInto<T, T1>( T injectedValue, ObjectLongToObjectFunction<T1> function ) where T1 : T
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override long Sum()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override long Max()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override long Min()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override int Size()
		 {
			  return _entriesInMemory + ( _hasOneKey ? 1 : 0 ) + ( _hasZeroKey ? 1 : 0 );
		 }

		 public override void AppendString( Appendable appendable, string start, string separator, string end )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override void Close()
		 {
			  ++_modCount;
			  if ( _memory != null )
			  {
					_memory.free();
					_memory = null;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting void rehashWithoutGrow()
		 internal virtual void RehashWithoutGrow()
		 {
			  Rehash( _capacity );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting void growAndRehash()
		 internal virtual void GrowAndRehash()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int newCapacity = capacity * 2;
			  int newCapacity = _capacity * 2;
			  if ( newCapacity < _capacity )
			  {
					throw new Exception( "Map reached capacity limit" );
			  }
			  Rehash( newCapacity );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting int hashAndMask(long element)
		 internal virtual int HashAndMask( long element )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long h = org.eclipse.collections.impl.SpreadFunctions.longSpreadOne(element);
			  long h = SpreadFunctions.longSpreadOne( element );
			  return Long.GetHashCode( h ) & ( _capacity - 1 );
		 }

		 internal virtual int IndexOf( long element )
		 {
			  int idx = HashAndMask( element );
			  int firstRemovedIdx = -1;

			  for ( int i = 0; i < _capacity; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long keyAtIdx = getKeyAt(idx);
					long keyAtIdx = GetKeyAt( idx );

					if ( keyAtIdx == element )
					{
						 return idx;
					}

					if ( keyAtIdx == EMPTY_KEY )
					{
						 return firstRemovedIdx == -1 ? idx : firstRemovedIdx;
					}

					if ( keyAtIdx == REMOVED_KEY && firstRemovedIdx == -1 )
					{
						 firstRemovedIdx = idx;
					}

					idx = ( idx + 1 ) & ( _capacity - 1 );
			  }

			  throw new AssertionError( "Failed to determine index for " + element );
		 }

		 private long UpdateValueForSentinelKey( long key, long initialValueIfAbsent, LongToLongFunction function )
		 {
			  if ( key == EMPTY_KEY )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long newValue = function.applyAsLong(hasZeroKey ? zeroValue : initialValueIfAbsent);
					long newValue = function.applyAsLong( _hasZeroKey ? _zeroValue : initialValueIfAbsent );
					_hasZeroKey = true;
					_zeroValue = newValue;
					return newValue;
			  }
			  if ( key == REMOVED_KEY )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long newValue = function.applyAsLong(hasOneKey ? oneValue : initialValueIfAbsent);
					long newValue = function.applyAsLong( _hasOneKey ? _oneValue : initialValueIfAbsent );
					_hasOneKey = true;
					_oneValue = newValue;
					return newValue;
			  }
			  throw new AssertionError( "Invalid sentinel key: " + key );
		 }

		 private void Rehash( int newCapacity )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int prevCapacity = capacity;
			  int prevCapacity = _capacity;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Memory prevMemory = memory;
			  Memory prevMemory = _memory;
			  _entriesInMemory = 0;
			  _removals = 0;
			  AllocateMemory( newCapacity );

			  for ( int i = 0; i < prevCapacity; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long key = prevMemory.readLong(i * ENTRY_SIZE);
					long key = prevMemory.ReadLong( i * _entrySize );
					if ( !IsSentinelKey( key ) )
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long value = prevMemory.readLong((i * ENTRY_SIZE) + ENTRY_SIZE / 2);
						 long value = prevMemory.ReadLong( ( i * _entrySize ) + _entrySize / 2 );
						 Put( key, value );
					}
			  }

			  prevMemory.Free();
		 }

		 private static bool IsSentinelKey( long key )
		 {
			  return key == EMPTY_KEY || key == REMOVED_KEY;
		 }

		 private void AllocateMemory( int newCapacity )
		 {
			  checkArgument( newCapacity > 1 && bitCount( newCapacity ) == 1, "Capacity must be power of 2" );
			  _capacity = newCapacity;
			  _resizeOccupancyThreshold = ( int )( newCapacity * LOAD_FACTOR );
			  _resizeRemovalsThreshold = ( int )( newCapacity * REMOVALS_FACTOR );
			  _memory = _allocator.allocate( newCapacity * _entrySize, true );
		 }

		 private long RemoveForSentinelKey( long key, long ifAbsent )
		 {
			  if ( key == EMPTY_KEY )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long result = hasZeroKey ? zeroValue : ifAbsent;
					long result = _hasZeroKey ? _zeroValue : ifAbsent;
					_hasZeroKey = false;
					return result;
			  }
			  if ( key == REMOVED_KEY )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long result = hasOneKey ? oneValue : ifAbsent;
					long result = _hasOneKey ? _oneValue : ifAbsent;
					_hasOneKey = false;
					return result;
			  }
			  throw new AssertionError( "Invalid sentinel key: " + key );
		 }

		 private long GetForSentinelKey( long key, long ifAbsent )
		 {
			  if ( key == EMPTY_KEY )
			  {
					return _hasZeroKey ? _zeroValue : ifAbsent;
			  }
			  if ( key == REMOVED_KEY )
			  {
					return _hasOneKey ? _oneValue : ifAbsent;
			  }
			  throw new AssertionError( "Invalid sentinel key: " + key );
		 }

		 private long GetIfAbsentPutForSentinelKey( long key, LongFunction0 supplier )
		 {
			  if ( key == EMPTY_KEY )
			  {
					if ( !_hasZeroKey )
					{
						 ++_modCount;
						 _hasZeroKey = true;
						 _zeroValue = supplier.value();
					}
					return _zeroValue;
			  }
			  if ( key == REMOVED_KEY )
			  {
					if ( !_hasOneKey )
					{
						 ++_modCount;
						 _hasOneKey = true;
						 _oneValue = supplier.value();
					}
					return _oneValue;
			  }
			  throw new AssertionError( "Invalid sentinel key: " + key );
		 }

		 private void SetKeyAt( int idx, long key )
		 {
			  _memory.writeLong( idx * _entrySize, key );
		 }

		 private long GetKeyAt( int idx )
		 {
			  return _memory.readLong( idx * _entrySize );
		 }

		 private void SetValueAt( int idx, long value )
		 {
			  _memory.writeLong( ( idx * _entrySize ) + _entrySize / 2, value );
		 }

		 private long GetValueAt( int idx )
		 {
			  return _memory.readLong( ( idx * _entrySize ) + _entrySize / 2 );
		 }

		 private void PutForSentinelKey( long key, long value )
		 {
			  if ( key == EMPTY_KEY )
			  {
					_hasZeroKey = true;
					_zeroValue = value;
			  }
			  else if ( key == REMOVED_KEY )
			  {
					_hasOneKey = true;
					_oneValue = value;
			  }
			  else
			  {
					throw new AssertionError( "Invalid sentinel key: " + key );
			  }
		 }

		 private class KeyValuesView : AbstractLazyIterable<LongLongPair>
		 {
			 private readonly LinearProbeLongLongHashMap _outerInstance;

			 public KeyValuesView( LinearProbeLongLongHashMap outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public void each(org.eclipse.collections.api.block.procedure.Procedure<? super org.eclipse.collections.api.tuple.primitive.LongLongPair> procedure)
			  public override void Each<T1>( Procedure<T1> procedure )
			  {
					throw new System.NotSupportedException();
			  }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public void forEachWithIndex(org.eclipse.collections.api.block.procedure.primitive.ObjectIntProcedure<? super org.eclipse.collections.api.tuple.primitive.LongLongPair> objectIntProcedure)
			  public override void ForEachWithIndex<T1>( ObjectIntProcedure<T1> objectIntProcedure )
			  {
					throw new System.NotSupportedException();
			  }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <P> void forEachWith(org.eclipse.collections.api.block.procedure.Procedure2<? super org.eclipse.collections.api.tuple.primitive.LongLongPair, ? super P> procedure, P parameter)
			  public override void ForEachWith<P, T1>( Procedure2<T1> procedure, P parameter )
			  {
					throw new System.NotSupportedException();
			  }

			  public override IEnumerator<LongLongPair> Iterator()
			  {
					return new KeyValuesIterator( _outerInstance );
			  }
		 }

		 private class KeyValuesIterator : IEnumerator<LongLongPair>
		 {
			 internal bool InstanceFieldsInitialized = false;

			 internal virtual void InitializeInstanceFields()
			 {
				 ModCount = _outerInstance.modCount;
			 }

			 private readonly LinearProbeLongLongHashMap _outerInstance;

			 public KeyValuesIterator( LinearProbeLongLongHashMap outerInstance )
			 {
				 this._outerInstance = outerInstance;

				 if ( !InstanceFieldsInitialized )
				 {
					 InitializeInstanceFields();
					 InstanceFieldsInitialized = true;
				 }
			 }

			  internal long ModCount;
			  internal int Visited;
			  internal int Idx;

			  internal bool HandledZero;
			  internal bool HandledOne;

			  public override LongLongPair Next()
			  {
					if ( !HasNext() )
					{
						 throw new NoSuchElementException( "iterator is exhausted" );
					}

					++Visited;

					if ( !HandledZero )
					{
						 HandledZero = true;
						 if ( outerInstance.hasZeroKey )
						 {
							  return pair( 0L, outerInstance.zeroValue );
						 }
					}

					if ( !HandledOne )
					{
						 HandledOne = true;
						 if ( outerInstance.hasOneKey )
						 {
							  return pair( 1L, outerInstance.oneValue );
						 }
					}

					long key = outerInstance.getKeyAt( Idx );
					while ( IsSentinelKey( key ) )
					{
						 ++Idx;
						 key = outerInstance.getKeyAt( Idx );
					}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long value = getValueAt(idx);
					long value = outerInstance.getValueAt( Idx );
					++Idx;
					return pair( key, value );
			  }

			  public override void Remove()
			  {
					throw new System.NotSupportedException();
			  }

			  public override bool HasNext()
			  {
					outerInstance.validateIteratorState( ModCount );
					return Visited != outerInstance.Size();
			  }
		 }

		 private class KeysIterator : MutableLongIterator
		 {
			 internal bool InstanceFieldsInitialized = false;

			 internal virtual void InitializeInstanceFields()
			 {
				 ModCount = _outerInstance.modCount;
			 }

			 private readonly LinearProbeLongLongHashMap _outerInstance;

			 public KeysIterator( LinearProbeLongLongHashMap outerInstance )
			 {
				 this._outerInstance = outerInstance;

				 if ( !InstanceFieldsInitialized )
				 {
					 InitializeInstanceFields();
					 InstanceFieldsInitialized = true;
				 }
			 }

			  internal long ModCount;
			  internal int Visited;
			  internal int Idx;

			  internal bool HandledZero;
			  internal bool HandledOne;

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
						 if ( outerInstance.hasZeroKey )
						 {
							  return 0L;
						 }
					}

					if ( !HandledOne )
					{
						 HandledOne = true;
						 if ( outerInstance.hasOneKey )
						 {
							  return 1L;
						 }
					}

					long key = outerInstance.getKeyAt( Idx );
					while ( IsSentinelKey( key ) )
					{
						 ++Idx;
						 key = outerInstance.getKeyAt( Idx );
					}

					++Idx;
					return key;
			  }

			  public override void Remove()
			  {
					throw new System.NotSupportedException();
			  }

			  public override bool HasNext()
			  {
					outerInstance.validateIteratorState( ModCount );
					return Visited < outerInstance.Size();
			  }
		 }

		 private void ValidateIteratorState( long iteratorModCount )
		 {
			  if ( iteratorModCount != _modCount )
			  {
					throw new ConcurrentModificationException();
			  }
		 }
	}

}