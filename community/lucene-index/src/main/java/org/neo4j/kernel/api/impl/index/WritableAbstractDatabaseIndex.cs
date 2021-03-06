﻿using System.Collections.Generic;

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

	using Org.Neo4j.Graphdb;
	using AbstractIndexPartition = Org.Neo4j.Kernel.Api.Impl.Index.partition.AbstractIndexPartition;
	using LuceneIndexWriter = Org.Neo4j.Kernel.Api.Impl.Schema.writer.LuceneIndexWriter;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;

	/// <summary>
	/// Writable lucene index representation that wraps provided index implementation and
	/// allow read only operations only on top of it. </summary>
	/// @param <INDEX> - particular index implementation </param>
	public class WritableAbstractDatabaseIndex<INDEX, READER> : AbstractDatabaseIndex<INDEX, READER> where INDEX : AbstractLuceneIndex<READER> where READER : Org.Neo4j.Storageengine.Api.schema.IndexReader
	{
		 // lock used to guard commits and close of lucene indexes from separate threads
		 private readonly ReentrantLock _commitCloseLock = new ReentrantLock();

		 public WritableAbstractDatabaseIndex( INDEX luceneIndex ) : base( luceneIndex )
		 {
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void create() throws java.io.IOException
		 public override void Create()
		 {
			  luceneIndex.create();
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
		 public override bool ReadOnly
		 {
			 get
			 {
				  return false;
			 }
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
		 public override void Drop()
		 {
			  _commitCloseLock.@lock();
			  try
			  {
					CommitLockedDrop();
			  }
			  finally
			  {
					_commitCloseLock.unlock();
			  }
		 }

		 protected internal virtual void CommitLockedDrop()
		 {
			  luceneIndex.drop();
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flush() throws java.io.IOException
		 public override void Flush()
		 {
			  _commitCloseLock.@lock();
			  try
			  {
					CommitLockedFlush();
			  }
			  finally
			  {
					_commitCloseLock.unlock();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void commitLockedFlush() throws java.io.IOException
		 protected internal virtual void CommitLockedFlush()
		 {
			  luceneIndex.flush( false );
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _commitCloseLock.@lock();
			  try
			  {
					CommitLockedClose();
			  }
			  finally
			  {
					_commitCloseLock.unlock();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void commitLockedClose() throws java.io.IOException
		 protected internal virtual void CommitLockedClose()
		 {
			  luceneIndex.close();
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.graphdb.ResourceIterator<java.io.File> snapshot() throws java.io.IOException
		 public override ResourceIterator<File> Snapshot()
		 {
			  _commitCloseLock.@lock();
			  try
			  {
					return luceneIndex.snapshot();
			  }
			  finally
			  {
					_commitCloseLock.unlock();
			  }
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void maybeRefreshBlocking() throws java.io.IOException
		 public override void MaybeRefreshBlocking()
		 {
			  luceneIndex.maybeRefreshBlocking();
		 }

		 /// <summary>
		 /// Add new partition to the index. Must only be called by a single thread at a time.
		 /// </summary>
		 /// <returns> newly created partition </returns>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.kernel.api.impl.index.partition.AbstractIndexPartition addNewPartition() throws java.io.IOException
		 public virtual AbstractIndexPartition AddNewPartition()
		 {
			  return luceneIndex.addNewPartition();
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void markAsOnline() throws java.io.IOException
		 public override void MarkAsOnline()
		 {
			  _commitCloseLock.@lock();
			  try
			  {
					luceneIndex.markAsOnline();
			  }
			  finally
			  {
					_commitCloseLock.unlock();
			  }
		 }

		 public override LuceneIndexWriter IndexWriter
		 {
			 get
			 {
				  return luceneIndex.getIndexWriter( this );
			 }
		 }

		 public virtual bool HasSinglePartition( IList<AbstractIndexPartition> partitions )
		 {
			  return luceneIndex.hasSinglePartition( partitions );
		 }

		 public virtual AbstractIndexPartition GetFirstPartition( IList<AbstractIndexPartition> partitions )
		 {
			  return luceneIndex.getFirstPartition( partitions );
		 }
	}

}