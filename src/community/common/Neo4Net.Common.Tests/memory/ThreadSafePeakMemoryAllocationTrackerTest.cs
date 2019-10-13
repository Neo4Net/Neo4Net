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
namespace Neo4Net.Memory
{
	using Test = org.junit.jupiter.api.Test;


	using Race = Neo4Net.Test.Race;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;

	internal class ThreadSafePeakMemoryAllocationTrackerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRegisterConcurrentAllocationsAndDeallocations() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRegisterConcurrentAllocationsAndDeallocations()
		 {
			  // given
			  ThreadSafePeakMemoryAllocationTracker tracker = new ThreadSafePeakMemoryAllocationTracker( GlobalMemoryTracker.Instance );
			  Race race = new Race();
			  race.AddContestants(10, () =>
			  {
				for ( int i = 1; i < 100; i++ )
				{
					 tracker.Allocated( i );
					 assertThat( tracker.UsedDirectMemory(), greaterThan(0L) );
				}
				for ( int i = 1; i < 100; i++ )
				{
					 assertThat( tracker.UsedDirectMemory(), greaterThan(0L) );
					 tracker.Deallocated( i );
				}
			  }, 1);

			  // when
			  race.Go();

			  // then
			  assertEquals( 0, tracker.UsedDirectMemory() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRegisterPeakMemoryUsage() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRegisterPeakMemoryUsage()
		 {
			  // given
			  ThreadSafePeakMemoryAllocationTracker tracker = new ThreadSafePeakMemoryAllocationTracker( GlobalMemoryTracker.Instance );
			  int threads = 200;
			  long[] allocations = new long[threads];
			  ThreadLocalRandom random = ThreadLocalRandom.current();
			  long sum = 0;
			  for ( int i = 0; i < allocations.Length; i++ )
			  {
					allocations[i] = random.Next( 1, 10_000 );
					sum += allocations[i];
			  }

			  // when
			  Race race = new Race();
			  for ( int i = 0; i < threads; i++ )
			  {
					int id = i;
					race.AddContestant( () => tracker.allocated(allocations[id]) );
			  }
			  race.Go();
			  long peakAfterAllocation = tracker.PeakMemoryUsage();
			  LongStream.of( allocations ).forEach( tracker.deallocated );
			  long peakAfterDeallocation = tracker.PeakMemoryUsage();
			  LongStream.of( allocations ).forEach( tracker.allocated );
			  tracker.Allocated( 10 ); // <-- 10 more than previous peak
			  long peakAfterHigherReallocation = tracker.PeakMemoryUsage();
			  LongStream.of( allocations ).forEach( tracker.deallocated );
			  tracker.Deallocated( 10 );
			  long peakAfterFinalDeallocation = tracker.PeakMemoryUsage();

			  // then
			  assertEquals( sum, peakAfterAllocation );
			  assertEquals( sum, peakAfterDeallocation );
			  assertEquals( sum + 10, peakAfterHigherReallocation );
			  assertEquals( sum + 10, peakAfterFinalDeallocation );
		 }
	}

}