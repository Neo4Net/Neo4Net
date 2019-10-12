using System;
using System.Collections.Generic;

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
namespace Neo4Net.Logging
{
	using Test = org.junit.jupiter.api.Test;


	using Suppliers = Neo4Net.Function.Suppliers;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.endsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.sameInstance;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;

	internal class FormattedLogProviderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnSameLoggerForSameClass()
		 internal virtual void ShouldReturnSameLoggerForSameClass()
		 {
			  // Given
			  FormattedLogProvider logProvider = FormattedLogProvider.ToOutputStream( new MemoryStream() );

			  // Then
			  FormattedLog log = logProvider.getLog( this.GetType() );
			  assertThat( logProvider.GetLog( typeof( FormattedLogProviderTest ) ), sameInstance( log ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReturnSameLoggerForSameContext()
		 internal virtual void ShouldReturnSameLoggerForSameContext()
		 {
			  // Given
			  FormattedLogProvider logProvider = FormattedLogProvider.ToOutputStream( new MemoryStream() );

			  // Then
			  FormattedLog log = logProvider.GetLog( "test context" );
			  assertThat( logProvider.GetLog( "test context" ), sameInstance( log ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldLogWithAbbreviatedClassNameAsContext()
		 internal virtual void ShouldLogWithAbbreviatedClassNameAsContext()
		 {
			  // Given
			  StringWriter writer = new StringWriter();
			  FormattedLogProvider logProvider = NewFormattedLogProvider( writer );
			  FormattedLog log = logProvider.GetLog( typeof( StringWriter ) );

			  // When
			  log.Info( "Terminator 2" );

			  // Then
			  assertThat( writer.ToString(), endsWith(format("INFO [j.i.StringWriter] Terminator 2%n")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSetLevelForLogWithMatchingContext()
		 internal virtual void ShouldSetLevelForLogWithMatchingContext()
		 {
			  // Given
			  StringWriter writer = new StringWriter();
			  FormattedLogProvider logProvider = NewFormattedLogProvider( writer, "java.io.StringWriter", Level.Debug );

			  // When
			  FormattedLog stringWriterLog = logProvider.GetLog( typeof( StringWriter ) );
			  FormattedLog otherClassLog = logProvider.GetLog( typeof( PrintWriter ) );
			  FormattedLog matchingNamedLog = logProvider.GetLog( "java.io.StringWriter" );
			  FormattedLog nonMatchingNamedLog = logProvider.GetLog( "java.io.Foo" );

			  // Then
			  assertThat( stringWriterLog.DebugEnabled, @is( true ) );
			  assertThat( otherClassLog.DebugEnabled, @is( false ) );
			  assertThat( matchingNamedLog.DebugEnabled, @is( true ) );
			  assertThat( nonMatchingNamedLog.DebugEnabled, @is( false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldSetLevelForLogWithPartiallyMatchingContext()
		 internal virtual void ShouldSetLevelForLogWithPartiallyMatchingContext()
		 {
			  // Given
			  StringWriter writer = new StringWriter();
			  FormattedLogProvider logProvider = NewFormattedLogProvider( writer, "java.io", Level.Debug );

			  // When
			  FormattedLog stringWriterLog = logProvider.GetLog( typeof( StringWriter ) );
			  FormattedLog printWriterLog = logProvider.GetLog( typeof( PrintWriter ) );
			  FormattedLog otherClassLog = logProvider.GetLog( typeof( DateTime ) );
			  FormattedLog matchingNamedLog = logProvider.GetLog( "java.io.Foo" );
			  FormattedLog nonMatchingNamedLog = logProvider.GetLog( "java.util.Foo" );

			  // Then
			  assertThat( stringWriterLog.DebugEnabled, @is( true ) );
			  assertThat( printWriterLog.DebugEnabled, @is( true ) );
			  assertThat( otherClassLog.DebugEnabled, @is( false ) );
			  assertThat( matchingNamedLog.DebugEnabled, @is( true ) );
			  assertThat( nonMatchingNamedLog.DebugEnabled, @is( false ) );
		 }

		 private static FormattedLogProvider NewFormattedLogProvider( StringWriter writer )
		 {
			  return NewFormattedLogProvider( writer, Collections.emptyMap() );
		 }

		 private static FormattedLogProvider NewFormattedLogProvider( StringWriter writer, string context, Level level )
		 {
			  return NewFormattedLogProvider( writer, Collections.singletonMap( context, level ) );
		 }

		 private static FormattedLogProvider NewFormattedLogProvider( StringWriter writer, IDictionary<string, Level> levels )
		 {
			  return new FormattedLogProvider( Suppliers.singleton( new PrintWriter( writer ) ), ZoneOffset.UTC, true, levels, Level.Info, true );
		 }
	}

}