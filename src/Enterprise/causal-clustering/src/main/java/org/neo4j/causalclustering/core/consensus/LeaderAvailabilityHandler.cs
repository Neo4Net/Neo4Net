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
namespace Neo4Net.causalclustering.core.consensus
{

	using ClusterId = Neo4Net.causalclustering.identity.ClusterId;
	using ComposableMessageHandler = Neo4Net.causalclustering.messaging.ComposableMessageHandler;
	using Neo4Net.causalclustering.messaging;

	public class LeaderAvailabilityHandler : LifecycleMessageHandler<RaftMessages_ReceivedInstantClusterIdAwareMessage<JavaToDotNetGenericWildcard>>
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.neo4j.causalclustering.messaging.LifecycleMessageHandler<RaftMessages_ReceivedInstantClusterIdAwareMessage<?>> delegateHandler;
		 private readonly LifecycleMessageHandler<RaftMessages_ReceivedInstantClusterIdAwareMessage<object>> _delegateHandler;
		 private readonly LeaderAvailabilityTimers _leaderAvailabilityTimers;
		 private readonly ShouldRenewElectionTimeout _shouldRenewElectionTimeout;
		 private readonly RaftMessageTimerResetMonitor _raftMessageTimerResetMonitor;

		 public LeaderAvailabilityHandler<T1>( LifecycleMessageHandler<T1> delegateHandler, LeaderAvailabilityTimers leaderAvailabilityTimers, RaftMessageTimerResetMonitor raftMessageTimerResetMonitor, System.Func<long> term )
		 {
			  this._delegateHandler = delegateHandler;
			  this._leaderAvailabilityTimers = leaderAvailabilityTimers;
			  this._shouldRenewElectionTimeout = new ShouldRenewElectionTimeout( term );
			  this._raftMessageTimerResetMonitor = raftMessageTimerResetMonitor;
		 }

		 public static ComposableMessageHandler Composable( LeaderAvailabilityTimers leaderAvailabilityTimers, RaftMessageTimerResetMonitor raftMessageTimerResetMonitor, System.Func<long> term )
		 {
			  return @delegate => new LeaderAvailabilityHandler( @delegate, leaderAvailabilityTimers, raftMessageTimerResetMonitor, term );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void start(org.neo4j.causalclustering.identity.ClusterId clusterId) throws Throwable
		 public override void Start( ClusterId clusterId )
		 {
			 lock ( this )
			 {
				  _delegateHandler.start( clusterId );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void stop() throws Throwable
		 public override void Stop()
		 {
			 lock ( this )
			 {
				  _delegateHandler.stop();
			 }
		 }

		 public override void Handle<T1>( RaftMessages_ReceivedInstantClusterIdAwareMessage<T1> message )
		 {
			  HandleTimeouts( message );
			  _delegateHandler.handle( message );
		 }

		 private void HandleTimeouts<T1>( RaftMessages_ReceivedInstantClusterIdAwareMessage<T1> message )
		 {
			  if ( message.dispatch( _shouldRenewElectionTimeout ) )
			  {
					_raftMessageTimerResetMonitor.timerReset();
					_leaderAvailabilityTimers.renewElection();
			  }
		 }

		 private class ShouldRenewElectionTimeout : RaftMessages_Handler<bool, Exception>
		 {
			  internal readonly System.Func<long> Term;

			  internal ShouldRenewElectionTimeout( System.Func<long> term )
			  {
					this.Term = term;
			  }

			  public override bool? Handle( RaftMessages_AppendEntries_Request request )
			  {
					return request.LeaderTerm() >= Term.AsLong;
			  }

			  public override bool? Handle( RaftMessages_Heartbeat heartbeat )
			  {
					return heartbeat.LeaderTerm() >= Term.AsLong;
			  }

			  public override bool? Handle( RaftMessages_Vote_Request request )
			  {
					return false;
			  }

			  public override bool? Handle( RaftMessages_Vote_Response response )
			  {
					return false;
			  }

			  public override bool? Handle( RaftMessages_PreVote_Request request )
			  {
					return false;
			  }

			  public override bool? Handle( RaftMessages_PreVote_Response response )
			  {
					return false;
			  }

			  public override bool? Handle( RaftMessages_AppendEntries_Response response )
			  {
					return false;
			  }

			  public override bool? Handle( RaftMessages_LogCompactionInfo logCompactionInfo )
			  {
					return false;
			  }

			  public override bool? Handle( RaftMessages_HeartbeatResponse heartbeatResponse )
			  {
					return false;
			  }

			  public override bool? Handle( RaftMessages_Timeout_Election election )
			  {
					return false;
			  }

			  public override bool? Handle( RaftMessages_Timeout_Heartbeat heartbeat )
			  {
					return false;
			  }

			  public override bool? Handle( RaftMessages_NewEntry_Request request )
			  {
					return false;
			  }

			  public override bool? Handle( RaftMessages_NewEntry_BatchRequest batchRequest )
			  {
					return false;
			  }

			  public override bool? Handle( RaftMessages_PruneRequest pruneRequest )
			  {
					return false;
			  }
		 }
	}

}