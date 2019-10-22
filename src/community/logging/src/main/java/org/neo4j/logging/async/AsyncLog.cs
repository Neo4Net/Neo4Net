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

	using Neo4Net.Utils.Concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.logging.async.AsyncLogEvent.bulkLogEvent;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.logging.async.AsyncLogEvent.logEvent;

	public class AsyncLog : AbstractLog
	{
		 private readonly Log _log;
		 private readonly AsyncEventSender<AsyncLogEvent> _events;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public AsyncLog(@Nonnull AsyncEventSender<AsyncLogEvent> events, @Nonnull Log log)
		 public AsyncLog( AsyncEventSender<AsyncLogEvent> events, Log log )
		 {
			  this._log = requireNonNull( log, "Log" );
			  this._events = requireNonNull( events, "AsyncEventSender<AsyncLogEvent>" );
		 }

		 public override bool DebugEnabled
		 {
			 get
			 {
				  return _log.DebugEnabled;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public org.Neo4Net.logging.Logger debugLogger()
		 public override Logger DebugLogger()
		 {
			  return new AsyncLogger( _events, _log.debugLogger() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public org.Neo4Net.logging.Logger infoLogger()
		 public override Logger InfoLogger()
		 {
			  return new AsyncLogger( _events, _log.infoLogger() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public org.Neo4Net.logging.Logger warnLogger()
		 public override Logger WarnLogger()
		 {
			  return new AsyncLogger( _events, _log.warnLogger() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public org.Neo4Net.logging.Logger errorLogger()
		 public override Logger ErrorLogger()
		 {
			  return new AsyncLogger( _events, _log.errorLogger() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void bulk(@Nonnull Consumer<org.Neo4Net.logging.Log> consumer)
		 public override void Bulk( Consumer<Log> consumer )
		 {
			  _events.send( bulkLogEvent( _log, consumer ) );
		 }

		 private class AsyncLogger : Logger
		 {
			  internal readonly Logger Logger;
			  internal readonly AsyncEventSender<AsyncLogEvent> Events;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: AsyncLogger(@Nonnull AsyncEventSender<AsyncLogEvent> events, @Nonnull Logger logger)
			  internal AsyncLogger( AsyncEventSender<AsyncLogEvent> events, Logger logger )
			  {
					this.Logger = requireNonNull( logger, "Logger" );
					this.Events = requireNonNull( events );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void log(@Nonnull String message)
			  public override void Log( string message )
			  {
					Events.send( logEvent( Logger, message ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void log(@Nonnull String message, @Nonnull Throwable throwable)
			  public override void Log( string message, Exception throwable )
			  {
					requireNonNull( throwable );
					Events.send( logEvent( Logger, message, throwable ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void log(@Nonnull String format, @Nonnull Object... arguments)
			  public override void Log( string format, params object[] arguments )
			  {
					requireNonNull( arguments );
					Events.send( logEvent( Logger, format, arguments ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void bulk(@Nonnull Consumer<org.Neo4Net.logging.Logger> consumer)
			  public override void Bulk( Consumer<Logger> consumer )
			  {
					Events.send( bulkLogEvent( Logger, consumer ) );
			  }
		 }
	}

}