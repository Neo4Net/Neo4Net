using System;

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
namespace Neo4Net.causalclustering.core.consensus
{

	using Timeouts = Neo4Net.causalclustering.core.consensus.RaftMachine.Timeouts;
	using TimeoutHandler = Neo4Net.causalclustering.core.consensus.schedule.TimeoutHandler;
	using Timer = Neo4Net.causalclustering.core.consensus.schedule.Timer;
	using TimerService = Neo4Net.causalclustering.core.consensus.schedule.TimerService;
	using Neo4Net.Functions;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using Group = Neo4Net.Scheduler.Group;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.schedule.TimeoutFactory.fixedTimeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.schedule.TimeoutFactory.uniformRandomTimeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.consensus.schedule.Timer.CancelMode.ASYNC;

	internal class LeaderAvailabilityTimers
	{
		 private readonly long _electionTimeout;
		 private readonly long _heartbeatInterval;
		 private readonly Clock _clock;
		 private readonly TimerService _timerService;
		 private readonly Log _log;

		 private volatile long _lastElectionRenewalMillis;

		 private Timer _heartbeatTimer;
		 private Timer _electionTimer;

		 internal LeaderAvailabilityTimers( Duration electionTimeout, Duration heartbeatInterval, Clock clock, TimerService timerService, LogProvider logProvider )
		 {
			  this._electionTimeout = electionTimeout.toMillis();
			  this._heartbeatInterval = heartbeatInterval.toMillis();
			  this._clock = clock;
			  this._timerService = timerService;
			  this._log = logProvider.getLog( this.GetType() );

			  if ( this._electionTimeout < this._heartbeatInterval )
			  {
					throw new System.ArgumentException( string.Format( "Election timeout {0} should not be shorter than heartbeat interval {1}", this._electionTimeout, this._heartbeatInterval ) );
			  }
		 }

		 internal virtual void Start( ThrowingConsumer<Clock, Exception> electionAction, ThrowingConsumer<Clock, Exception> heartbeatAction )
		 {
			 lock ( this )
			 {
				  this._electionTimer = _timerService.create( Timeouts.ELECTION, Group.RAFT_TIMER, Renewing( electionAction ) );
				  this._electionTimer.set( uniformRandomTimeout( _electionTimeout, _electionTimeout * 2, MILLISECONDS ) );
      
				  this._heartbeatTimer = _timerService.create( Timeouts.HEARTBEAT, Group.RAFT_TIMER, Renewing( heartbeatAction ) );
				  this._heartbeatTimer.set( fixedTimeout( _heartbeatInterval, MILLISECONDS ) );
      
				  _lastElectionRenewalMillis = _clock.millis();
			 }
		 }

		 internal virtual void Stop()
		 {
			 lock ( this )
			 {
				  if ( _electionTimer != null )
				  {
						_electionTimer.cancel( ASYNC );
				  }
				  if ( _heartbeatTimer != null )
				  {
						_heartbeatTimer.cancel( ASYNC );
				  }
			 }
		 }

		 internal virtual void RenewElection()
		 {
			 lock ( this )
			 {
				  _lastElectionRenewalMillis = _clock.millis();
				  if ( _electionTimer != null )
				  {
						_electionTimer.reset();
				  }
			 }
		 }

		 internal virtual bool ElectionTimedOut
		 {
			 get
			 {
				 lock ( this )
				 {
					  return _clock.millis() - _lastElectionRenewalMillis >= _electionTimeout;
				 }
			 }
		 }

		 // Getters for immutable values
		 internal virtual long ElectionTimeout
		 {
			 get
			 {
				  return _electionTimeout;
			 }
		 }

		 private TimeoutHandler Renewing( ThrowingConsumer<Clock, Exception> action )
		 {
			  return timeout =>
			  {
				try
				{
					 action.Accept( _clock );
				}
				catch ( Exception e )
				{
					 _log.error( "Failed to process timeout.", e );
				}
				timeout.reset();
			  };
		 }
	}

}