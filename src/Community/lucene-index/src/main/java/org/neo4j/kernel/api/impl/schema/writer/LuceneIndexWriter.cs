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
namespace Neo4Net.Kernel.Api.Impl.Schema.writer
{
	using Document = org.apache.lucene.document.Document;
	using Term = Org.Apache.Lucene.Index.Term;
	using Query = org.apache.lucene.search.Query;

	/// <summary>
	/// A thin wrapper around <seealso cref="org.apache.lucene.index.IndexWriter"/> that exposes only some part of it's
	/// functionality that it really needed and hides a fact that index is partitioned.
	/// </summary>
	public interface LuceneIndexWriter
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void addDocument(org.apache.lucene.document.Document document) throws java.io.IOException;
		 void AddDocument( Document document );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void addDocuments(int numDocs, Iterable<org.apache.lucene.document.Document> document) throws java.io.IOException;
		 void AddDocuments( int numDocs, IEnumerable<Document> document );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void updateDocument(org.apache.lucene.index.Term term, org.apache.lucene.document.Document document) throws java.io.IOException;
		 void UpdateDocument( Term term, Document document );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void deleteDocuments(org.apache.lucene.index.Term term) throws java.io.IOException;
		 void DeleteDocuments( Term term );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void deleteDocuments(org.apache.lucene.search.Query query) throws java.io.IOException;
		 void DeleteDocuments( Query query );
	}

}