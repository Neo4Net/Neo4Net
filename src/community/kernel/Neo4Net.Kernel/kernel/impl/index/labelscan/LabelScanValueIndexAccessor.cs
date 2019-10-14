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

	using Neo4Net.Cursors;
	using Neo4Net.Index.Internal.gbptree;

	/// <summary>
	/// Base class for iterator and index-progressor of label scans.
	/// </summary>
	internal abstract class LabelScanValueIndexAccessor
	{
		 /// <summary>
		 /// <seealso cref="RawCursor"/> to lazily read new <seealso cref="LabelScanValue"/> from.
		 /// </summary>
		 protected internal readonly RawCursor<Hit<LabelScanKey, LabelScanValue>, IOException> Cursor;
		 /// <summary>
		 /// Remove provided cursor from this collection when iterator is exhausted.
		 /// </summary>
		 private readonly ICollection<RawCursor<Hit<LabelScanKey, LabelScanValue>, IOException>> _toRemoveFromWhenClosed;
		 /// <summary>
		 /// Current base nodeId, i.e. the <seealso cref="LabelScanKey.idRange"/> of the current <seealso cref="LabelScanKey"/>.
		 /// </summary>
		 internal long BaseNodeId;
		 /// <summary>
		 /// Bit set of the current <seealso cref="LabelScanValue"/>.
		 /// </summary>
		 protected internal long Bits;
		 /// <summary>
		 /// LabelId of previously retrieved <seealso cref="LabelScanKey"/>, for debugging and asserting purposes.
		 /// </summary>
		 private int _prevLabel = -1;
		 /// <summary>
		 /// IdRange of previously retrieved <seealso cref="LabelScanKey"/>, for debugging and asserting purposes.
		 /// </summary>
		 private long _prevRange = -1;
		 /// <summary>
		 /// Indicate provided cursor has been closed.
		 /// </summary>
		 protected internal bool Closed;

		 internal LabelScanValueIndexAccessor( ICollection<RawCursor<Hit<LabelScanKey, LabelScanValue>, IOException>> toRemoveFromWhenClosed, RawCursor<Hit<LabelScanKey, LabelScanValue>, IOException> cursor )
		 {
			  this._toRemoveFromWhenClosed = toRemoveFromWhenClosed;
			  this.Cursor = cursor;
		 }

		 internal virtual bool KeysInOrder( LabelScanKey key )
		 {
			  Debug.Assert( key.LabelId >= _prevLabel, "Expected to get ordered results, got " + key + );
						 " where previous label was " + _prevLabel;
			  Debug.Assert( key.IdRange > _prevRange, "Expected to get ordered results, got " + key + );
						 " where previous range was " + _prevRange;
			  _prevLabel = key.LabelId;
			  _prevRange = key.IdRange;
			  // Made as a method returning boolean so that it can participate in an assert call.
			  return true;
		 }

		 public virtual void Close()
		 {
			  if ( !Closed )
			  {
					try
					{
						 Cursor.close();
					}
					catch ( IOException e )
					{
						 throw new UncheckedIOException( e );
					}
					finally
					{
						 _toRemoveFromWhenClosed.remove( Cursor );
						 Closed = true;
					}
			  }
		 }
	}

}