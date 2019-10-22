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

using Neo4Net.Concurrency;

namespace Neo4Net.Memory
{
   /// <summary>
   /// Global memory tracker that can be used in a global multi threaded context to record
   /// allocation and de-allocation of native memory. </summary>
   /// <seealso cref= Neo4Net.Memory.IMemoryAllocationTracker </seealso>
   /// <seealso cref= MemoryTracker </seealso>
   public class GlobalMemoryTracker : IMemoryAllocationTracker
   {
      public static readonly GlobalMemoryTracker Instance = new GlobalMemoryTracker();

      private readonly StripedLongAdder _allocatedBytes = new StripedLongAdder();

      private GlobalMemoryTracker()
      {
      }

      public long UsedDirectMemory()
      {
         return _allocatedBytes.GetValue(); //$!!$ Sum();
      }

      public void Allocated(long bytes)
      {
         _allocatedBytes.Add(bytes);
      }

      public void Deallocated(long bytes)
      {
         _allocatedBytes.Add(-bytes);
      }
   }
}