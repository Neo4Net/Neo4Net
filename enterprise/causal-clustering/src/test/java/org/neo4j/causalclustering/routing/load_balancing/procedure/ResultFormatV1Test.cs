﻿using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.routing.load_balancing.procedure
{
	using Test = org.junit.Test;

	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class ResultFormatV1Test
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeToAndFromRecordFormat()
		 public virtual void ShouldSerializeToAndFromRecordFormat()
		 {
			  // given
			  IList<Endpoint> writers = new IList<Endpoint> { Endpoint.write( new AdvertisedSocketAddress( "write", 1 ) ), Endpoint.write( new AdvertisedSocketAddress( "write", 2 ) ), Endpoint.write( new AdvertisedSocketAddress( "write", 3 ) ) };
			  IList<Endpoint> readers = new IList<Endpoint> { Endpoint.read( new AdvertisedSocketAddress( "read", 4 ) ), Endpoint.read( new AdvertisedSocketAddress( "read", 5 ) ), Endpoint.read( new AdvertisedSocketAddress( "read", 6 ) ), Endpoint.read( new AdvertisedSocketAddress( "read", 7 ) ) };
			  IList<Endpoint> routers = singletonList( Endpoint.route( new AdvertisedSocketAddress( "route", 8 ) ) );

			  long ttlSeconds = 5;
			  LoadBalancingResult original = new LoadBalancingResult( routers, writers, readers, ttlSeconds * 1000 );

			  // when
			  object[] record = ResultFormatV1.Build( original );

			  // then
			  LoadBalancingResult parsed = ResultFormatV1.Parse( record );

			  assertEquals( original, parsed );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeToAndFromRecordFormatWithNoEntries()
		 public virtual void ShouldSerializeToAndFromRecordFormatWithNoEntries()
		 {
			  // given
			  IList<Endpoint> writers = emptyList();
			  IList<Endpoint> readers = emptyList();
			  IList<Endpoint> routers = emptyList();

			  long ttlSeconds = 0;
			  LoadBalancingResult original = new LoadBalancingResult( routers, writers, readers, ttlSeconds * 1000 );

			  // when
			  object[] record = ResultFormatV1.Build( original );

			  // then
			  LoadBalancingResult parsed = ResultFormatV1.Parse( record );

			  assertEquals( original, parsed );
		 }
	}

}