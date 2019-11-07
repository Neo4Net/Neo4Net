using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.Kernel.impl.index.labelscan
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;


	using PrimitiveLongResourceIterator = Neo4Net.Collections.PrimitiveLongResourceIterator;
	using Neo4Net.Cursors;
	using Neo4Net.Index.Internal.gbptree;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.labelscan.LabelScanValue.RANGE_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.index.labelscan.NativeLabelScanWriter.rangeOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.StorageEngine.schema.LabelScanReader_Fields.NO_ID;

	/// <summary>
	/// <seealso cref="LongIterator"/> which iterate over multiple <seealso cref="LabelScanValue"/> and for each
	/// iterate over each set bit, returning actual node ids, i.e. {@code nodeIdRange+bitOffset}.
	/// 
	/// The provided <seealso cref="RawCursor"/> is managed externally, e.g. <seealso cref="NativeLabelScanReader"/>,
	/// this because implemented interface lacks close-method.
	/// </summary>
	internal class LabelScanValueIterator : LabelScanValueIndexAccessor, PrimitiveLongResourceIterator
	{
		 private long _fromId;
		 private bool _hasNextDecided;
		 private bool _hasNext;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal long NextConflict;

		 /// <param name="fromId"> IEntity to start from (exclusive). The cursor gives entries that are effectively small bit-sets and the fromId may
		 /// be somewhere inside a bit-set range. </param>
		 internal LabelScanValueIterator( IRawCursor<Hit<LabelScanKey, LabelScanValue>, IOException> cursor, ICollection<RawCursor<Hit<LabelScanKey, LabelScanValue>, IOException>> toRemoveFromWhenClosed, long fromId ) : base( toRemoveFromWhenClosed, cursor )
		 {
			  this._fromId = fromId;
		 }

		 public override bool HasNext()
		 {
			  if ( !_hasNextDecided )
			  {
					_hasNext = FetchNext();
					_hasNextDecided = true;
			  }
			  return _hasNext;
		 }

		 public override long Next()
		 {
			  if ( !HasNext() )
			  {
					throw new NoSuchElementException( "No more elements in " + this );
			  }
			  _hasNextDecided = false;
			  return NextConflict;
		 }

		 /// <returns> next node id in the current <seealso cref="LabelScanValue"/> or, if current value exhausted,
		 /// goes to next <seealso cref="LabelScanValue"/> from <seealso cref="RawCursor"/>. Returns {@code true} if next node id
		 /// was found, otherwise {@code false}. </returns>
		 protected internal virtual bool FetchNext()
		 {
			  while ( true )
			  {
					if ( Bits != 0 )
					{
						 int delta = Long.numberOfTrailingZeros( Bits );
						 Bits &= Bits - 1;
						 NextConflict = BaseNodeId + delta;
						 _hasNext = true;
						 return true;
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
					BaseNodeId = hit.Key().IdRange * RANGE_SIZE;
					Bits = hit.Value().Bits;

					if ( _fromId != NO_ID )
					{
						 // If we've been told to start at a specific id then trim off ids in this range less than or equal to that id
						 long range = rangeOf( _fromId );
						 if ( range == hit.Key().IdRange )
						 {
							  // Only do this if we're in the idRange that fromId is in, otherwise there were no ids this time in this range
							  long relativeStartId = _fromId % RANGE_SIZE;
							  long mask = relativeStartId == RANGE_SIZE - 1 ? -1 : ( 1L << ( relativeStartId + 1 ) ) - 1;
							  Bits &= ~mask;
						 }
						 // ... and let's not do that again, only for the first idRange
						 _fromId = NO_ID;
					}

					//noinspection AssertWithSideEffects
					Debug.Assert( KeysInOrder( hit.Key() ) );
			  }
		 }
	}

}