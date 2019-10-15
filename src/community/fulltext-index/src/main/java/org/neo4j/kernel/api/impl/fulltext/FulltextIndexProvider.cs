using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

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
namespace Neo4Net.Kernel.Api.Impl.Fulltext
{
	using ParseException = org.apache.lucene.queryparser.classic.ParseException;


	using AnalyzerProvider = Neo4Net.Graphdb.index.fulltext.AnalyzerProvider;
	using IndexCapability = Neo4Net.Internal.Kernel.Api.IndexCapability;
	using IndexReference = Neo4Net.Internal.Kernel.Api.IndexReference;
	using InternalIndexState = Neo4Net.Internal.Kernel.Api.InternalIndexState;
	using IndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using MisconfiguredIndexException = Neo4Net.Internal.Kernel.Api.exceptions.schema.MisconfiguredIndexException;
	using IndexProviderDescriptor = Neo4Net.Internal.Kernel.Api.schema.IndexProviderDescriptor;
	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using Neo4Net.Kernel.Api.Impl.Index;
	using DirectoryFactory = Neo4Net.Kernel.Api.Impl.Index.storage.DirectoryFactory;
	using IndexStorageFactory = Neo4Net.Kernel.Api.Impl.Index.storage.IndexStorageFactory;
	using PartitionedIndexStorage = Neo4Net.Kernel.Api.Impl.Index.storage.PartitionedIndexStorage;
	using LuceneSchemaIndexBuilder = Neo4Net.Kernel.Api.Impl.Schema.LuceneSchemaIndexBuilder;
	using SchemaIndex = Neo4Net.Kernel.Api.Impl.Schema.SchemaIndex;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using IndexDirectoryStructure = Neo4Net.Kernel.Api.Index.IndexDirectoryStructure;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using AuxiliaryTransactionState = Neo4Net.Kernel.api.txstate.auxiliary.AuxiliaryTransactionState;
	using AuxiliaryTransactionStateManager = Neo4Net.Kernel.api.txstate.auxiliary.AuxiliaryTransactionStateManager;
	using AuxiliaryTransactionStateProvider = Neo4Net.Kernel.api.txstate.auxiliary.AuxiliaryTransactionStateProvider;
	using Config = Neo4Net.Kernel.configuration.Config;
	using KernelTransactionImplementation = Neo4Net.Kernel.Impl.Api.KernelTransactionImplementation;
	using IndexSamplingConfig = Neo4Net.Kernel.Impl.Api.index.sampling.IndexSamplingConfig;
	using TokenHolders = Neo4Net.Kernel.impl.core.TokenHolders;
	using OperationalMode = Neo4Net.Kernel.impl.factory.OperationalMode;
	using ByteBufferFactory = Neo4Net.Kernel.Impl.Index.Schema.ByteBufferFactory;
	using AllStoreHolder = Neo4Net.Kernel.Impl.Newapi.AllStoreHolder;
	using StoreMigrationParticipant = Neo4Net.Kernel.impl.storemigration.StoreMigrationParticipant;
	using SchemaIndexMigrator = Neo4Net.Kernel.impl.storemigration.participant.SchemaIndexMigrator;
	using Log = Neo4Net.Logging.Log;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using EntityType = Neo4Net.Storageengine.Api.EntityType;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.exceptions.Status_General.InvalidArguments;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.FulltextIndexSettings.readOrInitializeDescriptor;

	internal class FulltextIndexProvider : IndexProvider, FulltextAdapter, AuxiliaryTransactionStateProvider
	{
		 private const string TX_STATE_PROVIDER_KEY = "FULLTEXT SCHEMA INDEX TRANSACTION STATE";

		 private readonly FileSystemAbstraction _fileSystem;
		 private readonly Config _config;
		 private readonly TokenHolders _tokenHolders;
		 private readonly OperationalMode _operationalMode;
		 private readonly string _defaultAnalyzerName;
		 private readonly string _defaultEventuallyConsistentSetting;
		 private readonly AuxiliaryTransactionStateManager _auxiliaryTransactionStateManager;
		 private readonly Log _log;
		 private readonly IndexUpdateSink _indexUpdateSink;
		 private readonly ConcurrentMap<StoreIndexDescriptor, FulltextIndexAccessor> _openOnlineAccessors;
		 private readonly IndexStorageFactory _indexStorageFactory;

		 internal FulltextIndexProvider( IndexProviderDescriptor descriptor, IndexDirectoryStructure.Factory directoryStructureFactory, FileSystemAbstraction fileSystem, Config config, TokenHolders tokenHolders, DirectoryFactory directoryFactory, OperationalMode operationalMode, IJobScheduler scheduler, AuxiliaryTransactionStateManager auxiliaryTransactionStateManager, Log log ) : base( descriptor, directoryStructureFactory )
		 {
			  this._fileSystem = fileSystem;
			  this._config = config;
			  this._tokenHolders = tokenHolders;
			  this._operationalMode = operationalMode;
			  this._auxiliaryTransactionStateManager = auxiliaryTransactionStateManager;
			  this._log = log;

			  _defaultAnalyzerName = config.Get( FulltextConfig.FulltextDefaultAnalyzer );
			  _defaultEventuallyConsistentSetting = Convert.ToString( config.Get( FulltextConfig.EventuallyConsistent ) );
			  _indexUpdateSink = new IndexUpdateSink( scheduler, config.Get( FulltextConfig.EventuallyConsistentIndexUpdateQueueMaxLength ) );
			  _openOnlineAccessors = new ConcurrentDictionary<StoreIndexDescriptor, FulltextIndexAccessor>();
			  _indexStorageFactory = BuildIndexStorageFactory( fileSystem, directoryFactory );
		 }

		 private IndexStorageFactory BuildIndexStorageFactory( FileSystemAbstraction fileSystem, DirectoryFactory directoryFactory )
		 {
			  return new IndexStorageFactory( directoryFactory, fileSystem, DirectoryStructure() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean indexIsOnline(org.neo4j.kernel.api.impl.index.storage.PartitionedIndexStorage indexStorage, org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor) throws java.io.IOException
		 private bool IndexIsOnline( PartitionedIndexStorage indexStorage, StoreIndexDescriptor descriptor )
		 {
			  using ( SchemaIndex index = LuceneSchemaIndexBuilder.create( descriptor, _config ).withIndexStorage( indexStorage ).build() )
			  {
					if ( index.exists() )
					{
						 index.open();
						 return index.Online;
					}
					return false;
			  }
		 }

		 private PartitionedIndexStorage GetIndexStorage( long indexId )
		 {
			  return _indexStorageFactory.indexStorageOf( indexId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
		 public override void Start()
		 {
			  base.Start();
			  _auxiliaryTransactionStateManager.registerProvider( this );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stop() throws Throwable
		 public override void Stop()
		 {
			  _auxiliaryTransactionStateManager.unregisterProvider( this );
			  _indexStorageFactory.close();
		 }

		 public override IndexCapability GetCapability( StoreIndexDescriptor descriptor )
		 {
			  FulltextIndexDescriptor fulltextIndexDescriptor;
			  if ( descriptor is FulltextIndexDescriptor )
			  {
					// We got our own index descriptor type, so we can ask it directly.
					fulltextIndexDescriptor = ( FulltextIndexDescriptor ) descriptor;
					return new FulltextIndexCapability( fulltextIndexDescriptor.EventuallyConsistent );
			  }
			  SchemaDescriptor schema = descriptor.Schema();
			  if ( schema is FulltextSchemaDescriptor )
			  {
					// The fulltext schema descriptor is readily available with our settings.
					// This could be the situation where the index creation is about to be committed.
					// In that case, the schema descriptor is our own legit type, but the StoreIndexDescriptor is generic.
					FulltextSchemaDescriptor fulltextSchemaDescriptor = ( FulltextSchemaDescriptor ) schema;
					return new FulltextIndexCapability( fulltextSchemaDescriptor.EventuallyConsistent );
			  }
			  // The schema descriptor is probably a generic multi-token descriptor.
			  // This happens if it was loaded from the schema store instead of created by our provider.
			  // This would be the case when the IndexingService is starting up, and if so, we probably have an online accessor that we can ask instead.
			  FulltextIndexAccessor accessor = GetOpenOnlineAccessor( descriptor );
			  if ( accessor != null )
			  {
					fulltextIndexDescriptor = accessor.Descriptor;
					return new FulltextIndexCapability( fulltextIndexDescriptor.EventuallyConsistent );
			  }
			  // All of the above has failed, so we need to load the settings in from the storage directory of the index.
			  // This situation happens during recovery.
			  PartitionedIndexStorage indexStorage = GetIndexStorage( descriptor.Id );
			  fulltextIndexDescriptor = readOrInitializeDescriptor( descriptor, _defaultAnalyzerName, _tokenHolders.propertyKeyTokens(), indexStorage.IndexFolder, _fileSystem );
			  return new FulltextIndexCapability( fulltextIndexDescriptor.EventuallyConsistent );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.storageengine.api.schema.IndexDescriptor bless(org.neo4j.storageengine.api.schema.IndexDescriptor index) throws org.neo4j.internal.kernel.api.exceptions.schema.MisconfiguredIndexException
		 public override IndexDescriptor Bless( IndexDescriptor index )
		 {
			  if ( !( index.Schema() is FulltextSchemaDescriptor ) )
			  {
					// The fulltext index provider only support fulltext indexes.
					throw new MisconfiguredIndexException( InvalidArguments, "The index provider '" + ProviderDescriptor + "' only supports fulltext index " + "descriptors. Make sure that fulltext indexes are created using the relevant fulltext index procedures." );
			  }
			  return base.Bless( index );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public String getPopulationFailure(org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor) throws IllegalStateException
		 public override string GetPopulationFailure( StoreIndexDescriptor descriptor )
		 {
			  string failure = GetIndexStorage( descriptor.Id ).StoredIndexFailure;
			  if ( string.ReferenceEquals( failure, null ) )
			  {
					throw new System.InvalidOperationException( "Index " + descriptor.Id + " isn't failed" );
			  }
			  return failure;
		 }

		 public override InternalIndexState GetInitialState( StoreIndexDescriptor descriptor )
		 {
			  PartitionedIndexStorage indexStorage = GetIndexStorage( descriptor.Id );
			  string failure = indexStorage.StoredIndexFailure;
			  if ( !string.ReferenceEquals( failure, null ) )
			  {
					return InternalIndexState.FAILED;
			  }
			  try
			  {
					return IndexIsOnline( indexStorage, descriptor ) ? InternalIndexState.ONLINE : InternalIndexState.POPULATING;
			  }
			  catch ( IOException )
			  {
					return InternalIndexState.POPULATING;
			  }
		 }

		 public override IndexPopulator GetPopulator( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig, ByteBufferFactory bufferFactory )
		 {
			  PartitionedIndexStorage indexStorage = GetIndexStorage( descriptor.Id );
			  FulltextIndexDescriptor fulltextIndexDescriptor = readOrInitializeDescriptor( descriptor, _defaultAnalyzerName, _tokenHolders.propertyKeyTokens(), indexStorage.IndexFolder, _fileSystem );
			  DatabaseIndex<FulltextIndexReader> fulltextIndex = FulltextIndexBuilder.Create( fulltextIndexDescriptor, _config, _tokenHolders.propertyKeyTokens() ).withFileSystem(_fileSystem).withOperationalMode(_operationalMode).withIndexStorage(indexStorage).withPopulatingMode(true).build();
			  if ( fulltextIndex.ReadOnly )
			  {
					throw new System.NotSupportedException( "Can't create populator for read only index" );
			  }
			  _log.debug( "Creating populator for fulltext schema index: %s", descriptor );
			  return new FulltextIndexPopulator( fulltextIndexDescriptor, fulltextIndex, () => FulltextIndexSettings.saveFulltextIndexSettings(fulltextIndexDescriptor, indexStorage.IndexFolder, _fileSystem) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.api.index.IndexAccessor getOnlineAccessor(org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor, org.neo4j.kernel.impl.api.index.sampling.IndexSamplingConfig samplingConfig) throws java.io.IOException
		 public override IndexAccessor GetOnlineAccessor( StoreIndexDescriptor descriptor, IndexSamplingConfig samplingConfig )
		 {
			  PartitionedIndexStorage indexStorage = GetIndexStorage( descriptor.Id );

			  FulltextIndexDescriptor fulltextIndexDescriptor = readOrInitializeDescriptor( descriptor, _defaultAnalyzerName, _tokenHolders.propertyKeyTokens(), indexStorage.IndexFolder, _fileSystem );
			  FulltextIndexBuilder fulltextIndexBuilder = FulltextIndexBuilder.Create( fulltextIndexDescriptor, _config, _tokenHolders.propertyKeyTokens() ).withFileSystem(_fileSystem).withOperationalMode(_operationalMode).withIndexStorage(indexStorage).withPopulatingMode(false);
			  if ( fulltextIndexDescriptor.EventuallyConsistent )
			  {
					fulltextIndexBuilder = fulltextIndexBuilder.WithIndexUpdateSink( _indexUpdateSink );
			  }
			  DatabaseFulltextIndex fulltextIndex = fulltextIndexBuilder.Build();
			  fulltextIndex.open();

			  ThreadStart onClose = () => _openOnlineAccessors.remove(descriptor);
			  FulltextIndexAccessor accessor = new FulltextIndexAccessor( _indexUpdateSink, fulltextIndex, fulltextIndexDescriptor, onClose );
			  _openOnlineAccessors.put( descriptor, accessor );
			  _log.debug( "Created online accessor for fulltext schema index %s: %s", descriptor, accessor );
			  return accessor;
		 }

		 internal virtual FulltextIndexAccessor GetOpenOnlineAccessor( StoreIndexDescriptor descriptor )
		 {
			  return _openOnlineAccessors.get( descriptor );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.neo4j.kernel.impl.storemigration.StoreMigrationParticipant storeMigrationParticipant(final org.neo4j.io.fs.FileSystemAbstraction fs, org.neo4j.io.pagecache.PageCache pageCache)
		 public override StoreMigrationParticipant StoreMigrationParticipant( FileSystemAbstraction fs, PageCache pageCache )
		 {
			  return new SchemaIndexMigrator( fs, this );
		 }

		 public override SchemaDescriptor SchemaFor( EntityType type, string[] entityTokens, Properties indexConfiguration, params string[] properties )
		 {
			  if ( entityTokens.Length == 0 )
			  {
					throw new BadSchemaException( "At least one " + ( type == EntityType.NODE ? "label" : "relationship type" ) + " must be specified when creating a fulltext index." );
			  }
			  if ( properties.Length == 0 )
			  {
					throw new BadSchemaException( "At least one property name must be specified when creating a fulltext index." );
			  }
			  if ( Arrays.asList( properties ).contains( LuceneFulltextDocumentStructure.FIELD_ENTITY_ID ) )
			  {
					throw new BadSchemaException( "Unable to index the property, the name is reserved for internal use " + LuceneFulltextDocumentStructure.FIELD_ENTITY_ID );
			  }
			  int[] entityTokenIds = new int[entityTokens.Length];
			  if ( type == EntityType.NODE )
			  {
					_tokenHolders.labelTokens().getOrCreateIds(entityTokens, entityTokenIds);
			  }
			  else
			  {
					_tokenHolders.relationshipTypeTokens().getOrCreateIds(entityTokens, entityTokenIds);
			  }
			  int[] propertyIds = java.util.properties.Select( _tokenHolders.propertyKeyTokens().getOrCreateId ).ToArray();

			  SchemaDescriptor schema = SchemaDescriptorFactory.multiToken( entityTokenIds, type, propertyIds );
			  indexConfiguration.putIfAbsent( FulltextIndexSettings.INDEX_CONFIG_ANALYZER, _defaultAnalyzerName );
			  indexConfiguration.putIfAbsent( FulltextIndexSettings.INDEX_CONFIG_EVENTUALLY_CONSISTENT, _defaultEventuallyConsistentSetting );
			  return new FulltextSchemaDescriptor( schema, indexConfiguration );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public ScoreEntityIterator query(org.neo4j.kernel.api.KernelTransaction ktx, String indexName, String queryString) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException, org.apache.lucene.queryparser.classic.ParseException
		 public override ScoreEntityIterator Query( KernelTransaction ktx, string indexName, string queryString )
		 {
			  KernelTransactionImplementation kti = ( KernelTransactionImplementation ) ktx;
			  AllStoreHolder allStoreHolder = ( AllStoreHolder ) kti.DataRead();
			  IndexReference indexReference = kti.SchemaRead().indexGetForName(indexName);
			  FulltextIndexReader fulltextIndexReader;
			  if ( kti.HasTxStateWithChanges() && !((FulltextSchemaDescriptor) indexReference.Schema()).EventuallyConsistent )
			  {
					FulltextAuxiliaryTransactionState auxiliaryTxState = ( FulltextAuxiliaryTransactionState ) allStoreHolder.AuxiliaryTxState( TX_STATE_PROVIDER_KEY );
					fulltextIndexReader = auxiliaryTxState.IndexReader( indexReference, kti );
			  }
			  else
			  {
					IndexReader indexReader = allStoreHolder.IndexReader( indexReference, false );
					fulltextIndexReader = ( FulltextIndexReader ) indexReader;
			  }
			  return fulltextIndexReader.Query( queryString );
		 }

		 public override void AwaitRefresh()
		 {
			  _indexUpdateSink.awaitUpdateApplication();
		 }

		 public override Stream<AnalyzerProvider> ListAvailableAnalyzers()
		 {
			  IEnumerable<AnalyzerProvider> providers = AnalyzerProvider.load( typeof( AnalyzerProvider ) );
			  return StreamSupport.stream( providers.spliterator(), false );
		 }

		 public virtual object IdentityKey
		 {
			 get
			 {
				  return TX_STATE_PROVIDER_KEY;
			 }
		 }

		 public override AuxiliaryTransactionState CreateNewAuxiliaryTransactionState()
		 {
			  return new FulltextAuxiliaryTransactionState( this, _log );
		 }
	}

}