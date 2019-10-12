using System;
using System.Collections;
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
namespace Neo4Net.causalclustering.upstream.strategies
{

	using ReadReplicaInfo = Neo4Net.causalclustering.discovery.ReadReplicaInfo;
	using TopologyService = Neo4Net.causalclustering.discovery.TopologyService;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;

	public class ConnectRandomlyToServerGroupImpl
	{
		 private readonly IList<string> _groups;
		 private readonly TopologyService _topologyService;
		 private readonly MemberId _myself;
		 private readonly Random _random = new Random();

		 internal ConnectRandomlyToServerGroupImpl( IList<string> groups, TopologyService topologyService, MemberId myself )
		 {
			  this._groups = groups;
			  this._topologyService = topologyService;
			  this._myself = myself;
		 }

		 public virtual Optional<MemberId> UpstreamDatabase()
		 {
			  IDictionary<MemberId, ReadReplicaInfo> replicas = _topologyService.localReadReplicas().members();

			  IList<MemberId> choices = _groups.stream().flatMap(group => replicas.SetOfKeyValuePairs().Where(IsMyGroupAndNotMe(group))).map(DictionaryEntry.getKey).collect(Collectors.toList());

			  if ( choices.Count == 0 )
			  {
					return null;
			  }
			  else
			  {
					return choices[_random.Next( choices.Count )];
			  }
		 }

		 private System.Predicate<KeyValuePair<MemberId, ReadReplicaInfo>> IsMyGroupAndNotMe( string group )
		 {
			  return entry => entry.Value.groups().contains(group) && !entry.Key.Equals(_myself);
		 }
	}

}