using System;

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
namespace Org.Neo4j.Kernel.ha.factory
{
	using InternalLoggerFactory = org.jboss.netty.logging.InternalLoggerFactory;


	using ClusterSettings = Org.Neo4j.cluster.ClusterSettings;
	using InstanceId = Org.Neo4j.cluster.InstanceId;
	using ClusterClient = Org.Neo4j.cluster.client.ClusterClient;
	using ClusterClientModule = Org.Neo4j.cluster.client.ClusterClientModule;
	using NettyLoggerFactory = Org.Neo4j.cluster.logging.NettyLoggerFactory;
	using ClusterMemberAvailability = Org.Neo4j.cluster.member.ClusterMemberAvailability;
	using ClusterMemberEvents = Org.Neo4j.cluster.member.ClusterMemberEvents;
	using MemberIsAvailable = Org.Neo4j.cluster.member.paxos.MemberIsAvailable;
	using PaxosClusterMemberAvailability = Org.Neo4j.cluster.member.paxos.PaxosClusterMemberAvailability;
	using PaxosClusterMemberEvents = Org.Neo4j.cluster.member.paxos.PaxosClusterMemberEvents;
	using ObjectStreamFactory = Org.Neo4j.cluster.protocol.atomicbroadcast.ObjectStreamFactory;
	using ClusterConfiguration = Org.Neo4j.cluster.protocol.cluster.ClusterConfiguration;
	using ClusterListener = Org.Neo4j.cluster.protocol.cluster.ClusterListener;
	using ElectionCredentialsProvider = Org.Neo4j.cluster.protocol.election.ElectionCredentialsProvider;
	using NotElectableElectionCredentialsProvider = Org.Neo4j.cluster.protocol.election.NotElectableElectionCredentialsProvider;
	using Org.Neo4j.com;
	using RequestMonitor = Org.Neo4j.com.monitor.RequestMonitor;
	using StoreCopyClientMonitor = Org.Neo4j.com.storecopy.StoreCopyClientMonitor;
	using StoreUtil = Org.Neo4j.com.storecopy.StoreUtil;
	using TransactionCommittingResponseUnpacker = Org.Neo4j.com.storecopy.TransactionCommittingResponseUnpacker;
	using DatabaseManager = Org.Neo4j.Dbms.database.DatabaseManager;
	using Org.Neo4j.Function;
	using Predicates = Org.Neo4j.Function.Predicates;
	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using EditionLocksFactories = Org.Neo4j.Graphdb.factory.EditionLocksFactories;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using PlatformModule = Org.Neo4j.Graphdb.factory.module.PlatformModule;
	using AbstractEditionModule = Org.Neo4j.Graphdb.factory.module.edition.AbstractEditionModule;
	using DefaultEditionModule = Org.Neo4j.Graphdb.factory.module.edition.DefaultEditionModule;
	using DatabaseIdContext = Org.Neo4j.Graphdb.factory.module.id.DatabaseIdContext;
	using IdContextFactoryBuilder = Org.Neo4j.Graphdb.factory.module.id.IdContextFactoryBuilder;
	using HostnamePort = Org.Neo4j.Helpers.HostnamePort;
	using NamedThreadFactory = Org.Neo4j.Helpers.NamedThreadFactory;
	using DiagnosticsManager = Org.Neo4j.@internal.Diagnostics.DiagnosticsManager;
	using Kernel = Org.Neo4j.@internal.Kernel.Api.Kernel;
	using InvalidTransactionTypeKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.InvalidTransactionTypeKernelException;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using NetworkConnectionTracker = Org.Neo4j.Kernel.api.net.NetworkConnectionTracker;
	using AvailabilityGuard = Org.Neo4j.Kernel.availability.AvailabilityGuard;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Settings = Org.Neo4j.Kernel.configuration.Settings;
	using SslPolicyLoader = Org.Neo4j.Kernel.configuration.ssl.SslPolicyLoader;
	using EnterpriseBuiltInDbmsProcedures = Org.Neo4j.Kernel.enterprise.builtinprocs.EnterpriseBuiltInDbmsProcedures;
	using EnterpriseBuiltInProcedures = Org.Neo4j.Kernel.enterprise.builtinprocs.EnterpriseBuiltInProcedures;
	using Org.Neo4j.Kernel.ha;
	using ConversationSPI = Org.Neo4j.Kernel.ha.cluster.ConversationSPI;
	using DefaultConversationSPI = Org.Neo4j.Kernel.ha.cluster.DefaultConversationSPI;
	using DefaultElectionCredentialsProvider = Org.Neo4j.Kernel.ha.cluster.DefaultElectionCredentialsProvider;
	using DefaultMasterImplSPI = Org.Neo4j.Kernel.ha.cluster.DefaultMasterImplSPI;
	using HANewSnapshotFunction = Org.Neo4j.Kernel.ha.cluster.HANewSnapshotFunction;
	using HighAvailabilityMemberChangeEvent = Org.Neo4j.Kernel.ha.cluster.HighAvailabilityMemberChangeEvent;
	using HighAvailabilityMemberContext = Org.Neo4j.Kernel.ha.cluster.HighAvailabilityMemberContext;
	using HighAvailabilityMemberListener = Org.Neo4j.Kernel.ha.cluster.HighAvailabilityMemberListener;
	using HighAvailabilityMemberState = Org.Neo4j.Kernel.ha.cluster.HighAvailabilityMemberState;
	using HighAvailabilityMemberStateMachine = Org.Neo4j.Kernel.ha.cluster.HighAvailabilityMemberStateMachine;
	using SimpleHighAvailabilityMemberContext = Org.Neo4j.Kernel.ha.cluster.SimpleHighAvailabilityMemberContext;
	using SwitchToMaster = Org.Neo4j.Kernel.ha.cluster.SwitchToMaster;
	using SwitchToSlave = Org.Neo4j.Kernel.ha.cluster.SwitchToSlave;
	using SwitchToSlaveBranchThenCopy = Org.Neo4j.Kernel.ha.cluster.SwitchToSlaveBranchThenCopy;
	using SwitchToSlaveCopyThenBranch = Org.Neo4j.Kernel.ha.cluster.SwitchToSlaveCopyThenBranch;
	using ClusterMembers = Org.Neo4j.Kernel.ha.cluster.member.ClusterMembers;
	using HighAvailabilitySlaves = Org.Neo4j.Kernel.ha.cluster.member.HighAvailabilitySlaves;
	using ObservedClusterMembers = Org.Neo4j.Kernel.ha.cluster.member.ObservedClusterMembers;
	using CommitProcessSwitcher = Org.Neo4j.Kernel.ha.cluster.modeswitch.CommitProcessSwitcher;
	using ComponentSwitcherContainer = Org.Neo4j.Kernel.ha.cluster.modeswitch.ComponentSwitcherContainer;
	using HighAvailabilityModeSwitcher = Org.Neo4j.Kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher;
	using LabelTokenCreatorSwitcher = Org.Neo4j.Kernel.ha.cluster.modeswitch.LabelTokenCreatorSwitcher;
	using LockManagerSwitcher = Org.Neo4j.Kernel.ha.cluster.modeswitch.LockManagerSwitcher;
	using PropertyKeyCreatorSwitcher = Org.Neo4j.Kernel.ha.cluster.modeswitch.PropertyKeyCreatorSwitcher;
	using RelationshipTypeCreatorSwitcher = Org.Neo4j.Kernel.ha.cluster.modeswitch.RelationshipTypeCreatorSwitcher;
	using StatementLocksFactorySwitcher = Org.Neo4j.Kernel.ha.cluster.modeswitch.StatementLocksFactorySwitcher;
	using UpdatePullerSwitcher = Org.Neo4j.Kernel.ha.cluster.modeswitch.UpdatePullerSwitcher;
	using RequestContextFactory = Org.Neo4j.Kernel.ha.com.RequestContextFactory;
	using ConversationManager = Org.Neo4j.Kernel.ha.com.master.ConversationManager;
	using DefaultSlaveFactory = Org.Neo4j.Kernel.ha.com.master.DefaultSlaveFactory;
	using Master = Org.Neo4j.Kernel.ha.com.master.Master;
	using MasterImpl = Org.Neo4j.Kernel.ha.com.master.MasterImpl;
	using MasterServer = Org.Neo4j.Kernel.ha.com.master.MasterServer;
	using Slave = Org.Neo4j.Kernel.ha.com.master.Slave;
	using SlaveFactory = Org.Neo4j.Kernel.ha.com.master.SlaveFactory;
	using Slaves = Org.Neo4j.Kernel.ha.com.master.Slaves;
	using InvalidEpochExceptionHandler = Org.Neo4j.Kernel.ha.com.slave.InvalidEpochExceptionHandler;
	using MasterClientResolver = Org.Neo4j.Kernel.ha.com.slave.MasterClientResolver;
	using SlaveServer = Org.Neo4j.Kernel.ha.com.slave.SlaveServer;
	using HaIdGeneratorFactory = Org.Neo4j.Kernel.ha.id.HaIdGeneratorFactory;
	using HaIdReuseEligibility = Org.Neo4j.Kernel.ha.id.HaIdReuseEligibility;
	using ClusterDatabaseInfoProvider = Org.Neo4j.Kernel.ha.management.ClusterDatabaseInfoProvider;
	using HighlyAvailableKernelData = Org.Neo4j.Kernel.ha.management.HighlyAvailableKernelData;
	using CommitPusher = Org.Neo4j.Kernel.ha.transaction.CommitPusher;
	using OnDiskLastTxIdGetter = Org.Neo4j.Kernel.ha.transaction.OnDiskLastTxIdGetter;
	using TransactionPropagator = Org.Neo4j.Kernel.ha.transaction.TransactionPropagator;
	using CommitProcessFactory = Org.Neo4j.Kernel.Impl.Api.CommitProcessFactory;
	using TransactionCommitProcess = Org.Neo4j.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionHeaderInformation = Org.Neo4j.Kernel.Impl.Api.TransactionHeaderInformation;
	using DelegatingTokenHolder = Org.Neo4j.Kernel.impl.core.DelegatingTokenHolder;
	using LastTxIdGetter = Org.Neo4j.Kernel.impl.core.LastTxIdGetter;
	using ReadOnlyTokenCreator = Org.Neo4j.Kernel.impl.core.ReadOnlyTokenCreator;
	using ThreadToStatementContextBridge = Org.Neo4j.Kernel.impl.core.ThreadToStatementContextBridge;
	using TokenCreator = Org.Neo4j.Kernel.impl.core.TokenCreator;
	using TokenHolder = Org.Neo4j.Kernel.impl.core.TokenHolder;
	using TokenHolders = Org.Neo4j.Kernel.impl.core.TokenHolders;
	using EnterpriseConstraintSemantics = Org.Neo4j.Kernel.impl.enterprise.EnterpriseConstraintSemantics;
	using EnterpriseEditionModule = Org.Neo4j.Kernel.impl.enterprise.EnterpriseEditionModule;
	using EnterpriseIdTypeConfigurationProvider = Org.Neo4j.Kernel.impl.enterprise.id.EnterpriseIdTypeConfigurationProvider;
	using ConfigurableIOLimiter = Org.Neo4j.Kernel.impl.enterprise.transaction.log.checkpoint.ConfigurableIOLimiter;
	using CanWrite = Org.Neo4j.Kernel.impl.factory.CanWrite;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using GraphDatabaseFacade = Org.Neo4j.Kernel.impl.factory.GraphDatabaseFacade;
	using ReadOnly = Org.Neo4j.Kernel.impl.factory.ReadOnly;
	using StatementLocksFactorySelector = Org.Neo4j.Kernel.impl.factory.StatementLocksFactorySelector;
	using IndexConfigStore = Org.Neo4j.Kernel.impl.index.IndexConfigStore;
	using Locks = Org.Neo4j.Kernel.impl.locking.Locks;
	using LocksFactory = Org.Neo4j.Kernel.impl.locking.LocksFactory;
	using StatementLocksFactory = Org.Neo4j.Kernel.impl.locking.StatementLocksFactory;
	using DefaultNetworkConnectionTracker = Org.Neo4j.Kernel.impl.net.DefaultNetworkConnectionTracker;
	using PageCacheWarmer = Org.Neo4j.Kernel.impl.pagecache.PageCacheWarmer;
	using Procedures = Org.Neo4j.Kernel.impl.proc.Procedures;
	using MetaDataStore = Org.Neo4j.Kernel.impl.store.MetaDataStore;
	using TransactionId = Org.Neo4j.Kernel.impl.store.TransactionId;
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using IdTypeConfigurationProvider = Org.Neo4j.Kernel.impl.store.id.configuration.IdTypeConfigurationProvider;
	using TransactionHeaderInformationFactory = Org.Neo4j.Kernel.impl.transaction.TransactionHeaderInformationFactory;
	using LogicalTransactionStore = Org.Neo4j.Kernel.impl.transaction.log.LogicalTransactionStore;
	using NoSuchTransactionException = Org.Neo4j.Kernel.impl.transaction.log.NoSuchTransactionException;
	using ReadableClosablePositionAwareChannel = Org.Neo4j.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using CheckPointer = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using Org.Neo4j.Kernel.impl.transaction.log.entry;
	using TransactionLogFiles = Org.Neo4j.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using DataSourceManager = Org.Neo4j.Kernel.impl.transaction.state.DataSourceManager;
	using DatabaseTransactionStats = Org.Neo4j.Kernel.impl.transaction.stats.DatabaseTransactionStats;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using KernelData = Org.Neo4j.Kernel.@internal.KernelData;
	using KernelDiagnostics = Org.Neo4j.Kernel.@internal.KernelDiagnostics;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using Lifecycle = Org.Neo4j.Kernel.Lifecycle.Lifecycle;
	using ByteCounterMonitor = Org.Neo4j.Kernel.monitoring.ByteCounterMonitor;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using StoreId = Org.Neo4j.Storageengine.Api.StoreId;
	using UsageData = Org.Neo4j.Udc.UsageData;
	using UsageDataKeys = Org.Neo4j.Udc.UsageDataKeys;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.EditionLocksFactories.createLockFactory;
	using static Org.Neo4j.Kernel.impl.transaction.log.TransactionMetadataCache.TransactionMetadata;

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
//ORIGINAL LINE: public HighlyAvailableEditionModule(final org.neo4j.graphdb.factory.module.PlatformModule platformModule)
		 public HighlyAvailableEditionModule( PlatformModule platformModule )
		 {
			  IoLimiterConflict = new ConfigurableIOLimiter( platformModule.Config );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.lifecycle.LifeSupport life = platformModule.life;
			  LifeSupport life = platformModule.Life;
			  life.Add( platformModule.DataSourceManager );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.lifecycle.LifeSupport paxosLife = new org.neo4j.kernel.lifecycle.LifeSupport();
			  LifeSupport paxosLife = new LifeSupport();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.lifecycle.LifeSupport clusteringLife = new org.neo4j.kernel.lifecycle.LifeSupport();
			  LifeSupport clusteringLife = new LifeSupport();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.io.fs.FileSystemAbstraction fs = platformModule.fileSystem;
			  FileSystemAbstraction fs = platformModule.FileSystem;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.configuration.Config config = platformModule.config;
			  Config config = platformModule.Config;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.util.Dependencies dependencies = platformModule.dependencies;
			  Dependencies dependencies = platformModule.Dependencies;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.logging.internal.LogService logging = platformModule.logging;
			  LogService logging = platformModule.Logging;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.monitoring.Monitors monitors = platformModule.monitors;
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
//ORIGINAL LINE: final long idReuseSafeZone = config.get(org.neo4j.kernel.ha.HaSettings.id_reuse_safe_zone_time).toMillis();
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
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicReference<org.neo4j.kernel.ha.cluster.HighAvailabilityMemberStateMachine> electionProviderRef = new java.util.concurrent.atomic.AtomicReference<>();
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
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicReference<org.neo4j.kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher> exceptionHandlerRef = new java.util.concurrent.atomic.AtomicReference<>();
			  AtomicReference<HighAvailabilityModeSwitcher> exceptionHandlerRef = new AtomicReference<HighAvailabilityModeSwitcher>();
			  InvalidEpochExceptionHandler invalidEpochHandler = () => exceptionHandlerRef.get().forceElections();

			  // At the point in time the LogEntryReader hasn't been instantiated yet. The StorageEngine is responsible
			  // for instantiating the CommandReaderFactory, required by a LogEntryReader. The StorageEngine is
			  // created in the DataSourceModule, after this module.
			  //   That is OK though because all users of it, instantiated below, will not use it right away,
			  // but merely provide a way to get access to it. That's why this is a Supplier and will be asked
			  // later, after the data source module and all that have started.
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({"deprecation", "unchecked"}) System.Func<org.neo4j.kernel.impl.transaction.log.entry.LogEntryReader<org.neo4j.kernel.impl.transaction.log.ReadableClosablePositionAwareChannel>> logEntryReader = () -> resolveDatabaseDependency(platformModule, org.neo4j.kernel.impl.transaction.log.entry.LogEntryReader.class);
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
//ORIGINAL LINE: final org.neo4j.function.Factory<org.neo4j.kernel.ha.com.master.MasterImpl.SPI> masterSPIFactory = () -> new org.neo4j.kernel.ha.cluster.DefaultMasterImplSPI(resolveDatabaseDependency(platformModule, org.neo4j.kernel.impl.factory.GraphDatabaseFacade.class), platformModule.fileSystem, platformModule.monitors, tokenHolders, idContext.getIdGeneratorFactory(), resolveDatabaseDependency(platformModule, org.neo4j.kernel.impl.api.TransactionCommitProcess.class), resolveDatabaseDependency(platformModule, org.neo4j.kernel.impl.transaction.log.checkpoint.CheckPointer.class), resolveDatabaseDependency(platformModule, org.neo4j.kernel.impl.transaction.log.TransactionIdStore.class), resolveDatabaseDependency(platformModule, org.neo4j.kernel.impl.transaction.log.LogicalTransactionStore.class), platformModule.dataSourceManager.getDataSource(), logging.getInternalLogProvider());
			  Factory<MasterImpl.SPI> masterSPIFactory = () => new DefaultMasterImplSPI(ResolveDatabaseDependency(platformModule, typeof(GraphDatabaseFacade)), platformModule.FileSystem, platformModule.Monitors, _tokenHolders, idContext.IdGeneratorFactory, ResolveDatabaseDependency(platformModule, typeof(TransactionCommitProcess)), ResolveDatabaseDependency(platformModule, typeof(CheckPointer)), ResolveDatabaseDependency(platformModule, typeof(TransactionIdStore)), ResolveDatabaseDependency(platformModule, typeof(LogicalTransactionStore)), platformModule.DataSourceManager.DataSource, logging.InternalLogProvider);

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

			  DelegatingTokenHolder propertyKeyTokenHolder = new DelegatingTokenHolder( CreatePropertyKeyCreator( config, componentSwitcherContainer, masterDelegateInvocationHandler, requestContextFactory, kernelProvider ), Org.Neo4j.Kernel.impl.core.TokenHolder_Fields.TYPE_PROPERTY_KEY );
			  DelegatingTokenHolder labelTokenHolder = new DelegatingTokenHolder( CreateLabelIdCreator( config, componentSwitcherContainer, masterDelegateInvocationHandler, requestContextFactory, kernelProvider ), Org.Neo4j.Kernel.impl.core.TokenHolder_Fields.TYPE_LABEL );
			  DelegatingTokenHolder relationshipTypeTokenHolder = new DelegatingTokenHolder( CreateRelationshipTypeCreator( config, componentSwitcherContainer, masterDelegateInvocationHandler, requestContextFactory, kernelProvider ), Org.Neo4j.Kernel.impl.core.TokenHolder_Fields.TYPE_RELATIONSHIP_TYPE );

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

		 private class ClusterListener_AdapterAnonymousInnerClass : Org.Neo4j.cluster.protocol.cluster.ClusterListener_Adapter
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
//ORIGINAL LINE: public void registerEditionSpecificProcedures(org.neo4j.kernel.impl.proc.Procedures procedures) throws org.neo4j.internal.kernel.api.exceptions.KernelException
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
//ORIGINAL LINE: private static org.neo4j.kernel.impl.transaction.TransactionHeaderInformationFactory createHeaderInformationFactory(final org.neo4j.kernel.ha.cluster.HighAvailabilityMemberContext memberContext)
		 private static TransactionHeaderInformationFactory CreateHeaderInformationFactory( HighAvailabilityMemberContext memberContext )
		 {
			  return new TransactionHeaderInformationFactory_WithRandomBytesAnonymousInnerClass( memberContext );
		 }

		 private class TransactionHeaderInformationFactory_WithRandomBytesAnonymousInnerClass : Org.Neo4j.Kernel.impl.transaction.TransactionHeaderInformationFactory_WithRandomBytes
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

		 private static CommitProcessFactory CreateCommitProcessFactory( Dependencies dependencies, LogService logging, Monitors monitors, Config config, LifeSupport paxosLife, ClusterClient clusterClient, ClusterMembers members, JobScheduler jobScheduler, Master master, RequestContextFactory requestContextFactory, ComponentSwitcherContainer componentSwitcherContainer, System.Func<LogEntryReader<ReadableClosablePositionAwareChannel>> logEntryReader )
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

			  Factory<Locks> locksFactory = () => EditionLocksFactories.createLockManager(lockFactory, config, clock);

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
//ORIGINAL LINE: private void registerRecovery(final org.neo4j.kernel.impl.factory.DatabaseInfo databaseInfo, final org.neo4j.graphdb.DependencyResolver dependencyResolver, final org.neo4j.logging.internal.LogService logging, org.neo4j.graphdb.factory.module.PlatformModule platformModule)
		 private void RegisterRecovery( DatabaseInfo databaseInfo, DependencyResolver dependencyResolver, LogService logging, PlatformModule platformModule )
		 {
			  MemberStateMachine.addHighAvailabilityMemberListener( new HighAvailabilityMemberListener_AdapterAnonymousInnerClass( this, databaseInfo, dependencyResolver, logging, platformModule ) );
		 }

		 private class HighAvailabilityMemberListener_AdapterAnonymousInnerClass : Org.Neo4j.Kernel.ha.cluster.HighAvailabilityMemberListener_Adapter
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
			  if ( txInfo.TransactionIdConflict() == Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID )
			  {
					metaDataStore.LastTransactionCommitTimestamp = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_COMMIT_TIMESTAMP;
					return;
			  }
			  if ( lastCommitTimestampFromStore == Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.UNKNOWN_TX_COMMIT_TIMESTAMP || lastCommitTimestampFromStore == Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_COMMIT_TIMESTAMP )
			  {
					long lastCommitTimestampFromLogs;
					try
					{
						 TransactionMetadata metadata = txStore.GetMetadataFor( txInfo.TransactionIdConflict() );
						 lastCommitTimestampFromLogs = metadata.TimeWritten;
					}
					catch ( NoSuchTransactionException )
					{
						 lastCommitTimestampFromLogs = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.UNKNOWN_TX_COMMIT_TIMESTAMP;
					}
					catch ( IOException e )
					{
						 throw new System.InvalidOperationException( "Unable to read transaction logs", e );
					}
					metaDataStore.LastTransactionCommitTimestamp = lastCommitTimestampFromLogs;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.neo4j.com.Server.Configuration masterServerConfig(final org.neo4j.kernel.configuration.Config config)
		 private static Server.Configuration MasterServerConfig( Config config )
		 {
			  return CommonConfig( config );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.neo4j.com.Server.Configuration slaveServerConfig(final org.neo4j.kernel.configuration.Config config)
		 private static Server.Configuration SlaveServerConfig( Config config )
		 {
			  return CommonConfig( config );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.neo4j.com.Server.Configuration commonConfig(final org.neo4j.kernel.configuration.Config config)
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