using System;

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
	using Bootstrap = io.netty.bootstrap.Bootstrap;
	using Channel = io.netty.channel.Channel;
	using ChannelFuture = io.netty.channel.ChannelFuture;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChannelInboundHandler = io.netty.channel.ChannelInboundHandler;
	using ChannelInitializer = io.netty.channel.ChannelInitializer;
	using EmbeddedChannel = io.netty.channel.embedded.EmbeddedChannel;
	using NioEventLoopGroup = io.netty.channel.nio.NioEventLoopGroup;
	using SocketChannel = io.netty.channel.socket.SocketChannel;
	using NioSocketChannel = io.netty.channel.socket.nio.NioSocketChannel;
	using EventExecutor = io.netty.util.concurrent.EventExecutor;
	using After = org.junit.After;
	using Test = org.junit.Test;


	using BoltConnection = Neo4Net.Bolt.runtime.BoltConnection;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using NullLog = Neo4Net.Logging.NullLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static io.netty.buffer.ByteBufUtil.writeUtf8;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.inLog;

	public class HouseKeeperTest
	{
		 private EmbeddedChannel _channel;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void cleanup()
		 public virtual void Cleanup()
		 {
			  if ( _channel != null )
			  {
					_channel.finishAndReleaseAll();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStopConnectionOnChannelInactive()
		 public virtual void ShouldStopConnectionOnChannelInactive()
		 {
			  BoltConnection connection = mock( typeof( BoltConnection ) );
			  _channel = new EmbeddedChannel( new HouseKeeper( connection, NullLog.Instance ) );

			  _channel.pipeline().fireChannelInactive();

			  verify( connection ).stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotPropagateChannelInactive() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotPropagateChannelInactive()
		 {
			  ChannelInboundHandler next = mock( typeof( ChannelInboundHandler ) );
			  BoltConnection connection = mock( typeof( BoltConnection ) );
			  _channel = new EmbeddedChannel( new HouseKeeper( connection, NullLog.Instance ), next );

			  _channel.pipeline().fireChannelInactive();

			  verify( next, never() ).channelInactive(any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStopConnectionOnExceptionCaught()
		 public virtual void ShouldStopConnectionOnExceptionCaught()
		 {
			  BoltConnection connection = mock( typeof( BoltConnection ) );
			  _channel = new EmbeddedChannel( new HouseKeeper( connection, NullLog.Instance ) );

			  _channel.pipeline().fireExceptionCaught(new Exception("some exception"));

			  verify( connection ).stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogExceptionOnExceptionCaught()
		 public virtual void ShouldLogExceptionOnExceptionCaught()
		 {
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  BoltConnection connection = mock( typeof( BoltConnection ) );
			  _channel = new EmbeddedChannel( new HouseKeeper( connection, logProvider.GetLog( typeof( HouseKeeper ) ) ) );

			  Exception exception = new Exception( "some exception" );
			  _channel.pipeline().fireExceptionCaught(exception);

			  verify( connection ).stop();
			  logProvider.AssertExactly( inLog( typeof( HouseKeeper ) ).error( startsWith( "Fatal error occurred when handling a client connection" ), equalTo( exception ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotPropagateExceptionCaught() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotPropagateExceptionCaught()
		 {
			  ChannelInboundHandler next = mock( typeof( ChannelInboundHandler ) );
			  BoltConnection connection = mock( typeof( BoltConnection ) );
			  _channel = new EmbeddedChannel( new HouseKeeper( connection, NullLog.Instance ), next );

			  _channel.pipeline().fireExceptionCaught(new Exception("some exception"));

			  verify( next, never() ).exceptionCaught(any(), any());
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLogExceptionsWhenEvenLoopIsShuttingDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotLogExceptionsWhenEvenLoopIsShuttingDown()
		 {
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  BoltConnection connection = mock( typeof( BoltConnection ) );
			  HouseKeeper houseKeeper = new HouseKeeper( connection, logProvider.GetLog( typeof( HouseKeeper ) ) );
			  Bootstrap bootstrap = NewBootstrap( houseKeeper );

			  try
			  {
					  using ( ServerSocket serverSocket = new ServerSocket( 0 ) )
					  {
						ChannelFuture future = bootstrap.connect( "localhost", serverSocket.LocalPort ).sync();
						Channel channel = future.channel();
      
						// write some messages without flushing
						for ( int i = 0; i < 100; i++ )
						{
							 // use void promise which should redirect all write errors back to the pipeline and the HouseKeeper
							 channel.write( writeUtf8( channel.alloc(), "Hello" ), channel.voidPromise() );
						}
      
						// stop the even loop to make all pending writes fail
						bootstrap.config().group().shutdownGracefully();
						// await for the channel to be closed by the HouseKeeper
						channel.closeFuture().sync();
					  }
			  }
			  finally
			  {
					// make sure event loop group is always terminated
					bootstrap.config().group().shutdownGracefully().sync();
			  }

			  logProvider.AssertNoLoggingOccurred();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogOnlyTheFirstCaughtException() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLogOnlyTheFirstCaughtException()
		 {
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  BoltConnection connection = mock( typeof( BoltConnection ) );
			  HouseKeeper houseKeeper = new HouseKeeper( connection, logProvider.GetLog( typeof( HouseKeeper ) ) );
			  Bootstrap bootstrap = NewBootstrap( houseKeeper );

			  Exception error1 = new Exception( "error #1" );
			  Exception error2 = new Exception( "error #2" );
			  Exception error3 = new Exception( "error #3" );

			  try
			  {
					  using ( ServerSocket serverSocket = new ServerSocket( 0 ) )
					  {
						ChannelFuture future = bootstrap.connect( "localhost", serverSocket.LocalPort ).sync();
						Channel channel = future.channel();
      
						// fire multiple errors
						channel.pipeline().fireExceptionCaught(error1);
						channel.pipeline().fireExceptionCaught(error2);
						channel.pipeline().fireExceptionCaught(error3);
      
						// await for the channel to be closed by the HouseKeeper
						channel.closeFuture().sync();
					  }
			  }
			  finally
			  {
					// make sure event loop group is always terminated
					bootstrap.config().group().shutdownGracefully().sync();
			  }

			  logProvider.AssertExactly( inLog( typeof( HouseKeeper ) ).error( startsWith( "Fatal error occurred when handling a client connection" ), equalTo( error1 ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLogConnectionResetErrors() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotLogConnectionResetErrors()
		 {
			  // Given
			  AssertableLogProvider logProvider = new AssertableLogProvider();
			  HouseKeeper keeper = new HouseKeeper( null, logProvider.GetLog( typeof( HouseKeeper ) ) );
			  Channel channel = mock( typeof( Channel ) );
			  when( channel.ToString() ).thenReturn("[some channel info]");
			  ChannelHandlerContext ctx = mock( typeof( ChannelHandlerContext ) );
			  when( ctx.channel() ).thenReturn(channel);
			  when( ctx.executor() ).thenReturn(mock(typeof(EventExecutor)));
			  IOException connResetError = new IOException( "Connection reset by peer" );

			  // When
			  keeper.ExceptionCaught( ctx, connResetError );

			  // Then
			  logProvider.AssertExactly( AssertableLogProvider.inLog( typeof( HouseKeeper ) ).warn( "Fatal error occurred when handling a client connection, " + "remote peer unexpectedly closed connection: %s", channel ) );
		 }

		 private static Bootstrap NewBootstrap( HouseKeeper houseKeeper )
		 {
			  return ( new Bootstrap() ).group(new NioEventLoopGroup(1)).channel(typeof(NioSocketChannel)).handler(new ChannelInitializerAnonymousInnerClass(houseKeeper));
		 }

		 private class ChannelInitializerAnonymousInnerClass : ChannelInitializer<SocketChannel>
		 {
			 private Neo4Net.Bolt.transport.pipeline.HouseKeeper _houseKeeper;

			 public ChannelInitializerAnonymousInnerClass( Neo4Net.Bolt.transport.pipeline.HouseKeeper houseKeeper )
			 {
				 this._houseKeeper = houseKeeper;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void initChannel(io.netty.channel.socket.SocketChannel ch) throws Exception
			 protected internal override void initChannel( SocketChannel ch )
			 {
				  ch.pipeline().addLast(_houseKeeper);
			 }
		 }
	}

}