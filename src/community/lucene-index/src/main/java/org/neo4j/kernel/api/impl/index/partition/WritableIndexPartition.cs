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
namespace Neo4Net.Kernel.Api.Impl.Index.partition
{
	using IndexWriter = Org.Apache.Lucene.Index.IndexWriter;
	using IndexWriterConfig = Org.Apache.Lucene.Index.IndexWriterConfig;
	using SearcherFactory = org.apache.lucene.search.SearcherFactory;
	using SearcherManager = org.apache.lucene.search.SearcherManager;
	using Directory = org.apache.lucene.store.Directory;


	using Neo4Net.Graphdb;
	using IOUtils = Neo4Net.Io.IOUtils;
	using LuceneIndexSnapshots = Neo4Net.Kernel.Api.Impl.Index.backup.LuceneIndexSnapshots;

	/// <summary>
	/// Represents a single writable partition of a partitioned lucene index. </summary>
	/// <seealso cref= AbstractIndexPartition </seealso>
	public class WritableIndexPartition : AbstractIndexPartition
	{
		 private readonly IndexWriter _indexWriter;
		 private readonly SearcherManager _searcherManager;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public WritableIndexPartition(java.io.File partitionFolder, org.apache.lucene.store.Directory directory, org.apache.lucene.index.IndexWriterConfig writerConfig) throws java.io.IOException
		 public WritableIndexPartition( File partitionFolder, Directory directory, IndexWriterConfig writerConfig ) : base( partitionFolder, directory )
		 {
			  this._indexWriter = new IndexWriter( directory, writerConfig );
			  this._searcherManager = new SearcherManager( _indexWriter, new SearcherFactory() );
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
		 public override IndexWriter IndexWriter
		 {
			 get
			 {
				  return _indexWriter;
			 }
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PartitionSearcher acquireSearcher() throws java.io.IOException
		 public override PartitionSearcher AcquireSearcher()
		 {
			  return new PartitionSearcher( _searcherManager );
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void maybeRefreshBlocking() throws java.io.IOException
		 public override void MaybeRefreshBlocking()
		 {
			  _searcherManager.maybeRefreshBlocking();
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  IOUtils.closeAll( _searcherManager, _indexWriter, Directory );
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.graphdb.ResourceIterator<java.io.File> snapshot() throws java.io.IOException
		 public override ResourceIterator<File> Snapshot()
		 {
			  return LuceneIndexSnapshots.forIndex( PartitionFolder, _indexWriter );
		 }
	}

}