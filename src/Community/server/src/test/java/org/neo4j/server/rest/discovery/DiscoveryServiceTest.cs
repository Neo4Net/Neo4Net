using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Server.rest.discovery
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Answers = org.mockito.Answers;


	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ConnectorPortRegister = Neo4Net.Kernel.configuration.ConnectorPortRegister;
	using ServerSettings = Neo4Net.Server.configuration.ServerSettings;
	using JsonFormat = Neo4Net.Server.rest.repr.formats.JsonFormat;
	using EntityOutputFormat = Neo4Net.Test.server.EntityOutputFormat;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.rest.discovery.CommunityDiscoverableURIs.communityDiscoverableURIs;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class DiscoveryServiceTest
	public class DiscoveryServiceTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static Iterable<Object[]> data()
		 public static IEnumerable<object[]> Data()
		 {
			  IList<object[]> cases = new List<object[]>();

			  // Default config
			  cases.Add( new object[]{ "http://localhost:7474", "http://localhost:7474", null, null, "bolt://localhost:7687" } );
			  cases.Add( new object[]{ "https://localhost:7473", "https://localhost:7473", null, null, "bolt://localhost:7687" } );
			  cases.Add( new object[]{ "http://www.example.com", "http://www.example.com", null, null, "bolt://www.example.com:7687" } );

			  // Default config + default listen address 0.0.0.0
			  cases.Add( new object[]{ "http://localhost:7474 - 0.0.0.0", "http://localhost:7474", null, OverrideWithDefaultListenAddress( "0.0.0.0" ), "bolt://localhost:7687" } );
			  cases.Add( new object[]{ "https://localhost:7473 - 0.0.0.0", "https://localhost:7473", null, OverrideWithDefaultListenAddress( "0.0.0.0" ), "bolt://localhost:7687" } );
			  cases.Add( new object[]{ "http://www.example.com - 0.0.0.0", "http://www.example.com", null, OverrideWithDefaultListenAddress( "0.0.0.0" ), "bolt://www.example.com:7687" } );

			  // Default config + default listen address ::
			  cases.Add( new object[]{ "http://localhost:7474 - ::", "http://localhost:7474", null, OverrideWithDefaultListenAddress( "::" ), "bolt://localhost:7687" } );
			  cases.Add( new object[]{ "https://localhost:7473 - ::", "https://localhost:7473", null, OverrideWithDefaultListenAddress( "::" ), "bolt://localhost:7687" } );
			  cases.Add( new object[]{ "http://www.example.com - ::", "http://www.example.com", null, OverrideWithDefaultListenAddress( "::" ), "bolt://www.example.com:7687" } );

			  // Default config + bolt listen address [::]:8888
			  cases.Add( new object[]{ "http://localhost:7474 - [::]:8888", "http://localhost:7474", null, CombineConfigOverriders( OverrideWithDefaultListenAddress( "::" ), OverrideWithListenAddress( "::", 8888 ) ), "bolt://localhost:8888" } );
			  cases.Add( new object[]{ "https://localhost:7473 - [::]:8888", "https://localhost:7473", null, CombineConfigOverriders( OverrideWithDefaultListenAddress( "::" ), OverrideWithListenAddress( "::", 8888 ) ), "bolt://localhost:8888" } );
			  cases.Add( new object[]{ "http://www.example.com - [::]:8888", "http://www.example.com", null, CombineConfigOverriders( OverrideWithDefaultListenAddress( "::" ), OverrideWithListenAddress( "::", 8888 ) ), "bolt://www.example.com:8888" } );

			  // Default config + advertised address
			  cases.Add( new object[]{ "http://www.example.com (advertised 1)", "http://www.example.com", null, OverrideWithAdvertisedAddress( "www.example.com", 8898 ), "bolt://www.example.com:8898" } );
			  cases.Add( new object[]{ "http://www.example.com (advertised 2)", "http://www.example.com", null, OverrideWithAdvertisedAddress( "www2.example.com", 7576 ), "bolt://www2.example.com:7576" } );

			  // Default config + advertised address with port 0
			  cases.Add( new object[]{ "http://www.example.com (advertised 3)", "http://www.example.com", Register( "bolt", "localhost", 9999 ), OverrideWithAdvertisedAddress( "www2.example.com", 0 ), "bolt://www2.example.com:9999" } );

			  // Default config + discoverable address
			  cases.Add( new object[]{ "http://www.example.com (discoverable 1)", "http://www.example.com", null, OverrideWithDiscoverable( "bolt://www.notanexample.com:7777" ), "bolt://www.notanexample.com:7777" } );
			  cases.Add( new object[]{ "http://www.example.com (discoverable 2)", "http://www.example.com", null, OverrideWithDiscoverable( "something://www.notanexample.com:7777" ), "something://www.notanexample.com:7777" } );

			  // Default config + discoverable address + advertised address
			  cases.Add( new object[]{ "http://www.example.com (discoverable and advertised 1)", "http://www.example.com", null, CombineConfigOverriders( OverrideWithDiscoverable( "bolt://www.notanexample.com:7777" ), OverrideWithAdvertisedAddress( "www.notanexample2.com", 8888 ) ), "bolt://www.notanexample.com:7777" } );
			  cases.Add( new object[]{ "http://www.example.com (discoverable and advertised 2)", "http://www.example.com", null, CombineConfigOverriders( OverrideWithAdvertisedAddress( "www.notanexample2.com", 8888 ), OverrideWithDiscoverable( "bolt://www.notanexample.com:7777" ) ), "bolt://www.notanexample.com:7777" } );

			  return cases;
		 }

		 private readonly NeoServer _neoServer = mock( typeof( NeoServer ), Answers.RETURNS_DEEP_STUBS );
		 private readonly ConnectorPortRegister _portRegistry = mock( typeof( ConnectorPortRegister ) );

		 private URI _baseUri;
		 private URI _dataUri;
		 private URI _managementUri;
		 private System.Action<ConnectorPortRegister> _portRegistryOverrider;
		 private System.Action<Config> _configOverrider;

		 private string _expectedDataUri;
		 private string _expectedManagementUri;
		 private string _expectedBoltUri;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public DiscoveryServiceTest(String description, String baseUri, System.Action<org.neo4j.kernel.configuration.ConnectorPortRegister> portRegistryOverrider, System.Action<org.neo4j.kernel.configuration.Config> configOverrider, String expectedBoltUri) throws Throwable
		 public DiscoveryServiceTest( string description, string baseUri, System.Action<ConnectorPortRegister> portRegistryOverrider, System.Action<Config> configOverrider, string expectedBoltUri )
		 {
			  this._baseUri = new URI( baseUri );
			  this._dataUri = new URI( "/data" );
			  this._managementUri = new URI( "/management" );
			  this._portRegistryOverrider = portRegistryOverrider;
			  this._configOverrider = configOverrider;

			  this._expectedDataUri = this._baseUri.resolve( this._dataUri ).ToString();
			  this._expectedManagementUri = this._baseUri.resolve( this._managementUri ).ToString();
			  this._expectedBoltUri = expectedBoltUri;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws java.net.URISyntaxException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  if ( _portRegistryOverrider != null )
			  {
					_portRegistryOverrider.accept( _portRegistry );
			  }
			  else
			  {
					when( _portRegistry.getLocalAddress( "bolt" ) ).thenReturn( new HostnamePort( "localhost", 7687 ) );
			  }

			  DependencyResolver dependencyResolver = mock( typeof( DependencyResolver ) );
			  when( dependencyResolver.ResolveDependency( typeof( ConnectorPortRegister ) ) ).thenReturn( _portRegistry );
			  when( _neoServer.Database.Graph.DependencyResolver ).thenReturn( dependencyResolver );
		 }

		 private Config MockConfig()
		 {
			  Dictionary<string, string> settings = new Dictionary<string, string>();
			  settings[GraphDatabaseSettings.auth_enabled.name()] = "false";
			  settings[( new BoltConnector( "bolt" ) ).type.name()] = "BOLT";
			  settings[( new BoltConnector( "bolt" ) ).enabled.name()] = "true";
			  settings[ServerSettings.management_api_path.name()] = _managementUri.ToString();
			  settings[ServerSettings.rest_api_path.name()] = _dataUri.ToString();

			  Config config = Config.defaults( settings );

			  if ( _configOverrider != null )
			  {
					_configOverrider.accept( config );
			  }

			  return config;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private DiscoveryService testDiscoveryService() throws java.net.URISyntaxException
		 private DiscoveryService TestDiscoveryService()
		 {
			  Config config = MockConfig();
			  return new DiscoveryService( config, new EntityOutputFormat( new JsonFormat(), _baseUri, null ), communityDiscoverableURIs(config, _portRegistry) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnValidJSON() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnValidJSON()
		 {
			  Response response = TestDiscoveryService().getDiscoveryDocument(UriInfo(_baseUri));
			  string json = StringHelper.NewString( ( sbyte[] ) response.Entity );

			  assertNotNull( json );
			  assertThat( json.Length, @is( greaterThan( 0 ) ) );
			  assertThat( json, @is( not( "\"\"" ) ) );
			  assertThat( json, @is( not( "null" ) ) );
		 }

		 private UriInfo UriInfo( URI baseUri )
		 {
			  UriInfo uriInfo = mock( typeof( UriInfo ) );
			  when( uriInfo.BaseUri ).thenReturn( baseUri );
			  return uriInfo;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnBoltURI() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnBoltURI()
		 {
			  Response response = TestDiscoveryService().getDiscoveryDocument(UriInfo(_baseUri));
			  string json = StringHelper.NewString( ( sbyte[] ) response.Entity );
			  assertThat( json, containsString( "\"bolt\" : \"" + _expectedBoltUri ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnDataURI() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnDataURI()
		 {
			  Response response = TestDiscoveryService().getDiscoveryDocument(UriInfo(_baseUri));
			  string json = StringHelper.NewString( ( sbyte[] ) response.Entity );
			  assertThat( json, containsString( "\"data\" : \"" + _expectedDataUri + "/\"" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnManagementURI() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnManagementURI()
		 {
			  Response response = TestDiscoveryService().getDiscoveryDocument(UriInfo(_baseUri));
			  string json = StringHelper.NewString( ( sbyte[] ) response.Entity );
			  assertThat( json, containsString( "\"management\" : \"" + _expectedManagementUri + "/\"" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnRedirectToAbsoluteAPIUsingOutputFormat() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnRedirectToAbsoluteAPIUsingOutputFormat()
		 {
			  Config config = Config.defaults( ServerSettings.browser_path, "/browser/" );

			  string baseUri = "http://www.example.com:5435";
			  DiscoveryService ds = new DiscoveryService( config, new EntityOutputFormat( new JsonFormat(), new URI(baseUri), null ), communityDiscoverableURIs(config, null) );

			  Response response = ds.RedirectToBrowser();

			  assertThat( response.Metadata.getFirst( "Location" ), @is( new URI( "http://www.example" + ".com:5435/browser/" ) ) );
		 }

		 private static System.Action<ConnectorPortRegister> Register( string connector, string host, int port )
		 {
			  return register => when( register.getLocalAddress( connector ) ).thenReturn( new HostnamePort( host, port ) );
		 }

		 private static System.Action<ConnectorPortRegister> CombineRegisterers( params System.Action<ConnectorPortRegister>[] overriders )
		 {
			  return config =>
			  {
				foreach ( Consumer<ConnectorPortRegister> overrider in overriders )
				{
					 overrider.accept( config );
				}
			  };
		 }

		 private static System.Action<Config> OverrideWithAdvertisedAddress( string host, int port )
		 {
			  return config => config.augment( ( new BoltConnector( "bolt" ) ).advertised_address.name(), AdvertisedSocketAddress.advertisedAddress(host, port) );
		 }

		 private static System.Action<Config> OverrideWithListenAddress( string host, int port )
		 {
			  return config => config.augment( ( new BoltConnector( "bolt" ) ).listen_address.name(), AdvertisedSocketAddress.advertisedAddress(host, port) );
		 }

		 private static System.Action<Config> OverrideWithDefaultListenAddress( string host )
		 {
			  return config => config.augment( GraphDatabaseSettings.default_listen_address, host );
		 }

		 private static System.Action<Config> OverrideWithDiscoverable( string uri )
		 {
			  return config => config.augment( ServerSettings.bolt_discoverable_address, uri );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs private static System.Action<org.neo4j.kernel.configuration.Config> combineConfigOverriders(System.Action<org.neo4j.kernel.configuration.Config>... overriders)
		 private static System.Action<Config> CombineConfigOverriders( params System.Action<Config>[] overriders )
		 {
			  return config =>
			  {
				foreach ( Consumer<Config> overrider in overriders )
				{
					 overrider.accept( config );
				}
			  };
		 }
	}

}