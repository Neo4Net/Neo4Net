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
namespace Org.Neo4j.Values.@virtual
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.iteratorsEqual;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.NO_VALUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.list;

	internal class AppendedPrependListTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAppendToList()
		 internal virtual void ShouldAppendToList()
		 {
			  // Given
			  ListValue inner = list( longValue( 5L ), longValue( 6L ), longValue( 7L ), longValue( 8L ), longValue( 9L ), longValue( 10L ), longValue( 11L ) );

			  // When
			  ListValue appended = inner.Append( longValue( 12L ), longValue( 13L ), longValue( 14L ) );

			  // Then
			  ListValue expected = list( longValue( 5L ), longValue( 6L ), longValue( 7L ), longValue( 8L ), longValue( 9L ), longValue( 10L ), longValue( 11L ), longValue( 12L ), longValue( 13L ), longValue( 14L ) );
			  AssertListValuesEquals( appended, expected );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleEmptyAppend()
		 internal virtual void ShouldHandleEmptyAppend()
		 {
			  // Given
			  ListValue inner = list( longValue( 5L ), longValue( 6L ), longValue( 7L ), longValue( 8L ), longValue( 9L ), longValue( 10L ), longValue( 11L ) );

			  // When
			  ListValue appended = inner.Append();

			  // Then
			  AssertListValuesEquals( appended, inner );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAppendToListWithDroppedNull()
		 internal virtual void ShouldAppendToListWithDroppedNull()
		 {
			  // Given
			  ListValue inner = list( longValue( 5L ), longValue( 6L ), longValue( 7L ), longValue( 8L ), NO_VALUE, longValue( 9L ), longValue( 10L ), longValue( 11L ) );

			  // When
			  ListValue appended = inner.DropNoValues().append(longValue(12L), longValue(13L), longValue(14L));

			  // Then
			  ListValue expected = list( longValue( 5L ), longValue( 6L ), longValue( 7L ), longValue( 8L ), longValue( 9L ), longValue( 10L ), longValue( 11L ), longValue( 12L ), longValue( 13L ), longValue( 14L ) );
			  AssertListValuesEquals( appended, expected );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldPrependToList()
		 internal virtual void ShouldPrependToList()
		 {
			  // Given
			  ListValue inner = list( longValue( 5L ), longValue( 6L ), longValue( 7L ), longValue( 8L ), longValue( 9L ), longValue( 10L ), longValue( 11L ) );

			  // When
			  ListValue prepend = inner.Prepend( longValue( 2L ), longValue( 3L ), longValue( 4L ) );

			  // Then
			  ListValue expected = list( longValue( 2L ), longValue( 3L ), longValue( 4L ), longValue( 5L ), longValue( 6L ), longValue( 7L ), longValue( 8L ), longValue( 9L ), longValue( 10L ), longValue( 11L ) );
			  AssertListValuesEquals( prepend, expected );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleEmptyPrepend()
		 internal virtual void ShouldHandleEmptyPrepend()
		 {
			  // Given
			  ListValue inner = list( longValue( 5L ), longValue( 6L ), longValue( 7L ), longValue( 8L ), longValue( 9L ), longValue( 10L ), longValue( 11L ) );

			  // When
			  ListValue prepend = inner.Prepend();

			  // Then
			  AssertListValuesEquals( prepend, inner );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldPrependToListWithDroppedNull()
		 internal virtual void ShouldPrependToListWithDroppedNull()
		 {
			  // Given
			  ListValue inner = list( longValue( 5L ), longValue( 6L ), longValue( 7L ), longValue( 8L ), NO_VALUE, longValue( 9L ), longValue( 10L ), longValue( 11L ) );

			  // When
			  ListValue appended = inner.DropNoValues().prepend(longValue(2L), longValue(3L), longValue(4L));

			  // Then
			  ListValue expected = list( longValue( 2L ), longValue( 3L ), longValue( 4L ), longValue( 5L ), longValue( 6L ), longValue( 7L ), longValue( 8L ), longValue( 9L ), longValue( 10L ), longValue( 11L ) );

			  AssertListValuesEquals( appended, expected );
		 }

		 private void AssertListValuesEquals( ListValue appended, ListValue expected )
		 {
			  assertEquals( expected, appended );
			  assertEquals( expected.GetHashCode(), appended.GetHashCode() );
			  assertArrayEquals( expected.AsArray(), appended.AsArray() );
			  assertTrue( iteratorsEqual( expected.GetEnumerator(), appended.GetEnumerator() ) );
		 }
	}

}