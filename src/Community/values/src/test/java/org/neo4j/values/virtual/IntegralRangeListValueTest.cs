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
namespace Neo4Net.Values.@virtual
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.list;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.range;

	internal class IntegralRangeListValueTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleRangeWithStepOne()
		 internal virtual void ShouldHandleRangeWithStepOne()
		 {
			  ListValue range = range( 5L, 11L, 1L );

			  ListValue expected = list( longValue( 5L ), longValue( 6L ), longValue( 7L ), longValue( 8L ), longValue( 9L ), longValue( 10L ), longValue( 11L ) );

			  assertEquals( range, expected );
			  assertEquals( range.GetHashCode(), expected.GetHashCode() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleRangeWithBiggerSteps()
		 internal virtual void ShouldHandleRangeWithBiggerSteps()
		 {
			  ListValue range = range( 5L, 11L, 3L );

			  ListValue expected = list( longValue( 5L ), longValue( 8L ), longValue( 11L ) );

			  assertEquals( range, expected );
			  assertEquals( range.GetHashCode(), expected.GetHashCode() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleNegativeStep()
		 internal virtual void ShouldHandleNegativeStep()
		 {
			  ListValue range = range( 11L, 5L, -3L );

			  ListValue expected = list( longValue( 11L ), longValue( 8L ), longValue( 5L ) );

			  assertEquals( range, expected );
			  assertEquals( range.GetHashCode(), expected.GetHashCode() );
		 }
	}

}