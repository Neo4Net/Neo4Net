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
namespace Neo4Net.causalclustering.core.consensus.state
{

	using ConsecutiveInFlightCache = Neo4Net.causalclustering.core.consensus.log.cache.ConsecutiveInFlightCache;
	using Neo4Net.causalclustering.core.state.storage;
	using Neo4Net.causalclustering.core.state.storage;
	using InMemoryRaftLog = Neo4Net.causalclustering.core.consensus.log.InMemoryRaftLog;
	using RaftLog = Neo4Net.causalclustering.core.consensus.log.RaftLog;
	using RaftMembership = Neo4Net.causalclustering.core.consensus.membership.RaftMembership;
	using RaftLogCommand = Neo4Net.causalclustering.core.consensus.outcome.RaftLogCommand;
	using Outcome = Neo4Net.causalclustering.core.consensus.outcome.Outcome;
	using Neo4Net.causalclustering.core.consensus.roles.follower;
	using TermState = Neo4Net.causalclustering.core.consensus.term.TermState;
	using VoteState = Neo4Net.causalclustering.core.consensus.vote.VoteState;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asSet;

	public class RaftStateBuilder
	{
		 public static RaftStateBuilder RaftState()
		 {
			  return new RaftStateBuilder();
		 }

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public MemberId MyselfConflict;
		 private ISet<MemberId> _votingMembers = emptySet();
		 private ISet<MemberId> _replicationMembers = emptySet();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public long TermConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public MemberId LeaderConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public long LeaderCommitConflict = -1;
		 private MemberId _votedFor;
		 private RaftLog _entryLog = new InMemoryRaftLog();
		 private bool _supportPreVoting;
		 private ISet<MemberId> _votesForMe = emptySet();
		 private ISet<MemberId> _preVotesForMe = emptySet();
		 private long _lastLogIndexBeforeWeBecameLeader = -1;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public long CommitIndexConflict = -1;
		 private FollowerStates<MemberId> _followerStates = new FollowerStates<MemberId>();
		 private bool _isPreElection;
		 private bool _refusesToBeLeader;

		 public virtual RaftStateBuilder Myself( MemberId myself )
		 {
			  this.MyselfConflict = myself;
			  return this;
		 }

		 public virtual RaftStateBuilder VotingMembers( ISet<MemberId> currentMembers )
		 {
			  this._votingMembers = currentMembers;
			  return this;
		 }

		 private RaftStateBuilder ReplicationMembers( ISet<MemberId> replicationMembers )
		 {
			  this._replicationMembers = replicationMembers;
			  return this;
		 }

		 public virtual RaftStateBuilder Term( long term )
		 {
			  this.TermConflict = term;
			  return this;
		 }

		 public virtual RaftStateBuilder Leader( MemberId leader )
		 {
			  this.LeaderConflict = leader;
			  return this;
		 }

		 public virtual RaftStateBuilder LeaderCommit( long leaderCommit )
		 {
			  this.LeaderCommitConflict = leaderCommit;
			  return this;
		 }

		 public virtual RaftStateBuilder VotedFor( MemberId votedFor )
		 {
			  this._votedFor = votedFor;
			  return this;
		 }

		 public virtual RaftStateBuilder EntryLog( RaftLog entryLog )
		 {
			  this._entryLog = entryLog;
			  return this;
		 }

		 public virtual RaftStateBuilder VotesForMe( ISet<MemberId> votesForMe )
		 {
			  this._votesForMe = votesForMe;
			  return this;
		 }

		 public virtual RaftStateBuilder SupportsPreVoting( bool supportPreVoting )
		 {
			  this._supportPreVoting = supportPreVoting;
			  return this;
		 }

		 public virtual RaftStateBuilder LastLogIndexBeforeWeBecameLeader( long lastLogIndexBeforeWeBecameLeader )
		 {
			  this._lastLogIndexBeforeWeBecameLeader = lastLogIndexBeforeWeBecameLeader;
			  return this;
		 }

		 public virtual RaftStateBuilder CommitIndex( long commitIndex )
		 {
			  this.CommitIndexConflict = commitIndex;
			  return this;
		 }

		 public virtual RaftStateBuilder setPreElection( bool isPreElection )
		 {
			  this._isPreElection = isPreElection;
			  return this;
		 }

		 public virtual RaftStateBuilder setRefusesToBeLeader( bool refusesToBeLeader )
		 {
			  this._refusesToBeLeader = refusesToBeLeader;
			  return this;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public RaftState build() throws java.io.IOException
		 public virtual RaftState Build()
		 {
			  StateStorage<TermState> termStore = new InMemoryStateStorage<TermState>( new TermState() );
			  StateStorage<VoteState> voteStore = new InMemoryStateStorage<VoteState>( new VoteState() );
			  StubMembership membership = new StubMembership( _votingMembers, _replicationMembers );

			  RaftState state = new RaftState( MyselfConflict, termStore, membership, _entryLog, voteStore, new ConsecutiveInFlightCache(), NullLogProvider.Instance, _supportPreVoting, _refusesToBeLeader );

			  ICollection<Neo4Net.causalclustering.core.consensus.RaftMessages_Directed> noMessages = Collections.emptyList();
			  IList<RaftLogCommand> noLogCommands = Collections.emptyList();

			  state.Update( new Outcome( null, TermConflict, LeaderConflict, LeaderCommitConflict, _votedFor, _votesForMe, _preVotesForMe, _lastLogIndexBeforeWeBecameLeader, _followerStates, false, noLogCommands, noMessages, emptySet(), CommitIndexConflict, emptySet(), _isPreElection ) );

			  return state;
		 }

		 public virtual RaftStateBuilder VotingMembers( params MemberId[] members )
		 {
			  return VotingMembers( asSet( members ) );
		 }

		 public virtual RaftStateBuilder ReplicationMembers( params MemberId[] members )
		 {
			  return ReplicationMembers( asSet( members ) );
		 }

		 public virtual RaftStateBuilder MessagesSentToFollower( MemberId member, long nextIndex )
		 {
			  return this;
		 }

		 private class StubMembership : RaftMembership
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal ISet<MemberId> VotingMembersConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly ISet<MemberId> ReplicationMembersConflict;

			  internal StubMembership( ISet<MemberId> votingMembers, ISet<MemberId> replicationMembers )
			  {
					this.VotingMembersConflict = votingMembers;
					this.ReplicationMembersConflict = replicationMembers;
			  }

			  public override ISet<MemberId> VotingMembers()
			  {
					return VotingMembersConflict;
			  }

			  public override ISet<MemberId> ReplicationMembers()
			  {
					return ReplicationMembersConflict;
			  }

			  public override void RegisterListener( Neo4Net.causalclustering.core.consensus.membership.RaftMembership_Listener listener )
			  {
					throw new System.NotSupportedException();
			  }
		 }
	}

}