using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.core.consensus.outcome
{

	using MemberId = Neo4Net.causalclustering.identity.MemberId;

	public class Messages : IEnumerable<KeyValuePair<MemberId, Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage>>
	{
		 private readonly IDictionary<MemberId, Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> _map;

		 internal Messages( IDictionary<MemberId, Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> map )
		 {
			  this._map = map;
		 }

		 public virtual bool HasMessageFor( MemberId member )
		 {
			  return _map.ContainsKey( member );
		 }

		 public virtual Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage MessageFor( MemberId member )
		 {
			  return _map[member];
		 }

		 public override IEnumerator<KeyValuePair<MemberId, Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage>> Iterator()
		 {
			  return _map.SetOfKeyValuePairs().GetEnumerator();
		 }
	}

}