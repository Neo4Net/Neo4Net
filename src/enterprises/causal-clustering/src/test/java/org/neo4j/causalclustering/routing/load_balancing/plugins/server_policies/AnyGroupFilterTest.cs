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
namespace Neo4Net.causalclustering.routing.load_balancing.plugins.server_policies
{
	using Test = org.junit.Test;


	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;

	public class AnyGroupFilterTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnServersMatchingAnyGroup()
		 public virtual void ShouldReturnServersMatchingAnyGroup()
		 {
			  // given
			  AnyGroupFilter groupFilter = new AnyGroupFilter( asSet( "china-west", "europe" ) );

			  ServerInfo serverA = new ServerInfo( new AdvertisedSocketAddress( "bolt", 1 ), new MemberId( System.Guid.randomUUID() ), asSet("china-west") );
			  ServerInfo serverB = new ServerInfo( new AdvertisedSocketAddress( "bolt", 2 ), new MemberId( System.Guid.randomUUID() ), asSet("europe") );
			  ServerInfo serverC = new ServerInfo( new AdvertisedSocketAddress( "bolt", 3 ), new MemberId( System.Guid.randomUUID() ), asSet("china", "china-west") );
			  ServerInfo serverD = new ServerInfo( new AdvertisedSocketAddress( "bolt", 4 ), new MemberId( System.Guid.randomUUID() ), asSet("china-west", "china") );
			  ServerInfo serverE = new ServerInfo( new AdvertisedSocketAddress( "bolt", 5 ), new MemberId( System.Guid.randomUUID() ), asSet("china-east", "asia") );
			  ServerInfo serverF = new ServerInfo( new AdvertisedSocketAddress( "bolt", 6 ), new MemberId( System.Guid.randomUUID() ), asSet("europe-west") );
			  ServerInfo serverG = new ServerInfo( new AdvertisedSocketAddress( "bolt", 7 ), new MemberId( System.Guid.randomUUID() ), asSet("china-west", "europe") );
			  ServerInfo serverH = new ServerInfo( new AdvertisedSocketAddress( "bolt", 8 ), new MemberId( System.Guid.randomUUID() ), asSet("africa") );

			  ISet<ServerInfo> data = asSet( serverA, serverB, serverC, serverD, serverE, serverF, serverG, serverH );

			  // when
			  ISet<ServerInfo> output = groupFilter.Apply( data );

			  // then
			  ISet<int> ports = new HashSet<int>();
			  foreach ( ServerInfo info in output )
			  {
					ports.Add( info.BoltAddress().Port );
			  }

			  assertEquals( asSet( 1, 2, 3, 4, 7 ), ports );
		 }
	}

}