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
//	import static org.neo4j.values.storable.Values.booleanValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.intValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValueTestUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.EMPTY_LIST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.concat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.list;

	internal class ConcatListTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleZeroListConcatenation()
		 internal virtual void ShouldHandleZeroListConcatenation()
		 {
			  // Given
			  ListValue inner = EMPTY_LIST;

			  // When
			  ListValue concat = concat( inner );

			  // Then
			  assertTrue( concat.Empty );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleSingleListConcatenation()
		 internal virtual void ShouldHandleSingleListConcatenation()
		 {
			  // Given
			  ListValue inner = list( stringValue( "foo" ), longValue( 42 ), booleanValue( true ) );

			  // When
			  ListValue concat = concat( inner );

			  // Then
			  assertEquals( inner, concat );
			  assertEquals( inner.GetHashCode(), concat.GetHashCode() );
			  assertArrayEquals( inner.AsArray(), concat.AsArray() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleMultipleListConcatenation()
		 internal virtual void ShouldHandleMultipleListConcatenation()
		 {
			  // Given
			  ListValue inner1 = list( stringValue( "foo" ), longValue( 42 ), booleanValue( true ) );
			  ListValue inner2 = list( list( stringValue( "bar" ), intValue( 42 ) ) );
			  ListValue inner3 = list( map( "foo", 1337L, "bar", 42 ), stringValue( "baz" ) );

			  // When
			  ListValue concat = concat( inner1, inner2, inner3 );

			  // Then
			  ListValue expected = list( stringValue( "foo" ), longValue( 42 ), booleanValue( true ), list( stringValue( "bar" ), intValue( 42 ) ), map( "foo", 1337L, "bar", 42 ), stringValue( "baz" ) );
			  assertEquals( expected, concat );
			  assertEquals( expected.GetHashCode(), concat.GetHashCode() );
			  assertArrayEquals( expected.AsArray(), concat.AsArray() );
		 }
	}

}