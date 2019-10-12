using System.Collections.Generic;

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
namespace Neo4Net.@unsafe.Impl.Batchimport
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Race = Neo4Net.Test.Race;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using Client = Neo4Net.@unsafe.Impl.Batchimport.DataStatistics.Client;
	using RelationshipTypeCount = Neo4Net.@unsafe.Impl.Batchimport.DataStatistics.RelationshipTypeCount;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class DataStatisticsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.RandomRule random = new org.neo4j.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSumCounts() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSumCounts()
		 {
			  // given
			  DataStatistics stats = new DataStatistics( 1, 2, new RelationshipTypeCount[0] );
			  Race race = new Race();
			  int types = 10;
			  long[] expected = new long[types];
			  int threads = Runtime.Runtime.availableProcessors();
			  for ( int i = 0; i < threads; i++ )
			  {
					long[] local = new long[types];
					for ( int j = 0; j < types; j++ )
					{
						 local[j] = Random.Next( 1_000, 2_000 );
						 expected[j] += local[j];
					}
					race.AddContestant(() =>
					{
					 using ( DataStatistics.Client client = stats.NewClient() )
					 {
						  for ( int typeId = 0; typeId < types; typeId++ )
						  {
								while ( local[typeId]-- > 0 )
								{
									 client.Increment( typeId );
								}
						  }
					 }
					});
			  }

			  // when
			  race.Go();

			  // then
			  stats.forEach( count => assertEquals( expected[count.TypeId], count.Count ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGrowArrayProperly()
		 public virtual void ShouldGrowArrayProperly()
		 {
			  // given
			  DataStatistics stats = new DataStatistics( 1, 1, new RelationshipTypeCount[0] );

			  // when
			  int typeId = 1_000;
			  using ( Client client = stats.NewClient() )
			  {
					client.Increment( typeId );
			  }

			  // then
			  RelationshipTypeCount count = TypeCount( stats.GetEnumerator(), typeId );
			  assertEquals( 1, count.Count );
			  assertEquals( typeId, count.TypeId );
		 }

		 private RelationshipTypeCount TypeCount( IEnumerator<RelationshipTypeCount> iterator, int typeId )
		 {
			  while ( iterator.MoveNext() )
			  {
					RelationshipTypeCount count = iterator.Current;
					if ( count.TypeId == typeId )
					{
						 return count;
					}
			  }
			  throw new System.InvalidOperationException( "Couldn't find " + typeId );
		 }
	}

}