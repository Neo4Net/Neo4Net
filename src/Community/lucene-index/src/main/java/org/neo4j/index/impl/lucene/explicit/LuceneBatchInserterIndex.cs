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
namespace Neo4Net.Index.impl.lucene.@explicit
{
	using Document = org.apache.lucene.document.Document;
	using IndexWriter = Org.Apache.Lucene.Index.IndexWriter;
	using IndexWriterConfig = Org.Apache.Lucene.Index.IndexWriterConfig;
	using IndexableField = Org.Apache.Lucene.Index.IndexableField;
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;
	using Query = org.apache.lucene.search.Query;
	using SearcherFactory = org.apache.lucene.search.SearcherFactory;
	using SearcherManager = org.apache.lucene.search.SearcherManager;
	using Sort = org.apache.lucene.search.Sort;
	using TopDocs = org.apache.lucene.search.TopDocs;
	using Directory = org.apache.lucene.store.Directory;
	using LongSets = org.eclipse.collections.impl.factory.primitive.LongSets;


	using Neo4Net.Graphdb;
	using Neo4Net.Graphdb.index;
	using Neo4Net.Helpers.Collection;
	using ValueContext = Neo4Net.Index.lucene.ValueContext;
	using IOUtils = Neo4Net.Io.IOUtils;
	using ExplicitIndexHits = Neo4Net.Kernel.api.ExplicitIndexHits;
	using DocValuesCollector = Neo4Net.Kernel.Api.Impl.Index.collector.DocValuesCollector;
	using IndexEntityType = Neo4Net.Kernel.impl.index.IndexEntityType;
	using IoPrimitiveUtils = Neo4Net.Kernel.impl.util.IoPrimitiveUtils;
	using BatchInserterIndex = Neo4Net.@unsafe.Batchinsert.BatchInserterIndex;

	internal class LuceneBatchInserterIndex : BatchInserterIndex
	{
		 private readonly IndexIdentifier _identifier;
		 private readonly IndexType _type;

		 private IndexWriter _writer;
		 private SearcherManager _searcherManager;
		 private readonly bool _createdNow;
		 private IDictionary<string, LruCache<string, ICollection<EntityId>>> _cache;
		 private int _updateCount;
		 private readonly int _commitBatchSize = 500000;
		 private readonly RelationshipLookup _relationshipLookup;

		 internal interface RelationshipLookup
		 {
			  EntityId Lookup( long id );
		 }

		 internal LuceneBatchInserterIndex( File dbStoreDir, IndexIdentifier identifier, IDictionary<string, string> config, RelationshipLookup relationshipLookup )
		 {
			  File storeDir = GetStoreDir( dbStoreDir );
			  this._createdNow = !LuceneDataSource.GetFileDirectory( storeDir, identifier ).exists();
			  this._identifier = identifier;
			  this._type = IndexType.GetIndexType( config );
			  this._relationshipLookup = relationshipLookup;
			  this._writer = InstantiateWriter( storeDir );
			  this._searcherManager = InstantiateSearcherManager( _writer );
		 }

		 public override void Add( long id, IDictionary<string, object> properties )
		 {
			  try
			  {
					Document document = IndexType.NewDocument( EntityId( id ) );
					foreach ( KeyValuePair<string, object> entry in properties.SetOfKeyValuePairs() )
					{
						 string key = entry.Key;
						 object value = entry.Value;
						 AddSingleProperty( id, document, key, value );
					}
					_writer.addDocument( document );
					if ( ++_updateCount == _commitBatchSize )
					{
						 _writer.commit();
						 _updateCount = 0;
					}
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 private EntityId EntityId( long id )
		 {
			  if ( _identifier.entityType == IndexEntityType.Node )
			  {
					return new EntityId_IdData( id );
			  }

			  return _relationshipLookup.lookup( id );
		 }

		 private void AddSingleProperty( long entityId, Document document, string key, object value )
		 {
			  foreach ( object oneValue in IoPrimitiveUtils.asArray( value ) )
			  {
					bool isValueContext = oneValue is ValueContext;
					oneValue = isValueContext ? ( ( ValueContext ) oneValue ).CorrectValue : oneValue.ToString();
					_type.addToDocument( document, key, oneValue );
					if ( _createdNow )
					{
						 // If we know that the index was created this session
						 // then we can go ahead and add stuff to the cache directly
						 // when adding to the index.
						 AddToCache( entityId, key, oneValue );
					}
			  }
		 }

		 private void AddToCache( long entityId, string key, object value )
		 {
			  if ( this._cache == null )
			  {
					return;
			  }

			  string valueAsString = value.ToString();
			  LruCache<string, ICollection<EntityId>> cache = this._cache[key];
			  if ( cache != null )
			  {
					ICollection<EntityId> ids = cache.Get( valueAsString );
					if ( ids == null )
					{
						 ids = new HashSet<EntityId>();
						 cache.Put( valueAsString, ids );
					}
					ids.Add( new EntityId_IdData( entityId ) );
			  }
		 }

		 private void AddToCache( ICollection<EntityId> ids, string key, object value )
		 {
			  if ( this._cache == null )
			  {
					return;
			  }

			  string valueAsString = value.ToString();
			  LruCache<string, ICollection<EntityId>> cache = this._cache[key];
			  if ( cache != null )
			  {
					cache.Put( valueAsString, ids );
			  }
		 }

		 private ExplicitIndexHits GetFromCache( string key, object value )
		 {
			  if ( this._cache == null )
			  {
					return null;
			  }

			  string valueAsString = value.ToString();
			  LruCache<string, ICollection<EntityId>> cache = this._cache[key];
			  if ( cache != null )
			  {
					ICollection<EntityId> ids = cache.Get( valueAsString );
					if ( ids != null )
					{
						 return new ConstantScoreIterator( ids, Float.NaN );
					}
			  }
			  return null;
		 }

		 public override void UpdateOrAdd( long entityId, IDictionary<string, object> properties )
		 {
			  try
			  {
					RemoveFromCache( entityId );
					_writer.deleteDocuments( _type.idTermQuery( entityId ) );
					Add( entityId, properties );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void removeFromCache(long entityId) throws java.io.IOException
		 private void RemoveFromCache( long entityId )
		 {
			  IndexSearcher searcher = _searcherManager.acquire();
			  try
			  {
					Query query = _type.idTermQuery( entityId );
					TopDocs docs = searcher.search( query, 1 );
					if ( docs.totalHits > 0 )
					{
						 Document document = searcher.doc( docs.scoreDocs[0].doc );
						 foreach ( IndexableField field in document.Fields )
						 {
							  string key = field.name();
							  object value = field.stringValue();
							  RemoveFromCache( entityId, key, value );
						 }
					}
			  }
			  finally
			  {
					_searcherManager.release( searcher );
			  }
		 }

		 private void RemoveFromCache( long entityId, string key, object value )
		 {
			  if ( this._cache == null )
			  {
					return;
			  }

			  string valueAsString = value.ToString();
			  LruCache<string, ICollection<EntityId>> cache = this._cache[key];
			  if ( cache != null )
			  {
					ICollection<EntityId> ids = cache.Get( valueAsString );
					if ( ids != null )
					{
						 ids.remove( new EntityId_IdData( entityId ) );
					}
			  }
		 }

		 private IndexWriter InstantiateWriter( File folder )
		 {
			  Directory dir = null;
			  try
			  {
					dir = LuceneDataSource.GetDirectory( folder, _identifier );
					IndexWriterConfig writerConfig = new IndexWriterConfig( _type.analyzer );
					writerConfig.RAMBufferSizeMB = DetermineGoodBufferSize( writerConfig.RAMBufferSizeMB );
					return new IndexWriter( dir, writerConfig );
			  }
			  catch ( IOException e )
			  {
					IOUtils.closeAllSilently( dir );
					throw new Exception( e );
			  }
		 }

		 private double DetermineGoodBufferSize( double atLeast )
		 {
			  double heapHint = Runtime.Runtime.maxMemory() / (1024 * 1024 * 14);
			  double result = Math.Max( atLeast, heapHint );
			  return Math.Min( result, 700 );
		 }

		 private static SearcherManager InstantiateSearcherManager( IndexWriter writer )
		 {
			  try
			  {
					return new SearcherManager( writer, true, new SearcherFactory() );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 private void CloseSearcher()
		 {
			  try
			  {
					if ( _searcherManager != null )
					{
						 this._searcherManager.close();
					}
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
			  finally
			  {
					this._searcherManager = null;
			  }
		 }

		 private void CloseWriter()
		 {
			  try
			  {
					if ( this._writer != null )
					{
						 this._writer.close();
					}
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
			  finally
			  {
					this._writer = null;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.neo4j.graphdb.index.IndexHits<long> query(org.apache.lucene.search.Query query, final String key, final Object value)
		 private IndexHits<long> Query( Query query, string key, object value )
		 {
			  IndexSearcher searcher;
			  try
			  {
					searcher = _searcherManager.acquire();
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
			  try
			  {
					DocValuesCollector collector = new DocValuesCollector( true );
					searcher.search( query, collector );
					IndexHits<Document> result = collector.GetIndexHits( Sort.RELEVANCE );
					ExplicitIndexHits primitiveHits = null;
					if ( string.ReferenceEquals( key, null ) || this._cache == null || !this._cache.ContainsKey( key ) )
					{
						 primitiveHits = new DocToIdIterator( result, Collections.emptyList(), null, LongSets.immutable.empty() );
					}
					else
					{
						 primitiveHits = new DocToIdIteratorAnonymousInnerClass( this, result, Collections.emptyList(), LongSets.immutable.empty(), key, value );
					}
					return WrapIndexHits( primitiveHits );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
			  finally
			  {
					try
					{
						 _searcherManager.release( searcher );
					}
					catch ( IOException )
					{
					}
			  }
		 }

		 private class DocToIdIteratorAnonymousInnerClass : DocToIdIterator
		 {
			 private readonly LuceneBatchInserterIndex _outerInstance;

			 private string _key;
			 private object _value;

			 public DocToIdIteratorAnonymousInnerClass( LuceneBatchInserterIndex outerInstance, IndexHits<Document> result, UnknownType emptyList, UnknownType empty, string key, object value ) : base( result, emptyList, null, empty )
			 {
				 this.outerInstance = outerInstance;
				 this._key = key;
				 this._value = value;
				 ids = new List<>();
			 }

			 private readonly ICollection<EntityId> ids;

			 protected internal override bool fetchNext()
			 {
				  if ( base.fetchNext() )
				  {
						ids.add( new EntityId_IdData( next ) );
						return true;
				  }
				  outerInstance.addToCache( ids, _key, _value );
				  return false;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.neo4j.graphdb.index.IndexHits<long> wrapIndexHits(final org.neo4j.kernel.api.ExplicitIndexHits ids)
		 private IndexHits<long> WrapIndexHits( ExplicitIndexHits ids )
		 {
			  return new IndexHitsAnonymousInnerClass( this, ids );
		 }

		 private class IndexHitsAnonymousInnerClass : IndexHits<long>
		 {
			 private readonly LuceneBatchInserterIndex _outerInstance;

			 private ExplicitIndexHits _ids;

			 public IndexHitsAnonymousInnerClass( LuceneBatchInserterIndex outerInstance, ExplicitIndexHits ids )
			 {
				 this.outerInstance = outerInstance;
				 this._ids = ids;
			 }

			 public bool hasNext()
			 {
				  return _ids.hasNext();
			 }

			 public long? next()
			 {
				  return _ids.next();
			 }

			 public void remove()
			 {
				  throw new System.NotSupportedException();
			 }

			 public ResourceIterator<long> iterator()
			 {
				  return this;
			 }

			 public int size()
			 {
				  return _ids.size();
			 }

			 public void close()
			 {
				  _ids.close();
			 }

			 public long? Single
			 {
				 get
				 {
					  try
					  {
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final System.Nullable<long> result = ids.hasNext() ? ids.next() : null;
							long? result = _ids.hasNext() ? _ids.next() : null;
   
							if ( _ids.hasNext() )
							{
								 throw new NoSuchElementException( format( "More than one item in %s, first:%d, second:%d", _ids, result, _ids.next() ) );
							}
							return result;
					  }
					  finally
					  {
							close();
					  }
				 }
			 }

			 public float currentScore()
			 {
				  return 0;
			 }
		 }

		 public override IndexHits<long> Get( string key, object value )
		 {
			  ExplicitIndexHits cached = GetFromCache( key, value );
			  return cached != null ? WrapIndexHits( cached ) : Query( _type.get( key, value ), key, value );
		 }

		 public override IndexHits<long> Query( string key, object queryOrQueryObject )
		 {
			  return Query( _type.query( key, queryOrQueryObject, null ), null, null );
		 }

		 public override IndexHits<long> Query( object queryOrQueryObject )
		 {
			  return Query( _type.query( null, queryOrQueryObject, null ), null, null );
		 }

		 public virtual void Shutdown()
		 {
			  CloseSearcher();
			  CloseWriter();
		 }

		 private File GetStoreDir( File dbStoreDir )
		 {
			  File dir = new File( dbStoreDir, "index" );
			  if ( !dir.exists() && !dir.mkdirs() )
			  {
					throw new Exception( "Unable to create directory path[" + dir.AbsolutePath + "] for Neo4j store." );
			  }
			  return dir;
		 }

		 public override void Flush()
		 {
			  try
			  {
					_searcherManager.maybeRefreshBlocking();
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 public override void SetCacheCapacity( string key, int size )
		 {
			  if ( this._cache == null )
			  {
					this._cache = new Dictionary<string, LruCache<string, ICollection<EntityId>>>();
			  }
			  LruCache<string, ICollection<EntityId>> cache = this._cache[key];
			  if ( cache != null )
			  {
					cache.Resize( size );
			  }
			  else
			  {
					cache = new LruCache<string, ICollection<EntityId>>( "Batch inserter cache for " + key, size );
					this._cache[key] = cache;
			  }
		 }
	}

}