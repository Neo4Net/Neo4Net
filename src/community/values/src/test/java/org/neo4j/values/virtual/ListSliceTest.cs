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
namespace Neo4Net.Values.@virtual
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.iteratorsEqual;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.@virtual.VirtualValues.EMPTY_LIST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.@virtual.VirtualValues.list;

	internal class ListSliceTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSliceList()
		 internal virtual void ShouldSliceList()
		 {
			  // Given
			  ListValue inner = list( longValue( 5L ), longValue( 6L ), longValue( 7L ), longValue( 8L ), longValue( 9L ), longValue( 10L ), longValue( 11L ) );

			  // When
			  ListValue slice = inner.Slice( 2, 4 );

			  // Then
			  ListValue expected = list( longValue( 7L ), longValue( 8L ) );
			  assertEquals( expected, slice );
			  assertEquals( expected.GetHashCode(), slice.GetHashCode() );
			  assertArrayEquals( expected.AsArray(), slice.AsArray() );
			  assertTrue( iteratorsEqual( expected.GetEnumerator(), slice.GetEnumerator() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnEmptyListIfEmptyRange()
		 internal virtual void ShouldReturnEmptyListIfEmptyRange()
		 {
			  // Given
			  ListValue inner = list( longValue( 5L ), longValue( 6L ), longValue( 7L ), longValue( 8L ), longValue( 9L ), longValue( 10L ), longValue( 11L ) );

			  // When
			  ListValue slice = inner.Slice( 4, 2 );

			  // Then
			  assertEquals( slice, EMPTY_LIST );
			  assertTrue( iteratorsEqual( slice.GetEnumerator(), EMPTY_LIST.GetEnumerator() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleExceedingRange()
		 internal virtual void ShouldHandleExceedingRange()
		 {
			  // Given
			  ListValue inner = list( longValue( 5L ), longValue( 6L ), longValue( 7L ), longValue( 8L ), longValue( 9L ), longValue( 10L ), longValue( 11L ) );

			  // When
			  ListValue slice = inner.Slice( 2, 400000 );

			  // Then
			  ListValue expected = list( longValue( 7L ), longValue( 8L ), longValue( 9L ), longValue( 10L ), longValue( 11L ) );
			  assertEquals( expected, slice );
			  assertEquals( expected.GetHashCode(), slice.GetHashCode() );
			  assertArrayEquals( expected.AsArray(), slice.AsArray() );
			  assertTrue( iteratorsEqual( expected.GetEnumerator(), slice.GetEnumerator() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleNegativeStart()
		 internal virtual void ShouldHandleNegativeStart()
		 {
			  // Given
			  ListValue inner = list( longValue( 5L ), longValue( 6L ), longValue( 7L ), longValue( 8L ), longValue( 9L ), longValue( 10L ), longValue( 11L ) );

			  // When
			  ListValue slice = inner.Slice( -2, 400000 );

			  // Then
			  assertEquals( inner, slice );
			  assertEquals( inner.GetHashCode(), slice.GetHashCode() );
			  assertArrayEquals( inner.AsArray(), slice.AsArray() );
			  assertTrue( iteratorsEqual( inner.GetEnumerator(), slice.GetEnumerator() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBeAbleToDropFromList()
		 internal virtual void ShouldBeAbleToDropFromList()
		 {
			  // Given
			  ListValue inner = list( longValue( 5L ), longValue( 6L ), longValue( 7L ), longValue( 8L ), longValue( 9L ), longValue( 10L ), longValue( 11L ) );

			  // When
			  ListValue drop = inner.Drop( 4 );

			  // Then
			  ListValue expected = list( longValue( 9L ), longValue( 10L ), longValue( 11L ) );
			  assertEquals( expected, drop );
			  assertEquals( expected.GetHashCode(), drop.GetHashCode() );
			  assertArrayEquals( expected.AsArray(), drop.AsArray() );
			  assertTrue( iteratorsEqual( expected.GetEnumerator(), drop.GetEnumerator() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldBeAbleToTakeFromList()
		 internal virtual void ShouldBeAbleToTakeFromList()
		 {
			  // Given
			  ListValue inner = list( longValue( 5L ), longValue( 6L ), longValue( 7L ), longValue( 8L ), longValue( 9L ), longValue( 10L ), longValue( 11L ) );

			  // When
			  ListValue take = inner.Take( 3 );

			  // Then
			  ListValue expected = list( longValue( 5L ), longValue( 6L ), longValue( 7L ) );
			  assertEquals( expected, take );
			  assertEquals( expected.GetHashCode(), take.GetHashCode() );
			  assertArrayEquals( expected.AsArray(), take.AsArray() );
			  assertTrue( iteratorsEqual( expected.GetEnumerator(), take.GetEnumerator() ) );
		 }
	}

}