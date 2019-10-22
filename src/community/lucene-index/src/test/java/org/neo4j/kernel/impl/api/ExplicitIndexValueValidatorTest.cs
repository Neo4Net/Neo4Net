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
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.api.ExplicitIndexValueValidator.INSTANCE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.api.LuceneIndexValueValidator.MAX_TERM_LENGTH;

	internal class ExplicitIndexValueValidatorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nullIsNotAllowed()
		 internal virtual void NullIsNotAllowed()
		 {
			  System.ArgumentException iae = assertThrows( typeof( System.ArgumentException ), () => INSTANCE.validate(null) );
			  assertEquals( iae.Message, "Null value" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void stringOverExceedLimitNotAllowed()
		 internal virtual void StringOverExceedLimitNotAllowed()
		 {
			  int length = MAX_TERM_LENGTH * 2;
			  System.ArgumentException iae = assertThrows( typeof( System.ArgumentException ), () => INSTANCE.validate(RandomStringUtils.randomAlphabetic(length)) );
			  assertThat( iae.Message, containsString( "Property value size is too large for index. Please see index documentation for limitations." ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nullToStringIsNotAllowed()
		 internal virtual void NullToStringIsNotAllowed()
		 {
			  object testValue = mock( typeof( object ) );
			  when( testValue.ToString() ).thenReturn(null);
			  System.ArgumentException iae = assertThrows( typeof( System.ArgumentException ), () => INSTANCE.validate(testValue) );
			  assertThat( iae.Message, containsString( "has null toString" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void numberIsValidValue()
		 internal virtual void NumberIsValidValue()
		 {
			  INSTANCE.validate( 5 );
			  INSTANCE.validate( 5.0d );
			  INSTANCE.validate( 5.0f );
			  INSTANCE.validate( 5L );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shortStringIsValidValue()
		 internal virtual void ShortStringIsValidValue()
		 {
			  INSTANCE.validate( RandomStringUtils.randomAlphabetic( 5 ) );
			  INSTANCE.validate( RandomStringUtils.randomAlphabetic( 10 ) );
			  INSTANCE.validate( RandomStringUtils.randomAlphabetic( 250 ) );
			  INSTANCE.validate( RandomStringUtils.randomAlphabetic( 450 ) );
			  INSTANCE.validate( RandomStringUtils.randomAlphabetic( MAX_TERM_LENGTH ) );
		 }
	}

}