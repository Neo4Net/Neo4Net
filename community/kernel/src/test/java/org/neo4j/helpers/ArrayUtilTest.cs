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
namespace Org.Neo4j.Helpers
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;

	internal class ArrayUtilTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProduceUnionOfTwoArrays()
		 internal virtual void ShouldProduceUnionOfTwoArrays()
		 {
			  // GIVEN
			  string[] first = new string[] { "one", "three" };
			  string[] other = new string[] { "two", "four", "five" };

			  // WHEN
			  string[] union = ArrayUtil.Union( first, other );

			  // THEN
			  assertEquals( asSet( "one", "two", "three", "four", "five" ), asSet( union ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProduceUnionWhereFirstIsNull()
		 internal virtual void ShouldProduceUnionWhereFirstIsNull()
		 {
			  // GIVEN
			  string[] first = null;
			  string[] other = new string[] { "one", "two" };

			  // WHEN
			  string[] union = ArrayUtil.Union( first, other );

			  // THEN
			  assertEquals( asSet( "one", "two" ), asSet( union ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldProduceUnionWhereOtherIsNull()
		 internal virtual void ShouldProduceUnionWhereOtherIsNull()
		 {
			  // GIVEN
			  string[] first = new string[] { "one", "two" };
			  string[] other = null;

			  // WHEN
			  string[] union = ArrayUtil.Union( first, other );

			  // THEN
			  assertEquals( asSet( "one", "two" ), asSet( union ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCheckNullSafeEqual()
		 internal virtual void ShouldCheckNullSafeEqual()
		 {
			  // WHEN/THEN
			  assertTrue( ArrayUtil.NullSafeEquals( null, null ) );
			  assertFalse( ArrayUtil.NullSafeEquals( "1", null ) );
			  assertFalse( ArrayUtil.NullSafeEquals( null, "1" ) );
			  assertTrue( ArrayUtil.NullSafeEquals( "1", "1" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void emptyArray()
		 internal virtual void EmptyArray()
		 {
			  assertTrue( ArrayUtil.IsEmpty( null ) );
			  assertTrue( ArrayUtil.IsEmpty( new string[] {} ) );
			  assertFalse( ArrayUtil.IsEmpty( new long?[] { 1L } ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldConcatOneAndMany()
		 internal virtual void ShouldConcatOneAndMany()
		 {
			  // WHEN
			  int?[] result = ArrayUtil.Concat( 0, 1, 2, 3, 4 );

			  // THEN
			  for ( int i = 0; i < 5; i++ )
			  {
					assertEquals( ( int? )i, result[i] );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldConcatSeveralArrays()
		 internal virtual void ShouldConcatSeveralArrays()
		 {
			  // GIVEN
			  int?[] a = new int?[] { 0, 1, 2 };
			  int?[] b = new int?[] { 3, 4 };
			  int?[] c = new int?[] { 5, 6, 7, 8 };

			  // WHEN
			  int?[] result = ArrayUtil.ConcatArrays( a, b, c );

			  // THEN
			  assertEquals( a.Length + b.Length + c.Length, result.Length );

			  for ( int i = 0; i < result.Length; i++ )
			  {
					assertEquals( ( int? ) i, result[i] );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFindIndexOf()
		 internal virtual void ShouldFindIndexOf()
		 {
			  // GIVEN
			  int?[] numbers = ArrayUtil.Concat( 0, 1, 2, 3, 4, 5 );

			  // WHEN/THEN
			  for ( int i = 0; i < 6; i++ )
			  {
					assertEquals( i, ArrayUtil.IndexOf( numbers, i ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFindLastOf()
		 internal virtual void ShouldFindLastOf()
		 {
			  // GIVEN
			  int?[] numbers = new int?[]{ 0, 100, 4, 5, 6, 3 };

			  // WHEN/THEN
			  assertEquals( 3, ( int ) ArrayUtil.LastOf( numbers ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRemoveItems()
		 internal virtual void ShouldRemoveItems()
		 {
			  // GIVEN
			  int?[] numbers = ArrayUtil.Concat( 0, 1, 2, 3, 4, 5 );

			  // WHEN
			  int?[] trimmed = ArrayUtil.Without( numbers, 2 );
			  trimmed = ArrayUtil.Without( trimmed, 5 );
			  trimmed = ArrayUtil.Without( trimmed, 0 );

			  // THEN
			  assertEquals( 3, trimmed.Length );
			  assertFalse( ArrayUtil.Contains( trimmed, 0 ) );
			  assertTrue( ArrayUtil.Contains( trimmed, 1 ) );
			  assertFalse( ArrayUtil.Contains( trimmed, 2 ) );
			  assertTrue( ArrayUtil.Contains( trimmed, 3 ) );
			  assertTrue( ArrayUtil.Contains( trimmed, 4 ) );
			  assertFalse( ArrayUtil.Contains( trimmed, 5 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldConcatArrays()
		 internal virtual void ShouldConcatArrays()
		 {
			  // GIVEN
			  int?[] initial = new int?[] { 0, 1, 2 };

			  // WHEN
			  int?[] all = ArrayUtil.Concat( initial, 3, 4, 5 );

			  // THEN
			  assertArrayEquals( new int?[] { 0, 1, 2, 3, 4, 5 }, all );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReverseEvenCount()
		 internal virtual void ShouldReverseEvenCount()
		 {
			  // given
			  int?[] array = new int?[] { 0, 1, 2, 3, 4, 5 };

			  // when
			  ArrayUtil.Reverse( array );

			  // then
			  assertArrayEquals( new int?[] { 5, 4, 3, 2, 1, 0 }, array );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReverseUnevenCount()
		 internal virtual void ShouldReverseUnevenCount()
		 {
			  // given
			  int?[] array = new int?[] { 0, 1, 2, 3, 4 };

			  // when
			  ArrayUtil.Reverse( array );

			  // then
			  assertArrayEquals( new int?[] { 4, 3, 2, 1, 0 }, array );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReverseEmptyArray()
		 internal virtual void ShouldReverseEmptyArray()
		 {
			  // given
			  int?[] array = new int?[] {};

			  // when
			  ArrayUtil.Reverse( array );

			  // then
			  assertArrayEquals( new int?[] {}, array );
		 }
	}

}