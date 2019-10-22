using System;

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

	using PageCursorTracer = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracer;

	/// <summary>
	/// Recording tracer of page cursor events.
	/// Records and counts number of <seealso cref="Pin"/> and <seealso cref="Fault"/> events.
	/// Propagate those counters to global page cache tracer during event reporting.
	/// </summary>
	public class RecordingPageCursorTracer : RecordingTracer, PageCursorTracer
	{

		 private int _pins;
		 private int _faults;
		 private PageCacheTracer _tracer;

		 public RecordingPageCursorTracer() : base(typeof(Pin), typeof(Fault))
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs public RecordingPageCursorTracer(Class... eventTypesToTrace)
		 public RecordingPageCursorTracer( params Type[] eventTypesToTrace ) : base( eventTypesToTrace )
		 {
		 }

		 public override long Faults()
		 {
			  return _faults;
		 }

		 public override long Pins()
		 {
			  return _pins;
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

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.Neo4Net.io.pagecache.tracing.PinEvent beginPin(boolean writeLock, final long filePageId, final org.Neo4Net.io.pagecache.PageSwapper swapper)
		 public override PinEvent BeginPin( bool writeLock, long filePageId, PageSwapper swapper )
		 {
			  return new PinEventAnonymousInnerClass( this, filePageId, swapper );
		 }

		 private class PinEventAnonymousInnerClass : PinEvent
		 {
			 private readonly RecordingPageCursorTracer _outerInstance;

			 private long _filePageId;
			 private PageSwapper _swapper;

			 public PinEventAnonymousInnerClass( RecordingPageCursorTracer outerInstance, long filePageId, PageSwapper swapper )
			 {
				 this.outerInstance = outerInstance;
				 this._filePageId = filePageId;
				 this._swapper = swapper;
				 hit = true;
			 }

			 private bool hit;

			 public long CachePageId
			 {
				 set
				 {
				 }
			 }

			 public PageFaultEvent beginPageFault()
			 {
				  hit = false;
				  return new PageFaultEventAnonymousInnerClass( this );
			 }

			 private class PageFaultEventAnonymousInnerClass : PageFaultEvent
			 {
				 private readonly PinEventAnonymousInnerClass _outerInstance;

				 public PageFaultEventAnonymousInnerClass( PinEventAnonymousInnerClass outerInstance )
				 {
					 this.outerInstance = outerInstance;
				 }

				 public void addBytesRead( long bytes )
				 {
				 }

				 public void done()
				 {
					  _outerInstance.outerInstance.pageFaulted( _outerInstance.filePageId, _outerInstance.swapper );
				 }

				 public void done( Exception throwable )
				 {
				 }

				 public EvictionEvent beginEviction()
				 {
					  return EvictionEvent.NULL;
				 }

				 public long CachePageId
				 {
					 set
					 {
					 }
				 }
			 }

			 public void hit()
			 {
			 }

			 public void done()
			 {
				  outerInstance.pinned( _filePageId, _swapper, hit );
			 }
		 }

		 public override void Init( PageCacheTracer tracer )
		 {
			  this._tracer = tracer;
		 }

		 public override void ReportEvents()
		 {
			  Objects.requireNonNull( _tracer );
			  _tracer.pins( _pins );
			  _tracer.faults( _faults );
		 }

		 public override long AccumulatedHits()
		 {
			  return 0;
		 }

		 public override long AccumulatedFaults()
		 {
			  return 0;
		 }

		 private void PageFaulted( long filePageId, PageSwapper swapper )
		 {
			  _faults++;
			  Record( new Fault( swapper, filePageId ) );
		 }

		 private void Pinned( long filePageId, PageSwapper swapper, bool hit )
		 {
			  _pins++;
			  Record( new Pin( swapper, filePageId, hit ) );
		 }

		 public class Fault : Event
		 {
			  internal Fault( PageSwapper io, long pageId ) : base( io, pageId )
			  {
			  }
		 }

		 public class Pin : Event
		 {
			  internal bool Hit;

			  internal Pin( PageSwapper io, long pageId, bool hit ) : base( io, pageId )
			  {
					this.Hit = hit;
			  }

			  public override string ToString()
			  {
					return string.Format( "{0}{{io={1}, pageId={2},hit={3}}}", this.GetType().Name, Io, PageId, Hit );
			  }
		 }

	}

}