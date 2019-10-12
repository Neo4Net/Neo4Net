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
namespace Org.Neo4j.Kernel.Api.Impl.Schema.writer
{
	using Document = org.apache.lucene.document.Document;
	using IndexWriter = Org.Apache.Lucene.Index.IndexWriter;
	using Term = Org.Apache.Lucene.Index.Term;
	using Query = org.apache.lucene.search.Query;


	using Org.Neo4j.Kernel.Api.Impl.Index;
	using AbstractIndexPartition = Org.Neo4j.Kernel.Api.Impl.Index.partition.AbstractIndexPartition;

	/// <summary>
	/// Schema Lucene index writer implementation that supports writing into multiple partitions and creates partitions
	/// on-demand if needed.
	/// <para>
	/// Writer threats partition as writable if partition has number of live and deleted documents that is less then configured
	/// <seealso cref="MAXIMUM_PARTITION_SIZE"/>.
	/// First observable partition that satisfy writer criteria is used for writing.
	/// </para>
	/// </summary>
	public class PartitionedIndexWriter : LuceneIndexWriter
	{
		 private readonly WritableAbstractDatabaseIndex _index;

		 // by default we still keep a spare of 10% to the maximum partition size: During concurrent updates
		 // it could happen that 2 threads reserve space in a partition (without claiming it by doing addDocument):
		 private readonly int? _maximumPartitionSize = Integer.getInteger( "luceneSchemaIndex.maxPartitionSize", IndexWriter.MAX_DOCS - ( IndexWriter.MAX_DOCS / 10 ) );

		 public PartitionedIndexWriter( WritableAbstractDatabaseIndex index )
		 {
			  this._index = index;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void addDocument(org.apache.lucene.document.Document doc) throws java.io.IOException
		 public override void AddDocument( Document doc )
		 {
			  GetIndexWriter( 1 ).addDocument( doc );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void addDocuments(int numDocs, Iterable<org.apache.lucene.document.Document> documents) throws java.io.IOException
		 public override void AddDocuments( int numDocs, IEnumerable<Document> documents )
		 {
			  GetIndexWriter( numDocs ).addDocuments( documents );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void updateDocument(org.apache.lucene.index.Term term, org.apache.lucene.document.Document doc) throws java.io.IOException
		 public override void UpdateDocument( Term term, Document doc )
		 {
			  IList<AbstractIndexPartition> partitions = _index.Partitions;
			  if ( _index.hasSinglePartition( partitions ) && WritablePartition( _index.getFirstPartition( partitions ), 1 ) )
			  {
					_index.getFirstPartition( partitions ).IndexWriter.updateDocument( term, doc );
			  }
			  else
			  {
					DeleteDocuments( term );
					AddDocument( doc );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void deleteDocuments(org.apache.lucene.search.Query query) throws java.io.IOException
		 public override void DeleteDocuments( Query query )
		 {
			  IList<AbstractIndexPartition> partitions = _index.Partitions;
			  foreach ( AbstractIndexPartition partition in partitions )
			  {
					partition.IndexWriter.deleteDocuments( query );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void deleteDocuments(org.apache.lucene.index.Term term) throws java.io.IOException
		 public override void DeleteDocuments( Term term )
		 {
			  IList<AbstractIndexPartition> partitions = _index.Partitions;
			  foreach ( AbstractIndexPartition partition in partitions )
			  {
					partition.IndexWriter.deleteDocuments( term );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.apache.lucene.index.IndexWriter getIndexWriter(int numDocs) throws java.io.IOException
		 private IndexWriter GetIndexWriter( int numDocs )
		 {
			  lock ( _index )
			  {
					// We synchronise on the index to coordinate with all writers about how many partitions we
					// have, and when new ones are created. The discovery that a new partition needs to be added,
					// and the call to index.addNewPartition() must be atomic.
					return UnsafeGetIndexWriter( numDocs );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.apache.lucene.index.IndexWriter unsafeGetIndexWriter(int numDocs) throws java.io.IOException
		 private IndexWriter UnsafeGetIndexWriter( int numDocs )
		 {
			  IList<AbstractIndexPartition> indexPartitions = _index.Partitions;
			  int size = indexPartitions.Count;
			  //noinspection ForLoopReplaceableByForEach
			  for ( int i = 0; i < size; i++ )
			  {
					// We should find the *first* writable partition, so we can fill holes left by index deletes,
					// after they were merged away:
					AbstractIndexPartition partition = indexPartitions[i];
					if ( WritablePartition( partition, numDocs ) )
					{
						 return partition.IndexWriter;
					}
			  }
			  AbstractIndexPartition indexPartition = _index.addNewPartition();
			  return indexPartition.IndexWriter;
		 }

		 private bool WritablePartition( AbstractIndexPartition partition, int numDocs )
		 {
			  return _maximumPartitionSize - partition.IndexWriter.maxDoc() >= numDocs;
		 }
	}


}