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
namespace Org.Neo4j.causalclustering.catchup
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChannelInboundHandlerAdapter = io.netty.channel.ChannelInboundHandlerAdapter;
	using ReferenceCountUtil = io.netty.util.ReferenceCountUtil;

	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;


	public class ServerMessageTypeHandler : ChannelInboundHandlerAdapter
	{
		 private readonly Log _log;
		 private readonly CatchupServerProtocol _protocol;

		 public ServerMessageTypeHandler( CatchupServerProtocol protocol, LogProvider logProvider )
		 {
			  this._protocol = protocol;
			  this._log = logProvider.getLog( this.GetType() );
		 }

		 public override void ChannelRead( ChannelHandlerContext ctx, object msg )
		 {
			  if ( _protocol.isExpecting( CatchupServerProtocol.State.MessageType ) )
			  {
					RequestMessageType requestMessageType = RequestMessageType.from( ( ( ByteBuf ) msg ).readByte() );

					if ( requestMessageType.Equals( RequestMessageType.TxPullRequest ) )
					{
						 _protocol.expect( CatchupServerProtocol.State.TxPull );
					}
					else if ( requestMessageType.Equals( RequestMessageType.StoreId ) )
					{
						 _protocol.expect( CatchupServerProtocol.State.GetStoreId );
					}
					else if ( requestMessageType.Equals( RequestMessageType.CoreSnapshot ) )
					{
						 _protocol.expect( CatchupServerProtocol.State.GetCoreSnapshot );
					}
					else if ( requestMessageType.Equals( RequestMessageType.PrepareStoreCopy ) )
					{
						 _protocol.expect( CatchupServerProtocol.State.PrepareStoreCopy );
					}
					else if ( requestMessageType.Equals( RequestMessageType.StoreFile ) )
					{
						 _protocol.expect( CatchupServerProtocol.State.GetStoreFile );
					}
					else if ( requestMessageType.Equals( RequestMessageType.IndexSnapshot ) )
					{
						 _protocol.expect( CatchupServerProtocol.State.GetIndexSnapshot );
					}
					else
					{
						 _log.warn( "No handler found for message type %s", requestMessageType );
					}

					ReferenceCountUtil.release( msg );
			  }
			  else
			  {
					ctx.fireChannelRead( msg );
			  }
		 }
	}

}