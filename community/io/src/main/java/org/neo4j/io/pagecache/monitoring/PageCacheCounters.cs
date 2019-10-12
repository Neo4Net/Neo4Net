/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Io.pagecache.monitoring
{
	/// <summary>
	/// The PageCacheCounters exposes internal counters from the page cache.
	/// The data for these counters is sourced through the PageCacheTracer API.
	/// </summary>
	public interface PageCacheCounters
	{
		 /// <returns> The number of page faults observed thus far. </returns>
		 long Faults();

		 /// <returns> The number of page evictions observed thus far. </returns>
		 long Evictions();

		 /// <returns> The number of page pins observed thus far. </returns>
		 long Pins();

		 /// <returns> The number of page unpins observed thus far. </returns>
		  long Unpins();

		 /// <returns> The number of page cache hits so far. </returns>
		 long Hits();

		 /// <returns> The number of page flushes observed thus far. </returns>
		 long Flushes();

		 /// <returns> The sum total of bytes read in through page faults thus far. </returns>
		 long BytesRead();

		 /// <returns> The sum total of bytes written through flushes thus far. </returns>
		 long BytesWritten();

		 /// <returns> The number of file mappings observed thus far. </returns>
		 long FilesMapped();

		 /// <returns> The number of file unmappings observed thus far. </returns>
		 long FilesUnmapped();

		 /// <returns> The number of page evictions that have thrown exceptions thus far. </returns>
		 long EvictionExceptions();

		 /// <returns> The cache hit ratio observed thus far. </returns>
		 double HitRatio();

		 /// <returns> The current usage ration of number of used pages to the total number of pages or {@code NaN} if it cannot
		 /// be determined. </returns>
		 double UsageRatio();
	}

}