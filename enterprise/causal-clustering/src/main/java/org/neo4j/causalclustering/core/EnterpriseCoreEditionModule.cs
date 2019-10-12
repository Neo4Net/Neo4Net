using System;
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
namespace Org.Neo4j.causalclustering.core
{

	using CatchupAddressProvider = Org.Neo4j.causalclustering.catchup.CatchupAddressProvider;
	using LocalDatabase = Org.Neo4j.causalclustering.catchup.storecopy.LocalDatabase;
	using StoreFiles = Org.Neo4j.causalclustering.catchup.storecopy.StoreFiles;
	using ConsensusModule = Org.Neo4j.causalclustering.core.consensus.ConsensusModule;
	using RaftMessages = Org.Neo4j.causalclustering.core.consensus.RaftMessages;
	using RaftProtocolClientInstallerV1 = Org.Neo4j.causalclustering.core.consensus.protocol.v1.RaftProtocolClientInstallerV1;
	using RaftProtocolClientInstallerV2 = Org.Neo4j.causalclustering.core.consensus.protocol.v2.RaftProtocolClientInstallerV2;
	using Role = Org.Neo4j.causalclustering.core.consensus.roles.Role;
	using ReplicationBenchmarkProcedure = Org.Neo4j.causalclustering.core.replication.ReplicationBenchmarkProcedure;
	using Replicator = Org.Neo4j.causalclustering.core.replication.Replicator;
	using CoreServerModule = Org.Neo4j.causalclustering.core.server.CoreServerModule;
	using ClusterStateDirectory = Org.Neo4j.causalclustering.core.state.ClusterStateDirectory;
	using ClusterStateException = Org.Neo4j.causalclustering.core.state.ClusterStateException;
	using ClusteringModule = Org.Neo4j.causalclustering.core.state.ClusteringModule;
	using CoreStateMachinesModule = Org.Neo4j.causalclustering.core.state.machines.CoreStateMachinesModule;
	using FreeIdFilteredIdGeneratorFactory = Org.Neo4j.causalclustering.core.state.machines.id.FreeIdFilteredIdGeneratorFactory;
	using CoreMonitor = Org.Neo4j.causalclustering.diagnostics.CoreMonitor;
	using CoreTopologyService = Org.Neo4j.causalclustering.discovery.CoreTopologyService;
	using DiscoveryServiceFactory = Org.Neo4j.causalclustering.discovery.DiscoveryServiceFactory;
	using TopologyService = Org.Neo4j.causalclustering.discovery.TopologyService;
	using ClusterOverviewProcedure = Org.Neo4j.causalclustering.discovery.procedures.ClusterOverviewProcedure;
	using CoreRoleProcedure = Org.Neo4j.causalclustering.discovery.procedures.CoreRoleProcedure;
	using InstalledProtocolsProcedure = Org.Neo4j.causalclustering.discovery.procedures.InstalledProtocolsProcedure;
	using DuplexPipelineWrapperFactory = Org.Neo4j.causalclustering.handlers.DuplexPipelineWrapperFactory;
	using PipelineWrapper = Org.Neo4j.causalclustering.handlers.PipelineWrapper;
	using VoidPipelineWrapperFactory = Org.Neo4j.causalclustering.handlers.VoidPipelineWrapperFactory;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using Org.Neo4j.causalclustering.logging;
	using Org.Neo4j.causalclustering.logging;
	using Org.Neo4j.causalclustering.logging;
	using Org.Neo4j.causalclustering.messaging;
	using Org.Neo4j.causalclustering.messaging;
	using RaftOutbound = Org.Neo4j.causalclustering.messaging.RaftOutbound;
	using SenderService = Org.Neo4j.causalclustering.messaging.SenderService;
	using InstalledProtocolHandler = Org.Neo4j.causalclustering.net.InstalledProtocolHandler;
	using Org.Neo4j.causalclustering.protocol;
	using NettyPipelineBuilderFactory = Org.Neo4j.causalclustering.protocol.NettyPipelineBuilderFactory;
	using Org.Neo4j.causalclustering.protocol;
	using Org.Neo4j.causalclustering.protocol;
	using Org.Neo4j.causalclustering.protocol;
	using ApplicationProtocolRepository = Org.Neo4j.causalclustering.protocol.handshake.ApplicationProtocolRepository;
	using ApplicationSupportedProtocols = Org.Neo4j.causalclustering.protocol.handshake.ApplicationSupportedProtocols;
	using HandshakeClientInitializer = Org.Neo4j.causalclustering.protocol.handshake.HandshakeClientInitializer;
	using ModifierProtocolRepository = Org.Neo4j.causalclustering.protocol.handshake.ModifierProtocolRepository;
	using ModifierSupportedProtocols = Org.Neo4j.causalclustering.protocol.handshake.ModifierSupportedProtocols;
	using ProtocolStack = Org.Neo4j.causalclustering.protocol.handshake.ProtocolStack;
	using LoadBalancingPluginLoader = Org.Neo4j.causalclustering.routing.load_balancing.LoadBalancingPluginLoader;
	using LoadBalancingProcessor = Org.Neo4j.causalclustering.routing.load_balancing.LoadBalancingProcessor;
	using GetServersProcedureForMultiDC = Org.Neo4j.causalclustering.routing.load_balancing.procedure.GetServersProcedureForMultiDC;
	using GetServersProcedureForSingleDC = Org.Neo4j.causalclustering.routing.load_balancing.procedure.GetServersProcedureForSingleDC;
	using LegacyGetServersProcedure = Org.Neo4j.causalclustering.routing.load_balancing.procedure.LegacyGetServersProcedure;
	using GetRoutersForAllDatabasesProcedure = Org.Neo4j.causalclustering.routing.multi_cluster.procedure.GetRoutersForAllDatabasesProcedure;
	using GetRoutersForDatabaseProcedure = Org.Neo4j.causalclustering.routing.multi_cluster.procedure.GetRoutersForDatabaseProcedure;
	using NoOpUpstreamDatabaseStrategiesLoader = Org.Neo4j.causalclustering.upstream.NoOpUpstreamDatabaseStrategiesLoader;
	using UpstreamDatabaseSelectionStrategy = Org.Neo4j.causalclustering.upstream.UpstreamDatabaseSelectionStrategy;
	using UpstreamDatabaseStrategiesLoader = Org.Neo4j.causalclustering.upstream.UpstreamDatabaseStrategiesLoader;
	using UpstreamDatabaseStrategySelector = Org.Neo4j.causalclustering.upstream.UpstreamDatabaseStrategySelector;
	using TypicallyConnectToRandomReadReplicaStrategy = Org.Neo4j.causalclustering.upstream.strategies.TypicallyConnectToRandomReadReplicaStrategy;
	using StoreUtil = Org.Neo4j.com.storecopy.StoreUtil;
	using Predicates = Org.Neo4j.Function.Predicates;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using PlatformModule = Org.Neo4j.Graphdb.factory.module.PlatformModule;
	using AbstractEditionModule = Org.Neo4j.Graphdb.factory.module.edition.AbstractEditionModule;
	using DefaultEditionModule = Org.Neo4j.Graphdb.factory.module.edition.DefaultEditionModule;
	using IdContextFactoryBuilder = Org.Neo4j.Graphdb.factory.module.id.IdContextFactoryBuilder;
	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;
	using SocketAddress = Org.Neo4j.Helpers.SocketAddress;
	using Org.Neo4j.Helpers.Collection;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using NetworkConnectionTracker = Org.Neo4j.Kernel.api.net.NetworkConnectionTracker;
	using AvailabilityGuard = Org.Neo4j.Kernel.availability.AvailabilityGuard;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using SslPolicyLoader = Org.Neo4j.Kernel.configuration.ssl.SslPolicyLoader;
	using EnterpriseBuiltInDbmsProcedures = Org.Neo4j.Kernel.enterprise.builtinprocs.EnterpriseBuiltInDbmsProcedures;
	using EnterpriseBuiltInProcedures = Org.Neo4j.Kernel.enterprise.builtinprocs.EnterpriseBuiltInProcedures;
	using SchemaWriteGuard = Org.Neo4j.Kernel.Impl.Api.SchemaWriteGuard;
	using TransactionHeaderInformation = Org.Neo4j.Kernel.Impl.Api.TransactionHeaderInformation;
	using ThreadToStatementContextBridge = Org.Neo4j.Kernel.impl.core.ThreadToStatementContextBridge;
	using EnterpriseConstraintSemantics = Org.Neo4j.Kernel.impl.enterprise.EnterpriseConstraintSemantics;
	using EnterpriseEditionModule = Org.Neo4j.Kernel.impl.enterprise.EnterpriseEditionModule;
	using OnlineBackupSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using ConfigurableIOLimiter = Org.Neo4j.Kernel.impl.enterprise.transaction.log.checkpoint.ConfigurableIOLimiter;
	using StatementLocksFactorySelector = Org.Neo4j.Kernel.impl.factory.StatementLocksFactorySelector;
	using IndexConfigStore = Org.Neo4j.Kernel.impl.index.IndexConfigStore;
	using DefaultNetworkConnectionTracker = Org.Neo4j.Kernel.impl.net.DefaultNetworkConnectionTracker;
	using PageCacheWarmer = Org.Neo4j.Kernel.impl.pagecache.PageCacheWarmer;
	using Procedures = Org.Neo4j.Kernel.impl.proc.Procedures;
	using TransactionHeaderInformationFactory = Org.Neo4j.Kernel.impl.transaction.TransactionHeaderInformationFactory;
	using LogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Org.Neo4j.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using TransactionLogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using DataSourceManager = Org.Neo4j.Kernel.impl.transaction.state.DataSourceManager;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using DatabaseHealth = Org.Neo4j.Kernel.@internal.DatabaseHealth;
	using KernelData = Org.Neo4j.Kernel.@internal.KernelData;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using Clocks = Org.Neo4j.Time.Clocks;
	using UsageData = Org.Neo4j.Udc.UsageData;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.raft_messages_log_path;

	/// <summary>
	/// This implementation of <seealso cref="AbstractEditionModule"/> creates the implementations of services
	/// that are specific to the Enterprise Core edition that provides a core cluster.
	/// </summary>
	public class EnterpriseCoreEditionModule : DefaultEditionModule
	{
		 private readonly ConsensusModule _consensusModule;
		 private readonly ReplicationModule _replicationModule;
		 private readonly CoreTopologyService _topologyService;
		 protected internal readonly LogProvider LogProvider;
		 protected internal readonly Config Config;
		 private readonly System.Func<Stream<Pair<AdvertisedSocketAddress, ProtocolStack>>> _clientInstalledProtocols;
		 private readonly System.Func<Stream<Pair<SocketAddress, ProtocolStack>>> _serverInstalledProtocols;
		 private readonly CoreServerModule _coreServerModule;
		 private readonly CoreStateMachinesModule _coreStateMachinesModule;

		 public enum RaftLogImplementation
		 {
			  InMemory,
			  Segmented
		 }

		 private LoadBalancingProcessor LoadBalancingProcessor
		 {
			 get
			 {
				  try
				  {
						return LoadBalancingPluginLoader.load( _topologyService, _consensusModule.raftMachine(), LogProvider, Config );
				  }
				  catch ( Exception e )
				  {
						throw new Exception( e );
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void registerEditionSpecificProcedures(org.neo4j.kernel.impl.proc.Procedures procedures) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 public override void RegisterEditionSpecificProcedures( Procedures procedures )
		 {
			  procedures.RegisterProcedure( typeof( EnterpriseBuiltInDbmsProcedures ), true );
			  procedures.RegisterProcedure( typeof( EnterpriseBuiltInProcedures ), true );
			  procedures.Register( new LegacyGetServersProcedure( _topologyService, _consensusModule.raftMachine(), Config, LogProvider ) );

			  if ( Config.get( CausalClusteringSettings.MultiDcLicense ) )
			  {
					procedures.Register( new GetServersProcedureForMultiDC( LoadBalancingProcessor ) );
			  }
			  else
			  {
					procedures.Register( new GetServersProcedureForSingleDC( _topologyService, _consensusModule.raftMachine(), Config, LogProvider ) );
			  }

			  procedures.Register( new GetRoutersForAllDatabasesProcedure( _topologyService, Config ) );
			  procedures.Register( new GetRoutersForDatabaseProcedure( _topologyService, Config ) );
			  procedures.Register( new ClusterOverviewProcedure( _topologyService, LogProvider ) );
			  procedures.Register( new CoreRoleProcedure( _consensusModule.raftMachine() ) );
			  procedures.Register( new InstalledProtocolsProcedure( _clientInstalledProtocols, _serverInstalledProtocols ) );
			  procedures.RegisterComponent( typeof( Replicator ), x => _replicationModule.Replicator, false );
			  procedures.RegisterProcedure( typeof( ReplicationBenchmarkProcedure ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public EnterpriseCoreEditionModule(final org.neo4j.graphdb.factory.module.PlatformModule platformModule, final org.neo4j.causalclustering.discovery.DiscoveryServiceFactory discoveryServiceFactory)
		 public EnterpriseCoreEditionModule( PlatformModule platformModule, DiscoveryServiceFactory discoveryServiceFactory )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.util.Dependencies dependencies = platformModule.dependencies;
			  Dependencies dependencies = platformModule.Dependencies;
			  Config = platformModule.Config;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.logging.internal.LogService logging = platformModule.logging;
			  LogService logging = platformModule.Logging;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.io.fs.FileSystemAbstraction fileSystem = platformModule.fileSystem;
			  FileSystemAbstraction fileSystem = platformModule.FileSystem;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.io.layout.DatabaseLayout databaseLayout = platformModule.storeLayout.databaseLayout(config.get(org.neo4j.graphdb.factory.GraphDatabaseSettings.active_database));
			  DatabaseLayout databaseLayout = platformModule.StoreLayout.databaseLayout( Config.get( GraphDatabaseSettings.active_database ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.lifecycle.LifeSupport life = platformModule.life;
			  LifeSupport life = platformModule.Life;

			  CoreMonitor.register( logging.InternalLogProvider, logging.UserLogProvider, platformModule.Monitors );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File dataDir = config.get(org.neo4j.graphdb.factory.GraphDatabaseSettings.data_directory);
			  File dataDir = Config.get( GraphDatabaseSettings.data_directory );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.core.state.ClusterStateDirectory clusterStateDirectory = new org.neo4j.causalclustering.core.state.ClusterStateDirectory(dataDir, databaseLayout.databaseDirectory(), false);
			  ClusterStateDirectory clusterStateDirectory = new ClusterStateDirectory( dataDir, databaseLayout.DatabaseDirectory(), false );
			  try
			  {
					clusterStateDirectory.Initialize( fileSystem );
			  }
			  catch ( ClusterStateException e )
			  {
					throw new Exception( e );
			  }
			  dependencies.SatisfyDependency( clusterStateDirectory );

			  AvailabilityGuard globalGuard = GetGlobalAvailabilityGuard( platformModule.Clock, logging, platformModule.Config );
			  ThreadToTransactionBridgeConflict = dependencies.SatisfyDependency( new ThreadToStatementContextBridge( globalGuard ) );

			  LogProvider = logging.InternalLogProvider;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final System.Func<org.neo4j.kernel.internal.DatabaseHealth> databaseHealthSupplier = () -> platformModule.dataSourceManager.getDataSource().getDependencyResolver().resolveDependency(org.neo4j.kernel.internal.DatabaseHealth.class);
			  System.Func<DatabaseHealth> databaseHealthSupplier = () => platformModule.DataSourceManager.DataSource.DependencyResolver.resolveDependency(typeof(DatabaseHealth));

			  WatcherServiceFactoryConflict = directory => CreateFileSystemWatcherService( fileSystem, directory, logging, platformModule.JobScheduler, Config, FileWatcherFileNameFilter() );
			  dependencies.SatisfyDependencies( WatcherServiceFactoryConflict );
			  LogFiles logFiles = BuildLocalDatabaseLogFiles( platformModule, fileSystem, databaseLayout );
			  LocalDatabase localDatabase = new LocalDatabase( databaseLayout, new StoreFiles( fileSystem, platformModule.PageCache ), logFiles, platformModule.DataSourceManager, databaseHealthSupplier, globalGuard, LogProvider );

			  IdentityModule identityModule = new IdentityModule( platformModule, clusterStateDirectory.Get() );

			  ClusteringModule clusteringModule = GetClusteringModule( platformModule, discoveryServiceFactory, clusterStateDirectory, identityModule, dependencies, databaseLayout );

			  // We need to satisfy the dependency here to keep users of it, such as BoltKernelExtension, happy.
			  dependencies.SatisfyDependency( SslPolicyLoader.create( Config, LogProvider ) );

			  PipelineWrapper clientPipelineWrapper = PipelineWrapperFactory().forClient(Config, dependencies, LogProvider, CausalClusteringSettings.SslPolicy);
			  PipelineWrapper serverPipelineWrapper = PipelineWrapperFactory().forServer(Config, dependencies, LogProvider, CausalClusteringSettings.SslPolicy);
			  PipelineWrapper backupServerPipelineWrapper = PipelineWrapperFactory().forServer(Config, dependencies, LogProvider, OnlineBackupSettings.ssl_policy);

			  NettyPipelineBuilderFactory clientPipelineBuilderFactory = new NettyPipelineBuilderFactory( clientPipelineWrapper );
			  NettyPipelineBuilderFactory serverPipelineBuilderFactory = new NettyPipelineBuilderFactory( serverPipelineWrapper );
			  NettyPipelineBuilderFactory backupServerPipelineBuilderFactory = new NettyPipelineBuilderFactory( backupServerPipelineWrapper );

			  _topologyService = clusteringModule.TopologyService();

			  long logThresholdMillis = Config.get( CausalClusteringSettings.UnknownAddressLoggingThrottle ).toMillis();

			  SupportedProtocolCreator supportedProtocolCreator = new SupportedProtocolCreator( Config, LogProvider );
			  ApplicationSupportedProtocols supportedRaftProtocols = supportedProtocolCreator.CreateSupportedRaftProtocol();
			  ICollection<ModifierSupportedProtocols> supportedModifierProtocols = supportedProtocolCreator.CreateSupportedModifierProtocols();

			  ApplicationProtocolRepository applicationProtocolRepository = new ApplicationProtocolRepository( Org.Neo4j.causalclustering.protocol.Protocol_ApplicationProtocols.values(), supportedRaftProtocols );
			  ModifierProtocolRepository modifierProtocolRepository = new ModifierProtocolRepository( Org.Neo4j.causalclustering.protocol.Protocol_ModifierProtocols.values(), supportedModifierProtocols );

			  ProtocolInstallerRepository<Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Client> protocolInstallerRepository = new ProtocolInstallerRepository<Org.Neo4j.causalclustering.protocol.ProtocolInstaller_Orientation_Client>( asList( new RaftProtocolClientInstallerV2.Factory( clientPipelineBuilderFactory, LogProvider ), new RaftProtocolClientInstallerV1.Factory( clientPipelineBuilderFactory, LogProvider ) ), Org.Neo4j.causalclustering.protocol.ModifierProtocolInstaller_Fields.AllClientInstallers );

			  Duration handshakeTimeout = Config.get( CausalClusteringSettings.HandshakeTimeout );
			  HandshakeClientInitializer channelInitializer = new HandshakeClientInitializer( applicationProtocolRepository, modifierProtocolRepository, protocolInstallerRepository, clientPipelineBuilderFactory, handshakeTimeout, LogProvider, platformModule.Logging.UserLogProvider );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.messaging.SenderService raftSender = new org.neo4j.causalclustering.messaging.SenderService(channelInitializer, logProvider);
			  SenderService raftSender = new SenderService( channelInitializer, LogProvider );
			  life.Add( raftSender );
			  this._clientInstalledProtocols = raftSender.installedProtocols;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.logging.MessageLogger<org.neo4j.causalclustering.identity.MemberId> messageLogger = createMessageLogger(config, life, identityModule.myself());
			  MessageLogger<MemberId> messageLogger = CreateMessageLogger( Config, life, identityModule.Myself() );

			  RaftOutbound raftOutbound = new RaftOutbound( _topologyService, raftSender, clusteringModule.ClusterIdentity(), LogProvider, logThresholdMillis );
			  Outbound<MemberId, Org.Neo4j.causalclustering.core.consensus.RaftMessages_RaftMessage> loggingOutbound = new LoggingOutbound<MemberId, Org.Neo4j.causalclustering.core.consensus.RaftMessages_RaftMessage>( raftOutbound, identityModule.Myself(), messageLogger );

			  _consensusModule = new ConsensusModule( identityModule.Myself(), platformModule, loggingOutbound, clusterStateDirectory.Get(), _topologyService );

			  dependencies.SatisfyDependency( _consensusModule.raftMachine() );

			  _replicationModule = new ReplicationModule( _consensusModule.raftMachine(), identityModule.Myself(), platformModule, Config, loggingOutbound, clusterStateDirectory.Get(), fileSystem, LogProvider, globalGuard, localDatabase );

			  _coreStateMachinesModule = new CoreStateMachinesModule( identityModule.Myself(), platformModule, clusterStateDirectory.Get(), Config, _replicationModule.Replicator, _consensusModule.raftMachine(), dependencies, localDatabase );

			  IdContextFactoryConflict = IdContextFactoryBuilder.of( _coreStateMachinesModule.idTypeConfigurationProvider, platformModule.JobScheduler ).withIdGenerationFactoryProvider( ignored => _coreStateMachinesModule.idGeneratorFactory ).withFactoryWrapper( generator => new FreeIdFilteredIdGeneratorFactory( generator, _coreStateMachinesModule.freeIdCondition ) ).build();

			  // TODO: this is broken, coreStateMachinesModule.tokenHolders should be supplier, somehow...
			  this.TokenHoldersProviderConflict = databaseName => _coreStateMachinesModule.tokenHolders;
			  this.LocksSupplierConflict = _coreStateMachinesModule.locksSupplier;
			  this.CommitProcessFactoryConflict = _coreStateMachinesModule.commitProcessFactory;
			  this.AccessCapabilityConflict = new LeaderCanWrite( _consensusModule.raftMachine() );

			  InstalledProtocolHandler serverInstalledProtocolHandler = new InstalledProtocolHandler();

			  this._coreServerModule = new CoreServerModule( identityModule, platformModule, _consensusModule, _coreStateMachinesModule, clusteringModule, _replicationModule, localDatabase, databaseHealthSupplier, clusterStateDirectory.Get(), clientPipelineBuilderFactory, serverPipelineBuilderFactory, backupServerPipelineBuilderFactory, serverInstalledProtocolHandler );

			  TypicallyConnectToRandomReadReplicaStrategy defaultStrategy = new TypicallyConnectToRandomReadReplicaStrategy( 2 );
			  defaultStrategy.Inject( _topologyService, Config, LogProvider, identityModule.Myself() );
			  UpstreamDatabaseStrategySelector catchupStrategySelector = CreateUpstreamDatabaseStrategySelector( identityModule.Myself(), Config, LogProvider, _topologyService, defaultStrategy );

			  Org.Neo4j.causalclustering.catchup.CatchupAddressProvider_PrioritisingUpstreamStrategyBasedAddressProvider catchupAddressProvider = new Org.Neo4j.causalclustering.catchup.CatchupAddressProvider_PrioritisingUpstreamStrategyBasedAddressProvider( _consensusModule.raftMachine(), _topologyService, catchupStrategySelector );
			  RaftServerModule.CreateAndStart( platformModule, _consensusModule, identityModule, _coreServerModule, localDatabase, serverPipelineBuilderFactory, messageLogger, catchupAddressProvider, supportedRaftProtocols, supportedModifierProtocols, serverInstalledProtocolHandler );
			  _serverInstalledProtocols = serverInstalledProtocolHandler.installedProtocols;

			  EditionInvariants( platformModule, dependencies, Config, logging, life );

			  life.Add( _coreServerModule.membershipWaiterLifecycle );
		 }

		 private UpstreamDatabaseStrategySelector CreateUpstreamDatabaseStrategySelector( MemberId myself, Config config, LogProvider logProvider, TopologyService topologyService, UpstreamDatabaseSelectionStrategy defaultStrategy )
		 {
			  UpstreamDatabaseStrategiesLoader loader;
			  if ( config.Get( CausalClusteringSettings.MultiDcLicense ) )
			  {
					loader = new UpstreamDatabaseStrategiesLoader( topologyService, config, myself, logProvider );
					logProvider.getLog( this.GetType() ).Info("Multi-Data Center option enabled.");
			  }
			  else
			  {
					loader = new NoOpUpstreamDatabaseStrategiesLoader();
			  }

			  return new UpstreamDatabaseStrategySelector( defaultStrategy, loader, logProvider );
		 }

		 private LogFiles BuildLocalDatabaseLogFiles( PlatformModule platformModule, FileSystemAbstraction fileSystem, DatabaseLayout databaseLayout )
		 {
			  try
			  {
					return LogFilesBuilder.activeFilesBuilder( databaseLayout, fileSystem, platformModule.PageCache ).withConfig( Config ).build();
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 protected internal virtual ClusteringModule GetClusteringModule( PlatformModule platformModule, DiscoveryServiceFactory discoveryServiceFactory, ClusterStateDirectory clusterStateDirectory, IdentityModule identityModule, Dependencies dependencies, DatabaseLayout databaseLayout )
		 {
			  return new ClusteringModule( discoveryServiceFactory, identityModule.Myself(), platformModule, clusterStateDirectory.Get(), databaseLayout );
		 }

		 protected internal virtual DuplexPipelineWrapperFactory PipelineWrapperFactory()
		 {
			  return new VoidPipelineWrapperFactory();
		 }

		 internal static System.Predicate<string> FileWatcherFileNameFilter()
		 {
			  return Predicates.any( fileName => fileName.StartsWith( TransactionLogFiles.DEFAULT_NAME ), fileName => fileName.StartsWith( IndexConfigStore.INDEX_DB_FILE_NAME ), filename => filename.StartsWith( StoreUtil.TEMP_COPY_DIRECTORY_NAME ), filename => filename.EndsWith( PageCacheWarmer.SUFFIX_CACHEPROF ) );
		 }

		 private static MessageLogger<MemberId> CreateMessageLogger( Config config, LifeSupport life, MemberId myself )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.logging.MessageLogger<org.neo4j.causalclustering.identity.MemberId> messageLogger;
			  MessageLogger<MemberId> messageLogger;
			  if ( config.Get( CausalClusteringSettings.RaftMessagesLogEnable ) )
			  {
					File logFile = config.Get( raft_messages_log_path );
					messageLogger = life.Add( new BetterMessageLogger<>( myself, RaftMessagesLog( logFile ), Clocks.systemClock() ) );
			  }
			  else
			  {
					messageLogger = new NullMessageLogger<MemberId>();
			  }
			  return messageLogger;
		 }

		 private void EditionInvariants( PlatformModule platformModule, Dependencies dependencies, Config config, LogService logging, LifeSupport life )
		 {
			  StatementLocksFactoryProviderConflict = locks => ( new StatementLocksFactorySelector( locks, config, logging ) ).select();

			  dependencies.SatisfyDependency( CreateKernelData( platformModule.FileSystem, platformModule.PageCache, platformModule.StoreLayout.storeDirectory(), config, platformModule.DataSourceManager, life ) );

			  IoLimiterConflict = new ConfigurableIOLimiter( platformModule.Config );

			  HeaderInformationFactoryConflict = CreateHeaderInformationFactory();

			  SchemaWriteGuardConflict = CreateSchemaWriteGuard();

			  TransactionStartTimeoutConflict = config.Get( GraphDatabaseSettings.transaction_start_timeout ).toMillis();

			  ConstraintSemanticsConflict = new EnterpriseConstraintSemantics();

			  PublishEditionInfo( dependencies.ResolveDependency( typeof( UsageData ) ), platformModule.DatabaseInfo, config );

			  ConnectionTrackerConflict = dependencies.SatisfyDependency( CreateConnectionTracker() );
		 }

		 public virtual bool Leader
		 {
			 get
			 {
				  return _consensusModule.raftMachine().currentRole() == Role.LEADER;
			 }
		 }

		 private static PrintWriter RaftMessagesLog( File logFile )
		 {
			  //noinspection ResultOfMethodCallIgnored
			  logFile.ParentFile.mkdirs();
			  try
			  {
					return new PrintWriter( new FileStream( logFile, true ) );
			  }
			  catch ( FileNotFoundException e )
			  {
					throw new Exception( e );
			  }
		 }

		 private static SchemaWriteGuard CreateSchemaWriteGuard()
		 {
			  return Org.Neo4j.Kernel.Impl.Api.SchemaWriteGuard_Fields.AllowAllWrites;
		 }

		 private static KernelData CreateKernelData( FileSystemAbstraction fileSystem, PageCache pageCache, File storeDir, Config config, DataSourceManager dataSourceManager, LifeSupport life )
		 {
			  KernelData kernelData = new KernelData( fileSystem, pageCache, storeDir, config, dataSourceManager );
			  return life.Add( kernelData );
		 }

		 private static TransactionHeaderInformationFactory CreateHeaderInformationFactory()
		 {
			  return () => new TransactionHeaderInformation(-1, -1, new sbyte[0]);
		 }

		 protected internal override NetworkConnectionTracker CreateConnectionTracker()
		 {
			  return new DefaultNetworkConnectionTracker();
		 }

		 public override void CreateSecurityModule( PlatformModule platformModule, Procedures procedures )
		 {
			  EnterpriseEditionModule.createEnterpriseSecurityModule( this, platformModule, procedures );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void disableCatchupServer() throws Throwable
		 public virtual void DisableCatchupServer()
		 {
			  _coreServerModule.catchupServer().disable();
		 }
	}

}