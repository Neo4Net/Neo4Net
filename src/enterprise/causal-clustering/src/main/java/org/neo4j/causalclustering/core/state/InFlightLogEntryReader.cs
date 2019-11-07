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
namespace Neo4Net.causalclustering.core.state
{

	using InFlightCache = Neo4Net.causalclustering.core.consensus.log.cache.InFlightCache;
	using RaftLogCursor = Neo4Net.causalclustering.core.consensus.log.RaftLogCursor;
	using RaftLogEntry = Neo4Net.causalclustering.core.consensus.log.RaftLogEntry;
	using ReadableRaftLog = Neo4Net.causalclustering.core.consensus.log.ReadableRaftLog;

	public class InFlightLogEntryReader : IDisposable
	{
		 private readonly ReadableRaftLog _raftLog;
		 private readonly InFlightCache _inFlightCache;
		 private readonly bool _pruneAfterRead;

		 private RaftLogCursor _cursor;
		 private bool _useCache = true;

		 public InFlightLogEntryReader( ReadableRaftLog raftLog, InFlightCache inFlightCache, bool pruneAfterRead )
		 {
			  this._raftLog = raftLog;
			  this._inFlightCache = inFlightCache;
			  this._pruneAfterRead = pruneAfterRead;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Neo4Net.causalclustering.core.consensus.log.RaftLogEntry get(long logIndex) throws java.io.IOException
		 public virtual RaftLogEntry Get( long logIndex )
		 {
			  RaftLogEntry entry = null;

			  if ( _useCache )
			  {
					entry = _inFlightCache.get( logIndex );
			  }

			  if ( entry == null )
			  {
					/*
					 * N.B.
					 * This fallback code is strictly necessary since getUsingCursor() requires
					 * that entries are accessed in strictly increasing order using a single cursor.
					 */
					_useCache = false;
					entry = GetUsingCursor( logIndex );
			  }

			  if ( _pruneAfterRead )
			  {
					_inFlightCache.prune( logIndex );
			  }

			  return entry;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Neo4Net.causalclustering.core.consensus.log.RaftLogEntry getUsingCursor(long logIndex) throws java.io.IOException
		 private RaftLogEntry GetUsingCursor( long logIndex )
		 {
			  if ( _cursor == null )
			  {
					_cursor = _raftLog.getEntryCursor( logIndex );
			  }

			  if ( _cursor.next() )
			  {
					if ( _cursor.index() != logIndex )
					{
						 throw new System.InvalidOperationException( format( "expected index %d but was %s", logIndex, _cursor.index() ) );
					}
					return _cursor.get();
			  }
			  else
			  {
					return null;
			  }
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
	}

}