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

	using ConsecutiveInFlightCache = Neo4Net.causalclustering.core.consensus.log.cache.ConsecutiveInFlightCache;
	using InFlightCache = Neo4Net.causalclustering.core.consensus.log.cache.InFlightCache;
	using InMemoryRaftLog = Neo4Net.causalclustering.core.consensus.log.InMemoryRaftLog;
	using RaftLog = Neo4Net.causalclustering.core.consensus.log.RaftLog;
	using Neo4Net.causalclustering.core.consensus.membership;
	using RaftMembershipManager = Neo4Net.causalclustering.core.consensus.membership.RaftMembershipManager;
	using RaftMembershipState = Neo4Net.causalclustering.core.consensus.membership.RaftMembershipState;
	using ConsensusOutcome = Neo4Net.causalclustering.core.consensus.outcome.ConsensusOutcome;
	using TimerService = Neo4Net.causalclustering.core.consensus.schedule.TimerService;
	using RaftLogShippingManager = Neo4Net.causalclustering.core.consensus.shipping.RaftLogShippingManager;
	using TermState = Neo4Net.causalclustering.core.consensus.term.TermState;
	using VoteState = Neo4Net.causalclustering.core.consensus.vote.VoteState;
	using SendToMyself = Neo4Net.causalclustering.core.replication.SendToMyself;
	using Neo4Net.causalclustering.core.state.storage;
	using Neo4Net.causalclustering.core.state.storage;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Neo4Net.causalclustering.messaging;
	using Neo4Net.causalclustering.messaging;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using Clocks = Neo4Net.Time.Clocks;

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
		 private Neo4Net.causalclustering.core.consensus.membership.RaftGroup_Builder _memberSetBuilder;

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

		 public RaftMachineBuilder( MemberId member, int expectedClusterSize, Neo4Net.causalclustering.core.consensus.membership.RaftGroup_Builder memberSetBuilder )
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