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

using Xunit;

namespace Neo4Net.Memory
{
   public class LocalMemoryTrackerTest
   {
      [Fact]
      public void TrackMemoryAllocations()
      {
         LocalMemoryTracker memoryTracker = new LocalMemoryTracker();
         memoryTracker.Allocated(10);
         memoryTracker.Allocated(20);
         memoryTracker.Allocated(40);
         Assert.Equal(70, memoryTracker.UsedDirectMemory());
      }

      [Fact]
      public void TrackMemoryDeallocations()
      {
         LocalMemoryTracker memoryTracker = new LocalMemoryTracker();
         memoryTracker.Allocated(100);
         Assert.Equal(100, memoryTracker.UsedDirectMemory());

         memoryTracker.Deallocated(20);
         Assert.Equal(80, memoryTracker.UsedDirectMemory());

         memoryTracker.Deallocated(40);
         Assert.Equal(40, memoryTracker.UsedDirectMemory());
      }
   }
}