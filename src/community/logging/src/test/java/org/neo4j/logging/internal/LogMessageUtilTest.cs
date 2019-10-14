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
namespace Neo4Net.Logging.Internal
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.Internal.LogMessageUtil.slf4jToStringFormatPlaceholders;

	public class LogMessageUtilTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowWhenStringIsNull()
		 public virtual void ShouldThrowWhenStringIsNull()
		 {
			  try
			  {
					slf4jToStringFormatPlaceholders( null );
					fail( "Exception expected" );
			  }
			  catch ( System.NullReferenceException )
			  {
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDoNothingForEmptyString()
		 public virtual void ShouldDoNothingForEmptyString()
		 {
			  assertEquals( "", slf4jToStringFormatPlaceholders( "" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDoNothingForStringWithoutPlaceholders()
		 public virtual void ShouldDoNothingForStringWithoutPlaceholders()
		 {
			  assertEquals( "Simple log message", slf4jToStringFormatPlaceholders( "Simple log message" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReplaceSlf4jPlaceholderWithStringFormatPlaceholder()
		 public virtual void ShouldReplaceSlf4jPlaceholderWithStringFormatPlaceholder()
		 {
			  assertEquals( "Log message with %s single placeholder", slf4jToStringFormatPlaceholders( "Log message with {} single placeholder" ) );
			  assertEquals( "Log message %s with two %s placeholders", slf4jToStringFormatPlaceholders( "Log message {} with two {} placeholders" ) );
			  assertEquals( "Log %s message %s with three %s placeholders", slf4jToStringFormatPlaceholders( "Log {} message {} with three {} placeholders" ) );
		 }
	}

}