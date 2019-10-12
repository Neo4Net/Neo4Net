using System;

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
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using SimpleChannelInboundHandler = io.netty.channel.SimpleChannelInboundHandler;

	using BoltRequestMessageReader = Org.Neo4j.Bolt.messaging.BoltRequestMessageReader;
	using Neo4jPack = Org.Neo4j.Bolt.messaging.Neo4jPack;
	using UnpackerProvider = Org.Neo4j.Bolt.messaging.UnpackerProvider;
	using ByteBufInput = Org.Neo4j.Bolt.v1.packstream.ByteBufInput;
	using Log = Org.Neo4j.Logging.Log;
	using LogService = Org.Neo4j.Logging.@internal.LogService;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static io.netty.buffer.ByteBufUtil.hexDump;

	public class MessageDecoder : SimpleChannelInboundHandler<ByteBuf>
	{
		 private readonly ByteBufInput _input;
		 private readonly Org.Neo4j.Bolt.messaging.Neo4jPack_Unpacker _unpacker;
		 private readonly BoltRequestMessageReader _reader;
		 private readonly Log _log;

		 public MessageDecoder( UnpackerProvider unpackProvider, BoltRequestMessageReader reader, LogService logService )
		 {
			  this._input = new ByteBufInput();
			  this._unpacker = unpackProvider.NewUnpacker( _input );
			  this._reader = reader;
			  this._log = logService.GetInternalLog( this.GetType() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void channelRead0(io.netty.channel.ChannelHandlerContext channelHandlerContext, io.netty.buffer.ByteBuf byteBuf) throws Exception
		 protected internal override void ChannelRead0( ChannelHandlerContext channelHandlerContext, ByteBuf byteBuf )
		 {
			  _input.start( byteBuf );
			  byteBuf.markReaderIndex();
			  try
			  {
					_reader.read( _unpacker );
			  }
			  catch ( Exception error )
			  {
					LogMessageOnError( byteBuf );
					throw error;
			  }
			  finally
			  {
					_input.stop();
			  }
		 }

		 private void LogMessageOnError( ByteBuf byteBuf )
		 {
			  // move reader index back to the beginning of the message in order to log its full content
			  byteBuf.resetReaderIndex();
			  _log.error( "Failed to read an inbound message:\n" + hexDump( byteBuf ) + '\n' );
		 }
	}

}