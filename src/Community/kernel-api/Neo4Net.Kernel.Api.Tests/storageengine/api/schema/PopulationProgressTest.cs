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
namespace Neo4Net.Kernel.Api.StorageEngine.schema
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using Inject = Neo4Net.Test.extension.Inject;
	using RandomExtension = Neo4Net.Test.extension.RandomExtension;
	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.StorageEngine.schema.PopulationProgress.multiple;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.StorageEngine.schema.PopulationProgress.single;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(RandomExtension.class) class PopulationProgressTest
	internal class PopulationProgressTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject protected org.Neo4Net.test.rule.RandomRule random;
		 protected internal RandomRule Random;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCalculateProgressOfSingle()
		 internal virtual void ShouldCalculateProgressOfSingle()
		 {
			  // given
			  PopulationProgress populationProgress = single( 50, 100 );

			  // when
			  float progress = populationProgress.Progress;

			  // then
			  assertEquals( 0.5f, progress );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCalculateProgressOfMultipleEquallyWeightedProgresses()
		 internal virtual void ShouldCalculateProgressOfMultipleEquallyWeightedProgresses()
		 {
			  // given
			  PopulationProgress part1 = single( 1, 1 );
			  PopulationProgress part2 = single( 4, 10 );
			  PopulationProgress multi = multiple().add(part1, 1).add(part2, 1).build();

			  // when
			  float progress = multi.Progress;

			  // then
			  assertEquals( 0.5f + 0.2f, progress );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCalculateProgressOfMultipleDifferentlyWeightedProgresses()
		 internal virtual void ShouldCalculateProgressOfMultipleDifferentlyWeightedProgresses()
		 {
			  // given
			  PopulationProgress part1 = single( 1, 3 );
			  PopulationProgress part2 = single( 4, 10 );
			  PopulationProgress multi = multiple().add(part1, 3).add(part2, 1).build();

			  // when
			  float progress = multi.Progress;

			  // then
			  assertEquals( ( ( 1f / 3f ) * ( 3f / 4f ) ) + ( ( 4f / 10 ) * ( 1f / 4f ) ), progress );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAlwaysResultInFullyCompleted()
		 internal virtual void ShouldAlwaysResultInFullyCompleted()
		 {
			  // given
			  int partCount = Random.Next( 5, 10 );
			  PopulationProgress_MultiBuilder builder = multiple();
			  for ( int i = 0; i < partCount; i++ )
			  {
					long total = Random.nextLong( 10_000_000 );
					builder.Add( single( total, total ), Random.nextFloat() * Random.Next(1, 10) );
			  }
			  PopulationProgress populationProgress = builder.Build();

			  // when
			  float progress = populationProgress.Progress;

			  // then
			  assertEquals( 1f, progress );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCalculateProgressForNestedMultipleParts()
		 internal virtual void ShouldCalculateProgressForNestedMultipleParts()
		 {
			  // given
			  PopulationProgress multiPart1 = multiple().add(single(1, 1), 1).add(single(1, 5), 1).build(); // should result in 60%
			  assertEquals( 0.6f, multiPart1.Progress );
			  PopulationProgress multiPart2 = multiple().add(single(6, 10), 1).add(single(1, 5), 1).build(); // should result in 40%
			  assertEquals( 0.4f, multiPart2.Progress );

			  // when
			  PopulationProgress_MultiBuilder builder = multiple();
			  PopulationProgress all = builder.Add( multiPart1, 1 ).add( multiPart2, 1 ).build();

			  // then
			  assertEquals( 0.5, all.Progress );
		 }
	}

}