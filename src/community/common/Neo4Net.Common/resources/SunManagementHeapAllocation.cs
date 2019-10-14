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

namespace Neo4Net.Resources
{
   using ThreadMXBean = com.sun.management.ThreadMXBean;

   internal class SunManagementHeapAllocation : HeapAllocation
   {
      /// <summary>
      /// Invoked from <seealso cref="HeapAllocation.load(java.lang.management.ThreadMXBean)"/> through reflection.
      /// </summary>
      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      //ORIGINAL LINE: @SuppressWarnings("unused") static HeapAllocation load(java.lang.management.ThreadMXBean bean)
      internal static HeapAllocation Load(java.lang.management.ThreadMXBean bean)
      {
         if (typeof(ThreadMXBean).IsInstanceOfType(bean))
         {
            return new SunManagementHeapAllocation((ThreadMXBean)bean);
         }
         return NotAvailable;
      }

      private readonly ThreadMXBean _threadMXBean;

      private SunManagementHeapAllocation(ThreadMXBean threadMXBean)
      {
         this._threadMXBean = threadMXBean;
      }

      public override long AllocatedBytes(long threadId)
      {
         if (!_threadMXBean.ThreadAllocatedMemorySupported)
         {
            return -1;
         }
         if (!_threadMXBean.ThreadAllocatedMemoryEnabled)
         {
            _threadMXBean.ThreadAllocatedMemoryEnabled = true;
         }
         return _threadMXBean.getThreadAllocatedBytes(threadId);
      }
   }
}