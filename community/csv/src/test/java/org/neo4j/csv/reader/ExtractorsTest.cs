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
	using Test = org.junit.jupiter.api.Test;

	using IntExtractor = Org.Neo4j.Csv.Reader.Extractors.IntExtractor;
	using CoordinateReferenceSystem = Org.Neo4j.Values.Storable.CoordinateReferenceSystem;
	using PointValue = Org.Neo4j.Values.Storable.PointValue;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;

	internal class ExtractorsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldExtractStringArray()
		 internal virtual void ShouldExtractStringArray()
		 {
			  // GIVEN
			  Extractors extractors = new Extractors( ',' );
			  string data = "abcde,fghijkl,mnopq";

			  // WHEN
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") Extractor<String[]> extractor = (Extractor<String[]>) extractors.valueOf("STRING[]");
			  Extractor<string[]> extractor = ( Extractor<string[]> ) extractors.ValueOf( "STRING[]" );
			  extractor.Extract( data.ToCharArray(), 0, data.Length, false );

			  // THEN
			  assertArrayEquals( new string[] { "abcde", "fghijkl", "mnopq" }, extractor.Value() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldExtractLongArray()
		 internal virtual void ShouldExtractLongArray()
		 {
			  // GIVEN
			  Extractors extractors = new Extractors( ',' );
			  long[] longData = new long[] { 123, 4567, 987654321 };
			  string data = ToString( longData, ',' );

			  // WHEN
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") Extractor<long[]> extractor = (Extractor<long[]>) extractors.valueOf("long[]");
			  Extractor<long[]> extractor = ( Extractor<long[]> ) extractors.ValueOf( "long[]" );
			  extractor.Extract( data.ToCharArray(), 0, data.Length, false );

			  // THEN
			  assertArrayEquals( longData, extractor.Value() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldExtractBooleanArray()
		 internal virtual void ShouldExtractBooleanArray()
		 {
			  // GIVEN
			  Extractors extractors = new Extractors( ',' );
			  bool[] booleanData = new bool[] { true, false, true };
			  string data = ToString( booleanData, ',' );

			  // WHEN
			  Extractor<bool[]> extractor = extractors.BooleanArray();
			  extractor.Extract( data.ToCharArray(), 0, data.Length, false );

			  // THEN
			  AssertBooleanArrayEquals( booleanData, extractor.Value() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldExtractDoubleArray()
		 internal virtual void ShouldExtractDoubleArray()
		 {
			  // GIVEN
			  Extractors extractors = new Extractors( ',' );
			  double[] doubleData = new double[] { 123.123, 4567.4567, 987654321.0987 };
			  string data = ToString( doubleData, ',' );

			  // WHEN
			  Extractor<double[]> extractor = extractors.DoubleArray();
			  extractor.Extract( data.ToCharArray(), 0, data.Length, false );

			  // THEN
			  assertArrayEquals( doubleData, extractor.Value(), 0.001 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailExtractingLongArrayWhereAnyValueIsEmpty()
		 internal virtual void ShouldFailExtractingLongArrayWhereAnyValueIsEmpty()
		 {
			  // GIVEN
			  Extractors extractors = new Extractors( ';' );
			  long[] longData = new long[] { 112233, 4455, 66778899 };
			  string data = ToString( longData, ';' ) + ";";

			  // WHEN extracting long[] from "<number>;<number>...;" i.e. ending with a delimiter
			  assertThrows( typeof( System.FormatException ), () => extractors.LongArray().extract(data.ToCharArray(), 0, data.Length, false) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldFailExtractingLongArrayWhereAnyValueIsntReallyANumber()
		 internal virtual void ShouldFailExtractingLongArrayWhereAnyValueIsntReallyANumber()
		 {
			  // GIVEN
			  Extractors extractors = new Extractors( ';' );

			  // WHEN extracting long[] from "<number>;<number>...;" i.e. ending with a delimiter
			  string data = "123;456;abc;789";
			  assertThrows( typeof( System.FormatException ), () => extractors.ValueOf("long[]").extract(data.ToCharArray(), 0, data.Length, false) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldExtractPoint()
		 internal virtual void ShouldExtractPoint()
		 {
			  // GIVEN
			  Extractors extractors = new Extractors( ',' );
			  PointValue value = Values.pointValue( CoordinateReferenceSystem.WGS84, 13.2, 56.7 );

			  // WHEN
			  char[] asChars = "Point{latitude: 56.7, longitude: 13.2}".ToCharArray();
			  Extractors.PointExtractor extractor = extractors.Point();
			  string headerInfo = "{crs:WGS-84}";
			  extractor.Extract( asChars, 0, asChars.Length, false, PointValue.parseHeaderInformation( headerInfo ) );

			  // THEN
			  assertEquals( value, extractor.ValueConflict );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldExtractNegativeInt()
		 internal virtual void ShouldExtractNegativeInt()
		 {
			  // GIVEN
			  Extractors extractors = new Extractors( ',' );
			  int value = -1234567;

			  // WHEN
			  char[] asChars = value.ToString().ToCharArray();
			  IntExtractor extractor = extractors.Int_();
			  extractor.Extract( asChars, 0, asChars.Length, false );

			  // THEN
			  assertEquals( value, extractor.IntValue() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldExtractEmptyStringForEmptyArrayString()
		 internal virtual void ShouldExtractEmptyStringForEmptyArrayString()
		 {
			  // GIVEN
			  Extractors extractors = new Extractors( ',' );
			  string value = "";

			  // WHEN
			  Extractor<string[]> extractor = extractors.StringArray();
			  extractor.Extract( value.ToCharArray(), 0, value.Length, false );

			  // THEN
			  assertEquals( 0, extractor.Value().Length );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldExtractEmptyLongArrayForEmptyArrayString()
		 internal virtual void ShouldExtractEmptyLongArrayForEmptyArrayString()
		 {
			  // GIVEN
			  Extractors extractors = new Extractors( ',' );
			  string value = "";

			  // WHEN
			  Extractor<long[]> extractor = extractors.LongArray();
			  extractor.Extract( value.ToCharArray(), 0, value.Length, false );

			  // THEN
			  assertEquals( 0, extractor.Value().Length );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldExtractTwoEmptyStringsForSingleDelimiterInArrayString()
		 internal virtual void ShouldExtractTwoEmptyStringsForSingleDelimiterInArrayString()
		 {
			  // GIVEN
			  Extractors extractors = new Extractors( ',' );
			  string value = ",";

			  // WHEN
			  Extractor<string[]> extractor = extractors.StringArray();
			  extractor.Extract( value.ToCharArray(), 0, value.Length, false );

			  // THEN
			  assertArrayEquals( new string[] { "", "" }, extractor.Value() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldExtractEmptyStringForEmptyQuotedString()
		 internal virtual void ShouldExtractEmptyStringForEmptyQuotedString()
		 {
			  // GIVEN
			  Extractors extractors = new Extractors( ',' );
			  string value = "";

			  // WHEN
			  Extractor<string> extractor = extractors.String();
			  extractor.Extract( value.ToCharArray(), 0, value.Length, true );

			  // THEN
			  assertEquals( "", extractor.Value() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldExtractNullForEmptyQuotedStringIfConfiguredTo()
		 internal virtual void ShouldExtractNullForEmptyQuotedStringIfConfiguredTo()
		 {
			  // GIVEN
			  Extractors extractors = new Extractors( ';', true );
			  Extractor<string> extractor = extractors.String();

			  // WHEN
			  extractor.Extract( new char[0], 0, 0, true );
			  string extracted = extractor.Value();

			  // THEN
			  assertNull( extracted );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldTrimStringArrayIfConfiguredTo()
		 internal virtual void ShouldTrimStringArrayIfConfiguredTo()
		 {
			  // GIVEN
			  Extractors extractors = new Extractors( ';', true, true );
			  string value = "ab;cd ; ef; gh ";

			  // WHEN
			  char[] asChars = value.ToCharArray();
			  Extractor<string[]> extractor = extractors.StringArray();
			  extractor.Extract( asChars, 0, asChars.Length, true );

			  // THEN
			  assertArrayEquals( new string[] { "ab", "cd", "ef", "gh" }, extractor.Value() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotTrimStringIfNotConfiguredTo()
		 internal virtual void ShouldNotTrimStringIfNotConfiguredTo()
		 {
			  // GIVEN
			  Extractors extractors = new Extractors( ';', true, false );
			  string value = "ab;cd ; ef; gh ";

			  // WHEN
			  char[] asChars = value.ToCharArray();
			  Extractor<string[]> extractor = extractors.StringArray();
			  extractor.Extract( asChars, 0, asChars.Length, true );

			  // THEN
			  assertArrayEquals( new string[] { "ab", "cd ", " ef", " gh " }, extractor.Value() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCloneExtractor()
		 internal virtual void ShouldCloneExtractor()
		 {
			  // GIVEN
			  Extractors extractors = new Extractors( ';' );
			  Extractor<string> e1 = extractors.String();
			  Extractor<string> e2 = e1.Clone();

			  // WHEN
			  string v1 = "abc";
			  e1.Extract( v1.ToCharArray(), 0, v1.Length, false );
			  assertEquals( v1, e1.Value() );
			  assertNull( e2.Value() );

			  // THEN
			  string v2 = "def";
			  e2.Extract( v2.ToCharArray(), 0, v2.Length, false );
			  assertEquals( v2, e2.Value() );
			  assertEquals( v1, e1.Value() );
		 }

		 private static string ToString( long[] values, char delimiter )
		 {
			  StringBuilder builder = new StringBuilder();
			  foreach ( long value in values )
			  {
					builder.Append( builder.Length > 0 ? delimiter : "" ).Append( value );
			  }
			  return builder.ToString();
		 }

		 private static string ToString( double[] values, char delimiter )
		 {
			  StringBuilder builder = new StringBuilder();
			  foreach ( double value in values )
			  {
					builder.Append( builder.Length > 0 ? delimiter : "" ).Append( value );
			  }
			  return builder.ToString();
		 }

		 private static string ToString( bool[] values, char delimiter )
		 {
			  StringBuilder builder = new StringBuilder();
			  foreach ( bool value in values )
			  {
					builder.Append( builder.Length > 0 ? delimiter : "" ).Append( value );
			  }
			  return builder.ToString();
		 }

		 private static void AssertBooleanArrayEquals( bool[] expected, bool[] values )
		 {
			  assertEquals( expected.Length, values.Length, "Array lengths differ" );
			  for ( int i = 0; i < expected.Length; i++ )
			  {
					assertEquals( expected[i], values[i], "Item " + i + " differs" );
			  }
		 }
	}

}