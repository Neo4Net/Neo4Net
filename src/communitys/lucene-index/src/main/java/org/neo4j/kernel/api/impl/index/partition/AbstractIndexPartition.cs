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
	using SearcherManager = org.apache.lucene.search.SearcherManager;
	using Directory = org.apache.lucene.store.Directory;


	using Neo4Net.Graphdb;

	/// <summary>
	/// Represents a single partition of a partitioned lucene index. Each partition is a separate Lucene index.
	/// Contains and manages lifecycle of the corresponding <seealso cref="Directory"/>, <seealso cref="IndexWriter writer"/> and
	/// <seealso cref="SearcherManager"/>.
	/// </summary>
	public abstract class AbstractIndexPartition : System.IDisposable
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly Directory DirectoryConflict;
		 protected internal readonly File PartitionFolder;

		 public AbstractIndexPartition( File partitionFolder, Directory directory )
		 {
			  this.PartitionFolder = partitionFolder;
			  this.DirectoryConflict = directory;
		 }

		 /// <summary>
		 /// Retrieve index partition directory </summary>
		 /// <returns> partition directory </returns>
		 public virtual Directory Directory
		 {
			 get
			 {
				  return DirectoryConflict;
			 }
		 }

		 /// <summary>
		 /// Retrieve index partition writer </summary>
		 /// <returns> partition writer </returns>
		 public abstract IndexWriter IndexWriter { get; }

		 /// <summary>
		 /// Return searcher for requested partition.
		 /// There is no tracking of acquired searchers, so the expectation is that callers will call close on acquired
		 /// searchers to release resources.
		 /// </summary>
		 /// <returns> partition searcher </returns>
		 /// <exception cref="IOException"> if exception happened during searcher acquisition </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract PartitionSearcher acquireSearcher() throws java.io.IOException;
		 public abstract PartitionSearcher AcquireSearcher();

		 /// <summary>
		 /// Refresh partition to make newly inserted data visible for readers.
		 /// </summary>
		 /// <exception cref="IOException"> if refreshing fails. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract void maybeRefreshBlocking() throws java.io.IOException;
		 public abstract void MaybeRefreshBlocking();

		 /// <summary>
		 /// Retrieve list of consistent Lucene index files for this partition.
		 /// </summary>
		 /// <returns> the iterator over index files. </returns>
		 /// <exception cref="IOException"> if any IO operation fails. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract org.neo4j.graphdb.ResourceIterator<java.io.File> snapshot() throws java.io.IOException;
		 public abstract ResourceIterator<File> Snapshot();

	}

}