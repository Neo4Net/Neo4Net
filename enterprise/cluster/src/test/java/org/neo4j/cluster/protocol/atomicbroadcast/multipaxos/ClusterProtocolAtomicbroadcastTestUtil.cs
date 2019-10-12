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
namespace Org.Neo4j.cluster.protocol.atomicbroadcast.multipaxos
{

	public class ClusterProtocolAtomicbroadcastTestUtil
	{
		 private ClusterProtocolAtomicbroadcastTestUtil()
		 {
		 }

		 public static IEnumerable<InstanceId> Ids( int size )
		 {
			  IList<InstanceId> ids = new List<InstanceId>();
			  for ( int i = 1; i <= size; i++ )
			  {
					ids.Add( new InstanceId( i ) );
			  }
			  return ids;
		 }

		 public static IDictionary<InstanceId, URI> Members( int size )
		 {
			  IDictionary<InstanceId, URI> members = new Dictionary<InstanceId, URI>();
			  for ( int i = 1; i <= size; i++ )
			  {
					members[new InstanceId( i )] = URI.create( "http://localhost:" + ( 6000 + i ) + "?serverId=" + i );
			  }
			  return members;
		 }
	}

}