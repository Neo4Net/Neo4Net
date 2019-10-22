﻿using System;

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
namespace Neo4Net.Bolt.transport.pipeline
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using SimpleChannelInboundHandler = io.netty.channel.SimpleChannelInboundHandler;

	using BoltRequestMessageReader = Neo4Net.Bolt.messaging.BoltRequestMessageReader;
	using Neo4NetPack = Neo4Net.Bolt.messaging.Neo4NetPack;
	using UnpackerProvider = Neo4Net.Bolt.messaging.UnpackerProvider;
	using ByteBufInput = Neo4Net.Bolt.v1.packstream.ByteBufInput;
	using Log = Neo4Net.Logging.Log;
	using LogService = Neo4Net.Logging.Internal.LogService;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static io.netty.buffer.ByteBufUtil.hexDump;

	public class MessageDecoder : SimpleChannelInboundHandler<ByteBuf>
	{
		 private readonly ByteBufInput _input;
		 private readonly Neo4Net.Bolt.messaging.Neo4NetPack_Unpacker _unpacker;
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