/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Tooling
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;

	internal class CharacterConverterTest
	{
		 private readonly CharacterConverter _converter = new CharacterConverter();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldConvertCharacter()
		 internal virtual void ShouldConvertCharacter()
		 {
			  // GIVEN
			  string candidates = "abcdefghijklmnopqrstuvwxyzåäö\"'^`\\"; // to name a few

			  // THEN
			  for ( int i = 0; i < candidates.Length; i++ )
			  {
					char expected = candidates[i];
					AssertCorrectConversion( expected, expected.ToString() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldConvertRawAscii()
		 internal virtual void ShouldConvertRawAscii()
		 {
			  for ( char expected = ( char )0; expected < char.MaxValue; expected++ )
			  {
					AssertCorrectConversion( expected, "\\" + ( int ) expected );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldConvertEscaped_t_AsTab()
		 internal virtual void ShouldConvertEscapedTAsTab()
		 {
			  // GIVEN
			  char expected = '\t';

			  // THEN
			  AssertCorrectConversion( expected, "\\t" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldConvert_t_AsTab()
		 internal virtual void ShouldConvertTAsTab()
		 {
			  // GIVEN
			  char expected = '\t';

			  // THEN
			  AssertCorrectConversion( expected, "\t" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldConvertSpelledOut_TAB_AsTab()
		 internal virtual void ShouldConvertSpelledOutTABAsTab()
		 {
			  // GIVEN
			  char expected = '\t';

			  // THEN
			  AssertCorrectConversion( expected, "TAB" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotAcceptRandomEscapedStrings()
		 internal virtual void ShouldNotAcceptRandomEscapedStrings()
		 {
			  assertThrows( typeof( System.ArgumentException ), () => _converter.apply("\\bogus") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotAcceptStrings()
		 internal virtual void ShouldNotAcceptStrings()
		 {
			  assertThrows( typeof( System.ArgumentException ), () => _converter.apply("bogus") );
		 }

		 private void AssertCorrectConversion( char expected, string material )
		 {
			  // WHEN
			  char converted = _converter.apply( material ).Value;

			  // THEN
			  assertEquals( expected, converted );
		 }
	}

}