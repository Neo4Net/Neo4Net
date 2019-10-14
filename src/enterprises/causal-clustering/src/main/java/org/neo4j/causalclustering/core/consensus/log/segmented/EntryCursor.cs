/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.core.consensus.log.segmented
{

	using Neo4Net.causalclustering.core.consensus.log.segmented.OpenEndRangeMap;
	using Neo4Net.Cursors;
	using Neo4Net.Cursors;

	/// <summary>
	/// The entry store allows iterating over RAFT log entries efficiently and handles moving from one
	/// segment to the next in a transparent manner. It can thus be mainly viewed as a factory for a
	/// smart segment-crossing cursor.
	/// </summary>
	internal class EntryCursor : IOCursor<EntryRecord>
	{
		 private readonly Segments _segments;
		 private IOCursor<EntryRecord> _cursor;
		 private ValueRange<long, SegmentFile> _segmentRange;
		 private long _currentIndex;

		 private long _limit = long.MaxValue;
		 private CursorValue<EntryRecord> _currentRecord = new CursorValue<EntryRecord>();

		 internal EntryCursor( Segments segments, long logIndex )
		 {
			  this._segments = segments;
			  this._currentIndex = logIndex - 1;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next() throws java.io.IOException
		 public override bool Next()
		 {
			  _currentIndex++;
			  if ( _segmentRange == null || _currentIndex >= _limit )
			  {
					if ( !NextSegment() )
					{
						 return false;
					}
			  }

			  if ( _cursor.next() )
			  {
					_currentRecord.set( _cursor.get() );
					return true;
			  }

			  _currentRecord.invalidate();
			  return false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean nextSegment() throws java.io.IOException
		 private bool NextSegment()
		 {
			  _segmentRange = _segments.getForIndex( _currentIndex );
			  Optional<SegmentFile> optionalFile = _segmentRange.value();

			  if ( !optionalFile.Present )
			  {
					_currentRecord.invalidate();
					return false;
			  }

			  SegmentFile file = optionalFile.get();

			  /* Open new reader before closing old, so that pruner cannot overtake us. */
			  IOCursor<EntryRecord> oldCursor = _cursor;
			  try
			  {
					_cursor = file.GetCursor( _currentIndex );
			  }
			  catch ( DisposedException )
			  {
					_currentRecord.invalidate();
					return false;
			  }

			  if ( oldCursor != null )
			  {
					oldCursor.close();
			  }

			  _limit = _segmentRange.limit().GetValueOrDefault(long.MaxValue);

			  return true;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  if ( _cursor != null )
			  {
					_cursor.close();
			  }
		 }

		 public override EntryRecord Get()
		 {
			  return _currentRecord.get();
		 }
	}

}