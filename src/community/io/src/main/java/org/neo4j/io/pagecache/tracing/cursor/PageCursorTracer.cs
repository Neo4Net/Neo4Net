﻿/*
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
namespace Neo4Net.Io.pagecache.tracing.cursor
{

	/// <summary>
	/// Event tracer for page cursors.
	/// 
	/// Performs event tracing related to particular page cursors and expose simple counters around them.
	/// Since events of each particular page cursor are part of whole page cache events, each particular page cursor
	/// tracer will eventually report them to global page cache counters/tracers.
	/// </summary>
	/// <seealso cref= PageCursorTracer </seealso>
	public interface PageCursorTracer : PageCursorCounters
	{

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 PageCursorTracer NULL = new PageCursorTracer()
	//	 {
	//		  @@Override public long faults()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public long pins()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public long unpins()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public long hits()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public long bytesRead()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public long evictions()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public long evictionExceptions()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public long bytesWritten()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public long flushes()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public double hitRatio()
	//		  {
	//				return 0d;
	//		  }
	//
	//		  @@Override public PinEvent beginPin(boolean writeLock, long filePageId, PageSwapper swapper)
	//		  {
	//				return PinEvent.NULL;
	//		  }
	//
	//		  @@Override public void init(PageCacheTracer tracer)
	//		  {
	//
	//		  }
	//
	//		  @@Override public void reportEvents()
	//		  {
	//
	//		  }
	//
	//		  @@Override public long accumulatedHits()
	//		  {
	//				return 0;
	//		  }
	//
	//		  @@Override public long accumulatedFaults()
	//		  {
	//				return 0;
	//		  }
	//	 };

		 PinEvent BeginPin( bool writeLock, long filePageId, PageSwapper swapper );

		 /// <summary>
		 /// Initialize page cursor tracer with required context dependent values. </summary>
		 /// <param name="tracer"> page cache tracer </param>
		 void Init( PageCacheTracer tracer );

		 /// <summary>
		 /// Report to global page cache tracer events observed by current page cursor tracer.
		 /// As soon as any event will be reported, page cursor tracer reset corresponding counters and completely forgets
		 /// about all of them except for accumulated counterparts.
		 /// </summary>
		 void ReportEvents();

		 /// <summary>
		 /// Accumulated number of hits that tracer observed over all reporting cycles.
		 /// In counterpart to hits metric that reset each time when reporting cycle is over </summary>
		 /// <returns> accumulated number of hits </returns>
		 long AccumulatedHits();

		 /// <summary>
		 /// Accumulated number of faults that tracer observed over all reporting cycles.
		 /// In counterpart to faults metric that reset each time when reporting cycle is over </summary>
		 /// <returns> accumulated number of faults </returns>
		 long AccumulatedFaults();

	}

}