﻿using System;

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
	/// <summary>
	/// Exception that will be thrown in case if encounter IOException during Lucene document retrieval.
	/// </summary>
	/// <seealso cref= org.apache.lucene.document.Document </seealso>
	/// <seealso cref= org.apache.lucene.search.IndexSearcher </seealso>
	/// <seealso cref= org.apache.lucene.search.DocIdSetIterator </seealso>
	public class LuceneDocumentRetrievalException : Exception
	{
		 private long _documentId;

		 public LuceneDocumentRetrievalException( string message, long documentId, Exception cause ) : this( message, cause )
		 {
			  this._documentId = documentId;
		 }

		 public LuceneDocumentRetrievalException( string message, Exception cause ) : base( message, cause )
		 {
		 }

		 public virtual long DocumentId
		 {
			 get
			 {
				  return _documentId;
			 }
		 }
	}

}