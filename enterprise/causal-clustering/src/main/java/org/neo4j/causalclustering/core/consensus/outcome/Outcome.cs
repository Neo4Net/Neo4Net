using System.Collections.Generic;
using System.Diagnostics;

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
namespace Org.Neo4j.causalclustering.core.consensus.outcome
{

	using Message = Org.Neo4j.causalclustering.messaging.Message;
	using Role = Org.Neo4j.causalclustering.core.consensus.roles.Role;
	using ReadableRaftState = Org.Neo4j.causalclustering.core.consensus.state.ReadableRaftState;
	using Org.Neo4j.causalclustering.core.consensus.roles.follower;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;

	/// <summary>
	/// Holds the outcome of a RAFT role's handling of a message. The role handling logic is stateless
	/// and responds to RAFT messages in the context of a supplied state. The outcome is later consumed
	/// to update the state and do operations embedded as commands within the outcome.
	/// 
	/// A state update could be to change role, change term, etc.
	/// A command could be to append to the RAFT log, tell the log shipper that there was a mismatch, etc.
	/// </summary>
	public class Outcome : Message, ConsensusOutcome
	{
		 /* Common */
		 private Role _nextRole;

		 private long _term;
		 private MemberId _leader;

		 private long _leaderCommit;

		 private ICollection<RaftLogCommand> _logCommands = new List<RaftLogCommand>();
		 private ICollection<Org.Neo4j.causalclustering.core.consensus.RaftMessages_Directed> _outgoingMessages = new List<Org.Neo4j.causalclustering.core.consensus.RaftMessages_Directed>();

		 private long _commitIndex;

		 /* Follower */
		 private MemberId _votedFor;
		 private bool _renewElectionTimeout;
		 private bool _needsFreshSnapshot;
		 private bool _isPreElection;
		 private ISet<MemberId> _preVotesForMe;

		 /* Candidate */
		 private ISet<MemberId> _votesForMe;
		 private long _lastLogIndexBeforeWeBecameLeader;

		 /* Leader */
		 private FollowerStates<MemberId> _followerStates;
		 private ICollection<ShipCommand> _shipCommands = new List<ShipCommand>();
		 private bool _electedLeader;
		 private long? _steppingDownInTerm;
		 private ISet<MemberId> _heartbeatResponses;

		 public Outcome( Role currentRole, ReadableRaftState ctx )
		 {
			  Defaults( currentRole, ctx );
		 }

		 public Outcome( Role nextRole, long term, MemberId leader, long leaderCommit, MemberId votedFor, ISet<MemberId> votesForMe, ISet<MemberId> preVotesForMe, long lastLogIndexBeforeWeBecameLeader, FollowerStates<MemberId> followerStates, bool renewElectionTimeout, ICollection<RaftLogCommand> logCommands, ICollection<Org.Neo4j.causalclustering.core.consensus.RaftMessages_Directed> outgoingMessages, ICollection<ShipCommand> shipCommands, long commitIndex, ISet<MemberId> heartbeatResponses, bool isPreElection )
		 {
			  this._nextRole = nextRole;
			  this._term = term;
			  this._leader = leader;
			  this._leaderCommit = leaderCommit;
			  this._votedFor = votedFor;
			  this._votesForMe = new HashSet<MemberId>( votesForMe );
			  this._preVotesForMe = new HashSet<MemberId>( preVotesForMe );
			  this._lastLogIndexBeforeWeBecameLeader = lastLogIndexBeforeWeBecameLeader;
			  this._followerStates = followerStates;
			  this._renewElectionTimeout = renewElectionTimeout;
			  this._heartbeatResponses = new HashSet<MemberId>( heartbeatResponses );

			  this._logCommands.addAll( logCommands );
			  this._outgoingMessages.addAll( outgoingMessages );
			  this._shipCommands.addAll( shipCommands );
			  this._commitIndex = commitIndex;
			  this._isPreElection = isPreElection;
			  this._steppingDownInTerm = long?.empty();
		 }

		 private void Defaults( Role currentRole, ReadableRaftState ctx )
		 {
			  _nextRole = currentRole;

			  _term = ctx.Term();
			  _leader = ctx.Leader();

			  _leaderCommit = ctx.LeaderCommit();

			  _votedFor = ctx.VotedFor();
			  _renewElectionTimeout = false;
			  _needsFreshSnapshot = false;

			  _isPreElection = ( currentRole == Role.FOLLOWER ) && ctx.PreElection;
			  _steppingDownInTerm = long?.empty();
			  _preVotesForMe = _isPreElection ? new HashSet<MemberId>( ctx.PreVotesForMe() ) : emptySet();
			  _votesForMe = ( currentRole == Role.CANDIDATE ) ? new HashSet<MemberId>( ctx.VotesForMe() ) : emptySet();
			  _heartbeatResponses = ( currentRole == Role.LEADER ) ? new HashSet<MemberId>( ctx.HeartbeatResponses() ) : emptySet();

			  _lastLogIndexBeforeWeBecameLeader = ( currentRole == Role.LEADER ) ? ctx.LastLogIndexBeforeWeBecameLeader() : -1;
			  _followerStates = ( currentRole == Role.LEADER ) ? ctx.FollowerStates() : new FollowerStates<MemberId>();

			  _commitIndex = ctx.CommitIndex();
		 }

		 public virtual Role NextRole
		 {
			 set
			 {
				  this._nextRole = value;
			 }
		 }

		 public virtual long NextTerm
		 {
			 set
			 {
				  this._term = value;
			 }
		 }

		 public virtual MemberId Leader
		 {
			 set
			 {
				  this._leader = value;
			 }
			 get
			 {
				  return _leader;
			 }
		 }

		 public virtual long LeaderCommit
		 {
			 set
			 {
				  this._leaderCommit = value;
			 }
			 get
			 {
				  return _leaderCommit;
			 }
		 }

		 public virtual void AddLogCommand( RaftLogCommand logCommand )
		 {
			  this._logCommands.Add( logCommand );
		 }

		 public virtual void AddOutgoingMessage( Org.Neo4j.causalclustering.core.consensus.RaftMessages_Directed message )
		 {
			  this._outgoingMessages.Add( message );
		 }

		 public virtual MemberId VotedFor
		 {
			 set
			 {
				  this._votedFor = value;
			 }
			 get
			 {
				  return _votedFor;
			 }
		 }

		 public virtual void RenewElectionTimeout()
		 {
			  this._renewElectionTimeout = true;
		 }

		 public virtual void MarkNeedForFreshSnapshot()
		 {
			  this._needsFreshSnapshot = true;
		 }

		 public virtual void AddVoteForMe( MemberId voteFrom )
		 {
			  this._votesForMe.Add( voteFrom );
		 }

		 public virtual long LastLogIndexBeforeWeBecameLeader
		 {
			 set
			 {
				  this._lastLogIndexBeforeWeBecameLeader = value;
			 }
			 get
			 {
				  return _lastLogIndexBeforeWeBecameLeader;
			 }
		 }

		 public virtual void ReplaceFollowerStates( FollowerStates<MemberId> followerStates )
		 {
			  this._followerStates = followerStates;
		 }

		 public virtual void AddShipCommand( ShipCommand shipCommand )
		 {
			  _shipCommands.Add( shipCommand );
		 }

		 public virtual void ElectedLeader()
		 {
			  Debug.Assert( !SteppingDown );
			  this._electedLeader = true;
		 }

		 public virtual void SteppingDown( long stepDownTerm )
		 {
			  Debug.Assert( !_electedLeader );
			  _steppingDownInTerm = long?.of( stepDownTerm );
		 }

		 public override string ToString()
		 {
			  return "Outcome{" +
						"nextRole=" + _nextRole +
						", term=" + _term +
						", leader=" + _leader +
						", leaderCommit=" + _leaderCommit +
						", logCommands=" + _logCommands +
						", outgoingMessages=" + _outgoingMessages +
						", commitIndex=" + _commitIndex +
						", votedFor=" + _votedFor +
						", renewElectionTimeout=" + _renewElectionTimeout +
						", needsFreshSnapshot=" + _needsFreshSnapshot +
						", votesForMe=" + _votesForMe +
						", preVotesForMe=" + _preVotesForMe +
						", lastLogIndexBeforeWeBecameLeader=" + _lastLogIndexBeforeWeBecameLeader +
						", followerStates=" + _followerStates +
						", shipCommands=" + _shipCommands +
						", electedLeader=" + _electedLeader +
						", steppingDownInTerm=" + _steppingDownInTerm +
						'}';
		 }

		 public virtual Role Role
		 {
			 get
			 {
				  return _nextRole;
			 }
		 }

		 public virtual long Term
		 {
			 get
			 {
				  return _term;
			 }
		 }



		 public virtual ICollection<RaftLogCommand> LogCommands
		 {
			 get
			 {
				  return _logCommands;
			 }
		 }

		 public virtual ICollection<Org.Neo4j.causalclustering.core.consensus.RaftMessages_Directed> OutgoingMessages
		 {
			 get
			 {
				  return _outgoingMessages;
			 }
		 }


		 public virtual bool ElectionTimeoutRenewed()
		 {
			  return _renewElectionTimeout;
		 }

		 public override bool NeedsFreshSnapshot()
		 {
			  return _needsFreshSnapshot;
		 }

		 public virtual ISet<MemberId> VotesForMe
		 {
			 get
			 {
				  return _votesForMe;
			 }
		 }


		 public virtual FollowerStates<MemberId> FollowerStates
		 {
			 get
			 {
				  return _followerStates;
			 }
		 }

		 public virtual ICollection<ShipCommand> ShipCommands
		 {
			 get
			 {
				  return _shipCommands;
			 }
		 }

		 public virtual bool ElectedLeader
		 {
			 get
			 {
				  return _electedLeader;
			 }
		 }

		 public virtual bool SteppingDown
		 {
			 get
			 {
				  return _steppingDownInTerm.HasValue;
			 }
		 }

		 public virtual long? StepDownTerm()
		 {
			  return _steppingDownInTerm;
		 }

		 public virtual long CommitIndex
		 {
			 get
			 {
				  return _commitIndex;
			 }
			 set
			 {
				  this._commitIndex = value;
			 }
		 }


		 public virtual void AddHeartbeatResponse( MemberId from )
		 {
			  this._heartbeatResponses.Add( from );
		 }

		 public virtual ISet<MemberId> HeartbeatResponses
		 {
			 get
			 {
				  return _heartbeatResponses;
			 }
		 }

		 public virtual bool PreElection
		 {
			 set
			 {
				  this._isPreElection = value;
			 }
			 get
			 {
				  return _isPreElection;
			 }
		 }


		 public virtual void AddPreVoteForMe( MemberId from )
		 {
			  this._preVotesForMe.Add( from );
		 }

		 public virtual ISet<MemberId> PreVotesForMe
		 {
			 get
			 {
				  return _preVotesForMe;
			 }
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
			  Outcome outcome = ( Outcome ) o;
			  return _term == outcome._term && _leaderCommit == outcome._leaderCommit && _commitIndex == outcome._commitIndex && _renewElectionTimeout == outcome._renewElectionTimeout && _needsFreshSnapshot == outcome._needsFreshSnapshot && _isPreElection == outcome._isPreElection && _lastLogIndexBeforeWeBecameLeader == outcome._lastLogIndexBeforeWeBecameLeader && _electedLeader == outcome._electedLeader && _nextRole == outcome._nextRole && Objects.Equals( _steppingDownInTerm, outcome._steppingDownInTerm ) && Objects.Equals( _leader, outcome._leader ) && Objects.Equals( _logCommands, outcome._logCommands ) && Objects.Equals( _outgoingMessages, outcome._outgoingMessages ) && Objects.Equals( _votedFor, outcome._votedFor ) && Objects.Equals( _preVotesForMe, outcome._preVotesForMe ) && Objects.Equals( _votesForMe, outcome._votesForMe ) && Objects.Equals( _followerStates, outcome._followerStates ) && Objects.Equals( _shipCommands, outcome._shipCommands ) && Objects.Equals( _heartbeatResponses, outcome._heartbeatResponses );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _nextRole, _term, _leader, _leaderCommit, _logCommands, _outgoingMessages, _commitIndex, _votedFor, _renewElectionTimeout, _needsFreshSnapshot, _isPreElection, _preVotesForMe, _votesForMe, _lastLogIndexBeforeWeBecameLeader, _followerStates, _shipCommands, _electedLeader, _steppingDownInTerm, _heartbeatResponses );
		 }
	}

}