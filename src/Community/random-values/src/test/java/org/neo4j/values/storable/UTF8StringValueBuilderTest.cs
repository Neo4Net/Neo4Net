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
namespace Neo4Net.Values.Storable
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;

	internal class UTF8StringValueBuilderTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleSingleByteCodePoints()
		 internal virtual void ShouldHandleSingleByteCodePoints()
		 {
			  // Given
			  UTF8StringValueBuilder builder = new UTF8StringValueBuilder();
			  int codepoint = char.ConvertToUtf32( "$", 0 );

			  // When
			  builder.AddCodePoint( codepoint );
			  builder.AddCodePoint( codepoint );
			  builder.AddCodePoint( codepoint );

			  // Then
			  TextValue textValue = builder.Build();
			  assertThat( textValue.StringValue(), equalTo("$$$") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleTwoByteCodePoints()
		 internal virtual void ShouldHandleTwoByteCodePoints()
		 {
			  // Given
			  UTF8StringValueBuilder builder = new UTF8StringValueBuilder();
			  int codepoint = char.ConvertToUtf32( "¢", 0 );

			  // When
			  builder.AddCodePoint( codepoint );
			  builder.AddCodePoint( codepoint );
			  builder.AddCodePoint( codepoint );

			  // Then
			  TextValue textValue = builder.Build();
			  assertThat( textValue.StringValue(), equalTo("¢¢¢") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleThreeByteCodePoints()
		 internal virtual void ShouldHandleThreeByteCodePoints()
		 {
			  // Given
			  UTF8StringValueBuilder builder = new UTF8StringValueBuilder();
			  int codepoint = char.ConvertToUtf32( "€", 0 );

			  // When
			  builder.AddCodePoint( codepoint );
			  builder.AddCodePoint( codepoint );
			  builder.AddCodePoint( codepoint );

			  // Then
			  TextValue textValue = builder.Build();
			  assertThat( textValue.StringValue(), equalTo("€€€") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleFourByteCodePoints()
		 internal virtual void ShouldHandleFourByteCodePoints()
		 {
			  // Given
			  UTF8StringValueBuilder builder = new UTF8StringValueBuilder();
			  int codepoint = char.ConvertToUtf32( "\uD800\uDF48", 0 );

			  // When
			  builder.AddCodePoint( codepoint );
			  builder.AddCodePoint( codepoint );
			  builder.AddCodePoint( codepoint );

			  // Then
			  TextValue textValue = builder.Build();
			  assertThat( textValue.StringValue(), equalTo("\uD800\uDF48\uD800\uDF48\uD800\uDF48") );
		 }
	}

}