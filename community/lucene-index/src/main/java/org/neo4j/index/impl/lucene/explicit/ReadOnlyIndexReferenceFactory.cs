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
namespace Org.Neo4j.Index.impl.lucene.@explicit
{
	using DirectoryReader = Org.Apache.Lucene.Index.DirectoryReader;
	using IndexReader = Org.Apache.Lucene.Index.IndexReader;
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;


	using ExplicitIndexNotFoundKernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;

	public class ReadOnlyIndexReferenceFactory : IndexReferenceFactory
	{
		 public ReadOnlyIndexReferenceFactory( LuceneDataSource.LuceneFilesystemFacade filesystemFacade, File baseStorePath, IndexTypeCache typeCache ) : base( filesystemFacade, baseStorePath, typeCache )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: IndexReference createIndexReference(IndexIdentifier identifier) throws java.io.IOException, org.neo4j.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 internal override IndexReference CreateIndexReference( IndexIdentifier identifier )
		 {
			  IndexReader reader = DirectoryReader.open( GetIndexDirectory( identifier ) );
			  IndexSearcher indexSearcher = NewIndexSearcher( identifier, reader );
			  return new ReadOnlyIndexReference( identifier, indexSearcher );
		 }

		 internal override IndexReference Refresh( IndexReference indexReference )
		 {
			  return indexReference;
		 }
	}

}