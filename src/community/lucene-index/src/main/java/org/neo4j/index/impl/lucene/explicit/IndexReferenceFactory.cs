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
namespace Neo4Net.Index.impl.lucene.@explicit
{
	using DirectoryReader = Org.Apache.Lucene.Index.DirectoryReader;
	using IndexReader = Org.Apache.Lucene.Index.IndexReader;
	using IndexWriter = Org.Apache.Lucene.Index.IndexWriter;
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;
	using Directory = org.apache.lucene.store.Directory;


	using ExplicitIndexNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException;

	/// <summary>
	/// Factory that build appropriate (read only or writable) <seealso cref="IndexReference"/> for provided <seealso cref="IndexIdentifier"/>
	/// or refresh previously constructed instance.
	/// </summary>
	internal abstract class IndexReferenceFactory
	{
		 private readonly File _baseStorePath;
		 private readonly IndexTypeCache _typeCache;
		 private readonly LuceneDataSource.LuceneFilesystemFacade _filesystemFacade;

		 internal IndexReferenceFactory( LuceneDataSource.LuceneFilesystemFacade filesystemFacade, File baseStorePath, IndexTypeCache typeCache )
		 {
			  this._filesystemFacade = filesystemFacade;
			  this._baseStorePath = baseStorePath;
			  this._typeCache = typeCache;
		 }

		 /// <summary>
		 /// Create new <seealso cref="IndexReference"/> for provided <seealso cref="IndexIdentifier"/>. </summary>
		 /// <param name="indexIdentifier"> index identifier to build index for. </param>
		 /// <returns> newly create <seealso cref="IndexReference"/>
		 /// </returns>
		 /// <exception cref="IOException"> in case of exception during accessing lucene reader/writer. </exception>
		 /// <exception cref="ExplicitIndexNotFoundKernelException"> if the index is dropped prior to, or concurrently with, this
		 /// operation. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract IndexReference createIndexReference(IndexIdentifier indexIdentifier) throws java.io.IOException, Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 internal abstract IndexReference CreateIndexReference( IndexIdentifier indexIdentifier );

		 /// <summary>
		 /// If nothing has changed underneath (since the searcher was last created or refreshed) {@code searcher} is
		 /// returned. But if something has changed a refreshed searcher is returned. It makes use if the
		 /// <seealso cref="DirectoryReader.openIfChanged(DirectoryReader, IndexWriter, bool)"/> which faster than opening an index
		 /// from scratch.
		 /// </summary>
		 /// <param name="indexReference"> the <seealso cref="IndexReference"/> to refresh. </param>
		 /// <returns> a refreshed version of the searcher or, if nothing has changed,
		 ///         {@code null}. </returns>
		 /// <exception cref="RuntimeException"> if there's a problem with the index. </exception>
		 /// <exception cref="ExplicitIndexNotFoundKernelException"> if the index is dropped prior to, or concurrently with, this
		 /// operation. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract IndexReference refresh(IndexReference indexReference) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
		 internal abstract IndexReference Refresh( IndexReference indexReference );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.lucene.store.Directory getIndexDirectory(IndexIdentifier identifier) throws java.io.IOException
		 internal virtual Directory GetIndexDirectory( IndexIdentifier identifier )
		 {
			  return _filesystemFacade.getDirectory( _baseStorePath, identifier );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.lucene.search.IndexSearcher newIndexSearcher(IndexIdentifier identifier, org.apache.lucene.index.IndexReader reader) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 internal virtual IndexSearcher NewIndexSearcher( IndexIdentifier identifier, IndexReader reader )
		 {
			  IndexSearcher searcher = new IndexSearcher( reader );
			  IndexType type = GetType( identifier );
			  if ( type.Similarity != null )
			  {
					searcher.Similarity = type.Similarity;
			  }
			  return searcher;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: IndexType getType(IndexIdentifier identifier) throws Neo4Net.Kernel.Api.Internal.Exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 internal virtual IndexType GetType( IndexIdentifier identifier )
		 {
			  return _typeCache.getIndexType( identifier, false );
		 }
	}


}