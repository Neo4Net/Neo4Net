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
namespace Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage
{

	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;
	using RecoveryCleanupWorkCollector = Org.Neo4j.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using DiagnosticsManager = Org.Neo4j.@internal.Diagnostics.DiagnosticsManager;
	using TokenNameLookup = Org.Neo4j.@internal.Kernel.Api.TokenNameLookup;
	using TransactionFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using ConstraintValidationException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.ConstraintValidationException;
	using CreateConstraintFailureException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.CreateConstraintFailureException;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using IOLimiter = Org.Neo4j.Io.pagecache.IOLimiter;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using VersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using TransactionApplyKernelException = Org.Neo4j.Kernel.Api.Exceptions.TransactionApplyKernelException;
	using LabelScanStore = Org.Neo4j.Kernel.api.labelscan.LabelScanStore;
	using LabelScanWriter = Org.Neo4j.Kernel.api.labelscan.LabelScanWriter;
	using LoggingMonitor = Org.Neo4j.Kernel.api.labelscan.LoggingMonitor;
	using TransactionCountingStateVisitor = Org.Neo4j.Kernel.api.txstate.TransactionCountingStateVisitor;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using BatchTransactionApplier = Org.Neo4j.Kernel.Impl.Api.BatchTransactionApplier;
	using BatchTransactionApplierFacade = Org.Neo4j.Kernel.Impl.Api.BatchTransactionApplierFacade;
	using CountsRecordState = Org.Neo4j.Kernel.Impl.Api.CountsRecordState;
	using CountsStoreBatchTransactionApplier = Org.Neo4j.Kernel.Impl.Api.CountsStoreBatchTransactionApplier;
	using ExplicitBatchIndexApplier = Org.Neo4j.Kernel.Impl.Api.ExplicitBatchIndexApplier;
	using ExplicitIndexApplierLookup = Org.Neo4j.Kernel.Impl.Api.ExplicitIndexApplierLookup;
	using ExplicitIndexProvider = Org.Neo4j.Kernel.Impl.Api.ExplicitIndexProvider;
	using IndexReaderFactory = Org.Neo4j.Kernel.Impl.Api.IndexReaderFactory;
	using SchemaState = Org.Neo4j.Kernel.Impl.Api.SchemaState;
	using TransactionApplier = Org.Neo4j.Kernel.Impl.Api.TransactionApplier;
	using TransactionApplierFacade = Org.Neo4j.Kernel.Impl.Api.TransactionApplierFacade;
	using IndexProviderMap = Org.Neo4j.Kernel.Impl.Api.index.IndexProviderMap;
	using IndexStoreView = Org.Neo4j.Kernel.Impl.Api.index.IndexStoreView;
	using IndexingService = Org.Neo4j.Kernel.Impl.Api.index.IndexingService;
	using IndexingServiceFactory = Org.Neo4j.Kernel.Impl.Api.index.IndexingServiceFactory;
	using IndexingUpdateService = Org.Neo4j.Kernel.Impl.Api.index.IndexingUpdateService;
	using FullLabelStream = Org.Neo4j.Kernel.Impl.Api.scan.FullLabelStream;
	using SchemaCache = Org.Neo4j.Kernel.Impl.Api.store.SchemaCache;
	using BridgingCacheAccess = Org.Neo4j.Kernel.impl.cache.BridgingCacheAccess;
	using ConstraintSemantics = Org.Neo4j.Kernel.impl.constraints.ConstraintSemantics;
	using CacheAccessBackDoor = Org.Neo4j.Kernel.impl.core.CacheAccessBackDoor;
	using TokenHolders = Org.Neo4j.Kernel.impl.core.TokenHolders;
	using OperationalMode = Org.Neo4j.Kernel.impl.factory.OperationalMode;
	using IndexConfigStore = Org.Neo4j.Kernel.impl.index.IndexConfigStore;
	using NativeLabelScanStore = Org.Neo4j.Kernel.impl.index.labelscan.NativeLabelScanStore;
	using LockGroup = Org.Neo4j.Kernel.impl.locking.LockGroup;
	using LockService = Org.Neo4j.Kernel.impl.locking.LockService;
	using IdController = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.id.IdController;
	using NeoStores = Org.Neo4j.Kernel.impl.store.NeoStores;
	using Org.Neo4j.Kernel.impl.store;
	using SchemaStorage = Org.Neo4j.Kernel.impl.store.SchemaStorage;
	using StoreFactory = Org.Neo4j.Kernel.impl.store.StoreFactory;
	using StoreType = Org.Neo4j.Kernel.impl.store.StoreType;
	using Org.Neo4j.Kernel.impl.store.format;
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;
	using CacheInvalidationBatchTransactionApplier = Org.Neo4j.Kernel.impl.transaction.command.CacheInvalidationBatchTransactionApplier;
	using HighIdBatchTransactionApplier = Org.Neo4j.Kernel.impl.transaction.command.HighIdBatchTransactionApplier;
	using IndexActivator = Org.Neo4j.Kernel.impl.transaction.command.IndexActivator;
	using IndexBatchTransactionApplier = Org.Neo4j.Kernel.impl.transaction.command.IndexBatchTransactionApplier;
	using IndexUpdatesWork = Org.Neo4j.Kernel.impl.transaction.command.IndexUpdatesWork;
	using LabelUpdateWork = Org.Neo4j.Kernel.impl.transaction.command.LabelUpdateWork;
	using NeoStoreBatchTransactionApplier = Org.Neo4j.Kernel.impl.transaction.command.NeoStoreBatchTransactionApplier;
	using IntegrityValidator = Org.Neo4j.Kernel.impl.transaction.state.IntegrityValidator;
	using DynamicIndexStoreView = Org.Neo4j.Kernel.impl.transaction.state.storeview.DynamicIndexStoreView;
	using NeoStoreIndexStoreView = Org.Neo4j.Kernel.impl.transaction.state.storeview.NeoStoreIndexStoreView;
	using DependencySatisfier = Org.Neo4j.Kernel.impl.util.DependencySatisfier;
	using IdOrderingQueue = Org.Neo4j.Kernel.impl.util.IdOrderingQueue;
	using DatabaseHealth = Org.Neo4j.Kernel.@internal.DatabaseHealth;
	using Lifecycle = Org.Neo4j.Kernel.Lifecycle.Lifecycle;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using IndexImplementation = Org.Neo4j.Kernel.spi.explicitindex.IndexImplementation;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using CommandReaderFactory = Org.Neo4j.Storageengine.Api.CommandReaderFactory;
	using CommandsToApply = Org.Neo4j.Storageengine.Api.CommandsToApply;
	using StorageCommand = Org.Neo4j.Storageengine.Api.StorageCommand;
	using StorageEngine = Org.Neo4j.Storageengine.Api.StorageEngine;
	using StorageReader = Org.Neo4j.Storageengine.Api.StorageReader;
	using StoreFileMetadata = Org.Neo4j.Storageengine.Api.StoreFileMetadata;
	using StoreId = Org.Neo4j.Storageengine.Api.StoreId;
	using TransactionApplicationMode = Org.Neo4j.Storageengine.Api.TransactionApplicationMode;
	using ResourceLocker = Org.Neo4j.Storageengine.Api.@lock.ResourceLocker;
	using SchemaRule = Org.Neo4j.Storageengine.Api.schema.SchemaRule;
	using ReadableTransactionState = Org.Neo4j.Storageengine.Api.txstate.ReadableTransactionState;
	using TxStateVisitor = Org.Neo4j.Storageengine.Api.txstate.TxStateVisitor;
	using VisibleForTesting = Org.Neo4j.Util.VisibleForTesting;
	using Org.Neo4j.Util.concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.locking.LockService.NO_LOCK_SERVICE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.TransactionApplicationMode.RECOVERY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.TransactionApplicationMode.REVERSE_RECOVERY;

	public class RecordStorageEngine : StorageEngine, Lifecycle
	{
		 private readonly IndexingService _indexingService;
		 private readonly NeoStores _neoStores;
		 private readonly TokenHolders _tokenHolders;
		 private readonly DatabaseHealth _databaseHealth;
		 private readonly IndexConfigStore _indexConfigStore;
		 private readonly SchemaCache _schemaCache;
		 private readonly IntegrityValidator _integrityValidator;
		 private readonly CacheAccessBackDoor _cacheAccess;
		 private readonly LabelScanStore _labelScanStore;
		 private readonly IndexProviderMap _indexProviderMap;
		 private readonly ExplicitIndexApplierLookup _explicitIndexApplierLookup;
		 private readonly SchemaState _schemaState;
		 private readonly SchemaStorage _schemaStorage;
		 private readonly ConstraintSemantics _constraintSemantics;
		 private readonly IdOrderingQueue _explicitIndexTransactionOrdering;
		 private readonly LockService _lockService;
		 private readonly WorkSync<System.Func<LabelScanWriter>, LabelUpdateWork> _labelScanStoreSync;
		 private readonly CommandReaderFactory _commandReaderFactory;
		 private readonly WorkSync<IndexingUpdateService, IndexUpdatesWork> _indexUpdatesSync;
		 private readonly IndexStoreView _indexStoreView;
		 private readonly ExplicitIndexProvider _explicitIndexProviderLookup;
		 private readonly IdController _idController;
		 private readonly int _denseNodeThreshold;
		 private readonly int _recordIdBatchSize;

		 public RecordStorageEngine( DatabaseLayout databaseLayout, Config config, PageCache pageCache, FileSystemAbstraction fs, LogProvider logProvider, LogProvider userLogProvider, TokenHolders tokenHolders, SchemaState schemaState, ConstraintSemantics constraintSemantics, JobScheduler scheduler, TokenNameLookup tokenNameLookup, LockService lockService, IndexProviderMap indexProviderMap, IndexingService.Monitor indexingServiceMonitor, DatabaseHealth databaseHealth, ExplicitIndexProvider explicitIndexProvider, IndexConfigStore indexConfigStore, IdOrderingQueue explicitIndexTransactionOrdering, IdGeneratorFactory idGeneratorFactory, IdController idController, Monitors monitors, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, OperationalMode operationalMode, VersionContextSupplier versionContextSupplier )
		 {
			  this._tokenHolders = tokenHolders;
			  this._schemaState = schemaState;
			  this._lockService = lockService;
			  this._databaseHealth = databaseHealth;
			  this._explicitIndexProviderLookup = explicitIndexProvider;
			  this._indexConfigStore = indexConfigStore;
			  this._constraintSemantics = constraintSemantics;
			  this._explicitIndexTransactionOrdering = explicitIndexTransactionOrdering;

			  this._idController = idController;
			  StoreFactory factory = new StoreFactory( databaseLayout, config, idGeneratorFactory, pageCache, fs, logProvider, versionContextSupplier );
			  _neoStores = factory.OpenAllNeoStores( true );

			  try
			  {
					_schemaCache = new SchemaCache( constraintSemantics, Collections.emptyList(), indexProviderMap );
					_schemaStorage = new SchemaStorage( _neoStores.SchemaStore );

					NeoStoreIndexStoreView neoStoreIndexStoreView = new NeoStoreIndexStoreView( lockService, _neoStores );
					bool readOnly = config.Get( GraphDatabaseSettings.read_only ) && operationalMode == OperationalMode.single;
					monitors.AddMonitorListener( new LoggingMonitor( logProvider.GetLog( typeof( NativeLabelScanStore ) ) ) );
					_labelScanStore = new NativeLabelScanStore( pageCache, databaseLayout, fs, new FullLabelStream( neoStoreIndexStoreView ), readOnly, monitors, recoveryCleanupWorkCollector );

					_indexStoreView = new DynamicIndexStoreView( neoStoreIndexStoreView, _labelScanStore, lockService, _neoStores, logProvider );
					this._indexProviderMap = indexProviderMap;
					_indexingService = IndexingServiceFactory.createIndexingService( config, scheduler, indexProviderMap, _indexStoreView, tokenNameLookup, Iterators.asList( _schemaStorage.loadAllSchemaRules() ), logProvider, userLogProvider, indexingServiceMonitor, schemaState, readOnly );

					_integrityValidator = new IntegrityValidator( _neoStores, _indexingService );
					_cacheAccess = new BridgingCacheAccess( _schemaCache, schemaState, tokenHolders );

					_explicitIndexApplierLookup = new Org.Neo4j.Kernel.Impl.Api.ExplicitIndexApplierLookup_Direct( explicitIndexProvider );

					_labelScanStoreSync = new WorkSync<Supplier<LabelScanWriter>, LabelUpdateWork>( _labelScanStore.newWriter );

					_commandReaderFactory = new RecordStorageCommandReaderFactory();
					_indexUpdatesSync = new WorkSync<IndexingUpdateService, IndexUpdatesWork>( _indexingService );

					_denseNodeThreshold = config.Get( GraphDatabaseSettings.dense_node_threshold );
					_recordIdBatchSize = config.Get( GraphDatabaseSettings.record_id_batch_size );
			  }
			  catch ( Exception failure )
			  {
					_neoStores.close();
					throw failure;
			  }
		 }

		 public override StorageReader NewReader()
		 {
			  System.Func<IndexReaderFactory> indexReaderFactory = () => new Org.Neo4j.Kernel.Impl.Api.IndexReaderFactory_Caching(_indexingService);
			  return new RecordStorageReader( _tokenHolders, _schemaStorage, _neoStores, _indexingService, _schemaCache, indexReaderFactory, _labelScanStore.newReader, AllocateCommandCreationContext() );
		 }

		 public override RecordStorageCommandCreationContext AllocateCommandCreationContext()
		 {
			  return new RecordStorageCommandCreationContext( _neoStores, _denseNodeThreshold, _recordIdBatchSize );
		 }

		 public override CommandReaderFactory CommandReaderFactory()
		 {
			  return _commandReaderFactory;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("resource") @Override public void createCommands(java.util.Collection<org.neo4j.storageengine.api.StorageCommand> commands, org.neo4j.storageengine.api.txstate.ReadableTransactionState txState, org.neo4j.storageengine.api.StorageReader storageReader, org.neo4j.storageengine.api.lock.ResourceLocker locks, long lastTransactionIdWhenStarted, org.neo4j.storageengine.api.txstate.TxStateVisitor_Decorator additionalTxStateVisitor) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException, org.neo4j.internal.kernel.api.exceptions.schema.CreateConstraintFailureException, org.neo4j.internal.kernel.api.exceptions.schema.ConstraintValidationException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public override void CreateCommands( ICollection<StorageCommand> commands, ReadableTransactionState txState, StorageReader storageReader, ResourceLocker locks, long lastTransactionIdWhenStarted, Org.Neo4j.Storageengine.Api.txstate.TxStateVisitor_Decorator additionalTxStateVisitor )
		 {
			  if ( txState != null )
			  {
					// We can make this cast here because we expected that the storageReader passed in here comes from
					// this storage engine itself, anything else is considered a bug. And we do know the inner workings
					// of the storage statements that we create.
					RecordStorageCommandCreationContext creationContext = ( ( RecordStorageReader ) storageReader ).CommandCreationContext;
					TransactionRecordState recordState = creationContext.CreateTransactionRecordState( _integrityValidator, lastTransactionIdWhenStarted, locks );

					// Visit transaction state and populate these record state objects
					TxStateVisitor txStateVisitor = new TransactionToRecordStateVisitor( recordState, _schemaState, _schemaStorage, _constraintSemantics );
					CountsRecordState countsRecordState = new CountsRecordState();
					txStateVisitor = additionalTxStateVisitor.apply( txStateVisitor );
					txStateVisitor = new TransactionCountingStateVisitor( txStateVisitor, storageReader, txState, countsRecordState );
					using ( TxStateVisitor visitor = txStateVisitor )
					{
						 txState.Accept( visitor );
					}

					// Convert record state into commands
					recordState.ExtractCommands( commands );
					countsRecordState.ExtractCommands( commands );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void apply(org.neo4j.storageengine.api.CommandsToApply batch, org.neo4j.storageengine.api.TransactionApplicationMode mode) throws Exception
		 public override void Apply( CommandsToApply batch, TransactionApplicationMode mode )
		 {
			  // Have these command appliers as separate try-with-resource to have better control over
			  // point between closing this and the locks above
			  try
			  {
					  using ( IndexActivator indexActivator = new IndexActivator( _indexingService ), LockGroup locks = new LockGroup(), BatchTransactionApplier batchApplier = Applier(mode, indexActivator) )
					  {
						while ( batch != null )
						{
							 using ( TransactionApplier txApplier = batchApplier.StartTx( batch, locks ) )
							 {
								  batch.Accept( txApplier );
							 }
							 batch = batch.Next();
						}
					  }
			  }
			  catch ( Exception cause )
			  {
					TransactionApplyKernelException kernelException = new TransactionApplyKernelException( cause, "Failed to apply transaction: %s", batch );
					_databaseHealth.panic( kernelException );
					throw kernelException;
			  }
		 }

		 /// <summary>
		 /// Creates a <seealso cref="BatchTransactionApplierFacade"/> that is to be used for all transactions
		 /// in a batch. Each transaction is handled by a <seealso cref="TransactionApplierFacade"/> which wraps the
		 /// individual <seealso cref="TransactionApplier"/>s returned by the wrapped <seealso cref="BatchTransactionApplier"/>s.
		 /// 
		 /// After all transactions have been applied the appliers are closed.
		 /// </summary>
		 protected internal virtual BatchTransactionApplierFacade Applier( TransactionApplicationMode mode, IndexActivator indexActivator )
		 {
			  List<BatchTransactionApplier> appliers = new List<BatchTransactionApplier>();
			  // Graph store application. The order of the decorated store appliers is irrelevant
			  appliers.Add( new NeoStoreBatchTransactionApplier( mode.version(), _neoStores, _cacheAccess, LockService(mode) ) );
			  if ( mode.needsHighIdTracking() )
			  {
					appliers.Add( new HighIdBatchTransactionApplier( _neoStores ) );
			  }
			  if ( mode.needsCacheInvalidationOnUpdates() )
			  {
					appliers.Add( new CacheInvalidationBatchTransactionApplier( _neoStores, _cacheAccess ) );
			  }
			  if ( mode.needsAuxiliaryStores() )
			  {
					// Counts store application
					appliers.Add( new CountsStoreBatchTransactionApplier( _neoStores.Counts, mode ) );

					// Schema index application
					appliers.Add( new IndexBatchTransactionApplier( _indexingService, _labelScanStoreSync, _indexUpdatesSync, _neoStores.NodeStore, _neoStores.RelationshipStore, _neoStores.PropertyStore, indexActivator ) );

					// Explicit index application
					appliers.Add( new ExplicitBatchIndexApplier( _indexConfigStore, _explicitIndexApplierLookup, _explicitIndexTransactionOrdering, mode ) );
			  }

			  // Perform the application
			  return new BatchTransactionApplierFacade( appliers.ToArray() );
		 }

		 private LockService LockService( TransactionApplicationMode mode )
		 {
			  return mode == RECOVERY || mode == REVERSE_RECOVERY ? NO_LOCK_SERVICE : _lockService;
		 }

		 public virtual void SatisfyDependencies( DependencySatisfier satisfier )
		 {
			  satisfier.SatisfyDependency( _explicitIndexApplierLookup );
			  satisfier.SatisfyDependency( _cacheAccess );
			  satisfier.SatisfyDependency( _indexProviderMap );
			  satisfier.SatisfyDependency( _integrityValidator );
			  satisfier.SatisfyDependency( _labelScanStore );
			  satisfier.SatisfyDependency( _indexingService );
			  satisfier.SatisfyDependency( _neoStores.MetaDataStore );
			  satisfier.SatisfyDependency( _indexStoreView );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void init() throws Throwable
		 public override void Init()
		 {
			  _labelScanStore.init();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
		 public override void Start()
		 {
			  _neoStores.makeStoreOk();
			  _neoStores.startCountStore(); // TODO: move this to counts store lifecycle
			  _indexingService.start();
			  _labelScanStore.start();
			  _idController.start();
		 }

		 public override void LoadSchemaCache()
		 {
			  IList<SchemaRule> schemaRules = Iterators.asList( _neoStores.SchemaStore.loadAllSchemaRules() );
			  _schemaCache.load( schemaRules );
		 }

		 public override void ClearBufferedIds()
		 {
			  _idController.clear();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws Throwable
		 public override void Stop()
		 {
			  _indexingService.stop();
			  _labelScanStore.stop();
			  _idController.stop();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown() throws Throwable
		 public override void Shutdown()
		 {
			  _indexingService.shutdown();
			  _labelScanStore.shutdown();
			  _neoStores.close();
		 }

		 public override void FlushAndForce( IOLimiter limiter )
		 {
			  _indexingService.forceAll( limiter );
			  _labelScanStore.force( limiter );
			  foreach ( IndexImplementation index in _explicitIndexProviderLookup.allIndexProviders() )
			  {
					index.Force();
			  }
			  _neoStores.flush( limiter );
		 }

		 public override void RegisterDiagnostics( DiagnosticsManager diagnosticsManager )
		 {
			  _neoStores.registerDiagnostics( diagnosticsManager );
		 }

		 public override void ForceClose()
		 {
			  try
			  {
					Shutdown();
			  }
			  catch ( Exception e )
			  {
					throw new Exception( e );
			  }
		 }

		 public override void PrepareForRecoveryRequired()
		 {
			  _neoStores.deleteIdGenerators();
		 }

		 public override ICollection<StoreFileMetadata> ListStorageFiles()
		 {
			  IList<StoreFileMetadata> files = new List<StoreFileMetadata>();
			  foreach ( StoreType type in StoreType.values() )
			  {
					if ( type.Equals( StoreType.COUNTS ) )
					{
						 AddCountStoreFiles( files );
					}
					else
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.store.RecordStore<org.neo4j.kernel.impl.store.record.AbstractBaseRecord> recordStore = neoStores.getRecordStore(type);
						 RecordStore<AbstractBaseRecord> recordStore = _neoStores.getRecordStore( type );
						 StoreFileMetadata metadata = new StoreFileMetadata( recordStore.StorageFile, recordStore.RecordSize );
						 Files.Add( metadata );
					}
			  }
			  return files;
		 }

		 private void AddCountStoreFiles( IList<StoreFileMetadata> files )
		 {
			  IEnumerable<File> countStoreFiles = _neoStores.Counts.allFiles();
			  foreach ( File countStoreFile in countStoreFiles )
			  {
					StoreFileMetadata countStoreFileMetadata = new StoreFileMetadata( countStoreFile, Org.Neo4j.Kernel.impl.store.format.RecordFormat_Fields.NO_RECORD_SIZE );
					Files.Add( countStoreFileMetadata );
			  }
		 }

		 /// <returns> the underlying <seealso cref="NeoStores"/> which should <strong>ONLY</strong> be accessed by tests
		 /// until all tests are properly converted to not rely on access to <seealso cref="NeoStores"/>. Currently there
		 /// are important tests which asserts details about the neo stores that are very important to test,
		 /// but to convert all those tests might be a bigger piece of work. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting public org.neo4j.kernel.impl.store.NeoStores testAccessNeoStores()
		 public virtual NeoStores TestAccessNeoStores()
		 {
			  return _neoStores;
		 }

		 public virtual StoreId StoreId
		 {
			 get
			 {
				  return _neoStores.MetaDataStore.StoreId;
			 }
		 }

		 public override Lifecycle SchemaAndTokensLifecycle()
		 {
			  return new LifecycleAdapterAnonymousInnerClass( this );
		 }

		 private class LifecycleAdapterAnonymousInnerClass : LifecycleAdapter
		 {
			 private readonly RecordStorageEngine _outerInstance;

			 public LifecycleAdapterAnonymousInnerClass( RecordStorageEngine outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void init()
			 {
				  _outerInstance.tokenHolders.propertyKeyTokens().InitialTokens = _outerInstance.neoStores.PropertyKeyTokenStore.Tokens;
				  _outerInstance.tokenHolders.relationshipTypeTokens().InitialTokens = _outerInstance.neoStores.RelationshipTypeTokenStore.Tokens;
				  _outerInstance.tokenHolders.labelTokens().InitialTokens = _outerInstance.neoStores.LabelTokenStore.Tokens;
				  outerInstance.LoadSchemaCache();
				  _outerInstance.indexingService.init();
			 }
		 }
	}

}