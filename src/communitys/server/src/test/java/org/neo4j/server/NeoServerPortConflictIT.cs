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
	using Test = org.junit.Test;


	using ListenSocketAddress = Neo4Net.Helpers.ListenSocketAddress;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using CommunityServerBuilder = Neo4Net.Server.helpers.CommunityServerBuilder;
	using ExclusiveServerTestBase = Neo4Net.Test.server.ExclusiveServerTestBase;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class NeoServerPortConflictIT : ExclusiveServerTestBase
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldComplainIfServerPortIsAlreadyTaken() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldComplainIfServerPortIsAlreadyTaken()
		 {
			  using ( ServerSocket socket = new ServerSocket( 0, 0, InetAddress.LocalHost ) )
			  {
					ListenSocketAddress contestedAddress = new ListenSocketAddress( socket.InetAddress.HostName, socket.LocalPort );
					AssertableLogProvider logProvider = new AssertableLogProvider();
					CommunityNeoServer server = CommunityServerBuilder.server( logProvider ).onAddress( contestedAddress ).usingDataDir( Folder.directory( Name.MethodName ).AbsolutePath ).build();
					try
					{
						 server.Start();

						 fail( "Should have reported failure to start" );
					}
					catch ( ServerStartupException e )
					{
						 assertThat( e.Message, containsString( "Starting Neo4j failed" ) );
					}

					logProvider.AssertAtLeastOnce( AssertableLogProvider.inLog( containsString( "CommunityNeoServer" ) ).error( "Failed to start Neo4j on %s: %s", contestedAddress, format( "Address %s is already in use, cannot bind to it.", contestedAddress ) ) );
					server.Stop();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldComplainIfServerHTTPSPortIsAlreadyTaken() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldComplainIfServerHTTPSPortIsAlreadyTaken()
		 {
			  using ( ServerSocket httpsSocket = new ServerSocket( 0, 0, InetAddress.LocalHost ) )
			  {
					ListenSocketAddress unContestedAddress = new ListenSocketAddress( httpsSocket.InetAddress.HostName, 0 );
					ListenSocketAddress httpsAddress = new ListenSocketAddress( httpsSocket.InetAddress.HostName, httpsSocket.LocalPort );
					AssertableLogProvider logProvider = new AssertableLogProvider();
					CommunityNeoServer server = CommunityServerBuilder.server( logProvider ).onAddress( unContestedAddress ).onHttpsAddress( httpsAddress ).withHttpsEnabled().usingDataDir(Folder.directory(Name.MethodName).AbsolutePath).build();
					try
					{
						 server.Start();

						 fail( "Should have reported failure to start" );
					}
					catch ( ServerStartupException e )
					{
						 assertThat( e.Message, containsString( "Starting Neo4j failed" ) );
					}

					logProvider.AssertAtLeastOnce( AssertableLogProvider.inLog( containsString( "CommunityNeoServer" ) ).error( "Failed to start Neo4j on %s: %s", unContestedAddress, format( "At least one of the addresses %s or %s is already in use, cannot bind to it.", unContestedAddress, httpsAddress ) ) );
					server.Stop();
			  }
		 }
	}

}