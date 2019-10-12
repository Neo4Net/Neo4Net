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
namespace Org.Neo4j.causalclustering.discovery
{

	using LeaderInfo = Org.Neo4j.causalclustering.core.consensus.LeaderInfo;
	using LeaderListener = Org.Neo4j.causalclustering.core.consensus.LeaderListener;
	using RaftMachine = Org.Neo4j.causalclustering.core.consensus.RaftMachine;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;

	/// <summary>
	/// Makes the Raft aware of changes to the core topology and vice versa
	/// </summary>
	public class RaftCoreTopologyConnector : LifecycleAdapter, CoreTopologyService_Listener, LeaderListener
	{
		 private readonly CoreTopologyService _coreTopologyService;
		 private readonly RaftMachine _raftMachine;
		 private readonly string _dbName;

		 public RaftCoreTopologyConnector( CoreTopologyService coreTopologyService, RaftMachine raftMachine, string dbName )
		 {
			  this._coreTopologyService = coreTopologyService;
			  this._raftMachine = raftMachine;
			  this._dbName = dbName;
		 }

		 public override void Start()
		 {
			  _coreTopologyService.addLocalCoreTopologyListener( this );
			  _raftMachine.registerListener( this );
		 }

		 public override void OnCoreTopologyChange( CoreTopology coreTopology )
		 {
			 lock ( this )
			 {
				  ISet<MemberId> targetMembers = coreTopology.Members().Keys;
				  _raftMachine.TargetMembershipSet = targetMembers;
			 }
		 }

		 public override void OnLeaderSwitch( LeaderInfo leaderInfo )
		 {
			  _coreTopologyService.setLeader( leaderInfo, _dbName );
		 }

		 public override void OnLeaderStepDown( long stepDownTerm )
		 {
			  _coreTopologyService.handleStepDown( stepDownTerm, _dbName );
		 }

		 public override string DbName()
		 {
			  return this._dbName;
		 }
	}

}