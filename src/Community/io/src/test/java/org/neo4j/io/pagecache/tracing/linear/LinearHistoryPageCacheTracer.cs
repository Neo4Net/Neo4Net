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


	using static Neo4Net.Io.pagecache.tracing.linear.HEvents.EvictionRunHEvent;
	using static Neo4Net.Io.pagecache.tracing.linear.HEvents.MajorFlushHEvent;
	using static Neo4Net.Io.pagecache.tracing.linear.HEvents.MappedFileHEvent;
	using static Neo4Net.Io.pagecache.tracing.linear.HEvents.UnmappedFileHEvent;

	/// <summary>
	/// Tracer for global page cache events that add all of them to event history tracer that can build proper linear
	/// history across all tracers.
	/// Only use this for debugging internal data race bugs and the like, in the page cache. </summary>
	/// <seealso cref= HEvents </seealso>
	/// <seealso cref= LinearHistoryPageCursorTracer </seealso>
	public sealed class LinearHistoryPageCacheTracer : PageCacheTracer
	{

		 private LinearHistoryTracer _tracer;

		 internal LinearHistoryPageCacheTracer( LinearHistoryTracer tracer )
		 {
			  this._tracer = tracer;
		 }

		 public override void MappedFile( File file )
		 {
			  _tracer.add( new MappedFileHEvent( file ) );
		 }

		 public override void UnmappedFile( File file )
		 {
			  _tracer.add( new UnmappedFileHEvent( file ) );
		 }

		 public override EvictionRunEvent BeginPageEvictions( int pageCountToEvict )
		 {
			  return _tracer.add( new EvictionRunHEvent( _tracer, pageCountToEvict ) );
		 }

		 public override MajorFlushEvent BeginFileFlush( PageSwapper swapper )
		 {
			  return _tracer.add( new MajorFlushHEvent( _tracer, swapper.File() ) );
		 }

		 public override MajorFlushEvent BeginCacheFlush()
		 {
			  return _tracer.add( new MajorFlushHEvent( _tracer, null ) );
		 }

		 public override long Faults()
		 {
			  return 0;
		 }

		 public override long Evictions()
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

		 public override long Flushes()
		 {
			  return 0;
		 }

		 public override long BytesRead()
		 {
			  return 0;
		 }

		 public override long BytesWritten()
		 {
			  return 0;
		 }

		 public override long FilesMapped()
		 {
			  return 0;
		 }

		 public override long FilesUnmapped()
		 {
			  return 0;
		 }

		 public override long EvictionExceptions()
		 {
			  return 0;
		 }

		 public override double HitRatio()
		 {
			  return 0d;
		 }

		 public override double UsageRatio()
		 {
			  return 0d;
		 }

		 public override void Pins( long pins )
		 {
		 }

		 public override void Unpins( long unpins )
		 {
		 }

		 public override void Hits( long hits )
		 {
		 }

		 public override void Faults( long faults )
		 {
		 }

		 public override void BytesRead( long bytesRead )
		 {
		 }

		 public override void Evictions( long evictions )
		 {
		 }

		 public override void EvictionExceptions( long evictionExceptions )
		 {
		 }

		 public override void BytesWritten( long bytesWritten )
		 {
		 }

		 public override void Flushes( long flushes )
		 {
		 }

		 public override void MaxPages( long maxPages )
		 {
		 }
	}

}