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
	using Function = org.eclipse.collections.api.block.function.Function;
	using Function0 = org.eclipse.collections.api.block.function.Function0;
	using LongToObjectFunction = org.eclipse.collections.api.block.function.primitive.LongToObjectFunction;
	using Procedure = org.eclipse.collections.api.block.procedure.Procedure;
	using LongObjectProcedure = org.eclipse.collections.api.block.procedure.primitive.LongObjectProcedure;
	using LongProcedure = org.eclipse.collections.api.block.procedure.primitive.LongProcedure;
	using MutableLongObjectMap = org.eclipse.collections.api.map.primitive.MutableLongObjectMap;
	using LongLongHashMap = org.eclipse.collections.impl.map.mutable.primitive.LongLongHashMap;
	using LongObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.LongObjectHashMap;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using Inject = Neo4Net.Test.extension.Inject;
	using RandomExtension = Neo4Net.Test.extension.RandomExtension;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doReturn;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.@internal.verification.VerificationModeFactory.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.intValue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(RandomExtension.class) class ValuesMapTest
	internal class ValuesMapTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.RandomRule rnd;
		 private RandomRule _rnd;

		 private readonly ValuesMap _map = NewMap();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putGet()
		 internal virtual void PutGet()
		 {
			  _map.put( 0, intValue( 10 ) );
			  _map.put( 1, intValue( 11 ) );
			  _map.put( 2, intValue( 12 ) );

			  assertEquals( intValue( 10 ), _map.get( 0 ) );
			  assertEquals( intValue( 11 ), _map.get( 1 ) );
			  assertEquals( intValue( 12 ), _map.get( 2 ) );
			  // default empty value
			  assertNull( _map.get( 3 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putAll()
		 internal virtual void PutAll()
		 {
			  _map.putAll( LongObjectHashMap.newWithKeysValues( 0, intValue( 10 ), 1, intValue( 11 ), 2, intValue( 12 ) ) );
			  assertEquals( 3, _map.size() );
			  assertEquals( intValue( 10 ), _map.get( 0 ) );
			  assertEquals( intValue( 11 ), _map.get( 1 ) );
			  assertEquals( intValue( 12 ), _map.get( 2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getIfAbsent()
		 internal virtual void getIfAbsent()
		 {
			  assertEquals( intValue( -1 ), _map.getIfAbsent( 0, () => intValue(-1) ) );
			  assertEquals( intValue( -1 ), _map.getIfAbsent( 1, () => intValue(-1) ) );
			  assertEquals( intValue( -1 ), _map.getIfAbsent( 2, () => intValue(-1) ) );
			  assertEquals( intValue( -1 ), _map.getIfAbsent( 3, () => intValue(-1) ) );

			  _map.putAll( LongObjectHashMap.newWithKeysValues( 0, intValue( 10 ), 1, intValue( 11 ), 2, intValue( 12 ) ) );

			  assertEquals( intValue( 10 ), _map.getIfAbsent( 0, () => intValue(-1) ) );
			  assertEquals( intValue( 11 ), _map.getIfAbsent( 1, () => intValue(-1) ) );
			  assertEquals( intValue( 12 ), _map.getIfAbsent( 2, () => intValue(-1) ) );
			  assertEquals( intValue( -1 ), _map.getIfAbsent( 3, () => intValue(-1) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getIfAbsentPut()
		 internal virtual void getIfAbsentPut()
		 {
			  assertEquals( intValue( 10 ), _map.getIfAbsentPut( 0, intValue( 10 ) ) );
			  assertEquals( intValue( 10 ), _map.getIfAbsentPut( 0, intValue( 100 ) ) );
			  assertEquals( intValue( 11 ), _map.getIfAbsentPut( 1, intValue( 11 ) ) );
			  assertEquals( intValue( 11 ), _map.getIfAbsentPut( 1, intValue( 110 ) ) );
			  assertEquals( intValue( 12 ), _map.getIfAbsentPut( 2, intValue( 12 ) ) );
			  assertEquals( intValue( 12 ), _map.getIfAbsentPut( 2, intValue( 120 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getIfAbsentPut_Supplier()
		 internal virtual void getIfAbsentPut_Supplier()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.block.function.Function0<org.neo4j.values.storable.Value> supplier = mock(org.eclipse.collections.api.block.function.Function0.class);
			  Function0<Value> supplier = mock( typeof( Function0 ) );
			  doReturn( intValue( 10 ), intValue( 11 ), intValue( 12 ) ).when( supplier ).value();

			  assertEquals( intValue( 10 ), _map.getIfAbsentPut( 0, supplier ) );
			  assertEquals( intValue( 11 ), _map.getIfAbsentPut( 1, supplier ) );
			  assertEquals( intValue( 12 ), _map.getIfAbsentPut( 2, supplier ) );
			  verify( supplier, times( 3 ) ).value();

			  assertEquals( intValue( 10 ), _map.getIfAbsentPut( 0, supplier ) );
			  assertEquals( intValue( 11 ), _map.getIfAbsentPut( 1, supplier ) );
			  assertEquals( intValue( 12 ), _map.getIfAbsentPut( 2, supplier ) );
			  verifyNoMoreInteractions( supplier );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getIfAbsentPutWithKey()
		 internal virtual void getIfAbsentPutWithKey()
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("Convert2Lambda") final org.eclipse.collections.api.block.function.primitive.LongToObjectFunction<org.neo4j.values.storable.Value> function = spy(new org.eclipse.collections.api.block.function.primitive.LongToObjectFunction<org.neo4j.values.storable.Value>()
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
			  LongToObjectFunction<Value> function = spy( new LongToObjectFunctionAnonymousInnerClass( this ) );

			  assertEquals( intValue( 10 ), _map.getIfAbsentPutWithKey( 0, function ) );
			  assertEquals( intValue( 10 ), _map.getIfAbsentPutWithKey( 0, function ) );
			  assertEquals( intValue( 11 ), _map.getIfAbsentPutWithKey( 1, function ) );
			  assertEquals( intValue( 11 ), _map.getIfAbsentPutWithKey( 1, function ) );
			  assertEquals( intValue( 12 ), _map.getIfAbsentPutWithKey( 2, function ) );
			  assertEquals( intValue( 12 ), _map.getIfAbsentPutWithKey( 2, function ) );

			  verify( function ).valueOf( eq( 0L ) );
			  verify( function ).valueOf( eq( 1L ) );
			  verify( function ).valueOf( eq( 2L ) );
			  verifyNoMoreInteractions( function );
		 }

		 private class LongToObjectFunctionAnonymousInnerClass : LongToObjectFunction<Value>
		 {
			 private readonly ValuesMapTest _outerInstance;

			 public LongToObjectFunctionAnonymousInnerClass( ValuesMapTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override Value valueOf( long x )
			 {
				  return intValue( 10 + ( int ) x );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void getIfAbsentPutWith()
		 internal virtual void getIfAbsentPutWith()
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({"Convert2Lambda", "Anonymous2MethodRef"}) final org.eclipse.collections.api.block.function.Function<String, org.neo4j.values.storable.Value> function = spy(new org.eclipse.collections.api.block.function.Function<String, org.neo4j.values.storable.Value>()
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
			  Function<string, Value> function = spy( new FunctionAnonymousInnerClass( this ) );

			  assertEquals( intValue( 10 ), _map.getIfAbsentPutWith( 0, function, "10" ) );
			  assertEquals( intValue( 10 ), _map.getIfAbsentPutWith( 0, function, "10" ) );
			  assertEquals( intValue( 11 ), _map.getIfAbsentPutWith( 1, function, "11" ) );
			  assertEquals( intValue( 11 ), _map.getIfAbsentPutWith( 1, function, "11" ) );
			  assertEquals( intValue( 12 ), _map.getIfAbsentPutWith( 2, function, "12" ) );
			  assertEquals( intValue( 12 ), _map.getIfAbsentPutWith( 2, function, "12" ) );

			  verify( function ).valueOf( eq( "10" ) );
			  verify( function ).valueOf( eq( "11" ) );
			  verify( function ).valueOf( eq( "12" ) );
			  verifyNoMoreInteractions( function );
		 }

		 private class FunctionAnonymousInnerClass : Function<string, Value>
		 {
			 private readonly ValuesMapTest _outerInstance;

			 public FunctionAnonymousInnerClass( ValuesMapTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override Value valueOf( string s )
			 {
				  return intValue( Convert.ToInt32( s ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void putOverwrite()
		 internal virtual void PutOverwrite()
		 {
			  _map.putAll( LongObjectHashMap.newWithKeysValues( 0, intValue( 10 ), 1, intValue( 11 ), 2, intValue( 12 ) ) );

			  assertEquals( intValue( 10 ), _map.get( 0 ) );
			  assertEquals( intValue( 11 ), _map.get( 1 ) );
			  assertEquals( intValue( 12 ), _map.get( 2 ) );

			  _map.putAll( LongObjectHashMap.newWithKeysValues( 0, intValue( 20 ), 1, intValue( 21 ), 2, intValue( 22 ) ) );

			  assertEquals( intValue( 20 ), _map.get( 0 ) );
			  assertEquals( intValue( 21 ), _map.get( 1 ) );
			  assertEquals( intValue( 22 ), _map.get( 2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void size()
		 internal virtual void Size()
		 {
			  assertEquals( 0, _map.size() );
			  _map.put( 0, intValue( 10 ) );
			  assertEquals( 1, _map.size() );
			  _map.put( 1, intValue( 11 ) );
			  assertEquals( 2, _map.size() );
			  _map.put( 2, intValue( 12 ) );
			  assertEquals( 3, _map.size() );
			  _map.put( 0, intValue( 20 ) );
			  _map.put( 1, intValue( 20 ) );
			  _map.put( 2, intValue( 20 ) );
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

			  _map.put( 0, intValue( 10 ) );
			  assertTrue( _map.containsKey( 0 ) );

			  _map.put( 1, intValue( 11 ) );
			  assertTrue( _map.containsKey( 1 ) );

			  _map.put( 2, intValue( 12 ) );
			  assertTrue( _map.containsKey( 2 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void forEachKey()
		 internal virtual void ForEachKey()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.block.procedure.primitive.LongProcedure consumer = mock(org.eclipse.collections.api.block.procedure.primitive.LongProcedure.class);
			  LongProcedure consumer = mock( typeof( LongProcedure ) );
			  _map.putAll( LongObjectHashMap.newWithKeysValues( 0, intValue( 10 ), 1, intValue( 11 ), 2, intValue( 12 ) ) );

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
//ORIGINAL LINE: final org.eclipse.collections.api.block.procedure.Procedure<org.neo4j.values.storable.Value> consumer = mock(org.eclipse.collections.api.block.procedure.Procedure.class);
			  Procedure<Value> consumer = mock( typeof( Procedure ) );
			  _map.putAll( LongObjectHashMap.newWithKeysValues( 0, intValue( 10 ), 1, intValue( 11 ), 2, intValue( 12 ) ) );

			  _map.forEachValue( consumer );

			  verify( consumer ).value( eq( intValue( 10 ) ) );
			  verify( consumer ).value( eq( intValue( 11 ) ) );
			  verify( consumer ).value( eq( intValue( 12 ) ) );
			  verifyNoMoreInteractions( consumer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void forEachKeyValue()
		 internal virtual void ForEachKeyValue()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.block.procedure.primitive.LongObjectProcedure<org.neo4j.values.storable.Value> consumer = mock(org.eclipse.collections.api.block.procedure.primitive.LongObjectProcedure.class);
			  LongObjectProcedure<Value> consumer = mock( typeof( LongObjectProcedure ) );
			  _map.putAll( LongObjectHashMap.newWithKeysValues( 0, intValue( 10 ), 1, intValue( 11 ), 2, intValue( 12 ) ) );

			  _map.forEachKeyValue( consumer );

			  verify( consumer ).value( eq( 0L ), eq( intValue( 10 ) ) );
			  verify( consumer ).value( eq( 1L ), eq( intValue( 11 ) ) );
			  verify( consumer ).value( eq( 2L ), eq( intValue( 12 ) ) );
			  verifyNoMoreInteractions( consumer );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void clear()
		 internal virtual void Clear()
		 {
			  _map.clear();
			  assertEquals( 0, _map.size() );

			  _map.putAll( LongObjectHashMap.newWithKeysValues( 0, intValue( 10 ), 1, intValue( 11 ), 2, intValue( 12 ) ) );

			  assertEquals( 3, _map.size() );

			  _map.clear();
			  assertEquals( 0, _map.size() );

			  _map.clear();
			  assertEquals( 0, _map.size() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void randomizedWithSharedValuesContainer()
		 internal virtual void RandomizedWithSharedValuesContainer()
		 {
			  const int maps = 13;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int COUNT = 10000 + rnd.nextInt(1000);
			  int count = 10000 + _rnd.Next( 1000 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final AppendOnlyValuesContainer valuesContainer = new AppendOnlyValuesContainer(new TestMemoryAllocator());
			  AppendOnlyValuesContainer valuesContainer = new AppendOnlyValuesContainer( new TestMemoryAllocator() );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<ValuesMap> actualMaps = new java.util.ArrayList<>();
			  IList<ValuesMap> actualMaps = new List<ValuesMap>();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<org.eclipse.collections.api.map.primitive.MutableLongObjectMap<org.neo4j.values.storable.Value>> expectedMaps = new java.util.ArrayList<>();
			  IList<MutableLongObjectMap<Value>> expectedMaps = new List<MutableLongObjectMap<Value>>();

			  for ( int i = 0; i < maps; i++ )
			  {
					actualMaps.Add( NewMap( valuesContainer ) );
					expectedMaps.Add( new LongObjectHashMap<>() );
			  }

			  for ( int i = 0; i < maps; i++ )
			  {
					Put( count, actualMaps[i], expectedMaps[i] );
			  }

			  for ( int i = 0; i < maps; i++ )
			  {
					Remove( count, actualMaps[i], expectedMaps[i] );
			  }

			  for ( int i = 0; i < maps; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.map.primitive.MutableLongObjectMap<org.neo4j.values.storable.Value> expected = expectedMaps.get(i);
					MutableLongObjectMap<Value> expected = expectedMaps[i];
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ValuesMap actual = actualMaps.get(i);
					ValuesMap actual = actualMaps[i];
					expected.forEachKeyValue( ( k, v ) => assertEquals( v, actual.Get( k ) ) );
			  }
		 }

		 private void Remove( int count, ValuesMap actualMap, MutableLongObjectMap<Value> expectedMap )
		 {
			  for ( int i = 0; i < count / 2; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long key = rnd.nextLong(count);
					long key = _rnd.nextLong( count );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.values.storable.Value value = rnd.randomValues().nextValue();
					Value value = _rnd.randomValues().nextValue();
					actualMap.Put( key, value );
					expectedMap.put( key, value );
			  }
		 }

		 private void Put( int count, ValuesMap actualMap, MutableLongObjectMap<Value> expectedMap )
		 {
			  for ( int i = 0; i < count * 2; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long key = rnd.nextLong(count);
					long key = _rnd.nextLong( count );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.values.storable.Value value = rnd.randomValues().nextValue();
					Value value = _rnd.randomValues().nextValue();
					actualMap.Put( key, value );
					expectedMap.put( key, value );
			  }
		 }

		 private static ValuesMap NewMap()
		 {
			  return NewMap( new AppendOnlyValuesContainer( new TestMemoryAllocator() ) );
		 }

		 private static ValuesMap NewMap( AppendOnlyValuesContainer valuesContainer )
		 {
			  return new ValuesMap( new LongLongHashMap(), valuesContainer );
		 }
	}

}