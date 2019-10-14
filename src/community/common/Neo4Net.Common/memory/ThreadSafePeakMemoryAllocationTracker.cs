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
   //	import static Long.max;

   /// <summary>
   /// A <seealso cref="MemoryAllocationTracker"/> which is thread-safe, forwards allocations and deallocations to another <seealso cref="MemoryAllocationTracker"/>
   /// and will register peak memory usage during its lifetime.
   /// </summary>
   public class ThreadSafePeakMemoryAllocationTracker : MemoryAllocationTracker
   {
      // Why AtomicLong instead of LongAdder? AtomicLong fits this use case due to:
      // - Having much faster "sum", this is used in every call to allocate/deallocate
      // - Convenient and accurate sum when making allocations to correctly register peak memory usage
      private readonly AtomicLong _allocated = new AtomicLong();

      private readonly AtomicLong _peak = new AtomicLong();
      private readonly MemoryAllocationTracker _alsoReportTo;

      public ThreadSafePeakMemoryAllocationTracker(MemoryAllocationTracker alsoReportTo)
      {
         this._alsoReportTo = alsoReportTo;
      }

      public override void Allocated(long bytes)
      {
         // Update allocated
         long total = _allocated.addAndGet(bytes);

         // Update peak
         long currentPeak;
         long updatedPeak;
         do
         {
            currentPeak = _peak.get();
            if (currentPeak >= total)
            {
               break;
            }
            updatedPeak = max(currentPeak, total);
         } while (!_peak.compareAndSet(currentPeak, updatedPeak));

         _alsoReportTo.allocated(bytes);
      }

      public override void Deallocated(long bytes)
      {
         _allocated.addAndGet(-bytes);
         _alsoReportTo.deallocated(bytes);
      }

      public override long UsedDirectMemory()
      {
         return _allocated.get();
      }

      public virtual long PeakMemoryUsage()
      {
         return _peak.get();
      }
   }
}