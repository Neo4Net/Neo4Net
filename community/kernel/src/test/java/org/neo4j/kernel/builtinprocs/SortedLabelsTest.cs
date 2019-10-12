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
namespace Org.Neo4j.Kernel.builtinprocs
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNotEquals;

	internal class SortedLabelsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testEquals()
		 internal virtual void TestEquals()
		 {
			  long[] longsA = new long[]{ 1L, 2L, 3L };
			  long[] longsB = new long[]{ 3L, 2L, 1L };
			  long[] longsC = new long[]{ 1L, 2L, 3L, 4L };
			  SortedLabels a = SortedLabels.From( longsA );
			  SortedLabels b = SortedLabels.From( longsB );
			  SortedLabels c = SortedLabels.From( longsC );

			  // self
			  //noinspection EqualsWithItself
			  assertEquals( a, a );

			  // unordered self
			  assertEquals( a, b );
			  assertEquals( b, a );

			  // other
			  assertNotEquals( a, c );
			  assertNotEquals( c, a );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testHashCodeOfLabelSet()
		 internal virtual void TestHashCodeOfLabelSet()
		 {
			  long[] longsA = new long[]{ 1L, 2L, 3L };
			  long[] longsB = new long[]{ 3L, 2L, 1L };
			  long[] longsC = new long[]{ 1L, 2L, 3L, 4L };
			  SortedLabels a = SortedLabels.From( longsA );
			  SortedLabels b = SortedLabels.From( longsB );
			  SortedLabels c = SortedLabels.From( longsC );

			  assertEquals( a.GetHashCode(), b.GetHashCode() );
			  assertNotEquals( a.GetHashCode(), c.GetHashCode() );
		 }
	}

}