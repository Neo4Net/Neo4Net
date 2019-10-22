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
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.junit.jupiter.api.Assertions.assertArrayEquals;
   //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
   //	import static org.junit.jupiter.api.Assertions.assertEquals;

   internal class TextUtilTest
   {
      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void shouldReplaceVariablesWithValuesInTemplateString()
      internal virtual void ShouldReplaceVariablesWithValuesInTemplateString()
      {
         // given
         string template = "This is a $FIRST that $SECOND $THIRD!";
         IDictionary<string, string> values = new Dictionary<string, string>();
         values["FIRST"] = "String";
         values["SECOND"] = "should";
         values["THIRD"] = "act as a template!";

         // when
         string @string = TextUtil.TemplateString(template, values);

         // then
        Assert.Equals("This is a String that should act as a template!!", @string);
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void shouldTokenizeStringWithWithoutQuotes()
      internal virtual void ShouldTokenizeStringWithWithoutQuotes()
      {
         // given
         string untokenized = "First Second Third";

         // when
         string[] tokenized = TextUtil.TokenizeStringWithQuotes(untokenized);

         // then
         assertArrayEquals(new string[] { "First", "Second", "Third" }, tokenized);
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void shouldTokenizeStringWithQuotes()
      internal virtual void ShouldTokenizeStringWithQuotes()
      {
         // given
         string untokenized = "First \"Second one\" Third \"And a fourth\"";

         // when
         string[] tokenized = TextUtil.TokenizeStringWithQuotes(untokenized);

         // then
         assertArrayEquals(new string[] { "First", "Second one", "Third", "And a fourth" }, tokenized);
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void shouldTokenStringWithWithQuotesAndEscapedSpaces()
      internal virtual void ShouldTokenStringWithWithQuotesAndEscapedSpaces()
      {
         // given
         string untokenized = "First \"Second one\" Third And\\ a\\ fourth";

         // when
         string[] tokenized = TextUtil.TokenizeStringWithQuotes(untokenized);

         // then
         assertArrayEquals(new string[] { "First", "Second one", "Third", "And a fourth" }, tokenized);
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void shouldPreserveBackslashes()
      internal virtual void ShouldPreserveBackslashes()
      {
         // given
         string untokenized = "First C:\\a\\b\\c";

         // when
         string[] tokenized = TextUtil.TokenizeStringWithQuotes(untokenized, true, true);

         // then
         assertArrayEquals(new string[] { "First", "C:\\a\\b\\c" }, tokenized);
      }

      //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
      [Fact] //ORIGINAL LINE: @Test void preserveOnlyPathBackslashes()
      internal virtual void PreserveOnlyPathBackslashes()
      {
         // given
         string untokenized = "First C:\\a\\ r\\b\\c";

         // when
         string[] tokenized = TextUtil.TokenizeStringWithQuotes(untokenized, true, true, false);

         // then
         assertArrayEquals(new string[] { "First", "C:\\a r\\b\\c" }, tokenized);
      }
   }
}