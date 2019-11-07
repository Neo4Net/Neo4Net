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

namespace Neo4Net.Ports.Allocation
{
   using TemporaryFolder = org.junit.rules.TemporaryFolder;

   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static Arrays.asList;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.hamcrest.CoreMatchers.@is;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.hamcrest.MatcherAssert.assertThat;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.junit.jupiter.api.Assertions.fail;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static Neo4Net.ports.allocation.PortConstants.EphemeralPortMinimum;

   //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
   //ORIGINAL LINE: @EnableRuleMigrationSupport public class PortRepositoryIT
   public class PortRepositoryIT
   {
      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      //ORIGINAL LINE: @Rule public final org.junit.rules.TemporaryFolder temporaryFolder = new org.junit.rules.TemporaryFolder();
      public readonly TemporaryFolder TemporaryFolder = new TemporaryFolder();

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void shouldReservePorts() throws Exception
             //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      internal virtual void ShouldReservePorts()
      {
         PortRepository portRepository1 = new PortRepository(TemporaryDirectory(), EphemeralPortMinimum);

         int port1 = portRepository1.ReserveNextPort("foo");
         int port2 = portRepository1.ReserveNextPort("foo");
         int port3 = portRepository1.ReserveNextPort("foo");

         assertThat((new HashSet<>(asList(port1, port2, port3))).Count, @is(3));
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void shouldCoordinateUsingFileSystem() throws Exception
             //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      internal virtual void ShouldCoordinateUsingFileSystem()
      {
         Path temporaryDirectory = temporaryDirectory();
         PortRepository portRepository1 = new PortRepository(temporaryDirectory, EphemeralPortMinimum);
         PortRepository portRepository2 = new PortRepository(temporaryDirectory, EphemeralPortMinimum);

         int port1 = portRepository1.ReserveNextPort("foo");
         int port2 = portRepository1.ReserveNextPort("foo");
         int port3 = portRepository1.ReserveNextPort("foo");
         int port4 = portRepository2.ReserveNextPort("foo");
         int port5 = portRepository2.ReserveNextPort("foo");
         int port6 = portRepository1.ReserveNextPort("foo");

         assertThat((new HashSet<>(asList(port1, port2, port3, port4, port5, port6))).Count, @is(6));
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void shouldNotOverrun() throws Exception
             //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      internal virtual void ShouldNotOverrun()
      {
         PortRepository portRepository1 = new PortRepository(TemporaryDirectory(), 65534);

         portRepository1.ReserveNextPort("foo");
         portRepository1.ReserveNextPort("foo");

         try
         {
            portRepository1.ReserveNextPort("foo");

            fail("Failure was expected");
         }
         catch (System.InvalidOperationException e)
         {
            assertThat(e.Message, @is("There are no more ports available"));
         }
      }

      //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
      //ORIGINAL LINE: private java.nio.file.Path temporaryDirectory() throws java.io.IOException
      private Path TemporaryDirectory()
      {
         return TemporaryFolder.newFolder("port-repository").toPath();
      }
   }
}