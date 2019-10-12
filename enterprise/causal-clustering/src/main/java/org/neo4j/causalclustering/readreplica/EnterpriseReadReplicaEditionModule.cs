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
namespace Org.Neo4j.causalclustering.readreplica
{
	using ChannelInitializer = io.netty.channel.ChannelInitializer;
	using SocketChannel = io.netty.channel.socket.SocketChannel;


	using CatchUpClient = Org.Neo4j.causalclustering.catchup.CatchUpClient;
	using CatchUpResponseHandler = Org.Neo4j.causalclustering.catchup.CatchUpResponseHandler;
	using CatchupProtocolClientInstaller = Org.Neo4j.causalclustering.catchup.CatchupProtocolClientInstaller;
	using CatchupServerBuilder = Org.Neo4j.causalclustering.catchup.CatchupServerBuilder;
	using CheckPointerService = Org.Neo4j.causalclustering.catchup.CheckPointerService;
	using RegularCatchupServerHandler = Org.Neo4j.causalclustering.catchup.RegularCatchupServerHandler;
	using CopiedStoreRecovery = Org.Neo4j.causalclustering.catchup.storecopy.CopiedStoreRecovery;
	using LocalDatabase = Org.Neo4j.causalclustering.catchup.storecopy.LocalDatabase;
	using RemoteStore = Org.Neo4j.causalclustering.catchup.storecopy.RemoteStore;
	using StoreCopyClient = Org.Neo4j.causalclustering.catchup.storecopy.StoreCopyClient;
	using StoreCopyProcess = Org.Neo4j.causalclustering.catchup.storecopy.StoreCopyProcess;
	using StoreFiles = Org.Neo4j.causalclustering.catchup.storecopy.StoreFiles;
	using BatchingTxApplier = Org.Neo4j.causalclustering.catchup.tx.BatchingTxApplier;
	using CatchupPollingProcess = Org.Neo4j.causalclustering.catchup.tx.CatchupPollingProcess;
	using TransactionLogCatchUpFactory = Org.Neo4j.causalclustering.catchup.tx.TransactionLogCatchUpFactory;
	using TxPullClient = Org.Neo4j.causalclustering.catchup.tx.TxPullClient;
	using CausalClusteringSettings = Org.Neo4j.causalclustering.core.CausalClusteringSettings;
	using SupportedProtocolCreator = Org.Neo4j.causalclustering.core.SupportedProtocolCreator;
	using TransactionBackupServiceProvider = Org.Neo4j.causalclustering.core.TransactionBackupServiceProvider;
	using TimerService = Org.Neo4j.causalclustering.core.consensus.schedule.TimerService;
	using CommandIndexTracker = Org.Neo4j.causalclustering.core.state.machines.id.CommandIndexTracker;
	using DiscoveryServiceFactory = Org.Neo4j.causalclustering.discovery.DiscoveryServiceFactory;
	using RemoteMembersResolver = Org.Neo4j.causalclustering.discovery.RemoteMembersResolver;
	using TopologyService = Org.Neo4j.causalclustering.discovery.TopologyService;
	using TopologyServiceMultiRetryStrategy = Org.Neo4j.causalclustering.discovery.TopologyServiceMultiRetryStrategy;
	using TopologyServiceRetryStrategy = Org.Neo4j.causalclustering.discovery.TopologyServiceRetryStrategy;
	using ClusterOverviewProcedure = Org.Neo4j.causalclustering.discovery.procedures.ClusterOverviewProcedure;
	using ReadReplicaRoleProcedure = Org.Neo4j.causalclustering.discovery.procedures.ReadReplicaRoleProcedure;
	using DuplexPipelineWrapperFactory = Org.Neo4j.causalclustering.handlers.DuplexPipelineWrapperFactory;
	using PipelineWrapper = Org.Neo4j.causalclustering.handlers.PipelineWrapper;
	using VoidPipelineWrapperFactory = Org.Neo4j.causalclustering.handlers.VoidPipelineWrapperFactory;
	using CompositeSuspendable = Org.Neo4j.causalclustering.helper.CompositeSuspendable;
	using ExponentialBackoffStrategy = Org.Neo4j.causalclustering.helper.ExponentialBackoffStrategy;
	using MemberId = Org.Neo4j.causalclustering.identity.MemberId;
	using InstalledProtocolHandler = Org.Neo4j.causalclustering.net.InstalledProtocolHandler;
	using Server = Org.Neo4j.causalclustering.net.Server;
	using Org.Neo4j.causalclustering.protocol;
	using NettyPipelineBuilderFactory = Org.Neo4j.causalclustering.protocol.NettyPipelineBuilderFactory;
	using Org.Neo4j.causalclustering.protocol;
	using Protocol_ApplicationProtocols = Org.Neo4j.causalclustering.protocol.Protocol_ApplicationProtocols;
	using Org.Neo4j.causalclustering.protocol;
	using Org.Neo4j.causalclustering.protocol;
	using ApplicationProtocolRepository = Org.Neo4j.causalclustering.protocol.handshake.ApplicationProtocolRepository;
	using ApplicationSupportedProtocols = Org.Neo4j.causalclustering.protocol.handshake.ApplicationSupportedProtocols;
	using HandshakeClientInitializer = Org.Neo4j.causalclustering.protocol.handshake.HandshakeClientInitializer;
	using ModifierProtocolRepository = Org.Neo4j.causalclustering.protocol.handshake.ModifierProtocolRepository;
	using ModifierSupportedProtocols = Org.Neo4j.causalclustering.protocol.handshake.ModifierSupportedProtocols;
	using NoOpUpstreamDatabaseStrategiesLoader = Org.Neo4j.causalclustering.upstream.NoOpUpstreamDatabaseStrategiesLoader;
	using UpstreamDatabaseStrategiesLoader = Org.Neo4j.causalclustering.upstream.UpstreamDatabaseStrategiesLoader;
	using UpstreamDatabaseStrategySelector = Org.Neo4j.causalclustering.upstream.UpstreamDatabaseStrategySelector;
	using ConnectToRandomCoreServerStrategy = Org.Neo4j.causalclustering.upstream.strategies.ConnectToRandomCoreServerStrategy;
	using StoreUtil = Org.Neo4j.com.storecopy.StoreUtil;
	using Predicates = Org.Neo4j.Function.Predicates;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using PlatformModule = Org.Neo4j.Graphdb.factory.module.PlatformModule;
	using AbstractEditionModule = Org.Neo4j.Graphdb.factory.module.edition.AbstractEditionModule;
	using DefaultEditionModule = Org.Neo4j.Graphdb.factory.module.edition.DefaultEditionModule;
	using IdContextFactoryBuilder = Org.Neo4j.Graphdb.factory.module.id.IdContextFactoryBuilder;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using NetworkConnectionTracker = Org.Neo4j.Kernel.api.net.NetworkConnectionTracker;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using SslPolicyLoader = Org.Neo4j.Kernel.configuration.ssl.SslPolicyLoader;
	using EnterpriseBuiltInDbmsProcedures = Org.Neo4j.Kernel.enterprise.builtinprocs.EnterpriseBuiltInDbmsProcedures;
	using EnterpriseBuiltInProcedures = Org.Neo4j.Kernel.enterprise.builtinprocs.EnterpriseBuiltInProcedures;
	using CommitProcessFactory = Org.Neo4j.Kernel.Impl.Api.CommitProcessFactory;
	using ReadOnlyTransactionCommitProcess = Org.Neo4j.Kernel.Impl.Api.ReadOnlyTransactionCommitProcess;
	using TransactionCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionRepresentationCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionRepresentationCommitProcess;
	using DelegatingTokenHolder = Org.Neo4j.Kernel.impl.core.DelegatingTokenHolder;
	using ReadOnlyTokenCreator = Org.Neo4j.Kernel.impl.core.ReadOnlyTokenCreator;
	using ThreadToStatementContextBridge = Org.Neo4j.Kernel.impl.core.ThreadToStatementContextBridge;
	using TokenHolder = Org.Neo4j.Kernel.impl.core.TokenHolder;
	using TokenHolders = Org.Neo4j.Kernel.impl.core.TokenHolders;
	using EnterpriseConstraintSemantics = Org.Neo4j.Kernel.impl.enterprise.EnterpriseConstraintSemantics;
	using EnterpriseEditionModule = Org.Neo4j.Kernel.impl.enterprise.EnterpriseEditionModule;
	using OnlineBackupSettings = Org.Neo4j.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using EnterpriseIdTypeConfigurationProvider = Org.Neo4j.Kernel.impl.enterprise.id.EnterpriseIdTypeConfigurationProvider;
	using ConfigurableIOLimiter = Org.Neo4j.Kernel.impl.enterprise.transaction.log.checkpoint.ConfigurableIOLimiter;
	using ReadOnly = Org.Neo4j.Kernel.impl.factory.ReadOnly;
	using StatementLocksFactorySelector = Org.Neo4j.Kernel.impl.factory.StatementLocksFactorySelector;
	using IndexConfigStore = Org.Neo4j.Kernel.impl.index.IndexConfigStore;
	using DefaultNetworkConnectionTracker = Org.Neo4j.Kernel.impl.net.DefaultNetworkConnectionTracker;
	using PageCacheWarmer = Org.Neo4j.Kernel.impl.pagecache.PageCacheWarmer;
	using Procedures = Org.Neo4j.Kernel.impl.proc.Procedures;
	using TransactionHeaderInformationFactory = Org.Neo4j.Kernel.impl.transaction.TransactionHeaderInformationFactory;
	using TransactionAppender = Org.Neo4j.Kernel.impl.transaction.log.TransactionAppender;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using CheckPointer = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using LogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Org.Neo4j.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using TransactionLogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using DatabaseHealth = Org.Neo4j.Kernel.@internal.DatabaseHealth;
	using KernelData = Org.Neo4j.Kernel.@internal.KernelData;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using Group = Org.Neo4j.Scheduler.Group;
	using StorageEngine = Org.Neo4j.Storageengine.Api.StorageEngine;
	using Clocks = Org.Neo4j.Time.Clocks;
	using UsageData = Org.Neo4j.Udc.UsageData;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.transaction_listen_address;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.discovery.ResolutionResolverFactory.chooseResolver;

	/// <summary>
	/// This implementation of <seealso cref="AbstractEditionModule"/> creates the implementations of services
	/// that are specific to the Enterprise Read Replica edition.
	/// </summary>
	public class EnterpriseReadReplicaEditionModule : DefaultEditionModule
	{
		 private readonly TopologyService _topologyService;
		 private readonly LogProvider _logProvider;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public EnterpriseReadReplicaEditionModule(final org.neo4j.graphdb.factory.module.PlatformModule platformModule, final org.neo4j.causalclustering.discovery.DiscoveryServiceFactory discoveryServiceFactory, org.neo4j.causalclustering.identity.MemberId myself)
		 public EnterpriseReadReplicaEditionModule( PlatformModule platformModule, DiscoveryServiceFactory discoveryServiceFactory, MemberId myself )
		 {
			  LogService logging = platformModule.Logging;

			  IoLimiterConflict = new ConfigurableIOLimiter( platformModule.Config );
			  platformModule.JobScheduler.TopLevelGroupName = "ReadReplica " + myself;

			  Dependencies dependencies = platformModule.Dependencies;
			  Config config = platformModule.Config;
			  FileSystemAbstraction fileSystem = platformModule.FileSystem;
			  PageCache pageCache = platformModule.PageCache;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.io.layout.DatabaseLayout databaseLayout = platformModule.storeLayout.databaseLayout(config.get(org.neo4j.graphdb.factory.GraphDatabaseSettings.active_database));
			  DatabaseLayout databaseLayout = platformModule.StoreLayout.databaseLayout( config.Get( GraphDatabaseSettings.active_database ) );

			  LifeSupport life = platformModule.Life;

			  ThreadToTransactionBridgeConflict = dependencies.SatisfyDependency( new ThreadToStatementContextBridge( GetGlobalAvailabilityGuard( platformModule.Clock, logging, platformModule.Config ) ) );
			  this.AccessCapabilityConflict = new ReadOnly();

			  WatcherServiceFactoryConflict = dir => CreateFileSystemWatcherService( fileSystem, dir, logging, platformModule.JobScheduler, config, FileWatcherFileNameFilter() );
			  dependencies.SatisfyDependencies( WatcherServiceFactoryConflict );

			  ReadReplicaLockManager emptyLockManager = new ReadReplicaLockManager();
			  LocksSupplierConflict = () => emptyLockManager;
			  StatementLocksFactoryProviderConflict = locks => ( new StatementLocksFactorySelector( locks, config, logging ) ).select();

			  IdContextFactoryConflict = IdContextFactoryBuilder.of( new EnterpriseIdTypeConfigurationProvider( config ), platformModule.JobScheduler ).withFileSystem( fileSystem ).build();

			  TokenHoldersProviderConflict = databaseName => new TokenHolders( new DelegatingTokenHolder( new ReadOnlyTokenCreator(), Org.Neo4j.Kernel.impl.core.TokenHolder_Fields.TYPE_PROPERTY_KEY ), new DelegatingTokenHolder(new ReadOnlyTokenCreator(), Org.Neo4j.Kernel.impl.core.TokenHolder_Fields.TYPE_LABEL), new DelegatingTokenHolder(new ReadOnlyTokenCreator(), Org.Neo4j.Kernel.impl.core.TokenHolder_Fields.TYPE_RELATIONSHIP_TYPE) );

			  File contextDirectory = platformModule.StoreLayout.storeDirectory();
			  life.Add( dependencies.SatisfyDependency( new KernelData( fileSystem, pageCache, contextDirectory, config, platformModule.DataSourceManager ) ) );

			  HeaderInformationFactoryConflict = TransactionHeaderInformationFactory.DEFAULT;

			  SchemaWriteGuardConflict = () =>
			  {
			  };

			  TransactionStartTimeoutConflict = config.Get( GraphDatabaseSettings.transaction_start_timeout ).toMillis();

			  ConstraintSemanticsConflict = new EnterpriseConstraintSemantics();

			  PublishEditionInfo( dependencies.ResolveDependency( typeof( UsageData ) ), platformModule.DatabaseInfo, config );
			  CommitProcessFactoryConflict = ReadOnly();

			  ConnectionTrackerConflict = dependencies.SatisfyDependency( CreateConnectionTracker() );

			  _logProvider = platformModule.Logging.InternalLogProvider;
			  LogProvider userLogProvider = platformModule.Logging.UserLogProvider;

			  _logProvider.getLog( this.GetType() ).info(string.Format("Generated new id: {0}", myself));

			  RemoteMembersResolver hostnameResolver = chooseResolver( config, platformModule.Logging );

			  ConfigureDiscoveryService( discoveryServiceFactory, dependencies, config, _logProvider );

			  _topologyService = discoveryServiceFactory.ReadReplicaTopologyService( config, _logProvider, platformModule.JobScheduler, myself, hostnameResolver, ResolveStrategy( config, _logProvider ) );

			  life.Add( dependencies.SatisfyDependency( _topologyService ) );

			  // We need to satisfy the dependency here to keep users of it, such as BoltKernelExtension, happy.
			  dependencies.SatisfyDependency( SslPolicyLoader.create( config, _logProvider ) );

			  DuplexPipelineWrapperFactory pipelineWrapperFactory = pipelineWrapperFactory();
			  PipelineWrapper serverPipelineWrapper = pipelineWrapperFactory.ForServer( config, dependencies, _logProvider, CausalClusteringSettings.ssl_policy );
			  PipelineWrapper clientPipelineWrapper = pipelineWrapperFactory.ForClient( config, dependencies, _logProvider, CausalClusteringSettings.ssl_policy );
			  PipelineWrapper backupServerPipelineWrapper = pipelineWrapperFactory.ForServer( config, dependencies, _logProvider, OnlineBackupSettings.ssl_policy );

			  NettyPipelineBuilderFactory clientPipelineBuilderFactory = new NettyPipelineBuilderFactory( clientPipelineWrapper );
			  NettyPipelineBuilderFactory serverPipelineBuilderFactory = new NettyPipelineBuilderFactory( serverPipelineWrapper );
			  NettyPipelineBuilderFactory backupServerPipelineBuilderFactory = new NettyPipelineBuilderFactory( backupServerPipelineWrapper );

			  SupportedProtocolCreator supportedProtocolCreator = new SupportedProtocolCreator( config, _logProvider );
			  ApplicationSupportedProtocols supportedCatchupProtocols = supportedProtocolCreator.CreateSupportedCatchupProtocol();
			  ICollection<ModifierSupportedProtocols> supportedModifierProtocols = supportedProtocolCreator.CreateSupportedModifierProtocols();

			  ApplicationProtocolRepository applicationProtocolRepository = new ApplicationProtocolRepository( Protocol_ApplicationProtocols.values(), supportedCatchupProtocols );
			  ModifierProtocolRepository modifierProtocolRepository = new ModifierProtocolRepository( Org.Neo4j.causalclustering.protocol.Protocol_ModifierProtocols.values(), supportedModifierProtocols );

			  System.Func<CatchUpResponseHandler, ChannelInitializer<SocketChannel>> channelInitializer = handler =>
			  {
				ProtocolInstallerRepository<ProtocolInstaller.Orientation.Client> protocolInstallerRepository = new ProtocolInstallerRepository<ProtocolInstaller.Orientation.Client>( singletonList( new CatchupProtocolClientInstaller.Factory( clientPipelineBuilderFactory, _logProvider, handler ) ), ModifierProtocolInstaller.allClientInstallers );
				Duration handshakeTimeout = config.Get( CausalClusteringSettings.handshake_timeout );
				return new HandshakeClientInitializer( applicationProtocolRepository, modifierProtocolRepository, protocolInstallerRepository, clientPipelineBuilderFactory, handshakeTimeout, _logProvider, userLogProvider );
			  };

			  long inactivityTimeoutMs = config.Get( CausalClusteringSettings.catch_up_client_inactivity_timeout ).toMillis();

			  CatchUpClient catchUpClient = life.Add( new CatchUpClient( _logProvider, Clocks.systemClock(), inactivityTimeoutMs, channelInitializer ) );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final System.Func<org.neo4j.kernel.internal.DatabaseHealth> databaseHealthSupplier = () -> platformModule.dataSourceManager.getDataSource().getDependencyResolver().resolveDependency(org.neo4j.kernel.internal.DatabaseHealth.class);
			  System.Func<DatabaseHealth> databaseHealthSupplier = () => platformModule.DataSourceManager.DataSource.DependencyResolver.resolveDependency(typeof(DatabaseHealth));

			  StoreFiles storeFiles = new StoreFiles( fileSystem, pageCache );
			  LogFiles logFiles = BuildLocalDatabaseLogFiles( platformModule, fileSystem, databaseLayout, config );

			  LocalDatabase localDatabase = new LocalDatabase( databaseLayout, storeFiles, logFiles, platformModule.DataSourceManager, databaseHealthSupplier, GetGlobalAvailabilityGuard( platformModule.Clock, platformModule.Logging, platformModule.Config ), _logProvider );

			  System.Func<TransactionCommitProcess> writableCommitProcess = () => new TransactionRepresentationCommitProcess(localDatabase.DataSource().DependencyResolver.resolveDependency(typeof(TransactionAppender)), localDatabase.DataSource().DependencyResolver.resolveDependency(typeof(StorageEngine)));

			  LifeSupport txPulling = new LifeSupport();
			  int maxBatchSize = config.Get( CausalClusteringSettings.read_replica_transaction_applier_batch_size );

			  CommandIndexTracker commandIndexTracker = platformModule.Dependencies.satisfyDependency( new CommandIndexTracker() );
			  BatchingTxApplier batchingTxApplier = new BatchingTxApplier( maxBatchSize, () => localDatabase.DataSource().DependencyResolver.resolveDependency(typeof(TransactionIdStore)), writableCommitProcess, platformModule.Monitors, platformModule.Tracers.pageCursorTracerSupplier, platformModule.VersionContextSupplier, commandIndexTracker, _logProvider );

			  TimerService timerService = new TimerService( platformModule.JobScheduler, _logProvider );

			  ExponentialBackoffStrategy storeCopyBackoffStrategy = new ExponentialBackoffStrategy( 1, config.Get( CausalClusteringSettings.store_copy_backoff_max_wait ).toMillis(), TimeUnit.MILLISECONDS );

			  RemoteStore remoteStore = new RemoteStore( platformModule.Logging.InternalLogProvider, fileSystem, platformModule.PageCache, new StoreCopyClient( catchUpClient, platformModule.Monitors, _logProvider, storeCopyBackoffStrategy ), new TxPullClient( catchUpClient, platformModule.Monitors ), new TransactionLogCatchUpFactory(), config, platformModule.Monitors );

			  CopiedStoreRecovery copiedStoreRecovery = new CopiedStoreRecovery( config, platformModule.KernelExtensionFactories, platformModule.PageCache );

			  txPulling.Add( copiedStoreRecovery );

			  CompositeSuspendable servicesToStopOnStoreCopy = new CompositeSuspendable();

			  StoreCopyProcess storeCopyProcess = new StoreCopyProcess( fileSystem, pageCache, localDatabase, copiedStoreRecovery, remoteStore, _logProvider );

			  ConnectToRandomCoreServerStrategy defaultStrategy = new ConnectToRandomCoreServerStrategy();
			  defaultStrategy.Inject( _topologyService, config, _logProvider, myself );

			  UpstreamDatabaseStrategySelector upstreamDatabaseStrategySelector = CreateUpstreamDatabaseStrategySelector( myself, config, _logProvider, _topologyService, defaultStrategy );

			  CatchupPollingProcess catchupProcess = new CatchupPollingProcess( _logProvider, localDatabase, servicesToStopOnStoreCopy, catchUpClient, upstreamDatabaseStrategySelector, timerService, config.Get( CausalClusteringSettings.pull_interval ).toMillis(), batchingTxApplier, platformModule.Monitors, storeCopyProcess, databaseHealthSupplier, _topologyService );
			  dependencies.SatisfyDependencies( catchupProcess );

			  txPulling.Add( batchingTxApplier );
			  txPulling.Add( catchupProcess );
			  txPulling.Add( new WaitForUpToDateStore( catchupProcess, _logProvider ) );

			  ExponentialBackoffStrategy retryStrategy = new ExponentialBackoffStrategy( 1, 30, TimeUnit.SECONDS );
			  life.Add( new ReadReplicaStartupProcess( remoteStore, localDatabase, txPulling, upstreamDatabaseStrategySelector, retryStrategy, _logProvider, platformModule.Logging.UserLogProvider, storeCopyProcess, _topologyService ) );

			  CheckPointerService checkPointerService = new CheckPointerService( () => localDatabase.DataSource().DependencyResolver.resolveDependency(typeof(CheckPointer)), platformModule.JobScheduler, Group.CHECKPOINT );
			  RegularCatchupServerHandler catchupServerHandler = new RegularCatchupServerHandler( platformModule.Monitors, _logProvider, localDatabase.storeId, localDatabase.dataSource, localDatabase.isAvailable, fileSystem, null, checkPointerService );

			  InstalledProtocolHandler installedProtocolHandler = new InstalledProtocolHandler(); // TODO: hook into a procedure
			  Server catchupServer = ( new CatchupServerBuilder( catchupServerHandler ) ).serverHandler( installedProtocolHandler ).catchupProtocols( supportedCatchupProtocols ).modifierProtocols( supportedModifierProtocols ).pipelineBuilder( serverPipelineBuilderFactory ).userLogProvider( userLogProvider ).debugLogProvider( _logProvider ).listenAddress( config.Get( transaction_listen_address ) ).serverName( "catchup-server" ).build();

			  TransactionBackupServiceProvider transactionBackupServiceProvider = new TransactionBackupServiceProvider( _logProvider, userLogProvider, supportedCatchupProtocols, supportedModifierProtocols, backupServerPipelineBuilderFactory, catchupServerHandler, installedProtocolHandler );
			  Optional<Server> backupCatchupServer = transactionBackupServiceProvider.ResolveIfBackupEnabled( config );

			  servicesToStopOnStoreCopy.Add( catchupServer );
			  backupCatchupServer.ifPresent( servicesToStopOnStoreCopy.add );

			  life.Add( catchupServer ); // must start last and stop first, since it handles external requests
			  backupCatchupServer.ifPresent( life.add );
		 }

		 private UpstreamDatabaseStrategySelector CreateUpstreamDatabaseStrategySelector( MemberId myself, Config config, LogProvider logProvider, TopologyService topologyService, ConnectToRandomCoreServerStrategy defaultStrategy )
		 {
			  UpstreamDatabaseStrategiesLoader loader;
			  if ( config.Get( CausalClusteringSettings.multi_dc_license ) )
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

		 protected internal virtual void ConfigureDiscoveryService( DiscoveryServiceFactory discoveryServiceFactory, Dependencies dependencies, Config config, LogProvider logProvider )
		 {
		 }

		 protected internal virtual DuplexPipelineWrapperFactory PipelineWrapperFactory()
		 {
			  return new VoidPipelineWrapperFactory();
		 }

		 internal static System.Predicate<string> FileWatcherFileNameFilter()
		 {
			  return Predicates.any( fileName => fileName.StartsWith( TransactionLogFiles.DEFAULT_NAME ), fileName => fileName.StartsWith( IndexConfigStore.INDEX_DB_FILE_NAME ), filename => filename.StartsWith( StoreUtil.BRANCH_SUBDIRECTORY ), filename => filename.StartsWith( StoreUtil.TEMP_COPY_DIRECTORY_NAME ), filename => filename.EndsWith( PageCacheWarmer.SUFFIX_CACHEPROF ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void registerEditionSpecificProcedures(org.neo4j.kernel.impl.proc.Procedures procedures) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 public override void RegisterEditionSpecificProcedures( Procedures procedures )
		 {
			  procedures.RegisterProcedure( typeof( EnterpriseBuiltInDbmsProcedures ), true );
			  procedures.RegisterProcedure( typeof( EnterpriseBuiltInProcedures ), true );
			  procedures.Register( new ReadReplicaRoleProcedure() );
			  procedures.Register( new ClusterOverviewProcedure( _topologyService, _logProvider ) );
		 }

		 private CommitProcessFactory ReadOnly()
		 {
			  return ( appender, storageEngine, config ) => new ReadOnlyTransactionCommitProcess();
		 }

		 protected internal override NetworkConnectionTracker CreateConnectionTracker()
		 {
			  return new DefaultNetworkConnectionTracker();
		 }

		 public override void CreateSecurityModule( PlatformModule platformModule, Procedures procedures )
		 {
			  EnterpriseEditionModule.createEnterpriseSecurityModule( this, platformModule, procedures );
		 }

		 private static TopologyServiceRetryStrategy ResolveStrategy( Config config, LogProvider logProvider )
		 {
			  long refreshPeriodMillis = config.Get( CausalClusteringSettings.cluster_topology_refresh ).toMillis();
			  int pollingFrequencyWithinRefreshWindow = 2;
			  int numberOfRetries = pollingFrequencyWithinRefreshWindow + 1; // we want to have more retries at the given frequency than there is time in a refresh period
			  return new TopologyServiceMultiRetryStrategy( refreshPeriodMillis / pollingFrequencyWithinRefreshWindow, numberOfRetries, logProvider );
		 }

		 private static LogFiles BuildLocalDatabaseLogFiles( PlatformModule platformModule, FileSystemAbstraction fileSystem, DatabaseLayout databaseLayout, Config config )
		 {
			  try
			  {
					return LogFilesBuilder.activeFilesBuilder( databaseLayout, fileSystem, platformModule.PageCache ).withConfig( config ).build();
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }
	}

}