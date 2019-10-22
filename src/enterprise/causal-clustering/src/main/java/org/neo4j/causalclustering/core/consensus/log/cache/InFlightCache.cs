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
namespace Neo4Net.causalclustering.core.consensus.log.cache
{

	/// <summary>
	/// A cache for in-flight entries which also tracks the size of the cache.
	/// </summary>
	public interface InFlightCache
	{
		 /// <summary>
		 /// Enables the cache.
		 /// </summary>
		 void Enable();

		 /// <summary>
		 /// Put item into the cache.
		 /// </summary>
		 /// <param name="logIndex"> the index of the log entry. </param>
		 /// <param name="entry"> the Raft log entry. </param>
		 void Put( long logIndex, RaftLogEntry entry );

		 /// <summary>
		 /// Get item from the cache.
		 /// </summary>
		 /// <param name="logIndex"> the index of the log entry. </param>
		 /// <returns> the log entry. </returns>
		 RaftLogEntry Get( long logIndex );

		 /// <summary>
		 /// Disposes of a range of elements from the tail of the consecutive cache.
		 /// </summary>
		 /// <param name="fromIndex"> the index to start from (inclusive). </param>
		 void Truncate( long fromIndex );

		 /// <summary>
		 /// Prunes items from the cache.
		 /// </summary>
		 /// <param name="upToIndex"> the last index to prune (inclusive). </param>
		 void Prune( long upToIndex );

		 /// <returns> the amount of data in the cache. </returns>
		 long TotalBytes();

		 /// <returns> the number of log entries in the cache. </returns>
		 int ElementCount();
	}

}