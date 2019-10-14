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

namespace Neo4Net.Helpers
{
   using ExpectedException = org.junit.rules.ExpectedException;

   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.junit.jupiter.api.Assertions.assertEquals;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.neo4j.helpers.Numbers.safeCastIntToShort;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.neo4j.helpers.Numbers.safeCastIntToUnsignedShort;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.neo4j.helpers.Numbers.safeCastLongToByte;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.neo4j.helpers.Numbers.safeCastLongToInt;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.neo4j.helpers.Numbers.safeCastLongToShort;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.neo4j.helpers.Numbers.unsignedShortToInt;

   //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
   //ORIGINAL LINE: @EnableRuleMigrationSupport public class NumbersTest
   public class NumbersTest
   {
      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      //ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
      public ExpectedException ExpectedException = ExpectedException.none();

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void failSafeCastLongToInt()
      internal virtual void FailSafeCastLongToInt()
      {
         ExpectedException.expect(typeof(ArithmeticException));
         ExpectedException.expectMessage("Value 2147483648 is too big to be represented as int");

         safeCastLongToInt(int.MaxValue + 1L);
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void failSafeCastLongToShort()
      internal virtual void FailSafeCastLongToShort()
      {
         ExpectedException.expect(typeof(ArithmeticException));
         ExpectedException.expectMessage("Value 32768 is too big to be represented as short");

         safeCastLongToShort(short.MaxValue + 1L);
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void failSafeCastIntToUnsignedShort()
      internal virtual void FailSafeCastIntToUnsignedShort()
      {
         ExpectedException.expect(typeof(ArithmeticException));
         ExpectedException.expectMessage("Value 131068 is too big to be represented as unsigned short");

         safeCastIntToUnsignedShort(short.MaxValue << 2);
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void failSafeCastLongToByte()
      internal virtual void FailSafeCastLongToByte()
      {
         ExpectedException.expect(typeof(ArithmeticException));
         ExpectedException.expectMessage("Value 128 is too big to be represented as byte");

         safeCastLongToByte(sbyte.MaxValue + 1);
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void failSafeCastIntToShort()
      internal virtual void FailSafeCastIntToShort()
      {
         ExpectedException.expect(typeof(ArithmeticException));
         ExpectedException.expectMessage("Value 32768 is too big to be represented as short");

         safeCastIntToShort(short.MaxValue + 1);
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void castLongToInt()
      internal virtual void CastLongToInt()
      {
         assertEquals(1, safeCastLongToInt(1L));
         assertEquals(10, safeCastLongToInt(10L));
         assertEquals(-1, safeCastLongToInt(-1L));
         assertEquals(int.MaxValue, safeCastLongToInt(int.MaxValue));
         assertEquals(int.MinValue, safeCastLongToInt(int.MinValue));
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void castLongToShort()
      internal virtual void CastLongToShort()
      {
         assertEquals(1, safeCastLongToShort(1L));
         assertEquals(10, safeCastLongToShort(10L));
         assertEquals(-1, safeCastLongToShort(-1L));
         assertEquals(short.MaxValue, safeCastLongToShort(short.MaxValue));
         assertEquals(short.MinValue, safeCastLongToShort(short.MinValue));
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void castIntToUnsignedShort()
      internal virtual void CastIntToUnsignedShort()
      {
         assertEquals(1, safeCastIntToUnsignedShort(1));
         assertEquals(10, safeCastIntToUnsignedShort(10));
         assertEquals(-1, safeCastIntToUnsignedShort((short.MaxValue << 1) + 1));
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void castIntToShort()
      internal virtual void CastIntToShort()
      {
         assertEquals(1, safeCastIntToShort(1));
         assertEquals(10, safeCastIntToShort(10));
         assertEquals(short.MaxValue, safeCastIntToShort(short.MaxValue));
         assertEquals(short.MinValue, safeCastIntToShort(short.MinValue));
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void castLongToByte()
      internal virtual void CastLongToByte()
      {
         assertEquals(1, safeCastLongToByte(1L));
         assertEquals(10, safeCastLongToByte(10L));
         assertEquals(-1, safeCastLongToByte(-1L));
         assertEquals(sbyte.MaxValue, safeCastLongToByte(sbyte.MaxValue));
         assertEquals(sbyte.MinValue, safeCastLongToByte(sbyte.MinValue));
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void castUnsignedShortToInt()
      internal virtual void CastUnsignedShortToInt()
      {
         assertEquals(1, unsignedShortToInt((short)1));
         assertEquals(short.MaxValue, unsignedShortToInt(short.MaxValue));
         assertEquals((short.MaxValue << 1) | 1, unsignedShortToInt((short)-1));
      }
   }
}