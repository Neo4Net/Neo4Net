﻿/*
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
//	import static org.neo4j.values.storable.Values.NO_VALUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.list;

	internal class DropNoValuesListValueTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFilterList()
		 internal virtual void ShouldFilterList()
		 {
			  // Given
			  ListValue inner = list( NO_VALUE, longValue( 6L ), NO_VALUE, longValue( 8L ), longValue( 9L ), longValue( 11L ), NO_VALUE );

			  // When
			  ListValue filter = inner.DropNoValues();

			  // Then
			  ListValue expected = list( longValue( 6L ), longValue( 8L ), longValue( 9L ), longValue( 11L ) );
			  assertEquals( filter, expected );
			  assertEquals( filter.GetHashCode(), expected.GetHashCode() );
			  assertArrayEquals( filter.AsArray(), expected.AsArray() );
		 }
	}

}