using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.causalclustering.core.consensus.membership
{

	using ExposedRaftState = Neo4Net.causalclustering.core.consensus.state.ExposedRaftState;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using DatabaseHealth = Neo4Net.Kernel.@internal.DatabaseHealth;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using Group = Neo4Net.Scheduler.Group;
	using JobHandle = Neo4Net.Scheduler.JobHandle;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;


	/// <summary>
	/// Waits until member has "fully joined" the raft membership.
	/// We consider a member fully joined where:
	/// <ul>
	/// <li>It is a member of the voting group
	/// (its opinion will count towards leader elections and committing entries), and</li>
	/// <li>It is sufficiently caught up with the leader,
	/// so that long periods of unavailability are unlikely, should the leader fail.</li>
	/// </ul>
	/// <para>
	/// To determine whether the member is sufficiently caught up, we check periodically how far behind we are,
	/// once every {@code maxCatchupLag}. If the leader is always moving forwards we will never fully catch up,
	/// so all we look for is that we have caught up with where the leader was the <i>previous</i> time
	/// that we checked.
	/// </para>
	/// </summary>
	public class MembershipWaiter
	{
		 public interface Monitor
		 {
			  void WaitingToHearFromLeader();

			  void WaitingToCatchupWithLeader( long localCommitIndex, long leaderCommitIndex );

			  void JoinedRaftGroup();
		 }

		 private readonly MemberId _myself;
		 private readonly JobScheduler _jobScheduler;
		 private readonly System.Func<DatabaseHealth> _dbHealthSupplier;
		 private readonly long _maxCatchupLag;
		 private long _currentCatchupDelayInMs;
		 private readonly Log _log;
		 private readonly Monitor _monitor;

		 public MembershipWaiter( MemberId myself, JobScheduler jobScheduler, System.Func<DatabaseHealth> dbHealthSupplier, long maxCatchupLag, LogProvider logProvider, Monitors monitors )
		 {
			  this._myself = myself;
			  this._jobScheduler = jobScheduler;
			  this._dbHealthSupplier = dbHealthSupplier;
			  this._maxCatchupLag = maxCatchupLag;
			  this._currentCatchupDelayInMs = maxCatchupLag;
			  this._log = logProvider.getLog( this.GetType() );
			  this._monitor = monitors.NewMonitor( typeof( Monitor ) );
		 }

		 internal virtual CompletableFuture<bool> WaitUntilCaughtUpMember( RaftMachine raft )
		 {
			  CompletableFuture<bool> catchUpFuture = new CompletableFuture<bool>();

			  Evaluator evaluator = new Evaluator( this, raft, catchUpFuture, _dbHealthSupplier );

			  JobHandle jobHandle = _jobScheduler.schedule( Group.MEMBERSHIP_WAITER, evaluator, _currentCatchupDelayInMs, MILLISECONDS );

			  catchUpFuture.whenComplete( ( result, e ) => jobHandle.cancel( true ) );

			  return catchUpFuture;
		 }

		 private class Evaluator : ThreadStart
		 {
			 private readonly MembershipWaiter _outerInstance;

			  internal readonly RaftMachine Raft;
			  internal readonly CompletableFuture<bool> CatchUpFuture;

			  internal long LastLeaderCommit;
			  internal readonly System.Func<DatabaseHealth> DbHealthSupplier;

			  internal Evaluator( MembershipWaiter outerInstance, RaftMachine raft, CompletableFuture<bool> catchUpFuture, System.Func<DatabaseHealth> dbHealthSupplier )
			  {
				  this._outerInstance = outerInstance;
					this.Raft = raft;
					this.CatchUpFuture = catchUpFuture;
					this.LastLeaderCommit = raft.State().leaderCommit();
					this.DbHealthSupplier = dbHealthSupplier;
			  }

			  public override void Run()
			  {
					if ( !DbHealthSupplier.get().Healthy )
					{
						 CatchUpFuture.completeExceptionally( DbHealthSupplier.get().cause() );
					}
					else if ( IAmAVotingMember() && CaughtUpWithLeader() )
					{
						 CatchUpFuture.complete( true );
						 outerInstance.monitor.JoinedRaftGroup();
					}
					else
					{
						 outerInstance.currentCatchupDelayInMs += SECONDS.toMillis( 1 );
						 long longerDelay = outerInstance.currentCatchupDelayInMs < outerInstance.maxCatchupLag ? outerInstance.currentCatchupDelayInMs : outerInstance.maxCatchupLag;
						 outerInstance.jobScheduler.Schedule( Group.MEMBERSHIP_WAITER, this, longerDelay, MILLISECONDS );
					}
			  }

			  internal virtual bool IAmAVotingMember()
			  {
					ISet<object> votingMembers = Raft.state().votingMembers();
					bool votingMember = votingMembers.Contains( outerInstance.myself );
					if ( !votingMember )
					{
						 outerInstance.log.Debug( "I (%s) am not a voting member: [%s]", outerInstance.myself, votingMembers );
					}
					return votingMember;
			  }

			  internal virtual bool CaughtUpWithLeader()
			  {
					bool caughtUpWithLeader = false;

					ExposedRaftState state = Raft.state();
					long localCommit = state.CommitIndex();
					LastLeaderCommit = state.LeaderCommit();
					if ( LastLeaderCommit != -1 )
					{
						 caughtUpWithLeader = localCommit == LastLeaderCommit;
						 outerInstance.monitor.WaitingToCatchupWithLeader( localCommit, LastLeaderCommit );
					}
					else
					{
						 outerInstance.monitor.WaitingToHearFromLeader();
					}
					return caughtUpWithLeader;
			  }
		 }

	}

}