using System;
using System.Collections.Generic;
using System.Text;

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
namespace Neo4Net.Logging
{
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using Matchers = org.hamcrest.Matchers;
	using StringDescription = org.hamcrest.StringDescription;
	using TestRule = org.junit.rules.TestRule;
	using Statement = org.junit.runners.model.Statement;


	using Iterators = Neo4Net.Collections.Helpers.Iterators;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.text.StringEscapeUtils.escapeJava;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.anyOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.arrayContaining;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class AssertableLogProvider : AbstractLogProvider<Log>, TestRule
	{
		 private readonly bool _debugEnabled;
		 private readonly IList<LogCall> _logCalls = new CopyOnWriteArrayList<LogCall>();

		 public AssertableLogProvider() : this(false)
		 {
		 }

		 public AssertableLogProvider( bool debugEnabled )
		 {
			  this._debugEnabled = debugEnabled;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.junit.runners.model.Statement apply(final org.junit.runners.model.Statement super, org.junit.runner.Description description)
		 public override Statement Apply( Statement @base, org.junit.runner.Description description )
		 {
			  return new StatementAnonymousInnerClass( this, @base );
		 }

		 private class StatementAnonymousInnerClass : Statement
		 {
			 private readonly AssertableLogProvider _outerInstance;

			 private Statement @base;

			 public StatementAnonymousInnerClass( AssertableLogProvider outerInstance, Statement @base )
			 {
				 this.outerInstance = outerInstance;
				 this.@base = @base;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void Evaluate() throws Throwable
			 public override void Evaluate()
			 {
				  try
				  {
						@base.Evaluate();
				  }
				  catch ( Exception failure )
				  {
						outerInstance.Print( System.out );
						throw failure;
				  }
			 }
		 }

		 public virtual void Print( PrintStream @out )
		 {
			  foreach ( LogCall call in _logCalls )
			  {
					@out.println( call.ToLogLikeString() );
					if ( call.Throwable != null )
					{
						 call.Throwable.printStackTrace( @out );
					}
			  }
		 }

		 public enum Level
		 {
			  Debug,
			  Info,
			  Warn,
			  Error
		 }

		 private sealed class LogCall
		 {
			  internal readonly string Context;
			  internal readonly Level Level;
			  internal readonly string Message;
			  internal readonly object[] Arguments;
			  internal readonly Exception Throwable;

			  internal LogCall( string context, Level level, string message, object[] arguments, Exception throwable )
			  {
					this.Level = level;
					this.Context = context;
					this.Message = message;
					this.Arguments = arguments;
					this.Throwable = throwable;
			  }

			  public override string ToString()
			  {
					StringBuilder builder = new StringBuilder( "LogCall{" );
					builder.Append( Context );
					builder.Append( " " );
					builder.Append( Level );
					builder.Append( ", message=" );
					if ( !string.ReferenceEquals( Message, null ) )
					{
						 builder.Append( '\'' ).Append( escapeJava( Message ) ).Append( '\'' );
					}
					else
					{
						 builder.Append( "null" );
					}
					builder.Append( ", arguments=" );
					if ( Arguments != null )
					{
						 builder.Append( "[" );
						 bool first = true;
						 foreach ( object arg in Arguments )
						 {
							  if ( !first )
							  {
									builder.Append( ',' );
							  }
							  first = false;
							  builder.Append( escapeJava( "" + arg ) );
						 }
						 builder.Append( "]" );
					}
					else
					{
						 builder.Append( "null" );
					}
					builder.Append( ", throwable=" );
					if ( Throwable != null )
					{
						 builder.Append( '\'' ).Append( escapeJava( Throwable.ToString() ) ).Append('\'');
					}
					else
					{
						 builder.Append( "null" );
					}
					builder.Append( "}" );
					return builder.ToString();
			  }

			  public string ToLogLikeString()
			  {
					string msg;
					if ( Arguments != null )
					{
						 try
						 {
							  msg = format( Message, Arguments );
						 }
						 catch ( IllegalFormatException )
						 {
							  msg = format( "IllegalFormat{message: \"%s\", arguments: %s}", Message, Arrays.ToString( Arguments ) );
						 }
					}
					else
					{
						 msg = Message;
					}
					return format( "%s @ %s: %s", Level, Context, msg );
			  }
		 }

		 private class LogCallRecorder : Logger
		 {
			 private readonly AssertableLogProvider _outerInstance;

			  internal readonly string Context;
			  internal readonly Level Level;

			  internal LogCallRecorder( AssertableLogProvider outerInstance, string context, Level level )
			  {
				  this._outerInstance = outerInstance;
					this.Context = context;
					this.Level = level;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void log(@Nonnull String message)
			  public override void Log( string message )
			  {
					outerInstance.logCalls.Add( new LogCall( Context, Level, message, null, null ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void log(@Nonnull String message, @Nonnull Throwable throwable)
			  public override void Log( string message, Exception throwable )
			  {
					outerInstance.logCalls.Add( new LogCall( Context, Level, message, null, throwable ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void log(@Nonnull String format, @Nonnull Object... arguments)
			  public override void Log( string format, params object[] arguments )
			  {
					outerInstance.logCalls.Add( new LogCall( Context, Level, format, arguments, null ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void bulk(@Nonnull Consumer<Logger> consumer)
			  public override void Bulk( Consumer<Logger> consumer )
			  {
					consumer.accept( this );
			  }
		 }

		 private class AssertableLog : AbstractLog
		 {
			 private readonly AssertableLogProvider _outerInstance;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly Logger DebugLoggerConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly Logger InfoLoggerConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly Logger WarnLoggerConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly Logger ErrorLoggerConflict;

			  internal AssertableLog( AssertableLogProvider outerInstance, string context )
			  {
				  this._outerInstance = outerInstance;
					this.DebugLoggerConflict = new LogCallRecorder( outerInstance, context, Level.Debug );
					this.InfoLoggerConflict = new LogCallRecorder( outerInstance, context, Level.Info );
					this.WarnLoggerConflict = new LogCallRecorder( outerInstance, context, Level.Warn );
					this.ErrorLoggerConflict = new LogCallRecorder( outerInstance, context, Level.Error );
			  }

			  public override bool DebugEnabled
			  {
				  get
				  {
						return outerInstance.debugEnabled;
				  }
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public Logger debugLogger()
			  public override Logger DebugLogger()
			  {
					return DebugLoggerConflict;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public Logger infoLogger()
			  public override Logger InfoLogger()
			  {
					return InfoLoggerConflict;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public Logger warnLogger()
			  public override Logger WarnLogger()
			  {
					return WarnLoggerConflict;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public Logger errorLogger()
			  public override Logger ErrorLogger()
			  {
					return ErrorLoggerConflict;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void bulk(@Nonnull Consumer<Log> consumer)
			  public override void Bulk( Consumer<Log> consumer )
			  {
					consumer.accept( this );
			  }
		 }

		 protected internal override Log BuildLog( Type loggingClass )
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  return new AssertableLog( this, loggingClass.FullName );
		 }

		 protected internal override Log BuildLog( string context )
		 {
			  return new AssertableLog( this, context );
		 }

		 //
		 // TEST TOOLS
		 //

		 private static readonly Matcher<Level> _debugLevelMatcher = equalTo( Level.Debug );
		 private static readonly Matcher<Level> _infoLevelMatcher = equalTo( Level.Info );
		 private static readonly Matcher<Level> _warnLevelMatcher = equalTo( Level.Warn );
		 private static readonly Matcher<Level> _errorLevelMatcher = equalTo( Level.Error );
		 private static readonly Matcher<Level> _anyLevelMatcher = any( typeof( Level ) );
		 private static readonly Matcher<string> _anyMessageMatcher = anyOf( any( typeof( string ) ), nullValue() );
		 private static readonly Matcher<object[]> _nullArgumentsMatcher = nullValue( typeof( object[] ) );
		 private static readonly Matcher<object[]> _anyArgumentsMatcher = anyOf( any( typeof( object[] ) ), nullValue() );
		 private static readonly Matcher<Exception> _nullThrowableMatcher = nullValue( typeof( Exception ) );
		 private static readonly Matcher<Exception> _anyThrowableMatcher = anyOf( any( typeof( Exception ) ), nullValue() );

		 public sealed class LogMatcher
		 {
			  internal readonly Matcher<string> ContextMatcher;
			  internal readonly Matcher<Level> LevelMatcher;
			  internal readonly Matcher<string> MessageMatcher;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.hamcrest.Matcher<? extends Object[]> argumentsMatcher;
			  internal readonly Matcher<object[]> ArgumentsMatcher;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.hamcrest.Matcher<? extends Throwable> throwableMatcher;
			  internal readonly Matcher<Exception> ThrowableMatcher;

			  public LogMatcher<T1, T2>( Matcher<string> contextMatcher, Matcher<Level> levelMatcher, Matcher<string> messageMatcher, Matcher<T1> argumentsMatcher, Matcher<T2> throwableMatcher ) where T1 : Object[] where T2 : Exception
			  {
					this.ContextMatcher = contextMatcher;
					this.LevelMatcher = levelMatcher;
					this.MessageMatcher = messageMatcher;
					this.ArgumentsMatcher = argumentsMatcher;
					this.ThrowableMatcher = throwableMatcher;
			  }

			  protected internal bool Matches( LogCall logCall )
			  {
					return logCall != null && ContextMatcher.matches( logCall.Context ) && LevelMatcher.matches( logCall.Level ) && MessageMatcher.matches( logCall.Message ) && ArgumentsMatcher.matches( logCall.Arguments ) && ThrowableMatcher.matches( logCall.Throwable );
			  }

			  public override string ToString()
			  {
					Description description = new StringDescription();
					description.appendText( "LogMatcher{" );
					description.appendDescriptionOf( ContextMatcher );
					description.appendText( ", " );
					description.appendDescriptionOf( LevelMatcher );
					description.appendText( ", message=" );
					description.appendDescriptionOf( MessageMatcher );
					description.appendText( ", arguments=" );
					description.appendDescriptionOf( ArgumentsMatcher );
					description.appendText( ", throwable=" );
					description.appendDescriptionOf( ThrowableMatcher );
					description.appendText( "}" );
					return description.ToString();
			  }
		 }

		 public sealed class LogMatcherBuilder
		 {
			  internal readonly Matcher<string> ContextMatcher;

			  internal LogMatcherBuilder( Matcher<string> contextMatcher )
			  {
					this.ContextMatcher = contextMatcher;
			  }

			  public LogMatcher Debug( string message )
			  {
					return new LogMatcher( ContextMatcher, _debugLevelMatcher, equalTo( message ), _nullArgumentsMatcher, _nullThrowableMatcher );
			  }

			  public LogMatcher Debug( Matcher<string> messageMatcher )
			  {
					return new LogMatcher( ContextMatcher, _debugLevelMatcher, messageMatcher, _nullArgumentsMatcher, _nullThrowableMatcher );
			  }

			  public LogMatcher Debug( Matcher<string> messageMatcher, Matcher<Exception> throwableMatcher )
			  {
					return new LogMatcher( ContextMatcher, _debugLevelMatcher, messageMatcher, _nullArgumentsMatcher, throwableMatcher );
			  }

			  public LogMatcher Debug( string format, params object[] arguments )
			  {
					return debug( equalTo( format ), arguments );
			  }

			  public LogMatcher Debug( Matcher<string> format, params object[] arguments )
			  {
					return new LogMatcher( ContextMatcher, _debugLevelMatcher, format, arrayContaining( EnsureMatchers( arguments ) ), _nullThrowableMatcher );
			  }

			  public LogMatcher Info( string message )
			  {
					return new LogMatcher( ContextMatcher, _infoLevelMatcher, equalTo( message ), _nullArgumentsMatcher, _nullThrowableMatcher );
			  }

			  public LogMatcher Info( Matcher<string> messageMatcher )
			  {
					return new LogMatcher( ContextMatcher, _infoLevelMatcher, messageMatcher, _nullArgumentsMatcher, _nullThrowableMatcher );
			  }

			  public LogMatcher Info( Matcher<string> messageMatcher, Matcher<Exception> throwableMatcher )
			  {
					return new LogMatcher( ContextMatcher, _infoLevelMatcher, messageMatcher, _nullArgumentsMatcher, throwableMatcher );
			  }

			  public LogMatcher Info( string format, params object[] arguments )
			  {
					return info( equalTo( format ), arguments );
			  }

			  public LogMatcher Info( Matcher<string> format, params object[] arguments )
			  {
					return new LogMatcher( ContextMatcher, _infoLevelMatcher, format, arrayContaining( EnsureMatchers( arguments ) ), _nullThrowableMatcher );
			  }

			  public LogMatcher Warn( string message )
			  {
					return new LogMatcher( ContextMatcher, _warnLevelMatcher, equalTo( message ), _nullArgumentsMatcher, _nullThrowableMatcher );
			  }

			  public LogMatcher Warn( Matcher<string> messageMatcher )
			  {
					return new LogMatcher( ContextMatcher, _warnLevelMatcher, messageMatcher, _nullArgumentsMatcher, _nullThrowableMatcher );
			  }

			  public LogMatcher Warn( Matcher<string> messageMatcher, Matcher<Exception> throwableMatcher )
			  {
					return new LogMatcher( ContextMatcher, _warnLevelMatcher, messageMatcher, _nullArgumentsMatcher, throwableMatcher );
			  }

			  public LogMatcher Warn( string format, params object[] arguments )
			  {
					return warn( equalTo( format ), arguments );
			  }

			  public LogMatcher Warn( Matcher<string> format, params object[] arguments )
			  {
					return new LogMatcher( ContextMatcher, _warnLevelMatcher, format, arrayContaining( EnsureMatchers( arguments ) ), _nullThrowableMatcher );
			  }

			  public LogMatcher AnyError()
			  {
					return new LogMatcher( ContextMatcher, _errorLevelMatcher, Matchers.any( typeof( string ) ), _anyArgumentsMatcher, _anyThrowableMatcher );
			  }

			  public LogMatcher Error( string message )
			  {
					return new LogMatcher( ContextMatcher, _errorLevelMatcher, equalTo( message ), _nullArgumentsMatcher, _nullThrowableMatcher );
			  }

			  public LogMatcher Error( Matcher<string> messageMatcher )
			  {
					return new LogMatcher( ContextMatcher, _errorLevelMatcher, messageMatcher, _nullArgumentsMatcher, _nullThrowableMatcher );
			  }

			  public LogMatcher Error<T1>( Matcher<string> messageMatcher, Matcher<T1> throwableMatcher ) where T1 : Exception
			  {
					return new LogMatcher( ContextMatcher, _errorLevelMatcher, messageMatcher, _nullArgumentsMatcher, throwableMatcher );
			  }

			  public LogMatcher Error( string format, params object[] arguments )
			  {
					return error( equalTo( format ), arguments );
			  }

			  public LogMatcher Error( Matcher<string> format, params object[] arguments )
			  {
					return new LogMatcher( ContextMatcher, _errorLevelMatcher, format, arrayContaining( EnsureMatchers( arguments ) ), _nullThrowableMatcher );
			  }

			  public LogMatcher Any()
			  {
					return new LogMatcher( ContextMatcher, _anyLevelMatcher, _anyMessageMatcher, anyOf( _nullArgumentsMatcher, _anyArgumentsMatcher ), anyOf( _nullThrowableMatcher, _anyThrowableMatcher ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private org.hamcrest.Matcher<Object>[] ensureMatchers(Object... arguments)
			  internal Matcher<object>[] EnsureMatchers( params object[] arguments )
			  {
					IList<Matcher> matchers = new List<Matcher>();
					foreach ( object arg in arguments )
					{
						 if ( arg is Matcher )
						 {
							  matchers.Add( ( Matcher ) arg );
						 }
						 else
						 {
							  matchers.Add( equalTo( arg ) );
						 }
					}
					return matchers.ToArray();
			  }
		 }

		 public static LogMatcherBuilder InLog( Type logClass )
		 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  return inLog( equalTo( logClass.FullName ) );
		 }

		 public static LogMatcherBuilder InLog( string context )
		 {
			  return inLog( equalTo( context ) );
		 }

		 public static LogMatcherBuilder InLog( Matcher<string> contextMatcher )
		 {
			  return new LogMatcherBuilder( contextMatcher );
		 }

		 public virtual void AssertExactly( params LogMatcher[] expected )
		 {
			  IEnumerator<LogMatcher> expectedIterator = asList( expected ).GetEnumerator();

			  lock ( _logCalls )
			  {
					IEnumerator<LogCall> callsIterator = _logCalls.GetEnumerator();

					while ( expectedIterator.MoveNext() )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 if ( callsIterator.hasNext() )
						 {
							  LogMatcher logMatcher = expectedIterator.Current;
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							  LogCall logCall = callsIterator.next();
							  if ( !logMatcher.Matches( logCall ) )
							  {
									fail( format( "Log call did not match expectation\n  Expected: %s\n  Call was: %s", logMatcher, logCall ) );
							  }
						 }
						 else
						 {
							  fail( format( "Got fewer log calls than expected. The missing log calls were:\n%s", Describe( expectedIterator ) ) );
						 }
					}

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( callsIterator.hasNext() )
					{
						 fail( format( "Got more log calls than expected. The remaining log calls were:\n%s", Serialize( callsIterator ) ) );
					}
			  }
		 }

		 /// <returns> a <seealso cref="MessageMatcher"/> which compares the raw messages, i.e. even for format strings that gets an array of arguments
		 /// passed along with it the comparison will be on the message string with the formatting indicators intact and no arguments formatted in. </returns>
		 public virtual MessageMatcher RawMessageMatcher()
		 {
			  return new MessageMatcher( this, logCall => logCall.message );
		 }

		 /// <returns> a <seealso cref="MessageMatcher"/> which compares the formatted messages, i.e. after the message and its arguments have
		 /// been formatted by <seealso cref="String.format(string, object...)"/> - the resulting log messages. </returns>
		 public virtual MessageMatcher FormattedMessageMatcher()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return new MessageMatcher( this, LogCall::toLogLikeString );
		 }

		 /// <returns> a <seealso cref="MessageMatcher"/> which compares strings from <seealso cref="LogCall.toString()"/>. </returns>
		 public virtual MessageMatcher InternalToStringMessageMatcher()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return new MessageMatcher( this, LogCall::toString );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs private final void assertContains(int logSkipCount, System.Func<LogCall,String> stringifyer, org.hamcrest.Matcher<String>... matchers)
		 private void AssertContains( int logSkipCount, System.Func<LogCall, string> stringifyer, params Matcher<string>[] matchers )
		 {
			  lock ( _logCalls )
			  {
					assertEquals( _logCalls.Count, logSkipCount + matchers.Length );
					for ( int i = 0; i < matchers.Length; i++ )
					{
						 LogCall logCall = _logCalls[logSkipCount + i];
						 Matcher<string> matcher = matchers[i];

						 if ( !matcher.matches( stringifyer( logCall ) ) )
						 {
							  StringDescription description = new StringDescription();
							  description.appendDescriptionOf( matcher );
							  fail( format( "Expected log statement with message as %s, but none found. Actual log call was:\n%s", description.ToString(), logCall.ToString() ) );
						 }
					}
			  }
		 }

		 public virtual void AssertContainsThrowablesMatching( int logSkipCount, params Exception[] throwables )
		 {
			  lock ( _logCalls )
			  {
					assertEquals( _logCalls.Count, logSkipCount + throwables.Length );
					for ( int i = 0; i < throwables.Length; i++ )
					{
						 LogCall logCall = _logCalls[logSkipCount + i];
						 Exception throwable = throwables[i];

						 if ( logCall.Throwable == null && throwable != null || logCall.Throwable != null && logCall.Throwable.GetType() != throwable.GetType() )
						 {
							  fail( format( "Expected %s, but was:\n%s", throwable, logCall.Throwable ) );
						 }
					}
			  }
		 }

		 /// <summary>
		 /// Note: Does not care about ordering.
		 /// </summary>
		 public virtual void AssertAtLeastOnce( params LogMatcher[] expected )
		 {
			  ISet<LogMatcher> expectedMatchers = Iterators.asSet( expected );
			  lock ( _logCalls )
			  {
					expectedMatchers.removeIf( this.containsMatchingLogCall );

					if ( expectedMatchers.Count > 0 )
					{
						 fail( format( "These log calls were expected, but never occurred:\n%s\nActual log calls were:\n%s", Describe( expectedMatchers.GetEnumerator() ), Serialize(_logCalls.GetEnumerator()) ) );
					}
			  }
		 }

		 public virtual void AssertNone( LogMatcher notExpected )
		 {
			  LogCall logCall = FirstMatchingLogCall( notExpected );
			  if ( logCall != null )
			  {
					fail( format( "Log call was not expected, but occurred:\n%s\n", logCall.ToString() ) );
			  }
		 }

		 private void AssertNotContains( string partOfMessage, System.Func<LogCall, string> stringifyer )
		 {
			  if ( ContainsLogCallContaining( partOfMessage, stringifyer ) )
			  {
					fail( format( "Expected no log statement containing '%s', but at least one found. Actual log calls were:\n%s", partOfMessage, Serialize( _logCalls.GetEnumerator() ) ) );
			  }
		 }

		 private void AssertContains( string partOfMessage, System.Func<LogCall, string> stringifyer )
		 {
			  if ( !ContainsLogCallContaining( partOfMessage, stringifyer ) )
			  {
					fail( format( "Expected at least one log statement containing '%s', but none found. Actual log calls were:\n%s", partOfMessage, Serialize( _logCalls.GetEnumerator() ) ) );
			  }
		 }

		 private bool ContainsLogCallContaining( string partOfMessage, System.Func<LogCall, string> stringifyer )
		 {
			  lock ( _logCalls )
			  {
					foreach ( LogCall logCall in _logCalls )
					{
						 if ( stringifyer( logCall ).contains( partOfMessage ) )
						 {
							  return true;
						 }
					}
			  }
			  return false;
		 }

		 public virtual bool ContainsMatchingLogCall( LogMatcher logMatcher )
		 {
			  return FirstMatchingLogCall( logMatcher ) != null;
		 }

		 private LogCall FirstMatchingLogCall( LogMatcher logMatcher )
		 {
			  lock ( _logCalls )
			  {
					foreach ( LogCall logCall in _logCalls )
					{
						 if ( logMatcher.Matches( logCall ) )
						 {
							  return logCall;
						 }
					}
			  }
			  return null;
		 }

		 private void AssertContains( Matcher<string> messageMatcher, System.Func<LogCall, string> stringifyer )
		 {
			  lock ( _logCalls )
			  {
					foreach ( LogCall logCall in _logCalls )
					{
						 if ( messageMatcher.matches( stringifyer( logCall ) ) )
						 {
							  return;
						 }
					}
					StringDescription description = new StringDescription();
					description.appendDescriptionOf( messageMatcher );
					fail( format( "Expected at least one log statement with message as %s, but none found. Actual log calls were:\n%s", description.ToString(), Serialize(_logCalls.GetEnumerator()) ) );
			  }
		 }

		 private void AssertContainsSingle( Matcher<string> messageMatcher, System.Func<LogCall, string> stringifyer )
		 {
			  bool found = false;
			  lock ( _logCalls )
			  {
					foreach ( LogCall logCall in _logCalls )
					{
						 if ( messageMatcher.matches( stringifyer( logCall ) ) )
						 {
							  if ( !found )
							  {
									found = true;
							  }
							  else
							  {
									StringDescription description = new StringDescription();
									description.appendDescriptionOf( messageMatcher );
									fail( format( "Expected exactly one log statement with message as %s, but multiple found. Actual log calls were:%n%s", description.ToString(), Serialize(_logCalls.GetEnumerator()) ) );
							  }
						 }
					}
					if ( !found )
					{
						 StringDescription description = new StringDescription();
						 description.appendDescriptionOf( messageMatcher );
						 fail( format( "Expected at least one log statement with message as %s, but none found. Actual log calls were:\n%s", description.ToString(), Serialize(_logCalls.GetEnumerator()) ) );
					}
			  }
		 }

		 public virtual void AssertNoLoggingOccurred()
		 {
			  if ( _logCalls.Count != 0 )
			  {
					fail( format( "Expected no log messages at all, but got:\n%s", Serialize( _logCalls.GetEnumerator() ) ) );
			  }
		 }

		 /// <summary>
		 /// Clear this logger for re-use.
		 /// </summary>
		 public virtual void Clear()
		 {
			  _logCalls.Clear();
		 }

		 public virtual string Serialize()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return Serialize( _logCalls.GetEnumerator(), LogCall::toLogLikeString );
		 }

		 private string Describe( IEnumerator<LogMatcher> matchers )
		 {
			  StringBuilder sb = new StringBuilder();
			  while ( matchers.MoveNext() )
			  {
					sb.Append( matchers.Current.ToString() );
					sb.Append( "\n" );
			  }
			  return sb.ToString();
		 }

		 private string Serialize( IEnumerator<LogCall> events )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return Serialize( events, LogCall::toString );
		 }

		 private string Serialize( IEnumerator<LogCall> events, System.Func<LogCall, string> serializer )
		 {
			  StringBuilder sb = new StringBuilder();
			  while ( events.MoveNext() )
			  {
					sb.Append( serializer( events.Current ) );
					sb.Append( "\n" );
			  }
			  return sb.ToString();
		 }

		 public class MessageMatcher
		 {
			 private readonly AssertableLogProvider _outerInstance;

			  internal readonly System.Func<LogCall, string> Stringifyer;

			  internal MessageMatcher( AssertableLogProvider outerInstance, System.Func<LogCall, string> stringifyer )
			  {
				  this._outerInstance = outerInstance;
					this.Stringifyer = stringifyer;
			  }

			  public virtual void AssertContainsSingle( Matcher<string> messageMatcher )
			  {
					_outerInstance.assertContainsSingle( messageMatcher, Stringifyer );
			  }

			  public virtual void AssertContains( Matcher<string> messageMatcher )
			  {
					_outerInstance.assertContains( messageMatcher, Stringifyer );
			  }

			  public virtual void AssertContains( string partOfMessage )
			  {
					_outerInstance.assertContains( partOfMessage, Stringifyer );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public final void assertContains(int logSkipCount, org.hamcrest.Matcher<String>... matchers)
			  public void AssertContains( int logSkipCount, params Matcher<string>[] matchers )
			  {
					_outerInstance.assertContains( logSkipCount, Stringifyer, matchers );
			  }

			  public virtual void AssertNotContains( string partOfMessage )
			  {
					_outerInstance.assertNotContains( partOfMessage, Stringifyer );
			  }
		 }
	}

}