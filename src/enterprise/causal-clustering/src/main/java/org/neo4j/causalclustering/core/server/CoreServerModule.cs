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
namespace Neo4Net.causalclustering.core.server
{

	using CatchUpClient = Neo4Net.causalclustering.catchup.CatchUpClient;
	using CatchupClientBuilder = Neo4Net.causalclustering.catchup.CatchupClientBuilder;
	using CatchupServerBuilder = Neo4Net.causalclustering.catchup.CatchupServerBuilder;
	using CatchupServerHandler = Neo4Net.causalclustering.catchup.CatchupServerHandler;
	using CheckPointerService = Neo4Net.causalclustering.catchup.CheckPointerService;
	using RegularCatchupServerHandler = Neo4Net.causalclustering.catchup.RegularCatchupServerHandler;
	using CommitStateHelper = Neo4Net.causalclustering.catchup.storecopy.CommitStateHelper;
	using CopiedStoreRecovery = Neo4Net.causalclustering.catchup.storecopy.CopiedStoreRecovery;
	using LocalDatabase = Neo4Net.causalclustering.catchup.storecopy.LocalDatabase;
	using RemoteStore = Neo4Net.causalclustering.catchup.storecopy.RemoteStore;
	using StoreCopyClient = Neo4Net.causalclustering.catchup.storecopy.StoreCopyClient;
	using StoreCopyProcess = Neo4Net.causalclustering.catchup.storecopy.StoreCopyProcess;
	using TransactionLogCatchUpFactory = Neo4Net.causalclustering.catchup.tx.TransactionLogCatchUpFactory;
	using TxPullClient = Neo4Net.causalclustering.catchup.tx.TxPullClient;
	using ConsensusModule = Neo4Net.causalclustering.core.consensus.ConsensusModule;
	using RaftMessages = Neo4Net.causalclustering.core.consensus.RaftMessages;
	using PruningScheduler = Neo4Net.causalclustering.core.consensus.log.pruning.PruningScheduler;
	using MembershipWaiter = Neo4Net.causalclustering.core.consensus.membership.MembershipWaiter;
	using MembershipWaiterLifecycle = Neo4Net.causalclustering.core.consensus.membership.MembershipWaiterLifecycle;
	using ClusteringModule = Neo4Net.causalclustering.core.state.ClusteringModule;
	using CommandApplicationProcess = Neo4Net.causalclustering.core.state.CommandApplicationProcess;
	using CoreLife = Neo4Net.causalclustering.core.state.CoreLife;
	using CoreSnapshotService = Neo4Net.causalclustering.core.state.CoreSnapshotService;
	using CoreState = Neo4Net.causalclustering.core.state.CoreState;
	using LongIndexMarshal = Neo4Net.causalclustering.core.state.LongIndexMarshal;
	using RaftLogPruner = Neo4Net.causalclustering.core.state.RaftLogPruner;
	using CoreStateMachinesModule = Neo4Net.causalclustering.core.state.machines.CoreStateMachinesModule;
	using CoreStateDownloader = Neo4Net.causalclustering.core.state.snapshot.CoreStateDownloader;
	using CoreStateDownloaderService = Neo4Net.causalclustering.core.state.snapshot.CoreStateDownloaderService;
	using Neo4Net.causalclustering.core.state.storage;
	using Neo4Net.causalclustering.core.state.storage;
	using CompositeSuspendable = Neo4Net.causalclustering.helper.CompositeSuspendable;
	using ExponentialBackoffStrategy = Neo4Net.causalclustering.helper.ExponentialBackoffStrategy;
	using Suspendable = Neo4Net.causalclustering.helper.Suspendable;
	using Neo4Net.causalclustering.messaging;
	using InstalledProtocolHandler = Neo4Net.causalclustering.net.InstalledProtocolHandler;
	using Server = Neo4Net.causalclustering.net.Server;
	using NettyPipelineBuilderFactory = Neo4Net.causalclustering.protocol.NettyPipelineBuilderFactory;
	using ApplicationSupportedProtocols = Neo4Net.causalclustering.protocol.handshake.ApplicationSupportedProtocols;
	using ModifierSupportedProtocols = Neo4Net.causalclustering.protocol.handshake.ModifierSupportedProtocols;
	using PlatformModule = Neo4Net.GraphDb.factory.module.PlatformModule;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Config = Neo4Net.Kernel.configuration.Config;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using DatabaseHealth = Neo4Net.Kernel.Internal.DatabaseHealth;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using LogService = Neo4Net.Logging.Internal.LogService;
	using Group = Neo4Net.Scheduler.Group;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.CausalClusteringSettings.transaction_listen_address;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.time.Clocks.systemClock;

	public class CoreServerModule
	{
		 public const string CLUSTER_ID_NAME = "cluster-id";
		 public const string LAST_FLUSHED_NAME = "last-flushed";
		 public const string DB_NAME = "db-name";

		 public readonly MembershipWaiterLifecycle MembershipWaiterLifecycle;
		 private readonly Server _catchupServer;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("OptionalUsedAsFieldOrParameterType") private final java.util.Optional<Neo4Net.causalclustering.net.Server> backupServer;
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
		 private readonly IJobScheduler _jobScheduler;
		 private readonly LogProvider _logProvider;
		 private readonly LogProvider _userLogProvider;
		 private readonly PlatformModule _platformModule;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public CoreServerModule(Neo4Net.causalclustering.core.IdentityModule identityModule, final Neo4Net.graphdb.factory.module.PlatformModule platformModule, Neo4Net.causalclustering.core.consensus.ConsensusModule consensusModule, Neo4Net.causalclustering.core.state.machines.CoreStateMachinesModule coreStateMachinesModule, Neo4Net.causalclustering.core.state.ClusteringModule clusteringModule, Neo4Net.causalclustering.ReplicationModule replicationModule, Neo4Net.causalclustering.catchup.storecopy.LocalDatabase localDatabase, System.Func<Neo4Net.kernel.internal.DatabaseHealth> dbHealthSupplier, java.io.File clusterStateDirectory, Neo4Net.causalclustering.protocol.NettyPipelineBuilderFactory clientPipelineBuilderFactory, Neo4Net.causalclustering.protocol.NettyPipelineBuilderFactory serverPipelineBuilderFactory, Neo4Net.causalclustering.protocol.NettyPipelineBuilderFactory backupServerPipelineBuilderFactory, Neo4Net.causalclustering.net.InstalledProtocolHandler installedProtocolsHandler)
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
//ORIGINAL LINE: final Neo4Net.kernel.impl.util.Dependencies dependencies = platformModule.dependencies;
			  Dependencies dependencies = platformModule.Dependencies;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.logging.internal.LogService logging = platformModule.logging;
			  LogService logging = platformModule.Logging;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.io.fs.FileSystemAbstraction fileSystem = platformModule.fileSystem;
			  FileSystemAbstraction fileSystem = platformModule.FileSystem;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.kernel.lifecycle.LifeSupport life = platformModule.life;
			  LifeSupport life = platformModule.Life;

			  this._logProvider = logging.InternalLogProvider;
			  this._userLogProvider = logging.UserLogProvider;

			  CompositeSuspendable servicesToStopOnStoreCopy = new CompositeSuspendable();

			  StateStorage<long> lastFlushedStorage = platformModule.Life.add( new DurableStateStorage<long>( platformModule.FileSystem, clusterStateDirectory, LAST_FLUSHED_NAME, new LongIndexMarshal(), platformModule.Config.get(CausalClusteringSettings.last_flushed_state_size), _logProvider ) );

			  consensusModule.RaftMembershipManager().RecoverFromIndexSupplier = lastFlushedStorage.getInitialState;

			  CoreState coreState = new CoreState( coreStateMachinesModule.CoreStateMachines, replicationModule.SessionTracker, lastFlushedStorage );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final System.Func<Neo4Net.kernel.internal.DatabaseHealth> databaseHealthSupplier = () -> platformModule.dataSourceManager.getDataSource().getDependencyResolver().resolveDependency(Neo4Net.kernel.internal.DatabaseHealth.class);
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