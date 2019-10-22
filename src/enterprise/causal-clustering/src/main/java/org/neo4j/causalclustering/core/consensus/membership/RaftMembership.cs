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

	using MemberId = Neo4Net.causalclustering.identity.MemberId;

	/// <summary>
	/// Exposes a view of the members of a Raft cluster. Essentially it gives access to two sets - the set of voting
	/// members and the set of replication members.
	/// This class also allows for listeners to be notified of membership changes.
	/// </summary>
	public interface RaftMembership
	{
		 /// <returns> members whose votes count towards consensus. The returned set should be considered immutable. </returns>
		 ISet<MemberId> VotingMembers();

		 /// <returns> members to which replication should be attempted. The returned set should be considered immutable. </returns>
		 ISet<MemberId> ReplicationMembers();

		 /// <summary>
		 /// Register a membership listener.
		 /// </summary>
		 /// <param name="listener"> The listener. </param>
		 void RegisterListener( RaftMembership_Listener listener );

		 /// <summary>
		 /// This interface must be implemented from whoever wants to be notified of membership changes. Membership changes
		 /// are additions to and removals from the voting and replication members set.
		 /// </summary>
	}

	 public interface RaftMembership_Listener
	 {
		  /// <summary>
		  /// This method is called on additions to and removals from either the voting or replication members sets.
		  /// The implementation has the responsibility of figuring out what the actual change is.
		  /// </summary>
		  void OnMembershipChanged();
	 }

}