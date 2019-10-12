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
namespace Org.Neo4j.causalclustering.core.server
{

	using CatchUpClient = Org.Neo4j.causalclustering.catchup.CatchUpClient;
	using CatchupClientBuilder = Org.Neo4j.causalclustering.catchup.CatchupClientBuilder;
	using CatchupServerBuilder = Org.Neo4j.causalclustering.catchup.CatchupServerBuilder;
	using CatchupServerHandler = Org.Neo4j.causalclustering.catchup.CatchupServerHandler;
	using CheckPointerService = Org.Neo4j.causalclustering.catchup.CheckPointerService;
	using RegularCatchupServerHandler = Org.Neo4j.causalclustering.catchup.RegularCatchupServerHandler;
	using CommitStateHelper = Org.Neo4j.causalclustering.catchup.storecopy.CommitStateHelper;
	using CopiedStoreRecovery = Org.Neo4j.causalclustering.catchup.storecopy.CopiedStoreRecovery;
	using LocalDatabase = Org.Neo4j.causalclustering.catchup.storecopy.LocalDatabase;
	using RemoteStore = Org.Neo4j.causalclustering.catchup.storecopy.RemoteStore;
	using StoreCopyClient = Org.Neo4j.causalclustering.catchup.storecopy.StoreCopyClient;
	using StoreCopyProcess = Org.Neo4j.causalclustering.catchup.storecopy.StoreCopyProcess;
	using TransactionLogCatchUpFactory = Org.Neo4j.causalclustering.catchup.tx.TransactionLogCatchUpFactory;
	using TxPullClient = Org.Neo4j.causalclustering.catchup.tx.TxPullClient;
	using ConsensusModule = Org.Neo4j.causalclustering.core.consensus.ConsensusModule;
	using RaftMessages = Org.Neo4j.causalclustering.core.consensus.RaftMessages;
	using PruningScheduler = Org.Neo4j.causalclustering.core.consensus.log.pruning.PruningScheduler;
	using MembershipWaiter = Org.Neo4j.causalclustering.core.consensus.membership.MembershipWaiter;
	using MembershipWaiterLifecycle = Org.Neo4j.causalclustering.core.consensus.membership.MembershipWaiterLifecycle;
	using ClusteringModule = Org.Neo4j.causalclustering.core.state.ClusteringModule;
	using CommandApplicationProcess = Org.Neo4j.causalclustering.core.state.CommandApplicationProcess;
	using CoreLife = Org.Neo4j.causalclustering.core.state.CoreLife;
	using CoreSnapshotService = Org.Neo4j.causalclustering.core.state.CoreSnapshotService;
	using CoreState = Org.Neo4j.causalclustering.core.state.CoreState;
	using LongIndexMarshal = Org.Neo4j.causalclustering.core.state.LongIndexMarshal;
	using RaftLogPruner = Org.Neo4j.causalclustering.core.state.RaftLogPruner;
	using CoreStateMachinesModule = Org.Neo4j.causalclustering.core.state.machines.CoreStateMachinesModule;
	using CoreStateDownloader = Org.Neo4j.causalclustering.core.state.snapshot.CoreStateDownloader;
	using CoreStateDownloaderService = Org.Neo4j.causalclustering.core.state.snapshot.CoreStateDownloaderService;
	using Org.Neo4j.causalclustering.core.state.storage;
	using Org.Neo4j.causalclustering.core.state.storage;
	using CompositeSuspendable = Org.Neo4j.causalclustering.helper.CompositeSuspendable;
	using ExponentialBackoffStrategy = Org.Neo4j.causalclustering.helper.ExponentialBackoffStrategy;
	using Suspendable = Org.Neo4j.causalclustering.helper.Suspendable;
	using Org.Neo4j.causalclustering.messaging;
	using InstalledProtocolHandler = Org.Neo4j.causalclustering.net.InstalledProtocolHandler;
	using Server = Org.Neo4j.causalclustering.net.Server;
	using NettyPipelineBuilderFactory = Org.Neo4j.causalclustering.protocol.NettyPipelineBuilderFactory;
	using ApplicationSupportedProtocols = Org.Neo4j.causalclustering.protocol.handshake.ApplicationSupportedProtocols;
	using ModifierSupportedProtocols = Org.Neo4j.causalclustering.protocol.handshake.ModifierSupportedProtocols;
	using PlatformModule = Org.Neo4j.Graphdb.factory.module.PlatformModule;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using CheckPointer = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using DatabaseHealth = Org.Neo4j.Kernel.@internal.DatabaseHealth;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using Group = Org.Neo4j.Scheduler.Group;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.CausalClusteringSettings.transaction_listen_address;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.time.Clocks.systemClock;

	public class CoreServerModule
	{
		 public const string CLUSTER_ID_NAME = "cluster-id";
		 public const string LAST_FLUSHED_NAME = "last-flushed";
		 public const string DB_NAME = "db-name";

		 public readonly MembershipWaiterLifecycle MembershipWaiterLifecycle;
		 private readonly Server _catchupServer;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("OptionalUsedAsFieldOrParameterType") private final java.util.Optional<org.neo4j.causalclustering.net.Server> backupServer;
		 private readonly Optional<Server> _backupServer;
		 private readonly IdentityModule _identityModule;
		 private readonly CoreStateMachinesModule _coreStateMachinesModule;
		 private readonly ConsensusModule _consensusModule;
		 private readonly ClusteringModule _clusteringModule;
		 private readonly LocalDatabase _localDatabase;
		 private readonly System.Func<DatabaseHealth> _dbHealthSupplier;
		 private readonly CommandApplicationProcess _commandApplicationProcess;
		 private readonly CoreSnapshotService _snapshotService;
		 private readonly CoreStateDownloaderService _downloadService;
		 private readonly Config _config;
		 private readonly JobScheduler _jobScheduler;
		 private readonly LogProvider _logProvider;
		 private readonly LogProvider _userLogProvider;
		 private readonly PlatformModule _platformModule;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public CoreServerModule(org.neo4j.causalclustering.core.IdentityModule identityModule, final org.neo4j.graphdb.factory.module.PlatformModule platformModule, org.neo4j.causalclustering.core.consensus.ConsensusModule consensusModule, org.neo4j.causalclustering.core.state.machines.CoreStateMachinesModule coreStateMachinesModule, org.neo4j.causalclustering.core.state.ClusteringModule clusteringModule, org.neo4j.causalclustering.ReplicationModule replicationModule, org.neo4j.causalclustering.catchup.storecopy.LocalDatabase localDatabase, System.Func<org.neo4j.kernel.internal.DatabaseHealth> dbHealthSupplier, java.io.File clusterStateDirectory, org.neo4j.causalclustering.protocol.NettyPipelineBuilderFactory clientPipelineBuilderFactory, org.neo4j.causalclustering.protocol.NettyPipelineBuilderFactory serverPipelineBuilderFactory, org.neo4j.causalclustering.protocol.NettyPipelineBuilderFactory backupServerPipelineBuilderFactory, org.neo4j.causalclustering.net.InstalledProtocolHandler installedProtocolsHandler)
		 public CoreServerModule( IdentityModule identityModule, PlatformModule platformModule, ConsensusModule consensusModule, CoreStateMachinesModule coreStateMachinesModule, ClusteringModule clusteringModule, ReplicationModule replicationModule, LocalDatabase localDatabase, System.Func<DatabaseHealth> dbHealthSupplier, File clusterStateDirectory, NettyPipelineBuilderFactory clientPipelineBuilderFactory, NettyPipelineBuilderFactory serverPipelineBuilderFactory, NettyPipelineBuilderFactory backupServerPipelineBuilderFactory, InstalledProtocolHandler installedProtocolsHandler )
		 {
			  this._identityModule = identityModule;
			  this._coreStateMachinesModule = coreStateMachinesModule;
			  this._consensusModule = consensusModule;
			  this._clusteringModule = clusteringModule;
			  this._localDatabase = localDatabase;
			  this._dbHealthSupplier = dbHealthSupplier;
			  this._platformModule = platformModule;

			  this._config = platformModule.Config;
			  this._jobScheduler = platformModule.JobScheduler;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.util.Dependencies dependencies = platformModule.dependencies;
			  Dependencies dependencies = platformModule.Dependencies;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.logging.internal.LogService logging = platformModule.logging;
			  LogService logging = platformModule.Logging;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.io.fs.FileSystemAbstraction fileSystem = platformModule.fileSystem;
			  FileSystemAbstraction fileSystem = platformModule.FileSystem;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.lifecycle.LifeSupport life = platformModule.life;
			  LifeSupport life = platformModule.Life;

			  this._logProvider = logging.InternalLogProvider;
			  this._userLogProvider = logging.UserLogProvider;

			  CompositeSuspendable servicesToStopOnStoreCopy = new CompositeSuspendable();

			  StateStorage<long> lastFlushedStorage = platformModule.Life.add( new DurableStateStorage<long>( platformModule.FileSystem, clusterStateDirectory, LAST_FLUSHED_NAME, new LongIndexMarshal(), platformModule.Config.get(CausalClusteringSettings.last_flushed_state_size), _logProvider ) );

			  consensusModule.RaftMembershipManager().RecoverFromIndexSupplier = lastFlushedStorage.getInitialState;

			  CoreState coreState = new CoreState( coreStateMachinesModule.CoreStateMachines, replicationModule.SessionTracker, lastFlushedStorage );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final System.Func<org.neo4j.kernel.internal.DatabaseHealth> databaseHealthSupplier = () -> platformModule.dataSourceManager.getDataSource().getDependencyResolver().resolveDependency(org.neo4j.kernel.internal.DatabaseHealth.class);
			  System.Func<DatabaseHealth> databaseHealthSupplier = () => platformModule.DataSourceManager.DataSource.DependencyResolver.resolveDependency(typeof(DatabaseHealth));
			  _commandApplicationProcess = new CommandApplicationProcess( consensusModule.RaftLog(), platformModule.Config.get(CausalClusteringSettings.state_machine_apply_max_batch_size), platformModule.Config.get(CausalClusteringSettings.state_machine_flush_window_size), databaseHealthSupplier, _logProvider, replicationModule.ProgressTracker, replicationModule.SessionTracker, coreState, consensusModule.InFlightCache(), platformModule.Monitors );

			  platformModule.Dependencies.satisfyDependency( _commandApplicationProcess ); // lastApplied() for CC-robustness

			  this._snapshotService = new CoreSnapshotService( _commandApplicationProcess, coreState, consensusModule.RaftLog(), consensusModule.RaftMachine() );

			  CatchUpClient catchUpClient = CreateCatchupClient( clientPipelineBuilderFactory );
			  CoreStateDownloader downloader = CreateCoreStateDownloader( servicesToStopOnStoreCopy, catchUpClient );

			  this._downloadService = new CoreStateDownloaderService( platformModule.JobScheduler, downloader, _commandApplicationProcess, _logProvider, ( new ExponentialBackoffStrategy( 1, 30, SECONDS ) ).newTimeout(), databaseHealthSupplier, platformModule.Monitors );

			  this.MembershipWaiterLifecycle = CreateMembershipWaiterLifecycle();

			  SupportedProtocolCreator supportedProtocolCreator = new SupportedProtocolCreator( _config, _logProvider );
			  ApplicationSupportedProtocols supportedCatchupProtocols = supportedProtocolCreator.CreateSupportedCatchupProtocol();
			  ICollection<ModifierSupportedProtocols> supportedModifierProtocols = supportedProtocolCreator.CreateSupportedModifierProtocols();

			  CheckPointerService checkPointerService = new CheckPointerService( () => localDatabase.DataSource().DependencyResolver.resolveDependency(typeof(CheckPointer)), _jobScheduler, Group.CHECKPOINT );
			  CatchupServerHandler catchupServerHandler = new RegularCatchupServerHandler( platformModule.Monitors, _logProvider, localDatabase.storeId, localDatabase.dataSource, localDatabase.isAvailable, fileSystem, _snapshotService, checkPointerService );

			  _catchupServer = ( new CatchupServerBuilder( catchupServerHandler ) ).serverHandler( installedProtocolsHandler ).catchupProtocols( supportedCatchupProtocols ).modifierProtocols( supportedModifierProtocols ).pipelineBuilder( serverPipelineBuilderFactory ).userLogProvider( _userLogProvider ).debugLogProvider( _logProvider ).listenAddress( _config.get( transaction_listen_address ) ).serverName( "catchup-server" ).build();

			  TransactionBackupServiceProvider transactionBackupServiceProvider = new TransactionBackupServiceProvider( _logProvider, _userLogProvider, supportedCatchupProtocols, supportedModifierProtocols, backupServerPipelineBuilderFactory, catchupServerHandler, installedProtocolsHandler );

			  _backupServer = transactionBackupServiceProvider.ResolveIfBackupEnabled( _config );

			  RaftLogPruner raftLogPruner = new RaftLogPruner( consensusModule.RaftMachine(), _commandApplicationProcess, platformModule.Clock );
			  dependencies.SatisfyDependency( raftLogPruner );

			  life.Add( new PruningScheduler( raftLogPruner, _jobScheduler, _config.get( CausalClusteringSettings.raft_log_pruning_frequency ).toMillis(), _logProvider ) );

			  servicesToStopOnStoreCopy.Add( this._catchupServer );
			  _backupServer.ifPresent( servicesToStopOnStoreCopy.add );
		 }

		 private CatchUpClient CreateCatchupClient( NettyPipelineBuilderFactory clientPipelineBuilderFactory )
		 {
			  SupportedProtocolCreator supportedProtocolCreator = new SupportedProtocolCreator( _config, _logProvider );
			  ApplicationSupportedProtocols supportedCatchupProtocols = supportedProtocolCreator.CreateSupportedCatchupProtocol();
			  ICollection<ModifierSupportedProtocols> supportedModifierProtocols = supportedProtocolCreator.CreateSupportedModifierProtocols();
			  Duration handshakeTimeout = _config.get( CausalClusteringSettings.handshake_timeout );

			  CatchUpClient catchUpClient = ( new CatchupClientBuilder( supportedCatchupProtocols, supportedModifierProtocols, clientPipelineBuilderFactory, handshakeTimeout, _logProvider, _userLogProvider, systemClock() ) ).build();
			  _platformModule.life.add( catchUpClient );
			  return catchUpClient;
		 }

		 private CoreStateDownloader CreateCoreStateDownloader( Suspendable servicesToSuspendOnStoreCopy, CatchUpClient catchUpClient )
		 {
			  ExponentialBackoffStrategy storeCopyBackoffStrategy = new ExponentialBackoffStrategy( 1, _config.get( CausalClusteringSettings.store_copy_backoff_max_wait ).toMillis(), TimeUnit.MILLISECONDS );

			  RemoteStore remoteStore = new RemoteStore( _logProvider, _platformModule.fileSystem, _platformModule.pageCache, new StoreCopyClient( catchUpClient, _platformModule.monitors, _logProvider, storeCopyBackoffStrategy ), new TxPullClient( catchUpClient, _platformModule.monitors ), new TransactionLogCatchUpFactory(), _config, _platformModule.monitors );

			  CopiedStoreRecovery copiedStoreRecovery = _platformModule.life.add( new CopiedStoreRecovery( _platformModule.config, _platformModule.kernelExtensionFactories, _platformModule.pageCache ) );

			  StoreCopyProcess storeCopyProcess = new StoreCopyProcess( _platformModule.fileSystem, _platformModule.pageCache, _localDatabase, copiedStoreRecovery, remoteStore, _logProvider );

			  CommitStateHelper commitStateHelper = new CommitStateHelper( _platformModule.pageCache, _platformModule.fileSystem, _config );
			  return new CoreStateDownloader( _localDatabase, servicesToSuspendOnStoreCopy, remoteStore, catchUpClient, _logProvider, storeCopyProcess, _coreStateMachinesModule.coreStateMachines, _snapshotService, commitStateHelper );
		 }

		 private MembershipWaiterLifecycle CreateMembershipWaiterLifecycle()
		 {
			  long electionTimeout = _config.get( CausalClusteringSettings.leader_election_timeout ).toMillis();
			  MembershipWaiter membershipWaiter = new MembershipWaiter( _identityModule.myself(), _jobScheduler, _dbHealthSupplier, electionTimeout * 4, _logProvider, _platformModule.monitors );
			  long joinCatchupTimeout = _config.get( CausalClusteringSettings.join_catch_up_timeout ).toMillis();
			  return new MembershipWaiterLifecycle( membershipWaiter, joinCatchupTimeout, _consensusModule.raftMachine(), _logProvider );
		 }

		 public virtual Server CatchupServer()
		 {
			  return _catchupServer;
		 }

		 public virtual Optional<Server> BackupServer()
		 {
			  return _backupServer;
		 }

		 public virtual CoreLife CreateCoreLife<T1>( LifecycleMessageHandler<T1> handler )
		 {
			  return new CoreLife( _consensusModule.raftMachine(), _localDatabase, _clusteringModule.clusterBinder(), _commandApplicationProcess, _coreStateMachinesModule.coreStateMachines, handler, _snapshotService, _downloadService );
		 }

		 public virtual CommandApplicationProcess CommandApplicationProcess()
		 {
			  return _commandApplicationProcess;
		 }

		 public virtual CoreStateDownloaderService DownloadService()
		 {
			  return _downloadService;
		 }
	}

}