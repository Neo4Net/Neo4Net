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
namespace Neo4Net.Server
{
	using After = org.junit.After;
	using Test = org.junit.Test;


	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using ListenSocketAddress = Neo4Net.Helpers.ListenSocketAddress;
	using ConnectorPortRegister = Neo4Net.Kernel.configuration.ConnectorPortRegister;
	using HttpConnector = Neo4Net.Kernel.configuration.HttpConnector;
	using ServerSettings = Neo4Net.Server.configuration.ServerSettings;
	using JaxRsResponse = Neo4Net.Server.rest.JaxRsResponse;
	using RestRequest = Neo4Net.Server.rest.RestRequest;
	using ExclusiveServerTestBase = Neo4Net.Test.server.ExclusiveServerTestBase;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.helpers.CommunityServerBuilder.server;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.helpers.CommunityServerBuilder.serverOnRandomPorts;

	public class ServerConfigIT : ExclusiveServerTestBase
	{
		 private CommunityNeoServer _server;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void stopTheServer()
		 public virtual void StopTheServer()
		 {
			  _server.stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPickUpAddressFromConfig() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPickUpAddressFromConfig()
		 {
			  ListenSocketAddress nonDefaultAddress = new ListenSocketAddress( "0.0.0.0", 0 );
			  _server = _server().onAddress(nonDefaultAddress).usingDataDir(Folder.directory(Name.MethodName).AbsolutePath).build();
			  _server.start();

			  HostnamePort localHttpAddress = LocalHttpAddress;
			  assertNotEquals( HttpConnector.Encryption.NONE.defaultPort, localHttpAddress.Port );
			  assertEquals( nonDefaultAddress.Hostname, localHttpAddress.Host );

			  JaxRsResponse response = ( new RestRequest( _server.baseUri() ) ).get();

			  assertThat( response.Status, @is( 200 ) );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPickupRelativeUrisForManagementApiAndRestApi() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPickupRelativeUrisForManagementApiAndRestApi()
		 {
			  string dataUri = "a/different/data/uri/";
			  string managementUri = "a/different/management/uri/";

			  _server = serverOnRandomPorts().withRelativeRestApiUriPath("/" + dataUri).usingDataDir(Folder.directory(Name.MethodName).AbsolutePath).withRelativeManagementApiUriPath("/" + managementUri).build();
			  _server.start();

			  JaxRsResponse response = ( new RestRequest() ).get(_server.baseUri().ToString() + dataUri, MediaType.TEXT_HTML_TYPE);
			  assertEquals( 200, response.Status );

			  response = ( new RestRequest() ).get(_server.baseUri().ToString() + managementUri);
			  assertEquals( 200, response.Status );
			  response.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGenerateWADLWhenExplicitlyEnabledInConfig() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldGenerateWADLWhenExplicitlyEnabledInConfig()
		 {
			  _server = serverOnRandomPorts().withProperty(ServerSettings.wadl_enabled.name(), "true").usingDataDir(Folder.directory(Name.MethodName).AbsolutePath).build();
			  _server.start();
			  JaxRsResponse response = ( new RestRequest() ).get(_server.baseUri().ToString() + "application.wadl", MediaType.WILDCARD_TYPE);

			  assertEquals( 200, response.Status );
			  assertEquals( "application/vnd.sun.wadl+xml", response.Headers.get( "Content-Type" ).GetEnumerator().next() );
			  assertThat( response.Entity, containsString( "<application xmlns=\"http://wadl.dev.java" + ".net/2009/02\">" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotGenerateWADLWhenNotExplicitlyEnabledInConfig() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotGenerateWADLWhenNotExplicitlyEnabledInConfig()
		 {
			  _server = serverOnRandomPorts().usingDataDir(Folder.directory(Name.MethodName).AbsolutePath).build();
			  _server.start();
			  JaxRsResponse response = ( new RestRequest() ).get(_server.baseUri().ToString() + "application.wadl", MediaType.WILDCARD_TYPE);

			  assertEquals( 404, response.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotGenerateWADLWhenExplicitlyDisabledInConfig() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotGenerateWADLWhenExplicitlyDisabledInConfig()
		 {
			  _server = serverOnRandomPorts().withProperty(ServerSettings.wadl_enabled.name(), "false").usingDataDir(Folder.directory(Name.MethodName).AbsolutePath).build();
			  _server.start();
			  JaxRsResponse response = ( new RestRequest() ).get(_server.baseUri().ToString() + "application.wadl", MediaType.WILDCARD_TYPE);

			  assertEquals( 404, response.Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEnableConsoleServiceByDefault() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldEnableConsoleServiceByDefault()
		 {
			  // Given
			  _server = serverOnRandomPorts().usingDataDir(Folder.directory(Name.MethodName).AbsolutePath).build();
			  _server.start();

			  // When & then
			  assertEquals( 200, ( new RestRequest() ).get(_server.baseUri().ToString() + "db/manage/server/console").Status );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDisableConsoleServiceWhenAskedTo() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDisableConsoleServiceWhenAskedTo()
		 {
			  // Given
			  _server = serverOnRandomPorts().withProperty(ServerSettings.console_module_enabled.name(), "false").usingDataDir(Folder.directory(Name.MethodName).AbsolutePath).build();
			  _server.start();

			  // When & then
			  assertEquals( 404, ( new RestRequest() ).get(_server.baseUri().ToString() + "db/manage/server/console").Status );
		 }

		 private HostnamePort LocalHttpAddress
		 {
			 get
			 {
				  ConnectorPortRegister connectorPortRegister = _server.Database.Graph.DependencyResolver.resolveDependency( typeof( ConnectorPortRegister ) );
				  return connectorPortRegister.GetLocalAddress( "http" );
			 }
		 }
	}

}