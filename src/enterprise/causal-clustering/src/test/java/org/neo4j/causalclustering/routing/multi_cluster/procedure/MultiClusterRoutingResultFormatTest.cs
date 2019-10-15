﻿using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.routing.multi_cluster.procedure
{
	using Test = org.junit.Test;


	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class MultiClusterRoutingResultFormatTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeToAndFromRecordFormat()
		 public virtual void ShouldSerializeToAndFromRecordFormat()
		 {
			  IList<Endpoint> fooRouters = new IList<Endpoint> { Endpoint.route( new AdvertisedSocketAddress( "host1", 1 ) ), Endpoint.route( new AdvertisedSocketAddress( "host2", 1 ) ), Endpoint.route( new AdvertisedSocketAddress( "host3", 1 ) ) };

			  IList<Endpoint> barRouters = new IList<Endpoint> { Endpoint.route( new AdvertisedSocketAddress( "host4", 1 ) ), Endpoint.route( new AdvertisedSocketAddress( "host5", 1 ) ), Endpoint.route( new AdvertisedSocketAddress( "host6", 1 ) ) };

			  IDictionary<string, IList<Endpoint>> routers = new Dictionary<string, IList<Endpoint>>();
			  routers["foo"] = fooRouters;
			  routers["bar"] = barRouters;

			  long ttlSeconds = 5;
			  MultiClusterRoutingResult original = new MultiClusterRoutingResult( routers, ttlSeconds * 1000 );

			  object[] record = MultiClusterRoutingResultFormat.Build( original );

			  MultiClusterRoutingResult parsed = MultiClusterRoutingResultFormat.Parse( record );

			  assertEquals( original, parsed );
		 }
	}

}