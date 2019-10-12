using System;

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
namespace Neo4Net.Index.impl.lucene.@explicit
{
	using DirectoryReader = Org.Apache.Lucene.Index.DirectoryReader;
	using IndexReader = Org.Apache.Lucene.Index.IndexReader;
	using IndexWriter = Org.Apache.Lucene.Index.IndexWriter;
	using IndexWriterConfig = Org.Apache.Lucene.Index.IndexWriterConfig;
	using KeepOnlyLastCommitDeletionPolicy = Org.Apache.Lucene.Index.KeepOnlyLastCommitDeletionPolicy;
	using SnapshotDeletionPolicy = Org.Apache.Lucene.Index.SnapshotDeletionPolicy;
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;
	using Similarity = org.apache.lucene.search.similarities.Similarity;
	using Directory = org.apache.lucene.store.Directory;


	using ExplicitIndexNotFoundKernelException = Neo4Net.@internal.Kernel.Api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;

	internal class WritableIndexReferenceFactory : IndexReferenceFactory
	{
		 internal WritableIndexReferenceFactory( LuceneDataSource.LuceneFilesystemFacade filesystemFacade, File baseStorePath, IndexTypeCache typeCache ) : base( filesystemFacade, baseStorePath, typeCache )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: IndexReference createIndexReference(IndexIdentifier identifier) throws java.io.IOException, org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 internal override IndexReference CreateIndexReference( IndexIdentifier identifier )
		 {
			  IndexWriter writer = NewIndexWriter( identifier );
			  IndexReader reader = DirectoryReader.open( writer, true );
			  IndexSearcher indexSearcher = NewIndexSearcher( identifier, reader );
			  return new WritableIndexReference( identifier, indexSearcher, writer );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: IndexReference refresh(IndexReference indexReference) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 internal override IndexReference Refresh( IndexReference indexReference )
		 {
			  try
			  {
					DirectoryReader reader = ( DirectoryReader ) indexReference.Searcher.IndexReader;
					IndexWriter writer = indexReference.Writer;
					IndexReader reopened = DirectoryReader.openIfChanged( reader, writer );
					if ( reopened != null )
					{
						 IndexSearcher newSearcher = NewIndexSearcher( indexReference.Identifier, reopened );
						 indexReference.DetachOrClose();
						 return new WritableIndexReference( indexReference.Identifier, newSearcher, writer );
					}
					return indexReference;
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.apache.lucene.index.IndexWriter newIndexWriter(IndexIdentifier identifier) throws org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 private IndexWriter NewIndexWriter( IndexIdentifier identifier )
		 {
			  try
			  {
					Directory indexDirectory = GetIndexDirectory( identifier );
					IndexType type = GetType( identifier );
					IndexWriterConfig writerConfig = new IndexWriterConfig( type.Analyzer );
					writerConfig.IndexDeletionPolicy = new SnapshotDeletionPolicy( new KeepOnlyLastCommitDeletionPolicy() );
					Similarity similarity = type.Similarity;
					if ( similarity != null )
					{
						 writerConfig.Similarity = similarity;
					}
					return new IndexWriter( indexDirectory, writerConfig );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }
	}

}