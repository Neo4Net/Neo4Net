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
namespace Neo4Net.Collection.primitive
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;

	internal class PrimitiveLongIntKeyValueArrayTest
	{
		 private const int DEFAULT_VALUE = -1;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testEnsureCapacity()
		 internal virtual void TestEnsureCapacity()
		 {
			  PrimitiveLongIntKeyValueArray map = new PrimitiveLongIntKeyValueArray();
			  assertThat( map.Capacity(), equalTo(PrimitiveLongIntKeyValueArray.DEFAULT_INITIAL_CAPACITY) );

			  map.EnsureCapacity( 10 );
			  assertThat( map.Capacity(), greaterThanOrEqualTo(10) );

			  map.EnsureCapacity( 100 );
			  assertThat( map.Capacity(), greaterThanOrEqualTo(100) );

			  map.EnsureCapacity( 1000 );
			  assertThat( map.Capacity(), greaterThanOrEqualTo(1000) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testSize()
		 internal virtual void TestSize()
		 {
			  PrimitiveLongIntKeyValueArray map = new PrimitiveLongIntKeyValueArray();
			  assertThat( map.Size(), equalTo(0) );

			  map.PutIfAbsent( 1, 100 );
			  map.PutIfAbsent( 2, 200 );
			  map.PutIfAbsent( 3, 300 );
			  assertThat( map.Size(), equalTo(3) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testGetOrDefault()
		 internal virtual void TestGetOrDefault()
		 {
			  PrimitiveLongIntKeyValueArray map = new PrimitiveLongIntKeyValueArray();
			  map.PutIfAbsent( 1, 100 );
			  map.PutIfAbsent( 2, 200 );
			  map.PutIfAbsent( 3, 300 );

			  assertThat( map.GetOrDefault( 1, DEFAULT_VALUE ), equalTo( 100 ) );
			  assertThat( map.GetOrDefault( 2, DEFAULT_VALUE ), equalTo( 200 ) );
			  assertThat( map.GetOrDefault( 3, DEFAULT_VALUE ), equalTo( 300 ) );
			  assertThat( map.GetOrDefault( 4, DEFAULT_VALUE ), equalTo( DEFAULT_VALUE ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testPutIfAbsent()
		 internal virtual void TestPutIfAbsent()
		 {
			  PrimitiveLongIntKeyValueArray map = new PrimitiveLongIntKeyValueArray();

			  assertThat( map.PutIfAbsent( 1, 100 ), equalTo( true ) );
			  assertThat( map.PutIfAbsent( 2, 200 ), equalTo( true ) );
			  assertThat( map.PutIfAbsent( 3, 300 ), equalTo( true ) );
			  assertThat( map.Size(), equalTo(3) );
			  assertThat( map.Keys(), equalTo(new long[]{ 1, 2, 3 }) );

			  assertThat( map.PutIfAbsent( 2, 2000 ), equalTo( false ) );
			  assertThat( map.PutIfAbsent( 3, 3000 ), equalTo( false ) );
			  assertThat( map.PutIfAbsent( 4, 4000 ), equalTo( true ) );
			  assertThat( map.Size(), equalTo(4) );
			  assertThat( map.Keys(), equalTo(new long[]{ 1, 2, 3, 4 }) );
			  assertThat( map.GetOrDefault( 2, DEFAULT_VALUE ), equalTo( 200 ) );
			  assertThat( map.GetOrDefault( 3, DEFAULT_VALUE ), equalTo( 300 ) );
			  assertThat( map.GetOrDefault( 4, DEFAULT_VALUE ), equalTo( 4000 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testReset()
		 internal virtual void TestReset()
		 {
			  PrimitiveLongIntKeyValueArray map = new PrimitiveLongIntKeyValueArray();
			  map.PutIfAbsent( 1, 100 );
			  map.PutIfAbsent( 2, 200 );
			  map.PutIfAbsent( 3, 300 );

			  map.Reset( 1000 );
			  assertThat( map.Size(), equalTo(0) );
			  assertThat( map.Capacity(), greaterThanOrEqualTo(1000) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testKeys()
		 internal virtual void TestKeys()
		 {
			  PrimitiveLongIntKeyValueArray map = new PrimitiveLongIntKeyValueArray();
			  map.PutIfAbsent( 1, 100 );
			  map.PutIfAbsent( 2, 200 );
			  map.PutIfAbsent( 3, 300 );
			  map.PutIfAbsent( 2, 200 );
			  map.PutIfAbsent( 3, 300 );
			  map.PutIfAbsent( 8, 800 );
			  map.PutIfAbsent( 7, 700 );
			  map.PutIfAbsent( 6, 600 );
			  map.PutIfAbsent( 5, 500 );

			  assertThat( map.Size(), equalTo(7) );
			  assertThat( map.Keys(), equalTo(new long[]{ 1, 2, 3, 8, 7, 6, 5 }) );
		 }
	}

}