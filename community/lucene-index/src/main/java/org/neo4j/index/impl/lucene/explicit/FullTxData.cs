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
namespace Org.Neo4j.Index.impl.lucene.@explicit
{
	using Document = org.apache.lucene.document.Document;
	using Store = org.apache.lucene.document.Field.Store;
	using StoredField = org.apache.lucene.document.StoredField;
	using StringField = org.apache.lucene.document.StringField;
	using DirectoryReader = Org.Apache.Lucene.Index.DirectoryReader;
	using IndexReader = Org.Apache.Lucene.Index.IndexReader;
	using IndexWriter = Org.Apache.Lucene.Index.IndexWriter;
	using IndexWriterConfig = Org.Apache.Lucene.Index.IndexWriterConfig;
	using Term = Org.Apache.Lucene.Index.Term;
	using BooleanClause = org.apache.lucene.search.BooleanClause;
	using Occur = org.apache.lucene.search.BooleanClause.Occur;
	using BooleanQuery = org.apache.lucene.search.BooleanQuery;
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;
	using MatchAllDocsQuery = org.apache.lucene.search.MatchAllDocsQuery;
	using PrefixQuery = org.apache.lucene.search.PrefixQuery;
	using Query = org.apache.lucene.search.Query;
	using Sort = org.apache.lucene.search.Sort;
	using TermQuery = org.apache.lucene.search.TermQuery;
	using WildcardQuery = org.apache.lucene.search.WildcardQuery;
	using Directory = org.apache.lucene.store.Directory;
	using RAMDirectory = org.apache.lucene.store.RAMDirectory;
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using MutableLongObjectMap = org.eclipse.collections.api.map.primitive.MutableLongObjectMap;
	using LongObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.LongObjectHashMap;


	using QueryContext = Org.Neo4j.Index.lucene.QueryContext;
	using DocValuesCollector = Org.Neo4j.Kernel.Api.Impl.Index.collector.DocValuesCollector;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.impl.lucene.@explicit.LuceneExplicitIndex.KEY_DOC_ID;

	internal class FullTxData : TxData
	{
		 /*
		  * The concept of orphan exists to find entities when querying where the transaction state
		  * (i.e. a FullTxData object) has seen removed entities w/o key and potentially also w/o value.
		  * A TxData instance receiving "add" calls with null key/value is an instance used to track removals.
		  * A Lucene document storing state about e.g. {@code index.remove( myNode, "name" )}
		  * <pre>
		  * {
		  *     __all__: "name"
		  * }
		  * </pre>
		  *
		  * A Lucene document storing state about e.g. {@code index.remove( myNode )}
		  * <pre>
		  * {
		  *     __all__: "1"
		  * }
		  * where queries would (if there are any orphans at all stored) include the "all orphans" value ("1") as
		  * well as any specific key which is pulled out from the incoming query.
		  */
		 private const string ORPHANS_KEY = "__all__";
		 /// <summary>
		 /// When querying we need to distinguish between documents coming from the store and documents
		 /// coming from transaction state. A field with this key is put on all documents in transaction state.
		 /// </summary>
		 public const string TX_STATE_KEY = "__tx_state__";
		 private static readonly sbyte[] _txStateValue = new sbyte[] { 1 };
		 private const string ORPHANS_VALUE = "1";

		 private Directory _directory;
		 private IndexWriter _writer;
		 private bool _modified;
		 private IndexReader _reader;
		 private IndexSearcher _searcher;
		 private readonly MutableLongObjectMap<Document> _cachedDocuments = new LongObjectHashMap<Document>();
		 private ISet<string> _orphans;

		 internal FullTxData( LuceneExplicitIndex index ) : base( index )
		 {
		 }

		 internal override void Add( TxDataHolder holder, EntityId entityId, string key, object value )
		 {
			  try
			  {
					EnsureLuceneDataInstantiated();
					long id = entityId.Id();
					Document document = FindDocument( id );
					bool add = false;
					if ( document == null )
					{
						 document = IndexType.NewDocument( entityId );
						 document.add( new StoredField( TX_STATE_KEY, _txStateValue ) );
						 _cachedDocuments.put( id, document );
						 add = true;
					}

					if ( string.ReferenceEquals( key, null ) && value == null )
					{
						 // Set a special "always hit" flag
						 document.add( new StringField( ORPHANS_KEY, ORPHANS_VALUE, Store.NO ) );
						 AddOrphan( null );
					}
					else if ( value == null )
					{
						 // Set a special "always hit" flag
						 document.add( new StringField( ORPHANS_KEY, key, Store.NO ) );
						 AddOrphan( key );
					}
					else
					{
						 Index.type.addToDocument( document, key, value );
					}

					if ( add )
					{
						 _writer.addDocument( document );
					}
					else
					{
						 _writer.updateDocument( Index.type.idTerm( id ), document );
					}
					InvalidateSearcher();
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 private void AddOrphan( string key )
		 {
			  if ( _orphans == null )
			  {
					_orphans = new HashSet<string>();
			  }
			  _orphans.Add( key );
		 }

		 private Document FindDocument( long id )
		 {
			  return _cachedDocuments.get( id );
		 }

		 private void EnsureLuceneDataInstantiated()
		 {
			  if ( this._directory == null )
			  {
					try
					{
						 this._directory = new RAMDirectory();
						 IndexWriterConfig writerConfig = new IndexWriterConfig( Index.type.analyzer );
						 this._writer = new IndexWriter( _directory, writerConfig );
					}
					catch ( IOException e )
					{
						 throw new Exception( e );
					}
			  }
		 }

		 internal override void Remove( TxDataHolder holder, EntityId entityId, string key, object value )
		 {
			  try
			  {
					EnsureLuceneDataInstantiated();
					long id = entityId.Id();
					Document document = FindDocument( id );
					if ( document != null )
					{
						 Index.type.removeFromDocument( document, key, value );
						 if ( LuceneDataSource.DocumentIsEmpty( document ) )
						 {
							  _writer.deleteDocuments( Index.type.idTerm( id ) );
						 }
						 else
						 {
							  _writer.updateDocument( Index.type.idTerm( id ), document );
						 }
					}
					InvalidateSearcher();
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 internal override ICollection<EntityId> Query( TxDataHolder holder, Query query, QueryContext contextOrNull )
		 {
			  return InternalQuery( query, contextOrNull );
		 }

		 private ICollection<EntityId> InternalQuery( Query query, QueryContext contextOrNull )
		 {
			  if ( this._directory == null )
			  {
					return Collections.emptySet();
			  }

			  try
			  {
					Sort sorting = contextOrNull != null ? contextOrNull.Sorting : null;
					bool prioritizeCorrectness = contextOrNull == null || !contextOrNull.TradeCorrectnessForSpeed;
					IndexSearcher theSearcher = Searcher( prioritizeCorrectness );
					query = IncludeOrphans( query );
					DocValuesCollector docValuesCollector = new DocValuesCollector( prioritizeCorrectness );
					theSearcher.search( query, docValuesCollector );
					ICollection<EntityId> result = new List<EntityId>();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.iterator.LongIterator valuesIterator = docValuesCollector.getSortedValuesIterator(KEY_DOC_ID, sorting);
					LongIterator valuesIterator = docValuesCollector.GetSortedValuesIterator( KEY_DOC_ID, sorting );
					while ( valuesIterator.hasNext() )
					{
						 result.Add( new EntityId_IdData( valuesIterator.next() ) );
					}
					return result;
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 private Query IncludeOrphans( Query query )
		 {
			  if ( _orphans == null )
			  {
					return query;
			  }

			  return ( new BooleanQuery.Builder() ).add(InjectOrphans(query), BooleanClause.Occur.SHOULD).add(new TermQuery(new Term(ORPHANS_KEY, ORPHANS_VALUE)), BooleanClause.Occur.SHOULD).build();
		 }

		 private Query InjectOrphans( Query query )
		 {
			  if ( query is BooleanQuery )
			  {
					BooleanQuery source = ( BooleanQuery ) query;
					BooleanQuery.Builder builder = new BooleanQuery.Builder();
					foreach ( BooleanClause clause in source.clauses() )
					{
						 builder.add( InjectOrphans( clause.Query ), clause.Occur );
					}
					return builder.build();
			  }

			  string orphanField = ExtractTermField( query );
			  if ( string.ReferenceEquals( orphanField, null ) )
			  {
					return query;
			  }

			  return ( new BooleanQuery.Builder() ).add(query, BooleanClause.Occur.SHOULD).add(new TermQuery(new Term(ORPHANS_KEY, orphanField)), BooleanClause.Occur.SHOULD).build();
		 }

		 private string ExtractTermField( Query query )
		 {
			  // Try common types of queries
			  if ( query is TermQuery )
			  {
					return ( ( TermQuery )query ).Term.field();
			  }
			  else if ( query is WildcardQuery )
			  {
					return ( ( WildcardQuery )query ).Term.field();
			  }
			  else if ( query is PrefixQuery )
			  {
					return ( ( PrefixQuery )query ).Prefix.field();
			  }
			  else if ( query is MatchAllDocsQuery )
			  {
					return null;
			  }

			  // Try to extract terms and get it that way
			  string field = GetFieldFromExtractTerms( query );
			  if ( !string.ReferenceEquals( field, null ) )
			  {
					return field;
			  }

			  // Last resort: since Query doesn't have a common interface for getting
			  // the term/field of its query this is one option.
			  return GetFieldViaReflection( query );
		 }

		 private string GetFieldViaReflection( Query query )
		 {
			  try
			  {
					try
					{
						 Term term = ( Term ) query.GetType().GetMethod("getTerm").invoke(query);
						 return term.field();
					}
					catch ( NoSuchMethodException )
					{
						 return ( string ) query.GetType().GetMethod("getField").invoke(query);
					}
			  }
			  catch ( Exception e )
			  {
					throw new Exception( e );
			  }
		 }

		 private string GetFieldFromExtractTerms( Query query )
		 {
			  ISet<Term> terms = new HashSet<Term>();
			  try
			  {
					_searcher.createNormalizedWeight( query, false ).extractTerms( terms );
			  }
			  catch ( IOException ioe )
			  {
					throw new System.NotSupportedException( ioe );
			  }
			  catch ( System.NotSupportedException )
			  {
					// TODO This is for "*" queries and such. Lucene doesn't seem
					// to be able/willing to rewrite such queries.
					// Just ignore the orphans then... OK?
			  }
			  return terms.Count == 0 ? null : terms.GetEnumerator().next().field();
		 }

		 internal override void Close()
		 {
			  LuceneUtil.Close( this._writer );
			  LuceneUtil.Close( this._reader );
			  LuceneUtil.Close( this._searcher );
		 }

		 private void InvalidateSearcher()
		 {
			  this._modified = true;
		 }

		 private IndexSearcher Searcher( bool allowRefreshSearcher )
		 {
			  if ( this._searcher != null && ( !_modified || !allowRefreshSearcher ) )
			  {
					return this._searcher;
			  }

			  try
			  {
					IndexReader newReader = this._reader == null ? DirectoryReader.open( this._writer ) : DirectoryReader.openIfChanged( ( DirectoryReader ) this._reader );
					if ( newReader == null )
					{
						 return this._searcher;
					}
					LuceneUtil.Close( _reader );
					this._reader = newReader;
					LuceneUtil.Close( _searcher );
					_searcher = new IndexSearcher( _reader );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
			  finally
			  {
					if ( allowRefreshSearcher )
					{
						 this._modified = false;
					}
			  }
			  return this._searcher;
		 }

		 internal override IndexSearcher AsSearcher( TxDataHolder holder, QueryContext context )
		 {
			  bool refresh = context == null || !context.TradeCorrectnessForSpeed;
			  return Searcher( refresh );
		 }

		 internal override ICollection<EntityId> Get( TxDataHolder holder, string key, object value )
		 {
			  return InternalQuery( Index.type.get( key, value ), null );
		 }

		 internal override ICollection<EntityId> GetOrphans( string key )
		 {
			  return emptyList();
		 }
	}

}