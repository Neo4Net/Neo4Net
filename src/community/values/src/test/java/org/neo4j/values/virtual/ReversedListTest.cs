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
//	import static org.neo4j.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.EMPTY_LIST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.list;

	internal class ReversedListTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleEmptyList()
		 internal virtual void ShouldHandleEmptyList()
		 {
			  // Given
			  ListValue inner = EMPTY_LIST;
			  // When
			  ListValue reverse = inner.Reverse();

			  // Then
			  assertEquals( inner, reverse );
			  assertEquals( inner.GetHashCode(), reverse.GetHashCode() );
			  assertArrayEquals( inner.AsArray(), reverse.AsArray() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleSingleItemList()
		 internal virtual void ShouldHandleSingleItemList()
		 {
			  // Given
			  ListValue inner = list( longValue( 5L ) );

			  // When
			  ListValue reverse = inner.Reverse();

			  // Then
			  assertEquals( inner, reverse );
			  assertEquals( inner.GetHashCode(), reverse.GetHashCode() );
			  assertArrayEquals( inner.AsArray(), reverse.AsArray() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReverseList()
		 internal virtual void ShouldReverseList()
		 {
			  // Given
			  ListValue inner = list( longValue( 5L ), longValue( 6L ), longValue( 7L ) );

			  // When
			  ListValue reverse = inner.Reverse();

			  // Then
			  ListValue expected = list( longValue( 7L ), longValue( 6L ), longValue( 5L ) );
			  assertEquals( expected, reverse );
			  assertEquals( expected.GetHashCode(), reverse.GetHashCode() );
			  assertArrayEquals( expected.AsArray(), reverse.AsArray() );
		 }
	}

}