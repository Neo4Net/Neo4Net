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
namespace Org.Neo4j.Graphdb.factory.module
{

	using DatabaseEditionContext = Org.Neo4j.Graphdb.factory.module.edition.context.DatabaseEditionContext;
	using DatabaseIdContext = Org.Neo4j.Graphdb.factory.module.id.DatabaseIdContext;
	using RecoveryCleanupWorkCollector = Org.Neo4j.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using TokenNameLookup = Org.Neo4j.@internal.Kernel.Api.TokenNameLookup;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using IOLimiter = Org.Neo4j.Io.pagecache.IOLimiter;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using VersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using DatabaseCreationContext = Org.Neo4j.Kernel.DatabaseCreationContext;
	using AutoIndexing = Org.Neo4j.Kernel.api.explicitindex.AutoIndexing;
	using DatabaseAvailability = Org.Neo4j.Kernel.availability.DatabaseAvailability;
	using DatabaseAvailabilityGuard = Org.Neo4j.Kernel.availability.DatabaseAvailabilityGuard;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Org.Neo4j.Kernel.extension;
	using CommitProcessFactory = Org.Neo4j.Kernel.Impl.Api.CommitProcessFactory;
	using DefaultExplicitIndexProvider = Org.Neo4j.Kernel.Impl.Api.DefaultExplicitIndexProvider;
	using ExplicitIndexProvider = Org.Neo4j.Kernel.Impl.Api.ExplicitIndexProvider;
	using NonTransactionalTokenNameLookup = Org.Neo4j.Kernel.Impl.Api.NonTransactionalTokenNameLookup;
	using SchemaWriteGuard = Org.Neo4j.Kernel.Impl.Api.SchemaWriteGuard;
	using InternalAutoIndexing = Org.Neo4j.Kernel.Impl.Api.explicitindex.InternalAutoIndexing;
	using IndexingService = Org.Neo4j.Kernel.Impl.Api.index.IndexingService;
	using ConstraintSemantics = Org.Neo4j.Kernel.impl.constraints.ConstraintSemantics;
	using TokenHolders = Org.Neo4j.Kernel.impl.core.TokenHolders;
	using CoreAPIAvailabilityGuard = Org.Neo4j.Kernel.impl.coreapi.CoreAPIAvailabilityGuard;
	using AccessCapability = Org.Neo4j.Kernel.impl.factory.AccessCapability;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using GraphDatabaseFacade = Org.Neo4j.Kernel.impl.factory.GraphDatabaseFacade;
	using IndexConfigStore = Org.Neo4j.Kernel.impl.index.IndexConfigStore;
	using Locks = Org.Neo4j.Kernel.impl.locking.Locks;
	using StatementLocksFactory = Org.Neo4j.Kernel.impl.locking.StatementLocksFactory;
	using Procedures = Org.Neo4j.Kernel.impl.proc.Procedures;
	using QueryEngineProvider = Org.Neo4j.Kernel.impl.query.QueryEngineProvider;
	using IdController = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.id.IdController;
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using TransactionHeaderInformationFactory = Org.Neo4j.Kernel.impl.transaction.TransactionHeaderInformationFactory;
	using TransactionMonitor = Org.Neo4j.Kernel.impl.transaction.TransactionMonitor;
	using StoreCopyCheckPointMutex = Org.Neo4j.Kernel.impl.transaction.log.checkpoint.StoreCopyCheckPointMutex;
	using LogFileCreationMonitor = Org.Neo4j.Kernel.impl.transaction.log.files.LogFileCreationMonitor;
	using DatabaseTransactionStats = Org.Neo4j.Kernel.impl.transaction.stats.DatabaseTransactionStats;
	using CollectionsFactorySupplier = Org.Neo4j.Kernel.impl.util.collection.CollectionsFactorySupplier;
	using FileSystemWatcherService = Org.Neo4j.Kernel.impl.util.watcher.FileSystemWatcherService;
	using DatabaseHealth = Org.Neo4j.Kernel.@internal.DatabaseHealth;
	using TransactionEventHandlers = Org.Neo4j.Kernel.@internal.TransactionEventHandlers;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using Tracers = Org.Neo4j.Kernel.monitoring.tracing.Tracers;
	using LogService = Org.Neo4j.Logging.@internal.LogService;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using SystemNanoClock = Org.Neo4j.Time.SystemNanoClock;

	public class ModularDatabaseCreationContext : DatabaseCreationContext
	{
		 private readonly string _databaseName;
		 private readonly Config _config;
		 private readonly IdGeneratorFactory _idGeneratorFactory;
		 private readonly LogService _logService;
		 private readonly JobScheduler _scheduler;
		 private readonly TokenNameLookup _tokenNameLookup;
		 private readonly DependencyResolver _globalDependencies;
		 private readonly TokenHolders _tokenHolders;
		 private readonly Locks _locks;
		 private readonly StatementLocksFactory _statementLocksFactory;
		 private readonly SchemaWriteGuard _schemaWriteGuard;
		 private readonly TransactionEventHandlers _transactionEventHandlers;
		 private readonly IndexingService.Monitor _indexingServiceMonitor;
		 private readonly FileSystemAbstraction _fs;
		 private readonly DatabaseTransactionStats _transactionStats;
		 private readonly DatabaseHealth _databaseHealth;
		 private readonly LogFileCreationMonitor _physicalLogMonitor;
		 private readonly TransactionHeaderInformationFactory _transactionHeaderInformationFactory;
		 private readonly CommitProcessFactory _commitProcessFactory;
		 private readonly AutoIndexing _autoIndexing;
		 private readonly IndexConfigStore _indexConfigStore;
		 private readonly ExplicitIndexProvider _explicitIndexProvider;
		 private readonly PageCache _pageCache;
		 private readonly ConstraintSemantics _constraintSemantics;
		 private readonly Monitors _monitors;
		 private readonly Tracers _tracers;
		 private readonly Procedures _procedures;
		 private readonly IOLimiter _ioLimiter;
		 private readonly DatabaseAvailabilityGuard _databaseAvailabilityGuard;
		 private readonly CoreAPIAvailabilityGuard _coreAPIAvailabilityGuard;
		 private readonly SystemNanoClock _clock;
		 private readonly AccessCapability _accessCapability;
		 private readonly StoreCopyCheckPointMutex _storeCopyCheckPointMutex;
		 private readonly RecoveryCleanupWorkCollector _recoveryCleanupWorkCollector;
		 private readonly IdController _idController;
		 private readonly DatabaseInfo _databaseInfo;
		 private readonly VersionContextSupplier _versionContextSupplier;
		 private readonly CollectionsFactorySupplier _collectionsFactorySupplier;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final Iterable<org.neo4j.kernel.extension.KernelExtensionFactory<?>> kernelExtensionFactories;
		 private readonly IEnumerable<KernelExtensionFactory<object>> _kernelExtensionFactories;
		 private readonly System.Func<File, FileSystemWatcherService> _watcherServiceFactory;
		 private readonly GraphDatabaseFacade _facade;
		 private readonly IEnumerable<QueryEngineProvider> _engineProviders;
		 private readonly DatabaseLayout _databaseLayout;
		 private readonly DatabaseAvailability _databaseAvailability;

		 internal ModularDatabaseCreationContext( string databaseName, PlatformModule platformModule, DatabaseEditionContext editionContext, Procedures procedures, GraphDatabaseFacade facade )
		 {
			  this._databaseName = databaseName;
			  this._config = platformModule.Config;
			  DatabaseIdContext idContext = editionContext.IdContext;
			  this._idGeneratorFactory = idContext.IdGeneratorFactory;
			  this._idController = idContext.IdController;
			  this._databaseLayout = platformModule.StoreLayout.databaseLayout( databaseName );
			  this._logService = platformModule.Logging;
			  this._scheduler = platformModule.JobScheduler;
			  this._globalDependencies = platformModule.Dependencies;
			  this._tokenHolders = editionContext.CreateTokenHolders();
			  this._tokenNameLookup = new NonTransactionalTokenNameLookup( _tokenHolders );
			  this._locks = editionContext.CreateLocks();
			  this._statementLocksFactory = editionContext.CreateStatementLocksFactory();
			  this._schemaWriteGuard = editionContext.SchemaWriteGuard;
			  this._transactionEventHandlers = new TransactionEventHandlers( facade );
			  this._monitors = new Monitors( platformModule.Monitors );
			  this._indexingServiceMonitor = _monitors.newMonitor( typeof( IndexingService.Monitor ) );
			  this._physicalLogMonitor = _monitors.newMonitor( typeof( LogFileCreationMonitor ) );
			  this._fs = platformModule.FileSystem;
			  this._transactionStats = editionContext.CreateTransactionMonitor();
			  this._databaseHealth = new DatabaseHealth( platformModule.PanicEventGenerator, _logService.getInternalLog( typeof( DatabaseHealth ) ) );
			  this._transactionHeaderInformationFactory = editionContext.HeaderInformationFactory;
			  this._commitProcessFactory = editionContext.CommitProcessFactory;
			  this._autoIndexing = new InternalAutoIndexing( platformModule.Config, _tokenHolders.propertyKeyTokens() );
			  this._indexConfigStore = new IndexConfigStore( _databaseLayout, _fs );
			  this._explicitIndexProvider = new DefaultExplicitIndexProvider();
			  this._pageCache = platformModule.PageCache;
			  this._constraintSemantics = editionContext.ConstraintSemantics;
			  this._tracers = platformModule.Tracers;
			  this._procedures = procedures;
			  this._ioLimiter = editionContext.IoLimiter;
			  this._clock = platformModule.Clock;
			  this._databaseAvailabilityGuard = editionContext.CreateDatabaseAvailabilityGuard( _clock, _logService, _config );
			  this._databaseAvailability = new DatabaseAvailability( _databaseAvailabilityGuard, _transactionStats, platformModule.Clock, AwaitActiveTransactionDeadlineMillis );
			  this._coreAPIAvailabilityGuard = new CoreAPIAvailabilityGuard( _databaseAvailabilityGuard, editionContext.TransactionStartTimeout );
			  this._accessCapability = editionContext.AccessCapability;
			  this._storeCopyCheckPointMutex = new StoreCopyCheckPointMutex();
			  this._recoveryCleanupWorkCollector = platformModule.RecoveryCleanupWorkCollector;
			  this._databaseInfo = platformModule.DatabaseInfo;
			  this._versionContextSupplier = platformModule.VersionContextSupplier;
			  this._collectionsFactorySupplier = platformModule.CollectionsFactorySupplier;
			  this._kernelExtensionFactories = platformModule.KernelExtensionFactories;
			  this._watcherServiceFactory = editionContext.WatcherServiceFactory;
			  this._facade = facade;
			  this._engineProviders = platformModule.EngineProviders;
		 }

		 public virtual string DatabaseName
		 {
			 get
			 {
				  return _databaseName;
			 }
		 }

		 public virtual DatabaseLayout DatabaseLayout
		 {
			 get
			 {
				  return _databaseLayout;
			 }
		 }

		 public virtual Config Config
		 {
			 get
			 {
				  return _config;
			 }
		 }

		 public virtual IdGeneratorFactory IdGeneratorFactory
		 {
			 get
			 {
				  return _idGeneratorFactory;
			 }
		 }

		 public virtual LogService LogService
		 {
			 get
			 {
				  return _logService;
			 }
		 }

		 public virtual JobScheduler Scheduler
		 {
			 get
			 {
				  return _scheduler;
			 }
		 }

		 public virtual TokenNameLookup TokenNameLookup
		 {
			 get
			 {
				  return _tokenNameLookup;
			 }
		 }

		 public virtual DependencyResolver GlobalDependencies
		 {
			 get
			 {
				  return _globalDependencies;
			 }
		 }

		 public virtual TokenHolders TokenHolders
		 {
			 get
			 {
				  return _tokenHolders;
			 }
		 }

		 public virtual Locks Locks
		 {
			 get
			 {
				  return _locks;
			 }
		 }

		 public virtual StatementLocksFactory StatementLocksFactory
		 {
			 get
			 {
				  return _statementLocksFactory;
			 }
		 }

		 public virtual SchemaWriteGuard SchemaWriteGuard
		 {
			 get
			 {
				  return _schemaWriteGuard;
			 }
		 }

		 public virtual TransactionEventHandlers TransactionEventHandlers
		 {
			 get
			 {
				  return _transactionEventHandlers;
			 }
		 }

		 public virtual IndexingService.Monitor IndexingServiceMonitor
		 {
			 get
			 {
				  return _indexingServiceMonitor;
			 }
		 }

		 public virtual FileSystemAbstraction Fs
		 {
			 get
			 {
				  return _fs;
			 }
		 }

		 public virtual TransactionMonitor TransactionMonitor
		 {
			 get
			 {
				  return _transactionStats;
			 }
		 }

		 public virtual DatabaseHealth DatabaseHealth
		 {
			 get
			 {
				  return _databaseHealth;
			 }
		 }

		 public virtual LogFileCreationMonitor PhysicalLogMonitor
		 {
			 get
			 {
				  return _physicalLogMonitor;
			 }
		 }

		 public virtual TransactionHeaderInformationFactory TransactionHeaderInformationFactory
		 {
			 get
			 {
				  return _transactionHeaderInformationFactory;
			 }
		 }

		 public virtual CommitProcessFactory CommitProcessFactory
		 {
			 get
			 {
				  return _commitProcessFactory;
			 }
		 }

		 public virtual AutoIndexing AutoIndexing
		 {
			 get
			 {
				  return _autoIndexing;
			 }
		 }

		 public virtual IndexConfigStore IndexConfigStore
		 {
			 get
			 {
				  return _indexConfigStore;
			 }
		 }

		 public virtual ExplicitIndexProvider ExplicitIndexProvider
		 {
			 get
			 {
				  return _explicitIndexProvider;
			 }
		 }

		 public virtual PageCache PageCache
		 {
			 get
			 {
				  return _pageCache;
			 }
		 }

		 public virtual ConstraintSemantics ConstraintSemantics
		 {
			 get
			 {
				  return _constraintSemantics;
			 }
		 }

		 public virtual Monitors Monitors
		 {
			 get
			 {
				  return _monitors;
			 }
		 }

		 public virtual Tracers Tracers
		 {
			 get
			 {
				  return _tracers;
			 }
		 }

		 public virtual Procedures Procedures
		 {
			 get
			 {
				  return _procedures;
			 }
		 }

		 public virtual IOLimiter IoLimiter
		 {
			 get
			 {
				  return _ioLimiter;
			 }
		 }

		 public virtual DatabaseAvailabilityGuard DatabaseAvailabilityGuard
		 {
			 get
			 {
				  return _databaseAvailabilityGuard;
			 }
		 }

		 public virtual CoreAPIAvailabilityGuard CoreAPIAvailabilityGuard
		 {
			 get
			 {
				  return _coreAPIAvailabilityGuard;
			 }
		 }

		 public virtual SystemNanoClock Clock
		 {
			 get
			 {
				  return _clock;
			 }
		 }

		 public virtual AccessCapability AccessCapability
		 {
			 get
			 {
				  return _accessCapability;
			 }
		 }

		 public virtual StoreCopyCheckPointMutex StoreCopyCheckPointMutex
		 {
			 get
			 {
				  return _storeCopyCheckPointMutex;
			 }
		 }

		 public virtual RecoveryCleanupWorkCollector RecoveryCleanupWorkCollector
		 {
			 get
			 {
				  return _recoveryCleanupWorkCollector;
			 }
		 }

		 public virtual IdController IdController
		 {
			 get
			 {
				  return _idController;
			 }
		 }

		 public virtual DatabaseInfo DatabaseInfo
		 {
			 get
			 {
				  return _databaseInfo;
			 }
		 }

		 public virtual VersionContextSupplier VersionContextSupplier
		 {
			 get
			 {
				  return _versionContextSupplier;
			 }
		 }

		 public virtual CollectionsFactorySupplier CollectionsFactorySupplier
		 {
			 get
			 {
				  return _collectionsFactorySupplier;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public Iterable<org.neo4j.kernel.extension.KernelExtensionFactory<?>> getKernelExtensionFactories()
		 public virtual IEnumerable<KernelExtensionFactory<object>> KernelExtensionFactories
		 {
			 get
			 {
				  return _kernelExtensionFactories;
			 }
		 }

		 public virtual System.Func<File, FileSystemWatcherService> WatcherServiceFactory
		 {
			 get
			 {
				  return _watcherServiceFactory;
			 }
		 }

		 public virtual GraphDatabaseFacade Facade
		 {
			 get
			 {
				  return _facade;
			 }
		 }

		 public virtual IEnumerable<QueryEngineProvider> EngineProviders
		 {
			 get
			 {
				  return _engineProviders;
			 }
		 }

		 public virtual DatabaseAvailability DatabaseAvailability
		 {
			 get
			 {
				  return _databaseAvailability;
			 }
		 }

		 private long AwaitActiveTransactionDeadlineMillis
		 {
			 get
			 {
				  return _config.get( GraphDatabaseSettings.shutdown_transaction_end_timeout ).toMillis();
			 }
		 }
	}

}