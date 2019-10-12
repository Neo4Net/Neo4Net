using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

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
namespace Org.Neo4j.Ssl
{
	using Channel = io.netty.channel.Channel;
	using ChannelDuplexHandler = io.netty.channel.ChannelDuplexHandler;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChannelInboundHandlerAdapter = io.netty.channel.ChannelInboundHandlerAdapter;
	using ChannelPipeline = io.netty.channel.ChannelPipeline;
	using ChannelPromise = io.netty.channel.ChannelPromise;
	using SslContext = io.netty.handler.ssl.SslContext;
	using SslHandler = io.netty.handler.ssl.SslHandler;
	using SslHandshakeCompletionEvent = io.netty.handler.ssl.SslHandshakeCompletionEvent;


	public class ClientSideOnConnectSslHandler : ChannelDuplexHandler
	{
		 private readonly ChannelPipeline _pipeline;
		 private readonly SslContext _sslContext;
		 private readonly ICollection<System.Func<SSLEngine, SSLEngine>> _engineModifications;

		 internal ClientSideOnConnectSslHandler( Channel channel, SslContext sslContext, bool verifyHostname, string[] tlsVersions )
		 {
			  this._pipeline = channel.pipeline();
			  this._sslContext = sslContext;

			  this._engineModifications = new List<Function<SSLEngine, SSLEngine>>();
			  _engineModifications.Add( new EssentialEngineModifications( tlsVersions, true ) );
			  if ( verifyHostname )
			  {
					_engineModifications.Add( new ClientSideHostnameVerificationEngineModification() );
			  }
		 }

		 /// <summary>
		 /// Main event that is triggered for connections and swapping out SslHandler for this handler. channelActive and handlerAdded handlers are
		 /// secondary boundary cases to this.
		 /// </summary>
		 /// <param name="ctx"> Context of the existing channel </param>
		 /// <param name="remoteAddress"> the address used for initating a connection to a remote host (has type InetSocketAddress) </param>
		 /// <param name="localAddress"> the local address that will be used for receiving responses from the remote host </param>
		 /// <param name="promise"> the Channel promise to notify once the operation completes </param>
		 /// <exception cref="Exception"> when there is an error of any sort </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void connect(io.netty.channel.ChannelHandlerContext ctx, java.net.SocketAddress remoteAddress, java.net.SocketAddress localAddress, io.netty.channel.ChannelPromise promise) throws Exception
		 public override void Connect( ChannelHandlerContext ctx, SocketAddress remoteAddress, SocketAddress localAddress, ChannelPromise promise )
		 {
			  SslHandler sslHandler = CreateSslHandler( ctx, ( InetSocketAddress ) remoteAddress );
			  ReplaceSelfWith( sslHandler );
			  ctx.connect( remoteAddress, localAddress, promise );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void handlerAdded(io.netty.channel.ChannelHandlerContext ctx) throws Exception
		 public override void HandlerAdded( ChannelHandlerContext ctx )
		 {
			  // Sometimes the connect event will have happened before adding, the channel will be active then
			  if ( ctx.channel().Active )
			  {
					SslHandler sslHandler = CreateSslHandler( ctx, ( InetSocketAddress ) ctx.channel().remoteAddress() );
					ReplaceSelfWith( sslHandler );
					sslHandler.handlerAdded( ctx );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(io.netty.channel.ChannelHandlerContext ctx, Object msg, io.netty.channel.ChannelPromise promise) throws Exception
		 public override void Write( ChannelHandlerContext ctx, object msg, ChannelPromise promise )
		 {
			  throw new Exception( Thread.CurrentThread.Name + " - This handler does not write" );
		 }

		 /// <summary>
		 /// Replaces this entry of handler in the netty pipeline with the provided SslHandler and maintains the handler name
		 /// </summary>
		 /// <param name="sslHandler"> configured netty handler that enables TLS </param>
		 private void ReplaceSelfWith( SslHandler sslHandler )
		 {
			  string myName = _pipeline.toMap().entrySet().Where(entry => this.Equals(entry.Value)).Select(DictionaryEntry.getKey).First().orElseThrow(() => new System.InvalidOperationException("This handler has no name"));
			  _pipeline.replace( this, myName, sslHandler );
			  _pipeline.addAfter( myName, "handshakeCompletionSslDetailsHandler", new HandshakeCompletionSslDetailsHandler( this ) );
		 }

		 private SslHandler CreateSslHandler( ChannelHandlerContext ctx, InetSocketAddress inetSocketAddress )
		 {
			  SSLEngine sslEngine = _sslContext.newEngine( ctx.alloc(), inetSocketAddress.HostName, inetSocketAddress.Port );
			  foreach ( System.Func<SSLEngine, SSLEngine> mod in _engineModifications )
			  {
					sslEngine = mod( sslEngine );
			  }
			  // Don't need to set tls versions since that is set up from the context
			  return new SslHandler( sslEngine );
		 }

		 /// <summary>
		 /// Ssl protocol details are negotiated after handshake is complete.
		 /// Some tests rely on having these ssl details available.
		 /// Having this adapter exposes those details to the tests.
		 /// </summary>
		 private class HandshakeCompletionSslDetailsHandler : ChannelInboundHandlerAdapter
		 {
			 private readonly ClientSideOnConnectSslHandler _outerInstance;

			 public HandshakeCompletionSslDetailsHandler( ClientSideOnConnectSslHandler outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void userEventTriggered(io.netty.channel.ChannelHandlerContext ctx, Object evt) throws Exception
			  public override void UserEventTriggered( ChannelHandlerContext ctx, object evt )
			  {
					if ( evt is SslHandshakeCompletionEvent )
					{
						 SslHandshakeCompletionEvent sslHandshakeEvent = ( SslHandshakeCompletionEvent ) evt;
						 if ( sslHandshakeEvent.cause() == null )
						 {
							  SslHandler sslHandler = ctx.pipeline().get(typeof(SslHandler));
							  string ciphers = sslHandler.engine().Session.CipherSuite;
							  string protocols = sslHandler.engine().Session.Protocol;

							  ctx.fireUserEventTriggered( new SslHandlerDetailsRegisteredEvent( ciphers, protocols ) );
						 }
					}
					ctx.fireUserEventTriggered( evt );
			  }
		 }
	}

}