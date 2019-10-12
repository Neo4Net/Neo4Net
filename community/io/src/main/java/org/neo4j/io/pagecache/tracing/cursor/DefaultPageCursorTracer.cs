using System;

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
namespace Org.Neo4j.Io.pagecache.tracing.cursor
{

	using MathUtil = Org.Neo4j.Helpers.MathUtil;

	public class DefaultPageCursorTracer : PageCursorTracer
	{
		private bool InstanceFieldsInitialized = false;

		public DefaultPageCursorTracer()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_pinTracingEvent = new DefaultPinEvent( this );
		}

		 private long _pins;
		 private long _unpins;
		 private long _hits;
		 private long _historicalHits;
		 private long _faults;
		 private long _historicalFaults;
		 private long _bytesRead;
		 private long _bytesWritten;
		 private long _evictions;
		 private long _evictionExceptions;
		 private long _flushes;

		 private PageCacheTracer _pageCacheTracer = PageCacheTracer.NULL;
		 private DefaultPinEvent _pinTracingEvent;

		 public override void Init( PageCacheTracer pageCacheTracer )
		 {
			  this._pageCacheTracer = pageCacheTracer;
		 }

		 public override void ReportEvents()
		 {
			  if ( _pins > 0 )
			  {
					_pageCacheTracer.pins( _pins );
			  }
			  if ( _unpins > 0 )
			  {
					_pageCacheTracer.unpins( _unpins );
			  }
			  if ( _hits > 0 )
			  {
					_pageCacheTracer.hits( _hits );
					_historicalHits = _historicalHits + _hits;
			  }
			  if ( _faults > 0 )
			  {
					_pageCacheTracer.faults( _faults );
					_historicalFaults = _historicalFaults + _faults;
			  }
			  if ( _bytesRead > 0 )
			  {
					_pageCacheTracer.bytesRead( _bytesRead );
			  }
			  if ( _evictions > 0 )
			  {
					_pageCacheTracer.evictions( _evictions );
			  }
			  if ( _evictionExceptions > 0 )
			  {
					_pageCacheTracer.evictionExceptions( _evictionExceptions );
			  }
			  if ( _bytesWritten > 0 )
			  {
					_pageCacheTracer.bytesWritten( _bytesWritten );
			  }
			  if ( _flushes > 0 )
			  {
					_pageCacheTracer.flushes( _flushes );
			  }
			  Reset();
		 }

		 public override long AccumulatedHits()
		 {
			  return _historicalHits + _hits;
		 }

		 public override long AccumulatedFaults()
		 {
			  return _historicalFaults + _faults;
		 }

		 private void Reset()
		 {
			  _pins = 0;
			  _unpins = 0;
			  _hits = 0;
			  _faults = 0;
			  _bytesRead = 0;
			  _bytesWritten = 0;
			  _evictions = 0;
			  _evictionExceptions = 0;
			  _flushes = 0;
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
			  return _unpins;
		 }

		 public override long Hits()
		 {
			  return _hits;
		 }

		 public override long BytesRead()
		 {
			  return _bytesRead;
		 }

		 public override long Evictions()
		 {
			  return _evictions;
		 }

		 public override long EvictionExceptions()
		 {
			  return _evictionExceptions;
		 }

		 public override long BytesWritten()
		 {
			  return _bytesWritten;
		 }

		 public override long Flushes()
		 {
			  return _flushes;
		 }

		 public override double HitRatio()
		 {
			  return MathUtil.portion( Hits(), Faults() );
		 }

		 public override PinEvent BeginPin( bool writeLock, long filePageId, PageSwapper swapper )
		 {
			  _pins++;
			  _pinTracingEvent.eventHits = 1;
			  return _pinTracingEvent;
		 }

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
				  return flushEventOpportunity;
			 }

			 public void threwException( IOException exception )
			 {
				  outerInstance.evictionExceptions++;
			 }

			 public long CachePageId
			 {
				 set
				 {
				 }
			 }

			 public void close()
			 {
				  outerInstance.evictions++;
			 }
		 }

		 private readonly PageFaultEvent pageFaultEvent = new PageFaultEventAnonymousInnerClass();

		 private class PageFaultEventAnonymousInnerClass : PageFaultEvent
		 {
			 public void addBytesRead( long bytes )
			 {
				  outerInstance.bytesRead += bytes;
			 }

			 public void done()
			 {
				  outerInstance.faults++;
			 }

			 public void done( Exception throwable )
			 {
				  done();
			 }

			 public EvictionEvent beginEviction()
			 {
				  return evictionEvent;
			 }

			 public long CachePageId
			 {
				 set
				 {
				 }
			 }
		 }

		 private readonly FlushEventOpportunity flushEventOpportunity = new FlushEventOpportunityAnonymousInnerClass();

		 private class FlushEventOpportunityAnonymousInnerClass : FlushEventOpportunity
		 {
			 public FlushEvent beginFlush( long filePageId, long cachePageId, PageSwapper swapper )
			 {
				  return flushEvent;
			 }
		 }

		 private readonly FlushEvent flushEvent = new FlushEventAnonymousInnerClass();

		 private class FlushEventAnonymousInnerClass : FlushEvent
		 {
			 public void addBytesWritten( long bytes )
			 {
				  outerInstance.bytesWritten += bytes;
			 }

			 public void done()
			 {
				  outerInstance.flushes++;
			 }

			 public void done( IOException exception )
			 {
				  done();
			 }

			 public void addPagesFlushed( int pageCount )
			 {
			 }
		 }

		 private class DefaultPinEvent : PinEvent
		 {
			 private readonly DefaultPageCursorTracer _outerInstance;

			 public DefaultPinEvent( DefaultPageCursorTracer outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  internal int EventHits = 1;

			  public virtual long CachePageId
			  {
				  set
				  {
				  }
			  }

			  public override PageFaultEvent BeginPageFault()
			  {
					EventHits = 0;
					return pageFaultEvent;
			  }

			  public override void Hit()
			  {
					outerInstance.hits += EventHits;
			  }

			  public override void Done()
			  {
					outerInstance.unpins++;
			  }
		 }
	}

}