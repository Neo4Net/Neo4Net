using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.core.consensus.shipping
{

	using ReadableRaftLog = Neo4Net.causalclustering.core.consensus.log.ReadableRaftLog;
	using InFlightCache = Neo4Net.causalclustering.core.consensus.log.cache.InFlightCache;
	using RaftMembership = Neo4Net.causalclustering.core.consensus.membership.RaftMembership;
	using ShipCommand = Neo4Net.causalclustering.core.consensus.outcome.ShipCommand;
	using TimerService = Neo4Net.causalclustering.core.consensus.schedule.TimerService;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Neo4Net.causalclustering.messaging;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using LogProvider = Neo4Net.Logging.LogProvider;

	public class RaftLogShippingManager : LifecycleAdapter, Neo4Net.causalclustering.core.consensus.membership.RaftMembership_Listener
	{
		 private readonly Outbound<MemberId, Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> _outbound;
		 private readonly LogProvider _logProvider;
		 private readonly ReadableRaftLog _raftLog;
		 private readonly Clock _clock;
		 private readonly MemberId _myself;

		 private readonly RaftMembership _membership;
		 private readonly long _retryTimeMillis;
		 private readonly int _catchupBatchSize;
		 private readonly int _maxAllowedShippingLag;
		 private readonly InFlightCache _inFlightCache;

		 private IDictionary<MemberId, RaftLogShipper> _logShippers = new Dictionary<MemberId, RaftLogShipper>();
		 private LeaderContext _lastLeaderContext;

		 private bool _running;
		 private bool _stopped;
		 private TimerService _timerService;

		 public RaftLogShippingManager( Outbound<MemberId, Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> outbound, LogProvider logProvider, ReadableRaftLog raftLog, TimerService timerService, Clock clock, MemberId myself, RaftMembership membership, long retryTimeMillis, int catchupBatchSize, int maxAllowedShippingLag, InFlightCache inFlightCache )
		 {
			  this._outbound = outbound;
			  this._logProvider = logProvider;
			  this._raftLog = raftLog;
			  this._timerService = timerService;
			  this._clock = clock;
			  this._myself = myself;
			  this._membership = membership;
			  this._retryTimeMillis = retryTimeMillis;
			  this._catchupBatchSize = catchupBatchSize;
			  this._maxAllowedShippingLag = maxAllowedShippingLag;
			  this._inFlightCache = inFlightCache;
			  membership.RegisterListener( this );
		 }

		 /// <summary>
		 /// Paused when stepping down from leader role.
		 /// </summary>
		 public virtual void Pause()
		 {
			 lock ( this )
			 {
				  _running = false;
      
				  _logShippers.Values.forEach( RaftLogShipper.stop );
				  _logShippers.Clear();
			 }
		 }

		 /// <summary>
		 /// Resumed when becoming leader.
		 /// </summary>
		 public virtual void Resume( LeaderContext initialLeaderContext )
		 {
			 lock ( this )
			 {
				  if ( _stopped )
				  {
						return;
				  }
      
				  _running = true;
      
				  foreach ( MemberId member in _membership.replicationMembers() )
				  {
						EnsureLogShipperRunning( member, initialLeaderContext );
				  }
      
				  _lastLeaderContext = initialLeaderContext;
			 }
		 }

		 public override void Stop()
		 {
			 lock ( this )
			 {
				  Pause();
				  _stopped = true;
			 }
		 }

		 private void EnsureLogShipperRunning( MemberId member, LeaderContext leaderContext )
		 {
			  RaftLogShipper logShipper = _logShippers[member];
			  if ( logShipper == null && !member.Equals( _myself ) )
			  {
					logShipper = new RaftLogShipper( _outbound, _logProvider, _raftLog, _clock, _timerService, _myself, member, leaderContext.Term, leaderContext.CommitIndex, _retryTimeMillis, _catchupBatchSize, _maxAllowedShippingLag, _inFlightCache );

					_logShippers[member] = logShipper;

					logShipper.Start();
			  }
		 }

		 public virtual void HandleCommands( IEnumerable<ShipCommand> shipCommands, LeaderContext leaderContext )
		 {
			 lock ( this )
			 {
				  foreach ( ShipCommand shipCommand in shipCommands )
				  {
						foreach ( RaftLogShipper logShipper in _logShippers.Values )
						{
							 shipCommand.ApplyTo( logShipper, leaderContext );
						}
				  }
      
				  _lastLeaderContext = leaderContext;
			 }
		 }

		 public override void OnMembershipChanged()
		 {
			 lock ( this )
			 {
				  if ( _lastLeaderContext == null || !_running )
				  {
						return;
				  }
      
				  IDictionary<MemberId, RaftLogShipper>.KeyCollection toBeRemoved = new HashSet<MemberId>( _logShippers.Keys );
				  toBeRemoved.removeAll( _membership.replicationMembers() );
      
				  foreach ( MemberId member in toBeRemoved )
				  {
						RaftLogShipper logShipper = _logShippers.Remove( member );
						if ( logShipper != null )
						{
							 logShipper.Stop();
						}
				  }
      
				  foreach ( MemberId replicationMember in _membership.replicationMembers() )
				  {
						EnsureLogShipperRunning( replicationMember, _lastLeaderContext );
				  }
			 }
		 }

		 public override string ToString()
		 {
			  return format( "RaftLogShippingManager{logShippers=%s, myself=%s}", _logShippers, _myself );
		 }
	}

}