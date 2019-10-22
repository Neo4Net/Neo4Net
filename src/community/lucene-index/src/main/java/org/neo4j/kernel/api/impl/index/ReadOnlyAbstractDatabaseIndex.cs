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
namespace Neo4Net.Kernel.Api.Impl.Index
{

	using Neo4Net.GraphDb;
	using LuceneIndexWriter = Neo4Net.Kernel.Api.Impl.Schema.writer.LuceneIndexWriter;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;

	/// <summary>
	/// Read only lucene index representation that wraps provided index implementation and
	/// allow read only operations only on top of it. </summary>
	/// @param <INDEX> - particular index implementation </param>
	public abstract class ReadOnlyAbstractDatabaseIndex<INDEX, READER> : AbstractDatabaseIndex<INDEX, READER> where INDEX : AbstractLuceneIndex<READER> where READER : Neo4Net.Storageengine.Api.schema.IndexReader
	{
		 public ReadOnlyAbstractDatabaseIndex( INDEX luceneIndex ) : base( luceneIndex )
		 {
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
		 public override void Create()
		 {
			  throw new System.NotSupportedException( "Index creation in read only mode is not supported." );
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
		 public override bool ReadOnly
		 {
			 get
			 {
				  return true;
			 }
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
		 public override void Drop()
		 {
			  throw new System.NotSupportedException( "Index drop is not supported in read only mode." );
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
		 public override void Flush()
		 {
			  // nothing to flush in read only mode
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  luceneIndex.close();
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.graphdb.ResourceIterator<java.io.File> snapshot() throws java.io.IOException
		 public override ResourceIterator<File> Snapshot()
		 {
			  return luceneIndex.snapshot();
		 }

		 /// <summary>
		 /// {@inheritDoc}
		 /// </summary>
		 public override void MaybeRefreshBlocking()
		 {
			  //nothing to refresh in read only mode
		 }

		 public override LuceneIndexWriter IndexWriter
		 {
			 get
			 {
				  throw new System.NotSupportedException( "Can't get index writer for read only lucene index." );
			 }
		 }

		 /// <summary>
		 /// Unsupported operation in read only index.
		 /// </summary>
		 public override void MarkAsOnline()
		 {
			  throw new System.NotSupportedException( "Can't mark read only index." );
		 }
	}

}