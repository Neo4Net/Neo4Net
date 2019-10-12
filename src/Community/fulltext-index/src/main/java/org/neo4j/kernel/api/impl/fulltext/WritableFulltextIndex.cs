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
namespace Neo4Net.Kernel.Api.Impl.Fulltext
{

	using Neo4Net.Kernel.Api.Impl.Index;

	internal class WritableFulltextIndex : WritableAbstractDatabaseIndex<LuceneFulltextIndex, FulltextIndexReader>, DatabaseFulltextIndex
	{
		 private readonly IndexUpdateSink _indexUpdateSink;

		 internal WritableFulltextIndex( IndexUpdateSink indexUpdateSink, LuceneFulltextIndex fulltextIndex ) : base( fulltextIndex )
		 {
			  this._indexUpdateSink = indexUpdateSink;
		 }

		 public override string ToString()
		 {
			  return luceneIndex.ToString();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void commitLockedFlush() throws java.io.IOException
		 protected internal override void CommitLockedFlush()
		 {
			  _indexUpdateSink.awaitUpdateApplication();
			  base.CommitLockedFlush();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void commitLockedClose() throws java.io.IOException
		 protected internal override void CommitLockedClose()
		 {
			  _indexUpdateSink.awaitUpdateApplication();
			  base.CommitLockedClose();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public TransactionStateLuceneIndexWriter getTransactionalIndexWriter() throws java.io.IOException
		 public virtual TransactionStateLuceneIndexWriter TransactionalIndexWriter
		 {
			 get
			 {
				  return new TransactionStateLuceneIndexWriter( luceneIndex );
			 }
		 }
	}

}