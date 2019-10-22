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
namespace Neo4Net.Index.impl.lucene.@explicit
{
	using Document = org.apache.lucene.document.Document;
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;
	using Query = org.apache.lucene.search.Query;
	using ScoreDoc = org.apache.lucene.search.ScoreDoc;
	using Sort = org.apache.lucene.search.Sort;
	using TopDocs = org.apache.lucene.search.TopDocs;
	using TopFieldCollector = org.apache.lucene.search.TopFieldCollector;


	using Neo4Net.Helpers.Collections;
	using QueryContext = Neo4Net.Index.lucene.QueryContext;
	using Neo4Net.Kernel.Impl.Api.explicitindex;

	internal class TopDocsIterator : AbstractIndexHits<Document>
	{
		 private readonly IEnumerator<ScoreDoc> _iterator;
		 private ScoreDoc _currentDoc;
		 private readonly int _size;
		 private readonly IndexSearcher _searcher;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: TopDocsIterator(org.apache.lucene.search.Query query, org.Neo4Net.index.lucene.QueryContext context, org.apache.lucene.search.IndexSearcher searcher) throws java.io.IOException
		 internal TopDocsIterator( Query query, QueryContext context, IndexSearcher searcher )
		 {
			  TopDocs docs = ToTopDocs( query, context, searcher );
			  this._size = docs.scoreDocs.length;
			  this._iterator = new ArrayIterator<ScoreDoc>( docs.scoreDocs );
			  this._searcher = searcher;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.apache.lucene.search.TopDocs toTopDocs(org.apache.lucene.search.Query query, org.Neo4Net.index.lucene.QueryContext context, org.apache.lucene.search.IndexSearcher searcher) throws java.io.IOException
		 private TopDocs ToTopDocs( Query query, QueryContext context, IndexSearcher searcher )
		 {
			  Sort sorting = context != null ? context.Sorting : null;
			  TopDocs topDocs;
			  if ( sorting == null && context != null )
			  {
					topDocs = searcher.search( query, context.Top );
			  }
			  else
			  {
					if ( context == null || !context.TradeCorrectnessForSpeed )
					{
						 TopFieldCollector collector = LuceneDataSource.ScoringCollector( sorting, context.Top );
						 searcher.search( query, collector );
						 topDocs = collector.topDocs();
					}
					else
					{
						 topDocs = searcher.search( query, null, context.Top, sorting );
					}
			  }
			  return topDocs;
		 }

		 protected internal override Document FetchNextOrNull()
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  if ( !_iterator.hasNext() )
			  {
					return null;
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  _currentDoc = _iterator.next();
			  try
			  {
					return _searcher.doc( _currentDoc.doc );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 public override float CurrentScore()
		 {
			  return _currentDoc.score;
		 }

		 public override int Size()
		 {
			  return this._size;
		 }
	}

}