using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.causalclustering.discovery
{
	using AbstractLogger = com.hazelcast.logging.AbstractLogger;
	using LogEvent = com.hazelcast.logging.LogEvent;


	using Log = Org.Neo4j.Logging.Log;
	using Logger = Org.Neo4j.Logging.Logger;
	using NullLogger = Org.Neo4j.Logging.NullLogger;

	public class HazelcastLogger : AbstractLogger
	{
		 private readonly Log _log;
		 private readonly Level _minLevel;

		 internal HazelcastLogger( Log log, Level minLevel )
		 {
			  this._log = log;
			  this._minLevel = minLevel;
		 }

		 public override void Log( Level level, string message )
		 {
			  GetLogger( level ).log( message );
		 }

		 public override void Log( Level level, string message, Exception thrown )
		 {
			  GetLogger( level ).log( message, thrown );
		 }

		 public override void Log( LogEvent logEvent )
		 {
			  LogRecord logRecord = logEvent.LogRecord;

			  string message = "Member[" + logEvent.Member + "] " + logRecord.Message;

			  Logger logger = GetLogger( logRecord.Level );
			  Exception thrown = logRecord.Thrown;

			  if ( thrown == null )
			  {
					logger.Log( message );
			  }
			  else
			  {
					logger.Log( message, thrown );
			  }
		 }

		 public override Level Level
		 {
			 get
			 {
				  return _minLevel;
			 }
		 }

		 public override bool IsLoggable( Level level )
		 {
			  return level.intValue() >= _minLevel.intValue();
		 }

		 private Logger GetLogger( Level level )
		 {
			  int levelValue = level.intValue();

			  if ( levelValue < _minLevel.intValue() )
			  {
					return NullLogger.Instance;
			  }
			  else if ( levelValue <= Level.FINE.intValue() )
			  {
					return _log.debugLogger();
			  }
			  else if ( levelValue <= Level.INFO.intValue() )
			  {
					return _log.infoLogger();
			  }
			  else if ( levelValue <= Level.WARNING.intValue() )
			  {
					return _log.warnLogger();
			  }
			  else
			  {
					return _log.errorLogger();
			  }
		 }
	}

}