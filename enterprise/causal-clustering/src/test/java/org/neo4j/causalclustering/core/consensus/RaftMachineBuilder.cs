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
namespace Org.Neo4j.causalclustering.core.consensus
{

	using ConsecutiveInFlightCache = Org.Neo4j.causalclustering.core.consensus.log.cache.ConsecutiveInFlightCache;
	using InFlightCache = Org.Neo4j.causalclustering.core.consensus.log.cache.InFlightCache;
	using InMemoryRaftLog = Org.Neo4j.causalclustering.core.consensus.log.InMemoryRaftLog;
	using RaftLog = Org.Neo4j.causalclustering.core.consensus.log.RaftLog;
	using Org.Neo4j.causalclustering.core.consensus.membership;
	using RaftMembershipManager = Org.Neo4j.causalclustering.core.consensus.membership.RaftMembershipManager;
	using RaftMembershipState = Org.Neo4j.causalclustering.core.consensus.membership.RaftMembershipState;
	using ConsensusOutcome = Org.Neo4j.causalclustering.core.consensus.outcome.ConsensusOutcome;
	using TimerService = Org.Neo4j.causalclustering.core.consensus.schedule.TimerService;
	using RaftLogShippingManager = Org.Neo4j.causalclustering.core.consensus.shipping.RaftLogShippingManager;
	using TermState = Org.Neo4j.causalclustering.core.consensus.term.TermState;
	using VoteState = Org.Neo4j.causalclustering.core.consensus.vote.VoteState;
	using SendToMyself = Org.Neo4j.causalclustering.core.replication.SendToMyself;
	using Org.Neo4j.causalclustering.core.state.storage;
	using Org.Neo4j.causalclustering.core.state.storage;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using Org.Neo4j.causalclustering.messaging;
	using Org.Neo4j.causalclustering.messaging;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using Clocks = Org.Neo4j.Time.Clocks;

	public class RaftMachineBuilder
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			_termStateStorage = new InMemoryStateStorage<TermState>( _termState );
			_term = _termState.currentTerm();
			_retryTimeMillis = _electionTimeout / 2;
		}

		 private readonly MemberId _member;

		 private int _expectedClusterSize;
		 private Org.Neo4j.causalclustering.core.consensus.membership.RaftGroup_Builder _memberSetBuilder;

		 private TermState _termState = new TermState();
		 private StateStorage<TermState> _termStateStorage;
		 private StateStorage<VoteState> _voteStateStorage = new InMemoryStateStorage<VoteState>( new VoteState() );
		 private RaftLog _raftLog = new InMemoryRaftLog();
		 private TimerService _timerService;

		 private Inbound<RaftMessages_RaftMessage> _inbound = handler =>
		 {
		 };
		 private Outbound<MemberId, RaftMessages_RaftMessage> _outbound = ( to, message, block ) =>
		 {
		 };

		 private LogProvider _logProvider = NullLogProvider.Instance;
		 private Clock _clock = Clocks.systemClock();

		 private long _term;

		 private long _electionTimeout = 500;
		 private long _heartbeatInterval = 150;

		 private long _catchupTimeout = 30000;
		 private long _retryTimeMillis;
		 private int _catchupBatchSize = 64;
		 private int _maxAllowedShippingLag = 256;
		 private StateStorage<RaftMembershipState> _raftMembership = new InMemoryStateStorage<RaftMembershipState>( new RaftMembershipState() );
		 private Monitors _monitors = new Monitors();
		 private CommitListener _commitListener = commitIndex =>
		 {
		 };
		 private InFlightCache _inFlightCache = new ConsecutiveInFlightCache();

		 public RaftMachineBuilder( MemberId member, int expectedClusterSize, Org.Neo4j.causalclustering.core.consensus.membership.RaftGroup_Builder memberSetBuilder )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  this._member = member;
			  this._expectedClusterSize = expectedClusterSize;
			  this._memberSetBuilder = memberSetBuilder;
		 }

		 public virtual RaftMachine Build()
		 {
			  _termState.update( _term );
			  LeaderAvailabilityTimers leaderAvailabilityTimers = new LeaderAvailabilityTimers( Duration.ofMillis( _electionTimeout ), Duration.ofMillis( _heartbeatInterval ), _clock, _timerService, _logProvider );
			  SendToMyself leaderOnlyReplicator = new SendToMyself( _member, _outbound );
			  RaftMembershipManager membershipManager = new RaftMembershipManager( leaderOnlyReplicator, _memberSetBuilder, _raftLog, _logProvider, _expectedClusterSize, leaderAvailabilityTimers.ElectionTimeout, _clock, _catchupTimeout, _raftMembership );
			  membershipManager.RecoverFromIndexSupplier = () => 0;
			  RaftLogShippingManager logShipping = new RaftLogShippingManager( _outbound, _logProvider, _raftLog, _timerService, _clock, _member, membershipManager, _retryTimeMillis, _catchupBatchSize, _maxAllowedShippingLag, _inFlightCache );
			  RaftMachine raft = new RaftMachine( _member, _termStateStorage, _voteStateStorage, _raftLog, leaderAvailabilityTimers, _outbound, _logProvider, membershipManager, logShipping, _inFlightCache, false, false, _monitors );
			  _inbound.registerHandler(incomingMessage =>
			  {
				try
				{
					 ConsensusOutcome outcome = raft.Handle( incomingMessage );
					 _commitListener.notifyCommitted( outcome.CommitIndex );
				}
				catch ( IOException e )
				{
					 throw new Exception( e );
				}
			  });

			  try
			  {
					membershipManager.Start();
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }

			  return raft;
		 }

		 public virtual RaftMachineBuilder ElectionTimeout( long electionTimeout )
		 {
			  this._electionTimeout = electionTimeout;
			  return this;
		 }

		 public virtual RaftMachineBuilder HeartbeatInterval( long heartbeatInterval )
		 {
			  this._heartbeatInterval = heartbeatInterval;
			  return this;
		 }

		 public virtual RaftMachineBuilder TimerService( TimerService timerService )
		 {
			  this._timerService = timerService;
			  return this;
		 }

		 public virtual RaftMachineBuilder Outbound( Outbound<MemberId, RaftMessages_RaftMessage> outbound )
		 {
			  this._outbound = outbound;
			  return this;
		 }

		 public virtual RaftMachineBuilder Inbound( Inbound<RaftMessages_RaftMessage> inbound )
		 {
			  this._inbound = inbound;
			  return this;
		 }

		 public virtual RaftMachineBuilder RaftLog( RaftLog raftLog )
		 {
			  this._raftLog = raftLog;
			  return this;
		 }

		 public virtual RaftMachineBuilder InFlightCache( InFlightCache inFlightCache )
		 {
			  this._inFlightCache = inFlightCache;
			  return this;
		 }

		 public virtual RaftMachineBuilder Clock( Clock clock )
		 {
			  this._clock = clock;
			  return this;
		 }

		 public virtual RaftMachineBuilder CommitListener( CommitListener commitListener )
		 {
			  this._commitListener = commitListener;
			  return this;
		 }

		 internal virtual RaftMachineBuilder Monitors( Monitors monitors )
		 {
			  this._monitors = monitors;
			  return this;
		 }

		 public virtual RaftMachineBuilder Term( long term )
		 {
			  this._term = term;
			  return this;
		 }

		 public interface CommitListener
		 {
			  /// <summary>
			  /// Called when the highest committed index increases.
			  /// </summary>
			  void NotifyCommitted( long commitIndex );
		 }
	}

}