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
	using LongFunction = org.eclipse.collections.api.block.function.primitive.LongFunction;
	using LongFunction0 = org.eclipse.collections.api.block.function.primitive.LongFunction0;
	using LongToLongFunction = org.eclipse.collections.api.block.function.primitive.LongToLongFunction;
	using LongLongProcedure = org.eclipse.collections.api.block.procedure.primitive.LongLongProcedure;
	using LongProcedure = org.eclipse.collections.api.block.procedure.primitive.LongProcedure;
	using MutableLongIterator = org.eclipse.collections.api.iterator.MutableLongIterator;
	using ImmutableLongList = org.eclipse.collections.api.list.primitive.ImmutableLongList;
	using MutableLongList = org.eclipse.collections.api.list.primitive.MutableLongList;
	using LongLongMap = org.eclipse.collections.api.map.primitive.LongLongMap;
	using MutableLongLongMap = org.eclipse.collections.api.map.primitive.MutableLongLongMap;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongLongPair = org.eclipse.collections.api.tuple.primitive.LongLongPair;
	using LongLists = org.eclipse.collections.impl.factory.primitive.LongLists;
	using LongLongMaps = org.eclipse.collections.impl.factory.primitive.LongLongMaps;
	using LongSets = org.eclipse.collections.impl.factory.primitive.LongSets;
	using LongLongHashMap = org.eclipse.collections.impl.map.mutable.primitive.LongLongHashMap;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using Nested = org.junit.jupiter.api.Nested;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using LocalMemoryTracker = Neo4Net.Memory.LocalMemoryTracker;
	using IMemoryAllocationTracker = Neo4Net.Memory.IMemoryAllocationTracker;
	using Inject = Neo4Net.Test.extension.Inject;
	using RandomExtension = Neo4Net.Test.extension.RandomExtension;
	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.eclipse.collections.impl.list.mutable.primitive.LongArrayList.newListWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.eclipse.collections.impl.map.mutable.primitive.LongLongHashMap.newWithKeysValues;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.eclipse.collections.impl.tuple.primitive.PrimitiveTuples.pair;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertDoesNotThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doReturn;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.collection.LinearProbeLongLongHashMap.DEFAULT_CAPACITY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.collection.LinearProbeLongLongHashMap.REMOVALS_FACTOR;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(RandomExtension.class) class LinearProbeLongLongHashMapTest
	internal class LinearProbeLongLongHashMapTest
	{
		private bool InstanceFieldsInitialized = false;

		public LinearProbeLongLongHashMapTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_memoryAllocator = new OffHeapMemoryAllocator( _memoryTracker, _blockAllocator );
			_map = NewMap();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.RandomRule rnd;
		 private RandomRule _rnd;

		 private readonly CachingOffHeapBlockAllocator _blockAllocator = new CachingOffHeapBlockAllocator();
		 private readonly IMemoryAllocationTracker _memoryTracker = new LocalMemoryTracker();
		 private MemoryAllocator _memoryAllocator;

		 private LinearProbeLongLongHashMap _map;

		 private LinearProbeLongLongHashMap NewMap()
		 {
			  return new LinearProbeLongLongHashMap( _memoryAllocator );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach void tearDown()
		 internal virtual void TearDown()
		 {
			  _map.close();
			  assertEquals( 0, _memoryTracker.usedDirectMemory(), "Leaking memory" );
			  _blockAllocator.release();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putGetRemove()
		 internal virtual void PutGetRemove()
		 {
			  _map.put( 0, 10 );
			  _map.put( 1, 11 );
			  _map.put( 2, 12 );

			  assertEquals( 10, _map.get( 0 ) );
			  assertEquals( 11, _map.get( 1 ) );
			  assertEquals( 12, _map.get( 2 ) );
			  // default empty value
			  assertEquals( 0, _map.get( 3 ) );

			  _map.remove( 1 );
			  _map.remove( 2 );
			  _map.remove( 0 );

			  assertEquals( 0, _map.get( 0 ) );
			  assertEquals( 0, _map.get( 1 ) );
			  assertEquals( 0, _map.get( 2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putAll()
		 internal virtual void PutAll()
		 {
			  _map.putAll( newWithKeysValues( 0, 10, 1, 11, 2, 12 ) );
			  assertEquals( 3, _map.size() );
			  assertEquals( 10, _map.get( 0 ) );
			  assertEquals( 11, _map.get( 1 ) );
			  assertEquals( 12, _map.get( 2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getIfAbsent()
		 internal virtual void getIfAbsent()
		 {
			  assertEquals( -1, _map.getIfAbsent( 0, -1 ) );
			  assertEquals( -1, _map.getIfAbsent( 1, -1 ) );
			  assertEquals( -1, _map.getIfAbsent( 2, -1 ) );
			  assertEquals( -1, _map.getIfAbsent( 3, -1 ) );

			  _map.putAll( newWithKeysValues( 0, 10, 1, 11, 2, 12 ) );

			  assertEquals( 10, _map.getIfAbsent( 0, -1 ) );
			  assertEquals( 11, _map.getIfAbsent( 1, -1 ) );
			  assertEquals( 12, _map.getIfAbsent( 2, -1 ) );
			  assertEquals( -1, _map.getIfAbsent( 3, -1 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getIfAbsentPut()
		 internal virtual void getIfAbsentPut()
		 {
			  assertEquals( 10, _map.getIfAbsentPut( 0, 10 ) );
			  assertEquals( 10, _map.getIfAbsentPut( 0, 100 ) );
			  assertEquals( 11, _map.getIfAbsentPut( 1, 11 ) );
			  assertEquals( 11, _map.getIfAbsentPut( 1, 110 ) );
			  assertEquals( 12, _map.getIfAbsentPut( 2, 12 ) );
			  assertEquals( 12, _map.getIfAbsentPut( 2, 120 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getIfAbsentPut_Supplier()
		 internal virtual void getIfAbsentPut_Supplier()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.block.function.primitive.LongFunction0 supplier = mock(org.eclipse.collections.api.block.function.primitive.LongFunction0.class);
			  LongFunction0 supplier = mock( typeof( LongFunction0 ) );
			  doReturn( 10L, 11L, 12L ).when( supplier ).value();

			  assertEquals( 10, _map.getIfAbsentPut( 0, supplier ) );
			  assertEquals( 11, _map.getIfAbsentPut( 1, supplier ) );
			  assertEquals( 12, _map.getIfAbsentPut( 2, supplier ) );
			  verify( supplier, times( 3 ) ).value();

			  assertEquals( 10, _map.getIfAbsentPut( 0, supplier ) );
			  assertEquals( 11, _map.getIfAbsentPut( 1, supplier ) );
			  assertEquals( 12, _map.getIfAbsentPut( 2, supplier ) );
			  verifyNoMoreInteractions( supplier );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getIfAbsentPutWithKey()
		 internal virtual void getIfAbsentPutWithKey()
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("Convert2Lambda") final org.eclipse.collections.api.block.function.primitive.LongToLongFunction function = spy(new org.eclipse.collections.api.block.function.primitive.LongToLongFunction()
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
			  LongToLongFunction function = spy( new LongToLongFunctionAnonymousInnerClass( this ) );

			  assertEquals( 10, _map.getIfAbsentPutWithKey( 0, function ) );
			  assertEquals( 10, _map.getIfAbsentPutWithKey( 0, function ) );
			  assertEquals( 11, _map.getIfAbsentPutWithKey( 1, function ) );
			  assertEquals( 11, _map.getIfAbsentPutWithKey( 1, function ) );
			  assertEquals( 12, _map.getIfAbsentPutWithKey( 2, function ) );
			  assertEquals( 12, _map.getIfAbsentPutWithKey( 2, function ) );

			  verify( function ).valueOf( eq( 0L ) );
			  verify( function ).valueOf( eq( 1L ) );
			  verify( function ).valueOf( eq( 2L ) );
			  verifyNoMoreInteractions( function );
		 }

		 private class LongToLongFunctionAnonymousInnerClass : LongToLongFunction
		 {
			 private readonly LinearProbeLongLongHashMapTest _outerInstance;

			 public LongToLongFunctionAnonymousInnerClass( LinearProbeLongLongHashMapTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override long valueOf( long x )
			 {
				  return 10 + x;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getIfAbsentPutWith()
		 internal virtual void getIfAbsentPutWith()
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({"Convert2Lambda", "Anonymous2MethodRef"}) final org.eclipse.collections.api.block.function.primitive.LongFunction<String> function = spy(new org.eclipse.collections.api.block.function.primitive.LongFunction<String>()
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
			  LongFunction<string> function = spy( new LongFunctionAnonymousInnerClass( this ) );

			  assertEquals( 10, _map.getIfAbsentPutWith( 0, function, "10" ) );
			  assertEquals( 10, _map.getIfAbsentPutWith( 0, function, "10" ) );
			  assertEquals( 11, _map.getIfAbsentPutWith( 1, function, "11" ) );
			  assertEquals( 11, _map.getIfAbsentPutWith( 1, function, "11" ) );
			  assertEquals( 12, _map.getIfAbsentPutWith( 2, function, "12" ) );
			  assertEquals( 12, _map.getIfAbsentPutWith( 2, function, "12" ) );

			  verify( function ).longValueOf( eq( "10" ) );
			  verify( function ).longValueOf( eq( "11" ) );
			  verify( function ).longValueOf( eq( "12" ) );
			  verifyNoMoreInteractions( function );
		 }

		 private class LongFunctionAnonymousInnerClass : LongFunction<string>
		 {
			 private readonly LinearProbeLongLongHashMapTest _outerInstance;

			 public LongFunctionAnonymousInnerClass( LinearProbeLongLongHashMapTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }


			 public override long longValueOf( string s )
			 {
				  return Convert.ToInt64( s );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getOrThrow()
		 internal virtual void getOrThrow()
		 {
			  assertThrows( typeof( System.InvalidOperationException ), () => _map.getOrThrow(0) );
			  assertThrows( typeof( System.InvalidOperationException ), () => _map.getOrThrow(1) );
			  assertThrows( typeof( System.InvalidOperationException ), () => _map.getOrThrow(2) );

			  _map.putAll( newWithKeysValues( 0, 10, 1, 11, 2, 12 ) );

			  assertEquals( 10, _map.getOrThrow( 0 ) );
			  assertEquals( 11, _map.getOrThrow( 1 ) );
			  assertEquals( 12, _map.getOrThrow( 2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putOverwrite()
		 internal virtual void PutOverwrite()
		 {
			  _map.putAll( newWithKeysValues( 0, 10, 1, 11, 2, 12 ) );

			  assertEquals( 10, _map.get( 0 ) );
			  assertEquals( 11, _map.get( 1 ) );
			  assertEquals( 12, _map.get( 2 ) );

			  _map.putAll( newWithKeysValues( 0, 20, 1, 21, 2, 22 ) );

			  assertEquals( 20, _map.get( 0 ) );
			  assertEquals( 21, _map.get( 1 ) );
			  assertEquals( 22, _map.get( 2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void size()
		 internal virtual void Size()
		 {
			  assertEquals( 0, _map.size() );
			  _map.put( 0, 10 );
			  assertEquals( 1, _map.size() );
			  _map.put( 1, 11 );
			  assertEquals( 2, _map.size() );
			  _map.put( 2, 12 );
			  assertEquals( 3, _map.size() );
			  _map.put( 0, 20 );
			  _map.put( 1, 20 );
			  _map.put( 2, 20 );
			  assertEquals( 3, _map.size() );
			  _map.remove( 0 );
			  assertEquals( 2, _map.size() );
			  _map.remove( 1 );
			  assertEquals( 1, _map.size() );
			  _map.remove( 2 );
			  assertEquals( 0, _map.size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void containsKey()
		 internal virtual void ContainsKey()
		 {
			  assertFalse( _map.containsKey( 0 ) );
			  assertFalse( _map.containsKey( 1 ) );
			  assertFalse( _map.containsKey( 2 ) );

			  _map.put( 0, 10 );
			  assertTrue( _map.containsKey( 0 ) );
			  _map.put( 1, 11 );
			  assertTrue( _map.containsKey( 1 ) );
			  _map.put( 2, 12 );
			  assertTrue( _map.containsKey( 2 ) );

			  _map.remove( 0 );
			  assertFalse( _map.containsKey( 0 ) );
			  _map.remove( 1 );
			  assertFalse( _map.containsKey( 1 ) );
			  _map.remove( 2 );
			  assertFalse( _map.containsKey( 2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void containsValue()
		 internal virtual void ContainsValue()
		 {
			  assertFalse( _map.containsValue( 10 ) );
			  assertFalse( _map.containsValue( 11 ) );
			  assertFalse( _map.containsValue( 12 ) );

			  _map.put( 0, 10 );
			  assertTrue( _map.containsValue( 10 ) );

			  _map.put( 1, 11 );
			  assertTrue( _map.containsValue( 11 ) );

			  _map.put( 2, 12 );
			  assertTrue( _map.containsValue( 12 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void removeKeyIfAbsent()
		 internal virtual void RemoveKeyIfAbsent()
		 {
			  assertEquals( 10, _map.removeKeyIfAbsent( 0, 10 ) );
			  assertEquals( 11, _map.removeKeyIfAbsent( 1, 11 ) );
			  assertEquals( 12, _map.removeKeyIfAbsent( 2, 12 ) );

			  _map.put( 0, 10 );
			  _map.put( 1, 11 );
			  _map.put( 2, 12 );

			  assertEquals( 10, _map.removeKeyIfAbsent( 0, -1 ) );
			  assertEquals( 11, _map.removeKeyIfAbsent( 1, -1 ) );
			  assertEquals( 12, _map.removeKeyIfAbsent( 2, -1 ) );

			  assertEquals( 0, _map.size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void updateValue()
		 internal virtual void UpdateValue()
		 {
			  _map.updateValue( 0, 10, v => -v );
			  _map.updateValue( 1, 11, v => -v );
			  _map.updateValue( 2, 12, v => -v );

			  assertEquals( -10, _map.get( 0 ) );
			  assertEquals( -11, _map.get( 1 ) );
			  assertEquals( -12, _map.get( 2 ) );

			  _map.updateValue( 0, 0, v => -v );
			  _map.updateValue( 1, 0, v => -v );
			  _map.updateValue( 2, 0, v => -v );

			  assertEquals( 10, _map.get( 0 ) );
			  assertEquals( 11, _map.get( 1 ) );
			  assertEquals( 12, _map.get( 2 ) );

			  assertEquals( 3, _map.size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addToValue()
		 internal virtual void AddToValue()
		 {
			  assertEquals( 10, _map.addToValue( 0, 10 ) );
			  assertEquals( 11, _map.addToValue( 1, 11 ) );
			  assertEquals( 12, _map.addToValue( 2, 12 ) );

			  assertEquals( 110, _map.addToValue( 0, 100 ) );
			  assertEquals( 111, _map.addToValue( 1, 100 ) );
			  assertEquals( 112, _map.addToValue( 2, 100 ) );

			  assertEquals( 3, _map.size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void forEachKey()
		 internal virtual void ForEachKey()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.block.procedure.primitive.LongProcedure consumer = mock(org.eclipse.collections.api.block.procedure.primitive.LongProcedure.class);
			  LongProcedure consumer = mock( typeof( LongProcedure ) );
			  _map.putAll( newWithKeysValues( 0, 10, 1, 11, 2, 12 ) );

			  _map.forEachKey( consumer );

			  verify( consumer ).value( eq( 0L ) );
			  verify( consumer ).value( eq( 1L ) );
			  verify( consumer ).value( eq( 2L ) );
			  verifyNoMoreInteractions( consumer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void forEachValue()
		 internal virtual void ForEachValue()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.block.procedure.primitive.LongProcedure consumer = mock(org.eclipse.collections.api.block.procedure.primitive.LongProcedure.class);
			  LongProcedure consumer = mock( typeof( LongProcedure ) );
			  _map.putAll( newWithKeysValues( 0, 10, 1, 11, 2, 12 ) );

			  _map.forEachValue( consumer );

			  verify( consumer ).value( eq( 10L ) );
			  verify( consumer ).value( eq( 11L ) );
			  verify( consumer ).value( eq( 12L ) );
			  verifyNoMoreInteractions( consumer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void forEachKeyValue()
		 internal virtual void ForEachKeyValue()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.block.procedure.primitive.LongLongProcedure consumer = mock(org.eclipse.collections.api.block.procedure.primitive.LongLongProcedure.class);
			  LongLongProcedure consumer = mock( typeof( LongLongProcedure ) );
			  _map.putAll( newWithKeysValues( 0, 10, 1, 11, 2, 12 ) );

			  _map.forEachKeyValue( consumer );

			  verify( consumer ).value( eq( 0L ), eq( 10L ) );
			  verify( consumer ).value( eq( 1L ), eq( 11L ) );
			  verify( consumer ).value( eq( 2L ), eq( 12L ) );
			  verifyNoMoreInteractions( consumer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void clear()
		 internal virtual void Clear()
		 {
			  _map.clear();
			  assertEquals( 0, _map.size() );

			  _map.putAll( newWithKeysValues( 0, 10, 1, 11, 2, 12 ) );
			  assertEquals( 3, _map.size() );

			  _map.clear();
			  assertEquals( 0, _map.size() );

			  _map.clear();
			  assertEquals( 0, _map.size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void toList()
		 internal virtual void ToList()
		 {
			  assertEquals( 0, _map.toList().size() );

			  _map.putAll( ToMap( 0, 1, 2, 3, 4, 5 ) );
			  assertEquals( newListWith( 0, 1, 2, 3, 4, 5 ), _map.toList().sortThis() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void toArray()
		 internal virtual void ToArray()
		 {
			  assertEquals( 0, _map.toArray().Length );

			  _map.putAll( ToMap( 0, 1, 2, 3, 4, 5 ) );

			  assertArrayEquals( new long[]{ 0, 1, 2, 3, 4, 5 }, _map.toSortedArray() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void keysIterator()
		 internal virtual void KeysIterator()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.primitive.LongSet keys = org.eclipse.collections.impl.factory.primitive.LongSets.immutable.of(0L, 1L, 2L, 42L);
			  LongSet keys = LongSets.immutable.of( 0L, 1L, 2L, 42L );
			  keys.forEach( k => _map.put( k, k * 10 ) );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.iterator.MutableLongIterator iter = map.longIterator();
			  MutableLongIterator iter = _map.longIterator();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.set.primitive.MutableLongSet found = new org.eclipse.collections.impl.set.mutable.primitive.LongHashSet();
			  MutableLongSet found = new LongHashSet();
			  while ( iter.hasNext() )
			  {
					found.add( iter.next() );
			  }

			  assertEquals( keys, found );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void keysIteratorFailsWhenMapIsClosed()
		 internal virtual void KeysIteratorFailsWhenMapIsClosed()
		 {
			  _map.putAll( newWithKeysValues( 0, 10, 1, 11, 2, 12 ) );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.iterator.MutableLongIterator iter = map.longIterator();
			  MutableLongIterator iter = _map.longIterator();

			  assertTrue( iter.hasNext() );
			  assertEquals( 0, iter.next() );

			  _map.close();

			  assertThrows( typeof( ConcurrentModificationException ), iter.hasNext );
			  assertThrows( typeof( ConcurrentModificationException ), iter.next );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void grow()
		 internal virtual void Grow()
		 {
			  _map = spy( _map );

			  for ( int i = 0; i < DEFAULT_CAPACITY; i++ )
			  {
					_map.put( 100 + i, i );
			  }
			  verify( _map ).growAndRehash();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void rehashWhenTooManyRemovals()
		 internal virtual void RehashWhenTooManyRemovals()
		 {
			  _map = spy( _map );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int numOfElements = DEFAULT_CAPACITY / 2;
			  int numOfElements = DEFAULT_CAPACITY / 2;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int removalsToTriggerRehashing = (int)(DEFAULT_CAPACITY * REMOVALS_FACTOR);
			  int removalsToTriggerRehashing = ( int )( DEFAULT_CAPACITY * REMOVALS_FACTOR );

			  for ( int i = 0; i < numOfElements; i++ )
			  {
					_map.put( 100 + i, i );
			  }

			  assertEquals( numOfElements, _map.size() );
			  verify( _map, never() ).rehashWithoutGrow();
			  verify( _map, never() ).growAndRehash();

			  for ( int i = 0; i < removalsToTriggerRehashing; i++ )
			  {
					_map.remove( 100 + i );
			  }

			  assertEquals( numOfElements - removalsToTriggerRehashing, _map.size() );
			  verify( _map ).rehashWithoutGrow();
			  verify( _map, never() ).growAndRehash();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void randomizedTest()
		 internal virtual void RandomizedTest()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int count = 10000 + rnd.nextInt(1000);
			  int count = 10000 + _rnd.Next( 1000 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.map.primitive.MutableLongLongMap m = new org.eclipse.collections.impl.map.mutable.primitive.LongLongHashMap();
			  MutableLongLongMap m = new LongLongHashMap();
			  while ( m.size() < count )
			  {
					m.put( _rnd.nextLong(), _rnd.nextLong() );
			  }

			  m.forEachKeyValue((k, v) =>
			  {
				assertFalse( _map.containsKey( k ) );
				_map.put( k, v );
				assertTrue( _map.containsKey( k ) );
				assertEquals( v, _map.get( k ) );
				assertEquals( v, _map.getOrThrow( k ) );
				assertEquals( v, _map.getIfAbsent( k, v * 2 ) );
				assertEquals( v, _map.getIfAbsentPut( k, v * 2 ) );
				assertEquals( v, _map.getIfAbsentPut( k, () => v * 2 ) );
			  });

			  assertEquals( m.size(), _map.size() );
			  assertTrue( m.Keys.allSatisfy( _map.containsKey ) );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<org.eclipse.collections.api.tuple.primitive.LongLongPair> toRemove = m.keyValuesView().select(p -> rnd.nextInt(100) < 75).toList().shuffleThis(rnd.random());
			  IList<LongLongPair> toRemove = m.keyValuesView().select(p => _rnd.Next(100) < 75).toList().shuffleThis(_rnd.random());

			  toRemove.ForEach(p =>
			  {
				long k = p.One;
				long v = p.Two;

				_map.updateValue( k, v + 1, x => -x );
				assertEquals( -v, _map.get( k ) );

				_map.remove( k );
				assertEquals( v * 2, _map.removeKeyIfAbsent( k, v * 2 ) );
				assertEquals( v * 2, _map.getIfAbsent( k, v * 2 ) );
				assertFalse( _map.containsKey( k ) );
				assertThrows( typeof( System.InvalidOperationException ), () => _map.getOrThrow(k) );

				_map.updateValue( k, v + 42, x => -x );
				assertEquals( -v - 42, _map.get( k ) );
			  });

			  toRemove.ForEach( p => _map.removeKey( p.One ) );

			  assertEquals( count - toRemove.Count, _map.size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nested class Collisions
		 internal class Collisions
		 {
			 internal bool InstanceFieldsInitialized = false;

			 internal virtual void InitializeInstanceFields()
			 {
				 CollisionsConflict = GenerateKeyCollisions( 5 );
				 A = CollisionsConflict.get( 0 );
				 B = CollisionsConflict.get( 1 );
				 C = CollisionsConflict.get( 2 );
				 D = CollisionsConflict.get( 3 );
				 E = CollisionsConflict.get( 4 );
			 }

			 private readonly LinearProbeLongLongHashMapTest _outerInstance;

			 public Collisions( LinearProbeLongLongHashMapTest outerInstance )
			 {
				 this._outerInstance = outerInstance;

				 if ( !InstanceFieldsInitialized )
				 {
					 InitializeInstanceFields();
					 InstanceFieldsInitialized = true;
				 }
			 }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  internal ImmutableLongList CollisionsConflict;
			  internal long A;
			  internal long B;
			  internal long C;
			  internal long D;
			  internal long E;

			  internal virtual ImmutableLongList GenerateKeyCollisions( int n )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long seed = rnd.nextLong();
					long seed = outerInstance.rnd.NextLong();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.list.primitive.MutableLongList elements;
					MutableLongList elements;
					using ( LinearProbeLongLongHashMap s = new LinearProbeLongLongHashMap( outerInstance.memoryAllocator ) )
					{
						 long v = s.HashAndMask( seed );
						 while ( s.HashAndMask( v ) != 0 || v == 0 || v == 1 )
						 {
							  ++v;
						 }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int h = s.hashAndMask(v);
						 int h = s.HashAndMask( v );
						 elements = LongLists.mutable.with( v );

						 while ( elements.size() < n )
						 {
							  ++v;
							  if ( s.HashAndMask( v ) == h )
							  {
									elements.add( v );
							  }
						 }
					}
					return elements.toImmutable();
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addAll()
			  internal virtual void AddAll()
			  {
					Fill( outerInstance.map, CollisionsConflict.toArray() );
					assertEquals( CollisionsConflict, outerInstance.map.toSortedList() );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addAllReversed()
			  internal virtual void AddAllReversed()
			  {
					Fill( outerInstance.map, CollisionsConflict.toReversed().toArray() );
					assertEquals( CollisionsConflict.toReversed(), outerInstance.map.toList() );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addAllRemoveLast()
			  internal virtual void AddAllRemoveLast()
			  {
					Fill( outerInstance.map, CollisionsConflict.toArray() );
					outerInstance.map.Remove( E );
					assertEquals( newListWith( A, B, C, D ), outerInstance.map.toList() );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addAllRemoveFirst()
			  internal virtual void AddAllRemoveFirst()
			  {
					Fill( outerInstance.map, CollisionsConflict.toArray() );
					outerInstance.map.Remove( A );
					assertEquals( newListWith( B, C, D, E ), outerInstance.map.toList() );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addAllRemoveMiddle()
			  internal virtual void AddAllRemoveMiddle()
			  {
					Fill( outerInstance.map, CollisionsConflict.toArray() );
					outerInstance.map.Remove( B );
					outerInstance.map.Remove( D );
					assertEquals( newListWith( A, C, E ), outerInstance.map.toList() );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addAllRemoveMiddle2()
			  internal virtual void AddAllRemoveMiddle2()
			  {
					Fill( outerInstance.map, CollisionsConflict.toArray() );
					outerInstance.map.Remove( A );
					outerInstance.map.Remove( C );
					outerInstance.map.Remove( E );
					assertEquals( newListWith( B, D ), outerInstance.map.toList() );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addReusesRemovedHead()
			  internal virtual void AddReusesRemovedHead()
			  {
					Fill( outerInstance.map, A, B, C );

					outerInstance.map.Remove( A );
					assertEquals( newListWith( B, C ), outerInstance.map.toList() );

					outerInstance.map.Put( D, 42 );
					assertEquals( newListWith( D, B, C ), outerInstance.map.toList() );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addReusesRemovedTail()
			  internal virtual void AddReusesRemovedTail()
			  {
					Fill( outerInstance.map, A, B, C );

					outerInstance.map.Remove( C );
					assertEquals( newListWith( A, B ), outerInstance.map.toList() );

					outerInstance.map.Put( D, 42 );
					assertEquals( newListWith( A, B, D ), outerInstance.map.toList() );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addReusesRemovedMiddle()
			  internal virtual void AddReusesRemovedMiddle()
			  {
					Fill( outerInstance.map, A, B, C );

					outerInstance.map.Remove( B );
					assertEquals( newListWith( A, C ), outerInstance.map.toList() );

					outerInstance.map.Put( D, 42 );
					assertEquals( newListWith( A, D, C ), outerInstance.map.toList() );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addReusesRemovedMiddle2()
			  internal virtual void AddReusesRemovedMiddle2()
			  {
					Fill( outerInstance.map, A, B, C, D, E );

					outerInstance.map.Remove( B );
					outerInstance.map.Remove( C );
					assertEquals( newListWith( A, D, E ), outerInstance.map.toList() );

					outerInstance.map.Put( C, 1 );
					outerInstance.map.Put( B, 2 );
					assertEquals( newListWith( A, C, B, D, E ), outerInstance.map.toList() );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void rehashingCompactsSparseSentinels()
			  internal virtual void RehashingCompactsSparseSentinels()
			  {
					Fill( outerInstance.map, A, B, C, D, E );

					outerInstance.map.Remove( B );
					outerInstance.map.Remove( D );
					outerInstance.map.Remove( E );
					assertEquals( newListWith( A, C ), outerInstance.map.toList() );

					Fill( outerInstance.map, B, D, E );
					assertEquals( newListWith( A, B, C, D, E ), outerInstance.map.toList() );

					outerInstance.map.Remove( B );
					outerInstance.map.Remove( D );
					outerInstance.map.Remove( E );
					assertEquals( newListWith( A, C ), outerInstance.map.toList() );

					outerInstance.map.RehashWithoutGrow();
					Fill( outerInstance.map, E, D, B );
					assertEquals( newListWith( A, C, E, D, B ), outerInstance.map.toList() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nested class IterationConcurrentModification
		 internal class IterationConcurrentModification
		 {
			 private readonly LinearProbeLongLongHashMapTest _outerInstance;

			 public IterationConcurrentModification( LinearProbeLongLongHashMapTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void put()
			  internal virtual void Put()
			  {
					TestIteratorsFail( m => m.put( 0, 0 ), pair( 0L, 10L ), pair( 1L, 11L ), pair( 2L, 12L ), pair( 3L, 13L ) );
					TestIteratorsFail( m => m.put( 1, 1 ), pair( 0L, 10L ), pair( 1L, 11L ), pair( 2L, 12L ), pair( 3L, 13L ) );
					TestIteratorsFail( m => m.put( 0, 0 ), pair( 1L, 11L ), pair( 2L, 12L ), pair( 3L, 13L ) );
					TestIteratorsFail( m => m.put( 1, 1 ), pair( 0L, 10L ), pair( 2L, 12L ), pair( 3L, 13L ) );
					TestIteratorsFail( m => m.put( 2, 2 ), pair( 0L, 10L ), pair( 1L, 11L ), pair( 2L, 12L ), pair( 3L, 13L ) );
					TestIteratorsFail( m => m.put( 4, 14 ), pair( 0L, 10L ), pair( 1L, 11L ), pair( 2L, 12L ), pair( 3L, 13L ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getIfAbsentPut_put()
			  internal virtual void getIfAbsentPut_put()
			  {
					TestIteratorsFail( m => m.getIfAbsentPut( 0, 0 ), pair( 1L, 11L ), pair( 2L, 12L ), pair( 3L, 13L ) );
					TestIteratorsFail( m => m.getIfAbsentPut( 1, 1 ), pair( 0L, 10L ), pair( 2L, 12L ), pair( 3L, 13L ) );
					TestIteratorsFail( m => m.getIfAbsentPut( 4, 4 ), pair( 0L, 10L ), pair( 1L, 11L ), pair( 2L, 12L ), pair( 3L, 13L ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getIfAbsentPut_onlyGetNoPut()
			  internal virtual void getIfAbsentPut_onlyGetNoPut()
			  {
					Fill( outerInstance.map, 0L, 1L, 2L, 3L );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.iterator.MutableLongIterator keyIter = map.longIterator();
					MutableLongIterator keyIter = outerInstance.map.LongIterator();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Iterator<org.eclipse.collections.api.tuple.primitive.LongLongPair> keyValueIter = map.keyValuesView().iterator();
					IEnumerator<LongLongPair> keyValueIter = outerInstance.map.KeyValuesView().GetEnumerator();

					outerInstance.map.getIfAbsentPut( 0, 0 );
					outerInstance.map.getIfAbsentPut( 1, 1 );
					outerInstance.map.getIfAbsentPut( 2, 2 );

					assertDoesNotThrow( keyIter.hasNext );
					assertDoesNotThrow( keyIter.next );
					assertDoesNotThrow( keyValueIter.hasNext );
					assertDoesNotThrow( keyValueIter.next );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void remove()
			  internal virtual void Remove()
			  {
					TestIteratorsFail( m => m.remove( 0 ), pair( 0L, 10L ), pair( 1L, 11L ), pair( 2L, 12L ), pair( 3L, 13L ) );
					TestIteratorsFail( m => m.remove( 1 ), pair( 0L, 10L ), pair( 1L, 11L ), pair( 2L, 12L ), pair( 3L, 13L ) );
					TestIteratorsFail( m => m.remove( 0 ), pair( 1L, 11L ), pair( 2L, 12L ), pair( 3L, 13L ) );
					TestIteratorsFail( m => m.remove( 1 ), pair( 0L, 10L ), pair( 2L, 12L ), pair( 3L, 13L ) );
					TestIteratorsFail( m => m.remove( 2 ), pair( 0L, 10L ), pair( 1L, 11L ), pair( 2L, 12L ), pair( 3L, 13L ) );
					TestIteratorsFail( m => m.remove( 4 ), pair( 0L, 10L ), pair( 1L, 11L ), pair( 2L, 12L ), pair( 3L, 13L ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putAll()
			  internal virtual void PutAll()
			  {
					TestIteratorsFail( m => m.putAll( newWithKeysValues( 0, 0 ) ), pair( 0L, 10L ), pair( 1L, 11L ), pair( 2L, 12L ), pair( 3L, 13L ) );
					TestIteratorsFail( m => m.putAll( newWithKeysValues( 4, 4 ) ), pair( 0L, 10L ), pair( 1L, 11L ), pair( 2L, 12L ), pair( 3L, 13L ) );
					TestIteratorsFail( m => m.putAll( LongLongMaps.immutable.empty() ), pair(0L, 10L), pair(1L, 11L), pair(2L, 12L), pair(3L, 13L) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void updateValue()
			  internal virtual void UpdateValue()
			  {
					TestIteratorsFail( m => m.updateValue( 0, 0, x => x * 2 ), pair( 0L, 10L ), pair( 1L, 11L ), pair( 2L, 12L ), pair( 3L, 13L ) );
					TestIteratorsFail( m => m.updateValue( 2, 2, x => x * 2 ), pair( 0L, 10L ), pair( 1L, 11L ), pair( 2L, 12L ), pair( 3L, 13L ) );
					TestIteratorsFail( m => m.updateValue( 4, 4, x => x * 2 ), pair( 0L, 10L ), pair( 1L, 11L ), pair( 2L, 12L ), pair( 3L, 13L ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void close()
			  internal virtual void Close()
			  {
					TestIteratorsFail( LinearProbeLongLongHashMap.close, pair( 0L, 10L ), pair( 2L, 12L ) );
			  }

			  internal virtual void TestIteratorsFail( System.Action<LinearProbeLongLongHashMap> mutator, params LongLongPair[] initialValues )
			  {
					outerInstance.map.Clear();
					foreach ( LongLongPair pair in initialValues )
					{
						 outerInstance.map.putPair( pair );
					}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.iterator.MutableLongIterator keysIterator = map.longIterator();
					MutableLongIterator keysIterator = outerInstance.map.LongIterator();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Iterator<org.eclipse.collections.api.tuple.primitive.LongLongPair> keyValueIterator = map.keyValuesView().iterator();
					IEnumerator<LongLongPair> keyValueIterator = outerInstance.map.KeyValuesView().GetEnumerator();

					assertTrue( keysIterator.hasNext() );
					assertDoesNotThrow( keysIterator.next );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( keyValueIterator.hasNext() );
					assertDoesNotThrow( keyValueIterator.next );

					mutator( outerInstance.map );

					assertThrows( typeof( ConcurrentModificationException ), keysIterator.hasNext );
					assertThrows( typeof( ConcurrentModificationException ), keysIterator.next );
					assertThrows( typeof( ConcurrentModificationException ), keyValueIterator.hasNext );
					assertThrows( typeof( ConcurrentModificationException ), keyValueIterator.next );
			  }
		 }

		 private static void Fill( MutableLongLongMap m, params long[] keys )
		 {
			  foreach ( long key in keys )
			  {
					m.put( key, System.nanoTime() );
			  }
		 }

		 private static LongLongMap ToMap( params long[] keys )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.map.primitive.MutableLongLongMap m = new org.eclipse.collections.impl.map.mutable.primitive.LongLongHashMap();
			  MutableLongLongMap m = new LongLongHashMap();
			  Fill( m, keys );
			  return m;
		 }
	}

}