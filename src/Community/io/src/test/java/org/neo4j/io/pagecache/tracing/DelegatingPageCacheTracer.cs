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
namespace Neo4Net.Io.pagecache.tracing
{

	/// <summary>
	/// A PageCacheTracer that delegates all calls to a wrapped instance.
	/// 
	/// Useful for overriding specific functionality in a sub-class.
	/// </summary>
	public class DelegatingPageCacheTracer : PageCacheTracer
	{
		 private readonly PageCacheTracer @delegate;

		 public DelegatingPageCacheTracer( PageCacheTracer @delegate )
		 {
			  this.@delegate = @delegate;
		 }

		 public override void MappedFile( File file )
		 {
			  @delegate.MappedFile( file );
		 }

		 public override long BytesRead()
		 {
			  return @delegate.BytesRead();
		 }

		 public override MajorFlushEvent BeginFileFlush( PageSwapper swapper )
		 {
			  return @delegate.BeginFileFlush( swapper );
		 }

		 public override EvictionRunEvent BeginPageEvictions( int pageCountToEvict )
		 {
			  return @delegate.BeginPageEvictions( pageCountToEvict );
		 }

		 public override long Unpins()
		 {
			  return @delegate.Unpins();
		 }

		 public override long Hits()
		 {
			  return @delegate.Hits();
		 }

		 public override MajorFlushEvent BeginCacheFlush()
		 {
			  return @delegate.BeginCacheFlush();
		 }

		 public override long BytesWritten()
		 {
			  return @delegate.BytesWritten();
		 }

		 public override long Pins()
		 {
			  return @delegate.Pins();
		 }

		 public override long FilesUnmapped()
		 {
			  return @delegate.FilesUnmapped();
		 }

		 public override void UnmappedFile( File file )
		 {
			  @delegate.UnmappedFile( file );
		 }

		 public override long EvictionExceptions()
		 {
			  return @delegate.EvictionExceptions();
		 }

		 public override double HitRatio()
		 {
			  return @delegate.HitRatio();
		 }

		 public override double UsageRatio()
		 {
			  return @delegate.UsageRatio();
		 }

		 public override void Pins( long pins )
		 {
			  @delegate.Pins( pins );
		 }

		 public override void Unpins( long unpins )
		 {
			  @delegate.Unpins( unpins );
		 }

		 public override void Hits( long hits )
		 {
			  @delegate.Hits( hits );
		 }

		 public override void Faults( long faults )
		 {
			  @delegate.Faults( faults );
		 }

		 public override void BytesRead( long bytesRead )
		 {
			  @delegate.BytesRead( bytesRead );
		 }

		 public override void Evictions( long evictions )
		 {
			  @delegate.Evictions( evictions );
		 }

		 public override void EvictionExceptions( long evictionExceptions )
		 {
			  @delegate.EvictionExceptions( evictionExceptions );
		 }

		 public override void BytesWritten( long bytesWritten )
		 {
			  @delegate.BytesWritten( bytesWritten );
		 }

		 public override void Flushes( long flushes )
		 {
			  @delegate.Flushes( flushes );
		 }

		 public override void MaxPages( long maxPages )
		 {
			  @delegate.MaxPages( maxPages );
		 }

		 public override long FilesMapped()
		 {
			  return @delegate.FilesMapped();
		 }

		 public override long Flushes()
		 {
			  return @delegate.Flushes();
		 }

		 public override long Faults()
		 {
			  return @delegate.Faults();
		 }

		 public override long Evictions()
		 {
			  return @delegate.Evictions();
		 }
	}

}