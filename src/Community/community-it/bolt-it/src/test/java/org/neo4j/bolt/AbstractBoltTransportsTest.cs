using System;
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
namespace Neo4Net.Bolt
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Neo4jPack = Neo4Net.Bolt.messaging.Neo4jPack;
	using Neo4jPackV1 = Neo4Net.Bolt.v1.messaging.Neo4jPackV1;
	using TransportTestUtil = Neo4Net.Bolt.v1.transport.integration.TransportTestUtil;
	using SecureSocketConnection = Neo4Net.Bolt.v1.transport.socket.client.SecureSocketConnection;
	using SecureWebSocketConnection = Neo4Net.Bolt.v1.transport.socket.client.SecureWebSocketConnection;
	using SocketConnection = Neo4Net.Bolt.v1.transport.socket.client.SocketConnection;
	using TransportConnection = Neo4Net.Bolt.v1.transport.socket.client.TransportConnection;
	using WebSocketConnection = Neo4Net.Bolt.v1.transport.socket.client.WebSocketConnection;
	using Neo4jPackV2 = Neo4Net.Bolt.v2.messaging.Neo4jPackV2;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.runners.Parameterized.Parameter;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.runners.Parameterized.Parameters;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public abstract class AbstractBoltTransportsTest
	public abstract class AbstractBoltTransportsTest
	{
		 private static readonly IList<Type> _connectionClasses = Arrays.asList( typeof( SocketConnection ), typeof( WebSocketConnection ), typeof( SecureSocketConnection ), typeof( SecureWebSocketConnection ) );

		 private static readonly IList<Neo4jPack> _neo4jPackVersions = Arrays.asList( new Neo4jPackV1(), new Neo4jPackV2() );

		 [Parameter(0)]
		 public Type ConnectionClass;

		 [Parameter(1)]
		 public Neo4jPack Neo4jPack;

		 [Parameter(2)]
		 public string Name;

		 protected internal HostnamePort Address;
		 protected internal TransportConnection Connection;
		 protected internal TransportTestUtil Util;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void initializeConnectionAndUtil() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void InitializeConnectionAndUtil()
		 {
			  Connection = System.Activator.CreateInstance( ConnectionClass );
			  Util = new TransportTestUtil( Neo4jPack );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void disconnectFromDatabase() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DisconnectFromDatabase()
		 {
			  if ( Connection != null )
			  {
					Connection.disconnect();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters(name = "{2}") public static java.util.List<Object[]> parameters()
		 public static IList<object[]> Parameters()
		 {
			  IList<object[]> result = new List<object[]>();
			  foreach ( Type connectionClass in _connectionClasses )
			  {
					foreach ( Neo4jPack neo4jPack in _neo4jPackVersions )
					{
						 result.Add( new object[]{ connectionClass, neo4jPack, NewName( connectionClass, neo4jPack ) } );
					}
			  }
			  return result;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected org.neo4j.bolt.v1.transport.socket.client.TransportConnection newConnection() throws Exception
		 protected internal virtual TransportConnection NewConnection()
		 {
			  return System.Activator.CreateInstance( ConnectionClass );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void reconnect() throws Exception
		 protected internal virtual void Reconnect()
		 {
			  if ( Connection != null )
			  {
					Connection.disconnect();
			  }
			  Connection = NewConnection();
		 }

		 private static string NewName( Type connectionClass, Neo4jPack neo4jPack )
		 {
			  return connectionClass.Name + " & " + neo4jPack;
		 }
	}

}