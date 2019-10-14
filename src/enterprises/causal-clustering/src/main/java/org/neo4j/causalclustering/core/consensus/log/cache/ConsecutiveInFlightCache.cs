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
namespace Neo4Net.causalclustering.core.consensus.log.cache
{
	using CoreReplicatedContent = Neo4Net.causalclustering.core.state.machines.tx.CoreReplicatedContent;

	/// <summary>
	/// A cache that keeps Raft log entries in memory, generally to bridge the gap
	/// between the time that a Raft log is being appended to the local Raft log and
	/// at a later time applied to the store. This cache optimises for the up-to-date
	/// case and does not cater specifically for lagging followers. It is better to let
	/// those catch up from entries read from disk where possible.
	/// <para>
	/// The cache relies on highly efficient underlying data structures (a circular
	/// buffer) and also allows on to specify a maximum bound on the number of entries
	/// as well as their total size where known, see <seealso cref="CoreReplicatedContent.size()"/> ()}.
	/// </para>
	/// </summary>
	public class ConsecutiveInFlightCache : InFlightCache
	{
		 private readonly ConsecutiveCache<RaftLogEntry> _cache;
		 private readonly RaftLogEntry[] _evictions;
		 private readonly InFlightCacheMonitor _monitor;

		 private long _totalBytes;
		 private long _maxBytes;
		 private bool _enabled;

		 public ConsecutiveInFlightCache() : this(1024, 8 * 1024 * 1024, InFlightCacheMonitor.VOID, true)
		 {
		 }

		 public ConsecutiveInFlightCache( int capacity, long maxBytes, InFlightCacheMonitor monitor, bool enabled )
		 {
			  this._cache = new ConsecutiveCache<RaftLogEntry>( capacity );
			  this._evictions = new RaftLogEntry[capacity];

			  this._maxBytes = maxBytes;
			  this._monitor = monitor;
			  this._enabled = enabled;

			  monitor.MaxBytes = maxBytes;
			  monitor.MaxElements = capacity;
		 }

		 public override void Enable()
		 {
			 lock ( this )
			 {
				  _enabled = true;
			 }
		 }

		 public override void Put( long logIndex, RaftLogEntry entry )
		 {
			 lock ( this )
			 {
				  if ( !_enabled )
				  {
						return;
				  }
      
				  _totalBytes += SizeOf( entry );
				  _cache.put( logIndex, entry, _evictions );
				  ProcessEvictions();
      
				  while ( _totalBytes > _maxBytes )
				  {
						RaftLogEntry evicted = _cache.remove();
						_totalBytes -= SizeOf( evicted );
				  }
			 }
		 }

		 public override RaftLogEntry Get( long logIndex )
		 {
			 lock ( this )
			 {
				  if ( !_enabled )
				  {
						return null;
				  }
      
				  RaftLogEntry entry = _cache.get( logIndex );
      
				  if ( entry == null )
				  {
						_monitor.miss();
				  }
				  else
				  {
						_monitor.hit();
				  }
      
				  return entry;
			 }
		 }

		 public override void Truncate( long fromIndex )
		 {
			 lock ( this )
			 {
				  if ( !_enabled )
				  {
						return;
				  }
      
				  _cache.truncate( fromIndex, _evictions );
				  ProcessEvictions();
			 }
		 }

		 public override void Prune( long upToIndex )
		 {
			 lock ( this )
			 {
				  if ( !_enabled )
				  {
						return;
				  }
      
				  _cache.prune( upToIndex, _evictions );
				  ProcessEvictions();
			 }
		 }

		 public override long TotalBytes()
		 {
			 lock ( this )
			 {
				  return _totalBytes;
			 }
		 }

		 public override int ElementCount()
		 {
			 lock ( this )
			 {
				  return _cache.size();
			 }
		 }

		 private long SizeOf( RaftLogEntry entry )
		 {
			  return entry.Content().size().GetValueOrDefault(0L);
		 }

		 private void ProcessEvictions()
		 {
			  for ( int i = 0; i < _evictions.Length; i++ )
			  {
					RaftLogEntry entry = _evictions[i];
					if ( entry == null )
					{
						 break;
					}
					_evictions[i] = null;
					_totalBytes -= SizeOf( entry );
			  }

			  _monitor.TotalBytes = _totalBytes;
			  _monitor.ElementCount = _cache.size();
		 }
	}

}