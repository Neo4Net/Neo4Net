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
namespace Neo4Net.Consistency.statistics
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;

	internal class DefaultCountsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCountPerThread()
		 internal virtual void ShouldCountPerThread()
		 {
			  // GIVEN
			  Counts counts = new DefaultCounts( 3 );

			  // WHEN
			  Counts.incAndGet( Counts_Type.activeCache, 0 );
			  Counts.incAndGet( Counts_Type.activeCache, 1 );
			  Counts.incAndGet( Counts_Type.backLinks, 2 );

			  // THEN
			  assertEquals( 2, Counts.sum( Counts_Type.activeCache ) );
			  assertEquals( 1, Counts.sum( Counts_Type.backLinks ) );
			  assertEquals( 0, Counts.sum( Counts_Type.clearCache ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldResetCounts()
		 internal virtual void ShouldResetCounts()
		 {
			  // GIVEN
			  Counts counts = new DefaultCounts( 2 );
			  Counts.incAndGet( Counts_Type.activeCache, 0 );
			  assertEquals( 1, Counts.sum( Counts_Type.activeCache ) );

			  // WHEN
			  Counts.reset();

			  // THEN
			  assertEquals( 0, Counts.sum( Counts_Type.activeCache ) );
		 }
	}

}