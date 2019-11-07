using System;

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
namespace Neo4Net.Values.Storable
{
	using Test = org.junit.jupiter.api.Test;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.StringsLibrary.STRINGS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.stringValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.utf8Value;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.utils.AnyValueTestUtil.assertEqual;

	internal class UTF8StringValueTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleDifferentTypesOfStrings()
		 internal virtual void ShouldHandleDifferentTypesOfStrings()
		 {
			  foreach ( string @string in STRINGS )
			  {
					TextValue stringValue = stringValue( @string );
					sbyte[] bytes = @string.GetBytes( UTF_8 );
					TextValue utf8 = utf8Value( bytes );
					assertEqual( stringValue, utf8 );
					assertThat( stringValue.Length(), equalTo(utf8.Length()) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldTrimDifferentTypesOfStrings()
		 internal virtual void ShouldTrimDifferentTypesOfStrings()
		 {
			  foreach ( string @string in STRINGS )
			  {
					TextValue stringValue = stringValue( @string );
					sbyte[] bytes = @string.GetBytes( UTF_8 );
					TextValue utf8 = utf8Value( bytes );
					AssertSame( stringValue.Trim(), utf8.Trim() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldLTrimDifferentTypesOfStrings()
		 internal virtual void ShouldLTrimDifferentTypesOfStrings()
		 {
			  foreach ( string @string in STRINGS )
			  {
					TextValue stringValue = stringValue( @string );
					sbyte[] bytes = @string.GetBytes( UTF_8 );
					TextValue utf8 = utf8Value( bytes );
					AssertSame( stringValue.Ltrim(), utf8.Ltrim() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void trimShouldBeSameAsLtrimAndRtrim()
		 internal virtual void TrimShouldBeSameAsLtrimAndRtrim()
		 {
			  foreach ( string @string in STRINGS )
			  {
					TextValue utf8 = utf8Value( @string.GetBytes( UTF_8 ) );
					AssertSame( utf8.Trim(), utf8.Ltrim().rtrim() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSubstring()
		 internal virtual void ShouldSubstring()
		 {
			  string @string = "ü";
			  TextValue utf8 = utf8Value( @string.GetBytes( UTF_8 ) );
			  assertThat( utf8.Substring( 0, 1 ).stringValue(), equalTo("ü") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRTrimDifferentTypesOfStrings()
		 internal virtual void ShouldRTrimDifferentTypesOfStrings()
		 {
			  foreach ( string @string in STRINGS )
			  {
					TextValue stringValue = stringValue( @string );
					sbyte[] bytes = @string.GetBytes( UTF_8 );
					TextValue utf8 = utf8Value( bytes );
					AssertSame( stringValue.Rtrim(), utf8.Rtrim() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCompareTo()
		 internal virtual void ShouldCompareTo()
		 {
			  foreach ( string string1 in STRINGS )
			  {
					foreach ( string string2 in STRINGS )
					{
						 AssertCompareTo( string1, string2 );
					}
			  }
		 }

		 internal static void AssertCompareTo( string string1, string string2 )
		 {
			  TextValue textValue1 = stringValue( string1 );
			  TextValue textValue2 = stringValue( string2 );
			  TextValue utf8Value1 = utf8Value( string1.GetBytes( UTF_8 ) );
			  TextValue utf8Value2 = utf8Value( string2.GetBytes( UTF_8 ) );
			  int a = textValue1.CompareTo( textValue2 );
			  int x = textValue1.CompareTo( utf8Value2 );
			  int y = utf8Value1.CompareTo( textValue2 );
			  int z = utf8Value1.CompareTo( utf8Value2 );

			  assertThat( Math.Sign( a ), equalTo( Math.Sign( x ) ) );
			  assertThat( Math.Sign( a ), equalTo( Math.Sign( y ) ) );
			  assertThat( Math.Sign( a ), equalTo( Math.Sign( z ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReverse()
		 internal virtual void ShouldReverse()
		 {
			  foreach ( string @string in STRINGS )
			  {
					TextValue stringValue = stringValue( @string );
					sbyte[] bytes = @string.GetBytes( UTF_8 );
					TextValue utf8 = utf8Value( bytes );
					AssertSame( stringValue.Reverse(), utf8.Reverse() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleOffset()
		 internal virtual void ShouldHandleOffset()
		 {
			  // Given
			  sbyte[] bytes = "abcdefg".GetBytes( UTF_8 );

			  // When
			  TextValue textValue = utf8Value( bytes, 3, 2 );

			  // Then
			  AssertSame( textValue, stringValue( "de" ) );
			  assertThat( textValue.Length(), equalTo(stringValue("de").length()) );
			  AssertSame( textValue.Reverse(), stringValue("ed") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleAdditionWithOffset()
		 internal virtual void ShouldHandleAdditionWithOffset()
		 {
			  // Given
			  sbyte[] bytes = "abcdefg".GetBytes( UTF_8 );

			  // When
			  UTF8StringValue a = ( UTF8StringValue ) utf8Value( bytes, 1, 2 );
			  UTF8StringValue b = ( UTF8StringValue ) utf8Value( bytes, 3, 3 );

			  // Then
			 AssertSame( a.Plus( a ), stringValue( "bcbc" ) );
			 AssertSame( a.Plus( b ), stringValue( "bcdef" ) );
			 AssertSame( b.Plus( a ), stringValue( "defbc" ) );
			 AssertSame( b.Plus( b ), stringValue( "defdef" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleAdditionWithOffsetAndNonAscii()
		 internal virtual void ShouldHandleAdditionWithOffsetAndNonAscii()
		 {
			  // Given, two characters that require three bytes each
			  sbyte[] bytes = "ⲹ楡".GetBytes( UTF_8 );

			  // When
			  UTF8StringValue a = ( UTF8StringValue ) utf8Value( bytes, 0, 3 );
			  UTF8StringValue b = ( UTF8StringValue ) utf8Value( bytes, 3, 3 );

			  // Then
			  AssertSame( a.Plus( a ), stringValue( "ⲹⲹ" ) );
			  AssertSame( a.Plus( b ), stringValue( "ⲹ楡" ) );
			  AssertSame( b.Plus( a ), stringValue( "楡ⲹ" ) );
			  AssertSame( b.Plus( b ), stringValue( "楡楡" ) );
		 }

		 private void AssertSame( TextValue lhs, TextValue rhs )
		 {
			  assertThat( format( "%s.length != %s.length", lhs, rhs ), lhs.Length(), equalTo(rhs.Length()) );
			  assertThat( format( "%s != %s", lhs, rhs ), lhs, equalTo( rhs ) );
			  assertThat( format( "%s != %s", rhs, lhs ), rhs, equalTo( lhs ) );
			  assertThat( format( "%s.hashCode != %s.hashCode", rhs, lhs ), lhs.GetHashCode(), equalTo(rhs.GetHashCode()) );
			  assertThat( format( "%s.hashCode64 != %s.hashCode64", rhs, lhs ), lhs.HashCode64(), equalTo(rhs.HashCode64()) );
			  assertThat( lhs, equalTo( rhs ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleTooLargeStartPointInSubstring()
		 internal virtual void ShouldHandleTooLargeStartPointInSubstring()
		 {
			  // Given
			  TextValue value = utf8Value( "hello".GetBytes( UTF_8 ) );

			  // When
			  TextValue substring = value.Substring( 8, -3 );

			  // Then
			  assertThat( substring, equalTo( StringValue.EMPTY ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleTooLargeLengthInSubstring()
		 internal virtual void ShouldHandleTooLargeLengthInSubstring()
		 {
			  // Given
			  TextValue value = utf8Value( "hello".GetBytes( UTF_8 ) );

			  // When
			  TextValue substring = value.Substring( 3, 73 );

			  // Then
			  assertThat( substring.StringValue(), equalTo("lo") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowOnNegativeStart()
		 internal virtual void ShouldThrowOnNegativeStart()
		 {
			  // Given
			  TextValue value = utf8Value( "hello".GetBytes( UTF_8 ) );

			  assertThrows( typeof( System.IndexOutOfRangeException ), () => value.Substring(-4, 7) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldThrowOnNegativeLength()
		 internal virtual void ShouldThrowOnNegativeLength()
		 {
			  // Given
			  TextValue value = utf8Value( "hello".GetBytes( UTF_8 ) );

			  assertThrows( typeof( System.IndexOutOfRangeException ), () => value.Substring(4, -7) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleStringPredicatesWithOffset()
		 internal virtual void ShouldHandleStringPredicatesWithOffset()
		 {
			  // Given
			  sbyte[] bytes = "abcdefghijklmnoprstuvxyzABCDEFGHIJKLMNOPRSTUVXYZ".GetBytes( UTF_8 );

			  for ( int offset = 0; offset <= bytes.Length; offset++ )
			  {
					for ( int length = 0; length < bytes.Length - offset; length++ )
					{
						 TextValue value = utf8Value( bytes, offset, length );

						 for ( int otherOffset = 0; otherOffset <= bytes.Length; otherOffset++ )
						 {
							  for ( int otherLength = 0; otherLength < bytes.Length - otherOffset; otherLength++ )
							  {
									TextValue other = utf8Value( bytes, otherOffset, otherLength );
									assertThat( value.StartsWith( other ), equalTo( otherLength == 0 || otherOffset == offset && otherLength <= length ) );
									assertThat( value.EndsWith( other ), equalTo( otherLength == 0 || otherOffset >= offset && otherLength == length + offset - otherOffset ) );
									assertThat( value.Contains( other ), equalTo( otherLength == 0 || otherOffset >= offset && otherLength <= length + offset - otherOffset ) );

							  }
						 }

					}
			  }
		 }
	}

}