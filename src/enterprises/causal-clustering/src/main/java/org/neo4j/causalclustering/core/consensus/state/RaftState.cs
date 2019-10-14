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
namespace Neo4Net.causalclustering.core.consensus.state
{

	using InFlightCache = Neo4Net.causalclustering.core.consensus.log.cache.InFlightCache;
	using RaftLog = Neo4Net.causalclustering.core.consensus.log.RaftLog;
	using ReadableRaftLog = Neo4Net.causalclustering.core.consensus.log.ReadableRaftLog;
	using RaftMembership = Neo4Net.causalclustering.core.consensus.membership.RaftMembership;
	using Outcome = Neo4Net.causalclustering.core.consensus.outcome.Outcome;
	using RaftLogCommand = Neo4Net.causalclustering.core.consensus.outcome.RaftLogCommand;
	using Neo4Net.causalclustering.core.consensus.roles.follower;
	using TermState = Neo4Net.causalclustering.core.consensus.term.TermState;
	using VoteState = Neo4Net.causalclustering.core.consensus.vote.VoteState;
	using Neo4Net.causalclustering.core.state.storage;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	public class RaftState : ReadableRaftState
	{
		 private readonly MemberId _myself;
		 private readonly StateStorage<TermState> _termStorage;
		 private readonly StateStorage<VoteState> _voteStorage;
		 private readonly RaftMembership _membership;
		 private readonly Log _log;
		 private readonly RaftLog _entryLog;
		 private readonly InFlightCache _inFlightCache;
		 private readonly bool _supportPreVoting;

		 private TermState _termState;
		 private VoteState _voteState;

		 private MemberId _leader;
		 private LeaderInfo _leaderInfo = LeaderInfo.INITIAL;
		 private ISet<MemberId> _votesForMe = new HashSet<MemberId>();
		 private ISet<MemberId> _preVotesForMe = new HashSet<MemberId>();
		 private ISet<MemberId> _heartbeatResponses = new HashSet<MemberId>();
		 private FollowerStates<MemberId> _followerStates = new FollowerStates<MemberId>();
		 private long _leaderCommit = -1;
		 private long _commitIndex = -1;
		 private long _lastLogIndexBeforeWeBecameLeader = -1;
		 private bool _isPreElection;
		 private readonly bool _refuseToBeLeader;

		 public RaftState( MemberId myself, StateStorage<TermState> termStorage, RaftMembership membership, RaftLog entryLog, StateStorage<VoteState> voteStorage, InFlightCache inFlightCache, LogProvider logProvider, bool supportPreVoting, bool refuseToBeLeader )
		 {
			  this._myself = myself;
			  this._termStorage = termStorage;
			  this._voteStorage = voteStorage;
			  this._membership = membership;
			  this._entryLog = entryLog;
			  this._inFlightCache = inFlightCache;
			  this._supportPreVoting = supportPreVoting;
			  this._log = logProvider.getLog( this.GetType() );

			  // Initial state
			  this._isPreElection = supportPreVoting;
			  this._refuseToBeLeader = refuseToBeLeader;
		 }

		 public override MemberId Myself()
		 {
			  return _myself;
		 }

		 public override ISet<MemberId> VotingMembers()
		 {
			  return _membership.votingMembers();
		 }

		 public override ISet<MemberId> ReplicationMembers()
		 {
			  return _membership.replicationMembers();
		 }

		 public override long Term()
		 {
			  return TermState().currentTerm();
		 }

		 private TermState TermState()
		 {
			  if ( _termState == null )
			  {
					_termState = _termStorage.InitialState;
			  }
			  return _termState;
		 }

		 public override MemberId Leader()
		 {
			  return _leader;
		 }

		 public override LeaderInfo LeaderInfo()
		 {
			  return _leaderInfo;
		 }

		 public override long LeaderCommit()
		 {
			  return _leaderCommit;
		 }

		 public override MemberId VotedFor()
		 {
			  return VoteState().votedFor();
		 }

		 private VoteState VoteState()
		 {
			  if ( _voteState == null )
			  {
					_voteState = _voteStorage.InitialState;
			  }
			  return _voteState;
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
			  return _entryLog;
		 }

		 public override long CommitIndex()
		 {
			  return _commitIndex;
		 }

		 public override bool SupportPreVoting()
		 {
			  return _supportPreVoting;
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
			  return _refuseToBeLeader;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void update(org.neo4j.causalclustering.core.consensus.outcome.Outcome outcome) throws java.io.IOException
		 public virtual void Update( Outcome outcome )
		 {
			  if ( TermState().update(outcome.Term) )
			  {
					_termStorage.persistStoreData( TermState() );
			  }
			  if ( VoteState().update(outcome.VotedFor, outcome.Term) )
			  {
					_voteStorage.persistStoreData( VoteState() );
			  }

			  LogIfLeaderChanged( outcome.Leader );
			  _leader = outcome.Leader;
			  _leaderInfo = new LeaderInfo( outcome.Leader, outcome.Term );

			  _leaderCommit = outcome.LeaderCommit;
			  _votesForMe = outcome.VotesForMe;
			  _preVotesForMe = outcome.PreVotesForMe;
			  _heartbeatResponses = outcome.HeartbeatResponses;
			  _lastLogIndexBeforeWeBecameLeader = outcome.LastLogIndexBeforeWeBecameLeader;
			  _followerStates = outcome.FollowerStates;
			  _isPreElection = outcome.PreElection;

			  foreach ( RaftLogCommand logCommand in outcome.LogCommands )
			  {
					logCommand.ApplyTo( _entryLog, _log );
					logCommand.ApplyTo( _inFlightCache, _log );
			  }
			  _commitIndex = outcome.CommitIndex;
		 }

		 private void LogIfLeaderChanged( MemberId leader )
		 {
			  if ( this._leader == null )
			  {
					if ( leader != null )
					{
						 _log.info( "First leader elected: %s", leader );
					}
					return;
			  }

			  if ( !this._leader.Equals( leader ) )
			  {
					_log.info( "Leader changed from %s to %s", this._leader, leader );
			  }
		 }

		 public virtual ExposedRaftState Copy()
		 {
			  return new ReadOnlyRaftState( this, LeaderCommit(), CommitIndex(), EntryLog().appendIndex(), LastLogIndexBeforeWeBecameLeader(), Term(), VotingMembers() );
		 }

		 private class ReadOnlyRaftState : ExposedRaftState
		 {
			 private readonly RaftState _outerInstance;


//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly long LeaderCommitConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly long CommitIndexConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly long AppendIndexConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly long LastLogIndexBeforeWeBecameLeaderConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly long TermConflict;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly ISet<MemberId> VotingMembersConflict; // returned set is never mutated

			  internal ReadOnlyRaftState( RaftState outerInstance, long leaderCommit, long commitIndex, long appendIndex, long lastLogIndexBeforeWeBecameLeader, long term, ISet<MemberId> votingMembers )
			  {
				  this._outerInstance = outerInstance;
					this.LeaderCommitConflict = leaderCommit;
					this.CommitIndexConflict = commitIndex;
					this.AppendIndexConflict = appendIndex;
					this.LastLogIndexBeforeWeBecameLeaderConflict = lastLogIndexBeforeWeBecameLeader;
					this.TermConflict = term;
					this.VotingMembersConflict = votingMembers;
			  }

			  public override long LastLogIndexBeforeWeBecameLeader()
			  {
					return LastLogIndexBeforeWeBecameLeaderConflict;
			  }

			  public override long LeaderCommit()
			  {
					return this.LeaderCommitConflict;
			  }

			  public override long CommitIndex()
			  {
					return this.CommitIndexConflict;
			  }

			  public override long AppendIndex()
			  {
					return this.AppendIndexConflict;
			  }

			  public override long Term()
			  {
					return this.TermConflict;
			  }

			  public override ISet<MemberId> VotingMembers()
			  {
					return this.VotingMembersConflict;
			  }
		 }
	}

}