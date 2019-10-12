using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.protocol.handshake
{
	using Bootstrap = io.netty.bootstrap.Bootstrap;
	using ServerBootstrap = io.netty.bootstrap.ServerBootstrap;
	using Channel = io.netty.channel.Channel;
	using ChannelFuture = io.netty.channel.ChannelFuture;
	using ChannelInitializer = io.netty.channel.ChannelInitializer;
	using ChannelOption = io.netty.channel.ChannelOption;
	using ChannelPipeline = io.netty.channel.ChannelPipeline;
	using NioEventLoopGroup = io.netty.channel.nio.NioEventLoopGroup;
	using SocketChannel = io.netty.channel.socket.SocketChannel;
	using NioServerSocketChannel = io.netty.channel.socket.nio.NioServerSocketChannel;
	using NioSocketChannel = io.netty.channel.socket.nio.NioSocketChannel;
	using LengthFieldBasedFrameDecoder = io.netty.handler.codec.LengthFieldBasedFrameDecoder;
	using LengthFieldPrepender = io.netty.handler.codec.LengthFieldPrepender;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using SimpleNettyChannel = Org.Neo4j.causalclustering.messaging.SimpleNettyChannel;
	using NullLog = Org.Neo4j.Logging.NullLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.empty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ApplicationProtocolCategory.CATCHUP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ApplicationProtocolCategory.RAFT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ModifierProtocolCategory.COMPRESSION;

	public class NettyProtocolHandshakeIT
	{
		private bool InstanceFieldsInitialized = false;

		public NettyProtocolHandshakeIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_raftApplicationProtocolRepository = new ApplicationProtocolRepository( TestProtocols_TestApplicationProtocols.values(), _supportedRaftApplicationProtocol );
			_catchupApplicationProtocolRepository = new ApplicationProtocolRepository( TestProtocols_TestApplicationProtocols.values(), _supportedCatchupApplicationProtocol );
			_compressionModifierProtocolRepository = new ModifierProtocolRepository( TestProtocols_TestModifierProtocols.values(), _supportedCompressionModifierProtocols );
			_unsupportingModifierProtocolRepository = new ModifierProtocolRepository( TestProtocols_TestModifierProtocols.values(), _noSupportedModifierProtocols );
		}

		 private ApplicationSupportedProtocols _supportedRaftApplicationProtocol = new ApplicationSupportedProtocols( RAFT, emptyList() );
		 private ApplicationSupportedProtocols _supportedCatchupApplicationProtocol = new ApplicationSupportedProtocols( CATCHUP, emptyList() );
		 private ICollection<ModifierSupportedProtocols> _supportedCompressionModifierProtocols = asList( new ModifierSupportedProtocols( COMPRESSION, TestProtocols_TestModifierProtocols.listVersionsOf( COMPRESSION ) ) );
		 private ICollection<ModifierSupportedProtocols> _noSupportedModifierProtocols = emptyList();

		 private ApplicationProtocolRepository _raftApplicationProtocolRepository;
		 private ApplicationProtocolRepository _catchupApplicationProtocolRepository;
		 private ModifierProtocolRepository _compressionModifierProtocolRepository;
		 private ModifierProtocolRepository _unsupportingModifierProtocolRepository;

		 private Server _server;
		 private HandshakeClient _handshakeClient;
		 private Client _client;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _server = new Server();
			  _server.start( _raftApplicationProtocolRepository, _compressionModifierProtocolRepository );

			  _handshakeClient = new HandshakeClient();

			  _client = new Client( _handshakeClient );
			  _client.connect( _server.port() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _client.disconnect();
			  _server.stop();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSuccessfullyHandshakeKnownProtocolOnClientWithCompression() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSuccessfullyHandshakeKnownProtocolOnClientWithCompression()
		 {
			  // when
			  CompletableFuture<ProtocolStack> clientHandshakeFuture = _handshakeClient.initiate( new SimpleNettyChannel( _client.channel, NullLog.Instance ), _raftApplicationProtocolRepository, _compressionModifierProtocolRepository );

			  // then
			  ProtocolStack clientProtocolStack = clientHandshakeFuture.get( 1, TimeUnit.MINUTES );
			  assertThat( clientProtocolStack.ApplicationProtocol(), equalTo(TestProtocols_TestApplicationProtocols.latest(RAFT)) );
			  assertThat( clientProtocolStack.ModifierProtocols(), contains(TestProtocols_TestModifierProtocols.latest(COMPRESSION)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSuccessfullyHandshakeKnownProtocolOnServerWithCompression() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSuccessfullyHandshakeKnownProtocolOnServerWithCompression()
		 {
			  // when
			  CompletableFuture<ProtocolStack> clientFuture = _handshakeClient.initiate( new SimpleNettyChannel( _client.channel, NullLog.Instance ), _raftApplicationProtocolRepository, _compressionModifierProtocolRepository );
			  CompletableFuture<ProtocolStack> serverHandshakeFuture = GetServerHandshakeFuture( clientFuture );

			  // then
			  ProtocolStack serverProtocolStack = serverHandshakeFuture.get( 1, TimeUnit.MINUTES );
			  assertThat( serverProtocolStack.ApplicationProtocol(), equalTo(TestProtocols_TestApplicationProtocols.latest(RAFT)) );
			  assertThat( serverProtocolStack.ModifierProtocols(), contains(TestProtocols_TestModifierProtocols.latest(COMPRESSION)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSuccessfullyHandshakeKnownProtocolOnClientNoModifiers() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSuccessfullyHandshakeKnownProtocolOnClientNoModifiers()
		 {
			  // when
			  CompletableFuture<ProtocolStack> clientHandshakeFuture = _handshakeClient.initiate( new SimpleNettyChannel( _client.channel, NullLog.Instance ), _raftApplicationProtocolRepository, _unsupportingModifierProtocolRepository );

			  // then
			  ProtocolStack clientProtocolStack = clientHandshakeFuture.get( 1, TimeUnit.MINUTES );
			  assertThat( clientProtocolStack.ApplicationProtocol(), equalTo(TestProtocols_TestApplicationProtocols.latest(RAFT)) );
			  assertThat( clientProtocolStack.ModifierProtocols(), empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSuccessfullyHandshakeKnownProtocolOnServerNoModifiers() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSuccessfullyHandshakeKnownProtocolOnServerNoModifiers()
		 {
			  // when
			  CompletableFuture<ProtocolStack> clientFuture = _handshakeClient.initiate( new SimpleNettyChannel( _client.channel, NullLog.Instance ), _raftApplicationProtocolRepository, _unsupportingModifierProtocolRepository );
			  CompletableFuture<ProtocolStack> serverHandshakeFuture = GetServerHandshakeFuture( clientFuture );

			  // then
			  ProtocolStack serverProtocolStack = serverHandshakeFuture.get( 1, TimeUnit.MINUTES );
			  assertThat( serverProtocolStack.ApplicationProtocol(), equalTo(TestProtocols_TestApplicationProtocols.latest(RAFT)) );
			  assertThat( serverProtocolStack.ModifierProtocols(), empty() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = ClientHandshakeException.class) public void shouldFailHandshakeForUnknownProtocolOnClient() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailHandshakeForUnknownProtocolOnClient()
		 {
			  // when
			  CompletableFuture<ProtocolStack> clientHandshakeFuture = _handshakeClient.initiate( new SimpleNettyChannel( _client.channel, NullLog.Instance ), _catchupApplicationProtocolRepository, _compressionModifierProtocolRepository );

			  // then
			  try
			  {
					clientHandshakeFuture.get( 1, TimeUnit.MINUTES );
			  }
			  catch ( ExecutionException ex )
			  {
					throw ex.InnerException;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = ServerHandshakeException.class) public void shouldFailHandshakeForUnknownProtocolOnServer() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailHandshakeForUnknownProtocolOnServer()
		 {
			  // when
			  CompletableFuture<ProtocolStack> clientFuture = _handshakeClient.initiate( new SimpleNettyChannel( _client.channel, NullLog.Instance ), _catchupApplicationProtocolRepository, _compressionModifierProtocolRepository );

			  CompletableFuture<ProtocolStack> serverHandshakeFuture = GetServerHandshakeFuture( clientFuture );

			  // then
			  try
			  {
					serverHandshakeFuture.get( 1, TimeUnit.MINUTES );
			  }
			  catch ( ExecutionException ex )
			  {
					throw ex.InnerException;
			  }
		 }

		 /// <summary>
		 /// Only attempt to access handshakeServer when client has completed, and do so whether client has completed normally or exceptionally
		 /// This is to avoid NullPointerException if handshakeServer accessed too soon
		 /// </summary>
		 private CompletableFuture<ProtocolStack> GetServerHandshakeFuture( CompletableFuture<ProtocolStack> clientFuture )
		 {
			  return clientFuture.handle( ( ignoreSuccess, ignoreFailure ) => null ).thenCompose( ignored => _server.handshakeServer.protocolStackFuture() );
		 }

		 private class Server
		 {
			  internal Channel Channel;
			  internal NioEventLoopGroup EventLoopGroup;
			  internal HandshakeServer HandshakeServer;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: void start(final ApplicationProtocolRepository applicationProtocolRepository, final ModifierProtocolRepository modifierProtocolRepository)
			  internal virtual void Start( ApplicationProtocolRepository applicationProtocolRepository, ModifierProtocolRepository modifierProtocolRepository )
			  {
					EventLoopGroup = new NioEventLoopGroup();
					ServerBootstrap bootstrap = ( new ServerBootstrap() ).group(EventLoopGroup).channel(typeof(NioServerSocketChannel)).option(ChannelOption.SO_REUSEADDR, true).localAddress(0).childHandler(new ChannelInitializerAnonymousInnerClass(this, applicationProtocolRepository, modifierProtocolRepository));

					Channel = bootstrap.bind().syncUninterruptibly().channel();
			  }

			  private class ChannelInitializerAnonymousInnerClass : ChannelInitializer<SocketChannel>
			  {
				  private readonly Server _outerInstance;

				  private Org.Neo4j.causalclustering.protocol.handshake.ApplicationProtocolRepository _applicationProtocolRepository;
				  private Org.Neo4j.causalclustering.protocol.handshake.ModifierProtocolRepository _modifierProtocolRepository;

				  public ChannelInitializerAnonymousInnerClass( Server outerInstance, Org.Neo4j.causalclustering.protocol.handshake.ApplicationProtocolRepository applicationProtocolRepository, Org.Neo4j.causalclustering.protocol.handshake.ModifierProtocolRepository modifierProtocolRepository )
				  {
					  this.outerInstance = outerInstance;
					  this._applicationProtocolRepository = applicationProtocolRepository;
					  this._modifierProtocolRepository = modifierProtocolRepository;
				  }

				  protected internal override void initChannel( SocketChannel ch )
				  {
						ChannelPipeline pipeline = ch.pipeline();
						_outerInstance.handshakeServer = new HandshakeServer( _applicationProtocolRepository, _modifierProtocolRepository, new SimpleNettyChannel( ch, NullLog.Instance ) );
						pipeline.addLast( "frameEncoder", new LengthFieldPrepender( 4 ) );
						pipeline.addLast( "frameDecoder", new LengthFieldBasedFrameDecoder( int.MaxValue, 0, 4, 0, 4 ) );
						pipeline.addLast( "responseMessageEncoder", new ServerMessageEncoder() );
						pipeline.addLast( "requestMessageDecoder", new ServerMessageDecoder() );
						pipeline.addLast( new NettyHandshakeServer( _outerInstance.handshakeServer ) );
				  }
			  }

			  internal virtual void Stop()
			  {
					Channel.close().awaitUninterruptibly();
					Channel = null;
					EventLoopGroup.shutdownGracefully( 0, 0, SECONDS );
			  }

			  internal virtual int Port()
			  {
					return ( ( InetSocketAddress ) Channel.localAddress() ).Port;
			  }
		 }

		 private class Client
		 {
			  internal Bootstrap Bootstrap;
			  internal NioEventLoopGroup EventLoopGroup;
			  internal Channel Channel;

			  internal Client( HandshakeClient handshakeClient )
			  {
					EventLoopGroup = new NioEventLoopGroup();
					Bootstrap = ( new Bootstrap() ).group(EventLoopGroup).channel(typeof(NioSocketChannel)).handler(new ClientInitializer(handshakeClient));
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("SameParameterValue") void connect(int port)
			  internal virtual void Connect( int port )
			  {
					ChannelFuture channelFuture = Bootstrap.connect( "localhost", port ).awaitUninterruptibly();
					Channel = channelFuture.channel();
			  }

			  internal virtual void Disconnect()
			  {
					if ( Channel != null )
					{
						 Channel.close().awaitUninterruptibly();
						 EventLoopGroup.shutdownGracefully( 0, 0, SECONDS );
					}
			  }
		 }

		 internal class ClientInitializer : ChannelInitializer<SocketChannel>
		 {
			  internal readonly HandshakeClient HandshakeClient;

			  internal ClientInitializer( HandshakeClient handshakeClient )
			  {
					this.HandshakeClient = handshakeClient;
			  }

			  protected internal override void InitChannel( SocketChannel channel )
			  {
					ChannelPipeline pipeline = channel.pipeline();
					pipeline.addLast( "frameEncoder", new LengthFieldPrepender( 4 ) );
					pipeline.addLast( "frameDecoder", new LengthFieldBasedFrameDecoder( int.MaxValue, 0, 4, 0, 4 ) );
					pipeline.addLast( "requestMessageEncoder", new ClientMessageEncoder() );
					pipeline.addLast( "responseMessageDecoder", new ClientMessageDecoder() );
					pipeline.addLast( new NettyHandshakeClient( HandshakeClient ) );
			  }
		 }
	}

}