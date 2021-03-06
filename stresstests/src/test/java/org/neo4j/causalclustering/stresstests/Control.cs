﻿using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.stresstests
{

	using DumpUtils = Org.Neo4j.Diagnostics.utils.DumpUtils;
	using Log = Org.Neo4j.Logging.Log;
	using Futures = Org.Neo4j.Util.concurrent.Futures;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.function.Suppliers.untilTimeExpired;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Exceptions.findCauseOrSuppressed;

	public class Control
	{
		 private readonly AtomicBoolean _stopTheWorld = new AtomicBoolean();
		 private readonly System.Func<bool> _keepGoing;
		 private readonly Log _log;
		 private readonly long _totalDurationMinutes;
		 private Exception _failure;

		 public Control( Config config )
		 {
			  this._log = config.LogProvider().getLog(this.GetType());
			  long workDurationMinutes = config.WorkDurationMinutes();
			  this._totalDurationMinutes = workDurationMinutes + config.ShutdownDurationMinutes();

			  System.Func<bool> notExpired = untilTimeExpired( workDurationMinutes, MINUTES );
			  this._keepGoing = () => !_stopTheWorld.get() && notExpired();
		 }

		 public virtual bool KeepGoing()
		 {
			  return _keepGoing.AsBoolean;
		 }

		 public virtual void OnFailure( Exception cause )
		 {
			 lock ( this )
			 {
				  if ( !KeepGoing() && findCauseOrSuppressed(cause, t => t is InterruptedException).Present )
				  {
						_log.info( "Ignoring interrupt at end of test", cause );
						return;
				  }
      
				  if ( _failure == null )
				  {
						_failure = cause;
				  }
				  else
				  {
						_failure.addSuppressed( cause );
				  }
				  _log.error( "Failure occurred", cause );
				  _log.error( "Thread dump always printed on failure" );
				  ThreadDump();
				  _stopTheWorld.set( true );
			 }
		 }

		 public virtual void AssertNoFailure()
		 {
			 lock ( this )
			 {
				  if ( _failure != null )
				  {
						throw new Exception( "Test failed", _failure );
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void awaitEnd(Iterable<java.util.concurrent.Future<?>> completions) throws InterruptedException, java.util.concurrent.TimeoutException, java.util.concurrent.ExecutionException
		 public virtual void AwaitEnd<T1>( IEnumerable<T1> completions )
		 {
			  Futures.combine( completions ).get( _totalDurationMinutes, MINUTES );
		 }

		 private void ThreadDump()
		 {
			  _log.info( DumpUtils.threadDump() );
		 }
	}

}