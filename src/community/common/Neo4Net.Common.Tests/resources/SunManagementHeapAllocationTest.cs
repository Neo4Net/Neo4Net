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
   using Matchers = org.hamcrest.Matchers;

   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static Thread.currentThread;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.hamcrest.MatcherAssert.assertThat;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.hamcrest.Matchers.instanceOf;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.junit.jupiter.api.Assertions.assertEquals;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.junit.jupiter.api.Assertions.assertNotSame;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.junit.jupiter.api.Assumptions.assumeFalse;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static Neo4Net.resources.HeapAllocation.HEAP_ALLOCATION;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static Neo4Net.resources.HeapAllocation.NOT_AVAILABLE;

   internal class SunManagementHeapAllocationTest
   {
      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      //ORIGINAL LINE: @BeforeEach void onlyOnSupportedJvms()
      internal virtual void OnlyOnSupportedJvms()
      {
         assumeFalse(HEAP_ALLOCATION == NOT_AVAILABLE);
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void shouldLoadHeapAllocation()
      internal virtual void ShouldLoadHeapAllocation()
      {
         assertNotSame(NOT_AVAILABLE, HEAP_ALLOCATION);
         assertThat(HEAP_ALLOCATION, instanceOf(typeof(SunManagementHeapAllocation)));
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void shouldMeasureAllocation()
      internal virtual void ShouldMeasureAllocation()
      {
         // given
         long allocatedBytes = HEAP_ALLOCATION.allocatedBytes(currentThread());

         // when
         IList<object> objects = new List<object>();
         for (int i = 0; i < 17; i++)
         {
            objects.Add(new object());
         }

         // then
         assertThat(allocatedBytes, Matchers.lessThan(HEAP_ALLOCATION.allocatedBytes(currentThread())));
        Assert.Equals(17, objects.Count);
      }
   }
}