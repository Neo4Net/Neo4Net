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
namespace Neo4Net.Bolt.transport.pipeline
{
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ChannelFutureListener = io.netty.channel.ChannelFutureListener;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChannelInboundHandlerAdapter = io.netty.channel.ChannelInboundHandlerAdapter;
	using ReferenceCountUtil = io.netty.util.ReferenceCountUtil;

	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	public class ProtocolHandshaker : ChannelInboundHandlerAdapter
	{
		 public const int BOLT_MAGIC_PREAMBLE = 0x6060B017;
		 private static readonly int _handshakeBufferSize = 5 * Integer.BYTES;

		 private readonly BoltChannel _boltChannel;
		 private readonly BoltProtocolFactory _boltProtocolFactory;
		 private readonly Log _log;
		 private readonly bool _encryptionRequired;
		 private readonly bool _encrypted;

		 private ByteBuf _handshakeBuffer;
		 private BoltProtocol _protocol;

		 public ProtocolHandshaker( BoltProtocolFactory boltProtocolFactory, BoltChannel boltChannel, LogProvider logging, bool encryptionRequired, bool encrypted )
		 {
			  this._boltProtocolFactory = boltProtocolFactory;
			  this._boltChannel = boltChannel;
			  this._log = logging.getLog( this.GetType() );
			  this._encryptionRequired = encryptionRequired;
			  this._encrypted = encrypted;
		 }

		 public override void HandlerAdded( ChannelHandlerContext ctx )
		 {
			  _handshakeBuffer = ctx.alloc().buffer(_handshakeBufferSize, _handshakeBufferSize);
		 }

		 public override void HandlerRemoved( ChannelHandlerContext ctx )
		 {
			  _handshakeBuffer.release();
			  _handshakeBuffer = null;
		 }

		 public override void ChannelRead( ChannelHandlerContext ctx, object msg )
		 {
			  try
			  {
					if ( !( msg is ByteBuf ) )
					{
						 // we know it is HTTP as we only have HTTP (for Websocket) and TCP handlers installed.
						 _log.warn( "Unsupported connection type: 'HTTP'. Bolt protocol only operates over a TCP connection or WebSocket." );
						 ctx.close();
						 return;
					}
					ByteBuf buf = ( ByteBuf ) msg;

					AssertEncryptedIfRequired();

					// try to fill out handshake buffer
					_handshakeBuffer.writeBytes( buf, Math.Min( buf.readableBytes(), _handshakeBuffer.writableBytes() ) );

					// we filled up the handshake buffer
					if ( _handshakeBuffer.writableBytes() == 0 )
					{
						 if ( VerifyBoltPreamble() )
						 {
							  // let's handshake
							  if ( PerformHandshake() )
							  {
									// announce selected protocol to the client
									ctx.writeAndFlush( ctx.alloc().buffer(4).writeInt((int)_protocol.version()) );

									// install related protocol handlers into the pipeline
									_protocol.install();
									ctx.pipeline().remove(this);

									// if we somehow end up with more data in the incoming buffers, let's send them
									// down to the pipeline for the chosen protocol handlers to handle whatever they
									// are.
									if ( buf.readableBytes() > 0 )
									{
										 ctx.fireChannelRead( buf.readRetainedSlice( buf.readableBytes() ) );
									}
							  }
							  else
							  {
									ctx.writeAndFlush( ctx.alloc().buffer().writeBytes(new sbyte[]{ 0, 0, 0, 0 }) ).addListener(ChannelFutureListener.CLOSE);
							  }
						 }
						 else
						 {
							  ctx.close();
						 }
					}
			  }
			  finally
			  {
					ReferenceCountUtil.release( msg );
			  }
		 }

		 public override void ExceptionCaught( ChannelHandlerContext ctx, Exception cause )
		 {
			  _log.error( "Fatal error occurred during protocol handshaking: " + ctx.channel(), cause );
			  ctx.close();
		 }

		 public override void ChannelInactive( ChannelHandlerContext ctx )
		 {
			  ctx.close();
		 }

		 private void AssertEncryptedIfRequired()
		 {
			  if ( _encryptionRequired && !_encrypted )
			  {
					throw new SecurityException( "An unencrypted connection attempt was made where encryption is required." );
			  }
		 }

		 private bool VerifyBoltPreamble()
		 {
			  if ( _handshakeBuffer.getInt( 0 ) != BOLT_MAGIC_PREAMBLE )
			  {
					_log.debug( "Invalid Bolt handshake signature. Expected 0x%08X, but got: 0x%08X", BOLT_MAGIC_PREAMBLE, _handshakeBuffer.getInt( 0 ) );
					return false;
			  }

			  return true;
		 }

		 private bool PerformHandshake()
		 {
			  long[] suggestions = new long[4];
			  for ( int i = 0; i < 4; i++ )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long suggestion = handshakeBuffer.getInt((i + 1) * Integer.BYTES) & 0xFFFFFFFFL;
					long suggestion = _handshakeBuffer.getInt( ( i + 1 ) * Integer.BYTES ) & 0xFFFFFFFFL;

					_protocol = _boltProtocolFactory( suggestion, _boltChannel );
					if ( _protocol != null )
					{
						 break;
					}
					suggestions[i] = suggestion;
			  }

			  if ( _protocol == null )
			  {
					_log.debug( "Failed Bolt handshake: Bolt versions suggested by client '%s' are not supported by this server.", Arrays.ToString( suggestions ) );
			  }

			  return _protocol != null;
		 }
	}

}