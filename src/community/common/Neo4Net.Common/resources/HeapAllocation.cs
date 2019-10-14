using System;
using System.Text;
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
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static Character.toUpperCase;

   public abstract class HeapAllocation
   {
      //JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
      public static readonly HeapAllocation HeapAllocationConflict;

      public static readonly HeapAllocation NotAvailable;

      static HeapAllocation()
      {
         NotAvailable = new HeapAllocationNotAvailable(); // must be first!
         HeapAllocationConflict = Load(ManagementFactory.ThreadMXBean);
      }

      /// <summary>
      /// Returns number of allocated bytes by the thread.
      /// </summary>
      /// <param name="thread">
      ///         the thread to get the used CPU time for. </param>
      /// <returns> number of allocated bytes for specified thread. </returns>
      public long AllocatedBytes(Thread thread)
      {
         return allocatedBytes(thread.Id);
      }

      /// <summary>
      /// Returns number of allocated bytes by the thread.
      /// </summary>
      /// <param name="threadId">
      ///         the id of the thread to get the allocation information for. </param>
      /// <returns> number of allocated bytes for specified threadId. </returns>
      public abstract long AllocatedBytes(long threadId);

      private static HeapAllocation Load(ThreadMXBean bean)
      {
         Type<HeapAllocation> @base = typeof(HeapAllocation);
         StringBuilder name = (new StringBuilder()).Append(@base.Assembly.GetName().Name).Append('.');
         string pkg = bean.GetType().Assembly.GetName().Name;
         int start = 0;
         int end = pkg.IndexOf('.', start);
         while (end > 0)
         {
            name.Append(toUpperCase(pkg[start])).Append(pkg.Substring(start + 1, end - (start + 1)));
            start = end + 1;
            end = pkg.IndexOf('.', start);
         }
         name.Append(toUpperCase(pkg[start])).Append(pkg.Substring(start + 1));
         name.Append(@base.Name);
         try
         {
            return requireNonNull((HeapAllocation)Type.GetType(name.ToString()).getDeclaredMethod("load", typeof(ThreadMXBean)).invoke(null, bean), "Loader method returned null.");
         }
         catch (Exception e)
         {
            //noinspection ConstantConditions -- this can actually happen if the code order is wrong
            if (NotAvailable == null)
            {
               throw new LinkageError("Bad code loading order.", e);
            }
            return NotAvailable;
         }
      }

      private class HeapAllocationNotAvailable : HeapAllocation
      {
         public override long AllocatedBytes(long threadId)
         {
            return -1;
         }
      }
   }
}