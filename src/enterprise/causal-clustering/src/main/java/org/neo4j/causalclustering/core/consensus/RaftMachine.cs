using System;
using System.Collections.Generic;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.core.consensus
{

	using RaftLog = Neo4Net.causalclustering.core.consensus.log.RaftLog;
	using InFlightCache = Neo4Net.causalclustering.core.consensus.log.cache.InFlightCache;
	using RaftMembershipManager = Neo4Net.causalclustering.core.consensus.membership.RaftMembershipManager;
	using ConsensusOutcome = Neo4Net.causalclustering.core.consensus.outcome.ConsensusOutcome;
	using Outcome = Neo4Net.causalclustering.core.consensus.outcome.Outcome;
	using Role = Neo4Net.causalclustering.core.consensus.roles.Role;
	using TimerService = Neo4Net.causalclustering.core.consensus.schedule.TimerService;
	using RaftLogShippingManager = Neo4Net.causalclustering.core.consensus.shipping.RaftLogShippingManager;
	using ExposedRaftState = Neo4Net.causalclustering.core.consensus.state.ExposedRaftState;
	using RaftState = Neo4Net.causalclustering.core.consensus.state.RaftState;
	using TermState = Neo4Net.causalclustering.core.consensus.term.TermState;
	using VoteState = Neo4Net.causalclustering.core.consensus.vote.VoteState;
	using RaftCoreState = Neo4Net.causalclustering.core.state.snapshot.RaftCoreState;
	using Neo4Net.causalclustering.core.state.storage;
	using Neo4Net.causalclustering.helper;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Neo4Net.causalclustering.messaging;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.consensus.roles.Role.LEADER;

	/// <summary>
	/// Implements the Raft Consensus Algorithm.
	/// <para>
	/// The algorithm is driven by incoming messages provided to <seealso cref="handle"/>.
	/// </para>
	/// </summary>
	public class RaftMachine : LeaderLocator, CoreMetaData
	{
		 private readonly RaftMessageTimerResetMonitor _raftMessageTimerResetMonitor;
		 private InFlightCache _inFlightCache;

		 public enum Timeouts
		 {
			  Election,
			  Heartbeat
		 }

		 private readonly RaftState _state;
		 private readonly MemberId _myself;

		 private readonly LeaderAvailabilityTimers _leaderAvailabilityTimers;
		 private RaftMembershipManager _membershipManager;

		 private readonly VolatileFuture<MemberId> _volatileLeader = new VolatileFuture<MemberId>( null );

		 private readonly Outbound<MemberId, RaftMessages_RaftMessage> _outbound;
		 private readonly Log _log;
		 private volatile Role _currentRole = Role.FOLLOWER;

		 private RaftLogShippingManager _logShipping;

		 public RaftMachine( MemberId myself, StateStorage<TermState> termStorage, StateStorage<VoteState> voteStorage, RaftLog entryLog, LeaderAvailabilityTimers leaderAvailabilityTimers, Outbound<MemberId, RaftMessages_RaftMessage> outbound, LogProvider logProvider, RaftMembershipManager membershipManager, RaftLogShippingManager logShipping, InFlightCache inFlightCache, bool refuseToBecomeLeader, bool supportPreVoting, Monitors monitors )
		 {
			  this._myself = myself;
			  this._leaderAvailabilityTimers = leaderAvailabilityTimers;

			  this._outbound = outbound;
			  this._logShipping = logShipping;
			  this._log = logProvider.getLog( this.GetType() );

			  this._membershipManager = membershipManager;

			  this._inFlightCache = inFlightCache;
			  this._state = new RaftState( myself, termStorage, membershipManager, entryLog, voteStorage, inFlightCache, logProvider, supportPreVoting, refuseToBecomeLeader );

			  _raftMessageTimerResetMonitor = monitors.NewMonitor( typeof( RaftMessageTimerResetMonitor ) );
		 }

		 /// <summary>
		 /// This should be called after the major recovery operations are complete. Before this is called
		 /// this instance cannot become a leader (the timers are disabled) and entries will not be cached
		 /// in the in-flight map, because the application process is not running and ready to consume them.
		 /// </summary>
		 public virtual void PostRecoveryActions()
		 {
			 lock ( this )
			 {
				  _leaderAvailabilityTimers.start( this.electionTimeout, clock => Handle( RaftMessages_ReceivedInstantAwareMessage.of( clock.instant(), new RaftMessages_Timeout_Heartbeat(_myself) ) ) );
      
				  _inFlightCache.enable();
			 }
		 }

		 public virtual void StopTimers()
		 {
			 lock ( this )
			 {
				  _leaderAvailabilityTimers.stop();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private synchronized void electionTimeout(java.time.Clock clock) throws java.io.IOException
		 private void ElectionTimeout( Clock clock )
		 {
			 lock ( this )
			 {
				  if ( _leaderAvailabilityTimers.ElectionTimedOut )
				  {
						TriggerElection( clock );
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void triggerElection(java.time.Clock clock) throws java.io.IOException
		 public virtual void TriggerElection( Clock clock )
		 {
					Handle( RaftMessages_ReceivedInstantAwareMessage.of( clock.instant(), new RaftMessages_Timeout_Election(_myself) ) );
		 }

		 public virtual void Panic()
		 {
			  StopTimers();
		 }

		 public virtual RaftCoreState CoreState()
		 {
			 lock ( this )
			 {
				  return new RaftCoreState( _membershipManager.Committed );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void installCoreState(org.Neo4Net.causalclustering.core.state.snapshot.RaftCoreState coreState) throws java.io.IOException
		 public virtual void InstallCoreState( RaftCoreState coreState )
		 {
			 lock ( this )
			 {
				  _membershipManager.install( coreState.Committed() );
			 }
		 }

		 public virtual ISet<MemberId> TargetMembershipSet
		 {
			 set
			 {
				 lock ( this )
				 {
					  _membershipManager.TargetMembershipSet = value;
         
					  if ( _currentRole == LEADER )
					  {
							_membershipManager.onFollowerStateChange( _state.followerStates() );
					  }
				 }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.causalclustering.identity.MemberId getLeader() throws NoLeaderFoundException
		 public virtual MemberId Leader
		 {
			 get
			 {
				  return WaitForLeader( 0, Objects.nonNull );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.causalclustering.identity.MemberId waitForLeader(long timeoutMillis, System.Predicate<org.Neo4Net.causalclustering.identity.MemberId> predicate) throws NoLeaderFoundException
		 private MemberId WaitForLeader( long timeoutMillis, System.Predicate<MemberId> predicate )
		 {
			  try
			  {
					return _volatileLeader.get( timeoutMillis, predicate );
			  }
			  catch ( InterruptedException e )
			  {
					Thread.CurrentThread.Interrupt();

					throw new NoLeaderFoundException( e );
			  }
			  catch ( TimeoutException e )
			  {
					throw new NoLeaderFoundException( e );
			  }
		 }

		 private ICollection<LeaderListener> _leaderListeners = new List<LeaderListener>();

		 public override void RegisterListener( LeaderListener listener )
		 {
			 lock ( this )
			 {
				  _leaderListeners.Add( listener );
				  listener.OnLeaderSwitch( _state.leaderInfo() );
			 }
		 }

		 public override void UnregisterListener( LeaderListener listener )
		 {
			 lock ( this )
			 {
				  _leaderListeners.remove( listener );
			 }
		 }

		 /// <summary>
		 /// Every call to state() gives you an immutable copy of the current state.
		 /// </summary>
		 /// <returns> A fresh view of the state. </returns>
		 public virtual ExposedRaftState State()
		 {
			 lock ( this )
			 {
				  return _state.copy();
			 }
		 }

		 private void NotifyLeaderChanges( Outcome outcome )
		 {
			  foreach ( LeaderListener listener in _leaderListeners )
			  {
					listener.OnLeaderEvent( outcome );
			  }
		 }

		 private void HandleLogShipping( Outcome outcome )
		 {
			  LeaderContext leaderContext = new LeaderContext( outcome.Term, outcome.LeaderCommit );
			  if ( outcome.ElectedLeader )
			  {
					_logShipping.resume( leaderContext );
			  }
			  else if ( outcome.SteppingDown )
			  {
					_logShipping.pause();
			  }

			  if ( outcome.Role == LEADER )
			  {
					_logShipping.handleCommands( outcome.ShipCommands, leaderContext );
			  }
		 }

		 private bool LeaderChanged( Outcome outcome, MemberId oldLeader )
		 {
			  return !Objects.Equals( oldLeader, outcome.Leader );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized org.Neo4Net.causalclustering.core.consensus.outcome.ConsensusOutcome handle(RaftMessages_RaftMessage incomingMessage) throws java.io.IOException
		 public virtual ConsensusOutcome Handle( RaftMessages_RaftMessage incomingMessage )
		 {
			 lock ( this )
			 {
				  Outcome outcome = _currentRole.handler.handle( incomingMessage, _state, _log );
      
				  bool newLeaderWasElected = LeaderChanged( outcome, _state.leader() );
      
				  _state.update( outcome ); // updates to raft log happen within
				  SendMessages( outcome );
      
				  HandleTimers( outcome );
				  HandleLogShipping( outcome );
      
				  DriveMembership( outcome );
      
				  _volatileLeader.set( outcome.Leader );
      
				  if ( newLeaderWasElected )
				  {
						NotifyLeaderChanges( outcome );
				  }
				  return outcome;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void driveMembership(org.Neo4Net.causalclustering.core.consensus.outcome.Outcome outcome) throws java.io.IOException
		 private void DriveMembership( Outcome outcome )
		 {
			  _membershipManager.processLog( outcome.CommitIndex, outcome.LogCommands );

			  _currentRole = outcome.Role;
			  _membershipManager.onRole( _currentRole );

			  if ( _currentRole == LEADER )
			  {
					_membershipManager.onFollowerStateChange( _state.followerStates() );
			  }
		 }

		 private void HandleTimers( Outcome outcome )
		 {
			  if ( outcome.ElectionTimeoutRenewed() )
			  {
					_raftMessageTimerResetMonitor.timerReset();
					_leaderAvailabilityTimers.renewElection();
			  }
		 }

		 private void SendMessages( Outcome outcome )
		 {
			  foreach ( RaftMessages_Directed outgoingMessage in outcome.OutgoingMessages )
			  {
					try
					{
						 _outbound.send( outgoingMessage.To(), outgoingMessage.Message() );
					}
					catch ( Exception e )
					{
						 _log.warn( format( "Failed to send message %s.", outgoingMessage ), e );
					}
			  }
		 }

		 public virtual bool Leader
		 {
			 get
			 {
				  return _currentRole == LEADER;
			 }
		 }

		 public virtual Role CurrentRole()
		 {
			  return _currentRole;
		 }

		 public virtual MemberId Identity()
		 {
			  return _myself;
		 }

		 public virtual RaftLogShippingManager LogShippingManager()
		 {
			  return _logShipping;
		 }

		 public override string ToString()
		 {
			  return format( "RaftInstance{role=%s, term=%d, currentMembers=%s}", _currentRole, Term(), VotingMembers() );
		 }

		 public virtual long Term()
		 {
			  return _state.term();
		 }

		 public virtual ISet<MemberId> VotingMembers()
		 {
			  return _membershipManager.votingMembers();
		 }

		 public virtual ISet<MemberId> ReplicationMembers()
		 {
			  return _membershipManager.replicationMembers();
		 }
	}

}