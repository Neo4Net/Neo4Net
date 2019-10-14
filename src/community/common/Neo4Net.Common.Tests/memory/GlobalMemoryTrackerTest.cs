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
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.junit.jupiter.api.Assertions.assertEquals;

   internal class GlobalMemoryTrackerTest
   {
      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void trackMemoryAllocations()
      internal virtual void TrackMemoryAllocations()
      {
         long initialUsedMemory = GlobalMemoryTracker.Instance.usedDirectMemory();
         GlobalMemoryTracker.Instance.allocated(10);
         GlobalMemoryTracker.Instance.allocated(20);
         GlobalMemoryTracker.Instance.allocated(40);
         assertEquals(70, GlobalMemoryTracker.Instance.usedDirectMemory() - initialUsedMemory);
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void trackMemoryDeallocations()
      internal virtual void TrackMemoryDeallocations()
      {
         long initialUsedMemory = GlobalMemoryTracker.Instance.usedDirectMemory();
         GlobalMemoryTracker.Instance.allocated(100);
         assertEquals(100, GlobalMemoryTracker.Instance.usedDirectMemory() - initialUsedMemory);

         GlobalMemoryTracker.Instance.deallocated(20);
         assertEquals(80, GlobalMemoryTracker.Instance.usedDirectMemory() - initialUsedMemory);

         GlobalMemoryTracker.Instance.deallocated(40);
         assertEquals(40, GlobalMemoryTracker.Instance.usedDirectMemory() - initialUsedMemory);
      }
   }
}