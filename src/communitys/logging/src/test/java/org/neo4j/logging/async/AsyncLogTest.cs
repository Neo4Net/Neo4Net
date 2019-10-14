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
namespace Neo4Net.Logging.async
{
	using Matcher = org.hamcrest.Matcher;
	using ParameterizedTest = org.junit.jupiter.@params.ParameterizedTest;
	using Arguments = org.junit.jupiter.@params.provider.Arguments;
	using MethodSource = org.junit.jupiter.@params.provider.MethodSource;


	using Neo4Net.Utils.Concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.allOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.endsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.sameInstance;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.inLog;

	public class AsyncLogTest
	{
		 private static readonly Exception _exception = new Exception();

		 public static Stream<Arguments> Parameters()
		 {
			  IList<Arguments> parameters = new List<Arguments>();
			  foreach ( Invocation invocation in Invocation.values() )
			  {
					foreach ( Level level in Enum.GetValues( typeof( Level ) ) )
					{
						 foreach ( Style style in Style.values() )
						 {
							  parameters.Add( Arguments.of( invocation, level, style ) );
						 }
					}
			  }
			  return parameters.stream();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @MethodSource("parameters") void shouldLogAsynchronously(Invocation invocation, Level level, Style style)
		 internal virtual void ShouldLogAsynchronously( Invocation invocation, Level level, Style style )
		 {
			  // given
			  AssertableLogProvider logging = new AssertableLogProvider();
			  Log log = logging.getLog( this.GetType() );
			  DeferredSender events = new DeferredSender();
			  AsyncLog asyncLog = new AsyncLog( events, log );

			  // when
			  log( invocation.decorate( asyncLog ), level, style );
			  // then
			  logging.AssertNoLoggingOccurred();

			  // when
			  events.Process();
			  // then
			  MatcherBuilder matcherBuilder = new MatcherBuilder( inLog( this.GetType() ) );
			  log( matcherBuilder, level, style );
			  logging.AssertExactly( matcherBuilder.Matcher() );
		 }

		 private void Log( Log log, Level level, Style style )
		 {
			  style.invoke( this, level.logger( log ) );
		 }

		 internal abstract class Invocation
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           DIRECT { Log decorate(org.neo4j.logging.Log log) { return new DirectLog(log); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           INDIRECT { Log decorate(org.neo4j.logging.Log log) { return log; } };

			  private static readonly IList<Invocation> valueList = new List<Invocation>();

			  static Invocation()
			  {
				  valueList.Add( DIRECT );
				  valueList.Add( INDIRECT );
			  }

			  public enum InnerEnum
			  {
				  DIRECT,
				  INDIRECT
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private Invocation( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal abstract Neo4Net.Logging.Log decorate( Neo4Net.Logging.Log log );

			 public static IList<Invocation> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static Invocation valueOf( string name )
			 {
				 foreach ( Invocation enumInstance in Invocation.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 internal abstract class Level
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           DEBUG { Logger logger(org.neo4j.logging.Log log) { return log.debugLogger(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           INFO { Logger logger(org.neo4j.logging.Log log) { return log.infoLogger(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           WARN { Logger logger(org.neo4j.logging.Log log) { return log.warnLogger(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           ERROR { Logger logger(org.neo4j.logging.Log log) { return log.errorLogger(); } };

			  private static readonly IList<Level> valueList = new List<Level>();

			  static Level()
			  {
				  valueList.Add( DEBUG );
				  valueList.Add( INFO );
				  valueList.Add( WARN );
				  valueList.Add( ERROR );
			  }

			  public enum InnerEnum
			  {
				  DEBUG,
				  INFO,
				  WARN,
				  ERROR
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private Level( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal abstract Neo4Net.Logging.Logger logger( Neo4Net.Logging.Log log );

			 public static IList<Level> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static Level valueOf( string name )
			 {
				 foreach ( Level enumInstance in Level.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 internal abstract class Style
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           MESSAGE { void invoke(AsyncLogTest state, org.neo4j.logging.Logger logger) { logger.log("a message"); } public String toString() { return " <message> "; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           THROWABLE { void invoke(AsyncLogTest state, org.neo4j.logging.Logger logger) { logger.log("an exception", exception); } public String toString() { return " <message>, <exception> "; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           FORMAT { void invoke(AsyncLogTest state, org.neo4j.logging.Logger logger) { logger.log("a %s message", "formatted"); } public String toString() { return " <format>, <parameters...> "; } };

			  private static readonly IList<Style> valueList = new List<Style>();

			  static Style()
			  {
				  valueList.Add( MESSAGE );
				  valueList.Add( THROWABLE );
				  valueList.Add( FORMAT );
			  }

			  public enum InnerEnum
			  {
				  MESSAGE,
				  THROWABLE,
				  FORMAT
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private Style( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal abstract void invoke( AsyncLogTest state, Neo4Net.Logging.Logger logger );

			 public static IList<Style> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static Style valueOf( string name )
			 {
				 foreach ( Style enumInstance in Style.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 internal class DeferredSender : AsyncEventSender<AsyncLogEvent>
		 {
			  internal readonly IList<AsyncLogEvent> Events = new List<AsyncLogEvent>();

			  public override void Send( AsyncLogEvent @event )
			  {
					Events.Add( @event );
			  }

			  public virtual void Process()
			  {
					foreach ( AsyncLogEvent @event in Events )
					{
						 @event.Process();
					}
					Events.Clear();
			  }
		 }

		 internal class DirectLog : AbstractLog
		 {
			  internal readonly Log Log;

			  internal DirectLog( Log log )
			  {
					this.Log = log;
			  }

			  public override bool DebugEnabled
			  {
				  get
				  {
						throw new System.NotSupportedException();
				  }
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void bulk(@Nonnull Consumer<org.neo4j.logging.Log> consumer)
			  public override void Bulk( Consumer<Log> consumer )
			  {
					throw new System.NotSupportedException();
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public org.neo4j.logging.Logger debugLogger()
			  public override Logger DebugLogger()
			  {
					return new LoggerAnonymousInnerClass( this );
			  }

			  private class LoggerAnonymousInnerClass : Logger
			  {
				  private readonly DirectLog _outerInstance;

				  public LoggerAnonymousInnerClass( DirectLog outerInstance )
				  {
					  this.outerInstance = outerInstance;
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String message)
				  public void log( string message )
				  {
						_outerInstance.log.debug( message );
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String message, @Nonnull Throwable throwable)
				  public void log( string message, Exception throwable )
				  {
						_outerInstance.log.debug( message, throwable );
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String format, @Nonnull Object... arguments)
				  public void log( string format, params object[] arguments )
				  {
						_outerInstance.log.debug( format, arguments );
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void bulk(@Nonnull Consumer<org.neo4j.logging.Logger> consumer)
				  public void bulk( Consumer<Logger> consumer )
				  {
						throw new System.NotSupportedException();
				  }
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public org.neo4j.logging.Logger infoLogger()
			  public override Logger InfoLogger()
			  {
					return new LoggerAnonymousInnerClass2( this );
			  }

			  private class LoggerAnonymousInnerClass2 : Logger
			  {
				  private readonly DirectLog _outerInstance;

				  public LoggerAnonymousInnerClass2( DirectLog outerInstance )
				  {
					  this.outerInstance = outerInstance;
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String message)
				  public void log( string message )
				  {
						_outerInstance.log.info( message );
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String message, @Nonnull Throwable throwable)
				  public void log( string message, Exception throwable )
				  {
						_outerInstance.log.info( message, throwable );
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String format, @Nonnull Object... arguments)
				  public void log( string format, params object[] arguments )
				  {
						_outerInstance.log.info( format, arguments );
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void bulk(@Nonnull Consumer<org.neo4j.logging.Logger> consumer)
				  public void bulk( Consumer<Logger> consumer )
				  {
						throw new System.NotSupportedException();
				  }
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public org.neo4j.logging.Logger warnLogger()
			  public override Logger WarnLogger()
			  {
					return new LoggerAnonymousInnerClass3( this );
			  }

			  private class LoggerAnonymousInnerClass3 : Logger
			  {
				  private readonly DirectLog _outerInstance;

				  public LoggerAnonymousInnerClass3( DirectLog outerInstance )
				  {
					  this.outerInstance = outerInstance;
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String message)
				  public void log( string message )
				  {
						_outerInstance.log.warn( message );
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String message, @Nonnull Throwable throwable)
				  public void log( string message, Exception throwable )
				  {
						_outerInstance.log.warn( message, throwable );
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String format, @Nonnull Object... arguments)
				  public void log( string format, params object[] arguments )
				  {
						_outerInstance.log.warn( format, arguments );
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void bulk(@Nonnull Consumer<org.neo4j.logging.Logger> consumer)
				  public void bulk( Consumer<Logger> consumer )
				  {
						throw new System.NotSupportedException();
				  }
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public org.neo4j.logging.Logger errorLogger()
			  public override Logger ErrorLogger()
			  {
					return new LoggerAnonymousInnerClass4( this );
			  }

			  private class LoggerAnonymousInnerClass4 : Logger
			  {
				  private readonly DirectLog _outerInstance;

				  public LoggerAnonymousInnerClass4( DirectLog outerInstance )
				  {
					  this.outerInstance = outerInstance;
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String message)
				  public void log( string message )
				  {
						_outerInstance.log.error( message );
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String message, @Nonnull Throwable throwable)
				  public void log( string message, Exception throwable )
				  {
						_outerInstance.log.error( message, throwable );
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String format, @Nonnull Object... arguments)
				  public void log( string format, params object[] arguments )
				  {
						_outerInstance.log.error( format, arguments );
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void bulk(@Nonnull Consumer<org.neo4j.logging.Logger> consumer)
				  public void bulk( Consumer<Logger> consumer )
				  {
						throw new System.NotSupportedException();
				  }
			  }
		 }

		 internal class MatcherBuilder : AbstractLog
		 {
			  internal readonly AssertableLogProvider.LogMatcherBuilder Builder;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal AssertableLogProvider.LogMatcher MatcherConflict;

			  internal MatcherBuilder( AssertableLogProvider.LogMatcherBuilder builder )
			  {
					this.Builder = builder;
			  }

			  public virtual AssertableLogProvider.LogMatcher Matcher()
			  {
					return requireNonNull( MatcherConflict, "invalid use, no matcher built" );
			  }

			  public override bool DebugEnabled
			  {
				  get
				  {
						return true;
				  }
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void bulk(@Nonnull Consumer<org.neo4j.logging.Log> consumer)
			  public override void Bulk( Consumer<Log> consumer )
			  {
					throw new System.NotSupportedException();
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public org.neo4j.logging.Logger debugLogger()
			  public override Logger DebugLogger()
			  {
					return new LoggerAnonymousInnerClass( this );
			  }

			  private class LoggerAnonymousInnerClass : Logger
			  {
				  private readonly MatcherBuilder _outerInstance;

				  public LoggerAnonymousInnerClass( MatcherBuilder outerInstance )
				  {
					  this.outerInstance = outerInstance;
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String message)
				  public void log( string message )
				  {
						_outerInstance.matcher = _outerInstance.builder.debug( _outerInstance.messageMatcher( message ) );
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String message, @Nonnull Throwable throwable)
				  public void log( string message, Exception throwable )
				  {
						_outerInstance.matcher = _outerInstance.builder.debug( _outerInstance.messageMatcher( message ), sameInstance( throwable ) );
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String format, @Nonnull Object... arguments)
				  public void log( string format, params object[] arguments )
				  {
						_outerInstance.matcher = _outerInstance.builder.debug( _outerInstance.messageMatcher( format ), arguments );
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void bulk(@Nonnull Consumer<org.neo4j.logging.Logger> consumer)
				  public void bulk( Consumer<Logger> consumer )
				  {
						throw new System.NotSupportedException();
				  }
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public org.neo4j.logging.Logger infoLogger()
			  public override Logger InfoLogger()
			  {
					return new LoggerAnonymousInnerClass2( this );
			  }

			  private class LoggerAnonymousInnerClass2 : Logger
			  {
				  private readonly MatcherBuilder _outerInstance;

				  public LoggerAnonymousInnerClass2( MatcherBuilder outerInstance )
				  {
					  this.outerInstance = outerInstance;
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String message)
				  public void log( string message )
				  {
						_outerInstance.matcher = _outerInstance.builder.info( _outerInstance.messageMatcher( message ) );
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String message, @Nonnull Throwable throwable)
				  public void log( string message, Exception throwable )
				  {
						_outerInstance.matcher = _outerInstance.builder.info( _outerInstance.messageMatcher( message ), sameInstance( throwable ) );
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String format, @Nonnull Object... arguments)
				  public void log( string format, params object[] arguments )
				  {
						_outerInstance.matcher = _outerInstance.builder.info( _outerInstance.messageMatcher( format ), arguments );
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void bulk(@Nonnull Consumer<org.neo4j.logging.Logger> consumer)
				  public void bulk( Consumer<Logger> consumer )
				  {
						throw new System.NotSupportedException();
				  }
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public org.neo4j.logging.Logger warnLogger()
			  public override Logger WarnLogger()
			  {
					return new LoggerAnonymousInnerClass3( this );
			  }

			  private class LoggerAnonymousInnerClass3 : Logger
			  {
				  private readonly MatcherBuilder _outerInstance;

				  public LoggerAnonymousInnerClass3( MatcherBuilder outerInstance )
				  {
					  this.outerInstance = outerInstance;
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String message)
				  public void log( string message )
				  {
						_outerInstance.matcher = _outerInstance.builder.warn( _outerInstance.messageMatcher( message ) );
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String message, @Nonnull Throwable throwable)
				  public void log( string message, Exception throwable )
				  {
						_outerInstance.matcher = _outerInstance.builder.warn( _outerInstance.messageMatcher( message ), sameInstance( throwable ) );
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String format, @Nonnull Object... arguments)
				  public void log( string format, params object[] arguments )
				  {
						_outerInstance.matcher = _outerInstance.builder.warn( _outerInstance.messageMatcher( format ), arguments );
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void bulk(@Nonnull Consumer<org.neo4j.logging.Logger> consumer)
				  public void bulk( Consumer<Logger> consumer )
				  {
						throw new System.NotSupportedException();
				  }
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public org.neo4j.logging.Logger errorLogger()
			  public override Logger ErrorLogger()
			  {
					return new LoggerAnonymousInnerClass4( this );
			  }

			  private class LoggerAnonymousInnerClass4 : Logger
			  {
				  private readonly MatcherBuilder _outerInstance;

				  public LoggerAnonymousInnerClass4( MatcherBuilder outerInstance )
				  {
					  this.outerInstance = outerInstance;
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String message)
				  public void log( string message )
				  {
						_outerInstance.matcher = _outerInstance.builder.error( _outerInstance.messageMatcher( message ) );
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String message, @Nonnull Throwable throwable)
				  public void log( string message, Exception throwable )
				  {
						_outerInstance.matcher = _outerInstance.builder.error( _outerInstance.messageMatcher( message ), sameInstance( throwable ) );
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void log(@Nonnull String format, @Nonnull Object... arguments)
				  public void log( string format, params object[] arguments )
				  {
						_outerInstance.matcher = _outerInstance.builder.error( _outerInstance.messageMatcher( format ), arguments );
				  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void bulk(@Nonnull Consumer<org.neo4j.logging.Logger> consumer)
				  public void bulk( Consumer<Logger> consumer )
				  {
						throw new System.NotSupportedException();
				  }
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: private org.hamcrest.Matcher<String> messageMatcher(@Nonnull String message)
			  internal virtual Matcher<string> MessageMatcher( string message )
			  {
					return allOf( startsWith( "[AsyncLog @ " ), endsWith( "]  " + message ) );
			  }
		 }
	}

}