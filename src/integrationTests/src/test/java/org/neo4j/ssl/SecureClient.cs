using System;

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
namespace Neo4Net.Ssl
{
	using Bootstrap = io.netty.bootstrap.Bootstrap;
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using Channel = io.netty.channel.Channel;
	using ChannelFuture = io.netty.channel.ChannelFuture;
	using ChannelHandler = io.netty.channel.ChannelHandler;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChannelInboundHandlerAdapter = io.netty.channel.ChannelInboundHandlerAdapter;
	using ChannelInitializer = io.netty.channel.ChannelInitializer;
	using ChannelPipeline = io.netty.channel.ChannelPipeline;
	using SimpleChannelInboundHandler = io.netty.channel.SimpleChannelInboundHandler;
	using NioEventLoopGroup = io.netty.channel.nio.NioEventLoopGroup;
	using SocketChannel = io.netty.channel.socket.SocketChannel;
	using NioSocketChannel = io.netty.channel.socket.nio.NioSocketChannel;
	using SslContext = io.netty.handler.ssl.SslContext;
	using SslHandshakeCompletionEvent = io.netty.handler.ssl.SslHandshakeCompletionEvent;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.assertion.Assert.assertEventually;

	public class SecureClient
	{
		 private Bootstrap _bootstrap;
		 private NioEventLoopGroup _eventLoopGroup;
		 private Channel _channel;
		 private Bucket _bucket = new Bucket();

		 private string _protocol;
		 private string _ciphers;
		 private SslHandshakeCompletionEvent _handshakeEvent;
		 private CompletableFuture<Channel> _handshakeFuture = new CompletableFuture<Channel>();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public SecureClient(SslPolicy sslPolicy) throws javax.net.ssl.SSLException
		 public SecureClient( SslPolicy sslPolicy )
		 {
			  _eventLoopGroup = new NioEventLoopGroup();
			  _bootstrap = ( new Bootstrap() ).group(_eventLoopGroup).channel(typeof(NioSocketChannel)).handler(new ClientInitializer(this, sslPolicy, _bucket));
		 }

		 public virtual Future<Channel> SslHandshakeFuture()
		 {
			  return _handshakeFuture;
		 }

		 public virtual void Connect( int port )
		 {
			  ChannelFuture channelFuture = _bootstrap.connect( "localhost", port ).awaitUninterruptibly();
			  _channel = channelFuture.channel();
			  if ( !channelFuture.Success )
			  {
					throw new Exception( "Failed to connect", channelFuture.cause() );
			  }
		 }

		 internal virtual void Disconnect()
		 {
			  if ( _channel != null )
			  {
					_channel.close().awaitUninterruptibly();
					_eventLoopGroup.shutdownGracefully( 0, 0, SECONDS );
			  }

			  _bucket.collectedData.release();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void assertResponse(io.netty.buffer.ByteBuf expected) throws InterruptedException
		 internal virtual void AssertResponse( ByteBuf expected )
		 {
			  assertEventually( _channel.ToString(), () => _bucket.collectedData, equalTo(expected), 5, SECONDS );
		 }

		 internal virtual Channel Channel()
		 {
			  return _channel;
		 }

		 internal virtual string Ciphers()
		 {
			  if ( string.ReferenceEquals( _ciphers, null ) )
			  {
					throw new System.InvalidOperationException( "Handshake must have been completed" );
			  }
			  return _ciphers;
		 }

		 internal virtual string Protocol()
		 {
			  if ( string.ReferenceEquals( _protocol, null ) )
			  {
					throw new System.InvalidOperationException( "Handshake must have been completed" );
			  }
			  return _protocol;
		 }

		 internal class Bucket : SimpleChannelInboundHandler<ByteBuf>
		 {
			  internal readonly ByteBuf CollectedData;

			  internal Bucket()
			  {
					CollectedData = ByteBufAllocator.DEFAULT.buffer();
			  }

			  protected internal override void ChannelRead0( ChannelHandlerContext ctx, ByteBuf msg )
			  {
					CollectedData.writeBytes( msg );
			  }

			  public override void ExceptionCaught( ChannelHandlerContext ctx, Exception cause )
			  {
			  }
		 }

		 public class ClientInitializer : ChannelInitializer<SocketChannel>
		 {
			 private readonly SecureClient _outerInstance;

			  internal SslContext SslContext;
			  internal readonly Bucket Bucket;
			  internal readonly SslPolicy SslPolicy;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ClientInitializer(SslPolicy sslPolicy, Bucket bucket) throws javax.net.ssl.SSLException
			  internal ClientInitializer( SecureClient outerInstance, SslPolicy sslPolicy, Bucket bucket )
			  {
				  this._outerInstance = outerInstance;
					this.SslContext = sslPolicy.NettyClientContext();
					this.Bucket = bucket;
					this.SslPolicy = sslPolicy;
			  }

			  protected internal override void InitChannel( SocketChannel channel )
			  {
					ChannelPipeline pipeline = channel.pipeline();

					ChannelHandler clientOnConnectSslHandler = SslPolicy.nettyClientHandler( channel, SslContext );

					pipeline.addLast( clientOnConnectSslHandler );
					pipeline.addLast( new ChannelInboundHandlerAdapterAnonymousInnerClass( this ) );
					pipeline.addLast( Bucket );
			  }

			  private class ChannelInboundHandlerAdapterAnonymousInnerClass : ChannelInboundHandlerAdapter
			  {
				  private readonly ClientInitializer _outerInstance;

				  public ChannelInboundHandlerAdapterAnonymousInnerClass( ClientInitializer outerInstance )
				  {
					  this.outerInstance = outerInstance;
				  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void userEventTriggered(io.netty.channel.ChannelHandlerContext ctx, Object evt) throws Exception
				  public override void userEventTriggered( ChannelHandlerContext ctx, object evt )
				  {
						if ( evt is SslHandlerDetailsRegisteredEvent )
						{
							 SslHandlerDetailsRegisteredEvent sslHandlerDetailsRegisteredEvent = ( SslHandlerDetailsRegisteredEvent ) evt;
							 outerInstance.outerInstance.protocol = sslHandlerDetailsRegisteredEvent.Protocol;
							 outerInstance.outerInstance.ciphers = sslHandlerDetailsRegisteredEvent.CipherSuite;
							 outerInstance.outerInstance.handshakeFuture.complete( ctx.channel() ); // We complete the handshake here since it will also signify that the correct
							 // information has been carried
							 return;
						}
						if ( evt is SslHandshakeCompletionEvent )
						{
							 outerInstance.outerInstance.handshakeEvent = ( SslHandshakeCompletionEvent ) evt;
							 if ( outerInstance.outerInstance.handshakeEvent.cause() != null )
							 {
								  outerInstance.outerInstance.handshakeFuture.completeExceptionally( outerInstance.outerInstance.handshakeEvent.cause() );
							 }
							 // We do not complete if no error, that will be handled by the funky SslHandlerReplacedEvent
						}
				  }
			  }
		 }
	}

}