/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Org.Neo4j.causalclustering.messaging
{

	using ChannelDuplexHandler = io.netty.channel.ChannelDuplexHandler;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using IdleStateEvent = io.netty.handler.timeout.IdleStateEvent;

	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;

	public class IdleChannelReaperHandler : ChannelDuplexHandler
	{
		 private ReconnectingChannels _channels;

		 public IdleChannelReaperHandler( ReconnectingChannels channels )
		 {
			  this._channels = channels;
		 }

		 public override void UserEventTriggered( ChannelHandlerContext ctx, object evt )
		 {
			  if ( evt is IdleStateEvent && evt == IdleStateEvent.ALL_IDLE_STATE_EVENT )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.net.InetSocketAddress socketAddress = (java.net.InetSocketAddress) ctx.channel().remoteAddress();
					InetSocketAddress socketAddress = ( InetSocketAddress ) ctx.channel().remoteAddress();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.helpers.AdvertisedSocketAddress address = new org.neo4j.helpers.AdvertisedSocketAddress(socketAddress.getHostName(), socketAddress.getPort());
					AdvertisedSocketAddress address = new AdvertisedSocketAddress( socketAddress.HostName, socketAddress.Port );

					_channels.remove( address );
			  }
		 }
	}

}