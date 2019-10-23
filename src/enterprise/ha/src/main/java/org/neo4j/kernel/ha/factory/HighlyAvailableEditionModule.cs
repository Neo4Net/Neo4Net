using System;

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
namespace Neo4Net.Kernel.ha.factory
{
	using InternalLoggerFactory = org.jboss.netty.logging.InternalLoggerFactory;


	using ClusterSettings = Neo4Net.cluster.ClusterSettings;
	using InstanceId = Neo4Net.cluster.InstanceId;
	using ClusterClient = Neo4Net.cluster.client.ClusterClient;
	using ClusterClientModule = Neo4Net.cluster.client.ClusterClientModule;
	using NettyLoggerFactory = Neo4Net.cluster.logging.NettyLoggerFactory;
	using ClusterMemberAvailability = Neo4Net.cluster.member.ClusterMemberAvailability;
	using ClusterMemberEvents = Neo4Net.cluster.member.ClusterMemberEvents;
	using MemberIsAvailable = Neo4Net.cluster.member.paxos.MemberIsAvailable;
	using PaxosClusterMemberAvailability = Neo4Net.cluster.member.paxos.PaxosClusterMemberAvailability;
	using PaxosClusterMemberEvents = Neo4Net.cluster.member.paxos.PaxosClusterMemberEvents;
	using ObjectStreamFactory = Neo4Net.cluster.protocol.atomicbroadcast.ObjectStreamFactory;
	using ClusterConfiguration = Neo4Net.cluster.protocol.cluster.ClusterConfiguration;
	using ClusterListener = Neo4Net.cluster.protocol.cluster.ClusterListener;
	using ElectionCredentialsProvider = Neo4Net.cluster.protocol.election.ElectionCredentialsProvider;
	using NotElectableElectionCredentialsProvider = Neo4Net.cluster.protocol.election.NotElectableElectionCredentialsProvider;
	using Neo4Net.com;
	using RequestMonitor = Neo4Net.com.monitor.RequestMonitor;
	using StoreCopyClientMonitor = Neo4Net.com.storecopy.StoreCopyClientMonitor;
	using StoreUtil = Neo4Net.com.storecopy.StoreUtil;
	using TransactionCommittingResponseUnpacker = Neo4Net.com.storecopy.TransactionCommittingResponseUnpacker;
	using DatabaseManager = Neo4Net.Dbms.database.DatabaseManager;
	using Neo4Net.Functions;
	using Predicates = Neo4Net.Functions.Predicates;
	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using EditionLocksFactories = Neo4Net.GraphDb.factory.EditionLocksFactories;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using PlatformModule = Neo4Net.GraphDb.factory.module.PlatformModule;
	using AbstractEditionModule = Neo4Net.GraphDb.factory.module.edition.AbstractEditionModule;
	using DefaultEditionModule = Neo4Net.GraphDb.factory.module.edition.DefaultEditionModule;
	using DatabaseIdContext = Neo4Net.GraphDb.factory.module.id.DatabaseIdContext;
	using IdContextFactoryBuilder = Neo4Net.GraphDb.factory.module.id.IdContextFactoryBuilder;
	using HostnamePort = Neo4Net.Helpers.HostnamePort;
	using NamedThreadFactory = Neo4Net.Helpers.NamedThreadFactory;
	using DiagnosticsManager = Neo4Net.Internal.Diagnostics.DiagnosticsManager;
	using Kernel = Neo4Net.Kernel.Api.Internal.Kernel;
	using InvalidTransactionTypeKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.InvalidTransactionTypeKernelException;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using NetworkConnectionTracker = Neo4Net.Kernel.api.net.NetworkConnectionTracker;
	using AvailabilityGuard = Neo4Net.Kernel.availability.AvailabilityGuard;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using SslPolicyLoader = Neo4Net.Kernel.configuration.ssl.SslPolicyLoader;
	using EnterpriseBuiltInDbmsProcedures = Neo4Net.Kernel.enterprise.builtinprocs.EnterpriseBuiltInDbmsProcedures;
	using EnterpriseBuiltInProcedures = Neo4Net.Kernel.enterprise.builtinprocs.EnterpriseBuiltInProcedures;
	using Neo4Net.Kernel.ha;
	using ConversationSPI = Neo4Net.Kernel.ha.cluster.ConversationSPI;
	using DefaultConversationSPI = Neo4Net.Kernel.ha.cluster.DefaultConversationSPI;
	using DefaultElectionCredentialsProvider = Neo4Net.Kernel.ha.cluster.DefaultElectionCredentialsProvider;
	using DefaultMasterImplSPI = Neo4Net.Kernel.ha.cluster.DefaultMasterImplSPI;
	using HANewSnapshotFunction = Neo4Net.Kernel.ha.cluster.HANewSnapshotFunction;
	using HighAvailabilityMemberChangeEvent = Neo4Net.Kernel.ha.cluster.HighAvailabilityMemberChangeEvent;
	using HighAvailabilityMemberContext = Neo4Net.Kernel.ha.cluster.HighAvailabilityMemberContext;
	using HighAvailabilityMemberListener = Neo4Net.Kernel.ha.cluster.HighAvailabilityMemberListener;
	using HighAvailabilityMemberState = Neo4Net.Kernel.ha.cluster.HighAvailabilityMemberState;
	using HighAvailabilityMemberStateMachine = Neo4Net.Kernel.ha.cluster.HighAvailabilityMemberStateMachine;
	using SimpleHighAvailabilityMemberContext = Neo4Net.Kernel.ha.cluster.SimpleHighAvailabilityMemberContext;
	using SwitchToMaster = Neo4Net.Kernel.ha.cluster.SwitchToMaster;
	using SwitchToSlave = Neo4Net.Kernel.ha.cluster.SwitchToSlave;
	using SwitchToSlaveBranchThenCopy = Neo4Net.Kernel.ha.cluster.SwitchToSlaveBranchThenCopy;
	using SwitchToSlaveCopyThenBranch = Neo4Net.Kernel.ha.cluster.SwitchToSlaveCopyThenBranch;
	using ClusterMembers = Neo4Net.Kernel.ha.cluster.member.ClusterMembers;
	using HighAvailabilitySlaves = Neo4Net.Kernel.ha.cluster.member.HighAvailabilitySlaves;
	using ObservedClusterMembers = Neo4Net.Kernel.ha.cluster.member.ObservedClusterMembers;
	using CommitProcessSwitcher = Neo4Net.Kernel.ha.cluster.modeswitch.CommitProcessSwitcher;
	using ComponentSwitcherContainer = Neo4Net.Kernel.ha.cluster.modeswitch.ComponentSwitcherContainer;
	using HighAvailabilityModeSwitcher = Neo4Net.Kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher;
	using LabelTokenCreatorSwitcher = Neo4Net.Kernel.ha.cluster.modeswitch.LabelTokenCreatorSwitcher;
	using LockManagerSwitcher = Neo4Net.Kernel.ha.cluster.modeswitch.LockManagerSwitcher;
	using PropertyKeyCreatorSwitcher = Neo4Net.Kernel.ha.cluster.modeswitch.PropertyKeyCreatorSwitcher;
	using RelationshipTypeCreatorSwitcher = Neo4Net.Kernel.ha.cluster.modeswitch.RelationshipTypeCreatorSwitcher;
	using StatementLocksFactorySwitcher = Neo4Net.Kernel.ha.cluster.modeswitch.StatementLocksFactorySwitcher;
	using UpdatePullerSwitcher = Neo4Net.Kernel.ha.cluster.modeswitch.UpdatePullerSwitcher;
	using RequestContextFactory = Neo4Net.Kernel.ha.com.RequestContextFactory;
	using ConversationManager = Neo4Net.Kernel.ha.com.master.ConversationManager;
	using DefaultSlaveFactory = Neo4Net.Kernel.ha.com.master.DefaultSlaveFactory;
	using Master = Neo4Net.Kernel.ha.com.master.Master;
	using MasterImpl = Neo4Net.Kernel.ha.com.master.MasterImpl;
	using MasterServer = Neo4Net.Kernel.ha.com.master.MasterServer;
	using Slave = Neo4Net.Kernel.ha.com.master.Slave;
	using SlaveFactory = Neo4Net.Kernel.ha.com.master.SlaveFactory;
	using Slaves = Neo4Net.Kernel.ha.com.master.Slaves;
	using InvalidEpochExceptionHandler = Neo4Net.Kernel.ha.com.slave.InvalidEpochExceptionHandler;
	using MasterClientResolver = Neo4Net.Kernel.ha.com.slave.MasterClientResolver;
	using SlaveServer = Neo4Net.Kernel.ha.com.slave.SlaveServer;
	using HaIdGeneratorFactory = Neo4Net.Kernel.ha.id.HaIdGeneratorFactory;
	using HaIdReuseEligibility = Neo4Net.Kernel.ha.id.HaIdReuseEligibility;
	using ClusterDatabaseInfoProvider = Neo4Net.Kernel.ha.management.ClusterDatabaseInfoProvider;
	using HighlyAvailableKernelData = Neo4Net.Kernel.ha.management.HighlyAvailableKernelData;
	using CommitPusher = Neo4Net.Kernel.ha.transaction.CommitPusher;
	using OnDiskLastTxIdGetter = Neo4Net.Kernel.ha.transaction.OnDiskLastTxIdGetter;
	using TransactionPropagator = Neo4Net.Kernel.ha.transaction.TransactionPropagator;
	using CommitProcessFactory = Neo4Net.Kernel.Impl.Api.CommitProcessFactory;
	using TransactionCommitProcess = Neo4Net.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionHeaderInformation = Neo4Net.Kernel.Impl.Api.TransactionHeaderInformation;
	using DelegatingTokenHolder = Neo4Net.Kernel.impl.core.DelegatingTokenHolder;
	using LastTxIdGetter = Neo4Net.Kernel.impl.core.LastTxIdGetter;
	using ReadOnlyTokenCreator = Neo4Net.Kernel.impl.core.ReadOnlyTokenCreator;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using TokenCreator = Neo4Net.Kernel.impl.core.TokenCreator;
	using TokenHolder = Neo4Net.Kernel.impl.core.TokenHolder;
	using TokenHolders = Neo4Net.Kernel.impl.core.TokenHolders;
	using EnterpriseConstraintSemantics = Neo4Net.Kernel.impl.enterprise.EnterpriseConstraintSemantics;
	using EnterpriseEditionModule = Neo4Net.Kernel.impl.enterprise.EnterpriseEditionModule;
	using EnterpriseIdTypeConfigurationProvider = Neo4Net.Kernel.impl.enterprise.id.EnterpriseIdTypeConfigurationProvider;
	using ConfigurableIOLimiter = Neo4Net.Kernel.impl.enterprise.transaction.log.checkpoint.ConfigurableIOLimiter;
	using CanWrite = Neo4Net.Kernel.impl.factory.CanWrite;
	using DatabaseInfo = Neo4Net.Kernel.impl.factory.DatabaseInfo;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using ReadOnly = Neo4Net.Kernel.impl.factory.ReadOnly;
	using StatementLocksFactorySelector = Neo4Net.Kernel.impl.factory.StatementLocksFactorySelector;
	using IndexConfigStore = Neo4Net.Kernel.impl.index.IndexConfigStore;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using LocksFactory = Neo4Net.Kernel.impl.locking.LocksFactory;
	using StatementLocksFactory = Neo4Net.Kernel.impl.locking.StatementLocksFactory;
	using DefaultNetworkConnectionTracker = Neo4Net.Kernel.impl.net.DefaultNetworkConnectionTracker;
	using PageCacheWarmer = Neo4Net.Kernel.impl.pagecache.PageCacheWarmer;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using MetaDataStore = Neo4Net.Kernel.impl.store.MetaDataStore;
	using TransactionId = Neo4Net.Kernel.impl.store.TransactionId;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdTypeConfigurationProvider = Neo4Net.Kernel.impl.store.id.configuration.IdTypeConfigurationProvider;
	using TransactionHeaderInformationFactory = Neo4Net.Kernel.impl.transaction.TransactionHeaderInformationFactory;
	using LogicalTransactionStore = Neo4Net.Kernel.impl.transaction.log.LogicalTransactionStore;
	using NoSuchTransactionException = Neo4Net.Kernel.impl.transaction.log.NoSuchTransactionException;
	using ReadableClosablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using TransactionLogFiles = Neo4Net.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using DataSourceManager = Neo4Net.Kernel.impl.transaction.state.DataSourceManager;
	using DatabaseTransactionStats = Neo4Net.Kernel.impl.transaction.stats.DatabaseTransactionStats;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using KernelData = Neo4Net.Kernel.Internal.KernelData;
	using KernelDiagnostics = Neo4Net.Kernel.Internal.KernelDiagnostics;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using ByteCounterMonitor = Neo4Net.Kernel.monitoring.ByteCounterMonitor;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using LogService = Neo4Net.Logging.Internal.LogService;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using StoreId = Neo4Net.Kernel.Api.StorageEngine.StoreId;
	using UsageData = Neo4Net.Udc.UsageData;
	using UsageDataKeys = Neo4Net.Udc.UsageDataKeys;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.EditionLocksFactories.createLockFactory;
	using static Neo4Net.Kernel.impl.transaction.log.TransactionMetadataCache.TransactionMetadata;

	/// <summary>
	/// This implementation of <seealso cref="AbstractEditionModule"/> creates the implementations of services
	/// that are specific to the Enterprise edition.
	/// </summary>
	public class HighlyAvailableEditionModule : DefaultEditionModule
	{
		 public HighAvailabilityMemberStateMachine MemberStateMachine;
		 public ClusterMembers Members;
		 private TokenHolders _tokenHolders;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public HighlyAvailableEditionModule(final org.Neo4Net.graphdb.factory.module.PlatformModule platformModule)
		 public HighlyAvailableEditionModule( PlatformModule platformModule )
		 {
			  IoLimiterConflict = new ConfigurableIOLimiter( platformModule.Config );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.lifecycle.LifeSupport life = platformModule.life;
			  LifeSupport life = platformModule.Life;
			  life.Add( platformModule.DataSourceManager );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.lifecycle.LifeSupport paxosLife = new org.Neo4Net.kernel.lifecycle.LifeSupport();
			  LifeSupport paxosLife = new LifeSupport();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.lifecycle.LifeSupport clusteringLife = new org.Neo4Net.kernel.lifecycle.LifeSupport();
			  LifeSupport clusteringLife = new LifeSupport();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.io.fs.FileSystemAbstraction fs = platformModule.fileSystem;
			  FileSystemAbstraction fs = platformModule.FileSystem;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.configuration.Config config = platformModule.config;
			  Config config = platformModule.Config;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.util.Dependencies dependencies = platformModule.dependencies;
			  Dependencies dependencies = platformModule.Dependencies;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.logging.internal.LogService logging = platformModule.logging;
			  LogService logging = platformModule.Logging;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.monitoring.Monitors monitors = platformModule.monitors;
			  Monitors monitors = platformModule.Monitors;

			  this.AccessCapabilityConflict = config.Get( GraphDatabaseSettings.read_only ) ? new ReadOnly() : new CanWrite();

			  WatcherServiceFactoryConflict = dir => CreateFileSystemWatcherService( platformModule.FileSystem, dir, logging, platformModule.JobScheduler, config, FileWatcherFileNameFilter() );

			  ThreadToTransactionBridgeConflict = dependencies.SatisfyDependency( new ThreadToStatementContextBridge( GetGlobalAvailabilityGuard( platformModule.Clock, logging, platformModule.Config ) ) );

			  // Set Netty logger
			  InternalLoggerFactory.DefaultFactory = new NettyLoggerFactory( logging.InternalLogProvider );

			  DatabaseLayout databaseLayout = platformModule.StoreLayout.databaseLayout( config.Get( GraphDatabaseSettings.active_database ) );
			  life.Add( new BranchedDataMigrator( databaseLayout.DatabaseDirectory() ) );
			  DelegateInvocationHandler<Master> masterDelegateInvocationHandler = new DelegateInvocationHandler<Master>( typeof( Master ) );
			  Master master = ( Master ) newProxyInstance( typeof( Master ).ClassLoader, new Type[]{ typeof( Master ) }, masterDelegateInvocationHandler );
			  InstanceId serverId = config.Get( ClusterSettings.server_id );

			  RequestContextFactory requestContextFactory = dependencies.SatisfyDependency( new RequestContextFactory( serverId.ToIntegerIndex(), () => ResolveDatabaseDependency(platformModule, typeof(TransactionIdStore)) ) );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long idReuseSafeZone = config.get(org.Neo4Net.kernel.ha.HaSettings.id_reuse_safe_zone_time).toMillis();
			  long idReuseSafeZone = config.Get( HaSettings.id_reuse_safe_zone_time ).toMillis();
			  TransactionCommittingResponseUnpacker responseUnpacker = dependencies.SatisfyDependency( new TransactionCommittingResponseUnpacker( dependencies, config.Get( HaSettings.pull_apply_batch_size ), idReuseSafeZone ) );

			  System.Func<Kernel> kernelProvider = () => ResolveDatabaseDependency(platformModule, typeof(Kernel));

			  TransactionStartTimeoutConflict = config.Get( HaSettings.state_switch_timeout ).toMillis();

			  DelegateInvocationHandler<ClusterMemberEvents> clusterEventsDelegateInvocationHandler = new DelegateInvocationHandler<ClusterMemberEvents>( typeof( ClusterMemberEvents ) );
			  DelegateInvocationHandler<HighAvailabilityMemberContext> memberContextDelegateInvocationHandler = new DelegateInvocationHandler<HighAvailabilityMemberContext>( typeof( HighAvailabilityMemberContext ) );
			  DelegateInvocationHandler<ClusterMemberAvailability> clusterMemberAvailabilityDelegateInvocationHandler = new DelegateInvocationHandler<ClusterMemberAvailability>( typeof( ClusterMemberAvailability ) );

			  ClusterMemberEvents clusterEvents = dependencies.SatisfyDependency( ( ClusterMemberEvents ) newProxyInstance( typeof( ClusterMemberEvents ).ClassLoader, new Type[]{ typeof( ClusterMemberEvents ), typeof( Lifecycle ) }, clusterEventsDelegateInvocationHandler ) );

			  HighAvailabilityMemberContext memberContext = ( HighAvailabilityMemberContext ) newProxyInstance( typeof( HighAvailabilityMemberContext ).ClassLoader, new Type[]{ typeof( HighAvailabilityMemberContext ) }, memberContextDelegateInvocationHandler );
			  ClusterMemberAvailability clusterMemberAvailability = dependencies.SatisfyDependency( ( ClusterMemberAvailability ) newProxyInstance( typeof( ClusterMemberAvailability ).ClassLoader, new Type[]{ typeof( ClusterMemberAvailability ) }, clusterMemberAvailabilityDelegateInvocationHandler ) );

			  // TODO There's a cyclical dependency here that should be fixed
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicReference<org.Neo4Net.kernel.ha.cluster.HighAvailabilityMemberStateMachine> electionProviderRef = new java.util.concurrent.atomic.AtomicReference<>();
			  AtomicReference<HighAvailabilityMemberStateMachine> electionProviderRef = new AtomicReference<HighAvailabilityMemberStateMachine>();
			  OnDiskLastTxIdGetter lastTxIdGetter = new OnDiskLastTxIdGetter( () => ResolveDatabaseDependency(platformModule, typeof(TransactionIdStore)).LastCommittedTransactionId );
			  ElectionCredentialsProvider electionCredentialsProvider = config.Get( HaSettings.slave_only ) ? new NotElectableElectionCredentialsProvider() : new DefaultElectionCredentialsProvider(config.Get(ClusterSettings.server_id), lastTxIdGetter, () => electionProviderRef.get().CurrentState);

			  ObjectStreamFactory objectStreamFactory = new ObjectStreamFactory();

			  ClusterClientModule clusterClientModule = new ClusterClientModule( clusteringLife, dependencies, monitors, config, logging, electionCredentialsProvider );
			  ClusterClient clusterClient = clusterClientModule.ClusterClient;
			  PaxosClusterMemberEvents localClusterEvents = new PaxosClusterMemberEvents(clusterClient, clusterClient, clusterClient, clusterClient, logging.InternalLogProvider, item =>
			  {
						  foreach ( MemberIsAvailable member in item.CurrentAvailableMembers )
						  {
								if ( member.RoleUri.Scheme.Equals( "ha" ) )
								{
									 if ( HighAvailabilityModeSwitcher.getServerId( member.RoleUri ).Equals( platformModule.Config.get( ClusterSettings.server_id ) ) )
									 {
										  logging.GetInternalLog( typeof( PaxosClusterMemberEvents ) ).error( string.Format( "Instance " + "{0} has" + " the same serverId as ours ({1}) - will not " + "join this cluster", member.RoleUri, config.Get( ClusterSettings.server_id ).toIntegerIndex() ) );
										  return true;
									 }
								}
						  }
						  return true;
			  }, new HANewSnapshotFunction(), objectStreamFactory, objectStreamFactory, platformModule.Monitors.newMonitor(typeof(NamedThreadFactory.Monitor)));

			  // Force a reelection after we enter the cluster
			  // and when that election is finished refresh the snapshot
			  clusterClient.AddClusterListener( new ClusterListener_AdapterAnonymousInnerClass( this, clusterClient ) );

			  HighAvailabilityMemberContext localMemberContext = new SimpleHighAvailabilityMemberContext( clusterClient.ServerId, config.Get( HaSettings.slave_only ) );
			  PaxosClusterMemberAvailability localClusterMemberAvailability = new PaxosClusterMemberAvailability( clusterClient.ServerId, clusterClient, clusterClient, logging.InternalLogProvider, objectStreamFactory, objectStreamFactory );

			  memberContextDelegateInvocationHandler.Delegate = localMemberContext;
			  clusterEventsDelegateInvocationHandler.Delegate = localClusterEvents;
			  clusterMemberAvailabilityDelegateInvocationHandler.Delegate = localClusterMemberAvailability;

			  ObservedClusterMembers observedMembers = new ObservedClusterMembers( logging.InternalLogProvider, clusterClient, clusterClient, clusterEvents, config.Get( ClusterSettings.server_id ) );

			  AvailabilityGuard globalAvailabilityGuard = GetGlobalAvailabilityGuard( platformModule.Clock, platformModule.Logging, platformModule.Config );
			  MemberStateMachine = new HighAvailabilityMemberStateMachine( memberContext, globalAvailabilityGuard, observedMembers, clusterEvents, clusterClient, logging.InternalLogProvider );

			  Members = dependencies.SatisfyDependency( new ClusterMembers( observedMembers, MemberStateMachine ) );

			  dependencies.SatisfyDependency( MemberStateMachine );
			  paxosLife.Add( MemberStateMachine );
			  electionProviderRef.set( MemberStateMachine );

			  HighAvailabilityLogger highAvailabilityLogger = new HighAvailabilityLogger( logging.UserLogProvider, config.Get( ClusterSettings.server_id ) );
			  globalAvailabilityGuard.AddListener( highAvailabilityLogger );
			  clusterEvents.AddClusterMemberListener( highAvailabilityLogger );
			  clusterClient.AddClusterListener( highAvailabilityLogger );

			  paxosLife.Add( ( Lifecycle )clusterEvents );
			  paxosLife.Add( localClusterMemberAvailability );

			  EnterpriseIdTypeConfigurationProvider idTypeConfigurationProvider = new EnterpriseIdTypeConfigurationProvider( config );
			  HaIdGeneratorFactory editionIdGeneratorFactory = ( HaIdGeneratorFactory ) CreateIdGeneratorFactory( masterDelegateInvocationHandler, logging.InternalLogProvider, requestContextFactory, fs, idTypeConfigurationProvider );
			  HaIdReuseEligibility eligibleForIdReuse = new HaIdReuseEligibility( Members, platformModule.Clock, idReuseSafeZone );
			  IdContextFactoryConflict = IdContextFactoryBuilder.of( idTypeConfigurationProvider, platformModule.JobScheduler ).withIdGenerationFactoryProvider( any => editionIdGeneratorFactory ).withIdReuseEligibility( eligibleForIdReuse ).build();
			  DatabaseIdContext idContext = IdContextFactoryConflict.createIdContext( config.Get( GraphDatabaseSettings.active_database ) );

			  // TODO There's a cyclical dependency here that should be fixed
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicReference<org.Neo4Net.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher> exceptionHandlerRef = new java.util.concurrent.atomic.AtomicReference<>();
			  AtomicReference<HighAvailabilityModeSwitcher> exceptionHandlerRef = new AtomicReference<HighAvailabilityModeSwitcher>();
			  InvalidEpochExceptionHandler invalidEpochHandler = () => exceptionHandlerRef.get().forceElections();

			  // At the point in time the LogEntryReader hasn't been instantiated yet. The StorageEngine is responsible
			  // for instantiating the CommandReaderFactory, required by a LogEntryReader. The StorageEngine is
			  // created in the DataSourceModule, after this module.
			  //   That is OK though because all users of it, instantiated below, will not use it right away,
			  // but merely provide a way to get access to it. That's why this is a Supplier and will be asked
			  // later, after the data source module and all that have started.
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({"deprecation", "unchecked"}) System.Func<org.Neo4Net.kernel.impl.transaction.log.entry.LogEntryReader<org.Neo4Net.kernel.impl.transaction.log.ReadableClosablePositionAwareChannel>> logEntryReader = () -> resolveDatabaseDependency(platformModule, org.Neo4Net.kernel.impl.transaction.log.entry.LogEntryReader.class);
			  System.Func<LogEntryReader<ReadableClosablePositionAwareChannel>> logEntryReader = () => ResolveDatabaseDependency(platformModule, typeof(LogEntryReader));

			  MasterClientResolver masterClientResolver = new MasterClientResolver( logging.InternalLogProvider, responseUnpacker, invalidEpochHandler, ( int ) config.Get( HaSettings.read_timeout ).toMillis(), (int) config.Get(HaSettings.lock_read_timeout).toMillis(), config.Get(HaSettings.max_concurrent_channels_per_slave), config.Get(HaSettings.com_chunk_size).intValue(), logEntryReader );

			  LastUpdateTime lastUpdateTime = new LastUpdateTime();

			  DelegateInvocationHandler<UpdatePuller> updatePullerDelegate = new DelegateInvocationHandler<UpdatePuller>( typeof( UpdatePuller ) );
			  UpdatePuller updatePullerProxy = ( UpdatePuller ) Proxy.newProxyInstance( typeof( UpdatePuller ).ClassLoader, new Type[]{ typeof( UpdatePuller ) }, updatePullerDelegate );
			  dependencies.SatisfyDependency( updatePullerProxy );

			  PullerFactory pullerFactory = new PullerFactory( requestContextFactory, master, lastUpdateTime, logging.InternalLogProvider, serverId, invalidEpochHandler, config.Get( HaSettings.pull_interval ).toMillis(), platformModule.JobScheduler, dependencies, globalAvailabilityGuard, MemberStateMachine, monitors, config );

			  dependencies.SatisfyDependency( paxosLife.Add( pullerFactory.CreateObligationFulfiller( updatePullerProxy ) ) );

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  System.Func<Slave, SlaveServer> slaveServerFactory = slave => new SlaveServer( slave, SlaveServerConfig( config ), logging.InternalLogProvider, monitors.NewMonitor( typeof( ByteCounterMonitor ), typeof( SlaveServer ).FullName ), monitors.NewMonitor( typeof( RequestMonitor ), typeof( SlaveServer ).FullName ) );

			  SwitchToSlave switchToSlaveInstance = ChooseSwitchToSlaveStrategy( platformModule, config, dependencies, logging, monitors, masterDelegateInvocationHandler, requestContextFactory, clusterMemberAvailability, masterClientResolver, updatePullerProxy, pullerFactory, slaveServerFactory, editionIdGeneratorFactory, databaseLayout );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.function.Factory<org.Neo4Net.kernel.ha.com.master.MasterImpl.SPI> masterSPIFactory = () -> new org.Neo4Net.kernel.ha.cluster.DefaultMasterImplSPI(resolveDatabaseDependency(platformModule, org.Neo4Net.kernel.impl.factory.GraphDatabaseFacade.class), platformModule.fileSystem, platformModule.monitors, tokenHolders, idContext.getIdGeneratorFactory(), resolveDatabaseDependency(platformModule, org.Neo4Net.kernel.impl.api.TransactionCommitProcess.class), resolveDatabaseDependency(platformModule, org.Neo4Net.kernel.impl.transaction.log.checkpoint.CheckPointer.class), resolveDatabaseDependency(platformModule, org.Neo4Net.kernel.impl.transaction.log.TransactionIdStore.class), resolveDatabaseDependency(platformModule, org.Neo4Net.kernel.impl.transaction.log.LogicalTransactionStore.class), platformModule.dataSourceManager.getDataSource(), logging.getInternalLogProvider());
			  IFactory<MasterImpl.SPI> masterSPIFactory = () => new DefaultMasterImplSPI(ResolveDatabaseDependency(platformModule, typeof(GraphDatabaseFacade)), platformModule.FileSystem, platformModule.Monitors, _tokenHolders, idContext.IdGeneratorFactory, ResolveDatabaseDependency(platformModule, typeof(TransactionCommitProcess)), ResolveDatabaseDependency(platformModule, typeof(CheckPointer)), ResolveDatabaseDependency(platformModule, typeof(TransactionIdStore)), ResolveDatabaseDependency(platformModule, typeof(LogicalTransactionStore)), platformModule.DataSourceManager.DataSource, logging.InternalLogProvider);

			  System.Func<Locks, ConversationSPI> conversationSPIFactory = locks => new DefaultConversationSPI( locks, platformModule.JobScheduler );
			  System.Func<Locks, ConversationManager> conversationManagerFactory = locks => new ConversationManager( conversationSPIFactory( locks ), config );

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			  System.Func<ConversationManager, LifeSupport, Master> masterFactory = ( conversationManager, life1 ) => life1.add( new MasterImpl( masterSPIFactory.NewInstance(), conversationManager, monitors.NewMonitor(typeof(MasterImpl.Monitor), typeof(MasterImpl).FullName), config ) );

			  System.Func<Master, ConversationManager, MasterServer> masterServerFactory = ( master1, conversationManager ) =>
			  {
						  TransactionChecksumLookup txChecksumLookup = new TransactionChecksumLookup( ResolveDatabaseDependency( platformModule, typeof( TransactionIdStore ) ), ResolveDatabaseDependency( platformModule, typeof( LogicalTransactionStore ) ) );

						  return new MasterServer( master1, logging.InternalLogProvider, MasterServerConfig( config ), new BranchDetectingTxVerifier( logging.InternalLogProvider, txChecksumLookup ), monitors.NewMonitor( typeof( ByteCounterMonitor ), typeof( MasterServer ).FullName ), monitors.NewMonitor( typeof( RequestMonitor ), typeof( MasterServer ).FullName ), conversationManager, logEntryReader() );
			  };

			  SwitchToMaster switchToMasterInstance = new SwitchToMaster( logging, editionIdGeneratorFactory, config, dependencies.ProvideDependency( typeof( SlaveFactory ) ), conversationManagerFactory, masterFactory, masterServerFactory, masterDelegateInvocationHandler, clusterMemberAvailability, platformModule.DataSourceManager.getDataSource );

			  ComponentSwitcherContainer componentSwitcherContainer = new ComponentSwitcherContainer();
			  System.Func<StoreId> storeIdSupplier = () => platformModule.DataSourceManager.DataSource.StoreId;

			  HighAvailabilityModeSwitcher highAvailabilityModeSwitcher = new HighAvailabilityModeSwitcher( switchToSlaveInstance, switchToMasterInstance, clusterClient, clusterMemberAvailability, clusterClient, storeIdSupplier, config.Get( ClusterSettings.server_id ), componentSwitcherContainer, platformModule.DataSourceManager, logging );

			  exceptionHandlerRef.set( highAvailabilityModeSwitcher );

			  clusterClient.AddBindingListener( highAvailabilityModeSwitcher );
			  MemberStateMachine.addHighAvailabilityMemberListener( highAvailabilityModeSwitcher );

			  paxosLife.Add( highAvailabilityModeSwitcher );

			  componentSwitcherContainer.Add( new UpdatePullerSwitcher( updatePullerDelegate, pullerFactory ) );

			  life.Add( requestContextFactory );
			  life.Add( responseUnpacker );

			  platformModule.DiagnosticsManager.appendProvider( new HighAvailabilityDiagnostics( MemberStateMachine, clusterClient ) );

			  dependencies.SatisfyDependency( SslPolicyLoader.create( config, logging.InternalLogProvider ) ); // for bolt and web server

			  // Create HA services
			  LocksFactory lockFactory = createLockFactory( config, logging );
			  LocksSupplierConflict = () => CreateLockManager(lockFactory, componentSwitcherContainer, config, masterDelegateInvocationHandler, requestContextFactory, globalAvailabilityGuard, platformModule.Clock, logging);
			  StatementLocksFactoryProviderConflict = locks => CreateStatementLocksFactory( locks, componentSwitcherContainer, config, logging );

			  DelegatingTokenHolder propertyKeyTokenHolder = new DelegatingTokenHolder( CreatePropertyKeyCreator( config, componentSwitcherContainer, masterDelegateInvocationHandler, requestContextFactory, kernelProvider ), Neo4Net.Kernel.impl.core.TokenHolder_Fields.TYPE_PROPERTY_KEY );
			  DelegatingTokenHolder labelTokenHolder = new DelegatingTokenHolder( CreateLabelIdCreator( config, componentSwitcherContainer, masterDelegateInvocationHandler, requestContextFactory, kernelProvider ), Neo4Net.Kernel.impl.core.TokenHolder_Fields.TYPE_LABEL );
			  DelegatingTokenHolder relationshipTypeTokenHolder = new DelegatingTokenHolder( CreateRelationshipTypeCreator( config, componentSwitcherContainer, masterDelegateInvocationHandler, requestContextFactory, kernelProvider ), Neo4Net.Kernel.impl.core.TokenHolder_Fields.TYPE_RELATIONSHIP_TYPE );

			  // HA will only support a single token holder
			  _tokenHolders = new TokenHolders( propertyKeyTokenHolder, labelTokenHolder, relationshipTypeTokenHolder );
			  TokenHoldersProviderConflict = ignored => _tokenHolders;

			  dependencies.SatisfyDependency( CreateKernelData( config, platformModule.DataSourceManager, Members, fs, platformModule.PageCache, platformModule.StoreLayout.storeDirectory(), lastUpdateTime, lastTxIdGetter, life ) );

			  CommitProcessFactoryConflict = CreateCommitProcessFactory( dependencies, logging, monitors, config, paxosLife, clusterClient, Members, platformModule.JobScheduler, master, requestContextFactory, componentSwitcherContainer, logEntryReader );

			  HeaderInformationFactoryConflict = CreateHeaderInformationFactory( memberContext );

			  SchemaWriteGuardConflict = () =>
			  {
				if ( !MemberStateMachine.Master )
				{
					 throw new InvalidTransactionTypeKernelException( "Modifying the database schema can only be done on the master server, " + "this server is a slave. Please issue schema modification commands directly to the master." );
				}
			  };

			  config.Augment( GraphDatabaseSettings.allow_upgrade, Settings.FALSE );

			  ConstraintSemanticsConflict = new EnterpriseConstraintSemantics();

			  ConnectionTrackerConflict = dependencies.SatisfyDependency( CreateConnectionTracker() );

			  RegisterRecovery( platformModule.DatabaseInfo, dependencies, logging, platformModule );

			  UsageData usageData = dependencies.ResolveDependency( typeof( UsageData ) );
			  PublishEditionInfo( usageData, platformModule.DatabaseInfo, config );
			  PublishServerId( config, usageData );

			  // Ordering of lifecycles is important. Clustering infrastructure should start before paxos components
			  life.Add( clusteringLife );
			  life.Add( paxosLife );
		 }

		 private class ClusterListener_AdapterAnonymousInnerClass : Neo4Net.cluster.protocol.cluster.ClusterListener_Adapter
		 {
			 private readonly HighlyAvailableEditionModule _outerInstance;

			 private ClusterClient _clusterClient;

			 public ClusterListener_AdapterAnonymousInnerClass( HighlyAvailableEditionModule outerInstance, ClusterClient clusterClient )
			 {
				 this.outerInstance = outerInstance;
				 this._clusterClient = clusterClient;
				 hasRequestedElection = true;
			 }

			 internal bool hasRequestedElection;
			 // request or thereafter

			 public override void enteredCluster( ClusterConfiguration clusterConfiguration )
			 {
				  _clusterClient.performRoleElections();
			 }

			 public override void elected( string role, InstanceId instanceId, URI electedMember )
			 {
				  if ( hasRequestedElection && role.Equals( ClusterConfiguration.COORDINATOR ) )
				  {
						_clusterClient.removeClusterListener( this );
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void registerEditionSpecificProcedures(org.Neo4Net.kernel.impl.proc.Procedures procedures) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.KernelException
		 public override void RegisterEditionSpecificProcedures( Procedures procedures )
		 {
			  procedures.RegisterProcedure( typeof( EnterpriseBuiltInDbmsProcedures ), true );
			  procedures.RegisterProcedure( typeof( EnterpriseBuiltInProcedures ), true );
		 }

		 private static StatementLocksFactory CreateStatementLocksFactory( Locks locks, ComponentSwitcherContainer componentSwitcherContainer, Config config, LogService logging )
		 {
			  StatementLocksFactory configuredStatementLocks = ( new StatementLocksFactorySelector( locks, config, logging ) ).select();

			  DelegateInvocationHandler<StatementLocksFactory> locksFactoryDelegate = new DelegateInvocationHandler<StatementLocksFactory>( typeof( StatementLocksFactory ) );
			  StatementLocksFactory locksFactory = ( StatementLocksFactory ) newProxyInstance( typeof( StatementLocksFactory ).ClassLoader, new Type[]{ typeof( StatementLocksFactory ) }, locksFactoryDelegate );

			  StatementLocksFactorySwitcher locksSwitcher = new StatementLocksFactorySwitcher( locksFactoryDelegate, configuredStatementLocks );
			  componentSwitcherContainer.Add( locksSwitcher );

			  return locksFactory;
		 }

		 internal static System.Predicate<string> FileWatcherFileNameFilter()
		 {
			  return Predicates.any( fileName => fileName.StartsWith( TransactionLogFiles.DEFAULT_NAME ), fileName => fileName.StartsWith( IndexConfigStore.INDEX_DB_FILE_NAME ), filename => filename.StartsWith( StoreUtil.BRANCH_SUBDIRECTORY ), filename => filename.StartsWith( StoreUtil.TEMP_COPY_DIRECTORY_NAME ), filename => filename.EndsWith( PageCacheWarmer.SUFFIX_CACHEPROF ) );
		 }

		 private static SwitchToSlave ChooseSwitchToSlaveStrategy( PlatformModule platformModule, Config config, Dependencies dependencies, LogService logging, Monitors monitors, DelegateInvocationHandler<Master> masterDelegateInvocationHandler, RequestContextFactory requestContextFactory, ClusterMemberAvailability clusterMemberAvailability, MasterClientResolver masterClientResolver, UpdatePuller updatePullerProxy, PullerFactory pullerFactory, System.Func<Slave, SlaveServer> slaveServerFactory, HaIdGeneratorFactory idGeneratorFactory, DatabaseLayout databaseLayout )
		 {
			  switch ( config.Get( HaSettings.branched_data_copying_strategy ) )
			  {
					case branch_then_copy:
						 return new SwitchToSlaveBranchThenCopy( databaseLayout, logging, platformModule.FileSystem, config, idGeneratorFactory, masterDelegateInvocationHandler, clusterMemberAvailability, requestContextFactory, pullerFactory, platformModule.KernelExtensionFactories, masterClientResolver, monitors.NewMonitor( typeof( SwitchToSlave.Monitor ) ), monitors.NewMonitor( typeof( StoreCopyClientMonitor ) ), platformModule.DataSourceManager.getDataSource, () => ResolveDatabaseDependency(platformModule, typeof(TransactionIdStore)), slaveServerFactory, updatePullerProxy, platformModule.PageCache, monitors, () => ResolveDatabaseDependency(platformModule, typeof(DatabaseTransactionStats)) );
					case copy_then_branch:
						 return new SwitchToSlaveCopyThenBranch( databaseLayout, logging, platformModule.FileSystem, config, idGeneratorFactory, masterDelegateInvocationHandler, clusterMemberAvailability, requestContextFactory, pullerFactory, platformModule.KernelExtensionFactories, masterClientResolver, monitors.NewMonitor( typeof( SwitchToSlave.Monitor ) ), monitors.NewMonitor( typeof( StoreCopyClientMonitor ) ), platformModule.DataSourceManager.getDataSource, () => ResolveDatabaseDependency(platformModule, typeof(TransactionIdStore)), slaveServerFactory, updatePullerProxy, platformModule.PageCache, monitors, () => ResolveDatabaseDependency(platformModule, typeof(DatabaseTransactionStats)) );
					default:
						 throw new Exception( "Unknown branched data copying strategy" );
			  }
		 }

		 private static void PublishServerId( Config config, UsageData sysInfo )
		 {
			  sysInfo.Set( UsageDataKeys.serverId, config.Get( ClusterSettings.server_id ).ToString() );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.Neo4Net.kernel.impl.transaction.TransactionHeaderInformationFactory createHeaderInformationFactory(final org.Neo4Net.kernel.ha.cluster.HighAvailabilityMemberContext memberContext)
		 private static TransactionHeaderInformationFactory CreateHeaderInformationFactory( HighAvailabilityMemberContext memberContext )
		 {
			  return new TransactionHeaderInformationFactory_WithRandomBytesAnonymousInnerClass( memberContext );
		 }

		 private class TransactionHeaderInformationFactory_WithRandomBytesAnonymousInnerClass : Neo4Net.Kernel.impl.transaction.TransactionHeaderInformationFactory_WithRandomBytes
		 {
			 private HighAvailabilityMemberContext _memberContext;

			 public TransactionHeaderInformationFactory_WithRandomBytesAnonymousInnerClass( HighAvailabilityMemberContext memberContext )
			 {
				 this._memberContext = memberContext;
			 }

			 protected internal override TransactionHeaderInformation createUsing( sbyte[] additionalHeader )
			 {
				  return new TransactionHeaderInformation( _memberContext.ElectedMasterId.toIntegerIndex(), _memberContext.MyId.toIntegerIndex(), additionalHeader );
			 }
		 }

		 private static CommitProcessFactory CreateCommitProcessFactory( Dependencies dependencies, LogService logging, Monitors monitors, Config config, LifeSupport paxosLife, ClusterClient clusterClient, ClusterMembers members, IJobScheduler jobScheduler, Master master, RequestContextFactory requestContextFactory, ComponentSwitcherContainer componentSwitcherContainer, System.Func<LogEntryReader<ReadableClosablePositionAwareChannel>> logEntryReader )
		 {
			  DefaultSlaveFactory slaveFactory = dependencies.SatisfyDependency( new DefaultSlaveFactory( logging.InternalLogProvider, monitors, config.Get( HaSettings.com_chunk_size ).intValue(), logEntryReader ) );

			  HostnamePort me = config.Get( ClusterSettings.cluster_server );
			  Slaves slaves = dependencies.SatisfyDependency( paxosLife.Add( new HighAvailabilitySlaves( members, clusterClient, slaveFactory, me ) ) );

			  TransactionPropagator transactionPropagator = new TransactionPropagator( TransactionPropagator.from( config ), logging.GetInternalLog( typeof( TransactionPropagator ) ), slaves, new CommitPusher( jobScheduler ) );
			  paxosLife.Add( transactionPropagator );

			  DelegateInvocationHandler<TransactionCommitProcess> commitProcessDelegate = new DelegateInvocationHandler<TransactionCommitProcess>( typeof( TransactionCommitProcess ) );

			  CommitProcessSwitcher commitProcessSwitcher = new CommitProcessSwitcher( transactionPropagator, master, commitProcessDelegate, requestContextFactory, monitors, dependencies, config );
			  componentSwitcherContainer.Add( commitProcessSwitcher );

			  return new HighlyAvailableCommitProcessFactory( commitProcessDelegate );
		 }

		 private static IdGeneratorFactory CreateIdGeneratorFactory( DelegateInvocationHandler<Master> masterDelegateInvocationHandler, LogProvider logging, RequestContextFactory requestContextFactory, FileSystemAbstraction fs, IdTypeConfigurationProvider idTypeConfigurationProvider )
		 {
			  HaIdGeneratorFactory idGeneratorFactory = new HaIdGeneratorFactory( masterDelegateInvocationHandler, logging, requestContextFactory, fs, idTypeConfigurationProvider );
			  /*
			   * We don't really switch to master here. We just need to initialize the idGenerator so the initial store
			   * can be started (if required). In any case, the rest of the database is in pending state, so nothing will
			   * happen until events start arriving and that will set us to the proper state anyway.
			   */
			  idGeneratorFactory.SwitchToMaster();
			  return idGeneratorFactory;
		 }

		 private static Locks CreateLockManager( LocksFactory lockFactory, ComponentSwitcherContainer componentSwitcherContainer, Config config, DelegateInvocationHandler<Master> masterDelegateInvocationHandler, RequestContextFactory requestContextFactory, AvailabilityGuard availabilityGuard, Clock clock, LogService logService )
		 {
			  DelegateInvocationHandler<Locks> lockManagerDelegate = new DelegateInvocationHandler<Locks>( typeof( Locks ) );
			  Locks lockManager = ( Locks ) newProxyInstance( typeof( Locks ).ClassLoader, new Type[]{ typeof( Locks ) }, lockManagerDelegate );

			  IFactory<Locks> locksFactory = () => EditionLocksFactories.createLockManager(lockFactory, config, clock);

			  LockManagerSwitcher lockManagerModeSwitcher = new LockManagerSwitcher( lockManagerDelegate, masterDelegateInvocationHandler, requestContextFactory, availabilityGuard, locksFactory, logService.InternalLogProvider, config );

			  componentSwitcherContainer.Add( lockManagerModeSwitcher );
			  return lockManager;
		 }

		 private static TokenCreator CreateRelationshipTypeCreator( Config config, ComponentSwitcherContainer componentSwitcherContainer, DelegateInvocationHandler<Master> masterInvocationHandler, RequestContextFactory requestContextFactory, System.Func<Kernel> kernelProvider )
		 {
			  if ( config.Get( GraphDatabaseSettings.read_only ) )
			  {
					return new ReadOnlyTokenCreator();
			  }

			  DelegateInvocationHandler<TokenCreator> relationshipTypeCreatorDelegate = new DelegateInvocationHandler<TokenCreator>( typeof( TokenCreator ) );
			  TokenCreator relationshipTypeCreator = ( TokenCreator ) newProxyInstance( typeof( TokenCreator ).ClassLoader, new Type[]{ typeof( TokenCreator ) }, relationshipTypeCreatorDelegate );

			  RelationshipTypeCreatorSwitcher typeCreatorModeSwitcher = new RelationshipTypeCreatorSwitcher( relationshipTypeCreatorDelegate, masterInvocationHandler, requestContextFactory, kernelProvider );

			  componentSwitcherContainer.Add( typeCreatorModeSwitcher );
			  return relationshipTypeCreator;
		 }

		 private static TokenCreator CreatePropertyKeyCreator( Config config, ComponentSwitcherContainer componentSwitcherContainer, DelegateInvocationHandler<Master> masterDelegateInvocationHandler, RequestContextFactory requestContextFactory, System.Func<Kernel> kernelSupplier )
		 {
			  if ( config.Get( GraphDatabaseSettings.read_only ) )
			  {
					return new ReadOnlyTokenCreator();
			  }

			  DelegateInvocationHandler<TokenCreator> propertyKeyCreatorDelegate = new DelegateInvocationHandler<TokenCreator>( typeof( TokenCreator ) );
			  TokenCreator propertyTokenCreator = ( TokenCreator ) newProxyInstance( typeof( TokenCreator ).ClassLoader, new Type[]{ typeof( TokenCreator ) }, propertyKeyCreatorDelegate );

			  PropertyKeyCreatorSwitcher propertyKeyCreatorModeSwitcher = new PropertyKeyCreatorSwitcher( propertyKeyCreatorDelegate, masterDelegateInvocationHandler, requestContextFactory, kernelSupplier );

			  componentSwitcherContainer.Add( propertyKeyCreatorModeSwitcher );
			  return propertyTokenCreator;
		 }

		 private static TokenCreator CreateLabelIdCreator( Config config, ComponentSwitcherContainer componentSwitcherContainer, DelegateInvocationHandler<Master> masterDelegateInvocationHandler, RequestContextFactory requestContextFactory, System.Func<Kernel> kernelProvider )
		 {
			  if ( config.Get( GraphDatabaseSettings.read_only ) )
			  {
					return new ReadOnlyTokenCreator();
			  }

			  DelegateInvocationHandler<TokenCreator> labelIdCreatorDelegate = new DelegateInvocationHandler<TokenCreator>( typeof( TokenCreator ) );
			  TokenCreator labelIdCreator = ( TokenCreator ) newProxyInstance( typeof( TokenCreator ).ClassLoader, new Type[]{ typeof( TokenCreator ) }, labelIdCreatorDelegate );

			  LabelTokenCreatorSwitcher modeSwitcher = new LabelTokenCreatorSwitcher( labelIdCreatorDelegate, masterDelegateInvocationHandler, requestContextFactory, kernelProvider );

			  componentSwitcherContainer.Add( modeSwitcher );
			  return labelIdCreator;
		 }

		 private static KernelData CreateKernelData( Config config, DataSourceManager dataSourceManager, ClusterMembers members, FileSystemAbstraction fs, PageCache pageCache, File storeDir, LastUpdateTime lastUpdateTime, LastTxIdGetter txIdGetter, LifeSupport life )
		 {
			  ClusterDatabaseInfoProvider databaseInfo = new ClusterDatabaseInfoProvider( members, txIdGetter, lastUpdateTime );
			  return life.Add( new HighlyAvailableKernelData( dataSourceManager, members, databaseInfo, fs, pageCache, storeDir, config ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void registerRecovery(final org.Neo4Net.kernel.impl.factory.DatabaseInfo databaseInfo, final org.Neo4Net.graphdb.DependencyResolver dependencyResolver, final org.Neo4Net.logging.internal.LogService logging, org.Neo4Net.graphdb.factory.module.PlatformModule platformModule)
		 private void RegisterRecovery( DatabaseInfo databaseInfo, DependencyResolver dependencyResolver, LogService logging, PlatformModule platformModule )
		 {
			  MemberStateMachine.addHighAvailabilityMemberListener( new HighAvailabilityMemberListener_AdapterAnonymousInnerClass( this, databaseInfo, dependencyResolver, logging, platformModule ) );
		 }

		 private class HighAvailabilityMemberListener_AdapterAnonymousInnerClass : Neo4Net.Kernel.ha.cluster.HighAvailabilityMemberListener_Adapter
		 {
			 private readonly HighlyAvailableEditionModule _outerInstance;

			 private DatabaseInfo _databaseInfo;
			 private DependencyResolver _dependencyResolver;
			 private LogService _logging;
			 private PlatformModule _platformModule;

			 public HighAvailabilityMemberListener_AdapterAnonymousInnerClass( HighlyAvailableEditionModule outerInstance, DatabaseInfo databaseInfo, DependencyResolver dependencyResolver, LogService logging, PlatformModule platformModule )
			 {
				 this.outerInstance = outerInstance;
				 this._databaseInfo = databaseInfo;
				 this._dependencyResolver = dependencyResolver;
				 this._logging = logging;
				 this._platformModule = platformModule;
			 }

			 public override void masterIsAvailable( HighAvailabilityMemberChangeEvent @event )
			 {
				  if ( @event.OldState.Equals( HighAvailabilityMemberState.TO_MASTER ) && @event.NewState.Equals( HighAvailabilityMemberState.MASTER ) )
				  {
						doAfterRecoveryAndStartup();
				  }
			 }

			 public override void slaveIsAvailable( HighAvailabilityMemberChangeEvent @event )
			 {
				  if ( @event.OldState.Equals( HighAvailabilityMemberState.TO_SLAVE ) && @event.NewState.Equals( HighAvailabilityMemberState.SLAVE ) )
				  {
						doAfterRecoveryAndStartup();
				  }
			 }

			 private void doAfterRecoveryAndStartup()
			 {
				  try
				  {
						DiagnosticsManager diagnosticsManager = _dependencyResolver.resolveDependency( typeof( DiagnosticsManager ) );

						NeoStoreDataSource neoStoreDataSource = _platformModule.dataSourceManager.DataSource;

						diagnosticsManager.PrependProvider( new KernelDiagnostics.Versions( _databaseInfo, neoStoreDataSource.StoreId ) );
						neoStoreDataSource.RegisterDiagnosticsWith( diagnosticsManager );
						diagnosticsManager.AppendProvider( new KernelDiagnostics.StoreFiles( neoStoreDataSource.DatabaseLayout ) );
						AssureLastCommitTimestampInitialized( _dependencyResolver, _platformModule.config );
				  }
				  catch ( Exception throwable )
				  {
						Log messagesLog = _logging.getInternalLog( typeof( EnterpriseEditionModule ) );
						messagesLog.Error( "Post recovery error", throwable );
						try
						{
							 _outerInstance.memberStateMachine.stop();
						}
						catch ( Exception throwable1 )
						{
							 messagesLog.Warn( "Could not stop", throwable1 );
						}
						try
						{
							 _outerInstance.memberStateMachine.start();
						}
						catch ( Exception throwable1 )
						{
							 messagesLog.Warn( "Could not start", throwable1 );
						}
				  }
			 }
		 }

		 private static void AssureLastCommitTimestampInitialized( DependencyResolver globalResolver, Config config )
		 {
			  GraphDatabaseFacade databaseFacade = globalResolver.ResolveDependency( typeof( DatabaseManager ) ).getDatabaseFacade( config.Get( GraphDatabaseSettings.active_database ) ).get();
			  DependencyResolver databaseResolver = databaseFacade.DependencyResolver;
			  MetaDataStore metaDataStore = databaseResolver.ResolveDependency( typeof( MetaDataStore ) );
			  LogicalTransactionStore txStore = databaseResolver.ResolveDependency( typeof( LogicalTransactionStore ) );

			  TransactionId txInfo = metaDataStore.LastCommittedTransaction;
			  long lastCommitTimestampFromStore = txInfo.CommitTimestamp();
			  if ( txInfo.TransactionIdConflict() == Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID )
			  {
					metaDataStore.LastTransactionCommitTimestamp = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_COMMIT_TIMESTAMP;
					return;
			  }
			  if ( lastCommitTimestampFromStore == Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.UNKNOWN_TX_COMMIT_TIMESTAMP || lastCommitTimestampFromStore == Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_COMMIT_TIMESTAMP )
			  {
					long lastCommitTimestampFromLogs;
					try
					{
						 TransactionMetadata metadata = txStore.GetMetadataFor( txInfo.TransactionIdConflict() );
						 lastCommitTimestampFromLogs = metadata.TimeWritten;
					}
					catch ( NoSuchTransactionException )
					{
						 lastCommitTimestampFromLogs = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore_Fields.UNKNOWN_TX_COMMIT_TIMESTAMP;
					}
					catch ( IOException e )
					{
						 throw new System.InvalidOperationException( "Unable to read transaction logs", e );
					}
					metaDataStore.LastTransactionCommitTimestamp = lastCommitTimestampFromLogs;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.Neo4Net.com.Server.Configuration masterServerConfig(final org.Neo4Net.kernel.configuration.Config config)
		 private static Server.Configuration MasterServerConfig( Config config )
		 {
			  return CommonConfig( config );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.Neo4Net.com.Server.Configuration slaveServerConfig(final org.Neo4Net.kernel.configuration.Config config)
		 private static Server.Configuration SlaveServerConfig( Config config )
		 {
			  return CommonConfig( config );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.Neo4Net.com.Server.Configuration commonConfig(final org.Neo4Net.kernel.configuration.Config config)
		 private static Server.Configuration CommonConfig( Config config )
		 {
			  return new ConfigurationAnonymousInnerClass( config );
		 }

		 private class ConfigurationAnonymousInnerClass : Server.Configuration
		 {
			 private Config _config;

			 public ConfigurationAnonymousInnerClass( Config config )
			 {
				 this._config = config;
			 }

			 public long OldChannelThreshold
			 {
				 get
				 {
					  return _config.get( HaSettings.lock_read_timeout ).toMillis();
				 }
			 }

			 public int MaxConcurrentTransactions
			 {
				 get
				 {
					  return _config.get( HaSettings.max_concurrent_channels_per_slave );
				 }
			 }

			 public int ChunkSize
			 {
				 get
				 {
					  return _config.get( HaSettings.com_chunk_size ).intValue();
				 }
			 }

			 public HostnamePort ServerAddress
			 {
				 get
				 {
					  return _config.get( HaSettings.ha_server );
				 }
			 }
		 }

		 protected internal override NetworkConnectionTracker CreateConnectionTracker()
		 {
			  return new DefaultNetworkConnectionTracker();
		 }

		 public override void CreateSecurityModule( PlatformModule platformModule, Procedures procedures )
		 {
			  EnterpriseEditionModule.createEnterpriseSecurityModule( this, platformModule, procedures );
		 }

		 private static T ResolveDatabaseDependency<T>( PlatformModule platform, Type clazz )
		 {
				 clazz = typeof( T );
			  return platform.DataSourceManager.DataSource.DependencyResolver.resolveDependency( clazz );
		 }
	}

}