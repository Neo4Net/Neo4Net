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
   /// <summary>
   /// Global memory tracker that can be used in a global multi threaded context to record
   /// allocation and de-allocation of native memory. </summary>
   /// <seealso cref= org.neo4j.memory.MemoryAllocationTracker </seealso>
   /// <seealso cref= MemoryTracker </seealso>
   public class GlobalMemoryTracker : MemoryAllocationTracker
   {
      public static readonly GlobalMemoryTracker Instance = new GlobalMemoryTracker();

      private readonly LongAdder _allocatedBytes = new LongAdder();

      private GlobalMemoryTracker()
      {
      }

      public override long UsedDirectMemory()
      {
         return _allocatedBytes.sum();
      }

      public override void Allocated(long bytes)
      {
         _allocatedBytes.add(bytes);
      }

      public override void Deallocated(long bytes)
      {
         _allocatedBytes.add(-bytes);
      }
   }
}