using System;
using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel
{

	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using Neo4Net.GraphDb;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using RecoveryCleanupWorkCollector = Neo4Net.Index.Internal.gbptree.RecoveryCleanupWorkCollector;
	using DiagnosticsManager = Neo4Net.Internal.Diagnostics.DiagnosticsManager;
	using Kernel = Neo4Net.Kernel.Api.Internal.Kernel;
	using TokenNameLookup = Neo4Net.Kernel.Api.Internal.TokenNameLookup;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using InwardKernel = Neo4Net.Kernel.api.InwardKernel;
	using AutoIndexing = Neo4Net.Kernel.api.explicitindex.AutoIndexing;
	using LabelScanStore = Neo4Net.Kernel.api.labelscan.LabelScanStore;
	using AvailabilityGuard = Neo4Net.Kernel.availability.AvailabilityGuard;
	using DatabaseAvailability = Neo4Net.Kernel.availability.DatabaseAvailability;
	using DatabaseAvailabilityGuard = Neo4Net.Kernel.availability.DatabaseAvailabilityGuard;
	using Config = Neo4Net.Kernel.configuration.Config;
	using DatabaseKernelExtensions = Neo4Net.Kernel.extension.DatabaseKernelExtensions;
	using Neo4Net.Kernel.extension;
	using CommitProcessFactory = Neo4Net.Kernel.Impl.Api.CommitProcessFactory;
	using DatabaseSchemaState = Neo4Net.Kernel.Impl.Api.DatabaseSchemaState;
	using ExplicitIndexProvider = Neo4Net.Kernel.Impl.Api.ExplicitIndexProvider;
	using ExplicitIndexTransactionStateProvider = Neo4Net.Kernel.Impl.Api.ExplicitIndexTransactionStateProvider;
	using KernelAuxTransactionStateManager = Neo4Net.Kernel.Impl.Api.KernelAuxTransactionStateManager;
	using KernelImpl = Neo4Net.Kernel.Impl.Api.KernelImpl;
	using KernelTransactions = Neo4Net.Kernel.Impl.Api.KernelTransactions;
	using KernelTransactionsSnapshot = Neo4Net.Kernel.Impl.Api.KernelTransactionsSnapshot;
	using SchemaState = Neo4Net.Kernel.Impl.Api.SchemaState;
	using SchemaWriteGuard = Neo4Net.Kernel.Impl.Api.SchemaWriteGuard;
	using StackingQueryRegistrationOperations = Neo4Net.Kernel.Impl.Api.StackingQueryRegistrationOperations;
	using StatementOperationParts = Neo4Net.Kernel.Impl.Api.StatementOperationParts;
	using TransactionCommitProcess = Neo4Net.Kernel.Impl.Api.TransactionCommitProcess;
	using TransactionHooks = Neo4Net.Kernel.Impl.Api.TransactionHooks;
	using IndexProviderMap = Neo4Net.Kernel.Impl.Api.index.IndexProviderMap;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using QueryRegistrationOperations = Neo4Net.Kernel.Impl.Api.operations.QueryRegistrationOperations;
	using ConstraintIndexCreator = Neo4Net.Kernel.Impl.Api.state.ConstraintIndexCreator;
	using KernelTransactionMonitor = Neo4Net.Kernel.Impl.Api.transaciton.monitor.KernelTransactionMonitor;
	using KernelTransactionMonitorScheduler = Neo4Net.Kernel.Impl.Api.transaciton.monitor.KernelTransactionMonitorScheduler;
	using ConstraintSemantics = Neo4Net.Kernel.impl.constraints.ConstraintSemantics;
	using TokenHolders = Neo4Net.Kernel.impl.core.TokenHolders;
	using AccessCapability = Neo4Net.Kernel.impl.factory.AccessCapability;
	using DatabaseInfo = Neo4Net.Kernel.impl.factory.DatabaseInfo;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using OperationalMode = Neo4Net.Kernel.impl.factory.OperationalMode;
	using ExplicitIndexStore = Neo4Net.Kernel.impl.index.ExplicitIndexStore;
	using IndexConfigStore = Neo4Net.Kernel.impl.index.IndexConfigStore;
	using LockService = Neo4Net.Kernel.impl.locking.LockService;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using ReentrantLockService = Neo4Net.Kernel.impl.locking.ReentrantLockService;
	using StatementLocksFactory = Neo4Net.Kernel.impl.locking.StatementLocksFactory;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using QueryEngineProvider = Neo4Net.Kernel.impl.query.QueryEngineProvider;
	using QueryExecutionEngine = Neo4Net.Kernel.impl.query.QueryExecutionEngine;
	using SimpleKernelContext = Neo4Net.Kernel.impl.spi.SimpleKernelContext;
	using RecordStorageEngine = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using IdController = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.id.IdController;
	using RecordFormatPropertyConfigurator = Neo4Net.Kernel.impl.store.format.RecordFormatPropertyConfigurator;
	using RecordFormatSelector = Neo4Net.Kernel.impl.store.format.RecordFormatSelector;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdBasedStoreEntityCounters = Neo4Net.Kernel.impl.store.stats.IdBasedStoreEntityCounters;
	using DatabaseMigrator = Neo4Net.Kernel.impl.storemigration.DatabaseMigrator;
	using VisibleMigrationProgressMonitor = Neo4Net.Kernel.impl.storemigration.monitoring.VisibleMigrationProgressMonitor;
	using StoreMigrator = Neo4Net.Kernel.impl.storemigration.participant.StoreMigrator;
	using TransactionHeaderInformationFactory = Neo4Net.Kernel.impl.transaction.TransactionHeaderInformationFactory;
	using TransactionMonitor = Neo4Net.Kernel.impl.transaction.TransactionMonitor;
	using BatchingTransactionAppender = Neo4Net.Kernel.impl.transaction.log.BatchingTransactionAppender;
	using LogVersionRepository = Neo4Net.Kernel.impl.transaction.log.LogVersionRepository;
	using LogVersionUpgradeChecker = Neo4Net.Kernel.impl.transaction.log.LogVersionUpgradeChecker;
	using LoggingLogFileMonitor = Neo4Net.Kernel.impl.transaction.log.LoggingLogFileMonitor;
	using LogicalTransactionStore = Neo4Net.Kernel.impl.transaction.log.LogicalTransactionStore;
	using PhysicalLogicalTransactionStore = Neo4Net.Kernel.impl.transaction.log.PhysicalLogicalTransactionStore;
	using ReadableClosablePositionAwareChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosablePositionAwareChannel;
	using TransactionAppender = Neo4Net.Kernel.impl.transaction.log.TransactionAppender;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using TransactionMetadataCache = Neo4Net.Kernel.impl.transaction.log.TransactionMetadataCache;
	using CheckPointScheduler = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointScheduler;
	using CheckPointThreshold = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointThreshold;
	using CheckPointerImpl = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointerImpl;
	using SimpleTriggerInfo = Neo4Net.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using StoreCopyCheckPointMutex = Neo4Net.Kernel.impl.transaction.log.checkpoint.StoreCopyCheckPointMutex;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using Neo4Net.Kernel.impl.transaction.log.entry;
	using LogFileCreationMonitor = Neo4Net.Kernel.impl.transaction.log.files.LogFileCreationMonitor;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using LogFilesBuilder = Neo4Net.Kernel.impl.transaction.log.files.LogFilesBuilder;
	using LogPruneStrategyFactory = Neo4Net.Kernel.impl.transaction.log.pruning.LogPruneStrategyFactory;
	using LogPruning = Neo4Net.Kernel.impl.transaction.log.pruning.LogPruning;
	using LogPruningImpl = Neo4Net.Kernel.impl.transaction.log.pruning.LogPruningImpl;
	using ReverseTransactionCursorLoggingMonitor = Neo4Net.Kernel.impl.transaction.log.reverse.ReverseTransactionCursorLoggingMonitor;
	using ReversedSingleFileTransactionCursor = Neo4Net.Kernel.impl.transaction.log.reverse.ReversedSingleFileTransactionCursor;
	using LogRotation = Neo4Net.Kernel.impl.transaction.log.rotation.LogRotation;
	using LogRotationImpl = Neo4Net.Kernel.impl.transaction.log.rotation.LogRotationImpl;
	using DefaultIndexProviderMap = Neo4Net.Kernel.impl.transaction.state.DefaultIndexProviderMap;
	using NeoStoreFileListing = Neo4Net.Kernel.impl.transaction.state.NeoStoreFileListing;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using SynchronizedArrayIdOrderingQueue = Neo4Net.Kernel.impl.util.SynchronizedArrayIdOrderingQueue;
	using CollectionsFactorySupplier = Neo4Net.Kernel.impl.util.collection.CollectionsFactorySupplier;
	using LogProgressReporter = Neo4Net.Kernel.impl.util.monitoring.LogProgressReporter;
	using ProgressReporter = Neo4Net.Kernel.impl.util.monitoring.ProgressReporter;
	using FileSystemWatcherService = Neo4Net.Kernel.impl.util.watcher.FileSystemWatcherService;
	using DatabaseHealth = Neo4Net.Kernel.Internal.DatabaseHealth;
	using TransactionEventHandlers = Neo4Net.Kernel.Internal.TransactionEventHandlers;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Lifecycles = Neo4Net.Kernel.Lifecycle.Lifecycles;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Tracers = Neo4Net.Kernel.monitoring.tracing.Tracers;
	using CorruptedLogsTruncator = Neo4Net.Kernel.recovery.CorruptedLogsTruncator;
	using DefaultRecoveryService = Neo4Net.Kernel.recovery.DefaultRecoveryService;
	using LogTailScanner = Neo4Net.Kernel.recovery.LogTailScanner;
	using LoggingLogTailScannerMonitor = Neo4Net.Kernel.recovery.LoggingLogTailScannerMonitor;
	using Recovery = Neo4Net.Kernel.recovery.Recovery;
	using RecoveryMonitor = Neo4Net.Kernel.recovery.RecoveryMonitor;
	using RecoveryService = Neo4Net.Kernel.recovery.RecoveryService;
	using RecoveryStartInformationProvider = Neo4Net.Kernel.recovery.RecoveryStartInformationProvider;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using LogService = Neo4Net.Logging.Internal.LogService;
	using CpuClock = Neo4Net.Resources.CpuClock;
	using HeapAllocation = Neo4Net.Resources.HeapAllocation;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using NodePropertyAccessor = Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor;
	using StorageEngine = Neo4Net.Kernel.Api.StorageEngine.StorageEngine;
	using StoreFileMetadata = Neo4Net.Kernel.Api.StorageEngine.StoreFileMetadata;
	using StoreId = Neo4Net.Kernel.Api.StorageEngine.StoreId;
	using SystemNanoClock = Neo4Net.Time.SystemNanoClock;
	using VisibleForTesting = Neo4Net.Utils.VisibleForTesting;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.Exceptions.throwIfUnchecked;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.extension.KernelExtensionFailureStrategies.fail;

	public class NeoStoreDataSource : LifecycleAdapter
	{
		 public const string DEFAULT_DATA_SOURCE_NAME = "nioneodb";

		 private readonly Monitors _monitors;
		 private readonly Tracers _tracers;

		 private readonly Log _msgLog;
		 private readonly LogService _logService;
		 private readonly AutoIndexing _autoIndexing;
		 private readonly LogProvider _logProvider;
		 private readonly LogProvider _userLogProvider;
		 private readonly DependencyResolver _dependencyResolver;
		 private readonly TokenNameLookup _tokenNameLookup;
		 private readonly TokenHolders _tokenHolders;
		 private readonly StatementLocksFactory _statementLocksFactory;
		 private readonly SchemaWriteGuard _schemaWriteGuard;
		 private readonly TransactionEventHandlers _transactionEventHandlers;
		 private readonly IdGeneratorFactory _idGeneratorFactory;
		 private readonly IJobScheduler _scheduler;
		 private readonly Config _config;
		 private readonly LockService _lockService;
		 private readonly IndexingService.Monitor _indexingServiceMonitor;
		 private readonly FileSystemAbstraction _fs;
		 private readonly TransactionMonitor _transactionMonitor;
		 private readonly DatabaseHealth _databaseHealth;
		 private readonly LogFileCreationMonitor _physicalLogMonitor;
		 private readonly TransactionHeaderInformationFactory _transactionHeaderInformationFactory;
		 private readonly CommitProcessFactory _commitProcessFactory;
		 private readonly PageCache _pageCache;
		 private readonly ConstraintSemantics _constraintSemantics;
		 private readonly Procedures _procedures;
		 private readonly IOLimiter _ioLimiter;
		 private readonly DatabaseAvailabilityGuard _databaseAvailabilityGuard;
		 private readonly SystemNanoClock _clock;
		 private readonly IndexConfigStore _indexConfigStore;
		 private readonly ExplicitIndexProvider _explicitIndexProvider;
		 private readonly StoreCopyCheckPointMutex _storeCopyCheckPointMutex;
		 private readonly CollectionsFactorySupplier _collectionsFactorySupplier;
		 private readonly Locks _locks;
		 private readonly DatabaseAvailability _databaseAvailability;

		 private Dependencies _dataSourceDependencies;
		 private LifeSupport _life;
		 private IndexProviderMap _indexProviderMap;
		 private readonly string _databaseName;
		 private readonly DatabaseLayout _databaseLayout;
		 private readonly bool _readOnly;
		 private readonly IdController _idController;
		 private readonly DatabaseInfo _databaseInfo;
		 private readonly RecoveryCleanupWorkCollector _recoveryCleanupWorkCollector;
		 private readonly VersionContextSupplier _versionContextSupplier;
		 private readonly AccessCapability _accessCapability;

		 private StorageEngine _storageEngine;
		 private QueryExecutionEngine _executionEngine;
		 private NeoStoreTransactionLogModule _transactionLogModule;
		 private NeoStoreKernelModule _kernelModule;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final Iterable<org.Neo4Net.kernel.extension.KernelExtensionFactory<?>> kernelExtensionFactories;
		 private readonly IEnumerable<KernelExtensionFactory<object>> _kernelExtensionFactories;
		 private readonly System.Func<File, FileSystemWatcherService> _watcherServiceFactory;
		 private readonly GraphDatabaseFacade _facade;
		 private readonly IEnumerable<QueryEngineProvider> _engineProviders;
		 private readonly bool _failOnCorruptedLogFiles;
		 private readonly KernelAuxTransactionStateManager _auxTxStateManager;

		 public NeoStoreDataSource( DatabaseCreationContext context )
		 {
			  this._databaseName = context.DatabaseName;
			  this._databaseLayout = context.DatabaseLayout;
			  this._config = context.Config;
			  this._idGeneratorFactory = context.IdGeneratorFactory;
			  this._tokenNameLookup = context.TokenNameLookup;
			  this._dependencyResolver = context.GlobalDependencies;
			  this._scheduler = context.Scheduler;
			  this._logService = context.LogService;
			  this._autoIndexing = context.AutoIndexing;
			  this._indexConfigStore = context.IndexConfigStore;
			  this._explicitIndexProvider = context.ExplicitIndexProvider;
			  this._storeCopyCheckPointMutex = context.StoreCopyCheckPointMutex;
			  this._logProvider = context.LogService.InternalLogProvider;
			  this._userLogProvider = context.LogService.UserLogProvider;
			  this._tokenHolders = context.TokenHolders;
			  this._locks = context.Locks;
			  this._statementLocksFactory = context.StatementLocksFactory;
			  this._schemaWriteGuard = context.SchemaWriteGuard;
			  this._transactionEventHandlers = context.TransactionEventHandlers;
			  this._indexingServiceMonitor = context.IndexingServiceMonitor;
			  this._fs = context.Fs;
			  this._transactionMonitor = context.TransactionMonitor;
			  this._databaseHealth = context.DatabaseHealth;
			  this._physicalLogMonitor = context.PhysicalLogMonitor;
			  this._transactionHeaderInformationFactory = context.TransactionHeaderInformationFactory;
			  this._constraintSemantics = context.ConstraintSemantics;
			  this._monitors = context.Monitors;
			  this._tracers = context.Tracers;
			  this._procedures = context.Procedures;
			  this._ioLimiter = context.IoLimiter;
			  this._databaseAvailabilityGuard = context.DatabaseAvailabilityGuard;
			  this._clock = context.Clock;
			  this._accessCapability = context.AccessCapability;
			  this._recoveryCleanupWorkCollector = context.RecoveryCleanupWorkCollector;

			  this._readOnly = context.Config.get( GraphDatabaseSettings.read_only );
			  this._idController = context.IdController;
			  this._databaseInfo = context.DatabaseInfo;
			  this._versionContextSupplier = context.VersionContextSupplier;
			  this._kernelExtensionFactories = context.KernelExtensionFactories;
			  this._watcherServiceFactory = context.WatcherServiceFactory;
			  this._facade = context.Facade;
			  this._engineProviders = context.EngineProviders;
			  this._msgLog = _logProvider.getLog( this.GetType() );
			  this._lockService = new ReentrantLockService();
			  this._commitProcessFactory = context.CommitProcessFactory;
			  this._pageCache = context.PageCache;
			  this._collectionsFactorySupplier = context.CollectionsFactorySupplier;
			  this._databaseAvailability = context.DatabaseAvailability;
			  this._failOnCorruptedLogFiles = context.Config.get( GraphDatabaseSettings.fail_on_corrupted_log_files );
			  _auxTxStateManager = new KernelAuxTransactionStateManager();
		 }

		 // We do our own internal life management:
		 // start() does life.init() and life.start(),
		 // stop() does life.stop() and life.shutdown().
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws java.io.IOException
		 public override void Start()
		 {
			  _dataSourceDependencies = new Dependencies( _dependencyResolver );
			  _dataSourceDependencies.satisfyDependency( this );
			  _dataSourceDependencies.satisfyDependency( _monitors );
			  _dataSourceDependencies.satisfyDependency( _tokenHolders );
			  _dataSourceDependencies.satisfyDependency( _facade );
			  _dataSourceDependencies.satisfyDependency( _indexConfigStore );
			  _dataSourceDependencies.satisfyDependency( _explicitIndexProvider );
			  _dataSourceDependencies.satisfyDependency( _databaseHealth );
			  _dataSourceDependencies.satisfyDependency( _storeCopyCheckPointMutex );
			  _dataSourceDependencies.satisfyDependency( _transactionMonitor );
			  _dataSourceDependencies.satisfyDependency( _locks );
			  _dataSourceDependencies.satisfyDependency( _databaseAvailabilityGuard );
			  _dataSourceDependencies.satisfyDependency( _databaseAvailability );
			  _dataSourceDependencies.satisfyDependency( _idGeneratorFactory );
			  _dataSourceDependencies.satisfyDependency( _idController );
			  _dataSourceDependencies.satisfyDependency( new IdBasedStoreEntityCounters( this._idGeneratorFactory ) );
			  _dataSourceDependencies.satisfyDependency( _auxTxStateManager );

			  _life = new LifeSupport();
			  _dataSourceDependencies.satisfyDependency( _explicitIndexProvider );
			  _life.add( InitializeExtensions( _dataSourceDependencies ) );
			  _life.add( _recoveryCleanupWorkCollector );
			  _dataSourceDependencies.satisfyDependency( _lockService );
			  _life.add( _indexConfigStore );

			  FileSystemWatcherService watcherService = _watcherServiceFactory.apply( _databaseLayout.databaseDirectory() );
			  _life.add( watcherService );
			  _dataSourceDependencies.satisfyDependency( watcherService );

			  _life.add( Lifecycles.multiple( _explicitIndexProvider.allIndexProviders() ) );

			  // Check the tail of transaction logs and validate version
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.transaction.log.entry.LogEntryReader<org.Neo4Net.kernel.impl.transaction.log.ReadableClosablePositionAwareChannel> logEntryReader = new org.Neo4Net.kernel.impl.transaction.log.entry.VersionAwareLogEntryReader<>();
			  LogEntryReader<ReadableClosablePositionAwareChannel> logEntryReader = new VersionAwareLogEntryReader<ReadableClosablePositionAwareChannel>();

			  LogFiles logFiles = LogFilesBuilder.builder( _databaseLayout, _fs ).withLogEntryReader( logEntryReader ).withLogFileMonitor( _physicalLogMonitor ).withConfig( _config ).withDependencies( _dataSourceDependencies ).build();

			  LoggingLogFileMonitor logFileMonitor = new LoggingLogFileMonitor( _msgLog );
			  LoggingLogTailScannerMonitor tailMonitor = new LoggingLogTailScannerMonitor( _logService.getInternalLog( typeof( LogTailScanner ) ) );
			  ReverseTransactionCursorLoggingMonitor reverseCursorMonitor = new ReverseTransactionCursorLoggingMonitor( _logService.getInternalLog( typeof( ReversedSingleFileTransactionCursor ) ) );
			  _monitors.addMonitorListener( logFileMonitor );
			  _monitors.addMonitorListener( tailMonitor );
			  _monitors.addMonitorListener( reverseCursorMonitor );
			  _life.add(LifecycleAdapter.onShutdown(() =>
			  {
				// We might be started and stopped multiple times, so make sure we clean up after ourselves when we're stopped.
				_monitors.removeMonitorListener( logFileMonitor );
				_monitors.removeMonitorListener( tailMonitor );
				_monitors.removeMonitorListener( reverseCursorMonitor );
			  }));

			  LogTailScanner tailScanner = new LogTailScanner( logFiles, logEntryReader, _monitors, _failOnCorruptedLogFiles );
			  LogVersionUpgradeChecker.check( tailScanner, _config );

			  // Upgrade the store before we begin
			  RecordFormats formats = SelectStoreFormats( _config, _databaseLayout, _fs, _pageCache, _logService );
			  UpgradeStore( formats, tailScanner );

			  // Build all modules and their services
			  StorageEngine storageEngine = null;
			  try
			  {
					DatabaseSchemaState databaseSchemaState = new DatabaseSchemaState( _logProvider );

					SynchronizedArrayIdOrderingQueue explicitIndexTransactionOrdering = new SynchronizedArrayIdOrderingQueue();

					System.Func<KernelTransactionsSnapshot> transactionsSnapshotSupplier = () => _kernelModule.kernelTransactions().get();
					_idController.initialize( transactionsSnapshotSupplier );

					storageEngine = BuildStorageEngine( _explicitIndexProvider, _indexConfigStore, databaseSchemaState, explicitIndexTransactionOrdering, _databaseInfo.operationalMode, _versionContextSupplier );
					_life.add( logFiles );

					TransactionIdStore transactionIdStore = _dataSourceDependencies.resolveDependency( typeof( TransactionIdStore ) );

					_versionContextSupplier.init( transactionIdStore.getLastClosedTransactionId );

					LogVersionRepository logVersionRepository = _dataSourceDependencies.resolveDependency( typeof( LogVersionRepository ) );
					NeoStoreTransactionLogModule transactionLogModule = BuildTransactionLogs( logFiles, _config, _logProvider, _scheduler, storageEngine, logEntryReader, explicitIndexTransactionOrdering, transactionIdStore );
					transactionLogModule.SatisfyDependencies( _dataSourceDependencies );

					BuildRecovery( _fs, transactionIdStore, tailScanner, _monitors.newMonitor( typeof( RecoveryMonitor ) ), _monitors.newMonitor( typeof( RecoveryStartInformationProvider.Monitor ) ), logFiles, storageEngine, transactionLogModule.LogicalTransactionStore(), logVersionRepository );

					// At the time of writing this comes from the storage engine (IndexStoreView)
					NodePropertyAccessor nodePropertyAccessor = _dataSourceDependencies.resolveDependency( typeof( NodePropertyAccessor ) );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final NeoStoreKernelModule kernelModule = buildKernel(logFiles, transactionLogModule.transactionAppender(), dataSourceDependencies.resolveDependency(org.Neo4Net.kernel.impl.api.index.IndexingService.class), databaseSchemaState, dataSourceDependencies.resolveDependency(org.Neo4Net.kernel.api.labelscan.LabelScanStore.class), storageEngine, indexConfigStore, transactionIdStore, databaseAvailabilityGuard, clock, nodePropertyAccessor);
					NeoStoreKernelModule kernelModule = BuildKernel( logFiles, transactionLogModule.TransactionAppender(), _dataSourceDependencies.resolveDependency(typeof(IndexingService)), databaseSchemaState, _dataSourceDependencies.resolveDependency(typeof(LabelScanStore)), storageEngine, _indexConfigStore, transactionIdStore, _databaseAvailabilityGuard, _clock, nodePropertyAccessor );

					kernelModule.SatisfyDependencies( _dataSourceDependencies );

					// Do these assignments last so that we can ensure no cyclical dependencies exist
					this._storageEngine = storageEngine;
					this._transactionLogModule = transactionLogModule;
					this._kernelModule = kernelModule;

					_dataSourceDependencies.satisfyDependency( this );
					_dataSourceDependencies.satisfyDependency( databaseSchemaState );
					_dataSourceDependencies.satisfyDependency( logEntryReader );
					_dataSourceDependencies.satisfyDependency( storageEngine );

					_executionEngine = QueryEngineProvider.initialize( _dataSourceDependencies, _facade, _engineProviders );
			  }
			  catch ( Exception e )
			  {
					// Something unexpected happened during startup
					_msgLog.warn( "Exception occurred while setting up store modules. Attempting to close things down.", e );
					try
					{
						 // Close the neostore, so that locks are released properly
						 if ( storageEngine != null )
						 {
							  storageEngine.ForceClose();
						 }
					}
					catch ( Exception closeException )
					{
						 _msgLog.error( "Couldn't close neostore after startup failure", closeException );
					}
					throwIfUnchecked( e );
					throw new Exception( e );
			  }

			  _life.add( new DatabaseDiagnostics( _dataSourceDependencies.resolveDependency( typeof( DiagnosticsManager ) ), this, _databaseInfo ) );
			  _life.add( _databaseAvailability );
			  _life.Last = LifecycleToTriggerCheckPointOnShutdown();

			  try
			  {
					_life.start();
			  }
			  catch ( Exception e )
			  {
					// Something unexpected happened during startup
					_msgLog.warn( "Exception occurred while starting the datasource. Attempting to close things down.", e );
					try
					{
						 _life.shutdown();
						 // Close the neostore, so that locks are released properly
						 storageEngine.ForceClose();
					}
					catch ( Exception closeException )
					{
						 _msgLog.error( "Couldn't close neostore after startup failure", closeException );
					}
					throw new Exception( e );
			  }
			  /*
			   * At this point recovery has completed and the datasource is ready for use. Whatever panic might have
			   * happened before has been healed. So we can safely set the kernel health to ok.
			   * This right now has any real effect only in the case of internal restarts (for example, after a store copy
			   * in the case of HA). Standalone instances will have to be restarted by the user, as is proper for all
			   * kernel panics.
			   */
			  _databaseHealth.healed();
		 }

		 private LifeSupport InitializeExtensions( Dependencies dependencies )
		 {
			  LifeSupport extensionsLife = new LifeSupport();

			  extensionsLife.Add( new DatabaseKernelExtensions( new SimpleKernelContext( _databaseLayout.databaseDirectory(), _databaseInfo, dependencies ), _kernelExtensionFactories, dependencies, fail() ) );

			  _indexProviderMap = extensionsLife.Add( new DefaultIndexProviderMap( dependencies, _config ) );
			  dependencies.SatisfyDependency( _indexProviderMap );
			  extensionsLife.Init();
			  return extensionsLife;
		 }

		 private static RecordFormats SelectStoreFormats( Config config, DatabaseLayout databaseLayout, FileSystemAbstraction fs, PageCache pageCache, LogService logService )
		 {
			  LogProvider logging = logService.InternalLogProvider;
			  RecordFormats formats = RecordFormatSelector.selectNewestFormat( config, databaseLayout, fs, pageCache, logging );
			  ( new RecordFormatPropertyConfigurator( formats, config ) ).configure();
			  return formats;
		 }

		 private void UpgradeStore( RecordFormats format, LogTailScanner tailScanner )
		 {
			  VisibleMigrationProgressMonitor progressMonitor = new VisibleMigrationProgressMonitor( _logService.getUserLog( typeof( StoreMigrator ) ) );
			  ( new DatabaseMigrator( progressMonitor, _fs, _config, _logService, _indexProviderMap, _explicitIndexProvider, _pageCache, format, tailScanner, _scheduler ) ).migrate( _databaseLayout );
		 }

		 private StorageEngine BuildStorageEngine( ExplicitIndexProvider explicitIndexProviderLookup, IndexConfigStore indexConfigStore, SchemaState schemaState, SynchronizedArrayIdOrderingQueue explicitIndexTransactionOrdering, OperationalMode operationalMode, VersionContextSupplier versionContextSupplier )
		 {
			  RecordStorageEngine storageEngine = new RecordStorageEngine( _databaseLayout, _config, _pageCache, _fs, _logProvider, _userLogProvider, _tokenHolders, schemaState, _constraintSemantics, _scheduler, _tokenNameLookup, _lockService, _indexProviderMap, _indexingServiceMonitor, _databaseHealth, explicitIndexProviderLookup, indexConfigStore, explicitIndexTransactionOrdering, _idGeneratorFactory, _idController, _monitors, _recoveryCleanupWorkCollector, operationalMode, versionContextSupplier );

			  // We pretend that the storage engine abstract hides all details within it. Whereas that's mostly
			  // true it's not entirely true for the time being. As long as we need this call below, which
			  // makes available one or more internal things to the outside world, there are leaks to plug.
			  storageEngine.SatisfyDependencies( _dataSourceDependencies );

			  return _life.add( storageEngine );
		 }

		 private NeoStoreTransactionLogModule BuildTransactionLogs( LogFiles logFiles, Config config, LogProvider logProvider, IJobScheduler scheduler, StorageEngine storageEngine, LogEntryReader<ReadableClosablePositionAwareChannel> logEntryReader, SynchronizedArrayIdOrderingQueue explicitIndexTransactionOrdering, TransactionIdStore transactionIdStore )
		 {
			  TransactionMetadataCache transactionMetadataCache = new TransactionMetadataCache();
			  if ( config.Get( GraphDatabaseSettings.ephemeral ) )
			  {
					config.AugmentDefaults( GraphDatabaseSettings.keep_logical_logs, "1 files" );
			  }

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.transaction.log.pruning.LogPruning logPruning = new org.Neo4Net.kernel.impl.transaction.log.pruning.LogPruningImpl(fs, logFiles, logProvider, new org.Neo4Net.kernel.impl.transaction.log.pruning.LogPruneStrategyFactory(), clock, config);
			  LogPruning logPruning = new LogPruningImpl( _fs, logFiles, logProvider, new LogPruneStrategyFactory(), _clock, config );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.transaction.log.rotation.LogRotation logRotation = new org.Neo4Net.kernel.impl.transaction.log.rotation.LogRotationImpl(monitors.newMonitor(org.Neo4Net.kernel.impl.transaction.log.rotation.LogRotation_Monitor.class), logFiles, databaseHealth);
			  LogRotation logRotation = new LogRotationImpl( _monitors.newMonitor( typeof( Neo4Net.Kernel.impl.transaction.log.rotation.LogRotation_Monitor ) ), logFiles, _databaseHealth );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.transaction.log.TransactionAppender appender = life.add(new org.Neo4Net.kernel.impl.transaction.log.BatchingTransactionAppender(logFiles, logRotation, transactionMetadataCache, transactionIdStore, explicitIndexTransactionOrdering, databaseHealth));
			  TransactionAppender appender = _life.add( new BatchingTransactionAppender( logFiles, logRotation, transactionMetadataCache, transactionIdStore, explicitIndexTransactionOrdering, _databaseHealth ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.transaction.log.LogicalTransactionStore logicalTransactionStore = new org.Neo4Net.kernel.impl.transaction.log.PhysicalLogicalTransactionStore(logFiles, transactionMetadataCache, logEntryReader, monitors, failOnCorruptedLogFiles);
			  LogicalTransactionStore logicalTransactionStore = new PhysicalLogicalTransactionStore( logFiles, transactionMetadataCache, logEntryReader, _monitors, _failOnCorruptedLogFiles );

			  CheckPointThreshold threshold = CheckPointThreshold.createThreshold( config, _clock, logPruning, logProvider );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.transaction.log.checkpoint.CheckPointerImpl checkPointer = new org.Neo4Net.kernel.impl.transaction.log.checkpoint.CheckPointerImpl(transactionIdStore, threshold, storageEngine, logPruning, appender, databaseHealth, logProvider, tracers.checkPointTracer, ioLimiter, storeCopyCheckPointMutex);
			  CheckPointerImpl checkPointer = new CheckPointerImpl( transactionIdStore, threshold, storageEngine, logPruning, appender, _databaseHealth, logProvider, _tracers.checkPointTracer, _ioLimiter, _storeCopyCheckPointMutex );

			  long recurringPeriod = threshold.CheckFrequencyMillis();
			  CheckPointScheduler checkPointScheduler = new CheckPointScheduler( checkPointer, _ioLimiter, scheduler, recurringPeriod, _databaseHealth );

			  _life.add( checkPointer );
			  _life.add( checkPointScheduler );

			  return new NeoStoreTransactionLogModule( logicalTransactionStore, logFiles, logRotation, checkPointer, appender, explicitIndexTransactionOrdering );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void buildRecovery(final org.Neo4Net.io.fs.FileSystemAbstraction fileSystemAbstraction, org.Neo4Net.kernel.impl.transaction.log.TransactionIdStore transactionIdStore, org.Neo4Net.kernel.recovery.LogTailScanner tailScanner, org.Neo4Net.kernel.recovery.RecoveryMonitor recoveryMonitor, org.Neo4Net.kernel.recovery.RecoveryStartInformationProvider.Monitor positionMonitor, final org.Neo4Net.kernel.impl.transaction.log.files.LogFiles logFiles, org.Neo4Net.Kernel.Api.StorageEngine.StorageEngine storageEngine, org.Neo4Net.kernel.impl.transaction.log.LogicalTransactionStore logicalTransactionStore, org.Neo4Net.kernel.impl.transaction.log.LogVersionRepository logVersionRepository)
		 private void BuildRecovery( FileSystemAbstraction fileSystemAbstraction, TransactionIdStore transactionIdStore, LogTailScanner tailScanner, RecoveryMonitor recoveryMonitor, RecoveryStartInformationProvider.Monitor positionMonitor, LogFiles logFiles, StorageEngine storageEngine, LogicalTransactionStore logicalTransactionStore, LogVersionRepository logVersionRepository )
		 {
			  RecoveryService recoveryService = new DefaultRecoveryService( storageEngine, tailScanner, transactionIdStore, logicalTransactionStore, logVersionRepository, positionMonitor );
			  CorruptedLogsTruncator logsTruncator = new CorruptedLogsTruncator( _databaseLayout.databaseDirectory(), logFiles, fileSystemAbstraction );
			  ProgressReporter progressReporter = new LogProgressReporter( _logService.getInternalLog( typeof( Recovery ) ) );
			  Lifecycle schemaLife = storageEngine.SchemaAndTokensLifecycle();
			  Recovery recovery = new Recovery( recoveryService, logsTruncator, schemaLife, recoveryMonitor, progressReporter, _failOnCorruptedLogFiles );
			  _life.add( recovery );
		 }

		 private NeoStoreKernelModule BuildKernel( LogFiles logFiles, TransactionAppender appender, IndexingService indexingService, DatabaseSchemaState databaseSchemaState, LabelScanStore labelScanStore, StorageEngine storageEngine, IndexConfigStore indexConfigStore, TransactionIdStore transactionIdStore, AvailabilityGuard databaseAvailabilityGuard, SystemNanoClock clock, NodePropertyAccessor nodePropertyAccessor )
		 {
			  AtomicReference<CpuClock> cpuClockRef = SetupCpuClockAtomicReference();
			  AtomicReference<HeapAllocation> heapAllocationRef = SetupHeapAllocationAtomicReference();

			  TransactionCommitProcess transactionCommitProcess = _commitProcessFactory.create( appender, storageEngine, _config );

			  /*
			   * This is used by explicit indexes and constraint indexes whenever a transaction is to be spawned
			   * from within an existing transaction. It smells, and we should look over alternatives when time permits.
			   */
			  System.Func<Kernel> kernelProvider = () => _kernelModule.kernelAPI();

			  ConstraintIndexCreator constraintIndexCreator = new ConstraintIndexCreator( kernelProvider, indexingService, nodePropertyAccessor, _logProvider );

			  ExplicitIndexStore explicitIndexStore = new ExplicitIndexStore( _config, indexConfigStore, kernelProvider, _explicitIndexProvider );

			  StatementOperationParts statementOperationParts = _dataSourceDependencies.satisfyDependency( BuildStatementOperations( cpuClockRef, heapAllocationRef ) );

			  TransactionHooks hooks = new TransactionHooks();
			  _auxTxStateManager.registerProvider( new ExplicitIndexTransactionStateProvider( indexConfigStore, _explicitIndexProvider ) );

			  KernelTransactions kernelTransactions = _life.add( new KernelTransactions( _config, _statementLocksFactory, constraintIndexCreator, statementOperationParts, _schemaWriteGuard, _transactionHeaderInformationFactory, transactionCommitProcess, _auxTxStateManager, hooks, _transactionMonitor, databaseAvailabilityGuard, _tracers, storageEngine, _procedures, transactionIdStore, clock, cpuClockRef, heapAllocationRef, _accessCapability, _autoIndexing, explicitIndexStore, _versionContextSupplier, _collectionsFactorySupplier, _constraintSemantics, databaseSchemaState, indexingService, _tokenHolders, DatabaseName, _dataSourceDependencies ) );

			  BuildTransactionMonitor( kernelTransactions, clock, _config );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.api.KernelImpl kernel = new org.Neo4Net.kernel.impl.api.KernelImpl(kernelTransactions, hooks, databaseHealth, transactionMonitor, procedures, config);
			  KernelImpl kernel = new KernelImpl( kernelTransactions, hooks, _databaseHealth, _transactionMonitor, _procedures, _config );

			  kernel.RegisterTransactionHook( _transactionEventHandlers );
			  _life.add( kernel );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.kernel.impl.transaction.state.NeoStoreFileListing fileListing = new org.Neo4Net.kernel.impl.transaction.state.NeoStoreFileListing(databaseLayout, logFiles, labelScanStore, indexingService, explicitIndexProvider, storageEngine);
			  NeoStoreFileListing fileListing = new NeoStoreFileListing( _databaseLayout, logFiles, labelScanStore, indexingService, _explicitIndexProvider, storageEngine );
			  _dataSourceDependencies.satisfyDependency( fileListing );

			  return new NeoStoreKernelModule( transactionCommitProcess, kernel, kernelTransactions, fileListing );
		 }

		 private AtomicReference<CpuClock> SetupCpuClockAtomicReference()
		 {
			  AtomicReference<CpuClock> cpuClock = new AtomicReference<CpuClock>( CpuClock.NOT_AVAILABLE );
			  System.Action<bool, bool> cpuClockUpdater = ( before, after ) =>
			  {
				if ( after )
				{
					 cpuClock.set( CpuClock.CPU_CLOCK );
				}
				else
				{
					 cpuClock.set( CpuClock.NOT_AVAILABLE );
				}
			  };
			  cpuClockUpdater( null, _config.get( GraphDatabaseSettings.track_query_cpu_time ) );
			  _config.registerDynamicUpdateListener( GraphDatabaseSettings.track_query_cpu_time, cpuClockUpdater );
			  return cpuClock;
		 }

		 private AtomicReference<HeapAllocation> SetupHeapAllocationAtomicReference()
		 {
			  AtomicReference<HeapAllocation> heapAllocation = new AtomicReference<HeapAllocation>( HeapAllocation.NOT_AVAILABLE );
			  System.Action<bool, bool> heapAllocationUpdater = ( before, after ) =>
			  {
				if ( after )
				{
					 heapAllocation.set( HeapAllocation.HEAP_ALLOCATION );
				}
				else
				{
					 heapAllocation.set( HeapAllocation.NOT_AVAILABLE );
				}
			  };
			  heapAllocationUpdater( null, _config.get( GraphDatabaseSettings.track_query_allocation ) );
			  _config.registerDynamicUpdateListener( GraphDatabaseSettings.track_query_allocation, heapAllocationUpdater );
			  return heapAllocation;
		 }

		 private void BuildTransactionMonitor( KernelTransactions kernelTransactions, SystemNanoClock clock, Config config )
		 {
			  KernelTransactionMonitor kernelTransactionTimeoutMonitor = new KernelTransactionMonitor( kernelTransactions, clock, _logService );
			  _dataSourceDependencies.satisfyDependency( kernelTransactionTimeoutMonitor );
			  KernelTransactionMonitorScheduler transactionMonitorScheduler = new KernelTransactionMonitorScheduler( kernelTransactionTimeoutMonitor, _scheduler, config.Get( GraphDatabaseSettings.transaction_monitor_check_interval ).toMillis() );
			  _life.add( transactionMonitorScheduler );
		 }

		 public override void Stop()
		 {
			 lock ( this )
			 {
				  if ( !_life.Running )
				  {
						return;
				  }
      
				  _life.stop();
				  AwaitAllClosingTransactions();
				  // Checkpointing is now triggered as part of life.shutdown see lifecycleToTriggerCheckPointOnShutdown()
				  // Shut down all services in here, effectively making the database unusable for anyone who tries.
				  _life.shutdown();
			 }
		 }

		 private void AwaitAllClosingTransactions()
		 {
			  KernelTransactions kernelTransactions = _kernelModule.kernelTransactions();
			  kernelTransactions.TerminateTransactions();

			  while ( kernelTransactions.HaveClosingTransaction() )
			  {
					LockSupport.parkNanos( TimeUnit.MILLISECONDS.toNanos( 10 ) );
			  }
		 }

		 private Lifecycle LifecycleToTriggerCheckPointOnShutdown()
		 {
			  // Write new checkpoint in the log only if the kernel is healthy.
			  // We cannot throw here since we need to shutdown without exceptions,
			  // so let's make the checkpointing part of the life, so LifeSupport can handle exceptions properly
			  return LifecycleAdapter.onShutdown(() =>
			  {
				if ( _databaseHealth.Healthy )
				{
					 // Flushing of neo stores happens as part of the checkpoint
					 _transactionLogModule.checkPointing().forceCheckPoint(new SimpleTriggerInfo("Database shutdown"));
				}
			  });
		 }

		 public virtual StoreId StoreId
		 {
			 get
			 {
				  return _storageEngine.StoreId;
			 }
		 }

		 public virtual DatabaseLayout DatabaseLayout
		 {
			 get
			 {
				  return _databaseLayout;
			 }
		 }

		 public virtual bool ReadOnly
		 {
			 get
			 {
				  return _readOnly;
			 }
		 }

		 public virtual QueryExecutionEngine ExecutionEngine
		 {
			 get
			 {
				  return _executionEngine;
			 }
		 }

		 public virtual InwardKernel Kernel
		 {
			 get
			 {
				  return _kernelModule.kernelAPI();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.graphdb.ResourceIterator<org.Neo4Net.Kernel.Api.StorageEngine.StoreFileMetadata> listStoreFiles(boolean includeLogs) throws java.io.IOException
		 public virtual ResourceIterator<StoreFileMetadata> ListStoreFiles( bool includeLogs )
		 {
			  NeoStoreFileListing.StoreFileListingBuilder fileListingBuilder = NeoStoreFileListing.builder();
			  if ( !includeLogs )
			  {
					fileListingBuilder.ExcludeLogFiles();
			  }
			  return fileListingBuilder.Build();
		 }

		 public virtual NeoStoreFileListing NeoStoreFileListing
		 {
			 get
			 {
				  return _kernelModule.fileListing();
			 }
		 }

		 public virtual void RegisterDiagnosticsWith( DiagnosticsManager manager )
		 {
			  _storageEngine.registerDiagnostics( manager );
			  manager.RegisterAll( typeof( DataSourceDiagnostics ), this );
		 }

		 public virtual DependencyResolver DependencyResolver
		 {
			 get
			 {
				  return _dataSourceDependencies;
			 }
		 }

		 private StatementOperationParts BuildStatementOperations( AtomicReference<CpuClock> cpuClockRef, AtomicReference<HeapAllocation> heapAllocationRef )
		 {
			  QueryRegistrationOperations queryRegistrationOperations = new StackingQueryRegistrationOperations( _clock, cpuClockRef, heapAllocationRef );

			  return new StatementOperationParts( queryRegistrationOperations );
		 }

		 /// <summary>
		 /// Hook that must be called before there is an HA mode switch (eg master/slave switch),
		 /// i.e. after state has changed to pending and before state is about to change to the new target state.
		 /// This must only be called when the database is otherwise inaccessible.
		 /// </summary>
		 public virtual void BeforeModeSwitch()
		 {
			  ClearTransactions();
		 }

		 private void ClearTransactions()
		 {
			  // We don't want to have buffered ids carry over to the new role
			  _storageEngine.clearBufferedIds();

			  // Get rid of all pooled transactions, as they will otherwise reference
			  // components that have been swapped out during the mode switch.
			  _kernelModule.kernelTransactions().disposeAll();
		 }

		 /// <summary>
		 /// Hook that must be called after an HA mode switch (eg master/slave switch) have completed.
		 /// This must only be called when the database is otherwise inaccessible.
		 /// </summary>
		 public virtual void AfterModeSwitch()
		 {
			  _storageEngine.loadSchemaCache();
			  ClearTransactions();
		 }

		 public virtual StoreCopyCheckPointMutex StoreCopyCheckPointMutex
		 {
			 get
			 {
				  return _storeCopyCheckPointMutex;
			 }
		 }

		 public virtual string DatabaseName
		 {
			 get
			 {
				  return _databaseName;
			 }
		 }

		 public virtual AutoIndexing AutoIndexing
		 {
			 get
			 {
				  return _autoIndexing;
			 }
		 }

		 public virtual TokenHolders TokenHolders
		 {
			 get
			 {
				  return _tokenHolders;
			 }
		 }

		 public virtual TransactionEventHandlers TransactionEventHandlers
		 {
			 get
			 {
				  return _transactionEventHandlers;
			 }
		 }

		 public virtual DatabaseAvailabilityGuard DatabaseAvailabilityGuard
		 {
			 get
			 {
				  return _databaseAvailabilityGuard;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting public org.Neo4Net.kernel.lifecycle.LifeSupport getLife()
		 public virtual LifeSupport Life
		 {
			 get
			 {
				  return _life;
			 }
		 }
	}

}