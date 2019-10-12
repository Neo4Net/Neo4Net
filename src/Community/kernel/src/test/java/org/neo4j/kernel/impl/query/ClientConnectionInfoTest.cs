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
namespace Neo4Net.Kernel.impl.query
{
	using Test = org.junit.Test;

	using BoltConnectionInfo = Neo4Net.Kernel.impl.query.clientconnection.BoltConnectionInfo;
	using ClientConnectionInfo = Neo4Net.Kernel.impl.query.clientconnection.ClientConnectionInfo;
	using HttpConnectionInfo = Neo4Net.Kernel.impl.query.clientconnection.HttpConnectionInfo;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class ClientConnectionInfoTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void connectionDetailsForBoltQuerySource()
		 public virtual void ConnectionDetailsForBoltQuerySource()
		 {
			  // given
			  ClientConnectionInfo clientConnection = ( new BoltConnectionInfo( "bolt-42", "username", "neo4j-java-bolt-driver", new InetSocketAddress( "127.0.0.1", 56789 ), new InetSocketAddress( "127.0.0.1", 7687 ) ) ).withUsername( "username" );

			  // when
			  string connectionDetails = clientConnection.AsConnectionDetails();

			  // then
			  assertEquals( "bolt-session\tbolt\tusername\tneo4j-java-bolt-driver\t\tclient/127.0.0.1:56789\t" + "server/127.0.0.1:7687>\tusername", connectionDetails );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void connectionDetailsForHttpQuerySource()
		 public virtual void ConnectionDetailsForHttpQuerySource()
		 {
			  // given
			  ClientConnectionInfo clientConnection = ( new HttpConnectionInfo( "http-42", "http", new InetSocketAddress( "127.0.0.1", 1337 ), null, "/db/data/transaction/45/commit" ) ).withUsername( "username" );

			  // when
			  string connectionDetails = clientConnection.AsConnectionDetails();

			  // then
			  assertEquals( "server-session\thttp\t127.0.0.1\t/db/data/transaction/45/commit\tusername", connectionDetails );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void connectionDetailsForEmbeddedQuerySource()
		 public virtual void ConnectionDetailsForEmbeddedQuerySource()
		 {
			  // when
			  string connectionDetails = ClientConnectionInfo.EMBEDDED_CONNECTION.asConnectionDetails();

			  // then
			  assertEquals( "embedded-session\t", connectionDetails );
		 }
	}

}