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

	/// <summary>
	/// Caches (offsetIndex) -> (byteOffset) mappings, which can be used to find an exact or
	/// approximate byte position for an entry given an index. The index is defined as a relative
	/// offset index starting from 0 for each segment, instead of the absolute logIndex in the
	/// log file.
	/// 
	/// The necessity and efficiency of this cache is understood by considering the values put into
	/// it. When closing cursors the position after the last entry is cached so that when the next
	/// batch of entries are to be read the position is already known.
	/// </summary>
	internal class PositionCache
	{
		 private static readonly LogPosition _beginningOfRecords = new LogPosition( 0, SegmentHeader.Size );
		 internal const int CACHE_SIZE = 8;

		 private LogPosition[] _cache = new LogPosition[CACHE_SIZE];
		 private int _pos;

		 internal PositionCache()
		 {
			  for ( int i = 0; i < _cache.Length; i++ )
			  {
					_cache[i] = _beginningOfRecords;
			  }
		 }

		 /// <summary>
		 /// Saves a known position in the cache.
		 /// </summary>
		 /// <param name="position"> The position which should interpreted as (offsetIndex, byteOffset). </param>
		 public virtual void Put( LogPosition position )
		 {
			 lock ( this )
			 {
				  _cache[_pos] = position;
				  _pos = ( _pos + 1 ) % CACHE_SIZE;
			 }
		 }

		 /// <summary>
		 /// Returns a position at or before the searched offsetIndex, the closest known.
		 /// Users will have to scan forward to reach the exact position.
		 /// </summary>
		 /// <param name="offsetIndex"> The relative index. </param>
		 /// <returns> A position at or before the searched offsetIndex. </returns>
		 internal virtual LogPosition Lookup( long offsetIndex )
		 {
			 lock ( this )
			 {
				  if ( offsetIndex == 0 )
				  {
						return _beginningOfRecords;
				  }
      
				  LogPosition best = _beginningOfRecords;
      
				  for ( int i = 0; i < CACHE_SIZE; i++ )
				  {
						if ( _cache[i].logIndex <= offsetIndex && _cache[i].logIndex > best.LogIndex )
						{
							 best = _cache[i];
						}
				  }
      
				  return best;
			 }
		 }
	}

}