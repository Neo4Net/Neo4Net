using System;
using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.Kernel.impl.store
{

	using Neo4Net.GraphDb.config;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using ArrayUtil = Neo4Net.Helpers.ArrayUtil;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using Neo4Net.Collections.Helpers;
	using Neo4Net.Collections.Helpers;
	using Neo4Net.Collections.Helpers;
	using DiagnosticsManager = Neo4Net.Internal.Diagnostics.DiagnosticsManager;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PagedFile = Neo4Net.Io.pagecache.PagedFile;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using CountsAccessor = Neo4Net.Kernel.Impl.Api.CountsAccessor;
	using CountsTracker = Neo4Net.Kernel.impl.store.counts.CountsTracker;
	using ReadOnlyCountsTracker = Neo4Net.Kernel.impl.store.counts.ReadOnlyCountsTracker;
	using CapabilityType = Neo4Net.Kernel.impl.store.format.CapabilityType;
	using FormatFamily = Neo4Net.Kernel.impl.store.format.FormatFamily;
	using RecordFormatSelector = Neo4Net.Kernel.impl.store.format.RecordFormatSelector;
	using RecordFormats = Neo4Net.Kernel.impl.store.format.RecordFormats;
	using MetaDataRecordFormat = Neo4Net.Kernel.impl.store.format.standard.MetaDataRecordFormat;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using Neo4Net.Kernel.impl.store.kvstore;
	using AbstractBaseRecord = Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using Logger = Neo4Net.Logging.Logger;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterators.iterator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterators.loop;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.MetaDataStore.Position.STORE_VERSION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.MetaDataStore.getRecord;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.MetaDataStore.versionLongToString;

	/// <summary>
	/// This class contains the references to the "NodeStore,RelationshipStore,
	/// PropertyStore and RelationshipTypeStore". NeoStores doesn't actually "store"
	/// anything but extends the AbstractStore for the "type and version" validation
	/// performed in there.
	/// </summary>
	public class NeoStores : IDisposable
	{
		 private const string STORE_ALREADY_CLOSED_MESSAGE = "Specified store was already closed.";
		 private static readonly string _storeNotInitializedTemplate = "Specified store was not initialized. Please specify" +
																						  " %s as one of the stores types that should be open" +
																						  " to be able to use it.";

		 public static bool IsStorePresent( PageCache pageCache, DatabaseLayout databaseLayout )
		 {
			  File metaDataStore = databaseLayout.MetadataStore();
			  try
			  {
					  using ( PagedFile ignore = pageCache.Map( metaDataStore, MetaDataStore.GetPageSize( pageCache ) ) )
					  {
						return true;
					  }
			  }
			  catch ( IOException )
			  {
					return false;
			  }
		 }

		 private static readonly StoreType[] _storeTypes = StoreType.values();

		 private readonly System.Predicate<StoreType> INSTANTIATED_RECORD_STORES = ( StoreType type ) =>
		 {
			return type.RecordStore && _stores[type.ordinal()] != null;
		 };

		 private readonly DatabaseLayout _layout;
		 private readonly Config _config;
		 private readonly IdGeneratorFactory _idGeneratorFactory;
		 private readonly PageCache _pageCache;
		 private readonly LogProvider _logProvider;
		 private readonly VersionContextSupplier _versionContextSupplier;
		 private readonly bool _createIfNotExist;
		 private readonly File _metadataStore;
		 private readonly StoreType[] _initializedStores;
		 private readonly FileSystemAbstraction _fileSystemAbstraction;
		 private readonly RecordFormats _recordFormats;
		 // All stores, as Object due to CountsTracker being different that all other stores.
		 private readonly object[] _stores;
		 private readonly OpenOption[] _openOptions;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: NeoStores(Neo4Net.io.layout.DatabaseLayout layout, Neo4Net.kernel.configuration.Config config, Neo4Net.kernel.impl.store.id.IdGeneratorFactory idGeneratorFactory, Neo4Net.io.pagecache.PageCache pageCache, final Neo4Net.logging.LogProvider logProvider, Neo4Net.io.fs.FileSystemAbstraction fileSystemAbstraction, Neo4Net.io.pagecache.tracing.cursor.context.VersionContextSupplier versionContextSupplier, Neo4Net.kernel.impl.store.format.RecordFormats recordFormats, boolean createIfNotExist, StoreType[] storeTypes, java.nio.file.OpenOption[] openOptions)
		 internal NeoStores( DatabaseLayout layout, Config config, IdGeneratorFactory idGeneratorFactory, PageCache pageCache, LogProvider logProvider, FileSystemAbstraction fileSystemAbstraction, VersionContextSupplier versionContextSupplier, RecordFormats recordFormats, bool createIfNotExist, StoreType[] storeTypes, OpenOption[] openOptions )
		 {
			  this._layout = layout;
			  this._metadataStore = layout.MetadataStore();
			  this._config = config;
			  this._idGeneratorFactory = idGeneratorFactory;
			  this._pageCache = pageCache;
			  this._logProvider = logProvider;
			  this._fileSystemAbstraction = fileSystemAbstraction;
			  this._versionContextSupplier = versionContextSupplier;
			  this._recordFormats = recordFormats;
			  this._createIfNotExist = createIfNotExist;
			  this._openOptions = openOptions;

			  VerifyRecordFormat();
			  _stores = new object[StoreType.values().length];
			  try
			  {
					foreach ( StoreType type in storeTypes )
					{
						 GetOrCreateStore( type );
					}
			  }
			  catch ( Exception initException )
			  {
					try
					{
						 Close();
					}
					catch ( Exception closeException )
					{
						 initException.addSuppressed( closeException );
					}
					throw initException;
			  }
			  _initializedStores = storeTypes;
		 }

		 /// <summary>
		 /// Closes the node,relationship,property and relationship type stores.
		 /// </summary>
		 public override void Close()
		 {
			  Exception ex = null;
			  foreach ( StoreType type in _storeTypes )
			  {
					try
					{
						 CloseStore( type );
					}
					catch ( Exception t )
					{
						 ex = Exceptions.chain( ex, t );
					}
			  }

			  if ( ex != null )
			  {
					throw ex;
			  }
		 }

		 private void VerifyRecordFormat()
		 {
			  try
			  {
					string expectedStoreVersion = _recordFormats.storeVersion();
					long record = getRecord( _pageCache, _metadataStore, STORE_VERSION );
					if ( record != MetaDataRecordFormat.FIELD_NOT_PRESENT )
					{
						 string actualStoreVersion = versionLongToString( record );
						 RecordFormats actualStoreFormat = RecordFormatSelector.selectForVersion( actualStoreVersion );
						 if ( !IsCompatibleFormats( actualStoreFormat ) )
						 {
							  throw new UnexpectedStoreVersionException( actualStoreVersion, expectedStoreVersion );
						 }
					}
			  }
			  catch ( NoSuchFileException )
			  {
					// Occurs when there is no file, which is obviously when creating a store.
					// Caught as an exception because we want to leave as much interaction with files as possible
					// to the page cache.
			  }
			  catch ( IOException e )
			  {
					throw new UnderlyingStorageException( e );
			  }
		 }

		 private bool IsCompatibleFormats( RecordFormats storeFormat )
		 {
			  return FormatFamily.isSameFamily( _recordFormats, storeFormat ) && _recordFormats.hasCompatibleCapabilities( storeFormat, CapabilityType.FORMAT ) && _recordFormats.generation() >= storeFormat.Generation();
		 }

		 private void CloseStore( StoreType type )
		 {
			  int i = type.ordinal();
			  if ( _stores[i] != null )
			  {
					try
					{
						 type.close( _stores[i] );
					}
					finally
					{
						 _stores[i] = null;
					}
			  }
		 }

		 public virtual void Flush( IOLimiter limiter )
		 {
			  try
			  {
					CountsTracker counts = ( CountsTracker ) _stores[StoreType.Counts.ordinal()];
					if ( counts != null )
					{
						 Counts.rotate( MetaDataStore.LastCommittedTransactionId );
					}
					_pageCache.flushAndForce( limiter );
			  }
			  catch ( IOException e )
			  {
					throw new UnderlyingStorageException( "Failed to flush", e );
			  }
		 }

		 private object OpenStore( StoreType type )
		 {
			  int storeIndex = type.ordinal();
			  object store = type.open( this );
			  _stores[storeIndex] = store;
			  return store;
		 }

		 private T Initialize<T>( T store ) where T : CommonAbstractStore
		 {
			  store.initialise( _createIfNotExist );
			  return store;
		 }

		 /// <summary>
		 /// Returns specified store by type from already opened store array. If store is not opened exception will be
		 /// thrown.
		 /// </summary>
		 /// <seealso cref= #getOrCreateStore </seealso>
		 /// <param name="storeType"> store type to retrieve </param>
		 /// <returns> store of requested type </returns>
		 /// <exception cref="IllegalStateException"> if opened store not found </exception>
		 private object GetStore( StoreType storeType )
		 {
			  object store = _stores[storeType.ordinal()];
			  if ( store == null )
			  {
					string message = ArrayUtil.contains( _initializedStores, storeType ) ? STORE_ALREADY_CLOSED_MESSAGE : string.format( _storeNotInitializedTemplate, storeType.name() );
					throw new System.InvalidOperationException( message );
			  }
			  return store;
		 }

		 /// <summary>
		 /// Returns specified store by type from already opened store array. Will open a new store if can't find any.
		 /// Should be used only during construction of stores.
		 /// </summary>
		 /// <seealso cref= #getStore </seealso>
		 /// <param name="storeType"> store type to get or create </param>
		 /// <returns> store of requested type </returns>
		 private object GetOrCreateStore( StoreType storeType )
		 {
			  object store = _stores[storeType.ordinal()];
			  if ( store == null )
			  {
					store = OpenStore( storeType );
			  }
			  return store;
		 }

		 /// <returns> the NeoStore. </returns>
		 public virtual MetaDataStore MetaDataStore
		 {
			 get
			 {
				  return ( MetaDataStore ) GetStore( StoreType.MetaData );
			 }
		 }

		 /// <returns> The node store </returns>
		 public virtual NodeStore NodeStore
		 {
			 get
			 {
				  return ( NodeStore ) GetStore( StoreType.Node );
			 }
		 }

		 private DynamicArrayStore NodeLabelStore
		 {
			 get
			 {
				  return ( DynamicArrayStore ) GetStore( StoreType.NodeLabel );
			 }
		 }

		 /// <summary>
		 /// The relationship store.
		 /// </summary>
		 /// <returns> The relationship store </returns>
		 public virtual RelationshipStore RelationshipStore
		 {
			 get
			 {
				  return ( RelationshipStore ) GetStore( StoreType.Relationship );
			 }
		 }

		 /// <summary>
		 /// Returns the relationship type store.
		 /// </summary>
		 /// <returns> The relationship type store </returns>
		 public virtual RelationshipTypeTokenStore RelationshipTypeTokenStore
		 {
			 get
			 {
				  return ( RelationshipTypeTokenStore ) GetStore( StoreType.RelationshipTypeToken );
			 }
		 }

		 private DynamicStringStore RelationshipTypeTokenNamesStore
		 {
			 get
			 {
				  return ( DynamicStringStore ) GetStore( StoreType.RelationshipTypeTokenName );
			 }
		 }

		 /// <summary>
		 /// Returns the label store.
		 /// </summary>
		 /// <returns> The label store </returns>
		 public virtual LabelTokenStore LabelTokenStore
		 {
			 get
			 {
				  return ( LabelTokenStore ) GetStore( StoreType.LabelToken );
			 }
		 }

		 private DynamicStringStore LabelTokenNamesStore
		 {
			 get
			 {
				  return ( DynamicStringStore ) GetStore( StoreType.LabelTokenName );
			 }
		 }

		 /// <summary>
		 /// Returns the property store.
		 /// </summary>
		 /// <returns> The property store </returns>
		 public virtual PropertyStore PropertyStore
		 {
			 get
			 {
				  return ( PropertyStore ) GetStore( StoreType.Property );
			 }
		 }

		 private DynamicStringStore StringPropertyStore
		 {
			 get
			 {
				  return ( DynamicStringStore ) GetStore( StoreType.PropertyString );
			 }
		 }

		 private DynamicArrayStore ArrayPropertyStore
		 {
			 get
			 {
				  return ( DynamicArrayStore ) GetStore( StoreType.PropertyArray );
			 }
		 }

		 /// <returns> the <seealso cref="PropertyKeyTokenStore"/> </returns>
		 public virtual PropertyKeyTokenStore PropertyKeyTokenStore
		 {
			 get
			 {
				  return ( PropertyKeyTokenStore ) GetStore( StoreType.PropertyKeyToken );
			 }
		 }

		 private DynamicStringStore PropertyKeyTokenNamesStore
		 {
			 get
			 {
				  return ( DynamicStringStore ) GetStore( StoreType.PropertyKeyTokenName );
			 }
		 }

		 /// <summary>
		 /// The relationship group store.
		 /// </summary>
		 /// <returns> The relationship group store. </returns>
		 public virtual RelationshipGroupStore RelationshipGroupStore
		 {
			 get
			 {
				  return ( RelationshipGroupStore ) GetStore( StoreType.RelationshipGroup );
			 }
		 }

		 /// <returns> the schema store. </returns>
		 public virtual SchemaStore SchemaStore
		 {
			 get
			 {
				  return ( SchemaStore ) GetStore( StoreType.Schema );
			 }
		 }

		 public virtual CountsTracker Counts
		 {
			 get
			 {
				  return ( CountsTracker ) GetStore( StoreType.Counts );
			 }
		 }

		 private CountsTracker CreateWritableCountsTracker( DatabaseLayout databaseLayout )
		 {
			  return new CountsTracker( _logProvider, _fileSystemAbstraction, _pageCache, _config, databaseLayout, _versionContextSupplier );
		 }

		 private ReadOnlyCountsTracker CreateReadOnlyCountsTracker( DatabaseLayout databaseLayout )
		 {
			  return new ReadOnlyCountsTracker( _logProvider, _fileSystemAbstraction, _pageCache, _config, databaseLayout );
		 }

		 private IEnumerable<CommonAbstractStore> InstantiatedRecordStores()
		 {
			  IEnumerator<StoreType> storeTypes = new FilteringIterator<StoreType>( iterator( _storeTypes ), INSTANTIATED_RECORD_STORES );
			  return loop( new IteratorWrapperAnonymousInnerClass( this, storeTypes ) );
		 }

		 private class IteratorWrapperAnonymousInnerClass : IteratorWrapper<CommonAbstractStore, StoreType>
		 {
			 private readonly NeoStores _outerInstance;

			 public IteratorWrapperAnonymousInnerClass( NeoStores outerInstance, IEnumerator<StoreType> storeTypes ) : base( storeTypes )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override CommonAbstractStore underlyingObjectToObject( StoreType type )
			 {
				  return ( CommonAbstractStore ) _outerInstance.stores[type.ordinal()];
			 }
		 }

		 public virtual void MakeStoreOk()
		 {
			  VisitStore(store =>
			  {
				store.makeStoreOk();
				return false;
			  });
		 }

		 /// <summary>
		 /// Throws cause of store not being OK.
		 /// </summary>
		 public virtual void VerifyStoreOk()
		 {
			  VisitStore(store =>
			  {
				store.checkStoreOk();
				return false;
			  });
		 }

		 public virtual void LogVersions( Logger msgLog )
		 {
			  VisitStore(store =>
			  {
				store.logVersions( msgLog );
				return false;
			  });
		 }

		 public virtual void LogIdUsage( Logger msgLog )
		 {
			  VisitStore(store =>
			  {
				store.logIdUsage( msgLog );
				return false;
			  });
		 }

		 /// <summary>
		 /// Visits this store, and any other store managed by this store.
		 /// TODO this could, and probably should, replace all override-and-do-the-same-thing-to-all-my-managed-stores
		 /// methods like:
		 /// <seealso cref="close()"/> (where that method could be deleted all together, note a specific behaviour of Counts'Store'})
		 /// </summary>
		 public virtual void VisitStore( Visitor<CommonAbstractStore, Exception> visitor )
		 {
			  foreach ( CommonAbstractStore store in InstantiatedRecordStores() )
			  {
					store.visitStore( visitor );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void startCountStore() throws java.io.IOException
		 public virtual void StartCountStore()
		 {
			  // TODO: move this to LifeCycle
			  Counts.start();
		 }

		 public virtual void DeleteIdGenerators()
		 {
			  VisitStore(store =>
			  {
				store.deleteIdGenerator();
				return false;
			  });
		 }

		 public virtual void AssertOpen()
		 {
			  if ( _stores[StoreType.Node.ordinal()] == null )
			  {
					throw new System.InvalidOperationException( "Database has been shutdown" );
			  }
		 }

		 internal virtual CommonAbstractStore CreateNodeStore()
		 {
			  return Initialize( new NodeStore( _layout.nodeStore(), _layout.idNodeStore(), _config, _idGeneratorFactory, _pageCache, _logProvider, (DynamicArrayStore) GetOrCreateStore(StoreType.NodeLabel), _recordFormats, _openOptions ) );
		 }

		 internal virtual CommonAbstractStore CreateNodeLabelStore()
		 {
			  return CreateDynamicArrayStore( _layout.nodeLabelStore(), _layout.idNodeLabelStore(), IdType.NODE_LABELS, GraphDatabaseSettings.label_block_size );
		 }

		 internal virtual CommonAbstractStore CreatePropertyKeyTokenStore()
		 {
			  return Initialize( new PropertyKeyTokenStore( _layout.propertyKeyTokenStore(), _layout.idPropertyKeyTokenStore(), _config, _idGeneratorFactory, _pageCache, _logProvider, (DynamicStringStore) GetOrCreateStore(StoreType.PropertyKeyTokenName), _recordFormats, _openOptions ) );
		 }

		 internal virtual CommonAbstractStore CreatePropertyKeyTokenNamesStore()
		 {
			  return CreateDynamicStringStore( _layout.propertyKeyTokenNamesStore(), _layout.idPropertyKeyTokenNamesStore(), IdType.PROPERTY_KEY_TOKEN_NAME, TokenStore.NAME_STORE_BLOCK_SIZE );
		 }

		 internal virtual CommonAbstractStore CreatePropertyStore()
		 {
			  return Initialize( new PropertyStore( _layout.propertyStore(), _layout.idPropertyStore(), _config, _idGeneratorFactory, _pageCache, _logProvider, (DynamicStringStore) GetOrCreateStore(StoreType.PropertyString), (PropertyKeyTokenStore) GetOrCreateStore(StoreType.PropertyKeyToken), (DynamicArrayStore) GetOrCreateStore(StoreType.PropertyArray), _recordFormats, _openOptions ) );
		 }

		 internal virtual CommonAbstractStore CreatePropertyStringStore()
		 {
			  return CreateDynamicStringStore( _layout.propertyStringStore(), _layout.idPropertyStringStore(), IdType.STRING_BLOCK, GraphDatabaseSettings.string_block_size );
		 }

		 internal virtual CommonAbstractStore CreatePropertyArrayStore()
		 {
			  return CreateDynamicArrayStore( _layout.propertyArrayStore(), _layout.idPropertyArrayStore(), IdType.ARRAY_BLOCK, GraphDatabaseSettings.array_block_size );
		 }

		 internal virtual CommonAbstractStore CreateRelationshipStore()
		 {
			  return Initialize( new RelationshipStore( _layout.relationshipStore(), _layout.idRelationshipStore(), _config, _idGeneratorFactory, _pageCache, _logProvider, _recordFormats, _openOptions ) );
		 }

		 internal virtual CommonAbstractStore CreateRelationshipTypeTokenStore()
		 {
			  return Initialize( new RelationshipTypeTokenStore( _layout.relationshipTypeTokenStore(), _layout.idRelationshipTypeTokenStore(), _config, _idGeneratorFactory, _pageCache, _logProvider, (DynamicStringStore) GetOrCreateStore(StoreType.RelationshipTypeTokenName), _recordFormats, _openOptions ) );
		 }

		 internal virtual CommonAbstractStore CreateRelationshipTypeTokenNamesStore()
		 {
			  return CreateDynamicStringStore( _layout.relationshipTypeTokenNamesStore(), _layout.idRelationshipTypeTokenNamesStore(), IdType.RELATIONSHIP_TYPE_TOKEN_NAME, TokenStore.NAME_STORE_BLOCK_SIZE );
		 }

		 internal virtual CommonAbstractStore CreateLabelTokenStore()
		 {
			  return Initialize( new LabelTokenStore( _layout.labelTokenStore(), _layout.idLabelTokenStore(), _config, _idGeneratorFactory, _pageCache, _logProvider, (DynamicStringStore) GetOrCreateStore(StoreType.LabelTokenName), _recordFormats, _openOptions ) );
		 }

		 internal virtual CommonAbstractStore CreateSchemaStore()
		 {
			  return Initialize( new SchemaStore( _layout.schemaStore(), _layout.idSchemaStore(), _config, IdType.SCHEMA, _idGeneratorFactory, _pageCache, _logProvider, _recordFormats, _openOptions ) );
		 }

		 internal virtual CommonAbstractStore CreateRelationshipGroupStore()
		 {
			  return Initialize( new RelationshipGroupStore( _layout.relationshipGroupStore(), _layout.idRelationshipGroupStore(), _config, _idGeneratorFactory, _pageCache, _logProvider, _recordFormats, _openOptions ) );
		 }

		 internal virtual CommonAbstractStore CreateLabelTokenNamesStore()
		 {
			  return CreateDynamicStringStore( _layout.labelTokenNamesStore(), _layout.idLabelTokenNamesStore(), IdType.LABEL_TOKEN_NAME, TokenStore.NAME_STORE_BLOCK_SIZE );
		 }

		 internal virtual CountsTracker CreateCountStore()
		 {
			  bool readOnly = _config.get( GraphDatabaseSettings.read_only );
			  CountsTracker counts = readOnly ? CreateReadOnlyCountsTracker( _layout ) : CreateWritableCountsTracker( _layout );
			  NeoStores neoStores = this;
			  Counts.Initializer = new DataInitializerAnonymousInnerClass( this, neoStores );

			  try
			  {
					Counts.init(); // TODO: move this to LifeCycle
			  }
			  catch ( IOException e )
			  {
					throw new UnderlyingStorageException( "Failed to initialize counts store", e );
			  }
			  return counts;
		 }

		 private class DataInitializerAnonymousInnerClass : DataInitializer<Neo4Net.Kernel.Impl.Api.CountsAccessor_Updater>
		 {
			 private readonly NeoStores _outerInstance;

			 private Neo4Net.Kernel.impl.store.NeoStores _neoStores;

			 public DataInitializerAnonymousInnerClass( NeoStores outerInstance, Neo4Net.Kernel.impl.store.NeoStores neoStores )
			 {
				 this.outerInstance = outerInstance;
				 this._neoStores = neoStores;
				 log = outerInstance.logProvider.GetLog( typeof( MetaDataStore ) );
			 }

			 private readonly Log log;

			 public void initialize( Neo4Net.Kernel.Impl.Api.CountsAccessor_Updater updater )
			 {
				  log.warn( "Missing counts store, rebuilding it." );
				  ( new CountsComputer( _neoStores, _outerInstance.pageCache, _outerInstance.layout ) ).initialize( updater );
				  log.warn( "Counts store rebuild completed." );
			 }

			 public long initialVersion()
			 {
				  return ( ( MetaDataStore ) outerInstance.getOrCreateStore( StoreType.MetaData ) ).LastCommittedTransactionId;
			 }
		 }

		 internal virtual CommonAbstractStore CreateMetadataStore()
		 {
			  return Initialize( new MetaDataStore( _metadataStore, _layout.idMetadataStore(), _config, _idGeneratorFactory, _pageCache, _logProvider, _recordFormats.metaData(), _recordFormats.storeVersion(), _openOptions ) );
		 }

		 private CommonAbstractStore CreateDynamicStringStore( File storeFile, File idFile, IdType idType, Setting<int> blockSizeProperty )
		 {
			  return createDynamicStringStore( storeFile, idFile, idType, _config.get( blockSizeProperty ) );
		 }

		 private CommonAbstractStore CreateDynamicStringStore( File storeFile, File idFile, IdType idType, int blockSize )
		 {
			  return Initialize( new DynamicStringStore( storeFile, idFile, _config, idType, _idGeneratorFactory, _pageCache, _logProvider, blockSize, _recordFormats.dynamic(), _recordFormats.storeVersion(), _openOptions ) );
		 }

		 private CommonAbstractStore CreateDynamicArrayStore( File storeFile, File idFile, IdType idType, Setting<int> blockSizeProperty )
		 {
			  return createDynamicArrayStore( storeFile, idFile, idType, _config.get( blockSizeProperty ) );
		 }

		 internal virtual CommonAbstractStore CreateDynamicArrayStore( File storeFile, File idFile, IdType idType, int blockSize )
		 {
			  if ( blockSize <= 0 )
			  {
					throw new System.ArgumentException( "Block size of dynamic array store should be positive integer." );
			  }
			  return Initialize( new DynamicArrayStore( storeFile, idFile, _config, idType, _idGeneratorFactory, _pageCache, _logProvider, blockSize, _recordFormats, _openOptions ) );
		 }

		 public virtual void RegisterDiagnostics( DiagnosticsManager diagnosticsManager )
		 {
			  diagnosticsManager.RegisterAll( typeof( NeoStoresDiagnostics ), this );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public <RECORD extends Neo4Net.kernel.impl.store.record.AbstractBaseRecord> RecordStore<RECORD> getRecordStore(StoreType type)
		 public virtual RecordStore<RECORD> GetRecordStore<RECORD>( StoreType type ) where RECORD : Neo4Net.Kernel.Impl.Store.Records.AbstractBaseRecord
		 {
			  Debug.Assert( type.RecordStore );
			  return ( RecordStore<RECORD> ) GetStore( type );
		 }

		 public virtual RecordFormats RecordFormats
		 {
			 get
			 {
				  return _recordFormats;
			 }
		 }
	}

}