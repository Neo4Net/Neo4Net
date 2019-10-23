using System;
using System.Collections.Concurrent;
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
namespace Neo4Net.Index.impl.lucene.@explicit
{
	using Analyzer = org.apache.lucene.analysis.Analyzer;
	using TokenStream = org.apache.lucene.analysis.TokenStream;
	using Tokenizer = org.apache.lucene.analysis.Tokenizer;
	using KeywordAnalyzer = org.apache.lucene.analysis.core.KeywordAnalyzer;
	using LowerCaseFilter = org.apache.lucene.analysis.core.LowerCaseFilter;
	using WhitespaceAnalyzer = org.apache.lucene.analysis.core.WhitespaceAnalyzer;
	using WhitespaceTokenizer = org.apache.lucene.analysis.core.WhitespaceTokenizer;
	using Document = org.apache.lucene.document.Document;
	using DirectoryReader = Org.Apache.Lucene.Index.DirectoryReader;
	using DocValuesType = Org.Apache.Lucene.Index.DocValuesType;
	using FieldInfo = Org.Apache.Lucene.Index.FieldInfo;
	using IndexCommit = Org.Apache.Lucene.Index.IndexCommit;
	using IndexReader = Org.Apache.Lucene.Index.IndexReader;
	using IndexableField = Org.Apache.Lucene.Index.IndexableField;
	using LeafReaderContext = Org.Apache.Lucene.Index.LeafReaderContext;
	using SnapshotDeletionPolicy = Org.Apache.Lucene.Index.SnapshotDeletionPolicy;
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;
	using Sort = org.apache.lucene.search.Sort;
	using TopDocs = org.apache.lucene.search.TopDocs;
	using TopFieldCollector = org.apache.lucene.search.TopFieldCollector;
	using Directory = org.apache.lucene.store.Directory;
	using FSDirectory = org.apache.lucene.store.FSDirectory;
	using RAMDirectory = org.apache.lucene.store.RAMDirectory;


	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Neo4Net.GraphDb;
	using Neo4Net.GraphDb.config;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using IndexManager = Neo4Net.GraphDb.index.IndexManager;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using Neo4Net.Helpers.Collections;
	using Neo4Net.Helpers.Collections;
	using ExplicitIndexNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using OperationalMode = Neo4Net.Kernel.impl.factory.OperationalMode;
	using IndexConfigStore = Neo4Net.Kernel.impl.index.IndexConfigStore;
	using IndexEntityType = Neo4Net.Kernel.impl.index.IndexEntityType;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;


	/// <summary>
	/// An DataSource optimized for the <seealso cref="LuceneIndexImplementation"/>.
	/// </summary>
	public class LuceneDataSource : LifecycleAdapter
	{
		 public abstract class Configuration
		 {
			  public static readonly Setting<int> LuceneSearcherCacheSize = GraphDatabaseSettings.lucene_searcher_cache_size;
			  public static readonly Setting<bool> Ephemeral = GraphDatabaseSettings.ephemeral;
		 }

		 /// <summary>
		 /// Default <seealso cref="Analyzer"/> for fulltext parsing.
		 /// </summary>
		 internal static readonly Analyzer LOWER_CASE_WHITESPACE_ANALYZER = new AnalyzerAnonymousInnerClass();

		 private class AnalyzerAnonymousInnerClass : Analyzer
		 {
			 protected internal override TokenStreamComponents createComponents( string fieldName )
			 {
				  Tokenizer source = new WhitespaceTokenizer();
				  TokenStream filter = new LowerCaseFilter( source );
				  return new TokenStreamComponents( source, filter );
			 }

			 public override string ToString()
			 {
				  return "LOWER_CASE_WHITESPACE_ANALYZER";
			 }
		 }

		 public static readonly Analyzer WhitespaceAnalyzer = new WhitespaceAnalyzer();
		 public static readonly Analyzer KeywordAnalyzer = new KeywordAnalyzer();
		 private readonly DatabaseLayout _directoryStructure;
		 private readonly Config _config;
		 private readonly FileSystemAbstraction _fileSystemAbstraction;
		 private readonly OperationalMode _operationalMode;
		 private IndexClockCache _indexSearchers;
		 private File _baseStorePath;
		 private readonly ReentrantReadWriteLock @lock = new ReentrantReadWriteLock();
		 private readonly IndexConfigStore _indexStore;
		 private IndexTypeCache _typeCache;
		 private bool _readOnly;
		 private bool _closed;
		 private IndexReferenceFactory _indexReferenceFactory;
		 private IDictionary<IndexIdentifier, IDictionary<string, DocValuesType>> _indexTypeMap;

		 /// <summary>
		 /// Constructs this data source.
		 /// </summary>
		 public LuceneDataSource( DatabaseLayout directoryStructure, Config config, IndexConfigStore indexStore, FileSystemAbstraction fileSystemAbstraction, OperationalMode operationalMode )
		 {
			  this._directoryStructure = directoryStructure;
			  this._config = config;
			  this._indexStore = indexStore;
			  this._typeCache = new IndexTypeCache( indexStore );
			  this._fileSystemAbstraction = fileSystemAbstraction;
			  this._operationalMode = operationalMode;
		 }

		 public override void Init()
		 {
			  LuceneFilesystemFacade filesystemFacade = _config.get( Configuration.Ephemeral ) ? LuceneFilesystemFacade.Memory : LuceneFilesystemFacade.Fs;
			  _readOnly = IsReadOnly( _config, _operationalMode );
			  _indexSearchers = new IndexClockCache( _config.get( Configuration.LuceneSearcherCacheSize ) );
			  this._baseStorePath = filesystemFacade.ensureDirectoryExists( _fileSystemAbstraction, GetLuceneIndexStoreDirectory( _directoryStructure ) );
			  filesystemFacade.cleanWriteLocks( _baseStorePath );
			  this._typeCache = new IndexTypeCache( _indexStore );
			  this._indexReferenceFactory = _readOnly ? new ReadOnlyIndexReferenceFactory( filesystemFacade, _baseStorePath, _typeCache ) : new WritableIndexReferenceFactory( filesystemFacade, _baseStorePath, _typeCache );
			  this._indexTypeMap = new ConcurrentDictionary<IndexIdentifier, IDictionary<string, DocValuesType>>();
			  _closed = false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void assertValidType(String key, Object value, IndexIdentifier identifier) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 internal virtual void AssertValidType( string key, object value, IndexIdentifier identifier )
		 {
			  DocValuesType expectedType;
			  string expectedTypeName;
			  if ( value is Number )
			  {
					expectedType = DocValuesType.SORTED_NUMERIC;
					expectedTypeName = "numbers";
			  }
			  else
			  {
					expectedType = DocValuesType.SORTED_SET;
					expectedTypeName = "strings";
			  }
			  IDictionary<string, DocValuesType> stringDocValuesTypeMap = _indexTypeMap[identifier];
			  // If the index searcher has never been loaded, we need to load it now to populate the map.
			  int iterations = 0; // Iterate a bit in case we race with an index drop or create.
			  while ( stringDocValuesTypeMap == null && iterations++ < 20 )
			  {
					// We don't use ensureInstantiated because we want to surface the exception in this case.
					GetIndexSearcher( identifier ).close();
					stringDocValuesTypeMap = _indexTypeMap[identifier];
			  }

			  if ( stringDocValuesTypeMap == null )
			  {
					// Looks like we are running into some adversarial racing, so let's just give up.
					throw new ExplicitIndexNotFoundKernelException( "Index '%s' doesn't exist.", identifier );
			  }

			  DocValuesType actualType = stringDocValuesTypeMap.putIfAbsent( key, expectedType );
			  if ( actualType != null && !actualType.Equals( DocValuesType.NONE ) && !actualType.Equals( expectedType ) )
			  {
					throw new System.ArgumentException( string.Format( "Cannot index '{0}' for key '{1}', since this key has been used to index {2}. Raw value of the index type is {3}", value, key, expectedTypeName, actualType ) );

			  }
		 }

		 public static File GetLuceneIndexStoreDirectory( DatabaseLayout directoryStructure )
		 {
			  return directoryStructure.File( "index" );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: IndexType getType(IndexIdentifier identifier, boolean recovery) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 internal virtual IndexType GetType( IndexIdentifier identifier, bool recovery )
		 {
			  return _typeCache.getIndexType( identifier, recovery );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void shutdown() throws java.io.IOException
		 public override void Shutdown()
		 {
			  lock ( this )
			  {
					if ( _closed )
					{
						 return;
					}
					_closed = true;
					foreach ( IndexReference searcher in _indexSearchers.values() )
					{
						 searcher.Dispose();
					}
					_indexSearchers.clear();
			  }
		 }

		 private IndexReference[] AllIndexes
		 {
			 get
			 {
				 lock ( this )
				 {
					  Neo4Net.Index.impl.lucene.@explicit.IndexClockCache.ValueCollection indexReferences = _indexSearchers.values();
					  return indexReferences.toArray( new IndexReference[0] );
				 }
			 }
		 }

		 internal virtual void Force()
		 {
			  if ( _readOnly )
			  {
					return;
			  }
			  foreach ( IndexReference index in AllIndexes )
			  {
					try
					{
						 index.Writer.commit();
					}
					catch ( IOException e )
					{
						 throw new Exception( "Unable to commit changes to " + index.Identifier, e );
					}
			  }
		 }

		 internal virtual void getReadLock()
		 {
			  @lock.readLock().@lock();
		 }

		 internal virtual void ReleaseReadLock()
		 {
			  @lock.readLock().unlock();
		 }

		 internal virtual void getWriteLock()
		 {
			  @lock.writeLock().@lock();
		 }

		 internal virtual void ReleaseWriteLock()
		 {
			  @lock.writeLock().unlock();
		 }

		 private static File GetFileDirectory( File storeDir, IndexEntityType type )
		 {
			  File path = new File( storeDir, "lucene" );
			  string extra = type.nameToLowerCase();
			  return new File( path, extra );
		 }

		 internal static File GetFileDirectory( File storeDir, IndexIdentifier identifier )
		 {
			  return new File( GetFileDirectory( storeDir, identifier.EntityType ), identifier.IndexName );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static org.apache.lucene.store.Directory getDirectory(java.io.File storeDir, IndexIdentifier identifier) throws java.io.IOException
		 internal static Directory GetDirectory( File storeDir, IndexIdentifier identifier )
		 {
			  return FSDirectory.open( GetFileDirectory( storeDir, identifier ).toPath() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static org.apache.lucene.search.TopFieldCollector scoringCollector(org.apache.lucene.search.Sort sorting, int n) throws java.io.IOException
		 internal static TopFieldCollector ScoringCollector( Sort sorting, int n )
		 {
			  return TopFieldCollector.create( sorting, n, false, true, false );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: IndexReference getIndexSearcher(IndexIdentifier identifier) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 internal virtual IndexReference GetIndexSearcher( IndexIdentifier identifier )
		 {
			  AssertNotClosed();
			  IndexReference searcher = _indexSearchers.get( identifier );
			  if ( searcher == null )
			  {
					return SyncGetIndexSearcher( identifier );
			  }
			  lock ( searcher )
			  {
					/*
					 * We need to get again a reference to the searcher because it might be so that
					 * it was refreshed while we waited. Once in here though no one will mess with
					 * our searcher
					 */
					searcher = _indexSearchers.get( identifier );
					if ( searcher == null || searcher.Closed )
					{
						 return SyncGetIndexSearcher( identifier );
					}
					searcher = RefreshSearcherIfNeeded( searcher );
					searcher.IncRef();
					return searcher;
			  }
		 }

		 private void AssertNotClosed()
		 {
			  if ( _closed )
			  {
					throw new System.InvalidOperationException( "Lucene index provider has been shut down" );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private synchronized IndexReference syncGetIndexSearcher(IndexIdentifier identifier) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 private IndexReference SyncGetIndexSearcher( IndexIdentifier identifier )
		 {
			 lock ( this )
			 {
				  try
				  {
						IndexReference indexReference = _indexSearchers.get( identifier );
						if ( indexReference == null )
						{
							 indexReference = _indexReferenceFactory.createIndexReference( identifier );
							 _indexSearchers.put( identifier, indexReference );
							 ConcurrentDictionary<string, DocValuesType> fieldTypes = new ConcurrentDictionary<string, DocValuesType>();
							 IndexSearcher searcher = indexReference.Searcher;
							 IList<LeafReaderContext> leaves = searcher.TopReaderContext.leaves();
							 foreach ( LeafReaderContext leafReaderContext in leaves )
							 {
								  foreach ( FieldInfo fieldInfo in leafReaderContext.reader().FieldInfos )
								  {
										fieldTypes[fieldInfo.name] = fieldInfo.DocValuesType;
								  }
							 }
							 _indexTypeMap[identifier] = fieldTypes;
						}
						else
						{
							 if ( !_readOnly )
							 {
								  lock ( indexReference )
								  {
										indexReference = RefreshSearcherIfNeeded( indexReference );
								  }
							 }
						}
						indexReference.IncRef();
						return indexReference;
				  }
				  catch ( IOException e )
				  {
						throw new UncheckedIOException( e );
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private IndexReference refreshSearcherIfNeeded(IndexReference searcher) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 private IndexReference RefreshSearcherIfNeeded( IndexReference searcher )
		 {
			  if ( searcher.CheckAndClearStale() )
			  {
					searcher = _indexReferenceFactory.refresh( searcher );
					if ( searcher != null )
					{
						 _indexSearchers.put( searcher.Identifier, searcher );
					}
			  }
			  return searcher;
		 }

		 internal virtual void InvalidateIndexSearcher( IndexIdentifier identifier )
		 {
			  IndexReference searcher = _indexSearchers.get( identifier );
			  if ( searcher != null )
			  {
					searcher.SetStale();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void deleteIndex(IndexIdentifier identifier, boolean recovery) throws java.io.IOException
		 internal virtual void DeleteIndex( IndexIdentifier identifier, bool recovery )
		 {
			  if ( _readOnly )
			  {
					throw new System.InvalidOperationException( "Index deletion in read only mode is not supported." );
			  }
			  CloseIndex( identifier );
			  FileUtils.deleteRecursively( GetFileDirectory( _baseStorePath, identifier ) );
			  _indexTypeMap.Remove( identifier );
			  bool removeFromIndexStore = !recovery || ( _indexStore.has( identifier.EntityType.entityClass(), identifier.IndexName ) );
			  if ( removeFromIndexStore )
			  {
					_indexStore.remove( identifier.EntityType.entityClass(), identifier.IndexName );
			  }
			  _typeCache.invalidate( identifier );
		 }

		 internal static Document FindDocument( IndexType type, IndexSearcher searcher, long IEntityId )
		 {
			  try
			  {
					TopDocs docs = searcher.search( type.IdTermQuery( IEntityId ), 1 );
					if ( docs.scoreDocs.length > 0 )
					{
						 return searcher.doc( docs.scoreDocs[0].doc );
					}
					return null;
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 internal static bool DocumentIsEmpty( Document document )
		 {
			  IList<IndexableField> fields = document.Fields;
			  foreach ( IndexableField field in fields )
			  {
					if ( LuceneExplicitIndex.IsValidKey( field.name() ) )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 private void CloseIndex( IndexIdentifier identifier )
		 {
			 lock ( this )
			 {
				  try
				  {
						IndexReference searcher = _indexSearchers.remove( identifier );
						if ( searcher != null )
						{
							 searcher.Dispose();
						}
				  }
				  catch ( IOException e )
				  {
						throw new Exception( "Unable to close lucene writer " + identifier, e );
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.graphdb.ResourceIterator<java.io.File> listWritableStoreFiles() throws java.io.IOException
		 private ResourceIterator<File> ListWritableStoreFiles()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Collection<java.io.File> files = new java.util.ArrayList<>();
			  ICollection<File> files = new List<File>();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Collection<org.Neo4Net.helpers.collection.Pair<org.apache.lucene.index.SnapshotDeletionPolicy,org.apache.lucene.index.IndexCommit>> snapshots = new java.util.ArrayList<>();
			  ICollection<Pair<SnapshotDeletionPolicy, IndexCommit>> snapshots = new List<Pair<SnapshotDeletionPolicy, IndexCommit>>();
			  MakeSureAllIndexesAreInstantiated();
			  foreach ( IndexReference index in AllIndexes )
			  {
					SnapshotDeletionPolicy deletionPolicy = ( SnapshotDeletionPolicy ) index.Writer.Config.IndexDeletionPolicy;
					File indexDirectory = GetFileDirectory( _baseStorePath, index.Identifier );
					IndexCommit commit;
					try
					{
						 // Throws IllegalStateException if no commits yet
						 commit = deletionPolicy.snapshot();
					}
					catch ( System.InvalidOperationException )
					{
						 /*
						  * This is insane but happens if we try to snapshot an existing index
						  * that has no commits. This is a bad API design - it should return null
						  * or something. This is not exceptional.
						  *
						  * For the time being we just do a commit and try again.
						  */
						 index.Writer.commit();
						 commit = deletionPolicy.snapshot();
					}

					foreach ( string fileName in commit.FileNames )
					{
						 Files.Add( new File( indexDirectory, fileName ) );
					}
					snapshots.Add( Pair.of( deletionPolicy, commit ) );
			  }
			  return new PrefetchingResourceIteratorAnonymousInnerClass( this, files, snapshots );
		 }

		 private class PrefetchingResourceIteratorAnonymousInnerClass : PrefetchingResourceIterator<File>
		 {
			 private readonly LuceneDataSource _outerInstance;

			 private ICollection<File> _files;
			 private ICollection<Pair<SnapshotDeletionPolicy, IndexCommit>> _snapshots;

			 public PrefetchingResourceIteratorAnonymousInnerClass( LuceneDataSource outerInstance, ICollection<File> files, ICollection<Pair<SnapshotDeletionPolicy, IndexCommit>> snapshots )
			 {
				 this.outerInstance = outerInstance;
				 this._files = files;
				 this._snapshots = snapshots;
				 filesIterator = Files.GetEnumerator();
			 }

			 private readonly IEnumerator<File> filesIterator;

			 protected internal override File fetchNextOrNull()
			 {
				  return filesIterator.hasNext() ? filesIterator.next() : null;
			 }

			 public override void close()
			 {
				  Exception exception = null;
				  foreach ( Pair<SnapshotDeletionPolicy, IndexCommit> policyAndCommit in _snapshots )
				  {
						try
						{
							 policyAndCommit.First().release(policyAndCommit.Other());
						}
						catch ( Exception e ) when ( e is IOException || e is Exception )
						{
							 if ( exception == null )
							 {
								  exception = e is IOException ? new UncheckedIOException( ( IOException ) e ) : ( Exception ) e;
							 }
							 else
							 {
								  exception.addSuppressed( e );
							 }
						}
				  }
				  if ( exception != null )
				  {
						throw exception;
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.graphdb.ResourceIterator<java.io.File> listReadOnlyStoreFiles() throws java.io.IOException
		 private ResourceIterator<File> ListReadOnlyStoreFiles()
		 {
			  // In read-only mode we don't need to take a snapshot, because the index will not be modified.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Collection<java.io.File> files = new java.util.ArrayList<>();
			  ICollection<File> files = new List<File>();
			  MakeSureAllIndexesAreInstantiated();
			  foreach ( IndexReference index in AllIndexes )
			  {
					File indexDirectory = GetFileDirectory( _baseStorePath, index.Identifier );
					IndexSearcher searcher = index.Searcher;
					using ( IndexReader indexReader = searcher.IndexReader )
					{
						 DirectoryReader directoryReader = ( DirectoryReader ) indexReader;
						 IndexCommit commit = directoryReader.IndexCommit;
						 foreach ( string fileName in commit.FileNames )
						 {
							  Files.Add( new File( indexDirectory, fileName ) );
						 }
					}
			  }
			  return Iterators.asResourceIterator( Files.GetEnumerator() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.Neo4Net.graphdb.ResourceIterator<java.io.File> listStoreFiles() throws java.io.IOException
		 internal virtual ResourceIterator<File> ListStoreFiles()
		 {
			  if ( _readOnly )
			  {
					return ListReadOnlyStoreFiles();
			  }
			  else
			  {
					return ListWritableStoreFiles();
			  }
		 }

		 private void MakeSureAllIndexesAreInstantiated()
		 {
			  foreach ( string name in _indexStore.getNames( typeof( Node ) ) )
			  {
					IDictionary<string, string> config = _indexStore.get( typeof( Node ), name );
					if ( config[Neo4Net.GraphDb.index.IndexManager_Fields.PROVIDER].Equals( LuceneIndexImplementation.SERVICE_NAME ) )
					{
						 EnsureInstantiated( new IndexIdentifier( IndexEntityType.Node, name ) );
					}
			  }
			  foreach ( string name in _indexStore.getNames( typeof( Relationship ) ) )
			  {
					IDictionary<string, string> config = _indexStore.get( typeof( Relationship ), name );
					if ( config[Neo4Net.GraphDb.index.IndexManager_Fields.PROVIDER].Equals( LuceneIndexImplementation.SERVICE_NAME ) )
					{
						 EnsureInstantiated( new IndexIdentifier( IndexEntityType.Relationship, name ) );
					}
			  }
		 }

		 private void EnsureInstantiated( IndexIdentifier identifier )
		 {
			  try
			  {
					IndexReference indexSearcher = GetIndexSearcher( identifier );
					indexSearcher.Close();
			  }
			  catch ( ExplicitIndexNotFoundKernelException )
			  {
					// Ignore supposedly concurrently dropped indexes.
			  }
		 }

		 private bool IsReadOnly( Config config, OperationalMode operationalMode )
		 {
			  return config.Get( GraphDatabaseSettings.read_only ) && ( OperationalMode.single == operationalMode );
		 }

		 internal abstract class LuceneFilesystemFacade
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           FS { Directory getDirectory(java.io.File baseStorePath, IndexIdentifier identifier) throws java.io.IOException { return org.apache.lucene.store.FSDirectory.open(getFileDirectory(baseStorePath, identifier).toPath()); } void cleanWriteLocks(java.io.File dir) { if(!dir.isDirectory()) { return; } for(java.io.File file : dir.listFiles()) { if(file.isDirectory()) { cleanWriteLocks(file); } else if(file.getName().equals("write.lock")) { boolean success = file.delete(); assert success; } } } File ensureDirectoryExists(org.Neo4Net.io.fs.FileSystemAbstraction fileSystem, java.io.File dir) { if(!dir.exists() && !dir.mkdirs()) { String message = String.format("Unable to create directory path[%s] for Neo4Net store" + ".", dir.getAbsolutePath()); throw new RuntimeException(message); } return dir; } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           MEMORY { Directory getDirectory(java.io.File baseStorePath, IndexIdentifier identifier) { return new org.apache.lucene.store.RAMDirectory(); } void cleanWriteLocks(java.io.File path) { } File ensureDirectoryExists(org.Neo4Net.io.fs.FileSystemAbstraction fileSystem, java.io.File path) { try { fileSystem.mkdirs(path); } catch(java.io.IOException e) { throw new RuntimeException(e); } return path; } };

			  private static readonly IList<LuceneFilesystemFacade> valueList = new List<LuceneFilesystemFacade>();

			  static LuceneFilesystemFacade()
			  {
				  valueList.Add( FS );
				  valueList.Add( MEMORY );
			  }

			  public enum InnerEnum
			  {
				  FS,
				  MEMORY
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private LuceneFilesystemFacade( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract org.apache.lucene.store.Directory getDirectory(java.io.File baseStorePath, IndexIdentifier identifier) throws java.io.IOException;
			  internal abstract org.apache.lucene.store.Directory getDirectory( java.io.File baseStorePath, IndexIdentifier identifier );

			  internal abstract java.io.File ensureDirectoryExists( Neo4Net.Io.fs.FileSystemAbstraction fileSystem, java.io.File path );

			  internal abstract void cleanWriteLocks( java.io.File path );

			 public static IList<LuceneFilesystemFacade> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static LuceneFilesystemFacade ValueOf( string name )
			 {
				 foreach ( LuceneFilesystemFacade enumInstance in LuceneFilesystemFacade.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }
	}

}