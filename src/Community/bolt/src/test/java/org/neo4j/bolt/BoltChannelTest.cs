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
namespace Neo4Net.Bolt
{
	using Channel = io.netty.channel.Channel;
	using ChannelFuture = io.netty.channel.ChannelFuture;
	using EmbeddedChannel = io.netty.channel.embedded.EmbeddedChannel;
	using Test = org.junit.jupiter.api.Test;

	using SocketAddress = Neo4Net.Helpers.SocketAddress;
	using ClientConnectionInfo = Neo4Net.Kernel.impl.query.clientconnection.ClientConnectionInfo;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class BoltChannelTest
	{
		 private readonly Channel _channel = mock( typeof( Channel ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCloseUnderlyingChannelWhenItIsOpen()
		 internal virtual void ShouldCloseUnderlyingChannelWhenItIsOpen()
		 {
			  Channel channel = ChannelMock( true );
			  BoltChannel boltChannel = new BoltChannel( "bolt-1", "bolt", channel );

			  boltChannel.Close();

			  verify( channel ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldNotCloseUnderlyingChannelWhenItIsClosed()
		 internal virtual void ShouldNotCloseUnderlyingChannelWhenItIsClosed()
		 {
			  Channel channel = ChannelMock( false );
			  BoltChannel boltChannel = new BoltChannel( "bolt-1", "bolt", channel );

			  boltChannel.Close();

			  verify( channel, never() ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHaveId()
		 internal virtual void ShouldHaveId()
		 {
			  BoltChannel boltChannel = new BoltChannel( "bolt-42", "bolt", _channel );

			  assertEquals( "bolt-42", boltChannel.Id() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHaveConnector()
		 internal virtual void ShouldHaveConnector()
		 {
			  BoltChannel boltChannel = new BoltChannel( "bolt-1", "my-bolt", _channel );

			  assertEquals( "my-bolt", boltChannel.Connector() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHaveConnectTime()
		 internal virtual void ShouldHaveConnectTime()
		 {
			  BoltChannel boltChannel = new BoltChannel( "bolt-1", "my-bolt", _channel );

			  assertThat( boltChannel.ConnectTime(), greaterThan(0L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHaveUsernameAndUserAgent()
		 internal virtual void ShouldHaveUsernameAndUserAgent()
		 {
			  BoltChannel boltChannel = new BoltChannel( "bolt-1", "my-bolt", _channel );

			  assertNull( boltChannel.Username() );
			  boltChannel.UpdateUser( "hello", "my-bolt-driver/1.2.3" );
			  assertEquals( "hello", boltChannel.Username() );
			  assertEquals( "my-bolt-driver/1.2.3", boltChannel.UserAgent() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @SuppressWarnings("deprecation") void shouldExposeClientConnectionInfo()
		 internal virtual void ShouldExposeClientConnectionInfo()
		 {
			  EmbeddedChannel channel = new EmbeddedChannel();
			  BoltChannel boltChannel = new BoltChannel( "bolt-42", "my-bolt", channel );

			  ClientConnectionInfo info1 = boltChannel.Info();
			  assertEquals( "bolt-42", info1.ConnectionId() );
			  assertEquals( "bolt", info1.Protocol() );
			  assertEquals( SocketAddress.format( channel.remoteAddress() ), info1.ClientAddress() );

			  boltChannel.UpdateUser( "Tom", "my-driver" );

			  ClientConnectionInfo info2 = boltChannel.Info();
			  assertEquals( "bolt-42", info2.ConnectionId() );
			  assertEquals( "bolt", info2.Protocol() );
			  assertEquals( SocketAddress.format( channel.remoteAddress() ), info2.ClientAddress() );
			  assertThat( info2.AsConnectionDetails(), containsString("Tom") );
			  assertThat( info2.AsConnectionDetails(), containsString("my-driver") );
		 }

		 private static Channel ChannelMock( bool open )
		 {
			  Channel channel = mock( typeof( Channel ) );
			  when( channel.Open ).thenReturn( open );
			  ChannelFuture channelFuture = mock( typeof( ChannelFuture ) );
			  when( channel.close() ).thenReturn(channelFuture);
			  return channel;
		 }
	}

}