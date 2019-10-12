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
namespace Neo4Net.causalclustering.messaging
{
	using Bootstrap = io.netty.bootstrap.Bootstrap;
	using ByteBuf = io.netty.buffer.ByteBuf;
	using ByteBufAllocator = io.netty.buffer.ByteBufAllocator;
	using ChannelHandler = io.netty.channel.ChannelHandler;
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChannelInboundHandlerAdapter = io.netty.channel.ChannelInboundHandlerAdapter;
	using ChannelInitializer = io.netty.channel.ChannelInitializer;
	using EventLoopGroup = io.netty.channel.EventLoopGroup;
	using NioEventLoopGroup = io.netty.channel.nio.NioEventLoopGroup;
	using SocketChannel = io.netty.channel.socket.SocketChannel;
	using NioSocketChannel = io.netty.channel.socket.nio.NioSocketChannel;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using Server = Neo4Net.causalclustering.net.Server;
	using ListenSocketAddress = Neo4Net.Helpers.ListenSocketAddress;
	using Log = Neo4Net.Logging.Log;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;

	public class ReconnectingChannelIT
	{
		private bool InstanceFieldsInitialized = false;

		public ReconnectingChannelIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_server = new Server(_channel => ;
		}

		 private static readonly int _port = PortAuthority.allocatePort();
		 private const long DEFAULT_TIMEOUT_MS = 20_000;
		 private readonly Log _log = NullLogProvider.Instance.getLog( this.GetType() );
		 private readonly ListenSocketAddress _listenAddress = new ListenSocketAddress( "localhost", _port );
		 private Server _server = new Server(_channel =>
		 {
		 }, _listenAddress, "test-server");
		 private EventLoopGroup _elg;
		 private ReconnectingChannel _channel;
		 private AtomicInteger _childCount = new AtomicInteger();
		 private readonly ChannelHandler childCounter = new ChannelInitializerAnonymousInnerClass();

		 private class ChannelInitializerAnonymousInnerClass : ChannelInitializer<SocketChannel>
		 {
			 protected internal override void initChannel( SocketChannel ch )
			 {
				  ch.pipeline().addLast(new ChannelInboundHandlerAdapterAnonymousInnerClass(this));
			 }

			 private class ChannelInboundHandlerAdapterAnonymousInnerClass : ChannelInboundHandlerAdapter
			 {
				 private readonly ChannelInitializerAnonymousInnerClass _outerInstance;

				 public ChannelInboundHandlerAdapterAnonymousInnerClass( ChannelInitializerAnonymousInnerClass outerInstance )
				 {
					 this.outerInstance = outerInstance;
				 }

				 public override void channelActive( ChannelHandlerContext ctx )
				 {
					  outerInstance.outerInstance.childCount.incrementAndGet();
				 }

				 public override void channelInactive( ChannelHandlerContext ctx )
				 {
					  outerInstance.outerInstance.childCount.decrementAndGet();
				 }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  _elg = new NioEventLoopGroup( 0 );
			  Bootstrap bootstrap = ( new Bootstrap() ).channel(typeof(NioSocketChannel)).group(_elg).handler(childCounter);
			  _channel = new ReconnectingChannel( bootstrap, _elg.next(), _listenAddress, _log );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void After()
		 {
			  _elg.shutdownGracefully( 0, DEFAULT_TIMEOUT_MS, MILLISECONDS ).awaitUninterruptibly();
			  _server.stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToSendMessage() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToSendMessage()
		 {
			  // given
			  _server.start();

			  // when
			  _channel.start();

			  // when
			  Future<Void> fSend = _channel.writeAndFlush( EmptyBuffer() );

			  // then will be successfully completed
			  fSend.get( DEFAULT_TIMEOUT_MS, TimeUnit.MILLISECONDS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowDeferredSend() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowDeferredSend()
		 {
			  // given
			  _channel.start();
			  _server.start();

			  // this is slightly racy, but generally we will send before the channel was connected
			  // this is benign in the sense that the test will pass in the condition where it was already connected as well

			  // when
			  Future<Void> fSend = _channel.writeAndFlush( EmptyBuffer() );

			  // then will be successfully completed
			  fSend.get( DEFAULT_TIMEOUT_MS, TimeUnit.MILLISECONDS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = java.util.concurrent.ExecutionException.class) public void shouldFailSendWhenNoServer() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailSendWhenNoServer()
		 {
			  // given
			  _channel.start();

			  // when
			  Future<Void> fSend = _channel.writeAndFlush( EmptyBuffer() );

			  // then will throw
			  fSend.get( DEFAULT_TIMEOUT_MS, TimeUnit.MILLISECONDS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReconnectAfterServerComesBack() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReconnectAfterServerComesBack()
		 {
			  // given
			  _server.start();
			  _channel.start();

			  // when
			  Future<Void> fSend = _channel.writeAndFlush( EmptyBuffer() );

			  // then will not throw
			  fSend.get( DEFAULT_TIMEOUT_MS, TimeUnit.MILLISECONDS );

			  // when
			  _server.stop();
			  fSend = _channel.writeAndFlush( EmptyBuffer() );

			  // then will throw
			  try
			  {
					fSend.get( DEFAULT_TIMEOUT_MS, TimeUnit.MILLISECONDS );
					fail( "Expected failure to send" );
			  }
			  catch ( ExecutionException )
			  {
					// pass
			  }

			  // when
			  _server.start();
			  fSend = _channel.writeAndFlush( EmptyBuffer() );

			  // then will not throw
			  fSend.get( DEFAULT_TIMEOUT_MS, TimeUnit.MILLISECONDS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowSendingOnDisposedChannel() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowSendingOnDisposedChannel()
		 {
			  // given
			  _server.start();
			  _channel.start();

			  // ensure we are connected
			  Future<Void> fSend = _channel.writeAndFlush( EmptyBuffer() );
			  fSend.get( DEFAULT_TIMEOUT_MS, TimeUnit.MILLISECONDS );
			  assertEventually( _childCount.get, equalTo( 1 ), DEFAULT_TIMEOUT_MS, MILLISECONDS );

			  // when
			  _channel.dispose();

			  try
			  {
					_channel.writeAndFlush( EmptyBuffer() );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// expected
			  }

			  // then
			  assertEventually( _childCount.get, equalTo( 0 ), DEFAULT_TIMEOUT_MS, MILLISECONDS );
		 }

		 private ByteBuf EmptyBuffer()
		 {
			  return ByteBufAllocator.DEFAULT.buffer();
		 }
	}

}