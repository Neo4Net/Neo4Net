using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.causalclustering.core
{

	using CatchupAddressProvider = Neo4Net.causalclustering.catchup.CatchupAddressProvider;
	using LocalDatabase = Neo4Net.causalclustering.catchup.storecopy.LocalDatabase;
	using StoreFiles = Neo4Net.causalclustering.catchup.storecopy.StoreFiles;
	using ConsensusModule = Neo4Net.causalclustering.core.consensus.ConsensusModule;
	using RaftMessages = Neo4Net.causalclustering.core.consensus.RaftMessages;
	using RaftProtocolClientInstallerV1 = Neo4Net.causalclustering.core.consensus.protocol.v1.RaftProtocolClientInstallerV1;
	using RaftProtocolClientInstallerV2 = Neo4Net.causalclustering.core.consensus.protocol.v2.RaftProtocolClientInstallerV2;
	using Role = Neo4Net.causalclustering.core.consensus.roles.Role;
	using ReplicationBenchmarkProcedure = Neo4Net.causalclustering.core.replication.ReplicationBenchmarkProcedure;
	using Replicator = Neo4Net.causalclustering.core.replication.Replicator;
	using CoreServerModule = Neo4Net.causalclustering.core.server.CoreServerModule;
	using ClusterStateDirectory = Neo4Net.causalclustering.core.state.ClusterStateDirectory;
	using ClusterStateException = Neo4Net.causalclustering.core.state.ClusterStateException;
	using ClusteringModule = Neo4Net.causalclustering.core.state.ClusteringModule;
	using CoreStateMachinesModule = Neo4Net.causalclustering.core.state.machines.CoreStateMachinesModule;
	using FreeIdFilteredIdGeneratorFactory = Neo4Net.causalclustering.core.state.machines.id.FreeIdFilteredIdGeneratorFactory;
	using CoreMonitor = Neo4Net.causalclustering.diagnostics.CoreMonitor;
	using CoreTopologyService = Neo4Net.causalclustering.discovery.CoreTopologyService;
	using DiscoveryServiceFactory = Neo4Net.causalclustering.discovery.DiscoveryServiceFactory;
	using TopologyService = Neo4Net.causalclustering.discovery.TopologyService;
	using ClusterOverviewProcedure = Neo4Net.causalclustering.discovery.procedures.ClusterOverviewProcedure;
	using CoreRoleProcedure = Neo4Net.causalclustering.discovery.procedures.CoreRoleProcedure;
	using InstalledProtocolsProcedure = Neo4Net.causalclustering.discovery.procedures.InstalledProtocolsProcedure;
	using DuplexPipelineWrapperFactory = Neo4Net.causalclustering.handlers.DuplexPipelineWrapperFactory;
	using PipelineWrapper = Neo4Net.causalclustering.handlers.PipelineWrapper;
	using VoidPipelineWrapperFactory = Neo4Net.causalclustering.handlers.VoidPipelineWrapperFactory;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using Neo4Net.causalclustering.logging;
	using Neo4Net.causalclustering.logging;
	using Neo4Net.causalclustering.logging;
	using Neo4Net.causalclustering.messaging;
	using Neo4Net.causalclustering.messaging;
	using RaftOutbound = Neo4Net.causalclustering.messaging.RaftOutbound;
	using SenderService = Neo4Net.causalclustering.messaging.SenderService;
	using InstalledProtocolHandler = Neo4Net.causalclustering.net.InstalledProtocolHandler;
	using Neo4Net.causalclustering.protocol;
	using NettyPipelineBuilderFactory = Neo4Net.causalclustering.protocol.NettyPipelineBuilderFactory;
	using Neo4Net.causalclustering.protocol;
	using Neo4Net.causalclustering.protocol;
	using Neo4Net.causalclustering.protocol;
	using ApplicationProtocolRepository = Neo4Net.causalclustering.protocol.handshake.ApplicationProtocolRepository;
	using ApplicationSupportedProtocols = Neo4Net.causalclustering.protocol.handshake.ApplicationSupportedProtocols;
	using HandshakeClientInitializer = Neo4Net.causalclustering.protocol.handshake.HandshakeClientInitializer;
	using ModifierProtocolRepository = Neo4Net.causalclustering.protocol.handshake.ModifierProtocolRepository;
	using ModifierSupportedProtocols = Neo4Net.causalclustering.protocol.handshake.ModifierSupportedProtocols;
	using ProtocolStack = Neo4Net.causalclustering.protocol.handshake.ProtocolStack;
	using LoadBalancingPluginLoader = Neo4Net.causalclustering.routing.load_balancing.LoadBalancingPluginLoader;
	using LoadBalancingProcessor = Neo4Net.causalclustering.routing.load_balancing.LoadBalancingProcessor;
	using GetServersProcedureForMultiDC = Neo4Net.causalclustering.routing.load_balancing.procedure.GetServersProcedureForMultiDC;
	using GetServersProcedureForSingleDC = Neo4Net.causalclustering.routing.load_balancing.procedure.GetServersProcedureForSingleDC;
	using LegacyGetServersProcedure = Neo4Net.causalclustering.routing.load_balancing.procedure.LegacyGetServersProcedure;
	using GetRoutersForAllDatabasesProcedure = Neo4Net.causalclustering.routing.multi_cluster.procedure.GetRoutersForAllDatabasesProcedure;
	using GetRoutersForDatabaseProcedure = Neo4Net.causalclustering.routing.multi_cluster.procedure.GetRoutersForDatabaseProcedure;
	using NoOpUpstreamDatabaseStrategiesLoader = Neo4Net.causalclustering.upstream.NoOpUpstreamDatabaseStrategiesLoader;
	using UpstreamDatabaseSelectionStrategy = Neo4Net.causalclustering.upstream.UpstreamDatabaseSelectionStrategy;
	using UpstreamDatabaseStrategiesLoader = Neo4Net.causalclustering.upstream.UpstreamDatabaseStrategiesLoader;
	using UpstreamDatabaseStrategySelector = Neo4Net.causalclustering.upstream.UpstreamDatabaseStrategySelector;
	using TypicallyConnectToRandomReadReplicaStrategy = Neo4Net.causalclustering.upstream.strategies.TypicallyConnectToRandomReadReplicaStrategy;
	using StoreUtil = Neo4Net.com.storecopy.StoreUtil;
	using Predicates = Neo4Net.Functions.Predicates;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using PlatformModule = Neo4Net.Graphdb.factory.module.PlatformModule;
	using AbstractEditionModule = Neo4Net.Graphdb.factory.module.edition.AbstractEditionModule;
	using DefaultEditionModule = Neo4Net.Graphdb.factory.module.edition.DefaultEditionModule;
	using IdContextFactoryBuilder = Neo4Net.Graphdb.factory.module.id.IdContextFactoryBuilder;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using SocketAddress = Neo4Net.Helpers.SocketAddress;
	using Neo4Net.Helpers.Collections;
	using KernelException = Neo4Net.@internal.Kernel.Api.exceptions.KernelException;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using NetworkConnectionTracker = Neo4Net.Kernel.api.net.NetworkConnectionTracker;
	using AvailabilityGuard = Neo4Net.Kernel.availability.AvailabilityGuard;
	using Config = Neo4Net.Kernel.configuration.Config;
	using SslPolicyLoader = Neo4Net.Kernel.configuration.ssl.SslPolicyLoader;
	using EnterpriseBuiltInDbmsProcedures = Neo4Net.Kernel.enterprise.builtinprocs.EnterpriseBuiltInDbmsProcedures;
	using EnterpriseBuiltInProcedures = Neo4Net.Kernel.enterprise.builtinprocs.EnterpriseBuiltInProcedures;
	using SchemaWriteGuard = Neo4Net.Kernel.Impl.Api.SchemaWriteGuard;
	using TransactionHeaderInformation = Neo4Net.Kernel.Impl.Api.TransactionHeaderInformation;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using EnterpriseConstraintSemantics = Neo4Net.Kernel.impl.enterprise.EnterpriseConstraintSemantics;
	using EnterpriseEditionModule = Neo4Net.Kernel.impl.enterprise.EnterpriseEditionModule;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using ConfigurableIOLimiter = Neo4Net.Kernel.impl.enterprise.transaction.log.checkpoint.ConfigurableIOLimiter;
	using StatementLocksFactorySelector = Neo4Net.Kernel.impl.factory.StatementLocksFactorySelector;
	using IndexConfigStore = Neo4Net.Kernel.impl.index.IndexConfigStore;
	using DefaultNetworkConnectionTracker = Neo4Net.Kernel.impl.net.DefaultNetworkConnectionTracker;
	using PageCacheWarmer = Neo4Net.Kernel.impl.pagecache.PageCacheWarmer;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using TransactionHeaderInformationFactory = Neo4Net.Kernel.impl.transaction.TransactionHeaderInformationFactory;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using TransactionLogFiles = Neo4Net.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using DataSourceManager = Neo4Net.Kernel.impl.transaction.state.DataSourceManager;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using DatabaseHealth = Neo4Net.Kernel.@internal.DatabaseHealth;
	using KernelData = Neo4Net.Kernel.@internal.KernelData;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using LogService = Neo4Net.Logging.@internal.LogService;
	using Clocks = Neo4Net.Time.Clocks;
	using UsageData = Neo4Net.Udc.UsageData;

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

			  ApplicationProtocolRepository applicationProtocolRepository = new ApplicationProtocolRepository( Neo4Net.causalclustering.protocol.Protocol_ApplicationProtocols.values(), supportedRaftProtocols );
			  ModifierProtocolRepository modifierProtocolRepository = new ModifierProtocolRepository( Neo4Net.causalclustering.protocol.Protocol_ModifierProtocols.values(), supportedModifierProtocols );

			  ProtocolInstallerRepository<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Client> protocolInstallerRepository = new ProtocolInstallerRepository<Neo4Net.causalclustering.protocol.ProtocolInstaller_Orientation_Client>( asList( new RaftProtocolClientInstallerV2.Factory( clientPipelineBuilderFactory, LogProvider ), new RaftProtocolClientInstallerV1.Factory( clientPipelineBuilderFactory, LogProvider ) ), Neo4Net.causalclustering.protocol.ModifierProtocolInstaller_Fields.AllClientInstallers );

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
			  Outbound<MemberId, Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage> loggingOutbound = new LoggingOutbound<MemberId, Neo4Net.causalclustering.core.consensus.RaftMessages_RaftMessage>( raftOutbound, identityModule.Myself(), messageLogger );

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

			  Neo4Net.causalclustering.catchup.CatchupAddressProvider_PrioritisingUpstreamStrategyBasedAddressProvider catchupAddressProvider = new Neo4Net.causalclustering.catchup.CatchupAddressProvider_PrioritisingUpstreamStrategyBasedAddressProvider( _consensusModule.raftMachine(), _topologyService, catchupStrategySelector );
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
			  return Neo4Net.Kernel.Impl.Api.SchemaWriteGuard_Fields.AllowAllWrites;
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