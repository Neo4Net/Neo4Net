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
namespace Neo4Net.@unsafe.Impl.Batchimport.store
{

	using RecoveryCleanupWorkCollector = Neo4Net.Index.Internal.gbptree.RecoveryCleanupWorkCollector;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseFile = Neo4Net.Io.layout.DatabaseFile;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PagedFile = Neo4Net.Io.pagecache.PagedFile;
	using DefaultPageCacheTracer = Neo4Net.Io.pagecache.tracing.DefaultPageCacheTracer;
	using PageCacheTracer = Neo4Net.Io.pagecache.tracing.PageCacheTracer;
	using DefaultPageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.DefaultPageCursorTracerSupplier;
	using PageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using LabelScanStore = Neo4Net.Kernel.api.labelscan.LabelScanStore;
	using Config = Neo4Net.Kernel.configuration.Config;
	using FullStoreChangeStream = Neo4Net.Kernel.Impl.Api.scan.FullStoreChangeStream;
	using NativeLabelScanStore = Neo4Net.Kernel.impl.index.labelscan.NativeLabelScanStore;
	using ConfiguringPageCacheFactory = Neo4Net.Kernel.impl.pagecache.ConfiguringPageCacheFactory;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using PropertyStore = Neo4Net.Kernel.impl.store.PropertyStore;
	using Neo4Net.Kernel.impl.store;
	using RelationshipGroupStore = Neo4Net.Kernel.impl.store.RelationshipGroupStore;
	using RelationshipStore = Neo4Net.Kernel.impl.store.RelationshipStore;
	using StoreFactory = Neo4Net.Kernel.impl.store.StoreFactory;
	using StoreType = Neo4Net.Kernel.impl.store.StoreType;
	using CountsTracker = Neo4Net.Kernel.impl.store.counts.CountsTracker;
	using Capability = Neo4Net.Kernel.impl.store.format.Capability;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using DefaultIdGeneratorFactory = Neo4Net.Kernel.impl.store.id.DefaultIdGeneratorFactory;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using LifeSupport = Neo4Net.Kernel.Lifecycle.LifeSupport;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using LogService = Neo4Net.Logging.Internal.LogService;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using MemoryStatsVisitor = Neo4Net.@unsafe.Impl.Batchimport.cache.MemoryStatsVisitor;
	using Input_Estimates = Neo4Net.@unsafe.Impl.Batchimport.input.Input_Estimates;
	using BatchingLabelTokenRepository = Neo4Net.@unsafe.Impl.Batchimport.store.BatchingTokenRepository.BatchingLabelTokenRepository;
	using BatchingPropertyKeyTokenRepository = Neo4Net.@unsafe.Impl.Batchimport.store.BatchingTokenRepository.BatchingPropertyKeyTokenRepository;
	using BatchingRelationshipTypeTokenRepository = Neo4Net.@unsafe.Impl.Batchimport.store.BatchingTokenRepository.BatchingRelationshipTypeTokenRepository;
	using IoTracer = Neo4Net.@unsafe.Impl.Batchimport.store.io.IoTracer;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.dense_node_threshold;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.pagecache_memory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.IOUtils.closeAll;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.IOLimiter_Fields.UNLIMITED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.labelscan.NativeLabelScanStore.getLabelScanStoreFile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.StoreType.PROPERTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.StoreType.PROPERTY_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.StoreType.PROPERTY_STRING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.StoreType.RELATIONSHIP_GROUP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_COMMIT_TIMESTAMP;

	/// <summary>
	/// Creator and accessor of <seealso cref="NeoStores"/> with some logic to provide very batch friendly services to the
	/// <seealso cref="NeoStores"/> when instantiating it. Different services for specific purposes.
	/// </summary>
	public class BatchingNeoStores : AutoCloseable, Neo4Net.@unsafe.Impl.Batchimport.cache.MemoryStatsVisitor_Visitable
	{
		 private const string TEMP_STORE_NAME = "temp.db";
		 // Empirical and slightly defensive threshold where relationship records seem to start requiring double record units.
		 // Basically decided by picking a maxId of pointer (as well as node ids) in the relationship record and randomizing its data,
		 // seeing which is a maxId where records starts to require a secondary unit.
		 internal static readonly long DoubleRelationshipRecordUnitThreshold = 1L << 33;
		 private static readonly StoreType[] _tempStoreTypes = new StoreType[] { RELATIONSHIP_GROUP, PROPERTY, PROPERTY_ARRAY, PROPERTY_STRING };

		 private readonly FileSystemAbstraction _fileSystem;
		 private readonly LogProvider _logProvider;
		 private readonly DatabaseLayout _databaseLayout;
		 private readonly DatabaseLayout _temporaryDatabaseLayout;
		 private readonly Config _neo4jConfig;
		 private readonly Configuration _importConfiguration;
		 private readonly PageCache _pageCache;
		 private readonly IoTracer _ioTracer;
		 private readonly RecordFormats _recordFormats;
		 private readonly AdditionalInitialIds _initialIds;
		 private readonly bool _externalPageCache;
		 private readonly IdGeneratorFactory _idGeneratorFactory;

		 // Some stores are considered temporary during the import and will be reordered/restructured
		 // into the main store. These temporary stores will live here
		 private NeoStores _neoStores;
		 private NeoStores _temporaryNeoStores;
		 private BatchingPropertyKeyTokenRepository _propertyKeyRepository;
		 private BatchingLabelTokenRepository _labelRepository;
		 private BatchingRelationshipTypeTokenRepository _relationshipTypeRepository;
		 private LifeSupport _life = new LifeSupport();
		 private LabelScanStore _labelScanStore;
		 private PageCacheFlusher _flusher;
		 private bool _doubleRelationshipRecordUnits;

		 private bool _successful;

		 private BatchingNeoStores( FileSystemAbstraction fileSystem, PageCache pageCache, File databaseDirectory, RecordFormats recordFormats, Config neo4jConfig, Configuration importConfiguration, LogService logService, AdditionalInitialIds initialIds, bool externalPageCache, IoTracer ioTracer )
		 {
			  this._fileSystem = fileSystem;
			  this._recordFormats = recordFormats;
			  this._importConfiguration = importConfiguration;
			  this._initialIds = initialIds;
			  this._logProvider = logService.InternalLogProvider;
			  this._databaseLayout = DatabaseLayout.of( databaseDirectory );
			  this._temporaryDatabaseLayout = DatabaseLayout.of( _databaseLayout.file( TEMP_STORE_NAME ), TEMP_STORE_NAME );
			  this._neo4jConfig = neo4jConfig;
			  this._pageCache = pageCache;
			  this._ioTracer = ioTracer;
			  this._externalPageCache = externalPageCache;
			  this._idGeneratorFactory = new DefaultIdGeneratorFactory( fileSystem );
		 }

		 private bool DatabaseExistsAndContainsData()
		 {
			  File metaDataFile = _databaseLayout.metadataStore();
			  try
			  {
					  using ( PagedFile pagedFile = _pageCache.map( metaDataFile, _pageCache.pageSize(), StandardOpenOption.READ ) )
					  {
						// OK so the db probably exists
					  }
			  }
			  catch ( IOException )
			  {
					// It's OK
					return false;
			  }

			  using ( NeoStores stores = NewStoreFactory( _databaseLayout ).openNeoStores( StoreType.NODE, StoreType.RELATIONSHIP ) )
			  {
					return stores.NodeStore.HighId > 0 || stores.RelationshipStore.HighId > 0;
			  }
		 }

		 /// <summary>
		 /// Called when expecting a clean {@code storeDir} folder and where a new store will be created.
		 /// This happens on an initial attempt to import.
		 /// </summary>
		 /// <exception cref="IOException"> on I/O error. </exception>
		 /// <exception cref="IllegalStateException"> if {@code storeDir} already contains a database. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void createNew() throws java.io.IOException
		 public virtual void CreateNew()
		 {
			  AssertDatabaseIsEmptyOrNonExistent();

			  // There may have been a previous import which was killed before it even started, where the label scan store could
			  // be in a semi-initialized state. Better to be on the safe side and deleted it. We get her after determining that
			  // the db is either completely empty or non-existent anyway, so deleting this file is OK.
			  _fileSystem.deleteFile( getLabelScanStoreFile( _databaseLayout ) );

			  InstantiateStores();
			  _neoStores.MetaDataStore.setLastCommittedAndClosedTransactionId( _initialIds.lastCommittedTransactionId(), _initialIds.lastCommittedTransactionChecksum(), BASE_TX_COMMIT_TIMESTAMP, _initialIds.lastCommittedTransactionLogByteOffset(), _initialIds.lastCommittedTransactionLogVersion() );
			  _neoStores.startCountStore();
		 }

		 public virtual void AssertDatabaseIsEmptyOrNonExistent()
		 {
			  if ( DatabaseExistsAndContainsData() )
			  {
					throw new System.InvalidOperationException( _databaseLayout.databaseDirectory() + " already contains data, cannot do import here" );
			  }
		 }

		 /// <summary>
		 /// Called when expecting a previous attempt/state of a database to open, where some store files should be kept,
		 /// but others deleted. All temporary stores will be deleted in this call.
		 /// </summary>
		 /// <param name="mainStoresToKeep"> <seealso cref="Predicate"/> controlling which files to keep, i.e. {@code true} means keep, {@code false} means delete. </param>
		 /// <param name="tempStoresToKeep"> <seealso cref="Predicate"/> controlling which files to keep, i.e. {@code true} means keep, {@code false} means delete. </param>
		 /// <exception cref="IOException"> on I/O error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void pruneAndOpenExistingStore(System.Predicate<org.neo4j.kernel.impl.store.StoreType> mainStoresToKeep, System.Predicate<org.neo4j.kernel.impl.store.StoreType> tempStoresToKeep) throws java.io.IOException
		 public virtual void PruneAndOpenExistingStore( System.Predicate<StoreType> mainStoresToKeep, System.Predicate<StoreType> tempStoresToKeep )
		 {
			  DeleteStoreFiles( _temporaryDatabaseLayout, tempStoresToKeep );
			  DeleteStoreFiles( _databaseLayout, mainStoresToKeep );
			  InstantiateStores();
			  _neoStores.startCountStore();
		 }

		 private void DeleteStoreFiles( DatabaseLayout databaseLayout, System.Predicate<StoreType> storesToKeep )
		 {
			  foreach ( StoreType type in StoreType.values() )
			  {
					if ( type.RecordStore && !storesToKeep( type ) )
					{
						 DatabaseFile databaseFile = type.DatabaseFile;
						 databaseLayout.File( databaseFile ).forEach( _fileSystem.deleteFile );
						 databaseLayout.IdFile( databaseFile ).ifPresent( _fileSystem.deleteFile );
					}
			  }
		 }

		 private void InstantiateKernelExtensions()
		 {
			  _life = new LifeSupport();
			  _life.start();
			  _labelScanStore = new NativeLabelScanStore( _pageCache, _databaseLayout, _fileSystem, Neo4Net.Kernel.Impl.Api.scan.FullStoreChangeStream_Fields.Empty, false, new Monitors(), RecoveryCleanupWorkCollector.immediate() );
			  _life.add( _labelScanStore );
		 }

		 private void InstantiateStores()
		 {
			  _neoStores = NewStoreFactory( _databaseLayout ).openAllNeoStores( true );
			  _propertyKeyRepository = new BatchingPropertyKeyTokenRepository( _neoStores.PropertyKeyTokenStore );
			  _labelRepository = new BatchingLabelTokenRepository( _neoStores.LabelTokenStore );
			  _relationshipTypeRepository = new BatchingRelationshipTypeTokenRepository( _neoStores.RelationshipTypeTokenStore );
			  _temporaryNeoStores = InstantiateTempStores();
			  InstantiateKernelExtensions();

			  // Delete the id generators because makeStoreOk isn't atomic in the sense that there's a possibility of an unlucky timing such
			  // that if the process is killed at the right time some store may end up with a .id file that looks to be CLEAN and has highId=0,
			  // i.e. effectively making the store look empty on the next start. Normal recovery of a db is sort of protected by this recovery
			  // recognizing that the db needs recovery when it looks at the tx log and also calling deleteIdGenerators. In the import case
			  // there are no tx logs at all, and therefore we do this manually right here.
			  _neoStores.deleteIdGenerators();
			  _temporaryNeoStores.deleteIdGenerators();

			  _neoStores.makeStoreOk();
			  _temporaryNeoStores.makeStoreOk();
		 }

		 private NeoStores InstantiateTempStores()
		 {
			  return NewStoreFactory( _temporaryDatabaseLayout ).openNeoStores( true, _tempStoreTypes );
		 }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public static BatchingNeoStores BatchingNeoStoresConflict( FileSystemAbstraction fileSystem, File storeDir, RecordFormats recordFormats, Configuration config, LogService logService, AdditionalInitialIds initialIds, Config dbConfig, JobScheduler jobScheduler )
		 {
			  Config neo4jConfig = GetNeo4jConfig( config, dbConfig );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.io.pagecache.tracing.PageCacheTracer tracer = new org.neo4j.io.pagecache.tracing.DefaultPageCacheTracer();
			  PageCacheTracer tracer = new DefaultPageCacheTracer();
			  PageCache pageCache = CreatePageCache( fileSystem, neo4jConfig, logService.InternalLogProvider, tracer, DefaultPageCursorTracerSupplier.INSTANCE, EmptyVersionContextSupplier.EMPTY, jobScheduler );

			  return new BatchingNeoStores( fileSystem, pageCache, storeDir, recordFormats, neo4jConfig, config, logService, initialIds, false, tracer.bytesWritten );
		 }

		 public static BatchingNeoStores BatchingNeoStoresWithExternalPageCache( FileSystemAbstraction fileSystem, PageCache pageCache, PageCacheTracer tracer, File storeDir, RecordFormats recordFormats, Configuration config, LogService logService, AdditionalInitialIds initialIds, Config dbConfig )
		 {
			  Config neo4jConfig = GetNeo4jConfig( config, dbConfig );

			  return new BatchingNeoStores( fileSystem, pageCache, storeDir, recordFormats, neo4jConfig, config, logService, initialIds, true, tracer.bytesWritten );
		 }

		 private static Config GetNeo4jConfig( Configuration config, Config dbConfig )
		 {
			  dbConfig.Augment( stringMap( dense_node_threshold.name(), valueOf(config.DenseNodeThreshold()), pagecache_memory.name(), valueOf(config.PageCacheMemory()) ) );
			  return dbConfig;
		 }

		 private static PageCache CreatePageCache( FileSystemAbstraction fileSystem, Config config, LogProvider log, PageCacheTracer tracer, PageCursorTracerSupplier cursorTracerSupplier, VersionContextSupplier contextSupplier, JobScheduler jobScheduler )
		 {
			  return ( new ConfiguringPageCacheFactory( fileSystem, config, tracer, cursorTracerSupplier, log.GetLog( typeof( BatchingNeoStores ) ), contextSupplier, jobScheduler ) ).OrCreatePageCache;
		 }

		 private StoreFactory NewStoreFactory( DatabaseLayout databaseLayout, params OpenOption[] openOptions )
		 {
			  return new StoreFactory( databaseLayout, _neo4jConfig, _idGeneratorFactory, _pageCache, _fileSystem, _recordFormats, _logProvider, EmptyVersionContextSupplier.EMPTY, openOptions );
		 }

		 /// <returns> temporary relationship group store which will be deleted in <seealso cref="close()"/>. </returns>
		 public virtual RecordStore<RelationshipGroupRecord> TemporaryRelationshipGroupStore
		 {
			 get
			 {
				  return _temporaryNeoStores.RelationshipGroupStore;
			 }
		 }

		 /// <returns> temporary property store which will be deleted in <seealso cref="close()"/>. </returns>
		 public virtual PropertyStore TemporaryPropertyStore
		 {
			 get
			 {
				  return _temporaryNeoStores.PropertyStore;
			 }
		 }

		 public virtual IoTracer IoTracer
		 {
			 get
			 {
				  return _ioTracer;
			 }
		 }

		 public virtual NodeStore NodeStore
		 {
			 get
			 {
				  return _neoStores.NodeStore;
			 }
		 }

		 public virtual PropertyStore PropertyStore
		 {
			 get
			 {
				  return _neoStores.PropertyStore;
			 }
		 }

		 public virtual BatchingPropertyKeyTokenRepository PropertyKeyRepository
		 {
			 get
			 {
				  return _propertyKeyRepository;
			 }
		 }

		 public virtual BatchingLabelTokenRepository LabelRepository
		 {
			 get
			 {
				  return _labelRepository;
			 }
		 }

		 public virtual BatchingRelationshipTypeTokenRepository RelationshipTypeRepository
		 {
			 get
			 {
				  return _relationshipTypeRepository;
			 }
		 }

		 public virtual RelationshipStore RelationshipStore
		 {
			 get
			 {
				  return _neoStores.RelationshipStore;
			 }
		 }

		 public virtual RelationshipGroupStore RelationshipGroupStore
		 {
			 get
			 {
				  return _neoStores.RelationshipGroupStore;
			 }
		 }

		 public virtual CountsTracker CountsStore
		 {
			 get
			 {
				  return _neoStores.Counts;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  // Here as a safety mechanism when e.g. panicking.
			  if ( _flusher != null )
			  {
					StopFlushingPageCache();
			  }

			  FlushAndForce();

			  // Flush out all pending changes
			  closeAll( _propertyKeyRepository, _labelRepository, _relationshipTypeRepository );

			  // Close the neo store
			  _life.shutdown();
			  closeAll( _neoStores, _temporaryNeoStores );
			  if ( !_externalPageCache )
			  {
					_pageCache.close();
			  }

			  if ( _successful )
			  {
					Cleanup();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void cleanup() throws java.io.IOException
		 private void Cleanup()
		 {
			  File tempStoreDirectory = _temporaryDatabaseLayout.StoreLayout.storeDirectory();
			  if ( !tempStoreDirectory.ParentFile.Equals( _databaseLayout.databaseDirectory() ) )
			  {
					throw new System.InvalidOperationException( "Temporary store is dislocated. It should be located under current database directory but instead located in: " + tempStoreDirectory.Parent );
			  }
			  _fileSystem.deleteRecursively( tempStoreDirectory );
		 }

		 public virtual long LastCommittedTransactionId
		 {
			 get
			 {
				  return _neoStores.MetaDataStore.LastCommittedTransactionId;
			 }
		 }

		 public virtual LabelScanStore LabelScanStore
		 {
			 get
			 {
				  return _labelScanStore;
			 }
		 }

		 public virtual NeoStores NeoStores
		 {
			 get
			 {
				  return _neoStores;
			 }
		 }

		 public virtual void StartFlushingPageCache()
		 {
			  if ( _importConfiguration.sequentialBackgroundFlushing() )
			  {
					if ( _flusher != null )
					{
						 throw new System.InvalidOperationException( "Flusher already started" );
					}
					_flusher = new PageCacheFlusher( _pageCache );
					_flusher.Start();
			  }
		 }

		 public virtual void StopFlushingPageCache()
		 {
			  if ( _importConfiguration.sequentialBackgroundFlushing() )
			  {
					if ( _flusher == null )
					{
						 throw new System.InvalidOperationException( "Flusher not started" );
					}
					_flusher.halt();
					_flusher = null;
			  }
		 }

		 public override void AcceptMemoryStatsVisitor( MemoryStatsVisitor visitor )
		 {
			  visitor.OffHeapUsage( _pageCache.maxCachedPages() * _pageCache.pageSize() );
		 }

		 public virtual PageCache PageCache
		 {
			 get
			 {
				  return _pageCache;
			 }
		 }

		 public virtual void FlushAndForce()
		 {
			  if ( _propertyKeyRepository != null )
			  {
					_propertyKeyRepository.flush();
			  }
			  if ( _labelRepository != null )
			  {
					_labelRepository.flush();
			  }
			  if ( _relationshipTypeRepository != null )
			  {
					_relationshipTypeRepository.flush();
			  }
			  if ( _neoStores != null )
			  {
					_neoStores.flush( UNLIMITED );
					FlushIdFiles( _neoStores, StoreType.values() );
			  }
			  if ( _temporaryNeoStores != null )
			  {
					_temporaryNeoStores.flush( UNLIMITED );
					FlushIdFiles( _temporaryNeoStores, _tempStoreTypes );
			  }
			  if ( _labelScanStore != null )
			  {
					_labelScanStore.force( UNLIMITED );
			  }
		 }

		 public virtual void Success()
		 {
			  _successful = true;
		 }

		 public virtual bool DetermineDoubleRelationshipRecordUnits( Input_Estimates inputEstimates )
		 {
			  _doubleRelationshipRecordUnits = _recordFormats.hasCapability( Capability.SECONDARY_RECORD_UNITS ) && inputEstimates.NumberOfRelationships() > DoubleRelationshipRecordUnitThreshold;
			  return _doubleRelationshipRecordUnits;
		 }

		 public virtual bool UsesDoubleRelationshipRecordUnits()
		 {
			  return _doubleRelationshipRecordUnits;
		 }

		 private void FlushIdFiles( NeoStores neoStores, StoreType[] storeTypes )
		 {
			  foreach ( StoreType type in storeTypes )
			  {
					if ( type.RecordStore )
					{
						 RecordStore<AbstractBaseRecord> recordStore = neoStores.GetRecordStore( type );
						 Optional<File> idFile = _databaseLayout.idFile( type.DatabaseFile );
						 idFile.ifPresent( f => _idGeneratorFactory.create( f, recordStore.HighId, false ) );
					}
			  }
		 }
	}

}