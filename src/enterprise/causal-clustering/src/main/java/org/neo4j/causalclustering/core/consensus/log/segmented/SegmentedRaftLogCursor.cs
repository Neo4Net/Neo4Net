/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.core.consensus.log.segmented
{

	using Neo4Net.Cursors;
	using Neo4Net.Cursors;

	internal class SegmentedRaftLogCursor : RaftLogCursor
	{
		 private readonly IOCursor<EntryRecord> _inner;
		 private CursorValue<RaftLogEntry> _current;
		 private long _index;

		 internal SegmentedRaftLogCursor( long fromIndex, IOCursor<EntryRecord> inner )
		 {
			  this._inner = inner;
			  this._current = new CursorValue<RaftLogEntry>();
			  this._index = fromIndex - 1;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next() throws java.io.IOException
		 public override bool Next()
		 {
			  bool hasNext = _inner.next();
			  if ( hasNext )
			  {
					_current.set( _inner.get().logEntry() );
					_index++;
			  }
			  else
			  {
					_current.invalidate();
			  }
			  return hasNext;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _inner.close();
		 }

		 public override long Index()
		 {
			  return _index;
		 }

		 public override RaftLogEntry Get()
		 {
			  return _current.get();
		 }
	}

}