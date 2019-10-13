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
namespace Neo4Net.causalclustering.discovery
{
	using Test = org.junit.Test;

	using ConnectorUri = Neo4Net.causalclustering.discovery.ClientConnectorAddresses.ConnectorUri;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.ClientConnectorAddresses.Scheme.bolt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.ClientConnectorAddresses.Scheme.http;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.ClientConnectorAddresses.Scheme.https;

	public class ClientConnectorAddressesTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeToString()
		 public virtual void ShouldSerializeToString()
		 {
			  // given
			  ClientConnectorAddresses connectorAddresses = new ClientConnectorAddresses(new IList<ConnectorUri>
			  {
				  new ConnectorUri( bolt, new AdvertisedSocketAddress( "host", 1 ) ),
				  new ConnectorUri( http, new AdvertisedSocketAddress( "host", 2 ) ),
				  new ConnectorUri( https, new AdvertisedSocketAddress( "host", 3 ) ),
				  new ConnectorUri( bolt, new AdvertisedSocketAddress( "::1", 4 ) ),
				  new ConnectorUri( http, new AdvertisedSocketAddress( "::", 5 ) ),
				  new ConnectorUri( https, new AdvertisedSocketAddress( "fe80:1:2::3", 6 ) )
			  });

			  string expectedString = "bolt://host:1,http://host:2,https://host:3,bolt://[::1]:4,http://[::]:5,https://[fe80:1:2::3]:6";

			  // when
			  string connectorAddressesString = connectorAddresses.ToString();

			  // then
			  assertEquals( expectedString, connectorAddressesString );

			  // when
			  ClientConnectorAddresses @out = ClientConnectorAddresses.FromString( connectorAddressesString );

			  // then
			  assertEquals( connectorAddresses, @out );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSerializeWithNoHttpsAddress()
		 public virtual void ShouldSerializeWithNoHttpsAddress()
		 {
			  // given
			  ClientConnectorAddresses connectorAddresses = new ClientConnectorAddresses(asList(new ConnectorUri(bolt, new AdvertisedSocketAddress("host", 1)), new ConnectorUri(http, new AdvertisedSocketAddress("host", 2))
			 ));

			  // when
			  ClientConnectorAddresses @out = ClientConnectorAddresses.FromString( connectorAddresses.ToString() );

			  // then
			  assertEquals( connectorAddresses, @out );
		 }
	}

}