using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Api.Impl.Schema.reader
{
	using Document = org.apache.lucene.document.Document;


	using Neo4Net.Helpers.Collections;

	public class LuceneAllEntriesIndexAccessorReader : BoundedIterable<long>
	{
		 private readonly BoundedIterable<Document> _documents;
		 private readonly System.Func<Document, long> _entityIdReader;

		 public LuceneAllEntriesIndexAccessorReader( BoundedIterable<Document> documents, System.Func<Document, long> entityIdReader )
		 {
			  this._documents = documents;
			  this._entityIdReader = entityIdReader;
		 }

		 public override long MaxCount()
		 {
			  return _documents.maxCount();
		 }

		 public override IEnumerator<long> Iterator()
		 {
			  IEnumerator<Document> iterator = _documents.GetEnumerator();
			  return new IteratorAnonymousInnerClass( this, iterator );
		 }

		 private class IteratorAnonymousInnerClass : IEnumerator<long>
		 {
			 private readonly LuceneAllEntriesIndexAccessorReader _outerInstance;

			 private IEnumerator<Document> _iterator;

			 public IteratorAnonymousInnerClass( LuceneAllEntriesIndexAccessorReader outerInstance, IEnumerator<Document> iterator )
			 {
				 this.outerInstance = outerInstance;
				 this._iterator = iterator;
			 }

			 public bool hasNext()
			 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  return _iterator.hasNext();
			 }

			 public long? next()
			 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  return _outerInstance.entityIdReader.applyAsLong( _iterator.next() );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
		 public override void Close()
		 {
			  _documents.close();
		 }

	}

}