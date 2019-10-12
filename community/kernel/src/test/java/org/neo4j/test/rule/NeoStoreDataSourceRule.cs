using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Org.Neo4j.Test.rule
{

	using DependencyResolver = Org.Neo4j.Graphdb.DependencyResolver;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using RecoveryCleanupWorkCollector = Org.Neo4j.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using DiagnosticsManager = Org.Neo4j.@internal.Diagnostics.DiagnosticsManager;
	using TokenNameLookup = Org.Neo4j.@internal.Kernel.Api.TokenNameLookup;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using IOLimiter = Org.Neo4j.Io.pagecache.IOLimiter;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using VersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using DatabaseCreationContext = Org.Neo4j.Kernel.DatabaseCreationContext;
	using NeoStoreDataSource = Org.Neo4j.Kernel.NeoStoreDataSource;
	using AutoIndexing = Org.Neo4j.Kernel.api.explicitindex.AutoIndexing;
	using IndexProvider = Org.Neo4j.Kernel.Api.Index.IndexProvider;
	using DatabaseAvailability = Org.Neo4j.Kernel.availability.DatabaseAvailability;
	using DatabaseAvailabilityGuard = Org.Neo4j.Kernel.availability.DatabaseAvailabilityGuard;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Org.Neo4j.Kernel.extension;
	using CommitProcessFactory = Org.Neo4j.Kernel.Impl.Api.CommitProcessFactory;
	using ExplicitIndexProvider = Org.Neo4j.Kernel.Impl.Api.ExplicitIndexProvider;
	using SchemaWriteGuard = Org.Neo4j.Kernel.Impl.Api.SchemaWriteGuard;
	using InternalAutoIndexing = Org.Neo4j.Kernel.Impl.Api.explicitindex.InternalAutoIndexing;
	using IndexingService = Org.Neo4j.Kernel.Impl.Api.index.IndexingService;
	using ConstraintSemantics = Org.Neo4j.Kernel.impl.constraints.ConstraintSemantics;
	using StandardConstraintSemantics = Org.Neo4j.Kernel.impl.constraints.StandardConstraintSemantics;
	using TransactionVersionContextSupplier = Org.Neo4j.Kernel.impl.context.TransactionVersionContextSupplier;
	using DatabasePanicEventGenerator = Org.Neo4j.Kernel.impl.core.DatabasePanicEventGenerator;
	using TokenHolders = Org.Neo4j.Kernel.impl.core.TokenHolders;
	using CoreAPIAvailabilityGuard = Org.Neo4j.Kernel.impl.coreapi.CoreAPIAvailabilityGuard;
	using AccessCapability = Org.Neo4j.Kernel.impl.factory.AccessCapability;
	using CanWrite = Org.Neo4j.Kernel.impl.factory.CanWrite;
	using CommunityCommitProcessFactory = Org.Neo4j.Kernel.impl.factory.CommunityCommitProcessFactory;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using GraphDatabaseFacade = Org.Neo4j.Kernel.impl.factory.GraphDatabaseFacade;
	using IndexConfigStore = Org.Neo4j.Kernel.impl.index.IndexConfigStore;
	using Locks = Org.Neo4j.Kernel.impl.locking.Locks;
	using StatementLocks = Org.Neo4j.Kernel.impl.locking.StatementLocks;
	using StatementLocksFactory = Org.Neo4j.Kernel.impl.locking.StatementLocksFactory;
	using Procedures = Org.Neo4j.Kernel.impl.proc.Procedures;
	using QueryEngineProvider = Org.Neo4j.Kernel.impl.query.QueryEngineProvider;
	using BufferedIdController = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.id.BufferedIdController;
	using IdController = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.id.IdController;
	using BufferingIdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.BufferingIdGeneratorFactory;
	using DefaultIdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using IdReuseEligibility = Org.Neo4j.Kernel.impl.store.id.IdReuseEligibility;
	using CommunityIdTypeConfigurationProvider = Org.Neo4j.Kernel.impl.store.id.configuration.CommunityIdTypeConfigurationProvider;
	using IdTypeConfigurationProvider = Org.Neo4j.Kernel.impl.store.id.configuration.IdTypeConfigurationProvider;
	using TransactionHeaderInformationFactory = Org.Neo4j.Kernel.impl.transaction.TransactionHeaderInformationFactory;
	using TransactionMonitor = Org.Neo4j.Kernel.impl.transaction.TransactionMonitor;
	using StoreCopyCheckPointMutex = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.StoreCopyCheckPointMutex;
	using LogFileCreationMonitor = Org.Neo4j.Kernel.impl.transaction.log.files.LogFileCreationMonitor;
	using DatabaseTransactionStats = Org.Neo4j.Kernel.impl.transaction.stats.DatabaseTransactionStats;
	using TransactionCounters = Org.Neo4j.Kernel.impl.transaction.stats.TransactionCounters;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using UnsatisfiedDependencyException = Org.Neo4j.Kernel.impl.util.UnsatisfiedDependencyException;
	using CollectionsFactorySupplier = Org.Neo4j.Kernel.impl.util.collection.CollectionsFactorySupplier;
	using FileSystemWatcherService = Org.Neo4j.Kernel.impl.util.watcher.FileSystemWatcherService;
	using DatabaseHealth = Org.Neo4j.Kernel.@internal.DatabaseHealth;
	using TransactionEventHandlers = Org.Neo4j.Kernel.@internal.TransactionEventHandlers;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using Tracers = Org.Neo4j.Kernel.monitoring.tracing.Tracers;
	using NullLog = Org.Neo4j.Logging.NullLog;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using SimpleLogService = Org.Neo4j.Logging.@internal.SimpleLogService;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using Clocks = Org.Neo4j.Time.Clocks;
	using SystemNanoClock = Org.Neo4j.Time.SystemNanoClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.RETURNS_MOCKS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.DEFAULT_DATABASE_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.default_schema_provider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexProvider.EMPTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.collection.CollectionsFactorySupplier_Fields.ON_HEAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.util.watcher.FileSystemWatcherService.EMPTY_WATCHER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.MockedNeoStores.mockedTokenHolders;

	public class NeoStoreDataSourceRule : ExternalResource
	{
		 private NeoStoreDataSource _dataSource;

		 public virtual NeoStoreDataSource GetDataSource( DatabaseLayout databaseLayout, FileSystemAbstraction fs, PageCache pageCache )
		 {
			  return GetDataSource( databaseLayout, fs, pageCache, new Dependencies() );
		 }

		 public virtual NeoStoreDataSource GetDataSource( DatabaseLayout databaseLayout, FileSystemAbstraction fs, PageCache pageCache, DependencyResolver otherCustomOverriddenDependencies )
		 {
			  ShutdownAnyRunning();

			  StatementLocksFactory locksFactory = mock( typeof( StatementLocksFactory ) );
			  StatementLocks statementLocks = mock( typeof( StatementLocks ) );
			  Org.Neo4j.Kernel.impl.locking.Locks_Client locks = mock( typeof( Org.Neo4j.Kernel.impl.locking.Locks_Client ) );
			  when( statementLocks.Optimistic() ).thenReturn(locks);
			  when( statementLocks.Pessimistic() ).thenReturn(locks);
			  when( locksFactory.NewInstance() ).thenReturn(statementLocks);

			  JobScheduler jobScheduler = mock( typeof( JobScheduler ), RETURNS_MOCKS );
			  Monitors monitors = new Monitors();

			  Dependencies mutableDependencies = new Dependencies( otherCustomOverriddenDependencies );

			  // Satisfy non-satisfied dependencies
			  Config config = Dependency( mutableDependencies, typeof( Config ), deps => Config.defaults() );
			  config.augment( default_schema_provider, EMPTY.ProviderDescriptor.name() );
			  LogService logService = Dependency( mutableDependencies, typeof( LogService ), deps => new SimpleLogService( NullLogProvider.Instance ) );
			  IdGeneratorFactory idGeneratorFactory = Dependency( mutableDependencies, typeof( IdGeneratorFactory ), deps => new DefaultIdGeneratorFactory( fs ) );
			  IdTypeConfigurationProvider idConfigurationProvider = Dependency( mutableDependencies, typeof( IdTypeConfigurationProvider ), deps => new CommunityIdTypeConfigurationProvider() );
			  DatabaseHealth databaseHealth = Dependency( mutableDependencies, typeof( DatabaseHealth ), deps => new DatabaseHealth( mock( typeof( DatabasePanicEventGenerator ) ), NullLog.Instance ) );
			  SystemNanoClock clock = Dependency( mutableDependencies, typeof( SystemNanoClock ), deps => Clocks.nanoClock() );
			  TransactionMonitor transactionMonitor = Dependency( mutableDependencies, typeof( TransactionMonitor ), deps => new DatabaseTransactionStats() );
			  DatabaseAvailabilityGuard databaseAvailabilityGuard = Dependency( mutableDependencies, typeof( DatabaseAvailabilityGuard ), deps => new DatabaseAvailabilityGuard( DEFAULT_DATABASE_NAME, deps.resolveDependency( typeof( SystemNanoClock ) ), NullLog.Instance ) );
			  Dependency( mutableDependencies, typeof( DiagnosticsManager ), deps => new DiagnosticsManager( NullLog.Instance ) );
			  Dependency( mutableDependencies, typeof( IndexProvider ), deps => EMPTY );

			  _dataSource = new NeoStoreDataSource( new TestDatabaseCreationContext( DEFAULT_DATABASE_NAME, databaseLayout, config, idGeneratorFactory, logService, mock( typeof( JobScheduler ), RETURNS_MOCKS ), mock( typeof( TokenNameLookup ) ), mutableDependencies, mockedTokenHolders(), locksFactory, mock(typeof(SchemaWriteGuard)), mock(typeof(TransactionEventHandlers)), IndexingService.NO_MONITOR, fs, transactionMonitor, databaseHealth, mock(typeof(LogFileCreationMonitor)), TransactionHeaderInformationFactory.DEFAULT, new CommunityCommitProcessFactory(), mock(typeof(InternalAutoIndexing)), mock(typeof(IndexConfigStore)), mock(typeof(ExplicitIndexProvider)), pageCache, new StandardConstraintSemantics(), monitors, new Tracers("null", NullLog.Instance, monitors, jobScheduler, clock), mock(typeof(Procedures)), Org.Neo4j.Io.pagecache.IOLimiter_Fields.Unlimited, databaseAvailabilityGuard, clock, new CanWrite(), new StoreCopyCheckPointMutex(), RecoveryCleanupWorkCollector.immediate(), new BufferedIdController(new BufferingIdGeneratorFactory(idGeneratorFactory, Org.Neo4j.Kernel.impl.store.id.IdReuseEligibility_Fields.Always, idConfigurationProvider), jobScheduler), DatabaseInfo.COMMUNITY, new TransactionVersionContextSupplier(), ON_HEAP, Collections.emptyList(), file => EMPTY_WATCHER, new GraphDatabaseFacade(), Iterables.empty() ) );
			  return _dataSource;
		 }

		 private static T Dependency<T>( Dependencies dependencies, Type type, System.Func<DependencyResolver, T> defaultSupplier )
		 {
				 type = typeof( T );
			  try
			  {
					return dependencies.ResolveDependency( type );
			  }
			  catch ( Exception e ) when ( e is System.ArgumentException || e is UnsatisfiedDependencyException )
			  {
					return dependencies.SatisfyDependency( defaultSupplier( dependencies ) );
			  }
		 }

		 private void ShutdownAnyRunning()
		 {
			  if ( _dataSource != null )
			  {
					_dataSource.stop();
			  }
		 }

		 protected internal override void After( bool successful )
		 {
			  ShutdownAnyRunning();
		 }

		 private class TestDatabaseCreationContext : DatabaseCreationContext
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly string DatabaseNameConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly DatabaseLayout DatabaseLayoutConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly Config ConfigConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly IdGeneratorFactory IdGeneratorFactoryConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly LogService LogServiceConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly JobScheduler SchedulerConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly TokenNameLookup TokenNameLookupConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly DependencyResolver DependencyResolverConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly TokenHolders TokenHoldersConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly StatementLocksFactory StatementLocksFactoryConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly SchemaWriteGuard SchemaWriteGuardConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly TransactionEventHandlers TransactionEventHandlersConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly IndexingService.Monitor IndexingServiceMonitorConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly FileSystemAbstraction FsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly TransactionMonitor TransactionMonitorConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly DatabaseHealth DatabaseHealthConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly LogFileCreationMonitor PhysicalLogMonitorConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly TransactionHeaderInformationFactory TransactionHeaderInformationFactoryConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly CommitProcessFactory CommitProcessFactoryConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly AutoIndexing AutoIndexingConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly IndexConfigStore IndexConfigStoreConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly ExplicitIndexProvider ExplicitIndexProviderConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly PageCache PageCacheConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly ConstraintSemantics ConstraintSemanticsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly Monitors MonitorsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly Tracers TracersConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly Procedures ProceduresConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly IOLimiter IoLimiterConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly DatabaseAvailabilityGuard DatabaseAvailabilityGuardConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly SystemNanoClock ClockConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly AccessCapability AccessCapabilityConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly StoreCopyCheckPointMutex StoreCopyCheckPointMutexConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly RecoveryCleanupWorkCollector RecoveryCleanupWorkCollectorConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly IdController IdControllerConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly DatabaseInfo DatabaseInfoConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly VersionContextSupplier VersionContextSupplierConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly CollectionsFactorySupplier CollectionsFactorySupplierConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final Iterable<org.neo4j.kernel.extension.KernelExtensionFactory<?>> kernelExtensionFactories;
			  internal readonly IEnumerable<KernelExtensionFactory<object>> KernelExtensionFactoriesConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly System.Func<File, FileSystemWatcherService> WatcherServiceFactoryConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly GraphDatabaseFacade FacadeConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly IEnumerable<QueryEngineProvider> EngineProvidersConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly DatabaseAvailability DatabaseAvailabilityConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly CoreAPIAvailabilityGuard CoreAPIAvailabilityGuardConflict;

			  internal TestDatabaseCreationContext<T1>( string databaseName, DatabaseLayout databaseLayout, Config config, IdGeneratorFactory idGeneratorFactory, LogService logService, JobScheduler scheduler, TokenNameLookup tokenNameLookup, DependencyResolver dependencyResolver, TokenHolders tokenHolders, StatementLocksFactory statementLocksFactory, SchemaWriteGuard schemaWriteGuard, TransactionEventHandlers transactionEventHandlers, IndexingService.Monitor indexingServiceMonitor, FileSystemAbstraction fs, TransactionMonitor transactionMonitor, DatabaseHealth databaseHealth, LogFileCreationMonitor physicalLogMonitor, TransactionHeaderInformationFactory transactionHeaderInformationFactory, CommitProcessFactory commitProcessFactory, AutoIndexing autoIndexing, IndexConfigStore indexConfigStore, ExplicitIndexProvider explicitIndexProvider, PageCache pageCache, ConstraintSemantics constraintSemantics, Monitors monitors, Tracers tracers, Procedures procedures, IOLimiter ioLimiter, DatabaseAvailabilityGuard databaseAvailabilityGuard, SystemNanoClock clock, AccessCapability accessCapability, StoreCopyCheckPointMutex storeCopyCheckPointMutex, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, IdController idController, DatabaseInfo databaseInfo, VersionContextSupplier versionContextSupplier, CollectionsFactorySupplier collectionsFactorySupplier, IEnumerable<T1> kernelExtensionFactories, System.Func<File, FileSystemWatcherService> watcherServiceFactory, GraphDatabaseFacade facade, IEnumerable<QueryEngineProvider> engineProviders )
			  {
					this.DatabaseNameConflict = databaseName;
					this.DatabaseLayoutConflict = databaseLayout;
					this.ConfigConflict = config;
					this.IdGeneratorFactoryConflict = idGeneratorFactory;
					this.LogServiceConflict = logService;
					this.SchedulerConflict = scheduler;
					this.TokenNameLookupConflict = tokenNameLookup;
					this.DependencyResolverConflict = dependencyResolver;
					this.TokenHoldersConflict = tokenHolders;
					this.StatementLocksFactoryConflict = statementLocksFactory;
					this.SchemaWriteGuardConflict = schemaWriteGuard;
					this.TransactionEventHandlersConflict = transactionEventHandlers;
					this.IndexingServiceMonitorConflict = indexingServiceMonitor;
					this.FsConflict = fs;
					this.TransactionMonitorConflict = transactionMonitor;
					this.DatabaseHealthConflict = databaseHealth;
					this.PhysicalLogMonitorConflict = physicalLogMonitor;
					this.TransactionHeaderInformationFactoryConflict = transactionHeaderInformationFactory;
					this.CommitProcessFactoryConflict = commitProcessFactory;
					this.AutoIndexingConflict = autoIndexing;
					this.IndexConfigStoreConflict = indexConfigStore;
					this.ExplicitIndexProviderConflict = explicitIndexProvider;
					this.PageCacheConflict = pageCache;
					this.ConstraintSemanticsConflict = constraintSemantics;
					this.MonitorsConflict = monitors;
					this.TracersConflict = tracers;
					this.ProceduresConflict = procedures;
					this.IoLimiterConflict = ioLimiter;
					this.DatabaseAvailabilityGuardConflict = databaseAvailabilityGuard;
					this.ClockConflict = clock;
					this.AccessCapabilityConflict = accessCapability;
					this.StoreCopyCheckPointMutexConflict = storeCopyCheckPointMutex;
					this.RecoveryCleanupWorkCollectorConflict = recoveryCleanupWorkCollector;
					this.IdControllerConflict = idController;
					this.DatabaseInfoConflict = databaseInfo;
					this.VersionContextSupplierConflict = versionContextSupplier;
					this.CollectionsFactorySupplierConflict = collectionsFactorySupplier;
					this.KernelExtensionFactoriesConflict = kernelExtensionFactories;
					this.WatcherServiceFactoryConflict = watcherServiceFactory;
					this.FacadeConflict = facade;
					this.EngineProvidersConflict = engineProviders;
					this.DatabaseAvailabilityConflict = new DatabaseAvailability( databaseAvailabilityGuard, mock( typeof( TransactionCounters ) ), clock, 0 );
					this.CoreAPIAvailabilityGuardConflict = new CoreAPIAvailabilityGuard( databaseAvailabilityGuard, 0 );
			  }

			  public virtual string DatabaseName
			  {
				  get
				  {
						return DatabaseNameConflict;
				  }
			  }

			  public virtual DatabaseLayout DatabaseLayout
			  {
				  get
				  {
						return DatabaseLayoutConflict;
				  }
			  }

			  public virtual Config Config
			  {
				  get
				  {
						return ConfigConflict;
				  }
			  }

			  public virtual IdGeneratorFactory IdGeneratorFactory
			  {
				  get
				  {
						return IdGeneratorFactoryConflict;
				  }
			  }

			  public virtual LogService LogService
			  {
				  get
				  {
						return LogServiceConflict;
				  }
			  }

			  public virtual JobScheduler Scheduler
			  {
				  get
				  {
						return SchedulerConflict;
				  }
			  }

			  public virtual TokenNameLookup TokenNameLookup
			  {
				  get
				  {
						return TokenNameLookupConflict;
				  }
			  }

			  public virtual DependencyResolver GlobalDependencies
			  {
				  get
				  {
						return DependencyResolverConflict;
				  }
			  }

			  public virtual DependencyResolver DependencyResolver
			  {
				  get
				  {
						return DependencyResolverConflict;
				  }
			  }

			  public virtual TokenHolders TokenHolders
			  {
				  get
				  {
						return TokenHoldersConflict;
				  }
			  }

			  public virtual Locks Locks
			  {
				  get
				  {
						return mock( typeof( Locks ) );
				  }
			  }

			  public virtual StatementLocksFactory StatementLocksFactory
			  {
				  get
				  {
						return StatementLocksFactoryConflict;
				  }
			  }

			  public virtual SchemaWriteGuard SchemaWriteGuard
			  {
				  get
				  {
						return SchemaWriteGuardConflict;
				  }
			  }

			  public virtual TransactionEventHandlers TransactionEventHandlers
			  {
				  get
				  {
						return TransactionEventHandlersConflict;
				  }
			  }

			  public virtual IndexingService.Monitor IndexingServiceMonitor
			  {
				  get
				  {
						return IndexingServiceMonitorConflict;
				  }
			  }

			  public virtual FileSystemAbstraction Fs
			  {
				  get
				  {
						return FsConflict;
				  }
			  }

			  public virtual TransactionMonitor TransactionMonitor
			  {
				  get
				  {
						return TransactionMonitorConflict;
				  }
			  }

			  public virtual DatabaseHealth DatabaseHealth
			  {
				  get
				  {
						return DatabaseHealthConflict;
				  }
			  }

			  public virtual LogFileCreationMonitor PhysicalLogMonitor
			  {
				  get
				  {
						return PhysicalLogMonitorConflict;
				  }
			  }

			  public virtual TransactionHeaderInformationFactory TransactionHeaderInformationFactory
			  {
				  get
				  {
						return TransactionHeaderInformationFactoryConflict;
				  }
			  }

			  public virtual CommitProcessFactory CommitProcessFactory
			  {
				  get
				  {
						return CommitProcessFactoryConflict;
				  }
			  }

			  public virtual AutoIndexing AutoIndexing
			  {
				  get
				  {
						return AutoIndexingConflict;
				  }
			  }

			  public virtual IndexConfigStore IndexConfigStore
			  {
				  get
				  {
						return IndexConfigStoreConflict;
				  }
			  }

			  public virtual ExplicitIndexProvider ExplicitIndexProvider
			  {
				  get
				  {
						return ExplicitIndexProviderConflict;
				  }
			  }

			  public virtual PageCache PageCache
			  {
				  get
				  {
						return PageCacheConflict;
				  }
			  }

			  public virtual ConstraintSemantics ConstraintSemantics
			  {
				  get
				  {
						return ConstraintSemanticsConflict;
				  }
			  }

			  public virtual Monitors Monitors
			  {
				  get
				  {
						return MonitorsConflict;
				  }
			  }

			  public virtual Tracers Tracers
			  {
				  get
				  {
						return TracersConflict;
				  }
			  }

			  public virtual Procedures Procedures
			  {
				  get
				  {
						return ProceduresConflict;
				  }
			  }

			  public virtual IOLimiter IoLimiter
			  {
				  get
				  {
						return IoLimiterConflict;
				  }
			  }

			  public virtual DatabaseAvailabilityGuard DatabaseAvailabilityGuard
			  {
				  get
				  {
						return DatabaseAvailabilityGuardConflict;
				  }
			  }

			  public virtual CoreAPIAvailabilityGuard CoreAPIAvailabilityGuard
			  {
				  get
				  {
						return CoreAPIAvailabilityGuardConflict;
				  }
			  }

			  public virtual SystemNanoClock Clock
			  {
				  get
				  {
						return ClockConflict;
				  }
			  }

			  public virtual AccessCapability AccessCapability
			  {
				  get
				  {
						return AccessCapabilityConflict;
				  }
			  }

			  public virtual StoreCopyCheckPointMutex StoreCopyCheckPointMutex
			  {
				  get
				  {
						return StoreCopyCheckPointMutexConflict;
				  }
			  }

			  public virtual RecoveryCleanupWorkCollector RecoveryCleanupWorkCollector
			  {
				  get
				  {
						return RecoveryCleanupWorkCollectorConflict;
				  }
			  }

			  public virtual IdController IdController
			  {
				  get
				  {
						return IdControllerConflict;
				  }
			  }

			  public virtual DatabaseInfo DatabaseInfo
			  {
				  get
				  {
						return DatabaseInfoConflict;
				  }
			  }

			  public virtual VersionContextSupplier VersionContextSupplier
			  {
				  get
				  {
						return VersionContextSupplierConflict;
				  }
			  }

			  public virtual CollectionsFactorySupplier CollectionsFactorySupplier
			  {
				  get
				  {
						return CollectionsFactorySupplierConflict;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public Iterable<org.neo4j.kernel.extension.KernelExtensionFactory<?>> getKernelExtensionFactories()
			  public virtual IEnumerable<KernelExtensionFactory<object>> KernelExtensionFactories
			  {
				  get
				  {
						return KernelExtensionFactoriesConflict;
				  }
			  }

			  public virtual System.Func<File, FileSystemWatcherService> WatcherServiceFactory
			  {
				  get
				  {
						return WatcherServiceFactoryConflict;
				  }
			  }

			  public virtual GraphDatabaseFacade Facade
			  {
				  get
				  {
						return FacadeConflict;
				  }
			  }

			  public virtual IEnumerable<QueryEngineProvider> EngineProviders
			  {
				  get
				  {
						return EngineProvidersConflict;
				  }
			  }

			  public virtual DatabaseAvailability DatabaseAvailability
			  {
				  get
				  {
						return DatabaseAvailabilityConflict;
				  }
			  }
		 }

	}

}