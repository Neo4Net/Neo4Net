using System.Threading;

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
   /// <summary>
   /// Measures CPU time by thread.
   /// </summary>
   public abstract class CpuClock
   {
      public static readonly CpuClock CPU_CLOCK = new CpuClockAnonymousInnerClass();

      private class CpuClockAnonymousInnerClass : CpuClock
      {
         private readonly ThreadMXBean threadMXBean = ManagementFactory.ThreadMXBean;

         public override long cpuTimeNanos(long threadId)
         {
            if (!threadMXBean.ThreadCpuTimeSupported)
            {
               return -1;
            }
            if (!threadMXBean.ThreadCpuTimeEnabled)
            {
               threadMXBean.ThreadCpuTimeEnabled = true;
            }
            return threadMXBean.getThreadCpuTime(threadId);
         }
      }

      public static readonly CpuClock NOT_AVAILABLE = new CpuClockAnonymousInnerClass2();

      private class CpuClockAnonymousInnerClass2 : CpuClock
      {
         public override long cpuTimeNanos(long threadId)
         {
            return -1;
         }
      }

      /// <summary>
      /// Returns the current CPU time used by the thread, in nanoseconds.
      /// </summary>
      /// <param name="thread">
      ///         the thread to get the used CPU time for. </param>
      /// <returns> the current CPU time used by the thread, in nanoseconds. </returns>
      public long CpuTimeNanos(Thread thread)
      {
         return cpuTimeNanos(thread.Id);
      }

      /// <summary>
      /// Returns the current CPU time used by the thread, in nanoseconds.
      /// </summary>
      /// <param name="threadId">
      ///         the id of the thread to get the used CPU time for. </param>
      /// <returns> the current CPU time used by the thread, in nanoseconds, or {@code -1} if getting the CPU time is not
      /// supported. </returns>
      public abstract long CpuTimeNanos(long threadId);
   }
}