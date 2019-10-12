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

	using PageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;

	public class LinearTracers
	{
		 private readonly LinearHistoryPageCacheTracer _pageCacheTracer;
		 private readonly PageCursorTracerSupplier _cursorTracerSupplier;
		 private readonly LinearHistoryTracer _tracer;

		 internal LinearTracers( LinearHistoryPageCacheTracer pageCacheTracer, PageCursorTracerSupplier cursorTracerSupplier, LinearHistoryTracer tracer )
		 {
			  this._pageCacheTracer = pageCacheTracer;
			  this._cursorTracerSupplier = cursorTracerSupplier;
			  this._tracer = tracer;
		 }

		 public virtual LinearHistoryPageCacheTracer PageCacheTracer
		 {
			 get
			 {
				  return _pageCacheTracer;
			 }
		 }

		 public virtual PageCursorTracerSupplier CursorTracerSupplier
		 {
			 get
			 {
				  return _cursorTracerSupplier;
			 }
		 }

		 public virtual void PrintHistory( PrintStream err )
		 {
			  _tracer.printHistory( err );
		 }

		 public virtual void ProcessHistory( System.Action<HEvents.HEvent> processor )
		 {
			  _tracer.processHistory( processor );
		 }
	}

}