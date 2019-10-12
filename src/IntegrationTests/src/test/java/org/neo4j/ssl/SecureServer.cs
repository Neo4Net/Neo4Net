using System;

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
namespace Neo4Net.Ssl
{
	using ServerBootstrap = io.netty.bootstrap.ServerBootstrap;
	using ByteBuf = io.netty.buffer.ByteBuf;
	using Channel = io.netty.channel.Channel;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChannelInitializer = io.netty.channel.ChannelInitializer;
	using ChannelOption = io.netty.channel.ChannelOption;
	using ChannelPipeline = io.netty.channel.ChannelPipeline;
	using SimpleChannelInboundHandler = io.netty.channel.SimpleChannelInboundHandler;
	using NioEventLoopGroup = io.netty.channel.nio.NioEventLoopGroup;
	using SocketChannel = io.netty.channel.socket.SocketChannel;
	using NioServerSocketChannel = io.netty.channel.socket.nio.NioServerSocketChannel;
	using SslContext = io.netty.handler.ssl.SslContext;
	using SslHandler = io.netty.handler.ssl.SslHandler;


	public class SecureServer
	{
		 public static readonly sbyte[] Response = new sbyte[] { 5, 6, 7, 8 };

		 private SslContext _sslContext;
		 private Channel _channel;
		 private NioEventLoopGroup _eventLoopGroup;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public SecureServer(SslPolicy sslPolicy) throws javax.net.ssl.SSLException
		 public SecureServer( SslPolicy sslPolicy )
		 {
			  this._sslContext = sslPolicy.NettyServerContext();
		 }

		 public virtual void Start()
		 {
			  _eventLoopGroup = new NioEventLoopGroup();
			  ServerBootstrap bootstrap = ( new ServerBootstrap() ).group(_eventLoopGroup).channel(typeof(NioServerSocketChannel)).option(ChannelOption.SO_REUSEADDR, true).localAddress(0).childHandler(new ChannelInitializerAnonymousInnerClass(this));

			  _channel = bootstrap.bind().syncUninterruptibly().channel();
		 }

		 private class ChannelInitializerAnonymousInnerClass : ChannelInitializer<SocketChannel>
		 {
			 private readonly SecureServer _outerInstance;

			 public ChannelInitializerAnonymousInnerClass( SecureServer outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void initChannel(io.netty.channel.socket.SocketChannel ch) throws Exception
			 protected internal override void initChannel( SocketChannel ch )
			 {
				  ChannelPipeline pipeline = ch.pipeline();

				  SSLEngine sslEngine = _outerInstance.sslContext.newEngine( ch.alloc() );
				  sslEngine.NeedClientAuth = true;
				  SslHandler sslHandler = new SslHandler( sslEngine );
				  pipeline.addLast( sslHandler );

				  pipeline.addLast( new Responder() );
			 }
		 }

		 public virtual void Stop()
		 {
			  _channel.close().awaitUninterruptibly();
			  _channel = null;
			  _eventLoopGroup.shutdownGracefully( 0, 0, SECONDS );
		 }

		 public virtual int Port()
		 {
			  return ( ( InetSocketAddress ) _channel.localAddress() ).Port;
		 }

		 internal class Responder : SimpleChannelInboundHandler<ByteBuf>
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void channelRead0(io.netty.channel.ChannelHandlerContext ctx, io.netty.buffer.ByteBuf msg) throws Exception
			  protected internal override void ChannelRead0( ChannelHandlerContext ctx, ByteBuf msg )
			  {
					ctx.channel().writeAndFlush(ctx.alloc().buffer().writeBytes(Response));
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void exceptionCaught(io.netty.channel.ChannelHandlerContext ctx, Throwable cause) throws Exception
			  public override void ExceptionCaught( ChannelHandlerContext ctx, Exception cause )
			  {
					//cause.printStackTrace(); // for debugging
			  }
		 }
	}

}