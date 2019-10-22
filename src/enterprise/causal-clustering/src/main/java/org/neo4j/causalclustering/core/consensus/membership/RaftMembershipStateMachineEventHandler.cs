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

	using Role = Neo4Net.causalclustering.core.consensus.roles.Role;
	using Neo4Net.causalclustering.core.consensus.roles.follower;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;

	internal interface RaftMembershipStateMachineEventHandler
	{
		 RaftMembershipStateMachineEventHandler OnRole( Role role );

		 RaftMembershipStateMachineEventHandler OnRaftGroupCommitted();

		 RaftMembershipStateMachineEventHandler OnFollowerStateChange( FollowerStates<MemberId> followerStates );

		 RaftMembershipStateMachineEventHandler OnMissingMember( MemberId member );

		 RaftMembershipStateMachineEventHandler OnSuperfluousMember( MemberId member );

		 RaftMembershipStateMachineEventHandler OnTargetChanged( ISet<MemberId> targetMembers );

		 void OnExit();

		 void OnEntry();
	}

	 internal abstract class RaftMembershipStateMachineEventHandler_Adapter : RaftMembershipStateMachineEventHandler
	 {
		  public override RaftMembershipStateMachineEventHandler OnRole( Role role )
		  {
				return this;
		  }

		  public override RaftMembershipStateMachineEventHandler OnRaftGroupCommitted()
		  {
				return this;
		  }

		  public override RaftMembershipStateMachineEventHandler OnMissingMember( MemberId member )
		  {
				return this;
		  }

		  public override RaftMembershipStateMachineEventHandler OnSuperfluousMember( MemberId member )
		  {
				return this;
		  }

		  public override RaftMembershipStateMachineEventHandler OnFollowerStateChange( FollowerStates<MemberId> followerStates )
		  {
				return this;
		  }

		  public override RaftMembershipStateMachineEventHandler OnTargetChanged( ISet<object> targetMembers )
		  {
				return this;
		  }

		  public override void OnExit()
		  {
		  }
		  public override void OnEntry()
		  {
		  }
	 }

}