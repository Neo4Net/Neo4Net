/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.causalclustering.core.replication
{
	using RaftMessages = Neo4Net.causalclustering.core.consensus.RaftMessages;
	using Neo4Net.causalclustering.messaging;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;

	public class SendToMyself
	{
		 private readonly MemberId _myself;
		 private readonly Outbound<MemberId, Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> _outbound;

		 public SendToMyself( MemberId myself, Outbound<MemberId, Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> outbound )
		 {
			  this._myself = myself;
			  this._outbound = outbound;
		 }

		 public virtual void Replicate( ReplicatedContent content )
		 {
			  _outbound.send( _myself, new Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_Request( _myself, content ) );
		 }
	}

}