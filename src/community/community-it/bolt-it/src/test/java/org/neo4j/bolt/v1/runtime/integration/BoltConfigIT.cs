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
namespace Neo4Net.Bolt.v1.runtime.integration
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using InitMessage = Neo4Net.Bolt.v1.messaging.request.InitMessage;
	using Neo4NetWithSocket = Neo4Net.Bolt.v1.transport.integration.Neo4NetWithSocket;
	using TransportConnection = Neo4Net.Bolt.v1.transport.socket.client.TransportConnection;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.bolt.v1.transport.integration.Neo4NetWithSocket.DEFAULT_CONNECTOR_KEY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.bolt.v1.transport.integration.TransportTestUtil.eventuallyDisconnects;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.configuration.BoltConnector.EncryptionLevel.REQUIRED;

	public class BoltConfigIT : AbstractBoltTransportsTest
	{
		 private const string ANOTHER_CONNECTOR_KEY = "1";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.bolt.v1.transport.integration.Neo4NetWithSocket server = new org.Neo4Net.bolt.v1.transport.integration.Neo4NetWithSocket(getClass(), settings ->
		 public Neo4NetWithSocket Server = new Neo4NetWithSocket(this.GetType(), settings =>
		 {
					 settings.put( ( new BoltConnector( DEFAULT_CONNECTOR_KEY ) ).type.name(), "BOLT" );
					 settings.put( ( new BoltConnector( DEFAULT_CONNECTOR_KEY ) ).enabled.name(), "true" );
					 settings.put( ( new BoltConnector( DEFAULT_CONNECTOR_KEY ) ).address.name(), "localhost:0" );
					 settings.put( ( new BoltConnector( ANOTHER_CONNECTOR_KEY ) ).type.name(), "BOLT" );
					 settings.put( ( new BoltConnector( ANOTHER_CONNECTOR_KEY ) ).enabled.name(), "true" );
					 settings.put( ( new BoltConnector( ANOTHER_CONNECTOR_KEY ) ).address.name(), "localhost:0" );
					 settings.put( ( new BoltConnector( ANOTHER_CONNECTOR_KEY ) ).encryption_level.name(), REQUIRED.name() );
		 });
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.SuppressOutput suppressOutput = org.Neo4Net.test.rule.SuppressOutput.suppressAll();
		 public SuppressOutput SuppressOutput = SuppressOutput.suppressAll();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSupportMultipleConnectors() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSupportMultipleConnectors()
		 {
			  HostnamePort address0 = Server.lookupConnector( DEFAULT_CONNECTOR_KEY );
			  AssertConnectionAccepted( address0, NewConnection() );

			  HostnamePort address1 = Server.lookupConnector( ANOTHER_CONNECTOR_KEY );
			  AssertConnectionRejected( address1, NewConnection() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertConnectionRejected(org.Neo4Net.helpers.HostnamePort address, org.Neo4Net.bolt.v1.transport.socket.client.TransportConnection client) throws Exception
		 private void AssertConnectionRejected( HostnamePort address, TransportConnection client )
		 {
			  client.Connect( address ).send( Util.defaultAcceptedVersions() );

			  assertThat( client, eventuallyDisconnects() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertConnectionAccepted(org.Neo4Net.helpers.HostnamePort address, org.Neo4Net.bolt.v1.transport.socket.client.TransportConnection client) throws Exception
		 private void AssertConnectionAccepted( HostnamePort address, TransportConnection client )
		 {
			  client.Connect( address ).send( Util.defaultAcceptedVersions() ).send(Util.chunk(new InitMessage("TestClient/1.1", emptyMap())));
			  assertThat( client, Util.eventuallyReceivesSelectedProtocolVersion() );
		 }
	}

}