using System;
using System.Collections.Generic;
using System.Threading;

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
namespace Org.Neo4j.@unsafe.Batchinsert.@internal
{

	using PrimitiveLongCollections = Org.Neo4j.Collection.PrimitiveLongCollections;
	using ConstraintViolationException = Org.Neo4j.Graphdb.ConstraintViolationException;
	using Label = Org.Neo4j.Graphdb.Label;
	using NotFoundException = Org.Neo4j.Graphdb.NotFoundException;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using ConstraintCreator = Org.Neo4j.Graphdb.schema.ConstraintCreator;
	using ConstraintDefinition = Org.Neo4j.Graphdb.schema.ConstraintDefinition;
	using IndexCreator = Org.Neo4j.Graphdb.schema.IndexCreator;
	using IndexDefinition = Org.Neo4j.Graphdb.schema.IndexDefinition;
	using Service = Org.Neo4j.Helpers.Service;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using Org.Neo4j.Helpers.Collection;
	using RecoveryCleanupWorkCollector = Org.Neo4j.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using IndexReference = Org.Neo4j.@internal.Kernel.Api.IndexReference;
	using InternalIndexState = Org.Neo4j.@internal.Kernel.Api.InternalIndexState;
	using NamedToken = Org.Neo4j.@internal.Kernel.Api.NamedToken;
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using IndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using MisconfiguredIndexException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.MisconfiguredIndexException;
	using IndexProviderDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using LabelSchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.LabelSchemaDescriptor;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using IOLimiter = Org.Neo4j.Io.pagecache.IOLimiter;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using PageCacheTracer = Org.Neo4j.Io.pagecache.tracing.PageCacheTracer;
	using PageCursorTracerSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using IndexPopulationFailedKernelException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexPopulationFailedKernelException;
	using IndexProvider = Org.Neo4j.Kernel.Api.Index.IndexProvider;
	using SchemaDescriptorFactory = Org.Neo4j.Kernel.api.schema.SchemaDescriptorFactory;
	using ConstraintDescriptor = Org.Neo4j.Kernel.api.schema.constraints.ConstraintDescriptor;
	using ConstraintDescriptorFactory = Org.Neo4j.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using IndexBackedConstraintDescriptor = Org.Neo4j.Kernel.api.schema.constraints.IndexBackedConstraintDescriptor;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using DatabaseKernelExtensions = Org.Neo4j.Kernel.extension.DatabaseKernelExtensions;
	using Org.Neo4j.Kernel.extension;
	using KernelExtensionFailureStrategies = Org.Neo4j.Kernel.extension.KernelExtensionFailureStrategies;
	using DatabaseSchemaState = Org.Neo4j.Kernel.Impl.Api.DatabaseSchemaState;
	using NonTransactionalTokenNameLookup = Org.Neo4j.Kernel.Impl.Api.NonTransactionalTokenNameLookup;
	using IndexProviderMap = Org.Neo4j.Kernel.Impl.Api.index.IndexProviderMap;
	using IndexProxy = Org.Neo4j.Kernel.Impl.Api.index.IndexProxy;
	using IndexStoreView = Org.Neo4j.Kernel.Impl.Api.index.IndexStoreView;
	using IndexingService = Org.Neo4j.Kernel.Impl.Api.index.IndexingService;
	using IndexingServiceFactory = Org.Neo4j.Kernel.Impl.Api.index.IndexingServiceFactory;
	using FullLabelStream = Org.Neo4j.Kernel.Impl.Api.scan.FullLabelStream;
	using SchemaCache = Org.Neo4j.Kernel.Impl.Api.store.SchemaCache;
	using ConstraintSemantics = Org.Neo4j.Kernel.impl.constraints.ConstraintSemantics;
	using DelegatingTokenHolder = Org.Neo4j.Kernel.impl.core.DelegatingTokenHolder;
	using TokenHolder = Org.Neo4j.Kernel.impl.core.TokenHolder;
	using TokenHolders = Org.Neo4j.Kernel.impl.core.TokenHolders;
	using TokenNotFoundException = Org.Neo4j.Kernel.impl.core.TokenNotFoundException;
	using BaseNodeConstraintCreator = Org.Neo4j.Kernel.impl.coreapi.schema.BaseNodeConstraintCreator;
	using IndexCreatorImpl = Org.Neo4j.Kernel.impl.coreapi.schema.IndexCreatorImpl;
	using IndexDefinitionImpl = Org.Neo4j.Kernel.impl.coreapi.schema.IndexDefinitionImpl;
	using InternalSchemaActions = Org.Neo4j.Kernel.impl.coreapi.schema.InternalSchemaActions;
	using NodeKeyConstraintDefinition = Org.Neo4j.Kernel.impl.coreapi.schema.NodeKeyConstraintDefinition;
	using NodePropertyExistenceConstraintDefinition = Org.Neo4j.Kernel.impl.coreapi.schema.NodePropertyExistenceConstraintDefinition;
	using RelationshipPropertyExistenceConstraintDefinition = Org.Neo4j.Kernel.impl.coreapi.schema.RelationshipPropertyExistenceConstraintDefinition;
	using UniquenessConstraintDefinition = Org.Neo4j.Kernel.impl.coreapi.schema.UniquenessConstraintDefinition;
	using DatabaseInfo = Org.Neo4j.Kernel.impl.factory.DatabaseInfo;
	using IndexConfigStore = Org.Neo4j.Kernel.impl.index.IndexConfigStore;
	using NativeLabelScanStore = Org.Neo4j.Kernel.impl.index.labelscan.NativeLabelScanStore;
	using Locks = Org.Neo4j.Kernel.impl.locking.Locks;
	using NoOpClient = Org.Neo4j.Kernel.impl.locking.NoOpClient;
	using ConfiguringPageCacheFactory = Org.Neo4j.Kernel.impl.pagecache.ConfiguringPageCacheFactory;
	using PageCacheLifecycle = Org.Neo4j.Kernel.impl.pagecache.PageCacheLifecycle;
	using JobSchedulerFactory = Org.Neo4j.Kernel.impl.scheduler.JobSchedulerFactory;
	using SimpleKernelContext = Org.Neo4j.Kernel.impl.spi.SimpleKernelContext;
	using PropertyCreator = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.PropertyCreator;
	using PropertyDeleter = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.PropertyDeleter;
	using PropertyTraverser = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.PropertyTraverser;
	using RecordStorageReader = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.RecordStorageReader;
	using RelationshipCreator = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.RelationshipCreator;
	using RelationshipGroupGetter = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.RelationshipGroupGetter;
	using CountsComputer = Org.Neo4j.Kernel.impl.store.CountsComputer;
	using LabelTokenStore = Org.Neo4j.Kernel.impl.store.LabelTokenStore;
	using NeoStores = Org.Neo4j.Kernel.impl.store.NeoStores;
	using NodeLabels = Org.Neo4j.Kernel.impl.store.NodeLabels;
	using NodeStore = Org.Neo4j.Kernel.impl.store.NodeStore;
	using PropertyKeyTokenStore = Org.Neo4j.Kernel.impl.store.PropertyKeyTokenStore;
	using PropertyStore = Org.Neo4j.Kernel.impl.store.PropertyStore;
	using Org.Neo4j.Kernel.impl.store;
	using RelationshipStore = Org.Neo4j.Kernel.impl.store.RelationshipStore;
	using RelationshipTypeTokenStore = Org.Neo4j.Kernel.impl.store.RelationshipTypeTokenStore;
	using SchemaStore = Org.Neo4j.Kernel.impl.store.SchemaStore;
	using StoreFactory = Org.Neo4j.Kernel.impl.store.StoreFactory;
	using UnderlyingStorageException = Org.Neo4j.Kernel.impl.store.UnderlyingStorageException;
	using CountsTracker = Org.Neo4j.Kernel.impl.store.counts.CountsTracker;
	using RecordFormatSelector = Org.Neo4j.Kernel.impl.store.format.RecordFormatSelector;
	using RecordFormats = Org.Neo4j.Kernel.impl.store.format.RecordFormats;
	using DefaultIdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Org.Neo4j.Kernel.impl.store.id.IdType;
	using IdValidator = Org.Neo4j.Kernel.impl.store.id.validation.IdValidator;
	using ConstraintRule = Org.Neo4j.Kernel.impl.store.record.ConstraintRule;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using LabelTokenRecord = Org.Neo4j.Kernel.impl.store.record.LabelTokenRecord;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using PrimitiveRecord = Org.Neo4j.Kernel.impl.store.record.PrimitiveRecord;
	using PropertyBlock = Org.Neo4j.Kernel.impl.store.record.PropertyBlock;
	using PropertyKeyTokenRecord = Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using Record = Org.Neo4j.Kernel.impl.store.record.Record;
	using RelationshipGroupRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipGroupRecord;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using RelationshipTypeTokenRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipTypeTokenRecord;
	using DefaultIndexProviderMap = Org.Neo4j.Kernel.impl.transaction.state.DefaultIndexProviderMap;
	using Org.Neo4j.Kernel.impl.transaction.state;
	using Org.Neo4j.Kernel.impl.transaction.state;
	using DynamicIndexStoreView = Org.Neo4j.Kernel.impl.transaction.state.storeview.DynamicIndexStoreView;
	using NeoStoreIndexStoreView = Org.Neo4j.Kernel.impl.transaction.state.storeview.NeoStoreIndexStoreView;
	using Dependencies = Org.Neo4j.Kernel.impl.util.Dependencies;
	using ValueUtils = Org.Neo4j.Kernel.impl.util.ValueUtils;
	using GlobalStoreLocker = Org.Neo4j.Kernel.@internal.locker.GlobalStoreLocker;
	using StoreLocker = Org.Neo4j.Kernel.@internal.locker.StoreLocker;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using Log = Org.Neo4j.Logging.Log;
	using LogProvider = Org.Neo4j.Logging.LogProvider;
	using NullLog = Org.Neo4j.Logging.NullLog;
	using StoreLogService = Org.Neo4j.Logging.@internal.StoreLogService;
	using JobScheduler = Org.Neo4j.Scheduler.JobScheduler;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using IndexDescriptorFactory = Org.Neo4j.Storageengine.Api.schema.IndexDescriptorFactory;
	using SchemaRule = Org.Neo4j.Storageengine.Api.schema.SchemaRule;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;
	using VisibleForTesting = Org.Neo4j.Util.VisibleForTesting;
	using Value = Org.Neo4j.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static bool.Parse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongCollections.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.logs_directory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.store_internal_log_path;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Numbers.safeCastLongToInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.TokenRead_Fields.NO_TOKEN;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.IndexingService.NO_MONITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.locking.LockService.NO_LOCK_SERVICE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.NodeLabelsField.parseLabelsField;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.PropertyStore.encodeString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.Preconditions.checkState;

	public class BatchInserterImpl : BatchInserter, IndexConfigStoreProvider
	{
		 private readonly LifeSupport _life;
		 private readonly NeoStores _neoStores;
		 private readonly IndexConfigStore _indexStore;
		 private readonly DatabaseLayout _databaseLayout;
		 private readonly TokenHolders _tokenHolders;
		 private readonly IdGeneratorFactory _idGeneratorFactory;
		 private readonly IndexProviderMap _indexProviderMap;
		 private readonly Log _msgLog;
		 private readonly SchemaCache _schemaCache;
		 private readonly Config _config;
		 private readonly BatchInserterImpl.BatchSchemaActions _actions;
		 private readonly StoreLocker _storeLocker;
		 private readonly PageCache _pageCache;
		 private readonly RecordStorageReader _storageReader;
		 private readonly StoreLogService _logService;
		 private readonly FileSystemAbstraction _fileSystem;
		 private readonly Monitors _monitors;
		 private readonly JobScheduler _jobScheduler;
		 private bool _labelsTouched;
		 private bool _isShutdown;

		 private readonly System.Func<long, Label> labelIdToLabelFunction = ( long from ) =>
		 {
			try
			{
				 return label( _tokenHolders.labelTokens().getTokenById(safeCastLongToInt(from)).name() );
			}
			catch ( TokenNotFoundException e )
			{
				 throw new Exception( e );
			}
		 };

		 private readonly FlushStrategy _flushStrategy;
		 // Helper structure for setNodeProperty
		 private readonly RelationshipCreator _relationshipCreator;
		 private readonly DirectRecordAccessSet _recordAccess;
		 private readonly PropertyTraverser _propertyTraverser;
		 private readonly PropertyCreator _propertyCreator;
		 private readonly PropertyDeleter _propertyDeletor;

		 private readonly NodeStore _nodeStore;
		 private readonly RelationshipStore _relationshipStore;
		 private readonly RelationshipTypeTokenStore _relationshipTypeTokenStore;
		 private readonly PropertyKeyTokenStore _propertyKeyTokenStore;
		 private readonly PropertyStore _propertyStore;
		 private readonly SchemaStore _schemaStore;
		 private readonly NeoStoreIndexStoreView _storeIndexStoreView;

		 private readonly LabelTokenStore _labelTokenStore;
		 private readonly Org.Neo4j.Kernel.impl.locking.Locks_Client _noopLockClient = new NoOpClient();
		 private readonly long _maxNodeId;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public BatchInserterImpl(final java.io.File databaseDirectory, final org.neo4j.io.fs.FileSystemAbstraction fileSystem, java.util.Map<String, String> stringParams, Iterable<org.neo4j.kernel.extension.KernelExtensionFactory<?>> kernelExtensions) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public BatchInserterImpl<T1>( File databaseDirectory, FileSystemAbstraction fileSystem, IDictionary<string, string> stringParams, IEnumerable<T1> kernelExtensions )
		 {
			  RejectAutoUpgrade( stringParams );
			  IDictionary<string, string> @params = DefaultParams;
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
			  @params.putAll( stringParams );
			  this._config = Config.defaults( @params );
			  this._fileSystem = fileSystem;

			  _life = new LifeSupport();
			  this._databaseLayout = DatabaseLayout.of( databaseDirectory );
			  this._jobScheduler = JobSchedulerFactory.createInitialisedScheduler();
			  _life.add( _jobScheduler );

			  _storeLocker = TryLockStore( fileSystem );
			  ConfiguringPageCacheFactory pageCacheFactory = new ConfiguringPageCacheFactory( fileSystem, _config, PageCacheTracer.NULL, Org.Neo4j.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null, NullLog.Instance, EmptyVersionContextSupplier.EMPTY, _jobScheduler );
			  PageCache pageCache = pageCacheFactory.OrCreatePageCache;
			  _life.add( new PageCacheLifecycle( pageCache ) );

			  _config.augment( logs_directory, databaseDirectory.CanonicalPath );
			  File internalLog = _config.get( store_internal_log_path );

			  _logService = _life.add( StoreLogService.withInternalLog( internalLog ).build( fileSystem ) );
			  _msgLog = _logService.getInternalLog( this.GetType() );

			  bool dump = _config.get( GraphDatabaseSettings.dump_configuration );
			  this._idGeneratorFactory = new DefaultIdGeneratorFactory( fileSystem );

			  LogProvider internalLogProvider = _logService.InternalLogProvider;
			  RecordFormats recordFormats = RecordFormatSelector.selectForStoreOrConfig( _config, _databaseLayout, fileSystem, pageCache, internalLogProvider );
			  StoreFactory sf = new StoreFactory( this._databaseLayout, _config, _idGeneratorFactory, pageCache, fileSystem, recordFormats, internalLogProvider, EmptyVersionContextSupplier.EMPTY );

			  _maxNodeId = recordFormats.Node().MaxId;

			  if ( dump )
			  {
					DumpConfiguration( @params, System.out );
			  }
			  _msgLog.info( Thread.CurrentThread + " Starting BatchInserter(" + this + ")" );
			  _life.start();
			  _neoStores = sf.OpenAllNeoStores( true );
			  _neoStores.verifyStoreOk();
			  this._pageCache = pageCache;

			  _nodeStore = _neoStores.NodeStore;
			  _relationshipStore = _neoStores.RelationshipStore;
			  _relationshipTypeTokenStore = _neoStores.RelationshipTypeTokenStore;
			  _propertyKeyTokenStore = _neoStores.PropertyKeyTokenStore;
			  _propertyStore = _neoStores.PropertyStore;
			  RecordStore<RelationshipGroupRecord> relationshipGroupStore = _neoStores.RelationshipGroupStore;
			  _schemaStore = _neoStores.SchemaStore;
			  _labelTokenStore = _neoStores.LabelTokenStore;

			  _monitors = new Monitors();

			  _storeIndexStoreView = new NeoStoreIndexStoreView( NO_LOCK_SERVICE, _neoStores );
			  Dependencies deps = new Dependencies();
			  Monitors monitors = new Monitors();
	//<<<<<<< HEAD
			  deps.SatisfyDependencies( fileSystem, _config, _logService, _storeIndexStoreView, pageCache, monitors, RecoveryCleanupWorkCollector.immediate() );

			  DatabaseKernelExtensions extensions = _life.add( new DatabaseKernelExtensions( new SimpleKernelContext( databaseDirectory, DatabaseInfo.TOOL, deps ), kernelExtensions, deps, KernelExtensionFailureStrategies.ignore() ) );

			  _indexProviderMap = _life.add( new DefaultIndexProviderMap( extensions, _config ) );

			  TokenHolder propertyKeyTokenHolder = new DelegatingTokenHolder( this.createNewPropertyKeyId, Org.Neo4j.Kernel.impl.core.TokenHolder_Fields.TYPE_PROPERTY_KEY );
			  propertyKeyTokenHolder.InitialTokens = _propertyKeyTokenStore.Tokens;
			  TokenHolder relationshipTypeTokenHolder = new DelegatingTokenHolder( this.createNewRelationshipType, Org.Neo4j.Kernel.impl.core.TokenHolder_Fields.TYPE_RELATIONSHIP_TYPE );
			  relationshipTypeTokenHolder.InitialTokens = _relationshipTypeTokenStore.Tokens;
			  TokenHolder labelTokenHolder = new DelegatingTokenHolder( this.createNewLabelId, Org.Neo4j.Kernel.impl.core.TokenHolder_Fields.TYPE_LABEL );
			  labelTokenHolder.InitialTokens = _labelTokenStore.Tokens;
			  _tokenHolders = new TokenHolders( propertyKeyTokenHolder, labelTokenHolder, relationshipTypeTokenHolder );

			  _indexStore = _life.add( new IndexConfigStore( this._databaseLayout, fileSystem ) );
			  _schemaCache = new SchemaCache( LoadConstraintSemantics(), _schemaStore, _indexProviderMap );

			  _actions = new BatchSchemaActions( this );

			  // Record access
			  _recordAccess = new DirectRecordAccessSet( _neoStores );
			  _relationshipCreator = new RelationshipCreator( new RelationshipGroupGetter( relationshipGroupStore ), relationshipGroupStore.StoreHeaderInt );
			  _propertyTraverser = new PropertyTraverser();
			  _propertyCreator = new PropertyCreator( _propertyStore, _propertyTraverser );
			  _propertyDeletor = new PropertyDeleter( _propertyTraverser );

			  _flushStrategy = new BatchedFlushStrategy( _recordAccess, _config.get( GraphDatabaseSettings.batch_inserter_batch_size ) );
			  _storageReader = new RecordStorageReader( _neoStores );
		 }

		 private StoreLocker TryLockStore( FileSystemAbstraction fileSystem )
		 {
			  StoreLocker storeLocker = new GlobalStoreLocker( fileSystem, this._databaseLayout.StoreLayout );
			  try
			  {
					storeLocker.CheckLock();
			  }
			  catch ( Exception e )
			  {
					try
					{
						 storeLocker.Dispose();
					}
					catch ( IOException ce )
					{
						 e.addSuppressed( ce );
					}
					throw e;
			  }
			  return storeLocker;
		 }

		 private static IDictionary<string, string> DefaultParams
		 {
			 get
			 {
				  IDictionary<string, string> @params = new Dictionary<string, string>();
				  @params[GraphDatabaseSettings.pagecache_memory.name()] = "32m";
				  return @params;
			 }
		 }

		 public override bool NodeHasProperty( long node, string propertyName )
		 {
			  return PrimitiveHasProperty( GetNodeRecord( node ).forChangingData(), propertyName );
		 }

		 public override bool RelationshipHasProperty( long relationship, string propertyName )
		 {
			  return PrimitiveHasProperty( _recordAccess.RelRecords.getOrLoad( relationship, null ).forReadingData(), propertyName );
		 }

		 public override void SetNodeProperty( long node, string propertyName, object propertyValue )
		 {
			  RecordAccess_RecordProxy<NodeRecord, Void> nodeRecord = GetNodeRecord( node );
			  SetPrimitiveProperty( nodeRecord, propertyName, propertyValue );

			  _flushStrategy.flush();
		 }

		 public override void SetRelationshipProperty( long relationship, string propertyName, object propertyValue )
		 {
			  RecordAccess_RecordProxy<RelationshipRecord, Void> relationshipRecord = GetRelationshipRecord( relationship );
			  SetPrimitiveProperty( relationshipRecord, propertyName, propertyValue );

			  _flushStrategy.flush();
		 }

		 public override void RemoveNodeProperty( long node, string propertyName )
		 {
			  int propertyKey = GetOrCreatePropertyKeyId( propertyName );
			  _propertyDeletor.removePropertyIfExists( GetNodeRecord( node ), propertyKey, _recordAccess.PropertyRecords );
			  _flushStrategy.flush();
		 }

		 public override void RemoveRelationshipProperty( long relationship, string propertyName )
		 {
			  int propertyKey = GetOrCreatePropertyKeyId( propertyName );
			  _propertyDeletor.removePropertyIfExists( GetRelationshipRecord( relationship ), propertyKey, _recordAccess.PropertyRecords );
			  _flushStrategy.flush();
		 }

		 public override IndexCreator CreateDeferredSchemaIndex( Label label )
		 {
			  return new IndexCreatorImpl( _actions, label );
		 }

		 private void SetPrimitiveProperty<T1>( RecordAccess_RecordProxy<T1> primitiveRecord, string propertyName, object propertyValue ) where T1 : Org.Neo4j.Kernel.impl.store.record.PrimitiveRecord
		 {
			  int propertyKey = GetOrCreatePropertyKeyId( propertyName );
			  RecordAccess<PropertyRecord, PrimitiveRecord> propertyRecords = _recordAccess.PropertyRecords;

			  _propertyCreator.primitiveSetProperty( primitiveRecord, propertyKey, ValueUtils.asValue( propertyValue ), propertyRecords );
		 }

		 private void ValidateIndexCanBeCreated( int labelId, int[] propertyKeyIds )
		 {
			  VerifyIndexOrUniquenessConstraintCanBeCreated( labelId, propertyKeyIds, "Index for given {label;property} already exists" );
		 }

		 private void ValidateUniquenessConstraintCanBeCreated( int labelId, int[] propertyKeyIds )
		 {
			  VerifyIndexOrUniquenessConstraintCanBeCreated( labelId, propertyKeyIds, "It is not allowed to create node keys, uniqueness constraints or indexes on the same {label;property}" );
		 }

		 private void ValidateNodeKeyConstraintCanBeCreated( int labelId, int[] propertyKeyIds )
		 {
			  VerifyIndexOrUniquenessConstraintCanBeCreated( labelId, propertyKeyIds, "It is not allowed to create node keys, uniqueness constraints or indexes on the same {label;property}" );
		 }

		 private void VerifyIndexOrUniquenessConstraintCanBeCreated( int labelId, int[] propertyKeyIds, string errorMessage )
		 {
			  LabelSchemaDescriptor schemaDescriptor = SchemaDescriptorFactory.forLabel( labelId, propertyKeyIds );
			  ConstraintDescriptor constraintDescriptor = ConstraintDescriptorFactory.uniqueForSchema( schemaDescriptor );
			  ConstraintDescriptor nodeKeyDescriptor = ConstraintDescriptorFactory.nodeKeyForSchema( schemaDescriptor );
			  if ( _schemaCache.hasIndex( schemaDescriptor ) || _schemaCache.hasConstraintRule( constraintDescriptor ) || _schemaCache.hasConstraintRule( nodeKeyDescriptor ) )
			  {
					throw new ConstraintViolationException( errorMessage );
			  }
		 }

		 private void ValidateNodePropertyExistenceConstraintCanBeCreated( int labelId, int[] propertyKeyIds )
		 {
			  ConstraintDescriptor constraintDescriptor = ConstraintDescriptorFactory.existsForLabel( labelId, propertyKeyIds );

			  if ( _schemaCache.hasConstraintRule( constraintDescriptor ) )
			  {
					throw new ConstraintViolationException( "Node property existence constraint for given {label;property} already exists" );
			  }
		 }

		 private void ValidateRelationshipConstraintCanBeCreated( int relTypeId, int propertyKeyId )
		 {
			  ConstraintDescriptor constraintDescriptor = ConstraintDescriptorFactory.existsForLabel( relTypeId, propertyKeyId );

			  if ( _schemaCache.hasConstraintRule( constraintDescriptor ) )
			  {
					throw new ConstraintViolationException( "Relationship property existence constraint for given {type;property} already exists" );
			  }
		 }

		 private IndexReference CreateIndex( int labelId, int[] propertyKeyIds, Optional<string> indexName )
		 {
			  LabelSchemaDescriptor schema = SchemaDescriptorFactory.forLabel( labelId, propertyKeyIds );
			  IndexProvider provider = _indexProviderMap.DefaultProvider;
			  IndexProviderDescriptor providerDescriptor = provider.ProviderDescriptor;
			  IndexDescriptor index = IndexDescriptorFactory.forSchema( schema, indexName, providerDescriptor );
			  StoreIndexDescriptor schemaRule;
			  try
			  {
					schemaRule = provider.Bless( index ).withId( _schemaStore.nextId() );
			  }
			  catch ( MisconfiguredIndexException e )
			  {
					throw new ConstraintViolationException( "Unable to create index. The index configuration was refused by the '" + providerDescriptor + "' index provider.", e );
			  }

			  foreach ( DynamicRecord record in _schemaStore.allocateFrom( schemaRule ) )
			  {
					_schemaStore.updateRecord( record );
			  }
			  _schemaCache.addSchemaRule( schemaRule );
			  _labelsTouched = true;
			  _flushStrategy.forceFlush();
			  return schemaRule;
		 }

		 private void RepopulateAllIndexes( NativeLabelScanStore labelIndex )
		 {
			  LogProvider logProvider = _logService.InternalLogProvider;
			  LogProvider userLogProvider = _logService.UserLogProvider;
			  IndexStoreView indexStoreView = new DynamicIndexStoreView( _storeIndexStoreView, labelIndex, NO_LOCK_SERVICE, _neoStores, logProvider );
			  IndexingService indexingService = IndexingServiceFactory.createIndexingService( _config, _jobScheduler, _indexProviderMap, indexStoreView, new NonTransactionalTokenNameLookup( _tokenHolders ), emptyList(), logProvider, userLogProvider, NO_MONITOR, new DatabaseSchemaState(logProvider), false );
			  _life.add( indexingService );
			  try
			  {
					StoreIndexDescriptor[] descriptors = IndexesNeedingPopulation;
					indexingService.CreateIndexes( true, descriptors );
					foreach ( StoreIndexDescriptor descriptor in descriptors )
					{
						 IndexProxy indexProxy = GetIndexProxy( indexingService, descriptor );
						 try
						 {
							  indexProxy.AwaitStoreScanCompleted( 0, TimeUnit.MILLISECONDS );
						 }
						 catch ( IndexPopulationFailedKernelException )
						 {
							  // In this scenario this is OK
						 }
					}
					indexingService.ForceAll( Org.Neo4j.Io.pagecache.IOLimiter_Fields.Unlimited );
			  }
			  catch ( InterruptedException )
			  {
					// Someone wanted us to abort this. The indexes may not have been fully populated. This just means that they will be populated on next startup.
					Thread.CurrentThread.Interrupt();
			  }
		 }

		 private IndexProxy GetIndexProxy( IndexingService indexingService, StoreIndexDescriptor descriptpr )
		 {
			  try
			  {
					return indexingService.GetIndexProxy( descriptpr.Schema() );
			  }
			  catch ( IndexNotFoundKernelException e )
			  {
					throw new System.InvalidOperationException( "Expected index by descriptor " + descriptpr + " to exist, but didn't", e );
			  }
		 }

		 private void RebuildCounts()
		 {
			  CountsTracker counts = _neoStores.Counts;
			  try
			  {
					Counts.start();
			  }
			  catch ( IOException e )
			  {
					throw new UnderlyingStorageException( e );
			  }

			  CountsComputer.recomputeCounts( _neoStores, _pageCache, _databaseLayout );
		 }

		 private StoreIndexDescriptor[] IndexesNeedingPopulation
		 {
			 get
			 {
				  IList<StoreIndexDescriptor> indexesNeedingPopulation = new List<StoreIndexDescriptor>();
				  foreach ( StoreIndexDescriptor rule in _schemaCache.indexDescriptors() )
				  {
						IndexProvider provider = _indexProviderMap.lookup( rule.ProviderDescriptor() );
						if ( provider.GetInitialState( rule ) != InternalIndexState.FAILED )
						{
							 indexesNeedingPopulation.Add( rule );
						}
				  }
				  return indexesNeedingPopulation.ToArray();
			 }
		 }

		 public override ConstraintCreator CreateDeferredConstraint( Label label )
		 {
			  return new BaseNodeConstraintCreator( new BatchSchemaActions( this ), label );
		 }

		 private void CreateUniqueIndexAndOwningConstraint( LabelSchemaDescriptor schema, IndexBackedConstraintDescriptor constraintDescriptor )
		 {
			  // TODO: Do not create duplicate index

			  long indexId = _schemaStore.nextId();
			  long constraintRuleId = _schemaStore.nextId();

			  IndexProvider provider = _indexProviderMap.DefaultProvider;
			  IndexProviderDescriptor providerDescriptor = provider.ProviderDescriptor;
			  IndexDescriptor index = IndexDescriptorFactory.uniqueForSchema( schema, providerDescriptor );
			  StoreIndexDescriptor storeIndexDescriptor;
			  try
			  {
					storeIndexDescriptor = provider.Bless( index ).withIds( indexId, constraintRuleId );
			  }
			  catch ( MisconfiguredIndexException e )
			  {
					throw new ConstraintViolationException( "Unable to create index. The index configuration was refused by the '" + providerDescriptor + "' index provider.", e );
			  }

			  ConstraintRule constraintRule = ConstraintRule.constraintRule( constraintRuleId, constraintDescriptor, indexId );

			  foreach ( DynamicRecord record in _schemaStore.allocateFrom( constraintRule ) )
			  {
					_schemaStore.updateRecord( record );
			  }
			  _schemaCache.addSchemaRule( constraintRule );
			  foreach ( DynamicRecord record in _schemaStore.allocateFrom( storeIndexDescriptor ) )
			  {
					_schemaStore.updateRecord( record );
			  }
			  _schemaCache.addSchemaRule( storeIndexDescriptor );
			  _labelsTouched = true;
			  _flushStrategy.forceFlush();
		 }

		 private void CreateUniquenessConstraintRule( LabelSchemaDescriptor descriptor )
		 {
			  CreateUniqueIndexAndOwningConstraint( descriptor, ConstraintDescriptorFactory.uniqueForSchema( descriptor ) );
		 }

		 private void CreateNodeKeyConstraintRule( LabelSchemaDescriptor descriptor )
		 {
			  CreateUniqueIndexAndOwningConstraint( descriptor, ConstraintDescriptorFactory.nodeKeyForSchema( descriptor ) );
		 }

		 private void CreateNodePropertyExistenceConstraintRule( int labelId, params int[] propertyKeyIds )
		 {
			  SchemaRule rule = ConstraintRule.constraintRule( _schemaStore.nextId(), ConstraintDescriptorFactory.existsForLabel(labelId, propertyKeyIds) );

			  foreach ( DynamicRecord record in _schemaStore.allocateFrom( rule ) )
			  {
					_schemaStore.updateRecord( record );
			  }
			  _schemaCache.addSchemaRule( rule );
			  _labelsTouched = true;
			  _flushStrategy.forceFlush();
		 }

		 private void CreateRelTypePropertyExistenceConstraintRule( int relTypeId, params int[] propertyKeyIds )
		 {
			  SchemaRule rule = ConstraintRule.constraintRule( _schemaStore.nextId(), ConstraintDescriptorFactory.existsForRelType(relTypeId, propertyKeyIds) );

			  foreach ( DynamicRecord record in _schemaStore.allocateFrom( rule ) )
			  {
					_schemaStore.updateRecord( record );
			  }
			  _schemaCache.addSchemaRule( rule );
			  _flushStrategy.forceFlush();
		 }

		 private int GetOrCreatePropertyKeyId( string name )
		 {
			  return _tokenHolders.propertyKeyTokens().getOrCreateId(name);
		 }

		 private int GetOrCreateRelationshipTypeId( string name )
		 {
			  return _tokenHolders.relationshipTypeTokens().getOrCreateId(name);
		 }

		 private int GetOrCreateLabelId( string name )
		 {
			  return _tokenHolders.labelTokens().getOrCreateId(name);
		 }

		 private bool PrimitiveHasProperty( PrimitiveRecord record, string propertyName )
		 {
			  int propertyKeyId = _tokenHolders.propertyKeyTokens().getIdByName(propertyName);
			  return propertyKeyId != NO_TOKEN && _propertyTraverser.findPropertyRecordContaining( record, propertyKeyId, _recordAccess.PropertyRecords, false ) != Record.NO_NEXT_PROPERTY.intValue();
		 }

		 private static void RejectAutoUpgrade( IDictionary<string, string> @params )
		 {
			  if ( parseBoolean( @params[GraphDatabaseSettings.allow_upgrade.name()] ) )
			  {
					throw new System.ArgumentException( "Batch inserter is not allowed to do upgrade of a store." );
			  }
		 }

		 public override long CreateNode( IDictionary<string, object> properties, params Label[] labels )
		 {
			  return InternalCreateNode( _nodeStore.nextId(), properties, labels );
		 }

		 private long InternalCreateNode( long nodeId, IDictionary<string, object> properties, params Label[] labels )
		 {
			  NodeRecord nodeRecord = _recordAccess.NodeRecords.create( nodeId, null ).forChangingData();
			  nodeRecord.InUse = true;
			  nodeRecord.SetCreated();
			  nodeRecord.NextProp = _propertyCreator.createPropertyChain( nodeRecord, PropertiesIterator( properties ), _recordAccess.PropertyRecords );

			  if ( labels.Length > 0 )
			  {
					SetNodeLabels( nodeRecord, labels );
			  }

			  _flushStrategy.flush();
			  return nodeId;
		 }

		 private IEnumerator<PropertyBlock> PropertiesIterator( IDictionary<string, object> properties )
		 {
			  if ( properties == null || properties.Count == 0 )
			  {
					return emptyIterator();
			  }
			  return new IteratorWrapperAnonymousInnerClass( this );
		 }

		 private class IteratorWrapperAnonymousInnerClass : IteratorWrapper<PropertyBlock, KeyValuePair<string, object>>
		 {
			 private readonly BatchInserterImpl _outerInstance;

			 public IteratorWrapperAnonymousInnerClass( BatchInserterImpl outerInstance ) : base( properties.entrySet().GetEnumerator() )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override PropertyBlock underlyingObjectToObject( KeyValuePair<string, object> property )
			 {
				  return _outerInstance.propertyCreator.encodePropertyValue( _outerInstance.getOrCreatePropertyKeyId( property.Key ), ValueUtils.asValue( property.Value ) );
			 }
		 }

		 private void SetNodeLabels( NodeRecord nodeRecord, params Label[] labels )
		 {
			  NodeLabels nodeLabels = parseLabelsField( nodeRecord );
			  nodeLabels.Put( GetOrCreateLabelIds( labels ), _nodeStore, _nodeStore.DynamicLabelStore );
			  _labelsTouched = true;
		 }

		 private long[] GetOrCreateLabelIds( Label[] labels )
		 {
			  long[] ids = new long[labels.Length];
			  int cursor = 0;
			  for ( int i = 0; i < ids.Length; i++ )
			  {
					int labelId = GetOrCreateLabelId( labels[i].Name() );
					if ( !ArrayContains( ids, cursor, labelId ) )
					{
						 ids[cursor++] = labelId;
					}
			  }
			  if ( cursor < ids.Length )
			  {
					ids = Arrays.copyOf( ids, cursor );
			  }
			  return ids;
		 }

		 private static bool ArrayContains( long[] ids, int cursor, int labelId )
		 {
			  for ( int i = 0; i < cursor; i++ )
			  {
					if ( ids[i] == labelId )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 public override void CreateNode( long id, IDictionary<string, object> properties, params Label[] labels )
		 {
			  IdValidator.assertValidId( IdType.NODE, id, _maxNodeId );
			  if ( _nodeStore.isInUse( id ) )
			  {
					throw new System.ArgumentException( "id=" + id + " already in use" );
			  }
			  long highId = _nodeStore.HighId;
			  if ( highId <= id )
			  {
					_nodeStore.HighestPossibleIdInUse = id;
			  }
			  InternalCreateNode( id, properties, labels );
		 }

		 public override void SetNodeLabels( long node, params Label[] labels )
		 {
			  NodeRecord record = GetNodeRecord( node ).forChangingData();
			  SetNodeLabels( record, labels );
			  _flushStrategy.flush();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Iterable<org.neo4j.graphdb.Label> getNodeLabels(final long node)
		 public override IEnumerable<Label> GetNodeLabels( long node )
		 {
			  return () =>
			  {
				NodeRecord record = GetNodeRecord( node ).forReadingData();
				long[] labels = parseLabelsField( record ).get( _nodeStore );
				return map( labelIdToLabelFunction, PrimitiveLongCollections.iterator( labels ) );
			  };
		 }

		 public override bool NodeHasLabel( long node, Label label )
		 {
			  int labelId = _tokenHolders.labelTokens().getIdByName(label.Name());
			  return labelId != NO_TOKEN && NodeHasLabel( node, labelId );
		 }

		 private bool NodeHasLabel( long node, int labelId )
		 {
			  NodeRecord record = GetNodeRecord( node ).forReadingData();
			  foreach ( long label in parseLabelsField( record ).get( _nodeStore ) )
			  {
					if ( label == labelId )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 public override long CreateRelationship( long node1, long node2, RelationshipType type, IDictionary<string, object> properties )
		 {
			  long id = _relationshipStore.nextId();
			  int typeId = GetOrCreateRelationshipTypeId( type.Name() );
			  _relationshipCreator.relationshipCreate( id, typeId, node1, node2, _recordAccess, _noopLockClient );
			  if ( properties != null && properties.Count > 0 )
			  {
					RelationshipRecord record = _recordAccess.RelRecords.getOrLoad( id, null ).forChangingData();
					record.NextProp = _propertyCreator.createPropertyChain( record, PropertiesIterator( properties ), _recordAccess.PropertyRecords );
			  }
			  _flushStrategy.flush();
			  return id;
		 }

		 public override void SetNodeProperties( long node, IDictionary<string, object> properties )
		 {
			  NodeRecord record = GetNodeRecord( node ).forChangingData();
			  if ( record.NextProp != Record.NO_NEXT_PROPERTY.intValue() )
			  {
					_propertyDeletor.deletePropertyChain( record, _recordAccess.PropertyRecords );
			  }
			  record.NextProp = _propertyCreator.createPropertyChain( record, PropertiesIterator( properties ), _recordAccess.PropertyRecords );
			  _flushStrategy.flush();
		 }

		 public override void SetRelationshipProperties( long rel, IDictionary<string, object> properties )
		 {
			  RelationshipRecord record = _recordAccess.RelRecords.getOrLoad( rel, null ).forChangingData();
			  if ( record.NextProp != Record.NO_NEXT_PROPERTY.intValue() )
			  {
					_propertyDeletor.deletePropertyChain( record, _recordAccess.PropertyRecords );
			  }
			  record.NextProp = _propertyCreator.createPropertyChain( record, PropertiesIterator( properties ), _recordAccess.PropertyRecords );
			  _flushStrategy.flush();
		 }

		 public override bool NodeExists( long nodeId )
		 {
			  _flushStrategy.forceFlush();
			  return _nodeStore.isInUse( nodeId );
		 }

		 public override IDictionary<string, object> GetNodeProperties( long nodeId )
		 {
			  NodeRecord record = GetNodeRecord( nodeId ).forReadingData();
			  if ( record.NextProp != Record.NO_NEXT_PROPERTY.intValue() )
			  {
					return GetPropertyChain( record.NextProp );
			  }
			  return Collections.emptyMap();
		 }

		 public override IEnumerable<long> GetRelationshipIds( long nodeId )
		 {
			  _flushStrategy.forceFlush();
			  return new BatchRelationshipIterableAnonymousInnerClass( this, _storageReader, nodeId );
		 }

		 private class BatchRelationshipIterableAnonymousInnerClass : BatchRelationshipIterable<long>
		 {
			 private readonly BatchInserterImpl _outerInstance;

			 public BatchRelationshipIterableAnonymousInnerClass( BatchInserterImpl outerInstance, RecordStorageReader storageReader, long nodeId ) : base( storageReader, nodeId )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override long? nextFrom( long relId, int type, long startNode, long endNode )
			 {
				  return relId;
			 }
		 }

		 public override IEnumerable<BatchRelationship> GetRelationships( long nodeId )
		 {
			  _flushStrategy.forceFlush();
			  return new BatchRelationshipIterableAnonymousInnerClass2( this, _storageReader, nodeId );
		 }

		 private class BatchRelationshipIterableAnonymousInnerClass2 : BatchRelationshipIterable<BatchRelationship>
		 {
			 private readonly BatchInserterImpl _outerInstance;

			 public BatchRelationshipIterableAnonymousInnerClass2( BatchInserterImpl outerInstance, RecordStorageReader storageReader, long nodeId ) : base( storageReader, nodeId )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override BatchRelationship nextFrom( long relId, int type, long startNode, long endNode )
			 {
				  return outerInstance.batchRelationshipOf( relId, type, startNode, endNode );
			 }
		 }

		 private BatchRelationship BatchRelationshipOf( long id, int type, long startNode, long endNode )
		 {
			  try
			  {
					return new BatchRelationship( id, startNode, endNode, RelationshipType.withName( _tokenHolders.relationshipTypeTokens().getTokenById(type).name() ) );
			  }
			  catch ( TokenNotFoundException e )
			  {
					throw new Exception( e );
			  }
		 }

		 public override BatchRelationship GetRelationshipById( long relId )
		 {
			  RelationshipRecord record = GetRelationshipRecord( relId ).forReadingData();
			  return BatchRelationshipOf( relId, record.Type, record.FirstNode, record.SecondNode );
		 }

		 public override IDictionary<string, object> GetRelationshipProperties( long relId )
		 {
			  RelationshipRecord record = _recordAccess.RelRecords.getOrLoad( relId, null ).forChangingData();
			  if ( record.NextProp != Record.NO_NEXT_PROPERTY.intValue() )
			  {
					return GetPropertyChain( record.NextProp );
			  }
			  return Collections.emptyMap();
		 }

		 public override void Shutdown()
		 {
			  if ( _isShutdown )
			  {
					throw new System.InvalidOperationException( "Batch inserter already has shutdown" );
			  }
			  _isShutdown = true;

			  _flushStrategy.forceFlush();

			  RebuildCounts();

			  try
			  {
					NativeLabelScanStore labelIndex = BuildLabelIndex();
					RepopulateAllIndexes( labelIndex );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
			  finally
			  {
					_neoStores.close();

					try
					{
						 _storeLocker.Dispose();
					}
					catch ( IOException e )
					{
						 throw new UnderlyingStorageException( "Could not release store lock", e );
					}

					_msgLog.info( Thread.CurrentThread + " Clean shutdown on BatchInserter(" + this + ")" );
					_life.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.impl.index.labelscan.NativeLabelScanStore buildLabelIndex() throws java.io.IOException
		 private NativeLabelScanStore BuildLabelIndex()
		 {
			  NativeLabelScanStore labelIndex = new NativeLabelScanStore( _pageCache, _databaseLayout, _fileSystem, new FullLabelStream( _storeIndexStoreView ), false, _monitors, RecoveryCleanupWorkCollector.immediate() );
			  if ( _labelsTouched )
			  {
					labelIndex.Drop();
			  }
			  // Rebuild will happen as part of this call if it was dropped
			  _life.add( labelIndex );
			  return labelIndex;
		 }

		 public override string ToString()
		 {
			  return "EmbeddedBatchInserter[" + _databaseLayout + "]";
		 }

		 private IDictionary<string, object> GetPropertyChain( long nextProp )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<String, Object> map = new java.util.HashMap<>();
			  IDictionary<string, object> map = new Dictionary<string, object>();
			  _propertyTraverser.getPropertyChain(nextProp, _recordAccess.PropertyRecords, propBlock =>
			  {
				try
				{
					 string key = _tokenHolders.propertyKeyTokens().getTokenById(propBlock.KeyIndexId).name();
					 Value propertyValue = propBlock.newPropertyValue( _propertyStore );
					 map[key] = propertyValue.asObject();
				}
				catch ( TokenNotFoundException e )
				{
					 throw new Exception( e );
				}
			  });
			  return map;
		 }

		 private int CreateNewPropertyKeyId( string stringKey )
		 {
			  int keyId = ( int ) _propertyKeyTokenStore.nextId();
			  PropertyKeyTokenRecord record = new PropertyKeyTokenRecord( keyId );
			  record.InUse = true;
			  record.SetCreated();
			  ICollection<DynamicRecord> keyRecords = _propertyKeyTokenStore.allocateNameRecords( encodeString( stringKey ) );
			  record.NameId = ( int ) Iterables.first( keyRecords ).Id;
			  record.AddNameRecords( keyRecords );
			  _propertyKeyTokenStore.updateRecord( record );
			  _tokenHolders.propertyKeyTokens().addToken(new NamedToken(stringKey, keyId));
			  return keyId;
		 }

		 private int CreateNewLabelId( string stringKey )
		 {
			  int keyId = ( int ) _labelTokenStore.nextId();
			  LabelTokenRecord record = new LabelTokenRecord( keyId );
			  record.InUse = true;
			  record.SetCreated();
			  ICollection<DynamicRecord> keyRecords = _labelTokenStore.allocateNameRecords( encodeString( stringKey ) );
			  record.NameId = ( int ) Iterables.first( keyRecords ).Id;
			  record.AddNameRecords( keyRecords );
			  _labelTokenStore.updateRecord( record );
			  _tokenHolders.labelTokens().addToken(new NamedToken(stringKey, keyId));
			  return keyId;
		 }

		 private int CreateNewRelationshipType( string name )
		 {
			  int id = ( int ) _relationshipTypeTokenStore.nextId();
			  RelationshipTypeTokenRecord record = new RelationshipTypeTokenRecord( id );
			  record.InUse = true;
			  record.SetCreated();
			  ICollection<DynamicRecord> nameRecords = _relationshipTypeTokenStore.allocateNameRecords( encodeString( name ) );
			  record.NameId = ( int ) Iterables.first( nameRecords ).Id;
			  record.AddNameRecords( nameRecords );
			  _relationshipTypeTokenStore.updateRecord( record );
			  _tokenHolders.relationshipTypeTokens().addToken(new NamedToken(name, id));
			  return id;
		 }

		 private RecordAccess_RecordProxy<NodeRecord, Void> GetNodeRecord( long id )
		 {
			  if ( id < 0 || id >= _nodeStore.HighId )
			  {
					throw new NotFoundException( "id=" + id );
			  }
			  return _recordAccess.NodeRecords.getOrLoad( id, null );
		 }

		 private RecordAccess_RecordProxy<RelationshipRecord, Void> GetRelationshipRecord( long id )
		 {
			  if ( id < 0 || id >= _relationshipStore.HighId )
			  {
					throw new NotFoundException( "id=" + id );
			  }
			  return _recordAccess.RelRecords.getOrLoad( id, null );
		 }

		 public virtual string StoreDir
		 {
			 get
			 {
				  return _databaseLayout.databaseDirectory().Path;
			 }
		 }

		 public virtual IndexConfigStore IndexStore
		 {
			 get
			 {
				  return this._indexStore;
			 }
		 }

		 private static void DumpConfiguration( IDictionary<string, string> config, PrintStream @out )
		 {
			  foreach ( KeyValuePair<string, string> entry in config.SetOfKeyValuePairs() )
			  {
					if ( entry.Value != null )
					{
						 @out.println( entry.Key + "=" + entry.Value );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @VisibleForTesting NeoStores getNeoStores()
		 internal virtual NeoStores NeoStores
		 {
			 get
			 {
				  return _neoStores;
			 }
		 }

		 internal virtual void ForceFlushChanges()
		 {
			  _flushStrategy.forceFlush();
		 }

		 private static ConstraintSemantics LoadConstraintSemantics()
		 {
			  IEnumerable<ConstraintSemantics> semantics = Service.load( typeof( ConstraintSemantics ) );
			  IList<ConstraintSemantics> candidates = Iterables.asList( semantics );
			  checkState( candidates.Count > 0, format( "At least one implementation of %s should be available.", typeof( ConstraintSemantics ) ) );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return Collections.max( candidates, System.Collections.IComparer.comparingInt( ConstraintSemantics::getPriority ) );
		 }

		 private class BatchSchemaActions : InternalSchemaActions
		 {
			 private readonly BatchInserterImpl _outerInstance;

			 public BatchSchemaActions( BatchInserterImpl outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal virtual int[] GetOrCreatePropertyKeyIds( IEnumerable<string> properties )
			  {
					return Iterables.stream( properties ).mapToInt( _outerInstance.getOrCreatePropertyKeyId ).toArray();
			  }

			  internal virtual int[] GetOrCreatePropertyKeyIds( string[] properties )
			  {
					return java.util.properties.Select( _outerInstance.getOrCreatePropertyKeyId ).ToArray();
			  }

			  public override IndexDefinition CreateIndexDefinition( Label label, Optional<string> indexName, params string[] propertyKeys )
			  {
					int labelId = outerInstance.getOrCreateLabelId( label.Name() );
					int[] propertyKeyIds = GetOrCreatePropertyKeyIds( propertyKeys );

					outerInstance.validateIndexCanBeCreated( labelId, propertyKeyIds );

					IndexReference indexReference = outerInstance.createIndex( labelId, propertyKeyIds, indexName );
					return new IndexDefinitionImpl( this, indexReference, new Label[]{ label }, propertyKeys, false );
			  }

			  public override void DropIndexDefinitions( IndexDefinition indexDefinition )
			  {
					throw UnsupportedException();
			  }

			  public override ConstraintDefinition CreatePropertyUniquenessConstraint( IndexDefinition indexDefinition )
			  {
					int labelId = outerInstance.getOrCreateLabelId( indexDefinition.Label.name() );
					int[] propertyKeyIds = GetOrCreatePropertyKeyIds( indexDefinition.PropertyKeys );
					LabelSchemaDescriptor descriptor = SchemaDescriptorFactory.forLabel( labelId, propertyKeyIds );

					outerInstance.validateUniquenessConstraintCanBeCreated( labelId, propertyKeyIds );
					outerInstance.createUniquenessConstraintRule( descriptor );
					return new UniquenessConstraintDefinition( this, indexDefinition );
			  }

			  public override ConstraintDefinition CreateNodeKeyConstraint( IndexDefinition indexDefinition )
			  {
					int labelId = outerInstance.getOrCreateLabelId( indexDefinition.Label.name() );
					int[] propertyKeyIds = GetOrCreatePropertyKeyIds( indexDefinition.PropertyKeys );
					LabelSchemaDescriptor descriptor = SchemaDescriptorFactory.forLabel( labelId, propertyKeyIds );

					outerInstance.validateNodeKeyConstraintCanBeCreated( labelId, propertyKeyIds );
					outerInstance.createNodeKeyConstraintRule( descriptor );
					return new NodeKeyConstraintDefinition( this, indexDefinition );
			  }

			  public override ConstraintDefinition CreatePropertyExistenceConstraint( Label label, params string[] propertyKeys )
			  {
					int labelId = outerInstance.getOrCreateLabelId( label.Name() );
					int[] propertyKeyIds = GetOrCreatePropertyKeyIds( propertyKeys );

					outerInstance.validateNodePropertyExistenceConstraintCanBeCreated( labelId, propertyKeyIds );

					outerInstance.createNodePropertyExistenceConstraintRule( labelId, propertyKeyIds );
					return new NodePropertyExistenceConstraintDefinition( this, label, propertyKeys );
			  }

			  public override ConstraintDefinition CreatePropertyExistenceConstraint( RelationshipType type, string propertyKey )
			  {
					int relationshipTypeId = outerInstance.getOrCreateRelationshipTypeId( type.Name() );
					int propertyKeyId = outerInstance.getOrCreatePropertyKeyId( propertyKey );

					outerInstance.validateRelationshipConstraintCanBeCreated( relationshipTypeId, propertyKeyId );

					outerInstance.createRelTypePropertyExistenceConstraintRule( relationshipTypeId, propertyKeyId );
					return new RelationshipPropertyExistenceConstraintDefinition( this, type, propertyKey );
			  }

			  public override void DropPropertyUniquenessConstraint( Label label, string[] properties )
			  {
					throw UnsupportedException();
			  }

			  public override void DropNodeKeyConstraint( Label label, string[] properties )
			  {
					throw UnsupportedException();
			  }

			  public override void DropNodePropertyExistenceConstraint( Label label, string[] properties )
			  {
					throw UnsupportedException();
			  }

			  public override void DropRelationshipPropertyExistenceConstraint( RelationshipType type, string propertyKey )
			  {
					throw UnsupportedException();
			  }

			  public override string GetUserMessage( KernelException e )
			  {
					throw UnsupportedException();
			  }

			  public override void AssertInOpenTransaction()
			  {
					// BatchInserterImpl always is expected to be running in one big single "transaction"
			  }

			  internal virtual System.NotSupportedException UnsupportedException()
			  {
					return new System.NotSupportedException( "Batch inserter doesn't support this" );
			  }
		 }

		 internal interface FlushStrategy
		 {
			  void Flush();

			  void ForceFlush();
		 }

		 internal sealed class BatchedFlushStrategy : FlushStrategy
		 {
			  internal readonly DirectRecordAccessSet DirectRecordAccess;
			  internal readonly int BatchSize;
			  internal int Attempts;

			  internal BatchedFlushStrategy( DirectRecordAccessSet directRecordAccess, int batchSize )
			  {
					this.DirectRecordAccess = directRecordAccess;
					this.BatchSize = batchSize;
			  }

			  public override void Flush()
			  {
					Attempts++;
					if ( Attempts >= BatchSize )
					{
						 ForceFlush();
					}
			  }

			  public override void ForceFlush()
			  {
					DirectRecordAccess.commit();
					Attempts = 0;
			  }
		 }
	}

}