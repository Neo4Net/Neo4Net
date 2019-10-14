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
namespace Neo4Net.Kernel.Impl.Api
{
	using RandomStringUtils = org.apache.commons.lang3.RandomStringUtils;
	using RandomUtils = org.apache.commons.lang3.RandomUtils;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;

	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.NO_VALUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

	public class IndexTextValueLengthValidatorTest
	{
		 private const int MAX_BYTE_LENGTH = 20_000;

		 private IndexTextValueLengthValidator _validator = new IndexTextValueLengthValidator( MAX_BYTE_LENGTH );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public readonly ExpectedException ExpectedException = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tooLongByteArrayIsNotAllowed()
		 public virtual void TooLongByteArrayIsNotAllowed()
		 {
			  int length = MAX_BYTE_LENGTH * 2;
			  ExpectedException.expect( typeof( System.ArgumentException ) );
			  ExpectedException.expectMessage( containsString( "Property value size is too large for index. Please see index documentation for limitations." ) );
			  _validator.validate( RandomUtils.NextBytes( length ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void tooLongStringIsNotAllowed()
		 public virtual void TooLongStringIsNotAllowed()
		 {
			  int length = MAX_BYTE_LENGTH * 2;
			  ExpectedException.expect( typeof( System.ArgumentException ) );
			  ExpectedException.expectMessage( containsString( "Property value size is too large for index. Please see index documentation for limitations." ) );
			  _validator.validate( String( length ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shortByteArrayIsValid()
		 public virtual void ShortByteArrayIsValid()
		 {
			  _validator.validate( RandomUtils.NextBytes( 3 ) );
			  _validator.validate( RandomUtils.NextBytes( 30 ) );
			  _validator.validate( RandomUtils.NextBytes( 300 ) );
			  _validator.validate( RandomUtils.NextBytes( 4303 ) );
			  _validator.validate( RandomUtils.NextBytes( 13234 ) );
			  _validator.validate( RandomUtils.NextBytes( MAX_BYTE_LENGTH ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shortStringIsValid()
		 public virtual void ShortStringIsValid()
		 {
			  _validator.validate( String( 3 ) );
			  _validator.validate( String( 30 ) );
			  _validator.validate( String( 300 ) );
			  _validator.validate( String( 4303 ) );
			  _validator.validate( String( 13234 ) );
			  _validator.validate( String( MAX_BYTE_LENGTH ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nullIsNotAllowed()
		 public virtual void NullIsNotAllowed()
		 {
			  ExpectedException.expect( typeof( System.ArgumentException ) );
			  ExpectedException.expectMessage( "Null value" );
			  _validator.validate( ( sbyte[] ) null );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nullValueIsNotAllowed()
		 public virtual void NullValueIsNotAllowed()
		 {
			  ExpectedException.expect( typeof( System.ArgumentException ) );
			  ExpectedException.expectMessage( "Null value" );
			  _validator.validate( ( Value ) null );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void noValueIsNotAllowed()
		 public virtual void NoValueIsNotAllowed()
		 {
			  ExpectedException.expect( typeof( System.ArgumentException ) );
			  ExpectedException.expectMessage( "Null value" );
			  _validator.validate( NO_VALUE );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void shouldFailOnTooLongNonLatinString()
		 public virtual void ShouldFailOnTooLongNonLatinString()
		 {
			  // given
			  sbyte[] facesBytes = StringOfEmojis( 1 );

			  // when
			  _validator.validate( stringValue( StringHelper.NewString( facesBytes, UTF_8 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSucceedOnReasonablyLongNonLatinString()
		 public virtual void ShouldSucceedOnReasonablyLongNonLatinString()
		 {
			  // given
			  sbyte[] facesBytes = StringOfEmojis( 0 );

			  // when
			  _validator.validate( stringValue( StringHelper.NewString( facesBytes, UTF_8 ) ) );
		 }

		 private sbyte[] StringOfEmojis( int beyondMax )
		 {
			  sbyte[] poutingFaceSymbol = "\uD83D\uDE21".GetBytes();
			  int count = MAX_BYTE_LENGTH / poutingFaceSymbol.Length + beyondMax;
			  sbyte[] facesBytes = new sbyte[poutingFaceSymbol.Length * count];
			  for ( int i = 0; i < count; i++ )
			  {
					Array.Copy( poutingFaceSymbol, 0, facesBytes, i * poutingFaceSymbol.Length, poutingFaceSymbol.Length );
			  }
			  return facesBytes;
		 }

		 private Value String( int length )
		 {
			  return stringValue( RandomStringUtils.randomAlphabetic( length ) );
		 }
	}

}