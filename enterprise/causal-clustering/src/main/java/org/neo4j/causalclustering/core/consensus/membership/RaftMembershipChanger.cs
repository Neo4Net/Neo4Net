using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.core.consensus.membership
{

	using ReadableRaftLog = Org.Neo4j.causalclustering.core.consensus.log.ReadableRaftLog;
	using Role = Org.Neo4j.causalclustering.core.consensus.roles.Role;
	using Org.Neo4j.causalclustering.core.consensus.roles.follower;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

	/// <summary>
	/// This class carries the core raft membership change state machine, which defines the
	/// legal state transitions when adding or removing members from the raft voting group.
	/// 
	/// The state machine has these 4 states:
	/// 
	/// <pre>
	///   INACTIVE                    completely inactive, not leader
	/// 
	///   IDLE,                       leader, but idle, no work to do
	///   CATCHUP IN PROGRESS,        member catching up
	///   CONSENSUS IN PROGRESS       caught up member being added to voting group
	/// </pre>
	/// 
	/// The normal progression when adding a member is:
	/// <pre>
	///   IDLE->CATCHUP->CONSENSUS->IDLE
	/// </pre>
	/// 
	/// the normal progression when removing a member is:
	/// <pre>
	///   IDLE->CONSENSUS->IDLE
	/// </pre>
	/// 
	/// Only a single member change is handled at a time.
	/// </summary>
	internal class RaftMembershipChanger
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			State = new Inactive( this );
		}

		 private readonly Log _log;
		 public RaftMembershipStateMachineEventHandler State;

		 private readonly ReadableRaftLog _raftLog;
		 private readonly Clock _clock;
		 private readonly long _electionTimeout;

		 private readonly RaftMembershipManager _membershipManager;
		 private long _catchupTimeout;

		 private MemberId _catchingUpMember;

		 internal RaftMembershipChanger( ReadableRaftLog raftLog, Clock clock, long electionTimeout, LogProvider logProvider, long catchupTimeout, RaftMembershipManager membershipManager )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  this._raftLog = raftLog;
			  this._clock = clock;
			  this._electionTimeout = electionTimeout;
			  this._catchupTimeout = catchupTimeout;
			  this._membershipManager = membershipManager;
			  this._log = logProvider.getLog( this.GetType() );
		 }

		 private void HandleState( RaftMembershipStateMachineEventHandler newState )
		 {
			 lock ( this )
			 {
				  RaftMembershipStateMachineEventHandler oldState = State;
				  this.State = newState;
      
				  if ( oldState != newState )
				  {
						oldState.OnExit();
						newState.OnEntry();
      
						_log.info( newState.ToString() );
						_membershipManager.stateChanged();
				  }
			 }
		 }

		 internal virtual void OnRole( Role role )
		 {
			  HandleState( State.onRole( role ) );
		 }

		 internal virtual void OnRaftGroupCommitted()
		 {
			  HandleState( State.onRaftGroupCommitted() );
		 }

		 internal virtual void OnFollowerStateChange( FollowerStates<MemberId> followerStates )
		 {
			  HandleState( State.onFollowerStateChange( followerStates ) );
		 }

		 internal virtual void OnMissingMember( MemberId member )
		 {
			  HandleState( State.onMissingMember( member ) );
		 }

		 internal virtual void OnSuperfluousMember( MemberId member )
		 {
			  HandleState( State.onSuperfluousMember( member ) );
		 }

		 internal virtual void OnTargetChanged( ISet<MemberId> targetMembers )
		 {
			  HandleState( State.onTargetChanged( targetMembers ) );
		 }

		 private class Inactive : RaftMembershipStateMachineEventHandler_Adapter
		 {
			 private readonly RaftMembershipChanger _outerInstance;

			 public Inactive( RaftMembershipChanger outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override RaftMembershipStateMachineEventHandler OnRole( Role role )
			  {
					if ( role == Role.LEADER )
					{
						 if ( outerInstance.membershipManager.UncommittedMemberChangeInLog() )
						 {
							  return new ConsensusInProgress( _outerInstance );
						 }
						 else
						 {
							  return new Idle( _outerInstance );
						 }
					}
					return this;
			  }

			  public override string ToString()
			  {
					return "Inactive{}";
			  }
		 }

		 internal abstract class ActiveBaseState : RaftMembershipStateMachineEventHandler_Adapter
		 {
			 private readonly RaftMembershipChanger _outerInstance;

			 public ActiveBaseState( RaftMembershipChanger outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override RaftMembershipStateMachineEventHandler OnRole( Role role )
			  {
					if ( role != Role.LEADER )
					{
						 return new Inactive( _outerInstance );
					}
					else
					{
						 return this;
					}
			  }
		 }

		 private class Idle : ActiveBaseState
		 {
			 private readonly RaftMembershipChanger _outerInstance;

			 public Idle( RaftMembershipChanger outerInstance ) : base( outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override RaftMembershipStateMachineEventHandler OnMissingMember( MemberId member )
			  {
					return new CatchingUp( _outerInstance, member );
			  }

			  public override RaftMembershipStateMachineEventHandler OnSuperfluousMember( MemberId member )
			  {
					ISet<MemberId> updatedVotingMembers = new HashSet<MemberId>( outerInstance.membershipManager.VotingMembers() );
					updatedVotingMembers.remove( member );
					outerInstance.membershipManager.DoConsensus( updatedVotingMembers );

					return new ConsensusInProgress( _outerInstance );
			  }

			  public override string ToString()
			  {
					return "Idle{}";
			  }
		 }

		 private class CatchingUp : ActiveBaseState
		 {
			 private readonly RaftMembershipChanger _outerInstance;

			  internal readonly CatchupGoalTracker CatchupGoalTracker;
			  internal bool MovingToConsensus;

			  internal CatchingUp( RaftMembershipChanger outerInstance, MemberId member ) : base( outerInstance )
			  {
				  this._outerInstance = outerInstance;
					this.CatchupGoalTracker = new CatchupGoalTracker( outerInstance.raftLog, outerInstance.clock, outerInstance.electionTimeout, outerInstance.catchupTimeout );
					outerInstance.catchingUpMember = member;
			  }

			  public override void OnEntry()
			  {
					outerInstance.membershipManager.AddAdditionalReplicationMember( outerInstance.catchingUpMember );
					outerInstance.log.Info( "Adding replication member: " + outerInstance.catchingUpMember );
			  }

			  public override void OnExit()
			  {
					if ( !MovingToConsensus )
					{
						 outerInstance.membershipManager.RemoveAdditionalReplicationMember( outerInstance.catchingUpMember );
						 outerInstance.log.Info( "Removing replication member: " + outerInstance.catchingUpMember );
					}
			  }

			  public override RaftMembershipStateMachineEventHandler OnRole( Role role )
			  {
					if ( role != Role.LEADER )
					{
						 return new Inactive( _outerInstance );
					}
					else
					{
						 return this;
					}
			  }

			  public override RaftMembershipStateMachineEventHandler OnFollowerStateChange( FollowerStates<MemberId> followerStates )
			  {
					CatchupGoalTracker.updateProgress( followerStates.Get( outerInstance.catchingUpMember ) );

					if ( CatchupGoalTracker.Finished )
					{
						 if ( CatchupGoalTracker.GoalAchieved )
						 {
							  ISet<MemberId> updatedVotingMembers = new HashSet<MemberId>( outerInstance.membershipManager.VotingMembers() );
							  updatedVotingMembers.Add( outerInstance.catchingUpMember );
							  outerInstance.membershipManager.DoConsensus( updatedVotingMembers );

							  MovingToConsensus = true;
							  return new ConsensusInProgress( _outerInstance );
						 }
						 else
						 {
							  return new Idle( _outerInstance );
						 }
					}
					return this;
			  }

			  public override RaftMembershipStateMachineEventHandler OnTargetChanged( ISet<object> targetMembers )
			  {
					if ( !targetMembers.Contains( outerInstance.catchingUpMember ) )
					{
						 return new Idle( _outerInstance );
					}
					else
					{
						 return this;
					}
			  }

			  public override string ToString()
			  {
					return format( "CatchingUp{catchupGoalTracker=%s, catchingUpMember=%s}", CatchupGoalTracker, outerInstance.catchingUpMember );
			  }
		 }

		 private class ConsensusInProgress : ActiveBaseState
		 {
			 private readonly RaftMembershipChanger _outerInstance;

			 public ConsensusInProgress( RaftMembershipChanger outerInstance ) : base( outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override RaftMembershipStateMachineEventHandler OnRaftGroupCommitted()
			  {
					return new Idle( _outerInstance );
			  }

			  public override void OnEntry()
			  {
			  }

			  public override void OnExit()
			  {
					outerInstance.membershipManager.RemoveAdditionalReplicationMember( outerInstance.catchingUpMember );
					outerInstance.log.Info( "Removing replication member: " + outerInstance.catchingUpMember );
			  }

			  public override string ToString()
			  {
					return "ConsensusInProgress{}";
			  }
		 }
	}

}