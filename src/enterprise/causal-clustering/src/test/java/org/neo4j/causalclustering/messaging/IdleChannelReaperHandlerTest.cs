/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.messaging
{
	using Channel = io.netty.channel.Channel;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using IdleStateEvent = io.netty.handler.timeout.IdleStateEvent;
	using Test = org.junit.Test;

	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class IdleChannelReaperHandlerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveChannelViaCallback()
		 public virtual void ShouldRemoveChannelViaCallback()
		 {
			  // given
			  AdvertisedSocketAddress address = new AdvertisedSocketAddress( "localhost", 1984 );
			  ReconnectingChannels channels = new ReconnectingChannels();
			  channels.PutIfAbsent( address, mock( typeof( ReconnectingChannel ) ) );

			  IdleChannelReaperHandler reaper = new IdleChannelReaperHandler( channels );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.net.InetSocketAddress socketAddress = address.socketAddress();
			  InetSocketAddress socketAddress = address.SocketAddressConflict();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final io.netty.channel.Channel channel = mock(io.netty.channel.Channel.class);
			  Channel channel = mock( typeof( Channel ) );
			  when( channel.remoteAddress() ).thenReturn(socketAddress);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final io.netty.channel.ChannelHandlerContext context = mock(io.netty.channel.ChannelHandlerContext.class);
			  ChannelHandlerContext context = mock( typeof( ChannelHandlerContext ) );
			  when( context.channel() ).thenReturn(channel);

			  // when
			  reaper.UserEventTriggered( context, IdleStateEvent.ALL_IDLE_STATE_EVENT );

			  // then
			  assertNull( channels.Get( address ) );
		 }
	}

}