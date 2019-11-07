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
namespace Neo4Net.causalclustering.core.consensus.membership
{

	using RaftLogCursor = Neo4Net.causalclustering.core.consensus.log.RaftLogCursor;
	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using ReadableRaftLog = Neo4Net.causalclustering.core.consensus.log.ReadableRaftLog;
	using RaftLogCommand = Neo4Net.causalclustering.core.consensus.outcome.RaftLogCommand;
	using Role = Neo4Net.causalclustering.core.consensus.roles.Role;
	using Neo4Net.causalclustering.core.consensus.roles.follower;
	using SendToMyself = Neo4Net.causalclustering.core.replication.SendToMyself;
	using Neo4Net.causalclustering.core.state.storage;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterables.first;

	/// <summary>
	/// This class drives raft membership changes by glueing together various components:
	/// - target membership from hazelcast
	/// - raft membership state machine
	/// - raft log events
	/// </summary>
	public class RaftMembershipManager : LifecycleAdapter, RaftMembership, Neo4Net.causalclustering.core.consensus.outcome.RaftLogCommand_Handler
	{
		 private RaftMembershipChanger _membershipChanger;

		 private ISet<MemberId> _targetMembers;

		 private readonly SendToMyself _sendToMyself;
		 private readonly RaftGroup_Builder<MemberId> _memberSetBuilder;
		 private readonly ReadableRaftLog _raftLog;
		 private readonly Log _log;
		 private readonly StateStorage<RaftMembershipState> _storage;

		 private System.Func<long> _recoverFromIndexSupplier;
		 private RaftMembershipState _state;

		 private readonly int _minimumConsensusGroupSize;

		 private volatile ISet<MemberId> _votingMembers = Collections.unmodifiableSet( new HashSet<MemberId>() );
		 // votingMembers + additionalReplicationMembers
		 private volatile ISet<MemberId> _replicationMembers = Collections.unmodifiableSet( new HashSet<MemberId>() );

		 private ISet<RaftMembership_Listener> _listeners = new HashSet<RaftMembership_Listener>();
		 private ISet<MemberId> _additionalReplicationMembers = new HashSet<MemberId>();

		 public RaftMembershipManager( SendToMyself sendToMyself, RaftGroup_Builder<MemberId> memberSetBuilder, ReadableRaftLog raftLog, LogProvider logProvider, int minimumConsensusGroupSize, long electionTimeout, Clock clock, long catchupTimeout, StateStorage<RaftMembershipState> membershipStorage )
		 {
			  this._sendToMyself = sendToMyself;
			  this._memberSetBuilder = memberSetBuilder;
			  this._raftLog = raftLog;
			  this._minimumConsensusGroupSize = minimumConsensusGroupSize;
			  this._storage = membershipStorage;
			  this._log = logProvider.getLog( this.GetType() );
			  this._membershipChanger = new RaftMembershipChanger( raftLog, clock, electionTimeout, logProvider, catchupTimeout, this );
		 }

		 public virtual System.Func<long> RecoverFromIndexSupplier
		 {
			 set
			 {
				  this._recoverFromIndexSupplier = value;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws java.io.IOException
		 public override void Start()
		 {
			  this._state = _storage.InitialState;
			  long recoverFromIndex = _recoverFromIndexSupplier.AsLong;
			  _log.info( "Membership state before recovery: " + _state );
			  _log.info( "Recovering from: " + recoverFromIndex + " to: " + _raftLog.appendIndex() );

			  using ( RaftLogCursor cursor = _raftLog.getEntryCursor( recoverFromIndex ) )
			  {
					while ( cursor.Next() )
					{
						 Append( cursor.Index(), cursor.get() );
					}
			  }

			  _log.info( "Membership state after recovery: " + _state );
			  UpdateMemberSets();
		 }

		 public virtual ISet<MemberId> TargetMembershipSet
		 {
			 set
			 {
				  bool targetMembershipChanged = !value.SetEquals( this._targetMembers );
   
				  this._targetMembers = new HashSet<MemberId>( value );
   
				  if ( targetMembershipChanged )
				  {
						_log.info( "Target membership: " + value );
				  }
   
				  _membershipChanger.onTargetChanged( value );
   
				  CheckForStartCondition();
			 }
		 }

		 private ISet<MemberId> MissingMembers()
		 {
			  if ( _targetMembers == null || VotingMembers() == null )
			  {
					return emptySet();
			  }
			  ISet<MemberId> missingMembers = new HashSet<MemberId>( _targetMembers );
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'removeAll' method:
			  missingMembers.removeAll( VotingMembers() );

			  return missingMembers;
		 }

		 /// <summary>
		 /// All the externally published sets are derived from the committed and appended sets.
		 /// </summary>
		 private void UpdateMemberSets()
		 {
			  _votingMembers = Collections.unmodifiableSet( _state.Latest );

			  HashSet<MemberId> newReplicationMembers = new HashSet<MemberId>( _votingMembers );
			  newReplicationMembers.addAll( _additionalReplicationMembers );

			  _replicationMembers = Collections.unmodifiableSet( newReplicationMembers );
			  _listeners.forEach( RaftMembership_Listener.onMembershipChanged );
		 }

		 /// <summary>
		 /// Adds an additional member to replicate to. Members that are joining need to
		 /// catch up sufficiently before they become part of the voting group.
		 /// </summary>
		 /// <param name="member"> The member which will be added to the replication group. </param>
		 internal virtual void AddAdditionalReplicationMember( MemberId member )
		 {
			  _additionalReplicationMembers.Add( member );
			  UpdateMemberSets();
		 }

		 /// <summary>
		 /// Removes a member previously part of the additional replication member group.
		 /// 
		 /// This either happens because they caught up sufficiently and became part of the
		 /// voting group or because they failed to catch up in time.
		 /// </summary>
		 /// <param name="member"> The member to remove from the replication group. </param>
		 internal virtual void RemoveAdditionalReplicationMember( MemberId member )
		 {
			  _additionalReplicationMembers.remove( member );
			  UpdateMemberSets();
		 }

		 private bool SafeToRemoveMember
		 {
			 get
			 {
				  ISet<MemberId> votingMembers = votingMembers();
				  bool safeToRemoveMember = votingMembers != null && votingMembers.Count > _minimumConsensusGroupSize;
   
				  if ( !safeToRemoveMember )
				  {
						ISet<MemberId> membersToRemove = SuperfluousMembers();
   
						_log.info( "Not safe to remove %s %s because it would reduce the number of voting members below the expected " + "cluster size of %d. Voting members: %s", membersToRemove.Count > 1 ? "members" : "member", membersToRemove, _minimumConsensusGroupSize, votingMembers );
				  }
   
				  return safeToRemoveMember;
			 }
		 }

		 private ISet<MemberId> SuperfluousMembers()
		 {
			  if ( _targetMembers == null || VotingMembers() == null )
			  {
					return emptySet();
			  }
			  ISet<MemberId> superfluousMembers = new HashSet<MemberId>( VotingMembers() );
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'removeAll' method:
			  superfluousMembers.removeAll( _targetMembers );

			  return superfluousMembers;
		 }

		 private void CheckForStartCondition()
		 {
			  if ( MissingMembers().Count > 0 )
			  {
					_membershipChanger.onMissingMember( first( MissingMembers() ) );
			  }
			  else if ( SuperfluousMembers().Count > 0 && SafeToRemoveMember )
			  {
					_membershipChanger.onSuperfluousMember( first( SuperfluousMembers() ) );
			  }
		 }

		 /// <summary>
		 /// Used by the membership changer for getting consensus on a new set of members.
		 /// </summary>
		 /// <param name="newVotingMemberSet"> The new set of members. </param>
		 internal virtual void DoConsensus( ISet<MemberId> newVotingMemberSet )
		 {
			  _log.info( "Getting consensus on new voting member set %s", newVotingMemberSet );
			  _sendToMyself.replicate( _memberSetBuilder.build( newVotingMemberSet ) );
		 }

		 /// <summary>
		 /// Called by the membership changer when it has changed state and in response
		 /// the membership manager potentially feeds it back with an event to start
		 /// a new membership change operation.
		 /// </summary>
		 internal virtual void StateChanged()
		 {
			  CheckForStartCondition();
		 }

		 public virtual void OnFollowerStateChange( FollowerStates<MemberId> followerStates )
		 {
			  _membershipChanger.onFollowerStateChange( followerStates );
		 }

		 public virtual void OnRole( Role role )
		 {
			  _membershipChanger.onRole( role );
		 }

		 public override ISet<MemberId> VotingMembers()
		 {
			  return _votingMembers;
		 }

		 public override ISet<MemberId> ReplicationMembers()
		 {
			  return _replicationMembers;
		 }

		 public override void RegisterListener( RaftMembership_Listener listener )
		 {
			  _listeners.Add( listener );
		 }

		 internal virtual bool UncommittedMemberChangeInLog()
		 {
			  return _state.uncommittedMemberChangeInLog();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void processLog(long commitIndex, java.util.Collection<Neo4Net.causalclustering.core.consensus.outcome.RaftLogCommand> logCommands) throws java.io.IOException
		 public virtual void ProcessLog( long commitIndex, ICollection<RaftLogCommand> logCommands )
		 {
			  foreach ( RaftLogCommand logCommand in logCommands )
			  {
					logCommand.Dispatch( this );
			  }

			  if ( _state.commit( commitIndex ) )
			  {
					_membershipChanger.onRaftGroupCommitted();
					_storage.persistStoreData( _state );
					UpdateMemberSets();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void append(long baseIndex, Neo4Net.causalclustering.core.consensus.log.RaftLogEntry... entries) throws java.io.IOException
		 public override void Append( long baseIndex, params RaftLogEntry[] entries )
		 {
			  /* The warnings in this method are rarely expected occurrences which warrant to be logged with significance. */

			  foreach ( RaftLogEntry entry in entries )
			  {
					if ( entry.Content() is RaftGroup )
					{
						 RaftGroup<MemberId> raftGroup = ( RaftGroup<MemberId> ) entry.Content();

						 if ( _state.uncommittedMemberChangeInLog() )
						 {
							  _log.warn( "Appending with uncommitted membership change in log" );
						 }

						 if ( _state.append( baseIndex, new HashSet<MemberId>( raftGroup.Members ) ) )
						 {
							  _log.info( "Appending new member set %s", _state );
							  _storage.persistStoreData( _state );
							  UpdateMemberSets();
						 }
						 else
						 {
							  _log.warn( "Appending member set was ignored. Current state: %s, Appended set: %s, Log index: %d%n", _state, raftGroup, baseIndex );
						 }
					}
					baseIndex++;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void truncate(long fromIndex) throws java.io.IOException
		 public override void Truncate( long fromIndex )
		 {
			  if ( _state.truncate( fromIndex ) )
			  {
					_storage.persistStoreData( _state );
					UpdateMemberSets();
			  }
		 }

		 public override void Prune( long pruneIndex )
		 {
			  // only the actual log prunes
		 }

		 public virtual MembershipEntry Committed
		 {
			 get
			 {
				  return _state.committed();
   
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void install(MembershipEntry committed) throws java.io.IOException
		 public virtual void Install( MembershipEntry committed )
		 {
			  _state = new RaftMembershipState( committed.LogIndex(), committed, null );
			  _storage.persistStoreData( _state );
			  UpdateMemberSets();
		 }
	}

}