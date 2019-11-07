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
namespace Neo4Net.Bolt.v1.transport.integration
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using SecureSocketConnection = Neo4Net.Bolt.v1.transport.socket.client.SecureSocketConnection;
	using SecureWebSocketConnection = Neo4Net.Bolt.v1.transport.socket.client.SecureWebSocketConnection;
	using SocketConnection = Neo4Net.Bolt.v1.transport.socket.client.SocketConnection;
	using TransportConnection = Neo4Net.Bolt.v1.transport.socket.client.TransportConnection;
	using WebSocketConnection = Neo4Net.Bolt.v1.transport.socket.client.WebSocketConnection;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class ConnectionIT
	public class ConnectionIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4NetWithSocket server = new Neo4NetWithSocket(getClass());
		 public Neo4NetWithSocket Server = new Neo4NetWithSocket( this.GetType() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter public Neo4Net.bolt.v1.transport.socket.client.TransportConnection connection;
		 public TransportConnection Connection;

		 private HostnamePort _address;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters public static java.util.Collection<Neo4Net.bolt.v1.transport.socket.client.TransportConnection> transports()
		 public static ICollection<TransportConnection> Transports()
		 {
			  return asList( new SecureSocketConnection(), new SocketConnection(), new SecureWebSocketConnection(), new WebSocketConnection() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _address = Server.lookupDefaultConnector();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanup() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Cleanup()
		 {
			  if ( Connection != null )
			  {
					Connection.disconnect();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCloseConnectionOnInvalidHandshake() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCloseConnectionOnInvalidHandshake()
		 {
			  // GIVEN
			  Connection.connect( _address );

			  // WHEN
			  Connection.send( new sbyte[]{ unchecked( ( sbyte ) 0xDE ), unchecked( ( sbyte ) 0xAD ), unchecked( ( sbyte ) 0xB0 ), ( sbyte ) 0x17, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } );

			  // THEN
			  Exception.expect( typeof( IOException ) );
			  Connection.recv( 4 );
		 }
	}

}