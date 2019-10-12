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
namespace Neo4Net.causalclustering.discovery
{

	using MemberId = Neo4Net.causalclustering.identity.MemberId;

	public interface Topology<T> where T : DiscoveryServerInfo
	{
		 IDictionary<MemberId, T> Members();

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default TopologyDifference difference(Topology<T> other)
	//	 {
	//		  Set<MemberId> members = members().keySet();
	//		  Set<MemberId> otherMembers = other.members().keySet();
	//
	//		  Set<Difference> added = otherMembers.stream().filter(m -> !members.contains(m)).map(memberId -> Difference.asDifference(other, memberId)).collect(toSet());
	//
	//		  Set<Difference> removed = members.stream().filter(m -> !otherMembers.contains(m)).map(memberId -> Difference.asDifference(this, memberId)).collect(toSet());
	//
	//		  return new TopologyDifference(added, removed);
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default java.util.Optional<T> find(org.neo4j.causalclustering.identity.MemberId memberId)
	//	 {
	//		  return Optional.ofNullable(members().get(memberId));
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java default interface methods:
//		 default java.util.Map<org.neo4j.causalclustering.identity.MemberId, T> filterHostsByDb(java.util.Map<org.neo4j.causalclustering.identity.MemberId, T> s, String dbName)
	//	 {
	//		  return s.entrySet().stream().filter(e -> e.getValue().getDatabaseName().equals(dbName)).collect(Collectors.toMap(Map.Entry::getKey, Map.Entry::getValue));
	//	 }

		 Topology<T> FilterTopologyByDb( string dbName );
	}

}