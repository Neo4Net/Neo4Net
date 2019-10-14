using System;
using System.Collections.Generic;

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
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;
	using Matcher = org.hamcrest.Matcher;
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Clocks = Neo4Net.Time.Clocks;
	using FakeClock = Neo4Net.Time.FakeClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.inLog;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class CappedLoggerTest
	public class CappedLoggerTest
	{

		 public interface LogMethod
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: void log(@Nonnull CappedLogger logger, @Nonnull String msg);
			  void Log( CappedLogger logger, string msg );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: void log(@Nonnull CappedLogger logger, @Nonnull String msg, @Nonnull Throwable cause);
			  void Log( CappedLogger logger, string msg, Exception cause );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static Iterable<Object[]> parameters()
		 public static IEnumerable<object[]> Parameters()
		 {
			  LogMethod debug = new LogMethodAnonymousInnerClass();
			  LogMethod info = new LogMethodAnonymousInnerClass2();
			  LogMethod warn = new LogMethodAnonymousInnerClass3();
			  LogMethod error = new LogMethodAnonymousInnerClass4();
			  return Arrays.asList( new object[]{ "debug", debug }, new object[] { "info", info }, new object[] { "warn", warn }, new object[] { "error", error } );
		 }

		 private class LogMethodAnonymousInnerClass : LogMethod
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull CappedLogger logger, @Nonnull String msg)
			 public void log( CappedLogger logger, string msg )
			 {
				  logger.Debug( msg );
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull CappedLogger logger, @Nonnull String msg, @Nonnull Throwable cause)
			 public void log( CappedLogger logger, string msg, Exception cause )
			 {
				  logger.Debug( msg, cause );
			 }
		 }

		 private class LogMethodAnonymousInnerClass2 : LogMethod
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull CappedLogger logger, @Nonnull String msg)
			 public void log( CappedLogger logger, string msg )
			 {
				  logger.Debug( msg );
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull CappedLogger logger, @Nonnull String msg, @Nonnull Throwable cause)
			 public void log( CappedLogger logger, string msg, Exception cause )
			 {
				  logger.Info( msg, cause );
			 }
		 }

		 private class LogMethodAnonymousInnerClass3 : LogMethod
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull CappedLogger logger, @Nonnull String msg)
			 public void log( CappedLogger logger, string msg )
			 {
				  logger.Debug( msg );
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull CappedLogger logger, @Nonnull String msg, @Nonnull Throwable cause)
			 public void log( CappedLogger logger, string msg, Exception cause )
			 {
				  logger.Warn( msg, cause );
			 }
		 }

		 private class LogMethodAnonymousInnerClass4 : LogMethod
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull CappedLogger logger, @Nonnull String msg)
			 public void log( CappedLogger logger, string msg )
			 {
				  logger.Debug( msg );
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull CappedLogger logger, @Nonnull String msg, @Nonnull Throwable cause)
			 public void log( CappedLogger logger, string msg, Exception cause )
			 {
				  logger.Error( msg, cause );
			 }
		 }

		 private class ExceptionWithoutStackTrace : Exception
		 {
			  internal ExceptionWithoutStackTrace( string message ) : base( message )
			  {
			  }

			  public override Exception FillInStackTrace()
			  {
					return this;
			  }
		 }

		 private class ExceptionWithoutStackTrace2 : Exception
		 {
			  internal ExceptionWithoutStackTrace2( string message ) : base( message )
			  {
			  }

			  public override Exception FillInStackTrace()
			  {
					return this;
			  }
		 }

		 private readonly string _logName;
		 private readonly LogMethod _logMethod;

		 private AssertableLogProvider _logProvider;
		 private CappedLogger _logger;

		 public CappedLoggerTest( string logName, LogMethod logMethod )
		 {
			  this._logName = logName;
			  this._logMethod = logMethod;
		 }

		 public virtual string[] LogLines( int lineCount )
		 {
			  return LogLines( lineCount, 0 );
		 }

		 public virtual string[] LogLines( int lineCount, int startAt )
		 {
			  string[] lines = new string[lineCount];
			  for ( int i = 0; i < lineCount; i++ )
			  {
					string msg = string.Format( "### {0:D4} ###", startAt + i );
					lines[i] = msg;
					_logMethod.log( _logger, msg );
			  }
			  return lines;
		 }

		 public virtual void AssertLoggedLines( string[] lines, int count )
		 {
			  AssertLoggedLines( lines, count, 0 );
		 }

		 public virtual void AssertLoggedLines( string[] lines, int count, int skip )
		 {
			  Matcher<string>[] matchers = new Matcher[count];
			  int i;
			  for ( i = 0; i < skip; i++ )
			  {
					matchers[i] = any( typeof( string ) );
			  }
			  for ( ; i < count; i++ )
			  {
					string line = lines[i];
					matchers[i] = containsString( line );
			  }

			  _logProvider.rawMessageMatcher().assertContains(skip, matchers);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _logProvider = new AssertableLogProvider();
			  _logger = new CappedLogger( _logProvider.getLog( typeof( CappedLogger ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void mustThrowIfDelegateIsNull()
		 public virtual void MustThrowIfDelegateIsNull()
		 {
			  new CappedLogger( null );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustLogWithoutLimitConfiguration()
		 public virtual void MustLogWithoutLimitConfiguration()
		 {
			  int lineCount = 1000;
			  string[] lines = LogLines( lineCount );
			  AssertLoggedLines( lines, lineCount );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustLogExceptions()
		 public virtual void MustLogExceptions()
		 {
			  _logMethod.log( _logger, "MESSAGE", new ArithmeticException( "EXCEPTION" ) );
			  AssertableLogProvider.MessageMatcher matcher = _logProvider.internalToStringMessageMatcher();
			  matcher.AssertContains( "MESSAGE" );
			  matcher.AssertContains( "ArithmeticException" );
			  matcher.AssertContains( "EXCEPTION" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void mustThrowOnSettingZeroCountLimit()
		 public virtual void MustThrowOnSettingZeroCountLimit()
		 {
			  _logger.CountLimit = 0;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void mustThrowOnSettingNegativeCountLimit()
		 public virtual void MustThrowOnSettingNegativeCountLimit()
		 {
			  _logger.CountLimit = -1;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void mustThrowOnZeroTimeLimit()
		 public virtual void MustThrowOnZeroTimeLimit()
		 {
			  _logger.setTimeLimit( 0, MILLISECONDS, Clocks.systemClock() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void mustThrowOnNegativeTimeLimit()
		 public virtual void MustThrowOnNegativeTimeLimit()
		 {
			  _logger.setTimeLimit( -1, MILLISECONDS, Clocks.systemClock() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void mustThrowOnNullTimeUnit()
		 public virtual void MustThrowOnNullTimeUnit()
		 {
			  _logger.setTimeLimit( 10, null, Clocks.systemClock() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = IllegalArgumentException.class) public void mustThrowOnNullClock()
		 public virtual void MustThrowOnNullClock()
		 {
			  _logger.setTimeLimit( 10, TimeUnit.MILLISECONDS, null );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustAllowConfigurationChaining()
		 public virtual void MustAllowConfigurationChaining()
		 {
			  _logger.setCountLimit( 1 ).setTimeLimit( 10, MILLISECONDS, Clocks.systemClock() ).setDuplicateFilterEnabled(true).unsetCountLimit().unsetTimeLimit().setCountLimit(1);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustLimitByConfiguredCount()
		 public virtual void MustLimitByConfiguredCount()
		 {
			  int limit = 10;
			  _logger.CountLimit = limit;
			  string[] lines = LogLines( limit + 1 );
			  AssertLoggedLines( lines, limit );
			  _logProvider.assertNone( CurrentLog( inLog( typeof( CappedLogger ) ), containsString( lines[limit] ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustLogAfterResetWithCountLimit()
		 public virtual void MustLogAfterResetWithCountLimit()
		 {
			  int limit = 10;
			  _logger.CountLimit = limit;
			  string[] lines = LogLines( limit + 1 );
			  _logger.reset();
			  string[] moreLines = LogLines( 1, limit + 1 );
			  AssertLoggedLines( ArrayUtils.addAll( ArrayUtils.subarray( lines, 0, limit ), moreLines ), 1 + limit );
			  _logProvider.assertNone( CurrentLog( inLog( typeof( CappedLogger ) ), containsString( lines[limit] ) ) );
			  _logProvider.rawMessageMatcher().assertContains(containsString(moreLines[0]));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unsettingCountLimitMustLetMessagesThrough()
		 public virtual void UnsettingCountLimitMustLetMessagesThrough()
		 {
			  int limit = 10;
			  _logger.CountLimit = limit;
			  string[] lines = LogLines( limit + 1 );
			  _logger.unsetCountLimit();
			  int moreLineCount = 1000;
			  string[] moreLines = LogLines( moreLineCount, limit + 1 );
			  AssertLoggedLines( ArrayUtils.addAll( ArrayUtils.subarray( lines, 0, limit ), moreLines ), moreLineCount + limit );
			  _logProvider.assertNone( CurrentLog( inLog( typeof( CappedLogger ) ), containsString( lines[limit] ) ) );
			  AssertLoggedLines( moreLines, moreLineCount, limit );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustNotLogMessagesWithinConfiguredTimeLimit()
		 public virtual void MustNotLogMessagesWithinConfiguredTimeLimit()
		 {
			  FakeClock clock = DefaultFakeClock;
			  _logger.setTimeLimit( 1, TimeUnit.MILLISECONDS, clock );
			  _logMethod.log( _logger, "### AAA ###" );
			  _logMethod.log( _logger, "### BBB ###" );
			  clock.Forward( 1, TimeUnit.MILLISECONDS );
			  _logMethod.log( _logger, "### CCC ###" );

			  _logProvider.rawMessageMatcher().assertContains(containsString("### AAA ###"));
			  _logProvider.assertNone( CurrentLog( inLog( typeof( CappedLogger ) ), containsString( "### BBB ###" ) ) );
			  _logProvider.rawMessageMatcher().assertContains(containsString("### CCC ###"));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unsettingTimeLimitMustLetMessagesThrough()
		 public virtual void UnsettingTimeLimitMustLetMessagesThrough()
		 {
			  FakeClock clock = DefaultFakeClock;
			  _logger.setTimeLimit( 1, TimeUnit.MILLISECONDS, clock );
			  _logMethod.log( _logger, "### AAA ###" );
			  _logMethod.log( _logger, "### BBB ###" );
			  clock.Forward( 1, TimeUnit.MILLISECONDS );
			  _logMethod.log( _logger, "### CCC ###" );
			  _logMethod.log( _logger, "### DDD ###" );
			  _logger.unsetTimeLimit(); // Note that we are not advancing the clock!
			  _logMethod.log( _logger, "### EEE ###" );

			  _logProvider.rawMessageMatcher().assertContains(containsString("### AAA ###"));
			  _logProvider.assertNone( CurrentLog( inLog( typeof( CappedLogger ) ), containsString( "### BBB ###" ) ) );
			  _logProvider.rawMessageMatcher().assertContains(containsString("### CCC ###"));
			  _logProvider.assertNone( CurrentLog( inLog( typeof( CappedLogger ) ), containsString( "### DDD ###" ) ) );
			  _logProvider.rawMessageMatcher().assertContains(containsString("### EEE ###"));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustLogAfterResetWithTimeLimit()
		 public virtual void MustLogAfterResetWithTimeLimit()
		 {
			  FakeClock clock = DefaultFakeClock;
			  _logger.setTimeLimit( 1, TimeUnit.MILLISECONDS, clock );
			  _logMethod.log( _logger, "### AAA ###" );
			  _logMethod.log( _logger, "### BBB ###" );
			  _logger.reset();
			  _logMethod.log( _logger, "### CCC ###" );

			  _logProvider.rawMessageMatcher().assertContains(containsString("### AAA ###"));
			  _logProvider.assertNone( CurrentLog( inLog( typeof( CappedLogger ) ), containsString( "### BBB ###" ) ) );
			  _logProvider.rawMessageMatcher().assertContains(containsString("### CCC ###"));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustOnlyLogMessagesThatPassBothLimits()
		 public virtual void MustOnlyLogMessagesThatPassBothLimits()
		 {
			  FakeClock clock = DefaultFakeClock;
			  _logger.CountLimit = 2;
			  _logger.setTimeLimit( 1, TimeUnit.MILLISECONDS, clock );
			  _logMethod.log( _logger, "### AAA ###" );
			  _logMethod.log( _logger, "### BBB ###" ); // Filtered by the time limit
			  clock.Forward( 1, TimeUnit.MILLISECONDS );
			  _logMethod.log( _logger, "### CCC ###" ); // Filtered by the count limit
			  _logger.reset();
			  _logMethod.log( _logger, "### DDD ###" );

			  _logProvider.rawMessageMatcher().assertContains(containsString("### AAA ###"));
			  _logProvider.assertNone( CurrentLog( inLog( typeof( CappedLogger ) ), containsString( "### BBB ###" ) ) );
			  _logProvider.assertNone( CurrentLog( inLog( typeof( CappedLogger ) ), containsString( "### CCC ###" ) ) );
			  _logProvider.rawMessageMatcher().assertContains(containsString("### DDD ###"));
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustFilterDuplicateMessageAndNullException()
		 public virtual void MustFilterDuplicateMessageAndNullException()
		 {
			  _logger.DuplicateFilterEnabled = true;
			  _logMethod.log( _logger, "### AAA ###" );
			  _logMethod.log( _logger, "### AAA ###" ); // duplicate
			  _logMethod.log( _logger, "### BBB ###" );
			  string[] lines = new string[]{ "### AAA ###", "### BBB ###" };
			  AssertLoggedLines( lines, lines.Length );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustFilterDuplicateMessageAndException()
		 public virtual void MustFilterDuplicateMessageAndException()
		 {
			  _logger.DuplicateFilterEnabled = true;
			  _logMethod.log( _logger, "### AAA ###", new ExceptionWithoutStackTrace( "exc_aaa" ) );
			  _logMethod.log( _logger, "### AAA ###", new ExceptionWithoutStackTrace( "exc_aaa" ) ); // duplicate
			  _logMethod.log( _logger, "### BBB ###", new ExceptionWithoutStackTrace( "exc_bbb" ) );

			  string[] messages = new string[]{ "### AAA ###", "### BBB ###" };
			  AssertLoggedLines( messages, messages.Length );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustLogSameMessageAndDifferentExceptionWithDuplicateLimit()
		 public virtual void MustLogSameMessageAndDifferentExceptionWithDuplicateLimit()
		 {
			  _logger.DuplicateFilterEnabled = true;
			  _logMethod.log( _logger, "### AAA ###", new ExceptionWithoutStackTrace( "exc_aaa" ) );
			  _logMethod.log( _logger, "### AAA ###", new ExceptionWithoutStackTrace( "exc_bbb" ) ); // Different message
			  _logMethod.log( _logger, "### AAA ###", new ExceptionWithoutStackTrace2( "exc_bbb" ) ); // Different type

			  string[] messages = new string[]{ "### AAA ###", "### AAA ###", "### AAA ###" };
			  AssertLoggedLines( messages, messages.Length );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustLogSameMessageAndNonNullExceptionWithDuplicateLimit()
		 public virtual void MustLogSameMessageAndNonNullExceptionWithDuplicateLimit()
		 {
			  _logger.DuplicateFilterEnabled = true;
			  _logMethod.log( _logger, "### AAA ###" );
			  _logMethod.log( _logger, "### AAA ###", new ExceptionWithoutStackTrace( null ) ); // Different message
			  _logMethod.log( _logger, "### AAA ###", new ExceptionWithoutStackTrace2( null ) ); // Different type

			  string[] messages = new string[]{ "### AAA ###", "### AAA ###", "### AAA ###" };
			  AssertLoggedLines( messages, messages.Length );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustFilterSameMessageAndExceptionWithNullMessage()
		 public virtual void MustFilterSameMessageAndExceptionWithNullMessage()
		 {
			  _logger.DuplicateFilterEnabled = true;
			  _logMethod.log( _logger, "### AAA ###", new ExceptionWithoutStackTrace( null ) );
			  _logMethod.log( _logger, "### AAA ###", new ExceptionWithoutStackTrace( null ) );
			  _logMethod.log( _logger, "### BBB ###" );

			  string[] messages = new string[]{ "### AAA ###", "### BBB ###" };
			  AssertLoggedLines( messages, messages.Length );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustLogDifferentMessageAndSameExceptionWithDuplicateLimit()
		 public virtual void MustLogDifferentMessageAndSameExceptionWithDuplicateLimit()
		 {
			  _logger.DuplicateFilterEnabled = true;
			  _logMethod.log( _logger, "### AAA ###", new ExceptionWithoutStackTrace( "xyz" ) );
			  _logMethod.log( _logger, "### BBB ###", new ExceptionWithoutStackTrace( "xyz" ) );

			  string[] messages = new string[]{ "### AAA ###", "### BBB ###" };
			  AssertLoggedLines( messages, messages.Length );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustLogDifferentMessageAndDifferentExceptionWithDuplicateLimit()
		 public virtual void MustLogDifferentMessageAndDifferentExceptionWithDuplicateLimit()
		 {
			  _logger.DuplicateFilterEnabled = true;
			  _logMethod.log( _logger, "### AAA ###", new ExceptionWithoutStackTrace( "foo" ) );
			  _logMethod.log( _logger, "### BBB ###", new ExceptionWithoutStackTrace( "bar" ) );

			  string[] messages = new string[]{ "### AAA ###", "### BBB ###" };
			  AssertLoggedLines( messages, messages.Length );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustLogSameMessageAndExceptionAfterResetWithDuplicateFilter()
		 public virtual void MustLogSameMessageAndExceptionAfterResetWithDuplicateFilter()
		 {
			  _logger.DuplicateFilterEnabled = true;
			  _logMethod.log( _logger, "### AAA ###", new ExceptionWithoutStackTrace( "xyz" ) );
			  _logger.reset();
			  _logMethod.log( _logger, "### AAA ###", new ExceptionWithoutStackTrace( "xyz" ) );

			  string[] messages = new string[]{ "### AAA ###", "### AAA ###" };
			  AssertLoggedLines( messages, messages.Length );
		 }

		 private AssertableLogProvider.LogMatcher CurrentLog( AssertableLogProvider.LogMatcherBuilder logMatcherBuilder, Matcher<string> stringMatcher )
		 {
			  switch ( _logName )
			  {
			  case "debug":
					return logMatcherBuilder.Debug( stringMatcher );
			  case "info":
					return logMatcherBuilder.Info( stringMatcher );
			  case "warn":
					return logMatcherBuilder.Warn( stringMatcher );
			  case "error":
					return logMatcherBuilder.Error( stringMatcher );
			  default:
					throw new Exception( "Unknown log name" );
			  }
		 }

		 private FakeClock DefaultFakeClock
		 {
			 get
			 {
				  return Clocks.fakeClock( 1000, TimeUnit.MILLISECONDS );
			 }
		 }
	}

}