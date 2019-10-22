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
   using Neo4Net.Concurrency;
   using Xunit;
   using Race = Neo4Net.Test.Race;



   public class ThreadSafePeakMemoryAllocationTrackerTest
   {
      
      [Fact]
            
       public void ShouldRegisterConcurrentAllocationsAndDeallocations()
      {
         // given
         ThreadSafePeakMemoryAllocationTracker tracker = new ThreadSafePeakMemoryAllocationTracker(GlobalMemoryTracker.Instance);
         Race race = new Race();
         race.AddContestants(10, () =>
         {
            for (int i = 1; i < 100; i++)
            {
               tracker.Allocated(i);
               Assert.True(tracker.UsedDirectMemory() > 0L); //$!!$ tac assertThat(tracker.UsedDirectMemory(), greaterThan(0L));
            }
            for (int i = 1; i < 100; i++)
            {
               Assert.True(tracker.UsedDirectMemory() > 0L); //$!!$ tac assertThat(tracker.UsedDirectMemory(), greaterThan(0L));
               tracker.Deallocated(i);
            }
         }, 1);

         // when
         race.Go();

         // then
       Assert.Equal(0, tracker.UsedDirectMemory());
      }

      
      [Fact] 
             
       public void ShouldRegisterPeakMemoryUsage()
      {
         // given
         ThreadSafePeakMemoryAllocationTracker tracker = new ThreadSafePeakMemoryAllocationTracker(GlobalMemoryTracker.Instance);
         int threads = 200;
         long[] allocations = new long[threads];
         //$!!$ ThreadLocalRandom random = //$!!$ ThreadLocalRandom.Current();
         long sum = 0;
         for (int i = 0; i < allocations.Length; i++)
         {
            allocations[i] = ThreadLocalRandom.Next(1, 10_000);
            sum += allocations[i];
         }

         // when
         Race race = new Race();
         for (int i = 0; i < threads; i++)
         {
            int id = i;
            race.AddContestant(() => tracker.Allocated(allocations[id]));
         }
         race.Go();
         long peakAfterAllocation = tracker.PeakMemoryUsage();
         LongStream.of(allocations).forEach(tracker.Deallocated);
         long peakAfterDeallocation = tracker.PeakMemoryUsage();
         LongStream.of(allocations).forEach(tracker.Allocated);
         tracker.Allocated(10); // <-- 10 more than previous peak
         long peakAfterHigherReallocation = tracker.PeakMemoryUsage();
         LongStream.of(allocations).forEach(tracker.Deallocated);
         tracker.Deallocated(10);
         long peakAfterFinalDeallocation = tracker.PeakMemoryUsage();

         // then
       Assert.Equal(sum, peakAfterAllocation);
       Assert.Equal(sum, peakAfterDeallocation);
       Assert.Equal(sum + 10, peakAfterHigherReallocation);
       Assert.Equal(sum + 10, peakAfterFinalDeallocation);
      }
   }
}