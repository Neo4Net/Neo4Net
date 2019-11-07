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

namespace Neo4Net.Test.matchers.matchertests
{
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.hamcrest.MatcherAssert.assertThat;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.hamcrest.Matchers.allOf;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.hamcrest.Matchers.containsString;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static Neo4Net.test.matchers.ByteArrayMatcher.byteArray;

   internal class ByteArrayMatcherTest
   {
      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void metaTestForByteArrayMatcher()
      internal virtual void MetaTestForByteArrayMatcher()
      {
         sbyte[] a = new sbyte[] { 1, (sbyte)-2, 3 };
         sbyte[] b = new sbyte[] { 1, 3, (sbyte)-2 };
         assertThat("a == a", a, byteArray(a));

         string caughtError = null;
         try
         {
            assertThat("a != b", a, byteArray(b)); // this must fail
         }
         catch (AssertionError error)
         {
            caughtError = error.Message;
         }
         string expectedMessage = "Expected: byte[] { 01 03 FE }";
         string butMessage = "     but: byte[] { 01 FE 03 }";
         assertThat("should have thrown on a != b", caughtError, allOf(containsString(expectedMessage), containsString(butMessage)));
      }
   }
}