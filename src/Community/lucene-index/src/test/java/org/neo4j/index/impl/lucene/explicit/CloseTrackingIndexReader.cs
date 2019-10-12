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
namespace Neo4Net.Index.impl.lucene.@explicit
{
	using CompositeReader = Org.Apache.Lucene.Index.CompositeReader;
	using Fields = Org.Apache.Lucene.Index.Fields;
	using IndexReader = Org.Apache.Lucene.Index.IndexReader;
	using StoredFieldVisitor = Org.Apache.Lucene.Index.StoredFieldVisitor;
	using Term = Org.Apache.Lucene.Index.Term;

	public class CloseTrackingIndexReader : CompositeReader
	{

		 private bool _closed;

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: protected java.util.List<? extends org.apache.lucene.index.IndexReader> getSequentialSubReaders()
		 protected internal override IList<IndexReader> SequentialSubReaders
		 {
			 get
			 {
				  return null;
			 }
		 }

		 public override Fields GetTermVectors( int docID )
		 {
			  return null;
		 }

		 public override int NumDocs()
		 {
			  return 0;
		 }

		 public override int MaxDoc()
		 {
			  return 0;
		 }

		 public override void Document( int docID, StoredFieldVisitor visitor )
		 {

		 }

		 protected internal override void DoClose()
		 {
			  _closed = true;
		 }

		 public override int DocFreq( Term term )
		 {
			  return 0;
		 }

		 public override long TotalTermFreq( Term term )
		 {
			  return 0;
		 }

		 public override long GetSumDocFreq( string field )
		 {
			  return 0;
		 }

		 public override int GetDocCount( string field )
		 {
			  return 0;
		 }

		 public override long GetSumTotalTermFreq( string field )
		 {
			  return 0;
		 }

		 public virtual bool Closed
		 {
			 get
			 {
				  return _closed;
			 }
		 }
	}

}