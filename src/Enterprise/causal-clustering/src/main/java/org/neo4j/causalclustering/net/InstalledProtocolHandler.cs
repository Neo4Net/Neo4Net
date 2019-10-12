using System.Collections.Concurrent;

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
namespace Neo4Net.causalclustering.net
{
	using ChannelHandler = io.netty.channel.ChannelHandler;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChannelInboundHandlerAdapter = io.netty.channel.ChannelInboundHandlerAdapter;


	using ProtocolStack = Neo4Net.causalclustering.protocol.handshake.ProtocolStack;
	using ServerHandshakeFinishedEvent = Neo4Net.causalclustering.protocol.handshake.ServerHandshakeFinishedEvent;
	using SocketAddress = Neo4Net.Helpers.SocketAddress;
	using Neo4Net.Helpers.Collection;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ChannelHandler.Sharable public class InstalledProtocolHandler extends io.netty.channel.ChannelInboundHandlerAdapter
	public class InstalledProtocolHandler : ChannelInboundHandlerAdapter
	{
		 private ConcurrentMap<SocketAddress, ProtocolStack> _installedProtocols = new ConcurrentDictionary<SocketAddress, ProtocolStack>();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void userEventTriggered(io.netty.channel.ChannelHandlerContext ctx, Object evt) throws Exception
		 public override void UserEventTriggered( ChannelHandlerContext ctx, object evt )
		 {
			  if ( evt is Neo4Net.causalclustering.protocol.handshake.ServerHandshakeFinishedEvent_Created )
			  {
					Neo4Net.causalclustering.protocol.handshake.ServerHandshakeFinishedEvent_Created created = ( Neo4Net.causalclustering.protocol.handshake.ServerHandshakeFinishedEvent_Created ) evt;
					_installedProtocols.put( created.AdvertisedSocketAddress, created.ProtocolStack );
			  }
			  else if ( evt is Neo4Net.causalclustering.protocol.handshake.ServerHandshakeFinishedEvent_Closed )
			  {
					Neo4Net.causalclustering.protocol.handshake.ServerHandshakeFinishedEvent_Closed closed = ( Neo4Net.causalclustering.protocol.handshake.ServerHandshakeFinishedEvent_Closed ) evt;
					_installedProtocols.remove( closed.AdvertisedSocketAddress );
			  }
			  else
			  {
					base.UserEventTriggered( ctx, evt );
			  }
		 }

		 public virtual Stream<Pair<SocketAddress, ProtocolStack>> InstalledProtocols()
		 {
			  return _installedProtocols.entrySet().Select(entry => Pair.of(entry.Key, entry.Value));
		 }
	}

}