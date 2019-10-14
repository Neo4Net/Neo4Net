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
namespace Neo4Net.Io.pagecache.tracing.recording
{


	public class RecordingPageCacheTracer : RecordingTracer, PageCacheTracer
	{
		 private AtomicLong _pins = new AtomicLong();
		 private AtomicLong _faults = new AtomicLong();
		 private AtomicLong _evictions = new AtomicLong();

		 public RecordingPageCacheTracer() : base(typeof(Evict))
		 {
		 }

		 public override void MappedFile( File file )
		 {
			  // we currently do not record these
		 }

		 public override void UnmappedFile( File file )
		 {
			  // we currently do not record these
		 }

		 public override EvictionRunEvent BeginPageEvictions( int pageCountToEvict )
		 {
			  return new EvictionRunEventAnonymousInnerClass( this );
		 }

		 private class EvictionRunEventAnonymousInnerClass : EvictionRunEvent
		 {
			 private readonly RecordingPageCacheTracer _outerInstance;

			 public EvictionRunEventAnonymousInnerClass( RecordingPageCacheTracer outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public EvictionEvent beginEviction()
			 {
				  return new RecordingEvictionEvent( _outerInstance );
			 }

			 public void close()
			 {
			 }
		 }

		 public override MajorFlushEvent BeginFileFlush( PageSwapper swapper )
		 {
			  return MajorFlushEvent.NULL;
		 }

		 public override MajorFlushEvent BeginCacheFlush()
		 {
			  return MajorFlushEvent.NULL;
		 }

		 public override long Faults()
		 {
			  return _faults.get();
		 }

		 public override long Pins()
		 {
			  return _pins.get();
		 }

		 public override long Evictions()
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
			  this._pins.getAndAdd( pins );
		 }

		 public override void Unpins( long unpins )
		 {
		 }

		 public override void Hits( long hits )
		 {
		 }

		 public override void Faults( long faults )
		 {
			  this._faults.getAndAdd( faults );
		 }

		 public override void BytesRead( long bytesRead )
		 {
		 }

		 public override void Evictions( long evictions )
		 {
			  this._evictions.getAndAdd( evictions );
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

		 private void Evicted( long filePageId, PageSwapper swapper )
		 {
			  Record( new Evict( swapper, filePageId ) );
		 }

		 public class Evict : Event
		 {
			  internal Evict( PageSwapper io, long pageId ) : base( io, pageId )
			  {
			  }
		 }

		 private class RecordingEvictionEvent : EvictionEvent
		 {
			 private readonly RecordingPageCacheTracer _outerInstance;

			 public RecordingEvictionEvent( RecordingPageCacheTracer outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long FilePageIdConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal PageSwapper SwapperConflict;

			  public virtual long FilePageId
			  {
				  set
				  {
   
						this.FilePageIdConflict = value;
				  }
			  }

			  public virtual PageSwapper Swapper
			  {
				  set
				  {
						this.SwapperConflict = value;
				  }
			  }

			  public override FlushEventOpportunity FlushEventOpportunity()
			  {
					return Neo4Net.Io.pagecache.tracing.FlushEventOpportunity_Fields.Null;
			  }

			  public override void ThrewException( IOException exception )
			  {
			  }

			  public virtual long CachePageId
			  {
				  set
				  {
				  }
			  }

			  public override void Close()
			  {
					outerInstance.evicted( FilePageIdConflict, SwapperConflict );
			  }
		 }
	}

}