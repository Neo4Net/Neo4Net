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
namespace Org.Neo4j.Kernel.Impl.Api
{
	using RandomStringUtils = org.apache.commons.lang3.RandomStringUtils;
	using RandomUtils = org.apache.commons.lang3.RandomUtils;
	using Test = org.junit.jupiter.api.Test;

	using Org.Neo4j.Kernel.impl.util;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.LuceneIndexValueValidator.INSTANCE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.LuceneIndexValueValidator.MAX_TERM_LENGTH;

	internal class LuceneIndexValueValidatorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void tooLongArrayIsNotAllowed()
		 internal virtual void TooLongArrayIsNotAllowed()
		 {
			  int length = MAX_TERM_LENGTH + 1;
			  System.ArgumentException iae = assertThrows( typeof( System.ArgumentException ), () => Validator.validate(RandomUtils.NextBytes(length)) );
			  assertThat( iae.Message, containsString( "Property value size is too large for index. Please see index documentation for limitations." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void stringOverExceedLimitNotAllowed()
		 internal virtual void StringOverExceedLimitNotAllowed()
		 {
			  int length = MAX_TERM_LENGTH * 2;
			  System.ArgumentException iae = assertThrows( typeof( System.ArgumentException ), () => Validator.validate(RandomStringUtils.randomAlphabetic(length)) );
			  assertThat( iae.Message, containsString( "Property value size is too large for index. Please see index documentation for limitations." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nullIsNotAllowed()
		 internal virtual void NullIsNotAllowed()
		 {
			  System.ArgumentException iae = assertThrows( typeof( System.ArgumentException ), () => Validator.validate(null) );
			  assertEquals( iae.Message, "Null value" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void numberIsValidValue()
		 internal virtual void NumberIsValidValue()
		 {
			  Validator.validate( 5 );
			  Validator.validate( 5.0d );
			  Validator.validate( 5.0f );
			  Validator.validate( 5L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shortArrayIsValidValue()
		 internal virtual void ShortArrayIsValidValue()
		 {
			  Validator.validate( new long[] { 1, 2, 3 } );
			  Validator.validate( RandomUtils.NextBytes( 200 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shortStringIsValidValue()
		 internal virtual void ShortStringIsValidValue()
		 {
			  Validator.validate( RandomStringUtils.randomAlphabetic( 5 ) );
			  Validator.validate( RandomStringUtils.randomAlphabetic( 10 ) );
			  Validator.validate( RandomStringUtils.randomAlphabetic( 250 ) );
			  Validator.validate( RandomStringUtils.randomAlphabetic( 450 ) );
			  Validator.validate( RandomStringUtils.randomAlphabetic( MAX_TERM_LENGTH ) );
		 }

		 // Meant to be overridden for tests that want to verify the same things, but for a different validator
		 private Validator<object> Validator
		 {
			 get
			 {
				  return @object => INSTANCE.validate( Values.of( @object ) );
			 }
		 }
	}

}