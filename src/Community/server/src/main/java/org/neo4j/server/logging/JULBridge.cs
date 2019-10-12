using System;
using System.Collections.Concurrent;

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
namespace Neo4Net.Server.logging
{

	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using Logger = Neo4Net.Logging.Logger;

	public class JULBridge : Handler
	{
		 private const string UNKNOWN_LOGGER_NAME = "unknown";

		 private readonly LogProvider _logProvider;
		 private readonly ConcurrentMap<string, Log> _logs = new ConcurrentDictionary<string, Log>();

		 protected internal JULBridge( LogProvider logProvider )
		 {
			  this._logProvider = logProvider;
		 }

		 public static void ResetJUL()
		 {
			  LogManager.LogManager.reset();
		 }

		 public static void ForwardTo( LogProvider logProvider )
		 {
			  RootJULLogger().addHandler(new JULBridge(logProvider));
		 }

		 private static java.util.logging.Logger RootJULLogger()
		 {
			  return LogManager.LogManager.getLogger( "" );
		 }

		 public override void Publish( LogRecord record )
		 {
			  if ( record == null )
			  {
					return;
			  }

			  string message = GetMessage( record );
			  if ( string.ReferenceEquals( message, null ) )
			  {
					return;
			  }

			  string context = record.LoggerName;
			  Log log = GetLog( ( !string.ReferenceEquals( context, null ) ) ? context : UNKNOWN_LOGGER_NAME );
			  Logger logger = GetLogger( record, log );

			  Exception throwable = record.Thrown;
			  if ( throwable == null )
			  {
					logger.Log( message );
			  }
			  else
			  {
					logger.Log( message, throwable );
			  }
		 }

		 private Logger GetLogger( LogRecord record, Log log )
		 {
			  int level = record.Level.intValue();
			  if ( level <= Level.FINE.intValue() )
			  {
					return log.DebugLogger();
			  }
			  else if ( level <= Level.INFO.intValue() )
			  {
					return log.InfoLogger();
			  }
			  else if ( level <= Level.WARNING.intValue() )
			  {
					return log.WarnLogger();
			  }
			  else
			  {
					return log.ErrorLogger();
			  }
		 }

		 private Log GetLog( string name )
		 {
			  Log log = _logs.get( name );
			  if ( log == null )
			  {
					Log newLog = _logProvider.getLog( name );
					log = _logs.putIfAbsent( name, newLog );
					if ( log == null )
					{
						 log = newLog;
					}
			  }
			  return log;
		 }

		 private string GetMessage( LogRecord record )
		 {
			  string message = record.Message;
			  if ( string.ReferenceEquals( message, null ) )
			  {
					return null;
			  }

			  ResourceBundle bundle = record.ResourceBundle;
			  if ( bundle != null )
			  {
					try
					{
						 message = bundle.getString( message );
					}
					catch ( MissingResourceException )
					{
						 // leave message as it was
					}
			  }

			  object[] @params = record.Parameters;
			  if ( @params != null && @params.Length > 0 )
			  {
					message = MessageFormat.format( message, @params );
			  }
			  return message;
		 }

		 public override void Flush()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws SecurityException
		 public override void Close()
		 {
		 }
	}

}