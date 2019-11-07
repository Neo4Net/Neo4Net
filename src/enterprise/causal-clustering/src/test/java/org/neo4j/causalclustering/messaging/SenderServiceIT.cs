using System.Collections.Generic;

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
namespace Neo4Net.causalclustering.messaging
{
	using ChannelHandlerContext = io.netty.channel.ChannelHandlerContext;
	using ChannelInboundHandler = io.netty.channel.ChannelInboundHandler;
	using ChannelInboundHandlerAdapter = io.netty.channel.ChannelInboundHandlerAdapter;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using RaftMessages = Neo4Net.causalclustering.core.consensus.RaftMessages;
	using MemberIdSet = Neo4Net.causalclustering.core.consensus.membership.MemberIdSet;
	using RaftProtocolClientInstallerV1 = Neo4Net.causalclustering.core.consensus.protocol.v1.RaftProtocolClientInstallerV1;
	using RaftProtocolServerInstallerV1 = Neo4Net.causalclustering.core.consensus.protocol.v1.RaftProtocolServerInstallerV1;
	using RaftProtocolClientInstallerV2 = Neo4Net.causalclustering.core.consensus.protocol.v2.RaftProtocolClientInstallerV2;
	using RaftProtocolServerInstallerV2 = Neo4Net.causalclustering.core.consensus.protocol.v2.RaftProtocolServerInstallerV2;
	using ClusterId = Neo4Net.causalclustering.identity.ClusterId;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Server = Neo4Net.causalclustering.net.Server;
	using Neo4Net.causalclustering.protocol;
	using NettyPipelineBuilderFactory = Neo4Net.causalclustering.protocol.NettyPipelineBuilderFactory;
	using Neo4Net.causalclustering.protocol;
	using Protocol_ApplicationProtocols = Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocols;
	using Protocol_ModifierProtocols = Neo4Net.causalclustering.protocol.Protocol_ModifierProtocols;
	using Neo4Net.causalclustering.protocol;
	using Neo4Net.causalclustering.protocol;
	using ApplicationProtocolRepository = Neo4Net.causalclustering.protocol.handshake.ApplicationProtocolRepository;
	using ApplicationSupportedProtocols = Neo4Net.causalclustering.protocol.handshake.ApplicationSupportedProtocols;
	using HandshakeClientInitializer = Neo4Net.causalclustering.protocol.handshake.HandshakeClientInitializer;
	using HandshakeServerInitializer = Neo4Net.causalclustering.protocol.handshake.HandshakeServerInitializer;
	using ModifierProtocolRepository = Neo4Net.causalclustering.protocol.handshake.ModifierProtocolRepository;
	using ModifierSupportedProtocols = Neo4Net.causalclustering.protocol.handshake.ModifierSupportedProtocols;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using ListenSocketAddress = Neo4Net.Helpers.ListenSocketAddress;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using PortAuthority = Neo4Net.Ports.Allocation.PortAuthority;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.handlers.VoidPipelineWrapperFactory.VOID_WRAPPER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterators.asSet;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class SenderServiceIT
	public class SenderServiceIT
	{
		private bool InstanceFieldsInitialized = false;

		public SenderServiceIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_applicationProtocolRepository = new ApplicationProtocolRepository( Protocol_ApplicationProtocols.values(), _supportedApplicationProtocol );
			_modifierProtocolRepository = new ModifierProtocolRepository( Protocol_ModifierProtocols.values(), _supportedModifierProtocols );
		}

		 private readonly LogProvider _logProvider = NullLogProvider.Instance;

		 private readonly ApplicationSupportedProtocols _supportedApplicationProtocol = new ApplicationSupportedProtocols( Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocolCategory.Raft, Arrays.asList( Protocol_ApplicationProtocols.RAFT_1.implementation(), Protocol_ApplicationProtocols.RAFT_2.implementation() ) );
		 private readonly ICollection<ModifierSupportedProtocols> _supportedModifierProtocols = emptyList();

		 private ApplicationProtocolRepository _applicationProtocolRepository;
		 private ModifierProtocolRepository _modifierProtocolRepository;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter public boolean blocking;
		 public bool Blocking;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(1) public Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocols clientProtocol;
		 public Protocol_ApplicationProtocols ClientProtocol;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "blocking={0} protocol={1}") public static Iterable<Object[]> params()
		 public static IEnumerable<object[]> Params()
		 {
			  return ClientRepositories().stream().flatMap(r => Stream.of(new object[]{ true, r }, new object[]{ false, r })).collect(Collectors.toList());
		 }

		 private static ICollection<Protocol_ApplicationProtocols> ClientRepositories()
		 {
			  return Arrays.asList( Protocol_ApplicationProtocols.RAFT_1, Protocol_ApplicationProtocols.RAFT_2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSendAndReceive() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSendAndReceive()
		 {
			  // given: raft server handler
			  int port = PortAuthority.allocatePort();
			  Semaphore messageReceived = new Semaphore( 0 );
			  ChannelInboundHandler nettyHandler = new ChannelInboundHandlerAdapterAnonymousInnerClass( this, messageReceived );
			  Server raftServer = raftServer( nettyHandler, port );
			  raftServer.Start();

			  // given: raft messaging service
			  SenderService sender = RaftSender();
			  sender.Start();

			  // when
			  AdvertisedSocketAddress to = new AdvertisedSocketAddress( "localhost", port );
			  MemberId memberId = new MemberId( System.Guid.randomUUID() );
			  ClusterId clusterId = new ClusterId( System.Guid.randomUUID() );

			  Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_Request newEntryMessage = new Neo4Net.causalclustering.core.consensus.RaftMessages_NewEntry_Request( memberId, new MemberIdSet( asSet( memberId ) ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Neo4Net.causalclustering.core.consensus.RaftMessages_ClusterIdAwareMessage<?> message = Neo4Net.causalclustering.core.consensus.RaftMessages_ClusterIdAwareMessage.of(clusterId, newEntryMessage);
			  Neo4Net.causalclustering.core.consensus.RaftMessages_ClusterIdAwareMessage<object> message = Neo4Net.causalclustering.core.consensus.RaftMessages_ClusterIdAwareMessage.of( clusterId, newEntryMessage );

			  sender.Send( to, message, Blocking );

			  // then
			  assertTrue( messageReceived.tryAcquire( 15, SECONDS ) );

			  // cleanup
			  sender.Stop();
			  raftServer.Stop();
		 }

		 private class ChannelInboundHandlerAdapterAnonymousInnerClass : ChannelInboundHandlerAdapter
		 {
			 private readonly SenderServiceIT _outerInstance;

			 private Semaphore _messageReceived;

			 public ChannelInboundHandlerAdapterAnonymousInnerClass( SenderServiceIT outerInstance, Semaphore messageReceived )
			 {
				 this.outerInstance = outerInstance;
				 this._messageReceived = messageReceived;
			 }

			 public override void channelRead( ChannelHandlerContext ctx, object msg )
			 {
				  _messageReceived.release();
			 }
		 }

		 private Server RaftServer( ChannelInboundHandler nettyHandler, int port )
		 {
			  NettyPipelineBuilderFactory pipelineFactory = new NettyPipelineBuilderFactory( VOID_WRAPPER );

			  RaftProtocolServerInstallerV1.Factory factoryV1 = new RaftProtocolServerInstallerV1.Factory( nettyHandler, pipelineFactory, _logProvider );
			  RaftProtocolServerInstallerV2.Factory factoryV2 = new RaftProtocolServerInstallerV2.Factory( nettyHandler, pipelineFactory, _logProvider );
			  ProtocolInstallerRepository<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Server> installer = new ProtocolInstallerRepository<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Server>( Arrays.asList( factoryV1, factoryV2 ), Neo4Net.causalclustering.protocol.ModifierProtocolInstaller_Fields.AllServerInstallers );

			  HandshakeServerInitializer channelInitializer = new HandshakeServerInitializer( _applicationProtocolRepository, _modifierProtocolRepository, installer, pipelineFactory, _logProvider );

			  ListenSocketAddress listenAddress = new ListenSocketAddress( "localhost", port );
			  return new Server( channelInitializer, null, _logProvider, _logProvider, listenAddress, "raft-server" );
		 }

		 private SenderService RaftSender()
		 {
			  NettyPipelineBuilderFactory pipelineFactory = new NettyPipelineBuilderFactory( VOID_WRAPPER );

			  RaftProtocolClientInstallerV1.Factory factoryV1 = new RaftProtocolClientInstallerV1.Factory( pipelineFactory, _logProvider );
			  RaftProtocolClientInstallerV2.Factory factoryV2 = new RaftProtocolClientInstallerV2.Factory( pipelineFactory, _logProvider );
			  ProtocolInstallerRepository<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Client> protocolInstaller = new ProtocolInstallerRepository<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Client>( Arrays.asList( factoryV1, factoryV2 ), Neo4Net.causalclustering.protocol.ModifierProtocolInstaller_Fields.AllClientInstallers );

			  HandshakeClientInitializer channelInitializer = new HandshakeClientInitializer( ClientRepository(), _modifierProtocolRepository, protocolInstaller, pipelineFactory, Duration.ofSeconds(5), _logProvider, _logProvider );

			  return new SenderService( channelInitializer, _logProvider );
		 }

		 private ApplicationProtocolRepository ClientRepository()
		 {
			  return new ApplicationProtocolRepository( new Protocol_ApplicationProtocols[]{ Protocol_ApplicationProtocols.RAFT_2 }, new ApplicationSupportedProtocols( Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocolCategory.Raft, emptyList() ) );
		 }
	}

}