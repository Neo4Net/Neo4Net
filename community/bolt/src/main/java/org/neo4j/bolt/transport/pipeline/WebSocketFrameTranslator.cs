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
namespace Org.Neo4j.Bolt.transport.pipeline
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ChannelDuplexHandler = io.netty.channel.ChannelDuplexHandler;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChannelPromise = io.netty.channel.ChannelPromise;
	using BinaryWebSocketFrame = io.netty.handler.codec.http.websocketx.BinaryWebSocketFrame;

	/// <summary>
	/// Translates websocket frames to bytebufs, and bytebufs to frames. Intermediary layer between our binary protocol
	/// and nettys built-in websocket handlers.
	/// </summary>
	public class WebSocketFrameTranslator : ChannelDuplexHandler
	{
		 public override void ChannelRead( ChannelHandlerContext ctx, object msg )
		 {
			  if ( msg is BinaryWebSocketFrame )
			  {
					ctx.fireChannelRead( ( ( BinaryWebSocketFrame ) msg ).content() );
			  }
			  else
			  {
					ctx.fireChannelRead( msg );
			  }
		 }

		 public override void Write( ChannelHandlerContext ctx, object msg, ChannelPromise promise )
		 {
			  if ( msg is ByteBuf )
			  {
					ctx.write( new BinaryWebSocketFrame( ( ByteBuf ) msg ), promise );
			  }
			  else
			  {
					ctx.write( msg, promise );
			  }
		 }
	}

}