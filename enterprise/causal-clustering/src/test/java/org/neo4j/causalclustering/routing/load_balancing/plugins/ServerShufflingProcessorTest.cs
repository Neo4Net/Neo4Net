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
namespace Org.Neo4j.causalclustering.routing.load_balancing.plugins
{
	using Test = org.junit.Test;


	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class ServerShufflingProcessorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldShuffleServers() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldShuffleServers()
		 {
			  // given
			  LoadBalancingProcessor @delegate = mock( typeof( LoadBalancingPlugin ) );

			  IList<Endpoint> routers = new IList<Endpoint> { Endpoint.route( new AdvertisedSocketAddress( "route", 1 ) ), Endpoint.route( new AdvertisedSocketAddress( "route", 2 ) ) };
			  IList<Endpoint> writers = new IList<Endpoint> { Endpoint.write( new AdvertisedSocketAddress( "write", 3 ) ), Endpoint.write( new AdvertisedSocketAddress( "write", 4 ) ), Endpoint.write( new AdvertisedSocketAddress( "write", 5 ) ) };
			  IList<Endpoint> readers = new IList<Endpoint> { Endpoint.read( new AdvertisedSocketAddress( "read", 6 ) ), Endpoint.read( new AdvertisedSocketAddress( "read", 7 ) ), Endpoint.read( new AdvertisedSocketAddress( "read", 8 ) ), Endpoint.read( new AdvertisedSocketAddress( "read", 9 ) ) };

			  long ttl = 1000;
			  Org.Neo4j.causalclustering.routing.load_balancing.LoadBalancingProcessor_Result result = new LoadBalancingResult( new List<Endpoint>( routers ), new List<Endpoint>( writers ), new List<Endpoint>( readers ), ttl );

			  when( @delegate.Run( any() ) ).thenReturn(result);

			  ServerShufflingProcessor plugin = new ServerShufflingProcessor( @delegate );

			  bool completeShuffle = false;
			  for ( int i = 0; i < 1000; i++ ) // we try many times to make false negatives extremely unlikely
			  {
					// when
					Org.Neo4j.causalclustering.routing.load_balancing.LoadBalancingProcessor_Result shuffledResult = plugin.Run( Collections.emptyMap() );

					// then: should still contain the same endpoints
					assertThat( shuffledResult.RouteEndpoints(), containsInAnyOrder(routers.ToArray()) );
					assertThat( shuffledResult.WriteEndpoints(), containsInAnyOrder(writers.ToArray()) );
					assertThat( shuffledResult.ReadEndpoints(), containsInAnyOrder(readers.ToArray()) );
					assertEquals( shuffledResult.TtlMillis(), ttl );

					// but possibly in a different order
//JAVA TO C# CONVERTER WARNING: LINQ 'SequenceEqual' is not always identical to Java AbstractList 'equals':
//ORIGINAL LINE: boolean readersEqual = shuffledResult.readEndpoints().equals(readers);
					bool readersEqual = shuffledResult.ReadEndpoints().SequenceEqual(readers);
//JAVA TO C# CONVERTER WARNING: LINQ 'SequenceEqual' is not always identical to Java AbstractList 'equals':
//ORIGINAL LINE: boolean writersEqual = shuffledResult.writeEndpoints().equals(writers);
					bool writersEqual = shuffledResult.WriteEndpoints().SequenceEqual(writers);
//JAVA TO C# CONVERTER WARNING: LINQ 'SequenceEqual' is not always identical to Java AbstractList 'equals':
//ORIGINAL LINE: boolean routersEqual = shuffledResult.routeEndpoints().equals(routers);
					bool routersEqual = shuffledResult.RouteEndpoints().SequenceEqual(routers);

					if ( !readersEqual && !writersEqual && !routersEqual )
					{
						 // we don't stop until it is completely different
						 completeShuffle = true;
						 break;
					}
			  }

			  assertTrue( completeShuffle );
		 }
	}

}