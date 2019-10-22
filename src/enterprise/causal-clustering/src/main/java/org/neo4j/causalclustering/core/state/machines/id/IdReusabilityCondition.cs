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
namespace Neo4Net.causalclustering.core.state.machines.id
{

	using LeaderInfo = Neo4Net.causalclustering.core.consensus.LeaderInfo;
	using LeaderListener = Neo4Net.causalclustering.core.consensus.LeaderListener;
	using RaftMachine = Neo4Net.causalclustering.core.consensus.RaftMachine;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;

	/// <summary>
	/// Determines whether it is safe to reuse freed ids, based on current leader and tracking its own transactions.
	/// This should guarantee that a single freed id only ends up on a single core.
	/// </summary>
	public class IdReusabilityCondition : System.Func<bool>, LeaderListener
	{
		 private static readonly System.Func<bool> _alwaysFalse = () => false;

		 private CommandIndexTracker _commandIndexTracker;
		 private readonly RaftMachine _raftMachine;
		 private readonly MemberId _myself;

		 private volatile System.Func<bool> _currentSupplier = _alwaysFalse;

		 public IdReusabilityCondition( CommandIndexTracker commandIndexTracker, RaftMachine raftMachine, MemberId myself )
		 {
			  this._commandIndexTracker = commandIndexTracker;
			  this._raftMachine = raftMachine;
			  this._myself = myself;
			  raftMachine.RegisterListener( this );
		 }

		 public override bool AsBoolean
		 {
			 get
			 {
				  return _currentSupplier.AsBoolean;
			 }
		 }

		 public override void OnLeaderSwitch( LeaderInfo leaderInfo )
		 {
			  if ( _myself.Equals( leaderInfo.MemberId() ) )
			  {
					// We just became leader
					_currentSupplier = new LeaderIdReusabilityCondition( _commandIndexTracker, _raftMachine );
			  }
			  else
			  {
					// We are not the leader
					_currentSupplier = _alwaysFalse;
			  }
		 }

		 private class LeaderIdReusabilityCondition : System.Func<bool>
		 {
			  internal readonly CommandIndexTracker CommandIndexTracker;
			  internal readonly long CommandIdWhenBecameLeader;

			  internal volatile bool HasAppliedOldTransactions;

			  internal LeaderIdReusabilityCondition( CommandIndexTracker commandIndexTracker, RaftMachine raftMachine )
			  {
					this.CommandIndexTracker = commandIndexTracker;

					// Get highest command id seen
					this.CommandIdWhenBecameLeader = raftMachine.State().lastLogIndexBeforeWeBecameLeader();
			  }

			  public override bool AsBoolean
			  {
				  get
				  {
						// Once all transactions from previous term are applied we don't need to recheck with the CommandIndexTracker
						if ( !HasAppliedOldTransactions )
						{
							 HasAppliedOldTransactions = CommandIndexTracker.AppliedCommandIndex > CommandIdWhenBecameLeader;
						}
   
						return HasAppliedOldTransactions;
				  }
			  }
		 }
	}

}