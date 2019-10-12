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
namespace Org.Neo4j.Logging
{

	/// <summary>
	/// Buffers all messages sent to it, and is able to replay those messages into
	/// another Logger.
	/// <para>
	/// This can be used to start up services that need logging when they start, but
	/// where, for one reason or another, we have not yet set up proper logging in
	/// the application lifecycle.
	/// </para>
	/// <para>
	/// This will replay messages in the order they are received, *however*, it will
	/// not preserve the time stamps of the original messages.
	/// </para>
	/// <para>
	/// You should not use this for logging messages where the time stamps are
	/// important.
	/// </para>
	/// <para>
	/// You should also not use this logger, when there is a risk that it can be
	/// subjected to an unbounded quantity of log messages, since the buffer keeps
	/// all messages until it gets a chance to replay them.
	/// </para>
	/// </summary>
	public class BufferingLog : AbstractLog
	{
		 private interface LogMessage
		 {
			  void ReplayInto( Log other );

			  void PrintTo( PrintWriter pw );
		 }

		 private readonly LinkedList<LogMessage> _buffer = new LinkedList<LogMessage>();

		 private abstract class BufferingLogger : Logger
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public abstract void bulk(@Nonnull Consumer<Logger> consumer);
			 public abstract void Bulk( Consumer<Logger> consumer );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public abstract void log(@Nonnull String format, @Nullable Object... arguments);
			 public abstract void Log( string format, params object[] arguments );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public abstract void log(@Nonnull String message, @Nonnull Throwable throwable);
			 public abstract void Log( string message, Exception throwable );
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public abstract void log(@Nonnull String message);
			 public abstract void Log( string message );
			 private readonly BufferingLog _outerInstance;

			 public BufferingLogger( BufferingLog outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void log(@Nonnull String message)
			  public override void Log( string message )
			  {
					LogMessage logMessage = BuildMessage( message );
					lock ( outerInstance.buffer )
					{
						 outerInstance.buffer.AddLast( logMessage );
					}
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: protected abstract LogMessage buildMessage(@Nonnull String message);
			  protected internal abstract LogMessage BuildMessage( string message );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void log(@Nonnull final String message, @Nonnull final Throwable throwable)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
			  public override void Log( string message, Exception throwable )
			  {
					LogMessage logMessage = BuildMessage( message, throwable );
					lock ( outerInstance.buffer )
					{
						 outerInstance.buffer.AddLast( logMessage );
					}
			  }

			  protected internal abstract LogMessage BuildMessage( string message, Exception throwable );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void log(@Nonnull String format, @Nonnull Object... arguments)
			  public override void Log( string format, params object[] arguments )
			  {
					LogMessage logMessage = BuildMessage( format, arguments );
					lock ( outerInstance.buffer )
					{
						 outerInstance.buffer.AddLast( logMessage );
					}
			  }

			  protected internal abstract LogMessage BuildMessage( string message, params object[] arguments );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void bulk(@Nonnull Consumer<Logger> consumer)
			  public override void Bulk( Consumer<Logger> consumer )
			  {
					lock ( outerInstance.buffer )
					{
						 consumer.accept( this );
					}
			  }
		 }

		 private readonly Logger debugLogger = new BufferingLoggerAnonymousInnerClass();

		 private class BufferingLoggerAnonymousInnerClass : BufferingLogger
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public LogMessage buildMessage(@Nonnull final String message)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
			 public override LogMessage buildMessage( string message )
			 {
				  return new LogMessageAnonymousInnerClass( this, message );
			 }

			 private class LogMessageAnonymousInnerClass : LogMessage
			 {
				 private readonly BufferingLoggerAnonymousInnerClass _outerInstance;

				 private string _message;

				 public LogMessageAnonymousInnerClass( BufferingLoggerAnonymousInnerClass outerInstance, string message )
				 {
					 this.outerInstance = outerInstance;
					 this._message = message;
				 }

				 public void replayInto( Log other )
				 {
					  other.Debug( _message );
				 }

				 public void printTo( PrintWriter pw )
				 {
					  pw.println( _message );
				 }
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public LogMessage buildMessage(@Nonnull final String message, @Nonnull final Throwable throwable)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
			 public override LogMessage buildMessage( string message, Exception throwable )
			 {
				  return new LogMessageAnonymousInnerClass2( this, message, throwable );
			 }

			 private class LogMessageAnonymousInnerClass2 : LogMessage
			 {
				 private readonly BufferingLoggerAnonymousInnerClass _outerInstance;

				 private string _message;
				 private Exception _throwable;

				 public LogMessageAnonymousInnerClass2( BufferingLoggerAnonymousInnerClass outerInstance, string message, Exception throwable )
				 {
					 this.outerInstance = outerInstance;
					 this._message = message;
					 this._throwable = throwable;
				 }

				 public void replayInto( Log other )
				 {
					  other.Debug( _message, _throwable );
				 }

				 public void printTo( PrintWriter pw )
				 {
					  pw.println( _message );
					  _throwable.printStackTrace( pw );
				 }
			 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public LogMessage buildMessage(final String format, final Object... arguments)
			 public override LogMessage buildMessage( string format, params object[] arguments )
			 {
				  return new LogMessageAnonymousInnerClass3( this, format, arguments );
			 }

			 private class LogMessageAnonymousInnerClass3 : LogMessage
			 {
				 private readonly BufferingLoggerAnonymousInnerClass _outerInstance;

				 private string _format;
				 private object[] _arguments;

				 public LogMessageAnonymousInnerClass3( BufferingLoggerAnonymousInnerClass outerInstance, string format, object[] arguments )
				 {
					 this.outerInstance = outerInstance;
					 this._format = format;
					 this._arguments = arguments;
				 }

				 public void replayInto( Log other )
				 {
					  other.Debug( _format, _arguments );
				 }

				 public void printTo( PrintWriter pw )
				 {
					  pw.println( string.format( _format, _arguments ) );
				 }
			 }
		 }

		 private readonly Logger infoLogger = new BufferingLoggerAnonymousInnerClass2();

		 private class BufferingLoggerAnonymousInnerClass2 : BufferingLogger
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public LogMessage buildMessage(@Nonnull final String message)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
			 public override LogMessage buildMessage( string message )
			 {
				  return new LogMessageAnonymousInnerClass4( this, message );
			 }

			 private class LogMessageAnonymousInnerClass4 : LogMessage
			 {
				 private readonly BufferingLoggerAnonymousInnerClass2 _outerInstance;

				 private string _message;

				 public LogMessageAnonymousInnerClass4( BufferingLoggerAnonymousInnerClass2 outerInstance, string message )
				 {
					 this.outerInstance = outerInstance;
					 this._message = message;
				 }

				 public void replayInto( Log other )
				 {
					  other.Info( _message );
				 }

				 public void printTo( PrintWriter pw )
				 {
					  pw.println( _message );
				 }
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public LogMessage buildMessage(@Nonnull final String message, @Nonnull final Throwable throwable)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
			 public override LogMessage buildMessage( string message, Exception throwable )
			 {
				  return new LogMessageAnonymousInnerClass5( this, message, throwable );
			 }

			 private class LogMessageAnonymousInnerClass5 : LogMessage
			 {
				 private readonly BufferingLoggerAnonymousInnerClass2 _outerInstance;

				 private string _message;
				 private Exception _throwable;

				 public LogMessageAnonymousInnerClass5( BufferingLoggerAnonymousInnerClass2 outerInstance, string message, Exception throwable )
				 {
					 this.outerInstance = outerInstance;
					 this._message = message;
					 this._throwable = throwable;
				 }

				 public void replayInto( Log other )
				 {
					  other.Info( _message, _throwable );
				 }

				 public void printTo( PrintWriter pw )
				 {
					  pw.println( _message );
					  _throwable.printStackTrace( pw );
				 }
			 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public LogMessage buildMessage(final String format, final Object... arguments)
			 public override LogMessage buildMessage( string format, params object[] arguments )
			 {
				  return new LogMessageAnonymousInnerClass6( this, format, arguments );
			 }

			 private class LogMessageAnonymousInnerClass6 : LogMessage
			 {
				 private readonly BufferingLoggerAnonymousInnerClass2 _outerInstance;

				 private string _format;
				 private object[] _arguments;

				 public LogMessageAnonymousInnerClass6( BufferingLoggerAnonymousInnerClass2 outerInstance, string format, object[] arguments )
				 {
					 this.outerInstance = outerInstance;
					 this._format = format;
					 this._arguments = arguments;
				 }

				 public void replayInto( Log other )
				 {
					  other.Info( _format, _arguments );
				 }

				 public void printTo( PrintWriter pw )
				 {
					  pw.println( string.format( _format, _arguments ) );
				 }
			 }
		 }

		 private readonly Logger warnLogger = new BufferingLoggerAnonymousInnerClass3();

		 private class BufferingLoggerAnonymousInnerClass3 : BufferingLogger
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public LogMessage buildMessage(@Nonnull final String message)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
			 public override LogMessage buildMessage( string message )
			 {
				  return new LogMessageAnonymousInnerClass7( this, message );
			 }

			 private class LogMessageAnonymousInnerClass7 : LogMessage
			 {
				 private readonly BufferingLoggerAnonymousInnerClass3 _outerInstance;

				 private string _message;

				 public LogMessageAnonymousInnerClass7( BufferingLoggerAnonymousInnerClass3 outerInstance, string message )
				 {
					 this.outerInstance = outerInstance;
					 this._message = message;
				 }

				 public void replayInto( Log other )
				 {
					  other.Warn( _message );
				 }

				 public void printTo( PrintWriter pw )
				 {
					  pw.println( _message );
				 }
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public LogMessage buildMessage(@Nonnull final String message, @Nonnull final Throwable throwable)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
			 public override LogMessage buildMessage( string message, Exception throwable )
			 {
				  return new LogMessageAnonymousInnerClass8( this, message, throwable );
			 }

			 private class LogMessageAnonymousInnerClass8 : LogMessage
			 {
				 private readonly BufferingLoggerAnonymousInnerClass3 _outerInstance;

				 private string _message;
				 private Exception _throwable;

				 public LogMessageAnonymousInnerClass8( BufferingLoggerAnonymousInnerClass3 outerInstance, string message, Exception throwable )
				 {
					 this.outerInstance = outerInstance;
					 this._message = message;
					 this._throwable = throwable;
				 }

				 public void replayInto( Log other )
				 {
					  other.Warn( _message, _throwable );
				 }

				 public void printTo( PrintWriter pw )
				 {
					  pw.println( _message );
					  _throwable.printStackTrace( pw );
				 }
			 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public LogMessage buildMessage(final String format, final Object... arguments)
			 public override LogMessage buildMessage( string format, params object[] arguments )
			 {
				  return new LogMessageAnonymousInnerClass9( this, format, arguments );
			 }

			 private class LogMessageAnonymousInnerClass9 : LogMessage
			 {
				 private readonly BufferingLoggerAnonymousInnerClass3 _outerInstance;

				 private string _format;
				 private object[] _arguments;

				 public LogMessageAnonymousInnerClass9( BufferingLoggerAnonymousInnerClass3 outerInstance, string format, object[] arguments )
				 {
					 this.outerInstance = outerInstance;
					 this._format = format;
					 this._arguments = arguments;
				 }

				 public void replayInto( Log other )
				 {
					  other.Warn( _format, _arguments );
				 }

				 public void printTo( PrintWriter pw )
				 {
					  pw.println( string.format( _format, _arguments ) );
				 }
			 }
		 }

		 private readonly Logger errorLogger = new BufferingLoggerAnonymousInnerClass4();

		 private class BufferingLoggerAnonymousInnerClass4 : BufferingLogger
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public LogMessage buildMessage(@Nonnull final String message)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
			 public override LogMessage buildMessage( string message )
			 {
				  return new LogMessageAnonymousInnerClass10( this, message );
			 }

			 private class LogMessageAnonymousInnerClass10 : LogMessage
			 {
				 private readonly BufferingLoggerAnonymousInnerClass4 _outerInstance;

				 private string _message;

				 public LogMessageAnonymousInnerClass10( BufferingLoggerAnonymousInnerClass4 outerInstance, string message )
				 {
					 this.outerInstance = outerInstance;
					 this._message = message;
				 }

				 public void replayInto( Log other )
				 {
					  other.Error( _message );
				 }

				 public void printTo( PrintWriter pw )
				 {
					  pw.println( _message );
				 }
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public LogMessage buildMessage(@Nonnull final String message, @Nonnull final Throwable throwable)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
			 public override LogMessage buildMessage( string message, Exception throwable )
			 {
				  return new LogMessageAnonymousInnerClass11( this, message, throwable );
			 }

			 private class LogMessageAnonymousInnerClass11 : LogMessage
			 {
				 private readonly BufferingLoggerAnonymousInnerClass4 _outerInstance;

				 private string _message;
				 private Exception _throwable;

				 public LogMessageAnonymousInnerClass11( BufferingLoggerAnonymousInnerClass4 outerInstance, string message, Exception throwable )
				 {
					 this.outerInstance = outerInstance;
					 this._message = message;
					 this._throwable = throwable;
				 }

				 public void replayInto( Log other )
				 {
					  other.Error( _message, _throwable );
				 }

				 public void printTo( PrintWriter pw )
				 {
					  pw.println( _message );
					  _throwable.printStackTrace( pw );
				 }
			 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public LogMessage buildMessage(final String format, final Object... arguments)
			 public override LogMessage buildMessage( string format, params object[] arguments )
			 {
				  return new LogMessageAnonymousInnerClass12( this, format, arguments );
			 }

			 private class LogMessageAnonymousInnerClass12 : LogMessage
			 {
				 private readonly BufferingLoggerAnonymousInnerClass4 _outerInstance;

				 private string _format;
				 private object[] _arguments;

				 public LogMessageAnonymousInnerClass12( BufferingLoggerAnonymousInnerClass4 outerInstance, string format, object[] arguments )
				 {
					 this.outerInstance = outerInstance;
					 this._format = format;
					 this._arguments = arguments;
				 }

				 public void replayInto( Log other )
				 {
					  other.Error( _format, _arguments );
				 }

				 public void printTo( PrintWriter pw )
				 {
					  pw.println( string.format( _format, _arguments ) );
				 }
			 }
		 }

		 public override bool DebugEnabled
		 {
			 get
			 {
				  return true;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public Logger debugLogger()
		 public override Logger DebugLogger()
		 {
			  return this.debugLogger;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public Logger infoLogger()
		 public override Logger InfoLogger()
		 {
			  return infoLogger;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public Logger warnLogger()
		 public override Logger WarnLogger()
		 {
			  return warnLogger;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public Logger errorLogger()
		 public override Logger ErrorLogger()
		 {
			  return errorLogger;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void bulk(@Nonnull Consumer<Log> consumer)
		 public override void Bulk( Consumer<Log> consumer )
		 {
			  lock ( _buffer )
			  {
					consumer.accept( this );
			  }
		 }

		 /// <summary>
		 /// Replays buffered messages and clears the buffer.
		 /// </summary>
		 /// <param name="other"> the log to reply into </param>
		 public virtual void ReplayInto( Log other )
		 {
			  lock ( _buffer )
			  {
					LogMessage message = _buffer.RemoveFirst();
					while ( message != null )
					{
						 message.ReplayInto( other );
						 message = _buffer.RemoveFirst();
					}
			  }
		 }

		 public override string ToString()
		 {
			  lock ( _buffer )
			  {
					StringWriter stringWriter = new StringWriter();
					PrintWriter sb = new PrintWriter( stringWriter );
					foreach ( LogMessage message in _buffer )
					{
						 message.PrintTo( sb );
					}
					return stringWriter.ToString();
			  }
		 }
	}

}