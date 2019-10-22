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
namespace Neo4Net.causalclustering.core.consensus.log
{


	using Neo4Net.Helpers.Collections;
	using LogPosition = Neo4Net.Kernel.impl.transaction.log.LogPosition;

	public class RaftLogMetadataCache
	{
		 private readonly LruCache<long, RaftLogEntryMetadata> _raftLogEntryCache;

		 public RaftLogMetadataCache( int logEntryCacheSize )
		 {
			  this._raftLogEntryCache = new LruCache<long, RaftLogEntryMetadata>( "Raft log entry cache", logEntryCacheSize );
		 }

		 public virtual void Clear()
		 {
			  _raftLogEntryCache.clear();
		 }

		 /// Returns the metadata for the entry at position {<param name="logIndex">}, null if the metadata is not present in the cache </param>
		 public virtual RaftLogEntryMetadata GetMetadata( long logIndex )
		 {
			  return _raftLogEntryCache.get( logIndex );
		 }

		 public virtual RaftLogEntryMetadata CacheMetadata( long logIndex, long entryTerm, LogPosition position )
		 {
			  RaftLogEntryMetadata result = new RaftLogEntryMetadata( entryTerm, position );
			  _raftLogEntryCache.put( logIndex, result );
			  return result;
		 }

		 public virtual void RemoveUpTo( long upTo )
		 {
			  Remove( key => key <= upTo );
		 }

		 public virtual void RemoveUpwardsFrom( long startingFrom )
		 {
			  Remove( key => key >= startingFrom );
		 }

		 private void Remove( System.Func<long, bool> predicate )
		 {
			  _raftLogEntryCache.Keys.removeIf( predicate.test );
		 }

		 public class RaftLogEntryMetadata
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly long EntryTermConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly LogPosition StartPositionConflict;

			  public RaftLogEntryMetadata( long entryTerm, LogPosition startPosition )
			  {
					Objects.requireNonNull( startPosition );
					this.EntryTermConflict = entryTerm;
					this.StartPositionConflict = startPosition;
			  }

			  public virtual long EntryTerm
			  {
				  get
				  {
						return EntryTermConflict;
				  }
			  }

			  public virtual LogPosition StartPosition
			  {
				  get
				  {
						return StartPositionConflict;
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

					RaftLogEntryMetadata that = ( RaftLogEntryMetadata ) o;

					if ( EntryTermConflict != that.EntryTermConflict )
					{
						 return false;
					}
					return StartPositionConflict.Equals( that.StartPositionConflict );

			  }

			  public override int GetHashCode()
			  {
					int result = ( int )( EntryTermConflict ^ ( ( long )( ( ulong )EntryTermConflict >> 32 ) ) );
					result = 31 * result + StartPositionConflict.GetHashCode();
					return result;
			  }

			  public override string ToString()
			  {
					return "RaftLogEntryMetadata{" +
							  "entryTerm=" + EntryTermConflict +
							  ", startPosition=" + StartPositionConflict +
							  '}';
			  }
		 }
	}

}