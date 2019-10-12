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
namespace Org.Neo4j.Kernel.Api.Impl.Index.partition
{
	using IndexWriter = Org.Apache.Lucene.Index.IndexWriter;
	using SearcherFactory = org.apache.lucene.search.SearcherFactory;
	using SearcherManager = org.apache.lucene.search.SearcherManager;
	using Directory = org.apache.lucene.store.Directory;


	using Org.Neo4j.Graphdb;
	using IOUtils = Org.Neo4j.Io.IOUtils;
	using LuceneIndexSnapshots = Org.Neo4j.Kernel.Api.Impl.Index.backup.LuceneIndexSnapshots;

	/// <summary>
	/// Represents a single read only partition of a partitioned lucene index.
	/// Read only partition do not support write to index and performs all read operations based on index opened in read
	/// only mode.
	/// </summary>
	public class ReadOnlyIndexPartition : AbstractIndexPartition
	{
		 private readonly SearcherManager _searcherManager;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ReadOnlyIndexPartition(java.io.File partitionFolder, org.apache.lucene.store.Directory directory) throws java.io.IOException
		 internal ReadOnlyIndexPartition( File partitionFolder, Directory directory ) : base( partitionFolder, directory )
		 {
			  this._searcherManager = new SearcherManager( directory, new SearcherFactory() );
		 }

		 public override IndexWriter IndexWriter
		 {
			 get
			 {
				  throw new System.NotSupportedException( "Retrieving index writer from read only index partition is " + "unsupported." );
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
		 ///  Refresh partition. No-op in read only partition.
		 /// </summary>
		 /// <exception cref="IOException"> if refreshing fails. </exception>
		 public override void MaybeRefreshBlocking()
		 {
			  // nothing to refresh in read only partition
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  IOUtils.closeAll( _searcherManager, DirectoryConflict );
		 }

		 /// <summary>
		 /// Retrieve list of consistent Lucene index files for read only partition.
		 /// </summary>
		 /// <returns> the iterator over index files. </returns>
		 /// <exception cref="IOException"> if any IO operation fails. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.graphdb.ResourceIterator<java.io.File> snapshot() throws java.io.IOException
		 public override ResourceIterator<File> Snapshot()
		 {
			  return LuceneIndexSnapshots.forIndex( PartitionFolder, DirectoryConflict );
		 }
	}


}