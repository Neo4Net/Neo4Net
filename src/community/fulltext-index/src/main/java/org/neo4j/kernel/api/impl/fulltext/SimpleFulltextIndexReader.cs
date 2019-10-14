using System;

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
	using Analyzer = org.apache.lucene.analysis.Analyzer;
	using MultiFieldQueryParser = org.apache.lucene.queryparser.classic.MultiFieldQueryParser;
	using ParseException = org.apache.lucene.queryparser.classic.ParseException;
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;
	using Query = org.apache.lucene.search.Query;
	using Sort = org.apache.lucene.search.Sort;
	using TotalHitCountCollector = org.apache.lucene.search.TotalHitCountCollector;

	using DocValuesCollector = Neo4Net.Kernel.Api.Impl.Index.collector.DocValuesCollector;
	using ValuesIterator = Neo4Net.Kernel.Api.Impl.Index.collector.ValuesIterator;
	using IndexReaderCloseException = Neo4Net.Kernel.Api.Impl.Schema.reader.IndexReaderCloseException;
	using TokenHolder = Neo4Net.Kernel.impl.core.TokenHolder;
	using Value = Neo4Net.Values.Storable.Value;

	/// <summary>
	/// Lucene index reader that is able to read/sample a single partition of a partitioned Lucene index.
	/// </summary>
	/// <seealso cref= PartitionedFulltextIndexReader </seealso>
	internal class SimpleFulltextIndexReader : FulltextIndexReader
	{
		 private readonly SearcherReference _searcherRef;
		 private readonly Analyzer _analyzer;
		 private readonly TokenHolder _propertyKeyTokenHolder;
		 private readonly string[] _properties;

		 internal SimpleFulltextIndexReader( SearcherReference searcherRef, string[] properties, Analyzer analyzer, TokenHolder propertyKeyTokenHolder )
		 {
			  this._searcherRef = searcherRef;
			  this._properties = properties;
			  this._analyzer = analyzer;
			  this._propertyKeyTokenHolder = propertyKeyTokenHolder;
		 }

		 public override void Close()
		 {
			  try
			  {
					_searcherRef.close();
			  }
			  catch ( IOException e )
			  {
					throw new IndexReaderCloseException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public ScoreEntityIterator query(String queryString) throws org.apache.lucene.queryparser.classic.ParseException
		 public override ScoreEntityIterator Query( string queryString )
		 {
			  MultiFieldQueryParser multiFieldQueryParser = new MultiFieldQueryParser( _properties, _analyzer );
			  multiFieldQueryParser.AllowLeadingWildcard = true;
			  Query query = multiFieldQueryParser.parse( queryString );
			  return IndexQuery( query );
		 }

		 private ScoreEntityIterator IndexQuery( Query query )
		 {
			  try
			  {
					DocValuesCollector docValuesCollector = new DocValuesCollector( true );
					IndexSearcher.search( query, docValuesCollector );
					ValuesIterator sortedValuesIterator = docValuesCollector.GetSortedValuesIterator( LuceneFulltextDocumentStructure.FIELD_ENTITY_ID, Sort.RELEVANCE );
					return new ScoreEntityIterator( sortedValuesIterator );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 private IndexSearcher IndexSearcher
		 {
			 get
			 {
				  return _searcherRef.IndexSearcher;
			 }
		 }

		 public override long CountIndexedNodes( long nodeId, int[] propertyKeyIds, params Value[] propertyValues )
		 {
			  try
			  {
					string[] propertyKeys = new string[propertyKeyIds.Length];
					for ( int i = 0; i < propertyKeyIds.Length; i++ )
					{
						 propertyKeys[i] = _propertyKeyTokenHolder.getTokenById( propertyKeyIds[i] ).name();
					}
					Query query = LuceneFulltextDocumentStructure.NewCountNodeEntriesQuery( nodeId, propertyKeys, propertyValues );
					TotalHitCountCollector collector = new TotalHitCountCollector();
					IndexSearcher.search( query, collector );
					return collector.TotalHits;
			  }
			  catch ( Exception e )
			  {
					throw new Exception( e );
			  }
		 }
	}

}