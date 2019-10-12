using System;
using System.Collections.Generic;

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
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChannelPipeline = io.netty.channel.ChannelPipeline;
	using ByteToMessageDecoder = io.netty.handler.codec.ByteToMessageDecoder;
	using HttpObjectAggregator = io.netty.handler.codec.http.HttpObjectAggregator;
	using HttpServerCodec = io.netty.handler.codec.http.HttpServerCodec;
	using WebSocketFrameAggregator = io.netty.handler.codec.http.websocketx.WebSocketFrameAggregator;
	using WebSocketServerProtocolHandler = io.netty.handler.codec.http.websocketx.WebSocketServerProtocolHandler;
	using SslContext = io.netty.handler.ssl.SslContext;
	using SslHandler = io.netty.handler.ssl.SslHandler;

	using ProtocolHandshaker = Org.Neo4j.Bolt.transport.pipeline.ProtocolHandshaker;
	using WebSocketFrameTranslator = Org.Neo4j.Bolt.transport.pipeline.WebSocketFrameTranslator;
	using Exceptions = Org.Neo4j.Helpers.Exceptions;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.bolt.transport.pipeline.ProtocolHandshaker.BOLT_MAGIC_PREAMBLE;

	public class TransportSelectionHandler : ByteToMessageDecoder
	{
		 private const string WEBSOCKET_MAGIC = "GET ";
		 private const int MAX_WEBSOCKET_HANDSHAKE_SIZE = 65536;
		 private const int MAX_WEBSOCKET_FRAME_SIZE = 65536;

		 private readonly BoltChannel _boltChannel;
		 private readonly SslContext _sslCtx;
		 private readonly bool _encryptionRequired;
		 private readonly bool _isEncrypted;
		 private readonly LogProvider _logging;
		 private readonly BoltProtocolFactory _boltProtocolFactory;
		 private readonly Log _log;

		 internal TransportSelectionHandler( BoltChannel boltChannel, SslContext sslCtx, bool encryptionRequired, bool isEncrypted, LogProvider logging, BoltProtocolFactory boltProtocolFactory )
		 {
			  this._boltChannel = boltChannel;
			  this._sslCtx = sslCtx;
			  this._encryptionRequired = encryptionRequired;
			  this._isEncrypted = isEncrypted;
			  this._logging = logging;
			  this._boltProtocolFactory = boltProtocolFactory;
			  this._log = logging.GetLog( typeof( TransportSelectionHandler ) );
		 }

		 protected internal override void Decode( ChannelHandlerContext ctx, ByteBuf @in, IList<object> @out )
		 {
			  // Will use the first five bytes to detect a protocol.
			  if ( @in.readableBytes() < 5 )
			  {
					return;
			  }

			  if ( DetectSsl( @in ) )
			  {
					EnableSsl( ctx );
			  }
			  else if ( IsHttp( @in ) )
			  {
					SwitchToWebsocket( ctx );
			  }
			  else if ( IsBoltPreamble( @in ) )
			  {
					SwitchToSocket( ctx );
			  }
			  else
			  {
					// TODO: send a alert_message for a ssl connection to terminate the handshake
					@in.clear();
					ctx.close();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void exceptionCaught(io.netty.channel.ChannelHandlerContext ctx, Throwable cause) throws Exception
		 public override void ExceptionCaught( ChannelHandlerContext ctx, Exception cause )
		 {
			  try
			  {
					// Netty throws a NativeIoException on connection reset - directly importing that class
					// caused a host of linking errors, because it depends on JNI to work. Hence, we just
					// test on the message we know we'll get.
					if ( Exceptions.contains( cause, e => e.Message.contains( "Connection reset by peer" ) ) )
					{
						 _log.warn( "Fatal error occurred when initialising pipeline, " + "remote peer unexpectedly closed connection: %s", ctx.channel() );
					}
					else
					{
						 _log.error( "Fatal error occurred when initialising pipeline: " + ctx.channel(), cause );
					}
			  }
			  finally
			  {
					ctx.close();
			  }
		 }

		 private bool IsBoltPreamble( ByteBuf @in )
		 {
			  return @in.getInt( 0 ) == BOLT_MAGIC_PREAMBLE;
		 }

		 private bool DetectSsl( ByteBuf buf )
		 {
			  return _sslCtx != null && SslHandler.isEncrypted( buf );
		 }

		 private bool IsHttp( ByteBuf buf )
		 {
			  for ( int i = 0; i < WEBSOCKET_MAGIC.Length; ++i )
			  {
					if ( buf.getUnsignedByte( buf.readerIndex() + i ) != WEBSOCKET_MAGIC[i] )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 private void EnableSsl( ChannelHandlerContext ctx )
		 {
			  ChannelPipeline p = ctx.pipeline();
			  p.addLast( _sslCtx.newHandler( ctx.alloc() ) );
			  p.addLast( new TransportSelectionHandler( _boltChannel, null, _encryptionRequired, true, _logging, _boltProtocolFactory ) );
			  p.remove( this );
		 }

		 private void SwitchToSocket( ChannelHandlerContext ctx )
		 {
			  ChannelPipeline p = ctx.pipeline();
			  p.addLast( NewHandshaker() );
			  p.remove( this );
		 }

		 private void SwitchToWebsocket( ChannelHandlerContext ctx )
		 {
			  ChannelPipeline p = ctx.pipeline();
			  p.addLast( new HttpServerCodec(), new HttpObjectAggregator(MAX_WEBSOCKET_HANDSHAKE_SIZE), new WebSocketServerProtocolHandler("/", null, false, MAX_WEBSOCKET_FRAME_SIZE), new WebSocketFrameAggregator(MAX_WEBSOCKET_FRAME_SIZE), new WebSocketFrameTranslator(), NewHandshaker() );
			  p.remove( this );
		 }

		 private ProtocolHandshaker NewHandshaker()
		 {
			  return new ProtocolHandshaker( _boltProtocolFactory, _boltChannel, _logging, _encryptionRequired, _isEncrypted );
		 }
	}

}