using System.Collections.Generic;
using System.Diagnostics;

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
namespace Org.Neo4j.Kernel.impl.index.labelscan
{

	using Org.Neo4j.Cursor;
	using Resource = Org.Neo4j.Graphdb.Resource;
	using Org.Neo4j.Index.@internal.gbptree;
	using IndexProgressor = Org.Neo4j.Storageengine.Api.schema.IndexProgressor;

	/// <summary>
	/// <seealso cref="IndexProgressor"/> which steps over multiple <seealso cref="LabelScanValue"/> and for each
	/// iterate over each set bit, returning actual node ids, i.e. {@code nodeIdRange+bitOffset}.
	/// 
	/// </summary>
	public class LabelScanValueIndexProgressor : LabelScanValueIndexAccessor, IndexProgressor, Resource
	{

		 private readonly Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeLabelClient _client;

		 internal LabelScanValueIndexProgressor( RawCursor<Hit<LabelScanKey, LabelScanValue>, IOException> cursor, ICollection<RawCursor<Hit<LabelScanKey, LabelScanValue>, IOException>> toRemoveFromWhenClosed, Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeLabelClient client ) : base( toRemoveFromWhenClosed, cursor )
		 {
			  this._client = client;
		 }

		 /// <summary>
		 ///  Progress through the index until the next accepted entry.
		 /// 
		 ///  Progress the cursor to the current <seealso cref="LabelScanValue"/>, if this is not accepted by the client or if current
		 ///  value is exhausted it continues to the next <seealso cref="LabelScanValue"/>  from <seealso cref="RawCursor"/>. </summary>
		 /// <returns> <code>true</code> if an accepted entry was found, <code>false</code> otherwise </returns>
		 public override bool Next()
		 {
			  for ( ; ; )
			  {
					while ( Bits != 0 )
					{
						 int delta = Long.numberOfTrailingZeros( Bits );
						 Bits &= Bits - 1;
						 if ( _client.acceptNode( BaseNodeId + delta, null ) )
						 {
							  return true;
						 }
					}
					try
					{
						 if ( !Cursor.next() )
						 {
							  Close();
							  return false;
						 }
					}
					catch ( IOException e )
					{
						 throw new UncheckedIOException( e );
					}

					Hit<LabelScanKey, LabelScanValue> hit = Cursor.get();
					BaseNodeId = hit.Key().IdRange * LabelScanValue.RangeSize;
					Bits = hit.Value().Bits;

					//noinspection AssertWithSideEffects
					Debug.Assert( KeysInOrder( hit.Key() ) );
			  }
		 }
	}

}