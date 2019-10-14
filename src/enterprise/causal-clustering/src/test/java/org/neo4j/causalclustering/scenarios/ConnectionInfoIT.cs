using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.scenarios
{
	using After = org.junit.After;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Server = Neo4Net.causalclustering.net.Server;
	using ListenSocketAddress = Neo4Net.Helpers.ListenSocketAddress;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;
	using ClusterRule = Neo4Net.Test.causalclustering.ClusterRule;

	public class ConnectionInfoIT
	{
		 private Socket _testSocket;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.causalclustering.ClusterRule clusterRule = new org.neo4j.test.causalclustering.ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0);
		 public readonly ClusterRule ClusterRule = new ClusterRule().withNumberOfCoreMembers(3).withNumberOfReadReplicas(0);

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void teardown() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Teardown()
		 {
			  if ( _testSocket != null )
			  {
					Unbind( _testSocket );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testAddressAlreadyBoundMessage() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestAddressAlreadyBoundMessage()
		 {
			  // given
			  _testSocket = BindPort( "localhost", PortAuthority.allocatePort() );

			  // when
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  AssertableLogProvider userLogProvider = new AssertableLogProvider();
			  ListenSocketAddress listenSocketAddress = new ListenSocketAddress( "localhost", _testSocket.LocalPort );

			  Server catchupServer = new Server(channel =>
			  {
			  }, logProvider, userLogProvider, listenSocketAddress, "server-name");

			  //then
			  try
			  {
					catchupServer.Start();
			  }
			  catch ( Exception )
			  {
					//expected.
			  }
			  logProvider.FormattedMessageMatcher().assertContains("server-name: address is already bound: ");
			  userLogProvider.FormattedMessageMatcher().assertContains("server-name: address is already bound: ");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("SameParameterValue") private java.net.Socket bindPort(String address, int port) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private Socket BindPort( string address, int port )
		 {
			  Socket socket = new Socket();
			  socket.bind( new InetSocketAddress( address, port ) );
			  return socket;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void unbind(java.net.Socket socket) throws java.io.IOException
		 private void Unbind( Socket socket )
		 {
			  socket.close();
		 }
	}

}