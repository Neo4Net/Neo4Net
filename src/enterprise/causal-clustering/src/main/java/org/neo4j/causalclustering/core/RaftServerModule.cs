using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.causalclustering.core
{
	using ChannelInboundHandler = io.netty.channel.ChannelInboundHandler;


	using CatchupAddressProvider = Neo4Net.causalclustering.catchup.CatchupAddressProvider;
	using LocalDatabase = Neo4Net.causalclustering.catchup.storecopy.LocalDatabase;
	using ConsensusModule = Neo4Net.causalclustering.core.consensus.ConsensusModule;
	using ContinuousJob = Neo4Net.causalclustering.core.consensus.ContinuousJob;
	using LeaderAvailabilityHandler = Neo4Net.causalclustering.core.consensus.LeaderAvailabilityHandler;
	using RaftMessageMonitoringHandler = Neo4Net.causalclustering.core.consensus.RaftMessageMonitoringHandler;
	using RaftMessageNettyHandler = Neo4Net.causalclustering.core.consensus.RaftMessageNettyHandler;
	using RaftMessageTimerResetMonitor = Neo4Net.causalclustering.core.consensus.RaftMessageTimerResetMonitor;
	using Neo4Net.causalclustering.core.consensus;
	using RaftProtocolServerInstallerV1 = Neo4Net.causalclustering.core.consensus.protocol.v1.RaftProtocolServerInstallerV1;
	using RaftProtocolServerInstallerV2 = Neo4Net.causalclustering.core.consensus.protocol.v2.RaftProtocolServerInstallerV2;
	using CoreServerModule = Neo4Net.causalclustering.core.server.CoreServerModule;
	using RaftMessageApplier = Neo4Net.causalclustering.core.state.RaftMessageApplier;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Neo4Net.causalclustering.logging;
	using ComposableMessageHandler = Neo4Net.causalclustering.messaging.ComposableMessageHandler;
	using Neo4Net.causalclustering.messaging;
	using Neo4Net.causalclustering.messaging;
	using Server = Neo4Net.causalclustering.net.Server;
	using Neo4Net.causalclustering.protocol;
	using NettyPipelineBuilderFactory = Neo4Net.causalclustering.protocol.NettyPipelineBuilderFactory;
	using Neo4Net.causalclustering.protocol;
	using Neo4Net.causalclustering.protocol;
	using Neo4Net.causalclustering.protocol;
	using ApplicationProtocolRepository = Neo4Net.causalclustering.protocol.handshake.ApplicationProtocolRepository;
	using ApplicationSupportedProtocols = Neo4Net.causalclustering.protocol.handshake.ApplicationSupportedProtocols;
	using HandshakeServerInitializer = Neo4Net.causalclustering.protocol.handshake.HandshakeServerInitializer;
	using ModifierProtocolRepository = Neo4Net.causalclustering.protocol.handshake.ModifierProtocolRepository;
	using ModifierSupportedProtocols = Neo4Net.causalclustering.protocol.handshake.ModifierSupportedProtocols;
	using PlatformModule = Neo4Net.GraphDb.factory.module.PlatformModule;
	using ListenSocketAddress = Neo4Net.Helpers.ListenSocketAddress;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using Group = Neo4Net.Scheduler.Group;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.raft_in_queue_max_batch;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.raft_in_queue_max_batch_bytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.raft_in_queue_max_bytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.causalclustering.core.CausalClusteringSettings.raft_in_queue_size;

	internal class RaftServerModule
	{
		 private readonly PlatformModule _platformModule;
		 private readonly ConsensusModule _consensusModule;
		 private readonly IdentityModule _identityModule;
		 private readonly ApplicationSupportedProtocols _supportedApplicationProtocol;
		 private readonly LocalDatabase _localDatabase;
		 private readonly MessageLogger<MemberId> _messageLogger;
		 private readonly LogProvider _logProvider;
		 private readonly NettyPipelineBuilderFactory _pipelineBuilderFactory;
		 private Neo4Net.causalclustering.catchup.CatchupAddressProvider_PrioritisingUpstreamStrategyBasedAddressProvider _catchupAddressProvider;
		 private readonly ICollection<ModifierSupportedProtocols> _supportedModifierProtocols;

		 private RaftServerModule( PlatformModule platformModule, ConsensusModule consensusModule, IdentityModule identityModule, CoreServerModule coreServerModule, LocalDatabase localDatabase, NettyPipelineBuilderFactory pipelineBuilderFactory, MessageLogger<MemberId> messageLogger, Neo4Net.causalclustering.catchup.CatchupAddressProvider_PrioritisingUpstreamStrategyBasedAddressProvider catchupAddressProvider, ApplicationSupportedProtocols supportedApplicationProtocol, ICollection<ModifierSupportedProtocols> supportedModifierProtocols, ChannelInboundHandler installedProtocolsHandler )
		 {
			  this._platformModule = platformModule;
			  this._consensusModule = consensusModule;
			  this._identityModule = identityModule;
			  this._supportedApplicationProtocol = supportedApplicationProtocol;
			  this._localDatabase = localDatabase;
			  this._messageLogger = messageLogger;
			  this._logProvider = platformModule.Logging.InternalLogProvider;
			  this._pipelineBuilderFactory = pipelineBuilderFactory;
			  this._catchupAddressProvider = catchupAddressProvider;
			  this._supportedModifierProtocols = supportedModifierProtocols;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.Neo4Net.causalclustering.messaging.LifecycleMessageHandler<org.Neo4Net.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage<?>> messageHandlerChain = createMessageHandlerChain(coreServerModule);
			  LifecycleMessageHandler<RaftMessages_ReceivedInstantClusterIdAwareMessage<object>> messageHandlerChain = CreateMessageHandlerChain( coreServerModule );

			  CreateRaftServer( coreServerModule, messageHandlerChain, installedProtocolsHandler );
		 }

		 internal static void CreateAndStart( PlatformModule platformModule, ConsensusModule consensusModule, IdentityModule identityModule, CoreServerModule coreServerModule, LocalDatabase localDatabase, NettyPipelineBuilderFactory pipelineBuilderFactory, MessageLogger<MemberId> messageLogger, Neo4Net.causalclustering.catchup.CatchupAddressProvider_PrioritisingUpstreamStrategyBasedAddressProvider addressProvider, ApplicationSupportedProtocols supportedApplicationProtocol, ICollection<ModifierSupportedProtocols> supportedModifierProtocols, ChannelInboundHandler installedProtocolsHandler )
		 {
			  new RaftServerModule( platformModule, consensusModule, identityModule, coreServerModule, localDatabase, pipelineBuilderFactory, messageLogger, addressProvider, supportedApplicationProtocol, supportedModifierProtocols, installedProtocolsHandler );
		 }

		 private void CreateRaftServer<T1>( CoreServerModule coreServerModule, LifecycleMessageHandler<T1> messageHandlerChain, ChannelInboundHandler installedProtocolsHandler )
		 {
			  ApplicationProtocolRepository applicationProtocolRepository = new ApplicationProtocolRepository( Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocols.values(), _supportedApplicationProtocol );
			  ModifierProtocolRepository modifierProtocolRepository = new ModifierProtocolRepository( Neo4Net.causalclustering.protocol.Protocol_ModifierProtocols.values(), _supportedModifierProtocols );

			  RaftMessageNettyHandler nettyHandler = new RaftMessageNettyHandler( _logProvider );
			  RaftProtocolServerInstallerV2.Factory raftProtocolServerInstallerV2 = new RaftProtocolServerInstallerV2.Factory( nettyHandler, _pipelineBuilderFactory, _logProvider );
			  RaftProtocolServerInstallerV1.Factory raftProtocolServerInstallerV1 = new RaftProtocolServerInstallerV1.Factory( nettyHandler, _pipelineBuilderFactory, _logProvider );
			  ProtocolInstallerRepository<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Server> protocolInstallerRepository = new ProtocolInstallerRepository<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Server>( asList( raftProtocolServerInstallerV1, raftProtocolServerInstallerV2 ), Neo4Net.causalclustering.protocol.ModifierProtocolInstaller_Fields.AllServerInstallers );

			  HandshakeServerInitializer handshakeServerInitializer = new HandshakeServerInitializer( applicationProtocolRepository, modifierProtocolRepository, protocolInstallerRepository, _pipelineBuilderFactory, _logProvider );

			  ListenSocketAddress raftListenAddress = _platformModule.config.get( CausalClusteringSettings.RaftListenAddress );
			  Server raftServer = new Server( handshakeServerInitializer, installedProtocolsHandler, _logProvider, _platformModule.logging.UserLogProvider, raftListenAddress, "raft-server" );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.Neo4Net.causalclustering.messaging.LoggingInbound<org.Neo4Net.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage<?>> loggingRaftInbound = new org.Neo4Net.causalclustering.messaging.LoggingInbound<>(nettyHandler, messageLogger, identityModule.myself());
			  LoggingInbound<RaftMessages_ReceivedInstantClusterIdAwareMessage<object>> loggingRaftInbound = new LoggingInbound<RaftMessages_ReceivedInstantClusterIdAwareMessage<object>>( nettyHandler, _messageLogger, _identityModule.myself() );
			  loggingRaftInbound.RegisterHandler( messageHandlerChain );

			  _platformModule.life.add( raftServer ); // must start before core state so that it can trigger snapshot downloads when necessary
			  _platformModule.life.add( coreServerModule.CreateCoreLife( messageHandlerChain ) );
			  _platformModule.life.add( coreServerModule.CatchupServer() ); // must start last and stop first, since it handles external requests
			  coreServerModule.BackupServer().ifPresent(_platformModule.life.add);
			  _platformModule.life.add( coreServerModule.DownloadService() );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.Neo4Net.causalclustering.messaging.LifecycleMessageHandler<org.Neo4Net.causalclustering.core.consensus.RaftMessages_ReceivedInstantClusterIdAwareMessage<?>> createMessageHandlerChain(org.Neo4Net.causalclustering.core.server.CoreServerModule coreServerModule)
		 private LifecycleMessageHandler<RaftMessages_ReceivedInstantClusterIdAwareMessage<object>> CreateMessageHandlerChain( CoreServerModule coreServerModule )
		 {
			  RaftMessageApplier messageApplier = new RaftMessageApplier( _localDatabase, _logProvider, _consensusModule.raftMachine(), coreServerModule.DownloadService(), coreServerModule.CommandApplicationProcess(), _catchupAddressProvider );

			  ComposableMessageHandler monitoringHandler = RaftMessageMonitoringHandler.composable( _platformModule.clock, _platformModule.monitors );
			  ComposableMessageHandler batchingMessageHandler = CreateBatchingHandler( _platformModule.config );
			  ComposableMessageHandler leaderAvailabilityHandler = LeaderAvailabilityHandler.composable( _consensusModule.LeaderAvailabilityTimers, _platformModule.monitors.newMonitor( typeof( RaftMessageTimerResetMonitor ) ), _consensusModule.raftMachine().term );
			  ComposableMessageHandler clusterBindingHandler = ClusterBindingHandler.Composable( _logProvider );

			  return clusterBindingHandler.compose( leaderAvailabilityHandler ).compose( batchingMessageHandler ).compose( monitoringHandler ).apply( messageApplier );
		 }

		 private ComposableMessageHandler CreateBatchingHandler( Config config )
		 {
			  System.Func<ThreadStart, ContinuousJob> jobFactory = runnable => new ContinuousJob( _platformModule.jobScheduler.threadFactory( Group.RAFT_BATCH_HANDLER ), runnable, _logProvider );

			  BoundedPriorityQueue.Config inQueueConfig = new BoundedPriorityQueue.Config( config.Get( raft_in_queue_size ), config.Get( raft_in_queue_max_bytes ) );
			  BatchingMessageHandler.Config batchConfig = new BatchingMessageHandler.Config( config.Get( raft_in_queue_max_batch ), config.Get( raft_in_queue_max_batch_bytes ) );

			  return BatchingMessageHandler.Composable( inQueueConfig, batchConfig, jobFactory, _logProvider );
		 }
	}

}