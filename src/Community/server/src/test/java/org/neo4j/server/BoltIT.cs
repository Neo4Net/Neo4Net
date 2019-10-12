using System;
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
namespace Neo4Net.Server
{
	using After = org.junit.After;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using TemporaryFolder = org.junit.rules.TemporaryFolder;


	using SecureSocketConnection = Neo4Net.Bolt.v1.transport.socket.client.SecureSocketConnection;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using ConnectorPortRegister = Neo4Net.Kernel.configuration.ConnectorPortRegister;
	using JaxRsResponse = Neo4Net.Server.rest.JaxRsResponse;
	using RestRequest = Neo4Net.Server.rest.RestRequest;
	using JsonHelper = Neo4Net.Server.rest.domain.JsonHelper;
	using ExclusiveServerTestBase = Neo4Net.Test.server.ExclusiveServerTestBase;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.server.helpers.CommunityServerBuilder.serverOnRandomPorts;

	public class BoltIT : ExclusiveServerTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.TemporaryFolder tmpDir = new org.junit.rules.TemporaryFolder();
		 public TemporaryFolder TmpDir = new TemporaryFolder();

		 private CommunityNeoServer _server;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void stopTheServer()
		 public virtual void StopTheServer()
		 {
			  if ( _server != null )
			  {
					_server.stop();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLaunchBolt() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLaunchBolt()
		 {
			  // When I run Neo4j with Bolt enabled
			  _server = serverOnRandomPorts().withProperty((new BoltConnector("bolt")).type.name(), "BOLT").withProperty((new BoltConnector("bolt")).enabled.name(), "true").withProperty((new BoltConnector("bolt")).encryption_level.name(), "REQUIRED").withProperty((new BoltConnector("bolt")).listen_address.name(), "localhost:0").usingDataDir(TmpDir.Root.AbsolutePath).build();
			  _server.start();
			  ConnectorPortRegister connectorPortRegister = GetDependency( typeof( ConnectorPortRegister ) );

			  // Then
			  AssertEventuallyServerResponds( "localhost", connectorPortRegister.GetLocalAddress( "bolt" ).Port );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToSpecifyHostAndPort() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToSpecifyHostAndPort()
		 {
			  // When
			  StartServerWithBoltEnabled();

			  ConnectorPortRegister connectorPortRegister = GetDependency( typeof( ConnectorPortRegister ) );
			  // Then
			  AssertEventuallyServerResponds( "localhost", connectorPortRegister.GetLocalAddress( "bolt" ).Port );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void boltAddressShouldComeFromConnectorAdvertisedAddress() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void BoltAddressShouldComeFromConnectorAdvertisedAddress()
		 {
			  // Given
			  string host = "neo4j.com";

			  StartServerWithBoltEnabled( host, 9999, "localhost", 0 );
			  RestRequest request = ( new RestRequest( _server.baseUri() ) ).host(host);

			  // When
			  JaxRsResponse response = request.Get();

			  // Then
			  IDictionary<string, object> map = JsonHelper.jsonToMap( response.Entity );
			  assertThat( map["bolt"].ToString(), containsString("bolt://" + host + ":" + 9999) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void startServerWithBoltEnabled() throws java.io.IOException
		 private void StartServerWithBoltEnabled()
		 {
			  StartServerWithBoltEnabled( "localhost", 7687, "localhost", 7687 );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void startServerWithBoltEnabled(String advertisedHost, int advertisedPort, String listenHost, int listenPort) throws java.io.IOException
		 private void StartServerWithBoltEnabled( string advertisedHost, int advertisedPort, string listenHost, int listenPort )
		 {
			  _server = serverOnRandomPorts().withProperty((new BoltConnector("bolt")).type.name(), "BOLT").withProperty((new BoltConnector("bolt")).enabled.name(), "true").withProperty((new BoltConnector("bolt")).encryption_level.name(), "REQUIRED").withProperty((new BoltConnector("bolt")).advertised_address.name(), advertisedHost + ":" + advertisedPort).withProperty((new BoltConnector("bolt")).listen_address.name(), listenHost + ":" + listenPort).usingDataDir(TmpDir.Root.AbsolutePath).build();
			  _server.start();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertEventuallyServerResponds(String host, int port) throws Exception
		 private void AssertEventuallyServerResponds( string host, int port )
		 {
			  SecureSocketConnection conn = new SecureSocketConnection();
			  conn.Connect( new HostnamePort( host, port ) );
			  conn.Send( new sbyte[]{ ( sbyte ) 0x60, ( sbyte ) 0x60, unchecked( ( sbyte ) 0xB0 ), ( sbyte ) 0x17, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } );
			  assertThat( conn.Recv( 4 ), equalTo( new sbyte[]{ 0, 0, 0, 1 } ) );
		 }

		 private T GetDependency<T>( Type clazz )
		 {
				 clazz = typeof( T );
			  return _server.Database.Graph.DependencyResolver.resolveDependency( clazz );
		 }
	}

}