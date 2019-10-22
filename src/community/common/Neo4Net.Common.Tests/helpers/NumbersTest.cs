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
   //	import static org.Neo4Net.helpers.Numbers.safeCastIntToShort;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.Neo4Net.helpers.Numbers.safeCastIntToUnsignedShort;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.Neo4Net.helpers.Numbers.safeCastLongToByte;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.Neo4Net.helpers.Numbers.safeCastLongToInt;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.Neo4Net.helpers.Numbers.safeCastLongToShort;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.Neo4Net.helpers.Numbers.unsignedShortToInt;

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
        Assert.Equals(1, safeCastLongToInt(1L));
        Assert.Equals(10, safeCastLongToInt(10L));
        Assert.Equals(-1, safeCastLongToInt(-1L));
        Assert.Equals(int.MaxValue, safeCastLongToInt(int.MaxValue));
        Assert.Equals(int.MinValue, safeCastLongToInt(int.MinValue));
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void castLongToShort()
      internal virtual void CastLongToShort()
      {
        Assert.Equals(1, safeCastLongToShort(1L));
        Assert.Equals(10, safeCastLongToShort(10L));
        Assert.Equals(-1, safeCastLongToShort(-1L));
        Assert.Equals(short.MaxValue, safeCastLongToShort(short.MaxValue));
        Assert.Equals(short.MinValue, safeCastLongToShort(short.MinValue));
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void castIntToUnsignedShort()
      internal virtual void CastIntToUnsignedShort()
      {
        Assert.Equals(1, safeCastIntToUnsignedShort(1));
        Assert.Equals(10, safeCastIntToUnsignedShort(10));
        Assert.Equals(-1, safeCastIntToUnsignedShort((short.MaxValue << 1) + 1));
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void castIntToShort()
      internal virtual void CastIntToShort()
      {
        Assert.Equals(1, safeCastIntToShort(1));
        Assert.Equals(10, safeCastIntToShort(10));
        Assert.Equals(short.MaxValue, safeCastIntToShort(short.MaxValue));
        Assert.Equals(short.MinValue, safeCastIntToShort(short.MinValue));
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void castLongToByte()
      internal virtual void CastLongToByte()
      {
        Assert.Equals(1, safeCastLongToByte(1L));
        Assert.Equals(10, safeCastLongToByte(10L));
        Assert.Equals(-1, safeCastLongToByte(-1L));
        Assert.Equals(sbyte.MaxValue, safeCastLongToByte(sbyte.MaxValue));
        Assert.Equals(sbyte.MinValue, safeCastLongToByte(sbyte.MinValue));
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void castUnsignedShortToInt()
      internal virtual void CastUnsignedShortToInt()
      {
        Assert.Equals(1, unsignedShortToInt((short)1));
        Assert.Equals(short.MaxValue, unsignedShortToInt(short.MaxValue));
        Assert.Equals((short.MaxValue << 1) | 1, unsignedShortToInt((short)-1));
      }
   }
}