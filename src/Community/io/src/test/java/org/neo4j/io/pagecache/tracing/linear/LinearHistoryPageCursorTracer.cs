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
namespace Neo4Net.Io.pagecache.tracing.linear
{
	using PageCursorTracer = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracer;

	/// <summary>
	/// Tracer for page cache cursor events that add all of them to event history tracer that can build proper linear
	/// history across all tracers.
	/// Only use this for debugging internal data race bugs and the like, in the page cache.
	/// </summary>
	/// <seealso cref= HEvents </seealso>
	/// <seealso cref= LinearHistoryPageCacheTracer </seealso>
	public class LinearHistoryPageCursorTracer : PageCursorTracer
	{
		 private LinearHistoryTracer _tracer;

		 internal LinearHistoryPageCursorTracer( LinearHistoryTracer tracer )
		 {
			  this._tracer = tracer;
		 }

		 public override long Faults()
		 {
			  return 0;
		 }

		 public override long Pins()
		 {
			  return 0;
		 }

		 public override long Unpins()
		 {
			  return 0;
		 }

		 public override long Hits()
		 {
			  return 0;
		 }

		 public override long BytesRead()
		 {
			  return 0;
		 }

		 public override long Evictions()
		 {
			  return 0;
		 }

		 public override long EvictionExceptions()
		 {
			  return 0;
		 }

		 public override long BytesWritten()
		 {
			  return 0;
		 }

		 public override long Flushes()
		 {
			  return 0;
		 }

		 public override double HitRatio()
		 {
			  return 0d;
		 }

		 public override PinEvent BeginPin( bool writeLock, long filePageId, PageSwapper swapper )
		 {
			  return _tracer.add( new HEvents.PinHEvent( _tracer, writeLock, filePageId, swapper ) );
		 }

		 public override void Init( PageCacheTracer tracer )
		 {
			  // nothing to do
		 }

		 public override void ReportEvents()
		 {
			  // nothing to do
		 }

		 public override long AccumulatedHits()
		 {
			  return 0;
		 }

		 public override long AccumulatedFaults()
		 {
			  return 0;
		 }
	}

}