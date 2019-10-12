using System;
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
	using Document = org.apache.lucene.document.Document;
	using LongSet = org.eclipse.collections.api.set.primitive.LongSet;

	using Neo4Net.Graphdb.index;

	public class DocToIdIterator : AbstractExplicitIndexHits
	{
		 private readonly ICollection<EntityId> _removedInTransactionState;
		 private readonly EntityId_LongCostume _idCostume = new EntityId_LongCostume();
		 private IndexReference _searcherOrNull;
		 private readonly IndexHits<Document> _source;
		 private readonly LongSet _idsModifiedInTransactionState;

		 public DocToIdIterator( IndexHits<Document> source, ICollection<EntityId> exclude, IndexReference searcherOrNull, LongSet idsModifiedInTransactionState )
		 {
			  this._source = source;
			  this._removedInTransactionState = exclude;
			  this._searcherOrNull = searcherOrNull;
			  this._idsModifiedInTransactionState = idsModifiedInTransactionState;
			  if ( source.Size() == 0 )
			  {
					Close();
			  }
		 }

		 protected internal override bool FetchNext()
		 {
			  while ( _source.MoveNext() )
			  {
					Document doc = _source.Current;
					long id = IdFromDoc( doc );
					bool documentIsFromStore = doc.getField( FullTxData.TX_STATE_KEY ) == null;
					bool idWillBeReturnedByTransactionStateInstead = documentIsFromStore && _idsModifiedInTransactionState.contains( id );
					if ( _removedInTransactionState.Contains( _idCostume.setId( id ) ) || idWillBeReturnedByTransactionStateInstead )
					{
						 // Skip this one, continue to the next
						 continue;
					}
					return Next( id );
			  }
			  return EndReached();
		 }

		 internal static long IdFromDoc( Document doc )
		 {
			  return long.Parse( doc.get( LuceneExplicitIndex.KEY_DOC_ID ) );
		 }

		 protected internal virtual bool EndReached()
		 {
			  Close();
			  return false;
		 }

		 public override void Close()
		 {
			  if ( !Closed )
			  {
					this._searcherOrNull.close();
					this._searcherOrNull = null;
			  }
		 }

		 public override int Size()
		 {
			  /*
			   * If stuff was removed from the index during this tx and during the same tx a query that matches them is
			   * issued, then it is possible to get negative size from the IndexHits result, if exclude is larger than source.
			   * To avoid such weirdness, we return at least 0. Note that the iterator will return no results, as it should.
			   */
			  return Math.Max( 0, _source.size() - _removedInTransactionState.Count );
		 }

		 private bool Closed
		 {
			 get
			 {
				  return _searcherOrNull == null;
			 }
		 }

		 public override float CurrentScore()
		 {
			  return _source.currentScore();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void finalize() throws Throwable
		 protected internal override void Finalize()
		 {
			  Close();
			  base.Finalize();
		 }
	}

}