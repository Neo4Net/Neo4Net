using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Org.Neo4j.Server.rest.discovery
{
	using Test = org.junit.Test;


	using BoltConnector = Org.Neo4j.Kernel.configuration.BoltConnector;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ConnectorPortRegister = Org.Neo4j.Kernel.configuration.ConnectorPortRegister;
	using ServerSettings = Org.Neo4j.Server.configuration.ServerSettings;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.discovery.CommunityDiscoverableURIs.communityDiscoverableURIs;

	public class CommunityDiscoverableURIsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAdvertiseDataAndManagementURIs() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAdvertiseDataAndManagementURIs()
		 {
			  DiscoverableURIs uris = communityDiscoverableURIs( Config.defaults(), null );

			  assertEquals( map( "data", "/db/data/", "management", "/db/manage/" ), ToMap( uris ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAdvertiseBoltIfExplicitlyConfigured() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAdvertiseBoltIfExplicitlyConfigured()
		 {
			  DiscoverableURIs uris = communityDiscoverableURIs( Config.defaults( ServerSettings.bolt_discoverable_address, "bolt://banana.com:1234" ), null );

			  assertEquals( "bolt://banana.com:1234", ToMap( uris )["bolt"] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLookupBoltPortInRegisterIfConfiguredTo0() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLookupBoltPortInRegisterIfConfiguredTo0()
		 {
			  BoltConnector bolt = new BoltConnector( "honestJakesBoltConnector" );
			  ConnectorPortRegister register = new ConnectorPortRegister();
			  register.Register( bolt.Key(), new InetSocketAddress(1337) );

			  DiscoverableURIs uris = communityDiscoverableURIs( Config.builder().withSetting(bolt.AdvertisedAddress, "apple.com:0").withSetting(bolt.Enabled, "true").withSetting(bolt.Type, BoltConnector.ConnectorType.BOLT.name()).build(), register );

			  assertEquals( "bolt://apple.com:1337", ToMap( uris )["bolt"] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOmitBoltIfNoConnectorConfigured() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOmitBoltIfNoConnectorConfigured()
		 {
			  DiscoverableURIs uris = communityDiscoverableURIs( Config.builder().build(), null );

			  assertFalse( ToMap( uris ).ContainsKey( "bolt" ) );
		 }

		 private IDictionary<string, object> ToMap( DiscoverableURIs uris )
		 {
			  IDictionary<string, object> @out = new Dictionary<string, object>();
			  uris.ForEach( ( k, v ) => @out.put( k, v.toASCIIString() ) );
			  return @out;
		 }
	}

}