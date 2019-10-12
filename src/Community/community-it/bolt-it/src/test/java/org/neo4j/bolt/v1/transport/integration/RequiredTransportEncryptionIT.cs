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
namespace Neo4Net.Bolt.v1.transport.integration
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;

	using Neo4jPackV1 = Neo4Net.Bolt.v1.messaging.Neo4jPackV1;
	using SocketConnection = Neo4Net.Bolt.v1.transport.socket.client.SocketConnection;
	using TransportConnection = Neo4Net.Bolt.v1.transport.socket.client.TransportConnection;
	using WebSocketConnection = Neo4Net.Bolt.v1.transport.socket.client.WebSocketConnection;
	using Neo4Net.Function;
	using Neo4Net.Graphdb.config;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using BoltConnector = Neo4Net.Kernel.configuration.BoltConnector;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.transport.integration.Neo4jWithSocket.DEFAULT_CONNECTOR_KEY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.transport.integration.TransportTestUtil.eventuallyDisconnects;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.BoltConnector.EncryptionLevel.REQUIRED;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class RequiredTransportEncryptionIT
	public class RequiredTransportEncryptionIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4jWithSocket server = new Neo4jWithSocket(getClass(), settings ->
		 public Neo4jWithSocket Server = new Neo4jWithSocket(this.GetType(), settings =>
		 {
					 Setting<BoltConnector.EncryptionLevel> encryptionLevel = ( new BoltConnector( DEFAULT_CONNECTOR_KEY ) ).encryption_level;
					 settings.put( encryptionLevel.name(), REQUIRED.name() );
		 });

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(0) public org.neo4j.function.Factory<org.neo4j.bolt.v1.transport.socket.client.TransportConnection> cf;
		 public Factory<TransportConnection> Cf;

		 private HostnamePort _address;
		 private TransportConnection _client;
		 private TransportTestUtil _util;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters public static java.util.Collection<org.neo4j.function.Factory<org.neo4j.bolt.v1.transport.socket.client.TransportConnection>> transports()
		 public static ICollection<Factory<TransportConnection>> Transports()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return asList( SocketConnection::new, WebSocketConnection::new );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  this._client = Cf.newInstance();
			  this._address = Server.lookupDefaultConnector();
			  this._util = new TransportTestUtil( new Neo4jPackV1() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void teardown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Teardown()
		 {
			  if ( _client != null )
			  {
					_client.disconnect();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseUnencryptedConnectionOnHandshakeWhenEncryptionIsRequired() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseUnencryptedConnectionOnHandshakeWhenEncryptionIsRequired()
		 {
			  // When
			  _client.connect( _address ).send( _util.acceptedVersions( 1, 0, 0, 0 ) );

			  assertThat( _client, eventuallyDisconnects() );
		 }
	}

}