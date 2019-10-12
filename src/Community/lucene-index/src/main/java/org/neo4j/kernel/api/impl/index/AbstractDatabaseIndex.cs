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

	using AbstractIndexPartition = Neo4Net.Kernel.Api.Impl.Index.partition.AbstractIndexPartition;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;

	/// <summary>
	/// This class collects the common features of <seealso cref="ReadOnlyAbstractDatabaseIndex"/> and <seealso cref="WritableAbstractDatabaseIndex"/>.
	/// </summary>
	internal abstract class AbstractDatabaseIndex<INDEX, READER> : DatabaseIndex<READER> where INDEX : AbstractLuceneIndex<READER> where READER : Neo4Net.Storageengine.Api.schema.IndexReader
	{
		public abstract void MarkAsOnline();
		public abstract Neo4Net.Kernel.Api.Impl.Schema.writer.LuceneIndexWriter IndexWriter { get; }
		public abstract void MaybeRefreshBlocking();
		public abstract Neo4Net.Graphdb.ResourceIterator<java.io.File> Snapshot();
		public abstract void Flush();
		public abstract void Drop();
		public abstract bool ReadOnly { get; }
		public abstract void Create();
		 protected internal readonly INDEX LuceneIndex;

		 internal AbstractDatabaseIndex( INDEX luceneIndex )
		 {
			  this.LuceneIndex = luceneIndex;
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void open() throws java.io.IOException
		 public override void Open()
		 {
			  LuceneIndex.open();
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
		 public virtual bool Open
		 {
			 get
			 {
				  return LuceneIndex.Open;
			 }
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean exists() throws java.io.IOException
		 public override bool Exists()
		 {
			  return LuceneIndex.exists();
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
		 public virtual bool Valid
		 {
			 get
			 {
				  return LuceneIndex.Valid;
			 }
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
		 public override LuceneAllDocumentsReader AllDocumentsReader()
		 {
			  return LuceneIndex.allDocumentsReader();
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
		 public virtual IList<AbstractIndexPartition> Partitions
		 {
			 get
			 {
				  return LuceneIndex.Partitions;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public READER getIndexReader() throws java.io.IOException
		 public virtual READER IndexReader
		 {
			 get
			 {
				  return LuceneIndex.IndexReader;
			 }
		 }

		 public virtual IndexDescriptor Descriptor
		 {
			 get
			 {
				  return LuceneIndex.Descriptor;
			 }
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean isOnline() throws java.io.IOException
		 public virtual bool Online
		 {
			 get
			 {
				  return LuceneIndex.Online;
			 }
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void markAsFailed(String failure) throws java.io.IOException
		 public override void MarkAsFailed( string failure )
		 {
			  LuceneIndex.markAsFailed( failure );
		 }
	}

}