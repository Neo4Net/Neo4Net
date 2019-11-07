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
namespace Neo4Net.Kernel.Impl.Api.state
{
	using LazyIterable = org.eclipse.collections.api.LazyIterable;
	using LazyLongIterable = org.eclipse.collections.api.LazyLongIterable;
	using LongIterable = org.eclipse.collections.api.LongIterable;
	using RichIterable = org.eclipse.collections.api.RichIterable;
	using MutableBag = org.eclipse.collections.api.bag.MutableBag;
	using MutableBooleanBag = org.eclipse.collections.api.bag.primitive.MutableBooleanBag;
	using MutableByteBag = org.eclipse.collections.api.bag.primitive.MutableByteBag;
	using MutableCharBag = org.eclipse.collections.api.bag.primitive.MutableCharBag;
	using MutableDoubleBag = org.eclipse.collections.api.bag.primitive.MutableDoubleBag;
	using MutableFloatBag = org.eclipse.collections.api.bag.primitive.MutableFloatBag;
	using MutableIntBag = org.eclipse.collections.api.bag.primitive.MutableIntBag;
	using MutableLongBag = org.eclipse.collections.api.bag.primitive.MutableLongBag;
	using MutableShortBag = org.eclipse.collections.api.bag.primitive.MutableShortBag;
	using MutableSortedBag = org.eclipse.collections.api.bag.sorted.MutableSortedBag;
	using Function = org.eclipse.collections.api.block.function.Function;
	using Function0 = org.eclipse.collections.api.block.function.Function0;
	using Function2 = org.eclipse.collections.api.block.function.Function2;
	using BooleanFunction = org.eclipse.collections.api.block.function.primitive.BooleanFunction;
	using ByteFunction = org.eclipse.collections.api.block.function.primitive.ByteFunction;
	using CharFunction = org.eclipse.collections.api.block.function.primitive.CharFunction;
	using DoubleFunction = org.eclipse.collections.api.block.function.primitive.DoubleFunction;
	using DoubleObjectToDoubleFunction = org.eclipse.collections.api.block.function.primitive.DoubleObjectToDoubleFunction;
	using FloatFunction = org.eclipse.collections.api.block.function.primitive.FloatFunction;
	using FloatObjectToFloatFunction = org.eclipse.collections.api.block.function.primitive.FloatObjectToFloatFunction;
	using IntFunction = org.eclipse.collections.api.block.function.primitive.IntFunction;
	using IntObjectToIntFunction = org.eclipse.collections.api.block.function.primitive.IntObjectToIntFunction;
	using LongFunction = org.eclipse.collections.api.block.function.primitive.LongFunction;
	using LongObjectToLongFunction = org.eclipse.collections.api.block.function.primitive.LongObjectToLongFunction;
	using LongToObjectFunction = org.eclipse.collections.api.block.function.primitive.LongToObjectFunction;
	using ShortFunction = org.eclipse.collections.api.block.function.primitive.ShortFunction;
	using Predicate = org.eclipse.collections.api.block.predicate.Predicate;
	using Predicate2 = org.eclipse.collections.api.block.predicate.Predicate2;
	using LongObjectPredicate = org.eclipse.collections.api.block.predicate.primitive.LongObjectPredicate;
	using Procedure = org.eclipse.collections.api.block.procedure.Procedure;
	using Procedure2 = org.eclipse.collections.api.block.procedure.Procedure2;
	using LongObjectProcedure = org.eclipse.collections.api.block.procedure.primitive.LongObjectProcedure;
	using LongProcedure = org.eclipse.collections.api.block.procedure.primitive.LongProcedure;
	using ObjectIntProcedure = org.eclipse.collections.api.block.procedure.primitive.ObjectIntProcedure;
	using MutableBooleanCollection = org.eclipse.collections.api.collection.primitive.MutableBooleanCollection;
	using MutableByteCollection = org.eclipse.collections.api.collection.primitive.MutableByteCollection;
	using MutableCharCollection = org.eclipse.collections.api.collection.primitive.MutableCharCollection;
	using MutableDoubleCollection = org.eclipse.collections.api.collection.primitive.MutableDoubleCollection;
	using MutableFloatCollection = org.eclipse.collections.api.collection.primitive.MutableFloatCollection;
	using MutableIntCollection = org.eclipse.collections.api.collection.primitive.MutableIntCollection;
	using MutableLongCollection = org.eclipse.collections.api.collection.primitive.MutableLongCollection;
	using MutableShortCollection = org.eclipse.collections.api.collection.primitive.MutableShortCollection;
	using MutableList = org.eclipse.collections.api.list.MutableList;
	using MutableMap = org.eclipse.collections.api.map.MutableMap;
	using ImmutableLongObjectMap = org.eclipse.collections.api.map.primitive.ImmutableLongObjectMap;
	using LongObjectMap = org.eclipse.collections.api.map.primitive.LongObjectMap;
	using MutableLongLongMap = org.eclipse.collections.api.map.primitive.MutableLongLongMap;
	using MutableLongObjectMap = org.eclipse.collections.api.map.primitive.MutableLongObjectMap;
	using MutableObjectDoubleMap = org.eclipse.collections.api.map.primitive.MutableObjectDoubleMap;
	using MutableObjectLongMap = org.eclipse.collections.api.map.primitive.MutableObjectLongMap;
	using MutableSortedMap = org.eclipse.collections.api.map.sorted.MutableSortedMap;
	using MutableMultimap = org.eclipse.collections.api.multimap.MutableMultimap;
	using MutableBagMultimap = org.eclipse.collections.api.multimap.bag.MutableBagMultimap;
	using PartitionMutableBag = org.eclipse.collections.api.partition.bag.PartitionMutableBag;
	using MutableSet = org.eclipse.collections.api.set.MutableSet;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using MutableSortedSet = org.eclipse.collections.api.set.sorted.MutableSortedSet;
	using Pair = org.eclipse.collections.api.tuple.Pair;
	using LongLongPair = org.eclipse.collections.api.tuple.primitive.LongLongPair;
	using LongObjectPair = org.eclipse.collections.api.tuple.primitive.LongObjectPair;
	using AbstractLazyIterable = org.eclipse.collections.impl.lazy.AbstractLazyIterable;
	using LazyIterableAdapter = org.eclipse.collections.impl.lazy.LazyIterableAdapter;
	using SynchronizedLongObjectMap = org.eclipse.collections.impl.map.mutable.primitive.SynchronizedLongObjectMap;
	using UnmodifiableLongObjectMap = org.eclipse.collections.impl.map.mutable.primitive.UnmodifiableLongObjectMap;
	using PrimitiveTuples = org.eclipse.collections.impl.tuple.primitive.PrimitiveTuples;


	using Value = Neo4Net.Values.Storable.Value;


	public class ValuesMap : MutableLongObjectMap<Value>
	{
		 private const long NONE = -1L;
		 private readonly MutableLongLongMap _refs;
		 private readonly ValuesContainer _valuesContainer;

		 public ValuesMap( MutableLongLongMap refs, ValuesContainer valuesContainer )
		 {
			  this._valuesContainer = valuesContainer;
			  this._refs = refs;
		 }

		 public override int Size()
		 {
			  return _refs.size();
		 }

		 public override bool Empty
		 {
			 get
			 {
				  return _refs.Empty;
			 }
		 }

		 public override Value First
		 {
			 get
			 {
				  throw new System.NotSupportedException();
			 }
		 }

		 public override Value Last
		 {
			 get
			 {
				  throw new System.NotSupportedException();
			 }
		 }

		 public override bool Contains( object @object )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override bool ContainsAllIterable<T1>( IEnumerable<T1> source )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override bool ContainsAll<T1>( ICollection<T1> source )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override bool ContainsAllArguments( params object[] elements )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override RichIterable<LongObjectPair<Value>> KeyValuesView()
		 {
			  return new KeyValuesView( this );
		 }

		 public override Value Put( long key, Value value )
		 {
			  requireNonNull( value, "Cannot put null values" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.values.storable.Value prev = get(key);
			  Value prev = Get( key );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long ref = valuesContainer.add(value);
			  long @ref = _valuesContainer.add( value );
			  _refs.put( key, @ref );
			  return prev;
		 }

		 public override void PutAll<T1>( LongObjectMap<T1> map ) where T1 : Neo4Net.Values.Storable.Value
		 {
			  map.forEachKeyValue( this.put );
		 }

		 public override Value Get( long key )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long ref = refs.getIfAbsent(key, NONE);
			  long @ref = _refs.getIfAbsent( key, NONE );
			  return @ref == NONE ? null : _valuesContainer.get( @ref );
		 }

		 public override Value GetIfAbsentPut( long key, Value value )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.values.storable.Value existing = get(key);
			  Value existing = Get( key );
			  if ( existing != null )
			  {
					return existing;
			  }
			  Put( key, value );
			  return value;
		 }

		 public override Value GetIfAbsentPut<T1>( long key, Function0<T1> supplier ) where T1 : Neo4Net.Values.Storable.Value
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.values.storable.Value existing = get(key);
			  Value existing = Get( key );
			  if ( existing != null )
			  {
					return existing;
			  }
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.values.storable.Value value = supplier.value();
			  Value value = supplier.value();
			  Put( key, value );
			  return value;
		 }

		 public override Value GetIfAbsentPutWithKey<T1>( long key, LongToObjectFunction<T1> function ) where T1 : Neo4Net.Values.Storable.Value
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.values.storable.Value existing = get(key);
			  Value existing = Get( key );
			  if ( existing != null )
			  {
					return existing;
			  }
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.values.storable.Value value = function.ValueOf(key);
			  Value value = function.ValueOf( key );
			  Put( key, value );
			  return value;
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <P> Neo4Net.values.storable.Value getIfAbsentPutWith(long key, org.eclipse.collections.api.block.function.Function<? super P, ? extends Neo4Net.values.storable.Value> function, P parameter)
		 public override Value GetIfAbsentPutWith<P, T1>( long key, Function<T1> function, P parameter ) where T1 : Neo4Net.Values.Storable.Value
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.values.storable.Value existing = get(key);
			  Value existing = Get( key );
			  if ( existing != null )
			  {
					return existing;
			  }
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.values.storable.Value value = function.ValueOf(parameter);
			  Value value = function.ValueOf( parameter );
			  Put( key, value );
			  return value;
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public Neo4Net.values.storable.Value updateValue(long key, org.eclipse.collections.api.block.function.Function0<? extends Neo4Net.values.storable.Value> factory, org.eclipse.collections.api.block.function.Function<? super Neo4Net.values.storable.Value, ? extends Neo4Net.values.storable.Value> function)
		 public override Value UpdateValue<T1, T2>( long key, Function0<T1> factory, Function<T2> function ) where T1 : Neo4Net.Values.Storable.Value where T2 : Neo4Net.Values.Storable.Value
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <P> Neo4Net.values.storable.Value updateValueWith(long key, org.eclipse.collections.api.block.function.Function0<? extends Neo4Net.values.storable.Value> factory, org.eclipse.collections.api.block.function.Function2<? super Neo4Net.values.storable.Value, ? super P, ? extends Neo4Net.values.storable.Value> function, P parameter)
		 public override Value UpdateValueWith<P, T1, T2>( long key, Function0<T1> factory, Function2<T2> function, P parameter ) where T1 : Neo4Net.Values.Storable.Value where T2 : Neo4Net.Values.Storable.Value
		 {
			  throw new System.NotSupportedException();
		 }

		 public override MutableObjectLongMap<Value> FlipUniqueValues()
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public org.eclipse.collections.api.map.primitive.MutableLongObjectMap<Neo4Net.values.storable.Value> tap(org.eclipse.collections.api.block.procedure.Procedure<? super Neo4Net.values.storable.Value> procedure)
		 public override MutableLongObjectMap<Value> Tap<T1>( Procedure<T1> procedure )
		 {
			  ForEachValue( procedure );
			  return this;
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public void each(org.eclipse.collections.api.block.procedure.Procedure<? super Neo4Net.values.storable.Value> procedure)
		 public override void Each<T1>( Procedure<T1> procedure )
		 {
			  _refs.forEachKey(@ref =>
			  {
				Value value = _valuesContainer.get( @ref );
				procedure.value( value );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public org.eclipse.collections.api.map.primitive.MutableLongObjectMap<Neo4Net.values.storable.Value> select(org.eclipse.collections.api.block.predicate.primitive.LongObjectPredicate<? super Neo4Net.values.storable.Value> predicate)
		 public override MutableLongObjectMap<Value> Select<T1>( LongObjectPredicate<T1> predicate )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public org.eclipse.collections.api.map.primitive.MutableLongObjectMap<Neo4Net.values.storable.Value> reject(org.eclipse.collections.api.block.predicate.primitive.LongObjectPredicate<? super Neo4Net.values.storable.Value> predicate)
		 public override MutableLongObjectMap<Value> Reject<T1>( LongObjectPredicate<T1> predicate )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override ImmutableLongObjectMap<Value> ToImmutable()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override MutableLongSet KeySet()
		 {
			  return _refs.Keys.asUnmodifiable();
		 }

		 public override LazyLongIterable KeysView()
		 {
			  return _refs.keysView();
		 }

		 public override ValuesMap WithKeyValue( long key, Value value )
		 {
			  Put( key, value );
			  return this;
		 }

		 public override ValuesMap WithoutKey( long key )
		 {
			  RemoveKey( key );
			  return this;
		 }

		 public override ValuesMap WithoutAllKeys( LongIterable keys )
		 {
			  keys.forEach( this.removeKey );
			  return this;
		 }

		 public override MutableLongObjectMap<Value> AsUnmodifiable()
		 {
			  return new UnmodifiableLongObjectMap<Value>( this );
		 }

		 public override MutableLongObjectMap<Value> AsSynchronized()
		 {
			  return new SynchronizedLongObjectMap<Value>( this );
		 }

		 public override Value GetIfAbsent<T1>( long key, Function0<T1> ifAbsent ) where T1 : Neo4Net.Values.Storable.Value
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.values.storable.Value existing = get(key);
			  Value existing = Get( key );
			  if ( existing != null )
			  {
					return existing;
			  }
			  return ifAbsent.value();
		 }

		 public override bool ContainsKey( long key )
		 {
			  return _refs.containsKey( key );
		 }

		 public override Value RemoveKey( long key )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long ref = refs.removeKeyIfAbsent(key, NONE);
			  long @ref = _refs.removeKeyIfAbsent( key, NONE );
			  return @ref == NONE ? null : _valuesContainer.remove( @ref );
		 }

		 public override Value Remove( long key )
		 {
			  return RemoveKey( key );
		 }

		 public override void Clear()
		 {
			  _refs.clear();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <K, VV> org.eclipse.collections.api.map.MutableMap<K, VV> aggregateInPlaceBy(org.eclipse.collections.api.block.function.Function<? super Neo4Net.values.storable.Value, ? extends K> groupBy, org.eclipse.collections.api.block.function.Function0<? extends VV> zeroValueFactory, org.eclipse.collections.api.block.procedure.Procedure2<? super VV, ? super Neo4Net.values.storable.Value> mutatingAggregator)
		 public override MutableMap<K, VV> AggregateInPlaceBy<K, VV, T1, T2, T3>( Function<T1> groupBy, Function0<T2> zeroValueFactory, Procedure2<T3> mutatingAggregator ) where T1 : K where T2 : VV
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <K, VV> org.eclipse.collections.api.map.MutableMap<K, VV> aggregateBy(org.eclipse.collections.api.block.function.Function<? super Neo4Net.values.storable.Value, ? extends K> groupBy, org.eclipse.collections.api.block.function.Function0<? extends VV> zeroValueFactory, org.eclipse.collections.api.block.function.Function2<? super VV, ? super Neo4Net.values.storable.Value, ? extends VV> nonMutatingAggregator)
		 public override MutableMap<K, VV> AggregateBy<K, VV, T1, T2, T3>( Function<T1> groupBy, Function0<T2> zeroValueFactory, Function2<T3> nonMutatingAggregator ) where T1 : K where T2 : VV where T3 : VV
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <VV> org.eclipse.collections.api.multimap.bag.MutableBagMultimap<VV, Neo4Net.values.storable.Value> groupByEach(org.eclipse.collections.api.block.function.Function<? super Neo4Net.values.storable.Value, ? extends Iterable<VV>> function)
		 public override MutableBagMultimap<VV, Value> GroupByEach<VV, T1>( Function<T1> function ) where T1 : IEnumerable<VV>
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <V, R extends org.eclipse.collections.api.multimap.MutableMultimap<V, Neo4Net.values.storable.Value>> R groupByEach(org.eclipse.collections.api.block.function.Function<? super Neo4Net.values.storable.Value, ? extends Iterable<V>> function, R target)
		 public override R GroupByEach<V, R, T1>( Function<T1> function, R target ) where R : org.eclipse.collections.api.multimap.MutableMultimap<V, Neo4Net.Values.Storable.Value> where T1 : IEnumerable<V>
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <VV> org.eclipse.collections.api.multimap.bag.MutableBagMultimap<VV, Neo4Net.values.storable.Value> groupBy(org.eclipse.collections.api.block.function.Function<? super Neo4Net.values.storable.Value, ? extends VV> function)
		 public override MutableBagMultimap<VV, Value> GroupBy<VV, T1>( Function<T1> function ) where T1 : VV
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <V, R extends org.eclipse.collections.api.multimap.MutableMultimap<V, Neo4Net.values.storable.Value>> R groupBy(org.eclipse.collections.api.block.function.Function<? super Neo4Net.values.storable.Value, ? extends V> function, R target)
		 public override R GroupBy<V, R, T1>( Function<T1> function, R target ) where R : org.eclipse.collections.api.multimap.MutableMultimap<V, Neo4Net.Values.Storable.Value> where T1 : V
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <VV> org.eclipse.collections.api.map.MutableMap<VV, Neo4Net.values.storable.Value> groupByUniqueKey(org.eclipse.collections.api.block.function.Function<? super Neo4Net.values.storable.Value, ? extends VV> function)
		 public override MutableMap<VV, Value> GroupByUniqueKey<VV, T1>( Function<T1> function ) where T1 : VV
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <V, R extends org.eclipse.collections.api.map.MutableMap<V, Neo4Net.values.storable.Value>> R groupByUniqueKey(org.eclipse.collections.api.block.function.Function<? super Neo4Net.values.storable.Value, ? extends V> function, R target)
		 public override R GroupByUniqueKey<V, R, T1>( Function<T1> function, R target ) where R : org.eclipse.collections.api.map.MutableMap<V, Neo4Net.Values.Storable.Value> where T1 : V
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <VV> org.eclipse.collections.api.bag.MutableBag<VV> collectIf(org.eclipse.collections.api.block.predicate.Predicate<? super Neo4Net.values.storable.Value> predicate, org.eclipse.collections.api.block.function.Function<? super Neo4Net.values.storable.Value, ? extends VV> function)
		 public override MutableBag<VV> CollectIf<VV, T1, T2>( Predicate<T1> predicate, Function<T2> function ) where T2 : VV
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <V, R extends java.util.Collection<V>> R collectIf(org.eclipse.collections.api.block.predicate.Predicate<? super Neo4Net.values.storable.Value> predicate, org.eclipse.collections.api.block.function.Function<? super Neo4Net.values.storable.Value, ? extends V> function, R target)
		 public override R CollectIf<V, R, T1, T2>( Predicate<T1> predicate, Function<T2> function, R target ) where R : ICollection<V> where T2 : V
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <VV> org.eclipse.collections.api.bag.MutableBag<VV> collect(org.eclipse.collections.api.block.function.Function<? super Neo4Net.values.storable.Value, ? extends VV> function)
		 public override MutableBag<VV> Collect<VV, T1>( Function<T1> function ) where T1 : VV
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <V, R extends java.util.Collection<V>> R collect(org.eclipse.collections.api.block.function.Function<? super Neo4Net.values.storable.Value, ? extends V> function, R target)
		 public override R Collect<V, R, T1>( Function<T1> function, R target ) where R : ICollection<V> where T1 : V
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public org.eclipse.collections.api.bag.primitive.MutableBooleanBag collectBoolean(org.eclipse.collections.api.block.function.primitive.BooleanFunction<? super Neo4Net.values.storable.Value> booleanFunction)
		 public override MutableBooleanBag CollectBoolean<T1>( BooleanFunction<T1> booleanFunction )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <R extends org.eclipse.collections.api.collection.primitive.MutableBooleanCollection> R collectBoolean(org.eclipse.collections.api.block.function.primitive.BooleanFunction<? super Neo4Net.values.storable.Value> booleanFunction, R target)
		 public override R CollectBoolean<R, T1>( BooleanFunction<T1> booleanFunction, R target ) where R : org.eclipse.collections.api.collection.primitive.MutableBooleanCollection
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public org.eclipse.collections.api.bag.primitive.MutableByteBag collectByte(org.eclipse.collections.api.block.function.primitive.ByteFunction<? super Neo4Net.values.storable.Value> byteFunction)
		 public override MutableByteBag CollectByte<T1>( ByteFunction<T1> byteFunction )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <R extends org.eclipse.collections.api.collection.primitive.MutableByteCollection> R collectByte(org.eclipse.collections.api.block.function.primitive.ByteFunction<? super Neo4Net.values.storable.Value> byteFunction, R target)
		 public override R CollectByte<R, T1>( ByteFunction<T1> byteFunction, R target ) where R : org.eclipse.collections.api.collection.primitive.MutableByteCollection
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public org.eclipse.collections.api.bag.primitive.MutableCharBag collectChar(org.eclipse.collections.api.block.function.primitive.CharFunction<? super Neo4Net.values.storable.Value> charFunction)
		 public override MutableCharBag CollectChar<T1>( CharFunction<T1> charFunction )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <R extends org.eclipse.collections.api.collection.primitive.MutableCharCollection> R collectChar(org.eclipse.collections.api.block.function.primitive.CharFunction<? super Neo4Net.values.storable.Value> charFunction, R target)
		 public override R CollectChar<R, T1>( CharFunction<T1> charFunction, R target ) where R : org.eclipse.collections.api.collection.primitive.MutableCharCollection
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public org.eclipse.collections.api.bag.primitive.MutableDoubleBag collectDouble(org.eclipse.collections.api.block.function.primitive.DoubleFunction<? super Neo4Net.values.storable.Value> doubleFunction)
		 public override MutableDoubleBag CollectDouble<T1>( DoubleFunction<T1> doubleFunction )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <R extends org.eclipse.collections.api.collection.primitive.MutableDoubleCollection> R collectDouble(org.eclipse.collections.api.block.function.primitive.DoubleFunction<? super Neo4Net.values.storable.Value> doubleFunction, R target)
		 public override R CollectDouble<R, T1>( DoubleFunction<T1> doubleFunction, R target ) where R : org.eclipse.collections.api.collection.primitive.MutableDoubleCollection
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public org.eclipse.collections.api.bag.primitive.MutableFloatBag collectFloat(org.eclipse.collections.api.block.function.primitive.FloatFunction<? super Neo4Net.values.storable.Value> floatFunction)
		 public override MutableFloatBag CollectFloat<T1>( FloatFunction<T1> floatFunction )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <R extends org.eclipse.collections.api.collection.primitive.MutableFloatCollection> R collectFloat(org.eclipse.collections.api.block.function.primitive.FloatFunction<? super Neo4Net.values.storable.Value> floatFunction, R target)
		 public override R CollectFloat<R, T1>( FloatFunction<T1> floatFunction, R target ) where R : org.eclipse.collections.api.collection.primitive.MutableFloatCollection
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public org.eclipse.collections.api.bag.primitive.MutableIntBag collectInt(org.eclipse.collections.api.block.function.primitive.IntFunction<? super Neo4Net.values.storable.Value> intFunction)
		 public override MutableIntBag CollectInt<T1>( IntFunction<T1> intFunction )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <R extends org.eclipse.collections.api.collection.primitive.MutableIntCollection> R collectInt(org.eclipse.collections.api.block.function.primitive.IntFunction<? super Neo4Net.values.storable.Value> intFunction, R target)
		 public override R CollectInt<R, T1>( IntFunction<T1> intFunction, R target ) where R : org.eclipse.collections.api.collection.primitive.MutableIntCollection
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public org.eclipse.collections.api.bag.primitive.MutableLongBag collectLong(org.eclipse.collections.api.block.function.primitive.LongFunction<? super Neo4Net.values.storable.Value> longFunction)
		 public override MutableLongBag CollectLong<T1>( LongFunction<T1> longFunction )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <R extends org.eclipse.collections.api.collection.primitive.MutableLongCollection> R collectLong(org.eclipse.collections.api.block.function.primitive.LongFunction<? super Neo4Net.values.storable.Value> longFunction, R target)
		 public override R CollectLong<R, T1>( LongFunction<T1> longFunction, R target ) where R : org.eclipse.collections.api.collection.primitive.MutableLongCollection
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public org.eclipse.collections.api.bag.primitive.MutableShortBag collectShort(org.eclipse.collections.api.block.function.primitive.ShortFunction<? super Neo4Net.values.storable.Value> shortFunction)
		 public override MutableShortBag CollectShort<T1>( ShortFunction<T1> shortFunction )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <R extends org.eclipse.collections.api.collection.primitive.MutableShortCollection> R collectShort(org.eclipse.collections.api.block.function.primitive.ShortFunction<? super Neo4Net.values.storable.Value> shortFunction, R target)
		 public override R CollectShort<R, T1>( ShortFunction<T1> shortFunction, R target ) where R : org.eclipse.collections.api.collection.primitive.MutableShortCollection
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <P, VV> org.eclipse.collections.api.bag.MutableBag<VV> collectWith(org.eclipse.collections.api.block.function.Function2<? super Neo4Net.values.storable.Value, ? super P, ? extends VV> function, P parameter)
		 public override MutableBag<VV> CollectWith<P, VV, T1>( Function2<T1> function, P parameter ) where T1 : VV
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <P, V, R extends java.util.Collection<V>> R collectWith(org.eclipse.collections.api.block.function.Function2<? super Neo4Net.values.storable.Value, ? super P, ? extends V> function, P parameter, R targetCollection)
		 public override R CollectWith<P, V, R, T1>( Function2<T1> function, P parameter, R targetCollection ) where R : ICollection<V> where T1 : V
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <VV> org.eclipse.collections.api.bag.MutableBag<VV> flatCollect(org.eclipse.collections.api.block.function.Function<? super Neo4Net.values.storable.Value, ? extends Iterable<VV>> function)
		 public override MutableBag<VV> FlatCollect<VV, T1>( Function<T1> function ) where T1 : IEnumerable<VV>
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <V, R extends java.util.Collection<V>> R flatCollect(org.eclipse.collections.api.block.function.Function<? super Neo4Net.values.storable.Value, ? extends Iterable<V>> function, R target)
		 public override R FlatCollect<V, R, T1>( Function<T1> function, R target ) where R : ICollection<V> where T1 : IEnumerable<V>
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public Neo4Net.values.storable.Value detect(org.eclipse.collections.api.block.predicate.Predicate<? super Neo4Net.values.storable.Value> predicate)
		 public override Value Detect<T1>( Predicate<T1> predicate )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <P> Neo4Net.values.storable.Value detectWith(org.eclipse.collections.api.block.predicate.Predicate2<? super Neo4Net.values.storable.Value, ? super P> predicate, P parameter)
		 public override Value DetectWith<P, T1>( Predicate2<T1> predicate, P parameter )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public java.util.Optional<Neo4Net.values.storable.Value> detectOptional(org.eclipse.collections.api.block.predicate.Predicate<? super Neo4Net.values.storable.Value> predicate)
		 public override Optional<Value> DetectOptional<T1>( Predicate<T1> predicate )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <P> java.util.Optional<Neo4Net.values.storable.Value> detectWithOptional(org.eclipse.collections.api.block.predicate.Predicate2<? super Neo4Net.values.storable.Value, ? super P> predicate, P parameter)
		 public override Optional<Value> DetectWithOptional<P, T1>( Predicate2<T1> predicate, P parameter )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <P> Neo4Net.values.storable.Value detectWithIfNone(org.eclipse.collections.api.block.predicate.Predicate2<? super Neo4Net.values.storable.Value, ? super P> predicate, P parameter, org.eclipse.collections.api.block.function.Function0<? extends Neo4Net.values.storable.Value> function)
		 public override Value DetectWithIfNone<P, T1, T2>( Predicate2<T1> predicate, P parameter, Function0<T2> function ) where T2 : Neo4Net.Values.Storable.Value
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public int count(org.eclipse.collections.api.block.predicate.Predicate<? super Neo4Net.values.storable.Value> predicate)
		 public override int Count<T1>( Predicate<T1> predicate )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <P> int countWith(org.eclipse.collections.api.block.predicate.Predicate2<? super Neo4Net.values.storable.Value, ? super P> predicate, P parameter)
		 public override int CountWith<P, T1>( Predicate2<T1> predicate, P parameter )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public boolean anySatisfy(org.eclipse.collections.api.block.predicate.Predicate<? super Neo4Net.values.storable.Value> predicate)
		 public override bool AnySatisfy<T1>( Predicate<T1> predicate )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <P> boolean anySatisfyWith(org.eclipse.collections.api.block.predicate.Predicate2<? super Neo4Net.values.storable.Value, ? super P> predicate, P parameter)
		 public override bool AnySatisfyWith<P, T1>( Predicate2<T1> predicate, P parameter )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public boolean allSatisfy(org.eclipse.collections.api.block.predicate.Predicate<? super Neo4Net.values.storable.Value> predicate)
		 public override bool AllSatisfy<T1>( Predicate<T1> predicate )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <P> boolean allSatisfyWith(org.eclipse.collections.api.block.predicate.Predicate2<? super Neo4Net.values.storable.Value, ? super P> predicate, P parameter)
		 public override bool AllSatisfyWith<P, T1>( Predicate2<T1> predicate, P parameter )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public boolean noneSatisfy(org.eclipse.collections.api.block.predicate.Predicate<? super Neo4Net.values.storable.Value> predicate)
		 public override bool NoneSatisfy<T1>( Predicate<T1> predicate )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <P> boolean noneSatisfyWith(org.eclipse.collections.api.block.predicate.Predicate2<? super Neo4Net.values.storable.Value, ? super P> predicate, P parameter)
		 public override bool NoneSatisfyWith<P, T1>( Predicate2<T1> predicate, P parameter )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <IV> IV injectInto(IV injectedValue, org.eclipse.collections.api.block.function.Function2<? super IV, ? super Neo4Net.values.storable.Value, ? extends IV> function)
		 public override IV InjectInto<IV, T1>( IV injectedValue, Function2<T1> function ) where T1 : IV
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public int injectInto(int injectedValue, org.eclipse.collections.api.block.function.primitive.IntObjectToIntFunction<? super Neo4Net.values.storable.Value> function)
		 public override int InjectInto<T1>( int injectedValue, IntObjectToIntFunction<T1> function )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public long injectInto(long injectedValue, org.eclipse.collections.api.block.function.primitive.LongObjectToLongFunction<? super Neo4Net.values.storable.Value> function)
		 public override long InjectInto<T1>( long injectedValue, LongObjectToLongFunction<T1> function )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public float injectInto(float injectedValue, org.eclipse.collections.api.block.function.primitive.FloatObjectToFloatFunction<? super Neo4Net.values.storable.Value> function)
		 public override float InjectInto<T1>( float injectedValue, FloatObjectToFloatFunction<T1> function )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public double injectInto(double injectedValue, org.eclipse.collections.api.block.function.primitive.DoubleObjectToDoubleFunction<? super Neo4Net.values.storable.Value> function)
		 public override double InjectInto<T1>( double injectedValue, DoubleObjectToDoubleFunction<T1> function )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override R Into<R>( R target ) where R : ICollection<Neo4Net.Values.Storable.Value>
		 {
			  throw new System.NotSupportedException();
		 }

		 public override MutableList<Value> ToList()
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <V extends Comparable<? super V>> org.eclipse.collections.api.list.MutableList<Neo4Net.values.storable.Value> toSortedListBy(org.eclipse.collections.api.block.function.Function<? super Neo4Net.values.storable.Value, ? extends V> function)
		 public override MutableList<Value> ToSortedListBy<V, T1>( Function<T1> function )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override MutableSet<Value> ToSet()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override MutableSortedSet<Value> ToSortedSet()
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public org.eclipse.collections.api.set.sorted.MutableSortedSet<Neo4Net.values.storable.Value> toSortedSet(java.util.Comparator<? super Neo4Net.values.storable.Value> comparator)
		 public override MutableSortedSet<Value> ToSortedSet<T1>( IComparer<T1> comparator )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <V extends Comparable<? super V>> org.eclipse.collections.api.set.sorted.MutableSortedSet<Neo4Net.values.storable.Value> toSortedSetBy(org.eclipse.collections.api.block.function.Function<? super Neo4Net.values.storable.Value, ? extends V> function)
		 public override MutableSortedSet<Value> ToSortedSetBy<V, T1>( Function<T1> function )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override MutableBag<Value> ToBag()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override MutableSortedBag<Value> ToSortedBag()
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public org.eclipse.collections.api.bag.sorted.MutableSortedBag<Neo4Net.values.storable.Value> toSortedBag(java.util.Comparator<? super Neo4Net.values.storable.Value> comparator)
		 public override MutableSortedBag<Value> ToSortedBag<T1>( IComparer<T1> comparator )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <V extends Comparable<? super V>> org.eclipse.collections.api.bag.sorted.MutableSortedBag<Neo4Net.values.storable.Value> toSortedBagBy(org.eclipse.collections.api.block.function.Function<? super Neo4Net.values.storable.Value, ? extends V> function)
		 public override MutableSortedBag<Value> ToSortedBagBy<V, T1>( Function<T1> function )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <NK, NV> org.eclipse.collections.api.map.MutableMap<NK, NV> toMap(org.eclipse.collections.api.block.function.Function<? super Neo4Net.values.storable.Value, ? extends NK> keyFunction, org.eclipse.collections.api.block.function.Function<? super Neo4Net.values.storable.Value, ? extends NV> valueFunction)
		 public override MutableMap<NK, NV> ToMap<NK, NV, T1, T2>( Function<T1> keyFunction, Function<T2> valueFunction ) where T1 : NK where T2 : NV
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <NK, NV> org.eclipse.collections.api.map.sorted.MutableSortedMap<NK, NV> toSortedMap(org.eclipse.collections.api.block.function.Function<? super Neo4Net.values.storable.Value, ? extends NK> keyFunction, org.eclipse.collections.api.block.function.Function<? super Neo4Net.values.storable.Value, ? extends NV> valueFunction)
		 public override MutableSortedMap<NK, NV> ToSortedMap<NK, NV, T1, T2>( Function<T1> keyFunction, Function<T2> valueFunction ) where T1 : NK where T2 : NV
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <NK, NV> org.eclipse.collections.api.map.sorted.MutableSortedMap<NK, NV> toSortedMap(java.util.Comparator<? super NK> comparator, org.eclipse.collections.api.block.function.Function<? super Neo4Net.values.storable.Value, ? extends NK> keyFunction, org.eclipse.collections.api.block.function.Function<? super Neo4Net.values.storable.Value, ? extends NV> valueFunction)
		 public override MutableSortedMap<NK, NV> ToSortedMap<NK, NV, T1, T2, T3>( IComparer<T1> comparator, Function<T2> keyFunction, Function<T3> valueFunction ) where T2 : NK where T3 : NV
		 {
			  throw new System.NotSupportedException();
		 }

		 public override LazyIterable<Value> AsLazy()
		 {
			  return new LazyIterableAdapter<Value>( this );
		 }

		 public override object[] ToArray()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override T[] ToArray<T>( T[] target )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public Neo4Net.values.storable.Value min(java.util.Comparator<? super Neo4Net.values.storable.Value> comparator)
		 public override Value Min<T1>( IComparer<T1> comparator )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public Neo4Net.values.storable.Value max(java.util.Comparator<? super Neo4Net.values.storable.Value> comparator)
		 public override Value Max<T1>( IComparer<T1> comparator )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override Value Min()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override Value Max()
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <V extends Comparable<? super V>> Neo4Net.values.storable.Value minBy(org.eclipse.collections.api.block.function.Function<? super Neo4Net.values.storable.Value, ? extends V> function)
		 public override Value MinBy<V, T1>( Function<T1> function )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <V extends Comparable<? super V>> Neo4Net.values.storable.Value maxBy(org.eclipse.collections.api.block.function.Function<? super Neo4Net.values.storable.Value, ? extends V> function)
		 public override Value MaxBy<V, T1>( Function<T1> function )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public long sumOfInt(org.eclipse.collections.api.block.function.primitive.IntFunction<? super Neo4Net.values.storable.Value> function)
		 public override long SumOfInt<T1>( IntFunction<T1> function )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public double sumOfFloat(org.eclipse.collections.api.block.function.primitive.FloatFunction<? super Neo4Net.values.storable.Value> function)
		 public override double SumOfFloat<T1>( FloatFunction<T1> function )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public long sumOfLong(org.eclipse.collections.api.block.function.primitive.LongFunction<? super Neo4Net.values.storable.Value> function)
		 public override long SumOfLong<T1>( LongFunction<T1> function )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public double sumOfDouble(org.eclipse.collections.api.block.function.primitive.DoubleFunction<? super Neo4Net.values.storable.Value> function)
		 public override double SumOfDouble<T1>( DoubleFunction<T1> function )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override MutableBag<S> SelectInstancesOf<S>( Type clazz )
		 {
				 clazz = typeof( S );
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public org.eclipse.collections.api.bag.MutableBag<Neo4Net.values.storable.Value> select(org.eclipse.collections.api.block.predicate.Predicate<? super Neo4Net.values.storable.Value> predicate)
		 public override MutableBag<Value> Select<T1>( Predicate<T1> predicate )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <R extends java.util.Collection<Neo4Net.values.storable.Value>> R select(org.eclipse.collections.api.block.predicate.Predicate<? super Neo4Net.values.storable.Value> predicate, R target)
		 public override R Select<R, T1>( Predicate<T1> predicate, R target ) where R : ICollection<Neo4Net.Values.Storable.Value>
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <P> org.eclipse.collections.api.bag.MutableBag<Neo4Net.values.storable.Value> selectWith(org.eclipse.collections.api.block.predicate.Predicate2<? super Neo4Net.values.storable.Value, ? super P> predicate, P parameter)
		 public override MutableBag<Value> SelectWith<P, T1>( Predicate2<T1> predicate, P parameter )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <P, R extends java.util.Collection<Neo4Net.values.storable.Value>> R selectWith(org.eclipse.collections.api.block.predicate.Predicate2<? super Neo4Net.values.storable.Value, ? super P> predicate, P parameter, R targetCollection)
		 public override R SelectWith<P, R, T1>( Predicate2<T1> predicate, P parameter, R targetCollection ) where R : ICollection<Neo4Net.Values.Storable.Value>
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public org.eclipse.collections.api.bag.MutableBag<Neo4Net.values.storable.Value> reject(org.eclipse.collections.api.block.predicate.Predicate<? super Neo4Net.values.storable.Value> predicate)
		 public override MutableBag<Value> Reject<T1>( Predicate<T1> predicate )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <P> org.eclipse.collections.api.bag.MutableBag<Neo4Net.values.storable.Value> rejectWith(org.eclipse.collections.api.block.predicate.Predicate2<? super Neo4Net.values.storable.Value, ? super P> predicate, P parameter)
		 public override MutableBag<Value> RejectWith<P, T1>( Predicate2<T1> predicate, P parameter )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <R extends java.util.Collection<Neo4Net.values.storable.Value>> R reject(org.eclipse.collections.api.block.predicate.Predicate<? super Neo4Net.values.storable.Value> predicate, R target)
		 public override R Reject<R, T1>( Predicate<T1> predicate, R target ) where R : ICollection<Neo4Net.Values.Storable.Value>
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <P, R extends java.util.Collection<Neo4Net.values.storable.Value>> R rejectWith(org.eclipse.collections.api.block.predicate.Predicate2<? super Neo4Net.values.storable.Value, ? super P> predicate, P parameter, R targetCollection)
		 public override R RejectWith<P, R, T1>( Predicate2<T1> predicate, P parameter, R targetCollection ) where R : ICollection<Neo4Net.Values.Storable.Value>
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public org.eclipse.collections.api.partition.bag.PartitionMutableBag<Neo4Net.values.storable.Value> partition(org.eclipse.collections.api.block.predicate.Predicate<? super Neo4Net.values.storable.Value> predicate)
		 public override PartitionMutableBag<Value> Partition<T1>( Predicate<T1> predicate )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <P> org.eclipse.collections.api.partition.bag.PartitionMutableBag<Neo4Net.values.storable.Value> partitionWith(org.eclipse.collections.api.block.predicate.Predicate2<? super Neo4Net.values.storable.Value, ? super P> predicate, P parameter)
		 public override PartitionMutableBag<Value> PartitionWith<P, T1>( Predicate2<T1> predicate, P parameter )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override MutableBag<Pair<Value, S>> Zip<S>( IEnumerable<S> that )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override R Zip<S, R>( IEnumerable<S> that, R target ) where R : ICollection<org.eclipse.collections.api.tuple.Pair<Neo4Net.Values.Storable.Value, S>>
		 {
			  throw new System.NotSupportedException();
		 }

		 public override MutableSet<Pair<Value, int>> ZipWithIndex()
		 {
			  throw new System.NotSupportedException();
		 }

		 public override R ZipWithIndex<R>( R target ) where R : ICollection<org.eclipse.collections.api.tuple.Pair<Neo4Net.Values.Storable.Value, int>>
		 {
			  throw new System.NotSupportedException();
		 }

		 public override RichIterable<RichIterable<Value>> Chunk( int size )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <VV> org.eclipse.collections.api.map.primitive.MutableObjectLongMap<VV> sumByInt(org.eclipse.collections.api.block.function.Function<? super Neo4Net.values.storable.Value, ? extends VV> groupBy, org.eclipse.collections.api.block.function.primitive.IntFunction<? super Neo4Net.values.storable.Value> function)
		 public override MutableObjectLongMap<VV> SumByInt<VV, T1, T2>( Function<T1> groupBy, IntFunction<T2> function ) where T1 : VV
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <VV> org.eclipse.collections.api.map.primitive.MutableObjectDoubleMap<VV> sumByFloat(org.eclipse.collections.api.block.function.Function<? super Neo4Net.values.storable.Value, ? extends VV> groupBy, org.eclipse.collections.api.block.function.primitive.FloatFunction<? super Neo4Net.values.storable.Value> function)
		 public override MutableObjectDoubleMap<VV> SumByFloat<VV, T1, T2>( Function<T1> groupBy, FloatFunction<T2> function ) where T1 : VV
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <VV> org.eclipse.collections.api.map.primitive.MutableObjectLongMap<VV> sumByLong(org.eclipse.collections.api.block.function.Function<? super Neo4Net.values.storable.Value, ? extends VV> groupBy, org.eclipse.collections.api.block.function.primitive.LongFunction<? super Neo4Net.values.storable.Value> function)
		 public override MutableObjectLongMap<VV> SumByLong<VV, T1, T2>( Function<T1> groupBy, LongFunction<T2> function ) where T1 : VV
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <VV> org.eclipse.collections.api.map.primitive.MutableObjectDoubleMap<VV> sumByDouble(org.eclipse.collections.api.block.function.Function<? super Neo4Net.values.storable.Value, ? extends VV> groupBy, org.eclipse.collections.api.block.function.primitive.DoubleFunction<? super Neo4Net.values.storable.Value> function)
		 public override MutableObjectDoubleMap<VV> SumByDouble<VV, T1, T2>( Function<T1> groupBy, DoubleFunction<T2> function ) where T1 : VV
		 {
			  throw new System.NotSupportedException();
		 }

		 public override void AppendString( Appendable appendable, string start, string separator, string end )
		 {
			  try
			  {
					appendable.append( format( "ValuesMap[size: %d]", _refs.size() ) );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override void ForEachKey( LongProcedure procedure )
		 {
			  _refs.forEachKey( procedure );
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public void forEachKeyValue(org.eclipse.collections.api.block.procedure.primitive.LongObjectProcedure<? super Neo4Net.values.storable.Value> procedure)
		 public override void ForEachKeyValue<T1>( LongObjectProcedure<T1> procedure )
		 {
			  _refs.forEachKeyValue((key, @ref) =>
			  {
				Value value = _valuesContainer.get( @ref );
				procedure.value( key, value );
			  });
		 }

		 public override bool ContainsValue( object value )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public void forEachValue(org.eclipse.collections.api.block.procedure.Procedure<? super Neo4Net.values.storable.Value> procedure)
		 public override void ForEachValue<T1>( Procedure<T1> procedure )
		 {
			  ForEachKeyValue( ( k, v ) => procedure.value( v ) );
		 }

		 public override ICollection<Value> Values()
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public void forEach(org.eclipse.collections.api.block.procedure.Procedure<? super Neo4Net.values.storable.Value> procedure)
		 public override void ForEach<T1>( Procedure<T1> procedure )
		 {
			  ForEachValue( procedure );
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public void forEachWithIndex(org.eclipse.collections.api.block.procedure.primitive.ObjectIntProcedure<? super Neo4Net.values.storable.Value> objectIntProcedure)
		 public override void ForEachWithIndex<T1>( ObjectIntProcedure<T1> objectIntProcedure )
		 {
			  throw new System.NotSupportedException();
		 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public <P> void forEachWith(org.eclipse.collections.api.block.procedure.Procedure2<? super Neo4Net.values.storable.Value, ? super P> procedure, P parameter)
		 public override void ForEachWith<P, T1>( Procedure2<T1> procedure, P parameter )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override IEnumerator<Value> Iterator()
		 {
			  throw new System.NotSupportedException();
		 }

		 private class KeyValuesView : AbstractLazyIterable<LongObjectPair<Value>>
		 {
			 private readonly ValuesMap _outerInstance;

			 public KeyValuesView( ValuesMap outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: public void each(org.eclipse.collections.api.block.procedure.Procedure<? super org.eclipse.collections.api.tuple.primitive.LongObjectPair<Neo4Net.values.storable.Value>> procedure)
			  public override void Each<T1>( Procedure<T1> procedure )
			  {
					foreach ( LongObjectPair<Value> valueLongObjectPair in this )
					{
						 procedure.value( valueLongObjectPair );
					}
			  }

			  public override IEnumerator<LongObjectPair<Value>> Iterator()
			  {
					IEnumerator<LongLongPair> refsIterator = outerInstance.refs.keyValuesView().GetEnumerator();
					return new IteratorAnonymousInnerClass( this, refsIterator );
			  }

			  private class IteratorAnonymousInnerClass : IEnumerator<LongObjectPair<Value>>
			  {
				  private readonly KeyValuesView _outerInstance;

				  private IEnumerator<LongLongPair> _refsIterator;

				  public IteratorAnonymousInnerClass( KeyValuesView outerInstance, IEnumerator<LongLongPair> refsIterator )
				  {
					  this.outerInstance = outerInstance;
					  this._refsIterator = refsIterator;
				  }

				  public bool hasNext()
				  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						return _refsIterator.hasNext();
				  }

				  public LongObjectPair<Value> next()
				  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.tuple.primitive.LongLongPair key2ref = refsIterator.next();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						LongLongPair key2ref = _refsIterator.next();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long key = key2ref.getOne();
						long key = key2ref.One;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long ref = key2ref.getTwo();
						long @ref = key2ref.Two;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.values.storable.Value value = valuesContainer.get(ref);
						Value value = outerInstance.outerInstance.valuesContainer.get( @ref );
						return PrimitiveTuples.pair( key, value );
				  }
			  }
		 }
	}

}