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
namespace Org.Neo4j.Bolt.transport
{
	using EmbeddedChannel = io.netty.channel.embedded.EmbeddedChannel;
	using Test = org.junit.jupiter.api.Test;
	using ArgumentCaptor = org.mockito.ArgumentCaptor;

	using ListenSocketAddress = Org.Neo4j.Helpers.ListenSocketAddress;
	using NetworkConnectionTracker = Org.Neo4j.Kernel.api.net.NetworkConnectionTracker;
	using TrackedNetworkConnection = Org.Neo4j.Kernel.api.net.TrackedNetworkConnection;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.transport.TransportThrottleGroup.NO_THROTTLE;

	internal class SocketTransportTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldManageChannelsInChannelInitializer()
		 internal virtual void ShouldManageChannelsInChannelInitializer()
		 {
			  NetworkConnectionTracker connectionTracker = mock( typeof( NetworkConnectionTracker ) );
			  SocketTransport socketTransport = NewSocketTransport( connectionTracker, NO_THROTTLE );

			  EmbeddedChannel channel = new EmbeddedChannel( socketTransport.ChannelInitializer() );

			  ArgumentCaptor<TrackedNetworkConnection> captor = ArgumentCaptor.forClass( typeof( TrackedNetworkConnection ) );
			  verify( connectionTracker ).add( captor.capture() );
			  verify( connectionTracker, never() ).remove(any());

			  channel.close();

			  verify( connectionTracker ).remove( captor.Value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldManageThrottlersInChannelInitializer()
		 internal virtual void ShouldManageThrottlersInChannelInitializer()
		 {
			  TransportThrottleGroup throttleGroup = mock( typeof( TransportThrottleGroup ) );
			  SocketTransport socketTransport = NewSocketTransport( NetworkConnectionTracker.NO_OP, throttleGroup );

			  EmbeddedChannel channel = new EmbeddedChannel( socketTransport.ChannelInitializer() );

			  verify( throttleGroup ).install( channel );
			  verify( throttleGroup, never() ).uninstall(channel);

			  channel.close();

			  verify( throttleGroup ).uninstall( channel );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldInstallTransportSelectionHandler()
		 internal virtual void ShouldInstallTransportSelectionHandler()
		 {
			  SocketTransport socketTransport = NewSocketTransport( NetworkConnectionTracker.NO_OP, NO_THROTTLE );

			  EmbeddedChannel channel = new EmbeddedChannel( socketTransport.ChannelInitializer() );

			  TransportSelectionHandler handler = channel.pipeline().get(typeof(TransportSelectionHandler));
			  assertNotNull( handler );
		 }

		 private static SocketTransport NewSocketTransport( NetworkConnectionTracker connectionTracker, TransportThrottleGroup throttleGroup )
		 {
			  return new SocketTransport( "bolt", new ListenSocketAddress( "localhost", 7687 ), null, false, NullLogProvider.Instance, throttleGroup, mock( typeof( BoltProtocolFactory ) ), connectionTracker );
		 }
	}

}