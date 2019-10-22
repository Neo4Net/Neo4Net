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
   public class GlobalMemoryTrackerTest
   {
      [Fact]
      public void TrackMemoryAllocations()
      {
         long initialUsedMemory = GlobalMemoryTracker.Instance.UsedDirectMemory();
         GlobalMemoryTracker.Instance.Allocated(10);
         GlobalMemoryTracker.Instance.Allocated(20);
         GlobalMemoryTracker.Instance.Allocated(40);
         Assert.Equal(70, GlobalMemoryTracker.Instance.UsedDirectMemory() - initialUsedMemory);
      }

      [Fact]
      public void TrackMemoryDeallocations()
      {
         long initialUsedMemory = GlobalMemoryTracker.Instance.UsedDirectMemory();
         GlobalMemoryTracker.Instance.Allocated(100);
         Assert.Equal(100, GlobalMemoryTracker.Instance.UsedDirectMemory() - initialUsedMemory);

         GlobalMemoryTracker.Instance.Deallocated(20);
         Assert.Equal(80, GlobalMemoryTracker.Instance.UsedDirectMemory() - initialUsedMemory);

         GlobalMemoryTracker.Instance.Deallocated(40);
         Assert.Equal(40, GlobalMemoryTracker.Instance.UsedDirectMemory() - initialUsedMemory);
      }
   }
}