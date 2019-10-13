using System.Collections.Concurrent;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.causalclustering.messaging.address
{

	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Log = Neo4Net.Logging.Log;
	using CappedLogger = Neo4Net.Logging.@internal.CappedLogger;

	public class UnknownAddressMonitor
	{
		 private readonly Log _log;
		 private readonly Clock _clock;
		 private readonly long _timeLimitMs;
		 private IDictionary<MemberId, CappedLogger> _loggers = new ConcurrentDictionary<MemberId, CappedLogger>();

		 public UnknownAddressMonitor( Log log, Clock clock, long timeLimitMs )
		 {
			  this._log = log;
			  this._clock = clock;
			  this._timeLimitMs = timeLimitMs;
		 }

		 public virtual void LogAttemptToSendToMemberWithNoKnownAddress( MemberId to )
		 {
			  CappedLogger cappedLogger = _loggers[to];
			  if ( cappedLogger == null )
			  {
					cappedLogger = new CappedLogger( _log );
					cappedLogger.SetTimeLimit( _timeLimitMs, MILLISECONDS, _clock );
					_loggers[to] = cappedLogger;
			  }
			  cappedLogger.Info( string.Format( "No address found for {0}, probably because the member has been shut down.", to ) );
		 }
	}

}