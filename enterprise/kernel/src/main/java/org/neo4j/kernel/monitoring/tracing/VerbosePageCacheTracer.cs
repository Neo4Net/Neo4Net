/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.Kernel.monitoring.tracing
{

	using TimeUtil = Org.Neo4j.Helpers.TimeUtil;
	using ByteUnit = Org.Neo4j.Io.ByteUnit;
	using PageSwapper = Org.Neo4j.Io.pagecache.PageSwapper;
	using DefaultPageCacheTracer = Org.Neo4j.Io.pagecache.tracing.DefaultPageCacheTracer;
	using FlushEvent = Org.Neo4j.Io.pagecache.tracing.FlushEvent;
	using FlushEventOpportunity = Org.Neo4j.Io.pagecache.tracing.FlushEventOpportunity;
	using MajorFlushEvent = Org.Neo4j.Io.pagecache.tracing.MajorFlushEvent;
	using Log = Org.Neo4j.Logging.Log;
	using SystemNanoClock = Org.Neo4j.Time.SystemNanoClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.FeatureToggles.flag;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.util.FeatureToggles.getInteger;

	public class VerbosePageCacheTracer : DefaultPageCacheTracer
	{
		 private static readonly bool _useRawReportingUnits = flag( typeof( VerbosePageCacheTracer ), "reportInRawUnits", false );
		 private static readonly int _speedReportingTimeThreshold = getInteger( typeof( VerbosePageCacheTracer ), "speedReportingThresholdSeconds", 10 );

		 private readonly Log _log;
		 private readonly SystemNanoClock _clock;
		 private readonly AtomicLong _flushedPages = new AtomicLong();
		 private readonly AtomicLong _flushBytesWritten = new AtomicLong();

		 internal VerbosePageCacheTracer( Log log, SystemNanoClock clock )
		 {
			  this._log = log;
			  this._clock = clock;
		 }

		 public override void MappedFile( File file )
		 {
			  _log.info( format( "Map file: '%s'.", file.Name ) );
			  base.MappedFile( file );
		 }

		 public override void UnmappedFile( File file )
		 {
			  _log.info( format( "Unmap file: '%s'.", file.Name ) );
			  base.UnmappedFile( file );
		 }

		 public override MajorFlushEvent BeginCacheFlush()
		 {
			  _log.info( "Start whole page cache flush." );
			  return new PageCacheMajorFlushEvent( this, _flushedPages.get(), _flushBytesWritten.get(), _clock.nanos() );
		 }

		 public override MajorFlushEvent BeginFileFlush( PageSwapper swapper )
		 {
			  string fileName = swapper.File().Name;
			  _log.info( format( "Flushing file: '%s'.", fileName ) );
			  return new FileFlushEvent( this, fileName, _flushedPages.get(), _flushBytesWritten.get(), _clock.nanos() );
		 }

		 private static string NanosToString( long nanos )
		 {
			  if ( _useRawReportingUnits )
			  {
					return nanos + "ns";
			  }
			  return TimeUtil.nanosToString( nanos );
		 }

		 private static string FlushSpeed( long bytesWrittenInTotal, long flushTimeNanos )
		 {
			  if ( _useRawReportingUnits )
			  {
					return BytesInNanoSeconds( bytesWrittenInTotal, flushTimeNanos );
			  }
			  long seconds = TimeUnit.NANOSECONDS.toSeconds( flushTimeNanos );
			  if ( seconds > 0 )
			  {
					return BytesToString( bytesWrittenInTotal / seconds ) + "/s";
			  }
			  else
			  {
					return BytesInNanoSeconds( bytesWrittenInTotal, flushTimeNanos );
			  }
		 }

		 private static string BytesInNanoSeconds( long bytesWrittenInTotal, long flushTimeNanos )
		 {
			  long bytesInNanoSecond = flushTimeNanos > 0 ? ( bytesWrittenInTotal / flushTimeNanos ) : bytesWrittenInTotal;
			  return bytesInNanoSecond + "bytes/ns";
		 }

		 private static string BytesToString( long bytes )
		 {
			  if ( _useRawReportingUnits )
			  {
					return bytes + "bytes";
			  }
			  return ByteUnit.bytesToString( bytes );
		 }

		 private readonly FlushEvent flushEvent = new FlushEventAnonymousInnerClass();

		 private class FlushEventAnonymousInnerClass : FlushEvent
		 {
			 public void addBytesWritten( long bytes )
			 {
				  outerInstance.bytesWritten.add( bytes );
				  outerInstance.flushBytesWritten.getAndAdd( bytes );
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
				  outerInstance.flushedPages.getAndAdd( pageCount );
			 }
		 }

		 private class FileFlushEvent : MajorFlushEvent
		 {
			 private readonly VerbosePageCacheTracer _outerInstance;

			  internal readonly long StartTimeNanos;
			  internal readonly string FileName;
			  internal long FlushesOnStart;
			  internal long BytesWrittenOnStart;

			  internal FileFlushEvent( VerbosePageCacheTracer outerInstance, string fileName, long flushesOnStart, long bytesWrittenOnStart, long startTimeNanos )
			  {
				  this._outerInstance = outerInstance;
					this.FileName = fileName;
					this.FlushesOnStart = flushesOnStart;
					this.BytesWrittenOnStart = bytesWrittenOnStart;
					this.StartTimeNanos = startTimeNanos;
			  }

			  public override FlushEventOpportunity FlushEventOpportunity()
			  {
					return new VerboseFlushOpportunity( _outerInstance, FileName, StartTimeNanos, BytesWrittenOnStart );
			  }

			  public override void Close()
			  {
					long fileFlushNanos = outerInstance.clock.Nanos() - StartTimeNanos;
					long bytesWrittenInTotal = outerInstance.flushBytesWritten.get() - BytesWrittenOnStart;
					long flushedPagesInTotal = outerInstance.flushedPages.get() - FlushesOnStart;
					outerInstance.log.Info( "'%s' flush completed. Flushed %s in %d pages. Flush took: %s. Average speed: %s.", FileName, BytesToString( bytesWrittenInTotal ), flushedPagesInTotal, NanosToString( fileFlushNanos ), FlushSpeed( bytesWrittenInTotal, fileFlushNanos ) );
			  }
		 }

		 private class PageCacheMajorFlushEvent : MajorFlushEvent
		 {
			 private readonly VerbosePageCacheTracer _outerInstance;

			  internal readonly long FlushesOnStart;
			  internal readonly long BytesWrittenOnStart;
			  internal readonly long StartTimeNanos;

			  internal PageCacheMajorFlushEvent( VerbosePageCacheTracer outerInstance, long flushesOnStart, long bytesWrittenOnStart, long startTimeNanos )
			  {
				  this._outerInstance = outerInstance;
					this.FlushesOnStart = flushesOnStart;
					this.BytesWrittenOnStart = bytesWrittenOnStart;
					this.StartTimeNanos = startTimeNanos;
			  }

			  public override FlushEventOpportunity FlushEventOpportunity()
			  {
					return new VerboseFlushOpportunity( _outerInstance, "Page Cache", StartTimeNanos, BytesWrittenOnStart );
			  }

			  public override void Close()
			  {
					long pageCacheFlushNanos = outerInstance.clock.Nanos() - StartTimeNanos;
					long bytesWrittenInTotal = outerInstance.flushBytesWritten.get() - BytesWrittenOnStart;
					long flushedPagesInTotal = outerInstance.flushedPages.get() - FlushesOnStart;
					outerInstance.log.Info( "Page cache flush completed. Flushed %s in %d pages. Flush took: %s. Average speed: %s.", BytesToString( bytesWrittenInTotal ), flushedPagesInTotal, NanosToString( pageCacheFlushNanos ), FlushSpeed( bytesWrittenInTotal, pageCacheFlushNanos ) );
			  }
		 }

		 private class VerboseFlushOpportunity : FlushEventOpportunity
		 {
			 private readonly VerbosePageCacheTracer _outerInstance;

			  internal readonly string FileName;
			  internal long LastReportingTime;
			  internal long LastReportedBytesWritten;

			  internal VerboseFlushOpportunity( VerbosePageCacheTracer outerInstance, string fileName, long nanoStartTime, long bytesWrittenOnStart )
			  {
				  this._outerInstance = outerInstance;
					this.FileName = fileName;
					this.LastReportingTime = nanoStartTime;
					this.LastReportedBytesWritten = bytesWrittenOnStart;
			  }

			  public override FlushEvent BeginFlush( long filePageId, long cachePageId, PageSwapper swapper )
			  {
					long now = outerInstance.clock.Nanos();
					long opportunityIntervalNanos = now - LastReportingTime;
					if ( TimeUnit.NANOSECONDS.toSeconds( opportunityIntervalNanos ) > _speedReportingTimeThreshold )
					{
						 long writtenBytes = outerInstance.flushBytesWritten.get();
						 outerInstance.log.Info( format( "'%s' flushing speed: %s.", FileName, FlushSpeed( writtenBytes - LastReportedBytesWritten, opportunityIntervalNanos ) ) );
						 LastReportingTime = now;
						 LastReportedBytesWritten = writtenBytes;
					}
					return flushEvent;
			  }
		 }
	}

}