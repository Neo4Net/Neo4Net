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
namespace Org.Neo4j.Bolt.v3.runtime.integration
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;

	using Neo4jWithSocket = Org.Neo4j.Bolt.v1.transport.integration.Neo4jWithSocket;
	using TransportTestUtil = Org.Neo4j.Bolt.v1.transport.integration.TransportTestUtil;
	using SecureSocketConnection = Org.Neo4j.Bolt.v1.transport.socket.client.SecureSocketConnection;
	using SecureWebSocketConnection = Org.Neo4j.Bolt.v1.transport.socket.client.SecureWebSocketConnection;
	using SocketConnection = Org.Neo4j.Bolt.v1.transport.socket.client.SocketConnection;
	using TransportConnection = Org.Neo4j.Bolt.v1.transport.socket.client.TransportConnection;
	using WebSocketConnection = Org.Neo4j.Bolt.v1.transport.socket.client.WebSocketConnection;
	using HelloMessage = Org.Neo4j.Bolt.v3.messaging.request.HelloMessage;
	using HostnamePort = Org.Neo4j.Helpers.HostnamePort;
	using MapUtil = Org.Neo4j.Helpers.Collection.MapUtil;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.messaging.util.MessageMatchers.msgSuccess;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v1.transport.integration.TransportTestUtil.eventuallyReceives;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v3.messaging.BoltProtocolV3ComponentFactory.newMessageEncoder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.v3.messaging.BoltProtocolV3ComponentFactory.newNeo4jPack;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.auth_enabled;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public abstract class BoltV3TransportBase
	public abstract class BoltV3TransportBase
	{
		 protected internal const string USER_AGENT = "TestClient/3.0";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.bolt.v1.transport.integration.Neo4jWithSocket server = new org.neo4j.bolt.v1.transport.integration.Neo4jWithSocket(getClass(), settings -> settings.put(auth_enabled.name(), "false"));
		 public Neo4jWithSocket Server = new Neo4jWithSocket( this.GetType(), settings => settings.put(auth_enabled.name(), "false") );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter public Class connectionClass;
		 public Type ConnectionClass;

		 protected internal HostnamePort Address;
		 protected internal TransportConnection Connection;
		 protected internal TransportTestUtil Util;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.List<Class> transports()
		 public static IList<Type> Transports()
		 {
			  return new IList<Type> { typeof( SocketConnection ), typeof( WebSocketConnection ), typeof( SecureSocketConnection ), typeof( SecureWebSocketConnection ) };
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  Address = Server.lookupDefaultConnector();
			  Connection = System.Activator.CreateInstance( ConnectionClass );
			  Util = new TransportTestUtil( newNeo4jPack(), newMessageEncoder() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  if ( Connection != null )
			  {
					Connection.disconnect();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void negotiateBoltV3() throws Exception
		 protected internal virtual void NegotiateBoltV3()
		 {
			  Connection.connect( Address ).send( Util.acceptedVersions( 3, 0, 0, 0 ) ).send( Util.chunk( new HelloMessage( MapUtil.map( "user_agent", USER_AGENT ) ) ) );

			  assertThat( Connection, eventuallyReceives( new sbyte[]{ 0, 0, 0, 3 } ) );
			  assertThat( Connection, Util.eventuallyReceives( msgSuccess() ) );
		 }
	}

}