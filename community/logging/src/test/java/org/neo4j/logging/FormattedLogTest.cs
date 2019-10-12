using System;

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
namespace Org.Neo4j.Logging
{
	using Test = org.junit.jupiter.api.Test;


	using Suppliers = Org.Neo4j.Function.Suppliers;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;

	internal class FormattedLogTest
	{
		 private static readonly System.Func<ZonedDateTime> _dateTimeSupplier = () => ZonedDateTime.of(1984, 10, 26, 4, 23, 24, 343000000, ZoneOffset.UTC);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void logShouldWriteMessage()
		 internal virtual void LogShouldWriteMessage()
		 {
			  // Given
			  StringWriter writer = new StringWriter();
			  Log log = NewFormattedLog( writer );

			  // When
			  log.Info( "Terminator 2" );

			  // Then
			  assertThat( writer.ToString(), equalTo(format("1984-10-26 04:23:24.343+0000 INFO [test] Terminator 2%n")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void logShouldWriteMessageAndThrowable()
		 internal virtual void LogShouldWriteMessageAndThrowable()
		 {
			  // Given
			  StringWriter writer = new StringWriter();
			  Log log = NewFormattedLog( writer );

			  // When
			  log.Info( "Hasta la vista, baby", NewThrowable( "<message>", "<stacktrace>" ) );

			  // Then
			  assertThat( writer.ToString(), equalTo(format("1984-10-26 04:23:24.343+0000 INFO [test] Hasta la vista, baby " + "<message>%n<stacktrace>")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void logShouldWriteMessageAndThrowableWithNullMessage()
		 internal virtual void LogShouldWriteMessageAndThrowableWithNullMessage()
		 {
			  // Given
			  StringWriter writer = new StringWriter();
			  Log log = NewFormattedLog( writer );

			  // When
			  log.Info( "Hasta la vista, baby", NewThrowable( null, "<stacktrace>" ) );

			  // Then
			  assertThat( writer.ToString(), equalTo(format("1984-10-26 04:23:24.343+0000 INFO [test] Hasta la vista, baby%n<stacktrace>")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void logShouldWriteMessageWithFormat()
		 internal virtual void LogShouldWriteMessageWithFormat()
		 {
			  // Given
			  StringWriter writer = new StringWriter();
			  Log log = NewFormattedLog( writer );

			  // When
			  log.Info( "I need your %s, your %s and your %s", "clothes", "boots", "motorcycle" );

			  // Then
			  assertThat( writer.ToString(), equalTo(format("1984-10-26 04:23:24.343+0000 INFO [test] I need your clothes, your boots and your " + "motorcycle%n")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void logShouldWriteNotFormattedMessageWhenNoParametersGiven()
		 internal virtual void LogShouldWriteNotFormattedMessageWhenNoParametersGiven()
		 {
			  // Given
			  StringWriter writer = new StringWriter();
			  Log log = NewFormattedLog( writer );

			  // When
			  log.Info( "Come with me if you %s to live!", new object[]{} );

			  // Then
			  assertThat( writer.ToString(), equalTo(format("1984-10-26 04:23:24.343+0000 INFO [test] Come with me if you %%s to live!%n")) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void logShouldFailAndWriteNothingForInvalidParametersArray()
		 internal virtual void LogShouldFailAndWriteNothingForInvalidParametersArray()
		 {
			  // Given
			  StringWriter writer = new StringWriter();
			  Log log = NewFormattedLog( writer );

			  assertThrows( typeof( IllegalFormatException ), () => log.info("%s like me. A T-%d, advanced prototype.", "Not", "1000", 1000) );
			  assertThat( writer.ToString(), equalTo("") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotWriteLogIfLevelIsHigherThanWritten()
		 internal virtual void ShouldNotWriteLogIfLevelIsHigherThanWritten()
		 {
			  // Given
			  StringWriter writer = new StringWriter();
			  Log log = NewFormattedLog( writer, Level.Warn );

			  // When
			  log.Info( "I know now why you cry. But it's something I can never do." );

			  // Then
			  assertThat( writer.ToString(), equalTo("") );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAllowLevelToBeChanged()
		 internal virtual void ShouldAllowLevelToBeChanged()
		 {
			  // Given
			  StringWriter writer = new StringWriter();
			  FormattedLog log = NewFormattedLog( writer, Level.Info );

			  // When
			  log.Info( "No, it's when there's nothing wrong with you, but you hurt anyway. You get it?" );
			  log.Level = Level.Warn;
			  log.Info( "I know now why you cry. But it's something I can never do." );
			  log.Level = Level.Debug;
			  log.Info( "There's 215 bones in the human body. That's one." );

			  // Then
			  assertThat( writer.ToString(), equalTo(format("%s%n%s%n", "1984-10-26 04:23:24.343+0000 INFO [test] No, it's when there's nothing wrong with you, but " + "you hurt anyway. You get it?", "1984-10-26 04:23:24.343+0000 INFO [test] There's 215 bones in the human body. That's one.")) );
		 }

		 private static FormattedLog NewFormattedLog( StringWriter writer )
		 {
			  return NewFormattedLog( writer, Level.Debug );
		 }

		 private static FormattedLog NewFormattedLog( StringWriter writer, Level level )
		 {
			  return FormattedLog.WithUTCTimeZone().withCategory("test").withLogLevel(level).withTimeSupplier(_dateTimeSupplier).toPrintWriter(Suppliers.singleton(new PrintWriter(writer)));
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static Throwable newThrowable(final String message, final String stackTrace)
		 private static Exception NewThrowable( string message, string stackTrace )
		 {
			  return new ThrowableAnonymousInnerClass( message, stackTrace );
		 }

		 private class ThrowableAnonymousInnerClass : Exception
		 {
			 private string _message;
			 private string _stackTrace;

			 public ThrowableAnonymousInnerClass( string message, string stackTrace )
			 {
				 this._message = message;
				 this._stackTrace = stackTrace;
			 }

			 public override void printStackTrace( PrintWriter s )
			 {
				  s.append( _stackTrace );
			 }

			 public override string Message
			 {
				 get
				 {
					  return _message;
				 }
			 }
		 }
	}

}