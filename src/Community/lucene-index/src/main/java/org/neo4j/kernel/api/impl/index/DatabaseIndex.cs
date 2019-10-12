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
namespace Neo4Net.Kernel.Api.Impl.Index
{

	using Neo4Net.Graphdb;
	using WritableIndexSnapshotFileIterator = Neo4Net.Kernel.Api.Impl.Index.backup.WritableIndexSnapshotFileIterator;
	using AbstractIndexPartition = Neo4Net.Kernel.Api.Impl.Index.partition.AbstractIndexPartition;
	using LuceneIndexWriter = Neo4Net.Kernel.Api.Impl.Schema.writer.LuceneIndexWriter;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;

	/// <summary>
	/// Lucene index that may consist of one or multiple separate lucene indexes that are represented as independent
	/// <seealso cref="AbstractIndexPartition partitions"/>.
	/// </summary>
	public interface DatabaseIndex<READER> : System.IDisposable where READER : Neo4Net.Storageengine.Api.schema.IndexReader
	{
		 /// <summary>
		 /// Creates new index.
		 /// As part of creation process index will allocate all required folders, index failure storage
		 /// and will create its first partition.
		 /// <para>
		 /// <b>Index creation do not automatically open it. To be able to use index please open it first.</b>
		 /// 
		 /// </para>
		 /// </summary>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void create() throws java.io.IOException;
		 void Create();

		 /// <summary>
		 /// Open index with all allocated partitions.
		 /// </summary>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void open() throws java.io.IOException;
		 void Open();

		 /// <summary>
		 /// Check if index is open or not </summary>
		 /// <returns> true if index is open </returns>
		 bool Open { get; }

		 /// <summary>
		 /// Check if index is opened in read only mode </summary>
		 /// <returns> true if index open in rad only mode </returns>
		 bool ReadOnly { get; }

		 /// <summary>
		 /// Check lucene index existence within all allocated partitions.
		 /// </summary>
		 /// <returns> true if index exist in all partitions, false when index is empty or does not exist </returns>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean exists() throws java.io.IOException;
		 bool Exists();

		 /// <summary>
		 /// Verify state of the index.
		 /// If index is already open and in use method assume that index is valid since lucene already operating with it,
		 /// otherwise necessary checks perform.
		 /// </summary>
		 /// <returns> true if lucene confirm that index is in valid clean state or index is already open. </returns>
		 bool Valid { get; }

		 /// <summary>
		 /// Close index and deletes all it's partitions.
		 /// </summary>
		 void Drop();

		 /// <summary>
		 /// Commits all index partitions.
		 /// </summary>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void flush() throws java.io.IOException;
		 void Flush();

		 /// <summary>
		 /// Creates an iterable over all <seealso cref="org.apache.lucene.document.Document document"/>s in all partitions.
		 /// </summary>
		 /// <returns> LuceneAllDocumentsReader over all documents </returns>
		 LuceneAllDocumentsReader AllDocumentsReader();

		 /// <summary>
		 /// Snapshot of all file in all index partitions.
		 /// </summary>
		 /// <returns> iterator over all index files. </returns>
		 /// <exception cref="IOException"> </exception>
		 /// <seealso cref= WritableIndexSnapshotFileIterator </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.neo4j.graphdb.ResourceIterator<java.io.File> snapshot() throws java.io.IOException;
		 ResourceIterator<File> Snapshot();

		 /// <summary>
		 /// Refresh all partitions to make newly inserted data visible for readers.
		 /// </summary>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void maybeRefreshBlocking() throws java.io.IOException;
		 void MaybeRefreshBlocking();

		 /// <summary>
		 /// Get index partitions </summary>
		 /// <returns> list of index partition </returns>
		 IList<AbstractIndexPartition> Partitions { get; }

		 LuceneIndexWriter IndexWriter { get; }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: READER getIndexReader() throws java.io.IOException;
		 READER IndexReader { get; }

		 IndexDescriptor Descriptor { get; }

		 /// <summary>
		 /// Check if this index is marked as online.
		 /// </summary>
		 /// <returns> <code>true</code> if index is online, <code>false</code> otherwise </returns>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: boolean isOnline() throws java.io.IOException;
		 bool Online { get; }

		 /// <summary>
		 /// Marks index as online by including "status" -> "online" map into commit metadata of the first partition.
		 /// </summary>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void markAsOnline() throws java.io.IOException;
		 void MarkAsOnline();

		 /// <summary>
		 /// Writes the given failure message to the failure storage.
		 /// </summary>
		 /// <param name="failure"> the failure message. </param>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void markAsFailed(String failure) throws java.io.IOException;
		 void MarkAsFailed( string failure );
	}

}