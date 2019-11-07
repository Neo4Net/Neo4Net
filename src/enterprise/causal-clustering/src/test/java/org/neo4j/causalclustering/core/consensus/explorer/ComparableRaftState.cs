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
namespace Neo4Net.causalclustering.core.consensus.explorer
{

	using ConsecutiveInFlightCache = Neo4Net.causalclustering.core.consensus.log.cache.ConsecutiveInFlightCache;
	using InFlightCache = Neo4Net.causalclustering.core.consensus.log.cache.InFlightCache;
	using RaftLog = Neo4Net.causalclustering.core.consensus.log.RaftLog;
	using ReadableRaftLog = Neo4Net.causalclustering.core.consensus.log.ReadableRaftLog;
	using Outcome = Neo4Net.causalclustering.core.consensus.outcome.Outcome;
	using RaftLogCommand = Neo4Net.causalclustering.core.consensus.outcome.RaftLogCommand;
	using Neo4Net.causalclustering.core.consensus.roles.follower;
	using ReadableRaftState = Neo4Net.causalclustering.core.consensus.state.ReadableRaftState;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

	public class ComparableRaftState : ReadableRaftState
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly MemberId MyselfConflict;
		 private readonly ISet<MemberId> _votingMembers;
		 private readonly ISet<MemberId> _replicationMembers;
		 private readonly Log _log;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal long TermConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal MemberId LeaderConflict;
		 private LeaderInfo _leaderInfo = LeaderInfo.INITIAL;
		 private long _leaderCommit = -1;
		 private MemberId _votedFor;
		 private ISet<MemberId> _votesForMe = new HashSet<MemberId>();
		 private ISet<MemberId> _preVotesForMe = new HashSet<MemberId>();
		 private ISet<MemberId> _heartbeatResponses = new HashSet<MemberId>();
		 private long _lastLogIndexBeforeWeBecameLeader = -1;
		 private FollowerStates<MemberId> _followerStates = new FollowerStates<MemberId>();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly RaftLog EntryLogConflict;
		 private readonly InFlightCache _inFlightCache;
		 private long _commitIndex = -1;
		 private bool _isPreElection;
		 private readonly bool _refusesToBeLeader;

		 internal ComparableRaftState( MemberId myself, ISet<MemberId> votingMembers, ISet<MemberId> replicationMembers, bool refusesToBeLeader, RaftLog entryLog, InFlightCache inFlightCache, LogProvider logProvider )
		 {
			  this.MyselfConflict = myself;
			  this._votingMembers = votingMembers;
			  this._replicationMembers = replicationMembers;
			  this.EntryLogConflict = entryLog;
			  this._inFlightCache = inFlightCache;
			  this._log = logProvider.getLog( this.GetType() );
			  this._refusesToBeLeader = refusesToBeLeader;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ComparableRaftState(Neo4Net.causalclustering.core.consensus.state.ReadableRaftState original) throws java.io.IOException
		 internal ComparableRaftState( ReadableRaftState original ) : this( original.Myself(), original.VotingMembers(), original.ReplicationMembers(), original.RefusesToBeLeader(), new ComparableRaftLog(original.EntryLog()), new ConsecutiveInFlightCache(), NullLogProvider.Instance )
		 {
		 }

		 public override MemberId Myself()
		 {
			  return MyselfConflict;
		 }

		 public override ISet<MemberId> VotingMembers()
		 {
			  return _votingMembers;
		 }

		 public override ISet<MemberId> ReplicationMembers()
		 {
			  return _replicationMembers;
		 }

		 public override long Term()
		 {
			  return TermConflict;
		 }

		 public override MemberId Leader()
		 {
			  return LeaderConflict;
		 }

		 public override LeaderInfo LeaderInfo()
		 {
			  return _leaderInfo;
		 }

		 public override long LeaderCommit()
		 {
			  return 0;
		 }

		 public override MemberId VotedFor()
		 {
			  return _votedFor;
		 }

		 public override ISet<MemberId> VotesForMe()
		 {
			  return _votesForMe;
		 }

		 public override ISet<MemberId> HeartbeatResponses()
		 {
			  return _heartbeatResponses;
		 }

		 public override long LastLogIndexBeforeWeBecameLeader()
		 {
			  return _lastLogIndexBeforeWeBecameLeader;
		 }

		 public override FollowerStates<MemberId> FollowerStates()
		 {
			  return _followerStates;
		 }

		 public override ReadableRaftLog EntryLog()
		 {
			  return EntryLogConflict;
		 }

		 public override long CommitIndex()
		 {
			  return _commitIndex;
		 }

		 public override bool SupportPreVoting()
		 {
			  return false;
		 }

		 public virtual bool PreElection
		 {
			 get
			 {
				  return _isPreElection;
			 }
		 }

		 public override ISet<MemberId> PreVotesForMe()
		 {
			  return _preVotesForMe;
		 }

		 public override bool RefusesToBeLeader()
		 {
			  return _refusesToBeLeader;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void update(Neo4Net.causalclustering.core.consensus.outcome.Outcome outcome) throws java.io.IOException
		 public virtual void Update( Outcome outcome )
		 {
			  TermConflict = outcome.Term;
			  _votedFor = outcome.VotedFor;
			  LeaderConflict = outcome.Leader;
			  _votesForMe = outcome.VotesForMe;
			  _lastLogIndexBeforeWeBecameLeader = outcome.LastLogIndexBeforeWeBecameLeader;
			  _followerStates = outcome.FollowerStates;
			  _isPreElection = outcome.PreElection;

			  foreach ( RaftLogCommand logCommand in outcome.LogCommands )
			  {
					logCommand.ApplyTo( EntryLogConflict, _log );
					logCommand.ApplyTo( _inFlightCache, _log );
			  }

			  _commitIndex = outcome.CommitIndex;
		 }

		 public override string ToString()
		 {
			  return format( "state{myself=%s, term=%s, leader=%s, leaderCommit=%d, appended=%d, committed=%d, " + "votedFor=%s, votesForMe=%s, lastLogIndexBeforeWeBecameLeader=%d, followerStates=%s}", MyselfConflict, TermConflict, LeaderConflict, _leaderCommit, EntryLogConflict.appendIndex(), _commitIndex, _votedFor, _votesForMe, _lastLogIndexBeforeWeBecameLeader, _followerStates );
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }
			  ComparableRaftState that = ( ComparableRaftState ) o;
			  return Objects.Equals( TermConflict, that.TermConflict ) && Objects.Equals( _lastLogIndexBeforeWeBecameLeader, that._lastLogIndexBeforeWeBecameLeader ) && Objects.Equals( MyselfConflict, that.MyselfConflict ) && Objects.Equals( _votingMembers, that._votingMembers ) && Objects.Equals( LeaderConflict, that.LeaderConflict ) && Objects.Equals( _leaderCommit, that._leaderCommit ) && Objects.Equals( EntryLogConflict, that.EntryLogConflict ) && Objects.Equals( _votedFor, that._votedFor ) && Objects.Equals( _votesForMe, that._votesForMe ) && Objects.Equals( _followerStates, that._followerStates );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( MyselfConflict, _votingMembers, TermConflict, LeaderConflict, EntryLogConflict, _votedFor, _votesForMe, _lastLogIndexBeforeWeBecameLeader, _followerStates );
		 }
	}

}