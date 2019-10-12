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
namespace Neo4Net.Kernel.impl.util
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class MovingAverageTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveAverageMovingWithChanges()
		 public virtual void ShouldHaveAverageMovingWithChanges()
		 {
			  // GIVEN
			  MovingAverage average = new MovingAverage( 5 );

			  // WHEN moving to 10 as average
			  long avg = average.Average();
			  for ( int i = 0; i < 5; i++ )
			  {
					average.Add( 10 );
					assertEquals( 10L, average.Average() );
					avg = average.Average();
			  }
			  assertEquals( 10L, average.Average() );

			  // WHEN moving to 100 as average
			  for ( int i = 0; i < 5; i++ )
			  {
					average.Add( 100 );
					assertThat( average.Average(), greaterThan(avg) );
					avg = average.Average();
			  }
			  assertEquals( 100L, average.Average() );
		 }
	}

}