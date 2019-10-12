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
namespace Org.Neo4j.Kernel.Api.Impl.Index
{
	using Document = org.apache.lucene.document.Document;
	using IndexReader = Org.Apache.Lucene.Index.IndexReader;
	using MultiFields = Org.Apache.Lucene.Index.MultiFields;
	using DocIdSetIterator = org.apache.lucene.search.DocIdSetIterator;
	using FilteredDocIdSetIterator = org.apache.lucene.search.FilteredDocIdSetIterator;
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;
	using Bits = org.apache.lucene.util.Bits;


	using Org.Neo4j.Helpers.Collection;
	using Org.Neo4j.Helpers.Collection;
	using PartitionSearcher = Org.Neo4j.Kernel.Api.Impl.Index.partition.PartitionSearcher;

	/// <summary>
	/// Provides a view of all <seealso cref="Document"/>s in a single partition.
	/// </summary>
	public class LucenePartitionAllDocumentsReader : BoundedIterable<Document>
	{
		 private readonly PartitionSearcher _partitionSearcher;
		 private readonly IndexSearcher _searcher;
		 private readonly IndexReader _reader;

		 public LucenePartitionAllDocumentsReader( PartitionSearcher partitionSearcher )
		 {
			  this._partitionSearcher = partitionSearcher;
			  this._searcher = partitionSearcher.IndexSearcher;
			  this._reader = _searcher.IndexReader;
		 }

		 public override long MaxCount()
		 {
			  return _reader.maxDoc();
		 }

		 public override IEnumerator<Document> Iterator()
		 {
			  return new PrefetchingIteratorAnonymousInnerClass( this );
		 }

		 private class PrefetchingIteratorAnonymousInnerClass : PrefetchingIterator<Document>
		 {
			 private readonly LucenePartitionAllDocumentsReader _outerInstance;

			 public PrefetchingIteratorAnonymousInnerClass( LucenePartitionAllDocumentsReader outerInstance )
			 {
				 this.outerInstance = outerInstance;
				 idIterator = outerInstance.iterateAllDocs();
			 }

			 internal DocIdSetIterator idIterator;

			 protected internal override Document fetchNextOrNull()
			 {
				  try
				  {
						int doc = idIterator.nextDoc();
						if ( doc == DocIdSetIterator.NO_MORE_DOCS )
						{
							 return null;
						}
						return outerInstance.getDocument( doc );
				  }
				  catch ( IOException e )
				  {
						throw new LuceneDocumentRetrievalException( "Can't fetch document id from lucene index.", e );
				  }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _partitionSearcher.Dispose();
		 }

		 private Document GetDocument( int docId )
		 {
			  try
			  {
					return _searcher.doc( docId );
			  }
			  catch ( IOException e )
			  {
					throw new LuceneDocumentRetrievalException( "Can't retrieve document with id: " + docId + ".", docId, e );
			  }
		 }

		 private DocIdSetIterator IterateAllDocs()
		 {
			  Bits liveDocs = MultiFields.getLiveDocs( _reader );
			  DocIdSetIterator allDocs = DocIdSetIterator.all( _reader.maxDoc() );
			  if ( liveDocs == null )
			  {
					return allDocs;
			  }

			  return new FilteredDocIdSetIteratorAnonymousInnerClass( this, allDocs, liveDocs );
		 }

		 private class FilteredDocIdSetIteratorAnonymousInnerClass : FilteredDocIdSetIterator
		 {
			 private readonly LucenePartitionAllDocumentsReader _outerInstance;

			 private Bits _liveDocs;

			 public FilteredDocIdSetIteratorAnonymousInnerClass( LucenePartitionAllDocumentsReader outerInstance, DocIdSetIterator allDocs, Bits liveDocs ) : base( allDocs )
			 {
				 this.outerInstance = outerInstance;
				 this._liveDocs = liveDocs;
			 }

			 protected internal override bool match( int doc )
			 {
				  return _liveDocs.get( doc );
			 }
		 }
	}

}