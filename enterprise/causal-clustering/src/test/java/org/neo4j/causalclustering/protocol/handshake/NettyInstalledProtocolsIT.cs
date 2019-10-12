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
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChannelInboundHandler = io.netty.channel.ChannelInboundHandler;
	using ChannelOption = io.netty.channel.ChannelOption;
	using SimpleChannelInboundHandler = io.netty.channel.SimpleChannelInboundHandler;
	using NioEventLoopGroup = io.netty.channel.nio.NioEventLoopGroup;
	using NioServerSocketChannel = io.netty.channel.socket.nio.NioServerSocketChannel;
	using NioSocketChannel = io.netty.channel.socket.nio.NioSocketChannel;
	using BaseMatcher = org.hamcrest.BaseMatcher;
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using RaftMessages = Org.Neo4j.causalclustering.core.consensus.RaftMessages;
	using RaftProtocolClientInstallerV1 = Org.Neo4j.causalclustering.core.consensus.protocol.v1.RaftProtocolClientInstallerV1;
	using RaftProtocolServerInstallerV1 = Org.Neo4j.causalclustering.core.consensus.protocol.v1.RaftProtocolServerInstallerV1;
	using RaftProtocolClientInstallerV2 = Org.Neo4j.causalclustering.core.consensus.protocol.v2.RaftProtocolClientInstallerV2;
	using RaftProtocolServerInstallerV2 = Org.Neo4j.causalclustering.core.consensus.protocol.v2.RaftProtocolServerInstallerV2;
	using VoidPipelineWrapperFactory = Org.Neo4j.causalclustering.handlers.VoidPipelineWrapperFactory;
	using ClusterId = Org.Neo4j.causalclustering.identity.ClusterId;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using Org.Neo4j.causalclustering.protocol;
	using Org.Neo4j.causalclustering.protocol;
	using Org.Neo4j.causalclustering.protocol;
	using Org.Neo4j.causalclustering.protocol;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using AssertableLogProvider = Org.Neo4j.Logging.AssertableLogProvider;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using PortAuthority = Org.Neo4j.Ports.Allocation.PortAuthority;
	using Streams = Org.Neo4j.Stream.Streams;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ApplicationProtocolCategory.RAFT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ApplicationProtocols.RAFT_1;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ApplicationProtocols.RAFT_2;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.protocol.Protocol_ModifierProtocolCategory.COMPRESSION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.assertion.Assert.assertEventually;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class NettyInstalledProtocolsIT
	public class NettyInstalledProtocolsIT
	{
		 private const int TIMEOUT_SECONDS = 10;
		 private Parameters _parameters;
		 private AssertableLogProvider _logProvider;

		 public NettyInstalledProtocolsIT( Parameters parameters )
		 {
			  this._parameters = parameters;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.Collection<Parameters> data()
		 public static ICollection<Parameters> Data()
		 {
			  Stream<Optional<Protocol_ModifierProtocol>> noModifierProtocols = Stream.of( null );
			  Stream<Optional<Protocol_ModifierProtocol>> individualModifierProtocols = Stream.of( Protocol_ModifierProtocols.values() ).map(Optional.of);

			  return Stream.concat( noModifierProtocols, individualModifierProtocols ).flatMap( protocol => Stream.of( Raft1WithCompressionModifier( protocol ), Raft2WithCompressionModifiers( protocol ) ) ).collect( Collectors.toList() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("OptionalUsedAsFieldOrParameterType") private static Parameters raft1WithCompressionModifier(java.util.Optional<org.neo4j.causalclustering.protocol.Protocol_ModifierProtocol> protocol)
		 private static Parameters Raft1WithCompressionModifier( Optional<Protocol_ModifierProtocol> protocol )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IList<string> versions = Streams.ofOptional( protocol ).map( Protocol::implementation ).collect( Collectors.toList() );
			  return new Parameters( "Raft 1, modifiers: " + protocol, new ApplicationSupportedProtocols( RAFT, singletonList( RAFT_1.implementation() ) ), singletonList(new ModifierSupportedProtocols(COMPRESSION, versions)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("OptionalUsedAsFieldOrParameterType") private static Parameters raft2WithCompressionModifiers(java.util.Optional<org.neo4j.causalclustering.protocol.Protocol_ModifierProtocol> protocol)
		 private static Parameters Raft2WithCompressionModifiers( Optional<Protocol_ModifierProtocol> protocol )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  IList<string> versions = Streams.ofOptional( protocol ).map( Protocol::implementation ).collect( Collectors.toList() );
			  return new Parameters( "Raft 2, modifiers: " + protocol, new ApplicationSupportedProtocols( RAFT, singletonList( RAFT_2.implementation() ) ), singletonList(new ModifierSupportedProtocols(COMPRESSION, versions)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSuccessfullySendAndReceiveAMessage() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSuccessfullySendAndReceiveAMessage()
		 {
			  // given
			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_Heartbeat raftMessage = new Org.Neo4j.causalclustering.core.consensus.RaftMessages_Heartbeat( new MemberId( System.Guid.randomUUID() ), 1, 2, 3 );
			  Org.Neo4j.causalclustering.core.consensus.RaftMessages_ClusterIdAwareMessage<Org.Neo4j.causalclustering.core.consensus.RaftMessages_Heartbeat> networkMessage = Org.Neo4j.causalclustering.core.consensus.RaftMessages_ClusterIdAwareMessage.of( new ClusterId( System.Guid.randomUUID() ), raftMessage );

			  // when
			  _client.send( networkMessage ).syncUninterruptibly();

			  // then
			  assertEventually( messages => string.Format( "Received messages {0} should contain message decorating {1}", messages, raftMessage ), () => _server.received(), contains(MessageMatches(networkMessage)), TIMEOUT_SECONDS, SECONDS );
		 }

		 private Server _server;
		 private Client _client;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _logProvider = new AssertableLogProvider( true );
			  ApplicationProtocolRepository applicationProtocolRepository = new ApplicationProtocolRepository( Org.Neo4j.causalclustering.protocol.Protocol_ApplicationProtocols.values(), _parameters.applicationSupportedProtocol );
			  ModifierProtocolRepository modifierProtocolRepository = new ModifierProtocolRepository( Protocol_ModifierProtocols.values(), _parameters.modifierSupportedProtocols );

			  NettyPipelineBuilderFactory serverPipelineBuilderFactory = new NettyPipelineBuilderFactory( VoidPipelineWrapperFactory.VOID_WRAPPER );
			  NettyPipelineBuilderFactory clientPipelineBuilderFactory = new NettyPipelineBuilderFactory( VoidPipelineWrapperFactory.VOID_WRAPPER );

			  _server = new Server( serverPipelineBuilderFactory );
			  _server.start( applicationProtocolRepository, modifierProtocolRepository, _logProvider );

			  Config config = Config.builder().withSetting(CausalClusteringSettings.handshake_timeout, TIMEOUT_SECONDS + "s").build();

			  _client = new Client( applicationProtocolRepository, modifierProtocolRepository, clientPipelineBuilderFactory, config, _logProvider );

			  _client.connect( _server.port() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  _client.disconnect();
			  _server.stop();
			  _logProvider.clear();
		 }

		 private class Parameters
		 {
			  internal readonly string Name;
			  internal readonly ApplicationSupportedProtocols ApplicationSupportedProtocol;
			  internal readonly ICollection<ModifierSupportedProtocols> ModifierSupportedProtocols;

			  internal Parameters( string name, ApplicationSupportedProtocols applicationSupportedProtocol, ICollection<ModifierSupportedProtocols> modifierSupportedProtocols )
			  {
					this.Name = name;
					this.ApplicationSupportedProtocol = applicationSupportedProtocol;
					this.ModifierSupportedProtocols = modifierSupportedProtocols;
			  }

			  public override string ToString()
			  {
					return Name;
			  }
		 }

		 internal class Server
		 {
			  internal Channel Channel;
			  internal NioEventLoopGroup EventLoopGroup;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly IList<object> ReceivedConflict = new CopyOnWriteArrayList<object>();
			  internal NettyPipelineBuilderFactory PipelineBuilderFactory;

			  internal ChannelInboundHandler nettyHandler = new SimpleChannelInboundHandlerAnonymousInnerClass();

			  private class SimpleChannelInboundHandlerAnonymousInnerClass : SimpleChannelInboundHandler<object>
			  {
				  protected internal override void channelRead0( ChannelHandlerContext ctx, object msg )
				  {
						outerInstance.received.Add( msg );
				  }
			  }

			  internal Server( NettyPipelineBuilderFactory pipelineBuilderFactory )
			  {
					this.PipelineBuilderFactory = pipelineBuilderFactory;
			  }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: void start(final ApplicationProtocolRepository applicationProtocolRepository, final ModifierProtocolRepository modifierProtocolRepository, org.neo4j.logging.LogProvider logProvider)
			  internal virtual void Start( ApplicationProtocolRepository applicationProtocolRepository, ModifierProtocolRepository modifierProtocolRepository, LogProvider logProvider )
			  {
					RaftProtocolServerInstallerV2.Factory raftFactoryV2 = new RaftProtocolServerInstallerV2.Factory( nettyHandler, PipelineBuilderFactory, logProvider );
					RaftProtocolServerInstallerV1.Factory raftFactoryV1 = new RaftProtocolServerInstallerV1.Factory( nettyHandler, PipelineBuilderFactory, logProvider );
					ProtocolInstallerRepository<Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Server> protocolInstallerRepository = new ProtocolInstallerRepository<Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Server>( Arrays.asList( raftFactoryV1, raftFactoryV2 ), Org.Neo4j.causalclustering.protocol.ModifierProtocolInstaller_Fields.AllServerInstallers );

					EventLoopGroup = new NioEventLoopGroup();
					ServerBootstrap bootstrap = ( new ServerBootstrap() ).group(EventLoopGroup).channel(typeof(NioServerSocketChannel)).option(ChannelOption.SO_REUSEADDR, true).localAddress(PortAuthority.allocatePort()).childHandler((new HandshakeServerInitializer(applicationProtocolRepository, modifierProtocolRepository, protocolInstallerRepository, PipelineBuilderFactory, logProvider)).asChannelInitializer());

					Channel = bootstrap.bind().syncUninterruptibly().channel();
			  }

			  internal virtual void Stop()
			  {
					Channel.close().syncUninterruptibly();
					EventLoopGroup.shutdownGracefully( 0, TIMEOUT_SECONDS, SECONDS );
			  }

			  internal virtual int Port()
			  {
					return ( ( InetSocketAddress ) Channel.localAddress() ).Port;
			  }

			  public virtual ICollection<object> Received()
			  {
					return ReceivedConflict;
			  }
		 }

		 internal class Client
		 {
			  internal Bootstrap Bootstrap;
			  internal NioEventLoopGroup EventLoopGroup;
			  internal Channel Channel;
			  internal HandshakeClientInitializer HandshakeClientInitializer;

			  internal Client( ApplicationProtocolRepository applicationProtocolRepository, ModifierProtocolRepository modifierProtocolRepository, NettyPipelineBuilderFactory pipelineBuilderFactory, Config config, LogProvider logProvider )
			  {
					RaftProtocolClientInstallerV2.Factory raftFactoryV2 = new RaftProtocolClientInstallerV2.Factory( pipelineBuilderFactory, logProvider );
					RaftProtocolClientInstallerV1.Factory raftFactoryV1 = new RaftProtocolClientInstallerV1.Factory( pipelineBuilderFactory, logProvider );
					ProtocolInstallerRepository<Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Client> protocolInstallerRepository = new ProtocolInstallerRepository<Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Client>( Arrays.asList( raftFactoryV1, raftFactoryV2 ), Org.Neo4j.causalclustering.protocol.ModifierProtocolInstaller_Fields.AllClientInstallers );
					EventLoopGroup = new NioEventLoopGroup();
					Duration handshakeTimeout = config.Get( CausalClusteringSettings.handshake_timeout );
					HandshakeClientInitializer = new HandshakeClientInitializer( applicationProtocolRepository, modifierProtocolRepository, protocolInstallerRepository, pipelineBuilderFactory, handshakeTimeout, logProvider, logProvider );
					Bootstrap = ( new Bootstrap() ).group(EventLoopGroup).channel(typeof(NioSocketChannel)).handler(HandshakeClientInitializer);
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("SameParameterValue") void connect(int port)
			  internal virtual void Connect( int port )
			  {
					ChannelFuture channelFuture = Bootstrap.connect( "localhost", port ).syncUninterruptibly();
					Channel = channelFuture.channel();
			  }

			  internal virtual void Disconnect()
			  {
					if ( Channel != null )
					{
						 Channel.close().syncUninterruptibly();
						 EventLoopGroup.shutdownGracefully( 0, TIMEOUT_SECONDS, SECONDS ).syncUninterruptibly();
					}
			  }

			  internal virtual ChannelFuture Send( object message )
			  {
					return Channel.writeAndFlush( message );
			  }
		 }

		 private Matcher<object> MessageMatches<T1>( Org.Neo4j.causalclustering.core.consensus.RaftMessages_ClusterIdAwareMessage<T1> expected ) where T1 : Org.Neo4j.causalclustering.core.consensus.RaftMessages_RaftMessage
		 {
			  return new MessageMatcher( this, expected );
		 }

		 internal class MessageMatcher : BaseMatcher<object>
		 {
			 private readonly NettyInstalledProtocolsIT _outerInstance;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.neo4j.causalclustering.core.consensus.RaftMessages_ClusterIdAwareMessage<? extends org.neo4j.causalclustering.core.consensus.RaftMessages_RaftMessage> expected;
			  internal readonly Org.Neo4j.causalclustering.core.consensus.RaftMessages_ClusterIdAwareMessage<Org.Neo4j.causalclustering.core.consensus.RaftMessages_RaftMessage> Expected;

			  internal MessageMatcher<T1>( NettyInstalledProtocolsIT outerInstance, Org.Neo4j.causalclustering.core.consensus.RaftMessages_ClusterIdAwareMessage<T1> expected ) where T1 : Org.Neo4j.causalclustering.core.consensus.RaftMessages_RaftMessage
			  {
				  this._outerInstance = outerInstance;
					this.Expected = expected;
			  }

			  public override bool Matches( object item )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: if (item instanceof org.neo4j.causalclustering.core.consensus.RaftMessages_ClusterIdAwareMessage<?>)
					if ( item is Org.Neo4j.causalclustering.core.consensus.RaftMessages_ClusterIdAwareMessage<object> )
					{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.neo4j.causalclustering.core.consensus.RaftMessages_ClusterIdAwareMessage<?> message = (org.neo4j.causalclustering.core.consensus.RaftMessages_ClusterIdAwareMessage<?>) item;
						 Org.Neo4j.causalclustering.core.consensus.RaftMessages_ClusterIdAwareMessage<object> message = ( Org.Neo4j.causalclustering.core.consensus.RaftMessages_ClusterIdAwareMessage<object> ) item;
						 return message.ClusterId().Equals(Expected.clusterId()) && message.message().Equals(Expected.message());
					}
					else
					{
						 return false;
					}
			  }

			  public override void DescribeTo( Description description )
			  {
					description.appendText( "Cluster ID " ).appendValue( Expected.clusterId() ).appendText(" message ").appendValue(Expected.message());
			  }
		 }
	}

}