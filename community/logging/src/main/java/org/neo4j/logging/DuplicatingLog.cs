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
	/// A <seealso cref="Log"/> implementation that duplicates all messages to other Log instances
	/// </summary>
	public class DuplicatingLog : AbstractLog
	{
		 private readonly CopyOnWriteArraySet<Log> _logs;
		 private readonly DuplicatingLogger _debugLogger;
		 private readonly DuplicatingLogger _infoLogger;
		 private readonly DuplicatingLogger _warnLogger;
		 private readonly DuplicatingLogger _errorLogger;

		 /// <param name="logs"> A list of <seealso cref="Log"/> instances that messages should be duplicated to </param>
		 public DuplicatingLog( params Log[] logs ) : this( Arrays.asList( logs ) )
		 {
		 }

		 /// <param name="logs"> A list of <seealso cref="Log"/> instances that messages should be duplicated to </param>
		 public DuplicatingLog( IList<Log> logs )
		 {
			  List<Logger> debugLoggers = new List<Logger>( logs.Count );
			  List<Logger> infoLoggers = new List<Logger>( logs.Count );
			  List<Logger> warnLoggers = new List<Logger>( logs.Count );
			  List<Logger> errorLoggers = new List<Logger>( logs.Count );

			  foreach ( Log log in logs )
			  {
					debugLoggers.Add( log.DebugLogger() );
					infoLoggers.Add( log.InfoLogger() );
					warnLoggers.Add( log.WarnLogger() );
					errorLoggers.Add( log.ErrorLogger() );
			  }

			  this._logs = new CopyOnWriteArraySet<Log>( logs );
			  this._debugLogger = new DuplicatingLogger( debugLoggers );
			  this._infoLogger = new DuplicatingLogger( infoLoggers );
			  this._warnLogger = new DuplicatingLogger( warnLoggers );
			  this._errorLogger = new DuplicatingLogger( errorLoggers );
		 }

		 /// <summary>
		 /// Remove a <seealso cref="Log"/> from the duplicating set
		 /// </summary>
		 /// <param name="log"> the Log to be removed </param>
		 /// <returns> true if the log was found and removed </returns>
		 public virtual bool Remove( Log log )
		 {
			  bool removed = this._logs.remove( log );
			  this._debugLogger.remove( log.DebugLogger() );
			  this._infoLogger.remove( log.InfoLogger() );
			  this._warnLogger.remove( log.WarnLogger() );
			  this._errorLogger.remove( log.ErrorLogger() );
			  return removed;
		 }

		 public override bool DebugEnabled
		 {
			 get
			 {
				  foreach ( Log log in _logs )
				  {
						if ( log.DebugEnabled )
						{
							 return true;
						}
				  }
				  return false;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public Logger debugLogger()
		 public override Logger DebugLogger()
		 {
			  return this._debugLogger;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public Logger infoLogger()
		 public override Logger InfoLogger()
		 {
			  return this._infoLogger;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public Logger warnLogger()
		 public override Logger WarnLogger()
		 {
			  return this._warnLogger;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Nonnull @Override public Logger errorLogger()
		 public override Logger ErrorLogger()
		 {
			  return this._errorLogger;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void bulk(@Nonnull Consumer<Log> consumer)
		 public override void Bulk( Consumer<Log> consumer )
		 {
			  Bulk( new LinkedList<Log>( _logs ), new List<Log>( _logs.size() ), consumer );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static void bulk(final java.util.LinkedList<Log> remaining, final java.util.ArrayList<Log> bulkLogs, final System.Action<Log> finalConsumer)
		 private static void Bulk( LinkedList<Log> remaining, List<Log> bulkLogs, System.Action<Log> finalConsumer )
		 {
			  if ( remaining.Count > 0 )
			  {
					Log log = remaining.pop();
					log.Bulk(bulkLog =>
					{
					 bulkLogs.Add( bulkLog );
					 Bulk( remaining, bulkLogs, finalConsumer );
					});
			  }
			  else
			  {
					Log log = new DuplicatingLog( bulkLogs );
					finalConsumer( log );
			  }
		 }

		 private class DuplicatingLogger : Logger
		 {
			  internal readonly CopyOnWriteArraySet<Logger> Loggers;

			  internal DuplicatingLogger( IList<Logger> loggers )
			  {
					this.Loggers = new CopyOnWriteArraySet<Logger>( loggers );
			  }

			  public virtual bool Remove( Logger logger )
			  {
					return this.Loggers.remove( logger );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void log(@Nonnull String message)
			  public override void Log( string message )
			  {
					foreach ( Logger logger in Loggers )
					{
						 logger.Log( message );
					}
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void log(@Nonnull String message, @Nonnull Throwable throwable)
			  public override void Log( string message, Exception throwable )
			  {
					foreach ( Logger logger in Loggers )
					{
						 logger.Log( message, throwable );
					}
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void log(@Nonnull String format, @Nonnull Object... arguments)
			  public override void Log( string format, params object[] arguments )
			  {
					foreach ( Logger logger in Loggers )
					{
						 logger.Log( format, arguments );
					}
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void bulk(@Nonnull Consumer<Logger> consumer)
			  public override void Bulk( Consumer<Logger> consumer )
			  {
					Bulk( new LinkedList<Logger>( Loggers ), new List<Logger>( Loggers.size() ), consumer );
			  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static void bulk(final java.util.LinkedList<Logger> remaining, final java.util.ArrayList<Logger> bulkLoggers, final System.Action<Logger> finalConsumer)
			  internal static void Bulk( LinkedList<Logger> remaining, List<Logger> bulkLoggers, System.Action<Logger> finalConsumer )
			  {
					if ( remaining.Count > 0 )
					{
						 Logger logger = remaining.pop();
						 logger.Bulk(bulkLogger =>
						 {
						  bulkLoggers.Add( bulkLogger );
						  Bulk( remaining, bulkLoggers, finalConsumer );
						 });
					}
					else
					{
						 Logger logger = new DuplicatingLogger( bulkLoggers );
						 finalConsumer( logger );
					}
			  }
		 }
	}

}