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
	using Test = org.junit.Test;


	using ByteUnit = Org.Neo4j.Io.ByteUnit;
	using DummyPageSwapper = Org.Neo4j.Io.pagecache.tracing.DummyPageSwapper;
	using EvictionEvent = Org.Neo4j.Io.pagecache.tracing.EvictionEvent;
	using EvictionRunEvent = Org.Neo4j.Io.pagecache.tracing.EvictionRunEvent;
	using FlushEvent = Org.Neo4j.Io.pagecache.tracing.FlushEvent;
	using FlushEventOpportunity = Org.Neo4j.Io.pagecache.tracing.FlushEventOpportunity;
	using MajorFlushEvent = Org.Neo4j.Io.pagecache.tracing.MajorFlushEvent;
	using AssertableLogProvider = Org.Neo4j.Logging.AssertableLogProvider;
	using Log = Org.Neo4j.Logging.Log;
	using Clocks = Org.Neo4j.Time.Clocks;
	using FakeClock = Org.Neo4j.Time.FakeClock;

	public class VerbosePageCacheTracerTest
	{
		private bool InstanceFieldsInitialized = false;

		public VerbosePageCacheTracerTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_log = _logProvider.getLog( this.GetType() );
		}

		 private AssertableLogProvider _logProvider = new AssertableLogProvider( true );
		 private Log _log;
		 private FakeClock _clock = Clocks.fakeClock();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void traceFileMap()
		 public virtual void TraceFileMap()
		 {
			  VerbosePageCacheTracer tracer = CreateTracer();
			  tracer.MappedFile( new File( "mapFile" ) );
			  _logProvider.formattedMessageMatcher().assertContains("Map file: 'mapFile'.");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void traceUnmapFile()
		 public virtual void TraceUnmapFile()
		 {
			  VerbosePageCacheTracer tracer = CreateTracer();
			  tracer.UnmappedFile( new File( "unmapFile" ) );
			  _logProvider.formattedMessageMatcher().assertContains("Unmap file: 'unmapFile'.");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void traceSinglePageCacheFlush()
		 public virtual void TraceSinglePageCacheFlush()
		 {
			  VerbosePageCacheTracer tracer = CreateTracer();
			  using ( MajorFlushEvent majorFlushEvent = tracer.BeginCacheFlush() )
			  {
					FlushEventOpportunity flushEventOpportunity = majorFlushEvent.FlushEventOpportunity();
					FlushEvent flushEvent = flushEventOpportunity.BeginFlush( 1, 2, new DummyPageSwapper( "testFile", 1 ) );
					flushEvent.AddBytesWritten( 2 );
					flushEvent.AddPagesFlushed( 7 );
					flushEvent.Done();
			  }
			  _logProvider.formattedMessageMatcher().assertContains("Start whole page cache flush.");
			  _logProvider.formattedMessageMatcher().assertContains("Page cache flush completed. Flushed 2B in 7 pages. Flush took: 0ns. " + "Average speed: 2bytes/ns.");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void evictionDoesNotInfluenceFlushNumbers()
		 public virtual void EvictionDoesNotInfluenceFlushNumbers()
		 {
			  VerbosePageCacheTracer tracer = CreateTracer();
			  using ( MajorFlushEvent majorFlushEvent = tracer.BeginCacheFlush() )
			  {
					FlushEventOpportunity flushEventOpportunity = majorFlushEvent.FlushEventOpportunity();
					FlushEvent flushEvent = flushEventOpportunity.BeginFlush( 1, 2, new DummyPageSwapper( "testFile", 1 ) );
					_clock.forward( 2, TimeUnit.MILLISECONDS );

					using ( EvictionRunEvent evictionRunEvent = tracer.BeginPageEvictions( 5 ) )
					{
						 using ( EvictionEvent evictionEvent = evictionRunEvent.BeginEviction() )
						 {
							  FlushEventOpportunity evictionEventOpportunity = evictionEvent.FlushEventOpportunity();
							  FlushEvent evictionFlush = evictionEventOpportunity.BeginFlush( 2, 3, new DummyPageSwapper( "evictionFile", 1 ) );
							  evictionFlush.AddPagesFlushed( 10 );
							  evictionFlush.AddPagesFlushed( 100 );
						 }
					}
					flushEvent.AddBytesWritten( 2 );
					flushEvent.AddPagesFlushed( 7 );
					flushEvent.Done();
			  }
			  _logProvider.formattedMessageMatcher().assertContains("Start whole page cache flush.");
			  _logProvider.formattedMessageMatcher().assertContains("Page cache flush completed. Flushed 2B in 7 pages. Flush took: 2ms. " + "Average speed: 0bytes/ns.");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void traceFileFlush()
		 public virtual void TraceFileFlush()
		 {
			  VerbosePageCacheTracer tracer = CreateTracer();
			  DummyPageSwapper swapper = new DummyPageSwapper( "fileToFlush", 1 );
			  using ( MajorFlushEvent fileToFlush = tracer.BeginFileFlush( swapper ) )
			  {
					FlushEventOpportunity flushEventOpportunity = fileToFlush.FlushEventOpportunity();
					FlushEvent flushEvent = flushEventOpportunity.BeginFlush( 1, 2, swapper );
					flushEvent.AddPagesFlushed( 100 );
					flushEvent.AddBytesWritten( ByteUnit.ONE_MEBI_BYTE );
					flushEvent.Done();
					_clock.forward( 1, TimeUnit.SECONDS );
					FlushEvent flushEvent2 = flushEventOpportunity.BeginFlush( 1, 2, swapper );
					flushEvent2.AddPagesFlushed( 10 );
					flushEvent2.AddBytesWritten( ByteUnit.ONE_MEBI_BYTE );
					flushEvent2.Done();
			  }
			  _logProvider.formattedMessageMatcher().assertContains("Flushing file: 'fileToFlush'.");
			  _logProvider.formattedMessageMatcher().assertContains("'fileToFlush' flush completed. Flushed 2.000MiB in 110 pages. Flush took: 1s. Average speed: 2.000MiB/s.");
		 }

		 private VerbosePageCacheTracer CreateTracer()
		 {
			  return new VerbosePageCacheTracer( _log, _clock );
		 }
	}

}