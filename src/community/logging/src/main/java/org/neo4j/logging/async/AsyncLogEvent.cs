using System;

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

	using AsyncEvent = Neo4Net.Utils.Concurrent.AsyncEvent;

	public sealed class AsyncLogEvent : AsyncEvent
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: static AsyncLogEvent logEvent(@Nonnull Logger logger, @Nonnull String message)
		 internal static AsyncLogEvent LogEvent( Logger logger, string message )
		 {
			  return new AsyncLogEvent( requireNonNull( logger, "logger" ), requireNonNull( message, "message" ), null );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: static AsyncLogEvent logEvent(@Nonnull Logger logger, @Nonnull String message, @Nonnull Throwable throwable)
		 internal static AsyncLogEvent LogEvent( Logger logger, string message, Exception throwable )
		 {
			  return new AsyncLogEvent( requireNonNull( logger, "logger" ), requireNonNull( message, "message" ), requireNonNull( throwable, "Throwable" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: static AsyncLogEvent logEvent(@Nonnull Logger logger, @Nonnull String format, @Nullable Object... arguments)
		 internal static AsyncLogEvent LogEvent( Logger logger, string format, params object[] arguments )
		 {
			  return new AsyncLogEvent( requireNonNull( logger, "logger" ), requireNonNull( format, "format" ), arguments == null ? new object[0] : arguments );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: static AsyncLogEvent bulkLogEvent(@Nonnull Log log, @Nonnull final java.util.function.Consumer<org.Neo4Net.logging.Log> consumer)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 internal static AsyncLogEvent BulkLogEvent( Log log, System.Action<Log> consumer )
		 {
			  requireNonNull( consumer, "Consumer<Log>" );
			  return new AsyncLogEvent( requireNonNull( log, "log" ), new BulkLoggerAnonymousInnerClass( consumer ) );
		 }

		 private class BulkLoggerAnonymousInnerClass : BulkLogger
		 {
			 private System.Action<Log> _consumer;

			 public BulkLoggerAnonymousInnerClass( System.Action<Log> consumer )
			 {
				 this._consumer = consumer;
			 }

			 internal override void process( long timestamp, object target )
			 {
				  _consumer( ( Log ) target ); // TODO: include timestamp!
			 }

			 public override string ToString()
			 {
				  return "Log.bulkLog( " + _consumer + " )";
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: static AsyncLogEvent bulkLogEvent(@Nonnull Logger logger, @Nonnull final java.util.function.Consumer<org.Neo4Net.logging.Logger> consumer)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 internal static AsyncLogEvent BulkLogEvent( Logger logger, System.Action<Logger> consumer )
		 {
			  requireNonNull( consumer, "Consumer<Logger>" );
			  return new AsyncLogEvent( requireNonNull( logger, "logger" ), new BulkLoggerAnonymousInnerClass2( consumer ) );
		 }

		 private class BulkLoggerAnonymousInnerClass2 : BulkLogger
		 {
			 private System.Action<Logger> _consumer;

			 public BulkLoggerAnonymousInnerClass2( System.Action<Logger> consumer )
			 {
				 this._consumer = consumer;
			 }

			 internal override void process( long timestamp, object target )
			 {
				  _consumer( ( Logger ) target ); // TODO: include timestamp!
			 }

			 public override string ToString()
			 {
				  return "Logger.bulkLog( " + _consumer + " )";
			 }
		 }

		 public void Process()
		 {
			  if ( _parameter == null )
			  {
					( ( Logger ) _target ).log( "[AsyncLog @ " + Timestamp() + "]  " + _message );
			  }
			  else if ( _parameter is Exception )
			  {
					( ( Logger ) _target ).log( "[AsyncLog @ " + Timestamp() + "]  " + _message, (Exception) _parameter );
			  }
			  else if ( _parameter is object[] )
			  {
					( ( Logger ) _target ).log( "[AsyncLog @ " + Timestamp() + "]  " + _message, (object[]) _parameter );
			  }
			  else if ( _parameter is BulkLogger )
			  {
					( ( BulkLogger ) _parameter ).process( _timestamp, _target );
			  }
		 }

		 private readonly long _timestamp;
		 private readonly object _target;
		 private readonly string _message;
		 private readonly object _parameter;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: private AsyncLogEvent(@Nonnull Object target, @Nullable Object parameter)
		 private AsyncLogEvent( object target, object parameter ) : this( target, "", parameter )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: private AsyncLogEvent(@Nonnull Object target, @Nonnull String message, @Nullable Object parameter)
		 private AsyncLogEvent( object target, string message, object parameter )
		 {
			  this._target = target;
			  this._message = message;
			  this._parameter = parameter;
			  this._timestamp = DateTimeHelper.CurrentUnixTimeMillis();
		 }

		 public override string ToString()
		 {
			  if ( _parameter == null )
			  {
					return "log( @ " + Timestamp() + ": \"" + _message + "\" )";
			  }
			  if ( _parameter is Exception )
			  {
					return "log( @ " + Timestamp() + ": \"" + _message + "\", " + _parameter + " )";
			  }
			  if ( _parameter is object[] )
			  {
					return "log( @ " + Timestamp() + ": \"" + _message + "\", " + Arrays.ToString((object[]) _parameter) + " )";
			  }
			  if ( _parameter is BulkLogger )
			  {
					return _parameter.ToString();
			  }
			  return base.ToString();
		 }

		 private abstract class BulkLogger
		 {
			  internal abstract void Process( long timestamp, object target );
		 }

		 private string Timestamp()
		 {
			  return _dateFormatThreadLocal.get().format(new DateTime(_timestamp));
		 }

		 private static readonly ThreadLocal<DateFormat> _dateFormatThreadLocal = ThreadLocal.withInitial(() =>
		 {
		 SimpleDateFormat format = new SimpleDateFormat( "yyyy-MM-dd HH:mm:ss.SSSZ" );
		 format.TimeZone = TimeZone.getTimeZone( "UTC" );
		 return format;
		 });
	}


}