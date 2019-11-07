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
//	import static Neo4Net.values.storable.Values.EMPTY_STRING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.charValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.stringValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.@virtual.VirtualValues.list;

	internal class CharValueTest
	{
		 private static char[] _chars = new char[] { ' ', '楡', 'a', '7', 'Ö' };

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleDifferentTypesOfChars()
		 internal virtual void ShouldHandleDifferentTypesOfChars()
		 {
			  foreach ( char c in _chars )
			  {
					TextValue charValue = charValue( c );
					TextValue stringValue = stringValue( Convert.ToString( c ) );

					assertThat( charValue, equalTo( stringValue ) );
					assertThat( charValue.Length(), equalTo(stringValue.Length()) );
					assertThat( charValue.GetHashCode(), equalTo(stringValue.GetHashCode()) );
					assertThat( charValue.Split( Convert.ToString( c ) ), equalTo( stringValue.Split( Convert.ToString( c ) ) ) );
					assertThat( charValue.ToUpper(), equalTo(stringValue.ToUpper()) );
					assertThat( charValue.ToLower(), equalTo(stringValue.ToLower()) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSplit()
		 internal virtual void ShouldSplit()
		 {
			  CharValue charValue = charValue( 'a' );
			  assertThat( charValue.Split( "a" ), equalTo( list( EMPTY_STRING, EMPTY_STRING ) ) );
			  assertThat( charValue.Split( "A" ), equalTo( list( charValue ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldTrim()
		 internal virtual void ShouldTrim()
		 {
			  assertThat( charValue( 'a' ).Trim(), equalTo(charValue('a')) );
			  assertThat( charValue( ' ' ).Trim(), equalTo(EMPTY_STRING) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldLTrim()
		 internal virtual void ShouldLTrim()
		 {
			  assertThat( charValue( 'a' ).ltrim(), equalTo(charValue('a')) );
			  assertThat( charValue( ' ' ).ltrim(), equalTo(EMPTY_STRING) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldRTrim()
		 internal virtual void ShouldRTrim()
		 {
			  assertThat( charValue( 'a' ).rtrim(), equalTo(charValue('a')) );
			  assertThat( charValue( ' ' ).rtrim(), equalTo(EMPTY_STRING) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReverse()
		 internal virtual void ShouldReverse()
		 {
			  foreach ( char c in _chars )
			  {
					CharValue charValue = charValue( c );
					assertThat( charValue.Reverse(), equalTo(charValue) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReplace()
		 internal virtual void ShouldReplace()
		 {
			  assertThat( charValue( 'a' ).replace( "a", "a long string" ), equalTo( stringValue( "a long string" ) ) );
			  assertThat( charValue( 'a' ).replace( "b", "a long string" ), equalTo( charValue( 'a' ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSubstring()
		 internal virtual void ShouldSubstring()
		 {
			  assertThat( charValue( 'a' ).substring( 0, 1 ), equalTo( charValue( 'a' ) ) );
			  assertThat( charValue( 'a' ).substring( 1, 2 ), equalTo( EMPTY_STRING ) );
		 }
	}

}