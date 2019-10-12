using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

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
namespace Org.Neo4j.Csv.Reader
{
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using After = org.junit.After;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Parameters = org.junit.runners.Parameterized.Parameters;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.csv.reader.CharSeekers.charSeeker;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.csv.reader.Readables.wrap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.array;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class BufferedCharSeekerTest
	public class BufferedCharSeekerTest
	{
		 private static readonly char[] _whitespaceChars = new char[] { UnicodeCategory.SpaceSeparator, UnicodeCategory.ParagraphSeparator, '\u00A0', '\u2007', '\u202F', '\t', '\f', '\u001C', '\u001D', '\u001E', '\u001F' };

		 private static readonly char[] _delimiterChars = new char[] { ',', '\t' };

		 private const string TEST_SOURCE = "TestSource";
		 private readonly bool _useThreadAhead;
		 private const int TAB = '\t';
		 private const int COMMA = ',';
		 private static readonly Random _random = new Random();
		 private readonly Extractors _extractors = new Extractors( ',' );
		 private readonly Mark _mark = new Mark();

		 private CharSeeker _seeker;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters(name = "{1}") public static java.util.Collection<Object[]> data()
		 public static ICollection<object[]> Data()
		 {
			  return asList( new object[] { false, "without thread-ahead" }, new object[] { true, "with thread-ahead" } );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void closeSeeker() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CloseSeeker()
		 {
			  if ( _seeker != null )
			  {
					_seeker.Dispose();
			  }
		 }

		 /// <param name="description"> used to provider a better description of what the boolean values means,
		 /// which shows up in the junit results. </param>
		 public BufferedCharSeekerTest( bool useThreadAhead, string description )
		 {
			  this._useThreadAhead = useThreadAhead;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindCertainCharacter() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindCertainCharacter()
		 {
			  // GIVEN
			  _seeker = _seeker( "abcdefg\thijklmnop\tqrstuvxyz" );

			  // WHEN/THEN
			  // first value
			  assertTrue( _seeker.seek( _mark, TAB ) );
			  assertEquals( '\t', _mark.character() );
			  assertFalse( _mark.EndOfLine );
			  assertEquals( "abcdefg", _seeker.extract( _mark, _extractors.@string() ).value() );

			  // second value
			  assertTrue( _seeker.seek( _mark, TAB ) );
			  assertEquals( '\t', _mark.character() );
			  assertFalse( _mark.EndOfLine );
			  assertEquals( "hijklmnop", _seeker.extract( _mark, _extractors.@string() ).value() );

			  // third value
			  assertTrue( _seeker.seek( _mark, TAB ) );
			  assertTrue( _mark.EndOfLine );
			  assertEquals( "qrstuvxyz", _seeker.extract( _mark, _extractors.@string() ).value() );

			  // no more values
			  assertFalse( _seeker.seek( _mark, TAB ) );
			  assertFalse( _seeker.seek( _mark, TAB ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadMultipleLines() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadMultipleLines()
		 {
			  // GIVEN
			  _seeker = _seeker( "1\t2\t3\n" + "4\t5\t6\n" );

			  // WHEN/THEN
			  assertTrue( _seeker.seek( _mark, TAB ) );
			  assertEquals( 1L, _seeker.extract( _mark, _extractors.long_() ).longValue() );

			  assertTrue( _seeker.seek( _mark, TAB ) );
			  assertEquals( 2L, _seeker.extract( _mark, _extractors.long_() ).longValue() );

			  assertTrue( _seeker.seek( _mark, TAB ) );
			  assertEquals( 3L, _seeker.extract( _mark, _extractors.long_() ).longValue() );
			  assertTrue( _mark.EndOfLine );

			  assertTrue( _seeker.seek( _mark, TAB ) );
			  assertEquals( 4L, _seeker.extract( _mark, _extractors.long_() ).longValue() );

			  assertTrue( _seeker.seek( _mark, TAB ) );
			  assertEquals( 5L, _seeker.extract( _mark, _extractors.long_() ).longValue() );

			  assertTrue( _seeker.seek( _mark, TAB ) );
			  assertEquals( 6L, _seeker.extract( _mark, _extractors.long_() ).longValue() );

			  assertTrue( _mark.EndOfLine );
			  assertFalse( _seeker.seek( _mark, TAB ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeekThroughAdditionalBufferRead() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeekThroughAdditionalBufferRead()
		 {
			  // GIVEN
			  _seeker = _seeker( "1234,5678,9012,3456", Config( 12 ) );
			  // read more here             ^

			  // WHEN/THEN
			  _seeker.seek( _mark, COMMA );
			  assertEquals( 1234L, _seeker.extract( _mark, _extractors.long_() ).longValue() );
			  _seeker.seek( _mark, COMMA );
			  assertEquals( 5678L, _seeker.extract( _mark, _extractors.long_() ).longValue() );
			  _seeker.seek( _mark, COMMA );
			  assertEquals( 9012L, _seeker.extract( _mark, _extractors.long_() ).longValue() );
			  _seeker.seek( _mark, COMMA );
			  assertEquals( 3456L, _seeker.extract( _mark, _extractors.long_() ).longValue() );
			  assertFalse( _seeker.seek( _mark, COMMA ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleWindowsEndOfLineCharacters() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleWindowsEndOfLineCharacters()
		 {
			  // GIVEN
			  _seeker = _seeker( "here,comes,Windows\r\n" + "and,it,has\r" + "other,line,endings" );

			  // WHEN/THEN
			  assertEquals( "here", _seeker.seek( _mark, COMMA ) ? _seeker.extract( _mark, _extractors.@string() ).value() : "" );
			  assertEquals( "comes", _seeker.seek( _mark, COMMA ) ? _seeker.extract( _mark, _extractors.@string() ).value() : "" );
			  assertEquals( "Windows", _seeker.seek( _mark, COMMA ) ? _seeker.extract( _mark, _extractors.@string() ).value() : "" );
			  assertTrue( _mark.EndOfLine );
			  assertEquals( "and", _seeker.seek( _mark, COMMA ) ? _seeker.extract( _mark, _extractors.@string() ).value() : "" );
			  assertEquals( "it", _seeker.seek( _mark, COMMA ) ? _seeker.extract( _mark, _extractors.@string() ).value() : "" );
			  assertEquals( "has", _seeker.seek( _mark, COMMA ) ? _seeker.extract( _mark, _extractors.@string() ).value() : "" );
			  assertTrue( _mark.EndOfLine );
			  assertEquals( "other", _seeker.seek( _mark, COMMA ) ? _seeker.extract( _mark, _extractors.@string() ).value() : "" );
			  assertEquals( "line", _seeker.seek( _mark, COMMA ) ? _seeker.extract( _mark, _extractors.@string() ).value() : "" );
			  assertEquals( "endings", _seeker.seek( _mark, COMMA ) ? _seeker.extract( _mark, _extractors.@string() ).value() : "" );
			  assertTrue( _mark.EndOfLine );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleReallyWeirdChars() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleReallyWeirdChars()
		 {
			  // GIVEN
			  int cols = 3;
			  int rows = 3;
			  char delimiter = '\t';
			  string[][] data = RandomWeirdValues( cols, rows, delimiter, '\n', '\r' );
			  _seeker = _seeker( Join( data, delimiter ) );

			  // WHEN/THEN
			  for ( int row = 0; row < rows; row++ )
			  {
					for ( int col = 0; col < cols; col++ )
					{
						 assertTrue( _seeker.seek( _mark, TAB ) );
						 assertEquals( data[row][col], _seeker.extract( _mark, _extractors.@string() ).value() );
					}
					assertTrue( _mark.EndOfLine );
			  }
			  assertFalse( _seeker.seek( _mark, TAB ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleEmptyValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleEmptyValues()
		 {
			  // GIVEN
			  _seeker = _seeker( "1,,3,4" );

			  // WHEN
			  assertTrue( _seeker.seek( _mark, COMMA ) );
			  assertEquals( 1, _seeker.extract( _mark, _extractors.int_() ).intValue() );

			  assertTrue( _seeker.seek( _mark, COMMA ) );

			  assertTrue( _seeker.seek( _mark, COMMA ) );
			  assertEquals( 3, _seeker.extract( _mark, _extractors.int_() ).intValue() );

			  assertTrue( _seeker.seek( _mark, COMMA ) );
			  assertEquals( 4, _seeker.extract( _mark, _extractors.int_() ).intValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLetEolCharSkippingMessUpPositionsInMark() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotLetEolCharSkippingMessUpPositionsInMark()
		 {
			  // GIVEN
			  _seeker = _seeker( "12,34,56\n789,901,23", Config( 9 ) );
			  // read more here          ^        ^

			  // WHEN
			  assertTrue( _seeker.seek( _mark, COMMA ) );
			  assertEquals( 12, _seeker.extract( _mark, _extractors.int_() ).intValue() );
			  assertTrue( _seeker.seek( _mark, COMMA ) );
			  assertEquals( 34, _seeker.extract( _mark, _extractors.int_() ).intValue() );
			  assertTrue( _seeker.seek( _mark, COMMA ) );
			  assertEquals( 56, _seeker.extract( _mark, _extractors.int_() ).intValue() );

			  assertTrue( _seeker.seek( _mark, COMMA ) );
			  assertEquals( 789, _seeker.extract( _mark, _extractors.int_() ).intValue() );
			  assertTrue( _seeker.seek( _mark, COMMA ) );
			  assertEquals( 901, _seeker.extract( _mark, _extractors.int_() ).intValue() );
			  assertTrue( _seeker.seek( _mark, COMMA ) );
			  assertEquals( 23, _seeker.extract( _mark, _extractors.int_() ).intValue() );

			  assertFalse( _seeker.seek( _mark, COMMA ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeEofEvenIfBufferAlignsWithEnd() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeEofEvenIfBufferAlignsWithEnd()
		 {
			  // GIVEN
			  _seeker = _seeker( "123,56", Config( 6 ) );

			  // WHEN
			  assertTrue( _seeker.seek( _mark, COMMA ) );
			  assertEquals( 123, _seeker.extract( _mark, _extractors.int_() ).intValue() );
			  assertTrue( _seeker.seek( _mark, COMMA ) );
			  assertEquals( 56, _seeker.extract( _mark, _extractors.int_() ).intValue() );

			  // THEN
			  assertFalse( _seeker.seek( _mark, COMMA ) );
			  assertFalse( _seeker.seek( _mark, COMMA ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSkipEmptyLastValue() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSkipEmptyLastValue()
		 {
			  // GIVEN
			  _seeker = _seeker( "one,two,three,\n" + "uno,dos,tres," );

			  // WHEN
			  AssertNextValue( _seeker, _mark, COMMA, "one" );
			  AssertNextValue( _seeker, _mark, COMMA, "two" );
			  AssertNextValue( _seeker, _mark, COMMA, "three" );
			  AssertNextValueNotExtracted( _seeker, _mark, COMMA );
			  assertTrue( _mark.EndOfLine );

			  AssertNextValue( _seeker, _mark, COMMA, "uno" );
			  AssertNextValue( _seeker, _mark, COMMA, "dos" );
			  AssertNextValue( _seeker, _mark, COMMA, "tres" );
			  AssertNextValueNotExtracted( _seeker, _mark, COMMA );
			  AssertEnd( _seeker, _mark, COMMA );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExtractEmptyStringForEmptyQuotedString() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldExtractEmptyStringForEmptyQuotedString()
		 {
			  // GIVEN
			  _seeker = _seeker( "\"\",,\"\"" );

			  // WHEN
			  AssertNextValue( _seeker, _mark, COMMA, "" );
			  AssertNextValueNotExtracted( _seeker, _mark, COMMA );
			  AssertNextValue( _seeker, _mark, COMMA, "" );

			  // THEN
			  assertFalse( _seeker.seek( _mark, COMMA ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExtractNullForEmptyFieldWhenWeSkipEOLChars() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldExtractNullForEmptyFieldWhenWeSkipEOLChars()
		 {
			  // GIVEN
			  _seeker = _seeker( "\"\",\r\n" );

			  // WHEN
			  AssertNextValue( _seeker, _mark, COMMA, "" );
			  AssertNextValueNotExtracted( _seeker, _mark, COMMA );

			  // THEN
			  assertFalse( _seeker.seek( _mark, COMMA ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldContinueThroughCompletelyEmptyLines() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldContinueThroughCompletelyEmptyLines()
		 {
			  // GIVEN
			  _seeker = _seeker( "one,two,three\n\n\nfour,five,six" );

			  // WHEN/THEN
			  assertArrayEquals( new string[] { "one", "two", "three" }, NextLineOfAllStrings( _seeker, _mark ) );
			  assertArrayEquals( new string[] { "four", "five", "six" }, NextLineOfAllStrings( _seeker, _mark ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleDoubleCharValues() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleDoubleCharValues()
		 {
			  _seeker = _seeker( "v\uD800\uDC00lue one\t\"v\uD801\uDC01lue two\"\tv\uD804\uDC03lue three" );
			  assertTrue( _seeker.seek( _mark, TAB ) );
			  assertEquals( "v𐀀lue one", _seeker.extract( _mark, _extractors.@string() ).value() );
			  assertTrue( _seeker.seek( _mark, TAB ) );
			  assertEquals( "v𐐁lue two", _seeker.extract( _mark, _extractors.@string() ).value() );
			  assertTrue( _seeker.seek( _mark, TAB ) );
			  assertEquals( "v𑀃lue three", _seeker.extract( _mark, _extractors.@string() ).value() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadQuotes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadQuotes()
		 {
			  // GIVEN
			  _seeker = _seeker( "value one\t\"value two\"\tvalue three" );

			  // WHEN/THEN
			  assertTrue( _seeker.seek( _mark, TAB ) );
			  assertEquals( "value one", _seeker.extract( _mark, _extractors.@string() ).value() );

			  assertTrue( _seeker.seek( _mark, TAB ) );
			  assertEquals( "value two", _seeker.extract( _mark, _extractors.@string() ).value() );

			  assertTrue( _seeker.seek( _mark, TAB ) );
			  assertEquals( "value three", _seeker.extract( _mark, _extractors.@string() ).value() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadQuotedValuesWithDelimiterInside() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadQuotedValuesWithDelimiterInside()
		 {
			  // GIVEN
			  _seeker = _seeker( "value one\t\"value\ttwo\"\tvalue three" );

			  // WHEN/THEN
			  assertTrue( _seeker.seek( _mark, TAB ) );
			  assertEquals( "value one", _seeker.extract( _mark, _extractors.@string() ).value() );

			  assertTrue( _seeker.seek( _mark, TAB ) );
			  assertEquals( "value\ttwo", _seeker.extract( _mark, _extractors.@string() ).value() );

			  assertTrue( _seeker.seek( _mark, TAB ) );
			  assertEquals( "value three", _seeker.extract( _mark, _extractors.@string() ).value() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadQuotedValuesWithNewLinesInside() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadQuotedValuesWithNewLinesInside()
		 {
			  // GIVEN
			  _seeker = _seeker( "value one\t\"value\ntwo\"\tvalue three", WithMultilineFields( Config(), true ) );

			  // WHEN/THEN
			  assertTrue( _seeker.seek( _mark, TAB ) );
			  assertEquals( "value one", _seeker.extract( _mark, _extractors.@string() ).value() );

			  assertTrue( _seeker.seek( _mark, TAB ) );
			  assertEquals( "value\ntwo", _seeker.extract( _mark, _extractors.@string() ).value() );

			  assertTrue( _seeker.seek( _mark, TAB ) );
			  assertEquals( "value three", _seeker.extract( _mark, _extractors.@string() ).value() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleDoubleQuotes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleDoubleQuotes()
		 {
			  // GIVEN
			  _seeker = _seeker( "\"value \"\"one\"\"\"\t\"\"\"value\"\" two\"\t\"va\"\"lue\"\" three\"" );

			  // "value ""one"""
			  // """value"" two"
			  // "va""lue"" three"

			  // WHEN/THEN
			  assertTrue( _seeker.seek( _mark, TAB ) );
			  assertEquals( "value \"one\"", _seeker.extract( _mark, _extractors.@string() ).value() );

			  assertTrue( _seeker.seek( _mark, TAB ) );
			  assertEquals( "\"value\" two", _seeker.extract( _mark, _extractors.@string() ).value() );

			  assertTrue( _seeker.seek( _mark, TAB ) );
			  assertEquals( "va\"lue\" three", _seeker.extract( _mark, _extractors.@string() ).value() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleSlashEncodedQuotesIfConfiguredWithLegacyStyleQuoting() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleSlashEncodedQuotesIfConfiguredWithLegacyStyleQuoting()
		 {
			  // GIVEN
			  _seeker = _seeker( "\"value \\\"one\\\"\"\t\"\\\"value\\\" two\"\t\"va\\\"lue\\\" three\"", WithLegacyStyleQuoting( Config(), true ) );

			  // WHEN/THEN
			  assertTrue( _seeker.seek( _mark, TAB ) );
			  assertEquals( "value \"one\"", _seeker.extract( _mark, _extractors.@string() ).value() );

			  assertTrue( _seeker.seek( _mark, TAB ) );
			  assertEquals( "\"value\" two", _seeker.extract( _mark, _extractors.@string() ).value() );

			  assertTrue( _seeker.seek( _mark, TAB ) );
			  assertEquals( "va\"lue\" three", _seeker.extract( _mark, _extractors.@string() ).value() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecognizeStrayQuoteCharacters() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecognizeStrayQuoteCharacters()
		 {
			  // GIVEN
			  _seeker = _seeker( "one,two\",th\"ree\n" + "four,five,s\"ix" );

			  // THEN
			  AssertNextValue( _seeker, _mark, COMMA, "one" );
			  AssertNextValue( _seeker, _mark, COMMA, "two\"" );
			  AssertNextValue( _seeker, _mark, COMMA, "th\"ree" );
			  assertTrue( _mark.EndOfLine );
			  AssertNextValue( _seeker, _mark, COMMA, "four" );
			  AssertNextValue( _seeker, _mark, COMMA, "five" );
			  AssertNextValue( _seeker, _mark, COMMA, "s\"ix" );
			  AssertEnd( _seeker, _mark, COMMA );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotMisinterpretUnfilledRead() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotMisinterpretUnfilledRead()
		 {
			  // GIVEN
			  CharReadable readable = new ControlledCharReadable( "123,456,789\n" + "abc,def,ghi", 5 );
			  _seeker = _seeker( readable );

			  // WHEN/THEN
			  AssertNextValue( _seeker, _mark, COMMA, "123" );
			  AssertNextValue( _seeker, _mark, COMMA, "456" );
			  AssertNextValue( _seeker, _mark, COMMA, "789" );
			  assertTrue( _mark.EndOfLine );
			  AssertNextValue( _seeker, _mark, COMMA, "abc" );
			  AssertNextValue( _seeker, _mark, COMMA, "def" );
			  AssertNextValue( _seeker, _mark, COMMA, "ghi" );
			  AssertEnd( _seeker, _mark, COMMA );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFindAnyValuesForEmptySource() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFindAnyValuesForEmptySource()
		 {
			  // GIVEN
			  _seeker = _seeker( "" );

			  // WHEN/THEN
			  assertFalse( _seeker.seek( _mark, COMMA ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeQuotesInQuotes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSeeQuotesInQuotes()
		 {
			  // GIVEN
			  //                4,     """",   "f\oo"
			  _seeker = _seeker( "4,\"\"\"\",\"f\\oo\"" );

			  // WHEN/THEN
			  AssertNextValue( _seeker, _mark, COMMA, "4" );
			  AssertNextValue( _seeker, _mark, COMMA, "\"" );
			  AssertNextValue( _seeker, _mark, COMMA, "f\\oo" );
			  assertFalse( _seeker.seek( _mark, COMMA ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEscapeBackslashesInQuotesIfConfiguredWithLegacyStyleQuoting() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldEscapeBackslashesInQuotesIfConfiguredWithLegacyStyleQuoting()
		 {
			  // GIVEN
			  //                4,    "\\\"",   "f\oo"
			  _seeker = _seeker( "4,\"\\\\\\\"\",\"f\\oo\"", WithLegacyStyleQuoting( Config(), true ) );

			  // WHEN/THEN
			  AssertNextValue( _seeker, _mark, COMMA, "4" );
			  AssertNextValue( _seeker, _mark, COMMA, "\\\"" );
			  AssertNextValue( _seeker, _mark, COMMA, "f\\oo" );
			  assertFalse( _seeker.seek( _mark, COMMA ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListenToMusic() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListenToMusic()
		 {
			  // GIVEN
			  string data = "\"1\",\"ABBA\",\"1992\"\n" +
						 "\"2\",\"Roxette\",\"1986\"\n" +
						 "\"3\",\"Europe\",\"1979\"\n" +
						 "\"4\",\"The Cardigans\",\"1992\"";
			  _seeker = _seeker( data );

			  // WHEN
			  AssertNextValue( _seeker, _mark, COMMA, "1" );
			  AssertNextValue( _seeker, _mark, COMMA, "ABBA" );
			  AssertNextValue( _seeker, _mark, COMMA, "1992" );
			  assertTrue( _mark.EndOfLine );
			  AssertNextValue( _seeker, _mark, COMMA, "2" );
			  AssertNextValue( _seeker, _mark, COMMA, "Roxette" );
			  AssertNextValue( _seeker, _mark, COMMA, "1986" );
			  assertTrue( _mark.EndOfLine );
			  AssertNextValue( _seeker, _mark, COMMA, "3" );
			  AssertNextValue( _seeker, _mark, COMMA, "Europe" );
			  AssertNextValue( _seeker, _mark, COMMA, "1979" );
			  assertTrue( _mark.EndOfLine );
			  AssertNextValue( _seeker, _mark, COMMA, "4" );
			  AssertNextValue( _seeker, _mark, COMMA, "The Cardigans" );
			  AssertNextValue( _seeker, _mark, COMMA, "1992" );
			  AssertEnd( _seeker, _mark, COMMA );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnCharactersAfterEndQuote() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailOnCharactersAfterEndQuote()
		 {
			  // GIVEN
			  string data = "abc,\"def\"ghi,jkl";
			  _seeker = _seeker( data );

			  // WHEN
			  AssertNextValue( _seeker, _mark, COMMA, "abc" );
			  try
			  {
					_seeker.seek( _mark, COMMA );
					fail( "Should've failed" );
			  }
			  catch ( DataAfterQuoteException e )
			  {
					// THEN good
					assertEquals( TEST_SOURCE, e.Source().sourceDescription() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseMultilineFieldWhereEndQuoteIsOnItsOwnLineSingleCharNewline() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParseMultilineFieldWhereEndQuoteIsOnItsOwnLineSingleCharNewline()
		 {
			  ShouldParseMultilineFieldWhereEndQuoteIsOnItsOwnLine( "\n" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseMultilineFieldWhereEndQuoteIsOnItsOwnLinePlatformNewline() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParseMultilineFieldWhereEndQuoteIsOnItsOwnLinePlatformNewline()
		 {
			  ShouldParseMultilineFieldWhereEndQuoteIsOnItsOwnLine( "%n" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailOnReadingFieldLargerThanBufferSize() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailOnReadingFieldLargerThanBufferSize()
		 {
			  // GIVEN
			  string data = Lines( "\n", "a,b,c", "d,e,f", "\"g,h,i", "abcdefghijlkmopqrstuvwxyz,l,m" );
			  _seeker = _seeker( data, WithMultilineFields( Config( 20 ), true ) );

			  // WHEN
			  AssertNextValue( _seeker, _mark, COMMA, "a" );
			  AssertNextValue( _seeker, _mark, COMMA, "b" );
			  AssertNextValue( _seeker, _mark, COMMA, "c" );
			  assertTrue( _mark.EndOfLine );
			  AssertNextValue( _seeker, _mark, COMMA, "d" );
			  AssertNextValue( _seeker, _mark, COMMA, "e" );
			  AssertNextValue( _seeker, _mark, COMMA, "f" );
			  assertTrue( _mark.EndOfLine );

			  // THEN
			  try
			  {
					_seeker.seek( _mark, COMMA );
					fail( "Should have failed" );
			  }
			  catch ( System.InvalidOperationException e )
			  {
					// Good
					string source = _seeker.sourceDescription();
					assertTrue( e.Message.contains( "Tried to read" ) );
					assertTrue( e.Message.contains( source + ":3" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotInterpretBackslashQuoteDifferentlyIfDisabledLegacyStyleQuoting() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotInterpretBackslashQuoteDifferentlyIfDisabledLegacyStyleQuoting()
		 {
			  // GIVEN data with the quote character ' for easier readability
			  char slash = '\\';
			  string data = Lines( "\n", "'abc''def" + slash + "''ghi'" );
			  _seeker = _seeker( data, WithLegacyStyleQuoting( WithQuoteCharacter( Config(), '\'' ), false ) );

			  // WHEN/THEN
			  AssertNextValue( _seeker, _mark, COMMA, "abc'def" + slash + "'ghi" );
			  assertFalse( _seeker.seek( _mark, COMMA ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shouldParseMultilineFieldWhereEndQuoteIsOnItsOwnLine(String newline) throws Exception
		 private void ShouldParseMultilineFieldWhereEndQuoteIsOnItsOwnLine( string newline )
		 {
			  // GIVEN
			  string data = Lines( newline, "1,\"Bar\"", "2,\"Bar", "", "Quux", "\"", "3,\"Bar", "", "Quux\"", "" );
			  _seeker = _seeker( data, WithMultilineFields( Config(), true ) );

			  // THEN
			  AssertNextValue( _seeker, _mark, COMMA, "1" );
			  AssertNextValue( _seeker, _mark, COMMA, "Bar" );
			  AssertNextValue( _seeker, _mark, COMMA, "2" );
			  AssertNextValue( _seeker, _mark, COMMA, Lines( newline, "Bar", "", "Quux", "" ) );
			  AssertNextValue( _seeker, _mark, COMMA, "3" );
			  AssertNextValue( _seeker, _mark, COMMA, Lines( newline, "Bar", "", "Quux" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTrimWhitespace() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTrimWhitespace()
		 {
			  // given
			  string data = Lines( "\n", "Foo, Bar,  Twobar , \"Baz\" , \" Quux \",\"Wiii \" , Waaaa  " );

			  // when
			  _seeker = _seeker( data, WithTrimStrings( Config(), true ) );

			  // then
			  AssertNextValue( _seeker, _mark, COMMA, "Foo" );
			  AssertNextValue( _seeker, _mark, COMMA, "Bar" );
			  AssertNextValue( _seeker, _mark, COMMA, "Twobar" );
			  AssertNextValue( _seeker, _mark, COMMA, "Baz" );
			  AssertNextValue( _seeker, _mark, COMMA, " Quux " );
			  AssertNextValue( _seeker, _mark, COMMA, "Wiii " );
			  AssertNextValue( _seeker, _mark, COMMA, "Waaaa" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTrimStringsWithFirstLineCharacterSpace() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTrimStringsWithFirstLineCharacterSpace()
		 {
			  // given
			  string line = " ,a, ,b, ";
			  _seeker = _seeker( line, WithTrimStrings( Config(), true ) );

			  // when/then
			  AssertNextValueNotExtracted( _seeker, _mark, COMMA );
			  AssertNextValue( _seeker, _mark, COMMA, "a" );
			  AssertNextValueNotExtracted( _seeker, _mark, COMMA );
			  AssertNextValue( _seeker, _mark, COMMA, "b" );
			  AssertNextValueNotExtracted( _seeker, _mark, COMMA );
			  AssertEnd( _seeker, _mark, COMMA );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseAndTrimRandomStrings() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParseAndTrimRandomStrings()
		 {
			  // given
			  StringBuilder builder = new StringBuilder();
			  int columns = _random.Next( 10 ) + 5;
			  int lines = 100;
			  IList<string> expected = new List<string>();
			  char delimiter = RandomDelimiter();
			  for ( int i = 0; i < lines; i++ )
			  {
					for ( int j = 0; j < columns; j++ )
					{
						 if ( j > 0 )
						 {
							  if ( _random.nextBoolean() )
							  {
									// Space before delimiter
									builder.Append( RandomWhitespace( delimiter ) );
							  }
							  builder.Append( delimiter );
							  if ( _random.nextBoolean() )
							  {
									// Space before delimiter
									builder.Append( RandomWhitespace( delimiter ) );
							  }
						 }
						 bool quote = _random.nextBoolean();
						 if ( _random.nextBoolean() )
						 {
							  string value = "";
							  if ( quote )
							  {
									// Quote
									if ( _random.nextBoolean() )
									{
										 // Space after quote start
										 value += RandomWhitespace( delimiter );
									}
							  }
							  // Actual value
							  value += _random.Next().ToString();
							  if ( quote )
							  {
									if ( _random.nextBoolean() )
									{
										 // Space before quote end
										 value += RandomWhitespace( delimiter );
									}
							  }
							  expected.Add( value );
							  builder.Append( quote ? "\"" + value + "\"" : value );
						 }
						 else
						 {
							  expected.Add( null );
						 }
					}
					builder.Append( format( "%n" ) );
			  }
			  string data = builder.ToString();
			  _seeker = _seeker( data, WithTrimStrings( Config(), true ) );

			  // when
			  IEnumerator<string> next = expected.GetEnumerator();
			  for ( int i = 0; i < lines; i++ )
			  {
					for ( int j = 0; j < columns; j++ )
					{
						 // then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 string nextExpected = next.next();
						 if ( string.ReferenceEquals( nextExpected, null ) )
						 {
							  AssertNextValueNotExtracted( _seeker, _mark, delimiter );
						 }
						 else
						 {
							  AssertNextValue( _seeker, _mark, delimiter, nextExpected );
						 }
					}
			  }
			  AssertEnd( _seeker, _mark, delimiter );
		 }

		 private char RandomDelimiter()
		 {
			  return _delimiterChars[_random.Next( _delimiterChars.Length )];
		 }

		 private char RandomWhitespace( char except )
		 {
			  char ch;
			  do
			  {
					ch = _whitespaceChars[_random.Next( _whitespaceChars.Length )];
			  } while ( ch == except );
			  return ch;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseNonLatinCharacters() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldParseNonLatinCharacters()
		 {
			  // given
			  IList<string[]> expected = new IList<string[]> { array( "普通�?/普通話", "\uD83D\uDE21" ), array( "\uD83D\uDE21\uD83D\uDCA9\uD83D\uDC7B", "ⲹ楡�?톜ഷۢ⼈�?�늉�?�₭샺ጚ砧攡跿家䯶�?⬖�?�犽ۼ" ), array( " 㺂�?鋦毠", ";먵�?裬岰鷲趫\uA8C5얱㓙髿ᚳᬼ≩�?� " ) };
			  string data = Lines( format( "%n" ), expected );

			  // when
			  _seeker = _seeker( data );

			  // then
			  foreach ( string[] line in expected )
			  {
					foreach ( string cell in line )
					{
						 AssertNextValue( _seeker, _mark, COMMA, cell );
					}
			  }
			  AssertEnd( _seeker, _mark, COMMA );
		 }

		 private string Lines( string newline, IList<string[]> cells )
		 {
			  string[] lines = new string[cells.Count];
			  int i = 0;
			  foreach ( string[] columns in cells )
			  {
					lines[i++] = StringUtils.join( columns, "," );
			  }
			  return lines( newline, lines );
		 }

		 private string Lines( string newline, params string[] lines )
		 {
			  StringBuilder builder = new StringBuilder();
			  foreach ( string line in lines )
			  {
					if ( builder.Length > 0 )
					{
						 builder.Append( format( newline ) );
					}
					builder.Append( line );
			  }
			  return builder.ToString();
		 }

		 private string[][] RandomWeirdValues( int cols, int rows, params char[] except )
		 {
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: string[][] data = new string[rows][cols];
			  string[][] data = RectangularArrays.RectangularStringArray( rows, cols );
			  for ( int row = 0; row < rows; row++ )
			  {
					for ( int col = 0; col < cols; col++ )
					{
						 data[row][col] = RandomWeirdValue( except );
					}
			  }
			  return data;
		 }

		 private string RandomWeirdValue( params char[] except )
		 {
			  int length = _random.Next( 10 ) + 5;
			  char[] chars = new char[length];
			  for ( int i = 0; i < length; i++ )
			  {
					chars[i] = RandomWeirdChar( except );
			  }
			  return new string( chars );
		 }

		 private char RandomWeirdChar( params char[] except )
		 {
			  while ( true )
			  {
					char candidate = ( char ) _random.Next( char.MaxValue );
					if ( !In( candidate, except ) )
					{
						 return candidate;
					}
			  }
		 }

		 private bool In( char candidate, char[] set )
		 {
			  foreach ( char ch in set )
			  {
					if ( ch == candidate )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 private string Join( string[][] data, char delimiter )
		 {
			  string delimiterString = delimiter.ToString();
			  StringBuilder builder = new StringBuilder();
			  foreach ( string[] line in data )
			  {
					for ( int i = 0; i < line.Length; i++ )
					{
						 builder.Append( i > 0 ? delimiterString : "" ).Append( line[i] );
					}
					builder.Append( "\n" );
			  }
			  return builder.ToString();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertNextValue(CharSeeker seeker, Mark mark, int delimiter, String expectedValue) throws java.io.IOException
		 private void AssertNextValue( CharSeeker seeker, Mark mark, int delimiter, string expectedValue )
		 {
			  assertTrue( seeker.Seek( mark, delimiter ) );
			  assertEquals( expectedValue, seeker.Extract( mark, _extractors.@string() ).value() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertNextValueNotExtracted(CharSeeker seeker, Mark mark, int delimiter) throws java.io.IOException
		 private void AssertNextValueNotExtracted( CharSeeker seeker, Mark mark, int delimiter )
		 {
			  assertTrue( seeker.Seek( mark, delimiter ) );
			  assertFalse( seeker.TryExtract( mark, _extractors.@string() ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertEnd(CharSeeker seeker, Mark mark, int delimiter) throws java.io.IOException
		 private void AssertEnd( CharSeeker seeker, Mark mark, int delimiter )
		 {
			  assertTrue( mark.EndOfLine );
			  assertFalse( seeker.Seek( mark, delimiter ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String[] nextLineOfAllStrings(CharSeeker seeker, Mark mark) throws java.io.IOException
		 private string[] NextLineOfAllStrings( CharSeeker seeker, Mark mark )
		 {
			  IList<string> line = new List<string>();
			  while ( seeker.Seek( mark, COMMA ) )
			  {
					line.Add( seeker.Extract( mark, _extractors.@string() ).value() );
					if ( mark.EndOfLine )
					{
						 break;
					}
			  }
			  return line.ToArray();
		 }

		 private CharSeeker Seeker( CharReadable readable )
		 {
			  return Seeker( readable, Config() );
		 }

		 private CharSeeker Seeker( CharReadable readable, Configuration config )
		 {
			  return charSeeker( readable, config, _useThreadAhead );
		 }

		 private CharSeeker Seeker( string data )
		 {
			  return Seeker( data, Config() );
		 }

		 private CharSeeker Seeker( string data, Configuration config )
		 {
			  return seeker( wrap( StringReaderWithName( data, TEST_SOURCE ), data.Length * 2 ), config );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private java.io.Reader stringReaderWithName(String data, final String name)
		 private Reader StringReaderWithName( string data, string name )
		 {
			  return new StringReaderAnonymousInnerClass( this, data, name );
		 }

		 private class StringReaderAnonymousInnerClass : StringReader
		 {
			 private readonly BufferedCharSeekerTest _outerInstance;

			 private string _name;

			 public StringReaderAnonymousInnerClass( BufferedCharSeekerTest outerInstance, string data, string name ) : base( data )
			 {
				 this.outerInstance = outerInstance;
				 this._name = name;
			 }

			 public override string ToString()
			 {
				  return _name;
			 }
		 }

		 private static Configuration Config()
		 {
			  return Config( 1_000 );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static Configuration config(final int bufferSize)
		 private static Configuration Config( int bufferSize )
		 {
			  return new Configuration_OverriddenAnonymousInnerClass( Configuration_Fields.Default, bufferSize );
		 }

		 private class Configuration_OverriddenAnonymousInnerClass : Configuration_Overridden
		 {
			 private int _bufferSize;

			 public Configuration_OverriddenAnonymousInnerClass( Org.Neo4j.Csv.Reader.Configuration configurationFields, int bufferSize ) : base( configurationFields.Default )
			 {
				 this._bufferSize = bufferSize;
			 }

			 public override int bufferSize()
			 {
				  return _bufferSize;
			 }
		 }

		 private static Configuration WithMultilineFields( Configuration config, bool multiline )
		 {
			  return new Configuration_OverriddenAnonymousInnerClass2( config, multiline );
		 }

		 private class Configuration_OverriddenAnonymousInnerClass2 : Configuration_Overridden
		 {
			 private bool _multiline;

			 public Configuration_OverriddenAnonymousInnerClass2( Org.Neo4j.Csv.Reader.Configuration config, bool multiline ) : base( config )
			 {
				 this._multiline = multiline;
			 }

			 public override bool multilineFields()
			 {
				  return _multiline;
			 }
		 }

		 private static Configuration WithLegacyStyleQuoting( Configuration config, bool legacyStyleQuoting )
		 {
			  return new Configuration_OverriddenAnonymousInnerClass3( config, legacyStyleQuoting );
		 }

		 private class Configuration_OverriddenAnonymousInnerClass3 : Configuration_Overridden
		 {
			 private bool _legacyStyleQuoting;

			 public Configuration_OverriddenAnonymousInnerClass3( Org.Neo4j.Csv.Reader.Configuration config, bool legacyStyleQuoting ) : base( config )
			 {
				 this._legacyStyleQuoting = legacyStyleQuoting;
			 }

			 public override bool legacyStyleQuoting()
			 {
				  return _legacyStyleQuoting;
			 }
		 }

		 private static Configuration WithQuoteCharacter( Configuration config, char quoteCharacter )
		 {
			  return new Configuration_OverriddenAnonymousInnerClass4( config, quoteCharacter );
		 }

		 private class Configuration_OverriddenAnonymousInnerClass4 : Configuration_Overridden
		 {
			 private char _quoteCharacter;

			 public Configuration_OverriddenAnonymousInnerClass4( Org.Neo4j.Csv.Reader.Configuration config, char quoteCharacter ) : base( config )
			 {
				 this._quoteCharacter = quoteCharacter;
			 }

			 public override char quotationCharacter()
			 {
				  return _quoteCharacter;
			 }
		 }

		 private static Configuration WithTrimStrings( Configuration config, bool trimStrings )
		 {
			  return new Configuration_OverriddenAnonymousInnerClass5( config, trimStrings );
		 }

		 private class Configuration_OverriddenAnonymousInnerClass5 : Configuration_Overridden
		 {
			 private bool _trimStrings;

			 public Configuration_OverriddenAnonymousInnerClass5( Org.Neo4j.Csv.Reader.Configuration config, bool trimStrings ) : base( config )
			 {
				 this._trimStrings = trimStrings;
			 }

			 public override bool trimStrings()
			 {
				  return _trimStrings;
			 }
		 }

		 private class ControlledCharReadable : CharReadable_Adapter
		 {
			  internal readonly StringReader Reader;
			  internal readonly int MaxBytesPerRead;
			  internal readonly string Data;

			  internal ControlledCharReadable( string data, int maxBytesPerRead )
			  {
					this.Data = data;
					this.Reader = new StringReader( data );
					this.MaxBytesPerRead = maxBytesPerRead;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public SectionedCharBuffer read(SectionedCharBuffer buffer, int from) throws java.io.IOException
			  public override SectionedCharBuffer Read( SectionedCharBuffer buffer, int from )
			  {
					buffer.Compact( buffer, from );
					buffer.ReadFrom( Reader, MaxBytesPerRead );
					return buffer;
			  }

			  public override int Read( char[] into, int offset, int length )
			  {
					throw new System.NotSupportedException();
			  }

			  public override long Position()
			  {
					return 0;
			  }

			  public override string SourceDescription()
			  {
					return this.GetType().Name;
			  }

			  public override long Length()
			  {
					return Data.Length * 2;
			  }
		 }
	}

}