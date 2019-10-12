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
namespace Neo4Net.causalclustering.core.consensus.explorer.action
{

	using MemberId = Neo4Net.causalclustering.identity.MemberId;

	public class NewEntry : Action
	{
		 private readonly MemberId _member;

		 public NewEntry( MemberId member )
		 {
			  this._member = member;
		 }

		 public override ClusterState Advance( ClusterState previous )
		 {
			  ClusterState newClusterState = new ClusterState( previous );
			  LinkedList<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> newQueue = new LinkedList<Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage>( previous.Queues[_member] );
			  newQueue.AddLast( new Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_Request( _member, new ReplicatedString( "content" ) ) );
			  newClusterState.Queues[_member] = newQueue;
			  return newClusterState;
		 }
	}

}