/*
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
namespace Neo4Net.Io.pagecache.tracing
{

	using MathUtil = Neo4Net.Helpers.MathUtil;

	/// <summary>
	/// The default PageCacheTracer implementation, that just increments counters.
	/// </summary>
	public class DefaultPageCacheTracer : PageCacheTracer
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly LongAdder FaultsConflict = new LongAdder();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly LongAdder EvictionsConflict = new LongAdder();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly LongAdder PinsConflict = new LongAdder();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly LongAdder UnpinsConflict = new LongAdder();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly LongAdder HitsConflict = new LongAdder();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly LongAdder FlushesConflict = new LongAdder();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly LongAdder BytesReadConflict = new LongAdder();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly LongAdder BytesWrittenConflict = new LongAdder();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly LongAdder FilesMappedConflict = new LongAdder();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly LongAdder FilesUnmappedConflict = new LongAdder();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly LongAdder EvictionExceptionsConflict = new LongAdder();
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly AtomicLong MaxPagesConflict = new AtomicLong();

		 private readonly FlushEvent flushEvent = new FlushEventAnonymousInnerClass();

		 private class FlushEventAnonymousInnerClass : FlushEvent
		 {
			 public void addBytesWritten( long bytes )
			 {
				  outerInstance.bytesWritten.add( bytes );
			 }

			 public void done()
			 {
				  outerInstance.flushes.increment();
			 }

			 public void done( IOException exception )
			 {
				  done();
			 }

			 public void addPagesFlushed( int pageCount )
			 {
			 }
		 }

		 private readonly FlushEventOpportunity _flushEventOpportunity = ( filePageId, cachePageId, swapper ) => flushEvent;

		 private readonly EvictionEvent evictionEvent = new EvictionEventAnonymousInnerClass();

		 private class EvictionEventAnonymousInnerClass : EvictionEvent
		 {
			 public long FilePageId
			 {
				 set
				 {
				 }
			 }

			 public PageSwapper Swapper
			 {
				 set
				 {
				 }
			 }

			 public FlushEventOpportunity flushEventOpportunity()
			 {
				  return outerInstance.flushEventOpportunity;
			 }

			 public void threwException( IOException exception )
			 {
				  outerInstance.evictionExceptions.increment();
			 }

			 public long CachePageId
			 {
				 set
				 {
				 }
			 }

			 public void close()
			 {
				  outerInstance.evictions.increment();
			 }
		 }

		 private readonly EvictionRunEvent evictionRunEvent = new EvictionRunEventAnonymousInnerClass();

		 private class EvictionRunEventAnonymousInnerClass : EvictionRunEvent
		 {
			 public EvictionEvent beginEviction()
			 {
				  return evictionEvent;
			 }

			 public void close()
			 {
			 }
		 }

		 private readonly MajorFlushEvent majorFlushEvent = new MajorFlushEventAnonymousInnerClass();

		 private class MajorFlushEventAnonymousInnerClass : MajorFlushEvent
		 {
			 public FlushEventOpportunity flushEventOpportunity()
			 {
				  return outerInstance.flushEventOpportunity;
			 }

			 public void close()
			 {
			 }
		 }

		 public override void MappedFile( File file )
		 {
			  FilesMappedConflict.increment();
		 }

		 public override void UnmappedFile( File file )
		 {
			  FilesUnmappedConflict.increment();
		 }

		 public override EvictionRunEvent BeginPageEvictions( int pageCountToEvict )
		 {
			  return evictionRunEvent;
		 }

		 public override MajorFlushEvent BeginFileFlush( PageSwapper swapper )
		 {
			  return majorFlushEvent;
		 }

		 public override MajorFlushEvent BeginCacheFlush()
		 {
			  return majorFlushEvent;
		 }

		 public override long Faults()
		 {
			  return FaultsConflict.sum();
		 }

		 public override long Evictions()
		 {
			  return EvictionsConflict.sum();
		 }

		 public override long Pins()
		 {
			  return PinsConflict.sum();
		 }

		 public override long Unpins()
		 {
			  return UnpinsConflict.sum();
		 }

		 public override long Hits()
		 {
			  return HitsConflict.sum();
		 }

		 public override long Flushes()
		 {
			  return FlushesConflict.sum();
		 }

		 public override long BytesRead()
		 {
			  return BytesReadConflict.sum();
		 }

		 public override long BytesWritten()
		 {
			  return BytesWrittenConflict.sum();
		 }

		 public override long FilesMapped()
		 {
			  return FilesMappedConflict.sum();
		 }

		 public override long FilesUnmapped()
		 {
			  return FilesUnmappedConflict.sum();
		 }

		 public override long EvictionExceptions()
		 {
			  return EvictionExceptionsConflict.sum();
		 }

		 public override double HitRatio()
		 {
			  return MathUtil.portion( Hits(), Faults() );
		 }

		 public override double UsageRatio()
		 {
			  return ( FaultsConflict.sum() - EvictionsConflict.sum() ) / (double) MaxPagesConflict.get();
		 }

		 public override void Pins( long pins )
		 {
			  this.PinsConflict.add( pins );
		 }

		 public override void Unpins( long unpins )
		 {
			  this.UnpinsConflict.add( unpins );
		 }

		 public override void Hits( long hits )
		 {
			  this.HitsConflict.add( hits );
		 }

		 public override void Faults( long faults )
		 {
			  this.FaultsConflict.add( faults );
		 }

		 public override void BytesRead( long bytesRead )
		 {
			  this.BytesReadConflict.add( bytesRead );
		 }

		 public override void Evictions( long evictions )
		 {
			  this.EvictionsConflict.add( evictions );
		 }

		 public override void EvictionExceptions( long evictionExceptions )
		 {
			  this.EvictionExceptionsConflict.add( evictionExceptions );
		 }

		 public override void BytesWritten( long bytesWritten )
		 {
			  this.BytesWrittenConflict.add( bytesWritten );
		 }

		 public override void Flushes( long flushes )
		 {
			  this.FlushesConflict.add( flushes );
		 }

		 public override void MaxPages( long maxPages )
		 {
			  this.MaxPagesConflict.set( maxPages );
		 }
	}

}