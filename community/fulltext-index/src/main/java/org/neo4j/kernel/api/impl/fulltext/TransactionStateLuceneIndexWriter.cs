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
namespace Org.Neo4j.Kernel.Api.Impl.Fulltext
{
	using Document = org.apache.lucene.document.Document;
	using DirectoryReader = Org.Apache.Lucene.Index.DirectoryReader;
	using IndexWriter = Org.Apache.Lucene.Index.IndexWriter;
	using Term = Org.Apache.Lucene.Index.Term;
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;
	using Query = org.apache.lucene.search.Query;
	using Directory = org.apache.lucene.store.Directory;
	using RAMDirectory = org.apache.lucene.store.RAMDirectory;


	using IOUtils = Org.Neo4j.Io.IOUtils;
	using IndexWriterConfigs = Org.Neo4j.Kernel.Api.Impl.Index.IndexWriterConfigs;
	using LuceneIndexWriter = Org.Neo4j.Kernel.Api.Impl.Schema.writer.LuceneIndexWriter;

	public class TransactionStateLuceneIndexWriter : LuceneIndexWriter, System.IDisposable
	{
		 private readonly LuceneFulltextIndex _index;
		 private IndexWriter _writer;
		 private readonly Directory _directory;

		 internal TransactionStateLuceneIndexWriter( LuceneFulltextIndex index )
		 {
			  this._index = index;
			  _directory = new RAMDirectory();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void addDocument(org.apache.lucene.document.Document document) throws java.io.IOException
		 public override void AddDocument( Document document )
		 {
			  _writer.addDocument( document );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void addDocuments(int numDocs, Iterable<org.apache.lucene.document.Document> document) throws java.io.IOException
		 public override void AddDocuments( int numDocs, IEnumerable<Document> document )
		 {
			  _writer.addDocuments( document );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void updateDocument(org.apache.lucene.index.Term term, org.apache.lucene.document.Document document) throws java.io.IOException
		 public override void UpdateDocument( Term term, Document document )
		 {
			  _writer.updateDocument( term, document );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void deleteDocuments(org.apache.lucene.index.Term term) throws java.io.IOException
		 public override void DeleteDocuments( Term term )
		 {
			  _writer.deleteDocuments( term );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void deleteDocuments(org.apache.lucene.search.Query query) throws java.io.IOException
		 public override void DeleteDocuments( Query query )
		 {
			  _writer.deleteDocuments( query );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void resetWriterState() throws java.io.IOException
		 internal virtual void ResetWriterState()
		 {
			  if ( _writer != null )
			  {
					// Note that 'rollback' closes the writer.
					_writer.rollback();
			  }
			  OpenWriter();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void openWriter() throws java.io.IOException
		 private void OpenWriter()
		 {
			  _writer = new IndexWriter( _directory, IndexWriterConfigs.transactionState( _index.Analyzer ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: FulltextIndexReader getNearRealTimeReader() throws java.io.IOException
		 internal virtual FulltextIndexReader NearRealTimeReader
		 {
			 get
			 {
				  DirectoryReader directoryReader = DirectoryReader.open( _writer, true );
				  IndexSearcher searcher = new IndexSearcher( directoryReader );
				  SearcherReference searcherRef = new DirectSearcherReference( searcher, directoryReader );
				  return new SimpleFulltextIndexReader( searcherRef, _index.PropertiesArray, _index.Analyzer, _index.PropertyKeyTokenHolder );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  IOUtils.closeAll( _writer, _directory );
		 }
	}

}