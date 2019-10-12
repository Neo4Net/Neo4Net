using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Neo4Net.causalclustering.core.consensus.log
{

	public class InMemoryRaftLog : RaftLog
	{
		 private readonly IDictionary<long, RaftLogEntry> _raftLog = new Dictionary<long, RaftLogEntry>();

		 private long _prevIndex = -1;
		 private long _prevTerm = -1;

		 private long _appendIndex = -1;
		 private long _commitIndex = -1;
		 private long _term = -1;

		 public override long Append( params RaftLogEntry[] entries )
		 {
			 lock ( this )
			 {
				  long newAppendIndex = _appendIndex;
				  foreach ( RaftLogEntry entry in entries )
				  {
						newAppendIndex = AppendSingle( entry );
				  }
				  return newAppendIndex;
			 }
		 }

		 private long AppendSingle( RaftLogEntry logEntry )
		 {
			 lock ( this )
			 {
				  Objects.requireNonNull( logEntry );
				  if ( logEntry.Term() >= _term )
				  {
						_term = logEntry.Term();
				  }
				  else
				  {
						throw new System.InvalidOperationException( string.Format( "Non-monotonic term {0:D} for in entry {1} in term {2:D}", logEntry.Term(), logEntry.ToString(), _term ) );
				  }
      
				  _appendIndex++;
				  _raftLog[_appendIndex] = logEntry;
				  return _appendIndex;
			 }
		 }

		 public override long Prune( long safeIndex )
		 {
			 lock ( this )
			 {
				  if ( safeIndex > _prevIndex )
				  {
						long removeIndex = _prevIndex + 1;
      
						_prevTerm = ReadEntryTerm( safeIndex );
						_prevIndex = safeIndex;
      
						do
						{
							 _raftLog.Remove( removeIndex );
							 removeIndex++;
						} while ( removeIndex <= safeIndex );
				  }
      
				  return _prevIndex;
			 }
		 }

		 public override long AppendIndex()
		 {
			 lock ( this )
			 {
				  return _appendIndex;
			 }
		 }

		 public override long PrevIndex()
		 {
			 lock ( this )
			 {
				  return _prevIndex;
			 }
		 }

		 public override long ReadEntryTerm( long logIndex )
		 {
			 lock ( this )
			 {
				  if ( logIndex == _prevIndex )
				  {
						return _prevTerm;
				  }
				  else if ( logIndex < _prevIndex || logIndex > _appendIndex )
				  {
						return -1;
				  }
				  return _raftLog[logIndex].term();
			 }
		 }

		 public override void Truncate( long fromIndex )
		 {
			 lock ( this )
			 {
				  if ( fromIndex <= _commitIndex )
				  {
						throw new System.ArgumentException( "cannot truncate before the commit index" );
				  }
				  else if ( fromIndex > _appendIndex )
				  {
						throw new System.ArgumentException( "Cannot truncate at index " + fromIndex + " when append index is " + _appendIndex );
				  }
				  else if ( fromIndex <= _prevIndex )
				  {
						_prevIndex = -1;
						_prevTerm = -1;
				  }
      
				  for ( long i = _appendIndex; i >= fromIndex; --i )
				  {
						_raftLog.Remove( i );
				  }
      
				  if ( _appendIndex >= fromIndex )
				  {
						_appendIndex = fromIndex - 1;
				  }
				  _term = ReadEntryTerm( _appendIndex );
			 }
		 }

		 public override RaftLogCursor GetEntryCursor( long fromIndex )
		 {
			 lock ( this )
			 {
				  return new RaftLogCursorAnonymousInnerClass( this, fromIndex );
			 }
		 }

		 private class RaftLogCursorAnonymousInnerClass : RaftLogCursor
		 {
			 private readonly InMemoryRaftLog _outerInstance;

			 private long _fromIndex;

			 public RaftLogCursorAnonymousInnerClass( InMemoryRaftLog outerInstance, long fromIndex )
			 {
				 this.outerInstance = outerInstance;
				 this._fromIndex = fromIndex;
				 currentIndex = fromIndex - 1;
			 }

			 private long currentIndex;
			 internal RaftLogEntry current;

			 public bool next()
			 {
				  currentIndex++;
				  bool hasNext;

				  lock ( _outerInstance )
				  {
						hasNext = currentIndex <= _outerInstance.appendIndex;
						if ( hasNext )
						{
							 if ( currentIndex <= _outerInstance.prevIndex || currentIndex > _outerInstance.appendIndex )
							 {
								  return false;
							 }
							 current = _outerInstance.raftLog[currentIndex];
						}
						else
						{
							 current = null;
						}
				  }
				  return hasNext;
			 }

			 public void close()
			 {
			 }

			 public long index()
			 {
				  return currentIndex;
			 }

			 public RaftLogEntry get()
			 {
				  return current;
			 }
		 }

		 public override long Skip( long index, long term )
		 {
			 lock ( this )
			 {
				  if ( index > _appendIndex )
				  {
						_raftLog.Clear();
      
						_appendIndex = index;
						_prevIndex = index;
						_prevTerm = term;
				  }
				  return _appendIndex;
			 }
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }
			  InMemoryRaftLog that = ( InMemoryRaftLog ) o;
			  return Objects.Equals( _appendIndex, that._appendIndex ) && Objects.Equals( _commitIndex, that._commitIndex ) && Objects.Equals( _term, that._term ) && Objects.Equals( _raftLog, that._raftLog );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _raftLog, _appendIndex, _commitIndex, _term );
		 }
	}

}